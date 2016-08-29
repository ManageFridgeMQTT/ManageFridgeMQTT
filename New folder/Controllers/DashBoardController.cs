using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using DevExpress.Data.PivotGrid;
using DevExpress.Utils;
using DevExpress.Web.Mvc;
using DevExpress.XtraPivotGrid;
using DMSERoute.Helpers;
using DMSERoute.Helpers.Html;
using eRoute.Filters;
using eRoute.Models;
using eRoute.Models.ViewModel;
using System.Web;
using System.Web.Caching;
using System.Data;
using System.Data.Objects;
using System.IO;

namespace eRoute.Controllers
{
    [Authorize]
    [LogAndRedirectOnError]
    public class DashBoardController : Controller
    {
        #region Index
        [CompressFilter]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region TerritoryPartial
        [ActionAuthorize("DashBoard_TerritoryPartial")]
        [CompressFilter]
        public ActionResult TerritoryPartial_SMSSASM(FormCollection formParam)
        {
            var model = new HomeVM();

            model.listSaleman = new List<Map_Salesman>();

            model.VisitDate = DateTime.Now;
            model.strDate = DateTime.Now.ToString(Constant.ShortDatePattern);

            model.ListRegion = ControllerHelper.GetListRegion(string.Empty);
            model.ListArea = ControllerHelper.GetListArea(string.Empty);
            model.listSS = ControllerHelper.GetListSaleSup(string.Empty);
            model.listDis = ControllerHelper.GetListDistributor(string.Empty, string.Empty);

            return PartialView(model);
        }

        #region AJAX
        [ActionAuthorize("GetRouteByUser")]
        [CompressFilter]
        public ActionResult GetRouteByUser(string routeCD, int? distributorID, string regionID, string areaID)
        {
            routeCD = Utility.StringParse(routeCD);
            distributorID = Utility.IntParse(distributorID);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            //var j = Global.Context.pp_GetRouteByUser(routeCD, distributorID, regionID, areaID, SessionHelper.GetSession<string>("UserName")).ToList();
            //return Json(new
            //{
            //    html = j
            //});
            return null;
        }

        [ActionAuthorize("GetOutletInRoute")]
        [CompressFilter]
        public ActionResult GetOutletInRoute(string routeCD, string salesmanID, int? distributorID, string strVisitDate)
        {
            #region PARAM
            routeCD = Utility.StringParse(routeCD);
            salesmanID = Utility.StringParse(salesmanID);
            distributorID = Utility.IntParse(distributorID);
            DateTime visitDate = CacheDataHelper.DashBoardDate; //Utility.DateTimeParse(strVisitDate);
            #endregion

            #region GET DATA
            var j = Global.Context.pp_GetVisitInfo(routeCD, distributorID, string.Empty, string.Empty, salesmanID, visitDate, SessionHelper.GetSession<string>("UserName")).ToList();
            var listOutletInRoute = (from a in j
                                     select new OutletInRoute()
                                     {
                                         ASMID = a.ASMID,
                                         ASMName = a.ASMName,
                                         SaleSupID = a.SaleSupID,
                                         SaleSupName = a.SaleSupName,
                                         DistributorID = a.DistributorID,
                                         RouteID = a.RouteID,
                                         RouteName = a.RouteName,
                                         SalesmanID = a.SalesmanID,
                                         SalesmanName = a.SalesmanName,
                                         VisitDate = a.VisitDate.ToShortPattern(),
                                         VisitOrder = Utility.DecimalParse(a.VisitOrder),
                                         RenderOrder = Utility.DecimalParse(a.RenderOrder),
                                         OutletID = a.OutletID,
                                         OutletName = a.OutletName,
                                         Address = a.Address,
                                         Phone = a.Phone,
                                         Latitude = a.Latitude,
                                         Longtitude = a.Longtitude,
                                         ImageFile = a.ImageFile,
                                         MarkerColor = a.MarkerColor,
                                         ISMCP = a.ISMCP
                                     }).Distinct().ToList();
            var listSMVisit = (from a in j
                               where a.SMTimeStart != null
                                    && a.SMLatitude != 0
                                    && a.SMLongitude != 0
                               select new OutletSMVisit()
                               {
                                   DistributorID = a.DistributorID,
                                   OutletID = a.OutletID,
                                   DropSize = Utility.StringParse(a.DropSize),
                                   TotalAmt = Utility.StringParse(a.TotalAmt),
                                   TotalSKU = Utility.StringParse(a.TotalSKU),
                                   Reason = a.Reason,
                                   SMTimeStart = a.SMTimeStart,
                                   SMTimeEnd = a.SMTimeEnd,
                                   SMLatitude = a.SMLatitude,
                                   SMLongitude = a.SMLongitude,
                                   SMDistance = Utility.StringParse(a.SMDistance),
                                   ISMCP = a.ISMCP,
                                   HasOrder = a.HasOrder,
                                   HasVisit = a.HasVisit,
                                   RN = Utility.DecimalParse(a.RenderOrder),
                                   MarkerColor = a.MarkerColor
                               }).Distinct().ToList();
            var listSSVisit = (from a in j
                               where a.SUPTimeStart != null
                                    && a.SUPLatitudeStart != 0
                                    && a.SUPLongtitudeStart != 0
                               select new OutletSSVisit()
                               {
                                   DistributorID = a.DistributorID,
                                   OutletID = a.OutletID,
                                   SUPTimeStart = a.SUPTimeStart,
                                   SUPTimeEnd = a.SUPTimeEnd,
                                   SUPLatitudeStart = Utility.DecimalDParse(a.SUPLatitudeStart),
                                   SUPLongtitudeStart = Utility.DecimalDParse(a.SUPLongtitudeStart),
                                   SUPDistance = Utility.StringParse(a.SUPDistance)
                               }).Distinct().ToList();

            var listASMVisit = (from a in j
                                where a.ASMTimeStart != null
                                    && a.ASMLatitudeStart != 0
                                    && a.ASMLongtitudeStart != 0
                                select new OutletASMVisit()
                                {
                                    DistributorID = a.DistributorID,
                                    OutletID = a.OutletID,
                                    ASMTimeStart = a.ASMTimeStart,
                                    ASMTimeEnd = a.ASMTimeEnd,
                                    ASMLatitudeStart = Utility.DecimalDParse(a.ASMLatitudeStart),
                                    ASMLongtitudeStart = Utility.DecimalDParse(a.ASMLongtitudeStart),
                                    ASMDistance = Utility.StringParse(a.ASMDistance)
                                }).Distinct().ToList();

            #region MERGE DATA
            foreach (OutletInRoute oir in listOutletInRoute)
            {
                oir.ListSMVisit = new List<OutletSMVisit>();
                oir.ListSSVisit = new List<OutletSSVisit>();
                oir.ListASMVisit = new List<OutletASMVisit>();

                oir.ListSMVisit.AddRange(listSMVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.SMTimeStart).ToList());
                oir.ListSSVisit.AddRange(listSSVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.SUPTimeStart).ToList());
                oir.ListASMVisit.AddRange(listASMVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.ASMTimeStart).ToList());

