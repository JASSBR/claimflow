using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClaimFlow.Documents.Seeding;

/// <summary>
/// Realistic-looking evidence for the demo, generated at seed time so dates always match the seeded claims.
/// Every organisation is fictitious. Some documents contradict the declaration on purpose: the AI review must spot it.
/// </summary>
internal static class DemoPdfs
{
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

    static DemoPdfs() => QuestPDF.Settings.License = LicenseType.Community;

    public static byte[] AccidentReport(DateOnly incidentDate) => Render(
        "Constat amiable d'accident automobile",
        "Exemplaire assuré — véhicule A",
        page =>
        {
            Field(page, "Date de l'accident", $"{incidentDate.ToString("dd/MM/yyyy", French)} à 18 h 40");
            Field(page, "Lieu", "Rond-point de la Liberté, 69003 Lyon");
            Field(page, "Blessés", "Non");
            Field(page, "Véhicule A", "Peugeot 308 — immatriculation GH-482-PL — assuré Thomas Moreau, contrat POL-104233");
            Field(page, "Véhicule B", "Renault Clio — immatriculation FT-913-KD — assurée Claire Dubois");
            Field(page, "Point de choc initial (A)", "Avant droit");
            Field(page, "Circonstances (A)", "S'engageait sur le rond-point ; n'a pas cédé le passage au véhicule B déjà engagé.");
            Field(page, "Circonstances (B)", "Circulait sur le rond-point.");
            Field(page, "Dégâts apparents (A)", "Pare-chocs avant, phare avant droit, aile avant droite.");
            Field(page, "Observations", "Croquis : le véhicule A heurte le flanc gauche du véhicule B en entrant sur le rond-point.");
        });

    public static byte[] BodyworkQuote(DateOnly incidentDate) => Render(
        "Devis de réparation n° D-2026-0417",
        "Carrosserie des Lilas — 12 rue des Lilas, 69003 Lyon — SIRET 000 000 000 00000 (fictif)",
        page =>
        {
            Field(page, "Client", "Thomas Moreau — Peugeot 308 GH-482-PL");
            Field(page, "Date du devis", incidentDate.AddDays(1).ToString("dd/MM/yyyy", French));
            Field(page, "Origine des dommages", "Choc arrière");
            Table(page,
            [
                ("Pare-chocs arrière (fourniture + peinture)", 1_180m),
                ("Hayon : redressage et peinture", 1_240m),
                ("Feu arrière gauche", 410m),
                ("Main d'œuvre (9 h)", 650m),
            ]);
            Field(page, "Total TTC", 3_480m.ToString("C", French));
            Field(page, "Validité", "30 jours");
        });

    public static byte[] InsuranceCertificate(DateOnly incidentDate) => Render(
        "Attestation d'assurance automobile",
        "Assurances Horizon (société fictive) — service sinistres",
        page =>
        {
            Field(page, "Contrat", "POL-104233 — formule Tous risques");
            Field(page, "Assuré", "Thomas Moreau");
            Field(page, "Véhicule", "Peugeot 308 — GH-482-PL");
            Field(page, "Période de validité", $"du {incidentDate.AddMonths(-7).ToString("dd/MM/yyyy", French)} au {incidentDate.AddMonths(5).ToString("dd/MM/yyyy", French)}");
            Field(page, "Franchise dommages", 300m.ToString("C", French));
        });

    public static byte[] LeakInvestigation(DateOnly incidentDate) => Render(
        "Rapport de recherche de fuite",
        "Plomberie Saône Services (société fictive) — intervention n° RF-8812",
        page =>
        {
            Field(page, "Adresse", "8 quai Perrache, 69002 Lyon — appartement 3B");
            Field(page, "Date d'intervention", incidentDate.AddDays(1).ToString("dd/MM/yyyy", French));
            Field(page, "Origine", "Fuite sur le raccord d'alimentation du lave-linge de l'appartement du dessus (4B).");
            Field(page, "Constat", "Plafond de cuisine taché sur environ 6 m², peinture cloquée, humidité 38 % au plafond.");
            Field(page, "Réparation de la cause", "Raccord remplacé le jour de l'intervention ; fuite stoppée.");
            Field(page, "Recommandation", "Laisser sécher 3 semaines avant reprise des embellissements.");
        });

    public static byte[] CeilingQuote(DateOnly incidentDate) => Render(
        "Devis embellissements n° E-5531",
        "Peintures Croix-Rousse (société fictive)",
        page =>
        {
            Field(page, "Chantier", "8 quai Perrache, 69002 Lyon — cuisine appartement 3B");
            Field(page, "Date du devis", incidentDate.AddDays(4).ToString("dd/MM/yyyy", French));
            Table(page,
            [
                ("Traitement anti-taches et enduit plafond (6 m²)", 1_150m),
                ("Peinture plafond et murs cuisine", 2_350m),
                ("Protection, nettoyage, évacuation", 520m),
                ("Main d'œuvre complémentaire", 780m),
            ]);
            Field(page, "Total TTC", 4_800m.ToString("C", French));
        });

    private static byte[] Render(string title, string subtitle, Action<ColumnDescriptor> body) =>
        Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(style => style.FontSize(11));
            page.Header().Column(header =>
            {
                header.Item().Text(title).FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                header.Item().Text(subtitle).FontSize(9).FontColor(Colors.Grey.Darken1);
                header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });
            page.Content().PaddingTop(16).Column(column =>
            {
                column.Spacing(8);
                body(column);
            });
            page.Footer().AlignCenter().Text("Document d'exemple généré pour la démonstration ClaimFlow — données fictives")
                .FontSize(8).FontColor(Colors.Grey.Medium);
        })).GeneratePdf();

    private static void Field(ColumnDescriptor column, string label, string value) =>
        column.Item().Text(text =>
        {
            text.Span($"{label} : ").SemiBold();
            text.Span(value);
        });

    private static void Table(ColumnDescriptor column, (string Label, decimal Amount)[] lines) =>
        column.Item().PaddingVertical(4).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(110);
            });
            foreach (var (label, amount) in lines)
            {
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).Text(label);
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).AlignRight().Text(amount.ToString("C", French));
            }
        });
}
