using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using DevExpress.Data.PivotGrid;
using DevExpress.Utils;
using DevExpress.Web.Mvc;
using DevExpress.XtraPivotGrid;
using DMSERoute.Helpers;
using DMSERoute.Helpers.Html;
using eRoute.Filters;
using eRoute.Models;
using eRoute.Models.ViewModel;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using DevExpress.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using DevExpress.Web.ASPxEditors;
//using DevExpress.Web.ASPxEditors;


namespace eRoute.Controllers
{
    #region Controller
    [Authorize]
    [InitializeSimpleMembership]
    [LogAndRedirectOnError]
    public class IssuesController : Controller
    {
        string username = SessionHelper.GetSession<string>("UserName");

        #region ReportIssues
        [Authorize]
        [ActionAuthorize("Issues_ReportIssues", true)]
        public ActionResult ReportIssues(string strFromDate, string strToDate, string act, FormCollection formParam)//
        {

            //PermissionHelper.CheckPermissionByFeature("Issues_ReportIssues", this);
            ReportIssuesModel model = new ReportIssuesModel();

            #region Validate Data
            try
            {
                //Begin get
                //Set default value
                if (string.IsNullOrEmpty(strFromDate))
                {
                    model.areaID = string.Empty;
                    model.FromDate = DateTime.Today;
                    model.distributorID = 0;                  
                    model.regionID = string.Empty;
                    model.routeID = string.Empty;                 
                    model.ToDate = DateTime.Today;
                    model.status = 0;
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.strToDate = model.ToDate.ToShortPattern();                   
                }
                else
                {
                    model.areaID = Utility.StringParse(EditorExtension.GetValue<string>("AreaID"));// Utility.StringParse(areaID);
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.distributorID = Utility.IntParse(EditorExtension.GetValue<int>("DistributorID"));// Utility.IntParse(distributorID);                   
                    model.regionID = Utility.StringParse(EditorExtension.GetValue<string>("RegionID"));// Utility.StringParse(regionID);
                    model.routeID = Utility.StringParse(EditorExtension.GetValue<string>("RouteID"));// Utility.StringParse(routeID);
                   
                    model.ToDate = Utility.DateTimeParse(strToDate);
                    model.status = Utility.IntParse(EditorExtension.GetValue<int>("Status"));
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.strToDate = model.ToDate.ToShortPattern();  

                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            List<StatusIssues> listItem = new List<StatusIssues>();
            StatusIssues ins = new StatusIssues();

            ins.ID = 110;
            ins.Name = Utility.Phrase("Open");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 111;
            ins.Name = Utility.Phrase("ReOpen");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 112;
            ins.Name = Utility.Phrase("WAITING_CONFIRM");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 113;
            ins.Name = Utility.Phrase("Close");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 0;
            ins.Name = Utility.Phrase("All");
            listItem.Add(ins);
            model.ListStatus = listItem;
            #region LoadComboboxListItem
           
            model.ListRegion = ControllerHelper.GetListRegion(model.regionID); //model.regionID);
            if (string.IsNullOrEmpty(model.regionID) && model.ListRegion.Count == 1)
            {
                model.regionID = model.ListRegion.First().RegionID;
            }

            model.ListArea = ControllerHelper.GetListArea(model.regionID);
            if (string.IsNullOrEmpty(model.areaID) && model.ListArea.Count == 1)
            {
                model.areaID = model.ListArea.First().AreaID;
            }

            model.ListDistributor = ControllerHelper.GetListDistributorWithRegionArea(model.regionID, model.areaID);
            if (model.distributorID == 0 && model.ListDistributor.Count == 1)
            {
                model.distributorID = model.ListDistributor.First().DistributorID;
            }

            model.ListRoute = ControllerHelper.GetListRouteWithRegionAreaDis(model.regionID, model.areaID, model.distributorID);
            if (string.IsNullOrEmpty(model.routeID) && model.ListRoute.Count == 1)
            {
                model.routeID = model.ListRoute.First().RouteID;
            }
            #endregion

            if (model.ToDate < model.FromDate)
            {
                //model.listItem = new List<pp_ReportIssuesResult>();
                ViewBag.StatusMessage = "Ngày không hợp lệ. Vui lòng chọn ngày hợp lệ.";
                return View(model);
            }

            #region Set default select if drop have only one item
            #endregion

            if (
                !string.IsNullOrEmpty(strFromDate)
                )
            {
                model.listItem = Global.Context.pp_ReportIssues(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID, model.routeID, SessionHelper.GetSession<string>("UserName"), model.status).ToList();
               
                
            }
            else
            {
                //model.listItem = new DataTable();
                model.listItem = new List<pp_ReportIssuesResult>();
            }
            SessionHelper.SetSession<ReportIssuesModel>("ReportIssues", model);
            //SessionHelper.SetSession<ReportSMVisitSummaryVM>("ReportSMVisitSummary", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportIssuesExport");
            }
            return View(model);
        }
        public ActionResult ReportIssuesExport()
        {
            List<pp_ReportIssuesResult> model = new List<pp_ReportIssuesResult>();
            if (SessionHelper.GetSession<ReportIssuesModel>("ReportIssues") != null)
            {
                List<pp_ReportIssuesResult>  list = SessionHelper.GetSession<ReportIssuesModel>("ReportIssues").listItem;
                foreach (var line in list)
                {
                    line.Content = line.Content.Replace("&lt;br/&gt;", " ");
                    line.Resolve = line.Resolve.Replace("&lt;br/&gt;", " ");                  
                    model.Add(line);
                }
            }
            return GridViewExtension.ExportToXlsx(ReportExportOutletTrackingSettingsRAWData(), model);
        }

        private static GridViewSettings ReportExportOutletTrackingSettingsRAWData()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "dxGridView";
            settings.KeyFieldName = "IssueID";
            settings.CallbackRouteValues = new { Controller = "Issues", Action = "ReportIssuesExport" };
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.Columns.Add("RegionName").Caption = Utility.Phrase("RegionName");
            settings.Columns.Add("AreaName").Caption = Utility.Phrase("AreaName");
            settings.Columns.Add("DistributorCD").Caption = Utility.Phrase("DistributorCode");
            settings.Columns.Add("DistributorName").Caption = Utility.Phrase("DistributorName");
            settings.Columns.Add("RouteID").Caption = Utility.Phrase("RouteID");
            settings.Columns.Add("RouteName").Caption = Utility.Phrase("RouteName");
            settings.Columns.Add("SalesmanCode").Caption = Utility.Phrase("SalesmanID");
            settings.Columns.Add("SalemanName").Caption = Utility.Phrase("SalesmanName");

            settings.Columns.Add("VisitDate").Caption = Utility.Phrase("VisitDate");

            settings.Columns.Add("VisitID").Caption = Utility.Phrase("VisitID");
            settings.Columns.Add("OutletID").Caption = Utility.Phrase("OutletID");
            settings.Columns.Add("OutletName").Caption = Utility.Phrase("OutletName");

            settings.Columns.Add("IssueID").Caption = Utility.Phrase("IssueID");            

            settings.Columns.Add("IssueDate").Caption = Utility.Phrase("IssueDate");

            //settings.Columns.Add("Status").Caption = Utility.Phrase("Status");
            settings.Columns.Add(col =>
            {
                col.FieldName = "Status";
                col.ColumnType = MVCxGridViewColumnType.ComboBox;
                col.Caption = Utility.Phrase("Status");
                //col.Width = System.Web.UI.WebControls.Unit.Percentage(5);
                var cb = col.PropertiesEdit as ComboBoxProperties;
               
                cb.Items.Add(new ListEditItem()
                {
                    Index = 0,
                    Text = Utility.Phrase("Open"),
                    Value = 0
                });
                cb.Items.Add(new ListEditItem()
                {
                    Index = 1,
                    Text = Utility.Phrase("ReOpen"),
                    Value = 1
                });
                cb.Items.Add(new ListEditItem()
                {
                    Index = 2,
                    Text = Utility.Phrase("Released"),
                    Value = 2
                });
                cb.Items.Add(new ListEditItem()
                {
                    Index = 3,
                    Text = Utility.Phrase("Close"),
                    Value = 3
                });
                cb.Items.Add(new ListEditItem()
                {
                    Index = 4,
                    Text = Utility.Phrase("All"),
                    Value = 4
                });
            });
            settings.Columns.Add("Content").Caption = Utility.Phrase("Content");            
            
            settings.Columns.Add("Resolve").Caption = Utility.Phrase("Resolve");
            settings.Columns.Add("Pic").Caption = Utility.Phrase("UpdateBy");
            return settings;
        }
        public ActionResult ComboBoxPartialStatus()
        {
            List<StatusIssues> listItem = new List<StatusIssues>();
            StatusIssues ins = new StatusIssues();
            ins.ID = 110;
            ins.Name = Utility.Phrase("Open");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 111;
            ins.Name = Utility.Phrase("ReOpen");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 112;
            ins.Name = Utility.Phrase("WAITING_CONFIRM");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 113;
            ins.Name = Utility.Phrase("Close");
            listItem.Add(ins);
            return PartialView(listItem);
        }
        [Authorize]
        public PartialViewResult ReportIssuesPartial()
        {
            
            List<pp_ReportIssuesResult> model = SessionHelper.GetSession<ReportIssuesModel>("ReportIssues").listItem;
            return PartialView("ReportIssuesPartial", model);            
        }
        //[AllowHtml]
        [HttpPost, ValidateInput(false)]
        public ActionResult AddDes()
        {
            string content = Request.Params["content"];           
            string issuesID = Request.Params["issuesID"];            
            // Insert E_Issues 1 new row
            using (ERouteDataContext db = new ERouteDataContext())
            {
                E_Issue InsertLine = (
                from sh in db.E_Issues
                where sh.IssueID == issuesID
                orderby sh.Date descending
                select sh).FirstOrDefault<E_Issue>();
                if (InsertLine != null)
                {
                    E_Issue ins = new E_Issue();
                    ins.IssueID = InsertLine.IssueID;
                    ins.Content = InsertLine.Content;
                    ins.Date = InsertLine.Date;
                    ins.DistributorCode = InsertLine.DistributorCode;
                    ins.OutletID = InsertLine.OutletID;
                    ins.Pic = InsertLine.Pic;
                    ins.Resolve = content;
                    ins.SalesmanCode = InsertLine.SalesmanCode;
                    ins.Status = 111;
                    ins.UpdateDate = DateTime.Now;
                    ins.VisitDate = InsertLine.VisitDate;
                    ins.Pic = User.Identity.Name;
                    db.E_Issues.InsertOnSubmit(ins);                    
                    db.SubmitChanges();
                }
                var listrow = (
                from sh in db.E_Issues
                where sh.IssueID == issuesID
                select sh).ToList<E_Issue>();
                foreach (var item in listrow)
                {
                    item.Status = 111;
                    db.SubmitChanges();
                }
                // Cap nhat trang thai E_IssueConfirmation = 2
                var listrowConfirmation = (
                    from sh in db.E_IssueConfirmations
                    where sh.IssueID == issuesID
                    select sh).ToList<E_IssueConfirmation>();
                foreach (var item in listrowConfirmation)
                {
                    item.NewStatus = 121;
                    db.SubmitChanges();
                }
                //end
            }
            //end   
           
            ReportIssuesModel model = SessionHelper.GetSession<ReportIssuesModel>("ReportIssues");
            if (model.status != 0)
            {
                //model.listItem.Rows
                model.listItem.RemoveAll(rm => rm.IssueID == issuesID);
            }
            else
            {
                var item =  model.listItem.Find(find => find.IssueID == issuesID);
                item.Status = 111;
                string strdate = String.Format("{0:dd/MM/yyyy hh:mm:ss}", DateTime.Now.Date);
                item.Resolve = strdate + "&lt;br/&gt;" + content + "&lt;br/&gt;" + item.Resolve;
                model.listItem.RemoveAll(rm => rm.IssueID == issuesID);
                model.listItem.Add(item);
            }
            SessionHelper.SetSession<ReportIssuesModel>("ReportIssues",model);
            return Json("");
        }
        public ActionResult LoadPopUp()
        {
            // Intentionally pauses server-side processing,
            // to demonstrate the Loading Panel functionality.
            string issuesID = Request.Params["issuesID"];
            List<pp_ReportIssuesResult> model = new List<pp_ReportIssuesResult>();
            if (SessionHelper.GetSession<ReportIssuesModel>("ReportIssues") != null)
            {
                model = SessionHelper.GetSession<ReportIssuesModel>("ReportIssues").listItem;
            }
            else
            {
                model = new List<pp_ReportIssuesResult>();
                return Json(new { Content = "", Resolve = "" });
            }
            pp_ReportIssuesResult find = model.Find(f => f.IssueID == issuesID);
            ///find.Content = Html.Raw(HttpUtility.HtmlEncode(find.Content).Replace("\n", "<br/>"))

            if (find != null)
            {
                string visitDate;
                string issueDate;
                string status;
                if(find.Status == 0)
                {
                    status = Utility.Phrase("Open");
                }else
                if (find.Status == 1)
                {
                    status = Utility.Phrase("ReOpen");
                }else
                if (find.Status == 2)
                {
                    status = Utility.Phrase("Released");
                }else                
                {
                    status = Utility.Phrase("Close");
                } 

                var culture = System.Globalization.CultureInfo.CurrentCulture;
                if (culture.Name == "vi-VN")
                {

                    visitDate = String.Format("{0:dd/MM/yyyy}", find.VisitDate.ToShortDatePattern());
                    issueDate = String.Format("{0:dd/MM/yyyy}", find.IssueDate.ToShortDatePattern()); 

                }
                else {
                    visitDate = String.Format("{0}", find.VisitDate.Value.Date.ToString("MM/dd/yyyy"));
                    issueDate = String.Format("{0}", find.IssueDate.Value.Date.ToString("MM/dd/yyyy")); 
                }
                return Json(new { 
                    RegionName = find.RegionName,
                    AreaName = find.AreaName,
                    DistributorCode = find.DistributorCD,
                    DistributorName = find.DistributorName,
                    RouteID = find.RouteID,
                    RouteName = find.RouteName,
                    SalesmanID = find.SalesmanCode,
                    SalesmanName = find.SalemanName,                    
                    VisitDate = visitDate,
                    VisitID = find.VisitID,
                    OutletID = find.OutletID,
                    OutletName =find.OutletName,
                    IssueDate = issueDate,
                    Status = status,
                    Content = HttpUtility.HtmlDecode(find.Content), Resolve = HttpUtility.HtmlDecode(find.Resolve),
                    //Them danh sach Image vao pop
                    ListImage = find.Image
                });
            }
            else
            {
                return Json(new { Content = "", Resolve = "" });
            }
        }
        #endregion
    }
   

    #endregion
}