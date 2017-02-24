using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.ViewModels
{
    public class ReporteEscuelaViewModel
    {
        public AsistenciasReporte Asistencias { get; set; }
        public TrabajosReporte Trabajos { get; set; }
        public InstrumentosReporte Instrumentos { get; set; }
        public ExamenesReporte Examenes { get; set; }
        public IEnumerable<PromedioGeneralGrupoReporte> Promedios { get; internal set; }

        public class InstrumentosReporte
        {
            public double Promedio { get; set; }
            public long Aprovados { get; set; }
            public long NoAprovados { get; set; }
        }

        public class TrabajosReporte
        {
            public long Entregados { get; set; }
            public long NoEntregados { get; set; }
            public long Incompleto { get; set; }
        }

        public class AsistenciasReporte
        {
            public long Faltas { get; set; }
            public long Asistencias { get; set; }
            public long Justificaciones { get; set; }
            public long Retardo { get; set; }
            public long Suspencion { get; set; }
        }

        public class ExamenReporte
        {
            public long Aprovado { get; set; }
            public long NoAprovado { get; set; }
            public double Promedio { get; set; }
        }

        public class ExamenesReporte
        {
            public ExamenReporte Bimestrales { get; set; }
            public ExamenReporte Diagnostico { get; set; }
            public ExamenReporte Parciales { get; set; }
            public ExamenReporte Recuperacion { get; set; }
        }

        public class PromedioGeneralGrupoReporte
        {
            public string Grupo { get; set; }
            public double Promedio { get; set; }
            public long NoAprovados { get; set; }
            public long P6 { get; set; }
            public long P7 { get; set; }
            public long P8 { get; set; }
            public long P9Y10 { get; set; }
        }
    }
}