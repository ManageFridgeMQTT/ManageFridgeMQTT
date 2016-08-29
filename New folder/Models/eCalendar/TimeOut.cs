using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hammer.Models.ReportHRTimekeepingTableAdapters;

namespace Hammer.Models
{
    public class TimeOut : DMS_pp_ReportHRDetailTableAdapter
    {
        public override int Fill(ReportHRTimekeeping.DMS_pp_ReportHRDetailDataTable dataTable, DateTime? StartDate, DateTime? EndDate, string User, string RegionID)
        {
            this.CommandCollection[0].CommandTimeout = 0;
            return base.Fill(dataTable, StartDate, EndDate, User, RegionID);
        }
    }
}