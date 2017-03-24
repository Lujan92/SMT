using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMT.Models;
namespace SMT.Controllers
{
    [Authorize, Credencial]
    public class PortafolioController : Controller
    {
        public JsonResult listarPortafolio(Guid grupo, int bimestre, int page = 0)
        {
            return Json(Portafolio.cargarPortafolio(grupo, bimestre, Usuario.getIDVisor(User.Identity.GetUserId()), page, 30), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CargarPortafolio(Guid ID, Guid IDGrupo, int bimestre)
        {
            Portafolio res = Portafolio.getPortafolio(ID);
            SMTDevEntities db = new SMTDevEntities();
            if (res == null)
            {
                res = new Portafolio();
                res.IDGrupo = IDGrupo;
                res.IDBimestre = db.Bimestres.Where(b => b.IDGrupo == IDGrupo && b.Bimestre == bimestre).Select(b => b.IDBimestre).FirstOrDefault();
            }
            return PartialView("_Portafolio", res);
        }

        [HttpPost]
        public JsonResult GuardarPortafolio(Portafolio res, int bimestre = 1)
        {
            try
            {
                var db = new SMTDevEntities();

                //if (res.IDPortafolio > 0)
                //{
                //    //return Json(res.editar());
                //}

                var usuario = User.Identity.GetUserId();
                var bim = bimestre;
                Guid? idBimestre = db.Bimestres
                    .Where(i => i.IDGrupo == res.IDGrupo && i.Bimestre == bim && i.Grupos.IDUsuario == usuario)
                    .Select(i => i.IDBimestre)
                    .FirstOrDefault();
                if (idBimestre == default(Guid))
                    throw new Exception("No tienes acceso a este grupo");

                res.IDBimestre = idBimestre;
                var IDPortafolio = res.crear();
                List<Guid> alumnos = db.Alumno.Where(i => i.IDGrupo == res.IDGrupo).Select(i => i.IDAlumno).ToList();
                List<Models.DB.PortafolioAlumno> als = new List<PortafolioAlumno>();
                foreach (var m in alumnos)
                {
                    Models.DB.PortafolioAlumno alumno = new Models.DB.PortafolioAlumno()
                    {
                        IDPortafolioAlumno = Guid.NewGuid(),
                        IDAlumno = m,
                        IDPortafolio = IDPortafolio,
                        FechaActualizacion = DateTime.Now,
                        Estado = 1,
                        Aspecto1 = "0",
                        Aspecto2 = "0",
                        Aspecto3 = "0",
                        Aspecto4 = "0",
                        Aspecto5 = "0",

                    };
                    als.Add(alumno);
                    db.PortafolioAlumno.Add(alumno);
                }
                db.SaveChanges();
                return Json(new ResultViewModel(true, null, new
                {
                    IDPortafolio = res.IDPortafolio,
                    Fecha = Util.toHoraMexico(res.FechaEntrega.Value).ToString("dd-MM-yyyy"),
                    FechaEntrega = Util.toHoraMexico(res.FechaEntrega.Value).ToString("yyyy-MM-dd"),
                    observacion = res.Descripcion,
                    Nombre = res.Nombre,
                    Descripcion = res.Descripcion,
                    TipoTrabajo = res.TipoPortafolio != null ? res.TipoPortafolio.Nombre : "",
                    IDTipoPortafolio = res.IDTipoPortafolio,
                    Aspecto1 = res.Aspecto1,
                    Aspecto2 = res.Aspecto2,
                    Aspecto3 = res.Aspecto3,
                    Aspecto4 = res.Aspecto4,
                    Aspecto5 = res.Aspecto5,

                    Criterio1 = res.Criterio1,
                    Criterio2 = res.Criterio2,
                    Criterio3 = res.Criterio3,
                    Criterio4 = res.Criterio4,
                    Criterio5 = res.Criterio5,

                    Activo1 = res.Activo1,
                    Activo2 = res.Activo2,
                    Activo3 = res.Activo3,
                    Activo4 = res.Activo4,
                    Activo5 = res.Activo5,

                    Observacion1 = res.Observacion1,
                    Observacion2 = res.Observacion2,
                    Observacion3 = res.Observacion3,
                    Observacion4 = res.Observacion4,
                    Observacion5 = res.Observacion5,

                    Reactivo1 = res.Reactivo1,
                    Reactivo2 = res.Reactivo2,
                    Reactivo3 = res.Reactivo3,
                    Reactivo4 = res.Reactivo4,
                    Reactivo5 = res.Reactivo5,

                    entrega = als.Select(i => new
                    {
                        id = i.IDAlumno,
                        estado = i.Estado,
                        Aspecto1 = i.Aspecto1,
                        Aspecto2 = i.Aspecto2,
                        Aspecto3 = i.Aspecto3,
                        Aspecto4 = i.Aspecto4,
                        Aspecto5 = i.Aspecto5,
                    })
                                    .ToList()
                }));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public ActionResult Nuevo()
        {
            SMTDevEntities db = new SMTDevEntities();

            ViewBag.tipos = db.TipoPortafolio.OrderBy(i => i.Orden).Select(i => new
            {
                id = i.IDTipoPortafolio,
                name = i.Nombre
            })
            .ToList();

            return PartialView(new Models.DB.Portafolio()
            {
                Activo1 = true,
                Activo2 = true,
            });
        }

        public ActionResult editar(string id)
        {
            Guid? idPort = Guid.Empty;
            idPort = new Guid(id);

            SMTDevEntities db = new SMTDevEntities();

            ViewBag.tipos = db.TipoPortafolio.OrderBy(i => i.Orden).Select(i => new
            {
                id = i.IDTipoPortafolio,
                name = i.Nombre
            })
            .ToList();
            Portafolio port = db.Portafolio.FirstOrDefault(r => r.IDPortafolio == idPort);
            Portafolio port2 = db.Portafolio.Single(r => r.IDPortafolio == idPort);
            return PartialView(db.Portafolio.Single(r => r.IDPortafolio == idPort));
        }
        [HttpPost]
        public JsonResult Nuevo(Guid grupo, int bimestre, int estado)
        {
            try
            {
                return Json(new ResultViewModel(true, null, Trabajo.nueva(grupo, bimestre, estado, User.Identity.GetUserId(), "")));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult actualizarCalificacion(Guid alumno, Guid portafolio, string aspecto, string calificacion)
        {
            try
            {
                using (SMTDevEntities db = new SMTDevEntities())
                {
                    PortafolioAlumno a = db.PortafolioAlumno.FirstOrDefault(i => i.IDAlumno == alumno && i.IDPortafolio == portafolio);
                    if (a == null)
                    {
                        a = new PortafolioAlumno()
                        {
                            IDAlumno = alumno,
                            IDPortafolio = portafolio,
                            FechaActualizacion = DateTime.Now,

                        };
                        db.PortafolioAlumno.Add(a);
                        db.SaveChanges();
                    }

                    switch (aspecto)
                    {
                        case "Aspecto1":
                            a.Aspecto1 = calificacion;
                            break;
                        case "Aspecto2":
                            a.Aspecto2 = calificacion;
                            break;
                        case "Aspecto3":
                            a.Aspecto3 = calificacion;
                            break;
                        case "Aspecto4":
                            a.Aspecto4 = calificacion;
                            break;
                        case "Aspecto5":
                            a.Aspecto5 = calificacion;
                            break;
                    }

                    db.SaveChanges();

                    AlumnoDesempenio.actualizarAlumno(alumno, a.Portafolio.IDGrupo.Value, a.Portafolio.Bimestres.Bimestre.Value, new { portafolio = true });
                    
                    return Json(new ResultViewModel(true, null, null));
                }
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Editar(Portafolio portfolio)
        {
            try
            {
                return Json(new ResultViewModel(true, null, portfolio.editar(User.Identity.GetUserId())));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }


        [HttpPost]
        public JsonResult Calificacion(Guid id, Guid portafolio,Guid grupo)
        {
            try
            {
                Portafolio.calificacion(id, portafolio,grupo);
                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }


        [HttpPost]
        public JsonResult reactivos(Guid id)
        {
            try
            {
                
                using (SMTDevEntities db = new SMTDevEntities())
                {
                    Portafolio port = db.Portafolio.FirstOrDefault(i => i.IDPortafolio == id);

                    PortafolioAlumno alumno = db.PortafolioAlumno.FirstOrDefault(i => i.IDAlumno == id && i.IDPortafolio == port.IDPortafolio);
                    if (port == null)
                        throw new Exception("No existe este portafolio");

                    var TotalReactivos = port.Reactivo1 + port.Reactivo2 + port.Reactivo3 + port.Reactivo4 + port.Reactivo5;


                    return Json(TotalReactivos);
                }
                
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
                Portafolio.eliminar(id, User.Identity.GetUserId());
                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult ActualizarObservacion(Guid id, string aspecto, string observacion)
        {
            try
            {
                Portafolio.actualizarObservacion(id, aspecto, observacion, User.Identity.GetUserId());
                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public JsonResult GetDefecto(Guid tipo)
        {
            return Json(PortafolioDefecto.get(tipo, User.Identity.GetUserId()), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarPorDefecto(PortafolioDefecto defecto, Guid IDTipoPortafolio)
        {
            try
            {
                SMTDevEntities db = new SMTDevEntities();
                TipoPortafolio def = db.TipoPortafolio.FirstOrDefault(i => i.IDTipoPortafolio == IDTipoPortafolio);

                defecto.TipoTrabajo = def.Nombre;
                defecto.actualizar(User.Identity.GetUserId());
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

            List<Portafolio> portafolios = db.Portafolio.Where(i => i.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre).OrderBy(i => i.FechaEntrega).ToList();
            ViewBag.alumnos = db.Alumno.Where(i => i.IDGrupo == grupo).OrderBy(i => i.ApellidoPaterno).ThenBy(i => i.ApellidoMaterno).ThenBy(i => i.Nombre).ToList();
            ViewBag.tipos = db.TipoPortafolio.Where(i => i.Portafolio.Any(a => a.Bimestres.Bimestre == bimestre && a.Bimestres.IDGrupo == grupo)).OrderBy(i => i.Orden).ToList();
            ViewBag.header = new HeaderGrupoReporte(grupo, $"Portafolio bimestre {bimestre}");
            return View(portafolios);
        }
    }
}