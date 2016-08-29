using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammer.Models;
using System.Globalization;
using Hammer.Helpers;
using WebMatrix.WebData;
//using DevExpress.Web.ASPxImageSlider;
using eRoute.Models.eCalendar;
using eRoute.Filters;
using eRoute.Models.eCalendar;
using eRoute;

namespace Hammer.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class MySchedulerController : Controller
    {
        [Authorize]
        [ActionAuthorize("eCalendar_MyScheduler", true)]
        public ActionResult Index()
        {
            int id = WebSecurity.GetUserId(User.Identity.Name);
            HammerDataProvider.ActionSaveLog(id);
            if (Session["Home_ScheduleType"] == null)
            {
                Session["Home_ScheduleType"] = "D";
            }
            return View();
        }

        [HttpPost]
        public ActionResult GetCurrentUICulture()
        {
            return Content(((CultureInfo)this.Session["Culture"]).Name);
        }

        public ActionResult ApproveSchedulePartialView()
        {
            int id = WebSecurity.GetUserId(User.Identity.Name);
            HammerDataProvider.ActionSaveLog(id);
            return PartialView("ApproveSchedulePartialView", HammerDataProvider.GetApproveScheduleDataObject(
                    User.Identity.Name,
                    Session["Home_ScheduleType"] == null ? "M" : Session["Home_ScheduleType"].ToString()));
        }

        public ActionResult DetailAppointmentPartial(DateTime date, string employeeID)
        {
            int id = WebSecurity.GetUserId(User.Identity.Name);
            HammerDataProvider.ActionSaveLog(id);
            return PartialView(eRoute.Models.eCalendar.HammerDataProvider.GetDetailAppointments(date, employeeID));
        }

        [HttpPost]
        public ActionResult ScheduleTypeChanged()
        {
            int id = WebSecurity.GetUserId(User.Identity.Name);
            HammerDataProvider.ActionSaveLog(id);
            string scheduleType = Request.Params["scheduleType"];
            Session["Home_ScheduleType"] = scheduleType;
            return Json(scheduleType);
        }

        [HttpPost]
        public ActionResult RequestDelete()
        {
            int id = WebSecurity.GetUserId(User.Identity.Name);
            HammerDataProvider.ActionSaveLog(id);
            string appointmentId = Request.Params["appointmentId"];
            int appId = 0;
            if (Int32.TryParse(appointmentId, out appId))
            {
                Appointment line = HammerDataProvider.GetAppointmentById(appId);
                DateTime systemDate = DateTime.Now.Date;
                int kq = DateTime.Compare(line.StartDate.Value.Date, systemDate);
                if (kq <= 0)// less today.
                {
                    int rs1 = HammerDataProvider.CheckDateInScheduleSubmitSetting(line.StartDate.Value.Date, line.UserLogin, line.ScheduleType);
                    if (rs1 == 1)
                    {
                        return Json(appId);
                    }
                    else
                    {
                        bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                        if (hasAssessment == true)
                        {
                            return Json(appId);
                        }
                        else
                        {
                            HammerDataProvider.RequestDelete(appId);
                            //send email
                            DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                            EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                            //EmployeeModel employeeSub = new EmployeeModel();
                            string links = "ApproveSchedule";
                            string action = "thay đổi";
                            string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                            List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                            employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                            try
                            {
                                Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                foreach (EmployeeModel item in employeeSub)
                                {
                                    DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                    Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                    employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
                else // lon hon today.
                {
                    int rs1 = HammerDataProvider.CheckDateInScheduleSubmitSetting(line.StartDate.Value.Date, line.UserLogin, line.ScheduleType);
                    if (rs1 == 1)
                    {
                        int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
                        if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
                        {// Thoi diem config.
                            int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                            int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                            if (NoIntTime >= NoTime) // Sau Time
                            {
                                //Lay danh sach cac ngay cua tuan sau de khoa:
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                                DateTime MinnextWeek = nextWeek.Min();
                                if (line.StartDate.Value.Date >= MinnextWeek.Date)
                                {
                                    bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                                    if (hasAssessment == true)
                                    {
                                        return Json(appId);
                                    }
                                    else
                                    {
                                        HammerDataProvider.RequestDelete(appId);
                                        //send email
                                        DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                                        EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                                        //EmployeeModel employeeSub = new EmployeeModel();
                                        string links = "ApproveSchedule";
                                        string action = "thay đổi";
                                        string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                                        List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                                        employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                                        try
                                        {
                                            Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                            foreach (EmployeeModel item in employeeSub)
                                            {
                                                DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                                Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                                employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                            }
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                }
                                else
                                {
                                    return Json(appId);
                                }
                            }
                            else // nho hon h cai dat
                            {
                                //Lay danh sach cac ngay cua tuan sau de khoa:
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                DateTime MinnextWeek = nextWeek.Min();
                                if (line.StartDate.Value.Date >= MinnextWeek.Date)
                                {
                                    bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                                    if (hasAssessment == true)
                                    {
                                        return Json(appId);
                                    }
                                    else
                                    {
                                        HammerDataProvider.RequestDelete(appId);
                                        //send email
                                        DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                                        EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                                        //EmployeeModel employeeSub = new EmployeeModel();
                                        string links = "ApproveSchedule";
                                        string action = "thay đổi";
                                        string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                                        List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                                        employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                                        try
                                        {
                                            Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                            foreach (EmployeeModel item in employeeSub)
                                            {
                                                DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                                Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                                employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                            }
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                }
                                else
                                {
                                    return Json(appId);
                                }
                            }

                        }
                        else // nho hon Ngay cf
                        {
                            //Lay danh sach cac ngay cua tuan sau de khoa:
                            List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                            DateTime MinnextWeek = nextWeek.Min();
                            if (line.StartDate.Value.Date >= MinnextWeek.Date)
                            {
                                bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                                if (hasAssessment == true)
                                {
                                    return Json(appId);
                                }
                                else
                                {
                                    HammerDataProvider.RequestDelete(appId);
                                    //send email
                                    DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                                    EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                                    //EmployeeModel employeeSub = new EmployeeModel();
                                    string links = "ApproveSchedule";
                                    string action = "thay đổi";
                                    string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                                    List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                                    employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                                    try
                                    {
                                        Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                        foreach (EmployeeModel item in employeeSub)
                                        {
                                            DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                            Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                            employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }
                            else
                            {
                                return Json(appId);
                            }
                        }
                    }
                    else // Mo ngay tuong lai
                    {
                        bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                        if (hasAssessment == true)
                        {
                            return Json(appId);
                        }
                        else
                        {
                            HammerDataProvider.RequestDelete(appId);
                            //send email
                            DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                            EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                            //EmployeeModel employeeSub = new EmployeeModel();
                            string links = "ApproveSchedule";
                            string action = "thay đổi";
                            string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                            List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                            employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                            try
                            {
                                Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                foreach (EmployeeModel item in employeeSub)
                                {
                                    DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                    Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                    employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }
            }
            return Json(appId);
        }

        [HttpPost]
        public ActionResult CancelRequestDelete()
        {
            int id = WebSecurity.GetUserId(User.Identity.Name);
            HammerDataProvider.ActionSaveLog(id);
            string appointmentId = Request.Params["appointmentId"];
            int appId = 0;
            if (Int32.TryParse(appointmentId, out appId))
            {
                Appointment line = HammerDataProvider.GetAppointmentById(appId);
                DateTime systemDate = DateTime.Now.Date;
                int kq = DateTime.Compare(line.StartDate.Value.Date, systemDate);
                if (kq <= 0)// less today.
                {
                    int rs1 = HammerDataProvider.CheckDateInScheduleSubmitSetting(line.StartDate.Value.Date, line.UserLogin, line.ScheduleType);
                    if (rs1 == 1)
                    {
                        return Json(appId);
                    }
                    else
                    {
                        bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                        if (hasAssessment == true)
                        {
                            return Json(appId);
                        }
                        else
                        {
                            HammerDataProvider.CancelRequestDelete(appId);
                            //send email
                            DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                            EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                            string links = "ApproveSchedule";
                            string action = "hủy thay đổi";
                            string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                            List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                            employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                            try
                            {
                                Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                foreach (EmployeeModel item in employeeSub)
                                {
                                    DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                    Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                    employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                else // tuong lai
                {
                    int rs1 = HammerDataProvider.CheckDateInScheduleSubmitSetting(line.StartDate.Value.Date, line.UserLogin, line.ScheduleType);
                    if (rs1 == 1)
                    {
                        int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
                        if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
                        {// Thoi diem config.
                            int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                            int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                            if (NoIntTime >= NoTime) // Sau Time
                            {
                                //Lay danh sach cac ngay cua tuan sau de khoa:
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                                DateTime MinnextWeek = nextWeek.Min();
                                if (line.StartDate.Value.Date >= MinnextWeek.Date)
                                {
                                    bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                                    if (hasAssessment == true)
                                    {
                                        return Json(appId);
                                    }
                                    else
                                    {
                                        HammerDataProvider.CancelRequestDelete(appId);
                                        //send email
                                        DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                                        EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                                        string links = "ApproveSchedule";
                                        string action = "hủy thay đổi";
                                        string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                                        List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                                        employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                                        try
                                        {
                                            Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                            foreach (EmployeeModel item in employeeSub)
                                            {
                                                DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                                Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                                employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                            }
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Lay danh sach cac ngay cua tuan sau de khoa:
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                DateTime MinnextWeek = nextWeek.Min();
                                if (line.StartDate.Value.Date >= MinnextWeek.Date)
                                {
                                    bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                                    if (hasAssessment == true)
                                    {
                                        return Json(appId);
                                    }
                                    else
                                    {
                                        HammerDataProvider.CancelRequestDelete(appId);
                                        //send email
                                        DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                                        EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                                        string links = "ApproveSchedule";
                                        string action = "hủy thay đổi";
                                        string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                                        List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                                        employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                                        try
                                        {
                                            Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                            foreach (EmployeeModel item in employeeSub)
                                            {
                                                DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                                Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                                employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                            }
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }
                            }
                        }
                        else // NHO HON NGAY CF
                        {
                            List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                            DateTime MinnextWeek = nextWeek.Min();
                            if (line.StartDate.Value.Date >= MinnextWeek.Date)
                            {
                                bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                                if (hasAssessment == true)
                                {
                                    return Json(appId);
                                }
                                else
                                {
                                    HammerDataProvider.CancelRequestDelete(appId);
                                    //send email
                                    DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                                    EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                                    string links = "ApproveSchedule";
                                    string action = "hủy thay đổi";
                                    string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                                    List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                                    employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                                    try
                                    {
                                        Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                        foreach (EmployeeModel item in employeeSub)
                                        {
                                            DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                            Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                            employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }
                    }
                    else // mo ngay tuong lai
                    {
                        bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(line);
                        if (hasAssessment == true)
                        {
                            return Json(appId);
                        }
                        else
                        {
                            HammerDataProvider.CancelRequestDelete(appId);
                            //send email
                            DMSSalesForce employee = HammerDataProvider.GetSalesforceById(User.Identity.Name);
                            EmployeeModel employeelevel = HammerDataProvider.PrepareScheduleGetlevel(User.Identity.Name);
                            string links = "ApproveSchedule";
                            string action = "hủy thay đổi";
                            string emailTemplate = Server.MapPath(Constants.EmailChangeScheduleManageHTML);
                            List<EmployeeModel> employeeSub = new List<EmployeeModel>();
                            employeeSub = HammerDataProvider.GetMangerEmployee(User.Identity.Name);
                            try
                            {
                                Appointment input = HammerDataProvider.GetAppointmentById(Convert.ToInt32(appId));
                                foreach (EmployeeModel item in employeeSub)
                                {
                                    DMSSalesForce employeeSend = HammerDataProvider.GetSalesforceById(item.EmployeeID);
                                    Util.InitChangeSchedulerEmail(employeeSend.Email, employee.EmployeeName,
                                    employeeSub == null ? "" : item.EmployeeName, emailTemplate, "D", input.StartDate.Value.Date.ToString("dd/MM/yyyy"), action, Util.GetBaseUrl() + links, item.Level, employeelevel.Level);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            else
            {
            }
            return Json(appId);
        }

        [HttpPost]
        public ActionResult AppointmentDetailPartial()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            int id = Convert.ToInt32(Request.Params["id"]);
            return PartialView(HammerDataProvider.GetAppointmentById(id));
        }

        [HttpPost]
        public ActionResult DetailInfoPartial()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            int id = Convert.ToInt32(Request.Params["id"]);
            Session["Home_Id"] = id;
            Appointment appointment = HammerDataProvider.GetAppointmentById(id);
            List<Appointment> list =
                HammerDataProvider.GetDetailAppointments(appointment.StartDate.Value, appointment.UserLogin);
            if (list.Count <= 0)
            {
                list.Add(appointment);
            }
            return PartialView(list);
        }

        public ActionResult DetailInfoPartialGet()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            int id = Convert.ToInt32(Session["Home_Id"]);
            Appointment appointment = HammerDataProvider.GetAppointmentById(id);
            List<Appointment> list =
                HammerDataProvider.GetDetailAppointments(appointment.StartDate.Value, appointment.UserLogin);
            if (list.Count <= 0)
            {
                list.Add(appointment);
            }
            return PartialView("DetailInfoPartial", list);
        }
    }
}