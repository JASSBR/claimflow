using ClaimFlow.Claims.Domain;
using static ClaimFlow.Claims.Domain.UnitTests.ClaimBuilder;

namespace ClaimFlow.Claims.Domain.UnitTests;

public class ClaimDeclarationTests
{
    [Fact]
    public void Declare_CreatesDeclaredClaim_WhenDataIsValid()
    {
        var result = Claim.Declare(ValidData(), Number, Handler, Now);

        result.IsSuccess.ShouldBeTrue();
        var claim = result.Value;
        claim.Status.ShouldBe(ClaimStatus.Declared);
        claim.Number.ShouldBe(Number);
        claim.DeclaredAt.ShouldBe(Now);
        claim.DeclaredByName.ShouldBe(Handler.Name);
        claim.History.ShouldBeEmpty();
        claim.AllowedActions.ShouldBe([ClaimAction.StartReview]);
    }

    [Fact]
    public void Declare_RaisesClaimDeclared()
    {
        var claim = Declared();

        var domainEvent = claim.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<ClaimDeclared>();
        domainEvent.ClaimId.ShouldBe(claim.Id);
        domainEvent.Number.ShouldBe(claim.Number);
    }

    [Fact]
    public void Declare_NormalizesInput()
    {
        var claim = Claim.Declare(ValidData() with { PolicyNumber = "  pol-123456 ", Description = "  Broken window.  ", ClaimedAmount = 10.005m }, Number, Handler, Now).Value;

        claim.PolicyNumber.ShouldBe("POL-123456");
        claim.Description.ShouldBe("Broken window.");
        claim.ClaimedAmount.ShouldBe(10.00m);
    }

    [Fact]
    public void Declare_ReturnsEveryViolatedRule_AtOnce()
    {
        var data = new DeclareClaimData("123", ClaimType.Home, new DateOnly(2026, 10, 3), "short", 0m);

        var result = Claim.Declare(data, Number, Handler, Now);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBe(
            [ClaimErrors.PolicyNumberInvalid, ClaimErrors.IncidentDateInFuture, ClaimErrors.DescriptionLength, ClaimErrors.ClaimedAmountOutOfRange],
            ignoreOrder: true);
    }

    [Fact]
    public void Declare_Fails_WhenIncidentIsTimeBarred()
    {
        var result = Claim.Declare(ValidData() with { IncidentDate = new DateOnly(2024, 10, 1) }, Number, Handler, Now);

        result.Errors.ShouldBe([ClaimErrors.IncidentTimeBarred]);
    }

    [Fact]
    public void Declare_Accepts_IncidentExactlyTwoYearsAgo()
    {
        Claim.Declare(ValidData() with { IncidentDate = new DateOnly(2024, 10, 2) }, Number, Handler, Now).IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1_000_000.01)]
    public void Declare_RejectsAmount_OutsideAllowedRange(decimal amount)
    {
        Claim.Declare(ValidData(amount), Number, Handler, Now).Errors.ShouldBe([ClaimErrors.ClaimedAmountOutOfRange]);
    }

    [Fact]
    public void Declare_RejectsUndefinedClaimType()
    {
        Claim.Declare(ValidData() with { Type = (ClaimType)99 }, Number, Handler, Now).Errors.ShouldBe([ClaimErrors.TypeInvalid]);
    }

    [Fact]
    public void Declare_RejectsAmount_ThatRoundsToZero()
    {
        Claim.Declare(ValidData(0.004m), Number, Handler, Now).Errors.ShouldBe([ClaimErrors.ClaimedAmountOutOfRange]);
    }

    [Fact]
    public void Declare_RejectsNullTextFields_WithoutThrowing()
    {
        var result = Claim.Declare(ValidData() with { PolicyNumber = null!, Description = null! }, Number, Handler, Now);

        result.Errors.ShouldBe([ClaimErrors.PolicyNumberInvalid, ClaimErrors.DescriptionLength], ignoreOrder: true);
    }
}
