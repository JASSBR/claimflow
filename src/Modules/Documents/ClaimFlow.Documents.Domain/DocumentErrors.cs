using ClaimFlow.SharedKernel;

namespace ClaimFlow.Documents.Domain;

public static class DocumentErrors
{
    public static readonly Error UnsupportedFormat =
        Error.Validation("document.unsupported_format", "Only PDF, PNG and JPEG files are accepted (checked from the file content, not its name).");

    public static readonly Error TooLarge =
        Error.Validation("document.too_large", $"A document cannot exceed {ClaimDocument.MaxSizeBytes / (1024 * 1024)} MB.");

    public static readonly Error Empty =
        Error.Validation("document.empty", "The file is empty.");

    public static readonly Error TooManyDocuments =
        Error.Conflict("document.too_many", $"A claim file holds at most {ClaimDocument.MaxDocumentsPerClaim} documents.");

    public static readonly Error ClaimNotFound =
        Error.NotFound("document.claim_not_found", "No claim exists with this id.");

    public static readonly Error NotFound =
        Error.NotFound("document.not_found", "No document exists with this id on this claim.");
}
