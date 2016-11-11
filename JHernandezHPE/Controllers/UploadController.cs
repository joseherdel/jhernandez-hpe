using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JHernandezHPE.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/Championship"),
                                               Path.GetFileName("tournament01.txt"));
                    file.SaveAs(path);
                    TempData["successMessage"] = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    TempData["errorMessage"] = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                TempData["errorMessage"] = "You have not specified a file.";
            }
            return RedirectToAction("Index", "Home");
        }

    }
}