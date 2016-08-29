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
    public class ShiftSettingController : Controller
    {
        //
        // GET: /ShiftSetting/  

        private static readonly ILog Log = LogManager.GetLogger(typeof(ShiftSettingController));
        [Authorize]
        [ActionAuthorize("eCalendar_ShiftSetting", true)]
        public ActionResult Index()
        {           
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            //if (User.IsInRole("SysAdmin") == false)
            //{
            //    return RedirectToAction("index", "ErrorPermission");
            //}            
            List<ShiftSetting> list = new List<ShiftSetting>();
            list = HammerDataProvider.GetListShift();
            (from item in list select item).
                    ToList().ForEach(item =>
                    {
                        TimeSpan ts = TimeSpan.Parse(item.StartTime);
                        double totalSeconds = ts.TotalSeconds;
                        DateTime tem = new DateTime();
                        tem = tem.AddSeconds(totalSeconds);
                        item.StartTime = tem.TimeOfDay.ToString();                       
                        TimeSpan end = TimeSpan.Parse(item.EndTime);
                        totalSeconds = end.TotalSeconds;
                        DateTime tem2 = new DateTime();
                        tem2 = tem2.AddSeconds(totalSeconds);
                        item.EndTime = tem2.TimeOfDay.ToString();
                        item.UserLogin = User.Identity.Name;                        
                    });
            Session["DetailSetting"] = list;
            return View();           
        }

        public ActionResult DetailPrepareSchedulePartialView()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            List<ShiftSetting> list = new List<ShiftSetting>();
            list = HammerDataProvider.GetListShift();
            Session["DetailSetting"] = list;
            return PartialView(Session["DetailSetting"]);
        }
        public ActionResult UpdateDetailSchedule(ShiftSetting model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ModelState.IsValid)
            {
                var list = Session["DetailSetting"] as List<ShiftSetting>;              
                
                (from item in list where item.ShiftID == model.ShiftID select item).
                    ToList().ForEach(item =>
                    {                        
                        item.ShiftID = model.ShiftID;                      
                        TimeSpan ts = TimeSpan.Parse(model.StartTime.Split(' ')[1]);
                        double totalSeconds = ts.TotalSeconds;
                        DateTime tem = new DateTime();
                        tem = tem.AddSeconds(totalSeconds);
                        item.StartTime = tem.TimeOfDay.ToString();

                        TimeSpan end = TimeSpan.Parse(model.EndTime.Split(' ')[1]);
                        totalSeconds = end.TotalSeconds;
                        DateTime tem2 = new DateTime();
                        tem2 = tem2.AddSeconds(totalSeconds);                      
                        item.EndTime = tem2.TimeOfDay.ToString();  
                    
                        item.UserLogin = User.Identity.Name;
                        item.CreatedDate = DateTime.Now;
                        item.DesEn = model.DesEn;
                        item.DesVN = model.DesVN;
                        item.Active = model.Active;
                        HammerDataProvider.SaveShift(item);
                    });
                Session["DetailSetting"] = list;
            }
            return PartialView("DetailPrepareSchedulePartialView", Session["DetailSetting"]);
        }  
    }
}
