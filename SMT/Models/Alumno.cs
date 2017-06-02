using Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace SMT.Models.DB
{
    [MetadataType(typeof(AlumnoMD))]
    public partial class Alumno
    {
        #region Metadatos 
        public class AlumnoMD
        {
            [Display(Name = "Nombre completo")]
            public string NombreCompleto { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Nombre")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Apellido Paterno")]
            public string ApellidoPaterno { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Apellido Materno")]
            public string ApellidoMaterno { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "CURP")]
            public string Curp { get; set; }
                        

            [Required(ErrorMessage = "Es obligatorio")]            
            [Display(Name = "Es NEE")]
            public bool EsUSAER { get; set; }

            

        }
        #endregion

        public bool? EsTaller { get; set; }
        public List<HttpPostedFileBase> archivo { get; set; }

        public List<Sesion> sesionArray { get; set; }
        public List<Trabajo> trabajoArray { get; set; }
        public List<Portafolio> portafolioArray { get; set; }
        public List<Examen> examenArray { get; set; }



        public class AlumnoSimple
        {
            public string id { get; set; }
            public string nombre { get; set; }
            public string apellidoPaterno { get; set; }
            public string apellidoMaterno { get; set; }
            public string curp { get; set; }
            public int estado { get; set; }
            public int total { get; set; }
            public string grupo { get; set; }
            public string salon { get; set; }
            public string semaforo { get; set; }
            public string colorSemaforo { get; set; }
        }

        #region CRUD
        public Guid crear()
        {
            SMTDevEntities db = new SMTDevEntities();
            this.FechaActualizacion = DateTime.Now;                        
            this.Estado = 1;
            this.Nombre = Util.UpperTitle(this.Nombre);
            this.ApellidoPaterno = Util.UpperTitle(this.ApellidoPaterno);
            this.ApellidoMaterno = Util.UpperTitle(this.ApellidoMaterno);
            this.NombreCompleto = Util.RemoveDiacritics(this.Nombre + " " + this.ApellidoPaterno + " " + this.ApellidoMaterno).ToLower();
            this.Curp = this.Curp.ToUpper();
            this.IDAlumno = IDAlumno == default(Guid) ? Guid.NewGuid() : IDAlumno;

            db.Alumno.Add(this);
            db.SaveChanges();

            #region Actualizar relaciones
            // Agregamos al nuevo alumno en todos los proyectos, examenes, etc existentes

            List<Bimestres> bimestres = db.Bimestres.Where(i => i.IDGrupo == IDGrupo).ToList();

            foreach(var bim in bimestres) {
                foreach(var exam in bim.Examen.ToList()) {
                    foreach(var pregunta in exam.ExamenTema.ToList()) {
                        pregunta.ExamenAlumno.Add(new ExamenAlumno {
                            IDAlumno = IDAlumno,
                            Calificacion = 0,
                            FechaActualizacion = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                }

                foreach (var exam in bim.DiagnosticoCiclo.ToList()) {
                    foreach (var pregunta in exam.DiagnosticoCicloTema.ToList()) {
                        pregunta.DiagnosticoCicloAlumno.Add(new DiagnosticoCicloAlumno {
                            IDAlumno = IDAlumno,
                            Calificacion = 0,
                            FechaActualizacion = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                }

                foreach (var portafolio in bim.Portafolio.ToList())
                {
                    portafolio.PortafolioAlumno.Add(new DB.PortafolioAlumno() {
                        IDPortafolioAlumno = Guid.NewGuid(),
                        IDAlumno = IDAlumno,
                        Aspecto1 = "0",
                        Aspecto2 = "0",
                        Aspecto3 = "0",
                        Aspecto4 = "0",
                        Aspecto5 = "0",
                        Estado = 0,
                        FechaActualizacion = DateTime.Now
                    });
                    db.SaveChanges();
                }

                
                foreach (var trabajo in bim.Trabajo.ToList())
                {
                    trabajo.TrabajoAlumno.Add(new DB.TrabajoAlumno()
                    {
                        IDAlumno = IDAlumno,
                        IDTrabajoAlumno = Guid.NewGuid(),
                        Observaciones = "",
                        Estado = 1,
                        FechaActualizacion = DateTime.Now
                    });
                    db.SaveChanges();
                }

                DB.AlumnoDesempenio.actualizarAlumno(IDAlumno, IDGrupo, 1, null,true);
                DB.AlumnoDesempenio.actualizarAlumno(IDAlumno, IDGrupo, 2, null,true);
                DB.AlumnoDesempenio.actualizarAlumno(IDAlumno, IDGrupo, 3, null,true);
                DB.AlumnoDesempenio.actualizarAlumno(IDAlumno, IDGrupo, 4, null,true);
                DB.AlumnoDesempenio.actualizarAlumno(IDAlumno, IDGrupo, 5, null,true);
            }

            #endregion

            return this.IDAlumno;
        }

        public Guid editar()
        {
            SMTDevEntities db = new SMTDevEntities();
            Alumno p = db.Alumno.Where(i => i.IDAlumno == IDAlumno).FirstOrDefault();
            p.Nombre = Util.UpperTitle(Nombre);
            p.ApellidoPaterno = Util.UpperTitle(ApellidoPaterno);
            p.ApellidoMaterno = Util.UpperTitle(ApellidoMaterno);
            p.NombreCompleto = Util.RemoveDiacritics(Nombre + " " + ApellidoPaterno + " " + ApellidoMaterno).ToLower();
            p.Curp = Curp.ToUpper();
            p.EsUSAER = EsUSAER;
            p.Grupo = Grupo;
            p.FechaActualizacion = DateTime.Now;
            p.FechaSync = DateTime.Now;
            db.SaveChanges();

            return IDAlumno;
        }

        public static Guid eliminar(Guid ID)
        {
            SMTDevEntities db = new SMTDevEntities();
            Alumno p = db.Alumno.Where(i => i.IDAlumno == ID).FirstOrDefault();
            p.AlumnoSesion.Clear();
            p.TrabajoAlumno.Clear();
            p.ExamenAlumno.Clear();
            p.PortafolioAlumno.Clear();
            db.Alumno.Remove(p);
            db.SaveChanges();
            return p.IDAlumno;
        }
        #endregion

        public static string ImportData(MemoryStream stream, int option, Guid IDGrupo)
        {
            
            string rechazados = "";
            int ok = 0;
            int fail = 0;
            int repeat = 0;
            var cont = 0;
            if (stream != null)
            {
                SMTDevEntities db = new SMTDevEntities();
                stream.Position = 0;
                IExcelDataReader reader;
                if (option == 1)
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                else
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                DataSet result = reader.AsDataSet();

                int row = 1;
                int table = 0;
                
                while (table < result.Tables.Count)
                {

                    foreach (var column in result.Tables[table].Columns.Cast<DataColumn>().ToArray())
                    {
                        if (result.Tables[table].AsEnumerable().All(i => i.IsNull(column)))
                            result.Tables[table].Columns.Remove(column);
                    }

                    foreach (var mrow in result.Tables[table].Rows.Cast<DataRow>().ToArray())
                    {
                        if (string.IsNullOrEmpty(string.Join("", mrow.ItemArray).Trim()))
                            result.Tables[table].Rows.Remove(mrow);
                    }

                    int k = 0;
                    while (row < result.Tables[table].Rows.Count)
                    {
                        
                        string column = result.Tables[table].Rows[0][0].ToString();
                        string temp = result.Tables[table].Rows[row][0].ToString();
                        if (column.Contains("Nombre") && temp != null && !string.IsNullOrEmpty(temp))
                        {
                            Alumno alu = new Alumno();
                            alu.Nombre = result.Tables[table].Rows[row][0].ToString();
                            alu.ApellidoPaterno = result.Tables[table].Rows[row][1].ToString();
                            alu.ApellidoMaterno = result.Tables[table].Rows[row][2].ToString();
                            alu.Curp = result.Tables[table].Rows[row][3].ToString();
                            if (result.Tables[table].Rows[row][4].ToString().ToLower() == "si" || result.Tables[table].Rows[row][4].ToString().ToLower()=="x")
                            {
                                alu.EsUSAER = true;
                            }
                            else
                            {
                                alu.EsUSAER = false;
                            }
                            if (result.Tables[table].Rows[0][5].ToString() == "Grupo*")
                            {
                                if (!String.IsNullOrEmpty(result.Tables[table].Rows[row][5].ToString()))
                                {
                                    Regex rgx = new Regex(@"^[A-Za-z]{1,3}");
                                    if (rgx.IsMatch(result.Tables[table].Rows[row][5].ToString()))
                                    {
                                        alu.Grupo = result.Tables[table].Rows[row][5].ToString();
                                    }
                                    else
                                    {
                                        return "Formato Incorrecto en el Grupo: " + result.Tables[table].Rows[row][5].ToString() + " (Ej. ABC, A, B)";
                                    }
                                }
                            }
                            
                            alu.IDGrupo = IDGrupo;
                            alu.crear();
                            cont++;
                        }
                        else
                        {
                            cont--;
                        }
                        row++;
                        
                        
                    }
                    table++;
                }
            }
            return "Ok";
        }

        public static Alumno getAlumno(Guid ID)
        {
            SMTDevEntities db = new SMTDevEntities();
            return db.Alumno.Where(i => i.IDAlumno == ID).FirstOrDefault();
        }

        public static List<AlumnoSimple> listaSimple(Guid idgrupo, string usuario)
        {
            using (var db = new SMTDevEntities()) {
                var alumnos = db.Alumno
                    .Where(i => i.IDGrupo == idgrupo && i.Grupos.IDUsuario == usuario)
                    .OrderBy(i => i.ApellidoPaterno)
                    .ThenBy(i => i.ApellidoMaterno)
                    .ThenBy(i => i.Nombre)
                    .Select(i => new {
                        i.IDAlumno, i.Nombre, i.ApellidoMaterno,
                        i.ApellidoPaterno, i.Curp, i.EsUSAER, i.Estado,
                        i.Grupo, i.ColorPromedio,
                        i.PromedioBimestre1, i.PromedioBimestre2, i.PromedioBimestre3,
                        i.PromedioBimestre4, i.PromedioBimestre5, i.PromedioTotal,
                        Secciones = new {
                            Trabajos = new {
                                Any = i.TrabajoAlumno.Select(t => t.Trabajo).Where(t => t.IDGrupo == idgrupo).Any(),
                                Semaforo = i.AlumnoDesempenio.Where(o => o.IDGrupo == idgrupo).Select(o => o.PromedioTrabajo).FirstOrDefault()
                            },
                            Portafolios = new {
                                Any = i.PortafolioAlumno.Select(t => t.Portafolio).Where(t => t.IDGrupo == idgrupo).Any(),
                                Semaforo = i.AlumnoDesempenio.Where(o => o.IDGrupo == idgrupo).Select(o => o.PromedioPortafolio).FirstOrDefault()
                            },
                            Examenes = new {
                                Any = i.ExamenAlumno.Any(),
                                Semaforo = i.AlumnoDesempenio.Where(o => o.IDGrupo == idgrupo).Select(o => o.PromedioExamen).FirstOrDefault()
                            },
                            Asistencias = new {
                                Any = i.AlumnoSesion.Select(t => t.Sesion).Where(t => t.IDGrupo == idgrupo).Any(),
                                Semaforo = i.AlumnoDesempenio.Where(o => o.IDGrupo == idgrupo).Select(o => o.PromedioAsistencia).FirstOrDefault()
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
                            promedioParcial < 6 ? AlumnoDesempenioStatus.APOYO :
                            promedioParcial < 8.5 ? AlumnoDesempenioStatus.REGULAR :
                            promedioParcial >= 8.5 ? AlumnoDesempenioStatus.BIEN : "";

                        return new AlumnoSimple {
                            id = i.IDAlumno.ToString(),
                            nombre = i.Nombre,
                            apellidoPaterno = i.ApellidoPaterno,
                            apellidoMaterno = i.ApellidoMaterno,
                            curp = i.Curp,
                            estado = i.EsUSAER ? 0 : i.Estado,
                            grupo = i.Grupo,
                            semaforo = i.EsUSAER ? "NEE" : 
                                colorPromedio == AlumnoDesempenioStatus.BIEN ? "BIEN" :
                                colorPromedio == AlumnoDesempenioStatus.REGULAR ? "REGULAR" :
                                colorPromedio == AlumnoDesempenioStatus.APOYO ? "APOYO" : "",
                            colorSemaforo = i.EsUSAER ? AlumnoDesempenioStatus.USAER : colorPromedio,
                        };
                    })
                    .ToList();

                return alumnos;
            }
        }

        public static List<AlumnoSimple> busqueda(string usuario, string nombre,int page = 1, int pageSize = 20)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                List<AlumnoSimple> alumnos = new List<AlumnoSimple>();
                nombre = Util.RemoveDiacritics(nombre.ToLower());

                var total = db.Alumno.Where(i => i.Grupos.IDUsuario == usuario && i.NombreCompleto.Contains(nombre)).Count();
                
                alumnos = db.Alumno.Where(i=>i.Grupos.IDUsuario==usuario && i.NombreCompleto.Contains(nombre))
                            .Select(i => new AlumnoSimple()
                            {
                                id = i.IDAlumno.ToString(),
                                nombre = i.Nombre,
                                apellidoPaterno = i.ApellidoPaterno,
                                apellidoMaterno = i.ApellidoMaterno,
                                curp = i.Curp,
                                estado = i.Estado,
                                total = total,
                                salon=i.Grupos.Materia+" "+i.Grupos.Grado + ""+i.Grupos.Grupo,
                                grupo=i.Grupos.IDGrupo.ToString(),
                            })
                            .OrderBy(i => i.nombre+" "+i.apellidoPaterno)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

                return alumnos;
            }
        }

        public static void actualizarEstadoSesion(Guid idalumno, Guid idsesion, int estado, string usuario)
        {
            using(SMTDevEntities db = new SMTDevEntities())
            {
                if (!db.Sesion.Any(i => i.IDSesion == idsesion && i.Grupos.IDUsuario == usuario))
                    throw new Exception("No tienes acceso a esta sesión");

                AlumnoSesion aluse = db.AlumnoSesion.FirstOrDefault(i => i.IDAlumno == idalumno && i.IDSesion == idsesion);

                if(aluse == null)
                {
                    aluse = new DB.AlumnoSesion() {
                        IDAlumno = idalumno,
                        IDSesion = idsesion,
                        Estado = estado,
                        FechaActualizacion = DateTime.Now
                    };

                    db.AlumnoSesion.Add(aluse);
                    db.SaveChanges();
                }
                else
                {
                    aluse.Estado = estado;
                    aluse.FechaActualizacion = DateTime.Now;
                    aluse.FechaSync = DateTime.Now;
                    db.SaveChanges();
                }

                DB.AlumnoDesempenio.actualizarAlumno(idalumno, aluse.Sesion.IDGrupo, aluse.Sesion.Bimestres.Bimestre.Value, new { asistencia = true });

            }
        }
        public static List<Sesion> getAlumnoSesiones(Guid ID, Guid IDGrupo)
        {
            SMTDevEntities db = new SMTDevEntities();
            return db.Sesion.Where(i => i.IDGrupo == IDGrupo).OrderBy(i => i.Bimestres.Bimestre).ToList();
        }
        public static List<Trabajo> getAlumnoTrabajos(Guid ID, Guid IDGrupo)
        {
            SMTDevEntities db = new SMTDevEntities();
            return db.Trabajo.Where(i => i.IDGrupo == IDGrupo).OrderBy(i => i.Bimestres.Bimestre).ToList();
        }
        public static List<Portafolio> getAlumnoPortafolio(Guid ID, Guid IDGrupo)
        {
            SMTDevEntities db = new SMTDevEntities();
            return db.Portafolio.Where(i => i.Bimestres.IDGrupo == IDGrupo).OrderBy(i => i.Bimestres.Bimestre).ToList();
        }
        public static List<Examen> getAlumnoExamen(Guid ID, Guid IDGrupo)
        {
            SMTDevEntities db = new SMTDevEntities();
            return db.Examen.Where(i => i.Bimestres.IDGrupo == IDGrupo).OrderBy(i => i.Bimestres.Bimestre).ToList();
        }
        

        public class AsistenciaViewModal
        {
            public string sesion { get; set; }
            public List<AlumnoAsistencia> alumnos { get; set; }

            public class AlumnoAsistencia
            {
                public Guid idAlumno { get; set; }
                public int estado { get; set; }

            }
        }
    }
}