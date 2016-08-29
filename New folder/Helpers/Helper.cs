using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using eRoute.ACModels;
using eRoute.Models;
using eRoute.Models.ViewModel;
using WebMatrix.WebData;
using System.Data.SqlClient;
using System.Data;


namespace DMSERoute.Helpers
{
    #region Cache
    public static class CacheDataHelper
    {
        public static int CacheMasterDataMinute = Utility.IntParse(ConfigurationSettings.AppSettings["CacheMasterDataMinute"]);
        public static int CacheTransDataMinute = Utility.IntParse(ConfigurationSettings.AppSettings["CacheTransDataMinute"]);

        public static DateTime DashBoardDate = Utility.DateTimeParse(ConfigurationSettings.AppSettings["DashBoard"]) == DateTime.MinValue ? DateTime.Today : Utility.DateTimeParse(ConfigurationSettings.AppSettings["DashBoard"]);

        public static int CommanyID
        {
            get
            {
                var row = Global.Context.CustomSettings.Where(x => x.SettingCode == "CompanyID" && x.SettingGroup == "CompanyID").FirstOrDefault();
                if(row != null)
                {
                    
                    return int.Parse(row.SettingValue);
                }
                else
                {
                    return 0;
                }
            }
        }

        #region Master Data
        public static List<Distributor> CacheListDistributor()
        {
            List<Distributor> list;
            if (!CacheHelperDynamic.Exists("CacheListDistributor"))
            {
                list = Global.Context.Distributors.ToList();
                CacheHelperDynamic.Add<List<Distributor>>(list, "CacheListDistributor", CacheMasterDataMinute);
            }
            else
            {
                list = CacheHelperDynamic.Get<List<Distributor>>("CacheListDistributor");
            }
            return list;
        }

        public static List<DMSRegion> CacheListDMSRegion()
        {
            var cc = HttpRuntime.Cache.Get("CacheListDMSRegion");
            List<DMSRegion> t = new List<DMSRegion>();
            if (cc == null)
            {
                t = Global.Context.DMSRegions.ToList();
                HttpRuntime.Cache.Insert("CacheListDMSRegion", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<DMSRegion>)cc;
            }
            return t;
        }
        public static List<DMSArea> CacheListDMSArea()
        {
            var cc = HttpRuntime.Cache.Get("CacheListDMSArea");
            List<DMSArea> t = new List<DMSArea>();
            if (cc == null)
            {
                t = Global.Context.DMSAreas.ToList();
                HttpRuntime.Cache.Insert("CacheListDMSArea", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<DMSArea>)cc;
            }
            return t;
        }

        public static List<DMSProvince> CacheListDMSProvince()
        {
            var cc = HttpRuntime.Cache.Get("CacheListDMSProvince");
            List<DMSProvince> t = new List<DMSProvince>();
            if (cc == null)
            {
                t = Global.Context.DMSProvinces.ToList();
                HttpRuntime.Cache.Insert("CacheListDMSProvince", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<DMSProvince>)cc;
            }
            return t;
        }

        public static List<DMSSalesForce> CacheListDMSSalesForce()
        {
            var cc = HttpRuntime.Cache.Get("CacheListDMSSalesForce");
            List<DMSSalesForce> t = new List<DMSSalesForce>();
            if (cc == null)
            {
                t = Global.Context.DMSSalesForces.ToList();
                HttpRuntime.Cache.Insert("CacheListDMSSalesForce", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<DMSSalesForce>)cc;
            }
            return t;
        }

        public static List<DMSSFAssignment> CacheListDMSSFAssignment()
        {
            var cc = HttpRuntime.Cache.Get("CacheListDMSSFAssignment");
            List<DMSSFAssignment> t = new List<DMSSFAssignment>();
            if (cc == null)
            {
                t = Global.Context.DMSSFAssignments.ToList();
                HttpRuntime.Cache.Insert("CacheListDMSSFAssignment", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<DMSSFAssignment>)cc;
            }
            return t;
        }

        public static List<DMSSFHierarchy> CacheListDMSSFHierarchy()
        {
            var cc = HttpRuntime.Cache.Get("CacheListDMSSFHierarchy");
            List<DMSSFHierarchy> t = new List<DMSSFHierarchy>();
            if (cc == null)
            {
                t = Global.Context.DMSSFHierarchies.ToList();
                HttpRuntime.Cache.Insert("CacheListDMSSFHierarchy", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<DMSSFHierarchy>)cc;
            }
            return t;
        }

        public static List<Route> CacheListRoute()
        {
            var cc = HttpRuntime.Cache.Get("CacheListRoute");
            List<Route> t = new List<Route>();
            if (cc == null || ((List<Route>)cc).Count == 0)
            {
                t = Global.Context.Routes.ToList();
                HttpRuntime.Cache.Insert("CacheListRoute", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<Route>)cc;
            }
            return t;
        }

        public static List<Outlet> CacheListOutlet()
        {
            var cc = HttpRuntime.Cache.Get("CacheListOutlet");
            List<Outlet> t = new List<Outlet>();
            if (cc == null)
            {
                t = Global.Context.Outlets.ToList();
                HttpRuntime.Cache.Insert("CacheListOutlet", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<Outlet>)cc;
            }
            return t;
        }

        public static List<Salesman> CacheListSalesman()
        {
            var cc = HttpRuntime.Cache.Get("CacheListSalesman");
            List<Salesman> t = new List<Salesman>();
            if (cc == null)
            {
                t = Global.Context.Salesmans.ToList();
                HttpRuntime.Cache.Insert("CacheListSalesman", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<Salesman>)cc;
            }
            return t;
        }

        public static List<VisitPlanHistory> CacheVisitPlanHistory()
        {
            var cc = HttpRuntime.Cache.Get("CacheVisitPlanHistory");
            List<VisitPlanHistory> t = new List<VisitPlanHistory>();
            if (cc == null)
            {
                t = Global.Context.VisitPlanHistories.Where(x=>x.VisitDate.Month == CacheDataHelper.DashBoardDate.Month).ToList();
                HttpRuntime.Cache.Insert("CacheVisitPlanHistory", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<VisitPlanHistory>)cc;
            }
            return t;
        }

        public static List<UserTerritory> CacheListUserTerritory()
        {
            var cc = HttpRuntime.Cache.Get("CacheListUserTerritory");
            List<UserTerritory> t = new List<UserTerritory>();
            if (cc == null || ((List<UserTerritory>)cc).Count == 0)
            {
                t = Global.Context.UserTerritories.ToList();
                HttpRuntime.Cache.Insert("CacheListUserTerritory", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<UserTerritory>)cc;
            }
            return t;
        }

        public static List<RoleUser> CacheListRoleUser()
        {
            var cc = HttpRuntime.Cache.Get("CacheListRoleUser");
            List<RoleUser> t = new List<RoleUser>();
            if (cc == null)
            {
                t = Global.Context.RoleUsers.ToList();
                HttpRuntime.Cache.Insert("CacheListRoleUser", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<RoleUser>)cc;
            }
            return t;
        }

        public static List<Role> CacheListRole()
        {
            var cc = HttpRuntime.Cache.Get("CacheListRole");
            List<Role> t = new List<Role>();
            if (cc == null)
            {
                t = Global.Context.Roles.ToList();
                HttpRuntime.Cache.Insert("CacheListRole", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<Role>)cc;
            }
            return t;
        }

        public static List<RoleFeature> CacheListRoleFeature()
        {
            var cc = HttpRuntime.Cache.Get("CacheListRoleFeature");
            List<RoleFeature> t = new List<RoleFeature>();
            if (cc == null)
            {
                t = Global.Context.RoleFeatures.ToList();
                HttpRuntime.Cache.Insert("CacheListRoleFeature", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<RoleFeature>)cc;
            }
            return t;
        }

        public static List<Feature> CacheListFeature()
        {
            var cc = HttpRuntime.Cache.Get("CacheListFeature");
            List<Feature> t = new List<Feature>();
            if (cc == null)
            {
                t = Global.Context.Features.ToList();
                HttpRuntime.Cache.Insert("CacheListFeature", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<Feature>)cc;
            }
            return t;
        }

        #endregion

        #region SalesData
        public static List<pp_ReportDashBoardResult> CacheSalesAssessmentResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheSalesAssessmentResult");
            List<pp_ReportDashBoardResult> t = new List<pp_ReportDashBoardResult>();
            if (cc == null || ((List<pp_ReportDashBoardResult>)cc).Count == 0)
            {
                //t = Global.Context.pp_ReportSalesAssessment(DashBoardDate, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, "admin").ToList();
                t = Global.Context.pp_ReportDashBoard(DashBoardDate, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty).ToList();
                HttpRuntime.Cache.Insert("CacheSalesAssessmentResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<pp_ReportDashBoardResult>)cc;
            }
            return t;
        }
        public static List<pp_GetReportOrderIndexLevelResult> CacheGetReportOrderIndexLevelResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheGetReportOrderIndexLevelResult");
            List<pp_GetReportOrderIndexLevelResult> t = new List<pp_GetReportOrderIndexLevelResult>();
            if (cc == null)
            {
                t = Global.Context.pp_GetReportOrderIndexLevel(DashBoardDate, "admin").ToList();
                HttpRuntime.Cache.Insert("CacheGetReportOrderIndexLevelResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<pp_GetReportOrderIndexLevelResult>)cc;
            }
            return t;
        }
        public static List<SalesTargetKPI> ListSalesTargetKPIInMonth()
        {
            var cc = HttpRuntime.Cache.Get("ListSalesTargetKPIInMonth");
            List<SalesTargetKPI> t = new List<SalesTargetKPI>();
            if (cc == null)
            {
                DateTime today = CacheDataHelper.DashBoardDate;
                t = Global.Context.SalesTargetKPIs.Where(x =>
                    x.YearNbr == today.Year
                    && x.MonthNbr == today.Month
                    && x.Type.Equals("Month")
                    ).ToList();
                HttpRuntime.Cache.Insert("ListSalesTargetKPIInMonth", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<SalesTargetKPI>)cc;
            }
            return t;
        }
        
        public static List<pp_GetSalesmanSyncNSalesTimeResult> CacheSalesmanSyncNSalesTimeResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheSalesmanSyncNSalesTimeResult");
            List<pp_GetSalesmanSyncNSalesTimeResult> t = new List<pp_GetSalesmanSyncNSalesTimeResult>();

            if (cc == null)
            {
                TimeSpan syncTime = Utility.DateTimeParse(ConfigurationSettings.AppSettings["SMSyncTime"]).TimeOfDay;
                TimeSpan firstVisitTime = Utility.DateTimeParse(ConfigurationSettings.AppSettings["SMFirstVisitTime"]).TimeOfDay;
                t = Global.Context.pp_GetSalesmanSyncNSalesTime(DashBoardDate, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, syncTime, firstVisitTime, "admin").ToList();
                HttpRuntime.Cache.Insert("CacheSalesmanSyncNSalesTimeResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<pp_GetSalesmanSyncNSalesTimeResult>)cc;
            }
            return t;
        }
        public static List<pp_GetReportVisitInMonthResult> CacheReportVisitInMonth()
        {
            var cc = HttpRuntime.Cache.Get("CacheReportVisitInMonth");
            List<pp_GetReportVisitInMonthResult> t = new List<pp_GetReportVisitInMonthResult>();
            //if (cc == null)
            {
                t = Global.Context.pp_GetReportVisitInMonth(DashBoardDate.FirstDayOfMonth(), DashBoardDate).ToList();
                HttpRuntime.Cache.Insert("CacheReportVisitInMonth", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            //else
            //{
            //    t = (List<pp_GetReportVisitInMonthResult>)cc;
            //}
            return t;
        }
        public static List<pp_GetSyncBaselineResult> CacheSyncVisitInMonth()
        {
            var cc = HttpRuntime.Cache.Get("CacheSyncVisitInMonth");
            List<pp_GetSyncBaselineResult> t = new List<pp_GetSyncBaselineResult>();
            if (cc == null)
            {
                t = Global.Context.pp_GetSyncBaseline(DashBoardDate.FirstDayOfMonth(), DashBoardDate).ToList();
                HttpRuntime.Cache.Insert("CacheSyncVisitInMonth", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<pp_GetSyncBaselineResult>)cc;
            }
            return t;
        }

        public static List<pp_GetBaselineTargetInMonthResult> CacheBaselineTargetInMonth()
        {
            var cc = HttpRuntime.Cache.Get("CacheBaselineTargetInMonth");
            List<pp_GetBaselineTargetInMonthResult> t = new List<pp_GetBaselineTargetInMonthResult>();
            if (cc == null)
            {
                t = Global.Context.pp_GetBaselineTargetInMonth(DashBoardDate.FirstDayOfMonth(), DashBoardDate).ToList();
                HttpRuntime.Cache.Insert("CacheBaselineTargetInMonth", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<pp_GetBaselineTargetInMonthResult>)cc;
            }
            return t;
        }

        public static List<pp_GetBaselineTargetInDayResult> CacheBaselineTargetInDay()
        {
            var cc = HttpRuntime.Cache.Get("CacheBaselineTargetInDay");
            List<pp_GetBaselineTargetInDayResult> t = new List<pp_GetBaselineTargetInDayResult>();
            if (cc == null)
            {
                t = Global.Context.pp_GetBaselineTargetInDay(DashBoardDate).ToList();
                HttpRuntime.Cache.Insert("CacheBaselineTargetInDay", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<pp_GetBaselineTargetInDayResult>)cc;
            }
            return t;
        }

        public static List<usp_GetListImageByResult> CacheGetListImageByEval(string sEvalID, string auditor = "")
        {
            var cc = HttpRuntime.Cache.Get("CacheGetListImageByEval");
            List<usp_GetListImageByResult> t = new List<usp_GetListImageByResult>();
            //if (cc == null)
            {
                if (!string.IsNullOrEmpty(auditor))
                {
                    t = Global.VisibilityContext.usp_GetListImageBy(sEvalID, "", auditor, -1, -1, -1, 1).ToList();
                }
                else if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.Auditor.ToString()))
                {
                    t = Global.VisibilityContext.usp_GetListImageBy(sEvalID, "", SessionHelper.GetSession<string>("UserName"), -1, -1, -1, 0).ToList();
                }
                else
                {
                    t = Global.VisibilityContext.usp_GetListImageBy(sEvalID, "", "", -1, -1, -1, 0).ToList();
                }
                
                HttpRuntime.Cache.Insert("CacheGetListImageByEval", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            //else
            //{
            //    t = (List<usp_GetListImageByResult>)cc;
            //}
            return t;
        }

        public static List<usp_GetEvaluationInfoByTypeResult> CacheGetAllEvaluation()
        {
            //var cc = HttpRuntime.Cache.Get("CacheGetAllEvaluation");
            List<usp_GetEvaluationInfoByTypeResult> t = new List<usp_GetEvaluationInfoByTypeResult>();
            //if (cc == null)
            //{
            //    t = Global.VisibilityContext.usp_GetEvaluationInfoByType("All", "").ToList();
            //    HttpRuntime.Cache.Insert("CacheGetAllEvaluation", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            //}
            //else
            //{
            //    t = (List<usp_GetEvaluationInfoByTypeResult>)cc;
            //}
            t = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "", "", "").ToList();
            return t;
        }
        #endregion

        #region VisitData
        public static DataTable CacheASMVisitInfoResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheASMVisitInfoResult");
            DataTable t = new DataTable();
            if (cc == null)
            {
                // t = Global.Context.pp_GetASMVisitInfo(string.Empty, string.Empty, string.Empty, DashBoardDate, "admin").ToList();
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("pp_GetASMVisitInfo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;
                        //// Add parameters
                        cmd.Parameters.AddWithValue("@RegionID", "");
                        cmd.Parameters.AddWithValue("@AreaID", "");
                        cmd.Parameters.AddWithValue("@ASMID", "");
                        cmd.Parameters.AddWithValue("@VisitDate", DashBoardDate);
                        cmd.Parameters.AddWithValue("@UserName", "admin");
                        var adapt = new SqlDataAdapter(cmd);
                        adapt.Fill(t);
                    }
                }
                HttpRuntime.Cache.Insert("CacheASMVisitInfoResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (DataTable)cc;
            }
            return t;
        }
        
        public static DataTable CacheSUPVisitInfoResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheSUPVisitInfoResult");
            DataTable t = new DataTable();
            if (cc == null)
            {
                // t = Global.Context.pp_GetSUPVisitInfo(string.Empty, string.Empty, string.Empty, string.Empty, 0, DashBoardDate, "admin").ToList();
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("pp_GetSUPVisitInfo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;
                        // Add parameters
                        cmd.Parameters.AddWithValue("@RegionID", string.Empty);
                        cmd.Parameters.AddWithValue("@AreaID", string.Empty);
                        cmd.Parameters.AddWithValue("@ASMID", string.Empty);
                        cmd.Parameters.AddWithValue("@SaleSupID", string.Empty);
                        cmd.Parameters.AddWithValue("@DistributorID", 0);
                        cmd.Parameters.AddWithValue("@VisitDate", DashBoardDate);
                        cmd.Parameters.AddWithValue("@UserName", "admin");
                        var adapt = new SqlDataAdapter(cmd);
                        adapt.Fill(t);
                    }
                }
                HttpRuntime.Cache.Insert("CacheSUPVisitInfoResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (DataTable)cc;
            }
            return t;
        }

        public static List<pp_GetSalemanLastLocationResult> CacheSalemanLastLocationResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheSalemanLastLocationResult");
            List<pp_GetSalemanLastLocationResult> t = new List<pp_GetSalemanLastLocationResult>();
            if (cc == null)
            {
                t = Global.Context.pp_GetSalemanLastLocation("admin", string.Empty, 0, string.Empty, DashBoardDate).ToList();
                HttpRuntime.Cache.Insert("CacheSalemanLastLocationResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<pp_GetSalemanLastLocationResult>)cc;
            }
            return t;
        }
        public static List<pp_ReportVisitResult> CacheReportVisitResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheReportVisitResult");
            List<pp_ReportVisitResult> t = new List<pp_ReportVisitResult>();
            if (cc == null)
            {
                t = Global.Context.pp_ReportVisit(DashBoardDate, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "admin").ToList();
                HttpRuntime.Cache.Insert("CacheReportVisitResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<pp_ReportVisitResult>)cc;
            }
            return t;
        }
        public static List<pp_ReportOrderIndexLevelResult> CacheSMOrderLevelResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheSMOrderLevelResult");
            List<pp_ReportOrderIndexLevelResult> t = new List<pp_ReportOrderIndexLevelResult>();
            //if (cc == null)
            //{
                string username = SessionHelper.GetSession<string>("UserName");
                t = Global.Context.pp_ReportOrderIndexLevel(DashBoardDate.FirstDayOfMonth(), DashBoardDate, username, 1).ToList();
                HttpRuntime.Cache.Insert("CacheSMOrderLevelResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            //}
            //else
            //{
            //    t = (List<pp_ReportOrderIndexLevelResult>)cc;
            //}
            return t;
        }
        public static List<pp_ReportOrderIndexLevelResult> CacheSMVisitLevelResult()
        {
            var cc = HttpRuntime.Cache.Get("CacheSMVisitLevelResult");
            List<pp_ReportOrderIndexLevelResult> t = new List<pp_ReportOrderIndexLevelResult>();
            //if (cc == null)
            //{
                string username = SessionHelper.GetSession<string>("UserName");
                t = Global.Context.pp_ReportOrderIndexLevel(DashBoardDate.FirstDayOfMonth(), DashBoardDate, username, 0).ToList();
                HttpRuntime.Cache.Insert("CacheSMVisitLevelResult", t, null, DateTime.UtcNow.AddMinutes(CacheTransDataMinute), Cache.NoSlidingExpiration);
            //}
            //else
            //{
            //    t = (List<pp_ReportOrderIndexLevelResult>)cc;
            //}
            return t;
        }
        #endregion

