using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Security.Cryptography.X509Certificates;
using WebMatrix.WebData;
using eRoute.Filters;
using eRoute.Models;
using DMSERoute.Helpers;
using DevExpress.Web.Mvc;
//using DevExpress.Web.ASPxMenu;
using System.Collections;
using System.Net.Security;
using Microsoft.Web.WebPages.OAuth;
using eRoute.ACModels;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Configuration;
using eRoute.Models.ViewModel;
using System.Net;
using Newtonsoft.Json;

namespace eRoute.Controllers
{
    [InitializeSimpleMembership]
    [LogAndRedirectOnError]
    public class AccountController : Controller
    {
        #region Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.CaptCha = ControllerHelper.valueCustomSetting("reCaptcha");
            ViewBag.SiteKey = ControllerHelper.valueCustomSetting("SiteKeyCaptCha");
            ViewBag.ReturnUrl = returnUrl;
            ShowValue();
            return View();
        }

        //
        // POST: /Account/Login

        [AllowAnonymous]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(LoginModel model, string returnUrl, string reCaptCha, FormCollection col)
        {

            if(reCaptCha != null && Utility.IntParse(reCaptCha) == 1)
            {
                var response = HttpContext.Request.Form["g-recaptcha-response"];
                string secretKey = ControllerHelper.valueCustomSetting("SecretkeyCaptCha");
                var client = new WebClient();
                // get captcha verification result
                var verificationResultJson = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
                // convert json to object
                var verificationResult = JsonConvert.DeserializeObject<ReCaptchaClass>(verificationResultJson);
                // process verification result
                if (verificationResult.Success)
                {
                    if (ModelState.IsValid)
                    {
                        if (WebSecurity.Login(model.UserName.Trim(), model.Password, persistCookie: model.RememberMe))
                        {
                            PermissionHelper.SetSession(model);
                            string chageLang = col["cmbLang"].ToString();
                            HttpCookie ckieLanguage = new HttpCookie(Utils.CurrentLanguageCookieKey);
                            HttpContext.Response.Cookies.Remove(Utils.CurrentLanguageCookieKey);
                            ckieLanguage.Value = chageLang;
                            HttpContext.Response.SetCookie(ckieLanguage);

                            Utility.SessionLanguage = chageLang;

                            string strLang = string.Empty;
                            List<Language> lang = Global.Context.Languages.Where(x => x.Active == true).ToList();
                            foreach (var item in lang)
                            {
                                strLang += "|" + item.Code + "," + item.Image;
                            }
                            Session["lang"] = strLang.TrimStart('|');

                            ChangePasswordModel mode = new ChangePasswordModel();
                            var query = (from x in Global.Context.UserProfiles
                                         join z in Global.Context.UserProfileInfos on x.UserName equals (z.LoginID)
                                         join p in Global.Context.webpages_Memberships on x.UserId equals (p.UserId)
                                         where x.UserName == model.UserName
                                         select new { x.UserName, p.Password, z.FullName, z.Email, z.Phone }).FirstOrDefault();//z.FullName, z.Email, z.Phone, 
                            mode.OldPassword = query.Password;
                            mode.UserName = query.UserName;
                            mode.FullName = query.FullName;
                            mode.Email = query.Email;
                            mode.Phone = query.Phone;
                            SessionHelper.SetSession<ChangePasswordModel>("ChangePasswordModel", mode);
                            System.Web.HttpContext.Current.Application["usr_" + model.UserName] = System.Web.HttpContext.Current.Session.SessionID;
                            return RedirectToLocal(returnUrl);
                        }
                        else
                        {
                            if (!WebSecurity.IsConfirmed(model.UserName) && WebSecurity.UserExists(model.UserName))
                            {
                                int UserID = Global.Context.UserProfiles.Where(x => x.UserName == model.UserName).FirstOrDefault().UserId;
                                string confirmToken = Global.Context.webpages_Memberships.Where(x => x.UserId == UserID).FirstOrDefault().ConfirmationToken;
                                return RedirectToAction("ChangePass", "Account", new { userName = model.UserName, token = confirmToken });
                            }
                            else
                            {
                                ModelState.AddModelError("", Utility.Phrase("InputUserOrPassNotAvailble"));
                            }
                        }
                    }
                }
                else
                {
                    var error = verificationResult.ErrorCodes[0].ToLower();
                    switch (error)
                    {
                        case ("missing-input-secret"):
                            ModelState.AddModelError("", Utility.Phrase("TheSecretParameterIsMissing"));
                            break;

                        case ("invalid-input-secret"):
                            ModelState.AddModelError("", Utility.Phrase("TheSecretParameterIsInvalidOrMalformed"));
                            break;

                        case ("missing-input-response"):
                            ModelState.AddModelError("", Utility.Phrase("TheCaptchaInputIsMissing"));
                            break;

                        case ("invalid-input-response"):
                            ModelState.AddModelError("", Utility.Phrase("TheCaptchaInputIsInvalidOrMalformed"));
                            break;

                        default:
                            ModelState.AddModelError("", Utility.Phrase("ErrorOccuredPleaseTryAgain"));
                            break;
                    }
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (WebSecurity.Login(model.UserName.Trim(), model.Password, persistCookie: model.RememberMe))
                    {
                        PermissionHelper.SetSession(model);
                        string chageLang = col["cmbLang"].ToString();
                        HttpCookie ckieLanguage = new HttpCookie(Utils.CurrentLanguageCookieKey);
                        HttpContext.Response.Cookies.Remove(Utils.CurrentLanguageCookieKey);
                        ckieLanguage.Value = chageLang;
                        HttpContext.Response.SetCookie(ckieLanguage);

                        Utility.SessionLanguage = chageLang;

                        string strLang = string.Empty;
                        List<Language> lang = Global.Context.Languages.Where(x => x.Active == true).ToList();
                        foreach (var item in lang)
                        {
                            strLang += "|" + item.Code + "," + item.Image;
                        }
                        Session["lang"] = strLang.TrimStart('|');

                        ChangePasswordModel mode = new ChangePasswordModel();
                        var query = (from x in Global.Context.UserProfiles
                                     join z in Global.Context.UserProfileInfos on x.UserName equals (z.LoginID)
                                     join p in Global.Context.webpages_Memberships on x.UserId equals (p.UserId)
                                     where x.UserName == model.UserName
                                     select new { x.UserName, p.Password, z.FullName, z.Email, z.Phone }).FirstOrDefault();//z.FullName, z.Email, z.Phone, 
                        mode.OldPassword = query.Password;
                        mode.UserName = query.UserName;
                        mode.FullName = query.FullName;
                        mode.Email = query.Email;
                        mode.Phone = query.Phone;
                        SessionHelper.SetSession<ChangePasswordModel>("ChangePasswordModel", mode);
                        System.Web.HttpContext.Current.Application["usr_" + model.UserName] = System.Web.HttpContext.Current.Session.SessionID;
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        if (!WebSecurity.IsConfirmed(model.UserName) && WebSecurity.UserExists(model.UserName))
                        {
                            int UserID = Global.Context.UserProfiles.Where(x => x.UserName == model.UserName).FirstOrDefault().UserId;
                            string confirmToken = Global.Context.webpages_Memberships.Where(x => x.UserId == UserID).FirstOrDefault().ConfirmationToken;
                            return RedirectToAction("ChangePass", "Account", new { userName = model.UserName, token = confirmToken });
                        }
                        else
                        {
                            ModelState.AddModelError("", Utility.Phrase("InputUserOrPassNotAvailble"));
                        }
                    }
                }
            }
            ViewBag.CaptCha = reCaptCha;
            ViewBag.SiteKey = ControllerHelper.valueCustomSetting("SiteKeyCaptCha");
            // If we got this far, something failed, redisplay form
            ShowValue();
            return View(model);
        } 
        #endregion

