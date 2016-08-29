using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eRoute.Models;
using DMSERoute.Helpers;
using System.ComponentModel.DataAnnotations;

namespace eRoute.Models.ViewModel
{
    public class ReportImageCaptureVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Int32 displayID { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public decimal greaterThan { get; set; }
        public decimal smallerThan { get; set; }

        public string showType { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<DMSDisplay> listDisplay { get; set; }

        //public List<pp_ReportImageCapture_Result> listItem { get; set; }
    }
    public class ReportImageCaptureMTDVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Int32 displayID { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public decimal greaterThan { get; set; }
        public decimal smallerThan { get; set; }

        public string showType { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<DMSDisplay> listDisplay { get; set; }

        //public List<pp_ReportImageCaptureMTD_Result> listItem { get; set; }
    }

    public class ReportImageCaptureSMVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Int32 displayID { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public decimal greaterThan { get; set; }
        public decimal smallerThan { get; set; }

        public string showType { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<DMSDisplay> listDisplay { get; set; }

        //public List<pp_ReportImageCaptureSM_Result> listItem { get; set; }
    }

    public class ReportImagePassBySalesmanMTDVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Int32 displayID { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public decimal greaterThan { get; set; }
        public decimal smallerThan { get; set; }

        public string showType { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<DMSDisplay> listDisplay { get; set; }

        //public List<pp_ReportImagePassBySalesmanMTD_Result> listItem { get; set; }
    }

    public class ReportImageLastSMVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Int32 displayID { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public decimal greaterThan { get; set; }
        public decimal smallerThan { get; set; }

        public string showType { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<DMSDisplay> listDisplay { get; set; }

        //public List<pp_ReportImageLastSM_Result> listItem { get; set; }
    }

    public class ReportImageLastMTDVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Int32 displayID { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public decimal greaterThan { get; set; }
        public decimal smallerThan { get; set; }

        public string showType { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<DMSDisplay> listDisplay { get; set; }

        //public List<pp_ReportImageLastMTD_Result> listItem { get; set; }
    }


    public class ReportImageLastCaptureSMVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Int32 displayID { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public decimal greaterThan { get; set; }
        public decimal smallerThan { get; set; }

        public string showType { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<DMSDisplay> listDisplay { get; set; }

        //public List<pp_ReportImageLastCaptureSM_Result> listItem { get; set; }
    }

    public class ReportImageLastCaptureMTDVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public Int32 displayID { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public decimal greaterThan { get; set; }
        public decimal smallerThan { get; set; }

        public string showType { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<DMSDisplay> listDisplay { get; set; }

        //public List<pp_ReportImageLastCaptureMTD_Result> listItem { get; set; }
    }

    public class ReportSalesVM
    {
        public string txt_date { get; set; }
        public string txt_salesman { get; set; }
        public SelectList selectListS { get; set; }
        public Salesman salesman { get; set; }
        public List<Salesman> listS { get; set; }
        public Distributor distributor { get; set; }
        public Route route { get; set; }
        public List<Outlet> listOutlets { get; set; }
        public string totalOutlet { get; set; }
        public List<OrderHeader> listOutletOrders { get; set; }
        public string totalOutletOrder { get; set; }
        public string totalOutletVisit { get; set; }
        public List<OrderHeader> listOutletOutRanges { get; set; }
        public string totalOutletOutRange { get; set; }
        public OrderHeader orderFirstVisit { get; set; }
        public OrderHeader orderLastVisit { get; set; }
        public List<ReportItem> listInfo { get; set; }

        public List<MerchandiseImage> listImage { get; set; }
        public string array_time { get; set; }
        public string array_time_move { get; set; }
        public string maxTimeVisit { get; set; }
        public string minTimeVisit { get; set; }
        public string divAverage { get; set; }
        public string countInfo { get; set; }

        public string totalQuantity { get; set; }
        public string totalAmount { get; set; }
        public string totalSKU { get; set; }
        public string totalOrder { get; set; }
        public string totalLPPC { get; set; }

        public DateTime Date { get; set; }

        public DMSSalesForce SaleSup { get; set; }
        public DMSSalesForce ASM { get; set; }
    }

    public class ReportItem
    {
        public Outlet o { get; set; }
        public OrderHeader od { get; set; }
        public VisitPlanHistory vph { get; set; }

        public string TimeMove { get; set; }
        public string TimeVisit { get; set; }

        public TimeSpan tsTimeMove { get; set; }
        public TimeSpan tsTimeVisit { get; set; }

        public int VisitOrderReal { get; set; }
        public string VisitOrder { get; set; }
        public string OutletID { get; set; }
        public string OutletName { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Distance { get; set; }
        public string OutOfRange { get; set; }
        public string HasOrder { get; set; }
        public string TotalSKU { get; set; }
        public string DropSize { get; set; }
        public string TotalAmount { get; set; }
        public string Reason { get; set; }

        public string visitImage { get; set; }
        public string outletImage { get; set; }

        public string markImage { get; set; }
        public string MarkResult { get; set; }
        public string MarkFailReason { get; set; }
        public string Auditor { get; set; }
        public string strMarkDate { get; set; }
        public string DisplayName { get; set; }

        public decimal Lat { get; set; }
        public decimal Long { get; set; }
        public decimal SMLat { get; set; }
        public decimal SMLong { get; set; }

        public string GPSType { get; set; }
        public string IsEnableAirPlaneMode { get; set; }
        public string IsEnableGPSMode { get; set; }
        public string IsEnableNetworkMode { get; set; }

    }

    public class MarkResult
    {
        public string VisitID { get; set; }
        public string OutletID { get; set; }
        public string SalemanID { get; set; }
        public int DistributorID { get; set; }

        public string AuditorName { get; set; }
        public string AuditorPhone { get; set; }
        public string AuditorEmail { get; set; }

        public DateTime MarkDate { get; set; }
        public string strMarkDate { get; set; }

