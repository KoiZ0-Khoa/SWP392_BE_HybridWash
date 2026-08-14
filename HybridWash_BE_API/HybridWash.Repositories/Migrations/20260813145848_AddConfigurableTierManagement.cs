using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableTierManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastTierReviewedAt",
                table: "Customers",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerTierHistory",
                columns: table => new
                {
                    TierHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    PreviousTier = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    NewTier = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    QualifyingSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QualifyingVisits = table.Column<int>(type: "int", nullable: false),
                    ReviewType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTierHistory", x => x.TierHistoryID);
                    table.ForeignKey(
                        name: "FK_CustomerTierHistory_Customers",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TierRules",
                columns: table => new
                {
                    TierRuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TierName = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    MinimumSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumVisits = table.Column<int>(type: "int", nullable: false),
                    EvaluationPeriodMonths = table.Column<int>(type: "int", nullable: false),
                    BookingWindowDays = table.Column<int>(type: "int", nullable: false),
                    PointMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BenefitDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TierRules", x => x.TierRuleID);
                });

            migrationBuilder.InsertData(
                table: "TierRules",
                columns: new[] { "TierRuleID", "BenefitDescription", "BookingWindowDays", "EvaluationPeriodMonths", "IsActive", "MinimumSpend", "MinimumVisits", "PointMultiplier", "Rank", "TierName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Book up to 7 days in advance.", 7, 12, true, 0m, 0, 1.00m, 1, "Member", new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Book up to 10 days in advance and earn 10% bonus points.", 10, 12, true, 500000m, 5, 1.10m, 2, "Silver", new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Book up to 12 days in advance and earn 25% bonus points.", 12, 12, true, 2000000m, 15, 1.25m, 3, "Gold", new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "Book up to 14 days in advance and earn 50% bonus points.", 14, 12, true, 5000000m, 30, 1.50m, 4, "Platinum", new DateTime(2026, 8, 13, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTierHistory_CustomerID_ReviewedAt",
                table: "CustomerTierHistory",
                columns: new[] { "CustomerID", "ReviewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TierRules_Rank",
                table: "TierRules",
                column: "Rank",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TierRules_TierName",
                table: "TierRules",
                column: "TierName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerTierHistory");

            migrationBuilder.DropTable(
                name: "TierRules");

            migrationBuilder.DropColumn(
                name: "LastTierReviewedAt",
                table: "Customers");
        }
    }
}
