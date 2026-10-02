using Anthropic;
using Anthropic.Models.Beta;
using Anthropic.Models.Beta.Messages;
using ClaimFlow.Claims.Contracts;
using Microsoft.Extensions.Options;

namespace ClaimFlow.Documents.Analysis;

/// <summary>
/// Sends the claim and its documents to Claude in one request. PDFs go natively with citations enabled, so every
/// statement in the answer points to a page of a real document; images are sent for visual inspection.
/// </summary>
internal sealed class ClaudeClaimAnalyst(AnthropicClient client, IOptions<AiOptions> options) : IClaimAnalyst
{
    private const int MaxOutputTokens = 16_000;

    public async Task<AnalysisOutcome> AnalyzeAsync(
        ClaimSnapshot claim,
        IReadOnlyList<AnalysisDocument> documents,
        CancellationToken cancellationToken)
    {
        var content = new List<BetaContentBlockParam>();
        foreach (var document in documents)
        {
            var data = Convert.ToBase64String(document.Content);
            content.Add(string.Equals(document.ContentType, "application/pdf", StringComparison.Ordinal)
                ? new BetaRequestDocumentBlock
                {
                    Source = new BetaBase64PdfSource { Data = data },
                    Title = document.Title,
                    Citations = new BetaCitationsConfigParam { Enabled = true },
                }
                : new BetaImageBlockParam
                {
                    Source = new BetaBase64ImageSource { Data = data, MediaType = document.ContentType },
                });
        }

        content.Add(new BetaTextBlockParam { Text = AnalysisPrompt.ForClaim(claim, documents) });

        var response = await client.Beta.Messages.Create(
            new MessageCreateParams
            {
                Model = options.Value.Model,
                MaxTokens = MaxOutputTokens,
                System = AnalysisPrompt.System,
                OutputConfig = new BetaOutputConfig { Effort = options.Value.Effort },
                // If a safety classifier declines, the server re-serves the request with a suitable model in the same call.
                Betas = [AnthropicBeta.ServerSideFallback2026_07_01],
                Fallbacks = new Default(),
                Messages = [new BetaMessageParam { Role = Role.User, Content = content }],
            },
            cancellationToken);

        var usage = response.Usage;
        if (response.StopReason is { } stopReason && string.Equals(stopReason.Raw(), "refusal", StringComparison.Ordinal))
        {
            return new AnalysisOutcome(true, string.Empty, [], response.Model, usage.InputTokens, usage.OutputTokens);
        }

        var blocks = response.Content
            .Select(block => block.TryPickText(out var text) ? text : null)
            .OfType<BetaTextBlock>()
            .Select(text => new CitedBlock(text.Text, [.. (text.Citations ?? []).Select(ToPassage).OfType<CitedPassage>()]));
        var (composed, citations) = AnalysisComposer.Compose(blocks, documents);
        return new AnalysisOutcome(false, composed, citations, response.Model, usage.InputTokens, usage.OutputTokens);
    }

    private static CitedPassage? ToPassage(BetaTextCitation citation) =>
        citation.TryPickCitationPageLocation(out var page)
            ? new CitedPassage((int)page.DocumentIndex, page.CitedText, (int)page.StartPageNumber, (int)page.EndPageNumber)
            : null;
}
