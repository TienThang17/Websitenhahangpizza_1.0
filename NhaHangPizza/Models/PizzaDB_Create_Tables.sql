/* =====================================================================
   HE THONG QUAN LY DAT VA BAN BANH PIZZA - Nhom 69 - De tai 19
   SQL Server script: tao database, bang, khoa, rang buoc, du lieu mau
   Luu y: doi ten database (PizzaDB) neu ban da co database rieng.
   Script co the chay lai nhieu lan (chi tao khi chua ton tai).
   ===================================================================== */

IF DB_ID(N'PizzaDB') IS NULL
    CREATE DATABASE PizzaDB;
GO
USE PizzaDB;
GO

/* ---------------------------------------------------------------------
   1. BANG DANH MUC / TRA CUU
   --------------------------------------------------------------------- */

-- Ban an (TrangThaiBan: 0 = Trong, 1 = Dang dung, 2 = Cho thanh toan)
IF OBJECT_ID(N'dbo.BanAn') IS NULL
CREATE TABLE dbo.BanAn (
    SoBan        INT NOT NULL CONSTRAINT PK_BanAn PRIMARY KEY,
    TrangThaiBan INT NOT NULL CONSTRAINT DF_BanAn_TrangThai DEFAULT 0,
    CONSTRAINT CK_BanAn_TrangThai CHECK (TrangThaiBan IN (0,1,2))
);
GO

-- Kich thuoc banh: nho / trung / lon. HeSoGia nhan voi gia co ban cua pizza
IF OBJECT_ID(N'dbo.KichThuoc') IS NULL
CREATE TABLE dbo.KichThuoc (
    MaKichThuoc  INT NOT NULL CONSTRAINT PK_KichThuoc PRIMARY KEY,
    TenKichThuoc NVARCHAR(20) NOT NULL CONSTRAINT UQ_KichThuoc_Ten UNIQUE,
    HeSoGia      DECIMAL(4,2) NOT NULL,
    CONSTRAINT CK_KichThuoc_HeSo CHECK (HeSoGia > 0)
);
GO

-- Vo banh: don / day. PhuThu cong them vao gia moi banh
IF OBJECT_ID(N'dbo.VoBanh') IS NULL
CREATE TABLE dbo.VoBanh (
    MaVoBanh  INT NOT NULL CONSTRAINT PK_VoBanh PRIMARY KEY,
    TenVoBanh NVARCHAR(20) NOT NULL CONSTRAINT UQ_VoBanh_Ten UNIQUE,
    PhuThu    DECIMAL(18,0) NOT NULL CONSTRAINT DF_VoBanh_PhuThu DEFAULT 0,
    CONSTRAINT CK_VoBanh_PhuThu CHECK (PhuThu >= 0)
);
GO

-- Cac buoc quy trinh chuan bi va lam banh (dung cho man hinh theo doi)
IF OBJECT_ID(N'dbo.BuocLamBanh') IS NULL
CREATE TABLE dbo.BuocLamBanh (
    MaBuoc  INT NOT NULL CONSTRAINT PK_BuocLamBanh PRIMARY KEY,
    TenBuoc NVARCHAR(100) NOT NULL,
    ThuTu   INT NOT NULL CONSTRAINT UQ_BuocLamBanh_ThuTu UNIQUE
);
GO

/* ---------------------------------------------------------------------
   2. SAN PHAM: PIZZA, THANH PHAN, NUOC UONG
   --------------------------------------------------------------------- */

-- Pizza (lop cha). LoaiPizza: 1 = Lam san, 2 = Tu lam
-- DonGia la gia co ban cho co NHO; gia thuc te = DonGia * HeSoGia + PhuThu vo (+ topping neu tu lam)
IF OBJECT_ID(N'dbo.Pizza') IS NULL
CREATE TABLE dbo.Pizza (
    MaPizza   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pizza PRIMARY KEY,
    TenPizza  NVARCHAR(100) NOT NULL,
    LoaiPizza INT NOT NULL,
    DonGia    DECIMAL(18,0) NOT NULL,
    HinhAnh   NVARCHAR(255) NULL,
    TrangThai BIT NOT NULL CONSTRAINT DF_Pizza_TrangThai DEFAULT 1,  -- 1 = dang ban
    CONSTRAINT CK_Pizza_Loai CHECK (LoaiPizza IN (1,2)),
    CONSTRAINT CK_Pizza_DonGia CHECK (DonGia >= 0)
);
GO

