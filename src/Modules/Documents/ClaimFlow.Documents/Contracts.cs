namespace ClaimFlow.Documents;

public sealed record DocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string UploadedBy,
    DateTimeOffset UploadedAt);

public sealed record ClaimAnalysisResponse(
    Guid Id,
    DateTimeOffset CreatedAt,
    string RequestedBy,
    string Model,
    bool Refused,
    string Content,
    IReadOnlyList<AnalysisCitation> Citations,
    int DocumentCount,
    long InputTokens,
    long OutputTokens);

public sealed record DocumentSettingsResponse(
    bool AiEnabled,
    string? AiModel,
    long MaxSizeBytes,
    int MaxDocumentsPerClaim,
    IReadOnlyList<string> AcceptedContentTypes);

/// <summary>A passage of a document the AI relied on. Numbered so the review text can reference it as [[n]].</summary>
public sealed record AnalysisCitation(int Number, Guid DocumentId, string DocumentTitle, string CitedText, int? StartPage, int? EndPage);
