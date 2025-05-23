using DD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity; // Nếu dùng EF6
namespace DD.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();

        // GET: SanPham
        public async Task<ActionResult> Index(int page = 1, int pageSize = 10)
        {
            var sanPhamList = await db.SanPhams
                .OrderByDescending(p => p.MaSanPham) // Sắp xếp mới nhất trước
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Thêm thông tin phân trang
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = await db.SanPhams.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling((double)ViewBag.TotalRecords / pageSize);

            return View(sanPhamList);
        }

        // GET: SanPham/Create
        public ActionResult Create()
        {
            // Tạo danh sách LoaiSanPham từ schema SQL
            ViewBag.LoaiSanPhamList = new List<string> { "doan", "thucuong", "dungcu" };
            return View(new SanPham());
        }

        // POST: SanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SanPham model, HttpPostedFileBase imageFile)
        {
            ViewBag.LoaiSanPhamList = new List<string> { "doan", "thucuong", "dungcu" };

            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra dữ liệu
                    if (string.IsNullOrWhiteSpace(model.TenSanPham))
                        ModelState.AddModelError("TenSanPham", "Tên sản phẩm không được để trống");

                    if (model.Gia <= 0)
                        ModelState.AddModelError("Gia", "Giá phải lớn hơn 0");

                    if (string.IsNullOrWhiteSpace(model.LoaiSanPham) || !ViewBag.LoaiSanPhamList.Contains(model.LoaiSanPham))
                    {
                        System.Diagnostics.Debug.WriteLine($"LoaiSanPham không hợp lệ: '{model.LoaiSanPham}'");
                        ModelState.AddModelError("LoaiSanPham", $"Loại sản phẩm không hợp lệ. Giá trị nhận được: '{model.LoaiSanPham}'");
                    }

                    if (model.SoLuongTon < 0)
                        ModelState.AddModelError("SoLuongTon", "Số lượng tồn phải lớn hơn hoặc bằng 0");

                    // Xử lý file hình ảnh
                    string fileName = null;
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var extension = Path.GetExtension(imageFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("imageFile", "Chỉ chấp nhận file .jpg, .jpeg, .png");
                        }
                        else if (imageFile.ContentLength > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("imageFile", "Kích thước file không được vượt quá 5MB");
                        }
                        else
                        {
                            fileName = $"SP_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                            string uploadPath = Server.MapPath("~/Images");
                            if (!Directory.Exists(uploadPath))
                            {
                                Directory.CreateDirectory(uploadPath);
                            }
                            string filePath = Path.Combine(uploadPath, fileName);
                            imageFile.SaveAs(filePath);
                            model.HinhAnh = $"/Images/{fileName}";
                        }
                    }
                    else
                    {
                        model.HinhAnh = "/Images/default-product.jpg";
                    }

                    if (!ModelState.IsValid)
                    {
                        // Ghi log các lỗi trong ModelState
                        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                        {
                            System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                        }
                        return View(model);
                    }

                    // Thêm sản phẩm mới
                    var sanPham = new SanPham
                    {
                        TenSanPham = model.TenSanPham?.Trim(),
                        MoTa = model.MoTa?.Trim(),
                        Gia = model.Gia,
                        SoLuongTon = model.SoLuongTon,
                        LoaiSanPham = model.LoaiSanPham,
                        HanSuDung = model.HanSuDung,
                        TrangThai = model.TrangThai ?? "Bình thường",
                        HinhAnh = model.HinhAnh,
                        NgayTao = DateTime.Now
                    };

                    db.SanPhams.Add(sanPham);
                    await db.SaveChangesAsync();

                    // Ghi lịch sử tồn kho
                    var lichSuTonKho = new LichSuTonKho
                    {
                        MaSanPham = sanPham.MaSanPham,
                        SoLuongThayDoi = sanPham.SoLuongTon,
                        LyDo = "Thêm sản phẩm mới",
                        NgayGiaoDich = DateTime.Now
                    };
                    db.LichSuTonKhoes.Add(lichSuTonKho);
                    await db.SaveChangesAsync();

                    TempData["Success"] = "Thêm sản phẩm thành công";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Ghi log khi ModelState không hợp lệ
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine($"ModelState Error: {error.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                ModelState.AddModelError("", $"Lỗi khi thêm sản phẩm: {ex.Message}");
            }

            return View(model);
        }

        // GET: SanPham/Edit
        public async Task<ActionResult> Edit(int id)
        {
            var sanPham = await db.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            ViewBag.LoaiSanPhamList = new List<string> { "doan", "thucuong", "dungcu" };
            return View(sanPham);
        }

        // POST: SanPham/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(SanPham model, HttpPostedFileBase imageFile)
        {
            ViewBag.LoaiSanPhamList = new List<string> { "doan", "thucuong", "dungcu" };

            try
            {
                if (ModelState.IsValid)
                {
                    var sanPham = await db.SanPhams.FindAsync(model.MaSanPham);
                    if (sanPham == null)
                        return HttpNotFound();

                    // Kiểm tra dữ liệu...
                    if (string.IsNullOrWhiteSpace(model.TenSanPham))
                        ModelState.AddModelError("TenSanPham", "Tên sản phẩm không được để trống");

                    if (model.Gia <= 0)
                        ModelState.AddModelError("Gia", "Giá phải lớn hơn 0");

                    if (string.IsNullOrWhiteSpace(model.LoaiSanPham) || !ViewBag.LoaiSanPhamList.Contains(model.LoaiSanPham))
                        ModelState.AddModelError("LoaiSanPham", "Loại sản phẩm không hợp lệ");

                    if (model.SoLuongTon < 0)
                        ModelState.AddModelError("SoLuongTon", "Số lượng tồn phải >= 0");

                    if (!ModelState.IsValid)
                        return View(model);

                    // Xử lý hình ảnh mới nếu có
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var extension = Path.GetExtension(imageFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                            return View(model);

                        if (imageFile.ContentLength > 5 * 1024 * 1024)
                            return View(model);

                        // Xóa ảnh cũ nếu không phải mặc định
                        if (!string.IsNullOrEmpty(sanPham.HinhAnh) && sanPham.HinhAnh != "default-product.jpg")
                        {
                            string oldImagePath = Server.MapPath("~/Image/" + sanPham.HinhAnh);
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        string fileName = Path.GetFileName(imageFile.FileName); // Giữ tên gốc
                        string uploadPath = Server.MapPath("~/Image");
                        string filePath = Path.Combine(uploadPath, fileName);
                        imageFile.SaveAs(filePath);

                        // ❗️Chỉ lưu tên file
                        sanPham.HinhAnh = fileName;
                    }


                    // Cập nhật thông tin
                    int oldSoLuongTon = sanPham.SoLuongTon;
                    sanPham.TenSanPham = model.TenSanPham?.Trim();
                    sanPham.MoTa = model.MoTa?.Trim();
                    sanPham.Gia = model.Gia;
                    sanPham.SoLuongTon = model.SoLuongTon;
                    sanPham.LoaiSanPham = model.LoaiSanPham;
                    sanPham.HanSuDung = model.HanSuDung;
                    sanPham.TrangThai = model.TrangThai ?? "Bình thường";
                    sanPham.NgayCapNhat = DateTime.Now;

                    // Ghi lịch sử tồn kho nếu thay đổi
                    if (oldSoLuongTon != model.SoLuongTon)
                    {
                        var lichSuTonKho = new LichSuTonKho
                        {
                            MaSanPham = sanPham.MaSanPham,
                            SoLuongThayDoi = model.SoLuongTon - oldSoLuongTon,
                            LyDo = "Cập nhật số lượng tồn",
                            NgayGiaoDich = DateTime.Now
                        };
                        db.LichSuTonKhoes.Add(lichSuTonKho);
                    }

                    await db.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sản phẩm thành công";
                    TempData["AnchorId"] = "sanpham-" + model.MaSanPham; // ✅ cuộn lại dòng này
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi: {ex.Message}");
                ModelState.AddModelError("", $"Lỗi cập nhật: {ex.Message}");
            }

            return View(model);
        }

        // GET: SanPham/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var sanPham = await db.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int MaSanPham)
        { 
            try
            {
                var sanPham = db.SanPhams.Find(MaSanPham);
                if (sanPham == null)
                {
                    return HttpNotFound();
                }

                // Xóa ảnh nếu có
                if (!string.IsNullOrEmpty(sanPham.HinhAnh) && sanPham.HinhAnh != "/Images/default-product.jpg")
                {
                    var imagePath = Server.MapPath("~" + sanPham.HinhAnh);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa lịch sử tồn kho liên quan
                var lichSu = db.LichSuTonKhoes.Where(x => x.MaSanPham == MaSanPham).ToList();
                db.LichSuTonKhoes.RemoveRange(lichSu);

                db.SanPhams.Remove(sanPham);
                db.SaveChanges();

                TempData["Success"] = "Xóa sản phẩm thành công";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi EF: " + ex.InnerException?.InnerException?.Message);
                TempData["Error"] = "Lỗi khi xóa sản phẩm. Có thể sản phẩm đang được tham chiếu ở bảng khác.";
            }

            return RedirectToAction("Index");
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