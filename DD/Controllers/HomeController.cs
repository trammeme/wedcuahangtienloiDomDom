using DD.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc; // Giữ lại nếu bạn sử dụng ASP.NET MVC
// using Microsoft.AspNetCore.Mvc; // Xóa nếu bạn không sử dụng ASP.NET Core
using System.Data.Entity;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using QRCoder;
using System.IO;

namespace DD.Controllers
{
    public class HomeController : Controller // Đảm bảo kế thừa đúng từ Controller
    {
        private readonly camlyEntities1 db = new camlyEntities1();
        private List<GioHang> cartItems = new List<GioHang>();
        public DbSet<CHITIETDATHANG> CHITIETDATHANG { get; set; }

        public ActionResult Index()
        {
            var khuyenMais = db.KhuyenMais.ToList();
            var danhSachSanPham = db.SanPhams.ToList();
            var model = new MyViewModel
            {
                SanPhams = danhSachSanPham,
                KhuyenMais = khuyenMais
            };

            return View(model);
        }

        public ActionResult Slider()
        {
            return View();
        }
        [ChildActionOnly]
        public ActionResult Footer()
        {
            return PartialView("_Footer");
        }
        public ActionResult sliderbar()
        {
            return View();
        }
        public ActionResult ProductDetail(int id)
        {
            var product = db.SanPhams.FirstOrDefault(p => p.MaSanPham == id); // Lấy sản phẩm theo id

            if (product == null)
            {
                return HttpNotFound(); // Nếu không tìm thấy sản phẩm, trả về lỗi 404
            }

            return View(product); // Trả về view với thông tin sản phẩm
        }

      public ActionResult Detail(int id)
        {
            var product = db.SanPhams
                .Include(p => p.BinhLuans.Select(b => b.NguoiDung))
                .FirstOrDefault(p => p.MaSanPham == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            product.BinhLuans = product.BinhLuans
                .Where(b => b.MaSanPham == id)
                .OrderByDescending(b => b.NgayBinhLuan)
                .ToList();

            return View(product);
        }
        
        public ActionResult GetComments(int masanpham)
        {
            var comments = db.BinhLuans
                .Where(b => b.MaSanPham == masanpham)
                .OrderByDescending(b => b.NgayBinhLuan)
                .Include(b => b.NguoiDung)
                .ToList();

            return PartialView("BinhLuanPartial", comments);
        }
        [HttpPost]
        public ActionResult AddComment(int masanpham, string noiDung)
        {
            try
            {
                if (Session["User"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để bình luận" });
                }
                var user = Session["User"] as NguoiDung;
                if (user == null || user.MaKH <= 0)
                {
                    return Json(new { success = false, message = "Thông tin người dùng không hợp lệ" });
                }
                var product = db.SanPhams.Find(masanpham);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }
                var comment = new BinhLuan
                {
                    MaSanPham = masanpham,
                    NoiDung = noiDung,
                    MaKH = user.MaKH,
                    MaBinhLuan = "BL" + user.MaKH + DateTime.Now.ToString("yyyyMMddHHmmss"), // Tạo mã dựa trên ID người dùng và thời gian
                    NgayBinhLuan = DateTime.Now
                };
                db.BinhLuans.Add(comment);
                db.SaveChanges();

                // Trả về JSON response với thông báo thành công
                return Json(new
                {
                    success = true,
                    productId = masanpham,
                    authorName = user.TenKH,
                    content = noiDung,
                    date = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    message = "Bình luận đã được thêm thành công"
                });
            }
            catch (DbEntityValidationException dbEx)
            {
                var errors = dbEx.EntityValidationErrors
                    .SelectMany(validationResult => validationResult.ValidationErrors)
                    .Select(validationError => validationError.ErrorMessage);
                return Json(new { success = false, message = "Có lỗi xảy ra: " + string.Join(", ", errors) });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddComment method: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại sau." });
            }
        }

        public ActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var searchResults = db.SanPhams
                .Where(p => p.TenSanPham.Contains(query) || p.MoTa.Contains(query))
                .ToList();

            var model = new MyViewModel
            {
                SanPhams = searchResults,
                KhuyenMais = db.KhuyenMais.ToList()
            };

            return View("Index", model);
        }

