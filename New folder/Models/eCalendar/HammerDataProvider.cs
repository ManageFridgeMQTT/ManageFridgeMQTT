using DevExpress.Web.Mvc;
using eRoute.Models.eCalendar;
using Hammer.Helpers;
using log4net;
using OfficeOpenXml;
//using Utility.Phrase("PrepareSchedule");
//using Utility.Phrase("ReportEmployeesStatus");
//using Utility.Phrase("SendCalendar");
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Security;
using WebMatrix.WebData;
using eRoute.Models.eCalendar;
using Hammer.Models;
using DMSERoute.Helpers;
//namespace Hammer.Models
namespace eRoute.Models.eCalendar
{
    public class HammerDataProvider
    {
        private const string HammerContextKey = "HammerDataContext";
        private static readonly ILog Log = LogManager.GetLogger(typeof(HammerDataProvider));
        private static MVCxAppointmentStorage defaultAppointmentStorage;
        private static MVCxAppointmentStorage customAppointmentStorage;
        private static MVCxResourceStorage defaultResourceStorage;
        public static HammerDataContext Context
        {
            get
            {
                if (System.Web.HttpContext.Current.Items["HammerDataContext"] == null)
                {
                    System.Web.HttpContext.Current.Items["HammerDataContext"] = new HammerDataContext();
                }
                return System.Web.HttpContext.Current.Items["HammerDataContext"] as HammerDataContext;
            }
        }
        public static MVCxAppointmentStorage DefaultAppointmentStorage
        {
            get
            {
                if (HammerDataProvider.defaultAppointmentStorage == null)
                {
                    HammerDataProvider.defaultAppointmentStorage = HammerDataProvider.CreateDefaultAppointmentStorage();
                }
                return HammerDataProvider.defaultAppointmentStorage;
            }
        }
        public static MVCxAppointmentStorage CustomAppointmentStorage
        {
            get
            {
                if (HammerDataProvider.customAppointmentStorage == null)
                {
                    HammerDataProvider.customAppointmentStorage = HammerDataProvider.CreateCustomAppointmentStorage();
                }
                return HammerDataProvider.customAppointmentStorage;
            }
        }
        public static MVCxResourceStorage DefaultResourceStorage
        {
            get
            {
                if (HammerDataProvider.defaultResourceStorage == null)
                {
                    HammerDataProvider.defaultResourceStorage = HammerDataProvider.CreateDefaultResourceStorage();
                }
                return HammerDataProvider.defaultResourceStorage;
            }
        }
        public static void ActionSaveLog(int userLogin)
        {
            string page = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            string action = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
            string url = string.Empty;
            if (System.Web.HttpContext.Current.Request.UrlReferrer != null)
            {
                url = System.Web.HttpContext.Current.Request.UrlReferrer.AbsolutePath;
            }
            Util.LogUserAction(page, action, null, userLogin, url);
        }
        public static bool ApproveScheduleCheckAssement(Appointment app)
        {
            NoAssessmentModel noAssessmentModel = new NoAssessmentModel();
            NoTrainingAssessment noTrainingAssessment = (
                from Header in Context.NoTrainingAssessments
                where Header.UserID.Trim() == app.UserLogin.Trim()
                && Header.AssessmentDate.Date == app.StartDate.Value.Date
                select Header).FirstOrDefault<NoTrainingAssessment>();
            if (noTrainingAssessment != null)
            {
                return true;
            }
            else
            {
                SMAssessmentModel rs = new SMAssessmentModel();
                rs.GetSMAssessmentModel();
                var query = (from Header in Context.SMTrainingAssessmentHeaders
                             where Header.UserID.Trim() == app.UserLogin.Trim()
                             && Header.AssessmentDate.Date == app.StartDate.Value.Date
                             select Header).FirstOrDefault();
                if (query != null)
                {
                    return true;
                }
                else
                {
                    var check = (from Header in Context.TrainingAssessmentHeaders
                                 where Header.UserID.Trim() == app.UserLogin.Trim()
                                 && Header.AssessmentDate.Date == app.StartDate.Value.Date
                                 select Header).FirstOrDefault();
                    if (check != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        public static void RequestDelete(int uniqueID)
        {
            Appointment parent = HammerDataProvider.Context.Appointments.SingleOrDefault((Appointment x) => x.UniqueID == uniqueID);
            if (parent != null)
            {
                IQueryable<Appointment> queryable =
                    from ap in HammerDataProvider.Context.Appointments
                    where ap.StartDate.GetValueOrDefault(System.DateTime.Now).Date == parent.StartDate.GetValueOrDefault(System.DateTime.Now).Date && ap.ScheduleType == parent.ScheduleType && ap.UserLogin == parent.UserLogin
                    select ap;
                foreach (Appointment current in queryable)
                {
                    current.Label = new int?(10);
                }
                HammerDataProvider.Context.SubmitChanges();
            }
        }
        public static void CancelRequestDelete(int uniqueID)
        {
            Appointment parent = HammerDataProvider.Context.Appointments.SingleOrDefault((Appointment x) => x.UniqueID == uniqueID);
            if (parent != null)
            {
                IQueryable<Appointment> queryable =
                    from ap in HammerDataProvider.Context.Appointments
                    where ap.StartDate.GetValueOrDefault(System.DateTime.Now).Date == parent.StartDate.GetValueOrDefault(System.DateTime.Now).Date && ap.ScheduleType == parent.ScheduleType && ap.UserLogin == parent.UserLogin
                    select ap;
                foreach (Appointment current in queryable)
                {
                    current.Label = new int?(3);
                }
                HammerDataProvider.Context.SubmitChanges();
            }
        }
        public static bool CheckPermissionRoles(string userLogin)
        {
            DMSSFHierarchy dMSSFHierarchy = (
                from sfAssignment in HammerDataProvider.Context.DMSSFHierarchies
                join sf in HammerDataProvider.Context.DMSSalesForces on sfAssignment.LevelID equals sf.SFLevel
                where sf.EmployeeID.Trim() == userLogin.Trim() && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            return dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true;
        }
        public static bool PrepareScheduleCheckApp(System.DateTime date, string userLogin, string type)
        {
            return !(type == "M") || (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UserLogin == userLogin && carSchedule.StartDate.Value.Date == date.Date && carSchedule.EndDate.Value.Date == date.Date && carSchedule.ScheduleType == type && (carSchedule.Label == (int?)0 || carSchedule.Label == (int?)2 || carSchedule.Label == (int?)1)
                select carSchedule).FirstOrDefault<Appointment>() != null;
        }
        public static bool CheckExistOpenDate(System.DateTime date, string userLogin)
        {
            bool result;
            try
            {
                result = (
                    from item in HammerDataProvider.Context.ScheduleSubmitSettings
                    where item.Date.Day == date.Day && item.Date.Month == date.Month && item.Date.Year == date.Year && item.EmployeeID.Trim() == userLogin.Trim() && item.Status == 0
                    select item).Any<ScheduleSubmitSetting>();
            }
            catch (System.Exception)
            {
                result = false;
            }
            return result;
        }
        public static void SaveSystem(SystemSetting ins)
        {
            SystemSetting systemSetting = (
                from sh in HammerDataProvider.Context.SystemSettings
                where sh.ID == ins.ID
                select sh).SingleOrDefault<SystemSetting>();
            systemSetting.UserLogin = ins.UserLogin;
            systemSetting.Number = ins.Number;
            systemSetting.CreatedDate = ins.CreatedDate;
            systemSetting.Desr = ins.Desr;
            HammerDataProvider.Context.SubmitChanges();
        }
        public static void DeletePeriod(string PeriodID)
        {
            PeriodSetting periodSetting = (
                from sh in HammerDataProvider.Context.PeriodSettings
                where sh.PeriodID == PeriodID
                select sh).SingleOrDefault<PeriodSetting>();
            if (periodSetting != null)
            {
                HammerDataProvider.Context.PeriodSettings.DeleteOnSubmit(periodSetting);
                HammerDataProvider.Context.SubmitChanges();
            }

        }
        public static bool CheckPeriod(string PeriodID)
        {
            var asscapa = (
                from sh in HammerDataProvider.Context.AssessmentCapacities
                where sh.PeriodID.Trim() == PeriodID.Trim()
                select sh).FirstOrDefault<AssessmentCapacity>();
            if (asscapa != null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public static void SavePeriod(PeriodSetting ins)
        {
            PeriodSetting periodSetting = (
                from sh in HammerDataProvider.Context.PeriodSettings
                where sh.PeriodID == ins.PeriodID
                select sh).SingleOrDefault<PeriodSetting>();
            if (periodSetting != null)
            {
                periodSetting.UserCreated = ins.UserCreated;
                periodSetting.DesEn = ins.DesEn;
                periodSetting.DesVN = ins.DesVN;
                periodSetting.CreatedDate = ins.CreatedDate;
                HammerDataProvider.Context.SubmitChanges();
            }
            else
            {
                HammerDataProvider.Context.PeriodSettings.InsertOnSubmit(ins);
                HammerDataProvider.Context.SubmitChanges();
            }
        }
        public static void SaveShift(ShiftSetting ins)
        {
            ShiftSetting shiftSetting = (
                from sh in HammerDataProvider.Context.ShiftSettings
                where sh.ShiftID == ins.ShiftID
                select sh).SingleOrDefault<ShiftSetting>();
            shiftSetting.StartTime = ins.StartTime;
            shiftSetting.EndTime = ins.EndTime;
            shiftSetting.UserLogin = ins.UserLogin;
            shiftSetting.DesEn = ins.DesEn;
            shiftSetting.DesVN = ins.DesVN;
            shiftSetting.CreatedDate = ins.CreatedDate;
            shiftSetting.Active = ins.Active;
            HammerDataProvider.Context.SubmitChanges();
        }
        public static SystemSetting GetSystemSetting(string ID)
        {
            return (
                from item in HammerDataProvider.Context.SystemSettings
                where item.ID.Trim() == ID.Trim()
                select item).SingleOrDefault<SystemSetting>();
        }
        public static System.Collections.Generic.List<SystemSetting> GetListSystem()
        {
            return (
                from item in HammerDataProvider.Context.SystemSettings
                select item).ToList<SystemSetting>().ToList<SystemSetting>();
        }
        public static System.Collections.Generic.List<ShiftSetting> GetListShift()
        {
            return (
                from item in HammerDataProvider.Context.ShiftSettings
                where item.Active == true
                select item into a

                orderby a.CreatedDate
                select a).ToList<ShiftSetting>().ToList<ShiftSetting>();
        }
        public static DMSHamResultType GetListResultType(char ID)
        {
            return (
                from item in HammerDataProvider.Context.DMSHamResultTypes
                where item.RefResult == ID
                select item into a
                select a).SingleOrDefault();
        }
        public static System.Collections.Generic.List<DMSHamResultType> GetListResultType()
        {
            return (
                from item in HammerDataProvider.Context.DMSHamResultTypes
                select item into a
                select a).ToList<DMSHamResultType>().ToList<DMSHamResultType>();
        }
        public static ShiftSetting GetShift(string ID)
        {
            return (
                from item in HammerDataProvider.Context.ShiftSettings
                where item.ShiftID.Trim() == ID.Trim()
                select item).SingleOrDefault<ShiftSetting>();
        }
        public static System.DateTime GetMinOpenDate(string userLogin)
        {
            System.Collections.Generic.List<ScheduleSubmitSetting> list = (
                from x in
                    (
                        from item in HammerDataProvider.Context.ScheduleSubmitSettings
                        where item.EmployeeID.Trim() == userLogin.Trim() && item.Type.Trim() == "D" && item.Status == 0
                        select item).ToList<ScheduleSubmitSetting>()
                orderby x.Date.Date
                select x).ToList<ScheduleSubmitSetting>();
            if (list == null || list.Count<ScheduleSubmitSetting>() <= 0)
            {
                return System.DateTime.Now.AddDays(1.0);
            }
            if (list[0].Date.Date >= System.DateTime.Now.AddDays(1.0).Date)
            {
                return System.DateTime.Now.AddDays(1.0);
            }
            return list[0].Date.Date;
        }
        public static string GetOutletName(string outletCD)
        {
            string result;
            try
            {
                result = (
                    from outlet in HammerDataProvider.Context.Outlets
                    where outlet.BaccountCD.Trim() == outletCD.Trim()
                    select outlet).SingleOrDefault<Outlet>().AcctName;
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = string.Empty;
            }
            return result;
        }
        public static bool IsValidWorkWith(string employeeID, System.DateTime workingDate)
        {
            Appointment appointment = (
                from ap in HammerDataProvider.Context.Appointments
                where ap.AllDay == (bool?)true && ap.UserLogin.Trim() == employeeID.Trim() && ap.StartDate.Value.Date == workingDate.Date && ap.Label == (int?)3 && ap.ScheduleType == "D"
                select ap).FirstOrDefault<Appointment>();
            return appointment != null;
        }
        public static bool IsValidMCP(string salepersonCD, System.DateTime workingDate)
        {
            return (
                from vs in HammerDataProvider.Context.DMSVisitPlanHistories
                where vs.SlsperID.Trim() == salepersonCD.Trim() && vs.VisitDate.Date == workingDate.Date
                select vs).Count<DMSVisitPlanHistory>() > 0;
        }
        public static System.Collections.Generic.List<EmployeeModel> GetDetailEmployeeID(System.Collections.Generic.List<DetailScheduleModel> List)
        {
            if (List != null)
            {
                System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
                List = List.Distinct<DetailScheduleModel>().ToList<DetailScheduleModel>();
                foreach (DetailScheduleModel current in List)
                {
                    EmployeeModel item = new EmployeeModel();
                    item = HammerDataProvider.PrepareScheduleGetlevel(current.EmployeeIDG);
                    list.Add(item);
                }
                return list;
            }
            return null;
        }
        public static List<EmployeeModel> GetSubordinateNoDuplicate(string userLogin)
        {
            if (userLogin == null)
                userLogin = String.Empty;
            string a = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            string a2 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == userLogin.Trim().ToUpper());
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == userLogin && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy == null)
            {
                return list;
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && !dMSSFHierarchy.Parent.HasValue)
            {
                a = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                a2 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                if (a != "ApproveSchedule")
                {
                    System.Collections.Generic.List<EmployeeModel> allEmployee = HammerDataProvider.GetAllEmployee(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = allEmployee.GetEnumerator())
                    {
                        EmployeeModel nsm;
                        while (enumerator.MoveNext())
                        {
                            nsm = enumerator.Current;
                            EmployeeModel employeeModel = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nsm.EmployeeID) == null)
                            {
                                list.Add(nsm);
                            }
                        }
                        return list;
                    }
                }
                if (a == "ViewSchedule" && a2 == "Index" || a == "ReportAssessmentTraining")
                {
                    System.Collections.Generic.List<EmployeeModel> list2 = HammerDataProvider.ViewScheduleGetEmployeeLevelup(dMSSalesForce.EmployeeID, dMSSFHierarchy.TerritoryType.Value);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list2.GetEnumerator())
                    {
                        EmployeeModel nv;
                        while (enumerator.MoveNext())
                        {
                            nv = enumerator.Current;
                            EmployeeModel employeeModel2 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nv.EmployeeID) == null)
                            {
                                list.Add(nv);
                            }
                        }
                        return list;
                    }
                }
                System.Collections.Generic.List<EmployeeModel> subordinateNSM = HammerDataProvider.GetSubordinateNSM(dMSSalesForce);
                foreach (EmployeeModel rsm in subordinateNSM)
                {
                    EmployeeModel employeeModel3 = new EmployeeModel();
                    //EmployeeModel rsm;
                    if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                    {
                        list.Add(rsm);
                    }
                }
                System.Collections.Generic.List<EmployeeModel> subordinateRSM = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        EmployeeModel employeeModel4 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                        {
                            list.Add(rsm);
                        }
                    }
                    return list;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == false)
            {
                if (a == "ViewSchedule" && a2 == "Index" || a == "ReportAssessmentTraining")
                {
                    System.Collections.Generic.List<EmployeeModel> list3 = HammerDataProvider.ViewScheduleGetEmployeeLevelup(dMSSalesForce.EmployeeID, dMSSFHierarchy.TerritoryType.Value);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list3.GetEnumerator())
                    {
                        EmployeeModel nv;
                        while (enumerator.MoveNext())
                        {
                            nv = enumerator.Current;
                            EmployeeModel employeeModel5 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nv.EmployeeID) == null)
                            {
                                list.Add(nv);
                            }
                        }
                        return list;
                    }
                }
                else if (a == "AssessmentCapacity")
                {
                    System.Collections.Generic.List<EmployeeModel> list3 = HammerDataProvider.ViewScheduleGetEmployeeLevelup(dMSSalesForce.EmployeeID, dMSSFHierarchy.TerritoryType.Value);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list3.GetEnumerator())
                    {
                        EmployeeModel nv;
                        while (enumerator.MoveNext())
                        {
                            nv = enumerator.Current;
                            EmployeeModel employeeModel5 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nv.EmployeeID) == null)
                            {
                                list.Add(nv);
                            }
                        }
                        return list;
                    }
                }
                System.Collections.Generic.List<EmployeeModel> allEmployee2 = HammerDataProvider.GetAllEmployee(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = allEmployee2.GetEnumerator())
                {
                    EmployeeModel nsm;
                    while (enumerator.MoveNext())
                    {
                        nsm = enumerator.Current;
                        EmployeeModel employeeModel6 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == nsm.EmployeeID) == null)
                        {
                            list.Add(nsm);
                        }
                    }
                    return list;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.Parent.HasValue)
            {
                a = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                a2 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                if (a == "ApproveSchedule" && a2 == "Index")
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateRSM2 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM2.GetEnumerator())
                    {
                        EmployeeModel rsm;
                        while (enumerator.MoveNext())
                        {
                            rsm = enumerator.Current;
                            EmployeeModel employeeModel7 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                            {
                                list.Add(rsm);
                            }
                        }
                        return list;
                    }
                }
                if (a == "SendCalendar" && a2 == "Index")
                {
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = dMSSalesForce.EmployeeID,
                        EmployeeName = dMSSalesForce.EmployeeName,
                        Level = dMSSFHierarchy.LevelName
                    });
                    return list;
                }
                if ((a == "PrepareSchedule" && a2 == "Index") || (a == "PrepareScheduleNew" && a2 == "Index"))
                {
                    //list.Add(new EmployeeModel
                    //{
                    //    EmployeeID = dMSSalesForce.EmployeeID,
                    //    EmployeeName = dMSSalesForce.EmployeeName,
                    //    Level = dMSSFHierarchy.LevelName
                    //});
                    //cho phep NSM chuan bi lich cho cac nhan vien cap duoi.
                    System.Collections.Generic.List<EmployeeModel> subordinateRSM3 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                    foreach (EmployeeModel rsm in subordinateRSM3)
                    {
                        if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                        {
                            list.Add(rsm);
                        }
                        System.Collections.Generic.List<EmployeeModel> subordinateASM = HammerDataProvider.GetSubordinateASM(rsm.EmployeeID);
                        foreach (EmployeeModel current2 in subordinateASM)
                        {
                            if (list.Find((EmployeeModel f) => f.EmployeeID == current2.EmployeeID) == null)
                            {
                                list.Add(current2);
                            }
                            System.Collections.Generic.List<EmployeeModel> subordinateSS = HammerDataProvider.GetSubordinateSS(current2.EmployeeID);
                            foreach (EmployeeModel current3 in subordinateSS)
                            {
                                if (list.Find((EmployeeModel f) => f.EmployeeID == current3.EmployeeID) == null)
                                {
                                    list.Add(current3);
                                }
                            }
                        }
                    }
                    return list;
                }
                if (a == "PrepareSchedule" && (a2 == "DetailPrepareSchedulePartialView" || a2 == "UpdateDetailSchedule"))
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateRSM3 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM3.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            EmployeeModel current = enumerator.Current;
                            System.Collections.Generic.List<EmployeeModel> subordinateASM = HammerDataProvider.GetSubordinateASM(current.EmployeeID);
                            foreach (EmployeeModel current2 in subordinateASM)
                            {
                                System.Collections.Generic.List<EmployeeModel> subordinateSS = HammerDataProvider.GetSubordinateSS(current2.EmployeeID);
                                foreach (EmployeeModel current3 in subordinateSS)
                                {
                                    System.Collections.Generic.List<EmployeeModel> subordinateSM = HammerDataProvider.GetSubordinateSM(current3.EmployeeID);
                                    foreach (EmployeeModel sm in subordinateSM)
                                    {
                                        EmployeeModel employeeModel8 = new EmployeeModel();
                                        if (list.Find((EmployeeModel f) => f.EmployeeID == sm.EmployeeID) == null)
                                        {
                                            sm.SS = current3.EmployeeID + "-" + current3.EmployeeName;
                                            sm.ASM = current2.EmployeeID + "-" + current2.EmployeeName;
                                            sm.RSM = current.EmployeeID + "-" + current.EmployeeName;
                                            list.Add(sm);
                                        }
                                    }
                                }
                            }
                        }
                        return list;
                    }
                }
                if (a == "ReportAssessmentTraining")
                {
                    System.Collections.Generic.List<EmployeeModel> list5 = HammerDataProvider.ViewScheduleGetEmployeeLevelup(dMSSalesForce.EmployeeID, dMSSFHierarchy.TerritoryType.Value);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list5.GetEnumerator())
                    {
                        EmployeeModel nv;
                        while (enumerator.MoveNext())
                        {
                            nv = enumerator.Current;
                            EmployeeModel employeeModel9 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nv.EmployeeID) == null)
                            {
                                list.Add(nv);
                            }
                        }
                        return list;
                    }
                }
                //if (!(a == "ViewSchedule") || !(a2 == "Index"))
                //{
                //    return list;
                //}
                System.Collections.Generic.List<EmployeeModel> list4 = HammerDataProvider.ViewScheduleGetEmployeeLevelup(dMSSalesForce.EmployeeID, dMSSFHierarchy.TerritoryType.Value);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list4.GetEnumerator())
                {
                    EmployeeModel nv;
                    while (enumerator.MoveNext())
                    {
                        nv = enumerator.Current;
                        EmployeeModel employeeModel9 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == nv.EmployeeID) == null)
                        {
                            list.Add(nv);
                        }
                    }
                    return list;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == false)
            {
                if (a == "ViewSchedule" && a2 == "Index" || a == "ReportAssessmentTraining")
                {
                    System.Collections.Generic.List<EmployeeModel> list5 = HammerDataProvider.ViewScheduleGetEmployeeLevelup(dMSSalesForce.EmployeeID, dMSSFHierarchy.TerritoryType.Value);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list5.GetEnumerator())
                    {
                        EmployeeModel nv;
                        while (enumerator.MoveNext())
                        {
                            nv = enumerator.Current;
                            EmployeeModel employeeModel10 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nv.EmployeeID) == null)
                            {
                                list.Add(nv);
                            }
                        }
                        return list;
                    }
                }
                else if (a == "AssessmentCapacity")
                {
                    System.Collections.Generic.List<EmployeeModel> list5 = HammerDataProvider.ViewScheduleGetEmployeeLevelup(dMSSalesForce.EmployeeID, dMSSFHierarchy.TerritoryType.Value);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list5.GetEnumerator())
                    {
                        EmployeeModel nv;
                        while (enumerator.MoveNext())
                        {
                            nv = enumerator.Current;
                            EmployeeModel employeeModel10 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nv.EmployeeID) == null)
                            {
                                list.Add(nv);
                            }
                        }
                        return list;
                    }
                }
                System.Collections.Generic.List<EmployeeModel> subordinateRSM4 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM4.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        EmployeeModel employeeModel11 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                        {
                            list.Add(rsm);
                        }
                        System.Collections.Generic.List<EmployeeModel> subordinateASM2 = HammerDataProvider.GetSubordinateASM(rsm.EmployeeID);
                        foreach (EmployeeModel asm in subordinateASM2)
                        {
                            EmployeeModel employeeModel12 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                            {
                                list.Add(asm);
                            }
                            System.Collections.Generic.List<EmployeeModel> subordinateSS2 = HammerDataProvider.GetSubordinateSS(asm.EmployeeID);
                            foreach (EmployeeModel ss in subordinateSS2)
                            {
                                EmployeeModel employeeModel13 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                                {
                                    list.Add(ss);
                                }
                            }
                        }
                    }
                    return list;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
            {
                if (a == "PrepareSchedule" && a2 == "Index")
                {
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = dMSSalesForce.EmployeeID,
                        EmployeeName = dMSSalesForce.EmployeeName,
                        Level = dMSSFHierarchy.LevelName
                    });
                }
                if (a == "PrepareSchedule" && (a2 == "DetailPrepareSchedulePartialView" || a2 == "UpdateDetailSchedule"))
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateASM3 = HammerDataProvider.GetSubordinateASM(dMSSalesForce.EmployeeID);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateASM3.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            EmployeeModel current4 = enumerator.Current;
                            System.Collections.Generic.List<EmployeeModel> subordinateSS3 = HammerDataProvider.GetSubordinateSS(current4.EmployeeID);
                            foreach (EmployeeModel current5 in subordinateSS3)
                            {
                                System.Collections.Generic.List<EmployeeModel> subordinateSM2 = HammerDataProvider.GetSubordinateSM(current5.EmployeeID);
                                foreach (EmployeeModel sm in subordinateSM2)
                                {
                                    EmployeeModel employeeModel14 = new EmployeeModel();
                                    if (list.Find((EmployeeModel f) => f.EmployeeID == sm.EmployeeID) == null)
                                    {
                                        sm.SS = current5.EmployeeID + "-" + current5.EmployeeName;
                                        sm.ASM = current4.EmployeeID + "-" + current4.EmployeeName;
                                        sm.RSM = dMSSalesForce.EmployeeID + "-" + dMSSalesForce.EmployeeName;
                                        list.Add(sm);
                                    }
                                }
                            }
                        }
                        return list;
                    }
                }
                if (a == "ViewSchedule" && a2 == "Index" || a == "ReportAssessmentTraining")
                {
                    System.Collections.Generic.List<EmployeeModel> list6 = HammerDataProvider.ViewScheduleGetEmployeeLevelup(dMSSalesForce.EmployeeID, dMSSFHierarchy.TerritoryType.Value);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list6.GetEnumerator())
                    {
                        EmployeeModel nv;
                        while (enumerator.MoveNext())
                        {
                            nv = enumerator.Current;
                            EmployeeModel employeeModel15 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nv.EmployeeID) == null)
                            {
                                list.Add(nv);
                            }
                        }
                        return list;
                    }
                }
                if (a == "PrepareScheduleNew" && a2 == "Process")
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateASM41 = HammerDataProvider.GetSubordinateASM(dMSSalesForce.EmployeeID);
                    foreach (EmployeeModel asm in subordinateASM41)
                    {
                        EmployeeModel employeeModel16 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                        {
                            list.Add(asm);
                        }
                    }
                    return list;
                }
                System.Collections.Generic.List<EmployeeModel> subordinateASM4 = HammerDataProvider.GetSubordinateASM(dMSSalesForce.EmployeeID);
                foreach (EmployeeModel asm in subordinateASM4)
                {
                    EmployeeModel employeeModel16 = new EmployeeModel();
                    if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                    {
                        list.Add(asm);
                    }
                }
                System.Collections.Generic.List<EmployeeModel> sSSMNotInASM = HammerDataProvider.GetSSSMNotInASM(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = sSSMNotInASM.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        EmployeeModel employeeModel17 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                        {
                            list.Add(ss);
                        }
                    }
                    return list;
                }
            }
            #region LASM
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == false)
            {
                if (a == "ViewSchedule" && a2 == "Index" || a == "ReportAssessmentTraining")
                {
                    System.Collections.Generic.List<EmployeeModel> list7 = HammerDataProvider.ViewScheduleGetSubordinateSS(dMSSalesForce.EmployeeID);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list7.GetEnumerator())
                    {
                        EmployeeModel ss;
                        while (enumerator.MoveNext())
                        {
                            ss = enumerator.Current;
                            EmployeeModel employeeModel18 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                            {
                                list.Add(ss);
                            }
                        }
                        return list;
                    }
                }
                else if (a == "AssessmentCapacity")
                {
                    System.Collections.Generic.List<EmployeeModel> list7 = HammerDataProvider.ViewScheduleGetSubordinateSS(dMSSalesForce.EmployeeID);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list7.GetEnumerator())
                    {
                        EmployeeModel ss;
                        while (enumerator.MoveNext())
                        {
                            ss = enumerator.Current;
                            EmployeeModel employeeModel18 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                            {
                                list.Add(ss);
                            }
                        }
                        return list;
                    }
                }
                System.Collections.Generic.List<EmployeeModel> list8 = HammerDataProvider.ViewScheduleGetSubordinateSS(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list8.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        EmployeeModel employeeModel19 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                        {
                            list.Add(ss);
                        }
                    }
                    return list;
                }
            }
            #endregion
            #region ASM
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
            {
                if (a == "PrepareSchedule" && (a2 == "DetailPrepareSchedulePartialView" || a2 == "UpdateDetailSchedule"))
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateSS4 = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSS4.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            EmployeeModel current6 = enumerator.Current;
                            System.Collections.Generic.List<EmployeeModel> subordinateSM3 = HammerDataProvider.GetSubordinateSM(current6.EmployeeID);
                            foreach (EmployeeModel sm in subordinateSM3)
                            {
                                EmployeeModel employeeModel20 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == sm.EmployeeID) == null)
                                {
                                    sm.SS = current6.EmployeeID + "-" + current6.EmployeeName;
                                    sm.ASM = dMSSalesForce.EmployeeID + "-" + dMSSalesForce.EmployeeName;
                                    list.Add(sm);
                                }
                            }
                        }
                        return list;
                    }
                }
                else if (a == "ViewSchedule" && a2 == "Index" || a == "ReportAssessmentTraining")
                {
                    System.Collections.Generic.List<EmployeeModel> list9 = HammerDataProvider.ViewScheduleGetSubordinateSS(dMSSalesForce.EmployeeID);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = list9.GetEnumerator())
                    {
                        EmployeeModel ss;
                        while (enumerator.MoveNext())
                        {
                            ss = enumerator.Current;
                            EmployeeModel employeeModel21 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                            {
                                list.Add(ss);
                            }
                        }
                        return list;
                    }
                }
                else
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateSS5 = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSS5.GetEnumerator())
                    {
                        EmployeeModel ss;
                        while (enumerator.MoveNext())
                        {
                            ss = enumerator.Current;
                            EmployeeModel employeeModel22 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                            {
                                list.Add(ss);
                            }
                        }
                        return list;
                    }
                }
            }
            #endregion
            if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == false)
            {
                list.Add(new EmployeeModel
                {
                    EmployeeID = dMSSalesForce.EmployeeID,
                    EmployeeName = dMSSalesForce.EmployeeName,
                    Level = dMSSFHierarchy.LevelName
                });
            }
            else
            {
                if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true)
                {
                    a = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                    a2 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                    if (a == "PrepareScheduleNew" || a == "PrepareSchedule")
                    {
                        System.Collections.Generic.List<EmployeeModel> subordinateSM4 = HammerDataProvider.GetSubordinateSM(dMSSalesForce.EmployeeID);
                        using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSM4.GetEnumerator())
                        {
                            EmployeeModel sm;
                            while (enumerator.MoveNext())
                            {
                                sm = enumerator.Current;
                                EmployeeModel employeeModel23 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == sm.EmployeeID) == null)
                                {
                                    sm.SS = dMSSalesForce.EmployeeID + "-" + dMSSalesForce.EmployeeName;
                                    list.Add(sm);
                                }
                            }
                            return list;
                        }
                    }
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = dMSSalesForce.EmployeeID,
                        EmployeeName = dMSSalesForce.EmployeeName,
                        Level = dMSSFHierarchy.LevelName
                    });
                }
            }
            return list;
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSubordinateDown(string userLogin)
        {
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == userLogin.Trim().ToUpper());
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == userLogin && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy == null)
            {
                return list;
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && !dMSSFHierarchy.Parent.HasValue)
            {
                System.Collections.Generic.List<EmployeeModel> allEmployee = HammerDataProvider.GetAllEmployee(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = allEmployee.GetEnumerator())
                {
                    EmployeeModel nsm;
                    while (enumerator.MoveNext())
                    {
                        nsm = enumerator.Current;
                        EmployeeModel employeeModel = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == nsm.EmployeeID) == null)
                        {
                            list.Add(nsm);
                        }
                    }
                    return list;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.Parent.HasValue)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateRSM = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator2 = subordinateRSM.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator2.MoveNext())
                    {
                        rsm = enumerator2.Current;
                        EmployeeModel employeeModel2 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                        {
                            list.Add(rsm);
                        }
                    }
                    return list;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateASM = HammerDataProvider.GetSubordinateASM(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator3 = subordinateASM.GetEnumerator())
                {
                    EmployeeModel asm;
                    while (enumerator3.MoveNext())
                    {
                        asm = enumerator3.Current;
                        EmployeeModel employeeModel3 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                        {
                            list.Add(asm);
                        }
                    }
                    return list;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateSS = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator4 = subordinateSS.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator4.MoveNext())
                    {
                        ss = enumerator4.Current;
                        EmployeeModel employeeModel4 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                        {
                            list.Add(ss);
                        }
                    }
                    return list;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateSM = HammerDataProvider.GetSubordinateSM(dMSSalesForce.EmployeeID);
                foreach (EmployeeModel ss in subordinateSM)
                {
                    EmployeeModel employeeModel5 = new EmployeeModel();
                    if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                    {
                        list.Add(ss);
                    }
                }
            }
            return list;
        }
        public static System.Collections.Generic.List<EmployeeModel> GetAllEmployee(DMSSalesForce emp)
        {
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            IQueryable<string> regions =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == emp.EmployeeID && sfAssignment.IsActive == true
                select sfAssignment.RegionID;
            if (regions.Count<string>() > 0)
            {
                IQueryable<EmployeeModel> source =
                    from sfa in HammerDataProvider.Context.DMSSFAssignments
                    join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where sfh.IsSalesForce == (bool?)true && regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)78 && sfh.Parent != null && sfa.IsActive == true
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                foreach (EmployeeModel current in source.Distinct<EmployeeModel>().ToList<EmployeeModel>())
                {
                    list.Add(current);
                }
                IQueryable<EmployeeModel> source2 =
                    from sfa in HammerDataProvider.Context.DMSSFAssignments
                    join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where  regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)82 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                foreach (EmployeeModel current2 in source2.Distinct<EmployeeModel>().ToList<EmployeeModel>())
                {
                    list.Add(current2);
                }
                IQueryable<EmployeeModel> source3 =
                    from sfa in HammerDataProvider.Context.DMSSFAssignments
                    join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where  regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)65 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                foreach (EmployeeModel current3 in source3.Distinct<EmployeeModel>().ToList<EmployeeModel>())
                {
                    list.Add(current3);
                }
                IQueryable<EmployeeModel> source4 =
                    from sfa in HammerDataProvider.Context.DMSSFAssignments
                    join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)68 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                foreach (EmployeeModel current4 in source4.Distinct<EmployeeModel>().ToList<EmployeeModel>())
                {
                    list.Add(current4);
                }
                return list;
            }
            IQueryable<EmployeeModel> source5 =
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where sfh.IsSalesForce == (bool?)true && (int?)sfh.TerritoryType == (int?)78 && sfh.Parent != null && sfh.IsSalesForce == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            foreach (EmployeeModel current5 in source5.Distinct<EmployeeModel>().ToList<EmployeeModel>())
            {
                list.Add(current5);
            }
            IQueryable<EmployeeModel> source6 =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  (int?)sfh.TerritoryType == (int?)82 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            foreach (EmployeeModel current6 in source6.Distinct<EmployeeModel>().ToList<EmployeeModel>())
            {
                list.Add(current6);
            }
            IQueryable<EmployeeModel> source7 =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  (int?)sfh.TerritoryType == (int?)65 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            foreach (EmployeeModel current7 in source7.Distinct<EmployeeModel>().ToList<EmployeeModel>())
            {
                list.Add(current7);
            }
            IQueryable<EmployeeModel> source8 =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  (int?)sfh.TerritoryType == (int?)68 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            foreach (EmployeeModel current8 in source8.Distinct<EmployeeModel>().ToList<EmployeeModel>())
            {
                list.Add(current8);
            }
            return list;
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSubordinateCDD(DMSSalesForce emp)
        {
            IQueryable<EmployeeModel> source =
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where sfh.IsSalesForce == (bool?)true && (int?)sfh.TerritoryType == (int?)78 && sfh.Parent == null && sf.Active == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSubordinateNSM(DMSSalesForce emp)
        {
            IQueryable<string> regions =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == emp.EmployeeID && sfAssignment.IsActive == true
                 && sfAssignment.IsBaseAssignment == true
                select sfAssignment.RegionID;
            if (regions.Count<string>() > 0)
            {
                IQueryable<EmployeeModel> source =
                    from sfa in HammerDataProvider.Context.DMSSFAssignments
                    join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where sfh.IsSalesForce == (bool?)true && regions.ToList<string>().Contains(sfa.RegionID)
                    && (int?)sfh.TerritoryType == (int?)78
                    && sfh.Parent != null && sfa.IsActive == true
                    && sf.Active == (bool?)true
                    && sfa.IsBaseAssignment == true
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
            }
            IQueryable<EmployeeModel> source2 =
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where sfh.IsSalesForce == (bool?)true && (int?)sfh.TerritoryType == (int?)78 && sfh.Parent != null && sf.Active == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            return source2.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSubordinateRSM(DMSSalesForce emp)
        {
            IQueryable<string> regions =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == emp.EmployeeID && sfAssignment.IsActive == true
                select sfAssignment.RegionID;
            if (regions.Count<string>() > 0)
            {
                IQueryable<EmployeeModel> source =
                    from sfa in HammerDataProvider.Context.DMSSFAssignments
                    join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where  regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)82 && sfh.IsSalesForce == (bool?)true && sf.Active == (bool?)true && sfa.IsBaseAssignment == (bool?)true
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
            }
            IQueryable<EmployeeModel> source2 =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  (int?)sfh.TerritoryType == (int?)82 && sfh.IsSalesForce == (bool?)true && sf.Active == (bool?)true && sfa.IsBaseAssignment == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            return source2.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSSSMNotInRSM(DMSSalesForce emp)
        {
            IQueryable<string> regions =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == emp.EmployeeID && sfAssignment.IsActive == true
                select sfAssignment.RegionID;
            if (regions.Count<string>() > 0)
            {
                IQueryable<EmployeeModel> source =
                    from sfa in HammerDataProvider.Context.DMSSFAssignments
                    join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where  regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)68 && sfh.IsSalesForce == (bool?)true && sf.Active == (bool?)true
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
            }
            IQueryable<EmployeeModel> source2 =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  (int?)sfh.TerritoryType == (int?)68 && sfh.IsSalesForce == (bool?)true && sf.Active == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            return source2.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSSSMNotInASM(string rsmID)
        {
            IQueryable<string> regions =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == rsmID.Trim() && sfAssignment.IsActive == true && sfAssignment.IsBaseAssignment == (bool?)true
                select sfAssignment.RegionID;
            IQueryable<EmployeeModel> source =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)68 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true && sf.Active == (bool?)true
                //&& sfa.IsBaseAssignment == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSubordinateASM(string rsmID)
        {
            IQueryable<string> regions =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == rsmID.Trim() && sfAssignment.IsActive == true && sfAssignment.IsBaseAssignment == (bool?)true
                select sfAssignment.RegionID;
            IQueryable<EmployeeModel> source =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)65 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true && sf.Active == (bool?)true && sfa.IsBaseAssignment == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> ViewScheduleGetEmployeeLevelup(string NVID, char type)
        {
            if (type == 'N')
            {
                IQueryable<string> regions =
                    from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                    where sfAssignment.EmployeeID.Trim() == NVID.Trim() && sfAssignment.IsActive == true
                    select sfAssignment.RegionID;
                if (regions.Count<string>() > 0)
                {
                    IQueryable<EmployeeModel> source =
                        from sfa in HammerDataProvider.Context.DMSSFAssignments
                        join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                        join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                        where sfh.IsSalesForce == (bool?)true && regions.ToList<string>().Contains(sfa.RegionID) && sfh.Parent != null && (int?)sfh.TerritoryType != (int?)((int)type) && sfa.IsActive == true
                        select new EmployeeModel
                        {
                            EmployeeID = sf.EmployeeID,
                            EmployeeName = sf.EmployeeName,
                            Level = sfh.LevelName
                        };
                    return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
                }
                IQueryable<EmployeeModel> source2 =
                    from sf in HammerDataProvider.Context.DMSSalesForces
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where sfh.IsSalesForce == (bool?)true && sfh.Parent != null
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                return source2.Distinct<EmployeeModel>().ToList<EmployeeModel>();
            }
            else
            {
                if (type == 'R')
                {
                    char[] allowedType = new char[]
					{
						'A',
						'D'
					};
                    IQueryable<string> regions =
                        from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                        where sfAssignment.EmployeeID.Trim() == NVID.Trim() && sfAssignment.IsActive == true
                        select sfAssignment.RegionID;
                    IQueryable<EmployeeModel> source3 =
                        from sfa in HammerDataProvider.Context.DMSSFAssignments
                        join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                        join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                        where allowedType.ToList<char>().Contains(sfa.ApplyTo) && regions.ToList<string>().Contains(sfa.RegionID) && allowedType.ToList<char>().Contains(sfh.TerritoryType.Value) && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                        select new EmployeeModel
                        {
                            EmployeeID = sf.EmployeeID,
                            EmployeeName = sf.EmployeeName,
                            Level = sfh.LevelName
                        };
                    return source3.Distinct<EmployeeModel>().ToList<EmployeeModel>();
                }
                if (type == 'A')
                {
                    IQueryable<string> areas =
                        from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                        where sfAssignment.EmployeeID.Trim() == NVID.Trim() && sfAssignment.IsActive == true
                        select sfAssignment.AreaID;
                    IQueryable<EmployeeModel> source4 =
                        from sfa in HammerDataProvider.Context.DMSSFAssignments
                        join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                        join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                        where  areas.ToList<string>().Contains(sfa.AreaID) && (int?)sfh.TerritoryType == (int?)68 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                        select new EmployeeModel
                        {
                            EmployeeID = sf.EmployeeID,
                            EmployeeName = sf.EmployeeName,
                            Level = sfh.LevelName
                        };
                    return source4.Distinct<EmployeeModel>().ToList<EmployeeModel>();
                }
                IQueryable<EmployeeModel> source5 =
                    from sf in HammerDataProvider.Context.DMSSalesForces
                    join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                    where sf.EmployeeID.Trim() == NVID.Trim() && (int?)sfh.TerritoryType == (int?)68 && sfh.IsSalesForce == (bool?)true
                    select new EmployeeModel
                    {
                        EmployeeID = sf.EmployeeID,
                        EmployeeName = sf.EmployeeName,
                        Level = sfh.LevelName
                    };
                return source5.Distinct<EmployeeModel>().ToList<EmployeeModel>();
            }
        }
        public static System.Collections.Generic.List<EmployeeModel> ViewScheduleGetSubordinateSS(string asmId)
        {
            IQueryable<string> areas =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == asmId.Trim() && sfAssignment.IsActive == true
                select sfAssignment.AreaID;
            IQueryable<EmployeeModel> source =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  areas.ToList<string>().Contains(sfa.AreaID) && (int?)sfh.TerritoryType == (int?)68 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSubordinateSS(string asmId)
        {           
              
            IQueryable<string> areas =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == asmId.Trim() && sfAssignment.IsActive == true && sfAssignment.IsBaseAssignment == (bool?)true
                select sfAssignment.AreaID;
            IQueryable<EmployeeModel> source =
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where areas.ToList<string>().Contains(sfa.AreaID) && sfh.TerritoryType == 'D' && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true && sf.Active == (bool?)true 
                //&& sfa.IsBaseAssignment == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                };
            return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static EmployeeModel GetRSMForASM(string ASM)
        {
            IQueryable<string> regions =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == ASM.Trim() && sfAssignment.IsActive == true && sfAssignment.IsBaseAssignment == (bool?)true
                select sfAssignment.RegionID;
            return (
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  regions.ToList<string>().Contains(sfa.RegionID) && (int?)sfh.TerritoryType == (int?)82 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true && sf.Active == (bool?)true && sfa.IsBaseAssignment == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                }).FirstOrDefault<EmployeeModel>();
        }
        public static EmployeeModel GetASMForSS(string SS)
        {
            IQueryable<string> ares =
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == SS.Trim() && sfAssignment.IsActive == true && sfAssignment.IsBaseAssignment == (bool?)true
                select sfAssignment.AreaID;
            return (
                from sfa in HammerDataProvider.Context.DMSSFAssignments
                join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where  ares.ToList<string>().Contains(sfa.AreaID) && (int?)sfh.TerritoryType == (int?)65 && sfh.IsSalesForce == (bool?)true && sfa.IsActive == true && sf.Active == (bool?)true && sfa.IsBaseAssignment == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                }).FirstOrDefault<EmployeeModel>();
        }
        public static EmployeeModel GetSSAssmentforUnique(Appointment model)
        {
            List<EmployeeModel> list = new List<EmployeeModel>();
            var query =
                from app in Context.Appointments
                where app.Employees.Trim() == model.Employees.Trim()
                 && app.StartDate.Value == model.StartDate.Value
                 && app.ScheduleType == "D"
                 && app.Label == 3
                select app;
            foreach (var item in query)
            {
                EmployeeModel ins = new EmployeeModel();
                ins = (from sfa in HammerDataProvider.Context.DMSSFAssignments
                       join sf in HammerDataProvider.Context.DMSSalesForces on sfa.EmployeeID equals sf.EmployeeID
                       join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                       where sfa.ApplyTo == 'D' && sfh.TerritoryType == 'D'
                       && sf.EmployeeID.Trim() == item.UserLogin.Trim()
                       && sfh.IsSalesForce == (bool?)true
                       && sfa.IsActive == true
                       && sf.Active == (bool?)true
                       && sfa.IsBaseAssignment == (bool?)true
                       select new EmployeeModel
                       {
                           EmployeeID = sf.EmployeeID,
                           EmployeeName = sf.EmployeeName,
                           Level = sfh.LevelName
                       }).FirstOrDefault<EmployeeModel>();
                if (ins != null)
                {
                    list.Add(ins);
                }
            }
            return list.FirstOrDefault();
        }
        public static EmployeeModel GetSMForSS(string SM)
        {
            return (
                from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                join sm in HammerDataProvider.Context.Salespersons on new
                {
                    CompanyID = smAssignment.CompanyID,
                    SalesperID = smAssignment.SalesPersonID.Value
                } equals new
                {
                    CompanyID = sm.CompanyID,
                    SalesperID = sm.SalespersonID
                }
                where sm.SalespersonCD.Trim() == SM.Trim()
                select new EmployeeModel
                {
                    SS = smAssignment.SalesSupID,
                    EmployeeID = sm.SalespersonCD,
                    EmployeeName = sm.Descr,
                    Level = "SM"
                }).FirstOrDefault<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> GetSubordinateSM(string salesSupID)
        {
            IQueryable<EmployeeModel> source =
                from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                join sm in HammerDataProvider.Context.Salespersons on new
                {
                    CompanyID = smAssignment.CompanyID,
                    SalesperID = smAssignment.SalesPersonID.Value
                } equals new
                {
                    CompanyID = sm.CompanyID,
                    SalesperID = sm.SalespersonID
                }
                where smAssignment.SalesSupID.Trim() == salesSupID.Trim()
                select new EmployeeModel
                {
                    EmployeeID = sm.SalespersonCD,
                    EmployeeName = sm.Descr,
                    Level = "SM",
                    RouteID = smAssignment.RouteCD
                };
            return source.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static SystemRole EmployeeInRole(string employeeID)
        {
            if (HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().Exists((DMSSalesForce x) => x.EmployeeID.Trim() == employeeID.Trim()))
            {
                return SystemRole.SalesForce;
            }
            if (HammerDataProvider.Context.Salespersons.ToList<Salesperson>().Exists((Salesperson x) => x.SalespersonCD.Trim() == employeeID.Trim()))
            {
                return SystemRole.Salesman;
            }
            return SystemRole.System;
        }
        public static System.Collections.Generic.List<DMSVisitPlanHistory> GetVisitPlanHistory(string salespersonCD, System.DateTime visitDate)
        {
            IOrderedQueryable<DMSVisitPlanHistory> source =
                from visitplan in HammerDataProvider.Context.DMSVisitPlanHistories
                where visitplan.SlsperID == salespersonCD && visitplan.VisitDate.Date == visitDate.Date
                orderby visitplan.VisitOrder
                select visitplan;
            return source.ToList<DMSVisitPlanHistory>();
        }
        public static Appointment GetAppointmentById(int id)
        {
            return (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UniqueID == id
                select ap).SingleOrDefault<Appointment>();
        }
        public static void DeleteDetailAppointment(int parentId)
        {
            try
            {
                Appointment parent = HammerDataProvider.Context.Appointments.SingleOrDefault((Appointment x) => x.UniqueID == parentId);
                if (parent != null)
                {
                    System.Collections.Generic.List<System.DateTime> openList = HammerDataProvider.GetOpenList(parent.UserLogin, parent.ScheduleType);
                    if (openList.Exists(a => a.Date.Date == parent.StartDate.Value.Date) == false)
                    {
                        int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
                        if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
                        {// Thoi diem config.
                            int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                            int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                            if (NoIntTime >= NoTime) // Sau Time
                            {
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                                DateTime MinnextWeek = nextWeek.Min();
                                if (parent.StartDate.Value.Date >= MinnextWeek.Date)
                                {
                                    IQueryable<Appointment> queryable =
                                        from ap in HammerDataProvider.Context.Appointments
                                        where ap.StartDate.GetValueOrDefault(System.DateTime.Now).Date == parent.StartDate.GetValueOrDefault(System.DateTime.Now).Date && ap.ScheduleType == parent.ScheduleType && ap.UserLogin == parent.UserLogin
                                        select ap;
                                    foreach (Appointment current in queryable)
                                    {
                                        HammerDataProvider.Context.Appointments.DeleteOnSubmit(current);
                                    }
                                    HammerDataProvider.Context.SubmitChanges();
                                }
                            }
                            else // truoc time
                            {
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                DateTime MinnextWeek = nextWeek.Min();
                                if (parent.StartDate.Value.Date >= MinnextWeek.Date)
                                {
                                    IQueryable<Appointment> queryable =
                                        from ap in HammerDataProvider.Context.Appointments
                                        where ap.StartDate.GetValueOrDefault(System.DateTime.Now).Date == parent.StartDate.GetValueOrDefault(System.DateTime.Now).Date && ap.ScheduleType == parent.ScheduleType && ap.UserLogin == parent.UserLogin
                                        select ap;
                                    foreach (Appointment current in queryable)
                                    {
                                        HammerDataProvider.Context.Appointments.DeleteOnSubmit(current);
                                    }
                                    HammerDataProvider.Context.SubmitChanges();
                                }
                            }
                        }
                        else // be hon ngay cf
                        {
                            List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                            DateTime MinnextWeek = nextWeek.Min();
                            if (parent.StartDate.Value.Date >= MinnextWeek.Date)
                            {
                                IQueryable<Appointment> queryable =
                                    from ap in HammerDataProvider.Context.Appointments
                                    where ap.StartDate.GetValueOrDefault(System.DateTime.Now).Date == parent.StartDate.GetValueOrDefault(System.DateTime.Now).Date && ap.ScheduleType == parent.ScheduleType && ap.UserLogin == parent.UserLogin
                                    select ap;
                                foreach (Appointment current in queryable)
                                {
                                    HammerDataProvider.Context.Appointments.DeleteOnSubmit(current);
                                }
                                HammerDataProvider.Context.SubmitChanges();
                            }
                        }
                    }
                    else // mo Ngày
                    {
                        IQueryable<Appointment> queryable =
                                           from ap in HammerDataProvider.Context.Appointments
                                           where ap.StartDate.GetValueOrDefault(System.DateTime.Now).Date == parent.StartDate.GetValueOrDefault(System.DateTime.Now).Date && ap.ScheduleType == parent.ScheduleType && ap.UserLogin == parent.UserLogin
                                           select ap;
                        foreach (Appointment current in queryable)
                        {
                            HammerDataProvider.Context.Appointments.DeleteOnSubmit(current);
                        }
                        HammerDataProvider.Context.SubmitChanges();
                    }
                }


                //Appointment parent = HammerDataProvider.Context.Appointments.SingleOrDefault((Appointment x) => x.UniqueID == parentId);
                //if (parent != null)
                //{
                //    IQueryable<Appointment> queryable =
                //        from ap in HammerDataProvider.Context.Appointments
                //        where ap.StartDate.GetValueOrDefault(System.DateTime.Now).Date == parent.StartDate.GetValueOrDefault(System.DateTime.Now).Date && ap.ScheduleType == parent.ScheduleType && ap.UserLogin == parent.UserLogin
                //        select ap;
                //    foreach (Appointment current in queryable)
                //    {
                //        HammerDataProvider.Context.Appointments.DeleteOnSubmit(current);
                //    }
                //    HammerDataProvider.Context.SubmitChanges();
                //}
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
            }
        }
        public static System.Collections.Generic.List<System.DateTime> GetOpenList(string employeeID, string scheduleType)
        {
            return (
                from setting in HammerDataProvider.Context.ScheduleSubmitSettings
                where setting.EmployeeID.Trim() == employeeID.Trim() && setting.Status == 0 && setting.Type.Trim() == scheduleType.Trim()
                select setting.Date.Date).ToList<System.DateTime>();
        }
        public static DMSSalesForce GetSalesforceByIdActive(string employeeID)
        {
            return (
                from sf in HammerDataProvider.Context.DMSSalesForces
                where sf.EmployeeID.Trim() == employeeID.Trim() && sf.Active == (bool?)true
                select sf).SingleOrDefault<DMSSalesForce>();
        }
        public static DMSSalesForce GetSalesforceById(string employeeID)
        {
            return (
                from sf in HammerDataProvider.Context.DMSSalesForces
                where sf.EmployeeID.Trim() == employeeID.Trim()
                select sf).SingleOrDefault<DMSSalesForce>();
        }
        public static void RejectAppointment(string employeeID, string scheduleType, string rejectReason, string emailTemplate, string userLogin)
        {
            System.Collections.Generic.List<System.DateTime> openList = HammerDataProvider.GetOpenList(employeeID, scheduleType);
            //Check ngay với đk:
            int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
            if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
            {// Thoi diem config.
                int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                if (NoIntTime >= NoTime) // Sau Time
                {
                    //Lay danh sach cac ngay cua tuan sau de khoa:
                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                    DateTime Max = nextWeek.Min();
                    IQueryable<Appointment> queryable =
                        from ap in HammerDataProvider.Context.Appointments
                        where (ap.StartDate.Value.Date >= Max.Date
                        || openList.Contains(ap.StartDate.Value.Date))
                        && (ap.Label == (int?)0 || ap.Label == (int?)2 || ap.Label == (int?)3) && ap.ScheduleType.Trim() == scheduleType.Trim() && ap.UserLogin.Trim().ToUpper() == employeeID.Trim().ToUpper()
                        select ap;
                    if (queryable.Count<Appointment>() <= 0)
                    {
                        return;
                    }
                    System.Collections.Generic.List<Appointment> source = new List<Appointment>();
                    foreach (Appointment current in queryable)
                    {
                        bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(current);
                        if (hasAssessment == false)
                        {
                            current.Label = new int?(1);
                            current.RejectReason = rejectReason;
                            source.Add(current);
                        }
                    }
                    HammerDataProvider.Context.SubmitChanges();
                    if (source.Count > 0)
                    {
                        UsersModel currentUser = HammerDataProvider.GetCurrentUser(userLogin);
                        DMSSalesForce salesforceById = HammerDataProvider.GetSalesforceById(employeeID);
                        Util.InitRejectSchedulerEmail(salesforceById.Email, salesforceById.EmployeeName, emailTemplate, (
                            from x in source
                            select x.StartDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>(), rejectReason, scheduleType, Util.GetBaseUrl() + "SendCalendar", currentUser.Role, currentUser.FullName);
                    }

                    //
                }
                else // nho hon gio config
                {

                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                    DateTime Max = nextWeek.Min();
                    IQueryable<Appointment> queryable =
                        from ap in HammerDataProvider.Context.Appointments
                        where (ap.StartDate.Value.Date >= Max.Date
                        || openList.Contains(ap.StartDate.Value.Date))
                        && (ap.Label == (int?)0 || ap.Label == (int?)2 || ap.Label == (int?)3) && ap.ScheduleType.Trim() == scheduleType.Trim() && ap.UserLogin.Trim().ToUpper() == employeeID.Trim().ToUpper()
                        select ap;
                    if (queryable.Count<Appointment>() <= 0)
                    {
                        return;
                    }
                    System.Collections.Generic.List<Appointment> source = new List<Appointment>();
                    foreach (Appointment current in queryable)
                    {
                        bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(current);
                        if (hasAssessment == false)
                        {
                            current.Label = new int?(1);
                            current.RejectReason = rejectReason;
                            source.Add(current);
                        }
                    }
                    HammerDataProvider.Context.SubmitChanges();
                    if (source.Count > 0)
                    {
                        UsersModel currentUser = HammerDataProvider.GetCurrentUser(userLogin);
                        DMSSalesForce salesforceById = HammerDataProvider.GetSalesforceById(employeeID);
                        Util.InitRejectSchedulerEmail(salesforceById.Email, salesforceById.EmployeeName, emailTemplate, (
                            from x in source
                            select x.StartDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>(), rejectReason, scheduleType, Util.GetBaseUrl() + "SendCalendar", currentUser.Role, currentUser.FullName);
                    }
                }
            }
            else
            { // ko cung thoi diem cf
                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                DateTime Min = nextWeek.Min();
                IQueryable<Appointment> queryable =
                from ap in HammerDataProvider.Context.Appointments
                where (ap.StartDate.Value.Date >= Min.Date
                || openList.Contains(ap.StartDate.Value.Date))
                && (ap.Label == (int?)0 || ap.Label == (int?)2 || ap.Label == (int?)3) && ap.ScheduleType.Trim() == scheduleType.Trim() && ap.UserLogin.Trim().ToUpper() == employeeID.Trim().ToUpper()
                select ap;
                if (queryable.Count<Appointment>() <= 0)
                {
                    return;
                }
                System.Collections.Generic.List<Appointment> source = new List<Appointment>();
                foreach (Appointment current in queryable)
                {
                    bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(current);
                    if (hasAssessment == false)
                    {
                        current.Label = new int?(1);
                        current.RejectReason = rejectReason;
                        source.Add(current);
                    }
                }
                HammerDataProvider.Context.SubmitChanges();
                if (source.Count > 0)
                {
                    UsersModel currentUser = HammerDataProvider.GetCurrentUser(userLogin);
                    DMSSalesForce salesforceById = HammerDataProvider.GetSalesforceById(employeeID);
                    Util.InitRejectSchedulerEmail(salesforceById.Email, salesforceById.EmployeeName, emailTemplate, (
                        from x in source
                        select x.StartDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>(), rejectReason, scheduleType, Util.GetBaseUrl() + "SendCalendar", currentUser.Role, currentUser.FullName);
                }
                //
            }
        }
        public static bool RejectTaskAppointment(int id, string reason, string emailTemplate)
        {
            Appointment appointment = (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UniqueID == id
                select ap).SingleOrDefault<Appointment>();
            if (appointment == null)
            {
                return false;
            }
            else
            {
                bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(appointment);
                if (hasAssessment == true)
                {
                    return false;
                }
                else
                {
                    System.Collections.Generic.List<System.DateTime> openList = HammerDataProvider.GetOpenList(appointment.UserLogin, appointment.ScheduleType);
                    if (openList.Exists(a => a.Date.Date == appointment.StartDate.Value.Date) == false)
                    {
                        if (appointment.StartDate.Value.Date > System.DateTime.Now.Date)
                        {
                            //Check ngay với đk:
                            int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
                            if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
                            {// Thoi diem config.
                                int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                                int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                                if (NoIntTime >= NoTime) // Sau Time
                                {
                                    //Lay danh sach cac ngay cua tuan sau de khoa:
                                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                                    DateTime MinnextWeek = nextWeek.Min();
                                    if (appointment.StartDate.Value.Date >= MinnextWeek.Date)
                                    {
                                        appointment.Label = new int?(1);
                                        appointment.RejectReason = reason;
                                        HammerDataProvider.Context.SubmitChanges();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else // nho hon gio config
                                {
                                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                    DateTime MinnextWeek = nextWeek.Min();
                                    if (appointment.StartDate.Value.Date >= MinnextWeek.Date)
                                    {
                                        appointment.Label = new int?(1);
                                        appointment.RejectReason = reason;
                                        HammerDataProvider.Context.SubmitChanges();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            { // ko cung thoi diem cf
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                DateTime Min = nextWeek.Min();
                                if (appointment.StartDate.Value.Date >= Min.Date) // Lớn hơn đầu ngày tuần sau đến mãi mãi
                                {
                                    appointment.Label = new int?(1);
                                    appointment.RejectReason = reason;
                                    HammerDataProvider.Context.SubmitChanges();
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (openList.Contains(appointment.StartDate.Value.Date))
                            {
                                appointment.Label = new int?(1);
                                appointment.RejectReason = reason;
                                HammerDataProvider.Context.SubmitChanges();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        appointment.Label = new int?(1);
                        appointment.RejectReason = reason;
                        HammerDataProvider.Context.SubmitChanges();
                        return true;
                    }
                }
            }

        }
        public static bool RejectAppointment(int id, string reason, string emailTemplate)
        {
            Appointment appointment = (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UniqueID == id
                select ap).SingleOrDefault<Appointment>();
            if (appointment == null)
            {
                return false;
            }
            else
            {
                bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(appointment);
                if (hasAssessment == true)
                {
                    return false;
                }
                else
                {
                    System.Collections.Generic.List<System.DateTime> openList = HammerDataProvider.GetOpenList(appointment.UserLogin, appointment.ScheduleType);
                    if (openList.Exists(a => a.Date.Date == appointment.StartDate.Value.Date) == false)
                    {
                        if (appointment.StartDate.Value.Date > System.DateTime.Now.Date)
                        {
                            //Check ngay với đk:
                            int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
                            if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
                            {// Thoi diem config.
                                int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                                int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                                if (NoIntTime >= NoTime) // Sau Time
                                {
                                    //Lay danh sach cac ngay cua tuan sau de khoa:
                                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                                    DateTime MinnextWeek = nextWeek.Min();
                                    if (appointment.StartDate.Value.Date >= MinnextWeek)
                                    {
                                        IQueryable<Appointment> queryable =
                                from detail in HammerDataProvider.Context.Appointments
                                where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType
                                select detail;
                                        foreach (Appointment current in queryable)
                                        {
                                            current.Label = new int?(1);
                                            current.RejectReason = reason;
                                        }
                                        appointment.Label = new int?(1);
                                        appointment.RejectReason = reason;
                                        HammerDataProvider.Context.SubmitChanges();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else // nho hon gio config
                                {
                                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                    DateTime MinnextWeek = nextWeek.Min();
                                    if (appointment.StartDate.Value.Date >= MinnextWeek)
                                    {
                                        IQueryable<Appointment> queryable =
                                from detail in HammerDataProvider.Context.Appointments
                                where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType
                                select detail;
                                        foreach (Appointment current in queryable)
                                        {
                                            current.Label = new int?(1);
                                            current.RejectReason = reason;
                                        }
                                        appointment.Label = new int?(1);
                                        appointment.RejectReason = reason;
                                        HammerDataProvider.Context.SubmitChanges();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            { // ko cung thoi diem cf
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                DateTime Min = nextWeek.Min();
                                if (appointment.StartDate.Value.Date >= Min.Date) // Lớn hơn đầu ngày tuần sau đến mãi mãi
                                {
                                    IQueryable<Appointment> queryable =
                                from detail in HammerDataProvider.Context.Appointments
                                where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType
                                select detail;
                                    foreach (Appointment current in queryable)
                                    {
                                        current.Label = new int?(1);
                                        current.RejectReason = reason;
                                    }
                                    appointment.Label = new int?(1);
                                    appointment.RejectReason = reason;
                                    HammerDataProvider.Context.SubmitChanges();
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            //Kiem DK                         
                        }
                        else
                        {
                            if (openList.Contains(appointment.StartDate.Value.Date))
                            {
                                IQueryable<Appointment> queryable =
                                from detail in HammerDataProvider.Context.Appointments
                                where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType
                                select detail;
                                foreach (Appointment current in queryable)
                                {
                                    current.Label = new int?(1);
                                    current.RejectReason = reason;
                                }
                                appointment.Label = new int?(1);
                                appointment.RejectReason = reason;
                                HammerDataProvider.Context.SubmitChanges();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        IQueryable<Appointment> queryable =
                                from detail in HammerDataProvider.Context.Appointments
                                where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType
                                select detail;
                        foreach (Appointment current in queryable)
                        {
                            current.Label = new int?(1);
                            current.RejectReason = reason;
                        }
                        appointment.Label = new int?(1);
                        appointment.RejectReason = reason;
                        HammerDataProvider.Context.SubmitChanges();
                        return true;
                    }
                }
            }

        }
        public static void ApproveAppointment(string employeeID, string scheduleType, string emailTemplate)
        {
            try
            {
                System.Collections.Generic.List<System.DateTime> openList = HammerDataProvider.GetOpenList(employeeID, scheduleType);
                //Check ngay với đk:
                int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
                if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
                {// Thoi diem config.
                    int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                    int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                    if (NoIntTime >= NoTime) // Sau Time
                    {
                        //Lay danh sach cac ngay cua tuan sau de khoa:
                        List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                        DateTime Min = nextWeek.Min();
                        IQueryable<Appointment> queryable =
                            from ap in HammerDataProvider.Context.Appointments
                            where ap.UserLogin == employeeID && ap.ScheduleType == scheduleType && ap.Label != (int?)3
                            && (ap.StartDate.Value.Date >= Min.Date || openList.Contains(ap.StartDate.Value.Date))
                            select ap;
                        if (queryable.Count<Appointment>() > 0)
                        {
                            System.Collections.Generic.List<Appointment> source = new List<Appointment>();
                            foreach (Appointment current in queryable)
                            {
                                bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(current);
                                if (hasAssessment == false)
                                {
                                    current.Label = new int?(3);
                                    source.Add(current);
                                }
                            }
                            HammerDataProvider.Context.SubmitChanges();
                            if (source.Count > 0)
                            {
                                DMSSalesForce salesforceById = HammerDataProvider.GetSalesforceById(employeeID);
                                Util.InitSchedulerEmail(salesforceById.Email, salesforceById.EmployeeName, emailTemplate, (
                                    from x in source
                                    select x.StartDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>(), scheduleType.Trim(), Util.GetBaseUrl());
                            }
                        }
                    }
                    else // Sau thoi gian config
                    {

                        List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                        DateTime Min = nextWeek.Min();
                        IQueryable<Appointment> queryable =
                            from ap in HammerDataProvider.Context.Appointments
                            where ap.UserLogin == employeeID && ap.ScheduleType == scheduleType && ap.Label != (int?)3
                            && (ap.StartDate.Value.Date >= Min.Date || openList.Contains(ap.StartDate.Value.Date))
                            select ap;
                        if (queryable.Count<Appointment>() > 0)
                        {
                            System.Collections.Generic.List<Appointment> source = new List<Appointment>();
                            foreach (Appointment current in queryable)
                            {
                                bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(current);
                                if (hasAssessment == false)
                                {
                                    current.Label = new int?(3);
                                    source.Add(current);
                                }
                            }
                            HammerDataProvider.Context.SubmitChanges();
                            if (source.Count > 0)
                            {
                                DMSSalesForce salesforceById = HammerDataProvider.GetSalesforceById(employeeID);
                                Util.InitSchedulerEmail(salesforceById.Email, salesforceById.EmployeeName, emailTemplate, (
                                    from x in source
                                    select x.StartDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>(), scheduleType.Trim(), Util.GetBaseUrl());
                            }
                        }
                    }
                }
                else
                { // ko cung thoi diem cf
                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                    DateTime Min = nextWeek.Min();
                    IQueryable<Appointment> queryable =
                            from ap in HammerDataProvider.Context.Appointments
                            where ap.UserLogin == employeeID && ap.ScheduleType == scheduleType && ap.Label != (int?)3
                            && (ap.StartDate.Value.Date >= Min.Date || openList.Contains(ap.StartDate.Value.Date))
                            select ap;
                    if (queryable.Count<Appointment>() > 0)
                    {
                        System.Collections.Generic.List<Appointment> source = new List<Appointment>();
                        foreach (Appointment current in queryable)
                        {
                            bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(current);
                            if (hasAssessment == false)
                            {
                                current.Label = new int?(3);
                                source.Add(current);
                            }
                        }
                        HammerDataProvider.Context.SubmitChanges();
                        if (source.Count > 0)
                        {
                            DMSSalesForce salesforceById = HammerDataProvider.GetSalesforceById(employeeID);
                            Util.InitSchedulerEmail(salesforceById.Email, salesforceById.EmployeeName, emailTemplate, (
                                from x in source
                                select x.StartDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>(), scheduleType.Trim(), Util.GetBaseUrl());
                        }
                    }
                }

                //IQueryable<Appointment> queryable =
                //    from ap in HammerDataProvider.Context.Appointments
                //    where ap.UserLogin == employeeID && ap.ScheduleType == scheduleType && ap.Label != (int?)3 
                //    && (ap.StartDate.Value.Date > System.DateTime.Now.Date || openList.Contains(ap.StartDate.Value.Date))
                //    select ap;
                //if (queryable.Count<Appointment>() > 0)
                //{
                //    System.Collections.Generic.List<Appointment> source = new List<Appointment>();
                //    foreach (Appointment current in queryable)
                //    {
                //        bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(current);
                //        if (hasAssessment == false)
                //        {
                //            current.Label = new int?(3);
                //            source.Add(current);
                //        }
                //    }
                //    HammerDataProvider.Context.SubmitChanges();
                //    if (source.Count > 0)
                //    {
                //        DMSSalesForce salesforceById = HammerDataProvider.GetSalesforceById(employeeID);
                //        Util.InitSchedulerEmail(salesforceById.Email, salesforceById.EmployeeName, emailTemplate, (
                //            from x in source
                //            select x.StartDate.Value.Date).Distinct<System.DateTime>().ToList<System.DateTime>(), scheduleType.Trim(), Util.GetBaseUrl());
                //    }
                //}
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
            }
        }
        public static bool ApproveTaskAppointment(int id, string emailTemplate, string UserAppro)
        {
            Appointment appointment = (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UniqueID == id
                select ap).SingleOrDefault<Appointment>();
            if (appointment == null)
            {
                return false;
            }
            else
            {
                bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(appointment);
                if (hasAssessment == true)
                {
                    return false;
                }
                else
                {
                    System.Collections.Generic.List<System.DateTime> openList = HammerDataProvider.GetOpenList(appointment.UserLogin, appointment.ScheduleType);
                    if (openList.Exists(a => a.Date.Date == appointment.StartDate.Value.Date) == false)
                    {
                        if (appointment.StartDate.Value.Date > System.DateTime.Now.Date)
                        {
                            //Check ngay với đk:
                            int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
                            if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
                            {// Thoi diem config.
                                int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                                int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                                if (NoIntTime >= NoTime) // Sau Time
                                {
                                    //Lay danh sach cac ngay cua tuan sau de khoa:
                                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                                    DateTime MinnextWeek = nextWeek.Min();
                                    if (appointment.StartDate.Value.Date >= MinnextWeek.Date)
                                    {
                                        appointment.UserAppro = UserAppro;
                                        appointment.Label = new int?(3);
                                        HammerDataProvider.Context.SubmitChanges();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else //truoc thoi gian config
                                {
                                    //Lay danh sach 
                                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                    DateTime MinnextWeek = nextWeek.Min();
                                    if (appointment.StartDate.Value.Date >= MinnextWeek.Date)
                                    {
                                        appointment.UserAppro = UserAppro;
                                        appointment.Label = new int?(3);
                                        HammerDataProvider.Context.SubmitChanges();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            { // be hơn  thoi diem cf
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                DateTime Min = nextWeek.Min();
                                if (appointment.StartDate.Value.Date >= Min.Date) // Lớn hơn đầu ngày tuần sau đến mãi mãi
                                {
                                    appointment.UserAppro = UserAppro;
                                    appointment.Label = new int?(3);
                                    HammerDataProvider.Context.SubmitChanges();
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (openList.Contains(appointment.StartDate.Value.Date))
                            {
                                appointment.UserAppro = UserAppro;
                                appointment.Label = new int?(3);
                                HammerDataProvider.Context.SubmitChanges();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        appointment.UserAppro = UserAppro;
                        appointment.Label = new int?(3);
                        HammerDataProvider.Context.SubmitChanges();
                        return true;
                    }
                }
            }


        }
        public static bool ApproveAppointment(int id, string emailTemplate, string UserAppro)
        {
            Appointment appointment = (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UniqueID == id
                select ap).SingleOrDefault<Appointment>();
            if (appointment == null)
            {
                return false;
            }
            else
            {
                bool hasAssessment = HammerDataProvider.ApproveScheduleCheckAssement(appointment);
                if (hasAssessment == true)
                {
                    return false;
                }
                else
                {
                    System.Collections.Generic.List<System.DateTime> openList = HammerDataProvider.GetOpenList(appointment.UserLogin, appointment.ScheduleType);
                    if (openList.Exists(a => a.Date.Date == appointment.StartDate.Value.Date) == false)
                    {
                        if (appointment.StartDate.Value.Date > System.DateTime.Now.Date)
                        {
                            //Check ngay với đk:
                            int NoDate = CultureHelper.GetNameDate(DateTime.Now.DayOfWeek);
                            if (NoDate >= HammerDataProvider.GetSystemSetting("LO").Number)
                            {// Thoi diem config.
                                int NoTime = HammerDataProvider.GetSystemSetting("LT").Number;
                                int NoIntTime = Convert.ToInt32(DateTime.Now.TimeOfDay.TotalSeconds);
                                if (NoIntTime >= NoTime) // Sau Time
                                {
                                    //Lay danh sach cac ngay cua tuan sau de khoa:
                                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now.AddDays(7), 7);
                                    DateTime MinNextWeek = nextWeek.Min();
                                    if (appointment.StartDate.Value.Date >= MinNextWeek.Date)
                                    {
                                        IQueryable<Appointment> queryable =
                                        from detail in HammerDataProvider.Context.Appointments
                                        where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType && detail.Label != (int?)3
                                        select detail;
                                        foreach (Appointment current in queryable)
                                        {
                                            current.Label = new int?(3);
                                        }
                                        appointment.UserAppro = UserAppro;
                                        appointment.Label = new int?(3);
                                        HammerDataProvider.Context.SubmitChanges();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    //Lay danh sach cac ngay cua tuan sau de khoa:
                                    List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                    DateTime MinNextWeek = nextWeek.Min();
                                    if (appointment.StartDate.Value.Date >= MinNextWeek.Date)
                                    {
                                        IQueryable<Appointment> queryable =
                                        from detail in HammerDataProvider.Context.Appointments
                                        where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType && detail.Label != (int?)3
                                        select detail;
                                        foreach (Appointment current in queryable)
                                        {
                                            current.Label = new int?(3);
                                        }
                                        appointment.UserAppro = UserAppro;
                                        appointment.Label = new int?(3);
                                        HammerDataProvider.Context.SubmitChanges();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            { // ko cung thoi diem cf
                                List<DateTime> nextWeek = CultureHelper.GetDateByNextWeek(DateTime.Now, 7);
                                DateTime Min = nextWeek.Min();
                                if (appointment.StartDate.Value.Date >= Min.Date) // Lớn hơn đầu ngày tuần sau đến mãi mãi
                                {
                                    IQueryable<Appointment> queryable =
                                 from detail in HammerDataProvider.Context.Appointments
                                 where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType && detail.Label != (int?)3
                                 select detail;
                                    foreach (Appointment current in queryable)
                                    {
                                        current.Label = new int?(3);
                                    }
                                    appointment.UserAppro = UserAppro;
                                    appointment.Label = new int?(3);
                                    HammerDataProvider.Context.SubmitChanges();
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            //Kiem DK                         
                        }
                        else
                        {
                            if (openList.Contains(appointment.StartDate.Value.Date))
                            {
                                IQueryable<Appointment> queryable =
                                from detail in HammerDataProvider.Context.Appointments
                                where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType && detail.Label != (int?)3
                                select detail;
                                foreach (Appointment current in queryable)
                                {
                                    current.Label = new int?(3);
                                }
                                appointment.UserAppro = UserAppro;
                                appointment.Label = new int?(3);
                                HammerDataProvider.Context.SubmitChanges();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        IQueryable<Appointment> queryable =
                                from detail in HammerDataProvider.Context.Appointments
                                where detail.StartDate.GetValueOrDefault(System.DateTime.Now).Date == appointment.StartDate.GetValueOrDefault(System.DateTime.Now).Date && detail.UserLogin == appointment.UserLogin && detail.ScheduleType == appointment.ScheduleType && detail.Label != (int?)3
                                select detail;
                        foreach (Appointment current in queryable)
                        {
                            current.Label = new int?(3);
                        }
                        appointment.UserAppro = UserAppro;
                        appointment.Label = new int?(3);
                        HammerDataProvider.Context.SubmitChanges();
                        return true;
                    }
                }
            }

        }
        public static System.Collections.IEnumerable GetAppointments(string employeeID, string scheduleType)
        {
            SystemSetting systemSetting = (
                from stt in HammerDataProvider.Context.SystemSettings
                where stt.ID == "AP"
                select stt).FirstOrDefault<SystemSetting>();
            if (systemSetting != null)
            {
                System.DateTime DateReview = System.DateTime.Now.AddMonths(systemSetting.Number * -1);
                List<Appointment> rs = new List<Appointment>();

                rs = (from ap in HammerDataProvider.Context.Appointments
                    where ap.UserLogin == employeeID && ap.ScheduleType == scheduleType
                    && ap.StartDate.Value.Date >= DateReview.Date && ap.IsDelete == false
                    select ap).ToList();
                return rs;
            }
            else
            {
                return null;
            }
        }
        public static System.Collections.IEnumerable GetAppointments(string employeeID, int label)
        {
            return
                from ap in HammerDataProvider.Context.Appointments
                where ap.UserLogin == employeeID && ap.Label.GetValueOrDefault(0) == label
                 && ap.IsDelete == false
                select ap;
        }
        public static System.Collections.Generic.List<Appointment> GetDetailAppointmentListTaskStatus(string employeeID, System.DateTime date)
        {
            System.Collections.Generic.List<Appointment> result;
            try
            {
                System.Collections.Generic.List<Appointment> list = (
                    from ap in HammerDataProvider.Context.Appointments
                    where ap.StartDate.Value.Day == date.Day && ap.StartDate.Value.Month == date.Month
                    && ap.StartDate.Value.Year == date.Year
                    && ap.ScheduleType.Trim() == "D"
                    && ap.UserLogin.Trim() == employeeID.Trim()                 
                    select ap).ToList<Appointment>();
                result = list;
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = null;
            }
            return result;
        }
        public static System.Collections.Generic.List<Appointment> GetDetailAppointmentListTask(string employeeID, System.DateTime date)
        {
            System.Collections.Generic.List<Appointment> result;
            try
            {
                System.Collections.Generic.List<Appointment> list = (
                    from ap in HammerDataProvider.Context.Appointments
                    where ap.StartDate.Value.Day == date.Day && ap.StartDate.Value.Month == date.Month
                    && ap.StartDate.Value.Year == date.Year
                    && ap.ScheduleType.Trim() == "D"
                    && ap.UserLogin.Trim() == employeeID.Trim()
                    && ap.IsDelete == false
                    select ap).ToList<Appointment>();
                result = list;
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = null;
            }
            return result;
        }
        public static System.Collections.Generic.List<Appointment> GetDetailAppointmentTask(string employeeID, System.DateTime date)
        {
            System.Collections.Generic.List<Appointment> result;
            try
            {
                System.Collections.Generic.List<Appointment> list = (
                    from ap in HammerDataProvider.Context.Appointments
                    where ap.StartDate.Value.Day == date.Day
                    && ap.StartDate.Value.Month == date.Month
                    && ap.StartDate.Value.Year == date.Year
                    && ap.Label.Value == 3
                    && ap.ScheduleType.Trim() == "D"
                    && ap.UserLogin.Trim() == employeeID.Trim()
                    && ap.IsDelete == false
                    select ap).ToList<Appointment>();
                result = list;
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = null;
            }
            return result;
        }
        public static System.Collections.Generic.List<Appointment> GetDetailAppointmentViewAssessment(string employeeID, System.DateTime date, bool type)
        {
            System.Collections.Generic.List<Appointment> result;
            try
            {
                if (type)
                {
                    System.Collections.Generic.List<Appointment> list = (
                        from ap in HammerDataProvider.Context.Appointments
                        where ap.StartDate.Value.Day == date.Day
                        && ap.StartDate.Value.Month == date.Month
                        && ap.StartDate.Value.Year == date.Year
                        && ap.Label.Value == 3 && ap.IsWW == true
                        && ap.ScheduleType.Trim() == "D"
                        && ap.UserLogin.Trim() == employeeID.Trim()
                         && ap.IsDelete == false
                        select ap).ToList<Appointment>();
                    result = list;
                }
                else
                {
                    System.Collections.Generic.List<Appointment> list2 = (
                        from ap in HammerDataProvider.Context.Appointments
                        where ap.StartDate.Value.Day == date.Day
                        && ap.StartDate.Value.Month == date.Month
                        && ap.StartDate.Value.Year == date.Year
                        && ap.Label.Value == 3 && ap.IsWW == false
                        && ap.ScheduleType.Trim() == "D"
                        && ap.UserLogin.Trim() == employeeID.Trim()
                         && ap.IsDelete == false
                        select ap).ToList<Appointment>();
                    result = list2;
                }
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = null;
            }
            return result;
        }
        public static Appointment GetDetailAppointment(string employeeID, System.DateTime date)
        {
            Appointment result;
            try
            {
                Appointment appointment = (
                    from ap in HammerDataProvider.Context.Appointments
                    where ap.StartDate.Value.Day == date.Day
                    && ap.StartDate.Value.Month == date.Month
                    && ap.StartDate.Value.Year == date.Year
                    && ap.Label.Value == 3 && ap.ScheduleType.Trim() == "D"
                    && ap.UserLogin.Trim() == employeeID.Trim()
                     && ap.IsDelete == false
                    select ap).FirstOrDefault<Appointment>();
                result = appointment;
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = null;
            }
            return result;
        }
        public static DMSVisitPlanHistory GetVisitPlanHistory(System.DateTime visitDate, string routeID)
        {
            return (
                from vp in HammerDataProvider.Context.DMSVisitPlanHistories
                where vp.VisitDate.Date == visitDate.Date && vp.RouteID.Trim() == routeID.Trim()
                select vp).FirstOrDefault<DMSVisitPlanHistory>();
        }
        public static string GetASMIdByAreaID(string areaID)
        {
            string result;
            try
            {
                DMSSFAssignment dMSSFAssignment = (
                    from sf in HammerDataProvider.Context.DMSSalesForces
                    join sfAssignmnet in HammerDataProvider.Context.DMSSFAssignments on sf.EmployeeID equals sfAssignmnet.EmployeeID
                    where (int)sf.AssignmentLevel == 65 && sfAssignmnet.IsActive == true && sfAssignmnet.AreaID.Trim() == areaID.Trim()
                    select sfAssignmnet).FirstOrDefault<DMSSFAssignment>();
                if (dMSSFAssignment != null)
                {
                    result = dMSSFAssignment.EmployeeID;
                }
                else
                {
                    result = string.Empty;
                }
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = string.Empty;
            }
            return result;
        }
        public static System.Collections.IEnumerable GetAppointments(string employeeID, int label, System.DateTime fromDate, System.DateTime toDate, string scheduleType)
        {
            label = 3;
            if (label == 4)
            {
                return
                    from ap in HammerDataProvider.Context.Appointments
                    where ap.UserLogin == employeeID && ap.StartDate.Value.Date >= fromDate.Date
                    && ap.EndDate.Value.Date <= toDate.Date
                    && ap.ScheduleType == scheduleType
                    && ap.IsDelete == false
                    select ap;
            }
            return
                from ap in HammerDataProvider.Context.Appointments
                where ap.UserLogin == employeeID && ap.Label.GetValueOrDefault(0) == label
                && ap.StartDate.Value.Date >= fromDate.Date
                && ap.StartDate.Value.Date <= toDate.Date && ap.ScheduleType == scheduleType
                  && ap.IsDelete == false
                select ap;
        }
        public static System.Collections.Generic.List<Appointment> GetDetailAppointments(System.DateTime date, string employeeID)
        {
            return (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UserLogin == employeeID && ap.StartDate.Value.Date == date.Date && ap.ScheduleType == "D"
                 && ap.IsDelete == false
                select ap into x
                orderby x.StartDate
                select x).ToList<Appointment>();
        }
        public static System.Collections.IEnumerable GetResource(string employeeID)
        {
            return
                from rs in HammerDataProvider.Context.Resources
                where rs.ResourceName == employeeID
                select rs;
        }
        public static ApproveScheduleObject GetApproveScheduleDataObject(string employeeID, string scheduleType)
        {
            return new ApproveScheduleObject
            {
                Appointments = HammerDataProvider.GetAppointments(employeeID, scheduleType),
                Resources = HammerDataProvider.GetResource(employeeID)
            };
        }
        public static System.Collections.Generic.List<Appointment> GetEditAppointments(string userLogin)
        {
            return (
                from schedule in HammerDataProvider.Context.Appointments
                where schedule.UserLogin == userLogin && schedule.AllDay == (bool?)true
                  && schedule.IsDelete == false
                select schedule).ToList<Appointment>();
        }
        public static System.Collections.Generic.List<Appointment> GetDetailAppointments(string userLogin)
        {
            return (
                from schedule in HammerDataProvider.Context.Appointments
                where schedule.UserLogin == userLogin && schedule.AllDay == (bool?)false
                  && schedule.IsDelete == false
                select schedule).ToList<Appointment>();
        }
        private static MVCxAppointmentStorage CreateDefaultAppointmentStorage()
        {
            return new MVCxAppointmentStorage
            {
                Mappings =
                {
                    AppointmentId = "UniqueID",
                    Start = "StartDate",
                    End = "EndDate",
                    Subject = "Subject",
                    Description = "Description",
                    Location = "Location",
                    AllDay = "AllDay",
                    Type = "Type",
                    RecurrenceInfo = "RecurrenceInfo",
                    ReminderInfo = "ReminderInfo",
                    Label = "Label",
                    Status = "Status",
                    ResourceId = "ResourceID"
                }
            };
        }
        private static MVCxAppointmentStorage CreateCustomAppointmentStorage()
        {
            MVCxAppointmentStorage mVCxAppointmentStorage = HammerDataProvider.CreateDefaultAppointmentStorage();
            mVCxAppointmentStorage.CustomFieldMappings.Add("Phone", "Phone");
            mVCxAppointmentStorage.CustomFieldMappings.Add("Employees", "Employees");
            mVCxAppointmentStorage.CustomFieldMappings.Add("RejectReason", "RejectReason");
            mVCxAppointmentStorage.CustomFieldMappings.Add("UserLogin", "UserLogin");
            return mVCxAppointmentStorage;
        }
        private static MVCxResourceStorage CreateDefaultResourceStorage()
        {
            return new MVCxResourceStorage
            {
                Mappings =
                {
                    ResourceId = "ResourceID",
                    Caption = "ResourceName"
                }
            };
        }
        public static void InsertAppointment(Appointment appt)
        {
            if (appt == null)
            {
                return;
            }
            HammerDataContext hammerDataContext = new HammerDataContext();
            appt.UniqueID = appt.GetHashCode();
            hammerDataContext.Appointments.InsertOnSubmit(appt);
            hammerDataContext.SubmitChanges();
        }
        public static void UpdateAppointment(Appointment appt)
        {
            if (appt == null)
            {
                return;
            }
            Appointment appointment = (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UniqueID == appt.UniqueID
                select carSchedule).SingleOrDefault<Appointment>();
            appointment.UniqueID = appt.UniqueID;
            appointment.StartDate = appt.StartDate;
            appointment.EndDate = appt.EndDate;
            appointment.AllDay = appt.AllDay;
            appointment.Subject = appt.Subject;
            appointment.Description = appt.Description;
            appointment.Location = appt.Location;
            appointment.RecurrenceInfo = appt.RecurrenceInfo;
            appointment.ReminderInfo = appt.ReminderInfo;
            appointment.Status = appt.Status;
            appointment.Type = appt.Type;
            appointment.Label = appt.Label;
            appointment.ResourceID = appt.ResourceID;
            appointment.Employees = appt.Employees;
            appointment.Phone = appt.Phone;
            appointment.UserAppro = appt.UserAppro;
            appointment.RouteID = appt.RouteID;
            appointment.ShiftID = appt.ShiftID;
            appointment.IsDelete = appt.IsDelete;
            HammerDataProvider.Context.SubmitChanges();
        }
        public static void RemoveAppointment(Appointment appt)
        {
            Appointment entity = (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UniqueID == appt.UniqueID
                select carSchedule).SingleOrDefault<Appointment>();
            HammerDataProvider.Context.Appointments.DeleteOnSubmit(entity);
            HammerDataProvider.Context.SubmitChanges();
        }
        public static SMAssessmentModel UploadSMAssessmentFromExcel(MemoryStream stream, string employeeID)
        {
            try
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    SMAssessmentModel model = new SMAssessmentModel();
                    model = model.GetSMAssessmentModel();
                    model.Header.Released = false;
                    //Check correct template
                    ExcelWorksheet ws = package.Workbook.Worksheets["SR"];
                    if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "vi-VN")
                    {
                        if (!ws.Cells["A1"].Value.ToString().Trim().ToLower().Contains(("BẢN ĐÁNH GIÁ SALES REP").ToLower()) ||
                        !ws.Cells["A2"].Value.ToString().Trim().ToLower().Contains(("Họ Tên Sales Rep").ToLower()) ||
                        !ws.Cells["A3"].Value.ToString().Trim().ToLower().Contains(("Khu vực").ToLower()) ||
                        !ws.Cells["C2"].Value.ToString().Trim().ToLower().Contains(("Mã công việc").ToLower()) ||
                        !ws.Cells["C3"].Value.ToString().Trim().ToLower().Contains(("Nhà phân phối").ToLower()))
                        {
                            return null;
                        }
                    }
                    else if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "en-US")
                    {
                        if (!ws.Cells["A1"].Value.ToString().Trim().ToLower().Contains(("REVIEW SALES REP").ToLower()) ||
                        !ws.Cells["A2"].Value.ToString().Trim().ToLower().Contains(("Full Name Sales Rep").ToLower()) ||
                        !ws.Cells["A3"].Value.ToString().Trim().ToLower().Contains(("Region").ToLower()) ||
                        !ws.Cells["C2"].Value.ToString().Trim().ToLower().Contains(("Unique I D").ToLower()) ||
                        !ws.Cells["C3"].Value.ToString().Trim().ToLower().Contains(("Distributor").ToLower()))
                        {
                            return null;
                        }
                    }
                    
                    string assessmentDateStr = ws.Cells["D2"].Value == null ? "" : ws.Cells["D2"].Value.ToString();
                    string assessmentFor = ws.Cells["B2"].Value == null ? "" : ws.Cells["B2"].Value.ToString();
                    string areaID = ws.Cells["B3"].Value == null ? "" : ws.Cells["B3"].Value.ToString();
                    string companyCD = ws.Cells["D3"].Value == null ? "" : ws.Cells["D3"].Value.ToString();

                    string salesObjective = ws.Cells["B4"].Value == null ? "" : ws.Cells["B4"].Value.ToString();
                    string trainingObjective = ws.Cells["B5"].Value == null ? "" : ws.Cells["B5"].Value.ToString();
                    string comment = ws.Cells["B47"].Value == null ? "" : ws.Cells["B47"].Value.ToString();
                    string nextTrainingObjective = ws.Cells["B48"].Value == null ? "" : ws.Cells["B48"].Value.ToString();
                    var assessmentDatelist = assessmentDateStr.Split('-');
                    string UniqueID = assessmentDatelist[0].ToString();
                    model.Header.UniqueID = Convert.ToInt32(UniqueID);
                    Appointment app = GetAppointmentById(model.Header.UniqueID.Value);
                    model.Header.AssessmentDate = app.StartDate.Value;

                    //if (!GetAssessmentDate(employeeID).Contains(model.Header.AssessmentDate.Date))
                    //{
                    //    return null;
                    //}

                    string[] companyCDSplit = companyCD.Split('-');
                    if (companyCDSplit.Length > 0)
                    {
                        companyCD = companyCDSplit[0];
                    }

                    var query = (from company in Context.Distributors
                                 where company.CompanyCD.Trim() == companyCD.Trim()
                                 select company).FirstOrDefault();

                    string[] assessmentForSplit = assessmentFor.Split('-');
                    if (assessmentForSplit.Length > 0)
                    {
                        assessmentFor = assessmentForSplit[0];
                    }
                    model.Header.AssessmentFor = assessmentFor;
                    model.Header.AreaID = areaID;
                    model.Header.DistributorID = query == null ? string.Empty : query.CompanyID.ToString();
                    model.Header.UserID = employeeID;
                    model.Header.TraningObjective = trainingObjective;
                    model.Header.SalesObjective = salesObjective;

                    model.Header.Comment = comment;
                    model.Header.NextTrainingObjective = nextTrainingObjective;

                    string scoreString = string.Empty;
                    int score = 1;

                    string pros = string.Empty;
                    string cons = string.Empty;
                    string note = string.Empty;

                    //1	Preparation	Chuẩn bị đầu ngày
                    scoreString = ws.Cells["B8"].Value == null ? "" : ws.Cells["B8"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C8"].Value == null ? "" : ws.Cells["C8"].Value.ToString();
                    cons = ws.Cells["D8"].Value == null ? "" : ws.Cells["D8"].Value.ToString();
                    note = ws.Cells["E8"].Value == null ? "" : ws.Cells["E8"].Value.ToString();
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 1).UniqueID = app.UniqueID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 1).AssessmentDate = app.StartDate.Value;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 1).AssessmentFor = assessmentFor;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 1).UserID = employeeID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 1).CriteriaScore = score;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 1).Note = note;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 1).Pros = pros;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 1).Cons = cons;
                    //2	Outlet visiting	Thăm viếng cửa hàng
                    scoreString = ws.Cells["B11"].Value == null ? "" : ws.Cells["B11"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C11"].Value == null ? "" : ws.Cells["C11"].Value.ToString();
                    cons = ws.Cells["D11"].Value == null ? "" : ws.Cells["D11"].Value.ToString();
                    note = ws.Cells["E11"].Value == null ? "" : ws.Cells["E11"].Value.ToString();

                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 2).UniqueID = app.UniqueID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 2).AssessmentDate = app.StartDate.Value;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 2).AssessmentFor = assessmentFor;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 2).UserID = employeeID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 2).CriteriaScore = score;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 2).Note = note;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 2).Pros = pros;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 2).Cons = cons;
                    //3	End day	Kết thúc cuối ngày
                    scoreString = ws.Cells["B15"].Value == null ? "" : ws.Cells["B15"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C15"].Value == null ? "" : ws.Cells["C15"].Value.ToString();
                    cons = ws.Cells["D15"].Value == null ? "" : ws.Cells["D15"].Value.ToString();
                    note = ws.Cells["E15"].Value == null ? "" : ws.Cells["E15"].Value.ToString();

                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 3).UniqueID = app.UniqueID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 3).AssessmentDate = app.StartDate.Value;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 3).AssessmentFor = assessmentFor;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 3).UserID = employeeID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 3).CriteriaScore = score;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 3).Note = note;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 3).Pros = pros;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 3).Cons = cons;
                    //4	Routebook PDA	Routebook PDA
                    scoreString = ws.Cells["B19"].Value == null ? "" : ws.Cells["B19"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C19"].Value == null ? "" : ws.Cells["C19"].Value.ToString();
                    cons = ws.Cells["D19"].Value == null ? "" : ws.Cells["D19"].Value.ToString();
                    note = ws.Cells["E19"].Value == null ? "" : ws.Cells["E19"].Value.ToString();

                    model.Tools.SingleOrDefault(x => x.CriteriaID == 4).UniqueID = app.UniqueID;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 4).AssessmentDate = app.StartDate.Value;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 4).AssessmentFor = assessmentFor;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 4).UserID = employeeID;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 4).CriteriaScore = score;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 4).Note = note;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 4).Pros = pros;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 4).Cons = cons;
                    //7	Brochure	Tài liệu dụng cụ chào hàng
                    scoreString = ws.Cells["B20"].Value == null ? "" : ws.Cells["B20"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C20"].Value == null ? "" : ws.Cells["C20"].Value.ToString();
                    cons = ws.Cells["D20"].Value == null ? "" : ws.Cells["D20"].Value.ToString();
                    note = ws.Cells["E20"].Value == null ? "" : ws.Cells["E20"].Value.ToString();

                    model.Tools.SingleOrDefault(x => x.CriteriaID == 7).UniqueID = app.UniqueID;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 7).AssessmentDate = app.StartDate.Value;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 7).AssessmentFor = assessmentFor;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 7).UserID = employeeID;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 7).CriteriaScore = score;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 7).Note = note;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 7).Pros = pros;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 7).Cons = cons;
                    //8	Display program tools document	Tài liệu dụng cụ trưng bày
                    scoreString = ws.Cells["B21"].Value == null ? "" : ws.Cells["B21"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C21"].Value == null ? "" : ws.Cells["C21"].Value.ToString();
                    cons = ws.Cells["D21"].Value == null ? "" : ws.Cells["D21"].Value.ToString();
                    note = ws.Cells["E21"].Value == null ? "" : ws.Cells["E21"].Value.ToString();

                    model.Tools.SingleOrDefault(x => x.CriteriaID == 8).UniqueID = app.UniqueID;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 8).AssessmentDate = app.StartDate.Value;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 8).AssessmentFor = assessmentFor;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 8).UserID = employeeID;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 8).CriteriaScore = score;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 8).Note = note;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 8).Pros = pros;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 8).Cons = cons;
                    //9	Briefcase/ Cặp đựng hồ sơ
                    scoreString = ws.Cells["B22"].Value == null ? "" : ws.Cells["B22"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C22"].Value == null ? "" : ws.Cells["C22"].Value.ToString();
                    cons = ws.Cells["D22"].Value == null ? "" : ws.Cells["D22"].Value.ToString();
                    note = ws.Cells["E22"].Value == null ? "" : ws.Cells["E22"].Value.ToString();

                    model.Tools.SingleOrDefault(x => x.CriteriaID == 9).UniqueID = app.UniqueID;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 9).AssessmentDate = app.StartDate.Value;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 9).AssessmentFor = assessmentFor;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 9).UserID = employeeID;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 9).CriteriaScore = score;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 9).Note = note;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 9).Pros = pros;
                    model.Tools.SingleOrDefault(x => x.CriteriaID == 9).Cons = cons;

                    //10Plan review	Xem lại kế hoạch
                    scoreString = ws.Cells["B24"].Value == null ? "" : ws.Cells["B24"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C24"].Value == null ? "" : ws.Cells["C24"].Value.ToString();
                    cons = ws.Cells["D24"].Value == null ? "" : ws.Cells["D24"].Value.ToString();
                    note = ws.Cells["E24"].Value == null ? "" : ws.Cells["E24"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 10).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 10).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 10).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 10).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 10).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 10).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 10).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 10).Cons = cons;
                    //11Sales opening	Mở đầu cuộc bán hàng
                    scoreString = ws.Cells["B28"].Value == null ? "" : ws.Cells["B28"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C28"].Value == null ? "" : ws.Cells["C28"].Value.ToString();
                    cons = ws.Cells["D28"].Value == null ? "" : ws.Cells["D28"].Value.ToString();
                    note = ws.Cells["E28"].Value == null ? "" : ws.Cells["E28"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 11).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 11).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 11).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 11).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 11).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 11).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 11).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 11).Cons = cons;
                    //12	Outlet considering	Xem xét cửa hàng
                    scoreString = ws.Cells["B30"].Value == null ? "" : ws.Cells["B30"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C30"].Value == null ? "" : ws.Cells["C30"].Value.ToString();
                    cons = ws.Cells["D30"].Value == null ? "" : ws.Cells["D30"].Value.ToString();
                    note = ws.Cells["E30"].Value == null ? "" : ws.Cells["E30"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 12).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 12).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 12).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 12).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 12).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 12).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 12).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 12).Cons = cons;
                    //13	Offers	Chào hàng
                    scoreString = ws.Cells["B33"].Value == null ? "" : ws.Cells["B33"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C33"].Value == null ? "" : ws.Cells["C33"].Value.ToString();
                    cons = ws.Cells["D33"].Value == null ? "" : ws.Cells["D33"].Value.ToString();
                    note = ws.Cells["E33"].Value == null ? "" : ws.Cells["E33"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 13).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 13).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 13).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 13).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 13).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 13).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 13).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 13).Cons = cons;
                    //14	Offer finishing	Kết thúc chào hàng
                    scoreString = ws.Cells["B36"].Value == null ? "" : ws.Cells["B36"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C36"].Value == null ? "" : ws.Cells["C36"].Value.ToString();
                    cons = ws.Cells["D36"].Value == null ? "" : ws.Cells["D36"].Value.ToString();
                    note = ws.Cells["E36"].Value == null ? "" : ws.Cells["E36"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 14).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 14).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 14).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 14).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 14).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 14).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 14).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 14).Cons = cons;
                    //15	Take note	Ghi chép số liệu
                    scoreString = ws.Cells["B38"].Value == null ? "" : ws.Cells["B38"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C38"].Value == null ? "" : ws.Cells["C38"].Value.ToString();
                    cons = ws.Cells["D38"].Value == null ? "" : ws.Cells["D38"].Value.ToString();
                    note = ws.Cells["E38"].Value == null ? "" : ws.Cells["E38"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 15).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 15).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 15).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 15).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 15).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 15).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 15).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 15).Cons = cons;
                    //17	Product display	Trưng bày hàng hóa
                    scoreString = ws.Cells["B40"].Value == null ? "" : ws.Cells["B40"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C40"].Value == null ? "" : ws.Cells["C40"].Value.ToString();
                    cons = ws.Cells["D40"].Value == null ? "" : ws.Cells["D40"].Value.ToString();
                    note = ws.Cells["E40"].Value == null ? "" : ws.Cells["E40"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 17).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 17).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 17).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 17).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 17).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 17).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 17).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 17).Cons = cons;
                    //18	Chup anh
                    scoreString = ws.Cells["B43"].Value == null ? "" : ws.Cells["B43"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C43"].Value == null ? "" : ws.Cells["C43"].Value.ToString();
                    cons = ws.Cells["D43"].Value == null ? "" : ws.Cells["D43"].Value.ToString();
                    note = ws.Cells["E43"].Value == null ? "" : ws.Cells["E43"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 18).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 18).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 18).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 18).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 18).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 18).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 18).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 18).Cons = cons;
                    //19 tong ket danh gia
                    scoreString = ws.Cells["B44"].Value == null ? "" : ws.Cells["B44"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C44"].Value == null ? "" : ws.Cells["C44"].Value.ToString();
                    cons = ws.Cells["D44"].Value == null ? "" : ws.Cells["D44"].Value.ToString();
                    note = ws.Cells["E44"].Value == null ? "" : ws.Cells["E44"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 19).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 19).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 19).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 19).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 19).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 19).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 19).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 19).Cons = cons;

                    //20 nhung muc jhac
                    scoreString = ws.Cells["B46"].Value == null ? "" : ws.Cells["B46"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C46"].Value == null ? "" : ws.Cells["C46"].Value.ToString();
                    cons = ws.Cells["D46"].Value == null ? "" : ws.Cells["D46"].Value.ToString();
                    note = ws.Cells["E46"].Value == null ? "" : ws.Cells["E46"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 20).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 20).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 20).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 20).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 20).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 20).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 20).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 20).Cons = cons;
                    return model;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return null;
            }
        }

        /// <summary>
        /// Read excel file for SS Assessment
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static AssessmentModel UploadAssessmentFromExcel(MemoryStream stream, string employeeID)
        {
            try
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    AssessmentModel model = new AssessmentModel();
                    model.Header.Released = false;
                    //Check correct template
                    ExcelWorksheet ws = package.Workbook.Worksheets["DANHGIASS"];
                    if (!ws.Cells["A1"].Value.ToString().Trim().ToLower().Contains(("ĐÁNH GIÁ ViỆC THỰC HiỆN FIELD COACHING - DÀNH CHO ASM/RSM").ToLower()) ||
                        !ws.Cells["A2"].Value.ToString().Trim().ToLower().Contains(("Tên Sale Sup").ToLower()) ||
                        !ws.Cells["A3"].Value.ToString().Trim().ToLower().Contains(("Sales Rep được huấn luyện").ToLower()) ||
                        !ws.Cells["C2"].Value.ToString().Trim().ToLower().Contains(("ASM phụ trách").ToLower()) ||
                        !ws.Cells["C3"].Value.ToString().Trim().ToLower().Contains(("NPP").ToLower()) ||
                        !ws.Cells["A4"].Value.ToString().Trim().ToLower().Contains(("Mã công việc").ToLower()) ||
                        !ws.Cells["A5"].Value.ToString().Trim().ToLower().Contains(("Ngày gửi đánh giá").ToLower()))
                    {
                        return null;
                    }
                    string assessmentDateStr = ws.Cells["B4"].Value == null ? "" : ws.Cells["B4"].Value.ToString();
                    string assessmentFor = ws.Cells["B2"].Value == null ? "" : ws.Cells["B2"].Value.ToString();
                    string companyCD = ws.Cells["D3"].Value == null ? "" : ws.Cells["D3"].Value.ToString();

                    var assessmentDatelist = assessmentDateStr.Split('-');
                    string UniqueID = assessmentDatelist[0].ToString();
                    model.Header.UniqueID = Convert.ToInt32(UniqueID);
                    Appointment app = GetAppointmentById(model.Header.UniqueID.Value);
                    model.Header.AssessmentDate = app.StartDate.Value;
                    string SM = ws.Cells["B3"].Value == null ? "" : ws.Cells["B3"].Value.ToString();
                    model.Header.SM = SM;

                    if (assessmentFor != string.Empty)
                    {
                        string abcdComment = ws.Cells["B17"].Value == null ? "" : ws.Cells["B17"].Value.ToString();
                        string abcdNextTrainingObjective = ws.Cells["E17"].Value == null ? "" : ws.Cells["E17"].Value.ToString();

                        model.Header.ABCDComment = abcdComment;
                        model.Header.ABCDNextTrainingObjective = abcdNextTrainingObjective;
                    }

                    string comment = ws.Cells["B52"].Value == null ? "" : ws.Cells["B52"].Value.ToString();
                    string nextTrainingObjective = ws.Cells["E52"].Value == null ? "" : ws.Cells["E52"].Value.ToString();
                    model.Header.Comment = comment;
                    model.Header.NextTrainingObjective = nextTrainingObjective;

                    string mark = ws.Cells["B7"].Value == null ? "" : ws.Cells["B7"].Value.ToString();

                    string training = ws.Cells["B8"].Value == null ? "" : ws.Cells["B8"].Value.ToString().Trim();
                    training = training.Length > 10 ? training.Substring(0, 10) : training;
                    string PC = ws.Cells["B9"].Value == null ? "" : ws.Cells["B9"].Value.ToString().Trim();
                    PC = PC.Length > 10 ? PC.Substring(0, 10) : PC;
                    string LPPC = ws.Cells["B10"].Value == null ? "" : ws.Cells["B10"].Value.ToString().Trim();
                    LPPC = LPPC.Length > 10 ? LPPC.Substring(0, 10) : LPPC;
                    model.Header.Mark = mark;
                    model.Header.Training = training;
                    model.Header.PC = PC;
                    model.Header.LPPC = LPPC;

                    string[] companyCDSplit = companyCD.Split('-');
                    if (companyCDSplit.Length > 0)
                    {
                        companyCD = companyCDSplit[0];
                    }

                    var query = (from company in Context.Distributors
                                 where company.CompanyCD.Trim() == companyCD.Trim()
                                 select company).FirstOrDefault();

                    string[] assessmentForSplit = assessmentFor.Split('-');
                    if (assessmentForSplit.Length > 0)
                    {
                        assessmentFor = assessmentForSplit[0];
                    }
                    model.Header.AssessmentFor = assessmentFor;
                    model.Header.DistributorID = query == null ? string.Empty : query.CompanyID.ToString();
                    model.Header.UserID = employeeID;

                    if (!string.IsNullOrEmpty(model.Header.DistributorID))
                    {
                        model.Header.AreaID = GetAreaByDistributor(Convert.ToInt32(model.Header.DistributorID));
                    }


                    string scoreString = string.Empty;
                    int score = 1;
                    string note = string.Empty;
                    string pros = string.Empty;
                    string cons = string.Empty;

                    //1	Understanding	Am hiểu	P

                    scoreString = ws.Cells["B13"].Value == null ? "" : ws.Cells["B13"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C13"].Value == null ? "" : ws.Cells["C13"].Value.ToString();
                    cons = ws.Cells["D13"].Value == null ? "" : ws.Cells["D13"].Value.ToString();
                    note = ws.Cells["E13"].Value == null ? "" : ws.Cells["E13"].Value.ToString();

                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).AssessmentDate = app.StartDate.Value;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).AssessmentFor = assessmentFor;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).UniqueID = app.UniqueID;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).SM = SM;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).UserID = employeeID;
                    if (assessmentFor != string.Empty)
                    {
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).CriteriaScore = score;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).Note = note;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).Pros = pros;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 1).Cons = cons;

                    }
                    //2	Discussing	Bàn bạc	P

                    scoreString = ws.Cells["B14"].Value == null ? "" : ws.Cells["B14"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C14"].Value == null ? "" : ws.Cells["C14"].Value.ToString();
                    cons = ws.Cells["D14"].Value == null ? "" : ws.Cells["D14"].Value.ToString();
                    note = ws.Cells["E14"].Value == null ? "" : ws.Cells["E14"].Value.ToString();

                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).AssessmentDate = app.StartDate.Value;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).AssessmentFor = assessmentFor;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).UniqueID = app.UniqueID;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).SM = SM;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).UserID = employeeID;
                    if (assessmentFor != string.Empty)
                    {
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).CriteriaScore = score;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).Note = note;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).Pros = pros;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 2).Cons = cons;
                    }
                    //3	Coaching	Chỉ bảo	P

                    scoreString = ws.Cells["B15"].Value == null ? "" : ws.Cells["B15"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C15"].Value == null ? "" : ws.Cells["C15"].Value.ToString();
                    cons = ws.Cells["D15"].Value == null ? "" : ws.Cells["D15"].Value.ToString();
                    note = ws.Cells["E15"].Value == null ? "" : ws.Cells["E15"].Value.ToString();

                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).AssessmentDate = app.StartDate.Value;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).AssessmentFor = assessmentFor;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).UniqueID = app.UniqueID;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).SM = SM;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).UserID = employeeID;
                    if (assessmentFor != string.Empty)
                    {
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).CriteriaScore = score;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).Note = note;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).Pros = pros;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 3).Cons = cons;
                    }
                    //4	Conclusion	Đúc kết	P

                    scoreString = ws.Cells["B16"].Value == null ? "" : ws.Cells["B16"].Value.ToString();
                    score = 1;

                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C16"].Value == null ? "" : ws.Cells["C16"].Value.ToString();
                    cons = ws.Cells["D16"].Value == null ? "" : ws.Cells["D16"].Value.ToString();
                    note = ws.Cells["E16"].Value == null ? "" : ws.Cells["E16"].Value.ToString();

                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).AssessmentDate = app.StartDate.Value;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).AssessmentFor = assessmentFor;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).UniqueID = app.UniqueID;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).SM = SM;
                    model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).UserID = employeeID;
                    if (assessmentFor != string.Empty)
                    {
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).CriteriaScore = score;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).Note = note;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).Pros = pros;
                        model.TrainingProcess.SingleOrDefault(x => x.CriteriaID == 4).Cons = cons;
                    }
                    // CHUAN BI DAU NGAY
                    scoreString = ws.Cells["B20"].Value == null ? "" : ws.Cells["B20"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C20"].Value == null ? "" : ws.Cells["C20"].Value.ToString();
                    cons = ws.Cells["D20"].Value == null ? "" : ws.Cells["D20"].Value.ToString();
                    note = ws.Cells["E20"].Value == null ? "" : ws.Cells["E20"].Value.ToString();

                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).AssessmentDate = app.StartDate.Value;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).AssessmentFor = assessmentFor;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).UniqueID = app.UniqueID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).SM = SM;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).UserID = employeeID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).CriteriaScore = score;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).Note = note;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).Pros = pros;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 24).Cons = cons;
                    // THAM VIENG CUA HANG
                    scoreString = ws.Cells["B23"].Value == null ? "" : ws.Cells["B23"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C23"].Value == null ? "" : ws.Cells["C23"].Value.ToString();
                    cons = ws.Cells["D23"].Value == null ? "" : ws.Cells["D23"].Value.ToString();
                    note = ws.Cells["E23"].Value == null ? "" : ws.Cells["E23"].Value.ToString();

                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).AssessmentDate = app.StartDate.Value;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).AssessmentFor = assessmentFor;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).UniqueID = app.UniqueID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).SM = SM;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).UserID = employeeID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).CriteriaScore = score;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).Note = note;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).Pros = pros;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 25).Cons = cons;
                    //KE THUC CUOI NGÀY
                    scoreString = ws.Cells["B27"].Value == null ? "" : ws.Cells["B27"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C27"].Value == null ? "" : ws.Cells["C27"].Value.ToString();
                    cons = ws.Cells["D27"].Value == null ? "" : ws.Cells["D27"].Value.ToString();
                    note = ws.Cells["E27"].Value == null ? "" : ws.Cells["E27"].Value.ToString();

                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).AssessmentDate = app.StartDate.Value;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).AssessmentFor = assessmentFor;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).UniqueID = app.UniqueID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).SM = SM;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).UserID = employeeID;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).CriteriaScore = score;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).Note = note;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).Pros = pros;
                    model.DailyWorks.SingleOrDefault(x => x.CriteriaID == 26).Cons = cons;
                    // PDA
                    scoreString = ws.Cells["B31"].Value == null ? "" : ws.Cells["B31"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C31"].Value == null ? "" : ws.Cells["C31"].Value.ToString();
                    cons = ws.Cells["D31"].Value == null ? "" : ws.Cells["D31"].Value.ToString();
                    note = ws.Cells["E31"].Value == null ? "" : ws.Cells["E31"].Value.ToString();

                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).AssessmentDate = app.StartDate.Value;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).AssessmentFor = assessmentFor;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).UniqueID = app.UniqueID;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).SM = SM;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).UserID = employeeID;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).CriteriaScore = score;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).Note = note;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).Pros = pros;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 27).Cons = cons;
                    //2. Tài liệu/dụng cụ chào hàng
                    scoreString = ws.Cells["B32"].Value == null ? "" : ws.Cells["B32"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C32"].Value == null ? "" : ws.Cells["C32"].Value.ToString();
                    cons = ws.Cells["D32"].Value == null ? "" : ws.Cells["D32"].Value.ToString();
                    note = ws.Cells["E32"].Value == null ? "" : ws.Cells["E32"].Value.ToString();

                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).AssessmentDate = app.StartDate.Value;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).AssessmentFor = assessmentFor;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).UniqueID = app.UniqueID;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).SM = SM;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).UserID = employeeID;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).CriteriaScore = score;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).Note = note;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).Pros = pros;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 28).Cons = cons;
                    //3. Tài liệu/dụng cụ trưng bày
                    scoreString = ws.Cells["B33"].Value == null ? "" : ws.Cells["B33"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C33"].Value == null ? "" : ws.Cells["C33"].Value.ToString();
                    cons = ws.Cells["D33"].Value == null ? "" : ws.Cells["D33"].Value.ToString();
                    note = ws.Cells["E33"].Value == null ? "" : ws.Cells["E33"].Value.ToString();

                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).AssessmentDate = app.StartDate.Value;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).AssessmentFor = assessmentFor;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).UniqueID = app.UniqueID;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).SM = SM;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).UserID = employeeID;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).CriteriaScore = score;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).Note = note;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).Pros = pros;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 29).Cons = cons;
                    //4. Cặp đựng hồ sơ
                    scoreString = ws.Cells["B34"].Value == null ? "" : ws.Cells["B34"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C34"].Value == null ? "" : ws.Cells["C34"].Value.ToString();
                    cons = ws.Cells["D34"].Value == null ? "" : ws.Cells["D34"].Value.ToString();
                    note = ws.Cells["E34"].Value == null ? "" : ws.Cells["E34"].Value.ToString();

                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).AssessmentDate = app.StartDate.Value;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).AssessmentFor = assessmentFor;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).UniqueID = app.UniqueID;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).SM = SM;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).UserID = employeeID;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).CriteriaScore = score;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).Note = note;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).Pros = pros;
                    model.ToolsSM.SingleOrDefault(x => x.CriteriaID == 30).Cons = cons;
                    //1. Xem lại kế hoạch
                    scoreString = ws.Cells["B36"].Value == null ? "" : ws.Cells["B36"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C36"].Value == null ? "" : ws.Cells["C36"].Value.ToString();
                    cons = ws.Cells["D36"].Value == null ? "" : ws.Cells["D36"].Value.ToString();
                    note = ws.Cells["E36"].Value == null ? "" : ws.Cells["E36"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 31).Cons = cons;
                    //2. Mở đầu cuộc bán hàng
                    scoreString = ws.Cells["B37"].Value == null ? "" : ws.Cells["B37"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C37"].Value == null ? "" : ws.Cells["C37"].Value.ToString();
                    cons = ws.Cells["D37"].Value == null ? "" : ws.Cells["D37"].Value.ToString();
                    note = ws.Cells["E37"].Value == null ? "" : ws.Cells["E37"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 32).Cons = cons;
                    //3. Xem xét cửa hàng
                    scoreString = ws.Cells["B38"].Value == null ? "" : ws.Cells["B38"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C38"].Value == null ? "" : ws.Cells["C38"].Value.ToString();
                    cons = ws.Cells["D38"].Value == null ? "" : ws.Cells["D38"].Value.ToString();
                    note = ws.Cells["E38"].Value == null ? "" : ws.Cells["E38"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 33).Cons = cons;
                    //4. Chào hàng (GTSP/XLPB)
                    scoreString = ws.Cells["B39"].Value == null ? "" : ws.Cells["B39"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C39"].Value == null ? "" : ws.Cells["C39"].Value.ToString();
                    cons = ws.Cells["D39"].Value == null ? "" : ws.Cells["D39"].Value.ToString();
                    note = ws.Cells["E39"].Value == null ? "" : ws.Cells["E39"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 34).Cons = cons;
                    //5. Kết thúc chào hàng
                    scoreString = ws.Cells["B40"].Value == null ? "" : ws.Cells["B40"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C40"].Value == null ? "" : ws.Cells["C40"].Value.ToString();
                    cons = ws.Cells["D40"].Value == null ? "" : ws.Cells["D40"].Value.ToString();
                    note = ws.Cells["E40"].Value == null ? "" : ws.Cells["E40"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 35).Cons = cons;
                    //6. Ghi chép số liệu
                    scoreString = ws.Cells["B41"].Value == null ? "" : ws.Cells["B41"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C41"].Value == null ? "" : ws.Cells["C41"].Value.ToString();
                    cons = ws.Cells["D41"].Value == null ? "" : ws.Cells["D41"].Value.ToString();
                    note = ws.Cells["E41"].Value == null ? "" : ws.Cells["E41"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 36).Cons = cons;
                    //7. Trưng bày hàng hóa
                    scoreString = ws.Cells["B42"].Value == null ? "" : ws.Cells["B42"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C42"].Value == null ? "" : ws.Cells["C42"].Value.ToString();
                    cons = ws.Cells["D42"].Value == null ? "" : ws.Cells["D42"].Value.ToString();
                    note = ws.Cells["E42"].Value == null ? "" : ws.Cells["E42"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 37).Cons = cons;
                    //8. Chụp ảnh
                    scoreString = ws.Cells["B43"].Value == null ? "" : ws.Cells["B43"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C43"].Value == null ? "" : ws.Cells["C43"].Value.ToString();
                    cons = ws.Cells["D43"].Value == null ? "" : ws.Cells["D43"].Value.ToString();
                    note = ws.Cells["E43"].Value == null ? "" : ws.Cells["E43"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 38).Cons = cons;
                    //9. Tổng kết và đánh giá
                    scoreString = ws.Cells["B44"].Value == null ? "" : ws.Cells["B44"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }

                    pros = ws.Cells["C44"].Value == null ? "" : ws.Cells["C44"].Value.ToString();
                    cons = ws.Cells["D44"].Value == null ? "" : ws.Cells["D44"].Value.ToString();
                    note = ws.Cells["E44"].Value == null ? "" : ws.Cells["E44"].Value.ToString();

                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 39).Cons = cons;
                    ////10. Những mục khác
                    scoreString = ws.Cells["B45"].Value == null ? "" : ws.Cells["B45"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = Convert.ToInt32(scoreString);
                    }
                    pros = ws.Cells["C45"].Value == null ? "" : ws.Cells["C45"].Value.ToString();
                    cons = ws.Cells["D45"].Value == null ? "" : ws.Cells["D45"].Value.ToString();
                    note = ws.Cells["E45"].Value == null ? "" : ws.Cells["E45"].Value.ToString();
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).AssessmentDate = app.StartDate.Value;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).AssessmentFor = assessmentFor;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).UniqueID = app.UniqueID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).SM = SM;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).UserID = employeeID;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).CriteriaScore = score;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).Note = note;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).Pros = pros;
                    model.Steps.SingleOrDefault(x => x.CriteriaID == 40).Cons = cons;
                    //20	Coaching plan	Kế hoạch huấn luyện	U
                    scoreString = ws.Cells["B48"].Value == null ? "" : ws.Cells["B48"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = scoreString.Trim() == "Có" ? 1 : 0;
                    }

                    pros = ws.Cells["C48"].Value == null ? "" : ws.Cells["C48"].Value.ToString();
                    cons = ws.Cells["D48"].Value == null ? "" : ws.Cells["D48"].Value.ToString();
                    note = ws.Cells["E48"].Value == null ? "" : ws.Cells["E48"].Value.ToString();

                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).AssessmentDate = app.StartDate.Value;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).AssessmentFor = assessmentFor;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).UniqueID = app.UniqueID;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).SM = SM;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).UserID = employeeID;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).CriteriaScore = score;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).Note = note;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).Pros = pros;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 20).Cons = cons;
                    //21	Sales Rep Assessment	Bảng đánh giá Sales Rep	U
                    scoreString = ws.Cells["B49"].Value == null ? "" : ws.Cells["B49"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = scoreString.Trim() == "Có" ? 1 : 0;
                    }

                    pros = ws.Cells["C49"].Value == null ? "" : ws.Cells["C49"].Value.ToString();
                    cons = ws.Cells["D49"].Value == null ? "" : ws.Cells["D49"].Value.ToString();
                    note = ws.Cells["E49"].Value == null ? "" : ws.Cells["E49"].Value.ToString();

                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).AssessmentDate = app.StartDate.Value;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).AssessmentFor = assessmentFor;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).UniqueID = app.UniqueID;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).SM = SM;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).UserID = employeeID;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).CriteriaScore = score;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).Note = note;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).Pros = pros;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 21).Cons = cons;
                    //22	Monthly coaching report	Báo cáo huấn luyện hàng tháng	U
                    scoreString = ws.Cells["B50"].Value == null ? "" : ws.Cells["B50"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = scoreString.Trim() == "Có" ? 1 : 0;
                    }

                    pros = ws.Cells["C50"].Value == null ? "" : ws.Cells["C50"].Value.ToString();
                    cons = ws.Cells["D50"].Value == null ? "" : ws.Cells["D50"].Value.ToString();
                    note = ws.Cells["E50"].Value == null ? "" : ws.Cells["E50"].Value.ToString();

                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).AssessmentDate = app.StartDate.Value;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).AssessmentFor = assessmentFor;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).UniqueID = app.UniqueID;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).SM = SM;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).UserID = employeeID;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).CriteriaScore = score;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).Note = note;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).Pros = pros;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 22).Cons = cons;
                    //23	Sales Rep's personal criteria	Bảng chỉ tiêu cá nhân của các Sales Rep	U
                    scoreString = ws.Cells["B51"].Value == null ? "" : ws.Cells["B51"].Value.ToString();
                    score = 1;
                    if (!string.IsNullOrEmpty(scoreString))
                    {
                        score = scoreString.Trim() == "Có" ? 1 : 0;
                    }

                    pros = ws.Cells["C51"].Value == null ? "" : ws.Cells["C51"].Value.ToString();
                    cons = ws.Cells["D51"].Value == null ? "" : ws.Cells["D51"].Value.ToString();
                    note = ws.Cells["E51"].Value == null ? "" : ws.Cells["E51"].Value.ToString();

                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).AssessmentDate = app.StartDate.Value;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).AssessmentFor = assessmentFor;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).UniqueID = app.UniqueID;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).SM = SM;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).UserID = employeeID;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).CriteriaScore = score;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).Note = note;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).Pros = pros;
                    model.UpdateAndArchive.SingleOrDefault(x => x.CriteriaID == 23).Cons = cons;
                    return model;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return null;
            }
        }

        public static bool HasApprovedAppointment(System.DateTime date, string userID, int UniqueID)
        {
            return (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UserLogin == userID && ap.ScheduleType == "D" && ap.UniqueID == UniqueID && ap.Label.GetValueOrDefault(0) == 3
                select ap).Any((Appointment x) => x.StartDate.Value.Day == date.Day && x.StartDate.Value.Month == date.Month && x.StartDate.Value.Year == date.Year && x.StartDate.Value.TimeOfDay == date.TimeOfDay);
        }
        public static bool HasApprovedAppointment(System.DateTime date, string userID)
        {
            return (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UserLogin == userID && ap.ScheduleType == "D" && ap.Label.GetValueOrDefault(0) == 3
                select ap).Any((Appointment x) => x.StartDate.Value.Day == date.Day && x.StartDate.Value.Month == date.Month && x.StartDate.Value.Year == date.Year);
        }
        public static bool HasAssessmentNoWW(System.DateTime assessmentDate, string userLogin, int UniqueID)
        {
            return (
                from noTraining in HammerDataProvider.Context.NoTrainingAssessments
                where noTraining.AssessmentDate.Day == assessmentDate.Day && noTraining.AssessmentDate.Month == assessmentDate.Month && noTraining.AssessmentDate.Year == assessmentDate.Year && noTraining.UserID.Trim() == userLogin.Trim() && noTraining.Released && noTraining.UniqueID == (int?)UniqueID 
         
                select noTraining).Any<NoTrainingAssessment>();
        }
        public static bool HasAssessmentWW(System.DateTime assessmentDate, string userLogin, int UniqueID)
        {
            return (
                from smAssessment in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                where smAssessment.AssessmentDate.Day == assessmentDate.Day && smAssessment.AssessmentDate.Month == assessmentDate.Month && smAssessment.AssessmentDate.Year == assessmentDate.Year && smAssessment.UserID.Trim() == userLogin.Trim() && smAssessment.Released && smAssessment.UniqueID == (int?)UniqueID 
                select smAssessment).Any<SMTrainingAssessmentHeader>() || (
                from ssAssessment in HammerDataProvider.Context.TrainingAssessmentHeaders
                where ssAssessment.AssessmentDate.Day == assessmentDate.Day && ssAssessment.AssessmentDate.Month == assessmentDate.Month && ssAssessment.AssessmentDate.Year == assessmentDate.Year && ssAssessment.UserID.Trim() == userLogin.Trim() && ssAssessment.Released && ssAssessment.UniqueID == (int?)UniqueID 
                select ssAssessment).Any<TrainingAssessmentHeader>();
        }
        public static bool HasAssessment(System.DateTime assessmentDate, string userLogin, int UniqueID)
        {
            return (
                from smAssessment in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                where smAssessment.AssessmentDate.Day == assessmentDate.Day && smAssessment.AssessmentDate.Month == assessmentDate.Month && smAssessment.AssessmentDate.Year == assessmentDate.Year && smAssessment.UserID.Trim() == userLogin.Trim() && smAssessment.Released && smAssessment.UniqueID == (int?)UniqueID 
                select smAssessment).Any<SMTrainingAssessmentHeader>() || (
                from ssAssessment in HammerDataProvider.Context.TrainingAssessmentHeaders
                where ssAssessment.AssessmentDate.Day == assessmentDate.Day && ssAssessment.AssessmentDate.Month == assessmentDate.Month && ssAssessment.AssessmentDate.Year == assessmentDate.Year && ssAssessment.UserID.Trim() == userLogin.Trim() && ssAssessment.Released && ssAssessment.UniqueID == (int?)UniqueID 
                select ssAssessment).Any<TrainingAssessmentHeader>() || (
                from noTraining in HammerDataProvider.Context.NoTrainingAssessments
                where noTraining.AssessmentDate.Day == assessmentDate.Day && noTraining.AssessmentDate.Month == assessmentDate.Month && noTraining.AssessmentDate.Year == assessmentDate.Year && noTraining.UserID.Trim() == userLogin.Trim() && noTraining.Released && noTraining.UniqueID == (int?)UniqueID 
                select noTraining).Any<NoTrainingAssessment>();
        }
        public static bool HasAssessment(System.DateTime assessmentDate, string userLogin)
        {
            return (
                from smAssessment in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                where smAssessment.AssessmentDate.Day == assessmentDate.Day
                && smAssessment.AssessmentDate.Month == assessmentDate.Month
                && smAssessment.AssessmentDate.Year == assessmentDate.Year
                && smAssessment.UserID.Trim() == userLogin.Trim()
                && smAssessment.Released
                select smAssessment).Any<SMTrainingAssessmentHeader>() || (
                from ssAssessment in HammerDataProvider.Context.TrainingAssessmentHeaders
                where ssAssessment.AssessmentDate.Day == assessmentDate.Day
                && ssAssessment.AssessmentDate.Month == assessmentDate.Month
                && ssAssessment.AssessmentDate.Year == assessmentDate.Year
                && ssAssessment.UserID.Trim() == userLogin.Trim()
                && ssAssessment.Released
                select ssAssessment).Any<TrainingAssessmentHeader>() || (
                from noTraining in HammerDataProvider.Context.NoTrainingAssessments
                where noTraining.AssessmentDate.Day == assessmentDate.Day
                && noTraining.AssessmentDate.Month == assessmentDate.Month
                && noTraining.AssessmentDate.Year == assessmentDate.Year
                && noTraining.UserID.Trim() == userLogin.Trim() && noTraining.Released
                select noTraining).Any<NoTrainingAssessment>();
        }
        public static NoTrainingAssessment GetNoTrainingAssessment(System.DateTime assessmentDate, string employeeID)
        {
            NoTrainingAssessment arg_226_0;
            if ((arg_226_0 = (
                from assessment in HammerDataProvider.Context.NoTrainingAssessments
                where assessment.UserID.Trim() == employeeID.Trim()
                select assessment).SingleOrDefault((NoTrainingAssessment x) => x.AssessmentDate.Day == assessmentDate.Day && x.AssessmentDate.Month == assessmentDate.Month && x.AssessmentDate.Year == assessmentDate.Year)) == null)
            {
                arg_226_0 = new NoTrainingAssessment
                {
                    AssessmentDate = assessmentDate.Date,
                    UserID = employeeID
                };
            }
            return arg_226_0;
        }
        public static NoTrainingAssessment GetNoTrainingAssessment(System.DateTime assessmentDate, string employeeID, string unique)
        {
            NoTrainingAssessment rs = new NoTrainingAssessment();
            rs =
                (
                  from assessment in HammerDataProvider.Context.NoTrainingAssessments
                  where assessment.UserID.Trim() == employeeID.Trim() && assessment.UniqueID.Value.ToString().Trim() == unique.Trim()
                  select assessment).FirstOrDefault();
            if (rs == null)
            {
                rs = new NoTrainingAssessment();
                rs.AssessmentDate = assessmentDate;
                rs.UniqueID = Convert.ToInt32(unique);
                rs.UserID = employeeID;
                rs.IsDelete = false;
            }            
            return rs;    
                      
        }
        public static System.Collections.Generic.List<string> GetListUniqueIDAllTask(string employeeID)
        {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<ComboDateAssessmentModel> list2 = new System.Collections.Generic.List<ComboDateAssessmentModel>();
            System.DateTime date = System.DateTime.Now.Date;
            System.DateTime dateTime = System.DateTime.Now.Date.AddDays(-1.0);
            System.Collections.Generic.List<Appointment> detailAppointmentTask = HammerDataProvider.GetDetailAppointmentTask(employeeID, date.Date);
            foreach (Appointment current in detailAppointmentTask)
            {
                if (!HammerDataProvider.HasAssessment(current.StartDate.Value, employeeID, current.UniqueID) && HammerDataProvider.HasApprovedAppointment(current.StartDate.Value, employeeID, current.UniqueID))
                {
                    list2.Add(new ComboDateAssessmentModel
                    {
                        StartTime = current.StartDate.Value,
                        EndTime = current.EndDate.Value,
                        UniqueID = current.UniqueID
                    });
                }
            }
            System.Collections.Generic.List<Appointment> detailAppointmentTask2 = HammerDataProvider.GetDetailAppointmentTask(employeeID, dateTime.Date);
            foreach (Appointment current2 in detailAppointmentTask2)
            {
                if (!HammerDataProvider.HasAssessment(current2.StartDate.Value, employeeID, current2.UniqueID) && HammerDataProvider.HasApprovedAppointment(current2.StartDate.Value, employeeID, current2.UniqueID))
                {
                    list2.Add(new ComboDateAssessmentModel
                    {
                        StartTime = current2.StartDate.Value,
                        EndTime = current2.EndDate.Value,
                        UniqueID = current2.UniqueID
                    });
                }
            }
            System.Collections.Generic.List<System.DateTime> list3 = (
                from setting in HammerDataProvider.Context.ScheduleSubmitSettings
                where setting.EmployeeID.Trim() == employeeID.Trim() && setting.Status == 0
                select setting.Date.Date).ToList<System.DateTime>();
            foreach (System.DateTime item in list3)
            {
                if (list2.Find(delegate(ComboDateAssessmentModel a)
                {
                    System.DateTime arg_1C_0 = a.StartTime.Date;
                    System.DateTime item2 = item;
                    return arg_1C_0 == item2.Date;
                }) == null)
                {
                    string arg_3B4_0 = employeeID;
                    System.DateTime item3 = item;
                    System.Collections.Generic.List<Appointment> detailAppointmentTask3 = HammerDataProvider.GetDetailAppointmentTask(arg_3B4_0, item3.Date);
                    foreach (Appointment current3 in detailAppointmentTask3)
                    {
                        if (!HammerDataProvider.HasAssessment(current3.StartDate.Value, employeeID, current3.UniqueID) && HammerDataProvider.HasApprovedAppointment(current3.StartDate.Value, employeeID, current3.UniqueID))
                        {
                            list2.Add(new ComboDateAssessmentModel
                            {
                                StartTime = current3.StartDate.Value,
                                EndTime = current3.EndDate.Value,
                                UniqueID = current3.UniqueID
                            });
                        }
                    }
                }
            }
            foreach (ComboDateAssessmentModel current4 in list2)
            {
                list.Add(current4.UniqueID.ToString());
            }
            return list;
        }
        public static System.Collections.Generic.List<ComboDateAssessmentModel> GetAssessmentDateAllTask(string employeeID, System.DateTime date, bool type)
        {
            System.Collections.Generic.List<ComboDateAssessmentModel> list = new System.Collections.Generic.List<ComboDateAssessmentModel>();
            System.Collections.Generic.List<Appointment> detailAppointmentViewAssessment = HammerDataProvider.GetDetailAppointmentViewAssessment(employeeID, date.Date, type);
            foreach (Appointment current in detailAppointmentViewAssessment)
            {
                bool flag = HammerDataProvider.HasAssessment(current.StartDate.Value, employeeID, current.UniqueID);
                if (flag)
                {
                    list.Add(new ComboDateAssessmentModel
                    {
                        StartTime = current.StartDate.Value,
                        EndTime = current.EndDate.Value,
                        UniqueID = current.UniqueID
                    });
                }
            }
            list = (
                from a in list
                orderby a.StartTime
                select a).ToList<ComboDateAssessmentModel>();
            return list;
        }
        public static System.Collections.Generic.List<ComboDateAssessmentModel> GetAssessmentDateAllTask(string employeeID)
        {
            System.Collections.Generic.List<ComboDateAssessmentModel> list = new System.Collections.Generic.List<ComboDateAssessmentModel>();
            System.DateTime date = System.DateTime.Now.Date;
            //System.DateTime dateTime = System.DateTime.Now.Date.AddDays(-1.0);
            System.Collections.Generic.List<Appointment> detailAppointmentTask = HammerDataProvider.GetDetailAppointmentTask(employeeID, date.Date);
            foreach (Appointment current in detailAppointmentTask)
            {
                if (!HammerDataProvider.HasAssessment(current.StartDate.Value, employeeID, current.UniqueID) && HammerDataProvider.HasApprovedAppointment(current.StartDate.Value, employeeID, current.UniqueID))
                {
                    list.Add(new ComboDateAssessmentModel
                    {
                        StartTime = current.StartDate.Value,
                        EndTime = current.EndDate.Value,
                        UniqueID = current.UniqueID
                    });
                }
            }
            // Get Date In List
            List<ListDateAssessmentAgain> listDateAsss = (
               from setting in HammerDataProvider.Context.ListDateAssessmentAgains
               where setting.Active == true
               select setting).ToList<ListDateAssessmentAgain>();
            foreach (var item in listDateAsss)
            {
                ComboDateAssessmentModel check = list.Find(f => f.StartTime.Date == item.AssessmentDate.Date);
                if(check == null)
                {
                     List<Appointment> detailAppointmentTask3 = HammerDataProvider.GetDetailAppointmentTask(employeeID, item.AssessmentDate);
                    foreach (Appointment current3 in detailAppointmentTask3)
                    {
                        if (!HammerDataProvider.HasAssessment(current3.StartDate.Value, employeeID, current3.UniqueID) && HammerDataProvider.HasApprovedAppointment(current3.StartDate.Value, employeeID, current3.UniqueID))
                        {
                            list.Add(new ComboDateAssessmentModel
                            {
                                StartTime = current3.StartDate.Value,
                                EndTime = current3.EndDate.Value,
                                UniqueID = current3.UniqueID
                            });
                        }
                    }
                }                
            }
            System.Collections.Generic.List<System.DateTime> list2 = (
                from setting in HammerDataProvider.Context.ScheduleSubmitSettings
                where setting.EmployeeID.Trim() == employeeID.Trim() && setting.Status == 0
                select setting.Date.Date).ToList<System.DateTime>();
            foreach (System.DateTime item in list2)
            {
                if (list.Find(delegate(ComboDateAssessmentModel a)
                {
                    System.DateTime arg_1C_0 = a.StartTime.Date;
                    System.DateTime item2 = item;
                    return arg_1C_0 == item2.Date;
                }) == null)
                {
                    string arg_3AC_0 = employeeID;
                    System.DateTime item3 = item;
                    System.Collections.Generic.List<Appointment> detailAppointmentTask3 = HammerDataProvider.GetDetailAppointmentTask(arg_3AC_0, item3.Date);
                    foreach (Appointment current3 in detailAppointmentTask3)
                    {
                        if (!HammerDataProvider.HasAssessment(current3.StartDate.Value, employeeID, current3.UniqueID) && HammerDataProvider.HasApprovedAppointment(current3.StartDate.Value, employeeID, current3.UniqueID))
                        {
                            list.Add(new ComboDateAssessmentModel
                            {
                                StartTime = current3.StartDate.Value,
                                EndTime = current3.EndDate.Value,
                                UniqueID = current3.UniqueID
                            });
                        }
                    }
                }
            }
            list = (
                from a in list
                orderby a.StartTime
                select a).ToList<ComboDateAssessmentModel>();
            return list;
        }
        public static System.Collections.Generic.List<System.DateTime> GetAssessmentDate(string employeeID)
        {
            System.Collections.Generic.List<System.DateTime> list = new System.Collections.Generic.List<System.DateTime>();
            System.DateTime date = System.DateTime.Now.Date;
            System.DateTime dateTime = System.DateTime.Now.Date.AddDays(-1.0);
            if (!HammerDataProvider.HasAssessment(date, employeeID) && HammerDataProvider.HasApprovedAppointment(date, employeeID))
            {
                list.Add(date);
            }
            if (!HammerDataProvider.HasAssessment(dateTime, employeeID) && HammerDataProvider.HasApprovedAppointment(dateTime, employeeID))
            {
                list.Add(dateTime);
            }
            System.Collections.Generic.List<System.DateTime> list2 = (
                from setting in HammerDataProvider.Context.ScheduleSubmitSettings
                where setting.EmployeeID.Trim() == employeeID.Trim() && setting.Status == 0
                select setting.Date.Date).ToList<System.DateTime>();
            foreach (System.DateTime current in list2)
            {
                if (!list.Contains(current) && !HammerDataProvider.HasAssessment(current, employeeID) && HammerDataProvider.HasApprovedAppointment(current, employeeID))
                {
                    list.Add(current);
                }
            }
            return list;
        }
        public static int GetSMDistributor(string salesmanID)
        {
            int result;
            try
            {
                result = (
                    from assignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                    join sp in HammerDataProvider.Context.Salespersons on new
                    {
                        CompanyID = assignment.CompanyID,
                        SalesPersonID = assignment.SalesPersonID.Value
                    } equals new
                    {
                        CompanyID = sp.CompanyID,
                        SalesPersonID = sp.SalespersonID
                    }
                    where sp.SalespersonCD == salesmanID
                    select sp.CompanyID).SingleOrDefault<int>();
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = 0;
            }
            return result;
        }
        public static int GetWWDistributor(Appointment appointment)
        {
            int result;
            try
            {
                if (!string.IsNullOrEmpty(appointment.RouteID))
                {
                    DMSDistributorRouteAssignment dMSDistributorRouteAssignment = (
                        from routeAssignmnet in HammerDataProvider.Context.DMSDistributorRouteAssignments
                        where routeAssignmnet.RouteCD.Trim() == appointment.RouteID.Trim()
                        select routeAssignmnet).SingleOrDefault<DMSDistributorRouteAssignment>();
                    if (dMSDistributorRouteAssignment != null)
                    {
                        result = dMSDistributorRouteAssignment.CompanyID;
                    }
                    else
                    {
                        result = 0;
                    }
                }
                else
                {
                    result = 0;
                }
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = 0;
            }
            return result;
        }
        public static string GetAreaByDistributor(int distributorID)
        {
            string result;
            try
            {
                result = (
                    from dist in HammerDataProvider.Context.Distributors
                    where dist.CompanyID == distributorID
                    select dist.AreaID).SingleOrDefault<string>();
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = "";
            }
            return result;
        }
        public static Appointment HasAppointment(System.DateTime date, string userID)
        {
            return (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UserLogin == userID && ap.ScheduleType == "D" && ap.AllDay == (bool?)true && ap.Label.GetValueOrDefault(0) == 3
                select ap).FirstOrDefault((Appointment x) => x.StartDate.Value.Day == date.Day && x.StartDate.Value.Month == date.Month && x.StartDate.Value.Year == date.Year);
        }
        public static void SaveSMAssessment(SMAssessmentModel model)
        {
            try
            {
                //Fix bug gan lai uniquer khi ko tim thay o Header                
                //model.Header.UniqueID = model.DailyWorks[0].UniqueID;
                //
                //Kiem tra xem co noi dung no huan luyen ko ?
                NoTrainingAssessment noTrainingAssessment = (
                    from x in HammerDataProvider.Context.NoTrainingAssessments
                    where
                    x.AssessmentDate.Day == model.Header.AssessmentDate.Day
                    && x.AssessmentDate.Month == model.Header.AssessmentDate.Month
                    && x.AssessmentDate.Year == model.Header.AssessmentDate.Year
                    && x.UniqueID == model.Header.UniqueID
                    && x.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                    && x.UserID.Trim() == model.Header.UserID.Trim()
                    select x).SingleOrDefault<NoTrainingAssessment>();
                if (noTrainingAssessment != null)
                {
                    Context.NoTrainingAssessments.DeleteOnSubmit(noTrainingAssessment);
                }
                if (HammerDataProvider.Context.SMTrainingAssessmentHeaders.ToList<SMTrainingAssessmentHeader>().Exists((SMTrainingAssessmentHeader x) => x.AssessmentDate.Date == model.Header.AssessmentDate.Date && x.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay && x.UniqueID == model.Header.UniqueID && x.UserID == model.Header.UserID))
                {
                    SMTrainingAssessmentHeader sMTrainingAssessmentHeader = (
                        from header in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                        where header.AssessmentDate.Date == model.Header.AssessmentDate.Date && header.UserID == model.Header.UserID && header.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay && header.UniqueID == model.Header.UniqueID
                        select header).SingleOrDefault<SMTrainingAssessmentHeader>();
                    sMTrainingAssessmentHeader.Comment = model.Header.Comment;
                    sMTrainingAssessmentHeader.AreaID = model.Header.AreaID;
                    sMTrainingAssessmentHeader.DistributorID = model.Header.DistributorID;
                    sMTrainingAssessmentHeader.NextTrainingObjective = model.Header.NextTrainingObjective;
                    sMTrainingAssessmentHeader.TraningObjective = model.Header.TraningObjective;
                    sMTrainingAssessmentHeader.SalesObjective = model.Header.SalesObjective;
                    sMTrainingAssessmentHeader.Released = model.Header.Released;
                    sMTrainingAssessmentHeader.AssessmentFor = model.Header.AssessmentFor;
                    sMTrainingAssessmentHeader.UniqueID = model.Header.UniqueID;
                    sMTrainingAssessmentHeader.IsDelete = false;
                    IQueryable<SMTrainingAssessmentsDetail> queryable =
                        from detail in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                        where detail.AssessmentDate.Date == model.Header.AssessmentDate.Date && detail.UserID == model.Header.UserID && detail.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay && detail.UniqueID == model.Header.UniqueID
                        select detail;
                    foreach (SMTrainingAssessmentsDetail current in queryable)
                    {
                        HammerDataProvider.Context.SMTrainingAssessmentsDetails.DeleteOnSubmit(current);
                    }
                    HammerDataProvider.Context.SubmitChanges();
                    foreach (SMTrainingAssessmentsDetail current2 in model.DailyWorks)
                    {
                        current2.IsDelete = false;
                        HammerDataProvider.Context.SMTrainingAssessmentsDetails.InsertOnSubmit(current2);
                    }
                    foreach (SMTrainingAssessmentsDetail current3 in model.Tools)
                    {
                        current3.IsDelete = false;
                        HammerDataProvider.Context.SMTrainingAssessmentsDetails.InsertOnSubmit(current3);
                    }
                    foreach (SMTrainingAssessmentsDetail current4 in model.Steps)
                    {
                        current4.IsDelete = false;
                        HammerDataProvider.Context.SMTrainingAssessmentsDetails.InsertOnSubmit(current4);
                    }
                    HammerDataProvider.Context.SubmitChanges();
                }
                else
                {
                    IQueryable<SMTrainingAssessmentsDetail> queryable2 =
                        from detail in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                        where detail.AssessmentDate.Date == model.Header.AssessmentDate.Date && detail.UserID == model.Header.UserID && detail.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay && detail.UniqueID == model.Header.UniqueID
                        select detail;
                    foreach (SMTrainingAssessmentsDetail current5 in queryable2)
                    {
                        HammerDataProvider.Context.SMTrainingAssessmentsDetails.DeleteOnSubmit(current5);
                    }
                    HammerDataProvider.Context.SubmitChanges();
                    foreach (SMTrainingAssessmentsDetail current6 in model.DailyWorks)
                    {
                        current6.IsDelete = false;
                        HammerDataProvider.Context.SMTrainingAssessmentsDetails.InsertOnSubmit(current6);
                    }
                    foreach (SMTrainingAssessmentsDetail current7 in model.Tools)
                    {
                        current7.IsDelete = false;
                        HammerDataProvider.Context.SMTrainingAssessmentsDetails.InsertOnSubmit(current7);
                    }
                    foreach (SMTrainingAssessmentsDetail current8 in model.Steps)
                    {
                        current8.IsDelete = false;
                        HammerDataProvider.Context.SMTrainingAssessmentsDetails.InsertOnSubmit(current8);
                    }
                    model.Header.IsDelete = false;
                    HammerDataProvider.Context.SMTrainingAssessmentHeaders.InsertOnSubmit(model.Header);
                    HammerDataProvider.Context.SubmitChanges();
                }
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        public static void SubmitSMAssessment(SMAssessmentModel model)
        {
            try
            {
                HammerDataProvider.SaveSMAssessment(model);
                SMTrainingAssessmentHeader sMTrainingAssessmentHeader = (
                    from header in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                    where header.AssessmentDate.Date == model.Header.AssessmentDate.Date && header.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay && header.UniqueID == model.Header.UniqueID && header.UserID == model.Header.UserID
                    select header).SingleOrDefault<SMTrainingAssessmentHeader>();
                sMTrainingAssessmentHeader.Released = true;
                System.Collections.Generic.List<SMTrainingAssessmentsDetail> list = (
                    from detail in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                    where detail.AssessmentDate.Date == model.Header.AssessmentDate.Date && detail.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay && detail.UniqueID == model.Header.UniqueID && detail.UserID == model.Header.UserID
                      && ((from critDw in HammerDataProvider.Context.SMCriterias
                           where critDw.Active == true
                           select critDw.CriteriaID).ToList()).Contains(detail.CriteriaID)
                    select detail).ToList<SMTrainingAssessmentsDetail>();
                decimal d = 0m;
                foreach (SMTrainingAssessmentsDetail current in list)
                {
                    d += current.CriteriaScore;
                }
                System.Collections.Generic.List<SMCriteria> list2 = (
                    from detail in HammerDataProvider.Context.SMCriterias
                    where detail.Active == true
                    select detail).ToList<SMCriteria>();
                decimal value = d / list2.Count;
                sMTrainingAssessmentHeader.TotalScore = new decimal?(value);
                HammerDataProvider.Context.SubmitChanges();
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        public static void SaveAssessment(AssessmentModel model)
        {
            try
            {
                //Kiem tra xem co noi dung no huan luyen ko ?
                NoTrainingAssessment noTrainingAssessment = (
                    from x in HammerDataProvider.Context.NoTrainingAssessments
                    where
                    x.AssessmentDate.Day == model.Header.AssessmentDate.Day
                    && x.AssessmentDate.Month == model.Header.AssessmentDate.Month
                    && x.AssessmentDate.Year == model.Header.AssessmentDate.Year
                    && x.UniqueID == model.Header.UniqueID
                    && x.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                    && x.UserID.Trim() == model.Header.UserID.Trim()
                    select x).SingleOrDefault<NoTrainingAssessment>();
                if (noTrainingAssessment != null)
                {
                    Context.NoTrainingAssessments.DeleteOnSubmit(noTrainingAssessment);
                }
                //go
                if (Context.TrainingAssessmentHeaders.ToList().Exists(
                    x => x.AssessmentDate.Date == model.Header.AssessmentDate.Date
                        && x.UniqueID == model.Header.UniqueID
                        && x.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                        && x.UserID == model.Header.UserID))
                {
                    var assessmentHeader = (from header in Context.TrainingAssessmentHeaders
                                            where header.AssessmentDate.Date == model.Header.AssessmentDate.Date
                                            && header.UniqueID == model.Header.UniqueID
                                            && header.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                                            && header.UserID == model.Header.UserID
                                            select header).SingleOrDefault();

                    if (model.Header.AssessmentFor == null)
                        model.Header.AssessmentFor = string.Empty;
                    assessmentHeader.AssessmentFor = model.Header.AssessmentFor;
                    assessmentHeader.AreaID = model.Header.AreaID;
                    assessmentHeader.DistributorID = model.Header.DistributorID;
                    assessmentHeader.LPPC = model.Header.LPPC;
                    assessmentHeader.PC = model.Header.PC;
                    assessmentHeader.SRUnderstood = model.Header.SRUnderstood;
                    assessmentHeader.Training = model.Header.Training;
                    assessmentHeader.PC = model.Header.PC;
                    assessmentHeader.OutletUnderstood = model.Header.OutletUnderstood;
                    assessmentHeader.SM = model.Header.SM;
                    assessmentHeader.UniqueID = model.Header.UniqueID;
                    assessmentHeader.Mark = model.Header.Mark;
                    assessmentHeader.ABCDComment = model.Header.ABCDComment;
                    assessmentHeader.ABCDNextTrainingObjective = model.Header.ABCDNextTrainingObjective;
                    assessmentHeader.Comment = model.Header.Comment;
                    assessmentHeader.NextTrainingObjective = model.Header.NextTrainingObjective;
                    assessmentHeader.IsDelete = false;
                    //Process detail
                    //Clear detail
                    var assessmentDetail = (from detail in Context.TrainingAssessmentsDetails
                                            where detail.AssessmentDate.Date == model.Header.AssessmentDate.Date
                                            && detail.UniqueID == model.Header.UniqueID
                                            && detail.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                                            && detail.UserID == model.Header.UserID
                                            select detail);
                    foreach (var item in assessmentDetail)
                    {
                        Context.TrainingAssessmentsDetails.DeleteOnSubmit(item);
                    }
                    Context.SubmitChanges();
                    //Insert new detail line
                    foreach (var item in model.TrainingProcess)
                    {
                        if (model.Header.AssessmentFor == string.Empty)
                        {
                            item.AssessmentFor = string.Empty;
                            item.Note = string.Empty;
                            item.CriteriaScore = 1;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }

                    foreach (var item in model.DailyWorks)
                    {
                        if (model.Header.AssessmentFor == string.Empty)
                        {
                            item.AssessmentFor = string.Empty;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }

                    foreach (var item in model.ToolsSM)
                    {
                        if (model.Header.AssessmentFor == string.Empty)
                        {
                            item.AssessmentFor = string.Empty;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }

                    foreach (var item in model.Steps)
                    {
                        if (model.Header.AssessmentFor == string.Empty)
                        {
                            item.AssessmentFor = string.Empty;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }
                    foreach (var item in model.UpdateAndArchive)
                    {
                        if (model.Header.AssessmentFor == string.Empty)
                        {
                            item.AssessmentFor = string.Empty;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }
                    Context.SubmitChanges();
                }
                else
                {
                    //Process detail
                    //Clear detail
                    var assessmentDetail = (from detail in Context.TrainingAssessmentsDetails
                                            where detail.AssessmentDate.Date == model.Header.AssessmentDate.Date
                                            && detail.UniqueID == model.Header.UniqueID
                                            && detail.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                                            && detail.UserID == model.Header.UserID
                                            select detail);
                    foreach (var item in assessmentDetail)
                    {
                        Context.TrainingAssessmentsDetails.DeleteOnSubmit(item);
                    }
                    Context.SubmitChanges();
                    //Insert new detail line
                    foreach (var item in model.TrainingProcess)
                    {
                        if (model.Header.AssessmentFor == null)
                        {
                            item.AssessmentFor = string.Empty;
                            item.Note = string.Empty;
                            item.CriteriaScore = 0;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }

                    foreach (var item in model.DailyWorks)
                    {
                        if (model.Header.AssessmentFor == null)
                        {
                            item.AssessmentFor = string.Empty;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }

                    foreach (var item in model.ToolsSM)
                    {
                        if (model.Header.AssessmentFor == null)
                        {
                            item.AssessmentFor = string.Empty;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }

                    foreach (var item in model.Steps)
                    {
                        if (model.Header.AssessmentFor == null)
                        {
                            item.AssessmentFor = string.Empty;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }
                    foreach (var item in model.UpdateAndArchive)
                    {
                        if (model.Header.AssessmentFor == null)
                        {
                            item.AssessmentFor = string.Empty;
                        }
                        item.SM = model.Header.SM;
                        item.UniqueID = model.Header.UniqueID;
                        item.IsDelete = false;
                        Context.TrainingAssessmentsDetails.InsertOnSubmit(item);
                    }
                    //Process Header
                    if (model.Header.AssessmentFor == null)
                        model.Header.AssessmentFor = string.Empty;
                    model.Header.IsDelete = false;
                    Context.TrainingAssessmentHeaders.InsertOnSubmit(model.Header);
                    Context.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        public static void SubmitAssessment(AssessmentModel model)
        {
            try
            {
                HammerDataProvider.SaveAssessment(model);
                TrainingAssessmentHeader trainingAssessmentHeader = (
                    from header in HammerDataProvider.Context.TrainingAssessmentHeaders
                    where header.AssessmentDate.Date == model.Header.AssessmentDate.Date && header.UserID == model.Header.UserID
                    && header.UniqueID == model.Header.UniqueID
                    && header.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                    select header).SingleOrDefault<TrainingAssessmentHeader>();
                trainingAssessmentHeader.Released = true;
                System.Collections.Generic.List<TrainingAssessmentsDetail> list = (
                    from detail in HammerDataProvider.Context.TrainingAssessmentsDetails
                    where detail.AssessmentDate.Date == model.Header.AssessmentDate.Date && detail.UserID == model.Header.UserID
                     && detail.UniqueID == model.Header.UniqueID
                    && detail.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                     && ((from critDw in HammerDataProvider.Context.Criterias
                          where critDw.Active == true && critDw.CriteriaType != 'U'
                          select critDw.CriteriaID).ToList()).Contains(detail.CriteriaID)
                    select detail).ToList<TrainingAssessmentsDetail>();
                decimal d = 0m;
                foreach (TrainingAssessmentsDetail current in list)
                {
                    d += current.CriteriaScore;
                }
                System.Collections.Generic.List<Criteria> list2 = (
                    from detail in HammerDataProvider.Context.Criterias
                    where (detail.CriteriaType != 'U') && detail.Active == true
                    select detail).ToList<Criteria>();
                decimal value = d / list2.Count;
                trainingAssessmentHeader.TotalScore = new decimal?(value);
                HammerDataProvider.Context.SubmitChanges();
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        public static void SaveNoTraining(NoAssessmentModel model)
        {
            try
            {
                // Kiem danh gia huan luyen 
                DMSSFHierarchy query = HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(model.Header.UserID);
                if (query.TerritoryType == 'D' && query.IsSalesForce == true)
                {
                    if (HammerDataProvider.Context.SMTrainingAssessmentHeaders.ToList<SMTrainingAssessmentHeader>().Exists((SMTrainingAssessmentHeader x)
                        => x.AssessmentDate.Date == model.Header.AssessmentDate.Date
                        && x.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                        && x.UniqueID == model.Header.UniqueID
                        && x.UserID == model.Header.UserID))
                    {
                        //xoa header
                        var assessmentHeader = (from header in Context.SMTrainingAssessmentHeaders
                                                where header.AssessmentDate.Date == model.Header.AssessmentDate.Date
                                                && header.UserID == model.Header.UserID
                                                && header.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                                                && header.UniqueID == model.Header.UniqueID
                                                select header).SingleOrDefault();
                        HammerDataProvider.Context.SMTrainingAssessmentHeaders.DeleteOnSubmit(assessmentHeader);
                        var assessmentDetail = (from detail in Context.SMTrainingAssessmentsDetails
                                                where detail.AssessmentDate.Date == model.Header.AssessmentDate.Date
                                                && detail.UserID == model.Header.UserID
                                                && detail.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                                                && detail.UniqueID == model.Header.UniqueID
                                                select detail);
                        foreach (var item in assessmentDetail)
                        {
                            Context.SMTrainingAssessmentsDetails.DeleteOnSubmit(item);
                        }
                        Context.SubmitChanges();
                    }
                }
                else
                {
                    if (HammerDataProvider.Context.TrainingAssessmentHeaders.ToList<TrainingAssessmentHeader>().Exists((TrainingAssessmentHeader x)
                        => x.AssessmentDate.Date == model.Header.AssessmentDate.Date
                        && x.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                        && x.UniqueID == model.Header.UniqueID
                        && x.UserID == model.Header.UserID))
                    {
                        //xoa header
                        var assessmentHeader = (from header in Context.TrainingAssessmentHeaders
                                                where header.AssessmentDate.Date == model.Header.AssessmentDate.Date
                                                && header.UserID == model.Header.UserID
                                                && header.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                                                && header.UniqueID == model.Header.UniqueID
                                                select header).SingleOrDefault();
                        HammerDataProvider.Context.TrainingAssessmentHeaders.DeleteOnSubmit(assessmentHeader);

                        var assessmentDetail = (from detail in Context.TrainingAssessmentsDetails
                                                where detail.AssessmentDate.Date == model.Header.AssessmentDate.Date
                                                && detail.UserID == model.Header.UserID
                                                && detail.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay
                                                && detail.UniqueID == model.Header.UniqueID
                                                select detail);
                        foreach (var item in assessmentDetail)
                        {
                            Context.TrainingAssessmentsDetails.DeleteOnSubmit(item);
                        }
                        Context.SubmitChanges();
                    }
                }
                //go
                NoTrainingAssessment noTrainingAssessment = (
                    from x in HammerDataProvider.Context.NoTrainingAssessments
                    where x.AssessmentDate.Day == model.Header.AssessmentDate.Day && x.AssessmentDate.Month == model.Header.AssessmentDate.Month && x.AssessmentDate.Year == model.Header.AssessmentDate.Year && x.UniqueID == model.Header.UniqueID && x.AssessmentDate.TimeOfDay == model.Header.AssessmentDate.TimeOfDay && x.UserID.Trim() == model.Header.UserID.Trim()
                    select x).SingleOrDefault<NoTrainingAssessment>();
                if (noTrainingAssessment != null)
                {
                    noTrainingAssessment.UniqueID = model.Header.UniqueID;
                    noTrainingAssessment.Results = model.Header.Results;
                    noTrainingAssessment.Works = model.Header.Works;
                    noTrainingAssessment.Released = model.Header.Released;
                    noTrainingAssessment.IsDelete = false;
                }
                else
                {
                    model.Header.IsDelete = false;
                    HammerDataProvider.Context.NoTrainingAssessments.InsertOnSubmit(model.Header);
                }
                HammerDataProvider.Context.SubmitChanges();
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                throw ex;
            }
        }
        public static System.Collections.Generic.List<Distributor> GetDistributors()
        {
            return HammerDataProvider.Context.Distributors.ToList<Distributor>();
        }
        public static List<Distributor> GetDistributorsNPP(int NPP)
        {
            List<Distributor> sm = (
                from sman in HammerDataProvider.Context.Distributors
                where sman.CompanyID == NPP
                select sman).ToList<Distributor>();
            if (sm != null)
            {
                return sm;
            }
            return null;
        }
        public static Distributor GetDistributorsCD(int NPP)
        {
            Distributor sm = (
                from sman in HammerDataProvider.Context.Distributors
                where sman.CompanyID == NPP
                select sman).FirstOrDefault<Distributor>();
            return sm;
        }
        public static DMSProvince GetProvinceName(string id)
        {
            DMSProvince sm = (
                from sman in HammerDataProvider.Context.DMSProvinces
                where sman.ProvinceID == id
                select sman).FirstOrDefault<DMSProvince>();
            return sm;
        }
        public static System.Collections.Generic.List<Distributor> GetDistributors(string areaID)
        {
            return (
                from dist in HammerDataProvider.Context.Distributors
                where dist.AreaID == areaID
                select dist).Distinct<Distributor>().ToList<Distributor>();
        }
        public static System.Collections.Generic.List<Area> GetAreasWithRegion(string RegionID)
        {
            List<Area> listItem = new List<Area>();
            listItem = (
               from area in HammerDataProvider.Context.Areas
               where area.RegionID.Trim() == RegionID.Trim()
               select area).Distinct<Area>().ToList<Area>();
            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Area() { AreaID = string.Empty, AreaName = string.Empty });
            }
            return listItem;
        }
        public static System.Collections.Generic.List<Region> GetRegion()
        {
            List<Region> listItem = new List<Region>();
            listItem = HammerDataProvider.Context.Regions.ToList<Region>();
            if (listItem.Count > 1)
            {
                listItem.Insert(0, new Region() { RegionID = string.Empty, RegionName = string.Empty });
            }
            return listItem;
        }
        public static System.Collections.Generic.List<Area> GetAreas()
        {
            return HammerDataProvider.Context.Areas.ToList<Area>();
        }
        public static System.Collections.Generic.List<Area> GetAreas(string AreaID)
        {
            return (
               from dist in HammerDataProvider.Context.Areas
               where dist.AreaID == AreaID
               select dist).Distinct<Area>().ToList<Area>();
        }
        public static System.Collections.Generic.List<Criteria> SSGetDailyWorkingCriteria()
        {
            return (
                from crit in HammerDataProvider.Context.Criterias
                where (int)crit.CriteriaType == 'D' && crit.Active == true
                select crit).ToList<Criteria>();
        }
        public static System.Collections.Generic.List<SMCriteria> GetDailyWorkingCriteria()
        {
            return (
                from crit in HammerDataProvider.Context.SMCriterias
                where (int)crit.CriteriaType == 68 && crit.Active == true
                select crit).ToList<SMCriteria>();
        }
        public static System.Collections.Generic.List<Criteria> SSGetToolsCriteria()
        {
            return (
                from crit in HammerDataProvider.Context.Criterias
                where (int)crit.CriteriaType == 'O' && crit.Active == true
                select crit).ToList<Criteria>();
        }
        public static System.Collections.Generic.List<SMCriteria> GetToolsCriteria()
        {
            return (
                from crit in HammerDataProvider.Context.SMCriterias
                where crit.CriteriaType == 'T' && crit.Active == true
                select crit).ToList<SMCriteria>();
        }
        public static System.Collections.Generic.List<Criteria> SSGetSaleStepCriteria()
        {
            return (
                from crit in HammerDataProvider.Context.Criterias
                where (int)crit.CriteriaType == 'S' && crit.Active == true
                select crit).ToList<Criteria>();
        }
        public static System.Collections.Generic.List<SMCriteria> GetSaleStepCriteria()
        {
            return (
                from crit in HammerDataProvider.Context.SMCriterias
                where crit.CriteriaType == 'S' && crit.Active == true
                select crit).ToList<SMCriteria>();
        }
        public static System.Collections.Generic.List<Criteria> GetTrainingQualityCriterias()
        {
            return (
                from crit in HammerDataProvider.Context.Criterias
                where (int)crit.CriteriaType == 81 && crit.Active == true
                select crit).ToList<Criteria>();
        }
        public static System.Collections.Generic.List<Criteria> GetTrainingProcessCriterias()
        {
            return (
                from crit in HammerDataProvider.Context.Criterias
                where (int)crit.CriteriaType == 80 && crit.Active == true
                select crit).ToList<Criteria>();
        }
        public static System.Collections.Generic.List<Criteria> GetUsingToolsCriterias()
        {
            return (
                from crit in HammerDataProvider.Context.Criterias
                where (int)crit.CriteriaType == 84 && crit.Active == true
                select crit).ToList<Criteria>();
        }
        public static System.Collections.Generic.List<Criteria> GetUpdateArchiveCriterias()
        {
            return (
                from crit in HammerDataProvider.Context.Criterias
                where (int)crit.CriteriaType == 85 && crit.Active == true
                select crit).ToList<Criteria>();
        }
        public static string GetSSCriteriaDescription(int criteriaID)
        {
            string result;
            try
            {
                Criteria sMCriteria = (
                    from crit in HammerDataProvider.Context.Criterias
                    where crit.CriteriaID == criteriaID
                    select crit).SingleOrDefault<Criteria>();
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "vi-VN")
                {
                    result = sMCriteria.DescriptionVN;
                }
                else
                {
                    result = sMCriteria.Description;
                }
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = string.Empty;
            }
            return result;
        }
        public static string GetSMCriteriaDescription(int criteriaID)
        {
            string result;
            try
            {
                SMCriteria sMCriteria = (
                    from crit in HammerDataProvider.Context.SMCriterias
                    where crit.CriteriaID == criteriaID
                    select crit).SingleOrDefault<SMCriteria>();
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "vi-VN")
                {
                    result = sMCriteria.DescriptionVN;
                }
                else
                {
                    result = sMCriteria.Description;
                }
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = string.Empty;
            }
            return result;
        }
        public static string GetTrainingCriteriaDescription(int id)
        {
            string result;
            try
            {
                Criteria criteria = (
                    from crit in HammerDataProvider.Context.Criterias
                    where crit.CriteriaID == id
                    select crit).SingleOrDefault<Criteria>();
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "vi-VN")
                {
                    result = criteria.DescriptionVN;
                }
                else
                {
                    result = criteria.Description;
                }
            }
            catch (System.Exception ex)
            {
                HammerDataProvider.Log.Error(ex.Message, ex);
                result = string.Empty;
            }
            return result;
        }
        public static NoAssessmentModel GetNoAssessment(System.DateTime assessmentDate, string userLogin)
        {
            NoTrainingAssessment noTrainingAssessment = (
                from x in HammerDataProvider.Context.NoTrainingAssessments
                where x.AssessmentDate.Day == assessmentDate.Day && x.AssessmentDate.Month == assessmentDate.Month && x.AssessmentDate.Year == assessmentDate.Year && x.UserID.Trim() == userLogin.Trim()
                select x).SingleOrDefault<NoTrainingAssessment>();
            if (noTrainingAssessment != null)
            {
                return new NoAssessmentModel
                {
                    Header = noTrainingAssessment,
                    AllowChange = false,
                    HasTraining = false
                };
            }
            return null;
        }
        public static NoAssessmentModel GetNoAssessment(System.DateTime assessmentDate, string userLogin, int UniqueID)
        {
            NoTrainingAssessment noTrainingAssessment = (
                from x in HammerDataProvider.Context.NoTrainingAssessments
                where x.AssessmentDate.Day == assessmentDate.Day && x.AssessmentDate.Month == assessmentDate.Month && x.AssessmentDate.Year == assessmentDate.Year && x.AssessmentDate.TimeOfDay == assessmentDate.TimeOfDay && x.UserID.Trim() == userLogin.Trim() && x.UniqueID == (int?)UniqueID
                select x).SingleOrDefault<NoTrainingAssessment>();
            if (noTrainingAssessment != null)
            {
                return new NoAssessmentModel
                {
                    Header = noTrainingAssessment,
                    AllowChange = false,
                    HasTraining = false
                };
            }
            return null;
        }
        public static UsersModel GetCurrentUser(string userName)
        {
            UsersModel result;
            using (UsersContext usersContext = new UsersContext())
            {
                SimpleRoleProvider roles = (SimpleRoleProvider)System.Web.Security.Roles.Provider;
                System.Collections.Generic.List<UserProfile> source = (
                    from x in Global.Context.UserProfiles
                    orderby x.UserId
                    select x).ToList();
                result = (
                    from user in source
                    where user.UserName.ToLower() == userName.ToLower()
                    select new UsersModel
                    {
                        UserName = user.UserName,
                        FullName = user.UserName,
                        Role = roles.GetRolesForUser(userName).FirstOrDefault<string>()
                    }).SingleOrDefault<UsersModel>();
            }
            return result;
        }
        public static System.Collections.Generic.List<UsersModel> GetUsers()
        {
            System.Collections.Generic.List<UsersModel> result;
            using (UsersContext usersContext = new UsersContext())
            {
                SimpleRoleProvider roles = (SimpleRoleProvider)System.Web.Security.Roles.Provider;
                System.Collections.Generic.List<UserProfile> source = (
                    from x in Global.Context.UserProfiles
                    orderby x.UserId
                    select x).ToList();
                result = (
                    from user in source
                    select new UsersModel
                    {
                        UserName = user.UserName,
                        FullName = user.UserName,
                        Role = roles.GetRolesForUser(user.UserName).FirstOrDefault<string>()
                    }).ToList<UsersModel>();
            }
            return result;
        }
        public static System.Collections.Generic.List<RolesModel> GetRoles()
        {
            SimpleRoleProvider simpleRoleProvider = (SimpleRoleProvider)System.Web.Security.Roles.Provider;
            string[] allRoles = simpleRoleProvider.GetAllRoles();
            System.Collections.Generic.List<RolesModel> list = new System.Collections.Generic.List<RolesModel>();
            for (int i = 0; i < allRoles.Length; i++)
            {
                RolesModel item = new RolesModel
                {
                    RoleId = i + 1,
                    RoleName = allRoles[i],
                    OldRoleName = allRoles[i]
                };
                list.Add(item);
            }
            return list;
        }
        public static int GetNewEditableNo()
        {
            System.Collections.Generic.IEnumerable<ScheduleErrorModel> editableError = HammerDataProvider.GetEditableError();
            if (editableError.Count<ScheduleErrorModel>() <= 0)
            {
                return 0;
            }
            return editableError.Last<ScheduleErrorModel>().No + 1;
        }
        public static ScheduleErrorModel GetEditableError(int No)
        {
            return (
                from error in HammerDataProvider.GetEditableError()
                where error.No == No
                select error).FirstOrDefault<ScheduleErrorModel>();
        }
        public static System.Collections.Generic.IList<ScheduleErrorModel> GetEditableError()
        {
            return (System.Collections.Generic.IList<ScheduleErrorModel>)System.Web.HttpContext.Current.Session["ExcelError"];
        }
        public static void InsertError(ScheduleErrorModel err)
        {
            ScheduleErrorModel scheduleErrorModel = new ScheduleErrorModel();
            scheduleErrorModel.No = HammerDataProvider.GetNewEditableNo();
            scheduleErrorModel.Year = err.Year;
            scheduleErrorModel.Month = err.Month;
            scheduleErrorModel.Day = err.Day;
            scheduleErrorModel.Title = err.Title;
            scheduleErrorModel.Content = err.Content;
            scheduleErrorModel.Outlet = err.Outlet;
            scheduleErrorModel.PhoneNumber = err.PhoneNumber;
            scheduleErrorModel.WWCode = err.WWCode;
            HammerDataProvider.GetEditableError().Add(scheduleErrorModel);
        }
        public static void DeleteError(int No)
        {
            ScheduleErrorModel editableError = HammerDataProvider.GetEditableError(No);
            if (editableError != null)
            {
                HammerDataProvider.GetEditableError().Remove(editableError);
            }
        }
        public static void UpdateError(ScheduleErrorModel error)
        {
            ScheduleErrorModel editableError = HammerDataProvider.GetEditableError(error.No);
            if (editableError != null)
            {
                editableError.Year = error.Year;
                editableError.Month = error.Month;
                editableError.Day = error.Day;
                editableError.Title = error.Title;
                editableError.Content = error.Content;
                editableError.Outlet = error.Outlet;
                editableError.PhoneNumber = error.PhoneNumber;
                editableError.WWCode = error.WWCode;
            }
        }
        public static System.Collections.Generic.List<EmployeeModel> GetEmployeesRegionAreaPhanCap(string RegionID, string AreaID, List<EmployeeModel> list)
        {
            if (String.IsNullOrEmpty(RegionID) && String.IsNullOrEmpty(AreaID))
            {
                return list;
            }
            else if (!String.IsNullOrEmpty(RegionID) && String.IsNullOrEmpty(AreaID))
            {

                List<EmployeeModel> list2 = (
                    from nv in list
                    join
                        sf in HammerDataProvider.Context.DMSSalesForces on nv.EmployeeID equals sf.EmployeeID
                    join assm in HammerDataProvider.Context.DMSSFAssignments
                    on sf.EmployeeID equals assm.EmployeeID
                    where assm.RegionID == RegionID
                   && sf.Active == true && assm.IsActive == true
                    select nv).Distinct<EmployeeModel>().ToList<EmployeeModel>();
                return list2;
            }
            else if (!String.IsNullOrEmpty(RegionID) && !String.IsNullOrEmpty(AreaID))
            {
                List<EmployeeModel> list2 = (
                    from nv in list
                    join
                        sf in HammerDataProvider.Context.DMSSalesForces on nv.EmployeeID equals sf.EmployeeID
                    join assm in HammerDataProvider.Context.DMSSFAssignments
                    on sf.EmployeeID equals assm.EmployeeID
                    where assm.RegionID == RegionID && assm.AreaID == AreaID
                   && sf.Active == true && assm.IsActive == true
                    select nv).Distinct<EmployeeModel>().ToList<EmployeeModel>();
                return list2;
            }
            else if (String.IsNullOrEmpty(RegionID) && !String.IsNullOrEmpty(AreaID))
            {
                List<EmployeeModel> list2 = (
                   from nv in list
                   join
                       sf in HammerDataProvider.Context.DMSSalesForces on nv.EmployeeID equals sf.EmployeeID
                   join assm in HammerDataProvider.Context.DMSSFAssignments
                   on sf.EmployeeID equals assm.EmployeeID
                   where assm.AreaID == AreaID
                  && sf.Active == true && assm.IsActive == true
                   select nv).Distinct<EmployeeModel>().ToList<EmployeeModel>();
                return list2;
            }
            else
            {
                return null;
            }

        }
        public static System.Collections.Generic.List<EmployeeModel> GetAddEmployees(string RegionID, string AreaID)
        {
            if (String.IsNullOrEmpty(RegionID) && String.IsNullOrEmpty(AreaID))
            {
                System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
                System.Collections.Generic.List<DMSSalesForce> list2 = (
                    from sf in HammerDataProvider.Context.DMSSalesForces
                    where sf.Active == true
                    select sf).Distinct<DMSSalesForce>().ToList<DMSSalesForce>();
                foreach (DMSSalesForce item in list2)
                {
                    DMSSFHierarchy dMSSFHierarchy = new DMSSFHierarchy();
                    dMSSFHierarchy = (
                        from sfh in HammerDataProvider.Context.DMSSFHierarchies
                        where sfh.LevelID == item.SFLevel
                        select sfh).SingleOrDefault<DMSSFHierarchy>();
                    if (dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true)
                    {
                        list.Add(new EmployeeModel
                        {
                            EmployeeID = item.EmployeeID,
                            EmployeeName = item.EmployeeName,
                            Level = dMSSFHierarchy.LevelName
                        });
                    }
                }
                return list;
            }
            else if (!String.IsNullOrEmpty(RegionID) && String.IsNullOrEmpty(AreaID))
            {
                List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
                List<DMSSalesForce> list2 = (
                    from sf in HammerDataProvider.Context.DMSSalesForces
                    join assm in HammerDataProvider.Context.DMSSFAssignments
                    on sf.EmployeeID equals assm.EmployeeID
                    where assm.RegionID == RegionID
                   && sf.Active == true && assm.IsActive == true
                    select sf).Distinct<DMSSalesForce>().ToList<DMSSalesForce>();
                foreach (DMSSalesForce item in list2)
                {
                    DMSSFHierarchy dMSSFHierarchy = new DMSSFHierarchy();
                    dMSSFHierarchy = (
                        from sfh in HammerDataProvider.Context.DMSSFHierarchies
                        where sfh.LevelID == item.SFLevel
                        select sfh).SingleOrDefault<DMSSFHierarchy>();
                    if (dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true)
                    {
                        list.Add(new EmployeeModel
                        {
                            EmployeeID = item.EmployeeID,
                            EmployeeName = item.EmployeeName,
                            Level = dMSSFHierarchy.LevelName
                        });
                    }
                }
                return list;
            }
            else if (!String.IsNullOrEmpty(RegionID) && !String.IsNullOrEmpty(AreaID))
            {
                List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
                List<DMSSalesForce> list2 = (
                    from sf in HammerDataProvider.Context.DMSSalesForces
                    join assm in HammerDataProvider.Context.DMSSFAssignments
                    on sf.EmployeeID equals assm.EmployeeID
                    where assm.RegionID == RegionID && assm.AreaID == AreaID
                      && sf.Active == true && assm.IsActive == true
                    select sf).Distinct<DMSSalesForce>().ToList<DMSSalesForce>();
                foreach (DMSSalesForce item in list2)
                {
                    DMSSFHierarchy dMSSFHierarchy = new DMSSFHierarchy();
                    dMSSFHierarchy = (
                        from sfh in HammerDataProvider.Context.DMSSFHierarchies
                        where sfh.LevelID == item.SFLevel
                        select sfh).SingleOrDefault<DMSSFHierarchy>();
                    if (dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true)
                    {
                        list.Add(new EmployeeModel
                        {
                            EmployeeID = item.EmployeeID,
                            EmployeeName = item.EmployeeName,
                            Level = dMSSFHierarchy.LevelName
                        });
                    }
                }
                return list;
            }
            else if (String.IsNullOrEmpty(RegionID) && !String.IsNullOrEmpty(AreaID))
            {
                List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
                List<DMSSalesForce> list2 = (
                    from sf in HammerDataProvider.Context.DMSSalesForces
                    join assm in HammerDataProvider.Context.DMSSFAssignments
                    on sf.EmployeeID equals assm.EmployeeID
                    where assm.AreaID == AreaID
                      && sf.Active == true && assm.IsActive == true
                    select sf).Distinct<DMSSalesForce>().ToList<DMSSalesForce>();
                foreach (DMSSalesForce item in list2)
                {
                    DMSSFHierarchy dMSSFHierarchy = new DMSSFHierarchy();
                    dMSSFHierarchy = (
                        from sfh in HammerDataProvider.Context.DMSSFHierarchies
                        where sfh.LevelID == item.SFLevel
                        select sfh).SingleOrDefault<DMSSFHierarchy>();
                    if (dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true)
                    {
                        list.Add(new EmployeeModel
                        {
                            EmployeeID = item.EmployeeID,
                            EmployeeName = item.EmployeeName,
                            Level = dMSSFHierarchy.LevelName
                        });
                    }
                }
                return list;
            }
            else
            {
                return null;
            }

        }
        //public static System.Collections.Generic.List<EmployeeModel> GetEmployees(string RegionID, string AreaID)
        //{
        //    if (!String.IsNullOrEmpty(RegionID) && !String.IsNullOrEmpty(AreaID))
        //    {
        //        System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
        //        System.Collections.Generic.List<DMSSalesForce> list2 = (
        //            from sf in HammerDataProvider.Context.DMSSalesForces                    
        //            where sf.Active == true                   
        //            select sf).Distinct<DMSSalesForce>().ToList<DMSSalesForce>();
        //        foreach (DMSSalesForce item in list2)
        //        {
        //            DMSSFHierarchy dMSSFHierarchy = new DMSSFHierarchy();
        //            dMSSFHierarchy = (
        //                from sfh in HammerDataProvider.Context.DMSSFHierarchies
        //                where sfh.LevelID == item.SFLevel
        //                select sfh).SingleOrDefault<DMSSFHierarchy>();
        //            if (dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true)
        //            {
        //                list.Add(new EmployeeModel
        //                {
        //                    EmployeeID = item.EmployeeID,
        //                    EmployeeName = item.EmployeeName,
        //                    Level = dMSSFHierarchy.LevelName
        //                });
        //            }
        //        }
        //        if (list.Count > 1)
        //        {
        //            list.Insert(0, new EmployeeModel() { EmployeeID = string.Empty, EmployeeName = string.Empty });
        //        }
        //        return list;
        //    }
        //    else if (!String.IsNullOrEmpty(RegionID) && String.IsNullOrEmpty(AreaID))
        //    {
        //        System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
        //        System.Collections.Generic.List<DMSSalesForce> list2 = (
        //            from sf in HammerDataProvider.Context.DMSSalesForces
        //            where sf.Active == true 
        //            select sf).Distinct<DMSSalesForce>().ToList<DMSSalesForce>();
        //        foreach (DMSSalesForce item in list2)
        //        {
        //            DMSSFHierarchy dMSSFHierarchy = new DMSSFHierarchy();
        //            dMSSFHierarchy = (
        //                from sfh in HammerDataProvider.Context.DMSSFHierarchies
        //                where sfh.LevelID == item.SFLevel
        //                select sfh).SingleOrDefault<DMSSFHierarchy>();
        //            if (dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true)
        //            {
        //                list.Add(new EmployeeModel
        //                {
        //                    EmployeeID = item.EmployeeID,
        //                    EmployeeName = item.EmployeeName,
        //                    Level = dMSSFHierarchy.LevelName
        //                });
        //            }
        //        }
        //        if (list.Count > 1)
        //        {
        //            list.Insert(0, new EmployeeModel() { EmployeeID = string.Empty, EmployeeName = string.Empty });
        //        }
        //        return list;
        //    }  
        //    else
        //    {
        //        return null;
        //    }

        //}
        public static System.Collections.Generic.List<EmployeeModel> GetEmployees()
        {
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            System.Collections.Generic.List<DMSSalesForce> list2 = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                select sf).Distinct<DMSSalesForce>().ToList<DMSSalesForce>();
            foreach (DMSSalesForce item in list2)
            {
                DMSSFHierarchy dMSSFHierarchy = new DMSSFHierarchy();
                dMSSFHierarchy = (
                    from sfh in HammerDataProvider.Context.DMSSFHierarchies
                    where sfh.LevelID == item.SFLevel
                    select sfh).SingleOrDefault<DMSSFHierarchy>();
                if (dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true)
                {
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = item.EmployeeID,
                        EmployeeName = item.EmployeeName,
                        Level = dMSSFHierarchy.LevelName
                    });
                }
            }
            return list;
        }
        public static System.Collections.Generic.List<EmployeeModel> GetEmployees(string NV)
        {
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            System.Collections.Generic.List<DMSSalesForce> list2 = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                where sf.EmployeeID == NV
                select sf).Distinct<DMSSalesForce>().ToList<DMSSalesForce>();
            foreach (DMSSalesForce item in list2)
            {
                DMSSFHierarchy dMSSFHierarchy = new DMSSFHierarchy();
                dMSSFHierarchy = (
                    from sfh in HammerDataProvider.Context.DMSSFHierarchies
                    where sfh.LevelID == item.SFLevel
                    select sfh).SingleOrDefault<DMSSFHierarchy>();
                if (dMSSFHierarchy != null && dMSSFHierarchy.IsSalesForce == true)
                {
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = item.EmployeeID,
                        EmployeeName = item.EmployeeName,
                        Level = dMSSFHierarchy.LevelName
                    });
                }
            }
            return list;
        }
        public static void UpdateScheduleSubmitSetting(ScheduleSubmitSetting appt)
        {
            if (appt == null)
            {
                return;
            }
            ScheduleSubmitSetting scheduleSubmitSetting = (
               from setting in HammerDataProvider.Context.ScheduleSubmitSettings
               where setting.ID == appt.ID
               select setting).SingleOrDefault<ScheduleSubmitSetting>();
            if (scheduleSubmitSetting == null)
            {
                return;
            }
            scheduleSubmitSetting.Status = 1;
            HammerDataProvider.Context.SubmitChanges();

        }
        public static void InsertScheduleSubmitSetting(ScheduleSubmitSetting appt)
        {
            if (appt == null)
            {
                return;
            }
            HammerDataContext hammerDataContext = new HammerDataContext();
            hammerDataContext.ScheduleSubmitSettings.InsertOnSubmit(appt);
            hammerDataContext.SubmitChanges();
        }
        public static int CheckAssessmentDate(System.DateTime date, string user)
        {
            HammerDataContext hammerDataContext = new HammerDataContext();
            HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == user.Trim().ToUpper());
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == user.Trim().ToUpper() && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy == null)
            {
                return 1;
            }
            if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true)
            {
                System.Collections.Generic.IEnumerable<SMTrainingAssessmentHeader> source =
                    from setting in hammerDataContext.SMTrainingAssessmentHeaders
                    where setting.UserID == user && setting.AssessmentDate == date
                    select setting;
                if (source.Count<SMTrainingAssessmentHeader>() > 0)
                {
                    return 1;
                }
                System.Collections.Generic.IEnumerable<NoTrainingAssessment> source2 =
                    from setting in hammerDataContext.NoTrainingAssessments
                    where setting.UserID == user && setting.AssessmentDate == date
                    select setting;
                if (source2.Count<NoTrainingAssessment>() > 0)
                {
                    return 1;
                }
                return 0;
            }
            else
            {
                System.Collections.Generic.IEnumerable<TrainingAssessmentHeader> source3 =
                    from setting in hammerDataContext.TrainingAssessmentHeaders
                    where setting.UserID == user && setting.AssessmentDate == date
                    select setting;
                if (source3.Count<TrainingAssessmentHeader>() > 0)
                {
                    return 1;
                }
                System.Collections.Generic.IEnumerable<NoTrainingAssessment> source4 =
                    from setting in hammerDataContext.NoTrainingAssessments
                    where setting.UserID == user && setting.AssessmentDate == date
                    select setting;
                if (source4.Count<NoTrainingAssessment>() > 0)
                {
                    return 1;
                }
                return 0;
            }
        }
        public static int CheckScheduleSubmitSetting(ScheduleSubmitSettingEx st, string Kind)
        {
            HammerDataContext hammerDataContext = new HammerDataContext();
            System.Collections.Generic.IEnumerable<ScheduleSubmitSetting> source =
                from setting in hammerDataContext.ScheduleSubmitSettings
                where setting.UserLogin == st.UserLogin && setting.Date.Date == st.Date.Date && setting.EmployeeID == st.EmployeeID && setting.Status == 0 && setting.Type == Kind
                select setting;
            if (source.Count<ScheduleSubmitSetting>() > 0)
            {
                return 1;
            }
            return 0;
        }
        public static List<ScheduleSubmitSetting> GetScheduleSubmitSetting(string Type, string Em, DateTime Start, DateTime End)
        {
            if (Type == "All")
            {
                List<ScheduleSubmitSetting> source =
                    (from setting in Context.ScheduleSubmitSettings
                     join sf in Context.DMSSalesForces on setting.EmployeeID equals sf.EmployeeID
                     where setting.EmployeeID.ToUpper() == Em.ToUpper() &&
                     setting.Date.Date >= Start.Date && setting.Date.Date <= End.Date
                     orderby setting.CreatedDate descending
                     select setting).ToList();
                return source.ToList();
            }
            else
            {
                List<ScheduleSubmitSetting> source =
                    (from setting in Context.ScheduleSubmitSettings
                     join sf in Context.DMSSalesForces on setting.EmployeeID equals sf.EmployeeID
                     where setting.Type == Type && setting.EmployeeID.ToUpper() == Em.ToUpper() &&
                     setting.Date.Date >= Start.Date && setting.Date.Date <= End.Date
                     orderby setting.CreatedDate descending
                     select setting).ToList();
                return source.ToList();
            }
        }
        public static List<ScheduleSubmitSetting> GetOpenForYear(string EmployeeID)
        {
            List<ScheduleSubmitSetting> source =
                (from setting in Context.ScheduleSubmitSettings
                 where setting.Status == 0 && setting.EmployeeID.ToUpper() == EmployeeID.ToUpper()
                 orderby setting.CreatedDate descending
                 select setting).ToList();
            return source;
        }
        public static System.Collections.IEnumerable GetScheduleSubmitSetting()
        {
            HammerDataContext hammerDataContext = new HammerDataContext();
            var source =
                from setting in hammerDataContext.ScheduleSubmitSettings
                join sf in hammerDataContext.DMSSalesForces on setting.EmployeeID equals sf.EmployeeID
                where setting.Type == "D"
                orderby setting.CreatedDate descending
                select new
                {
                    Date = setting.Date,
                    EmployeeID = setting.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Type = (setting.Type == "D") ? Utility.Phrase("PrepareSchedule.Messages.DetailSchedule") : Utility.Phrase("PrepareSchedule.Messages.Month"),
                    Note = setting.Note,
                    Status = setting.Status,
                    CloseTime = setting.CloseTime,
                    CreatedDate = setting.CreatedDate
                };
            return source.ToList();
        }
        public static int CheckDateInScheduleSubmitSetting(System.DateTime date, string userOnline, string Kind)
        {
            HammerDataContext hammerDataContext = new HammerDataContext();
            var source =
                from setting in hammerDataContext.ScheduleSubmitSettings
                join sf in hammerDataContext.DMSSalesForces on setting.EmployeeID equals sf.EmployeeID
                where setting.Date.Date == date.Date && sf.LoginID == userOnline && setting.Status == 0 && setting.Type == Kind
                select new
                {
                    setting.Date,
                    setting.EmployeeID,
                    sf.EmployeeName,
                    setting.Note,
                    setting.Status
                };
            if (source.Count() > 0)
            {
                return 0;
            }
            return 1;
        }
        public static void UpdateStatusSubmitSetting(System.DateTime date, string userOnline, string Kind)
        {
            if (userOnline == null)
            {
                return;
            }
            ScheduleSubmitSetting scheduleSubmitSetting = (
                from setting in HammerDataProvider.Context.ScheduleSubmitSettings
                join sf in HammerDataProvider.Context.DMSSalesForces on setting.EmployeeID equals sf.EmployeeID
                where setting.Date.Date == date.Date && sf.LoginID == userOnline && setting.Status == 0 && setting.Type == Kind
                select setting).SingleOrDefault<ScheduleSubmitSetting>();
            if (scheduleSubmitSetting == null)
            {
                return;
            }
            scheduleSubmitSetting.Status = 1;
            scheduleSubmitSetting.Date = date;
            HammerDataProvider.Context.SubmitChanges();
        }
        public static System.Collections.Generic.List<ScheduleErrorModel> ReadExcel(System.IO.Stream FileInput, string type)
        {
            System.Collections.Generic.List<ScheduleErrorModel> list = new System.Collections.Generic.List<ScheduleErrorModel>();
            using (ExcelPackage excelPackage = new ExcelPackage(FileInput))
            {
                ExcelWorkbook workbook = excelPackage.Workbook;
                if (workbook != null)
                {
                    for (int i = 1; i < 2; i++)
                    {
                        ExcelWorksheet excelWorksheet = workbook.Worksheets[i];
                        int num = 1;
                        for (int j = 2; j <= excelWorksheet.Dimension.End.Row; j++)
                        {
                            ScheduleErrorModel scheduleErrorModel = new ScheduleErrorModel();
                            if (excelWorksheet.Cells[j, 1].Value != null)
                            {
                                scheduleErrorModel.EmployeeID = excelWorksheet.Cells[j, 1].Value.ToString();
                            }
                            if (excelWorksheet.Cells[j, 4].Value != null
                                 && excelWorksheet.Cells[j, 3].Value != null
                                 && excelWorksheet.Cells[j, 2].Value != null)
                            {
                                try
                                {
                                    scheduleErrorModel.Year = Convert.ToInt32(excelWorksheet.Cells[j, 4].Value);
                                    scheduleErrorModel.Month = Convert.ToInt32(excelWorksheet.Cells[j, 3].Value);
                                    scheduleErrorModel.Day = Convert.ToInt32(excelWorksheet.Cells[j, 2].Value);
                                }
                                catch (System.Exception)
                                {
                                    scheduleErrorModel.Year = 0;
                                    scheduleErrorModel.Month = 0;
                                    scheduleErrorModel.Day = 0;
                                }
                            }
                            if (excelWorksheet.Cells[j, 5].Value != null
                                && excelWorksheet.Cells[j, 6].Value != null)
                            {
                                scheduleErrorModel.StartTime = excelWorksheet.Cells[j, 5].Value.ToString();
                                scheduleErrorModel.EndTime = excelWorksheet.Cells[j, 6].Value.ToString();
                            }
                            if (excelWorksheet.Cells[i, 7].Value != null)
                            {
                                scheduleErrorModel.Title = excelWorksheet.Cells[i, 7].Value.ToString();
                            }

                            if (excelWorksheet.Cells[j, 8].Value != null)
                            {
                                scheduleErrorModel.Content = excelWorksheet.Cells[j, 8].Value.ToString();
                            }
                            if (excelWorksheet.Cells[j, 9].Value != null)
                            {
                                scheduleErrorModel.Outlet = excelWorksheet.Cells[j, 9].Value.ToString();
                            }
                            if (excelWorksheet.Cells[j, 10].Value != null)
                            {
                                scheduleErrorModel.PhoneNumber = excelWorksheet.Cells[j, 10].Value.ToString();
                            }
                            if (excelWorksheet.Cells[j, 11].Value != null)
                            {
                                scheduleErrorModel.WWCode = excelWorksheet.Cells[j, 11].Value.ToString();
                            }
                            scheduleErrorModel.No = num + 1;
                            scheduleErrorModel.Status = 0;
                            if (scheduleErrorModel.EmployeeID != null || scheduleErrorModel.Day != 0 || scheduleErrorModel.Month != 0 || scheduleErrorModel.Year != 0 || scheduleErrorModel.Title != null || scheduleErrorModel.Content != null || scheduleErrorModel.Outlet != null || scheduleErrorModel.PhoneNumber != null || scheduleErrorModel.WWCode != null)
                            {
                                if ((!(excelWorksheet.Name.Trim() == "LichThang") || !(type == "month")) && (!(excelWorksheet.Name.Trim() == "LichChiTiet") || !(type == "detail")))
                                {
                                    scheduleErrorModel.Status = 2;
                                    scheduleErrorModel.Note = Utility.Phrase("SendCalendar.CheckTab");
                                }
                                scheduleErrorModel.No = num;
                                list.Add(scheduleErrorModel);
                                num++;
                            }
                        }
                    }
                }
                excelPackage.Dispose();
            }
            return list;
        }
        public static int CheckNullExcel(ScheduleErrorModel data, string type)
        {
            if (data.EmployeeID == null || data.EmployeeID.Trim() == string.Empty || data.Day == 0 || data.Month == 0 || data.Year == 0 || data.StartTime == null || data.EndTime == null || data.Title == null || data.Content == null || data.StartTime.Trim() == string.Empty || data.EndTime.Trim() == string.Empty || data.Title.Trim() == string.Empty || data.Content.Trim() == string.Empty)
            {
                return 1;
            }
            int result;
            try
            {
                System.DateTime dateTime = System.Convert.ToDateTime(string.Concat(new object[]
				{
					data.Year.ToString(),
					'/',
					data.Month.ToString(),
					'/',
					data.Day.ToString(),
					' ',
					data.StartTime.ToString()
				}));
                System.Convert.ToDateTime(string.Concat(new object[]
				{
					data.Year.ToString(),
					'/',
					data.Month.ToString(),
					'/',
					data.Day.ToString(),
					' ',
					data.EndTime.ToString()
				}));
                bool flag = HammerDataProvider.IsValidTime(data.StartTime);
                bool flag2 = HammerDataProvider.IsValidTime(data.EndTime);
                if (flag && flag2)
                {
                    System.DateTime dateTime2 = new System.DateTime(2079, 6, 5);
                    int num = System.DateTime.Compare(dateTime.Date, dateTime2.Date);
                    if (num >= 1)
                    {
                        result = 1;
                    }
                    else
                    {
                        result = 0;
                    }
                }
                else
                {
                    result = 1;
                }
            }
            catch (System.Exception)
            {
                result = 1;
            }
            return result;
        }
        public static int CheckexsitAppointment(string userOnline, System.DateTime Start, System.DateTime End, string Kind)
        {
            Appointment appointment = new Appointment();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == userOnline && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.Parent.HasValue)
            {
                appointment = (
                    from carSchedule in HammerDataProvider.Context.Appointments
                    where carSchedule.UserLogin == userOnline && carSchedule.StartDate.Value.Date == Start.Date && carSchedule.EndDate.Value.Date == End.Date && carSchedule.ScheduleType == Kind && carSchedule.Label == (int?)3
                    select carSchedule).FirstOrDefault<Appointment>();
            }
            else
            {
                if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
                {
                    appointment = (
                        from carSchedule in HammerDataProvider.Context.Appointments
                        where carSchedule.UserLogin == userOnline && carSchedule.StartDate.Value.Date == Start.Date && carSchedule.EndDate.Value.Date == End.Date && carSchedule.ScheduleType == Kind && carSchedule.Label == (int?)3
                        select carSchedule).FirstOrDefault<Appointment>();
                }
                else
                {
                    appointment = (
                        from carSchedule in HammerDataProvider.Context.Appointments
                        where carSchedule.UserLogin == userOnline && carSchedule.StartDate.Value.Date == Start.Date && carSchedule.EndDate.Value.Date == End.Date && carSchedule.ScheduleType == Kind && carSchedule.Label != (int?)3
                        select carSchedule).FirstOrDefault<Appointment>();
                }
            }
            if (appointment != null)
            {
                return 1;
            }
            return 0;
        }
        public static int CheckexsitAppointmentNew(string userOnline, System.DateTime Start, System.DateTime End, string Kind)
        {
            Appointment appointment = (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UserLogin == userOnline && carSchedule.StartDate.Value.Date == Start.Date && carSchedule.EndDate.Value.Date == End.Date && carSchedule.ScheduleType == Kind && carSchedule.Label == (int?)0
                select carSchedule).FirstOrDefault<Appointment>();
            if (appointment != null)
            {
                return 1;
            }
            return 0;
        }
        public static int CheckexsitAppointmentDetail(string userOnline, System.DateTime Start, System.DateTime End, bool AllDay)
        {
            Appointment appointment = (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UserLogin == userOnline && carSchedule.StartDate.Value == Start && carSchedule.EndDate.Value == End && carSchedule.AllDay == (bool?)AllDay && (carSchedule.Label == (int?)2 || carSchedule.Label == (int?)4)
                select carSchedule).FirstOrDefault<Appointment>();
            if (appointment != null)
            {
                return 1;
            }
            return 0;
        }
        public static int CheckexsitAppointmentNewDetail(string userOnline, System.DateTime Start, System.DateTime End, bool AllDay)
        {
            Appointment appointment = (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UserLogin == userOnline && carSchedule.StartDate.Value == Start && carSchedule.EndDate.Value == End && carSchedule.AllDay == (bool?)AllDay && carSchedule.Label == (int?)0
                select carSchedule).FirstOrDefault<Appointment>();
            if (appointment != null)
            {
                return 1;
            }
            return 0;
        }
        public static int CheckErrorEmployee(string userOnline, string ID)
        {
            System.Collections.Generic.List<EmployeeModel> subordinateDown = HammerDataProvider.GetSubordinateDown(userOnline);
            bool flag = subordinateDown.Exists((EmployeeModel elet) => elet.EmployeeID == ID);
            if (flag)
            {
                return 0;
            }
            return 1;
        }
        public static System.Collections.Generic.List<ScheduleErrorModel> CheckErrorTempleteMonthPass(System.Collections.Generic.List<ScheduleErrorModel> input)
        {
            bool flag = false;
            System.Collections.Generic.List<ScheduleErrorModel> list = new System.Collections.Generic.List<ScheduleErrorModel>();
            System.Collections.Generic.List<ScheduleErrorModel> list2 = new System.Collections.Generic.List<ScheduleErrorModel>();
            ScheduleErrorModel scheduleErrorModel = new ScheduleErrorModel();
            input = (
                from x in input
                orderby x.Day
                select x).ThenBy((ScheduleErrorModel y) => y.Month).ThenBy((ScheduleErrorModel z) => z.Year).ToList<ScheduleErrorModel>();
            foreach (ScheduleErrorModel current in input)
            {
                if (current.Day == scheduleErrorModel.Day && current.Month == scheduleErrorModel.Month && current.Year == scheduleErrorModel.Year)
                {
                    current.Status = 1;
                    flag = true;
                    current.Note = Utility.Phrase("SendCalendar.DateSame");
                }
                else
                {
                    scheduleErrorModel = current;
                }
                list2.Add(current);
            }
            if (flag)
            {
                list = list2;
            }
            list = (
                from x in list
                orderby x.No
                select x).ToList<ScheduleErrorModel>();
            return list;
        }
        public static System.Collections.Generic.List<ScheduleErrorModel> CheckErrorTempleteMonth(System.Collections.Generic.List<ScheduleErrorModel> input)
        {
            bool flag = false;
            System.Collections.Generic.List<ScheduleErrorModel> source = new System.Collections.Generic.List<ScheduleErrorModel>();
            System.Collections.Generic.List<ScheduleErrorModel> list = new System.Collections.Generic.List<ScheduleErrorModel>();
            ScheduleErrorModel scheduleErrorModel = new ScheduleErrorModel();
            input = (
                from x in input
                orderby x.Day
                select x).ThenBy((ScheduleErrorModel y) => y.Month).ThenBy((ScheduleErrorModel z) => z.Year).ToList<ScheduleErrorModel>();
            foreach (ScheduleErrorModel current in input)
            {
                if (current.Day == scheduleErrorModel.Day && current.Month == scheduleErrorModel.Month && current.Year == scheduleErrorModel.Year)
                {
                    current.Status = 1;
                    flag = true;
                    current.Note = Utility.Phrase("SendCalendar.DateSame");
                }
                else
                {
                    scheduleErrorModel = current;
                }
                list.Add(current);
            }
            if (flag)
            {
                source = list;
            }
            else
            {
                source = input;
            }
            return (
                from x in source
                orderby x.No
                select x).ToList<ScheduleErrorModel>();
        }
        public static System.Collections.Generic.List<ScheduleErrorModel> SetTypeDetailRecordTemplete(System.Collections.Generic.List<ScheduleErrorModel> input)
        {
            System.Collections.Generic.List<ScheduleErrorModel> list = new System.Collections.Generic.List<ScheduleErrorModel>();
            ScheduleErrorModel scheduleErrorModel = new ScheduleErrorModel();
            foreach (ScheduleErrorModel current in input)
            {
                if (current.Day != scheduleErrorModel.Day || current.Month != scheduleErrorModel.Month || current.Year != scheduleErrorModel.Year)
                {
                    scheduleErrorModel = current;
                    scheduleErrorModel.Status = 9;
                }
                list.Add(current);
            }
            return list;
        }
        public static System.Collections.Generic.List<ScheduleErrorModel> CheckErrorTempletePass(System.Collections.Generic.List<ScheduleErrorModel> input, string type)
        {
            bool flag = false;
            System.Collections.Generic.List<ScheduleErrorModel> ListOneDate = new System.Collections.Generic.List<ScheduleErrorModel>();
            System.Collections.Generic.List<ScheduleErrorModel> result = new System.Collections.Generic.List<ScheduleErrorModel>();
            System.Collections.Generic.List<ScheduleErrorModel> list4 = new System.Collections.Generic.List<ScheduleErrorModel>();
            new ScheduleErrorModel();
            var list2 = (
                from list in input
                select new
                {
                    NV = list.EmployeeID
                }).Distinct().ToList();
            foreach (var itemNV in list2)
            {
                var list3 = (
                    from list in input
                    where list.EmployeeID == itemNV.NV
                    select new
                    {
                        NV = list.EmployeeID,
                        Year = list.Year,
                        Month = list.Month,
                        Day = list.Day
                    }).Distinct().ToList();
                foreach (var item in list3)
                {
                    new ScheduleErrorModel();
                    ScheduleErrorModel scheduleErrorModel = new ScheduleErrorModel();
                    input.FirstOrDefault((ScheduleErrorModel a) => a.EmployeeID == item.NV && a.Day == item.Day && a.Month == item.Month && a.Year == item.Year);
                    scheduleErrorModel = input.LastOrDefault((ScheduleErrorModel a) => a.EmployeeID == item.NV && a.Day == item.Day && a.Month == item.Month && a.Year == item.Year);
                    ListOneDate = (
                        from a in input
                        where a.EmployeeID == item.NV && a.Day == item.Day && a.Month == item.Month && a.Year == item.Year
                        select a).ToList<ScheduleErrorModel>();
                    if (ListOneDate.Count > 1)
                    {
                        System.DateTime dateTime = System.Convert.ToDateTime(ListOneDate[0].StartTime);
                        System.DateTime dateTime2 = System.Convert.ToDateTime(ListOneDate[1].StartTime);
                        System.TimeSpan.Compare(dateTime.TimeOfDay, dateTime2.TimeOfDay);
                        dateTime2 = System.Convert.ToDateTime(ListOneDate[0].EndTime);
                        System.DateTime dateTime3 = System.Convert.ToDateTime(scheduleErrorModel.EndTime);
                        System.TimeSpan.Compare(dateTime.TimeOfDay, dateTime3.TimeOfDay);
                        int i;
                        for (i = 1; i < ListOneDate.Count - 1; i++)
                        {
                            System.DateTime dateTime4 = System.Convert.ToDateTime(ListOneDate[i].EndTime);
                            System.DateTime dateTime5 = System.Convert.ToDateTime(ListOneDate[i + 1].StartTime);
                            int num = System.TimeSpan.Compare(dateTime4.TimeOfDay, dateTime5.TimeOfDay);
                            if (num != 0)
                            {
                                flag = true;
                                (
                                    from t in ListOneDate
                                    where t.No == ListOneDate[i].No
                                    select t).ToList<ScheduleErrorModel>().ForEach(delegate(ScheduleErrorModel p)
                                {
                                    p.Status = 1;
                                    p.Note = Utility.Phrase("SendCalendar.EndTimeNotStartTime");
                                });
                            }
                        }
                    }
                    foreach (ScheduleErrorModel current in ListOneDate)
                    {
                        list4.Add(current);
                    }
                }
            }
            if (flag)
            {
                result = list4;
            }
            return result;
        }
        public static System.Collections.Generic.List<ScheduleErrorModel> CheckErrorTemplete(System.Collections.Generic.List<ScheduleErrorModel> input, string type)
        {
            bool flag = false;
            System.Collections.Generic.List<ScheduleErrorModel> ListOneDate = new System.Collections.Generic.List<ScheduleErrorModel>();
            System.Collections.Generic.List<ScheduleErrorModel> result = new System.Collections.Generic.List<ScheduleErrorModel>();
            System.Collections.Generic.List<ScheduleErrorModel> list4 = new System.Collections.Generic.List<ScheduleErrorModel>();
            new ScheduleErrorModel();
            var list2 = (
                from list in input
                select new
                {
                    NV = list.EmployeeID
                }).Distinct().ToList();
            foreach (var itemNV in list2)
            {
                var list3 = (
                    from list in input
                    where list.EmployeeID == itemNV.NV
                    select new
                    {
                        NV = list.EmployeeID,
                        Year = list.Year,
                        Month = list.Month,
                        Day = list.Day
                    }).Distinct().ToList();
                foreach (var item in list3)
                {
                    new ScheduleErrorModel();
                    ScheduleErrorModel scheduleErrorModel = new ScheduleErrorModel();
                    input.FirstOrDefault((ScheduleErrorModel a) => a.EmployeeID == item.NV && a.Day == item.Day && a.Month == item.Month && a.Year == item.Year);
                    scheduleErrorModel = input.LastOrDefault((ScheduleErrorModel a) => a.EmployeeID == item.NV && a.Day == item.Day && a.Month == item.Month && a.Year == item.Year);
                    ListOneDate = (
                        from a in input
                        where a.EmployeeID == item.NV && a.Day == item.Day && a.Month == item.Month && a.Year == item.Year
                        select a).ToList<ScheduleErrorModel>();
                    if (ListOneDate.Count > 1)
                    {
                        System.DateTime dateTime = System.Convert.ToDateTime(ListOneDate[0].EndTime);
                        System.DateTime dateTime2 = System.Convert.ToDateTime(ListOneDate[1].StartTime);
                        System.TimeSpan.Compare(dateTime.TimeOfDay, dateTime2.TimeOfDay);
                        dateTime2 = System.Convert.ToDateTime(ListOneDate[0].EndTime);
                        System.DateTime dateTime3 = System.Convert.ToDateTime(scheduleErrorModel.EndTime);
                        System.TimeSpan.Compare(dateTime2.TimeOfDay, dateTime3.TimeOfDay);
                        int i;
                        for (i = 1; i < ListOneDate.Count - 1; i++)
                        {
                            System.DateTime dateTime4 = System.Convert.ToDateTime(ListOneDate[i].EndTime);
                            System.DateTime dateTime5 = System.Convert.ToDateTime(ListOneDate[i + 1].StartTime);
                            int num = System.TimeSpan.Compare(dateTime4.TimeOfDay, dateTime5.TimeOfDay);
                            if (num != 0)
                            {
                                flag = true;
                                (
                                    from t in ListOneDate
                                    where t.No == ListOneDate[i].No
                                    select t).ToList<ScheduleErrorModel>().ForEach(delegate(ScheduleErrorModel p)
                                {
                                    p.Note = Utility.Phrase("SendCalendar.EndTimeNotStartTime");
                                    p.Status = 1;
                                });
                            }
                        }
                    }
                    foreach (ScheduleErrorModel current in ListOneDate)
                    {
                        list4.Add(current);
                    }
                }
            }
            if (flag)
            {
                result = list4;
            }
            else
            {
                result = input;
            }
            return result;
        }
        public static bool IsValidTime(string thetime)
        {
            Regex regex = new Regex("^(?:[0-2])?[0-9]:[0-5][0-9]$");
            return regex.IsMatch(thetime);
        }
        public static string GetNameEmployee(string ID)
        {
            DMSSalesForce dMSSalesForce = (
                from carSchedule in HammerDataProvider.Context.DMSSalesForces
                where carSchedule.EmployeeID == ID
                select carSchedule).FirstOrDefault<DMSSalesForce>();
            if (dMSSalesForce != null)
            {
                return dMSSalesForce.EmployeeName;
            }
            return null;
        }
        public static DMSSFAssignment GetAssSFIsBase(string ID)
        {
            DMSSFAssignment dMSSalesForce = (
                from carSchedule in HammerDataProvider.Context.DMSSFAssignments
                where carSchedule.EmployeeID == ID && carSchedule.IsActive == true && 
                carSchedule.IsBaseAssignment == true
                select carSchedule).FirstOrDefault<DMSSFAssignment>();           
            return dMSSalesForce;
        }
        public static DMSSFAssignment GetAssSFIsNotBase(string ID)
        {
            DMSSFAssignment dMSSalesForce = (
                from carSchedule in HammerDataProvider.Context.DMSSFAssignments
                where carSchedule.EmployeeID == ID && carSchedule.IsActive == true              
                select carSchedule).FirstOrDefault<DMSSFAssignment>();
            return dMSSalesForce;
        }
        public static Salesperson GetSMName(string NVBH)
        {
            Salesperson sm = (
                from sman in HammerDataProvider.Context.Salespersons
                where sman.SalespersonCD == NVBH 
                select sman).FirstOrDefault<Salesperson>();
            if (sm != null)
            {
                return sm;
            }
            return null;
        }
        public static List<Salesperson> GetNameSM(string NVBH)
        {
            List<Salesperson> sm = (
                from sman in HammerDataProvider.Context.Salespersons
                where sman.SalespersonCD == NVBH && sman.CompanyID == 2
                select sman).ToList<Salesperson>();
            if (sm != null)
            {
                return sm;
            }
            return null;
        }
        public static System.Collections.Generic.List<ScheduleErrorModel> CheckRecordError(System.Collections.Generic.List<ScheduleErrorModel> input, string type)
        {
            System.Collections.Generic.List<ScheduleErrorModel> list = new System.Collections.Generic.List<ScheduleErrorModel>();
            System.Collections.Generic.List<ScheduleErrorModel> list2 = new System.Collections.Generic.List<ScheduleErrorModel>();
            bool flag = false;
            for (int i = 0; i < input.Count; i++)
            {
                ScheduleErrorModel scheduleErrorModel = new ScheduleErrorModel();
                scheduleErrorModel = input[i];
                int num = HammerDataProvider.CheckNullExcel(input[i], type);
                if (num == 1)
                {
                    scheduleErrorModel.Status = num;
                    flag = true;
                    scheduleErrorModel.Note = Utility.Phrase("SendCalendar.CheckNull");
                }
                else
                {
                    DMSSalesForce salesforceByIdActive = HammerDataProvider.GetSalesforceByIdActive(input[i].EmployeeID);
                    if (salesforceByIdActive != null)
                    {
                        System.DateTime dateTime = System.Convert.ToDateTime(string.Concat(new object[]
						{
							scheduleErrorModel.Year.ToString(),
							'/',
							scheduleErrorModel.Month.ToString(),
							'/',
							scheduleErrorModel.Day.ToString(),
							' ',
							scheduleErrorModel.StartTime.ToString()
						}));
                        System.DateTime dateTime2 = System.Convert.ToDateTime(string.Concat(new object[]
						{
							scheduleErrorModel.Year.ToString(),
							'/',
							scheduleErrorModel.Month.ToString(),
							'/',
							scheduleErrorModel.Day.ToString(),
							' ',
							scheduleErrorModel.EndTime.ToString()
						}));
                        string kind = "D";
                        int num2 = System.DateTime.Compare(dateTime, dateTime2);
                        if (num2 >= 0)
                        {
                            scheduleErrorModel.Status = 1;
                            flag = true;
                            scheduleErrorModel.Note = Utility.Phrase("SendCalendar.FromLessTo");
                        }
                        NoAssessmentModel noAssessmentModel = new NoAssessmentModel();
                        noAssessmentModel = HammerDataProvider.ViewNoAssessment(input[i].EmployeeID, dateTime.Date);
                        if (noAssessmentModel.Header.UserID != null)
                        {
                            scheduleErrorModel.Status = 1;
                            flag = true;
                            scheduleErrorModel.Note = Utility.Phrase("SendCalendar.WasAssessment");
                        }
                        else
                        {
                            SMAssessmentModel sMAssessmentModel = new SMAssessmentModel();
                            sMAssessmentModel = HammerDataProvider.ViewAssessmentSM(input[i].EmployeeID, dateTime.Date);
                            if (sMAssessmentModel.Header.AssessmentFor != null)
                            {
                                scheduleErrorModel.Status = 1;
                                flag = true;
                                scheduleErrorModel.Note = Utility.Phrase("SendCalendar.WasAssessment");
                            }
                            else
                            {
                                AssessmentModel assessmentModel = new AssessmentModel();
                                assessmentModel = HammerDataProvider.ViewAssessment(input[i].EmployeeID, dateTime.Date);
                                if (assessmentModel.Header.AssessmentFor != null)
                                {
                                    scheduleErrorModel.Status = 1;
                                    flag = true;
                                    scheduleErrorModel.Note = Utility.Phrase("SendCalendar.WasAssessment");
                                }
                            }
                        }
                        System.DateTime date = System.DateTime.Now.Date;
                        int num3 = System.DateTime.Compare(dateTime.Date, date);
                        if (num3 <= 0)
                        {
                            int num4 = HammerDataProvider.CheckDateInScheduleSubmitSetting(dateTime, input[i].EmployeeID, kind);
                            if (num4 == 1)
                            {
                                scheduleErrorModel.Status = 1;
                                flag = true;
                                scheduleErrorModel.Note = Utility.Phrase("SendCalendar.DateClose");
                            }
                            int num5 = HammerDataProvider.CheckexsitAppointment(input[i].EmployeeID, dateTime, dateTime2, kind);
                            if (num5 == 1)
                            {
                                scheduleErrorModel.Status = num5;
                                flag = true;
                                scheduleErrorModel.Note = Utility.Phrase("SendCalendar.SchedulerAppro");
                            }
                            if (scheduleErrorModel.WWCode != null && scheduleErrorModel.WWCode.Trim() != string.Empty)
                            {
                                string wWCode = scheduleErrorModel.WWCode;
                                if (!string.IsNullOrEmpty(wWCode))
                                {
                                    SystemRole systemRole = HammerDataProvider.EmployeeInRole(wWCode);
                                    if (systemRole == SystemRole.Salesman)
                                    {
                                        if (HammerDataProvider.GetRouteWithSMandSS(scheduleErrorModel.EmployeeID, scheduleErrorModel.WWCode) == null)
                                        {
                                            scheduleErrorModel.Status = 1;
                                            flag = true;
                                            scheduleErrorModel.Note = Utility.Phrase("SendCalendar.MCPError");
                                        }
                                    }
                                    else
                                    {
                                        if (systemRole == SystemRole.SalesForce)
                                        {
                                            scheduleErrorModel.Status = 1;
                                            flag = true;
                                            scheduleErrorModel.Note = Utility.Phrase("SendCalendar.ErrorIssSalefore");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            int num6 = HammerDataProvider.CheckexsitAppointment(input[i].EmployeeID, dateTime, dateTime2, kind);
                            if (num6 == 1)
                            {
                                scheduleErrorModel.Status = num6;
                                flag = true;
                                scheduleErrorModel.Note = Utility.Phrase("SendCalendar.SchedulerAppro");
                            }
                            if (scheduleErrorModel.WWCode != null && scheduleErrorModel.WWCode.Trim() != string.Empty)
                            {
                                string wWCode2 = scheduleErrorModel.WWCode;
                                if (!string.IsNullOrEmpty(wWCode2))
                                {
                                    SystemRole systemRole2 = HammerDataProvider.EmployeeInRole(wWCode2);
                                    if (systemRole2 == SystemRole.Salesman)
                                    {
                                        if (HammerDataProvider.GetRouteWithSMandSS(scheduleErrorModel.EmployeeID, scheduleErrorModel.WWCode) == null)
                                        {
                                            scheduleErrorModel.Status = 1;
                                            flag = true;
                                            scheduleErrorModel.Note = Utility.Phrase("SendCalendar.MCPError");
                                        }
                                    }
                                    else
                                    {
                                        if (systemRole2 == SystemRole.SalesForce)
                                        {
                                            scheduleErrorModel.Status = 1;
                                            flag = true;
                                            scheduleErrorModel.Note = Utility.Phrase("SendCalendar.ErrorIssSalefore");
                                        }
                                    }
                                }
                            }
                        }
                        if (scheduleErrorModel.Status == 2)
                        {
                            scheduleErrorModel.Status = 2;
                            flag = true;
                        }
                    }
                    else
                    {
                        scheduleErrorModel.Status = 1;
                        flag = true;
                        scheduleErrorModel.Note = Utility.Phrase("SendCalendar.SFError");
                    }
                }
                list.Add(scheduleErrorModel);
            }
            if (flag)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    ScheduleErrorModel item = new ScheduleErrorModel();
                    item = input[j];
                    list2.Add(item);
                }
            }
            return list2;
        }
        public static void InsertOrUpdate(System.Collections.Generic.List<ScheduleErrorModel> input, string type, string userApprove)
        {
            //HammerDataProvider.<>c__DisplayClass3ec <>c__DisplayClass3ec = new HammerDataProvider.<>c__DisplayClass3ec();
            //<>c__DisplayClass3ec.input = input;
            var list4 = (
                from list in input
                select new
                {
                    EmployeeIDG = list.EmployeeID,
                    Day = list.Day,
                    Month = list.Month,
                    Year = list.Year
                }).Distinct().ToList();
            foreach (var i in list4)
            {
                System.DateTime Date = System.Convert.ToDateTime(string.Concat(new object[]
				{
					i.Year.ToString(),
					'/',
					i.Month.ToString(),
					'/',
					i.Day.ToString()
				}));
                Appointment appointment = (
                    from carSchedule in HammerDataProvider.Context.Appointments
                    where carSchedule.UserLogin == i.EmployeeIDG && carSchedule.StartDate.Value.Date == Date.Date && (carSchedule.Label == (int?)0 || carSchedule.Label == (int?)2 || carSchedule.Label == (int?)1 || carSchedule.Label == (int?)3)
                    select carSchedule).FirstOrDefault<Appointment>();
                if (appointment != null)
                {
                    SchedulerDataHelper.RemoveAllAppointment(appointment);
                }
            }
            for (int i = 0; i < input.Count; i++)
            {
                Appointment appointment2 = new Appointment();
                appointment2.Subject = input[i].Title;
                appointment2.Description = input[i].Content;
                appointment2.Employees = input[i].WWCode;
                appointment2.Location = input[i].Outlet;
                appointment2.Phone = input[i].PhoneNumber;
                appointment2.Label = 3;
                appointment2.Status = input[i].Status;
                appointment2.IsMeeting = input[i].IsMeeting;
                System.DateTime DateFrom = System.DateTime.Now;
                System.DateTime DateTo = System.DateTime.Now;
                string TypeSchedule = "D";
                if (type == "detail")
                {
                    DateFrom = Convert.ToDateTime(input[i].Year.ToString() + '/'
                                     + input[i].Month.ToString() + '/' + input[i].Day.ToString() + ' ' + input[i].StartTime);
                    DateTo = Convert.ToDateTime(input[i].Year.ToString() + '/' +
                                          input[i].Month.ToString() + '/' + input[i].Day.ToString() + ' ' + input[i].EndTime.ToString());
                    TypeSchedule = "D";
                    appointment2.AllDay = false;

                }
                Appointment appointment3 = null;
                if (type == "detail")
                {
                    appointment3 = (
                        from carSchedule in HammerDataProvider.Context.Appointments
                        where carSchedule.UserLogin == input[i].EmployeeID && carSchedule.StartDate.Value.Date == DateFrom.Date && carSchedule.EndDate.Value.Date == DateTo.Date && carSchedule.ScheduleType == TypeSchedule && (carSchedule.Label == (int?)0 || carSchedule.Label == (int?)2 || carSchedule.Label == (int?)1 || carSchedule.Label == (int?)3)
                        select carSchedule).FirstOrDefault<Appointment>();
                }
                if (appointment3 != null)
                {
                    if (type == "detail")
                    {
                        appointment2.StartDate = new System.DateTime?(DateFrom);
                        appointment2.EndDate = new System.DateTime?(DateTo);
                        appointment2.UserLogin = input[i].EmployeeID;
                        appointment2.UserAppro = userApprove;
                        appointment2.UniqueID = appointment3.UniqueID;
                        appointment2.ScheduleType = "D";
                        appointment2.CreatedDateTime = new System.DateTime?(System.DateTime.Now);
                        appointment2.Status = new int?(0);
                        if (appointment3.Label == 1 || appointment3.Label == 2)
                        {
                            appointment2.Label = new int?(2);
                        }
                        string employees = appointment2.Employees;
                        if (!string.IsNullOrEmpty(employees))
                        {
                            SystemRole systemRole = HammerDataProvider.EmployeeInRole(employees);
                            if (systemRole == SystemRole.Salesman)
                            {
                                DMSDistributorRouteAssignment routeWithSMandSS = HammerDataProvider.GetRouteWithSMandSS(appointment2.UserLogin, appointment2.Employees);
                                if (routeWithSMandSS != null)
                                {
                                    appointment2.RouteID = routeWithSMandSS.RouteCD;
                                }
                            }
                            else
                            {
                                if (systemRole == SystemRole.SalesForce)
                                {
                                    Appointment appointment4 = HammerDataProvider.CheckWWGetAppointments(DateFrom.Date, appointment2.Employees);
                                    if (appointment4 != null)
                                    {
                                        appointment2.RouteID = appointment4.RouteID;
                                    }
                                }
                            }
                        }
                        DMSSFHierarchy dMSSFHierarchy = (
                            from sf in HammerDataProvider.Context.DMSSalesForces
                            join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                            where sf.LoginID.Trim() == input[i].EmployeeID.Trim() && sf.Active == (bool?)true
                            select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
                        if (dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.TerritoryType == 'N')
                        {
                            appointment2.Label = new int?(0);
                        }
                        else
                        {
                            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
                            {
                                appointment2.Label = new int?(0);
                            }
                            else
                            {
                                appointment2.Label = new int?(3);
                                appointment2.UserAppro = userApprove;
                            }
                        }
                        SchedulerDataHelper.UpdateAppointment(appointment2);
                    }
                }
                else
                {
                    appointment2.StartDate = new System.DateTime?(DateFrom);
                    appointment2.EndDate = new System.DateTime?(DateTo);
                    appointment2.UserLogin = input[i].EmployeeID;
                    appointment2.ScheduleType = "D";
                    appointment2.Status = new int?(0);
                    DMSSFHierarchy dMSSFHierarchy2 = (
                        from sf in HammerDataProvider.Context.DMSSalesForces
                        join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                        where sf.LoginID.Trim() == input[i].EmployeeID.Trim() && sf.Active == (bool?)true
                        select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
                    if (dMSSFHierarchy2.IsSalesForce == true && dMSSFHierarchy2.TerritoryType == 'N')
                    {
                        appointment2.Label = new int?(0);
                    }
                    else
                    {
                        if (dMSSFHierarchy2.TerritoryType == 'R' && dMSSFHierarchy2.IsSalesForce == true)
                        {
                            appointment2.Label = new int?(0);
                        }
                        else
                        {
                            appointment2.Label = new int?(3);
                            appointment2.UserAppro = userApprove;
                        }
                    }
                    appointment2.CreatedDateTime = new System.DateTime?(System.DateTime.Now);
                    string employees2 = appointment2.Employees;
                    if (!string.IsNullOrEmpty(employees2))
                    {
                        SystemRole systemRole2 = HammerDataProvider.EmployeeInRole(employees2);
                        if (systemRole2 == SystemRole.Salesman)
                        {
                            DMSDistributorRouteAssignment routeWithSMandSS2 = HammerDataProvider.GetRouteWithSMandSS(appointment2.UserLogin, appointment2.Employees);
                            if (routeWithSMandSS2 != null)
                            {
                                appointment2.RouteID = routeWithSMandSS2.RouteCD;
                            }
                        }
                        else
                        {
                            if (systemRole2 == SystemRole.SalesForce)
                            {
                                Appointment appointment5 = HammerDataProvider.CheckWWGetAppointments(DateFrom.Date, appointment2.Employees);
                                if (appointment5 != null)
                                {
                                    appointment2.RouteID = appointment5.RouteID;
                                }
                            }
                        }
                    }
                    HammerDataProvider.Context.Appointments.InsertOnSubmit(appointment2);
                }
            }
            HammerDataProvider.Context.SubmitChanges();
            var list2 = (
                from list in input
                select new
                {
                    EmployeeIDG = list.EmployeeID,
                    TemplateEmail = list.emailTemplate
                }).Distinct().ToList();
            foreach (var current in list2)
            {
                string str;
                if (current.EmployeeIDG == userApprove)
                {
                    DMSSalesForce salesforceById = HammerDataProvider.GetSalesforceById(current.EmployeeIDG);
                    EmployeeModel employeeModel = HammerDataProvider.PrepareScheduleGetlevel(current.EmployeeIDG);
                    str = "ApproveSchedule";
                    System.Collections.Generic.List<EmployeeModel> list3 = new System.Collections.Generic.List<EmployeeModel>();
                    list3 = HammerDataProvider.GetMangerEmployee(current.EmployeeIDG);
                    try
                    {
                        foreach (EmployeeModel current2 in list3)
                        {
                            DMSSalesForce salesforceById2 = HammerDataProvider.GetSalesforceById(current2.EmployeeID);
                            Util.InitUploadSchedulerEmail(salesforceById2, salesforceById.EmployeeName, (list3 == null) ? "" : current2.EmployeeName, current.TemplateEmail, type, input, Util.GetBaseUrl() + str, current2.Level, employeeModel.Level, salesforceById.EmployeeID);
                        }
                        continue;
                    }
                    catch (System.Exception)
                    {
                        continue;
                    }
                }
                DMSSalesForce salesforceById3 = HammerDataProvider.GetSalesforceById(userApprove);
                EmployeeModel employeeModel2 = HammerDataProvider.PrepareScheduleGetlevel(userApprove);
                EmployeeModel employeeModel3 = new EmployeeModel();
                str = "Home";
                employeeModel3 = HammerDataProvider.PrepareScheduleGetlevel(current.EmployeeIDG);
                DMSSalesForce salesforceById4 = HammerDataProvider.GetSalesforceById(employeeModel3.EmployeeID);
                try
                {
                    Util.InitUploadSchedulerEmail(salesforceById4, employeeModel3.EmployeeName, (employeeModel3 == null) ? "" : salesforceById3.EmployeeName, current.TemplateEmail, type, input, Util.GetBaseUrl() + str, employeeModel2.Level, employeeModel3.Level);
                }
                catch (System.Exception)
                {
                }
            }
        }
        public static DMSSFHierarchy PrepareScheduleGetDMSSFHierarchy(string user)
        {
            return (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim().ToUpper() == user.ToUpper() && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
        }
        public static EmployeeModel PrepareScheduleGetlevel(string user)
        {
            EmployeeModel employeeModel = new EmployeeModel();
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == user.Trim().ToUpper());
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == user && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && !dMSSFHierarchy.Parent.HasValue)
            {
                employeeModel.EmployeeID = dMSSalesForce.EmployeeID;
                employeeModel.EmployeeName = dMSSalesForce.EmployeeName;
                employeeModel.Level = dMSSFHierarchy.LevelName;
            }
            else
            {
                if (dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.Parent.HasValue)
                {
                    employeeModel.EmployeeID = dMSSalesForce.EmployeeID;
                    employeeModel.EmployeeName = dMSSalesForce.EmployeeName;
                    employeeModel.Level = dMSSFHierarchy.LevelName;
                }
                else
                {
                    if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
                    {
                        employeeModel.EmployeeID = dMSSalesForce.EmployeeID;
                        employeeModel.EmployeeName = dMSSalesForce.EmployeeName;
                        employeeModel.Level = dMSSFHierarchy.LevelName;
                    }
                    else
                    {
                        if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
                        {
                            employeeModel.EmployeeID = dMSSalesForce.EmployeeID;
                            employeeModel.EmployeeName = dMSSalesForce.EmployeeName;
                            employeeModel.Level = dMSSFHierarchy.LevelName;
                        }
                        else
                        {
                            if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true)
                            {
                                employeeModel.EmployeeID = dMSSalesForce.EmployeeID;
                                employeeModel.EmployeeName = dMSSalesForce.EmployeeName;
                                employeeModel.Level = dMSSFHierarchy.LevelName;
                            }
                        }
                    }
                }
            }
            return employeeModel;
        }
        public static DMSDistributorRouteAssignment GetRouteWithSMandSS(string NVSS, string NVSM)
        {
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == NVSS.Trim().ToUpper() && x.Active == true);
            DMSDistributorRouteAssignment dMSDistributorRouteAssignment = new DMSDistributorRouteAssignment();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == NVSS && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy == null)
            {
                return dMSDistributorRouteAssignment;
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && !dMSSFHierarchy.Parent.HasValue)
            {
                return null;
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.Parent.HasValue)
            {
                System.Collections.Generic.List<EmployeeModel> sSSMNotInRSM = HammerDataProvider.GetSSSMNotInRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = sSSMNotInRSM.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        dMSDistributorRouteAssignment = (
                            from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                            join sm in HammerDataProvider.Context.Salespersons on smAssignment.SalesPersonID equals (int?)sm.SalespersonID
                            where sm.SalespersonCD.Trim() == NVSM && smAssignment.SalesSupID == ss.EmployeeID
                            select smAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
                        if (dMSDistributorRouteAssignment != null)
                        {
                            DMSDistributorRouteAssignment result = dMSDistributorRouteAssignment;
                            return result;
                        }
                    }
                    return dMSDistributorRouteAssignment;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
            {
                System.Collections.Generic.List<EmployeeModel> sSSMNotInASM = HammerDataProvider.GetSSSMNotInASM(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator2 = sSSMNotInASM.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator2.MoveNext())
                    {
                        ss = enumerator2.Current;
                        dMSDistributorRouteAssignment = (
                            from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                            join sm in HammerDataProvider.Context.Salespersons on smAssignment.SalesPersonID equals (int?)sm.SalespersonID
                            where sm.SalespersonCD.Trim() == NVSM && smAssignment.SalesSupID == ss.EmployeeID
                            select smAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
                        if (dMSDistributorRouteAssignment != null)
                        {
                            DMSDistributorRouteAssignment result = dMSDistributorRouteAssignment;
                            return result;
                        }
                    }
                    return dMSDistributorRouteAssignment;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateSS = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSS.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        dMSDistributorRouteAssignment = (
                            from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                            join sm in HammerDataProvider.Context.Salespersons on smAssignment.SalesPersonID equals (int?)sm.SalespersonID
                            where sm.SalespersonCD.Trim() == NVSM && smAssignment.SalesSupID == ss.EmployeeID
                            select smAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
                        if (dMSDistributorRouteAssignment != null)
                        {
                            DMSDistributorRouteAssignment result = dMSDistributorRouteAssignment;
                            return result;
                        }
                    }
                    return dMSDistributorRouteAssignment;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true)
            {
                dMSDistributorRouteAssignment = (
                    from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                    join sm in HammerDataProvider.Context.Salespersons on smAssignment.SalesPersonID equals sm.SalespersonID
                    where sm.SalespersonCD.Trim() == NVSM && smAssignment.SalesSupID == NVSS
                    select smAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
                if (dMSDistributorRouteAssignment != null)
                {
                    return dMSDistributorRouteAssignment;
                }
            }
            return dMSDistributorRouteAssignment;
        }
        public static DMSVisitPlanHistory CheckWWGetVisitPlanHistory(string salespersonCD, System.DateTime visitDate)
        {
            return (
                from visitplan in HammerDataProvider.Context.DMSVisitPlanHistories
                where visitplan.SlsperID == salespersonCD && visitplan.VisitDate.Date == visitDate.Date
                select visitplan).FirstOrDefault<DMSVisitPlanHistory>();
        }
        public static Appointment CheckWWGetAppointments(System.DateTime date, string employeeID)
        {
            Appointment appointment = null;
            appointment = (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UserLogin == employeeID && ap.AllDay == (bool?)true && ap.Label == (int?)3 && ap.StartDate.Value.Date == date.Date && ap.ScheduleType == "D"
                select ap into x
                orderby x.StartDate
                select x).FirstOrDefault<Appointment>();
            if (appointment != null)
            {
                if (appointment.Employees != null)
                {
                    while (appointment != null)
                    {
                        if (HammerDataProvider.EmployeeInRole(appointment.Employees) == SystemRole.Salesman)
                        {
                            break;
                        }
                        appointment = (
                            from ap in HammerDataProvider.Context.Appointments
                            where ap.UserLogin == appointment.Employees && ap.AllDay == (bool?)true && ap.Label == (int?)3 && ap.StartDate.Value.Date == date.Date && ap.ScheduleType == "D"
                            select ap into x
                            orderby x.StartDate
                            select x).FirstOrDefault<Appointment>();
                    }
                }
                else
                {
                    appointment = null;
                }
            }
            return appointment;
        }
        public static DMSDistributorRouteAssignment GetRegionSMEmployees(string ID)
        {
            int SaleID = (
                from SalePerson in HammerDataProvider.Context.Salespersons
                where SalePerson.SalespersonCD.Trim() == ID.Trim() && SalePerson.CompanyID != 2
                select SalePerson.SalespersonID).FirstOrDefault<int>();
            return (
                from routeAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                where routeAssignment.SalesPersonID == (int?)SaleID
                select routeAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
        }
        public static DMSDistributorRouteAssignment GetRegionSSEmployees(string ID)
        {
            return (
                from routeAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                where routeAssignment.SalesSupID.Trim() == ID.Trim()
                select routeAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
        }
        public static EmployeeModel GetMangerCDD(string salesSupID)
        {
            string Nation = (
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == salesSupID.Trim()
                select sfAssignment.RegionID).FirstOrDefault<string>();
            return (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFAssignments on sf.EmployeeID equals sfAssignment.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where sf.EmployeeID.Trim() != salesSupID && (int?)sfh.TerritoryType == (int?)78 && sfh.IsSalesForce == (bool?)true && sfh.Parent == null && sfAssignment.RegionID == Nation && sf.Active == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                }).FirstOrDefault<EmployeeModel>();
        }
        public static EmployeeModel GetMangerNSM(string salesSupID)
        {
            string Nation = (
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == salesSupID.Trim()
                select sfAssignment.RegionID).FirstOrDefault<string>();
            return (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFAssignments on sf.EmployeeID equals sfAssignment.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where sf.EmployeeID.Trim() != salesSupID && (int?)sfh.TerritoryType == (int?)78 && sfh.IsSalesForce == (bool?)true && sfh.Parent != null && sfAssignment.RegionID == Nation && sf.Active == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                }).FirstOrDefault<EmployeeModel>();
        }
        public static EmployeeModel GetMangerRSM(string salesSupID)
        {
            string Region = (
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == salesSupID.Trim()
                select sfAssignment.RegionID).FirstOrDefault<string>();
            return (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFAssignments on sf.EmployeeID equals sfAssignment.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where sf.EmployeeID.Trim() != salesSupID && (int?)sfh.TerritoryType == (int?)82 && sfh.IsSalesForce == (bool?)true && sfAssignment.RegionID == Region && sf.Active == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                }).FirstOrDefault<EmployeeModel>();
        }
        public static EmployeeModel GetMangerArea(string salesSupID)
        {
            string Area = (
                from sfAssignment in HammerDataProvider.Context.DMSSFAssignments
                where sfAssignment.EmployeeID.Trim() == salesSupID.Trim()
                && sfAssignment.IsActive == true
                select sfAssignment.AreaID).FirstOrDefault<string>();
            return (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFAssignments on sf.EmployeeID equals sfAssignment.EmployeeID
                join sfh in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfh.LevelID
                where sf.EmployeeID.Trim() != salesSupID && (int?)sfh.TerritoryType == (int?)65 && sfh.IsSalesForce == (bool?)true && sfAssignment.AreaID == Area && sf.Active == (bool?)true
                select new EmployeeModel
                {
                    EmployeeID = sf.EmployeeID,
                    EmployeeName = sf.EmployeeName,
                    Level = sfh.LevelName
                }).FirstOrDefault<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> GetMangerEmployeeViewAppointment(string userLogin)
        {
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == userLogin.Trim().ToUpper());
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == userLogin && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy == null)
            {
                return list;
            }
            if (!(dMSSFHierarchy.IsSalesForce == true) || !(dMSSFHierarchy.TerritoryType == 'N') || dMSSFHierarchy.Parent.HasValue)
            {
                if (dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.Parent.HasValue)
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateCDD = HammerDataProvider.GetSubordinateCDD(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateCDD.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            EmployeeModel current = enumerator.Current;
                            list.Add(current);
                        }
                        return list;
                    }
                }
                if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateNSM = HammerDataProvider.GetSubordinateNSM(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator2 = subordinateNSM.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            EmployeeModel current2 = enumerator2.Current;
                            list.Add(current2);
                        }
                        return list;
                    }
                }
                if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateRSM = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator3 = subordinateRSM.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            EmployeeModel current3 = enumerator3.Current;
                            list.Add(current3);
                        }
                        return list;
                    }
                }
                list.Add(HammerDataProvider.GetMangerArea(dMSSalesForce.EmployeeID));
            }
            return list;
        }
        public static System.Collections.Generic.List<EmployeeModel> GetMangerEmployee(string userLogin)
        {
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == userLogin.Trim().ToUpper() && x.Active == true);
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == userLogin && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy == null)
            {
                return list;
            }
            if (!(dMSSFHierarchy.IsSalesForce == true) || !(dMSSFHierarchy.TerritoryType == 'N') || dMSSFHierarchy.Parent.HasValue)
            {
                if (dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.Parent.HasValue)
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateCDD = HammerDataProvider.GetSubordinateCDD(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateCDD.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            EmployeeModel current = enumerator.Current;
                            list.Add(current);
                        }
                        return list;
                    }
                }
                if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateNSM = HammerDataProvider.GetSubordinateNSM(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator2 = subordinateNSM.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            EmployeeModel current2 = enumerator2.Current;
                            list.Add(current2);
                        }
                        return list;
                    }
                }
                if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
                {
                    System.Collections.Generic.List<EmployeeModel> subordinateRSM = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator3 = subordinateRSM.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            EmployeeModel current3 = enumerator3.Current;
                            list.Add(current3);
                        }
                        return list;
                    }
                }
                list.Add(HammerDataProvider.GetMangerArea(dMSSalesForce.EmployeeID));
            }
            return list;
        }
        public static System.Collections.Generic.List<EmployeeModel> ReportResutlTrainingGetEmployees()
        {
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            IOrderedEnumerable<DMSSalesForce> orderedEnumerable =
                from x in
                    (
                        from sf in HammerDataProvider.Context.DMSSalesForces
                        where sf.SFLevel <= 2
                        select sf).ToList<DMSSalesForce>()
                orderby x.SFLevel
                select x;
            foreach (DMSSalesForce current in orderedEnumerable)
            {
                EmployeeModel employeeModel = new EmployeeModel();
                employeeModel.EmployeeID = current.EmployeeID;
                employeeModel.EmployeeName = current.EmployeeName;
                string level = string.Empty;
                if (current.SFLevel == 1)
                {
                    level = "NSM";
                }
                else
                {
                    if (current.SFLevel == 2 && current.AssignmentLevel == 'R')
                    {
                        level = "RSM";
                    }
                    else
                    {
                        if (current.AssignmentLevel == 'A')
                        {
                            level = "ASM";
                        }
                        else
                        {
                            level = "SS";
                        }
                    }
                }
                employeeModel.Level = level;
                list.Add(employeeModel);
            }
            return list;
        }
        public static System.Collections.Generic.List<Region> GetRegionEmployees(string user)
        {
            System.Collections.Generic.List<Region> list = new System.Collections.Generic.List<Region>();
            if (user != null)
            {
                IOrderedEnumerable<DMSSFAssignment> orderedEnumerable =
                    from x in
                        (
                            from sf in HammerDataProvider.Context.DMSSFAssignments
                            where sf.EmployeeID.Trim() == user.Trim()
                            && sf.IsActive == true
                            select sf).ToList<DMSSFAssignment>()
                    orderby x.RegionID
                    select x;
                foreach (DMSSFAssignment item in orderedEnumerable)
                {
                    Region item2 = (
                        from kv in HammerDataProvider.Context.Regions
                        where kv.RegionID.Trim() == item.RegionID.Trim()
                        select kv).FirstOrDefault<Region>();
                    list.Add(item2);
                }
                list = list.Distinct<Region>().ToList();
                list = (
                    from x in list
                    orderby x.RegionID
                    select x).ToList<Region>();
            }
            return list;
        }
        public static System.Collections.IEnumerable GetAssessmentsDetail(string User)
        {
            return
                from ap in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                where ap.UserID == User
                select ap;
        }
        public static Region GetRegionName(string User)
        {
            return (
                from re in HammerDataProvider.Context.Regions
                join sfa in HammerDataProvider.Context.DMSSFAssignments on re.RegionID equals sfa.RegionID
                where sfa.EmployeeID.Trim() == User.Trim()
                  && sfa.IsActive == true
                select re).FirstOrDefault<Region>();
        }
        public static Region GetNameRegion(string id)
        {
            return (
                from re in HammerDataProvider.Context.Regions
                where re.RegionID == id
                select re).FirstOrDefault<Region>();
        }
        public static Area GetNameArea(string id)
        {
            return (
                from re in HammerDataProvider.Context.Areas
                where re.AreaID == id
                select re).FirstOrDefault<Area>();
        }
        public static System.Collections.Generic.List<Region> HRGetRegion(string employeeID)
        {
            System.Collections.Generic.IEnumerable<string> listRegion = (
                from setting in HammerDataProvider.Context.DMSSFAssignments
                where setting.EmployeeID.Trim().ToUpper() == employeeID.Trim().ToUpper() && setting.IsActive == true
                select setting.RegionID).ToList<string>().Distinct<string>();

            System.Collections.Generic.List<Region> list = new System.Collections.Generic.List<Region>();

            System.Collections.Generic.List<Region> list2 = (
                from sf in HammerDataProvider.Context.Regions
                where listRegion.Contains(sf.RegionID)
                select sf).ToList<Region>();
            foreach (Region current in list2)
            {
                list.Add(current);
            }
            return list;
        }
        public static System.Collections.Generic.List<ReportEmployeesStatusModel> GetdataReportEmployeesStatus(string User, System.DateTime begin, System.DateTime end, string region, string area, string em)
        {
            System.Collections.Generic.List<ReportEmployeesStatusModel> list = new System.Collections.Generic.List<ReportEmployeesStatusModel>();
            System.Collections.Generic.List<EmployeeModel> listitem = HammerDataProvider.GetSubordinateNoDuplicate(User).Where(x => x.Level != "SM").ToList();
            List<EmployeeModel> list2 = HammerDataProvider.GetEmployeesRegionAreaPhanCap(region, area, listitem);
            if (!String.IsNullOrEmpty(em))
                list2 = list2.Where(x => x.EmployeeID.Trim() == em.Trim()).ToList();
            foreach (EmployeeModel current in list2)
            {
                for (int i = end.Subtract(begin).Days; i >= 0; i--)
                {
                    System.DateTime date = begin.AddDays((double)i).Date;
                    System.Collections.Generic.List<Appointment> detailAppointmentListTask = HammerDataProvider.GetDetailAppointmentListTaskStatus(current.EmployeeID, date.Date);
                    foreach (Appointment current2 in detailAppointmentListTask)
                    {
                        ReportEmployeesStatusModel reportEmployeesStatusModel = new ReportEmployeesStatusModel();
                        if (current2.IsDelete == false)
                        {
                            if (current2.Label == 3)
                            {
                                reportEmployeesStatusModel.Status = true;
                            }
                            else
                            {
                                reportEmployeesStatusModel.Status = false;
                            }
                        }
                        else
                        {
                            reportEmployeesStatusModel.Status = false;
                        }
                        reportEmployeesStatusModel.Date = current2.StartDate.Value;
                        reportEmployeesStatusModel.EmployeeID = current.EmployeeID;
                        reportEmployeesStatusModel.EmployeeName = current.EmployeeName;
                        reportEmployeesStatusModel.Level = current.Level;
                        reportEmployeesStatusModel.ShiftID = current2.ShiftID;
                        reportEmployeesStatusModel.StatusTraining = HammerDataProvider.HasAssessmentWW(current2.StartDate.Value, current.EmployeeID, current2.UniqueID);
                        reportEmployeesStatusModel.StatusNotTraining = HammerDataProvider.HasAssessmentNoWW(current2.StartDate.Value, current.EmployeeID, current2.UniqueID);
                        list.Add(reportEmployeesStatusModel);
                    }
                }
            }
            return list;
        }
        public static int CheckEmployeesInAppointmentMonth(string User, System.DateTime begin)
        {
            Appointment appointment = (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UserLogin == User && carSchedule.StartDate.Value.Date == begin && carSchedule.AllDay == (bool?)true && carSchedule.ScheduleType == "M"
                select carSchedule).FirstOrDefault<Appointment>();
            if (appointment != null)
            {
                return 1;
            }
            return 0;
        }
        public static int CheckEmployeesInAppointmentDetail(string User, System.DateTime begin)
        {
            Appointment appointment = (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UserLogin == User && carSchedule.StartDate.Value.Date == begin && carSchedule.AllDay == (bool?)true && carSchedule.ScheduleType == "D"
                select carSchedule).FirstOrDefault<Appointment>();
            if (appointment != null)
            {
                return 1;
            }
            return 0;
        }
        public static int CheckEmployeesInSubmitAssessment(string User, System.DateTime begin, string Level)
        {
            if (Level == "ASM" || Level == "RSM" || Level == "NSM")
            {
                TrainingAssessmentHeader trainingAssessmentHeader = (
                    from train in HammerDataProvider.Context.TrainingAssessmentHeaders
                    where train.UserID == User && train.AssessmentDate.Date == begin && train.Released == true
                    select train).FirstOrDefault<TrainingAssessmentHeader>();
                if (trainingAssessmentHeader != null)
                {
                    return 1;
                }
                SMTrainingAssessmentHeader sMTrainingAssessmentHeader = (
                    from SMtrainingAssessmentHeader in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                    where SMtrainingAssessmentHeader.UserID == User && SMtrainingAssessmentHeader.AssessmentDate.Date == begin && SMtrainingAssessmentHeader.Released == true
                    select SMtrainingAssessmentHeader).FirstOrDefault<SMTrainingAssessmentHeader>();
                if (sMTrainingAssessmentHeader != null)
                {
                    return 1;
                }
                return 0;
            }
            else
            {
                SMTrainingAssessmentHeader sMTrainingAssessmentHeader2 = (
                    from SMtrainingAssessmentHeader in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                    where SMtrainingAssessmentHeader.UserID == User && SMtrainingAssessmentHeader.AssessmentDate.Date == begin && SMtrainingAssessmentHeader.Released == true
                    select SMtrainingAssessmentHeader).FirstOrDefault<SMTrainingAssessmentHeader>();
                if (sMTrainingAssessmentHeader2 != null)
                {
                    return 1;
                }
                return 0;
            }
        }
        public static int CheckEmployeesInSubmitAssessmentNotTraining(string User, System.DateTime begin)
        {
            NoTrainingAssessment noTrainingAssessment = (
                from noTrain in HammerDataProvider.Context.NoTrainingAssessments
                where noTrain.UserID == User && noTrain.AssessmentDate.Date == begin && noTrain.Released == true
                select noTrain).FirstOrDefault<NoTrainingAssessment>();
            if (noTrainingAssessment != null)
            {
                return 1;
            }
            return 0;
        }
        public static System.Collections.Generic.List<EmployeeModel> ViewAssessmentGetSubordinate(string userLogin)
        {
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == userLogin.Trim().ToUpper());
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == userLogin && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy == null)
            {
                return list;
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && !dMSSFHierarchy.Parent.HasValue)
            {
                string a = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                if (a != "ApproveSchedule")
                {
                    System.Collections.Generic.List<EmployeeModel> allEmployee = HammerDataProvider.GetAllEmployee(dMSSalesForce);
                    using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = allEmployee.GetEnumerator())
                    {
                        EmployeeModel nsm;
                        while (enumerator.MoveNext())
                        {
                            nsm = enumerator.Current;
                            EmployeeModel employeeModel = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == nsm.EmployeeID) == null)
                            {
                                list.Add(nsm);
                            }
                        }
                        goto IL_E23;
                    }
                }
                System.Collections.Generic.List<EmployeeModel> subordinateNSM = HammerDataProvider.GetSubordinateNSM(dMSSalesForce);
                foreach (EmployeeModel rsm in subordinateNSM)
                {
                    EmployeeModel employeeModel2 = new EmployeeModel();
                    //EmployeeModel rsm;
                    if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                    {
                        list.Add(rsm);
                    }
                }
                System.Collections.Generic.List<EmployeeModel> subordinateRSM = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        EmployeeModel employeeModel3 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                        {
                            list.Add(rsm);
                        }
                    }
                    goto IL_E23;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == false)
            {
                System.Collections.Generic.List<EmployeeModel> allEmployee2 = HammerDataProvider.GetAllEmployee(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = allEmployee2.GetEnumerator())
                {
                    EmployeeModel nsm;
                    while (enumerator.MoveNext())
                    {
                        nsm = enumerator.Current;
                        EmployeeModel employeeModel4 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == nsm.EmployeeID) == null)
                        {
                            list.Add(nsm);
                        }
                    }
                    goto IL_E23;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.Parent.HasValue)
            {
                string a2 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                string a3 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                if (a2 != "ApproveSchedule" && a3 != "DetailPrepareSchedulePartialView")
                {
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = dMSSalesForce.EmployeeID,
                        EmployeeName = dMSSalesForce.EmployeeName,
                        Level = "NSM"
                    });
                }
                if (!(a2 != "PrepareSchedule") || !(a2 != "SendCalendar"))
                {
                    goto IL_E23;
                }
                System.Collections.Generic.List<EmployeeModel> subordinateRSM2 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM2.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        EmployeeModel employeeModel5 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                        {
                            list.Add(rsm);
                        }
                    }
                    goto IL_E23;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == false)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateRSM3 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM3.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        EmployeeModel employeeModel6 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                        {
                            list.Add(rsm);
                        }
                        System.Collections.Generic.List<EmployeeModel> subordinateASM = HammerDataProvider.GetSubordinateASM(rsm.EmployeeID);
                        foreach (EmployeeModel asm in subordinateASM)
                        {
                            EmployeeModel employeeModel7 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                            {
                                list.Add(asm);
                            }
                            System.Collections.Generic.List<EmployeeModel> subordinateSS = HammerDataProvider.GetSubordinateSS(asm.EmployeeID);
                            foreach (EmployeeModel ss in subordinateSS)
                            {
                                EmployeeModel employeeModel8 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                                {
                                    list.Add(ss);
                                }
                            }
                        }
                    }
                    goto IL_E23;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
            {
                string a4 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                string a5 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                if (a4 != "ApproveSchedule" && a5 != "DetailPrepareSchedulePartialView")
                {
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = dMSSalesForce.EmployeeID,
                        EmployeeName = dMSSalesForce.EmployeeName,
                        Level = "RSM"
                    });
                }
                System.Collections.Generic.List<EmployeeModel> subordinateASM2 = HammerDataProvider.GetSubordinateASM(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateASM2.GetEnumerator())
                {
                    EmployeeModel asm;
                    while (enumerator.MoveNext())
                    {
                        asm = enumerator.Current;
                        EmployeeModel employeeModel9 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                        {
                            list.Add(asm);
                        }
                    }
                    goto IL_E23;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == false)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateSS2 = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSS2.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        EmployeeModel employeeModel10 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                        {
                            list.Add(ss);
                        }
                    }
                    goto IL_E23;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
            {
                string a6 = System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                if (a6 == "ReportEmployeesStatus")
                {
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = dMSSalesForce.EmployeeID,
                        EmployeeName = dMSSalesForce.EmployeeName,
                        Level = "ASM"
                    });
                }
                System.Collections.Generic.List<EmployeeModel> subordinateSS3 = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSS3.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        EmployeeModel employeeModel11 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                        {
                            list.Add(ss);
                        }
                    }
                    goto IL_E23;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == false)
            {
                list.Add(new EmployeeModel
                {
                    EmployeeID = dMSSalesForce.EmployeeID,
                    EmployeeName = dMSSalesForce.EmployeeName,
                    Level = "SS"
                });
            }
            else
            {
                if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true)
                {
                    list.Add(new EmployeeModel
                    {
                        EmployeeID = dMSSalesForce.EmployeeID,
                        EmployeeName = dMSSalesForce.EmployeeName,
                        Level = "SS"
                    });
                }
            }
        IL_E23:
            return list.Distinct<EmployeeModel>().ToList<EmployeeModel>();
        }
        public static System.Collections.Generic.List<EmployeeModel> ViewAssessmentGetEmployees(string userLogin, System.DateTime Date, string check)
        {
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == userLogin.Trim().ToUpper());
            System.Collections.Generic.List<EmployeeModel> list = new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            new System.Collections.Generic.List<EmployeeModel>();
            System.Collections.Generic.List<EmployeeModel> list2 = new System.Collections.Generic.List<EmployeeModel>();
            System.Collections.Generic.List<EmployeeModel> list3 = new System.Collections.Generic.List<EmployeeModel>();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim().ToUpper() == userLogin.Trim().ToUpper() && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            bool check2 = System.Convert.ToBoolean(check);
            if (dMSSFHierarchy == null)
            {
                return list;
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && !dMSSFHierarchy.Parent.HasValue)
            {
                EmployeeModel employeeModel = new EmployeeModel();
                employeeModel.EmployeeID = dMSSalesForce.EmployeeID;
                employeeModel.EmployeeName = dMSSalesForce.EmployeeName;
                employeeModel.Level = dMSSFHierarchy.LevelName;
                bool flag = HammerDataProvider.CheckAssesmentAddList(employeeModel.EmployeeID, Date.Date, check2, employeeModel.Level);
                if (flag)
                {
                    list.Add(employeeModel);
                }
                System.Collections.Generic.List<EmployeeModel> subordinateRSM = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        bool flag2 = HammerDataProvider.CheckAssesmentAddList(rsm.EmployeeID, Date.Date, check2, rsm.Level);
                        if (flag2)
                        {
                            EmployeeModel employeeModel2 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                            {
                                list.Add(rsm);
                            }
                        }
                        System.Collections.Generic.List<EmployeeModel> subordinateASM = HammerDataProvider.GetSubordinateASM(rsm.EmployeeID);
                        foreach (EmployeeModel asm in subordinateASM)
                        {
                            bool flag3 = HammerDataProvider.CheckAssesmentAddList(asm.EmployeeID, Date.Date, check2, asm.Level);
                            if (flag3)
                            {
                                EmployeeModel employeeModel3 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                                {
                                    list.Add(asm);
                                }
                            }
                            System.Collections.Generic.List<EmployeeModel> subordinateSS = HammerDataProvider.GetSubordinateSS(asm.EmployeeID);
                            foreach (EmployeeModel ss in subordinateSS)
                            {
                                bool flag4 = HammerDataProvider.CheckAssesmentAddList(ss.EmployeeID, Date.Date, check2, ss.Level);
                                if (flag4)
                                {
                                    EmployeeModel employeeModel4 = new EmployeeModel();
                                    if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                                    {
                                        list.Add(ss);
                                    }
                                }
                            }
                        }
                    }
                    goto IL_13F4;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == false)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateRSM2 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM2.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        bool flag5 = HammerDataProvider.CheckAssesmentAddList(rsm.EmployeeID, Date.Date, check2, rsm.Level);
                        if (flag5)
                        {
                            EmployeeModel employeeModel5 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                            {
                                list.Add(rsm);
                            }
                        }
                        System.Collections.Generic.List<EmployeeModel> subordinateASM2 = HammerDataProvider.GetSubordinateASM(rsm.EmployeeID);
                        foreach (EmployeeModel asm in subordinateASM2)
                        {
                            bool flag6 = HammerDataProvider.CheckAssesmentAddList(asm.EmployeeID, Date.Date, check2, asm.Level);
                            if (flag6)
                            {
                                EmployeeModel employeeModel6 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                                {
                                    list.Add(asm);
                                }
                            }
                            System.Collections.Generic.List<EmployeeModel> subordinateSS2 = HammerDataProvider.GetSubordinateSS(asm.EmployeeID);
                            foreach (EmployeeModel ss in subordinateSS2)
                            {
                                bool flag7 = HammerDataProvider.CheckAssesmentAddList(ss.EmployeeID, Date.Date, check2, ss.Level);
                                if (flag7)
                                {
                                    EmployeeModel employeeModel7 = new EmployeeModel();
                                    if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                                    {
                                        list.Add(ss);
                                    }
                                }
                            }
                        }
                    }
                    goto IL_13F4;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.Parent.HasValue)
            {
                EmployeeModel employeeModel8 = new EmployeeModel();
                employeeModel8.EmployeeID = dMSSalesForce.EmployeeID;
                employeeModel8.EmployeeName = dMSSalesForce.EmployeeName;
                employeeModel8.Level = dMSSFHierarchy.LevelName;
                bool flag8 = HammerDataProvider.CheckAssesmentAddList(employeeModel8.EmployeeID, Date.Date, check2, employeeModel8.Level);
                if (flag8)
                {
                    list.Add(employeeModel8);
                }
                System.Collections.Generic.List<EmployeeModel> subordinateRSM3 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM3.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        bool flag9 = HammerDataProvider.CheckAssesmentAddList(rsm.EmployeeID, Date.Date, check2, rsm.Level);
                        if (flag9)
                        {
                            EmployeeModel employeeModel9 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                            {
                                list.Add(rsm);
                            }
                        }
                        System.Collections.Generic.List<EmployeeModel> subordinateASM3 = HammerDataProvider.GetSubordinateASM(rsm.EmployeeID);
                        foreach (EmployeeModel asm in subordinateASM3)
                        {
                            bool flag10 = HammerDataProvider.CheckAssesmentAddList(asm.EmployeeID, Date.Date, check2, asm.Level);
                            if (flag10)
                            {
                                EmployeeModel employeeModel10 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                                {
                                    list.Add(asm);
                                }
                            }
                            System.Collections.Generic.List<EmployeeModel> subordinateSS3 = HammerDataProvider.GetSubordinateSS(asm.EmployeeID);
                            foreach (EmployeeModel ss in subordinateSS3)
                            {
                                bool flag11 = HammerDataProvider.CheckAssesmentAddList(ss.EmployeeID, Date.Date, check2, ss.Level);
                                if (flag11)
                                {
                                    EmployeeModel employeeModel11 = new EmployeeModel();
                                    if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                                    {
                                        list.Add(ss);
                                    }
                                }
                            }
                        }
                    }
                    goto IL_13F4;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == false)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateRSM4 = HammerDataProvider.GetSubordinateRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateRSM4.GetEnumerator())
                {
                    EmployeeModel rsm;
                    while (enumerator.MoveNext())
                    {
                        rsm = enumerator.Current;
                        EmployeeModel employeeModel12 = new EmployeeModel();
                        if (list.Find((EmployeeModel f) => f.EmployeeID == rsm.EmployeeID) == null)
                        {
                            list.Add(rsm);
                        }
                        System.Collections.Generic.List<EmployeeModel> subordinateASM4 = HammerDataProvider.GetSubordinateASM(rsm.EmployeeID);
                        foreach (EmployeeModel asm in subordinateASM4)
                        {
                            EmployeeModel employeeModel13 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                            {
                                list.Add(asm);
                            }
                            System.Collections.Generic.List<EmployeeModel> subordinateSS4 = HammerDataProvider.GetSubordinateSS(asm.EmployeeID);
                            foreach (EmployeeModel ss in subordinateSS4)
                            {
                                EmployeeModel employeeModel14 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                                {
                                    list.Add(ss);
                                }
                            }
                        }
                    }
                    goto IL_13F4;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
            {
                EmployeeModel employeeModel15 = new EmployeeModel();
                employeeModel15.EmployeeID = dMSSalesForce.EmployeeID;
                employeeModel15.EmployeeName = dMSSalesForce.EmployeeName;
                employeeModel15.Level = dMSSFHierarchy.LevelName;
                bool flag12 = HammerDataProvider.CheckAssesmentAddList(employeeModel15.EmployeeID, Date.Date, check2, employeeModel15.Level);
                if (flag12)
                {
                    list.Add(employeeModel15);
                }
                System.Collections.Generic.List<EmployeeModel> subordinateASM5 = HammerDataProvider.GetSubordinateASM(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateASM5.GetEnumerator())
                {
                    EmployeeModel asm;
                    while (enumerator.MoveNext())
                    {
                        asm = enumerator.Current;
                        bool flag13 = HammerDataProvider.CheckAssesmentAddList(asm.EmployeeID, Date.Date, check2, asm.Level);
                        if (flag13)
                        {
                            EmployeeModel employeeModel16 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == asm.EmployeeID) == null)
                            {
                                list.Add(asm);
                            }
                        }
                        System.Collections.Generic.List<EmployeeModel> subordinateSS5 = HammerDataProvider.GetSubordinateSS(asm.EmployeeID);
                        foreach (EmployeeModel ss in subordinateSS5)
                        {
                            bool flag14 = HammerDataProvider.CheckAssesmentAddList(ss.EmployeeID, Date.Date, check2, ss.Level);
                            if (flag14)
                            {
                                EmployeeModel employeeModel17 = new EmployeeModel();
                                if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                                {
                                    list.Add(ss);
                                }
                            }
                        }
                    }
                    goto IL_13F4;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == false)
            {
                list.Clear();
                list2.Clear();
                list3.Clear();
                EmployeeModel employeeModel18 = new EmployeeModel();
                employeeModel18.EmployeeID = dMSSalesForce.EmployeeID;
                employeeModel18.EmployeeName = dMSSalesForce.EmployeeName;
                employeeModel18.Level = dMSSFHierarchy.LevelName;
                bool flag15 = HammerDataProvider.CheckAssesmentAddList(employeeModel18.EmployeeID, Date.Date, check2, employeeModel18.Level);
                if (flag15)
                {
                    list.Add(employeeModel18);
                }
                System.Collections.Generic.List<EmployeeModel> subordinateSS6 = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSS6.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        bool flag16 = HammerDataProvider.CheckAssesmentAddList(ss.EmployeeID, Date.Date, check2, ss.Level);
                        if (flag16)
                        {
                            EmployeeModel employeeModel19 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                            {
                                list.Add(ss);
                            }
                        }
                    }
                    goto IL_13F4;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
            {
                list.Clear();
                list2.Clear();
                list3.Clear();
                EmployeeModel employeeModel20 = new EmployeeModel();
                employeeModel20.EmployeeID = dMSSalesForce.EmployeeID;
                employeeModel20.EmployeeName = dMSSalesForce.EmployeeName;
                employeeModel20.Level = dMSSFHierarchy.LevelName;
                bool flag17 = HammerDataProvider.CheckAssesmentAddList(employeeModel20.EmployeeID, Date.Date, check2, employeeModel20.Level);
                if (flag17)
                {
                    list.Add(employeeModel20);
                }
                System.Collections.Generic.List<EmployeeModel> subordinateSS7 = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSS7.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        bool flag18 = HammerDataProvider.CheckAssesmentAddList(ss.EmployeeID, Date.Date, check2, ss.Level);
                        if (flag18)
                        {
                            EmployeeModel employeeModel21 = new EmployeeModel();
                            if (list.Find((EmployeeModel f) => f.EmployeeID == ss.EmployeeID) == null)
                            {
                                list.Add(ss);
                            }
                        }
                    }
                    goto IL_13F4;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == false)
            {
                EmployeeModel employeeModel22 = new EmployeeModel();
                employeeModel22.EmployeeID = dMSSalesForce.EmployeeID;
                employeeModel22.EmployeeName = dMSSalesForce.EmployeeName;
                employeeModel22.Level = dMSSFHierarchy.LevelName;
                bool flag19 = HammerDataProvider.CheckAssesmentAddList(employeeModel22.EmployeeID, Date.Date, check2, employeeModel22.Level);
                if (flag19)
                {
                    list.Add(employeeModel22);
                }
            }
            else
            {
                if (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true)
                {
                    EmployeeModel employeeModel23 = new EmployeeModel();
                    employeeModel23.EmployeeID = dMSSalesForce.EmployeeID;
                    employeeModel23.EmployeeName = dMSSalesForce.EmployeeName;
                    employeeModel23.Level = dMSSFHierarchy.LevelName;
                    bool flag20 = HammerDataProvider.CheckAssesmentAddList(employeeModel23.EmployeeID, Date.Date, check2, employeeModel23.Level);
                    if (flag20)
                    {
                        list.Add(employeeModel23);
                    }
                }
            }
        IL_13F4:
            list = (
                from o in list
                orderby o.EmployeeID
                select o).ToList<EmployeeModel>();
            return list;
        }
        public static bool CheckAssesmentAddList(string employeeID, System.DateTime dateass, bool check, string Level)
        {
            bool result = false;
            if (check)
            {
                if (Level == "SS")
                {
                    SMTrainingAssessmentHeader sMTrainingAssessmentHeader = (
                        from Header in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                        where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                        select Header).FirstOrDefault<SMTrainingAssessmentHeader>();
                    if (sMTrainingAssessmentHeader != null)
                    {
                        result = true;
                    }
                    else
                    {
                        TrainingAssessmentHeader trainingAssessmentHeader = (
                            from Header in HammerDataProvider.Context.TrainingAssessmentHeaders
                            where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                            select Header).FirstOrDefault<TrainingAssessmentHeader>();
                        result = (trainingAssessmentHeader != null);
                    }
                }
                else
                {
                    TrainingAssessmentHeader trainingAssessmentHeader2 = (
                        from Header in HammerDataProvider.Context.TrainingAssessmentHeaders
                        where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                        select Header).FirstOrDefault<TrainingAssessmentHeader>();
                    if (trainingAssessmentHeader2 != null)
                    {
                        result = true;
                    }
                    else
                    {
                        SMTrainingAssessmentHeader sMTrainingAssessmentHeader2 = (
                            from Header in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                            where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                            select Header).FirstOrDefault<SMTrainingAssessmentHeader>();
                        result = (sMTrainingAssessmentHeader2 != null);
                    }
                }
            }
            else
            {
                if (Level != "SM")
                {
                    NoTrainingAssessment noTrainingAssessment = (
                        from Header in HammerDataProvider.Context.NoTrainingAssessments
                        where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                        select Header).FirstOrDefault<NoTrainingAssessment>();
                    result = (noTrainingAssessment != null);
                }
            }
            return result;
        }
        public static bool ViewAssessmentGetLevel(string userLogin)
        {
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == userLogin && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            return dMSSFHierarchy != null && (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true);
        }
        public static SMAssessmentModel ViewAssessmentSM(string employeeID, System.DateTime dateass, int UniqueID)
        {
            SMAssessmentModel sMAssessmentModel = new SMAssessmentModel();
            sMAssessmentModel.GetSMAssessmentModel();
            SMTrainingAssessmentHeader sMTrainingAssessmentHeader = (
                from Header in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                where Header.UserID.Trim() == employeeID.Trim()
                && Header.AssessmentDate.Date == dateass.Date && Header.UniqueID == (int?)UniqueID
                && Header.Released == true
                select Header).FirstOrDefault<SMTrainingAssessmentHeader>();
            if (sMTrainingAssessmentHeader != null)
            {
                sMAssessmentModel.Header.AssessmentDate = sMTrainingAssessmentHeader.AssessmentDate;
                sMAssessmentModel.Header.UniqueID = sMTrainingAssessmentHeader.UniqueID;
                sMAssessmentModel.Header.AssessmentFor = sMTrainingAssessmentHeader.AssessmentFor;
                sMAssessmentModel.Header.DistributorID = sMTrainingAssessmentHeader.DistributorID;
                //int company = (int)System.Convert.ToInt16(sMTrainingAssessmentHeader.DistributorID);
                //string text = (
                //    from dist in HammerDataProvider.Context.Distributors
                //    where dist.CompanyID == company
                //    select dist.CompanyName).SingleOrDefault<string>();
                //sMAssessmentModel.Header.DistributorID = ((sMTrainingAssessmentHeader == null) ? string.Empty : text);
                sMAssessmentModel.Header.UserID = employeeID;
                if (!string.IsNullOrEmpty(sMAssessmentModel.Header.DistributorID))
                {
                    sMAssessmentModel.Header.AreaID = HammerDataProvider.GetAreaByDistributor(System.Convert.ToInt32(sMTrainingAssessmentHeader.DistributorID));
                }
                sMAssessmentModel.Header.SalesObjective = sMTrainingAssessmentHeader.SalesObjective;
                sMAssessmentModel.Header.TraningObjective = sMTrainingAssessmentHeader.TraningObjective;
                System.Collections.Generic.List<SMTrainingAssessmentsDetail> list = (
                    from Header in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                    where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date
                    && Header.UniqueID == UniqueID
                     && ((from critDw in HammerDataProvider.Context.SMCriterias
                          where critDw.Active == true
                          select critDw.CriteriaID).ToList()).Contains(Header.CriteriaID)
                    select Header).ToList<SMTrainingAssessmentsDetail>();
                foreach (SMTrainingAssessmentsDetail item in list)
                {
                    if (item.CriteriaID <= 3)
                    {
                        sMAssessmentModel.DailyWorks.RemoveAll((SMTrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                        sMAssessmentModel.DailyWorks.Add(item);
                    }
                    else
                    {
                        if (item.CriteriaID > 3 && item.CriteriaID <= 9)
                        {
                            if (item.CriteriaID == 5)
                            { }
                            else if (item.CriteriaID == 6)
                            { }
                            else
                            {
                                sMAssessmentModel.Tools.RemoveAll((SMTrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                                sMAssessmentModel.Tools.Add(item);
                            }
                        }
                        else
                        {
                            if (item.CriteriaID != 16)
                            {
                                sMAssessmentModel.Steps.RemoveAll((SMTrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                                sMAssessmentModel.Steps.Add(item);
                            }
                        }
                    }
                }
                sMAssessmentModel.Steps = sMAssessmentModel.Steps.OrderBy(a => a.CriteriaID).ToList();
                sMAssessmentModel.Header.Comment = sMTrainingAssessmentHeader.Comment;
                sMAssessmentModel.Header.NextTrainingObjective = sMTrainingAssessmentHeader.NextTrainingObjective;
            }
            return sMAssessmentModel;
        }
        public static SMAssessmentModel ViewAssessmentSM(string employeeID, System.DateTime dateass)
        {
            SMAssessmentModel sMAssessmentModel = new SMAssessmentModel();
            sMAssessmentModel.GetSMAssessmentModel();
            SMTrainingAssessmentHeader sMTrainingAssessmentHeader = (
                from Header in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                select Header).FirstOrDefault<SMTrainingAssessmentHeader>();
            if (sMTrainingAssessmentHeader != null)
            {
                sMAssessmentModel.Header.AssessmentFor = sMTrainingAssessmentHeader.AssessmentFor;
                int company = (int)System.Convert.ToInt16(sMTrainingAssessmentHeader.DistributorID);
                string text = (
                    from dist in HammerDataProvider.Context.Distributors
                    where dist.CompanyID == company
                    select dist.CompanyName).SingleOrDefault<string>();
                sMAssessmentModel.Header.DistributorID = ((sMTrainingAssessmentHeader == null) ? string.Empty : text);
                sMAssessmentModel.Header.UserID = employeeID;
                if (!string.IsNullOrEmpty(sMAssessmentModel.Header.DistributorID))
                {
                    sMAssessmentModel.Header.AreaID = HammerDataProvider.GetAreaByDistributor(System.Convert.ToInt32(sMTrainingAssessmentHeader.DistributorID));
                }
                sMAssessmentModel.Header.SalesObjective = sMTrainingAssessmentHeader.SalesObjective;
                sMAssessmentModel.Header.TraningObjective = sMTrainingAssessmentHeader.TraningObjective;
                System.Collections.Generic.List<SMTrainingAssessmentsDetail> list = (
                    from Header in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                    where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date
                     && ((from critDw in HammerDataProvider.Context.SMCriterias
                          where critDw.Active == true
                          select critDw.CriteriaID).ToList()).Contains(Header.CriteriaID)
                    select Header).ToList<SMTrainingAssessmentsDetail>();
                foreach (SMTrainingAssessmentsDetail item in list)
                {
                    if (item.CriteriaID <= 3)
                    {
                        sMAssessmentModel.DailyWorks.RemoveAll((SMTrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                        sMAssessmentModel.DailyWorks.Add(item);
                    }
                    else
                    {
                        if (item.CriteriaID > 3 && item.CriteriaID <= 9)
                        {
                            sMAssessmentModel.Tools.RemoveAll((SMTrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                            sMAssessmentModel.Tools.Add(item);
                        }
                        else
                        {
                            sMAssessmentModel.Steps.RemoveAll((SMTrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                            sMAssessmentModel.Steps.Add(item);
                        }
                    }
                }
                sMAssessmentModel.Steps = sMAssessmentModel.Steps.OrderBy(a => a.CriteriaID).ToList();
                sMAssessmentModel.Header.Comment = sMTrainingAssessmentHeader.Comment;
                sMAssessmentModel.Header.NextTrainingObjective = sMTrainingAssessmentHeader.NextTrainingObjective;
            }
            return sMAssessmentModel;
        }
        public static AssessmentModel ViewAssessment(string employeeID, System.DateTime dateass, int UniqueID)
        {
            AssessmentModel assessmentModel = new AssessmentModel();
            TrainingAssessmentHeader trainingAssessmentHeader = (
                from Header in HammerDataProvider.Context.TrainingAssessmentHeaders
                where Header.UserID.Trim() == employeeID.Trim()
                && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                && Header.UniqueID == UniqueID
                select Header).FirstOrDefault<TrainingAssessmentHeader>();
            if (trainingAssessmentHeader != null)
            {
                assessmentModel.Header.UserID = trainingAssessmentHeader.UserID;
                assessmentModel.Header.UniqueID = trainingAssessmentHeader.UniqueID;
                assessmentModel.Header.SM = trainingAssessmentHeader.SM;
                assessmentModel.Header.AssessmentDate = trainingAssessmentHeader.AssessmentDate;
                int company = (int)System.Convert.ToInt16(trainingAssessmentHeader.DistributorID);
                string text = (
                    from dist in HammerDataProvider.Context.Distributors
                    where dist.CompanyID == company
                    select dist.CompanyName).SingleOrDefault<string>();
                assessmentModel.Header.DistributorID = ((trainingAssessmentHeader == null) ? string.Empty : company.ToString());
                assessmentModel.Header.AssessmentFor = trainingAssessmentHeader.AssessmentFor;
                if (!string.IsNullOrEmpty(assessmentModel.Header.DistributorID))
                {
                    assessmentModel.Header.AreaID = HammerDataProvider.GetAreaByDistributor(System.Convert.ToInt32(trainingAssessmentHeader.DistributorID));
                }
                assessmentModel.Header.SRUnderstood = trainingAssessmentHeader.SRUnderstood;
                assessmentModel.Header.OutletUnderstood = trainingAssessmentHeader.OutletUnderstood;
                assessmentModel.Header.Training = trainingAssessmentHeader.Training;
                assessmentModel.Header.PC = trainingAssessmentHeader.PC;
                assessmentModel.Header.LPPC = trainingAssessmentHeader.LPPC;
                assessmentModel.Header.Mark = trainingAssessmentHeader.Mark;
                assessmentModel.Header.ABCDComment = trainingAssessmentHeader.ABCDComment;
                assessmentModel.Header.ABCDNextTrainingObjective = trainingAssessmentHeader.ABCDNextTrainingObjective;
                assessmentModel.Header.Comment = trainingAssessmentHeader.Comment;
                assessmentModel.Header.NextTrainingObjective = trainingAssessmentHeader.NextTrainingObjective;
                System.Collections.Generic.List<TrainingAssessmentsDetail> list = (
                    from Header in HammerDataProvider.Context.TrainingAssessmentsDetails
                    where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date
                        && ((from critDw in HammerDataProvider.Context.Criterias
                             where critDw.Active == true
                             select critDw.CriteriaID).ToList()).Contains(Header.CriteriaID)
                    select Header).ToList<TrainingAssessmentsDetail>();
                foreach (TrainingAssessmentsDetail item in list)
                {
                    if (item.CriteriaID <= 4)
                    {
                        assessmentModel.TrainingProcess.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                        assessmentModel.TrainingProcess.Add(item);
                    }
                    else
                    {
                        if (item.CriteriaID >= 24 && item.CriteriaID <= 26)
                        {
                            assessmentModel.DailyWorks.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                            assessmentModel.DailyWorks.Add(item);
                        }
                        else
                        {
                            if (item.CriteriaID > 26 && item.CriteriaID <= 30)
                            {
                                assessmentModel.ToolsSM.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                                assessmentModel.ToolsSM.Add(item);
                            }
                            else
                            {
                                if (item.CriteriaID >= 20 && item.CriteriaID <= 26)
                                {
                                    assessmentModel.UpdateAndArchive.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                                    assessmentModel.UpdateAndArchive.Add(item);
                                }
                                else
                                {
                                    assessmentModel.Steps.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                                    assessmentModel.Steps.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            return assessmentModel;
        }
        public static AssessmentModel ViewAssessment(string employeeID, System.DateTime dateass)
        {
            AssessmentModel assessmentModel = new AssessmentModel();
            TrainingAssessmentHeader trainingAssessmentHeader = (
                from Header in HammerDataProvider.Context.TrainingAssessmentHeaders
                where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                select Header).FirstOrDefault<TrainingAssessmentHeader>();
            if (trainingAssessmentHeader != null)
            {
                assessmentModel.Header.UserID = trainingAssessmentHeader.UserID;
                assessmentModel.Header.AssessmentDate = trainingAssessmentHeader.AssessmentDate;
                int company = (int)System.Convert.ToInt16(trainingAssessmentHeader.DistributorID);
                string text = (
                    from dist in HammerDataProvider.Context.Distributors
                    where dist.CompanyID == company
                    select dist.CompanyName).SingleOrDefault<string>();
                assessmentModel.Header.DistributorID = ((trainingAssessmentHeader == null) ? string.Empty : text);
                assessmentModel.Header.AssessmentFor = trainingAssessmentHeader.AssessmentFor;
                if (!string.IsNullOrEmpty(assessmentModel.Header.DistributorID))
                {
                    assessmentModel.Header.AreaID = HammerDataProvider.GetAreaByDistributor(System.Convert.ToInt32(trainingAssessmentHeader.DistributorID));
                }
                assessmentModel.Header.SRUnderstood = trainingAssessmentHeader.SRUnderstood;
                assessmentModel.Header.OutletUnderstood = trainingAssessmentHeader.OutletUnderstood;
                assessmentModel.Header.Training = trainingAssessmentHeader.Training;
                assessmentModel.Header.PC = trainingAssessmentHeader.PC;
                assessmentModel.Header.LPPC = trainingAssessmentHeader.LPPC;
                System.Collections.Generic.List<TrainingAssessmentsDetail> list = (
                    from Header in HammerDataProvider.Context.TrainingAssessmentsDetails
                    where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date
                     && ((from critDw in HammerDataProvider.Context.Criterias
                          where critDw.Active == true
                          select critDw.CriteriaID).ToList()).Contains(Header.CriteriaID)
                    select Header).ToList<TrainingAssessmentsDetail>();
                foreach (TrainingAssessmentsDetail item in list)
                {
                    if (item.CriteriaID <= 4)
                    {
                        assessmentModel.TrainingProcess.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                        assessmentModel.TrainingProcess.Add(item);
                    }
                    else
                    {
                        if (item.CriteriaID >= 24 && item.CriteriaID <= 26)
                        {
                            assessmentModel.DailyWorks.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                            assessmentModel.DailyWorks.Add(item);
                        }
                        else
                        {
                            if (item.CriteriaID > 26 && item.CriteriaID <= 30)
                            {
                                assessmentModel.ToolsSM.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                                assessmentModel.ToolsSM.Add(item);
                            }
                            else
                            {
                                assessmentModel.Steps.RemoveAll((TrainingAssessmentsDetail rm) => rm.CriteriaID == item.CriteriaID);
                                assessmentModel.Steps.Add(item);
                            }
                        }
                    }
                }
            }
            return assessmentModel;
        }
        public static NoAssessmentModel ViewNoAssessment(string employeeID, System.DateTime dateass, int UniqueID)
        {
            NoAssessmentModel noAssessmentModel = new NoAssessmentModel();
            NoTrainingAssessment noTrainingAssessment = (
                from Header in HammerDataProvider.Context.NoTrainingAssessments
                where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.UniqueID == (int?)UniqueID && Header.Released == true
                select Header).FirstOrDefault<NoTrainingAssessment>();
            if (noTrainingAssessment != null)
            {
                noAssessmentModel.Header.UniqueID = noTrainingAssessment.UniqueID;
                noAssessmentModel.Header.Results = noTrainingAssessment.Results;
                noAssessmentModel.Header.Works = noTrainingAssessment.Works;
                noAssessmentModel.Header.UserID = noTrainingAssessment.UserID;
            }
            return noAssessmentModel;
        }
        public static NoAssessmentModel ViewNoAssessment(string employeeID, System.DateTime dateass)
        {
            NoAssessmentModel noAssessmentModel = new NoAssessmentModel();
            NoTrainingAssessment noTrainingAssessment = (
                from Header in HammerDataProvider.Context.NoTrainingAssessments
                where Header.UserID.Trim() == employeeID.Trim() && Header.AssessmentDate.Date == dateass.Date && Header.Released == true
                select Header).FirstOrDefault<NoTrainingAssessment>();
            if (noTrainingAssessment != null)
            {
                noAssessmentModel.Header.Results = noTrainingAssessment.Results;
                noAssessmentModel.Header.Works = noTrainingAssessment.Works;
                noAssessmentModel.Header.UserID = noTrainingAssessment.UserID;
            }
            return noAssessmentModel;
        }
        #region AssesmentCapacity
        public static PeriodSetting GetPeriodSettingWithID(string ID)
        {
            return (
                 from item in HammerDataProvider.Context.PeriodSettings
                 where item.PeriodID.Trim() == ID.Trim()
                 select item into a
                 orderby a.CreatedDate
                 select a).FirstOrDefault();
        }
        public static List<PeriodSetting> GetListPeriodSettingWithID(string ID)
        {
            return (
                 from item in HammerDataProvider.Context.PeriodSettings
                 where item.PeriodID.Trim() == ID.Trim()
                 select item into a
                 orderby a.CreatedDate
                 select a).ToList();
        }
        public static List<PeriodSetting> GetPeriodSetting()
        {
            return (
                 from item in HammerDataProvider.Context.PeriodSettings
                 select item into a
                 orderby a.CreatedDate
                 select a).ToList<PeriodSetting>().ToList<PeriodSetting>();
        }
        public static List<ExcelDetailsAssessmentCapacity> AssessmentCapacityConvertExcel(List<DetailsAssessmentCapacity> input)
        {
            List<ExcelDetailsAssessmentCapacity> rs = new List<ExcelDetailsAssessmentCapacity>();
            foreach (var item in input)
            {
                ExcelDetailsAssessmentCapacity ins = new ExcelDetailsAssessmentCapacity();
                ins.Mark1 = item.Mark1;
                ins.Mark2 = item.Mark2;
                ins.Mark3 = item.Mark3;
                ins.Mark4 = item.Mark4;
                ins.Mark5 = item.Mark5;
                ins.Mark6 = item.Mark6;
                ins.Mark7 = item.Mark7;
                ins.Mark8 = item.Mark8;
                ins.Mark9 = item.Mark9;
                ins.Mark10 = item.Mark10;
                ins.AffectOthers = item.AffectOthers;
                ins.BreakthroughImprovement = item.BreakthroughImprovement;
                ins.CommitmentCooperation = item.CommitmentCooperation; ;
                ins.CustomerOrientation = item.CustomerOrientation;
                ins.EmployeeID = item.EmployeeID;
                ins.InitiativeSpeed = item.InitiativeSpeed;
                ins.Integrity = item.Integrity;
                ins.MentoringDeveloping = item.MentoringDeveloping;
                ins.PassionateSuccess = item.PassionateSuccess;
                ins.PeriodID = item.PeriodID;
                ins.StrategicThinking = item.StrategicThinking;
                ins.TeamLeader = item.TeamLeader;
                rs.Add(ins);
            }
            return rs;

        }
        public static List<DetailsAssessmentCapacity> GetDetailsAssessmentCapacityWithPeriod(string Period, string User)
        {
            List<DetailsAssessmentCapacity> rs = new List<DetailsAssessmentCapacity>();
            var listnv = HammerDataProvider.GetSubordinateNoDuplicate(User);
            List<string> listWhere = new List<string>();
            foreach (var item in listnv)
            {
                listWhere.Add(item.EmployeeID);
            }
            if (Period == null)
                Period = "";
            List<AssessmentCapacity> source =
               (from setting in HammerDataProvider.Context.AssessmentCapacities
                where setting.PeriodID.Trim() == Period.Trim() && listWhere.Contains(setting.EmployeeID)
                select setting).ToList<AssessmentCapacity>();
            if (source.Count() <= 0)
                return rs;
            int No = 1;
            foreach (var item in source)
            {
                DetailsAssessmentCapacity ins = new DetailsAssessmentCapacity();
                ins.No = No;
                ins.Mark1 = item.Mark1;
                ins.Mark2 = item.Mark2;
                ins.Mark3 = item.Mark3;
                ins.Mark4 = item.Mark4;
                ins.Mark5 = item.Mark5;
                ins.Mark6 = item.Mark6;
                ins.Mark7 = item.Mark7;
                ins.Mark8 = item.Mark8;
                ins.Mark9 = item.Mark9;
                ins.Mark10 = item.Mark10;
                ins.AffectOthers = item.AffectOthers;
                ins.BreakthroughImprovement = item.BreakthroughImprovement;
                ins.CommitmentCooperation = item.CommitmentCooperation;
                ins.CreatedDate = item.CreatedDate;
                ins.CreatedUser = item.CreatedUser;
                ins.CustomerOrientation = item.CustomerOrientation;
                ins.EmployeeID = item.EmployeeID;
                ins.EmployeeName = item.EmployeeName;
                ins.InitiativeSpeed = item.InitiativeSpeed;
                ins.Integrity = item.Integrity;
                ins.MentoringDeveloping = item.MentoringDeveloping;
                ins.PassionateSuccess = item.PassionateSuccess;
                ins.PeriodID = item.PeriodID;
                ins.SFLevel = item.SFLevel;
                ins.StrategicThinking = item.StrategicThinking;
                ins.TeamLeader = item.TeamLeader;
                ins.UpdateDate = item.UpdateDate;
                ins.UpdateUser = item.UpdateUser;
                ins.IsDatabase = true;
                rs.Add(ins);
                No++;

            }
            return rs;

        }
        public static List<DetailsAssessmentCapacity> GetDetailsAssessmentCapacity()
        {
            List<DetailsAssessmentCapacity> rs = new List<DetailsAssessmentCapacity>();
            var source =
                from setting in HammerDataProvider.Context.AssessmentCapacities
                orderby setting.UpdateDate descending
                select setting;
            int No = 1;
            foreach (var item in source)
            {
                DetailsAssessmentCapacity ins = new DetailsAssessmentCapacity();
                ins.No = No;
                ins.Mark1 = item.Mark1;
                ins.Mark2 = item.Mark2;
                ins.Mark3 = item.Mark3;
                ins.Mark4 = item.Mark4;
                ins.Mark5 = item.Mark5;
                ins.Mark6 = item.Mark6;
                ins.Mark7 = item.Mark7;
                ins.Mark8 = item.Mark8;
                ins.Mark9 = item.Mark9;
                ins.Mark10 = item.Mark10;
                ins.AffectOthers = item.AffectOthers;
                ins.BreakthroughImprovement = item.BreakthroughImprovement;
                ins.CommitmentCooperation = item.CommitmentCooperation;
                ins.CreatedDate = item.CreatedDate;
                ins.CreatedUser = item.CreatedUser;
                ins.CustomerOrientation = item.CustomerOrientation;
                ins.EmployeeID = item.EmployeeID;
                ins.EmployeeName = item.EmployeeName;
                ins.InitiativeSpeed = item.InitiativeSpeed;
                ins.Integrity = item.Integrity;
                ins.MentoringDeveloping = item.MentoringDeveloping;
                ins.PassionateSuccess = item.PassionateSuccess;
                ins.PeriodID = item.PeriodID;
                ins.SFLevel = item.SFLevel;
                ins.StrategicThinking = item.StrategicThinking;
                ins.TeamLeader = item.TeamLeader;
                ins.UpdateDate = item.UpdateDate;
                ins.UpdateUser = item.UpdateUser;
                ins.IsDatabase = true;
                rs.Add(ins);
                No++;

            }
            return rs;

        }
        public static List<DMSSFHierarchy> GetDMSSFHierarchy()
        {
            return
               (from setting in HammerDataProvider.Context.DMSSFHierarchies
                where setting.IsSalesForce == true
                orderby setting.CreatedDateTime descending
                select setting).ToList();
        }
        public static void InsertAssessmentCapacity(DetailsAssessmentCapacity item)
        {
            AssessmentCapacity find = (from ac in HammerDataProvider.Context.AssessmentCapacities
                                       where ac.EmployeeID.Trim() == item.EmployeeID.Trim()
                                       && ac.PeriodID.Trim() == item.PeriodID.Trim()
                                       select ac).FirstOrDefault();
            if (find == null)
            {
                AssessmentCapacity ins = new AssessmentCapacity();
                ins.Mark1 = item.Mark1;
                ins.Mark2 = item.Mark2;
                ins.Mark3 = item.Mark3;
                ins.Mark4 = item.Mark4;
                ins.Mark5 = item.Mark5;
                ins.Mark6 = item.Mark6;
                ins.Mark7 = item.Mark7;
                ins.Mark8 = item.Mark8;
                ins.Mark9 = item.Mark9;
                ins.Mark10 = item.Mark10;
                ins.AffectOthers = item.AffectOthers;
                ins.BreakthroughImprovement = item.BreakthroughImprovement;
                ins.CommitmentCooperation = item.CommitmentCooperation;
                ins.CreatedDate = item.CreatedDate;
                ins.CreatedUser = item.CreatedUser;
                ins.CustomerOrientation = item.CustomerOrientation;
                ins.EmployeeID = item.EmployeeID;
                ins.EmployeeName = item.EmployeeName;
                ins.InitiativeSpeed = item.InitiativeSpeed;
                ins.Integrity = item.Integrity;
                ins.MentoringDeveloping = item.MentoringDeveloping;
                ins.PassionateSuccess = item.PassionateSuccess;
                ins.PeriodID = item.PeriodID;
                ins.SFLevel = item.SFLevel;
                ins.StrategicThinking = item.StrategicThinking;
                ins.TeamLeader = item.TeamLeader;
                ins.UpdateDate = item.UpdateDate;
                ins.UpdateUser = item.UpdateUser;
                Context.AssessmentCapacities.InsertOnSubmit(ins);
                Context.SubmitChanges();
            }
            else
            {
                find.Mark1 = item.Mark1;
                find.Mark2 = item.Mark2;
                find.Mark3 = item.Mark3;
                find.Mark4 = item.Mark4;
                find.Mark5 = item.Mark5;
                find.Mark6 = item.Mark6;
                find.Mark7 = item.Mark7;
                find.Mark8 = item.Mark8;
                find.Mark9 = item.Mark9;
                find.Mark10 = item.Mark10;
                find.AffectOthers = item.AffectOthers;
                find.BreakthroughImprovement = item.BreakthroughImprovement;
                find.CommitmentCooperation = item.CommitmentCooperation;
                find.CreatedDate = item.CreatedDate;
                find.CreatedUser = item.CreatedUser;
                find.CustomerOrientation = item.CustomerOrientation;
                find.EmployeeName = item.EmployeeName;
                find.InitiativeSpeed = item.InitiativeSpeed;
                find.Integrity = item.Integrity;
                find.MentoringDeveloping = item.MentoringDeveloping;
                find.PassionateSuccess = item.PassionateSuccess;
                find.SFLevel = item.SFLevel;
                find.StrategicThinking = item.StrategicThinking;
                find.TeamLeader = item.TeamLeader;
                find.UpdateDate = item.UpdateDate;
                find.UpdateUser = item.UpdateUser;
                Context.SubmitChanges();
            }
        }
        public static bool AssessmentCapacityGetLevel(string userLogin)
        {
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == userLogin && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            return dMSSFHierarchy != null && (dMSSFHierarchy.TerritoryType == 'D' && dMSSFHierarchy.IsSalesForce == true);
        }
        public static DMSSFHierarchy AssessmentCapacityGetDMSSFHierarchyAllUser(string user)
        {
            return (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim().ToUpper() == user.ToUpper()
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
        }
        public static List<ExcelDetailsAssessmentCapacity> AssessmentCapacityUploadFromExcel(MemoryStream stream, string employeeID)
        {
            try
            {
                List<ExcelDetailsAssessmentCapacity> model = new List<ExcelDetailsAssessmentCapacity>();
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    //Check correct template
                    ExcelWorksheet ws = package.Workbook.Worksheets["Assesment-Capacity"];
                    if (ws != null)
                    {
                        for (int i = 2; i <= ws.Dimension.End.Row; i++)
                        {
                            ExcelDetailsAssessmentCapacity ins = new ExcelDetailsAssessmentCapacity();
                            try
                            {
                                ins.PeriodID = ws.Cells[i, 1].Value.ToString();
                                PeriodSetting line = GetPeriodSettingWithID(ins.PeriodID);
                                if (line == null)
                                {
                                    ins.Note = "Kỳ không tồn tại trên hệ thống";
                                }
                                ins.EmployeeID = ws.Cells[i, 2].Value.ToString();
                                DMSSalesForce lineEm = GetSalesforceById(ins.EmployeeID);
                                if (lineEm == null)
                                {
                                    ins.Note = "Nhân viên không tồn tại trên hệ thống";
                                }
                                List<EmployeeModel> listEmdown = HammerDataProvider.GetSubordinateNoDuplicate(employeeID);
                                bool kq = listEmdown.Exists(x => x.EmployeeID == ins.EmployeeID);
                                if (kq == false)
                                {
                                    ins.Note = "Nhân viên không thuộc quyền quản lý";
                                }

                            }
                            catch (System.Exception)
                            {
                                ins.PeriodID = "";
                                ins.EmployeeID = "";
                                ins.Note = "Kỳ - nhân viên rỗng hoặc số dòng trống chưa được xóa hoàn toàn.";
                            }
                            try
                            {
                                ins.PassionateSuccess = Convert.ToInt32(ws.Cells[i, 3].Value.ToString());
                                ins.Mark1 = ws.Cells[i, 4].Value.ToString();

                                ins.BreakthroughImprovement = Convert.ToInt32(ws.Cells[i, 5].Value.ToString());
                                ins.Mark2 = ws.Cells[i, 6].Value.ToString(); ;

                                ins.InitiativeSpeed = Convert.ToInt32(ws.Cells[i, 7].Value.ToString());
                                ins.Mark3 = ws.Cells[i, 8].Value.ToString();

                                ins.CustomerOrientation = Convert.ToInt32(ws.Cells[i, 9].Value.ToString());
                                ins.Mark4 = ws.Cells[i, 10].Value.ToString();

                                ins.CommitmentCooperation = Convert.ToInt32(ws.Cells[i, 11].Value.ToString());
                                ins.Mark5 = ws.Cells[i, 12].Value.ToString();

                                ins.Integrity = Convert.ToInt32(ws.Cells[i, 13].Value.ToString());
                                ins.Mark6 = ws.Cells[i, 14].Value.ToString();

                                ins.MentoringDeveloping = Convert.ToInt32(ws.Cells[i, 15].Value.ToString());
                                ins.Mark7 = ws.Cells[i, 16].Value.ToString();

                                ins.AffectOthers = Convert.ToInt32(ws.Cells[i, 17].Value.ToString());
                                ins.Mark8 = ws.Cells[i, 18].Value.ToString();

                                ins.TeamLeader = Convert.ToInt32(ws.Cells[i, 19].Value.ToString());
                                ins.Mark9 = ws.Cells[i, 20].Value.ToString();

                                ins.StrategicThinking = Convert.ToInt32(ws.Cells[i, 21].Value.ToString());
                                ins.Mark10 = ws.Cells[i, 22].Value.ToString();
                            }
                            catch (System.Exception)
                            {
                                ins.Note = "Điểm không hợp lệ";
                            }
                            model.Add(ins);
                        }
                    }
                    package.Dispose();

                }
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return null;
            }
        }
        #endregion
        #region PrepareNew
        public static char getResult(string year, int week, string EmWW, string Em, DateTime? startdate)
        {
            DMSDistributorRouteAssignment route = GetRouteWithSMandSS(Em, EmWW);
            if (route == null)
            {
                return '0';
            }
            else
            {
                DMSHamResult rs = (from Header in HammerDataProvider.Context.DMSHamResults
                                   where Header.RouteID.Trim() == route.RouteCD.Trim() && Header.Year == year
                                   && Header.Week == week
                                   select Header).FirstOrDefault<DMSHamResult>();
                if (rs != null)
                {
                    //2015-10-02 
                    if (rs.IsNew == true)
                    {
                        return 'B';
                    }
                    else
                    {
                        return rs.RefResult;
                    }
                    //SystemSetting SMM = GetSystemSetting("SMM");
                    //if (startdate != null)
                    //{
                    //    if (rs.CreatedDateTime.Value.Date <= startdate.Value.AddDays(SMM.Number))
                    //    {
                    //        return 'B';
                    //    }
                    //    else
                    //    {
                    //        return rs.RefResult;
                    //    }
                    //}
                    //else
                    //{
                    //    return rs.RefResult;
                    //}
                }
                else
                {
                    return '0';
                }
            }
        }
        public static ConfigSystem getConfigSystem(string codeCD)
        {
            return (
               from Header in HammerDataProvider.Context.ConfigSystems
               where Header.CodeCD == codeCD
               select Header).FirstOrDefault<ConfigSystem>();
        }
        public static DMSHamResult getResultRoute(string year, int week, string SS,string RouteID)
        {
            return (
               from Header in HammerDataProvider.Context.DMSHamResults
               where Header.Year == year
               && Header.Week == week && Header.SS == SS && Header.RouteID == RouteID
               select Header).FirstOrDefault<DMSHamResult>();
        }
        public static DMSHamResult getResultKem(string year, int week, string Em)
        {
            return (
               from Header in HammerDataProvider.Context.DMSHamResults
               where Header.Year == year
               && Header.Week == week && Header.RefResult == 'A'
               select Header).FirstOrDefault<DMSHamResult>();
        }
        public static DMSHamResult getResult(string year, int week, string EmWW, string Em)
        {
            DMSDistributorRouteAssignment route = GetRouteWithSMandSS(Em, EmWW);
            if (route == null)
                return null;
            return (
               from Header in HammerDataProvider.Context.DMSHamResults
               where Header.RouteID.Trim() == route.RouteCD.Trim() && Header.Year == year
               && Header.Week == week
               select Header).FirstOrDefault<DMSHamResult>();
        }
        public static DMSHamResult getPercentHLFrom(string ReNbr)
        {
            return (
               from Header in HammerDataProvider.Context.DMSHamResults
               where Header.RefNbr == ReNbr
               select Header).FirstOrDefault<DMSHamResult>();
        }
        public static DMSHamResultsASM getResultASM(string year, int week, string EmWW, string Em)
        {
            return (
               from Header in HammerDataProvider.Context.DMSHamResultsASMs
               where Header.ASM.Trim() == EmWW.Trim() && Header.Year == year
               && Header.RSM.Trim().ToUpper() == Em.Trim().ToUpper()
               && Header.Week == week
               select Header).FirstOrDefault<DMSHamResultsASM>();
        }
        public static DMSHamResultsASM getResultASM(string year, int week, string Em)
        {
            return (
               from Header in HammerDataProvider.Context.DMSHamResultsASMs
               where Header.ASM.Trim() == Em.Trim() && Header.Year == year             
               && Header.Week == week
               select Header).FirstOrDefault<DMSHamResultsASM>();
        }
        //public static DMSHamResultsSS getResultSS(string year, int week, string EmWW, string Em)
        //{
        //    return (
        //       from Header in HammerDataProvider.Context.DMSHamResultsSSes
        //       where Header.ASM.Trim() == EmWW.Trim() && Header.Year == year
        //       && Header.SS.Trim().ToUpper() == Em.Trim().ToUpper()
        //       && Header.Week == week
        //       select Header).FirstOrDefault<DMSHamResultsSS>();
        //}
        public static char getResultCharSS(string year, int week, string EmWW, string Em)
        {
            DMSHamResultsSS rs = (
                      from Header in HammerDataProvider.Context.DMSHamResultsSSes
                      where Header.SS.Trim() == EmWW.Trim() && Header.Year == year
                      //&& Header.ASM.Trim().ToUpper() == Em.Trim().ToUpper()
                      && Header.Week == week
                      select Header).FirstOrDefault<DMSHamResultsSS>(); ;
            //DMSSalesForce sf = (
            //         from Header in Context.DMSSalesForces
            //         where Header.EmployeeID.Trim() == EmWW.Trim()
            //         select Header).FirstOrDefault<DMSSalesForce>(); ;
            if (rs != null)
            {
                //2015-10-02 bo dk SS new di lay ket qua tu SS de tinh luon
                if(rs.IsNew == true)
                {
                    return 'B';
                }
                else
                {
                    return rs.RefResult;
                }
                //SystemSetting SMM = GetSystemSetting("SMM");
                //if (sf != null && SMM != null)
                //{
                //    DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                //    if (PSASM != null)
                //    {
                //        if (PSASM.CreatedDateTime.Value.Date <= sf.CreatedDateTime.Value.AddDays(SMM.Number))
                //        {
                //            return 'B';
                //        }
                //        else
                //        {
                //            return rs.RefResult;
                //        }
                //    }
                //    else
                //    {
                //        return '0';
                //    }
                //}
                //else
                //{
                //    return rs.RefResult;
                //}
            }
            else
            {
                return '0';
            }

        }
        public static char getResultCharASM(string year, int week, string EmWW, string Em)
        {
            DMSHamResultsASM rs = (
                      from Header in HammerDataProvider.Context.DMSHamResultsASMs
                      where Header.ASM.Trim() == EmWW.Trim() && Header.Year == year
                      //&& Header.RSM.Trim().ToUpper() == Em.Trim().ToUpper()
                      && Header.Week == week
                      select Header).FirstOrDefault<DMSHamResultsASM>(); ;
            DMSSalesForce sf = (
                     from Header in Context.DMSSalesForces
                     where Header.EmployeeID.Trim() == EmWW.Trim()
                     select Header).FirstOrDefault<DMSSalesForce>(); ;
            if (rs != null)
            {
                SystemSetting SMM = GetSystemSetting("SMM");
                if (sf != null)
                {
                    DMSHamResult PSASM = HammerDataProvider.getPercentHLFrom(rs.RefNbr);
                    if (PSASM != null)
                    {
                        if (PSASM.CreatedDateTime.Value.Date <= sf.CreatedDateTime.Value.AddDays(SMM.Number))
                        {
                            return 'B';
                        }
                        else
                        {
                            return rs.RefResult;
                        }
                    }
                    else
                    {
                        return '0';
                    }
                }
                else
                {
                    return rs.RefResult;
                }
            }
            else
            {
                return '0';
            }

        }
        public static DMSHamResultsASM getResultASMKem(string year, int week, string Em)
        {
            return (
               from Header in HammerDataProvider.Context.DMSHamResultsASMs
               where Header.Year == year
               && Header.RSM.Trim().ToUpper() == Em.Trim().ToUpper()
               && Header.Week == week
               && Header.RefResult == 'A'
               select Header).FirstOrDefault<DMSHamResultsASM>();
        }
        public static DMSHamResultsSS getResultSSKem(string year, int week, string Em)
        {
            return (
               from Header in HammerDataProvider.Context.DMSHamResultsSSes
               where Header.Year == year
               && Header.ASM.Trim().ToUpper() == Em.Trim().ToUpper()
               && Header.Week == week
               && Header.RefResult == 'A'
               select Header).FirstOrDefault<DMSHamResultsSS>();
        }
        public static DMSHamResultsSS getResultSS(string year, int week, string EmWW, string Em)
        {
            return (
               from Header in HammerDataProvider.Context.DMSHamResultsSSes
               where Header.SS.Trim() == EmWW.Trim() && Header.Year == year
               //&& Header.ASM.Trim().ToUpper() == Em.Trim().ToUpper()
               && Header.Week == week
               select Header).FirstOrDefault<DMSHamResultsSS>();
        }
        //public static PrepareWW getPrepareWW(string year, int week, string EmWW)
        //{
        //    return (
        //       from Header in HammerDataProvider.Context.PrepareWW
        //       where Header.EmployeeID.Trim() == EmWW.Trim() && Header.Year == year
        //       && Header.Week == week
        //       select Header).FirstOrDefault<DMSHamResult>();
        //}
        public static bool CheckWWSMInGrid(int no, string SM, string User, DateTime date, List<DetailScheduleModel> list)
        {
            try
            {

                List<DateTime> currenWeek = CultureHelper.GetDateByCurrentWeek(date.Date, 7);
                List<DetailScheduleModel> listdetail = (
                   from app in list
                   where currenWeek.Contains(app.DateView.Date) &&
                   app.EmployeeIDG.ToUpper().Trim() == User.ToUpper().Trim()
                   && app.WorkWith == SM.Trim() && app.No != no
                   select app).ToList();
                if (listdetail.Count >= GetSystemSetting("WSM").Number)
                {
                    return true;
                }
                else
                {
                    List<Appointment> listapp = (
                   from app in Context.Appointments
                   where currenWeek.Contains(app.StartDate.Value.Date) &&
                   app.UserLogin.ToUpper().Trim() == User.ToUpper().Trim()
                   && app.Employees == SM.Trim()
                   select app).ToList();
                    if (listapp.Count >= GetSystemSetting("WSM").Number)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }
        public static bool CheckWWSMInWeek(string SM, string User, DateTime date)
        {
            try
            {
                List<DateTime> currenWeek = CultureHelper.GetDateByCurrentWeek(date.Date, 7);
                List<Appointment> listapp = (
                   from app in Context.Appointments
                   where currenWeek.Contains(app.StartDate.Value.Date) &&
                   app.UserLogin.ToUpper().Trim() == User.ToUpper().Trim()
                   && app.Employees == SM.Trim()
                   select app).ToList();
                if (listapp.Count >= GetSystemSetting("WSM").Number)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return false;
            }
        }
        public static List<NoneWWType> GetListNonWWType()
        {
            return (
                from item in HammerDataProvider.Context.NoneWWTypes
                select item into a
                select a).ToList<NoneWWType>().ToList<NoneWWType>();
        }
        public static NoneWWType GetNonWWType()
        {
            return (
                from item in HammerDataProvider.Context.NoneWWTypes
                select item into a
                select a).FirstOrDefault();
        }
        public static int? GetNumberPreapreWW(string year, int week, string em)
        {
            return (
                   from item in Context.PrepareWWs
                   where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                   select item).ToList().Count();
        }
        public static PrepareNonWW GetInPreapreWWNone(string year, int week, string em, string shift, string ID)
        {
            PrepareNonWW WWline = new PrepareNonWW();
            switch (ID)
            {
                case "MoAM":
                    WWline = (
                 from item in Context.PrepareNonWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                 select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "TuAM":
                    WWline = (
                  from item in Context.PrepareNonWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                   && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                  select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "WeAM":
                    WWline = (
                  from item in Context.PrepareNonWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper().Trim() == em.ToUpper().Trim()
                  && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                  select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "ThAM":
                    WWline = (
                  from item in Context.PrepareNonWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                  select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "FrAM":
                    WWline = (
                  from item in Context.PrepareNonWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                  select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "SaAM":
                    WWline = (
                 from item in Context.PrepareNonWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                 select item).FirstOrDefault<PrepareNonWW>();
                    break;

                case "MoPM":
                    WWline = (
                  from item in Context.PrepareNonWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                  select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "TuPM":
                    WWline = (
                  from item in Context.PrepareNonWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                  select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "WePM":
                    WWline = (
                 from item in Context.PrepareNonWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                 select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "ThPM":
                    WWline = (
                  from item in Context.PrepareNonWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                   && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                  select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "FrPM":
                    WWline = (
                  from item in Context.PrepareNonWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                  select item).FirstOrDefault<PrepareNonWW>();
                    break;
                case "SaPM":
                    WWline = (
                 from item in Context.PrepareNonWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.Day == ID.Substring(0, 2) && item.Status == 'R' && item.Shift == shift
                 select item).FirstOrDefault<PrepareNonWW>();
                    break;
            }
            if (WWline != null)
                return WWline;
            else
                return null;
        }
        public static PrepareWW GetInPreapreWWAM(string year, int week, string em, bool AM, string ID)
        {
            PrepareWW WWline = new PrepareWW();
            switch (ID)
            {
                case "MoAM":
                    WWline = (
                 from item in Context.PrepareWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.MoAM == AM && item.Status == 'N'
                 select item).FirstOrDefault<PrepareWW>();
                    break;
                case "TuAM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.TuAM == AM && item.Status == 'N'
                select item).FirstOrDefault<PrepareWW>();
                    break;
                case "WeAM":
                    WWline = (
                 from item in Context.PrepareWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.WeAM == AM && item.Status == 'N'
                 select item).FirstOrDefault<PrepareWW>();
                    break;
                case "ThAM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.ThAM == AM && item.Status == 'N'
                select item).FirstOrDefault<PrepareWW>();
                    break;
                case "FrAM":
                    WWline = (
                  from item in Context.PrepareWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.FrAM == AM && item.Status == 'N'
                  select item).FirstOrDefault<PrepareWW>();
                    break;
                case "SaAM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.SaAM == AM && item.Status == 'N'
                select item).FirstOrDefault<PrepareWW>();
                    break;

                case "MoPM":
                    WWline = (
                 from item in Context.PrepareWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.MoPM == AM && item.Status == 'N'
                 select item).FirstOrDefault<PrepareWW>();
                    break;
                case "TuPM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.TuPM == AM && item.Status == 'N'
                select item).FirstOrDefault<PrepareWW>();
                    break;
                case "WePM":
                    WWline = (
                 from item in Context.PrepareWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.WePM == AM && item.Status == 'N'
                 select item).FirstOrDefault<PrepareWW>();
                    break;
                case "ThPM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.ThPM == AM && item.Status == 'N'
                select item).FirstOrDefault<PrepareWW>();
                    break;
                case "FrPM":
                    WWline = (
                  from item in Context.PrepareWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.FrPM == AM && item.Status == 'N'
                  select item).FirstOrDefault<PrepareWW>();
                    break;
                case "SaPM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.SaPM == AM && item.Status == 'N'
                select item).FirstOrDefault<PrepareWW>();
                    break;
            }
            if (WWline != null)
                return WWline;
            else
                return null;

        }
        public static PrepareWW GetInPreapreWWReleased(string year, int week, string em, bool AM, string ID)
        {
            PrepareWW WWline = new PrepareWW();
            switch (ID)
            {
                case "MoAM":
                    WWline = (
                 from item in Context.PrepareWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.MoAM == AM && item.Status == 'R'
                 select item).FirstOrDefault<PrepareWW>();
                    break;
                case "TuAM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.TuAM == AM && item.Status == 'R'
                select item).FirstOrDefault<PrepareWW>();
                    break;
                case "WeAM":
                    WWline = (
                 from item in Context.PrepareWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.WeAM == AM && item.Status == 'R'
                 select item).FirstOrDefault<PrepareWW>();
                    break;
                case "ThAM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.ThAM == AM && item.Status == 'R'
                select item).FirstOrDefault<PrepareWW>();
                    break;
                case "FrAM":
                    WWline = (
                  from item in Context.PrepareWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.FrAM == AM && item.Status == 'R'
                  select item).FirstOrDefault<PrepareWW>();
                    break;
                case "SaAM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.SaAM == AM && item.Status == 'R'
                select item).FirstOrDefault<PrepareWW>();
                    break;

                case "MoPM":
                    WWline = (
                 from item in Context.PrepareWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.MoPM == AM && item.Status == 'R'
                 select item).FirstOrDefault<PrepareWW>();
                    break;
                case "TuPM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.TuPM == AM && item.Status == 'R'
                select item).FirstOrDefault<PrepareWW>();
                    break;
                case "WePM":
                    WWline = (
                 from item in Context.PrepareWWs
                 where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                 && item.WePM == AM && item.Status == 'R'
                 select item).FirstOrDefault<PrepareWW>();
                    break;
                case "ThPM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.ThPM == AM && item.Status == 'R'
                select item).FirstOrDefault<PrepareWW>();
                    break;
                case "FrPM":
                    WWline = (
                  from item in Context.PrepareWWs
                  where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                  && item.FrPM == AM && item.Status == 'R'
                  select item).FirstOrDefault<PrepareWW>();
                    break;
                case "SaPM":
                    WWline = (
                from item in Context.PrepareWWs
                where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                && item.SaPM == AM && item.Status == 'R'
                select item).FirstOrDefault<PrepareWW>();
                    break;
            }
            if (WWline != null)
                return WWline;
            else
                return null;

        }
        public static PrepareWW GetInPreapreWW(string year, int week, string em, string ww)
        {
            PrepareWW WWline = new PrepareWW();
            if (EmployeeInRole(ww) == SystemRole.SalesForce)
            {
                WWline = (
                     from item in Context.PrepareWWs
                     where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                     && item.EmployeeWW.ToUpper() == ww.ToUpper()
                     select item).FirstOrDefault<PrepareWW>();
            }
            else
            {
                WWline = (
                      from item in Context.PrepareWWs
                      where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()
                      && item.RouteID.ToUpper() == ww.ToUpper()
                      select item).FirstOrDefault<PrepareWW>();
                
            }
            if (WWline != null)
            {
                return WWline;
            }
            else
            {
                return null;
            }
        }
        public static PrepareNonWW GetInPreapreNoWW(string year, int week, string em)
        {
            PrepareNonWW NoWWline = (
                   from item in Context.PrepareNonWWs
                   where item.Year == year && item.Week == week 
                   && item.EmployeeID.ToUpper() == em.ToUpper()
                   select item).FirstOrDefault<PrepareNonWW>();
            if (NoWWline != null)
            {
                return NoWWline;
            }
            else
            {
                return null;
            }
        }
        public static List<PrepareNonWW> GetListInPreapreNoWW(string year, int week, string em)
        {
            List<PrepareNonWW> NoWWline = (
                   from item in Context.PrepareNonWWs
                   where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == em.ToUpper()

                   select item).ToList();
            if (NoWWline != null)
            {

                return NoWWline;
            }
            else
            {
                return null;
            }
        }
        public static bool InsertUpdatePrepareWW(PrepareWW WW)
        {
            try
            {                
                //Update
                //PrepareWW WWline = (
                //from item in Context.PrepareWWs
                //where item.Year == WW.Year && item.Week == WW.Week && item.EmployeeID == WW.EmployeeID
                //&& item.EmployeeWW.ToUpper() == WW.EmployeeWW.ToUpper()
                //select item).FirstOrDefault<PrepareWW>();
                //Where Route la chinh
                PrepareWW WWline = new PrepareWW();
                if (EmployeeInRole(WW.EmployeeWW) == SystemRole.Salesman)
                {
                     WWline = (
                   from item in Context.PrepareWWs
                   where item.Year == WW.Year && item.Week == WW.Week && item.EmployeeID == WW.EmployeeID
                   && item.RouteID.ToUpper() == WW.RouteID.ToUpper()
                   select item).FirstOrDefault<PrepareWW>();
                }
                else
                {
                     WWline = (
                    from item in Context.PrepareWWs
                    where item.Year == WW.Year && item.Week == WW.Week && item.EmployeeID == WW.EmployeeID
                    && item.EmployeeWW.ToUpper() == WW.EmployeeWW.ToUpper()
                    select item).FirstOrDefault<PrepareWW>();
                }
                if (WWline != null)
                {
                    WWline.RefResult = WW.RefResult;
                    WWline.CreatedDateTime = DateTime.Now;
                    WWline.UserLogin = WW.UserLogin;
                    WWline.EmployeeID = WW.EmployeeID;
                    WWline.EmployeeWW = WW.EmployeeWW;
                    WWline.FrAM = WW.FrAM;
                    WWline.FrAMDes = WW.FrAMDes;
                    WWline.FrPM = WW.FrPM;
                    WWline.FrPMDes = WW.FrPMDes;
                    WWline.MoAM = WW.MoAM;
                    WWline.MoAMDes = WW.MoAMDes;
                    WWline.MoPM = WW.MoPM;
                    WWline.MoPMDes = WW.MoPMDes;
                    WWline.RefResult = WW.RefResult;
                    WWline.RouteID = WW.RouteID;
                    WWline.SaAM = WW.SaAM;
                    WWline.SaAMDes = WW.SaAMDes;
                    WWline.SaPM = WW.SaPM;
                    WWline.SaPMDes = WW.SaPMDes;
                    WWline.Status = WW.Status;
                    WWline.ThAM = WW.ThAM;
                    WWline.ThAMDes = WW.ThAMDes;
                    WWline.ThPM = WW.ThPM;
                    WWline.ThPMDes = WW.ThPMDes;
                    WWline.TuAM = WW.TuAM;
                    WWline.TuAMDes = WW.TuAMDes;
                    WWline.TuPM = WW.TuPM;
                    WWline.TuPMDes = WW.TuPMDes;
                    WWline.UserLogin = WW.UserLogin;
                    WWline.WeAM = WW.WeAM;
                    WWline.WeAMDes = WW.WeAMDes;
                    WWline.Week = WW.Week;
                    WWline.WePM = WW.WePM;
                    WWline.WePMDes = WW.WePMDes;
                    WWline.Year = WW.Year;
                    WWline.Status = WW.Status;                  
                }
                else                //insert
                {
                    WW.Status = 'N';
                    WW.CreatedDateTime = DateTime.Now;
                    WW.UserLogin = WW.UserLogin;                   
                    if (EmployeeInRole(WW.EmployeeWW) == SystemRole.Salesman)
                    {
                        if (GetRouteWithSMandSS(WW.EmployeeID, WW.EmployeeWW) != null)
                        {
                            WW.RouteID = GetRouteWithSMandSS(WW.EmployeeID, WW.EmployeeWW).RouteCD;
                        }
                        else
                        {
                            WW.RouteID = string.Empty;
                        }
                    }
                    else
                    {
                         if (GetRouteWithSF(WW.EmployeeID, WW.EmployeeWW) != null)
                        {
                            WW.RouteID = GetRouteWithSF(WW.EmployeeID, WW.EmployeeWW).RouteCD;
                        }
                        else
                        {
                            WW.RouteID = string.Empty;
                        }                                           
                    }
                    Context.PrepareWWs.InsertOnSubmit(WW);
                }
                Context.SubmitChanges();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }

        }
       
        public static bool UpdateStatusPrepareWW(PrepareWW WW)
        {
            try
            {
                //Update
                PrepareWW WWline = (
                from item in Context.PrepareWWs
                where item.Year == WW.Year && item.Week == WW.Week && item.EmployeeID == WW.EmployeeID
                && item.EmployeeWW.ToUpper() == WW.EmployeeWW.ToUpper()
                select item).FirstOrDefault<PrepareWW>();
                if (WWline != null)
                {                    
                    WWline.Status = WW.Status;
                    Context.SubmitChanges();
                }
               
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }

        }
        public static bool SendPrepareWW(PrepareWW WW)
        {
            try
            {
                //Update
                PrepareWW WWline = (
                from item in Context.PrepareWWs
                where item.Year == WW.Year && item.Week == WW.Week && item.EmployeeID == WW.EmployeeID
                && item.EmployeeWW.ToUpper() == WW.EmployeeWW.ToUpper()
                select item).FirstOrDefault<PrepareWW>();
                if (WWline != null)
                {
                    WWline.RefResult = WW.RefResult;
                    WWline.CreatedDateTime = DateTime.Now;
                    WWline.UserLogin = WW.UserLogin;
                    WWline.EmployeeID = WW.EmployeeID;
                    WWline.EmployeeWW = WW.EmployeeWW;
                    WWline.FrAM = WW.FrAM;
                    WWline.FrAMDes = WW.FrAMDes;
                    WWline.FrPM = WW.FrPM;
                    WWline.FrPMDes = WW.FrPMDes;
                    WWline.MoAM = WW.MoAM;
                    WWline.MoAMDes = WW.MoAMDes;
                    WWline.MoPM = WW.MoPM;
                    WWline.MoPMDes = WW.MoPMDes;
                    WWline.RefResult = WW.RefResult;
                    WWline.RouteID = WW.RouteID;
                    WWline.SaAM = WW.SaAM;
                    WWline.SaAMDes = WW.SaAMDes;
                    WWline.SaPM = WW.SaPM;
                    WWline.SaPMDes = WW.SaPMDes;
                    WWline.Status = WW.Status;
                    WWline.ThAM = WW.ThAM;
                    WWline.ThAMDes = WW.ThAMDes;
                    WWline.ThPM = WW.ThPM;
                    WWline.ThPMDes = WW.ThPMDes;
                    WWline.TuAM = WW.TuAM;
                    WWline.TuAMDes = WW.TuAMDes;
                    WWline.TuPM = WW.TuPM;
                    WWline.TuPMDes = WW.TuPMDes;
                    WWline.UserLogin = WW.UserLogin;
                    WWline.WeAM = WW.WeAM;
                    WWline.WeAMDes = WW.WeAMDes;
                    WWline.Week = WW.Week;
                    WWline.WePM = WW.WePM;
                    WWline.WePMDes = WW.WePMDes;
                    WWline.Year = WW.Year;
                    WWline.Status = 'R';
                }
                Context.SubmitChanges();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }

        }
        public static bool SendPrepareNoWW(PrepareNoWWwModel NoWW, string userlogin)
        {
            try
            {
                //Update
                PrepareNonWW NonWWline = (
                from item in Context.PrepareNonWWs
                where item.Year == NoWW.Year && item.Week == NoWW.Week && item.EmployeeID.ToUpper() == NoWW.gvEmployeeID.ToUpper()
                 && item.Day == NoWW.Day && item.Shift == NoWW.Shift
                select item).FirstOrDefault<PrepareNonWW>();
                if (NonWWline != null)
                {
                    NonWWline.Day = NoWW.Day;
                    NonWWline.Des = NoWW.Des;
                    NonWWline.Status = NonWWline.Status;
                    NonWWline.UserLogin = userlogin;
                    NonWWline.CreatedDateTime = DateTime.Now;
                    NonWWline.Status = 'R';
                }
                Context.SubmitChanges();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }

        }
        public static void ClearPrepareWW(string year, int week, string user)
        {
            List<PrepareWW> NonWWline = (
              from item in Context.PrepareWWs
              where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == user.ToUpper()

              select item).ToList<PrepareWW>();
            foreach (PrepareWW item in NonWWline)
            {
                Context.PrepareWWs.DeleteOnSubmit(item);
            }
            Context.SubmitChanges();

        }
        public static void ClearPrepareNoWW(string year,int week, string user)
        {
            List<PrepareNonWW> NonWWline = (
              from item in Context.PrepareNonWWs
              where item.Year == year && item.Week == week && item.EmployeeID.ToUpper() == user.ToUpper()
              
              select item).ToList<PrepareNonWW>();
            foreach (PrepareNonWW   item in NonWWline)
            {  
                Context.PrepareNonWWs.DeleteOnSubmit(item);
                Context.SubmitChanges();
            }
           

            
        }
        public static bool InsertUpdatePrepareNoWW(PrepareNoWWwModel NoWW, string userlogin)
        {
            try
            {                
                
                //Update
                PrepareNonWW NonWWline = (
                from item in Context.PrepareNonWWs
                where item.Year == NoWW.Year && item.Week == NoWW.Week && item.EmployeeID.ToUpper() == NoWW.gvEmployeeID.ToUpper()
                 && item.Day == NoWW.Day && item.Shift == NoWW.Shift
                select item).FirstOrDefault<PrepareNonWW>();
                if (NonWWline != null)
                {
                    NonWWline.Day = NoWW.Day;
                    NonWWline.RefWWType = NoWW.RefWWType;
                    NonWWline.Des = NoWW.Des;
                    NonWWline.Status = NonWWline.Status;
                    NonWWline.UserLogin = userlogin;
                    NonWWline.CreatedDateTime = DateTime.Now;

                }
                else                //insert
                {
                    PrepareNonWW ins = new PrepareNonWW();
                    ins.Day = NoWW.Day;
                    ins.Des = NoWW.Des;
                    ins.EmployeeID = NoWW.gvEmployeeID;
                    ins.RefWWType = NoWW.RefWWType;
                    ins.Week = NoWW.Week.Value;
                    ins.Year = NoWW.Year;
                    ins.Shift = NoWW.Shift;
                    ins.Status = 'N';
                    ins.UserLogin = userlogin;
                    ins.CreatedDateTime = DateTime.Now;
                    Context.PrepareNonWWs.InsertOnSubmit(ins);
                }
                Context.SubmitChanges();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }

        }
        public static bool UpdateStatusPrepareNoWW(PrepareNonWW NoWW)
        {
            try
            {
                //Update
                PrepareNonWW NonWWline = (
                from item in Context.PrepareNonWWs
                where item.Year == NoWW.Year && item.Week == NoWW.Week && item.EmployeeID.ToUpper() == NoWW.EmployeeID.ToUpper()
                 && item.Day == NoWW.Day && item.Shift == NoWW.Shift
                select item).FirstOrDefault<PrepareNonWW>();
                if (NonWWline != null)
                {
                    NonWWline.Status = NoWW.Status;
                    Context.SubmitChanges();
                    
                }
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }

        }
        public static Appointment PreparNewCheckApp(System.DateTime date, string userLogin, string shift)
        {
            return (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UserLogin == userLogin && carSchedule.StartDate.Value.Date == date.Date
                && carSchedule.EndDate.Value.Date == date.Date
                && carSchedule.ShiftID == shift
                && (carSchedule.Label == 10 || carSchedule.Label == 3)
                && carSchedule.IsDelete == false
                select carSchedule).FirstOrDefault<Appointment>();
        }
        public static Appointment PreparNewCheckApp(System.DateTime date, string userLogin, string shift,string EmWW)
        {
            return (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UserLogin == userLogin && carSchedule.StartDate.Value.Date == date.Date
                && carSchedule.EndDate.Value.Date == date.Date
                && carSchedule.ShiftID == shift && carSchedule.Employees.Trim().ToUpper() == EmWW.Trim().ToUpper()
                && (carSchedule.Label == 10 || carSchedule.Label == 3)
                && carSchedule.IsDelete == false
                select carSchedule).FirstOrDefault<Appointment>();
        }
        public static void UpdateNoTraning(NoTrainingAssessment ass)
        {
            if (ass == null)
            {
                return;
            }
            NoTrainingAssessment appointment = (
                from carSchedule in HammerDataProvider.Context.NoTrainingAssessments
                where carSchedule.UniqueID == ass.UniqueID
                select carSchedule).SingleOrDefault<NoTrainingAssessment>();
            if (appointment != null)
            {
                appointment.IsDelete = true;
                HammerDataProvider.Context.SubmitChanges();
            }
        }
        public static void UpdateSMTraning(SMTrainingAssessmentHeader ass)
        {
            if (ass == null)
            {
                return;
            }
            SMTrainingAssessmentHeader appointment = (
                from carSchedule in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                where carSchedule.UniqueID == ass.UniqueID
                select carSchedule).SingleOrDefault<SMTrainingAssessmentHeader>();
            if (appointment != null)
            {
                appointment.IsDelete = true;
            }
            SMTrainingAssessmentsDetail detail = (
                from carSchedule in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                where carSchedule.UniqueID == ass.UniqueID
                select carSchedule).SingleOrDefault<SMTrainingAssessmentsDetail>();
            if (detail != null)
            {
                detail.IsDelete = true;
            }
            HammerDataProvider.Context.SubmitChanges();
        }
        public static void UpdateTraning(TrainingAssessmentHeader ass)
        {
            if (ass == null)
            {
                return;
            }
            TrainingAssessmentHeader appointment = (
                from carSchedule in HammerDataProvider.Context.TrainingAssessmentHeaders
                where carSchedule.UniqueID == ass.UniqueID
                select carSchedule).SingleOrDefault<TrainingAssessmentHeader>();
            if (appointment != null)
            {
                appointment.IsDelete = true;
            }
            TrainingAssessmentsDetail detail = (
                from carSchedule in HammerDataProvider.Context.TrainingAssessmentsDetails
                where carSchedule.UniqueID == ass.UniqueID
                select carSchedule).SingleOrDefault<TrainingAssessmentsDetail>();
            if (detail != null)
            {
                detail.IsDelete = true;
            }
            Context.SubmitChanges();
        }
        public static void UpdateIsDeleteAppoint(Appointment app)
        {
            if (app == null)
            {
                return;
            }
            Appointment appointment = (
                from carSchedule in HammerDataProvider.Context.Appointments
                where carSchedule.UniqueID == app.UniqueID
                select carSchedule).SingleOrDefault<Appointment>();
            if (app != null)
            {
                appointment.IsDelete = true;
                Context.SubmitChanges();
            }
        }
        public static SMTrainingAssessmentHeader GetSMTrainingAssessment(System.DateTime assessmentDate, string employeeID, string unique)
        {
            return (
                 from assessment in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                 where assessment.UserID.Trim() == employeeID.Trim() && assessment.UniqueID.Value.ToString().Trim() == unique.Trim()
                 select assessment).FirstOrDefault();
            
        }
        public static TrainingAssessmentHeader GetTrainingAssessment(System.DateTime assessmentDate, string employeeID, string unique)
        {
            return (
                from assessment in HammerDataProvider.Context.TrainingAssessmentHeaders
                where assessment.UserID.Trim() == employeeID.Trim() && assessment.UniqueID.Value.ToString().Trim() == unique.Trim()
                select assessment).FirstOrDefault();
           
        }
        public static ScheduleSubmitSetting CheckOpeningDateEm(DateTime Date, string Em)
        {
            ScheduleSubmitSetting source =
                  (from setting in Context.ScheduleSubmitSettings                   
                   where setting.EmployeeID.ToUpper() == Em.ToUpper()             
                   && setting.Status == 0
                   && setting.Date.Date == Date.Date
                   orderby setting.CreatedDate descending
                   select setting).FirstOrDefault();
            return source;
        }
        public static bool CloseDateOpeningEm(DateTime Date, string Em)
        {
            ScheduleSubmitSetting source =
                 (from setting in Context.ScheduleSubmitSettings
                  where setting.EmployeeID.ToUpper() == Em.ToUpper()
                  && setting.Status == 0
                  && setting.Date.Date == Date.Date
                  orderby setting.CreatedDate descending
                  select setting).FirstOrDefault();
            if (source != null)
            {
                source.Status = 1;
                Context.SubmitChanges();
                return true;
            }
            else
            {
                return false;
            }
           
        }
        public static ScheduleSubmitSetting GetOpenEnableDate(string Em, string Year,int  Week, string shift)
        {

            DateTime d;
            
            DateTime.TryParse(Year.ToString() + "/01/01", out d);
            d = d.AddDays(7 * (Week - 1));          
            while (d.DayOfWeek != DayOfWeek.Monday) d = d.AddDays(-1);
            switch (shift)
            {
                case "MoAM":
                    d = d.AddDays(0);
                    break;
                case "TuAM":
                    d = d.AddDays(1);
                    break;
                case "WeAM":                   
                    d = d.AddDays(2);
                    break;
                case "ThAM":                    
                    d = d.AddDays(3);
                    break;
                case "FrAM":
                    d = d.AddDays(4);                 
                    break;
                case "SaAM":
                    d = d.AddDays(5);                      
                    break;
                case "MoPM":                    
                    d = d.AddDays(0);
                    break;
                case "TuPM":
                    d = d.AddDays(1); 
                    break;
                case "WePM":
                    d = d.AddDays(2); 
                    break;
                case "ThPM":
                    d = d.AddDays(3);         
                    break;
                case "FrPM":
                    d = d.AddDays(4); 
                    break;
                case "SaPM":
                    d = d.AddDays(5); 
                    break;
            }                      
            ScheduleSubmitSetting source =
                    (from setting in Context.ScheduleSubmitSettings
                     join sf in Context.DMSSalesForces on setting.EmployeeID equals sf.EmployeeID
                     where setting.EmployeeID.ToUpper() == Em.ToUpper() &&
                     setting.Date.Date == d.Date
                     && setting.Type == shift.Substring(2,2)
                     && setting.Status == 0
                     orderby setting.CreatedDate descending
                     select setting).FirstOrDefault();
            return source;
            
        }
       
        public static DMSDistributorRouteAssignment GetRouteWithSF(string NVSF, string NVWW)
        {
            DMSSalesForce dMSSalesForce = HammerDataProvider.Context.DMSSalesForces.ToList<DMSSalesForce>().FirstOrDefault((DMSSalesForce x) => (x.LoginID ?? "").Trim().ToUpper() == NVSF.Trim().ToUpper() && x.Active == true);
            DMSDistributorRouteAssignment dMSDistributorRouteAssignment = new DMSDistributorRouteAssignment();
            DMSSFHierarchy dMSSFHierarchy = (
                from sf in HammerDataProvider.Context.DMSSalesForces
                join sfAssignment in HammerDataProvider.Context.DMSSFHierarchies on sf.SFLevel equals sfAssignment.LevelID
                where sf.LoginID.Trim() == NVSF && sf.Active == (bool?)true
                select sfAssignment).SingleOrDefault<DMSSFHierarchy>();
            if (dMSSFHierarchy == null)
            {
                return dMSDistributorRouteAssignment;
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && !dMSSFHierarchy.Parent.HasValue)
            {
                return null;
            }
            if (dMSSFHierarchy.TerritoryType == 'N' && dMSSFHierarchy.IsSalesForce == true && dMSSFHierarchy.Parent.HasValue)
            {
                System.Collections.Generic.List<EmployeeModel> sSSMNotInRSM = HammerDataProvider.GetSSSMNotInRSM(dMSSalesForce);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = sSSMNotInRSM.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        dMSDistributorRouteAssignment = (
                            from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                            join sm in HammerDataProvider.Context.Salespersons on smAssignment.SalesPersonID equals (int?)sm.SalespersonID
                            where  smAssignment.SalesSupID == ss.EmployeeID
                            select smAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
                        if (dMSDistributorRouteAssignment != null)
                        {
                            DMSDistributorRouteAssignment result = dMSDistributorRouteAssignment;
                            return result;
                        }
                    }
                    return dMSDistributorRouteAssignment;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'R' && dMSSFHierarchy.IsSalesForce == true)
            {
                System.Collections.Generic.List<EmployeeModel> sSSMNotInASM = HammerDataProvider.GetSSSMNotInASM(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator2 = sSSMNotInASM.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator2.MoveNext())
                    {
                        ss = enumerator2.Current;
                        dMSDistributorRouteAssignment = (
                            from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                            join sm in HammerDataProvider.Context.Salespersons on smAssignment.SalesPersonID equals (int?)sm.SalespersonID
                            where  smAssignment.SalesSupID == ss.EmployeeID
                            select smAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
                        if (dMSDistributorRouteAssignment != null)
                        {
                            DMSDistributorRouteAssignment result = dMSDistributorRouteAssignment;
                            return result;
                        }
                    }
                    return dMSDistributorRouteAssignment;
                }
            }
            if (dMSSFHierarchy.TerritoryType == 'A' && dMSSFHierarchy.IsSalesForce == true)
            {
                System.Collections.Generic.List<EmployeeModel> subordinateSS = HammerDataProvider.GetSubordinateSS(dMSSalesForce.EmployeeID);
                using (System.Collections.Generic.List<EmployeeModel>.Enumerator enumerator = subordinateSS.GetEnumerator())
                {
                    EmployeeModel ss;
                    while (enumerator.MoveNext())
                    {
                        ss = enumerator.Current;
                        dMSDistributorRouteAssignment = (
                            from smAssignment in HammerDataProvider.Context.DMSDistributorRouteAssignments
                            join sm in HammerDataProvider.Context.Salespersons on smAssignment.SalesPersonID equals (int?)sm.SalespersonID
                            where  smAssignment.SalesSupID == ss.EmployeeID
                            select smAssignment).FirstOrDefault<DMSDistributorRouteAssignment>();
                        if (dMSDistributorRouteAssignment != null)
                        {
                            DMSDistributorRouteAssignment result = dMSDistributorRouteAssignment;
                            return result;
                        }
                    }
                    return dMSDistributorRouteAssignment;
                }
            }            
            return dMSDistributorRouteAssignment;
        }
        public static Appointment CheckWWGetAppointmentsShift(System.DateTime date, string employeeID,string shiftID)
        {
            Appointment appointment = null;
            appointment = (
                from ap in HammerDataProvider.Context.Appointments
                where ap.UserLogin == employeeID && ap.Label == (int?)3 && ap.StartDate.Value.Date == date.Date && ap.ScheduleType == "D"
                && ap.ShiftID == shiftID
                select ap into x
                orderby x.StartDate
                select x).FirstOrDefault<Appointment>();
            if (appointment != null)
            {
                if (appointment.Employees != null)
                {
                    while (appointment != null)
                    {
                        if (HammerDataProvider.EmployeeInRole(appointment.Employees) == SystemRole.Salesman)
                        {
                            break;
                        }
                        appointment = (
                            from ap in HammerDataProvider.Context.Appointments
                            where ap.UserLogin == appointment.Employees && ap.Label == (int?)3 && ap.StartDate.Value.Date == date.Date && ap.ScheduleType == "D"
                            && ap.ShiftID == shiftID
                            select ap into x
                            orderby x.StartDate
                            select x).FirstOrDefault<Appointment>();
                    }
                }
                else
                {
                    appointment = null;
                }
            }
            return appointment;
        }
        public static PopUpTooltipModel GetToolTipSaleOrg(string NVWW)
        {         
            PopUpTooltipModel rs = new PopUpTooltipModel();            
            List<DMSSFAssignment> source =
                    (from setting in Context.DMSSFAssignments
                     where setting.EmployeeID == NVWW && setting.IsActive == true                    
                     select setting).ToList();
            foreach (DMSSFAssignment item in source)
            {
                Region region =
                   (from setting in Context.Regions
                    where setting.RegionID == item.RegionID
                   select setting).FirstOrDefault();
                rs.RegionName = rs.RegionName + region.RegionName;
                Area area =
                   (from setting in Context.Areas
                    where setting.RegionID == item.RegionID && setting.AreaID == item.AreaID
                    select setting).FirstOrDefault();
                rs.AreaName = rs.AreaName + area.AreaName;
                Distributor dis =
                  (from setting in Context.Distributors
                   where setting.CompanyID == item.DistributorID
                   select setting).FirstOrDefault();
            }
            return rs;
        }
        #endregion
    }
}