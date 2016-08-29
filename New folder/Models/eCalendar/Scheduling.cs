using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using DevExpress.Web.Mvc;
using System.ComponentModel.DataAnnotations;
//using Utility.Phrase("SendCalendar");
using eRoute.Models.eCalendar;

namespace Hammer.Models
{
    public class SchedulerDataObject
    {       
//        [Display(Name = "EmployeeID", ResourceType = typeof(Messages))]
        public string EmployeeID { get; set; }
        [Required]
//        [Display(Name = "ScheduleType", ResourceType = typeof(Messages))]
        public string ScheduleType { get; set; }       
//        [Display(Name = "FileName", ResourceType = typeof(Messages))]
        public string FileName { get; set; }
        public IEnumerable Appointments { get; set; }
        public IEnumerable Resources { get; set; }
       
    }
    public class SchedulerDataHelper
    {
       
        public static IEnumerable GetResources()
        {
            HammerDataContext db = new HammerDataContext();
            return from res in db.Resources select res;
        }
        public static IEnumerable GetAppointments(string userLogin)
        {
            HammerDataContext db = new HammerDataContext();
            DateTime sytemdate = DateTime.Now.Date;

            return from schedule in db.Appointments where schedule.UserLogin == userLogin && schedule.ScheduleType == "M"   select schedule; 
           
        }
        // get data de xu ly nut send cho Month
        public static List<Appointment> GetEditAppointments(string userLogin)        {
             
              HammerDataContext db = new HammerDataContext();
              return (from schedule in db.Appointments where schedule.UserLogin == userLogin && schedule.ScheduleType == "M" && schedule.Label == 0 select schedule).ToList(); 
        
        }
        // get data de xu ly nut send cho Detail
        public static List<Appointment> GetEditDetailAppointments(string userLogin)
        {

            HammerDataContext db = new HammerDataContext();
            return (from schedule in db.Appointments where schedule.UserLogin == userLogin && schedule.ScheduleType == "M" && schedule.Label == 0 select schedule).ToList();

        }
        public static IEnumerable GetDetailAppointments(string userLogin)
        {
            HammerDataContext db = new HammerDataContext();         
            SystemSetting st = (from stt in db.SystemSettings
                                where stt.ID == "AP"
                                select stt).FirstOrDefault();
            DateTime DateReview = DateTime.Now.AddMonths(st.Number * -1);
            return from schedule in db.Appointments where schedule.UserLogin == userLogin 
                       && schedule.ScheduleType == "D"  
                       && schedule.StartDate.Value.Date >= DateReview.Date
                   select schedule;

        }
        // lay data cho month
        public static string UserLogin { get; set; }       
        public static SchedulerDataObject DataObject
        {
            get
            {
                return new SchedulerDataObject()
                {
                   
                    Appointments = GetAppointments(UserLogin),                  
                    Resources = GetResources()
                };
            }
            
        }
        //
        // lay data cho load lich chi tiet       
        public static SchedulerDataObject DataObjectDetail
        {
            get
            {
                return new SchedulerDataObject()
                {

                    Appointments = GetDetailAppointments(UserLogin),
                    Resources = GetResources()
                };
            }

        }
       
        
        static MVCxAppointmentStorage defaultAppointmentStorage;
        public static MVCxAppointmentStorage DefaultAppointmentStorage
        {
            get
            {
                if (defaultAppointmentStorage == null)
                    defaultAppointmentStorage = CreateDefaultAppointmentStorage();
                return defaultAppointmentStorage;
            }
        }
        