        [HttpPost]
        public ActionResult AddToFavorites(int productId)
        {
            var favorites = Session["Favorites"] as List<int> ?? new List<int>();

            if (!favorites.Contains(productId))
            {
                favorites.Add(productId);
                Session["Favorites"] = favorites;
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult RemoveFromFavorites(int productId)
        {
            var favorites = Session["Favorites"] as List<int>;
            if (favorites != null && favorites.Contains(productId))
            {
                favorites.Remove(productId);
                Session["Favorites"] = favorites;
            }

            return Json(new { success = true });
        }

        public ActionResult GetFavorites()
        {
            var favorites = Session["Favorites"] as List<int>;
            return Json(favorites ?? new List<int>(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Favorites()
        {
            var favorites = Session["Favorites"] as List<int> ?? new List<int>();

            var favoriteProducts = db.SanPhams
                .Where(p => favorites.Contains(p.MaSanPham))
                .ToList();

            return View(favoriteProducts);
        }

        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }


        public ActionResult Detailsp(int id)
        {
            var book = (from s in db.SanPhams where s.MaSanPham == id select s).SingleOrDefault();

            if (book == null)
            {
                return HttpNotFound(); // Nếu không tìm thấy sách
            }

            return View(book);
        }

        // Thêm đơn hàng
        public ActionResult ThemGioHang(int id, string url)
        {
            // Lấy giỏ hàng từ Session nếu có, nếu không có tạo mới
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang> ?? new List<GioHang>();

            // Tìm sản phẩm trong giỏ hàng
            GioHang sanpham = lstGioHang.Find(n => n.iMaSanPham == id);

            if (sanpham == null)
            {
                // Nếu sản phẩm chưa có trong giỏ hàng, tạo mới và thêm vào giỏ hàng
                sanpham = new GioHang(id);
                lstGioHang.Add(sanpham);
            }
            else
            {
                // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng
                sanpham.iSoLuong++;
            }

            // Lưu giỏ hàng vào Session
            Session["GioHang"] = lstGioHang;

            // Kiểm tra giá trị của url và điều hướng
            if (string.IsNullOrEmpty(url))
            {
                // Nếu url không hợp lệ, chuyển hướng đến trang mặc định (thay "Index" và "Home" theo yêu cầu của bạn)
                return RedirectToAction("Index", "Home");
            }

            // Nếu url hợp lệ, điều hướng tới url đó
            return Redirect(url);
        }

        // Tính tổng số lượng và tổng tiền của đơn hàng
        private int TongSoLuong() => LayGioHang().Sum(n => n.iSoLuong);
        private decimal TongTien() => LayGioHang().Sum(n => n.dThanhTien);

        public ActionResult GioHang()
        {
            var lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            return View(lstGioHang);
        }

        // Cập nhật thông tin đơn hàng
        public ActionResult CapNhatDonHang(int iMaSanPham, int txtSoLuong)
        {
            var lstGioHang = LayGioHang(); // Lấy giỏ hàng từ session

            var productToUpdate = lstGioHang.FirstOrDefault(n => n.iMaSanPham == iMaSanPham);
            if (productToUpdate != null)
            {
                // Cập nhật số lượng sản phẩm
                productToUpdate.iSoLuong = txtSoLuong;

                // Không cần gán dThanhTien, vì nó sẽ tự tính toán
            }

            // Lưu lại giỏ hàng sau khi cập nhật
            Session["GioHang"] = lstGioHang;

            // Quay lại trang giỏ hàng
            return RedirectToAction("GioHang");
        }

        public ActionResult XoaSPKhoiGioHang(int? iMaSanPham)
        {
            if (iMaSanPham == null)
            {
                // Nếu không có sản phẩm, quay về trang giỏ hàng
                return RedirectToAction("Index");
            }

            // Lấy giỏ hàng từ session
            var lstDonHang = LayGioHang();

            // Tìm sản phẩm cần xóa
            var productToRemove = lstDonHang.FirstOrDefault(n => n.iMaSanPham == iMaSanPham);
            if (productToRemove != null)
            {
                // Xóa sản phẩm khỏi giỏ hàng
                lstDonHang.Remove(productToRemove);
            }

            // Cập nhật lại giỏ hàng trong session
            Session["GioHang"] = lstDonHang;

            // Quay lại trang giỏ hàng đã cập nhật
            return RedirectToAction("GioHang");
        }

        public ActionResult XoaGioHang()
        {
            Session["GioHang"] = null;
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult DatHang()
        {
            // Kiểm tra đăng nhập chưa
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy hàng từ Session
            List<GioHang> lstGioHang = LayGioHang();
            if (!lstGioHang.Any())
            {
                return RedirectToAction("GioHang"); // Nếu không có hàng trong giỏ
            }

            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();

            return View(lstGioHang);
        }


        [HttpPost]
        public ActionResult DatHang(string diaChi, string soDienThoai, DateTime? ngayGiao)
        {
            try
            {
                // Kiểm tra người dùng đã đăng nhập
                if (Session["user"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Các validation checks hiện tại
                if (string.IsNullOrEmpty(diaChi) || string.IsNullOrEmpty(soDienThoai) || !ngayGiao.HasValue)
                {
                    TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin";
                    return RedirectToAction("DatHang");
                }

                var today = DateTime.Today;
                if (ngayGiao.Value.Date < today.AddDays(1) || ngayGiao.Value.Date > today.AddDays(30))
                {
                    TempData["ErrorMessage"] = "Ngày giao phải từ ngày mai và không quá 30 ngày kể từ hôm nay!";
                    return RedirectToAction("DatHang");
                }

                // Lấy thông tin giỏ hàng và khách hàng
                List<GioHang> lstGioHang = LayGioHang();
                var khachHang = Session["user"] as NguoiDung;

                if (!lstGioHang.Any() || khachHang == null || khachHang.MaKH <= 0)
                {
                    TempData["ErrorMessage"] = "Có lỗi với giỏ hàng hoặc thông tin khách hàng";
                    return RedirectToAction("DatHang");
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Tạo đơn hàng mới
                        var donHang = new DONHANG
                        {
                            HoTen = khachHang.TenKH,
                            MaKH = khachHang.MaKH,
                            NgayDat = DateTime.Now,
                            NgayGiao = ngayGiao.Value,
                            DiaChi = diaChi,
                            DienThoai = soDienThoai.Trim(),
                            TinhTrangGiaoHang = 1
                        };

                        db.DONHANGs.Add(donHang);
                        db.SaveChanges();

                        // Thêm chi tiết đơn hàng
                        decimal tongTienDonHang = 0;
                        foreach (var item in lstGioHang)
                        {
                            var chiTiet = new CHITIETDATHANG
                            {
                                MaDonHang = donHang.MaDonHang,
                                MaSanPham = item.iMaSanPham,
                                SoLuong = item.iSoLuong,
                                DonGia = (decimal)item.dGia
                            };
                            db.CHITIETDATHANGs.Add(chiTiet);
                            tongTienDonHang += (decimal)item.dThanhTien;
                        }

                        // Tính và cập nhật điểm tích lũy
                        // Tính và cập nhật điểm tích lũy  
                        var tichDiem = db.TichDiems.FirstOrDefault(t => t.MaKH == khachHang.MaKH);
                        if (tichDiem == null)
                        {
                            tichDiem = new TichDiem
                            {
                                MaKH = khachHang.MaKH,
                                Diem = 0,
                                CapBac = "Thành viên mới"
                            };
                            db.TichDiems.Add(tichDiem);
                        }

                        // Trước khi cập nhật điểm  
                        // Tính điểm: cứ 100,000 VNĐ được 1 điểm  
                        int diemThuong = (int)(tongTienDonHang / 10000);
                        tichDiem.Diem += diemThuong;

                        // Cập nhật cấp bậc
                        tichDiem.CapBac = new QuanLyTichDiemController().CalculateCapBac(tichDiem.Diem);

                        // Lưu thay đổi vào cơ sở dữ liệu
                        db.SaveChanges();


                        db.SaveChanges();
                        transaction.Commit();

                        // Gửi email xác nhận và thông báo điểm tích lũy
                        if (!string.IsNullOrEmpty(khachHang.Email))
                        {
                            try
                            {
                                SendEmail(
                                    khachHang.Email,
                                    khachHang.TenKH,
                                    Convert.ToDouble(tongTienDonHang),
                                    TongSoLuong(),
                                    DateTime.Now.ToString("dd/MM/yyyy"),
                                    ngayGiao.Value.ToString("dd/MM/yyyy")
                                );
                            }
                            catch (Exception emailEx)
                            {
                                // Log lỗi email nhưng không ảnh hưởng đến việc đặt hàng
                                System.Diagnostics.Debug.WriteLine($"Lỗi gửi email: {emailEx.Message}");
                            }
                        }

                        // Xóa giỏ hàng và thông báo thành công với điểm tích lũy hiện tại
                        Session["GioHang"] = null;
                        TempData["SuccessMessage"] = $"Đặt hàng thành công! Bạn được cộng {diemThuong} điểm. Tổng điểm hiện tại của bạn là {tichDiem.Diem} điểm. Cấp bậc hiện tại: {tichDiem.CapBac}";

                        return RedirectToAction("XacNhanDonHang");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                        return RedirectToAction("DatHang");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("DatHang");
            }
        }

        // Helper method để validate số điện thoại
        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return false;
            // Kiểm tra số điện thoại VN (10 số, bắt đầu bằng 0)
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$");
        }
        public ActionResult XacNhanDonHang()
        {

            return View();
        }
        private bool SendEmail(string toEmail, string tenkh, double tongtien, int tongsoluong, string ngaydat, string ngaygiao)
        {
            List<GioHang> lstCart = LayGioHang();
            var fromEmail = "2224802010596@student.tdmu.edu.vn"; // Email của bạn  
            var fromPassword = "lqci gzzj tvaq ocss"; // Mật khẩu của bạn  

            var body = $@"  
    <html>  
        <head>  
            <style>  
                body {{ font-family: Arial, sans-serif; }}  
                .container {{ max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}  
                h4 {{ color: #4CAF50; }}  
                table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}  
                th, td {{ border: 1px solid #ddd; padding: 10px; text-align: center; }}  
                th {{ background-color: #f2f2f2; }}  
                .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}  
            </style>  
        </head>  
        <body>  
            <div class='container'>  
                <h4>Cảm ơn <strong>{tenkh}</strong> đã đặt sản phẩm tại cửa hàng <strong>DOMDOM</strong> của chúng tôi!</h4>  
                <h4>Sau đây là thông tin đơn hàng bạn đã đặt:</h4>  
                <h4>Ngày đặt: <strong>{ngaydat}</strong></h4>  
                <h4>Ngày giao dự kiến: <strong>{ngaygiao}</strong></h4>  
                <table>  
                    <tr>  
                        <th>Mã Sản Phẩm</th>  
                        <th>Tên Sản Phẩm</th>  
                        <th>Số Lượng</th>  
                        <th>Đơn Giá</th>  
                        <th>Thành Tiền</th>  
                    </tr>";

            foreach (var item in lstCart)
            {
                body += $@"  
        <tr>  
            <td><b>{item.iMaSanPham}</b></td>  
            <td>{item.sTenSanPham}</td>  
            <td>{item.iSoLuong}</td>  
            <td>{item.dGia:#,##0} VND</td>  
            <td>{item.dThanhTien:#,##0} VND</td>  
        </tr>";
            }

            body += $@"  
                </table>  
                <h4>Tổng số lượng: <strong>{tongsoluong:#,##0}</strong></h4>  
                <h4>Tổng tiền: <strong>{tongtien:#,##0} VND</strong></h4>  
                <h4>Đơn hàng sẽ được nhanh chóng giao tới bạn! Xin cảm ơn!</h4>  
                <div class='footer'>  
                    <p> Xin chân thành cảm ơn! </p>  
                    <p> Cửa hàng DOMDOM - Đội ngũ chăm sóc khách hàng. </p>  
                </div>  
            </div>  
        </body>  
    </html>";

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            // Tạo và gửi email  
            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Xác nhận đơn hàng",
                Body = body,
                IsBodyHtml = true, // Gửi mail dạng HTML  
            };

            mailMessage.To.Add(toEmail);

            // Gửi mail  
            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu việc gửi mail thất bại  
                ViewBag.Message = $"Đã xảy ra lỗi khi gửi email: {ex.Message}";
                return false;
            }
       
        }



        public ActionResult DoiDiem()
        {
            var user = Session["User"] as NguoiDung;

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var diemTichLuy = db.TichDiems.FirstOrDefault(t => t.MaKH == user.MaKH);
            if (diemTichLuy == null)
            {
                return HttpNotFound(); // Không tìm thấy thông tin tích điểm của người dùng
            }

            // Tải danh sách sản phẩm khuyến mãi
            ViewBag.PromotionalProducts = db.SanPhamKhuyenMais.ToList();

            return View(diemTichLuy);
        }
        [HttpPost]
        public ActionResult DoiDiem(int soDiem, string loaiDoiDiem, int? productId)
        {
            var user = Session["User"] as NguoiDung;

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var diemTichLuy = db.TichDiems.FirstOrDefault(t => t.MaKH == user.MaKH);
            if (diemTichLuy == null)
            {
                return HttpNotFound(); // Nếu không tìm thấy thông tin tích điểm của người dùng
            }

            if (diemTichLuy.Diem < soDiem)
            {
                TempData["ErrorMessage"] = "Số điểm không đủ để thực hiện giao dịch."; // Thông báo nếu điểm không đủ
                return RedirectToAction("DoiDiem");
            }

            // Giảm số điểm của người dùng
            diemTichLuy.Diem -= soDiem;

            if (loaiDoiDiem == "voucher")
            {
                var voucherCode = "VOUCHER" + new Random().Next(1000, 9999);

                // Tạo mã QR cho voucher
                var qrCodeImage = EmailHelper.GenerateQRCode(voucherCode); // Gọi đúng phương thức GenerateQRCode từ EmailHelper

                // Chuyển đổi mã QR thành byte array
                var qrCodeByteArray = EmailHelper.ConvertToByteArray(qrCodeImage); // Gọi đúng phương thức ConvertToByteArray từ EmailHelper

                // Gửi email với mã voucher và mã QR
                EmailHelper.SendEmailWithQRCode(user.Email, "Mã voucher của bạn", $"Mã voucher của bạn là: {voucherCode}", qrCodeByteArray);

                TempData["SuccessMessage"] = "Đổi điểm thành công! Mã voucher đã được gửi về email của bạn.";
            }
            else if (loaiDoiDiem == "sanpham" && productId.HasValue)
            {
                var product = db.SanPhamKhuyenMais.FirstOrDefault(p => p.MaSanPham == productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm khuyến mãi không tồn tại.";
                    return RedirectToAction("DoiDiem");
                }

                TempData["SuccessMessage"] = $"Đổi điểm thành công! Bạn đã nhận được sản phẩm khuyến mãi: {product.TenSanPham}.";
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public static class EmailHelper
        {
            public static void SendEmailWithQRCode(string toEmail, string subject, string body, byte[] qrCodeImage)
            {
                try
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("2224802010596@student.tdmu.edu.vn", "lqci gzzj tvaq ocss"),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("2224802010596@student.tdmu.edu.vn"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };

                    var qrCodeAttachment = new Attachment(new MemoryStream(qrCodeImage), "voucherQRCode.png");
                    mailMessage.Attachments.Add(qrCodeAttachment);

                    mailMessage.To.Add(toEmail);

                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Không thể gửi email: " + ex.Message);
                    throw new Exception("Gửi email thất bại: " + ex.Message);
                }
            }

            public static System.Drawing.Image GenerateQRCode(string text)
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new QRCode(qrCodeData);
                    return qrCode.GetGraphic(20); // Return an Image object
                }
            }

            public static byte[] ConvertToByteArray(System.Drawing.Image image)
            {
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
    
        public static byte[] GenerateQRCode(string text)
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new QRCode(qrCodeData);

                    using (var ms = new MemoryStream())
                    {
                        qrCode.GetGraphic(20).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }

       
        // Hàm kiểm tra xem người dùng có điểm tích lũy không
        public ActionResult GetUserPoints()
        {
            var user = Session["User"] as NguoiDung;

            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để kiểm tra điểm." }, JsonRequestBehavior.AllowGet);
            }

            var diemTichLuy = db.TichDiems.FirstOrDefault(t => t.MaKH == user.MaKH);

            if (diemTichLuy == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin điểm của bạn." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, points = diemTichLuy.Diem }, JsonRequestBehavior.AllowGet); // Trả về điểm tích lũy
        }
        public ActionResult Getfooter()
        {
            return PartialView("_footer");
        }

    }
}

    
