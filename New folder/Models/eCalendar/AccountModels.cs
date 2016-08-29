using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
//using Utility.Phrase("Account");
using Hammer.Validators;


namespace Hammer.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
//        [Display(Name = "UserName", ResourceType = typeof(Messages))]
        public string UserName { get; set; }
//        [Display(Name = "FullName", ResourceType = typeof(Messages))]
        public string FullName { get; set; }
    }

    public class ChangePasswordModel
    {
       //[Required(ErrorMessageResourceName = "ErrControlReq", ErrorMessageResourceType = typeof(Validations))]
        [DataType(DataType.Password)]
//        [Display(Name = "OldPassword", ResourceType = typeof(Messages))]
        public string OldPassword { get; set; }

       // [Required(ErrorMessageResourceName = "ErrControlReq", ErrorMessageResourceType = typeof(Validations))]
        [StringLength(24, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
//        [Display(Name = "NewPassword", ResourceType = typeof(Messages))]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
//        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Messages))]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
       // [Required(ErrorMessageResourceName = "ErrControlReq", ErrorMessageResourceType = typeof(Validations))]
//        [Display(Name = "UserName", ResourceType = typeof(Messages))]
        public string UserName { get; set; }

      //  [Required(ErrorMessageResourceName = "ErrControlReq", ErrorMessageResourceType = typeof(Validations))]
        [DataType(DataType.Password)]
//        [Display(Name = "Password", ResourceType = typeof(Messages))]
        public string Password { get; set; }

        bool? rememberMe;
//        [Display(Name = "RememberMe", ResourceType = typeof(Messages))]
        public bool? RememberMe
        {
            get { return rememberMe ?? false; }
            set { rememberMe = value; }
        }
    }

    public class RegisterModel
    {
        //[Required(ErrorMessageResourceName = "ErrControlReq", ErrorMessageResourceType = typeof(Validations))]
//        [Display(Name = "UserName", ResourceType = typeof(Messages))]
        public string UserName { get; set; }

//        [Display(Name = "FullName", ResourceType = typeof(Messages))]
        public string FullName { get; set; }

      //  [Required(ErrorMessageResourceName = "ErrControlReq", ErrorMessageResourceType = typeof(Validations))]
        [DataType(DataType.EmailAddress)]
//        [Display(Name = "EmailAddress", ResourceType = typeof(Messages))]
        public string Email { get; set; }

       // [Required(ErrorMessageResourceName = "ErrControlReq", ErrorMessageResourceType = typeof(Validations))]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
//        [Display(Name = "Password", ResourceType = typeof(Messages))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
//        [Display(Name = "ConfirmPassword", ResourceType = typeof(Messages))]
       // [System.Web.Mvc.Compare("Password", ErrorMessageResourceName = "ErrConfirmPwdNotMatch", ErrorMessageResourceType = typeof(Validations))]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; }
    }

    public class UsersModel
    {
//        [Display(Name = "UserName", ResourceType = typeof(Messages))]
        public string UserName { get; set; }
//        [Display(Name = "FullName", ResourceType = typeof(Messages))]
        public string FullName { get; set; }
//        [Display(Name = "Role", ResourceType = typeof(Messages))]
        public string Role { get; set; }
    }

    public class RolesModel
    {
        public int RoleId { get; set; }
        [Required]
        [ExistedRoleValidation]
        public string RoleName { get; set; }
        public string OldRoleName { get; set; }
    }
}