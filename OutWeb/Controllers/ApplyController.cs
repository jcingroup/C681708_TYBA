using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.TrainModels.TrainApplyModels;
using OutWeb.Models.FrontEnd.TrainModels.TrainListModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using OutWeb.Provider;
using System;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
    public class ApplyController : Controller
    {

        public ApplyController()
        {
            ViewBag.IsFirstPage = false;
        }

        // GET: Apply
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // 套程式-線上報名
        public ActionResult List(int? page, string qry, string sort, string bdate, string edate)
        {
            TrainListViewModel model = new TrainListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.BeginDate = string.IsNullOrEmpty(bdate) ? DateTime.MinValue : Convert.ToDateTime(bdate);
            model.Filter.EndDate = string.IsNullOrEmpty(edate) ? DateTime.MaxValue : Convert.ToDateTime(edate);
            using (var module = new TrainFrontModule())
            {
                model.Result = module.GetList(model.Filter);
            }
            return View(model);
        }

        public ActionResult Content(int? trainID)
        {
            if (trainID == null)
                throw new Exception("此報名已結束或不存在");
            TrainContentModel model = new TrainContentModel();
            using (var module = new TrainFrontModule())
            {
                model = module.GetTrainContentByID((int)trainID);
            }
            //取檔案
            using (var fileModule = new FileModule())
            {
                model.FilesData = fileModule.GetFiles((int)model.ID, "Train", "M");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult GetApplyIDByTrainID(int? trainID)
        {
            bool isSuccess = true;
            string msg = string.Empty;
            int applyID = 0;
            try
            {
                using (var module = new TrainFrontModule())
                {
                    if (UserFrontProvider.Instance.User != null)
                    {
                        applyID = module.GetApplyByUserAccountAndTrainID((int)trainID);
                    }
                    else
                    {
                        isSuccess = false;
                        msg = "請先登入";
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                msg = ex.Message;
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("TrainApply", "User", new { trainID = (int)trainID, applyID = applyID });
            return Json(new { success = isSuccess, msg = msg, url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }

        // 報名表單
        public ActionResult ApplyAdd(int? trainID)
        {
            TrainApplyViewModel model = new TrainApplyViewModel();

            try
            {
                if (trainID == null)
                    throw new Exception("此報名已結束或不存在");
                if (UserFrontProvider.Instance.User == null)
                {
                    string url = Request.Url.OriginalString;
                    TempData["LoginValid"] = "請先登入再報名.";
                    return RedirectToAction("Login", "Home", new { redirectUrl = url });
                }
                //修改報名需要判斷user時否已報名過
                using (var module = new TrainFrontModule())
                {
                    module.CheckHasEnrollTrain((int)trainID);
                    model.TrainInfo = module.GetTrainContentByID((int)trainID);
                }

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("List");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ApplyAdd(TrainApplyViewModel model)
        {
            int identityID = 0;
            if (model.TrainInfo.ID == 0)
                throw new Exception("此報名已結束或不存在");
            using (var module = new TrainFrontModule())
            {
                identityID = module.SaveTrainApplyData(model.TrainInfo.ID, model.Apply);
            }
            return RedirectToAction("TrainApply", "User", new { trainID = model.TrainInfo.ID, applyID = identityID });
        }

        // ## 靜態demo
        // 活動內容
        public ActionResult contentDemo()
        {
            return View();
        }
        // 報名頁面-1
        public ActionResult Apply()
        {
            return View();
        }
        // 報名頁面-2
        public ActionResult Apply2()
        {
            return View();
        }
        // 報名頁面-3
        public ActionResult Apply3()
        {
            return View();
        }
    }
}