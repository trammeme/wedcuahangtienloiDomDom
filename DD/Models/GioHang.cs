using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class GioHang
    {
        thua1Entities3 db = new thua1Entities3();

        public int iMaSanPham { get; set; }
        public string sTenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal dGia { get; set; }  // Đổi từ double sang decimal
        public int iSoLuong { get; set; }
        public int dThanhVien { get; set; }

        public decimal dThanhTien  // Đổi từ double sang decimal
        {
            get { return iSoLuong * dGia; }
        }

        public GioHang(int ms)
        {
            iMaSanPham = ms;
            SanPham s = db.SanPhams.Single(n => n.MaSanPham == iMaSanPham);
            sTenSanPham = s.TenSanPham;
            HinhAnh = s.HinhAnh;
            dGia = s.Gia;  // Không cần Parse vì đã cùng kiểu decimal
            iSoLuong = 1;
        }
    }
}