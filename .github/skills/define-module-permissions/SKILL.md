---
name: define-module-permissions
description: "Create strongly typed module configuration options with a standard section name and register them in the module's dependency injection container."
---

> Evidence: value gate verified the facts below against this repository.

Create strongly typed module configuration options with a standard section name and register them in the module's dependency injection container.

Applies in: `claim-flow-claims`.

## What is specific to this repository

- ClaimPermissions defines ReadPolicy as "claims.read" and WritePolicy as "claims.write". [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L16-L17]
- ClaimPermissions defines Writers and Readers role arrays using ClaimsRoles constants. [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L19-L20]

## Rules this change touches

- [BusinessRule] Settling a claim (ClaimAction.Settle) requires the user to hold the Manager role, whereas any other claim action requires the user to hold at least one Writer role (Handler or Manager). (approved, conviction 1.00, src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L26-L28)

## Start here

`src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs` [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L13-L41] is the worked example for this change, as it stands in the repository:

```csharp
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
```

## Workflow

1. Create a permissions class in the module's Security namespace. [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L1-L1]
2. Define ReadPolicy and WritePolicy string constants. [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L16-L16]
3. Define Readers and Writers string arrays containing allowed roles from ClaimsRoles. [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L19-L19]

## Checklist

- [ ] Policy name constants ReadPolicy and WritePolicy are defined. [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L16-L16]
- [ ] Roles are declared in Readers and Writers arrays using standard ClaimsRoles constants. [src/Modules/Claims/ClaimFlow.Claims/Security/ClaimPermissions.cs:L19-L19]
