using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO.Compression;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using DevExpress.Web.Mvc;
using DMSERoute.Helpers;
using eRoute.Models;
using Settings;

namespace eRoute
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    [LogAndRedirectOnError]
    public class MvcApplication : System.Web.HttpApplication
    {
        public static List<Language> ListLanguage = new List<Language>();

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "DashBoard", action = "Home", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders.DefaultBinder = new DevExpress.Web.Mvc.DevExpressEditorsBinder();

            #region Utility Config
            ConnectionStringSettings conn = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            Utility.Connect = conn.ConnectionString;
            Utility.DefaultLanguage = "EN";
            Utility.DefaultLanguageID = 1;
            Utility.info = new System.Globalization.CultureInfo("en-US");
            Utility.info.NumberFormat.NumberDecimalDigits = 2;
            Utility.info.NumberFormat.CurrencyDecimalDigits = 2;
            Utility.info.NumberFormat.CurrencyGroupSeparator = ",";
            Utility.info.NumberFormat.CurrencyDecimalSeparator = ".";
            Utility.info.NumberFormat.CurrencySymbol = String.Empty;
            Utility.info.DateTimeFormat.FullDateTimePattern = "dd-MM-yyyy HH:mm";
            Utility.info.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
            Utility.info.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";

            Utility.infoEN = new System.Globalization.CultureInfo("en-US");
            Utility.infoEN.NumberFormat.NumberDecimalDigits = 2;
            Utility.infoEN.NumberFormat.CurrencyDecimalDigits = 2;
            Utility.infoEN.NumberFormat.CurrencyGroupSeparator = "";
            Utility.infoEN.NumberFormat.CurrencyDecimalSeparator = ".";
            Utility.infoEN.NumberFormat.CurrencySymbol = String.Empty;
            Utility.infoEN.DateTimeFormat.FullDateTimePattern = "MM-dd-yyyy HH:mm";
            Utility.infoEN.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
            Utility.infoEN.DateTimeFormat.ShortDatePattern = "MM-dd-yyyy";

            Utility.pathMatlab = HttpContext.Current.Server.MapPath("~/MATLAB/");
            Utility.LogPath = HttpContext.Current.Server.MapPath("~/Logs/");

            Utility.DateSQLPattern = "yyyy/MM/dd";
            Utility.DateReportPattern = "yyyy-MM-dd";
            ControllerHelper.LoadPhrase("");
            ListLanguage = new List<Language>();
            ListLanguage.AddRange(ControllerHelper.LoadLanguage());
            #endregion

            #region Thread

            #endregion
        }

        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            //DevExpressHelper.Theme = "Aqua";

            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Utils.CurrentLanguage);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Utils.CurrentLanguage);

            Ini.fileName = HttpRuntime.AppDomainAppPath + Utils.IniName;
            Ini.Load(Ini.Instance, Ini.fileName);
            Ini.Save(Ini.Instance, Ini.fileName);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            DevExpress.Web.ASPxWebControl.SetIECompatibilityMode(8);
        }
    }

    #region CompressFilter
    public class CompressFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Mvc.ActionExecutingContext filterContext)
        {
            HttpRequestBase request = filterContext.HttpContext.Request;
            string acceptEncoding = request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(acceptEncoding))
                return;
            acceptEncoding = acceptEncoding.ToUpperInvariant();
            HttpResponseBase response = filterContext.HttpContext.Response;
            if (acceptEncoding.Contains("GZIP"))
            {
                response.AppendHeader("Content-encoding", "gzip");
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
            }
            else
                if (acceptEncoding.Contains("DEFLATE"))
                {
                    response.AppendHeader("Content-encoding", "deflate");
                    response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                }
        }
    }
    #endregion

    #region LogAndRedirectOnErrorAttribute
    public class LogAndRedirectOnErrorAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            //Do logging here
            Utility.LogEx("Exception", filterContext.Exception);

            //redirect to error handler
            filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(
            new { controller = "Home", action = "Index" }));

            // Stop any other exception handlers from running
            filterContext.ExceptionHandled = true;

            // CLear out anything already in the response
            filterContext.HttpContext.Response.Clear();
        }
    }
    #endregion

    #region ActionAuthorize
    [LogAndRedirectOnError]
    public class ActionAuthorize : AuthorizeAttribute
    {
        string featureName;
        bool isMenu = false;
        public ActionAuthorize(string feature, bool isMenu = false)
        {
            this.featureName = feature;
            this.isMenu = isMenu;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // Fire back an unauthorized response
                filterContext.HttpContext.Response.StatusCode = 403;
            }
            else
                base.HandleUnauthorizedRequest(filterContext);
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //Not yet login web and Logged on to Window
            if
            (HttpContext.Current.Request.LogonUserIdentity != null &&
                HttpContext.Current.Request.LogonUserIdentity.IsAuthenticated &&
                string.IsNullOrEmpty(SessionHelper.GetSession<string>("UserName"))
                )
            {
                string currentURL = HttpContext.Current.Request.Url.LocalPath;
                if (currentURL != "/")
                {
                    filterContext.Controller.TempData["ErrorPermission"] = Utility.Phrase("ErrorPermission_TimeOut");
                }
                GoToLogin(filterContext);
            }
            else
            {
                if (featureName != "DashBoard")
                {
                    CheckPermission(filterContext);
                }
            }
        }

        public void GoToLogin(AuthorizationContext filterContext)
        {
            string loginUrl = string.Format("{0}?ReturnUrl={1}"
                        , FormsAuthentication.LoginUrl
                        , HttpUtility.UrlEncode(filterContext.HttpContext.Request.Url.AbsoluteUri));
            filterContext.Result = new RedirectResult(loginUrl);
        }

        public void CheckPermission(AuthorizationContext filterContext)
        {
            if (featureName != string.Empty
                && !PermissionHelper.CheckPermissionByFeature(featureName, this.isMenu))
            {
                filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(
                new { controller = "Home", action = "ErrorPermission" }));
            }
        }
    }
    #endregion


}