using DD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class SanPhamKhuyenMaiController : Controller
    {
        private sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();

        // GET: SanPhamKhuyenMai
        public ActionResult Index()
        {
            var sanPhamList = db.SanPhamKhuyenMais
                .Include("SanPham") // Include related SanPham for display
                .ToList();
            return View(sanPhamList);
        }

        // GET: SanPhamKhuyenMai/Create
        // GET: SanPhamKhuyenMai/Create
        public ActionResult Create()
        {
            var sanPhams = new List<SelectListItem>
            {
                new SelectListItem { Value = null, Text = "Không liên kết (khuyến mãi chung)" }
            };
                    sanPhams.AddRange(db.SanPhams.Select(s => new SelectListItem
                    {
                        Value = s.MaSanPham.ToString(),
                        Text = s.TenSanPham
                    }));

            ViewBag.MaSanPham = new SelectList(sanPhams, "Value", "Text");
            return View();
        }

        // POST: SanPhamKhuyenMai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanPhamKhuyenMaiViewModel model, HttpPostedFileBase HinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string hinhAnhPath = null;
                    if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(HinhAnhFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/Image/SanPhamKhuyenMai/"), fileName);
                        HinhAnhFile.SaveAs(path);
                        hinhAnhPath = $"~/Content/Image/SanPhamKhuyenMai/{fileName}";
                    }
                    else
                    {
                        ModelState.AddModelError("HinhAnhFile", "Vui lòng chọn file hình ảnh.");
                        ViewBag.MaSanPham = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = null, Text = "Không liên kết (khuyến mãi chung)" }
                }.Concat(db.SanPhams.Select(s => new SelectListItem
                {
                    Value = s.MaSanPham.ToString(),
                    Text = s.TenSanPham
                })), "Value", "Text");
                        return View(model);
                    }

                    var sanPhamKhuyenMai = new SanPhamKhuyenMai
                    {
                        TenSanPham = model.TenSanPham,
                        MoTaKhuyenMai = model.MoTaKhuyenMai,
                        HinhAnh = hinhAnhPath,
                        MaSanPham = model.MaSanPham ?? 0, // Có thể null, nhưng cần kiểm tra entity
                        SoLuongKhuyenMai = model.SoLuongKhuyenMai,
                        TrangThaiSPKM = model.SoLuongKhuyenMai > 0 ? "Còn áp dụng" : "Hết số lượng"
                    };

                    db.SanPhamKhuyenMais.Add(sanPhamKhuyenMai);
                    db.SaveChanges();

                    db.Database.ExecuteSqlCommand("EXEC CapNhatTrangThaiSanPhamKhuyenMai @MaSanPhamKM",
                        new System.Data.SqlClient.SqlParameter("@MaSanPhamKM", sanPhamKhuyenMai.MaSanPhamKM));

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    string innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                    ModelState.AddModelError("", $"Lỗi khi tạo sản phẩm khuyến mãi: {ex.Message} - Nội dung lỗi: {innerExceptionMessage}");
                }
            }

            ViewBag.MaSanPham = new SelectList(new List<SelectListItem>
        {
                new SelectListItem { Value = null, Text = "Không liên kết (khuyến mãi chung)" }
            }.Concat(db.SanPhams.Select(s => new SelectListItem
            {
                Value = s.MaSanPham.ToString(),
                Text = s.TenSanPham
            })), "Value", "Text");
                    return View(model);
        }

        // GET: SanPhamKhuyenMai/Edit/5
        public ActionResult Edit(int id)
        {
            var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(id);
            if (sanPhamKhuyenMai == null)
            {
                return HttpNotFound();
            }

            var model = new SanPhamKhuyenMaiViewModel
            {
                MaSanPhamKM = sanPhamKhuyenMai.MaSanPhamKM,
                TenSanPham = sanPhamKhuyenMai.TenSanPham,
                MoTaKhuyenMai = sanPhamKhuyenMai.MoTaKhuyenMai,
                HinhAnh = sanPhamKhuyenMai.HinhAnh,
                MaSanPham = sanPhamKhuyenMai.MaSanPham,
                SoLuongKhuyenMai = sanPhamKhuyenMai.SoLuongKhuyenMai ?? 0
            };

            // Populate ViewBag.MaSanPham từ bảng SanPhamKhuyenMais
            ViewBag.MaSanPham = new SelectList(db.SanPhamKhuyenMais, "MaSanPhamKM", "TenSanPham", sanPhamKhuyenMai.MaSanPhamKM);
            return View(model);
        }
        // POST: SanPhamKhuyenMai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SanPhamKhuyenMaiViewModel model, HttpPostedFileBase HinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(model.MaSanPhamKM);
                    if (sanPhamKhuyenMai == null)
                    {
                        return HttpNotFound();
                    }

                    // Xử lý tải file
                    if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(HinhAnhFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/Image/SanPhamKhuyenMai/"), fileName);
                        HinhAnhFile.SaveAs(path);
                        sanPhamKhuyenMai.HinhAnh = $"~/Content/Image/SanPhamKhuyenMai/{fileName}";
                    }
                    else
                    {
                        sanPhamKhuyenMai.HinhAnh = model.HinhAnh; // Giữ hình ảnh hiện tại
                    }

                    sanPhamKhuyenMai.TenSanPham = model.TenSanPham;
                    sanPhamKhuyenMai.MoTaKhuyenMai = model.MoTaKhuyenMai;
                    sanPhamKhuyenMai.MaSanPham = model.MaSanPham ?? 0;
                    sanPhamKhuyenMai.SoLuongKhuyenMai = model.SoLuongKhuyenMai; // Đảm bảo là int
                    sanPhamKhuyenMai.TrangThaiSPKM = model.SoLuongKhuyenMai > 0 ? "Còn áp dụng" : "Hết số lượng";

                    db.SaveChanges();

                    // Gọi stored procedure
                    db.Database.ExecuteSqlCommand("EXEC CapNhatTrangThaiSanPhamKhuyenMai @MaSanPhamKM",
                        new System.Data.SqlClient.SqlParameter("@MaSanPhamKM", sanPhamKhuyenMai.MaSanPhamKM));

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Ghi log chi tiết lỗi
                    string innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                    ModelState.AddModelError("", $"Lỗi khi cập nhật sản phẩm khuyến mãi: {ex.Message} - Nội dung lỗi chi tiết: {innerExceptionMessage}");
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message} - Inner: {innerExceptionMessage}");
                }
            }
            else
            {
                // Ghi log lỗi xác thực
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                string validationErrors = string.Join("; ", errors);
                ModelState.AddModelError("", "Lỗi xác thực: " + validationErrors);
                System.Diagnostics.Debug.WriteLine("Validation errors: " + validationErrors);
            }

            // Khôi phục dropdown
            ViewBag.MaSanPham = new SelectList(db.SanPhams, "MaSanPham", "TenSanPham", model.MaSanPham);
            return View(model);
        }

        // GET: SanPhamKhuyenMai/Delete/5
        // GET: SanPhamKhuyenMai/Delete/5
        public ActionResult Delete(int id)
        {
            var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(id);
            if (sanPhamKhuyenMai == null)
            {
                return HttpNotFound();
            }

            var model = new SanPhamKhuyenMaiViewModel
            {
                MaSanPhamKM = sanPhamKhuyenMai.MaSanPhamKM,
                TenSanPham = sanPhamKhuyenMai.TenSanPham,
                MoTaKhuyenMai = sanPhamKhuyenMai.MoTaKhuyenMai,
                HinhAnh = sanPhamKhuyenMai.HinhAnh,
                MaSanPham = sanPhamKhuyenMai.MaSanPham,
                SoLuongKhuyenMai = sanPhamKhuyenMai.SoLuongKhuyenMai ?? 0

            };

            return View(model);
        }

        // POST: SanPhamKhuyenMai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(id);
                if (sanPhamKhuyenMai != null)
                {
                    db.SanPhamKhuyenMais.Remove(sanPhamKhuyenMai);
                    db.SaveChanges();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi xóa sản phẩm khuyến mãi: {ex.Message}");
                var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(id);
                if (sanPhamKhuyenMai == null)
                {
                    return HttpNotFound();
                }
                var model = new SanPhamKhuyenMaiViewModel
                {
                    MaSanPhamKM = sanPhamKhuyenMai.MaSanPhamKM,
                    TenSanPham = sanPhamKhuyenMai.TenSanPham,
                    MoTaKhuyenMai = sanPhamKhuyenMai.MoTaKhuyenMai,
                    HinhAnh = sanPhamKhuyenMai.HinhAnh,
                    MaSanPham = sanPhamKhuyenMai.MaSanPham,
                    SoLuongKhuyenMai = sanPhamKhuyenMai.SoLuongKhuyenMai ?? 0
                };
                return View(model);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}