-- Pizza lam san (lop con): thanh phan co dinh, khong thay doi
IF OBJECT_ID(N'dbo.PizzaLamSan') IS NULL
CREATE TABLE dbo.PizzaLamSan (
    MaPizza INT NOT NULL CONSTRAINT PK_PizzaLamSan PRIMARY KEY,
    MoTa    NVARCHAR(MAX) NULL,
    CONSTRAINT FK_PizzaLamSan_Pizza FOREIGN KEY (MaPizza) REFERENCES dbo.Pizza(MaPizza) ON DELETE CASCADE
);
GO

-- Pizza tu lam (lop con): luon co sot ca chua, khach chon them thanh phan
IF OBJECT_ID(N'dbo.PizzaTuLam') IS NULL
CREATE TABLE dbo.PizzaTuLam (
    MaPizza       INT NOT NULL CONSTRAINT PK_PizzaTuLam PRIMARY KEY,
    CoSotCaChua   BIT NOT NULL CONSTRAINT DF_PizzaTuLam_Sot DEFAULT 1,
    CONSTRAINT FK_PizzaTuLam_Pizza FOREIGN KEY (MaPizza) REFERENCES dbo.Pizza(MaPizza) ON DELETE CASCADE
);
GO

-- Thanh phan / topping. TrangThai: 1 = con, 0 = tam het
IF OBJECT_ID(N'dbo.ThanhPhan') IS NULL
CREATE TABLE dbo.ThanhPhan (
    MaTP      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThanhPhan PRIMARY KEY,
    TenTP     NVARCHAR(100) NOT NULL CONSTRAINT UQ_ThanhPhan_Ten UNIQUE,
    GiaTien   DECIMAL(18,0) NOT NULL CONSTRAINT DF_ThanhPhan_Gia DEFAULT 0,
    TrangThai INT NOT NULL CONSTRAINT DF_ThanhPhan_TrangThai DEFAULT 1,
    CONSTRAINT CK_ThanhPhan_Gia CHECK (GiaTien >= 0),
    CONSTRAINT CK_ThanhPhan_TrangThai CHECK (TrangThai IN (0,1))
);
GO

-- Thanh phan co dinh cua tung pizza lam san (quan he N-N)
IF OBJECT_ID(N'dbo.ThanhPhanPizzaLamSan') IS NULL
CREATE TABLE dbo.ThanhPhanPizzaLamSan (
    MaPizza INT NOT NULL,
    MaTP    INT NOT NULL,
    CONSTRAINT PK_ThanhPhanPizzaLamSan PRIMARY KEY (MaPizza, MaTP),
    CONSTRAINT FK_TPPLS_PizzaLamSan FOREIGN KEY (MaPizza) REFERENCES dbo.PizzaLamSan(MaPizza) ON DELETE CASCADE,
    CONSTRAINT FK_TPPLS_ThanhPhan   FOREIGN KEY (MaTP)    REFERENCES dbo.ThanhPhan(MaTP)
);
GO

-- Nuoc uong (ap dung cho moi co banh)
IF OBJECT_ID(N'dbo.NuocUong') IS NULL
CREATE TABLE dbo.NuocUong (
    MaNuoc     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NuocUong PRIMARY KEY,
    TenNuoc    NVARCHAR(100) NOT NULL CONSTRAINT UQ_NuocUong_Ten UNIQUE,
    DonGia     DECIMAL(18,0) NOT NULL,
    SoLuongTon INT NOT NULL CONSTRAINT DF_NuocUong_Ton DEFAULT 0,
    HinhAnh    NVARCHAR(255) NULL,
    CONSTRAINT CK_NuocUong_Gia CHECK (DonGia >= 0),
    CONSTRAINT CK_NuocUong_Ton CHECK (SoLuongTon >= 0)
);
GO

