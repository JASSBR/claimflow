using ClaimFlow.Claims.Domain;
using ClaimFlow.SharedKernel;

namespace ClaimFlow.Claims.Domain.UnitTests;

internal static class ClaimBuilder
{
    public static readonly DateTimeOffset Now = new(2026, 10, 2, 9, 0, 0, TimeSpan.Zero);

    public const string Number = "SIN-2026-000042";

    public static readonly Actor Handler = new("lea", "Léa Martin");
    public static readonly Actor Manager = new("karim", "Karim Benali");
    public static readonly Actor OtherManager = new("nadia", "Nadia Haddad");
    public static readonly ApprovalAuthority Unlimited = new(Manager, decimal.MaxValue);

    public static DeclareClaimData ValidData(decimal claimedAmount = 1_000m) => new(
        PolicyNumber: "POL-123456",
        Type: ClaimType.Auto,
        IncidentDate: new DateOnly(2026, 9, 28),
        Description: "Rear-end collision at a red light.",
        ClaimedAmount: claimedAmount);

    public static Claim Declared(decimal claimedAmount = 1_000m) => Claim.Declare(ValidData(claimedAmount), Number, Handler, Now).Value;

    public static Claim UnderReview(decimal claimedAmount = 1_000m)
    {
        var claim = Declared(claimedAmount);
        claim.StartReview(Handler, Now).IsSuccess.ShouldBeTrue();
        return claim;
    }

    public static Claim Approved(decimal claimedAmount = 1_000m)
    {
        var claim = UnderReview(claimedAmount);
        claim.Approve(claimedAmount, Unlimited, Now).IsSuccess.ShouldBeTrue();
        return claim;
    }
}
