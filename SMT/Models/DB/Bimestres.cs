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
    
    public partial class Bimestres
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Bimestres()
        {
            this.Sesion = new HashSet<Sesion>();
            this.HabilidadesAlumno = new HashSet<HabilidadesAlumno>();
            this.Portafolio = new HashSet<Portafolio>();
            this.Trabajo = new HashSet<Trabajo>();
            this.DiagnosticoCiclo = new HashSet<DiagnosticoCiclo>();
            this.Examen = new HashSet<Examen>();
        }
    
        public System.Guid IDBimestre { get; set; }
        public Nullable<System.Guid> IDGrupo { get; set; }
        public Nullable<int> Bimestre { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sesion> Sesion { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HabilidadesAlumno> HabilidadesAlumno { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Portafolio> Portafolio { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Trabajo> Trabajo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DiagnosticoCiclo> DiagnosticoCiclo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Examen> Examen { get; set; }
        public virtual Grupos Grupos { get; set; }
    }
}
