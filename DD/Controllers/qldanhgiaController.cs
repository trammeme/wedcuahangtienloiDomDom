using DD.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class QLDanhGiaController : Controller
    {
        private readonly string connectionString = "Data Source=MINU\\SQLEXPRESS;Initial Catalog=Dom1;Integrated Security=True;";

        public ActionResult Index()
        {
            List<qldanhgia> danhGiaList = new List<qldanhgia>();

            // Mở kết nối với SQL Server
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Truy vấn dữ liệu từ bảng DanhGia
                string query = "SELECT MaDangGia, MaKH, NoiDung, SoSao, NgayDanhGia, IsApproved FROM DanhGia";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Đọc dữ liệu từ bảng và thêm vào danh sách danhGiaList
                    while (reader.Read())
                    {
                        danhGiaList.Add(new qldanhgia
                        {
                            MaDangGia = reader["MaDangGia"].ToString(),
                            MaKH = reader["MaKH"].ToString(),
                            NoiDung = reader["NoiDung"].ToString(),
                            SoSao = (int)reader["SoSao"],
                            NgayDanhGia = (DateTime)reader["NgayDanhGia"],
                            IsApproved = reader["IsApproved"] != DBNull.Value ? (bool)reader["IsApproved"] : false
                        });
                    }
                }
            }

            // Trả về PartialView với danh sách đánh giá
            return PartialView("Index", danhGiaList);  // Chú ý, trả về một partial view với tên `_DanhGiaPartial`
        }
        public ActionResult Approve(string maDangGia)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE DanhGia SET IsApproved = 1 WHERE MaDangGia = @MaDangGia";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaDangGia", maDangGia);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // Phương thức xóa đánh giá
        public ActionResult Delete(string maDangGia)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM DanhGia WHERE MaDangGia = @MaDangGia";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaDangGia", maDangGia);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
        private void SaveResponse(qldanhgia danhGia)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE DanhGia SET PhanHoi = @PhanHoi WHERE MaDangGia = @MaDangGia";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PhanHoi", danhGia.PhanHoi);
                cmd.Parameters.AddWithValue("@MaDangGia", danhGia.MaDangGia);
                cmd.ExecuteNonQuery();
            }
        }
        // Phương thức trả lời đánh giá (thêm trả lời vào cơ sở dữ liệu)
        public ActionResult Reply(string maDangGia)
        {
            var danhGia = GetDanhGiaByMaDangGia(maDangGia);
            return View(danhGia);
        }
        private qldanhgia GetDanhGiaByMaDangGia(string maDangGia)
        {
            qldanhgia danhGia = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT MaDangGia, MaKH, NoiDung, SoSao, NgayDanhGia, IsApproved FROM DanhGia WHERE MaDangGia = @MaDangGia";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaDangGia", maDangGia);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    danhGia = new qldanhgia
                    {
                        MaDangGia = reader["MaDangGia"].ToString(),
                        MaKH = reader["MaKH"].ToString(),
                        NoiDung = reader["NoiDung"].ToString(),
                        SoSao = (int)reader["SoSao"],
                        NgayDanhGia = (DateTime)reader["NgayDanhGia"],
                        IsApproved = reader["IsApproved"] != DBNull.Value ? (bool)reader["IsApproved"] : false
                    };
                }
            }

            return danhGia;
        }

        [HttpPost]
        public ActionResult Reply(qldanhgia danhGia)
        {
           
            SaveResponse(danhGia); 
            return RedirectToAction("Index");
        }
    }
}