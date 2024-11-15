using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class DonHangViewModel
    {
        public DONHANG DonHang { get; set; }
        public List<CharEnumerator> ChiTietDonHangs { get; set; }
    }
}