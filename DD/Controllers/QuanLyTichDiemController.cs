using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.SqlClient;
using QRCoder;
using System.Drawing.Imaging;
using System.IO;

namespace DD.Controllers
{
    public class QuanLyTichDiemController : Controller
    {
        private readonly sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();

        // GET: QuanLyTichDiem - Xem danh sách khách hàng và tích điểm
        // GET: QuanLyTichDiem - Xem danh sách khách hàng và tích điểm
        public ActionResult Index()
        {
            var result = from user in db.NguoiDungs
                         join point in db.TichDiems on user.MaKH equals point.MaKH into points
                         from point in points.DefaultIfEmpty()
                         select new UserPointViewModel
                         {
                             MaKH = user.MaKH,
                             TenKH = user.TenKH,
                             Email = user.Email,
                             ThangSinh = user.ThangSinh,
                             SoDienThoai = user.SoDienThoai,
                             Diem = point != null ? point.Diem : 0,
                             CapBac = point != null ? point.CapBac : "Không có cấp bậc",
                             NgayHetHan = point != null ? point.NgayHetHan : null
                         };

            return View(result.ToList());
        }

        // Lấy cấp bậc
        public string GetCapBac(int maKH)
        {
            var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
            if (tichDiem == null)
                return "Không có cấp bậc";

            // Kiểm tra điểm hết hạn
            if (tichDiem.NgayHetHan != null && tichDiem.NgayHetHan < DateTime.Today)
            {
                db.LichSuTichDiems.Add(new LichSuTichDiem
                {
                    MaKH = maKH,
                    DiemThayDoi = -tichDiem.Diem,
                    LyDo = "Điểm hết hạn",
                    NgayGiaoDich = DateTime.Now,
                    TrangThaiDiem = "Hết hạn"
                });
                tichDiem.Diem = 0;
                tichDiem.NgayHetHan = null;
                db.Database.ExecuteSqlCommand("EXEC CapNhatCapBacTichDiem @MaKH", new SqlParameter("@MaKH", maKH));
                db.SaveChanges();
            }
            else
            {
                db.Database.ExecuteSqlCommand("EXEC CapNhatCapBacTichDiem @MaKH", new SqlParameter("@MaKH", maKH));
                db.SaveChanges();
            }

            return db.TichDiems.FirstOrDefault(td => td.MaKH == maKH)?.CapBac ?? "Không có cấp bậc";
        }

