using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
//using Utility.Phrase("PrepareScheduleNew");
using Hammer.Helpers;
using Hammer.Validators;

namespace Hammer.Models
{

    public class Year
    {
        public string YearID { get; set; }
    }
    public class Week
    {
        public int WeekID { get; set; }
    }
    public class PrepareScheduleNewModel
    {
        //[Required]
//        [Display(Name = "Year", ResourceType = typeof(Messages))]
        public string Year { get; set; }
        //[Required]
//        [Display(Name = "Week", ResourceType = typeof(Messages))]
        public int? Week { get; set; }
        //[Required]
//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]
        public string EmployeeID { get; set; }

        public List<string> ListYear { get; set; }
        public List<int> ListWeek { get; set; }
    }
    public class PopUpTooltipModel
    {
        public string RegionName { get; set; }
        public string AreaName { get; set; }
        public string NPP { get; set; }
        public string ProviceName { get; set; }
        public string RouteCD { get; set; }
        public string SM { get; set; }
        public string SMName { get; set; }  
       
    }
    public class PopUpDesModel
    {
          public string Year { get; set; }
          public int? Week { get; set; }
          public string EmployeeID { get; set; }
          public string EmployeeWW { get; set; }
          public string ID { get; set; }
          public string Shift { get; set; }
          [Required]
          public string Des { get; set; }
    }
    public class SendEmailPreapre
    {
        public string Year { get; set; }
        public int? Week { get; set; }
        public string EmployeeID { get; set; }       
        public string ID { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; }     
    }
    public class PrepareNoWWwModel
    {
        public int? No { get; set; }
        public string Year { get; set; }
        public int? Week { get; set; }
        public string gvEmployeeID { get; set; }       
        public string Day { get; set; }
        public string Shift { get; set; }
        public string RefWWType { get; set; }
        [Required]        
        public string Des { get; set; }       
        public string Status { get; set; }
    }
    //public class PrepareWWwModel
    //{
    //    public string Year { get; set; }
    //    public int? Week { get; set; }
    //    public string EmployeeWW { get; set; }
    //    public string RouteID { get; set; }
    //    public string RefResult { get; set; }
    //    public string MoAM { get; set; }
    //    public string MoPM { get; set; }
    //    public string MoDes { get; set; }
    //    public string TuAM { get; set; }
    //    public string TuPM { get; set; }
    //    public string TuDes { get; set; }
    //    public string WeAM { get; set; }
    //    public string WePM { get; set; }
    //    public string WeDes { get; set; }
    //    public string ThAM { get; set; }
    //    public string ThPM { get; set; }
    //    public string ThDes { get; set; }
    //    public string FrAM { get; set; }
    //    public string FrPM { get; set; }
    //    public string FrDes { get; set; }
    //    public string SaAM { get; set; }
    //    public string SaPM { get; set; }
    //    public string SaDes { get; set; }
    //    public string UserLogin { get; set; }
    //    public DateTime CreatedDateTime { get; set; }
    //    public string Status { get; set; }
    //}
}