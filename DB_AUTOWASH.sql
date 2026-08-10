-- =============================================
-- DATABASE SETUP SCRIPT - SQL SERVER
-- PROJECT: AUTOWASH PRO
-- =============================================

-- 1. Tạo Database
CREATE DATABASE AutoWashPro;
GO

-- Chuyển sang Database vừa tạo
USE AutoWashPro;
GO


-- =============================================
-- 2. BẢNG KHÁCH HÀNG
-- =============================================
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    PhoneNumber VARCHAR(15) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,

    CurrentTier VARCHAR(20) DEFAULT 'Member'
        CHECK (CurrentTier IN ('Member', 'Silver', 'Gold', 'Platinum')),

    TotalSpent DECIMAL(18, 2) DEFAULT 0,
    CurrentPoints INT DEFAULT 0,

    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 3. BẢNG PHƯƠNG TIỆN
-- =============================================
CREATE TABLE Vehicles (
    VehicleID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL,

    LicensePlate VARCHAR(20) NOT NULL UNIQUE,
    VehicleType NVARCHAR(50),

    -- URL hình ảnh xe trước khi rửa
    ImageFrontUrl NVARCHAR(500),
    ImageBackOrSideUrl NVARCHAR(500),

    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (CustomerID)
        REFERENCES Customers(CustomerID)
);
GO


-- =============================================
-- 4. BẢNG KHUYẾN MÃI
-- =============================================
CREATE TABLE Promotions (
    PromotionID INT IDENTITY(1,1) PRIMARY KEY,

    PromoCode VARCHAR(50) UNIQUE,
    PromoName NVARCHAR(100) NOT NULL,

    PromoType VARCHAR(20)
        CHECK (PromoType IN ('Discount', 'FreeWash', 'AddOn')),

    TargetTier VARCHAR(20)
        CHECK (TargetTier IN ('Member', 'Silver', 'Gold', 'Platinum', 'All')),

    ValidFrom DATETIME,
    ValidTo DATETIME,

    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 5. BẢNG BOOKING / LỊCH SỬ RỬA XE
-- =============================================
CREATE TABLE Bookings (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,

    CustomerID INT NOT NULL,
    VehicleID INT NOT NULL,
    PromotionID INT NULL,

    -- Thời gian khách đặt lịch
    BookingTime DATETIME NOT NULL,

    -- Thời gian thực tế hoàn thành việc rửa
    ActualWashTime DATETIME NULL,

    Status VARCHAR(20) DEFAULT 'Pending'
        CHECK (Status IN ('Pending', 'Completed', 'Cancelled', 'NoShow')),

    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (CustomerID)
        REFERENCES Customers(CustomerID),

    FOREIGN KEY (VehicleID)
        REFERENCES Vehicles(VehicleID),

    FOREIGN KEY (PromotionID)
        REFERENCES Promotions(PromotionID)
);
GO


-- =============================================
-- 6. BẢNG SỔ CÁI ĐIỂM
-- =============================================
CREATE TABLE PointLedger (
    TransactionID INT IDENTITY(1,1) PRIMARY KEY,

    CustomerID INT NOT NULL,
    BookingID INT NULL,

    -- Dương = cộng điểm
    -- Âm = trừ điểm
    Points INT NOT NULL,

    TransactionType VARCHAR(20)
        CHECK (TransactionType IN ('Earn', 'Redeem', 'Expire')),

    -- Ngày hết hạn của giao dịch điểm
    ExpireDate DATETIME NULL,

    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (CustomerID)
        REFERENCES Customers(CustomerID),

    FOREIGN KEY (BookingID)
        REFERENCES Bookings(BookingID)
);
GO