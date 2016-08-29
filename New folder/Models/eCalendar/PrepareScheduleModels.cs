using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
//using Utility.Phrase("PrepareSchedule");
using Hammer.Helpers;
using Hammer.Validators;

namespace Hammer.Models
{
    public class MonthlyScheduleModel
    {
//        [Display(Name = "Date", ResourceType = typeof(Messages))]
        public DateTime Date { get; set; }
//        [Display(Name = "Content", ResourceType = typeof(Messages))]
        //[Required(ErrorMessageResourceName = "ErrControlRequired",
       //     ErrorMessageResourceType = typeof(Validations))]
        public string Content { get; set; }
    }

    public class DetailScheduleModel
    {
//        [Display(Name = "No", ResourceType = typeof(Messages))]
        public int? No { get; set; }
//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]
        public string EmployeeIDG { get; set; }
//        [Display(Name = "Shift", ResourceType = typeof(Messages))]
        public string Shift { get; set; }
//        [Display(Name = "DateView", ResourceType = typeof(Messages))]
        public DateTime DateView { get; set; }
//        [Display(Name = "Date", ResourceType = typeof(Messages))]
        public DateTime Date { get; set; }
//        [Display(Name = "DateTo", ResourceType = typeof(Messages))]
        public DateTime DateTo { get; set; } 
//        [Display(Name = "WorkingType", ResourceType = typeof(Messages))]
        public string WorkingType { get; set; }
//        [Display(Name = "Content", ResourceType = typeof(Messages))]      
        public string Content { get; set; }
//        [Display(Name = "WorkWith", ResourceType = typeof(Messages))]
      //  [RequiredIfWW("WorkingType", ErrorMessageResourceName = "ErrControlRequired",
      //      ErrorMessageResourceType = typeof(Validations))]
      //  [WorkWithValidation("Shift", "WorkingType", "Date","DateTo",ErrorMessageResourceName = "ErrInvalidWW", 
      //      ErrorMessageResourceType = typeof(Validations))]
        public string WorkWith { get; set; }
//        [Display(Name = "IsMeeting", ResourceType = typeof(Messages))]
        public bool IsMeeting { get; set; }
    }

    public class PrepareScheduleModel
    {
         [Required]
//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]       
        public string EmployeeID { get; set; }
//        [Display(Name = "ScheduleType", ResourceType = typeof(Messages))]
        public string ScheduleType { get; set; }
//        [Display(Name = "FromDate", ResourceType = typeof(Messages))]
        public DateTime FromDate { get; set; }
       // [DateGreaterThan("FromDate", ErrorMessageResourceName = "ErrEndDateGreaterThanFromDate", 
         //   ErrorMessageResourceType = typeof(Validations))]
        //[Required(ErrorMessageResourceName = "ErrControlRequired",
        //    ErrorMessageResourceType = typeof(Validations))]
//        [Display(Name = "EndDate", ResourceType = typeof(Messages))]
        public DateTime EndDate { get; set; }
//        [Display(Name = "Month", ResourceType = typeof(Messages))]
        public string Month { get; set; }
    }
}