        public decimal Mark { get; set; }
        public string Comment { get; set; }

        public string MarkName { get; set; }
        public string ReasonNotPass { get; set; }

        public string ImageFile { get; set; }

        public string DisplayName { get; set; }
    }

    public class ReportSyncItem
    {
        //public v_ReportFirstSync rpItem { get; set; }
        public pp_ReportSyncResult rpItem { get; set; }
        public string strUsePDATime { get; set; }
        public string firstTimeVisit { get; set; }
        public string lastTimeVisit { get; set; }
        public string syncTime { get; set; }

        public string RouteID { get; set; }
        public string SalesmanID { get; set; }
        public string SalesmanName { get; set; }
        public string Phone { get; set; }
        public string FirstOutlet { get; set; }
        public string LastOutlet { get; set; }
    }

    public class ReportSyncVM
    {
        public List<ReportSyncItem> listM { get; set; }
        public string txt_date { get; set; }
        public Distributor distributor { get; set; }
        public string firstTimeVisit { get; set; }
        public string lastTimeVisit { get; set; }
        public string firstTimeSync { get; set; }
        public string lastTimeSync { get; set; }
        public string minTimeCheckOut { get; set; }
        public string maxTimeCheckOut { get; set; }
    }

    public class ReportDistributorVM
    {
        public string txt_date { get; set; }
        public int branch { get; set; }
        //public List<SalesByDisResult> listItem { get; set; }
        public Distributor distributor { get; set; }
        public List<Distributor> ListD { get; set; }
        public string totalOutlet { get; set; }
        public string totalOutletVisit { get; set; }
        public string totalOrder { get; set; }
        public string totalQuantity { get; set; }
        public string totalSKU { get; set; }
        public string totalLPPC { get; set; }
        public string totalSOMCP { get; set; }
        public string totalVisitMCP { get; set; }
    }

    public class ReportAttendanceVM
    {
        public int DistributorID { get; set; }
        public string SalesSupID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public TimeSpan FirstTimeSync { get; set; }
        public TimeSpan FirstTimeVisit { get; set; }
        public TimeSpan LastTimeVisit { get; set; }
        public int OrderCount { get; set; }
        public decimal OrderDistance { get; set; }
        public int MinTimeVisit { get; set; }
        public int MinTimeMove { get; set; }

        public List<pp_ReportAttendanceResult> listItem { get; set; }

        public decimal decfirstTimeSync { get; set; }
        public decimal decfirstTimeVisit { get; set; }
        public decimal declastTimeVisit { get; set; }
        public Distributor distributor { get; set; }
    }

    public class ReportSalemanKPIVM
    {
        public List<pp_ReportSalemanDayMonitoringResult> ListItem { get; set; }

        public string RegionID { get; set; }
        public string AreaID { get; set; }
        public string ProvinceID { get; set; }
        public int DistributorID { get; set; }
        public string SalesSupID { get; set; }
        public string RouteID { get; set; }
        public DateTime FromDate { get; set; }
        public string strFromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string strToDate { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        public decimal KPIMandayLoss { get; set; }
        public decimal NonGPSPercent { get; set; }
        public decimal OrderDistanceValid { get; set; }
        public decimal OrderDistancePercent { get; set; }
        public decimal NonVisitPercent { get; set; }
        public TimeSpan FirstTimeVisit { get; set; }
        public TimeSpan LastTimeVisit { get; set; }

        public decimal decFirstTimeVisit { get; set; }
        public decimal decLastTimeVisit { get; set; }
    }

    public class ReportUserActionLogVM
    {
        public List<pp_GetUserInfoResult> ListItem { get; set; }
        public DateTime FromDate { get; set; }
        public string strFromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string strToDate { get; set; }

        public List<pp_GetUserInfoResult> ListUser { get; set; }
        public string loginID { get; set; }
        public Int32 userID { get; set; }
    }


    public class SalesForceMCPTrackingVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        public List<pp_SalesForceMCPTrackingResult> listItem { get; set; }
    }

    public class OutletLocationUpdateVM : ViewModelBase
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public string loginID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<pp_OutletLocationUpdateResult> listItem { get; set; }
    }

    public class ViewModelBase
    {
        public string Message { get; set; }
        public string Error { get; set; }
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ReportWorkWithVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        //public List<rpWorkWithItem> listItem { get; set; }
    }

    public class ReportReviewWorkWithVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public Int32 smallerThan { get; set; }
        public Int32 sMHourValid { get; set; }
        public Int32 aSMSSSMMinuteValid { get; set; }
        public Int32 aSMSSDistanceValid { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        //public List<pp_ReportReviewWorkWith_Result> listItem { get; set; }
    }

    public class rpVisitInfoVMItem
    {
        public pp_GetVisitInfoResult item { get; set; }
        public string SMTime { get; set; }
        public string SUPTime { get; set; }
        public string ASMTime { get; set; }
    }

    //public class ReportTotalOrderOfDayVM
    //{
    //    public string strFromDate { get; set; }
    //    public string strToDate { get; set; }

    //    public DateTime FromDate { get; set; }
    //    public DateTime ToDate { get; set; }

    //    public string regionID { get; set; }
    //    public string areaID { get; set; }
    //    public string provinceID { get; set; }
    //    public Int32 distributorID { get; set; }
    //    public string saleSupID { get; set; }
    //    public string routeID { get; set; }
    //    public Int32 employeeID { get; set; }
    //    public Int32 leaderID { get; set; }

    //    public List<DMSRegion> ListRegion { get; set; }
    //    public List<DMSArea> ListArea { get; set; }
    //    //public List<DMSProvince> ListProvince { get; set; }
    //    //public List<Distributor> ListDistributor { get; set; }
    //    //public List<DMSSalesForce> ListSForce { get; set; }
    //    //public List<Route> ListRoute { get; set; }
    //    public Int32 showAll { get; set; }

    //    public List<pp_PC_Result> listItem { get; set; }
    //}


    public class ReportTotalOrderOfDayMTDVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        //public List<DMSProvince> ListProvince { get; set; }
        //public List<Distributor> ListDistributor { get; set; }
        //public List<DMSSalesForce> ListSForce { get; set; }
        //public List<Route> ListRoute { get; set; }
        public Int32 showAll { get; set; }

        public List<pp_PCMTDResult> listItem { get; set; }
    }

    public class ReportTotalOrderOfDaySMVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        //public List<DMSProvince> ListProvince { get; set; }
        //public List<Distributor> ListDistributor { get; set; }
        //public List<DMSSalesForce> ListSForce { get; set; }
        //public List<Route> ListRoute { get; set; }
        public Int32 IsGreater { get; set; }
        public Int32 Percent { get; set; }
        public Int32 showAll { get; set; }

        public List<pp_PC_SMResult> listItem { get; set; }
    }

    public class ReportTotalOrderOfDaySMDailyVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        //public List<DMSProvince> ListProvince { get; set; }
        //public List<Distributor> ListDistributor { get; set; }
        //public List<DMSSalesForce> ListSForce { get; set; }
        //public List<Route> ListRoute { get; set; }
        public Int32 IsGreater { get; set; }
        public Int32 Percent { get; set; }
        public Int32 showAll { get; set; }

        //public List<pp_PC_SM_Daily_Result> listItem { get; set; }
    }

    public class ReportUserUseMobilityVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<pp_ReportUserUseMobilityResult> listItem { get; set; }
    }


    public class ReportLastOfDayVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        //public List<pp_ReportLastOfDay_Result> listItem { get; set; }
    }

    public class SMAjaxVM
    {
        public string code { get; set; }
        public string visittime { get; set; }
        public string name { get; set; }
        public string Avatar { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
    }

    public class SMMovementVM
    {
        public string SalemanCode { get; set; }
        public string strVisitTime { get; set; }
        public DateTime VisitTime { get; set; }
        public string SalemanName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
    }

    public class ReportSMVisitSummaryVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public string salesmanID { get; set; }

        public TimeSpan? firstTimeSync { get; set; }
        public TimeSpan? firstTimeVisitAM { get; set; }
        public TimeSpan? firstTimeVisitPM { get; set; }
        public TimeSpan? lastTimeVisit { get; set; }
        public TimeSpan? TimeDuration { get; set; }
        public decimal orderDistanceValid { get; set; }
        public int TimeVisit { get; set; }
        //public TimeSpan firstTimeSync { get; set; }
        //public TimeSpan firstTimeVisitAM { get; set; }
        //public TimeSpan firstTimeVisitPM { get; set; }
        //public TimeSpan lastTimeVisit { get; set; }
        //public decimal orderDistanceValid { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        public List<pp_ReportSMVisitSummaryResult> listItem { get; set; }
    }

    public class ReportOrderIndexVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public string salesmanID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        public List<pp_ReportOrderIndexLevelResult> listItem { get; set; }
    }

    public class ReportSalesAssessmentVM
    {
        public string strFromDate { get; set; }

        public DateTime FromDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public string salesmanID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        public List<pp_ReportSalesAssessmentResult> listItem { get; set; }
    }

    public class ReportPCVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public string salesmanID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        public string report { get; set; }

        public List<pp_ReportPC_SMResult> listItemSM { get; set; }
        public List<pp_ReportPC_MTDResult> listItemMTD { get; set; }
        public List<pp_ReportPC_SM_DailyResult> listItemSM_Daily { get; set; }

        public Int32 IsGreater { get; set; }
        public Int32 Percent { get; set; }
        public Int32 showAll { get; set; }
    }

    public class ReportOutletInvalidLocationVM
    {
        public string strFromDate { get; set; }

        public DateTime FromDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public string salesmanID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        //public List<pp_GetOutletInvalidLocationResult> listItem { get; set; }
        public DataTable listItem { get; set; }
    }

    public class ReportVisitVM
    {
        public string strFromDate { get; set; }

        public DateTime FromDate { get; set; }
        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public string salesmanID { get; set; }
        public string employeeID { get; set; }
        public string leaderID { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<pp_ReportVisitResult> listItem { get; set; }

        public ViewControlCombobox listComboboxRegion { get; set; }
        public ViewControlCombobox listComboboxArea { get; set; }
        public ViewControlCombobox listComboboxDistributor { get; set; }

    }
    public class StatusIssues
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
    public class ReportIssuesModel
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string regionID { get; set; }
        public string areaID { get; set; }
        public Int32 distributorID { get; set; }
        public string routeID { get; set; }
        public int status { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<Route> ListRoute { get; set; }
        public List<StatusIssues> ListStatus { get; set; }

        public List<pp_ReportIssuesResult> listItem { get; set; }
    }

    #region ReportSummarySales
    public class MV_ReportSummarySales
    {
        public MV_ReportSummarySales()
        {
            this.FromDate = DateTime.Now;
            this.ToDate = DateTime.Now;
            this.regionID = String.Empty;
            this.areaID = String.Empty;
            this.distributorID = 0;
            this.routeID = String.Empty;

            this.ReportSummarySalesResult = new List<pp_ReportSummarySalesResult>();
            this.ReportSummarySalesDetailResult = new List<pp_ReportSummarySalesDetailResult>();

        }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string regionID { get; set; }
        public string areaID { get; set; }
        public Int32 distributorID { get; set; }
        public string routeID { get; set; }

        public ViewControlCombobox listComboboxRegion { get; set; }
        public ViewControlCombobox listComboboxArea { get; set; }
        public ViewControlCombobox listComboboxDistributor { get; set; }
        public ViewControlCombobox listComboboxRoute { get; set; }
        public ViewControlCombobox listComboboxSaleSup { get; set; }

        public List<pp_ReportSummarySalesResult> ReportSummarySalesResult { get; set; }
        public List<pp_ReportSummarySalesDetailResult> ReportSummarySalesDetailResult { get; set; }
    }
    #endregion

