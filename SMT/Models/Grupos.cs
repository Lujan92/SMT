using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SMT.Models.DB
{
    [MetadataType(typeof(GruposMD))]
    public partial class Grupos
    {
        #region Metadatos 
        public class GruposMD
        {
            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Materia")]
            public string Materia { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Grado")]
            public string Grado { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]            
            [Display(Name = "Grupo")]
            public string Grupo { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Escuela")]
            public string Escuela { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Registro Federal Escolar")]
            public string RegistroFederalEscolar { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]            
            [Display(Name = "Turno")]
            public string Turno { get; set; }

            [Display(Name = "Color de Fondo")]
            public string Color { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]            
            [RegularExpression(@"^[0-9]{4}[\\\-]{1}[0-9]{4}$", ErrorMessage = "Formato Incorrecto (Ej. 2015-2016)")]
            [Display(Name = "Ciclo")]
            public string Ciclo { get; set; }

        }       

        #endregion

        #region CRUD
        public Guid crear()
        {
            var db = new SMTDevEntities();

            this.Grupo = this.Grupo.ToUpper();
            this.Materia = Util.UpperTitle(this.Materia);
            this.Escuela = Util.UpperTitle(this.Escuela);
            this.Timestamp = DateTime.Now;
            this.Status = 1;
            this.IDGrupo = IDGrupo == default(Guid) ? Guid.NewGuid() : IDGrupo;

            db.Grupos.Add(this);
            db.SaveChanges();
            return this.IDGrupo;
        }

        public Guid editar()
        {
            var db = new SMTDevEntities();
            var p = db.Grupos.Where(i => i.IDGrupo == IDGrupo).FirstOrDefault();

            p.Materia = Util.UpperTitle(Materia);
            p.Grado = Grado;
            p.Escuela = Util.UpperTitle(Escuela);
            p.Grupo = Grupo.ToUpper();
            p.RegistroFederalEscolar = RegistroFederalEscolar;
            p.Timestamp = DateTime.Now;
            p.Color = Color;
            p.Ciclo = Ciclo;
            p.FechaSync = DateTime.Now;
            
            db.SaveChanges();

            return IDGrupo;
        }

        public static Guid eliminar(Guid id)
        {
            var db = new SMTDevEntities();
            var p = db.Grupos.Where(i => i.IDGrupo == id).FirstOrDefault();

            p.Status = 0;
            p.FechaSync = DateTime.Now;
            db.SaveChanges();

            return id;
        }

        public static Guid archivar(Guid id)
        {
            var db = new SMTDevEntities();
            var p = db.Grupos.Where(i => i.IDGrupo == id).FirstOrDefault();

            p.Status = p.Status == 2 ? 1 : 2;
            p.FechaSync = DateTime.Now;
            db.SaveChanges();

            return id;
        }

        #endregion

        public static Grupos get(Guid id)
        {
            var db = new SMTDevEntities();
            return db.Grupos.Where(i => i.IDGrupo == id).FirstOrDefault();
        }

        public static Grupos[] getGrupos(string id, int tipo, int page,int pageSize = 10)
        {
            var db = new SMTDevEntities();
            return db.Grupos.Where(i => i.IDUsuario == id && i.Status== tipo)
                            .OrderBy(i => i.IDGrupo)
                            .Skip(page* pageSize)
                            .Take(pageSize)
                            .ToArray();            
        }

        public  static bool validarMaestro(Guid id, string usuario)
        {
            using(SMTDevEntities db = new SMTDevEntities())
            {

                return db.Grupos.Where(i => i.IDGrupo == id && i.IDUsuario == usuario).Any();
            }
        }

        public static Guid clonar(Guid id , string usuario)
        {
            using (var db = new SMTDevEntities())
            {
                var grupo = db.Grupos.FirstOrDefault(i => i.IDGrupo == id && i.IDUsuario == usuario);
                if (grupo == null)
                    throw new Exception("No existe este grupo");

                double diasDiff = 0;

                if (grupo.Ciclo != null && Regex.IsMatch(grupo.Ciclo, @"\d{4}-\d{4}"))
                {
                    int inicio = int.Parse( grupo.Ciclo.Split('-')[0]);
                    if (inicio < DateTime.Now.Year) {
                        diasDiff = (new DateTime(DateTime.Now.Year, 1, 1) - new DateTime(inicio, 1, 1)).TotalDays;
                        if (diasDiff < 0) diasDiff = 0;
                    }
                }

                var nuevo = new Grupos {
                    IDGrupo = Guid.NewGuid(),
                    IDUsuario = usuario,
                    Materia = grupo.Materia,
                    Ciclo = grupo.Ciclo,
                    Color = grupo.Color,
                    Descripcion = grupo.Descripcion,
                    Grado = grupo.Grado,
                    Grupo = grupo.Grupo,
                    Escuela = grupo.Escuela,
                    Turno = grupo.Turno,
                    RegistroFederalEscolar = grupo.RegistroFederalEscolar,
                    Status = grupo.Status,
                    Timestamp = DateTime.Now
                };

                if(diasDiff > 0)
                {
                    nuevo.Ciclo = DateTime.Now.Year + "-" + (DateTime.Now.Year + 1);
                }

                db.Grupos.Add(nuevo);
                db.SaveChanges();

                foreach(var bim in grupo.Bimestres.ToList())
                {
                    var bn = new Bimestres {
                        IDBimestre = Guid.NewGuid(),
                        IDGrupo = nuevo.IDGrupo,
                        Bimestre = bim.Bimestre,
                    };

                    nuevo.Bimestres.Add(bn);
                    db.SaveChanges();

                    foreach(var p in grupo.Portafolio.Where(i => i.IDBimestre == bim.IDBimestre).ToList())
                    {
                        var pt = new Portafolio {
                            IDPortafolio = Guid.NewGuid(),
                            Activo1 = p.Activo1,
                            Activo2 = p.Activo2,
                            Activo3 = p.Activo3,
                            Activo4 = p.Activo4,
                            Activo5 = p.Activo5,
                            Aspecto1 = p.Aspecto1,
                            Aspecto2 = p.Aspecto2,
                            Aspecto3 = p.Aspecto3,
                            Aspecto4 = p.Aspecto4,
                            Aspecto5 =p.Aspecto5,
                            Criterio1 = p.Criterio1,
                            Criterio2 = p.Criterio2,
                            Criterio3 = p.Criterio3,
                            Criterio4 = p.Criterio4,
                            Criterio5 = p.Criterio5,
                            Descripcion = p.Descripcion,
                            FechaEntrega = p.FechaEntrega.Value.AddDays(diasDiff),
                            IDBimestre = bn.IDBimestre,
                            Nombre = p.Nombre,
                            Observacion1 = p.Observacion1,
                            Observacion2 = p.Observacion2,
                            Observacion3 = p.Observacion3,
                            Observacion4 = p.Observacion4,
                            Observacion5 = p.Observacion5,
                           
                            IDTipoPortafolio = p.IDTipoPortafolio,
                            FechaActualizacion = DateTime.Now,
                        };

                       nuevo.Portafolio.Add(p);
                    }
                    db.SaveChanges();

                    foreach(var tra in grupo.Trabajo.Where(i => i.IDBimestre == bim.IDBimestre).ToList())
                    {
                        nuevo.Trabajo.Add(new Trabajo {
                            IDTrabajo = Guid.NewGuid(),
                            Descripcion = tra.Descripcion,
                            Fecha = tra.Fecha,
                            IDGrupo = nuevo.IDGrupo,
                            Nombre = tra.Nombre,
                            IDBimestre = bn.IDBimestre,
                            Observaciones = tra.Observaciones,
                            Tipo = tra.Tipo,
                        });
                    }

                    db.SaveChanges();

                    foreach(var exa in bim.Examen.ToList())
                    {
                        var exanuevo = new Examen {
                            IDExamen = Guid.NewGuid(),
                            FechaActualizacion = DateTime.Now,
                            FechaRegistro = DateTime.Now,
                            IDBimestre = bn.IDBimestre,
                            FechaEntrega = exa.FechaEntrega.AddDays(diasDiff),
                            Tipo = exa.Tipo,
                            Titulo = exa.Titulo
                        };

                        bn.Examen.Add(exanuevo);
                        db.SaveChanges();

                        foreach(var tema in exa.ExamenTema.ToList())
                        {
                            exanuevo.ExamenTema.Add(new ExamenTema {
                                Archivo = tema.Archivo,
                                Nombre = tema.Nombre,
                                Pregunta = tema.Pregunta,
                                Reactivos = tema.Reactivos,
                                Respuesta = tema.Respuesta,
                                Respuesta1 = tema.Respuesta1,
                                Respuesta2 = tema.Respuesta2,
                                TipoTema = tema.TipoTema
                            });
                        }

                        db.SaveChanges();

                    }

                    foreach (var exa in bim.DiagnosticoCiclo.ToList()) {
                        var exanuevo = new DiagnosticoCiclo {
                            IDDiagnosticoCiclo = Guid.NewGuid(),
                            FechaActualizacion = DateTime.Now,
                            FechaRegistro = DateTime.Now,
                            IDBimestre = bn.IDBimestre,
                            FechaEntrega = exa.FechaEntrega.AddDays(diasDiff),
                            Titulo = exa.Titulo
                        };

                        bn.DiagnosticoCiclo.Add(exanuevo);
                        db.SaveChanges();

                        foreach (var tema in exa.DiagnosticoCicloTema.ToList()) {
                            exanuevo.DiagnosticoCicloTema.Add(new DiagnosticoCicloTema {
                                IDTema = Guid.NewGuid(),
                                Archivo = tema.Archivo,
                                Nombre = tema.Nombre,
                                Pregunta = tema.Pregunta,
                                Reactivos = tema.Reactivos,
                                Respuesta = tema.Respuesta,
                                Respuesta1 = tema.Respuesta1,
                                Respuesta2 = tema.Respuesta2,
                                TipoTema = tema.TipoTema,
                            });
                        }

                        db.SaveChanges();

                    }

                }

                return nuevo.IDGrupo;
            }
        }
    }

    public partial class Bimestres
    {
        #region CRUD
        public Guid crear()
        {
            SMTDevEntities db = new SMTDevEntities();
            IDBimestre = IDBimestre == default(Guid) ? Guid.NewGuid() : IDBimestre;
            db.Bimestres.Add(this);
            db.SaveChanges();
            return this.IDBimestre;
        }
        #endregion

        public static bool validarMaestro(Guid idgrupo, int bimestre, string usuario)
        {
            using(var db = new SMTDevEntities())
            {
                return db.Bimestres.Any(i => i.IDGrupo == idgrupo && i.Bimestre == bimestre && i.Grupos.IDUsuario == usuario && i.Grupos.Status !=0);
            }
        }
    }

    public class HeaderGrupoReporte
    {
        public string maestro { get; set; }
        public string clave { get; set; }
        public string escuela { get; set; }
        public string grado { get; set; }
        public string grupo { get; set; }
        public string materia { get; set; }
        public string titulo { get; set; }

        public HeaderGrupoReporte()
        {

        }

        public HeaderGrupoReporte(Guid idgrupo, string titulo = "")
        {
            using(SMTDevEntities db = new SMTDevEntities())
            {
                Grupos g = db.Grupos.FirstOrDefault(i => i.IDGrupo == idgrupo);
                Usuario user = Usuario.Get(g.IDUsuario);
                this.maestro = string.Format("{0} {1} {2}",user.Nombre,user.ApellidoPaterno, user.ApellidoMaterno);
                this.clave = g.RegistroFederalEscolar;
                this.escuela = g.Escuela;
                this.grado = g.Grado;
                this.grupo = g.Grupo;
                this.materia = g.Materia;
                this.titulo = titulo;
            }
        }
    }

}