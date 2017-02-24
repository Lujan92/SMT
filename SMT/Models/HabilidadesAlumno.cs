using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models.DB
{
    public partial class HabilidadesAlumno
    {
        public static List<HabilidadesAlumnoSimple> cargarHabilidades(Guid grupo, int bimestre, string usuario, int page, int pageSize)
        {
            List<HabilidadesAlumnoSimple> habilidades = new List<HabilidadesAlumnoSimple>();

            using (SMTDevEntities db = new SMTDevEntities()) {
                habilidades = db.Alumno.Where(i => i.IDGrupo == grupo && i.Grupos.IDUsuario == usuario)
                    .OrderBy(i=>i.IDAlumno)
                    .ToList()
                    .Select(i => {
                        var hab = i.HabilidadesAlumno.Where(ha => ha.Bimestres.Bimestre == bimestre).FirstOrDefault();
                        return new HabilidadesAlumnoSimple {
                            id = hab != null ? hab.IDHabilidadAlumno : Guid.NewGuid(),
                            IDAlumno = i.IDAlumno,
                            Alumno = i.Nombre + " " + i.ApellidoPaterno + " " + i.ApellidoMaterno,
                            IDGrupo = i.IDGrupo,
                            Comprension = hab != null ? hab.Comprension : "",
                            Autoevaluacin = hab != null ? hab.Autoevaluacion : null,
                            Coevaluacion = hab != null ? hab.Coevaluacion : null,
                            Conocimiento = hab != null ? hab.Conocimiento : null,
                            Sintesis = hab != null ? hab.Sintesis : null,
                            Argumentacion = hab != null ? hab.Argumentacion : null,
                            ApoyoLectura = hab != null ? hab.ApoyoLectura : null,
                            ApoyoEscritura = hab != null ? hab.ApoyoEscritura : null,
                            ApoyoMatematicas = hab != null ? hab.ApoyoMatematicas : null,
                            SeInvolucraClase = hab != null ? hab.SeInvolucraClase : null,
                        };
                    })        
                    .OrderBy(i=>i.Alumno)                            
                    .ToList();

                foreach (HabilidadesAlumnoSimple s in habilidades)
                {
                    s.entrega = db.HabilidadesAlumno.Where(i => i.IDAlumno == s.id && i.IDGrupo==grupo && i.Bimestres.Bimestre==bimestre)
                        .Select(i => new Habilidad {
                            id = i.IDHabilidadAlumno,
                            IDAlumno = i.IDAlumno,
                            grupo = i.IDGrupo,                                        
                            Autoevaluacin = i.Autoevaluacion,
                            Coevaluacion = i.Coevaluacion,
                            Conocimiento = i.Conocimiento,
                            Sintesis = i.Sintesis,
                            Argumentacion = i.Argumentacion,
                            ApoyoLectura = i.ApoyoLectura,
                            ApoyoEscritura = i.ApoyoEscritura,
                            ApoyoMatematicas = i.ApoyoMatematicas,
                            SeInvolucraClase = i.SeInvolucraClase,
                        })
                        .ToList();
                }
            }

            return habilidades;
        }

        public Guid crear()
        {
            var db = new SMTDevEntities();

            IDHabilidadAlumno = Guid.NewGuid();
            db.HabilidadesAlumno.Add(this);
            db.SaveChanges();

            return IDHabilidadAlumno;
        }

        public static void actualizarEstado(Guid alumno,int habilidad,int bimestre,string estado, string usuario, Guid grupo)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                HabilidadesAlumno aluse = db.HabilidadesAlumno.FirstOrDefault(i => i.IDAlumno == alumno && i.IDGrupo==grupo && i.Bimestres.Bimestre==bimestre);

                if (aluse == null) {
                    aluse = new DB.HabilidadesAlumno {
                        IDHabilidadAlumno = Guid.NewGuid(),
                        IDAlumno = alumno,
                        IDGrupo = grupo,
                        IDBimestre = db.Bimestres.Where(i=>i.IDGrupo==grupo && i.Bimestre==bimestre).FirstOrDefault().IDBimestre,                       
                    };

                    switch (habilidad)
                    {
                        case 1:
                            aluse.SeInvolucraClase = estado=="Si"?true:false;
                            break;
                        case 2:
                            aluse.ApoyoMatematicas = Convert.ToInt32(estado);
                            break;
                        case 3:
                            aluse.ApoyoEscritura = Convert.ToInt32(estado);
                            break;
                        case 4:
                            aluse.ApoyoLectura = Convert.ToInt32(estado);
                            break;
                        case 5:
                            aluse.Argumentacion = estado;
                            break;
                        case 6:
                            aluse.Sintesis = estado;
                            break;
                        case 7:
                            aluse.Conocimiento = estado;
                            break;                                                
                        case 8:
                            aluse.Coevaluacion = Convert.ToInt32(estado);
                            break;
                        case 9:
                            aluse.Autoevaluacion = Convert.ToInt32(estado);
                            break;
                    }

                    db.HabilidadesAlumno.Add(aluse);
                    db.SaveChanges();

                }
                else
                {
                    switch (habilidad)
                    {
                        case 1:
                            aluse.SeInvolucraClase = estado == "Si" ? true : false;
                            break;
                        case 2:
                            aluse.ApoyoMatematicas = Convert.ToInt32(estado);
                            break;
                        case 3:
                            aluse.ApoyoEscritura = Convert.ToInt32(estado);
                            break;
                        case 4:
                            aluse.ApoyoLectura = Convert.ToInt32(estado);
                            break;
                        case 5:
                            aluse.Argumentacion = estado;
                            break;
                        case 6:
                            aluse.Sintesis = estado;
                            break;
                        case 7:
                            aluse.Conocimiento = estado;
                            break;                        
                        case 8:
                            aluse.Coevaluacion = Convert.ToInt32(estado);
                            break;
                        case 9:
                            aluse.Autoevaluacion = Convert.ToInt32(estado);
                            break;
                    }
                    db.SaveChanges();
                }

                AlumnoDesempenio.actualizarAlumno(alumno, grupo, bimestre, new { habilidad = true });

            }
        }
    }

    public class HabilidadesAlumnoSimple
    {
        public Guid id { get; set; }
        public Guid? IDAlumno { get; set; }
        public Guid? IDGrupo { get; set; }
        public string Comprension { get; set; }
        public string Alumno { get; set; }
        public int? Autoevaluacin { get; set; }
        public int? Coevaluacion { get; set; }
        public string Conocimiento { get; set; }
        public string Sintesis { get; set; }
        public string Argumentacion { get; set; }
        public int? ApoyoLectura { get; set; }
        public int? ApoyoEscritura { get; set; }
        public int? ApoyoMatematicas { get; set; }
        public bool? SeInvolucraClase { get; set; }
        public List<Habilidad> entrega { get; set; }

    }

    public class Habilidad
    {
        public Guid id { get; set; }
        public Guid? IDAlumno { get; set; }
        public Guid? grupo { get; set; }
        public string Comprension { get; set; }
        public int? Autoevaluacin { get; set; }
        public int? Coevaluacion { get; set; }
        public string Conocimiento { get; set; }
        public string Sintesis { get; set; }
        public string Argumentacion { get; set; }
        public int? ApoyoLectura { get; set; }
        public int? ApoyoEscritura { get; set; }
        public int? ApoyoMatematicas { get; set; }
        public bool? SeInvolucraClase { get; set; }
        public List<HabilidadesAlumno> entrega { get; set; }

    }


}