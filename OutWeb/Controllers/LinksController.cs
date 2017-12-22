using OutWeb.ActionFilter;
using OutWeb.Enums;
using OutWeb.Models.FrontEnd.LinkFrontEndModels;
using OutWeb.Models.Manage.ManageLinkModels;
using OutWeb.Modules.FrontEnd;
using OutWeb.Modules.Manage;
using OutWeb.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    [SiteCounterFilter]

    public class LinksController : Controller
    {
        // GET: Links
        public LinksController()
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
            LinkListFrontViewModel model = new LinkListFrontViewModel();
            try
            {
                using (var module = new LinkFrontModule())
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
            LinkModule module = new LinkModule();

            LinkDetailsDataModel model = (module.DoGetDetailsByID((int)ID) as LinkDetailsDataModel);
            if (model == null)
                return RedirectToAction("Login", "Login");
            //取圖檔
            ImgModule imgModule = new ImgModule();
            model.Image = imgModule.GetImages((int)model.Data.主索引, "Links", "M");
            module.Dispose();
            return View(model);
        }
    }
}