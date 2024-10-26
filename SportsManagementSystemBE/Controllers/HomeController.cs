using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SportsManagementSystemBE.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
           // ctrl + h using System =using System; using Newtonsoft.Json; using Newtonsoft.Json;
            ///[JsonIgnore] public virtual=[JsonIgnore] [JsonIgnore] public virtual.

            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
