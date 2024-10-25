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
        private readonly thuaEntities1 db = new thuaEntities1();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult Profile()
        {
            // Giả sử bạn đã lưu ID của người sử dụng trong Session khi họ đăng nhập  
            var user = (NguoiDung)Session["User"];
            if (user != null)
            {
                // Trả về thông tin người dùng cho view  
                return View(user);
            }

            // Nếu không có thông tin người dùng trong session, chuyển hướng về trang đăng nhập  
            return RedirectToAction("Login");
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem email đã tồn tại chưa
                if (db.NguoiDungs.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng nhập email khác.");
                    return View(user);
                }

                // Mã hóa mật khẩu trước khi lưu
                user.MatKhau = HashPassword(user.MatKhau); // Giả định bạn có hàm HashPassword

                // Tạo đối tượng NguoiDung
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

                // Tạo mã QR cho người dùng
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
                // Kiểm tra tài khoản quản trị viên
                if (db.QuanTriViens.Any(a => a.TenDangNhap == model.Username && a.MatKhau == model.Password))
                {
                    return RedirectToAction("Index", "Admin");
                }

                // Kiểm tra người dùng trong cơ sở dữ liệu
                var user = db.NguoiDungs.AsNoTracking().FirstOrDefault(u => u.TenDangNhap == model.Username && u.MatKhau == model.Password);

                if (user != null)
                {
                    // Thiết lập phiên người dùng
                    Session["User"] = user;
                    Session["TenKhachHang"] = user.TenKH;

                    // Tạo QR code chỉ nếu chưa tồn tại
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

            // Trả về view với ModelState có lỗi nếu không hợp lệ
            return View(model);
        }

  
    public ActionResult Logout()
        {
            Session.Clear(); // Xóa tất cả session
            TempData["Message"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Login", "Acccount");
        }

        private string HashPassword(string password)
        {
            // Thêm mã hóa cho mật khẩu (sử dụng BCrypt.Net hoặc tương tự)
            // ví dụ: return BCrypt.Net.BCrypt.HashPassword(password);
            return password; // Thay thế bằng phương thức mã hóa thực
        }

    }

}
