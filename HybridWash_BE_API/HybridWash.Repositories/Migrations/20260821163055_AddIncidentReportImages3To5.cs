using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentReportImages3To5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportedImage3",
                table: "IncidentReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportedImage4",
                table: "IncidentReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportedImage5",
                table: "IncidentReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportedImage3",
                table: "IncidentReports");

            migrationBuilder.DropColumn(
                name: "ReportedImage4",
                table: "IncidentReports");

            migrationBuilder.DropColumn(
                name: "ReportedImage5",
                table: "IncidentReports");
        }
    }
}