        static MVCxAppointmentStorage CreateDefaultAppointmentStorage()
        {
            MVCxAppointmentStorage appointmentStorage = new MVCxAppointmentStorage();
            appointmentStorage.Mappings.AppointmentId = "UniqueID";
            appointmentStorage.Mappings.Start = "StartDate";
            appointmentStorage.Mappings.End = "EndDate";
            appointmentStorage.Mappings.Subject = "Subject";
            appointmentStorage.Mappings.Description = "Description";
            appointmentStorage.Mappings.Location = "Location";
            appointmentStorage.Mappings.AllDay = "AllDay";
            appointmentStorage.Mappings.Type = "Type";
            appointmentStorage.Mappings.RecurrenceInfo = "RecurrenceInfo";
            appointmentStorage.Mappings.ReminderInfo = "ReminderInfo";
            appointmentStorage.Mappings.Label = "Label";
            appointmentStorage.Mappings.Status = "Status";
            appointmentStorage.Mappings.ResourceId = "ResourceID";
           

            return appointmentStorage;
        }
        static MVCxAppointmentStorage customAppointmentStorage;
        public static MVCxAppointmentStorage CustomAppointmentStorage
        {
            get
            {
                if (customAppointmentStorage == null)
                    customAppointmentStorage = CreateCustomAppointmentStorage();
                return customAppointmentStorage;
            }
        }
        static MVCxAppointmentStorage CreateCustomAppointmentStorage()
        {
            MVCxAppointmentStorage appointmentStorage = CreateDefaultAppointmentStorage();
            appointmentStorage.CustomFieldMappings.Add("Phone", "Phone");
            appointmentStorage.CustomFieldMappings.Add("Employees", "Employees");
            appointmentStorage.CustomFieldMappings.Add("Route", "Route");
            return appointmentStorage;
        }
        static MVCxResourceStorage defaultResourceStorage;
        public static MVCxResourceStorage DefaultResourceStorage
        {
            get
            {
                if (defaultResourceStorage == null)
                    defaultResourceStorage = CreateDefaultResourceStorage();
                return defaultResourceStorage;
            }
        }
        static MVCxResourceStorage CreateDefaultResourceStorage()
        {
            MVCxResourceStorage resourceStorage = new MVCxResourceStorage();
            resourceStorage.Mappings.ResourceId = "ResourceID";
            resourceStorage.Mappings.Caption = "ResourceName";

            return resourceStorage;
        }
        public static void InsertAppointment(Appointment appt)
        {
            if (appt == null)
                return;
            HammerDataContext db = new HammerDataContext();
            appt.UniqueID = appt.GetHashCode();            
            db.Appointments.InsertOnSubmit(appt);
            db.SubmitChanges();
        }
        public static void UpdateAppointment(Appointment appt)
        {
            if (appt == null)
                return;
            HammerDataContext db = new HammerDataContext();
            Appointment query = (Appointment)(from carSchedule in db.Appointments where carSchedule.UniqueID == appt.UniqueID select carSchedule).SingleOrDefault();          
            query.UniqueID = appt.UniqueID;
            query.StartDate = appt.StartDate;
            query.EndDate = appt.EndDate;
            query.AllDay = appt.AllDay;
            query.Subject = appt.Subject;
            query.Description = appt.Description;
            query.Location = appt.Location;
            query.RecurrenceInfo = appt.RecurrenceInfo;
            query.ReminderInfo = appt.ReminderInfo;
            query.Status = appt.Status;
            query.Type = appt.Type;
            query.Label = appt.Label;
            query.ResourceID = appt.ResourceID;
            query.Employees = appt.Employees;
            query.RouteID = appt.RouteID;
            query.UserAppro = appt.UserAppro;
            query.Phone = appt.Phone;
            query.ScheduleType = appt.ScheduleType;
            query.CreatedDateTime = appt.CreatedDateTime;
            db.SubmitChanges();
            
           
            appt.UniqueID = appt.GetHashCode();
            db.Appointments.InsertOnSubmit(appt);
            db.SubmitChanges();
        }
        public static void RemoveAppointment(Appointment appt)
        {
            HammerDataContext db = new HammerDataContext();
            Appointment query = (Appointment)(from carSchedule in db.Appointments where carSchedule.UniqueID == appt.UniqueID select carSchedule).SingleOrDefault();
            db.Appointments.DeleteOnSubmit(query);
            db.SubmitChanges();
        }
        public static void RemoveAllAppointment(Appointment appt)
        {
            HammerDataContext db = new HammerDataContext();
            List<Appointment> query = (from carSchedule in db.Appointments 
                                              where carSchedule.StartDate.Value.Date == appt.StartDate.Value.Date 
                                              && carSchedule.UserLogin.Trim() == appt.UserLogin.Trim()
                                              select carSchedule).ToList();
            db.Appointments.DeleteAllOnSubmit(query);
            db.SubmitChanges();
        }
    }
    public abstract class ScheduleBase
    {
        public ScheduleBase()
        {
        }
        public ScheduleBase(Appointment carScheduling)
        {
            if (carScheduling != null)
            {
                UniqueID = carScheduling.UniqueID;
                Type = carScheduling.Type;
                Label = carScheduling.Label;
                AllDay = Convert.ToInt16(carScheduling.AllDay);
                Location = carScheduling.Location;
                Phone = carScheduling.Phone;
                Status = carScheduling.Status;
                RecurrenceInfo = carScheduling.RecurrenceInfo;
                ReminderInfo = carScheduling.ReminderInfo;
                Employees = carScheduling.Employees;
                Route = carScheduling.RouteID;
                UserLogin = carScheduling.UserLogin;
            }
        }

