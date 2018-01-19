using System.Web.Mvc;

namespace OutWeb.Controllers
{
    public class ManagerController : Controller
    {
        public ManagerController()
        {
            ViewBag.IsFirstPage = false;
        }

        // 首頁banner-demo
        public ActionResult BannerList()
        {
            return View();
        }
        public ActionResult BannerEdit()
        {
            return View();
        }

        // 本會簡介-demo
        public ActionResult AboutUs()
        {
            return View();
        }

        // 比賽成績公告-demo
        public ActionResult ResultList()
        {
            return View();
        }
        public ActionResult ResultEdit()
        {
            return View();
        }

        // 檔案下載-demo
        public ActionResult DownloadList()
        {
            return View();
        }
        public ActionResult DownloadEdit()
        {
            return View();
        }

        // 活動管理-demo
        public ActionResult ActivityList()
        {
            return View();
        }
        public ActionResult ActivityEdit()
        {
            return View();
        }
        // 線上報名-demo
        public ActionResult ApplyList()
        {
            return View();
        }
        public ActionResult ApplyEdit()
        {
            return View();
        }
    }
}