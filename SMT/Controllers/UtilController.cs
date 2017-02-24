using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SMT.Models.DB;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace SMT.Controllers
{
    public class UtilController : Controller
    {
        // GET: Util
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PruebaPrint() {
            return View();
        }
        public JsonResult Health()
        {
            try {
                SMTDevEntities db = new SMTDevEntities();

                Alumno a = db.Alumno.FirstOrDefault();

                return Json("ok", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            
            
        }

        public JsonResult Version()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());


            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            DateTime fechaCompilacion = new DateTime(2000, 1, 1, 0, 0, 0);
            Assembly web = Assembly.GetExecutingAssembly();
            AssemblyName webName = web.GetName();
            string[] myVersion = webName.Version.ToString().Split('.');
            fechaCompilacion = fechaCompilacion.AddDays(double.Parse(myVersion[2]));
            fechaCompilacion = fechaCompilacion.AddSeconds(double.Parse(myVersion[3]) * 2);

            var result = new
            {
                localIP = localIP,
                version = webName.Version.ToString(),
                fechaEnsamble = fechaCompilacion.ToUniversalTime().ToString()
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}