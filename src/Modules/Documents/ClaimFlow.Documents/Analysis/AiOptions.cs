namespace ClaimFlow.Documents.Analysis;

internal sealed class AiOptions
{
    public const string SectionName = "Ai";

    /// <summary>Anthropic API key. Read from a secret store (user-secrets, Azure Container Apps secret); never committed.</summary>
    public string? ApiKey { get; set; }

    public string Model { get; init; } = "claude-opus-5-5";

    /// <summary>Thinking depth / token spend. Medium is plenty to cross-check a handful of documents.</summary>
    public string Effort { get; init; } = "medium";

    /// <summary>Cap on what one analysis sends, well under the API's 32 MB request limit.</summary>
    public long MaxTotalBytes { get; init; } = 24 * 1024 * 1024;

    public bool Enabled => !string.IsNullOrWhiteSpace(ApiKey);
}
