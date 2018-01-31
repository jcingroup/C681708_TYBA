using OutWeb.ActionFilter;
using OutWeb.Models.Manage.ManageNewsModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using System;
using System.Web.Mvc;
using System.Linq;
using System.Web;

namespace OutWeb.Controllers
{
    public class NewsController : Controller
    {
        public NewsController()
        {
            ViewBag.IsFirstPage = false;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // 套程式-能源新聞資訊
        // 列表
        [HttpGet]
        public ActionResult List(int? page)
        {
            NewsListViewModel model = new NewsListViewModel();
            model.Filter.CurrentPage = page ?? 1;

            using (var module = new NewsModule())
            {
                model.Result = module.DoGetList(model.Filter);
                model.Result.Data = model.Result.Data.Where(o => o.DISABLE == false).ToList();
            }

            return View(model);
        }

        public ActionResult Content(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("List");

            NewsDetailsDataModel model = new NewsDetailsDataModel();
            using (var module = new NewsModule())
            {
                model = module.DoGetDetailsByID((int)ID);
                if (model.Data == null)
                    return RedirectToAction("List");
                model.Data.CONTENT = HttpUtility.HtmlDecode(model.Data.CONTENT);
            }
            return View(model);
        }
    }
}