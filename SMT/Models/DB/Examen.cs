//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SMT.Models.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class Examen
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Examen()
        {
            this.ExamenTema = new HashSet<ExamenTema>();
        }
    
        public System.Guid IDExamen { get; set; }
        public System.Guid IDBimestre { get; set; }
        public System.DateTime FechaRegistro { get; set; }
        public System.DateTime FechaEntrega { get; set; }
        public System.DateTime FechaActualizacion { get; set; }
        public string Titulo { get; set; }
        public string Tipo { get; set; }
        public System.DateTimeOffset FechaSync { get; set; }
    
        public virtual Bimestres Bimestres { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExamenTema> ExamenTema { get; set; }
    }
}
