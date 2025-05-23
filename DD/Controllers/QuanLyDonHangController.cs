using DD.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class QuanLyDonHangController : Controller
    {
        private readonly sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();

        // GET: QuanLyDonHang/Index - List orders with pagination and search
        public ActionResult Index(string search, int? tinhTrang, int page = 1, int pageSize = 10)
        {
            var orders = db.DONHANGs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o => o.MaKH.ToString().Contains(search) || o.NgayDat.ToString().Contains(search));
            }

            if (tinhTrang.HasValue)
            {
                orders = orders.Where(o => o.TinhTrangGiaoHang == tinhTrang.Value);
            }

            var pagedOrders = orders
                            .OrderByDescending(o => o.NgayDat)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = orders.Count();
            ViewBag.Search = search;
            ViewBag.TinhTrang = tinhTrang;

            ViewBag.TinhTrangGiaoHangList = new SelectList(db.TinhTrangGiaoHangs.ToList(), "MaTinhTrang", "TenTinhTrang");

            return View(pagedOrders);
        }
        public ActionResult ThongKe()
        {
            var result = db.DONHANGs
                .GroupBy(o => DbFunctions.TruncateTime(o.NgayDat))
                .Select(g => new ThongKeDonHangViewModel
                {
                    Ngay = g.Key.Value,
                    SoLuong = g.Count(),
                    TongTien = g.Sum(x => x.CHITIETDATHANGs.Sum(ct => (decimal?)ct.SoLuong * ct.DonGia) ?? 0)
                })
                .OrderByDescending(x => x.Ngay)
                .Take(7)
                .ToList();

            // Kiểm tra null ở đây
            if (result == null)
            {
                result = new List<ThongKeDonHangViewModel>();
            }

            return View(result);
        }



        // GET: QuanLyDonHang/Detail - Load order details
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var order = db.DONHANGs.SingleOrDefault(o => o.MaDonHang == id);
            if (order == null) return HttpNotFound();

            var orderDetails = db.CHITIETDATHANGs.Where(ct => ct.MaDonHang == id).ToList();
            ViewBag.OrderDetails = orderDetails;

            return PartialView("Detail", order);
        }

        // GET: QuanLyDonHang/Edit - Load edit form for an order
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var order = db.DONHANGs.SingleOrDefault(o => o.MaDonHang == id);
            if (order == null) return HttpNotFound();

            // Lấy danh sách trạng thái giao hàng từ bảng TinhTrangGiaoHang
            var tinhTrangGiaoHangList = db.TinhTrangGiaoHangs
                .Select(t => new SelectListItem
                {
                    Value = t.MaTinhTrang.ToString(),  // <-- Sửa lại đúng tên trường khóa chính
                    Text = t.TenTinhTrang   // <-- Sửa đúng tên thuộc tính thực tế

                }).ToList();

            ViewBag.TinhTrangGiaoHangList = tinhTrangGiaoHangList;

            return PartialView("Edit", order);
        }

        // POST: QuanLyDonHang/Edit - Update order status
        [HttpPost]
        public ActionResult Edit(FormCollection f)
        {
            try
            {
                if (int.TryParse(f["MaDonHang"], out int maDonHang))
                {
                    var order = db.DONHANGs.SingleOrDefault(o => o.MaDonHang == maDonHang);
                    if (order != null)
                    {
                        // Cập nhật trạng thái giao hàng
                        if (int.TryParse(f["TinhTrangGiaoHang"], out int tinhTrangGiaoHang))
                        {
                            order.TinhTrangGiaoHang = tinhTrangGiaoHang;
                        }
                        // Cập nhật trạng thái thanh toán
                        order.DaThanhToan = f["DaThanhToan"] == "true";
                        db.SaveChanges();
                        TempData["Message"] = "Cập nhật trạng thái đơn hàng thành công.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Mã đơn hàng không hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Lỗi trong Edit: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        // GET: QuanLyDonHang/Delete - Load delete confirmation form
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var order = db.DONHANGs.SingleOrDefault(o => o.MaDonHang == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return PartialView("Delete", order);
        }

        // POST: QuanLyDonHang/DeleteConfirmed - Delete an order and its details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var order = db.DONHANGs.Find(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("Index");
                }

                // Delete related CHITIETDATHANG records
                var orderDetails = db.CHITIETDATHANGs.Where(ct => ct.MaDonHang == id).ToList();
                db.CHITIETDATHANGs.RemoveRange(orderDetails);

                // Delete related ThanhToan records (nếu có)
                var thanhToanRecords = db.ThanhToans.Where(tt => tt.MaDonHang == id).ToList();
                db.ThanhToans.RemoveRange(thanhToanRecords);

                // Delete the order
                db.DONHANGs.Remove(order);
                db.SaveChanges();

                TempData["Message"] = "Xóa đơn hàng thành công.";
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa đơn hàng do có dữ liệu liên quan.";
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa đơn hàng: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa đơn hàng: {ex.Message}");
            }

            return RedirectToAction("Index");
        }
    }
}