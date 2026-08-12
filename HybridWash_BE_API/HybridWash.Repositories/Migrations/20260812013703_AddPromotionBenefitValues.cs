using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionBenefitValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Promotions', 'DiscountType') IS NULL
                    ALTER TABLE dbo.Promotions ADD DiscountType varchar(20) NULL;

                IF COL_LENGTH('dbo.Promotions', 'DiscountValue') IS NULL
                    ALTER TABLE dbo.Promotions ADD DiscountValue decimal(18,2) NULL;

                IF COL_LENGTH('dbo.Promotions', 'MaxDiscount') IS NULL
                    ALTER TABLE dbo.Promotions ADD MaxDiscount decimal(18,2) NULL;

                IF COL_LENGTH('dbo.Promotions', 'ServiceID') IS NULL
                    ALTER TABLE dbo.Promotions ADD ServiceID int NULL;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Promotions_ServiceID'
                      AND object_id = OBJECT_ID('dbo.Promotions'))
                    CREATE INDEX IX_Promotions_ServiceID
                        ON dbo.Promotions(ServiceID);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_Promotions_Services')
                    ALTER TABLE dbo.Promotions
                    ADD CONSTRAINT FK_Promotions_Services
                        FOREIGN KEY (ServiceID) REFERENCES dbo.Services(ServiceID);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_Promotions_Services')
                    ALTER TABLE dbo.Promotions
                        DROP CONSTRAINT FK_Promotions_Services;

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Promotions_ServiceID'
                      AND object_id = OBJECT_ID('dbo.Promotions'))
                    DROP INDEX IX_Promotions_ServiceID ON dbo.Promotions;

                IF COL_LENGTH('dbo.Promotions', 'ServiceID') IS NOT NULL
                    ALTER TABLE dbo.Promotions DROP COLUMN ServiceID;

                IF COL_LENGTH('dbo.Promotions', 'MaxDiscount') IS NOT NULL
                    ALTER TABLE dbo.Promotions DROP COLUMN MaxDiscount;

                IF COL_LENGTH('dbo.Promotions', 'DiscountValue') IS NOT NULL
                    ALTER TABLE dbo.Promotions DROP COLUMN DiscountValue;

                IF COL_LENGTH('dbo.Promotions', 'DiscountType') IS NOT NULL
                    ALTER TABLE dbo.Promotions DROP COLUMN DiscountType;
                """);
        }
    }
}
