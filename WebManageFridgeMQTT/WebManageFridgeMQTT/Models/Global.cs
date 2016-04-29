using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WebManageFridgeMQTT.Models
{
    public sealed class Global
    {
        public static DeviceTrackingDataContext Context
        {
            get
            {
                string ocKey = "key_" + HttpContext.Current.GetHashCode().ToString("x");
                if (!HttpContext.Current.Items.Contains(ocKey))
                {
                    var a = new DeviceTrackingDataContext();
                    a.CommandTimeout = Constant.StoreTimeOut;
                    HttpContext.Current.Items.Add(ocKey, a);
                }
                return HttpContext.Current.Items[ocKey] as DeviceTrackingDataContext;
            }
        }
    }
}