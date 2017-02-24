using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize]
    public class DesktopApiController : Controller
    {
        private SMTDevEntities db = new SMTDevEntities();

        [HttpPost]
        public JsonResult SincronizarGrupos(ICollection<Grupos> Grupos)
        {
            foreach(var grupo in (Grupos ?? new List<Grupos>())) {
                var dbGrupo = db.Grupos.FirstOrDefault(g => g.IDGrupo == grupo.IDGrupo);
                if (dbGrupo != null && grupo.FechaSync > dbGrupo.FechaSync) {
                    dbGrupo.Ciclo = grupo.Ciclo;
                    dbGrupo.Color = grupo.Color;
                    dbGrupo.Descripcion = grupo.Descripcion;
                    dbGrupo.Escuela = grupo.Escuela;
                    dbGrupo.EsTaller = grupo.EsTaller;
                    dbGrupo.Grado = grupo.Grado;
                    dbGrupo.Grupo = grupo.Grupo;
                    dbGrupo.Materia = grupo.Materia;
                    dbGrupo.RegistroFederalEscolar = grupo.RegistroFederalEscolar;
                    dbGrupo.Status = grupo.Status;
                    dbGrupo.Turno = grupo.Turno;
                    db.SaveChanges();
                }
                else if (dbGrupo == null) {
                    dbGrupo = new Grupos();
                    dbGrupo.Ciclo = grupo.Ciclo;
                    dbGrupo.Color = grupo.Color;
                    dbGrupo.Descripcion = grupo.Descripcion;
                    dbGrupo.EsTaller = grupo.EsTaller;
                    dbGrupo.Grado = grupo.Grado;
                    dbGrupo.RegistroFederalEscolar = grupo.RegistroFederalEscolar;
                    dbGrupo.Status = grupo.Status ?? 1;
                    dbGrupo.Turno = grupo.Turno;
                    dbGrupo.IDUsuario = User.Identity.GetUserId();

                    dbGrupo.Grupo = grupo.Grupo.ToUpper();
                    dbGrupo.Materia = Util.UpperTitle(grupo.Materia);
                    dbGrupo.Escuela = Util.UpperTitle(grupo.Escuela);
                    dbGrupo.Timestamp = DateTime.Now;
                    dbGrupo.IDGrupo = grupo.IDGrupo == default(Guid) ? Guid.NewGuid() : grupo.IDGrupo;

                    try {
                        db.Grupos.Add(dbGrupo);
                        db.SaveChanges();
                    } catch (Exception ex) {
                        Debug.WriteLine("[SincronizarGrupos]");
                        Debug.Write(ex);
                        Debug.Write(ex.InnerException);
                        continue;
                    }

                    for (int i = 1; i <= 5; i++) {
                        var bim = new Bimestres();
                        bim.IDGrupo = dbGrupo.IDGrupo;
                        bim.Bimestre = i;
                        bim.crear();
                    }
                }
            }

            var grupos = Models.DB.Grupos.getGrupos(User.Identity.GetUserId(), 1, 0).Select(o => new {
                o.Ciclo, o.Color, o.Descripcion, o.Escuela, o.EsTaller,
                o.Grado, o.Grupo, o.IDGrupo, o.IDUsuario, o.Materia,
                o.RegistroFederalEscolar, o.Status, o.Turno
            }).ToList();

            return Json(new ResultViewModel(true, "", grupos));
        }

        [HttpPost]
        public JsonResult SincronizarAlumnos(ICollection<AlumnoViewModel> alumnos) {
            foreach(var alumno in (alumnos ?? new List<AlumnoViewModel>())) {
                var dbAlumno = db.Alumno.FirstOrDefault(o => o.IDAlumno == alumno.IDAlumno);

                if (dbAlumno != null && alumno.FechaSync > dbAlumno.FechaSync) {
                    if(alumno.Deleted) {
                        dbAlumno.AlumnoSesion.Clear();
                        dbAlumno.TrabajoAlumno.Clear();
                        dbAlumno.ExamenAlumno.Clear();
                        dbAlumno.PortafolioAlumno.Clear();
                        db.Alumno.Remove(dbAlumno);
                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarAlumnos]");
                            Debug.Write(ex);
                        }
                        continue;
                    }

                    dbAlumno.Nombre = alumno.Nombre.Trim();
                    dbAlumno.ApellidoPaterno = alumno.ApellidoPaterno.Trim();
                    dbAlumno.ApellidoMaterno = alumno.ApellidoMaterno.Trim();
                    dbAlumno.Curp = alumno.Curp.Trim();
                    try {
                        dbAlumno.editar();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarAlumnos]");
                        Debug.Write(ex);
                    }
                }
                else if (dbAlumno == null && !alumno.Deleted) {
                    dbAlumno = new Alumno();
                    dbAlumno.Nombre = alumno.Nombre.Trim();
                    dbAlumno.ApellidoPaterno = alumno.ApellidoPaterno.Trim();
                    dbAlumno.ApellidoMaterno = alumno.ApellidoMaterno.Trim();
                    dbAlumno.Curp = alumno.Curp.Trim();
                    dbAlumno.IDGrupo = alumno.IDGrupo;
                    dbAlumno.IDAlumno = alumno.IDAlumno;
                    dbAlumno.EsUSAER = alumno.EsUSAER;
                    dbAlumno.FechaSync = DateTime.Now;

                    try {
                        dbAlumno.crear();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarAlumnos]");
                        Debug.Write(ex);
                    }
                }
            }

            return Json(new ResultViewModel(true, "", RetrieveAlumnos(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarAsistencia(ICollection<AsistenciaViewModel> sesiones) {
            foreach (var sesion in (sesiones ?? new List<AsistenciaViewModel>())) {
                var dbSesion = db.Sesion.FirstOrDefault(o => o.IDSesion == sesion.id);
                if (dbSesion != null) {
                    if (sesion.Deleted) {
                        dbSesion.AlumnoSesion.Clear();
                        db.Sesion.Remove(dbSesion);
                        try {
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(dbSesion.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarAsistencia]");
                            Debug.Write(ex);
                        }
                        continue;
                    }

                    if (sesion.FechaSync.HasValue && sesion.FechaSync > dbSesion.FechaSync) {
                        dbSesion.Fecha = sesion.fecha.AddHours(12);
                        dbSesion.Observacion = sesion.observacion;
                        dbSesion.FechaSync = DateTime.Now;
                    }

                    foreach (var sesAlumno in (sesion.asistencia ?? new List<AsistenciaAlumnoViewModel>())) {
                        var dbSesAlumno = dbSesion.AlumnoSesion.FirstOrDefault(o => o.IDAlumno == sesAlumno.id);
                        
                        if (dbSesAlumno != null && dbSesAlumno.FechaSync < sesAlumno.FechaSync) {
                            dbSesAlumno.Estado = sesAlumno.estado;
                            dbSesAlumno.FechaSync = DateTime.Now;
                        }
                        else if(dbSesAlumno == null) {
                            dbSesion.AlumnoSesion.Add(new AlumnoSesion {
                                IDAlumno = sesAlumno.id,
                                Estado = sesAlumno.estado,
                                FechaSync = DateTime.Now
                            });
                        }
                    }

                    try {
                        db.SaveChanges();
                        Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(dbSesion.IDGrupo));
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarAsistencia]");
                        Debug.Write(ex);
                    }
                }
                else if(!sesion.Deleted) {
                    var idBimestre = db.Bimestres
                        .Where(o => o.IDGrupo == sesion.IDGrupo && o.Bimestre == sesion.Bimestre)
                        .Select(o => o.IDBimestre)
                        .FirstOrDefault();

                    dbSesion = new Sesion {
                        IDSesion = sesion.id,
                        IDGrupo = sesion.IDGrupo,
                        IDBimestre = idBimestre,
                        Fecha = sesion.fecha,
                        FechaSync = DateTime.Now,
                        Observacion = sesion.observacion
                    };

                    try {
                        dbSesion = db.Sesion.Add(dbSesion);
                        try {
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(sesion.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarAsistencia]");
                            Debug.Write(ex);
                        }
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarAsistencia]");
                        Debug.Write(ex);
                        Debug.Write(ex.InnerException);
                        continue;
                    }

                    if (sesion.asistencia != null && sesion.asistencia.Any()) {
                        var asistencias = sesion.asistencia.Select(a => new AlumnoSesion {
                            IDAlumno = a.id,
                            Estado = a.estado,
                            FechaSync = DateTime.Now
                        }).ToList();

                        asistencias.ForEach(dbSesion.AlumnoSesion.Add);

                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarAsistencia]");
                            Debug.Write(ex);
                        }
                    }
                }
            }

            return Json(new ResultViewModel(true, "", RetrieveSesiones(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarTrabajos(ICollection<TrabajoViewModel> trabajos) {
            foreach (var trabajo in (trabajos ?? new List<TrabajoViewModel>())) {
                var dbTrabajo = db.Trabajo.FirstOrDefault(o => o.IDTrabajo == trabajo.id);

                if (dbTrabajo != null) {
                    if (trabajo.Deleted) {
                        dbTrabajo.TrabajoAlumno.Clear();
                        db.Trabajo.Remove(dbTrabajo);

                        try {
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(dbTrabajo.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarTrabajos]");
                            Debug.Write(ex);
                        }
                        continue;
                    }

                    if (trabajo.FechaSync > dbTrabajo.FechaSync) {
                        dbTrabajo.Nombre = Util.UpperTitle(trabajo.nombre);
                        dbTrabajo.Fecha = trabajo.fecha.AddMinutes(30);
                        dbTrabajo.Observaciones = trabajo.observacion;
                        dbTrabajo.Actividad = trabajo.actividad;
                        dbTrabajo.Tipo = trabajo.tipo;
                        dbTrabajo.FechaSync = DateTime.Now;
                        try {
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(trabajo.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarTrabajos]");
                            Debug.Write(ex);
                        }
                    }

                    foreach (var entrega in (trabajo.entrega ?? new List<EntregaAlumnoViewModel>())) {
                        var dbEntrega = dbTrabajo.TrabajoAlumno.FirstOrDefault(e => e.IDAlumno == entrega.id);
                        if (dbEntrega != null && entrega.FechaSync > dbEntrega.FechaSync) {
                            dbEntrega.Estado = entrega.estado;
                            dbEntrega.FechaSync = DateTime.Now;
                        }
                        else if (entrega == null) {
                            dbTrabajo.TrabajoAlumno.Add(new TrabajoAlumno {
                                IDAlumno = entrega.id,
                                Estado = entrega.estado,
                                FechaSync = DateTime.Now,
                                FechaActualizacion = DateTime.Now
                            });
                        }

                        try {
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(trabajo.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarTrabajos]");
                            Debug.Write(ex);
                        }
                    }
                }
                else if (dbTrabajo == null) {
                    var idBimestre = db.Bimestres.Where(i => i.IDGrupo == trabajo.IDGrupo && i.Bimestre == trabajo.Bimestre).Select(i => i.IDBimestre).FirstOrDefault();
                    var contador = db.Trabajo.Where(i => i.IDGrupo == trabajo.IDGrupo && i.Bimestres.Bimestre == trabajo.Bimestre).Count();

                    dbTrabajo = new Trabajo {
                        IDTrabajo = trabajo.id,
                        Fecha = trabajo.fecha.AddMinutes(30),
                        IDGrupo = trabajo.IDGrupo,
                        IDBimestre = idBimestre,
                        Observaciones = trabajo.observacion,
                        Nombre = "Trabajo-" + (contador + 1),
                        Tipo = trabajo.tipo
                    };

                    try {
                        db.Trabajo.Add(dbTrabajo);
                        db.SaveChanges();
                        Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(trabajo.IDGrupo));
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarTrabajos]");
                        Debug.Write(ex);
                        continue;
                    }

                    trabajo.entrega = (trabajo.entrega ?? new List<EntregaAlumnoViewModel>());

                    var entregas = trabajo.entrega.Select(e => e.id).ToList();
                    var alumnos = db.Alumno
                        .Where(a =>  a.IDGrupo == trabajo.IDGrupo && !entregas.Contains(a.IDAlumno))
                        .Select(a => new EntregaAlumnoViewModel {
                            id = a.IDAlumno,
                            estado = 1,
                        }).ToList();

                    trabajo.entrega.AddRange(alumnos);

                    foreach (var entrega in trabajo.entrega) {
                        db.TrabajoAlumno.Add(new TrabajoAlumno {
                            IDTrabajoAlumno = Guid.NewGuid(),
                            IDAlumno = entrega.id,
                            IDTrabajo = trabajo.id,
                            FechaActualizacion = DateTime.Now,
                            Estado = entrega.estado
                        });

                        try {
                            db.SaveChanges();
                            AlumnoDesempenio.actualizarAlumno(entrega.id, trabajo.IDGrupo, trabajo.Bimestre, new { trabajo = true });
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarTrabajos]");
                            Debug.Write(ex);
                        }
                    }
                }
            }

            return Json(new ResultViewModel(true, "", RetrieveTrabajos(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarExamenes(ICollection<ExamenApiViewModel> examenes) {
            foreach (var examen in (examenes ?? new List<ExamenApiViewModel>())) {
                var dbExamen = db.Examen.FirstOrDefault(o => o.IDExamen == examen.IDExamen);

                if (dbExamen != null && examen.FechaSync > dbExamen.FechaSync) {
                    if(examen.Deleted) {
                        try {
                            db.Examen.Remove(dbExamen);
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(examen.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }

                        continue;
                    }

                    dbExamen.FechaActualizacion = DateTime.Now;
                    dbExamen.FechaSync = DateTime.Now;
                    dbExamen.FechaEntrega = examen.FechaEntrega;
                    dbExamen.Tipo = examen.Tipo;

                    try {
                        db.SaveChanges();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarExamenes]");
                        Debug.Write(ex);
                    }

                    // Agregar/actualizar temas
                    foreach (var tema in examen.Temas) {
                        var dbTema = dbExamen.ExamenTema.FirstOrDefault(a => a.IDTema == tema.IDTema);

                        if (dbTema != null && tema.FechaSync > dbTema.FechaSync) {
                            dbTema.Nombre = tema.Nombre;
                            dbTema.Pregunta = tema.Pregunta;
                            dbTema.TipoTema = tema.TipoTema;
                            dbTema.Reactivos = tema.Reactivos;
                            dbTema.Respuesta = tema.Respuesta;
                            dbTema.Respuesta1 = tema.Respuesta1;
                            dbTema.Respuesta2 = tema.Respuesta2;
                            dbTema.Instrucciones = tema.Instrucciones;
                        }
                        else if (dbTema == null) {
                            dbTema = new ExamenTema {
                                IDTema = tema.IDTema,
                                Archivo = tema.Archivo,
                                FechaSync = DateTime.Now,
                                Instrucciones = tema.Instrucciones,
                                Nombre = tema.Nombre,
                                Pregunta = tema.Pregunta,
                                Reactivos = tema.Reactivos,
                                Respuesta = tema.Respuesta,
                                Respuesta1 = tema.Respuesta1,
                                Respuesta2 = tema.Respuesta2,
                                TipoTema = tema.TipoTema
                            };
                            dbExamen.ExamenTema.Add(dbTema);
                        }

                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }

                        tema.Alumnos = (tema.Alumnos ?? new List<ExamenTemaCalificacionViewModel>());

                        var entregas = tema.Alumnos.Select(e => e.IDAlumno).ToList();
                        var alumnos = db.Alumno
                            .Where(a =>  a.IDGrupo == examen.IDGrupo && !entregas.Contains(a.IDAlumno))
                            .Select(a => new ExamenTemaCalificacionViewModel {
                                IDAlumno = a.IDAlumno,
                                Calificacion = 0,
                            }).ToList();

                        tema.Alumnos.AddRange(alumnos);

                        foreach (var entrega in tema.Alumnos) {
                            var dbEntrega = dbTema.ExamenAlumno.FirstOrDefault(e => e.IDAlumno == entrega.IDAlumno);
                            if (dbEntrega != null && entrega.FechaSync > dbEntrega.FechaSync) {
                                dbEntrega.Calificacion = entrega.Calificacion;
                                dbEntrega.IDAlumno = entrega.IDAlumno;
                                dbEntrega.FechaActualizacion = DateTime.Now;
                                dbEntrega.FechaSync = DateTime.Now;
                            }
                            else if (dbEntrega == null) {
                                dbTema.ExamenAlumno.Add(new ExamenAlumno {
                                    Calificacion = entrega.Calificacion,
                                    IDAlumno = entrega.IDAlumno,
                                    FechaActualizacion = DateTime.Now,
                                    FechaSync = DateTime.Now
                                });
                            }
                        }

                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }
                    }

                    // Elilminar temas que no esten en la lista
                    foreach (var tema in dbExamen.ExamenTema.ToList()) {
                        if (!examen.Temas.Any(a => a.IDTema == tema.IDTema)) {
                            tema.ExamenAlumno.Clear();
                            db.ExamenTema.Remove(tema);
                            try {
                                db.SaveChanges();
                            }
                            catch (Exception ex) {
                                Debug.WriteLine("[SincronizarExamenes]");
                                Debug.Write(ex);
                            }
                        }
                    }

                    Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(examen.IDGrupo));
                }
                else if (dbExamen == null && !examen.Deleted) {
                    var idBimestre = db.Bimestres.Where(i => i.IDGrupo == examen.IDGrupo && i.Bimestre == examen.Bimestre).Select(i => i.IDBimestre).FirstOrDefault();
                    if (examen.Tipo == "Parcial") {
                        int tota = db.Examen.Count(a => a.IDBimestre == idBimestre && a.Tipo == "Parcial");
                        examen.Titulo = examen.Tipo + " " + (tota + 1);
                    }
                    else {
                        examen.Titulo = examen.Tipo + " " + db.Bimestres.Where(a => a.IDBimestre == idBimestre).Select(a => a.Bimestre).FirstOrDefault();
                    }

                    dbExamen = new Examen {
                        IDExamen = examen.IDExamen,
                        IDBimestre = idBimestre,
                        FechaActualizacion = DateTime.Now,
                        FechaRegistro = DateTime.Now,
                        FechaSync = DateTime.Now,
                        FechaEntrega = examen.FechaEntrega,
                        Titulo = Util.UppercaseFirst(examen.Titulo),
                        Tipo = examen.Tipo,
                    };

                    try {
                        db.Examen.Add(dbExamen);
                        db.SaveChanges();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarExamenes]");
                        Debug.Write(ex);
                        continue;
                    }

                    foreach (var tema in examen.Temas) {
                        var dbTema = new ExamenTema {
                            IDTema = tema.IDTema,
                            Archivo = tema.Archivo,
                            FechaSync = DateTime.Now,
                            Instrucciones = tema.Instrucciones,
                            Nombre = tema.Nombre,
                            Pregunta = tema.Pregunta,
                            Reactivos = tema.Reactivos,
                            Respuesta = tema.Respuesta,
                            Respuesta1 = tema.Respuesta1,
                            Respuesta2 = tema.Respuesta2,
                            TipoTema = tema.TipoTema
                        };

                        try {
                            dbExamen.ExamenTema.Add(dbTema);
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }

                        tema.Alumnos = (tema.Alumnos ?? new List<ExamenTemaCalificacionViewModel>());

                        var entregas = tema.Alumnos.Select(e => e.IDAlumno).ToList();
                        var alumnos = db.Alumno
                            .Where(a =>  a.IDGrupo == examen.IDGrupo && !entregas.Contains(a.IDAlumno))
                            .Select(a => new ExamenTemaCalificacionViewModel {
                                IDAlumno = a.IDAlumno,
                                Calificacion = 0,
                            }).ToList();

                        tema.Alumnos.AddRange(alumnos);

                        foreach (var alumno in tema.Alumnos) {
                            dbTema.ExamenAlumno.Add(new ExamenAlumno {
                                Calificacion = alumno.Calificacion,
                                IDAlumno = alumno.IDAlumno,
                                FechaActualizacion = DateTime.Now,
                                FechaSync = DateTime.Now
                            });
                        }

                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }
                    }

                    Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(examen.IDGrupo));
                }
            }

            return Json(new ResultViewModel(true, "", RetrieveExamenes(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarInstrumentos(ICollection<InstrumentoViewModel> instrumentos) {
            foreach (var instrumento in (instrumentos ?? new List<InstrumentoViewModel>())) {
                var dbInstrumento = db.Portafolio.FirstOrDefault(o => o.IDPortafolio == instrumento.IDPortafolio);

                if (dbInstrumento != null) {
                    if(instrumento.FechaSync > dbInstrumento.FechaSync) {
                        if (instrumento.Deleted) {
                            dbInstrumento.PortafolioAlumno.Clear();
                            db.Portafolio.Remove(dbInstrumento);

                            try {
                                db.SaveChanges();
                                Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(instrumento.IDGrupo));
                                continue;
                            }
                            catch (Exception ex) {
                                Debug.WriteLine("[SincronizarInstrumentos]");
                                Debug.Write(ex);
                                continue;
                            }
                        }

                        dbInstrumento.Nombre = Util.UppercaseFirst(instrumento.Nombre);
                        dbInstrumento.Descripcion = instrumento.Descripcion;
                        dbInstrumento.FechaEntrega = instrumento.FechaEntrega;

                        dbInstrumento.FechaActualizacion = DateTime.Now;
                        dbInstrumento.FechaSync = DateTime.Now;
                        dbInstrumento.Activo1 = instrumento.Activo1;
                        dbInstrumento.Aspecto1 = instrumento.Aspecto1;
                        dbInstrumento.Criterio1 = instrumento.Criterio1;
                        dbInstrumento.Activo2 = instrumento.Activo2;
                        dbInstrumento.Aspecto2 = instrumento.Aspecto2;
                        dbInstrumento.Criterio2 = instrumento.Criterio2;
                        dbInstrumento.Activo3 = instrumento.Activo3;
                        dbInstrumento.Aspecto3 = instrumento.Aspecto3;
                        dbInstrumento.Criterio3 = instrumento.Criterio3;
                        dbInstrumento.Activo4 = instrumento.Activo4;
                        dbInstrumento.Aspecto4 = instrumento.Aspecto4;
                        dbInstrumento.Criterio4 = instrumento.Criterio4;
                        dbInstrumento.Activo5 = instrumento.Activo5;
                        dbInstrumento.Aspecto5 = instrumento.Aspecto5;
                        dbInstrumento.Criterio5 = instrumento.Criterio5;
                        dbInstrumento.IDTipoPortafolio = instrumento.IDTipoPortafolio;

                        try {
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(instrumento.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarInstrumentos]");
                            Debug.Write(ex);
                            continue;
                        }
                    }

                    instrumento.entrega = (instrumento.entrega ?? new List<InstrumentoEntregaViewModel>());

                    // si por alguna razon no estan esos alumnos en entregas
                    var alumnosEntrega = instrumento.entrega.Select(a => a.id);
                    var toAdd = db.Alumno
                        .Where(a => a.IDGrupo == instrumento.IDGrupo && !alumnosEntrega.Contains(a.IDAlumno))
                        .Select(a => new InstrumentoEntregaViewModel {
                            id = a.IDAlumno,
                            estado = 1,
                            Aspecto1 = "0",
                            Aspecto2 = "0",
                            Aspecto3 = "0",
                            Aspecto4 = "0",
                            Aspecto5 = "0",
                        })
                        .ToList();

                    instrumento.entrega.AddRange(toAdd);

                    foreach (var entrega in instrumento.entrega) {
                        var dbEntrega = dbInstrumento.PortafolioAlumno.FirstOrDefault(p => p.IDAlumno == entrega.id);
                        if (dbEntrega == null) {
                            dbInstrumento.PortafolioAlumno.Add(new PortafolioAlumno {
                                IDAlumno = entrega.id,
                                Estado = entrega.estado,
                                Aspecto1 = entrega.Aspecto1,
                                Aspecto2 = entrega.Aspecto2,
                                Aspecto3 = entrega.Aspecto3,
                                Aspecto4 = entrega.Aspecto4,
                                Aspecto5 = entrega.Aspecto5,
                                FechaActualizacion = DateTime.Now,
                                FechaSync = DateTime.Now
                            });
                        }
                        else {
                            dbEntrega.Estado = entrega.estado;
                            dbEntrega.Aspecto1 = entrega.Aspecto1;
                            dbEntrega.Aspecto2 = entrega.Aspecto2;
                            dbEntrega.Aspecto3 = entrega.Aspecto3;
                            dbEntrega.Aspecto4 = entrega.Aspecto4;
                            dbEntrega.Aspecto5 = entrega.Aspecto5;
                            dbEntrega.FechaActualizacion = DateTime.Now;
                            dbEntrega.FechaSync = DateTime.Now;
                        }

                        try {
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(instrumento.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarInstrumentos]");
                            Debug.Write(ex);
                        }
                    }
                }
                else if (dbInstrumento == null && !instrumento.Deleted) {
                    dbInstrumento = new Portafolio {
                        IDPortafolio = instrumento.IDPortafolio,
                        IDGrupo = instrumento.IDGrupo,
                        IDBimestre = db.Bimestres.Where(b => b.IDGrupo == instrumento.IDGrupo && b.Bimestre == instrumento.Bimestre).Select(b => b.IDBimestre).FirstOrDefault(),
                        Nombre = Util.UppercaseFirst(instrumento.Nombre),
                        Descripcion = instrumento.Descripcion,
                        FechaEntrega = instrumento.FechaEntrega,
                        FechaActualizacion = DateTime.Now,
                        FechaSync = DateTime.Now,
                        Activo1 = instrumento.Activo1,
                        Aspecto1 = instrumento.Aspecto1,
                        Criterio1 = instrumento.Criterio1,
                        Activo2 = instrumento.Activo2,
                        Aspecto2 = instrumento.Aspecto2,
                        Criterio2 = instrumento.Criterio2,
                        Activo3 = instrumento.Activo3,
                        Aspecto3 = instrumento.Aspecto3,
                        Criterio3 = instrumento.Criterio3,
                        Activo4 = instrumento.Activo4,
                        Aspecto4 = instrumento.Aspecto4,
                        Criterio4 = instrumento.Criterio4,
                        Activo5 = instrumento.Activo5,
                        Aspecto5 = instrumento.Aspecto5,
                        Criterio5 = instrumento.Criterio5,
                        IDTipoPortafolio = instrumento.IDTipoPortafolio
                    };

                    try {
                        db.Portafolio.Add(dbInstrumento);
                        db.SaveChanges();
                        Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(instrumento.IDGrupo));
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarInstrumentos]");
                        Debug.Write(ex);
                        continue;
                    }

                    instrumento.entrega = (instrumento.entrega ?? new List<InstrumentoEntregaViewModel>());

                    // si por alguna razon no estan esos alumnos en entregas
                    var alumnosEntrega = instrumento.entrega.Select(a => a.id);
                    var toAdd = db.Alumno
                        .Where(a => a.IDGrupo == instrumento.IDGrupo && !alumnosEntrega.Contains(a.IDAlumno))
                        .Select(a => new InstrumentoEntregaViewModel {
                            id = a.IDAlumno,
                            estado = 1,
                            Aspecto1 = "0",
                            Aspecto2 = "0",
                            Aspecto3 = "0",
                            Aspecto4 = "0",
                            Aspecto5 = "0",
                        })
                        .ToList();

                    instrumento.entrega.AddRange(toAdd);

                    foreach (var entrega in instrumento.entrega) {
                        dbInstrumento.PortafolioAlumno.Add(new PortafolioAlumno {
                            IDPortafolioAlumno = Guid.NewGuid(),
                            IDAlumno = entrega.id,
                            Estado = entrega.estado,
                            Aspecto1 = entrega.Aspecto1,
                            Aspecto2 = entrega.Aspecto2,
                            Aspecto3 = entrega.Aspecto3,
                            Aspecto4 = entrega.Aspecto4,
                            Aspecto5 = entrega.Aspecto5,
                            FechaActualizacion = DateTime.Now,
                            FechaSync = DateTime.Now
                        });

                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarInstrumentos]");
                            Debug.Write(ex);
                        }
                    }

                    try {
                        Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(instrumento.IDGrupo));
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarInstrumentos]");
                        Debug.Write(ex);
                    }
                }
            }

            return Json(new ResultViewModel(true, "", RetrieveInstrumentos(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarHabilidades(ICollection<HabilidadesViewModel> habilidades) {
            foreach (var hab in (habilidades ?? new List<HabilidadesViewModel>())) {
                var dbHabilidad = db.HabilidadesAlumno.FirstOrDefault(h => h.IDHabilidadAlumno == hab.id);
                if (dbHabilidad != null && hab.FechaSync > dbHabilidad.FechaSync) {
                    dbHabilidad.ApoyoEscritura = hab.ApoyoEscritura;
                    dbHabilidad.ApoyoLectura = hab.ApoyoLectura;
                    dbHabilidad.ApoyoMatematicas = hab.ApoyoMatematicas;
                    dbHabilidad.Argumentacion = hab.Argumentacion;
                    dbHabilidad.Autoevaluacion = hab.Autoevaluacin;
                    dbHabilidad.Coevaluacion = hab.Coevaluacion;
                    dbHabilidad.Comprension = hab.Comprension;
                    dbHabilidad.Conocimiento = hab.Conocimiento;
                    dbHabilidad.FechaSync = DateTime.Now;
                    dbHabilidad.SeInvolucraClase = hab.SeInvolucraClase;
                    dbHabilidad.Sintesis = hab.Sintesis;

                    try {
                        db.SaveChanges();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarHabilidades]");
                        Debug.WriteLine(ex);
                        Debug.WriteLine(ex.InnerException);
                    }

                }
                else if (dbHabilidad == null) {
                    var habilidad = new HabilidadesAlumno {
                        ApoyoEscritura = hab.ApoyoEscritura,
                        ApoyoLectura = hab.ApoyoLectura,
                        ApoyoMatematicas = hab.ApoyoMatematicas,
                        Argumentacion = hab.Argumentacion,
                        Autoevaluacion = hab.Autoevaluacin,
                        Coevaluacion = hab.Coevaluacion,
                        Comprension = hab.Comprension,
                        Conocimiento = hab.Conocimiento,
                        FechaSync = DateTime.Now,
                        IDAlumno = hab.IDAlumno,
                        IDGrupo = hab.IDGrupo,
                        IDHabilidadAlumno = hab.id,
                        IDBimestre = db.Bimestres.Where(b => b.IDGrupo == hab.IDGrupo && b.Bimestre == hab.Bimestre).Select(b => b.IDBimestre).FirstOrDefault(),
                        SeInvolucraClase = hab.SeInvolucraClase,
                        Sintesis = hab.Sintesis
                    };

                    try {
                        habilidad.crear();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarHabilidades]");
                        Debug.WriteLine(ex);
                        Debug.WriteLine(ex.InnerException);
                    }
                }
            }

            return Json(new ResultViewModel(true, "", RetrieveHabilidades(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarDiagnosticoCiclo(ICollection<DiagnosticoCicloViewModel> diagnosticoCiclo) {
            foreach (var diag in (diagnosticoCiclo ?? new List<DiagnosticoCicloViewModel>())) {
                var dbDiag = db.DiagnosticoCiclo.FirstOrDefault(o => o.IDDiagnosticoCiclo == diag.IDDiagnosticoCiclo);
                if (dbDiag != null && diag.FechaSync > dbDiag.FechaSync) {
                    if (diag.Deleted) {
                        try {
                            db.DiagnosticoCiclo.Remove(dbDiag);
                            db.SaveChanges();
                            Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(diag.IDGrupo));
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }

                        continue;
                    }

                    dbDiag.FechaActualizacion = DateTime.Now;
                    dbDiag.FechaSync = DateTime.Now;
                    dbDiag.FechaEntrega = diag.FechaEntrega;

                    try {
                        db.SaveChanges();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarExamenes]");
                        Debug.Write(ex);
                    }

                    // Agregar/actualizar temas
                    foreach (var tema in diag.Temas) {
                        var dbTema = dbDiag.DiagnosticoCicloTema.FirstOrDefault(a => a.IDTema == tema.IDTema);

                        if (dbTema != null && tema.FechaSync > dbTema.FechaSync) {
                            dbTema.Nombre = tema.Nombre;
                            dbTema.Pregunta = tema.Pregunta;
                            dbTema.TipoTema = tema.TipoTema;
                            dbTema.Reactivos = tema.Reactivos;
                            dbTema.Respuesta = tema.Respuesta;
                            dbTema.Respuesta1 = tema.Respuesta1;
                            dbTema.Respuesta2 = tema.Respuesta2;
                            dbTema.Instrucciones = tema.Instrucciones;
                        }
                        else if (dbTema == null) {
                            dbTema = new DiagnosticoCicloTema {
                                IDTema = tema.IDTema,
                                Archivo = tema.Archivo,
                                FechaSync = DateTime.Now,
                                Instrucciones = tema.Instrucciones,
                                Nombre = tema.Nombre,
                                Pregunta = tema.Pregunta,
                                Reactivos = tema.Reactivos,
                                Respuesta = tema.Respuesta,
                                Respuesta1 = tema.Respuesta1,
                                Respuesta2 = tema.Respuesta2,
                                TipoTema = tema.TipoTema
                            };
                            dbDiag.DiagnosticoCicloTema.Add(dbTema);
                        }

                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }

                        tema.Alumnos = (tema.Alumnos ?? new List<DiagnosticoCicloCalificacionViewModel>());

                        var entregas = tema.Alumnos.Select(e => e.IDAlumno).ToList();
                        var alumnos = db.Alumno
                            .Where(a =>  a.IDGrupo == diag.IDGrupo && !entregas.Contains(a.IDAlumno))
                            .Select(a => new DiagnosticoCicloCalificacionViewModel {
                                IDAlumno = a.IDAlumno,
                                Calificacion = 0,
                            }).ToList();

                        tema.Alumnos.AddRange(alumnos);

                        foreach (var entrega in tema.Alumnos) {
                            var dbEntrega = dbTema.DiagnosticoCicloAlumno.FirstOrDefault(e => e.IDAlumno == entrega.IDAlumno);
                            if (dbEntrega != null && entrega.FechaSync > dbEntrega.FechaSync) {
                                dbEntrega.Calificacion = entrega.Calificacion;
                                dbEntrega.IDAlumno = entrega.IDAlumno;
                                dbEntrega.FechaActualizacion = DateTime.Now;
                                dbEntrega.FechaSync = DateTime.Now;
                            }
                            else if (dbEntrega == null) {
                                dbTema.DiagnosticoCicloAlumno.Add(new DiagnosticoCicloAlumno {
                                    Calificacion = entrega.Calificacion,
                                    IDAlumno = entrega.IDAlumno,
                                    FechaActualizacion = DateTime.Now,
                                    FechaSync = DateTime.Now
                                });
                            }
                        }

                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }
                    }

                    // Elilminar temas que no esten en la lista
                    foreach (var tema in dbDiag.DiagnosticoCicloTema.ToList()) {
                        if (!diag.Temas.Any(a => a.IDTema == tema.IDTema)) {
                            tema.DiagnosticoCicloAlumno.Clear();
                            db.DiagnosticoCicloTema.Remove(tema);

                            try {
                                db.SaveChanges();
                            }
                            catch (Exception ex) {
                                Debug.WriteLine("[SincronizarExamenes]");
                                Debug.Write(ex);
                            }
                        }
                    }

                    try {
                        Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(diag.IDGrupo));
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarExamenes]");
                        Debug.Write(ex);
                    }
                }
                else if (dbDiag == null && !diag.Deleted) {
                    var idBimestre = db.Bimestres.Where(i => i.IDGrupo == diag.IDGrupo && i.Bimestre == diag.Bimestre).Select(i => i.IDBimestre).FirstOrDefault();

                    dbDiag = new DiagnosticoCiclo {
                        IDDiagnosticoCiclo = diag.IDDiagnosticoCiclo,
                        IDBimestre = idBimestre,
                        FechaActualizacion = DateTime.Now,
                        FechaRegistro = DateTime.Now,
                        FechaSync = DateTime.Now,
                        FechaEntrega = diag.FechaEntrega,
                        Titulo = Util.UppercaseFirst(diag.Titulo),
                    };

                    try {
                        db.DiagnosticoCiclo.Add(dbDiag);
                        db.SaveChanges();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarExamenes]");
                        Debug.Write(ex);
                        continue;
                    }

                    foreach (var tema in diag.Temas) {
                        var dbTema = new DiagnosticoCicloTema {
                            IDTema = tema.IDTema,
                            Archivo = tema.Archivo,
                            FechaSync = DateTime.Now,
                            Instrucciones = tema.Instrucciones,
                            Nombre = tema.Nombre,
                            Pregunta = tema.Pregunta,
                            Reactivos = tema.Reactivos,
                            Respuesta = tema.Respuesta,
                            Respuesta1 = tema.Respuesta1,
                            Respuesta2 = tema.Respuesta2,
                            TipoTema = tema.TipoTema
                        };

                        try {
                            dbDiag.DiagnosticoCicloTema.Add(dbTema);
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }

                        tema.Alumnos = (tema.Alumnos ?? new List<DiagnosticoCicloCalificacionViewModel>());

                        var entregas = tema.Alumnos.Select(e => e.IDAlumno).ToList();
                        var alumnos = db.Alumno
                            .Where(a =>  a.IDGrupo == diag.IDGrupo && !entregas.Contains(a.IDAlumno))
                            .Select(a => new DiagnosticoCicloCalificacionViewModel {
                                IDAlumno = a.IDAlumno,
                                Calificacion = 0,
                            }).ToList();

                        tema.Alumnos.AddRange(alumnos);

                        foreach (var alumno in tema.Alumnos) {
                            dbTema.DiagnosticoCicloAlumno.Add(new DiagnosticoCicloAlumno {
                                Calificacion = alumno.Calificacion,
                                IDAlumno = alumno.IDAlumno,
                                FechaActualizacion = DateTime.Now,
                                FechaSync = DateTime.Now
                            });
                        }

                        try {
                            db.SaveChanges();
                        }
                        catch (Exception ex) {
                            Debug.WriteLine("[SincronizarExamenes]");
                            Debug.Write(ex);
                        }
                    }

                    try {
                        Task.Factory.StartNew(() => AlumnoDesempenio.actualizacionGeneral(diag.IDGrupo));
                    }
                    catch (Exception ex) {
                        Debug.WriteLine("[SincronizarExamenes]");
                        Debug.Write(ex);
                    }
                }
            }

            return Json(new ResultViewModel(true, "", RetrieveDiagnosticoCiclo(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarReporte() {
            return Json(new ResultViewModel(true, "", RetrieveReporte(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarTipoPortafolio() {
            return Json(new ResultViewModel(true, "", RetrieveTipoPortafolios(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult SincronizarSemaforo() {
            return Json(new ResultViewModel(true, "", RetrieveSemaforos(User.Identity.GetUserId())));
        }

        [HttpPost]
        public JsonResult UserDatabase() {
            return Json(new ResultViewModel(true, "", RetrieveUserDatabase()));
        }

        #region Metodos
        private dynamic RetrieveTipoPortafolios(string userid) {
            var result =
                from port in db.TipoPortafolio
                join def in db.PortafolioDefecto.Where(o => o.IDUsuario == userid) on port.Nombre equals def.TipoTrabajo into defDefault
                from def in defDefault.DefaultIfEmpty()
                select new { port, def };

            return result.AsEnumerable().Select(o => {
                dynamic port = o.def == null ? o.port : (object) o.def;

                if(port is PortafolioDefecto) {
                    port.Nombre = o.port.Nombre;
                }

                return new {
                    Nombre = port.Nombre,
                    IDTipoPortafolio = port.IDTipoPortafolio,
                    Aspecto1 = port.Aspecto1,
                    Aspecto2 = port.Aspecto2,
                    Aspecto3 = port.Aspecto3,
                    Aspecto4 = port.Aspecto4,
                    Aspecto5 = port.Aspecto5,

                    Criterio1 = port.Criterio1,
                    Criterio2 = port.Criterio2,
                    Criterio3 = port.Criterio3,
                    Criterio4 = port.Criterio4,
                    Criterio5 = port.Criterio5,

                    Activo1 = port.Activo1,
                    Activo2 = port.Activo2,
                    Activo3 = port.Activo3,
                    Activo4 = port.Activo4,
                    Activo5 = port.Activo5
                };
            }).ToList();
        }
        private dynamic RetrieveInstrumentos(string userid) {
            return db.Portafolio
                .Where(i => i.Grupos.IDUsuario == userid)
                .OrderByDescending(i => i.FechaEntrega)
                .Select(i => new {
                    IDGrupo = i.IDGrupo,
                    Bimestre = i.Bimestres.Bimestre,
                    IDPortafolio = i.IDPortafolio,
                    FechaEntrega = i.FechaEntrega,
                    observacion = i.Descripcion,
                    Descripcion = i.Descripcion,
                    TipoTrabajo = i.TipoPortafolio != null ? i.TipoPortafolio.Nombre : "",
                    IDTipoPortafolio = i.IDTipoPortafolio,
                    Nombre = i.Nombre,
                    Aspecto1 = i.Aspecto1,
                    Aspecto2 = i.Aspecto2,
                    Aspecto3 = i.Aspecto3,
                    Aspecto4 = i.Aspecto4,
                    Aspecto5 = i.Aspecto5,

                    Criterio1 = i.Criterio1,
                    Criterio2 = i.Criterio2,
                    Criterio3 = i.Criterio3,
                    Criterio4 = i.Criterio4,
                    Criterio5 = i.Criterio5,

                    Activo1 = i.Activo1,
                    Activo2 = i.Activo2,
                    Activo3 = i.Activo3,
                    Activo4 = i.Activo4,
                    Activo5 = i.Activo5,

                    Observacion1 = i.Observacion1,
                    Observacion2 = i.Observacion2,
                    Observacion3 = i.Observacion3,
                    Observacion4 = i.Observacion4,
                    Observacion5 = i.Observacion5,

                    entrega = i.PortafolioAlumno.Select(p => new {
                        id = p.IDAlumno,
                        estado = p.Estado,
                        Aspecto1 = p.Aspecto1 == null ? "0" : p.Aspecto1,
                        Aspecto2 = p.Aspecto2 == null ? "0" : p.Aspecto2,
                        Aspecto3 = p.Aspecto3 == null ? "0" : p.Aspecto3,
                        Aspecto4 = p.Aspecto4 == null ? "0" : p.Aspecto4,
                        Aspecto5 = p.Aspecto5 == null ? "0" : p.Aspecto5,
                        Semaforo = p.Alumno.AlumnoDesempenio
                            .Where(d => d.IDGrupo == i.IDGrupo && d.Bimestre == i.Bimestres.Bimestre)
                            .Select(d => d.ColorDiagnostico)
                            .FirstOrDefault()
                    })
                }).AsEnumerable()
                .Select(i => new {
                    Fecha = Util.toHoraMexico(i.FechaEntrega.Value).ToString("dd-MM-yyyy"),
                    FechaEntrega = Util.toHoraMexico(i.FechaEntrega.Value).ToString("dd-MM-yyyy"),
                    i.IDGrupo,i.Bimestre,i.IDPortafolio,i.Descripcion,i.TipoTrabajo,
                    i.IDTipoPortafolio,i.Nombre,i.Aspecto1,i.Aspecto2,i.Aspecto3,i.Aspecto4,
                    i.Aspecto5,i.Criterio1,i.Criterio2,i.Criterio3,i.Criterio4,i.Criterio5,
                    i.Activo1,i.Activo2,i.Activo3,i.Activo4,i.Activo5,i.Observacion1,
                    i.Observacion2,i.Observacion3,i.Observacion4,i.Observacion5,i.entrega,
                });
        }
        private dynamic RetrieveTrabajos(string userid) {
            return db.Trabajo
                    .Where(i => i.Grupos.IDUsuario == userid)
                    .OrderByDescending(i => i.Fecha )
                    .ThenBy(i => i.Nombre)
                    .Select(i => new {
                        i.IDGrupo,
                        i.Bimestres.Bimestre,
                        entrega = db.TrabajoAlumno
                            .Where(e => e.IDTrabajo == i.IDTrabajo)
                            .Select(e => new EntregaAlumno { id = e.IDAlumno, estado = e.Estado }).ToList(),
                        i.IDTrabajo, i.Fecha, i.Observaciones, i.Nombre, i.Tipo, i.Actividad })
                    .ToList()
                    .Select(i => new {
                        IDGrupo = i.IDGrupo,
                        Bimestre = i.Bimestre,
                        id = i.IDTrabajo,
                        observacion = i.Observaciones,
                        nombre = i.Nombre,
                        tipo = i.Tipo,
                        actividad = i.Actividad,
                        entrega = i.entrega,
                        fecha =i.Fecha.Value.ToString("dd-MM-yyyy"),
                        time = i.Fecha.Value.Ticks,
                    })
                    .ToList();
        }
        private dynamic RetrieveAlumnos(string userid) {
            var IDGrupos = db.Grupos.Where(i => i.IDUsuario == userid).Select(o => o.IDGrupo);
            var query = db.Alumno
                .Where(i => IDGrupos.Contains(i.IDGrupo))
                .OrderBy(i => i.ApellidoPaterno)
                .ThenBy(i => i.ApellidoMaterno)
                .ThenBy(i => i.Nombre);

            var alumnos = query
                .Select(i => new {
                    i.IDAlumno,
                    i.Nombre,
                    i.ApellidoMaterno,
                    i.ApellidoPaterno,
                    i.Curp,
                    i.EsUSAER,
                    i.Estado,
                    i.Grupo,
                    i.ColorPromedio,
                    //i.EsTaller,
                    i.IDGrupo,
                    i.FechaActualizacion,
                    i.PromedioBimestre1,
                    i.PromedioBimestre2,
                    i.PromedioBimestre3,
                    i.PromedioBimestre4,
                    i.PromedioBimestre5,
                    i.PromedioTotal,
                    Secciones = new {
                        Trabajos = new {
                            Any = i.TrabajoAlumno.Select(t => t.Trabajo).Where(t => t.IDGrupo == i.IDGrupo).Any(),
                            Semaforo = i.AlumnoDesempenio.Where(ad => ad.IDGrupo == i.IDGrupo).Select(ad => ad.PromedioTrabajo).FirstOrDefault()
                        },
                        Portafolios = new {
                            Any = i.PortafolioAlumno.Select(t => t.Portafolio).Where(t => t.IDGrupo == i.IDGrupo).Any(),
                            Semaforo = i.AlumnoDesempenio.Where(ad => ad.IDGrupo == i.IDGrupo).Select(ad => ad.PromedioPortafolio).FirstOrDefault()
                        },
                        Examenes = new {
                            Any = i.ExamenAlumno.Any(),
                            Semaforo = i.AlumnoDesempenio.Where(ad => ad.IDGrupo == i.IDGrupo).Select(ad => ad.PromedioExamen).FirstOrDefault()
                        },
                        Asistencias = new {
                            Any = i.AlumnoSesion.Select(t => t.Sesion).Where(t => t.IDGrupo == i.IDGrupo).Any(),
                            Semaforo = i.AlumnoDesempenio.Where(ad => ad.IDGrupo == i.IDGrupo).Select(ad => ad.PromedioAsistencia).FirstOrDefault()
                        },
                    }
                })
                .AsEnumerable()
                .Select(i => {
                    var secciones = new [] {
                        i.Secciones.Trabajos,
                        i.Secciones.Portafolios,
                        i.Secciones.Examenes,
                        i.Secciones.Asistencias
                    }.Where(s => s.Any).ToList();
                    var promedioParcial = secciones.Sum(s => s.Semaforo ?? 0) / (secciones.Count * 10.0);
                    var colorPromedio =
                        promedioParcial < 5 ? AlumnoDesempenioStatus.APOYO :
                        promedioParcial < 8.5 ? AlumnoDesempenioStatus.REGULAR :
                        promedioParcial >= 8.5 ? AlumnoDesempenioStatus.BIEN : "";

                    return new {
                        i.ApellidoPaterno,
                        i.ApellidoMaterno,
                        i.Nombre,
                        i.ColorPromedio,
                        i.Curp,
                        i.Estado,
                        //i.EsTaller,
                        i.EsUSAER,
                        FechaActualizacion = i.FechaActualizacion?.ToString("O"),
                        i.Grupo,
                        i.IDAlumno,
                        i.IDGrupo,
                        i.PromedioBimestre1,
                        i.PromedioBimestre2,
                        i.PromedioBimestre3,
                        i.PromedioBimestre4,
                        i.PromedioBimestre5,
                        i.PromedioTotal,
                        Semaforo = i.EsUSAER ? "NEE" :
                                    colorPromedio == AlumnoDesempenioStatus.BIEN ? "BIEN" :
                                    colorPromedio == AlumnoDesempenioStatus.REGULAR ? "REGULAR" :
                                    colorPromedio == AlumnoDesempenioStatus.APOYO ? "APOYO" : "",
                        ColorSemaforo = i.EsUSAER ? AlumnoDesempenioStatus.USAER : colorPromedio,
                    };
                })
                .ToList()
                .OrderBy(i => i.ApellidoPaterno)
                .ThenBy(i => i.ApellidoMaterno)
                .ThenBy(i => i.Nombre).ToList();

            return alumnos;
        }
        private dynamic RetrieveSesiones(string usuario) {
            return db.Sesion
                .Where(i => i.Grupos.IDUsuario == usuario)
                .OrderByDescending(i => i.Fecha)
                .Select(s => new {
                    IDGrupo = s.IDGrupo,
                    Bimestre = s.Bimestres.Bimestre ?? 1,
                    id = s.IDSesion,
                    fecha = s.Fecha,
                    observacion = s.Observacion,
                    asistencia = db.AlumnoSesion.Where(i => i.IDSesion == s.IDSesion)
                        .Select(i => new Sesion.AsistenciaAlumno {
                            id = i.IDAlumno,
                            estado = i.Estado,
                            semaforo = i.Alumno.AlumnoDesempenio
                                .Where(a => a.IDGrupo == i.Sesion.IDGrupo && a.Bimestre == i.Sesion.Bimestres.Bimestre)
                                .Select(a => a.ColorAsistencia).FirstOrDefault()
                        }).ToList()
                }).AsEnumerable()
                .Select(i => new {
                    IDGrupo = i.IDGrupo,
                    Bimestre = i.Bimestre,
                    fecha = Util.toHoraMexico(i.fecha).ToString("dd-MM-yyyy"),
                    id = i.id,
                    observacion = i.observacion,
                    asistencia = i.asistencia
                }).ToList();
        }
        private dynamic RetrieveGrupos(string userid) {
            return db.Grupos
                .Where(i => i.IDUsuario == userid)
                .Select(o => new {
                    o.Ciclo, o.Color, o.Descripcion,
                    o.Escuela, o.EsTaller, o.Grado,
                    o.Grupo, o.IDGrupo, o.IDUsuario,
                    o.Materia, o.RegistroFederalEscolar,
                    o.Status, o.Turno
                });
        }
        private dynamic RetrieveExamenes(string userid) {
            string url = ConfigurationManager.AppSettings["AWSUrl"] + "/" +userid + "/examenes/";
            return db.Examen
                .Where(i => i.Bimestres.Grupos.IDUsuario == userid)
                .OrderByDescending(i => i.FechaEntrega)
                .Select(i => new {
                    IDGrupo = i.Bimestres.IDGrupo,
                    Bimestre = i.Bimestres.Bimestre,
                    IDExamen = i.IDExamen.ToString(),
                    Titulo = i.Titulo,
                    Tipo = i.Tipo,
                    FechaEntrega = i.FechaEntrega,
                    Temas = i.ExamenTema.Select(t => new {
                        IDTema = t.IDTema.ToString(),
                        Nombre = t.Nombre,
                        TipoTema = t.TipoTema,
                        Reactivos = t.Reactivos,
                        Archivo = t.Archivo,
                        Pregunta = t.Pregunta,
                        Respuesta = t.Respuesta,
                        Respuesta1 = t.Respuesta1,
                        Respuesta2 = t.Respuesta2,
                        UrlArchivo = t.Archivo != null ? url + t.Archivo : null,
                        Instrucciones = t.Instrucciones,
                        Alumnos = t.ExamenAlumno.Select(a => new {
                            IDAlumno = a.IDAlumno.ToString(),
                            Calificacion = a.Calificacion == null ? 0 : a.Calificacion.Value
                        })
                    }).OrderBy(t => t.Nombre)
                })
                .AsEnumerable()
                .Select(i => new {
                    IDGrupo = i.IDGrupo, Bimestre = i.Bimestre, IDExamen = i.IDExamen,
                    Titulo = i.Titulo, Tipo = i.Tipo, Temas = i.Temas,
                    FechaEntrega = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                    FechaEntregaDesplegable = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                });
        }
        private dynamic RetrieveHabilidades(string userid) {
            return db.Alumno
                .Where(i => i.Grupos.IDUsuario == userid)
                .OrderBy(i => i.IDAlumno)
                .SelectMany(i => i.HabilidadesAlumno)
                .Select(hab => new {
                    id = hab != null ? hab.IDHabilidadAlumno : Guid.NewGuid(),
                    IDAlumno = hab.IDAlumno,
                    Alumno = hab.Alumno.Nombre + " " + hab.Alumno.ApellidoPaterno + " " + hab.Alumno.ApellidoMaterno,
                    IDGrupo = hab.Bimestres.IDGrupo,
                    Bimestre = hab.Bimestres.Bimestre,
                    Comprension = hab != null ? hab.Comprension : "",
                    Autoevaluacin = hab != null ? hab.Autoevaluacion : null,
                    Coevaluacion = hab != null ? hab.Coevaluacion : null,
                    Conocimiento = hab != null ? hab.Conocimiento : null,
                    Sintesis = hab != null ? hab.Sintesis : null,
                    Argumentacion = hab != null ? hab.Argumentacion : null,
                    ApoyoLectura = hab != null ? hab.ApoyoLectura : null,
                    ApoyoEscritura = hab != null ? hab.ApoyoEscritura : null,
                    ApoyoMatematicas = hab != null ? hab.ApoyoMatematicas : null,
                    SeInvolucraClase = hab != null ? hab.SeInvolucraClase : null,
                });
        }
        private dynamic RetrieveUserDatabase() {
            var userid = User.Identity.GetUserId();
            var sesiones = RetrieveSesiones(userid);
            var trabajos = RetrieveTrabajos(userid);
            var alumnos = RetrieveAlumnos(userid);
            var grupos = RetrieveGrupos(userid);
            var examenes = RetrieveExamenes(userid);
            var instrumentos = RetrieveInstrumentos(userid);
            var habilidades = RetrieveHabilidades(userid);
            var reporte = RetrieveReporte(userid);
            var diagnosticoCiclo = RetrieveDiagnosticoCiclo(userid);
            var tipoPortafolio = RetrieveTipoPortafolios(userid);
            var semaforo = RetrieveSemaforos(userid);

            var result = new {
                diagnosticoCiclo = diagnosticoCiclo,
                userid = User.Identity.GetUserId(),
                tipoPortafolio = tipoPortafolio,
                secciones = User.GetSecciones(),
                username = User.Identity.Name,
                instrumentos = instrumentos,
                habilidades = habilidades,
                sesiones = sesiones,
                trabajos = trabajos,
                examenes = examenes,
                semaforo = semaforo,
                reporte = reporte,
                alumnos = alumnos,
                grupos = grupos,
            };

            return result;
        }
        private dynamic RetrieveReporte(string userid) {
            var grupos = db.Grupos
                .Where(g => g.IDUsuario == userid && g.Status == 1)
                .SelectMany(g => g.Bimestres);
            var result = new List<dynamic>();

            foreach(var grupo in grupos) {
                List<AlumnoReporteViewModel> alumnos = AlumnoDesempenio.cargarReporte(grupo.IDGrupo.Value, grupo.Bimestre ?? 1, null);
                List<AlumnoReporteViewModel> alumnos2 = AlumnoDesempenio.cargarReporteGeneral(grupo.IDGrupo.Value, null);

                var promediosFinales = alumnos2.Select(a => new {
                    a.id,
                    a.promedioFinal,
                }).ToList();

                var promedioGrupal = new {
                    bimestre1 = alumnos2.Any() ? alumnos2.Select(a => a.promedioExamenParcial2Bimestre1).Average() : 0,
                    bimestre2 = alumnos2.Any() ? alumnos2.Select(a => a.promedioExamenParcial2Bimestre2).Average() : 0,
                    bimestre3 = alumnos2.Any() ? alumnos2.Select(a => a.promedioExamenParcial2Bimestre3).Average() : 0,
                    bimestre4 = alumnos2.Any() ? alumnos2.Select(a => a.promedioExamenParcial2Bimestre4).Average() : 0,
                    bimestre5 = alumnos2.Any() ? alumnos2.Select(a => a.promedioExamenParcial2Bimestre5).Average() : 0
                };

                result.Add(new {
                    IDGrupo = grupo.IDGrupo.Value,
                    Bimestre = grupo.Bimestre ?? 1,
                    headers = AlumnoDesempenio.cargarHeadersDeReporte(grupo.IDGrupo.Value, grupo.Bimestre ?? 1),
                    alumnos = alumnos,
                    resumen = AlumnoDesempenio.calcularResumen(alumnos2),
                    promediosFinales = promediosFinales,
                    promedioGrupal = promedioGrupal,
                    result = true
                });
            }

            return result;
        }
        private dynamic RetrieveDiagnosticoCiclo(string userid) {
            string url = ConfigurationManager.AppSettings["AWSUrl"] + "/" +userid + "/DiagnosticoCicloes/";

            return db.DiagnosticoCiclo
                .Where(i => i.Bimestres.Grupos.IDUsuario == userid)
                .OrderByDescending(i => i.FechaEntrega)
                .Select(i => new {
                    IDGrupo = i.Bimestres.IDGrupo,
                    Bimestre = i.Bimestres.Bimestre,
                    IDDiagnosticoCiclo = i.IDDiagnosticoCiclo,
                    FechaEntrega = i.FechaEntrega,
                    Titulo = i.Titulo,
                    Temas = i.DiagnosticoCicloTema.Select(t => new {
                        IDTema = t.IDTema,
                        Nombre = t.Nombre,
                        TipoTema = t.TipoTema,
                        Reactivos = t.Reactivos,
                        Archivo = t.Archivo,
                        Pregunta = t.Pregunta,
                        Respuesta = t.Respuesta,
                        Respuesta1 = t.Respuesta1,
                        Respuesta2 = t.Respuesta2,
                        UrlArchivo = t.Archivo != null ? url + t.Archivo : null,
                        Instrucciones = t.Instrucciones,
                        Alumnos = t.DiagnosticoCicloAlumno.Select(a => new {
                            IDAlumno = a.IDAlumno,
                            Calificacion = a.Calificacion == null ? 0 : a.Calificacion.Value,
                        })
                    }).OrderBy(t => t.Nombre)
                })
                .AsEnumerable()
                .Select(i => new {
                    i.IDGrupo, i.Bimestre, i.IDDiagnosticoCiclo, i.Titulo, i.Temas,
                    FechaEntrega = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                    FechaEntregaDesplegable = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                });
        }
        private dynamic RetrieveSemaforos(string userid) {
            return db.AlumnoDesempenio
                .Where(a => a.Grupos.IDUsuario == userid)
                .Select(a => new {
                    id = a.IDAlumno,
                    IDGrupo = a.IDGrupo,
                    Bimestre = a.Bimestre,
                    asistencia = a.ColorAsistencia,
                    trabajos = a.ColorTrabajo,
                    portafolio = a.ColorPortafolio,
                    examenes = a.ColorExamen,
                    diagnosticoCiclo = a.ColorDiagnostico,
                    colorPromedio = a.Alumno.ColorPromedio,
                });
        }

        public class AsistenciaViewModel
        {
            public List<AsistenciaAlumnoViewModel> asistencia { get; set; }
            public int Bimestre { get; set; }
            public DateTime fecha { get; set; }
            public Guid id { get; set; }
            public Guid IDGrupo { get; set; }
            public string observacion { get; set; }
            public DateTime? FechaSync { get; set; }
            public bool Deleted { get; set; }
        }
        public class AsistenciaAlumnoViewModel : Sesion.AsistenciaAlumno
        {
            public DateTime FechaSync { get; set; }
        }

        public class TrabajoViewModel
        {
            public string actividad { get; set; }
            public int Bimestre { get; set; }
            public List<EntregaAlumnoViewModel> entrega { get; set; }
            public DateTime fecha { get; set; }
            public Guid id { get; set; }
            public Guid IDGrupo { get; set; }
            public string nombre { get; set; }
            public string observacion { get; set; }
            public long time { get; set; }
            public string tipo { get; set; }
            public DateTime? FechaSync { get; set; }
            public bool Deleted { get; set; }
        }
        public class EntregaAlumnoViewModel : EntregaAlumno
        {
            public DateTime? FechaSync { get; set; }
            public new Guid id { get; set; }
        }

        public class ExamenTemaCalificacionViewModel
        {
            public double Calificacion { get; set; }
            public Guid IDAlumno { get; set; }
            public DateTime? FechaSync { get; set; }
        }
        public class ExamenTemaViewModel
        {
            public List<ExamenTemaCalificacionViewModel> Alumnos { get; set; }
            public string Archivo { get; set; }
            public Guid IDTema { get; set; }
            public string Instrucciones { get; set; }
            public string Nombre { get; set; }
            public string Pregunta { get; set; }
            public int Reactivos { get; set; }
            public string Respuesta { get; set; }
            public string Respuesta1 { get; set; }
            public string Respuesta2 { get; set; }
            public string TipoTema { get; set; }
            public string UrlArchivo { get; set; }
            public DateTime? FechaSync { get; set; }
        }
        public class ExamenApiViewModel
        {
            public int? Bimestre { get; set; }
            public DateTime FechaEntrega { get; set; }
            public string FechaEntregaDesplegable { get; set; }
            public Guid IDExamen { get; set; }
            public Guid IDGrupo { get; set; }
            public ICollection<ExamenTemaViewModel> Temas { get; set; }
            public string Tipo { get; set; }
            public string Titulo { get; set; }
            public bool Deleted { get; set; }
            public DateTime? FechaSync { get; set; }
        }

        public class InstrumentoEntregaViewModel
        {
            public string Aspecto1 { get; set; }
            public string Aspecto2 { get; set; }
            public string Aspecto3 { get; set; }
            public string Aspecto4 { get; set; }
            public string Aspecto5 { get; set; }
            public int? estado { get; set; }
            public Guid? id { get; set; }
            public string Semaforo { get; set; }
            public DateTime? FechaSync { get; set; }
        }
        public class InstrumentoViewModel
        {
            public bool Activo1 { get; set; }
            public bool Activo2 { get; set; }
            public bool Activo3 { get; set; }
            public bool Activo4 { get; set; }
            public bool Activo5 { get; set; }
            public string Aspecto1 { get; set; }
            public string Aspecto2 { get; set; }
            public string Aspecto3 { get; set; }
            public string Aspecto4 { get; set; }
            public string Aspecto5 { get; set; }
            public int? Bimestre { get; set; }
            public string Criterio1 { get; set; }
            public string Criterio2 { get; set; }
            public string Criterio3 { get; set; }
            public string Criterio4 { get; set; }
            public string Criterio5 { get; set; }
            public string Descripcion { get; set; }
            public List<InstrumentoEntregaViewModel> entrega { get; set; }
            public string Fecha { get; set; }
            public DateTime FechaEntrega { get; set; }
            public Guid? IDGrupo { get; set; }
            public Guid IDPortafolio { get; set; }
            public Guid? IDTipoPortafolio { get; set; }
            public string Nombre { get; set; }
            public string Observacion1 { get; set; }
            public string Observacion2 { get; set; }
            public string Observacion3 { get; set; }
            public string Observacion4 { get; set; }
            public string Observacion5 { get; set; }
            public string TipoTrabajo { get; set; }
            public DateTime? FechaSync { get; set; }
            public bool Deleted { get; set; }
        }

        public class HabilidadesViewModel
        {
            public string Alumno { get; set; }
            public int? ApoyoEscritura { get; set; }
            public int? ApoyoLectura { get; set; }
            public int? ApoyoMatematicas { get; set; }
            public string Argumentacion { get; set; }
            public int? Autoevaluacin { get; set; }
            public int? Bimestre { get; set; }
            public int? Coevaluacion { get; set; }
            public string Comprension { get; set; }
            public string Conocimiento { get; set; }
            public Guid id { get; set; }
            public Guid? IDAlumno { get; set; }
            public Guid? IDGrupo { get; set; }
            public bool? SeInvolucraClase { get; set; }
            public string Sintesis { get; set; }
            public DateTime FechaSync { get; set; }

        }

        public class DiagnosticoCicloCalificacionViewModel
        {
            public double Calificacion { get; set; }
            public Guid IDAlumno { get; set; }
            public DateTime? FechaSync { get; set; }
        }
        public class DiagnosticoCicloTemaViewModel
        {
            public List<DiagnosticoCicloCalificacionViewModel> Alumnos { get; set; }
            public string Archivo { get; set; }
            public Guid IDTema { get; set; }
            public string Instrucciones { get; set; }
            public string Nombre { get; set; }
            public string Pregunta { get; set; }
            public int Reactivos { get; set; }
            public string Respuesta { get; set; }
            public string Respuesta1 { get; set; }
            public string Respuesta2 { get; set; }
            public string TipoTema { get; set; }
            public string UrlArchivo { get; set; }
            public DateTime? FechaSync { get; set; }
        }
        public class DiagnosticoCicloViewModel
        {
            public int? Bimestre { get; set; }
            public DateTime FechaEntrega { get; set; }
            public string FechaEntregaDesplegable { get; set; }
            public Guid IDDiagnosticoCiclo { get; set; }
            public Guid? IDGrupo { get; set; }
            public ICollection<DiagnosticoCicloTemaViewModel> Temas { get; set; }
            public string Titulo { get; set; }
            public DateTime? FechaSync { get; set; }
            public bool Deleted { get; set; }
        }

        public class AlumnoViewModel : Alumno
        {
            public bool Deleted { get; set; }
        }

        #endregion
    }
}