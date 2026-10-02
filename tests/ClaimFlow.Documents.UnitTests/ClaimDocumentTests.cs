using ClaimFlow.Documents.Domain;
using ClaimFlow.SharedKernel;

namespace ClaimFlow.Documents.UnitTests;

public sealed class ClaimDocumentTests
{
    private static readonly Actor Lea = new("lea", "Léa Martin");
    private static readonly DateTimeOffset Now = new(2026, 10, 2, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid ClaimId = Guid.CreateVersion7();

    private static Result<ClaimDocument> Upload(string name = "devis.pdf", DocumentFormat? format = null, long size = 2_048, int existing = 0) =>
        ClaimDocument.Upload(ClaimId, name, format ?? DocumentFormat.Pdf, size, new string('a', 64), existing, Lea, Now);

    [Fact]
    public void Upload_StoresDetectedType_AndAuditData()
    {
        var document = Upload().Value;

        document.ContentType.ShouldBe("application/pdf");
        document.UploadedByName.ShouldBe("Léa Martin");
        document.StorageKey.ShouldBe($"{ClaimId:N}/{document.Id.Value:N}");
    }

    [Fact]
    public void Upload_RejectsUnknownFormat_AndOversizedFile_Together()
    {
        var result = ClaimDocument.Upload(ClaimId, "x.pdf", null, ClaimDocument.MaxSizeBytes + 1, new string('a', 64), 0, Lea, Now);

        result.Errors.ShouldBe([DocumentErrors.TooLarge, DocumentErrors.UnsupportedFormat], ignoreOrder: true);
    }

    [Fact]
    public void Upload_RejectsEmptyFile()
    {
        Upload(size: 0).Errors.ShouldBe([DocumentErrors.Empty]);
    }

    [Fact]
    public void Upload_RefusesBeyondTheMaximumPerClaim()
    {
        Upload(existing: ClaimDocument.MaxDocumentsPerClaim).Errors.ShouldBe([DocumentErrors.TooManyDocuments]);
    }

    [Theory]
    [InlineData("../../etc/passwd.pdf", "passwd.pdf")]
    [InlineData(@"C:\Users\x\Desktop\Constat.pdf", "Constat.pdf")]
    [InlineData("rapport.pdf.exe", "rapport.pdf.pdf")]
    [InlineData("<script>alert(1)<script>.pdf", "scriptalert(1)script.pdf")]
    [InlineData("<script>alert(1)</script>.pdf", "script.pdf")]
    [InlineData("   ", "document.pdf")]
    [InlineData(null, "document.pdf")]
    public void SanitizeFileName_KeepsADisplayNameOnly_WithTheDetectedExtension(string? input, string expected)
    {
        ClaimDocument.SanitizeFileName(input, DocumentFormat.Pdf).ShouldBe(expected);
    }

    [Fact]
    public void SanitizeFileName_BoundsLength()
    {
        ClaimDocument.SanitizeFileName(new string('a', 500) + ".png", DocumentFormat.Png).Length.ShouldBe(ClaimDocument.FileNameMaxLength);
    }
}
