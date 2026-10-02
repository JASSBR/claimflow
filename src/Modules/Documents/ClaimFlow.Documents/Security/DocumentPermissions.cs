using ClaimFlow.Claims.Contracts;

namespace ClaimFlow.Documents.Security;

internal static class DocumentPermissions
{
    public const string ReadPolicy = "documents.read";
    public const string WritePolicy = "documents.write";

    /// <summary>Per-user budget on AI reviews: the public demo pays per token, so nobody can loop it.</summary>
    public const string AnalysisRateLimit = "documents.analysis";

    public static readonly string[] Readers = [ClaimsRoles.Handler, ClaimsRoles.Manager, ClaimsRoles.Auditor];
    public static readonly string[] Writers = [ClaimsRoles.Handler, ClaimsRoles.Manager];
}
