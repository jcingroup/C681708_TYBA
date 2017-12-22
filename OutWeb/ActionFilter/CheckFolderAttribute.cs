using System.IO;
using System.Web;
using System.Web.Mvc;
namespace OutWeb.ActionFilter
{
    public class CheckFolderAttribute : ActionFilterAttribute
    {


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string temp = HttpContext.Current.Server.MapPath("~/Content/Upload/Manage/Images/Temp");
            if (Directory.Exists(temp))
            {
                var files = Directory.GetFiles(temp);
                if (files.Length > 0)
                {
                    foreach (var f in files)
                        File.Delete(f);
                }
                Directory.Delete(temp);
            }

            string[] dirAry = new string[] { "Content", "Upload", "Manage", "Images", "Temp" };
            string[] dirAry2 = new string[] { "Content", "Upload", "Manage", "Files", "Temp" };
            string[] dirAry3 = new string[] { "MailJson", "finish" };
            string[] dirAry4 = new string[] { "Content", "ExcelTemp"};



            string serverRoorDir = HttpContext.Current.Server.MapPath("~");
            string dir = string.Empty;
            foreach (string d in dirAry)
            {
                dir = serverRoorDir += @"\" + d;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            serverRoorDir = HttpContext.Current.Server.MapPath("~");
            string dir2 = string.Empty;
            foreach (string d in dirAry2)
            {
                dir2 = serverRoorDir += @"\" + d;
                if (!Directory.Exists(dir2))
                    Directory.CreateDirectory(dir2);
            }

            serverRoorDir = HttpContext.Current.Server.MapPath("~");
            string dir3 = string.Empty;
            foreach (string d in dirAry3)
            {
                dir3 = serverRoorDir += @"\" + d;
                if (!Directory.Exists(dir3))
                    Directory.CreateDirectory(dir3);
            }

            serverRoorDir = HttpContext.Current.Server.MapPath("~");
            string dir4 = string.Empty;
            foreach (string d in dirAry4)
            {
                dir4 = serverRoorDir += @"\" + d;
                if (!Directory.Exists(dir4))
                    Directory.CreateDirectory(dir4);
            }
            base.OnActionExecuting(filterContext);
        }
    }
}