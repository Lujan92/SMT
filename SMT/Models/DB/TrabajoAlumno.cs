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
    
    public partial class TrabajoAlumno
    {
        public System.Guid IDTrabajoAlumno { get; set; }
        public Nullable<System.Guid> IDTrabajo { get; set; }
        public Nullable<System.Guid> IDAlumno { get; set; }
        public string Observaciones { get; set; }
        public Nullable<System.DateTime> FechaActualizacion { get; set; }
        public Nullable<int> Estado { get; set; }
        public System.DateTimeOffset FechaSync { get; set; }
    
        public virtual Alumno Alumno { get; set; }
        public virtual Trabajo Trabajo { get; set; }
    }
}
