using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using DD.Models;

namespace DD.Controllers
{
    public class AccountController : Controller
    {
        private string connectionString = "Data Source=MINU\\SQLEXPRESS;Initial Catalog=luck;Integrated Security=True;";

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra xem email đã tồn tại chưa
                    SqlCommand checkEmailCmd = new SqlCommand("SELECT COUNT(*) FROM NguoiDung WHERE Email = @Email", conn);
                    checkEmailCmd.Parameters.AddWithValue("@Email", user.Email);
                    int emailExists = (int)checkEmailCmd.ExecuteScalar();

                    if (emailExists > 0)
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng nhập email khác.");
                        return View(user);
                    }

                    SqlCommand cmd = new SqlCommand("INSERT INTO NguoiDung (TenKH, Email, DiaChi, ThangSinh, GioiTinh, SoDienThoai, TenDangNhap, MatKhau) VALUES (@TenKH, @Email, @DiaChi, @ThangSinh, @GioiTinh, @SoDienThoai, @TenDangNhap, @MatKhau)", conn);

                    cmd.Parameters.AddWithValue("@TenKH", user.TenKH);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@DiaChi", user.DiaChi);
                    cmd.Parameters.AddWithValue("@ThangSinh", user.ThangSinh); // Đảm bảo giá trị là kiểu int
                    cmd.Parameters.AddWithValue("@GioiTinh", user.GioiTinh); // Truyền vào giá trị là "Nam" hoặc "Nữ"
                    cmd.Parameters.AddWithValue("@SoDienThoai", user.SoDienThoai);
                    cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                    cmd.Parameters.AddWithValue("@MatKhau", user.MatKhau); // Nên mã hóa mật khẩu


                    cmd.ExecuteNonQuery();
                }

                TempData["Message"] = "Đăng ký thành công!";
                return RedirectToAction("Index", "User"); // Chuyển đến trang User
            }
            return View(user);
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string queryAdmin = "SELECT COUNT(*) FROM QuanTriVien WHERE tendangnhap = @Username AND matkhau = @Password";
                    SqlCommand cmdAdmin = new SqlCommand(queryAdmin, conn);
                    cmdAdmin.Parameters.AddWithValue("@Username", model.Username);
                    cmdAdmin.Parameters.AddWithValue("@Password", model.Password); 

                    int adminExists = (int)cmdAdmin.ExecuteScalar();
                    if (adminExists > 0)
                    {
                        return RedirectToAction("Index", "Admin"); 
                    }

                    string queryUser = "SELECT COUNT(*) FROM NguoiDung WHERE tendangnhap = @Username AND matkhau = @Password";
                    SqlCommand cmdUser = new SqlCommand(queryUser, conn);
                    cmdUser.Parameters.AddWithValue("@Username", model.Username);
                    cmdUser.Parameters.AddWithValue("@Password", model.Password); 

                    int userExists = (int)cmdUser.ExecuteScalar();
                    if (userExists > 0)
                    {
                        TempData["SuccessMessage"] = "Đăng nhập thành công với tư cách người dùng!";
                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
                }
            }
            return View(model);
        }
    }
}
