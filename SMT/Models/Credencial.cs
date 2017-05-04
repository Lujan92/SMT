using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMT.Models.DB
{
    public partial class Credencial
    {
        /// <summary>
        /// Genera una licencia sin beneficiario. Si la licencia es principal, el dueño de la licencia se pone como beneficiario.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string generarCredencial(string usuario,string tipo, bool principal = false, DateTime? vigencia = null)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                Credencial cred = new Credencial()
                {
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    FechaVigencia = vigencia ?? DateTime.Now.AddYears(1),
                    IDCredencial = Guid.NewGuid().ToString(),
                    IDUsuarioPaga = usuario,
                    EsPrincipal = principal,
                    Tipo = tipo
                };

                if (principal == true)
                {
                    cred.IDUsuarioBeneficiario = usuario;

                    if (db.Credencial.Any(a => a.IDUsuarioPaga == usuario && a.EsPrincipal == true))
                        throw new Exception("No se puede asignar otra licencia a este usuario porque ya es poseedor de una licencia");
                }
                else
                {
                    if (db.Credencial.Any(a => a.IDUsuarioBeneficiario == usuario && a.EsPrincipal == false))
                        throw new Exception("No se puede asignar otra licencia a este usuario porque ya es poseedor de una licencia");
                }
         
               

                db.Credencial.Add(cred);
                db.SaveChanges();

                return cred.IDCredencial;
            }
        }

        public static bool tieneCredencialValida(string usuario)
        {
            using (SMTDevEntities db = new SMTDevEntities())
            {
                return db.Credencial.Where(i => i.IDUsuarioBeneficiario == usuario && i.Activo == true && i.FechaVigencia < DateTime.Now).Any();
            }
        }

        /// <summary>
        /// Asigna un usuario a una licencia ya creada
        /// </summary>
        /// <param name="usuario">Usuario dueño de la licencia</param>
        /// <param name="credencial">Licencia a la que se agregara el usuario</param>
        /// <param name="beneficiario">Usuario que se agrega a la licencia</param>
        public static void asignar(string usuario,string credencial, string beneficiario)
        {
            using(SMTDevEntities db = new SMTDevEntities())
            {
                Credencial cred = db.Credencial.FirstOrDefault(i => i.IDUsuarioPaga == usuario && i.IDCredencial == credencial);
                if(cred != null)
                {
                    cred.IDUsuarioBeneficiario = beneficiario;
                    db.SaveChanges();
                }
            }
        }

        public static IQueryable<Credencial> listarPorUsuario(string usuario)
        {
            SMTDevEntities db = new SMTDevEntities();


            return db.Credencial.Where(i => i.IDUsuarioPaga == usuario);
        }

        public static IQueryable<Credencial> listarPrincipales()
        {
            SMTDevEntities db = new SMTDevEntities();


            return db.Credencial.Where(i => i.EsPrincipal == true);
        }

        public static void cambiarEstatus(string usuario, string crendencial)
        {
            using (SMT.Models.DB.SMTDevEntities db = new Models.DB.SMTDevEntities())
            {
                var escuela = db.Credencial.FirstOrDefault(i => i.IDUsuarioPaga == usuario && i.IDCredencial == crendencial);
                if (escuela != null)
                {
                    escuela.Activo = !escuela.Activo;
                    db.SaveChanges();
                }
            }
        }

        public static void renovarcredencial(string id,DateTime fecha) {
            
     
            using (SMT.Models.DB.SMTDevEntities db = new Models.DB.SMTDevEntities())
            {
                var user = db.Credencial.FirstOrDefault(i => i.IDCredencial == id);
                if (user != null)
                {
                    user.FechaVigencia= fecha;
                    db.SaveChanges();
                }
            }

        }
        public static void cambiarEstatusPrincipal(string crendencial)
        {
            using (SMT.Models.DB.SMTDevEntities db = new Models.DB.SMTDevEntities())
            {
                var escuela = db.Credencial.FirstOrDefault(i => i.IDCredencial == crendencial && i.EsPrincipal == true);
                if (escuela != null)
                {
                    escuela.Activo = !escuela.Activo;
                   foreach (var m in db.Credencial.Where(a => a.IDUsuarioPaga == escuela.IDUsuarioPaga).ToList())
                    {
                        m.Activo = escuela.Activo;
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void eliminarbeneficiario(string usuario, string crendencial)
        {
            using (SMT.Models.DB.SMTDevEntities db = new Models.DB.SMTDevEntities())
            {
                var escuela = db.Credencial.FirstOrDefault(i => i.IDUsuarioPaga == usuario && i.IDUsuarioBeneficiario == crendencial && i.EsPrincipal == false);
                if (escuela != null)
                {
                    escuela.IDUsuarioBeneficiario = null;
                    db.SaveChanges();
                }
            }
        }

        public static void eliminarbeneficiario2(string usuario, string crendencial)
        {
            using (SMT.Models.DB.SMTDevEntities db = new Models.DB.SMTDevEntities())
            {
                var escuela = db.Credencial.FirstOrDefault(i => i.IDUsuarioPaga == usuario && i.IDCredencial == crendencial && i.EsPrincipal == false);
                if (escuela != null)
                {
                    escuela.IDUsuarioBeneficiario = null;
                    db.SaveChanges();
                }
            }
        }
        public static void eliminarPrincipal(string crendencial)
        {
            using (SMT.Models.DB.SMTDevEntities db = new Models.DB.SMTDevEntities())
            {
                var escuela = db.Credencial.FirstOrDefault(i => i.IDCredencial == crendencial && i.EsPrincipal == true);
                if (escuela != null)
                {
                   
                    foreach (var m in db.Credencial.Where(a => a.IDUsuarioPaga == escuela.IDUsuarioPaga && a.EsPrincipal == false).ToList())
                    {
                        
                        db.Credencial.Remove(m);
                    }

                    db.Credencial.Remove(escuela);
                    db.SaveChanges();
                }
            }
        }

        public static void eliminarSinAsingar(string crendencial)
        {
            using (SMT.Models.DB.SMTDevEntities db = new Models.DB.SMTDevEntities())
            {
                var escuela = db.Credencial.FirstOrDefault(i => i.IDCredencial == crendencial && i.EsPrincipal == true);
                if (escuela != null)
                {

                    foreach (var m in db.Credencial.Where(a => a.IDUsuarioPaga == escuela.IDUsuarioPaga && a.IDUsuarioBeneficiario == null && a.EsPrincipal == false).ToList())
                    {

                        db.Credencial.Remove(m);
                    }
                    db.SaveChanges();
                }
            }
        }

        public static bool tieneCredenciales(string usuario)
        {
            using (SMT.Models.DB.SMTDevEntities db = new Models.DB.SMTDevEntities())
            {
                return db.Credencial.Where(i => i.IDUsuarioPaga == usuario && i.EsPrincipal == false).Any(); 
            }
        }
    }




    public class CredencialAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = httpContext.User;
            if (user.Identity.IsAuthenticated)
            {
                var secciones = user.GetSecciones();
                if (secciones.TotalEnabled == 0) {
                    httpContext.Response.Redirect("/Activacion/");
                    return true;
                }
                else if (secciones.TotalEnabled == 1 && secciones.Tutor) {
                    httpContext.Response.Redirect("/Tutor");
                    return true;
                }

                string currentController = httpContext.Request.RequestContext.RouteData.GetRequiredString("controller");
                if (currentController.ToUpper() == "PAGOS")
                    return true;
                else
                {
                    if (!user.IsInRole("Root"))
                    {
                        int error = 0;

                        if (httpContext.Session["credencial-error"] != null)
                        {
                            error = int.Parse(httpContext.Session["credencial-error"].ToString());
                        }
                        else
                        {
                            using (SMTDevEntities db = new SMTDevEntities())
                            {
                                string id = user.Identity.GetUserId();

                                Credencial cred = db.Credencial.Where(i => i.IDUsuarioBeneficiario == id).FirstOrDefault();

                                if (cred == null)
                                {
                                    error = 1;
                                }
                                else
                                {
                                    if (cred.FechaVigencia < DateTime.Now)
                                        error = 2;
                                    if (cred.Activo == false)
                                        error = 3;
                                }


                            };
                        }

                        if (error > 0)
                        {
                            httpContext.Session.Add("credencial-error", error);
                            httpContext.Response.Redirect("/Pagos?error=" + error);
                        }
                    }


                }
            }

            return true;
        }
    }
    
}