using DevExpress.Web;
using DevExpress.Web.Mvc;
using DMSERoute.Helpers;
using eRoute.Models;
using eRoute.Models.ViewModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace eRoute.Controllers
{
    [Authorize]
    [LogAndRedirectOnError]
    public class ReportTrackingController : Controller
    {
        //
        // GET: /ReportTracking/

        public ActionResult Index()
        {
            return View();
        }

        #region Report Visit
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportVisit", true)]
        public ActionResult ReportVisit()//
        {
            MV_ReportVisit model = new MV_ReportVisit();
            SetDataCombobox(model);
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Tracking_ReportVisit")]
        //[CompressFilter]
        public ActionResult ReportVisit(string strFromDate, string Region, string Area, string Distributor, string Route, string act, string DisplayID)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            if (act != "ViewGallery")
            {
                #region Validate and Get Data
                try
                {
                    if (!string.IsNullOrEmpty(strFromDate))
                    {
                        model.FromDate = Utility.DateTimeParse(strFromDate);
                        model.areaID = Utility.StringParse(Area);
                        model.distributorID = Utility.IntParse(Distributor);
                        model.regionID = Utility.StringParse(Region);
                        model.routeID = Utility.StringParse(Route);

                        List<ObjParamSP> listParam = new List<ObjParamSP>();
                        listParam.Add(new ObjParamSP() { Key = "VisitDate", Value = model.FromDate });
                        listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.regionID });
                        listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.areaID });
                        listParam.Add(new ObjParamSP() { Key = "ProvinceID", Value = model.provinceID });
                        listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.distributorID });
                        listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.saleSupID });
                        listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.routeID });
                        listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.salesmanID });
                        listParam.Add(new ObjParamSP() { Key = "EmployeeID", Value = model.employeeID });
                        listParam.Add(new ObjParamSP() { Key = "LeaderID", Value = model.leaderID });
                        listParam.Add(new ObjParamSP() { Key = "UserName", Value = SessionHelper.GetSession<string>("UserName") });
                        model.ReportVisitResult = ControllerHelper.QueryStoredProcedure("pp_ReportVisit", listParam);
                        //model.ReportVisitResult = Global.Context.pp_ReportVisit(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, model.employeeID, model.leaderID, SessionHelper.GetSession<string>("UserName")).ToList();

                        model.ReportVisitSummary = Global.Context.pp_GetBLSalesKPI(model.FromDate, model.regionID, model.areaID, model.distributorID, model.saleSupID, model.routeID).ToList();
                        model.ReportImageProgram = Global.Context.pp_ReportImageProgram(model.FromDate, model.regionID, model.areaID, model.distributorID, model.routeID, model.salesmanID, SessionHelper.GetSession<string>("UserName")).ToList();
                        SessionHelper.SetSession<MV_ReportVisit>("MV_ReportVisit", model);
                    }
                }
                catch (Exception ex)
                {
                    CustomLog.LogError(ex);
                }
                #endregion
                SetDataCombobox(model);
                return View(model);
            }
            else
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
                ReportVisitGallery rs = new ReportVisitGallery();
                if (SessionHelper.GetSession<ReportVisitGallery>("ReportVisitGallery") != null)
                {
                    rs = SessionHelper.GetSession<ReportVisitGallery>("ReportVisitGallery");
                }
                
                if (DisplayID == null)
                {
                    DisplayID = "";
                }
               if (!string.IsNullOrEmpty(Route))
                {
                    rs.ListHeader = Global.VisibilityContext.usp_GetListImageGalleryHeader(model.routeID, model.FromDate).ToList();
                }
                rs.DisplayData = Global.VisibilityContext.uvw_GetDisplayInformations.ToList();
                List<usp_GetListImageGalleryResult> imagelist = new List<usp_GetListImageGalleryResult>();
                rs.NameSceen = Utility.Phrase("ReportVisitGallery");
                imagelist = Global.VisibilityContext.usp_GetListImageGallery(DisplayID, Route, Utility.DateTimeParse(strFromDate)).ToList();
                rs.ListImageBy = imagelist;
                 if (DisplayID != "")
                     rs.Display = Global.VisibilityContext.uvw_GetDisplayInformations.ToList().Where(x=> x.MaCTTB == DisplayID).FirstOrDefault().MoTa;
                
                Session["ListImageByCriteria"] = model;

                return View("ReportVisitDetailListImage", rs);
                //return View(model);
            }
        }

        [ActionAuthorize("Tracking_ReportVisitExportExecl")]
        public ActionResult ReportVisitExportExecl(string strFromDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            List<ObjParamSP> listParam = new List<ObjParamSP>();
            listParam.Add(new ObjParamSP() { Key = "VisitDate", Value = model.FromDate });
            listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.regionID });
            listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.areaID });
            listParam.Add(new ObjParamSP() { Key = "ProvinceID", Value = model.provinceID });
            listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.distributorID });
            listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.saleSupID });
            listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.routeID });
            listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.salesmanID });
            listParam.Add(new ObjParamSP() { Key = "EmployeeID", Value = model.employeeID });
            listParam.Add(new ObjParamSP() { Key = "LeaderID", Value = model.leaderID });
            listParam.Add(new ObjParamSP() { Key = "UserName", Value = SessionHelper.GetSession<string>("UserName") });
            model.ReportVisitResult = ControllerHelper.QueryStoredProcedure("pp_ReportVisit", listParam);

            //model.ReportVisitResult = Global.Context.pp_ReportVisit(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, model.employeeID, model.leaderID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportVisitExportExecl", null);

            return GridViewExtension.ExportToXlsx(ReportVisitSettingsRAWData(), model.ReportVisitResult);
        }

        [ActionAuthorize("Tracking_ReportVisitExportPDF")]
        public ActionResult ReportVisitExportPDF(string strFromDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            List<ObjParamSP> listParam = new List<ObjParamSP>();
            listParam.Add(new ObjParamSP() { Key = "VisitDate", Value = model.FromDate });
            listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.regionID });
            listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.areaID });
            listParam.Add(new ObjParamSP() { Key = "ProvinceID", Value = model.provinceID });
            listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.distributorID });
            listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.saleSupID });
            listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.routeID });
            listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.salesmanID });
            listParam.Add(new ObjParamSP() { Key = "EmployeeID", Value = model.employeeID });
            listParam.Add(new ObjParamSP() { Key = "LeaderID", Value = model.leaderID });
            listParam.Add(new ObjParamSP() { Key = "UserName", Value = SessionHelper.GetSession<string>("UserName") });
            model.ReportVisitResult = ControllerHelper.QueryStoredProcedure("pp_ReportVisit", listParam);

            //model.ReportVisitResult = Global.Context.pp_ReportVisit(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, model.employeeID, model.leaderID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportVisitExportPDF", null);

            return GridViewExtension.ExportToPdf(ReportVisitSettingsRAWData(), model.ReportVisitResult);
        }

        private static GridViewSettings ReportVisitSettingsRAWData()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportVisit";
            settings.KeyFieldName = "RouteID";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };
            settings.Columns.Add(field =>
            {

                field.FieldName = "RegionID";
                field.Caption = Utility.Phrase("RegionID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("RegionName");
            });
            //settings.Columns.Add(field =>
            //{

            //    field.FieldName = "RSMID";
            //    field.Caption = Utility.Phrase("RSMID");
            //});
            //settings.Columns.Add(field =>
            //{

            //    field.FieldName = "RSMName";
            //    field.Caption = Utility.Phrase("RSMName");
            //});
            settings.Columns.Add(field =>
            {

                field.FieldName = "AreaID";
                field.Caption = Utility.Phrase("AreaID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("AreaName");
            });
            //settings.Columns.Add(field =>
            //{

            //    field.FieldName = "ASMID";
            //    field.Caption = Utility.Phrase("ASMID");
            //});
            //settings.Columns.Add(field =>
            //{

            //    field.FieldName = "ASMName";
            //    field.Caption = Utility.Phrase("ASMName");
            //});
            //settings.Columns.Add(field =>
            //{

            //    field.FieldName = "ProvinceID";
            //    field.Caption = Utility.Phrase("ProvinceID");
            //});
            //settings.Columns.Add(field =>
            //{

            //    field.FieldName = "ProvinceName";
            //    field.Caption = Utility.Phrase("ProvinceName");
            //});
            settings.Columns.Add(field =>
            {

                field.FieldName = "DistributorCode";
                field.Caption = Utility.Phrase("DistributorCode");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("DistributorName");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "SaleSupID";
                field.Caption = Utility.Phrase("SaleSupID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("SaleSupName");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("RouteID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("RouteName");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("SalesmanID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("SalesmanName");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "VisitDate";
                field.Caption = Utility.Phrase("VisitDate");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strIsMCP";
                field.Caption = Utility.Phrase("strIsMCP");

            });
            //DATA AREA
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitOrder";
                field.Caption = Utility.Phrase("VisitOrder");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitOrderReal";
                field.Caption = Utility.Phrase("VisitOrderReal");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletID";
                field.Caption = Utility.Phrase("OutletID");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletName";
                field.Caption = Utility.Phrase("OutletName");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Address";
                field.Caption = Utility.Phrase("Address");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Mobile";
                field.Caption = Utility.Phrase("Mobile");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "StartTime";
                field.Caption = Utility.Phrase("StartTime");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "EndTime";
                field.Caption = Utility.Phrase("EndTime");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strTimeSpanVisit";
                field.Caption = Utility.Phrase("strTimeSpanVisit");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strTimeSpanMove";
                field.Caption = Utility.Phrase("strTimeSpanMove");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Distance";
                field.Caption = Utility.Phrase("Distance");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Lat";
                field.Caption = Utility.Phrase("Lat");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Long";
                field.Caption = Utility.Phrase("Long");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "HasOrder";
                field.Caption = Utility.Phrase("HasOrder");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalSKU";
                field.Caption = Utility.Phrase("TotalSKU");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DropSize";
                field.Caption = Utility.Phrase("DropSize");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalAmount";
                field.Caption = Utility.Phrase("TotalAmount");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Reason";
                field.Caption = Utility.Phrase("Reason");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "GPSType";
                field.Caption = Utility.Phrase("GPSType");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "IsEnableAirPlaneMode";
                field.Caption = Utility.Phrase("IsEnableAirPlaneMode");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "IsEnableGPSMode";
                field.Caption = Utility.Phrase("IsEnableGPSMode");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "IsEnableNetworkMode";
                field.Caption = Utility.Phrase("IsEnableNetworkMode");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
            });
            return settings;
        }

        public ActionResult ReloadTabVisit()
        {
            DataTable model = SessionHelper.GetSession<MV_ReportVisit>("MV_ReportVisit").ReportVisitResult;
            return PartialView("ReloadTabVisit", model);
        }

        public ActionResult ReloadTabOrderStatus()
        {
            DataTable model = SessionHelper.GetSession<MV_ReportVisit>("MV_ReportVisit").ReportVisitResult;
            return PartialView("ReloadTabOrderStatus", model);
        }

        public ActionResult ReloadTabImageDisplay()
        {
            DataTable model = SessionHelper.GetSession<MV_ReportVisit>("MV_ReportVisit").ReportVisitResult;
            return PartialView("ReloadTabImageDisplay", model);
        }
        //      
        #endregion

        #region Report Summary Sales
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportSummarySales", true)]
        public ActionResult ReportSummarySales()//
        {
            MV_ReportSummarySales model = new MV_ReportSummarySales();
            SetDataComboboxSummarySales(model);
            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Tracking_ReportSummarySales")]
        public ActionResult ReportSummarySales(string strFromDate, string strToDate, string Region, string Area, string Distributor,string Route)
        {
            MV_ReportSummarySales model = new MV_ReportSummarySales();
            #region Validate and Get Data
            try
            {
                int ConfigDateSum = Utility.IntParse(ControllerHelper.valueCustomSetting("ConfigDateSum"));

                DateTime fromDate = Utility.DateTimeParse(strFromDate);
                DateTime toDate = Utility.DateTimeParse(strToDate);
                DateTime dateEnd = (fromDate).AddDays(ConfigDateSum);

                if (toDate > dateEnd)
                {
                    ViewBag.StatusMessage = Utility.Phrase("DateNotThan")+ " " + ConfigDateSum + " " + Utility.Phrase("Date");
                }
                else
                {
                    if (!string.IsNullOrEmpty(strFromDate))
                    {
                        model.FromDate = Utility.DateTimeParse(strFromDate);
                        model.ToDate = Utility.DateTimeParse(strToDate);
                        model.areaID = Utility.StringParse(Area);
                        model.distributorID = Utility.IntParse(Distributor);
                        model.regionID = Utility.StringParse(Region);
                        model.routeID = Utility.StringParse(Route);

                        model.ReportSummarySalesResult = Global.Context.pp_ReportSummarySales(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID, model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();
                        model.ReportSummarySalesDetailResult = Global.Context.pp_ReportSummarySalesDetail(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID, model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();
                        SessionHelper.SetSession<MV_ReportSummarySales>("MV_ReportSummarySales", model);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            SetDataComboboxSummarySales(model);
            return View(model);
        }

        [ActionAuthorize("Tracking_ReportSummarySalesExportExecl")]
        public ActionResult ReportSummarySalesExportExecl(string strFromDate, string strToDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportSummarySales model = new MV_ReportSummarySales();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSummarySalesResult = Global.Context.pp_ReportSummarySales(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID,model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportSummarySalesExportExecl", null);
            
            return GridViewExtension.ExportToXlsx(ReportSummaruSalesSettingExport(), model.ReportSummarySalesResult);
        }

        [ActionAuthorize("Tracking_ReportSummarySalesDetailExportExecl")]
        public ActionResult ReportSummarySalesDetailExportExecl(string strFromDate, string strToDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportSummarySales model = new MV_ReportSummarySales();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSummarySalesDetailResult = Global.Context.pp_ReportSummarySalesDetail(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID, model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportSummarySalesDetailExportExecl", null);

            return GridViewExtension.ExportToXlsx(ReportSummaruSalesDetailSettingExport(), model.ReportSummarySalesDetailResult);
        }

        [ActionAuthorize("Tracking_ReportSummarySalesExportPDF")]
        public ActionResult ReportSummarySalesExportPDF(string strFromDate,string strToDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportSummarySales model = new MV_ReportSummarySales();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSummarySalesResult = Global.Context.pp_ReportSummarySales(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID,model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportSummarySalesExportPDF", null);

            return GridViewExtension.ExportToPdf(ReportSummaruSalesSettingExport(), model.ReportSummarySalesResult);

        }

        [ActionAuthorize("Tracking_ReportSummarySalesDetailExportPDF")]
        public ActionResult ReportSummarySalesDetailExportPDF(string strFromDate,string strToDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportSummarySales model = new MV_ReportSummarySales();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSummarySalesDetailResult = Global.Context.pp_ReportSummarySalesDetail(model.FromDate,model.ToDate, model.regionID, model.areaID, model.distributorID, model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportSummarySalesDetailExportPDF", null);

            return GridViewExtension.ExportToPdf(ReportSummaruSalesDetailSettingExport(), model.ReportSummarySalesDetailResult);

        }


        private static GridViewSettings ReportSummaruSalesSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportSummarySales";
            settings.KeyFieldName = "VisitDate";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionID";
                field.Caption = Utility.Phrase("Report_RegionID");
                
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaID";
                field.Caption = Utility.Phrase("Report_AreaID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorCode";
                field.Caption = Utility.Phrase("Report_DistributorID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("Report_Distributor");
            });
          
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupID";
                field.Caption = Utility.Phrase("TB_GSBHID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("TB_GSBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("Report_RouteID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("Report_Route");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("TB_NVBHID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderDate";
                field.Caption = Utility.Phrase("TB_DateWork");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletID";
                field.Caption = Utility.Phrase("TB_OutletID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletName";
                field.Caption = Utility.Phrase("TB_Outlet");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderNbr";
                field.Caption = Utility.Phrase("TB_OrderNbr");
            });
            
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalQty";
                field.Caption = Utility.Phrase("TB_TotalQuantity");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalAmt";
                field.Caption = Utility.Phrase("TB_Amount");
            });
            return settings;
        }

        private static GridViewSettings ReportSummaruSalesDetailSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportSummarySalesDetail";
            settings.KeyFieldName = "VisitDate";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("TB_NVBHID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderDate";
                field.Caption = Utility.Phrase("TB_DateWork");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletID";
                field.Caption = Utility.Phrase("TB_OutletID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletName";
                field.Caption = Utility.Phrase("TB_Outlet");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderNbr";
                field.Caption = Utility.Phrase("TB_OrderNbr");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "InventoryCD";
                field.Caption = Utility.Phrase("TB_InventotyCD");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "InventoryName";
                field.Caption = Utility.Phrase("TB_InventotyName");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderQty";   
                field.Caption = Utility.Phrase("TB_TotalQuantity");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleUnit";
                field.Caption = Utility.Phrase("TB_SaleUnit");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "UnitPrice";
                field.Caption = Utility.Phrase("TB_UnitPrice");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Amount";
                field.Caption = Utility.Phrase("TB_Amount");
            });
            return settings;
        }

        #endregion

        #region Report synchronous
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportSynchronous", true)]
        public ActionResult ReportSynchronous()//
        {
            MV_ReportVisit model = new MV_ReportVisit();
            SetDataCombobox(model);
            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Tracking_ReportSynchronous")]
        public ActionResult ReportSynchronous(string strFromDate,string strToDate, string Region, string Area, string Distributor, string strTimeRegular)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                if (!string.IsNullOrEmpty(strFromDate))
                {
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.ToDate = Utility.DateTimeParse(strToDate);
                    model.areaID = Utility.StringParse(Area);
                    model.distributorID = Utility.IntParse(Distributor);
                    model.regionID = Utility.StringParse(Region);

                    strTimeRegular = strTimeRegular ?? "08:00";
                    model.TimeRegular = TimeSpan.Parse(strTimeRegular);

                    model.ReportSyscResult = Global.Context.pp_ReportSyschronous(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID, SessionHelper.GetSession<string>("UserName"), model.TimeRegular).ToList();
                    SessionHelper.SetSession<MV_ReportVisit>("MV_ReportVisit", model);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            SetDataCombobox(model);
            return View(model);
        }

        private static GridViewSettings ReportSyncSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportSyschronous";
            settings.KeyFieldName = "VisitDate";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitDate";
                field.Caption = Utility.Phrase("TB_DateWork");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorCode";
                field.Caption = Utility.Phrase("Report_DistributorID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("Report_Distributor");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("Report_Route");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("TB_GSBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });
            
            settings.Columns.Add(field =>
            {
                field.FieldName = "LongtitudeBase";
                field.Caption = Utility.Phrase("TB_LongtitudeBase");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LatitudeBase";
                field.Caption = Utility.Phrase("TB_LatitudeBase");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LongtitudeSync";
                field.Caption = Utility.Phrase("TB_LongtitudeSync");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LatitudeSync";
                field.Caption = Utility.Phrase("TB_LatitudeSync");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistanceWithBase";
                field.Caption = Utility.Phrase("TB_DistanceWithBase");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ValidDistance";
                field.Caption = Utility.Phrase("TB_ValidDistance");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SystemSyncDate";
                field.Caption = Utility.Phrase("TB_SystemSyncDate");
                field.PropertiesEdit.DisplayFormatString = "M/d/yyyy HH:mm:ss";
            });

            MVCxGridViewColumn column = settings.Columns.Add("SyncTypeTemp");
            column.UnboundType = DevExpress.Data.UnboundColumnType.String;
            column.PropertiesEdit.DisplayFormatString = "c";
            column.Caption = Utility.Phrase("TB_StatusSync");
            MVCxGridViewColumn Assess = settings.Columns.Add("Assess");
            Assess.UnboundType = DevExpress.Data.UnboundColumnType.String;
            Assess.PropertiesEdit.DisplayFormatString = "c";
            Assess.Caption = Utility.Phrase("TB_Assess");
            MVCxGridViewColumn TimeDisparity = settings.Columns.Add("TimeDisparityTmp");
            TimeDisparity.UnboundType = DevExpress.Data.UnboundColumnType.String;
            TimeDisparity.PropertiesEdit.DisplayFormatString = "c";
            TimeDisparity.Caption = Utility.Phrase("TB_TimeDisparity");
            settings.CustomUnboundColumnData = (sender, e) =>
            {
                if (e.Column.FieldName == "SyncTypeTemp")
                {
                    string SyncType = (string)e.GetListSourceFieldValue("SyncType");
                    if (SyncType == "D")
                    {
                        e.Value = Utility.Phrase("Value_Yes");
                    }
                    else
                    {
                        e.Value = Utility.Phrase("");
                    }
                }
                else if (e.Column.FieldName == "Assess")
                {
                    string SyncAssess = (string)e.GetListSourceFieldValue("SyncType");
                    int IsTimeValid = (int)e.GetListSourceFieldValue("IsTimeValid");
                    if (SyncAssess == "D")
                    {
                        if (IsTimeValid == 1) { e.Value = Utility.Phrase("OnTime"); }
                        else { e.Value = Utility.Phrase("Late"); }
                    }
                }
                if (e.Column.FieldName == "TimeDisparityTmp")
                {
                    string SyncAssess = (string)e.GetListSourceFieldValue("SyncType");
                    int IsTimeValid = (int)e.GetListSourceFieldValue("IsTimeValid");
                    if(SyncAssess != "D" || IsTimeValid != 1)
                    {
                        TimeSpan Time = (TimeSpan)e.GetListSourceFieldValue("TimeDisparity");
                        e.Value = string.Format("{0:hh\\:mm\\:ss}", Time);
                    }  
                }
            };
            return settings;
        }

        [ActionAuthorize("Tracking_ReportSyncExportExecl")]
        public ActionResult ReportSyncExportExecl(string strFromDate, string strToDate, string Region, string Area, string Distributor, string strTimeRegular)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.TimeRegular = TimeSpan.Parse(strTimeRegular);
                
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSyscResult = Global.Context.pp_ReportSyschronous(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID, SessionHelper.GetSession<string>("UserName"), model.TimeRegular).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportSyncExportExecl", null);

            return GridViewExtension.ExportToXlsx(ReportSyncSettingExport(), model.ReportSyscResult);
        }

        [ActionAuthorize("Tracking_ReportSyncExportPDF")]
        public ActionResult ReportSyncExportPDF(string strFromDate, string strToDate, string Region, string Area, string Distributor, string Route, string strTimeRegular)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
                model.TimeRegular = TimeSpan.Parse(strTimeRegular);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSyscResult = Global.Context.pp_ReportSyschronous(model.FromDate, model.ToDate, model.regionID, model.areaID, model.distributorID, SessionHelper.GetSession<string>("UserName"), model.TimeRegular).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportSyncExportPDF", null);

            return GridViewExtension.ExportToPdf(ReportSyncSettingExport(), model.ReportSyscResult);
        }

        #endregion

        #region ReportReviewWorkWith
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportReviewWorkWith", true)]
        public ActionResult ReportReviewWorkWith()
        {
            MV_ReportVisit model = new MV_ReportVisit();
            SetDataCombobox(model);
            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Tracking_ReportReviewWorkWith")]
        public ActionResult ReportReviewWorkWith(string strFromDate, string strRegion, string strArea, string strDistributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                if (!string.IsNullOrEmpty(strFromDate))
                {
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.areaID = Utility.StringParse(strArea);
                    model.distributorID = Utility.IntParse(strDistributor);
                    model.regionID = Utility.StringParse(strRegion);

                    model.ReportReviewWorkWithResult = Global.Context.pp_ReportReviewWorkWith(model.FromDate, DateTime.Now, model.regionID, model.areaID, model.distributorID, "", "", SessionHelper.GetSession<string>("UserName")).ToList();
                    SessionHelper.SetSession<MV_ReportVisit>("MV_ReportVisit", model);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            SetDataCombobox(model);
            return View(model);
        }
        private static GridViewSettings ReportReviewWorkWithSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportReviewWorkWith";
            settings.KeyFieldName = "VisitDate";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "WWID";
                field.Caption = Utility.Phrase("ASM/SS");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "WWName";
                field.Caption = Utility.Phrase("Name_ASM/SS");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "WWTitle";
                field.Caption = Utility.Phrase("Titile_ASM/SS");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("RouteID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("RouteName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesManID";
                field.Caption = Utility.Phrase("SalesManID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesManName";
                field.Caption = Utility.Phrase("SalesManName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitDate";
                field.Caption = Utility.Phrase("VisitDate");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SMWorkAM";
                field.Caption = Utility.Phrase("SMWorkAM");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SMWorkPM";
                field.Caption = Utility.Phrase("SMWorkPM");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderSuccessAM";
                field.Caption = Utility.Phrase("OrderSuccessAM");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderSuccessPM";
                field.Caption = Utility.Phrase("OrderSuccessPM");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "GPSInvalid";
                field.Caption = Utility.Phrase("GPSInvalid");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "APMInvalid";
                field.Caption = Utility.Phrase("APMInvalid");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitValidAM";
                field.Caption = Utility.Phrase("VisitValidAM");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitValidPM";
                field.Caption = Utility.Phrase("VisitValidPM");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitValid";
                field.Caption = Utility.Phrase("VisitValid");
            });
            return settings;
        }

        [ActionAuthorize("Tracking_ReportReviewWorkWithExportExecl")]
        public ActionResult ReportReviewWorkWithExportExecl(string strFromDate, string strRegion, string strArea, string strDistributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(strArea);
                model.distributorID = Utility.IntParse(strDistributor);
                model.regionID = Utility.StringParse(strRegion);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportReviewWorkWithResult = Global.Context.pp_ReportReviewWorkWith(model.FromDate, DateTime.Now, model.regionID, model.areaID, model.distributorID, "", "", SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportReviewWorkWithExportExecl", null);

            return GridViewExtension.ExportToXlsx(ReportReviewWorkWithSettingExport(), model.ReportReviewWorkWithResult);
        }

        [ActionAuthorize("Tracking_ReportReviewWorkWithExportPDF")]
        public ActionResult ReportReviewWorkWithExportPDF(string strFromDate, string strRegion, string strArea, string strDistributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(strArea);
                model.distributorID = Utility.IntParse(strDistributor);
                model.regionID = Utility.StringParse(strRegion);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportReviewWorkWithResult = Global.Context.pp_ReportReviewWorkWith(model.FromDate, DateTime.Now, model.regionID, model.areaID, model.distributorID, "", "", SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportReviewWorkWithExportExecl", null);

            return GridViewExtension.ExportToPdf(ReportReviewWorkWithSettingExport(), model.ReportReviewWorkWithResult);
        }
        #endregion

        #region ReportWorkWith
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportWorkWith", true)]
        public ActionResult ReportWorkWith()
        {
            MV_ReportVisit model = new MV_ReportVisit();
            SetDataComboboxWorkWith(model);
            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Tracking_ReportWorkWith")]
        public ActionResult ReportWorkWith(string strFromDate, string strRegion, string strArea, string strSaleSup)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                if (!string.IsNullOrEmpty(strFromDate))
                {
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.areaID = Utility.StringParse(strArea);
                    model.distributorID = Utility.IntParse(0);
                    model.regionID = Utility.StringParse(strRegion);
                    model.saleSupID = Utility.StringParse(strSaleSup);
                    model.ReportWorkWithResult = Global.Context.pp_ReportWorkWith(model.FromDate, model.regionID, model.areaID, model.distributorID, model.saleSupID, "", SessionHelper.GetSession<string>("UserName"), 3).ToList();
                    SessionHelper.SetSession<MV_ReportVisit>("MV_ReportVisit", model);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            SetDataComboboxWorkWith(model);
            return View(model);
        }
        private static GridViewSettings ReportWorkWithSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportWorkWith";
            // settings.KeyFieldName = "VisitDate";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorCode";
                field.Caption = Utility.Phrase("DistributorCode");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("DistributorName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("RouteID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("RouteName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesManID";
                field.Caption = Utility.Phrase("SalesManID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesManName";
                field.Caption = Utility.Phrase("SalesManName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("SaleSupName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ASM";
                field.Caption = Utility.Phrase("ASM");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletID";
                field.Caption = Utility.Phrase("OutletID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletName";
                field.Caption = Utility.Phrase("OutletName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Address";
                field.Caption = Utility.Phrase("Address");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "HasOrder";
                field.Caption = Utility.Phrase("HasOrder");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitTimeSM";
                field.Caption = Utility.Phrase("SMTime");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitTimeSS";
                field.Caption = Utility.Phrase("SUPTime");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitTimeASM";
                field.Caption = Utility.Phrase("ASMTime");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistanceSM";
                field.Caption = Utility.Phrase("DistanceSM");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "DistanceSS";
                field.Caption = Utility.Phrase("SUPDistance");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistanceASM";
                field.Caption = Utility.Phrase("ASMDistance");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "ConfirmTimeSS";
                field.Caption = Utility.Phrase("ConfirmTimeSS");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ConfirmTimeASM";
                field.Caption = Utility.Phrase("ConfirmTimeASM");
            });
            return settings;
        }

        [ActionAuthorize("Tracking_ReportWorkWithExportExecl")]
        public ActionResult ReportWorkWithExportExecl(string strFromDate, string strRegion, string strArea, string strSaleSup)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(strArea);
                model.distributorID = Utility.IntParse(0);
                model.regionID = Utility.StringParse(strRegion);
                model.saleSupID = Utility.StringParse(strSaleSup);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportWorkWithResult = Global.Context.pp_ReportWorkWith(model.FromDate, model.regionID, model.areaID, model.distributorID, model.saleSupID, "", SessionHelper.GetSession<string>("UserName"), 3).ToList();
            ControllerHelper.LogUserAction("ReportTracking", "ReportWorkWithExportExecl", null);

            return GridViewExtension.ExportToXlsx(ReportWorkWithSettingExport(), model.ReportWorkWithResult);
        }

        [ActionAuthorize("Tracking_ReportWorkWithExportPDF")]
        public ActionResult ReportWorkWithExportPDF(string strFromDate, string strRegion, string strArea, string strSaleSup)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(strArea);
                model.distributorID = Utility.IntParse(0);
                model.regionID = Utility.StringParse(strRegion);
                model.saleSupID = Utility.StringParse(strSaleSup);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportWorkWithResult = Global.Context.pp_ReportWorkWith(model.FromDate, model.regionID, model.areaID, model.distributorID, model.saleSupID, "", SessionHelper.GetSession<string>("UserName"), 3).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportWorkWithExportExecl", null);

            return GridViewExtension.ExportToPdf(ReportWorkWithSettingExport(), model.ReportWorkWithResult);
        }
        #endregion

        #region ReportSaleDaily
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportSaleDaily", true)]
        public ActionResult ReportSaleDaily()
        {
            MV_ReportVisit model = new MV_ReportVisit();
            SetDataCombobox(model);
            #region set data dropbox
            List<SelectListItem> listChartType = new List<SelectListItem>(){
                new SelectListItem(){ Text = "Column", Value = "column", Selected = true },
                new SelectListItem(){ Text = "Line", Value = "line" }
                
            };
            model.listTypeChart = listChartType;
            List<SelectListItem> listGroup = new List<SelectListItem>();
            model.listGroup = listGroup;
            #endregion
            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Tracking_ReportSaleDaily", true)]
        public ActionResult ReportSaleDaily(string strFromDate, string strToDate, string Region, string Area, string Distributor, string Route, int txtTimeVisit)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                if (!string.IsNullOrEmpty(strFromDate) && !string.IsNullOrEmpty(strToDate))
                {
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.ToDate = Utility.DateTimeParse(strToDate);
                    model.regionID = Utility.StringParse(Region);
                    model.areaID = Utility.StringParse(Area);
                    model.distributorID = Utility.IntParse(Distributor);
                    model.routeID = Utility.StringParse(Route);
                    model.TimeVisit = Utility.IntParse(txtTimeVisit);

                    model.ReportSaleDailyResult = Global.Context.pp_ReportSMVisitSummary(model.FromDate, model.ToDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, SessionHelper.GetSession<string>("UserName"), null, null, null, null, null,model.TimeVisit).ToList();
                    SessionHelper.SetSession<MV_ReportVisit>("MV_ReportSaleDaily", model);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            SetDataCombobox(model);
            #region set data dropbox
            List<SelectListItem> listChartType = new List<SelectListItem>(){
                new SelectListItem(){ Text = "Column", Value = "column", Selected = true },
                new SelectListItem(){ Text = "Line", Value = "line" }
                
            };
            model.listTypeChart = listChartType;
            List<SelectListItem> listGroup = new List<SelectListItem>();
            listGroup.Add(new SelectListItem() { Text = Utility.Phrase("Salesman"), Value = "Salesman", Selected = true });
            if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.Admin.ToString()) 
                || PermissionHelper.CheckPermissionByFeature(Utility.RoleName.SuperAdmin.ToString())
                || PermissionHelper.CheckPermissionByFeature(Utility.RoleName.NSD.ToString())
                )
            {
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("Region"), Value = "Region", Selected = true });
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("Area"), Value = "Area" });
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("NPP"), Value = "Distributor" });
            }
            else if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.RSM.ToString()))
            {
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("Area"), Value = "Area", Selected = true });
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("NPP"), Value = "Distributor" });
            }
            else if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.ASM.ToString()))
            {
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("NPP"), Value = "Distributor", Selected = true });
            }
            model.listGroup = listGroup;
            #endregion
            return View(model);
        }
        private static GridViewSettings SaleDailySettingExport()
        {
            //string strMCP = ""; 
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportSaleDaily";
            settings.KeyFieldName = "FirstStartTimeAM";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            //settings.SettingsExport.RenderBrick = (sender, e) =>
            //{
            //    if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
            //        e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            //};
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("Report_Distributor");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("Report_Route");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("TB_GSBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitDate";
                field.Caption = Utility.Phrase("TB_Date");
                field.PropertiesEdit.DisplayFormatString = "yyyy-MM-dd";
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstStartTimeAM";
                field.Caption = Utility.Phrase("TB_FirstTimeVisit");
                field.PropertiesEdit.DisplayFormatString = "hh:mm:ss tt";
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LastEndTime";
                field.Caption = Utility.Phrase("TB_LastTimeVisit");
                field.PropertiesEdit.DisplayFormatString = "hh:mm:ss tt";
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletMustVisit";
                field.Caption = Utility.Phrase("TB_OutletMustVisit");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderCount";
                field.Caption = Utility.Phrase("TB_OrderCount");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletVisited";
                field.Caption = Utility.Phrase("TB_OutletVisited");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strIsMCP";
                field.Caption = Utility.Phrase("TB_IsRoute");
                //field.ColumnType = MVCxGridViewColumnType.ComboBox;
                //var cb = field.PropertiesEdit as ComboBoxProperties;

                //cb.Items.Add(new ListEditItem()
                //{
                //    Index = 0,
                //    Text = Utility.Phrase("IsRoute"),
                //    Value = "IsRoute"
                //});
                //cb.Items.Add(new ListEditItem()
                //{
                //    Index = 1,
                //    Text = Utility.Phrase("NoRoute"),
                //    Value = "NoRoute"
                //});
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalQuantity";
                field.Caption = Utility.Phrase("TB_TotalQuantity");
                field.PropertiesEdit.DisplayFormatString = "{0:0.00}";
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalAmount";
                field.Caption = Utility.Phrase("TB_TotalAmount");
                field.PropertiesEdit.DisplayFormatString = "{0:0.00}";
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitMCP";
                field.Caption = Utility.Phrase("TB_OrderRate");
                field.PropertiesEdit.DisplayFormatString = "{0:0.00}%";
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SOMCP";
                field.Caption = Utility.Phrase("TB_VisitRate");
                field.PropertiesEdit.DisplayFormatString = "{0:0.00}%";
            });

            return settings;
        }

        [ActionAuthorize("Tracking_SaleDailyExportExecl")]
        public ActionResult SaleDailyExportExecl(string strFromDate, string strToDate, string Region, string Area, string Distributor, string txtTimeVisit)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSaleDailyResult = Global.Context.pp_ReportSMVisitSummary(model.FromDate, model.ToDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, SessionHelper.GetSession<string>("UserName"), null, null, null, null, null, null).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "SaleDailyExportExecl", null);

            return GridViewExtension.ExportToXlsx(SaleDailySettingExport(), model.ReportSaleDailyResult);
        }

        [ActionAuthorize("Tracking_SaleDailyExportPDF")]
        public ActionResult SaleDailyExportPDF(string strFromDate, string strToDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSaleDailyResult = Global.Context.pp_ReportSMVisitSummary(model.FromDate, model.ToDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, SessionHelper.GetSession<string>("UserName"), null, null, null, null, null,null).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "SaleDailyExportPDF", null);

            return GridViewExtension.ExportToPdf(SaleDailySettingExport(), model.ReportSaleDailyResult);
        }

        public ActionResult SaleDailyLoadChart(string fromDate, string group)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            ChartData chartData = new ChartData();
            if (SessionHelper.GetSession<MV_ReportVisit>("MV_ReportSaleDaily") != null)
            {
                model = SessionHelper.GetSession<MV_ReportVisit>("MV_ReportSaleDaily");
                if (string.IsNullOrEmpty(group))
                {
                    group = "Salesman";
                }
                var result = new List<ReportSMVisitSummaryChartData>();
                var dynamicQuery = model.ReportSaleDailyResult.AsQueryable().GroupBy("new(" + group + "ID, " + group + "Name)", "it").Select("new(Key." + group + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(OrderCount) as OrderCount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, SUM(OutletMustVisit) AS OutletMustVisit, SUM(OutletVisited) AS OutletVisited)");
                foreach (dynamic item in dynamicQuery)
                {
                    result.Add(new ReportSMVisitSummaryChartData()
                    {
                        Name = item.Name,
                        TotalAmount = item.TotalAmount,
                        OrderCount = item.OrderCount,
                        TotalSKU = item.TotalSKU,
                        TotalQuantity = item.TotalQuantity,
                        OutletMustVisit = item.OutletMustVisit,
                        OutletVisited = item.OutletVisited,
                        VisitMCP = Decimal.Round(item.OutletMustVisit > 0 ? (item.OutletVisited * 100 / item.OutletMustVisit) : 0),
                        SOMCP = Decimal.Round(item.OutletMustVisit > 0 ? (item.OrderCount * 100 / item.OutletMustVisit) : 0),
                    });
                }

                #region Prepare Data For Chart
                var listColumns = (from item in result orderby item.Name select item.Name).Distinct().ToList();
                var seriesTotalAmount = (from item in result orderby item.Name select item.TotalAmount).Distinct().ToList();
                var seriesOrderCount = (from item in result orderby item.Name select (decimal)item.OrderCount).Distinct().ToList();
                var seriesTotalSKU = (from item in result orderby item.Name select item.TotalSKU).Distinct().ToList();
                var seriesTotalQuantity = (from item in result orderby item.Name select item.TotalQuantity).Distinct().ToList();
                var seriesOutletMustVisit = (from item in result orderby item.Name select item.OutletMustVisit).Distinct().ToList();
                var seriesOutletVisited = (from item in result orderby item.Name select item.OutletVisited).Distinct().ToList();
                var seriesVisitMCP = (from item in result orderby item.Name select (decimal)item.VisitMCP).Distinct().ToList();
                var seriesSOMCP = (from item in result orderby item.Name select (decimal)item.SOMCP).Distinct().ToList();
                #endregion

                #region Set Chart Data
                
                chartData.listSeries = new List<ColumnData>();
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalAmount"), visible = true, data = seriesTotalAmount });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OutletMustVisit"), visible = false, data = seriesOutletMustVisit });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OutletVisited"), visible = false, data = seriesOutletVisited });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OrderCount"), visible = false, data = seriesOrderCount });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("VisitMCP"), visible = false, data = seriesVisitMCP });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SOMCP"), visible = false, data = seriesSOMCP });
                chartData.listColumns = new List<string>();
                chartData.listColumns.AddRange(listColumns);
                chartData.chartName = Utility.Phrase("ChartEffective");// "Biểu đồ MTD";
                chartData.YName = "";
                #endregion
            }
            return Json(new
            {
                chartData
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ReportOutletInvalidLocation
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportOutletInvalidLocation", true)]
        public ActionResult ReportOutletInvalidLocation()
        {
            MV_ReportVisit model = new MV_ReportVisit();
            SetDataCombobox(model);
            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Tracking_ReportOutletInvalidLocation")]
        public ActionResult ReportOutletInvalidLocation(string Region, string Area, string Distributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);

                model.ReportOutletValidLocalResult = Global.Context.pp_ReportOutletInvalidLocation(model.regionID, model.areaID, model.distributorID).ToList();
                SessionHelper.SetSession<MV_ReportVisit>("MV_OutletInvalidLocation", model);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            SetDataCombobox(model);
            return View(model);
        }
        
        private static GridViewSettings OutletValidLocalSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportOutletInvalidLocation";
            settings.KeyFieldName = "OutletID";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionID";
                field.Caption = Utility.Phrase("Report_RegionID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaID";
                field.Caption = Utility.Phrase("Report_AreaID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorCode";
                field.Caption = Utility.Phrase("Report_DistributorID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("Report_Distributor");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("Report_RouteID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("Report_Route");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupID";
                field.Caption = Utility.Phrase("TB_GSBHID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("TB_GSBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("TB_NVBHID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletID";
                field.Caption = Utility.Phrase("TB_OutletID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletName";
                field.Caption = Utility.Phrase("TB_Outlet");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Address";
                field.Caption = Utility.Phrase("TB_Address");
            });
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "Longtitude";
            //    field.Caption = Utility.Phrase("TB_Longtitude");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "Latitude";
            //    field.Caption = Utility.Phrase("TB_Latitude");
            //});
            settings.Columns.Add(field =>
            {
                field.FieldName = "Phone";
                field.Caption = Utility.Phrase("TB_Phone");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Mobile";
                field.Caption = Utility.Phrase("TB_Mobile");

            });

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "Status";
            //    field.Caption = Utility.Phrase("TB_Status");
            //    //field.SetDataItemTemplateContent(c =>
            //    //{
            //    //    Response.Write(DataBinder.Eval(c.DataItem, "Status").ToString());
            //    //});
            //});
            MVCxGridViewColumn column = settings.Columns.Add("StatusTemp");
            column.UnboundType = DevExpress.Data.UnboundColumnType.String;
            column.PropertiesEdit.DisplayFormatString = "c";
            settings.CustomUnboundColumnData = (sender, e) => {
                if (e.Column.FieldName == "StatusTemp")
                {
                    string status = (string)e.GetListSourceFieldValue("Status");   
                    if(status == "A")
                    {
                        e.Value = Utility.Phrase("Active");
                    }
                    else { 
                        e.Value = Utility.Phrase("Inactive");
                    }
                }
            };
            return settings;
        }

        [ActionAuthorize("Tracking_OutletValidLocalExportExecl")]
        public ActionResult OutletValidLocalExportExecl(string strFromDate, string Region, string Area, string Distributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportOutletValidLocalResult = Global.Context.pp_ReportOutletInvalidLocation(model.regionID, model.areaID, model.distributorID).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "OutletValidLocalExportExecl", null);

            return GridViewExtension.ExportToXlsx(OutletValidLocalSettingExport(), model.ReportOutletValidLocalResult);
        }

        [ActionAuthorize("Tracking_OutletValidLocalExportPDF")]
        public ActionResult OutletValidLocalExportPDF(string strFromDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportOutletValidLocalResult = Global.Context.pp_ReportOutletInvalidLocation(model.regionID, model.areaID, model.distributorID).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "OutletValidLocalExportPDF", null);

            return GridViewExtension.ExportToPdf(OutletValidLocalSettingExport(), model.ReportOutletValidLocalResult);
        }

        #endregion

        #region Report Sale Effective
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportSaleEffective", true)]
        public ActionResult ReportSaleEffective()//
        {
            MV_ReportVisit model = new MV_ReportVisit();
            SetDataCombobox(model);

            #region set data dropbox
            List<SelectListItem> listChartType = new List<SelectListItem>(){
                new SelectListItem(){ Text = "Column", Value = "column", Selected = true },
                new SelectListItem(){ Text = "Line", Value = "line" }
                
            };
            model.listTypeChart = listChartType;
            List<SelectListItem> listGroup = new List<SelectListItem>();
            model.listGroup = listGroup;
            #endregion

            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Tracking_ReportSaleEffective")]
        public ActionResult ReportSaleEffective(string strFromDate, string Region, string Area, string Distributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                if (!string.IsNullOrEmpty(strFromDate))
                {
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.areaID = Utility.StringParse(Area);
                    model.distributorID = Utility.IntParse(Distributor);
                    model.regionID = Utility.StringParse(Region);

                    model.ReportSaleEffectiveResult = Global.Context.pp_ReportSalesAssessment(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, SessionHelper.GetSession<string>("UserName")).ToList();
                    SessionHelper.SetSession<MV_ReportVisit>("MV_ReportSaleEffective", model);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            SetDataCombobox(model);
            #region set data dropbox
            List<SelectListItem> listChartType = new List<SelectListItem>(){
                new SelectListItem(){ Text = "Column", Value = "column", Selected = true },
                new SelectListItem(){ Text = "Line", Value = "line" }
                
            };
            model.listTypeChart = listChartType;
            List<SelectListItem> listGroup = new List<SelectListItem>();
            listGroup.Add(new SelectListItem() { Text = Utility.Phrase("Salesman"), Value = "Salesman", Selected = true });
            if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.Admin.ToString()) || PermissionHelper.CheckPermissionByFeature(Utility.RoleName.SuperAdmin.ToString()))
            {
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("Region"), Value = "Region", Selected = true });
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("Area"), Value = "Area" });
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("NPP"), Value = "Distributor" });
            }
            else if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.RSM.ToString()))
            {
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("Area"), Value = "Area", Selected = true });
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("NPP"), Value = "Distributor" });
            }
            else if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.ASM.ToString()))
            {
                listGroup.Add(new SelectListItem() { Text = Utility.Phrase("NPP"), Value = "Distributor", Selected = true });
            }
            model.listGroup = listGroup;
            #endregion
            return View(model);
        }

        private static GridViewSettings SaleEffectiveSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportSaleEffective";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorCode";
                field.Caption = Utility.Phrase("Report_DistributorCode");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("Report_Distributor");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("Report_RouteID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("Report_Route");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("TB_SalesmanID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDHasMCP";
                field.Caption = Utility.Phrase("TB_MTDNumberWorkingScheduleDate");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDHasOrder";
                field.Caption = Utility.Phrase("TB_MTDWorkingDay");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "PercentWorkedDay";
                field.Caption = Utility.Phrase("TB_MTDWorkingScheduleRate");
            });
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "VisitDate";
            //    field.Caption = Utility.Phrase("TB_Date");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstSyncTime";
            //    field.Caption = Utility.Phrase("TB_SyncTime");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstStartTimeAM";
            //    field.Caption = Utility.Phrase("TB_FirstTimeVisit");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LastEndTime";
            //    field.Caption = Utility.Phrase("TB_LastTimeVisit");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "OutletMustVisit";
            //    field.Caption = Utility.Phrase("TB_OutletMustVisit");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "OutletVisited";
            //    field.Caption = Utility.Phrase("TB_OutletVisited");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "Visit_MCP";
            //    field.Caption = Utility.Phrase("TB_VisitRate");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "OrderCount";
            //    field.Caption = Utility.Phrase("TB_OrderCount");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "SO_MCP";
            //    field.Caption = Utility.Phrase("TB_OrderRate");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "TotalQuantity";
            //    field.Caption = Utility.Phrase("TB_TotalQuantity");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "TotalSKU";
            //    field.Caption = Utility.Phrase("TB_TotalSKU");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LPPC";
            //    field.Caption = Utility.Phrase("TB_LPPC");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "TotalAmount";
            //    field.Caption = Utility.Phrase("TB_TotalAmount");
            //});



            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDOutletMustVisit";
                field.Caption = Utility.Phrase("TB_MTDOutletMustVisit");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDOutletVisited";
                field.Caption = Utility.Phrase("TB_MTDOutletVisited");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "strIsMCP";
                field.Caption = Utility.Phrase("TB_IsRoute");
                field.ColumnType = MVCxGridViewColumnType.ComboBox;
                var cb = field.PropertiesEdit as ComboBoxProperties;

                cb.Items.Add(new ListEditItem()
                {
                    Index = 0,
                    Text = Utility.Phrase("IsRoute"),
                    Value = "IsRoute"
                });
                cb.Items.Add(new ListEditItem()
                {
                    Index = 1,
                    Text = Utility.Phrase("NoRoute"),
                    Value = "NoRoute"
                });
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDOrderCount";
                field.Caption = Utility.Phrase("TB_MTDOrderCount");
            });
            
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDTotalQuantity";
                field.Caption = Utility.Phrase("TB_MTDTotalQuantity");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDTotalSKU";
                field.Caption = Utility.Phrase("TB_MTDTotalSKU");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDLPPC";
                field.Caption = Utility.Phrase("TB_MTDLPPC");
            });
           
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDTotalAmount";
                field.Caption = Utility.Phrase("TB_MTDTotalAmount");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDVisit_MCP";
                field.Caption = Utility.Phrase("TB_MTDVisitRate");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDSO_MCP";
                field.Caption = Utility.Phrase("TB_MTDSO_MCP");
            });

            return settings;
        }

        [ActionAuthorize("Tracking_SaleEffectiveExportExecl")]
        public ActionResult SaleEffectiveExportExecl(string strFromDate, string Region, string Area, string Distributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSaleEffectiveResult = Global.Context.pp_ReportSalesAssessment(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "SaleEffectiveExportExecl", null);

            return GridViewExtension.ExportToXlsx(SaleEffectiveSettingExport(), model.ReportSaleEffectiveResult);
        }

        [ActionAuthorize("Tracking_SaleEffectiveExportPDF")]
        public ActionResult SaleEffectiveExportPDF(string strFromDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportSaleEffectiveResult = Global.Context.pp_ReportSalesAssessment(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "SaleEffectiveExportPDF", null);

            return GridViewExtension.ExportToPdf(SaleEffectiveSettingExport(), model.ReportSaleEffectiveResult);
        }

        public ActionResult SaleEffectiveLoadChart(string fromDate, string group)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            ChartData chartData = new ChartData();
            if (SessionHelper.GetSession<MV_ReportVisit>("MV_ReportSaleEffective") != null)
            {
                model = SessionHelper.GetSession<MV_ReportVisit>("MV_ReportSaleEffective");
                if (string.IsNullOrEmpty(group))
                {
                    group = "Salesman";
                }
                var result = new List<ReportSMVisitSummaryChartData>();
                var dynamicQuery = model.ReportSaleEffectiveResult.AsQueryable().GroupBy("new(" + group + "ID, " + group + "Name)", "it").Select("new(Key." + group + "Name as Name, SUM(MTDTotalAmount) as TotalAmount, SUM(MTDOrderCount) as OrderCount, SUM(MTDTotalSKU) as TotalSKU, SUM(MTDTotalQuantity) as TotalQuantity, SUM(MTDSO_MCP) AS MTDSO_MCP, SUM(MTDVisit_MCP) AS MTDVisit_MCP, SUM(MTDOutletMustVisit) AS OutletMustVisit, SUM(MTDOutletVisited) AS OutletVisited)");
                foreach (dynamic item in dynamicQuery)
                {
                    result.Add(new ReportSMVisitSummaryChartData()
                    {
                        Name = item.Name,
                        TotalAmount = item.TotalAmount,
                        OrderCount = item.OrderCount,
                        TotalSKU = item.TotalSKU,
                        TotalQuantity = item.TotalQuantity,
                        OutletMustVisit = item.OutletMustVisit,
                        OutletVisited = item.OutletVisited,
                        LPPC = item.OrderCount > 0 ? item.TotalSKU/ item.OrderCount : 0,
                        VisitMCP = Decimal.Round(item.OutletMustVisit > 0 ? (item.OutletVisited * 100 / item.OutletMustVisit) : 0),
                        SOMCP = Decimal.Round(item.OutletMustVisit > 0 ? (item.OrderCount * 100 / item.OutletMustVisit) : 0),
                    });
                }

                #region Prepare Data For Chart
                List<string> listColumns = new List<string>();
                List<decimal> seriesTotalAmount = new List<decimal>();
                List<decimal> seriesOrderCount = new List<decimal>();
                List<decimal> seriesTotalSKU = new List<decimal>();
                List<decimal> seriesTotalQuantity = new List<decimal>();
                List<decimal> seriesLPPC = new List<decimal>();
                List<decimal> seriesSOMCP = new List<decimal>();
                List<decimal> seriesVisitMCP = new List<decimal>();
                List<decimal> seriesOutletMustVisit = new List<decimal>();
                List<decimal> seriesOutletVisited = new List<decimal>();



                foreach (var elm in result.Distinct().ToList())
                {
                    listColumns.Add(elm.Name);
                    seriesTotalAmount.Add(elm.TotalAmount);
                    seriesOrderCount.Add(elm.OrderCount);
                    seriesTotalSKU.Add(elm.TotalSKU);
                    seriesTotalQuantity.Add(elm.TotalQuantity);
                    seriesLPPC.Add(elm.LPPC.HasValue ? elm.LPPC.Value : 0);
                    seriesSOMCP.Add(elm.SOMCP.HasValue ? elm.SOMCP.Value : 0);
                    seriesVisitMCP.Add(elm.VisitMCP.HasValue ? elm.VisitMCP.Value : 0);
                    seriesOutletMustVisit.Add(elm.OutletMustVisit);
                    seriesOutletVisited.Add(elm.OutletVisited);
                }
                #endregion

                #region Set Chart Data
                chartData.listSeries = new List<ColumnData>();
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalAmount"), visible = true, data = seriesTotalAmount });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OutletMustVisit"), visible = false, data = seriesOutletMustVisit });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OutletVisited"), visible = false, data = seriesOutletVisited });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("OrderCount"), visible = false, data = seriesOrderCount });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("VisitMCP"), visible = false, data = seriesVisitMCP });
                chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("SOMCP"), visible = false, data = seriesSOMCP });
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalSKU"), visible = false, data = seriesTotalSKU });
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("TotalQuantity"), visible = true, data = seriesTotalQuantity });
                //chartData.listSeries.Add(new ColumnData() { name = Utility.Phrase("LPPC"), visible = false, data = seriesLPPC });



                chartData.listColumns = new List<string>();
                chartData.listColumns.AddRange(listColumns);
                chartData.chartName = Utility.Phrase("ChartEffective");// "Biểu đồ MTD";
                chartData.YName = "";
                #endregion
            }
            return Json(new
            {
                chartData
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Report Assessment PC
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Tracking_ReportAssessmentPC", true)]
        public ActionResult ReportAssessmentPC()//
        {
            MV_ReportVisit model = new MV_ReportVisit();
            SetDataCombobox(model);
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Tracking_ReportAssessmentPC")]
        public ActionResult ReportAssessmentPC(string strFromDate, string Region, string Area, string Distributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                if (!string.IsNullOrEmpty(strFromDate))
                {
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.areaID = Utility.StringParse(Area);
                    model.distributorID = Utility.IntParse(Distributor);
                    model.regionID = Utility.StringParse(Region);

                    model.ReportAssessmentPCResult = Global.Context.pp_ReportPC_SM(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, 2, 0, 1, SessionHelper.GetSession<string>("UserName")).ToList();
                    SessionHelper.SetSession<MV_ReportVisit>("MV_ReportAssessmentPC", model);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            SetDataCombobox(model);
            return View(model);
        }

        private static GridViewSettings AssessmentPCSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportAssessmentPC";
            settings.KeyFieldName = "SalesmanID";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionID";
                field.Caption = Utility.Phrase("Report_RegionID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaID";
                field.Caption = Utility.Phrase("Report_AreaID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorID";
                field.Caption = Utility.Phrase("Report_DistributorID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("Report_Distributor");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("Report_RouteID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("Report_Route");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("TB_NVBHID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitDate";
                field.Caption = Utility.Phrase("TB_DayVisit");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "PCPass";
                field.Caption = Utility.Phrase("TB_AchievingPC");
                //field.UnboundType = DevExpress.Data.UnboundColumnType.String;
                //// An unbound expression. 
                //field.UnboundExpression = Utility.Phrase("[PCPass]+PCPass");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MCP";
                field.Caption = Utility.Phrase("TB_MCP");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderCount";
                field.Caption = Utility.Phrase("TB_OrderCount");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "PC_MCP";
                field.Caption = Utility.Phrase("TB_OderMCPRate");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDWorkPlan";
                field.Caption = Utility.Phrase("TB_WorkingDayPlan");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ActualManday";
                field.Caption = Utility.Phrase("TB_WorkingDay");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDPCPass";
                field.Caption = Utility.Phrase("TB_DayPassPC");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDMCP";
                field.Caption = Utility.Phrase("TB_MTDMCP");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDOrderCount";
                field.Caption = Utility.Phrase("TB_MTDNumberOrder");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTD_AVG_PC";
                field.Caption = Utility.Phrase("TB_MTDOrderAverage");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDPC_MCP";
                field.Caption = Utility.Phrase("TB_MTDOderMCPRate");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDTotalSKU";
                field.Caption = Utility.Phrase("TB_MTDTotalSKU");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "MTDLPPC";
                field.Caption = Utility.Phrase("TB_MTDLPPC");
            });
            return settings;
        }

        [ActionAuthorize("Tracking_AssessmentPCExportExecl")]
        public ActionResult AssessmentPCExportExecl(string strFromDate, string Region, string Area, string Distributor)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportAssessmentPCResult = Global.Context.pp_ReportPC_SM(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, 2, 0, 1, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "AssessmentPCExportExecl", null);

            return GridViewExtension.ExportToXlsx(AssessmentPCSettingExport(), model.ReportAssessmentPCResult);
        }

        [ActionAuthorize("Tracking_AssessmentPCExportPDF")]
        public ActionResult AssessmentPCExportPDF(string strFromDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportVisit model = new MV_ReportVisit();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportAssessmentPCResult = Global.Context.pp_ReportPC_SM(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, 2, 0, 1, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "AssessmentPCExportPDF", null);

            return GridViewExtension.ExportToPdf(AssessmentPCSettingExport(), model.ReportAssessmentPCResult);
        }

        #endregion

        #region Combobox
        public void SetDataComboboxSummarySales(MV_ReportSummarySales model)
        {
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.regionID;
            ctrCombobox.TitleKey = Utility.Phrase("RegionID");
            ctrCombobox.TitleName = Utility.Phrase("RegionName");
            ctrCombobox.listOption = ControllerHelper.GetListRegion().Select(s => new OptionCombobox { ID = s.RegionID, Key = s.RegionID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxRegion = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.areaID;
            ctrCombobox.TitleKey = Utility.Phrase("AreaID");
            ctrCombobox.TitleName = Utility.Phrase("AreaName");
            ctrCombobox.listOption = ControllerHelper.GetListArea(model.regionID).Select(s => new OptionCombobox { ID = s.AreaID, Key = s.AreaID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxArea = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("DistributorCode");
            ctrCombobox.TitleName = Utility.Phrase("DistributorName");
            ctrCombobox.SeleteID = model.distributorID.ToString();
            ctrCombobox.listOption = ControllerHelper.GetListDistributorWithRegionArea(model.regionID, model.areaID).Select(s => new OptionCombobox { ID = s.DistributorID.ToString(), Key = s.DistributorCode.ToString().Trim(), Value = s.DistributorName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxDistributor = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("RouteCD");
            ctrCombobox.TitleName = Utility.Phrase("RouteName");
            ctrCombobox.SeleteID = model.routeID;
            ctrCombobox.listOption = ControllerHelper.GetListRouteWithRegionAreaDis(model.regionID, model.areaID, model.distributorID).Select(s => new OptionCombobox { ID = s.RouteID.ToString(), Key = s.RouteID.ToString(), Value = s.RouteName }).Distinct().ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxRoute = ctrCombobox;
        }
        public void SetDataComboboxDrugstores(MV_ReportDrugstores model)
        {
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.regionID;
            ctrCombobox.TitleKey = Utility.Phrase("RegionID");
            ctrCombobox.TitleName = Utility.Phrase("RegionName");
            ctrCombobox.listOption = ControllerHelper.GetListRegion().Select(s => new OptionCombobox { ID = s.RegionID, Key = s.RegionID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxRegion = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.areaID;
            ctrCombobox.TitleKey = Utility.Phrase("AreaID");
            ctrCombobox.TitleName = Utility.Phrase("AreaName");
            ctrCombobox.listOption = ControllerHelper.GetListArea(model.regionID).Select(s => new OptionCombobox { ID = s.AreaID, Key = s.AreaID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxArea = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("DistributorCode");
            ctrCombobox.TitleName = Utility.Phrase("DistributorName");
            ctrCombobox.SeleteID = model.distributorID.ToString();
            ctrCombobox.listOption = ControllerHelper.GetListDistributorWithRegionArea(model.regionID, model.areaID).Select(s => new OptionCombobox { ID = s.DistributorID.ToString(), Key = s.DistributorCode.ToString(), Value = s.DistributorName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxDistributor = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("RouteCD");
            ctrCombobox.TitleName = Utility.Phrase("RouteName");
            ctrCombobox.SeleteID = model.routeID;
            ctrCombobox.listOption = ControllerHelper.GetListRouteWithRegionAreaDis(model.regionID, model.areaID, model.distributorID).Select(s => new OptionCombobox { ID = s.RouteID.ToString(), Key = s.RouteID.ToString(), Value = s.RouteName }).Distinct().ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxRoute = ctrCombobox;
        }
        public void SetDataComboboxIssues(MV_ReportIssues model)
        {
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.regionID;
            ctrCombobox.TitleKey = Utility.Phrase("RegionID");
            ctrCombobox.TitleName = Utility.Phrase("RegionName");
            ctrCombobox.listOption = ControllerHelper.GetListRegion().Select(s => new OptionCombobox { ID = s.RegionID, Key = s.RegionID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxRegion = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.areaID;
            ctrCombobox.TitleKey = Utility.Phrase("AreaID");
            ctrCombobox.TitleName = Utility.Phrase("AreaName");
            ctrCombobox.listOption = ControllerHelper.GetListArea(model.regionID).Select(s => new OptionCombobox { ID = s.AreaID, Key = s.AreaID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxArea = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("DistributorCode");
            ctrCombobox.TitleName = Utility.Phrase("DistributorName");
            ctrCombobox.SeleteID = model.distributorID.ToString();
            ctrCombobox.listOption = ControllerHelper.GetListDistributorWithRegionArea(model.regionID, model.areaID).Select(s => new OptionCombobox { ID = s.DistributorID.ToString(), Key = s.DistributorCode.ToString().Trim(), Value = s.DistributorName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxDistributor = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("RouteCD");
            ctrCombobox.TitleName = Utility.Phrase("RouteName");
            ctrCombobox.SeleteID = model.routeID;
            ctrCombobox.listOption = ControllerHelper.GetListRouteWithRegionAreaDis(model.regionID, model.areaID, model.distributorID).Select(s => new OptionCombobox { ID = s.RouteID.ToString(), Key = s.RouteID.ToString(), Value = s.RouteName }).Distinct().ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxRoute = ctrCombobox;


            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("StatusID");
            ctrCombobox.TitleName = Utility.Phrase("StatusName");
            ctrCombobox.SeleteID = model.StatusID.ToString();
            List<StatusIssues> listItem = new List<StatusIssues>();
            StatusIssues ins = new StatusIssues();

            ins.ID = 110;
            ins.Name = Utility.Phrase("Open");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 111;
            ins.Name = Utility.Phrase("ReOpen");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 112;
            ins.Name = Utility.Phrase("WAITING_CONFIRM");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 113;
            ins.Name = Utility.Phrase("Close");
            listItem.Add(ins);
            ins = new StatusIssues();
            ins.ID = 0;
            ins.Name = Utility.Phrase("All");
            listItem.Add(ins);

            ctrCombobox.listOption = listItem.Select(s => new OptionCombobox { ID = s.ID.ToString(), Key = s.ID.ToString(), Value = s.Name }).Distinct().ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxStatus = ctrCombobox;
        }
        public void SetDataCombobox(MV_ReportVisit model)
        {
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.regionID;
            ctrCombobox.TitleKey = Utility.Phrase("RegionID");
            ctrCombobox.TitleName = Utility.Phrase("RegionName");
            ctrCombobox.listOption = ControllerHelper.GetListRegion().Select(s => new OptionCombobox { ID = s.RegionID, Key = s.RegionID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxRegion = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.areaID;
            ctrCombobox.TitleKey = Utility.Phrase("AreaID");
            ctrCombobox.TitleName = Utility.Phrase("AreaName");
            ctrCombobox.listOption = ControllerHelper.GetListArea(model.regionID).Select(s => new OptionCombobox { ID = s.AreaID, Key = s.AreaID, Value = s.Name }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxArea = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("DistributorCode");
            ctrCombobox.TitleName = Utility.Phrase("DistributorName");
            ctrCombobox.SeleteID = model.distributorID.ToString();
            ctrCombobox.listOption = ControllerHelper.GetListDistributorWithRegionArea(model.regionID, model.areaID).Select(s => new OptionCombobox { ID = s.DistributorID.ToString(), Key = s.DistributorCode.ToString(), Value = s.DistributorName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxDistributor = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("RouteCD");
            ctrCombobox.TitleName = Utility.Phrase("RouteName");
            ctrCombobox.SeleteID = model.routeID;
            ctrCombobox.listOption = ControllerHelper.GetListRouteWithRegionAreaDis(model.regionID, model.areaID, model.distributorID).Select(s => new OptionCombobox { ID = s.RouteID.ToString(), Key = s.RouteID.ToString(), Value = s.RouteName }).Distinct().ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.listComboboxRoute = ctrCombobox;
        }
        public void SetDataComboboxWorkWith(MV_ReportVisit model)
        {
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.regionID;
            ctrCombobox.TitleKey = Utility.Phrase("RegionID");
            ctrCombobox.TitleName = Utility.Phrase("RegionName");
            ctrCombobox.listOption = ControllerHelper.GetListRegion(model.regionID).Select(s => new OptionCombobox { ID = s.RegionID, Key = s.RegionID, Value = s.Name }).ToList();
            model.listComboboxRegion = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.areaID;
            ctrCombobox.TitleKey = Utility.Phrase("AreaID");
            ctrCombobox.TitleName = Utility.Phrase("AreaName");
            ctrCombobox.listOption = ControllerHelper.GetListArea(model.regionID).Select(s => new OptionCombobox { ID = s.AreaID, Key = s.AreaID, Value = s.Name }).ToList();
            model.listComboboxArea = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("SaleSupID");
            ctrCombobox.TitleName = Utility.Phrase("SaleSupName");
            ctrCombobox.SeleteID = model.routeID;
            ctrCombobox.listOption = ControllerHelper.GetListSaleSup(model.regionID, model.areaID, "", 0).Select(s => new OptionCombobox { ID = s.EmployeeID.ToString(), Key = s.EmployeeID.ToString(), Value = s.EmployeeName }).ToList();
            model.listComboboxSaleSup = ctrCombobox;
        }


        public ActionResult ReloadOptionArea(string regionID)
        {
            ViewControlCombobox ctrArea = new ViewControlCombobox();
            ctrArea.SeleteID = null;
            ctrArea.TitleKey = Utility.Phrase("AreaID");
            ctrArea.TitleName = Utility.Phrase("AreaName");
            ctrArea.listOption = ControllerHelper.GetListArea(regionID).Select(s => new OptionCombobox { ID = s.AreaID, Key = s.AreaID, Value = s.Name }).ToList();

            ViewData["NameID"] = "Area";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrArea);
        }
        public ActionResult ReloadOptionDistributor(string regionID, string areaID)
        {
            ViewControlCombobox ctrDistributor = new ViewControlCombobox();
            ctrDistributor.TitleKey = Utility.Phrase("DistributorID");
            ctrDistributor.TitleName = Utility.Phrase("DistributorName");
            ctrDistributor.SeleteID = null;

            if (!string.IsNullOrEmpty(regionID) && !string.IsNullOrEmpty(areaID))
            {
                ctrDistributor.listOption = ControllerHelper.GetListDistributorWithRegionArea(regionID, areaID).Select(s => new OptionCombobox() { ID = s.DistributorID.ToString().Trim(), Key = s.DistributorCode, Value = s.DistributorName }).ToList();
            }
            else
            {
                ctrDistributor.listOption = ControllerHelper.ListDistributor.Select(s => new OptionCombobox() { ID = s.DistributorID.ToString(), Key = s.DistributorCode.ToString().Trim(), Value = s.DistributorName }).ToList();
            }
            ViewData["NameID"] = "Distributor";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrDistributor);
        }
        public ActionResult ReloadOptionRoute(string regionID, string areaID, int distributorID)
        {
            ViewControlCombobox ctrRoute = new ViewControlCombobox();
            ctrRoute.TitleKey = Utility.Phrase("RouteID");
            ctrRoute.TitleName = Utility.Phrase("RouteName");
            ctrRoute.SeleteID = null;
            ctrRoute.listOption = ControllerHelper.GetListRouteWithRegionAreaDis(regionID, areaID, distributorID).Select(s => new OptionCombobox { ID = s.RouteID, Key = s.RouteID, Value = s.RouteName }).Distinct().ToList();
            ViewData["NameID"] = "Route";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrRoute);
        }
        public ActionResult ReloadOptionSaleSup(string regionID, string areaID)
        {
            ViewControlCombobox ctrSaleSup = new ViewControlCombobox();
            ctrSaleSup.TitleKey = Utility.Phrase("SaleSupID");
            ctrSaleSup.TitleName = Utility.Phrase("SaleSupName");
            ctrSaleSup.SeleteID = null;
            ctrSaleSup.listOption = ControllerHelper.GetListSaleSup(regionID, areaID, "", 0).Select(s => new OptionCombobox { ID = s.EmployeeID, Key = s.EmployeeID, Value = s.EmployeeName }).ToList();
            ViewData["NameID"] = "SaleSup";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrSaleSup);
        }
        public ActionResult ReloadOptionSaleMan(string regionID, string areaID)
        {
            ViewControlCombobox ctrSaleMan = new ViewControlCombobox();
            ctrSaleMan.TitleKey = Utility.Phrase("SaleManID");
            ctrSaleMan.TitleName = Utility.Phrase("SaleManName");
            ctrSaleMan.SeleteID = null;
            ctrSaleMan.listOption = ControllerHelper.GetListSalesman(regionID, areaID, "", 0, "").Select(s => new OptionCombobox { ID = s.SalesmanID, Key = s.SalesmanID, Value = s.SalesmanName }).ToList();
            ViewData["NameID"] = "SaleMan";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrSaleMan);
        }
        public ActionResult ReloadOptionProgram(string ProgramID)
        {
            ViewControlCombobox ctrEvaluation = new ViewControlCombobox();
            ctrEvaluation.TitleKey = Utility.Phrase("EvaluationName");
            ctrEvaluation.TitleName = Utility.Phrase("TimeEvaluation");
            ctrEvaluation.SeleteID = null;
            ctrEvaluation.listOption = Global.VisibilityContext.DMSEvaluations.Where(x => x.ProgramID == ProgramID).Select(s => new OptionCombobox { ID = s.EvaluationID.ToString(), Key = s.EvaluationID.ToString(), Value = s.EvalDateFrom.Date.ToString() + " - " + s.EvalDateTo.Date.ToString() }).ToList();
            ViewData["NameID"] = "Evaluation";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrEvaluation);
        }
        #endregion

        #region LIST Report Evaluation
        #region ReportEvaluation
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_Evaluation", true)]
        public ActionResult ReportEvaluation()//
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvaluation();
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Report_Evaluation")]
        public ActionResult ReportEvaluation(string strFromDate, string strToDate, string Region, string Area, string Distributor, string SaleSup, string Route, string Saleman, string Program, string Evaluation, int EvalState = 0, int TypeOfSS = 0)
        {
            ReportEvalVM model = new ReportEvalVM();

            try
            {
                #region Validate and Get Data
                model.SetDataReportEvaluation(strFromDate, strToDate, Program, Region, Area, Utility.IntParse(Distributor), Route, string.Empty, SaleSup, Saleman, Evaluation, TypeOfSS);
                #endregion
                #region Result fllow Role
                List<ResultReportEvalVM> result = new List<ResultReportEvalVM>();

                if (model.RoleView == Utility.RoleName.Leader)
                {
                    var data = Global.VisibilityContext.usp_GetReportEvalBy(model.FromDate, model.ToDate, model.RegionID, model.AreaID, model.SaleSupID, model.RouteID, model.SalesmanID, model.ProgramID, model.EvaluationID, model.Auditor).ToList();
                    result = (
                        from row in data
                        group row by new { row.ProgramID, row.ProgramName, row.EvaluationID, row.EvalState, row.MarkingAssign } into g
                        select new ResultReportEvalVM()
                        {
                            ProgramName = g.Key.ProgramName,
                            EvaluationID = g.Key.EvaluationID,
                            EvalState = Int32.Parse(g.Key.EvalState.ToString()),
                            MarkingAssign = g.Key.MarkingAssign,
                            TotalImg = g.Sum(x => x.TotalImg.Value),
                            ImagMarking = g.Sum(x => x.ImagMarking.Value),
                            ImgApproved = g.Sum(x => x.ImgApproved.Value),
                            ImgPassProgram = g.Sum(x => x.ImgPassProgram.Value),
                            ImgRejected = g.Sum(x => x.ImgRejected.Value),
                            ImgReMarking = g.Sum(x => x.ImgReMarking.Value),
                            TotalOulet = g.Sum(x => x.TotalOulet.Value),
                            OuletHasMarking = g.Sum(x => x.OuletHasMarking.Value),
                        }
                        ).ToList();
                    model.listResult = result;
                }
                else if (model.RoleView == Utility.RoleName.Auditor)
                {
                    List<usp_GetReportEvalByResult> data = Global.VisibilityContext.usp_GetReportEvalBy(model.FromDate, model.ToDate, model.RegionID, model.AreaID, model.SaleSupID, model.RouteID, model.SalesmanID, model.ProgramID, model.EvaluationID, model.Auditor).ToList();
                    result = (
                        from row in data
                        group row by new { row.ProgramID, row.ProgramName, row.EvaluationID, row.EvalState } into g
                        select new ResultReportEvalVM()
                        {
                            ProgramName = g.Key.ProgramName,
                            EvaluationID = g.Key.EvaluationID,
                            EvalState = Int32.Parse(g.Key.EvalState.ToString()),
                            TotalImg = g.Sum(x => x.TotalImg.Value),
                            ImagMarking = g.Sum(x => x.ImagMarking.Value),
                            ImgApproved = g.Sum(x => x.ImgApproved.Value),
                            ImgPassProgram = g.Sum(x => x.ImgPassProgram.Value),
                            ImgRejected = g.Sum(x => x.ImgRejected.Value),
                            ImgReMarking = g.Sum(x => x.ImgReMarking.Value),
                            TotalOulet = g.Sum(x => x.TotalOulet.Value),
                            OuletHasMarking = g.Sum(x => x.OuletHasMarking.Value),
                        }
                        ).ToList();
                    model.listResult = result;
                }
                else if (model.RoleView == Utility.RoleName.Admin || model.RoleView == Utility.RoleName.SuperAdmin)
                {
                    var data = Global.VisibilityContext.usp_GetReportEvalBy(model.FromDate, model.ToDate, model.RegionID, model.AreaID, model.SaleSupID, model.RouteID, model.SalesmanID, model.ProgramID, model.EvaluationID, model.Auditor).ToList();
                    result = (
                        from row in data
                        group row by new { row.RegionID, row.RegionName, row.ProgramID, row.ProgramName } into g
                        select new ResultReportEvalVM()
                        {
                            RegionName = g.Key.RegionName,
                            ProgramName = g.Key.ProgramName,
                            TotalImg = g.Sum(x => x.TotalImg.Value),
                            ImagMarking = g.Sum(x => x.ImagMarking.Value),
                            ImgApproved = g.Sum(x => x.ImgApproved.Value),
                            ImgPassProgram = g.Sum(x => x.ImgPassProgram.Value),
                            ImgRejected = g.Sum(x => x.ImgRejected.Value),
                            ImgReMarking = g.Sum(x => x.ImgReMarking.Value),
                            TotalOulet = g.Sum(x => x.TotalOulet.Value),
                            OuletHasMarking = g.Sum(x => x.OuletHasMarking.Value),
                        }
                        ).ToList();
                    model.listResult = result;
                }
                else if (model.RoleView == Utility.RoleName.RSM || model.RoleView == Utility.RoleName.ASM)
                {
                    var data = Global.VisibilityContext.usp_GetReportEvalBy(model.FromDate, model.ToDate, model.RegionID, model.AreaID, model.SaleSupID, model.RouteID, model.SalesmanID, model.ProgramID, model.EvaluationID, model.Auditor).ToList();
                    result = (
                        from row in data
                        group row by new { row.AreaID, row.AreaName, row.DistributorID, row.DistributorName, row.SaleSupID, row.SaleSupName, row.ProgramID, row.ProgramName } into g
                        select new ResultReportEvalVM()
                        {
                            AreaName = g.Key.AreaName,
                            DistributorName = g.Key.DistributorName,
                            SaleSupName = g.Key.SaleSupName,
                            ProgramName = g.Key.ProgramName,
                            TotalImg = g.Sum(x => x.TotalImg.Value),
                            ImagMarking = g.Sum(x => x.ImagMarking.Value),
                            ImgApproved = g.Sum(x => x.ImgApproved.Value),
                            ImgPassProgram = g.Sum(x => x.ImgPassProgram.Value),
                            ImgRejected = g.Sum(x => x.ImgRejected.Value),
                            ImgReMarking = g.Sum(x => x.ImgReMarking.Value),
                            TotalOulet = g.Sum(x => x.TotalOulet.Value),
                            OuletHasMarking = g.Sum(x => x.OuletHasMarking.Value),
                        }
                        ).ToList();
                    model.listResult = result;
                }
                else if (model.RoleView == Utility.RoleName.SS)
                {
                    model.SaleSupID = SessionHelper.GetSession<string>("UserName");
                    List<usp_GetReportBySSResult> data = Global.VisibilityContext.usp_GetReportBySS(model.FromDate, model.ToDate, model.SaleSupID, model.RouteID, model.SalesmanID, model.ProgramID).ToList();
                    if (model.TypeOfSS == 1)
                    {
                        result = (
                        from row in data
                        group row by new { row.ProgramID, row.ProgramName, row.RouteID, row.RouteName } into g
                        select new ResultReportEvalVM()
                        {
                            ProgramName = g.Key.ProgramName,
                            RouteName = g.Key.RouteName,
                            TotalImg = g.Sum(x => x.TotalImg.Value),
                            ImagMarking = g.Sum(x => x.ImagMarking.Value),
                            ImgApproved = g.Sum(x => x.ImgApproved.Value),
                            ImgPassProgram = g.Sum(x => x.ImgPassProgram.Value),
                            ImgRejected = g.Sum(x => x.ImgRejected.Value),
                            ImgReMarking = g.Sum(x => x.ImgReMarking.Value),
                            ImgFake = g.Sum(x => x.ImgFake.Value),
                            NotCaptured = g.Sum(x => x.NotCaptured.Value)
                        }
                        ).ToList();
                        model.listResult = result;
                    }
                    else if (model.TypeOfSS == 2)
                    {
                        result = (
                        from row in data
                        group row by new { row.ProgramID, row.ProgramName, row.SalesmanID, row.SalesmanName } into g
                        select new ResultReportEvalVM()
                        {
                            ProgramName = g.Key.ProgramName,
                            SalesmanName = g.Key.SalesmanName,
                            TotalImg = g.Sum(x => x.TotalImg.Value),
                            ImagMarking = g.Sum(x => x.ImagMarking.Value),
                            ImgApproved = g.Sum(x => x.ImgApproved.Value),
                            ImgPassProgram = g.Sum(x => x.ImgPassProgram.Value),
                            ImgRejected = g.Sum(x => x.ImgRejected.Value),
                            ImgReMarking = g.Sum(x => x.ImgReMarking.Value),
                            ImgFake = g.Sum(x => x.ImgFake.Value),
                            NotCaptured = g.Sum(x => x.NotCaptured.Value)
                        }
                        ).ToList();
                        model.listResult = result;
                    }
                    else if (model.TypeOfSS == 3)
                    {
                        result = (
                        from row in data
                        group row by new { row.ProgramID, row.ProgramName, row.CustomerID, row.OutletName, row.Address } into g
                        select new ResultReportEvalVM()
                        {
                            ProgramName = g.Key.ProgramName,
                            RouteName = g.Key.OutletName,
                            Adress = g.Key.Address,
                            TotalImg = g.Sum(x => x.TotalImg.Value),
                            ImagMarking = g.Sum(x => x.ImagMarking.Value),
                            ImgApproved = g.Sum(x => x.ImgApproved.Value),
                            ImgPassProgram = g.Sum(x => x.ImgPassProgram.Value),
                            ImgRejected = g.Sum(x => x.ImgRejected.Value),
                            ImgReMarking = g.Sum(x => x.ImgReMarking.Value),
                            ImgFake = g.Sum(x => x.ImgFake.Value),
                            NotCaptured = g.Sum(x => x.NotCaptured.Value)
                        }
                        ).ToList();
                        model.listResult = result;
                    }
                    else
                    {
                        foreach (var item in data)
                        {
                            result.Add(
                                new ResultReportEvalVM()
                                {
                                    RouteID = item.RouteID,
                                    RouteName = item.RouteName,
                                    SalesmanID = item.SalesmanID,
                                    SalesmanName = item.SalesmanName,
                                    ProgramName = item.ProgramName,
                                    OutletName = item.OutletName,
                                    Adress = item.Address,
                                    TotalImg = item.TotalImg.Value,
                                    ImgPassProgram = item.ImgPassProgram.Value,
                                    ImgFake = item.ImgFake.Value,
                                    NotCaptured = item.NotCaptured.Value,
                                    PassRate = (item.ImgPassProgram.Value > 0) ? (item.ImgPassProgram.Value / item.TotalImg.Value) : 0
                                });
                        }
                        model.listResult = result;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            return View(model);
        } 
        #endregion

        #region EvalMarkImage
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_EvalMarkImage", true)]
        public ActionResult EvalMarkImage()
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvaluation();
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Report_EvalMarkImage")]
        public ActionResult EvalMarkImage(string strFromDate, string strToDate, string Program, string Region, string Area, string Distributor, string Route, string Outlet, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            try
            {
                #region Validate and Get Data
                model.SetDataReportEvaluation(strFromDate, strToDate, Program, Region, Area, Utility.IntParse(Distributor), Route, Outlet, string.Empty, string.Empty, Evaluation);
                model.DataEvalImage = Global.VisibilityContext.usp_GetReportEvalImageBy(model.FromDate, model.ToDate, model.ProgramID, model.EvaluationID, model.RegionID, model.AreaID, model.DistributorID, model.RouteID, model.OutletID, model.SaleSupID, model.SalesmanID).ToList();
                #endregion
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            return View(model);
        }

        //Export excel EvalMarkImage
        [ActionAuthorize("Report_EvalMarkImage")]
        public ActionResult ReportEvalMarkImageExportExecl(string strFromDate, string strToDate, string Program, string Region, string Area, string Distributor, string Route, string Outlet, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.ProgramID = Utility.StringParse(Program);
                model.RegionID = Utility.StringParse(Region);
                model.AreaID = Utility.StringParse(Area);
                model.DistributorID = Utility.IntParse(Distributor);
                model.RouteID = Utility.StringParse(Route);
                model.OutletID = Utility.StringParse(Outlet);
                model.EvaluationID = Utility.StringParse(Evaluation);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.DataEvalImage = Global.VisibilityContext.usp_GetReportEvalImageBy(model.FromDate, model.ToDate, model.ProgramID, model.EvaluationID, model.RegionID, model.AreaID, model.DistributorID, model.RouteID, model.OutletID, model.SaleSupID, model.SalesmanID).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportEvalMarkImageExportExecl", null);

            return GridViewExtension.ExportToXlsx(EvalMarkImageSettingExport(), model.DataEvalImage);
        }
        //Export PDF EvalMarkImage
        [ActionAuthorize("Report_EvalMarkImage")]
        public ActionResult ReportEvalMarkImageExportPDF(string strFromDate, string strToDate, string Program, string Region, string Area, string Distributor, string Route, string Outlet, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.ProgramID = Utility.StringParse(Program);
                model.RegionID = Utility.StringParse(Region);
                model.AreaID = Utility.StringParse(Area);
                model.DistributorID = Utility.IntParse(Distributor);
                model.RouteID = Utility.StringParse(Route);
                model.OutletID = Utility.StringParse(Outlet);
                model.EvaluationID = Utility.StringParse(Evaluation);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.DataEvalImage = Global.VisibilityContext.usp_GetReportEvalImageBy(model.FromDate, model.ToDate, model.ProgramID, model.EvaluationID, model.RegionID, model.AreaID, model.DistributorID, model.RouteID, model.OutletID, model.SaleSupID, model.SalesmanID).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportEvalMarkImageExportPDF", null);

            return GridViewExtension.ExportToPdf(EvalMarkImageSettingExport(), model.DataEvalImage);
        }
        //
        private static GridViewSettings EvalMarkImageSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportEvalMarkImage";
            settings.KeyFieldName = "Program";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.Columns.Add(field =>
            {
                field.FieldName = "ProgramID";
                field.Caption = Utility.Phrase("TB_ProgramID");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "ProgramName";
                field.Caption = Utility.Phrase("TB_Program");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "EvaluationID";
                field.Caption = Utility.Phrase("TB_Evaluation");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionID";
                field.Caption = Utility.Phrase("Report_RegionID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaID";
                field.Caption = Utility.Phrase("Report_AreaID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorID";
                field.Caption = Utility.Phrase("Report_DistributorID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("Report_Distributor");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("Report_Route");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("TB_GSBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletName";
                field.Caption = Utility.Phrase("TB_Outlet");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Address";
                field.Caption = Utility.Phrase("TB_Address");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DateImage";
                field.Caption = Utility.Phrase("TB_Date");
                field.PropertiesEdit.DisplayFormatString = "yyyy-MM-dd";
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalImg";
                field.Caption = Utility.Phrase("TB_TotalImg");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgMarked";
                field.Caption = Utility.Phrase("TB_ImgMarked");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgPass";
                field.Caption = Utility.Phrase("TB_ImgPass");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgNotPass";
                field.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                field.UnboundExpression = "TotalImg - ImgPass";
                field.Caption = Utility.Phrase("TB_ImgNotPass");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgFake";
                field.Caption = Utility.Phrase("TB_ImgFake");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgNotCaptured";
                field.Caption = Utility.Phrase("TB_ImgNotAccepted");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgNotMark";
                field.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                field.UnboundExpression = "TotalImg - ImgMarked";
                field.Caption = Utility.Phrase("TB_ImgNotMark");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RateImgPass";
                field.Caption = Utility.Phrase("TB_RateImgPass");
                //field.ColumnType = MVCxGridViewColumnType.SpinEdit;
                //field.PropertiesEdit.DisplayFormatString = "p0";
                field.PropertiesEdit.DisplayFormatString = "{0:0.00}%";

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RateImgNotPass";
                field.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                field.UnboundExpression = "100 - RateImgPass";
                field.Caption = Utility.Phrase("TB_RateImgNotPass");
                field.PropertiesEdit.DisplayFormatString = "{0:0.00}%";
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RateImgNotMark";
                field.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                field.UnboundExpression = "100 - RateImgMark";
                field.Caption = Utility.Phrase("TB_RateImgNotMark");
                field.PropertiesEdit.DisplayFormatString = "{0:0.00}%";
            });

            return settings;
        } 
        #endregion

        #region ReportEvalReason
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_EvalReason", true)]
        public ActionResult ReportEvalReason()//
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvaluation();
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Report_EvalReason")]
        public ActionResult ReportEvalReason(string strFromDate, string strToDate, string Route, string Saleman, string Program, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvaluation(strFromDate, strToDate, Program, string.Empty, string.Empty, 0, Route, string.Empty, string.Empty, Saleman, Evaluation);
            List<ObjParamSP> listParam = new List<ObjParamSP>();
            listParam.Add(new ObjParamSP() { Key = "Role", Value = model.RoleView.ToString() });
            listParam.Add(new ObjParamSP() { Key = "FromDate", Value = model.FromDate });
            listParam.Add(new ObjParamSP() { Key = "ToDate", Value = model.ToDate });
            listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
            listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
            listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
            listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
            listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
            listParam.Add(new ObjParamSP() { Key = "Auditor", Value = model.Auditor });
            model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReportReasonBySS", listParam);

            return View(model);
        }

        private static GridViewSettings EvalReasonSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportEvalReason";
            settings.KeyFieldName = "SalesmanName";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.DataBound = (s, e) =>
            {
                ASPxGridView gridView = (ASPxGridView)s;
                foreach (GridViewColumn column in gridView.Columns)
                {
                    if (column is GridViewDataColumn)
                    {
                        GridViewDataColumn dataColumn = (GridViewDataColumn)column;
                        dataColumn.Caption = Utility.Phrase("TB_" + dataColumn.FieldName);
                    }
                }
            };

            return settings;
        }
        [ActionAuthorize("Report_EvalReason")]
        public ActionResult ReportEvalReasonExportExcel(string strFromDate, string strToDate, string Route, string Saleman, string Program, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            #region Validate and Get Data
            try
            {
                model.SetDataReportEvaluation(strFromDate, strToDate, string.Empty, string.Empty, string.Empty, 0, Route, Saleman, Program, Evaluation);
                List<ObjParamSP> listParam = new List<ObjParamSP>();
                listParam.Add(new ObjParamSP() { Key = "Role", Value = model.RoleView.ToString() });
                listParam.Add(new ObjParamSP() { Key = "FromDate", Value = model.FromDate });
                listParam.Add(new ObjParamSP() { Key = "ToDate", Value = model.ToDate });
                listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
                listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
                listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
                listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
                listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
                listParam.Add(new ObjParamSP() { Key = "Auditor", Value = model.Auditor });
                model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReportReasonBySS", listParam);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            ControllerHelper.LogUserAction("ReportTracking", "ReportEvalReasonExportExcel", null);

            return GridViewExtension.ExportToXlsx(EvalReasonSettingExport(), model.TableResult);
        }

        [ActionAuthorize("Report_EvalReason")]
        public ActionResult ReportEvalReasonExportPDF(string strFromDate, string strToDate, string Route, string Saleman, string Program, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            #region Validate and Get Data
            try
            {
                model.SetDataReportEvaluation(strFromDate, strToDate, string.Empty, string.Empty, string.Empty, 0, Route, Saleman, Program, Evaluation);
                List<ObjParamSP> listParam = new List<ObjParamSP>();
                listParam.Add(new ObjParamSP() { Key = "Role", Value = model.RoleView.ToString() });
                listParam.Add(new ObjParamSP() { Key = "FromDate", Value = model.FromDate });
                listParam.Add(new ObjParamSP() { Key = "ToDate", Value = model.ToDate });
                listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
                listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
                listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
                listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
                listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
                listParam.Add(new ObjParamSP() { Key = "Auditor", Value = model.Auditor });
                model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReportReasonBySS", listParam);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            ControllerHelper.LogUserAction("ReportTracking", "ReportEvalReasonExportPDF", null);

            return GridViewExtension.ExportToPdf(EvalReasonSettingExport(), model.TableResult);
        }
        #endregion

        #region ReportEvalReview
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_EvalReview", true)]
        public ActionResult ReportEvalReview()//
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvaluation();
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Report_EvalReview")]
        public ActionResult ReportEvalReview(string strFromDate, string strToDate, string Program, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvaluation(strFromDate, strToDate, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, Program, Evaluation);
            if (!string.IsNullOrEmpty(model.EvaluationID) || !string.IsNullOrEmpty(model.ProgramID) || model.FromDate != null)
            {
                List<ResultReportEvalVM> data = Global.VisibilityContext.usp_GetReportEvalReview(model.FromDate, model.FromDate, model.ProgramID, model.EvaluationID, "").Select(s =>
                    new ResultReportEvalVM()
                    {
                        ProgramName = s.ProgramName,
                        EvaluationID = s.EvaluationID,
                        EvalState = s.EvalState.Value,
                        TotalAuditor = s.TotalAuditor.Value,
                        TotalImg = s.TotalImg.Value,
                        EvalReviewRate = s.EvalReviewRate.Value,
                        ImgHasReview = s.ImgHasReview.Value,
                        ImgNotPassProgram = s.ImgNotPassProgram.Value,
                        ImgApproved = s.ImgApproved.Value,
                        ImgRejected = s.ImgRejected.Value,
                        ImgReMarking = s.ImgReMarking.Value

                    }).ToList();

                model.listResult = data;
            }
            return View(model);
        }
        #endregion

        #region ReportEvalInventory
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_EvalInventory", true)]
        public ActionResult ReportEvalInventory()//
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvaluation();
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Report_EvalInventory")]
        public ActionResult ReportEvalInventory(string strFromDate, string strToDate, string Route, string Saleman, string Program, string Evaluation, int GroupInventory, int GroupSaleteam = 0, string Region = "", string Area = "", int Distributor = 0)
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvaluation(strFromDate, strToDate, Program, Region, Area, Distributor, Route, string.Empty, string.Empty, Saleman, Evaluation, 0, GroupInventory, GroupSaleteam);
            List<ObjParamSP> listParam = new List<ObjParamSP>();
            listParam.Add(new ObjParamSP() { Key = "Role", Value = model.RoleView.ToString() });
            listParam.Add(new ObjParamSP() { Key = "GroupInventory", Value = model.GroupInventory });
            listParam.Add(new ObjParamSP() { Key = "GroupSaleteam", Value = model.GroupSaleteam });
            listParam.Add(new ObjParamSP() { Key = "FromDate", Value = model.FromDate });
            listParam.Add(new ObjParamSP() { Key = "ToDate", Value = model.ToDate });
            listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.RegionID });
            listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.AreaID });
            listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.DistributorID });
            listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
            listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
            listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
            listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
            listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
            listParam.Add(new ObjParamSP() { Key = "Auditor", Value = model.Auditor });
            model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReporInventoryBy", listParam);

            return View(model);
        }

        private static GridViewSettings EvalInventorySettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportEvalInventory";
            settings.KeyFieldName = "Program";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.DataBound = (s, e) =>
            {
                ASPxGridView gridView = (ASPxGridView)s;
                foreach (GridViewColumn column in gridView.Columns)
                {
                    if (column is GridViewDataColumn)
                    {
                        GridViewDataColumn dataColumn = (GridViewDataColumn)column;
                        dataColumn.Caption = Utility.Phrase(dataColumn.FieldName);
                    }
                }
            };

            return settings;
        }
        [ActionAuthorize("Report_EvalInventory")]
        public ActionResult ReportEvalInventoryExportExcel(string strFromDate, string strToDate, string Route, string Saleman, string Program, string Evaluation, int GroupInventory, int GroupSaleteam = 0, string Region = "", string Area = "", int Distributor = 0)
        {
            ReportEvalVM model = new ReportEvalVM();
            #region Validate and Get Data
            try
            {
                model.SetDataReportEvaluation(strFromDate, strToDate, Program, Region, Area, Distributor, Route, string.Empty, string.Empty, Saleman, Evaluation, 0, GroupInventory, GroupSaleteam);
                List<ObjParamSP> listParam = new List<ObjParamSP>();
                listParam.Add(new ObjParamSP() { Key = "Role", Value = model.RoleView.ToString() });
                listParam.Add(new ObjParamSP() { Key = "GroupInventory", Value = model.GroupInventory });
                listParam.Add(new ObjParamSP() { Key = "GroupSaleteam", Value = model.GroupSaleteam });
                listParam.Add(new ObjParamSP() { Key = "FromDate", Value = model.FromDate });
                listParam.Add(new ObjParamSP() { Key = "ToDate", Value = model.ToDate });
                listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.RegionID });
                listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.AreaID });
                listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.DistributorID });
                listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
                listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
                listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
                listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
                listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
                listParam.Add(new ObjParamSP() { Key = "Auditor", Value = model.Auditor });
                model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReporInventoryBy", listParam);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            ControllerHelper.LogUserAction("ReportTracking", "ReportEvalInventoryExportExcel", null);

            return GridViewExtension.ExportToXlsx(EvalInventorySettingExport(), model.TableResult);
        }

        [ActionAuthorize("Report_EvalInventory")]
        public ActionResult ReportEvalInventoryExportPDF(string strFromDate, string strToDate, string Route, string Saleman, string Program, string Evaluation, int GroupInventory, int GroupSaleteam = 0, string Region = "", string Area = "", int Distributor = 0)
        {
            ReportEvalVM model = new ReportEvalVM();
            #region Validate and Get Data
            try
            {
                model.SetDataReportEvaluation(strFromDate, strToDate, Program, Region, Area, Distributor, Route, string.Empty, string.Empty, Saleman, Evaluation, 0, GroupInventory, GroupSaleteam);
                List<ObjParamSP> listParam = new List<ObjParamSP>();
                listParam.Add(new ObjParamSP() { Key = "Role", Value = model.RoleView.ToString() });
                listParam.Add(new ObjParamSP() { Key = "GroupInventory", Value = model.GroupInventory });
                listParam.Add(new ObjParamSP() { Key = "GroupSaleteam", Value = model.GroupSaleteam });
                listParam.Add(new ObjParamSP() { Key = "FromDate", Value = model.FromDate });
                listParam.Add(new ObjParamSP() { Key = "ToDate", Value = model.ToDate });
                listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.RegionID });
                listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.AreaID });
                listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.DistributorID });
                listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
                listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
                listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
                listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
                listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
                listParam.Add(new ObjParamSP() { Key = "Auditor", Value = model.Auditor });
                model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReporInventoryBy", listParam);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            ControllerHelper.LogUserAction("ReportTracking", "ReportEvalInventoryExportPDF", null);

            return GridViewExtension.ExportToPdf(EvalInventorySettingExport(), model.TableResult);
        }
        #endregion

        #region ReportEvalDetailByImage
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_EvalDetailByImage", true)]
        public ActionResult ReportEvalDetailByImage()//
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvalDetailByImage();
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Report_EvalDetailByImage")]
        public ActionResult ReportEvalDetailByImage(string Program, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            model.SetDataReportEvalDetailByImage(Program, Evaluation);

            #region Set param
            List<ObjParamSP> listParam = new List<ObjParamSP>();
            listParam.Add(new ObjParamSP() { Key = "ImageFromDate", Value = model.ImageFromDate });
            listParam.Add(new ObjParamSP() { Key = "ImageToDate", Value = model.ImageToDate });
            listParam.Add(new ObjParamSP() { Key = "MarkFromDate", Value = model.MarkFromDate });
            listParam.Add(new ObjParamSP() { Key = "MarkToDate", Value = model.MarkToDate });
            listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
            listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
            listParam.Add(new ObjParamSP() { Key = "AuditorID", Value = model.Auditor });
            listParam.Add(new ObjParamSP() { Key = "LeaderID", Value = model.LeaderID });
            listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.RegionID });
            listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.AreaID });
            listParam.Add(new ObjParamSP() { Key = "ProvinceID", Value = model.ProvinceID });
            listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.DistributorID });
            listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
            listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
            listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
            listParam.Add(new ObjParamSP() { Key = "UserName", Value = SessionHelper.GetSession<string>("UserName") });
            #endregion

            #region GetData
            model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReportEvalDetailByImage", listParam); 
            #endregion

            return View(model);
        }

        private static GridViewSettings EvalDetailByImageSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportEvalDetailByImage";
            settings.KeyFieldName = "SalesmanName";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.DataBound = (s, e) =>
            {
                ASPxGridView gridView = (ASPxGridView)s;
                foreach (GridViewColumn column in gridView.Columns)
                {
                    if (column is GridViewDataColumn)
                    {
                        GridViewDataColumn dataColumn = (GridViewDataColumn)column;
                        dataColumn.Caption = Utility.Phrase("TB_" + dataColumn.FieldName);
                    }
                }
            };

            return settings;
        }
        [ActionAuthorize("Report_EvalDetailByImage")]
        public ActionResult ReportEvalDetailByImageExportExcel(string Program, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            #region Validate and Get Data
            try
            {
                model.SetDataReportEvalDetailByImage(Program, Evaluation);

                #region Set param
                List<ObjParamSP> listParam = new List<ObjParamSP>();
                listParam.Add(new ObjParamSP() { Key = "ImageFromDate", Value = model.ImageFromDate });
                listParam.Add(new ObjParamSP() { Key = "ImageToDate", Value = model.ImageToDate });
                listParam.Add(new ObjParamSP() { Key = "MarkFromDate", Value = model.MarkFromDate });
                listParam.Add(new ObjParamSP() { Key = "MarkToDate", Value = model.MarkToDate });
                listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
                listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
                listParam.Add(new ObjParamSP() { Key = "AuditorID", Value = model.Auditor });
                listParam.Add(new ObjParamSP() { Key = "LeaderID", Value = model.LeaderID });
                listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.RegionID });
                listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.AreaID });
                listParam.Add(new ObjParamSP() { Key = "ProvinceID", Value = model.ProvinceID });
                listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.DistributorID });
                listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
                listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
                listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
                listParam.Add(new ObjParamSP() { Key = "UserName", Value = SessionHelper.GetSession<string>("UserName") });
                #endregion

                #region GetData
                model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReportEvalDetailByImage", listParam);
                #endregion
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            ControllerHelper.LogUserAction("ReportTracking", "ReportEvalDetailByImageExportExcel", null);

            return GridViewExtension.ExportToXlsx(EvalDetailByImageSettingExport(), model.TableResult);
        }

        [ActionAuthorize("Report_EvalDetailByImage")]
        public ActionResult ReportEvalDetailByImageExportPDF(string Program, string Evaluation)
        {
            ReportEvalVM model = new ReportEvalVM();
            #region Validate and Get Data
            try
            {
                model.SetDataReportEvalDetailByImage(Program, Evaluation);

                #region Set param
                List<ObjParamSP> listParam = new List<ObjParamSP>();
                listParam.Add(new ObjParamSP() { Key = "ImageFromDate", Value = model.ImageFromDate });
                listParam.Add(new ObjParamSP() { Key = "ImageToDate", Value = model.ImageToDate });
                listParam.Add(new ObjParamSP() { Key = "MarkFromDate", Value = model.MarkFromDate });
                listParam.Add(new ObjParamSP() { Key = "MarkToDate", Value = model.MarkToDate });
                listParam.Add(new ObjParamSP() { Key = "ProgramID", Value = model.ProgramID });
                listParam.Add(new ObjParamSP() { Key = "EvaluationID", Value = model.EvaluationID });
                listParam.Add(new ObjParamSP() { Key = "AuditorID", Value = model.Auditor });
                listParam.Add(new ObjParamSP() { Key = "LeaderID", Value = model.LeaderID });
                listParam.Add(new ObjParamSP() { Key = "RegionID", Value = model.RegionID });
                listParam.Add(new ObjParamSP() { Key = "AreaID", Value = model.AreaID });
                listParam.Add(new ObjParamSP() { Key = "ProvinceID", Value = model.ProvinceID });
                listParam.Add(new ObjParamSP() { Key = "DistributorID", Value = model.DistributorID });
                listParam.Add(new ObjParamSP() { Key = "SaleSupID", Value = model.SaleSupID });
                listParam.Add(new ObjParamSP() { Key = "RouteID", Value = model.RouteID });
                listParam.Add(new ObjParamSP() { Key = "SalesmanID", Value = model.SalesmanID });
                listParam.Add(new ObjParamSP() { Key = "UserName", Value = SessionHelper.GetSession<string>("UserName") });
                #endregion

                #region GetData
                model.TableResult = ControllerHelper.QueryStoredProcedure("usp_GetReportEvalDetailByImage", listParam);
                #endregion
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            ControllerHelper.LogUserAction("ReportTracking", "ReportEvalDetailByImageExportPDF", null);

            return GridViewExtension.ExportToPdf(EvalDetailByImageSettingExport(), model.TableResult);
        }
        #endregion
        #endregion

        #region Report drugstores need approval
        [ActionAuthorize("Report_DrugstoresNeedApproval", true)]
        public ActionResult ReportDrugstores ()
        {
            MV_ReportDrugstores model = new MV_ReportDrugstores();
            SetDataComboboxDrugstores(model);
            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Report_DrugstoresNeedApproval")]
        public ActionResult ReportDrugstores(string Region, string Area, string Distributor, string Route)
        {
            MV_ReportDrugstores model = new MV_ReportDrugstores();
            #region Validate and Get Data
            try
            {
                    model.areaID = Utility.StringParse(Area);
                    model.distributorID = Utility.IntParse(Distributor);
                    model.regionID = Utility.StringParse(Region);
                    model.routeID = Utility.StringParse(Route);

                    model.ReportDugstoresResult = Global.Context.pp_ReportDrugstores(model.regionID, model.areaID, model.distributorID, model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();
                    SessionHelper.SetSession<MV_ReportDrugstores>("MV_ReportDrugstores", model);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            SetDataComboboxDrugstores(model);
            return View(model);
        }

        [ActionAuthorize("Tracking_ReportDrugstoresExportExecl")]
        public ActionResult ReportDrugstoresExportExecl(string strFromDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportDrugstores model = new MV_ReportDrugstores();
            #region Validate and Get Data
            try
            {
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportDugstoresResult = Global.Context.pp_ReportDrugstores(model.regionID, model.areaID, model.distributorID, model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportDrugstoresExportExecl", null);

            return GridViewExtension.ExportToXlsx(ReportDrugstoresSettingExport(), model.ReportDugstoresResult);
        }

        [ActionAuthorize("Tracking_ReportDrugstoresExportPDF")]
        public ActionResult ReportDrugstoresExportPDF(string strFromDate, string Region, string Area, string Distributor, string Route)
        {
            MV_ReportDrugstores model = new MV_ReportDrugstores();
            #region Validate and Get Data
            try
            {
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportDugstoresResult = Global.Context.pp_ReportDrugstores(model.regionID, model.areaID, model.distributorID, model.routeID, SessionHelper.GetSession<string>("UserName")).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportDrugstoresExportPDF", null);

            return GridViewExtension.ExportToPdf(ReportDrugstoresSettingExport(), model.ReportDugstoresResult);
        }

        private static GridViewSettings ReportDrugstoresSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportDrugstores";
            settings.KeyFieldName = "VisitDate";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionID";
                field.Caption = Utility.Phrase("Report_RegionID");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("Report_Region");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaID";
                field.Caption = Utility.Phrase("Report_AreaID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("Report_Area");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorID";
                field.Caption = Utility.Phrase("Report_DistributorID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("Report_Distributor");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupID";
                field.Caption = Utility.Phrase("TB_GSBHID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("TB_GSBH");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("Report_RouteID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("Report_Route");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("TB_NVBHID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("TB_NVBH");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstCaptureDate";
                field.Caption = Utility.Phrase("TB_DateWork");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "CustomerCD";
                field.Caption = Utility.Phrase("TB_CustomerCD");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "CustomerName";
                field.Caption = Utility.Phrase("TB_CustomerName");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "Address";
                field.Caption = Utility.Phrase("TB_Address");
            });
           
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstAvatar";
                field.ColumnType = MVCxGridViewColumnType.BinaryImage;
                BinaryImageEditProperties properties = (BinaryImageEditProperties)field.PropertiesEdit;
                properties.ImageWidth = 120;
                properties.ImageHeight = 80;
                properties.ExportImageSettings.Width = 90;
                properties.ExportImageSettings.Height = 60;
                field.Caption = Utility.Phrase("TB_FirstAvartar");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstCaptureDate";
                field.Caption = Utility.Phrase("TB_FirstCaptureDate");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LastAvatar";
                field.Caption = Utility.Phrase("TB_LastAvartar");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "LastCaptureDate";
                field.Caption = Utility.Phrase("TB_LastCaptureDate");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "Distance";
                field.Caption = Utility.Phrase("TB_Distance");
            });

            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstLatitude";
                field.Caption = Utility.Phrase("TB_FirstLatitude");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstLongtitude";
                field.Caption = Utility.Phrase("TB_FirstLongtitude");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LastLatitude";
                field.Caption = Utility.Phrase("TB_LastLatitude");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LastLongtitude";
                field.Caption = Utility.Phrase("TB_LastLongtitude");
            });
           

            return settings;
        }
        #endregion

        #region Report Issues
        [ActionAuthorize("Report_ReportIssues", true)]
        public ActionResult ReportIssues()
        {
            MV_ReportIssues model = new MV_ReportIssues();
            if (SessionHelper.GetSession<MV_ReportIssues>("ReportIssues") != null)
            {
                model = SessionHelper.GetSession<MV_ReportIssues>("ReportIssues");
            }
            else
            {
                SetDataComboboxIssues(model);
            }
            model.ReportIssuesResult = Global.Context.pp_ReportIssues(model.FromDate, model.ToDate, model.regionID, model.areaID, model.routeID, SessionHelper.GetSession<string>("UserName"), model.StatusID).ToList();
            return View(model);
        }
        [HttpPost]
        [ActionAuthorize("Report_ReportIssues")]
        public ActionResult ReportIssues(string strFromDate, string strToDate, string Region, string Area, string Status)
        {
            MV_ReportIssues model = new MV_ReportIssues();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.regionID = Utility.StringParse(Region);
                model.StatusID = Utility.IntParse(Status);

                //Them dong sync data PDA -> Eroute
                //Global.Context.pp_SyncIssues();

                model.ReportIssuesResult = Global.Context.pp_ReportIssues(model.FromDate, model.ToDate,model.regionID, model.areaID, model.routeID, SessionHelper.GetSession<string>("UserName"), model.StatusID).ToList();
                SessionHelper.SetSession<MV_ReportIssues>("ReportIssues", model);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            SetDataComboboxIssues(model);
            return View(model);
        }

        [Authorize]
        [ActionAuthorize("Report_IssuesDetailConfirm")]
        public ActionResult ReportIssuesDetailConfirm(string VisitDate, string IssuesID, string SalesmanCode, string OutletID)
        {
            MV_ReportIssues model = new MV_ReportIssues();
            #region Validate and Get Data
            try
            {

                if (SessionHelper.GetSession<MV_ReportIssues>("ReportIssues") != null)
                {
                    model = SessionHelper.GetSession<MV_ReportIssues>("ReportIssues");
                }
                model.IssuesID = Utility.StringParse(IssuesID);
                model.SalesmanCode = Utility.StringParse(SalesmanCode);
                model.OutletID = Utility.StringParse(OutletID);
                model.VisitDate = Utility.DateTimeParse(VisitDate);

                model.ReportIssuesDetailResult = Global.Context.pp_ReportIssuesDetail(model.VisitDate,model.IssuesID, model.SalesmanCode, model.OutletID).ToList();
                model.ReportIssuesDetailConfirmResult = Global.Context.pp_ReportIssuesDetailConfirm(model.VisitDate, model.IssuesID, model.SalesmanCode, model.OutletID).ToList();
                SessionHelper.SetSession<MV_ReportIssues>("ReportIssuesDetailConfirm", model);
                
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            
            return View(model);
        }

        [HttpPost]
        //public ActionResult AddReportIssues(string IssuesID, string SalesmanCode, string OutletID, string DistributorCode, string VisitDate, string Content, string Status, string Resolve)
        public ActionResult ReportIssuesDetailConfirm(MV_ReportIssues model)
        {
            int? result = 0;
            try
            {
                var modelIssues = model.ReportIssuesDetailResult.FirstOrDefault();
                if (modelIssues.IssueID != null && modelIssues.OutletID != null && modelIssues.SalesmanCode != null && model.DistributorCode != null && model.ReportIssuesDetailResult.Count > 0)
                {

                    var issues = Global.Context.E_Issues.Where(x => x.IssueID == modelIssues.IssueID && x.OutletID == modelIssues.OutletID && x.SalesmanCode == modelIssues.SalesmanCode && x.DistributorCode == model.DistributorCode).FirstOrDefault();
                    var user = Global.Context.UserProfileInfos.Where(x => x.LoginID == SessionHelper.GetSession<string>("UserName")).FirstOrDefault();
                    string strdate = String.Format("{0:dd/MM/yyyy}", DateTime.Now.Date);
                    issues.UpdateDate = DateTime.Now;
                    if (model.Resolve == "" || model.Resolve == null)
                    {
                        issues.Resolve = strdate + ":" + model.NewResolve;
                    }
                    else
                    {
                        issues.Resolve = model.Resolve + "\n" + strdate + ":" + model.NewResolve;
                    }
                    issues.Status = 112;
                    issues.Pic = user.FullName;
                    Global.Context.SubmitChanges();
                    if(modelIssues.Status != 111)
                    {
                        if(model.ReportIssuesDetailResult != null)
                        {
                            foreach (var im in model.ReportIssuesDetailResult)
                            {
                                if(im.ImageNameIssues != null)
                                {
                                    var issuesImage = Global.Context.E_IssueImages.Where(x => x.IssueID == modelIssues.IssueID && x.ImageName == im.ImageNameIssues).FirstOrDefault();
                                    issuesImage.ImageComment = strdate + ":" + im.ImageComment;
                                }
                            }
                            Global.Context.SubmitChanges();
                        }
                    }
                    else
                    {
                        if(model.ReportIssuesDetailConfirmResult != null)
                        {
                            foreach (var imc in model.ReportIssuesDetailConfirmResult)
                            {
                                if(imc.ImageNameConfirm != null)
                                {
                                    var issuesImage = Global.Context.E_IssueConfirmationImages.Where(x => x.IssueID == modelIssues.IssueID && x.ImageName == imc.ImageNameConfirm).FirstOrDefault();
                                    issuesImage.ImageComment = strdate + ":" + imc.ImageCommentConfirm;
                                }
                                
                            }
                            Global.Context.SubmitChanges();
                        }
                    }
                    model.ReportIssuesDetailResult = Global.Context.pp_ReportIssuesDetail(modelIssues.VisitDate, modelIssues.IssueID, model.SalesmanCode, model.OutletID).ToList();
                    model.ReportIssuesDetailConfirmResult = Global.Context.pp_ReportIssuesDetailConfirm(modelIssues.VisitDate, modelIssues.IssueID, model.SalesmanCode, model.OutletID).ToList();
                    //Them dong Sync data Eroute -> PDA
                    Global.Context.pp_SyncIssuesToPDA(modelIssues.IssueID, ref result);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            return View(model);
        }

        [ActionAuthorize("Tracking_ReportIssuesExportExecl")]
        public ActionResult ReportIssuesExportExecl(string strFromDate, string strToDate, string Region, string Area, string Distributor, string Route, string Status)
        {
            MV_ReportIssues model = new MV_ReportIssues();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
                model.StatusID = Utility.IntParse(Status);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportIssuesResult = Global.Context.pp_ReportIssues(model.FromDate, model.ToDate, model.regionID, model.areaID, model.routeID, SessionHelper.GetSession<string>("UserName"), model.StatusID).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportIssuesExportExecl", null);

            return GridViewExtension.ExportToXlsx(ReportIssuesSettingExport(), model.ReportIssuesResult);
        }

        [ActionAuthorize("Tracking_ReportIssuesExportPDF")]
        public ActionResult ReportIssuesExportPDF(string strFromDate, string strToDate, string Region, string Area, string Distributor, string Route, string Status)
        {
            MV_ReportIssues model = new MV_ReportIssues();
            #region Validate and Get Data
            try
            {
                model.FromDate = Utility.DateTimeParse(strFromDate);
                model.ToDate = Utility.DateTimeParse(strToDate);
                model.areaID = Utility.StringParse(Area);
                model.distributorID = Utility.IntParse(Distributor);
                model.regionID = Utility.StringParse(Region);
                model.routeID = Utility.StringParse(Route);
                model.StatusID = Utility.IntParse(Status);

            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion
            model.ReportIssuesResult = Global.Context.pp_ReportIssues(model.FromDate, model.ToDate, model.regionID, model.areaID, model.routeID, SessionHelper.GetSession<string>("UserName"), model.StatusID).ToList();

            ControllerHelper.LogUserAction("ReportTracking", "ReportIssuesExportPDF", null);

            return GridViewExtension.ExportToPdf(ReportIssuesSettingExport(), model.ReportIssuesResult);
        }

        private static GridViewSettings ReportIssuesSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportIssues";
            settings.KeyFieldName = "VisitDate";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "RegionID";
            //    field.Caption = Utility.Phrase("Report_RegionID");

            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "RegionName";
            //    field.Caption = Utility.Phrase("Report_Region");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "AreaID";
            //    field.Caption = Utility.Phrase("Report_AreaID");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "AreaName";
            //    field.Caption = Utility.Phrase("Report_Area");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "DistributorID";
            //    field.Caption = Utility.Phrase("Report_DistributorID");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "DistributorName";
            //    field.Caption = Utility.Phrase("Report_Distributor");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "SaleSupID";
            //    field.Caption = Utility.Phrase("TB_GSBHID");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "SaleSupName";
            //    field.Caption = Utility.Phrase("TB_GSBH");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "RouteID";
            //    field.Caption = Utility.Phrase("Report_RouteID");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "RouteName";
            //    field.Caption = Utility.Phrase("Report_Route");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "SalesmanID";
            //    field.Caption = Utility.Phrase("TB_NVBHID");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "SalesmanName";
            //    field.Caption = Utility.Phrase("TB_NVBH");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstCaptureDate";
            //    field.Caption = Utility.Phrase("TB_DateWork");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "CustomerCD";
            //    field.Caption = Utility.Phrase("TB_CustomerCD");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "CustomerName";
            //    field.Caption = Utility.Phrase("TB_CustomerName");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "Address";
            //    field.Caption = Utility.Phrase("TB_Address");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstAvatar";
            //    field.ColumnType = MVCxGridViewColumnType.BinaryImage;
            //    BinaryImageEditProperties properties = (BinaryImageEditProperties)field.PropertiesEdit;
            //    properties.ImageWidth = 120;
            //    properties.ImageHeight = 80;
            //    properties.ExportImageSettings.Width = 90;
            //    properties.ExportImageSettings.Height = 60;
            //    field.Caption = Utility.Phrase("TB_FirstAvartar");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstCaptureDate";
            //    field.Caption = Utility.Phrase("TB_FirstCaptureDate");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LastAvatar";
            //    field.Caption = Utility.Phrase("TB_LastAvartar");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LastCaptureDate";
            //    field.Caption = Utility.Phrase("TB_LastCaptureDate");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "Distance";
            //    field.Caption = Utility.Phrase("TB_Distance");
            //});

            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstLatitude";
            //    field.Caption = Utility.Phrase("TB_FirstLatitude");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "FirstLongtitude";
            //    field.Caption = Utility.Phrase("TB_FirstLongtitude");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LastLatitude";
            //    field.Caption = Utility.Phrase("TB_LastLatitude");
            //});
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LastLongtitude";
            //    field.Caption = Utility.Phrase("TB_LastLongtitude");
            //});


            return settings;
        }
        #endregion

        #region Review Order

        [Authorize]
        [ActionAuthorize("Report_ReviewOrderManagement", true)]
        public ActionResult ReviewOrderManagement(MV_ReviewOrder model)
        {
            var startDate = !string.IsNullOrEmpty(model.StartDate) ? Utility.DateTimeParse(model.StartDate) : DateTime.Today;
            var endDate = !string.IsNullOrEmpty(model.EndDate) ? Utility.DateTimeParse(model.EndDate) : DateTime.Today;
            model.SalesmanID = (model.SalesmanID == null) ? string.Empty : model.SalesmanID;

            List<pp_ReportGetListReviewOrderResult> listOrder = Global.Context.pp_ReportGetListReviewOrder(string.Empty, startDate, endDate, model.Status, model.SalesmanID).ToList();

            ViewControlCombobox ctrSaleMan = new ViewControlCombobox();
            ctrSaleMan.TitleKey = Utility.Phrase("SaleManID");
            ctrSaleMan.TitleName = Utility.Phrase("SaleManName");
            ctrSaleMan.SeleteID = string.Empty;
            ctrSaleMan.listOption = ControllerHelper.GetListSalesman(string.Empty, string.Empty, string.Empty, 0, string.Empty).Select(s => new OptionCombobox { ID = s.SalesmanID, Key = s.SalesmanID, Value = s.SalesmanName }).ToList();

            MV_ReviewOrder result = new MV_ReviewOrder
            {
                StartDate = startDate.ToShortPattern(),
                EndDate = endDate.ToShortPattern(),
                Status = model.Status,
                SalesmanID = model.SalesmanID,
                ReportGetListReviewOrder = listOrder,
                ListComboboxSaleMan = ctrSaleMan
            };
            return View(result);
        }

        [Authorize]
        [ActionAuthorize("Report_ReviewOrderManagement", true)]
        public ActionResult ReviewOrderDetail(string OrderID = "")
        {
            if (string.IsNullOrEmpty(OrderID))
            {
                return RedirectToAction("ReviewOrderManagement");
            }

            pp_ReportGetListReviewOrderResult item = Global.Context.pp_ReportGetListReviewOrder(OrderID, (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue, 2, string.Empty).FirstOrDefault();
            List<OrderDetail> lstOrder = Global.Context.OrderDetails.Where(t => t.OrderCode == OrderID).ToList();
            MV_ReviewOrderDetail result = new MV_ReviewOrderDetail
            {
                OderDetailInfor = (item == null) ? new pp_ReportGetListReviewOrderResult() : item,
                ListOrder = lstOrder
            };
            return View(result);
        }

        [HttpPost]
        [ActionAuthorize("Report_ReviewOrderManagement")]
        public ActionResult FunctionOrder(int func, string dataJson)
        {
            //func = 1: Duyet giao hang, 2: Duyet khong giao, 3: Gui mail, 4: Mo lai don hang duyet giao hang
            ResponseMessage result = new ResponseMessage();
            List<MV_KeyOrderHeader> lstOrderHeader = new JavaScriptSerializer().Deserialize<List<MV_KeyOrderHeader>>(dataJson);
            OrderHeader order = new OrderHeader();

            var user = Global.Context.UserProfileInfos.Where(x => x.LoginID == SessionHelper.GetSession<string>("UserName")).FirstOrDefault();
            switch (func)
            {
                case 1: //Duyet giao hang
                    foreach (var item in lstOrderHeader)
                    {
                        order = Global.Context.OrderHeaders.Where(t => t.DistributorID == item.DistributorId && t.Code == item.Code
                        && t.OutletID == item.OutletId && t.SalesmanID == item.SalemanId && t.VisitDate.Date == Utility.DateTimeParse(item.VisitDate)).FirstOrDefault();
                        order.DeliveryStatus = 1;
                        order.PersonApprove = user.FullName;
                        order.TimeApprove = DateTime.Now;
                    }
                    Global.Context.SubmitChanges();
                    result = new ResponseMessage() { ID = 1, Message = Utility.Phrase("SuccessfullyUpdated") };

                    break;
                case 2: //Duyet khong giao
                    foreach (var item in lstOrderHeader)
                    {
                        order = Global.Context.OrderHeaders.Where(t => t.DistributorID == item.DistributorId && t.Code == item.Code
                        && t.OutletID == item.OutletId && t.SalesmanID == item.SalemanId && t.VisitDate.Date == Utility.DateTimeParse(item.VisitDate)).FirstOrDefault();
                        order.DeliveryStatus = -1;
                        order.PersonApprove = user.FullName;
                        order.TimeApprove = DateTime.Now;
                    }
                    Global.Context.SubmitChanges();
                    result = new ResponseMessage() { ID = 1, Message = Utility.Phrase("SuccessfullyUpdated") };

                    break;
                case 3: //Gui mail
                    List<string> lstSendMail = Global.Context.CustomSettings.Where(t => t.SettingCode == "EmailList").Select(t => t.SettingValue).FirstOrDefault().Split(';').ToList();
                    string mergePdfName = "";
                    string contentEmail = HttpContext.Server.MapPath(Constant.EmailDistributorHTML);
                    string bodyEmail = System.IO.File.ReadAllText(contentEmail);

                    // gom nhom nha phan phoi
                    Dictionary<int, List<MV_KeyOrderHeader>> groupDistributor = new Dictionary<int, List<MV_KeyOrderHeader>>();
                    List<int> lstDistributorId = lstOrderHeader.Select(t => t.DistributorId).ToList();
                    List<int> lstDistributorIdNotOverlap = lstDistributorId.Union(lstDistributorId).ToList();
                    for (int i = 0; i < lstDistributorIdNotOverlap.Count; i++)
                    {
                        var newDist = new List<MV_KeyOrderHeader>();
                        foreach (var item in lstOrderHeader)
                        {
                            if (lstDistributorIdNotOverlap[i] == item.DistributorId)
                            {
                                newDist.Add(item);
                            }
                        }
                        groupDistributor.Add(i, newDist);
                    }

                    //send mail cho tung nha phan phoi rieng
                    foreach (var distributor in groupDistributor)
                    {
                        PdfDocument outPdf = new PdfDocument();
                        foreach (var item in distributor.Value)
                        {
                            mergePdfName = item.DistributorId + DateTime.Now.ToString("hhmmssfff") + ".pdf";
                            string HTMLFile = HttpContext.Server.MapPath(Constant.EmailDeliveryFormHTML);
                            string HTMLBody = System.IO.File.ReadAllText(HTMLFile);
                            string sourceFile = HttpContext.Server.MapPath(Constant.PDFSourceFile);

                            if (!System.IO.File.Exists(sourceFile))
                            {
                                result = new ResponseMessage { ID = -1, Message = Utility.Phrase("CanNotSendEmail") };
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }

                            // get data and set data to HTML
                            var OverviewInfo = Global.Context.pp_ReportOverviewDeliveryForm(item.Code).FirstOrDefault();
                            var lstOrder = Global.Context.OrderDetails.Where(t => t.OrderCode == item.Code).ToList();
                            var lstPromotion = Global.Context.pp_ReportPromotionDeliveryForm(item.Code).ToList();


                            if (!string.IsNullOrEmpty(OverviewInfo.DistributorEmail))
                            {
                                lstSendMail.Add(OverviewInfo.DistributorEmail);
                            }

                            double totalDiscountAmt = 0;
                            double totalLineAmt = 0;
                            string rowOrder = "";
                            string promotionZone = "";

                            string rowOrderHTML = "<tr style='text-align: center'>" +
                                "<td style='border: 1px solid'>{OrderNo}</td>" +
                                "<td style='border: 1px solid'>{OrderCode}</td>" +
                                "<td style='border: 1px solid; text-align: left;'>{OrderName}</td>" +
                                "<td style='border: 1px solid'>{OrderSaleUnit}</td>" +
                                "<td style='border: 1px solid'>{OrderQty}</td>" +
                                "<td style='border: 1px solid;text-align: right;'>{OrderUnitPrice}</td>" +
                                "<td style='border: 1px solid;text-align: right;'>{OrderPromo}</td>" +
                                "<td style='border: 1px solid;text-align: right;'>{OrderCash}</td></tr>";
                            for (int i = 0; i < lstOrder.Count; i++)
                            {
                                rowOrder += rowOrderHTML;
                                rowOrder = rowOrder.Replace("{OrderNo}", (i + 1).ToString());
                                rowOrder = rowOrder.Replace("{OrderCode}", lstOrder[i].InventoryCD);
                                rowOrder = rowOrder.Replace("{OrderName}", lstOrder[i].InventoryName);
                                rowOrder = rowOrder.Replace("{OrderSaleUnit}", lstOrder[i].SalesUnit);
                                rowOrder = rowOrder.Replace("{OrderQty}", lstOrder[i].OrderQty.ToString());
                                rowOrder = rowOrder.Replace("{OrderUnitPrice}", Utility.StringParseWithDecimalDegit(lstOrder[i].UnitPrice));
                                rowOrder = rowOrder.Replace("{OrderPromo}", Utility.StringParseWithDecimalDegit(lstOrder[i].DiscountAmt));
                                rowOrder = rowOrder.Replace("{OrderCash}", Utility.StringParseWithDecimalDegit(lstOrder[i].LineAmt.HasValue ? lstOrder[i].LineAmt.Value : 0));
                                totalDiscountAmt += lstOrder[i].DiscountAmt.Value;
                                totalLineAmt += lstOrder[i].LineAmt.Value;
                            }

                            if (lstPromotion != null && lstPromotion.Count > 0)
                            {
                                string rowProHTML = "<tr style='text-align: center'>" +
                                "<td style='border: 1px solid'>{PromoNo}</td>" +
                                "<td style='border: 1px solid'>{PromoCode}</td>" +
                                "<td style='border: 1px solid; text-align: left;'>{InventoryName}</td>" +
                                "<td style='border: 1px solid'>{PromoSaleUnit}</td>" +
                                "<td style='border: 1px solid'>{PromoQty}</td>" +
                                "<td style='border: 1px solid'>{PromoName}</td></tr>";
                                string rowPro = "";
                                for (int i = 0; i < lstPromotion.Count; i++)
                                {
                                    rowPro += rowProHTML;
                                    rowPro = rowPro.Replace("{PromoNo}", (i + 1).ToString());
                                    rowPro = rowPro.Replace("{PromoCode}", lstPromotion[i].InventoryCD);
                                    rowPro = rowPro.Replace("{InventoryName}", lstPromotion[i].InventoryName);
                                    rowPro = rowPro.Replace("{PromoSaleUnit}", lstPromotion[i].SalesUnit);
                                    rowPro = rowPro.Replace("{PromoQty}", lstPromotion[i].OrderQty.ToString());
                                    rowPro = rowPro.Replace("{PromoName}", lstPromotion[i].PromptionName);
                                }
                                promotionZone = "<b>Khuyến mãi:</b>" +
                                    "<table width='100%' style='border-collapse: collapse;'>" +
                                        "<thead>" +
                                            "<tr>" +
                                                "<th style='border: 1px solid;width: 50px;'>STT</th>" +
                                                "<th style='border: 1px solid;width: 100px;'>Mã Hàng</th>" +
                                                "<th style='border: 1px solid;'>Tên Hàng</th>" +
                                                "<th style='border: 1px solid;width: 70px;'>ĐVT</th>" +
                                                "<th style='border: 1px solid;width: 90px;'>Số Lượng</th>" +
                                                "<th style='border: 1px solid;width: 220px;'>Tên CTKM</th>" +
                                            "</tr>" +
                                        "</thead>" +
                                        "<tbody>" +
                                            "{rowPro}" +
                                        "</tbody>" +
                                    "</table>";
                                promotionZone = promotionZone.Replace("{rowPro}", rowPro);
                            }

                            HTMLBody = HTMLBody.Replace("{DistributorName}", OverviewInfo.DistributorName);
                            HTMLBody = HTMLBody.Replace("{DistributorAddr}", OverviewInfo.DistributorAddress);
                            HTMLBody = HTMLBody.Replace("{Phone}", OverviewInfo.DistributorPhone);
                            HTMLBody = HTMLBody.Replace("{TimeApprove}", OverviewInfo.TimeApprove.ToShortPattern());
                            HTMLBody = HTMLBody.Replace("{Code}", item.Code);
                            HTMLBody = HTMLBody.Replace("{CustomerInfor}", OverviewInfo.OutletID + " - " + OverviewInfo.OutletName);
                            HTMLBody = HTMLBody.Replace("{CustomerAddr}", OverviewInfo.OutletAddress);
                            HTMLBody = HTMLBody.Replace("{VisitDate}", OverviewInfo.VisitDate.ToShortPattern());
                            HTMLBody = HTMLBody.Replace("{SalesmanName}", OverviewInfo.SalesmanName);
                            HTMLBody = HTMLBody.Replace("{rowOrder}", rowOrder);
                            HTMLBody = HTMLBody.Replace("{PromotionZone}", promotionZone);
                            HTMLBody = HTMLBody.Replace("{TotalLineAmt}", Utility.StringParseWithDecimalDegit(totalLineAmt));
                            HTMLBody = HTMLBody.Replace("{TotalDiscountAmt}", Utility.StringParseWithDecimalDegit(totalDiscountAmt));
                            HTMLBody = HTMLBody.Replace("{TotalPayment}", Utility.StringParseWithDecimalDegit(totalLineAmt - totalDiscountAmt));
                            HTMLBody = HTMLBody.Replace("{Note}", OverviewInfo.Note);
                            HTMLBody = HTMLBody.Replace("{PaymentString}", Utility.ConvertNumberToVietnamese((totalLineAmt - totalDiscountAmt).ToString()));

                            // create pdf file
                            var newFileName = DateTime.Now.ToString("ddMMyyyyhhmmssfff") + ".pdf";
                            string destFile = System.IO.Path.Combine(HttpContext.Server.MapPath("~/Content/FormReport/"), newFileName);
                            System.IO.File.Copy(sourceFile, destFile, true);
                            var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                            htmlToPdf.Orientation = NReco.PdfGenerator.PageOrientation.Landscape;
                            htmlToPdf.GeneratePdf(HTMLBody, null, destFile);
                            // merge file pdf
                            CopyPages(PdfReader.Open(destFile, PdfDocumentOpenMode.Import), outPdf);

                            order = Global.Context.OrderHeaders.Where(t => t.DistributorID == item.DistributorId && t.Code == item.Code
                            && t.OutletID == item.OutletId && t.SalesmanID == item.SalemanId && t.VisitDate.Date == Utility.DateTimeParse(item.VisitDate) && t.DeliveryStatus == 1).FirstOrDefault();
                            order.SenderSendMail = user.FullName;
                            order.TimeSendMail = DateTime.Now;
                        }

                        string mergePdfPath = System.IO.Path.Combine(HttpContext.Server.MapPath("~/Content/FormReport/"), mergePdfName);
                        outPdf.Save(mergePdfPath);

                        // send email
                        var lstSendMailNotOverlap = lstSendMail.Union(lstSendMail).ToList();
                        if (lstSendMailNotOverlap != null && lstSendMailNotOverlap.Count > 0)
                        {
                            SendMailAttachment(lstSendMailNotOverlap, Utility.Phrase("DeliveryForm"), bodyEmail, mergePdfPath);
                        }
                        Global.Context.SubmitChanges();
                        result = new ResponseMessage() { ID = 1, Message = Utility.Phrase("SuccessfullySendMail") };
                    }
                    break;
                case 4: //Mo lai don hang duyet giao hang
                    foreach (var item in lstOrderHeader)
                    {
                        order = Global.Context.OrderHeaders.Where(t => t.DistributorID == item.DistributorId && t.Code == item.Code
                        && t.OutletID == item.OutletId && t.SalesmanID == item.SalemanId && t.VisitDate.Date == Utility.DateTimeParse(item.VisitDate) && t.DeliveryStatus == -1).FirstOrDefault();
                        order.DeliveryStatus = 1;
                        order.PersonApprove = user.FullName;
                        order.TimeApprove = DateTime.Now;
                    }
                    Global.Context.SubmitChanges();
                    result = new ResponseMessage() { ID = 1, Message = Utility.Phrase("SuccessfullyUpdated") };

                    break;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private void SendMailAttachment(List<string> toMail, string subject, string body, string pathFile)
        {
            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Host = Constant.Host;
            client.Port = Constant.Port;

            ServicePointManager.ServerCertificateValidationCallback = OurCertificateValidation;

            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(Constant.FromEmail, Constant.Password);
            client.UseDefaultCredentials = false;
            client.Credentials = credentials;

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(Constant.FromEmail, Constant.DisplayName);
            foreach (var item in toMail)
            {
                msg.To.Add(new MailAddress(item));
            }

            Attachment attachment;
            attachment = new Attachment(pathFile);
            msg.Attachments.Add(attachment);

            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = body;

            client.Send(msg);
        }

        private static bool OurCertificateValidation(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }

        #endregion
    }
}
