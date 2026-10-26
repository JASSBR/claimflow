using ClaimFlow.Claims.Domain;
using ClaimFlow.SharedKernel;
using static ClaimFlow.Claims.Domain.UnitTests.ClaimBuilder;

namespace ClaimFlow.Claims.Domain.UnitTests;

public class ClaimWorkflowTests
{
    private static readonly DateTimeOffset Later = Now.AddHours(3);

    [Fact]
    public void HappyPath_ReviewApproveSettle_RecordsFullAuditTrail()
    {
        var claim = Declared(claimedAmount: 1_200m);
        claim.ClearDomainEvents();

        claim.StartReview(Handler, Later).IsSuccess.ShouldBeTrue();
        claim.Approve(1_000m, Unlimited, Later).IsSuccess.ShouldBeTrue();
        claim.Settle(OtherManager, Later).IsSuccess.ShouldBeTrue();

        claim.Status.ShouldBe(ClaimStatus.Settled);
        claim.ApprovedAmount.ShouldBe(1_000m);
        claim.LastUpdatedAt.ShouldBe(Later);
        claim.History.Select(change => change.To).ShouldBe([ClaimStatus.UnderReview, ClaimStatus.Approved, ClaimStatus.Settled]);
        claim.DomainEvents.Count.ShouldBe(3);
        claim.AllowedActions.ShouldBeEmpty();
    }

    [Fact]
    public void InformationLoop_KeepsReasonInHistory()
    {
        var claim = UnderReview();

        claim.RequestInformation(Handler, "  Send the police report.  ", Later).IsSuccess.ShouldBeTrue();
        claim.ResumeReview(Handler, Later).IsSuccess.ShouldBeTrue();

        claim.Status.ShouldBe(ClaimStatus.UnderReview);
        claim.History[1].Reason.ShouldBe("Send the police report.");
        claim.History[1].Action.ShouldBe(ClaimAction.RequestInformation);
    }

    [Fact]
    public void Reject_IsAllowed_WhileInformationIsRequested()
    {
        var claim = UnderReview();
        claim.RequestInformation(Handler, "Missing invoice.", Later);

        claim.Reject(Handler, "No answer after 30 days.", Later).IsSuccess.ShouldBeTrue();

        claim.Status.ShouldBe(ClaimStatus.Rejected);
        claim.AllowedActions.ShouldBe([ClaimAction.Reopen]);
    }

