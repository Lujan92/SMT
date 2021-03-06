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
    
    public partial class ExamenTema
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ExamenTema()
        {
            this.ExamenAlumno = new HashSet<ExamenAlumno>();
            this.PreguntaExamen = new HashSet<PreguntaExamen>();
        }
    
        public System.Guid IDExamen { get; set; }
        public System.Guid IDTema { get; set; }
        public string Nombre { get; set; }
        public int Reactivos { get; set; }
        public string Respuesta { get; set; }
        public string TipoTema { get; set; }
        public string Archivo { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta1 { get; set; }
        public string Respuesta2 { get; set; }
        public string Instrucciones { get; set; }
        public System.DateTimeOffset FechaSync { get; set; }
    
        public virtual Examen Examen { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExamenAlumno> ExamenAlumno { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PreguntaExamen> PreguntaExamen { get; set; }
    }
}
