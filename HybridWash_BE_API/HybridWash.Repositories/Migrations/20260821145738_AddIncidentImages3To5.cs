using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentImages3To5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IncidentImage3",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentImage4",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentImage5",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncidentImage3",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IncidentImage4",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IncidentImage5",
                table: "Bookings");
        }
    }
}
