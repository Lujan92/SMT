using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMT.Areas.Panel.Controllers
{
    [Authorize(Roles ="Venta,Root")]
    public class VendedoresController : Controller
    {
        // GET: Panel/Vendedores
        public ActionResult Index()
        {
            SMTDevEntities db = new SMTDevEntities();
            ViewBag.entidades = db.Entidad.ToList();

            return View();
        }

        public JsonResult ListarUsuarios(string busqueda, int? entidad, int page, int pageSize)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                using (SMTDevEntities db2 = new SMTDevEntities())
                {

                    IQueryable<ApplicationUser> query = db.Users;
                    List<Entidad> entidades = db2.Entidad.ToList();

                    if (!User.IsInRole("Root"))
                    {
                        var user = Usuario.Get(User.Identity.GetUserId());
                        var rol = db.Roles.Where(a => a.Name == "Root").Select(a => a.Id).FirstOrDefault();

                        query = query.Where(a => a.Entidad == user.Entidad && !a.Roles.Any(r=> r.RoleId == rol));
                    }
                    else  if (entidad != null)
                    {
                        query = query.Where(a => a.Entidad == entidad);
                    }

                    if (!string.IsNullOrEmpty(busqueda))
                    {
                        query = query.Where(a => a.Email.Contains(busqueda) || (a.Nombre + " " + a.ApellidoPaterno + " " + a.ApellidoMaterno).Contains(busqueda));
                    }

                    List<string> ids = query.Select(a => a.Id).ToList();

                    List<Credencial> query2 = db2.Credencial.Where(a => ids.Contains(a.IDUsuarioBeneficiario))
                                                            .OrderBy(a =>a.FechaCreacion)
                                                            .Skip((page - 1) * pageSize)
                                                            .Take(pageSize)
                                                            .ToList();

                    List<dynamic> result = new List<dynamic>();

                    foreach(var m in query2)
                    {
                        result.Add(query.Where(a => a.Id == m.IDUsuarioBeneficiario).ToList().Select(a => new
                        {
                            id = m.IDCredencial,
                            nombre = a.Nombre + " " + a.ApellidoPaterno + " " + a.ApellidoMaterno,
                            email = a.Email,
                            roles = string.Join(", ", db.Roles.Where(r => r.Users.Any(u => u.UserId == a.Id)).Select(r => r.Name).ToList()),
                            entidad = entidades.Where(e => e.Numero == a.Entidad).Select(e => e.Nombre).FirstOrDefault() ?? "Sin entidad",
                            pagada =m.FechaPagada !=null ? "Si" : "No"
                        })
                        .FirstOrDefault());
                    }


                    return Json(new
                    {
                        total = query2.Count(),
                        data = result

                    },JsonRequestBehavior.AllowGet);
                }
            }
        }

        [HttpPost]
        public JsonResult MarcarPagada(string id)
        {
            try
            {
                if (!User.IsInRole("Root"))
                    return Json(new ResultViewModel(false, "No tiene acceso a esta opción", null));

                using (SMTDevEntities db2 = new SMTDevEntities())
                {

                    Credencial credencial = db2.Credencial.FirstOrDefault(a => a.IDCredencial == id);

                    if(credencial == null)
                        return Json(new ResultViewModel(false, "Esta persona aun no cuenta con una licencia", null));

                    if (credencial.FechaPagada != null)
                        credencial.FechaPagada = null;
                    else
                        credencial.FechaPagada = DateTime.Now;
                    db2.SaveChanges();

                    return Json(new ResultViewModel(true,null,null));
                }
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }

        }
    }
}