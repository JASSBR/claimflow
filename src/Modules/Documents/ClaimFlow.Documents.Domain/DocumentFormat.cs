namespace ClaimFlow.Documents.Domain;

/// <summary>
/// The formats a claim file accepts, identified by their leading bytes ("magic numbers").
/// The browser-supplied Content-Type and file extension are never trusted: an .exe renamed invoice.pdf is rejected.
/// </summary>
public sealed record DocumentFormat(string ContentType, string Extension, bool SupportsCitations)
{
    public static readonly DocumentFormat Pdf = new("application/pdf", ".pdf", SupportsCitations: true);
    public static readonly DocumentFormat Png = new("image/png", ".png", SupportsCitations: false);
    public static readonly DocumentFormat Jpeg = new("image/jpeg", ".jpg", SupportsCitations: false);

    /// <summary>Bytes needed to recognise every supported format.</summary>
    public const int SignatureLength = 8;

    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    public static DocumentFormat? Detect(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith(PdfSignature))
        {
            return Pdf;
        }

        if (header.StartsWith(PngSignature))
        {
            return Png;
        }

        return header.StartsWith(JpegSignature) ? Jpeg : null;
    }

    public static DocumentFormat? FromContentType(string contentType) => contentType switch
    {
        "application/pdf" => Pdf,
        "image/png" => Png,
        "image/jpeg" => Jpeg,
        _ => null,
    };
}
