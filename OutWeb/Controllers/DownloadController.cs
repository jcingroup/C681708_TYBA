using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.DownloadFrontModel;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using System;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    public class DownloadController : Controller
    {
        public DownloadController()
        {
            ViewBag.IsFirstPage = false;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Download");
        }

        // 套程式-檔案下載
        // 列表
        public ActionResult Download(int? page)
        {
            page = page ?? 1;
            DownloadFrontViewModel model = new DownloadFrontViewModel();

            using (var mdu = new DownloadFrontModule())
            {
                try
                {
                    model.Result = mdu.GetList((int)page);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                }
            }

            return View(model);
        }

    }
}