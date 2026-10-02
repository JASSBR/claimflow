namespace ClaimFlow.Claims.Contracts;

/// <summary>Roles this module understands. Any identity provider issuing these in the "role" claim can drive it.</summary>
public static class ClaimsRoles
{
    /// <summary>Gestionnaire sinistres: handles files, approves within a delegated limit.</summary>
    public const string Handler = "claims.handler";

    /// <summary>Responsable indemnisation: approves any amount, releases payments.</summary>
    public const string Manager = "claims.manager";

    /// <summary>Auditeur: reads everything, changes nothing.</summary>
    public const string Auditor = "claims.auditor";
}
