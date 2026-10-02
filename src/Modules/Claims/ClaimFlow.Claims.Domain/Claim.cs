using System.Collections.Frozen;
using System.Text.RegularExpressions;
using ClaimFlow.SharedKernel;

namespace ClaimFlow.Claims.Domain;

public sealed record DeclareClaimData(
    string PolicyNumber,
    ClaimType Type,
    DateOnly IncidentDate,
    string Description,
    decimal ClaimedAmount);

/// <summary>The approval power delegated to a person ("délégation de pouvoir"): above <paramref name="Limit"/>, someone more senior must approve.</summary>
public sealed record ApprovalAuthority(Actor Approver, decimal Limit);

/// <summary>
/// An insurance claim ("sinistre") and its handling workflow.
/// Every state change goes through <see cref="Transition"/>, so the workflow table below is the single source of truth:
/// the API exposes <see cref="AllowedActions"/> from it, and the UI only renders the buttons the server allows.
/// </summary>
public sealed partial class Claim : AggregateRoot<ClaimId>
{
    public const int DescriptionMinLength = 10;
    public const int DescriptionMaxLength = 2000;
    public const decimal MaxClaimedAmount = 1_000_000m;

    // L114-1 Code des assurances: actions arising from an insurance contract are time-barred after two years.
    private const int LimitationPeriodYears = 2;

    private static readonly FrozenDictionary<ClaimAction, (ClaimStatus[] From, ClaimStatus To)> Workflow =
        new Dictionary<ClaimAction, (ClaimStatus[] From, ClaimStatus To)>
        {
            [ClaimAction.StartReview] = ([ClaimStatus.Declared], ClaimStatus.UnderReview),
            [ClaimAction.RequestInformation] = ([ClaimStatus.UnderReview], ClaimStatus.InformationRequested),
            [ClaimAction.ResumeReview] = ([ClaimStatus.InformationRequested], ClaimStatus.UnderReview),
            [ClaimAction.Approve] = ([ClaimStatus.UnderReview], ClaimStatus.Approved),
            [ClaimAction.Reject] = ([ClaimStatus.UnderReview, ClaimStatus.InformationRequested], ClaimStatus.Rejected),
            [ClaimAction.Settle] = ([ClaimStatus.Approved], ClaimStatus.Settled),
        }.ToFrozenDictionary();

    private readonly List<ClaimStatusChange> _history = [];

    private Claim()
    {
        // Materialization constructor for the persistence layer.
    }

    public string Number { get; private set; } = string.Empty;

    public string PolicyNumber { get; private set; } = string.Empty;

    public ClaimType Type { get; private set; }

