using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eRoute.Models.ViewModel
{
    public enum EnumSessionNames
    {
        Language,
        UserRole,
        UserFullName,
        Username,
    }
    public enum TypeDate
    {
        D,
        W,
        M,
        Q,
        Y
    }
    public enum ReportType
    {
        ReportBLSalesIn,
        ReportBLSales,
        ReportBLPromotion,
        ReportBLKPI,
        ReportInventoryRealtime,
        ReportBLINInventoryStock,
        ReportBLInventoryTracking,
        ReportBLProgramTracking,
        ReportMasterInventory
    }
}