using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammer.Models;
using DevExpress.Web.Mvc;
using Hammer.Helpers;
using log4net;
using eRoute.Filters;
using WebMatrix.WebData;
using eRoute.Models.eCalendar;
using eRoute;
using DMSERoute.Helpers;

namespace Hammer.Controllers
{
    [InitializeSimpleMembership]
    [Authorize()]
    public class ViewScheduleController : Controller
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(ViewScheduleController));
        //
        // GET: /ApproveSchedule/
        [Authorize]
        [ActionAuthorize("eCalendar_ViewSchedule", true)]
        public ActionResult Index(string EmployeeID)
        {

            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));

            ApproveScheduleModel model = new ApproveScheduleModel();
            model.EmployeeID = Utility.StringParse(EditorExtension.GetValue<string>("EmployeeID")); 
            model.Status = 0;
            model.ScheduleType = "D";

            if (EmployeeID != null)
            {
                ApproveScheduleObject obj = HammerDataProvider.GetApproveScheduleDataObject(model.EmployeeID, model.ScheduleType);
                Session["ApprovalSchedulerData"] = obj;
                Session["Approve_EmployeeID"] = model.EmployeeID;
            }
            else
            {
                ApproveScheduleObject obj = new ApproveScheduleObject();
                Session["ApprovalSchedulerData"] = obj;
            }
            return View(model);
            //return View(new ApproveScheduleModel()
            //{
            //    Status = 0,
            //    ScheduleType = "D"
            //});
        }
        [HttpPost]
        public ActionResult ApproveSchedulePartialView()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string employeeID = string.Empty;
            if (Session["Approve_EmployeeID"] != null)
                employeeID = Session["Approve_EmployeeID"].ToString();
            string scheduleType = "D";
            if (string.IsNullOrEmpty(employeeID) && Session["Approve_EmployeeID"] != null)
            {
                employeeID = Session["Approve_EmployeeID"].ToString();
            }

            if (string.IsNullOrEmpty(scheduleType) && Session["Approve_ScheduleType"] != null)
            {
                scheduleType = Session["Approve_ScheduleType"].ToString();
            }
            ApproveScheduleObject obj = HammerDataProvider.GetApproveScheduleDataObject(employeeID, scheduleType);
            Session["ApprovalSchedulerData"] = obj;
            Session["Approve_EmployeeID"] = employeeID;
            Session["Approve_ScheduleType"] = scheduleType;
            return PartialView(obj);
        }

        public ActionResult EditAppointment()
        {
            //Persist();
            string employeeID = Session["Approve_EmployeeID"].ToString();
            string scheduleType = Session["Approve_ScheduleType"].ToString();
            ApproveScheduleObject obj = HammerDataProvider.GetApproveScheduleDataObject(employeeID, scheduleType);
            Session["ApprovalSchedulerData"] = obj;
            return PartialView("ApproveSchedulePartialView", Session["ApprovalSchedulerData"]);
        }
       
        // Hieu code 28-02-14 View Assemt
        [HttpPost]
        public ActionResult ViewAssessment()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            ViewAssessmentModel model = new ViewAssessmentModel();
            string appointmentIds = Request.Params["appointmentIds"];
            string Em = Request.Params["Em"];
            if (Em != null && appointmentIds != null)
            {
                int app = Convert.ToInt32(appointmentIds);
                Appointment line = HammerDataProvider.GetAppointmentById(app);
                model.EmployeeID = Em;
                model.FromDate = line.StartDate.Value.Date;
                NoAssessmentModel check = HammerDataProvider.ViewNoAssessment(model.EmployeeID, model.FromDate, line.UniqueID);
                if (check.Header.UserID != null)
                {
                    model.HasTraining = false;
                }
                else
                {
                    model.HasTraining = true;
                }
                if (model.EmployeeID != null)
                {
                    bool Level = HammerDataProvider.ViewAssessmentGetLevel(model.EmployeeID);
                    if (Level == true)
                    {
                        SMAssessmentModel view = new SMAssessmentModel();
                        NoAssessmentModel viewno = new NoAssessmentModel();
                        if (model.HasTraining == true)
                        {
                            view = HammerDataProvider.ViewAssessmentSM(model.EmployeeID, model.FromDate, line.UniqueID);
                            Session["ApprovalSchedulerData"] = null;
                            return PartialView("DetailView", view);
                        }
                        else
                        {

                            viewno = HammerDataProvider.ViewNoAssessment(model.EmployeeID, model.FromDate, line.UniqueID);
                            return PartialView("DetailNoTrainningView", viewno);
                        }
                    }
                    else
                    {
                        if (model.HasTraining == true)
                        {
                            AssessmentModel view = new AssessmentModel();
                            view = HammerDataProvider.ViewAssessment(model.EmployeeID, model.FromDate, line.UniqueID);
                            return PartialView("DetailEmView", view);
                        }
                        else
                        {
                            NoAssessmentModel view = new NoAssessmentModel();
                            view = HammerDataProvider.ViewNoAssessment(model.EmployeeID, model.FromDate, line.UniqueID);
                            return PartialView("DetailNoTrainningView", view);
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Get schedule
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcel(string FromDate, string ToDate, string AttachSubordinateSchedule)
        {
            ApproveScheduleModel model = new ApproveScheduleModel();
            string employeeID = string.Empty;
            if (Session["Approve_EmployeeID"] != null)
                model.EmployeeID = Session["Approve_EmployeeID"].ToString();
            model.ScheduleType = "D";
            model.FromDate = Utility.DateTimeParse(FromDate);
            model.ToDate = Utility.DateTimeParse(ToDate);
            if (AttachSubordinateSchedule == "C")
            {
                model.AttachSubordinateSchedule = true;
            }
            else
            {
                model.AttachSubordinateSchedule = false;
            }
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string templatePath = Server.MapPath("/Templates/WorkingSchedule/") + "WorkingScheduleTemplate.xlsx";
            List<ScheduleExcelModel> scheduler = new List<ScheduleExcelModel>();
            //Get header appointments
            List<Appointment> headers = new List<Appointment>();
            foreach (var item in HammerDataProvider.GetAppointments(model.EmployeeID, model.Status,
                model.FromDate, model.ToDate, model.ScheduleType))
            {
                headers.Add(item as Appointment);
            }
            //Check if ScheduleType is Detail -> Get detail appointments            
            scheduler.Clear();
            foreach (var item in headers)
            {
                scheduler.Add(new ScheduleExcelModel()
                {
                    EmployeeIDG = item.UserLogin,
                    Date = item.StartDate.Value.Date,
                    Shift = item.ShiftID,
                    Title = item.Subject,
                    Content = item.Description,
                    WWCode = item.Employees
                });
            }
            if (!model.AttachSubordinateSchedule)
            {
                return File(Util.ExportExcel(scheduler, templatePath, model.EmployeeID), contentType,
                    string.Format("WorkingSchedule_{0}_{1}_{2}.xls", model.EmployeeID, model.FromDate.ToString("ddMMyyyy"),
                    model.ToDate.ToString("ddMMyyyy")));
            }
            else
            {
                List<EmployeeModel> subordinates = HammerDataProvider.GetSubordinateNoDuplicate(model.EmployeeID).
                    Where(x => x.Level != "SM" && x.EmployeeID != model.EmployeeID).ToList();
                List<MultiExportScheduleModelEx> multiExportList = new List<MultiExportScheduleModelEx>();
                //Add header
                List<ScheduleExcelModel> subScheduler = new List<ScheduleExcelModel>();
                multiExportList.Add(new MultiExportScheduleModelEx() { EmployeeID = model.EmployeeID, Schedule = scheduler });
                foreach (var emp in subordinates)
                {
                    //List<ScheduleExcelModel> subScheduler = new List<ScheduleExcelModel>();
                    foreach (Appointment item in
                        HammerDataProvider.GetAppointments(emp.EmployeeID,
                        model.Status, model.FromDate, model.ToDate, model.ScheduleType))
                    {
                        subScheduler.Add(new ScheduleExcelModel()
                        {
                            EmployeeIDG = item.UserLogin,
                            Date = item.StartDate.Value.Date,
                            Shift = item.ShiftID,
                            Title = item.Subject,
                            Content = item.Description,
                            WWCode = item.Employees
                        });

                    }
                    //multiExportList.Add(new MultiExportScheduleModelEx() { EmployeeID = emp.EmployeeID, Schedule = subScheduler });

                }
                multiExportList.Add(new MultiExportScheduleModelEx() { EmployeeID = null, Schedule = subScheduler });
                return File(Util.ExportAttachMultiEmployee(multiExportList, templatePath), contentType,
                    string.Format("WorkingSchedule_{0}_{1}_{2}.xls", model.EmployeeID, model.FromDate.ToString("ddMMyyyy"),
                    model.ToDate.ToString("ddMMyyyy")));
            }
        }

        public ActionResult DetailAppointmentPartial(DateTime date, string employeeID)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView(HammerDataProvider.GetDetailAppointments(date, employeeID));
        }

        [HttpPost]
        public ActionResult DeleteDetailAppoinment()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            try
            {
                string appointmentIdParam = Request.Params["appointmentId"];
                int appointmentId = 0;
                if (Int32.TryParse(appointmentIdParam, out appointmentId))
                {
                    HammerDataProvider.DeleteDetailAppointment(appointmentId);
                }
                return Json("");
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return Json(ex.Message);
            }
        }
        [HttpPost]
        public ActionResult MonthInforPartial()
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
            Session["Approve_Id"] = id;
            Appointment appointment = HammerDataProvider.GetAppointmentById(id);
            List<Appointment> list =
                HammerDataProvider.GetDetailAppointments(appointment.StartDate.Value, appointment.UserLogin);
            if (list.Count <= 0)
            {
                list.Add(appointment);
            }
            return PartialView(list);
        }
    }
}