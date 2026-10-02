namespace ClaimFlow.Documents.Analysis;

/// <summary>A document handed to the analyst, in the order the model sees it (citations refer to that index).</summary>
internal sealed record AnalysisDocument(Guid DocumentId, string Title, string ContentType, byte[] Content);

/// <summary>Raw material produced by the model: text blocks, each with the document passages it cites.</summary>
internal sealed record CitedBlock(string Text, IReadOnlyList<CitedPassage> Passages);

internal sealed record CitedPassage(int DocumentIndex, string CitedText, int? StartPage, int? EndPage);

internal sealed record AnalysisOutcome(
    bool Refused,
    string Content,
    IReadOnlyList<AnalysisCitation> Citations,
    string Model,
    long InputTokens,
    long OutputTokens);
