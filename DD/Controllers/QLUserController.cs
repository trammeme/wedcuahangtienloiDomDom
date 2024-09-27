using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using DD.Models;

namespace DD.Controllers
{
    public class QLUserController : Controller
    {
        private readonly string connectionString = "Data Source=MINU\\SQLEXPRESS;Initial Catalog=csdlDom;Integrated Security=True;";

        public ActionResult UserManagement()
        {
            List<QLUserViewModel> users = new List<QLUserViewModel>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM NguoiDung";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            users.Add(new QLUserViewModel
                            {
                                MaKH = reader["maKH"].ToString(),
                                TenKH = reader["tenKH"].ToString(),
                                Email = reader["email"].ToString(),
                                GioiTinh = reader["gioiTinh"].ToString(),
                                ThangSinh = reader["thangSinh"] != DBNull.Value ? (int)reader["thangSinh"] : 0,
                                DiaChi = reader["diaChi"].ToString(),
                                SoDienThoai = reader["soDienThoai"].ToString(),
                                TenDangNhap = reader["tenDangNhap"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Có lỗi xảy ra: " + ex.Message);
                    return new HttpStatusCodeResult(500, "Lỗi kết nối cơ sở dữ liệu");
                }
            }

            return View(users);
        }

        public ActionResult Edit(string maKH)
        {
            QLUserViewModel user = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM NguoiDung WHERE maKH = @MaKH";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaKH", maKH);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    user = new QLUserViewModel
                    {
                        MaKH = maKH,
                        TenKH = reader["tenKH"].ToString(),
                        Email = reader["email"].ToString(),
                        GioiTinh = reader["gioiTinh"].ToString(),
                        ThangSinh = reader["thangSinh"] != DBNull.Value ? (int)reader["thangSinh"] : 0,
                        DiaChi = reader["diaChi"].ToString(),
                        SoDienThoai = reader["soDienThoai"].ToString(),
                        TenDangNhap = reader["tenDangNhap"].ToString()
                    };
                }
            }

            if (user == null)
            {
                return HttpNotFound(); 
            }

            return Json(user, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(QLUserViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE NguoiDung SET tenKH = @TenKH, email = @Email, gioiTinh = @GioiTinh, thangSinh = @ThangSinh, diaChi = @DiaChi, soDienThoai = @SoDienThoai, tenDangNhap = @TenDangNhap WHERE maKH = @MaKH";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaKH", user.MaKH);
                    cmd.Parameters.AddWithValue("@TenKH", user.TenKH);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@GioiTinh", user.GioiTinh);
                    cmd.Parameters.AddWithValue("@ThangSinh", user.ThangSinh);
                    cmd.Parameters.AddWithValue("@DiaChi", user.DiaChi);
                    cmd.Parameters.AddWithValue("@SoDienThoai", user.SoDienThoai);
                    cmd.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true });
            }
            catch (SqlException sqlEx)
            {
                return Json(new { success = false, message = sqlEx.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string maKH)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM NguoiDung WHERE maKH = @MaKH";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaKH", maKH);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Người dùng đã được xóa thành công!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Không tìm thấy người dùng để xóa." });
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + sqlEx.Message });
            }
        }

    }
}
