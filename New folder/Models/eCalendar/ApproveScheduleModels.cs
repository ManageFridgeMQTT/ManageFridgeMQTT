using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using Utility.Phrase("ApproveSchedule");
using System.ComponentModel.DataAnnotations;
using DevExpress.Web.ASPxScheduler;
using DevExpress.Web.Mvc;
using System.Collections;

namespace Hammer.Models
{
    public class MultiExportScheduleModel
    {
        public string EmployeeID { get; set; }
        public List<ScheduleModel> Schedule { get; set; }
    }
    public class MultiExportScheduleModelEx
    {
        public string EmployeeID { get; set; }
        public List<ScheduleExcelModel> Schedule { get; set; }
    }
    
    public class ApproveScheduleModel
    {
//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]
        //[Required(ErrorMessageResourceName = "ErrControlRequired", ErrorMessageResourceType = typeof(Validations))]
        public string EmployeeID { get; set; }
       // [Required(ErrorMessageResourceName = "ErrControlRequired", ErrorMessageResourceType = typeof(Validations))]
        public string ScheduleType { get; set; }
//        [Display(Name = "Status", ResourceType = typeof(Messages))]
       // [Required(ErrorMessageResourceName = "ErrControlRequired", ErrorMessageResourceType = typeof(Validations))]
        public int Status { get; set; }
//        [Display(Name = "AttachSubordinateSchedule", ResourceType = typeof(Messages))]
        public bool AttachSubordinateSchedule { get; set; }
//        [Display(Name = "FromDate", ResourceType = typeof(Messages))]
        public DateTime FromDate { get; set; }
//        [Display(Name = "ToDate", ResourceType = typeof(Messages))]
        public DateTime ToDate { get; set; }
    }

    public class ApproveScheduleObject
    {
        public IEnumerable Appointments { get; set; }
        public IEnumerable Resources { get; set; }
    }

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
        public string RejectReason
        {

            get { return Convert.ToString(Appointment.CustomFields["RejectReason"]); }
        }
        public string UserLogin
        {
            get { return Convert.ToString(Appointment.CustomFields["UserLogin"]); }
        }
    }
}