-- =============================================
-- DATABASE SETUP SCRIPT - SQL SERVER
-- PROJECT: AUTOWASH
-- =============================================

-- 1. Tạo Database
CREATE DATABASE AUTOWASH;
GO

-- Chuyển sang Database vừa tạo
USE AUTOWASH;
GO


-- =============================================
-- 2. BẢNG KHÁCH HÀNG
-- =============================================
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    PhoneNumber VARCHAR(15) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
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
    Description NVARCHAR(500),

    PromoType VARCHAR(20)
        CHECK (PromoType IN ('Discount', 'FreeWash', 'AddOn')),

    TargetTier VARCHAR(20)
        CHECK (TargetTier IN ('Member', 'Silver', 'Gold', 'Platinum', 'All')),

    ValidFrom DATETIME,
    ValidTo DATETIME,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 5. BẢNG DỊCH VỤ (SERVICES) - [BẢNG MỚI]
-- =============================================
CREATE TABLE Services (
    ServiceID INT IDENTITY(1,1) PRIMARY KEY,
    ServiceName NVARCHAR(100) NOT NULL, -- Tên gói: Rửa tiêu chuẩn, Rửa VIP bọt tuyết...
    Description NVARCHAR(500),          -- Mô tả gói dịch vụ
    Price DECIMAL(18, 2) NOT NULL,      -- Giá tiền gốc của dịch vụ
    IsActive BIT DEFAULT 1,             -- 1: Đang phục vụ, 0: Ngừng phục vụ
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 6. BẢNG NHÂN VIÊN (STAFF)
-- =============================================
CREATE TABLE Staff (
    StaffID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(15) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    
    Role VARCHAR(50) DEFAULT 'Staff'
        CHECK (Role IN ('Staff', 'Manager', 'Admin')),

    IsActive BIT DEFAULT 1, -- 1: Đang làm việc, 0: Đã nghỉ
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 7. BẢNG KHUNG GIỜ (TIMESLOTS)
-- =============================================
CREATE TABLE TimeSlots (
    SlotID INT IDENTITY(1,1) PRIMARY KEY,
    
    StartTime TIME NOT NULL, -- Giờ bắt đầu (VD: 08:00)
    EndTime TIME NOT NULL,   -- Giờ kết thúc (VD: 10:00)
    CarCapacity INT NOT NULL DEFAULT 2,  
    BikeCapacity INT NOT NULL DEFAULT 5,
    
    IsActive BIT DEFAULT 1,  -- Bật/tắt khung giờ
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 8. BẢNG BOOKING / LỊCH SỬ RỬA XE (ĐÃ CẬP NHẬT SERVICE VÀ PRICE)
-- =============================================
CREATE TABLE Bookings (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,

    CustomerID INT NULL,
    VehicleID INT NULL,

    -- Thông tin khách vãng lai (Nếu CustomerID / VehicleID là NULL)
    GuestName NVARCHAR(100) NULL,
    GuestPhone VARCHAR(15) NULL,
    GuestLicensePlate VARCHAR(20) NULL,
    GuestVehicleType VARCHAR(20) NULL,

    ServiceID INT NOT NULL,  -- Gói dịch vụ khách chọn
    PromotionID INT NULL,
    
    SlotID INT NOT NULL,     -- Khóa ngoại nối vào khung giờ
    StaffID INT NULL,        -- Nhân viên nhận rửa (Khách đặt online thì NULL)

    -- Thời gian khách đặt lịch (Chỉ lấy ngày)
    BookingDate DATE NOT NULL,

    -- Lưu lại giá tiền để chốt sổ thu tiền (Không bị ảnh hưởng nếu sau này đổi giá dịch vụ)
    OriginalPrice DECIMAL(18, 2) DEFAULT 0, 
    FinalPrice DECIMAL(18, 2) DEFAULT 0,    

    -- Thời gian thực tế hoàn thành việc rửa
    ActualWashTime DATETIME NULL,

    -- Hình ảnh xe trước khi rửa & Ghi chú của Staff
    IncidentImage1 NVARCHAR(500) NULL,
    IncidentImage2 NVARCHAR(500) NULL,
    StaffNote NVARCHAR(1000) NULL,

    -- Trạng thái flow
    Status VARCHAR(20) DEFAULT 'Pending'
        CHECK (Status IN ('Pending', 'Confirmed', 'CheckedIn', 'Washing', 'Completed', 'CheckedOut', 'Cancelled', 'NoShow')),

    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID),
    FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID), -- Khóa ngoại bảng Services
    FOREIGN KEY (PromotionID) REFERENCES Promotions(PromotionID),
    FOREIGN KEY (SlotID) REFERENCES TimeSlots(SlotID),
    FOREIGN KEY (StaffID) REFERENCES Staff(StaffID)
);
GO


-- =============================================
-- 9. BẢNG SỔ CÁI ĐIỂM
-- =============================================
CREATE TABLE PointLedger (
    TransactionID INT IDENTITY(1,1) PRIMARY KEY,

    CustomerID INT NOT NULL,
    BookingID INT NULL,
    RewardRedemptionID INT NULL,

    -- Dương = cộng điểm, Âm = trừ điểm
    Points INT NOT NULL,

    TransactionType VARCHAR(20)
        CHECK (TransactionType IN ('Earn', 'Redeem', 'Expire')),

    Description NVARCHAR(500),

    -- Ngày hết hạn của giao dịch điểm
    ExpireDate DATETIME NULL,

    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID)
);
GO


