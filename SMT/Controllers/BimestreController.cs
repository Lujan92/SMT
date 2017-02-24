using Microsoft.AspNet.Identity;
using SMT.Models;
using SMT.Models.DB;
using System;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize, Credencial]
    public class BimestreController : Controller
    {
        // GET: Bimestre
        public ActionResult Index(Guid grupo, int bimestre)
        {
            if (!Bimestres.validarMaestro(grupo,bimestre, Usuario.getIDVisor(User.Identity.GetUserId())))
            {
                return View("SinAcceso");
            }
            else
            {
                BimestreResult bim = new BimestreResult()
                {
                    bimestre = bimestre,
                    id = grupo.ToString()
                };

                Grupos grupos = Grupos.get(grupo);

                bim.materia = grupos.Materia;
                bim.grupo = grupos.Grado + grupos.Grupo;
                bim.EsTaller = grupos.EsTaller;
                ViewBag.header = new HeaderGrupoReporte(grupo);

                return View(bim);
            }
              
        }

        public ActionResult ImprimirControl(Guid grupo, int bimestre)
        {
            BimestreResult bim = new BimestreResult()
            {
                bimestre = bimestre,
                id = grupo.ToString()
            };

            Grupos grupos = Grupos.get(grupo);

            bim.materia = grupos.Materia;
            bim.grupo = grupos.Grado + grupos.Grupo;
            bim.EsTaller = grupos.EsTaller;
            ViewBag.header = new HeaderGrupoReporte(grupo);
            return PartialView(bim);
        }


        public class BimestreResult
        {
            public string id { get; set; }
            public string grupo { get; set; }
            public string materia { get; set; }
            public int bimestre { get; set; }
            public bool? EsTaller { get; set; }
        }
    }
}