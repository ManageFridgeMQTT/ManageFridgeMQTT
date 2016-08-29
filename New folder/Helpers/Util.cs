using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using OfficeOpenXml;
using System.Reflection;
using System.IO;
//using Hammer.Models;
//using log4net;
using DevExpress.Charts.Native;
using System.Drawing;
using OfficeOpenXml.Style;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Mail;
using System.Net;
using DevExpress.XtraPrinting.Native.LayoutAdjustment;
using eRoute.Models;
using eRoute.Models.eCalendar;
using Hammer.Models;
using DMSERoute.Helpers;

namespace Hammer.Helpers
{
    public class Util
    {
        /// <summary>
        /// Logger
        /// </summary>
        //private static readonly ILog Log = LogManager.GetLogger(typeof(Util));

        public static void LogUserAction(string page, string action, string param, int User, string url)
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
                eRoute.Models.UserActionLog userAL = new eRoute.Models.UserActionLog();
                userAL.UserID = User;
                userAL.ReferUrl = url;
                userAL.Page = page;
                userAL.Action = action;
                userAL.Param = param;
                userAL.Date = DateTime.Now;               
                Global.Context.UserActionLogs.InsertOnSubmit(userAL);
                Global.Context.SubmitChanges();
            }
            catch (Exception ex)
            {
                //Log.Error(ex.Message, ex);
            }
        }
        /// <summary>
        /// Generate working schedule using template
        /// </summary>
        /// <param name="source">Source data</param>
        /// <param name="templatePath">Template path</param>
        /// <param name="scheduleType">Schedule type: month || detail</param>
        public static Byte[] GenerateScheduleUsingTemplate(List<ScheduleModel> source, string templatePath, string scheduleType)
        {

            using (Stream stream = new FileStream(templatePath, FileMode.Open,
                                      FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    switch (scheduleType)
                    {
                        case "month":
                            ExcelWorksheet monthSheet = package.Workbook.Worksheets["LichThang"];
                            package.Workbook.Worksheets.Delete("LichChiTiet");
                            try
                            {
                                monthSheet.Cells["A2"].LoadFromCollection(source);
                            }
                            catch (Exception ex)
                            {
                                //Log.Error(ex.Message, ex);
                            }
                            break;
                        case "detail":
                            ExcelWorksheet detailSheet = package.Workbook.Worksheets["LichChiTiet"];
                            package.Workbook.Worksheets.Delete("LichThang");
                            try
                            {
                                detailSheet.Cells["A2"].LoadFromCollection(source);
                            }
                            catch (Exception ex)
                            {
                               // //Log.Error(ex.Message, ex);
                            }
                            break;
                    }
                    return package.GetAsByteArray();
                }
            }
        }

        /// <summary>
        /// Generate Error Template
        /// </summary>
        /// <param name="source"></param>
        /// <param name="templatePath"></param>
        /// <param name="scheduleType"></param>
        /// <returns></returns>
        public static Byte[] GenerateErrorTemplate(List<ScheduleErrorModel> source, string templatePath, string scheduleType)
        {
            using (Stream stream = new FileStream(templatePath, FileMode.Open,
                                      FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    switch (scheduleType)
                    {
                        case "month":
                            ExcelWorksheet monthSheet = package.Workbook.Worksheets["LichThang"];
                            package.Workbook.Worksheets.Delete("LichChiTiet");
                            if (source == null)
                                break;
                            for (int i = 0; i < source.Count; i++)
                            {

                                if (source[i].Status == 1)
                                {
                                    for (int j = 1; j <= 11; j++)
                                    {
                                        monthSheet.Cells[i + 2, j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        monthSheet.Cells[i + 2, j].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                    }
                                }
                                else if (source[i].Status == 2)
                                {
                                    monthSheet.TabColor = Color.Red;
                                    break;
                                }
                                monthSheet.Cells[i + 2, 1].Value = source[i].EmployeeID;
                                monthSheet.Cells[i + 2, 2].Value = source[i].Day;
                                monthSheet.Cells[i + 2, 3].Value = source[i].Month;
                                monthSheet.Cells[i + 2, 4].Value = source[i].Year;
                                monthSheet.Cells[i + 2, 5].Value = source[i].StartTime;
                                monthSheet.Cells[i + 2, 6].Value = source[i].EndTime;
                                monthSheet.Cells[i + 2, 7].Value = source[i].Title;
                                monthSheet.Cells[i + 2, 8].Value = source[i].Content;
                                monthSheet.Cells[i + 2, 9].Value = source[i].Outlet;
                                monthSheet.Cells[i + 2, 10].Value = source[i].PhoneNumber;
                                monthSheet.Cells[i + 2, 11].Value = source[i].WWCode;
                                monthSheet.Cells[i + 2, 12].Value = source[i].Note;
                            }
                            break;
                        case "detail":
                            ExcelWorksheet detailSheet = package.Workbook.Worksheets["LichChiTiet"];
                            package.Workbook.Worksheets.Delete("LichThang");
                            if (source == null)
                                break;
                            for (int i = 0; i < source.Count; i++)
                            {
                                if (source[i].Status == 1)
                                {
                                    for (int j = 1; j <= 11; j++)
                                    {
                                        detailSheet.Cells[i + 2, j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        detailSheet.Cells[i + 2, j].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                    }
                                }
                                else if (source[i].Status == 2)
                                {
                                    detailSheet.TabColor = Color.Red;
                                    break;
                                }
                                detailSheet.Cells[i + 2, 1].Value = source[i].EmployeeID;
                                detailSheet.Cells[i + 2, 2].Value = source[i].Day;
                                detailSheet.Cells[i + 2, 3].Value = source[i].Month;
                                detailSheet.Cells[i + 2, 4].Value = source[i].Year;
                                detailSheet.Cells[i + 2, 5].Value = source[i].StartTime;
                                detailSheet.Cells[i + 2, 6].Value = source[i].EndTime;
                                detailSheet.Cells[i + 2, 7].Value = source[i].Title;
                                detailSheet.Cells[i + 2, 8].Value = source[i].Content;
                                detailSheet.Cells[i + 2, 9].Value = source[i].Outlet;
                                detailSheet.Cells[i + 2, 10].Value = source[i].PhoneNumber;
                                detailSheet.Cells[i + 2, 11].Value = source[i].WWCode;
                                detailSheet.Cells[i + 2, 13].Value = source[i].Note;
                            }
                            break;
                    }
                    return package.GetAsByteArray();
                }
            }
        }
        public static void SaveFileUpload(List<ScheduleErrorModel> source, string templatePath, string scheduleType, string File)
        {

            using (Stream stream = new FileStream(templatePath, FileMode.Open,
                                      FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    switch (scheduleType)
                    {
                        case "month":

                            ExcelWorksheet monthSheet = package.Workbook.Worksheets["LichThang"];
                            package.Workbook.Worksheets.Delete("LichChiTiet");
                            for (int i = 0; i < source.Count; i++)
                            {
                                monthSheet.Cells[i + 2, 1].Value = source[i].EmployeeID;
                                monthSheet.Cells[i + 2, 2].Value = source[i].Day;
                                monthSheet.Cells[i + 2, 3].Value = source[i].Month;
                                monthSheet.Cells[i + 2, 4].Value = source[i].Year;
                                monthSheet.Cells[i + 2, 5].Value = source[i].StartTime;
                                monthSheet.Cells[i + 2, 6].Value = source[i].EndTime;
                                monthSheet.Cells[i + 2, 7].Value = source[i].Title;
                                monthSheet.Cells[i + 2, 8].Value = source[i].Content;
                                monthSheet.Cells[i + 2, 9].Value = source[i].Outlet;
                                monthSheet.Cells[i + 2, 10].Value = source[i].PhoneNumber;
                                monthSheet.Cells[i + 2, 11].Value = source[i].WWCode;
                                monthSheet.Cells[i + 2, 13].Value = source[i].Note;
                            }
                            break;
                        case "detail":
                            ExcelWorksheet detailSheet = package.Workbook.Worksheets["LichChiTiet"];
                            package.Workbook.Worksheets.Delete("LichThang");
                            for (int i = 0; i < source.Count; i++)
                            {
                                detailSheet.Cells[i + 2, 1].Value = source[i].EmployeeID;
                                detailSheet.Cells[i + 2, 2].Value = source[i].Day;
                                detailSheet.Cells[i + 2, 3].Value = source[i].Month;
                                detailSheet.Cells[i + 2, 4].Value = source[i].Year;
                                detailSheet.Cells[i + 2, 5].Value = source[i].StartTime;
                                detailSheet.Cells[i + 2, 6].Value = source[i].EndTime;
                                detailSheet.Cells[i + 2, 7].Value = source[i].Title;
                                detailSheet.Cells[i + 2, 8].Value = source[i].Content;
                                detailSheet.Cells[i + 2, 9].Value = source[i].Outlet;
                                detailSheet.Cells[i + 2, 10].Value = source[i].PhoneNumber;
                                detailSheet.Cells[i + 2, 11].Value = source[i].WWCode;
                                detailSheet.Cells[i + 2, 13].Value = source[i].Note;
                            }
                            break;
                    }

                    FileStream fs = new FileStream(File, FileMode.CreateNew);
                    MemoryStream ms = new MemoryStream(package.GetAsByteArray());
                    ms.WriteTo(fs);
                    ms.Close();
                    fs.Close();
                    fs.Dispose();

                }
            }
        }
        public static Byte[] ExportExcelReportOpenDate(List<ScheduleSubmitSettingEx> source, string templatePath)
        {

            using (Stream stream = new FileStream(templatePath, FileMode.Open,
                                       FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {

                    ExcelWorksheet monthSheet = package.Workbook.Worksheets["Sheet1"];
                    if (source.Count() > 0)
                    {
                        monthSheet.Cells["A2"].LoadFromCollection(source);
                    }
                    return package.GetAsByteArray();
                }
            }
        }
        public static Byte[] ExportExcelReportEmployeeStatus(List<ReportEmployeesStatusModel> source, string templatePath)
        {

            using (Stream stream = new FileStream(templatePath, FileMode.Open,
                                       FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {

                    ExcelWorksheet monthSheet = package.Workbook.Worksheets["Sheet1"];
                    monthSheet.Cells["A2"].LoadFromCollection(source);
                    return package.GetAsByteArray();
                }
            }
        }
        public static Byte[] ExportExcelAssessmentCapacity(List<ExcelDetailsAssessmentCapacity> source, string templatePath)
        {

            using (Stream stream = new FileStream(templatePath, FileMode.Open,
                                       FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    if (source.Count > 0)
                    {
                        ExcelWorksheet Sheet = package.Workbook.Worksheets["Assesment-Capacity"];
                        Sheet.Cells["A2"].LoadFromCollection(source);
                        return package.GetAsByteArray();
                    }
                    else
                    {
                        ExcelWorksheet Sheet = package.Workbook.Worksheets["Assesment-Capacity"];
                        return package.GetAsByteArray();
                    }
                }
            }
        }
        /// <summary>
        /// Get Base Url
        /// </summary>
        /// <returns></returns>
        public static string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;
            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }

        /// <summary>
        /// Export schedule
        /// </summary>
        /// <param name="source"></param>
        /// <param name="templatePath"></param>
        /// <param name="scheduleType"></param>
        /// <returns></returns>
        public static Byte[] ExportExcel(List<ScheduleExcelModel> source, string templatePath, string employeeID)
        {

            using (Stream stream = new FileStream(templatePath, FileMode.Open,
                                      FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet monthSheet = package.Workbook.Worksheets["Lich"];
                    monthSheet.Name = employeeID;
                    try
                    {
                        monthSheet.Cells["A2"].LoadFromCollection(source);
                    }
                    catch (Exception ex)
                    {
                        //Log.Error(ex.StackTrace, ex);
                    }
                    return package.GetAsByteArray();
                }
            }
        }

        /// <summary>
        /// Export attach multiple employee
        /// </summary>
        /// <param name="source"></param>
        /// <param name="templatePath"></param>
        /// <returns></returns>
        public static Byte[] ExportAttachMultiEmployee(List<MultiExportScheduleModelEx> source, string templatePath)
        {
            using (Stream stream = new FileStream(templatePath, FileMode.Open,
                                      FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    foreach (var item in source)
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Lich"];
                        //ExcelWorksheet worksheet = package.Workbook.Worksheets.Copy("Lich", item.EmployeeID);
                        try
                        {
                            worksheet.Cells["A2"].LoadFromCollection(item.Schedule);
                        }
                        catch (Exception ex)
                        {
                           // //Log.Error(ex.Message, ex);
                        }
                        //ExcelWorksheet monthSheet = package.Workbook.Worksheets["Lich"];
                        //monthSheet.Cells["A2"].LoadFromCollection(source);
                        //return package.GetAsByteArray();
                    }
                    //try
                    //{
                    //    package.Workbook.Worksheets.Delete("Lich");
                    //}
                    //catch (Exception ex)
                    //{
                    //    //Log.Error(ex.Message, ex);
                    //}
                    return package.GetAsByteArray();
                }
            }
        }

        #region SendMail

        /// <summary>
        /// Init scheduler email (approve/reject/delete) etc
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fullname"></param>
        /// <param name="emailTemplate"></param>
        /// <param name="subject"></param>
        /// <param name="workingDate"></param>
        /// <returns>Stuff</returns>
        public static bool InitSchedulerEmail(string email, string fullname,
            string emailTemplate, DateTime workingDate, string scheduleType, string url)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);

                body = body.Replace("{FullName}", fullname);
                body = body.Replace("{WorkingDate}", workingDate.ToString("dd/MM/yyyy"));
                body = body.Replace("{ScheduleType}", scheduleType.Trim() == "D" ? "chi tiết" : "tháng");
                body = body.Replace("{WebUrl}", url);

                SendMail(email, string.Format(Constant.SubjectSchedule, workingDate.ToString("dd/MM/yyyy")), body);

                return true;
            }
            catch (Exception ex)
            {
               // //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        public static bool InitSchedulerEmail(string email, string fullname,
           string emailTemplate, List<DateTime> workingDate, string scheduleType, string url)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);

                body = body.Replace("{FullName}", fullname);
                body = body.Replace("{WorkingDate}", string.Join(",", workingDate.Select(d => d.ToString("dd/MM/yyyy")).ToArray()));
                body = body.Replace("{ScheduleType}", scheduleType.Trim() == "D" ? "chi tiết" : "tháng");
                body = body.Replace("{WebUrl}", url);

                SendMail(email, string.Format(Constant.SubjectSchedule, string.Join(",", workingDate.Select(d => d.ToString("dd/MM/yyyy")).ToArray())), body);

                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        public static bool InitSendEmailOpenDate(string email, string fullname,
           string emailTemplate, ScheduleSubmitSetting setting, string scheduleType, string url, string ccLevel1, string ccLevel2, string ccSystem)
        {
            try
            {
                string controller = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                string action = HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);

                body = body.Replace("{FullName}", fullname);
                if (action == "Close" && controller == "SubmitAssessment")
                {
                    body = body.Replace("{WorkingDate}", string.Join(",",
                        setting.Date.Date.ToString("dd/MM/yyyy")));
                    body = body.Replace("{Action}", string.Join(",",
                       "đóng"));
                }
                else
                {
                    body = body.Replace("{WorkingDate}", string.Join(",",
                        setting.Date.Date.ToString("dd/MM/yyyy")));
                    body = body.Replace("{Action}", string.Join(",",
                       "mở"));

                }
                body = body.Replace("{ScheduleType}", scheduleType.Trim() == "AM" ? "AM" : "PM");
                body = body.Replace("{WebUrl}", url);
                if (action == "Close" && controller == "SubmitAssessment")
                {
                    SendMailCC(email, string.Format("[Hamer] Thông báo đóng ngày", string.Join(",", setting.Date.Date.ToString("dd/MM/yyyy hh:mm"))), body, ccLevel1, ccLevel2, ccSystem);
                }
                else
                {
                    SendMailCC(email, string.Format("[Hamer] Thông báo mở ngày", string.Join(",", setting.Date.Date.ToString("dd/MM/yyyy"))), body, ccLevel1, ccLevel2, ccSystem);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        public static bool InitSchedulerEmail(string email, string fullname,
            string emailTemplate, Appointment appt, string scheduleType, string url)
        {
            try
            {
                string controller = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                string action = HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);

                body = body.Replace("{FullName}", fullname);
                if (action == "ApproveTask" && controller == "ApproveSchedule")
                {
                    body = body.Replace("{WorkingDate}", string.Join(",",
                        appt.StartDate.Value.ToString("dd/MM/yyyy hh:mm") + "-" + appt.EndDate.Value.ToString("dd/MM/yyyy hh:mm")));
                }
                else
                {
                    body = body.Replace("{WorkingDate}", string.Join(",",
                        appt.StartDate.Value.ToString("dd/MM/yyyy")));
                }
                //if (action == "RejectTask" && controller == "ApproveSchedule")
                //{
                //    body = body.Replace("{WorkingDate}", string.Join(",",
                //        appt.StartDate.Value.ToString("dd/MM/yyyy hh:mm") + "-" + appt.EndDate.Value.ToString("dd/MM/yyyy hh:mm")));
                //}   
                body = body.Replace("{ScheduleType}", scheduleType.Trim() == "D" ? "chi tiết" : "tháng");
                body = body.Replace("{WebUrl}", url);
                if (action == "ApproveTask" && controller == "ApproveSchedule")
                {
                    SendMail(email, string.Format(Constant.SubjectSchedule, string.Join(",", appt.StartDate.Value.ToString("dd/MM/yyyy hh:mm") + "-" + appt.EndDate.Value.ToString("dd/MM/yyyy hh:mm"))), body);
                }
                else
                {
                    SendMail(email, string.Format(Constant.SubjectSchedule, string.Join(",", appt.StartDate.Value.ToString("dd/MM/yyyy"))), body);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }

        private static bool InitSendMail(string email, string userFullName,
            string userName, string password, string confirmationToken,
            string emailTemplate, string subject, bool isAddNew)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string path = string.Empty;
                string body = System.IO.File.ReadAllText(path);

                body = body.Replace("{WebUrl}", Util.GetBaseUrl());
                body = body.Replace("{FullName}", userFullName);
                body = body.Replace("{Account}", userName);
                body = body.Replace("{Password}", password);
                body = body.Replace("{TokenConfirmation}", confirmationToken);

                SendMail(email, subject, body);

                var obj = new { Email = email, UserName = userName, Password = password, ConfirmationToken = confirmationToken };

                return true;
            }
            catch (Exception ex)
            {
                var obj = new { Email = email, UserName = userName, Password = password, ConfirmationToken = confirmationToken };
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        public static bool InitChangeSchedulerEmail(string email, string fullname, string ToName,
          string emailTemplate, string scheduleType, string Date, string action,
           string url, string levelUp, string levelDown)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);
                body = body.Replace("{FullName}", fullname);
                body = body.Replace("{SendName}", ToName);
                body = body.Replace("{LevelUp}", levelDown);
                body = body.Replace("{LevelDown}", levelUp);
                body = body.Replace("{record}", Date);
                body = body.Replace("{action}", action);
                body = body.Replace("{WebUrl}", url);
                body = body.Replace("{ScheduleType}", scheduleType == "detail" ? "chi tiết" : "tháng");

                SendMail(email, string.Format(Constant.SubjectSchedule, fullname), body);

                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        public static bool InitUploadSchedulerEmail(eRoute.Models.eCalendar.DMSSalesForce email, string fullname, string ToName,
           string emailTemplate, string scheduleType, List<ScheduleErrorModel> list,
            string url, string levelUp, string levelDown, string Userprepare)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);
                body = body.Replace("{FullName}", fullname);
                body = body.Replace("{SendName}", ToName);
                body = body.Replace("{LevelUp}", levelDown);
                body = body.Replace("{LevelDown}", levelUp);
                string date = "";
                var ListDay = (from ds in list where ds.EmployeeID == Userprepare select new { EmployeeID = ds.EmployeeID, Year = ds.Year, Month = ds.Month, Day = ds.Day }).Distinct().ToList();
                foreach (var item in ListDay)
                {

                    string DateFrom = item.Day.ToString() + '/'
                                     + item.Month.ToString() + '/' + item.Year.ToString();
                    if (date.Trim() != String.Empty)
                    {
                        date = date + ',' + DateFrom;
                    }
                    else
                    {
                        date = DateFrom;
                    }


                }
                body = body.Replace("{record}", date);
                body = body.Replace("{WebUrl}", url);
                body = body.Replace("{ScheduleType}", scheduleType == "detail" ? "chi tiết" : "tháng");

                SendMail(email.Email, string.Format(Constant.SubjectSchedule, fullname), body);

                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        public static bool InitUploadSchedulerEmail(eRoute.Models.eCalendar.DMSSalesForce email, string fullname, string ToName,
           string emailTemplate, string scheduleType, List<ScheduleErrorModel> list,
            string url, string levelUp, string levelDown)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);
                body = body.Replace("{FullName}", fullname);
                body = body.Replace("{SendName}", ToName);
                body = body.Replace("{LevelUp}", levelDown);
                body = body.Replace("{LevelDown}", levelUp);
                string date = "";
                var ListDay = (from ds in list where ds.EmployeeID == email.EmployeeID select new { EmployeeID = ds.EmployeeID, Year = ds.Year, Month = ds.Month, Day = ds.Day }).Distinct().ToList();
                foreach (var item in ListDay)
                {

                    string DateFrom = item.Day.ToString() + '/'
                                     + item.Month.ToString() + '/' + item.Year.ToString();
                    if (date.Trim() != String.Empty)
                    {
                        date = date + ',' + DateFrom;
                    }
                    else
                    {
                        date = DateFrom;
                    }


                }
                body = body.Replace("{record}", date);
                body = body.Replace("{WebUrl}", url);
                body = body.Replace("{ScheduleType}", scheduleType == "detail" ? "chi tiết" : "tháng");

                SendMail(email.Email, string.Format(Constant.SubjectSchedule, fullname), body);

                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        public static bool InitUploadSchedulerEmailNew(eRoute.Models.eCalendar.DMSSalesForce email, string fullname, string ToName,
           string emailTemplate, List<SendEmailPreapre> list,
            string url, string levelUp, string levelDown)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);
                body = body.Replace("{FullName}", fullname);
                body = body.Replace("{SendName}", ToName);
                body = body.Replace("{LevelUp}", levelDown);
                body = body.Replace("{LevelDown}", levelUp);
                string date = "";
                var ListDay = (from ds in list
                               where ds.EmployeeID == email.EmployeeID
                               select new
                               {
                                   EmployeeID = ds.EmployeeID,
                                   Year = ds.Date.Year,
                                   Month = ds.Date.Month,
                                   Day = ds.Date.Day
                                   ,
                                   Shift = ds.Shift
                               }).Distinct().ToList();
                foreach (var item in ListDay)
                {

                    string DateFrom = item.Day.ToString() + '/'
                                     + item.Month.ToString() + '/' + item.Year.ToString() + '-' + item.Shift;
                    if (date.Trim() != String.Empty)
                    {
                        date = date + ',' + ' ' + DateFrom;
                    }
                    else
                    {
                        date = DateFrom;
                    }


                }
                body = body.Replace("{record}", date);
                body = body.Replace("{WebUrl}", url);
                // body = body.Replace("{ScheduleType}", shift == "AM" ? "AM" : "PM");
                SendMail(email.Email, string.Format(Constant.SubjectSchedule, fullname), body);

                return true;
            }
            catch (Exception ex)
            {
               // //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        /// <summary>
        /// Doing stuff, lol
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fullname"></param>
        /// <param name="emailTemplate"></param>
        /// <param name="workingDate"></param>
        /// <param name="reason"></param>
        /// <returns>stuff</returns>
        public static bool InitRejectSchedulerEmail(string email, string fullname,
            string emailTemplate, DateTime workingDate, string reason, string scheduleType, string url,
            string role, string userFullname)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);

                body = body.Replace("{FullName}", fullname);
                body = body.Replace("{WorkingDate}", workingDate.ToString("dd/MM/yyyy"));
                body = body.Replace("{Reason}", reason);
                body = body.Replace("{ScheduleType}", scheduleType.Trim() == "D" ? "chi tiết" : "tháng");
                body = body.Replace("{WebUrl}", url);
                body = body.Replace("{User}", role + " - " + userFullname);

                SendMail(email, string.Format(Constant.SubjectSchedule, workingDate.ToString("dd/MM/yyyy")), body);

                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }
        public static bool InitRejectSchedulerEmail(string email, string fullname,
            string emailTemplate, Appointment appt, string reason, string scheduleType, string url,
            string role, string userFullname)
        {
            try
            {
                string controller = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
                string action = HttpContext.Current.Request.RequestContext.RouteData.Values["action"].ToString();
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);

                body = body.Replace("{FullName}", fullname);
                if (action == "RejectTask" && controller == "ApproveSchedule")
                {
                    body = body.Replace("{WorkingDate}", string.Join(",",
                        appt.StartDate.Value.ToString("dd/MM/yyyy hh:mm") + "-" + appt.EndDate.Value.ToString("dd/MM/yyyy hh:mm")));
                }
                //body = body.Replace("{WorkingDate}", string.Join(",", workingDate.Select(d => d.ToString("dd/MM/yyyy")).ToArray()));
                body = body.Replace("{Reason}", reason);
                body = body.Replace("{ScheduleType}", scheduleType.Trim() == "D" ? "chi tiết" : "tháng");
                body = body.Replace("{WebUrl}", url);
                body = body.Replace("{User}", role + " - " + userFullname);
                if (action == "RejectTask" && controller == "ApproveSchedule")
                {
                    SendMail(email, string.Format(Constant.SubjectSchedule, string.Join(",", appt.StartDate.Value.ToString("dd/MM/yyyy hh:mm") + "-" + appt.EndDate.Value.ToString("dd/MM/yyyy hh:mm"))), body);
                }
                else
                {
                    SendMail(email, string.Format(Constant.SubjectSchedule, string.Join(",", appt.StartDate.Value.ToString("dd/MM/yyyy hh:mm"))), body);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }

        public static bool InitRejectSchedulerEmail(string email, string fullname,
            string emailTemplate, List<DateTime> workingDate, string reason, string scheduleType, string url,
            string role, string userFullname)
        {
            try
            {
                //string path = Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(emailTemplate);

                body = body.Replace("{FullName}", fullname);
                body = body.Replace("{WorkingDate}", string.Join(",", workingDate.Select(d => d.ToString("dd/MM/yyyy")).ToArray()));
                body = body.Replace("{Reason}", reason);
                body = body.Replace("{ScheduleType}", scheduleType.Trim() == "D" ? "chi tiết" : "tháng");
                body = body.Replace("{WebUrl}", url);
                body = body.Replace("{User}", role + " - " + userFullname);

                SendMail(email, string.Format(Constant.SubjectSchedule, string.Join(",", workingDate.Select(d => d.ToString("dd/MM/yyyy")).ToArray())), body);

                return true;
            }
            catch (Exception ex)
            {
                //Log.Error(ex.StackTrace, ex);

                return false;
            }
        }

        /// <summary>
        /// Send mail
        /// </summary>
        /// <param name="toMail"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private static void SendMailCC(string toMail, string subject, string body, string ccLevel1, string ccLevel2, string ccSystem)
        {
            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = Constant.Host;
            client.Port = Constant.Port;

            ServicePointManager.ServerCertificateValidationCallback = OurCertificateValidation;

            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(Constant.FromEmail, Constant.Password);
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(Constant.FromEmail, Constant.DisplayName);
            msg.To.Add(new MailAddress(toMail));
            if (!string.IsNullOrEmpty(ccLevel1))
                msg.CC.Add(new MailAddress(ccLevel1));
            if (!string.IsNullOrEmpty(ccLevel2))
                msg.CC.Add(new MailAddress(ccLevel2));
            if (!string.IsNullOrEmpty(ccSystem))
                msg.CC.Add(new MailAddress(ccSystem));
            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = body;
            client.Send(msg);
        }
        private static void SendMail(string toMail, string subject, string body)
        {
            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = Constant.Host;
            client.Port = Constant.Port;

            ServicePointManager.ServerCertificateValidationCallback = OurCertificateValidation;

            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(Constant.FromEmail, Constant.Password);
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(Constant.FromEmail, Constant.DisplayName);
            msg.To.Add(new MailAddress(toMail));
            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = body;
            client.Send(msg);
        }

        private static bool OurCertificateValidation(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion

        #region Assessment

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Byte[] GenSMTrainingTemplate(string template, SMAssessmentModel model)
        {            
            using (Stream stream = new FileStream(template, FileMode.Open,
                                      FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets["SR"];
                    eRoute.Models.eCalendar.Distributor dist =
                        HammerDataProvider.GetDistributors().FirstOrDefault(x => x.CompanyID.ToString() ==
                        (model.Header.DistributorID == null ? "" : model.Header.DistributorID.Trim()));

                    ws.Cells["B2"].Value = model.Header.AssessmentFor;
                    ws.Cells["C2"].Value = Utility.Phrase("UniqueID");
                    ws.Cells["D2"].Value = model.Header.UniqueID.ToString() + "-" + model.Header.AssessmentDate.ToString("dd/MM/yyyy");
                    ws.Cells["B3"].Value = model.Header.AreaID;
                    ws.Cells["D3"].Value = dist != null ? dist.CompanyCD + " - " + dist.CompanyName : "";
                    ws.Cells["B2"].Style.Locked = true;
                    ws.Protection.IsProtected = true;
                    return package.GetAsByteArray();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Byte[] GenSSTrainingTemplate(string template, AssessmentModel model)
        {
            using (Stream stream = new FileStream(template, FileMode.Open,
                                      FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage package = new ExcelPackage(stream))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets["DANHGIASS"];
                    eRoute.Models.eCalendar.Distributor dist =
                        HammerDataProvider.GetDistributors().FirstOrDefault(x => x.CompanyID.ToString() ==
                        (model.Header.DistributorID == null ? "" : model.Header.DistributorID.Trim()));

                    //SS
                    ws.Cells["A4"].Value = Utility.Phrase("UniqueID");
                    ws.Cells["B2"].Value = model.Header.AssessmentFor;
                    ws.Cells["B3"].Value = model.Header.SM;
                    ws.Cells["D2"].Value = model.Header.UserID;
                    //ASM
                    Appointment appointment = HammerDataProvider.GetDetailAppointment(model.Header.UserID, model.Header.AssessmentDate);
                    if (appointment != null)
                    {
                        string routeID = appointment.RouteID;
                        if (!string.IsNullOrEmpty(routeID))
                        {
                            string areaID = HammerDataProvider.GetAreaByDistributor(dist.CompanyID);
                            string asm = HammerDataProvider.GetASMIdByAreaID(areaID);
                            ws.Cells["D2"].Value = asm;
                        }
                    }
                    ws.Cells["D3"].Value = dist != null ? dist.CompanyCD + " - " + dist.CompanyName : "";
                    //AssessmentDate
                    ws.Cells["B4"].Value = model.Header.UniqueID.ToString() + "-" + model.Header.AssessmentDate.ToString("dd/MM/yyyy");
                    //Submit date
                    ws.Cells["B5"].Value = DateTime.Now.ToString("dd/MM/yyyy");
                    ws.Protection.IsProtected = true;
                    return package.GetAsByteArray();
                }
            }
        }
        #endregion
    }

    public enum SystemRole
    {
        SalesForce,
        System,
        Salesman
    }

}