using OutWeb.Modules.Manage;
using System.Web.Mvc;
using System.Web.Routing;

namespace OutWeb.ActionFilter
{
    public class SiteCounterFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IpHistoryModule ipMdu = new IpHistoryModule();
            string ip = filterContext.RequestContext.HttpContext.Request.UserHostAddress;
            ipMdu.SiteCountHandler(ip, "front");

            //新聞人氣
            string controllerID = filterContext.RouteData.Values["controller"].ToString();
            string actionID = filterContext.RouteData.Values["action"].ToString();
            string method = filterContext.RequestContext.HttpContext.Request.HttpMethod;
            if (controllerID.Equals("News") && actionID.Equals("Content") && method.ToUpper().Equals("GET"))
            {
                var parameters = filterContext.ActionDescriptor.GetParameters();
                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterName == "ID")
                    {
                        var id = filterContext.HttpContext.Request[parameter.ParameterName];
                        int tryID = 0;
                        if (!int.TryParse(id, out tryID))
                        {
                            string url = filterContext.RequestContext.HttpContext.Request.Url.OriginalString;
                            filterContext.Result = new RedirectToRouteResult(
                                new RouteValueDictionary {
                                { "action", "Index" },
                                { "controller", "Home" }
                                });
                            break;
                        }
                        else
                        {

                            if (id != null && !id.Equals("0"))
                            {
                                ipMdu.AddCount("NewsFront", id);
                            }
                            break;
                        }

                    }
                }
            }
            ipMdu.Dispose();
        }
    }
}