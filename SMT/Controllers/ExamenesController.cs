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
    public class ExamenesController : Controller
    {
        public JsonResult Listar(Guid grupo,long bimestre)
        {
            return Json(Examen.listar(grupo, bimestre,Usuario.getIDVisor(User.Identity.GetUserId())),JsonRequestBehavior.AllowGet);
        }

        public JsonResult Get(Guid examen)
        {
            return Json(Examen.buscar(examen,Usuario.getIDVisor(User.Identity.GetUserId())), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Nuevo()
        {
            return PartialView(new Examen());
        }

        [HttpPost]
        public JsonResult Nuevo(Examen examen, Guid Grupo, int bimestre)
        {
            try
            {
                Grupos grupo = Grupos.get(Grupo);

                if (grupo.IDUsuario != User.Identity.GetUserId())
                    throw new Exception("No tienes acceso a este grupo");

                examen.IDBimestre = grupo.Bimestres.Where(a => a.Bimestre == bimestre).Select(a => a.IDBimestre).FirstOrDefault();

                if(examen.IDBimestre == default(Guid))
                    throw new Exception("Bimestre no encontrado");

                examen.IDExamen = Guid.NewGuid();
                Guid id = examen.crear(User.Identity.GetUserId());

            
                Examen.actualizarAlumnos(id);

                return Json(new ResultViewModel(true,null, Examen.buscar(id, User.Identity.GetUserId())[0]));
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
                    ExamenAlumno a = db.ExamenAlumno.FirstOrDefault(i => i.IDAlumno == alumno && i.IDTema == tema);

                    if(!db.Grupos.Any(i => i.IDGrupo == grupo && i.IDUsuario == usuario && i.Alumno.Any(b =>b.IDAlumno == alumno)))
                    {
                        throw new Exception("No tienes registrado este alumno");
                    }

                    if (!db.ExamenTema.Any(i => i.IDTema == tema && i.Examen.Bimestres.Grupos.IDUsuario == usuario))
                    {
                        throw new Exception("No acceso a este examen");
                    }

                    if (a == null)
                    {
                        a = new ExamenAlumno()
                        {
                            IDAlumno = alumno,
                            IDTema = tema,
                            FechaActualizacion = DateTime.Now,
                            FechaSync = DateTime.Now,
                            Calificacion = calificacion
                        };
                        db.ExamenAlumno.Add(a);
                        db.SaveChanges();
                    }
                    else
                    {
                        a.Calificacion = calificacion;
                        a.FechaActualizacion = DateTime.Now;
                    }

                    db.SaveChanges();



                    AlumnoDesempenio.actualizarAlumno(alumno, grupo, db.ExamenTema.Where(b => b.IDTema == tema).Select(b => b.Examen.Bimestres.Bimestre.Value).FirstOrDefault() , new { examen = true });

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
                Examen.eliminar(id, User.Identity.GetUserId());
                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Editar(Examen examen)
        {
            try
            {
                examen.editar(User.Identity.GetUserId());
                return Json(new ResultViewModel(true, null, Examen.buscar(examen.IDExamen, User.Identity.GetUserId()).FirstOrDefault()));
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
                string usuario =Usuario.getIDVisor(User.Identity.GetUserId());
                AmazonS3.SubirArchivo(file.InputStream, archivo, "/"+ usuario +  "/examenes/");

                return Json(new ResultViewModel(true,null, archivo));
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public FileResult Download(Guid examen)
        {

            using (SMTDevEntities db = new SMTDevEntities())
            {
                string usuario = User.Identity.GetUserId();
                Usuario usr = Usuario.GetByName(User.Identity.Name);
                Examen exa = db.Examen.FirstOrDefault(i => i.IDExamen == examen && i.Bimestres.Grupos.IDUsuario == usuario);
                Grupos grupo = exa.Bimestres.Grupos;
                MemoryStream ms = new MemoryStream();
                var doc = DocX.Create(ms, DocumentTypes.Document);

                if (exa == null)
                {

                    doc.InsertParagraph("No tiene acceso a este examen");
                    doc.Save();
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                }
                else
                {

                   

                    string[] titulos = new string[]
                    {
                        exa.Bimestres.Grupos.Escuela,
                        exa.Bimestres.Grupos.Materia + " " + exa.Bimestres.Grupos.Grado + exa.Bimestres.Grupos.Grupo,
                        usr.Nombre + " " + usr.ApellidoPaterno + " " + usr.ApellidoMaterno,
                        exa.Tipo,
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
                        if (i < 3)
                        {
                            encabezado.Append(text);
                            encabezado.Append(Environment.NewLine);
                        }
                        else {
                            titulo.Append(text);
                            titulo.Append(Environment.NewLine);
                        }
                        

                    }

                    doc.InsertParagraph(Environment.NewLine);

                    doc.InsertParagraph("Nombre:____________________________________________________________");

                    doc.InsertParagraph(Environment.NewLine);

                    int index = 0;
                    foreach (var m in exa.ExamenTema.ToArray())
                    {

                        if (String.IsNullOrWhiteSpace(m.Instrucciones))
                        {
                            index++;
                        }
                        else
                        {
                            index = 1;
                        }

                        Paragraph pregunta;
                        Paragraph instrucciones;

                        instrucciones = doc.InsertParagraph(m.Instrucciones);
                        pregunta = doc.InsertParagraph(index + ". " + m.Pregunta);                        
                        pregunta.FontSize(10);
                        pregunta.Italic();

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
                                    Novacode.Image img = doc.AddImage(AmazonS3.DescargarArchivo(m.Archivo, "/" + usuario + "/examenes"));

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

                        
                        

                    }

                    
                    doc.Save();
                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document",exa.Titulo + ".docx");
                }

                
               
            }
        }

        public ActionResult ImprimirCalificaciones(Guid grupo, long bimestre)
        {

            List<CalificacionExamenViewModel> examenes = new List<CalificacionExamenViewModel>();


            SMTDevEntities db = new SMTDevEntities();

            foreach(Examen exa in db.Examen.Where(i => i.Bimestres.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre).OrderBy(i => i.FechaEntrega).ToList())
            {
                examenes.Add(new CalificacionExamenViewModel()
                {
                    idExamen = exa.IDExamen,
                    tipo = exa.Tipo,
                    titulo = exa.Titulo,
                    calificaciones = db.ExamenAlumno.Where(i => i.ExamenTema.IDExamen == exa.IDExamen)
                    .GroupBy(i => i.IDAlumno)
                    .Select(i => new CalificacionExamenViewModel.CalificacionAlumno()
                    {
                         calificacion = i.Select(a => a.Calificacion.Value).Sum() * 10 / i.Select(a => a.ExamenTema.Reactivos).Sum(),
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
            var db = new SMTDevEntities();
            var examenes = db.Examen
                .Where(i => i.Bimestres.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre)
                .OrderBy(i => i.FechaEntrega)
                .Select(exa => new ExamenViewModel {
                    idExamen = exa.IDExamen,
                    tipo = exa.Tipo,
                    titulo = exa.Titulo,
                    alumnos = db.ExamenAlumno
                        .Where(i => i.ExamenTema.IDExamen == exa.IDExamen)
                        .Select(i => new ExamenViewModel.CalificacionAlumno {
                            calificacion = i.Calificacion != null ? i.Calificacion.Value : 5,
                            idAlumno = i.IDAlumno,
                            pregunta = i.IDTema
                        }).ToList(),
                    preguntas = db.ExamenTema.Where(i => i.IDExamen == exa.IDExamen)
                        .Select(i => new ExamenViewModel.Pregunta {
                            idPregunta = i.IDTema,
                            nombre = i.Instrucciones,
                            reactivos = i.Reactivos,
                            tema = i.Nombre
                        }).ToList()
                }).ToList();

            var alumnos = db.Alumno.Where(i => i.IDGrupo == grupo)
                .OrderBy(i => i.ApellidoPaterno)
                .ThenBy(i => i.ApellidoMaterno)
                .ThenBy(i => i.Nombre)
                .ToList();

            ViewBag.alumnos = alumnos;
            ViewBag.header = new HeaderGrupoReporte(grupo, $"Exámen bimestre {bimestre}");

            return View(examenes);
        }
    }
}