    [Fact]
    public void ForbiddenTransition_ReturnsConflict_AndLeavesClaimUntouched()
    {
        var claim = Declared();
        claim.ClearDomainEvents();

        var result = claim.Settle(OtherManager, Later);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
        result.Errors[0].Code.ShouldBe("claim.transition_not_allowed");
        claim.Status.ShouldBe(ClaimStatus.Declared);
        claim.History.ShouldBeEmpty();
        claim.DomainEvents.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DecisionsNeedingAReason_FailWithoutOne(string? reason)
    {
        var claim = UnderReview();

        claim.Reject(Handler, reason, Later).Errors.ShouldBe([ClaimErrors.ReasonRequired]);
        claim.RequestInformation(Handler, reason, Later).Errors.ShouldBe([ClaimErrors.ReasonRequired]);
        claim.Status.ShouldBe(ClaimStatus.UnderReview);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(0.004)]
    [InlineData(1_000.01)]
    public void Approve_RejectsAmount_NotBetweenZeroAndClaimed(decimal amount)
    {
        var claim = UnderReview(claimedAmount: 1_000m);

        claim.Approve(amount, Unlimited, Later).Errors.ShouldBe([ClaimErrors.ApprovedAmountOutOfRange]);
        claim.Status.ShouldBe(ClaimStatus.UnderReview);
        claim.ApprovedAmount.ShouldBeNull();
    }

    [Fact]
    public void Approve_FromWrongState_DoesNotSetAmount()
    {
        var claim = Declared();

        claim.Approve(500m, Unlimited, Later).IsFailure.ShouldBeTrue();
        claim.ApprovedAmount.ShouldBeNull();
    }

    [Fact]
    public void StatusChange_RaisesEventWithFromAndTo()
    {
        var claim = Approved();
        claim.ClearDomainEvents();

        claim.Settle(OtherManager, Later);

        var domainEvent = claim.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ClaimStatusChanged>();
        domainEvent.From.ShouldBe(ClaimStatus.Approved);
        domainEvent.To.ShouldBe(ClaimStatus.Settled);
        domainEvent.OccurredAt.ShouldBe(Later);
    }

    public static TheoryData<ClaimStatus, ClaimAction[]> AllowedActionsByStatus => new()
    {
        { ClaimStatus.Declared, [ClaimAction.StartReview] },
        { ClaimStatus.UnderReview, [ClaimAction.RequestInformation, ClaimAction.Approve, ClaimAction.Reject] },
        { ClaimStatus.InformationRequested, [ClaimAction.ResumeReview, ClaimAction.Reject] },
        { ClaimStatus.Approved, [ClaimAction.Settle] },
        { ClaimStatus.Rejected, [ClaimAction.Reopen] },
        { ClaimStatus.Settled, [] },
    };

    [Theory]
    [MemberData(nameof(AllowedActionsByStatus))]
    public void AllowedActions_MatchWorkflowTable(ClaimStatus status, ClaimAction[] expected)
    {
        var claim = status switch
        {
            ClaimStatus.Declared => Declared(),
            ClaimStatus.UnderReview => UnderReview(),
            ClaimStatus.Approved => Approved(),
            ClaimStatus.Rejected => Rejected(),
            ClaimStatus.Settled => Settled(),
            _ => WithInformationRequested(),
        };

        claim.Status.ShouldBe(status);
        claim.AllowedActions.ShouldBe(expected, ignoreOrder: true);
    }

    private static Claim WithInformationRequested()
    {
        var claim = UnderReview();
        claim.RequestInformation(Handler, "Photos please.", Later);
        return claim;
    }

    [Fact]
    public void Approve_AboveDelegatedAuthority_IsForbidden_AndLeavesClaimUnderReview()
    {
        var claim = UnderReview(claimedAmount: 25_000m);

        var result = claim.Approve(20_000m, new ApprovalAuthority(Handler, 10_000m), Later);

        result.Errors.ShouldHaveSingleItem().Code.ShouldBe("claim.approval_limit_exceeded");
        result.Errors[0].Type.ShouldBe(ErrorType.Forbidden);
        claim.Status.ShouldBe(ClaimStatus.UnderReview);
        claim.ApprovedById.ShouldBeNull();
    }

    [Fact]
    public void Approve_WithinDelegatedAuthority_RecordsApprover()
    {
        var claim = UnderReview(claimedAmount: 8_000m);

        claim.Approve(8_000m, new ApprovalAuthority(Handler, 10_000m), Later).IsSuccess.ShouldBeTrue();

        claim.ApprovedById.ShouldBe(Handler.Id);
        claim.History[^1].ActorName.ShouldBe(Handler.Name);
    }

    [Fact]
    public void Settle_ByTheApprover_ViolatesFourEyesPrinciple()
    {
        var claim = Approved();

        var result = claim.Settle(Manager, Later);

        result.Errors.ShouldBe([ClaimErrors.FourEyesViolation]);
        claim.Status.ShouldBe(ClaimStatus.Approved);
    }

    [Fact]
    public void AllowedActionsFor_HidesSettle_FromTheApprover_Only()
    {
        var claim = Approved();

        claim.AllowedActionsFor(Manager, Later).ShouldBeEmpty();
        claim.AllowedActionsFor(OtherManager, Later).ShouldBe([ClaimAction.Settle]);
    }

    [Fact]
    public void EveryTransition_IsAttributedToItsActor()
    {
        var claim = UnderReview();
        claim.ClearDomainEvents();

        claim.RequestInformation(Manager, "Photos please.", Later);

        claim.History[^1].ActorId.ShouldBe(Manager.Id);
        claim.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ClaimStatusChanged>().ActorName.ShouldBe(Manager.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(30)]
    public void Reopen_WithinThirtyDays_TransitionsToUnderReview_AndRecordsHistory(int daysAfterRejection)
    {
        var rejectedAt = Now.AddHours(1);
        var claim = Rejected(rejectedAt);
        claim.ClearDomainEvents();
        var reopenAt = rejectedAt.AddDays(daysAfterRejection);

        var result = claim.Reopen(Handler, "  Counter-expertise report received.  ", reopenAt);

        result.IsSuccess.ShouldBeTrue();
        claim.Status.ShouldBe(ClaimStatus.UnderReview);
        claim.LastUpdatedAt.ShouldBe(reopenAt);
        var change = claim.History[^1];
        change.From.ShouldBe(ClaimStatus.Rejected);
        change.To.ShouldBe(ClaimStatus.UnderReview);
        change.Action.ShouldBe(ClaimAction.Reopen);
        change.ActorId.ShouldBe(Handler.Id);
        change.ActorName.ShouldBe(Handler.Name);
        change.Reason.ShouldBe("Counter-expertise report received.");
        change.OccurredAt.ShouldBe(reopenAt);
        var domainEvent = claim.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ClaimStatusChanged>();
        domainEvent.From.ShouldBe(ClaimStatus.Rejected);
        domainEvent.To.ShouldBe(ClaimStatus.UnderReview);
        domainEvent.ActorName.ShouldBe(Handler.Name);
        domainEvent.OccurredAt.ShouldBe(reopenAt);
    }

    [Fact]
    public void Reopen_FromDayThirtyOne_FailsWithReopenWindowExpired()
    {
        var rejectedAt = Now.AddHours(1);
        var claim = Rejected(rejectedAt);
        claim.ClearDomainEvents();

        var day31 = claim.Reopen(Handler, "Late evidence", rejectedAt.AddDays(31));
        var justPastDay30 = claim.Reopen(Handler, "Late evidence", rejectedAt.AddDays(30).AddTicks(1));

        day31.Errors.ShouldBe([ClaimErrors.ReopenWindowExpired]);
        day31.Errors[0].Type.ShouldBe(ErrorType.Conflict);
        justPastDay30.Errors.ShouldBe([ClaimErrors.ReopenWindowExpired]);
        claim.Status.ShouldBe(ClaimStatus.Rejected);
        claim.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Reopen_OnSettledClaim_IsRefused()
    {
        var claim = Settled();
        claim.ClearDomainEvents();

        claim.AllowedActions.ShouldBeEmpty();
        claim.AllowedActionsFor(Handler, Later).ShouldBeEmpty();
        var result = claim.Reopen(Handler, "New invoice", Later);

        result.Errors.ShouldHaveSingleItem().Code.ShouldBe("claim.transition_not_allowed");
        result.Errors[0].Type.ShouldBe(ErrorType.Conflict);
        claim.Status.ShouldBe(ClaimStatus.Settled);
        claim.DomainEvents.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reopen_WithoutReason_FailsWithReasonRequired(string? reason)
    {
        var claim = Rejected(Now.AddHours(1));

        var result = claim.Reopen(Handler, reason, Now.AddDays(5));

        result.Errors.ShouldBe([ClaimErrors.ReasonRequired]);
        claim.Status.ShouldBe(ClaimStatus.Rejected);
    }

    [Fact]
    public void Reopen_SecondTimeAfterSubsequentRejection_FailsWithAlreadyReopened()
    {
        var rejectedAt = Now.AddHours(1);
        var claim = Rejected(rejectedAt);
        claim.Reopen(Handler, "First new evidence", rejectedAt.AddDays(5)).IsSuccess.ShouldBeTrue();
        var secondRejectAt = rejectedAt.AddDays(6);
        claim.Reject(Handler, "Still not covered", secondRejectAt).IsSuccess.ShouldBeTrue();

        var secondReopen = claim.Reopen(Handler, "Second attempt", secondRejectAt.AddDays(1));

        secondReopen.Errors.ShouldBe([ClaimErrors.AlreadyReopened]);
        secondReopen.Errors[0].Type.ShouldBe(ErrorType.Conflict);
        claim.Status.ShouldBe(ClaimStatus.Rejected);
    }

    [Fact]
    public void AllowedActionsFor_OffersReopenWithinThirtyDays_AndHidesItAfterDayThirtyOrAfterOneReopen()
    {
        var rejectedAt = Now.AddHours(1);
        var claim = Rejected(rejectedAt);

        claim.AllowedActionsFor(Handler, rejectedAt.AddDays(30)).ShouldBe([ClaimAction.Reopen]);
        claim.AllowedActionsFor(Handler, rejectedAt.AddDays(30).AddTicks(1)).ShouldBeEmpty();
        claim.AllowedActionsFor(Handler, rejectedAt.AddDays(31)).ShouldBeEmpty();

        claim.Reopen(Handler, "New evidence", rejectedAt.AddDays(10)).IsSuccess.ShouldBeTrue();
        claim.Reject(Handler, "Rejected again", rejectedAt.AddDays(12)).IsSuccess.ShouldBeTrue();

        claim.AllowedActionsFor(Handler, rejectedAt.AddDays(13)).ShouldBeEmpty();
    }
}
