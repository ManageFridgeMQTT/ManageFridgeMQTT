using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
//using Utility.Phrase("ScheduleSubmitSetting");
using eRoute.Models.eCalendar;

namespace Hammer.Models
{
    public class ScheduleType
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
    public class ScheduleSubmitSettingEx
    {
        public int No { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
      
        public string Note { get; set; }
        public int? CloseTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? Status { get; set; } 
        public bool? IsDatabase { get; set; }
        public string UserLogin { get; set; }    
    }    
    public class SendScheduleAgainMode
    {
        [Required]
//        [Display(Name = "ScheduleType", ResourceType = typeof(Messages))]
        public string ScheduleType { get; set; }

        [Required]
//        [Display(Name = "regionID", ResourceType = typeof(Messages))]
        public string regionID { get; set; }

//        [Display(Name = "areaID", ResourceType = typeof(Messages))]
        public string areaID { get; set; }  

        //[Required]
//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]
        public string EmployeeID { get; set; }
        [Required]
//        [Display(Name = "FromDate", ResourceType = typeof(Messages))]
        public DateTime FromDate { get; set; }
        [Required]
//        [Display(Name = "EndDate", ResourceType = typeof(Messages))]
        public DateTime EndDate { get; set; }
        [Required]
//        [Display(Name = "CloseTime", ResourceType = typeof(Messages))]
        public int CloseTime { get; set; }
        [Required]
//        [Display(Name = "Note", ResourceType = typeof(Messages))]
        public string Note { get; set; }
       
       

          
      
        public bool? sendEmail { get; set; }
        public List<ScheduleType> ListType { get; set; }
        public List<Region> ListRegion { get; set; }
        public List<Area> ListArea { get; set; }
    }
}