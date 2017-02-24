using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System.Linq;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize, Credencial]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            var secciones = User.GetSecciones();
            if (secciones.TotalEnabled == 1) {
                return
                    secciones.Grupos ? RedirectToAction("Index", "Grupos") :
                    secciones.Licencias ? RedirectToAction("Index", "Licencia") :
                    secciones.Tutor ? RedirectToAction("Index", "Tutor") :
                        RedirectToAction("Index", "Grupos");
            }
            else if(secciones.TotalEnabled == 0) {
                return RedirectToAction("Index", "Activacion");
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult CargarGrupos()
        {
            var UserActual = Usuario.getIDVisor(User.Identity.GetUserId());
            var Gr = Grupos.getGrupos(UserActual, 1, 0);
            return PartialView("_CargarGrupos",Gr);
        }
    }
}