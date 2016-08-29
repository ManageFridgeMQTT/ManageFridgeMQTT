using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using DevExpress.Web.ASPxUploadControl;
using DevExpress.Web.Mvc;
using Hammer.Models;
using log4net;
//using Utility.Phrase("SubmitAssessment");
using System.Globalization;
using Hammer.Helpers;
using System.IO;
using eRoute.Filters;
using WebMatrix.WebData;
using eRoute.Filters;
using eRoute.Models.eCalendar;
using eRoute;
using DMSERoute.Helpers;

namespace Hammer.Controllers
{
    [InitializeSimpleMembership]
    [Authorize()]
    public class SubmitAssessmentController : Controller
    {
        private const string SMTraining = "SMTraining";
        private const string SSTraining = "SSTraining";
        private const string NoTraining = "NoTraining";
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(SubmitAssessmentController));

        //
        // GET: /SubmitAssessment/
        [Authorize]
        [ActionAuthorize("eCalendar_SubmitAssessment", true)]
        public ActionResult Index()
        {
            string a = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
          
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            bool kq = HammerDataProvider.CheckPermissionRoles(User.Identity.Name);
            //if (kq == false)
            //{
            //    return RedirectToAction("index", "ErrorPermission");
            //}
            Session["Page_Index"] = SMTraining;
            return View();
        }

