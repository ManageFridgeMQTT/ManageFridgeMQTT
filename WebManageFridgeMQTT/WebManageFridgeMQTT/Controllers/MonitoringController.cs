using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebManageFridgeMQTT.Models;
using WebManageFridgeMQTT.Utility;

namespace WebManageFridgeMQTT.Controllers
{
    public class MonitoringController : Controller
    {
        //
        // GET: /Monitoring/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Device()
        {
            DeviceInfoMV model = new DeviceInfoMV();
            try
            {
                model.ListDeviceInfo = Global.Context.Sp_GetInfoDevice().ToList();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            
            return View(model.ListDeviceInfo);
        }

        public ActionResult DeviceModify(string thietBiID, string strFromDate, string strToDate)
        {
            DeviceActivity model = new DeviceActivity();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.ThietBiID = thietBiID;
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model.ListDataModify = Global.Context.GetInfoDeviceModify(model.ThietBiID, model.FromDate, model.ToDate).ToList();
            return PartialView("DeviceModify", model);
        }
        public ActionResult DeviceActivity(string thietBiID, string strFromDate, string strToDate)
        {
            DeviceActivity model = new DeviceActivity();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.ThietBiID = thietBiID;
            }
            if(!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model.ListData = Global.Context.GetInfoDeviceActivity(model.ThietBiID, model.FromDate, model.ToDate).ToList();
            return PartialView("DeviceActivity", model);
        }
        public ActionResult DeviceMove(string thietBiID, string strFromDate, string strToDate)
        {
            DeviceActivity model = new DeviceActivity();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.ThietBiID = thietBiID;
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model.ListDataMove = Global.Context.GetInfoDeviceMove(thietBiID, FromDate, ToDate).ToList();
            return PartialView("DeviceMove", model);
        }

    }
}