        #region LogOff
        //
        // POST: /Account/LogOff

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult LogOff()
        {
            //ControllerHelpers.LogUserAction("Account", "LogOff", "");

            //Session.Clear();
            Session.Abandon();
            ShowValue();
            WebSecurity.Logout();
            HttpRuntime.Cache.Remove("CacheListMenu");
            return RedirectToAction("Login", "Account");
        } 
        #endregion

        #region Manage
        [Authorize]
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Mật khẩu của bạn đã được thay đổi thành công."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                        //ControllerHelpers.LogUserAction("Account", "Manage", "");
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Mật khẩu cũ của bạn không hợp lệ.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        } 
        #endregion

        #region ChangePass
        public ActionResult ChangePass(string userName,string token, ManageMessageId? message)
        {
            SessionHelper.SetSession<string>("UserName", userName);
            ChangePasswordModel mode = new ChangePasswordModel();
            var query = (from x in Global.Context.UserProfiles
                         //join z in Global.Context.UserProfileInfos on x.UserName equals (z.LoginID)
                         join p in Global.Context.webpages_Memberships on x.UserId equals (p.UserId)
                         where p.ConfirmationToken == token
                         select new { x.UserName, p.Password }).FirstOrDefault();//z.FullName, z.Email, z.Phone, 
            mode.UserName = query.UserName;
            //mode.FullName = query.FullName;
            //mode.Email = query.Email;
            //mode.Phone = query.Phone;
            SessionHelper.SetSession<ChangePasswordModel>("ChangePasswordModel", mode);

            ChangePasswordInFirstLoginModel model = new ChangePasswordInFirstLoginModel();
            model.UserName = query.UserName;
            model.ConfirmationToken = token;
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Mật khẩu của bạn đã được thay đổi thành công."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";

            return View(model);
        }

        //
        // POST: /Account/ChangePass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePass(ChangePasswordInFirstLoginModel model)
        {
            if (ModelState.IsValid)
            {
                bool changePasswordSucceeded = false;
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                try
                {
                    //WebSecurity.ChangePassword(
                    changePasswordSucceeded = WebSecurity.ChangePassword(model.UserName, model.OldPassword, model.NewPassword);

                    if (changePasswordSucceeded)
                    {
                        if (WebSecurity.ConfirmAccount(model.UserName, model.ConfirmationToken))
                        {
                            WebSecurity.Login(model.UserName, model.NewPassword, persistCookie: model.RememberMe);

                            LoginModel modelLG = new LoginModel();
                            modelLG.UserName = model.UserName;
                            modelLG.Password = model.NewPassword;
                            PermissionHelper.SetSession(modelLG);
                            return RedirectToAction("ChangePass", new { userName = model.UserName, token = model.ConfirmationToken, Message = ManageMessageId.ChangePasswordSuccess });
                        }
                        else
                        {
                            WebSecurity.ChangePassword(model.UserName, model.NewPassword, model.OldPassword);
                            ModelState.AddModelError("", "Mã xác thực của bạn không hợp lệ.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Mật khẩu cũ của bạn không hợp lệ.");
                    }

                }
                catch (Exception e)
                {
                    if (changePasswordSucceeded)
                    {
                        WebSecurity.ChangePassword(model.UserName, model.NewPassword, model.OldPassword);
                    }
                    ModelState.AddModelError("", e);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        } 
        #endregion

        #region UpdateUser
        public ActionResult UpdateUser(string UserName, string FullName, string Email, string Phone, string PasswordOld, string NewPassword)
        {
            string strError = "No";
            if (UserName != string.Empty && PasswordOld != string.Empty && NewPassword != string.Empty)
            {
                bool update = WebSecurity.ChangePassword(UserName, PasswordOld, NewPassword);
                if (update)
                {
                    //UserProfileInfo rf = Global.Context.UserProfileInfos.Where(x => x.LoginID == UserName).FirstOrDefault();
                    //if (rf != null)
                    //{
                    //    rf.FullName = FullName;
                    //    rf.Phone = Phone;
                    //    rf.Email = Email;
                    //    Global.Context.SubmitChanges();
                    //}
                    strError = "Yes";
                }
            }
            var jsonResult = Json(new
            {
                Val = strError
            }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        #endregion

        #region ResetPass
        [AllowAnonymous]
        public ActionResult ResetPass(string token)
        {
            ResetPasswordVM model = new ResetPasswordVM();
            model.ConfirmationToken = token;
            ViewBag.StatusMessage =  Utility.Phrase("PleaseInputPassNew");
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPass(ResetPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                ValidateExtension.ValidateStringLengthRange(model.NewPassword, "Password", 3, 100);
                ValidateExtension.ValidateStringLengthRange(model.ConfirmPassword, "ConfirmPassword", 3, 100);

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", Utility.Phrase("InputPassNotAvailble"));
                }

                if (WebSecurity.ResetPassword(model.ConfirmationToken, model.NewPassword))
                {
                    //ControllerHelper.LogUserAction("Account", "ResetPassword", "");
                    ViewBag.StatusMessage = Utility.Phrase("SetPasswordSessussfull.PleaseExit");
                }
                else
                {
                    ModelState.AddModelError("", Utility.Phrase("SetPasswordFaild.PleaseContactAdmin"));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        } 
        #endregion

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Home", "DashBoard");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Tên đăng nhập đã tồn tại trong hệ thống";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "Mật khẩu không hợp lệ";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "Tên đăng nhập không hợp lệ";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        private void ShowValue()
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                //ArrayList caminohormiga = new ArrayList();
                ////caminohormiga.Add("Cloud-Priority.jpg");
                ////caminohormiga.Add("Cloud.jpg");
                //caminohormiga.Add("DMSpro screen 1.jpg");
                //caminohormiga.Add("DMSpro screen 2.jpg");
                //caminohormiga.Add("DMSpro screen 3.jpg");
                //caminohormiga.Add("DMSpro screen 4.jpg");
                //caminohormiga.Add("DMSpro screen 5.jpg");
                //Random ran = new Random();
                //int start = 0;
                //for (int i = 0; i < caminohormiga.Count; i++)
                //{
                //    start = ran.Next(0, caminohormiga.Count);
                //}
                //ViewBag.image = caminohormiga[start].ToString();
                ViewBag.image = getrandomfile();
                List<Language> lag = db.Languages.Where(x => x.Active == true).ToList();
                ViewData["lag"] = lag;
            }
        }
        private Random generator;
        private Random Generator
        {
            get
            {
                if (this.generator == null)
                {
                    this.generator = new Random();
                }
                return this.generator;
            }
        }
        private string getrandomfile()
        {
            string file = null;
            var extensions = new string[] { ".png", ".jpg", ".gif" };
            try
            {
                var di = new DirectoryInfo(Server.MapPath("~") + "/Content/Logo/");
                var rgFiles = di.GetFiles("*.*").Where(f => extensions.Contains(f.Extension.ToLower()));
                int fileCount = rgFiles.Count();
                if (fileCount > 0)
                {
                    int x = this.Generator.Next(0, fileCount);
                    file = rgFiles.ElementAt(x).Name;
                }
            }
            catch { }
            return file;
        }
        public static void CreateMenu(MVCxMenuItemCollection itemColl, int parentID)
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                List<pp_GetListMenuResult> list_sp_Menu_For_Lang = db.pp_GetListMenu(SessionHelper.GetSession<int>("RoleUser"), Utils.CurrentLanguage).ToList();
                foreach (var row in list_sp_Menu_For_Lang.Where(x => x.ParentID == parentID).OrderBy(x => x.Order))
                {
                    if (row.Path != null)
                    {
                        if (row.FeatureID == 1)
                        {
                            if (SessionHelper.GetSession<string>("UserName") == "admin")
                            {
                                itemColl.Add(item =>
                                {
                                    item.Text = row.Name;
                                    if (!string.IsNullOrEmpty(row.Path))
                                    {
                                        item.NavigateUrl = FooMethod(row.Path);
                                        //CustomLog.LogError(FooMethod(row.Path));
                                    }
                                    SubMenu(list_sp_Menu_For_Lang, item.Items, row.FeatureID.Value);
                                });
                            }
                        }
                        else
                        {
                            itemColl.Add(item =>
                            {
                                item.Text = row.Name;
                                if (!string.IsNullOrEmpty(row.Path))
                                {
                                    item.NavigateUrl = FooMethod(row.Path);
                                    //CustomLog.LogError(FooMethod(row.Path));
                                }
                                SubMenu(list_sp_Menu_For_Lang, item.Items, row.FeatureID.Value);
                            });
                        }
                    }
                }
            }
        }
        private static void SubMenu(List<pp_GetListMenuResult> list_sp_Menu_For_Lang, MVCxMenuItemCollection menu, int menuID)
        {
            foreach (var sub in list_sp_Menu_For_Lang.Where(x => x.ParentID == menuID).ToList())
            {
                if (sub.Path != null)
                {
                    menu.Add(item =>
                    {
                        item.Text = sub.Name;
                        item.NavigateUrl = FooMethod(sub.Path);
                        //CustomLog.LogError(FooMethod(sub.Path));
                        SubMenu(list_sp_Menu_For_Lang, item.Items, sub.FeatureID.Value);
                    });
                }
            }
        }

        public static string FooMethod(string path)
        {
            var mappedPath = Constant.appFolder + path;
            return mappedPath;
        }
        #endregion

        #region RoleFeatureAssignment
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Account_RoleFeatureAssignment", true)]
        public ActionResult RoleFeatureAssignment()
        {
            ViewData["Groups"] = Global.Context.Roles.ToList();
            ViewData["Features"] = Global.Context.Features.ToList();
            ViewData["FeaturesSelected"] = new List<Feature>();
            return View();
        }

        public ActionResult TreeViewGroup(string Group)
        {
            List<RoleFeature> query = new List<RoleFeature>();
            if (Group != null)
            {
                query = Global.Context.RoleFeatures.Where(x => x.RoleID == int.Parse(Group)).ToList();
            }
            ViewData["GroupFeature"] = query;
            ViewData["Features"] = Global.Context.Features.ToList();
            ViewData["FeaturesSelected"] = Global.Context.Features.Where(x => query.Select(s => s.FeatureID).Contains(x.ID)).ToList();
            return PartialView("TreeViewGroup");
        }
        [LogAndRedirectOnError]
        public JsonResult InsertGroupFeature(string ID, string List)
        {
            string strErro = string.Empty;
            if (!string.IsNullOrEmpty(ID) && ID != "null")
            {
                DataTable tbCus = new DataTable("RoleFeature");
                tbCus.Columns.Add("RoleID", typeof(int));
                tbCus.Columns.Add("FeatureID", typeof(int));
                List<string> lstFeature = List.Trim().Split(',').ToList();
                ERouteDataContext dataContext = new ERouteDataContext();
                var rolDeleted = (from prod in Global.Context.RoleFeatures where prod.RoleID == int.Parse(ID) select prod.RoleID).FirstOrDefault();
                if (rolDeleted > 0)
                {
                    Global.Context.sp_Delete_RoleFeature(rolDeleted);
                }
                foreach (var item in lstFeature)
                {
                    if (!string.IsNullOrEmpty(item.ToString()))
                    {
                        DataRow dr = tbCus.NewRow();

                        dr["RoleID"] = int.Parse(ID);
                        dr["FeatureID"] = int.Parse(item.ToString());
                        tbCus.Rows.Add(dr);
                    }
                }
                using (SqlBulkCopy sb = new SqlBulkCopy(Utility.Connect, SqlBulkCopyOptions.FireTriggers))
                {

                    sb.DestinationTableName = "RoleFeature";
                    sb.WriteToServer(tbCus);
                }
            }
            else
            {
                strErro = DMSERoute.Helpers.Utility.Phrase("InputControl");
            }
            if (string.IsNullOrEmpty(strErro))
            {
                int userID = SessionHelper.GetSession<int>("UserID");
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
            else
            {
                return Json(strErro);
            }
            return Json(DMSERoute.Helpers.Utility.Phrase("SaveGoupSucceed"));

        }

        public ActionResult ReloadFeatureMenu(string Group, string Check)
        {
            List<pp_GetListMenuResult> newlistMenu = new List<pp_GetListMenuResult>();
            List<pp_GetListMenuResult> listMenu = Global.Context.pp_GetListMenu(int.Parse(Group), Utils.CurrentLanguage).ToList();
            if (bool.Parse(Check))
            {
                List<Feature> listRoleFeature = new List<Feature>();
                if (Group != null)
                {
                    string[] listGroupToMenu = ConfigurationSettings.AppSettings["NameListGroupToMenu"].Trim().Split(',').ToArray();
                    using (ERouteDataContext db = new ERouteDataContext())
                    {
                        listRoleFeature = (from RF in db.RoleFeatures
                                           join Fea in db.Features.Where(x => x.Group.Equals(listGroupToMenu[0]) || x.Group.Equals(listGroupToMenu[1])) on RF.FeatureID equals Fea.ID
                                           where RF.RoleID.Equals(int.Parse(Group)) && listGroupToMenu.Contains(Fea.Group)
                                           select Fea).ToList();
                    }
                    //listRoleFeature = Global.Context.RoleFeatures.Where(x => x.RoleID == int.Parse(Group)).Select(s => s.FeatureID).ToList();
                }
                if (listRoleFeature != null)
                {
                    foreach (Feature elm in listRoleFeature)
                    {
                        if (!listMenu.Any(x => x.FeatureID.Equals(elm.ID)))
                        {
                            string nameMenu = "";
                            if (Global.Context.Phrases.Where(x => x.PhraseCode.Equals(elm.PhraseCode)).ToList() != null)
                            {
                                nameMenu = Utility.Phrase(elm.PhraseCode);
                            }
                            else
                            {
                                Utility.Phrase(elm.PhraseCode);
                                nameMenu = Utility.ConvertCodeToText(elm.PhraseCode);
                            }
                            pp_GetListMenuResult newMenu = new pp_GetListMenuResult()
                            {
                                Name = nameMenu,
                                FeatureID = elm.ID,
                                Order = null,
                                ParentID = 0
                            };
                            newlistMenu.Add(newMenu);
                        }
                        else
                        {
                            newlistMenu.Add(listMenu.SingleOrDefault(x => x.FeatureID.Equals(elm.ID)));
                        }
                    }

                    foreach (pp_GetListMenuResult elm in newlistMenu)
                    {
                        if (elm.ParentID != 0)
                        {
                            if (!newlistMenu.Any(x => x.FeatureID.Equals(elm.ParentID)))
                            {
                                elm.ParentID = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                newlistMenu = listMenu;
            }
            ViewData["FeaturesSelected"] = newlistMenu;
            return PartialView("TreeListPartial");
        }
        public JsonResult UpdateMenu(string roleID, string valMenu)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<JsonMenu> listMenu = ser.Deserialize<List<JsonMenu>>(valMenu);
            List<MenuFeature> list = Global.Context.MenuFeatures.Where(x => x.RoleID.Equals(int.Parse(roleID))).ToList();
            if (list != null)
            {
                foreach (MenuFeature item in list)
                {
                    Global.Context.MenuFeatures.DeleteOnSubmit(item);
                }
            }
            UpdateItemMenu(listMenu, 0, int.Parse(roleID));
            HttpRuntime.Cache.Remove("CacheListMenu");
            return Json(DMSERoute.Helpers.Utility.Phrase("Update Successfully"));
        }

        public void UpdateItemMenu(List<JsonMenu> listMenu, int parentID, int roleID)
        {
            int order = 1;
            foreach (JsonMenu elm in listMenu)
            {
                Feature itemFeature = Global.Context.Features.SingleOrDefault(x => x.ID.Equals(elm.id));
                if (itemFeature != null)
                {
                    string path = "";
                    if (!string.IsNullOrEmpty(itemFeature.Page) && !string.IsNullOrEmpty(itemFeature.Action))
                    {
                        path = "/" + itemFeature.Page + "/" + itemFeature.Action;
                    }
                    MenuFeature menu = new MenuFeature()
                    {
                        RoleID = roleID,
                        FeatureID = itemFeature.ID,
                        PhraseCode = itemFeature.PhraseCode,
                        Path = path,
                        Order = order,
                        ParentID = parentID,
                        IconClass = itemFeature.IconClass
                    };
                    Global.Context.MenuFeatures.InsertOnSubmit(menu);
                    Global.Context.SubmitChanges();
                    if (elm.children != null)
                    {
                        this.UpdateItemMenu(elm.children, elm.id, roleID);
                    }
                    order++;
                }

            }
        }
        #endregion

        #region Role management
        //
        // GET: /Role/

        [Authorize]
        [ActionAuthorize("RoleManager", true)]
        public ActionResult RoleManager()
        {
            List<Role> roles = Global.Context.Roles.ToList();
            List<RoleVM> model = Utility.CopyList<RoleVM>(roles);
            SessionHelper.SetSession<List<RoleVM>>("Roles", model);
            return View(model);
        }
        public ActionResult RolePartial()
        {
            List<RoleVM> model = SessionHelper.GetSession<List<RoleVM>>("Roles");
            return PartialView("RolePartial", model);
        }
        [HttpPost]
        [ActionAuthorize("Role_AddNewRole")]
        public ActionResult AddNewRole(RoleVM role)
        {
            var model = SessionHelper.GetSession<List<RoleVM>>("Roles");
            try
            {
                if (ModelState.IsValid)
                {
                    Role r = new Role()
                    {
                        RoleName = role.RoleName,
                        Description = role.Description,
                        ParentID = role.ParentID,
                        ApplicationCD = role.ApplicationCD
                    };
                    Global.Context.Roles.InsertOnSubmit(r);
                    Global.Context.SubmitChanges();
                    model.Add(role);
                    SessionHelper.SetSession<List<RoleVM>>("Roles", model);
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            return PartialView("RolePartial", model);
        }
        [HttpPost]
        [ActionAuthorize("Role_UpdateRole")]
        public ActionResult UpdateRole(RoleVM role)
        {
            List<RoleVM> model = SessionHelper.GetSession<List<RoleVM>>("Roles");
            try
            {
                if (ModelState.IsValid)
                {
                    Role r = Global.Context.Roles.SingleOrDefault(x => x.ID == role.ID);
                    r.RoleName = role.RoleName;
                    r.Description = role.Description;
                    r.ParentID = role.ParentID;
                    r.ApplicationCD = role.ApplicationCD;
                    Global.Context.SubmitChanges();
                    RoleVM roleVM = Utility.Copy<RoleVM>(r);
                    model = UpdateRoleForDataSession(model, roleVM);
                    SessionHelper.SetSession<List<RoleVM>>("Roles", model);
                }
            }
            catch (Exception e)
            {
                ViewData["EditError"] = e.Message;
            }
            return PartialView("RolePartial", model);
        }
        [NonAction]
        public static List<RoleVM> UpdateRoleForDataSession(List<RoleVM> list, RoleVM roleReplace)
        {
            int index = list.FindIndex(x => x.ID.Equals(roleReplace.ID));
            if (index > -1)
            {
                list[index] = roleReplace;
            }
            return list;
        }
        #endregion
    }
}
