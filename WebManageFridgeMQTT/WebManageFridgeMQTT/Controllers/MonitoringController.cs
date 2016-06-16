using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
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
            DeviceInfoMV model = new DeviceInfoMV();
            try
            {
                model.TreeDevice = Global.Context.GetTreeThietBi("").ToList();
                model.ListDeviceInfo = Global.Context.Sp_GetInfoDevice("").ToList();
                model.ListCongTrinh = Global.Context.GetInfoCongTrinh("").ToList();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }

            return View(model);
        }

        public ActionResult PopupReport()
        {
            DeviceActivity model = new DeviceActivity();
            return PartialView("PopupReport");
        }
        public ActionResult GetThietBi(string equipmentId, bool isParent)
        {
            var model = Global.Context.GetTreeThiet_ById(equipmentId, isParent).ToList();
            return Json(new { info = model }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TreeViewSelected(string strSearch)
        {
            var model = Global.Context.GetTreeThietBi("").ToList();
            var temp = new List<GetTreeThietBiResult>();
            if (string.IsNullOrWhiteSpace(strSearch))
            {
                return PartialView(model);
            }
            else
            {
             var  ChildModel = model.Where(x=> x.Cap == 1 && x.Name.Contains(strSearch.Trim())).ToList();
               var mergeModel = ChildModel;
               foreach (var level1 in ChildModel)
               {
                   var ParentModel = from p in model where p.Id == level1.Father select p;
                   mergeModel = mergeModel.Union(ParentModel).ToList();
               }
               return PartialView(mergeModel);
            }
        }
        public static void CreateTreeViewLeftPanel(List<GetTreeThietBiResult> listNote, MVCxTreeViewNodeCollection nodesCollection, string parentID)
        {
            var lstTemp = listNote.Where(x => x.Father == parentID).OrderBy(x => x.Name);

            foreach (var row in lstTemp)
            {
                string name = row.Name;
                string id = row.Id.ToString();
                if (id != null && name != null)
                {
                    MVCxTreeViewNode node = nodesCollection.Add(name, id);
                    CreateTreeViewLeftPanel(listNote, node.Nodes, id);
                }
            }
        }
        public ActionResult GetInfoDeviceById(string id)
        {
            var model = Global.Context.Sp_GetInfoDeviceById(id).FirstOrDefault();
            return Json(model);
        }
        public ActionResult Device(FormCollection formParam,  string strSearch)
        {
            
            DeviceInfoMV model = new DeviceInfoMV();
            try
            {
                model.ListDeviceInfo = Global.Context.Sp_GetInfoDevice("").ToList();
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
        public ActionResult DeviceActivity(string thietBiID)
        {
            ActivityChip model = new ActivityChip();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.infoDevice = Global.Context.Sp_GetInfoDeviceById(thietBiID).FirstOrDefault();
                DateTime FromDate = DateTime.Now.AddMonths(-1);
                DateTime ToDate = DateTime.Now;
                model.ListData = Global.Context.GetInfoDeviceActivity(thietBiID, FromDate, ToDate).OrderByDescending(x => x.ThoiGian).Take(5).ToList();
            }
            return PartialView("DeviceActivity", model);
        }
        public ActionResult PopupActivity(string thietBiID, string strFromDate, string strToDate)
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

            //TEST
            model.FromDate = DateTime.Now.AddMonths(-1);
            model.ToDate = DateTime.Now;
            model.ListData = Global.Context.GetInfoDeviceActivity(model.ThietBiID, model.FromDate, model.ToDate).OrderByDescending(x => x.ThoiGian).ToList();
            return PartialView("PopupActivity", model);
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
            model.ListDataMove = Global.Context.GetInfoDeviceMove(thietBiID, model.FromDate, model.ToDate).ToList();
            return PartialView("DeviceMove", model);
        }
        public ActionResult DeviceReport(string thietBiID, string strFromDate, string strToDate)
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
            model.ListDataReport = Global.Context.GetInfoDeviceReport(thietBiID, model.FromDate, model.ToDate).ToList();
            return PartialView("DeviceReport", model);
        }

        public ActionResult GetDeviceByCongTrinh(string congTrinhId)
        {
            List<GetTreeThietBiResult> model = new List<GetTreeThietBiResult>();
            model = Global.Context.GetTreeThietBi(congTrinhId).ToList();
            ViewData["ParentID"] = 0;
            return PartialView("TreeViewDevice", model);
        }
    }
}
