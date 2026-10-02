using System.Security.Cryptography;
using ClaimFlow.Claims.Contracts;
using ClaimFlow.Documents.Domain;
using ClaimFlow.Documents.Persistence;
using ClaimFlow.Documents.Storage;
using ClaimFlow.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Documents.Seeding;

internal static class DemoDocumentsSeeder
{
    private static readonly Actor Lea = new("lea", "Léa Martin");

    public static async Task SeedAsync(
        DocumentsDbContext dbContext,
        IDocumentStore store,
        IClaimDirectory claims,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Documents.AnyAsync(cancellationToken))
        {
            return;
        }

        // The rear-end collision: its report describes a front impact on a roundabout, and the quote exceeds the claim.
        var collision = (await claims.FindByPolicyAsync("POL-104233", cancellationToken))
            .FirstOrDefault(claim => claim.ClaimedAmount == 2_350m);
        if (collision is not null)
        {
            await AttachAsync(collision, "Constat amiable.pdf", DemoPdfs.AccidentReport(collision.IncidentDate));
            await AttachAsync(collision, "Devis Carrosserie des Lilas.pdf", DemoPdfs.BodyworkQuote(collision.IncidentDate));
            await AttachAsync(collision, "Attestation d'assurance.pdf", DemoPdfs.InsuranceCertificate(collision.IncidentDate));
        }

        // The water leak: consistent evidence, so the review has a clean case to compare with.
        if (await claims.FindByPolicyAsync("POL-208871", cancellationToken) is [var leak, ..])
        {
            await AttachAsync(leak, "Rapport de recherche de fuite.pdf", DemoPdfs.LeakInvestigation(leak.IncidentDate));
            await AttachAsync(leak, "Devis embellissements.pdf", DemoPdfs.CeilingQuote(leak.IncidentDate));
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        async Task AttachAsync(ClaimSnapshot claim, string fileName, byte[] content)
        {
            var document = ClaimDocument.Upload(
                claim.Id,
                fileName,
                DocumentFormat.Detect(content),
                content.LongLength,
                Convert.ToHexStringLower(SHA256.HashData(content)),
                documentsAlreadyOnClaim: 0,
                Lea,
                claim.DeclaredAt.AddHours(2)).Value;
            await store.SaveAsync(document.StorageKey, content, document.ContentType, cancellationToken);
            dbContext.Documents.Add(document);
        }
    }
}
