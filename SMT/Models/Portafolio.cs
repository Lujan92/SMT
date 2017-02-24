using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMT.Models.DB
{
    [MetadataType(typeof(PortafolioMD))]
    public partial class Portafolio
    {

        #region Metadatos 
        public class PortafolioMD
        {
            private const int longitudCriterio = 8000;
            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Nombre")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [Display(Name = "Fecha Entrega")]
            public DateTime FechaEntrega { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [Display(Name = "Instrumentos")]
            public long IDTipoPortafolio { get; set; }

            [Required(ErrorMessage = "Es obligatorio")]
            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Descripción")]
            public string Descripcion { get; set; }


            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Aspecto #1")]
            public string Aspecto1 { get; set; }


            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Aspecto #2")]
            public string Aspecto2 { get; set; }


            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Aspecto #3")]
            public string Aspecto3 { get; set; }


            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Aspecto #4")]
            public string Aspecto4 { get; set; }


            [StringLength(150, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Aspecto #5")]
            public string Aspecto5 { get; set; }


            [StringLength(longitudCriterio, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Criterios de Evaluación")]
            public string Criterio1 { get; set; }


            [StringLength(longitudCriterio, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Criterios de Evaluación")]
            public string Criterio2 { get; set; }


            [StringLength(longitudCriterio, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Criterios de Evaluación")]
            public string Criterio3 { get; set; }

            [StringLength(longitudCriterio, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Criterios de Evaluación")]
            public string Criterio4 { get; set; }

            [StringLength(longitudCriterio, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
            [Display(Name = "Criterios de Evaluación")]
            public string Criterio5 { get; set; }

        }
        #endregion



        public Guid crear()
        {
            SMTDevEntities db = new SMTDevEntities();


            if (db.TipoPortafolio.Any(a => a.IDTipoPortafolio == IDTipoPortafolio && a.Nombre == "Proyecto") && 
                db.Portafolio.Any(a => a.IDBimestre == IDBimestre && a.IDGrupo == IDGrupo && a.TipoPortafolio.Nombre == "Proyecto"))
                throw new Exception("No se puede crear otro portafolio de tipo proyecto porque solo puede existir uno");

            this.IDPortafolio = Guid.NewGuid();
            this.FechaActualizacion = DateTime.Now;
            this.Nombre = Util.UppercaseFirst(this.Nombre);
            db.Portafolio.Add(this);
            db.SaveChanges();
            return this.IDPortafolio;
        }

        public PortafolioSimple editar(string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                PortafolioSimple result = new PortafolioSimple();

                Portafolio port = db.Portafolio.FirstOrDefault(i => i.IDPortafolio == IDPortafolio && i.Grupos.IDUsuario == usuario);

                if (port == null)
                    throw new Exception("No existe este portafolio");

                if (db.TipoPortafolio.Any(a => a.IDTipoPortafolio == IDTipoPortafolio && a.Nombre == "Proyecto") && 
                    db.Portafolio.Any(a => a.IDPortafolio != port.IDPortafolio && a.IDBimestre == port.IDBimestre && a.IDGrupo == port.IDGrupo && a.TipoPortafolio.Nombre == "Proyecto"))
                    throw new Exception("No se puede crear otro portafolio de tipo proyecto porque solo puede existir uno");

                port.Nombre = Util.UppercaseFirst(Nombre);
                port.Descripcion = Descripcion;
                port.FechaEntrega = FechaEntrega;
      
                port.FechaActualizacion = DateTime.Now;
                port.FechaSync = DateTime.Now;
                port.Activo1 = Activo1;
                port.Aspecto1 = Aspecto1;
                port.Criterio1 = Criterio1;
                port.Activo2 = Activo2;
                port.Aspecto2 = Aspecto2;
                port.Criterio2 = Criterio2;
                port.Activo3 = Activo3;
                port.Aspecto3 = Aspecto3;
                port.Criterio3 = Criterio3;
                port.Activo4 = Activo4;
                port.Aspecto4 = Aspecto4;
                port.Criterio4 = Criterio4;
                port.Activo5 = Activo5;
                port.Aspecto5 = Aspecto5;
                port.Criterio5 = Criterio5;
                port.IDTipoPortafolio = IDTipoPortafolio;

                db.SaveChanges();

                result = new PortafolioSimple()
                {
                    IDPortafolio = port.IDPortafolio,
                    Fecha = port.FechaEntrega.Value.ToString("dd-MM-yyyy"),
                    FechaEntrega = port.FechaEntrega.Value.ToString("yyyy-MM-dd"),
                    observacion = port.Descripcion,
                    Descripcion = port.Descripcion,
                    TipoTrabajo = port.TipoPortafolio != null ? port.TipoPortafolio.Nombre : "",
                    IDTipoPortafolio = port.IDTipoPortafolio,
                    Nombre = port.Nombre,
                    Aspecto1 = port.Aspecto1,
                    Aspecto2 = port.Aspecto2,
                    Aspecto3 = port.Aspecto3,
                    Aspecto4 = port.Aspecto4,
                    Aspecto5 = port.Aspecto5,

                    Criterio1 = port.Criterio1,
                    Criterio2 = port.Criterio2,
                    Criterio3 = port.Criterio3,
                    Criterio4 = port.Criterio4,
                    Criterio5 = port.Criterio5,

                    Activo1 = port.Activo1,
                    Activo2 = port.Activo2,
                    Activo3 = port.Activo3,
                    Activo4 = port.Activo4,
                    Activo5 = port.Activo5,

                    Observacion1 = port.Observacion1,
                    Observacion2 = port.Observacion2,
                    Observacion3 = port.Observacion3,
                    Observacion4 = port.Observacion4,
                    Observacion5 = port.Observacion5,

                    entrega = port.PortafolioAlumno.Select(i => new EntregaPortafolio {
                        id = i.IDAlumno,
                        estado = i.Estado,
                        Aspecto1 = i.Aspecto1,
                        Aspecto2 = i.Aspecto2,
                        Aspecto3 = i.Aspecto3,
                        Aspecto4 = i.Aspecto4,
                        Aspecto5 = i.Aspecto5,
                    })
                    .ToList()
                };

                return result;
            }
        }

        public static void eliminar(Guid id, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                PortafolioSimple result = new PortafolioSimple();

                Portafolio port = db.Portafolio.FirstOrDefault(i => i.IDPortafolio == id && i.Grupos.IDUsuario == usuario);

                if (port == null)
                    throw new Exception("No existe este portafolio");

                port.PortafolioAlumno.Clear();
                db.Portafolio.Remove(port);

                db.SaveChanges();

            }
        }

        public static void actualizarObservacion(Guid id, string aspecto, string observacion, string usuario)
        {
            SMTDevEntities db = new SMTDevEntities();


            Portafolio port = db.Portafolio.FirstOrDefault(i => i.IDPortafolio == id && i.Grupos.IDUsuario == usuario);

            if (port == null)
                throw new Exception("No existe este portafolio");

            switch (aspecto)
            {
                case "Aspecto1":
                    port.Observacion1 = observacion;
                    break;
                case "Aspecto2":
                    port.Observacion2 = observacion;
                    break;
                case "Aspecto3":
                    port.Observacion3 = observacion;
                    break;
                case "Aspecto4":
                    port.Observacion4 = observacion;
                    break;
                case "Aspecto5":
                    port.Observacion5 = observacion;
                    break;
            }

            port.FechaActualizacion = DateTime.Now;
            port.FechaSync = DateTime.Now;
            db.SaveChanges();

        }

        public static List<PortafolioSimple> cargarPortafolio(Guid grupo, int bimestre, string usuario, int page, int pageSize)
        {
            List<PortafolioSimple> portafolio = new List<PortafolioSimple>();

            using (SMTDevEntities db = new SMTDevEntities())
            {
                IQueryable<Portafolio> query = db.Portafolio.Where(i => i.IDGrupo == grupo && i.Bimestres.Bimestre == bimestre && i.Grupos.IDUsuario == usuario);


                portafolio = query.OrderByDescending(i => i.FechaEntrega)
                                    .Skip(page * pageSize)
                                    .Take(pageSize)
                                    .ToList()
                                    .Select(i => new PortafolioSimple()
                                    {
                                        IDPortafolio = i.IDPortafolio,
                                        Fecha = Util.toHoraMexico(i.FechaEntrega.Value).ToString("dd-MM-yyyy"),
                                        FechaEntrega = Util.toHoraMexico(i.FechaEntrega.Value).ToString("dd-MM-yyyy"),
                                        observacion = i.Descripcion,
                                        Descripcion = i.Descripcion,
                                        TipoTrabajo = i.TipoPortafolio != null ? i.TipoPortafolio.Nombre : "",
                                        IDTipoPortafolio = i.IDTipoPortafolio,
                                        Nombre = i.Nombre,
                                        Aspecto1 = i.Aspecto1,
                                        Aspecto2 = i.Aspecto2,
                                        Aspecto3 = i.Aspecto3,
                                        Aspecto4 = i.Aspecto4,
                                        Aspecto5 = i.Aspecto5,

                                        Criterio1 = i.Criterio1,
                                        Criterio2 = i.Criterio2,
                                        Criterio3 = i.Criterio3,
                                        Criterio4 = i.Criterio4,
                                        Criterio5 = i.Criterio5,

                                        Activo1 = i.Activo1,
                                        Activo2 = i.Activo2,
                                        Activo3 = i.Activo3,
                                        Activo4 = i.Activo4,
                                        Activo5 = i.Activo5,

                                        Observacion1 = i.Observacion1,
                                        Observacion2 = i.Observacion2,
                                        Observacion3 = i.Observacion3,
                                        Observacion4 = i.Observacion4,
                                        Observacion5 = i.Observacion5,

                                    })
                                    .ToList();


                foreach (PortafolioSimple s in portafolio)
                {
                    s.entrega = db.PortafolioAlumno.Where(i => i.IDPortafolio == s.IDPortafolio)
                                    .Select(i => new EntregaPortafolio()
                                    {
                                        id = i.IDAlumno,
                                        estado = i.Estado,
                                        Aspecto1 = i.Aspecto1 == null ? "0" : i.Aspecto1,
                                        Aspecto2 = i.Aspecto2 == null ? "0" : i.Aspecto2,
                                        Aspecto3 = i.Aspecto3 == null ? "0" : i.Aspecto3,
                                        Aspecto4 = i.Aspecto4 == null ? "0" : i.Aspecto4,
                                        Aspecto5 = i.Aspecto5 == null ? "0" : i.Aspecto5,
                                        Semaforo = i.Alumno.AlumnoDesempenio.Where(d => d.IDGrupo == grupo && d.Bimestre == bimestre).Select(d => d.ColorDiagnostico).FirstOrDefault()
                                    })
                                    .ToList();
                }

            }

            return portafolio;
        }


        public static Portafolio getPortafolio(Guid ID)
        {
            var db = new SMTDevEntities();
            return db.Portafolio.Where(i => i.IDPortafolio == ID).FirstOrDefault();
        }

        public class PortafolioSimple
        {
            public Guid IDPortafolio { get; set; }
            public Guid? IDTipoPortafolio { get; set; }
            public string Descripcion { get; set; }
            public string FechaEntrega { get; set; }

            public string Fecha { get; set; }
            public string TipoTrabajo { get; set; }
            public string Tipo { get; set; }
            public List<EntregaPortafolio> entrega { get; set; }
            public string observacion { get; set; }
            public string Nombre { get; set; }
            public string Aspecto1 { get; set; }
            public string Aspecto2 { get; set; }
            public string Aspecto3 { get; set; }
            public string Aspecto4 { get; set; }
            public string Aspecto5 { get; set; }
            public string Criterio1 { get; set; }
            public string Criterio2 { get; set; }
            public string Criterio3 { get; set; }
            public string Criterio4 { get; set; }
            public string Criterio5 { get; set; }
            public bool Activo1 { get; set; }
            public bool Activo2 { get; set; }
            public bool Activo3 { get; set; }
            public bool Activo4 { get; set; }
            public bool Activo5 { get; set; }

            public string Observacion1 { get; set; }
            public string Observacion2 { get; set; }
            public string Observacion3 { get; set; }
            public string Observacion4 { get; set; }
            public string Observacion5 { get; set; }


        }

        public class EntregaPortafolio
        {
            public Guid? id { get; set; }
            public int? estado { get; set; }
            public string Aspecto1 { get; set; }
            public string Aspecto2 { get; set; }
            public string Aspecto3 { get; set; }
            public string Aspecto4 { get; set; }
            public string Aspecto5 { get; set; }
            public string Semaforo { get; set; }
        }

        public class ImpresionHeaderPortafolio
        {
            public List<Portafolio> portafolios { get; set; }
            public string tipo { get; set; }
            public Guid alumno { get; set; }

            public ImpresionHeaderPortafolio() { }


            public ImpresionHeaderPortafolio(List<Portafolio> portafolios, string tipo)
            {
                this.portafolios = portafolios;
                this.tipo = tipo;
            }

            public ImpresionHeaderPortafolio(Portafolio portafolio, string tipo)
            {
                this.portafolios = new List<Portafolio>() { portafolio};
                this.tipo = tipo;
            }

            public ImpresionHeaderPortafolio(List<Portafolio> portafolios, string tipo, Guid alumno)
            {
                this.portafolios = portafolios;
                this.tipo = tipo;
                this.alumno = alumno;
            }

            public ImpresionHeaderPortafolio(Portafolio portafolio, string tipo, Guid alumno)
            {
                this.portafolios = new List<Portafolio>() { portafolio };
                this.tipo = tipo;
                this.alumno = alumno;
            }
        }

    }
}