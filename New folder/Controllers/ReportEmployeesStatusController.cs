using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammer.Models;
//using Utility.Phrase("PrepareSchedule");
using Hammer.Helpers;
using eRoute.Filters;
using DevExpress.Web.Mvc;
using System.Threading;
using WebMatrix.WebData;
using eRoute.Models.eCalendar;
using eRoute;
using DMSERoute.Helpers;

namespace Hammer.Controllers
{
    [InitializeSimpleMembership]
    [Authorize()]
    public class ReportEmployeesStatusController : Controller
    {
        //
        // GET: /ReportEmployeesStatus/
        [Authorize]
        [ActionAuthorize("eCalendar_EmployeesStatusTracking", true)]
        public ActionResult Index(string RegionID, string AreaID, string EmployeeID, string FromDate, string EndDate)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            ReportEmployeesStatusFilterModel model = new ReportEmployeesStatusFilterModel();

            Session["DataReportEmployeeStatus"] = new List<ReportEmployeesStatusModel>();
            if (EmployeeID != null)
            {
                model.regionID = Utility.StringParse(EditorExtension.GetValue<string>("RegionID"));
                model.areaID = Utility.StringParse(EditorExtension.GetValue<string>("AreaID"));
                model.FromDate = Utility.DateTimeParse(FromDate);
                model.EndDate = Utility.DateTimeParse(EndDate);
                model.ListRegion = HammerDataProvider.GetRegionEmployees(User.Identity.Name);
                model.ListArea = HammerDataProvider.GetAreasWithRegion("");
                model.EmployeeID = Utility.StringParse(EditorExtension.GetValue<string>("EmployeeID"));
                List<ReportEmployeesStatusModel> reverseList = HammerDataProvider.GetdataReportEmployeesStatus(User.Identity.Name, model.FromDate, model.EndDate, model.regionID, model.areaID, model.EmployeeID);
                Session["DataReportEmployeeStatus"] = reverseList;
            }
            else
            {
                model.FromDate = DateTime.Now.Date;
                model.EndDate = DateTime.Now.Date;
                model.ListRegion = HammerDataProvider.GetRegionEmployees(User.Identity.Name);
                model.ListArea = HammerDataProvider.GetAreasWithRegion("");
                bool rs = HammerDataProvider.CheckPermissionRoles(User.Identity.Name);
                eRoute.Models.eCalendar.DMSSFHierarchy query = eRoute.Models.eCalendar.HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(User.Identity.Name);
                if (rs == true)
                {
                    if (query.IsSalesForce == true && query.TerritoryType == 'D')
                    {
                        DMSSFAssignment ass =  eRoute.Models.eCalendar.HammerDataProvider.GetAssSFIsBase(User.Identity.Name);
                        model.regionID = ass.RegionID;
                        model.areaID = ass.AreaID;
                        model.EmployeeID = ass.EmployeeID;
                        //model.ListRegion = HammerDataProvider.GetRegionEmployees(User.Identity.Name);
                        //if (model.ListRegion.Count > 0)
                        //{
                        //    model.regionID = model.ListRegion[0].RegionID;
                        //    model.ListArea = HammerDataProvider.GetAreasWithRegion(model.ListRegion[0].RegionID);
                        //    if (model.ListArea.Count > 0)
                        //    {
                        //        model.areaID = model.ListArea[0].AreaID;
                        //        model.EmployeeID = User.Identity.Name;
                        //    }
                        //}

                    }
                }
                Session["DataReportEmployeeStatus"] = new List<ReportEmployeesStatusModel>();
            }
            return View(model);
            //return View(new ReportEmployeesStatusFilterModel()
            //{                                
            //    FromDate = DateTime.Now.Date,
            //    EndDate = DateTime.Now.Date,
            //    ListRegion = HammerDataProvider.GetRegionEmployees(User.Identity.Name),
            //    ListArea = HammerDataProvider.GetAreasWithRegion("")
            //});
        }
        public ActionResult ComboBoxPartialRegion()
        {
            return PartialView(HammerDataProvider.GetRegion());
        }
        public ActionResult ComboBoxPartialArea()
        {
            string regionID = Request.Params["RegionID"].ToString();
            List<Area> listItem = HammerDataProvider.GetAreasWithRegion(regionID);
            return PartialView(listItem);
        }
        public ActionResult ComboBoxPartialEm()
        {
            string regionID = Request.Params["RegionID"].ToString();
            string areaID = Request.Params["AreaID"].ToString();
            if (areaID == "null")
                areaID = null;
            System.Collections.Generic.List<EmployeeModel> list2 = HammerDataProvider.GetSubordinateNoDuplicate(User.Identity.Name).Where(x => x.Level != "SM").ToList();
            List<EmployeeModel> listItem = HammerDataProvider.GetEmployeesRegionAreaPhanCap(regionID, areaID, list2);

            return PartialView(listItem);
        }
        public ActionResult DetailView()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView(Session["DataReportEmployeeStatus"]);
        }
        public ActionResult ExportExcelPartialView(string scheduleType)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView();
        }

        public FileResult ExportExcel()
        {
            string templatePath = Server.MapPath("/Templates/Report/") + "ReportStatusImportSchedules.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string filename = DateTime.Now.ToString("yyyyMMddhhmm") + "_eCalendar_TrackingStatus.xlsx";
            List<ReportEmployeesStatusModel> list = Session["DataReportEmployeeStatus"] as List<ReportEmployeesStatusModel>;
            list = list.OrderBy(x => x.Date).ThenBy(z => z.EmployeeID).ToList();
            Byte[] fileBytes = Util.ExportExcelReportEmployeeStatus(list, templatePath);
            FileResult result = File(fileBytes, contentType, filename);
            return result;
        }

        [HttpPost]
        public ActionResult ProcessSchedule(ReportEmployeesStatusFilterModel model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            List<ReportEmployeesStatusModel> reverseList = HammerDataProvider.GetdataReportEmployeesStatus(User.Identity.Name, model.FromDate, model.EndDate, model.regionID, model.areaID, model.EmployeeID);
            Session["DataReportEmployeeStatus"] = reverseList;
            return PartialView("DetailView", Session["DataReportEmployeeStatus"]);
        }
    }
}
