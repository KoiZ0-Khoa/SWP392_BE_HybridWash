using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HybridWash.Repositories.Migrations
{
    /// <summary>
    /// Baselines the existing AUTOWASH database and adds only the Flow 4 schema.
    /// Existing application tables are intentionally not recreated.
    /// </summary>
    public partial class Flow4RewardPromotionBaseline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.Promotions', 'Description') IS NULL
                    ALTER TABLE dbo.Promotions ADD Description NVARCHAR(500) NULL;

                IF COL_LENGTH('dbo.Promotions', 'IsActive') IS NULL
                    ALTER TABLE dbo.Promotions ADD IsActive BIT NOT NULL
                        CONSTRAINT DF_Promotions_IsActive DEFAULT 1;

                IF OBJECT_ID('dbo.Rewards', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Rewards (
                        RewardID INT IDENTITY(1,1) NOT NULL,
                        RewardName NVARCHAR(100) NOT NULL,
                        Description NVARCHAR(500) NULL,
                        RewardType VARCHAR(20) NOT NULL,
                        PointCost INT NOT NULL,
                        DiscountValue DECIMAL(18,2) NULL,
                        ServiceID INT NULL,
                        MinimumTier VARCHAR(20) NOT NULL
                            CONSTRAINT DF_Rewards_MinimumTier DEFAULT 'Member',
                        ValidFrom DATETIME NULL,
                        ValidTo DATETIME NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Rewards_IsActive DEFAULT 1,
                        CreatedAt DATETIME NULL CONSTRAINT DF_Rewards_CreatedAt DEFAULT GETDATE(),

                        CONSTRAINT PK_Rewards PRIMARY KEY (RewardID),
                        CONSTRAINT CK_Rewards_RewardType
                            CHECK (RewardType IN ('Discount', 'FreeWash', 'AddOn')),
                        CONSTRAINT CK_Rewards_PointCost CHECK (PointCost > 0),
                        CONSTRAINT CK_Rewards_DiscountValue
                            CHECK (DiscountValue IS NULL OR DiscountValue > 0),
                        CONSTRAINT CK_Rewards_MinimumTier
                            CHECK (MinimumTier IN ('Member', 'Silver', 'Gold', 'Platinum')),
                        CONSTRAINT CK_Rewards_ValidDates
                            CHECK (ValidFrom IS NULL OR ValidTo IS NULL OR ValidFrom < ValidTo),
                        CONSTRAINT FK_Rewards_Services
                            FOREIGN KEY (ServiceID) REFERENCES dbo.Services(ServiceID)
                    );
                END;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'UQ_Rewards_RewardName'
                      AND object_id = OBJECT_ID('dbo.Rewards')
                )
                    CREATE UNIQUE INDEX UQ_Rewards_RewardName
                    ON dbo.Rewards(RewardName);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Rewards_ServiceID'
                      AND object_id = OBJECT_ID('dbo.Rewards')
                )
                    CREATE INDEX IX_Rewards_ServiceID
                    ON dbo.Rewards(ServiceID);

                IF OBJECT_ID('dbo.RewardRedemptions', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.RewardRedemptions (
                        RedemptionID INT IDENTITY(1,1) NOT NULL,
                        RequestId UNIQUEIDENTIFIER NOT NULL,
                        CustomerID INT NOT NULL,
                        RewardID INT NOT NULL,
                        PointsSpent INT NOT NULL,
                        Status VARCHAR(20) NOT NULL
                            CONSTRAINT DF_RewardRedemptions_Status DEFAULT 'Issued',
                        RedeemedAt DATETIME NOT NULL,
                        UsedAt DATETIME NULL,
                        BookingID INT NULL,

                        CONSTRAINT PK_RewardRedemptions PRIMARY KEY (RedemptionID),
                        CONSTRAINT CK_RewardRedemptions_PointsSpent CHECK (PointsSpent > 0),
                        CONSTRAINT CK_RewardRedemptions_Status
                            CHECK (Status IN ('Issued', 'Used', 'Cancelled', 'Expired')),
                        CONSTRAINT FK_RewardRedemptions_Customers
                            FOREIGN KEY (CustomerID) REFERENCES dbo.Customers(CustomerID),
                        CONSTRAINT FK_RewardRedemptions_Rewards
                            FOREIGN KEY (RewardID) REFERENCES dbo.Rewards(RewardID),
                        CONSTRAINT FK_RewardRedemptions_Bookings
                            FOREIGN KEY (BookingID) REFERENCES dbo.Bookings(BookingID)
                    );
                END;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'UQ_RewardRedemptions_RequestId'
                      AND object_id = OBJECT_ID('dbo.RewardRedemptions')
                )
                    CREATE UNIQUE INDEX UQ_RewardRedemptions_RequestId
                    ON dbo.RewardRedemptions(RequestId);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_RewardRedemptions_BookingID'
                      AND object_id = OBJECT_ID('dbo.RewardRedemptions')
                )
                    CREATE INDEX IX_RewardRedemptions_BookingID
                    ON dbo.RewardRedemptions(BookingID);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_RewardRedemptions_CustomerID'
                      AND object_id = OBJECT_ID('dbo.RewardRedemptions')
                )
                    CREATE INDEX IX_RewardRedemptions_CustomerID
                    ON dbo.RewardRedemptions(CustomerID);

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_RewardRedemptions_RewardID'
                      AND object_id = OBJECT_ID('dbo.RewardRedemptions')
                )
                    CREATE INDEX IX_RewardRedemptions_RewardID
                    ON dbo.RewardRedemptions(RewardID);

                """);

            migrationBuilder.Sql(
                """

                IF COL_LENGTH('dbo.PointLedger', 'RewardRedemptionID') IS NULL
                    ALTER TABLE dbo.PointLedger ADD RewardRedemptionID INT NULL;

                IF COL_LENGTH('dbo.PointLedger', 'Description') IS NULL
                    ALTER TABLE dbo.PointLedger ADD Description NVARCHAR(500) NULL;

                """);

            migrationBuilder.Sql(
                """

                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_PointLedger_RewardRedemptions'
                )
                BEGIN
                    ALTER TABLE dbo.PointLedger
                    ADD CONSTRAINT FK_PointLedger_RewardRedemptions
                        FOREIGN KEY (RewardRedemptionID)
                        REFERENCES dbo.RewardRedemptions(RedemptionID);
                END;

                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'UX_PointLedger_RewardRedemptionID'
                      AND object_id = OBJECT_ID('dbo.PointLedger')
                )
                    CREATE UNIQUE INDEX UX_PointLedger_RewardRedemptionID
                    ON dbo.PointLedger(RewardRedemptionID)
                    WHERE RewardRedemptionID IS NOT NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_PointLedger_RewardRedemptions'
                )
                    ALTER TABLE dbo.PointLedger
                    DROP CONSTRAINT FK_PointLedger_RewardRedemptions;

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'UX_PointLedger_RewardRedemptionID'
                      AND object_id = OBJECT_ID('dbo.PointLedger')
                )
                    DROP INDEX UX_PointLedger_RewardRedemptionID ON dbo.PointLedger;

                IF COL_LENGTH('dbo.PointLedger', 'RewardRedemptionID') IS NOT NULL
                    ALTER TABLE dbo.PointLedger DROP COLUMN RewardRedemptionID;

                IF COL_LENGTH('dbo.PointLedger', 'Description') IS NOT NULL
                    ALTER TABLE dbo.PointLedger DROP COLUMN Description;

                IF OBJECT_ID('dbo.RewardRedemptions', 'U') IS NOT NULL
                    DROP TABLE dbo.RewardRedemptions;

                IF OBJECT_ID('dbo.Rewards', 'U') IS NOT NULL
                    DROP TABLE dbo.Rewards;

                IF COL_LENGTH('dbo.Promotions', 'IsActive') IS NOT NULL
                BEGIN
                    DECLARE @IsActiveConstraint NVARCHAR(128);
                    SELECT @IsActiveConstraint = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('dbo.Promotions')
                      AND c.name = 'IsActive';

                    IF @IsActiveConstraint IS NOT NULL
                        EXEC('ALTER TABLE dbo.Promotions DROP CONSTRAINT ['
                            + @IsActiveConstraint + ']');

                    ALTER TABLE dbo.Promotions DROP COLUMN IsActive;
                END;

                IF COL_LENGTH('dbo.Promotions', 'Description') IS NOT NULL
                    ALTER TABLE dbo.Promotions DROP COLUMN Description;
                """);
        }
    }
}
