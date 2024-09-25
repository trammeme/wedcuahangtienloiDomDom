using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DD.Models;
namespace DD.Models
{
    public class RegisterViewModel
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string Email { get; set; }
        public string GioiTinh { get; set; }
        public int ThangSinh { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
    }

}
   