/* ---------------------------------------------------------------------
   3. DON HANG
   --------------------------------------------------------------------- */

-- Don hang. TrangThaiDon tham chieu BuocLamBanh (1..6). TrangThaiThanhToan: 0 = chua, 1 = da thanh toan
IF OBJECT_ID(N'dbo.DonHang') IS NULL
CREATE TABLE dbo.DonHang (
    MaDonHang          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DonHang PRIMARY KEY,
    SoBan              INT NOT NULL,
    NgayTao            DATETIME NOT NULL CONSTRAINT DF_DonHang_NgayTao DEFAULT GETDATE(),
    TongTien           DECIMAL(18,0) NOT NULL CONSTRAINT DF_DonHang_TongTien DEFAULT 0,
    TrangThaiDon       INT NOT NULL CONSTRAINT DF_DonHang_TrangThai DEFAULT 1,
    TrangThaiThanhToan BIT NOT NULL CONSTRAINT DF_DonHang_TTTT DEFAULT 0,
    CONSTRAINT FK_DonHang_BanAn FOREIGN KEY (SoBan) REFERENCES dbo.BanAn(SoBan),
    CONSTRAINT FK_DonHang_Buoc  FOREIGN KEY (TrangThaiDon) REFERENCES dbo.BuocLamBanh(MaBuoc),
    CONSTRAINT CK_DonHang_TongTien CHECK (TongTien >= 0)
);
GO

-- Chi tiet don hang: moi dong la 1 pizza HOAC 1 nuoc uong
-- Pizza: bat buoc co MaKichThuoc + MaVoBanh. Nuoc uong: khong co kich thuoc/vo.
-- DonGia la gia da chot tai thoi diem dat (da gom he so size, phu thu vo, topping)
IF OBJECT_ID(N'dbo.ChiTietDonHang') IS NULL
CREATE TABLE dbo.ChiTietDonHang (
    MaChiTiet   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ChiTietDonHang PRIMARY KEY,
    MaDonHang   INT NOT NULL,
    MaPizza     INT NULL,
    MaNuoc      INT NULL,
    MaKichThuoc INT NULL,
    MaVoBanh    INT NULL,
    SoLuong     INT NOT NULL,
    DonGia      DECIMAL(18,0) NOT NULL,
    ThanhTien   AS (SoLuong * DonGia) PERSISTED,
    GhiChu      NVARCHAR(MAX) NULL,
    CONSTRAINT FK_CTDH_DonHang  FOREIGN KEY (MaDonHang)   REFERENCES dbo.DonHang(MaDonHang) ON DELETE CASCADE,
    CONSTRAINT FK_CTDH_Pizza    FOREIGN KEY (MaPizza)     REFERENCES dbo.Pizza(MaPizza),
    CONSTRAINT FK_CTDH_NuocUong FOREIGN KEY (MaNuoc)      REFERENCES dbo.NuocUong(MaNuoc),
    CONSTRAINT FK_CTDH_KichThuoc FOREIGN KEY (MaKichThuoc) REFERENCES dbo.KichThuoc(MaKichThuoc),
    CONSTRAINT FK_CTDH_VoBanh   FOREIGN KEY (MaVoBanh)    REFERENCES dbo.VoBanh(MaVoBanh),
    CONSTRAINT CK_CTDH_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_CTDH_DonGia  CHECK (DonGia >= 0),
    CONSTRAINT CK_CTDH_PizzaHoacNuoc CHECK (
        (MaPizza IS NOT NULL AND MaNuoc IS NULL AND MaKichThuoc IS NOT NULL AND MaVoBanh IS NOT NULL)
        OR
        (MaPizza IS NULL AND MaNuoc IS NOT NULL AND MaKichThuoc IS NULL AND MaVoBanh IS NULL)
    )
);
GO

