// Developer Express Code Central Example:
// How to change a theme on-the-fly
// 
// This sample illustrates the possibility to change themes on-the-fly. The current
// theme is based on the Cookies value that is set in the client-side
// RadioButtonList.SelectedIndexChanged event handler. See also:
// ASP:
// http://www.devexpress.com/scid=E1342
// MVC: Applying Themes
// (ms-help://DevExpress.NETv11.2/DevExpress.AspNet/CustomDocument9143.htm)
// http://www.devexpress.com/scid=E2871
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E3825

using System;
using System.Collections.Generic;
using System.Web;
using System.IO;

using eRoute;

public static class Utils {
    public const string
        CurrentThemeCookieKey = "theme",
        DefaultTheme = "Aqua",
        CurrentLanguageCookieKey = "Language",
        DefaultLanguage = "VI",
        IniName = "DevExTest.INI",
        LogName = "DevExTest.log";

    static HttpContext Context {
        get { return HttpContext.Current; }
    }

    static HttpRequest Request {
        get { return Context.Request; }
    }

    public static string CurrentTheme
    {
        get
        {
            String theme = DefaultTheme;
            if (Request.Cookies[CurrentThemeCookieKey] != null)
                theme = HttpUtility.UrlDecode(Request.Cookies[CurrentThemeCookieKey].Value);
            if (!Enum.IsDefined(typeof(CommonThemes), theme)) theme = DefaultTheme;
            return theme;
        }
    }

    public static string CurrentLanguage 
    {
        get
        {
            String language = DefaultLanguage;
            if (Request.Cookies[CurrentLanguageCookieKey] != null)
                language = HttpUtility.UrlDecode(Request.Cookies[CurrentLanguageCookieKey].Value);
            if (!Enum.IsDefined(typeof(CommonLanguages), language)) language = DefaultLanguage;
            return language;
        }
    }

    public class UserDataPath
    {
        static public string Get(string software)
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            dir = System.IO.Path.Combine(dir, software);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
