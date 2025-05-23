using System.ComponentModel.DataAnnotations;

namespace DD.Models
{
    public class SanPhamKhuyenMaiViewModel
    {
        public int MaSanPhamKM { get; set; } // Tự động tạo bởi cơ sở dữ liệu

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string TenSanPham { get; set; }

        [Required(ErrorMessage = "Mô tả khuyến mãi là bắt buộc")]
        public string MoTaKhuyenMai { get; set; }

        public string HinhAnh { get; set; }

        // Cho phép null để hỗ trợ khuyến mãi chung
        public int? MaSanPham { get; set; }

        [Required(ErrorMessage = "Số lượng khuyến mãi là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng khuyến mãi phải là số không âm")]
        public int SoLuongKhuyenMai { get; set; }
    }
}