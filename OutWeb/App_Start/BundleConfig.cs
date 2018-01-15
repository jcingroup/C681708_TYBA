using System.Web.Optimization;

namespace OutWeb.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Custom/JS").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-ui.min.js",
                        "~/ckeditor/ckeditor.js",
                        "~/ckfinder/ckfinder.js",
                        "~/Scripts/datepicker-zh-TW.js",
                        "~/Scripts/clockpicker.js",
                        "~/Scripts/jq_initialization.js"
                        ));

     
            bundles.Add(new StyleBundle("~/Custom/Css").Include(
                      "~/Content/css/jquery-ui.css",
                      "~/Content/css/clockpicker.css",
                      "~/Content/css/standalone.css"
                      ));
        }
    }
}