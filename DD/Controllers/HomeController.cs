using DD.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using QRCoder;
using System.IO;
using Hangfire;
using System.Data.SqlClient;
using System.Drawing.Imaging;

namespace DD.Controllers
{
    public class HomeController : Controller
    {
        private readonly sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();
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
            var product = db.SanPhams.FirstOrDefault(p => p.MaSanPham == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
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
        public ActionResult AddComment(int masanpham, string noiDung, int danhGia)
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
                if (string.IsNullOrWhiteSpace(noiDung))
                {
                    return Json(new { success = false, message = "Nội dung bình luận không được để trống" });
                }
                if (danhGia < 1 || danhGia > 5)
                {
                    return Json(new { success = false, message = "Đánh giá phải từ 1 đến 5 sao" });
                }
                var comment = new BinhLuan
                {
                    MaSanPham = masanpham,
                    NoiDung = noiDung,
                    MaKH = user.MaKH,
                    DanhGia = danhGia,
                    NgayBinhLuan = DateTime.Now
                };
                db.BinhLuans.Add(comment);
                db.SaveChanges();
                return Json(new
                {
                    success = true,
                    productId = masanpham,
                    authorName = user.TenKH,
                    content = noiDung,
                    rating = danhGia,
                    date = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    message = "Bình luận đã được thêm thành công"
                });
            }
            catch (DbEntityValidationException dbEx)
            {
                var errors = dbEx.EntityValidationErrors
                    .SelectMany(validationResult => validationResult.ValidationErrors)
                    .Select(validationError => validationError.ErrorMessage);
                return Json(new { success = false, message = "Lỗi xác thực: " + string.Join(", ", errors) });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong AddComment: {ex.Message}");
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
            var book = db.SanPhams.FirstOrDefault(s => s.MaSanPham == id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        public ActionResult ThemGioHang(int id, string url)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sanpham = lstGioHang.FirstOrDefault(n => n.iMaSanPham == id);

            if (sanpham == null)
            {
                // Dùng constructor có sẵn để tự load dữ liệu sản phẩm
                sanpham = new GioHang(id);
                lstGioHang.Add(sanpham);
            }
            else
            {
                sanpham.iSoLuong++;
            }

            Session["GioHang"] = lstGioHang;

            if (string.IsNullOrEmpty(url))
                return RedirectToAction("Index", "Home");
            else
                return Redirect(url);
        }


        private int TongSoLuong() => LayGioHang().Sum(n => n.iSoLuong);
        private decimal TongTien() => LayGioHang().Sum(n => n.dThanhTien);

        public ActionResult GioHang()
        {
            var lstGioHang = LayGioHang();
            ViewBag.TongSoLuong = TongSoLuong();
            ViewBag.TongTien = TongTien();
            return View(lstGioHang);
        }

        public ActionResult CapNhatDonHang(int iMaSanPham, int txtSoLuong)
        {
            var lstGioHang = LayGioHang();
            var productToUpdate = lstGioHang.FirstOrDefault(n => n.iMaSanPham == iMaSanPham);
            if (productToUpdate != null)
            {
                productToUpdate.iSoLuong = txtSoLuong;
            }
            Session["GioHang"] = lstGioHang;
            return RedirectToAction("GioHang");
        }

        public ActionResult XoaSPKhoiGioHang(int? iMaSanPham)
        {
            if (iMaSanPham == null)
            {
                return RedirectToAction("Index");
            }
            var lstDonHang = LayGioHang();
            var productToRemove = lstDonHang.FirstOrDefault(n => n.iMaSanPham == iMaSanPham);
            if (productToRemove != null)
            {
                lstDonHang.Remove(productToRemove);
            }
            Session["GioHang"] = lstDonHang;
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
            if (Session["user"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            List<GioHang> lstGioHang = LayGioHang();
            if (!lstGioHang.Any())
            {
                return RedirectToAction("GioHang");
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
                var user = Session["User"] as NguoiDung ?? Session["user"] as NguoiDung;
                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("Session['user'] là null");
                    return RedirectToAction("Login", "Account");
                }

                System.Diagnostics.Debug.WriteLine($"diaChi: {diaChi}, soDienThoai: {soDienThoai}, ngayGiao: {ngayGiao}");
                if (string.IsNullOrEmpty(diaChi) || string.IsNullOrEmpty(soDienThoai) || !ngayGiao.HasValue)
                {
                    TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin";
                    return RedirectToAction("DatHang");
                }

                if (!IsValidPhoneNumber(soDienThoai))
                {
                    TempData["ErrorMessage"] = "Số điện thoại không hợp lệ. Vui lòng nhập số 10 chữ số bắt đầu bằng 0.";
                    return RedirectToAction("DatHang");
                }

                var today = DateTime.Today;
                if (ngayGiao.Value.Date < today.AddDays(1) || ngayGiao.Value.Date > today.AddDays(30))
                {
                    TempData["ErrorMessage"] = "Ngày giao phải từ ngày mai và không quá 30 ngày kể từ hôm nay!";
                    return RedirectToAction("DatHang");
                }

                List<GioHang> lstGioHang = LayGioHang();
                if (!lstGioHang.Any() || user.MaKH <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Giỏ hàng rỗng: {lstGioHang.Count}, MaKH: {user.MaKH}");
                    TempData["ErrorMessage"] = "Có lỗi với giỏ hàng hoặc thông tin khách hàng";
                    return RedirectToAction("DatHang");
                }

                foreach (var item in lstGioHang)
                {
                    var sanPham = db.SanPhams.Find(item.iMaSanPham);
                    if (sanPham == null)
                    {
                        TempData["ErrorMessage"] = $"Sản phẩm {item.iMaSanPham} không tồn tại.";
                        return RedirectToAction("DatHang");
                    }
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var donHang = new DONHANG
                        {
                            HoTen = user.TenKH,
                            MaKH = user.MaKH,
                            NgayDat = DateTime.Now,
                            NgayGiao = ngayGiao.Value,
                            DiaChi = diaChi,
                            DienThoai = soDienThoai.Trim(),
                            TinhTrangGiaoHang = 1
                        };
                        db.DONHANGs.Add(donHang);
                        db.SaveChanges();

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

                        var tichDiem = db.TichDiems.FirstOrDefault(t => t.MaKH == user.MaKH);
                        if (tichDiem == null)
                        {
                            tichDiem = new TichDiem
                            {
                                MaKH = user.MaKH,
                                Diem = 0,
                                CapBac = "Không có cấp bậc",
                                NgayHetHan = DateTime.Today.AddYears(1)
                            };
                            db.TichDiems.Add(tichDiem);
                            db.SaveChanges();
                        }

                        if (tichDiem.NgayHetHan != null && tichDiem.NgayHetHan < DateTime.Today)
                        {
                            db.LichSuTichDiems.Add(new LichSuTichDiem
                            {
                                MaKH = user.MaKH,
                                DiemThayDoi = -tichDiem.Diem,
                                LyDo = "Điểm hết hạn",
                                NgayGiaoDich = DateTime.Now,
                                TrangThaiDiem = "Hết hạn"
                            });
                            tichDiem.Diem = 0;
                            tichDiem.NgayHetHan = null;
                        }

                        int diemThuong = (int)(tongTienDonHang / 100000);
                        tichDiem.Diem += diemThuong;
                        tichDiem.NgayHetHan = DateTime.Today.AddYears(1);

                        if (diemThuong > 0)
                        {
                            db.LichSuTichDiems.Add(new LichSuTichDiem
                            {
                                MaKH = user.MaKH,
                                DiemThayDoi = diemThuong,
                                LyDo = $"Tích điểm từ đơn hàng #{donHang.MaDonHang}",
                                NgayGiaoDich = DateTime.Now,
                                TrangThaiDiem = "Còn hiệu lực"
                            });
                        }

                        var quanLyTichDiem = new QuanLyTichDiemController();
                        tichDiem.CapBac = quanLyTichDiem.GetCapBac(user.MaKH) ?? "Không có cấp bậc";

                        db.SaveChanges();
                        transaction.Commit();

                        if (!string.IsNullOrEmpty(user.Email) && IsValidEmail(user.Email))
                        {
                            try
                            {
                                SendEmail(
                                    user.Email,
                                    user.TenKH,
                                    Convert.ToDouble(tongTienDonHang),
                                    TongSoLuong(),
                                    DateTime.Now.ToString("dd/MM/yyyy"),
                                    ngayGiao.Value.ToString("dd/MM/yyyy")
                                );
                            }
                            catch (Exception emailEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Lỗi gửi email: {emailEx.Message}");
                            }
                        }

                        Session["GioHang"] = null;
                        TempData["SuccessMessage"] = $"Đặt hàng thành công! Bạn được cộng {diemThuong} điểm. Tổng điểm hiện tại: {tichDiem.Diem}. Cấp bậc: {tichDiem.CapBac}";
                        return RedirectToAction("XacNhanDonHang");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Lỗi trong DatHang: {ex.Message}, Inner: {ex.InnerException?.Message}");
                        TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                        return RedirectToAction("DatHang");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi ngoài cùng trong DatHang: {ex.Message}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("DatHang");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$");
        }

        public ActionResult XacNhanDonHang()
        {
            return View();
        }

        private void SendEmail(string email, string tenKH, double tongTien, int soLuong, string ngayDat, string ngayGiao)
        {
            var fromAddress = new MailAddress("moimoixanhla@gmail.com", "Tên cửa hàng của bạn");
            var toAddress = new MailAddress(email, tenKH);
            const string fromPassword = "hvyt bzxm qzti esuo";
            string subject = "Xác nhận đơn hàng từ cửa hàng";
            string body = $"Chào {tenKH},\n\nCảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi.\n" +
                          $"Ngày đặt: {ngayDat}\nNgày giao dự kiến: {ngayGiao}\n" +
                          $"Tổng số lượng: {soLuong} sản phẩm\nTổng tiền: {tongTien:#,##0} VNĐ\n\n" +
                          $"Chúng tôi sẽ xử lý đơn hàng của bạn sớm nhất có thể.\n\nTrân trọng!";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        /*public ActionResult DoiDiem()
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var diemTichLuy = db.TichDiems.FirstOrDefault(t => t.MaKH == user.MaKH);
            if (diemTichLuy == null)
            {
                diemTichLuy = new TichDiem
                {
                    MaKH = user.MaKH,
                    Diem = 0,
                    CapBac = "Không có cấp bậc",
                    NgayHetHan = DateTime.Today.AddYears(1)
                };
                db.TichDiems.Add(diemTichLuy);
                db.SaveChanges();
            }
            ViewBag.PromotionalProducts = db.SanPhamKhuyenMais.ToList();
            return View(diemTichLuy);
        }*/

        
        public static class EmailHelper
        {
            public static void SendEmailWithQRCode(string toEmail, string subject, string body, byte[] qrCodeImage)
            {
                try
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("moimoixanhla@gmail.com", "hvyt bzxm qzti esuo"), // Sử dụng email thống nhất
                        EnableSsl = true,
                    };
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("moimoixanhla@gmail.com"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };
                    if (qrCodeImage != null)
                    {
                        var qrCodeAttachment = new Attachment(new MemoryStream(qrCodeImage), "voucherQRCode.png");
                        mailMessage.Attachments.Add(qrCodeAttachment);
                    }
                    mailMessage.To.Add(toEmail);
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Không thể gửi email: {ex.Message}");
                }
            }

            public static System.Drawing.Image GenerateQRCode(string text)
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new QRCode(qrCodeData);
                    return qrCode.GetGraphic(20);
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
            return Json(new { success = true, points = diemTichLuy.Diem }, JsonRequestBehavior.AllowGet);
        }
      
            public ActionResult DoiDiem()
            {
                // Kiểm tra tất cả các key session có thể có
                var user = Session["User"] as NguoiDung ?? Session["user"] as NguoiDung;
                string tenDangNhap = Session["TenDangNhap"] as string;

                // Nếu không có user object, thử tìm bằng TenDangNhap
                if (user == null && !string.IsNullOrEmpty(tenDangNhap))
                {
                    user = db.NguoiDungs.FirstOrDefault(n => n.TenDangNhap == tenDangNhap);
                }

                // Nếu vẫn không có user, redirect về login
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var tichDiem = db.TichDiems.FirstOrDefault(t => t.MaKH == user.MaKH);

                // Nếu chưa có record tích điểm, tạo mới
                if (tichDiem == null)
                {
                    tichDiem = new TichDiem
                    {
                        MaKH = user.MaKH,
                        Diem = 0,
                        CapBac = "Không có cấp bậc",
                        NgayHetHan = DateTime.Today.AddYears(1)
                    };
                    db.TichDiems.Add(tichDiem);
                    db.SaveChanges();
                }

                return View(tichDiem);
            }

            [HttpPost]
            public JsonResult DoiDiem(int soDiem)
            {
                try
                {
                    var user = Session["User"] as NguoiDung ?? Session["user"] as NguoiDung;
                    string tenDangNhap = Session["TenDangNhap"] as string;

                    if (user == null && !string.IsNullOrEmpty(tenDangNhap))
                    {
                        user = db.NguoiDungs.FirstOrDefault(n => n.TenDangNhap == tenDangNhap);
                    }

                    if (user == null)
                    {
                        return Json(new { success = false, message = "Phiên đăng nhập đã hết. Vui lòng đăng nhập lại." });
                    }

                    var tichDiem = db.TichDiems.FirstOrDefault(t => t.MaKH == user.MaKH);

                    if (tichDiem == null)
                    {
                        return Json(new { success = false, message = "Không có điểm tích lũy." });
                    }

                    if (soDiem <= 0 || soDiem > tichDiem.Diem)
                    {
                        return Json(new { success = false, message = "Số điểm không hợp lệ." });
                    }

                    // Kiểm tra điểm hết hạn
                    if (tichDiem.NgayHetHan != null && tichDiem.NgayHetHan < DateTime.Today)
                    {
                        db.LichSuTichDiems.Add(new LichSuTichDiem
                        {
                            MaKH = user.MaKH,
                            DiemThayDoi = -tichDiem.Diem,
                            LyDo = "Điểm hết hạn",
                            NgayGiaoDich = DateTime.Now,
                            TrangThaiDiem = "Hết hạn"
                        });
                        tichDiem.Diem = 0;
                        tichDiem.NgayHetHan = null;
                        db.SaveChanges();
                        return Json(new { success = false, message = "Điểm đã hết hạn." });
                    }

                    // Trừ điểm
                    var oldCapBac = tichDiem.CapBac;
                    tichDiem.Diem -= soDiem;
                    tichDiem.NgayHetHan = DateTime.Today.AddYears(1); // Gia hạn 1 năm

                    // Ghi lịch sử giao dịch
                    db.LichSuTichDiems.Add(new LichSuTichDiem
                    {
                        MaKH = user.MaKH,
                        DiemThayDoi = -soDiem,
                        LyDo = "Đổi điểm",
                        NgayGiaoDich = DateTime.Now,
                        TrangThaiDiem = "Đã sử dụng"
                    });

                    // Cập nhật cấp bậc
                    db.Database.ExecuteSqlCommand("EXEC CapNhatCapBacTichDiem @MaKH", new SqlParameter("@MaKH", user.MaKH));
                    db.SaveChanges();

                    // Tải lại thông tin tích điểm để đảm bảo dữ liệu mới nhất
                    var updatedTichDiem = db.TichDiems.FirstOrDefault(t => t.MaKH == user.MaKH);

                    // Gửi email thông báo
                    string qrCodeText = $"MaKH: {user.MaKH} - SoDiemDoi: {soDiem} - Ngay: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                    string qrImage = GenerateQrCode(qrCodeText);

                    string message = $@"
                    Bạn đã đổi <strong>{soDiem}</strong> điểm thành công.<br>
                    Tổng điểm hiện tại: <strong>{updatedTichDiem.Diem}</strong>.<br>
                    Đây là mã QR xác nhận của bạn:<br>
                    <img src='{qrImage}' alt='Mã QR' style='max-width: 200px;' /><br>
                    Vui lòng lưu mã QR này để sử dụng.";

                    SendEmailNotification(user.Email, message);

                    // Cập nhật cấp bậc nếu thay đổi
                    if (oldCapBac != updatedTichDiem.CapBac)
                    {
                        SendEmailNotification(user.Email, $"Cấp bậc mới: {updatedTichDiem.CapBac} (Sử dụng {soDiem} điểm)");
                    }

                    return Json(new
                    {
                        success = true,
                        message = "Đổi điểm thành công!",
                        points = updatedTichDiem.Diem,
                        capBac = updatedTichDiem.CapBac
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi trong DoiDiem: {ex.Message}");
                    return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
                }
            }

            public string GenerateQrCode(string text)
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        using (var bitmap = qrCode.GetGraphic(20))
                        {
                            using (var stream = new MemoryStream())
                            {
                                bitmap.Save(stream, ImageFormat.Png);
                                var base64 = Convert.ToBase64String(stream.ToArray());
                                return "data:image/png;base64," + base64;
                            }
                        }
                    }
                }
            }

            private void SendEmailNotification(string toEmail, string message)
            {
                try
                {
                    if (string.IsNullOrEmpty(toEmail)) return;

                    var fromEmail = new MailAddress("moimoixanhla@gmail.com", "Quản lý Tích Điểm DOMDOM");
                    var toAddress = new MailAddress(toEmail);
                    var subject = "Thông báo mã QR đổi điểm";
                    var body = $@"<!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8' />
                    </head>
                    <body>
                        <h3>Chào bạn,</h3>
                        <p>{message}</p>
                        <p>Cảm ơn bạn đã tham gia chương trình tích điểm của DOMDOM!</p>
                        <p>Trân trọng,<br>Đội ngũ DOMDOM</p>
                    </body>
                    </html>";

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        Credentials = new NetworkCredential("moimoixanhla@gmail.com", "hvyt bzxm qzti esuo"),
                        EnableSsl = true
                    };

                    using (var mailMessage = new MailMessage(fromEmail, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                        BodyEncoding = System.Text.Encoding.UTF8,
                        SubjectEncoding = System.Text.Encoding.UTF8
                    })
                    {
                        smtp.Send(mailMessage);
                        System.Diagnostics.Debug.WriteLine($"Email sent to {toEmail} at {DateTime.Now}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error sending email to {toEmail} at {DateTime.Now}: {ex.Message}");
                }
            }

            // Other actions remain unchanged...
        
    public ActionResult Getfooter()
        {
            return PartialView("_footer");
        }
    }
}