    public DateOnly IncidentDate { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal ClaimedAmount { get; private set; }

    public decimal? ApprovedAmount { get; private set; }

    public ClaimStatus Status { get; private set; }

    public DateTimeOffset DeclaredAt { get; private set; }

    public string DeclaredById { get; private set; } = string.Empty;

    public string DeclaredByName { get; private set; } = string.Empty;

    /// <summary>Who approved the claim. Kept to enforce the four-eyes rule at settlement.</summary>
    public string? ApprovedById { get; private set; }

    public DateTimeOffset LastUpdatedAt { get; private set; }

    /// <summary>Optimistic concurrency token, mapped to PostgreSQL's xmin system column.</summary>
    // Get-only: only the database writes it; EF Core sets the compiler-generated backing field.
    public uint Version { get; }

    public IReadOnlyList<ClaimStatusChange> History => _history.AsReadOnly();

    public IReadOnlyList<ClaimAction> AllowedActions =>
        [.. Workflow.Where(step => step.Value.From.Contains(Status)).Select(step => step.Key).Order()];

    /// <summary>The workflow actions this person may take now, segregation of duties included (role permissions are applied by the caller).</summary>
    public IReadOnlyList<ClaimAction> AllowedActionsFor(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return [.. AllowedActions.Where(action => action != ClaimAction.Settle || !IsApprover(actor))];
    }

    /// <param name="number">Business reference allocated by the caller from a database sequence: guaranteed unique and gap-tolerant.</param>
    public static Result<Claim> Declare(DeclareClaimData data, string number, Actor declaredBy, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentNullException.ThrowIfNull(declaredBy);

        var errors = Validate(data, DateOnly.FromDateTime(now.UtcDateTime));
        if (errors.Count > 0)
        {
            return Result.Failure<Claim>([.. errors]);
        }

        var claim = new Claim
        {
            Id = ClaimId.New(),
            Number = number,
            PolicyNumber = data.PolicyNumber.Trim().ToUpperInvariant(),
            Type = data.Type,
            IncidentDate = data.IncidentDate,
            Description = data.Description.Trim(),
            ClaimedAmount = RoundAmount(data.ClaimedAmount),
            Status = ClaimStatus.Declared,
            DeclaredAt = now,
            DeclaredById = declaredBy.Id,
            DeclaredByName = declaredBy.Name,
            LastUpdatedAt = now,
        };
        claim.Raise(new ClaimDeclared(claim.Id, claim.Number, declaredBy.Name, now));
        return claim;
    }

    public Result StartReview(Actor actor, DateTimeOffset now) => Transition(ClaimAction.StartReview, actor, reason: null, now);

    public Result RequestInformation(Actor actor, string? reason, DateTimeOffset now) =>
        string.IsNullOrWhiteSpace(reason)
            ? ClaimErrors.ReasonRequired
            : Transition(ClaimAction.RequestInformation, actor, reason.Trim(), now);

    public Result ResumeReview(Actor actor, DateTimeOffset now) => Transition(ClaimAction.ResumeReview, actor, reason: null, now);

    public Result Approve(decimal approvedAmount, ApprovalAuthority authority, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(authority);

        // Validate the amount that will be stored, not the raw input: 0.001 must not pass "> 0" and then persist as 0.00.
        var amount = RoundAmount(approvedAmount);
        if (amount <= 0 || amount > ClaimedAmount)
        {
            return ClaimErrors.ApprovedAmountOutOfRange;
        }

        if (amount > authority.Limit)
        {
            return ClaimErrors.ApprovalLimitExceeded(authority.Limit);
        }

        var result = Transition(ClaimAction.Approve, authority.Approver, reason: null, now);
        if (result.IsSuccess)
        {
            ApprovedAmount = amount;
            ApprovedById = authority.Approver.Id;
        }

        return result;
    }

    public Result Reject(Actor actor, string? reason, DateTimeOffset now) =>
        string.IsNullOrWhiteSpace(reason)
            ? ClaimErrors.ReasonRequired
            : Transition(ClaimAction.Reject, actor, reason.Trim(), now);

    /// <summary>Releases the payment. Four-eyes principle: the person who approved the amount cannot also pay it out.</summary>
    public Result Settle(Actor actor, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(actor);
        return Status == ClaimStatus.Approved && IsApprover(actor)
            ? ClaimErrors.FourEyesViolation
            : Transition(ClaimAction.Settle, actor, reason: null, now);
    }

    private static List<Error> Validate(DeclareClaimData data, DateOnly today)
    {
        var errors = new List<Error>();

        if (data.PolicyNumber is null || !PolicyNumberPattern().IsMatch(data.PolicyNumber.Trim()))
        {
            errors.Add(ClaimErrors.PolicyNumberInvalid);
        }

        if (data.IncidentDate > today)
        {
            errors.Add(ClaimErrors.IncidentDateInFuture);
        }
        else if (data.IncidentDate < today.AddYears(-LimitationPeriodYears))
        {
            errors.Add(ClaimErrors.IncidentTimeBarred);
        }

        var descriptionLength = data.Description?.Trim().Length ?? 0;
        if (descriptionLength is < DescriptionMinLength or > DescriptionMaxLength)
        {
            errors.Add(ClaimErrors.DescriptionLength);
        }

        if (!Enum.IsDefined(data.Type))
        {
            errors.Add(ClaimErrors.TypeInvalid);
        }

        if (RoundAmount(data.ClaimedAmount) is <= 0 or > MaxClaimedAmount)
        {
            errors.Add(ClaimErrors.ClaimedAmountOutOfRange);
        }

        return errors;
    }

    private static decimal RoundAmount(decimal amount) => decimal.Round(amount, 2, MidpointRounding.ToEven);

    private bool IsApprover(Actor actor) => string.Equals(ApprovedById, actor.Id, StringComparison.Ordinal);

    private Result Transition(ClaimAction action, Actor actor, string? reason, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var (from, to) = Workflow[action];
        if (!from.Contains(Status))
        {
            return ClaimErrors.TransitionNotAllowed(action, Status);
        }

        var previous = Status;
        Status = to;
        LastUpdatedAt = now;
        _history.Add(new ClaimStatusChange(previous, to, action, actor, reason, now));
        Raise(new ClaimStatusChanged(Id, Number, previous, to, actor.Name, now));
        return Result.Success();
    }

    [GeneratedRegex(@"^POL-\d{6}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex PolicyNumberPattern();
}
