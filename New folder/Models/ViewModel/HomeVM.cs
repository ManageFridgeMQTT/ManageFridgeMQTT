using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eRoute.Models.ViewModel
{
    public class HomeVM
    {
        public List<Distributor> listDis { get; set; }
        public List<DMSSalesForce> listASM { get; set; }
        public List<DMSSalesForce> listSS { get; set; }
        //public List<Salesman> listSaleman { get; set; }
        public DateTime VisitDate { get; set; }
        public string strHour { get; set; }
        public string strDate { get; set; }

        public List<DMSRegion> ListRegion { get; set; }
        public List<DMSArea> ListArea { get; set; }

        public string regionID { get; set; }
        public string areaID { get; set; }
        public int distributorID { get; set; }
        public string saleSupID { get; set; }

        public List<Map_Salesman> listSaleman { get; set; }
        public List<pp_GetCategoryItem_HeatMapResult> listCategory { get; set; }
        public List<pp_GetCategoryItem_HeatMapResult> listItem { get; set; }
        public int CategoryID { get; set; }
    }

    public class Map_Salesman
    {
        public string SalesmanID { get; set; }
        public string SalesmanName { get; set; }
    }

    public class Map_RouteCD
    {
        public string RouteID { get; set; }
        public int DistributorID { get; set; }
    }

    public class OutletOnMap
    {
        public string OutletID { get; set; }
        public string OutletName { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Long { get; set; }
    }
    public class OutletImportLocation
    {
        public string DistributorCode { get; set; }
        public string RouteCD { get; set; }
        public string OutletID { get; set; }
        public string OutletName { get; set; }
        public decimal? Lat { get; set; }
        public decimal? Long { get; set; }
        public string ImportMessage { get; set; }
    }

    public class OutletImportLocationVM : ViewModelBase
    {
        public List<OutletImportLocation> listImport { get; set; }
    }

    public class SlideVM
    {
        public List<string> listM { get; set; }
        public Outlet outlet { get; set; }
    }

    public class CustomerOnMapVM
    {
        public string OutletID { get; set; }
        public string OutletName { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Attn { get; set; }
        public string Address { get; set; }
        public string Latitude { get; set; }
        public string Longtitude { get; set; }
        public int VisitOrder { get; set; }
        public string SalesmanID { get; set; }
        public string SalesmanName { get; set; }
        public string ODLatitude { get; set; }
        public string ODLongtitude { get; set; }
        public string ODOutletID { get; set; }
        public string VisitDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ImageFile { get; set; }
        public string Status { get; set; }

        public List<orderVM> listOD { get; set; }

        public string VisitID { get; set; }
        public int ODVisitOrder { get; set; }

        public CustomerOnMapVM()
        {
            OutletID = string.Empty;
            OutletName = string.Empty;
            Phone = string.Empty;
            Mobile = string.Empty;
            Attn = string.Empty;
            Address = string.Empty;
            Latitude = string.Empty;
            Longtitude = string.Empty;
            VisitOrder = 0;
            SalesmanID = string.Empty;
            SalesmanName = string.Empty;
            ODLatitude = string.Empty;
            ODLongtitude = string.Empty;
            ODOutletID = string.Empty;
            StartTime = string.Empty;
            EndTime = string.Empty;
            ImageFile = string.Empty;
            Status = string.Empty;
        }
    }

    public class orderVM
    {
        public string Code { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TotalQty { get; set; }
        public string TotalAmt { get; set; }
        public string DropSize { get; set; }
        public string TotalSKU { get; set; }
        public string SalesmanID { get; set; }
        public string Reason { get; set; }
        public string Distance { get; set; }
        public string Status { get; set; }
    }



    public class OutletInRoute
    {
        public string ASMID { get; set; }
        public string ASMName { get; set; }
        public string SaleSupID { get; set; }
        public string SaleSupName { get; set; }
        public int DistributorID { get; set; }
        public string DistributorCode { get; set; }
        public string DistributorName { get; set; }
        public string RouteID { get; set; }
        public string RouteName { get; set; }
        public string SalesmanID { get; set; }
        public string SalesmanName { get; set; }
        public string VisitDate { get; set; }
        public decimal VisitOrder { get; set; }
        public string OutletID { get; set; }
        public string OutletName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longtitude { get; set; }
        public string ImageFile { get; set; }
        public decimal RenderOrder { get; set; }
        public string MarkerColor { get; set; }
        public decimal HasOrder { get; set; }
        public decimal HasVisit { get; set; }
        public decimal ISMCP { get; set; }

        public int T2 { get; set; }
        public int T3 { get; set; }
        public int T4 { get; set; }
        public int T5 { get; set; }
        public int T6 { get; set; }
        public int T7 { get; set; }
        public int CN { get; set; }

        public string strVisitInWeek { get; set; }

        public List<OutletSMVisit> ListSMVisit { get; set; }
        public List<OutletSSVisit> ListSSVisit { get; set; }
        public List<OutletASMVisit> ListASMVisit { get; set; }
        public List<OutletVisitImage> ListVisitImage { get; set; }
    }

    public class OutletSMVisit
    {
        public int DistributorID { get; set; }
        public string OutletID { get; set; }

        public string DropSize { get; set; }
        public string TotalAmt { get; set; }
        public string TotalSKU { get; set; }
        public string Reason { get; set; }
        public string SMTimeStart { get; set; }
        public string SMTimeEnd { get; set; }
        public decimal SMLatitude { get; set; }
        public decimal SMLongitude { get; set; }
        public string SMDistance { get; set; }

        public decimal ISMCP { get; set; }
        public decimal HasOrder { get; set; }
        public decimal HasVisit { get; set; }
        public decimal RN { get; set; }
        public string MarkerColor { get; set; }
        public string Code { get; set; }

        public string ImageFile { get; set; }
    }

    public class OutletSSVisit
    {
        public int DistributorID { get; set; }
        public string OutletID { get; set; }

        public string SUPTimeStart { get; set; }
        public string SUPTimeEnd { get; set; }
        public decimal SUPLatitudeStart { get; set; }
        public decimal SUPLongtitudeStart { get; set; }
        public string SUPDistance { get; set; }
    }

    public class OutletASMVisit
    {
        public int DistributorID { get; set; }
        public string OutletID { get; set; }
        public string ASMTimeStart { get; set; }
        public string ASMTimeEnd { get; set; }
        public decimal ASMLatitudeStart { get; set; }
        public decimal ASMLongtitudeStart { get; set; }
        public string ASMDistance { get; set; }
    }

    public class OutletVisitImage
    {
        public System.Nullable<int> DistributorID { get; set; }
        public string OutletID { get; set; }
        public string ImageFile { get; set; }
        public System.Nullable<DateTime> VisitDate { get; set; }
        public string strVisitDate { get; set; }
    }
}