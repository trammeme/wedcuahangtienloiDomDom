-- Create database and use it
CREATE DATABASE camly;
GO
USE camly;
GO

-- User table
CREATE TABLE NguoiDung (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    GioiTinh NVARCHAR(10),
    ThangSinh INT,
    DiaChi NVARCHAR(255),
    SoDienThoai VARCHAR(15),
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(255) NOT NULL
);

-- Admin table
CREATE TABLE QuanTriVien (
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(255) NOT NULL
);

-- Store table
CREATE TABLE CuaHang (
    MaCuaHang VARCHAR(50) PRIMARY KEY,
    DiaChi NVARCHAR(255) NOT NULL,
    AdminID INT,
    FOREIGN KEY (AdminID) REFERENCES QuanTriVien(AdminID)
);

-- Product table
CREATE TABLE SanPham (
    MaSanPham INT IDENTITY(1,1) PRIMARY KEY,
    TenSanPham NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(500),
    Gia DECIMAL(18, 2) NOT NULL,
    HinhAnh NVARCHAR(255),
    LoaiSanPham NVARCHAR(50) NOT NULL
);
-- Promotional Product table
CREATE TABLE SanPhamKhuyenMai (
    MaSanPhamKM VARCHAR(50) PRIMARY KEY,
    TenSanPham NVARCHAR(255) NOT NULL,
    MoTaKhuyenMai NVARCHAR(255),
    HinhAnh NVARCHAR(255),
    MaSanPham INT,
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
);


-- Promotion table
CREATE TABLE KhuyenMai (
    MaKhuyenMai VARCHAR(50) PRIMARY KEY,
    TenKhuyenMai NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255),
    NgayBatDau DATE NOT NULL,
    NgayKetThuc DATE NOT NULL,
    MaSanPhamKM VARCHAR(50),
    HinhAnh NVARCHAR(255),
    FOREIGN KEY (MaSanPhamKM) REFERENCES SanPhamKhuyenMai(MaSanPhamKM)
);

-- Store-Promotion relationship table
CREATE TABLE CuaHang_KhuyenMai (
    MaCuaHang VARCHAR(50),
    MaKhuyenMai VARCHAR(50),
    PRIMARY KEY (MaCuaHang, MaKhuyenMai),
    FOREIGN KEY (MaCuaHang) REFERENCES CuaHang(MaCuaHang),
    FOREIGN KEY (MaKhuyenMai) REFERENCES KhuyenMai(MaKhuyenMai)
);

-- Points table
CREATE TABLE TichDiem (
    MaKH INT PRIMARY KEY,
    Diem INT NOT NULL DEFAULT 0,
    CapBac NVARCHAR(50) DEFAULT N'Không có c?p b?c',
    FOREIGN KEY (MaKH) REFERENCES NguoiDung(MaKH)
);

CREATE TABLE BinhLuan (
    MaBinhLuan VARCHAR(50) PRIMARY KEY,
    MaKH INT NOT NULL,
    MaSanPham INT NOT NULL,
    NoiDung TEXT NOT NULL,
    NgayBinhLuan DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaKH) REFERENCES NguoiDung(MaKH), -- Gi? s? b?ng KhachHang có tru?ng MaKH
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham) -- Gi? s? b?ng SanPham có tru?ng MaSanPham
);

CREATE TABLE DONHANG (
    MaDonHang INT PRIMARY KEY IDENTITY(1,1),  -- Mã don hàng, t? d?ng tang
    MaKH INT NOT NULL,                         -- Mã khách hàng
    HoTen NVARCHAR(100) NOT NULL,             -- H? tên khách hàng
    DiaChi NVARCHAR(255) NOT NULL,            -- Ð?a ch? giao hàng
    DienThoai NVARCHAR(15) NOT NULL,          -- S? di?n tho?i khách hàng
    NgayDat DATETIME NOT NULL,                 -- Ngày d?t hàng
    NgayGiao DATETIME NOT NULL,                -- Ngày giao hàng
    TinhTrangGiaoHang INT NOT NULL,           -- Tr?ng thái giao hàng
    DaThanhToan BIT NOT NULL                   -- Tr?ng thái thanh toán
);

CREATE TABLE CHITIETDATHANG
(   
    MaChiTiet INT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang INT,
	MaSanPham INT,
	SoLuong INT CHECK(SoLuong>0),
	DonGia DECIMAL(18,2) CHECK(DonGia>=0),
	FOREIGN KEY (MaDonHang) REFERENCES DONHANG(MaDonHang),
    FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham)
)
-- T?o l?i b?ng DonHang


-- Insert data into NguoiDung table
INSERT INTO NguoiDung (TenKH, Email, GioiTinh, ThangSinh, DiaChi, SoDienThoai, TenDangNhap, MatKhau) 
VALUES 
('Nguyen Van A', 'nguyen@gmail.com', 'Nam', 5, '123 Le Loi, Q1, HCM', '0123456789', 'nguyenvana', 'vana'),
('Tran Thi B', 'tran@gmail.com', 'Nu', 11, '456 Nguyen Trai, Q5, HCM', '0987654321', 'tranthib', 'thib'),
('Duong Van Hai', 'duong@gmail.com', 'Nam', 2, '29 Cach Mang Thang Tam, Q3, HCM', '0976543210', 'duongvanhai', 'vanhai'),
('Phan Thi Thu Van', 'phan@gmail.com', 'Nu', 7, '235 Nguyen Anh Thu, Q12, HCM', '0923546789', 'phanthithuvan', 'thuvan'),
('Nguyen Phuong Thao', 'ngu@gmail.com', 'Nu', 9, '23/6 Nguyen Trai, Q1, HCM', '0912345890', 'nguyenphuongthao', 'phuongthao'),
('Nguyen Tuan Anh', 'nguyenanh@gmail.com', 'Nam', 6, '91/B Duong 3/2, Q10, HCM', '0909876543', 'nuyentuananh', 'tuananh'),
('Vo Tan Nghia', 'vo@gmail.com', 'Nam', 10, '212 Le Van Khuong, Q12, HCM', '0912309876', 'votannghia', 'tannghia');

