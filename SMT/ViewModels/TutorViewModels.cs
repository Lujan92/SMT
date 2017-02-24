using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SMT.ViewModels
{
    public class RegisterTutorViewModel : LoginTutorViewModel
    {
        [Display(Name = "Nombre(s)")]
        [Required(ErrorMessage = "Especifique su nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Apellido paterno")]
        [Required(ErrorMessage = "El apellido paterno es necesario")]
        public string APaterno { get; set; }

        [Display(Name = "Apellido materno")]
        [Required(ErrorMessage = "El apellido materno es necesario")]
        public string AMaterno { get; set; }

        [Required(ErrorMessage = "Este campo es obligatorio")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        public string Confirmar { get; set; }
    }

    public class LoginTutorViewModel : EmailViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es necesaria")]
        public string Password { get; set; }
    }

    public class AgregarTutorAlumnoViewModel : EmailViewModel
    {
        public Guid Id { get; set; }
    }

    public class EmailViewModel
    {
        [Display(Name = "Correo electrónico")]
        [Required(ErrorMessage = "Debe especificar una dirección de correo electrónico")]
        [EmailAddress(ErrorMessage = "Dirección inválida")]
        public string Email { get; set; }
    }

    public class TutorIndexAlumnoViewModel
    {
        public Guid IdAlumno { get; set; }
        public string CURP { get; set; }
        public string NombreCompleto { get; set; }
        public IEnumerable<Grupos> Grupos { get; set; }
    }

    public class TutorImprimirDetalleAlumnoViewModel
    {
        public Guid IDAlumno { get; set; }
        public string CURP { get; set; }
        public string NombreCompleto { get; set; }
        public Guid IDGrupo { get; set; }
        public bool EsUSAER { get; set; }
        public string Materia { get; set; }
        public string Grado { get; set; }
        public string Grupo { get; set; }
        public string Escuela { get; set; }
        public string NombreMaestro { get; set; }

        public IEnumerable<InformacionBimestreViewModel> Bimestres { get; set; }
    }

    public class TutorDetalleAlumnoViewModel : InformacionBimestreViewModel
    {
        public int Bimestre { get; set; }
        public Guid IDAlumno { get; set; }
        public string CURP { get; set; }
        public string NombreCompleto { get; set; }
        public Guid IDGrupo { get; set; }
        public bool EsUSAER { get; set; }
        public string Materia { get; set; }
        public string Grado { get; set; }
        public string Grupo { get; set; }
        public string Escuela { get; set; }
        public IEnumerable<ExamenDetalleAlumnoViewModel> Examenes { get; internal set; }
    }

    public class InformacionBimestreViewModel
    {
        public int Numero { get; set; }
        public IEnumerable<AlumnoSesion> Sesion { get; set; }
        public IEnumerable<TrabajoAlumno> Trabajo { get; set; }
        public IEnumerable<PortafolioAlumno> Portafolio { get; set; }
        public IEnumerable<Examen> Examen { get; set; }
        public ICollection<ExamenAlumno> ExamenAlumno { get; set; }
        public TutorHabilidadesViewModel Habilidades { get; set; }
        public IEnumerable<dynamic> Headers { get; set; }
        public AlumnoReporteViewModel Desempeno { get; set; }
    }

    public class ExamenDetalleAlumnoViewModel
    {
        public Examen Examen { get; set; }
        public IEnumerable<ExamenAlumno> Respuestas { get; set; }
    }

    public class TutorHabilidadesViewModel
    {
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
    }
}