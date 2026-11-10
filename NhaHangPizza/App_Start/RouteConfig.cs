using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NhaHangPizza
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Route tiện lợi: /Bep → DonHang/Bep
            routes.MapRoute(
                name: "Bep",
                url: "Bep",
                defaults: new { controller = "DonHang", action = "Bep" }
            );

            // Route tiện lợi: /ThuNgan → ThanhToan/ThuNgan
            routes.MapRoute(
                name: "ThuNgan",
                url: "ThuNgan",
                defaults: new { controller = "ThanhToan", action = "ThuNgan" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
