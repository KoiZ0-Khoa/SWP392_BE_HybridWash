USE AUTOWASH;
GO

IF COL_LENGTH('dbo.Promotions', 'Description') IS NULL
    ALTER TABLE dbo.Promotions ADD Description NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.Promotions', 'IsActive') IS NULL
    ALTER TABLE dbo.Promotions ADD IsActive BIT NOT NULL
        CONSTRAINT DF_Promotions_IsActive DEFAULT 1;
GO

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
        CONSTRAINT UQ_Rewards_RewardName UNIQUE (RewardName),
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
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Rewards_ServiceID'
      AND object_id = OBJECT_ID('dbo.Rewards')
)
    CREATE INDEX IX_Rewards_ServiceID ON dbo.Rewards(ServiceID);
GO

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
        RedeemedAt DATETIME NOT NULL
            CONSTRAINT DF_RewardRedemptions_RedeemedAt DEFAULT GETDATE(),
        UsedAt DATETIME NULL,
        BookingID INT NULL,

        CONSTRAINT PK_RewardRedemptions PRIMARY KEY (RedemptionID),
        CONSTRAINT UQ_RewardRedemptions_RequestId UNIQUE (RequestId),
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
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_RewardRedemptions_CustomerID'
      AND object_id = OBJECT_ID('dbo.RewardRedemptions')
)
    CREATE INDEX IX_RewardRedemptions_CustomerID
    ON dbo.RewardRedemptions(CustomerID);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_RewardRedemptions_RewardID'
      AND object_id = OBJECT_ID('dbo.RewardRedemptions')
)
    CREATE INDEX IX_RewardRedemptions_RewardID
    ON dbo.RewardRedemptions(RewardID);
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_RewardRedemptions_BookingID'
      AND object_id = OBJECT_ID('dbo.RewardRedemptions')
)
    CREATE INDEX IX_RewardRedemptions_BookingID
    ON dbo.RewardRedemptions(BookingID);
GO

IF COL_LENGTH('dbo.PointLedger', 'RewardRedemptionID') IS NULL
    ALTER TABLE dbo.PointLedger ADD RewardRedemptionID INT NULL;
GO

IF COL_LENGTH('dbo.PointLedger', 'Description') IS NULL
    ALTER TABLE dbo.PointLedger ADD Description NVARCHAR(500) NULL;
GO

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
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_PointLedger_RewardRedemptionID'
      AND object_id = OBJECT_ID('dbo.PointLedger')
)
BEGIN
    CREATE UNIQUE INDEX UX_PointLedger_RewardRedemptionID
    ON dbo.PointLedger(RewardRedemptionID)
    WHERE RewardRedemptionID IS NOT NULL;
END;
GO
