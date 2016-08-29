using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
//using Utility.Phrase("AssessmentCapacity");
using Hammer.Helpers;
using Hammer.Validators;

namespace Hammer.Models
{   
    public class AssessmentCapacityModel
    {  
//        [Display(Name = "Period", ResourceType = typeof(Messages))]
        public string Period { get; set; }

    }    
    public class DetailsAssessmentCapacity
    {
//        [Display(Name = "No", ResourceType = typeof(Messages))]
        public int? No { get; set; }
//        [Display(Name = "PeriodID", ResourceType = typeof(Messages))]
        public string PeriodID { get; set; }
//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]
        public string EmployeeID { get; set; }
//        [Display(Name = "EmployeeName", ResourceType = typeof(Messages))]
        public string EmployeeName { get; set; }
//        [Display(Name = "SFLevel", ResourceType = typeof(Messages))]
        public int? SFLevel { get; set; }
        [Required]
//        [Display(Name = "PassionateSuccess", ResourceType = typeof(Messages))]
        public int PassionateSuccess { get; set; }
//        [Display(Name = "Mark1", ResourceType = typeof(Messages))]
        public string Mark1 { get; set; }
//        [Display(Name = "BreakthroughImprovement", ResourceType = typeof(Messages))]
        public int BreakthroughImprovement { get; set; }
//        [Display(Name = "Mark2", ResourceType = typeof(Messages))]
        public string Mark2 { get; set; }
//        [Display(Name = "InitiativeSpeed", ResourceType = typeof(Messages))]
        public int InitiativeSpeed { get; set; }
//        [Display(Name = "Mark3", ResourceType = typeof(Messages))]
        public string Mark3 { get; set; }
//        [Display(Name = "CustomerOrientation", ResourceType = typeof(Messages))]
        public int CustomerOrientation { get; set; }
//        [Display(Name = "Mark4", ResourceType = typeof(Messages))]
        public string Mark4 { get; set; }
//        [Display(Name = "CommitmentCooperation", ResourceType = typeof(Messages))]
        public int CommitmentCooperation { get; set; }
//        [Display(Name = "Mark5", ResourceType = typeof(Messages))]
        public string Mark5 { get; set; }
//        [Display(Name = "Integrity", ResourceType = typeof(Messages))]
        public int Integrity { get; set; }
//        [Display(Name = "Mark6", ResourceType = typeof(Messages))]
        public string Mark6 { get; set; }
//        [Display(Name = "MentoringDeveloping", ResourceType = typeof(Messages))]
        public int MentoringDeveloping { get; set; }
//        [Display(Name = "Mark7", ResourceType = typeof(Messages))]
        public string Mark7 { get; set; }
//        [Display(Name = "AffectOthers", ResourceType = typeof(Messages))]
        public int AffectOthers { get; set; }
//        [Display(Name = "Mark8", ResourceType = typeof(Messages))]
        public string Mark8 { get; set; }
//        [Display(Name = "TeamLeader", ResourceType = typeof(Messages))]
        public int TeamLeader { get; set; }
//        [Display(Name = "Mark9", ResourceType = typeof(Messages))]
        public string Mark9 { get; set; }
//        [Display(Name = "StrategicThinking", ResourceType = typeof(Messages))]
        public int StrategicThinking { get; set; }
//        [Display(Name = "Mark10", ResourceType = typeof(Messages))]
        public string Mark10 { get; set; }
//        [Display(Name = "CreatedDate", ResourceType = typeof(Messages))]
        public DateTime CreatedDate { get; set; }
//        [Display(Name = "CreatedUser", ResourceType = typeof(Messages))]
        public string CreatedUser { get; set; }
//        [Display(Name = "UpdateDate", ResourceType = typeof(Messages))]
        public DateTime UpdateDate { get; set; }
//        [Display(Name = "UpdateUser", ResourceType = typeof(Messages))]
        public string UpdateUser { get; set; }       
        public bool IsDatabase { get; set; }
       

    }
    public class ExcelDetailsAssessmentCapacity
    {  
        public string PeriodID { get; set; }       
        public string EmployeeID { get; set; }          
        public int PassionateSuccess { get; set; }
        public string Mark1 { get; set; }       
        public int BreakthroughImprovement { get; set; }
        public string Mark2 { get; set; }       
        public int InitiativeSpeed { get; set; }
        public string Mark3 { get; set; }       
        public int CustomerOrientation { get; set; }
        public string Mark4 { get; set; }        
        public int CommitmentCooperation { get; set; }
        public string Mark5 { get; set; }     
        public int Integrity { get; set; }
        public string Mark6 { get; set; }       
        public int MentoringDeveloping { get; set; }
        public string Mark7 { get; set; }       
        public int AffectOthers { get; set; }
        public string Mark8 { get; set; }       
        public int TeamLeader { get; set; }
        public string Mark9 { get; set; }       
        public int StrategicThinking { get; set; }
        public string Mark10 { get; set; }
        public string Note { get; set; } 
    }   
}