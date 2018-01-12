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
            return View("AboutUs");
        }

        public ActionResult AboutUs()        {
            
            return View();
        }
    }
}