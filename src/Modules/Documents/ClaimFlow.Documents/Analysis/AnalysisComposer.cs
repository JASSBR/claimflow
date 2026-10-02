using System.Text;

namespace ClaimFlow.Documents.Analysis;

/// <summary>
/// Turns the model's cited text blocks into one readable text where every cited statement ends with [n] markers,
/// plus the numbered list of passages. Pure function: the part of the AI feature that can be fully unit-tested.
/// </summary>
internal static class AnalysisComposer
{
    public const string CitationMarkerFormat = "[[{0}]]";

    public static (string Content, IReadOnlyList<AnalysisCitation> Citations) Compose(
        IEnumerable<CitedBlock> blocks,
        IReadOnlyList<AnalysisDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        ArgumentNullException.ThrowIfNull(documents);

        var content = new StringBuilder();
        var citations = new List<AnalysisCitation>();
        foreach (var block in blocks)
        {
            content.Append(block.Text);
            foreach (var passage in block.Passages.Where(p => p.DocumentIndex >= 0 && p.DocumentIndex < documents.Count))
            {
                var number = NumberOf(passage, citations, documents);
                content.Append(System.Globalization.CultureInfo.InvariantCulture, $"[[{number}]]");
            }
        }

        return (content.ToString().Trim(), citations);
    }

    // The same passage cited twice keeps one number, like a footnote in a real report.
    private static int NumberOf(CitedPassage passage, List<AnalysisCitation> citations, IReadOnlyList<AnalysisDocument> documents)
    {
        var document = documents[passage.DocumentIndex];
        var existing = citations.Find(c =>
            c.DocumentId == document.DocumentId
            && c.StartPage == passage.StartPage
            && string.Equals(c.CitedText, passage.CitedText.Trim(), StringComparison.Ordinal));
        if (existing is not null)
        {
            return existing.Number;
        }

        var citation = new AnalysisCitation(
            citations.Count + 1, document.DocumentId, document.Title, passage.CitedText.Trim(), passage.StartPage, passage.EndPage);
        citations.Add(citation);
        return citation.Number;
    }
}
