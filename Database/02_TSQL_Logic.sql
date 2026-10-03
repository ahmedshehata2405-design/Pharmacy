-- =========================================================
-- PharmacyDB - T-SQL logic (run AFTER PharmacyDB.sql)
-- Safe to run more than once.
--
--   1. Data-integrity constraints
--   2. Indexes
--   3. Table types used to pass invoice lines to the procedures
--   4. usp_CreateSale      (validation + invoice + stock deduction, one transaction)
--   5. usp_CreatePurchase  (invoice + stock increase / new stock row, one transaction)
--
-- Needs SQL Server 2016 SP1 or newer (CREATE OR ALTER).
-- The web application calls 4 and 5 for every sale and purchase, so this
-- script is REQUIRED for those two screens to work.
-- =========================================================

USE PharmacyDB;
GO

-- Required by the filtered index below, and stored with each procedure when it is created.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- =========================================================
-- 1. Data-integrity constraints
-- =========================================================

-- The code assumes ONE inventory row per (branch, medicine). Enforce it in the database.
IF EXISTS (SELECT BranchID, MedicineID FROM dbo.Inventory GROUP BY BranchID, MedicineID HAVING COUNT(*) > 1)
    THROW 50001, N'Duplicate Inventory rows exist for the same branch + medicine. Merge them, then run this script again.', 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Inventory_Branch_Medicine' AND object_id = OBJECT_ID(N'dbo.Inventory'))
    CREATE UNIQUE INDEX UX_Inventory_Branch_Medicine ON dbo.Inventory (BranchID, MedicineID);
GO

