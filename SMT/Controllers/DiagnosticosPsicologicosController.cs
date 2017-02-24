using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static SMT.Models.DB.Alumno;

namespace SMT.Controllers
{
    [Authorize, Credencial]
    public class DiagnosticosPsicologicosController : Controller
    {
        public ActionResult Index(Guid id)
        {
            if (!Grupos.validarMaestro(id, Usuario.getIDVisor(User.Identity.GetUserId())))
            {
                return RedirectToAction("SinAcceso","Bimestre");
            }
            else
            {
                Grupos grupo = Grupos.get(id);

                return View(grupo);
            }
        }

        public JsonResult Listar(Guid grupo)
        {

            return Json(DiagnosticoPsicologico.listar(grupo),JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Actualizar(DiagnosticoPsicologico diag)
        {
            try
            {
                diag.editar();

                return Json(new ResultViewModel(true,null,null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public ActionResult Imprimir(Guid grupo)
        {
            List<ExamenViewModel> examenes = new List<ExamenViewModel>();


            SMTDevEntities db = new SMTDevEntities();
            ViewBag.header = new HeaderGrupoReporte(grupo, "Disgnósticos psicológicos");


            List<AlumnoSimple> alumnos = Alumno.listaSimple(grupo, User.Identity.GetUserId());
            ViewBag.alumnos = alumnos;

            var result = db.DiagnosticoPsicologico.Where(a => a.IDGrupo == grupo).ToList();

            return View(result);
        }
    }
}