using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.IO;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace DMSERoute.Helpers.Html
{
    public class HtmlBuilder
    {
        public static string RenderViewToHtml(ControllerContext controllerContext, string viewName, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            using (var sw = new StringWriter())
            {
                try
                {
                    ViewEngineResult viewResult = null;
                    lock (ViewEngines.Engines)
                    {
                        viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                    }
                    ViewContext viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData, sw);
                    viewResult.View.Render(viewContext, sw);

                    return sw.GetStringBuilder().ToString();

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public string RenderViewToHtml(ControllerContext controllerContext, ViewEngineResult viewEngine, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            using (var sw = new StringWriter())
            {
                try
                {
                    ViewContext viewContext = new ViewContext(controllerContext, viewEngine.View, viewData, tempData, sw);
                    viewEngine.View.Render(viewContext, sw);

                    return sw.GetStringBuilder().ToString();

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public static Dictionary<string, string> ParalledRenderViewToHtml(ControllerContext controllerContext, ViewDataDictionary viewData, TempDataDictionary tempData, params string[] views)
        {
            var dictionary = new ConcurrentDictionary<string, string>();
            var taskList = new Task[views.Length];
            var builder = new HtmlBuilder();

            for (int i = 0; i < views.Length; i++)
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, views[i]);
                taskList[i] = Task.Factory.StartNew((param) =>
                {
                    var obj = param as HtmlBuilderParam;
                    var html = string.Empty;
                    using (var sw = new StringWriter())
                    {
                        try
                        {
                            ViewContext viewContext = new ViewContext(controllerContext, obj.ViewResult.View, viewData, tempData, sw);
                            if (viewContext != null)
                            {
                                obj.ViewResult.View.Render(viewContext, sw);
                            }

                            html = sw.GetStringBuilder().ToString();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    dictionary.AddOrUpdate(obj.ViewName, html, (dictKey, oldValue) => html);


                }, new HtmlBuilderParam() { ViewName = views[i], ViewResult = viewResult });
            }

            Task.WaitAll(taskList);

            return new Dictionary<string, string>(dictionary);
        }

        private class HtmlBuilderParam
        {
            public string ViewName { get; set; }
            public ViewEngineResult ViewResult { get; set; }
        }
    }
}
