using System.Web.Optimization;

namespace Gateway.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles, string environment)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Content/js/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Content/js/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Content/js/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Content/js/bootstrap.js",
                "~/Content/js/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/spinner").Include(
                "~/Content/js/spinner.js",
                "~/Content/js/loading.js"
                ));

            bundles.Add(new StyleBundle("~/styles/css").Include(
                "~/Content/css/bootstrap.css",
                "~/Content/css/simple-sidebar.css",
                "~/Content/css/datatable.css",
                "~/Content/css/treeview.css",
                "~/Content/css/font-awesome.min.css",
                "~/Content/css/Site.css"
                ));
        }
    }
}