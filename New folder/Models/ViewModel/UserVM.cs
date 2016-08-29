using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace eRoute.Models.ViewModel
{
    public class RoleVM
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Tên đăng nhập phải có ít nhất 3 kí tự")]
        public string RoleName { get; set; }

        public string Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        public int ParentID { get; set; }
        public string ApplicationCD { get; set; }
    }

    public class UserVM
    {
        [Required(ErrorMessage = "Tên đăng nhập phải có ít nhất 3 kí tự")]
        public string UserName { get; set; }
        public string FullName { get; set; }

        [Required(ErrorMessage = "Xin vui lòng nhập email")]
        public string Email { get; set; }
        public string Phone { get; set; }

        [Required(ErrorMessage = "Xin vui lòng chọn nhóm quyền")]
        public int RoleID { get; set; }
        public int UserId { get; set; }

        public bool IsConfirmed { get; set; }

        public string RoleName { get; set; }
        public string ApplicationCD { get; set; }
    }

    public class ReportUserActionLogTerritoryVM
    {
        public List<pp_ReportUserActionLogTerritoryResult> ListItem { get; set; }
        public DateTime FromDate { get; set; }
        public string strFromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string strToDate { get; set; }

        public List<pp_GetUserInfoResult> ListUser { get; set; }
        public string loginID { get; set; }
        public Int32 userID { get; set; }
    }

    public class RoleFeatureAssignmentVM : BaseModel
    {
        public List<Role> ListRole { get; set; }
        public List<Feature> ListFeature { get; set; }
        public List<RoleFeature> ListRF { get; set; }
        public List<SelectListItem> ListCheckBox { get; set; }
        public Int32 roleID { get; set; }
    }

    public class BaseModel
    {
        public string pageTitle { get; set; }
        public string strError { get; set; }
        public string strMessage { get; set; }
        public string strSuccess { get; set; }
    }
    public class CustomDataSetting
    {
        public string SettingName { get; set; }
        public decimal? ValueFrom { get; set; }
        public decimal? ValueTo { get; set; }
        public string Color { get; set; }
        public string Type { get; set; }
    }
    public class ResponseMessage
    {
        public int ID { get; set; }
        public string Message { get; set; }
    }
}