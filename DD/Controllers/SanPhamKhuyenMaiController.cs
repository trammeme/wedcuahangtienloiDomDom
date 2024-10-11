using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class SanPhamKhuyenMaiController : Controller
    {
        // Khởi tạo context
        private luckEntities db = new luckEntities();

        // GET: SanPhamKhuyenMai
        public ActionResult Index()
        {
            var sanPhamList = db.SanPhamKhuyenMais.ToList(); // Lấy danh sách sản phẩm
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
        public ActionResult Create(SanPhamKhuyenMaiViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sanPham = new SanPhamKhuyenMai
                {
                    masanphamKM = model.MasanphamKM,
                    tensanpham = model.Tensanpham,
                    motaKhuyenMai = model.MotaKhuyenMai,
                    hinhAnh = $"~/Images/SanPhamKhuyenMai/{model.HinhAnh}" // Sửa chỗ này
                };

                db.SanPhamKhuyenMais.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sản phẩm
            }

            return View(model);
        }

        public ActionResult Delete(string id)
        {
            var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(id);
            if (sanPhamKhuyenMai == null)
            {
                return HttpNotFound();
            }
            return View(sanPhamKhuyenMai);
        }

        // POST: SanPhamKhuyenMai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(id);
            if (sanPhamKhuyenMai != null)
            {
                db.SanPhamKhuyenMais.Remove(sanPhamKhuyenMai);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }// GET: SanPhamKhuyenMai/Edit/5
        public ActionResult Edit(string id)
        {
            var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(id);
            if (sanPhamKhuyenMai == null)
            {
                return HttpNotFound();
            }
            return View(sanPhamKhuyenMai);
        }


        // POST: SanPhamKhuyenMai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SanPhamKhuyenMaiViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sanPhamKhuyenMai = db.SanPhamKhuyenMais.Find(model.MasanphamKM);
                if (sanPhamKhuyenMai != null)
                {
                    sanPhamKhuyenMai.masanphamKM = model.MasanphamKM;

                    sanPhamKhuyenMai.tensanpham = model.Tensanpham;
                    sanPhamKhuyenMai.motaKhuyenMai = model.MotaKhuyenMai;
                    sanPhamKhuyenMai.hinhAnh = $"~/Images/SanPhamKhuyenMai/{model.HinhAnh}"; // Cập nhật hình ảnh

                    db.SaveChanges();
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách sản phẩm
                }
            }
            return View(model);
        }

    }
}
