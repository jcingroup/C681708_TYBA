using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    public class AboutUsController : Controller
    {
        public AboutUsController()
        {
            ViewBag.IsFirstPage = false;
        }

        // GET: AboutUs
        public ActionResult Index()
        {
            return RedirectToAction("AboutUs");
        }

        public ActionResult AboutUs()
        {
            string content = string.Empty;
            using (EditorModule editorModule = new EditorModule())
            {
                content = HttpUtility.HtmlDecode(editorModule.GetContent());
            }
            ViewData["Content"] = content;
            return View();
        }
    }
}