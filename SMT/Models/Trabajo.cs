using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models.DB
{
    public partial class Trabajo
    {
        public static TrabajoSimple nueva(Guid grupo, int bimestre, int estado, string usuario,string tipo)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                DateTime nowMexico = Util.toHoraMexico(DateTime.Now);

                if (!db.Grupos.Any(i => i.IDGrupo == grupo && i.IDUsuario == usuario))
                    throw new Exception("No tienes acceso a este grupo");

                Guid? idBimestre = db.Bimestres.Where(i => i.IDGrupo == grupo && i.Bimestre == bimestre && i.Grupos.IDUsuario == usuario).Select(i => i.IDBimestre).FirstOrDefault();

                if (idBimestre == default(Guid) || idBimestre == null)
                    throw new Exception("No tienes acceso a este bimestre");
                var contador = db.Trabajo.Where(i => i.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre).Count();
                Trabajo tra = new Trabajo()
                {
                    IDTrabajo = Guid.NewGuid(),
                    Fecha = DateTime.Now,                    
                    IDGrupo = grupo,                    
                    IDBimestre = idBimestre.Value,
                    Observaciones="",                    
                    Nombre="Trabajo-"+(contador+1),
                    Tipo = tipo
                };

                db.Trabajo.Add(tra);
                db.SaveChanges();

                var alumnos = db.Alumno.Where(i => i.IDGrupo == grupo).Select(i => i.IDAlumno).ToList();
                var result = new TrabajoSimple {
                    fecha = nowMexico.ToString("dd-MM-yyyy"),
                    id = tra.IDTrabajo,
                    entrega = new List<EntregaAlumno>(),
                    nombre=tra.Nombre
                };

                foreach (var id in alumnos)
                {
                    db.TrabajoAlumno.Add(new DB.TrabajoAlumno {
                        IDTrabajoAlumno = Guid.NewGuid(),
                        IDAlumno = id,
                        IDTrabajo = tra.IDTrabajo,
                        FechaActualizacion = DateTime.Now,
                        Estado = 1
                    });

                    result.entrega.Add(new EntregaAlumno {
                        id = id,
                        estado = estado
                    });

                    db.SaveChanges();
                    AlumnoDesempenio.actualizarAlumno(id, grupo, bimestre, new { trabajo = true });
                }
               

                

                return result;
            }
        }
        
        public static List<TrabajoSimple> cargarTrabajos(Guid grupo, int bimestre, string usuario, int page, int pageSize)
        {
            List<TrabajoSimple> trabajos = new List<TrabajoSimple>();

            using (SMTDevEntities db = new SMTDevEntities())
            {
                trabajos = db.Trabajo
                    .Where(i => i.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre && i.Grupos.IDUsuario == usuario)
                    .OrderByDescending(i => i.Fecha)
                    .ThenBy(i => i.Nombre)
                    .Skip((page-1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new {
                        entrega = db.TrabajoAlumno
                            .Where(e => e.IDTrabajo == i.IDTrabajo)
                            .Select(e => new EntregaAlumno { id = e.IDAlumno, estado = e.Estado }).ToList(),
                        i.IDTrabajo, i.Fecha, i.Observaciones, i.Nombre, i.Tipo, i.Actividad })
                    .ToList()
                    .Select(i => new TrabajoSimple {
                        id = i.IDTrabajo,
                        observacion = i.Observaciones,
                        nombre = i.Nombre,
                        tipo = i.Tipo,
                        actividad = i.Actividad,
                        entrega = i.entrega,
                        fecha = i.Fecha.Value.ToString("dd-MM-yyyy"),
                        time = i.Fecha.Value.Ticks
                    })
                    .ToList();
            }

            return trabajos;
        }

        public static void editar(Guid id, DateTime fecha, string observacion, string usuario,string nombre, string tipo,string actividad)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                fecha = Util.toHoraUTC(fecha);
                if (!db.Trabajo.Any(i => i.IDTrabajo == id && i.Grupos.IDUsuario == usuario))
                    throw new Exception("No tienes acceso a este grupo");

                Trabajo tra = db.Trabajo.FirstOrDefault(i => i.IDTrabajo == id);

                tra.Nombre = Util.UpperTitle(nombre);
                tra.Fecha = fecha;
                tra.Observaciones = observacion;
                tra.Actividad = actividad;    
                tra.Tipo = tipo;
                tra.FechaSync = DateTime.Now;
                db.SaveChanges();
            }
        }

        public static void eliminar(Guid id, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {

                if (!db.Trabajo.Any(i => i.IDTrabajo == id && i.Grupos.IDUsuario == usuario))
                    throw new Exception("No tienes acceso a este grupo");

                Trabajo tra = db.Trabajo.FirstOrDefault(i => i.IDTrabajo == id);

                tra.TrabajoAlumno.Clear();


                db.Trabajo.Remove(tra);
                db.SaveChanges();
            }
        }

        public Guid editarDescripcion()
        {
            var db = new SMTDevEntities();
            var p = db.Trabajo.Where(i => i.IDTrabajo == IDTrabajo).FirstOrDefault();

            p.Descripcion = Descripcion;
            db.SaveChanges();

            return p.IDTrabajo;
        }

        public static Trabajo get(Guid ID)
        {
            var db = new SMTDevEntities();
            return db.Trabajo.Where(i => i.IDTrabajo == ID).FirstOrDefault();
        }
    }

    public partial class TrabajoAlumno
    {
        public static void actualizarEstado(Guid alumno, Guid trabajo, int estado, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                if (!db.Trabajo.Any(i => i.IDTrabajo == trabajo && i.Grupos.IDUsuario == usuario))
                    throw new Exception("No tienes acceso a esta sesión");

                TrabajoAlumno aluse = db.TrabajoAlumno.FirstOrDefault(i => i.IDAlumno == alumno && i.IDTrabajo == trabajo);

                if (aluse == null)
                {
                    aluse = new DB.TrabajoAlumno()
                    {
                        IDAlumno = alumno,
                        IDTrabajo = trabajo,
                        Estado = estado,
                        FechaActualizacion = DateTime.Now,
                        FechaSync = DateTime.Now,
                    };

                    db.TrabajoAlumno.Add(aluse);
                    db.SaveChanges();

                }
                else
                {
                    aluse.Estado = estado;
                    aluse.FechaActualizacion = DateTime.Now;
                    aluse.FechaSync = DateTime.Now;
                    db.SaveChanges();
                }


                var data  =db.Trabajo.Where(a => a.IDTrabajo == trabajo).Select(i =>new { i.IDGrupo, bimestre = i.Bimestres.Bimestre }).FirstOrDefault();

                AlumnoDesempenio.actualizarAlumno(alumno,data.IDGrupo.Value,data.bimestre.Value, new { trabajo = true });

            }


        }
    }



    public class TrabajoSimple
    {
        public Guid id { get; set; }
        public string fecha { get; set; }
        public List<EntregaAlumno> entrega { get; set; }
        public string observacion { get; set; }
        public string nombre { get; set; }
        public string tipo { get; set; }
        public string actividad { get; set; }
        public long time { get; set; }

    }

    public class EntregaAlumno
    {
        public Guid? id { get; set; }
        public int? estado { get; set; }

    }
}