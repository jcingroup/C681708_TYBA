using OutWeb.Entities;
using OutWeb.Models.Manage.ManageNotificationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OutWeb.Controllers
{
    public class ViewTemplateController : Controller
    {
        // GET: ViewTemplate
        public ActionResult Email()
        {
            return View(new EMAIL());
        }
    }
}