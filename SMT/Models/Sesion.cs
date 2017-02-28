using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models.DB
{
    public partial class Sesion
    {
        public static SesionSimple nueva(Guid grupo, int bimestre, int estado,string usuario)
        {
            using(var db = new SMTDevEntities())
            {
                if (!db.Grupos.Any(i => i.IDGrupo == grupo && i.IDUsuario == usuario))
                    throw new Exception("No tienes acceso a este grupo");

                Guid? idBimestre = db.Bimestres.Where(i => i.IDGrupo == grupo && i.Bimestre == bimestre && i.Grupos.IDUsuario == usuario).Select(i => i.IDBimestre).FirstOrDefault();

                if (idBimestre == default(Guid) || idBimestre == null)
                    throw new Exception("No tienes acceso a este bimestre");

                Sesion ses = new Sesion {
                    IDSesion = Guid.NewGuid(),
                    Fecha = DateTime.Now,
                    FechaActualiacion = DateTime.Now,
                    IDGrupo = grupo,
                    Observacion = "",
                    IDBimestre = idBimestre.Value
                };

                db.Sesion.Add(ses);
                db.SaveChanges();

                var alumnos = db.Alumno.Where(i => i.IDGrupo == grupo).Select(i => i.IDAlumno).ToList();
                var result = new SesionSimple {
                    fecha = Util.toHoraMexico(DateTime.Now).ToString("dd-MM-yyyy"),
                    id = ses.IDSesion,
                    asistencia = new List<AsistenciaAlumno>()
                }; 

                foreach (var id in alumnos)
                {
                    db.AlumnoSesion.Add(new DB.AlumnoSesion {
                        IDAlumno = id,
                        IDSesion = ses.IDSesion,
                        FechaActualizacion = DateTime.Now,
                        Estado = estado
                    });

                    result.asistencia.Add(new AsistenciaAlumno {
                        id = id,
                        estado = estado,
                        semaforo = AlumnoDesempenioStatus.BIEN
                    });

                    db.SaveChanges();
                    AlumnoDesempenio.actualizarAlumno(id, grupo, bimestre, new { asistencia = true },true);
                }
               


                return result;
            }
        }

        public static List<SesionSimple> cargarSesiones(Guid grupo, int bimestre, string usuario, int page, int pageSize)
        {
            using(var db = new SMTDevEntities()) {
                var result = db.Sesion
                    .Where(i => i.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre && i.Grupos.IDUsuario == usuario)
                    .OrderByDescending(i => i.Fecha)
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .Select(s => new {
                        id = s.IDSesion,
                        fecha = s.Fecha,
                        observacion = s.Observacion,
                        asistencia = db.AlumnoSesion.Where(i => i.IDSesion == s.IDSesion)
                            .Select(i => new AsistenciaAlumno {
                                id = i.IDAlumno,
                                estado = i.Estado,
                                semaforo = i.Alumno.AlumnoDesempenio
                                    .Where(a => a.IDGrupo == grupo && a.Bimestre == bimestre)
                                    .Select(a => a.ColorAsistencia).FirstOrDefault()
                            }).ToList()
                    }).AsEnumerable()
                    .Select(i => new SesionSimple {
                        fecha = i.fecha.ToString("dd-MM-yyyy"),
                        id = i.id,
                        observacion = i.observacion,
                        asistencia = i.asistencia
                    }).ToList();

                return result;
            }
        }
        
        public static void editar(Guid id, DateTime fecha, string observacion,string usuario)
        {
            using(SMTDevEntities db = new SMTDevEntities())
            {
                fecha = Util.toHoraUTC(fecha);
                if (!db.Sesion.Any(i => i.IDSesion == id && i.Grupos.IDUsuario == usuario))
                    throw new Exception("No tienes acceso a este grupo");

                Sesion ses = db.Sesion.FirstOrDefault(i => i.IDSesion == id);

                ses.Fecha = fecha;
                ses.Observacion = observacion;
                ses.FechaActualiacion = DateTime.Now;
                db.SaveChanges();
            }
        }

        public static void eliminar(Guid id, string usuario)
        {
            using (var db = new SMTDevEntities()) {
                if (!db.Sesion.Any(i => i.IDSesion == id && i.Grupos.IDUsuario == usuario))
                    throw new Exception("No tienes acceso a este grupo");

                Sesion ses = db.Sesion.FirstOrDefault(i => i.IDSesion == id);

                ses.AlumnoSesion.Clear();
                

                db.Sesion.Remove(ses);
                db.SaveChanges();
            }
        }

        public class SesionSimple
        {
            public Guid id { get; set; }
            public string fecha { get; set; }
            public List<AsistenciaAlumno> asistencia { get; set; }
            public string observacion { get; set; }

        }

        public class AsistenciaAlumno
        {
            public Guid id { get; set; }
            public int estado { get; set; }
            public string semaforo { get; set; }

        }
    }

    public enum TipoAsistenia
    {
        FALTA,ASISTIO,RETRAZO,SUSPENCION,JUSTIFICACION
    }
}