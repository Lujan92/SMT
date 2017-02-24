using Microsoft.AspNet.Identity;
using Novacode;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize, Credencial]
    public class DiagnosticoCiclosController : Controller
    {
        public JsonResult Listar(Guid grupo,long bimestre)
        {
            return Json(DiagnosticoCiclo.listar(grupo,Usuario.getIDVisor(User.Identity.GetUserId())),JsonRequestBehavior.AllowGet);
        }

        public JsonResult Get(Guid diagnostico)
        {
            return Json(DiagnosticoCiclo.buscar(diagnostico,Usuario.getIDVisor(User.Identity.GetUserId())), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Nuevo()
        {
            return PartialView(new DiagnosticoCiclo());
        }

        [HttpPost]
        public JsonResult Nuevo(DiagnosticoCiclo DiagnosticoCiclo, Guid Grupo, int bimestre = 1)
        {
            try
            {
                Grupos grupo = Grupos.get(Grupo);

                if (grupo.IDUsuario != User.Identity.GetUserId())
                    throw new Exception("No tienes acceso a este grupo");

                DiagnosticoCiclo.IDBimestre = grupo.Bimestres
                    .Where(a => a.Bimestre == bimestre)
                    .Select(a => a.IDBimestre)
                    .FirstOrDefault();

                if (DiagnosticoCiclo.IDBimestre == default(Guid))
                    throw new Exception("Bimestre no encontrado");

                Guid id = DiagnosticoCiclo.crear(User.Identity.GetUserId());
                DiagnosticoCiclo.actualizarAlumnos(id);

                return Json(new ResultViewModel(true,null, DiagnosticoCiclo.buscar(id, User.Identity.GetUserId()).FirstOrDefault()));
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult actualizarCalificacion(Guid alumno, Guid tema, int calificacion, Guid grupo)
        {
            try
            {
                using (SMTDevEntities db = new SMTDevEntities())
                {

                    string usuario = User.Identity.GetUserId();
                    DiagnosticoCicloAlumno a = db.DiagnosticoCicloAlumno.FirstOrDefault(i => i.IDAlumno == alumno && i.IDTema == tema);

                    if(!db.Grupos.Any(i => i.IDGrupo == grupo && i.IDUsuario == usuario && i.Alumno.Any(b =>b.IDAlumno == alumno)))
                    {
                        throw new Exception("No tienes registrado este alumno");
                    }

                    if (!db.DiagnosticoCicloTema.Any(i => i.IDTema == tema && i.DiagnosticoCiclo.Bimestres.Grupos.IDUsuario == usuario))
                    {
                        throw new Exception("No acceso a este diagnostico por ciclo");
                    }

                    if (a == null)
                    {
                        a = new DiagnosticoCicloAlumno()
                        {
                            IDAlumno = alumno,
                            IDTema = tema,
                            FechaActualizacion = DateTime.Now,
                            Calificacion = calificacion
                        };
                        db.DiagnosticoCicloAlumno.Add(a);
                        db.SaveChanges();
                    }
                    else
                    {
                        a.Calificacion = calificacion;
                        a.FechaActualizacion = DateTime.Now;
                    }

                    db.SaveChanges();

                    AlumnoDesempenio.actualizarAlumno(alumno, grupo, db.DiagnosticoCicloTema.Where(b => b.IDTema == tema).Select(b => b.DiagnosticoCiclo.Bimestres.Bimestre.Value).FirstOrDefault(), new { diagnostico = true });


                    return Json(new ResultViewModel(true, null, null));
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
                DiagnosticoCiclo.eliminar(id, User.Identity.GetUserId());
                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Editar(DiagnosticoCiclo DiagnosticoCiclo)
        {
            try
            {
                DiagnosticoCiclo.editar(User.Identity.GetUserId());
                return Json(new ResultViewModel(true, null, DiagnosticoCiclo.buscar(DiagnosticoCiclo.IDDiagnosticoCiclo, User.Identity.GetUserId())[0]));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult SubirArchivo(HttpPostedFileBase file)
        {
            try
            {
                string archivo = Guid.NewGuid().ToString() + ".jpg";
                string usuario = User.Identity.GetUserId();
                AmazonS3.SubirArchivo(file.InputStream, archivo, "/"+ usuario + "/DiagnosticoCiclos/");

                return Json(new ResultViewModel(true,null, archivo));
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public FileResult Download(Guid diagnostico)
        {

            using (SMTDevEntities db = new SMTDevEntities())
            {
                string usuario = User.Identity.GetUserId();
                DiagnosticoCiclo exa = db.DiagnosticoCiclo.FirstOrDefault(i => i.IDDiagnosticoCiclo == diagnostico && i.Bimestres.Grupos.IDUsuario == usuario);

                MemoryStream ms = new MemoryStream();
                var doc = DocX.Create(ms, DocumentTypes.Document);

                if (exa == null)
                {

                    doc.InsertParagraph("No tiene acceso a este dagnostico por ciclo");
                    doc.Save();
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                }
                else
                {
                    int index = 1;


                    string[] titulos = new string[]
                    {
                        exa.Bimestres.Grupos.Escuela,
                        exa.Bimestres.Grupos.Materia + " " + exa.Bimestres.Grupos.Grado + exa.Bimestres.Grupos.Grupo,
                        exa.Titulo
                    };

                    doc.AddHeaders();
                    Header header_default = doc.Headers.odd;
                    Paragraph encabezado = header_default.InsertParagraph();
                    Paragraph titulo = doc.InsertParagraph();

                    encabezado.Alignment = Alignment.center;
                    encabezado.FontSize(14D);
                    encabezado.Bold();

                    titulo.Alignment = Alignment.center;
                    titulo.FontSize(14D);
                    titulo.Bold();

                    for (int i = 0; i < titulos.Length; i++)
                    {

                        string text = titulos[i];
                        if (i < 2)
                        {
                            encabezado.Append(text);
                            encabezado.Append(Environment.NewLine);
                        }
                        else
                        {
                            titulo.Append(text);
                            titulo.Append(Environment.NewLine);
                        }


                    }
                    
                    doc.InsertParagraph(Environment.NewLine);

                    foreach (var m in exa.DiagnosticoCicloTema.ToArray())
                    {
                        

                        Paragraph pregunta;
                        Paragraph instrucciones;

                        pregunta = doc.InsertParagraph(index + ". " + m.Pregunta);
                        instrucciones = doc.InsertParagraph(m.Instrucciones);
                        instrucciones.FontSize(10);
                        instrucciones.Italic();

                        switch (m.TipoTema)
                        {
                            default:
                            case "Sin personalizar":
                                
                                doc.InsertParagraph(Environment.NewLine);
                                break;
                            case "Multiple":
                            case "Laguna":
                                doc.InsertParagraph(m.Respuesta);
                                break;
                            case "Columnas":
                                Table columnas = doc.AddTable(1, 2);


                                columnas.Rows[0].Cells[0].Width = 450;
                                columnas.Rows[0].Cells[1].Width = 450;
                                columnas.Rows[0].Cells[0].InsertParagraph(m.Respuesta1);
                                columnas.Rows[0].Cells[1].InsertParagraph(m.Respuesta2);

                                columnas.SetBorder(TableBorderType.Bottom, new Border(BorderStyle.Tcbs_none, 0, 0, Color.Black));
                                columnas.SetBorder(TableBorderType.Top, new Border(BorderStyle.Tcbs_none, 0, 0, Color.Black));
                                columnas.SetBorder(TableBorderType.Left, new Border(BorderStyle.Tcbs_none, 0, 0, Color.Black));
                                columnas.SetBorder(TableBorderType.Right, new Border(BorderStyle.Tcbs_none, 0, 0, Color.Black));
                                columnas.SetBorder(TableBorderType.InsideV, new Border(BorderStyle.Tcbs_none, 0, 0, Color.Black));

                                doc.InsertTable(columnas);

                                break;
                            case "Abierta":
                                if (m.Archivo != null)
                                {    
                                    Novacode.Image img = doc.AddImage(AmazonS3.DescargarArchivo(m.Archivo, "/" + usuario + "/DiagnosticoCiclos"));

                                    Paragraph p = doc.InsertParagraph("", false);
                                    Picture pic1 = img.CreatePicture();
                                    
                                    p.InsertPicture(pic1);
                                    p.Alignment = Alignment.center;
                                    
                                }

                                doc.InsertParagraph(Environment.NewLine);
                                break;
                        }



                        pregunta.Bold();
                        pregunta.FontSize(14D);

                        doc.InsertParagraph(Environment.NewLine);
                        
                        index++;

                    }

                    
                    doc.Save();
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document",exa.Titulo + ".docx");
                }

                
               
            }
        }

        public ActionResult ImprimirCalificaciones(Guid grupo, long bimestre)
        {

            var examenes = new List<CalificacionExamenViewModel>();
            var db = new SMTDevEntities();

            foreach (DiagnosticoCiclo exa in db.DiagnosticoCiclo.Where(i => i.Bimestres.IDGrupo == grupo).OrderBy(i => i.FechaEntrega).ToList())
            {
                examenes.Add(new CalificacionExamenViewModel
                {
                    idExamen = exa.IDDiagnosticoCiclo,
                    titulo = exa.Titulo,
                    calificaciones = db.DiagnosticoCicloAlumno.Where(i => i.DiagnosticoCicloTema.IDDiagnosticoCiclo == exa.IDDiagnosticoCiclo)
                    .GroupBy(i => i.IDAlumno)
                    .Select(i => new CalificacionExamenViewModel.CalificacionAlumno()
                    {
                        calificacion = i.Select(a => a.Calificacion.Value).Sum() * 10 / i.Select(a => a.DiagnosticoCicloTema.Reactivos).Sum(),
                        idAlumno = i.Key
                    })
                    .ToList()
                });
            }

            List<Alumno> alumnos = db.Alumno.Where(i => i.IDGrupo == grupo).OrderBy(i => i.NombreCompleto).ToList();
            ViewBag.alumnos = alumnos;

            return View(examenes);
        }

        public ActionResult Imprimir(Guid grupo, long bimestre)
        {
            var examenes = new List<ExamenViewModel>();
            ViewBag.header = new HeaderGrupoReporte(grupo, "Diagnóstico");

            SMTDevEntities db = new SMTDevEntities();

            foreach (DiagnosticoCiclo exa in db.DiagnosticoCiclo.Where(i => i.Bimestres.IDGrupo == grupo ).OrderBy(i => i.FechaEntrega).ToList())
            {
                examenes.Add(new ExamenViewModel()
                {
                    idExamen = exa.IDDiagnosticoCiclo,
                    titulo = exa.Titulo,
                    alumnos = db.DiagnosticoCicloAlumno.Where(i => i.DiagnosticoCicloTema.IDDiagnosticoCiclo == exa.IDDiagnosticoCiclo)
                                        .Select(i => new ExamenViewModel.CalificacionAlumno()
                                        {
                                            calificacion = i.Calificacion != null ? i.Calificacion.Value : 5,
                                            idAlumno = i.IDAlumno,
                                            pregunta = i.IDTema
                                        })
                                        .ToList(),
                    preguntas = db.DiagnosticoCicloTema.Where(i => i.IDDiagnosticoCiclo == exa.IDDiagnosticoCiclo)
                                             .Select(i => new ExamenViewModel.Pregunta()
                                             {
                                                 idPregunta = i.IDTema,
                                                 nombre = i.Instrucciones,
                                                 reactivos = i.Reactivos,
                                                 tema = i.Nombre
                                             })
                                             .ToList()
                });
            }

            List<Alumno> alumnos = db.Alumno.Where(i => i.IDGrupo == grupo).OrderBy(i => i.ApellidoPaterno).ThenBy(i => i.ApellidoMaterno).ThenBy(i => i.Nombre).ToList();
            ViewBag.alumnos = alumnos;

            return View(examenes);
        }
    }
}