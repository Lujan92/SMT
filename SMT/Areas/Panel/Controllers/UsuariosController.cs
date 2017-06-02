
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SMT;
using SMT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMT.Areas.Panel.Controllers
{
    [Authorize(Roles ="Root")]
    public class UsuariosController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Crear(Usuario user)
        {
            {
                try
                {
                    user.Username = user.Email;

                    if (await UserManager.FindByNameAsync(user.Username) != null)
                        return Json(new
                        {
                            Errors = new string[] { "El nombre de usuario ya existe" },

                        });

                    if (await UserManager.FindByEmailAsync(user.Email) != null)
                        return Json(new
                        {
                            Errors = new string[] { "El correo ya existe" },

                        });

                    var usuario = new ApplicationUser()
                    {
                        UserName = user.Username,
                        Email = user.Email,
                        Nombre = user.Nombre,
                        ApellidoPaterno = user.ApellidoPaterno,
                        ApellidoMaterno = user.ApellidoMaterno,
                        EmailConfirmed = true
                    };

                    var result = await UserManager.CreateAsync(usuario, user.Password);

                    if (user.IdRoles != null)
                        Usuario.agregarRoles(usuario.UserName, user.IdRoles);

                    var u = UserManager.FindByName(usuario.UserName);



                    if (result.Succeeded)
                    {
                        return Json(new ResultViewModel(true, "", null));
                    }
                    else
                    {
                        string errores = "<ul>";
                        foreach (var m in result.Errors)
                        {
                            errores += string.Format("<li>{0}</li>", m);
                        }

                        errores += "</ul>";
                        return Json(new ResultViewModel(false, errores, null));
                    }

                }
                catch (Exception e)
                {
                    return Json(new ResultViewModel(e));
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Editar(Usuario user)
        {
            {
                try
                {

                    user.editar();


                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        string code = UserManager.GeneratePasswordResetToken(user.Id);
                        UserManager.ResetPassword(user.Id, code, user.Password);
                    }



                    return Json(new ResultViewModel(true, "", null));
                }
                catch (Exception e)
                {
                    return Json(new ResultViewModel(e));
                }
            }
        }

        [HttpPost]
        public JsonResult Eliminar(Usuario user)
        {
            {
                try
                {

                    user.eliminar();

                    return Json(new ResultViewModel(true, "", null));
                }
                catch (Exception e)
                {
                    return Json(new ResultViewModel(e));
                }
            }
        }

        [HttpPost]
        public JsonResult CambiarEstado(string id)
        {
            {
                try
                {
                    var user = Usuario.Get(id);

                    user.cambiarEstatus();

                    return Json(new ResultViewModel(true, "", null));
                }
                catch (Exception e)
                {
                    return Json(new ResultViewModel(e));
                }
            }
        }

        public JsonResult Buscar(Usuario.FiltroUsuario busqueda)
        {
            return Json(Usuario.buscar(busqueda), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Get(string id)
        {
            ApplicationUser user = UserManager.FindById(id);
           
            if (id != null)
            {
                return Json(new Usuario()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Nombre = user.Nombre,
                    Email = user.Email,
                        
                    ApellidoPaterno = user.ApellidoPaterno,
                    ApellidoMaterno = user.ApellidoMaterno,
                    IdRoles = user.Roles.Select(i => i.RoleId).ToList()
                }, JsonRequestBehavior.AllowGet);
            }
            else
                return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}