    #region MV_ReportDrugstores
    public class MV_ReportDrugstores
    {
        public MV_ReportDrugstores()
        {
            this.regionID = String.Empty;
            this.areaID = String.Empty;
            this.distributorID = 0;
            this.routeID = String.Empty;

            this.ReportDugstoresResult = new List<pp_ReportDrugstoresResult>();
        }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public Int32 distributorID { get; set; }
        public string routeID { get; set; }

        public ViewControlCombobox listComboboxRegion { get; set; }
        public ViewControlCombobox listComboboxArea { get; set; }
        public ViewControlCombobox listComboboxDistributor { get; set; }
        public ViewControlCombobox listComboboxRoute { get; set; }

        public List<pp_ReportDrugstoresResult> ReportDugstoresResult { get; set; }

    }
    #endregion

    #region MV_ReportIssues
    public class MV_ReportIssues
    {
        public MV_ReportIssues()
        {
            this.FromDate = DateTime.Now;
            this.ToDate = DateTime.Now;
            this.regionID = String.Empty;
            this.areaID = String.Empty;
            this.distributorID = 0;
            this.routeID = String.Empty;
            this.StatusID = 0;

            this.ReportIssuesResult = new List<pp_ReportIssuesResult>();
            this.ReportIssuesDetailResult = new List<pp_ReportIssuesDetailResult>();
        }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string regionID { get; set; }
        public string areaID { get; set; }
        public Int32 distributorID { get; set; }
        public string routeID { get; set; }
        public int StatusID { get; set; }

        public string IssuesID { get; set; }
        public string SalesmanCode { get; set; }
        public string OutletID { get; set; }
        public string DistributorCode { get; set; }
        public string Content { get; set; }
        public string Resolve { get; set; }
        public string NewResolve { get; set; }
        public string ImgConment { get; set; }
        public DateTime VisitDate { get; set; }

        public ViewControlCombobox listComboboxRegion { get; set; }
        public ViewControlCombobox listComboboxArea { get; set; }
        public ViewControlCombobox listComboboxDistributor { get; set; }
        public ViewControlCombobox listComboboxRoute { get; set; }
        public ViewControlCombobox listComboboxStatus { get; set; }

        public List<pp_ReportIssuesResult> ReportIssuesResult { get; set; }
        public List<pp_ReportIssuesDetailResult> ReportIssuesDetailResult { get; set; }
        public List<pp_ReportIssuesDetailConfirmResult> ReportIssuesDetailConfirmResult { get; set; }
    }
    #endregion

    #region NewErouteWithNewLayout
    public class MV_ReportVisit
    {
        public MV_ReportVisit()
        {
            this.FromDate = DateTime.Now;
            this.ToDate = DateTime.Now;
            this.regionID = String.Empty;
            this.areaID = String.Empty;
            this.provinceID = String.Empty;
            this.distributorID = 0;
            this.saleSupID = String.Empty;
            this.routeID = String.Empty;
            this.salesmanID = String.Empty;
            this.employeeID = String.Empty;
            this.leaderID = String.Empty;
            this.TimeVisit = 0;

            this.ReportVisitResult = new DataTable();
            this.ReportSyscResult = new List<pp_ReportSyschronousResult>();
            this.ReportSaleDailyResult = new List<pp_ReportSMVisitSummaryResult>();
            this.ReportOutletValidLocalResult = new List<pp_ReportOutletInvalidLocationResult>();
            this.ReportSaleEffectiveResult = new List<pp_ReportSalesAssessmentResult>();
            this.ReportAssessmentPCResult = new List<pp_ReportPC_SMResult>();
            this.ReportReviewWorkWithResult = new List<pp_ReportReviewWorkWithResult>();
            this.ReportWorkWithResult = new List<pp_ReportWorkWithResult>();
        }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public string salesmanID { get; set; }
        public string employeeID { get; set; }
        public string leaderID { get; set; }
        public int TimeVisit { get; set; }
        public TimeSpan TimeRegular { get; set; }

        public ViewControlCombobox listComboboxRegion { get; set; }
        public ViewControlCombobox listComboboxArea { get; set; }
        public ViewControlCombobox listComboboxDistributor { get; set; }
        public ViewControlCombobox listComboboxRoute { get; set; }
        public ViewControlCombobox listComboboxSaleSup { get; set; }

        public DataTable ReportVisitResult { get; set; }
        public List<pp_GetBLSalesKPIResult> ReportVisitSummary { get; set; }
        public List<pp_ReportImageProgramResult> ReportImageProgram { get; set; }
        public List<pp_ReportSyschronousResult> ReportSyscResult { get; set; }
        public List<pp_ReportSMVisitSummaryResult> ReportSaleDailyResult { get; set; }
        public List<pp_ReportOutletInvalidLocationResult> ReportOutletValidLocalResult { get; set; }
        public List<pp_ReportSalesAssessmentResult> ReportSaleEffectiveResult { get; set; }
        public List<pp_ReportPC_SMResult> ReportAssessmentPCResult { get; set; }
        public List<pp_ReportReviewWorkWithResult> ReportReviewWorkWithResult { get; set; }
        public List<pp_ReportWorkWithResult> ReportWorkWithResult { get; set; }
        public List<SelectListItem> listTypeChart { get; set; }
        public List<SelectListItem> listGroup { get; set; }
    }

   

