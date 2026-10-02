using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ClaimFlow.Documents.Storage;

/// <summary>Azure Blob Storage (Azurite locally, through Aspire). The container is private: files are only served by the API, after authorization.</summary>
internal sealed class BlobDocumentStore(BlobServiceClient blobServiceClient) : IDocumentStore
{
    public const string ContainerName = "claim-documents";

    private BlobContainerClient Container => blobServiceClient.GetBlobContainerClient(ContainerName);

    public Task EnsureCreatedAsync(CancellationToken cancellationToken) =>
        Container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

    public Task SaveAsync(string key, ReadOnlyMemory<byte> content, string contentType, CancellationToken cancellationToken) =>
        Container.GetBlobClient(key).UploadAsync(
            BinaryData.FromBytes(content),
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            cancellationToken);

    public async Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken) =>
        await Container.GetBlobClient(key).OpenReadAsync(cancellationToken: cancellationToken);

    public async Task<byte[]> ReadAllAsync(string key, CancellationToken cancellationToken) =>
        (await Container.GetBlobClient(key).DownloadContentAsync(cancellationToken)).Value.Content.ToArray();
}
