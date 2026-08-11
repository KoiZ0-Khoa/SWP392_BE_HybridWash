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
    
    Role VARCHAR(50) DEFAULT 'Washer'
        CHECK (Role IN ('Washer', 'Manager', 'Admin')),

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
    Capacity INT NOT NULL,   -- Sức chứa (VD: nhận tối đa 5 xe)
    
    IsActive BIT DEFAULT 1,  -- Bật/tắt khung giờ
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO


-- =============================================
-- 8. BẢNG BOOKING / LỊCH SỬ RỬA XE (ĐÃ CẬP NHẬT SERVICE VÀ PRICE)
-- =============================================
CREATE TABLE Bookings (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,

    CustomerID INT NOT NULL,
    VehicleID INT NOT NULL,
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

    -- Trạng thái flow
    Status VARCHAR(20) DEFAULT 'Pending'
        CHECK (Status IN ('Pending', 'CheckedIn', 'Washing', 'Completed', 'Cancelled', 'NoShow')),

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

    -- Dương = cộng điểm, Âm = trừ điểm
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


-- Thêm cột PasswordHash cho bảng Khách hàng
ALTER TABLE Customers 
ADD PasswordHash VARCHAR(255) NULL;
GO

-- Thêm cột PasswordHash cho bảng Nhân viên
ALTER TABLE Staff 
ADD PasswordHash VARCHAR(255) NULL;
GO