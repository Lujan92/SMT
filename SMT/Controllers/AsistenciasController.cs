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
    public class AsistenciasController : Controller
    {
        public JsonResult CargarSesiones(Guid grupo,int bimestre,int page = 0)
        {
            return Json(Sesion.cargarSesiones(grupo,bimestre, Usuario.getIDVisor(User.Identity.GetUserId()), page, 3000),JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ActualizarEstado(Guid alumno, Guid sesion, int estado)
        {
            try
            {
                Alumno.actualizarEstadoSesion(alumno, sesion, estado, User.Identity.GetUserId());

                return Json(new ResultViewModel(true,null,null));
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Nuevo(Guid grupo, int bimestre, int estado)
        {
            try
            {
                return Json(new ResultViewModel(true,null,Sesion.nueva(grupo,bimestre,estado, User.Identity.GetUserId())));
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Editar(Guid id, DateTime fecha, string observacion)
        {
            try
            {
                observacion = observacion.Trim();
                Sesion.editar(id, fecha, observacion, User.Identity.GetUserId());

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
                Sesion.eliminar(id, User.Identity.GetUserId());

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public ActionResult Imprimir(Guid grupo, long bimestre)
        {
            SMTDevEntities db = new SMTDevEntities();
            List<AlumnoSimple> alumnos = Alumno.listaSimple(grupo, User.Identity.GetUserId());

            ViewBag.alumnos = alumnos;
            ViewBag.header = new HeaderGrupoReporte(grupo, $"Asistencias bimestre {bimestre}");

            List<AsistenciaViewModal> asistencias = db.Sesion.Where(i => i.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre)
                                                              .OrderBy(i => i.Fecha)
                                                              .ToList()
                                                              .Select(i => new AsistenciaViewModal()
                                                              {
                                                                  sesion = Util.toHoraMexico(i.Fecha).ToString("dd/MM/yyyy"),
                                                                  alumnos = i.AlumnoSesion.Select(a => new AsistenciaViewModal.AlumnoAsistencia()
                                                                  {
                                                                      estado = a.Estado,
                                                                      idAlumno = a.IDAlumno
                                                                  })
                                                                  .ToList()
                                                              })
                                                              .ToList();

            return View(asistencias);
        }
    }
}