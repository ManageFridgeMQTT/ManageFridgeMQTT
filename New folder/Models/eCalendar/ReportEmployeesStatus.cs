using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
//using Utility.Phrase("ReportEmployeesStatus");
using Hammer.Helpers;
using Hammer.Validators;
using eRoute.Models.eCalendar;

namespace Hammer.Models
{
    public class ReportEmployeesStatusModel
    {        
        public DateTime Date { get; set; }       
        public string ShiftID  { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Level { get; set; }       
        public bool Status { get; set; } 
        public bool StatusTraining { get; set; }
        public bool StatusNotTraining { get; set; }
    }


    public class ReportEmployeesStatusFilterModel
    {
        [Required]
//        [Display(Name = "regionID", ResourceType = typeof(Messages))]
        public string regionID { get; set; }

//        [Display(Name = "areaID", ResourceType = typeof(Messages))]
        public string areaID { get; set; }

        //[Required]
//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]
        public string EmployeeID { get; set; }
//        [Display(Name = "FromDate", ResourceType = typeof(Messages))]
        public DateTime FromDate { get; set; }

//        [Display(Name = "EndDate", ResourceType = typeof(Messages))]
        public DateTime EndDate { get; set; }
      

        public List<Region> ListRegion { get; set; }
        public List<Area> ListArea { get; set; }

    }
    public class ParamertersModel
    {
        
//        [Display(Name = "FromDate", ResourceType = typeof(Messages))]
        public DateTime FromDate { get; set; }       
          
//        [Display(Name = "EndDate", ResourceType = typeof(Messages))]
        public DateTime EndDate { get; set; }

//        [Display(Name = "RegionID", ResourceType = typeof(Messages))]      
        public string RegionID { get; set; }

      
       
    }
}