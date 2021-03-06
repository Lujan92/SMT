//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SMT.Models.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class Alumno
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Alumno()
        {
            this.HabilidadesAlumno = new HashSet<HabilidadesAlumno>();
            this.TutorAlumno = new HashSet<TutorAlumno>();
            this.AlumnoDesempenio = new HashSet<AlumnoDesempenio>();
            this.ExamenAlumno = new HashSet<ExamenAlumno>();
            this.DiagnosticoCicloAlumno = new HashSet<DiagnosticoCicloAlumno>();
            this.TrabajoAlumno = new HashSet<TrabajoAlumno>();
            this.DiagnosticoPsicologico = new HashSet<DiagnosticoPsicologico>();
            this.AlumnoSesion = new HashSet<AlumnoSesion>();
            this.PortafolioAlumno = new HashSet<PortafolioAlumno>();
        }
    
        public System.Guid IDAlumno { get; set; }
        public string Nombre { get; set; }
        public System.Guid IDGrupo { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Curp { get; set; }
        public int Estado { get; set; }
        public bool EsUSAER { get; set; }
        public Nullable<System.DateTime> FechaActualizacion { get; set; }
        public string NombreCompleto { get; set; }
        public Nullable<double> PromedioBimestre1 { get; set; }
        public Nullable<double> PromedioBimestre2 { get; set; }
        public Nullable<double> PromedioBimestre3 { get; set; }
        public Nullable<double> PromedioBimestre4 { get; set; }
        public Nullable<double> PromedioBimestre5 { get; set; }
        public Nullable<double> PromedioTotal { get; set; }
        public string ColorPromedio { get; set; }
        public string Grupo { get; set; }
        public System.DateTimeOffset FechaSync { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HabilidadesAlumno> HabilidadesAlumno { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TutorAlumno> TutorAlumno { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AlumnoDesempenio> AlumnoDesempenio { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExamenAlumno> ExamenAlumno { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DiagnosticoCicloAlumno> DiagnosticoCicloAlumno { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TrabajoAlumno> TrabajoAlumno { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DiagnosticoPsicologico> DiagnosticoPsicologico { get; set; }
        public virtual Grupos Grupos { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AlumnoSesion> AlumnoSesion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PortafolioAlumno> PortafolioAlumno { get; set; }
    }
}