    public class ReportVisitGallery
    {
        public string NameSceen;
        public Utility.StatusAutoMark status { get; set; }
        public DMSEvaluation Evaluation { get; set; }
        public bool CheckConnectMatLab { get; set; }
        public string Display { get; set; }
        public List<uvw_GetDisplayInformation> DisplayData { get; set; }
        public int TotalImages { get; set; }
        public int ImagesProgress { get; set; }
        public int ImageMarking { get; set; }
        public int ImagePesen { get; set; }
        public int TotalOutlet { get; set; }
        public int OutletMarking { get; set; }
        public int OutletRemain { get; set; }
        public int TimePlanMarking { get; set; }
        public double TimeMarking { get; set; }
        public int ImgNotExist { get; set; }
        public int ImgErrorMarking { get; set; }
        public int ImgThat { get; set; }
        public int ImgChuan { get; set; }
        public int ImgPass { get; set; }
        public int ImgNumberic { get; set; }
        public int ImgFakes { get; set; }
        public int ImgNotStandard { get; set; }
        public int ImgNotPass { get; set; }
        public int ImgNotPassNumberic { get; set; }
        public double TimeAverage { get; set; }
        public DateTime DateAutoMarking { get; set; }
        public string strDateFinish { get; set; }
        public string strTimeMarking { get; set; }
        public List<EvaluationImageClass> ImagesList { get; set; }
        public List<usp_GetListImageGalleryResult> ListImageBy { get; set; }
        public List<usp_GetListImageGalleryHeaderResult> ListHeader { get; set; }
        public ReportVisitGallery()
        {
            this.CheckConnectMatLab = false;
            this.status = Utility.StatusAutoMark.New;
            this.TotalImages = 0;
            this.ImagesProgress = 0;
            this.ImageMarking = 0;
            this.ImagePesen = 0;
            this.TotalOutlet = 0;
            this.OutletMarking = 0;
            this.OutletRemain = 0;
            this.TimePlanMarking = 0;
            this.TimeMarking = 0;
            this.ImgNotExist = 0;
            this.ImgErrorMarking = 0;
            this.ImgThat = 0;
            this.ImgChuan = 0;
            this.ImgPass = 0;
            this.ImgNumberic = 0;
            this.ImgFakes = 0;
            this.ImgNotStandard = 0;
            this.ImgNotPass = 0;
            this.ImgNotPassNumberic = 0;
            this.TimeAverage = 0;
            this.DateAutoMarking = DateTime.Now;
            Evaluation = new DMSEvaluation();
            ImagesList = new List<EvaluationImageClass>();
            ListImageBy = new List<usp_GetListImageGalleryResult>();
        }
    }
    //end
    public class MV_DashBoard
    {
        public MV_DashBoard()
        {
            this.TotalSM = 0;
            this.TotalSMHasVisit = 0;
            this.TotalSMHasOrder = 0;
            this.TotalSMSyncLateWithTime = 0;
            this.TotalOutletHasVisit = 0;
            this.TotalVisitPlan = 0;
            this.TotalOrder = 0;
            this.TotalSMSyncLate = 0;
            this.TotalAmount = 0;
            this.MTDTotalAmount = 0;
            this.TotalSMHasSync = 0;
            this.TotalRestVisit = 0;
            this.VisitDate = DateTime.Now;
            this.isDashboard = true;
        }

        public DateTime VisitDate { get; set; }
        public int TotalSM { get; set; }
        public int TotalSMHasVisit { get; set; }
        public int TotalSMHasOrder { get; set; }
        public int TotalSMSyncLateWithTime { get; set; }

        public int TotalOutletHasVisit { get; set; }
        public int TotalVisitPlan { get; set; }
        public int TotalRestVisit { get; set; }
        public int TotalOrder { get; set; }
        public int TotalSMSyncLate { get; set; }
        public int TotalSMHasSync { get; set; }

        public List<pp_ReportDashBoardResult> ListSMHasSync { get; set; }
        public List<pp_ReportDashBoardResult> ListSMNotSync { get; set; }
        public List<pp_ReportDashBoardResult> ListSMHasVisited { get; set; }
        public List<pp_ReportDashBoardResult> ListSMNotVisit { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal MTDTotalAmount { get; set; }
        public bool isDashboard { get; set; }
        //Danh cho ChamDiem
        public List<usp_GetEvaluationDetailWithUserResult> EvalDefinitionResult { get; set; }
        public List<usp_GetEvaluationUserByResult> ReviewListDetail { get; set; }
        public List<usp_GetOutletsImageReviewWithUserHeaderResult> ReviewListHeader { get; set; }

    }

    public class MV_PieTable
    {
        public PieData PieData { get; set; }
        public DataTable Tables { get; set; }
    }

    public class MV_ChartTabe
    {
        public ChartData ChartData { get; set; }
        public DataTable Tables { get; set; }
    }

    public class MV_ChartDashBoard
    {
        public ChartData ChartDataRevenue { get; set; }
        public ChartData ChartDataVisit { get; set; }
        public ChartData ChartDataOther { get; set; }
        public DataTable Tables { get; set; }
    }

    public class MV_ChartBox
    {
        public string NameTab { get; set; }
        public Utility.ChartName TypeChart { get; set; }
        public ChartData Chart { get; set; }
        public DataTable Tables { get; set; }
    }

    public class ReportReviewWorkWithNewVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public Int32 smallerThan { get; set; }
        public Int32 sMHourValid { get; set; }
        public Int32 aSMSSSMMinuteValid { get; set; }
        public Int32 aSMSSDistanceValid { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        public List<pp_ReportReviewWorkWithResult> listItem { get; set; }
    }


    public class ReportWorkWithNewVM
    {
        public string strFromDate { get; set; }
        public string strToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public string provinceID { get; set; }
        public Int32 distributorID { get; set; }
        public string saleSupID { get; set; }
        public string routeID { get; set; }
        public Int32 employeeID { get; set; }
        public Int32 leaderID { get; set; }
        public Int32 aSMSSSMMinuteValid { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }
        public List<DMSProvince> ListProvince { get; set; }
        public List<Distributor> ListDistributor { get; set; }
        public List<DMSSalesForce> ListSForce { get; set; }
        public List<Route> ListRoute { get; set; }

        public List<pp_ReportWorkWithResult> listItem { get; set; }
    }

