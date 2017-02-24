using System.Web;
using System.Web.Optimization;

namespace SMT
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/site.css"));

            #region Tutor
            bundles.Add(new StyleBundle("~/tutor/bundles/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/plugins/pnotify/jquery.pnotify.css",
                "~/Content/plugins/pnotify/jquery.pnotify.icons.css",
                "~/Content/Tutor/site.css"));
            bundles.Add(new ScriptBundle("~/tutor/bundles/js").Include(
                "~/Scripts/load.js",
                "~/Content/plugins/pnotify/jquery.pnotify.min.js",
                "~/Scripts/utilidades.js"));
            #endregion

            #region Panel

            bundles.Add(new ScriptBundle("~/panel/bundles/js").Include(
                "~/Scripts/load.js",
                "~/Content/plugins/pnotify/jquery.pnotify.min.js",
                "~/Scripts/utilidades.js",
                "~/Areas/Panel/Scripts/app.js"
            ));

            bundles.Add(new StyleBundle("~/panel/bundles/css").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/theme/font-awesome/css/font-awesome.min.css",
                "~/Content/plugins/pnotify/jquery.pnotify.css",
                "~/Content/plugins/pnotify/jquery.pnotify.icons.css",
                "~/Content/plugins/pace/themes/blue/pace-theme-flash.css",
                "~/Areas/Panel/Content/site.css",
                "~/Areas/Panel/Content/smt.panel.css"
            ));

            bundles.Add(new StyleBundle("~/panel/bundles/skin").Include(
                "~/Areas/Panel/Content/AdminLTE.css"
            ));

            bundles.Add(new StyleBundle("~/panel/bundles/skin-blue").Include(
                "~/Areas/Panel/Content/skins/skin-blue.min.css"
            ));

            #endregion
        }
    }
}
