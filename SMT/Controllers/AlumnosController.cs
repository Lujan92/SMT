using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static SMT.Models.DB.Alumno;

namespace SMT.Controllers
{
    [Authorize, Credencial]
    public class AlumnosController : Controller
    {       

        public JsonResult ListaSimple(Guid grupo)
        {
            return Json(Alumno.listaSimple(grupo, Usuario.getIDVisor(User.Identity.GetUserId())),JsonRequestBehavior.AllowGet);
        }

        public ActionResult CargarAlumno(Guid? ID, Guid? IDGrupo, bool? Tipo = false)
        {
            var res = Alumno.getAlumno(ID ?? default(Guid));
            if (res == null) {
                res = new Alumno();
                res.IDAlumno = default(Guid);
                res.IDGrupo = IDGrupo ?? Guid.NewGuid();
            }

            res.EsTaller = Tipo;
            
            return PartialView("_Alumno", res);
        }

        public JsonResult GuardarAlumno(Alumno res)
        {
            try {
                res.Nombre = res.Nombre.Trim();
                res.ApellidoPaterno = res.ApellidoPaterno.Trim();
                res.ApellidoMaterno = res.ApellidoMaterno.Trim();
                res.Curp = res.Curp.Trim();

                if (res.IDAlumno != default(Guid)) {
                    return Json(res.editar());
                }

                return Json(res.crear());
            }
            catch (Exception e)
            {
                return Json(e.InnerException != null ? e.InnerException.Message : e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EliminarAlumno(Guid ID)
        {
            return Json(Alumno.eliminar(ID));
        }

        public JsonResult ImportarAlumnos(Alumno alumno)
        {
            try
            {
                var res = "";
                if (alumno.archivo.Count()>0)
                {
                    for (int i = 0; i < alumno.archivo.Count(); i++) {
                        MemoryStream stream = new MemoryStream();
                        alumno.archivo[i].InputStream.CopyTo(stream);
                        var tipo = Util.GetFileType(alumno.archivo[i].FileName);
                        if (tipo == 0) {
                            return Json("El tipo de archivo no es el correcto");
                        }

                        res = Alumno.ImportData(stream,tipo , alumno.IDGrupo);
                    }
                    if (res != "Ok") {
                        return Json(res);
                    }
                    return Json(1);
                }
                return Json(0);
            }
            catch (Exception e)
            {
                return Json(e.InnerException != null ? e.InnerException.Message : e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DescargarTemplateAlumnos()
        {
            FileStream file = new FileStream(Server.MapPath("~/Content/Plantilla_Alumnos.xlsx"), FileMode.Open, FileAccess.Read);
            MemoryStream stream = new MemoryStream();
            file.CopyTo(stream);

            byte[] byteArray = stream.ToArray();

            Response.ClearContent();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", "attachment;filename=Plantilla.xlsx");
            Response.AppendHeader("Content-Length", byteArray.Length.ToString());
            Response.BinaryWrite(byteArray);
            Response.End();
            return new EmptyResult();
        }
        
        public ActionResult DescargarTemplateAlumnosTaller()
        {
            FileStream file = new FileStream(Server.MapPath("~/Content/Plantilla_AlumnosTaller.xlsx"), FileMode.Open, FileAccess.Read);
            MemoryStream stream = new MemoryStream();
            file.CopyTo(stream);

            byte[] byteArray = stream.ToArray();

            Response.ClearContent();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", string.Format("attachment;filename=Plantilla.xlsx"));
            Response.AppendHeader("Content-Length", byteArray.Length.ToString());
            Response.BinaryWrite(byteArray);
            Response.End();
            return new EmptyResult();
        }

        #region buscador Alumnos

        public ActionResult Buscador(string nombre="")
        {
            return View();
        }

        public JsonResult BuscarAlumnos(int page = 1, int pageSize = 20,string nombre="")
        {
            return Json(Alumno.busqueda(Usuario.getIDVisor(User.Identity.GetUserId()), nombre, page, pageSize), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetalleAlumno(Guid IDAlumno, Guid? IDGrupo)
        {
            Alumno alm = Alumno.getAlumno(IDAlumno);
            alm.sesionArray = Alumno.getAlumnoSesiones(IDAlumno, alm.IDGrupo);
            alm.trabajoArray = Alumno.getAlumnoTrabajos(IDAlumno, alm.IDGrupo);
            alm.portafolioArray= Alumno.getAlumnoPortafolio(IDAlumno, alm.IDGrupo);
            alm.examenArray = Alumno.getAlumnoExamen(IDAlumno, alm.IDGrupo);
            return View(alm);
        }
        #endregion region

        public JsonResult ActualizarDesempenio(Guid? grupo)
        {
            return Json(AlumnoDesempenio.actualizacionGeneral(grupo),JsonRequestBehavior.AllowGet);
        }

        public JsonResult CargarSemaforo(Guid grupo, int bimestre, string seccion, Guid? alumno)
        {

            return Json(AlumnoDesempenio.cargarSemaforos(grupo,bimestre,seccion, Usuario.getIDVisor(User.Identity.GetUserId()),alumno),JsonRequestBehavior.AllowGet);

        }

        public JsonResult CargarReporte(Guid grupo,long bimestre, Guid? alumno)
        {
            try
            {
                List<AlumnoReporteViewModel> alumnos = AlumnoDesempenio.cargarReporte(grupo,bimestre,alumno);
                List<AlumnoReporteViewModel> alumnos2 = AlumnoDesempenio.cargarReporteGeneral(grupo, alumno);

                var promediosFinales = alumnos2.Select(a => new
                {
                    a.id,
                    a.promedioFinal,
                }).ToList();

                var promedioGrupal = new
                {
                    bimestre1 = alumnos2.Select(a => a.promedioExamenParcial2Bimestre1).Average(),
                    bimestre2 = alumnos2.Select(a => a.promedioExamenParcial2Bimestre2).Average(),
                    bimestre3 = alumnos2.Select(a => a.promedioExamenParcial2Bimestre3).Average(),
                    bimestre4 = alumnos2.Select(a => a.promedioExamenParcial2Bimestre4).Average(),
                    bimestre5 = alumnos2.Select(a => a.promedioExamenParcial2Bimestre5).Average(),
                };


                return Json(new
                {
                    headers = AlumnoDesempenio.cargarHeadersDeReporte(grupo, bimestre),
                    alumnos = alumnos,
                    resumen = AlumnoDesempenio.calcularResumen(alumnos2),
                    promediosFinales = promediosFinales,
                    promedioGrupal = promedioGrupal,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(new {
                    headers = new List<dynamic>(),
                    alumnos = new List<dynamic>(),
                    resumen = new List<dynamic>(),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Imprimir(Guid grupo)
        {
            List<AlumnoSimple> alumnos = Alumno.listaSimple(grupo, Usuario.getIDVisor(User.Identity.GetUserId()));
            ViewBag.header = new HeaderGrupoReporte(grupo);
            return View(alumnos);
        }

        public ActionResult Detalle(Guid id, Guid? IDGrupo) {
            Alumno alm = Alumno.getAlumno(id);
            alm.sesionArray = Alumno.getAlumnoSesiones(id, alm.IDGrupo);
            alm.trabajoArray = Alumno.getAlumnoTrabajos(id, alm.IDGrupo);
            alm.portafolioArray = Alumno.getAlumnoPortafolio(id, alm.IDGrupo);
            alm.examenArray = Alumno.getAlumnoExamen(id, alm.IDGrupo);
            return View(alm);
        }
    }
}