        // Cập nhật cấp bậc sử dụng stored procedure
        [HttpPost]
        public JsonResult CapNhatCapBac(int maKH)
        {
            try
            {
                var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
                if (tichDiem == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin tích điểm." });

                // Kiểm tra điểm hết hạn
                if (tichDiem.NgayHetHan != null && tichDiem.NgayHetHan < DateTime.Today)
                {
                    db.LichSuTichDiems.Add(new LichSuTichDiem
                    {
                        MaKH = maKH,
                        DiemThayDoi = -tichDiem.Diem,
                        LyDo = "Điểm hết hạn",
                        NgayGiaoDich = DateTime.Now,
                        TrangThaiDiem = "Hết hạn"
                    });
                    tichDiem.Diem = 0;
                    tichDiem.NgayHetHan = null;
                }

                // Gọi stored procedure để cập nhật cấp bậc
                db.Database.ExecuteSqlCommand("EXEC CapNhatCapBacTichDiem @MaKH", new SqlParameter("@MaKH", maKH));
                db.SaveChanges();

                var updatedTichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
                return Json(new { success = true, capBac = updatedTichDiem.CapBac });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong CapNhatCapBac: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Cập nhật điểm (thêm hoặc trừ)
        [HttpPost]
        public JsonResult CapNhatDiem(int maKH, int diem, string lyDo)
        {
            try
            {
                if (diem == 0)
                    return Json(new { success = false, message = "Điểm thay đổi phải khác 0." });

                var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
                if (tichDiem == null)
                {
                    tichDiem = new TichDiem { MaKH = maKH, Diem = 0, CapBac = "Không có cấp bậc", NgayHetHan = DateTime.Today.AddYears(1) };
                    db.TichDiems.Add(tichDiem);
                }

                // Kiểm tra điểm hết hạn
                if (tichDiem.NgayHetHan != null && tichDiem.NgayHetHan < DateTime.Today)
                {
                    db.LichSuTichDiems.Add(new LichSuTichDiem
                    {
                        MaKH = maKH,
                        DiemThayDoi = -tichDiem.Diem,
                        LyDo = "Điểm hết hạn",
                        NgayGiaoDich = DateTime.Now,
                        TrangThaiDiem = "Hết hạn"
                    });
                    tichDiem.Diem = 0;
                    tichDiem.NgayHetHan = null;
                }

                // Kiểm tra điểm âm
                if (tichDiem.Diem + diem < 0)
                    return Json(new { success = false, message = "Điểm không đủ để thực hiện giao dịch." });

                var oldCapBac = tichDiem.CapBac;
                tichDiem.Diem += diem;
                tichDiem.NgayHetHan = DateTime.Today.AddYears(1); // Gia hạn 1 năm

                // Ghi lịch sử giao dịch
                db.LichSuTichDiems.Add(new LichSuTichDiem
                {
                    MaKH = maKH,
                    DiemThayDoi = diem,
                    LyDo = lyDo,
                    NgayGiaoDich = DateTime.Now,
                    TrangThaiDiem = diem > 0 ? "Còn hiệu lực" : "Đã sử dụng"
                });

                // Cập nhật cấp bậc
                db.Database.ExecuteSqlCommand("EXEC CapNhatCapBacTichDiem @MaKH", new SqlParameter("@MaKH", maKH));
                db.SaveChanges();

                var updatedTichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
                if (oldCapBac != updatedTichDiem.CapBac)
                {
                    var user = db.NguoiDungs.FirstOrDefault(u => u.MaKH == maKH);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        SendEmailNotification(user.Email, $"Cấp bậc mới: {updatedTichDiem.CapBac}");
                    }
                }

                return Json(new { success = true, diem = updatedTichDiem.Diem, capBac = updatedTichDiem.CapBac });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong CapNhatDiem: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Xóa điểm
        [HttpPost]
        public JsonResult XoaDiem(int maKH)
        {
            try
            {
                var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
                if (tichDiem == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin tích điểm." });

                var oldCapBac = tichDiem.CapBac;
                var diemThayDoi = -tichDiem.Diem;
                tichDiem.Diem = 0;
                tichDiem.NgayHetHan = null;

                // Ghi lịch sử
                db.LichSuTichDiems.Add(new LichSuTichDiem
                {
                    MaKH = maKH,
                    DiemThayDoi = diemThayDoi,
                    LyDo = "Xóa điểm bởi admin",
                    NgayGiaoDich = DateTime.Now,
                    TrangThaiDiem = "Đã sử dụng"
                });

                // Cập nhật cấp bậc
                db.Database.ExecuteSqlCommand("EXEC CapNhatCapBacTichDiem @MaKH", new SqlParameter("@MaKH", maKH));
                db.SaveChanges();

                var updatedTichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
                if (oldCapBac != updatedTichDiem.CapBac)
                {
                    var user = db.NguoiDungs.FirstOrDefault(u => u.MaKH == maKH);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        SendEmailNotification(user.Email, $"Cấp bậc mới: {updatedTichDiem.CapBac} (Điểm đã được xóa)");
                    }
                }

                return Json(new { success = true, message = "Điểm của khách hàng đã được xóa." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong XoaDiem: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Tích điểm từ đơn hàng
        [HttpPost]
        public JsonResult TichDiemTuDonHang(int maDonHang)
        {
            try
            {
                var donHang = db.DONHANGs.FirstOrDefault(dh => dh.MaDonHang == maDonHang && dh.DaThanhToan);
                if (donHang == null)
                    return Json(new { success = false, message = "Đơn hàng không hợp lệ hoặc chưa thanh toán." });

                var chiTietDonHang = db.CHITIETDATHANGs.Where(ct => ct.MaDonHang == maDonHang).ToList();
                decimal tongTien = chiTietDonHang.Sum(ct => (decimal)(ct.SoLuong * (ct.DonGia - (ct.GiamGia ?? 0))));
                int diemTich = (int)(tongTien / 100000); // 1 điểm cho mỗi 100.000 VNĐ

                if (diemTich == 0)
                    return Json(new { success = false, message = "Tổng tiền đơn hàng không đủ để tích điểm." });

                var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == donHang.MaKH);
                if (tichDiem == null)
                {
                    tichDiem = new TichDiem { MaKH = donHang.MaKH, Diem = 0, CapBac = "Không có cấp bậc", NgayHetHan = DateTime.Today.AddYears(1) };
                    db.TichDiems.Add(tichDiem);
                }

                // Kiểm tra điểm hết hạn
                if (tichDiem.NgayHetHan != null && tichDiem.NgayHetHan < DateTime.Today)
                {
                    db.LichSuTichDiems.Add(new LichSuTichDiem
                    {
                        MaKH = donHang.MaKH,
                        DiemThayDoi = -tichDiem.Diem,
                        LyDo = "Điểm hết hạn",
                        NgayGiaoDich = DateTime.Now,
                        TrangThaiDiem = "Hết hạn"
                    });
                    tichDiem.Diem = 0;
                    tichDiem.NgayHetHan = null;
                }

                var oldCapBac = tichDiem.CapBac;
                tichDiem.Diem += diemTich;
                tichDiem.NgayHetHan = DateTime.Today.AddYears(1); // Gia hạn 1 năm

                // Ghi lịch sử
                db.LichSuTichDiems.Add(new LichSuTichDiem
                {
                    MaKH = donHang.MaKH,
                    DiemThayDoi = diemTich,
                    LyDo = $"Tích điểm từ đơn hàng #{maDonHang}",
                    NgayGiaoDich = DateTime.Now,
                    TrangThaiDiem = "Còn hiệu lực"
                });

                // Cập nhật cấp bậc
                db.Database.ExecuteSqlCommand("EXEC CapNhatCapBacTichDiem @MaKH", new SqlParameter("@MaKH", donHang.MaKH));
                db.SaveChanges();

                var updatedTichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == donHang.MaKH);
                if (oldCapBac != updatedTichDiem.CapBac)
                {
                    var user = db.NguoiDungs.FirstOrDefault(u => u.MaKH == donHang.MaKH);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        SendEmailNotification(user.Email, $"Bạn đã tích {diemTich} điểm từ đơn hàng #{maDonHang}. Cấp bậc mới: {updatedTichDiem.CapBac}");
                    }
                }

                return Json(new { success = true, diem = updatedTichDiem.Diem, capBac = updatedTichDiem.CapBac });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong TichDiemTuDonHang: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Sử dụng điểm để giảm giá đơn hàng
        [HttpPost]
        public JsonResult SuDungDiem(int maDonHang, int diemSuDung)
        {
            try
            {
                var donHang = db.DONHANGs.FirstOrDefault(dh => dh.MaDonHang == maDonHang && !dh.DaThanhToan);
                if (donHang == null)
                    return Json(new { success = false, message = "Đơn hàng không hợp lệ hoặc đã thanh toán." });

                var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == donHang.MaKH);
                if (tichDiem == null || tichDiem.Diem < diemSuDung)
                    return Json(new { success = false, message = "Điểm không đủ để sử dụng." });

                // Kiểm tra điểm hết hạn
                if (tichDiem.NgayHetHan != null && tichDiem.NgayHetHan < DateTime.Today)
                {
                    db.LichSuTichDiems.Add(new LichSuTichDiem
                    {
                        MaKH = donHang.MaKH,
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

                // Tính giá trị giảm giá
                decimal giamGia = diemSuDung * 10000; // 1 điểm = 10.000 VNĐ
                var chiTietDonHang = db.CHITIETDATHANGs.Where(ct => ct.MaDonHang == maDonHang).ToList();
                decimal tongTien = chiTietDonHang.Sum(ct => (decimal)(ct.SoLuong * (ct.DonGia - (ct.GiamGia ?? 0))));
                if (giamGia > tongTien)
                    return Json(new { success = false, message = "Giảm giá vượt quá tổng tiền đơn hàng." });

                // Cập nhật giảm giá cho chi tiết đơn hàng
                chiTietDonHang[0].GiamGia = (chiTietDonHang[0].GiamGia ?? 0) + giamGia;

                var oldCapBac = tichDiem.CapBac;
                tichDiem.Diem -= diemSuDung;

                // Ghi lịch sử
                db.LichSuTichDiems.Add(new LichSuTichDiem
                {
                    MaKH = donHang.MaKH,
                    DiemThayDoi = -diemSuDung,
                    LyDo = $"Sử dụng điểm cho đơn hàng #{maDonHang}",
                    NgayGiaoDich = DateTime.Now,
                    TrangThaiDiem = "Đã sử dụng"
                });

                // Cập nhật cấp bậc
                db.Database.ExecuteSqlCommand("EXEC CapNhatCapBacTichDiem @MaKH", new SqlParameter("@MaKH", donHang.MaKH));
                db.SaveChanges();

                var updatedTichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == donHang.MaKH);
                if (oldCapBac != updatedTichDiem.CapBac)
                {
                    var user = db.NguoiDungs.FirstOrDefault(u => u.MaKH == donHang.MaKH);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        SendEmailNotification(user.Email, $"Cấp bậc mới: {updatedTichDiem.CapBac} (Sử dụng {diemSuDung} điểm)");
                    }
                }

                return Json(new { success = true, diem = updatedTichDiem.Diem, capBac = updatedTichDiem.CapBac });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong SuDungDiem: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Gửi email thông báo
        private void SendEmailNotification(string toEmail, string message, string qrCodeBase64 = null)
        {
            try
            {
                if (string.IsNullOrEmpty(toEmail)) return;

                var fromEmail = new MailAddress("moimoixanhla@gmail.com", "Quản lý Tích Điểm DOMDOM");
                var toAddress = new MailAddress(toEmail);
                var subject = "Thông báo tích điểm";
                var body = $@"<!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8' />
                </head>
                <body>
                    <h3>Chào bạn,</h3>
                    <p>{message}</p>
                    {(qrCodeBase64 != null ? $"<p>Đây là mã QR của bạn:</p><img src='{qrCodeBase64}' alt='Mã QR' style='max-width: 200px;' />" : "")}
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

        // Tạo mã QR
        private string GenerateQrCode(string text)
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

        [HttpPost]
        public JsonResult AwardBirthdayPoints()
        {
            try
            {
                var currentMonth = DateTime.Today.Month;
                var currentYear = DateTime.Today.Year;
                var birthdayUsers = db.NguoiDungs.Where(u => u.ThangSinh == currentMonth).ToList();
                var updatedUsers = new List<object>();
                var alreadyAwardedUsers = new List<object>();

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var user in birthdayUsers)
                        {
                            var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == user.MaKH);
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
                            }

                            // Kiểm tra nếu đã tặng điểm sinh nhật trong tháng này
                            var existingAward = db.LichSuTichDiems.Any(ls =>
                               ls.MaKH == user.MaKH &&
                               ls.LyDo == "Thưởng điểm sinh nhật" &&
                               ls.NgayGiaoDich.HasValue &&
                               ls.NgayGiaoDich.Value.Month == currentMonth &&
                               ls.NgayGiaoDich.Value.Year == currentYear);

                            if (existingAward)
                            {
                                alreadyAwardedUsers.Add(new
                                {
                                    MaKH = user.MaKH,
                                    TenKH = user.TenKH,
                                    Message = $"Đã tặng điểm sinh nhật cho khách hàng {user.TenKH} trong tháng này."
                                });
                                continue;
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
                            }

                            if (tichDiem.Diem >= 0)
                            {
                                var result = CapNhatDiem(user.MaKH, 100, "Thưởng điểm sinh nhật");
                                var jsonResult = result as JsonResult;
                                if (jsonResult != null)
                                {
                                    var data = jsonResult.Data as dynamic;
                                    if (data != null && data.success)
                                    {
                                        updatedUsers.Add(new
                                        {
                                            MaKH = user.MaKH,
                                            TenKH = user.TenKH,
                                            Diem = data.diem,
                                            CapBac = data.capBac
                                        });

                                        // Gửi email chúc mừng sinh nhật với mã QR
                                        if (!string.IsNullOrEmpty(user.Email))
                                        {
                                            string qrCodeText = $"MaKH: {user.MaKH} - DiemThuong: 100 - Ngay: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                                            string qrCodeBase64 = GenerateQrCode(qrCodeText);
                                            string emailMessage = $"Chúc mừng sinh nhật bạn {user.TenKH}!<br>" +
                                                                 $"Chúng tôi đã tặng bạn 100 điểm tích lũy như món quà sinh nhật.<br>" +
                                                             $"Tổng điểm hiện tại: {data.diem}. Cấp bậc: {data.capBac}.";
                                            SendEmailNotification(user.Email, emailMessage, qrCodeBase64);
                                        }
                                    }
                                }
                            }
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        string message = updatedUsers.Any() || alreadyAwardedUsers.Any()
                            ? "Hoàn tất xử lý điểm sinh nhật."
                            : "Không có khách hàng nào được tặng điểm sinh nhật.";

                        return Json(new
                        {
                            success = true,
                            message,
                            updatedUsers,
                            alreadyAwardedUsers
                        });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Lỗi trong AwardBirthdayPoints (transaction): {ex.Message}");
                        return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong AwardBirthdayPoints: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public JsonResult SendExpiryReminders()
        {
            try
            {
                var reminderDate = DateTime.Today.AddDays(30);
                var expiringPoints = db.TichDiems
                    .Where(td => td.NgayHetHan <= reminderDate && td.NgayHetHan >= DateTime.Today)
                    .ToList();

                foreach (var tichDiem in expiringPoints)
                {
                    var user = db.NguoiDungs.FirstOrDefault(u => u.MaKH == tichDiem.MaKH);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        SendEmailNotification(user.Email, $"Điểm của bạn ({tichDiem.Diem} điểm) sẽ hết hạn vào {tichDiem.NgayHetHan:dd/MM/yyyy}. Hãy sử dụng ngay!");
                    }
                }
                return Json(new { success = true, message = "Đã gửi thông báo hết hạn." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong SendExpiryReminders: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public JsonResult GetTierBenefits(int maKH)
        {
            var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
            if (tichDiem == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin tích điểm." }, JsonRequestBehavior.AllowGet);

            var allBenefits = new Dictionary<string, string[]>
        {
            {"Vàng", new[] {"Mã giảm giá 10%", "Hỗ trợ ưu tiên"}},
            {"Bạc", new[] {"Mã giảm giá 5%"}},
            {"Đồng", new[] {"Miễn phí vận chuyển cho đơn hàng đầu tiên"}}
        };

            string[] benefits;
            if (!allBenefits.TryGetValue(tichDiem.CapBac, out benefits))
            {
                benefits = new[] { "Không có lợi ích" };
            }

            return Json(new { success = true, capBac = tichDiem.CapBac, benefits }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LichSuTichDiem(int maKH, DateTime? tuNgay, DateTime? denNgay, string trangThai, string lyDo)
        {
            var lichSu = db.LichSuTichDiems
                .Where(ls => ls.MaKH == maKH)
                .AsQueryable();

            if (tuNgay.HasValue)
                lichSu = lichSu.Where(ls => ls.NgayGiaoDich >= tuNgay.Value);
            if (denNgay.HasValue)
                lichSu = lichSu.Where(ls => ls.NgayGiaoDich <= denNgay.Value.AddDays(1));
            if (!string.IsNullOrEmpty(trangThai))
                lichSu = lichSu.Where(ls => ls.TrangThaiDiem == trangThai);
            if (!string.IsNullOrEmpty(lyDo))
                lichSu = lichSu.Where(ls => ls.LyDo.Contains(lyDo));

            lichSu = lichSu.OrderByDescending(ls => ls.NgayGiaoDich);

            ViewBag.TenKH = db.NguoiDungs.FirstOrDefault(u => u.MaKH == maKH)?.TenKH;
            ViewBag.MaKH = maKH;
            ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
            ViewBag.TrangThai = trangThai;
            ViewBag.LyDo = lyDo;
            ViewBag.TichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);
            ViewBag.ThangSinh = db.NguoiDungs.FirstOrDefault(u => u.MaKH == maKH)?.ThangSinh;

            return View(lichSu.ToList());
        }
    }

    // ViewModel để hiển thị thông tin
    public class UserPointViewModel
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string Email { get; set; }
        public int? ThangSinh { get; set; }
        public string SoDienThoai { get; set; }
        public int Diem { get; set; }
        public string CapBac { get; set; }
        public DateTime? NgayHetHan { get; set; }
    }
}