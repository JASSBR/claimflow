namespace ClaimFlow.Documents.Storage;

/// <summary>Binary storage of claim documents. Metadata lives in PostgreSQL; bytes live in object storage.</summary>
internal interface IDocumentStore
{
    Task EnsureCreatedAsync(CancellationToken cancellationToken);

    Task SaveAsync(string key, ReadOnlyMemory<byte> content, string contentType, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken);

    Task<byte[]> ReadAllAsync(string key, CancellationToken cancellationToken);
}
