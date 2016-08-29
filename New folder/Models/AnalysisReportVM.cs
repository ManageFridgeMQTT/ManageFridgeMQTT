using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eRoute.Models.ViewModel
{
    public class AnalysisReportVM
    {
        public List<sp_Get_SF_Permission_By_LoginResult> sp_Get_SF_Permission_By_Login { get; set; }
        public List<sp_Get_SF_Permission_By_LoginResult> sp_Get_SF_Permission_By_Branch { get; set; }
        public List<sp_Get_SF_Permission_By_LoginResult> sp_Get_SF_Permission_By_Region { get; set; }
        public List<sp_Get_SF_Permission_By_LoginResult> sp_Get_SF_Permission_By_Area { get; set; }
        public List<sp_Get_SF_Permission_By_LoginResult> sp_Get_SF_Permission_By_Distributor { get; set; }
        public List<sp_Get_SF_Permission_By_LoginResult> sp_Get_SF_Permission_By_Route { get; set; }
        public List<sp_GetProductHierarchyResult> sp_GetProductHierarchyDis { get; set; }
        public List<sp_GetOutletHierachyResult> sp_GetOutletHierachyDis { get; set; }
        public List<sp_GetProductHierarchyResult> sp_GetProductHierarchy { get; set; }
        public List<sp_GetOutletHierachyResult> sp_GetOutletHierachy { get; set; }
        public List<sp_GetBrandByNumbericResult> sp_GetBrandByNumberic { get; set; }
        public List<sp_GetBrandByNumbericResult> sp_GetBrandByNumbericSub { get; set; }
    }
}