using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammer.Models;
//using Utility.Phrase("PrepareSchedule");
using Hammer.Helpers;
using log4net;
using eRoute.Filters;
using WebMatrix.WebData;
using System.Globalization;
using eRoute.Models.eCalendar;
using eRoute;
namespace Hammer.Controllers
{
    [InitializeSimpleMembership]
    [Authorize()]
    public class SystemSettingController : Controller
    {
        //
        // GET: /SystemSetting/

        private static readonly ILog Log = LogManager.GetLogger(typeof(SystemSettingController));
        [Authorize]
        [ActionAuthorize("eCalendar_SystemSetting", true)]
        public ActionResult Index()
        {
            //HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            //if (User.IsInRole("SysAdmin") == false)
            //{
            //    return RedirectToAction("index", "ErrorPermission");
            //}
            List<SystemSetting> list = new List<SystemSetting>();
            list = HammerDataProvider.GetListSystem();            
            Session["SystemSetting"] = list;
            return View();
        }

        public ActionResult DetailPrepareSchedulePartialView()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            List<SystemSetting> list = new List<SystemSetting>();
            list = HammerDataProvider.GetListSystem();     
            Session["SystemSetting"] = list;
            //return PartialView(Session["SystemSetting"]);
            return PartialView("DetailPrepareSchedulePartialView", Session["SystemSetting"]);
        }
        public ActionResult UpdateDetailSchedule(SystemSetting model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ModelState.IsValid)
            {
                var list = Session["SystemSetting"] as List<SystemSetting>;

                (from item in list where item.ID == model.ID select item).
                    ToList().ForEach(item =>
                    {    
                        item.UserLogin = User.Identity.Name;
                        item.CreatedDate = DateTime.Now;
                        item.Number = model.Number;
                        item.Desr = model.Desr;                        
                        HammerDataProvider.SaveSystem(item);
                    });
                Session["SystemSetting"] = list;
            }
            return PartialView("DetailPrepareSchedulePartialView", Session["SystemSetting"]);
        }       

    }
}