-- Thanh phan khach chon them cho dong pizza tu lam (GiaTien chot tai thoi diem dat)
IF OBJECT_ID(N'dbo.ChiTietTuLam_ThanhPhan') IS NULL
CREATE TABLE dbo.ChiTietTuLam_ThanhPhan (
    MaChiTiet INT NOT NULL,
    MaTP      INT NOT NULL,
    GiaTien   DECIMAL(18,0) NOT NULL,
    CONSTRAINT PK_CTTL_TP PRIMARY KEY (MaChiTiet, MaTP),
    CONSTRAINT FK_CTTL_ChiTiet   FOREIGN KEY (MaChiTiet) REFERENCES dbo.ChiTietDonHang(MaChiTiet) ON DELETE CASCADE,
    CONSTRAINT FK_CTTL_ThanhPhan FOREIGN KEY (MaTP)      REFERENCES dbo.ThanhPhan(MaTP)
);
GO

/* ---------------------------------------------------------------------
   4. THANH TOAN
   --------------------------------------------------------------------- */

-- Cong thanh toan the tin dung (APIKey: trong thuc te nen luu o noi an toan, khong luu ro trong DB)
IF OBJECT_ID(N'dbo.CongThanhToan') IS NULL
CREATE TABLE dbo.CongThanhToan (
    MaCong     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CongThanhToan PRIMARY KEY,
    TenCong    NVARCHAR(100) NOT NULL,
    GatewayURL NVARCHAR(MAX) NOT NULL,
    APIKey     NVARCHAR(MAX) NULL
);
GO

-- Thanh toan (lop cha). PhuongThuc: 1 = Tien mat, 2 = The tin dung
-- TrangThai: 0 = Cho xu ly, 1 = Thanh cong, 2 = That bai
IF OBJECT_ID(N'dbo.ThanhToan') IS NULL
CREATE TABLE dbo.ThanhToan (
    MaThanhToan   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThanhToan PRIMARY KEY,
    MaDonHang     INT NOT NULL,
    PhuongThuc    INT NOT NULL,
    SoTien        DECIMAL(18,0) NOT NULL,
    NgayThanhToan DATETIME NULL,
    TrangThai     INT NOT NULL CONSTRAINT DF_ThanhToan_TrangThai DEFAULT 0,
    CONSTRAINT FK_ThanhToan_DonHang FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang),
    CONSTRAINT CK_ThanhToan_PhuongThuc CHECK (PhuongThuc IN (1,2)),
    CONSTRAINT CK_ThanhToan_TrangThai CHECK (TrangThai IN (0,1,2)),
    CONSTRAINT CK_ThanhToan_SoTien CHECK (SoTien >= 0)
);
GO

-- Thanh toan tien mat (lop con)
IF OBJECT_ID(N'dbo.ThanhToanTienMat') IS NULL
CREATE TABLE dbo.ThanhToanTienMat (
    MaThanhToan  INT NOT NULL CONSTRAINT PK_ThanhToanTienMat PRIMARY KEY,
    TienKhachDua DECIMAL(18,0) NULL,
    TienThua     DECIMAL(18,0) NULL,
    CONSTRAINT FK_TTTM_ThanhToan FOREIGN KEY (MaThanhToan) REFERENCES dbo.ThanhToan(MaThanhToan) ON DELETE CASCADE
);
GO

-- Thanh toan the tin dung (lop con)
-- BAO MAT: SoThe chi luu dang che (vd: ************1234), KHONG luu so the day du va KHONG luu CVV
IF OBJECT_ID(N'dbo.ThanhToanThe') IS NULL
CREATE TABLE dbo.ThanhToanThe (
    MaThanhToan INT NOT NULL CONSTRAINT PK_ThanhToanThe PRIMARY KEY,
    MaCong      INT NULL,
    SoThe       CHAR(20) NOT NULL,
    TenChuThe   NVARCHAR(100) NOT NULL,
    NgayHetHan  DATE NOT NULL,
    MaGiaoDich  NVARCHAR(50) NULL,
    CONSTRAINT FK_TTThe_ThanhToan FOREIGN KEY (MaThanhToan) REFERENCES dbo.ThanhToan(MaThanhToan) ON DELETE CASCADE,
    CONSTRAINT FK_TTThe_Cong      FOREIGN KEY (MaCong)      REFERENCES dbo.CongThanhToan(MaCong)
);
GO

