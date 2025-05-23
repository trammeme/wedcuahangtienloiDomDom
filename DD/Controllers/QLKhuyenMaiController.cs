using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace DD.Controllers
{ public class QLKhuyenMaiController : Controller
        {
        sqlcuahangnghiepvuEntities1 db = new sqlcuahangnghiepvuEntities1();

            public ActionResult Index()
            {
                var khuyenMais = db.KhuyenMais.ToList(); // Lấy danh sách khuyến mãi
                return View(khuyenMais); // Truyền danh sách cho view
            }

        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KhuyenMaiViewModel model, HttpPostedFileBase HinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string fileName = null;

                    if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
                    {
                        fileName = Path.GetFileName(HinhAnhFile.FileName);

                        // Tạo đường dẫn tới thư mục lưu hình ảnh
                        string path = Path.Combine(Server.MapPath("~/Content/Image/sale"), fileName);

                        // Lưu hình ảnh vào thư mục
                        HinhAnhFile.SaveAs(path);
                    }

                    // Kiểm tra mã sản phẩm có tồn tại không
                    int maSP;
                    if (!int.TryParse(model.MaSanPhamKM, out maSP) || db.SanPhamKhuyenMais.Find(maSP) == null)
                    {
                        ModelState.AddModelError("MaSanPhamKM", "Mã sản phẩm không tồn tại.");
                        return View(model);
                    }

                    var KM = new KhuyenMai
                    {
                        MaKhuyenMai = model.MaKhuyenMai,
                        TenKhuyenMai = model.TenKhuyenMai,
                        MoTa = model.MoTa,
                        NgayBatDau = model.NgayBatDau,
                        NgayKetThuc = model.NgayKetThuc,
                        MaSanPhamKM = maSP,
                        HinhAnh = fileName
                    };

                    db.KhuyenMais.Add(KM);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                }
            }

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                var khuyenMai = db.KhuyenMais.Find(id);
                if (khuyenMai != null)
                {
                    db.KhuyenMais.Remove(khuyenMai);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Ghi log hoặc xử lý lỗi
                System.Diagnostics.Debug.WriteLine($"Lỗi: {ex.Message}");
            }

            return RedirectToAction("Index");
        }



        public ActionResult Edit(string id)
        {
            var KhuyenMai = db.KhuyenMais.Find(id);
            if (KhuyenMai == null)
            {
                return HttpNotFound();
            }
            return View(KhuyenMai);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KhuyenMai model)
        {
            if (ModelState.IsValid)
            {
                var KhuyenMai = db.KhuyenMais.Find(model.MaKhuyenMai);
                if (KhuyenMai == null)
                {
                    return HttpNotFound();
                }

                var sanPham = db.SanPhamKhuyenMais.Find(model.MaSanPhamKM);
                if (sanPham == null)
                {
                    ModelState.AddModelError("MaSanPhamKM", "Mã sản phẩm không tồn tại.");
                    return View(model);
                }

                // Xác thực NgayBatDau và NgayKetThuc
                if (model.NgayKetThuc < model.NgayBatDau)
                {
                    ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
                    return View(model);
                }

                // Cập nhật các thuộc tính
                KhuyenMai.MaSanPhamKM = model.MaSanPhamKM;
                KhuyenMai.MoTa = model.MoTa;
                KhuyenMai.NgayBatDau = model.NgayBatDau;
                KhuyenMai.NgayKetThuc = model.NgayKetThuc;

                // Cập nhật HinhAnh chỉ khi có giá trị mới
                if (!string.IsNullOrEmpty(model.HinhAnh))
                {
                    KhuyenMai.HinhAnh = Path.GetFileName(model.HinhAnh);
                }


                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine(ex.InnerException?.Message);
                    throw;
                }
            }
            return View(model);
        }
    }
}