        public int UniqueID { get; set; }
        public int? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int AllDay { get; set; }
        public string Subject { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public int? Label { get; set; }
        public string RecurrenceInfo { get; set; }
        public string ReminderInfo { get; set; }
        public string Phone { get; set; }
        public string Employees { get; set; }
        public string Route { get; set; }
        public string UserLogin { get; set; }
        public string UserApprove { get; set; }
        public bool? IsMeeting { get; set; }

        public virtual void Assign(ScheduleBase source)
        {
            if (source != null)
            {
                UniqueID = source.UniqueID;
                Type = source.Type;
                Label = source.Label;
                AllDay = source.AllDay;
                Location = source.Location;
                Phone = source.Phone;
                Employees = source.Employees;
                Status = source.Status;
                RecurrenceInfo = source.RecurrenceInfo;
                ReminderInfo = source.ReminderInfo;
                UserLogin = source.UserLogin;
                Route = source.Route;
                UserApprove = source.UserApprove;
                IsMeeting = source.IsMeeting;
            }
        }
    }
    public class ValidationSchedule : ScheduleBase
    {
        public ValidationSchedule()
        {
        }
        public ValidationSchedule(Appointment carScheduling)
            : base(carScheduling)
        {
            if (carScheduling != null)
            {
                UniqueID = carScheduling.UniqueID;
                Type = carScheduling.Type;
                Label = carScheduling.Label;
                AllDay = Convert.ToInt16(carScheduling.AllDay);
                Location = carScheduling.Location;
                Phone = carScheduling.Phone;
                Status = carScheduling.Status;
                RecurrenceInfo = carScheduling.RecurrenceInfo;
                ReminderInfo = carScheduling.ReminderInfo;
                Employees = carScheduling.Employees;
                RejectReason = carScheduling.RejectReason;
                UserLogin = carScheduling.UserLogin;
                Route = carScheduling.RouteID;
                UserApprove = carScheduling.UserAppro;
                IsMeeting = carScheduling.IsMeeting;
            }
        }

        [Required(ErrorMessage = "The Subject must contain at least one character.")]
        public string Subject { get; set; }
        public string Phone { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Employees { get; set; }
        public string RejectReason { get; set; }
        public string UserLogin { get; set; }

        public override void Assign(ScheduleBase source)
        {
            base.Assign(source);
            ValidationSchedule validationSchedule = source as ValidationSchedule;
            if (validationSchedule != null)
            {
                Subject = validationSchedule.Subject;
                Description = validationSchedule.Description;
                Location = validationSchedule.Location;
                Phone = validationSchedule.Phone;
                Employees = validationSchedule.Employees;
                StartDate = validationSchedule.StartDate;
                EndDate = validationSchedule.EndDate;
                RejectReason = validationSchedule.RejectReason;
                UserLogin = validationSchedule.UserLogin;
                Route = validationSchedule.Route;
                UserApprove = validationSchedule.UserApprove;
                IsMeeting = validationSchedule.IsMeeting;
            }
        }
    }
    public class EditableSchedule : ScheduleBase
    {
        public EditableSchedule()
        {
        }
        public EditableSchedule(Appointment carScheduling)
            : base(carScheduling)
        {
            UniqueID = carScheduling.UniqueID;
            Type = carScheduling.Type;
            Label = carScheduling.Label;
            AllDay = Convert.ToInt16(carScheduling.AllDay);
            Location = carScheduling.Location;
            Phone = carScheduling.Phone;
            Status = carScheduling.Status;
            RecurrenceInfo = carScheduling.RecurrenceInfo;
            ReminderInfo = carScheduling.ReminderInfo;
            Employees = carScheduling.Employees;
            Route = carScheduling.RouteID;
            UserApprove = carScheduling.UserAppro;
            IsMeeting = carScheduling.IsMeeting;
           
        }

        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public override void Assign(ScheduleBase source)
        {
            base.Assign(source);
            EditableSchedule editableSchedule = source as EditableSchedule;
            if (editableSchedule != null)
            {
                
                Subject = editableSchedule.Subject;
                Description = editableSchedule.Description;
                Location = editableSchedule.Location;
                Phone = editableSchedule.Phone;
                Employees = editableSchedule.Employees;
                StartDate = editableSchedule.StartDate;
                EndDate = editableSchedule.EndDate;
                Route = editableSchedule.Route;
                UserApprove = editableSchedule.UserApprove;
                IsMeeting = editableSchedule.IsMeeting;
               
            }
        }
    }

}