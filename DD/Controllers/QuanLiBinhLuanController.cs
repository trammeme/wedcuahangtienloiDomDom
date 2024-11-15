using DD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class QuanLiBinhLuanController : Controller
    {
        camlyEntities1 db = new camlyEntities1();

        public ActionResult Index()
        {
            var danhGias = db.BinhLuans.ToList();
            return View(danhGias);
        }

      /*  [HttpPost]
        public JsonResult ReplyToBinhLuan(string mabinhluan, string phanhoi)
        {
            if (string.IsNullOrEmpty(mabinhluan))
            {
                return Json(new { success = false, message = "Mã bình luận không hợp lệ" });
            }

            var binhLuan = db.BinhLuans.FirstOrDefault(d => d.MaBinhLuan == mabinhluan);
            
            if (binhLuan != null)
            {
                try
                {
                    binhLuan.PhanHoi = phanhoi;
                    binhLuan.NgayPhanHoi = DateTime.Now;  // Assuming you have this field
                    db.SaveChanges();
                    return Json(new { 
                        success = true, 
                        message = "Phản hồi đã được cập nhật",
                        ngayPhanHoi = binhLuan.NgayPhanHoi.Value.ToString("dd/MM/yyyy HH:mm")
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi cập nhật phản hồi: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Không tìm thấy bình luận" });
        }*/

        [HttpPost]
        public ActionResult DeleteBinhLuan(string mabinhluan)
        {
            if (string.IsNullOrEmpty(mabinhluan))
            {
                TempData["Error"] = "Mã bình luận không hợp lệ";
                return RedirectToAction("Index");
            }

            var binhLuan = db.BinhLuans.FirstOrDefault(d => d.MaBinhLuan == mabinhluan);
            
            if (binhLuan != null)
            {
                try
                {
                    db.BinhLuans.Remove(binhLuan);
                    db.SaveChanges();
                    TempData["Success"] = "Xóa bình luận thành công";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa bình luận: " + ex.Message;
                }
            }
            else
            {
                TempData["Error"] = "Không tìm thấy bình luận";
            }

            return RedirectToAction("Index");
        }
    }
}