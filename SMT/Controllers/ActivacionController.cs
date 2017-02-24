using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using SMT.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize]
    public class ActivacionController : Controller
    {
        private SMTDevEntities db = new SMTDevEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ActivacionViewModel model) {
            var remove = new List<string>();
            var add = new List<string>();

            if (model.Tutor && !User.IsInRole(Models.Tutor.Rol)) add.Add("Tutor");
            if (!model.Tutor && User.IsInRole("Tutor")) remove.Add("Tutor");
            if (!model.Maestro && User.IsInRole("Maestro")) remove.Add("Maestro");
            if (remove.Any() || add.Any()) {
                if(remove.Any())
                    await Usuario.quitarRolesByName(User.Identity.GetUserId(), remove);
                if(add.Any())
                    await Usuario.agregarRolesByName(User.Identity.GetUserId(), add);

                await Usuario.ActualizarRoles();
            }

            if (model.Tutor && !User.IsInRole("Tutor")) {
                return RedirectToAction(nameof(Tutor));
            }
            else { 
                if (model.Maestro && !User.IsInRole("Maestro")) {
                    return RedirectToAction(nameof(Maestro));
                }
                else {
                    if (model.Escuela && !User.GetSecciones().Licencias) {
                        return RedirectToAction(nameof(Escuela));
                    }
                    else if(!model.Escuela) { 
                        using (var adb = new ApplicationDbContext()) {
                            var id = User.Identity.GetUserId();
                            var user = adb.Users.FirstOrDefault(o => o.Id == id);
                            if (user != null) {
                                user.EsEscuela = false;
                                adb.SaveChanges();
                            }
                        }
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult Tutor() {
            return View();
        }

        public ActionResult Maestro() {
            ViewBag.Escuela = false;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Maestro(ActivacionPagoViewModel model) {
            await Usuario.agregarRolesByName(User.Identity.GetUserId(), new List<string> { "Maestro" });
            var id = User.Identity.GetUserId();

            if (!Credencial.tieneCredencialValida(id)) {
                var vigencia = DateTime.Now;

                switch (model.Paquete) {
                    case 0:
                        vigencia = vigencia.AddMonths(1);
                        break;
                    case 1:
                        vigencia = vigencia.AddMonths(3);
                        break;
                    case 2:
                        vigencia = vigencia.AddMonths(4);
                        break;
                    case 3:
                        vigencia = vigencia.AddMonths(6);
                        break;
                    case 4:
                        vigencia = vigencia.AddMonths(12);
                        break;
                }

                try {
                    Credencial.generarCredencial(id, TiposCredenciales.MAESTRO, true, vigencia);
                }
                catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }

            await Usuario.ActualizarRoles();

            return View("ExitoActivacionMaestro");
        }

        public ActionResult Escuela() {
            ViewBag.Escuela = true;
            return View("Maestro");
        }

        [HttpPost]
        public ActionResult Escuela(ActivacionPagoViewModel model) {
            var id = User.Identity.GetUserId();
            using (var adb = new ApplicationDbContext()) {
                var user = adb.Users.FirstOrDefault(o => o.Id == id);
                if (user != null) {
                    user.EsEscuela = true;
                    adb.SaveChanges();
                }
            }

            var tiene = Credencial.tieneCredencialValida(id);
            var vigencia = DateTime.Now;
            int i = 0;

            switch (model.Paquete) {
                case 0:
                    vigencia = vigencia.AddMonths(1);
                    break;
                case 1:
                    vigencia = vigencia.AddMonths(3);
                    break;
                case 2:
                    vigencia = vigencia.AddMonths(4);
                    break;
                case 3:
                    vigencia = vigencia.AddMonths(6);
                    break;
                case 4:
                    vigencia = vigencia.AddMonths(12);
                    break;
            }

            try {
                if (!tiene) {
                    Credencial.generarCredencial(id, TiposCredenciales.ESCUELA, true, vigencia);
                    i++;
                }

                for (; i < model.Licencias; i++) {
                    Credencial.generarCredencial(id, TiposCredenciales.MAESTRO, false, vigencia);
                }
            } catch(Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            
            return View("ExitoActivacionEscuela");
        }
    }
}