using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
        public class DGnguoidung
        {
            public string MaDanhGia { get; set; }
            public string MaKH { get; set; }
            public string NoiDung { get; set; }
            public int SoSao { get; set; }
            public DateTime NgayDanhGia { get; set; }

            // Thêm thông tin về sản phẩm và người đánh giá
            public string TenSanPham { get; set; }
            public string NguoiDanhGia { get; set; }

            // Thêm thuộc tính TenKhuyenMai
            public string TenKhuyenMai { get; set; }
        }
    }
