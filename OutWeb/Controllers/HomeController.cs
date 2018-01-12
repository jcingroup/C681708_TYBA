using OutWeb.ActionFilter;
using OutWeb.Entities;
using OutWeb.Models.FrontEnd.CaseFrontEndModels;
using OutWeb.Models.FrontEnd.CourseFrontEndModels;
using OutWeb.Models.FrontEnd.EditModels;
using OutWeb.Models.FrontEnd.HomeMultipleModels;
using OutWeb.Models.FrontEnd.NewsFrontEndModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using OutWeb.Repositories;
using System;
using System.Linq;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
    public class HomeController : Controller
    {
        public HomeController()
        {
            ViewBag.IsFirstPage = false;
        }

        public ActionResult Index()
        {
            ViewBag.IsFirstPage = true;
            HomeFrontListDataModel model = new HomeFrontListDataModel();

            #region 新聞

            NewsListFrontViewModel news = new NewsListFrontViewModel();
            using (var newsModule = new NewsFrontModule())
            {
                news.Filter.BeginDate = DateTime.MinValue;
                news.Filter.EndDate = DateTime.MaxValue;
                news.Result = newsModule.GetList(news.Filter);
                news.Result.Data = news.Result.Data.Where(o => o.DisplayHome == true).ToList();
                model.News.Result.Data = news.Result.Data.Take(5).ToList();
                foreach (var item in model.News.Result.Data)
                    PublicMethodRepository.HtmlDecode(item);


            }

            #endregion 新聞

            #region 線上課程
            CourseListFrontViewModel course = new CourseListFrontViewModel();
            using (var courseModule = new CourseFrontModule())
            {
                course.Result = courseModule.GetList();
                model.Course.Result.Data = course.Result.Data.Take(5).ToList();
                foreach (var item in model.Course.Result.Data)
                    PublicMethodRepository.HtmlDecode(item);


            }


            #endregion 線上課程

            #region 節能案例
            CaseListFrontViewModel caseData = new CaseListFrontViewModel();
            using (var caseModule = new CaseFrontModule())
            {
                caseData.Result.ListData = caseModule.GetListForFront();
                model.CaseData.Result.ListData = caseData.Result.ListData;
                //model.CaseData.Result.GroupData = caseData.Result.GroupData.Take(6).ToDictionary(k => k.Key, v => v.Value);
                foreach (var item in model.CaseData.Result.ListData)
                    PublicMethodRepository.HtmlDecode(item);
            }

            #endregion 節能案例

            return View(model);
        }        

        // 靜態-聯絡我們
        public ActionResult ContactUs()
        {
            return View();
        }
    }
}