/* ---------------------------------------------------------------------
   5. INDEX HO TRO TRUY VAN
   --------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DonHang_SoBan')
    CREATE INDEX IX_DonHang_SoBan ON dbo.DonHang(SoBan, TrangThaiThanhToan);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CTDH_MaDonHang')
    CREATE INDEX IX_CTDH_MaDonHang ON dbo.ChiTietDonHang(MaDonHang);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ThanhToan_MaDonHang')
    CREATE INDEX IX_ThanhToan_MaDonHang ON dbo.ThanhToan(MaDonHang);
GO

/* =====================================================================
   DU LIEU MAU (chi chen khi bang con rong)
   ===================================================================== */

-- Ban an: 10 ban
IF NOT EXISTS (SELECT 1 FROM dbo.BanAn)
INSERT dbo.BanAn (SoBan, TrangThaiBan)
VALUES (1,0),(2,0),(3,0),(4,0),(5,0),(6,0),(7,0),(8,0),(9,0),(10,0);
GO

-- Kich thuoc
IF NOT EXISTS (SELECT 1 FROM dbo.KichThuoc)
INSERT dbo.KichThuoc (MaKichThuoc, TenKichThuoc, HeSoGia)
VALUES (1, N'Cỡ nhỏ', 1.00), (2, N'Cỡ trung', 1.30), (3, N'Cỡ lớn', 1.60);
GO

-- Vo banh
IF NOT EXISTS (SELECT 1 FROM dbo.VoBanh)
INSERT dbo.VoBanh (MaVoBanh, TenVoBanh, PhuThu)
VALUES (1, N'Vỏ dòn', 0), (2, N'Vỏ dày', 10000);
GO

-- Cac buoc lam banh
IF NOT EXISTS (SELECT 1 FROM dbo.BuocLamBanh)
INSERT dbo.BuocLamBanh (MaBuoc, TenBuoc, ThuTu)
VALUES
 (1, N'Đã nhận đơn', 1),
 (2, N'Chuẩn bị nguyên liệu', 2),
 (3, N'Nhào/cán bột và tạo vỏ bánh', 3),
 (4, N'Phết sốt và thêm thành phần', 4),
 (5, N'Nướng bánh', 5),
 (6, N'Hoàn thành - phục vụ', 6);
GO

-- Thanh phan
IF NOT EXISTS (SELECT 1 FROM dbo.ThanhPhan)
INSERT dbo.ThanhPhan (TenTP, GiaTien, TrangThai)
VALUES
 (N'Phô mai Mozzarella', 15000, 1),
 (N'Xúc xích Pepperoni', 20000, 1),
 (N'Nấm', 10000, 1),
 (N'Ớt chuông', 8000, 1),
 (N'Ô liu', 10000, 1),
 (N'Thịt xông khói', 20000, 1),
 (N'Dứa', 8000, 1),
 (N'Tôm', 25000, 1),
 (N'Mực', 25000, 1),
 (N'Hành tây', 5000, 1),
 (N'Thịt bò bằm', 22000, 1),
 (N'Phô mai xanh', 18000, 1),
 (N'Giăm bông', 18000, 1),
 (N'Bắp ngọt', 7000, 0);   -- vi du thanh phan tam het
GO