        public ActionResult SMTrainingAssessmentHeaderPartial()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView();
        }

        public ActionResult NonTrainingAssessmentPartial()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView();
        }

        public ActionResult DownloadTemplate()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string templatePath = Server.MapPath("/Templates/Assessments/");
            if (User.IsInRole("SS"))
            {
                templatePath += "SR_Assessment_Template.xlsx";
                
            }
            else
            {
                templatePath += "SS_Assessment_Template.xlsx";
            }

            return new FilePathResult(templatePath, "application/octet-stream")
            {
                FileDownloadName = templatePath.Substring(templatePath.LastIndexOf('\\') + 1)
            };
        }

        [HttpPost]
        public ActionResult UploadSMAssessmentFile()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];
                MemoryStream mem = new MemoryStream();
                mem.SetLength(file.ContentLength);

                file.InputStream.Read(mem.GetBuffer(), 0, (int)file.ContentLength);

                SMAssessmentModel model = HammerDataProvider.UploadSMAssessmentFromExcel(mem, User.Identity.Name);
                if (model != null)
                {
                    if (!HammerDataProvider.GetListUniqueIDAllTask(User.Identity.Name).Contains(model.Header.UniqueID.Value.ToString()))
                    {
                        // return Json("Ngày đánh giá không hợp lệ");
                        return Json(Utility.Phrase("eCalendar_Validate_DateNotValid"));
                    }

                    return PartialView("SMTrainingAssessmentHeaderPartial", model);
                }
                return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
                // return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
            }
            else
            {
                return Json(Utility.Phrase("eCalendar_PleaseChooseFile"));//"Vui lòng chọn file để up");
            }
        }

        [HttpPost]
        public ActionResult UploadAssessmentFile()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase file = Request.Files[0];
                MemoryStream mem = new MemoryStream();
                mem.SetLength(file.ContentLength);
                file.InputStream.Read(mem.GetBuffer(), 0, (int)file.ContentLength);
                AssessmentModel model = HammerDataProvider.UploadAssessmentFromExcel(mem, User.Identity.Name);
                if (model != null)
                {
                    if (!HammerDataProvider.GetListUniqueIDAllTask(User.Identity.Name).Contains(model.Header.UniqueID.Value.ToString()))
                    {
                        return Json(Utility.Phrase("eCalendar_Validate_DateNotValid"));
                        // return Json(Utility.Phrase("Date"));
                    }
                    var appointment = (from ap in HammerDataProvider.Context.Appointments
                                       where ap.UserLogin == model.Header.UserID
                                  && ap.UniqueID == model.Header.UniqueID
                                  && ap.ScheduleType == "D"
                                  && ap.Label.GetValueOrDefault(0) == 3
                                       select ap).SingleOrDefault(x => x.StartDate.Value.Day
                                      == model.Header.AssessmentDate.Day && x.StartDate.Value.Month == model.Header.AssessmentDate.Month
                                      && x.StartDate.Value.Year == model.Header.AssessmentDate.Year
                                      && x.StartDate.Value.TimeOfDay == model.Header.AssessmentDate.TimeOfDay);

                    if (appointment != null)
                    {
                        EmployeeModel SS = HammerDataProvider.GetSSAssmentforUnique(appointment);
                        if (SS != null)
                        {
                            if (model.Header.AssessmentFor != SS.EmployeeID)
                                // return Json("Nhân viên SS không hợp lệ");
                                return Json(Utility.Phrase("eCalendar_Validate_SalesupNotValid"));
                            if (model.Header.SM != appointment.Employees)
                                return Json(Utility.Phrase("eCalendar_Validate_SalemanNotValid"));
                            //return Json("Nhân viên SM không hợp lệ");
                        }
                    }
                    return PartialView("TrainingAssessmentHeaderPartial", model);
                }
                return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
            }
            else
            {
                //return Json("Vui lòng chọn file để up");
                return Json(Utility.Phrase("eCalendar_PleaseChooseFile"));
            }
        }

        [HttpPost]
        public ActionResult SubmitSMAssessment(SMAssessmentModel model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ModelState.IsValid)
            {
                if (ValidateSMAssessmentModel(model))
                {
                    try
                    {
                        HammerDataProvider.SubmitSMAssessment(model);
                        ViewData["SubmitSuccessfull"] = Utility.Phrase("SaveOki");
                        ModelState.Remove("");
                        return PartialView("SubmitSuccessfully");
                    }
                    catch (Exception)
                    {
                        return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Có lỗi trong quá trình nhập liệu");
                    return PartialView("SMTrainingAssessmentHeaderPartial", model);
                }
            }
            else
            {
                ValidateSMAssessmentModel(model);
                return PartialView("SMTrainingAssessmentHeaderPartial", model);
            }
        }

        [HttpPost]
        public ActionResult LoadSMAssessment()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string assessmentDateParam = Request.Params["assessmentDate"];
            int uniqueid = Convert.ToInt32(assessmentDateParam);
            //DateTimeFormatInfo ukDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            //DateTime test = Convert.ToDateTime(assessmentDateParam, ukDtfi);
            Appointment app = HammerDataProvider.GetAppointmentById(uniqueid);
            if (app != null)
            {
                //In case exist no training assessment
                NoAssessmentModel noAssessment = HammerDataProvider.GetNoAssessment(app.StartDate.Value, User.Identity.Name, app.UniqueID);
                if (noAssessment != null)
                {
                    return PartialView("NoTraningAssessmentPartial", noAssessment);
                }
                //WW Schedule
                if (!string.IsNullOrEmpty(app.Employees))
                {
                    DMSSFHierarchy query = HammerDataProvider.PrepareScheduleGetDMSSFHierarchy(app.UserLogin);
                    if (query.TerritoryType == 'D' && query.IsSalesForce == true)
                    {
                        if (HammerDataProvider.EmployeeInRole(app.Employees) == Helpers.SystemRole.Salesman)
                        {
                            SMAssessmentModel model = new SMAssessmentModel();
                            model = model.GetSMAssessmentModel(app.StartDate.Value, User.Identity.Name, app.UniqueID);
                            Session["Page_Index"] = SMTraining;
                            return PartialView("SMTrainingAssessmentHeaderPartial", model);
                        }
                        // 04-08-2014 Hieu add them case SM not syn
                        else if (HammerDataProvider.EmployeeInRole(app.Employees) == Helpers.SystemRole.SalesForce)
                        {
                            AssessmentModel model = new AssessmentModel(app.StartDate.Value, User.Identity.Name);
                            Session["Page_Index"] = SSTraining;
                            return PartialView("TrainingAssessmentHeaderPartial", model);
                        }
                        else
                        {
                            Session["Page_Index"] = NoTraining;
                            return PartialView("NoSubmitSuccessfully");
                            //return Json("Nhân viên " + app.Employees + " không tồn tại trên hệ thống hoặc đã tắt kích hoạt!");                       
                        }
                    }
                    else
                    {
                        AssessmentModel model = new AssessmentModel(app.StartDate.Value, User.Identity.Name, app.UniqueID);
                        Session["Page_Index"] = SSTraining;
                        return PartialView("TrainingAssessmentHeaderPartial", model);
                    }
                }
                else //NWW Schedule
                {
                    Session["Page_Index"] = NoTraining;
                    return PartialView("NoTraningAssessmentPartial", new NoAssessmentModel(app.StartDate.Value, User.Identity.Name, assessmentDateParam));
                }
            }
            Session["Page_Index"] = SMTraining;
            return Json(Utility.Phrase("eCalendar_NoAppointment"));
        }

        [HttpPost]
        public ActionResult SaveSMAssessment(SMAssessmentModel model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            //if (model.DailyWorks.Count > 1)
            //    model.Header.UniqueID = model.DailyWorks[0].UniqueID;
            if (ValidateSMAssessmentModel(model))
            {
                try
                {
                    HammerDataProvider.SaveSMAssessment(model);
                    ModelState.Remove("");
                    // return Json("Lưu thành công");
                    return Json(Utility.Phrase("SaveOki"));
                }
                catch (Exception)
                {
                    return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
                }
            }
            else
            {
                ModelState.AddModelError("", Utility.Phrase("AnErrorOccurredWhileProcessing"));
                return PartialView("SMTrainingAssessmentHeaderPartial", model);
            }
        }

        [HttpPost]
        public ActionResult SaveAssessment(AssessmentModel model)
        {
            //model.Header.UniqueID = Utility.IntParse(EditorExtension.GetValue<string>("UniqueID"));
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ValidateAssessmentModel(model))
            {
                try
                {
                    HammerDataProvider.SaveAssessment(model);
                    ModelState.Remove("");
                    return Json(Utility.Phrase("SaveOki"));
                    //return Json(Utility.Phrase("SaveOki"));
                }
                catch (Exception)
                {
                    return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
                }
            }
            else
            {
                ModelState.AddModelError("", Utility.Phrase("AnErrorOccurredWhileProcessing"));
                return PartialView("TrainingAssessmentHeaderPartial", model);
            }
        }

        [HttpPost]
        public ActionResult SubmitAssessment(AssessmentModel model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ValidateAssessmentModel(model))
            {
                try
                {
                    HammerDataProvider.SubmitAssessment(model);
                    ViewData["SubmitSuccessfull"] = Utility.Phrase("SaveOki");
                    ModelState.Remove("");
                    return PartialView("SubmitSuccessfully");
                }
                catch (Exception)
                {
                    //ModelState.AddModelError("", "Có lỗi trong quá trình nhập liệu");
                    return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
                }
            }
            else
            {
                ModelState.AddModelError("", Utility.Phrase("AnErrorOccurredWhileProcessing"));
                return PartialView("TrainingAssessmentHeaderPartial", model);
            }
        }

        private bool ValidateSMAssessmentModel(SMAssessmentModel model)
        {
            bool result = true;
            string errMes = Utility.Phrase("ErrMinimumWords");

            string trainingObjective = model.Header.TraningObjective ?? "";
            string comment = model.Header.Comment ?? "";
            string nextTrainingObjective = model.Header.NextTrainingObjective ?? "";
            string salesObjective = model.Header.SalesObjective ?? "";

            if (trainingObjective.Trim().Split(' ').Length < 8 ||
                    trainingObjective.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
            {
                ModelState.AddModelError("Header.TraningObjective", errMes);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.TraningObjective");
            }

            if (comment.Trim().Split(' ').Length < 8 ||
                    comment.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
            {
                ModelState.AddModelError("Header.Comment", errMes);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.Comment");
            }

            if (nextTrainingObjective.Trim().Split(' ').Length < 8 ||
                    nextTrainingObjective.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
            {
                ModelState.AddModelError("Header.NextTrainingObjective", errMes);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.NextTrainingObjective");
            }

            if (salesObjective.Trim().Split(' ').Length < 8 ||
                    salesObjective.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
            {
                ModelState.AddModelError("Header.SalesObjective", errMes);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.SalesObjective");
            }

            for (int i = 0; i < model.DailyWorks.Count; i++)
            {
                string note = model.DailyWorks[i].Note ?? "";
                string pros = model.DailyWorks[i].Pros ?? "";
                string cons = model.DailyWorks[i].Cons ?? "";

                if (note.Trim().Split(' ').Length < 8 ||
                    note.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("DailyWorks[" + i + "].Note", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("DailyWorks[" + i + "].Note");
                }

                if (pros.Trim().Split(' ').Length < 8 ||
                    pros.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("DailyWorks[" + i + "].Pros", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("DailyWorks[" + i + "].Pros");
                }

                if (cons.Trim().Split(' ').Length < 8 ||
                    cons.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("DailyWorks[" + i + "].Cons", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("DailyWorks[" + i + "].Cons");
                }

            }

            for (int i = 0; i < model.Steps.Count; i++)
            {
                string note = model.Steps[i].Note ?? "";
                string pros = model.Steps[i].Pros ?? "";
                string cons = model.Steps[i].Cons ?? "";

                if (note.Trim().Split(' ').Length < 8 ||
                    note.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Steps[" + i + "].Note", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Steps[" + i + "].Note");
                }

                if (pros.Trim().Split(' ').Length < 8 ||
                    pros.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Steps[" + i + "].Pros", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Steps[" + i + "].Pros");
                }

                if (cons.Trim().Split(' ').Length < 8 ||
                    cons.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Steps[" + i + "].Cons", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Steps[" + i + "].Cons");
                }
            }

            for (int i = 0; i < model.Tools.Count; i++)
            {
                string note = model.Tools[i].Note ?? "";
                string pros = model.Tools[i].Pros ?? "";
                string cons = model.Tools[i].Cons ?? "";

                if (note.Trim().Split(' ').Length < 8 ||
                    note.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Tools[" + i + "].Note", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Tools[" + i + "].Note");
                }

                if (pros.Trim().Split(' ').Length < 8 ||
                    pros.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Tools[" + i + "].Pros", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Tools[" + i + "].Pros");
                }

                if (cons.Trim().Split(' ').Length < 8 ||
                    cons.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Tools[" + i + "].Cons", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Tools[" + i + "].Cons");
                }
            }

            return result;
        }

        private bool ValidateAssessmentModel(AssessmentModel model)
        {
            bool result = true;
            string errMes = Utility.Phrase("ErrMinimumWords");
            string nullMsg = Utility.Phrase("ErrControlReq");

            string training = model.Header.Training ?? "";
            string pc = model.Header.PC ?? "";
            string lppc = model.Header.LPPC ?? "";
            string mark = model.Header.Mark ?? "";
            string abcdnextTrainingObjective = model.Header.ABCDNextTrainingObjective ?? "";
            string abcdcomment = model.Header.ABCDComment ?? "";

            string nextTrainingObjective = model.Header.NextTrainingObjective ?? "";
            string comment = model.Header.Comment ?? "";


            #region Header

            if (model.Header.AssessmentFor != null)
            {
                if (abcdnextTrainingObjective.Trim().Split(' ').Length < 8 ||
                        abcdnextTrainingObjective.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Header.ABCDNextTrainingObjective", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Header.ABCDNextTrainingObjective");
                }
            }
            if (model.Header.AssessmentFor != null)
            {
                if (abcdcomment.Trim().Split(' ').Length < 8 ||
                       abcdcomment.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Header.ABCDComment", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Header.ABCDComment");
                }
            }
            if (comment.Trim().Split(' ').Length < 8 ||
                   comment.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
            {
                ModelState.AddModelError("Header.Comment", errMes);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.Comment");
            }
            if (nextTrainingObjective.Trim().Split(' ').Length < 8 ||
                   nextTrainingObjective.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
            {
                ModelState.AddModelError("Header.NextTrainingObjective", errMes);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.NextTrainingObjective");
            }
            if (string.IsNullOrEmpty(training.Trim()))
            {
                ModelState.AddModelError("Header.Training", nullMsg);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.Training");
            }

            if (string.IsNullOrEmpty(pc.Trim()))
            {
                ModelState.AddModelError("Header.PC", nullMsg);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.PC");
            }

            if (string.IsNullOrEmpty(lppc.Trim()))
            {
                ModelState.AddModelError("Header.LPPC", nullMsg);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.LPPC");
            }

            if (string.IsNullOrEmpty(mark.Trim()))
            {
                ModelState.AddModelError("Header.Mark", nullMsg);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.Mark");
            }
            #endregion
            #region TrainingProcess
            if (model.Header.AssessmentFor != null)
            {
                for (int i = 0; i < model.TrainingProcess.Count; i++)
                {
                    string note = model.TrainingProcess[i].Note ?? "";
                    string pros = model.TrainingProcess[i].Pros ?? "";
                    string cons = model.TrainingProcess[i].Cons ?? "";

                    if (note.Trim().Split(' ').Length < 8 ||
                        note.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                    {
                        ModelState.AddModelError("TrainingProcess[" + i + "].Note", errMes);
                        result = false;
                    }
                    else
                    {
                        ModelState.Remove("TrainingProcess[" + i + "].Note");
                    }

                    if (pros.Trim().Split(' ').Length < 8 ||
                        pros.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                    {
                        ModelState.AddModelError("TrainingProcess[" + i + "].Pros", errMes);
                        result = false;
                    }
                    else
                    {
                        ModelState.Remove("TrainingProcess[" + i + "].Pros");
                    }

                    if (cons.Trim().Split(' ').Length < 8 ||
                        cons.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                    {
                        ModelState.AddModelError("TrainingProcess[" + i + "].Cons", errMes);
                        result = false;
                    }
                    else
                    {
                        ModelState.Remove("TrainingProcess[" + i + "].Cons");
                    }
                }
            }
            #endregion
            #region DailyWorks
            for (int i = 0; i < model.DailyWorks.Count; i++)
            {
                string note = model.DailyWorks[i].Note ?? "";
                string pros = model.DailyWorks[i].Pros ?? "";
                string cons = model.DailyWorks[i].Cons ?? "";

                if (note.Trim().Split(' ').Length < 8 ||
                    note.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("DailyWorks[" + i + "].Note", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("DailyWorks[" + i + "].Note");
                }
                if (pros.Trim().Split(' ').Length < 8 ||
                       pros.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("DailyWorks[" + i + "].Pros", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("DailyWorks[" + i + "].Pros");
                }

                if (cons.Trim().Split(' ').Length < 8 ||
                    cons.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("DailyWorks[" + i + "].Cons", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("DailyWorks[" + i + "].Cons");
                }
            }
            #endregion
            #region ToolsSM
            for (int i = 0; i < model.ToolsSM.Count; i++)
            {
                string note = model.ToolsSM[i].Note ?? "";
                string pros = model.ToolsSM[i].Pros ?? "";
                string cons = model.ToolsSM[i].Cons ?? "";

                if (note.Trim().Split(' ').Length < 8 ||
                    note.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("ToolsSM[" + i + "].Note", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("ToolsSM[" + i + "].Note");
                }

                if (cons.Trim().Split(' ').Length < 8 ||
                    cons.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("ToolsSM[" + i + "].Cons", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("ToolsSM[" + i + "].Cons");
                }

                if (pros.Trim().Split(' ').Length < 8 ||
                   pros.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("ToolsSM[" + i + "].Pros", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("ToolsSM[" + i + "].Pros");
                }
            }
            #endregion
            #region Step
            for (int i = 0; i < model.Steps.Count; i++)
            {
                string note = model.Steps[i].Note ?? "";
                string pros = model.Steps[i].Pros ?? "";
                string cons = model.Steps[i].Cons ?? "";

                if (note.Trim().Split(' ').Length < 8 ||
                    note.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Steps[" + i + "].Note", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Steps[" + i + "].Note");
                }

                if (cons.Trim().Split(' ').Length < 8 ||
                    cons.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Steps[" + i + "].Cons", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Steps[" + i + "].Cons");
                }

                if (pros.Trim().Split(' ').Length < 8 ||
                   pros.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("Steps[" + i + "].Pros", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("Steps[" + i + "].Pros");
                }
            }
            #endregion
            #region UpdateAndArchive
            for (int i = 0; i < model.UpdateAndArchive.Count; i++)
            {
                string note = model.UpdateAndArchive[i].Note ?? "";
                string pros = model.UpdateAndArchive[i].Pros ?? "";
                string cons = model.UpdateAndArchive[i].Cons ?? "";
                if (cons.Trim().Split(' ').Length < 8 ||
                    cons.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("UpdateAndArchive[" + i + "].Cons", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("UpdateAndArchive[" + i + "].Cons");
                }

                if (pros.Trim().Split(' ').Length < 8 ||
                   pros.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("UpdateAndArchive[" + i + "].Pros", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("UpdateAndArchive[" + i + "].Pros");
                }
                if (note.Trim().Split(' ').Length < 8 ||
                    note.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
                {
                    ModelState.AddModelError("UpdateAndArchive[" + i + "].Note", errMes);
                    result = false;
                }
                else
                {
                    ModelState.Remove("UpdateAndArchive[" + i + "].Note");
                }
            }
            #endregion



            return result;
        }

        private bool ValidateNoTrainingAssessmentModel(NoAssessmentModel model)
        {
            bool result = true;
            string errMes = Utility.Phrase("ErrMinimumWords");

            string results = model.Header.Results ?? "";
            string works = model.Header.Works ?? "";

            if (results.Trim().Split(' ').Length < 8 ||
                    results.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
            {
                ModelState.AddModelError("Header.Results", errMes);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.Results");
            }

            if (works.Trim().Split(' ').Length < 8 ||
                    works.Trim().Split(' ').Where(x => !string.IsNullOrEmpty(x.Trim())).Count() < 8)
            {
                ModelState.AddModelError("Header.Works", errMes);
                result = false;
            }
            else
            {
                ModelState.Remove("Header.Works");
            }

            return result;
        }
        //[HttpPost]
        //public ActionResult SaveNoTraining(NoAssessmentModel model)
        //{
        //   // model.Header.UniqueID = Utility.IntParse(EditorExtension.GetValue<string>("UniqueID"));
        //    HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
        //    if (ValidateNoTrainingAssessmentModel(model))
        //    {
        //        try
        //        {

        //            HammerDataProvider.SaveNoTraining(model);
        //            return Json(Utility.Phrase("SaveOki"));
        //        }
        //        catch (Exception)
        //        {
        //            return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
        //        }
        //    }
        //    else
        //    {

        //        return PartialView("NoTraningAssessmentPartial", model);
        //    }
        //}
        [HttpPost]
        public ActionResult SaveNoTraining(string UniqueID, string Works, string Results)
        {
            NoAssessmentModel model = new NoAssessmentModel();
            model.Header.Works = Utility.StringParse(EditorExtension.GetValue<string>("Works"));
            model.Header.Results = Utility.StringParse(EditorExtension.GetValue<string>("Results"));
            model.Header.UserID = User.Identity.Name;
            if (UniqueID != null)
            {
                model.Header.UniqueID = Convert.ToInt32(UniqueID);
                Appointment ap = HammerDataProvider.GetAppointmentById(model.Header.UniqueID.Value);
                if (ap != null)
                    model.Header.AssessmentDate = HammerDataProvider.GetAppointmentById(model.Header.UniqueID.Value).StartDate.Value;
            }
            //model.Header.Results = Results;
            //model.Header.Works = Works;
            //model.Header.AssessmentDate = Utility.DateTimeParse(Date);
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ValidateNoTrainingAssessmentModel(model))
            {
                try
                {
                    // model.Header.Results 
                    HammerDataProvider.SaveNoTraining(model);
                    return Json(Utility.Phrase("SaveOki"));
                }
                catch (Exception)
                {
                    return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
                }
            }
            else
            {
                return Json(Utility.Phrase("eCalendar_Validate_InputNullOrNotPass"));
                //return PartialView("NoTraningAssessmentPartial", model);
            }
        }

        [HttpPost]
        public ActionResult SubmitNoTraining(string UniqueID, string Works, string Results)
        {
            NoAssessmentModel model = new NoAssessmentModel();
            model.Header.UserID = User.Identity.Name;
            if (UniqueID != null)
            {
                model.Header.UniqueID = Convert.ToInt32(UniqueID);
                Appointment ap = HammerDataProvider.GetAppointmentById(model.Header.UniqueID.Value);
                if (ap != null)
                    model.Header.AssessmentDate = HammerDataProvider.GetAppointmentById(model.Header.UniqueID.Value).StartDate.Value;
            }
            model.Header.Results = Results;
            model.Header.Works = Works;
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ValidateNoTrainingAssessmentModel(model))
            {
                try
                {
                    model.Header.Released = true;
                    HammerDataProvider.SaveNoTraining(model);
                    ViewData["SubmitSuccessfull"] = Utility.Phrase("SaveOki");
                    return PartialView("SubmitSuccessfully");
                }
                catch (Exception)
                {
                    return Json(Utility.Phrase("AnErrorOccurredWhileProcessing"));
                }
            }
            else
            {
                return PartialView("NoTraningAssessmentPartial", model);
            }
        }

        [HttpPost]
        public ActionResult DownloadTemplateSMTraining(SMAssessmentModel model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string templatePath = string.Empty;
            if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "vi-VN")
            {
                templatePath = Server.MapPath("/Templates/Assessments/SR_Assessment_Template.xlsx");               
            }
            else if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "en-US")
            {
                 //templatePath = Server.MapPath("/Templates/Assessments/No.xlsx");
                templatePath = Server.MapPath("/Templates/Assessments/SR_Assessment_Template_En.xlsx");   
                //templatePath += "No.xlsx";
            }
            else
            {
                templatePath += "No.xlsx";
            }
           // string templatePath = Server.MapPath("/Templates/Assessments/SR_Assessment_Template.xlsx");
            try
            {
                return File(Util.GenSMTrainingTemplate(templatePath, model), "application/octet-stream",
                    string.Format(Constants.SM_TRAINING_ASSESSMENT_FILENAME, model.Header.AssessmentFor,
                    model.Header.AssessmentDate.ToString("yyyyMMdd"), model.Header.UniqueID.Value));
            }
            catch
            {
                //return null;
                return Json(Utility.Phrase("eCalendar_FileDoesNotExist"));
                //return PartialView("SMTrainingAssessmentHeaderPartial", model);
                ///return PartialView("DownloadFileError");
            }
        }

        [HttpPost]
        public ActionResult DownloadTemplateSSTraining(AssessmentModel model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string templatePath = Server.MapPath("/Templates/Assessments/SS_Assessment_Template.xlsx");
            return File(Util.GenSSTrainingTemplate(templatePath, model), "application/octet-stream",
                string.Format(Constants.SS_TRAINING_ASSESSMENT_FILENAME, model.Header.AssessmentFor,
                model.Header.AssessmentDate.ToString("yyyyMMdd"), model.Header.UniqueID.Value));
        }
    }
}
