using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace DD.Controllers
{ public class QLKhuyenMaiController : Controller
        {
            luckEntities db = new luckEntities();

            // Hiển thị danh sách khuyến mãi
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
                var KM = new KhuyenMai
                {
                    makhuyenmai = model.MaKhuyenMai,
                    mota = model.MoTa,
                    ngayBatDau = model.NgayBatDau,
                    ngayKetThuc = model.NgayKetThuc,
                    masanphamKM= model.MaSanPhamKM,
                    hinhAnh = $"~/Images/khuyenmai/{model.HinhAnh}" // Sửa chỗ này
                };

                db.KhuyenMais.Add(KM);
                db.SaveChanges();
                return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sản phẩm
            }

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
                MaKhuyenMai = khuyenMai.makhuyenmai,
                MoTa = khuyenMai.mota,
                NgayBatDau = khuyenMai.ngayBatDau,
                NgayKetThuc = khuyenMai.ngayKetThuc,
                MaSanPhamKM = khuyenMai.masanphamKM,
                HinhAnh = khuyenMai.hinhAnh
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
                var KhuyenMai = db.KhuyenMais.Find(model.makhuyenmai);
                if (KhuyenMai != null)
                {
                    // Kiểm tra masanphamKM có tồn tại trong bảng SanPhamKhuyenMai
                    var sanPham = db.SanPhamKhuyenMais.Find(model.masanphamKM);
                    if (sanPham == null)
                    {
                        ModelState.AddModelError("MaSanPhamKM", "Mã sản phẩm không tồn tại.");
                        return View(model);
                    }

                    KhuyenMai.masanphamKM = model.masanphamKM;
                    KhuyenMai.mota = model.mota;
                    KhuyenMai.ngayBatDau = model.ngayBatDau;
                    KhuyenMai.ngayKetThuc = model.ngayKetThuc;
                    KhuyenMai.hinhAnh = $"~/Images/khuyenmai/{model.hinhAnh}"; // Cập nhật hình ảnh

                    db.SaveChanges();
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sản phẩm
                }
            }
            return View(model);
        }

    }
}
