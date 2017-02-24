using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMT.Models.DB;
using Microsoft.AspNet.Identity;
using SMT.Models;

namespace SMT.Controllers
{
    [Authorize, Credencial]
    public class HabilidadesController : Controller
    {
        public JsonResult CargarHabilidades(Guid grupo, int bimestre, int page = 0)
        {
            return Json(HabilidadesAlumno.cargarHabilidades(grupo, bimestre,Usuario.getIDVisor(User.Identity.GetUserId()), page, 20), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActualizarEstado(Guid alumno, string estado, int habilidad, int bimestre, Guid grupo)
        {
            try
            {
                    HabilidadesAlumno.actualizarEstado(alumno, habilidad,bimestre, estado, User.Identity.GetUserId(), grupo);

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Editar(long id, DateTime fecha, string observacion, string nombre)
        {
            try
            {
                //Trabajo.editar(id, fecha, observacion,Usuario.getIDVisor(User.Identity.GetUserId()), nombre);

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Eliminar(long id)
        {
            try
            {
                //Trabajo.eliminar(id,Usuario.getIDVisor(User.Identity.GetUserId()));

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public ActionResult Imprimir(Guid grupo, int bimestre)
        {
            SMTDevEntities db = new SMTDevEntities();
            List<Alumno> alumnos = db.Alumno.Where(i => i.IDGrupo == grupo).OrderBy(i => i.ApellidoPaterno).ThenBy(i => i.ApellidoMaterno).ThenBy(i => i.Nombre).ToList();
            ViewBag.alumnos = alumnos;

            List<HabilidadesAlumnoSimple> habs= HabilidadesAlumno.cargarHabilidades(grupo, bimestre, User.Identity.GetUserId(), 1, 3);
            ViewBag.header = new HeaderGrupoReporte(grupo, "Habilidades");
            return View(habs);
        }
    }
}