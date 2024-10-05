using DD.Models; // Đảm bảo rằng đường dẫn này chính xác
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

public class QLUserController : Controller
{
    private readonly string connectionString = "Data Source=MINU\\SQLEXPRESS;Initial Catalog=Dom1;Integrated Security=True;";

    public ActionResult Index()
    {
        List<QLUserViewModel> users = new List<QLUserViewModel>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM NguoiDung"; // Thay đổi truy vấn theo nhu cầu  

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        QLUserViewModel user = new QLUserViewModel
                        {
                            MaKH = reader["MaKH"].ToString(),
                            TenKH = reader["TenKH"].ToString(),
                            Email = reader["Email"].ToString(),
                            GioiTinh = reader["GioiTinh"].ToString(),
                            ThangSinh = reader.IsDBNull(reader.GetOrdinal("ThangSinh")) ? (int?)null : (int)reader["ThangSinh"],
                            DiaChi = reader["DiaChi"].ToString(),
                            SoDienThoai = reader["SoDienThoai"].ToString(),
                            TenDangNhap = reader["TenDangNhap"].ToString(),
                            MatKhau = reader["MatKhau"].ToString()
                        };

                        users.Add(user);
                    }
                }
            }
        }

        return PartialView("Index", users);  // Chú ý, trả về một partial view với tên `_DanhGiaPartial`
    }
    // Phương thức sửa
    public ActionResult Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) // Kiểm tra xem id có giá trị không  
        {
            return HttpNotFound(); // Nếu không có ID, trả về lỗi 404  
        }

        QLUserViewModel user = null;
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT * FROM NguoiDung WHERE MaKH = @MaKH";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MaKH", id); // Thêm tham số  
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new QLUserViewModel
                        {
                            MaKH = reader["MaKH"].ToString(),
                            TenKH = reader["TenKH"].ToString(),
                            Email = reader["Email"].ToString(),
                            GioiTinh = reader["GioiTinh"].ToString(),
                            ThangSinh = reader.IsDBNull(reader.GetOrdinal("ThangSinh")) ? (int?)null : (int)reader["ThangSinh"],
                            DiaChi = reader["DiaChi"].ToString(),
                            SoDienThoai = reader["SoDienThoai"].ToString(),
                            TenDangNhap = reader["TenDangNhap"].ToString(),
                            MatKhau = reader["MatKhau"].ToString()
                        };
                    }
                }
            }
        }

        return View(user);
    }
    [HttpPost]
    public ActionResult Edit(QLUserViewModel user)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "UPDATE NguoiDung SET TenKH = @TenKH, Email = @Email, GioiTinh = @GioiTinh, ThangSinh = @ThangSinh, DiaChi = @DiaChi, SoDienThoai = @SoDienThoai, TenDangNhap = @TenDangNhap, MatKhau = @MatKhau WHERE MaKH = @MaKH";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MaKH", user.MaKH);
                command.Parameters.AddWithValue("@TenKH", user.TenKH);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@GioiTinh", user.GioiTinh);
                command.Parameters.AddWithValue("@ThangSinh", (object)user.ThangSinh ?? DBNull.Value); // Xử lý giá trị null
                command.Parameters.AddWithValue("@DiaChi", user.DiaChi);
                command.Parameters.AddWithValue("@SoDienThoai", user.SoDienThoai);
                command.Parameters.AddWithValue("@TenDangNhap", user.TenDangNhap);
                command.Parameters.AddWithValue("@MatKhau", user.MatKhau);

                command.ExecuteNonQuery();
            }
        }
        return RedirectToAction("Index");
    }

    // Phương thức xóa
    // Phương thức xóa người dùng
    public ActionResult Delete(string maKH) // Đổi id thành maKH  
    {
        if (string.IsNullOrEmpty(maKH))
        {
            return HttpNotFound(); // Nếu không có ID, trả về lỗi 404  
        }

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "DELETE FROM NguoiDung WHERE MaKH = @MaKH";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@MaKH", maKH); // Gán giá trị cho tham số  

                command.ExecuteNonQuery(); // Thực thi câu lệnh xóa  
            }
        }

        return RedirectToAction("Index"); // Quay lại trang danh sách người dùng  
    }
}