using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class LichSuTichDiemViewModel
    {
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public List<LichSuTichDiem> LichSuTichDiems { get; set; }
    }
}