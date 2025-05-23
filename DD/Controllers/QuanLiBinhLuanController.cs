using DD.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace DD.Controllers
{
    public class QuanLiBinhLuanController : Controller
    {
        private sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();

        // Hiển thị danh sách bình luận


        public ActionResult Index(string filterDate)
        {
            DateTime? filterDateTime = string.IsNullOrEmpty(filterDate) ? (DateTime?)null : DateTime.Parse(filterDate);

            var query = from bl in db.BinhLuans
                        join kh in db.NguoiDungs on bl.MaKH equals kh.MaKH
                        where !filterDateTime.HasValue ||
                              DbFunctions.TruncateTime(bl.NgayBinhLuan) == DbFunctions.TruncateTime(filterDateTime)
                        select new BinhLuanViewModel
                        {
                            MaBinhLuan = bl.MaBinhLuan,
                            MaSanPham = bl.MaSanPham,
                            MaKH = bl.MaKH,
                            TenKhachHang = kh.TenKH,
                            Email = kh.Email,
                            NoiDung = bl.NoiDung,
                            DanhGia = bl.DanhGia,
                            NgayBinhLuan = bl.NgayBinhLuan,
                            PhanHoi = bl.PhanHoi,
                            NgayPhanHoi = bl.NgayPhanHoi
                        };

            var binhLuanList = query.ToList();

            ViewBag.SoBinhLuanMoi = binhLuanList.Count(bl => string.IsNullOrEmpty(bl.PhanHoi));
            ViewBag.FilterDate = filterDate;

            return View(binhLuanList);
        }





        // Các action khác như Reply, Create, v.v.


        public JsonResult GetSoBinhLuanMoi()
        {
            int soBinhLuanMoi = db.BinhLuans
                .Count(bl => string.IsNullOrEmpty(bl.PhanHoi));
            return Json(new { SoBinhLuanMoi = soBinhLuanMoi }, JsonRequestBehavior.AllowGet);
        }

        // Hiển thị form thêm bình luận
        public ActionResult Create()
        {
            // Lấy danh sách khách hàng và sản phẩm để chọn (nếu cần)
            ViewBag.MaKH = new SelectList(db.NguoiDungs, "MaKH", "TenKH");
            ViewBag.MaSanPham = new SelectList(db.SanPhams, "MaSanPham", "TenSanPham");
            return View();
        }

        // Xử lý thêm bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BinhLuan binhLuan)
        {
            if (ModelState.IsValid)
            {
                binhLuan.NgayBinhLuan = DateTime.Now;
                db.BinhLuans.Add(binhLuan);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MaKH = new SelectList(db.NguoiDungs, "MaKH", "TenKH", binhLuan.MaKH);
            ViewBag.MaSanPham = new SelectList(db.SanPhams, "MaSanPham", "TenSanPham", binhLuan.MaSanPham);
            return View(binhLuan);
        }

        // Hiển thị form phản hồi bình luận
        public ActionResult Reply(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            BinhLuan binhLuan = db.BinhLuans.Find(id);
            if (binhLuan == null)
            {
                return HttpNotFound();
            }
            return View(binhLuan); // Trả về một BinhLuan, khớp với @model BinhLuan
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reply(int id, BinhLuan binhLuan)
        {
            if (ModelState.IsValid)
            {
                BinhLuan bl = db.BinhLuans.Find(id);
                if (bl != null)
                {
                    bl.PhanHoi = binhLuan.PhanHoi;
                    bl.AdminID = 1.ToString(); // Giả sử AdminID = 1 (cần thay bằng logic xác thực admin)
                    bl.NgayPhanHoi = DateTime.Now;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(binhLuan);
        }
        // Hủy bỏ (nếu cần)
       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int maBinhLuan, string lyDoXoa)
        {
            try
            {
                var binhLuan = db.BinhLuans.Find(maBinhLuan);
                if (binhLuan == null)
                {
                    TempData["Error"] = "Không tìm thấy bình luận để xóa.";
                    return RedirectToAction("Index");
                }

                var nguoiDung = db.NguoiDungs.FirstOrDefault(nd => nd.MaKH == binhLuan.MaKH);
                var sanPham = db.SanPhams.FirstOrDefault(sp => sp.MaSanPham == binhLuan.MaSanPham);

                if (nguoiDung == null || string.IsNullOrEmpty(nguoiDung.Email))
                {
                    TempData["Error"] = "Không tìm thấy email của khách hàng để gửi thông báo.";
                    return RedirectToAction("Index");
                }

                if (sanPham == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin sản phẩm.";
                    return RedirectToAction("Index");
                }

                // Gửi email thông báo (viết trực tiếp)
                try
                {
                    var fromEmail = "moimoixanhla@gmail.com"; // thay bằng email thật
                    var fromPassword = "hvyt bzxm qzti esuo"; // thay bằng mật khẩu ứng dụng

                    var smtpClient = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential(fromEmail, fromPassword),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "Hệ thống DOMDOM"),
                        Subject = "Thông báo: Bình luận của bạn đã bị xóa",
                        Body = $"<h3>Xin chào {nguoiDung.TenKH},</h3>" +
                               $"<p>Bình luận của bạn về sản phẩm <strong>{sanPham.TenSanPham}</strong> đã bị xóa.</p>" +
                               $"<p><strong>Nội dung bình luận:</strong> {binhLuan.NoiDung}</p>" +
                               $"<p><strong>Lý do xóa:</strong> {lyDoXoa}</p>" +
                               $"<p>Nếu bạn có thắc mắc, vui lòng liên hệ với chúng tôi.</p>" +
                               $"<p>Trân trọng,<br/>Hệ thống DOMDOM</p>",
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(nguoiDung.Email);

                    smtpClient.Send(mailMessage);
                }
                catch (SmtpException smtpEx)
                {
                    TempData["Error"] = $"Lỗi SMTP: {smtpEx.Message}. Mã lỗi: {smtpEx.StatusCode}";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi gửi email: {ex.Message}";
                }

                db.BinhLuans.Remove(binhLuan);
                db.SaveChanges();

                TempData["Success"] = "Đã xóa bình luận và gửi email thông báo cho khách hàng.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra khi xóa bình luận: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
