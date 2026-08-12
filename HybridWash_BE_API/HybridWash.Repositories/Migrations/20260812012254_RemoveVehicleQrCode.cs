using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVehicleQrCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.Vehicles', 'QrCode') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.Vehicles DROP COLUMN QrCode;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.Vehicles', 'QrCode') IS NULL
                BEGIN
                    ALTER TABLE dbo.Vehicles ADD QrCode NVARCHAR(MAX) NULL;
                END;
                """);
        }
    }
}
