using DD.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class QLCuaHangController : Controller
    {
        private readonly string _connectionString = "Data Source=MINU\\SQLEXPRESS;Initial Catalog=Dom1;Integrated Security=True;";

        public async Task<ActionResult> Index()
        {
            List<CuaHang> danhSachCuaHang = new List<CuaHang>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand("SELECT * FROM CuaHang", connection);
                SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    danhSachCuaHang.Add(new CuaHang
                    {
                        macuahang = reader["macuahang"].ToString(),
                        makhuyenmai = reader["makhuyenmai"].ToString(),
                        diachi = reader["diachi"].ToString()
                    });
                }
            }

            return View(danhSachCuaHang);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CuaHang cuaHang)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand("INSERT INTO CuaHang (macuahang, makhuyenmai, diachi) VALUES (@macuahang, @makhuyenmai, @diachi)", connection);
                    command.Parameters.AddWithValue("@macuahang", cuaHang.macuahang);
                    command.Parameters.AddWithValue("@makhuyenmai", cuaHang.makhuyenmai);
                    command.Parameters.AddWithValue("@diachi", cuaHang.diachi);
                    await command.ExecuteNonQueryAsync();
                }
                return RedirectToAction("Index");
            }
            return View(cuaHang);
        }

        public async Task<ActionResult> Edit(string id)
        {
            CuaHang cuaHang = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM CuaHang WHERE macuahang = @macuahang", connection);
                command.Parameters.AddWithValue("@macuahang", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        cuaHang = new CuaHang
                        {
                            macuahang = reader["macuahang"].ToString(),
                            makhuyenmai = reader["makhuyenmai"].ToString(),
                            diachi = reader["diachi"].ToString()
                        };
                    }
                }
            }
            if (cuaHang == null)
            {
                return HttpNotFound();
            }
            return View(cuaHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CuaHang cuaHang)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var command = new SqlCommand("UPDATE CuaHang SET makhuyenmai = @makhuyenmai, diachi = @diachi WHERE macuahang = @macuahang", connection);
                    command.Parameters.AddWithValue("@macuahang", cuaHang.macuahang);
                    command.Parameters.AddWithValue("@makhuyenmai", cuaHang.makhuyenmai);
                    command.Parameters.AddWithValue("@diachi", cuaHang.diachi);
                    await command.ExecuteNonQueryAsync();
                }
                return RedirectToAction("Index");
            }
            return View(cuaHang);
        }

        // GET: Xác nhận xóa
        public async Task<ActionResult> Delete(string id)
        {
            CuaHang cuaHang = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT * FROM CuaHang WHERE macuahang = @macuahang", connection);
                command.Parameters.AddWithValue("@macuahang", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        cuaHang = new CuaHang
                        {
                            macuahang = reader["macuahang"].ToString(),
                            makhuyenmai = reader["makhuyenmai"].ToString(),
                            diachi = reader["diachi"].ToString()
                        };
                    }
                }
            }
            if (cuaHang == null)
            {
                return HttpNotFound();
            }
            return View(cuaHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string macuahang, string makhuyenmai)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("DELETE FROM CuaHang WHERE macuahang = @macuahang AND makhuyenmai = @makhuyenmai", connection);
                command.Parameters.AddWithValue("@macuahang", macuahang);
                command.Parameters.AddWithValue("@makhuyenmai", makhuyenmai);
                await command.ExecuteNonQueryAsync();
            }
            return RedirectToAction("Index");
        }



    }
}
