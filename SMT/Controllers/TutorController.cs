using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SMT.Models;
using SMT.Models.DB;
using SMT.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize]
    public class TutorController : Controller
    {
        private SMTDevEntities db = new SMTDevEntities();

        [Tutor]
        public ActionResult Index() {
            ViewBag.Alumnos = Tutor.ObtenerIndexViewModel(User.Identity.GetUserId(), db);
            return View();
        }

        [Tutor]
        public ActionResult Detalle(Guid idAlumno, int bimestre) {
            return View(Tutor.ObtenerDetallesViewModel(idAlumno, bimestre, db));
        }

        [Tutor]
        public ActionResult Imprimir(Guid id) {
            var detalles = Tutor.ObtenerImpresionDetallesViewModel(id, db);

            ViewBag.header = new HeaderGrupoReporte(detalles.IDGrupo, "Resumen alumno");

            return View(detalles);
        }

        public ActionResult NoTutorError() {
            if (User.IsInRole(Tutor.Rol)) return RedirectToAction(nameof(Index));
            else return View();
        }

        [Credencial]
        public ActionResult AgregarAlumno() {
            return View();
        }

        [HttpPost]
        [Credencial]
        public JsonResult ListarTutores(Guid idAlumno) {
            try {
                return Json(new { result = true, data = Tutor.ListarTutoresDeAlumno(idAlumno, db) });
            }
            catch (Exception ex) {
                return Json(new { result = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Credencial]
        public JsonResult QuitarAlumno(Guid idAlumno, string idTutor) {
            if (!User.IsInRole("Maestro") && !User.IsInRole("Root")) {
                return Json(new {
                    result = false,
                    message = "No tiene permiso para realizar esta acción"
                });
            }

            try {
                Tutor.QuitarAlumnoATutor(idAlumno, idTutor, db);
                return Json(new { result = true });
            }
            catch (Exception ex) {
                return Json(new { result = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Credencial]
        public async Task<JsonResult> AgregarAlumno(AgregarTutorAlumnoViewModel model) {
            if (!User.IsInRole("Maestro") && !User.IsInRole("Root")) {
                return Json(new {
                    result = false,
                    message = "No tiene permiso para realizar esta acción"
                });
            }

            if (ModelState.IsValid && !await Tutor.ExisteTutorConEmail(model.Email))
                await Invitacion.generarLicenciaYEnviarInvitacion(model.Email, Tutor.Rol);
            if (ModelState.IsValid && Tutor.TieneTutorAAlumno(model, db))
                ModelState.AddModelError(nameof(model.Email), "El tutor ya tiene al alumno registrado");
            if (ModelState.IsValid && Tutor.TieneMaximaCantidadDeTutores(model, db))
                ModelState.AddModelError(nameof(model.Email), "El alumno ya tiene demasiados tutores");

            if (!ModelState.IsValid) {
                return Json(new {
                    result = false,
                    message = string.Join("<br>", ModelState.SelectMany(o => o.Value.Errors).Select(e => e.ErrorMessage))
                });
            }

            try {
                Tutor.AgregarAlumnoATutor(model, db);
                return Json(new { result = true });
            }
            catch(Exception ex) {
                return Json(new { result = false, message = ex.Message });
            }
        }

        public class TutorAttribute : AuthorizeAttribute {
            protected override bool AuthorizeCore(HttpContextBase ctx) {
                if (ctx.User?.IsInRole(Tutor.Rol) == false) ctx.Response.Redirect("/Tutor/" + nameof(NoTutorError));
                return true;
            }
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            db.Dispose();
        }
    }
}