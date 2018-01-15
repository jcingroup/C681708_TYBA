using System.Web.Mvc;

namespace OutWeb.Controllers
{
    public class ResultController : Controller
    {
        public ResultController()
        {
            ViewBag.IsFirstPage = false;
        }

        // GET: Result
        public ActionResult Index()
        {
            return View("List");
        }

        // 比賽成績公告
        public ActionResult List()
        {
            return View();
        }
        public ActionResult Content()
        {
            return View();
        }
    }
}