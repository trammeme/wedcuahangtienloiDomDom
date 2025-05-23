using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml; // Thêm namespace của EPPlus
using System.IO;

namespace DD.Controllers
{
    public class QuanLyUserController : Controller
    {
        private readonly sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();

        // GET: QuanLyUser/Index - List users with pagination and search
        public ActionResult Index(string search, int page = 1, int pageSize = 10)
        {
            var users = db.NguoiDungs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.TenKH.Contains(search) || u.Email.Contains(search));
            }

            var pagedUsers = users
                            .OrderBy(u => u.MaKH)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = users.Count();
            ViewBag.Search = search;

            return View(pagedUsers);
        }

        // GET: QuanLyUser/Edit - Load edit form for a user
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var kq = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (kq == null) return HttpNotFound();
            return PartialView("Edit", kq);
        }

        // GET: QuanLyUser/Detail - Load user details
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user == null) return HttpNotFound();
            return PartialView("Detail", user);
        }

        // POST: QuanLyUser/Edit - Update user information
        [HttpPost]
        public ActionResult Edit(FormCollection f)
        {
            try
            {
                if (int.TryParse(f["maKH"], out int maKH))
                {
                    var kq = db.NguoiDungs.SingleOrDefault(n => n.MaKH == maKH);
                    if (kq != null)
                    {
                        kq.TenDangNhap = f["tenKH"];
                        kq.Email = f["email"];
                        kq.GioiTinh = f["gioitinh"];
                        kq.DiaChi = f["diachi"];
                        kq.SoDienThoai = f["sodienthoai"];
                        db.SaveChanges();
                        TempData["Message"] = "Cập nhật thông tin khách hàng thành công.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Mã khách hàng không hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Lỗi trong Edit: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        // GET: QuanLyUser/Delete - Load delete confirmation form
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return PartialView("Delete", user);
        }

        // POST: QuanLyUser/DeleteConfirmed - Delete a user and related records
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var user = db.NguoiDungs.Find(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                    return RedirectToAction("Index");
                }

                // Delete related LichSuTichDiem records
                var lichSuTichDiemRecords = db.LichSuTichDiems.Where(ls => ls.MaKH == id).ToList();
                db.LichSuTichDiems.RemoveRange(lichSuTichDiemRecords);

                // Delete related TichDiem records
                var tichDiemRecords = db.TichDiems.Where(t => t.MaKH == id).ToList();
                db.TichDiems.RemoveRange(tichDiemRecords);

                // Delete related DONHANG and CHITIETDATHANG records
                var donHangRecords = db.DONHANGs.Where(dh => dh.MaKH == id).ToList();
                foreach (var donHang in donHangRecords)
                {
                    // Delete related CHITIETDATHANG records
                    var chiTietDonHangRecords = db.CHITIETDATHANGs.Where(ct => ct.MaDonHang == donHang.MaDonHang).ToList();
                    db.CHITIETDATHANGs.RemoveRange(chiTietDonHangRecords);
                }
                db.DONHANGs.RemoveRange(donHangRecords);

                // Delete the user
                db.NguoiDungs.Remove(user);
                db.SaveChanges();

                TempData["Message"] = "Xóa khách hàng thành công.";
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng do có dữ liệu liên quan trong hệ thống (ví dụ: điểm tích lũy hoặc đơn hàng).";
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa khách hàng: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa khách hàng: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        // GET: QuanLyUser/ExportToExcel - Export user list to Excel
        public ActionResult ExportToExcel()
        {
            var users = db.NguoiDungs.ToList();

            // Disable license check for EPPlus (required for versions >= 5.0)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("KhachHang");
                worksheet.Cells[1, 1].Value = "MÃ KH";
                worksheet.Cells[1, 2].Value = "TÊN KH";
                worksheet.Cells[1, 3].Value = "EMAIL";
                worksheet.Cells[1, 4].Value = "GIỚI TÍNH";
                worksheet.Cells[1, 5].Value = "THÁNG SINH";
                worksheet.Cells[1, 6].Value = "ĐỊA CHỈ";
                worksheet.Cells[1, 7].Value = "ĐIỆN THOẠI";
                worksheet.Cells[1, 8].Value = "ĐĂNG NHẬP";
                worksheet.Cells[1, 9].Value = "MẬT KHẨU";

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }

                // Đổ dữ liệu
                for (int i = 0; i < users.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = users[i].MaKH;
                    worksheet.Cells[i + 2, 2].Value = users[i].TenKH;
                    worksheet.Cells[i + 2, 3].Value = users[i].Email;
                    worksheet.Cells[i + 2, 4].Value = users[i].GioiTinh;
                    worksheet.Cells[i + 2, 5].Value = users[i].ThangSinh;
                    worksheet.Cells[i + 2, 6].Value = users[i].DiaChi;
                    worksheet.Cells[i + 2, 7].Value = users[i].SoDienThoai;
                    worksheet.Cells[i + 2, 8].Value = users[i].TenDangNhap;
                    worksheet.Cells[i + 2, 9].Value = users[i].MatKhau;
                }

                // Tự động điều chỉnh cột
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Chuyển file Excel thành mảng byte
                var excelBytes = package.GetAsByteArray();

                // Trả về file Excel để tải xuống
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachKhachHang_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
            }
        }
    }
}