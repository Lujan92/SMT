using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMT.Models.DB;
using SMT.Models;

namespace SMT.Controllers
{
    [Authorize,Credencial]
    public class GruposController : Controller
    {
        // GET: Grupos
        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult CargarBarra(int tipo = 1, int page =0)
        {
            var UserActual =Usuario.getIDVisor(User.Identity.GetUserId());
            var Gr = Grupos.getGrupos(UserActual,tipo,page);
            return PartialView("_BarraGrupos", Gr);
        }
        public ActionResult CargarGrupo(Guid? ID = null, int Tipo=0)
        {            
            Grupos res = Grupos.get(ID ?? default(Guid));
            var ano1 = "";
            var ano2 = "";
            if (res == null)
            {                
                res = new Grupos();
                res.Grupo = "";
            }
            if (Tipo!=0)
            {
                res.EsTaller = true;
            }

            if( Util.toHoraMexico(DateTime.Now).Month >= 7 && Util.toHoraMexico(DateTime.Now).Day >= 15)
            {
                ano1 = Util.toHoraMexico(DateTime.Now).ToString("yyyy");
                ano2 = Util.toHoraMexico(DateTime.Now).AddYears(1).ToString("yyyy");
            }
            else
            {
                ano1 = Util.toHoraMexico(DateTime.Now).AddYears(-1).ToString("yyyy");
                ano2 = Util.toHoraMexico(DateTime.Now).ToString("yyyy");
            }            
            res.Ciclo =res.Ciclo==null? ano1 + "-" + ano2:res.Ciclo;

            return PartialView("_Grupos", res);
        }

        public JsonResult GuardarGrupo(Grupos res)
        {
            try
            {
                if (res.IDGrupo != default(Guid))
                {
                    return Json(res.editar(), JsonRequestBehavior.AllowGet);
                }
                res.IDUsuario= User.Identity.GetUserId();
                var IDGrupo = res.crear();
                for (int i = 1; i <= 5; i++)
                {
                    Bimestres bim = new Bimestres();
                    bim.IDGrupo = IDGrupo;
                    bim.Bimestre = i;
                    bim.crear();
                }
                return Json(IDGrupo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.InnerException != null ? e.InnerException.Message : e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EliminarGrupo(Guid ID)
        {
            return Json(Grupos.eliminar(ID));
        }
        [HttpPost]
        public JsonResult ArchivarGrupo(Guid ID)
        {
            return Json(Grupos.archivar(ID));
        }

        [HttpPost]
        public JsonResult Clonar(Guid ID)
        {
            try
            {
                return Json(new ResultViewModel(true,null, Grupos.clonar(ID, User.Identity.GetUserId())));
            }
            catch(Exception e)
            {
                return Json(new ResultViewModel(e));
            }
        }
    }
}