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
-- 5. BẢNG NHÂN VIÊN (STAFF) - [BẢNG MỚI]
-- =============================================
CREATE TABLE Staff (
    StaffID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(15) UNIQUE NOT NULL,
    
    Role VARCHAR(50) DEFAULT 'Washer'
        CHECK (Role IN ('Washer', 'Manager', 'Admin')),

    IsActive BIT DEFAULT 1, -- 1: Đang làm việc, 0: Đã nghỉ
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 6. BẢNG KHUNG GIỜ (TIMESLOTS) - [BẢNG MỚI]
-- =============================================
CREATE TABLE TimeSlots (
    SlotID INT IDENTITY(1,1) PRIMARY KEY,
    
    StartTime TIME NOT NULL, -- Giờ bắt đầu (VD: 08:00)
    EndTime TIME NOT NULL,   -- Giờ kết thúc (VD: 10:00)
    Capacity INT NOT NULL,   -- Sức chứa (VD: nhận tối đa 5 xe)
    
    IsActive BIT DEFAULT 1,  -- Bật/tắt khung giờ
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 7. BẢNG BOOKING / LỊCH SỬ RỬA XE (ĐÃ CẬP NHẬT)
-- =============================================
CREATE TABLE Bookings (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,

    CustomerID INT NOT NULL,
    VehicleID INT NOT NULL,
    PromotionID INT NULL,
    
    SlotID INT NOT NULL,     -- Khóa ngoại nối vào khung giờ
    StaffID INT NULL,        -- Nhân viên nhận rửa (Khách đặt online thì NULL)

    -- Thời gian khách đặt lịch (Chỉ lấy ngày)
    BookingDate DATE NOT NULL,

    -- Thời gian thực tế hoàn thành việc rửa
    ActualWashTime DATETIME NULL,

    -- Thêm trạng thái CheckedIn và Washing cho đúng flow
    Status VARCHAR(20) DEFAULT 'Pending'
        CHECK (Status IN ('Pending', 'CheckedIn', 'Washing', 'Completed', 'Cancelled', 'NoShow')),

    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (VehicleID) REFERENCES Vehicles(VehicleID),
    FOREIGN KEY (PromotionID) REFERENCES Promotions(PromotionID),
    
    -- Ràng buộc 2 bảng mới
    FOREIGN KEY (SlotID) REFERENCES TimeSlots(SlotID),
    FOREIGN KEY (StaffID) REFERENCES Staff(StaffID)
);
GO


-- =============================================
-- 8. BẢNG SỔ CÁI ĐIỂM
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

    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID)
);
GO