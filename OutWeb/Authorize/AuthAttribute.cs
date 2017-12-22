
using OutWeb.Modules.Manage;
using OutWeb.Provider;
using OutWeb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OutWeb.Authorize
{
    public class AuthAttribute : AuthorizeAttribute
    {
        public string ControllerID
        {
            get; internal set;
        }

        public string ActionID { get; internal set; }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {

            this.ControllerID = filterContext.RouteData.Values["controller"].ToString();
            this.ActionID = filterContext.RouteData.Values["action"].ToString();
            var parameters = filterContext.ActionDescriptor.GetParameters();
            string lang = "";
            //判斷action是否有帶lang參數
            foreach (var parameter in parameters)
            {
                if (parameter.ParameterName == "lang")
                {
                    lang = filterContext.HttpContext.Request[parameter.ParameterName];
                    break;
                }
            }
            //var lang = parameters.Select(s => new
            //{
            //    Name = s.ParameterName,
            //    Value = filterContext.HttpContext.Request[s.ParameterName]
            //})
            //.Where(w => w.Name == "lang").FirstOrDefault();

            if (string.IsNullOrEmpty(lang))
                PublicMethodRepository.CurrentLanguageEnum = Enums.Language.NotSet;
            else
                PublicMethodRepository.CurrentLanguageEnum = PublicMethodRepository.GetLanguageEnumByCode(lang);


            base.OnAuthorization(filterContext);

            //string ip = filterContext.RequestContext.HttpContext.Request.UserHostAddress;
            //IpHistoryModule.WriteIp(ip);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (UserProvider.Instance.User == null)
            {
                return false;
            }
            return true;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {

            if (UserProvider.Instance.User == null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    filterContext.HttpContext.Response.StatusCode = 403;
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            Error = "NotAuthorized",
                            LogOnUrl = urlHelper.Action("SignInFail", "SignIn")
                        }
                    };
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        action = "SignInFail",
                        controller = "SignIn"
                    }));
                }

            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

    }
}