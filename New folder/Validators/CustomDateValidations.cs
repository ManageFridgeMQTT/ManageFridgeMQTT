using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Hammer.Validators
{
    public class DateGreaterThanAttribute : ValidationAttribute, IClientValidatable
    {
        public string PropertyName { get; private set; }
        private string currentDisplayName;

        public DateGreaterThanAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            this.currentDisplayName = validationContext.DisplayName;
            var propertyInfo = validationContext.ObjectType.GetProperty(this.PropertyName);
            if (propertyInfo == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", this.PropertyName));
            }

            var testValue = propertyInfo.GetValue(validationContext.ObjectInstance, null) as DateTime?;

            if (testValue.Value.Date < ((DateTime)value).Date)
            {
                return ValidationResult.Success;
            }
            var displayName = propertyInfo.GetCustomAttributes(typeof(DisplayAttribute),
                false).Cast<DisplayAttribute>().Single().Name;
            return new ValidationResult(this.FormatErrorMessage(displayName));
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessageString,
                ValidationType = "dategreaterthan"
            };
            rule.ValidationParameters["propertytested"] = this.PropertyName;
            yield return rule;
        }
    }

    public class DateGreaterThanEqualAttribute : ValidationAttribute, IClientValidatable
    {
        public string PropertyName { get; private set; }

        public DateGreaterThanEqualAttribute(string propertyName)
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

            var testValue = propertyInfo.GetValue(validationContext.ObjectInstance, null) as DateTime?;

            if (testValue.Value.Date <= ((DateTime)value).Date)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(this.ErrorMessageString);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessageString,
                ValidationType = "dategreaterthanequal"
            };
            rule.ValidationParameters["propertytested"] = this.PropertyName;
            yield return rule;
        }
    }
}