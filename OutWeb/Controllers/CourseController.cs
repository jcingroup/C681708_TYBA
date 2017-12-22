using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.CourseFrontEndModels;
using OutWeb.Models.Manage.ManageCourseModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using System;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
    public class CourseController : Controller
    {
        // GET: Products
        public CourseController()
        {
            ViewBag.IsFirstPage = false;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // 套程式-能源新聞資訊
        // 列表
        public ActionResult List()
        {
            CourseListFrontViewModel model = new CourseListFrontViewModel();
            try
            {
                using (var module = new CourseFrontModule())
                {
                    model.Result = module.GetList();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
            return View(model);
        }

        public ActionResult Content(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("List");
            CourseDetailsDataModel model;
            using (var module = new CourseModule())
            {
                model = (module.DoGetDetailsByID((int)ID) as CourseDetailsDataModel);
            }

            if (model == null)
                return RedirectToAction("Login", "Login");
            //取檔案
            using (var fileModule = new FileModule())
            {
                model.FilesData = fileModule.GetFiles((int)model.Data.主索引, "Course", "M");
            }
            return View(model);
        }
    }
}