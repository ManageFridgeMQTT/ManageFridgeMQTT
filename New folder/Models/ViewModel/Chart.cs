using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eRoute.Models.ViewModel
{
    public class ColumnData
    {
        public string name { get; set; }
        public bool visible { get; set; }
        public string color { get; set; }
        public List<decimal> data { get; set; }
    }

    public class ColumnDataStr
    {
        public string name { get; set; }
        public bool visible { get; set; }
        public List<string> data { get; set; }
    }

    public class ChartData
    {
        public List<ColumnData> listSeries { get; set; }
        public string chartName { get; set; }
        
        public string YName { get; set; }
        public List<string> listColumns { get; set; }

        public string targetName { get; set; }
        public List<int> listTarget { get; set; }
    }

    public class ChartDataStr
    {
        public List<ColumnDataStr> listSeries { get; set; }
        public string chartName { get; set; }

        public string YName { get; set; }
        public List<string> listColumns { get; set; }

        public string targetName { get; set; }
        public List<int> listTarget { get; set; }
    }


    public class PieColumnData
    {
        public string name { get; set; }
        public decimal y { get; set; }
    }

    public class PieData
    {
        public List<PieColumnData> listSeries { get; set; }
        public string chartName { get; set; }
        public string tooltips { get; set; }
    }

    public class RenderDataHeatMap
    {
        public string Level1 { get; set; }
        public string Level2 { get; set; }
        public string Level3 { get; set; }
        public string Level4 { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string InventoryItem { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalOutlet { get; set; }
        public decimal TotalCoverage { get; set; }
        public decimal RatioQuantity { get; set; }
    }
    public class ReportInfoOutLetVisit
    {
        public string SalesmanID { get; set; }
        public string SalesmanName { get; set; }
        public string SaleSupID { get; set; }
        public string SaleSupName { get; set; }
        public string DistributorID { get; set; }
        public string OutLetID { get; set; }
        public string OutLetName { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Duration { get; set; }
        public string TimeSpanMove { get; set; }
        public string strIsMCP { get; set; }
        public string Distance { get; set; }
        public string IsEnableAirPlaneMode { get; set; }
        public string IsEnableGPSMode { get; set; }
        public string IsEnableNetworkMode { get; set; }
        public string ImageFile { get; set; }
        public string OutletVisited { get; set; }
        public string OutletRemain { get; set; }
        public string OrderCount { get; set; }
        public string Compliance { get; set; }
        public int HasOrder { get; set; }
        public string strTotalAmout { get; set; }
    }
    public class ReportSMVisitSummaryChartData
    {
        public string Level1 { get; set; }
        public string Level2 { get; set; }
        public string Level3 { get; set; }
        public string Level4 { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OrderCount { get; set; }
        public decimal TotalSKU { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal? LPPC { get; set; }
        public decimal OrderDistanceInvalid { get; set; }
        public decimal? SOMCP { get; set; }
        public decimal? RatioRevenue { get; set; }
        public decimal? TargetRevenue { get; set; }
        public decimal? VisitMCP { get; set; }
        public decimal OutletMustVisit { get; set; }
        public decimal OutletVisited { get; set; }
        public string strTotalAmount { get; set; }
        public string strOrderCount { get; set; }
        public string strTotalSKU { get; set; }
        public string strTotalQuantity { get; set; }
        public string strLPPC { get; set; }
        public string strOrderDistanceInvalid { get; set; }
        public string strSOMCP { get; set; }
        public string strVisitMCP { get; set; }
        public string strOutletMustVisit { get; set; }
        public string strOutletVisited { get; set; }
    }

    public class DashBoardGridData
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public decimal OutletMustVisit { get; set; }
        public decimal OutletVisited { get; set; }
        public decimal Visit_MCP { get; set; }
        public decimal OrderCount { get; set; }
        public decimal SO_MCP { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalSKU { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal LPPC { get; set; }

        public string strOutletMustVisit { get; set; }
        public string strOutletVisited { get; set; }
        public string strVisit_MCP { get; set; }
        public string strOrderCount { get; set; }
        public string strSO_MCP { get; set; }
        public string strTotalAmount { get; set; }
        public string strTotalSKU { get; set; }
        public string strTotalQuantity { get; set; }
        public string strLPPC { get; set; }
    }

    public class ReportPieTable
    {
        public string Name { get; set; }
        public string Rate { get; set; }
        public decimal Count { get; set; }
    }

    public class ChartPercentage
    {
        public string Name { get; set; }
        public decimal Target  { get; set; }
        public decimal Achieved { get; set; }
        public decimal Rest { get; set; }
        public decimal OrderCount { get; set; }
        public decimal AchievedPercentage { get; set; }
        public decimal RestPercentage { get; set; }
        public decimal AchievedPlan { get; set; }
        public decimal TargetDay { get; set; }
    }

    public class ChartSync
    {
        public string Name { get; set; }
        public decimal SMSyncInValid { get; set; }
        public decimal SMNotData { get; set; }
        public decimal SMDistanceInvalid { get; set; }
        public decimal SMTimeInValid { get; set; }
        public decimal TotalSM { get; set; }
    }

    public class CumulativeRevenue
    {
        public string Name { get; set; }
        public DateTime Day { get; set; }
        public decimal Value { get; set; }
    }
}
