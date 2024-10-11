using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class QuanLyDanhGiaController : Controller
    {
        luckEntities db = new luckEntities();

        // GET: DanhGia
        public ActionResult Index()
        {
            var danhGias = db.DanhGias.ToList();
            return View(danhGias);
        }

        // POST: Cập nhật đánh giá
        [HttpPost]
        public JsonResult UpdateDanhGia(string madanggia, string noidung, int sosao)
        {
            var danhGia = db.DanhGias.FirstOrDefault(d => d.madanggia == madanggia);
            if (danhGia != null)
            {
                danhGia.noidung = noidung;
                danhGia.sosao = sosao;
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // POST: Xóa đánh giá
        [HttpPost]
        public JsonResult DeleteDanhGia(string madanggia)
        {
            var danhGia = db.DanhGias.FirstOrDefault(d => d.madanggia == madanggia);
            if (danhGia != null)
            {
                db.DanhGias.Remove(danhGia);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // POST: Phản hồi đánh giá
        [HttpPost]
        public JsonResult ReplyToDanhGia(string madanggia, string phanHoi)
        {
            var danhGia = db.DanhGias.FirstOrDefault(d => d.madanggia == madanggia);
            if (danhGia != null)
            {
                danhGia.PhanHoi = phanHoi;
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
