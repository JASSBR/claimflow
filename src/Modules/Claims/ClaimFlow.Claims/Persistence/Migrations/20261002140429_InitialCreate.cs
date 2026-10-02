using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimFlow.Claims.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "claims");

            migrationBuilder.CreateSequence(
                name: "claim_number_seq",
                schema: "claims");

            migrationBuilder.CreateTable(
                name: "claims",
                schema: "claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PolicyNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IncidentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ClaimedAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeclaredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeclaredById = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DeclaredByName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApprovedById = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "claim_history",
                schema: "claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    From = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    To = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ActorId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ActorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claim_history_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalSchema: "claims",
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_claim_history_ClaimId",
                schema: "claims",
                table: "claim_history",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_claims_Number",
                schema: "claims",
                table: "claims",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_claims_PolicyNumber",
                schema: "claims",
                table: "claims",
                column: "PolicyNumber");

            migrationBuilder.CreateIndex(
                name: "IX_claims_Status_DeclaredAt",
                schema: "claims",
                table: "claims",
                columns: new[] { "Status", "DeclaredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_OccurredAt",
                schema: "claims",
                table: "outbox_messages",
                column: "OccurredAt",
                filter: "\"ProcessedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "claim_history",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "claims");

            migrationBuilder.DropTable(
                name: "claims",
                schema: "claims");

            migrationBuilder.DropSequence(
                name: "claim_number_seq",
                schema: "claims");
        }
    }
}
