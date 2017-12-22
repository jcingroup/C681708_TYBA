
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
    public class AuthAttributeFront : AuthorizeAttribute
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
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (UserFrontProvider.Instance.User == null)
            {
                return false;
            }
            return true;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {

            if (UserFrontProvider.Instance.User == null)
            {
                string url = filterContext.RequestContext.HttpContext.Request.Url.OriginalString;
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                { "action", "SignInFail" },
                { "controller", "Login" },
                { "redirectUrl", url } }

                //    new RouteValueDictionary(new
                //{
                //    action = "SignInFail",
                //    controller = "Login",
                //},
                //{ "someQuerystring", "someValue" }

                );
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

    }
}