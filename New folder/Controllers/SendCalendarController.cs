using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammer.Models;
//using DevExpress.Web.ASPxUploadControl;
using System.IO;
using OfficeOpenXml;
using DevExpress.Web.Mvc;
using System.Drawing;
using System.Web.UI;
//using Utility.Phrase("SendCalendar");
using System.Collections;
using DevExpress.Web.ASPxScheduler;
using Hammer.Helpers;
using System.Web.UI.WebControls;
using log4net;
using eRoute.Filters;
using WebMatrix.WebData;
//using DevExpress.Web;

using eRoute.Models.eCalendar;
using DMSERoute.Helpers;


namespace Hammer.Controllers
{
    [InitializeSimpleMembership]    
    [Authorize()]
    public class SendCalendarController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SendCalendarController));
        // GET: /SendCalendar/
        public ActionResult Index()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            bool kq = HammerDataProvider.CheckPermissionRoles(User.Identity.Name);
            DMSSFHierarchy query = HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(User.Identity.Name);
            if (kq == true)
            {
                if (query.IsSalesForce == true && query.TerritoryType == 'D')
                {
                    return RedirectToAction("index", "ErrorPermission");
                }
            }
            else
            {
                if (query.TerritoryType == 'N' && query.IsSalesForce == false)
                {

                }
                else
                {
                    return RedirectToAction("index", "ErrorPermission");
                }
            }
            Session["ProceError"] = null;
            Session["ExcelError"] = null;
            SchedulerDataHelper.UserLogin = null;           
            SchedulerDataObject idex = new SchedulerDataObject();
            idex.ScheduleType = "detail";
            Session["Type"] = "detail";
            Session["employeeID"] = null;
            //idex.Appointments = SchedulerDataHelper.GetDetailAppointments(User.Identity.Name);
            return View(idex);           
        }
        // load  partial content         
        public ActionResult CustomAppointmentFormPartialDetail(DateTime date, string employeeID)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            return PartialView(HammerDataProvider.GetDetailAppointments(date, employeeID));
        }
        [HttpPost]
        public ActionResult ScheduleTypeChanged()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            string scheduleType = Request.Params["scheduleType"];
            Session["Type"] = scheduleType;
            return Json(scheduleType);
        }
        [HttpPost]
        public ActionResult SchedulerPartial(SchedulerDataObject ojb)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            if (Session["employeeID"] != null )
            {
                ojb.EmployeeID = Session["employeeID"].ToString();
            }
            if (Session["Type"] != null)
            {
                string type = Session["Type"].ToString();
                SchedulerDataHelper.UserLogin = ojb.EmployeeID;
                if (type == "month")
                {
                    return PartialView("SchedulerPartial", SchedulerDataHelper.DataObject);                   
                }
                else
                {
                    return PartialView("SchedulerPartial", SchedulerDataHelper.DataObjectDetail);
                }
            }else
            {
                return PartialView("SchedulerPartial", ojb);
            }
        }        
        #region ExportTemplate        
        // error export excel
        public FileResult ExportTemplate(SchedulerDataObject ojb)
        {            
            string type = "";
            if (Session["Type"] != null)
            {
                type = Session["Type"].ToString();
            }
            string templatePath = Server.MapPath("/Templates/WorkingSchedule/") + "ErrorSchedule.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string filename = "MauLoi_" + DateTime.Now.ToString("ddMMyyyyhhmm") + ".xlsx";
            List<ScheduleErrorModel> list = Session["ExcelError"] as List<ScheduleErrorModel>;            
            Byte[] fileBytes = Util.GenerateErrorTemplate(list, templatePath, type);
            FileResult result = File(fileBytes, contentType, filename);
            return result; 
        }
        #endregion
        #region Load
       
        [HttpPost]
        public ActionResult LoadSchedule(SchedulerDataObject ojb)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            //delare
            Session["ProceError"] = null;
            Session["ExcelError"] = null;            
            Stream FileName = Request.Files[1].InputStream ;
            List<ScheduleErrorModel> ListExcel =  new List<ScheduleErrorModel>();
            List<ScheduleErrorModel> ListExcelEmail =  new List<ScheduleErrorModel>();
            List<ScheduleErrorModel> ListError = new List<ScheduleErrorModel>();
            if (Request.Files[1].ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" 
                || Request.Files[1].ContentType == "application/xlsx"
                || Request.Files[1].ContentType == "application/vnd.ms-excel")
            {
                //try
                //{
                    try
                    {
                        ListExcel = HammerDataProvider.ReadExcel(FileName, ojb.ScheduleType);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                        Session["ProceError"] = Utility.Phrase("SendCalendar.ErrorExcelType");
                        return PartialView("Index", ojb);
                    }
                    ListError = HammerDataProvider.CheckRecordError(ListExcel,ojb.ScheduleType);
                    if (ojb.ScheduleType == "detail")
                    {
                        if (ListError.Count <= 0)
                        {
                            try
                            {
                                ListError = HammerDataProvider.CheckErrorTempletePass(ListExcel, ojb.ScheduleType);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex.Message, ex);
                                Session["ProceError"] = Utility.Phrase("SendCalendar.ErrorCastValue");
                                return PartialView("Index", ojb);
                            }    
                        }
                        else
                        {
                            try
                            {
                                ListError = HammerDataProvider.CheckErrorTemplete(ListError, ojb.ScheduleType);
                            }
                             catch (Exception ex)
                            {
                                Log.Error(ex.Message, ex);
                                Session["ProceError"] = Utility.Phrase("SendCalendar.ErrorCastValue");
                                return PartialView("Index", ojb);
                            }                            
                        }
                    }                            

            }else
            {
                 ListExcel = null;
                 ListError = null;
            }
            if (ListError != null)
            {
                if (ListError.Count <= 0)
                {
                    //insert data base;
                    if (ojb.ScheduleType == "detail")
                    {
                        try
                        {                            
                            //Save file
                            string filePath;
                            FileInfo fileinfo = new FileInfo(Request.Files[1].FileName);
                            string NameOld = Request.Files[1].FileName.Split('.')[0];
                            if (User.IsInRole("NSM") == true)
                            {                                
                                filePath = Server.MapPath("/DataUpload/Detail/NSM/") + DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + User.Identity.Name + "-" + NameOld + ".xlsx";
                            }else if (User.IsInRole("RSM") == true)
                            {                              
                                filePath = Server.MapPath("/DataUpload/Detail/RSM/") + DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + User.Identity.Name + "-" + NameOld + ".xlsx";
                            }
                            else if (User.IsInRole("ASM") == true)
                            {                                
                                filePath = Server.MapPath("/DataUpload/Detail/ASM/") + DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + User.Identity.Name + "-" + NameOld + ".xlsx";
                            }
                            else
                            {                               
                                filePath = Server.MapPath("/DataUpload/Detail/SS/") + DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + User.Identity.Name + "-" + NameOld + ".xlsx";                      
                            }
                            string templatePath = Server.MapPath("/Templates/WorkingSchedule/") + "WorkingScheduleTemplate.xlsx";                                                                              
                            Util.SaveFileUpload(ListExcel, templatePath, ojb.ScheduleType,filePath);                           
                            foreach (var tem in ListExcel)
                            {
                                string emailTemplate = null;
                                if (User.Identity.Name != tem.EmployeeID)
                                {
                                    emailTemplate = Server.MapPath(Constants.EmailUploadScheduleHTML);
                                }
                                else
                                {
                                    emailTemplate = Server.MapPath(Constants.EmailUploadScheduleManageHTML);
                                }
                                if (tem.WWCode == string.Empty)
                                    tem.WWCode = null;
                                tem.emailTemplate = emailTemplate;
                                ListExcelEmail.Add(tem);
                            }
                            // run insert
                            //HammerDataProvider.InsertOrUpdate(HammerDataProvider.SetTypeDetailRecordTemplete(ListExcel),ojb.ScheduleType,User.Identity.Name);
                            HammerDataProvider.InsertOrUpdate(ListExcelEmail, ojb.ScheduleType, User.Identity.Name);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message, ex);                            
                            Session["ProceError"] = Utility.Phrase("SendCalendar.ErrorProcess") + ": " + ex.Message;
                            return PartialView("Index", ojb);
                            //return Json(data);
                        }
                    }
                    else
                    {
                        try
                        {
                            //Save file
                            string filePath;
                            FileInfo fileinfo = new FileInfo(Request.Files[1].FileName);
                            string NameOld = Session["employeeID"].ToString()+"-"+ Request.Files[1].FileName.Split('.')[0];                           
                            if (User.IsInRole("NSM") == true)
                            {
                                filePath = Server.MapPath("/DataUpload/Month/NSM/") + DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + User.Identity.Name + "-" + NameOld + ".xlsx";
                            }else if (User.IsInRole("RSM") == true)
                            {
                                filePath = Server.MapPath("/DataUpload/Month/RSM/") + DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + User.Identity.Name + "-" + NameOld + ".xlsx";
                            }
                            else if (User.IsInRole("ASM") == true)
                            {
                                filePath = Server.MapPath("/DataUpload/Month/ASM/") + DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + User.Identity.Name + "-" + NameOld + ".xlsx";
                            }
                            else
                            {
                                filePath = Server.MapPath("/DataUpload/Month/SS/") + DateTime.Now.ToString("yyyyMMddhhmmss") + "-" + User.Identity.Name + "-" + NameOld + ".xlsx";
                            }                                                       
                            string templatePath = Server.MapPath("/Templates/WorkingSchedule/") + "WorkingScheduleTemplate.xlsx";
                            Util.SaveFileUpload(ListExcel, templatePath, ojb.ScheduleType, filePath);
                            // run insert  
                            foreach (var tem in ListExcel)
                            {
                                string emailTemplate = null;
                                if (User.Identity.Name != tem.EmployeeID)
                                {
                                    emailTemplate = Server.MapPath(Constants.EmailUploadScheduleHTML);
                                }
                                else
                                {
                                    emailTemplate = Server.MapPath(Constants.EmailUploadScheduleManageHTML);
                                }
                                if (tem.WWCode == string.Empty)
                                    tem.WWCode = null;
                                tem.emailTemplate = emailTemplate;
                                ListExcelEmail.Add(tem);
                            }
                            HammerDataProvider.InsertOrUpdate(ListExcelEmail, ojb.ScheduleType, User.Identity.Name);                            
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message, ex);
                            var data = new JsonData
                            {
                                Message = Utility.Phrase("SendCalendar.ExcelError") +" - " +ex.Message,
                                TimeError = DateTime.Now.ToShortTimeString()
                            };                          
                            return Json(data);
                        }
                    }
                }
                else
                {
                    Session["ExcelError"] = ListError;                   
                }
            }
              
                
            Session["Type"] = ojb.ScheduleType;
            ojb.Appointments = SchedulerDataHelper.GetDetailAppointments(User.Identity.Name);
            return PartialView("Index", ojb);
               
        }
        public class JsonData
        {
            public string Message { get; set; }

            public string TimeError { get; set; }
        }
        #endregion
        #region CustomizationTemplateAppoint
        public class CustomAppointmentTemplateContainer : AppointmentFormTemplateContainer
        {
            public CustomAppointmentTemplateContainer(MVCxScheduler scheduler)
                : base(scheduler)
            {
            }
            public string Employees
            {
                get { return Convert.ToString(Appointment.CustomFields["Employees"]); }
            }
            public string Phone
            {
                get { return Convert.ToString(Appointment.CustomFields["Phone"]); }
            }
        }
        public void UpdateAppointment()
        {
            Appointment insertedAppt = SchedulerExtension.GetAppointmentToInsert<Appointment>("scheduler", SchedulerDataHelper.GetAppointments(Session["employeeID"].ToString()),
               SchedulerDataHelper.GetAppointments(Session["employeeID"].ToString()), SchedulerDataHelper.CustomAppointmentStorage, SchedulerDataHelper.DefaultResourceStorage);
            Appointment[] updatedAppt = SchedulerExtension.GetAppointmentsToUpdate<Appointment>("scheduler", SchedulerDataHelper.GetAppointments(Session["employeeID"].ToString()),
                 SchedulerDataHelper.GetResources(), SchedulerDataHelper.DefaultAppointmentStorage, SchedulerDataHelper.DefaultResourceStorage);
            
            foreach (var appt in updatedAppt)
            {
                if (appt.StartDate.Value.Date == appt.EndDate.Value.Date)
                {
                    break;
                }
                else
                {   
                    DateTime systemDate = DateTime.Now.Date;
                    int kq = DateTime.Compare(appt.StartDate.Value.Date, systemDate);
                    if (kq <= 0)// less today.
                    {
                        int Is = HammerDataProvider.CheckDateInScheduleSubmitSetting(appt.StartDate.Value.Date, Session["employeeID"].ToString(), "M");
                        if (Is == 1) // not data
                        {
                            break;
                        }
                        else
                        {
                            SchedulerDataHelper.UpdateAppointment(appt);
                        }                       
                    }
                    else // end less
                    {
                        //check data exsist label 2-4
                        int rs = HammerDataProvider.CheckexsitAppointment(Session["employeeID"].ToString(), appt.StartDate.Value.Date, appt.EndDate.Value.Date, "M");
                            if (rs == 1)
                            {
                                break;
                            }
                            else
                            {
                                        // check label 0
                                int rs1 = HammerDataProvider.CheckexsitAppointmentNew(Session["employeeID"].ToString(), appt.StartDate.Value.Date, appt.EndDate.Value.Date, "M");
                                        if (rs1 == 1)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            SchedulerDataHelper.UpdateAppointment(appt);
                                        }
                            }
                    }//end heigher
                }
            }//end for

            Appointment[] removedAppt = SchedulerExtension.GetAppointmentsToRemove<Appointment>("scheduler", SchedulerDataHelper.GetAppointments(Session["employeeID"].ToString()),
                SchedulerDataHelper.GetResources(), SchedulerDataHelper.DefaultAppointmentStorage, SchedulerDataHelper.DefaultResourceStorage);
            foreach (var appt in removedAppt)
            {
                SchedulerDataHelper.RemoveAppointment(appt);
            }
        }
        public void UpdateAppointmentDetail()
        {
            Appointment insertedAppt = SchedulerExtension.GetAppointmentToInsert<Appointment>("scheduler", SchedulerDataHelper.GetDetailAppointments(Session["employeeID"].ToString()),
               SchedulerDataHelper.GetDetailAppointments(Session["employeeID"].ToString()), SchedulerDataHelper.CustomAppointmentStorage, SchedulerDataHelper.DefaultResourceStorage);
            Appointment[] updatedAppt = SchedulerExtension.GetAppointmentsToUpdate<Appointment>("scheduler", SchedulerDataHelper.GetDetailAppointments(Session["employeeID"].ToString()),
                 SchedulerDataHelper.GetResources(), SchedulerDataHelper.DefaultAppointmentStorage, SchedulerDataHelper.DefaultResourceStorage);
            foreach (var appt in updatedAppt)
            {
                if (appt.StartDate.Value.Date != appt.EndDate.Value.Date)
                {
                    break;
                }
                else
                {
                    DateTime systemDate = DateTime.Now.Date;
                    int kq = DateTime.Compare(appt.StartDate.Value.Date, systemDate);
                    if (kq <= 0)// less today.
                    {
                        int Is = HammerDataProvider.CheckDateInScheduleSubmitSetting(appt.StartDate.Value.Date, Session["employeeID"].ToString(), "D");
                        if (Is == 1) // not data
                        {
                            break;
                        }
                        else
                        {
                            SchedulerDataHelper.UpdateAppointment(appt);
                        }
                    }
                    else
                    {// 
                        //check data exsist label 2-4
                        int rs = HammerDataProvider.CheckexsitAppointmentDetail(Session["employeeID"].ToString(), appt.StartDate.Value, appt.EndDate.Value, false);
                        if (rs == 1)
                        {
                            break;
                        }
                        else
                        {                                                        
                             SchedulerDataHelper.UpdateAppointment(appt);                           
                        }
                        //SchedulerDataHelper.UpdateAppointment(appt);
                    }//end higher                  
                }
            }//end for
            Appointment[] removedAppt = SchedulerExtension.GetAppointmentsToRemove<Appointment>("scheduler", SchedulerDataHelper.GetDetailAppointments(Session["employeeID"].ToString()),
                SchedulerDataHelper.GetResources(), SchedulerDataHelper.DefaultAppointmentStorage, SchedulerDataHelper.DefaultResourceStorage);
            foreach (var appt in removedAppt)
            {
                SchedulerDataHelper.RemoveAppointment(appt);
            }
        }
        #endregion
        //public class UploadControlHelper
        //{
        //    public static readonly DevExpress.Web.ValidationSettings ValidationSettings = new DevExpress.Web.ValidationSettings
        //    {
        //         a = new string[] { ".xlsx", },
        //        //NotAllowedFileExtensionErrorText = Utility.Phrase("SendCalendar.ErrorFileUpload")                
        //    };           
        //}
        public ActionResult CustomFormsPartialEditAppointmen()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            string type = "";
            if (Session["Type"] != null)
            {
                type = Session["Type"].ToString();
            }
            SchedulerDataHelper.UserLogin = Session["employeeID"].ToString();
            if (type == "month")
            {
                UpdateAppointment();
                return PartialView("SchedulerPartial", SchedulerDataHelper.DataObject);
            }
            else
            {
                UpdateAppointmentDetail();
                return PartialView("SchedulerPartial", SchedulerDataHelper.DataObjectDetail);
            }
        }
        public ActionResult CustomFormsPartialEditAppointmenDetail()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            SchedulerDataHelper.UserLogin = Session["employeeID"].ToString();
            UpdateAppointmentDetail();
            return PartialView("DetailSchedulerPartial", SchedulerDataHelper.DataObjectDetail);
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
            int id = 0;
            if (Request.Params["id"] != null)
            {
                id = Convert.ToInt32(Request.Params["id"]);
                Session["ID"] = id;
            }else
            {
                id = Convert.ToInt16(Session["ID"].ToString());
            }
            Appointment appointment = HammerDataProvider.GetAppointmentById(id);
            List<Appointment> list =
                HammerDataProvider.GetDetailAppointments(appointment.StartDate.Value, appointment.UserLogin);
            if (list.Count <= 0)
            {
                list.Add(appointment);  
            }
            return PartialView(list);
        }
        [HttpPost]
        public ActionResult SendCalendarLoadPartialView()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            string scheduleType = Request.Params["employeeID"];
            Session["employeeID"] = scheduleType;
            return Json(scheduleType);       
        }
        
    }
}
