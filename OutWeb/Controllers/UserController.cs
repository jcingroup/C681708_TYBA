using OutWeb.ActionFilter;
using OutWeb.Authorize;
using OutWeb.Models.FrontEnd.QuestionnairesModels;
using OutWeb.Models.FrontEnd.TrainModels.TrainApplyModels;
using OutWeb.Models.FrontEnd.TrainModels.TrainListModels;
using OutWeb.Models.Manage.QuestionnairesModels;
using OutWeb.Modules.FontEnd;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using OutWeb.Provider;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
    [AuthAttributeFront]
    public class UserController : Controller
    {
        public UserController()
        {
            ViewBag.IsFirstPage = false;
        }

        // GET: User
        // 用戶登入後的頁面
        public ActionResult Index()
        {
            return View("TrainList");
        }

        // 研討會報名列表
        public ActionResult TrainList(int? page, string qry, string sort, string bdate, string edate)
        {
            TrainListViewModel model = new TrainListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.BeginDate = string.IsNullOrEmpty(bdate) ? DateTime.MinValue : Convert.ToDateTime(bdate);
            model.Filter.EndDate = string.IsNullOrEmpty(edate) ? DateTime.MaxValue : Convert.ToDateTime(edate);
            using (var trainModule = new TrainFrontModule())
            {
                model.Result = trainModule.GetListByUser(model.Filter);
            }
            return View(model);
        }

        // 報名內頁- 已報名的可編輯，未報名的會自動帶驗證值
        public ActionResult TrainApply(int? trainID)
        {
            TrainApplyViewModel model = new TrainApplyViewModel();
            try
            {
                if (trainID == null)
                    throw new Exception("此研討會已結束或不存在");
        
                if (UserFrontProvider.Instance.User == null)
                {
                    string url = Request.Url.OriginalString;
                    return RedirectToAction("Login", "Home", new { redirectUrl = url });
                }
                using (var trainModule = new TrainFrontModule())
                {
                    int applyID = trainModule.GetApplyByUserAccountAndTrainID((int)trainID);

                    model.TrainInfo = trainModule.GetTrainContentByID((int)trainID);
                    model.Apply = trainModule.GetTrainApplyByID((int)trainID, (int)applyID);
                }

                if (model.Apply == null)
                    throw new Exception("無法取得此報名資料，已取消報名或被移除");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("TrainList", "User");
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult TrainApply(TrainApplyViewModel model)
        {
            try
            {
                using (var trainModule = new TrainFrontModule())
                {
                    var chkApplyExist = trainModule.GetTrainApplyByID((int)model.TrainInfo.ID, (int)model.Apply.ID);
                    if (chkApplyExist == null)
                        throw new Exception("無法取得此報名資料，已取消報名或被移除");
                    trainModule.SaveTrainApplyDataFromUser(model.Apply);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("TrainList", "User");
            }
            return RedirectToAction("TrainApply", "User", new { trainID = model.TrainInfo.ID, applyID = model.Apply.ID });
        }

        [HttpPost]
        public ActionResult DeleteApply(int trainID, int applyID)
        {
            bool isSuccess = true;
            using (var trainModule = new TrainApplyModule())
            {
                isSuccess = trainModule.DeleteApply(trainID, applyID);
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("TrainList", "User");
            return Json(new { success = isSuccess, url = redirectUrl });
        }

        // 問卷列表

        public ActionResult QuestionList(int? page)
        {
            QuestListViewModel model = new QuestListViewModel();
            model.Filter.Page = page ?? 1;
            QuestionnairesFrontModule questionnairesModule = new QuestionnairesFrontModule();
            model = questionnairesModule.GetListByUser(model.Filter.Page);
            questionnairesModule.Dispose();
            return View(model);
        }

        // 問卷內頁- 已填寫的可編輯，未報名的會自動帶驗證值
        public ActionResult QuestionApply(int? ID)
        {
            OutWeb.Modules.Manage.QuestionnairesModule mdu =
                   new Modules.Manage.QuestionnairesModule();
            QuestionDetailsDataModel model = new QuestionDetailsDataModel();
            if (ID == null)
                return RedirectToAction("List");
            try
            {
                model = mdu.DoGetDetailsByUserWitnEdit((int)ID);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("QuestionList", "User");
            }
            if (model.IsSignIn != null && model.IsSignIn.Equals("on"))
            {
                if (UserFrontProvider.Instance.User == null)
                {
                    TempData["LoginValid"] = "請先登入再填寫問卷.";
                    return RedirectToAction("Login", "Home");
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult QuestionApply(int? ID, List<QuestionDetailsAnswerModel> Answer)
        {
            bool success = true;
            string messages = "問卷填寫完成.";
            string redirectUrl = string.Empty;
            try
            {
                QuestionnairesFrontModule questionnairesModule = new QuestionnairesFrontModule();
                questionnairesModule.DeleteAllAnswerByUser((int)ID);
                questionnairesModule.InsertAnswer((int)ID, Answer);
                questionnairesModule.Dispose();
                redirectUrl = new UrlHelper(Request.RequestContext).Action("QuestionList", "User");
            }
            catch (System.Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            return Json(new { success = success, messages = messages, url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }
    }
}