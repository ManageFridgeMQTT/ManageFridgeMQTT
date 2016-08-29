using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hammer.Models
{
    public class ScheduleModel
    {
        public string EmployeeIDG { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Outlet { get; set; }
        public string PhoneNumber { get; set; }
        public string WWCode { get; set; }
        public bool IsMeeting { get; set; }
    }
    public class ScheduleExcelModel
    {
        public string EmployeeIDG { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string WWCode { get; set; }       
    }
    public class EmployeeModel
    {
        public string RSM { get; set; }
        public string ASM { get; set; }
        public string SS { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Level { get; set; }
        public string RouteID { get; set; }
    }
    public class ScheduleErrorModel
    {
        public string EmployeeID { get; set; }
        public string emailTemplate { get; set; }
        public int No { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Outlet { get; set; }
        public string PhoneNumber { get; set; }
        public string WWCode { get; set; }
        public string RouteID { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
        public bool IsMeeting { get; set; }
    }
}