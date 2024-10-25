using DD.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Data.Entity;

namespace DD.Controllers
{
    public class HomeController : Controller
    {
        private readonly thuaEntities1 db = new thuaEntities1();

        public ActionResult Index()
        {
            var khuyenMais = db.KhuyenMais.ToList(); // Lấy danh sách khuyến mãi
            var danhSachSanPham = db.SanPhams.ToList(); // Lấy danh sách sản phẩm

            // Tạo một view model
            var model = new MyViewModel
            {
                SanPhams = danhSachSanPham, // Truyền danh sách sản phẩm từ db
                KhuyenMais = khuyenMais // Truyền danh sách khuyến mãi từ db
            };

            return View(model); // Truyền model sang view
        }

        public ActionResult Slider()
        {
            return View();
        }

        public ActionResult ProductDetail(int id)
        {
            var book = (from s in db.SanPhams where s.MaSanPham == id select s).SingleOrDefault();

            if (book == null)
            {
                return HttpNotFound(); // Nếu không tìm thấy sách
            }

            return View(book);
        }

        public ActionResult Detail(int id)
        {
            var product = db.SanPhams
                .Include("BinhLuans.NguoiDung")
                .FirstOrDefault(p => p.MaSanPham == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Sắp xếp bình luận theo thời gian mới nhất
            product.BinhLuans = product.BinhLuans
                .Where(b => b.MaSanPham == id)
                .OrderByDescending(b => b.NgayBinhLuan)
                .ToList();

            return View(product);
        }

        public ActionResult GetComments(int masanpham)
        {
            var comments = db.BinhLuans
                .Where(b => b.MaSanPham == masanpham)
                .OrderByDescending(b => b.NgayBinhLuan)
                .Include(b => b.NguoiDung)
                .ToList();

            return PartialView("BinhLuanPartial", comments);
        }

        [HttpPost]
        public ActionResult AddComment(int masanpham, string noiDung)
        {
            try
            {
                if (Session["User"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để bình luận" });
                }

                var user = Session["User"] as NguoiDung;
                if (user == null)
                {
                    return Json(new { success = false, message = "Thông tin người dùng không hợp lệ" });
                }

                int maKH = user.MaKH;
                if (maKH <= 0)
                {
                    return Json(new { success = false, message = "Thông tin người dùng không hợp lệ" });
                }

                // Kiểm tra sản phẩm có tồn tại
                var product = db.SanPhams.Find(masanpham);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                var comment = new BinhLuan
                {
                    MaSanPham = masanpham,
                    NoiDung = noiDung,
                    MaKH = maKH,
                    MaBinhLuan = Guid.NewGuid().ToString(),
                    NgayBinhLuan = DateTime.Now
                };

                db.BinhLuans.Add(comment);
                db.SaveChanges();

                var commentInfo = new
                {
                    success = true,
                    productId = masanpham,
                    authorName = user.TenKH,
                    content = noiDung,
                    date = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    message = "Bình luận đã được thêm thành công"
                };

                return Json(commentInfo);
            }
            catch (DbEntityValidationException dbEx)
            {
                var errors = dbEx.EntityValidationErrors
                    .SelectMany(validationResult => validationResult.ValidationErrors)
                    .Select(validationError => validationError.ErrorMessage);

                return Json(new { success = false, message = "Có lỗi xảy ra: " + string.Join(", ", errors) });
            }
        }
    }
    }
