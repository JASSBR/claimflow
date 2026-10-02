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

    public DateTimeOffset LastUpdatedAt { get; private set; }

    /// <summary>Optimistic concurrency token, mapped to PostgreSQL's xmin system column.</summary>
    // Get-only: only the database writes it; EF Core sets the compiler-generated backing field.
    public uint Version { get; }

    public IReadOnlyList<ClaimStatusChange> History => _history.AsReadOnly();

    public IReadOnlyList<ClaimAction> AllowedActions =>
        [.. Workflow.Where(step => step.Value.From.Contains(Status)).Select(step => step.Key).Order()];

    /// <param name="number">Business reference allocated by the caller from a database sequence: guaranteed unique and gap-tolerant.</param>
    public static Result<Claim> Declare(DeclareClaimData data, string number, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(number);

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
            LastUpdatedAt = now,
        };
        claim.Raise(new ClaimDeclared(claim.Id, claim.Number, now));
        return claim;
    }

    public Result StartReview(DateTimeOffset now) => Transition(ClaimAction.StartReview, reason: null, now);

    public Result RequestInformation(string? reason, DateTimeOffset now) =>
        string.IsNullOrWhiteSpace(reason)
            ? ClaimErrors.ReasonRequired
            : Transition(ClaimAction.RequestInformation, reason.Trim(), now);

    public Result ResumeReview(DateTimeOffset now) => Transition(ClaimAction.ResumeReview, reason: null, now);

    public Result Approve(decimal approvedAmount, DateTimeOffset now)
    {
        // Validate the amount that will be stored, not the raw input: 0.001 must not pass "> 0" and then persist as 0.00.
        var amount = RoundAmount(approvedAmount);
        if (amount <= 0 || amount > ClaimedAmount)
        {
            return ClaimErrors.ApprovedAmountOutOfRange;
        }

        var result = Transition(ClaimAction.Approve, reason: null, now);
        if (result.IsSuccess)
        {
            ApprovedAmount = amount;
        }

        return result;
    }

    public Result Reject(string? reason, DateTimeOffset now) =>
        string.IsNullOrWhiteSpace(reason)
            ? ClaimErrors.ReasonRequired
            : Transition(ClaimAction.Reject, reason.Trim(), now);

    public Result Settle(DateTimeOffset now) => Transition(ClaimAction.Settle, reason: null, now);

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

    private Result Transition(ClaimAction action, string? reason, DateTimeOffset now)
    {
        var (from, to) = Workflow[action];
        if (!from.Contains(Status))
        {
            return ClaimErrors.TransitionNotAllowed(action, Status);
        }

        var previous = Status;
        Status = to;
        LastUpdatedAt = now;
        _history.Add(new ClaimStatusChange(previous, to, action, reason, now));
        Raise(new ClaimStatusChanged(Id, Number, previous, to, now));
        return Result.Success();
    }

    [GeneratedRegex(@"^POL-\d{6}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex PolicyNumberPattern();
}
