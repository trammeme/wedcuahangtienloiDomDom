using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class QuanLyUserController : Controller
    {
        luckEntities db = new luckEntities();

        // GET: QuanLyUser
        public ActionResult Index()
        {
            // Lấy danh sách người dùng từ bảng NguoiDungs
            var users = db.NguoiDungs.ToList();

            // Truyền danh sách người dùng tới view
            return View(users);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            // Giả sử MaKH là kiểu int
            var kq = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (kq == null)
            {
                return HttpNotFound(); // Xử lý nếu không tìm thấy người dùng
            }
            return PartialView("Edit", kq); // Trả về một partial view
        }

        [HttpPost]
        public ActionResult Edit(FormCollection f)
        {
            if (int.TryParse(f["maKH"], out int maKH)) // Chuyển đổi maKH thành int
            {
                // So sánh maKH với MaKH kiểu int
                NguoiDung kq = db.NguoiDungs.SingleOrDefault(n => n.MaKH == maKH);

                if (kq != null)
                {
                    kq.TenDangNhap = f["tenKH"];
                    kq.Email = f["email"];
                    kq.GioiTinh = f["gioitinh"];
                    kq.DiaChi = f["diachi"];
                    kq.SoDienThoai = f["sodienthoai"];
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            // Nếu chuyển đổi không thành công, bạn có thể xử lý ở đây
            return View();
        }

        [HttpGet]
        public ActionResult Detail(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user == null)
            {
                return HttpNotFound(); // Xử lý nếu không tìm thấy người dùng
            }
            return View(user); // Trả về view chi tiết
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user == null)
            {
                return HttpNotFound(); // Xử lý nếu không tìm thấy người dùng
            }
            return PartialView("Delete", user); // Trả về partial view để xác nhận xóa
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.NguoiDungs.SingleOrDefault(n => n.MaKH == id);
            if (user != null)
            {
                db.NguoiDungs.Remove(user); // Xóa người dùng
                db.SaveChanges(); // Lưu thay đổi
            }
            return RedirectToAction("Index"); // Quay lại danh sách
        }
    }
}
