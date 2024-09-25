using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DD.Models;
namespace DD.Controllers
{
    public class AccountController : Controller
    {


        private readonly string connectionString = "Data Source=MINU\\SQLEXPRESS;Initial Catalog=Dom1;Integrated Security=True;";

        // GET: Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string maKH = GenerateMaKH(); // Gọi phương thức để tạo mã KH
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO NguoiDung (maKH, tenKH, email, gioiTinh, thangSinh, diaChi, soDienThoai, tenDangNhap, matKhau) " +
                                   "VALUES (@MaKH, @TenKH, @Email, @GioiTinh, @ThangSinh, @DiaChi, @SoDienThoai, @TenDangNhap, @MatKhau)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaKH", maKH); // Thêm mã KH
                    cmd.Parameters.AddWithValue("@TenKH", model.TenKH);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@GioiTinh", model.GioiTinh);
                    cmd.Parameters.AddWithValue("@ThangSinh", model.ThangSinh);
                    cmd.Parameters.AddWithValue("@DiaChi", model.DiaChi);
                    cmd.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
                    cmd.Parameters.AddWithValue("@TenDangNhap", model.TenDangNhap);
                    cmd.Parameters.AddWithValue("@MatKhau", model.MatKhau);  // Hash mật khẩu nếu cần thiết

                    cmd.ExecuteNonQuery();
                    TempData["SuccessMessage"] = "Đăng ký thành công!";
                    return RedirectToAction("Login", "Account");
                }
            }
            return View(model);
        }

        private string GenerateMaKH()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM NguoiDung";
                SqlCommand cmd = new SqlCommand(query, conn);
                int count = (int)cmd.ExecuteScalar();
                return $"KH{count + 1:D3}"; // Tạo mã KH001, KH002, ...
            }
        }

        // GET: Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra trong bảng QuanTriVien
                    string queryAdmin = "SELECT COUNT(*) FROM QuanTriVien WHERE tendangnhap = @Username AND matkhau = @Password";
                    SqlCommand cmdAdmin = new SqlCommand(queryAdmin, conn);
                    cmdAdmin.Parameters.AddWithValue("@Username", model.Username);
                    cmdAdmin.Parameters.AddWithValue("@Password", model.Password);

                    int adminExists = (int)cmdAdmin.ExecuteScalar();
                    if (adminExists > 0)
                    {
                        TempData["SuccessMessage"] = "Đăng nhập thành công với tư cách quản trị viên!";
                        return RedirectToAction("Index", "Home");
                    }

                    // Kiểm tra trong bảng NguoiDung
                    string queryUser = "SELECT COUNT(*) FROM NguoiDung WHERE tendangnhap = @Username AND matkhau = @Password";
                    SqlCommand cmdUser = new SqlCommand(queryUser, conn);
                    cmdUser.Parameters.AddWithValue("@Username", model.Username);
                    cmdUser.Parameters.AddWithValue("@Password", model.Password);

                    int userExists = (int)cmdUser.ExecuteScalar();
                    if (userExists > 0)
                    {
                        TempData["SuccessMessage"] = "Đăng nhập thành công với tư cách người dùng!";
                        return RedirectToAction("KhuyenMai", "KhuyenMai");
                    }

                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }
            return View(model);
        }
    }
}
