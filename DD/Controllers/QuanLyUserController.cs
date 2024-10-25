using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class QuanLyUserController : Controller
    {
        thuaEntities1 db = new thuaEntities1();

        // GET: QuanLyUser
        public ActionResult Index()
        {
            // Lấy danh sách người dùng từ bảng NguoiDungs
            var users = db.NguoiDungs.ToList();

            // Truyền danh sách người dùng tới view
            return View(users);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            // Lấy thông tin người dùng dựa trên MaKH
            var kq = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (kq == null)
            {
                return HttpNotFound(); // Xử lý nếu không tìm thấy người dùng
            }
            return PartialView("Edit", kq); // Trả về một partial view
        }

        [HttpPost]
        public ActionResult Edit(FormCollection f)
        {
            if (int.TryParse(f["maKH"], out int maKH)) // Chuyển đổi maKH thành int
            {
                NguoiDung kq = db.NguoiDungs.SingleOrDefault(n => n.MaKH == maKH);

                if (kq != null)
                {
                    // Cập nhật thông tin người dùng
                    kq.TenDangNhap = f["tenKH"];
                    kq.Email = f["email"];
                    kq.GioiTinh = f["gioitinh"];
                    kq.DiaChi = f["diachi"];
                    kq.SoDienThoai = f["sodienthoai"];
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            // Nếu chuyển đổi không thành công, bạn có thể xử lý ở đây
            return View();
        }

        [HttpGet]
        public ActionResult Detail(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user == null)
            {
                return HttpNotFound(); // Xử lý nếu không tìm thấy người dùng
            }
            return View(user); // Trả về view chi tiết
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user == null)
            {
                return HttpNotFound(); // Xử lý nếu không tìm thấy người dùng
            }
            return PartialView("Delete", user); // Trả về partial view để xác nhận xóa
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user != null)
            {
                db.NguoiDungs.Remove(user); // Xóa người dùng
                db.SaveChanges(); // Lưu thay đổi
            }
            return RedirectToAction("Index"); // Quay lại danh sách
        }

        private void SendBirthdayEmail(NguoiDung user)
        {
            try
            {
                // Cấu hình email
                string fromMail = "2224802010596@student.tdmu.edu.vn"; // Email của bạn
                string fromPassword = "lqci gzzj tvaq ocss"; // Mật khẩu ứng dụng của Gmail

                MailMessage message = new MailMessage
                {
                    From = new MailAddress(fromMail),
                    Subject = $"Chúc mừng sinh nhật {user.TenKH}!",
                    Body = GetBirthdayEmailTemplate(user),
                    IsBodyHtml = true
                };
                message.To.Add(new MailAddress(user.Email));

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                System.Diagnostics.Debug.WriteLine($"Error sending email to {user.Email}: {ex.Message}");
            }
        }

        private string GetBirthdayEmailTemplate(NguoiDung user)
        {
            // Tạo mã voucher unique cho từng user
            string voucherCode = $"HPBD{user.MaKH}{DateTime.Now.Year}";

            // HTML template cho email
            StringBuilder emailBody = new StringBuilder();
            emailBody.AppendLine("<h1>Chúc mừng sinh nhật!</h1>");
            emailBody.AppendLine($"<p>Xin chào {user.TenKH},</p>");
            emailBody.AppendLine("<p>Chúc bạn một ngày sinh nhật thật vui vẻ!</p>");
            emailBody.AppendLine($"<p>Mã voucher của bạn: <strong>{voucherCode}</strong></p>");
            emailBody.AppendLine("<p>Cảm ơn bạn đã đồng hành cùng chúng tôi!</p>");
            emailBody.AppendLine("<p>Trân trọng,</p>");
            emailBody.AppendLine("<p>Đội ngũ hỗ trợ khách hàng</p>");

            return emailBody.ToString();
        }

        // Thêm action để test gửi email cho một user cụ thể (để debug)
        public ActionResult TestBirthdayEmail(int id)
        {
            var user = db.NguoiDungs.Find(id);
            if (user != null)
            {
                SendBirthdayEmail(user);
                TempData["Message"] = "Đã gửi email test thành công.";
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult SendBirthdayWishes(int id)
        {
            var user = db.NguoiDungs.Find(id);
            if (user != null)
            {
                SendBirthdayEmail(user);
                TempData["Message"] = "Đã gửi email chúc mừng sinh nhật thành công.";
            }
            return RedirectToAction("Index");
        }


    }

}

