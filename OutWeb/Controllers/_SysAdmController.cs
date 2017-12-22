using OutWeb.ActionFilter;
using OutWeb.Authorize;
using OutWeb.Entities;
using OutWeb.Enums;
using OutWeb.Exceptions;
using OutWeb.Models.Manage.CasesModels;
using OutWeb.Models.Manage.EditorModels;
using OutWeb.Models.Manage.IPModels;
using OutWeb.Models.Manage.ManageBookModels;
using OutWeb.Models.Manage.ManageCourseModels;
using OutWeb.Models.Manage.ManageLinkModels;
using OutWeb.Models.Manage.ManageNewsModels;
using OutWeb.Models.Manage.ManageNotificationModels;
using OutWeb.Models.Manage.ManageTrainApplyModels;
using OutWeb.Models.Manage.ManageTrainApplyModels.TrainApplyDetailsModels;
using OutWeb.Models.Manage.ManageTrainModels;
using OutWeb.Models.Manage.QuestionnairesModels;
using OutWeb.Models.Manage.QuestionnaireStatisticsModels;
using OutWeb.Models.Manage.TypeManageModels;
using OutWeb.Modules.Manage;
using OutWeb.Provider;
using OutWeb.Repositories;
using OutWeb.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
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

        #region 能源 分類管理

        /// <summary>
        /// 項目檔對應設定
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TypeManageSetting()
        {
            if (UserProvider.Instance.User.Role != UserRoleEnum.SUPERADMIN)
                throw new Exception("您尚未持有權限進入該頁面。");
            //產品分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            SelectList itemList = typeModule.GetItemDropDownList(true);
            SelectList typeList = typeModule.GetItemDropDownList(false);
            ViewBag.ItemList = itemList;
            ViewBag.TypeList = typeList;
            return View();
        }

        [HttpPost]
        public ActionResult TypeManageSetting(FormCollection form)
        {
            Exception error = null;
            DBEnergy db = new DBEnergy();
            int itemID = Convert.ToInt32(form["item"]);
            int typeID = Convert.ToInt32(form["type"]);
            分類對應檔 map = null;
            分類對應檔 chk;
            int index = 1;
            chk = db.分類對應檔.Where(o => o.對應項目索引 == itemID && o.對應分類類別索引 == typeID).FirstOrDefault();
            if (chk == null)
            {
                map = new 分類對應檔();
                var getLastIndentity = db.分類對應檔.Where(o => o.對應項目索引 == itemID).ToList();
                if (getLastIndentity.Count > 0)
                    index = getLastIndentity.Max(m => m.項次) + 1;
                map.項次 = index;
            }
            else
                map = chk;
            map.對應項目索引 = itemID;
            map.對應分類類別索引 = typeID;

            if (chk != null)
                db.Entry(map).State = EntityState.Modified;
            else
                db.分類對應檔.Add(map);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                error = ex;
            }
            //產品分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            SelectList itemList = typeModule.GetItemDropDownList(true, itemID);
            SelectList typeList = typeModule.GetItemDropDownList(false, typeID);
            ViewBag.ItemList = itemList;
            ViewBag.TypeList = typeList;
            if (error != null)
                TempData["Error"] = error.Message;
            else
                TempData["Success"] = "設定完成";

            return View();
        }

        /// <summary>
        /// 判斷分類是否已被引用 被引用不得刪除 目前只做出版品
        /// </summary>
        /// <param name="ID">分類代碼</param>
        /// <param name="filterTypeID">主分類代碼</param>
        /// <param name="actionName">功能項目名稱]</param>
        private bool CheckTypeHasUsing(int? ID, string actionName)
        {
            DBEnergy db = new DBEnergy();
            dynamic chk = null;
            string switchMode = string.Empty;
            if (actionName.StartsWith("Book"))
                switchMode = "B";
            switch (switchMode)
            {
                case ("B"):
                    chk = db.出版品主檔.Where(o => o.對應分類索引 == ID).ToList();
                    break;

                default:
                    break;
            };
            return chk.Count > 0;
        }



        public ActionResult TypeManageList(int? page, string qry, string sort, string status, int? type)
        {
            TypeManageModule typeModule = new TypeManageModule();
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            TypeManageListViewModel model = new TypeManageListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Status = status ?? string.Empty;
            model.Filter.TypeID = type;
            model.Filter.TypeName = model.Filter.TypeID.HasValue ? typeModule.GetTypeNameByTypeID((int)type) : "";

            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
            model.Result = (module.DoGetList(model.Filter, language) as TypeManageListResultModel);
            return View(model);
        }

        [HttpGet]
        public ActionResult TypeManageAdd(int? filterTypeID)
        {
            TypeManageDetailsDataModel defaultModel = new TypeManageDetailsDataModel();
            if (filterTypeID.HasValue)
            {
                DBEnergy db = new DBEnergy();
                var typeName = db.分類主檔.Where(o => o.主索引 == filterTypeID).First().分類名稱;
                defaultModel = new TypeManageDetailsDataModel()
                {
                    FilterTypeName = typeName,
                    FilterTypeID = filterTypeID
                };
            }
            return View(defaultModel);
        }

        [HttpPost]
        public ActionResult TypeManageAdd(FormCollection form)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int identityId = 0;
            if (!string.IsNullOrEmpty(form["FilterTypeID"]))
            {
                TypeManageModule mdu = new TypeManageModule();
                identityId = mdu.DoSaveSubTypeData(form, language);
            }
            else
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
                identityId = module.DoSaveData(form, language);
            }
            return RedirectToAction("TypeManageEdit", "_SysAdm", new { ID = identityId, filterTypeID = form["FilterTypeID"] });
        }

        [HttpGet]
        public ActionResult TypeManageEdit(int? ID, int? filterTypeID)
        {
            if (!ID.HasValue)
                return RedirectToAction("TypeManageList");
            TypeManageDetailsDataModel model = null;
            if (!filterTypeID.HasValue)
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
                model = (module.DoGetDetailsByID((int)ID) as TypeManageDetailsDataModel);
            }
            else
            {
                TypeManageModule mdu = new TypeManageModule();
                model = mdu.DoGetSubDetailsByID((int)ID, (int)filterTypeID);
                DBEnergy db = new DBEnergy();
                var typeName = db.分類主檔.Where(o => o.主索引 == filterTypeID).First().分類名稱;
                model.FilterTypeID = filterTypeID;
                model.FilterTypeName = typeName;
            }

            if (model == null)
                return RedirectToAction("Login", "SignIn");
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult TypeManageEdit(FormCollection form)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int identityId = 0;
            if (!string.IsNullOrEmpty(form["FilterTypeID"]))
            {
                TypeManageModule mdu = new TypeManageModule();
                identityId = mdu.DoSaveSubTypeData(form, language, Convert.ToInt32(form["TypeID"]));
            }
            else
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
                identityId = module.DoSaveData(form, language, Convert.ToInt32(form["TypeID"]));
            }
            return RedirectToAction("TypeManageEdit", "_SysAdm", new { ID = identityId, filterTypeID = form["FilterTypeID"] });
        }

        [HttpPost]
        public JsonResult TypeManageDelete(int? ID, int? filterTypeID)
        {
            bool success = true;
            JsonResult resultJson = new JsonResult();
            string messages = string.Empty;
            try
            {
                if (!filterTypeID.HasValue)
                {
                    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
                    module.DoDeleteByID((int)ID);
                }
                else
                {
                    TypeManageModule mdu = new TypeManageModule();
                    mdu.DoDeleteSubDataByID((int)ID, (int)filterTypeID);
                }

                messages = "刪除成功";
                resultJson = Json(new { success = success, messages = messages });
            }
            catch (GeneralTypeRelationExcption exRe)
            {
                success = false;
                resultJson = Json(new { success = success, messages = exRe.Message });
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            return resultJson;
        }

        #endregion 能源 分類管理

        #region 能源 國內外新聞

        [HttpGet]
        public ActionResult NewsList(int? page, string qry, string sort, string fSt, string hSt, string pDate, string lang, int? type)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            NewsListViewModel model = new NewsListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.DisplayForFrontEnd = fSt ?? string.Empty;
            model.Filter.DisplayForHomePage = hSt ?? string.Empty;
            model.Filter.PublishDate = pDate;
            model.Filter.Type = type == null ? null : type.ToString();

            using (var module = ListFactoryService.Create(Enums.ListMethodType.NEWS))
            {
                model.Result = (module.DoGetList(model.Filter, language) as NewsListResultModel);
            }

            //分類下拉選單
            try
            {
                TypeManageModule typeModule = new TypeManageModule();
                SelectList typeList = typeModule.CreateTypeManageDropList(type, true, "News");
                ViewBag.TypeList = typeList;
                typeModule.Dispose();
            }
            catch (TypeIsNotCreateException typeEx)
            {
                TempData["TypeError"] = typeEx.Message;
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult NewsAdd()
        {
            //產品分類下拉選單
            //SelectList typeList = typeModule.CreateGeneralTypeDropList(null, false);
            //ViewBag.TypeList = typeList;
            NewsDetailsDataModel defaultModel = new NewsDetailsDataModel();
            defaultModel.Data.發稿時間 = DateTime.UtcNow.AddHours(8);
            defaultModel.Data.發稿人 = "系統管理員";
            defaultModel.Data.顯示狀態 = true;
            //產品分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            SelectList typeList = typeModule.CreateTypeManageDropList(0, false, "News");
            ViewBag.TypeList = typeList;
            typeModule.Dispose();
            return View(defaultModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult NewsAdd(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int identityId = 0;
            using (var module = ListFactoryService.Create(Enums.ListMethodType.NEWS))
            {
                identityId = module.DoSaveData(form, language, null, images, files);
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("NewsEdit", "_SysAdm", new { ID = identityId });
            return Json(new { Url = redirectUrl });
        }

        [HttpGet]
        public ActionResult NewsEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("NewsList");
            NewsDetailsDataModel model;
            using (var module = ListFactoryService.Create(Enums.ListMethodType.NEWS))
            {
                model = (module.DoGetDetailsByID((int)ID) as NewsDetailsDataModel);
            }
            if (model == null)
                return RedirectToAction("Login", "SignIn");
            //取圖檔
            ImgModule imgModule = new ImgModule();
            model.ImagesData = imgModule.GetImages((int)model.Data.主索引, "News", "M");
            //取檔案
            FileModule fileModule = new FileModule();
            model.FilesData = fileModule.GetFiles((int)model.Data.主索引, "News", "M");
            //產品分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            SelectList typeList = typeModule.CreateTypeManageDropList(model.Data.分類代碼, false, "News");
            ViewBag.TypeList = typeList;

            imgModule.Dispose();
            fileModule.Dispose();
            typeModule.Dispose();
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult NewsEdit(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int? ID = Convert.ToInt32(form["newsID"]);
            int identityId = 0;
            using (var module = ListFactoryService.Create(Enums.ListMethodType.NEWS))
            {
                identityId = module.DoSaveData(form, language, ID, images, files);
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("NewsEdit", "_SysAdm", new { ID = identityId });
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult NewsDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                using (var module = ListFactoryService.Create(Enums.ListMethodType.NEWS))
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

        #endregion 能源 國內外新聞

        #region 能源 能源查核申報系統

        [HttpGet]
        public ActionResult EnergyAudit()
        {
            EnergyAuditModel model = new EnergyAuditModel() { ID = 1, MappingActionName = "EnergyAudit" };
            EditorModule editorModule = new EditorModule();
            model = editorModule.DoGetDetails(ref model);
            //取圖檔
            ImgModule imgModule = new ImgModule();
            model.ImagesData = imgModule.GetImages((int)model.ID, "EnergyAudit", "M");
            //取檔案
            FileModule fileModule = new FileModule();
            model.FilesData = fileModule.GetFiles((int)model.ID, "EnergyAudit", "M");
            editorModule.Dispose();
            imgModule.Dispose();
            fileModule.Dispose();
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EnergyAudit(FormCollection form, List<HttpPostedFileBase> files)
        {
            EnergyAuditModel model = new EnergyAuditModel();
            EditorModule editorModule = new EditorModule();
            editorModule.DoSaveData(form, files);
            editorModule.Dispose();
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("EnergyAudit", "_SysAdm");
            return Json(new { Url = redirectUrl });
        }

        #endregion 能源 能源查核申報系統

        #region 能源 能校指標申報系統

        [HttpGet]
        public ActionResult EnergyIndex()
        {
            EnergyIndexModel model = new EnergyIndexModel() { ID = 1, MappingActionName = "EnergyIndex" };
            EditorModule editorModule = new EditorModule();
            model = editorModule.DoGetDetails(ref model);
            //取圖檔
            ImgModule imgModule = new ImgModule();
            model.ImagesData = imgModule.GetImages((int)model.ID, "EnergyIndex", "M");
            //取檔案
            FileModule fileModule = new FileModule();
            model.FilesData = fileModule.GetFiles((int)model.ID, "EnergyIndex", "M");
            editorModule.Dispose();
            imgModule.Dispose();
            fileModule.Dispose();
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EnergyIndex(FormCollection form, List<HttpPostedFileBase> files)
        {
            EnergyAuditModel model = new EnergyAuditModel();
            EditorModule editorModule = new EditorModule();
            editorModule.DoSaveData(form, files);
            editorModule.Dispose();
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("EnergyIndex", "_SysAdm");
            return Json(new { Url = redirectUrl });
        }

        #endregion 能源 能校指標申報系統

        #region 能源 線上課程

        [HttpGet]
        public ActionResult CourseList(int? page, string qry, string sort, string fSt, string hSt, string pDate, string lang)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            CourseListViewModel model = new CourseListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.DisplayForFrontEnd = fSt ?? string.Empty;
            model.Filter.DisplayForHomePage = hSt ?? string.Empty;
            model.Filter.PublishDate = pDate;

            using (var module = ListFactoryService.Create(Enums.ListMethodType.COURSE))
            {
                model.Result = (module.DoGetList(model.Filter, language) as CourseListResultModel);
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult CourseAdd()
        {
            //產品分類下拉選單
            //SelectList typeList = typeModule.CreateGeneralTypeDropList(null, false);
            //ViewBag.TypeList = typeList;
            CourseDetailsDataModel defaultModel = new CourseDetailsDataModel();
            defaultModel.Data.發稿日期 = DateTime.UtcNow.AddHours(8);
            defaultModel.Data.發稿人 = "系統管理員";
            defaultModel.Data.顯示狀態 = true;
            defaultModel.Data.首頁顯示 = true;
            return View(defaultModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult CourseAdd(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int identityId = 0;
            using (var module = ListFactoryService.Create(Enums.ListMethodType.COURSE))
            {
                identityId = module.DoSaveData(form, language, null, images, files);
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("CourseEdit", "_SysAdm", new { ID = identityId });
            return Json(new { Url = redirectUrl });
        }

        [HttpGet]
        public ActionResult CourseEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("CourseList");
            CourseDetailsDataModel model;
            using (var module = ListFactoryService.Create(Enums.ListMethodType.COURSE))
            {
                model = (module.DoGetDetailsByID((int)ID) as CourseDetailsDataModel);
            }
            if (model == null)
                return RedirectToAction("Login", "SignIn");
            //取圖檔
            ImgModule imgModule = new ImgModule();
            model.ImagesData = imgModule.GetImages((int)model.Data.主索引, "Course", "M");
            //取檔案
            FileModule fileModule = new FileModule();
            model.FilesData = fileModule.GetFiles((int)model.Data.主索引, "Course", "M");
            imgModule.Dispose();
            fileModule.Dispose();
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult CourseEdit(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int? ID = Convert.ToInt32(form["courseID"]);
            int identityId = 0;
            using (var module = ListFactoryService.Create(Enums.ListMethodType.COURSE))
            {
                identityId = module.DoSaveData(form, language, ID, images, files);
            }
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("CourseEdit", "_SysAdm", new { ID = identityId });
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult CourseDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.COURSE);
                module.DoDeleteByID((int)ID);
                module.Dispose();
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

        #endregion 能源 線上課程

        #region 能源 節能案例

        public ActionResult CasesList(int? page, string qry, string sort, string pDate, string fSt, string lang, int? type1, int? type2)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            CasesListViewModel model = new CasesListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.DisplayForFrontEnd = fSt ?? string.Empty;
            model.Filter.PublishDate = pDate;
            model.Filter.TypeID1 = type1 == null ? null : type1;
            model.Filter.TypeID2 = type2 == null ? null : type2;
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.WORKS);
            model.Result = (module.DoGetList(model.Filter, language) as CasesListResultModel);
            //分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            try
            {
                SelectList type1List = typeModule.CreateTypeManageDropList(model.Filter.TypeID1, true, "Cases", 1);
                SelectList type2List = typeModule.CreateTypeManageDropList(model.Filter.TypeID2, true, "Cases", 2);
                ViewBag.Type1List = type1List;
                ViewBag.Type2List = type2List;
            }
            catch (TypeIsNotCreateException typeEx)
            {
                TempData["TypeError"] = typeEx.Message;
            }
            finally
            {
                module.Dispose();
                typeModule.Dispose();
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult CasesDataAdd()
        {
            TypeManageModule typeModule = new TypeManageModule();
            SelectList type1List = typeModule.CreateTypeManageDropList(0, false, "Cases", 1);
            SelectList type2List = typeModule.CreateTypeManageDropList(0, false, "Cases", 2);
            ViewBag.Type1List = type1List;
            ViewBag.Type2List = type2List;

            CasesDetailsDataModel defaultModel = new CasesDetailsDataModel();
            defaultModel.Data.發佈日期 = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.Data.顯示狀態 = true;
            defaultModel.Data.首頁顯示 = true;
            typeModule.Dispose();
            return View(defaultModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult CasesDataAdd(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            ListModuleService module = ListFactoryService.Create(ListMethodType.WORKS);
            int identityId = module.DoSaveData(form, language, null, images, files);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("CasesDataEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl });
        }

        [HttpGet]
        public ActionResult CasesDataEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("CasesList");
            ListModuleService module = ListFactoryService.Create(ListMethodType.WORKS);
            CasesDetailsDataModel model = (module.DoGetDetailsByID((int)ID) as CasesDetailsDataModel);
            if (model == null)
                return RedirectToAction("Login", "SignIn");
            //取圖檔
            ImgModule imgModule = new ImgModule();
            model.ImagesData = imgModule.GetImages((int)model.Data.主索引, "Cases", "M");
            //取檔案
            FileModule fileModule = new FileModule();
            model.FilesData = fileModule.GetFiles((int)model.Data.主索引, "Cases", "M");
            //分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            SelectList type1List = typeModule.CreateTypeManageDropList(model.Data.設備別, false, "Cases", 1);
            SelectList type2List = typeModule.CreateTypeManageDropList(model.Data.行業別, false, "Cases", 2);
            ViewBag.Type1List = type1List;
            ViewBag.Type2List = type2List;
            module.Dispose();
            imgModule.Dispose();
            fileModule.Dispose();
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult CasesDataEdit(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int? ID = Convert.ToInt32(form["casesID"]);
            ListModuleService module = ListFactoryService.Create(ListMethodType.WORKS);
            int identityId = module.DoSaveData(form, language, ID, images, files);
            CasesDetailsDataModel model = (module.DoGetDetailsByID((int)identityId) as CasesDetailsDataModel);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("CasesDataEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult CasesDataDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                ListModuleService module = ListFactoryService.Create(ListMethodType.WORKS);
                module.DoDeleteByID((int)ID);
                messages = "刪除成功";
                module.Dispose();
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }

            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        #endregion 能源 節能案例

        #region 能源 問卷調查

        [HttpGet]
        public ActionResult QuestionnairesList(int? page, string qry, string sort)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            QuestionListViewModel model = new QuestionListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            QuestionnairesModule module = new QuestionnairesModule();
            model.Result = module.DoGetList(model.Filter);
            module.Dispose();
            return View(model);
        }

        [HttpGet]
        public ActionResult QuestionnairesAdd()
        {
            QuestionDetailsDataModel defaultModel = new QuestionDetailsDataModel();
            defaultModel.OpeningTime = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.EndTime = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.Url = "請先存檔";
            defaultModel.SendCount = 3000;
            return View(defaultModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult QuestionnairesAdd(QuestionDetailsDataModel model, string[] topicID, string[] optionID)
        {
            var dateCompare = new TimeSpan(
                Convert.ToDateTime(model.OpeningTime).Ticks - Convert.ToDateTime(model.EndTime).Ticks).TotalDays;
            if (dateCompare >= 1)
            {
                ViewBag.Error = "結束日不得小於開始日";
            }
            QuestionnairesModule module = new QuestionnairesModule();
            int identityId = module.DoSaveData(model);
            module.Dispose();
            return RedirectToAction("QuestionnairesEdit", new { ID = identityId });
        }

        [HttpGet]
        public ActionResult QuestionnairesEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("QuestionnairesList");
            QuestionnairesModule module = new QuestionnairesModule();
            QuestionDetailsDataModel result = module.DoGetDetailsByID((int)ID);
            return View(result);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult QuestionnairesEdit(QuestionDetailsDataModel model)
        {
            var dateCompare = new TimeSpan(
            Convert.ToDateTime(model.OpeningTime).Ticks - Convert.ToDateTime(model.EndTime).Ticks).TotalDays;
            if (dateCompare >= 1)
            {
                ViewBag.Error = "結束日不得小於開始日";
            }
            QuestionnairesModule module = new QuestionnairesModule();
            int identityId = module.DoSaveData(model);
            module.Dispose();
            return RedirectToAction("QuestionnairesEdit", new { ID = identityId });
        }

        [HttpPost]
        public JsonResult QuestionnairesDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                QuestionnairesModule module = new QuestionnairesModule();
                module.DoDeleteByID((int)ID);
                messages = "刪除成功";
                module.Dispose();
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        #endregion 能源 問卷調查

        #region 能源 問卷統計

        [HttpGet]
        public ActionResult QuestionnaireStatisticsList(int? page, string qry, string sort)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            QuestionnaireStatisticsListViewModel model = new QuestionnaireStatisticsListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            QuestionnaireStatisticsModule module = new QuestionnaireStatisticsModule();
            model.Result = module.DoGetList(model.Filter);
            module.Dispose();

            return View(model);
        }

        [HttpGet]
        public ActionResult QuestionnaireStatisticsView(int? ID)
        {
            QuestionnaireStatisticsDetailsDataModel model = new QuestionnaireStatisticsDetailsDataModel();
            QuestionnaireStatisticsModule module = new QuestionnaireStatisticsModule();
            model = module.DoGetDetailsByID((int)ID);
            module.Dispose();
            return View(model);
        }

        #endregion 能源 問卷統計

        #region 能源 研討會

        [HttpGet]
        public ActionResult TrainList(int? page, string qry, string sort, string fSt, string hSt, string date, string lang)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            TrainListViewModel model = new TrainListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Status = fSt ?? string.Empty;
            model.Filter.HomeDisplay = hSt ?? string.Empty;
            model.Filter.PublishDate = date;
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
            model.Result = (module.DoGetList(model.Filter, language) as TrainListResultModel);
            module.Dispose();
            return View(model);
        }

        [HttpGet]
        public ActionResult TrainAdd()
        {
            TrainDetailsDataModel defaultModel = new TrainDetailsDataModel();
            defaultModel.Data.ActivityDateBegin = DateTime.UtcNow.AddHours(8);
            defaultModel.Data.DeadlineBegin = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.Data.DeadlineEnd = DateTime.UtcNow.AddHours(8).ToString("yyyy\\/MM\\/dd");
            defaultModel.Data.ActivityTimeRange =string.Format("{0}~{1}",  DateTime.UtcNow.AddHours(8).ToString("HH:mm"), DateTime.UtcNow.AddHours(8).ToString("HH:mm"));

            defaultModel.Data.EnrollmentRestrictions = 10;
            return View(defaultModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult TrainAdd(FormCollection form, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
            int identityId = module.DoSaveData(form, language, null, null, files);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("TrainEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl });
        }

        [HttpGet]
        public ActionResult TrainEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("TrainList");
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
            TrainDetailsDataModel model = (module.DoGetDetailsByID((int)ID) as TrainDetailsDataModel);
            if (model == null)
                return RedirectToAction("TrainList");
            //取檔案
            FileModule fileModule = new FileModule();
            model.FilesData = fileModule.GetFiles((int)model.Data.ID, "Train", "M");
            module.Dispose();

            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult TrainEdit(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int? ID = Convert.ToInt32(form["ID"]);
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
            int identityId = module.DoSaveData(form, language, ID, null, files);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("TrainEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult TrainDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAIN);
                module.DoDeleteByID((int)ID);
                messages = "刪除成功";
                module.Dispose();
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
        public ActionResult TrainApplyList(int? page, string qry, string sort, string bDate, string eDate, string lang)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            TrainApplyListViewModel model = new TrainApplyListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.ActivityBeginDate = bDate ?? string.Empty;
            model.Filter.ActivityEndDate = eDate ?? string.Empty;

            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAINAPPLY);
            model.Result = (module.DoGetList(model.Filter, language) as TrainApplyListResultModel);
            module.Dispose();
            return View(model);
        }

        public ActionResult TrainApplyEdit(int? trainID)
        {
            if (trainID == null)
                throw new Exception("查無此研討會相關資料");
            TrainApplyDetailsModel model = new TrainApplyDetailsModel();
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAINAPPLY);
            model = (module.DoGetDetailsByID((int)trainID) as TrainApplyDetailsModel);
            module.Dispose();
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteApply(int trainID, int applyID)
        {
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TRAINAPPLY);
            bool isSuccess = (module as TrainApplyModule).DeleteApply(trainID, applyID);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("TrainApplyEdit", "_SysAdm", new { trainID = trainID });
            module.Dispose();
            return Json(new { success = isSuccess, url = redirectUrl });
        }

        [HttpGet]
        public ActionResult TrainApplyViewData(int trainID, int applyID)
        {
            TrainApplyDataModel model = new TrainApplyDataModel();
            TrainApplyModule module = new TrainApplyModule();
            model = module.GetTrainApplyParticipantsDataByApplyID(trainID, applyID);
            module.Dispose();
            return PartialView("_TrainApplyViewPartial", model);
        }

        [HttpPost]
        public ActionResult TrainApplyViewData(TrainApplyViewModel model)
        {
            bool isSuccess = true;
            string msg = string.Empty;
            TrainApplyModule module = new TrainApplyModule();
            try
            {
                module.SaveApplyParticipantsData(model);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                msg = ex.Message;
            }
            finally
            {
                module.Dispose();
            }
            if (isSuccess)
                msg = "設定完成";
            return Json(new { success = isSuccess, msg = msg });
        }

        #endregion 能源 研討會

        #region 能源 外部連結

        public ActionResult LinksList(int? page, string qry, string sort, string status)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            LinkListViewModel model = new LinkListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Status = status;

            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.LINK);
            model.Result = (module.DoGetList(model.Filter, language) as LinkListResultModel);
            module.Dispose();
            return View(model);
        }

        [HttpGet]
        public ActionResult LinksAdd()
        {
            var defualtModel = new LinkDetailsDataModel();
            defualtModel.Data.狀態 = true;
            return View(defualtModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult LinksAdd(FormCollection form, List<HttpPostedFileBase> cover)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.LINK);
            int identityId = module.DoSaveData(form, language, null, cover, null);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("LinksEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl });
        }

        [HttpGet]
        public ActionResult LinksEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("LinksList");
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.LINK);
            LinkDetailsDataModel model = (module.DoGetDetailsByID((int)ID) as LinkDetailsDataModel);
            if (model == null)
                return RedirectToAction("Login", "SignIn");
            //取圖檔
            ImgModule imgModule = new ImgModule();
            model.Image = imgModule.GetImages((int)model.Data.主索引, "Links", "M");
            module.Dispose();
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult LinksEdit(FormCollection form, List<HttpPostedFileBase> cover)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int? ID = Convert.ToInt32(form["linkID"]);
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.LINK);
            int identityId = module.DoSaveData(form, language, ID, cover, null);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("LinksEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult LinksDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.LINK);
                module.DoDeleteByID((int)ID);
                messages = "刪除成功";
                module.Dispose();
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        public ActionResult LinksKind()
        {
            return View();
        }

        public ActionResult LinksKindEdit()
        {
            return View();
        }

        #endregion 能源 外部連結

        #region 能源 出版品

        [HttpGet]
        public ActionResult BookList(int? page, string qry, string sort, string status, string bDate, string eDate, int? type)
        {
            BookListViewModel model = new BookListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.PublishBeginDate = string.IsNullOrEmpty(bDate) ? DateTime.MinValue : Convert.ToDateTime(bDate);
            model.Filter.PublishEndDate = string.IsNullOrEmpty(eDate) ? DateTime.MaxValue : Convert.ToDateTime(eDate);
            model.Filter.Status = status;
            model.Filter.Type = type == null ? null : type.ToString();
            BookModule module = new BookModule();
            model.Result = module.DoGetList(model.Filter);

            //分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            try
            {
                SelectList typeList = typeModule.CreateTypeManageDropList(type, true, "Book");
                ViewBag.TypeList = typeList;
            }
            catch (TypeIsNotCreateException typeEx)
            {
                TempData["TypeError"] = typeEx.Message;
            }
            finally
            {
                module.Dispose();
                typeModule.Dispose();
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult BookAdd()
        {
            BookDetailsDataModel defaultModel = new BookDetailsDataModel();
            defaultModel.Data.發稿時間 = DateTime.UtcNow.AddHours(8);
            defaultModel.Data.發稿人 = "系統管理員";
            //出版品分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            SelectList typeList = typeModule.CreateTypeManageDropList(0, false, "Book");
            ViewBag.TypeList = typeList;
            return View(defaultModel);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult BookAdd(FormCollection form,
            List<HttpPostedFileBase> cover,
            List<HttpPostedFileBase> full,
            List<HttpPostedFileBase> chapter
            )
        {
            BookModule module = new BookModule();
            int identityId = module.DoSaveData(form, null, cover, full, chapter);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("BookEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl });
        }

        [HttpGet]
        public ActionResult BookEdit(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("BookList");
            BookModule module = new BookModule();

            BookDetailsDataModel model = module.DoGetDetailsByID((int)ID);
            if (model == null)
                return RedirectToAction("Login", "SignIn");
            //取圖檔
            ImgModule imgModule = new ImgModule();
            model.CoverImg = imgModule.GetImages((int)model.Data.主索引, "Book", "M");
            //取全文
            FileModule fileModule = new FileModule();
            model.FullBookFile = fileModule.GetFiles((int)model.Data.主索引, "Book", "M", FileUploadType.MODE1);
            //取章節
            model.ChapterFiles = fileModule.GetFiles((int)model.Data.主索引, "Book", "M", FileUploadType.MODE2);
            //取章節明細檔
            foreach (var cha in model.ChapterFiles)
                model.ChapterDetails.Add(module.GetChapterDetailsByFileID(model.Data.主索引, (int)cha.ID));

            //出版品分類下拉選單
            TypeManageModule typeModule = new TypeManageModule();
            SelectList typeList = typeModule.CreateTypeManageDropList(model.Data.對應分類索引, false, "Book");
            ViewBag.TypeList = typeList;
            module.Dispose();
            imgModule.Dispose();
            fileModule.Dispose();
            typeModule.Dispose();
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult BookEdit(FormCollection form,
      List<HttpPostedFileBase> cover,
      List<HttpPostedFileBase> full,
      List<HttpPostedFileBase> chapter
            )
        {
            int? ID = Convert.ToInt32(form["BookID"]);
            if (ID == null)
                return RedirectToAction("BookList");
            BookModule module = new BookModule();
            int identityId = module.DoSaveData(form, ID, cover, full, chapter);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("BookEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl });
        }

        [HttpPost]
        public JsonResult BookDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                BookModule module = new BookModule();
                module.DoDeleteByID((int)ID);
                messages = "刪除成功";
                module.Dispose();
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        #endregion 能源 出版品

        #region 能源 出版品分類
        /// <summary>
        /// 產品分類若停用判斷是否已有產品使用該分類
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckBookKindStatusHideHasOnUsed(int? ID)
        {
            bool success = true;
            JsonResult resultJson = new JsonResult();
            string messages = string.Empty;

            DBEnergy DB = new DBEnergy();
            int count = DB.出版品主檔.Where(o => o.對應分類索引 == ID && o.顯示狀態).Count();
            if (count > 0)
            {
                success = false;
                messages = "「尚有產品被歸類在此分類且狀態為顯示於，故無法停用。」";
            }
            else
                success = true;
            resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }


        public ActionResult BookKindList(int? page, string qry, string sort, string status, int? type)
        {
            BookModule modul = new BookModule();
            int currentTypeID = modul.GetBookTypeID();
            TypeManageModule typeModule = new TypeManageModule();
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            TypeManageListViewModel model = new TypeManageListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.Status = status ?? string.Empty;
            model.Filter.TypeID = currentTypeID;
            model.Filter.TypeName = model.Filter.TypeID.HasValue ? typeModule.GetTypeNameByTypeID(currentTypeID) : "";

            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
            model.Result = (module.DoGetList(model.Filter, language) as TypeManageListResultModel);
            typeModule.Dispose();
            module.Dispose();
            return View(model);
        }

        [HttpGet]
        public ActionResult BookKindAdd(int? filterTypeID)
        {
            TypeManageDetailsDataModel defaultModel = new TypeManageDetailsDataModel();
            if (filterTypeID.HasValue)
            {
                using (var db = new DBEnergy())
                {
                    var typeName = db.分類主檔.Where(o => o.主索引 == filterTypeID).First().分類名稱;
                    defaultModel = new TypeManageDetailsDataModel()
                    {
                        FilterTypeName = typeName,
                        FilterTypeID = filterTypeID
                    };
                }
            }
            return View(defaultModel);
        }

        [HttpPost]
        public ActionResult BookKindAdd(FormCollection form)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int identityId = 0;
            if (!string.IsNullOrEmpty(form["FilterTypeID"]))
            {
                TypeManageModule mdu = new TypeManageModule();
                identityId = mdu.DoSaveSubTypeData(form, language);
            }
            else
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
                identityId = module.DoSaveData(form, language);
                module.Dispose();
            }
            return RedirectToAction("BookKindEdit", "_SysAdm", new { ID = identityId, filterTypeID = form["FilterTypeID"] });
        }

        [HttpGet]
        public ActionResult BookKindEdit(int? ID, int? filterTypeID)
        {
            if (!ID.HasValue)
                return RedirectToAction("BookKindList");
            TypeManageDetailsDataModel model = null;
            if (!filterTypeID.HasValue)
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
                model = (module.DoGetDetailsByID((int)ID) as TypeManageDetailsDataModel);
                module.Dispose();
            }
            else
            {
                TypeManageModule mdu = new TypeManageModule();
                model = mdu.DoGetSubDetailsByID((int)ID, (int)filterTypeID);
                mdu.Dispose();

                using (var db = new DBEnergy())
                {
                    var typeName = db.分類主檔.Where(o => o.主索引 == filterTypeID).First().分類名稱;
                    model.FilterTypeID = filterTypeID;
                    model.FilterTypeName = typeName;
                }
            }

            if (model == null)
                return RedirectToAction("Login", "SignIn");
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult BookKindEdit(FormCollection form)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int identityId = 0;
            if (!string.IsNullOrEmpty(form["FilterTypeID"]))
            {
                TypeManageModule mdu = new TypeManageModule();
                identityId = mdu.DoSaveSubTypeData(form, language, Convert.ToInt32(form["TypeID"]));
                mdu.Dispose();
            }
            else
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
                identityId = module.DoSaveData(form, language, Convert.ToInt32(form["TypeID"]));
                module.Dispose();
            }
            return RedirectToAction("BookKindEdit", "_SysAdm", new { ID = identityId, filterTypeID = form["FilterTypeID"] });
        }

        [HttpPost]
        public JsonResult BookKindDelete(int? ID, int? filterTypeID)
        {
            bool success = true;
            JsonResult resultJson = new JsonResult();
            string messages = string.Empty;
            try
            {
                #region 檢查分類是否已被使用

                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                bool chkHasUsing = CheckTypeHasUsing(ID, actionName);
                if (chkHasUsing)
                {
                    success = false;
                    messages = "此分類使用中，無法移除";
                    return Json(new { success = success, messages = messages });
                }

                #endregion 檢查分類是否已被使用

                if (!filterTypeID.HasValue)
                {
                    ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.TYPEMANAGE);
                    module.DoDeleteByID((int)ID);
                    module.Dispose();
                }
                else
                {
                    TypeManageModule mdu = new TypeManageModule();
                    mdu.DoDeleteSubDataByID((int)ID, (int)filterTypeID);
                    mdu.Dispose();
                }

                messages = "刪除成功";
                resultJson = Json(new { success = success, messages = messages });
            }
            catch (GeneralTypeRelationExcption exRe)
            {
                success = false;
                resultJson = Json(new { success = success, messages = exRe.Message });
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            return resultJson;
        }

        #endregion 能源 出版品分類

        #region 通知管理

        [HttpGet]
        public ActionResult NoticeList(int? page, string qry, string sort, string dStartDate, string dEndDate, string lang, string type, int? status)
        {
            Language language = PublicMethodRepository.CurrentLanguageEnum;
            NotificationListViewModel model = new NotificationListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = qry ?? string.Empty;
            model.Filter.SortColumn = sort ?? string.Empty;
            model.Filter.DelieverStartDate = dStartDate;
            model.Filter.DelieverEndDate = dEndDate;
            model.Filter.Type = type;
            model.Filter.Status = status == null ? null : status.ToString();
            OutWeb.Modules.Manage.NotificationListModule module = new Modules.Manage.NotificationListModule();

            model.Result = (module.DoGetList(model.Filter, language) as NotificationListResultModel);

            return View(model);
        }

        #region 簡訊維護

        [HttpGet]
        public ActionResult NoticePhoneMsgEdit(int? ID)
        {
            SMSDataModel model;

            if (!ID.HasValue)
            {
                return RedirectToAction("NoticeList", "_SysAdm");
            }

            if (ID == 0)
            {
                model = new SMSDataModel();
                model.Data.DELIEVER_DATE = System.DateTime.Now;
            }
            else
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.SMS);
                model = (module.DoGetDetailsByID((int)ID) as SMSDataModel);
                module.Dispose();
            }

            if (model == null)
                return RedirectToAction("Login", "SignIn");

            //取檔案
            FileModule fileModule = new FileModule();
            model.FilesData = fileModule.GetFiles((int)model.Data.SMS_ID, "SMS", "M");
            fileModule.Dispose();
            return View(model);
        }

        /// <summary>
        /// 取得三竹簡訊及時回傳訊息
        /// </summary>
        /// <param name="ID">ID</param>
        [HttpPost]
        public JsonResult GetSMSImmediateReturn(int ID)
        {
            OutWeb.Modules.Manage.SMSModule mdu = new Modules.Manage.SMSModule();
            var result = mdu.GetImmediateReturn(ID).OrderBy(x => x.USEDFOR); //測試紀錄放前面
            mdu.Dispose();
            return Json(result, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 取得三竹簡訊剩餘餘額
        /// </summary>
        [HttpPost]
        public JsonResult GetSMSAccount()
        {
            OutWeb.Modules.Manage.SMSModule mdu = new Modules.Manage.SMSModule();
            int account = mdu.GetAccount();
            mdu.Dispose();
            return Json(account, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// 取消三竹預約簡訊
        /// </summary>
        /// <param name="ID">ID</param>
        [HttpPost]
        public JsonResult CancelSMSReservation(int ID)
        {
            OutWeb.Modules.Manage.SMSModule mdu = new Modules.Manage.SMSModule();
            int count = mdu.CancelReservation(ID);
            mdu.Dispose();
            return Json(count, JsonRequestBehavior.DenyGet);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult NoticePhoneMsgAddAndEdit(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int? ID = Convert.ToInt32(form["smsID"]);
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.SMS);
            //int identityId = module.DoSaveData(form, language, ID, images, files);
            OutWeb.Modules.Manage.SMSModule mdu = new Modules.Manage.SMSModule();
            var statusData = mdu.DoSaveAndExecData(form, language, ID, images, files);
            mdu.Dispose();
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("NoticePhoneMsgEdit", "_SysAdm", new { ID = statusData.identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl, Status = statusData.success, Count = statusData.count });
        }

        [HttpPost]
        public JsonResult NoticePhoneDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.SMS);
                module.DoDeleteByID((int)ID);
                messages = "刪除成功";
                module.Dispose();
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        #endregion 簡訊維護

        #region 傳真維護

        [HttpGet]
        public ActionResult NoticeFaxEdit(int? ID)
        {
            FaxDataModel model;

            if (!ID.HasValue)
            {
                return RedirectToAction("NoticeList", "_SysAdm");
            }

            if (ID == 0)
            {
                model = new FaxDataModel();
                model.Data.DELIEVER_DATE = System.DateTime.Now;
            }
            else
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.FAX);
                model = (module.DoGetDetailsByID((int)ID) as FaxDataModel);
                module.Dispose();
            }

            if (model == null)
                return RedirectToAction("Login", "SignIn");

            //取檔案
            FileModule fileModule = new FileModule();
            //聯絡清單
            model.MemberFile = fileModule.GetFiles((int)model.Data.FAX_ID, "Fax", "M", FileUploadType.MODE1);
            //附件檔
            model.AttachmentFiles = fileModule.GetFiles((int)model.Data.FAX_ID, "Fax", "M", FileUploadType.MODE2);
            fileModule.Dispose();

            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult NoticeFaxEditAndAdd(FormCollection form, List<HttpPostedFileBase> member, List<HttpPostedFileBase> attachment)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int? ID = Convert.ToInt32(form["faxID"]);
            //ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.FAX);
            OutWeb.Modules.Manage.FaxModule mdu = new Modules.Manage.FaxModule();
            var statusData = mdu.DoSaveAndExecData(form, language, ID, member, attachment);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("NoticeFAXEdit", "_SysAdm", new { ID = statusData.identityId });
            mdu.Dispose();
            return Json(new { Url = redirectUrl, Status = statusData.success, Count = statusData.count, ErrorMsg = statusData.showCustomerMsg });
        }

        [HttpPost]
        public JsonResult NoticeFaxDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.FAX);
                module.DoDeleteByID((int)ID);
                messages = "刪除成功";
                module.Dispose();
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        #endregion 傳真維護

        #region EMAIL

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult NoticeEmailEditAdd(FormCollection form, List<HttpPostedFileBase> images, List<HttpPostedFileBase> files)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.EMAIL);
            int identityId = module.DoSaveData(form, language, null, null, files);
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("NoticeEmailEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl, Action = "Edit" });
        }

        [HttpGet]
        public ActionResult NoticeEmailEdit(int? ID)
        {
            EmailDataModel model;
            if (!ID.HasValue)
            {
                return RedirectToAction("NoticeList", "_SysAdm");
            }

            if (ID == 0)
            {
                model = new EmailDataModel();
                model.Data.DELIEVER_DATE = System.DateTime.Now;
            }
            else
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.EMAIL);
                model = (module.DoGetDetailsByID((int)ID) as EmailDataModel);
                module.Dispose();
            }

            if (model == null)
                return RedirectToAction("Login", "SignIn");

            //取檔案
            FileModule fileModule = new FileModule();
            //聯絡清單
            model.MemberFile = fileModule.GetFiles((int)model.Data.EMAIL_ID, "Email", "M", FileUploadType.MODE1);
            //附件檔
            model.AttachmentFiles = fileModule.GetFiles((int)model.Data.EMAIL_ID, "Email", "M", FileUploadType.MODE2);
            fileModule.Dispose();

            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult NoticeEmailSave(FormCollection form, List<HttpPostedFileBase> filesMember, List<HttpPostedFileBase> filesAttachment)
        {
            string langCode = form["lang"] ?? PublicMethodRepository.CurrentLanguageCode;
            Language language = PublicMethodRepository.GetLanguageEnumByCode(langCode);
            int identityId = 0;
            int? ID = Convert.ToInt32(form["EmailID"]);
            ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.EMAIL);

            identityId = module.DoSaveData(form, language, ID, filesMember, filesAttachment);

            var redirectUrl = new UrlHelper(Request.RequestContext).Action("NoticeEmailEdit", "_SysAdm", new { ID = identityId });
            module.Dispose();
            return Json(new { Url = redirectUrl, Action = "Edit" });
        }

        [HttpPost]
        public JsonResult NoticeEmailDelete(int? ID)
        {
            bool success = true;
            string messages = string.Empty;
            try
            {
                ListModuleService module = ListFactoryService.Create(Enums.ListMethodType.EMAIL);
                module.DoDeleteByID((int)ID);
                messages = "刪除成功";
                module.Dispose();
            }
            catch (Exception ex)
            {
                success = false;
                messages = ex.Message;
            }
            var resultJson = Json(new { success = success, messages = messages });
            return resultJson;
        }

        #endregion EMAIL

        #endregion 通知管理

        #region 修改密碼

        /// 管理員密碼變更
        ///
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

        #endregion 修改密碼

        #region 登入來源

        public ActionResult IP(int? page, string bDate, string eDate, string qry)
        {
            IPListViewModel model = new IPListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.BeginDate = string.IsNullOrEmpty(bDate) ? DateTime.MinValue : Convert.ToDateTime(bDate);
            model.Filter.EndDate = string.IsNullOrEmpty(eDate) ? DateTime.MaxValue : Convert.ToDateTime(eDate);
            model.Filter.QueryString = qry;
            IpHistoryModule ipMdu = new IpHistoryModule();
            model.Result = ipMdu.GetList(model.Filter);
            ipMdu.Dispose();
            return View(model);
        }

        #endregion 登入來源
    }
}