-- =============================================
-- 10. BẢNG PHẦN THƯỞNG
-- =============================================
CREATE TABLE Rewards (
    RewardID INT IDENTITY(1,1) NOT NULL,
    RewardName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    RewardType VARCHAR(20) NOT NULL
        CHECK (RewardType IN ('Discount', 'FreeWash', 'AddOn')),
    PointCost INT NOT NULL CHECK (PointCost > 0),
    DiscountValue DECIMAL(18,2) NULL CHECK (DiscountValue IS NULL OR DiscountValue > 0),
    ServiceID INT NULL,
    MinimumTier VARCHAR(20) NOT NULL DEFAULT 'Member'
        CHECK (MinimumTier IN ('Member', 'Silver', 'Gold', 'Platinum')),
    ValidFrom DATETIME NULL,
    ValidTo DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Rewards PRIMARY KEY (RewardID),
    CONSTRAINT UQ_Rewards_RewardName UNIQUE (RewardName),
    CONSTRAINT FK_Rewards_Services FOREIGN KEY (ServiceID) REFERENCES Services(ServiceID),
    CHECK (ValidFrom IS NULL OR ValidTo IS NULL OR ValidFrom < ValidTo)
);
GO

CREATE INDEX IX_Rewards_ServiceID ON Rewards(ServiceID);
GO


-- =============================================
-- 11. BẢNG LỊCH SỬ ĐỔI PHẦN THƯỞNG
-- =============================================
CREATE TABLE RewardRedemptions (
    RedemptionID INT IDENTITY(1,1) NOT NULL,
    RequestId UNIQUEIDENTIFIER NOT NULL,
    CustomerID INT NOT NULL,
    RewardID INT NOT NULL,
    PointsSpent INT NOT NULL CHECK (PointsSpent > 0),
    Status VARCHAR(20) NOT NULL DEFAULT 'Issued'
        CHECK (Status IN ('Issued', 'Used', 'Cancelled', 'Expired')),
    RedeemedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UsedAt DATETIME NULL,
    BookingID INT NULL,

    CONSTRAINT PK_RewardRedemptions PRIMARY KEY (RedemptionID),
    CONSTRAINT UQ_RewardRedemptions_RequestId UNIQUE (RequestId),
    CONSTRAINT FK_RewardRedemptions_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    CONSTRAINT FK_RewardRedemptions_Rewards FOREIGN KEY (RewardID) REFERENCES Rewards(RewardID),
    CONSTRAINT FK_RewardRedemptions_Bookings FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID)
);
GO

CREATE INDEX IX_RewardRedemptions_CustomerID ON RewardRedemptions(CustomerID);
CREATE INDEX IX_RewardRedemptions_RewardID ON RewardRedemptions(RewardID);
CREATE INDEX IX_RewardRedemptions_BookingID ON RewardRedemptions(BookingID);
GO

ALTER TABLE PointLedger
ADD CONSTRAINT FK_PointLedger_RewardRedemptions
FOREIGN KEY (RewardRedemptionID) REFERENCES RewardRedemptions(RedemptionID);
GO

CREATE UNIQUE INDEX UX_PointLedger_RewardRedemptionID
ON PointLedger(RewardRedemptionID)
WHERE RewardRedemptionID IS NOT NULL;
GO

-- =============================================
-- 12. BẢNG BIÊN BẢN BÀN GIAO XE
-- =============================================
CREATE TABLE ParkingReceipts (
    ReceiptID INT IDENTITY(1,1) PRIMARY KEY,
    BookingID INT NOT NULL UNIQUE,
    IssueStaffID INT NOT NULL,
    VerifyStaffID INT NULL,
    Status VARCHAR(20) DEFAULT 'Issued',
    IsCustomerLeaving BIT DEFAULT 0,
    CustomerSignature NVARCHAR(MAX) NULL,
    IssuedAt DATETIME DEFAULT GETDATE(),
    VerifiedAt DATETIME NULL,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    FOREIGN KEY (IssueStaffID) REFERENCES Staff(StaffID),
    FOREIGN KEY (VerifyStaffID) REFERENCES Staff(StaffID)
);
GO