        public static List<pp_GetListMenuResult> CacheListMenu()
        {
            var cc = HttpRuntime.Cache.Get("CacheListMenu");
            List<pp_GetListMenuResult> t = new List<pp_GetListMenuResult>();
            if (cc == null)
            {
                using (ERouteDataContext db = new ERouteDataContext())
                {
                    t = db.pp_GetListMenu(SessionHelper.GetSession<int>("RoleUser"), Utils.CurrentLanguage).ToList();
                }
                HttpRuntime.Cache.Insert("CacheListMenu", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
                HttpRuntime.Cache.Insert("CacheRoleID", SessionHelper.GetSession<int>("RoleUser"), null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                var roleIDCache = HttpRuntime.Cache.Get("CacheRoleID");
                int roleID;
                if (roleIDCache == null)
                {
                    roleID = SessionHelper.GetSession<int>("RoleUser");
                    HttpRuntime.Cache.Insert("CacheRoleID", roleID, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
                }
                else {
                    roleID = (int)(HttpRuntime.Cache.Get("CacheRoleID"));
                }

                if (SessionHelper.GetSession<int>("RoleUser") != roleID)
                {
                    HttpRuntime.Cache.Remove("CacheListMenu");
                    HttpRuntime.Cache.Remove("CacheRoleID");
                    using (ERouteDataContext db = new ERouteDataContext())
                    {
                        t = db.pp_GetListMenu(SessionHelper.GetSession<int>("RoleUser"), Utils.CurrentLanguage).ToList();
                    }
                    HttpRuntime.Cache.Insert("CacheListMenu", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
                    HttpRuntime.Cache.Insert("CacheRoleID", SessionHelper.GetSession<int>("RoleUser"), null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
                }
                else
                {
                    t = (List<pp_GetListMenuResult>)cc;
                }
            }
            return t;
        }
        public static List<Language> CacheLanguages()
        {
            var cc = HttpRuntime.Cache.Get("CacheLanguages");
            List<Language> t = new List<Language>();
            if (cc == null)
            {
                using (ERouteDataContext db = new ERouteDataContext())
                {
                    t = Global.Context.Languages.ToList();
                }
                HttpRuntime.Cache.Insert("CacheLanguages", t, null, DateTime.UtcNow.AddMinutes(CacheMasterDataMinute), Cache.NoSlidingExpiration);
            }
            else
            {
                t = (List<Language>)cc;
            }
            return t;
        }
    }

    public static class CacheHelperDynamic
    {
        public static int CacheDataMinute = 60;

        /// <summary>
        /// Insert value into the cache using
        /// appropriate name/value pairs
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="o">Item to be cached</param>
        /// <param name="key">Name of item</param>
        public static void Add<T>(T o, string key)
        {
            // NOTE: Apply expiration parameters as you see fit.
            // I typically pull from configuration file.

            // In this example, I want an absolute
            // timeout so changes will always be reflected
            // at that time. Hence, the NoSlidingExpiration.
            HttpContext.Current.Cache.Insert(
                key,
                o,
                null,
                DateTime.Now.AddMinutes(CacheDataMinute),
                System.Web.Caching.Cache.NoSlidingExpiration);
        }

        public static void Add<T>(T o, string key, int minute)
        {
            // NOTE: Apply expiration parameters as you see fit.
            // I typically pull from configuration file.

            // In this example, I want an absolute
            // timeout so changes will always be reflected
            // at that time. Hence, the NoSlidingExpiration.
            HttpContext.Current.Cache.Insert(
                key,
                o,
                null,
                DateTime.Now.AddMinutes(minute),
                System.Web.Caching.Cache.NoSlidingExpiration);
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        public static void Clear(string key)
        {
            HttpContext.Current.Cache.Remove(key);
        }

        /// <summary>
        /// Check for item in cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            return HttpContext.Current.Cache[key] != null;
        }

        /// <summary>
        /// Retrieve cached item
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Name of cached item</param>
        /// <param name="value">Cached value. Default(T) if 
        /// item doesn't exist.</param>
        /// <returns>Cached item as type</returns>
        //public static bool Get<T>(string key, out T value)
        //{
        //    try
        //    {
        //        if (!Exists(key))
        //        {
        //            value = default(T);
        //            return false;
        //        }

        //        value = (T)HttpContext.Current.Cache[key];
        //    }
        //    catch
        //    {
        //        value = default(T);
        //        return false;
        //    }

        //    return true;
        //}

        public static T Get<T>(string key)
        {
            T value;
            try
            {
                if (!Exists(key))
                {
                    value = default(T);
                    return value;
                }

                value = (T)HttpContext.Current.Cache[key];
            }
            catch
            {
                value = default(T);
                return value;
            }

            return value;
        }
    }
    #endregion

    #region class ControllerHelper
    public static class ControllerHelper
    {

        #region CustomSetting
        public static string valueCustomSetting(string SettingCode)
        {
            var customSetting = Global.Context.CustomSettings.Where(x => x.SettingCode == SettingCode).FirstOrDefault();
            if (customSetting != null)
            {
                return customSetting.SettingValue;
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region SaleForceGetData
        #region Session
        public static List<Distributor> ListDistributor
        {
            get
            {
                if (SessionHelper.GetSession<List<Distributor>>("ListDistributor") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<Distributor> listDistributor = new List<Distributor>();

                    listDistributor.AddRange((from d in CacheDataHelper.CacheListDistributor()
                                              join ut in CacheDataHelper.CacheListUserTerritory()
                                                  on d.DistributorID equals ut.DistributorID
                                              where ut.UserName.Trim().Equals(username)
                                              select d
                               ).Distinct().OrderBy(a => a.DistributorName).ToList());

                    //listDistributor.AddRange(CacheDataHelper.CacheListDistributor().OrderBy(a => a.DistributorName).ToList());

                    SessionHelper.SetSession("ListDistributor", listDistributor);
                }
                return SessionHelper.GetSession<List<Distributor>>("ListDistributor");
            }
            set
            {
                SessionHelper.SetSession("ListDistributor", value);
            }
        }

        public static List<DMSRegion> ListRegion
        {
            get
            {
                if (SessionHelper.GetSession<List<DMSRegion>>("ListRegion") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<DMSRegion> listRegion = new List<DMSRegion>();

                    listRegion.AddRange((from r in CacheDataHelper.CacheListDMSRegion()
                                         join ut in CacheDataHelper.CacheListUserTerritory()
                                          on r.RegionID equals ut.RegionID
                                         where ut.UserName == username
                                         select r
                                ).Distinct().OrderBy(a => a.Name).ToList());

                    SessionHelper.SetSession("ListRegion", listRegion);
                }
                return SessionHelper.GetSession<List<DMSRegion>>("ListRegion");
            }
            set
            {
                SessionHelper.SetSession("ListRegion", value);
            }
        }
        public static List<DMSArea> ListArea
        {
            get
            {
                if (SessionHelper.GetSession<List<DMSArea>>("ListArea") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<DMSArea> listArea = new List<DMSArea>();

                    listArea.AddRange((from a in CacheDataHelper.CacheListDMSArea()
                                       join ut in CacheDataHelper.CacheListUserTerritory()
                                        on a.AreaID equals ut.AreaID
                                       where ut.UserName.Equals(username)
                                       select a
                                ).Distinct().OrderBy(a => a.Name).ToList());
                    SessionHelper.SetSession("ListArea", listArea);
                }
                return SessionHelper.GetSession<List<DMSArea>>("ListArea");
            }
            set
            {
                SessionHelper.SetSession("ListArea", value);
            }
        }

        public static List<DMSProvince> ListProvince
        {
            get
            {
                if (SessionHelper.GetSession<List<DMSProvince>>("ListProvince") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<DMSProvince> listProvince = new List<DMSProvince>();

                    listProvince.AddRange((from a in CacheDataHelper.CacheListDMSProvince()
                                           join ut in CacheDataHelper.CacheListUserTerritory()
                                            on a.AreaID equals ut.AreaID
                                           where ut.UserName.Equals(username)
                                           select a
                                ).Distinct().OrderBy(a => a.Province).ToList());
                    SessionHelper.SetSession("ListProvince", listProvince);
                }
                return SessionHelper.GetSession<List<DMSProvince>>("ListProvince");
            }
            set
            {
                SessionHelper.SetSession("ListProvince", value);
            }
        }

        public static List<DMSSalesForce> ListSaleSup
        {
            get
            {
                if (SessionHelper.GetSession<List<DMSSalesForce>>("ListSaleSup") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<DMSSalesForce> listSalesForce = new List<DMSSalesForce>();
                    listSalesForce.AddRange((from r in CacheDataHelper.CacheListRoute()
                                             join ut in CacheDataHelper.CacheListUserTerritory()
                                                  on new { r.RouteID, r.DistributorID } equals new { ut.RouteID, ut.DistributorID }
                                             join sf in CacheDataHelper.CacheListDMSSalesForce()
                                                  on r.SalesSupID equals sf.EmployeeID
                                             where ut.UserName.Equals(username)
                                             select sf
                                ).Distinct().OrderBy(a => a.EmployeeName).ToList());
                    SessionHelper.SetSession("ListSaleSup", listSalesForce);
                }
                return SessionHelper.GetSession<List<DMSSalesForce>>("ListSaleSup");
            }
            set
            {
                SessionHelper.SetSession("ListSaleSup", value);
            }
        }

        public static List<DMSSalesForce> ListASM
        {
            get
            {
                if (SessionHelper.GetSession<List<DMSSalesForce>>("ListASM") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<DMSSalesForce> listSalesForce = new List<DMSSalesForce>();
                    listSalesForce.AddRange((from ut in CacheDataHelper.CacheListUserTerritory()
                                             join d in CacheDataHelper.CacheListDistributor()
                                                    on ut.DistributorID equals d.DistributorID
                                             join sfa in CacheDataHelper.CacheListDMSSFAssignment()
                                                    on d.AreaID equals sfa.AreaID
                                             join sf in CacheDataHelper.CacheListDMSSalesForce()
                                                    on sfa.EmployeeID equals sf.EmployeeID
                                             where
                                                ut.UserName.Equals(username)
                                                && sfa.ApplyTo == 'A'
                                                && sf.Active == true
                                                && sfa.IsActive == true
                                                && sf.SFLevel == 3
                                                && sfa.IsBaseAssignment == true
                                             select sf
                                ).Distinct().OrderBy(a => a.EmployeeName).ToList());
                    SessionHelper.SetSession("ListASM", listSalesForce);
                }
                return SessionHelper.GetSession<List<DMSSalesForce>>("ListASM");
            }
            set
            {
                SessionHelper.SetSession("ListASM", value);
            }
        }

        public static List<DMSSFAssignment> ListSFAssignment
        {
            get
            {
                if (SessionHelper.GetSession<List<DMSSFAssignment>>("ListSFAssignment") == null)
                {
                    List<DMSSFAssignment> listSFAssignment = new List<DMSSFAssignment>();
                    listSFAssignment.AddRange(CacheDataHelper.CacheListDMSSFAssignment().Where(a => a.IsActive == true).ToList());
                    SessionHelper.SetSession("ListSFAssignment", listSFAssignment);
                }
                return SessionHelper.GetSession<List<DMSSFAssignment>>("ListSFAssignment");
            }
            set
            {
                SessionHelper.SetSession("ListSFAssignment", value);
            }
        }

        public static List<Route> ListRoute
        {
            get
            {
                if (SessionHelper.GetSession<List<Route>>("ListRoute") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<Route> listRoute = new List<Route>();
                    listRoute.AddRange((from r in CacheDataHelper.CacheListRoute()
                                        join ut in CacheDataHelper.CacheListUserTerritory()
                                         on new { r.RouteID, r.DistributorID } equals new { ut.RouteID, ut.DistributorID }
                                        where ut.UserName.Trim().Equals(username)
                                        select r
                                ).Distinct().OrderBy(a => a.RouteID).ToList());
                    SessionHelper.SetSession("ListRoute", listRoute);
                }
                return SessionHelper.GetSession<List<Route>>("ListRoute");
            }
            set
            {
                SessionHelper.SetSession("ListRoute", value);
            }
        }

        public static List<Outlet> ListOutlet
        {
            get
            {
                if (SessionHelper.GetSession<List<Outlet>>("ListOutlet") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<Outlet> listOutlet = new List<Outlet>();
                    listOutlet.AddRange((from o in CacheDataHelper.CacheListOutlet()
                                         join vp in CacheDataHelper.CacheVisitPlanHistory()
                                         on new { o.DistributorID, o.OutletID} equals new { vp.DistributorID, vp.OutletID}
                                         join ut in CacheDataHelper.CacheListUserTerritory()
                                         on new { vp.RouteID, vp.DistributorID } equals new { ut.RouteID, ut.DistributorID }
                                        where ut.UserName.Trim().Equals(username)
                                        select o
                                ).Distinct().OrderBy(a => a.OutletID).ToList());
                    SessionHelper.SetSession("ListOutlet", listOutlet);
                }
                return SessionHelper.GetSession<List<Outlet>>("ListOutlet");
            }
            set
            {
                SessionHelper.SetSession("ListOutlet", value);
            }
        }

        public static List<VisitPlanHistory> VisitPlanHistory
        {
            get
            {
                if (SessionHelper.GetSession<List<VisitPlanHistory>>("VisitPlanHistory") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<VisitPlanHistory> listVisitPlanHistory = new List<VisitPlanHistory>();
                    listVisitPlanHistory.AddRange((from vp in CacheDataHelper.CacheVisitPlanHistory()
                                         join ut in CacheDataHelper.CacheListUserTerritory()
                                         on new { vp.RouteID, vp.DistributorID } equals new { ut.RouteID, ut.DistributorID }
                                         where ut.UserName.Trim().Equals(username)
                                         select vp
                                ).Distinct().OrderBy(a => a.OutletID).ToList());
                    SessionHelper.SetSession("VisitPlanHistory", listVisitPlanHistory);
                }
                return SessionHelper.GetSession<List<VisitPlanHistory>>("VisitPlanHistory");
            }
            set
            {
                SessionHelper.SetSession("VisitPlanHistory", value);
            }
        }

        public static List<Salesman> ListSalesman
        {
            get
            {
                if (SessionHelper.GetSession<List<Salesman>>("ListSalesman") == null)
                {
                    string username = SessionHelper.GetSession<string>("UserName");
                    List<Salesman> listSalesman = new List<Salesman>();
                    listSalesman.AddRange((from r in CacheDataHelper.CacheListRoute()
                                           join sm in CacheDataHelper.CacheListSalesman()
                                                //on new { r.SalesmanID, r.DistributorID } equals new { sm.SalesmanID, DistributorID = sm.DistributorID }
                                                on r.SalesmanID equals sm.SalesmanID
                                           join ut in CacheDataHelper.CacheListUserTerritory()
                                                //on new { r.RouteID, r.DistributorID } equals new { ut.RouteID, ut.DistributorID }
                                                on r.RouteID equals ut.RouteID
                                           where
                                                ut.UserName.Trim().Equals(username)
                                                && r.DistributorID == sm.DistributorID
                                                && r.DistributorID == ut.DistributorID
                                           select sm
                                ).Distinct().OrderBy(a => a.SalesmanName).ToList());
                    SessionHelper.SetSession("ListSalesman", listSalesman);
                }
                return SessionHelper.GetSession<List<Salesman>>("ListSalesman");
            }
            set
            {
                SessionHelper.SetSession("ListSalesman", value);
            }
        }

        public static List<Map_Salesman> ListMapSalesman(List<Salesman> listItem)
        {
            List<Map_Salesman> listSalesman = new List<Map_Salesman>();
            if (listItem != null && listItem.Count > 1)
            {
                listSalesman.AddRange((from s in listItem
                                       select new Map_Salesman()
                                       {
                                           SalesmanID = s.SalesmanID
                                           ,
                                           SalesmanName = s.SalesmanID + " - " + s.SalesmanName
                                       }).Distinct().OrderBy(a => a.SalesmanName).ToList());
            }

            return listSalesman;
        }

        public static List<DMSDisplay> ListDisplay
        {
            get
            {
                if (SessionHelper.GetSession<List<DMSDisplay>>("ListDisplay") == null)
                {
                    List<DMSDisplay> listDisplay = new List<DMSDisplay>();
                    // MUST FILTER USER CAN ACCESS THIS DISPLAY
                    listDisplay.AddRange(Global.Context.DMSDisplays.OrderBy(a => a.DisplayName).ToList());
                    SessionHelper.SetSession("ListDisplay", listDisplay);
                }
                return SessionHelper.GetSession<List<DMSDisplay>>("ListDisplay");
            }
            set { SessionHelper.SetSession("ListDisplay", value); }
        }
        //public static List<DMSEvalProgram> ListDisplayProgram
        //{
        //    get
        //    {
        //        if (SessionHelper.GetSession<List<DMSEvalProgram>>("ListDisplayProgram") == null)
        //        {
        //            List<DMSEvalProgram> listDisplay = new List<DMSEvalProgram>();
        //            // MUST FILTER USER CAN ACCESS THIS DISPLAY
        //            listDisplay.AddRange(Global.Context.DMSEvalPrograms.OrderBy(a => a.ProgramName).ToList());
        //            SessionHelper.SetSession("ListDisplayProgram", listDisplay);
        //        }
        //        return SessionHelper.GetSession<List<DMSEvalProgram>>("ListDisplayProgram");
        //    }
        //    set { SessionHelper.SetSession("ListDisplayProgram", value); }
        //}
        #endregion

        //public static List<DMSRegion> GetListRegion()
        //{
        //    return GetListRegion(string.Empty);
        //}

        public static List<DMSRegion> GetListRegion(string regionID = "")
        {
            List<DMSRegion> listItem = new List<DMSRegion>();
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                listItem = ControllerHelper.ListRegion.ToList();
            }
            else
            {
                listItem = ControllerHelper.ListRegion.Where(a => a.RegionID.Equals(regionID) || regionID == string.Empty).ToList();
            }
            if (listItem.Count > 1)
            {
                listItem.Insert(0, new DMSRegion() { RegionID = string.Empty, Name = string.Empty });
            }
            return listItem;
        }

        public static List<DMSArea> GetListArea(string regionID)
        {
            List<DMSArea> listItem = ControllerHelper.ListArea.Where(a => a.RegionID.Equals(regionID) || regionID == string.Empty).ToList();
            if (listItem.Count > 1)
            {
                listItem.Insert(0, new DMSArea() { AreaID = string.Empty, Name = string.Empty });
            }
            return listItem;
        }
        public static List<DMSProvince> GetListProvince(string regionID, string areaID)
        {
            List<DMSProvince> listItem = new List<DMSProvince>();

            if (areaID != string.Empty)
            {
                listItem = ControllerHelper.ListProvince.Where(a =>
                    (a.RegionID.Equals(regionID) || regionID == string.Empty)
                    && (a.AreaID.Equals(areaID) || areaID == string.Empty)
                    ).ToList();
            }
            if (listItem.Count > 1)
            {
                listItem.Insert(0, new DMSProvince() { ProvinceID = string.Empty, Province = string.Empty });
            }
            return listItem;
        }

        public static List<Distributor> GetListDistributor(string regionID, string areaID, string provinceID)
        {
            List<Distributor> listItem = new List<Distributor>();

            listItem = ControllerHelper.ListDistributor.Where(a =>
                (a.RegionID.Equals(regionID) || regionID == string.Empty)
                && (a.AreaID.Equals(areaID) || areaID == string.Empty)
                && (a.ProvinceID.Equals(provinceID) || provinceID == string.Empty)
                ).ToList();

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Distributor() { DistributorID = 0, DistributorCode = string.Empty, DistributorName = string.Empty });
            }

            return listItem;
        }

        public static List<Distributor> GetListDistributor(string regionID, string areaID, string provinceID, string salesupID)
        {
            List<Distributor> listItem = new List<Distributor>();

            //listItem = ControllerHelper.ListDistributor.Where(a =>
            //    (a.RegionID.Equals(regionID) || regionID == string.Empty)
            //    && (a.AreaID.Equals(areaID) || areaID == string.Empty)
            //    && (a.ProvinceID.Equals(provinceID) || provinceID == string.Empty)
            //    ).ToList();

            listItem = (from d in ControllerHelper.ListDistributor
                        join r in ControllerHelper.ListRoute
                            on d.DistributorID equals r.DistributorID
                        where
                            (d.RegionID.Equals(regionID) || regionID == string.Empty)
                            && (d.AreaID == null || d.AreaID.Equals(areaID) || areaID == string.Empty)
                            && (d.ProvinceID.Equals(provinceID) || provinceID == string.Empty)//
                            && (r.SalesSupID.Equals(salesupID) || salesupID == string.Empty)
                        //&& (d.DistributorID == distributorID || distributorID == 0)
                        orderby d.DistributorName
                        select d
                        ).Distinct().ToList();

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Distributor() { DistributorID = 0, DistributorCode = string.Empty, DistributorName = string.Empty });
            }

            return listItem;
        }
        public static List<Distributor> GetListDistributorWithRegionArea(string regionID, string areaID)
        {
            List<Distributor> listItem = new List<Distributor>();

            listItem = (from d in ControllerHelper.ListDistributor
                        join r in ControllerHelper.ListRoute
                            on d.DistributorID equals r.DistributorID
                        where
                            (d.RegionID.Equals(regionID) || regionID == string.Empty)
                            && (d.AreaID == null || d.AreaID.Equals(areaID) || areaID == string.Empty)
                        orderby d.DistributorName
                        select d
                        ).Distinct().ToList();

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Distributor() { DistributorID = 0, DistributorCode = string.Empty, DistributorName = string.Empty });
            }
            return listItem;
        }
        public static List<Distributor> GetListDistributor(string asmID, string ssID)
        {
            List<Distributor> listItem = new List<Distributor>();
            string areaID = string.Empty;
            if (!string.IsNullOrEmpty(asmID))
            {
                areaID = ControllerHelper.ListSFAssignment.Where(a => a.EmployeeID == asmID).FirstOrDefault().AreaID;
            }

            listItem.AddRange((from d in ControllerHelper.ListDistributor
                               join r in ControllerHelper.ListRoute
                               on d.DistributorID equals r.DistributorID
                               join sf in ControllerHelper.ListSaleSup
                               on r.SalesSupID equals sf.EmployeeID
                               where
                                    (r.SalesSupID == ssID || ssID == string.Empty)
                                    && (d.AreaID.Equals(areaID) || areaID == string.Empty)
                               select d
                                ).Distinct().ToList());

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Distributor() { DistributorID = 0, DistributorCode = string.Empty, DistributorName = string.Empty });
            }
            return listItem;
        }

        public static List<DMSSalesForce> GetListSaleSup(string regionID, string areaID, string provinceID, int distributorID)
        {
            List<DMSSalesForce> listItem = new List<DMSSalesForce>();

            listItem = (from d in ControllerHelper.ListDistributor
                        join r in ControllerHelper.ListRoute
                            on d.DistributorID equals r.DistributorID
                        join sf in ControllerHelper.ListSaleSup
                            on r.SalesSupID equals sf.EmployeeID
                        where
                            (d.RegionID.Equals(regionID) || regionID == string.Empty)
                            && (d.AreaID.Equals(areaID) || areaID == string.Empty)
                            && (d.ProvinceID.Equals(provinceID) || provinceID == string.Empty)//
                            && (d.DistributorID == distributorID || distributorID == 0)
                        orderby sf.EmployeeName
                        select sf
                        ).Distinct().ToList();

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new DMSSalesForce() { EmployeeID = string.Empty, EmployeeName = string.Empty });
            }

            return listItem;
        }

        public static List<DMSSalesForce> GetListSaleSup(string asmID)
        {
            List<DMSSalesForce> listItem = new List<DMSSalesForce>();
            string areaID = string.Empty;
            if (!string.IsNullOrEmpty(asmID))
            {
                areaID = ControllerHelper.ListSFAssignment.Where(a => a.EmployeeID == asmID).FirstOrDefault().AreaID;
            }
            listItem.AddRange((from d in ControllerHelper.ListDistributor
                               join r in ControllerHelper.ListRoute
                                   on d.DistributorID equals r.DistributorID
                               join sf in ControllerHelper.ListSaleSup
                                   on r.SalesSupID equals sf.EmployeeID
                               where
                                   (d.AreaID.Equals(areaID) || areaID == string.Empty)
                               orderby sf.EmployeeName
                               select sf
                                ).Distinct().OrderBy(a => a.EmployeeName).ToList());

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new DMSSalesForce() { EmployeeID = string.Empty, EmployeeName = string.Empty });
            }

            return listItem;
        }
        public static List<Route> GetListRouteWithRegionAreaDis(string regionID, string areaID, int distributorID)
        {
            List<Route> listItem = new List<Route>();

            listItem = (from d in ControllerHelper.ListDistributor
                        join r in ControllerHelper.ListRoute
                            on d.DistributorID equals r.DistributorID
                        join sm in ControllerHelper.ListSalesman
                           on d.DistributorID  equals sm.DistributorID 
                        where
                            (d.RegionID.Equals(regionID) || regionID == string.Empty)
                            && (d.AreaID.Equals(areaID) || areaID == string.Empty)
                            && (d.DistributorID == distributorID || distributorID == 0)
                            && r.SalesmanID == sm.SalesmanID
                        orderby r.RouteID
                        select new Route(){
                            DistributorID = r.DistributorID,
                            RouteID = r.RouteCD,
                            RouteCD = r.RouteCD,
                            RouteName = sm.SalesmanName,
                            SalesmanID = r.SalesmanID,
                            SalesSupID = r.SalesSupID
                        }).GroupBy(p => p.RouteCD)
                          .Select(g => g.First()).Skip(0).Take(1000)
                          .ToList();

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Route() { RouteID = string.Empty, RouteCD = string.Empty, RouteName = string.Empty });
            }

            return listItem;
        }
        public static List<Route> GetListRoute(string regionID, string areaID, string provinceID, int distributorID, string salesupID)
        {
            List<Route> listItem = new List<Route>();

            listItem = (from d in ControllerHelper.ListDistributor
                        join r in ControllerHelper.ListRoute
                            on d.DistributorID equals r.DistributorID
                        join sm in ControllerHelper.ListSalesman
                           on d.DistributorID equals sm.DistributorID
                        where
                            (d.RegionID.Equals(regionID) || regionID == string.Empty)
                            && (d.AreaID.Equals(areaID) || areaID == string.Empty)
                            && (d.ProvinceID.Equals(provinceID) || provinceID == string.Empty)
                            && (d.DistributorID == distributorID || distributorID == 0)
                            && (r.SalesSupID.Equals(salesupID) || salesupID == string.Empty)
                            && r.SalesmanID == sm.SalesmanID
                        orderby r.RouteID
                        select new Route()
                        {
                            DistributorID = r.DistributorID,
                            RouteID = r.RouteCD,
                            RouteCD = r.RouteCD,
                            RouteName = sm.SalesmanName,
                            SalesmanID = r.SalesmanID,
                            SalesSupID = r.SalesSupID
                        }).GroupBy(p => p.RouteCD)
                          .Select(g => g.First())
                          .ToList();

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Route() { RouteID = string.Empty, RouteCD = string.Empty, RouteName = string.Empty });
            }

            return listItem;
        }

        public static List<Outlet> GetListOutlet(int distributorID, string routeID, string salesmanID)
        {
            List<Outlet> listItem = new List<Outlet>();

            listItem = (from o in ControllerHelper.ListOutlet
                        join vp in ControllerHelper.VisitPlanHistory
                        on new { o.DistributorID, o.OutletID } equals new { vp.DistributorID, vp.OutletID }
                        where(o.DistributorID == distributorID || distributorID == 0)
                            && (vp.RouteID == routeID || routeID == string.Empty)
                            && (vp.SalesmanID == salesmanID || salesmanID == string.Empty)
                        orderby o.OutletID
                        select o ).Skip(0).Take(1000).Distinct().ToList();

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Outlet() { OutletID = string.Empty, OutletName = string.Empty, Address = string.Empty });
            }

            return listItem;
        }
        public static List<Salesman> GetListSalesman(string regionID, string areaID, string provinceID, int distributorID, string salesupID)
        {
            List<Salesman> listItem = new List<Salesman>();

            listItem = (from d in ControllerHelper.ListDistributor
                        join r in ControllerHelper.ListRoute
                            on d.DistributorID equals r.DistributorID
                        join sm in ControllerHelper.ListSalesman
                            //on new { r.SalesmanID, r.DistributorID } equals new { sm.SalesmanID, DistributorID = sm.DistributorID }
                            on d.DistributorID equals sm.DistributorID
                        where
                            (d.RegionID.Equals(regionID) || regionID == string.Empty)
                            && (d.AreaID.Equals(areaID) || areaID == string.Empty)
                            && (d.ProvinceID.Equals(provinceID) || provinceID == string.Empty)
                            && (d.DistributorID == distributorID || distributorID == 0)
                            && (r.SalesSupID.Equals(salesupID) || salesupID == string.Empty)
                            && r.SalesmanID == sm.SalesmanID
                        orderby sm.SalesmanName
                        select sm).Distinct().ToList();

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Salesman() { SalesmanID = string.Empty, SalesmanName = string.Empty });
            }

            return listItem;
        }

        public static List<DMSSalesForce> GetListASM(string areaID, string provinceID, int distributorID, string salesupID)
        {
            List<DMSSalesForce> listItem = new List<DMSSalesForce>();
            listItem.AddRange((from d in ControllerHelper.ListDistributor
                               join sfa in ControllerHelper.ListSFAssignment
                                    on d.AreaID equals sfa.AreaID
                               join sf in ControllerHelper.ListASM
                                    on sfa.EmployeeID equals sf.EmployeeID
                               where
                                   (d.AreaID.Equals(areaID) || areaID == string.Empty)
                                   && (d.ProvinceID.Equals(provinceID) || provinceID == string.Empty)
                                   && (d.DistributorID == distributorID || distributorID == 0)
                                   && (sf.EmployeeID.Equals(salesupID) || salesupID == string.Empty)
                               orderby sf.EmployeeName
                               select sf).Distinct().ToList());

            if (listItem.Count > 1)
            {
                listItem.Insert(0, new DMSSalesForce() { EmployeeID = string.Empty, EmployeeName = string.Empty });
            }

            return listItem;
        }
        public static List<pp_ReportVisitResult> GetReportVisitResult(string regionID, string areaID,int distributorID, string salesupID, string salesmanID, string routeID, DateTime date)
        {
            string username = SessionHelper.GetSession<string>("UserName");

            if (date == CacheDataHelper.DashBoardDate)
            {
                var result = (from sm in CacheDataHelper.CacheReportVisitResult()
                              join ut in ControllerHelper.ListRoute
                              on new {df=  Utility.StringParse(sm.DistributorID), an = sm.RouteID, ui = sm.SalesmanID } equals new {df = Utility.StringParse(ut.DistributorID), an = ut.RouteID, ui = ut.SalesmanID }
                              where (sm.SaleSupID.Equals(salesupID) || salesupID == string.Empty)
                                     && (sm.DistributorID == distributorID || distributorID == 0)
                                     && (sm.SalesmanID.Equals(salesmanID) || salesmanID == string.Empty)
                                     && (sm.RegionID.Equals(regionID) || regionID == string.Empty)
                                      && (sm.AreaID.Equals(regionID) || areaID == string.Empty)
                              orderby sm.SalesmanName
                              select sm).Distinct().ToList();
                return result;
            }
            else
            {
                return Global.Context.pp_ReportVisit(date, regionID, areaID, string.Empty, distributorID, salesupID, routeID, salesmanID, string.Empty, string.Empty, username).ToList();
            }
        }
        public static List<pp_GetSalemanLastLocationResult> GetSalemanLastLocationResult(int distributorID, string salesupID, string salesmanID, DateTime date)
        {
            string username = SessionHelper.GetSession<string>("UserName");

            if (date == CacheDataHelper.DashBoardDate)
            {
                var result = (from sm in CacheDataHelper.CacheSalemanLastLocationResult()
                              join ut in ControllerHelper.ListRoute
                              on new { sm.DistributorID, sm.RouteID, sm.SalesmanID } equals new { ut.DistributorID, ut.RouteID, ut.SalesmanID }
                              where (sm.SaleSupID.Equals(salesupID) || salesupID == string.Empty)
                                     && (sm.DistributorID == distributorID || distributorID == 0)
                                     && (sm.SalesmanID.Equals(salesmanID) || salesmanID == string.Empty)
                              orderby sm.SalesmanName
                              select sm).Distinct().ToList();
                return result;
            }
            else
            {
                return Global.Context.pp_GetSalemanLastLocation(username, salesupID, distributorID, salesmanID, date).ToList();
            }
        }

        public static DataTable GetASMVisitInfoResult(string regionID, string areaID, string rsmID, string asmID, int distributorID, string salesupID, string salesmanID, string routeID, DateTime? date)
        {
            string username = SessionHelper.GetSession<string>("UserName");
            DataTable dt = new DataTable();
            if (date == CacheDataHelper.DashBoardDate)
            {
                var result = (from sm in CacheDataHelper.CacheASMVisitInfoResult().AsEnumerable()
                              join ut in ControllerHelper.ListASM
                              on new { ASMID = sm.Field<string>("ASMID") } equals new { ASMID = ut.EmployeeID }
                              where (sm.Field<string>("RegionID").Equals(regionID) || regionID == string.Empty)
                                     && (sm.Field<string>("AreaID").Equals(areaID) || areaID == string.Empty)
                                     && (sm.Field<string>("RSMID").Equals(rsmID) || rsmID == string.Empty)
                                     && (sm.Field<string>("ASMID").Equals(asmID) || asmID == string.Empty)
                                     && (sm.Field<string>("DistributorID").Equals(distributorID) || distributorID == 0)
                                     && (sm.Field<string>("SalesmanID").Equals(salesmanID) || salesmanID == string.Empty)
                              orderby sm.Field<string>("SalesmanName")
                              select sm);
                 if(result!=null && result.Count() >0)
                dt = result.CopyToDataTable();

                return dt;
            }
            else
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("pp_GetASMVisitInfo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;
                        //// Add parameters
                        cmd.Parameters.AddWithValue("@RegionID", regionID);
                        cmd.Parameters.AddWithValue("@AreaID", areaID);
                        cmd.Parameters.AddWithValue("@ASMID", asmID);
                        cmd.Parameters.AddWithValue("@VisitDate", date);
                        cmd.Parameters.AddWithValue("@UserName", username);
                        var adapt = new SqlDataAdapter(cmd);
                        adapt.Fill(dt);
                    }
                }
                return dt;
            }
        }

        public static DataTable GetSUPVisitInfoResult(string regionID, string areaID, string rsmID, string asmID, int? distributorID, string salesupID, string salesmanID, string routeID, DateTime date)
        {
            string username = SessionHelper.GetSession<string>("UserName");
            DataTable dt = new DataTable();
            if (date == CacheDataHelper.DashBoardDate)
            {
                //var result = (from sm in CacheDataHelper.CacheSUPVisitInfoResult()
                //              join ut in ControllerHelper.ListSaleSup
                //              on new { sm.SaleSupID } equals new { SaleSupID = ut.EmployeeID }
                //              where (sm.RegionID.Equals(regionID) || regionID == string.Empty)
                //                     && (sm.AreaID.Equals(areaID) || areaID == string.Empty)
                //                     && (sm.RSMID.Equals(rsmID) || rsmID == string.Empty)
                //                     && (sm.ASMID.Equals(asmID) || asmID == string.Empty)
                //                     && (sm.DistributorID == distributorID || distributorID == 0)
                //                     && (sm.SalesmanID.Equals(salesmanID) || salesmanID == string.Empty)
                //              orderby sm.SalesmanName
                //              select sm).Distinct().ToList();
                //return result;
                var result = (from sm in CacheDataHelper.CacheSUPVisitInfoResult().AsEnumerable()
                              join ut in ControllerHelper.ListASM
                              on new { SaleSupID = sm.Field<string>("SaleSupID") } equals new { SaleSupID = ut.EmployeeID }
                              where (sm.Field<string>("RegionID").Equals(regionID) || regionID == string.Empty)
                                     && (sm.Field<string>("AreaID").Equals(areaID) || areaID == string.Empty)
                                     && (sm.Field<string>("RSMID").Equals(rsmID) || rsmID == string.Empty)
                                     && (sm.Field<string>("ASMID").Equals(asmID) || asmID == string.Empty)
                                     && (sm.Field<string>("DistributorID").Equals(distributorID) || distributorID == 0)
                                     && (sm.Field<string>("SalesmanID").Equals(salesmanID) || salesmanID == string.Empty)
                              orderby sm.Field<string>("SalesmanName")
                              select sm);
                if (result != null && result.Count() > 0)
                dt = result.CopyToDataTable();

                return dt;
            }
            else
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("pp_GetSUPVisitInfo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RegionID", regionID);
                        cmd.Parameters.AddWithValue("@AreaID", areaID);
                        cmd.Parameters.AddWithValue("@ASMID", asmID);
                        cmd.Parameters.AddWithValue("@SaleSupID", salesupID);
                        cmd.Parameters.AddWithValue("@DistributorID", distributorID);
                        cmd.Parameters.AddWithValue("@VisitDate", date);
                        cmd.Parameters.AddWithValue("@UserName", username);
                        var adapt = new SqlDataAdapter(cmd);
                        adapt.Fill(dt);
                    }
                }
                return dt;
                //return Global.Context.pp_GetSUPVisitInfo(regionID, areaID, asmID, salesupID, distributorID, date, username).ToList();
            }
        }

        public static List<DashBoardGridData> GetSalesAssessment()
        {
            var model = (from sm in CacheDataHelper.CacheSalesAssessmentResult()
                                                          join ut in ControllerHelper.ListRoute
                                                              //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                                                          on sm.RouteID equals ut.RouteID
                                                          where
                                                              sm.DistributorID == ut.DistributorID
                                                              && sm.RouteID == ut.RouteID
                                                          select sm).Distinct().ToList();

            var result = new List<DashBoardGridData>();
            string groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }

            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(OrderCount) as OrderCount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited)");
            foreach (dynamic item in dynamicQuery)
            {
                var data = new DashBoardGridData()
                {
                    ID = item.ID,
                    Name = item.Name,
                    OutletMustVisit = item.OutletMustVisit,
                    OutletVisited = item.OutletVisited,
                    OrderCount = item.OrderCount,
                    TotalSKU = item.TotalSKU,
                    TotalQuantity = item.TotalQuantity,
                    TotalAmount = item.TotalAmount
                };

                data.LPPC = data.OrderCount > 0 && data.TotalSKU > 0 ? data.TotalSKU / data.OrderCount : 0;
                data.SO_MCP = data.OrderCount > 0 && data.OutletMustVisit > 0 ? data.OrderCount * 100 / data.OutletMustVisit : 0;
                data.Visit_MCP = data.OutletVisited > 0 && data.OutletMustVisit > 0 ? data.OutletVisited * 100 / data.OutletMustVisit : 0;

                data.strOutletMustVisit = Utility.StringParseWithRoundingDecimalDegit(data.OutletMustVisit);
                data.strOutletVisited = Utility.StringParseWithRoundingDecimalDegit(data.OutletVisited);
                data.strOrderCount = Utility.StringParseWithRoundingDecimalDegit(data.OrderCount);
                data.strTotalSKU = Utility.StringParseWithDecimalDegit(data.TotalSKU);
                data.strTotalQuantity = Utility.StringParseWithDecimalDegit(data.TotalQuantity);
                data.strTotalAmount = Utility.StringParseWithRoundingDecimalDegit(data.TotalAmount);
                data.strSO_MCP = Utility.StringParseWithDecimalDegit(data.SO_MCP);
                data.strVisit_MCP = Utility.StringParseWithDecimalDegit(data.Visit_MCP);
                data.strLPPC = Utility.StringParseWithDecimalDegit(data.LPPC);

                result.Add(data);
            }

            return result;
        }

        public static List<DashBoardGridData> GetSalesAssessmentMTD()
        {
            var model = (from sm in CacheDataHelper.CacheSalesAssessmentResult()
                                                          join ut in ControllerHelper.ListRoute
                                                              //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                                                          on sm.RouteID equals ut.RouteID
                                                          where
                                                              sm.DistributorID == ut.DistributorID
                                                              && sm.RouteID == ut.RouteID
                                                          select sm).Distinct().ToList();
            var result = new List<DashBoardGridData>();
            string groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }


            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(MTDTotalAmount) as TotalAmount, SUM(MTDOrderCount) as OrderCount, SUM(MTDTotalSKU) as TotalSKU, SUM(MTDTotalQuantity) as TotalQuantity, SUM(MTDOutletMustVisit) as OutletMustVisit, SUM(MTDOutletVisited) as OutletVisited)");
            foreach (dynamic item in dynamicQuery)
            {
                var data = new DashBoardGridData()
                {
                    ID = item.ID,
                    Name = item.Name,
                    OutletMustVisit = item.OutletMustVisit,
                    OutletVisited = item.OutletVisited,
                    OrderCount = item.OrderCount,
                    TotalSKU = item.TotalSKU,
                    TotalQuantity = item.TotalQuantity,
                    TotalAmount = item.TotalAmount
                };

                data.LPPC = data.OrderCount > 0 && data.TotalSKU > 0 ? data.TotalSKU / data.OrderCount : 0;
                data.SO_MCP = data.OrderCount > 0 && data.OutletMustVisit > 0 ? data.OrderCount * 100 / data.OutletMustVisit : 0;
                data.Visit_MCP = data.OutletVisited > 0 && data.OutletMustVisit > 0 ? data.OutletVisited * 100 / data.OutletMustVisit : 0;

                data.strOutletMustVisit = Utility.StringParseWithRoundingDecimalDegit(data.OutletMustVisit);
                data.strOutletVisited = Utility.StringParseWithRoundingDecimalDegit(data.OutletVisited);
                data.strOrderCount = Utility.StringParseWithRoundingDecimalDegit(data.OrderCount);
                data.strTotalSKU = Utility.StringParseWithDecimalDegit(data.TotalSKU);
                data.strTotalQuantity = Utility.StringParseWithDecimalDegit(data.TotalQuantity);
                data.strTotalAmount = Utility.StringParseWithRoundingDecimalDegit(data.TotalAmount);
                data.strSO_MCP = Utility.StringParseWithDecimalDegit(data.SO_MCP);
                data.strVisit_MCP = Utility.StringParseWithDecimalDegit(data.Visit_MCP);
                data.strLPPC = Utility.StringParseWithDecimalDegit(data.LPPC);

                result.Add(data);
            }

            return result;
        }

        public static List<DashBoardGridData> GetSalesmanSync()
        {
            List<pp_GetSalesmanSyncNSalesTimeResult> model = (from sm in CacheDataHelper.CacheSalesmanSyncNSalesTimeResult()
                                                              join ut in ControllerHelper.ListRoute
                                                                  //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                                                              on sm.RouteID equals ut.RouteID
                                                              where
                                                                  sm.DistributorID == ut.DistributorID
                                                                  && sm.RouteID == ut.RouteID
                                                              select sm).Distinct().ToList();
            var result = new List<DashBoardGridData>();
            string groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }


            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(CountFirstStartTimeAM) as OrderCount, SUM(CountSM) as OutletMustVisit, SUM(CountFirstSyncTime) as OutletVisited)");
            foreach (dynamic item in dynamicQuery)
            {
                var data = new DashBoardGridData()
                {
                    ID = item.ID,
                    Name = item.Name,
                    OutletMustVisit = item.OutletMustVisit,
                    OutletVisited = item.OutletVisited,
                    OrderCount = item.OrderCount,
                };

                data.strOutletMustVisit = Utility.StringParseWithRoundingDecimalDegit(data.OutletMustVisit);
                data.strOutletVisited = Utility.StringParseWithRoundingDecimalDegit(data.OutletVisited);
                data.strOrderCount = Utility.StringParseWithRoundingDecimalDegit(data.OrderCount);
                result.Add(data);
            }

            return result;
        }
        public static List<RenderDataHeatMap> GetQuantityCoverageProvince(string type, string dateSelected, string groupby, string categoryID, string itemID)
        {
          
            List<pp_GetDataRenderHeatMapResult> model = SessionHelper.GetSession<List<pp_GetDataRenderHeatMapResult>>("modelQC");
            var result = new List<RenderDataHeatMap>();
           if (groupby == "Province")
            {
                var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name, CategoryName, InventoryName)", "it")
                .Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name,  Key.CategoryName, Key.InventoryName, SUM(TotalOutlet) as TotalOutlet, SUM(Quantity) as TotalQuantity)");
                foreach (dynamic item in dynamicQuery)
                {
                    result.Add(new RenderDataHeatMap()
                    {
                        Level1 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        Category = item.CategoryName,
                        InventoryItem = item.InventoryName,
                        TotalOutlet = item.TotalOutlet,
                        TotalQuantity = item.TotalQuantity,
                        RatioQuantity = item.TotalOutlet > 0 ? ((item.TotalQuantity * 100) / item.TotalOutlet) : 0
                        //RatioQuantity = item.RatioQuantity
                    });
                }
            }           
            return result;
        }
        public static List<RenderDataHeatMap> GetQuantityCoverageTerritory(string type, string dateSelected, string groupby, string categoryID, string itemID)
        {
            List<pp_GetDataRenderHeatMapResult> model = SessionHelper.GetSession<List<pp_GetDataRenderHeatMapResult>>("modelQC");
            var result = new List<RenderDataHeatMap>();
            if (groupby == "Region")
            {
                var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name, CategoryName, InventoryName)", "it")
                .Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name,  Key.CategoryName, Key.InventoryName, SUM(TotalOutlet) as TotalOutlet, SUM(Quantity) as TotalQuantity)");
                foreach (dynamic item in dynamicQuery)
                {
                    result.Add(new RenderDataHeatMap()
                    {
                        Level1 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        Category = item.CategoryName,
                        InventoryItem = item.InventoryName,
                        TotalOutlet = item.TotalOutlet,
                        TotalQuantity = item.TotalQuantity,
                        RatioQuantity = item.TotalOutlet > 0 ? ((item.TotalQuantity * 100) / item.TotalOutlet) : 0
                        //RatioQuantity = item.RatioQuantity
                    });
                }
            }
            else if (groupby == "Area")
            {
                var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name, CategoryName, InventoryName)", "it")
                .Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name,  Key.CategoryName, Key.InventoryName,SUM(TotalOutlet) as TotalOutlet, SUM(Quantity) as TotalQuantity)");
                foreach (dynamic item in dynamicQuery)
                {
                    string id = item.ID.ToString();
                    string regionID = Global.Context.DMSAreas.Where(x => x.AreaID == id).Select(x => x.RegionID).FirstOrDefault();
                    string regionName = Global.Context.DMSRegions.Where(x => x.RegionID == regionID).Select(x => x.Name).FirstOrDefault();
                    result.Add(new RenderDataHeatMap()
                    {
                        Level1 = regionName,
                        Level2 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        Category = item.CategoryName,
                        InventoryItem = item.InventoryName,
                        TotalOutlet = item.TotalOutlet,
                        TotalQuantity = item.TotalQuantity,
                        RatioQuantity = item.TotalOutlet > 0 ? ((item.TotalQuantity * 100) / item.TotalOutlet) : 0
                        //RatioQuantity = item.RatioQuantity
                    });
                }
            }
            else if (groupby == "Distributor")
            {
                var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "Code, " + groupby + "Name, CategoryName, InventoryName)", "it")
                     .Select("new(Key." + groupby + "Code as ID, Key." + groupby + "Name as Name,  Key.CategoryName, Key.InventoryName, SUM(TotalOutlet) as TotalOutlet, SUM(Quantity) as TotalQuantity)");
                foreach (dynamic item in dynamicQuery)
                {
                    string id = item.ID.ToString();
                    Distributor distributors = Global.Context.Distributors.Where(x => x.DistributorCode == id).FirstOrDefault();
                    string areaName = Global.Context.DMSAreas.Where(x => x.AreaID == distributors.AreaID).Select(x => x.Name).FirstOrDefault();
                    string regionName = Global.Context.DMSRegions.Where(x => x.RegionID == distributors.RegionID).Select(x => x.Name).FirstOrDefault();
                    result.Add(new RenderDataHeatMap()
                    {
                        Level1 = regionName,
                        Level2 = areaName,
                        Level3 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        Category = item.CategoryName,
                        InventoryItem = item.InventoryName,
                        TotalOutlet = item.TotalOutlet,
                        TotalQuantity = item.TotalQuantity,
                        RatioQuantity = item.TotalOutlet > 0 ? ((item.TotalQuantity * 100) / item.TotalOutlet) : 0
                        //RatioQuantity = item.RatioQuantity
                    });
                }
            }
            else
            {
                var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name, CategoryName, InventoryName)", "it")
                .Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name,  Key.CategoryName, Key.InventoryName, SUM(TotalOutlet) as TotalOutlet, SUM(Quantity) as TotalQuantity)");
                foreach (dynamic item in dynamicQuery)
                {
                    string id = item.ID.ToString();
                    Route route = Global.Context.Routes.Where(x => x.RouteID == id).FirstOrDefault();
                    Distributor distributors = Global.Context.Distributors.Where(x => x.DistributorID == route.DistributorID).FirstOrDefault();
                    string areaName = Global.Context.DMSAreas.Where(x => x.AreaID == distributors.AreaID).Select(x => x.Name).FirstOrDefault();
                    string regionName = Global.Context.DMSRegions.Where(x => x.RegionID == distributors.RegionID).Select(x => x.Name).FirstOrDefault();
                    result.Add(new RenderDataHeatMap()
                    {
                        Level1 = regionName,
                        Level2 = areaName,
                        Level3 = distributors.DistributorName,
                        Level4 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        Category = item.CategoryName,
                        InventoryItem = item.InventoryName,
                        TotalOutlet = item.TotalOutlet,
                        TotalQuantity = item.TotalQuantity,
                        RatioQuantity = item.TotalOutlet > 0 ? ((item.TotalQuantity * 100) / item.TotalOutlet) : 0
                        //RatioQuantity = item.RatioQuantity
                    });
                }
            }
            return result;
        }
        public static List<ReportSMVisitSummaryChartData> GetRevenueProvince(string dateSelected, string groupby, string TerritoryID)
        {
            List<pp_ReportSalesAssessmentResult> model = SessionHelper.GetSession<List<pp_ReportSalesAssessmentResult>>("modelRev");
            var result = new List<ReportSMVisitSummaryChartData>();
            if (groupby == "Province")
            {
                var modelRegion = model.ToList();
                //var modelRegion = model.ToList();
                decimal targetRevenue = ControllerHelper.GetSaleTargetHeatMap(5, groupby, modelRegion);
                var dynamicQuery = modelRegion.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, SUM(OrderCount) as OrderCount)");
                foreach (dynamic item in dynamicQuery)
                {
                    result.Add(new ReportSMVisitSummaryChartData()
                    {
                        Level1 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        TargetRevenue = targetRevenue,
                        TotalAmount = item.TotalAmount,
                        RatioRevenue = targetRevenue > 0 ? ((item.TotalAmount * 100) / targetRevenue) : 0,
                        OrderCount = item.OrderCount,
                        TotalSKU = item.TotalSKU,
                        TotalQuantity = item.TotalQuantity,
                        LPPC = item.LPPC,
                        OutletMustVisit = item.OutletMustVisit,
                        OutletVisited = item.OutletVisited,
                        SOMCP = item.OutletMustVisit > 0 ? ((item.OrderCount * 100) / item.OutletMustVisit) : 0,
                        VisitMCP = item.OutletMustVisit > 0 ? ((item.OutletVisited * 100) / item.OutletMustVisit) : 0
                    });
                }
            }           
            return result;
        }
        public static List<ReportSMVisitSummaryChartData> GetRevenueTerritory(string dateSelected, string groupby, string TerritoryID)
        {
            List<pp_ReportSalesAssessmentResult> model = SessionHelper.GetSession<List<pp_ReportSalesAssessmentResult>>("modelRev");
            var result = new List<ReportSMVisitSummaryChartData>();
            if (groupby == "Province")
            {
                //var modelRegion = model.Where(x => x.RegionID == TerritoryID).ToList();
                var modelRegion = model.ToList();
                decimal targetRevenue = ControllerHelper.GetSaleTargetHeatMap(5, groupby, modelRegion);
                var dynamicQuery = modelRegion.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, SUM(OrderCount) as OrderCount)");
                foreach (dynamic item in dynamicQuery)
                {
                    result.Add(new ReportSMVisitSummaryChartData()
                    {
                        Level1 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        TargetRevenue = targetRevenue,
                        TotalAmount = item.TotalAmount,
                        RatioRevenue = targetRevenue > 0 ? ((item.TotalAmount * 100) / targetRevenue) : 0,
                        OrderCount = item.OrderCount,
                        TotalSKU = item.TotalSKU,
                        TotalQuantity = item.TotalQuantity,
                        LPPC = item.LPPC,
                        OutletMustVisit = item.OutletMustVisit,
                        OutletVisited = item.OutletVisited,
                        SOMCP = item.OutletMustVisit > 0 ? ((item.OrderCount * 100) / item.OutletMustVisit) : 0,
                        VisitMCP = item.OutletMustVisit > 0 ? ((item.OutletVisited * 100) / item.OutletMustVisit) : 0
                    });
                }
            }else
            if (groupby == "Region")
            {
                var modelRegion = model.Where(x => x.RegionID == TerritoryID).ToList();
                decimal targetRevenue = ControllerHelper.GetSaleTargetHeatMap(5, groupby, modelRegion);
                var dynamicQuery = modelRegion.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, SUM(OrderCount) as OrderCount)");
                foreach (dynamic item in dynamicQuery)
                {                       
                    result.Add(new ReportSMVisitSummaryChartData()
                    {
                        Level1 = item.Name ,                       
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        TargetRevenue = targetRevenue,
                        TotalAmount = item.TotalAmount,
                        RatioRevenue = targetRevenue > 0 ? ((item.TotalAmount * 100) / targetRevenue) :0,
                        OrderCount = item.OrderCount,
                        TotalSKU = item.TotalSKU,
                        TotalQuantity = item.TotalQuantity,
                        LPPC = item.LPPC,
                        OutletMustVisit = item.OutletMustVisit,
                        OutletVisited = item.OutletVisited,
                        SOMCP = item.OutletMustVisit > 0 ? ((item.OrderCount * 100) / item.OutletMustVisit) : 0,
                        VisitMCP = item.OutletMustVisit > 0 ? ((item.OutletVisited * 100) / item.OutletMustVisit) : 0
                    });
                }
            }
            else if (groupby == "Area")
            {
                var modelArea = model.Where(x => x.AreaID == TerritoryID).ToList();
                decimal targetRevenue = ControllerHelper.GetSaleTargetHeatMap(5, groupby, modelArea);
                var dynamicQuery = modelArea.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, SUM(OrderCount) as OrderCount)");
                foreach (dynamic item in dynamicQuery)
                {
                    string id = item.ID.ToString();
                    string regionID = Global.Context.DMSAreas.Where(x => x.AreaID == id).Select(x => x.RegionID).FirstOrDefault();
                    string regionName = Global.Context.DMSRegions.Where(x => x.RegionID == regionID).Select(x => x.Name).FirstOrDefault();
                    result.Add(new ReportSMVisitSummaryChartData()
                    {
                        Level1 = regionName,
                        Level2 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        TargetRevenue = targetRevenue,
                        TotalAmount = item.TotalAmount,
                        RatioRevenue = targetRevenue > 0 ? ((item.TotalAmount * 100) / targetRevenue):0,
                        OrderCount = item.OrderCount,
                        TotalSKU = item.TotalSKU,
                        TotalQuantity = item.TotalQuantity,
                        LPPC = item.LPPC,
                        OutletMustVisit = item.OutletMustVisit,
                        OutletVisited = item.OutletVisited,
                        SOMCP = item.OutletMustVisit > 0 ? ((item.OrderCount * 100) / item.OutletMustVisit) : 0,
                        VisitMCP = item.OutletMustVisit > 0 ? ((item.OutletVisited * 100) / item.OutletMustVisit) : 0
                    });
                }
            }
            else if (groupby == "Distributor")
            {
                var modelDistributor = model.Where(x => x.DistributorCode == TerritoryID).ToList();
                decimal targetRevenue = ControllerHelper.GetSaleTargetHeatMap(5, groupby, modelDistributor);
                var dynamicQuery = modelDistributor.AsQueryable().GroupBy("new(" + groupby + "Code, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Code as ID, Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, SUM(OrderCount) as OrderCount)");
                foreach (dynamic item in dynamicQuery)
                {
                    string id = item.ID.ToString();
                    Distributor distributors = Global.Context.Distributors.Where(x => x.DistributorCode == id).FirstOrDefault();
                    string areaName = Global.Context.DMSAreas.Where(x => x.AreaID == distributors.AreaID).Select(x => x.Name).FirstOrDefault();
                    string regionName = Global.Context.DMSRegions.Where(x => x.RegionID == distributors.RegionID).Select(x => x.Name).FirstOrDefault();
                    result.Add(new ReportSMVisitSummaryChartData()
                    {
                        Level1 = regionName,
                        Level2 = areaName,
                        Level3 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        TargetRevenue = targetRevenue,
                        TotalAmount = item.TotalAmount,
                        RatioRevenue = targetRevenue > 0 ? ((item.TotalAmount * 100) / targetRevenue):0,
                        OrderCount = item.OrderCount,
                        TotalSKU = item.TotalSKU,
                        TotalQuantity = item.TotalQuantity,
                        LPPC = item.LPPC,
                        OutletMustVisit = item.OutletMustVisit,
                        OutletVisited = item.OutletVisited,
                        SOMCP = item.OutletMustVisit > 0 ? ((item.OrderCount * 100) / item.OutletMustVisit) : 0,
                        VisitMCP = item.OutletMustVisit > 0 ? ((item.OutletVisited * 100) / item.OutletMustVisit) : 0
                    });
                }
            }
            else
            {
                var modelRoute = model.Where(x => x.RouteID == TerritoryID).ToList();
                decimal targetRevenue = ControllerHelper.GetSaleTargetHeatMap(5, groupby, modelRoute);
                var dynamicQuery = modelRoute.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, SUM(OrderCount) as OrderCount)");
                foreach (dynamic item in dynamicQuery)
                {
                    string id = item.ID.ToString();                  
                    Route route = Global.Context.Routes.Where(x => x.RouteID == id).FirstOrDefault();
                    Distributor distributors = Global.Context.Distributors.Where(x => x.DistributorID == route.DistributorID).FirstOrDefault();
                    string areaName = Global.Context.DMSAreas.Where(x => x.AreaID == distributors.AreaID).Select(x => x.Name).FirstOrDefault();
                    string regionName = Global.Context.DMSRegions.Where(x => x.RegionID == distributors.RegionID).Select(x => x.Name).FirstOrDefault();
                    result.Add(new ReportSMVisitSummaryChartData()
                    {
                        Level1 = regionName,
                        Level2 = areaName,
                        Level3 = distributors.DistributorName,
                        Level4 = item.Name,
                        Code = item.ID.ToString(),
                        Name = item.Name,
                        TargetRevenue = targetRevenue,
                        TotalAmount = item.TotalAmount,
                        RatioRevenue = targetRevenue > 0 ? ((item.TotalAmount * 100) / targetRevenue):0,
                        OrderCount = item.OrderCount,
                        TotalSKU = item.TotalSKU,
                        TotalQuantity = item.TotalQuantity,
                        LPPC = item.LPPC,
                        OutletMustVisit = item.OutletMustVisit,
                        OutletVisited = item.OutletVisited,
                        SOMCP = item.OutletMustVisit > 0 ? ((item.OrderCount * 100) / item.OutletMustVisit) : 0,
                        VisitMCP = item.OutletMustVisit > 0 ? ((item.OutletVisited * 100) / item.OutletMustVisit) : 0
                    });
                }
            }
            return result;
        }
        public static List<ReportSMVisitSummaryChartData> GetReportOrderIndexLevel()
        {
            var model = (from sm in CacheDataHelper.CacheSalesAssessmentResult()//CacheDataHelper.CacheGetReportOrderIndexLevelResult()
                         join ut in ControllerHelper.ListRoute
                             //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                         on sm.RouteID equals ut.RouteID
                         where
                             sm.DistributorID == ut.DistributorID
                             && sm.RouteID == ut.RouteID
                         select sm).Distinct().ToList();


            string groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }

            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, SUM(OrderCount) as OrderCount)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ReportSMVisitSummaryChartData()
                {
                    Code = item.ID,
                    Name = item.Name,
                    OutletMustVisit = item.OutletMustVisit,
                    OutletVisited = item.OutletVisited,
                    OrderCount = item.OrderCount,
                    SOMCP = item.OutletMustVisit > 0 ? ((item.OrderCount * 100) / item.OutletMustVisit) : 0,
                    VisitMCP = item.OutletMustVisit > 0 ? ((item.OutletVisited * 100) / item.OutletMustVisit) : 0
                });
            }

            return result;
        }

        public static decimal GetTargetBySalesTypeTargetType(string typeTarget)
        {
            #region ChartTarget
            string salesTeamType = Utility.Target.SM.ToString();
            decimal totalRevenue = 0;
            var listTargetKPI = (from sm in CacheDataHelper.ListSalesTargetKPIInMonth()
                                 join ut in ControllerHelper.ListRoute
                                 on new {p1 = sm.DistributorID, p2 = sm.SalesTeamID } equals new { p1 = ut.DistributorID, p2 = ut.SalesmanID }
                                 select sm).Distinct().ToList();
            var targetRevenueSalesman = (from s in listTargetKPI
                                         where s.SalesTeamType == salesTeamType && s.TypeTarget == typeTarget
                                         select s).ToList();
            totalRevenue = targetRevenueSalesman.Sum(x => x.Target.Value);
            return totalRevenue;
            #endregion
        }
        public static Dictionary<string, decimal> GetTargetDaySMByType(Utility.Target typeTarget)
        {
            #region ChartTarget
            string salesTeamType = Utility.Target.SM.ToString();
            Dictionary<string, decimal> listTargetSM = new Dictionary<string, decimal>();
            List<SalesTargetKPI> listTargetDayKPI = Global.Context.SalesTargetKPIs.Where(x => x.YearNbr == CacheDataHelper.DashBoardDate.Year && x.MonthNbr == CacheDataHelper.DashBoardDate.Month && x.Type == "Day").ToList();
            var listTargetKPI = (from sm in listTargetDayKPI
                                 join ut in ControllerHelper.ListRoute
                                 on new { p1 = sm.DistributorID, p2 = sm.SalesTeamID } equals new { p1 = ut.DistributorID, p2 = ut.SalesmanID }
                                 select sm).Distinct().ToList();
            var targetRevenueSalesman = (from s in listTargetKPI
                                         where s.SalesTeamType == salesTeamType && s.TypeTarget == typeTarget.ToString()
                                         select s).ToList();
            foreach (var elm in targetRevenueSalesman)
            {
                listTargetSM.Add(elm.SalesTeamID, elm.Target.Value);
            }

            return listTargetSM;
            #endregion
        }
        public static Dictionary<string, decimal> GetTargetMonthSMByType(Utility.Target typeTarget)
        {
            #region ChartTarget
            string salesTeamType = Utility.Target.SM.ToString();
            Dictionary<string, decimal> listTargetSM = new Dictionary<string, decimal>();
            var listTargetKPI = (from sm in CacheDataHelper.ListSalesTargetKPIInMonth()
                                 join ut in ControllerHelper.ListRoute
                                 on sm.SalesTeamID equals ut.SalesmanID
                                 select sm).Distinct().ToList();
            var targetRevenueSalesman = (from s in CacheDataHelper.ListSalesTargetKPIInMonth()
                                         where s.SalesTeamType == salesTeamType && s.TypeTarget == typeTarget.ToString()
                                         select s).ToList();
            foreach (var elm in targetRevenueSalesman)
            {
                listTargetSM.Add(elm.SalesTeamID, elm.Target.Value);
            }
            return listTargetSM;
            #endregion
        }
        public static decimal GetSaleTargetHeatMap(int salesTeamType, string typeTarget,   List<pp_ReportSalesAssessmentResult> listAll )
        {
            #region ChartTarget
            decimal totalRevenue = 0;
            DateTime today = DateTime.Now;
            var listTargetKPI = (from sm in Global.Context.SalesTargetKPIs.AsEnumerable()
                                 join ut in listAll on new {  ID = sm.DistributorID, Name = sm.SalesTeamID } equals new { ID = ut.DistributorID, Name =ut.SalesmanID }
                                 where sm.SalesTeamType == "SM" 
                                         && sm.Type.Equals("Day")
                                         && sm.TypeTarget.Equals("Revenue") 
                                         && sm.YearNbr == today.Year 
                                         && sm.MonthNbr == today.Month
                                 select sm).Distinct().ToList();
            totalRevenue = listTargetKPI.Sum(x => x.Target.Value);
            return totalRevenue;
            #endregion
        }

        public static List<DateTime> GetListDayWork(DateTime month)
        {
            List<DateTime> lstWord = new List<DateTime>();
            int intDaysThisMonth = DateTime.DaysInMonth(month.Year, month.Month);
            DateTime oBeginnngOfThisMonth = new DateTime(month.Year, month.Month, 1);
            for (int i = 1; i < intDaysThisMonth + 1; i++)
            {
                if (oBeginnngOfThisMonth.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                {
                }
                else
                {
                    lstWord.Add(new DateTime(month.Year, month.Month, i));
                } 
            }
            return lstWord;
        }

        
        #endregion

        #region Set Data For Chart Dashboard
        public static ChartData GetChartDataInMonthRevenue(ref DataTable chartTable)
        {
            #region ChartTarget
            decimal totalRevenue = GetTargetBySalesTypeTargetType("Revenue"); // need to editing 
            int daysOfMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            List<DateTime> listDayWord = GetListDayWork(DateTime.Now);
            #endregion

            #region GetData
            List<string> listDayOfMonth = new List<string>();
            List<int> listTargetRevenueOfMonth = new List<int>();
            List<decimal> listDataRevenueOfMonth = new List<decimal>();

            List<pp_GetReportVisitInMonthResult> listData = (from sm in CacheDataHelper.CacheReportVisitInMonth()
                                                             join ut in ControllerHelper.ListRoute
                                                             on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                             select sm).Distinct().ToList();
            if (listData != null)
            {
                var resultRevenue = listData.GroupBy(g => g.VisitDate.Value.Date).Select(s => new
                {
                    Name = s.Key.Date.ToString("dd"),
                    MTDTotalAmount = decimal.Parse(s.Sum(x => x.MTDTotalAmount).ToString())
                }).Distinct().ToList().ToDictionary(o => o.Name, o => o.MTDTotalAmount);


                int i = 1;
                foreach (DateTime elm in listDayWord)
                {
                    listDayOfMonth.Add(elm.Date.ToString("dd"));
                    decimal avgRevElm = (totalRevenue > 0 && listDayWord.Count > 0) ? ((decimal)totalRevenue * (decimal)i / (decimal)listDayWord.Count) : 0;
                    int value = Convert.ToInt32(avgRevElm);
                    listTargetRevenueOfMonth.Add(value);
                    decimal dataItem = 0;
                    foreach (var e in resultRevenue)
                    {
                        if (e.Key.ToString() == elm.Date.ToString("dd"))
                        {
                            dataItem = e.Value;
                        }
                    }
                    listDataRevenueOfMonth.Add(dataItem);
                    i++;
                }
            }
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.chartName = Utility.Phrase("ChartRevenueOfMonth");
            chartData.YName = Utility.Phrase("UnitRevenue");
            chartData.listColumns = listDayOfMonth;
            chartData.targetName = Utility.Phrase("TargetRevenueOfMonth");
            chartData.listTarget = listTargetRevenueOfMonth;
            chartData.listSeries = new List<ColumnData>();
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalRevenue"), visible = false, data = listDataRevenueOfMonth });
            #endregion

            #region Biding data table
            if (chartTable == null)
            {
                chartTable = new DataTable("ChartTable");
            }

            string groupby = PermissionHelper.GroupByRole();
            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = listData.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(OrderCount) as OrderCount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity)");

            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ReportSMVisitSummaryChartData()
                {
                    Name = item.Name,
                    TotalAmount = item.TotalAmount,
                    OrderCount = item.OrderCount,
                    TotalSKU = item.TotalSKU,
                    TotalQuantity = item.TotalQuantity,
                });
            }
            if (result.Count > 0)
            {
                chartTable.Columns.Add(groupby, typeof(String));
                foreach (var elm in result)
                {
                    chartTable.Columns.Add(elm.Name.ToString(), typeof(String));
                }

                List<string> rowAmount = new List<string>();
                rowAmount.Add(Utility.Phrase("TotalAmount"));
                rowAmount.AddRange(result.Select(s => Utility.StringParseWithRoundingDecimalDegit(s.TotalAmount)).ToList());
                DataRow dataRowAmount = chartTable.NewRow();
                dataRowAmount.ItemArray = rowAmount.ToArray();
                chartTable.Rows.Add(dataRowAmount);

                List<string> rowOrder = new List<string>();
                rowOrder.Add(Utility.Phrase("OrderCount"));
                rowOrder.AddRange(result.Select(s => Utility.StringParseWithRoundingDecimalDegit(s.OrderCount)).ToList());
                DataRow dataRowOrder = chartTable.NewRow();
                dataRowOrder.ItemArray = rowOrder.ToArray();
                chartTable.Rows.Add(dataRowOrder);


                List<string> rowSKU = new List<string>();
                rowSKU.Add(Utility.Phrase("TotalSKU"));
                rowSKU.AddRange(result.Select(s => Utility.StringParseWithRoundingDecimalDegit(s.TotalSKU)).ToList());
                DataRow dataRowSKU = chartTable.NewRow();
                dataRowSKU.ItemArray = rowSKU.ToArray();
                chartTable.Rows.Add(dataRowSKU);

                List<string> rowQuantity = new List<string>();
                rowQuantity.Add(Utility.Phrase("TotalQuantity"));
                rowQuantity.AddRange(result.Select(s => Utility.StringParseWithRoundingDecimalDegit(s.TotalQuantity)).ToList());
                DataRow dataRowQuantity = chartTable.NewRow();
                dataRowQuantity.ItemArray = rowQuantity.ToArray();
                chartTable.Rows.Add(dataRowQuantity);

            }
            #endregion

            return chartData;
        }

        public static ChartData GetChartDataInMonthVisit()
        {
            #region GetData
            List<pp_GetReportVisitInMonthResult> listData = (from sm in CacheDataHelper.CacheReportVisitInMonth()
                                                             join ut in ControllerHelper.ListRoute
                                                             on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                             where sm.OutletMustVisit > 0
                                                             select sm).Distinct().ToList();
            var resultRevenue = listData.GroupBy(g => g.VisitDate.Value.Date).Select(s => new
            {
                Name = s.Key.Date.ToString("dd"),
                OutletMustVisit = decimal.Parse(s.Sum(x => x.OutletMustVisit).ToString()),
                OutletVisited = decimal.Parse(s.Sum(x => x.OutletVisited).ToString()),
                OrderCount = decimal.Parse(s.Sum(x => x.OrderCount).ToString()),
                VisitMCP = Decimal.Round(decimal.Parse(s.Sum(x => x.OutletVisited).ToString()) * 100 / decimal.Parse(s.Sum(x => x.OutletMustVisit).ToString())),
                SOMCP = Decimal.Round(decimal.Parse(s.Sum(x => x.OrderCount).ToString()) * 100 / decimal.Parse(s.Sum(x => x.OutletMustVisit).ToString())),
            }).Distinct().ToList();

            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.chartName = Utility.Phrase("ChartVisitMCPOfMonth");
            chartData.YName = Utility.Phrase("UnitVisitMCP");
            chartData.listColumns = resultRevenue.Select(s => s.Name).ToList();
            chartData.listSeries = new List<ColumnData>();
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OutletMustVisit"), visible = true, data = resultRevenue.Select(s => s.OutletMustVisit).ToList() });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OutletVisited"), visible = true, data = resultRevenue.Select(s => s.OutletVisited).ToList() });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OrderCount"), visible = true, data = resultRevenue.Select(s => s.OrderCount).ToList() });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("VisitMCP"), visible = true, data = resultRevenue.Select(s => s.VisitMCP).ToList() });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SOMCP"), visible = true, data = resultRevenue.Select(s => s.SOMCP).ToList() });
            #endregion

            return chartData;
        }

        public static ChartData GetChartDataInMonthOther()
        {
            List<pp_GetReportVisitInMonthResult> listData = (from sm in CacheDataHelper.CacheReportVisitInMonth()
                                                             join ut in ControllerHelper.ListRoute
                                                             on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                             where sm.OutletMustVisit > 0
                                                             select sm).Distinct().ToList();
            #region Prepare Data For Chart

            var result = listData.GroupBy(g => g.VisitDate.Value.Date).Select(s => new
            {
                Name = s.Key.Date.ToString("dd"),
                TotalSKU = decimal.Parse(s.Sum(x => x.TotalSKU).ToString()),
                TotalQuantity = decimal.Parse(s.Sum(x => x.TotalQuantity).ToString()),
                LPPC = Decimal.Round(s.Sum(x => x.OrderCount) > 0 ? decimal.Parse(s.Sum(x => x.TotalSKU).ToString()) / decimal.Parse(s.Sum(x => x.OrderCount).ToString()) : 0,2),
                SOMCP = Decimal.Round(decimal.Parse(s.Sum(x => x.OrderCount).ToString()) * 100 / decimal.Parse(s.Sum(x => x.OutletMustVisit).ToString())),
                //decimal.Parse(s.Average(x => x.SOMCP).ToString()),
            }).Distinct().ToList();

            var listColumns = result.OrderBy(x => x.Name).Select(x => x.Name).ToList();
            var seriesTotalSKU = result.OrderBy(x => x.Name).Select(x => x.TotalSKU).ToList();
            var seriesTotalQuantity = result.OrderBy(x => x.Name).Select(x => x.TotalQuantity).ToList();
            var seriesLPPC = result.OrderBy(x => x.Name).Select(x => x.LPPC).ToList();
            var seriesSOMCP = result.OrderBy(x => x.Name).Select(x => x.SOMCP).ToList();
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.listSeries = new List<ColumnData>();
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalSKU"), visible = false, data = seriesTotalSKU });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalQuantity"), visible = true, data = seriesTotalQuantity });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("LPPC"), visible = false, data = seriesLPPC });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SOMCP"), visible = false, data = seriesSOMCP });
            chartData.listColumns = new List<string>();
            chartData.listColumns.AddRange(listColumns);
            chartData.chartName = Utility.Phrase("ChartMTD");
            chartData.YName = "";
            #endregion

            return chartData;
        }

        public static ChartData ChartRevenueInDay(ref DataTable chartTable, string type = null)
        {
            #region GetData
            List<pp_GetBaselineTargetInDayResult> listData = (from sm in CacheDataHelper.CacheBaselineTargetInDay()
                                                             join ut in ControllerHelper.ListRoute
                                                             on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                             select sm).Distinct().ToList();

            string groupby = PermissionHelper.GroupByRole();
            var result = new List<ChartPercentage>();
            var dynamicQuery = listData.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(Target) as Target, SUM(Achieved) as Achieved, Average(AchievedPercentage) as AchievedPercentage, SUM(Rest) as Rest)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ChartPercentage()
                {
                    Name = item.Name,
                    Target = item.Target,
                    Achieved = item.Achieved,
                    Rest = (item.Target > 0 ? (item.Achieved > item.Target ? 0 : (item.Target - item.Achieved)) : 0),
                    AchievedPercentage = item.Target > 0 ? item.Achieved * 100 / item.Target : 100,
                    RestPercentage = item.Target > 0 ? 
                                                    item.Achieved < item.Target ? 100 - (item.Achieved * 100 / item.Target) 
                                                    : 100 
                                                    : 0
                });
            }
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.chartName = Utility.Phrase("ChartRevenueOfDay");
            chartData.listColumns = result.Select(s => s.Name).ToList();
            chartData.listSeries = new List<ColumnData>();
            if (string.IsNullOrEmpty(type))
            {
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Rest"), visible = true, color = "#ED1C24", data = result.Select(s => s.Rest).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Achieved"), visible = true, color = "#7cb5ec", data = result.Select(s => s.Achieved).ToList() });
            }
            else
            {
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("RestPercentage"), visible = true, color = "#ED1C24", data = result.Select(s => s.RestPercentage).ToList() });
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("AchievedPercentage"), visible = true, color = "#7cb5ec", data = result.Select(s => s.AchievedPercentage).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Achieved"), visible = true, color = "#7cb5ec", data = result.Select(s => s.Achieved).ToList() });
            }
            
            #endregion

            #region Biding data table
            if (chartTable == null)
            {
                chartTable = new DataTable("ChartTable");
            }
            if (result.Count > 0)
            {
                chartTable.Columns.Add("", typeof(String));
                chartTable.Columns.Add(Utility.Phrase("Target"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("Achieved"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("Rest"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("AchievedPercentage"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("RestPercentage"), typeof(String));

                foreach (var elm in result)
                {
                    List<string> row = new List<string>();
                    row.Add(elm.Name);
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.Target));
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.Achieved));
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.Rest));
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.AchievedPercentage) + "%");
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.RestPercentage) + "%");
                    DataRow dataRow = chartTable.NewRow();
                    dataRow.ItemArray = row.ToArray();
                    chartTable.Rows.Add(dataRow);
                }

            }
            #endregion
            return chartData;
        }
        public static ChartData ChartRevenueInDayNew(ref DataTable chartTable, string type = null)
        {
            #region GetData
            List<pp_GetBaselineTargetInDayResult> listData = (from sm in CacheDataHelper.CacheBaselineTargetInDay()
                                                              join ut in ControllerHelper.ListRoute
                                                              on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                              select sm).Distinct().ToList();

            string groupby = PermissionHelper.GroupByRole();
            var result = new List<ChartPercentage>();
            var dynamicQuery = listData.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(Target) as Target, SUM(Achieved) as Achieved, Average(AchievedPercentage) as AchievedPercentage, SUM(Rest) as Rest)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ChartPercentage()
                {
                    Name = item.Name,
                    Achieved = item.Achieved,
                });
            }
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.chartName = Utility.Phrase("ChartRevenueOfDay");
            chartData.listColumns = result.Select(s => s.Name).ToList();
            chartData.listSeries = new List<ColumnData>();
            if (string.IsNullOrEmpty(type))
            {
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Rest"), visible = true, color = "#ED1C24", data = result.Select(s => s.Rest).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Achieved"), visible = true, color = "#7cb5ec", data = result.Select(s => s.Achieved).ToList() });
            }
            else
            {
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("RestPercentage"), visible = true, color = "#ED1C24", data = result.Select(s => s.RestPercentage).ToList() });
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("AchievedPercentage"), visible = true, color = "#7cb5ec", data = result.Select(s => s.AchievedPercentage).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Achieved"), visible = true, color = "#7cb5ec", data = result.Select(s => s.Achieved).ToList() });
            }

            #endregion

            #region Biding data table
            if (chartTable == null)
            {
                chartTable = new DataTable("ChartTable");
            }
            if (result.Count > 0)
            {
                chartTable.Columns.Add("", typeof(String));
                chartTable.Columns.Add(Utility.Phrase("Achieved"), typeof(String));

                foreach (var elm in result)
                {
                    List<string> row = new List<string>();
                    row.Add(elm.Name);
                    row.Add(Utility.StringNParseWithDecimalDegit(elm.Achieved));
                    DataRow dataRow = chartTable.NewRow();
                    dataRow.ItemArray = row.ToArray();
                    chartTable.Rows.Add(dataRow);
                }

            }
            #endregion
            return chartData;
        }

        public static ChartData ChartSyncInDay(ref DataTable chartTable)
        {
            ChartData chartData = new ChartData();
            int roleID = SessionHelper.GetSession<int>("RoleUser");
            if ((int)Utility.RoleName.SS == roleID)
            {
                #region GetData
                List<pp_GetSyncBaselineResult> listData = (from sm in CacheDataHelper.CacheSyncVisitInMonth()
                                                           join ut in ControllerHelper.ListRoute
                                                           on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                           where sm.VisitDate.Date == CacheDataHelper.DashBoardDate.Date
                                                           select sm).Distinct().ToList();

                List<decimal> result = new List<decimal>();
                decimal numberSMSyncInvalid = listData.Sum(s=>s.SyncTimeInValid);
                result.Add(numberSMSyncInvalid);
                decimal numberSMNotData = listData.Sum(s=>s.LastEndTimeInValid);
                result.Add(numberSMNotData);
                decimal numberSMInvalidDistance = listData.Sum(s=>s.DistanceInvalid);
                result.Add(numberSMInvalidDistance);
                decimal numberSMInvalidTime = listData.Sum(s=>s.TimeInValid);
                result.Add(numberSMInvalidTime);

                #endregion

                #region Set Chart Data
                chartData.chartName = Utility.Phrase("ChartSyncOfDay");
                List<string> listColumnt = new List<string>();
                listColumnt.Add(Utility.Phrase("SMSyncInvalid"));
                listColumnt.Add(Utility.Phrase("SMNotData"));
                listColumnt.Add(Utility.Phrase("SMDistanceInvalid"));
                listColumnt.Add(Utility.Phrase("SMTimeInvalid"));
                chartData.listColumns = listColumnt;
                chartData.listSeries = new List<ColumnData>();
                chartData.listSeries.Add(new ColumnData() { visible = true, data = result });
                #endregion
            }
            else
            {
                #region GetData
                List<pp_GetSyncBaselineResult> listData = (from sm in CacheDataHelper.CacheSyncVisitInMonth()
                                                           join ut in ControllerHelper.ListRoute
                                                           on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                           where sm.VisitDate.Date == CacheDataHelper.DashBoardDate.Date
                                                           select sm).Distinct().ToList();

                string groupby = PermissionHelper.GroupByRole();
                var listName = listData.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(SyncTimeInValid) as SMSyncInValid, SUM(LastEndTimeInValid) as SMNotData, SUM(DistanceInvalid) as SMDistanceInvalid, SUM(TimeInValid) as SMTimeInValid, COUNT() as TotalSM)");
                List<ChartSync> result = new List<ChartSync>();
                if(listName != null)
                {
                    foreach(dynamic elm in listName)
                    {
                        result.Add(new ChartSync(){
                            Name = elm.Name,
                            SMSyncInValid = elm.SMSyncInValid,
                            SMNotData = elm.SMNotData,
                            SMDistanceInvalid = elm.SMDistanceInvalid,
                            SMTimeInValid = elm.SMTimeInValid,
                            TotalSM = elm.TotalSM
                        });
                    }
                }
                #endregion

                #region Set Chart Data
                chartData.chartName = Utility.Phrase("ChartSyncOfDay");
                chartData.listColumns = result.Select(s=>s.Name).ToList();
                chartData.listSeries = new List<ColumnData>();
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SMSyncInvalid"), visible = true, data = result.Select(s => s.SMSyncInValid).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SMNotData"), visible = true, data = result.Select(s => s.SMNotData).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SMDistanceInvalid"), visible = true, data = result.Select(s => s.SMDistanceInvalid).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SMTimeInvalid"), visible = true, data = result.Select(s => s.SMTimeInValid).ToList() });
                #endregion

                #region Biding data table
                if (chartTable == null)
                {
                    chartTable = new DataTable("ChartTable");
                }
                if (result.Count > 0)
                {
                    chartTable.Columns.Add("", typeof(String));
                    chartTable.Columns.Add(Utility.Phrase("SMSyncInvalid"), typeof(String));
                    chartTable.Columns.Add(Utility.Phrase("SMNotData"), typeof(String));
                    chartTable.Columns.Add(Utility.Phrase("SMDistanceInvalid"), typeof(String));
                    chartTable.Columns.Add(Utility.Phrase("SMTimeInvalid"), typeof(String));
                    chartTable.Columns.Add(Utility.Phrase("TotalSM"), typeof(String));

                    foreach (var elm in result)
                    {
                        List<string> row = new List<string>();
                        row.Add(elm.Name);
                        row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.SMSyncInValid));
                        row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.SMNotData));
                        row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.SMDistanceInvalid));
                        row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.SMTimeInValid));
                        row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.SMTimeInValid));
                        DataRow dataRow = chartTable.NewRow();
                        dataRow.ItemArray = row.ToArray();
                        chartTable.Rows.Add(dataRow);
                    }

                }
                #endregion
            }
            
            return chartData;
        }

        public static PieData GetReportVisitLevel(ref DataTable visitTable)
        {
            List<pp_ReportOrderIndexLevelResult> model = CacheDataHelper.CacheSMVisitLevelResult();
            #region Prepare Data For Chart
            var result = new List<PieColumnData>();
            decimal total = model.Count();
            if (total > 0)
            {
                result = (from i in model
                                    group i by i.SettingName into g
                                    select new PieColumnData()
                                    {
                                        name = g.Key,
                                        y = g.Sum(s=>s.IsValue)
                                    }).ToList();
            }
            #endregion

            #region Biding data table
            string groupby = PermissionHelper.GroupByRole();

            var queryVisitTable = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name, SettingName)", "it").Select("new(Key." + groupby + "Name as Name, Key.SettingName as Rate, SUM(IsValue) as Count)");
            List<ReportPieTable> listVisit = new List<ReportPieTable>();
            foreach (dynamic item in queryVisitTable)
            {
                listVisit.Add(new ReportPieTable() { Name = item.Name, Rate = item.Rate, Count = item.Count });
            }

            if (queryVisitTable.Count() > 0)
            {
                if (visitTable == null)
                {
                    visitTable = new DataTable("VisitTable");
                }
                visitTable.Columns.Add("#", typeof(String));
                foreach (string elm in listVisit.GroupBy(g => g.Rate).OrderBy(o => o.Key).Select(s => s.Key).ToList())
                {
                    visitTable.Columns.Add(elm, typeof(String));
                }
                visitTable.Columns.Add(Utility.Phrase("CountSalesman"), typeof(String));

                var list = (from l in listVisit
                            group l by l.Name into g
                            orderby g.Key
                            select new
                            {
                                Name = g.Key,
                                Count = g.Sum(l => l.Count)
                            }).ToList();
                foreach (var elm in list)
                {
                    List<string> tempList = new List<string>();
                    tempList.Add(elm.Name);
                    var listgroupbyRate = (from l in listVisit
                                           where l.Name == elm.Name
                                           group l by l.Rate into g
                                           orderby g.Key
                                           select new
                                           {
                                               Rate = g.Key,
                                               Count = g.Sum(l => l.Count)
                                           }).ToList();
                    foreach (string item in listVisit.GroupBy(g => g.Rate).OrderBy(o => o.Key).Select(s => s.Key).ToList())
                    {
                        var rate = listgroupbyRate.Where(x => x.Rate.Equals(item)).SingleOrDefault();
                        if (rate != null)
                        {
                            tempList.Add(Utility.StringParseWithRoundingDecimalDegit(rate.Count));
                        }
                        else
                        {
                            tempList.Add("0");
                        }
                    }
                    tempList.Add(elm.Count.ToString());
                    visitTable.Rows.Add(tempList.ToArray<string>());
                }
            }
            #endregion


            #region Set Chart Data
            PieData chartData = new PieData();
            chartData.listSeries = new List<PieColumnData>();
            chartData.listSeries = result.OrderBy(o => o.name).ToList();
            chartData.chartName = Utility.Phrase("ReportOrderIndexChartPieVisit");
            chartData.tooltips = Utility.Phrase("ToolTipReportOrderIndexChartPieVisit");
            #endregion

            return chartData;
        }

        public static PieData GetReportOrderLevel(ref DataTable orderTable)
        {
            List<pp_ReportOrderIndexLevelResult> model = CacheDataHelper.CacheSMOrderLevelResult();
            #region Prepare Data For Chart
            var result = new List<PieColumnData>();
            decimal total = model.Count();
            if (total > 0)
            {
                result = (from i in model
                                    group i by i.SettingName into g
                                    select new PieColumnData()
                                    {
                                        name = g.Key,
                                        y = g.Sum(s => s.IsValue)
                                    }).ToList();
            }
            #endregion

            #region Biding data table
            string groupby = PermissionHelper.GroupByRole();

            var queryorderTable = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name, SettingName)", "it").Select("new(Key." + groupby + "Name as Name, Key.SettingName as Rate,  SUM(IsValue) as Count)");
            List<ReportPieTable> listOrder = new List<ReportPieTable>();
            foreach (dynamic item in queryorderTable)
            {
                listOrder.Add(new ReportPieTable() { Name = item.Name, Rate = item.Rate, Count = item.Count });
            }

            if (queryorderTable.Count() > 0)
            {
                if (orderTable == null)
                {
                    orderTable = new DataTable("orderTable");
                }
                orderTable.Columns.Add("#", typeof(String));
                foreach (string elm in listOrder.GroupBy(g => g.Rate).OrderBy(o => o.Key).Select(s => s.Key).ToList())
                {
                    orderTable.Columns.Add(elm, typeof(String));
                }
                orderTable.Columns.Add(Utility.Phrase("CountSalesman"), typeof(String));

                var list = (from l in listOrder
                            group l by l.Name into g
                            orderby g.Key
                            select new
                            {
                                Name = g.Key,
                                Count = g.Sum(l => l.Count)
                            }).ToList();
                foreach (var elm in list)
                {
                    List<string> tempList = new List<string>();
                    tempList.Add(elm.Name);
                    var listgroupbyRate = (from l in listOrder
                                           where l.Name == elm.Name
                                           group l by l.Rate into g
                                           orderby g.Key
                                           select new
                                           {
                                               Rate = g.Key,
                                               Count = g.Sum(l => l.Count)
                                           }).ToList();
                    foreach (string item in listOrder.GroupBy(g => g.Rate).OrderBy(o => o.Key).Select(s => s.Key).ToList())
                    {
                        var rate = listgroupbyRate.Where(x => x.Rate.Equals(item)).SingleOrDefault();
                        if (rate != null)
                        {
                            tempList.Add(Utility.StringParseWithRoundingDecimalDegit(rate.Count));
                        }
                        else
                        {
                            tempList.Add("0");
                        }
                    }
                    tempList.Add(elm.Count.ToString());
                    orderTable.Rows.Add(tempList.ToArray<string>());
                }
            }
            #endregion


            #region Set Chart Data
            PieData chartData = new PieData();
            chartData.listSeries = new List<PieColumnData>();
            chartData.listSeries.AddRange(result.OrderBy(a => a.name).ToList());
            chartData.chartName = Utility.Phrase("ReportOrderIndexChartPieOrder");
            chartData.tooltips = Utility.Phrase("ToolTipReportOrderIndexChartPieOrder");
            #endregion

            return chartData;
        }

        public static ChartData ChartCumulativeRevenue(ref DataTable chartTable)
        {
            List<pp_GetReportVisitInMonthResult> listData = (from sm in CacheDataHelper.CacheReportVisitInMonth()
                                                             join ut in ControllerHelper.ListRoute
                                                             on new { sm.DistributorID, sm.SalesmanID } equals new { ut.DistributorID, ut.SalesmanID }
                                                             select sm).Distinct().ToList();
            #region Prepare Data For Chart
            string groupby = PermissionHelper.GroupByRole();
            var list = listData.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name, VisitDate)", "it").Select("new(Key." + groupby + "Name as Name, Key.VisitDate as VisitDate, SUM(TotalAmount) as MTDTotalAmount)");
            List<CumulativeRevenue> result = new List<CumulativeRevenue>();
            if(list != null)
            {
                foreach (dynamic elm in list)
                {

                    if (!string.IsNullOrEmpty(elm.Name))
                    {
                        result.Add(new CumulativeRevenue()
                        {
                            Name = elm.Name,
                            Day = elm.VisitDate,
                            Value = elm.MTDTotalAmount
                        });
                    }
                }
            }

            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.chartName = Utility.Phrase("ChartCumulativeRevenue");
            chartData.listColumns = result.GroupBy(g => g.Day.Date).OrderBy(o=>o.Key.Date).Select(s => s.Key.Date.ToString("dd")).ToList();
            chartData.listSeries = new List<ColumnData>();
            var listGroupby = result.GroupBy(g => g.Name).Select(s => s.Key).ToList();
            foreach (string elm in listGroupby)
            {
                List<decimal> listOfItem = result.Where(x => x.Name.Equals(elm)).OrderBy(o => o.Day.Date).Select(s => s.Value).ToList();
                chartData.listSeries.Add(new ColumnData() { name = elm, visible = false, data = listOfItem });
            }
            #endregion

            #region Biding data table
            List<pp_GetBaselineTargetInMonthResult> listRevenueTarget = (from sm in CacheDataHelper.CacheBaselineTargetInMonth()
                                                             join ut in ControllerHelper.ListRoute
                                                             on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                             select sm).Distinct().ToList();
            var listDataTable = listRevenueTarget.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(Target) as Target, SUM(Achieved) as Achieved, Average(AchievedPercentage) as AchievedPercentage, SUM(Rest) as Rest, SUM(AchievedPlan) as AchievedPlan, SUM(TargetDay) as TargetDay)");
            List<ChartPercentage> resultTable = new List<ChartPercentage>(); 
            if (listDataTable != null)
            {
                foreach (dynamic elm in listDataTable)
                {
                    if (elm.Target > 0)
                    {
                        resultTable.Add(new ChartPercentage()
                        {
                            Name = elm.Name,
                            Target = elm.Target,
                            Achieved = elm.Achieved,
                            AchievedPercentage = elm.Target > 0 ? elm.Achieved * 100 / elm.Target : 100,
                            Rest = elm.Target - elm.Achieved < 0 ? 0 : elm.Target - elm.Achieved,
                            AchievedPlan = elm.AchievedPlan,
                            TargetDay = elm.TargetDay
                        });
                    }
                    else
                    {
                        resultTable.Add(new ChartPercentage()
                        {
                            Name = elm.Name,
                            Target = elm.Target,
                            Achieved = elm.Achieved,
                            AchievedPercentage = 0,
                            Rest = elm.Rest,
                            AchievedPlan = elm.AchievedPlan,
                            TargetDay = elm.TargetDay
                        });
                    }
                }
            }
            
            if (chartTable == null)
            {
                chartTable = new DataTable("ChartTable");
            }
            if (result.Count > 0)
            {
                chartTable.Columns.Add("", typeof(String));
                chartTable.Columns.Add(Utility.Phrase("Target"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("Achieved"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("Rest"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("AchievedPercentage"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("AchievedPlan"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("TargetDay"), typeof(String));

                foreach (var elm in resultTable)
                {
                    List<string> row = new List<string>();
                    row.Add(elm.Name);
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.Target));
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.Achieved));
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.Rest));
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.AchievedPercentage) + "%");
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.AchievedPlan));
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.TargetDay));
                    DataRow dataRow = chartTable.NewRow();
                    dataRow.ItemArray = row.ToArray();
                    chartTable.Rows.Add(dataRow);
                }
            }
            #endregion

            return chartData;
        }

        public static ChartData ChartVisitInDay(ref DataTable chartTable, string type = null)
        {
            #region GetData
            List<pp_GetReportVisitInMonthResult> listData = (from sm in CacheDataHelper.CacheReportVisitInMonth()
                                                             join ut in ControllerHelper.ListRoute
                                                             on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                             where sm.VisitDate.Value.Date == CacheDataHelper.DashBoardDate.Date
                                                             && sm.OutletMustVisit > 0
                                                             select sm).Distinct().ToList();

            string groupby = PermissionHelper.GroupByRole();
            var result = new List<ChartPercentage>();
            var dynamicQuery = listData.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, Average(VisitMCP) as VisitMCP)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ChartPercentage()
                {
                    Name = item.Name,
                    Target = item.OutletMustVisit,
                    Achieved = item.OutletVisited,
                    Rest = item.OutletMustVisit - item.OutletVisited,
                    AchievedPercentage = item.OutletVisited * 100 / item.OutletMustVisit,
                    RestPercentage = 100 - (item.OutletVisited * 100 / item.OutletMustVisit)
                });
            }
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.chartName = Utility.Phrase("ChartVisitOfDay");
            chartData.listColumns = result.Select(s => s.Name).ToList();
            chartData.listSeries = new List<ColumnData>();
            if (string.IsNullOrEmpty(type))
            {
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Rest"), visible = true, data = result.Select(s => s.Rest).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Achieved"), visible = true, data = result.Select(s => s.Achieved).ToList() });
            }
            else
            {
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("RestPercentage"), visible = true, data = result.Select(s => s.RestPercentage).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("AchievedPercentage"), visible = true, data = result.Select(s => s.AchievedPercentage).ToList() });
            }
            #endregion

            #region Biding data table
            if (chartTable == null)
            {
                chartTable = new DataTable("ChartTable");
            }
            if (result.Count > 0)
            {
                chartTable.Columns.Add("", typeof(String));
                chartTable.Columns.Add(Utility.Phrase("AchievedPercentage"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("RestPercentage"), typeof(String));

                foreach (var elm in result)
                {
                    List<string> row = new List<string>();
                    row.Add(elm.Name);
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.AchievedPercentage));
                    row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.RestPercentage));
                    DataRow dataRow = chartTable.NewRow();
                    dataRow.ItemArray = row.ToArray();
                    chartTable.Rows.Add(dataRow);
                }

            }
            #endregion
            return chartData;
        }

        public static ChartData ChartVisitInDayNew(ref DataTable chartTable, string type = null)
        {
            #region GetData
            List<pp_GetReportVisitInMonthResult> listData = (from sm in CacheDataHelper.CacheReportVisitInMonth()
                                                             join ut in ControllerHelper.ListRoute
                                                             on new { sm.DistributorID, sm.RouteID } equals new { ut.DistributorID, ut.RouteID }
                                                             where sm.VisitDate.Value.Date == CacheDataHelper.DashBoardDate.Date
                                                             && sm.OutletMustVisit > 0
                                                             select sm).Distinct().ToList();

            string groupby = PermissionHelper.GroupByRole();
            var result = new List<ChartPercentage>();
            var dynamicQuery = listData.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited,SUM(OrderCount) as OrderCount, Average(VisitMCP) as VisitMCP)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ChartPercentage()
                {
                    Name = item.Name,
                    Target = item.OutletMustVisit,
                    Rest = item.OutletMustVisit - item.OutletVisited,
                    OrderCount = item.OrderCount,
                    Achieved = item.OutletVisited,

                    //AchievedPercentage = item.OutletVisited * 100 / item.OutletMustVisit,
                    //RestPercentage = 100 - (item.OutletVisited * 100 / item.OutletMustVisit)
                });
            }
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.chartName = Utility.Phrase("ChartVisitOfDay");
            chartData.listColumns = result.Select(s => s.Name).ToList();
            chartData.listSeries = new List<ColumnData>();
            if (string.IsNullOrEmpty(type))
            {
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Total"), visible = true, data = result.Select(s => s.Target).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SMHasNotVisit"), visible = true, data = result.Select(s => s.Rest).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SMHasVisit"), visible = true, data = result.Select(s => s.Achieved).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OrderCount"), visible = true, data = result.Select(s => s.OrderCount).ToList() });
            }
            else
            {
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("Total"), visible = true, data = result.Select(s => s.Target).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SMHasNotVisit"), visible = true, data = result.Select(s => s.Rest).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SMHasVisit"), visible = true, data = result.Select(s => s.Achieved).ToList() });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OrderCount"), visible = true, data = result.Select(s => s.OrderCount).ToList() });
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("RestPercentage"), visible = true, data = result.Select(s => s.RestPercentage).ToList() });
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("AchievedPercentage"), visible = true, data = result.Select(s => s.AchievedPercentage).ToList() });
            }
            #endregion

            #region Biding data table
            if (chartTable == null)
            {
                chartTable = new DataTable("ChartTable");
            }
            if (result.Count > 0)
            {
                chartTable.Columns.Add("", typeof(String));
                //chartTable.Columns.Add(Utility.Phrase("AchievedPercentage"), typeof(String));
                //chartTable.Columns.Add(Utility.Phrase("RestPercentage"), typeof(String));
                //chartTable.Columns.Add(Utility.Phrase("Total"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("SMHasNotVisit"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("SMHasVisit"), typeof(String));
                chartTable.Columns.Add(Utility.Phrase("OrderCount"), typeof(String));

                foreach (var elm in result)
                {
                    List<string> row = new List<string>();
                    row.Add(elm.Name);
                    //row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.AchievedPercentage));
                    //row.Add(Utility.StringParseWithRoundingDecimalDegit(elm.RestPercentage));
                    //row.Add(Utility.StringParse(elm.Target));
                    row.Add(Utility.StringParse(elm.Rest));
                    row.Add(Utility.StringParse(elm.Achieved));
                    row.Add(Utility.StringParse(elm.OrderCount));
                    DataRow dataRow = chartTable.NewRow();
                    dataRow.ItemArray = row.ToArray();
                    chartTable.Rows.Add(dataRow);
                }

            }
            #endregion
            return chartData;
        }

        #endregion

        #region LogUserAction
        public static void LogUserAction(string page, string action, string param)
        {
            try
            {
                if (string.IsNullOrEmpty(param))
                {
                    param = HttpContext.Current.Request.Params.ToString();
                }

                if (param.Length > 1000)
                {
                    param = param.Substring(0, 1000);
                }

                UserActionLog userAL = new UserActionLog()
                {
                    UserID = SessionHelper.GetSession<int>("UserID"),
                    ReferUrl = HttpContext.Current.Request.UrlReferrer.AbsolutePath,
                    Page = page,
                    Action = action,
                    Param = param,
                    Date = DateTime.Now,

                };

                Global.Context.UserActionLogs.InsertOnSubmit(userAL);
                Global.Context.SubmitChanges();
            }
            catch (Exception ex)
            {

            }
        }

        public static string GetClientIpAddress(HttpRequestBase request)
        {
            string ip;
            try
            {
                ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrEmpty(ip))
                {
                    if (ip.IndexOf(",") > 0)
                    {
                        string[] ipRange = ip.Split(',');
                        int le = ipRange.Length - 1;
                        ip = ipRange[le];
                    }
                }
                else
                {
                    ip = request.UserHostAddress;
                }
            }
            catch { ip = null; }

            return ip;
        }

        public static string GetClientIpAddressPrivate(HttpRequestBase request)
        {
            try
            {
                var userHostAddress = request.UserHostAddress;

                // Attempt to parse.  If it fails, we catch below and return "0.0.0.0"
                // Could use TryParse instead, but I wanted to catch all exceptions
                IPAddress.Parse(userHostAddress);

                var xForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(xForwardedFor))
                    return userHostAddress;

                // Get a list of public ip addresses in the X_FORWARDED_FOR variable
                var publicForwardingIps = xForwardedFor.Split(',').Where(ip => !IsPrivateIpAddress(ip)).ToList();

                // If we found any, return the last one, otherwise return the user host address
                return publicForwardingIps.Any() ? publicForwardingIps.Last() : userHostAddress;
            }
            catch (Exception)
            {
                // Always return all zeroes for any failure (my calling code expects it)
                return "0.0.0.0";
            }
        }

        private static bool IsPrivateIpAddress(string ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are: 
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var ip = IPAddress.Parse(ipAddress);
            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock) return true; // Return to prevent further processing

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock) return true; // Return to prevent further processing

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock) return true; // Return to prevent further processing

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }
        #endregion

        #region LoadPhrase
        public static void LoadPhrase(string phraseCode)
        {
            try
            {
                #region Process language
                List<Language> listLanguage = new List<Language>();
                listLanguage.AddRange(Global.Context.Languages.ToList());

                Utility.Dictionaries = new Dictionary<string, Dictionary<string, string>>();
                Utility.ListLanguageID = new List<int>();
                foreach (Language item in listLanguage)
                {
                    Utility.Dictionaries.Add(item.Code, GetDictionaryPhraseByLanguage(item.LangID, phraseCode));
                    Utility.ListLanguageID.Add(item.LangID);
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static Dictionary<string, string> GetDictionaryPhraseByLanguage(int languageID, string phraseCode)
        {
            try
            {
                Dictionary<string, string> listPhrase = new Dictionary<string, string>();

                List<Phrase> lst = new List<Phrase>();
                if (string.IsNullOrEmpty(phraseCode))
                    lst.AddRange(Global.Context.Phrases.Where(a => a.LanguageID == languageID).ToList());
                else
                    lst.AddRange(Global.Context.Phrases.Where(a => (
                                                                        a.LanguageID == languageID
                                                                        //&& a.PhraseCode == phraseCode.Trim()
                                                                         && a.PhraseCode.Contains(phraseCode.Trim())
                                                                   )
                                                                   )
                                                                   .ToList()
                                                                   );

                foreach (Phrase item in lst)
                {
                    listPhrase.Add(item.PhraseCode, item.PhraseText);
                }

                return listPhrase;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public static List<Language> LoadLanguage()
        {
            try
            {
                List<Language> listLanguage = new List<Language>();
                listLanguage.AddRange(Global.Context.Languages.Where(a => a.Active == true).ToList());
                return listLanguage;
            }
            catch (Exception ex)
            {
                return new List<Language>();
            }
        }
        #endregion

        #region HeatMap
        public static List<pp_ReportSalesAssessmentResult> GetRevenueResult(string dateSelected)
        {
            List<pp_ReportSalesAssessmentResult> t = new List<pp_ReportSalesAssessmentResult>();
            if (!string.IsNullOrEmpty(dateSelected))
            {
                t = Global.Context.pp_ReportSalesAssessment(Utility.DateTimeParse(dateSelected), string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, "admin").ToList();
            }
            return t;
        }
        public static List<pp_GetDataRenderHeatMapResult> GetDataRenderHeatMapResult(string type, string dateSelected, string categoryID, string itemID)
        {
            List<pp_GetDataRenderHeatMapResult> t = new List<pp_GetDataRenderHeatMapResult>();
            if (!string.IsNullOrEmpty(dateSelected))
            {
                t = Global.Context.pp_GetDataRenderHeatMap(Utility.DateTimeParse(dateSelected), type, categoryID, itemID).Where(x=> x.RegionID != null && x.AreaID != null).ToList();
            }
            return t;
        }
        public static List<pp_GetCategoryItem_HeatMapResult> ListCategory(string typeHeatMap, string dateSelected)
        {
            string username = SessionHelper.GetSession<string>("UserName");
            List<pp_GetCategoryItem_HeatMapResult> t = new List<pp_GetCategoryItem_HeatMapResult>();
            if (!string.IsNullOrEmpty(dateSelected))
            {
                t = Global.Context.pp_GetCategoryItem_HeatMap(Utility.DateTimeParse(dateSelected), typeHeatMap, "Category").Distinct().OrderBy(a => a.CategoryName).ToList();
            }
            return t;
        }
        public static List<pp_GetCategoryItem_HeatMapResult> GetListItem(string dateSelected, string typeHeatMap, string categoryID)
        {
            string username = SessionHelper.GetSession<string>("UserName");
            List<pp_GetCategoryItem_HeatMapResult> listCategoryItem = new List<pp_GetCategoryItem_HeatMapResult>();
            if (!string.IsNullOrEmpty(dateSelected))
            {
                listCategoryItem = Global.Context.pp_GetCategoryItem_HeatMap(Utility.DateTimeParse(dateSelected), typeHeatMap, "Item")
                                    .Where(a => a.CategoryID.Equals(categoryID)).Distinct().OrderBy(a => a.InventoryName).ToList();
            }
            if (listCategoryItem.Count > 1)
            {
                listCategoryItem.Insert(0, new pp_GetCategoryItem_HeatMapResult() { InventoryID = 0, InventoryName = string.Empty });
            }
            return listCategoryItem;
        }
        #endregion
        public static object Sum<TSource>(this IQueryable<TSource> source, string member)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            PropertyInfo prop = typeof(TSource).GetProperty(member);
            ParameterExpression param = Expression.Parameter(typeof(TSource), "s");
            Expression selector = Expression.Lambda(Expression.MakeMemberAccess(param, prop), param);
            MethodInfo sumMethod = typeof(Queryable).GetMethods().First(m => m.Name == "Sum" && m.ReturnType == prop.PropertyType && m.IsGenericMethod);
            return source.Provider.Execute(Expression.Call(null, sumMethod.MakeGenericMethod(new Type[] { typeof(TSource) }), new Expression[] { source.Expression, Expression.Quote(selector) }));
        }

        public static DataTable QueryStoredProcedure(string name, List<ObjParamSP> listParam = null)
        {
            DataTable resulft = new DataTable();
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    conn.Open();
                    DataSet ds = new DataSet();
                    using (var cmd = new SqlCommand(name, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;
                        if (listParam != null)
                        {
                            foreach (ObjParamSP elm in listParam)
                            {
                                cmd.Parameters.Add(elm.Key, elm.Value ?? DBNull.Value);
                            }
                        }
                        var adapt = new SqlDataAdapter(cmd);
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count >= 1)
                        {
                            resulft = ds.Tables[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            return resulft;
        }
    }
    #endregion

    #region class PermissionHelper
    public class PermissionHelper
    {
        public static bool CheckPermissionByFeature(string featureCode, bool isMenu = false)
        {
            ControllerHelper.LogUserAction(featureCode, featureCode, "");
            if (!string.IsNullOrEmpty(featureCode))
            {
                List<Feature> listF = SessionHelper.GetSession<List<Feature>>("ListFeature");
                if (listF != null)
                {
                    if (listF.FirstOrDefault(a => a.FeatureCode == featureCode) != null)
                    {
                        return true;
                    }
                }
                

                #region Insert Feature if not exist
                string actionName = "";
                string controllerName = "";
                var routeValues = HttpContext.Current.Request.RequestContext.RouteData.Values;
                if (routeValues != null)
                {
                    if (routeValues.ContainsKey("action"))
                    {
                        actionName = HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                    }
                    if (routeValues.ContainsKey("controller"))
                    {
                        controllerName = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                    }
                }

                Feature model = Global.Context.Features.FirstOrDefault(a => a.FeatureCode == featureCode);
                string PhraseCode = null;
                string group = "Feature";
                if (isMenu)
                {
                    PhraseCode = "Menu_" + featureCode;
                    Utility.Phrase("Menu_" + featureCode);
                    group = "Menu";
                }

                if (model == null)
                {
                    model = new Feature()
                    {
                        ID = 0,
                        FeatureCode = featureCode,
                        FeatureName = featureCode,
                        Page = controllerName,
                        Action = actionName,
                        PhraseCode = PhraseCode,
                        Group = group,
                        IconClass = Utility.IconClassDefault
                    };
                    Global.Context.Features.InsertOnSubmit(model);
                    Global.Context.SubmitChanges();
                    return true;
                }
                #endregion
            }

            // return true;

            return false;
        }

        public static bool CheckPermissionByFeature(string featureCode, Controller c)
        {
            //TEST
            return CheckPermissionByFeature(featureCode);

            if (string.IsNullOrEmpty(SessionHelper.GetSession<string>("UserName")))
            {
                c.HttpContext.Response.Redirect("/Account/Login/");
                return false;
            }

            if (!HttpContext.Current.Application["usr_" + SessionHelper.GetSession<string>("UserName")].Equals(HttpContext.Current.Session.SessionID))
            {
                c.HttpContext.Response.Redirect("/Account/Login/");
                return false;
            }

            if (!CheckPermissionByFeature(featureCode))
            {
                c.HttpContext.Response.Redirect("/Home/ErrorPermission/");
                return false;
            }
            return true;
        }

        public static void SetSession(LoginModel model)
        {
            int userID = WebSecurity.GetUserId(model.UserName);
            if (userID > 0)
            {
                SessionHelper.SetSession<int>("UserID", userID);
                SessionHelper.SetSession<string>("UserName", model.UserName.ToUpper());
                //2016-05-04: Fix bug ma nhan vien va ten nhan vien khac nhau khong xem duoc giam sat
                //var checksf = (from x in Global.Context.DMSSalesForces where x.LoginID == model.UserName select x).FirstOrDefault();
                //if (checksf != null)
                //{
                //    if (checksf.LoginID != checksf.EmployeeID)
                //        SessionHelper.SetSession<string>("UserName", checksf.EmployeeID.ToUpper());
                //}
                //end fix
                RoleUser rule = Global.Context.RoleUsers.FirstOrDefault(a => a.UserID == userID);
                SessionHelper.SetSession<int>("RoleUser", rule.RoleID);

                #region ConfigDashBoard
                List<Utility.ChartName> listChartDashboard = new List<Utility.ChartName>();
                if (rule.RoleID == (int)Utility.RoleName.SS)
                {
                    var itemCustomSetting = Global.Context.CustomSettings.Where(x => x.SettingGroup == "DashBoardConfig" && x.SettingName == "DashBoardSSConfig").SingleOrDefault();
                    if (itemCustomSetting != null)
                    {
                        string strConfig = itemCustomSetting.SettingValue;
                        List<string> listConfig = strConfig.Split(';').ToList();
                        foreach (string elm in listConfig)
                        {
                            listChartDashboard.Add(Utility.ParseEnum<Utility.ChartName>(elm));
                        }
                    }
                }
                else if (rule.RoleID == (int)Utility.RoleName.ASM)
                {
                    var itemCustomSetting = Global.Context.CustomSettings.Where(x => x.SettingGroup == "DashBoardConfig" && x.SettingName == "DashBoardASMConfig").SingleOrDefault();
                    if (itemCustomSetting != null)
                    {
                        string strConfig = itemCustomSetting.SettingValue;
                        List<string> listConfig = strConfig.Split(';').ToList();
                        foreach (string elm in listConfig)
                        {
                            listChartDashboard.Add(Utility.ParseEnum<Utility.ChartName>(elm));
                        }
                    }
                }
                else if (rule.RoleID == (int)Utility.RoleName.RSM)
                {
                    var itemCustomSetting = Global.Context.CustomSettings.Where(x => x.SettingGroup == "DashBoardConfig" && x.SettingName == "DashBoardRSMConfig").SingleOrDefault();
                    if (itemCustomSetting != null)
                    {
                        string strConfig = itemCustomSetting.SettingValue;
                        List<string> listConfig = strConfig.Split(';').ToList();
                        foreach (string elm in listConfig)
                        {
                            listChartDashboard.Add(Utility.ParseEnum<Utility.ChartName>(elm));
                        }
                    }
                }
                else if (rule.RoleID == (int)Utility.RoleName.Auditor | rule.RoleID == (int)Utility.RoleName.Leader)
                {
                    var itemCustomSetting = Global.Context.CustomSettings.Where(x => x.SettingGroup == "DashBoardConfig" && x.SettingName == "DashBoardBODConfig").SingleOrDefault();
                    if (itemCustomSetting != null)
                    {
                        string strConfig = itemCustomSetting.SettingValue;
                        List<string> listConfig = strConfig.Split(';').ToList();
                        foreach (string elm in listConfig)
                        {
                            listChartDashboard.Add(Utility.ParseEnum<Utility.ChartName>(elm));
                        }
                    }
                }
                else
                {
                    var itemCustomSetting = Global.Context.CustomSettings.Where(x => x.SettingGroup == "DashBoardConfig" && x.SettingName == "DashBoardOtherConfig").SingleOrDefault();
                    if (itemCustomSetting != null)
                    {
                        string strConfig = itemCustomSetting.SettingValue;
                        List<string> listConfig = strConfig.Split(';').ToList();
                        foreach (string elm in listConfig)
                        {
                            listChartDashboard.Add(Utility.ParseEnum<Utility.ChartName>(elm));
                        }
                    }
                }
                SessionHelper.SetSession<List<Utility.ChartName>>("listChartDashboard", listChartDashboard);

                #endregion


                //Check DISTRIBUTOR
                Distributor dis = Global.Context.Distributors.FirstOrDefault(a => a.LoginID == model.UserName);
                if (dis != null)
                {
                    if (rule == null)
                    {
                        Global.Context.RoleUsers.InsertOnSubmit(new RoleUser() { RoleID = 10, UserID = userID });
                        Global.Context.SubmitChanges();
                    }

                    SessionHelper.SetSession<int>("BranchID", dis.DistributorID);
                    SessionHelper.SetSession<string>("BranchCode", dis.DistributorCode);
                    SessionHelper.SetSession<string>("BranchName", dis.DistributorName);
                    SessionHelper.SetSession<string>("BranchRegionID", dis.RegionID);
                    SessionHelper.SetSession<string>("BranchAreaID", dis.AreaID);
                    SessionHelper.SetSession<string>("BranchProvinceID", dis.ProvinceID);
                }
                else
                {
                    //CHECK SALESFORCE
                    DMSSalesForce sf = Global.Context.DMSSalesForces.FirstOrDefault(a => a.LoginID == model.UserName && a.Active == true);
                    if (sf != null)
                    {
                        try
                        {
                            DMSSFAssignment sfa = Global.Context.DMSSFAssignments.FirstOrDefault(a => a.EmployeeID == sf.EmployeeID && a.IsActive == true);
                            if (sfa != null)
                            {
                                SessionHelper.SetSession<string>("BranchRegionID", sfa.RegionID);
                                SessionHelper.SetSession<string>("BranchAreaID", sfa.AreaID);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                List<Feature> listF = (
                                     from u in Global.Context.UserProfiles
                                     where u.UserId == userID
                                     join ru in Global.Context.RoleUsers
                                     on u.UserId equals ru.UserID
                                     join r in Global.Context.Roles
                                     on ru.RoleID equals r.ID
                                     join rf in Global.Context.RoleFeatures
                                     on r.ID equals rf.RoleID
                                     join f in Global.Context.Features
                                     on rf.FeatureID equals f.ID
                                     select f
                                 ).Distinct().ToList();
                SessionHelper.SetSession<List<Feature>>("ListFeature", listF);
            }
        }

        public static string GroupByRole()
        {
            int roleID = SessionHelper.GetSession<int>("RoleUser"); 
            Role role = Global.Context.Roles.FirstOrDefault(a => a.ID == roleID);
            //Sy VN HARDCODE
            string groupby = "Region"; //"Region";

            if (role.RoleName.Equals("Admin") | role.RoleName.Equals("SuperAdmin"))
            {
                groupby = "Region";
            }
            else if (role.RoleName.Equals("RSM"))
            {
                groupby = "Area";
            }
            else if (role.RoleName.Equals("ASM"))
            {
                groupby = "Salesman";
            }
            else if (role.RoleName.Equals("SS"))
            {
                groupby = "Salesman";
            }
            else if (role.RoleName.Equals("TDV"))
            {
                groupby = "Salesman";
            }
            else if (role.RoleName.Equals("Distributor"))
            {
                groupby = "Salesman";
            }

            return groupby;
        }
    }
    #endregion

    #region class SessionHelper
    public class SessionHelper
    {
        public static T SetSession<T>(string sessionName, T obj)
        {
            HttpContext.Current.Session[sessionName.ToString()] = obj;
            return obj;
        }

        public static T GetSession<T>(string sessionName)
        {
            string sessionNameString = sessionName.ToString();
            if (HttpContext.Current.Session[sessionNameString] == null)
                return default(T);
            return (T)HttpContext.Current.Session[sessionNameString];
        }

        public static void ClearSession(string sessionName)
        {
            HttpContext.Current.Session.Remove(sessionName.ToString());
        }

        public static bool IsNull(string sessionName)
        {
            return HttpContext.Current.Session[sessionName.ToString()] == null;
        }
    }
    #endregion

    #region class FormatHelper
    public static class FormatHelper
    {
        public static string FormatDecimalMoney(this decimal? input)
        {
            string output = "0";
            if (input.HasValue)
            {
                output = input.Value.ToString("###,##0", CultureInfo.InvariantCulture);
            }
            return output;
        }

        public static string FormatDecimalLength(this decimal? input)
        {
            string output = "0.00";
            if (input.HasValue)
            {
                output = input.Value.ToString("###,##0.##", CultureInfo.InvariantCulture);
            }
            return output;
        }

        public static string FormatDecimalPercent(this decimal? input)
        {
            string output = "0%";
            if (input.HasValue)
            {
                output = (input.Value * 100).ToString("###,##0.##", CultureInfo.InvariantCulture) + "%";
            }
            return output;
        }

        public static string FormatDecimalMoney(this int? input)
        {
            string output = "0";
            if (input.HasValue)
            {
                output = input.Value.ToString("###,##0", CultureInfo.InvariantCulture);
            }
            return output;
        }

        public static string FormatDecimalMoney(this int input)
        {
            string output = "0";
            if (input != 0)
            {
                output = input.ToString("###,##0", CultureInfo.InvariantCulture);
            }
            return output;
        }

        public static string ToShortTimePattern(this TimeSpan? input)
        {
            string output = "";
            if (input.HasValue)
            {
                output = input.Value.Hours.ToString("00") + ":" + input.Value.Minutes.ToString("00");
            }
            return output;
        }

        public static string ToShortTimePattern(this TimeSpan input)
        {
            string output = "";
            output = input.Hours.ToString("00") + ":" + input.Minutes.ToString("00");
            return output;
        }

        public static string ToTimeSpanPattern(this TimeSpan input)
        {
            string output = "";//duration.ToString(@"dd\.hh\:mm\:ss"); 
            output = input.ToString(@"hh\:mm\:ss");//input.Hours.ToString("00") + ":" + input.Minutes.ToString("00") + ":" + input.Seconds.ToString("00");
            return output;
        }

        public static string ToShortMinutePattern(this TimeSpan input)
        {
            string output = "";
            output = input.Minutes.ToString("00") + ":" + input.Seconds.ToString("00");
            return output;
        }

        public static string ToShortDatePattern(this DateTime? input)
        {
            string output = "";
            if (input != null)
            {
                output = input.Value.ToString(Constant.ShortDatePattern);
            }
            return output;
        }

        public static string ToFullDateTimePattern(this DateTime? input)
        {
            string output = "";
            if (input != null)
            {
                output = input.Value.ToString(Constant.FullDateTimePattern);
            }
            return output;
        }
    }
    #endregion

    #region class ValidateExtension
    public static class ValidateExtension
    {
        public static string validateErrMes = string.Empty;

        #region ValidateString
        public static bool ValidateStringWhiteSpace(this string input, string fieldName)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                validateErrMes = "LBL_" + fieldName + "_WhiteSpaceError";
                result = false;
            }
            return result;
        }

        public static bool ValidateStringEmpty(this string input, string fieldName)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (string.IsNullOrEmpty(input))
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                result = false;
            }
            return result;
        }

        public static bool ValidateStringLengthMin(this string input, string fieldName, int minimumLength)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (minimumLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_MinError";
                    result = false;
                }

                input = input.Trim();
                if (input.Length < minimumLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MinError";
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringLengthMax(this string input, string fieldName, int maxLength)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (maxLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_MaxError";
                    result = false;
                }

                input = input.Trim();
                if (input.Length > maxLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MaxError";
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringLengthRange(this string input, string fieldName, int minimumLength, int maxLength)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (minimumLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_EmptyError";
                    return false;
                }

                input = input.Trim();

                if (input.Length < minimumLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MinError";
                    result = false;
                }

                if (input.Length > maxLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MaxError";
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringContain(this string input, string fieldName, string contain)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(contain))
            {
                if (!input.Contains(contain))
                {
                    validateErrMes = "LBL_" + fieldName + "_ContainError";
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringRegex(this string input, string fieldName, string regex, RegexOptions option)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(regex))
            {
                // Here we call Regex.Match.
                Match match = Regex.Match(input, regex, option);

                // Here we check the Match instance.
                if (!match.Success)
                {
                    validateErrMes = "LBL_" + fieldName + "_RegexError";
                    result = false;
                }
            }
            return result;
        }
        #endregion

        #region ValidateDecimal
        public static bool ValidateDecimalGreatZero(this decimal input, string fieldName)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input <= 0)
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalMin(this decimal input, string fieldName, decimal minimum)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalMax(this decimal input, string fieldName, decimal max)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalRange(this decimal input, string fieldName, decimal minimum, decimal max)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                result = false;
            }

            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                result = false;
            }
            return result;
        }
        #endregion

        #region ValidateInt
        public static bool ValidateIntGreatZero(this int input, string fieldName)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input <= 0)
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                result = false;
            }
            return result;
        }

        public static bool ValidateIntMin(this int input, string fieldName, int minimum)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                result = false;
            }
            return result;
        }

        public static bool ValidateIntMax(this int input, string fieldName, int max)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                result = false;
            }
            return result;
        }

        public static bool ValidateIntRange(this int input, string fieldName, int minimum, int max)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                result = false;
            }

            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                result = false;
            }
            return result;
        }
        #endregion

        #region ValidateString
        public static bool ValidateStringWhiteSpace(this string input, string fieldName, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                validateErrMes = "LBL_" + fieldName + "_WhiteSpaceError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }

        public static bool ValidateStringEmpty(this string input, string fieldName, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (string.IsNullOrEmpty(input))
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }

        public static bool ValidateStringLengthMin(this string input, string fieldName, int minimumLength, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (minimumLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_MinError";
                    controller.ModelState.AddModelError(fieldName, validateErrMes);
                    result = false;
                }

                input = input.Trim();
                if (input.Length < minimumLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MinError";
                    controller.ModelState.AddModelError(fieldName, validateErrMes);
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringLengthMax(this string input, string fieldName, int maxLength, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (maxLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_MaxError";
                    controller.ModelState.AddModelError(fieldName, validateErrMes);
                    result = false;
                }

                input = input.Trim();
                if (input.Length > maxLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MaxError";
                    controller.ModelState.AddModelError(fieldName, validateErrMes);
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringLengthRange(this string input, string fieldName, int minimumLength, int maxLength, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (minimumLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_EmptyError";
                    controller.ModelState.AddModelError(fieldName, validateErrMes);
                    result = false;
                }
                else
                {
                    input = input.Trim();
                    if (input.Length < minimumLength)
                    {
                        validateErrMes = "LBL_" + fieldName + "_MinError";
                        controller.ModelState.AddModelError(fieldName, validateErrMes);
                        result = false;
                    }

                    if (input.Length > maxLength)
                    {
                        validateErrMes = "LBL_" + fieldName + "_MaxError";
                        controller.ModelState.AddModelError(fieldName, validateErrMes);
                        result = false;
                    }
                }
            }
            return result;
        }

        public static bool ValidateStringContain(this string input, string fieldName, string contain, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(contain))
            {
                if (!input.Contains(contain))
                {
                    validateErrMes = "LBL_" + fieldName + "_ContainError";
                    controller.ModelState.AddModelError(fieldName, validateErrMes);
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringRegex(this string input, string fieldName, string regex, RegexOptions option, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(regex))
            {
                // Here we call Regex.Match.
                Match match = Regex.Match(input, regex, option);

                // Here we check the Match instance.
                if (!match.Success)
                {
                    validateErrMes = "LBL_" + fieldName + "_RegexError";
                    controller.ModelState.AddModelError(fieldName, validateErrMes);
                    result = false;
                }
            }
            return result;
        }
        #endregion

        #region ValidateDecimal
        public static bool ValidateDecimalGreatZero(this decimal input, string fieldName, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input <= 0)
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalMin(this decimal input, string fieldName, decimal minimum, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalMax(this decimal input, string fieldName, decimal max, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalRange(this decimal input, string fieldName, decimal minimum, decimal max, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }

            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }
        #endregion

        #region ValidateInt
        public static bool ValidateIntGreatZero(this int input, string fieldName, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input <= 0)
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }

        public static bool ValidateIntMin(this int input, string fieldName, int minimum, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }

        public static bool ValidateIntMax(this int input, string fieldName, int max, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }

        public static bool ValidateIntRange(this int input, string fieldName, int minimum, int max, Controller controller)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }

            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                controller.ModelState.AddModelError(fieldName, validateErrMes);
                result = false;
            }
            return result;
        }
        #endregion
    }
    #endregion

    #region LogAndRedirectOnError




    #endregion

    #region AutoMarking
    public static class AutoMarking
    {
        public static void ThreadProcessing(EvaluationMatlabClass matlab, List<EvaluationImageClass> imagelist, ref EvalAutoMarkMV results)
        {
            try
            {
                CustomLog.LogError("Start thread ");
                int ProcessType = 0;
                object output;
                object[] result;
                foreach (EvaluationImageClass elm in imagelist)
                {
                    //elm.CheckImageValidOrNot();
                    if (true)
                    {
                        try
                        {
                            ProcessType = 1; //Run check Image Real or Fake
                            output = null;
                            //CustomLog.LogError(" \n imageID: " + elm.CustomerImageID + "----image: " + elm.InputImagePath + "-------img2: " + elm.ComparedImagePath + "----path: " + matlab.BagPath);
                            matlab.MatlabObj.Feval(matlab.FunctionName, 10, out output, elm.InputImagePath, elm.ComparedImagePath, "None", matlab.RealFakeThreshold, matlab.StandardOrNotThreshold, matlab.PassOrNotThreshold, ProcessType, matlab.BagPath, matlab.MeanMhistRGBPath, matlab.VLFeat_LibPath);
                            result = output as object[];

                            elm.RealOrFakeResult = Convert.ToInt16(result[0]);
                            elm.PercentFake = Convert.ToDouble(result[3]);
                            elm.RealOrFakeTime = Convert.ToDouble(result[6]);
                            elm.RealOrFakeResult = (elm.PercentFake >= matlab.RealFakeThreshold) ? 0 : 1;
                            elm.ErrorStatus = elm.ErrorStatus + Convert.ToString(result[9]);
                            //CustomLog.LogError(" \n Real or Fake Ouput: " + elm.RealOrFakeResult.ToString() + ";" + elm.PercentFake.ToString() + ";" + elm.ErrorStatus);

                            ProcessType = 2;// Run check Image correlated with the outlet image
                            output = null;
                            matlab.MatlabObj.Feval(matlab.FunctionName, 10, out output, elm.InputImagePath, elm.ComparedImagePath, "None", matlab.RealFakeThreshold, matlab.StandardOrNotThreshold, matlab.PassOrNotThreshold, ProcessType, matlab.BagPath, matlab.MeanMhistRGBPath, matlab.VLFeat_LibPath);
                            result = output as object[];
                            elm.StandardOrNotResult = Convert.ToInt16(result[1]);
                            elm.NumCorrelation = Convert.ToDouble(result[4]);
                            elm.StandardOrNotTime = Convert.ToDouble(result[7]);
                            elm.StandardOrNotResult = (elm.NumCorrelation >= matlab.StandardOrNotThreshold) ? 1 : 0;
                            elm.ErrorStatus = elm.ErrorStatus + Convert.ToString(result[9]);
                            //CustomLog.LogError(" \n StandardOrNotResult: " + elm.RealOrFakeResult.ToString() + ";" + elm.PercentFake.ToString() + ";" + elm.ErrorStatus);

                            ProcessType = 3; //Run check Item exists or not
                            List<string> numberExist = new List<string>();
                            if (matlab.isNumberic)
                            {
                                //CustomLog.LogError("So: " + matlab.numNumericItem);
                                for (int k = 0; k < matlab.numNumericItem; k++)
                                {
                                    output = null;
                                    matlab.MatlabObj.Feval(matlab.FunctionName, 10, out output, elm.InputImagePath, elm.ComparedImagePath, matlab.arrItemImagePath[k], matlab.RealFakeThreshold, matlab.StandardOrNotThreshold, matlab.PassOrNotThreshold, ProcessType, matlab.BagPath, matlab.MeanMhistRGBPath, matlab.VLFeat_LibPath);
                                    result = output as object[];
                                    matlab.arrNumFeature[k] = Convert.ToDouble(result[5]);
                                    matlab.arrPassOrNotTime[k] = Convert.ToDouble(result[8]);
                                    matlab.arrPassOrNotResult[k] = (matlab.arrNumFeature[k] >= matlab.PassOrNotThreshold) ? 1 : 0;
                                    elm.ErrorStatus = elm.ErrorStatus + Convert.ToString(result[9]);
                                    if (matlab.arrPassOrNotResult[k] == 1)
                                    {
                                        numberExist.Add(matlab.arrItemNumbericID[k]);
                                    }
                                    //CustomLog.LogError("Numberic: " + matlab.arrPassOrNotResult[k].ToString() + ";" + elm.ErrorStatus);
                                }
                                elm.PassOrNotResult = matlab.arrPassOrNotResult.Aggregate(1, (a, b) => a * b);
                                elm.PassOrNotNumberic = matlab.arrPassOrNotResult.Aggregate(1, (a, b) => a * b);
                                elm.PassOrNotTime = matlab.arrPassOrNotTime.Aggregate(0.0, (a, b) => a + b) / matlab.numNumericItem;
                                elm.NumFeature = matlab.arrNumFeature.Aggregate(0.0, (a, b) => a + b);
                            }

                            elm.StandardOrNotResult = elm.StandardOrNotResult * elm.RealOrFakeResult;
                            elm.PassOrNotResult = elm.PassOrNotResult * elm.RealOrFakeResult * elm.StandardOrNotResult;
                            elm.ErrorStatus = (elm.ErrorStatus == "") ? "No Error" : elm.ErrorStatus;

                            using (NGVisibilityDataContext context = new NGVisibilityDataContext())
                            {
                                context.usp_UpdateAutoEvalOutletImageByID(
                                    elm.EvaluationID,
                                    elm.CustomerID,
                                    elm.CustomerImageID,
                                    elm.RealOrFakeResult,
                                    elm.StandardOrNotResult,
                                    elm.PassOrNotResult,
                                    string.Join(";", numberExist.ToArray())
                                    );
                            }
                            if (elm.RealOrFakeResult == 1)
                            {
                                results.ImgThat++;
                            }
                            else
                            {
                                results.ImgFakes++;
                            }
                            if (elm.StandardOrNotResult == 1)
                            {
                                results.ImgChuan++;
                            }
                            else
                            {
                                results.ImgNotStandard++;
                            }
                            if (elm.PassOrNotResult == 1)
                            {
                                results.ImgPass++;
                            }
                            else
                            {
                                results.ImgNotPass++;
                            }
                            if (numberExist.Count > 0)
                            {
                                results.ImgNumberic++;
                            }
                            else
                            {
                                results.ImgNotPassNumberic++;
                            }
                            results.TimeMarking = results.TimeMarking + elm.RealOrFakeTime + elm.StandardOrNotTime + elm.PassOrNotTime;
                            results.ImageMarking++;
                        }
                        catch (Exception ex)
                        {
                            CustomLog.LogError(ex);
                            results.ImgErrorMarking++;
                        }
                    }
                    else
                    {
                        results.ImgNotExist++;
                    }
                    results.ImagesProgress++;
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
        }
    }
    #endregion
}