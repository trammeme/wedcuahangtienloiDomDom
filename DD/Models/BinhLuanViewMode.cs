using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class BinhLuanViewModel
    {
        public int MaBinhLuan { get; set; }
        public int MaSanPham { get; set; }
        public int MaKH { get; set; }
        public string TenKhachHang { get; set; } // Tên khách hàng
        public string Email { get; set; } // Thêm email
        public string NoiDung { get; set; }
        public int? DanhGia { get; set; }
        public System.DateTime? NgayBinhLuan { get; set; }
        public string PhanHoi { get; set; }
        public System.DateTime? NgayPhanHoi { get; set; }
    }
}