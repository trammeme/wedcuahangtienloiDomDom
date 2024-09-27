using System;
using System.Data.SqlClient;
using System.Linq;
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

        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string maKH = GenerateMaKH(); // Gọi phương thức để tạo maKH

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "INSERT INTO NguoiDung (maKH, tenKH, email, gioiTinh, thangSinh, diaChi, soDienThoai, tenDangNhap, matKhau) " +
                                       "VALUES (@MaKH, @TenKH, @Email, @GioiTinh, @ThangSinh, @DiaChi, @SoDienThoai, @TenDangNhap, @MatKhau)";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@MaKH", maKH);
                        cmd.Parameters.AddWithValue("@TenKH", model.TenKH);
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@GioiTinh", model.GioiTinh);
                        cmd.Parameters.AddWithValue("@ThangSinh", model.ThangSinh);
                        cmd.Parameters.AddWithValue("@DiaChi", model.DiaChi);
                        cmd.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
                        cmd.Parameters.AddWithValue("@TenDangNhap", model.TenDangNhap);
                        cmd.Parameters.AddWithValue("@MatKhau", model.MatKhau);

                        cmd.ExecuteNonQuery();
                    }

                    TempData["SuccessMessage"] = "Tài khoản đã được đăng ký thành công!"; // Lưu thông báo thành công

                    return RedirectToAction("Login", "Account"); // Chuyển hướng về trang Login
                }
                catch (SqlException sqlEx)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + sqlEx.Message);
                }
            }

            return View(model); 
        }


        public string GenerateMaKH()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM NguoiDung";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                int recordCount = (int)checkCmd.ExecuteScalar();

                if (recordCount == 0)
                {
                    
                    return "KH001";
                }

                
                string query = "SELECT MAX(CAST(SUBSTRING(maKH, 3, LEN(maKH) - 2) AS INT)) " +
                               "FROM NguoiDung WHERE LEN(maKH) > 2"; 
                SqlCommand cmd = new SqlCommand(query, conn);
                object result = cmd.ExecuteScalar();

               
                int newMaKH = (result != DBNull.Value ? (int)result : 0) + 1;

                return "KH" + newMaKH.ToString("D3"); 
            }
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
