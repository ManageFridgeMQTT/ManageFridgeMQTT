using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hammer.Models;
using System.Globalization;
using WebMatrix.WebData;
using eRoute.Models.eCalendar;
using eRoute;
using DMSERoute.Helpers;
using DevExpress.Web.Mvc;

namespace Hammer.Controllers
{    
    [Authorize()]
    public class ViewAssessmentController : Controller
    {
        //
        // GET: /ViewAssessment/
        [Authorize]
        [ActionAuthorize("eCalendar_ViewAssessment", true)]
        public ActionResult Index()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));  
 
            return View(new ViewAssessmentModel()
            {               
                HasTraining = true,
                FromDate = DateTime.Now,
                EmployeeID = "",
                UniqueID = "",
                ListEmployee = HammerDataProvider.ViewAssessmentGetEmployees(User.Identity.Name, DateTime.Now.Date,"true")  
            });           
        }
        [HttpPost]
        public ActionResult CheckboxPartial()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            var check = Request.Params["Check"];
            Session["ViewAssessmentCheck"] = check;
            Session["Employees"] = null;
            //return null;
            return PartialView();
        }      
        [HttpPost]
        public ActionResult ComboBoxPartial()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            var date = Request.Params["FromDate"];          
            DateTimeFormatInfo ukDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            DateTime time = Convert.ToDateTime(date, ukDtfi);
            //if (Session["ViewAssessmentCheck"] == null)
            //    Session["ViewAssessmentCheck"] = true;
            Session["Employees"] = null;
            string check = Request.Params["HasTraining"];               
            List<EmployeeModel> list = HammerDataProvider.ViewAssessmentGetEmployees(User.Identity.Name, time, check);          
            return PartialView(list);

            //string regionID = Request.Params["RegionID"].ToString();
            //List<Area> listItem = HammerDataProvider.GetAreasWithRegion(regionID);
            //return PartialView(listItem);
        }
        [HttpPost]
        public ActionResult ComboBoxUniqueIDPartial()
        {
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));
            string NV = Request.Params["EmployeeID"];
            NV = Utility.StringParse(EditorExtension.GetValue<string>("EmployeeID")); 
            var date = Request.Params["FromDate"];
            DateTimeFormatInfo ukDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            DateTime time = Convert.ToDateTime(date, ukDtfi);
            string check = Request.Params["HasTraining"];   
            bool type = Convert.ToBoolean(check); ;
            //string check = Session["ViewAssessmentCheck"].ToString();
           // ViewData["ListUnique"] = null;
            List<ComboDateAssessmentModel> list = HammerDataProvider.GetAssessmentDateAllTask(NV, time.Date, type);
            return PartialView(list);
        }    
        [HttpPost]
        public ActionResult ProcessSchedule(ViewAssessmentModel model)
        {
            model.EmployeeID = Utility.StringParse(EditorExtension.GetValue<string>("EmployeeID")); 
            HammerDataProvider.ActionSaveLog(WebSecurity.GetUserId(User.Identity.Name));   
            if (model.EmployeeID != null)
            {                
                    SMAssessmentModel view = new SMAssessmentModel();
                    NoAssessmentModel viewno = new NoAssessmentModel();
                    if (model.HasTraining == true)
                    {                        
                        view = HammerDataProvider.ViewAssessmentSM(model.EmployeeID, model.FromDate,Convert.ToInt32(model.UniqueID));
                        // 08-01-2014 them vao de chay nhung thang SS danh gia SM luu vao bang nv
                        if (view.Header.UniqueID == null)
                        {
                            AssessmentModel viewnv = new AssessmentModel();
                            viewnv = HammerDataProvider.ViewAssessment(model.EmployeeID, model.FromDate, Convert.ToInt32(model.UniqueID));
                            return PartialView("DetailEmView", viewnv);
                        }
                        else
                        {
                            return PartialView("DetailView", view);
                        }
                    }
                    else
                    {
                        viewno = HammerDataProvider.ViewNoAssessment(model.EmployeeID, model.FromDate, Convert.ToInt32(model.UniqueID));
                        return PartialView("DetailNoTrainningView", viewno);
                    }                               
            }
            return null;
           
        }       
     }
}
