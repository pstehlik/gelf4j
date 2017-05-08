using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JSNLogDemo_Log4Net.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
		
log4net.ILog log = log4net.LogManager.GetLogger("serverlogger");
log.Info("info server log message");

            return View();
        }
    }
}



