using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.ViewModels
{
    public class ActivacionViewModel
    {
        public bool Tutor { get; set; } = false;
        public bool Maestro { get; set; } = false;
        public bool Escuela { get; set; } = false;
    }

    public class ActivacionPagoViewModel {
        public int Licencias { get; set; }
        public int Paquete { get; set; }
    }
}