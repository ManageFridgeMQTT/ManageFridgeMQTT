using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebManageFridgeMQTT.Models;

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
            model.ListDeviceInfo = Global.Context.Sp_GetInfoDevice().ToList();
            return View(model.ListDeviceInfo);
        }

        public ActionResult DeviceModify(string thietBiID, string strFromDate, string strToDate)
        {
            List<GetInfoDeviceModifyResult> model = new List<GetInfoDeviceModifyResult>();
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model = Global.Context.GetInfoDeviceModify(thietBiID, FromDate, ToDate).ToList();
            return PartialView("DeviceModify", model);
        }
        public ActionResult DeviceActivity(string thietBiID, string strFromDate, string strToDate)
        {
            List<GetInfoDeviceActivityResult> model = new List<GetInfoDeviceActivityResult>();
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model = Global.Context.GetInfoDeviceActivity(thietBiID, FromDate, ToDate).ToList();
            return PartialView("DeviceActivity", model);
        }
        public ActionResult DeviceMove(string thietBiID, string strFromDate, string strToDate)
        {
            List<GetInfoDeviceMoveResult> model = new List<GetInfoDeviceMoveResult>();
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model = Global.Context.GetInfoDeviceMove(thietBiID, FromDate, ToDate).ToList();
            return PartialView("DeviceMove", model);
        }
        
    }
}
