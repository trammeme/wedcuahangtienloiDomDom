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

            // Tạo khuyến mãi mới
            [HttpGet]
            public ActionResult Create()
            {
                return View();
            }


            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create(KhuyenMai model, HttpPostedFileBase file)
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra và lưu hình ảnh (nếu có)
                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/images/"), fileName);
                        file.SaveAs(path);
                        model.hinhAnh = "~/images/" + fileName; // Gán đường dẫn hình ảnh
                    }

                    // Kiểm tra nếu masanphamKM có tồn tại trong SanPhamKhuyenMai
                    var sanPham = db.SanPhamKhuyenMais.FirstOrDefault(sp => sp.masanphamKM == model.masanphamKM);
                    if (sanPham != null)
                    {
                        // Thêm dữ liệu khuyến mãi vào CSDL
                        db.KhuyenMais.Add(model);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Mã sản phẩm '{model.masanphamKM}' không tồn tại.");
                    }

                }

                return View(model); // Trả về model để hiển thị lỗi
            }

            // Xóa khuyến mãi
            public ActionResult Delete(int id)
            {
                var khuyenMai = db.KhuyenMais.Find(id);
                if (khuyenMai != null)
                {
                    db.KhuyenMais.Remove(khuyenMai);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

        }
    }
