using DD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class SanPhamController : Controller
    {
        private camlyEntities1 db = new camlyEntities1();

        public ActionResult Index()
        {
            var sanPhamList = db.SanPhams.ToList();
            return View(sanPhamList);
        }

        public ActionResult Create()
        {
            // Tạo mã sản phẩm tự động
            var lastProduct = db.SanPhams.OrderByDescending(p => p.MaSanPham).FirstOrDefault();
            int nextId = (lastProduct?.MaSanPham ?? 0) + 1;

            var model = new SanPham { MaSanPham = nextId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanPham model, HttpPostedFileBase imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string fileName = null;
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        // Xử lý file hình ảnh
                        fileName = Path.GetFileName(imageFile.FileName);
                        string uniqueFileName = $"SP{model.MaSanPham}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fileName)}";
                        string uploadPath = Server.MapPath("~/Images");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string filePath = Path.Combine(uploadPath, uniqueFileName);
                        imageFile.SaveAs(filePath);
                        model.HinhAnh = $"/Images/{uniqueFileName}";
                    }
                    else if (string.IsNullOrEmpty(model.HinhAnh))
                    {
                        // Nếu không có hình ảnh, sử dụng hình mặc định
                        model.HinhAnh = "/Images/default-product.jpg";
                    }

                    // Kiểm tra và làm sạch dữ liệu trước khi lưu
                    if (string.IsNullOrEmpty(model.TenSanPham))
                        ModelState.AddModelError("TenSanPham", "Tên sản phẩm không được để trống");

                    if (model.Gia <= 0)
                        ModelState.AddModelError("Gia", "Giá phải lớn hơn 0");

                    if (string.IsNullOrEmpty(model.LoaiSanPham))
                        ModelState.AddModelError("LoaiSanPham", "Vui lòng chọn loại sản phẩm");

                    if (!ModelState.IsValid)
                        return View(model);

                    // Tạo đối tượng mới để tránh tracking issues
                    var sanPham = new SanPham
                    {
                        MaSanPham = model.MaSanPham,
                        TenSanPham = model.TenSanPham?.Trim(),
                        MoTa = model.MoTa?.Trim(),
                        Gia = model.Gia,
                        LoaiSanPham = model.LoaiSanPham,
                        HinhAnh = model.HinhAnh
                    };

                    db.SanPhams.Add(sanPham);
                    db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu sản phẩm: " + ex.Message);
            }

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SanPham model, HttpPostedFileBase imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var sanPham = db.SanPhams.Find(model.MaSanPham);
                    if (sanPham == null)
                        return HttpNotFound();

                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        // Xử lý file hình ảnh mới
                        string uniqueFileName = $"SP{model.MaSanPham}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(imageFile.FileName)}";
                        string uploadPath = Server.MapPath("~/Images");
                        string filePath = Path.Combine(uploadPath, uniqueFileName);
                        imageFile.SaveAs(filePath);
                        sanPham.HinhAnh = $"/Images/{uniqueFileName}";
                    }

                    // Cập nhật thông tin sản phẩm
                    sanPham.TenSanPham = model.TenSanPham?.Trim();
                    sanPham.MoTa = model.MoTa?.Trim();
                    sanPham.Gia = model.Gia;
                    sanPham.LoaiSanPham = model.LoaiSanPham;

                    db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm: " + ex.Message);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var sanPham = db.SanPhams.Find(id);
                if (sanPham == null)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm cần xóa";
                    return RedirectToAction("Index");
                }

                // Lưu đường dẫn hình ảnh trước khi xóa
                string imageToDelete = sanPham.HinhAnh;

                // Xóa sản phẩm khỏi database
                db.SanPhams.Remove(sanPham);
                db.SaveChanges();

                // Sau khi xóa thành công khỏi database, xóa file hình ảnh
                if (!string.IsNullOrEmpty(imageToDelete))
                {
                    string fullPath = Server.MapPath("~" + imageToDelete);
                    if (System.IO.File.Exists(fullPath))
                    {
                        try
                        {
                            System.IO.File.Delete(fullPath);
                        }
                        catch (IOException)
                        {
                            // Log lỗi nếu không xóa được file
                            // Nhưng không throw exception vì sản phẩm đã xóa thành công
                        }
                    }
                }

                TempData["Success"] = "Xóa sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa sản phẩm: " + ex.Message;
                return RedirectToAction("Index");
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