-- Pizza lam san (LoaiPizza = 1) + 1 pizza tu lam (LoaiPizza = 2). DonGia la gia co NHO
IF NOT EXISTS (SELECT 1 FROM dbo.Pizza)
BEGIN
    INSERT dbo.Pizza (TenPizza, LoaiPizza, DonGia, HinhAnh, TrangThai)
    VALUES
     (N'Margherita',     1,  89000, N'/Content/images/margherita.jpg', 1),
     (N'Hawaiian',       1,  99000, N'/Content/images/hawaiian.jpg', 1),
     (N'Pepperoni',      1, 109000, N'/Content/images/pepperoni.jpg', 1),
     (N'Hải sản',        1, 129000, N'/Content/images/haisan.jpg', 1),
     (N'Bốn loại phô mai', 1, 119000, N'/Content/images/4cheese.jpg', 1),
     (N'Thập cẩm',       1, 125000, N'/Content/images/thapcam.jpg', 1),
     (N'Pizza tự làm',   2,  79000, N'/Content/images/tulam.jpg', 1);

    INSERT dbo.PizzaLamSan (MaPizza, MoTa)
    SELECT p.MaPizza, v.MoTa
    FROM (VALUES
        (N'Margherita',       N'Sốt cà chua, phô mai Mozzarella - vị cổ điển của Ý.'),
        (N'Hawaiian',         N'Giăm bông, dứa và phô mai Mozzarella.'),
        (N'Pepperoni',        N'Xúc xích Pepperoni cay nhẹ phủ phô mai Mozzarella.'),
        (N'Hải sản',          N'Tôm, mực, hành tây, ớt chuông trên nền sốt cà chua.'),
        (N'Bốn loại phô mai', N'Mozzarella, phô mai xanh cùng các loại phô mai đặc trưng.'),
        (N'Thập cẩm',         N'Pepperoni, thịt xông khói, nấm, ớt chuông, ô liu.')
    ) v(Ten, MoTa)
    JOIN dbo.Pizza p ON p.TenPizza = v.Ten;

    INSERT dbo.PizzaTuLam (MaPizza, CoSotCaChua)
    SELECT MaPizza, 1 FROM dbo.Pizza WHERE LoaiPizza = 2;

    INSERT dbo.ThanhPhanPizzaLamSan (MaPizza, MaTP)
    SELECT p.MaPizza, t.MaTP
    FROM (VALUES
        (N'Margherita',  N'Phô mai Mozzarella'),
        (N'Hawaiian',    N'Giăm bông'),
        (N'Hawaiian',    N'Dứa'),
        (N'Hawaiian',    N'Phô mai Mozzarella'),
        (N'Pepperoni',   N'Xúc xích Pepperoni'),
        (N'Pepperoni',   N'Phô mai Mozzarella'),
        (N'Hải sản',     N'Tôm'),
        (N'Hải sản',     N'Mực'),
        (N'Hải sản',     N'Hành tây'),
        (N'Hải sản',     N'Ớt chuông'),
        (N'Bốn loại phô mai', N'Phô mai Mozzarella'),
        (N'Bốn loại phô mai', N'Phô mai xanh'),
        (N'Thập cẩm',    N'Xúc xích Pepperoni'),
        (N'Thập cẩm',    N'Thịt xông khói'),
        (N'Thập cẩm',    N'Nấm'),
        (N'Thập cẩm',    N'Ớt chuông'),
        (N'Thập cẩm',    N'Ô liu')
    ) v(Pizza, TP)
    JOIN dbo.Pizza p     ON p.TenPizza = v.Pizza
    JOIN dbo.ThanhPhan t ON t.TenTP   = v.TP;
END
GO

-- Nuoc uong
IF NOT EXISTS (SELECT 1 FROM dbo.NuocUong)
INSERT dbo.NuocUong (TenNuoc, DonGia, SoLuongTon, HinhAnh)
VALUES
 (N'Cô-Ca',     20000, 100, N'/Content/images/coca.jpg'),
 (N'Pepsi',     20000, 100, N'/Content/images/pepsi.jpg'),
 (N'Nước chanh', 25000,  60, N'/Content/images/nuocchanh.jpg'),
 (N'Trà đào',   30000,  50, N'/Content/images/tradao.jpg'),
 (N'Nước suối', 10000, 200, N'/Content/images/nuocsuoi.jpg');
GO

-- Cong thanh toan gia lap
IF NOT EXISTS (SELECT 1 FROM dbo.CongThanhToan)
INSERT dbo.CongThanhToan (TenCong, GatewayURL, APIKey)
VALUES (N'Cổng thanh toán giả lập', N'https://localhost/mock-gateway', N'MOCK-API-KEY');
GO

PRINT N'Hoan tat: da tao database PizzaDB va du lieu mau.';
GO
