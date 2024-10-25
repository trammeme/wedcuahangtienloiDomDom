using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class SanPhamController : Controller
    {
        // Khởi tạo context
        private thuaEntities1 db = new thuaEntities1();

        // GET: SanPhamKhuyenMai
        public ActionResult Index()
        {
            var sanPhamList = db.SanPhams.ToList(); // Lấy danh sách sản phẩm
            return View(sanPhamList); // Trả về view hiển thị danh sách sản phẩm
        }

        // GET: SanPhamKhuyenMai/Create
        public ActionResult Create()
        {
            return View();
        }
        // Xử lý khi form được gửi


        // POST: SanPhamKhuyenMai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanPham model)
        {
            if (ModelState.IsValid)
            {
                var sanPham = new SanPham
                {
                    MaSanPham = model.MaSanPham,
                    TenSanPham = model.TenSanPham,
                    MoTa = model.MoTa,
                    Gia = model.Gia,
                    LoaiSanPham=model.LoaiSanPham,
                    HinhAnh = $"~/Images/{model.HinhAnh}" // Sửa chỗ này
                };

                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sản phẩm
            }

            return View(model);
        }

        public ActionResult Delete(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        // POST: SanPhamKhuyenMai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                db.SanPhams.Remove(sanPham);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }// GET: SanPhamKhuyenMai/Edit/5
         // GET: SanPham/Edit/5
        public ActionResult Edit(int id) // Thay đổi kiểu từ string sang int
        {
            var sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        // POST: SanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SanPham model)
        {
            if (ModelState.IsValid)
            {
                var sanPham = db.SanPhams.Find(model.MaSanPham);
                if (sanPham != null)
                {
                    sanPham.TenSanPham = model.TenSanPham;
                    sanPham.MoTa = model.MoTa;
                    sanPham.Gia = model.Gia;
                    sanPham.LoaiSanPham = model.LoaiSanPham;
                    sanPham.HinhAnh = $"~/Images/{model.HinhAnh}"; // Cập nhật hình ảnh

                    db.SaveChanges();
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sản phẩm
                }
            }
            return View(model);
        }


    }
}
