using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VersionedRestApi.Examples.Controllers
{
    public class ExampleMvcController : Controller
    {
        public ActionResult Index()
        {
            return View("~/Views/ExampleMvc/Index.cshtml");
        }
    }
}