using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class BinhLuan
    {
        public string MaBinhLuan { get; set; } // Should be string for GUID public DateTime NgayBinhLuan { get; set; }  
        public int MaSanPham { get; set; } // Ensure this is int public string NoiDung { get; set; }  
        public int MaKH { get; set; } // Ensure this is int public string MaBinhLuan { get; set; } // This should be string if you are using Guid public DateTime NgayBinhLuan { get; set; }  
        public string NoiDung { get; set; }
        public DateTime NgayBinhLuan { get; set; }
        public string PhanHoi { get; set; } // Thuộc tính phản hồi
        public Nullable<System.DateTime> NgayPhanHoi { get; set; }

        public virtual NguoiDung NguoiDung { get; set; } // Hoặc dùng kiểu dữ liệu nào phù hợp
        public virtual SanPham SanPham { get; set; } // Hoặc dùng kiểu dữ liệu nào phù hợp


    }
  
}