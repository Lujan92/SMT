using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize, Credencial]
    public class LicenciaController : Controller
    {
        // GET: Maestros
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult CargarDatos(string busqueda,string tipo, int page, int pageSize)
        {
            var id = User.Identity.GetUserId();
            using (ApplicationDbContext db = new ApplicationDbContext())
            {

                IQueryable<Credencial> cuenstas = Credencial.listarPorUsuario(id).Where(a => a.EsPrincipal == false);

                if (!string.IsNullOrEmpty(tipo))
                {
                    cuenstas = cuenstas.Where(a => a.Tipo == tipo);
                }

                if (!string.IsNullOrEmpty(busqueda))
                {
                    using (ApplicationDbContext adb = new ApplicationDbContext())
                    {
                        List<string> ids = adb.Users.Where(a => a.Email == busqueda || (a.Nombre + " " + a.ApellidoPaterno + " " + a.ApellidoMaterno).Contains(busqueda)).Select(a => a.Id).ToList();
                        cuenstas = cuenstas.Where(a => ids.Contains(a.IDUsuarioBeneficiario));
                       
                    }
                }

                var data = new
                {
                    id = id,
                    total = cuenstas.Count(),
                    cuentas = cuenstas.OrderBy(a => a.IDCredencial)
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize).ToList()
                                        .Select(a => new
                                        {
                                            persona = db.Users.Where(u => u.Id == a.IDUsuarioBeneficiario).Select(u => u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno).FirstOrDefault(),
                                            id = a.IDCredencial,
                                            usuario = a.IDUsuarioBeneficiario,
                                            fecha = a.FechaCreacion.ToString("dd/MM/yyyy"),
                                            vigencia = a.FechaVigencia.ToString("dd/MM/yyyy"),
                                            activo = a.Activo,
                                            tipo = a.Tipo,
                                           
                                        })
                                        .ToList()
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult EstablecerVizor(string usuario)
        {
            try
            {

                

                Usuario.verComoUsuario(User.Identity.GetUserId(), usuario);
                return Json(new ResultViewModel(true,null,null));
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        public JsonResult GetVisorActual()
        {
            return Json(Usuario.getVisor(User.Identity.GetUserId()),JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Asignar(string id, string email)
        {
            try
            {

                string benficiario = "";
      
                
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    benficiario = db.Users.Where(i => i.Email == email  ).Select(a => a.Id).FirstOrDefault();
                   

                }
                
                    if (string.IsNullOrEmpty(benficiario))
                    {
                    throw new Exception("Correo no valido o no registrado en la plataforma");
                    // Invitacion.asignarLicenciaYEnviarInvitacion(email, id);
                }
                    else
                        Credencial.asignar(User.Identity.GetUserId(), id, benficiario);


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
                Credencial.cambiarEstatus(User.Identity.GetUserId(), id);
               

                return Json(new ResultViewModel(true,null,null));
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult renovar(string id, DateTime fecha)
        {
            try
            {
                Credencial.renovarcredencial(id, fecha);


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
               Credencial.eliminarbeneficiario(User.Identity.GetUserId(), id);

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }

        [HttpPost]
        public JsonResult Eliminar2(string id)
        {
            try
            {
                Credencial.eliminarbeneficiario2(User.Identity.GetUserId(), id);

                return Json(new ResultViewModel(true, null, null));
            }
            catch (Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }


    }
}