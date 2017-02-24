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
    public class TrabajosController : Controller
    {
        public JsonResult CargarTrabajos(Guid grupo, int bimestre, int page = 0)
        {
            return Json(Trabajo.cargarTrabajos(grupo, bimestre, Usuario.getIDVisor(User.Identity.GetUserId()), page, 30), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Nuevo(Guid grupo, int bimestre, int estado,string tipo)
        {
            try
            {
                return Json(new ResultViewModel(true, null, Trabajo.nueva(grupo, bimestre, estado, User.Identity.GetUserId(), tipo)));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult ActualizarEstado(Guid alumno, Guid sesion, int estado)
        {
            try
            {
                TrabajoAlumno.actualizarEstado(alumno, sesion, estado, User.Identity.GetUserId());



                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Editar(Guid id, DateTime fecha, string observacion,string nombre,string tipo, string actividad)
        {
            try
            {
                Trabajo.editar(id, fecha, observacion, User.Identity.GetUserId(), nombre,tipo,actividad);

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Eliminar(Guid id)
        {
            try
            {
                Trabajo.eliminar(id, User.Identity.GetUserId());

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public ActionResult CargarTrabajo(Guid ID)
        {
            return PartialView("_Trabajo", Trabajo.get(ID));
        }
                
        public JsonResult GuardarTrabajo(Trabajo res)
        {
            try
            {                                
                return Json(res.editarDescripcion(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.InnerException != null ? e.InnerException.Message : e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Imprimir(Guid grupo, int bimestre)
        {
            SMTDevEntities db = new SMTDevEntities();

            List<Trabajo> tra = db.Trabajo.Where(a => a.IDGrupo == grupo && a.Bimestres.Bimestre == bimestre).OrderBy(i => i.Fecha).ToList();
            List<Alumno> alumnos = db.Alumno.Where(i => i.IDGrupo == grupo).OrderBy(i => i.ApellidoPaterno).ThenBy(i => i.ApellidoMaterno).ThenBy(i => i.Nombre).ToList();
            ViewBag.alumnos = alumnos;
            ViewBag.header = new HeaderGrupoReporte(grupo, $"Trabajos bimestre {bimestre}");
            return View(tra);
        }
        
    }
}