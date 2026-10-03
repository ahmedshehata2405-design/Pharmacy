-- =========================================================
-- PharmacyDB - Full Database Script
-- Pharmacy Management System (ASP.NET Core MVC - 3-Layer Architecture)
-- =========================================================

CREATE DATABASE PharmacyDB;
GO

USE PharmacyDB;
GO

-- =========================================================
-- 1. Roles
-- =========================================================
CREATE TABLE Roles
(
    RoleID   INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

INSERT INTO Roles (RoleName) VALUES (N'Admin'), (N'Pharmacist'), (N'Cashier');
GO

-- =========================================================
-- 2. Branches
-- =========================================================
CREATE TABLE Branches
(
    BranchID   INT IDENTITY(1,1) PRIMARY KEY,
    BranchName NVARCHAR(100) NOT NULL,
    Address    NVARCHAR(200) NULL,
    Phone      NVARCHAR(20)  NULL
);
GO

-- =========================================================
-- 3. Users
-- =========================================================
CREATE TABLE Users
(
    UserID       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    Email        NVARCHAR(100) NULL,
    RoleID       INT NOT NULL,
    BranchID     INT NOT NULL,
    IsActive     BIT NOT NULL DEFAULT 1,
    CreatedDate  DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Users_Roles    FOREIGN KEY (RoleID)   REFERENCES Roles(RoleID),
    CONSTRAINT FK_Users_Branches FOREIGN KEY (BranchID) REFERENCES Branches(BranchID)
);
GO

-- =========================================================
-- 4. Categories
-- =========================================================
CREATE TABLE Categories
(
    CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- =========================================================
-- 5. Medicines
-- =========================================================
CREATE TABLE Medicines
(
    MedicineID          INT IDENTITY(1,1) PRIMARY KEY,
    Name                NVARCHAR(150) NOT NULL,
    CategoryID          INT NOT NULL,
    Description         NVARCHAR(500) NULL,
    Unit                NVARCHAR(50)  NULL,  -- شريط / علبة / زجاجة ...
    RequiresPrescription BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Medicines_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO

-- =========================================================
-- 6. Suppliers
-- =========================================================
CREATE TABLE Suppliers
(
    SupplierID INT IDENTITY(1,1) PRIMARY KEY,
    Name       NVARCHAR(150) NOT NULL,
    Phone      NVARCHAR(20)  NULL,
    Email      NVARCHAR(100) NULL,
    Address    NVARCHAR(200) NULL
);
GO

-- =========================================================
-- 7. MedicineSuppliers (Many-to-Many)
-- =========================================================
CREATE TABLE MedicineSuppliers
(
    MedicineID    INT NOT NULL,
    SupplierID    INT NOT NULL,
    PurchasePrice DECIMAL(10,2) NOT NULL,

    CONSTRAINT PK_MedicineSuppliers PRIMARY KEY (MedicineID, SupplierID),
    CONSTRAINT FK_MedSup_Medicines FOREIGN KEY (MedicineID) REFERENCES Medicines(MedicineID),
    CONSTRAINT FK_MedSup_Suppliers FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID)
);
GO

-- =========================================================
-- 8. Inventory (per branch stock)
-- =========================================================
CREATE TABLE Inventory
(
    InventoryID  INT IDENTITY(1,1) PRIMARY KEY,
    BranchID     INT NOT NULL,
    MedicineID   INT NOT NULL,
    Quantity     INT NOT NULL DEFAULT 0,
    SellingPrice DECIMAL(10,2) NOT NULL,
    ExpiryDate   DATE NULL,

    CONSTRAINT FK_Inventory_Branches  FOREIGN KEY (BranchID)   REFERENCES Branches(BranchID),
    CONSTRAINT FK_Inventory_Medicines FOREIGN KEY (MedicineID) REFERENCES Medicines(MedicineID)
);
GO

-- =========================================================
-- 9. Purchases (from suppliers)
-- =========================================================
CREATE TABLE Purchases
(
    PurchaseID   INT IDENTITY(1,1) PRIMARY KEY,
    SupplierID   INT NOT NULL,
    BranchID     INT NOT NULL,
    PurchaseDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount  DECIMAL(12,2) NOT NULL DEFAULT 0,

    CONSTRAINT FK_Purchases_Suppliers FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID),
    CONSTRAINT FK_Purchases_Branches  FOREIGN KEY (BranchID)   REFERENCES Branches(BranchID)
);
GO

-- =========================================================
-- 10. PurchaseDetails
-- =========================================================
CREATE TABLE PurchaseDetails
(
    PurchaseDetailID INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseID       INT NOT NULL,
    MedicineID       INT NOT NULL,
    Quantity         INT NOT NULL,
    UnitPrice        DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_PurchaseDetails_Purchases FOREIGN KEY (PurchaseID) REFERENCES Purchases(PurchaseID),
    CONSTRAINT FK_PurchaseDetails_Medicines FOREIGN KEY (MedicineID) REFERENCES Medicines(MedicineID)
);
GO

-- =========================================================
-- 11. Customers
-- =========================================================
CREATE TABLE Customers
(
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    Name       NVARCHAR(150) NOT NULL,
    Phone      NVARCHAR(20)  NULL
);
GO

-- =========================================================
-- 12. Doctors
-- =========================================================
CREATE TABLE Doctors
(
    DoctorID       INT IDENTITY(1,1) PRIMARY KEY,
    Name           NVARCHAR(150) NOT NULL,
    Specialization NVARCHAR(100) NULL,
    Phone          NVARCHAR(20)  NULL
);
GO

-- =========================================================
-- 13. Prescriptions
-- =========================================================
CREATE TABLE Prescriptions
(
    PrescriptionID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID     INT NOT NULL,
    DoctorID       INT NOT NULL,
    IssueDate      DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Prescriptions_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    CONSTRAINT FK_Prescriptions_Doctors   FOREIGN KEY (DoctorID)   REFERENCES Doctors(DoctorID)
);
GO

-- =========================================================
-- 14. PrescriptionDetails
-- =========================================================
CREATE TABLE PrescriptionDetails
(
    PrescriptionDetailID INT IDENTITY(1,1) PRIMARY KEY,
    PrescriptionID       INT NOT NULL,
    MedicineID           INT NOT NULL,
    Dosage               NVARCHAR(100) NULL,
    Quantity             INT NOT NULL,

    CONSTRAINT FK_PrescDetails_Prescriptions FOREIGN KEY (PrescriptionID) REFERENCES Prescriptions(PrescriptionID),
    CONSTRAINT FK_PrescDetails_Medicines     FOREIGN KEY (MedicineID)     REFERENCES Medicines(MedicineID)
);
GO

-- =========================================================
-- 15. Sales (Invoices)
-- =========================================================
CREATE TABLE Sales
(
    SaleID         INT IDENTITY(1,1) PRIMARY KEY,
    BranchID       INT NOT NULL,
    CustomerID     INT NULL,
    PrescriptionID INT NULL,
    UserID         INT NOT NULL,  -- Cashier who made the sale
    SaleDate       DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount    DECIMAL(12,2) NOT NULL DEFAULT 0,

    CONSTRAINT FK_Sales_Branches      FOREIGN KEY (BranchID)       REFERENCES Branches(BranchID),
    CONSTRAINT FK_Sales_Customers     FOREIGN KEY (CustomerID)     REFERENCES Customers(CustomerID),
    CONSTRAINT FK_Sales_Prescriptions FOREIGN KEY (PrescriptionID) REFERENCES Prescriptions(PrescriptionID),
    CONSTRAINT FK_Sales_Users         FOREIGN KEY (UserID)         REFERENCES Users(UserID)
);
GO

-- =========================================================
-- 16. SaleDetails
-- =========================================================
CREATE TABLE SaleDetails
(
    SaleDetailID INT IDENTITY(1,1) PRIMARY KEY,
    SaleID       INT NOT NULL,
    MedicineID   INT NOT NULL,
    Quantity     INT NOT NULL,
    UnitPrice    DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_SaleDetails_Sales     FOREIGN KEY (SaleID)     REFERENCES Sales(SaleID),
    CONSTRAINT FK_SaleDetails_Medicines FOREIGN KEY (MedicineID) REFERENCES Medicines(MedicineID)
);
GO

-- =========================================================
-- Sample seed data (optional - for quick testing)
-- =========================================================
INSERT INTO Branches (BranchName, Address, Phone)
VALUES (N'الفرع الرئيسي',  N'شارع الجمهورية، وسط البلد، القاهرة',   N'0100000000'),
       (N'فرع مدينة نصر',  N'شارع عباس العقاد، مدينة نصر، القاهرة', N'01012345678'),
       (N'فرع الإسكندرية', N'شارع الكورنيش، سموحة، الإسكندرية',     N'01123456789'),
       (N'فرع الجيزة',     N'شارع الهرم، الجيزة',                    N'01234567890'),
       (N'فرع المنصورة',   N'شارع الجيش، المنصورة، الدقهلية',        N'01098765432');
GO

INSERT INTO Categories (CategoryName)
VALUES (N'مسكنات'), (N'مضادات حيوية'), (N'فيتامينات'),
       (N'أدوية القلب والضغط'), (N'السكر والغدد'), (N'الجهاز الهضمي'),
       (N'الحساسية والجهاز التنفسي'), (N'العناية بالبشرة والجروح'), (N'أدوية الأطفال');
GO

-- Medicines, stock, suppliers, customers, doctors and users: run 03_SeedData.sql after 02_TSQL_Logic.sql

PRINT N'PharmacyDB created successfully with all tables.';
