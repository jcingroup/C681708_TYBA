using OutWeb.ActionFilter;
using OutWeb.Enums;
using OutWeb.Models.FrontEnd.BookFrontEndModels;
using OutWeb.Models.Manage.ManageBookModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
    public class BookController : Controller
    {
        public BookController()
        {
            ViewBag.IsFirstPage = false;
        }

        // GET: Book
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // 套程式-出版品
        public ActionResult List(string qry, string bDate, string eDate, int? type)
        {
            Regex rgx = new Regex(@"\d{4}(?:/\d{1,2}){2}");

            if (!string.IsNullOrEmpty(bDate) && (!rgx.IsMatch(bDate)))
                return RedirectToAction("List");


            if (!string.IsNullOrEmpty(eDate) && (!rgx.IsMatch(eDate)))
                return RedirectToAction("List");

            BookFrontEndListDataModel model = new BookFrontEndListDataModel();
            try
            {
                using (var module = new BookFrontModule())
                {
                    model.Filter.QueryString = qry ?? string.Empty;
                    model.Filter.PublishBeginDate = string.IsNullOrEmpty(bDate) ? DateTime.MinValue : Convert.ToDateTime(bDate);
                    model.Filter.PublishEndDate = string.IsNullOrEmpty(eDate) ? DateTime.MaxValue : Convert.ToDateTime(eDate);
                    model.Filter.Type = type == null ? module.GetDefualtType().主索引.ToString() : type.ToString();
                    model.Result = module.GetList(model.Filter);

                    ViewBag.Subnav = "sub-book" + model.Filter.Type;
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
            BookDetailsDataModel model = null;

            using (var module = new BookModule())
            {
                model = module.DoGetDetailsByID((int)ID);
                if (model == null)
                    return RedirectToAction("Login", "Login");
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
                model.ChapterDetails = model.ChapterDetails.OrderByDescending(o => o.SQ).ToList();
                model.ChapterFiles = model.ChapterFiles.OrderBy(o => model.ChapterDetails.FindIndex(a => a.MapFileID == o.ID)).ToList();
                ViewBag.Subnav = "sub-book" + type;
            }
            using (var module = new BookFrontModule())
            {
                ViewBag.BreadcrumbCenter = type == null ? module.GetDefualtType().分類名稱 : module.GetTypeByID((int)type).分類名稱;
                ViewBag.Breadcrumb = model.Data.名稱;
            }
            return View(model);
        }
    }
}