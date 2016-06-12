using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace WebManageFridgeMQTT.Models
{
    public class Constant
    {
        public static int StoreTimeOut = Convert.ToInt32(ConfigurationSettings.AppSettings["StoreTimeOut"]);
    }

    public class Model
    {
    }
    public class DeviceInfoMV
    {
        public List<GetTreeThietBiResult> TreeDevice { get; set; }
        public List<Sp_GetInfoDeviceResult> ListDeviceInfo { get; set; }
        public List<Sp_GetInfoDeviceByIdResult> ListDeviceInfoId { get; set; }
    }
}