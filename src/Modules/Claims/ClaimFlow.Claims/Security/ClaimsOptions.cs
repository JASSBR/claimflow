namespace ClaimFlow.Claims.Security;

internal sealed class ClaimsOptions
{
    public const string SectionName = "Claims";

    /// <summary>Delegated approval limit of a claim handler, in EUR. Managers have no limit.</summary>
    public decimal HandlerApprovalLimit { get; init; } = 10_000m;
}
