﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class TichDiem
    {
        public int ID { get; set; }
        public string maKH { get; set; }
        public string tenKH { get; set; } // Đảm bảo thuộc tính này tồn tại
        public int diem { get; set; }
    }
}