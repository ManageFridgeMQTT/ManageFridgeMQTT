using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eRoute.Models;
using eRoute.Models.ViewModel;
using DMSERoute.Helpers;
using eRoute.Filters;
using System.Net.Security;
using System.Net;
using WebMatrix.WebData;
using System.Web.Security;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using DevExpress.Web.Mvc;
using DevExpress.XtraPivotGrid;
using System.Web.UI.WebControls;
using DevExpress.Utils;
using DevExpress.Data.PivotGrid;
using System.Transactions;
using System.Drawing.Printing;
using DevExpress.Web;

namespace eRoute.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    [LogAndRedirectOnError]
    public class UserController : Controller
    {
        string username = SessionHelper.GetSession<string>("UserName");

        #region User management
        //
        // GET: /User/

        [Authorize]
        [ActionAuthorize("User_Index", true)]
        public ActionResult Index()
        {
            PermissionHelper.CheckPermissionByFeature("User_Index", this);
            List<pp_UserManagementResult> listUser = Global.Context.pp_UserManagement(SessionHelper.GetSession<string>("UserName")).ToList();
            var model = Utility.CopyList<UserVM>(listUser);
            SessionHelper.SetSession<List<UserVM>>("User_Index", model);
            return View(model);
        }
        public ActionResult UserPartial()
        {
            var model = SessionHelper.GetSession<List<UserVM>>("User_Index");
            return PartialView("UserPartial", model);
        }
        public ActionResult ActionUserTable(string username, string action)
        {
            if (!String.IsNullOrEmpty(username))
            {
                username = username.Substring(0, username.IndexOf("|"));
            }
            var model = SessionHelper.GetSession<List<UserVM>>("User_Index");
            #region ResetPassEmail
            if (action == "ResetPass")
            {
                if (WebSecurity.UserExists(username))
                {
                    int userID = WebSecurity.GetUserId(username);
                    var userPass = Global.Context.webpages_Memberships.FirstOrDefault(a => a.UserId == userID);
                    if (userPass != null)
                    {
                        userPass.IsConfirmed = true;
                        Global.Context.SubmitChanges();

                        //ControllerHelper.LogUserAction("User", "ResetPass", username);
                        SendMailResetPass(username);
                    }
                }
                else
                {
                    ViewData["StatusMessage"] = "Tên đăng nhập này không tồn tại.";
                }
            }
            #endregion

            #region Active
            else if (action == "Active")
            {
                int userID = WebSecurity.GetUserId(username);

                if (userID > 0)
                {
                    var userPass = Global.Context.webpages_Memberships.FirstOrDefault(a => a.UserId == userID);
                    if (userPass != null)
                    {
                        userPass.IsConfirmed = true;
                        Global.Context.SubmitChanges();
                        UserVM userReplace = model.FirstOrDefault(x => x.UserName.Equals(username));
                        userReplace.IsConfirmed = true;
                        model = UpdateUserForDataSession(model, userReplace);
                        SessionHelper.SetSession<List<UserVM>>("User_Index", model);

                        ViewData["StatusMessage"] = "Active người dùng " + username + " thành công.";
                    }
                }
                else
                {
                    ViewData["StatusMessage"] = "Tên đăng nhập không tồn tại trong hệ thống";
                }
            }
            #endregion

            #region Inactive
            else if (action == "Inactive")
            {
                int userID = WebSecurity.GetUserId(username);

                if (userID > 0)
                {
                    var userPass = Global.Context.webpages_Memberships.FirstOrDefault(a => a.UserId == userID);
                    if (userPass != null)
                    {
                        userPass.IsConfirmed = false;
                        Global.Context.SubmitChanges();
                        UserVM userReplace = model.FirstOrDefault(x => x.UserName.Equals(username));
                        userReplace.IsConfirmed = false;
                        model = UpdateUserForDataSession(model, userReplace);
                        SessionHelper.SetSession<List<UserVM>>("User_Index", model);

                        ViewData["StatusMessage"] = "Inactive người dùng " + username + " thành công.";
                    }
                }
                else
                {
                    ViewData["StatusMessage"] = "Tên đăng nhập không tồn tại trong hệ thống";
                }
            }
            #endregion

            return PartialView("UserPartial", model);
        }
        [HttpPost]
        [ActionAuthorize("User_AddNewUser")]
        public ActionResult AddNewUser(UserVM user)
        {
            var model = SessionHelper.GetSession<List<UserVM>>("User_Index");
            try
            {
                if (ModelState.IsValid)
                {
                    ProcessString(user);
                    var sf = Global.Context.DMSSalesForces.FirstOrDefault(d => d.LoginID == user.UserName && d.Active == true);
                    var userInfo = Global.Context.UserProfileInfos.Where(x => x.Email == user.Email).FirstOrDefault();
                    if(userInfo == null)
                    {
                        if (sf != null)
                        {
                            if (!WebSecurity.UserExists(user.UserName))
                            {
                                string password = Membership.GeneratePassword(Constant.LengthPassword, Constant.NumberOfSpecialCharacters);
                                var sfh = Global.Context.Distributors.SingleOrDefault(d => d.LoginID == user.UserName);
                                //if (dist != null && !string.IsNullOrEmpty(dist.Email))
                                //{
                                string confirmationToken = WebSecurity.CreateUserAndAccount(user.UserName, password, null, true);
                                if (WebSecurity.GetUserId(user.UserName) > 0)
                                {
                                    if (string.IsNullOrEmpty(sf.Email))
                                        sf.Email = user.Email;
                                    if (InitSendMail(sf.Email, "", user.UserName, password, confirmationToken, Constant.EmailCreateUserHTML, Constant.SubjectCreate, true))
                                    {
                                        #region Insert RoleUser
                                        RoleUser ru = new RoleUser()
                                        {
                                            UserID = WebSecurity.GetUserId(user.UserName),
                                            RoleID = user.RoleID
                                        };
                                        Global.Context.RoleUsers.InsertOnSubmit(ru);

                                        #endregion
                                        #region Add UserProfile
                                        //string phone = string.Empty;
                                        //if (string.IsNullOrEmpty(sf.Phone1))
                                        //{
                                        //    if (string.IsNullOrEmpty(sf.Phone2))
                                        //    {
                                        //        phone = user.Email;
                                        //    }
                                        //    else
                                        //    {
                                        //        phone = sf.Phone2;
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    phone = sf.Phone1;
                                        //}
                                        //user.Phone = phone;
                                        user.Email = sf.Email;
                                        UserProfileInfo pro = new UserProfileInfo()
                                        {
                                            FullName = user.FullName,
                                            LoginID = user.UserName,
                                            Email = user.Email,
                                            Phone = user.Phone,
                                            ApplicationCD = "DMS",
                                            Comment = user.UserName
                                        };
                                        Global.Context.UserProfileInfos.InsertOnSubmit(pro);
                                        #endregion
                                        Global.Context.SubmitChanges();
                                        ViewData["StatusMessage"] = Utility.Phrase("Mess_CreatedUser") + " " + user.UserName + " " + Utility.Phrase("Successfull"); // "Đăng kí người dùng " + user.UserName + " thành công.";
                                        model.Add(user);
                                        SessionHelper.SetSession<List<UserVM>>("User_Index", model);
                                    }
                                    else
                                    {
                                        ViewData["EditError"] = Utility.Phrase("Mess_SendEmailFail");//"Không thể gửi mail thông báo cho người dùng. Vui lòng kiểm tra lại địa chỉ email hoặc liên hệ với nhà quản trị.";
                                    }
                                }
                                else
                                {
                                    ViewData["EditError"] = Utility.Phrase("Mess_CreatedUserFaild_ContactAdmin"); //"Không thể đăng kí người dùng. Vui lòng liên hệ nhà quản trị.";
                                }
                                //}
                                //else
                                //{
                                //    ViewData["EditError"] = Utility.Phrase("Mess_UserNotDistributor_EmailIsEmty");//"Người dùng không có trong danh mục Distributor hoặc email của người dùng này chưa có.";
                                //}
                            }
                            else
                            {
                                ViewData["EditError"] = Utility.Phrase("Mess_AccountIsExsit"); //"Tài khoản này đã tồn tại.";
                            }
                        }
                        else
                        {
                            if (!WebSecurity.UserExists(user.UserName))
                            {
                                string password = Membership.GeneratePassword(Constant.LengthPassword, Constant.NumberOfSpecialCharacters);
                                string confirmationToken = WebSecurity.CreateUserAndAccount(user.UserName, password, null, true);
                                if (InitSendMail(user.Email, "", user.UserName, password, confirmationToken, Constant.EmailCreateUserHTML, Constant.SubjectCreate, true))
                                {
                                    #region Insert RoleUser
                                    RoleUser ru = new RoleUser()
                                    {
                                        UserID = WebSecurity.GetUserId(user.UserName),
                                        RoleID = user.RoleID
                                    };
                                    Global.Context.RoleUsers.InsertOnSubmit(ru);

                                    #endregion
                                    #region Add UserProfile
                                    UserProfileInfo pro = new UserProfileInfo()
                                    {
                                        FullName = user.FullName,
                                        LoginID = user.UserName,
                                        Email = user.Email,
                                        Phone = user.Phone,
                                        ApplicationCD = user.ApplicationCD
                                    };
                                    Global.Context.UserProfileInfos.InsertOnSubmit(pro);
                                    #endregion
                                    Global.Context.SubmitChanges();
                                    ViewData["StatusMessage"] = Utility.Phrase("Mess_CreatedUser") + " " + user.UserName + " " + Utility.Phrase("Successfull"); // "Đăng kí người dùng " + user.UserName + " thành công.";
                                    model.Add(user);
                                    SessionHelper.SetSession<List<UserVM>>("User_Index", model);
                                }
                            }
                            else
                            {
                                ViewData["EditError"] = Utility.Phrase("Mess_AccountIsExsit"); //"Tài khoản này đã tồn tại.";
                            }
                        }
                    }
                    else
                    {
                        ViewData["EditError"] = Utility.Phrase("Mess_EmailIsExsit"); //"Email này đã tồn tại.";
                    }

                }
                else
                {
                    ViewData["EditError"] = Utility.Phrase("Mess_FieldIsRequired");//"Bạn chưa chọn nhóm quyền.";
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            return PartialView("UserPartial", model);
        }
        [HttpPost]
        [ActionAuthorize("User_UpdateUser")]
        public ActionResult UpdateUser(UserVM user)
        {
            var model = SessionHelper.GetSession<List<UserVM>>("User_Index");
            if (!user.ApplicationCD.Equals("DMS", StringComparison.InvariantCultureIgnoreCase))
            {
                ProcessString(user);

                if (user.Email.Length > 0 && user.FullName.Length > 0 && user.UserName.Length > 0)
                {
                    int userID = WebSecurity.GetUserId(user.UserName);
                    UserProfileInfo ufi = Global.Context.UserProfileInfos.FirstOrDefault(a => a.LoginID == user.UserName);
                    if (ufi != null)
                    {
                        ufi.FullName = user.FullName;
                        ufi.Phone = user.Phone;
                        ufi.Email = user.Email;
                        ufi.LoginID = user.UserName;
                        Global.Context.SubmitChanges();
                    }
                    //else
                    //{
                    //    eRoute.Models.UserProfileInfo ufiInsert = new eRoute.Models.UserProfileInfo();
                    //    ufiInsert.FullName = user.FullName;
                    //    ufiInsert.Phone = user.Phone;
                    //    ufiInsert.Email = user.Email;
                    //    ufiInsert.LoginID = user.Username;

                    //    Global.Context.UserProfileInfos.InsertOnSubmit(ufiInsert);
                    //    Global.Context.SubmitChanges();

                    //    //ControllerHelper.LogUserAction("User", "Update", user.Username);
                    //}

                    #region Delete OldRole
                    List<RoleUser> listRUOld = Global.Context.RoleUsers.Where(a => a.UserID == userID).ToList();
                    foreach (RoleUser item in listRUOld)
                    {
                        Global.Context.RoleUsers.DeleteOnSubmit(item);
                    }
                    Global.Context.SubmitChanges();
                    #endregion

                    #region Add Role User
                    RoleUser ru = new RoleUser()
                    {
                        UserID = userID,
                        RoleID = user.RoleID
                    };
                    Global.Context.RoleUsers.InsertOnSubmit(ru);
                    Global.Context.SubmitChanges();
                    //ViewBag.StatusMessage = "Cập nhật người dùng " + model.UserName + " thành công.";
                    #endregion

                    model = UpdateUserForDataSession(model, user);
                    SessionHelper.SetSession<List<UserVM>>("User_Index", model);
                }
                else
                {
                    ViewData["EditError"] = "Xin vui lòng nhập đầy đủ thông tin tài khoản, email và họ tên.";
                }
            }
            else
            {
                ViewData["EditError"] = "Bạn không được phép cập nhật user ngoài ERoute";
            }
            return PartialView("UserPartial", model);
        }

        public ActionResult UserExportExcel()
        {
            var model = SessionHelper.GetSession<List<UserVM>>("User_Index");
            return GridViewExtension.ExportToXlsx(UserExportExcelSetting(), model);
        }
        public ActionResult UserExportPDF()
        {
            var model = SessionHelper.GetSession<List<UserVM>>("User_Index");
            return GridViewExtension.ExportToPdf(UserExportExcelSetting(), model);
        }
        private static GridViewSettings UserExportExcelSetting()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "UserList";
            settings.KeyFieldName = "UserName;Email";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };
            settings.Columns.Add(field =>
            {
                field.FieldName = "RoleID";
                field.Caption = Utility.Phrase("Role") + " *";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
                field.ColumnType = MVCxGridViewColumnType.ComboBox;
                field.CellStyle.CssClass = "border-none";
                var comboBoxProperties = field.PropertiesEdit as ComboBoxProperties;
                comboBoxProperties.DataSource = Global.Context.Roles.ToList();
                comboBoxProperties.TextField = "RoleName";
                comboBoxProperties.ValueField = "ID";
                comboBoxProperties.ValueType = typeof(Int32);
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "UserName";
                field.Caption = Utility.Phrase("UserName") + " *"; ;
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;

                field.EditFormSettings.Visible = DefaultBoolean.True;
                field.ReadOnly = false;
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "FullName";
                field.Caption = Utility.Phrase("Name");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Email";
                field.Caption = Utility.Phrase("Email") + " *";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Phone";
                field.Caption = Utility.Phrase("Phone");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "IsConfirmed";
                field.Caption = Utility.Phrase("Status");
                field.ColumnType = MVCxGridViewColumnType.CheckBox;
                field.CellStyle.HorizontalAlign = HorizontalAlign.Center;
                field.EditCellStyle.HorizontalAlign = HorizontalAlign.Left;
                field.EditFormSettings.Visible = DefaultBoolean.True;
                field.ReadOnly = false;

            });
            settings.Columns.Add(col =>
            {
                col.FieldName = "ApplicationCD";
                col.ColumnType = MVCxGridViewColumnType.ComboBox;
                col.Caption = Utility.Phrase("AccountType") + " *";
                //col.Width = System.Web.UI.WebControls.Unit.Percentage(5);
                var cb = col.PropertiesEdit as ComboBoxProperties;
                cb.EnableCallbackMode = true;

                cb.Items.Add(new ListEditItem()
                {
                    Index = 0,
                    Text = "DMS",
                    Value = "DMS"
                });
                cb.Items.Add(new ListEditItem()
                {
                    Index = 1,
                    Text = "ETOOLS",
                    Value = "ETOOLS"
                });
            });
            return settings;
        }
        [NonAction]
        public static List<UserVM> UpdateUserForDataSession(List<UserVM> list, UserVM userReplace)
        {
            int index = list.FindIndex(x => x.UserName.Equals(userReplace.UserName));
            if (index > -1)
            {
                list[index] = userReplace;
            }
            return list;
        }

        private void ProcessString(UserVM user)
        {
            if (!string.IsNullOrEmpty(user.UserName))
            {
                user.UserName = user.UserName.Trim().Replace("\"", "");
            }
            else
            {
                user.UserName = string.Empty;
            }
            if (!string.IsNullOrEmpty(user.Email))
            {
                user.Email = user.Email.Trim().Replace("\"", "");
            }
            else
            {
                user.Email = string.Empty;
            }
            if (!string.IsNullOrEmpty(user.FullName))
            {
                user.FullName = user.FullName.Trim().Replace("\"", "");
            }
            else
            {
                user.FullName = string.Empty;
            }
            if (!string.IsNullOrEmpty(user.Phone))
            {
                user.Phone = user.Phone.Trim().Replace("\"", "");
            }
            else
            {
                user.Phone = string.Empty;
            }
        }

        private void SendMailResetPass(string userName)
        {
            int userID = WebSecurity.GetUserId(userName);
            //Get user email
            string email = string.Empty;
            string userFullName = string.Empty;

            Distributor d = Global.Context.Distributors.FirstOrDefault(a => a.LoginID == userName);
            if (d != null)
            {
                email = d.Email;
                userFullName = d.DistributorName;
                userName = d.LoginID;
            }
            else
            {
                DMSSalesForce sf = Global.Context.DMSSalesForces.FirstOrDefault(a => a.LoginID == userName && a.Active == true);
                if (sf != null)
                {
                    email = sf.Email;
                    userFullName = sf.EmployeeName;
                    userName = sf.LoginID;
                }
                else
                {
                    UserProfileInfo UserProfileInfo = Global.Context.UserProfileInfos.FirstOrDefault(a => a.LoginID == userName);
                    if (UserProfileInfo != null)
                    {
                        email = UserProfileInfo.Email;
                        userFullName = UserProfileInfo.FullName;
                        userName = UserProfileInfo.LoginID;
                    }
                }
            }

            if (!string.IsNullOrEmpty(email))
            {
                string token = WebSecurity.GeneratePasswordResetToken(userName, 10080); //token có hiệu lực 7 ngày   
                ViewData["StatusMessage"] = "Email reset mật khẩu người dùng " + userName + " đã được gửi thành công.";
                InitSendMail(email, userFullName, userName, string.Empty, token, Constant.EmailResetPassHTML, Constant.SubjectReset, false);
            }
            else
            {
                ViewData["StatusMessage"] = "Không tìm thấy Email của user " + userName + " trong hệ thống, không thể reset pass.";
            }
        }

        private bool InitSendMail(string email, string userFullName, string userName, string password, string confirmationToken, string emailTemplate, string subject, bool isAddNew)
        {
            try
            {
                string path = HttpContext.Server.MapPath(emailTemplate);
                string body = System.IO.File.ReadAllText(path);

                body = body.Replace("{WebUrl}", Utility.GetBaseUrl());
                body = body.Replace("{FullName}", userFullName);
                body = body.Replace("{Account}", userName);
                body = body.Replace("{Password}", password);
                body = body.Replace("{TokenConfirmation}", confirmationToken);

                SendMail(email, subject, body);

                var obj = new { Email = email, UserName = userName, Password = password, ConfirmationToken = confirmationToken };
                CustomLog.LogError("LogSendMail", obj);
                return true;
            }
            catch (Exception ex)
            {
                var obj = new { Email = email, UserName = userName, Password = password, ConfirmationToken = confirmationToken };
                CustomLog.LogError("LogErrorSendMail", ex, obj);
                if (!isAddNew)
                {
                    ViewData["StatusMessage"] = "Không thể gửi mail thông báo cho người dùng. Vui lòng kiểm tra lại địa chỉ email hoặc liên hệ với nhà quản trị.";
                }
                return false;
            }
        }

        private void SendMail(string toMail, string subject, string body)
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

        #region ReportUserActionLogTerritory
        [ActionAuthorize("User_ReportUserActionLogTerritory", true)]
        public ActionResult ReportUserActionLogTerritory(string strFromDate, string strToDate, string act, FormCollection formParam)
        {
            ReportUserActionLogTerritoryVM model = new ReportUserActionLogTerritoryVM();

            //Begin get
            //Set default value
            if (string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Today;
                model.ToDate = DateTime.Today;
                model.strFromDate = model.FromDate.ToShortPattern();
                model.strToDate = model.ToDate.ToShortPattern();
            }
            else
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.strFromDate = model.FromDate.ToShortPattern();
                model.strToDate = model.ToDate.ToShortPattern();
                model.userID = Utility.IntParse(EditorExtension.GetValue<int>("userID"));
            }

            model.ListUser = new List<pp_GetUserInfoResult>();
            model.ListUser.AddRange(Global.Context.pp_GetUserInfo(username, string.Empty).OrderBy(a => a.FullName).ToList());

            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ListItem = Global.Context.pp_ReportUserActionLogTerritory(model.FromDate, model.ToDate, model.userID).ToList();
            }
            else
            {
                model.ListItem = new List<pp_ReportUserActionLogTerritoryResult>();
            }

            SessionHelper.SetSession<ReportUserActionLogTerritoryVM>("ReportUserActionLogTerritory", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportUserActionLogTerritoryExport");
            }

            if (act == "ExportExcelRawData")
            {
                return RedirectToAction("ReportUserActionLogTerritoryExportRAWData");
            }

            return View(model);
        }

        public ActionResult ReportUserActionLogTerritoryPartial()
        {
            return PartialView("ReportUserActionLogTerritoryPartial", SessionHelper.GetSession<ReportUserActionLogTerritoryVM>("ReportUserActionLogTerritory").ListItem);
        }

        #region ReportUserActionLogTerritoryExport
        public ActionResult ReportUserActionLogTerritoryExport()
        {
            var model = SessionHelper.GetSession<ReportUserActionLogTerritoryVM>("ReportUserActionLogTerritory").ListItem;

            //ControllerHelper.LogUserAction("Home", "ReportUserActionLogTerritoryExport", null);

            return PivotGridExtension.ExportToXlsx(ReportUserActionLogTerritorySettings(true), model);
        }

        public static PivotGridSettings ReportUserActionLogTerritorySettings(bool isExcel)
        {
            PivotGridSettings settings = new PivotGridSettings();

            settings.Name = "UserActionLogTerritory";
            settings.CallbackRouteValues = new { Controller = "User", Action = "ReportUserActionLogTerritoryPartial" };
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowDragInCustomizationForm = false;
            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ColumnTotalsLocation = PivotTotalsLocation.Far;
            //settings.OptionsView.ShowGrandTotalsForSingleValues = true;
            settings.OptionsView.ShowRowTotals = true;
            settings.OptionsView.ShowTotalsForSingleValues = true;

            settings.Groups.Add("UserName  -RoleName - FullName  -Email - Phone - Date - Page - Action");

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 0;
                field.Caption = "UserName";
                field.FieldName = "UserName";
                field.CellStyle.VerticalAlign = VerticalAlign.Top;
                field.SummaryFilter.Mode = PivotSummaryFilterMode.SpecificLevel;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.AutomaticTotals;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 1;
                field.Caption = "Quyền";
                field.FieldName = "RoleName";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 2;
                field.Caption = "Họ & tên";
                field.FieldName = "FullName";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            if (isExcel)
            {
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                //field.AreaIndex = 2;
                field.Caption = "Region";
                    field.FieldName = "RegionName";
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                //field.AreaIndex = 2;
                field.Caption = "Area";
                    field.FieldName = "AreaName";
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                //field.AreaIndex = 2;
                field.Caption = "DistributorCode";
                    field.FieldName = "DistributorCode";
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                //field.AreaIndex = 2;
                field.Caption = "DistributorName";
                    field.FieldName = "DistributorName";
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                });
            }
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 3;
                field.Caption = "Email";
                field.FieldName = "Email";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 4;
                field.Caption = "Phone";
                field.FieldName = "Phone";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 5;
                field.Caption = "Ngày sử dụng";
                field.FieldName = "Date";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 5;
                field.Caption = "Thời gian vào đầu tiên";
                field.FieldName = "FisrtClick";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 5;
                field.Caption = "Thời gian vào cuối cùng";
                field.FieldName = "LastClick";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 6;
                field.Caption = "Page";
                field.FieldName = "Page";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 7;
                field.Caption = "Action";
                field.FieldName = "Action";
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });



            //DATA AREA
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                //field.AreaIndex = 0;
                field.FieldName = "Click";
                field.Caption = "Click";
                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });
            settings.OptionsPager.RowsPerPage = 0;
            return settings;
        }
        #endregion

        public ActionResult ReportUserActionLogTerritoryExportRAWData()
        {
            var model = SessionHelper.GetSession<ReportUserActionLogTerritoryVM>("ReportUserActionLogTerritory").ListItem;

            //ControllerHelper.LogUserAction("Home", "ReportUserActionLogTerritoryExport", null);

            return GridViewExtension.ExportToXlsx(ReportUserActionLogTerritorySettingsRAWData(), model);
        }

        private static GridViewSettings ReportUserActionLogTerritorySettingsRAWData()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportUserActionLogTerritory";
            settings.KeyFieldName = "UserName";
            settings.CallbackRouteValues = new { Controller = "User", Action = "ReportUserActionLogTerritoryPartial" };
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;

            settings.Columns.Add(field =>
            {
                field.Caption = "UserName";
                field.FieldName = "UserName";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Quyền";
                field.FieldName = "RoleName";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Họ & tên";
                field.FieldName = "FullName";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Region";
                field.FieldName = "RegionName";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Area";
                field.FieldName = "AreaName";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "DistributorCode";
                field.FieldName = "DistributorCode";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "DistributorName";
                field.FieldName = "DistributorName";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Email";
                field.FieldName = "Email";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Phone";
                field.FieldName = "Phone";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Ngày sử dụng";
                field.FieldName = "Date";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Thời gian vào đầu tiên";
                field.FieldName = "FisrtClick";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Thời gian vào cuối cùng";
                field.FieldName = "LastClick";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });

            settings.Columns.Add(field =>
            {
                field.Caption = "Page";
                field.FieldName = "Page";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "Action";
                field.FieldName = "Action";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });

            //DATA AREA
            settings.Columns.Add(field =>
            {
                field.FieldName = "Click";
                field.Caption = "Click";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });

            return settings;
        }
        #endregion

        #region ReportUserUseMobility
        [Authorize]
        [ActionAuthorize("User_ReportUserUseMobility", true)]
        public ActionResult ReportUserUseMobility(string strFromDate, string strToDate, string act, FormCollection formParam)
        {
            ReportUserUseMobilityVM model = new ReportUserUseMobilityVM();

            #region Validate Data
            try
            {
                //Begin get
                //Set default value
                if (string.IsNullOrEmpty(strFromDate))
                {
                    model.FromDate = DateTime.Today;
                    model.ToDate = DateTime.Today;
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.strToDate = model.ToDate.ToShortPattern();
                }
                else
                {
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.ToDate = Utility.DateTimeParse(strToDate);
                    model.strToDate = model.ToDate.ToShortPattern();
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            if (
                !string.IsNullOrEmpty(strFromDate)
                )
            {
                model.listItem = Global.Context.pp_ReportUserUseMobility(model.FromDate, model.ToDate, username).ToList();
            }
            else
            {
                model.listItem = new List<pp_ReportUserUseMobilityResult>();
            }
            SessionHelper.SetSession<ReportUserUseMobilityVM>("ReportUserUseMobility", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportUserUseMobilityExport");
            }

            if (act == "ExportExcelRawData")
            {
                return RedirectToAction("ReportUserUseMobilityExportRAWData");
            }

            return View(model);
        }

        [Authorize]
        public PartialViewResult ReportUserUseMobilityPartial()
        {
            List<pp_ReportUserUseMobilityResult> model = SessionHelper.GetSession<ReportUserUseMobilityVM>("ReportUserUseMobility").listItem;
            return PartialView("ReportUserUseMobilityPartial", model);
        }

        #region ReportSalemanKPIExport
        public ActionResult ReportUserUseMobilityExport()
        {
            var model = SessionHelper.GetSession<ReportUserUseMobilityVM>("ReportUserUseMobility").listItem;

            //ControllerHelper.LogUserAction("Home", "ReportUserUseMobilityExport", null);

            return PivotGridExtension.ExportToXlsx(ReportUserUseMobilitySettings(), model);
        }

        private static PivotGridSettings ReportUserUseMobilitySettings()
        {
            PivotGridSettings settings = new PivotGridSettings();

            settings.Name = "ReportUserUseMobility";
            settings.CallbackRouteValues = new { Controller = "Home", Action = "ReportUserUseMobilityPartial" };
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowDragInCustomizationForm = false;
            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ColumnTotalsLocation = PivotTotalsLocation.Far;
            settings.OptionsView.ShowRowTotals = false;
            settings.OptionsView.ShowTotalsForSingleValues = false;
            settings.OptionsView.ShowColumnGrandTotals = false;


            settings.Groups.Add("EmployeeID - EmployeeName");


            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = "EmployeeID";
                field.FieldName = "EmployeeID";
                field.CellStyle.VerticalAlign = VerticalAlign.Top;
                field.SummaryFilter.Mode = PivotSummaryFilterMode.SpecificLevel;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 1;
                field.Caption = "EmployeeName";
                field.FieldName = "EmployeeName";
                field.CellStyle.VerticalAlign = VerticalAlign.Top;
                field.SummaryFilter.Mode = PivotSummaryFilterMode.SpecificLevel;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 2;
                field.FieldName = "SM";
                field.Caption = "NVBH";
                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 4;
                field.FieldName = "ASM";
                field.Caption = "ASM";
                field.CellStyle.VerticalAlign = VerticalAlign.Top;
                field.SummaryFilter.Mode = PivotSummaryFilterMode.SpecificLevel;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 5;
                field.FieldName = "Register";
                field.Caption = "Có đăng ký";
                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 6;
                field.FieldName = "IsSync";
                field.Caption = "Có đồng bộ";
                field.CellStyle.VerticalAlign = VerticalAlign.Top;
                field.SummaryFilter.Mode = PivotSummaryFilterMode.SpecificLevel;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.OptionsPager.RowsPerPage = 0;
            return settings;
        }
        #endregion

        public ActionResult ReportUserUseMobilityExportRAWData()
        {
            var model = SessionHelper.GetSession<ReportUserUseMobilityVM>("ReportUserUseMobility").listItem;

            //ControllerHelper.LogUserAction("Home", "ReportUserUseMobilityExport", null);

            return GridViewExtension.ExportToXlsx(ReportUserUseMobilitySettingsRAWData(), model);
        }

        private static GridViewSettings ReportUserUseMobilitySettingsRAWData()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportUserUseMobility";
            settings.KeyFieldName = "EmployeeID";
            settings.CallbackRouteValues = new { Controller = "Home", Action = "ReportUserUseMobilityPartial" };
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;

            settings.Columns.Add(field =>
            {
                field.Caption = "EmployeeID";
                field.FieldName = "EmployeeID";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.Caption = "EmployeeName";
                field.FieldName = "EmployeeName";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "SM";
                field.Caption = "NVBH";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ASM";
                field.Caption = "ASM";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Register";
                field.Caption = "Có đăng ký";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "IsSync";
                field.Caption = "Có đồng bộ";
                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            return settings;
        }
        #endregion

        #region RoleFeatureAssignment
        [HttpGet]
        [ActionAuthorize("RoleFeatureAssignment", true)]
        [CompressFilter]
        public ActionResult RoleFeatureAssignment(int? roleID)
        {
            RoleFeatureAssignmentVM model = new RoleFeatureAssignmentVM();
            LoadRoleFeatureAssignmentData(Utility.IntParse(roleID), model);
            //model.strSuccess = Utility.Phrase("RoleFeatureAssignmentSaveOK");
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("RoleFeatureAssignment")]
        [CompressFilter]
        public ActionResult RoleFeatureAssignment(FormCollection formParam, string act)
        {
            int roleID = Utility.IntParse(EditorExtension.GetValue<int>("roleID"));
            RoleFeatureAssignmentVM model = new RoleFeatureAssignmentVM();
            if (act == "Save" && roleID > 0)
            {
                int[] selectedItems = CheckBoxListExtension.GetSelectedValues<int>("cbListFeature");
                SaveRoleFeatureAssignment(roleID, selectedItems);
                model.strSuccess = Utility.Phrase("RoleFeatureAssignmentSaveOK");
            }

            LoadRoleFeatureAssignmentData(Utility.IntParse(roleID), model);
            return View(model);
        }

        private void LoadRoleFeatureAssignmentData(int roleID, RoleFeatureAssignmentVM model)
        {
            //RoleFeatureAssignmentVM model = new RoleFeatureAssignmentVM();
            model.ListRole = Global.Context.Roles.OrderBy(a => a.RoleName).Distinct().ToList();
            model.roleID = Utility.IntParse(roleID);
            model.ListFeature = Global.Context.Features.OrderBy(a => a.FeatureName).ToList();
            model.ListRF = Global.Context.RoleFeatures.Where(a => a.RoleID == model.roleID).Distinct().ToList();
            model.ListCheckBox = new List<SelectListItem>();
            foreach (Feature ft in model.ListFeature)
            {
                SelectListItem slItem = new SelectListItem();
                slItem.Text = ft.FeatureName;
                slItem.Value = ft.ID.ToString();
                var m = model.ListRF.Where(a => a.FeatureID == ft.ID).FirstOrDefault();
                if (m != null && m.FeatureID != 0)
                {
                    slItem.Selected = true;
                }
                model.ListCheckBox.Add(slItem);
            }

            //return model;
        }

        private void SaveRoleFeatureAssignment(int roleID, int[] selectedItems)
        {
            using (var context = new ERouteDataContext())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    #region Delete
                    var listRF = context.RoleFeatures.Where(a => a.RoleID == roleID).ToList();
                    context.RoleFeatures.DeleteAllOnSubmit(listRF);
                    context.SubmitChanges();
                    #endregion

                    #region insert new
                    List<RoleFeature> listInsert = new List<RoleFeature>();
                    foreach (int featureID in selectedItems)
                    {
                        RoleFeature rf = new RoleFeature()
                        {
                            RoleID = roleID,
                            FeatureID = featureID
                        };
                        listInsert.Add(rf);
                    }
                    context.RoleFeatures.InsertAllOnSubmit(listInsert);
                    context.SubmitChanges();
                    #endregion

                    scope.Complete();
                }
            }
        }
        #endregion

        #region SyncUser
        public ActionResult SyncUser()
        {
            DMSERoute.Helpers.SyncUser.SyncUserAcuToERoute();
            return RedirectToAction("Index", "User");
        }
        #endregion

    }
}