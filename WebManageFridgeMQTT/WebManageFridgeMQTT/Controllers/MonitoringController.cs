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
            return View();
        }
        //public ActionResult GetTree()
        //{
        //    var mod = Global.Context.GetTreeThietBi().Take(50).ToList();
        //    TreeView myTreeView = new TreeView();
        //    myTreeView.Nodes.Clear();
        //    foreach (var item in mod)
        //    {
        //        TreeNode parent = new TreeNode("dd");
        //        if (item.Cap == 0)
        //        {
        //            parent.Text = item.Name;
        //            parent.Value = item.Id;
        //            myTreeView.Nodes.Add(parent);
        //        }
        //        foreach (var child in mod)
        //        {
        //            TreeNode tr = new TreeNode("as");
        //            if (child.Father == item.Id)
        //            {
        //                tr.Text = child.Name;
        //                tr.Value = child.Id;
        //                parent.ChildNodes.Add(tr);
        //            }
        //        }
        //    }
        //    return Json(new { data = myTreeView, JsonRequestBehavior.AllowGet });
        //}
        public ActionResult GetThietBi(string equipmentId, bool isParent)
        {
            var model = Global.Context.GetTreeThiet_ById(equipmentId, isParent).ToList();
            return Json(new { info = model }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TreeViewSelected(string strSearch)
        {
            var model = Global.Context.GetTreeThietBi().ToList();
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
        //public static void CreateTreeView(TreeNodeCollection parentNode, string parentID, List<GetTreeThietBiResult> model)
        //{


        //    foreach (var dta in model)
        //    {
        //        if (dta.capToString() == parentID)
        //        {
        //            String key = dta.Id.ToString();
        //            String text = dta.Name.ToString();
        //            TreeNode child = new TreeNode();
        //            child.Value = key;
        //            child.Text = text;
        //            TreeNodeCollection newParentNode = new TreeNodeCollection(child);
        //            CreateTreeView(newParentNode, dta.Id.ToString(), model);
        //        }
        //    }
        //}
        public ActionResult Device(FormCollection formParam,  string strSearch)
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
