-- =========================================================
-- PharmacyDB - Sample data (run AFTER PharmacyDB.sql and 02_TSQL_Logic.sql)
--
--   * 5 branches (the main one + 4 more)
--   * 9 medicine categories
--   * 37 medicines, stocked across ALL branches (different quantities per branch,
--     a few low-stock items and a few items that expire within a month, so the
--     dashboard alerts have something to show)
--   * suppliers (+ which supplier sells which medicine), customers, doctors
--   * users: an Admin, and a Pharmacist + a Cashier for every branch
--
-- Safe to run more than once: every row is inserted only if it is not already there
-- (branches / categories / medicines / suppliers by name, users by username,
-- stock by branch + medicine). Nothing existing is changed or deleted.
--
-- DEFAULT PASSWORDS (change them before handing the system to the client):
--     admin              Admin@123
--     pharmacist<N>      Pharm@123      (N = the branch number, e.g. pharmacist2)
--     cashier<N>         Cash@123       (e.g. cashier2)
-- =========================================================

USE PharmacyDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    -- -----------------------------------------------------
    -- 1. Branches
    -- -----------------------------------------------------
    INSERT INTO dbo.Branches (BranchName, Address, Phone)
    SELECT V.BranchName, V.Address, V.Phone
    FROM (VALUES
        (N'الفرع الرئيسي',   N'شارع الجمهورية، وسط البلد، القاهرة',   N'0100000000'),
        (N'فرع مدينة نصر',   N'شارع عباس العقاد، مدينة نصر، القاهرة', N'01012345678'),
        (N'فرع الإسكندرية',  N'شارع الكورنيش، سموحة، الإسكندرية',     N'01123456789'),
        (N'فرع الجيزة',      N'شارع الهرم، الجيزة',                    N'01234567890'),
        (N'فرع المنصورة',    N'شارع الجيش، المنصورة، الدقهلية',        N'01098765432')
    ) AS V(BranchName, Address, Phone)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Branches B WHERE B.BranchName = V.BranchName);

    -- -----------------------------------------------------
    -- 2. Categories
    -- -----------------------------------------------------
    INSERT INTO dbo.Categories (CategoryName)
    SELECT V.CategoryName
    FROM (VALUES
        (N'مسكنات'),
        (N'مضادات حيوية'),
        (N'فيتامينات'),
        (N'أدوية القلب والضغط'),
        (N'السكر والغدد'),
        (N'الجهاز الهضمي'),
        (N'الحساسية والجهاز التنفسي'),
        (N'العناية بالبشرة والجروح'),
        (N'أدوية الأطفال')
    ) AS V(CategoryName)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories C WHERE C.CategoryName = V.CategoryName);

    -- -----------------------------------------------------
    -- 3. Medicines (Rx = 1 means "requires a prescription"; Price = selling price)
    -- -----------------------------------------------------
    DECLARE @Seed TABLE
    (
        Name        NVARCHAR(150) NOT NULL,
        Cat         NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        Unit        NVARCHAR(50)  NULL,
        Rx          BIT           NOT NULL,
        Price       DECIMAL(10,2) NOT NULL
    );

    INSERT INTO @Seed (Name, Cat, Description, Unit, Rx, Price) VALUES
    -- مسكنات
    (N'بانادول إكسترا 500 مجم',      N'مسكنات', N'مسكن وخافض للحرارة',                  N'شريط',   0,  42.00),
    (N'بروفين 400 مجم',              N'مسكنات', N'مسكن ومضاد للالتهاب',                 N'شريط',   0,  38.00),
    (N'كتافلام 50 مجم',              N'مسكنات', N'مسكن للآلام والالتهابات',             N'شريط',   0,  55.00),
    (N'فولتارين جل',                 N'مسكنات', N'جل موضعي لآلام العضلات والمفاصل',     N'أنبوبة', 0,  68.00),
    -- مضادات حيوية
    (N'أوجمنتين 1 جم',               N'مضادات حيوية', N'مضاد حيوي واسع المدى',          N'علبة',   1, 165.00),
    (N'أموكسيل 500 مجم',             N'مضادات حيوية', N'مضاد حيوي للالتهابات البكتيرية', N'علبة',   1,  72.00),
    (N'زيثروماكس 500 مجم',           N'مضادات حيوية', N'مضاد حيوي للجهاز التنفسي',       N'علبة',   1, 118.00),
    (N'سيبروفلوكساسين 500 مجم',      N'مضادات حيوية', N'مضاد حيوي للمسالك البولية',      N'علبة',   1,  64.00),
    (N'فلاجيل 500 مجم',              N'مضادات حيوية', N'مضاد للطفيليات والبكتيريا اللاهوائية', N'علبة', 1, 35.00),
    -- فيتامينات
    (N'فيتامين سي 1000 مجم فوار',    N'فيتامينات', N'مقوي للمناعة',                     N'علبة',   0,  85.00),
    (N'فيتامين د3 50000 وحدة',       N'فيتامينات', N'تعويض نقص فيتامين د',              N'علبة',   0,  60.00),
    (N'نيوروبيون',                   N'فيتامينات', N'فيتامينات ب للأعصاب',              N'علبة',   0,  92.00),
    (N'سنتروم',                      N'فيتامينات', N'فيتامينات ومعادن متعددة',          N'علبة',   0, 250.00),
    (N'أوميجا 3',                    N'فيتامينات', N'مكمل غذائي لصحة القلب',            N'علبة',   0, 210.00),
    -- القلب والضغط
    (N'كونكور 5 مجم',                N'أدوية القلب والضغط', N'خافض للضغط ومنظم للقلب',  N'علبة',   1,  96.00),
    (N'أملور 5 مجم',                 N'أدوية القلب والضغط', N'خافض لضغط الدم',           N'علبة',   1,  58.00),
    (N'كوزار 50 مجم',                N'أدوية القلب والضغط', N'خافض لضغط الدم',           N'علبة',   1,  88.00),
    (N'أسبوكارد 75 مجم',             N'أدوية القلب والضغط', N'أسبرين لسيولة الدم',       N'علبة',   0,  30.00),
    -- السكر
    (N'جلوكوفاج 500 مجم',            N'السكر والغدد', N'خافض لسكر الدم',                N'علبة',   1,  45.00),
    (N'أماريل 2 مجم',                N'السكر والغدد', N'خافض لسكر الدم',                N'علبة',   1, 110.00),
    -- الجهاز الهضمي
    (N'أنتينال',                     N'الجهاز الهضمي', N'مطهر معوي',                    N'علبة',   0,  40.00),
    (N'بسكوبان',                     N'الجهاز الهضمي', N'مضاد للمغص والتقلصات',         N'علبة',   0,  45.00),
    (N'موتيليوم 10 مجم',             N'الجهاز الهضمي', N'للغثيان وعسر الهضم',           N'علبة',   0,  52.00),
    (N'نكسيوم 40 مجم',               N'الجهاز الهضمي', N'لعلاج الحموضة والقرحة',        N'علبة',   1, 190.00),
    (N'كونترولوك 40 مجم',            N'الجهاز الهضمي', N'لعلاج الحموضة والقرحة',        N'علبة',   1, 150.00),
    -- الحساسية والتنفس
    (N'كلاريتين 10 مجم',             N'الحساسية والجهاز التنفسي', N'مضاد للحساسية',      N'علبة',   0,  48.00),
    (N'زيرتك 10 مجم',                N'الحساسية والجهاز التنفسي', N'مضاد للحساسية',      N'علبة',   0,  55.00),
    (N'فنتولين بخاخ',                N'الحساسية والجهاز التنفسي', N'موسع للشعب الهوائية', N'بخاخ',  1,  62.00),
    (N'أوتريفين نقط أنف',            N'الحساسية والجهاز التنفسي', N'مزيل لاحتقان الأنف',  N'زجاجة', 0,  30.00),
    -- البشرة والجروح
    (N'بيتادين محلول',               N'العناية بالبشرة والجروح', N'مطهر للجروح',         N'زجاجة',  0,  35.00),
    (N'بيبانثين كريم',               N'العناية بالبشرة والجروح', N'كريم لترطيب وعلاج الجلد', N'أنبوبة', 0, 95.00),
    -- الأطفال
    (N'بروفين شراب للأطفال',         N'أدوية الأطفال', N'خافض حرارة ومسكن للأطفال',     N'زجاجة',  0,  40.00),
    (N'بانادول أطفال شراب',          N'أدوية الأطفال', N'خافض حرارة للأطفال',           N'زجاجة',  0,  33.00),
    (N'أوجمنتين 156 معلق للأطفال',   N'أدوية الأطفال', N'مضاد حيوي للأطفال',            N'زجاجة',  1,  85.00),
    (N'فيتامين د نقط للأطفال',       N'أدوية الأطفال', N'مكمل فيتامين د للرضع',         N'زجاجة',  0,  45.00),
    (N'سيتال شراب',                  N'أدوية الأطفال', N'مسكن وخافض حرارة',             N'زجاجة',  0,  38.00),
    (N'ديكونجستال شراب',             N'الحساسية والجهاز التنفسي', N'لاحتقان الصدر والكحة', N'زجاجة', 0, 47.00);

    INSERT INTO dbo.Medicines (Name, CategoryID, Description, Unit, RequiresPrescription)
    SELECT S.Name, C.CategoryID, S.Description, S.Unit, S.Rx
    FROM @Seed S
    JOIN dbo.Categories C ON C.CategoryName = S.Cat
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Medicines M WHERE M.Name = S.Name);

    -- -----------------------------------------------------
    -- 4. Stock in EVERY branch
    --    (the main branch stocks everything; the others ~80% of the list, so branches differ)
    --    Quantity / expiry are derived from the ids, so re-running gives the same result:
    --      about 1 in 13 rows is low stock (3-8 units),
    --      about 1 in 17 rows expires within 10-25 days, the rest expire in 6-23 months.
    -- -----------------------------------------------------
    INSERT INTO dbo.Inventory (BranchID, MedicineID, Quantity, SellingPrice, ExpiryDate)
    SELECT B.BranchID,
           M.MedicineID,
           CAST(CASE WHEN X.h % 13 = 0 THEN 3 + X.h % 6 ELSE 20 + X.h % 80 END AS INT),
           S.Price,
           CASE WHEN X.h % 17 = 0
                THEN CAST(DATEADD(DAY,   CAST(10 + X.h % 16 AS INT), GETDATE()) AS DATE)
                ELSE CAST(DATEADD(MONTH, CAST(6  + X.h % 18 AS INT), GETDATE()) AS DATE)
           END
    FROM dbo.Branches B
    CROSS JOIN @Seed S
    JOIN dbo.Medicines M ON M.Name = S.Name
    CROSS APPLY (SELECT ABS(CAST(CHECKSUM(B.BranchID, M.MedicineID) AS BIGINT)) AS h) X
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Inventory I WHERE I.BranchID = B.BranchID AND I.MedicineID = M.MedicineID)
      AND (B.BranchID = (SELECT MIN(BranchID) FROM dbo.Branches) OR X.h % 10 < 8);

    -- -----------------------------------------------------
    -- 5. Suppliers, and which supplier sells which medicine
    -- -----------------------------------------------------
    INSERT INTO dbo.Suppliers (Name, Phone, Email, Address)
    SELECT V.Name, V.Phone, V.Email, V.Address
    FROM (VALUES
        (N'الشركة المتحدة للأدوية',  N'0223456789', N'info@united-pharma.example', N'العباسية، القاهرة'),
        (N'ابن سينا للتوزيع الدوائي', N'0233344455', N'sales@ibnsina.example',      N'الدقي، الجيزة'),
        (N'شركة النيل للأدوية',      N'0224455667', N'contact@nile-pharma.example', N'مدينة نصر، القاهرة'),
        (N'الأمل للمستلزمات الطبية', N'0334455667', N'orders@alamal-med.example',   N'سموحة، الإسكندرية')
    ) AS V(Name, Phone, Email, Address)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Suppliers Sp WHERE Sp.Name = V.Name);

    INSERT INTO dbo.MedicineSuppliers (MedicineID, SupplierID, PurchasePrice)
    SELECT M.MedicineID, Sp.SupplierID, CAST(S.Price * 0.75 AS DECIMAL(10,2))
    FROM @Seed S
    JOIN dbo.Medicines M ON M.Name = S.Name
    CROSS JOIN dbo.Suppliers Sp
    WHERE Sp.Name IN (N'الشركة المتحدة للأدوية', N'ابن سينا للتوزيع الدوائي', N'شركة النيل للأدوية', N'الأمل للمستلزمات الطبية')
      AND (Sp.Name = N'الشركة المتحدة للأدوية'
           OR ABS(CAST(CHECKSUM(M.MedicineID, Sp.SupplierID) AS BIGINT)) % 3 <> 0)
      AND NOT EXISTS (SELECT 1 FROM dbo.MedicineSuppliers MS
                      WHERE MS.MedicineID = M.MedicineID AND MS.SupplierID = Sp.SupplierID);

    -- -----------------------------------------------------
    -- 6. Customers and doctors
    -- -----------------------------------------------------
    INSERT INTO dbo.Customers (Name, Phone)
    SELECT V.Name, V.Phone
    FROM (VALUES
        (N'أحمد محمود علي',     N'01011112222'),
        (N'منى السيد إبراهيم',  N'01122223333'),
        (N'محمد حسن عبد الله',  N'01233334444'),
        (N'سارة خالد يوسف',     N'01544445555'),
        (N'عمر فتحي سليمان',    N'01055556666'),
        (N'هدى عبد الرحمن',     N'01166667777'),
        (N'ياسر جمال الدين',    N'01277778888'),
        (N'نهى مصطفى كامل',     N'01588889999')
    ) AS V(Name, Phone)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Customers C WHERE C.Name = V.Name);

    INSERT INTO dbo.Doctors (Name, Specialization, Phone)
    SELECT V.Name, V.Specialization, V.Phone
    FROM (VALUES
        (N'د. هشام عبد العزيز', N'باطنة',          N'0223001001'),
        (N'د. رانيا سمير',      N'أطفال',          N'0223001002'),
        (N'د. طارق منصور',      N'عظام',           N'0223001003'),
        (N'د. دينا الشريف',     N'جلدية',          N'0223001004'),
        (N'د. خالد رمضان',      N'قلب وأوعية دموية', N'0223001005'),
        (N'د. أميرة فؤاد',      N'نساء وتوليد',    N'0223001006')
    ) AS V(Name, Specialization, Phone)
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Doctors D WHERE D.Name = V.Name);

    -- -----------------------------------------------------
    -- 7. Users (SHA-256 hex hashes, same format the application uses)
    -- -----------------------------------------------------
    INSERT INTO dbo.Users (Username, PasswordHash, Email, RoleID, BranchID, IsActive, CreatedDate)
    SELECT N'admin',
           N'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7',   -- Admin@123
           N'admin@pharmacy.com',
           (SELECT RoleID FROM dbo.Roles WHERE RoleName = N'Admin'),
           (SELECT MIN(BranchID) FROM dbo.Branches),
           1, GETDATE()
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin');

    INSERT INTO dbo.Users (Username, PasswordHash, Email, RoleID, BranchID, IsActive, CreatedDate)
    SELECT N'pharmacist' + CAST(B.BranchID AS NVARCHAR(10)),
           N'1c283856d6b1322d27cc44e4b00a87622318762221af41a07db59f28f34ee511',   -- Pharm@123
           NULL,
           (SELECT RoleID FROM dbo.Roles WHERE RoleName = N'Pharmacist'),
           B.BranchID, 1, GETDATE()
    FROM dbo.Branches B
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Users U WHERE U.Username = N'pharmacist' + CAST(B.BranchID AS NVARCHAR(10)));

    INSERT INTO dbo.Users (Username, PasswordHash, Email, RoleID, BranchID, IsActive, CreatedDate)
    SELECT N'cashier' + CAST(B.BranchID AS NVARCHAR(10)),
           N'47da0025ed1c462ef0ca564dd069883e6d2a94cab3c5b3859b01f3ba0bf0b7ea',   -- Cash@123
           NULL,
           (SELECT RoleID FROM dbo.Roles WHERE RoleName = N'Cashier'),
           B.BranchID, 1, GETDATE()
    FROM dbo.Branches B
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Users U WHERE U.Username = N'cashier' + CAST(B.BranchID AS NVARCHAR(10)));

    COMMIT TRANSACTION;

    PRINT N'Sample data loaded.';
    PRINT N'Branches: '   + CAST((SELECT COUNT(*) FROM dbo.Branches)   AS NVARCHAR(10));
    PRINT N'Medicines: '  + CAST((SELECT COUNT(*) FROM dbo.Medicines)  AS NVARCHAR(10));
    PRINT N'Stock rows: ' + CAST((SELECT COUNT(*) FROM dbo.Inventory)  AS NVARCHAR(10));
    PRINT N'Users: '      + CAST((SELECT COUNT(*) FROM dbo.Users)      AS NVARCHAR(10));
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH
GO
