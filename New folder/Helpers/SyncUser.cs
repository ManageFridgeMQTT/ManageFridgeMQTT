using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Globalization;
using WebMatrix.WebData;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Data.Entity;
using System.Net;
using eRoute.Models;
using eRoute.ACModels;
using System.Web.Security;
using System.IO.Compression;
using System.Text;
using System.IO;
using eRoute.Models.ViewModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Caching;
using System.Configuration;
using eRoute;

namespace DMSERoute.Helpers
{
    [LogAndRedirectOnError]
    public class SyncUser
    {   
        public static void SyncUserAcuToERoute()
        {
            //SyncRoleACU
            //Param Last process 
            //If datetime.now - last process > sleeptime
            //Select data has change
            //insert if not exist
            //insert user, user_in_role
            //else
            //update approve
            //update not approve
            //update password
            //update user_in_role
            //            
            var model = Global.Context.DMSUsers.Where(a => a.DataChanged == true).ToList();
            if (model.Count > 0)
            {
                var listRole = Global.Context.Roles.ToList();
                var listRoleUser = Global.Context.RoleUsers.ToList();

                foreach (DMSUser item in model)
                {
                    //insert if not exist
                    if (!WebSecurity.UserExists(item.Username))
                    {
                        //insert user, user_in_role
                        WebSecurity.CreateUserAndAccount(item.Username, item.Password, false);
                        var role = listRole.Where(a => a.RoleName == item.Rolename).FirstOrDefault();
                        int userID = WebSecurity.GetUserId(item.Username);
                        if (role != null && userID != 0)
                        {
                            RoleUser ru = new RoleUser()
                            {
                                RoleID = role.ID,
                                UserID = userID
                            };
                            Global.Context.RoleUsers.InsertOnSubmit(ru);
                            Global.Context.SubmitChanges();
                        }
                    }
                    else
                    {
                        int userID = WebSecurity.GetUserId(item.Username);
                        //update approve
                        //update not approve
                        if (WebSecurity.IsConfirmed(item.Username) != item.IsApproved)
                        {
                            var userPass = Global.Context.webpages_Memberships.Where(a => a.UserId == userID).FirstOrDefault();
                            if (userPass != null)
                            {
                                userPass.IsConfirmed = item.IsApproved;
                                Global.Context.SubmitChanges();
                            }
                        }

                        //update password
                        string token = WebSecurity.GeneratePasswordResetToken(item.Username, 10080);
                        WebSecurity.ResetPassword(token, item.Password);

                        //update user_in_role
                        var listRU = listRoleUser.Where(a => a.UserID == userID).ToList();
                        Global.Context.RoleUsers.DeleteAllOnSubmit(listRU);
                        Global.Context.SubmitChanges();

                        var role = listRole.Where(a => a.RoleName == item.Rolename).FirstOrDefault();
                        if (role != null && userID != 0)
                        {
                            RoleUser ru = new RoleUser()
                            {
                                RoleID = role.ID,
                                UserID = userID
                            };
                            Global.Context.RoleUsers.InsertOnSubmit(ru);
                            Global.Context.SubmitChanges();
                        }
                    }

                    //update changed done
                    item.DataChanged = false;
                    Global.Context.SubmitChanges();
                }
            }
        }
    }
}