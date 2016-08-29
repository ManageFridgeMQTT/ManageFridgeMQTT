using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DMSERoute.Helpers;
using eRoute.Models.ViewModel;

namespace eRoute.Models
{
    public class ManageUser
    {
        public static List<UserVM> GetUsers(string type)
        {
            List<UserVM> listUser = new List<UserVM>();
            string userName = SessionHelper.GetSession<string>("UserName");
            //Utility.co

            return listUser;
        }

        public static IEnumerable<Role> GetRoles()
        {
            return from r in Global.Context.Roles select r;
        }
    }
}