using OutWeb.ActionFilter;
using OutWeb.Authorize;
using OutWeb.Models.Manage.ActivityModels;
using OutWeb.Models.Manage.BannerModels;
using OutWeb.Models.Manage.DownloadModels;
using OutWeb.Models.Manage.ManageNewsModels;
using OutWeb.Models.Manage.ResultModels;
using OutWeb.Modules.Manage;
using System;
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

        //#region 能源 線上課程

        //[HttpGet]
        //public ActionResult CourseList(int? page, string qry, string sort, string fSt, string hSt, string pDate, string lang)
        //{
        //    Language language = PublicMethodRepository.CurrentLanguageEnum;
        //    CourseListViewModel model = new CourseListViewModel();
        //    model.Filter.CurrentPage = page ?? 1;
        //    model.Filter.QueryString = qry ?? string.Empty;
        //    model.Filter.SortColumn = sort ?? string.Empty;
        //    model.Filter.DisplayForFrontEnd = fSt ?? string.Empty;
        //    model.Filter.DisplayForHomePage = hSt ?? string.Empty;
        //    model.Filter.PublishDate = pDate;

        //    using (var module = ListFactoryService.Create(Enums.ListMethodType.COURSE))
        //    {
        //        model.Result = (module.DoGetList(model.Filter, language) as CourseListResultModel);
        //    }
        //    return View(model);
        //}

        //[HttpGet]
        //public ActionResult CourseAdd()
        //{
        //    //產品分類下拉選單
        //    //SelectList typeList = typeModule.CreateGeneralTypeDropList(null, false);
        //    //ViewBag.TypeList = typeList;
        //    CourseDetailsDataModel defaultModel = new CourseDetailsDataModel();
        //    defaultModel.Data.發稿日期 = DateTime.UtcNow.AddHours(8);
        //    defaultModel.Data.發稿人 = "系統管理員";
        //    defaultModel.Data.顯示狀態 = true;
        //    defaultModel.Data.首頁顯示 = true;
        //    return View(defaultModel);
        //}

        //[ValidateInput(false)]
        //[HttpPost]
        //public ActionResult CourseAdd(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        //{
        //    string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
        //    Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
        //    int identityId = 0;
        //    using (var module = ListFactoryService.Create(Enums.ListMethodType.COURSE))
        //    {
        //        identityId = module.DoSaveData(form, language, null, images, files);
        //    }
        //    var redirectUrl = new UrlHelper(Request.RequestContext).Action("CourseEdit", "_SysAdm", new { ID = identityId });
        //    return Json(new { Url = redirectUrl });
        //}

        //[HttpGet]
        //public ActionResult CourseEdit(int? ID)
        //{
        //    if (!ID.HasValue)
        //        return RedirectToAction("CourseList");
        //    CourseDetailsDataModel model;
        //    using (var module = ListFactoryService.Create(Enums.ListMethodType.COURSE))
        //    {
        //        model = (module.DoGetDetailsByID((int)ID) as CourseDetailsDataModel);
        //    }
        //    if (model == null)
        //        return RedirectToAction("Login", "SignIn");
        //    //取圖檔
        //    ImgModule imgModule = new ImgModule();
        //    model.ImagesData = imgModule.GetImages((int)model.Data.主索引, "Course", "M");
        //    //取檔案
        //    FileModule fileModule = new FileModule();
        //    model.FilesData = fileModule.GetFiles((int)model.Data.主索引, "Course", "M");
        //    imgModule.Dispose();
        //    fileModule.Dispose();
        //    return View(model);
        //}

        //[ValidateInput(false)]
        //[HttpPost]
        //public ActionResult CourseEdit(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        //{
        //    string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
        //    Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
        //    int? ID = Convert.ToInt32(form["courseID"]);
        //    int identityId = 0;
        //    using (var module = ListFactoryService.Create(Enums.ListMethodType.COURSE))
        //    {
        //        identityId = module.DoSaveData(form, language, ID, images, files);
        //    }
        //    var redirectUrl = new UrlHelper(Request.RequestContext).Action("CourseEdit", "_SysAdm", new { ID = identityId });
        //    return Json(new { Url = redirectUrl });
        //}

        //[HttpPost]
        //public JsonResult CourseDelete(int? ID)
        //{
        //    bool success = true;
        //    string messages = string.Empty;
        //    try
        //    {
        //        ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.COURSE);
        //        module.DoDeleteByID((int)ID);
        //        module.Dispose();
        //        messages = "刪除成功";
        //    }
        //    catch (Exception ex)
        //    {
        //        success = false;
        //        messages = ex.Message;
        //    }
        //    var resultJson = Json(new { success = success, messages = messages });
        //    return resultJson;
        //}

        //#endregion 能源 線上課程

        //#region 能源 研討會

        //[HttpGet]
        //public ActionResult TrainList(int? page, string qry, string sort, string fSt, string hSt, string date, string lang)
        //{
        //    Language language = PublicMethodRepository.CurrentLanguageEnum;
        //    TrainListViewModel model = new TrainListViewModel();
        //    model.Filter.CurrentPage = page ?? 1;
        //    model.Filter.QueryString = qry ?? string.Empty;
        //    model.Filter.SortColumn = sort ?? string.Empty;
        //    model.Filter.Status = fSt ?? string.Empty;
        //    model.Filter.HomeDisplay = hSt ?? string.Empty;
        //    model.Filter.PublishDate = date;
        //    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
        //    model.Result = (module.DoGetList(model.Filter, language) as TrainListResultModel);
        //    module.Dispose();
        //    return View(model);
        //}

        //[HttpGet]
        //public ActionResult TrainAdd()
        //{
        //    TrainDetailsDataModel defaultModel = new TrainDetailsDataModel();
        //    defaultModel.Data.ActivityDateBegin = DateTime.UtcNow.AddHours(8);
        //    defaultModel.Data.DeadlineBegin = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
        //    defaultModel.Data.DeadlineEnd = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
        //    defaultModel.Data.ActivityTimeRange = string.Format("{0}~{1}", DateTime.UtcNow.AddHours(8).ToString("HH:mm"), DateTime.UtcNow.AddHours(8).ToString("HH:mm"));

        //    defaultModel.Data.EnrollmentRestrictions = 10;
        //    return View(defaultModel);
        //}

        //[ValidateInput(false)]
        //[HttpPost]
        //public ActionResult TrainAdd(FormCollection form, List<HttpPostedFileBase> files)
        //{
        //    string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
        //    Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
        //    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
        //    int identityId = module.DoSaveData(form, language, null, null, files);
        //    var redirectUrl = new UrlHelper(Request.RequestContext).Action("TrainEdit", "_SysAdm", new { ID = identityId });
        //    module.Dispose();
        //    return Json(new { Url = redirectUrl });
        //}

        //[HttpGet]
        //public ActionResult TrainEdit(int? ID)
        //{
        //    if (!ID.HasValue)
        //        return RedirectToAction("TrainList");
        //    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
        //    TrainDetailsDataModel model = (module.DoGetDetailsByID((int)ID) as TrainDetailsDataModel);
        //    if (model == null)
        //        return RedirectToAction("TrainList");
        //    //取檔案
        //    FileModule fileModule = new FileModule();
        //    model.FilesData = fileModule.GetFiles((int)model.Data.ID, "Train", "M");
        //    module.Dispose();

        //    return View(model);
        //}

        //[ValidateInput(false)]
        //[HttpPost]
        //public ActionResult TrainEdit(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        //{
        //    string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
        //    Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
        //    int? ID = Convert.ToInt32(form["ID"]);
        //    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
        //    int identityId = module.DoSaveData(form, language, ID, null, files);
        //    var redirectUrl = new UrlHelper(Request.RequestContext).Action("TrainEdit", "_SysAdm", new { ID = identityId });
        //    module.Dispose();
        //    return Json(new { Url = redirectUrl });
        //}

        //[HttpPost]
        //public JsonResult TrainDelete(int? ID)
        //{
        //    bool success = true;
        //    string messages = string.Empty;
        //    try
        //    {
        //        ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
        //        module.DoDeleteByID((int)ID);
        //        messages = "刪除成功";
        //        module.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        success = false;
        //        messages = ex.Message;
        //    }
        //    var resultJson = Json(new { success = success, messages = messages });
        //    return resultJson;
        //}

        //[HttpGet]
        //public ActionResult TrainApplyList(int? page, string qry, string sort, string bDate, string eDate, string lang)
        //{
        //    Language language = PublicMethodRepository.CurrentLanguageEnum;
        //    TrainApplyListViewModel model = new TrainApplyListViewModel();
        //    model.Filter.CurrentPage = page ?? 1;
        //    model.Filter.QueryString = qry ?? string.Empty;
        //    model.Filter.SortColumn = sort ?? string.Empty;
        //    model.Filter.ActivityBeginDate = bDate ?? string.Empty;
        //    model.Filter.ActivityEndDate = eDate ?? string.Empty;

        //    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAINAPPLY);
        //    model.Result = (module.DoGetList(model.Filter, language) as TrainApplyListResultModel);
        //    module.Dispose();
        //    return View(model);
        //}

        //public ActionResult TrainApplyEdit(int? trainID)
        //{
        //    if (trainID == null)
        //        throw new Exception("查無此研討會相關資料");
        //    TrainApplyDetailsModel model = new TrainApplyDetailsModel();
        //    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAINAPPLY);
        //    model = (module.DoGetDetailsByID((int)trainID) as TrainApplyDetailsModel);
        //    module.Dispose();
        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult DeleteApply(int trainID, int applyID)
        //{
        //    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAINAPPLY);
        //    bool isSuccess = (module as TrainApplyModule).DeleteApply(trainID, applyID);
        //    var redirectUrl = new UrlHelper(Request.RequestContext).Action("TrainApplyEdit", "_SysAdm", new { trainID = trainID });
        //    module.Dispose();
        //    return Json(new { success = isSuccess, url = redirectUrl });
        //}

        //[HttpGet]
        //public ActionResult TrainApplyViewData(int trainID, int applyID)
        //{
        //    TrainApplyDataModel model = new TrainApplyDataModel();
        //    TrainApplyModule module = new TrainApplyModule();
        //    model = module.GetTrainApplyParticipantsDataByApplyID(trainID, applyID);
        //    module.Dispose();
        //    return PartialView("_TrainApplyViewPartial", model);
        //}

        //[HttpPost]
        //public ActionResult TrainApplyViewData(TrainApplyViewModel model)
        //{
        //    bool isSuccess = true;
        //    string msg = string.Empty;
        //    TrainApplyModule module = new TrainApplyModule();
        //    try
        //    {
        //        module.SaveApplyParticipantsData(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        isSuccess = false;
        //        msg = ex.Message;
        //    }
        //    finally
        //    {
        //        module.Dispose();
        //    }
        //    if (isSuccess)
        //        msg = "設定完成";
        //    return Json(new { success = isSuccess, msg = msg });
        //}

        //#endregion 能源 研討會

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
            model.Filter.QueryString = qry ?? string.Empty;
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
                if (model.OldFilesId.Count == 0 && model.Files.Count == 0)
                {
                    TempData["UndefinedFile"] = "請上傳檔案";
                    return RedirectToAction("ActivityEdit", new { ID = (int?)null });
                }
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