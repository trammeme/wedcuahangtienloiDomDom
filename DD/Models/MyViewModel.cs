using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class MyViewModel
    {

        public IEnumerable<SanPham> SanPhams { get; set; }
        public IEnumerable<KhuyenMai> KhuyenMais { get; set; }
        public IEnumerable<BinhLuan> BinhLuans { get; set; }
        public int? SelectedProductId { get; set; } // Thêm thuộc tính SelectedProductId
        public List<User> Nguoidung { get; set; }

    }
}