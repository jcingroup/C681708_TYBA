using Microsoft.Security.Application;
using OutWeb.ActionFilter;
using OutWeb.Models.FrontEnd.SearchSiteModels;
using OutWeb.Modules.FrontEnd;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]
    public class SearchController : Controller
    {
        public SearchController()
        {
            ViewBag.IsFirstPage = false;
        }

        // GET: Search
        [ValidateInput(false)]
        [HttpGet]
        public ActionResult Index(string str, int? page)
        {
            return RedirectToAction("Search", new { str = str, page = page });
        }

        public ActionResult Search(string str, int? page)
        {
            SearchListViewModel model = new SearchListViewModel();
            model.Filter.CurrentPage = page ?? 1;
            model.Filter.QueryString = str;
            SearchModule module = new SearchModule();
            model.Result = module.SearchSite(model.Filter);
            module.Dispose();
            return View(model);
        }
    }
}