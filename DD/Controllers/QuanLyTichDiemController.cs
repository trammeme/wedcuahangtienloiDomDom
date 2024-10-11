using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{

    public class QuanLyTichDiemController : Controller
    {
        luckEntities db = new luckEntities();

        // GET: QuanLyTichDiem
        public ActionResult Index()
        {
            var result = from user in db.NguoiDungs
                         join point in db.TichDiems on user.MaKH equals point.maKH into points
                         from point in points.DefaultIfEmpty() // Thực hiện LEFT JOIN
                         select new UserPointViewModel
                         {
                             MaKH = user.MaKH,
                             TenKH = user.TenKH,
                             Email = user.Email,
                             ThangSinh = user.ThangSinh,
                             Diem = point != null ? point.diem : 0, // Nếu không có điểm thì là 0
                             SoDienThoai = user.SoDienThoai // Gán giá trị SoDienThoai
                         };

            return View(result.ToList());
        }

        public ActionResult ManagePoints()
        {
            var result = (from td in db.TichDiems
                          join nd in db.NguoiDungs on td.maKH equals nd.MaKH
                          select new
                          {
                              nd.MaKH,
                              nd.TenKH,
                              nd.SoDienThoai,
                              td.diem
                          }).ToList();
            bool success = true; // Thay đổi theo logic của bạn

            return Json(new { success });
        }

        [HttpPost]
        public JsonResult CapThanhVien(int maKH, string capBac)
        {
            // Lấy thông tin điểm tích lũy của người dùng
            var tichDiem = db.TichDiems.FirstOrDefault(td => td.maKH == maKH);

            if (tichDiem != null)
            {
                // Cập nhật cấp bậc
                tichDiem.CapBac = capBac; // Đảm bảo rằng thuộc tính này tồn tại trong model TichDiem
                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}
