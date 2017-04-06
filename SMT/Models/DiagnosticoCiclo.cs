using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace SMT.Models.DB
{
    public partial class DiagnosticoCiclo
    {
        private static int tamanioImgByte = 2248576;

        public static List<DiagnosticoCicloResult> listar(Guid grupo, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                List<DiagnosticoCiclo> DiagnosticoCicloes = db.DiagnosticoCiclo.Where(i => i.Bimestres.IDGrupo == grupo && i.Bimestres.Grupos.IDUsuario == usuario).ToList();

                string url = ConfigurationManager.AppSettings["AWSUrl"] + "/" + usuario + "/DiagnosticoCicloes/";

                List<DiagnosticoCicloResult> result = DiagnosticoCicloes.OrderByDescending(i => i.FechaEntrega).Select(i => new DiagnosticoCicloResult()
                {
                    IDDiagnosticoCiclo = i.IDDiagnosticoCiclo.ToString(),
                    Titulo = i.Titulo,
                    FechaEntrega = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                    FechaEntregaDesplegable = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                    Temas = i.DiagnosticoCicloTema.Select(t => new TemasResult()
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
                        Alumnos = t.DiagnosticoCicloAlumno.Select(a => new AlumnoDiagnosticoCicloResult()
                        {
                            IDAlumno = a.IDAlumno.ToString(),
                            Calificacion = a.Calificacion == null ? 0 : a.Calificacion.Value,
                        }).ToList()
                    })
                   .ToList()
                }).ToList();

                return result;
            }
        }

        public static List<DiagnosticoCicloResult> buscar(Guid DiagnosticoCiclo, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                List<DiagnosticoCiclo> DiagnosticoCicloes = db.DiagnosticoCiclo.Where(i => i.IDDiagnosticoCiclo == DiagnosticoCiclo && i.Bimestres.Grupos.IDUsuario == usuario).ToList();

                string url = ConfigurationManager.AppSettings["AWSUrl"] + "/" + usuario + "/DiagnosticoCiclos/";

                List<DiagnosticoCicloResult> result = DiagnosticoCicloes.Select(i => new DiagnosticoCicloResult()
                {
                    IDDiagnosticoCiclo = i.IDDiagnosticoCiclo.ToString(),
                    Titulo = i.Titulo,
                    FechaEntrega = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                    FechaEntregaDesplegable = Util.toHoraMexico(i.FechaEntrega).ToString("dd-MM-yyyy"),
                    Temas = i.DiagnosticoCicloTema.Select(t => new TemasResult()
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
                        Alumnos = t.DiagnosticoCicloAlumno.Select(a => new AlumnoDiagnosticoCicloResult()
                        {
                            IDAlumno = a.IDAlumno.ToString(),
                            Calificacion = a.Calificacion == null ? 0 : a.Calificacion.Value
                        }).ToList()
                    })
                   .ToList()
                }).ToList();

                return result;
            }
        }

        public Guid crear(string usuario)
        {

            using (var db = new SMTDevEntities())
            {

                IDDiagnosticoCiclo = Guid.NewGuid();

                foreach (var m in this.DiagnosticoCicloTema.ToList())
                {
                    m.IDTema = m.IDTema == default(Guid) ? Guid.NewGuid() : m.IDTema;

                    if (m.file != null)
                    {
                        if (m.file.ContentLength <= tamanioImgByte)
                        {
                            m.Archivo = Guid.NewGuid().ToString() + ".jpg";
                            Stream imagen = Util.convertirJPGNew(m.file.InputStream, 800, 800);
                            AmazonS3.SubirArchivo(imagen, m.Archivo, "/" + usuario + "/DiagnosticoCiclos");
                        }
                        else
                        {
                            throw new System.ArgumentException("Error: el tamaño maximo de la imagen es de 2mb", "");
                        }


                    }

                }

                FechaRegistro = DateTime.Now;
                FechaActualizacion = DateTime.Now;
                Titulo = Util.UppercaseFirst(Titulo);
                IDDiagnosticoCiclo = IDDiagnosticoCiclo == default(Guid) ? Guid.NewGuid() : IDDiagnosticoCiclo;
                db.DiagnosticoCiclo.Add(this);
                db.SaveChanges();

                return IDDiagnosticoCiclo;
            }
        }

        public void editar(string usuario)
        {

            using (SMTDevEntities db = new SMTDevEntities())
            {
                DiagnosticoCiclo original = db.DiagnosticoCiclo.FirstOrDefault(a => a.IDDiagnosticoCiclo == IDDiagnosticoCiclo && a.Bimestres.Grupos.IDUsuario == usuario);
                if (original == null)
                {
                    throw new Exception("No se encontró el diagnostico por ciclo");
                }

                original.FechaActualizacion = DateTime.Now;
                original.FechaEntrega = FechaEntrega;

                original.Titulo = Util.UppercaseFirst(Titulo);


                db.SaveChanges();

                // Agregar/actualizar temas
                foreach (var m in this.DiagnosticoCicloTema.ToList())
                {
                    DiagnosticoCicloTema tema = original.DiagnosticoCicloTema.FirstOrDefault(a => a.IDTema == m.IDTema);

                    if (tema == null)
                    {
                        m.IDTema = m.IDTema == default(Guid) ? Guid.NewGuid() : m.IDTema;


                        if (m.file != null)
                        {



                            if (m.file.ContentLength <= tamanioImgByte)
                            {


                                Stream file = m.file.InputStream;
                                tema.Archivo = Guid.NewGuid().ToString() + ".jpg";
                                Stream imagen = Util.convertirJPGNew(file, 800, 800);
                                AmazonS3.SubirArchivo(imagen, tema.Archivo, "/" + usuario + "/DiagnosticoCiclos");


                            }
                            else
                            {
                                throw new System.ArgumentException("Error: el tamaño maximo de la imagen es de 2mb", "");
                            }
                        }

                        original.DiagnosticoCicloTema.Add(m);
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
                        tema.FechaSync = DateTime.Now;

                        if (m.file != null)
                        {



                            if (m.file.ContentLength <= tamanioImgByte)
                            {


                                Stream file = m.file.InputStream;
                                tema.Archivo = Guid.NewGuid().ToString() + ".jpg";
                                Stream imagen = Util.convertirJPGNew(file, 800, 800);
                                AmazonS3.SubirArchivo(imagen, tema.Archivo, "/" + usuario + "/DiagnosticoCiclos");


                            }
                            else
                            {
                                throw new System.ArgumentException("Error: el tamaño maximo de la imagen es de 2mb", "");
                            }
                        }

                        db.SaveChanges();
                    }
                }

                // Elilminar temas que no esten en la lista
                foreach (var tema in original.DiagnosticoCicloTema.ToList())
                {
                    if (!this.DiagnosticoCicloTema.Any(a => a.IDTema == tema.IDTema))
                    {
                        db.DiagnosticoCicloTema.Remove(tema);
                        db.SaveChanges();
                    }
                }

            };
        }

        public static void actualizarAlumnos(Guid id)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                DiagnosticoCiclo exa = db.DiagnosticoCiclo.FirstOrDefault(i => i.IDDiagnosticoCiclo == id);

                var alumnos = exa.Bimestres.Grupos.Alumno.Select(i => i.IDAlumno).ToList();

                foreach (var tema in exa.DiagnosticoCicloTema.ToList())
                {
                    foreach (var a in alumnos)
                    {
                        if (!tema.DiagnosticoCicloAlumno.Any(i => i.IDAlumno == a))
                        {
                            tema.DiagnosticoCicloAlumno.Add(new DiagnosticoCicloAlumno()
                            {
                                IDAlumno = a,
                                IDTema = tema.IDTema,
                                FechaSync = DateTime.Now,
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
                DiagnosticoCiclo exa = db.DiagnosticoCiclo.FirstOrDefault(a => a.IDDiagnosticoCiclo == id && a.Bimestres.Grupos.IDUsuario == usuario);
                if (exa == null)
                {
                    throw new Exception("No se encontro el Diagnostico por ciclo");
                }

                db.DiagnosticoCiclo.Remove(exa);
                db.SaveChanges();
            };
        }

        #region Clases
        public class DiagnosticoCicloResult
        {
            public string IDDiagnosticoCiclo { get; set; }
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
            public string Instrucciones { get; set; }
            public List<AlumnoDiagnosticoCicloResult> Alumnos { get; set; }
        }

        public class AlumnoDiagnosticoCicloResult
        {
            public string IDAlumno { get; set; }
            public double Calificacion { get; set; }
        }

        #endregion
    }

    public partial class DiagnosticoCicloTema
    {
        public HttpPostedFileBase file { get; set; }

    }
}