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
    
    public partial class TutorAlumno
    {
        public System.Guid IdAlumno { get; set; }
        public string IdTutor { get; set; }
        public System.DateTimeOffset FechaSync { get; set; }
    
        public virtual Alumno Alumno { get; set; }
    }
}
