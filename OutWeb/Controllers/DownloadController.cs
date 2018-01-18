using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.DownloadFrontModel;
using OutWeb.Models.Manage.CasesModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using System;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
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

        public ActionResult Content(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("List");

            CasesDetailsDataModel model;
            using (var module = new CaseModule())
            {
                model = (module.DoGetDetailsByID((int)ID) as CasesDetailsDataModel);
            }
            if (model == null)
                return RedirectToAction("Login", "Login");
            //取檔案
            using (var fileModule = new FileModule())
            {
                model.FilesData = fileModule.GetFiles((int)model.Data.主索引, "Cases", "M");
            }
            return View(model);
        }
    }
}