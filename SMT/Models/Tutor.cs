using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SMT.Models.DB;
using SMT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SMT.Models
{
    public class Tutor
    {
        public const string Rol = "Tutor";
        public const int CantidadMaximaTutores = 3;

        public static bool TieneTutorAAlumno(AgregarTutorAlumnoViewModel model, SMTDevEntities db) {
            var tutorid = GetTutorIdByEmail(model.Email);
            return db.TutorAlumno.Any(t => t.IdAlumno == model.Id && t.IdTutor == tutorid);
        }

        public static IEnumerable<TutorIndexAlumnoViewModel> ObtenerIndexViewModel(string tutorid, SMTDevEntities db) {
            var alumnos = db.TutorAlumno.Where(o => o.IdTutor == tutorid).Select(o => o.Alumno);
            var grouping = alumnos.GroupBy(o => o.Curp.Trim().ToUpper());
            var curps = grouping.Select(o => o.FirstOrDefault()).Where(o => o != null);
            return curps.Select(o => new TutorIndexAlumnoViewModel {
                CURP = o.Curp.ToUpper(),
                NombreCompleto = o.NombreCompleto.ToUpper(),
                IdAlumno = o.IDAlumno,
                Grupos = grouping
                    .Select(g => new { g.Key, Grupos = g.Select(i => i.Grupos) })
                    .Where(g => g.Key == o.Curp.Trim().ToUpper())
                    .Select(g => g.Grupos).FirstOrDefault()
            });
        }

        internal static TutorImprimirDetalleAlumnoViewModel ObtenerImpresionDetallesViewModel(Guid idAlumno, SMTDevEntities db) {
            var alumno = db.Alumno.Where(a => a.IDAlumno == idAlumno);
            var bimestres = new List<InformacionBimestreViewModel>();

            for (int bimestre = 1; bimestre <= 5; bimestre++) {
                var info = alumno.Select(a => new InformacionBimestreViewModel {
                    Numero = bimestre,

                    Sesion = db.Sesion
                        .Where(i => i.IDGrupo == a.IDGrupo && i.Bimestres.Bimestre == bimestre)
                        .SelectMany(m => a.AlumnoSesion
                            .Where(i => i.Sesion != null)
                            .Where(i => i.Sesion.IDBimestre == m.IDBimestre)
                            .OrderByDescending(i => i.Sesion.Fecha))
                        .GroupBy(i => i.IDSesion)
                        .Select(grp => grp.FirstOrDefault())
                        .Where(i => i != null)
                        .AsEnumerable(),

                    Trabajo = db.Trabajo
                        .Where(i => i.IDGrupo == a.IDGrupo && i.Bimestres.Bimestre == bimestre)
                        .SelectMany(m => a.TrabajoAlumno
                            .Where(i => i.Trabajo != null)
                            .Where(i => i.Trabajo.IDBimestre == m.IDBimestre)
                            .OrderByDescending(i => i.Trabajo.Fecha))
                        .GroupBy(i => i.IDTrabajoAlumno)
                        .Select(grp => grp.FirstOrDefault())
                        .Where(i => i != null)
                        .AsEnumerable(),

                    Portafolio = db.Portafolio
                        .Where(i => i.IDGrupo == a.IDGrupo && i.Bimestres.Bimestre == bimestre)
                        .SelectMany(m => a.PortafolioAlumno
                            .Where(i => i.Portafolio != null)
                            .Where(i => i.Portafolio.IDBimestre == m.IDBimestre)
                            .OrderByDescending(i => i.Portafolio.FechaEntrega))
                        .AsEnumerable(),

                    Habilidades = db.HabilidadesAlumno
                        .Where(ha => ha.IDAlumno == idAlumno)
                        .Where(ha => ha.Bimestres.Bimestre == bimestre)
                        .Select(hab => new TutorHabilidadesViewModel {
                            Autoevaluacin = hab != null ? hab.Autoevaluacion : null,
                            Coevaluacion = hab != null ? hab.Coevaluacion : null,
                            Conocimiento = hab != null ? hab.Conocimiento : null,
                            Sintesis = hab != null ? hab.Sintesis : null,
                            Argumentacion = hab != null ? hab.Argumentacion : null,
                            ApoyoLectura = hab != null ? hab.ApoyoLectura : null,
                            ApoyoEscritura = hab != null ? hab.ApoyoEscritura : null,
                            ApoyoMatematicas = hab != null ? hab.ApoyoMatematicas : null,
                            SeInvolucraClase = hab != null ? hab.SeInvolucraClase : null
                        }).FirstOrDefault(),

                    Examen = db.Examen.Where(i => i.Bimestres.IDGrupo == a.IDGrupo),
                    ExamenAlumno = a.ExamenAlumno
                }).AsEnumerable().Select(a => {
                    var ai = alumno.Select(i => new {i.IDGrupo}).FirstOrDefault();

                    a.Headers = AlumnoDesempenio.cargarHeadersDeReporte(ai.IDGrupo, bimestre);
                    a.Desempeno = AlumnoDesempenio.cargarReporte(ai.IDGrupo, bimestre, idAlumno).FirstOrDefault();

                    return a;
                }).FirstOrDefault();

                if (
                    info.Habilidades != null ||
                    info.Examen.Any() ||
                    info.Sesion.Any() ||
                    info.Trabajo.Any() ||
                    info.Portafolio.Any()
                ) {
                    bimestres.Add(info);
                }
            }

            var result = alumno.Select(a => new TutorImprimirDetalleAlumnoViewModel {
                IDAlumno = a.IDAlumno,
                IDGrupo = a.IDGrupo,
                CURP = string.IsNullOrEmpty(a.Curp) ? "" : a.Curp.Trim().ToUpper(),
                NombreCompleto = a.NombreCompleto,
                Grado = a.Grupos.Grado,
                Escuela = a.Grupos.Escuela,
                Materia = a.Grupos.Materia,
                Grupo = a.Grupos.Grupo,
                EsUSAER = a.EsUSAER,
            }).AsEnumerable().Select(a => {
                a.Bimestres = bimestres;
                return a;
            }).FirstOrDefault();

            return result;
        }

        internal static TutorDetalleAlumnoViewModel ObtenerDetallesViewModel(Guid idAlumno, int bimestre, SMTDevEntities db) {
            var alumno = db.Alumno.Where(a => a.IDAlumno == idAlumno);
            var result = alumno.Select(a => new TutorDetalleAlumnoViewModel {
                IDAlumno = a.IDAlumno,
                IDGrupo = a.IDGrupo,
                CURP = string.IsNullOrEmpty(a.Curp) ? "" : a.Curp.Trim().ToUpper(),
                NombreCompleto = a.NombreCompleto,
                Grado = a.Grupos.Grado,
                Escuela = a.Grupos.Escuela,
                Materia = a.Grupos.Materia,
                Grupo = a.Grupos.Grupo,
                EsUSAER = a.EsUSAER,
                Bimestre = bimestre,

                Sesion = db.Sesion
                    .Where(i => i.IDGrupo == a.IDGrupo && i.Bimestres.Bimestre == bimestre)
                    .SelectMany(m => a.AlumnoSesion
                        .Where(i => i.Sesion != null)
                        .Where(i => i.Sesion.IDBimestre == m.IDBimestre)
                        .OrderByDescending(i => i.Sesion.Fecha))
                    .GroupBy(i => i.IDSesion)
                    .Select(grp => grp.FirstOrDefault())
                    .Where(i => i != null)
                    .AsEnumerable(),

                Trabajo = db.Trabajo
                    .Where(i => i.IDGrupo == a.IDGrupo && i.Bimestres.Bimestre == bimestre)
                    .SelectMany(m => a.TrabajoAlumno
                        .Where(i => i.Trabajo != null)
                        .Where(i => i.Trabajo.IDBimestre == m.IDBimestre)
                        .OrderByDescending(i => i.Trabajo.Fecha))
                    .GroupBy(i => i.IDTrabajoAlumno)
                    .Select(grp => grp.FirstOrDefault())
                    .Where(i => i != null)
                    .AsEnumerable(),

                Portafolio = db.Portafolio
                    .Where(i => i.IDGrupo == a.IDGrupo && i.Bimestres.Bimestre == bimestre)
                    .SelectMany(m => a.PortafolioAlumno
                        .Where(i => i.Portafolio != null && i.Portafolio.IDBimestre == m.IDBimestre)
                        .OrderByDescending(i => i.Portafolio.FechaEntrega))
                    .AsEnumerable(),

                Examenes = db.ExamenAlumno
                    .Where(r => 
                        r.IDAlumno == a.IDAlumno && 
                        r.ExamenTema.Examen.Bimestres.Bimestre == bimestre)
                    .Select(i => i.ExamenTema.Examen)
                    .GroupBy(e => e.IDExamen)
                    .Select(grp => grp.FirstOrDefault())
                    .Where(ex => ex != null)
                    .OrderBy(ex => ex.FechaEntrega)
                    .Select(ex => new ExamenDetalleAlumnoViewModel {
                        Examen = ex,
                        Respuestas = a.ExamenAlumno
                            .Where(r =>
                                r.ExamenTema != null && 
                                r.ExamenTema.Examen != null && 
                                r.ExamenTema.Examen.Bimestres != null &&
                                r.ExamenTema.Examen.Bimestres.Bimestre == bimestre && 
                                r.ExamenTema.Examen.IDExamen == ex.IDExamen)
                    })
                    .AsEnumerable(),

                Habilidades = db.HabilidadesAlumno
                    .Where(ha => ha.IDAlumno == idAlumno)
                    .Where(ha => ha.Bimestres.Bimestre == bimestre)
                    .Select(hab => new TutorHabilidadesViewModel {
                        Autoevaluacin = hab != null ? hab.Autoevaluacion : null,
                        Coevaluacion = hab != null ? hab.Coevaluacion : null,
                        Conocimiento = hab != null ? hab.Conocimiento : null,
                        Sintesis = hab != null ? hab.Sintesis : null,
                        Argumentacion = hab != null ? hab.Argumentacion : null,
                        ApoyoLectura = hab != null ? hab.ApoyoLectura : null,
                        ApoyoEscritura = hab != null ? hab.ApoyoEscritura : null,
                        ApoyoMatematicas = hab != null ? hab.ApoyoMatematicas : null,
                        SeInvolucraClase = hab != null ? hab.SeInvolucraClase : null
                    }).FirstOrDefault()
            }).AsEnumerable().Select(a => {
                a.Headers = AlumnoDesempenio.cargarHeadersDeReporte(a.IDGrupo, bimestre);
                a.Desempeno = AlumnoDesempenio.cargarReporte(a.IDGrupo, bimestre, idAlumno).FirstOrDefault();
                return a;
            }).FirstOrDefault();

            return result;
        }

        public static void QuitarAlumnoATutor(Guid idAlumno, string idTutor, SMTDevEntities db) {
            var relacion = db.TutorAlumno.FirstOrDefault(ta => ta.IdAlumno == idAlumno && ta.IdTutor == idTutor);
            if (relacion != null) db.TutorAlumno.Remove(relacion);
            else throw new System.Exception("El tutor no tiene asignado al alumno especificado");

            db.SaveChanges();
        }

        public static void AgregarAlumnoATutor(AgregarTutorAlumnoViewModel model, SMTDevEntities db) {
            var tutorid = GetTutorIdByEmail(model.Email);
            db.TutorAlumno.Add(new TutorAlumno {
                IdAlumno = model.Id,
                IdTutor = tutorid,
                FechaSync = DateTime.Now
            });

            db.SaveChanges();
        }

        internal static bool TieneAccesoACurp(string curp, string tutorid, SMTDevEntities db) {
            curp = curp?.Trim().ToUpper();
            if (string.IsNullOrEmpty(curp)) return false;

            var alumnoIds = db.Alumno.Where(o => o.Curp.Trim().ToUpper() == curp).Select(o => o.IDAlumno);
            return db.TutorAlumno.Any(t => alumnoIds.Contains(t.IdAlumno) && t.IdTutor == tutorid);
        }

        public static IEnumerable<object> ListarTutoresDeAlumno(Guid idAlumno, SMTDevEntities db) {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var tutoresIds = db.TutorAlumno
                .Where(t => t.IdAlumno == idAlumno)
                .GroupBy(o => o.IdTutor)
                .Select(o => o.Key)
                .ToArray();

            return userManager.Users
                .Where(u => tutoresIds.Contains(u.Id))
                .Select(o => new {
                    Nombre = o.Nombre + " " + o.ApellidoPaterno + " " + o.ApellidoMaterno,
                    o.Email,
                    o.Id
                }).ToList();
        }

        public static bool TieneMaximaCantidadDeTutores(AgregarTutorAlumnoViewModel model, SMTDevEntities db) {
            var curpAlumno = db.Alumno.Where(o => o.IDAlumno == model.Id).Select(o => o.Curp).FirstOrDefault() ?? "";
            var alumnoIds = db.Alumno.Where(o => o.Curp.Trim().ToUpper() == curpAlumno.Trim().ToUpper()).Select(o => o.IDAlumno);
            return db.TutorAlumno
                .Where(t => alumnoIds.Contains(t.IdAlumno))
                .GroupBy(o => o.IdTutor)
                .Select(o => o.Key)
                .LongCount() >= CantidadMaximaTutores;
        }

        #region Usuarios
        public static async Task<bool> ExisteTutorConEmail(string email) {
            var db = new ApplicationDbContext();
            var roleMgr = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (await roleMgr.RoleExistsAsync(Rol)) {
                var userid = db.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefault();
                if (!string.IsNullOrEmpty(userid)) {
                    return await userManager.IsInRoleAsync(userid, Rol);
                }
            }

            return false;
        }

        public static string GetTutorIdByEmail(string email) {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userid = userManager.Users.Where(u => u.Email == email).Select(u => u.Id).FirstOrDefault();
            return userid;
        }
        #endregion
    }
}