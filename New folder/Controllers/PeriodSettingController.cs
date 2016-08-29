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
using DMSERoute.Helpers;

namespace Hammer.Controllers
{
    public class PeriodSettingController : Controller
    {
        //
        // GET: /PeriodSetting/

        private static readonly ILog Log = LogManager.GetLogger(typeof(PeriodSettingController));
        public ActionResult Index()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (User.IsInRole("SysAdmin") == false)
            {
                return RedirectToAction("index", "ErrorPermission");
            }
            Session["PeriodDetailSetting"] = null;
            List<PeriodSetting> list = new List<PeriodSetting>();
            list = HammerDataProvider.GetPeriodSetting();
            Session["PeriodDetailSetting"] = list;
            return View();
        }

        public ActionResult DetailPrepareSchedulePartialView()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            List<PeriodSetting> list = new List<PeriodSetting>();
            list = HammerDataProvider.GetPeriodSetting();
            Session["PeriodDetailSetting"] = list;
            return PartialView(Session["PeriodDetailSetting"]);
        }        
        public ActionResult UpdateDetailSetting(PeriodSetting model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ModelState.IsValid)
            {
                var list = Session["PeriodDetailSetting"] as List<PeriodSetting>;

                (from item in list where item.PeriodID == model.PeriodID select item).
                    ToList().ForEach(item =>
                    {
                        item.PeriodID = model.PeriodID;
                        item.UserCreated = User.Identity.Name;
                        item.CreatedDate = DateTime.Now;
                        item.DesEn = model.DesEn;
                        item.DesVN = model.DesVN;
                        HammerDataProvider.SavePeriod(item);
                    });
                Session["PeriodDetailSetting"] = list;
            }
            return PartialView("DetailPrepareSchedulePartialView", Session["PeriodDetailSetting"]);
        }
        public ActionResult AddDetailPartial(PeriodSetting model)
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            if (ModelState.IsValid)
            {
                if (model.PeriodID != null)
                {
                    var list = Session["PeriodDetailSetting"] as List<PeriodSetting>;
                    var find = list.Find(a => a.PeriodID == model.PeriodID.Trim());
                    if (find != null)
                    {
                        ViewData["PeriodDetailSettingEditError"] = Utility.Phrase("PeriodSetting.SameKey");
                    }
                    else
                    {
                        PeriodSetting item = new PeriodSetting();
                        item.PeriodID = model.PeriodID;
                        item.UserCreated = User.Identity.Name;
                        item.CreatedDate = DateTime.Now;
                        item.DesEn = model.DesEn;
                        item.DesVN = model.DesVN;
                        HammerDataProvider.SavePeriod(item);
                    }
                    Session["PeriodDetailSetting"] = HammerDataProvider.GetPeriodSetting();
                }
                else
                {
                    ViewData["PeriodDetailSettingEditError"] = Utility.Phrase("PeriodSetting.KeyNotNull");
                }
            }
            return PartialView("DetailPrepareSchedulePartialView", Session["PeriodDetailSetting"]);
        }
        public ActionResult DeletePartial(string PeriodID)
        {
            var list = Session["PeriodDetailSetting"] as List<PeriodSetting>;
            if (ModelState.IsValid)
            {
                try
                {
                    if (PeriodID != null)
                    {
                        bool rs = HammerDataProvider.CheckPeriod(PeriodID);
                        if (rs == true)
                        {
                            ViewData["PeriodDetailSettingEditError"] = Utility.Phrase("PeriodSetting.PeriodAssignterrors");
                        }
                        else
                        {
                            var find = list.Find(a => a.PeriodID == PeriodID);
                            if (find != null)
                            {
                                list.RemoveAll(a => a.PeriodID == PeriodID);
                            }
                            HammerDataProvider.DeletePeriod(PeriodID);
                        }
                    }
                    else
                    {
                        ViewData["PeriodDetailSettingEditError"] = Utility.Phrase("AssessmentCapacity.Correctallerrors");
                    }
                }
                catch (Exception e)
                {
                    ViewData["PeriodDetailSettingEditError"] = e.Message;
                }
            }
            else
            {
                ViewData["PeriodDetailSettingEditError"] = Utility.Phrase("AssessmentCapacity.Correctallerrors");

            }
            Session["PeriodDetailSetting"] = HammerDataProvider.GetPeriodSetting();
            return PartialView("DetailPrepareSchedulePartialView", Session["PeriodDetailSetting"]);
        }
    }
}
