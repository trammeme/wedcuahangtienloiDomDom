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
        camlyEntities1 db = new camlyEntities1  ();

        public ActionResult Index()
        {
            var users = db.NguoiDungs.ToList();

            return View(users);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var kq = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (kq == null)
            {
                return HttpNotFound(); 
            }
            return PartialView("Edit", kq); 
        }

        [HttpPost]
        public ActionResult Edit(FormCollection f)
        {
            if (int.TryParse(f["maKH"], out int maKH)) 
            {
                NguoiDung kq = db.NguoiDungs.SingleOrDefault(n => n.MaKH == maKH);

                if (kq != null)
                {
                    kq.TenDangNhap = f["tenKH"];
                    kq.Email = f["email"];
                    kq.GioiTinh = f["gioitinh"];
                    kq.DiaChi = f["diachi"];
                    kq.SoDienThoai = f["sodienthoai"];
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Detail(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user == null)
            {
                return HttpNotFound(); 
            }
            return View(user); 
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user == null)
            {
                return HttpNotFound(); 
            }
            return PartialView("Delete", user); 
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user != null)
            {
                db.NguoiDungs.Remove(user); 
                db.SaveChanges(); 
            }
            return RedirectToAction("Index"); 
        }

        private void SendBirthdayEmail(NguoiDung user)
        {
            try
            {
                string fromMail = "2224802010596@student.tdmu.edu.vn"; 
                string fromPassword = "lqci gzzj tvaq ocss"; 

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
                System.Diagnostics.Debug.WriteLine($"Error sending email to {user.Email}: {ex.Message}");
            }
        }

        private string GetBirthdayEmailTemplate(NguoiDung user)
        {
            string voucherCode = $"HPBD{user.MaKH}{DateTime.Now.Year}";

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

