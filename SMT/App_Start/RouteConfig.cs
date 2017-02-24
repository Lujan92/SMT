using System.Web.Mvc;
using System.Web.Routing;

namespace SMT
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Bimestre",
                url: "grupos/{grupo}/Bimestre/{bimestre}",
                defaults: new { controller = "Bimestre", action = "Index", bimestre = UrlParameter.Optional, grupo = UrlParameter.Optional },
                namespaces: new[] { "SMT.Controllers" }
            );

            routes.MapRoute(
              name: "Portafolio",
              url: "Instrumentos/{action}/{id}",
              defaults: new { controller = "Portafolio", action = "Index", id = UrlParameter.Optional },
              namespaces: new[] { "SMT.Controllers" }
          );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "SMT.Controllers" }
            );

            routes.MapRoute(
                name: "Tutor",
                url: "Tutor/Detalle/{idAlumno}/{bimestre}",
                defaults: new { controller = "Tutor", action = "Detalle" },
                namespaces: new[] { "SMT.Controllers" }
            );
        }
    }
}