    public class InventoryItemVM
    {
        public InventoryItemVM()
        {
            this.LastModifiedDateTime = DateTime.Now;
            ApplicationCD = Utility.TypeData.ETool.ToString();
        }

        [Required]
        [Remote("CheckInventoryCDExist", "Issues", HttpMethod = "POST", ErrorMessage = "Inventory code already exists. Please enter a different Inventory code.")]
        public string InventoryCD { get; set; }

        [Required]
        public string Descr { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be at long {1} characters long.")]
        public string ShortName { get; set; }

        [Required]
        public int SubBranchID { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at long {1} characters long.")]
        public string SubBranchCD { get; set; }
        [Required]
        public int VendorID { get; set; }
        [Required]
        public string VendorName { get; set; }
        public string ApplicationCD { get; set; }
        public int CreatedByID { get; set; }
        public int CreatedByScreenID { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int LastModifiedByID { get; set; }
        public int LastModifiedByScreenID { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public string tstamp { get; set; }
    }



    public class ReportEvalVM
    {
        public string Screen { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public DateTime? ImageFromDate { get; set; }
        public DateTime? ImageToDate { get; set; }
        public DateTime? MarkFromDate { get; set; }
        public DateTime? MarkToDate { get; set; }

        public string ProgramID { get; set; }
        public string EvaluationID { get; set; }

        public string RegionID { get; set; }
        public string AreaID { get; set; }
        public string ProvinceID { get; set; }
        public int DistributorID { get; set; }
        public string RouteID { get; set; }
        public string OutletID { get; set; }
        public string SaleSupID { get; set; }
        public string SalesmanID { get; set; }

        public string LeaderID { get; set; }
        public string Auditor { get; set; }

        public string UserName { get; set; }

        public int StatusEval { get; set; }
        public int TypeOfSS { get; set; }
        public int GroupInventory { get; set; }
        public int GroupSaleteam { get; set; }
        public Utility.RoleName RoleView { get; set; }


        public ViewControlCombobox listComboboxRegion { get; set; }
        public ViewControlCombobox listComboboxArea { get; set; }
        public ViewControlCombobox listComboboxDistributor { get; set; }
        public ViewControlCombobox listComboboxRoute { get; set; }
        public ViewControlCombobox listComboboxOutlet { get; set; }
        public ViewControlCombobox listComboboxSaleSup { get; set; }
        public ViewControlCombobox listComboboxSaleman { get; set; }
        public ViewControlCombobox listComboboxProgram { get; set; }
        public ViewControlCombobox listComboboxEvaluation { get; set; }
        public ViewControlCombobox listComboboxAuditor { get; set; }
        public List<SelectListItem> ListStatusEval { get; set; }
        public List<SelectListItem> ListTypeOfSS { get; set; }
        public List<SelectListItem> ListGroupInventory { get; set; }
        public List<SelectListItem> ListGroupSaleteam { get; set; }

        public List<ResultReportEvalVM> listResult { get; set; }
        public List<usp_GetReportEvalImageByResult> DataEvalImage { get; set; }
        public DataTable TableResult { get; set; }

        public ReportEvalVM()
        {
            this.FromDate = DateTime.Now;
            this.ToDate = DateTime.Now;
            this.TypeOfSS = 0;
            this.StatusEval = 0;
            this.RegionID = String.Empty;
            this.AreaID = String.Empty;
            this.DistributorID = 0;
            this.SaleSupID = String.Empty;
            this.RouteID = String.Empty;
            this.OutletID = String.Empty;
            this.SalesmanID = String.Empty;
            this.LeaderID = String.Empty;
            this.Auditor = String.Empty;
            this.SetRoleView();
            this.GroupInventory = 0;
            this.GroupSaleteam = 0;
            this.TableResult = new DataTable();
        }
        public void SetRoleView()
        {
            int RoleID = SessionHelper.GetSession<int>("RoleUser");
            if (RoleID == (int)Utility.RoleName.Leader)
            {
                this.RoleView = Utility.RoleName.Leader;
            }
            else if (RoleID == (int)Utility.RoleName.Auditor)
            {
                this.Auditor = SessionHelper.GetSession<string>("UserName");
                this.RoleView = Utility.RoleName.Auditor;
            }
            else if (RoleID == (int)Utility.RoleName.Admin)
            {
                this.RoleView = Utility.RoleName.Admin;
            }
            else if (RoleID == (int)Utility.RoleName.RSM)
            {
                //this.regionID = SessionHelper.GetSession<string>("UserName");
                this.RoleView = Utility.RoleName.RSM;
            }
            else if (RoleID == (int)Utility.RoleName.ASM)
            {
                //this.areaID = SessionHelper.GetSession<string>("UserName");
                this.RoleView = Utility.RoleName.ASM;
            }
            else if (RoleID == (int)Utility.RoleName.SS || RoleID == (int)Utility.RoleName.TDV)
            {
                //this.saleSupID = SessionHelper.GetSession<string>("UserName");
                this.RoleView = Utility.RoleName.SS;
            }
            else
            {
                this.RoleView = Utility.RoleName.SuperAdmin;
            }
        }

        public void SetDataReportEvaluation(string strFromDate = "", string strToDate = "", string ProgramID = "", string RegionID = "", string AreaID = "", int DistributorID = 0, string RouteID = "", string OutletID = "", string SaleSupID = "", string SalesmanID = "", string EvaluationID = "", int TypeOfSS = 0, int GroupInventory = 0, int GroupSaleteam = 0)
        {
            if (!string.IsNullOrEmpty(strFromDate))
            {
                this.FromDate = Utility.DateTimeParse(strFromDate);
            }
            else
            {
                this.FromDate = null;
            }
            if (!string.IsNullOrEmpty(strToDate))
            {
                this.ToDate = Utility.DateTimeParse(strToDate);
            }
            else
            {
                this.ToDate = null;
            }
            this.RegionID = Utility.StringParse(RegionID);
            this.AreaID = Utility.StringParse(AreaID);
            this.SaleSupID = Utility.StringParse(SaleSupID);
            this.DistributorID = DistributorID;
            this.RouteID = Utility.StringParse(RouteID);
            this.OutletID = Utility.StringParse(OutletID);
            this.SalesmanID = Utility.StringParse(SalesmanID);
            this.ProgramID = Utility.StringParse(ProgramID);
            this.EvaluationID = Utility.StringParse(EvaluationID);
            this.TypeOfSS = TypeOfSS;
            this.GroupInventory = GroupInventory;
            this.GroupSaleteam = GroupSaleteam;

            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = this.RegionID;
            ctrCombobox.TitleKey = Utility.Phrase("RegionID");
            ctrCombobox.TitleName = Utility.Phrase("RegionName");
            ctrCombobox.listOption = ControllerHelper.GetListRegion(this.RegionID).Select(s => new OptionCombobox { ID = s.RegionID, Key = s.RegionID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            this.listComboboxRegion = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = this.AreaID;
            ctrCombobox.TitleKey = Utility.Phrase("AreaID");
            ctrCombobox.TitleName = Utility.Phrase("AreaName");
            ctrCombobox.listOption = ControllerHelper.GetListArea(this.RegionID).Select(s => new OptionCombobox { ID = s.AreaID, Key = s.AreaID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            this.listComboboxArea = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("DistributorCode");
            ctrCombobox.TitleName = Utility.Phrase("DistributorName");
            ctrCombobox.SeleteID = this.DistributorID.ToString();
            ctrCombobox.listOption = ControllerHelper.GetListDistributorWithRegionArea(this.RegionID, this.AreaID).Select(s => new OptionCombobox { ID = s.DistributorID.ToString(), Key = s.DistributorCode.ToString(), Value = s.DistributorName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            this.listComboboxDistributor = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("SaleSupID");
            ctrCombobox.TitleName = Utility.Phrase("SaleSupName");
            ctrCombobox.SeleteID = this.SaleSupID;
            ctrCombobox.listOption = ControllerHelper.GetListSaleSup(this.RegionID, this.AreaID, "", this.DistributorID).Select(s => new OptionCombobox { ID = s.EmployeeID.ToString(), Key = s.EmployeeID.ToString(), Value = s.EmployeeName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            this.listComboboxSaleSup = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("RouteID");
            ctrCombobox.TitleName = Utility.Phrase("RouteName");
            ctrCombobox.SeleteID = this.RouteID;
            if (this.DistributorID != 0)
            {
                ctrCombobox.listOption = ControllerHelper.GetListRouteWithRegionAreaDis(this.RegionID, this.AreaID, this.DistributorID).Select(s => new OptionCombobox { ID = s.RouteID.ToString(), Key = s.RouteID.ToString(), Value = s.RouteName }).ToList();
            }
            if (ctrCombobox.listOption.Count == 2)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[1].ID;
            }
            this.listComboboxRoute = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("OutletID");
            ctrCombobox.TitleName = Utility.Phrase("OutletName");
            ctrCombobox.SeleteID = this.OutletID;
            if (this.DistributorID != 0 && !string.IsNullOrEmpty(this.RouteID))
            {
                ctrCombobox.listOption = ControllerHelper.GetListOutlet(this.DistributorID, this.RouteID, this.SalesmanID).Select(s => new OptionCombobox { ID = s.OutletID.ToString(), Key = s.OutletID.ToString(), Value = s.OutletName }).ToList();
            }
            //if (ctrCombobox.listOption.Count == 1)
            //{
            //    ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            //}
            this.listComboboxOutlet = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("SalesmanID");
            ctrCombobox.TitleName = Utility.Phrase("SalesmanName");
            ctrCombobox.SeleteID = this.SalesmanID;
            ctrCombobox.listOption = ControllerHelper.ListSalesman.Select(s => new OptionCombobox { ID = s.SalesmanID.ToString(), Key = s.SalesmanID.ToString(), Value = s.SalesmanName }).ToList();
            this.listComboboxSaleman = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("ProgramName");
            ctrCombobox.TitleName = Utility.Phrase("TimeProgram");
            ctrCombobox.listOption = Global.VisibilityContext.uvw_GetDisplayInformations.ToList().Select(s => new OptionCombobox { ID = s.MaCTTB.ToString(), Key = s.ChuongTrinhTrungBay.ToString(), Value = s.ThoiGianBatDau + " - " + s.ThoiGianKetThuc }).ToList();
            ctrCombobox.listOption.Insert(0, new OptionCombobox() { ID = string.Empty, Key = string.Empty, Value = string.Empty });
            ctrCombobox.SeleteID = this.ProgramID;
            this.listComboboxProgram = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("EvaluationName");
            ctrCombobox.TitleName = Utility.Phrase("TimeEvaluation");
            ctrCombobox.listOption = Global.VisibilityContext.DMSEvaluations.Select(s => new OptionCombobox { ID = s.EvaluationID.ToString(), Key = s.EvaluationID.ToString(), Value = s.EvalDateFrom.Date.ToString() + " - " + s.EvalDateTo.Date.ToString() }).ToList();
            ctrCombobox.listOption.Insert(0, new OptionCombobox() { ID = string.Empty, Key = string.Empty, Value = string.Empty });
            ctrCombobox.SeleteID = this.EvaluationID;
            this.listComboboxEvaluation = ctrCombobox;

            List<SelectListItem> listStatusEval = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("SelectAll"), Value = "0", Selected = (this.StatusEval == 0) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_1"), Value = "1", Selected = (this.StatusEval == 1) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_2"), Value = "2", Selected = (this.StatusEval == 2) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_3"), Value = "3", Selected = (this.StatusEval == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_4"), Value = "4", Selected = (this.StatusEval == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_5"), Value = "5", Selected = (this.StatusEval == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_6"), Value = "6", Selected = (this.StatusEval == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_7"), Value = "7", Selected = (this.StatusEval == 3) ? true : false },
            };
            this.ListStatusEval = listStatusEval;
            List<SelectListItem> listTypeOfSS = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("SelectAll"), Value = "0", Selected = (this.StatusEval == 0) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("TypeOfSS_1"), Value = "1", Selected = (this.StatusEval == 1) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("TypeOfSS_2"), Value = "2", Selected = (this.StatusEval == 2) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("TypeOfSS_3"), Value = "3", Selected = (this.StatusEval == 3) ? true : false }
            };
            this.ListTypeOfSS = listTypeOfSS;
            List<SelectListItem> listGroupInventory = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("Product"), Value = "0", Selected = (this.StatusEval == 0) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Category"), Value = "1", Selected = (this.StatusEval == 1) ? true : false }
            };
            this.ListGroupInventory = listGroupInventory;
            List<SelectListItem> listGroupSaleteam = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("All"), Value = "0", Selected = (this.StatusEval == 0) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Route"), Value = "1", Selected = (this.StatusEval == 1) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Saleman"), Value = "2", Selected = (this.StatusEval == 2) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Outlet"), Value = "3", Selected = (this.StatusEval == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("RouteSaleman"), Value = "4", Selected = (this.StatusEval == 4) ? true : false }
            };
            this.ListGroupSaleteam = listGroupSaleteam;
        }

        public void SetDataReportEvalDetailByImage(string ProgramID = "", string EvaluationID = "") {
            #region InputParam
            this.ProgramID = Utility.StringParse(ProgramID);
            this.EvaluationID = Utility.StringParse(EvaluationID);
            #endregion


            #region SET Combobox
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("ProgramName");
            ctrCombobox.TitleName = Utility.Phrase("TimeProgram");
            ctrCombobox.listOption = Global.VisibilityContext.uvw_GetDisplayInformations.ToList().Select(s => new OptionCombobox { ID = s.MaCTTB.ToString(), Key = s.ChuongTrinhTrungBay.ToString(), Value = s.ThoiGianBatDau + " - " + s.ThoiGianKetThuc }).ToList();
            ctrCombobox.listOption.Insert(0, new OptionCombobox() { ID = string.Empty, Key = string.Empty, Value = string.Empty });
            ctrCombobox.SeleteID = this.ProgramID;
            this.listComboboxProgram = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("EvaluationName");
            ctrCombobox.TitleName = Utility.Phrase("TimeEvaluation");
            ctrCombobox.listOption = Global.VisibilityContext.DMSEvaluations.Select(s => new OptionCombobox { ID = s.EvaluationID.ToString(), Key = s.EvaluationID.ToString(), Value = s.EvalDateFrom.Date.ToString() + " - " + s.EvalDateTo.Date.ToString() }).ToList();
            ctrCombobox.listOption.Insert(0, new OptionCombobox() { ID = string.Empty, Key = string.Empty, Value = string.Empty });
            ctrCombobox.SeleteID = this.EvaluationID;
            this.listComboboxEvaluation = ctrCombobox; 
            #endregion
        }
    }

    public class ResultReportEvalVM
    {
        public string RegionID { get; set; }
        public string RegionName { get; set; }
        public string AreaID { get; set; }
        public string AreaName { get; set; }
        public string SaleSupID { get; set; }
        public string SaleSupName { get; set; }
        public int DistributorID { get; set; }
        public string DistributorName { get; set; }
        public string RouteID { get; set; }
        public string RouteName { get; set; }
        public string SalesmanID { get; set; }
        public string SalesmanName { get; set; }
        public string ProgramID { get; set; }
        public string ProgramName { get; set; }
        public string EvaluationID { get; set; }
        public int EvalState { get; set; }
        public int ReviewRate { get; set; }
        public string OutletID { get; set; }
        public string OutletName { get; set; }
        public string Adress { get; set; }
        public DateTime? ImageDate { get; set; }
        public int TotalAuditor { get; set; }
        public string MarkingAssign { get; set; }
        public int TotalOulet { get; set; }
        public int OuletHasMarking { get; set; }
        public int TotalImg { get; set; }
        public int ImagMarking { get; set; }
        public int ImgFake { get; set; }
        public int NotCaptured { get; set; }
        public int ImgPassProgram { get; set; }
        public int ImgApproved { get; set; }
        public int ImgRejected { get; set; }
        public int ImgReMarking { get; set; }
        public int ImgReReview { get; set; }
        public int ImgHasReview { get; set; }
        public bool PassProgram { get; set; }
        public decimal PassRate { get; set; }
        public int ImgNotPassProgram { get; set; }
        public int EvalReviewRate { get; set; }
    }
    #endregion

    public class MV_ReviewOrder
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int Status { get; set; }
        public string SalesmanID { get; set; }
        public List<pp_ReportGetListReviewOrderResult> ReportGetListReviewOrder { get; set; }
        public List<SelectListItem> ListStatus { get; set; }
        public ViewControlCombobox ListComboboxSaleMan { get; set; }

    public MV_ReviewOrder()
        {
            this.Status = 0;
            this.SalesmanID = string.Empty;
            ListStatus = new List<SelectListItem>(){
                new SelectListItem(){ Text = "Đơn hàng duyệt giao hàng", Value = "1" },
                new SelectListItem(){ Text = "Đơn hàng chưa duyệt", Value = "0" },
                new SelectListItem(){ Text = "Đơn hàng duyệt không giao hàng", Value = "-1" }
            };
        }
    }

    public class MV_ReviewOrderDetail
    {
        public pp_ReportGetListReviewOrderResult OderDetailInfor { get; set; }
        public List<OrderDetail> ListOrder { get; set; }
    }

    public class MV_KeyOrderHeader
    {
        public int DistributorId { get; set; }
        public string Code { get; set; }
        public string OutletId { get; set; }
        public string SalemanId { get; set; }
        public string VisitDate { get; set; }
    }
}