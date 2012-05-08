using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using au.id.cxd.Text.SpellingService.Models;

namespace au.id.cxd.Text.SpellingService.Controllers
{
    public class DemoSpellingController : Controller
    {
        //
        // GET: /DemoSpelling/

        public ActionResult Index()
        {
            return View("DemoSpelling", new DemoSpellingModel());
        }

        // /DemoSpelling/CheckSpelling/
        [HttpPost]
        public ActionResult CheckSpelling(DemoSpellingModel demo)
        {
            demo.CorrectSpelling();

            return View("DemoSpelling", demo);
        }

    }
}
