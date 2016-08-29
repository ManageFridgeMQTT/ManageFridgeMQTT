using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eRoute.Models;

namespace eRoute.Models.ViewModel
{
    public class AssesmentVM
    {
        public List<sp_Assesment_Auditor_ViewResult> sp_Assesment_Auditor_View { get; set; }
        public List<sp_Assesment_Auditor_ViewResult> sp_Assesment_Auditor_View_Search { get; set; }
        public List<sp_Assesment_AuditorResult> sp_Assesment_Auditor { get; set; }
        public List<sp_Get_Cate_For_Lang_OneResult> list_category { get; set; }
        public List<sp_Assesment_Leader_ViewResult> sp_Assesment_Leader_View { get; set; }
        public List<sp_Assesment_Leader_ViewResult> sp_Assesment_Leader_View_By_Auditor { get; set; }
        public List<sp_Assesment_Leader_FillterResult> sp_Assesment_Leader_Fillter { get; set; }
        public string ItemCount { get; set; }
        public bool CheckItem { get; set; }
        public List<sp_List_Manage_ViewResult> sp_List_Manage_View { get; set; }
        public List<sp_Manage_View_Status_Open_DayResult> sp_Manage_View_Status_Open_Day { get; set; }
    }
    public class AssesmentTemp
    {
        public int Img { get; set; }
        public int Dis { get; set; }
        public string Cate { get; set; }
        public string Route { get; set; }
        public string Date { get; set; }
        public string Node { get; set; }
    }
}