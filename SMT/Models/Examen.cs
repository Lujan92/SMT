using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace SMT.Models.DB
{
    public partial class Examen
    {

        public static List<ExamenResult> listar(Guid grupo, long bimestre, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                List<Examen> examenes = db.Examen.Where(i => i.Bimestres.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre && i.Bimestres.Grupos.IDUsuario == usuario).ToList();

                string url = ConfigurationManager.AppSettings["AWSUrl"] + "/" + usuario + "/examenes/";

                List<ExamenResult> result = examenes.OrderByDescending(i => i.FechaEntrega).Select(i => new ExamenResult()
                {
                    IDExamen = i.IDExamen.ToString(),
                    Titulo = i.Titulo,
                    Tipo = i.Tipo,
                    FechaEntrega = i.FechaEntrega.ToString("dd-MM-yyyy"),
                    FechaEntregaDesplegable = i.FechaEntrega.ToString("dd-MM-yyyy"),
                    Temas = i.ExamenTema.Select(t => new TemasResult()
                    {
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
                        Alumnos = t.ExamenAlumno.Select(a => new AlumnoExamenResult()
                        {
                            IDAlumno = a.IDAlumno.ToString(),
                            Calificacion = a.Calificacion == null ? 0 : a.Calificacion.Value
                        }).ToList()
                    }).OrderBy(t => t.Nombre)
                   .ToList()
                }).ToList();

                return result;
            }
        }

        public static List<ExamenResult> buscar(Guid examen, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                List<Examen> examenes = db.Examen.Where(i => i.IDExamen == examen && i.Bimestres.Grupos.IDUsuario == usuario).ToList();

                string url = ConfigurationManager.AppSettings["AWSUrl"] + "/" + usuario + "/examenes/";

                List<ExamenResult> result = examenes.Select(i => new ExamenResult()
                {
                    IDExamen = i.IDExamen.ToString(),
                    Titulo = i.Titulo,
                    Tipo = i.Tipo,
                    FechaEntrega = Util.toHoraMexico(i.FechaEntrega).ToString("yyyy-MM-dd"),
                    FechaEntregaDesplegable = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                    Temas = i.ExamenTema.Select(t => new TemasResult()
                    {
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
                        Alumnos = t.ExamenAlumno.Select(a => new AlumnoExamenResult()
                        {
                            IDAlumno = a.IDAlumno.ToString(),
                            Calificacion = a.Calificacion == null ? 0 : a.Calificacion.Value
                        }).ToList()
                    }).OrderBy(t => t.Nombre)
                   .ToList()
                }).ToList();

                return result;
            }
        }



        public Guid crear(string usuario)
        {

            using (SMTDevEntities db = new SMTDevEntities())
            {

                // Validaciones
                if (Tipo == "Parcial" && db.Examen.Where(i => i.IDBimestre == IDBimestre && i.Tipo == "Parcial").Count() > 1)
                {
                    throw new Exception("No se permite crear mas de 2 exámenes parciales por bimestre");
                }

                if (Tipo == "Bimestral" && db.Examen.Where(i => i.IDBimestre == IDBimestre && i.Tipo == "Bimestral").Count() > 0)
                {
                    throw new Exception("No se permite crear mas un examen bimestral por bimestre");
                }

                if (Tipo == "Recuperación" && db.Examen.Where(i => i.IDBimestre == IDBimestre && i.Tipo == "Recuperación").Count() > 0)
                {
                    throw new Exception("No se permite crear mas un examen de recuperación por bimestre");
                }

                if (Tipo == "Diagnostico" && db.Examen.Where(i => i.IDBimestre == IDBimestre && i.Tipo == "Diagnostico").Count() > 0)
                {
                    throw new Exception("No se permite crear mas un examen de diagnostico por bimestre");
                }


                foreach (var m in this.ExamenTema.ToList())
                {
                    m.IDTema = m.IDTema == default(Guid) ? Guid.NewGuid() : m.IDTema;

                    if (m.file != null)
                    {
                        m.Archivo = Guid.NewGuid().ToString() + ".jpg";
                        Stream imagen = Util.convertirJPG(m.file.InputStream, 800, 800);
                        AmazonS3.SubirArchivo(imagen, m.Archivo, "/" + usuario + "/examenes");
                    }

                }

                if (Tipo == "Parcial")
                {
                    int tota = db.Examen.Where(a => a.IDBimestre == IDBimestre && a.Tipo == "Parcial").Count();
                    Titulo = Tipo + " " + (tota + 1);
                }
                else
                {
                    Titulo = Tipo + " " + db.Bimestres.Where(a => a.IDBimestre == IDBimestre).Select(a => a.Bimestre).FirstOrDefault();
                }

                FechaRegistro = DateTime.Now;
                FechaActualizacion = DateTime.Now;
                this.Titulo = Util.UppercaseFirst(this.Titulo);
                db.Examen.Add(this);
                db.SaveChanges();

                return IDExamen;
            }
        }

        public void editar(string usuario)
        {

            using (SMTDevEntities db = new SMTDevEntities())
            {
                Examen original = db.Examen.FirstOrDefault(a => a.IDExamen == IDExamen && a.Bimestres.Grupos.IDUsuario == usuario);
                if (original == null)
                {
                    throw new Exception("No se encontro el examen");
                }

                if (Tipo == "Parcial" && db.Examen.Where(i => i.IDExamen != original.IDExamen && i.IDBimestre == original.IDBimestre && i.Tipo == "Parcial").Count() > 1)
                {
                    throw new Exception("No se permite crear mas de 2 exámenes parciales por bimestre");
                }

                if (Tipo == "Bimestral" && db.Examen.Where(i => i.IDExamen != original.IDExamen && i.IDBimestre == original.IDBimestre && i.Tipo == "Bimestral").Count() > 0)
                {
                    throw new Exception("No se permite crear mas un examen bimestral por bimestre");
                }

                if (Tipo == "Recuperación" && db.Examen.Where(i => i.IDExamen != original.IDExamen && i.IDBimestre == original.IDBimestre && i.Tipo == "Recuperación").Count() > 0)
                {
                    throw new Exception("No se permite crear mas un examen de recuperación por bimestre");
                }

                if (Tipo == "Diagnostico" && db.Examen.Where(i => i.IDExamen != original.IDExamen && i.IDBimestre == original.IDBimestre && i.Tipo == "Diagnostico").Count() > 0)
                {
                    throw new Exception("No se permite crear mas un examen de diagnostico por bimestre");
                }

                // Tipo = "Bimestral";
                // FechaEntrega = DateTime.Now;
                original.FechaActualizacion = DateTime.Now;
                original.FechaSync = DateTime.Now;
                original.FechaEntrega = FechaEntrega;
                original.Tipo = Tipo;

                db.SaveChanges();

                // Agregar/actualizar temas
                foreach (var m in this.ExamenTema.ToList())
                {
                    ExamenTema tema = original.ExamenTema.FirstOrDefault(a => a.IDTema == m.IDTema);

                    if (tema == null)
                    {
                        if (m.file != null)
                        {
                            m.IDTema = m.IDTema == default(Guid) ? Guid.NewGuid() : m.IDTema;
                            m.Archivo = Guid.NewGuid().ToString() + ".jpg";
                            Stream imagen = Util.convertirJPG(m.file.InputStream, 800, 800);
                            AmazonS3.SubirArchivo(imagen, m.Archivo, "/" + usuario + "/examenes");
                        }

                        m.IDTema = m.IDTema == default(Guid) ? Guid.NewGuid() : m.IDTema;

                        var prueba = m.IDTema;
                        var pruebaw = m.IDExamen;
                        original.ExamenTema.Add(m);
                        db.SaveChanges();
                    }
                    else
                    {
                        tema.Nombre = m.Nombre;
                        tema.Pregunta = m.Pregunta;
                        tema.TipoTema = m.TipoTema;
                        tema.Reactivos = m.Reactivos;
                        tema.Respuesta = m.Respuesta;
                        tema.Respuesta1 = m.Respuesta1;
                        tema.Respuesta2 = m.Respuesta2;
                        tema.Instrucciones = m.Instrucciones;

                        if (m.file != null)
                        {
                            tema.IDTema = tema.IDTema == default(Guid) ? Guid.NewGuid() : tema.IDTema;
                            tema.Archivo = Guid.NewGuid().ToString() + ".jpg";
                            Stream imagen = Util.convertirJPG(m.file.InputStream, 800, 800);
                            AmazonS3.SubirArchivo(imagen, tema.Archivo, "/" + usuario + "/examenes");
                        }

                        db.SaveChanges();
                    }
                }

                // Elilminar temas que no esten en la lista
                foreach (var tema in original.ExamenTema.ToList())
                {
                    if (!this.ExamenTema.Any(a => a.IDTema == tema.IDTema))
                    {
                        db.ExamenTema.Remove(tema);
                        db.SaveChanges();
                    }
                }

            };
        }

        public static void actualizarAlumnos(Guid id)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                var exa = db.Examen.FirstOrDefault(i => i.IDExamen == id);
                var alumnos = exa.Bimestres.Grupos.Alumno.Select(i => i.IDAlumno).ToList();
                foreach (var tema in exa.ExamenTema.ToList())
                {
                    foreach (var a in alumnos)
                    {
                        if (!tema.ExamenAlumno.Any(i => i.IDAlumno == a))
                        {
                            tema.ExamenAlumno.Add(new ExamenAlumno
                            {
                                IDAlumno = a,
                                IDTema = tema.IDTema,
                                Calificacion = 0
                            });
                        }
                    }
                }

                db.SaveChanges();
            }


        }

        public static void eliminar(Guid id, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                Examen exa = db.Examen.FirstOrDefault(a => a.IDExamen == id && a.Bimestres.Grupos.IDUsuario == usuario);
                if (exa == null)
                {
                    throw new Exception("No se encontro el examen");
                }

                db.Examen.Remove(exa);
                db.SaveChanges();
            };
        }

        #region Clases
        public class ExamenResult
        {
            public string IDExamen { get; set; }
            public string Titulo { get; set; }
            public string Tipo { get; set; }
            public string FechaEntrega { get; set; }
            public string FechaEntregaDesplegable { get; set; }

            public List<TemasResult> Temas { get; set; }
        }

        public class TemasResult
        {
            public string IDTema { get; set; }
            public string Nombre { get; set; }
            public string TipoTema { get; set; }
            public float Reactivos { get; set; }
            public string Archivo { get; set; }
            public string Pregunta { get; set; }
            public string Respuesta { get; set; }
            public string Respuesta1 { get; set; }
            public string Respuesta2 { get; set; }
            public string UrlArchivo { get; set; }
            public List<AlumnoExamenResult> Alumnos { get; set; }
            public string Instrucciones { get; set; }
        }

        public class AlumnoExamenResult
        {
            public string IDAlumno { get; set; }
            public double Calificacion { get; set; }
        }

        #endregion
    }

    public partial class ExamenTema
    {
        public HttpPostedFileBase file { get; set; }

    }

    public class CalificacionExamenViewModel
    {

        public Guid idExamen { get; set; }
        public string titulo { get; set; }
        public string tipo { get; set; }
        public List<CalificacionAlumno> calificaciones { get; set; }


        public class CalificacionAlumno
        {
            public Guid idAlumno { get; set; }
            public double calificacion { get; set; }
        }
    }

    public class ExamenViewModel
    {

        public Guid idExamen { get; set; }
        public string titulo { get; set; }
        public string tipo { get; set; }
        public List<CalificacionAlumno> alumnos { get; set; }
        public List<Pregunta> preguntas { get; set; }

        public class Pregunta
        {
            public Guid idPregunta { get; set; }
            public string nombre { get; set; }
            public int reactivos { get; set; }
            public string tema { get; set; }
        }

        public class CalificacionAlumno
        {
            public Guid idAlumno { get; set; }
            public Guid pregunta { get; set; }
            public double calificacion { get; set; }
        }
    }
}