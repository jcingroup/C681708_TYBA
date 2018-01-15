using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.NewsFrontEndModels;
using OutWeb.Models.Manage.ManageNewsModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using System;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
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
        public ActionResult List(int? page, string qry, string bDate, string eDate, int? type)
        {
            NewsListFrontViewModel model = new NewsListFrontViewModel();
            try
            {
                using (var module = new NewsFrontModule())
                {
                    model.Filter.CurrentPage = page ?? 1;
                    model.Filter.QueryString = qry ?? string.Empty;
                    model.Filter.BeginDate = string.IsNullOrEmpty(bDate) ? DateTime.MinValue : Convert.ToDateTime(bDate);
                    model.Filter.EndDate = string.IsNullOrEmpty(eDate) ? DateTime.MaxValue : Convert.ToDateTime(eDate);
                    model.Filter.Type = type == null ? module.GetDefualtType().主索引.ToString() : type.ToString();

                    model.Result = module.GetList(model.Filter);

                    ViewBag.Subnav = "sub-news" + model.Filter.Type;
                    ViewBag.Breadcrumb = type == null ? module.GetDefualtType().分類名稱 : module.GetTypeByID((int)type).分類名稱;

                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
            return View(model);
        }

        public ActionResult Content(int? ID, int? type)
        {
            if (!ID.HasValue)
                return RedirectToAction("List");
            NewsDetailsDataModel model;
            using (var module = new NewsModule())
            {
                model = (module.DoGetDetailsByID((int)ID) as NewsDetailsDataModel);
                if (model == null)
                    return RedirectToAction("Login", "Login");
                //取檔案
                FileModule fileModule = new FileModule();
                model.FilesData = fileModule.GetFiles((int)model.Data.主索引, "News", "M");
            }
            using (var module = new NewsFrontModule())
            {
                ViewBag.BreadcrumbCenter = type == null ? module.GetDefualtType().分類名稱 : module.GetTypeByID((int)type).分類名稱;
            }

            ViewBag.Subnav = "sub-news" + type;
            ViewBag.Breadcrumb = model.Data.標題;

            return View(model);
        }
    }
}