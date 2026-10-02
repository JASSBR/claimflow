using System.Security.Claims;
using ClaimFlow.BuildingBlocks.Security;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Claims.Domain;
using Microsoft.Extensions.Options;
using Claim = ClaimFlow.Claims.Domain.Claim;

namespace ClaimFlow.Claims.Security;

/// <summary>
/// Role → permission matrix of the module. Who may *attempt* an action lives here; whether the action is legal for this
/// claim (workflow state, approval limit, four-eyes) lives in the domain. The two are combined into allowedActions.
/// </summary>
internal sealed class ClaimPermissions(IOptions<ClaimsOptions> options)
{
    public const string ReadPolicy = "claims.read";
    public const string WritePolicy = "claims.write";

    private static readonly string[] Writers = [ClaimsRoles.Handler, ClaimsRoles.Manager];
    private static readonly string[] Readers = [ClaimsRoles.Handler, ClaimsRoles.Manager, ClaimsRoles.Auditor];

    public static IReadOnlyList<string> ReaderRoles => Readers;

    public static IReadOnlyList<string> WriterRoles => Writers;

    public static bool CanPerform(ClaimsPrincipal user, ClaimAction action) => action == ClaimAction.Settle
        ? user.IsInRole(ClaimsRoles.Manager)
        : Writers.Any(user.IsInRole);

    public static bool CanDeclare(ClaimsPrincipal user) => Writers.Any(user.IsInRole);

    public decimal ApprovalLimitOf(ClaimsPrincipal user) => user.IsInRole(ClaimsRoles.Manager)
        ? Claim.MaxClaimedAmount
        : options.Value.HandlerApprovalLimit;

    public ApprovalAuthority AuthorityOf(ClaimsPrincipal user) => new(user.ToActor(), ApprovalLimitOf(user));

    /// <summary>What this user can do on this claim right now: workflow ∩ role ∩ segregation of duties.</summary>
    public static IReadOnlyList<ClaimAction> AllowedActions(Claim claim, ClaimsPrincipal user) =>
        [.. claim.AllowedActionsFor(user.ToActor()).Where(action => CanPerform(user, action))];
}