                if (oir.ListSMVisit.Count > 0)
                {
                    oir.HasVisit = 1;
                    if (oir.ListSMVisit.Exists(a => a.MarkerColor == "blue"))
                    {
                        oir.HasOrder = 1;
                        oir.MarkerColor = "blue";
                    }
                }
            }
            #endregion
            #endregion

            listOutletInRoute = listOutletInRoute.OrderBy(n => n.RenderOrder).ToList();
            var routeInfo = Global.Context.pp_GetRouteInfoByUser(routeCD, distributorID, string.Empty, string.Empty, string.Empty, visitDate, SessionHelper.GetSession<string>("UserName")).FirstOrDefault();
            listSMVisit = listSMVisit.OrderBy(a => a.SMTimeStart).ToList();
            listSSVisit = listSSVisit.OrderBy(a => a.SUPTimeStart).ToList();
            listASMVisit = listASMVisit.OrderBy(a => a.ASMTimeStart).ToList();

            return Json(new
            {
                html = listOutletInRoute,
                route = routeInfo,
                listSMVisit,
                listSSVisit,
                listASMVisit
            });
        }

        [ActionAuthorize("RenderListSMLastLocation")]
        [CompressFilter]
        public ActionResult RenderListSMLastLocation(string strSMSelected, int? distributorID, string salesSupID, string strVisitDate)
        {
            DateTime visitDate = CacheDataHelper.DashBoardDate; //Utility.DateTimeParse(strVisitDate);
            distributorID = Utility.IntParse(distributorID);
            salesSupID = Utility.StringParse(salesSupID);
            strSMSelected = Utility.StringParse(strSMSelected);

            List<string> listSM = new List<string>();
            if (!string.IsNullOrEmpty(strSMSelected))
            {
                listSM = strSMSelected.Split(',').Where(a => a != string.Empty).Distinct().ToList();
            }

            var listSMLastLocation = new List<pp_GetSalemanLastLocationResult>();//ControllerHelper.GetSalemanLastLocationResult(distributorID.Value, salesSupID, string.Empty, visitDate).ToList();
            if (PermissionHelper.CheckPermissionByFeature("SalesSup") || PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                listSMLastLocation = ControllerHelper.GetSalemanLastLocationResult(distributorID.Value, salesSupID, string.Empty, visitDate).ToList();
            }

            if (!string.IsNullOrEmpty(strSMSelected))
            {
                var listItem = (from sm in listSMLastLocation
                                join s in listSM
                                on sm.SalesmanID equals s
                                select sm
                                ).Distinct().ToList();

                return Json(new
                {
                    html = listItem
                });
            }

            return Json(new
            {
                html = listSMLastLocation
            });
        }

        
        [CompressFilter]
        public ActionResult RenderListASMLastLocation(string regionID, string areaID, string strVisitDate)
        {
            DateTime visitDate = CacheDataHelper.DashBoardDate;//Utility.DateTimeParse(strVisitDate);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);

            var listItem = new DataTable();
            if (PermissionHelper.CheckPermissionByFeature("RSM") || PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                listItem = ControllerHelper.GetASMVisitInfoResult(regionID, areaID, string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, visitDate);
            }

            return Json(new
            {
                html = listItem
            });
        }

        [ActionAuthorize("RenderListSSLastLocation")]
        [CompressFilter]
        public ActionResult RenderListSSLastLocation(string regionID, string areaID, string salesupID, int? distributorID, string strVisitDate)
        {
            DateTime visitDate = CacheDataHelper.DashBoardDate;
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            salesupID = Utility.StringParse(salesupID);
            distributorID = Utility.IntParse(distributorID);

            var listItem = new DataTable();
            if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                listItem = ControllerHelper.GetSUPVisitInfoResult(regionID, areaID, string.Empty, string.Empty, distributorID.Value, salesupID, string.Empty, string.Empty, visitDate);
            }

            return Json(new
            {
                html = listItem
            });
        }
        #endregion

        [ActionAuthorize("DashBoard_TerritoryPartial")]
        [CompressFilter]
        public ActionResult Chart_2_Partial(FormCollection formParam)
        {
            return PartialView();
        }

        [ActionAuthorize("DashBoard_TerritoryPartial")]
        [CompressFilter]
        public ActionResult Chart_1_Partial(FormCollection formParam)
        {
            return PartialView();
        }
        #endregion

        #region ReportSalesAssessmentChart
        public ActionResult ReportSalesAssessmentChart(string groupby)
        {
            //new List<string>(Global.Context.ExecuteQuery<string>("SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE'"));

            var model = (from sm in CacheDataHelper.CacheSalesAssessmentResult()
                                                          join ut in ControllerHelper.ListRoute
                                                              //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                                                          on sm.RouteID equals ut.RouteID
                                                          where
                                                              sm.DistributorID == ut.DistributorID
                                                              && sm.RouteID == ut.RouteID
                                                          select sm).Distinct().ToList();


            groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }
            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(MTDTotalAmount) as TotalAmount, SUM(MTDOrderCount) as OrderCount, SUM(MTDTotalSKU) as TotalSKU, SUM(MTDTotalQuantity) as TotalQuantity, AVERAGE(MTDLPPC) as LPPC, AVERAGE(MTDSO_MCP) as SOMCP, AVERAGE(MTDVisit_MCP) as VisitMCP)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ReportSMVisitSummaryChartData()
                {
                    Name = item.Name,
                    TotalAmount = item.TotalAmount,
                    OrderCount = item.OrderCount,
                    TotalSKU = item.TotalSKU,
                    TotalQuantity = item.TotalQuantity,
                    LPPC = item.LPPC,
                    SOMCP = item.SOMCP,
                    VisitMCP = item.VisitMCP,
                });
            }

            #region Prepare Data For Chart
            var listColumns = (from item in result orderby item.Name select item.Name).Distinct().ToList();
            var seriesTotalAmount = (from item in result orderby item.Name select item.TotalAmount).Distinct().ToList();
            var seriesOrderCount = (from item in result orderby item.Name select (decimal)item.OrderCount).Distinct().ToList();
            var seriesTotalSKU = (from item in result orderby item.Name select item.TotalSKU).Distinct().ToList();
            var seriesTotalQuantity = (from item in result orderby item.Name select item.TotalQuantity).Distinct().ToList();
            var seriesLPPC = (from item in result orderby item.Name select (decimal)item.LPPC).Distinct().ToList();
            var seriesSOMCP = (from item in result orderby item.Name select (decimal)item.SOMCP).Distinct().ToList();
            var seriesVisitMCP = (from item in result orderby item.Name select (decimal)item.VisitMCP).Distinct().ToList();
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.listSeries = new List<ColumnData>();
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalAmount"), visible = false, data = seriesTotalAmount });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OrderCount"), visible = false, data = seriesOrderCount });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalSKU"), visible = false, data = seriesTotalSKU });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalQuantity"), visible = true, data = seriesTotalQuantity });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("LPPC"), visible = false, data = seriesLPPC });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SOMCP"), visible = false, data = seriesSOMCP });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("VisitMCP"), visible = false, data = seriesVisitMCP });
            chartData.listColumns = new List<string>();
            chartData.listColumns.AddRange(listColumns);
            chartData.chartName = Utility.Phrase("ChartMTD");// "Biểu đồ MTD";
            chartData.YName = "";
            #endregion

            return Json(new
            {
                chartData
            }, JsonRequestBehavior.AllowGet);
            //return View();
        }

        public ActionResult ReportSalesAssessmentChartDaily(string groupby)
        {

            var model = (from sm in CacheDataHelper.CacheSalesAssessmentResult()
                                                          join ut in ControllerHelper.ListRoute
                                                              //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                                                          on sm.RouteID equals ut.RouteID
                                                          where
                                                              sm.DistributorID == ut.DistributorID
                                                              && sm.RouteID == ut.RouteID
                                                          select sm).Distinct().ToList();


            groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }

            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(OrderCount) as OrderCount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, AVERAGE(SO_MCP) as SOMCP, AVERAGE(Visit_MCP) as VisitMCP)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ReportSMVisitSummaryChartData()
                {
                    Name = item.Name,
                    TotalAmount = item.TotalAmount,
                    OrderCount = item.OrderCount,
                    TotalSKU = item.TotalSKU,
                    TotalQuantity = item.TotalQuantity,
                    LPPC = item.LPPC,
                    SOMCP = item.SOMCP,
                    VisitMCP = item.VisitMCP,
                });
            }

            #region Prepare Data For Chart
            var listColumns = (from item in result orderby item.Name select item.Name).Distinct().ToList();
            var seriesTotalAmount = (from item in result orderby item.Name select item.TotalAmount).Distinct().ToList();
            var seriesOrderCount = (from item in result orderby item.Name select (decimal)item.OrderCount).Distinct().ToList();
            var seriesTotalSKU = (from item in result orderby item.Name select item.TotalSKU).Distinct().ToList();
            var seriesTotalQuantity = (from item in result orderby item.Name select item.TotalQuantity).Distinct().ToList();
            var seriesLPPC = (from item in result orderby item.Name select (decimal)item.LPPC).Distinct().ToList();
            var seriesSOMCP = (from item in result orderby item.Name select (decimal)item.SOMCP).Distinct().ToList();
            var seriesVisitMCP = (from item in result orderby item.Name select (decimal)item.VisitMCP).Distinct().ToList();
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.listSeries = new List<ColumnData>();
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalAmount"), visible = false, data = seriesTotalAmount });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OrderCount"), visible = false, data = seriesOrderCount });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalSKU"), visible = false, data = seriesTotalSKU });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalQuantity"), visible = true, data = seriesTotalQuantity });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("LPPC"), visible = false, data = seriesLPPC });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SOMCP"), visible = false, data = seriesSOMCP });
            chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("VisitMCP"), visible = false, data = seriesVisitMCP });
            chartData.listColumns = new List<string>();
            chartData.listColumns.AddRange(listColumns);
            chartData.chartName = Utility.Phrase("ChartDaily");
            chartData.YName = "";
            #endregion

            return Json(new
            {
                chartData
            }, JsonRequestBehavior.AllowGet);
            //return View();
        }

        public ActionResult ReportSalesAssessmentChartPie(string groupby, string totalby)//
        {
            var model = (from sm in CacheDataHelper.CacheSalesAssessmentResult()
                                                          join ut in ControllerHelper.ListRoute
                                                              //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                                                          on sm.RouteID equals ut.RouteID
                                                          where
                                                              sm.DistributorID == ut.DistributorID
                                                              && sm.RouteID == ut.RouteID
                                                          select sm).Distinct().ToList();


            groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }

            #region Prepare Data For Chart
            var result = new List<PieColumnData>();
            decimal total = (decimal)model.AsQueryable<pp_ReportDashBoardResult>().Sum(totalby);
            if (total > 0)
            {
                var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as name,SUM(" + totalby + ") as y)");
                foreach (dynamic item in dynamicQuery)
                {
                    result.Add(new PieColumnData() { name = item.name, y = item.y * 100 / total });
                }
            }
            #endregion

            #region Set Chart Data
            PieData chartData = new PieData();
            chartData.listSeries = new List<PieColumnData>();
            chartData.listSeries.AddRange(result);
            chartData.chartName = Utility.Phrase("ReportMTDGroupBy" + groupby + "TotalBy" + totalby);//"Biểu đồ MTD";
            chartData.tooltips = Utility.Phrase("ReportTotalBy" + totalby);//"Doanh số";
            #endregion

            return Json(new
            {
                chartData
            }, JsonRequestBehavior.AllowGet);
        }

        #region ReportSalesAssessmentExportRAWData
        public ActionResult GridReportSalesAssessment()
        {
            return PartialView(ControllerHelper.GetSalesAssessment());
        }

        public ActionResult GridReportSalesAssessmentMTD()
        {

            return PartialView(ControllerHelper.GetSalesAssessmentMTD());
        }

        public ActionResult GridReportSalesmanSync()
        {

            return PartialView(ControllerHelper.GetSalesmanSync());
        }

        public static GridViewSettings ReportSalesAssessmentSettings()
        {
            var settings = new GridViewSettings
            {
                Name = "ReportSalesAssessment",
                KeyFieldName = "ID",
                CallbackRouteValues = new { Controller = "DashBoard", Action = "GridReportSalesAssessment" },
                Width = Unit.Percentage(100)
            };
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = false;
            settings.Settings.ShowFilterRowMenu = false;
            settings.Settings.ShowGroupPanel = false;
            settings.Settings.ShowFooter = true;

            settings.Columns.Add(field =>
            {

                field.FieldName = "ID";
                field.Caption = Utility.Phrase("ID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "Name";
                field.Caption = Utility.Phrase("Name");
            });
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstSyncTime";
            //    field.Caption = Utility.Phrase("FirstSyncTime");

            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstStartTimeAM";
            //    field.Caption = Utility.Phrase("FirstStartTimeAM");

            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstStartTimePM";
            //    field.Caption = Utility.Phrase("FirstStartTimePM");

            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LastEndTime";
            //    field.Caption = Utility.Phrase("LastEndTime");

            //});
            //DATA AREA
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOutletMustVisit";
                field.Caption = Utility.Phrase("OutletMustVisit");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOutletVisited";
                field.Caption = Utility.Phrase("OutletVisited");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOrderCount";
                field.Caption = Utility.Phrase("OrderCount");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strTotalSKU";
                field.Caption = Utility.Phrase("TotalSKU");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strTotalQuantity";
                field.Caption = Utility.Phrase("TotalQuantity");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strTotalAmount";
                field.Caption = Utility.Phrase("TotalAmount");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strLPPC";
                field.Caption = Utility.Phrase("LPPC");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strSO_MCP";
                field.Caption = Utility.Phrase("SO_MCP");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strVisit_MCP";
                field.Caption = Utility.Phrase("Visit_MCP");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            return settings;
        }

        public static GridViewSettings ReportSalesAssessmentMTDSettings()
        {
            var settings = new GridViewSettings
            {
                Name = "ReportSalesAssessmentMTD",
                KeyFieldName = "ID",
                CallbackRouteValues = new { Controller = "DashBoard", Action = "GridReportSalesAssessmentMTD" },
                Width = Unit.Percentage(100)
            };
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = false;
            settings.Settings.ShowFilterRowMenu = false;
            settings.Settings.ShowGroupPanel = false;
            settings.Settings.ShowFooter = true;

            settings.Columns.Add(field =>
            {

                field.FieldName = "ID";
                field.Caption = Utility.Phrase("ID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "Name";
                field.Caption = Utility.Phrase("Name");
            });
            //DATA AREA
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOutletMustVisit";
                field.Caption = Utility.Phrase("OutletMustVisit");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOutletVisited";
                field.Caption = Utility.Phrase("OutletVisited");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOrderCount";
                field.Caption = Utility.Phrase("OrderCount");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strTotalSKU";
                field.Caption = Utility.Phrase("TotalSKU");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strTotalQuantity";
                field.Caption = Utility.Phrase("TotalQuantity");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strTotalAmount";
                field.Caption = Utility.Phrase("TotalAmount");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strLPPC";
                field.Caption = Utility.Phrase("LPPC");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strSO_MCP";
                field.Caption = Utility.Phrase("SO_MCP");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strVisit_MCP";
                field.Caption = Utility.Phrase("Visit_MCP");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            return settings;
        }

        public static GridViewSettings ReportSalesmanSyncSettings()
        {
            var settings = new GridViewSettings
            {
                Name = "GridReportSalesmanSync",
                KeyFieldName = "ID",
                CallbackRouteValues = new { Controller = "DashBoard", Action = "GridReportSalesmanSync" },
                Width = Unit.Percentage(100)
            };
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = false;
            settings.Settings.ShowFilterRowMenu = false;
            settings.Settings.ShowGroupPanel = false;
            settings.Settings.ShowFooter = true;

            settings.Columns.Add(field =>
            {

                field.FieldName = "ID";
                field.Caption = Utility.Phrase("ID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "Name";
                field.Caption = Utility.Phrase("Name");
            });
            //DATA AREA
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOutletMustVisit";
                field.Caption = Utility.Phrase("CountSM");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOutletVisited";
                field.Caption = Utility.Phrase("CountFirstSyncTimeLate");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strOrderCount";
                field.Caption = Utility.Phrase("CountFirstStartTimeAMLate");
                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            return settings;
        }

        #endregion
        #endregion

        #region HeatMap
        [ActionAuthorize("DashBoard_TerritoryPartial")]
        [CompressFilter]
        public ActionResult HeatMap(FormCollection formParam)
        {
            return View();
        }

        [ActionAuthorize("DashBoard_TerritoryPartial")]
        [CompressFilter]
        public ActionResult TerritoryPartial(FormCollection formParam)
        {
            return PartialView();
        }

        //[ActionAuthorize("DashBoard_TerritoryPartial")]
        //[CompressFilter]
        public ActionResult GetOutletPolygon(string TerritoryID)
        {
            //var j = Global.Context.pp_GetOutletPolygon().ToList();
            var j = Global.Context.TerritoryPolygons.Where(a => a.Territory == TerritoryID).OrderBy(a => a.RenderOrder).ToList();
            var info = ControllerHelper.GetReportOrderIndexLevel().Where(a => a.Code == TerritoryID).FirstOrDefault();
            return Json(new
            {
                html = j,
                info = info
            }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult ReportOrderIndexLevel()
        {
            var model = (from sm in CacheDataHelper.CacheSalesAssessmentResult()//CacheDataHelper.CacheGetReportOrderIndexLevelResult()
                         join ut in ControllerHelper.ListRoute
                             //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                         on sm.RouteID equals ut.RouteID
                         where
                             sm.DistributorID == ut.DistributorID
                             && sm.RouteID == ut.RouteID
                         select sm).Distinct().ToList();


            string groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }

            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited, SUM(OrderCount) as OrderCount)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ReportSMVisitSummaryChartData()
                {
                    Code = item.ID,
                    Name = item.Name,
                    OutletMustVisit = item.OutletMustVisit,
                    OutletVisited = item.OutletVisited,
                    OrderCount = item.OrderCount,
                    SOMCP = item.OutletMustVisit > 0 ? ((item.OrderCount * 100) / item.OutletMustVisit) : 0,
                    VisitMCP = item.OutletMustVisit > 0 ? ((item.OutletVisited * 100) / item.OutletMustVisit) : 0,
                });
            }

            return Json(new
            {
                result
            }, JsonRequestBehavior.AllowGet);
            //return View();
        }

        public ActionResult ReportOrderIndexLevelTerritory()
        {
            List<pp_GetReportOrderIndexLevelResult> model = (from sm in CacheDataHelper.CacheGetReportOrderIndexLevelResult()
                                                             join ut in ControllerHelper.ListRoute
                                                                 //on new (sm.RouteID , sm.DistributorID) equals new (ut.RouteID, ut.DistributorID)
                                                             on sm.RouteID equals ut.RouteID
                                                             where
                                                                 sm.DistributorID == ut.DistributorID
                                                                 && sm.RouteID == ut.RouteID
                                                             select sm).Distinct().ToList();


            string groupby = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature("NSM"))
            {
                groupby = "Region";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                groupby = "Area";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                groupby = "Route";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                groupby = "Route";
            }

            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
            string js = string.Empty;
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ReportSMVisitSummaryChartData()
                {
                    Code = item.ID,
                    Name = item.Name,
                });

                js += "RenderOutletPolygon('" + item.ID + "');";
            }

            return JavaScript(js);
            //return View();
        }
        #endregion

        #region DashBoardNewLayOut
        [ActionAuthorize("DashBoard")]
        [CompressFilter]
        public ActionResult Home()
        {

            MV_DashBoard model = new MV_DashBoard();
            if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.Leader.ToString()))
            {
                model.isDashboard = false;

                model.ReviewListDetail = Global.VisibilityContext.usp_GetEvaluationUserBy("", User.Identity.Name).ToList();
                model.ReviewListHeader = Global.VisibilityContext.usp_GetOutletsImageReviewWithUserHeader(User.Identity.Name).ToList();
            }
            else if(PermissionHelper.CheckPermissionByFeature(Utility.RoleName.Auditor.ToString()))
            {
                model.isDashboard = false;
                model.EvalDefinitionResult = Global.VisibilityContext.usp_GetEvaluationDetailWithUser(User.Identity.Name).ToList();
                string username = SessionHelper.GetSession<string>("UserName");
                int ImgRejected = 0;
                var rejected = Global.VisibilityContext.DMSEvalWithUserRoles.Where(x => x.EvalUserID == username && x.TotalImageRejected.HasValue && x.TotalImageRejected.Value > 0).ToList();
                if(rejected != null)
                {
                    ImgRejected = rejected.Sum(x => x.TotalImageRejected.Value);
                    if(ImgRejected != 0)
                    {
                        ViewData["ImgRejected"] = ImgRejected;
                    }
                }
            } else if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.TradeMarketing.ToString()))
            {
                model.isDashboard = false;
                model.ReviewListHeader = Global.VisibilityContext.usp_GetOutletsImageReviewWithUserHeader("").ToList();
            }
            else
            {
                var listSalesAssessment = (from sm in CacheDataHelper.CacheSalesAssessmentResult()
                                                                            join ut in ControllerHelper.ListRoute
                                                                            on new { sm.RouteID, sm.DistributorID } equals new { ut.RouteID, ut.DistributorID }
                                                                            //on sm.RouteID equals ut.RouteID
                                                                            where
                                                                                sm.DistributorID == ut.DistributorID
                                                                                && sm.RouteID == ut.RouteID
                                                                            select sm).Distinct().ToList();

                model.TotalSM = listSalesAssessment.GroupBy(g => new { g.SalesmanID }).Select(s => s.First()).Distinct().Count();
                model.TotalVisitPlan = listSalesAssessment.Sum(s => s.OutletMustVisit);
                model.TotalSMHasVisit = listSalesAssessment.Where(x => x.OutletVisited > 0).GroupBy(g => new { g.SalesmanID }).Select(s => s.First()).Distinct().Count();
                model.TotalOutletHasVisit = listSalesAssessment.Sum(s => s.OutletVisited);
                model.TotalRestVisit = (model.TotalVisitPlan > 0 ? model.TotalVisitPlan - model.TotalOutletHasVisit : 0);
                
                model.TotalSMHasOrder = (from i in listSalesAssessment where i.HasOrder.Equals(1) select new {SalesmanID = i.SalesmanID }).Distinct().Count();
                model.TotalOrder = listSalesAssessment.Sum(s => s.OrderCount);
                model.TotalAmount = listSalesAssessment.Sum(s => s.TotalAmount);
                model.MTDTotalAmount = listSalesAssessment.Sum(s => s.MTDTotalAmount);
                model.TotalSMHasSync = listSalesAssessment.Where(x => x.FirstSyncTime.HasValue).GroupBy(g => new { g.SalesmanID }).Select(s => s.First()).Distinct().Count();

                var rowTimeSyncConfig = Global.Context.CustomSettings.Where(x => x.SettingCode == "SyncConfig" && x.SettingName == "SyncTimeConfig").FirstOrDefault();
                if (rowTimeSyncConfig != null)
                {
                    string strTime = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " + rowTimeSyncConfig.SettingValue;
                    DateTime timeSyncConfig = DateTime.Parse(strTime);
                    var listSMSyncLate = listSalesAssessment.Where(x => x.FirstSyncTime.HasValue).ToList();
                    model.TotalSMSyncLate = listSMSyncLate.Where(x => x.FirstSyncTime.Value.TimeOfDay > timeSyncConfig.TimeOfDay).GroupBy(g => g.SalesmanID).Select(s => s.First()).ToList().Count;
                    DateTime timeAddHour = new DateTime();
                    timeAddHour = timeSyncConfig;
                    timeAddHour.AddHours(1);
                    model.TotalSMSyncLateWithTime = listSMSyncLate.Where(x => x.FirstSyncTime.Value.TimeOfDay > timeSyncConfig.TimeOfDay && x.FirstSyncTime.Value.TimeOfDay <= timeAddHour.TimeOfDay).GroupBy(g => g.SalesmanID).Select(s => s.First()).ToList().Count;
                }

                #region ListSM
                model.ListSMHasSync = listSalesAssessment.Where(x => x.FirstSyncTime.HasValue).GroupBy(g => new { g.SalesmanID }).Select(s => s.First()).Distinct().ToList();
                model.ListSMNotSync = listSalesAssessment.Where(x=> !model.ListSMHasSync.Any(y=> y.SalesmanID == x.SalesmanID)).GroupBy(g => new { g.SalesmanID }).Select(s => s.First()).Distinct().ToList();
                model.ListSMHasVisited = listSalesAssessment.Where(x => x.OutletVisited > 0).GroupBy(g => new { g.SalesmanID }).Select(s => s.First()).Distinct().ToList();
                model.ListSMNotVisit = listSalesAssessment.Where(x => !model.ListSMHasVisited.Any(y => y.SalesmanID == x.SalesmanID) && model.ListSMHasSync.Any(y => y.SalesmanID == x.SalesmanID)).GroupBy(g => new { g.SalesmanID }).Select(s => s.First()).Distinct().ToList();
                #endregion

            }
            ViewBag.image = getrandomfile();
            return View(model);
        }
        private Random generator;
        private Random Generator
        {
            get
            {
                if (this.generator == null)
                {
                    this.generator = new Random();
                }
                return this.generator;
            }
        }
        private string getrandomfile()
        {
            string file = null;
            var extensions = new string[] { ".png", ".jpg", ".gif" };
            try
            {
                var di = new DirectoryInfo(Server.MapPath("~") + "/Content/Logo/");
                var rgFiles = di.GetFiles("*.*").Where(f => extensions.Contains(f.Extension.ToLower()));
                int fileCount = rgFiles.Count();
                if (fileCount > 0)
                {
                    int x = this.Generator.Next(0, fileCount);
                    file = rgFiles.ElementAt(x).Name;
                }
            }
            catch { }
            return file;
        }
        public ActionResult LoadChart()
        {
            MV_ChartDashBoard chartVisit = new MV_ChartDashBoard();
            DataTable chartTable = new DataTable();
            chartVisit.ChartDataRevenue = ControllerHelper.GetChartDataInMonthRevenue(ref chartTable);
            chartVisit.Tables = chartTable;
            chartVisit.ChartDataVisit = ControllerHelper.ChartCumulativeRevenue(ref chartTable);
            //chartVisit.ChartDataVisit = ControllerHelper.GetChartDataInMonthVisit();
            chartVisit.ChartDataOther = ControllerHelper.GetChartDataInMonthOther();
            return PartialView("~/Views/Shared/Control/ChartTablePartial.cshtml", chartVisit);
            
        }

        public ActionResult LoadChartBox()
        {
            List<MV_ChartBox> listChart = new List<MV_ChartBox>();
            List<Utility.ChartName> listChartDashboard = SessionHelper.GetSession<List<Utility.ChartName>>("listChartDashboard");
            foreach (Utility.ChartName elm in listChartDashboard)
            {
                switch (elm)
                {
                    case Utility.ChartName.RevenueDayOfSS:
                        {
                            MV_ChartBox chartRevenueDaySS = new MV_ChartBox();
                            chartRevenueDaySS.NameTab = "TabRevenueDay";
                            chartRevenueDaySS.TypeChart = Utility.ChartName.RevenueDayOfSS;
                            DataTable tableRevenueDaySS = new DataTable();
                            chartRevenueDaySS.Chart = ControllerHelper.ChartRevenueInDay(ref tableRevenueDaySS);
                            chartRevenueDaySS.Tables = tableRevenueDaySS;
                            listChart.Add(chartRevenueDaySS);
                            break;
                        }
                    case Utility.ChartName.RevenueDayOfASM_RSM:
                        {
                            MV_ChartBox chartRevenueInday = new MV_ChartBox();
                            chartRevenueInday.NameTab = "TabRevenueDay";
                            chartRevenueInday.TypeChart = Utility.ChartName.RevenueDayOfASM_RSM;
                            DataTable tableRevenueInday = new DataTable();
                            chartRevenueInday.Chart = ControllerHelper.ChartRevenueInDay(ref tableRevenueInday);
                            chartRevenueInday.Tables = tableRevenueInday;
                            listChart.Add(chartRevenueInday);
                            break;
                        }
                    case Utility.ChartName.RevenueDayPerOfSS:
                        {
                            MV_ChartBox chartRevenueDayPerSS = new MV_ChartBox();
                            chartRevenueDayPerSS.NameTab = "TabRevenueDayPer";
                            chartRevenueDayPerSS.TypeChart = Utility.ChartName.RevenueDayPerOfSS;
                            DataTable tableRevenueDayPerSS = new DataTable();
                            chartRevenueDayPerSS.Chart = ControllerHelper.ChartRevenueInDay(ref tableRevenueDayPerSS, "Percentage");
                            chartRevenueDayPerSS.Tables = tableRevenueDayPerSS;
                            listChart.Add(chartRevenueDayPerSS);
                            break;
                        }
                    case Utility.ChartName.RevenueDayPerOfASM_RSM:
                        {
                            MV_ChartBox chartRevenueDayPer = new MV_ChartBox();
                            chartRevenueDayPer.NameTab = "TabRevenueDayPer";
                            chartRevenueDayPer.TypeChart = Utility.ChartName.RevenueDayPerOfASM_RSM;
                            DataTable tableRevenueDayPer = new DataTable();
                            chartRevenueDayPer.Chart = ControllerHelper.ChartRevenueInDayNew(ref tableRevenueDayPer, "Percentage");
                            chartRevenueDayPer.Tables = tableRevenueDayPer;
                            listChart.Add(chartRevenueDayPer);
                            break;
                        }
                    case Utility.ChartName.VisitDayPerOfSS:
                        {
                            MV_ChartBox chartVisitDayPerSS = new MV_ChartBox();
                            chartVisitDayPerSS.NameTab = "TabVisitDayPer";
                            chartVisitDayPerSS.TypeChart = Utility.ChartName.VisitDayPerOfSS;
                            DataTable tableVisitDayPerSS = new DataTable();
                            chartVisitDayPerSS.Chart = ControllerHelper.ChartVisitInDay(ref tableVisitDayPerSS, "Percentage");
                            chartVisitDayPerSS.Tables = tableVisitDayPerSS;
                            listChart.Add(chartVisitDayPerSS);
                            break;
                        }
                    case Utility.ChartName.VisitDayPerOfASM_RSM:
                        {
                            MV_ChartBox chartVisitDayPer = new MV_ChartBox();
                            chartVisitDayPer.NameTab = "TabVisitDayPer";
                            chartVisitDayPer.TypeChart = Utility.ChartName.VisitDayPerOfASM_RSM;
                            DataTable tableVisitDayPer = new DataTable();
                            chartVisitDayPer.Chart = ControllerHelper.ChartVisitInDayNew(ref tableVisitDayPer, "Percentage");
                            chartVisitDayPer.Tables = tableVisitDayPer;
                            listChart.Add(chartVisitDayPer);
                            break;
                        }
                    case Utility.ChartName.VisitDayOfSS:
                        {
                            MV_ChartBox chartVisitDay = new MV_ChartBox();
                            chartVisitDay.NameTab = "TabVisitDay";
                            chartVisitDay.TypeChart = Utility.ChartName.VisitDayOfSS;
                            DataTable tableVisitDay = new DataTable();
                            chartVisitDay.Chart = ControllerHelper.ChartVisitInDay(ref tableVisitDay);
                            chartVisitDay.Tables = tableVisitDay;
                            listChart.Add(chartVisitDay);
                            break;
                        }
                    case Utility.ChartName.VisitDayOfASM_RSM:
                        {
                            MV_ChartBox chartVisitInDay = new MV_ChartBox();
                            chartVisitInDay.NameTab = "TabVisitDay";
                            chartVisitInDay.TypeChart = Utility.ChartName.VisitDayOfASM_RSM;
                            DataTable tableVisitInday = new DataTable();
                            chartVisitInDay.Chart = ControllerHelper.ChartVisitInDay(ref tableVisitInday);
                            chartVisitInDay.Tables = tableVisitInday;
                            listChart.Add(chartVisitInDay);
                            break;
                        }
                    case Utility.ChartName.VisitMonthOfASM_RSM:
                        {
                            MV_ChartBox chartVisitInDay = new MV_ChartBox();
                            chartVisitInDay.NameTab = "TabVisitMonth";
                            chartVisitInDay.TypeChart = Utility.ChartName.VisitMonthOfASM_RSM;
                            chartVisitInDay.Chart = ControllerHelper.GetChartDataInMonthVisit();
                            listChart.Add(chartVisitInDay);
                            break;
                        }
                    case Utility.ChartName.VisitMonthLine:
                        {
                            MV_ChartBox chartVisitInDay = new MV_ChartBox();
                            chartVisitInDay.NameTab = "TabVisitMonthLine";
                            chartVisitInDay.TypeChart = Utility.ChartName.VisitMonthOfASM_RSM;
                            chartVisitInDay.Chart = ControllerHelper.GetChartDataInMonthVisit();
                            listChart.Add(chartVisitInDay);
                            break;
                        }
                    case Utility.ChartName.SyncDaySS:
                        {
                            MV_ChartBox chartSyncInDay = new MV_ChartBox();
                            chartSyncInDay.NameTab = "TabSyncDay";
                            chartSyncInDay.TypeChart = Utility.ChartName.SyncDaySS;
                            DataTable tableSyncInday = new DataTable();
                            chartSyncInDay.Chart = ControllerHelper.ChartSyncInDay(ref tableSyncInday);
                            chartSyncInDay.Tables = tableSyncInday;
                            listChart.Add(chartSyncInDay);
                            break;
                        }
                    case Utility.ChartName.SyncDayASM_RSM:
                        {
                            MV_ChartBox chartSyncInDay = new MV_ChartBox();
                            chartSyncInDay.NameTab = "TabSyncDay";
                            chartSyncInDay.TypeChart = Utility.ChartName.SyncDayASM_RSM;
                            DataTable tableSyncInday = new DataTable();
                            chartSyncInDay.Chart = ControllerHelper.ChartSyncInDay(ref tableSyncInday);
                            chartSyncInDay.Tables = tableSyncInday;
                            listChart.Add(chartSyncInDay);
                            break;
                        }
                    case Utility.ChartName.RevenueInMonth:
                        {
                            MV_ChartBox chartRevenueInMonth = new MV_ChartBox();
                            chartRevenueInMonth.NameTab = "TabCumulativeRevenueInMonth";
                            chartRevenueInMonth.TypeChart = Utility.ChartName.RevenueInMonth;
                            DataTable tableRevenueInMonth = new DataTable();
                            chartRevenueInMonth.Chart = ControllerHelper.ChartCumulativeRevenue(ref tableRevenueInMonth);
                            chartRevenueInMonth.Tables = tableRevenueInMonth;
                            listChart.Add(chartRevenueInMonth);
                            break;
                        }
                    case Utility.ChartName.RevenueInMonthOfBOD:
                        {
                            MV_ChartBox chartRevenueInMonth = new MV_ChartBox();
                            chartRevenueInMonth.NameTab = "TabRevenueInMonth";
                            chartRevenueInMonth.TypeChart = Utility.ChartName.RevenueInMonthOfBOD;
                            DataTable tableRevenueInMonth = new DataTable();
                            chartRevenueInMonth.Chart = ControllerHelper.GetChartDataInMonthRevenue(ref tableRevenueInMonth);
                            chartRevenueInMonth.Tables = tableRevenueInMonth;
                            listChart.Add(chartRevenueInMonth);
                            break;
                        }
                    case Utility.ChartName.OtherInMonth:
                        {
                            MV_ChartBox chartRevenueInMonth = new MV_ChartBox();
                            chartRevenueInMonth.NameTab = "TabOther";
                            chartRevenueInMonth.TypeChart = Utility.ChartName.OtherInMonth;
                            DataTable tableRevenueInMonth = new DataTable();
                            chartRevenueInMonth.Chart = ControllerHelper.GetChartDataInMonthOther();
                            chartRevenueInMonth.Tables = tableRevenueInMonth;
                            listChart.Add(chartRevenueInMonth);
                            break;
                        }
                }
            }
            return PartialView("~/Views/Shared/Control/ChartBoxPartial.cshtml", listChart);
        }

        public ActionResult LoadPieVisit()
        {
            ViewData["NameID"] = "StatisVisit";
            MV_PieTable statisticVisit = new MV_PieTable();
            DataTable visitTable = new DataTable("VisitTable");
            statisticVisit.PieData = ControllerHelper.GetReportVisitLevel(ref visitTable);
            statisticVisit.Tables = visitTable;
            return PartialView("~/Views/Shared/Control/PieTablePartial.cshtml", statisticVisit);
        }
        public ActionResult LoadPieOrder()
        {
            ViewData["NameID"] = "StatisOrder";
            MV_PieTable statisticOrder = new MV_PieTable();
            DataTable orderTable = new DataTable("OrderTable");
            statisticOrder.PieData = ControllerHelper.GetReportOrderLevel(ref orderTable);
            statisticOrder.Tables = orderTable;
            return PartialView("~/Views/Shared/Control/PieTablePartial.cshtml", statisticOrder);
            
        }
        #endregion


        #region Config Dashboard
        public void SaveConfig(string role)
        {

        }
        #endregion


        #region Chart item
        
        #endregion

        public ActionResult TestChartQlik()
        {
            return View();
        }
    }
}
