
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMT.Models
{
    public class Invitacion
    {
        /// <summary>
        /// Se genera una licencia y se envía la invitación al usuario que se va a crear.
        /// </summary>
        /// <param name="email">Email para generar el usuario y enviar invitacion</param>
        /// <param name="rol">Rol que se asignara al usuario</param>
        /// <param name="usuarioPadre">Usuario dueño de las licencias</param>
        public async static Task generarLicenciaYEnviarInvitacion(string email, string rol, string usuarioPadre = null)
        {
            await enviar(email, rol, null, usuarioPadre);
        }

        /// <summary>
        /// Se asigna una licencia y se envía la invitación al usuario que se va a crear.
        /// </summary>
        /// <param name="email">Email para generar el usuario y enviar invitacion</param>
        /// <param name="licencia">Id de la licencia</param>
        public async static void asignarLicenciaYEnviarInvitacion(string email, string licencia)
        {
            await enviar(email, null, licencia, null);
        }

        /// <summary>
        /// Genera/Asigna una licencia y envía la invitación al usuario que se va a crear.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="rol"></param>
        /// <param name="licencia"></param>
        /// <param name="usuarioPadre"></param>
        /// <returns></returns>
        private async static Task<bool> enviar(string email, string rol, string licencia, string usuarioPadre)
        {
            Usuario user = new Usuario()
            {
                Email = email,
                Nombre = "",
                ApellidoPaterno = "",
                ApellidoMaterno = "",
                Password = Guid.NewGuid().ToString()
            };

            if(rol == null && licencia != null)
            {
                using(SMTDevEntities db = new SMTDevEntities())
                {
                    rol = db.Credencial.Where(a => a.IDCredencial == licencia).Select(a => a.Tipo).FirstOrDefault();
                }
            }

            if (await crearUsuario(rol, user))
            {
                if (!string.IsNullOrEmpty(usuarioPadre))
                    generarLicencia(email, rol, usuarioPadre);
                else
                    asignarLicencia(email, licencia);
            }


            string url = await generarCodigoReinicio(email);
            Util.SendEmailWithTemplate(email, "Invitación de registro", "InvitacionRegistro.html", new
            {
                url = url
            });

            return true;
        }

        private static void generarLicencia(string email,string rol,string usuarioPadre)
        {
            ApplicationUserManager UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var u = UserManager.FindByEmail(email);

            if (usuarioPadre != null)
            {
                // Agrega la credencial a una licencia principal
                string id = Credencial.generarCredencial(usuarioPadre, rol.ToUpper(), false); // Se genera
                Credencial.asignar(usuarioPadre, id, u.Id); // Se asigna
            }
            else
            {
                // Genera una licencia principal
                Credencial.generarCredencial(u.Id, rol.ToUpper(), true);
            }
        }

        private static void asignarLicencia(string email, string  licencia)
        {
            ApplicationUserManager UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var u = UserManager.FindByEmail(email);

            using (SMTDevEntities db = new SMTDevEntities())
            {
                string propietario = db.Credencial.Where(a => a.IDCredencial == licencia).Select(a => a.IDUsuarioPaga).FirstOrDefault();
                Credencial.asignar(propietario, licencia, u.Id);
            }
        }



        private static async Task<bool> crearUsuario(string rol,Usuario user)
        {

            ApplicationUserManager UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            #region Generar usuario
            user.Username = user.Email;

            if (await UserManager.FindByNameAsync(user.Username) != null)
                throw new Exception("El nombre de usuario ya existe");

            if (await UserManager.FindByEmailAsync(user.Email) != null)
                throw new Exception("El nombre de usuario ya existe");

            var usuario = new ApplicationUser()
            {
                UserName = user.Username,
                Email = user.Email,
                Nombre = user.Nombre,
                ApellidoPaterno = user.ApellidoPaterno,
                ApellidoMaterno = user.ApellidoMaterno,
                EmailConfirmed = false
            };

            var result = await UserManager.CreateAsync(usuario, user.Password);

            if (user.IdRoles != null)
                await Usuario.agregarRolesByName(usuario.UserName, new List<string>() { rol});
            #endregion


            if (result.Succeeded)
            {
                return true;
            }
            else
            {
                string errores = "<ul>";
                foreach (var m in result.Errors)
                {
                    errores += string.Format("<li>{0}</li>", m);
                }

                errores += "</ul>";
                throw new Exception(errores);
            }

        }

        /// <summary>
        /// Genera un codigo de reinicio de contraseña
        /// </summary>
        /// <returns></returns>
        public async static Task<string> generarCodigoReinicio(string email)
        {
            ApplicationUserManager UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var u = UserManager.FindByEmail(email);

            string code = await UserManager.GeneratePasswordResetTokenAsync(u.Id);
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var callbackUrl = url.Action("ResetPassword", "Account", new {
                userId = u.Id,
                code = code,
                email = email,
                area = ""
            }, protocol: HttpContext.Current.Request.Url.Scheme);

            return callbackUrl;
        }

    }
}