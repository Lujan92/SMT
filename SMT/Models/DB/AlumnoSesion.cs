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
    
    public partial class AlumnoSesion
    {
        public System.Guid IDSesion { get; set; }
        public System.Guid IDAlumno { get; set; }
        public System.DateTime FechaActualizacion { get; set; }
        public int Estado { get; set; }
        public System.DateTimeOffset FechaSync { get; set; }
    
        public virtual Alumno Alumno { get; set; }
        public virtual Sesion Sesion { get; set; }
    }
}
