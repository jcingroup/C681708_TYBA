using OutWeb.Models.FrontEnd.HomeMultipleModels;
using OutWeb.Modules.FrontEnd;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            ViewBag.IsFirstPage = false;
        }

        public ActionResult Index()
        {
            ViewBag.IsFirstPage = true;
            HomeFrontListDataModel model = new HomeFrontListDataModel();
            using (var homeModule = new HomeFrontModule())
            {
                model = homeModule.GetList();
            }

            return View(model);
        }

        // 靜態-聯絡我們
        public ActionResult ContactUs()
        {
            return View();
        }
    }
}