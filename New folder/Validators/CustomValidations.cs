using System.ComponentModel.DataAnnotations;
using System;
using Hammer.Models;
using WebMatrix.WebData;
using System.Web.Security;
using eRoute.Models.eCalendar;

namespace Hammer.Validators
{
    public class RequiredIfWWAttribute : ValidationAttribute
    {
        public string PropertyName { get; private set; }

        public RequiredIfWWAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyInfo = validationContext.ObjectType.GetProperty(this.PropertyName);
            if (propertyInfo == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", this.PropertyName));
            }

            var testValue = propertyInfo.GetValue(validationContext.ObjectInstance, null) as string;

            if (testValue == "WW" && !string.IsNullOrEmpty(value as string))
            {
                return ValidationResult.Success;
            }
            else if (testValue == "NWW" && value == null)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
        }
    }
    public class ContentValidationAttribute : ValidationAttribute
    {
        public string Des { get; private set; }


        public ContentValidationAttribute(string des)
        {
            this.Des = des;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var des = validationContext.ObjectType.GetProperty(this.Des);
            var workWith = validationContext.ObjectType.GetProperty("Des");

            if (des == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", this.Des));
            }  
            if (value == "8")
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(string.Format("Unknown property {0}", this.Des));
            //return new ValidationResult(this.ErrorMessageString);
        }
    }
    public class WorkWithValidationAttribute : ValidationAttribute
    {
        public string WorkingType { get; private set; }

        public string WorkingDate { get; private set; }
        public string WorkingDateTo { get; private set; }
        public string Shitf { get; private set; }

        public WorkWithValidationAttribute(string shitf, string workingType, string workingDate, string workingDateTo)
        {
            this.WorkingType = workingType;
            this.WorkingDate = workingDate;
            this.WorkingDateTo = workingDateTo;
            this.Shitf = shitf;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var workingType = validationContext.ObjectType.GetProperty(this.WorkingType);
            var workingDate = validationContext.ObjectType.GetProperty(this.WorkingDate);
            var workingDateTo = validationContext.ObjectType.GetProperty(this.WorkingDateTo);
            var shitf = validationContext.ObjectType.GetProperty(this.Shitf);
            var workWith = validationContext.ObjectType.GetProperty("WorkWith");

            if (workingType == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", this.WorkingType));
            }

            if (workingDate == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", this.WorkingDate));
            }
            if (workingDateTo == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", this.WorkingDateTo));
            }
            if (shitf == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", this.Shitf));
            }

            var wshitf = workingType.GetValue(validationContext.ObjectInstance, null) as string;
            var wType = workingType.GetValue(validationContext.ObjectInstance, null) as string;
            var wDate = workingDate.GetValue(validationContext.ObjectInstance, null) as DateTime?;
            var wDateTo = workingDate.GetValue(validationContext.ObjectInstance, null) as DateTime?;

            if (wType == "WW" && !string.IsNullOrEmpty(value as string))
            {
                if ((HammerDataProvider.EmployeeInRole(value as string) == Helpers.SystemRole.SalesForce &&
                    HammerDataProvider.IsValidWorkWith(value as string, wDate.Value)) ||
                    (HammerDataProvider.EmployeeInRole(value as string) == Helpers.SystemRole.Salesman))
                {
                    return ValidationResult.Success;
                }

                return new ValidationResult(this.ErrorMessageString);
            }
            else if (wType == "NWW" && value == null)
            {
                return ValidationResult.Success;
            }
            else if (wType == "NWW" && value != null)
            {
                workWith.SetValue(validationContext.ObjectInstance, null, null);
                return ValidationResult.Success;
            }
            return new ValidationResult(this.ErrorMessageString);
        }
    }

    public class ExistedRoleValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var roleProvider = (SimpleRoleProvider)Roles.Provider;
            string role = (string)value;
            if (!roleProvider.RoleExists(role))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(this.ErrorMessage);
        }
    }
}