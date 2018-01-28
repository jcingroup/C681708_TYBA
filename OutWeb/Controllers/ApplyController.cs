using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.ApplyModels;
using OutWeb.Models.FrontEnd.TrainModels.TrainApplyModels;
using OutWeb.Models.FrontEnd.TrainModels.TrainListModels;
using OutWeb.Models.Manage.ActivityModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using OutWeb.Provider;
using System;
using System.Web.Mvc;
using System.Linq;
using OutWeb.Models.FrontEnd.DownloadFrontModel;

namespace OutWeb.Controllers
{
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
        public ActionResult List(int? page)
        {
            page = page ?? 1;
            ApplyListViewModel model = new ApplyListViewModel();
            using (var applymodule = new ApplyFrontModule())
            {
                model = applymodule.GetList((int)page);
            }
            return View(model);
        }

        public ActionResult Content(int? id)
        {
            if (id == null)
                throw new Exception("查無該活動賽事");
            ApplyListContentModel model = new ApplyListContentModel();
            using (var applymodule = new ApplyFrontModule())
            {
                var listData = applymodule.GetList(0, true).ListData;
                if (listData.Count > 0)
                {
                    model.Data = listData.Where(o => o.ID == id).FirstOrDefault();
                    if (model.Data == null)
                    {
                        TempData["ErrorMsg"] = "查無該活動賽事";
                        return RedirectToAction("List");

                    }
                    //取檔案
                    using (var fileModule = new FileModule())
                    {
                        model.Files = fileModule.GetFiles((int)model.Data.ID, "Activity", "F");
                    }
                }
            }
            return View(model);
        }



        // ## 靜態demo
        // 活動內容
        public ActionResult contentDemo()
        {
            return View();
        }
        // 報名頁面-1
        public JsonResult ValidTeamName(int actId, int groupId, string teamName)
        {
            bool success = true;
            string msg = string.Empty;
            using (var activityModule = new ActivityModule())
            {
                bool valid = activityModule.ValidHasSeamTeamName(actId, groupId, teamName);
                if (valid)
                {
                    success = false;
                }
            }
            return Json(new { success = success }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Apply(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("List");
            }
            ApplyViewDetailsModel model = new ApplyViewDetailsModel();
            using (var applyModule = new ApplyFrontModule())
            {
                model = applyModule.GetViewActivityDetailsByID((int)id);
                if (model == null)
                {
                    TempData["ErrorMsg"] = "查無該活動賽事";
                    return RedirectToAction("List");
                }
            }
            return View(model);
        }
        // 報名頁面-2
        [HttpPost]
        public ActionResult Apply2(ApplyDataModel model)
        {
            using (var applyModule = new ApplyFrontModule())
            {
                var details = applyModule.GetViewActivityDetailsByID(model.ActivityID);
                model.ApplyGroupName = details.ActivityGroup.Where(o => o.GroupID == model.ApplyGroupID).First().GroupName;
                if (model == null)
                    TempData["ErrorMsg"] = "查無該活動賽事";
            }

            Session["ApplyInfo"] = model;
            return View(model);
        }


        // 報名頁面-3
        public ActionResult Apply3()
        {
            if (Session["ApplyInfo"] == null)
            {
                TempData["ErrorMsg"] = "網頁閒置過久,請重新填寫";
                return RedirectToAction("ApplyList");
            }
            ApplyDataModel model = Session["ApplyInfo"] as ApplyDataModel;
            //資料庫存檔
            using (var applyModule = new ApplyFrontModule())
            {
                ApplyDataModel result = applyModule.SaveApply(model);
                return View(result);
            }
        }
    }
}