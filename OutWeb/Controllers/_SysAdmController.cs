using OutWeb.ActionFilter;
using OutWeb.Authorize;
using OutWeb.Models.Manage.ActivityModels;
using OutWeb.Models.Manage.ApplyMaintainModels;
using OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels;
using OutWeb.Models.Manage.ApplyMaintainModels.ApplyDetailsModels.ApplyModalModels;
using OutWeb.Models.Manage.BannerModels;
using OutWeb.Models.Manage.DownloadModels;
using OutWeb.Models.Manage.ManageNewsModels;
using OutWeb.Models.Manage.ResultModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [Auth]
    [ErrorHandler]
    [CheckFolder]
    public class _SysAdmController : Controller
    {
        public _SysAdmController()
        {
            ViewBag.IsFirstPage = false;
        }

        #region 籃委會

        // 檔案下載
        public ActionResult DownloadList(int? page, string qry, string sort, string disable, string pDate)
        {
            DownLoadListViewModel model = new DownLoadListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Disable = disable ?? string.Empty;
            model.Filter.PublishDate = pDate;

            using (DownloadModule module = new DownloadModule())
            {
                model.Result = module.DoGetList(model.Filter);
            }

            return View(model);
        }

        public ActionResult DownloadAdd()
        {
            DownloadDetailsModel defaultModel = new DownloadDetailsModel();
            defaultModel.Disable = false;
            defaultModel.PublishDateStr = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.Sort = 1;
            return View(defaultModel);
        }

        public ActionResult DownloadEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("DownloadList");
            DownloadDetailsModel model = new DownloadDetailsModel();
            using (DownloadModule module = new DownloadModule())
            {
                model = module.DoGetDetailsByID((int)ID);
            }
            FileModule fileModule = new FileModule();
            model.Files = fileModule.GetFiles((int)model.ID, "Download", "F");
            return View(model);
        }

        [HttpPost]
        public ActionResult DownloadSave(DownloadDataModel model)
        {
            int id = 0;
            using (DownloadModule module = new DownloadModule())
            {
                if (model.OldFilesId.Count == 0 && model.Files.Count == 0)
                {
                    TempData["UndefinedFile"] = "請上傳檔案";
                    return RedirectToAction("DownloadEdit", new { ID = (int?)null });
                }
                id = module.DoSaveData(model);
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("DownloadEdit", "_SysAdm", new { ID = id });
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult DownloadDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                using (DownloadModule module = new DownloadModule())
                {
                    module.DoDeleteByID((int)ID);
                }
                messages = "刪除成功";
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        // 首頁banner
        public ActionResult BannerList(int? page, string qry, string sort, string disable, string pDate)
        {
            BannerListViewModel model = new BannerListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Disable = disable ?? string.Empty;
            model.Filter.PublishDate = pDate;

            using (BannerModule module = new BannerModule())
            {
                model.Result = module.DoGetList(model.Filter);
            }

            return View(model);
        }

        public ActionResult BannerAdd()
        {
            BannerDetailsModel model = new BannerDetailsModel();
            model.Disable = false;
            model.Sort = 1;
            return View(model);
        }

        public ActionResult BannerEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("BannerList");
            BannerDetailsModel model = new BannerDetailsModel();
            using (BannerModule module = new BannerModule())
            {
                model = module.DoGetDetailsByID((int)ID);
            }
            FileModule fileModule = new FileModule();
            model.Files = fileModule.GetFiles((int)model.ID, "Banner", "F");
            return View(model);
        }

        [HttpPost]
        public ActionResult BannerSave(BannerDataModel model)
        {
            int id = 0;
            using (BannerModule module = new BannerModule())
            {
                if (model.OldFilesId.Count == 0 && model.Files.Count == 0)
                {
                    TempData["UndefinedFile"] = "請上傳檔案";
                    return RedirectToAction("BannerEdit", new { ID = (int?)null });
                }
                id = module.DoSaveData(model);
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("BannerEdit", "_SysAdm", new { ID = id });
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult BannerDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                using (BannerModule module = new BannerModule())
                {
                    module.DoDeleteByID((int)ID);
                }
                messages = "刪除成功";
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        // 本會簡介
        [HttpGet]
        public ActionResult AboutUs()
        {
            string content = string.Empty;
            using (EditorModule editorModule = new EditorModule())
            {
                content = editorModule.GetContent();
            }
            ViewData["Content"] = content;
            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AboutUs(FormCollection form)
        {
            string content = form["contenttext"];
            if (form == null)
                return View();
            EditorModule editorModule = new EditorModule();
            editorModule.SaveContent(form["contenttext"].ToString());
            editorModule.Dispose();
            return RedirectToAction("AboutUs");
        }

        //比賽訊息
        [HttpGet]
        public ActionResult NewsList(int? page, string qry, string sort, string disHome, string disable, string pDate)
        {
            NewsListViewModel model = new NewsListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Disable = disable ?? string.Empty;
            model.Filter.DisplayForHomePage = disHome ?? string.Empty;
            model.Filter.PublishDate = pDate ?? string.Empty;

            using (var module = new NewsModule())
            {
                model.Result = module.DoGetList(model.Filter);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult NewsAdd()
        {
            NewsDetailsDataModel defaultModel = new NewsDetailsDataModel();
            defaultModel.Data.PUB_DT_STR = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.Data.DISABLE = false;
            defaultModel.Data.SQ = 1;
            defaultModel.Data.HOME_PAGE_DISPLAY = true;
            return View(defaultModel);
        }

        public ActionResult NewsEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("NewsList");

            NewsDetailsDataModel model = new NewsDetailsDataModel();
            using (var module = new NewsModule())
            {
                model = module.DoGetDetailsByID((int)ID);
            }

            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult NewsSave(FormCollection form)
        {
            int? ID = Convert.ToInt32(form["pageId"]);
            int identityId = 0;
            using (var module = new NewsModule())
            {
                identityId = module.DoSaveData(form, ID);
            }
            return RedirectToAction("NewsEdit", new { ID = identityId });
        }

        [HttpPost]
        public JsonResult NewsDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                using (var module = new NewsModule())
                {
                    module.DoDeleteByID((int)ID);
                }
                messages = "刪除成功";
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        // 比賽成績公告
        [HttpGet]
        public ActionResult ResultList(int? page, string qry, string sort, string disHome, string disable, string pDate)
        {
            ResultListViewModel model = new ResultListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Disable = disable ?? string.Empty;
            model.Filter.DisplayForHomePage = disHome ?? string.Empty;
            model.Filter.PublishDate = pDate ?? string.Empty;

            using (var module = new ResultModule())
            {
                model.Result = module.DoGetList(model.Filter);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult ResultAdd()
        {
            ResultDetailsDataModel defaultModel = new ResultDetailsDataModel();
            defaultModel.Data.PUB_DT_STR = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.Data.DISABLE = false;
            defaultModel.Data.SQ = 1;
            return View(defaultModel);
        }

        public ActionResult ResultEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("ResultList");

            ResultDetailsDataModel model = new ResultDetailsDataModel();
            using (var module = new ResultModule())
            {
                model = module.DoGetDetailsByID((int)ID);
            }

            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ResultSave(FormCollection form)
        {
            int? ID = Convert.ToInt32(form["pageId"]);
            int identityId = 0;
            using (var module = new ResultModule())
            {
                identityId = module.DoSaveData(form, ID);
            }
            return RedirectToAction("ResultEdit", new { ID = identityId });
        }

        [HttpPost]
        public JsonResult ResultDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                using (var module = new ResultModule())
                {
                    module.DoDeleteByID((int)ID);
                }
                messages = "刪除成功";
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        // 活動管理
        public ActionResult ActivityList(int? page, string qry, string sort, string disable, string pDate)
        {
            ActivityListViewModel model = new ActivityListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry == null ? string.Empty : qry.Trim();
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Disable = disable ?? string.Empty;
            model.Filter.PublishDate = pDate;

            using (ActivityModule module = new ActivityModule())
            {
                model.Result = module.DoGetList(model.Filter);
            }

            return View(model);
        }

        public ActionResult ActivityAdd()
        {
            ActivityDetailsModel defaultModel = new ActivityDetailsModel();
            defaultModel.Disable = false;
            defaultModel.PublishDateStr = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.Sort = 1;
            return View(defaultModel);
        }

        public ActionResult ActivityEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("ActivityList");
            ActivityDetailsModel model = new ActivityDetailsModel();
            using (ActivityModule module = new ActivityModule())
            {
                model = module.DoGetDetailsByID((int)ID);
            }
            FileModule fileModule = new FileModule();
            model.Files = fileModule.GetFiles((int)model.ID, "Activity", "F");
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ActivitySave(ActivityDataModel model)
        {
            int id = 0;
            using (ActivityModule module = new ActivityModule())
            {
                id = module.DoSaveData(model);
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("ActivityEdit", "_SysAdm", new { ID = id });
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult ActivityDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                using (ActivityModule module = new ActivityModule())
                {
                    module.DoDeleteByID((int)ID);
                }
                messages = "刪除成功";
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        // 報名維護
        public ActionResult ApplyList(int? page, string sort)
        {
            page = page ?? 1;
            ApplyMaintainListViewModel model = new ApplyMaintainListViewModel();
            model.Filter.CurrentPage = (int)page;
            model.Filter.SortColumn = sort ?? string.Empty;
            using (ApplyMaintainModule module = new ApplyMaintainModule())
            {
                model.Result = module.DoGetList(model.Filter);
            }

            return View(model);
        }

        public ActionResult ApplyEdit(int? ID, int? page, int? groupId, string sort, string qry)
        {
            page = page ?? 1;
            if (!ID.HasValue)
                return RedirectToAction("ApplyList");
            ApplyDetailsDataModel model = new ApplyDetailsDataModel();
            model.ActivityID = (int)ID;
            model.ListData.Filter.CurrentPage = (int)page;
            model.ListData.Filter.GroupID = groupId;
            model.ListData.Filter.SortColumn = sort ?? string.Empty;
            model.ListData.Filter.QueryString = qry == null ? string.Empty : qry.Trim();

            using (ApplyMaintainModule module = new ApplyMaintainModule())
            {
                model = module.GetApplyDetails(model);
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult ApplyDelete(int? actID, int? applyID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                using (ApplyMaintainModule module = new ApplyMaintainModule())
                {
                    module.DoDeleteByID((int)actID, (int)applyID);
                }
                messages = "刪除成功";
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        [HttpGet]
        public ActionResult ApplyModal(int actID, int applyID)
        {
            ApplyModalDataModel model = new ApplyModalDataModel();
            using (var applyModule = new ApplyFrontModule())
            {
                using (ApplyMaintainModule module = new ApplyMaintainModule())
                {
                    model = module.GetApplyData((int)actID, (int)applyID);
                    if (model == null)
                        TempData["ErrorMsg"] = "查無該活動賽事";
                    else
                        TempData["ErrorMsg"] = "";
                }
            }
            return PartialView("_ApplyModalPartial", model);
        }

        [HttpPost]
        public ActionResult ApplyModal(ApplyModalDataModel model)
        {
            using (ApplyMaintainModule module = new ApplyMaintainModule())
            {
                var details = module.SaveApply(model);
                if (details == null)
                    TempData["ErrorMsg"] = "查無該活動賽事";
            }
            return RedirectToAction("ApplyEdit", new { ID = model.ActivityID });
        }

        //修改密碼
        [HttpGet]
        public ActionResult ChangePW()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePW(FormCollection form)
        {
            SignInModule signInModule = new SignInModule();
            try
            {
                signInModule.ChangePassword(form);
                ViewBag.Message = "success";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            finally
            {
                signInModule.Dispose();
            }
            return View();
        }

        #endregion 籃委會
    }
}