using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data.Entity.Validation;

namespace DD.Controllers
{ public class QLKhuyenMaiController : Controller
        {
        thuaEntities1 db = new thuaEntities1();

            public ActionResult Index()
            {
                var khuyenMais = db.KhuyenMais.ToList(); // Lấy danh sách khuyến mãi
                return View(khuyenMais); // Truyền danh sách cho view
            }

        public ActionResult Create()
        {
            return View();
        }
        // Xử lý khi form được gửi


        // POST: SanPhamKhuyenMai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KhuyenMaiViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var KM = new KhuyenMai
                    {
                        MaKhuyenMai = model.MaKhuyenMai,
                        TenKhuyenMai = model.TenKhuyenMai, // Gán giá trị cho TenKhuyenMai
                        MoTa = model.MoTa,
                        NgayBatDau = model.NgayBatDau,
                        NgayKetThuc=model.NgayKetThuc,
                        MaSanPhamKM = model.MaSanPhamKM,
                        HinhAnh = model.HinhAnh
                    };

                    db.KhuyenMais.Add(KM);
                    db.SaveChanges();
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sản phẩm
                }
                catch (DbEntityValidationException ex)
                {
                    // Lặp qua các lỗi và thêm vào ModelState để hiển thị
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
            }
            // Nếu có lỗi, view sẽ hiển thị lỗi validation
            return View(model);
        }



        public ActionResult Delete(string id)
        {
            System.Diagnostics.Debug.WriteLine($"Delete method called with id: {id}");

            var khuyenMai = db.KhuyenMais.Find(id);
            if (khuyenMai == null)
            {
                return HttpNotFound();
            }
            var viewModel = new KhuyenMaiViewModel
            {
                MaKhuyenMai = khuyenMai.MaKhuyenMai,
                MoTa = khuyenMai.MoTa,
                NgayBatDau = khuyenMai.NgayBatDau,
                NgayKetThuc = khuyenMai.NgayKetThuc,
                MaSanPhamKM = khuyenMai.MaSanPhamKM,
                HinhAnh = khuyenMai.HinhAnh
            };
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteConfirmed called with id: {id}");

            try
            {
                var khuyenMai = db.KhuyenMais.Find(id);
                if (khuyenMai != null)
                {
                    db.KhuyenMais.Remove(khuyenMai);
                    db.SaveChanges();
                    // Optional: inform the user that deletion was successful  
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Khuyen mai with id {id} not found.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during deletion: {ex.Message}");
                ModelState.AddModelError("", "Unable to delete the item. Please try again later.");
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
                if (KhuyenMai != null)
                {
                    // Kiểm tra masanphamKM có tồn tại trong bảng SanPhamKhuyenMai
                    var sanPham = db.SanPhamKhuyenMais.Find(model.MaSanPhamKM);
                    if (sanPham == null)
                    {
                        ModelState.AddModelError("MaSanPhamKM", "Mã sản phẩm không tồn tại.");
                        return View(model);
                    }

                    KhuyenMai.MaSanPhamKM = model.MaSanPhamKM;
                    KhuyenMai.MoTa = model.MoTa;
                    KhuyenMai.NgayBatDau = model.NgayBatDau;
                    KhuyenMai.NgayKetThuc = model.NgayKetThuc;
                    KhuyenMai.HinhAnh = $"~/Images/khuyenmai/{model.HinhAnh}"; // Cập nhật hình ảnh

                    db.SaveChanges();
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sản phẩm
                }
            }
            return View(model);
        }

    }
}