-- Insert data into QuanTriVien table
INSERT INTO QuanTriVien (TenDangNhap, MatKhau)
VALUES 
('ad1', 'admin1'),
('ad2', 'admin2'),
('ad3', 'admin3');

-- Insert data into SanPham table
INSERT INTO SanPham (TenSanPham, MoTa, Gia, HinhAnh, LoaiSanPham)
VALUES 
(N'Cà phê', N'Cà phê den d?m dà, v?i huong v? m?nh m?, d?ng và thom ngát', 20000, '/Images/caphe.jpg', 'thucuong'),
(N'S?a chua xoài', N'S?a chua k?t h?p xoài tuoi mát, ng?t d?u', 30000, '/Images/suachuaxoai.jpg', 'thucuong'),
(N'Xoài ép', N'Nu?c ép xoài tuoi mát, gi? nguyên huong v? t? nhiên', 25000, '/Images/nuocepxoai.jpg', 'thucuong'),
(N'Trà nhãn', N'Trà nhãn huong thom nhè nh?, k?t h?p v?i v? thanh mát c?a trà', 28000, '/Images/trasen.jpg', 'thucuong'),
(N'Bánh cá', N'Bánh cá nhân d?u d? m?m m?i, v? ng?t bùi', 15000, '/Images/banhca.jpg', 'doan'),
(N'Bánh bao', N'Bánh bao nhân th?t v?i v? bánh m?m, thom', 20000, '/Images/banhbao.jpg', 'doan'),
(N'Ch? cá', N'Ch? cá thom ngon, du?c làm t? cá tuoi ngon', 35000, '/Images/chaca.jpg', 'doan'),
(N'Khoai tây', N'Khoai tây chiên giòn r?m, thom ngon', 18000, '/Images/khoaitay.jpg', 'doan'),
(N'Mì gà s?t', N'Mì gà s?t cay h?p d?n v?i huong v? d?m dà', 40000, '/Images/migasot.jpg', 'doan');

-- Insert data into SanPhamKhuyenMai table
INSERT INTO SanPhamKhuyenMai (MaSanPhamKM, TenSanPham, MoTaKhuyenMai, HinhAnh, MaSanPham) 
VALUES 
('SP01', 'Oreo', 'Bánh oreo ngon', 'images/SPOreo1', 5),
('SP02', 'S?a', 'S?a tuoi ngon', 'images/SPKM2', 2),
('SP03', 'Coca Cola', 'Nu?c ng?t có gas', 'images/SPKM3', 3);

-- Insert data into KhuyenMai table
INSERT INTO KhuyenMai (MaKhuyenMai, TenKhuyenMai, MoTa, NgayBatDau, NgayKetThuc, MaSanPhamKM, HinhAnh) 
VALUES 
('KM01', N'Khuy?n mãi bánh', N'KM cho bánh', '2024-10-10', '2024-10-20', 'SP01', 'images/khuyenmai1'),
('KM02', N'Khuy?n mãi c?c', N'Khuy?n mãi cho c?c', '2024-10-15', '2024-10-25', 'SP02', 'images/khuyenmai2.jpg'),
('KM03', N'Khuy?n mãi s?a', N'Khuy?n mãi cho s?a', '2024-10-20', '2024-10-30', 'SP03', 'images/khuyenmai3.jpg');

-- Insert data into CuaHang table
INSERT INTO CuaHang (MaCuaHang, DiaChi, AdminID)
VALUES 
('CH1', '123 Le Loi, Q1, HCM', 1),
('CH2', '456 Nguyen Trai, Q5, HCM', 2),
('CH3', '35/1 Dien Bien Phu, Q3, HCM', 3),
('CH4', '10/5 Nhi Binh, Hoc Mon, HCM', 1),
('CH5', '789 To Ky, Q12, HCM', 2),
('CH6', '135 Le Thi Hong, Go Vap, HCM', 3),
('CH7', '21/12 Ta Quang Buu, Q12, HCM', 1);

-- Insert data into CuaHang_KhuyenMai table
INSERT INTO CuaHang_KhuyenMai (MaCuaHang, MaKhuyenMai)
VALUES 
('CH1', 'KM01'),
('CH1', 'KM02'),
('CH2', 'KM01'),
('CH3', 'KM02'),
('CH4', 'KM02'),
('CH5', 'KM01'),
('CH6', 'KM02'),
('CH7', 'KM01');

-- Insert data into TichDiem table
INSERT INTO TichDiem (MaKH, Diem)
VALUES 
(1, 100),
(2, 250),
(3, 150),
(4, 50),
(5, 24),
(6, 35),
(7, 8);
