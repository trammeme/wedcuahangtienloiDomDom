using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DD.Models;
using QRCoder;

namespace DD.Controllers
{
    public class AccountController : Controller
    {
        private readonly camlyEntities1 db = new camlyEntities1();
      

        public ActionResult Register()
        {
            return View();
        }
        public ActionResult Profile()
        {
            var user = (NguoiDung)Session["User"];
            if (user != null)
            {
                return View(user);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                if (db.NguoiDungs.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng nhập email khác.");
                    return View(user);
                }

                user.MatKhau = HashPassword(user.MatKhau); 
                var nguoiDung = new NguoiDung
                {
                    TenDangNhap = user.TenDangNhap,
                    MatKhau = user.MatKhau,
                    Email = user.Email,
                    ThangSinh = user.ThangSinh,
                    DiaChi = user.DiaChi,
                    SoDienThoai = user.SoDienThoai,
                    GioiTinh = user.GioiTinh,
                    TenKH = user.TenKH,
                };

                db.NguoiDungs.Add(nguoiDung);
                db.SaveChanges();

                string customerData = nguoiDung.Email;
                var qrCodeImage = GenerateQRCode(customerData);
                SaveQRCodeImage(nguoiDung.TenDangNhap, qrCodeImage);

                TempData["Message"] = "Đăng ký thành công mời bạn đăng nhập!";
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        private Bitmap GenerateQRCode(string customerData)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(customerData, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                return qrCode.GetGraphic(20);
            }
        }

        private string SaveQRCodeImage(string username, Bitmap qrCodeImage)
        {
            var directoryPath = Server.MapPath("~/wwwroot/qrcodes");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var fileName = $"{username}.png";
            var filePath = Path.Combine(directoryPath, fileName);
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                qrCodeImage.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
            }

            return filePath;
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.QuanTriViens.Any(a => a.TenDangNhap == model.Username && a.MatKhau == model.Password))
                {
                    return RedirectToAction("Index", "Admin");
                }

                var user = db.NguoiDungs.AsNoTracking().FirstOrDefault(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);

                if (user != null)
                {
                    Session["User"] = user;
                    Session["TenKhachHang"] = user.TenKH;
                    Session["Email"] = user.Email;
                    Session["PhoneNumber"] = user.SoDienThoai;
                    Session["ThangSinh"] = user.ThangSinh;
                    Session["GioiTinh"] = user.GioiTinh;
                    // Tạo QR code chỉ nếu chưa tồn tại
                    // Lấy điểm tích lũy từ bảng TichDiem
                    var tichDiem = db.TichDiems.AsNoTracking().FirstOrDefault(td => td.MaKH == user.MaKH);
                    if (tichDiem != null)
                    {
                        Session["Diem"] = tichDiem.Diem;
                        Session["CapBac"] = tichDiem.CapBac;

                    }
                    else
                    {
                        Session["Diem"] = 0; // Nếu không có điểm tích lũy, đặt mặc định là 0
                    }
                    string qrCodeFileName = $"{user.TenDangNhap}.png";
                    Session["QRCodePath"] = qrCodeFileName;

                    var qrCodePath = Server.MapPath($"~/wwwroot/qrcodes/{qrCodeFileName}");
                    if (!System.IO.File.Exists(qrCodePath))
                    {
                        var qrCodeImage = GenerateQRCode(user.Email); // Giả sử GenerateQRCode là phương thức tạo QR code
                        SaveQRCodeImage(user.TenDangNhap, qrCodeImage); // Lưu QR code
                    }

                    TempData["SuccessMessage"] = "Đăng nhập thành công!";
                    return RedirectToAction("Index", "Home");
                }

                // Nếu không tìm thấy người dùng, thêm lỗi vào ModelState
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }


        public ActionResult Logout()
        {
            Session.Clear();
            TempData["Message"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {

            return password;

        }

    }
}
