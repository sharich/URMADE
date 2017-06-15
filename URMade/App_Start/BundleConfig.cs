using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;
using JavaScriptEngineSwitcher.Core;
using JavaScriptEngineSwitcher.Msie;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Optimization;

namespace URMade
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            JsEngineSwitcher engineSwitcher = JsEngineSwitcher.Instance;
            engineSwitcher.EngineFactories
                .AddMsie(new MsieSettings
                {
                    UseEcmaScript5Polyfill = true,
                    UseJson2Library = true
                })
                ;

            engineSwitcher.DefaultEngineName = MsieJsEngine.EngineName;


            bundles.UseCdn = true;
            var cssTransformer = new StyleTransformer();
            var jsTransformer = new ScriptTransformer();
            var nullOrderer = new NullOrderer();

            Action<string, string[]> addScriptBundle = (string bundleName, string[] includes) =>
            {
                var bundle = new ScriptBundle(bundleName);
                bundle.Include(includes);
                bundle.Transforms.Add(jsTransformer);
                bundle.Orderer = nullOrderer;
                bundles.Add(bundle);
            };

            Action<string, string[]> addStyleBundle = (string bundleName, string[] includes) =>
            {
                var bundle = new StyleBundle(bundleName);
                bundle.Include(includes);
                bundle.Transforms.Add(cssTransformer);
                bundle.Orderer = nullOrderer;
                bundles.Add(bundle);
            };

            addScriptBundle("~/bundles/jquery", new[] { "~/Scripts/jquery-{version}.js", "~/Scripts/jquery.unobtrusive-ajax.min.js" });
            addScriptBundle("~/bundles/jqueryval", new[] { "~/Scripts/jquery.validate*" });
            addScriptBundle("~/bundles/modernizr", new[] { "~/Scripts/modernizr-*" });
            addScriptBundle("~/bundles/lodash", new[] { "~/Scripts/lodash.js" });
            addScriptBundle("~/bundles/editable-list", new[] {
                "~/Scripts/lodash.js",
                "~/Scripts/dragula.min.js",
                "~/Scripts/editable-list.js" });

            addScriptBundle("~/bundles/tinymce", new[] {
                "~/Scripts/tinymce/tinymce.min.js",
                "~/Scripts/tinymce/jquery.tinymce.min.js" });

            addScriptBundle("~/bundles/bootstrap", new[] {
                "~/Scripts/bootstrap.js",
                "~/Scripts/doT.min.js",
                "~/Scripts/respond.js",
                "~/Scripts/global.js"});


            addScriptBundle("~/bundles/dragula", new[] { "~/Scripts/dragula.min.js" });

            addStyleBundle("~/Content/css", new[] {
                "~/Content/bootstrap.css",
                "~/Content/dragula.min.css",
                "~/Content/site.less" });
        }
    }
}
