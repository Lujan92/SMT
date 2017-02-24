using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models.Colecciones
{
    public class Entidad
    {
        public static readonly Entidad[] Entidades = new [] {
            new Entidad { Id = 1, Nombre = "AGUASCALIENTES" },
            new Entidad { Id = 2, Nombre = "BAJA CALIFORNIA" },
            new Entidad { Id = 3, Nombre = "BAJA CALIFORNIA SUR" },
            new Entidad { Id = 4, Nombre = "CAMPECHE" },
            new Entidad { Id = 7, Nombre = "CHIAPAS" },
            new Entidad { Id = 8, Nombre = "CHIHUAHUA" },
            new Entidad { Id = 5, Nombre = "COAHUILA DE ZARAGOZA" },
            new Entidad { Id = 6, Nombre = "COLIMA" },
            new Entidad { Id = 9, Nombre = "DISTRITO FEDERAL" },
            new Entidad { Id = 10, Nombre = "DURANGO" },
            new Entidad { Id = 11, Nombre = "GUANAJUATO" },
            new Entidad { Id = 12, Nombre = "GUERRERO" },
            new Entidad { Id = 13, Nombre = "HIDALGO" },
            new Entidad { Id = 14, Nombre = "JALISCO" },
            new Entidad { Id = 15, Nombre = "MÉXICO" },
            new Entidad { Id = 16, Nombre = "MICHOACÁN DE OCAMPO" },
            new Entidad { Id = 17, Nombre = "MORELOS" },
            new Entidad { Id = 18, Nombre = "NAYARIT" },
            new Entidad { Id = 19, Nombre = "NUEVO LEÓN" },
            new Entidad { Id = 20, Nombre = "OAXACA" },
            new Entidad { Id = 21, Nombre = "PUEBLA" },
            new Entidad { Id = 22, Nombre = "QUERÉTARO" },
            new Entidad { Id = 23, Nombre = "QUINTANA ROO" },
            new Entidad { Id = 24, Nombre = "SAN LUIS POTOSÍ" },
            new Entidad { Id = 25, Nombre = "SINALOA" },
            new Entidad { Id = 26, Nombre = "SONORA" },
            new Entidad { Id = 27, Nombre = "TABASCO" },
            new Entidad { Id = 28, Nombre = "TAMAULIPAS" },
            new Entidad { Id = 29, Nombre = "TLAXCALA" },
            new Entidad { Id = 30, Nombre = "VERACRUZ DE IGNACIO DE LA LLAVE" },
            new Entidad { Id = 31, Nombre = "YUCATÁN" },
            new Entidad { Id = 32, Nombre = "ZACATECAS" }
        };

        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}