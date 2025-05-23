using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class GioHang
    {
        sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();

        public int iMaSanPham { get; set; }
        public string sTenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal? dGia { get; set; }
        public int iSoLuong { get; set; }
        public string LoaiSanPham { get; set; }

        public decimal dThanhTien
        {
            get { return iSoLuong * (dGia ?? 0); }
        }

        public GioHang(int ms)
        {
            iMaSanPham = ms;
            SanPham s = db.SanPhams.Single(n => n.MaSanPham == iMaSanPham);
            sTenSanPham = s.TenSanPham;
            HinhAnh = s.HinhAnh;
            dGia = s.Gia;
            iSoLuong = 1;
            LoaiSanPham = s.LoaiSanPham; // Gán thêm ở đây
        }
    }
}
