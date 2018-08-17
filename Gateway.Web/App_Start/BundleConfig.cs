﻿using System.Web.Optimization;

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
                "~/Content/js/respond.js",
                "~/Content/js/bootstrap-quick-search.js"));

            bundles.Add(new ScriptBundle("~/bundles/spinner").Include(
                "~/Content/js/spinner.js",
                "~/Content/js/row-click.js",
                "~/Content/js/loading.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/charts").Include(
                "~/Content/js/d3.js",
                "~/Content/js/c3.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/datetime").Include(
                "~/Content/js/moment.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/customQueueCharts").Include(
                "~/Scripts/Queues/Shared/SelectionFilter.js",
                "~/Scripts/Queues/Shared/ChartRenderer.js",
                "~/Scripts/Queues/ControllerQueueCharts.js",
                "~/Scripts/Queues/ControllersQueueCharts.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/monitoring").Include(
                "~/Scripts/Monitoring/Monitoring.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/server-resources").Include(
                "~/Scripts/ServerResources/ServerResources.js"
            ));

            bundles.Add(new StyleBundle("~/styles/css").Include(
                "~/Content/css/bootstrap.css",
                "~/Content/css/font-awesome.css",
                "~/Content/css/simple-sidebar.css",
                "~/Content/css/datatable.css",
                "~/Content/css/treeview.css",
                "~/Content/css/Site.css",
                "~/Content/css/bootstrap-quick-search.css"
                ));

            bundles.Add(new StyleBundle("~/styles/charts").Include(
                "~/Content/css/c3.css"
                ));

        }
    }
}