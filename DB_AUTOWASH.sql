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
    QrCode VARCHAR(255) UNIQUE,

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
        CHECK (Role IN ('Washer', 'Manager', 'Admin', 'Staff')),

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

    -- Thông tin khách vãng lai (Guest)
    GuestName NVARCHAR(100) NULL,
    GuestPhone VARCHAR(20) NULL,
    GuestLicensePlate VARCHAR(50) NULL,
    GuestVehicleType NVARCHAR(50) NULL,

    -- Hình ảnh và ghi chú
    IncidentImage1 NVARCHAR(500) NULL,
    IncidentImage2 NVARCHAR(500) NULL,
    StaffNote NVARCHAR(500) NULL,

    -- Trạng thái flow
    Status VARCHAR(20) DEFAULT 'Pending'
        CONSTRAINT CHK_Bookings_Status CHECK (Status IN ('Pending', 'Confirmed', 'CheckedIn', 'Washing', 'Completed', 'CheckedOut', 'Cancelled', 'NoShow')),

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

-- =============================================
-- 10. BẢNG BIÊN BẢN GỬI XE (PARKING RECEIPTS)
-- =============================================
CREATE TABLE ParkingReceipts (
    ReceiptID INT IDENTITY(1,1) PRIMARY KEY,
    BookingID INT NOT NULL UNIQUE,

    IssueStaffID INT NOT NULL,
    VerifyStaffID INT NULL,

    Status VARCHAR(20) DEFAULT 'Issued'
        CHECK (Status IN ('Issued', 'Verified')),

    IsCustomerLeaving BIT DEFAULT 0,
    CustomerSignature NVARCHAR(MAX) NULL,

    IssuedAt DATETIME DEFAULT GETDATE(),
    VerifiedAt DATETIME NULL,

    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    FOREIGN KEY (IssueStaffID) REFERENCES Staff(StaffID),
    FOREIGN KEY (VerifyStaffID) REFERENCES Staff(StaffID)
);
GO

