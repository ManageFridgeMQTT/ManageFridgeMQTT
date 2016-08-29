using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
//using Utility.Phrase("ViewAssessment");
using Hammer.Helpers;
using Hammer.Validators;

namespace Hammer.Models
{




    public class ViewAssessmentModel
    {
        public bool HasTraining { get; set; }      
//        [Display(Name = "FromDate", ResourceType = typeof(Messages))]
        public DateTime FromDate { get; set; }

//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]
        public string EmployeeID { get; set; }

//        [Display(Name = "UniqueID", ResourceType = typeof(Messages))]
        public string UniqueID { get; set; }


        public List<EmployeeModel> ListEmployee { get; set; }
        public List<ComboDateAssessmentModel> ListUniqueID { get; set; }


    }
}