using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SMT.Models
{

    public class Usuario {
        #region Metadatos 
        [Required(ErrorMessage = "El email es obligatorio")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(30, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
        [Display(Name = "Usuario")]
        public string Username { get; set; }

        [StringLength(50, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(50, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
        [Display(Name = "Apelido paterno")]
        public string ApellidoPaterno { get; set; }
        [StringLength(50, ErrorMessage = "La longitud máxima de caracteres permitidos es de {1}")]
        [Display(Name = "Apellido materno")]
        public string ApellidoMaterno { get; set; }

        public string Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarPassword { get; set; }

        public List<string> IdRoles { get; set; }

        public int? Entidad { get; set; }
        #endregion

        public class FiltroUsuario {
            public string Username { get; set; }
            public string Nombre { get; set; }
            public string Email { get; set; }
            public string Roles { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
        }

        public class UsuarioResult {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Nombre { get; set; }
            public string Email { get; set; }
            public string Roles { get; set; }
            public bool Disabled { get; set; }
            public List<string> IdRoles { get; set; }
            public bool? EsEscuela { get; set; }
            public int? TotalCuentas { get; set; }
        }

        public static async Task ActualizarRoles() {
            if (HttpContext.Current == null || !HttpContext.Current.User.Identity.IsAuthenticated) return;
            var owinContext = HttpContext.Current.GetOwinContext();
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var singInManager = owinContext.Get<ApplicationSignInManager>();
            var userManager = owinContext.GetUserManager<ApplicationUserManager>();
            var authentication = owinContext.Authentication;
                authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

            var user = userManager.FindById(userId);
            await singInManager.SignInAsync(user, false, false);
        }

        public static Usuario Get(string Id) {
            ApplicationDbContext db = new ApplicationDbContext();
            ApplicationUser user = db.Users.FirstOrDefault(i => i.Id == Id);
            return Util.Cast(new Usuario(), user);
        }

        public static Usuario GetByName(string UserName) {
            ApplicationDbContext db = new ApplicationDbContext();
            return Util.Cast(new Usuario(), db.Users.FirstOrDefault(i => i.UserName == UserName));
        }


        public void editar() {
            using (ApplicationDbContext db = new ApplicationDbContext()) {
                ApplicationUser usuario = db.Users.FirstOrDefault(i => i.Id == Id);
                usuario.Nombre = Nombre;
                usuario.UserName = Email;
                usuario.Email = Email;
                usuario.ApellidoMaterno = ApellidoMaterno;
                usuario.ApellidoPaterno = ApellidoPaterno;
                usuario.EmailConfirmed = true;

                if (IdRoles != null) {
                    usuario.Roles.Clear();
                    foreach (string rol in IdRoles) {
                        IdentityRole r = db.Roles.FirstOrDefault(i => i.Id == rol);
                        var identyRol = new IdentityUserRole()
                        {
                            RoleId = r.Id,
                            UserId = usuario.Id
                        };
                        usuario.Roles.Add(identyRol);
                    }
                }

                db.SaveChanges();
            }
        }

        public void eliminar() {

            using (DB.SMTDevEntities db = new DB.SMTDevEntities()) {
                List<DB.Grupos> grupos = db.Grupos.Where(i => i.IDUsuario == Id).ToList();

                foreach (var m in grupos) {

                    db.Grupos.Remove(m);
                    db.SaveChanges();
                }

                DB.Credencial cuenta = db.Credencial.FirstOrDefault(i => i.IDUsuarioBeneficiario == Id);
                if (cuenta != null)
                    cuenta.IDUsuarioBeneficiario = null;

                db.SaveChanges();
            }

            using (ApplicationDbContext db = new ApplicationDbContext()) {
                ApplicationUser usuario = db.Users.FirstOrDefault(i => i.Id == Id);
                if (usuario != null) {
                    db.Users.Remove(usuario);
                    db.SaveChanges();
                }
            }
        }

        public bool? cambiarEstatus() {
            using (ApplicationDbContext db = new ApplicationDbContext()) {
                ApplicationUser usuario = db.Users.FirstOrDefault(i => i.Id == Id);
                usuario.Disabled = usuario.Disabled == null ? true : !usuario.Disabled;
                db.SaveChanges();
                return usuario.Disabled;
            }
        }


        public static List<RoleResult> getAllRoles() {
            var context = new ApplicationDbContext();
            var roles = context.Roles.Select(i => new RoleResult()
            {
                Id = i.Id,
                Nombre = i.Name
            }).ToList();
            return roles;
        }

        public static List<string> getRolesForUser(string id) {
            var context = new ApplicationDbContext();
            var roles = context.Roles.Where(i => i.Users.Any(a => a.UserId == id)).Select(i => i.Name).ToList();
            return roles;
        }

        public static ResultPaginado<UsuarioResult> buscar(FiltroUsuario filtro) {
            using (ApplicationDbContext db = new ApplicationDbContext()) {
                IQueryable<ApplicationUser> query = db.Users;

                if (!string.IsNullOrEmpty(filtro.Nombre))
                    query = query.Where(i => (i.ApellidoPaterno + " " + i.ApellidoMaterno + " " + i.Nombre).ToUpper().Contains(filtro.Nombre.ToUpper()));

                if (!string.IsNullOrEmpty(filtro.Username))
                    query = query.Where(i => i.UserName.ToUpper().Contains(filtro.Username.ToUpper()));

                if (!string.IsNullOrEmpty(filtro.Email))
                    query = query.Where(i => i.Email.Contains(filtro.Email));

                if (!string.IsNullOrEmpty(filtro.Roles))
                    query = query.Where(i => i.Roles.Any(a => a.RoleId == filtro.Roles));

                return new ResultPaginado<UsuarioResult>() {
                    data = query.OrderBy(i => i.Nombre)
                                .Skip((filtro.page - 1) * filtro.pageSize)
                                .Take(filtro.pageSize)
                                .ToList()
                                .Select(i => new UsuarioResult() {
                                    Id = i.Id,
                                    Username = i.UserName,
                                    Nombre = string.Format("{1} {2} {0}", i.Nombre, i.ApellidoPaterno, i.ApellidoMaterno),
                                    Email = i.Email,
                                    Roles = i.Roles != null ? string.Join(", ", getRolesForUser(i.Id)) : "",
                                    Disabled = i.Disabled != null ? i.Disabled.Value : false,
                                    EsEscuela = i.EsEscuela
                                })
                                .ToList(),
                    total = query.Count()
                };
            }
        }

        public static async Task quitarRolesByName(string userid, List<string> roles) {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                await userManager.RemoveFromRolesAsync(userid, roles.ToArray());
        }

        public static async Task agregarRolesByName(string userid, List<string> roles) {
            var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                await userManager.AddToRolesAsync(userid, roles.ToArray());
        }

        public static void agregarRoles(string username, List<string> roles) {
            using (ApplicationDbContext db = new ApplicationDbContext()) {
                ApplicationUser user = db.Users.FirstOrDefault(i => i.UserName == username);
                foreach (string rol in roles) {
                    IdentityRole r = db.Roles.FirstOrDefault(i => i.Id == rol);
                    var identyRol = new IdentityUserRole()
                    {
                        RoleId = r.Id,
                        UserId = user.Id
                    };
                    user.Roles.Add(identyRol);
                }

                db.SaveChanges();
            }
        }

        public static bool esEscuela(string id) {
            using (ApplicationDbContext db = new ApplicationDbContext()) {
                var es = db.Users.Where(i => i.Id == id).Select(u => u.EsEscuela).FirstOrDefault();
                return es == true;
            }
        }

        public static void verComoUsuario(string usuarioActual, string usuarioVer) {
            using (ApplicationDbContext db = new ApplicationDbContext()) {
                var user = db.Users.FirstOrDefault(i => i.Id == usuarioActual);

                if (user != null) {
                    user.ViendoUsuario = usuarioVer;
                    db.SaveChanges();
                }
                else if (usuarioVer == null) {
                    user.ViendoUsuario = null;
                    db.SaveChanges();
                }

            }
        }

        public static string getIDVisor(string usuario) {
            using (ApplicationDbContext db = new ApplicationDbContext()) {
                var user = db.Users.FirstOrDefault(i => i.Id == usuario);

                if (user != null && user.ViendoUsuario != null && user.ViendoUsuario != "")
                    return user.ViendoUsuario;
                else
                    return usuario;

            }
        }

        public static ApplicationUser getVisor(string usuarioActual) {
            ApplicationDbContext db = new ApplicationDbContext();
            var user = db.Users.FirstOrDefault(i => i.Id == usuarioActual);

            if (user == null || user.ViendoUsuario == null)
                return null;

            ApplicationUser visor = db.Users.FirstOrDefault(i => i.Id == user.ViendoUsuario);

            return visor;


        }




        /// <summary>
        /// Envía una invitación a un email y crea un usuario en base a ese email
        /// </summary>
        /// <param name="email"></param>
        public static void enviarInvitacionRegistro(string email)
        {

        }

    }

    public static class UsuarioExtensions {
        public static Usuario Get(this IPrincipal user) {
            return Usuario.Get(user.Identity.GetUserId());
        }
        /// <summary>
        /// Menús a mostrar
        /// </summary>
        public static Secciones GetSecciones(this IPrincipal user) {
            if (!user.Identity.IsAuthenticated) return new Secciones();
            var root = user.IsInRole("Root");
            return new Secciones {
                Panel = root,
                Grupos = root || user.IsInRole("Maestro"),
                Tutor = root || user.IsInRole(Tutor.Rol),
                Licencias = root || Usuario.esEscuela(user.Identity.GetUserId())
            };
        }

        public class Secciones
        {
            public bool Panel { get; internal set; }
            public bool Grupos { get; internal set; }
            public bool Tutor { get; internal set; }
            public bool Licencias { get; internal set; }
            public int TotalEnabled => new[] { Panel, Grupos, Tutor, Licencias }.Count(o => o);
        }
    }
}