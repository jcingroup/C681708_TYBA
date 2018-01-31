using OutWeb.Models.Manage.ResultModels;
using OutWeb.Modules.Manage;
using System.Web;
using System.Web.Mvc;
using System.Linq;
namespace OutWeb.Controllers
{
    public class ResultController : Controller
    {
        public ResultController()
        {
            ViewBag.IsFirstPage = false;
        }

        // GET: Result
        public ActionResult Index()
        {
            return View("List");
        }

        // 比賽成績公告
        [HttpGet]
        public ActionResult List(int? page)
        {
            ResultListViewModel model = new ResultListViewModel();
            model.Filter.CurrentPage = page ?? 1;

            using (var module = new ResultModule())
            {
                model.Result = module.DoGetList(model.Filter);
                model.Result.Data = model.Result.Data.Where(o => o.DISABLE == false).ToList();
            }

            return View(model);
        }


        public ActionResult Content(int? ID)
        {
            if (!ID.HasValue)
                return RedirectToAction("ResultList");

            ResultDetailsDataModel model = new ResultDetailsDataModel();
            using (var module = new ResultModule())
            {
                model = module.DoGetDetailsByID((int)ID);
                if (model.Data == null)
                    return RedirectToAction("List");
                model.Data.CONTENT = HttpUtility.HtmlDecode(model.Data.CONTENT);
            }

            return View(model);
        }
    }
}