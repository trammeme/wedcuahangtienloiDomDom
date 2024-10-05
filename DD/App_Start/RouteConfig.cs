using System.Linq;
using System.Web.Mvc;
using DD.Models; // Make sure to include your models namespace  
using System.Web.Mvc;
using System.Web.Routing; // Đây là chỉ thị cần thiết cho RouteCollection

namespace DD
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}