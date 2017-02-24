using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize]
    public class ReporteEscuelaController : Controller
    {
        private SMTDevEntities db = new SMTDevEntities();

        public ActionResult Index() {
            return View();
        }

        public ActionResult Reporte() {
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public ActionResult Reporte(string ids, int bim) {
            ViewBag.Reporte = ReporteEscuela.GenerarReporte(ids.Split(',').Select(Guid.Parse), bim, db);
            return View();
        }

        [HttpPost]
        public JsonResult ListarGrupos() {
            //var data = db.Credencial
            //    .Where(i => i.EsPrincipal)
            //    .Select(o => o.IDUsuarioPaga)
            //    .ToList()
            //    .SelectMany(userid => ReporteEscuela.ListarGrupos(userid, db).AsEnumerable())
            //    .GroupBy(g => g.IDGrupo)
            //    .Select(grp => grp.FirstOrDefault());

            //return Json(new { result = true, data });

            return Json(new { result = true, data = ReporteEscuela.ListarGrupos(User.Identity.GetUserId(), db).AsEnumerable() });
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            db.Dispose();
        }
    }
}