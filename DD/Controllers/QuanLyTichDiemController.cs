using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{

    public class QuanLyTichDiemController : Controller
    {
        camlyEntities1 db = new camlyEntities1();

        // GET: QuanLyTichDiem
        public ActionResult Index()
        {
            // Fetch the data with updated CapBac from the database
            var result = from user in db.NguoiDungs
                         join point in db.TichDiems on user.MaKH equals point.MaKH into points
                         from point in points.DefaultIfEmpty() // Thực hiện LEFT JOIN
                         select new UserPointViewModel
                         {
                             MaKH = user.MaKH,
                             TenKH = user.TenKH,
                             Email = user.Email,
                             ThangSinh = user.ThangSinh,
                             Diem = point != null ? point.Diem : 0,
                             CapBac = point != null ? point.CapBac : "Chưa có cấp bậc", // Lấy từ DB thay vì tính lại
                             SoDienThoai = user.SoDienThoai
                         };

            // Convert the result to a list
            var userPoints = result.ToList();

            return View(userPoints);
        }

        public string CalculateCapBac(int diem)
        {
            if (diem >= 1000) return "VIP";
            if (diem >= 500) return "Kim cương";
            if (diem >= 150) return "Vàng";
            if (diem >= 50) return "Bạc";
            if (diem >= 30) return "Đồng";
            return "Chưa có cấp bậc";
        }

        [HttpPost]
        public JsonResult CapThanhVien(int maKH)
        {
            // Get the user's points from TichDiem
            var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);

            if (tichDiem != null)
            {
                // Automatically determine CapBac based on Diem
                tichDiem.CapBac = CalculateCapBac(tichDiem.Diem);

                // Save the changes to the database
                db.SaveChanges();

                return Json(new { success = true, capBac = tichDiem.CapBac });
            }

            return Json(new { success = false });
        }


        [HttpPost]
        public JsonResult UpdatePoint(int maKH, int diem)
        {
            var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);

            if (tichDiem != null)
            {
                var oldCapBac = tichDiem.CapBac;
                tichDiem.Diem = diem;
                tichDiem.CapBac = CalculateCapBac(diem); // Cập nhật cấp bậc

                // Save changes to database
                db.SaveChanges();

                // Check if the CapBac was updated in the database
                var updatedTichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);

                if (updatedTichDiem != null && updatedTichDiem.CapBac != oldCapBac)
                {
                    // Nếu cấp bậc thay đổi, gửi email thông báo
                    var user = db.NguoiDungs.FirstOrDefault(u => u.MaKH == maKH);
                    if (user != null)
                    {
                        SendEmailNotification(user.Email, updatedTichDiem.CapBac);
                    }
                }

                return Json(new { success = true, capBac = updatedTichDiem.CapBac });
            }

            return Json(new { success = false });
        }

        private void SendEmailNotification(string toEmail, string newCapBac)
        {
            try
            {
                var fromEmail = new MailAddress("noreply@yourdomain.com", "Quản lý Tích Điểm");
                var toAddress = new MailAddress(toEmail);
                var subject = "Thông báo cấp bậc mới";
                var body = $"Chúc mừng bạn! Bạn đã đạt được cấp bậc mới: {newCapBac}. Cảm ơn bạn đã tham gia chương trình tích điểm của chúng tôi.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.yourmailserver.com",
                    Port = 587,
                    Credentials = new NetworkCredential("2224802010596@student.tdmu.edu.vn", "lqci gzzj tvaq ocss"),
                    EnableSsl = true
                };

                using (var message = new MailMessage(fromEmail, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Ghi lỗi vào log hoặc hiển thị thông báo lỗi
                System.Diagnostics.Debug.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        // Thêm phương thức để xóa điểm của người dùng
        [HttpPost]
        public JsonResult XoaDiem(int maKH)
        {
            var tichDiem = db.TichDiems.FirstOrDefault(td => td.MaKH == maKH);

            if (tichDiem != null)
            {
                // Lưu lại cấp bậc cũ trước khi xóa điểm
                var oldCapBac = tichDiem.CapBac;

                // Xóa điểm (giảm điểm về 0)
                tichDiem.Diem = 0;
                tichDiem.CapBac = "Chưa có cấp bậc"; // Reset cấp bậc

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                // Nếu cấp bậc đã thay đổi, gửi email thông báo
                if (tichDiem.CapBac != oldCapBac)
                {
                    var user = db.NguoiDungs.FirstOrDefault(u => u.MaKH == maKH);
                    if (user != null)
                    {
                        SendEmailNotification(user.Email, tichDiem.CapBac);
                    }
                }

                return Json(new { success = true, message = "Điểm của người dùng đã được xóa." });
            }

            return Json(new { success = false, message = "Không tìm thấy người dùng hoặc thông tin tích điểm." });
        }


    }
}