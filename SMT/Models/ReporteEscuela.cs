using Microsoft.AspNet.Identity.Owin;
using SMT.Models.DB;
using SMT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models
{
    public class ReporteEscuela
    {
        internal static IEnumerable<dynamic> ListarGrupos(string usuario, SMTDevEntities db) {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var grupos = db.Credencial
                .Where(c => c.IDUsuarioPaga == usuario)
                .SelectMany(c => db.Grupos.Where(g => g.IDUsuario == c.IDUsuarioBeneficiario && g.Status == 1))
                .GroupBy(g => g.IDGrupo)
                .Select(grp => grp.FirstOrDefault())
                .Select(o => new {
                    IDGrupo = o.IDGrupo,
                    Grupo = o.Grado + "° " + o.Grupo,
                    Materia = (o.Materia ?? "").ToUpper(),
                    IDUsuario = o.IDUsuario,
                }).ToArray();
            var idUsuarios = grupos.Select(g => g.IDUsuario);
            var nombres = 
               (from u in userManager.Users where idUsuarios.Contains(u.Id)
                select new { u.Id, Nombre = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno }).ToArray();

            return
                from g in grupos
                join u in nombres on g.IDUsuario equals u.Id
                select new { g.IDGrupo, g.Grupo, g.Materia, u.Nombre };
        }

        internal static ReporteEscuelaViewModel GenerarReporte(IEnumerable<Guid> idGrupos, int bimestre, SMTDevEntities db) {
            var asistencia = db.Sesion
                .Where(i => idGrupos.Contains(i.IDGrupo) && i.Bimestres.Bimestre == bimestre)
                .SelectMany(s => s.AlumnoSesion
                    .Where(a => a.Sesion != null)
                    .Select(a => new { s.IDGrupo, a.Estado, a.IDAlumno }))
                .ToList();
            var defaultGuid = default(Guid);
            var trabajos = db.Trabajo
                .Where(i => idGrupos.Contains(i.IDGrupo ?? defaultGuid) && i.Bimestres.Bimestre == bimestre)
                .SelectMany(m => m.TrabajoAlumno
                    .Where(i => i.Trabajo != null)
                    .Select(t => new { m.IDGrupo, t.Estado, t.IDAlumno }))
                .ToList();

            Func<string, double> toDouble = str => {
                double res = 0;
                return double.TryParse(str, out res) ? double.IsNaN(res) ? 0 : res : res;
            };

            var instrumentos = db.Portafolio
                .Where(i => idGrupos.Contains(i.IDGrupo ?? defaultGuid) && i.Bimestres.Bimestre == bimestre)
                .SelectMany(m => m.PortafolioAlumno
                    .Where(i => i.Portafolio != null)
                    .OrderByDescending(i => i.Portafolio.FechaEntrega)
                    .Select(i => new {
                        A1 = i.Aspecto1, A2 = i.Aspecto2, A3 = i.Aspecto3,
                        A4 = i.Aspecto4, A5 = i.Aspecto5, E = i.Estado,
                        ID = m.IDGrupo, IDA = i.IDAlumno
                    }))
                .AsEnumerable()
                .Select(o => new {
                    A1 = toDouble(o.A1), A2 = toDouble(o.A2), A3 = toDouble(o.A3),
                    A4 = toDouble(o.A4), A5 = toDouble(o.A5), Estado = o.E,
                    IDGrupo = o.ID, IDAlumno = o.IDA
                })
                .ToList();

            var examenes = db.Alumno
                .Where(a => idGrupos.Contains(a.IDGrupo))
                .SelectMany(a => a.ExamenAlumno
                    .Where(e => e.ExamenTema.Examen.Bimestres.Bimestre == bimestre) // XD
                    .Select(o => new {
                        a.IDGrupo, o.Calificacion, a.IDAlumno, o.ExamenTema.Examen.Tipo
                    }))
                .ToList();

            var promedios = idGrupos.Select(idGrupo => {
                var query = db.Alumno.Where(a => a.IDGrupo == idGrupo);
                var prome = (
                    bimestre == 1 ? query.Select(a => a.PromedioBimestre1) :
                    bimestre == 2 ? query.Select(a => a.PromedioBimestre2) :
                    bimestre == 3 ? query.Select(a => a.PromedioBimestre3) :
                    bimestre == 4 ? query.Select(a => a.PromedioBimestre4) :
                    query.Select(a => a.PromedioBimestre5)).AsEnumerable();

                return new {
                    idGrupo,
                    alumnos = prome.Select(a => toDouble("" + a)).ToList()
                };
            }).ToList();

            Func<string, ReporteEscuelaViewModel.ExamenReporte> getExamenReporte = str => {
                var ex = examenes.Where(o => o.Tipo == str);
                return new ReporteEscuelaViewModel.ExamenReporte {
                    Promedio = ex.Any() ? ex.Sum(e => e.Calificacion ?? 0) / ex.Count() : 0,
                    Aprovado = ex.Count(e => (e.Calificacion ?? 0) >= 6),
                    NoAprovado = ex.Count(e => (e.Calificacion ?? 0) < 6),
                };
            };

            return new ReporteEscuelaViewModel {
                Asistencias = new ReporteEscuelaViewModel.AsistenciasReporte {
                    Faltas = asistencia.Count(o => o.Estado == (int) TipoAsistenia.FALTA),
                    Asistencias = asistencia.Count(o => o.Estado == (int) TipoAsistenia.ASISTIO),
                    Justificaciones = asistencia.Count(o => o.Estado == (int) TipoAsistenia.JUSTIFICACION),
                    Retardo = asistencia.Count(o => o.Estado == (int) TipoAsistenia.RETRAZO),
                    Suspencion = asistencia.Count(o => o.Estado == (int) TipoAsistenia.SUSPENCION),
                },
                Trabajos = new ReporteEscuelaViewModel.TrabajosReporte {
                    Entregados = trabajos.Count(t => t.Estado == 1),
                    NoEntregados = trabajos.Count(t => t.Estado == 0),
                    Incompleto = trabajos.Count(t => t.Estado == 2)
                },
                Instrumentos = new ReporteEscuelaViewModel.InstrumentosReporte {
                    Promedio = instrumentos.Any() ? instrumentos.Sum(i => i.A1 + i.A2 + i.A3 + i.A4 + i.A5) / instrumentos.Count() : 0,
                    Aprovados = instrumentos.Count(i => (i.A1 + i.A2 + i.A3 + i.A4 + i.A5) >= 6),
                    NoAprovados = instrumentos.Count(i => (i.A1 + i.A2 + i.A3 + i.A4 + i.A5) < 6)
                },
                Examenes = new ReporteEscuelaViewModel.ExamenesReporte {
                    Diagnostico = getExamenReporte("Diagnostico"),
                    Parciales = getExamenReporte("Parcial"),
                    Bimestrales = getExamenReporte("Bimestral"),
                    Recuperacion = getExamenReporte("Recuperación"),
                },
                Promedios = promedios.Select(prom => {
                    var grupo = db.Grupos.Where(g => g.IDGrupo == prom.idGrupo);
                    var alumn = prom.alumnos;

                    return new ReporteEscuelaViewModel.PromedioGeneralGrupoReporte {
                        Grupo = grupo.Select(g => g.Grado + "° " + g.Grupo).FirstOrDefault() ?? "",
                        Promedio = alumn.Any() ? alumn.Sum(cal => cal) / alumn.Count() : 0,
                        NoAprovados = alumn.Count(cal => cal < 6),
                        P6 = alumn.Count(cal => cal >= 6 && cal < 7),
                        P7 = alumn.Count(cal => cal >= 7 && cal < 8),
                        P8 = alumn.Count(cal => cal >= 8 && cal < 9),
                        P9Y10 = alumn.Count(cal => cal >= 9)
                    };
                })
            };
        }
    }
}