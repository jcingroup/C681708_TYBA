using OutWeb.Entities;
using System;
using System.Web.Mvc;

namespace OutWeb.Authorize
{
    public class ErrorHandlerAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            Exception exception = filterContext.Exception;
            int logGuId = new System.Random().Next(0, 32767);
            LOGERR Log = new LOGERR();
            Log.ERR_GID = logGuId;
            Log.ERR_SRC = exception.Source;
            Log.ERR_SMRY = string.Format("messages：{0} 。 innerException：{1}", exception.Message, exception.InnerException);
            Log.ERR_DESC = exception.StackTrace;
            Log.LOG_DTM = DateTime.UtcNow.AddHours(8);
            DBEnergy DB = new DBEnergy();
            DB.LOGERR.Add(Log);
            DB.SaveChanges();

            var typedResult = filterContext.Result as ViewResult;
            if (typedResult != null)
            {
                var tmpModel = typedResult.ViewData.Model;
                typedResult.ViewData = filterContext.Controller.ViewData;
                typedResult.ViewData.Model = tmpModel;
                typedResult.ViewData.Add("LogGuId", logGuId);
                filterContext.Result = typedResult;
            }


            //if (filterContext.HttpContext.Request.IsAjaxRequest())
            //{
            //    var urlHelper = new UrlHelper(filterContext.RequestContext);
            //    filterContext.HttpContext.Response.StatusCode = 500;
            //    filterContext.Result = new JsonResult
            //    {
            //        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            //        Data = new
            //        {
            //            filterContext.Exception.Message,
            //            filterContext.Exception.StackTrace,
            //            Error = "ErrorHandler",
            //            LogOnUrl = urlHelper.Action("SignInFail", "SignIn")
            //        }
            //    };
            //    filterContext.ExceptionHandled = true;
            //}
            //else
            //{
            //    base.OnException(filterContext);
            //    Exception exception = filterContext.Exception;
            //    int logGuId = new System.Random().Next(0, 32767);
            //    LOGERR Log = new LOGERR();
            //    Log.ERR_GID = logGuId;
            //    Log.ERR_SRC = exception.Source;
            //    Log.ERR_SMRY = string.Format("messages：{0} 。 innerException：{1}", exception.Message, exception.InnerException);
            //    Log.ERR_DESC = exception.StackTrace;
            //    Log.LOG_DTM = DateTime.UtcNow.AddHours(8);
            //    DBEnergy DB = new DBEnergy();
            //    DB.LOGERR.Add(Log);
            //    DB.SaveChanges();

            //    var typedResult = filterContext.Result as ViewResult;
            //    if (typedResult != null)
            //    {
            //        var tmpModel = typedResult.ViewData.Model;
            //        typedResult.ViewData = filterContext.Controller.ViewData;
            //        typedResult.ViewData.Model = tmpModel;
            //        typedResult.ViewData.Add("LogGuId", logGuId);
            //        filterContext.Result = typedResult;
            //    }
            //}

        }
    }
}