using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammer.Models;
//using Utility.Phrase("ScheduleSubmitSetting");
using eRoute.Filters;
using WebMatrix.WebData;
using System.Globalization;
using DevExpress.Web.Mvc;
using Hammer.Helpers;
using eRoute.Models.eCalendar;
using eRoute;
using DMSERoute.Helpers;


namespace Hammer.Controllers
{
    [InitializeSimpleMembership]
    [Authorize()]
    public class ScheduleSubmitSettingController : Controller
    {
        //
        // GET: /ScheduleSubmitSetting/
        [Authorize]
        [ActionAuthorize("eCalendar_ScheduleSubmitSetting", true)]
        public ActionResult Index(string ScheduleType, string RegionID, string AreaID, string EmployeeID, string FromDate, string EndDate, string CloseTime, string Note)
        {
            //HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            //if (User.IsInRole("SysAdmin") == false)
            //{
            //    return RedirectToAction("index", "ErrorPermission");
            //}
            Session["IsDatabase"] = false;
            Session["ListDetailOpen"] = new List<ScheduleSubmitSettingEx>();
            List<ScheduleType> listItem = new List<ScheduleType>();
            ScheduleType sche = new ScheduleType();
            sche.ID = "All";
            sche.Name = Utility.Phrase("AllDay");//"Cả ngày";
            listItem.Add(sche);
            sche = new ScheduleType();
            sche.ID = "AM";
            sche.Name = Utility.Phrase("Morning"); //"Sáng";
            listItem.Add(sche);
            sche = new ScheduleType();
            sche.ID = "PM";
            sche.Name = Utility.Phrase("Afternoon");// "Chiều";
            listItem.Add(sche);
            // add so lieu khi click add
            SendScheduleAgainMode model = new SendScheduleAgainMode();
            model.sendEmail = true;
            if (!string.IsNullOrEmpty(EmployeeID))
            {
                model.ScheduleType = Utility.StringParse(EditorExtension.GetValue<string>("ScheduleType"));              
                model.regionID = Utility.StringParse(EditorExtension.GetValue<string>("RegionID"));
                model.areaID = Utility.StringParse(EditorExtension.GetValue<string>("AreaID"));
                model.FromDate = Utility.DateTimeParse(FromDate);
                model.EndDate = Utility.DateTimeParse(EndDate);
                model.EmployeeID = Utility.StringParse(EditorExtension.GetValue<string>("EmployeeID"));
                model.Note = Note;
                if (CloseTime != null)
                    model.CloseTime = Convert.ToInt32(CloseTime); 
                Session["ListDetailOpen"] = null;
                List<ScheduleSubmitSettingEx> List = new List<ScheduleSubmitSettingEx>();
                HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
                //ViewBag.Error = "";
                //if (String.IsNullOrEmpty(model.Note))
                //{
                //    ViewBag.Error = "Vui lòng chọn đủ các thông tin * trên màn hình";
                //    return View();
                //}
                #region Full filter
                if (!String.IsNullOrEmpty(model.regionID) && (!String.IsNullOrEmpty(model.areaID) || String.IsNullOrEmpty(model.areaID)) && !String.IsNullOrEmpty(model.EmployeeID))
                {
                    string Kind;
                    if (model.CloseTime > 0 && string.IsNullOrEmpty(model.Note) == false)
                    {
                        Kind = model.ScheduleType;
                        int days = model.EndDate.Subtract(model.FromDate).Days;
                        if (days < 0)
                        {
                            model.EmployeeID = model.EmployeeID;
                            ViewData["ErrNegativeDays"] = Utility.Phrase("ErrEndDateGreaterThanEqualFromDate");
                        }
                        else
                        {
                            if (model.EmployeeID != null)
                            {
                                bool FlagEror = false;
                                ViewData["ErrNegativeDays"] = null;
                                int no = days;
                                if (FlagEror == false)
                                {
                                    //check xem du lieu co ton tai ngay mo ngay ko neu mo ngay bao loi.
                                    string ErrorDate = null;
                                    while (no >= 0)
                                    {
                                        ScheduleSubmitSettingEx item = new ScheduleSubmitSettingEx();

                                        item.Date = model.FromDate.AddDays(no).Date;
                                        item.EmployeeID = model.EmployeeID;
                                        item.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
                                        item.Note = model.Note;
                                        item.Status = 0;
                                        item.UserLogin = User.Identity.Name;
                                        item.CreatedDate = DateTime.Now;
                                        item.Type = Kind;
                                        item.CloseTime = model.CloseTime;
                                        item.IsDatabase = false;
                                        if (HammerDataProvider.CheckAssessmentDate(item.Date, item.EmployeeID) == 1)
                                        {
                                            if (ErrorDate == null)
                                            {
                                                ErrorDate = item.Date.Date.ToString("dd/MM/yyyy");
                                            }
                                            else
                                            {
                                                ErrorDate = ErrorDate + ',' + item.Date.Date.ToString("dd/MM/yyyy");
                                            }
                                        }
                                        no--;
                                    }
                                    if (ErrorDate != null)
                                    {
                                        ViewData["ErrNegativeDays"] = ErrorDate + Utility.Phrase("ErrDateAssessment");
                                        //return Json(ErrorDate + Utility.Phrase("ErrDateAssessment"));
                                    }
                                    no = days;
                                    while (no >= 0)
                                    {
                                        ScheduleSubmitSettingEx item = new ScheduleSubmitSettingEx();
                                        item.Date = model.FromDate.AddDays(no).Date;
                                        item.EmployeeID = model.EmployeeID;
                                        item.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
                                        item.Note = model.Note;
                                        item.Status = 0;
                                        item.UserLogin = User.Identity.Name;
                                        item.CreatedDate = DateTime.Now;
                                        item.Type = Kind;
                                        item.CloseTime = model.CloseTime;
                                        if (item.Type == "All")
                                        {
                                            ScheduleSubmitSettingEx ins = new ScheduleSubmitSettingEx();
                                            item.Type = "AM";
                                            item.IsDatabase = false;
                                            if (HammerDataProvider.CheckScheduleSubmitSetting(item, item.Type) == 0)
                                            {
                                                List.Add(item);
                                            }
                                            ins = new ScheduleSubmitSettingEx();
                                            ins.Date = model.FromDate.AddDays(no).Date;
                                            ins.EmployeeID = model.EmployeeID;
                                            ins.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
                                            ins.Note = model.Note;
                                            ins.Status = 0;
                                            ins.UserLogin = User.Identity.Name;
                                            ins.CreatedDate = DateTime.Now;
                                            ins.CloseTime = model.CloseTime;
                                            ins.IsDatabase = false;
                                            ins.Type = "PM";
                                            if (HammerDataProvider.CheckScheduleSubmitSetting(ins, ins.Type) == 0)
                                            {
                                                List.Add(ins);
                                            }
                                        }
                                        else
                                        {
                                            if (HammerDataProvider.CheckScheduleSubmitSetting(item, item.Type) == 0)
                                            {
                                                List.Add(item);
                                            }
                                        }

                                        no--;
                                    }
                                }
                                else
                                {
                                    model.EmployeeID = model.EmployeeID;
                                    ViewData["ErrNegativeDays"] = Utility.Phrase("ErrIssisdata");
                                    //return Json(Utility.Phrase("ErrIssisdata"));
                                }
                            }
                        }
                    }
                    else
                    {
                        //return Json(Utility.Phrase("ErrCloseTime"));
                    }
                }
                #endregion
                #region Choose Region
                else
                {
                    List<EmployeeModel> listEmployess = HammerDataProvider.GetAddEmployees(model.regionID, model.areaID);
                    foreach (EmployeeModel line in listEmployess)
                    {
                        string Kind;
                        if (model.CloseTime > 0 && string.IsNullOrEmpty(model.Note) == false)
                        {
                            Kind = model.ScheduleType;
                            int days = model.EndDate.Subtract(model.FromDate).Days;

                            if (days >= 0)
                            {
                                int no = days;
                                //check xem du lieu co ton tai ngay mo ngay ko neu mo ngay bao loi.                           
                                while (no >= 0)
                                {
                                    ScheduleSubmitSettingEx item = new ScheduleSubmitSettingEx();
                                    item.Date = model.FromDate.AddDays(no).Date;
                                    item.EmployeeID = line.EmployeeID;
                                    item.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
                                    item.Note = model.Note;
                                    item.Status = 0;
                                    item.UserLogin = User.Identity.Name;
                                    item.CreatedDate = DateTime.Now;
                                    item.Type = Kind;
                                    item.CloseTime = model.CloseTime;
                                    if (HammerDataProvider.CheckAssessmentDate(item.Date, item.EmployeeID) != 1)
                                    {

                                        if (item.Type == "All")
                                        {
                                            ScheduleSubmitSettingEx ins = new ScheduleSubmitSettingEx();
                                            item.Type = "AM";
                                            item.IsDatabase = false;
                                            if (HammerDataProvider.CheckScheduleSubmitSetting(item, item.Type) == 0)
                                            {
                                                List.Add(item);
                                            }
                                            ins = new ScheduleSubmitSettingEx();
                                            ins.Date = model.FromDate.AddDays(no).Date;
                                            ins.EmployeeID = line.EmployeeID;
                                            ins.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
                                            ins.Note = model.Note;
                                            ins.Status = 0;
                                            ins.UserLogin = User.Identity.Name;
                                            ins.CreatedDate = DateTime.Now;
                                            ins.CloseTime = model.CloseTime;
                                            ins.Type = "PM";
                                            ins.IsDatabase = false;
                                            if (HammerDataProvider.CheckScheduleSubmitSetting(ins, ins.Type) == 0)
                                            {
                                                List.Add(ins);
                                            }
                                        }
                                        else
                                        {
                                            if (HammerDataProvider.CheckScheduleSubmitSetting(item, item.Type) == 0)
                                            {
                                                List.Add(item);
                                            }
                                        }

                                    }
                                    no--;
                                }

                            }
                        }
                    }
                }
                #endregion
                List<ScheduleSubmitSettingEx> InsertList = new List<ScheduleSubmitSettingEx>();
                InsertList = List.OrderByDescending(x => x.EmployeeID).Select((x, index)
                    => new ScheduleSubmitSettingEx
                    {
                        No = index + 1,
                        Date = x.Date,
                        EmployeeID = x.EmployeeID,
                        EmployeeName = x.EmployeeName,
                        Type = x.Type,
                        Note = x.Note,
                        Status = x.Status,
                        UserLogin = x.UserLogin,
                        CloseTime = x.CloseTime,
                        CreatedDate = x.CreatedDate,
                        IsDatabase = x.IsDatabase
                    }).ToList();
                Session["ListDetailOpen"] = InsertList.ToList();
                //
            }
            else
            {
                model. CloseTime = 1;
                model. FromDate = DateTime.Now.Date;
                model.EndDate = DateTime.Now.Date;
                model.ScheduleType = "All";
                //model.sendEmail = true;              
                model.ListType = listItem;
                model.ListRegion = HammerDataProvider.GetRegion();
                model.ListArea = HammerDataProvider.GetAreasWithRegion("");
                Session["ListDetailOpen"] = new List<ScheduleSubmitSettingEx>();
            }
            return View(model);
            //return View(new SendScheduleAgainMode()
            //{
            //    CloseTime = 1,
            //    FromDate = DateTime.Now.Date,
            //    EndDate = DateTime.Now.Date,
            //    ScheduleType = "All",
            //    sendEmail = true,
            //    ListType = listItem,
            //    ListRegion = HammerDataProvider.GetRegion(),
            //    ListArea = HammerDataProvider.GetAreasWithRegion("")

            //});

        }
        public ActionResult GridPartialView()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            List<ScheduleSubmitSettingEx> list = Session["ListDetailOpen"] as List<ScheduleSubmitSettingEx>;
            return PartialView(Session["ListDetailOpen"]);
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
            List<EmployeeModel> listItem = HammerDataProvider.GetAddEmployees(regionID, areaID);
            return PartialView(listItem);
        }
        public ActionResult ComboBoxPartialScheduleType()
        {
            List<ScheduleType> listItem = new List<ScheduleType>();
            ScheduleType ins = new ScheduleType();
            ins.ID = "All";
            ins.Name = "Huấn luyện - Không huấn luyện";
            listItem.Add(ins);
            ins = new ScheduleType();
            ins.ID = "WW";
            ins.Name = "Huấn luyện";
            listItem.Add(ins);
            ins = new ScheduleType();
            ins.ID = "NWW";
            ins.Name = "Không huấn luyện";
            listItem.Add(ins);
            return PartialView(listItem);
        }
        [HttpPost]
        public ActionResult Close()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string notiemail = Request.Params["sendEmail"];
            var listItem = Request.Params["ListItem"];
            List<ScheduleSubmitSettingEx> listDetail = Session["ListDetailOpen"] as List<ScheduleSubmitSettingEx>;
            string[] words = listItem.Split(',');
            foreach (string item in words)
            {
                if (String.IsNullOrEmpty(item))
                    return PartialView("GridPartialView", Session["ListDetailOpen"]);
                int val = int.Parse(item.ToString());
                ScheduleSubmitSettingEx pr = listDetail.Find(f => f.No == val && f.IsDatabase == true);
                if (pr != null)
                {
                    ScheduleSubmitSetting ins = new ScheduleSubmitSetting();
                    ins.Type = pr.Type;
                    ins.EmployeeID = pr.EmployeeID;
                    ins.Date = pr.Date;
                    ins.CloseTime = pr.CloseTime.Value;
                    ins.Note = pr.Note;
                    ins.Status = 1;
                    ins.UserLogin = pr.UserLogin;
                    ins.CreatedDate = DateTime.Now;
                    ins.IsDatabase = true;
                    ins.ID = pr.No;
                    HammerDataProvider.UpdateScheduleSubmitSetting(ins);
                    listDetail.Remove(pr);
                    pr.Status = 1;
                    listDetail.Add(pr);
                    #region SendEmail
                    if (Convert.ToBoolean(notiemail.ToString()) == true)
                    {
                        #region SendEmail
                        if (Convert.ToBoolean(notiemail.ToString()) == true)
                        {
                            string emailTemplate = Server.MapPath(Constants.EmailOpenScheduleHTML);
                            DMSSalesForce employee = HammerDataProvider.GetSalesforceById(pr.EmployeeID);
                            #region sendEmailLevel1
                            List<EmployeeModel> employeeLevel1 = new List<EmployeeModel>();
                            employeeLevel1 = HammerDataProvider.GetMangerEmployee(pr.EmployeeID);
                            if (employeeLevel1.Count() <= 0)
                                Util.InitSendEmailOpenDate(employee.Email, employee.EmployeeName,
                    emailTemplate, ins, ins.Type.Trim(), Util.GetBaseUrl(), "", "", HammerDataProvider.GetSystemSetting("EmailCC").Desr.Trim());
                            try
                            {
                                foreach (EmployeeModel itemNV in employeeLevel1)
                                {
                                    DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(itemNV.EmployeeID);
                                    #region DeleteAssUpTo
                                    List<EmployeeModel> employeeLevel2 = HammerDataProvider.GetMangerEmployee(employeeSend.EmployeeID);
                                    if (employeeLevel2.Count() <= 0)
                                        Util.InitSendEmailOpenDate(employee.Email, employee.EmployeeName,
                            emailTemplate, ins, ins.Type.Trim(), Util.GetBaseUrl(), employeeSend.Email, "", HammerDataProvider.GetSystemSetting("EmailCC").Desr.Trim());
                                    try
                                    {
                                        foreach (EmployeeModel itemlevel2 in employeeLevel2)
                                        {
                                            DMSSalesForce employeeSendLevel2 = HammerDataProvider.GetSalesforceById(itemlevel2.EmployeeID);
                                            #region Send
                                            Util.InitSendEmailOpenDate(employee.Email, employee.EmployeeName,
                                  emailTemplate, ins, ins.Type.Trim(), Util.GetBaseUrl(), employeeSend.Email, employeeSendLevel2.Email, HammerDataProvider.GetSystemSetting("EmailCC").Desr.Trim());
                                            #endregion
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //Log.Error(ex.Message, ex);
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception)
                            {

                            }
                            #endregion

                        }
                        #endregion
                    }
                    else
                    {

                    }
                    #endregion
                }

            }
            List<ScheduleSubmitSettingEx> InsertList = new List<ScheduleSubmitSettingEx>();
            InsertList = listDetail.OrderByDescending(x => x.EmployeeID).Select((x, index)
                => new ScheduleSubmitSettingEx
                {
                    No = index + 1,
                    Date = x.Date,
                    EmployeeID = x.EmployeeID,
                    EmployeeName = x.EmployeeName,
                    Type = x.Type,
                    Note = x.Note,
                    Status = x.Status,
                    UserLogin = x.UserLogin,
                    CloseTime = x.CloseTime,
                    CreatedDate = x.CreatedDate,
                    IsDatabase = x.IsDatabase
                }).ToList();
            Session["ListDetailOpen"] = InsertList.ToList();
            return PartialView("GridPartialView", Session["ListDetailOpen"]);
        }
        [HttpPost]
        public ActionResult Save()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string notiemail = Request.Params["sendEmail"];
            var listItem = Request.Params["ListItem"];
            List<ScheduleSubmitSettingEx> listDetail = Session["ListDetailOpen"] as List<ScheduleSubmitSettingEx>;
            string[] words = listItem.Split(',');
            foreach (string item in words)
            {
                if (String.IsNullOrEmpty(item))
                    return PartialView("GridPartialView", Session["ListDetailOpen"]);
                int val = int.Parse(item.ToString());
                ScheduleSubmitSettingEx pr = listDetail.Find(f => f.No == val && f.IsDatabase == false);
                if (pr != null)
                {
                    ScheduleSubmitSetting ins = new ScheduleSubmitSetting();
                    ins.Type = pr.Type;
                    ins.EmployeeID = pr.EmployeeID;
                    ins.Date = pr.Date;
                    ins.CloseTime = pr.CloseTime.Value;
                    ins.Note = pr.Note;
                    ins.Status = pr.Status.Value;
                    ins.UserLogin = pr.UserLogin;
                    ins.CreatedDate = DateTime.Now;
                    ins.IsDatabase = true;
                    HammerDataProvider.InsertScheduleSubmitSetting(ins);
                    listDetail.Remove(pr);
                    #region deleteAss
                    Appointment rscheck = HammerDataProvider.PreparNewCheckApp(ins.Date.Date, ins.EmployeeID, ins.Type);
                    if (rscheck != null)
                    {
                        NoTrainingAssessment noass = HammerDataProvider.GetNoTrainingAssessment(ins.Date.Date, ins.EmployeeID, rscheck.UniqueID.ToString());
                        if (noass != null)
                        {
                            HammerDataProvider.UpdateNoTraning(noass);
                        }
                        else
                        {
                            SMTrainingAssessmentHeader SMAss = HammerDataProvider.GetSMTrainingAssessment(ins.Date.Date, ins.EmployeeID, rscheck.UniqueID.ToString());
                            if (SMAss != null)
                            {
                                HammerDataProvider.UpdateSMTraning(SMAss);
                            }
                            else
                            {
                                TrainingAssessmentHeader Ass = HammerDataProvider.GetTrainingAssessment(ins.Date.Date, ins.EmployeeID, rscheck.UniqueID.ToString());
                                if (SMAss != null)
                                {
                                    HammerDataProvider.UpdateTraning(Ass);
                                }
                            }

                        }
                        //W
                        if (string.IsNullOrEmpty(rscheck.Employees))
                        {
                            PrepareNonWW updateno = new PrepareNonWW();
                            updateno.EmployeeID = rscheck.UserLogin;
                            CultureInfo ciCurr = CultureInfo.CurrentCulture;
                            int weekNum = ciCurr.Calendar.GetWeekOfYear(pr.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                            updateno.Year = pr.Date.Year.ToString();
                            updateno.Week = weekNum;
                            updateno.Day = pr.Date.DayOfWeek.ToString().Substring(0, 2);
                            updateno.Shift = rscheck.ShiftID;
                            updateno.Status = 'N';
                            HammerDataProvider.UpdateStatusPrepareNoWW(updateno);
                        }
                        HammerDataProvider.UpdateIsDeleteAppoint(rscheck);
                    }
                    #endregion

                    #region SendEmail
                    if (Convert.ToBoolean(notiemail.ToString()) == true)
                    {
                        string emailTemplate = Server.MapPath(Constants.EmailOpenScheduleHTML);
                        DMSSalesForce employee = HammerDataProvider.GetSalesforceById(pr.EmployeeID);

                        #region sendEmailLevel1
                        List<EmployeeModel> employeeLevel1 = new List<EmployeeModel>();
                        employeeLevel1 = HammerDataProvider.GetMangerEmployee(pr.EmployeeID);
                        if (employeeLevel1.Count() <= 0)
                            Util.InitSendEmailOpenDate(employee.Email, employee.EmployeeName,
                emailTemplate, ins, ins.Type.Trim(), Util.GetBaseUrl(), "", "", HammerDataProvider.GetSystemSetting("EmailCC").Desr.Trim());
                        try
                        {
                            foreach (EmployeeModel itemNV in employeeLevel1)
                            {
                                DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(itemNV.EmployeeID);
                               
                                #region deleteAssUp
                                Appointment rschecklevel1 = HammerDataProvider.PreparNewCheckApp(ins.Date.Date, employeeSend.EmployeeID, ins.Type, pr.EmployeeID);
                                if (rschecklevel1 != null)
                                {
                                    #region OpenLevel1
                                    ScheduleSubmitSetting inslevel1 = new ScheduleSubmitSetting();
                                    inslevel1.Type = pr.Type;
                                    inslevel1.EmployeeID = employeeSend.EmployeeID;
                                    inslevel1.Date = pr.Date;
                                    inslevel1.CloseTime = pr.CloseTime.Value;
                                    inslevel1.Note = pr.Note;
                                    inslevel1.Status = pr.Status.Value;
                                    inslevel1.UserLogin = pr.UserLogin;
                                    inslevel1.CreatedDate = DateTime.Now;
                                    inslevel1.IsDatabase = true;
                                    if (HammerDataProvider.CheckOpeningDateEm(inslevel1.Date, inslevel1.EmployeeID) == null)
                                    {
                                        HammerDataProvider.InsertScheduleSubmitSetting(inslevel1);
                                    }
                                    #endregion
                                    NoTrainingAssessment noass = HammerDataProvider.GetNoTrainingAssessment(ins.Date.Date, employeeSend.EmployeeID, rschecklevel1.UniqueID.ToString());
                                    if (noass != null)
                                    {
                                        HammerDataProvider.UpdateNoTraning(noass);
                                    }
                                    else
                                    {
                                        SMTrainingAssessmentHeader SMAss = HammerDataProvider.GetSMTrainingAssessment(ins.Date.Date, employeeSend.EmployeeID, rschecklevel1.UniqueID.ToString());
                                        if (SMAss != null)
                                        {
                                            HammerDataProvider.UpdateSMTraning(SMAss);
                                        }
                                        else
                                        {
                                            TrainingAssessmentHeader Ass = HammerDataProvider.GetTrainingAssessment(ins.Date.Date, employeeSend.EmployeeID, rschecklevel1.UniqueID.ToString());
                                            if (SMAss != null)
                                            {
                                                HammerDataProvider.UpdateTraning(Ass);
                                            }
                                        }

                                    }
                                    //W
                                    if (string.IsNullOrEmpty(rschecklevel1.Employees))
                                    {
                                        PrepareNonWW updateno = new PrepareNonWW();
                                        updateno.EmployeeID = rscheck.UserLogin;
                                        CultureInfo ciCurr = CultureInfo.CurrentCulture;
                                        int weekNum = ciCurr.Calendar.GetWeekOfYear(pr.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                                        updateno.Year = pr.Date.Year.ToString();
                                        updateno.Week = weekNum;
                                        updateno.Day = pr.Date.DayOfWeek.ToString().Substring(0, 2);
                                        updateno.Shift = rschecklevel1.ShiftID;
                                        updateno.Status = 'N';
                                        HammerDataProvider.UpdateStatusPrepareNoWW(updateno);
                                    }
                                    HammerDataProvider.UpdateIsDeleteAppoint(rschecklevel1);
                                }
                                #endregion
                                #region DeleteAssUpTo
                                List<EmployeeModel> employeeLevel2 = HammerDataProvider.GetMangerEmployee(employeeSend.EmployeeID);
                                if (employeeLevel2.Count() <= 0)
                                    Util.InitSendEmailOpenDate(employee.Email, employee.EmployeeName,
                        emailTemplate, ins, ins.Type.Trim(), Util.GetBaseUrl(), employeeSend.Email, "", HammerDataProvider.GetSystemSetting("EmailCC").Desr.Trim());
                                try
                                {
                                    foreach (EmployeeModel itemlevel2 in employeeLevel2)
                                    {
                                        DMSSalesForce employeeSendLevel2 = HammerDataProvider.GetSalesforceById(itemlevel2.EmployeeID);
                                        
                                        Appointment rschecklevel2 = HammerDataProvider.PreparNewCheckApp(ins.Date.Date, itemlevel2.EmployeeID, ins.Type, employeeSend.EmployeeID);
                                        if (rschecklevel2 != null)
                                        {
                                            #region OpenLevel2
                                            ScheduleSubmitSetting inslevel2 = new ScheduleSubmitSetting();
                                            inslevel2.Type = pr.Type;
                                            inslevel2.EmployeeID = employeeSendLevel2.EmployeeID;
                                            inslevel2.Date = pr.Date;
                                            inslevel2.CloseTime = pr.CloseTime.Value;
                                            inslevel2.Note = pr.Note;
                                            inslevel2.Status = pr.Status.Value;
                                            inslevel2.UserLogin = pr.UserLogin;
                                            inslevel2.CreatedDate = DateTime.Now;
                                            inslevel2.IsDatabase = true;
                                            if (HammerDataProvider.CheckOpeningDateEm(inslevel2.Date, inslevel2.EmployeeID) == null)
                                            {
                                                HammerDataProvider.InsertScheduleSubmitSetting(inslevel2);
                                            }
                                            #endregion
                                            NoTrainingAssessment noass = HammerDataProvider.GetNoTrainingAssessment(ins.Date.Date, itemlevel2.EmployeeID, rschecklevel2.UniqueID.ToString());
                                            if (noass != null)
                                            {
                                                HammerDataProvider.UpdateNoTraning(noass);
                                            }
                                            else
                                            {
                                                SMTrainingAssessmentHeader SMAss = HammerDataProvider.GetSMTrainingAssessment(ins.Date.Date, itemlevel2.EmployeeID, rschecklevel2.UniqueID.ToString());
                                                if (SMAss != null)
                                                {
                                                    HammerDataProvider.UpdateSMTraning(SMAss);
                                                }
                                                else
                                                {
                                                    TrainingAssessmentHeader Ass = HammerDataProvider.GetTrainingAssessment(ins.Date.Date, itemlevel2.EmployeeID, rschecklevel2.UniqueID.ToString());
                                                    if (SMAss != null)
                                                    {
                                                        HammerDataProvider.UpdateTraning(Ass);
                                                    }
                                                }
                                            }
                                            //W
                                            if (string.IsNullOrEmpty(rschecklevel2.Employees))
                                            {
                                                PrepareNonWW updateno = new PrepareNonWW();
                                                updateno.EmployeeID = rscheck.UserLogin;
                                                CultureInfo ciCurr = CultureInfo.CurrentCulture;
                                                int weekNum = ciCurr.Calendar.GetWeekOfYear(pr.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                                                updateno.Year = pr.Date.Year.ToString();
                                                updateno.Week = weekNum;
                                                updateno.Day = pr.Date.DayOfWeek.ToString().Substring(0, 2);
                                                updateno.Shift = rschecklevel2.ShiftID;
                                                updateno.Status = 'N';
                                                HammerDataProvider.UpdateStatusPrepareNoWW(updateno);
                                            }
                                            HammerDataProvider.UpdateIsDeleteAppoint(rschecklevel2);
                                        }
                                        #region Send
                                        Util.InitSendEmailOpenDate(employee.Email, employee.EmployeeName,
                              emailTemplate, ins, ins.Type.Trim(), Util.GetBaseUrl(), employeeSend.Email, employeeSendLevel2.Email, HammerDataProvider.GetSystemSetting("EmailCC").Desr.Trim());
                                        #endregion
                                    }

                                }
                                catch (Exception)
                                {
                                    //Log.Error(ex.Message, ex);
                                }

                                #endregion


                            }
                        }
                        catch (Exception)
                        {

                        }
                        #endregion
                    }
                    else // khong check xoa lich thoi
                    {
                        DMSSalesForce employee = HammerDataProvider.GetSalesforceById(pr.EmployeeID);
                        #region delelte
                        List<EmployeeModel> employeeLevel1 = new List<EmployeeModel>();
                        employeeLevel1 = HammerDataProvider.GetMangerEmployee(pr.EmployeeID);
                        try
                        {
                            foreach (EmployeeModel itemNV in employeeLevel1)
                            {
                                DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(itemNV.EmployeeID);
                                
                                #region deleteAssUp
                                Appointment rschecklevel1 = HammerDataProvider.PreparNewCheckApp(ins.Date.Date, employeeSend.EmployeeID, ins.Type, pr.EmployeeID);
                                if (rschecklevel1 != null)
                                {
                                    #region OpenLevel1
                                    ScheduleSubmitSetting inslevel1 = new ScheduleSubmitSetting();
                                    inslevel1.Type = pr.Type;
                                    inslevel1.EmployeeID = employeeSend.EmployeeID;
                                    inslevel1.Date = pr.Date;
                                    inslevel1.CloseTime = pr.CloseTime.Value;
                                    inslevel1.Note = pr.Note;
                                    inslevel1.Status = pr.Status.Value;
                                    inslevel1.UserLogin = pr.UserLogin;
                                    inslevel1.CreatedDate = DateTime.Now;
                                    inslevel1.IsDatabase = true;
                                    if (HammerDataProvider.CheckOpeningDateEm(inslevel1.Date, inslevel1.EmployeeID) == null)
                                    {
                                        HammerDataProvider.InsertScheduleSubmitSetting(inslevel1);
                                    }
                                    #endregion
                                    NoTrainingAssessment noass = HammerDataProvider.GetNoTrainingAssessment(ins.Date.Date, employeeSend.EmployeeID, rschecklevel1.UniqueID.ToString());
                                    if (noass != null)
                                    {
                                        HammerDataProvider.UpdateNoTraning(noass);
                                    }
                                    else
                                    {
                                        SMTrainingAssessmentHeader SMAss = HammerDataProvider.GetSMTrainingAssessment(ins.Date.Date, employeeSend.EmployeeID, rschecklevel1.UniqueID.ToString());
                                        if (SMAss != null)
                                        {
                                            HammerDataProvider.UpdateSMTraning(SMAss);
                                        }
                                        else
                                        {
                                            TrainingAssessmentHeader Ass = HammerDataProvider.GetTrainingAssessment(ins.Date.Date, employeeSend.EmployeeID, rschecklevel1.UniqueID.ToString());
                                            if (SMAss != null)
                                            {
                                                HammerDataProvider.UpdateTraning(Ass);
                                            }
                                        }

                                    }
                                    //W
                                    if (string.IsNullOrEmpty(rschecklevel1.Employees))
                                    {
                                        PrepareNonWW updateno = new PrepareNonWW();
                                        updateno.EmployeeID = rscheck.UserLogin;
                                        CultureInfo ciCurr = CultureInfo.CurrentCulture;
                                        int weekNum = ciCurr.Calendar.GetWeekOfYear(pr.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                                        updateno.Year = pr.Date.Year.ToString();
                                        updateno.Week = weekNum;
                                        updateno.Day = pr.Date.DayOfWeek.ToString().Substring(0, 2);
                                        updateno.Shift = rschecklevel1.ShiftID;
                                        updateno.Status = 'N';
                                        HammerDataProvider.UpdateStatusPrepareNoWW(updateno);
                                    }
                                    HammerDataProvider.UpdateIsDeleteAppoint(rschecklevel1);
                                }
                                #endregion
                                #region DeleteAssUpTo
                                List<EmployeeModel> employeeLevel2 = HammerDataProvider.GetMangerEmployee(employeeSend.EmployeeID);
                                try
                                {
                                    foreach (EmployeeModel itemlevel2 in employeeLevel2)
                                    {
                                        DMSSalesForce employeeSendLevel2 = HammerDataProvider.GetSalesforceById(itemlevel2.EmployeeID);
                                        
                                        Appointment rschecklevel2 = HammerDataProvider.PreparNewCheckApp(ins.Date.Date, itemlevel2.EmployeeID, ins.Type, employeeSend.EmployeeID);
                                        if (rschecklevel2 != null)
                                        {
                                            #region OpenLevel2
                                            ScheduleSubmitSetting inslevel2 = new ScheduleSubmitSetting();
                                            inslevel2.Type = pr.Type;
                                            inslevel2.EmployeeID = employeeSendLevel2.EmployeeID;
                                            inslevel2.Date = pr.Date;
                                            inslevel2.CloseTime = pr.CloseTime.Value;
                                            inslevel2.Note = pr.Note;
                                            inslevel2.Status = pr.Status.Value;
                                            inslevel2.UserLogin = pr.UserLogin;
                                            inslevel2.CreatedDate = DateTime.Now;
                                            inslevel2.IsDatabase = true;
                                            if (HammerDataProvider.CheckOpeningDateEm(inslevel2.Date, inslevel2.EmployeeID) == null)
                                            {
                                                HammerDataProvider.InsertScheduleSubmitSetting(inslevel2);
                                            }
                                            #endregion
                                            NoTrainingAssessment noass = HammerDataProvider.GetNoTrainingAssessment(ins.Date.Date, itemlevel2.EmployeeID, rschecklevel2.UniqueID.ToString());
                                            if (noass != null)
                                            {
                                                HammerDataProvider.UpdateNoTraning(noass);
                                            }
                                            else
                                            {
                                                SMTrainingAssessmentHeader SMAss = HammerDataProvider.GetSMTrainingAssessment(ins.Date.Date, itemlevel2.EmployeeID, rschecklevel2.UniqueID.ToString());
                                                if (SMAss != null)
                                                {
                                                    HammerDataProvider.UpdateSMTraning(SMAss);
                                                }
                                                else
                                                {
                                                    TrainingAssessmentHeader Ass = HammerDataProvider.GetTrainingAssessment(ins.Date.Date, itemlevel2.EmployeeID, rschecklevel2.UniqueID.ToString());
                                                    if (SMAss != null)
                                                    {
                                                        HammerDataProvider.UpdateTraning(Ass);
                                                    }
                                                }
                                            }
                                            //W
                                            if (string.IsNullOrEmpty(rschecklevel2.Employees))
                                            {
                                                PrepareNonWW updateno = new PrepareNonWW();
                                                updateno.EmployeeID = rscheck.UserLogin;
                                                CultureInfo ciCurr = CultureInfo.CurrentCulture;
                                                int weekNum = ciCurr.Calendar.GetWeekOfYear(pr.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                                                updateno.Year = pr.Date.Year.ToString();
                                                updateno.Week = weekNum;
                                                updateno.Day = pr.Date.DayOfWeek.ToString().Substring(0, 2);
                                                updateno.Shift = rschecklevel2.ShiftID;
                                                updateno.Status = 'N';
                                                HammerDataProvider.UpdateStatusPrepareNoWW(updateno);
                                            }
                                            HammerDataProvider.UpdateIsDeleteAppoint(rschecklevel2);
                                        }
                                    }

                                }
                                catch (Exception)
                                {
                                    //Log.Error(ex.Message, ex);
                                }
                                #endregion
                            }
                        }
                        catch (Exception)
                        {

                        }
                        #endregion
                    }
                    #endregion
                }

            }
            List<ScheduleSubmitSettingEx> InsertList = new List<ScheduleSubmitSettingEx>();
            InsertList = listDetail.OrderByDescending(x => x.EmployeeID).Select((x, index)
                => new ScheduleSubmitSettingEx
                {
                    No = index + 1,
                    Date = x.Date,
                    EmployeeID = x.EmployeeID,
                    EmployeeName = x.EmployeeName,
                    Type = x.Type,
                    Note = x.Note,
                    Status = x.Status,
                    UserLogin = x.UserLogin,
                    CloseTime = x.CloseTime,
                    CreatedDate = x.CreatedDate,
                    IsDatabase = x.IsDatabase
                }).ToList();
            Session["ListDetailOpen"] = InsertList.ToList();
            return PartialView("GridPartialView", Session["ListDetailOpen"]);
        }
        [HttpPost]
        public ActionResult ViewDetail()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            Session["ListDetailOpen"] = null;
            List<ScheduleSubmitSettingEx> listDetail = new List<ScheduleSubmitSettingEx>();
            string scheduleType = Request.Params["scheduleType"];
            string regionID = Request.Params["regionID"];
            if (regionID == "null")
            {
                regionID = null;
                return Json("Khu vực không được phép trống!");
            }
            string areaID = Request.Params["areaID"];
            if (areaID == "null")
                areaID = null;
            string EmployeeID = Request.Params["employees"];
            if (EmployeeID == "null")
                EmployeeID = null;
            var date = Request.Params["start"];
            DateTimeFormatInfo ukDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            DateTime start = Convert.ToDateTime(date, ukDtfi);
            date = Request.Params["end"];
            DateTime end = Convert.ToDateTime(date, ukDtfi);

            if (!String.IsNullOrEmpty(regionID) && (!String.IsNullOrEmpty(areaID) || String.IsNullOrEmpty(areaID)) && !String.IsNullOrEmpty(EmployeeID))
            {
                List<ScheduleSubmitSetting> rs = HammerDataProvider.GetScheduleSubmitSetting(scheduleType, EmployeeID, start, end);
                foreach (ScheduleSubmitSetting line in rs)
                {
                    ScheduleSubmitSettingEx item = new ScheduleSubmitSettingEx();
                    item.Date = line.Date.Date;
                    item.EmployeeID = line.EmployeeID;
                    item.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
                    item.Note = line.Note;
                    item.Status = line.Status;
                    item.UserLogin = line.UserLogin;
                    item.CreatedDate = line.CreatedDate;
                    item.Type = line.Type;
                    item.CloseTime = line.CloseTime;
                    item.No = line.ID;
                    item.IsDatabase = line.IsDatabase;
                    listDetail.Add(item);
                }
            }
            else
            {
                List<EmployeeModel> listEmployess = HammerDataProvider.GetAddEmployees(regionID, areaID);
                foreach (EmployeeModel row in listEmployess)
                {
                    List<ScheduleSubmitSetting> rs = HammerDataProvider.GetScheduleSubmitSetting(scheduleType, row.EmployeeID, start, end);
                    foreach (ScheduleSubmitSetting line in rs)
                    {
                        ScheduleSubmitSettingEx item = new ScheduleSubmitSettingEx();
                        item.Date = line.Date.Date;
                        item.EmployeeID = line.EmployeeID;
                        item.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
                        item.Note = line.Note;
                        item.Status = line.Status;
                        item.UserLogin = line.UserLogin;
                        item.CreatedDate = line.CreatedDate;
                        item.Type = line.Type;
                        item.CloseTime = line.CloseTime;
                        item.No = line.ID;
                        item.IsDatabase = line.IsDatabase;
                        listDetail.Add(item);
                    }
                }
            }
            Session["ListDetailOpen"] = listDetail;
            return PartialView("GridPartialView", Session["ListDetailOpen"]);
        }
        //[HttpPost]
        //public ActionResult Add(SendScheduleAgainMode model)
        //{
        //    Session["ListDetailOpen"] = null;
        //    List<ScheduleSubmitSettingEx> List = new List<ScheduleSubmitSettingEx>();
        //    HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
        //    if (String.IsNullOrEmpty(model.Note))
        //        return Json("Vui lòng điền đầy đủ thông tin trên màn hình!");
        //    #region Full filter
        //    if (!String.IsNullOrEmpty(model.regionID) && (!String.IsNullOrEmpty(model.areaID) || String.IsNullOrEmpty(model.areaID)) && !String.IsNullOrEmpty(model.EmployeeID))
        //    {
        //        string Kind;
        //        if (model.CloseTime > 0 && string.IsNullOrEmpty(model.Note) == false)
        //        {
        //            Kind = model.ScheduleType;
        //            int days = model.EndDate.Subtract(model.FromDate).Days;
        //            if (days < 0)
        //            {
        //                model.EmployeeID = model.EmployeeID;
        //                ViewData["ErrNegativeDays"] = Utility.Phrase("ErrEndDateGreaterThanEqualFromDate");
        //            }
        //            else
        //            {
        //                if (model.EmployeeID != null)
        //                {
        //                    bool FlagEror = false;
        //                    ViewData["ErrNegativeDays"] = null;
        //                    int no = days;
        //                    if (FlagEror == false)
        //                    {
        //                        //check xem du lieu co ton tai ngay mo ngay ko neu mo ngay bao loi.
        //                        string ErrorDate = null;
        //                        while (no >= 0)
        //                        {
        //                            ScheduleSubmitSettingEx item = new ScheduleSubmitSettingEx();

        //                            item.Date = model.FromDate.AddDays(no).Date;
        //                            item.EmployeeID = model.EmployeeID;
        //                            item.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
        //                            item.Note = model.Note;
        //                            item.Status = 0;
        //                            item.UserLogin = User.Identity.Name;
        //                            item.CreatedDate = DateTime.Now;
        //                            item.Type = Kind;
        //                            item.CloseTime = model.CloseTime;
        //                            item.IsDatabase = false;
        //                            if (HammerDataProvider.CheckAssessmentDate(item.Date, item.EmployeeID) == 1)
        //                            {
        //                                if (ErrorDate == null)
        //                                {
        //                                    ErrorDate = item.Date.Date.ToString("dd/MM/yyyy");
        //                                }
        //                                else
        //                                {
        //                                    ErrorDate = ErrorDate + ',' + item.Date.Date.ToString("dd/MM/yyyy");
        //                                }
        //                            }
        //                            no--;
        //                        }
        //                        if (ErrorDate != null)
        //                        {
        //                            ViewData["ErrNegativeDays"] = ErrorDate + Utility.Phrase("ErrDateAssessment");
        //                            return Json(ErrorDate + Utility.Phrase("ErrDateAssessment"));
        //                        }
        //                        no = days;
        //                        while (no >= 0)
        //                        {
        //                            ScheduleSubmitSettingEx item = new ScheduleSubmitSettingEx();
        //                            item.Date = model.FromDate.AddDays(no).Date;
        //                            item.EmployeeID = model.EmployeeID;
        //                            item.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
        //                            item.Note = model.Note;
        //                            item.Status = 0;
        //                            item.UserLogin = User.Identity.Name;
        //                            item.CreatedDate = DateTime.Now;
        //                            item.Type = Kind;
        //                            item.CloseTime = model.CloseTime;
        //                            if (item.Type == "All")
        //                            {
        //                                ScheduleSubmitSettingEx ins = new ScheduleSubmitSettingEx();
        //                                item.Type = "AM";
        //                                item.IsDatabase = false;
        //                                if (HammerDataProvider.CheckScheduleSubmitSetting(item, item.Type) == 0)
        //                                {
        //                                    List.Add(item);
        //                                }
        //                                ins = new ScheduleSubmitSettingEx();
        //                                ins.Date = model.FromDate.AddDays(no).Date;
        //                                ins.EmployeeID = model.EmployeeID;
        //                                ins.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
        //                                ins.Note = model.Note;
        //                                ins.Status = 0;
        //                                ins.UserLogin = User.Identity.Name;
        //                                ins.CreatedDate = DateTime.Now;
        //                                ins.CloseTime = model.CloseTime;
        //                                ins.IsDatabase = false;
        //                                ins.Type = "PM";
        //                                if (HammerDataProvider.CheckScheduleSubmitSetting(ins, ins.Type) == 0)
        //                                {
        //                                    List.Add(ins);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (HammerDataProvider.CheckScheduleSubmitSetting(item, item.Type) == 0)
        //                                {
        //                                    List.Add(item);
        //                                }
        //                            }

        //                            no--;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        model.EmployeeID = model.EmployeeID;
        //                        ViewData["ErrNegativeDays"] = Utility.Phrase("ErrIssisdata");
        //                        return Json(Utility.Phrase("ErrIssisdata"));
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            return Json(Utility.Phrase("ErrCloseTime"));
        //        }
        //    }
        //    #endregion
        //    #region Choose Region
        //    else
        //    {
        //        List<EmployeeModel> listEmployess = HammerDataProvider.GetAddEmployees(model.regionID, model.areaID);
        //        foreach (EmployeeModel line in listEmployess)
        //        {
        //            string Kind;
        //            if (model.CloseTime > 0 && string.IsNullOrEmpty(model.Note) == false)
        //            {
        //                Kind = model.ScheduleType;
        //                int days = model.EndDate.Subtract(model.FromDate).Days;

        //                if (days >= 0)
        //                {
        //                    int no = days;
        //                    //check xem du lieu co ton tai ngay mo ngay ko neu mo ngay bao loi.                           
        //                    while (no >= 0)
        //                    {
        //                        ScheduleSubmitSettingEx item = new ScheduleSubmitSettingEx();
        //                        item.Date = model.FromDate.AddDays(no).Date;
        //                        item.EmployeeID = line.EmployeeID;
        //                        item.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
        //                        item.Note = model.Note;
        //                        item.Status = 0;
        //                        item.UserLogin = User.Identity.Name;
        //                        item.CreatedDate = DateTime.Now;
        //                        item.Type = Kind;
        //                        item.CloseTime = model.CloseTime;
        //                        if (HammerDataProvider.CheckAssessmentDate(item.Date, item.EmployeeID) != 1)
        //                        {

        //                            if (item.Type == "All")
        //                            {
        //                                ScheduleSubmitSettingEx ins = new ScheduleSubmitSettingEx();
        //                                item.Type = "AM";
        //                                item.IsDatabase = false;
        //                                if (HammerDataProvider.CheckScheduleSubmitSetting(item, item.Type) == 0)
        //                                {
        //                                    List.Add(item);
        //                                }
        //                                ins = new ScheduleSubmitSettingEx();
        //                                ins.Date = model.FromDate.AddDays(no).Date;
        //                                ins.EmployeeID = line.EmployeeID;
        //                                ins.EmployeeName = HammerDataProvider.GetNameEmployee(item.EmployeeID);
        //                                ins.Note = model.Note;
        //                                ins.Status = 0;
        //                                ins.UserLogin = User.Identity.Name;
        //                                ins.CreatedDate = DateTime.Now;
        //                                ins.CloseTime = model.CloseTime;
        //                                ins.Type = "PM";
        //                                ins.IsDatabase = false;
        //                                if (HammerDataProvider.CheckScheduleSubmitSetting(ins, ins.Type) == 0)
        //                                {
        //                                    List.Add(ins);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (HammerDataProvider.CheckScheduleSubmitSetting(item, item.Type) == 0)
        //                                {
        //                                    List.Add(item);
        //                                }
        //                            }

        //                        }
        //                        no--;
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    #endregion
        //    List<ScheduleSubmitSettingEx> InsertList = new List<ScheduleSubmitSettingEx>();
        //    InsertList = List.OrderByDescending(x => x.EmployeeID).Select((x, index)
        //        => new ScheduleSubmitSettingEx
        //        {
        //            No = index + 1,
        //            Date = x.Date,
        //            EmployeeID = x.EmployeeID,
        //            EmployeeName = x.EmployeeName,
        //            Type = x.Type,
        //            Note = x.Note,
        //            Status = x.Status,
        //            UserLogin = x.UserLogin,
        //            CloseTime = x.CloseTime,
        //            CreatedDate = x.CreatedDate,
        //            IsDatabase = x.IsDatabase
        //        }).ToList();
        //    Session["ListDetailOpen"] = InsertList.ToList();
        //    return Json("Thêm dữ liệu thành công");
        //}
        public FileResult ExportExcel()
        {
            List<ScheduleSubmitSettingEx> list = new List<ScheduleSubmitSettingEx>();
            if (Session["ListDetailOpen"] != null)
            {
                list = Session["ListDetailOpen"] as List<ScheduleSubmitSettingEx>;
                list = list.OrderBy(x => x.Date).ThenBy(z => z.EmployeeID).ToList();
            }
            else
            {
                list = new List<ScheduleSubmitSettingEx>();
            }
            string templatePath = Server.MapPath("/Templates/Report/") + "ReportOpenDate.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string filename = DateTime.Now.ToString("yyyyMMddhhmm") + "_DanhSachMoNgay.xlsx";


            Byte[] fileBytes = Util.ExportExcelReportOpenDate(list, templatePath);
            FileResult result = File(fileBytes, contentType, filename);

            return result;
        }
    }
}
