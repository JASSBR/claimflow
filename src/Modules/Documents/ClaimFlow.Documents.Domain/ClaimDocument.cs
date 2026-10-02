using System.Text;
using ClaimFlow.SharedKernel;

namespace ClaimFlow.Documents.Domain;

/// <summary>
/// A piece of evidence attached to a claim (police report, quote, invoice, photo).
/// Immutable once uploaded: a claim file is an audit record, so a wrong document is superseded, never edited.
/// </summary>
public sealed class ClaimDocument : AggregateRoot<DocumentId>
{
    public const long MaxSizeBytes = 10 * 1024 * 1024;
    public const int MaxDocumentsPerClaim = 12;
    public const int FileNameMaxLength = 120;

    private ClaimDocument()
    {
        // Materialization constructor for the persistence layer.
    }

    public Guid ClaimId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    /// <summary>SHA-256 of the content: proves the stored file is the one that was uploaded.</summary>
    public string Sha256 { get; private set; } = string.Empty;

    public string UploadedById { get; private set; } = string.Empty;

    public string UploadedByName { get; private set; } = string.Empty;

    public DateTimeOffset UploadedAt { get; private set; }

    /// <summary>Storage key: derived from ids only, so a user-supplied file name can never steer where bytes are written.</summary>
    public string StorageKey => $"{ClaimId:N}/{Id.Value:N}";

    public static Result<ClaimDocument> Upload(
        Guid claimId,
        string fileName,
        DocumentFormat? detectedFormat,
        long sizeBytes,
        string sha256,
        int documentsAlreadyOnClaim,
        Actor uploadedBy,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(uploadedBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);

        if (documentsAlreadyOnClaim >= MaxDocumentsPerClaim)
        {
            return DocumentErrors.TooManyDocuments;
        }

        var errors = new List<Error>();
        if (sizeBytes <= 0)
        {
            errors.Add(DocumentErrors.Empty);
        }
        else if (sizeBytes > MaxSizeBytes)
        {
            errors.Add(DocumentErrors.TooLarge);
        }

        if (detectedFormat is null)
        {
            errors.Add(DocumentErrors.UnsupportedFormat);
        }

        if (errors.Count > 0)
        {
            return Result.Failure<ClaimDocument>([.. errors]);
        }

        return new ClaimDocument
        {
            Id = DocumentId.New(),
            ClaimId = claimId,
            FileName = SanitizeFileName(fileName, detectedFormat!),
            ContentType = detectedFormat!.ContentType,
            SizeBytes = sizeBytes,
            Sha256 = sha256,
            UploadedById = uploadedBy.Id,
            UploadedByName = uploadedBy.Name,
            UploadedAt = now,
        };
    }

    /// <summary>
    /// Keeps a display name only: no directories, no control characters, bounded length, and the extension of the
    /// format actually detected — so "report.pdf.exe" holding a PDF is shown as "report.pdf.pdf", never as an executable.
    /// </summary>
    public static string SanitizeFileName(string? fileName, DocumentFormat format)
    {
        ArgumentNullException.ThrowIfNull(format);

        var name = Path.GetFileNameWithoutExtension((fileName ?? string.Empty).Replace('\\', '/').Split('/')[^1]);
        var cleaned = new StringBuilder(name.Length);
        foreach (var character in name.Where(c => !char.IsControl(c) && c is not ('"' or '<' or '>' or '|' or ':' or '*' or '?')))
        {
            cleaned.Append(character);
        }

        var stem = cleaned.ToString().Trim().Trim('.');
        if (stem.Length == 0)
        {
            stem = "document";
        }

        var maxStem = FileNameMaxLength - format.Extension.Length;
        return (stem.Length > maxStem ? stem[..maxStem] : stem) + format.Extension;
    }
}
