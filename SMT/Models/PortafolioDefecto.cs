using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models.DB
{
    public partial class PortafolioDefecto
    {

        public void actualizar(string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
               
                PortafolioDefecto port = db.PortafolioDefecto.FirstOrDefault(i => i.TipoTrabajo == TipoTrabajo && i.IDUsuario == usuario);

                if (port == null)
                {
                    this.IDUsuario = usuario;
                    this.FechaActualizacion = DateTime.Now;
                    this.FechaSync = DateTime.Now;
                    this.IDPortafolioDefecto = Guid.NewGuid();
                    this.Nombre = this.Nombre;
                    db.PortafolioDefecto.Add(this);
                    db.SaveChanges();
                }
                else
                {
                    port.Nombre =Nombre;
                    port.TipoTrabajo = TipoTrabajo;
                    port.FechaActualizacion = DateTime.Now;
                    port.FechaSync = DateTime.Now;
                    port.Activo1 = Activo1;
                    port.Aspecto1 = Aspecto1;
                    port.Criterio1 = Criterio1;
                    port.Activo2 = Activo2;
                    port.Aspecto2 = Aspecto2;
                    port.Criterio2 = Criterio2;
                    port.Activo3 = Activo3;
                    port.Aspecto3 = Aspecto3;
                    port.Criterio3 = Criterio3;
                    port.Activo4 = Activo4;
                    port.Aspecto4 = Aspecto4;
                    port.Criterio4 = Criterio4;
                    port.Activo5 = Activo5;
                    port.Aspecto5 = Aspecto5;
                    port.Criterio5 = Criterio5;
                    port.FechaActualizacion = DateTime.Now;

                    db.SaveChanges();
                }
            }
        }


        public static dynamic get(Guid tipo, string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                Portafolio.PortafolioSimple result = new Portafolio.PortafolioSimple();

                TipoPortafolio def = db.TipoPortafolio.FirstOrDefault(i => i.IDTipoPortafolio == tipo);

                if (def != null)
                {

                    PortafolioDefecto port = db.PortafolioDefecto.FirstOrDefault(i => i.TipoTrabajo == def.Nombre && i.IDUsuario == usuario);

                    if (port != null)
                    {
                        return new
                        {
                            Aspecto1 = port.Aspecto1,
                            Aspecto2 = port.Aspecto2,
                            Aspecto3 = port.Aspecto3,
                            Aspecto4 = port.Aspecto4,
                            Aspecto5 = port.Aspecto5,

                            Criterio1 = port.Criterio1,
                            Criterio2 = port.Criterio2,
                            Criterio3 = port.Criterio3,
                            Criterio4 = port.Criterio4,
                            Criterio5 = port.Criterio5,

                            Activo1 = port.Activo1,
                            Activo2 = port.Activo2,
                            Activo3 = port.Activo3,
                            Activo4 = port.Activo4,
                            Activo5 = port.Activo5
                        };
                    }
                    else
                    {

                        return new
                        {
                            Aspecto1 = def.Aspecto1,
                            Aspecto2 = def.Aspecto2,
                            Aspecto3 = def.Aspecto3,
                            Aspecto4 = def.Aspecto4,
                            Aspecto5 = def.Aspecto5,

                            Criterio1 = def.Criterio1,
                            Criterio2 = def.Criterio2,
                            Criterio3 = def.Criterio3,
                            Criterio4 = def.Criterio4,
                            Criterio5 = def.Criterio5,

                            Activo1 = def.Activo1,
                            Activo2 = def.Activo2,
                            Activo3 = def.Activo3,
                            Activo4 = def.Activo4,
                            Activo5 = def.Activo5
                        };


                    }
                }
                return new
                {
                    Aspecto1 = "Aspecto 1",
                    Aspecto2 = "Aspecto 2",
                    Aspecto3 = "",
                    Aspecto4 = "",
                    Aspecto5 = "",

                    Criterio1 = "1 - Si lo incluye\n2 - Faltante\n3 - No lo incluye",
                    Criterio2 = "1 - Si lo incluye\n2 - Faltante\n3 - No lo incluye",
                    Criterio3 = "",
                    Criterio4 = "",
                    Criterio5 = "",

                    Activo1 = true,
                    Activo2 = true,
                    Activo3 = false,
                    Activo4 = false,
                    Activo5 = false
                };
            }
        }
    }
}