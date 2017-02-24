using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMT.Areas.Panel.Controllers
{
    [Authorize(Roles ="Root")]
    public class LicenciasController : Controller
    {
        // GET: Panel/Licencias
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult CargarDatos(string busqueda, int page, int pageSize)
        {
            var id = User.Identity.GetUserId();
            using (ApplicationDbContext db = new ApplicationDbContext())
            {

                IQueryable<Credencial> cuenstas = Credencial.listarPrincipales();

                if (!string.IsNullOrEmpty(busqueda)) {
                    using (ApplicationDbContext adb = new ApplicationDbContext())
                    {
                        busqueda = busqueda.ToLower();
                        List<string> ids = adb.Users.Where(a => a.Email.ToLower().Contains( busqueda) || (a.Nombre+" " + a.ApellidoPaterno + " " + a.ApellidoMaterno).ToLower().Contains(busqueda)).Select(a => a.Id).ToList();
                        cuenstas = cuenstas.Where(a => ids.Contains(a.IDUsuarioPaga));
                    }
                }

                var data = new
                {
                    id = id,
                    total = cuenstas.Count(),
                    cuentas = cuenstas.OrderBy(a => a.IDCredencial)
                                    .Skip((page-1) *pageSize)
                                    .Take(pageSize)
                                    .ToList().Select(a => new
                                    {
                                        persona = db.Users.Where(u => u.Id == a.IDUsuarioPaga).Select(u => u.Email).FirstOrDefault(),
                                        id = a.IDCredencial,
                                        usuario = a.IDUsuarioBeneficiario,
                                        fecha = a.FechaCreacion.ToString("dd/MM/yyyy"),
                                        vigencia = a.FechaVigencia.ToString("dd/MM/yyyy"),
                                        activo = a.Activo,
                                        licencias = Credencial.listarPorUsuario(a.IDUsuarioPaga).Where(i => i.EsPrincipal == false).Count(),
                                        tipo =a.Tipo
                                    })
                                    .ToList()
                            
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult Agregar(string id,  string tipo, int licencias)
        {
            try
            {

                using (SMTDevEntities db = new SMTDevEntities())
                {
                    Credencial c = db.Credencial.FirstOrDefault(i => i.IDCredencial == id);

                    if (c == null)
                        return Json(new ResultViewModel(false, "No se ha encontrado la licencia", null));

                    for (int i = 0; i < licencias; i++)
                        Credencial.generarCredencial(c.IDUsuarioPaga,tipo);
                };

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public async Task<JsonResult> Generar(string email, string tipoPrincipal, string tipo, int? licencias)
        {
            try
            {
                string id;
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    id = db.Users.Where(a => a.Email == email).Select(a => a.Id).FirstOrDefault();


                    if (string.IsNullOrEmpty(id))
                    {

                        await Invitacion.generarLicenciaYEnviarInvitacion(email, tipoPrincipal.ToUpper());
                        id = db.Users.Where(a => a.Email == email).Select(a => a.Id).FirstOrDefault();
                    }

                    switch (tipoPrincipal)
                    {
                        case "escuela":
                        case "maestro":
                        case "tutor":
                            Credencial.generarCredencial(id, tipoPrincipal.ToUpper(), true);
                            break;
                        default:
                            return Json(new ResultViewModel(false, "Tipo de licencia incorrecto", null));
                    }


                    for (int i = 0; i < licencias; i++)
                    {
                        Credencial.generarCredencial(id, tipo);
                    }
                }

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Activar(string id)
        {
            try
            {
                Credencial.cambiarEstatusPrincipal(id);


                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

    

        [HttpPost]
        public JsonResult Eliminar(string id)
        {
            try
            {
                Credencial.eliminarPrincipal(id);

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Renovar(string id, DateTime fecha)
        {
            try
            {
                Credencial.renovarcredencial(id,fecha);

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult eliminarSinAsingar(string id)
        {
            try
            {
                Credencial.eliminarSinAsingar(id);

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }
    }
}