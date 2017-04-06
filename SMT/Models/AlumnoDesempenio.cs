using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SMT.Models.DB
{
    public partial class AlumnoDesempenio
    {

        /// <summary>
        /// Actualizar promedios de un alumno en el bimestre de un grupo
        /// </summary>
        /// <param name="id">ID del alumno</param>
        /// <param name="grupo">ID de grupo</param>
        /// <param name="bimestre">Numero del bimestre</param>
        /// <param name="actualizar">Variables para saber que se va  actualizar, si pasan null se actualiza todo</param>
        /// <param name="otraTarea">Ejecuta el actualizado en otro hilo para no esperar respuesta</param>
        /// 
       
        public static void actualizarAlumno(Guid id,Guid grupo, int bimestre, dynamic actualizar, bool otraTarea = true)
        {
            if(otraTarea == true)
            {
                Task.Factory.StartNew(() =>
                {
                    actualizarAlumno(id,grupo,bimestre,actualizar);
                });
            }
            else
            {
                actualizarAlumno(id, grupo, bimestre, actualizar);
            }
            
        }

        /// <summary>
        /// Actualizar promedios de un alumno en el bimestre de un grupo
        /// </summary>
        /// <param name="id">ID del alumno</param>
        /// <param name="grupo">ID de grupo</param>
        /// <param name="bimestre">Numero del bimestre</param>
        /// <param name="actualizar">Variables para saber que se va  actualizar, si pasan null se actualiza todo</param>
        private static void actualizarAlumno(Guid id, Guid grupo,int bimestre, dynamic actualizar)
        {

            try {
                using (var db = new SMTDevEntities()) {
                    var colores = new [] {
                        AlumnoDesempenioStatus.BIEN, // Verde
                        AlumnoDesempenioStatus.REGULAR, // Amarillo
                        AlumnoDesempenioStatus.APOYO  // Rojo
                    };

                    var alumno = db.Alumno.FirstOrDefault(i => i.IDAlumno == id && i.IDGrupo == grupo);
                    if (alumno == null) return;

                    var desempenio = db.AlumnoDesempenio.FirstOrDefault(i => i.IDAlumno == id && i.IDGrupo == grupo && i.Bimestre == bimestre);
                    if (desempenio == null) {
                        desempenio = new AlumnoDesempenio {
                            IDAlumno = id,
                            IDGrupo = grupo,
                            Bimestre = bimestre,
                            IDUsuario = alumno.Grupos.IDUsuario
                        };

                        db.AlumnoDesempenio.Add(desempenio);
                        db.SaveChanges();
                    }





                    #region Asistencias
                    if (actualizar == null || actualizar.GetType().GetProperty("asistencia") != null)
                    {
                        var asistencias = db.AlumnoSesion
                            .Where(i => i.IDAlumno == id && i.Sesion.Bimestres.Bimestre == bimestre)
                            .Select(i => i.Estado).ToList();
                        var faltas = asistencias.Count(i => i == 0) + (asistencias.Count(i => i == 2) / 2.0);
                        desempenio.TotalAsistencias = asistencias.Count - (int)Math.Round(faltas);
                        desempenio.TotalFaltas = (int)Math.Round(faltas);
                        var falta = desempenio.TotalFaltas;
                        var asistencia = desempenio.TotalAsistencias;
                        var totalAsistencias = faltas + asistencia;
                        desempenio.PromedioAsistencia = (asistencia/totalAsistencias)*100;
                        desempenio.ColorAsistencia =
                            faltas <= 2 ? colores[0] :
                            faltas <= 4 ? colores[1] :
                            colores[2];
                    }
                    #endregion

                    #region Trabajos
                    if (actualizar == null || actualizar.GetType().GetProperty("trabajo") != null)
                    {
                        var trabajos = db.TrabajoAlumno
                            .Where(a => a.IDAlumno == id && a.Trabajo.Bimestres.Bimestre == bimestre)
                            .Select(a => a.Estado).ToList();

                        desempenio.TotalTrabajosNoCumplidos = trabajos.Count(t => t == 0);
                        desempenio.TotalTrabajosCumplidos = trabajos.Count(t => t == 1);
                        desempenio.TotalTrabajosMedios = trabajos.Count(t => t == 2);

                        var total = desempenio.TotalTrabajosCumplidos.Value + (desempenio.TotalTrabajosMedios.Value / 2);
                        desempenio.PromedioTrabajo = trabajos.Count == 0 ? 0 : (total * 100.0) / trabajos.Count;
                        desempenio.ColorTrabajo =
                            desempenio.PromedioTrabajo <= 60 ? colores[2] :
                            desempenio.PromedioTrabajo < 90 ? colores[1] : colores[0];
                    }
                    #endregion

                    #region Portafolio
                    if (actualizar == null || actualizar.GetType().GetProperty("portafolio") != null)
                    {
                        var portafolios = db.PortafolioAlumno
                            .Where(a => a.IDAlumno == id && a.Portafolio.Bimestres.Bimestre == bimestre)
                            .ToList();

                        double sumatoria = 0, sumTipo = 0, total = 0,
                               sumEsquema = 0, sumExpo = 0, sumGuia = 0,
                               sumLista = 0, sumLinea = 0, sumMapa = 0,
                               sumPortafolio = 0, sumProyecto = 0, sumProducciones = 0,
                               sumRegistro = 0, sumRubrica = 0, sumRevision = 0,
                               totalEsquema = 0, totalExpo = 0, totalGuia = 0, totalLista = 0,
                               totalLinea = 0, totalMapa = 0, totalPortafolio = 0,
                               totalProyecto = 0, totalProducciones = 0, totalRegistro = 0,
                               totalRubrica = 0, totalRevision = 0;
                        
                        foreach (var p in portafolios)
                        {
                            int cali;
                            int totalTipo = 0;
                            sumTipo = 0;

                            if (p.Aspecto1 != null && p.Aspecto1 != "0" && int.TryParse(p.Aspecto1, out cali))
                            {

                                total++;
                                sumatoria += cali;
                                sumTipo += cali;
                                totalTipo++;
                            }
                            if (p.Aspecto2 != null && p.Aspecto2 != "0" && int.TryParse(p.Aspecto2, out cali))
                            {
                                total++;
                                sumatoria += cali;
                                sumTipo += cali;
                                totalTipo++;
                            }
                            if (p.Aspecto3 != null && p.Aspecto3 != "0" && int.TryParse(p.Aspecto3, out cali))
                            {
                                total++;
                                sumatoria += cali;
                                sumTipo += cali;
                                totalTipo++;
                            }
                            if (p.Aspecto4 != null && p.Aspecto4 != "0" && int.TryParse(p.Aspecto4, out cali))
                            {
                                total++;
                                sumatoria += cali;
                                sumTipo += cali;
                                totalTipo++;
                            }
                            if (p.Aspecto5 != null && p.Aspecto5 != "0" && int.TryParse(p.Aspecto5, out cali))
                            {
                                total++;
                                sumatoria += cali;
                                sumTipo += cali;
                                totalTipo++;
                            }

                            totalTipo = totalTipo == 0 ? 1 : totalTipo;
                            var name = p.Portafolio.TipoPortafolio.Nombre;
                            switch (p.Portafolio.TipoPortafolio.Nombre)
                            {
                                case "Esquema y Mapas Conceptuales":
                                    sumEsquema += sumTipo / totalTipo;
                                    totalEsquema++;
                                    break;
                                case "Exposición":
                                    sumExpo += sumTipo / totalTipo;
                                    totalExpo++;
                                    break;
                                case "Guía de Observación":
                                    sumGuia += sumTipo / totalTipo;
                                    totalGuia++;
                                    break;
                                case "Lista de Cotejo o Control":
                                    sumLista += sumTipo / totalTipo;
                                    totalLista++;
                                    break;
                                case "Línea de tiempo":
                                    sumLinea += sumTipo / totalTipo;
                                    totalLinea++;
                                    break;
                                case "Mapa de Aprendizaje":
                                    sumMapa += sumTipo / totalTipo;
                                    totalMapa++;
                                    break;
                                case "Portafolio":
                                    sumPortafolio += sumTipo / totalTipo;
                                    totalPortafolio++;
                                    break;
                                case "Producciones Escritas o Gráficas":
                                    sumProducciones += sumTipo / totalTipo;
                                    totalProducciones++;
                                    break;
                                case "Proyecto":
                                    sumProyecto += sumTipo / totalTipo;
                                    totalProyecto++;
                                    break;
                                case "Registro Anecdótico o Anecdotario":
                                    sumRegistro += sumTipo / totalTipo;
                                    totalRegistro++;
                                    break;
                                case "Revisión de Cuadernos":
                                    sumRevision += sumTipo / totalTipo;
                                    totalRevision++;
                                    break;
                                case "Rúbrica o Matriz de Verificación":
                                    sumRubrica += sumTipo / totalTipo;
                                    totalRubrica++;
                                    break;
                            }
                         }
                      /*
                        var exposicion = 0.0;
                        var mapaMental = 0.0;
                        var reactivos = 0;
                        var totalExamen = 0.0;
                        var conExamen = 0;
                        var conMental = 0;

                        foreach (var p in portafolios) {
                            var boli = p.Portafolio.Activo1;
                            if (p.Portafolio.TipoPortafolio.Nombre == "Exposición") {
                                if (p.Portafolio.Activo1 == true) {
                                    exposicion += Double.Parse(p.Aspecto1);
                                    reactivos++;
                                }
                                if (p.Portafolio.Activo2 == true)
                                {
                                    exposicion += Double.Parse(p.Aspecto2);
                                    reactivos++;
                                }
                                if (p.Portafolio.Activo3 == true)
                                {
                                    exposicion += Double.Parse(p.Aspecto3);
                                    reactivos++;
                                }
                                if (p.Portafolio.Activo4 == true)
                                {
                                    exposicion += Double.Parse(p.Aspecto4);
                                    reactivos++;
                                }
                                if (p.Portafolio.Activo5 == true)
                                {
                                    exposicion += Double.Parse(p.Aspecto5);
                                    reactivos++;
                                }
                                conExamen++;
                                totalExamen = ((exposicion/(3*reactivos))*10)/conExamen;
                                reactivos = 0;

                            }
                            if (p.Portafolio.TipoPortafolio.Nombre == "Mapa Mental")
                            {
                                conMental++;
                                mapaMental += (int.Parse(p.Aspecto1) + int.Parse(p.Aspecto2) + int.Parse(p.Aspecto3) +
                                    int.Parse(p.Aspecto4) + int.Parse(p.Aspecto5)) / conMental;
                            }

                        }
                        */
                      //  desempenio.PromedioPortafolio = total == 0 ? 0 : sumatoria / total;
                        desempenio.PromedioPortafolioEsquemaMapa = totalEsquema == 0 ? 0 : sumEsquema / totalEsquema;
                        desempenio.PromedioPortafolioExposicion = totalExpo == 0 ? 0 : sumExpo / totalExpo;
                        desempenio.PromedioPortafolioGuiaObservacion = totalGuia == 0 ? 0 : sumGuia / totalGuia;
                        desempenio.PromedioPortafolioLineaTiempo = totalLinea == 0 ? 0 : sumLinea / totalLinea;
                        desempenio.PromedioPortafolioListaCotejo = totalLista == 0 ? 0 : sumLista / totalLista;
                        desempenio.PromedioPortafolioMapaAprendisaje = totalMapa == 0 ? 0 : sumMapa / totalMapa;
                        desempenio.PromedioPortafolioPortafolio = totalPortafolio == 0 ? 0 : sumPortafolio / totalPortafolio;
                        desempenio.PromedioPortafolioProducciones = totalProducciones == 0 ? 0 : sumProducciones / totalProducciones;
                        desempenio.PromedioPortafolioProyecto = totalProyecto == 0 ? 0 : sumProyecto / totalProyecto;
                        desempenio.PromedioPortafolioRegistroAnecdotico = totalRegistro == 0 ? 0 : sumRegistro / totalRegistro;
                        desempenio.PromedioPortafolioRevisionCuadernos = totalRevision == 0 ? 0 : sumRevision / totalRevision;
                        desempenio.PromedioPortafolioRubrica = totalRubrica == 0 ? 0 : sumRubrica / totalRubrica;
                      /*  desempenio.ColorPortafolio =
                            desempenio.PromedioPortafolio <= 6 ? colores[2] :
                            desempenio.PromedioPortafolio < 9 ? colores[1] : colores[0];*/
                    }
                    #endregion 

                    #region Examenes
                    if (actualizar == null || actualizar.GetType().GetProperty("examen") != null)
                    {
                        var examenes = db.ExamenAlumno
                            .Where(a => a.IDAlumno == id && a.ExamenTema.Examen.Bimestres.Bimestre == bimestre)
                            .ToList();
                        int reactivos = 0, reactivosParcial = 0, reactivosBimestral = 0,
                            reactivosDiagnostico = 0, reactivosRecuperacion = 0,
                            totalParcial = 0, totalBimestral = 0, totalRecuperacion = 0,
                            totalDiagnostico = 0;
                        double sumatoria = 0, sumParcial = 0, sumBimestral = 0, sumDiagnostico = 0, sumrecuperacion = 0;

                        foreach (var e in examenes)
                        {
                            double calif = 0;

                            if (e.Calificacion != null && !double.IsNaN(e.Calificacion.Value))
                            {
                                sumatoria += e.Calificacion.Value;
                                calif = e.Calificacion.Value;
                            }

                            reactivos += e.ExamenTema.Reactivos;

                            switch (e.ExamenTema.Examen.Tipo)
                            {
                                case "Parcial":
                                    sumParcial += calif;
                                    reactivosParcial += e.ExamenTema.Reactivos;
                                    totalParcial++;
                                    break;
                                case "Bimestral":
                                    sumBimestral += calif;
                                    reactivosBimestral += e.ExamenTema.Reactivos;
                                    totalBimestral++;
                                    break;
                                case "Recuperación":
                                    sumrecuperacion += calif;
                                    reactivosRecuperacion += e.ExamenTema.Reactivos;
                                    totalRecuperacion++;
                                    break;
                                case "Diagnostico":
                                    sumDiagnostico += calif;
                                    reactivosDiagnostico += e.ExamenTema.Reactivos;
                                    totalDiagnostico++;
                                    break;
                            }
                        }

                        desempenio.PromedioExamen = examenes.Count == 0 || reactivos == 0 ? 0 : ((sumatoria * 100) / reactivos);
                        desempenio.PromedioExamenBimestral = totalBimestral == 0 || reactivosBimestral == 0 ? 0 : ((sumBimestral * 100) / totalBimestral) / reactivosBimestral;
                        desempenio.PromedioExamenParcial = totalParcial == 0 || reactivosParcial == 0 ? 0 : ((sumParcial * 100) / totalParcial) / reactivosParcial;
                        desempenio.PromedioExamenDiagnostico = totalDiagnostico == 0 || reactivosDiagnostico == 0 ? 0 : ((sumDiagnostico * 100) / totalDiagnostico) / reactivosDiagnostico;
                        desempenio.PromedioExamenRecuperacion = totalRecuperacion == 0 || reactivosRecuperacion == 0 ? 0 : ((sumrecuperacion * 100) / totalRecuperacion) / reactivosRecuperacion;

                        desempenio.ColorExamen =
                            desempenio.PromedioExamen <= 60 ? colores[2] :
                            desempenio.PromedioExamen < 90 ? colores[1] : colores[0];
                    }
                    #endregion

                    #region Diagnosticos
                    if (actualizar == null || actualizar.GetType().GetProperty("diagnostico") != null)
                    {
                        var examenes = db.DiagnosticoCicloAlumno.Where(a => a.IDAlumno == id && a.DiagnosticoCicloTema.DiagnosticoCiclo.Bimestres.Bimestre == bimestre).ToList();
                        int reactivos = 0;
                        double sumatoria = 0;

                        foreach (var t in examenes)
                        {
                            if (t.Calificacion != null)
                                sumatoria += t.Calificacion.Value;
                            reactivos += t.DiagnosticoCicloTema.Reactivos;
                        }

                        desempenio.PromedioDiagnostico = examenes.Count == 0 || reactivos == 0 ? 0 : ((sumatoria * 100) / reactivos);

                        desempenio.ColorDiagnostico =
                            desempenio.PromedioDiagnostico <= 60 ? colores[2] :
                            desempenio.PromedioDiagnostico < 90 ? colores[1] : colores[0];
                    }
                    #endregion

                    #region Habilidad
                    if (actualizar == null || actualizar.GetType().GetProperty("habilidad") != null)
                    {
                        var habilidades = db.HabilidadesAlumno.Where(a => a.IDAlumno == id && a.Bimestres.Bimestre == bimestre).ToList();
                        desempenio.PromedioHabilidadAutoevaluacion = desempenio.PromedioHabilidadCoevaluacion = 0;
                        foreach (var t in habilidades)
                        {
                            if (t.Autoevaluacion != null)
                                desempenio.PromedioHabilidadAutoevaluacion += t.Autoevaluacion.Value;
                            if (t.Coevaluacion != null)
                                desempenio.PromedioHabilidadCoevaluacion += t.Coevaluacion.Value;
                        }

                        desempenio.PromedioHabilidadAutoevaluacion = habilidades.Count == 0 ? 0 : desempenio.PromedioHabilidadAutoevaluacion / habilidades.Count;
                        desempenio.PromedioHabilidadCoevaluacion = habilidades.Count == 0 ? 0 : desempenio.PromedioHabilidadCoevaluacion / habilidades.Count;
                    }
                    #endregion









                    // Calcular promedios de todos los bimestres
                    List<AlumnoDesempenio> todos = db.AlumnoDesempenio.Where(i => i.IDAlumno == id && i.IDGrupo == grupo).ToList();

                    alumno.PromedioBimestre1 = 
                        alumno.PromedioBimestre2 = 
                        alumno.PromedioBimestre3 = 
                        alumno.PromedioBimestre4 = 
                        alumno.PromedioBimestre5 = 0;

                    int totalBimestre = 0;
               
                    Func<double?, bool> exists = n => n.HasValue && !double.IsNaN(n.Value);
                    Func<double, double> normalize = n => {
                        var val = (double.IsNaN(n) ? 0 : n);
                        return val > 10 ? val / 10.0 : val;
                    };

                    foreach (var m in todos)
                    {
                        double sumatoria = new [] {
                            m.PromedioExamen, m.PromedioTrabajo, m.PromedioPortafolio, m.PromedioDiagnostico }.Sum(o => normalize(o ?? 0));
                   
                        int total = new [] {
                            m.PromedioExamen, m.PromedioTrabajo, m.PromedioPortafolio, m.PromedioDiagnostico }.Sum(o => exists(o) ? 1 : 0);

                        double promedio = sumatoria / 4;
                      
                        switch (m.Bimestre)
                        {
                            case 1:
                                alumno.PromedioBimestre1 += promedio;
                                break;
                            case 2:
                                alumno.PromedioBimestre2 += promedio;
                                break;
                            case 3:
                                alumno.PromedioBimestre3 += promedio;
                                break;
                            case 4:
                                alumno.PromedioBimestre4 += promedio;
                                break;
                            case 5:
                                alumno.PromedioBimestre5 += promedio;
                                break;

                        }

                        if (total > 0)
                            totalBimestre++;
                    }

                    alumno.PromedioTotal = ((alumno.PromedioBimestre1 ?? 0) +
                                            (alumno.PromedioBimestre2 ?? 0) +
                                            (alumno.PromedioBimestre3 ?? 0) +
                                            (alumno.PromedioBimestre4 ?? 0) +
                                            (alumno.PromedioBimestre5 ?? 0)) / (totalBimestre == 0 ? 1 : totalBimestre);

                    if (alumno.PromedioTotal <= 6)
                        alumno.ColorPromedio = colores[2];
                    else if (alumno.PromedioTotal < 9)
                        alumno.ColorPromedio = colores[1];
                    else
                        alumno.ColorPromedio = colores[0];

                    db.SaveChanges();
                }
            }
            catch(Exception e)
            {

            }
        }

        internal static void actualizarAlumno()
        {
            throw new NotImplementedException();
        }

       
        public static void actualizarSemaforo(Guid alumno, Guid grupo, int bimestre, int aprobado, int reprobado)
        {
            var colores = new[] {
                        AlumnoDesempenioStatus.BIEN, // Verde
                        AlumnoDesempenioStatus.REGULAR, // Amarillo
                        AlumnoDesempenioStatus.APOYO  // Rojo
                    };
            using (var db = new SMTDevEntities())
            {
                var desempenio = db.AlumnoDesempenio.FirstOrDefault(i => i.IDAlumno == alumno && i.IDGrupo == grupo && i.Bimestre == bimestre);
            }
        }
        private static void actualizarCalificacion(Guid id, Guid portafolio, Guid grupo)
        {

        }
        /// <summary>
        /// Actualiza o genera todo el desempeño de todos los alumnos de cada grupo en todos sus bimestres
        /// </summary>
        public static dynamic actualizacionGeneral(Guid? grupo)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                IQueryable<Grupos> query = db.Grupos;
                if (grupo != null || grupo != default(Guid))
                    query = query.Where(a => a.IDGrupo == grupo);
                int total = 0, err = 0;
                var grupos = query.ToList()
                                       .Select(a => new
                                        {
                                            bimestres = a.Bimestres.Select(i => i.Bimestre).ToArray(),
                                            a.IDGrupo
                                        })
                                        .ToList();

                foreach(var g in grupos)
                {
                    var alumnos = db.Alumno.Where(i => i.IDGrupo == g.IDGrupo).Select(a => a.IDAlumno).ToList();

                    foreach(var b in g.bimestres)
                    {
                        foreach(var a in alumnos)
                        {
                            try {
                                actualizarAlumno(a, g.IDGrupo, b.Value, null,false);
                                total++;
                            }
                            catch
                            {
                                err++;
                            }
                        }

                    }


                }

                return new
                {
                    actualizados = total,
                    errorres = err
                };


            }
        }


        public static dynamic cargarSemaforos(Guid grupo, int bimestre, string seccion, string usuario, Guid? alumno)
        {
            using(var db = new SMTDevEntities()) {
                var query = db.AlumnoDesempenio
                    .Where(a => a.IDGrupo == grupo && a.Bimestre == bimestre && a.Grupos.IDUsuario == usuario);

                if (alumno != null)
                    query = query.Where(i => i.IDAlumno == alumno);

                var result = query
                    .Select(i => new {
                        i.IDAlumno, i.ColorAsistencia, i.ColorDiagnostico, i.ColorExamen,
                        i.ColorPortafolio, i.ColorTrabajo, i.Alumno.ColorPromedio
                    })
                    .AsEnumerable()
                    .Select(a => new {
                        id = a.IDAlumno,
                        semaforo =  seccion == "asistencia" ? a.ColorAsistencia :
                                    seccion == "trabajos" ? a.ColorTrabajo :
                                    seccion == "instrumentos" ? a.ColorPortafolio :
                                    seccion == "examenes" ? a.ColorExamen :
                                    seccion == "diagnostico-ciclo" ? a.ColorDiagnostico :
                                    a.ColorPromedio
                    })
                    .ToList();

                return result;
            }
        }

        public static List<AlumnoReporteViewModel> cargarReporte(Guid grupo,long bimestre, Guid? alumno, bool onlyBimestre = false)
        {
            using (var db = new SMTDevEntities()) {
                var query = db.AlumnoDesempenio.Where(a => a.IDGrupo == grupo && a.Bimestre == bimestre);
                if (alumno != null)
                    query = query.Where(i => i.IDAlumno == alumno);

                var alumnos = query.OrderBy(a => new { a.Alumno.ApellidoPaterno, a.Alumno.ApellidoMaterno, a.Alumno.Nombre }).ToList();
                var reporte = new List<AlumnoReporteViewModel>();
                var parciales = db.Examen
                    .Where(b => b.Tipo == "Parcial" && b.Bimestres.IDGrupo == grupo && b.Bimestres.Bimestre == bimestre && b.ExamenTema.Any())
                    .Select(b => new {
                        b.IDExamen ,
                        bimestre = b.Bimestres.Bimestre,
                        reactivos = b.ExamenTema.Sum(a => a.Reactivos)
                    })
                    .OrderBy(i => i.IDExamen)
                    .Distinct()
                    .ToList();
                var bimestrales = db.Examen
                    .Where(b => b.Tipo == "Bimestral" && b.Bimestres.IDGrupo == grupo && b.Bimestres.Bimestre == bimestre && b.ExamenTema.Any())
                    .Select(b => new {
                        b.IDExamen,
                        bimestre = b.Bimestres.Bimestre,
                        reactivos = b.ExamenTema.Sum(a => a.Reactivos)
                    })
                    .OrderBy(i => i.IDExamen)
                    .Distinct()
                    .ToList();

                var diagnosticoCiclo = db.DiagnosticoCiclo.Where(b => b.Bimestres.IDGrupo == grupo && b.Bimestres.Bimestre == bimestre && b.DiagnosticoCicloTema.Any())
                     .Select(b => new {
                         b.IDDiagnosticoCiclo,
                         bimestre = b.Bimestres.Bimestre,
                         reactivos = b.DiagnosticoCicloTema.Sum(a => a.Reactivos)
                     })
                    .OrderBy(i => i.IDDiagnosticoCiclo)
                    .Distinct()
                    .ToList();

                var portafolioInfo = db.Portafolio
                        .Where(p =>
                            p.IDGrupo == grupo &&
                            p.Bimestres.Bimestre == bimestre)
                        .SelectMany(p => p.PortafolioAlumno.Select(pa => new {
                            pa.IDAlumno, Tipo = p.TipoPortafolio.Nombre, pa.Aspecto1,
                            pa.Aspecto2, pa.Aspecto3, pa.Aspecto4, pa.Aspecto5 }))
                        .ToList();

                Func<Guid, string, List<double?>> califPortafolio = (idAlumno, tipoPortafolio) => {
                    return portafolioInfo
                        .Where(p => p.IDAlumno == idAlumno && p.Tipo == tipoPortafolio)
                        .Select(p => {
                            var cal = new [] { p.Aspecto1, p.Aspecto2, p.Aspecto3, p.Aspecto4, p.Aspecto5 }.Sum(c => Convert.ToDouble(c));
                            return (double?) (cal > 10 ? 10 : cal);
                        }).ToList();
                };

                foreach (var a in alumnos.GroupBy(a => new { a.IDAlumno }))
                {
                    
                    var cali = new AlumnoReporteViewModel {
                        id = a.Key.IDAlumno,
                        totalAsistencias = a.Sum(m => m.TotalAsistencias ?? 0),
                        totalFaltas = a.Sum(m => m.TotalFaltas ?? 0),
                        totalTrabajosCumplidos = a.Sum(m => m.TotalTrabajosCumplidos ?? 0),
                        totalTrabajoNoCumplidos = a.Sum(m => m.TotalTrabajosNoCumplidos ?? 0),
                        totalTrabajosMedios = a.Sum(m => m.TotalTrabajosMedios ?? 0),
                    };
                   
                    cali.promedioDiagnosticoCiclo = calcularPromedioSEP(a.Select(b => b.PromedioDiagnostico).ToList(), 100);
                    cali.promedioExamenBimestral = calcularPromedioSEP(a.Select(m => m.PromedioExamenBimestral).ToList());
                    cali.promedioExamenParcial = calcularPromedioSEP(a.Select(m => m.PromedioExamenParcial).ToList());

                    var oralInternet = califPortafolio(a.Key.IDAlumno, "Presentacion Oral Internet");
                    var cuadroComparativo = califPortafolio(a.Key.IDAlumno, "Cuadro Comparativo");
                    var manualidad = califPortafolio(a.Key.IDAlumno, "Manualidad");
                    var prueba = califPortafolio(a.Key.IDAlumno, "Prueba");
                    var investigacionImpresa = califPortafolio(a.Key.IDAlumno, "Investigación Impresa");
                    var exposicionInternet = califPortafolio(a.Key.IDAlumno, "Exposicion Internet");
                    var lineaTiempoInternet = califPortafolio(a.Key.IDAlumno, "Línea de Tiempo Internet");
                    var cuadroDobleEntrada = califPortafolio(a.Key.IDAlumno, "Cuadro de Doble Entrada");
                    var cuadroSinoptico = califPortafolio(a.Key.IDAlumno, "Cuadro Sinoptico");
                    var ensayo = califPortafolio(a.Key.IDAlumno, "Ensayo");
                    var glosario = califPortafolio(a.Key.IDAlumno, "Glosario");
                    var mapaConceptual = califPortafolio(a.Key.IDAlumno, "Mapa Conceptual");
                    var mapaMental = califPortafolio(a.Key.IDAlumno, "Mapa Mental");
                    var presentacionElectronica = califPortafolio(a.Key.IDAlumno, "Presentacion Electrónica");
                    var resumen = califPortafolio(a.Key.IDAlumno, "Resumen");
                    var trabajoColaborativoInternet = califPortafolio(a.Key.IDAlumno, "Trabajo Colaborativo Internet");
                    var esquema = califPortafolio(a.Key.IDAlumno, "Esquema");
                    var cartel = califPortafolio(a.Key.IDAlumno, "Cartel");
                    var triptico = califPortafolio(a.Key.IDAlumno, "Triptico");
                    var exposicion = califPortafolio(a.Key.IDAlumno, "Exposición");
                    var guiaObservacion = califPortafolio(a.Key.IDAlumno, "Guía de Observación");
                    var mapaAprendizaje = califPortafolio(a.Key.IDAlumno, "Mapa de Aprendizaje");
                    var revisionCuadernos = califPortafolio(a.Key.IDAlumno, "Revisión de Cuadernos");
                    var esquemasMapas = califPortafolio(a.Key.IDAlumno, "Esquema y Mapas Conceptuales");
                    var linea = califPortafolio(a.Key.IDAlumno, "Línea de tiempo");



                    cali.promedioInvestigacionImpresa = calcularPromedioSEP(investigacionImpresa, investigacionImpresa.Count*10);
                    cali.promedioPresentacionOralInternet = calcularPromedioSEP(oralInternet, oralInternet.Count * 10);
                    cali.promedioGuia = calcularPromedioSEP(guiaObservacion, guiaObservacion.Count * 10);
                     
                    //  cali.promedioTriptico = calcularPromedioSEP(a.Select);
                    cali.promedioExposicion = calcularPromedioSEP(exposicion,exposicion.Count * 10);
                    cali.promedioMapaAprendisaje = calcularPromedioSEP(mapaAprendizaje, mapaAprendizaje.Count * 10);
                     cali.promedioRevisionCuadernos = calcularPromedioSEP(revisionCuadernos, revisionCuadernos.Count * 10);
                    cali.promedioLineaTiempo = calcularPromedioSEP(linea, linea.Count * 10);
                    cali.promedioCuadroComparativo = calcularPromedioSEP(cuadroComparativo, cuadroComparativo.Count * 10);
                    cali.promedioManualidad = calcularPromedioSEP(manualidad, manualidad.Count * 10);
                    cali.promedioPrueba = calcularPromedioSEP(prueba, prueba.Count * 10);
                    //cali.promedioInvestigacionImpresa = calcularPromedioSEP(investigacionImpresa, investigacionImpresa.Count * 10);
                    cali.promedioExposicionInternet = calcularPromedioSEP(exposicionInternet, exposicionInternet.Count * 10);
                    cali.promedioLineaTiempoInternet = calcularPromedioSEP(lineaTiempoInternet, lineaTiempoInternet.Count * 10);
                    cali.promedioCuadroDobleEntrada = calcularPromedioSEP(cuadroDobleEntrada, cuadroDobleEntrada.Count * 10);
                    cali.promedioCuadroSinoptico = calcularPromedioSEP(cuadroSinoptico, cuadroSinoptico.Count * 10);
                    cali.promedioEnsayo = calcularPromedioSEP(ensayo, ensayo.Count * 10);
                    cali.promedioGlosario = calcularPromedioSEP(glosario, glosario.Count * 10);
                    cali.promedioMapaConceptual = calcularPromedioSEP(mapaConceptual, mapaConceptual.Count * 10);
                    cali.promedioMapaMental = calcularPromedioSEP(mapaMental, mapaMental.Count * 10);
                    cali.promedioPresentacionElectronica = calcularPromedioSEP(presentacionElectronica, presentacionElectronica.Count * 10);
                    cali.promedioResumen = calcularPromedioSEP(resumen, resumen.Count * 10);
                    cali.promedioTrabajoColaborativoInternet = calcularPromedioSEP(trabajoColaborativoInternet, trabajoColaborativoInternet.Count * 10);
                    cali.promedioEsquema = calcularPromedioSEP(esquema, esquema.Count * 10);
                    cali.promedioCartel = calcularPromedioSEP(cartel, cartel.Count * 10);
                    cali.promedioTriptico = calcularPromedioSEP(triptico, triptico.Count * 10);

                    //cali.promedioExposicion = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioExposicion).ToList(), a.Where(m => m.PromedioPortafolioExposicion != 0).Count() * 10);
                 //   cali.promedioEsquema = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioEsquemaMapa).ToList(), a.Where(m => m.PromedioPortafolioEsquemaMapa != 0).Count() * 10);
                   // cali.promedioGuia = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioGuiaObservacion).ToList(), a.Where(m => m.PromedioPortafolioGuiaObservacion != 0).Count() * 10);
                   // cali.promedioLineaTiempo = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioLineaTiempo).ToList(), a.Where(m => m.PromedioPortafolioLineaTiempo != 0).Count() * 10);
                    cali.promedioListaCotejo = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioListaCotejo).ToList(), a.Where(m => m.PromedioPortafolioListaCotejo != 0).Count() * 10);
                  //  cali.promedioMapaAprendisaje = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioMapaAprendisaje).ToList(), a.Where(m => m.PromedioPortafolioMapaAprendisaje != 0).Count() * 10);
                    cali.promedioPortafolio = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioPortafolio).ToList(), a.Where(m => m.PromedioPortafolioPortafolio != 0).Count() * 10);
                    cali.promedioProduccionesEscritas = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioProducciones).ToList(), a.Where(m => m.PromedioPortafolioProducciones != 0).Count() * 10);
                    cali.promedioProyecto = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioProyecto).ToList(), a.Where(m => m.PromedioPortafolioProyecto != 0).Count() * 10);
                    cali.promedioRegistroAnecdotico = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioRegistroAnecdotico).ToList(), a.Where(m => m.PromedioPortafolioRegistroAnecdotico != 0).Count() * 10);
                    //cali.promedioRevisionCuadernos = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioRevisionCuadernos).ToList(), a.Where(m => m.PromedioPortafolioRevisionCuadernos != 0).Count() * 10);
                    cali.promedioRubrica = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioRubrica).ToList(), a.Where(m => m.PromedioPortafolioRubrica != 0).Count() * 10);
                    cali.promedioTrabajo = calcularPromedioSEP(a.Select(m => m.PromedioTrabajo).ToList(), 100);
                    cali.promedioAutoevaluacion = calcularPromedioSEP(a.Select(m => m.PromedioHabilidadAutoevaluacion).ToList());
                    cali.promedioCoevaluacion = calcularPromedioSEP(a.Select(m => m.PromedioHabilidadCoevaluacion).ToList());

                    if (double.IsNaN(cali.promedioDiagnosticoCiclo))
                        cali.promedioDiagnosticoCiclo = 0;
                    if (double.IsNaN(cali.promedioExamenDiagnostico))
                        cali.promedioExamenDiagnostico = 0;
                    if (double.IsNaN(cali.promedioEsquema))
                        cali.promedioEsquema = 0;
                    if (double.IsNaN(cali.promedioExamenBimestral))
                        cali.promedioExamenBimestral = 0;
                    if (double.IsNaN(cali.promedioExamenParcial))
                        cali.promedioExamenParcial = 0;
                    if (double.IsNaN(cali.promedioExamenRecuperacion))
                        cali.promedioExamenRecuperacion = 0;
                    if (double.IsNaN(cali.promedioExposicion))
                        cali.promedioExposicion = 0;
                    if (double.IsNaN(cali.promedioGuia))
                        cali.promedioGuia = 0;
                    if (double.IsNaN(cali.promedioLineaTiempo))
                        cali.promedioLineaTiempo = 0;
                    if (double.IsNaN(cali.promedioMapaAprendisaje))
                        cali.promedioMapaAprendisaje = 0;
                    if (double.IsNaN(cali.promedioPortafolio))
                        cali.promedioPortafolio = 0;
                    if (double.IsNaN(cali.promedioProduccionesEscritas))
                        cali.promedioProduccionesEscritas = 0;
                    if (double.IsNaN(cali.promedioProyecto))
                        cali.promedioProyecto = 0;
                    if (double.IsNaN(cali.promedioRegistroAnecdotico))
                        cali.promedioRegistroAnecdotico = 0;
                    if (double.IsNaN(cali.promedioRevisionCuadernos))
                        cali.promedioRevisionCuadernos = 0;
                    if (double.IsNaN(cali.promedioRubrica))
                        cali.promedioRubrica = 0;
                    if (double.IsNaN(cali.promedioTrabajo))
                        cali.promedioTrabajo = 0;
                    if (double.IsNaN(cali.totalFaltas))
                        cali.totalFaltas = 0;

                    var examenes = db.ExamenAlumno.Where(x => 
                        x.ExamenTema.Examen.Bimestres.IDGrupo == grupo && 
                        x.ExamenTema.Examen.Bimestres.Bimestre == bimestre && x.IDAlumno == a.Key.IDAlumno).ToList();
                    var idEx = default(Guid);

                    for (int bim = 1; bim <= 5; bim++) {
                        var listaParciales = parciales.Where(b => b.bimestre == bim).ToList();
                        if (listaParciales.Count() > 0) {
                            idEx = listaParciales[0].IDExamen;

                            var cal = calcularPromedioSEP(
                                examenes.Where(b => b.ExamenTema.Examen.IDExamen == idEx).Select(b => b.Calificacion).ToList(),
                                listaParciales[0].reactivos);
                            
            
                            switch(bim) {
                                case 1:
                                    cali.promedioExamenParcial1Bimestre1 = cal;
                                    break;
                                case 2:
                                    cali.promedioExamenParcial1Bimestre2 = cal;
                                  
                                    break;
                                case 3:
                                    cali.promedioExamenParcial1Bimestre3 = cal;
                                    break;
                                case 4:
                                    cali.promedioExamenParcial1Bimestre4 = cal;
                                    break;
                                case 5:
                                    cali.promedioExamenParcial1Bimestre5 = cal;
                                    break;
                            }

                            cali.promedioFinal += cal;
                        }

                     
                        if (listaParciales.Count() > 1) {
                            idEx = listaParciales[1].IDExamen;
                            var cal = calcularPromedioSEP(
                                examenes.Where(b => b.ExamenTema.Examen.IDExamen == idEx).Select(b => b.Calificacion).ToList(),
                                listaParciales[1].reactivos);

                            switch (bim) {
                                case 1:
                                    cali.promedioExamenParcial2Bimestre1 = cal;
                                    break;
                                case 2:
                                    cali.promedioExamenParcial2Bimestre2 = cal;
                                    break;
                                case 3:
                                    cali.promedioExamenParcial2Bimestre3 = cal;
                                    break;
                                case 4:
                                    cali.promedioExamenParcial2Bimestre4 = cal;
                                    break;
                                case 5:
                                    cali.promedioExamenParcial2Bimestre5 = cal;
                                    break;
                            }
                            cali.promedioFinal += cal;
                        }
                            
                        if(bimestrales.Any(b => b.bimestre == bim)) {
                            var exams = examenes
                                .Where(b => b.ExamenTema.Examen.Tipo == "Bimestral" && b.ExamenTema.Examen.Bimestres.Bimestre == bim)
                                .Select(b => b.Calificacion).ToList();
                            var reactivs = bimestrales.Where(b => b.bimestre == bim).Select(b => b.reactivos).FirstOrDefault();
                            var cal = calcularPromedioSEP(exams, reactivs);

                            switch (bim) {
                                case 1:
                                    cali.promedioExamenBimestre1 = cal;
                                    break;
                                case 2:
                                    cali.promedioExamenBimestre2 = cal;
                                    break;
                                case 3:
                                    cali.promedioExamenBimestre3 = cal;
                                    break;
                                case 4:
                                    cali.promedioExamenBimestre4 = cal;
                                    break;
                                case 5:
                                    cali.promedioExamenBimestre5 = cal;
                                    break;
                            }

                            cali.promedioFinal += cal;
                        }
                    }

                    switch (bimestre) {
                        case 1:
                            cali.promedioFinal = a.Select(ad => ad.Alumno.PromedioBimestre1).FirstOrDefault() ?? 0;
                            break;
                        case 2:
                            cali.promedioFinal = a.Select(ad => ad.Alumno.PromedioBimestre2).FirstOrDefault() ?? 0;
                            break;
                        case 3:
                            cali.promedioFinal = a.Select(ad => ad.Alumno.PromedioBimestre3).FirstOrDefault() ?? 0;
                            break;
                        case 4:
                            cali.promedioFinal = a.Select(ad => ad.Alumno.PromedioBimestre4).FirstOrDefault() ?? 0;
                            break;
                        case 5:
                            cali.promedioFinal = a.Select(ad => ad.Alumno.PromedioBimestre5).FirstOrDefault() ?? 0;
                            break;
                    }

                    cali.promedioFinal = cali.promedioFinal < 5 ? 5 : cali.promedioFinal > 10 ? 10 : cali.promedioFinal;

                    reporte.Add(cali);

                }


                return reporte;
            }


        }

        public static List<AlumnoReporteViewModel> cargarReporteGeneral(Guid grupo, Guid? alumno, SMTDevEntities db = null)
        {
            db = db ?? new SMTDevEntities();
            IQueryable<AlumnoDesempenio> query = db.AlumnoDesempenio.Where(a => a.IDGrupo == grupo);
            if (alumno != null) {
                query = query.Where(i => i.IDAlumno == alumno);
            }
            List<AlumnoDesempenio> alumnos = query.OrderBy(a => new { a.Alumno.ApellidoPaterno, a.Alumno.ApellidoMaterno, a.Alumno.Nombre }).ToList();
            List<AlumnoReporteViewModel> reporte = new List<AlumnoReporteViewModel>();

            var parciales = db.Examen.Where(b => b.Tipo == "Parcial" && b.Bimestres.IDGrupo == grupo && b.ExamenTema.Any())
                                        .Select(b => new {
                                            b.IDExamen,
                                            bimestre = b.Bimestres.Bimestre,
                                            reactivos = b.ExamenTema.Select(a => a.Reactivos).Sum()
                                        })
                                        .OrderBy(i => i.IDExamen)
                                        .Distinct()
                                        .ToList();
            var bimestrales = db.Examen.Where(b => b.Tipo == "Bimestral" && b.Bimestres.IDGrupo == grupo && b.ExamenTema.Any()).Select(b => new {
                b.IDExamen,
                bimestre = b.Bimestres.Bimestre,
                reactivos = b.ExamenTema.Select(a => a.Reactivos).Sum()
            })
            .OrderBy(i => i.IDExamen)
            .Distinct()
            .ToList();

            foreach (var a in alumnos.GroupBy(a => new { a.IDAlumno }))
            {
                #region Obtener valores ya precalculados
                AlumnoReporteViewModel cali = new AlumnoReporteViewModel()
                {
                    id = a.Key.IDAlumno,
                    totalAsistencias = a.Select(m => m.TotalAsistencias == null ? 0 : m.TotalAsistencias.Value).Sum(),
                    totalFaltas = a.Select(m => m.TotalFaltas == null ? 0 : m.TotalFaltas.Value).Sum(),
                    totalTrabajosCumplidos = a.Select(m => m.TotalTrabajosCumplidos == null ? 0 : m.TotalTrabajosCumplidos.Value).Sum(),
                    totalTrabajoNoCumplidos = a.Select(m => m.TotalTrabajosNoCumplidos == null ? 0 : m.TotalTrabajosNoCumplidos.Value).Sum(),
                    totalTrabajosMedios = a.Select(m => m.TotalTrabajosMedios == null ? 0 : m.TotalTrabajosMedios.Value).Sum(),
                };
                /*
                cali.promedioDiagnosticoCiclo = calcularPromedioSEP(a.Select(m => m.PromedioDiagnostico).ToList());
                cali.promedioExamenBimestral = calcularPromedioSEP(a.Select(m => m.PromedioExamenBimestral).ToList());
                cali.promedioExamenDiagnostico = calcularPromedioSEP(a.Select(m => m.PromedioExamenDiagnostico).ToList());
                cali.promedioExamenParcial = calcularPromedioSEP(a.Select(m => m.PromedioExamenParcial).ToList());
                cali.promedioExamenRecuperacion = calcularPromedioSEP(a.Select(m => m.PromedioExamenRecuperacion).ToList());
                cali.promedioExposicion = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioExposicion).ToList(), a.Where(m => m.PromedioPortafolioExposicion != 0).Count() * 10);
                cali.promedioEsquema = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioEsquemaMapa).ToList(), a.Where(m => m.PromedioPortafolioEsquemaMapa != 0).Count() * 10);
                cali.promedioGuia = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioGuiaObservacion).ToList(), a.Where(m => m.PromedioPortafolioGuiaObservacion != 0).Count() * 10);
                cali.promedioLineaTiempo = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioLineaTiempo).ToList(), a.Where(m => m.PromedioPortafolioLineaTiempo != 0).Count() * 10);
                cali.promedioListaCotejo = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioListaCotejo).ToList(), a.Where(m => m.PromedioPortafolioListaCotejo != 0).Count() * 10);
                cali.promedioMapaAprendisaje = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioMapaAprendisaje).ToList(), a.Where(m => m.PromedioPortafolioMapaAprendisaje != 0).Count() * 10);
                cali.promedioPortafolio = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioPortafolio).ToList(), a.Where(m => m.PromedioPortafolioPortafolio != 0).Count() * 10);
                cali.promedioProduccionesEscritas = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioProducciones).ToList(), a.Where(m => m.PromedioPortafolioProducciones != 0).Count() * 10);
                cali.promedioProyecto = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioProyecto).ToList(), a.Where(m => m.PromedioPortafolioProyecto != 0).Count() * 10);
                cali.promedioRegistroAnecdotico = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioRegistroAnecdotico).ToList(), a.Where(m => m.PromedioPortafolioRegistroAnecdotico != 0).Count() * 10);
                cali.promedioRevisionCuadernos = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioRevisionCuadernos).ToList(), a.Where(m => m.PromedioPortafolioRevisionCuadernos != 0).Count() * 10);
                cali.promedioRubrica = calcularPromedioSEP(a.Select(m => m.PromedioPortafolioRubrica).ToList(), a.Where(m => m.PromedioPortafolioRubrica != 0).Count() * 10);
                cali.promedioTrabajo = calcularPromedioSEP(a.Select(m => m.PromedioTrabajo).ToList(), a.Where(m => m.PromedioTrabajo != 0).Count() * 10);
                cali.promedioAutoevaluacion = calcularPromedioSEP(a.Select(m => m.PromedioHabilidadAutoevaluacion).ToList());
                cali.promedioCoevaluacion = calcularPromedioSEP(a.Select(m => m.PromedioHabilidadCoevaluacion).ToList());
                */
                if (double.IsNaN(cali.promedioDiagnosticoCiclo))
                    cali.promedioDiagnosticoCiclo = 0;
                if (double.IsNaN(cali.promedioExamenDiagnostico))
                    cali.promedioExamenDiagnostico = 0;
                if (double.IsNaN(cali.promedioEsquema))
                    cali.promedioEsquema = 0;
                if (double.IsNaN(cali.promedioExamenBimestral))
                    cali.promedioExamenBimestral = 0;
                if (double.IsNaN(cali.promedioExamenParcial))
                    cali.promedioExamenParcial = 0;
                if (double.IsNaN(cali.promedioExamenRecuperacion))
                    cali.promedioExamenRecuperacion = 0;
                if (double.IsNaN(cali.promedioExposicion))
                    cali.promedioExposicion = 0;
                if (double.IsNaN(cali.promedioGuia))
                    cali.promedioGuia = 0;
                if (double.IsNaN(cali.promedioLineaTiempo))
                    cali.promedioLineaTiempo = 0;
                if (double.IsNaN(cali.promedioMapaAprendisaje))
                    cali.promedioMapaAprendisaje = 0;
                if (double.IsNaN(cali.promedioPortafolio))
                    cali.promedioPortafolio = 0;
                if (double.IsNaN(cali.promedioProduccionesEscritas))
                    cali.promedioProduccionesEscritas = 0;
                if (double.IsNaN(cali.promedioProyecto))
                    cali.promedioProyecto = 0;
                if (double.IsNaN(cali.promedioRegistroAnecdotico))
                    cali.promedioRegistroAnecdotico = 0;
                if (double.IsNaN(cali.promedioRevisionCuadernos))
                    cali.promedioRevisionCuadernos = 0;
                if (double.IsNaN(cali.promedioRubrica))
                    cali.promedioRubrica = 0;
                if (double.IsNaN(cali.promedioTrabajo))
                    cali.promedioTrabajo = 0;
                if (double.IsNaN(cali.totalFaltas))
                    cali.totalFaltas = 0;

                #endregion

                IQueryable <ExamenAlumno> examenes = db.ExamenAlumno.Where(x => x.ExamenTema.Examen.Bimestres.IDGrupo == grupo && x.IDAlumno == a.Key.IDAlumno);
                var id = default(Guid);
                for (int index = 1; index <= 5; index++)
                {

                    var listaParciales = parciales.Where(b => b.bimestre == index).ToList();

                    if (listaParciales.Count() > 0)
                    {
                        id = listaParciales[0].IDExamen;
                        cali.GetType().GetProperty("promedioExamenParcial1Bimestre" + index)
                                        .SetValue(cali,
                                                calcularPromedioSEP(examenes.Where(b => b.ExamenTema.Examen.IDExamen == id).Select(b => b.Calificacion).ToList(),
                                                                    listaParciales[0].reactivos));
                    }
                    if (listaParciales.Count() > 1)
                    {
                        id = listaParciales[1].IDExamen;
                        cali.GetType().GetProperty("promedioExamenParcial2Bimestre" + index)
                                        .SetValue(cali,
                                                calcularPromedioSEP(examenes.Where(b => b.ExamenTema.Examen.IDExamen == id).Select(b => b.Calificacion).ToList(),
                                                                    listaParciales[1].reactivos));
                    }

                    if (bimestrales.Any(b => b.bimestre == index))
                        cali.GetType().GetProperty("promedioExamenBimestre" + index)
                                        .SetValue(cali,
                                                    calcularPromedioSEP(examenes.Where(b => b.ExamenTema.Examen.Tipo == "Bimestral" && b.ExamenTema.Examen.Bimestres.Bimestre == index).Select(b => b.Calificacion).ToList(),
                                                                        bimestrales.Where(b => b.bimestre == index).Select(b => b.reactivos).FirstOrDefault()));


                }


                cali.promedioFinal = cali.promedioExamenBimestre1 + cali.promedioExamenBimestre2 + cali.promedioExamenBimestre3 + cali.promedioExamenBimestre4 + cali.promedioExamenBimestre5;
                int cantidadPromedios = (cali.promedioExamenBimestre1 != 0 ? 1 : 0) + (cali.promedioExamenBimestre2 != 0 ? 1 : 0) + (cali.promedioExamenBimestre3 != 0 ? 1 : 0) + (cali.promedioExamenBimestre4 != 0 ? 1 : 0) + (cali.promedioExamenBimestre5 != 0 ? 1 : 0);
                cali.promedioFinal = cali.promedioFinal / (cantidadPromedios != 0 ? cantidadPromedios : 1);


                reporte.Add(cali);

            }

            return reporte;
        }

        /// <summary>
        /// Cargar datos necesarios para generar los headers de una tabla y saber que se va a mostrar
        /// </summary>
        /// <param name="grupo"></param>
        public static List<dynamic> cargarHeadersDeReporte(Guid grupo, long bimestre, SMTDevEntities db = null)
        {
            db = db ?? new SMTDevEntities();
            List<dynamic> result = new List<dynamic>();

            // Segun el orden que se insertan en el result sera como se muestre en la tabla
            //result.Add(new
            //{
            //    key = "totalAsistencias", // Identificador en base a  la clase AlumnoReporteViewModel
            //    name = "Asistencias" // Valor a desplegar
            //});

            #region Examenes
            var examenes = db.Examen
                .Where(a => a.Bimestres.IDGrupo == grupo && a.Bimestres.Bimestre == bimestre)
                .Select(a => new { a.Bimestres.Bimestre, a.Tipo })
                .ToList();

            for (int index = 1; index <= 5; index++) {
                int parciales = examenes.Where(a => a.Bimestre == index && a.Tipo == "Parcial").Count();
                if (parciales > 0) {
                    result.Add(new {
                        key = "promedioExamenParcial1Bimestre" + index,
                        name = "B" + index + ": Parcial 1",
                        examen = true
                    });

                    if (parciales > 1)
                        result.Add(new {
                            key = "promedioExamenParcial2Bimestre" + index,
                            name = "B" + index + ": Parcial 2",
                            examen = true
                        });
                }

                if (examenes.Any(a => a.Bimestre == index && a.Tipo == "Bimestral")) {
                    result.Add(new {
                        key = "promedioExamenBimestre" + index,
                        name = "Bimestral " + index,
                        examen = true,
                        bimestral = true
                    });
                }
            }
            #endregion

            result.Add(new
            {
                key = "promedioTrabajo",
                name = "Trabajos"
            });

            #region Portafolios
            var tiposPortafolio = db.TipoPortafolio.OrderBy(a => a.Orden).Select(a => a.Nombre).ToList();
            var headersPortafolio = new Dictionary<string, PortafolioHeaderTablaControl> {
                {
                    "Esquema y Mapas Conceptuales", new PortafolioHeaderTablaControl {
                        key = "promedioEsquema",
                        name = "Esquema y Mapas Conceptuales",
                        instrumentos = true,
                    }
                }, {
                    "Exposición", new PortafolioHeaderTablaControl {
                        key = "promedioExposicion",
                        name = "Exposición",
                        instrumentos = true,
                    }
                }, {
                    "Guía de Observación", new PortafolioHeaderTablaControl {
                        key = "promedioGuia",
                        name = "Guía de Observación",
                        instrumentos = true,
                    }
                }, {
                    "Lista de Cotejo o Control", new PortafolioHeaderTablaControl {
                        key = "promedioListaCotejo",
                        name = "Lista de Cotejo o Control",
                        instrumentos = true,
                    }
                }, {
                    "Línea de tiempo", new PortafolioHeaderTablaControl {
                        key = "promedioLineaTiempo",
                        name = "Línea de tiempo",
                        instrumentos = true,
                    }
                }, {
                    "Mapa de Aprendizaje", new PortafolioHeaderTablaControl {
                        key = "promedioMapaAprendisaje",
                        name = "Mapa de Aprendizaje",
                        instrumentos = true,
                    }
                }, {
                    "Portafolio", new PortafolioHeaderTablaControl {
                        key = "promedioPortafolio",
                        name = "Portafolio",
                        instrumentos = true,
                    }
                }, {
                    "Producciones Escritas o Gráficas", new PortafolioHeaderTablaControl {
                        key = "promedioProduccionesEscritas",
                        name = "Producciones Escritas o Gráficas",
                        instrumentos = true,
                    }
                }, {
                    "Proyecto", new PortafolioHeaderTablaControl {
                        key = "promedioProyecto",
                        name = "Proyecto",
                        instrumentos = true,
                    }
                }, {
                    "Revisión de Cuadernos", new PortafolioHeaderTablaControl {
                        key = "promedioRevisionCuadernos",
                        name = "Revisión de Cuadernos",
                        instrumentos = true,
                    }
                }, {
                    "Presentacion Oral Internet", new PortafolioHeaderTablaControl {
                        key = "promedioPresentacionOralInternet",
                        name = "Presentación Oral Internet",
                        instrumentos = true,
                    }
                }, {
                    "Registro Anecdótico o Anecdotario", new PortafolioHeaderTablaControl {
                        key = "promedioRegistroAnecdotico",
                        name = "Registro Anecdótico o Anecdotario",
                        instrumentos = true,
                    }
                },
                {
                    "Cuadro Comparativo", new PortafolioHeaderTablaControl {
                        key = "promedioCuadroComparativo",
                        name = "Cuadro Comparativo",
                        instrumentos = true,
                    }
                },
                {
                    "Manualidad", new PortafolioHeaderTablaControl {
                        key = "promedioManualidad",
                        name = "Manualidad",
                        instrumentos = true,
                    }
                },
                {
                    "Prueba", new PortafolioHeaderTablaControl {
                        key = "promedioPrueba",
                        name = "Prueba",
                        instrumentos = true,
                    }
                },
                {
                    "Investigación Impresa", new PortafolioHeaderTablaControl {
                        key = "promedioInvestigacionImpresa",
                        name = "Investigación Impresa",
                        instrumentos = true,
                    }
                },
                {
                    "Exposicion Internet", new PortafolioHeaderTablaControl {
                        key = "promedioExposicionInternet",
                        name = "Exposicion Internet",
                        instrumentos = true,
                    }
                },
                {
                    "Línea de Tiempo Internet", new PortafolioHeaderTablaControl {
                        key = "promedioLineaTiempoInternet",
                        name = "Línea de Tiempo Internet",
                        instrumentos = true,
                    }
                },
                {
                    "Cuadro de Doble Entrada", new PortafolioHeaderTablaControl {
                        key = "promedioCuadroDobleEntrada",
                        name = "Cuadro de Doble Entrada",
                        instrumentos = true,
                    }
                },
                {
                    "Cuadro Sinoptico", new PortafolioHeaderTablaControl {
                        key = "promedioCuadroSinoptico",
                        name = "Cuadro Sinoptico",
                        instrumentos = true,
                    }
                },
                {
                    "Ensayo", new PortafolioHeaderTablaControl {
                        key = "promedioEnsayo",
                        name = "Ensayo",
                        instrumentos = true,
                    }
                },
                {
                    "Glosario", new PortafolioHeaderTablaControl {
                        key = "promedioGlosario",
                        name = "Glosario",
                        instrumentos = true,
                    }
                },
                {
                    "Mapa Conceptual", new PortafolioHeaderTablaControl {
                        key = "promedioMapaConceptual",
                        name = "Mapa Conceptual",
                        instrumentos = true,
                    }
                },
                {
                    "Mapa Mental", new PortafolioHeaderTablaControl {
                        key = "promedioMapaMental",
                        name = "Mapa Mental",
                        instrumentos = true,
                    }
                },
                {
                    "Presentacion Electrónica", new PortafolioHeaderTablaControl {
                        key = "promedioPresentacionElectronica",
                        name = "Presentacion Electrónica",
                        instrumentos = true,
                    }
                },
                {
                    "Resumen", new PortafolioHeaderTablaControl {
                        key = "promedioResumen",
                        name = "Resumen",
                        instrumentos = true,
                    }
                },
                {
                    "Trabajo Colaborativo Internet", new PortafolioHeaderTablaControl {
                        key = "promedioTrabajoColaborativoInternet",
                        name = "Trabajo Colaborativo Internet",
                        instrumentos = true,
                    }
                },
                {
                    "Esquema", new PortafolioHeaderTablaControl {
                        key = "promedioEsquema",
                        name = "Esquema",
                        instrumentos = true,
                    }
                },
                {
                    "Cartel", new PortafolioHeaderTablaControl {
                        key = "promedioCartel",
                        name = "Cartel",
                        instrumentos = true,
                    }
                },
                {
                    "Triptico", new PortafolioHeaderTablaControl {
                        key = "promedioTriptico",
                        name = "Triptico",
                        instrumentos = true,
                    }
                },
            };

            var portafolios = db.Portafolio
                .Where(a => a.IDGrupo == grupo && a.Bimestres.Bimestre == bimestre)
                .GroupBy(a => a.TipoPortafolio.Nombre)
                .Select(a => a.OrderBy(p => p.FechaEntrega).FirstOrDefault())
                .Where(a => a != null)
                .Select(a => new { Fecha = a.FechaEntrega, Tipo = a.TipoPortafolio.Nombre })
                .OrderBy(a => a.Fecha)
                .Select(a => a.Tipo)
                .AsEnumerable()
                .Select(a => headersPortafolio.ContainsKey(a) ? headersPortafolio[a] : null)
                .Where(a => a != null)
                .ToList();

            result.AddRange(portafolios);
            #endregion


            #region Habilidad
            result.Add(new
            {
                key = "promedioAutoevaluacion",
                name = "Autoevaluación"
            });
            result.Add(new
            {
                key = "promedioCoevaluacion",
                name = "Coevaluacion"
            });
            #endregion

            if (db.DiagnosticoCiclo.Any(a => a.Bimestres.IDGrupo == grupo && a.Bimestres.Bimestre == bimestre))
                result.Add(new {
                    key = "promedioDiagnosticoCiclo",
                    name = "Diagnostico por Ciclo"
                });

            result.Add(new {
                key = nameof(AlumnoReporteViewModel.totalFaltas),
                name = "Inasistencias"
            });


            return result;
        }

        public static double calcularPromedioSEP(List<double?> calificaciones)
        {
            return calcularPromedioSEP(calificaciones, calificaciones.Count*10);
        }

        public static double calcularPromedioSEP(List<double?> calificaciones, int reactivos)
        {
            double cali = 0,
                   sum = 0;

            foreach(double? c in calificaciones)
            {
                sum += (c != null && !double.IsNaN(c.Value) ? c.Value : 0);
            }

            cali = sum * 10 / (reactivos == 0 ? 1 :reactivos);

            if (cali < 5)
                cali = 5;
            else if (cali > 10)
                cali = 10;

            return cali;
        }

        public static double calcularPromedio(List<double?> calificaciones)
        {
            double cali = 0,
                   sum = 0;

            foreach (double? c in calificaciones)
            {
                sum += (c != null ? c.Value : 0);
            }

            cali = sum / (calificaciones.Count == 0 ? 1 : calificaciones.Count);


            return cali;
        }

        public static List<ResumenCalificacionViewModel> calcularResumen(List<AlumnoReporteViewModel> calificaciones)
        {
            List<ResumenCalificacionViewModel> res = new List<ResumenCalificacionViewModel>();

            string[] bimestres = new string[] { "","Primer","Segundo","Tercero","Cuarto","Quinto"};

            for(int bimestre = 1; bimestre <= 5; bimestre++) {

                int noexisten = 0;
                ResumenCalificacionViewModel resumenBimestre = new ResumenCalificacionViewModel()
                {
                    totalAlumnos = calificaciones.GroupBy(a => a.id ).Count(),
                    bimestre = bimestres[bimestre]
                };

                double sum = 0;

                List<Guid> lalumnosSubieron = new List<Guid>(),
                          lalumnosSubieronYReprobaron = new List<Guid>(),
                          lalumnosReprobaronBimestre = new List<Guid>(),
                          lalumnosBajaron = new List<Guid>(),
                          lalumnosBajaronYAprobaron = new List<Guid>(),
                          lalumnosBajaronYReprobaron = new List<Guid>(),
                          lalumnos7 = new List<Guid>(),
                          lalumnos8 = new List<Guid>(),
                          lalumnos9 = new List<Guid>(),
                          lalumnos10 = new List<Guid>();

                foreach (var alum in calificaciones)
                {
                    double caliAnterior = 0, caliActual = 0;
                
                    if (bimestre == 1) {
                        if (alum.promedioExamenParcial2Bimestre1 > 0 ) {
                            caliAnterior = alum.promedioExamenParcial2Bimestre1;
                            caliActual = alum.promedioExamenBimestre1;
                        } else {
                        caliAnterior = alum.promedioExamenParcial1Bimestre1;
                        caliActual = alum.promedioExamenBimestre1;
                        }
                    }
                    if (bimestre == 2) {
                        if (alum.promedioExamenParcial2Bimestre2 > 0)
                        {
                            caliAnterior = alum.promedioExamenParcial2Bimestre2;
                            caliActual = alum.promedioExamenBimestre2;
                        }
                        else
                        {
                            caliAnterior = alum.promedioExamenParcial1Bimestre2;
                            caliActual = alum.promedioExamenBimestre2;
                        }
                    }
                    if (bimestre ==3) {
                        if (alum.promedioExamenParcial2Bimestre3 > 0)
                        {
                            caliAnterior = alum.promedioExamenParcial2Bimestre3;
                            caliActual = alum.promedioExamenBimestre3;
                        }
                        else
                        {
                            caliAnterior = alum.promedioExamenParcial1Bimestre3;
                            caliActual = alum.promedioExamenBimestre3;
                        }
                    }
                    if (bimestre ==4) {
                        if (alum.promedioExamenParcial2Bimestre4 > 0)
                        {
                            caliAnterior = alum.promedioExamenParcial2Bimestre4;
                            caliActual = alum.promedioExamenBimestre4;
                        }
                        else
                        {
                            caliAnterior = alum.promedioExamenParcial1Bimestre4;
                            caliActual = alum.promedioExamenBimestre4;
                        }
                    }
                    if (bimestre ==5){
                        if (alum.promedioExamenParcial2Bimestre5 > 0)
                        {
                            caliAnterior = alum.promedioExamenParcial2Bimestre5;
                            caliActual = alum.promedioExamenBimestre5;
                        }
                        else
                        {
                            caliAnterior = alum.promedioExamenParcial1Bimestre5;
                            caliActual = alum.promedioExamenBimestre5;
                        }
                    }

                    if (caliActual == 0)
                    {
                        noexisten++;
                        continue; // Si es 0 es porque no se ha calificado
                    }

                     
                          
                
                 
                    
                  

                    if (caliActual > caliAnterior) {
                        lalumnosSubieron.Add(alum.id);
                    }
                    if (caliActual<6) {
                        lalumnosReprobaronBimestre.Add(alum.id);
                        lalumnosBajaron.Add(alum.id);
                    }
                  
                    if (caliActual >= caliAnterior && caliActual <= 5) {
                        lalumnosSubieronYReprobaron.Add(alum.id);
                    }
                    if (caliActual <= caliAnterior && caliActual > 5)
                    {
                        lalumnosBajaronYAprobaron.Add(alum.id);
                    }
                    if (caliActual <= caliAnterior && caliActual <= 5)
                    {
                        lalumnosBajaronYReprobaron.Add(alum.id);
                    }


                    /*
                    if (caliActual > caliAnterior)
                    {
                        lalumnosSubieron.Add(alum.id);
                        if (caliActual < 6)
                        {
                            lalumnosSubieronYReprobaron.Add(alum.id);
                            lalumnosReprobaronBimestre.Add(alum.id);
                        }
                    }
                    else
                    {
                        lalumnosBajaron.Add(alum.id);
                        if (caliActual >= 6)
                            lalumnosBajaronYAprobaron.Add(alum.id);
                        else
                        {
                            lalumnosBajaronYReprobaron.Add(alum.id);
                            lalumnosReprobaronBimestre.Add(alum.id);
                        }
                    }



                    if (caliActual >= 6 && caliActual < 7)
                        lalumnos7.Add(alum.id);
                    if (caliActual >= 7 && caliActual < 8)
                        lalumnos8.Add(alum.id);
                    if (caliActual >= 8 && caliActual < 9)
                        lalumnos9.Add(alum.id);
                    if (caliActual >= 9)
                        lalumnos10.Add(alum.id);
    */
                    sum += caliActual;
                }


                resumenBimestre.existe = !(noexisten == calificaciones.Count);

                resumenBimestre.alumnos10 = lalumnos10.Distinct().Count();
                resumenBimestre.alumnos7 = lalumnos7.Distinct().Count();
                resumenBimestre.alumnos8 = lalumnos8.Distinct().Count();
                resumenBimestre.alumnos9 = lalumnos9.Distinct().Count();
                resumenBimestre.alumnosBajaron = lalumnosBajaron.Distinct().Count();
                resumenBimestre.alumnosBajaronYAprobaron = lalumnosBajaronYAprobaron.Distinct().Count();
                resumenBimestre.alumnosBajaronYReprobaron = lalumnosBajaronYReprobaron.Distinct().Count();
                resumenBimestre.alumnosReprobaronBimestre = lalumnosReprobaronBimestre.Distinct().Count();
                resumenBimestre.alumnosSubieron = lalumnosSubieron.Distinct().Count();
                resumenBimestre.alumnosSubieronYReprobaron = lalumnosSubieronYReprobaron.Distinct().Count();

                resumenBimestre.promedioBimestral = sum / (resumenBimestre.totalAlumnos == 0 ?1 : resumenBimestre.totalAlumnos);

                res.Add(resumenBimestre);
            }

            


            return res;
        }
    }

    public struct AlumnoDesempenioStatus
    {
        public const string USAER = "#f5caf5";
        public const string BIEN = "#D8FDD1";
        public const string REGULAR = "#F9F9BD";
        public const string APOYO = "#ff2a2a";
    }

    public class AlumnoReporteViewModel
    {

        public Guid id { get; set; }
        public double totalAsistencias { get; set; }
        public double totalFaltas { get; set; }
        public double promedioTrabajo { get; set; }
        public double promedioEsquema { get; set; }
        public double promedioExposicion { get; set; }
        public double promedioGuia { get; set; }
        public double promedioListaCotejo { get; set; }
        public double promedioLineaTiempo { get; set; }
        public double promedioMapaAprendisaje { get; set; }
        public double promedioPortafolio { get; set; }
        public double promedioProduccionesEscritas { get; set; }
        public double promedioProyecto { get; set; }
        public double promedioRegistroAnecdotico { get; set; }
        public double promedioRevisionCuadernos { get; set; }
        public double promedioRubrica { get; set; }
        public double promedioExamenParcial { get; set; }
        public double promedioExamenBimestral { get; set; }
        public double promedioExamenRecuperacion { get; set; }
        public double promedioExamenDiagnostico { get; set; }
        public double promedioDiagnosticoCiclo { get; set; }


        public double promedioExamenParcial1Bimestre1 { get; set; }
        public double promedioExamenParcial2Bimestre1 { get; set; }
        public double promedioExamenParcial1Bimestre2 { get; set; }
        public double promedioExamenParcial2Bimestre2 { get; set; }
        public double promedioExamenParcial1Bimestre3 { get; set; }
        public double promedioExamenParcial2Bimestre3 { get; set; }
        public double promedioExamenParcial1Bimestre4 { get; set; }
        public double promedioExamenParcial2Bimestre4 { get; set; }
        public double promedioExamenParcial1Bimestre5 { get; set; }
        public double promedioExamenParcial2Bimestre5 { get; set; }

        public double promedioExamenBimestre1 { get; set; }
        public double promedioExamenBimestre2 { get; set; }
        public double promedioExamenBimestre3 { get; set; }
        public double promedioExamenBimestre4 { get; set; }
        public double promedioExamenBimestre5 { get; set; }

        public double totalTrabajosCumplidos { get; set; }
        public double totalTrabajosMedios { get; set; }
        public double totalTrabajoNoCumplidos { get; set; }


        public double promedioAutoevaluacion { get; set; }
        public double promedioCoevaluacion { get; set; }

        public double promedioFinal { get; set; }
        public double promedioPresentacionOralInternet { get; internal set; }
        public double promedioCuadroComparativo { get; set; }
        public double promedioManualidad { get; set; }
        public double promedioPrueba { get; set; }
        public double promedioInvestigacionImpresa { get; set; }
        public double promedioExposicionInternet { get; set; }
        public double promedioLineaTiempoInternet { get; set; }
        public double promedioCuadroDobleEntrada { get; set; }
        public double promedioCuadroSinoptico { get; set; }
        public double promedioEnsayo { get; set; }
        public double promedioGlosario { get; set; }
        public double promedioMapaConceptual { get; set; }
        public double promedioMapaMental { get; set; }
        public double promedioPresentacionElectronica { get; set; }
        public double promedioResumen { get; set; }
        public double promedioTrabajoColaborativoInternet { get; set; }
        //public double promedioEsquema { get; set; }
        public double promedioCartel { get; set; }
        public double promedioTriptico { get; set; }
    }

    public class ResumenCalificacionViewModel
    {
        public string bimestre { get; set; }
        public int totalAlumnos { get; set; }
        public int alumnosSubieron { get; set; }
        public int alumnosBajaron { get; set; }
        public int alumnosBajaronYReprobaron { get; set; }
        public int alumnosBajaronYAprobaron { get; set; }
        public int alumnosSubieronYReprobaron { get; set; }
        public int alumnosReprobaronBimestre { get; set; }
        public int alumnos7 { get; set; }
        public int alumnos8 { get; set; }
        public int alumnos9 { get; set; }
        public int alumnos10 { get; set; }
        public double promedioBimestral { get; set; }
        public bool existe { get; set; }
    }
}