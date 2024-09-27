using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DD.Controllers
{
    public class TrangChuAdminController : Controller
    {
        // GET: TrangChuAdmin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UserManagemetnPartial()
        {
            return PartialView("_UserManagementPartial");
        }
    }
}