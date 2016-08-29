using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eRoute.Models
{
    public sealed class Global
    {
        public static ERouteDataContext Context
        {
            get
            {
                string ocKey = "key_" + HttpContext.Current.GetHashCode().ToString("x");
                if (!HttpContext.Current.Items.Contains(ocKey))
                {
                    var a = new ERouteDataContext();
                    a.CommandTimeout = Constant.StoreTimeOut;
                    HttpContext.Current.Items.Add(ocKey, a);
                }
                return HttpContext.Current.Items[ocKey] as ERouteDataContext;
            }
        }
        //public static NGVisibilityDataContext ContextVisibility = new NGVisibilityDataContext();
        public static NGVisibilityDataContext VisibilityContext
        {
            get
            {
                string ocKey = "VSkey_" + HttpContext.Current.GetHashCode().ToString("x");
                if (!HttpContext.Current.Items.Contains(ocKey))
                {
                    var a = new NGVisibilityDataContext();
                    a.CommandTimeout = Constant.StoreTimeOut;
                    HttpContext.Current.Items.Add(ocKey, a);
                }
                return HttpContext.Current.Items[ocKey] as NGVisibilityDataContext;
            }
        }
    }
}