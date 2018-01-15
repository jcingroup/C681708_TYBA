using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace OutWeb.Service
{
    public static class CustomHtmlHelper
    {
        /// <summary>
        /// 去除字串內含有的Html Tag以及截斷字元長度超過100
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static MvcHtmlString FormatHtmlString(this HtmlHelper helper, string str)
        {
            string convertStr = string.Empty;
            string htmlTagStr = Regex.Replace(str, @"<[^>]+>|&nbsp;|&ldquo;|&rdquo;", "").Trim();
            htmlTagStr = htmlTagStr.Length > 100 ? htmlTagStr.Substring(0, 100) : htmlTagStr;
            return MvcHtmlString.Create(htmlTagStr);
        }
    }
}