using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class qldanhgia
    {
       
            public string MaDangGia { get; set; }
            public string MaKH { get; set; }
            public string NoiDung { get; set; }
            public int SoSao { get; set; }
            public DateTime NgayDanhGia { get; set; } = DateTime.Now;
            public bool IsApproved { get; set; } = false; // Thêm thuộc tính này
        public string PhanHoi { get; set; } // Thêm thuộc tính này để lưu phản hồi

    }
}

