using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.QuestionnairesModels;
using OutWeb.Models.Manage.QuestionnairesModels;
using OutWeb.Modules.FontEnd;
using OutWeb.Provider;
using System.Collections.Generic;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
    public class QuestionController : Controller
    {
        public QuestionController()
        {
            ViewBag.IsFirstPage = false;
        }

        // GET: Question
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // 套程式-問卷
        public ActionResult List(int? page)
        {
            QuestListViewModel model = new QuestListViewModel();
            using (var Module = new QuestionnairesFrontModule())
            {
                model.Filter.Page = page ?? 1;
                model = Module.GetList(model.Filter.Page);
            }
            return View(model);
        }

        public ActionResult Content(int? ID)
        {
            if (ID == null)
                return RedirectToAction("List");
            QuestionDetailsDataModel model;
            using (var mdu = new Modules.Manage.QuestionnairesModule())
            {
                model = mdu.DoGetDetailsByID((int)ID);
                if (model.IsSignIn != null && model.IsSignIn.Equals("on"))
                {
                    if (UserFrontProvider.Instance.User == null)
                    {
                        string url = Request.Url.OriginalString;
                        TempData["LoginValid"] = "請先登入再填寫問卷.";
                        return RedirectToAction("Login", "Home", new { redirectUrl = url });
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult Content(int? ID, List<QuestionDetailsAnswerModel> Answer)
        {
            bool success = true;
            string messages = "問卷填寫完成.";
            string redirectUrl = string.Empty;
            try
            {
                using (var Module = new QuestionnairesFrontModule())
                {
                    Module.InsertAnswer((int)ID, Answer);
                    if (UserFrontProvider.Instance.User != null)
                        redirectUrl = new UrlHelper(Request.RequestContext).Action("QuestionList", "User");
                    else
                        redirectUrl = new UrlHelper(Request.RequestContext).Action("List", "Question");
                }
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