using OutWeb.Entities;
using System;
using System.Web.Mvc;

namespace OutWeb.Authorize
{
    public class ErrorHandlerAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var urlHelper = new UrlHelper(filterContext.RequestContext);
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        filterContext.Exception.Message,
                        filterContext.Exception.StackTrace,
                        Error = "ErrorHandler",
                        LogOnUrl = urlHelper.Action("SignInFail", "SignIn")
                    }
                };
            }
            else
            {
                //base.OnException(filterContext);
                Exception exception = filterContext.Exception;
                int logGuId = new System.Random().Next(0, 32767);
                LOGERR Log = new LOGERR();
                Log.ERR_GID = logGuId;
                Log.ERR_SRC = exception.Source;
                Log.ERR_SMRY = string.Format("messages：{0} 。 innerException：{1}", exception.Message, exception.InnerException);
                Log.ERR_DESC = exception.StackTrace;
                Log.LOG_DTM = DateTime.UtcNow.AddHours(8);
                TYBADB DB = new TYBADB();
                DB.LOGERR.Add(Log);
                DB.SaveChanges();


                string controllerName = (string)filterContext.RouteData.Values["controller"];
                string actionName = (string)filterContext.RouteData.Values["action"];
                HandleErrorInfo model = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
                filterContext.Result = new ViewResult
                {
                    ViewName = View,
                    MasterName = Master,
                    ViewData = new ViewDataDictionary<HandleErrorInfo>(model),
                    TempData = filterContext.Controller.TempData
                };
                var typedResult = filterContext.Result as ViewResult;
                typedResult.ViewData.Add("LogGuId", logGuId);
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;


                //var typedResult = filterContext.Result as ViewResult;
                //if (typedResult != null)
                //{
                //    var tmpModel = typedResult.ViewData.Model;
                //    typedResult.ViewData = filterContext.Controller.ViewData;
                //    typedResult.ViewData.Model = tmpModel;
                //    typedResult.ViewData.Add("LogGuId", logGuId);
                //    filterContext.Result = typedResult;
                //}
            }

        }
    }
}