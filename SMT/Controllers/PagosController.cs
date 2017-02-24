using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SMT.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        // GET: Pagos
        public ActionResult Index(int? error)
        {
            if(error != null)
            {
                return View("Error",error.Value);
            }
            return View();
        }

        public ActionResult Error(int error)
        {

            return View(error);
        }
    }
}