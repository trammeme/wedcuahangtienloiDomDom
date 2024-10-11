using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DD.Models
{
    public class KhuyenMaiViewModel
    {
        [Required(ErrorMessage = "Mã khuyến mãi là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Mã khuyến mãi không được vượt quá 50 ký tự.")]
        public string MaKhuyenMai { get; set; }

        [StringLength(100, ErrorMessage = "Mô tả không được vượt quá 100 ký tự.")]
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu khuyến mãi là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc khuyến mãi là bắt buộc.")]
        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm khuyến mãi là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Mã sản phẩm không được vượt quá 50 ký tự.")]
        public string MaSanPhamKM { get; set; }

        [StringLength(255, ErrorMessage = "Đường dẫn hình ảnh không được vượt quá 255 ký tự.")]
        public string HinhAnh { get; set; }
    }
}