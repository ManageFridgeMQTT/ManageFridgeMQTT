using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using DMSERoute.Helpers;

namespace eRoute.Models.Attribute
{
    public class RequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute
    {
        private string _displayName;

        public RequiredAttribute()
        {
            ErrorMessageResourceName = "LBL_Validation_Required";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _displayName = validationContext.DisplayName;
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = Utility.Phrase(ErrorMessageResourceName);
            return string.Format(msg, _displayName);
        }
    }

    public class RegularExpressionAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute
    {
        private string _displayName;

        public RegularExpressionAttribute()
            : base(@"^\w+$")
        {
        }

        public RegularExpressionAttribute(string pattern)
            : base(pattern)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _displayName = validationContext.DisplayName;
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = Utility.Phrase(ErrorMessageResourceName);
            return string.Format(msg, _displayName);
        }
    }

    public class RangeAttribute : System.ComponentModel.DataAnnotations.RangeAttribute
    {
        private string _displayName;

        public RangeAttribute()
            : base(0, 0)
        {
            ErrorMessageResourceName = "LBL_Validation_Required";
        }

        public RangeAttribute(double min, double max)
            : base(min, max)
        {
        }

        public RangeAttribute(int min, int max)
            : base(min, max)
        {
        }

        public RangeAttribute(Type type, string minimum, string maximum)
            : base(type, minimum, maximum)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _displayName = validationContext.DisplayName;
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = Utility.Phrase(ErrorMessageResourceName);
            return string.Format(msg, _displayName);
        }
    }

    public class StringLengthAttribute : System.ComponentModel.DataAnnotations.StringLengthAttribute
    {
        private string _displayName;

        public StringLengthAttribute()
            : base(0)
        {
            ErrorMessageResourceName = "LBL_Validation_Required";
        }

        public StringLengthAttribute(int maximumLength)
            : base(maximumLength)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            _displayName = validationContext.DisplayName;
            return base.IsValid(value, validationContext);
        }

        public override string FormatErrorMessage(string name)
        {
            var msg = Utility.Phrase(ErrorMessageResourceName);
            return string.Format(msg, _displayName);
        }
    }

    public class GreaterAttribute : ValidationAttribute
    {
        private string displayName;
        private string property;

        public GreaterAttribute(string property)
        {
            this.property = property;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                displayName = validationContext.DisplayName;
                var obj = validationContext.ObjectType.GetProperty(this.property);
                if (value != null && obj != null)
                {
                    var objValue = obj.GetValue(validationContext.ObjectInstance, null);

                    if (Convert.ToInt32(value) < Convert.ToInt32(objValue))
                    {
                        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName, objValue.ToString()));
                    }
                }
            }
            catch { }

            return ValidationResult.Success;
        }

        public string FormatErrorMessage(string name, string orginalValue)
        {
            var msg = Utility.Phrase(ErrorMessageResourceName);
            msg = string.Format(msg, orginalValue);
            return string.Format(msg, displayName);
        }
    }
}
