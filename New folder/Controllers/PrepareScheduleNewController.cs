using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eRoute.Models.eCalendar;
////using Utility.Phrase("PrepareScheduleNew");
//using Hammer.Helpers;
//using log4net;
//using eRoute.Filters;
using WebMatrix.WebData;
using System.Globalization;
using eRoute.Filters;
using eRoute.Models.eCalendar;
using eRoute.Models;
using Hammer.Models;
using Hammer.Helpers;
using DMSERoute.Helpers;
using DevExpress.Web.Mvc;



namespace eRoute.Controllers
{
    [InitializeSimpleMembership]
    [Authorize()]
    public class PrepareScheduleNewController : Controller
    {

        //
        // GET: /PrepareSchedule/
        //private static readonly ILog Log = LogManager.GetLogger(typeof(PrepareScheduleNewController));
        [Authorize]      
        [ActionAuthorize("eCalendar_PrepareSchedule", true)]
        public ActionResult Index(string EmployeeID,string Year,string Week)
        {
            PrepareScheduleNewModel model = new PrepareScheduleNewModel();

            model.EmployeeID = Utility.StringParse(EditorExtension.GetValue<string>("EmployeeID")); 
            model.Year = Year;
            model.Week = Convert.ToInt32(Week);
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            bool kq = HammerDataProvider.CheckPermissionRoles(User.Identity.Name);
            eRoute.Models.eCalendar.DMSSFHierarchy  query = eRoute.Models.eCalendar.HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(User.Identity.Name);
            if (kq == true)
            {
                if (query.IsSalesForce == true && query.TerritoryType == 'D')
                {
                    return RedirectToAction("index", "ErrorPermission");
                }
            }
            //else
            //{
            //    return RedirectToAction("index", "ErrorPermission");

            //}
            Session["GirdWW"] = null;
            Session["GirdNoWW"] = null;
            SendApp = String.Empty;
            if (model.EmployeeID != null && model.Year != null && model.Week != null)
                this.ProcessData(model);
            return View(model);
            //return View(new PrepareScheduleNewModel()
            //{
            //});
        }
        public string ProcessData(PrepareScheduleNewModel model)
        {
            #region AddWW
            List<PrepareWW> ListWW = new List<PrepareWW>();
            if (!string.IsNullOrEmpty(model.Year) && model.Week != null && !string.IsNullOrEmpty(model.EmployeeID))
            {
                //Check đã tồn tại trong lưu trưc.                
                List<EmployeeModel> ListEmpWW = HammerDataProvider.GetSubordinateNoDuplicate(model.EmployeeID);
                foreach (EmployeeModel item in ListEmpWW)
                {
                    //check
                    PrepareWW linedata = new PrepareWW();
                    if (HammerDataProvider.EmployeeInRole(item.EmployeeID) == SystemRole.Salesman)
                    {
                        ViewBag.Status = true;
                        linedata = HammerDataProvider.GetInPreapreWW(model.Year, model.Week.Value, model.EmployeeID, item.RouteID);
                    }
                    else
                    {
                        ViewBag.Status = false;
                        linedata = HammerDataProvider.GetInPreapreWW(model.Year, model.Week.Value, model.EmployeeID, item.EmployeeID);
                    }
                    if (linedata != null)
                    {
                        #region GetNameInfor SF
                        SystemRole role = HammerDataProvider.EmployeeInRole(item.EmployeeID);
                        //Get MCP if @employeeID is salesman                       
                        if (role == SystemRole.SalesForce)
                        {
                            eRoute.Models.eCalendar.DMSSFHierarchy query = eRoute.Models.eCalendar.HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(item.EmployeeID);
                            if (query != null)
                            {
                                if (query.TerritoryType == 'D' && query.IsSalesForce == true)
                                {
                                    eRoute.Models.eCalendar.DMSSFAssignment sfass = HammerDataProvider.GetAssSFIsBase(linedata.EmployeeWW);
                                    if (sfass != null)
                                    {
                                        linedata.RegionID = sfass.RegionID;
                                        if (HammerDataProvider.GetNameRegion(sfass.RegionID) != null)
                                            linedata.RegionName = HammerDataProvider.GetNameRegion(sfass.RegionID).RegionName;
                                        linedata.AreaID = sfass.AreaID;
                                        if (HammerDataProvider.GetNameArea(sfass.AreaID) != null)
                                            linedata.AreaName = HammerDataProvider.GetNameArea(sfass.AreaID).AreaName;

                                        eRoute.Models.eCalendar.Distributor dis = HammerDataProvider.GetDistributorsCD(sfass.DistributorID.Value);
                                        if (dis != null)
                                        {
                                            linedata.DistributorCD = dis.CompanyCD;
                                            linedata.DistributorName = dis.CompanyName;
                                            eRoute.Models.eCalendar.DMSProvince pr = HammerDataProvider.GetProvinceName(dis.ProvinceID);
                                            if (pr != null)
                                            {
                                                linedata.ProviceID = dis.ProvinceID;
                                                linedata.ProviceName = pr.Province;
                                            }
                                        }
                                    }
                                    DMSHamResultsSS Route = HammerDataProvider.getResultSS(linedata.Year, linedata.Week, linedata.EmployeeWW, linedata.EmployeeID);
                                    if (Route != null)
                                        linedata.SortID = Route.SortID;

                                }
                                else if (query.TerritoryType == 'A' && query.IsSalesForce == true)
                                {
                                    eRoute.Models.eCalendar.DMSSFAssignment sfass = HammerDataProvider.GetAssSFIsBase(linedata.EmployeeWW);
                                    if (sfass != null)
                                    {
                                        linedata.RegionID = sfass.RegionID;
                                        linedata.AreaID = sfass.AreaID;
                                        if (HammerDataProvider.GetNameRegion(sfass.RegionID) != null)
                                            linedata.RegionName = HammerDataProvider.GetNameRegion(sfass.RegionID).RegionName;
                                        if (HammerDataProvider.GetNameArea(sfass.AreaID) != null)
                                            linedata.AreaName = HammerDataProvider.GetNameArea(sfass.AreaID).AreaName;
                                    }
                                    DMSHamResultsASM Route = HammerDataProvider.getResultASM(linedata.Year, linedata.Week, linedata.EmployeeWW, linedata.EmployeeID);
                                    if (Route != null)
                                        linedata.SortID = Route.SortID;
                                }
                            }
                        }
                        #endregion
                        #region GetNameInfo SM
                        else
                        {
                            linedata.EmployeeWW = item.EmployeeID;
                            Salesperson sp = HammerDataProvider.GetSMName(linedata.EmployeeWW);
                            if (sp != null)
                            {
                                linedata.EmployeeWWName = sp.Descr;
                            }
                            DMSDistributorRouteAssignment route = HammerDataProvider.GetRouteWithSMandSS(linedata.EmployeeID, linedata.EmployeeWW);
                            if (route != null)
                            {
                                eRoute.Models.eCalendar.Distributor dis = HammerDataProvider.GetDistributorsCD(route.CompanyID);
                                if (dis != null)
                                {
                                    linedata.DistributorCD = dis.CompanyCD;
                                    linedata.DistributorName = dis.CompanyName;
                                    eRoute.Models.eCalendar.DMSProvince pr = HammerDataProvider.GetProvinceName(dis.ProvinceID);
                                    if (pr != null)
                                    {
                                        linedata.ProviceID = dis.ProvinceID;
                                        linedata.ProviceName = pr.Province;
                                    }
                                    linedata.RegionID = dis.RegionID;
                                    linedata.AreaID = dis.AreaID;
                                    if (HammerDataProvider.GetNameRegion(dis.RegionID) != null)
                                        linedata.RegionName = HammerDataProvider.GetNameRegion(dis.RegionID).RegionName;
                                    if (HammerDataProvider.GetNameArea(dis.AreaID) != null)
                                        linedata.AreaName = HammerDataProvider.GetNameArea(dis.AreaID).AreaName;
                                }
                            }
                            DMSHamResult Route = HammerDataProvider.getResultRoute(linedata.Year, linedata.Week, linedata.EmployeeID, linedata.RouteID);
                            if (Route != null)
                                linedata.SortID = Route.SortID;
                        }
                        #endregion
                        //linedata.Status = 'N';                       
                        ListWW.Add(linedata);
                    }
                    else
                    {
                        PrepareWW ins = new PrepareWW();
                        ins.EmployeeWW = item.EmployeeID;
                        ins.EmployeeID = model.EmployeeID;
                        ins.Year = model.Year;
                        ins.Week = model.Week.Value;
                        ins.UserLogin = User.Identity.Name;
                        ins.CreatedDateTime = DateTime.Now;
                        ins.Status = 'N';
                        SystemRole role = HammerDataProvider.EmployeeInRole(item.EmployeeID);
                        //Get MCP if @employeeID is salesman
                        #region SM
                        if (role == SystemRole.Salesman)
                        {
                            DMSDistributorRouteAssignment route = HammerDataProvider.GetRouteWithSMandSS(ins.EmployeeID, ins.EmployeeWW);
                            if (route != null)
                            {
                                ins.RouteID = route.RouteCD;
                                eRoute.Models.eCalendar.Distributor dis = HammerDataProvider.GetDistributorsCD(route.CompanyID);
                                if (dis != null)
                                {
                                    ins.DistributorCD = dis.CompanyCD;
                                    ins.DistributorName = dis.CompanyName;
                                    eRoute.Models.eCalendar.DMSProvince pr = HammerDataProvider.GetProvinceName(dis.ProvinceID);
                                    if (pr != null)
                                    {
                                        ins.ProviceID = dis.ProvinceID;
                                        ins.ProviceName = pr.Province;
                                    }
                                    ins.RegionID = dis.RegionID;
                                    ins.AreaID = dis.AreaID;
                                    if (HammerDataProvider.GetNameRegion(dis.RegionID) != null)
                                        ins.RegionName = HammerDataProvider.GetNameRegion(dis.RegionID).RegionName;
                                    if (HammerDataProvider.GetNameArea(dis.AreaID) != null)
                                        ins.AreaName = HammerDataProvider.GetNameArea(dis.AreaID).AreaName;
                                }
                            }
                            else
                            {
                                ins.RouteID = string.Empty;
                            }
                            Salesperson sp = HammerDataProvider.GetSMName(ins.EmployeeWW);
                            if (sp != null)
                            {
                                ins.EmployeeWWName = sp.Descr;
                                char rs = HammerDataProvider.getResult(model.Year, model.Week.Value, item.EmployeeID, model.EmployeeID, sp.UsrInitDate);
                                if (rs != '0')
                                {
                                    ins.RefResult = rs;
                                    DMSHamResult Route = HammerDataProvider.getResultRoute(model.Year, model.Week.Value, model.EmployeeID, ins.RouteID);
                                    if (Route != null)
                                        ins.SortID = Route.SortID;
                                    ListWW.Add(ins);
                                }
                            }
                            else
                            {
                                ins.RefResult = '0';
                                ListWW.Add(ins);
                            }

                        }
                        #endregion
                        #region SS
                        else
                        {
                            ins.RouteID = string.Empty;
                            ins.EmployeeWWName = HammerDataProvider.GetNameEmployee(ins.EmployeeWW);

                            eRoute.Models.eCalendar.DMSSFHierarchy query = eRoute.Models.eCalendar.HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(item.EmployeeID);
                            if (query != null)
                            {
                                if (query.TerritoryType == 'D' && query.IsSalesForce == true)
                                {
                                    eRoute.Models.eCalendar.DMSSFAssignment sfass = HammerDataProvider.GetAssSFIsNotBase(ins.EmployeeWW);
                                    if (sfass != null)
                                    {
                                        ins.RegionID = sfass.RegionID;
                                        ins.AreaID = sfass.AreaID;
                                        if (HammerDataProvider.GetNameRegion(sfass.RegionID) != null)
                                            ins.RegionName = HammerDataProvider.GetNameRegion(sfass.RegionID).RegionName;
                                        if (HammerDataProvider.GetNameArea(sfass.AreaID) != null)
                                            ins.AreaName = HammerDataProvider.GetNameArea(sfass.AreaID).AreaName;
                                        eRoute.Models.eCalendar.Distributor dis = HammerDataProvider.GetDistributorsCD(sfass.DistributorID.Value);
                                        if (dis != null)
                                        {
                                            ins.DistributorCD = dis.CompanyCD;
                                            ins.DistributorName = dis.CompanyName;
                                            eRoute.Models.eCalendar.DMSProvince pr = HammerDataProvider.GetProvinceName(dis.ProvinceID);
                                            if (pr != null)
                                            {
                                                ins.ProviceID = dis.ProvinceID;
                                                ins.ProviceName = pr.Province;
                                            }
                                        }
                                    }
                                    char checkrs = HammerDataProvider.getResultCharSS(model.Year, model.Week.Value, item.EmployeeID, model.EmployeeID);
                                    if (checkrs != '0')
                                    {
                                        ins.RefResult = checkrs;
                                        DMSHamResultsSS Route = HammerDataProvider.getResultSS(model.Year, model.Week.Value, ins.EmployeeWW, model.EmployeeID);
                                        if (Route != null)
                                            ins.SortID = Route.SortID;
                                        ListWW.Add(ins);
                                    }
                                }
                                else if (query.TerritoryType == 'A' && query.IsSalesForce == true)
                                {
                                    eRoute.Models.eCalendar.DMSSFAssignment sfass = HammerDataProvider.GetAssSFIsBase(ins.EmployeeWW);
                                    if (sfass != null)
                                    {
                                        ins.RegionID = sfass.RegionID;
                                        ins.AreaID = sfass.AreaID;
                                        if (HammerDataProvider.GetNameRegion(sfass.RegionID) != null)
                                            ins.RegionName = HammerDataProvider.GetNameRegion(sfass.RegionID).RegionName;
                                        if (HammerDataProvider.GetNameArea(sfass.AreaID) != null)
                                            ins.AreaName = HammerDataProvider.GetNameArea(sfass.AreaID).AreaName;
                                    }
                                    char checkrs = HammerDataProvider.getResultCharASM(model.Year, model.Week.Value, item.EmployeeID, model.EmployeeID);
                                    if (checkrs != '0')
                                    {
                                        ins.RefResult = checkrs;
                                        DMSHamResultsASM Route = HammerDataProvider.getResultASM(model.Year, model.Week.Value, ins.EmployeeWW, model.EmployeeID);
                                        if (Route != null)
                                            ins.SortID = Route.SortID;
                                        ListWW.Add(ins);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }

                Session["GirdWW"] = ListWW;
                //
                #region NoWW
                List<PrepareNoWWwModel> ListNoWW = new List<PrepareNoWWwModel>();
                ////check
                PrepareNonWW linenodata = HammerDataProvider.GetInPreapreNoWW(model.Year, model.Week.Value, model.EmployeeID);
                if (linenodata != null)
                {
                    int index = 1;
                    List<PrepareNonWW> listnoitem = HammerDataProvider.GetListInPreapreNoWW(model.Year, model.Week.Value, model.EmployeeID);
                    listnoitem = listnoitem.OrderBy(x => x.CreatedDateTime).ToList();
                    foreach (PrepareNonWW line in listnoitem)
                    {
                        PrepareNoWWwModel ins = new PrepareNoWWwModel();
                        ins.Day = line.Day;
                        ins.Des = line.Des;
                        ins.gvEmployeeID = line.EmployeeID;
                        ins.RefWWType = line.RefWWType;
                        ins.Week = line.Week;
                        ins.Year = line.Year;
                        ins.Shift = line.Shift;
                        ins.No = index;
                        ins.Status = line.Status.ToString();
                        ListNoWW.Add(ins);
                        index++;
                    }
                }// Hieu Add 29-07-2015 Yeu cau them lich no ww khong can check.
                else
                {
                    int no = 1;
                    for (int i = 2; i <= 7; i++)
                    {
                        PrepareNoWWwModel ins = new PrepareNoWWwModel();
                        #region Case Shift
                        switch (i)
                        {
                            case 2:
                                ins.Day = "Mo";
                                break;
                            case 3:
                                ins.Day = "Tu";
                                break;
                            case 4:
                                ins.Day = "We";
                                break;
                            case 5:
                                ins.Day = "Th";
                                break;
                            case 6:
                                ins.Day = "Fr";
                                break;
                            case 7:
                                ins.Day = "Sa";
                                break;
                        }
                        #endregion
                        ins.gvEmployeeID = model.EmployeeID;
                        ins.Year = model.Year;
                        ins.Week = model.Week;
                        ins.No = no;
                        if (HammerDataProvider.GetNonWWType() != null)
                        {
                            ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                        }
                        for (int j = 0; j <= 1; j++)
                        {
                            if (j == 0)
                            {
                                ins.Shift = "AM";
                                ins.Status = "N";
                                ListNoWW.Add(ins);
                                no++;
                            }
                            else
                            {
                                PrepareNoWWwModel inspm = new PrepareNoWWwModel();
                                inspm.Day = ins.Day;
                                inspm.No = no;
                                inspm.Des = ins.Des;
                                inspm.gvEmployeeID = ins.gvEmployeeID;
                                inspm.RefWWType = ins.RefWWType;
                                inspm.Week = ins.Week;
                                inspm.Year = ins.Year;
                                inspm.Shift = "PM";
                                inspm.Status = "N";
                                ListNoWW.Add(inspm);
                                no++;
                            }
                        }
                    }
                }
                Session["GirdNoWW"] = ListNoWW;
                #endregion               
                SendApp = String.Empty;
                return "OK";

            }
            else
            {
                return "Có lỗi xảy ra trên màn hình!";
            }

            #endregion

        }
        #region New
        #region Partial
        public string SendApp = String.Empty;
        public ActionResult TabPartialView()
        {
            return PartialView();
        }
        public ActionResult DetailViewNoWW()
        {
            if (Session["GirdNoWW"] == null)
                return PartialView("DetailViewNoWW", new List<PrepareNoWWwModel>());
            List<PrepareNoWWwModel> ListNoWW = Session["GirdNoWW"] as List<PrepareNoWWwModel>;
            ListNoWW = ListNoWW.OrderBy(x => x.No).ToList();
            Session["GirdNoWW"] = ListNoWW;
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("DetailViewNoWW", Session["GirdNoWW"]);
        }
        public ActionResult UpdateDetailViewNoWW(PrepareNoWWwModel model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string input = model.Des;
            if (input == null)
                input = String.Empty;
            input = input.Trim(); // xóa các khoảng trắng vô nghĩ ở 2 đầu
            int count = 1;
            for (int i = 1; i < input.Length; i++)
                if (input[i] == ' ' && input[i - 1] != ' ')
                    count++;
            if (count < 8)
            {
                ViewData["EditError"] = "Bạn mới nhập: " + count + " từ. "; //+ Utility.Phrase("SubmitAssessment.ErrMinimumWords");
               // ModelState.AddModelError("", Utility.Phrase("SubmitAssessment.ErrMinimumWords"));
            }
            if (ModelState.IsValid)
            {
                var list = Session["GirdNoWW"] as List<PrepareNoWWwModel>;
                (from item in list where item.No == model.No select item).
                    ToList().ForEach(item =>
                    {
                        item.Des = model.Des;
                        item.RefWWType = model.RefWWType;
                    });
                #region UpdateWW Check Des
                var findNoWW = list.Find(f => f.No == model.No);
                var listWW = Session["GirdWW"] as List<PrepareWW>;
                (from item in listWW where item.EmployeeID == findNoWW.gvEmployeeID select item).
                    ToList().ForEach(item =>
                    {
                        #region Case Shift
                        switch (findNoWW.Day)
                        {
                            case "Mo":
                                if (model.Shift == "AM")
                                {
                                    item.MoAMDes = null;
                                    item.MoAM = false;
                                }
                                else
                                {
                                    item.MoPMDes = null;
                                    item.MoPM = false;
                                }
                                break;
                            case "Tu":
                                if (model.Shift == "AM")
                                {
                                    item.TuAMDes = null;
                                    item.TuAM = false;
                                }
                                else
                                {
                                    item.TuPMDes = null;
                                    item.TuPM = false;
                                }
                                break;
                            case "We":
                                if (model.Shift == "AM")
                                {
                                    item.WeAMDes = null;
                                    item.WeAM = false;
                                }
                                else
                                {
                                    item.WePMDes = null;
                                    item.WePM = false;
                                }
                                break;
                            case "Th":
                                if (model.Shift == "AM")
                                {
                                    item.ThAMDes = null;
                                    item.ThAM = false;
                                }
                                else
                                {
                                    item.ThPMDes = null;
                                    item.ThPM = false;
                                }
                                break;
                            case "Fr":
                                if (model.Shift == "AM")
                                {
                                    item.FrAMDes = null;
                                    item.FrAM = false;
                                }
                                else
                                {
                                    item.FrPMDes = null;
                                    item.FrPM = false;
                                }
                                break;

                            case "Sa":
                                if (model.Shift == "AM")
                                {
                                    item.SaAMDes = null;
                                    item.SaAM = false;
                                }
                                else
                                {
                                    item.SaPMDes = null;
                                    item.SaPM = false;
                                }
                                break;
                        }
                        #endregion
                    });
                #endregion
                Session["GirdNoWW"] = list;
                Session["GirdWW"] = listWW;
            }
            return PartialView("DetailViewNoWW", Session["GirdNoWW"]);
        }
        public ActionResult DetailViewWW()
        {
            if (Session["GirdWW"] == null)
                return PartialView("DetailViewWW", new List<PrepareWW>());
            List<PrepareWW> ListWW = Session["GirdWW"] as List<PrepareWW>;
            ListWW = ListWW.OrderBy(x => x.SortID).ToList();
            Session["GirdWW"] = ListWW;
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("DetailViewWW", Session["GirdWW"]);
        }
        public ActionResult ComboBoxPartialYear()
        {
            string EmployeeID = Request.Params["EmployeeID"].ToString();
            List<Year> listYear = new List<Year>();
            Year now = new Year();
            now.YearID = DateTime.Now.Year.ToString();
            listYear.Add(now);
            List<ScheduleSubmitSetting> listopen = HammerDataProvider.GetOpenForYear(EmployeeID);
            foreach (ScheduleSubmitSetting item in listopen)
            {
                Year ins = new Year();
                ins.YearID = item.Date.Year.ToString();
                var rs = listYear.Find(f => f.YearID == item.Date.Year.ToString());
                if (rs == null)
                {
                    listYear.Add(ins);
                }
            }

            return PartialView(listYear);

        }
        public ActionResult ComboBoxPartialWeek()
        {
            string EmployeeID = Request.Params["EmployeeID"].ToString();
            string YearID = Request.Params["Year"].ToString();
            List<Week> listWeek = new List<Week>();
            Week now = new Week();
            now.WeekID = GetWeekOrderInYear(DateTime.Now) + 1;
            if (YearID == DateTime.Now.Year.ToString())
            {
                listWeek.Add(now);
            }
            List<ScheduleSubmitSetting> listopen = HammerDataProvider.GetOpenForYear(EmployeeID);
            foreach (ScheduleSubmitSetting item in listopen)
            {
                Week ins = new Week();
                ins.WeekID = GetWeekOrderInYear(item.Date.Date);
                var rs = listWeek.Find(f => f.WeekID == ins.WeekID);
                if (rs == null)
                {
                    if (YearID == item.Date.Year.ToString())
                    {
                        listWeek.Add(ins);
                    }
                }
            }
            listWeek = listWeek.OrderBy(x => x.WeekID).ToList();
            return PartialView(listWeek);
        }
        public ActionResult UnCheckWW()
        {
            if (Session["GirdWW"] != null)
            {
                #region WW
                PopUpDesModel model = new PopUpDesModel();
                model.Year = Request.Params["year"];
                model.Week = Convert.ToInt32(Request.Params["Week"].ToString());
                model.EmployeeWW = Request.Params["employeesWW"];
                model.EmployeeID = Request.Params["employeeID"];
                model.ID = Request.Params["iD"];
                model.Shift = Request.Params["shift"];
                model.Des = Request.Params["des"];
                List<PrepareWW> ListWW = Session["GirdWW"] as List<PrepareWW>;
                (from item in ListWW
                 where item.Year == model.Year
                       && item.Week == model.Week
                       && item.EmployeeID == model.EmployeeID
                        && item.EmployeeWW == model.EmployeeWW
                 select item).
                     ToList().ForEach(item =>
                     {
                         #region Case
                         switch (model.ID)
                         {
                             case "Mo":
                                 if (model.Shift == "AM")
                                 {
                                     item.MoAM = null;
                                     item.MoAMDes = null;
                                 }
                                 else
                                 {
                                     item.MoPM = null;
                                     item.MoPMDes = null;
                                 }
                                 break;
                             case "Tu":
                                 if (model.Shift == "AM")
                                 {
                                     item.TuAM = null;
                                     item.TuAMDes = null;
                                 }
                                 else
                                 {
                                     item.TuPM = null;
                                     item.TuPMDes = null;
                                 }
                                 break;
                             case "We":
                                 if (model.Shift == "AM")
                                 {
                                     item.WeAM = null;
                                     item.WeAMDes = null;
                                 }
                                 else
                                 {

                                     item.WePM = null;
                                     item.WePMDes = null;
                                 }
                                 break;
                             case "Th":
                                 if (model.Shift == "AM")
                                 {
                                     item.ThAM = null;
                                     item.ThAMDes = null;
                                 }
                                 else
                                 {
                                     item.ThPM = null;
                                     item.ThPMDes = null;
                                 }
                                 break;
                             case "Fr":
                                 if (model.Shift == "AM")
                                 {
                                     item.FrAM = null;
                                     item.FrAMDes = null;
                                 }
                                 else
                                 {
                                     item.FrPM = null;
                                     item.FrPMDes = null;
                                 }
                                 break;

                             case "Sa":
                                 if (model.Shift == "AM")
                                 {
                                     item.SaAM = null;
                                     item.SaAMDes = null;
                                 }
                                 else
                                 {
                                     item.SaPM = null;
                                     item.SaPMDes = null;
                                 }
                                 break;
                         }
                         #endregion
                     });
                #endregion
                #region NoneWW
                List<PrepareNoWWwModel> ListNoWW = new List<PrepareNoWWwModel>();
                if (Session["GirdNoWW"] == null)
                {
                    ListNoWW = new List<PrepareNoWWwModel>();
                }
                else
                {
                    ListNoWW = Session["GirdNoWW"] as List<PrepareNoWWwModel>;
                }
                PrepareNoWWwModel updateNoWW = ListNoWW.Find(f =>
               f.gvEmployeeID == model.EmployeeID
               && f.Year == model.Year
               && f.Week == model.Week
               && f.Shift == model.Shift
               && f.Day == model.ID);
                if (updateNoWW != null)
                {
                    (from item in ListNoWW
                     where item.gvEmployeeID == model.EmployeeID
                        && item.Year == model.Year
                        && item.Week == model.Week
                        && item.Shift == model.Shift
                        && item.Day == model.ID
                     select item).
                     ToList().ForEach(item =>
                     {
                         item.Day = updateNoWW.Day;
                         item.Des = null;
                         item.gvEmployeeID = model.EmployeeID;
                         item.RefWWType = updateNoWW.RefWWType;
                         item.Week = updateNoWW.Week;
                         item.Year = updateNoWW.Year;
                         item.Shift = updateNoWW.Shift;
                         item.No = updateNoWW.No;
                     });
                }
                else
                {
                    PrepareNoWWwModel item = new PrepareNoWWwModel();
                    item.Day = model.ID;
                    item.Des = null;
                    item.gvEmployeeID = model.EmployeeID;
                    item.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                    item.Week = model.Week;
                    item.Year = model.Year;
                    item.Shift = model.Shift;
                    item.Status = "N";
                    PrepareNoWWwModel maxNo = ListNoWW.OrderByDescending(i => i.No).FirstOrDefault();
                    if (maxNo != null)
                    {
                        item.No = maxNo.No + 1;
                    }
                    else
                    {
                        item.No = 1;
                    }
                    ListNoWW.Add(item);
                }
                #endregion
            }
            return Json("");
        }
        public ActionResult AddNoWW()
        {
            if (Session["GirdWW"] == null)
                return Json("");
            List<PrepareWW> ListWW = Session["GirdWW"] as List<PrepareWW>;
            int idex = 1;
            bool flagMoAM = true;
            bool flagTuAM = true;
            bool flagWeAM = true;
            bool flagThAM = true;
            bool flagFrAM = true;
            bool flagSaAM = true;
            bool flagMoPM = true;
            bool flagTuPM = true;
            bool flagWePM = true;
            bool flagThPM = true;
            bool flagFrPM = true;
            bool flagSaPM = true;
            List<PrepareNoWWwModel> ListNeedNoWW = new List<PrepareNoWWwModel>();
            if (Session["GirdNoWW"] == null)
            {
                ListNeedNoWW = new List<PrepareNoWWwModel>();
            }
            else
            {
                ListNeedNoWW = Session["GirdNoWW"] as List<PrepareNoWWwModel>;
            }
            foreach (PrepareWW model in ListWW)
            {
                //check
                PrepareNonWW linenodata = HammerDataProvider.GetInPreapreNoWW(model.Year, model.Week, model.EmployeeID);
                if (linenodata == null)
                {
                    #region Mon
                    if (flagMoAM == true)
                    {
                        if (model.MoAM == true)
                        {
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Mo" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                    }
                    if (flagMoPM == true)
                    {
                        if (model.MoPM == true)
                        {
                            flagMoPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Mo" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagMoPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Mo";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region Tus
                    if (flagTuAM == true)
                    {
                        if (model.TuAM == true)
                        {
                            flagTuAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Tu" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                    }
                    if (flagTuPM == true)
                    {
                        if (model.TuPM == true)
                        {
                            flagTuPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Tu" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagTuPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Tu";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region We
                    if (flagWeAM == true)
                    {
                        if (model.WeAM == true)
                        {
                            flagWeAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "We" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                    }
                    if (flagWePM == true)
                    {
                        if (model.WePM == true)
                        {
                            flagWePM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "We" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagWePM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Tu";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region Th
                    if (flagThAM == true)
                    {
                        if (model.ThAM == true)
                        {
                            flagThAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Th" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                    }
                    if (flagThPM == true)
                    {
                        if (model.ThPM == true)
                        {
                            flagThPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Th" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagThPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Th";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region Fr
                    if (flagFrAM == true)
                    {
                        if (model.FrAM == true)
                        {
                            flagFrAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Fr" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                    }
                    if (flagFrPM == true)
                    {
                        if (model.FrPM == true)
                        {
                            flagFrPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Fr" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagFrPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Fr";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region Sa
                    if (flagSaAM == true)
                    {
                        if (model.SaAM == true)
                        {
                            flagSaAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Sa" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                    }
                    if (flagSaPM == true)
                    {
                        if (model.SaPM == true)
                        {
                            flagSaPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Sa" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagSaPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Sa";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                }
                else // da ton tai roi
                {
                    #region Mon
                    if (flagMoAM == true)
                    {
                        if (model.MoAM == true)
                        {
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Mo" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagMoAM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Mo";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "AM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }

                    }
                    if (flagMoPM == true)
                    {
                        if (model.MoPM == true)
                        {
                            flagMoPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Mo" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagMoPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Mo";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region Tus
                    if (flagTuAM == true)
                    {
                        if (model.TuAM == true)
                        {
                            flagTuAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Tu" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagTuAM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Tu";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "AM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}

                        }
                    }
                    if (flagTuPM == true)
                    {
                        if (model.TuPM == true)
                        {
                            flagTuPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Tu" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagTuPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Tu";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region We
                    if (flagWeAM == true)
                    {
                        if (model.WeAM == true)
                        {
                            flagWeAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "We" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagWeAM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "We";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "AM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}

                        }
                    }
                    if (flagWePM == true)
                    {
                        if (model.WePM == true)
                        {
                            flagWePM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "We" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagWePM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Tu";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region Th
                    if (flagThAM == true)
                    {
                        if (model.ThAM == true)
                        {
                            flagThAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Th" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagThAM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Th";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "AM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}

                        }
                    }
                    if (flagThPM == true)
                    {
                        if (model.ThPM == true)
                        {
                            flagThPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Th" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagThPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Th";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region Fr
                    if (flagFrAM == true)
                    {
                        if (model.FrAM == true)
                        {
                            flagFrAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Fr" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagFrAM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Fr";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "AM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}

                        }
                    }
                    if (flagFrPM == true)
                    {
                        if (model.FrPM == true)
                        {
                            flagFrPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Fr" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagFrPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Fr";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                    #region Sa
                    if (flagSaAM == true)
                    {
                        if (model.SaAM == true)
                        {
                            flagSaAM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Sa" && f.Shift == "AM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagSaAM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Sa";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "AM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}

                        }
                    }
                    if (flagSaPM == true)
                    {
                        if (model.SaPM == true)
                        {
                            flagSaPM = false;
                            var find = ListNeedNoWW.Find(f => f.gvEmployeeID == model.EmployeeID && f.Day == "Sa" && f.Shift == "PM");
                            if (find != null)
                            {
                                ListNeedNoWW.Remove(find);
                            }
                        }
                        else
                        {
                            //flagSaPM = true;
                            //PrepareNoWWwModel ins = new PrepareNoWWwModel();
                            //ins.gvEmployeeID = model.EmployeeID;
                            //ins.Year = model.Year;
                            //ins.Week = model.Week;
                            //ins.Day = "Sa";
                            //if (HammerDataProvider.GetNonWWType() != null)
                            //{
                            //    ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                            //}
                            //ins.Shift = "PM";
                            //ins.Status = "N";
                            //var find = ListNeedNoWW.Find(f => f.gvEmployeeID == ins.gvEmployeeID && f.Day == ins.Day && f.Shift == ins.Shift);
                            //if (find == null)
                            //{
                            //    ins.No = idex;
                            //    ListNeedNoWW.Add(ins);
                            //    idex++;
                            //}
                            //else
                            //{
                            //    ListNeedNoWW.Remove(find);
                            //    ins.No = find.No;
                            //    ListNeedNoWW.Add(ins);
                            //}
                        }
                    }
                    #endregion
                }
            }
            Session["GirdNoWW"] = ListNeedNoWW;
            return Json("");
        }
        #endregion
        public static int GetWeekOrderInYear(DateTime time)
        {
            CultureInfo myCI = CultureInfo.CurrentCulture;
            Calendar myCal = myCI.Calendar;
            CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
            DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
            return myCal.GetWeekOfYear(time, myCWR, myFirstDOW);
        }
        [HttpPost]
        public ActionResult Process(PrepareScheduleNewModel model)
        {
            #region AddWW
            List<PrepareWW> ListWW = new List<PrepareWW>();
            if (!string.IsNullOrEmpty(model.Year) && model.Week != null && !string.IsNullOrEmpty(model.EmployeeID))
            {
                //Check đã tồn tại trong lưu trưc.                
                List<EmployeeModel> ListEmpWW = HammerDataProvider.GetSubordinateNoDuplicate(model.EmployeeID);
                foreach (EmployeeModel item in ListEmpWW)
                {
                    //check
                    PrepareWW linedata = new PrepareWW();
                    if (HammerDataProvider.EmployeeInRole(item.EmployeeID) == SystemRole.Salesman)
                    {
                        linedata = HammerDataProvider.GetInPreapreWW(model.Year, model.Week.Value, model.EmployeeID, item.RouteID);
                    }
                    else
                    {
                        linedata = HammerDataProvider.GetInPreapreWW(model.Year, model.Week.Value, model.EmployeeID, item.EmployeeID);
                    }
                    if (linedata != null)
                    {
                        #region GetNameInfor SF
                        SystemRole role = HammerDataProvider.EmployeeInRole(item.EmployeeID);
                        //Get MCP if @employeeID is salesman                       
                        if (role == SystemRole.SalesForce)
                        {
                           eRoute.Models.eCalendar.DMSSFHierarchy query = eRoute.Models.eCalendar.HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(item.EmployeeID);
                            if (query != null)
                            {
                                if (query.TerritoryType == 'D' && query.IsSalesForce == true)
                                {
                                    eRoute.Models.eCalendar.DMSSFAssignment sfass = HammerDataProvider.GetAssSFIsBase(linedata.EmployeeWW);
                                    if (sfass != null)
                                    {
                                        linedata.RegionID = sfass.RegionID;
                                        if (HammerDataProvider.GetNameRegion(sfass.RegionID) != null)
                                            linedata.RegionName = HammerDataProvider.GetNameRegion(sfass.RegionID).RegionName;
                                        linedata.AreaID = sfass.AreaID;
                                        if (HammerDataProvider.GetNameArea(sfass.AreaID) != null)
                                            linedata.AreaName = HammerDataProvider.GetNameArea(sfass.AreaID).AreaName;

                                        eRoute.Models.eCalendar.Distributor dis = HammerDataProvider.GetDistributorsCD(sfass.DistributorID.Value);
                                        if (dis != null)
                                        {
                                            linedata.DistributorCD = dis.CompanyCD;
                                            linedata.DistributorName = dis.CompanyName;
                                            eRoute.Models.eCalendar.DMSProvince pr = HammerDataProvider.GetProvinceName(dis.ProvinceID);
                                            if (pr != null)
                                            {
                                                linedata.ProviceID = dis.ProvinceID;
                                                linedata.ProviceName = pr.Province;
                                            }
                                        }
                                    }
                                    DMSHamResultsSS Route = HammerDataProvider.getResultSS(linedata.Year, linedata.Week, linedata.EmployeeWW, linedata.EmployeeID);
                                    if (Route != null)
                                        linedata.SortID = Route.SortID;

                                }
                                else if (query.TerritoryType == 'A' && query.IsSalesForce == true)
                                {
                                    eRoute.Models.eCalendar.DMSSFAssignment sfass = HammerDataProvider.GetAssSFIsBase(linedata.EmployeeWW);
                                    if (sfass != null)
                                    {
                                        linedata.RegionID = sfass.RegionID;
                                        linedata.AreaID = sfass.AreaID;
                                        if (HammerDataProvider.GetNameRegion(sfass.RegionID) != null)
                                            linedata.RegionName = HammerDataProvider.GetNameRegion(sfass.RegionID).RegionName;
                                        if (HammerDataProvider.GetNameArea(sfass.AreaID) != null)
                                            linedata.AreaName = HammerDataProvider.GetNameArea(sfass.AreaID).AreaName;
                                    }
                                    DMSHamResultsASM Route = HammerDataProvider.getResultASM(linedata.Year, linedata.Week, linedata.EmployeeWW, linedata.EmployeeID);
                                    if (Route != null)
                                        linedata.SortID = Route.SortID;
                                }
                            }
                        }
                        #endregion
                        #region GetNameInfo SM
                        else
                        {
                            linedata.EmployeeWW = item.EmployeeID;
                            Salesperson sp = HammerDataProvider.GetSMName(linedata.EmployeeWW);
                            if (sp != null)
                            {
                                linedata.EmployeeWWName = sp.Descr;
                            }
                            DMSDistributorRouteAssignment route = HammerDataProvider.GetRouteWithSMandSS(linedata.EmployeeID, linedata.EmployeeWW);
                            if (route != null)
                            {
                                eRoute.Models.eCalendar.Distributor dis = HammerDataProvider.GetDistributorsCD(route.CompanyID);
                                if (dis != null)
                                {
                                    linedata.DistributorCD = dis.CompanyCD;
                                    linedata.DistributorName = dis.CompanyName;
                                    eRoute.Models.eCalendar.DMSProvince pr = HammerDataProvider.GetProvinceName(dis.ProvinceID);
                                    if (pr != null)
                                    {
                                        linedata.ProviceID = dis.ProvinceID;
                                        linedata.ProviceName = pr.Province;
                                    }
                                    linedata.RegionID = dis.RegionID;
                                    linedata.AreaID = dis.AreaID;
                                    if (HammerDataProvider.GetNameRegion(dis.RegionID) != null)
                                        linedata.RegionName = HammerDataProvider.GetNameRegion(dis.RegionID).RegionName;
                                    if (HammerDataProvider.GetNameArea(dis.AreaID) != null)
                                        linedata.AreaName = HammerDataProvider.GetNameArea(dis.AreaID).AreaName;
                                }
                            }
                            DMSHamResult Route = HammerDataProvider.getResultRoute(linedata.Year, linedata.Week, linedata.EmployeeID, linedata.RouteID);
                            if (Route != null)
                                linedata.SortID = Route.SortID;
                        }
                        #endregion
                        //linedata.Status = 'N';                       
                        ListWW.Add(linedata);
                    }
                    else
                    {
                        PrepareWW ins = new PrepareWW();
                        ins.EmployeeWW = item.EmployeeID;
                        ins.EmployeeID = model.EmployeeID;
                        ins.Year = model.Year;
                        ins.Week = model.Week.Value;
                        ins.UserLogin = User.Identity.Name;
                        ins.CreatedDateTime = DateTime.Now;
                        ins.Status = 'N';
                        SystemRole role = HammerDataProvider.EmployeeInRole(item.EmployeeID);
                        //Get MCP if @employeeID is salesman
                        #region SM
                        if (role == SystemRole.Salesman)
                        {
                            DMSDistributorRouteAssignment route = HammerDataProvider.GetRouteWithSMandSS(ins.EmployeeID, ins.EmployeeWW);
                            if (route != null)
                            {
                                ins.RouteID = route.RouteCD;
                                eRoute.Models.eCalendar.Distributor dis = HammerDataProvider.GetDistributorsCD(route.CompanyID);
                                if (dis != null)
                                {
                                    ins.DistributorCD = dis.CompanyCD;
                                    ins.DistributorName = dis.CompanyName;
                                    eRoute.Models.eCalendar.DMSProvince pr = HammerDataProvider.GetProvinceName(dis.ProvinceID);
                                    if (pr != null)
                                    {
                                        ins.ProviceID = dis.ProvinceID;
                                        ins.ProviceName = pr.Province;
                                    }
                                    ins.RegionID = dis.RegionID;
                                    ins.AreaID = dis.AreaID;
                                    if (HammerDataProvider.GetNameRegion(dis.RegionID) != null)
                                        ins.RegionName = HammerDataProvider.GetNameRegion(dis.RegionID).RegionName;
                                    if (HammerDataProvider.GetNameArea(dis.AreaID) != null)
                                        ins.AreaName = HammerDataProvider.GetNameArea(dis.AreaID).AreaName;
                                }
                            }
                            else
                            {
                                ins.RouteID = string.Empty;
                            }
                            Salesperson sp = HammerDataProvider.GetSMName(ins.EmployeeWW);
                            if (sp != null)
                            {
                                ins.EmployeeWWName = sp.Descr;
                                char rs = HammerDataProvider.getResult(model.Year, model.Week.Value, item.EmployeeID, model.EmployeeID, sp.UsrInitDate);
                                if (rs != '0')
                                {
                                    ins.RefResult = rs;
                                    DMSHamResult Route = HammerDataProvider.getResultRoute(model.Year, model.Week.Value, model.EmployeeID, ins.RouteID);
                                    if (Route != null)
                                        ins.SortID = Route.SortID;
                                    ListWW.Add(ins);
                                }
                            }
                            else
                            {
                                ins.RefResult = '0';
                                ListWW.Add(ins);
                            }

                        }
                        #endregion
                        #region SS
                        else
                        {
                            ins.RouteID = string.Empty;
                            ins.EmployeeWWName = HammerDataProvider.GetNameEmployee(ins.EmployeeWW);

                            eRoute.Models.eCalendar.DMSSFHierarchy query = eRoute.Models.eCalendar.HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(item.EmployeeID);
                            if (query != null)
                            {
                                if (query.TerritoryType == 'D' && query.IsSalesForce == true)
                                {
                                    eRoute.Models.eCalendar.DMSSFAssignment sfass = HammerDataProvider.GetAssSFIsNotBase(ins.EmployeeWW);
                                    if (sfass != null)
                                    {
                                        ins.RegionID = sfass.RegionID;
                                        ins.AreaID = sfass.AreaID;
                                        if (HammerDataProvider.GetNameRegion(sfass.RegionID) != null)
                                            ins.RegionName = HammerDataProvider.GetNameRegion(sfass.RegionID).RegionName;
                                        if (HammerDataProvider.GetNameArea(sfass.AreaID) != null)
                                            ins.AreaName = HammerDataProvider.GetNameArea(sfass.AreaID).AreaName;
                                       eRoute.Models.eCalendar.Distributor dis = HammerDataProvider.GetDistributorsCD(sfass.DistributorID.Value);
                                        if (dis != null)
                                        {
                                            ins.DistributorCD = dis.CompanyCD;
                                            ins.DistributorName = dis.CompanyName;
                                            eRoute.Models.eCalendar.DMSProvince pr = HammerDataProvider.GetProvinceName(dis.ProvinceID);
                                            if (pr != null)
                                            {
                                                ins.ProviceID = dis.ProvinceID;
                                                ins.ProviceName = pr.Province;
                                            }
                                        }
                                    }
                                    char checkrs = HammerDataProvider.getResultCharSS(model.Year, model.Week.Value, item.EmployeeID, model.EmployeeID);
                                    if (checkrs != '0')
                                    {
                                        ins.RefResult = checkrs;
                                        DMSHamResultsSS Route = HammerDataProvider.getResultSS(model.Year, model.Week.Value, ins.EmployeeWW, model.EmployeeID);
                                        if (Route != null)
                                            ins.SortID = Route.SortID;
                                        ListWW.Add(ins);
                                    }
                                }
                                else if (query.TerritoryType == 'A' && query.IsSalesForce == true)
                                {
                                    eRoute.Models.eCalendar.DMSSFAssignment sfass = HammerDataProvider.GetAssSFIsBase(ins.EmployeeWW);
                                    if (sfass != null)
                                    {
                                        ins.RegionID = sfass.RegionID;
                                        ins.AreaID = sfass.AreaID;
                                        if (HammerDataProvider.GetNameRegion(sfass.RegionID) != null)
                                            ins.RegionName = HammerDataProvider.GetNameRegion(sfass.RegionID).RegionName;
                                        if (HammerDataProvider.GetNameArea(sfass.AreaID) != null)
                                            ins.AreaName = HammerDataProvider.GetNameArea(sfass.AreaID).AreaName;
                                    }
                                    char checkrs = HammerDataProvider.getResultCharASM(model.Year, model.Week.Value, item.EmployeeID, model.EmployeeID);
                                    if (checkrs != '0')
                                    {
                                        ins.RefResult = checkrs;
                                        DMSHamResultsASM Route = HammerDataProvider.getResultASM(model.Year, model.Week.Value, ins.EmployeeWW, model.EmployeeID);
                                        if (Route != null)
                                            ins.SortID = Route.SortID;
                                        ListWW.Add(ins);
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }

                Session["GirdWW"] = ListWW;

                //
                #region NoWW
                List<PrepareNoWWwModel> ListNoWW = new List<PrepareNoWWwModel>();
                ////check
                PrepareNonWW linenodata = HammerDataProvider.GetInPreapreNoWW(model.Year, model.Week.Value, model.EmployeeID);
                if (linenodata != null)
                {
                    int index = 1;
                    List<PrepareNonWW> listnoitem = HammerDataProvider.GetListInPreapreNoWW(model.Year, model.Week.Value, model.EmployeeID);
                    listnoitem = listnoitem.OrderBy(x => x.CreatedDateTime).ToList();
                    foreach (PrepareNonWW line in listnoitem)
                    {
                        PrepareNoWWwModel ins = new PrepareNoWWwModel();
                        ins.Day = line.Day;
                        ins.Des = line.Des;
                        ins.gvEmployeeID = line.EmployeeID;
                        ins.RefWWType = line.RefWWType;
                        ins.Week = line.Week;
                        ins.Year = line.Year;
                        ins.Shift = line.Shift;
                        ins.No = index;
                        ins.Status = line.Status.ToString();
                        ListNoWW.Add(ins);
                        index++;
                    }
                }// Hieu Add 29-07-2015 Yeu cau them lich no ww khong can check.
                else
                {
                    int no = 1;
                    for (int i = 2; i <= 7; i++)
                    {
                        PrepareNoWWwModel ins = new PrepareNoWWwModel();
                        #region Case Shift
                        switch (i)
                        {
                            case 2:
                                ins.Day = "Mo";
                                break;
                            case 3:
                                ins.Day = "Tu";
                                break;
                            case 4:
                                ins.Day = "We";
                                break;
                            case 5:
                                ins.Day = "Th";
                                break;
                            case 6:
                                ins.Day = "Fr";
                                break;
                            case 7:
                                ins.Day = "Sa";
                                break;
                        }
                        #endregion
                        ins.gvEmployeeID = model.EmployeeID;
                        ins.Year = model.Year;
                        ins.Week = model.Week;
                        ins.No = no;
                        if (HammerDataProvider.GetNonWWType() != null)
                        {
                            ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                        }
                        for (int j = 0; j <= 1; j++)
                        {
                            if (j == 0)
                            {
                                ins.Shift = "AM";
                                ins.Status = "N";
                                ListNoWW.Add(ins);
                                no++;
                            }
                            else
                            {
                                PrepareNoWWwModel inspm = new PrepareNoWWwModel();
                                inspm.Day = ins.Day;
                                inspm.No = no;
                                inspm.Des = ins.Des;
                                inspm.gvEmployeeID = ins.gvEmployeeID;
                                inspm.RefWWType = ins.RefWWType;
                                inspm.Week = ins.Week;
                                inspm.Year = ins.Year;
                                inspm.Shift = "PM";
                                inspm.Status = "N";
                                ListNoWW.Add(inspm);
                                no++;
                            }
                        }
                    }
                }
                Session["GirdNoWW"] = ListNoWW;
                #endregion
                //return PartialView("TabPartialView", Session["GirdWW"]);
                //if (HammerDataProvider.GetNumberPreapreWW(model.Year, model.Week.Value, model.EmployeeID) > 0)
                //{
                //    if (HammerDataProvider.GetNumberPreapreWW(model.Year, model.Week.Value, model.EmployeeID) != ListEmpWW.Count())
                //    {
                //        return Json("Số lượng nhân viên ww có sự thay đổi!");
                //    }
                //}
                SendApp = String.Empty;
                return Json("");

            }
            else
            {
                return Json("Có lỗi xảy ra trên màn hình!");
            }

            #endregion

        }
        public ActionResult AddDes()
        {
            PopUpDesModel model = new PopUpDesModel();
            model.Year = Request.Params["year"];
            model.Week = Convert.ToInt32(Request.Params["Week"].ToString());
            model.EmployeeWW = Request.Params["employeesWW"];
            model.EmployeeID = Request.Params["employeeID"];
            model.ID = Request.Params["iD"];
            model.Shift = Request.Params["shift"];
            model.Des = Request.Params["des"];
            List<PrepareWW> ListWW = new List<PrepareWW>();
            if (Session["GirdWW"] == null)
                ListWW = new List<PrepareWW>();
            ListWW = Session["GirdWW"] as List<PrepareWW>;
            // update 
            (from item in ListWW
             where item.Year == model.Year
                   && item.Week == model.Week
                   && item.EmployeeID == model.EmployeeID
                    && item.EmployeeWW != model.EmployeeWW
             select item).
                  ToList().ForEach(item =>
                  {
                      #region Case
                      switch (model.ID)
                      {
                          case "Mo":
                              if (model.Shift == "AM")
                              {
                                  item.MoAM = null;
                                  item.MoAMDes = null;
                              }
                              else
                              {
                                  item.MoPM = null;
                                  item.MoPMDes = null;
                              }
                              break;
                          case "Tu":
                              if (model.Shift == "AM")
                              {
                                  item.TuAM = null;
                                  item.TuAMDes = null;
                              }
                              else
                              {
                                  item.TuPM = null;
                                  item.TuPMDes = null;
                              }
                              break;
                          case "We":
                              if (model.Shift == "AM")
                              {
                                  item.WeAM = null;
                                  item.WeAMDes = null;
                              }
                              else
                              {

                                  item.WePM = null;
                                  item.WePMDes = null;
                              }
                              break;
                          case "Th":
                              if (model.Shift == "AM")
                              {
                                  item.ThAM = null;
                                  item.ThAMDes = null;
                              }
                              else
                              {
                                  item.ThPM = null;
                                  item.ThPMDes = null;
                              }
                              break;
                          case "Fr":
                              if (model.Shift == "AM")
                              {
                                  item.FrAM = null;
                                  item.FrAMDes = null;
                              }
                              else
                              {
                                  item.FrPM = null;
                                  item.FrPMDes = null;
                              }
                              break;

                          case "Sa":
                              if (model.Shift == "AM")
                              {
                                  item.SaAM = null;
                                  item.SaAMDes = null;
                              }
                              else
                              {
                                  item.SaPM = null;
                                  item.SaPMDes = null;
                              }
                              break;
                      }
                      #endregion
                  });
            PrepareWW update = ListWW.Find(f =>
                f.EmployeeWW == model.EmployeeWW
                && f.Year == model.Year
                && f.Week == model.Week
                && f.EmployeeID == model.EmployeeID
                );
            (from item in ListWW
             where item.EmployeeWW == model.EmployeeWW
                 && item.Year == model.Year
                 && item.Week == model.Week
                 && item.EmployeeID == model.EmployeeID
             select item).
                  ToList().ForEach(item =>
                  {
                      item.CreatedDateTime = update.CreatedDateTime;
                      item.EmployeeID = update.EmployeeID;
                      item.EmployeeWW = update.EmployeeWW;
                      item.Year = update.Year;
                      item.Week = update.Week;
                      item.RefResult = update.RefResult;
                      item.RouteID = update.RouteID;
                      item.UserLogin = update.UserLogin;
                      #region Case Shift
                      switch (model.ID)
                      {
                          case "Mo":
                              if (model.Shift == "AM")
                              {
                                  item.MoAMDes = model.Des;
                                  item.MoAM = true;
                              }
                              else
                              {
                                  item.MoPMDes = model.Des;
                                  item.MoPM = true;
                              }
                              break;
                          case "Tu":
                              if (model.Shift == "AM")
                              {
                                  item.TuAMDes = model.Des;
                                  item.TuAM = true;
                              }
                              else
                              {
                                  item.TuPMDes = model.Des;
                                  item.TuPM = true;
                              }
                              break;
                          case "We":
                              if (model.Shift == "AM")
                              {
                                  item.WeAMDes = model.Des;
                                  item.WeAM = true;
                              }
                              else
                              {
                                  item.WePMDes = model.Des;
                                  item.WePM = true;
                              }
                              break;
                          case "Th":
                              if (model.Shift == "AM")
                              {
                                  item.ThAMDes = model.Des;
                                  item.ThAM = true;
                              }
                              else
                              {
                                  item.ThPMDes = model.Des;
                                  item.ThPM = true;
                              }
                              break;
                          case "Fr":
                              if (model.Shift == "AM")
                              {
                                  item.FrAMDes = model.Des;
                                  item.FrAM = true;
                              }
                              else
                              {
                                  item.FrPMDes = model.Des;
                                  item.FrPM = true;
                              }
                              break;

                          case "Sa":
                              if (model.Shift == "AM")
                              {
                                  item.SaAMDes = model.Des;
                                  item.SaAM = true;
                              }
                              else
                              {
                                  item.SaPMDes = model.Des;
                                  item.SaPM = true;
                              }
                              break;
                      }
                      #endregion
                  });
            //Update No WW
            #region UpdateNoWW
            List<PrepareNoWWwModel> ListNoWW = new List<PrepareNoWWwModel>();
            if (Session["GirdNoWW"] == null)
            {
                ListNoWW = new List<PrepareNoWWwModel>();
            }
            else
            {
                ListNoWW = Session["GirdNoWW"] as List<PrepareNoWWwModel>;
            }
            if (ListNoWW.Count() <= 0)
            {
                int no = 1;
                for (int i = 2; i <= 7; i++)
                {
                    PrepareNoWWwModel ins = new PrepareNoWWwModel();
                    #region Case Shift
                    switch (i)
                    {
                        case 2:
                            ins.Day = "Mo";
                            break;
                        case 3:
                            ins.Day = "Tu";
                            break;
                        case 4:
                            ins.Day = "We";
                            break;
                        case 5:
                            ins.Day = "Th";
                            break;
                        case 6:
                            ins.Day = "Fr";
                            break;
                        case 7:
                            ins.Day = "Sa";
                            break;
                    }
                    #endregion
                    ins.gvEmployeeID = model.EmployeeID;
                    ins.Year = model.Year;
                    ins.Week = model.Week;
                    ins.No = no;
                    if (HammerDataProvider.GetNonWWType() != null)
                    {
                        ins.RefWWType = HammerDataProvider.GetNonWWType().RefWWType;
                    }
                    for (int j = 0; j <= 1; j++)
                    {
                        if (j == 0)
                        {
                            ins.Shift = "AM";
                            ins.Status = "N";
                            ListNoWW.Add(ins);
                            no++;
                        }
                        else
                        {
                            PrepareNoWWwModel inspm = new PrepareNoWWwModel();
                            inspm.Day = ins.Day;
                            inspm.No = no;
                            inspm.Des = ins.Des;
                            inspm.gvEmployeeID = ins.gvEmployeeID;
                            inspm.RefWWType = ins.RefWWType;
                            inspm.Week = ins.Week;
                            inspm.Year = ins.Year;
                            inspm.Shift = "PM";
                            inspm.Status = "N";
                            ListNoWW.Add(inspm);
                            no++;
                        }
                    }
                }
            }
            PrepareNoWWwModel updateNoWW = ListNoWW.Find(f =>
            f.gvEmployeeID == model.EmployeeID
            && f.Year == model.Year
            && f.Week == model.Week
            && f.Shift == model.Shift
            && f.Day == model.ID);
            ListNoWW.Remove(updateNoWW);
            Session["GirdNoWW"] = ListNoWW;
            #endregion
            ListWW = ListWW.OrderBy(x => x.EmployeeWW).ToList();
            Session["GirdWW"] = ListWW;
            return Json("");
        }
        public ActionResult ClearGrid()
        {
            Session["GirdWW"] = new List<PrepareWW>();
            Session["GirdNoWW"] = new List<PrepareNoWWwModel>();
            return Json("Xóa dữ liệu trên lưới thành công");
        }
        [HttpPost]
        public ActionResult Save()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (Session["GirdWW"] == null)
            {
                SendApp = Utility.Phrase("NoDataProcessing");
                return Json(new { ID = '0', Mess = Utility.Phrase("NoDataProcessing") });
               // return Json(Utility.Phrase("NoDataProcessing"));
            }
            string year = Request.Params["year"];
            int week = Convert.ToInt32(Request.Params["Week"].ToString());
            string employeeID = Request.Params["employeeID"];
            List<PrepareWW> ListWW = Session["GirdWW"] as List<PrepareWW>;
            List<PrepareNoWWwModel> ListNoWW = Session["GirdNoWW"] as List<PrepareNoWWwModel>;
            ListWW = ListWW.OrderBy(x => x.SortID).ToList();
            bool FlagSS = false;
            #region LuuBangTamWW
            if (ListWW != null)
            {
                //Dalen PR
                //HammerDataProvider.ClearPrepareWW(year, week, employeeID);
                //foreach (PrepareWW item in ListWW)
                //{
                //    //if (item.Status == 'N')
                //    //{
                //    HammerDataProvider.InsertUpdatePrepareWW(item);
                //    //}
                //}
                //end
                //2015-20-11: Fix case Mo 2 ngay Save mo het ca tuan.
                //HammerDataProvider.ClearPrepareWW(year, week, employeeID);
                foreach (PrepareWW item in ListWW)
                {
                    //if (item.Status == 'N')
                    //{
                    HammerDataProvider.InsertUpdatePrepareWW(item);
                    //}
                }
            }
            #endregion
            int checkno = 0;
            int checknoNghiPhep = 0;
            #region LuuBangTamNoneWW
            if (ListNoWW != null)
            {
                HammerDataProvider.ClearPrepareNoWW(year, week, employeeID);
                foreach (PrepareNoWWwModel item in ListNoWW)
                {
                    if (!string.IsNullOrEmpty(item.Des))
                    {
                        checkno++;
                        if (item.RefWWType.Trim() == "NP")
                            checknoNghiPhep++;
                        HammerDataProvider.InsertUpdatePrepareNoWW(item, User.Identity.Name);
                    }
                }
            }
            #endregion
            //DMSHamResult checkValidate = HammerDataProvider.

            int checkww = 0;
            int PercentMin = 0;
            int checkWWSS = 0;
            int sttcheckWWSS = 1;
            //ListWW = ListWW.OrderBy(x => x.
            foreach (PrepareWW item in ListWW)
            {
                int CheckRowEmployeeNoWW = 0;
                #region CheckRowEmployeeNoWW
                eRoute.Models.eCalendar.PrepareNonWW NoneWW1 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "MoAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW2 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "TuAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW3 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "WeAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW4 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "ThAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW5 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "FrAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW6 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "SaAM");

                eRoute.Models.eCalendar.PrepareNonWW NoneWW7 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "MoPM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW8 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "TuPM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW9 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "WePM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW10 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "ThPM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW11 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "FrPM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW12 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "SaPM");
                if (NoneWW1 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW2 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW3 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW4 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW5 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW6 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW7 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW8 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW9 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW10 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW11 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW12 != null)
                    CheckRowEmployeeNoWW++;
                #endregion
                int check = 0;
                bool FlagNextWW = false;
                #region CheckSoBuoiWWTrongTuan.
                #region CountWW
                if (item.MoAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }
                if (item.MoPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }


                if (item.TuAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }
                if (item.TuPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.WeAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.WePM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }


                if (item.ThAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.ThPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }


                if (item.FrAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.FrPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.SaAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.SaPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }
                #endregion
                #region GanFlagWW
                if (FlagNextWW == false && item.MoAM == true && item.MoPM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.MoPM == true && item.TuAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.TuAM == true && item.TuPM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.TuPM == true && item.WeAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.WeAM == true && item.WePM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.WePM == true && item.ThAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.ThAM == true && item.ThPM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.ThPM == true && item.FrAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.FrAM == true && item.FrPM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.FrPM == true && item.SaAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.SaAM == true && item.SaPM == true)
                    FlagNextWW = true;
                #endregion
                SystemRole role = HammerDataProvider.EmployeeInRole(item.EmployeeWW);
                //Get MCP if @employeeID is salesman
                #region SM
                if (role == SystemRole.Salesman)
                {
                    Salesperson sp = HammerDataProvider.GetSMName(item.EmployeeWW);
                    char checkrs = HammerDataProvider.getResult(item.Year, item.Week, item.EmployeeWW, item.EmployeeID, sp.UsrInitDate);
                    if (checkrs != 'B')
                    {
                        #region ValidateNextWW
                        //2015-10-20: Lay dieu kien check Route IsValidate hay khong
                        DMSHamResult RowCheck = HammerDataProvider.getResultRoute(item.Year, item.Week, item.EmployeeID, item.RouteID);
                        //ConfigSystem cf = HammerDataProvider.getConfigSystem("VaNextWW");
                        if (RowCheck != null)
                        {
                            if (RowCheck.IsValidate == true)
                            {
                                if (RowCheck.IsWWNext == true)
                                {
                                    if (FlagNextWW == true)
                                    {
                                        //SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww liền kề";
                                        //return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww liền kề");
                                        SendApp = Utility.Phrase("Employee") + ": " + item.EmployeeWWName + " " + Utility.Phrase("eCalendar_Validate_HaveWWNextDate");
                                        return Json(new { ID = '0', Mess = SendApp });
                                    }
                                }
                                #region Sốbuổiww
                                DMSHamResult rs = HammerDataProvider.getResult(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                if (rs != null)
                                {
                                    DMSHamResultsSS rsPercent = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeID, item.EmployeeID);
                                    if (rs != null)
                                        PercentMin = rsPercent.PercentSSMin.Value;
                                    if (checkno < 12)
                                    {
                                        //2015-11-24:Fix WW 6 SM chi lam lich 3 thang du 12 lich thi khong thong bao loi.
                                        if ((checkww + checkno) < 12)
                                        {
                                            if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                            {
                                                SendApp = Utility.Phrase("Employee") + ": " + item.EmployeeWWName + " "+ Utility.Phrase("eCalendar_Validate_HaveWWNotAvaild") + "(" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                                return Json(new { ID = '0', Mess = SendApp });


                                                //SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                                //return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")");
                                               
                                            }
                                            else
                                            {
                                                if ((checkww + checkno) >= 12)
                                                    continue;
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                    else
                    { //Lay dieu kien sm cho SM moi
                        #region RouteNew
                        DMSHamResult RowCheck = HammerDataProvider.getResultRoute(item.Year, item.Week, item.EmployeeID, item.RouteID);
                        //ConfigSystem cf = HammerDataProvider.getConfigSystem("VaNextWW");
                        if (RowCheck != null)
                        {
                            if (RowCheck.IsValidate == true)
                            {
                                DMSHamResult rs = HammerDataProvider.getResult(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                // DMSHamResult rs = HammerDataProvider.getResultKem(item.Year, item.Week, item.EmployeeID);
                                //DMSHamResultsSS rs = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                if (rs != null)
                                {
                                    DMSHamResultsSS rsPercent = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeID, item.EmployeeID);
                                    PercentMin = rs.PercentSSMin.Value;
                                    if (checkno < 12)
                                    {
                                        //2015-11-24:Fix WW 6 SM chi lam lich 3 thang du 12 lich thi khong thong bao loi.
                                        if ((checkww + checkno) < 12)
                                        {
                                            if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                            {
                                                SendApp = Utility.Phrase("Employee") + ": " + item.EmployeeWWName + " " + Utility.Phrase("eCalendar_Validate_HaveWWNotAvaild") + "(" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                                return Json(new { ID = '0', Mess = SendApp });
                                                //SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")";
                                                //return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")");
                                                // }
                                            }
                                            else
                                            {
                                                if ((checkww + checkno) >= 12)
                                                    continue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
                #region Nhanvien
                else
                {
                    eRoute.Models.eCalendar.DMSSFHierarchy query = eRoute.Models.eCalendar.HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(item.EmployeeWW);
                    if (query.TerritoryType == 'D' && query.IsSalesForce == true)
                    {
                        FlagSS = true;
                        #region SS
                        char checkrs = HammerDataProvider.getResultCharSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                        if (checkrs != 'B')
                        {
                            DMSHamResultsSS rs = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                            if (rs != null)
                            {
                                if (rs.IsValidate == true)
                                {
                                    //DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                                    //if (PSASM != null)
                                    //    PercentMin = PSASM.PercentASMMin.Value;
                                    //DMSHamResultsSS PSASM = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                    DMSHamResultsASM PSASM = HammerDataProvider.getResultASM(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                    if (PSASM != null)
                                        PercentMin = PSASM.PercentASMMin.Value;
                                    if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                    {
                                        if ((checkww + checkno) < 12 && CheckRowEmployeeNoWW < 12)
                                        {
                                            SendApp = Utility.Phrase("Employee") + ": " + item.EmployeeWWName + " " + Utility.Phrase("eCalendar_Validate_HaveWWNotAvaild") + "(" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            return Json(new { ID = '0', Mess = SendApp });

                                            //SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            //return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")");
                                        }
                                    }
                                    else
                                    {
                                        if ((checkww + checkno) >= 12)
                                            continue;
                                    }

                                }
                            }
                        }
                        else // SS moi
                        {
                            DMSHamResultsSS rs = HammerDataProvider.getResultSSKem(item.Year, item.Week, item.EmployeeID);
                            if (rs != null)
                            {
                                if (rs.IsValidate == true)
                                {
                                    //DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                                    //if (PSASM != null)
                                    //    PercentMin = PSASM.PercentASMMin.Value;
                                    //DMSHamResultsSS PSASM = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                    DMSHamResultsASM PSASM = HammerDataProvider.getResultASM(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                    if (PSASM != null)
                                        PercentMin = PSASM.PercentASMMin.Value;
                                    if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                    {
                                        if ((checkww + checkno) < 12 && CheckRowEmployeeNoWW < 12)
                                        {
                                            SendApp = Utility.Phrase("Employee") + ": " + item.EmployeeWWName + " " + Utility.Phrase("eCalendar_Validate_HaveWWNotAvaild") + "(" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            return Json(new { ID = '0', Mess = SendApp });
                                            //SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")";
                                            //return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")");
                                        }
                                    }
                                    else
                                    {
                                        if ((checkww + checkno) >= 12)
                                            continue;
                                    }
                                }
                            }
                        }
                    }
                        #endregion
                    #region ASM
                    else
                    {
                        char checkrs = HammerDataProvider.getResultCharASM(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                        if (checkrs != 'B')
                        {
                            DMSHamResultsASM rs = HammerDataProvider.getResultASM(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                            if (rs != null)
                            {
                                if (rs.IsValidate == true)
                                {
                                    DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                                    if (PSASM != null)
                                        PercentMin = PSASM.PercentRSMMin.Value;
                                    if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                    {
                                        if ((checkww + checkno) < 12 && CheckRowEmployeeNoWW < 12)
                                        {
                                            SendApp = Utility.Phrase("Employee") + ": " + item.EmployeeWWName + " " + Utility.Phrase("eCalendar_Validate_HaveWWNotAvaild") + "(" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            return Json(new { ID = '0', Mess = SendApp });
                                            //SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            //return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")");
                                        }
                                    }
                                    else
                                    {
                                        if ((checkww + checkno) >= 12)
                                            continue;
                                    }
                                }
                            }
                        }
                        else // ASM Moi
                        {
                            DMSHamResultsASM rs = HammerDataProvider.getResultASMKem(item.Year, item.Week, item.EmployeeID);
                            if (rs != null)
                            {
                                if (rs.IsValidate == true)
                                {
                                    DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                                    if (PSASM != null)
                                        PercentMin = PSASM.PercentRSMMin.Value;
                                    if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                    {
                                        if ((checkww + checkno) < 12 && CheckRowEmployeeNoWW < 12)
                                        {
                                            SendApp = Utility.Phrase("Employee") + ": " + item.EmployeeWWName + " " + Utility.Phrase("eCalendar_Validate_HaveWWNotAvaild") + "(" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            return Json(new { ID = '0', Mess = SendApp });
                                            //SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")";
                                            //return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")");
                                        }
                                    }
                                    else
                                    {
                                        if ((checkww + checkno) >= 12)
                                            continue;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion
                #endregion
                sttcheckWWSS++;
            }
            #region TatCacTuanPhaiCoLich
            if ((checkww + checkno) < 12)
            {
                SendApp = Utility.Phrase("Ecalendar_Validate_AllSchedulerinWeek");
                return Json(new { ID = '0', Mess = Utility.Phrase("Ecalendar_Validate_AllSchedulerinWeek") });
                //return Json(Utility.Phrase("Ecalendar_Validate_AllSchedulerinWeek"));
            }
            #endregion
            #region %Min
            //check tong so buoi huan luyen toi thieu.            
            if ((12 - checknoNghiPhep) != 0)
            {
                if (((checkww * 100 / (12 - checknoNghiPhep) < PercentMin)))
                {
                    SendApp = Utility.Phrase("Ecalendar_Validate_PercentWWGreaterThan") + ": " + PercentMin;
                   // return Json(Utility.Phrase("Ecalendar_Validate_PercentWWGreaterThan") + ": " + PercentMin);
                    return Json(new { ID = '0', Mess = Utility.Phrase("Ecalendar_Validate_PercentWWGreaterThan") + ": " + PercentMin });
                }
            }
            #endregion
            #region SoSSWWMin
            if (FlagSS == true)
            {
                //SystemSetting setting = HammerDataProvider.GetSystemSetting("MinSS");
                DMSHamResultsASM setting = HammerDataProvider.getResultASM(year, week, employeeID);
                if (setting != null)
                {
                    if (ListWW.Count() <= setting.NumberSSMin)
                    {
                        if (checkWWSS != ListWW.Count())
                        {
                            SendApp = Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + ListWW.Count();
                            //return Json(Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + ListWW.Count());
                            return Json(new { ID = '0', Mess = Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + ListWW.Count() });
                        }
                    }
                    else // lon hon 6  pahi ww di du 6 thang.
                    {
                        if (checkWWSS < setting.NumberSSMin)
                        {
                            SendApp = Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + setting.NumberSSMin;
                            //return Json(Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + setting.NumberSSMin);
                            return Json(new { ID = '0', Mess = Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + setting.NumberSSMin });
                        }
                    }
                }
            }
            #endregion
            //return Json("Lưu dữ liệu thành công");
            return Json(new { ID = '1', Mess = Utility.Phrase("SaveSuccessfully") });
           // return Json(Utility.Phrase("SaveSuccessfully"));
            //return PartialView("GridPartialView", Session["GirdWW"]);
        }
        public ActionResult ValidateData(string year, int week, string employeeID)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (Session["GirdWW"] == null)
            {
                SendApp = Utility.Phrase("NoDataProcessing");               
                return Json(new { ID = '0', Mess = Utility.Phrase("NoDataProcessing") });
                //return Json(Utility.Phrase("NoDataProcessing"));
            }
            List<PrepareWW> ListWW = Session["GirdWW"] as List<PrepareWW>;
            List<PrepareNoWWwModel> ListNoWW = Session["GirdNoWW"] as List<PrepareNoWWwModel>;
            ListWW = ListWW.OrderBy(x => x.SortID).ToList();
            bool FlagSS = false;
            if (ListWW != null)
            {
                HammerDataProvider.ClearPrepareWW(year, week, employeeID);
                foreach (PrepareWW item in ListWW)
                {
                    HammerDataProvider.InsertUpdatePrepareWW(item);
                }
            }
            int checkno = 0;
            int checknoNghiPhep = 0;
            if (ListNoWW != null)
            {
                HammerDataProvider.ClearPrepareNoWW(year, week, employeeID);
                foreach (PrepareNoWWwModel item in ListNoWW)
                {

                    if (!string.IsNullOrEmpty(item.Des))
                    {
                        checkno++;
                        if (item.RefWWType.Trim() == "NP")
                            checknoNghiPhep++;
                        HammerDataProvider.InsertUpdatePrepareNoWW(item, User.Identity.Name);
                    }
                }
            }
            int checkww = 0;
            int PercentMin = 0;
            int checkWWSS = 0;
            int sttcheckWWSS = 1;
            foreach (PrepareWW item in ListWW)
            {
                int CheckRowEmployeeNoWW = 0;
                #region CheckRowEmployeeNoWW
                eRoute.Models.eCalendar.PrepareNonWW NoneWW1 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "MoAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW2 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "TuAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW3 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "WeAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW4 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "ThAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW5 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "FrAM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW6 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "AM", "SaAM");

                eRoute.Models.eCalendar.PrepareNonWW NoneWW7 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "MoPM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW8 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "TuPM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW9 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "WePM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW10 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "ThPM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW11 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "FrPM");
                eRoute.Models.eCalendar.PrepareNonWW NoneWW12 = eRoute.Models.eCalendar.HammerDataProvider.GetInPreapreWWNone(year.ToString(), Convert.ToInt32(week), item.EmployeeWW, "PM", "SaPM");
                if (NoneWW1 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW2 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW3 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW4 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW5 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW6 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW7 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW8 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW9 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW10 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW11 != null)
                    CheckRowEmployeeNoWW++;
                if (NoneWW12 != null)
                    CheckRowEmployeeNoWW++;

                #endregion
                int check = 0;
                bool FlagNextWW = false;
                #region CheckSoBuoiWWTrongTuan.
                if (item.MoAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }
                if (item.MoPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }


                if (item.TuAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }
                if (item.TuPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.WeAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.WePM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }


                if (item.ThAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.ThPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }


                if (item.FrAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.FrPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.SaAM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }

                if (item.SaPM == true)
                {
                    checkWWSS = sttcheckWWSS;
                    check++;
                    checkww++;
                }
                if (FlagNextWW == false && item.MoAM == true && item.MoPM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.MoPM == true && item.TuAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.TuAM == true && item.TuPM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.TuPM == true && item.WeAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.WeAM == true && item.WePM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.WePM == true && item.ThAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.ThAM == true && item.ThPM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.ThPM == true && item.FrAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.FrAM == true && item.FrPM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.FrPM == true && item.SaAM == true)
                    FlagNextWW = true;

                if (FlagNextWW == false && item.SaAM == true && item.SaPM == true)
                    FlagNextWW = true;
                SystemRole role = HammerDataProvider.EmployeeInRole(item.EmployeeWW);
                //Get MCP if @employeeID is salesman
                #region SM
                if (role == SystemRole.Salesman)
                {
                    Salesperson sp = HammerDataProvider.GetSMName(item.EmployeeWW);
                    char checkrs = HammerDataProvider.getResult(item.Year, item.Week, item.EmployeeWW, item.EmployeeID, sp.UsrInitDate);
                    if (checkrs != 'B')
                    {
                        #region ValidateNextWW
                        //2015-10-20: Lay dieu kien check Route IsValidate hay khong
                        DMSHamResult RowCheck = HammerDataProvider.getResultRoute(item.Year, item.Week, item.EmployeeID, item.RouteID);
                        //ConfigSystem cf = HammerDataProvider.getConfigSystem("VaNextWW");
                        if (RowCheck != null)
                        {
                            if (RowCheck.IsValidate == true)
                            {
                                //ConfigSystem cf = HammerDataProvider.getConfigSystem("VaNextWW");
                                //if (cf != null)
                                //{
                                //    if (cf.Status == true)
                                //    {
                                if (RowCheck.IsWWNext == true)
                                {
                                    if (FlagNextWW == true)
                                    {
                                        SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww liền kề";
                                        return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww liền kề");
                                    }
                                }
                                #region Sốbuổiww
                                DMSHamResult rs = HammerDataProvider.getResult(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                               
                                if (rs != null)
                                {
                                    DMSHamResultsSS rsPercent = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeID, item.EmployeeID);
                                    if (rs != null)
                                        PercentMin = rsPercent.PercentSSMin.Value;
                                    //2015-11-24:Fix WW 6 SM chi lam lich 3 thang du 12 lich thi khong thong bao loi.
                                    if ((checkww + checkno) < 12)
                                    {
                                        if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                        {
                                            //2015-09-24: loi dk lon hon buoi hl
                                            //if ((checkww + checkno) < 12)
                                            //{
                                            SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")");
                                            // }
                                        }
                                        else
                                        {
                                            if ((checkww + checkno) >= 12)
                                                continue;
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                    else
                    { //Lay dieu kien sm cho SM moi
                        #region RouteNew
                        DMSHamResult RowCheck = HammerDataProvider.getResultRoute(item.Year, item.Week, item.EmployeeID, item.RouteID);
                        //ConfigSystem cf = HammerDataProvider.getConfigSystem("VaNextWW");
                        if (RowCheck != null)
                        {
                            if (RowCheck.IsValidate == true)
                            {
                                //DMSHamResult rs = HammerDataProvider.getResultKem(item.Year, item.Week, item.EmployeeID);
                                //DMSHamResultsSS rs = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                DMSHamResult rs = HammerDataProvider.getResult(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                if (rs != null)
                                {
                                    DMSHamResultsSS rsPercent = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeID, item.EmployeeID);
                                    if (rs != null)
                                        PercentMin = rsPercent.PercentSSMin.Value;   
                                    //PercentMin = rs.PercentSSMin.Value;
                                    //2015-11-24:Fix WW 6 SM chi lam lich 3 thang du 12 lich thi khong thong bao loi.
                                    if ((checkww + checkno) < 12)
                                    {
                                        if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                        {
                                            //2015-09-24: loi dk lon hon buoi hl
                                            //if ((checkww + checkno) < 12)
                                            //{
                                            SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")";
                                            return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")");
                                            // }
                                        }
                                        else
                                        {
                                            if ((checkww + checkno) >= 12)
                                                continue;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
                #region Nhanvien
                else
                {
                    eRoute.Models.eCalendar.DMSSFHierarchy query = eRoute.Models.eCalendar.HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(item.EmployeeWW);
                    if (query.TerritoryType == 'D' && query.IsSalesForce == true)
                    {
                        FlagSS = true;
                        #region SS
                        char checkrs = HammerDataProvider.getResultCharSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                        if (checkrs != 'B')
                        {
                            DMSHamResultsSS rs = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                            if (rs != null)
                            {
                                if (rs.IsValidate == true)
                                {
                                    //DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                                    //if (PSASM != null)
                                    //    PercentMin = PSASM.PercentASMMin.Value;
                                    DMSHamResultsASM PSASM = HammerDataProvider.getResultASM(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);

                                    //DMSHamResultsSS PSASM = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                    if (PSASM != null)
                                        PercentMin = PSASM.PercentASMMin.Value;
                                    if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                    {
                                        if ((checkww + checkno) < 12 && CheckRowEmployeeNoWW < 12)
                                        {
                                            SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")");
                                        }
                                    }
                                    else
                                    {
                                        if ((checkww + checkno) >= 12)
                                            continue;
                                    }
                                }
                            }
                        }
                        else // SS moi
                        {
                            DMSHamResultsSS rs = HammerDataProvider.getResultSSKem(item.Year, item.Week, item.EmployeeID);
                            if (rs != null)
                            {
                                if (rs.IsValidate == true)
                                {
                                    //DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                                    //if (PSASM != null)
                                    //    PercentMin = PSASM.PercentASMMin.Value;
                                    //DMSHamResultsSS PSASM = HammerDataProvider.getResultSS(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                                    DMSHamResultsASM PSASM = HammerDataProvider.getResultASM(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);

                                    if (PSASM != null)
                                        PercentMin = PSASM.PercentASMMin.Value;
                                    if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                    {
                                        if ((checkww + checkno) < 12 && CheckRowEmployeeNoWW < 12)
                                        {
                                            SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")";
                                            return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")");
                                        }
                                    }
                                    else
                                    {
                                        if ((checkww + checkno) >= 12)
                                            continue;
                                    }
                                }
                            }
                        }
                    }
                        #endregion
                    #region ASM
                    else
                    {
                        char checkrs = HammerDataProvider.getResultCharASM(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                        if (checkrs != 'B')
                        {
                            DMSHamResultsASM rs = HammerDataProvider.getResultASM(item.Year, item.Week, item.EmployeeWW, item.EmployeeID);
                            if (rs != null)
                            {
                                if (rs.IsValidate == true)
                                {
                                    DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                                    if (PSASM != null)
                                        PercentMin = PSASM.PercentRSMMin.Value;
                                    if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                    {
                                        if ((checkww + checkno) < 12 && CheckRowEmployeeNoWW < 12)
                                        {
                                            SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")";
                                            return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + " <= x <= " + rs.NumHLMax + ")");
                                        }
                                    }
                                    else
                                    {
                                        if ((checkww + checkno) >= 12)
                                            continue;
                                    }
                                }
                            }
                        }
                        else // ASM Moi
                        {
                            DMSHamResultsASM rs = HammerDataProvider.getResultASMKem(item.Year, item.Week, item.EmployeeID);
                            if (rs != null)
                            {
                                if (rs.IsValidate == true)
                                {
                                    DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                                    if (PSASM != null)
                                        PercentMin = PSASM.PercentRSMMin.Value;
                                    if ((check < rs.NumHLMin || check > rs.NumHLMax))
                                    {
                                        if ((checkww + checkno) < 12 && CheckRowEmployeeNoWW < 12)
                                        {
                                            SendApp = "Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")";
                                            return Json("Nhân viên: " + item.EmployeeWW + " có số buổi ww không hợp lệ (" + rs.NumHLMin + "<x<" + rs.NumHLMax + ")");
                                        }
                                    }
                                    else
                                    {
                                        if ((checkww + checkno) >= 12)
                                            continue;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion
                #endregion
                sttcheckWWSS++;
            }

            if ((checkww + checkno) < 12)
            {
                SendApp = Utility.Phrase("Ecalendar_Validate_AllSchedulerinWeek");
                //return Json(Utility.Phrase("Ecalendar_Validate_AllSchedulerinWeek"));
                return Json(new { ID = '0', Mess = Utility.Phrase("Ecalendar_Validate_AllSchedulerinWeek") });
            }
            //check tong so buoi huan luyen toi thieu.            
            if ((12 - checknoNghiPhep) != 0)
            {
                if (((checkww * 100 / (12 - checknoNghiPhep) < PercentMin)))
                {
                    SendApp = Utility.Phrase("Ecalendar_Validate_PercentWWGreaterThan") + ": "  + PercentMin;
                    //return Json(Utility.Phrase("Ecalendar_Validate_PercentWWGreaterThan") + ": "+  PercentMin);
                    return Json(new { ID = '0', Mess = Utility.Phrase("Ecalendar_Validate_PercentWWGreaterThan") });
                }
            }

            if (FlagSS == true)
            {
                //SystemSetting setting = HammerDataProvider.GetSystemSetting("MinSS");
                DMSHamResultsASM setting = HammerDataProvider.getResultASM(year, week, employeeID);
                if (setting != null)
                {
                    if (ListWW.Count() <= setting.NumberSSMin)
                    {
                        if (checkWWSS != ListWW.Count())
                        {
                            SendApp = Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") +": "+ ListWW.Count();
                           // return Json(Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") +": " + ListWW.Count());
                            return Json(new { ID = '0', Mess = Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + ListWW.Count() });
                        }
                    }
                    else // lon hon 6  pahi ww di du 6 thang.
                    {
                        if (checkWWSS < setting.NumberSSMin)
                        {
                            SendApp = Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + setting.NumberSSMin;
                            //return Json(Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") +": "+ setting.NumberSSMin);
                            return Json(new { ID = '0', Mess = Utility.Phrase("Ecalendar_Validate_NumberEmployeeWW") + ": " + setting.NumberSSMin });
                        }
                    }
                }
            }
            return Json(Utility.Phrase("SaveSuccessfully"));
            //return PartialView("GridPartialView", Session["GirdWW"]);
        }
        private static DateTime GetFirstDayOfWeek(DateTime dayInWeek, DayOfWeek dayOfWeek)
        {
            DateTime firstDayInWeek = dayInWeek.Date;
            while (firstDayInWeek.DayOfWeek != dayOfWeek)
            {
                firstDayInWeek = firstDayInWeek.AddDays(-1);
            }
            return firstDayInWeek;
        }

        public ActionResult Send()
        {
            string year = Request.Params["year"];
            int week = Convert.ToInt32(Request.Params["Week"].ToString());
            string employeeID = Request.Params["employeeID"];
            List<SendEmailPreapre> listsend = new List<SendEmailPreapre>();
            this.ValidateData(year, week, employeeID);
            if (SendApp != String.Empty)
                return Json(new { ID = '0', Mess = SendApp });
                //return Json(SendApp);
            #region Process
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            List<PrepareWW> ListWW = Session["GirdWW"] as List<PrepareWW>;
            List<PrepareNoWWwModel> ListNoWW = Session["GirdNoWW"] as List<PrepareNoWWwModel>;
            if (ListWW != null)
            {
                foreach (PrepareWW item in ListWW)
                {
                    HammerDataProvider.SendPrepareWW(item);
                    #region Add Apppoinment
                    Appointment app = new Appointment();
                    #region BuoiSang
                    if (item.MoAM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("M");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1;
                        app.EndDate = d2;
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.MoAMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.ShiftID = "AM";
                        app.IsMeeting = false;
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                        Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role == Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "MoAM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.TuAM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("M");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(1);
                        app.EndDate = d2.AddDays(1);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.TuAMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "AM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                        Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role == Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "TuAM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.WeAM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("M");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(2);
                        app.EndDate = d2.AddDays(2);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.WeAMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "AM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                        Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role == Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "WeAM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.ThAM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("M");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(3);
                        app.EndDate = d2.AddDays(3);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.ThAMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "AM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role ==  Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "ThAM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.FrAM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("M");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(4);
                        app.EndDate = d2.AddDays(4);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.FrAMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "AM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role ==  Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "FrAM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.SaAM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("M");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(5);
                        app.EndDate = d2.AddDays(5);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.SaAMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "AM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role ==  Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "SaAM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    #endregion
                    #region BuoiChieu
                    if (item.MoPM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("T");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1;
                        app.EndDate = d2;
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.MoPMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "PM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role ==  Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "MoPM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.TuPM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("T");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(1);
                        app.EndDate = d2.AddDays(1);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.TuPMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "PM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role == Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "TuPM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.WePM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("T");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(2);
                        app.EndDate = d2.AddDays(2);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.WePMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "PM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role == Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "WePM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.ThPM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("T");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(3);
                        app.EndDate = d2.AddDays(3);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.ThPMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "PM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role ==  Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "ThPM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.FrPM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("T");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(4);
                        app.EndDate = d2.AddDays(4);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.FrPMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "PM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role ==  Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "FrPM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    if (item.SaPM == true)
                    {
                        DateTime d1;
                        DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                        d1 = d1.AddDays(7 * (item.Week - 1));
                        while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                        ShiftSetting AM = HammerDataProvider.GetShift("T");
                        DateTime st = Convert.ToDateTime(AM.StartTime);
                        d1 = d1.AddHours(st.Hour);
                        d1 = d1.AddMinutes(st.Minute);
                        DateTime d2 = d1.Date;
                        DateTime end = Convert.ToDateTime(AM.EndTime);
                        d2 = d2.AddHours(end.Hour);
                        d2 = d2.AddMinutes(end.Minute);
                        app.StartDate = d1.AddDays(5);
                        app.EndDate = d2.AddDays(5);
                        app.Subject = Utility.Phrase("Training");
                        app.IsWW = true;
                        app.Description = item.SaPMDes;
                        app.Employees = item.EmployeeWW;
                        app.UserLogin = item.EmployeeID;
                        app.UserAppro = User.Identity.Name;
                        app.RouteID = item.RouteID;
                        app.IsSM = true;
                        app.ScheduleType = "D";
                        app.Label = 3;
                        app.CreatedDateTime = DateTime.Now;
                        app.AllDay = false;
                        app.Status = 0;
                        app.IsMeeting = false;
                        app.ShiftID = "PM";
                        app.IsDelete = false;
                        app.Week = item.Week;
                        app.Year = item.Year;
                        #region GetRoute
                         Hammer.Helpers.SystemRole role = eRoute.Models.eCalendar.HammerDataProvider.EmployeeInRole(app.Employees);
                        if (role ==  Hammer.Helpers.SystemRole.SalesForce)
                        {
                            Appointment appointment4 = HammerDataProvider.CheckWWGetAppointmentsShift(app.StartDate.Value.Date, app.Employees, app.ShiftID);
                            if (appointment4 != null)
                            {
                                app.RouteID = appointment4.RouteID;
                            }
                            app.IsSM = false;
                        }
                        #endregion
                        Appointment rscheck = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                        if (rscheck == null)
                        {
                            //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                            HammerDataProvider.InsertAppointment(app);
                        }
                        else
                        {
                            //2015-11-20: Neu khong mo ngay thi khong lam gi ca
                            if (app.Week <= GetWeekOrderInYear(DateTime.Now.Date))
                            {
                                eRoute.Models.eCalendar.ScheduleSubmitSetting openDate = eRoute.Models.eCalendar.HammerDataProvider.GetOpenEnableDate(app.UserLogin, app.Year, app.Week.Value, "SaPM");
                                if (openDate != null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    rscheck.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(rscheck);
                                }
                            }
                            else
                            {
                                //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                HammerDataProvider.InsertAppointment(app);
                                rscheck.IsDelete = true;
                                HammerDataProvider.UpdateAppointment(rscheck);
                            }
                        }
                        SendEmailPreapre send = new SendEmailPreapre();
                        send.EmployeeID = app.UserLogin;
                        send.Shift = app.ShiftID;
                        send.Week = item.Week;
                        send.Year = item.Year;
                        send.Date = app.StartDate.Value.Date;
                        listsend.Add(send);
                    }
                    #endregion
                    #endregion
                }
            }
            if (ListNoWW != null)
            {
                foreach (PrepareNoWWwModel item in ListNoWW)
                {
                    HammerDataProvider.SendPrepareNoWW(item, User.Identity.Name);
                    #region Add Apppoinment
                    Appointment app = new Appointment();
                    Appointment check;
                    SendEmailPreapre send;
                    if (!string.IsNullOrEmpty(item.Des))
                    {
                        app.IsDelete = false;
                        #region Case Shift
                        ShiftSetting ShiftSet = new ShiftSetting();
                        switch (item.Day)
                        {
                            case "Mo":

                                if (item.Shift == "AM")
                                {
                                    ShiftSet = HammerDataProvider.GetShift("M");
                                    app.ShiftID = "AM";
                                }
                                else
                                {
                                    ShiftSet = HammerDataProvider.GetShift("T");
                                    app.ShiftID = "PM";
                                }
                                DateTime d1;
                                DateTime.TryParse(item.Year.ToString() + "/01/01", out d1);
                                d1 = d1.AddDays(7 * (item.Week.Value - 1));
                                while (d1.DayOfWeek != DayOfWeek.Monday) d1 = d1.AddDays(-1);
                                DateTime st = Convert.ToDateTime(ShiftSet.StartTime);
                                d1 = d1.AddHours(st.Hour);
                                d1 = d1.AddMinutes(st.Minute);
                                DateTime d2 = d1.Date;
                                DateTime end = Convert.ToDateTime(ShiftSet.EndTime);
                                d2 = d2.AddHours(end.Hour);
                                d2 = d2.AddMinutes(end.Minute);
                                app.StartDate = d1;
                                app.EndDate = d2;
                                app.Subject = "No work with";
                                app.IsWW = false;
                                app.Description = item.Des;
                                app.Employees = "";
                                app.UserLogin = item.gvEmployeeID;
                                app.UserAppro = User.Identity.Name;
                                app.RouteID = null;
                                app.ScheduleType = "D";
                                app.Label = 3;
                                app.CreatedDateTime = DateTime.Now;
                                app.AllDay = false;
                                app.Status = 0;
                                if (item.RefWWType.Trim() == "HV")
                                {
                                    app.IsMeeting = true;
                                }
                                else
                                {
                                    app.IsMeeting = false;
                                }
                                app.Week = item.Week;
                                app.Year = item.Year;
                                check = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                                if (check == null)
                                {
                                    HammerDataProvider.InsertAppointment(app);
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                }
                                else
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                    app.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(app);
                                }
                                send = new SendEmailPreapre();
                                send.EmployeeID = app.UserLogin;
                                send.ID = item.Day;
                                send.Shift = item.Shift;
                                send.Week = item.Week;
                                send.Year = item.Year;
                                send.Date = app.StartDate.Value.Date;
                                listsend.Add(send);
                                send.Date = app.StartDate.Value.Date;
                                listsend.Add(send);
                                break;
                            case "Tu":

                                if (item.Shift == "AM")
                                {
                                    ShiftSet = HammerDataProvider.GetShift("M");
                                    app.ShiftID = "AM";
                                }
                                else
                                {
                                    ShiftSet = HammerDataProvider.GetShift("T");
                                    app.ShiftID = "PM";
                                }
                                DateTime Tud1;
                                DateTime.TryParse(item.Year.ToString() + "/01/01", out Tud1);
                                Tud1 = Tud1.AddDays(7 * (item.Week.Value - 1));
                                while (Tud1.DayOfWeek != DayOfWeek.Monday) Tud1 = Tud1.AddDays(-1);
                                DateTime Tust = Convert.ToDateTime(ShiftSet.StartTime);
                                Tud1 = Tud1.AddHours(Tust.Hour);
                                Tud1 = Tud1.AddMinutes(Tust.Minute);
                                DateTime Tud2 = Tud1.Date;
                                DateTime Tuend = Convert.ToDateTime(ShiftSet.EndTime);
                                Tud2 = Tud2.AddHours(Tuend.Hour);
                                Tud2 = Tud2.AddMinutes(Tuend.Minute);
                                app.StartDate = Tud1.AddDays(1);
                                app.EndDate = Tud2.AddDays(1);
                                app.Subject = "No work with";
                                app.IsWW = false;
                                app.Description = item.Des;
                                app.Employees = "";
                                app.UserLogin = item.gvEmployeeID;
                                app.UserAppro = User.Identity.Name;
                                app.RouteID = null;
                                app.ScheduleType = "D";
                                app.Label = 3;
                                app.CreatedDateTime = DateTime.Now;
                                app.AllDay = false;
                                app.Status = 0;
                                if (item.RefWWType.Trim() == "HV")
                                {
                                    app.IsMeeting = true;
                                }
                                else
                                {
                                    app.IsMeeting = false;
                                }
                                app.Week = item.Week;
                                app.Year = item.Year;
                                check = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                                if (check == null)
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                }
                                else
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                    app.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(app);
                                }
                                send = new SendEmailPreapre();
                                send.EmployeeID = app.UserLogin;
                                send.ID = item.Day;
                                send.Shift = item.Shift;
                                send.Week = item.Week;
                                send.Year = item.Year;
                                send.Date = app.StartDate.Value.Date;
                                listsend.Add(send);
                                break;
                            case "We":
                                if (item.Shift == "AM")
                                {
                                    ShiftSet = HammerDataProvider.GetShift("M");
                                    app.ShiftID = "AM";
                                }
                                else
                                {
                                    ShiftSet = HammerDataProvider.GetShift("T");
                                    app.ShiftID = "PM";
                                }
                                DateTime Wed1;
                                DateTime.TryParse(item.Year.ToString() + "/01/01", out Wed1);
                                Wed1 = Wed1.AddDays(7 * (item.Week.Value - 1));
                                while (Wed1.DayOfWeek != DayOfWeek.Monday) Wed1 = Wed1.AddDays(-1);
                                DateTime West = Convert.ToDateTime(ShiftSet.StartTime);
                                Wed1 = Wed1.AddHours(West.Hour);
                                Wed1 = Wed1.AddMinutes(West.Minute);
                                DateTime Wed2 = Wed1.Date;
                                DateTime Weend = Convert.ToDateTime(ShiftSet.EndTime);
                                Wed2 = Wed2.AddHours(Weend.Hour);
                                Wed2 = Wed2.AddMinutes(Weend.Minute);
                                app.StartDate = Wed1.AddDays(2);
                                app.EndDate = Wed2.AddDays(2);
                                app.Subject = "No work with";
                                app.IsWW = false;
                                app.Description = item.Des;
                                app.Employees = "";
                                app.UserLogin = item.gvEmployeeID;
                                app.UserAppro = User.Identity.Name;
                                app.RouteID = null;
                                app.ScheduleType = "D";
                                app.Label = 3;
                                app.CreatedDateTime = DateTime.Now;
                                app.AllDay = false;
                                app.Status = 0;
                                if (item.RefWWType.Trim() == "HV")
                                {
                                    app.IsMeeting = true;
                                }
                                else
                                {
                                    app.IsMeeting = false;
                                }
                                app.Week = item.Week;
                                app.Year = item.Year;
                                check = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                                if (check == null)
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                }
                                else
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                    app.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(app);
                                }
                                send = new SendEmailPreapre();
                                send.EmployeeID = app.UserLogin;
                                send.ID = item.Day;
                                send.Shift = item.Shift;
                                send.Week = item.Week;
                                send.Year = item.Year;
                                send.Date = app.StartDate.Value.Date;
                                listsend.Add(send);
                                break;
                            case "Th":
                                if (item.Shift == "AM")
                                {
                                    ShiftSet = HammerDataProvider.GetShift("M");
                                    app.ShiftID = "AM";
                                }
                                else
                                {
                                    ShiftSet = HammerDataProvider.GetShift("T");
                                    app.ShiftID = "PM";
                                }
                                DateTime Thd1;
                                DateTime.TryParse(item.Year.ToString() + "/01/01", out Thd1);
                                Thd1 = Thd1.AddDays(7 * (item.Week.Value - 1));
                                while (Thd1.DayOfWeek != DayOfWeek.Monday) Thd1 = Thd1.AddDays(-1);
                                DateTime Thst = Convert.ToDateTime(ShiftSet.StartTime);
                                Thd1 = Thd1.AddHours(Thst.Hour);
                                Thd1 = Thd1.AddMinutes(Thst.Minute);
                                DateTime Thd2 = Thd1.Date;
                                DateTime Thend = Convert.ToDateTime(ShiftSet.EndTime);
                                Thd2 = Thd2.AddHours(Thend.Hour);
                                Thd2 = Thd2.AddMinutes(Thend.Minute);
                                app.StartDate = Thd1.AddDays(3);
                                app.EndDate = Thd2.AddDays(3);
                                app.Subject = "No work with";
                                app.IsWW = false;
                                app.Description = item.Des;
                                app.Employees = "";
                                app.UserLogin = item.gvEmployeeID;
                                app.UserAppro = User.Identity.Name;
                                app.RouteID = null;
                                app.ScheduleType = "D";
                                app.Label = 3;
                                app.CreatedDateTime = DateTime.Now;
                                app.AllDay = false;
                                app.Status = 0;
                                if (item.RefWWType.Trim() == "HV")
                                {
                                    app.IsMeeting = true;
                                }
                                else
                                {
                                    app.IsMeeting = false;
                                }
                                app.Week = item.Week;
                                app.Year = item.Year;
                                check = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                                if (check == null)
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                }
                                else
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                    app.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(app);
                                }
                                send = new SendEmailPreapre();
                                send.EmployeeID = app.UserLogin;
                                send.ID = item.Day;
                                send.Shift = item.Shift;
                                send.Week = item.Week;
                                send.Year = item.Year;
                                send.Date = app.StartDate.Value.Date;
                                listsend.Add(send);
                                break;
                            case "Fr":
                                if (item.Shift == "AM")
                                {
                                    ShiftSet = HammerDataProvider.GetShift("M");
                                    app.ShiftID = "AM";
                                }
                                else
                                {
                                    ShiftSet = HammerDataProvider.GetShift("T");
                                    app.ShiftID = "PM";
                                }
                                DateTime Frd1;
                                DateTime.TryParse(item.Year.ToString() + "/01/01", out Frd1);
                                Frd1 = Frd1.AddDays(7 * (item.Week.Value - 1));
                                while (Frd1.DayOfWeek != DayOfWeek.Monday) Frd1 = Frd1.AddDays(-1);
                                DateTime Frst = Convert.ToDateTime(ShiftSet.StartTime);
                                Frd1 = Frd1.AddHours(Frst.Hour);
                                Frd1 = Frd1.AddMinutes(Frst.Minute);
                                DateTime Frd2 = Frd1.Date;
                                DateTime Frend = Convert.ToDateTime(ShiftSet.EndTime);
                                Frd2 = Frd2.AddHours(Frend.Hour);
                                Frd2 = Frd2.AddMinutes(Frend.Minute);
                                app.StartDate = Frd1.AddDays(4);
                                app.EndDate = Frd2.AddDays(4);
                                app.Subject = "No work with";
                                app.IsWW = false;
                                app.Description = item.Des;
                                app.Employees = "";
                                app.UserLogin = item.gvEmployeeID;
                                app.UserAppro = User.Identity.Name;
                                app.RouteID = null;
                                app.ScheduleType = "D";
                                app.Label = 3;
                                app.CreatedDateTime = DateTime.Now;
                                app.AllDay = false;
                                app.Status = 0;
                                if (item.RefWWType.Trim() == "HV")
                                {
                                    app.IsMeeting = true;
                                }
                                else
                                {
                                    app.IsMeeting = false;
                                }
                                app.Week = item.Week;
                                app.Year = item.Year;
                                check = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                                if (check == null)
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                }
                                else
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                    app.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(app);
                                }
                                send = new SendEmailPreapre();
                                send.EmployeeID = app.UserLogin;
                                send.ID = item.Day;
                                send.Shift = item.Shift;
                                send.Week = item.Week;
                                send.Year = item.Year;
                                send.Date = app.StartDate.Value.Date;
                                listsend.Add(send);
                                break;

                            case "Sa":
                                if (item.Shift == "AM")
                                {
                                    ShiftSet = HammerDataProvider.GetShift("M");
                                    app.ShiftID = "AM";
                                }
                                else
                                {
                                    ShiftSet = HammerDataProvider.GetShift("T");
                                    app.ShiftID = "PM";
                                }
                                DateTime Sad1;
                                DateTime.TryParse(item.Year.ToString() + "/01/01", out Sad1);
                                Sad1 = Sad1.AddDays(7 * (item.Week.Value - 1));
                                while (Sad1.DayOfWeek != DayOfWeek.Monday) Sad1 = Sad1.AddDays(-1);
                                DateTime Sast = Convert.ToDateTime(ShiftSet.StartTime);
                                Sad1 = Sad1.AddHours(Sast.Hour);
                                Sad1 = Sad1.AddMinutes(Sast.Minute);
                                DateTime Sad2 = Sad1.Date;
                                DateTime Saend = Convert.ToDateTime(ShiftSet.EndTime);
                                Sad2 = Sad2.AddHours(Saend.Hour);
                                Sad2 = Sad2.AddMinutes(Saend.Minute);
                                app.StartDate = Sad1.AddDays(5);
                                app.EndDate = Sad2.AddDays(5);
                                app.Subject = "No work with";
                                app.IsWW = false;
                                app.Description = item.Des;
                                app.Employees = "";
                                app.UserLogin = item.gvEmployeeID;
                                app.UserAppro = User.Identity.Name;
                                app.RouteID = null;
                                app.ScheduleType = "D";
                                app.Label = 3;
                                app.CreatedDateTime = DateTime.Now;
                                app.AllDay = false;
                                app.Status = 0;
                                if (item.RefWWType.Trim() == "HV")
                                {
                                    app.IsMeeting = true;
                                }
                                else
                                {
                                    app.IsMeeting = false;
                                }
                                app.Week = item.Week;
                                app.Year = item.Year;
                                check = HammerDataProvider.PreparNewCheckApp(app.StartDate.Value.Date, app.UserLogin, app.ShiftID);
                                if (check == null)
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                }
                                else
                                {
                                    //HammerDataProvider.CloseDateOpeningEm(app.StartDate.Value.Date, app.UserLogin.Trim());
                                    HammerDataProvider.InsertAppointment(app);
                                    app.IsDelete = true;
                                    HammerDataProvider.UpdateAppointment(app);
                                }
                                send = new SendEmailPreapre();
                                send.EmployeeID = app.UserLogin;
                                send.ID = item.Day;
                                send.Shift = item.Shift;
                                send.Week = item.Week;
                                send.Year = item.Year;
                                send.Date = app.StartDate.Value.Date;
                                listsend.Add(send);
                                break;
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            #endregion
            #region SendEmail
            string emailTemplate = Server.MapPath(Constant.EmailUploadScheduleHTML);
            var list2 = (
                from list in listsend
                select new
                {
                    EmployeeID = list.EmployeeID,
                    User = User.Identity.Name
                }).Distinct().ToList();
            foreach (var current in list2)
            {
                eRoute.Models.eCalendar.DMSSalesForce salesforceById3 = HammerDataProvider.GetSalesforceById(current.User);
                EmployeeModel employeeModel2 = HammerDataProvider.PrepareScheduleGetlevel(current.User);
                EmployeeModel employeeModel3 = new EmployeeModel();
                string str = "Home";
                employeeModel3 = HammerDataProvider.PrepareScheduleGetlevel(current.EmployeeID);
                eRoute.Models.eCalendar.DMSSalesForce salesforceById4 = HammerDataProvider.GetSalesforceById(employeeModel3.EmployeeID);
                try
                {
                    Util.InitUploadSchedulerEmailNew(salesforceById4, employeeModel3.EmployeeName, (employeeModel3 == null) ? "" : salesforceById3.EmployeeName, emailTemplate, listsend, Util.GetBaseUrl() + str, employeeModel2.Level, employeeModel3.Level);
                }
                catch (System.Exception)
                {
                }
            }
            #endregion
            //Session["GirdWW"] = new List<PrepareWW>();
            //Session["GirdNoWW"] = new List<PrepareNoWWwModel>();
            string mess = Utility.Phrase("SendScheduleOki");
            return Json(new { ID = '1', Mess = mess });
        }
        #endregion
    }
}