-- Stock can never go negative, prices can never be negative, invoice lines must have a positive quantity.
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Inventory_Quantity')
    ALTER TABLE dbo.Inventory WITH CHECK ADD CONSTRAINT CK_Inventory_Quantity CHECK (Quantity >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Inventory_SellingPrice')
    ALTER TABLE dbo.Inventory WITH CHECK ADD CONSTRAINT CK_Inventory_SellingPrice CHECK (SellingPrice >= 0);

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_SaleDetails_Quantity')
    ALTER TABLE dbo.SaleDetails WITH CHECK ADD CONSTRAINT CK_SaleDetails_Quantity CHECK (Quantity > 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_SaleDetails_UnitPrice')
    ALTER TABLE dbo.SaleDetails WITH CHECK ADD CONSTRAINT CK_SaleDetails_UnitPrice CHECK (UnitPrice >= 0);

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_PurchaseDetails_Quantity')
    ALTER TABLE dbo.PurchaseDetails WITH CHECK ADD CONSTRAINT CK_PurchaseDetails_Quantity CHECK (Quantity > 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_PurchaseDetails_UnitPrice')
    ALTER TABLE dbo.PurchaseDetails WITH CHECK ADD CONSTRAINT CK_PurchaseDetails_UnitPrice CHECK (UnitPrice >= 0);

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_PrescriptionDetails_Quantity')
    ALTER TABLE dbo.PrescriptionDetails WITH CHECK ADD CONSTRAINT CK_PrescriptionDetails_Quantity CHECK (Quantity > 0);

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Sales_TotalAmount')
    ALTER TABLE dbo.Sales WITH CHECK ADD CONSTRAINT CK_Sales_TotalAmount CHECK (TotalAmount >= 0);
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Purchases_TotalAmount')
    ALTER TABLE dbo.Purchases WITH CHECK ADD CONSTRAINT CK_Purchases_TotalAmount CHECK (TotalAmount >= 0);
GO

-- =========================================================
-- 2. Indexes (foreign-key lookups, dashboard, expiry alerts)
-- =========================================================
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inventory_ExpiryDate' AND object_id = OBJECT_ID(N'dbo.Inventory'))
    CREATE INDEX IX_Inventory_ExpiryDate ON dbo.Inventory (ExpiryDate) WHERE ExpiryDate IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Sales_SaleDate' AND object_id = OBJECT_ID(N'dbo.Sales'))
    CREATE INDEX IX_Sales_SaleDate ON dbo.Sales (SaleDate) INCLUDE (TotalAmount);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleDetails_SaleID' AND object_id = OBJECT_ID(N'dbo.SaleDetails'))
    CREATE INDEX IX_SaleDetails_SaleID ON dbo.SaleDetails (SaleID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseDetails_PurchaseID' AND object_id = OBJECT_ID(N'dbo.PurchaseDetails'))
    CREATE INDEX IX_PurchaseDetails_PurchaseID ON dbo.PurchaseDetails (PurchaseID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PrescriptionDetails_PrescriptionID' AND object_id = OBJECT_ID(N'dbo.PrescriptionDetails'))
    CREATE INDEX IX_PrescriptionDetails_PrescriptionID ON dbo.PrescriptionDetails (PrescriptionID, MedicineID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Medicines_Name' AND object_id = OBJECT_ID(N'dbo.Medicines'))
    CREATE INDEX IX_Medicines_Name ON dbo.Medicines (Name);
GO

-- =========================================================
-- 3. Table types (invoice lines are sent to the procedures as one table parameter)
-- =========================================================
IF TYPE_ID(N'dbo.SaleLineType') IS NULL
    CREATE TYPE dbo.SaleLineType AS TABLE
    (
        MedicineID INT NOT NULL,
        Quantity   INT NOT NULL CHECK (Quantity > 0)
    );
GO

IF TYPE_ID(N'dbo.PurchaseLineType') IS NULL
    CREATE TYPE dbo.PurchaseLineType AS TABLE
    (
        MedicineID INT NOT NULL,
        Quantity   INT NOT NULL CHECK (Quantity > 0),
        UnitPrice  DECIMAL(10,2) NOT NULL CHECK (UnitPrice >= 0)
    );
GO

-- =========================================================
-- 4. usp_CreateSale
--    Business rules, in this order:
--      - the invoice must have lines; the same medicine on several lines is merged
--      - a medicine that requires a prescription needs a prescription that lists it
--      - the medicine must be in this branch's stock, must not be expired, and the quantity must be available
--      - the unit price ALWAYS comes from Inventory.SellingPrice (never from the caller)
--    Stock rows are locked (UPDLOCK) while checking, so two cashiers cannot sell the last unit twice.
--    A rule violation returns @SaleID = NULL and a readable @ErrorMessage.
--    An unexpected error rolls everything back and is re-thrown (the app logs it).
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.usp_CreateSale
    @BranchID       INT,
    @CustomerID     INT = NULL,
    @PrescriptionID INT = NULL,
    @UserID         INT,
    @Lines          dbo.SaleLineType READONLY,
    @SaleID         INT OUTPUT,
    @ErrorMessage   NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @SaleID = NULL;
    SET @ErrorMessage = NULL;

    IF NOT EXISTS (SELECT 1 FROM @Lines)
    BEGIN
        SET @ErrorMessage = N'لا يوجد أصناف في الفاتورة.';
        RETURN;
    END

    -- one row per medicine, even if the caller sent the same medicine on several lines
    DECLARE @Merged TABLE (MedicineID INT NOT NULL PRIMARY KEY, Quantity INT NOT NULL);

    INSERT INTO @Merged (MedicineID, Quantity)
    SELECT MedicineID, SUM(Quantity)
    FROM @Lines
    GROUP BY MedicineID;

    DECLARE @Name  NVARCHAR(150);
    DECLARE @Avail INT;

    -- unknown medicine
    SELECT TOP (1) @Name = CAST(L.MedicineID AS NVARCHAR(20))
    FROM @Merged L
    LEFT JOIN dbo.Medicines M ON M.MedicineID = L.MedicineID
    WHERE M.MedicineID IS NULL;

    IF @Name IS NOT NULL
    BEGIN
        SET @ErrorMessage = N'الدواء (كود ' + @Name + N') غير موجود.';
        RETURN;
    END

    -- prescription rules
    IF @PrescriptionID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Prescriptions WHERE PrescriptionID = @PrescriptionID)
    BEGIN
        SET @ErrorMessage = N'الوصفة المختارة غير موجودة.';
        RETURN;
    END

    SET @Name = NULL;
    SELECT TOP (1) @Name = M.Name
    FROM @Merged L
    JOIN dbo.Medicines M ON M.MedicineID = L.MedicineID
    WHERE M.RequiresPrescription = 1
      AND (   @PrescriptionID IS NULL
           OR NOT EXISTS (SELECT 1
                          FROM dbo.PrescriptionDetails PD
                          WHERE PD.PrescriptionID = @PrescriptionID
                            AND PD.MedicineID = L.MedicineID));

    IF @Name IS NOT NULL
    BEGIN
        SET @ErrorMessage = CASE WHEN @PrescriptionID IS NULL
                THEN N'الدواء "' + @Name + N'" يحتاج وصفة طبية، من فضلك اختر وصفة أولًا.'
                ELSE N'الدواء "' + @Name + N'" غير مذكور في الوصفة المختارة.'
            END;
        RETURN;
    END

    DECLARE @Stock TABLE
    (
        MedicineID  INT NOT NULL PRIMARY KEY,
        InventoryID INT NULL,
        Available   INT NULL,
        Price       DECIMAL(10,2) NULL,
        ExpiryDate  DATE NULL,
        Requested   INT NOT NULL
    );

    DECLARE @Total DECIMAL(12,2);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- read AND lock this branch's stock rows for the requested medicines
        INSERT INTO @Stock (MedicineID, InventoryID, Available, Price, ExpiryDate, Requested)
        SELECT L.MedicineID, I.InventoryID, I.Quantity, I.SellingPrice, I.ExpiryDate, L.Quantity
        FROM @Merged L
        LEFT JOIN dbo.Inventory I WITH (UPDLOCK, ROWLOCK)
               ON I.MedicineID = L.MedicineID
              AND I.BranchID   = @BranchID;

        -- not stocked in this branch
        SET @Name = NULL;
        SELECT TOP (1) @Name = M.Name
        FROM @Stock S
        JOIN dbo.Medicines M ON M.MedicineID = S.MedicineID
        WHERE S.InventoryID IS NULL;

        IF @Name IS NOT NULL
        BEGIN
            SET @ErrorMessage = N'الدواء "' + @Name + N'" غير موجود في مخزون هذا الفرع.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- expired
        SET @Name = NULL;
        SELECT TOP (1) @Name = M.Name
        FROM @Stock S
        JOIN dbo.Medicines M ON M.MedicineID = S.MedicineID
        WHERE S.ExpiryDate IS NOT NULL
          AND S.ExpiryDate < CAST(GETDATE() AS DATE);

        IF @Name IS NOT NULL
        BEGIN
            SET @ErrorMessage = N'الدواء "' + @Name + N'" منتهي الصلاحية ولا يمكن بيعه.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- not enough quantity
        SET @Name = NULL;
        SELECT TOP (1) @Name = M.Name, @Avail = S.Available
        FROM @Stock S
        JOIN dbo.Medicines M ON M.MedicineID = S.MedicineID
        WHERE S.Available < S.Requested;

        IF @Name IS NOT NULL
        BEGIN
            SET @ErrorMessage = N'الكمية المطلوبة من الدواء "' + @Name
                              + N'" غير متوفرة (المتاح: ' + CAST(@Avail AS NVARCHAR(12)) + N').';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        SELECT @Total = SUM(Requested * Price) FROM @Stock;

        INSERT INTO dbo.Sales (BranchID, CustomerID, PrescriptionID, UserID, SaleDate, TotalAmount)
        VALUES (@BranchID, @CustomerID, @PrescriptionID, @UserID, GETDATE(), @Total);

        SET @SaleID = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.SaleDetails (SaleID, MedicineID, Quantity, UnitPrice)
        SELECT @SaleID, MedicineID, Requested, Price
        FROM @Stock;

        UPDATE I
        SET I.Quantity = I.Quantity - S.Requested
        FROM dbo.Inventory I
        JOIN @Stock S ON S.InventoryID = I.InventoryID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @SaleID = NULL;
        THROW;
    END CATCH
END
GO

-- =========================================================
-- 5. usp_CreatePurchase
--    Saves the purchase invoice and its lines, then adds the quantities to this branch's stock
--    (creating the stock row the first time the branch buys a medicine; its SellingPrice starts
--    at the purchase price and is meant to be adjusted from the Inventory screen).
-- =========================================================
CREATE OR ALTER PROCEDURE dbo.usp_CreatePurchase
    @SupplierID   INT,
    @BranchID     INT,
    @Lines        dbo.PurchaseLineType READONLY,
    @PurchaseID   INT OUTPUT,
    @ErrorMessage NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @PurchaseID = NULL;
    SET @ErrorMessage = NULL;

    IF NOT EXISTS (SELECT 1 FROM @Lines)
    BEGIN
        SET @ErrorMessage = N'لا يوجد أصناف في فاتورة الشراء.';
        RETURN;
    END

    DECLARE @Name NVARCHAR(20);

    SELECT TOP (1) @Name = CAST(L.MedicineID AS NVARCHAR(20))
    FROM @Lines L
    LEFT JOIN dbo.Medicines M ON M.MedicineID = L.MedicineID
    WHERE M.MedicineID IS NULL;

    IF @Name IS NOT NULL
    BEGIN
        SET @ErrorMessage = N'الدواء (كود ' + @Name + N') غير موجود.';
        RETURN;
    END

    -- per-medicine totals for the stock update (weighted-average price if a medicine is repeated)
    DECLARE @Merged TABLE
    (
        MedicineID INT NOT NULL PRIMARY KEY,
        Quantity   INT NOT NULL,
        UnitPrice  DECIMAL(10,2) NOT NULL
    );

    INSERT INTO @Merged (MedicineID, Quantity, UnitPrice)
    SELECT MedicineID,
           SUM(Quantity),
           CAST(SUM(Quantity * UnitPrice) / SUM(Quantity) AS DECIMAL(10,2))
    FROM @Lines
    GROUP BY MedicineID;

    DECLARE @Total DECIMAL(12,2);

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @Total = SUM(Quantity * UnitPrice) FROM @Lines;

        INSERT INTO dbo.Purchases (SupplierID, BranchID, PurchaseDate, TotalAmount)
        VALUES (@SupplierID, @BranchID, GETDATE(), @Total);

        SET @PurchaseID = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.PurchaseDetails (PurchaseID, MedicineID, Quantity, UnitPrice)
        SELECT @PurchaseID, MedicineID, Quantity, UnitPrice
        FROM @Lines;

        -- medicines this branch already stocks: add the quantity
        UPDATE I
        SET I.Quantity = I.Quantity + M.Quantity
        FROM dbo.Inventory I WITH (UPDLOCK, ROWLOCK)
        JOIN @Merged M ON M.MedicineID = I.MedicineID
        WHERE I.BranchID = @BranchID;

        -- medicines this branch has never stocked: create the stock row
        INSERT INTO dbo.Inventory (BranchID, MedicineID, Quantity, SellingPrice, ExpiryDate)
        SELECT @BranchID, M.MedicineID, M.Quantity, M.UnitPrice, NULL
        FROM @Merged M
        WHERE NOT EXISTS (SELECT 1
                          FROM dbo.Inventory I WITH (UPDLOCK, HOLDLOCK)
                          WHERE I.BranchID = @BranchID
                            AND I.MedicineID = M.MedicineID);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @PurchaseID = NULL;
        THROW;
    END CATCH
END
GO

PRINT N'T-SQL logic installed: constraints, indexes, usp_CreateSale, usp_CreatePurchase.';
