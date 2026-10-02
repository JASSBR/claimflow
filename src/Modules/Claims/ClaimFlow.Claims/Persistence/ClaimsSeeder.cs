using ClaimFlow.Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Claims.Persistence;

/// <summary>Demo data so a reviewer opening the live app immediately sees claims in every workflow state.</summary>
internal static class ClaimsSeeder
{
    private static readonly (string Policy, ClaimType Type, int DaysAgo, string Description, decimal Amount, ClaimAction[] Path)[] Samples =
    [
        ("POL-104233", ClaimType.Auto, 3, "Collision par l'arrière à un feu rouge, pare-chocs et hayon endommagés.", 2_350m, []),
        ("POL-208871", ClaimType.Home, 12, "Dégât des eaux venant du voisin du dessus, plafond de la cuisine taché.", 4_800m, [ClaimAction.StartReview]),
        ("POL-311902", ClaimType.Liability, 30, "Un client a glissé sur un sol mouillé dans la boutique, entorse du poignet.", 7_500m, [ClaimAction.StartReview, ClaimAction.RequestInformation]),
        ("POL-104233", ClaimType.Auto, 45, "Pare-brise fissuré par un gravillon sur l'autoroute.", 650m, [ClaimAction.StartReview, ClaimAction.Approve]),
        ("POL-412560", ClaimType.Home, 60, "Cambriolage : ordinateur, télévision et bijoux volés, porte arrière forcée.", 9_200m, [ClaimAction.StartReview, ClaimAction.Approve, ClaimAction.Settle]),
        ("POL-509114", ClaimType.Auto, 90, "Dégâts de grêle déclarés, mais les photos sont antérieures au début du contrat.", 3_100m, [ClaimAction.StartReview, ClaimAction.Reject]),
        ("POL-611478", ClaimType.Home, 5, "Tempête : tuiles arrachées, bâche posée en urgence.", 12_400m, []),
        ("POL-702338", ClaimType.Liability, 20, "Un enfant a cassé la fenêtre du voisin avec un ballon.", 380m, [ClaimAction.StartReview, ClaimAction.Approve]),
    ];

    public static async Task SeedAsync(ClaimsDbContext dbContext, TimeProvider timeProvider, CancellationToken cancellationToken)
    {
        if (await dbContext.Claims.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        foreach (var sample in Samples)
        {
            var declaredAt = now.AddDays(-sample.DaysAgo + 1);
            var claim = Claim.Declare(
                new DeclareClaimData(sample.Policy, sample.Type, DateOnly.FromDateTime(now.UtcDateTime.AddDays(-sample.DaysAgo)), sample.Description, sample.Amount),
                await dbContext.NextClaimNumberAsync(declaredAt, cancellationToken),
                declaredAt).Value;

            // One hour between steps, so the history reads in the order it happened.
            var at = declaredAt;
            foreach (var action in sample.Path)
            {
                at = at.AddHours(1);
                _ = action switch
                {
                    ClaimAction.StartReview => claim.StartReview(at),
                    ClaimAction.RequestInformation => claim.RequestInformation("Merci de transmettre le procès-verbal et les photos.", at),
                    ClaimAction.Approve => claim.Approve(sample.Amount * 0.9m, at),
                    ClaimAction.Reject => claim.Reject("Dommages antérieurs à la date d'effet du contrat.", at),
                    ClaimAction.Settle => claim.Settle(at),
                    _ => throw new InvalidOperationException($"Seed path does not support {action}."),
                };
            }

            // Seed data is not news: no outbox rows, no realtime notification storm on first start.
            claim.ClearDomainEvents();
            dbContext.Claims.Add(claim);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
