using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimFlow.Claims.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "claim_number_seq",
                schema: "claims");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "claim_number_seq",
                schema: "claims");
        }
    }
}
