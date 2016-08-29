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
using DevExpress.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.Web.Script.Serialization;


namespace eRoute.Controllers
{
    #region Controller
    [Authorize]
    [InitializeSimpleMembership]
    [LogAndRedirectOnError]
    public class TrackingController : Controller
    {
        string username = SessionHelper.GetSession<string>("UserName");

        #region Index
        [ActionAuthorize("Tracking_Index", true)]
        [CompressFilter]
        public ActionResult Index()
        {
            var model = new HomeVM { listDis = ControllerHelper.ListDistributor.OrderBy(a => a.DistributorName).ToList() };

            //if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            //{
            //    model.listSaleman = new List<Salesman>();
            //    model.listSaleman.AddRange(ControllerHelper.ListSalesman);
            //}
            //else
            //{
            //    model.listSaleman = new List<Salesman>();
            //}

            model.VisitDate = DateTime.Now;
            model.strDate = DateTime.Now.ToString(Constant.ShortDatePattern);
            model.strHour = DateTime.Now.ToString(Constant.ShortTimePattern);
            model.listDis = ControllerHelper.ListDistributor.OrderBy(a => a.DistributorName).ToList();

            model.ListRegion = ControllerHelper.GetListRegion(model.regionID);
            model.ListArea = ControllerHelper.GetListArea(model.regionID);

            return View(model);
        }

        public ActionResult ComboBoxPartial()
        {
            return PartialView(ControllerHelper.ListDistributor);
        }
        #endregion

        #region MovementMonitoring
        [ActionAuthorize("Home_MovementMonitoring")]
        [CompressFilter]
        public ActionResult MovementMonitoring()
        {
            var username = SessionHelper.GetSession<string>("UserName");
            var model = new HomeVM();

            //if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            //{
            //    model.listSaleman = new List<Map_Salesman>();
            //    model.listSaleman.AddRange(ControllerHelper.ListSalesman);
            //}
            //else
            //{
            //    model.listSaleman = new List<Salesman>();
            //}

            model.VisitDate = DateTime.Now;
            model.strDate = DateTime.Now.ToString(Constant.ShortDatePattern);
            model.strHour = DateTime.Now.ToString(Constant.ShortTimePattern);

            model.listASM = ControllerHelper.ListASM;
            model.listSS = ControllerHelper.GetListSaleSup(string.Empty);
            model.listDis = ControllerHelper.GetListDistributor(string.Empty, string.Empty);

            return View(model);
        }
        #endregion

        #region AJAX DATA
        public ActionResult SalesmanByDistributor(List<int> strDistributorID, string txt_date, string cmbSS, FormCollection formParam)
        {
            PermissionHelper.CheckPermissionByFeature("Home_SalesmanByDistributor", this);
            cmbSS = Utility.StringParse(EditorExtension.GetValue<string>("cmbSS"));
            List<Salesman> listSalesman = new List<Salesman>();
            DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
            strDistributorID = strDistributorID == null ? new List<int>() : strDistributorID;

            int userID = SessionHelper.GetSession<int>("UserID");
            string loginID = SessionHelper.GetSession<string>("UserName");

            if (strDistributorID.Count > 0)
            {
                listSalesman = (from s in Global.Context.Salesmans.AsNoTracking()
                                join v in Global.Context.VisitPlanHistories.AsNoTracking()
                                 on s.SalesmanID equals v.SalesmanID
                                join r in Global.Context.Routes.AsNoTracking()
                                on new { v.RouteID, v.DistributorID } equals new { r.RouteID, r.DistributorID }
                                join ut in Global.Context.UserTerritories.AsNoTracking()
                                on new { v.RouteID, v.DistributorID } equals new { ut.RouteID, ut.DistributorID }
                                where
                                (
                                    v.VisitDate == date
                                    && strDistributorID.Contains(v.DistributorID)
                                    && (r.SalesSupID == cmbSS || cmbSS == string.Empty)
                                    && ut.UserName == loginID
                                )
                                orderby s.SalesmanID
                                select s).Distinct().ToList();
            }
            else
            {
                listSalesman = ControllerHelper.ListSalesman;
                //listSalesman = (from s in Global.Context.Salesmans.AsNoTracking()
                //                join v in Global.Context.VisitPlanHistories.AsNoTracking()
                //                 on s.SalesmanID equals v.SalesmanID
                //                join r in Global.Context.Routes.AsNoTracking()
                //                on new { v.RouteID, v.DistributorID } equals new { r.RouteID, r.DistributorID }
                //                where
                //                (
                //                    v.VisitDate == date
                //                    && (r.SalesSupID == cmbSS || cmbSS == string.Empty)
                //                )
                //                orderby s.SalesmanID
                //                select s).Distinct().ToList();
            }

            ViewData.Model = listSalesman;

            string j = HtmlBuilder.RenderViewToHtml(ControllerContext, "~/Views/Tracking/SalesmanListPartialView.cshtml", ViewData, TempData);

            return Json(new
            {
                html = j
            });
        }

        public ActionResult DistributorSSASM(string strDistributorID, string cmbSS, string txt_date, FormCollection formParam)
        {
            DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
            cmbSS = Utility.StringParse(EditorExtension.GetValue<string>("cmbSS"));
            strDistributorID = Utility.StringParse(EditorExtension.GetValue<string>("cmbDistributorID"));

            string ssID = string.Empty;
            string asmID = string.Empty;
            int DistributorID = Utility.IntParse(strDistributorID);
            if (DistributorID != 0)
            {
                var ss = ControllerHelper.GetListSaleSup(string.Empty, string.Empty, string.Empty, DistributorID).FirstOrDefault();
                ssID = ss == null ? string.Empty : ss.EmployeeID;

                var asm = ControllerHelper.GetListASM(string.Empty, string.Empty, DistributorID, string.Empty).FirstOrDefault();
                asmID = asm == null ? string.Empty : asm.EmployeeID;
            }
            else if (!string.IsNullOrEmpty(cmbSS))
            {
                var asm = ControllerHelper.GetListASM(string.Empty, string.Empty, 0, cmbSS).FirstOrDefault();
                asmID = asm == null ? string.Empty : asm.EmployeeID;
            }

            return Json(new
            {
                ssID = ssID,
                asmID = asmID
            });
        }

        public ActionResult Refresh(List<int> strDistributorID, List<string> SalesmanID, string txt_date)
        {
            PermissionHelper.CheckPermissionByFeature("Home_Refresh", this);
            DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
            strDistributorID = strDistributorID == null ? new List<int>() : strDistributorID;
            SalesmanID = SalesmanID == null ? new List<string>() : SalesmanID;

            if (SalesmanID.Count > 0)
            {
                List<pp_GetSalemanLastLocationResult> listSalesman = new List<pp_GetSalemanLastLocationResult>();
                foreach (string s in SalesmanID)
                {
                    //pp_GetSalemanLastLocationResult r = Global.Context.pp_GetSalemanLastLocation(s, date).FirstOrDefault();
                    //if (r != null)
                    //{
                    //    listSalesman.Add(r);
                    //}
                }
                //ControllerHelper.LogUserAction("Tracking", "Refresh", null);
                return Json(new
                {
                    html = listSalesman
                });

            }
            return null;
        }

        public ActionResult Filter(List<int> strDistributorID, string txt_date, string txt_salesman)
        {
            PermissionHelper.CheckPermissionByFeature("Home_Filter", this);
            //CustomLog.LogError("Log", "BeginFilter");
            DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
            strDistributorID = strDistributorID == null ? new List<int>() : strDistributorID;
            txt_salesman = txt_salesman ?? string.Empty;

            if (!string.IsNullOrEmpty(txt_salesman))
            {
                List<CustomerOnMapVM> listC = new List<CustomerOnMapVM>();
                Salesman salesman = new Salesman();
                salesman = ControllerHelper.ListSalesman.FirstOrDefault(a => a.SalesmanID == txt_salesman);

                #region GET MCP
                var listVPH = (from v in Global.Context.VisitPlanHistories.AsNoTracking()
                               join c in Global.Context.Outlets.AsNoTracking()
                                on new { v.OutletID, v.DistributorID } equals new { c.OutletID, c.DistributorID }
                               from o in Global.Context.OrderHeaders
                                    .Where(o => o.OutletID == c.OutletID && o.VisitDate == date && o.SalesmanID == txt_salesman && o.DistributorID == v.DistributorID)
                                    .DefaultIfEmpty()
                               where
                               (
                                   v.VisitDate == date
                                   && v.SalesmanID == txt_salesman
                               )
                               orderby o.VisitID ascending
                               select new { vph = v, o = c, od = o }).Distinct().ToList();
                #endregion

                #region GET ORDERS
                List<OrderHeader> listOrder = (from o in Global.Context.OrderHeaders.AsNoTracking()
                                               where
                                                  o.VisitDate == date
                                                  && o.SalesmanID == txt_salesman
                                                  && o.DistributorID == salesman.DistributorID
                                               select o
                                                     ).ToList();
                #endregion

                #region GET CURRENT VISIT
                OrderHeader odLastVisit = new OrderHeader();
                odLastVisit = (from o in Global.Context.OrderHeaders.AsNoTracking()
                               where o.VisitDate == date
                               && o.SalesmanID == txt_salesman
                               && o.DistributorID == salesman.DistributorID
                               orderby o.StartTime descending
                               select o).FirstOrDefault();
                #endregion

                #region PARAM

                List<OrderHeader> currentOrders = new List<OrderHeader>();
                CustomerASO caso = new CustomerASO();
                int todayYear = DateTime.Today.Year;
                #endregion

                foreach (var item in listVPH)
                {
                    CustomerOnMapVM c = new CustomerOnMapVM();

                    #region Customer info
                    c.VisitDate = date.ToString(Constant.ShortDatePattern);
                    c.OutletID = item.o.OutletID;
                    c.OutletName = item.o.OutletName;
                    c.Phone = item.o.Phone;
                    c.Mobile = item.o.Mobile;
                    c.Attn = item.o.Attn;
                    c.Address = item.o.Address;

                    c.Latitude = item.o.Latitude.HasValue ? item.o.Latitude.Value.ToString() : "0";
                    c.Longtitude = item.o.Longtitude.HasValue ? item.o.Longtitude.Value.ToString() : "0";
                    #endregion

                    #region VPH
                    c.VisitOrder = item.vph.VisitOrder;
                    c.SalesmanID = item.vph.SalesmanID;

                    c.SalesmanName = salesman != null ? salesman.SalesmanName : string.Empty;
                    #endregion

                    #region Lay vi tri saleman hien tai : o.StartTime descending
                    if (odLastVisit != null)
                    {
                        c.ODLatitude = odLastVisit.Latitude.ToString();
                        c.ODLongtitude = odLastVisit.Longtitude.ToString();
                        c.ODOutletID = odLastVisit.OutletID;
                    }
                    else
                    {
                        c.ODLatitude = string.Empty;
                        c.ODLongtitude = string.Empty;
                        c.ODOutletID = string.Empty;
                    }
                    #endregion

                    #region Lay salesorder tai cua hang nay
                    currentOrders = (from o in listOrder
                                     where o.VisitDate == date
                                     && o.SalesmanID == item.vph.SalesmanID
                                     && o.OutletID == item.vph.OutletID
                                     orderby o.StartTime descending
                                     select o).ToList();

                    if (currentOrders != null && currentOrders.Count > 0)
                    {
                        c.listOD = new List<orderVM>();
                        foreach (OrderHeader o in currentOrders)
                        {
                            orderVM ovm = new orderVM();

                            ovm.Code = o.Code;
                            ovm.StartTime = o.StartTime.ToString(Constant.ShortTimePattern);
                            ovm.EndTime = o.EndTime.ToString(Constant.ShortTimePattern);
                            ovm.TotalQty = FormatHelper.FormatDecimalMoney(o.TotalQty);
                            ovm.TotalAmt = FormatHelper.FormatDecimalMoney(o.TotalAmt);
                            ovm.DropSize = FormatHelper.FormatDecimalMoney(o.DropSize);
                            ovm.TotalSKU = FormatHelper.FormatDecimalMoney(o.TotalSKU);
                            ovm.SalesmanID = o.SalesmanID;
                            ovm.Reason = o.Reason;
                            ovm.Distance = FormatHelper.FormatDecimalLength(o.Distance);
                            ovm.Status = o.Status;

                            c.StartTime = o.StartTime.ToString(Constant.ShortTimePattern);
                            c.EndTime = o.EndTime.ToString(Constant.ShortTimePattern);

                            c.listOD.Add(ovm);
                        }

                        #region Process Image Link
                        if (string.IsNullOrEmpty(currentOrders.Last().ImageFile))
                        {
                            if (string.IsNullOrEmpty(currentOrders.First().ImageFile))
                            {
                                c.ImageFile = Url.Content(Constant.OutletImageFolder) + "ImageNotFound.jpg";
                            }
                            else
                            {
                                c.ImageFile = Url.Content(Constant.OutletImageFolder) + currentOrders.First().ImageFile;
                            }
                        }
                        else
                        {
                            c.ImageFile = Url.Content(Constant.OutletImageFolder) + currentOrders.Last().ImageFile;
                        }
                        #endregion

                        c.Status = currentOrders.Last().Status;
                        c.VisitID = currentOrders.Last().VisitID;
                    }
                    #endregion

                    listC.Add(c);
                }

                var ordered = listC.OrderBy(a => a.StartTime).ToList();
                int n = 1;
                foreach (CustomerOnMapVM c in ordered)
                {
                    if (!string.IsNullOrEmpty(c.StartTime))
                    {
                        c.ODVisitOrder = n;
                        n++;
                    }
                }

                //CustomLog.LogError("Log", "EndFilter");
                //ControllerHelper.LogUserAction("Tracking", "Filter", null);

                //Get salemanLocation
                pp_GetSalemanLastLocationResult r = new pp_GetSalemanLastLocationResult(); //Global.Context.pp_GetSalemanLastLocation(salesman.SalesmanID, date).FirstOrDefault();

                return Json(new
                {
                    html = ordered,
                    smLocation = r
                });
            }

            return null;
        }

        public ActionResult SalemanMovement(List<int> strDistributorID, string txt_date, string txt_salesman)
        {
            PermissionHelper.CheckPermissionByFeature("Home_SalemanMovement", this);
            DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
            strDistributorID = strDistributorID == null ? new List<int>() : strDistributorID;
            txt_salesman = txt_salesman ?? string.Empty;

            if (!string.IsNullOrEmpty(txt_salesman))
            {
                Salesman salesman = new Salesman();
                salesman = Global.Context.Salesmans.AsNoTracking().FirstOrDefault(a => a.SalesmanID == txt_salesman);

                List<OrderHeader> listOrder = (from o in Global.Context.OrderHeaders.AsNoTracking()
                                               where
                                                  o.VisitDate == date
                                                  && o.SalesmanID == txt_salesman
                                                  && o.DistributorID == salesman.DistributorID
                                               select o
                                        ).ToList();
                List<SalesmanVisit> listSaleVisit = (from o in Global.Context.SalesmanVisits.AsNoTracking()
                                                     where
                                                        o.VisitDate == date
                                                        && o.SalesmanID == txt_salesman
                                                        && o.DistributorID == salesman.DistributorID
                                                     select o
                                        ).ToList();

                List<SMMovementVM> listModel = (from o in listOrder
                                                select new SMMovementVM()
                                                {
                                                    Latitude = o.Latitude,
                                                    Longtitude = o.Longtitude,
                                                    SalemanCode = o.SalesmanID,
                                                    VisitTime = o.StartTime,
                                                    strVisitTime = o.StartTime.ToTimePattern()
                                                }).ToList();

                listModel.AddRange((from o in listSaleVisit
                                    select new SMMovementVM()
                                    {
                                        Latitude = Utility.DecimalParse(o.Latitude),
                                        Longtitude = Utility.DecimalParse(o.Longtitude),
                                        SalemanCode = o.SalesmanID,
                                        VisitTime = o.VisitTime,
                                        strVisitTime = o.VisitTime.ToTimePattern()
                                    }).ToList());

                var ordered = listModel.OrderBy(a => a.VisitTime).ToList();

                return Json(new
                {
                    html = ordered
                });
            }

            return null;
        }

        public ActionResult SSMovement(string txt_date, string cmbSS, string txt_salesman, FormCollection formParam)
        {
            PermissionHelper.CheckPermissionByFeature("Home_SSMovement", this);
            cmbSS = Utility.StringParse(EditorExtension.GetValue<string>("cmbSS"));
            if (!string.IsNullOrEmpty(cmbSS) && !string.IsNullOrEmpty(txt_date))
            {
                DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
                cmbSS = cmbSS ?? string.Empty;

                if (!string.IsNullOrEmpty(cmbSS))
                {
                    //Get Salesman
                    DMSSalesForce saleSup = new DMSSalesForce();
                    saleSup = Global.Context.DMSSalesForces.AsNoTracking().FirstOrDefault(a => a.EmployeeID == cmbSS);

                    List<ASMVisitStore> listSaleVisit = (from o in Global.Context.ASMVisitStores.AsNoTracking()
                                                         where
                                                            o.Date == date
                                                            && o.SupCode == cmbSS
                                                            && o.AsmCode == string.Empty
                                                         select o
                                        ).ToList();

                    List<SMMovementVM> listModel = (from o in listSaleVisit
                                                    select new SMMovementVM()
                                                    {
                                                        Latitude = o.LatitudeStart,
                                                        Longtitude = o.LongtitudeStart,
                                                        SalemanCode = o.SupCode,
                                                        VisitTime = o.TimeStart,
                                                        strVisitTime = o.TimeStart.ToTimePattern()
                                                    }).ToList();

                    var ordered = listModel.OrderBy(a => a.VisitTime).ToList();

                    return Json(new
                    {
                        html = ordered
                    });
                }
            }
            return null;
        }

        public ActionResult ASMMovement(string txt_date, string cmbASM, string txt_salesman, FormCollection formParam)
        {
            PermissionHelper.CheckPermissionByFeature("Home_ASMMovement", this);
            cmbASM = Utility.StringParse(EditorExtension.GetValue<string>("cmbASM"));
            if (!string.IsNullOrEmpty(cmbASM) && !string.IsNullOrEmpty(txt_date))
            {
                DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
                cmbASM = cmbASM ?? string.Empty;

                if (!string.IsNullOrEmpty(cmbASM))
                {
                    //Get Salesman
                    DMSSalesForce saleSup = new DMSSalesForce();
                    saleSup = Global.Context.DMSSalesForces.AsNoTracking().FirstOrDefault(a => a.EmployeeID == cmbASM);

                    List<ASMVisitStore> listSaleVisit = (from o in Global.Context.ASMVisitStores.AsNoTracking()
                                                         where
                                                            o.Date == date
                                                            && o.AsmCode == cmbASM
                                                         //&& o.SupCode == string.Empty
                                                         select o
                                        ).ToList();

                    List<SMMovementVM> listModel = (from o in listSaleVisit
                                                    select new SMMovementVM()
                                                    {
                                                        Latitude = o.LatitudeStart,
                                                        Longtitude = o.LongtitudeStart,
                                                        SalemanCode = o.SupCode,
                                                        VisitTime = o.TimeStart,
                                                        strVisitTime = o.TimeStart.ToTimePattern()
                                                    }).ToList();

                    var ordered = listModel.OrderBy(a => a.VisitTime).ToList();

                    return Json(new
                    {
                        html = ordered
                    });
                }
            }
            return null;
        }

        public ActionResult SalesmanInfo(string txt_date, string txt_salesman)
        {
            PermissionHelper.CheckPermissionByFeature("Home_SalesmanInfo", this);
            if (!string.IsNullOrEmpty(txt_salesman) && !string.IsNullOrEmpty(txt_date))
            {
                DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
                txt_salesman = txt_salesman ?? string.Empty;

                if (!string.IsNullOrEmpty(txt_salesman))
                {
                    //Get Salesman
                    Salesman salesman = new Salesman();
                    salesman = Global.Context.Salesmans.AsNoTracking().FirstOrDefault(a => a.SalesmanID == txt_salesman);

                    //Get Route from Salesman
                    Route route = (from vph in Global.Context.VisitPlanHistories.AsNoTracking()
                                   join r in Global.Context.Routes.AsNoTracking()
                                   on new { vph.RouteID, vph.DistributorID } equals new { r.RouteID, r.DistributorID }
                                   where vph.SalesmanID == salesman.SalesmanID
                                   && vph.VisitDate == date
                                   select r).FirstOrDefault();

                    //Get listOutLet by today of Salesman
                    int countOutlet = (from vph in Global.Context.VisitPlanHistories.AsNoTracking()
                                       join o in Global.Context.Outlets.AsNoTracking()
                                       on new { vph.OutletID, vph.DistributorID } equals new { o.OutletID, o.DistributorID }
                                       where vph.SalesmanID == salesman.SalesmanID
                                       && vph.VisitDate == date
                                       select o).Distinct().Count();

                    List<OrderHeader> listOrder = (from o in Global.Context.OrderHeaders.AsNoTracking()
                                                   where o.SalesmanID == salesman.SalesmanID
                                                     && o.VisitDate == date
                                                   select o
                                              ).ToList();

                    //Lấy số khách hàng đã ghé thăm
                    int countOutletVisited = listOrder.Select(a => a.OutletID).Distinct().Count();
                    //int countOutletVisited = (from o in Global.Context.OrderHeaders.AsNoTracking()
                    //                          where o.SalesmanID == salesman.SalesmanID
                    //                            && o.VisitDate == date
                    //                          select new { o.OutletID }
                    //                          ).Distinct().Count();

                    //Lấy số khách hàng có đơn hàng
                    int countOutletHaveOrder = listOrder.Where(a => a.Code != string.Empty).Select(a => a.OutletID).Distinct().Count();
                    //int countOutletHaveOrder = (from o in Global.Context.OrderHeaders.AsNoTracking()
                    //                            where o.SalesmanID == salesman.SalesmanID
                    //                              && o.VisitDate == date
                    //                              && o.code != string.Empty
                    //                            select new { o.OutletID }
                    //                          ).Distinct().Count();

                    int countOrder = listOrder.Where(a => a.Code != string.Empty).Select(a => a.Code).Distinct().Count();
                    decimal totalSKU = listOrder.Sum(a => a.TotalSKU);
                    decimal totalSKUperCount = countOrder != 0 ? totalSKU / (decimal)countOrder : 0;
                    decimal totalAMT = listOrder.Sum(a => a.TotalAmt);
                    decimal totalQuantity = listOrder.Sum(a => a.DropSize);

                    //ControllerHelper.LogUserAction("Tracking", "SalesmanInfo", null);

                    return Json(new
                    {
                        txt_date,
                        route.RouteID,
                        route.RouteName,
                        salesman.SalesmanID,
                        salesman.SalesmanName,
                        countOutlet,
                        countOutletVisited,
                        countOutletHaveOrder,
                        countOrder,
                        totalSKU = FormatHelper.FormatDecimalMoney(totalSKU),
                        totalSKUperCount = FormatHelper.FormatDecimalLength(totalSKUperCount),
                        totalAMT = FormatHelper.FormatDecimalMoney(totalAMT),
                        totalQuantity = FormatHelper.FormatDecimalMoney(totalQuantity),
                    });
                }
                //return null;
            }
            return null;
        }

        [AllowAnonymous]
        public ActionResult CheckSession()
        {
            int userID = SessionHelper.GetSession<int>("UserID");
            if (userID <= 0)
            {
                return Json(new
                {
                    Authenticated = 0
                });
            }

            return Json(new
            {
                Authenticated = 1
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SMSSASMLocation(List<int> strDistributorID, List<string> SalesmanID, string txt_date)
        {
            PermissionHelper.CheckPermissionByFeature("Home_SMSSASMLocation", this);
            DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
            strDistributorID = strDistributorID == null ? new List<int>() : strDistributorID;
            SalesmanID = SalesmanID == null ? new List<string>() : SalesmanID;

            if (SalesmanID.Count > 0)
            {
                List<pp_GetSalemanLastLocationResult> listSalesman = new List<pp_GetSalemanLastLocationResult>();
                foreach (string s in SalesmanID)
                {
                    //pp_GetSalemanLastLocationResult r = Global.Context.pp_GetSalemanLastLocation(s, date).FirstOrDefault();
                    //if (r != null)
                    //{
                    //    listSalesman.Add(r);
                    //}
                }
                //ControllerHelper.LogUserAction("Tracking", "Refresh", null);
                return Json(new
                {
                    html = listSalesman
                });

            }
            return null;
        }

        public ActionResult SaleForceMovementMonitoring(List<int> strDistributorID, string txt_date, string txt_salesman)
        {
            PermissionHelper.CheckPermissionByFeature("Home_SaleForceMovementMonitoring", this);
            //CustomLog.LogError("Log", "BeginFilter");
            DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
            strDistributorID = strDistributorID == null ? new List<int>() : strDistributorID;
            txt_salesman = txt_salesman ?? string.Empty;

            if (!string.IsNullOrEmpty(txt_salesman))
            {
                List<CustomerOnMapVM> listC = new List<CustomerOnMapVM>();
                Salesman salesman = new Salesman();
                salesman = Global.Context.Salesmans.AsNoTracking().FirstOrDefault(a => a.SalesmanID == txt_salesman);

                #region GET MCP
                var listVPH = (from v in Global.Context.VisitPlanHistories.AsNoTracking()
                               join c in Global.Context.Outlets.AsNoTracking()
                                on new { v.OutletID, v.DistributorID } equals new { c.OutletID, c.DistributorID }
                               from o in Global.Context.OrderHeaders
                                    .Where(o => o.OutletID == c.OutletID && o.VisitDate == date && o.SalesmanID == txt_salesman && o.DistributorID == v.DistributorID)
                                    .DefaultIfEmpty()
                               where
                               (
                                   v.VisitDate == date
                                   && v.SalesmanID == txt_salesman
                               )
                               orderby o.VisitID ascending
                               select new { vph = v, o = c, od = o }).Distinct().ToList();
                #endregion

                #region GET ORDERS
                List<OrderHeader> listOrder = (from o in Global.Context.OrderHeaders.AsNoTracking()
                                               where
                                                  o.VisitDate == date
                                                  && o.SalesmanID == txt_salesman
                                                  && o.DistributorID == salesman.DistributorID
                                               select o
                                                     ).ToList();
                #endregion

                #region GET CURRENT VISIT
                OrderHeader odLastVisit = new OrderHeader();
                odLastVisit = (from o in Global.Context.OrderHeaders.AsNoTracking()
                               where o.VisitDate == date
                               && o.SalesmanID == txt_salesman
                               && o.DistributorID == salesman.DistributorID
                               orderby o.StartTime descending
                               select o).FirstOrDefault();
                #endregion

                #region PARAM

                List<OrderHeader> currentOrders = new List<OrderHeader>();
                CustomerASO caso = new CustomerASO();
                int todayYear = DateTime.Today.Year;
                #endregion

                foreach (var item in listVPH)
                {
                    CustomerOnMapVM c = new CustomerOnMapVM();

                    #region Customer info
                    c.VisitDate = date.ToString(Constant.ShortDatePattern);
                    c.OutletID = item.o.OutletID;
                    c.OutletName = item.o.OutletName;
                    c.Phone = item.o.Phone;
                    c.Mobile = item.o.Mobile;
                    c.Attn = item.o.Attn;
                    c.Address = item.o.Address;
                    c.Latitude = item.o.Latitude.HasValue ? item.o.Latitude.Value.ToString() : "0";
                    c.Longtitude = item.o.Longtitude.HasValue ? item.o.Longtitude.Value.ToString() : "0";
                    #endregion

                    #region VPH
                    c.VisitOrder = item.vph.VisitOrder;
                    c.SalesmanID = item.vph.SalesmanID;

                    c.SalesmanName = salesman != null ? salesman.SalesmanName : string.Empty;
                    #endregion

                    #region Lay vi tri saleman hien tai : o.StartTime descending
                    if (odLastVisit != null)
                    {
                        c.ODLatitude = odLastVisit.Latitude.ToString();
                        c.ODLongtitude = odLastVisit.Longtitude.ToString();
                        c.ODOutletID = odLastVisit.OutletID;
                    }
                    else
                    {
                        c.ODLatitude = string.Empty;
                        c.ODLongtitude = string.Empty;
                        c.ODOutletID = string.Empty;
                    }
                    #endregion

                    #region Lay salesorder tai cua hang nay
                    currentOrders = (from o in listOrder
                                     where o.VisitDate == date
                                     && o.SalesmanID == item.vph.SalesmanID
                                     && o.OutletID == item.vph.OutletID
                                     orderby o.StartTime descending
                                     select o).ToList();

                    if (currentOrders != null && currentOrders.Count > 0)
                    {
                        c.listOD = new List<orderVM>();
                        foreach (OrderHeader o in currentOrders)
                        {
                            orderVM ovm = new orderVM();

                            ovm.Code = o.Code;
                            ovm.StartTime = o.StartTime.ToString(Constant.ShortTimePattern);
                            ovm.EndTime = o.EndTime.ToString(Constant.ShortTimePattern);
                            ovm.TotalQty = FormatHelper.FormatDecimalMoney(o.TotalQty);
                            ovm.TotalAmt = FormatHelper.FormatDecimalMoney(o.TotalAmt);
                            ovm.DropSize = FormatHelper.FormatDecimalMoney(o.DropSize);
                            ovm.TotalSKU = FormatHelper.FormatDecimalMoney(o.TotalSKU);
                            ovm.SalesmanID = o.SalesmanID;
                            ovm.Reason = o.Reason;
                            ovm.Distance = FormatHelper.FormatDecimalLength(o.Distance);
                            ovm.Status = o.Status;

                            c.StartTime = o.StartTime.ToString(Constant.ShortTimePattern);
                            c.EndTime = o.EndTime.ToString(Constant.ShortTimePattern);

                            c.listOD.Add(ovm);
                        }

                        #region Process Image Link
                        if (string.IsNullOrEmpty(currentOrders.Last().ImageFile))
                        {
                            if (string.IsNullOrEmpty(currentOrders.First().ImageFile))
                            {
                                c.ImageFile = Url.Content(Constant.OutletImageFolder) + "ImageNotFound.jpg";
                            }
                            else
                            {
                                c.ImageFile = Url.Content(Constant.OutletImageFolder) + currentOrders.First().ImageFile;
                            }
                        }
                        else
                        {
                            c.ImageFile = Url.Content(Constant.OutletImageFolder) + currentOrders.Last().ImageFile;
                        }
                        #endregion

                        c.Status = currentOrders.Last().Status;
                        c.VisitID = currentOrders.Last().VisitID;
                    }
                    #endregion

                    listC.Add(c);
                }

                var ordered = listC.OrderBy(a => a.StartTime).ToList();
                int n = 1;
                foreach (CustomerOnMapVM c in ordered)
                {
                    if (!string.IsNullOrEmpty(c.StartTime))
                    {
                        c.ODVisitOrder = n;
                        n++;
                    }
                }

                //CustomLog.LogError("Log", "EndFilter");
                //ControllerHelper.LogUserAction("Tracking", "Filter", null);
                return Json(new
                {
                    html = ordered
                });
            }

            return null;
        }

        //public ActionResult TableVisitInfo(string txt_date, string txt_salesman)
        //{
        //    PermissionHelper.CheckPermissionByFeature("Home_SalesmanInfo", this);
        //    if (!string.IsNullOrEmpty(txt_salesman) && !string.IsNullOrEmpty(txt_date))
        //    {
        //        DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
        //        txt_salesman = txt_salesman ?? string.Empty;

        //        if (!string.IsNullOrEmpty(txt_salesman))
        //        {
        //            List<pp_GetVisitInfoResult> listModel = Global.Context.pp_GetVisitInfo(0, txt_salesman, date).ToList();
        //            List<rpVisitInfoVMItem> listVM = new List<rpVisitInfoVMItem>();
        //            foreach (pp_GetVisitInfoResult model in listModel)
        //            {
        //                rpVisitInfoVMItem item = new rpVisitInfoVMItem();
        //                item.item = model;
        //                item.SMTime = model.SMTimeStart.ToTimePattern() + " - " + model.SMTimeEnd.ToTimePattern();
        //                item.SUPTime = model.SUPTimeStart.ToTimePattern() + " - " + model.SUPTimeEnd.ToTimePattern();
        //                item.ASMTime = model.ASMTimeStart.ToTimePattern() + " - " + model.ASMTimeEnd.ToTimePattern();
        //                listVM.Add(item);
        //            }

        //            listVM = listVM.OrderBy(a => a.item.VisitOrder).ToList();

        //            return Json(new
        //            {
        //                list = listVM
        //            });
        //        }
        //    }
        //    return null;
        //}


        #endregion

        #region ComboBoxPartialMM
        public ActionResult ComboBoxPartialMMASM()
        {
            return PartialView(ControllerHelper.ListASM);
        }

        public ActionResult ComboBoxPartialMMDistributor()
        {
            string ASM = Utility.StringParse(Request.Params["ASM"]);
            string SS = Utility.StringParse(Request.Params["SS"]);
            string provinceID = Utility.StringParse(Request.Params["ProvinceID"]);

            List<Distributor> listItem = ControllerHelper.GetListDistributor(ASM, SS);
            return PartialView(listItem);
        }

        public ActionResult ComboBoxPartialMMSaleSup()
        {
            string ASM = Utility.StringParse(Request.Params["ASM"]);
            List<DMSSalesForce> listItem = ControllerHelper.GetListSaleSup(ASM);
            return PartialView(listItem);
        }
        #endregion

        #region ComboBoxPartial
        public ActionResult ComboBoxPartialASM()
        {
            return PartialView(ControllerHelper.ListASM);
        }

        public ActionResult ComboBoxPartialRegion()
        {
            return PartialView(ControllerHelper.ListRegion);
        }

        public ActionResult ComboBoxPartialArea()
        {
            string regionID = Utility.StringParse(Request.Params["RegionID"]);
            List<DMSArea> listItem = ControllerHelper.GetListArea(regionID);
            return PartialView(listItem);
        }

        public ActionResult ComboBoxPartialProvince()
        {
            var regionID = Utility.StringParse(Request.Params["RegionID"]);
            var areaID = Utility.StringParse(Request.Params["AreaID"]);

            var listItem = ControllerHelper.GetListProvince(regionID, areaID);
            return PartialView(listItem);
        }

        public ActionResult ComboBoxPartialDistributor()
        {
            var regionID = Utility.StringParse(Request.Params["RegionID"]);
            var areaID = Utility.StringParse(Request.Params["AreaID"]);
            var provinceID = string.Empty; //Utility.StringParse(Request.Params["ProvinceID"]);
            var salesSupID = string.Empty; //Utility.StringParse(Request.Params["SalesSupID"]);

            List<Distributor> listItem = new List<Distributor>();
            if (!string.IsNullOrEmpty(regionID) || !string.IsNullOrEmpty(areaID))
            {
                listItem = ControllerHelper.GetListDistributorWithRegionArea(regionID, areaID).ToList();
            }
            else
            {
                listItem = ControllerHelper.ListDistributor.ToList();
            }
            return PartialView(listItem);
        }

        public ActionResult ComboBoxPartialSaleSup()
        {
            var regionID = Utility.StringParse(Request.Params["RegionID"]);
            var areaID = Utility.StringParse(Request.Params["AreaID"]);
            var provinceID = Utility.StringParse(Request.Params["ProvinceID"]);
            var strDistributorID = Utility.IntParse(Request.Params["DistributorID"]);

            List<DMSSalesForce> listItem = ControllerHelper.GetListSaleSup(regionID, areaID, provinceID, strDistributorID);
            return PartialView(listItem);
        }

        public ActionResult ComboBoxPartialRoute()
        {
            var regionID = Utility.StringParse(Request.Params["RegionID"]);
            var areaID = Utility.StringParse(Request.Params["AreaID"]);
            var provinceID = string.Empty; //Utility.StringParse(Request.Params["ProvinceID"]);
            var strDistributorID = Utility.IntParse(Request.Params["DistributorID"]);
            var salesSupID = string.Empty; //Utility.StringParse(Request.Params["SalesSupID"]);

            List<Route> listItem = ControllerHelper.GetListRoute(regionID, areaID, provinceID, strDistributorID, salesSupID);
            return PartialView(listItem);
        }

        public ActionResult LoadSalesman(FormCollection formParam)
        {
            var regionID = Utility.StringParse(Request.Params["RegionID"]);
            var areaID = Utility.StringParse(Request.Params["AreaID"]);
            var provinceID = Utility.StringParse(Request.Params["ProvinceID"]);
            var distributorID = Utility.IntParse(Request.Params["DistributorID"]);
            var salesSupID = Utility.StringParse(Request.Params["SalesSupID"]);

            //var listSalesman = (from s in ControllerHelper.ListSalesman
            //                    where s.DistributorID == distributorID
            //                    select s).Distinct().ToList();
            var listSalesman = (from s in Global.Context.Salesmans.AsNoTracking()
                                where s.DistributorID == distributorID
                                select s).Distinct().ToList();
            var listMapSalesman = ControllerHelper.ListMapSalesman(listSalesman);
            ViewData.Model = listMapSalesman;

            //ControllerHelper.LogUserAction("Home", "SalesmanListPartialView", null);
            string j = HtmlBuilder.RenderViewToHtml(ControllerContext, "~/Views/Tracking/SalesmanListPartialView.cshtml", ViewData, TempData);
            return Json(new
            {
                html = j
            });
        }
        #endregion

        #region Slide
        public ActionResult Slide(string txt_date, string outletId)
        {
            PermissionHelper.CheckPermissionByFeature("Home_Slide", this);
            if (!string.IsNullOrEmpty(outletId))
            {
                DateTime date = DateTime.ParseExact(txt_date, Models.Constant.ShortDatePattern, System.Globalization.CultureInfo.InvariantCulture);
                Outlet o = Global.Context.Outlets.AsNoTracking().FirstOrDefault(a => a.OutletID == outletId);
                if (o != null)
                {
                    List<string> listM = (from a in Global.Context.MerchandiseImages.AsNoTracking()
                                          where
                                              a.OutletID == outletId
                                              && a.ImageFile != null
                                              && a.ImageFile != string.Empty
                                              && a.VisitDate == date
                                          select a.ImageFile).Distinct().ToList();

                    var mcp = (from vph in Global.Context.VisitPlanHistories//.AsNoTracking()
                               join d in Global.Context.Distributors//.AsNoTracking()
                               on vph.DistributorID equals d.DistributorID
                               where
                               vph.OutletID == o.OutletID
                               && vph.DistributorID == o.DistributorID
                               && vph.VisitDate == date
                               select new { d.DistributorCode, vph.SalesmanID }
                                                   ).FirstOrDefault();

                    List<string> listImage = new List<string>();
                    string filename = string.Empty;
                    string filenameOnServer = string.Empty;
                    foreach (string item in listM)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            filenameOnServer = Constant.SalesOrdersImageFolder + date.ToString(Constant.imageDateFormat) + "/";
                            filename = Url.Content(filenameOnServer + item);
                            listImage.Add(filename);
                        }
                    }

                    SlideVM model = new SlideVM()
                    {
                        outlet = o,
                        listM = listImage
                    };
                    return View(model);
                }
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region ReportSMVisitSummary
        [Authorize]
        [ActionAuthorize("Tracking_ReportSMVisitSummary", true)]
        public ActionResult ReportSMVisitSummary(string strFromDate, string strToDate, string act, FormCollection formParam, string txtTimeVisit)//
        {
            ReportSMVisitSummaryVM model = new ReportSMVisitSummaryVM();
            #region Validate Data
            try
            {
                //Begin get
                //Set default value
                if (string.IsNullOrEmpty(strFromDate))
                {
                    model.areaID = string.Empty;
                    model.FromDate = DateTime.Today;
                    model.distributorID = 0;
                    model.provinceID = string.Empty;
                    model.regionID = string.Empty;
                    model.routeID = string.Empty;
                    model.saleSupID = string.Empty;
                    model.ToDate = DateTime.Today;
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.strToDate = model.ToDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                    model.orderDistanceValid = 0;
                }
                else
                {
                    model.areaID = Utility.StringParse(EditorExtension.GetValue<string>("AreaID"));// Utility.StringParse(areaID);
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.distributorID = Utility.IntParse(EditorExtension.GetValue<int>("DistributorID"));// Utility.IntParse(distributorID);
                    model.provinceID = string.Empty; //Utility.StringParse(EditorExtension.GetValue<string>("ProvinceID"));// Utility.StringParse(provinceID);
                    model.regionID = Utility.StringParse(EditorExtension.GetValue<string>("RegionID"));// Utility.StringParse(regionID);
                    model.routeID = Utility.StringParse(EditorExtension.GetValue<string>("RouteID"));// Utility.StringParse(routeID);
                    model.saleSupID = Utility.StringParse(EditorExtension.GetValue<string>("SalesSupID"));// Utility.StringParse(salesSupID);
                    model.ToDate = Utility.DateTimeParse(strToDate);
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.strToDate = model.ToDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                    model.orderDistanceValid = 0;


                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            #region LoadComboboxListItem
            model.ListRegion = ControllerHelper.GetListRegion(model.regionID); //model.regionID);
            if (string.IsNullOrEmpty(model.regionID) && model.ListRegion.Count == 1)
            {
                model.regionID = model.ListRegion.First().RegionID;
            }

            model.ListArea = ControllerHelper.GetListArea(model.regionID);
            if (string.IsNullOrEmpty(model.areaID) && model.ListArea.Count == 1)
            {
                model.areaID = model.ListArea.First().AreaID;
            }

            model.ListProvince = new List<DMSProvince>();// ControllerHelper.GetListProvince(model.regionID, model.areaID);
            if (string.IsNullOrEmpty(model.provinceID) && model.ListProvince.Count == 1)
            {
                model.provinceID = model.ListProvince.First().ProvinceID;
            }

            model.ListSForce = ControllerHelper.GetListSaleSup(model.regionID, model.areaID, model.provinceID, model.distributorID);
            if (string.IsNullOrEmpty(model.saleSupID) && model.ListSForce.Count == 1)
            {
                model.saleSupID = model.ListSForce.First().EmployeeID;
            }

            model.ListDistributor = ControllerHelper.GetListDistributor(model.regionID, model.areaID, model.provinceID, model.saleSupID);
            if (model.distributorID == 0 && model.ListDistributor.Count == 1)
            {
                model.distributorID = model.ListDistributor.First().DistributorID;
            }

            model.ListRoute = ControllerHelper.GetListRoute(model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID);
            if (string.IsNullOrEmpty(model.routeID) && model.ListRoute.Count == 1)
            {
                model.routeID = model.ListRoute.First().RouteID;
            }
            #endregion

            if (model.ToDate < model.FromDate)
            {
                model.listItem = new List<pp_ReportSMVisitSummaryResult>();
                ViewBag.StatusMessage = "Ngày không hợp lệ. Vui lòng chọn ngày hợp lệ.";
                return View(model);
            }

            #region Set default select if drop have only one item
            #endregion

            if (
                !string.IsNullOrEmpty(strFromDate)
                )
            {
                model.listItem = Global.Context.pp_ReportSMVisitSummary(model.FromDate, model.ToDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, username, model.firstTimeSync, model.firstTimeVisitAM, model.firstTimeVisitPM, model.lastTimeVisit, model.orderDistanceValid, model.TimeVisit).ToList();
            }
            else
            {
                model.listItem = new List<pp_ReportSMVisitSummaryResult>();
            }
            SessionHelper.SetSession<ReportSMVisitSummaryVM>("ReportSMVisitSummary", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportSMVisitSummaryExport");
            }

            if (act == "ExportExcelRawData")
            {
                return RedirectToAction("ReportSMVisitSummaryExportRAWData");
            }
            return View(model);
        }

        [Authorize]
        public PartialViewResult ReportSMVisitSummaryPartial()
        {
            List<pp_ReportSMVisitSummaryResult> model = SessionHelper.GetSession<ReportSMVisitSummaryVM>("ReportSMVisitSummary").listItem;
            return PartialView("ReportSMVisitSummaryPartial", model);
        }

        #region ReportSalemanKPIExport
        [ActionAuthorize("Tracking_ReportSalemanKPIExport")]
        public ActionResult ReportSMVisitSummaryExport()
        {
            var model = SessionHelper.GetSession<ReportSMVisitSummaryVM>("ReportSMVisitSummary").listItem;
            return PivotGridExtension.ExportToXlsx(ReportSMVisitSummarySettings(), model);
        }

        private static PivotGridSettings ReportSMVisitSummarySettings()
        {
            PivotGridSettings settings = new PivotGridSettings();

            settings.Name = "ReportSMVisitSummary";
            settings.CallbackRouteValues = new { Controller = "Image", Action = "ReportSMVisitSummaryPartial" };
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowDragInCustomizationForm = false;
            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ColumnTotalsLocation = PivotTotalsLocation.Far;
            settings.OptionsView.ShowRowTotals = true;
            settings.OptionsView.ShowTotalsForSingleValues = true;

            settings.Groups.Add("RSMID - RegionID - ASMID - AreaID - ProvinceID - DistributorCode - SaleSupID - RouteID - SalesmanID");


            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;

                field.FieldName = "RegionID";
                field.Caption = Utility.Phrase("RegionID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 1;

                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("RegionName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 2;

                field.FieldName = "RSMID";
                field.Caption = Utility.Phrase("RSMID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 3;

                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 4;

                field.FieldName = "AreaID";
                field.Caption = Utility.Phrase("AreaID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 5;

                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("AreaName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 6;

                field.FieldName = "ASMID";
                field.Caption = Utility.Phrase("ASMID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 7;

                field.FieldName = "ASMName";
                field.Caption = Utility.Phrase("ASMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 8;

                field.FieldName = "ProvinceID";
                field.Caption = Utility.Phrase("ProvinceID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 9;

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 10;

                field.FieldName = "DistributorCode";
                field.Caption = Utility.Phrase("DistributorCode");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 11;

                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("DistributorName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 12;

                field.FieldName = "SaleSupID";
                field.Caption = Utility.Phrase("SaleSupID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 13;

                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("SaleSupName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 14;

                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("RouteID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 15;

                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("RouteName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 16;

                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("SalesmanID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 17;

                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("SalesmanName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 18;
                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortDatePattern;

                field.FieldName = "VisitDate";
                field.Caption = Utility.Phrase("VisitDate");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 19;
                field.FieldName = "strIsMCP";
                field.Caption = Utility.Phrase("strIsMCP");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });


            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 19;
                field.FieldName = "FirstSyncTime";
                field.Caption = Utility.Phrase("FirstSyncTime");

                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 20;
                field.FieldName = "FirstStartTimeAM";
                field.Caption = Utility.Phrase("FirstStartTimeAM");

                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 21;
                field.FieldName = "FirstStartTimePM";
                field.Caption = Utility.Phrase("FirstStartTimePM");

                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 22;
                field.FieldName = "LastEndTime";
                field.Caption = Utility.Phrase("LastEndTime");

                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });



            //DATA AREA
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 0;
                field.FieldName = "OutletMustVisit";
                field.Caption = Utility.Phrase("OutletMustVisit");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 1;
                field.FieldName = "OutletVisited";
                field.Caption = Utility.Phrase("OutletVisited");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 2;
                field.FieldName = "OrderCount";
                field.Caption = Utility.Phrase("OrderCount");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 3;
                field.FieldName = "TotalSKU";
                field.Caption = Utility.Phrase("TotalSKU");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 4;
                field.FieldName = "TotalQuantity";
                field.Caption = Utility.Phrase("TotalQuantity");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 5;
                field.FieldName = "TotalAmount";
                field.Caption = Utility.Phrase("TotalAmount");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.DataArea;
            //    field.AreaIndex = 6;
            //    field.FieldName = "LPPC";
            //    field.Caption = Utility.Phrase("LPPC");

            //    field.CellFormat.FormatType = FormatType.Custom;
            //    field.CellFormat.FormatString = "###,##0.##";
            //    field.SummaryType = PivotSummaryType.Sum;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 7;
                field.FieldName = "SOMCP";
                field.Caption = Utility.Phrase("SOMCP");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.00";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 8;
                field.FieldName = "VisitMCP";
                field.Caption = Utility.Phrase("VisitMCP");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.00";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 13;
                field.FieldName = "OrderDistanceInvalid";
                field.Caption = Utility.Phrase("OrderDistanceInvalid");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });
            settings.OptionsPager.RowsPerPage = 0;

            return settings;
        }
        #endregion

        public ActionResult ReportSMVisitSummaryExportRAWData()
        {
            var model = SessionHelper.GetSession<ReportSMVisitSummaryVM>("ReportSMVisitSummary").listItem;
            return GridViewExtension.ExportToXlsx(ReportSMVisitSummarySettingsRAWData(), model);
        }

        private static GridViewSettings ReportSMVisitSummarySettingsRAWData()
        {
            var settings = new GridViewSettings
            {
                Name = "ReportSMVisitSummary",
                KeyFieldName = "SaleSupID",
                CallbackRouteValues =
                    new { Controller = "Home", Action = "ReportSMVisitSummaryPartial" },
                Width = Unit.Percentage(100)
            };
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;

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
            settings.Columns.Add(field =>
            {

                field.FieldName = "RSMID";
                field.Caption = Utility.Phrase("RSMID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
            });
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
            settings.Columns.Add(field =>
            {

                field.FieldName = "ASMID";
                field.Caption = Utility.Phrase("ASMID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ASMName";
                field.Caption = Utility.Phrase("ASMName");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ProvinceID";
                field.Caption = Utility.Phrase("ProvinceID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
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
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstSyncTime";
                field.Caption = Utility.Phrase("FirstSyncTime");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstStartTimeAM";
                field.Caption = Utility.Phrase("FirstStartTimeAM");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstStartTimePM";
                field.Caption = Utility.Phrase("FirstStartTimePM");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LastEndTime";
                field.Caption = Utility.Phrase("LastEndTime");

            });
            //DATA AREA
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletMustVisit";
                field.Caption = Utility.Phrase("OutletMustVisit");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletVisited";
                field.Caption = Utility.Phrase("OutletVisited");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderCount";
                field.Caption = Utility.Phrase("OrderCount");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalSKU";
                field.Caption = Utility.Phrase("TotalSKU");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalQuantity";
                field.Caption = Utility.Phrase("TotalQuantity");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalAmount";
                field.Caption = Utility.Phrase("TotalAmount");

            });
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LPPC";
            //    field.Caption = Utility.Phrase("LPPC");

            //});
            settings.Columns.Add(field =>
            {
                field.FieldName = "SOMCP";
                field.Caption = Utility.Phrase("SOMCP");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "VisitMCP";
                field.Caption = Utility.Phrase("VisitMCP");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderDistanceInvalid";
                field.Caption = Utility.Phrase("OrderDistanceInvalid");

            });
            return settings;
        }

        public ActionResult ReportSMVisitSummaryChart(string groupby)
        {
            ReportSMVisitSummaryVM model = SessionHelper.GetSession<ReportSMVisitSummaryVM>("ReportSMVisitSummary");

            var result = new List<ReportSMVisitSummaryChartData>();

            #region Map Data by Filter
            if (groupby == "Region")
            {
                result =
                (
                  from row in model.listItem
                  group row by new { row.RegionID, row.RegionName } into g
                  select new ReportSMVisitSummaryChartData()
                  {
                      Name = g.Key.RegionName,
                      TotalAmount = g.Sum(x => x.TotalAmount),
                      OrderCount = g.Sum(x => x.OrderCount),
                      TotalSKU = g.Sum(x => x.TotalSKU),
                      TotalQuantity = g.Sum(x => x.TotalQuantity),
                      LPPC = g.Average(x => x.LPPC),
                      OrderDistanceInvalid = g.Sum(x => x.OrderDistanceInvalid),
                      SOMCP = g.Average(x => x.SOMCP),
                      VisitMCP = g.Average(x => x.VisitMCP),
                  }
                ).ToList();
            }
            else if (groupby == "RSM")
            {
                result =
                (
                  from row in model.listItem
                  group row by new { row.RSMID, row.RSMName } into g
                  select new ReportSMVisitSummaryChartData()
                  {
                      Name = g.Key.RSMName,
                      TotalAmount = g.Sum(x => x.TotalAmount),
                      OrderCount = g.Sum(x => x.OrderCount),
                      TotalSKU = g.Sum(x => x.TotalSKU),
                      TotalQuantity = g.Sum(x => x.TotalQuantity),
                      LPPC = g.Average(x => x.LPPC),
                      OrderDistanceInvalid = g.Sum(x => x.OrderDistanceInvalid),
                      SOMCP = g.Average(x => x.SOMCP),
                      VisitMCP = g.Average(x => x.VisitMCP),
                  }
                ).ToList();
            }
            else if (groupby == "Area")
            {
                result =
                (
                  from row in model.listItem
                  group row by new { row.AreaID, row.AreaName } into g
                  select new ReportSMVisitSummaryChartData()
                  {
                      Name = g.Key.AreaName,
                      TotalAmount = g.Sum(x => x.TotalAmount),
                      OrderCount = g.Sum(x => x.OrderCount),
                      TotalSKU = g.Sum(x => x.TotalSKU),
                      TotalQuantity = g.Sum(x => x.TotalQuantity),
                      LPPC = g.Average(x => x.LPPC),
                      OrderDistanceInvalid = g.Sum(x => x.OrderDistanceInvalid),
                      SOMCP = g.Average(x => x.SOMCP),
                      VisitMCP = g.Average(x => x.VisitMCP),
                  }
                ).ToList();
            }
            else if (groupby == "ASM")
            {
                result =
                (
                  from row in model.listItem
                  group row by new { row.ASMID, row.ASMName } into g
                  select new ReportSMVisitSummaryChartData()
                  {
                      Name = g.Key.ASMName,
                      TotalAmount = g.Sum(x => x.TotalAmount),
                      OrderCount = g.Sum(x => x.OrderCount),
                      TotalSKU = g.Sum(x => x.TotalSKU),
                      TotalQuantity = g.Sum(x => x.TotalQuantity),
                      LPPC = g.Average(x => x.LPPC),
                      OrderDistanceInvalid = g.Sum(x => x.OrderDistanceInvalid),
                      SOMCP = g.Average(x => x.SOMCP),
                      VisitMCP = g.Average(x => x.VisitMCP),
                  }
                ).ToList();
            }
            else if (groupby == "SS")
            {
                result =
                (
                  from row in model.listItem
                  group row by new { row.SaleSupID, row.SaleSupName } into g
                  select new ReportSMVisitSummaryChartData()
                  {
                      Name = g.Key.SaleSupName,
                      TotalAmount = g.Sum(x => x.TotalAmount),
                      OrderCount = g.Sum(x => x.OrderCount),
                      TotalSKU = g.Sum(x => x.TotalSKU),
                      TotalQuantity = g.Sum(x => x.TotalQuantity),
                      LPPC = g.Average(x => x.LPPC),
                      OrderDistanceInvalid = g.Sum(x => x.OrderDistanceInvalid),
                      SOMCP = g.Average(x => x.SOMCP),
                      VisitMCP = g.Average(x => x.VisitMCP),
                  }
                ).ToList();
            }
            else if (groupby == "NPP")
            {
                result =
                (
                  from row in model.listItem
                  group row by new { row.DistributorCode, row.DistributorName } into g
                  select new ReportSMVisitSummaryChartData()
                  {
                      Name = g.Key.DistributorName,
                      TotalAmount = g.Sum(x => x.TotalAmount),
                      OrderCount = g.Sum(x => x.OrderCount),
                      TotalSKU = g.Sum(x => x.TotalSKU),
                      TotalQuantity = g.Sum(x => x.TotalQuantity),
                      LPPC = g.Average(x => x.LPPC),
                      OrderDistanceInvalid = g.Sum(x => x.OrderDistanceInvalid),
                      SOMCP = g.Average(x => x.SOMCP),
                      VisitMCP = g.Average(x => x.VisitMCP),
                  }
                ).ToList();
            }
            else if (groupby == "Route")
            {
                result =
                (
                  from row in model.listItem
                  group row by new { row.RouteID, row.RouteName } into g
                  select new ReportSMVisitSummaryChartData()
                  {
                      Name = g.Key.RouteID,
                      TotalAmount = g.Sum(x => x.TotalAmount),
                      OrderCount = g.Sum(x => x.OrderCount),
                      TotalSKU = g.Sum(x => x.TotalSKU),
                      TotalQuantity = g.Sum(x => x.TotalQuantity),
                      LPPC = g.Average(x => x.LPPC),
                      OrderDistanceInvalid = g.Sum(x => x.OrderDistanceInvalid),
                      SOMCP = g.Average(x => x.SOMCP),
                      VisitMCP = g.Average(x => x.VisitMCP),
                  }
                ).ToList();
            }
            else if (groupby == "Salesman")
            {
                result =
                (
                  from row in model.listItem
                  group row by new { row.SalesmanID, row.SalesmanName } into g
                  select new ReportSMVisitSummaryChartData()
                  {
                      Name = g.Key.SalesmanName,
                      TotalAmount = g.Sum(x => x.TotalAmount),
                      OrderCount = g.Sum(x => x.OrderCount),
                      TotalSKU = g.Sum(x => x.TotalSKU),
                      TotalQuantity = g.Sum(x => x.TotalQuantity),
                      LPPC = g.Average(x => x.LPPC),
                      OrderDistanceInvalid = g.Sum(x => x.OrderDistanceInvalid),
                      SOMCP = g.Average(x => x.SOMCP),
                      VisitMCP = g.Average(x => x.VisitMCP),
                  }
                ).ToList();
            }
            #endregion

            #region Prepare Data For Chart
            var listColumns = (from item in result orderby item.Name select item.Name).Distinct().ToList();
            var seriesTotalAmount = (from item in result orderby item.Name select item.TotalAmount).Distinct().ToList();
            var seriesOrderCount = (from item in result orderby item.Name select (decimal)item.OrderCount).Distinct().ToList();
            var seriesTotalSKU = (from item in result orderby item.Name select item.TotalSKU).Distinct().ToList();
            var seriesTotalQuantity = (from item in result orderby item.Name select item.TotalQuantity).Distinct().ToList();
            var seriesLPPC = (from item in result orderby item.Name select (decimal)item.LPPC).Distinct().ToList();
            var seriesOrderDistanceInvalid = (from item in result orderby item.Name select (decimal)item.OrderDistanceInvalid).Distinct().ToList();
            var seriesSOMCP = (from item in result orderby item.Name select (decimal)item.SOMCP).Distinct().ToList();
            var seriesVisitMCP = (from item in result orderby item.Name select (decimal)item.VisitMCP).Distinct().ToList();
            #endregion

            #region Set Chart Data
            ChartData chartData = new ChartData();
            chartData.listSeries = new List<ColumnData>();
            chartData.listSeries.Add(new ColumnData() { name = "TotalAmount", data = seriesTotalAmount });
            chartData.listSeries.Add(new ColumnData() { name = "OrderCount", data = seriesOrderCount });
            chartData.listSeries.Add(new ColumnData() { name = "TotalSKU", data = seriesTotalSKU });
            chartData.listSeries.Add(new ColumnData() { name = "TotalQuantity", visible = true, data = seriesTotalQuantity });
            chartData.listSeries.Add(new ColumnData() { name = "LPPC", data = seriesLPPC });
            chartData.listSeries.Add(new ColumnData() { name = "OrderDistanceInvalid", data = seriesOrderDistanceInvalid });
            chartData.listSeries.Add(new ColumnData() { name = "SOMCP", data = seriesSOMCP });
            chartData.listSeries.Add(new ColumnData() { name = "VisitMCP", data = seriesVisitMCP });
            chartData.listColumns = new List<string>();
            chartData.listColumns.AddRange(listColumns);
            chartData.chartName = Utility.Phrase("Chart");
            chartData.YName = "";
            #endregion

            return Json(new
            {
                chartData
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ReportVisit
        [Authorize]
        [ActionAuthorize("Tracking_ReportVisit", true)]
        public ActionResult ReportVisit(string strFromDate, string act, FormCollection formParam)//
        {
            ReportVisitVM model = new ReportVisitVM();
            #region Validate Data
            try
            {
                //Begin get
                //Set default value
                if (string.IsNullOrEmpty(strFromDate))
                {
                    model.areaID = string.Empty;
                    model.FromDate = DateTime.Today;
                    model.distributorID = 0;
                    model.provinceID = string.Empty;
                    model.regionID = string.Empty;
                    model.routeID = string.Empty;
                    model.saleSupID = string.Empty;
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                }
                else
                {
                    model.areaID = Utility.StringParse(EditorExtension.GetValue<string>("AreaID"));// Utility.StringParse(areaID);
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.distributorID = Utility.IntParse(EditorExtension.GetValue<int>("DistributorID"));// Utility.IntParse(distributorID);
                    model.provinceID = string.Empty; //Utility.StringParse(EditorExtension.GetValue<string>("ProvinceID"));// Utility.StringParse(provinceID);
                    model.regionID = Utility.StringParse(EditorExtension.GetValue<string>("RegionID"));// Utility.StringParse(regionID);
                    model.routeID = Utility.StringParse(EditorExtension.GetValue<string>("RouteID"));// Utility.StringParse(routeID);
                    model.saleSupID = string.Empty; //Utility.StringParse(EditorExtension.GetValue<string>("SalesSupID"));// Utility.StringParse(salesSupID);
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            #region LoadComboboxListItem
            model.ListRegion = ControllerHelper.GetListRegion(model.regionID); //model.regionID);
            if (string.IsNullOrEmpty(model.regionID) && model.ListRegion.Count == 1)
            {
                model.regionID = model.ListRegion.First().RegionID;
            }

            model.ListArea = ControllerHelper.GetListArea(model.regionID);
            if (string.IsNullOrEmpty(model.areaID) && model.ListArea.Count == 1)
            {
                model.areaID = model.ListArea.First().AreaID;
            }

            //model.ListProvince = new List<DMSProvince>();// ControllerHelper.GetListProvince(model.regionID, model.areaID);
            //if (string.IsNullOrEmpty(model.provinceID) && model.ListProvince.Count == 1)
            //{
            //    model.provinceID = model.ListProvince.First().ProvinceID;
            //}

            model.ListSForce = ControllerHelper.GetListSaleSup(model.regionID, model.areaID, model.provinceID, model.distributorID);
            if (string.IsNullOrEmpty(model.saleSupID) && model.ListSForce.Count == 1)
            {
                model.saleSupID = model.ListSForce.First().EmployeeID;
            }

            model.ListDistributor = ControllerHelper.GetListDistributor(model.regionID, model.areaID, model.provinceID, model.saleSupID);
            if (model.distributorID == 0 && model.ListDistributor.Count == 1)
            {
                model.distributorID = model.ListDistributor.First().DistributorID;
            }

            model.ListRoute = ControllerHelper.GetListRoute(model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID);
            if (string.IsNullOrEmpty(model.routeID) && model.ListRoute.Count == 1)
            {
                model.routeID = model.ListRoute.First().RouteID;
            }
            #endregion

            model.leaderID = string.Empty;
            model.employeeID = string.Empty;

            #region Set default select if drop have only one item
            #endregion

            if (
                !string.IsNullOrEmpty(strFromDate)
                )
            {
                model.listItem = Global.Context.pp_ReportVisit(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, model.employeeID, model.leaderID, SessionHelper.GetSession<string>("UserName")).ToList();

                foreach (pp_ReportVisitResult item in model.listItem)
                {
                    if (item.VisitDate.HasValue)
                    {
                        item.ImageFile = item.VisitDate.Value.ToString(Constant.imageDateFormat) + "/" + item.ImageFile;
                    }
                }
            }
            else
            {
                model.listItem = new List<pp_ReportVisitResult>();
            }
            SessionHelper.SetSession<ReportVisitVM>("ReportVisit", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportVisitExport");
            }

            if (act == "ExportExcelRawData")
            {
                return RedirectToAction("ReportVisitExportRAWData");
            }

            return View(model);
        }

        [Authorize]
        public PartialViewResult ReportVisitPartial()
        {
            List<pp_ReportVisitResult> model = SessionHelper.GetSession<ReportVisitVM>("ReportVisit").listItem;
            return PartialView("ReportVisitPartial", model);
        }

        #region ReportVisitExpor
        public ActionResult ReportVisitExport()
        {
            var model = SessionHelper.GetSession<ReportVisitVM>("ReportVisit").listItem;

            ControllerHelper.LogUserAction("Home", "ReportVisitExport", null);

            return PivotGridExtension.ExportToXlsx(ReportVisitSettings(), model);
        }

        private static PivotGridSettings ReportVisitSettings()
        {
            PivotGridSettings settings = new PivotGridSettings();

            settings.Name = "ReportVisit";
            settings.CallbackRouteValues = new { Controller = "Tracking", Action = "ReportVisitPartial" };
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowDragInCustomizationForm = false;
            settings.OptionsView.ColumnTotalsLocation = PivotTotalsLocation.Far;

            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ShowColumnGrandTotals = false;
            settings.OptionsView.ShowColumnTotals = false;
            settings.OptionsView.ShowContextMenus = false;
            settings.OptionsView.ShowCustomTotalsForSingleValues = false;
            settings.OptionsView.ShowFilterHeaders = false;
            settings.OptionsView.ShowFilterSeparatorBar = false;
            settings.OptionsView.ShowGrandTotalsForSingleValues = false;
            settings.OptionsView.ShowRowGrandTotalHeader = false;
            settings.OptionsView.ShowRowGrandTotals = false;
            settings.OptionsView.ShowRowTotals = false;
            settings.OptionsView.ShowTotalsForSingleValues = false;

            settings.OptionsView.ShowDataHeaders = false;


            settings.Groups.Add("RSMName - RegionName - ASMName - AreaName - ProvinceName - DistributorName - SaleSupID - RouteID - SalesmanID");


            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 0;

            //    field.FieldName = "RegionID";
            //    field.Caption = Utility.Phrase("RegionID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 1;

                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("RegionName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 2;

            //    field.FieldName = "RSMID";
            //    field.Caption = Utility.Phrase("RSMID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 3;

                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 4;

            //    field.FieldName = "AreaID";
            //    field.Caption = Utility.Phrase("AreaID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 5;

                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("AreaName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 6;

            //    field.FieldName = "ASMID";
            //    field.Caption = Utility.Phrase("ASMID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 7;

                field.FieldName = "ASMName";
                field.Caption = Utility.Phrase("ASMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 8;

            //    field.FieldName = "ProvinceID";
            //    field.Caption = Utility.Phrase("ProvinceID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 9;

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 10;

            //    field.FieldName = "DistributorCode";
            //    field.Caption = Utility.Phrase("DistributorCode");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 11;

                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("DistributorName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 12;

                field.FieldName = "SaleSupID";
                field.Caption = Utility.Phrase("SaleSupID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 13;

                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("SaleSupName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 14;

                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("RouteID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 15;

                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("RouteName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 16;

                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("SalesmanID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 17;

                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("SalesmanName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 18;
                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortDatePattern;

                field.FieldName = "VisitDate";
                field.Caption = Utility.Phrase("VisitDate");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 19;
                field.FieldName = "strIsMCP";
                field.Caption = Utility.Phrase("strIsMCP");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });




            //DATA AREA
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 20;
                field.FieldName = "VisitOrder";
                field.Caption = Utility.Phrase("VisitOrder");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 21;
                field.FieldName = "VisitOrderReal";
                field.Caption = Utility.Phrase("VisitOrderReal");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 22;
                field.FieldName = "OutletID";
                field.Caption = Utility.Phrase("OutletID");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 23;
                field.FieldName = "OutletName";
                field.Caption = Utility.Phrase("OutletName");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 24;
                field.FieldName = "Address";
                field.Caption = Utility.Phrase("Address");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 25;
                field.FieldName = "Mobile";
                field.Caption = Utility.Phrase("Mobile");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 26;
                field.FieldName = "StartTime";
                field.Caption = Utility.Phrase("StartTime");

                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 27;
                field.FieldName = "EndTime";
                field.Caption = Utility.Phrase("EndTime");

                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 28;
                field.FieldName = "strTimeSpanVisit";
                field.Caption = Utility.Phrase("strTimeSpanVisit");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 29;
                field.FieldName = "strTimeSpanMove";
                field.Caption = Utility.Phrase("strTimeSpanMove");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 30;
                field.FieldName = "Distance";
                field.Caption = Utility.Phrase("Distance");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 31;
                field.FieldName = "Lat";
                field.Caption = Utility.Phrase("Lat");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 32;
                field.FieldName = "Long";
                field.Caption = Utility.Phrase("Long");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });



            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 33;
                field.FieldName = "HasOrder";
                field.Caption = Utility.Phrase("HasOrder");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 34;
                field.FieldName = "TotalSKU";
                field.Caption = Utility.Phrase("TotalSKU");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 35;
                field.FieldName = "DropSize";
                field.Caption = Utility.Phrase("DropSize");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 36;
                field.FieldName = "TotalAmount";
                field.Caption = Utility.Phrase("TotalAmount");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 37;
                field.FieldName = "Reason";
                field.Caption = Utility.Phrase("Reason");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Right;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 38;
                field.FieldName = "GPSType";
                field.Caption = Utility.Phrase("GPSType");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 39;
                field.FieldName = "IsEnableAirPlaneMode";
                field.Caption = Utility.Phrase("IsEnableAirPlaneMode");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 40;
                field.FieldName = "IsEnableGPSMode";
                field.Caption = Utility.Phrase("IsEnableGPSMode");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 41;
                field.FieldName = "IsEnableNetworkMode";
                field.Caption = Utility.Phrase("IsEnableNetworkMode");

                field.CellStyle.HorizontalAlign = HorizontalAlign.Left;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.OptionsPager.RowsPerPage = 0;

            return settings;
        }
        #endregion

        public ActionResult ReportVisitExportRAWData()
        {
            var model = SessionHelper.GetSession<ReportVisitVM>("ReportVisit").listItem;

            ControllerHelper.LogUserAction("Report", "ReportVisitExport", null);

            return GridViewExtension.ExportToPdf(ReportVisitSettingsRAWData(), model);
        }

        private static GridViewSettings ReportVisitSettingsRAWData()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "ReportVisit";
            settings.KeyFieldName = "RouteID";
            settings.CallbackRouteValues = new { Controller = "Tracking", Action = "ReportVisitPartial" };
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;

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
            settings.Columns.Add(field =>
            {

                field.FieldName = "RSMID";
                field.Caption = Utility.Phrase("RSMID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
            });
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
            settings.Columns.Add(field =>
            {

                field.FieldName = "ASMID";
                field.Caption = Utility.Phrase("ASMID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ASMName";
                field.Caption = Utility.Phrase("ASMName");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ProvinceID";
                field.Caption = Utility.Phrase("ProvinceID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
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

        [Authorize]
        public ActionResult ReportVisitPartialSlideShow()
        {
            List<pp_ReportVisitResult> model = SessionHelper.GetSession<ReportVisitVM>("ReportVisit").listItem;
            return View("ReportVisitPartialSlideShow", model);
        }
        #endregion

        #region ReportSalesAssessment
        #region ReportSalesAssessment
        [Authorize]
        [CompressFilter]
        [ActionAuthorize("Tracking_ReportSalesAssessment", true)]
        public ActionResult ReportSalesAssessment(string strFromDate, string act, FormCollection formParam)//
        {
            ReportSalesAssessmentVM model = new ReportSalesAssessmentVM();
            #region Validate Data
            try
            {
                //Begin get
                //Set default value
                if (string.IsNullOrEmpty(strFromDate))
                {
                    model.areaID = string.Empty;
                    model.FromDate = DateTime.Today;
                    model.distributorID = 0;
                    model.provinceID = string.Empty;
                    model.regionID = string.Empty;
                    model.routeID = string.Empty;
                    model.saleSupID = string.Empty;
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                }
                else
                {
                    model.areaID = Utility.StringParse(EditorExtension.GetValue<string>("AreaID"));// Utility.StringParse(areaID);
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.distributorID = Utility.IntParse(EditorExtension.GetValue<int>("DistributorID"));// Utility.IntParse(distributorID);
                    model.provinceID = string.Empty; //Utility.StringParse(EditorExtension.GetValue<string>("ProvinceID"));// Utility.StringParse(provinceID);
                    model.regionID = Utility.StringParse(EditorExtension.GetValue<string>("RegionID"));// Utility.StringParse(regionID);
                    model.routeID = Utility.StringParse(EditorExtension.GetValue<string>("RouteID"));// Utility.StringParse(routeID);
                    model.saleSupID = Utility.StringParse(EditorExtension.GetValue<string>("SalesSupID"));// Utility.StringParse(salesSupID);
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            #region LoadComboboxListItem
            model.ListRegion = ControllerHelper.GetListRegion(model.regionID); //model.regionID);

            model.ListArea = ControllerHelper.GetListArea(model.regionID);

            model.ListProvince = new List<DMSProvince>();// ControllerHelper.GetListProvince(model.regionID, model.areaID);

            model.ListSForce = ControllerHelper.GetListSaleSup(model.regionID, model.areaID, model.provinceID, model.distributorID);

            model.ListDistributor = ControllerHelper.GetListDistributor(model.regionID, model.areaID, model.provinceID, model.saleSupID);

            model.ListRoute = ControllerHelper.GetListRoute(model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID);
            #endregion
            if (
                !string.IsNullOrEmpty(strFromDate)
                )
            {
                model.listItem = Global.Context.pp_ReportSalesAssessment(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, model.salesmanID, username).ToList();
            }
            else
            {
                model.listItem = new List<pp_ReportSalesAssessmentResult>();
            }
            SessionHelper.SetSession<ReportSalesAssessmentVM>("ReportSalesAssessment", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportSalesAssessmentExport");
            }

            if (act == "ExportExcelRawData")
            {
                return RedirectToAction("ReportSalesAssessmentExportRAWData");
            }
            return View(model);
        }

        [Authorize]
        [CompressFilter]
        public PartialViewResult ReportSalesAssessmentPartial()
        {
            List<pp_ReportSalesAssessmentResult> model = SessionHelper.GetSession<ReportSalesAssessmentVM>("ReportSalesAssessment").listItem;
            return PartialView("ReportSalesAssessmentPartial", model);
        }
        #endregion

        #region ReportSalesAssessmentExport
        public ActionResult ReportSalesAssessmentExport()
        {
            var model = SessionHelper.GetSession<ReportSalesAssessmentVM>("ReportSalesAssessment").listItem;
            return PivotGridExtension.ExportToXlsx(ReportSalesAssessmentSettings(), model);
        }

        public static PivotGridSettings ReportSalesAssessmentSettings()
        {
            PivotGridSettings settings = new PivotGridSettings();

            settings.Name = "ReportSalesAssessment";
            settings.CallbackRouteValues = new { Controller = "Tracking", Action = "ReportSalesAssessmentPartial" };
            //settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowExpand = true;
            //settings.OptionsView.ColumnTotalsLocation = PivotTotalsLocation.Far;

            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ShowColumnGrandTotals = false;
            settings.OptionsView.ShowColumnTotals = false;
            settings.OptionsView.ShowContextMenus = true;
            settings.OptionsView.ShowCustomTotalsForSingleValues = false;
            settings.OptionsView.ShowFilterHeaders = false;
            settings.OptionsView.ShowFilterSeparatorBar = false;
            settings.OptionsView.ShowGrandTotalsForSingleValues = false;
            settings.OptionsView.ShowRowGrandTotalHeader = false;
            settings.OptionsView.ShowRowGrandTotals = false;
            settings.OptionsView.ShowRowTotals = false;
            settings.OptionsView.ShowTotalsForSingleValues = false;
            settings.OptionsView.ShowDataHeaders = false;

            settings.OptionsPager.Position = PagerPosition.TopAndBottom;
            settings.OptionsPager.PagerAlign = DevExpress.Web.ASPxPivotGrid.PagerAlign.Right;
            settings.OptionsPager.ShowDisabledButtons = true;
            settings.OptionsPager.ShowNumericButtons = true;
            settings.OptionsPager.ShowSeparators = false;
            settings.OptionsPager.Summary.Visible = false;
            settings.OptionsPager.PageSizeItemSettings.Visible = true;
            settings.OptionsPager.PageSizeItemSettings.Position = PagerPageSizePosition.Left;

            settings.OptionsView.ShowHorizontalScrollBar = true;


            settings.Groups.Add("RSMName - RegionName - ASMName - AreaName - ProvinceName - DistributorName - SaleSupID - RouteID - SalesmanID");

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 0;

            //    field.FieldName = "RegionID";
            //    field.Caption = Utility.Phrase("RegionID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 1;
                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("RegionName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 2;

            //    field.FieldName = "RSMID";
            //    field.Caption = Utility.Phrase("RSMID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 3;
                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.Visible = false;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 4;

            //    field.FieldName = "AreaID";
            //    field.Caption = Utility.Phrase("AreaID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                // field.AreaIndex = 5;
                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("AreaName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.Visible = false;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });
            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 6;

            //    field.FieldName = "ASMID";
            //    field.Caption = Utility.Phrase("ASMID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    //field.AreaIndex = 7;

            //    field.FieldName = "ASMName";
            //    field.Caption = Utility.Phrase("ASMName");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //    field.HeaderStyle.Wrap = DefaultBoolean.True;
            //});

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 8;

            //    field.FieldName = "ProvinceID";
            //    field.Caption = Utility.Phrase("ProvinceID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 9;

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.Visible = false;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    //field.AreaIndex = 10;

            //    field.FieldName = "DistributorCode";
            //    field.Caption = Utility.Phrase("DistributorCode");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 11;

                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("DistributorName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    //field.AreaIndex = 12;

            //    field.FieldName = "SaleSupID";
            //    field.Caption = Utility.Phrase("SaleSupID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    //field.AreaIndex = 13;

            //    field.FieldName = "SaleSupName";
            //    field.Caption = Utility.Phrase("SaleSupName");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //    field.HeaderStyle.Wrap = DefaultBoolean.True;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 14;

                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("RouteID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
                field.Visible = false;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    //field.AreaIndex = 15;

            //    field.FieldName = "RouteName";
            //    field.Caption = Utility.Phrase("RouteName");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //    field.HeaderStyle.Wrap = DefaultBoolean.True;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 16;

                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("SalesmanID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
                field.Visible = false;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 17;

                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("SalesmanName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 18;
                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortDatePattern;

                field.FieldName = "VisitDate";
                field.Caption = Utility.Phrase("VisitDate");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 19;
                field.FieldName = "strIsMCP";
                field.Caption = Utility.Phrase("strIsMCP");

                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });


            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 19;
            //    field.FieldName = "FirstSyncTime";
            //    field.Caption = Utility.Phrase("FirstSyncTime");

            //    field.ValueFormat.Format = Utility.info.DateTimeFormat;
            //    field.ValueFormat.FormatType = FormatType.DateTime;
            //    field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //    field.HeaderStyle.Wrap = DefaultBoolean.True;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 20;
                field.FieldName = "FirstStartTimeAM";
                field.Caption = Utility.Phrase("FirstStartTimeAM");

                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 21;
            //    field.FieldName = "FirstStartTimePM";
            //    field.Caption = Utility.Phrase("FirstStartTimePM");

            //    field.ValueFormat.Format = Utility.info.DateTimeFormat;
            //    field.ValueFormat.FormatType = FormatType.DateTime;
            //    field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //    field.HeaderStyle.Wrap = DefaultBoolean.True;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 22;
                field.FieldName = "LastEndTime";
                field.Caption = Utility.Phrase("LastEndTime");

                field.ValueFormat.Format = Utility.info.DateTimeFormat;
                field.ValueFormat.FormatType = FormatType.DateTime;
                field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortTimePattern;
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
                field.HeaderStyle.Wrap = DefaultBoolean.True;
            });



            //DATA AREA
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 0;
                field.FieldName = "OutletMustVisit";
                field.Caption = Utility.Phrase("OutletMustVisit");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 1;
                field.FieldName = "OutletVisited";
                field.Caption = Utility.Phrase("OutletVisited");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 2;
                field.FieldName = "OrderCount";
                field.Caption = Utility.Phrase("OrderCount");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.DataArea;
            //    field.AreaIndex = 3;
            //    field.FieldName = "TotalSKU";
            //    field.Caption = Utility.Phrase("TotalSKU");

            //    field.CellFormat.FormatType = FormatType.Custom;
            //    field.CellFormat.FormatString = "###,##0.##";
            //    field.SummaryType = PivotSummaryType.Sum;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 4;
                field.FieldName = "TotalQuantity";
                field.Caption = Utility.Phrase("TotalQuantity");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 5;
                field.FieldName = "TotalAmount";
                field.Caption = Utility.Phrase("TotalAmount");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.DataArea;
            //    field.AreaIndex = 6;
            //    field.FieldName = "LPPC";
            //    field.Caption = Utility.Phrase("LPPC");

            //    field.CellFormat.FormatType = FormatType.Custom;
            //    field.CellFormat.FormatString = "###,##0.##";
            //    field.SummaryType = PivotSummaryType.Sum;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 7;
                field.FieldName = "SO_MCP";
                field.Caption = Utility.Phrase("SO_MCP");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.00";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 8;
                field.FieldName = "Visit_MCP";
                field.Caption = Utility.Phrase("Visit_MCP");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.00";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 15;
                field.FieldName = "MTDOutletMustVisit";
                field.Caption = Utility.Phrase("MTDOutletMustVisit");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 16;
                field.FieldName = "MTDOutletVisited";
                field.Caption = Utility.Phrase("MTDOutletVisited");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 17;
                field.FieldName = "MTDOrderCount";
                field.Caption = Utility.Phrase("MTDOrderCount");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 18;
                field.FieldName = "MTDTotalSKU";
                field.Caption = Utility.Phrase("MTDTotalSKU");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 19;
                field.FieldName = "MTDTotalQuantity";
                field.Caption = Utility.Phrase("MTDTotalQuantity");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 20;
                field.FieldName = "MTDTotalAmount";
                field.Caption = Utility.Phrase("MTDTotalAmount");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.DataArea;
            //    field.AreaIndex = 21;
            //    field.FieldName = "MTDLPPC";
            //    field.Caption = Utility.Phrase("MTDLPPC");

            //    field.CellFormat.FormatType = FormatType.Custom;
            //    field.CellFormat.FormatString = "###,##0.##";
            //    field.SummaryType = PivotSummaryType.Sum;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 22;
                field.FieldName = "MTDSO_MCP";
                field.Caption = Utility.Phrase("MTDSO_MCP");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.00";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 23;
                field.FieldName = "MTDVisit_MCP";
                field.Caption = Utility.Phrase("MTDVisit_MCP");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.00";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 24;
                field.FieldName = "MTDHasOrder";
                field.Caption = Utility.Phrase("MTDHasOrder");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,##0.##";
                field.SummaryType = PivotSummaryType.Sum;
            });

            settings.PreRender = (sender, e) =>
            {
                var pivotGrid = (MVCxPivotGrid)sender;
                pivotGrid.Styles.HeaderStyle.Font.Bold = true;
                pivotGrid.Styles.HeaderStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.CellStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.FieldValueStyle.Font.Size = new FontUnit(12, UnitType.Pixel);

                pivotGrid.Styles.TotalCellStyle.Font.Bold = true;
                pivotGrid.Styles.FilterAreaStyle.Font.Bold = true;
                pivotGrid.Styles.ColumnAreaStyle.Font.Bold = true;
            };
            settings.EndRefresh = (sender, e) =>
            {
                var pivotGrid = (MVCxPivotGrid)sender;
                pivotGrid.Styles.HeaderStyle.Font.Bold = true;
                pivotGrid.Styles.HeaderStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.CellStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.FieldValueStyle.Font.Size = new FontUnit(12, UnitType.Pixel);

                pivotGrid.Styles.TotalCellStyle.Font.Bold = true;
                pivotGrid.Styles.FilterAreaStyle.Font.Bold = true;
                pivotGrid.Styles.ColumnAreaStyle.Font.Bold = true;
            };

            settings.OptionsPager.RowsPerPage = 0;

            return settings;
        }
        #endregion

        #region ReportSalesAssessmentExportRAWData
        public ActionResult ReportSalesAssessmentExportRAWData()
        {
            var model = SessionHelper.GetSession<ReportSalesAssessmentVM>("ReportSalesAssessment").listItem;
            return GridViewExtension.ExportToXlsx(ReportSalesAssessmentSettingsRAWData(), model);
        }

        private static GridViewSettings ReportSalesAssessmentSettingsRAWData()
        {
            var settings = new GridViewSettings
            {
                Name = "ReportSalesAssessment",
                KeyFieldName = "SaleSupID",
                CallbackRouteValues =
                    new { Controller = "Home", Action = "ReportSalesAssessmentPartial" },
                Width = Unit.Percentage(100)
            };
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;

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
            settings.Columns.Add(field =>
            {

                field.FieldName = "RSMID";
                field.Caption = Utility.Phrase("RSMID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
            });
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
            settings.Columns.Add(field =>
            {

                field.FieldName = "ASMID";
                field.Caption = Utility.Phrase("ASMID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ASMName";
                field.Caption = Utility.Phrase("ASMName");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ProvinceID";
                field.Caption = Utility.Phrase("ProvinceID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
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
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstSyncTime";
                field.Caption = Utility.Phrase("FirstSyncTime");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstStartTimeAM";
                field.Caption = Utility.Phrase("FirstStartTimeAM");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "FirstStartTimePM";
                field.Caption = Utility.Phrase("FirstStartTimePM");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "LastEndTime";
                field.Caption = Utility.Phrase("LastEndTime");

            });
            //DATA AREA
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletMustVisit";
                field.Caption = Utility.Phrase("OutletMustVisit");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OutletVisited";
                field.Caption = Utility.Phrase("OutletVisited");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "OrderCount";
                field.Caption = Utility.Phrase("OrderCount");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalSKU";
                field.Caption = Utility.Phrase("TotalSKU");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalQuantity";
                field.Caption = Utility.Phrase("TotalQuantity");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalAmount";
                field.Caption = Utility.Phrase("TotalAmount");

            });
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "LPPC";
            //    field.Caption = Utility.Phrase("LPPC");

            //});
            settings.Columns.Add(field =>
            {
                field.FieldName = "SO_MCP";
                field.Caption = Utility.Phrase("SOMCP");

            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Visit_MCP";
                field.Caption = Utility.Phrase("VisitMCP");

            });
            //settings.Columns.Add(field =>
            //{
            //    field.FieldName = "OrderDistanceInvalid";
            //    field.Caption = Utility.Phrase("OrderDistanceInvalid");

            //});
            return settings;
        }
        #endregion

        #region ReportSalesAssessmentChart
        [CompressFilter]
        public ActionResult ReportSalesAssessmentChart(string groupby)
        {
            ReportSalesAssessmentVM model = SessionHelper.GetSession<ReportSalesAssessmentVM>("ReportSalesAssessment");

            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = model.listItem.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(MTDTotalAmount) as TotalAmount, SUM(MTDOrderCount) as OrderCount, SUM(MTDTotalSKU) as TotalSKU, SUM(MTDTotalQuantity) as TotalQuantity, AVERAGE(MTDLPPC) as LPPC, AVERAGE(MTDSO_MCP) as SOMCP, AVERAGE(MTDVisit_MCP) as VisitMCP)");
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

            #region Map Data by Filter
            //if (groupby == "Region")
            //{
            //    result =
            //    (
            //      from row in model.listItem
            //      group row by new { row.RegionID, row.RegionName } into g
            //      select new ReportSMVisitSummaryChartData()
            //      {
            //          Name = g.Key.RegionName,
            //          TotalAmount = g.Sum(x => x.MTDTotalAmount),
            //          OrderCount = g.Sum(x => x.MTDOrderCount),
            //          TotalSKU = g.Sum(x => x.MTDTotalSKU),
            //          TotalQuantity = g.Sum(x => x.MTDTotalQuantity),
            //          LPPC = g.Average(x => x.MTDLPPC),
            //          SOMCP = g.Average(x => x.MTDSO_MCP),
            //          VisitMCP = g.Average(x => x.MTDVisit_MCP),
            //      }
            //    ).ToList();
            //}
            //else if (groupby == "RSM")
            //{
            //    result =
            //    (
            //      from row in model.listItem
            //      group row by new { row.RSMID, row.RSMName } into g
            //      select new ReportSMVisitSummaryChartData()
            //      {
            //          Name = g.Key.RSMName,
            //          TotalAmount = g.Sum(x => x.MTDTotalAmount),
            //          OrderCount = g.Sum(x => x.MTDOrderCount),
            //          TotalSKU = g.Sum(x => x.MTDTotalSKU),
            //          TotalQuantity = g.Sum(x => x.MTDTotalQuantity),
            //          LPPC = g.Average(x => x.MTDLPPC),
            //          SOMCP = g.Average(x => x.MTDSO_MCP),
            //          VisitMCP = g.Average(x => x.MTDVisit_MCP),
            //      }
            //    ).ToList();
            //}
            //else if (groupby == "Area")
            //{
            //    result =
            //    (
            //      from row in model.listItem
            //      group row by new { row.AreaID, row.AreaName } into g
            //      select new ReportSMVisitSummaryChartData()
            //      {
            //          Name = g.Key.AreaName,
            //          TotalAmount = g.Sum(x => x.MTDTotalAmount),
            //          OrderCount = g.Sum(x => x.MTDOrderCount),
            //          TotalSKU = g.Sum(x => x.MTDTotalSKU),
            //          TotalQuantity = g.Sum(x => x.MTDTotalQuantity),
            //          LPPC = g.Average(x => x.MTDLPPC),
            //          SOMCP = g.Average(x => x.MTDSO_MCP),
            //          VisitMCP = g.Average(x => x.MTDVisit_MCP),
            //      }
            //    ).ToList();
            //}
            //else if (groupby == "ASM")
            //{
            //    result =
            //    (
            //      from row in model.listItem
            //      group row by new { row.ASMID, row.ASMName } into g
            //      select new ReportSMVisitSummaryChartData()
            //      {
            //          Name = g.Key.ASMName,
            //          TotalAmount = g.Sum(x => x.TotalAmount),
            //          OrderCount = g.Sum(x => x.OrderCount),
            //          TotalSKU = g.Sum(x => x.TotalSKU),
            //          TotalQuantity = g.Sum(x => x.TotalQuantity),
            //          LPPC = g.Average(x => x.LPPC),
            //          SOMCP = g.Average(x => x.SO_MCP),
            //          VisitMCP = g.Average(x => x.Visit_MCP),
            //      }
            //    ).ToList();
            //}
            //else if (groupby == "SS")
            //{
            //    result =
            //    (
            //      from row in model.listItem
            //      group row by new { row.SaleSupID, row.SaleSupName } into g
            //      select new ReportSMVisitSummaryChartData()
            //      {
            //          Name = g.Key.SaleSupName,
            //          TotalAmount = g.Sum(x => x.MTDTotalAmount),
            //          OrderCount = g.Sum(x => x.MTDOrderCount),
            //          TotalSKU = g.Sum(x => x.MTDTotalSKU),
            //          TotalQuantity = g.Sum(x => x.MTDTotalQuantity),
            //          LPPC = g.Average(x => x.MTDLPPC),
            //          SOMCP = g.Average(x => x.MTDSO_MCP),
            //          VisitMCP = g.Average(x => x.MTDVisit_MCP),
            //      }
            //    ).ToList();
            //}
            //else if (groupby == "NPP")
            //{
            //    result =
            //    (
            //      from row in model.listItem
            //      group row by new { row.DistributorCode, row.DistributorName } into g
            //      select new ReportSMVisitSummaryChartData()
            //      {
            //          Name = g.Key.DistributorName,
            //          TotalAmount = g.Sum(x => x.MTDTotalAmount),
            //          OrderCount = g.Sum(x => x.MTDOrderCount),
            //          TotalSKU = g.Sum(x => x.MTDTotalSKU),
            //          TotalQuantity = g.Sum(x => x.MTDTotalQuantity),
            //          LPPC = g.Average(x => x.MTDLPPC),
            //          SOMCP = g.Average(x => x.MTDSO_MCP),
            //          VisitMCP = g.Average(x => x.MTDVisit_MCP),
            //      }
            //    ).ToList();
            //}
            //else if (groupby == "Route")
            //{
            //    result =
            //    (
            //      from row in model.listItem
            //      group row by new { row.RouteID, row.RouteName } into g
            //      select new ReportSMVisitSummaryChartData()
            //      {
            //          Name = g.Key.RouteID,
            //          TotalAmount = g.Sum(x => x.MTDTotalAmount),
            //          OrderCount = g.Sum(x => x.MTDOrderCount),
            //          TotalSKU = g.Sum(x => x.MTDTotalSKU),
            //          TotalQuantity = g.Sum(x => x.MTDTotalQuantity),
            //          LPPC = g.Average(x => x.MTDLPPC),
            //          SOMCP = g.Average(x => x.MTDSO_MCP),
            //          VisitMCP = g.Average(x => x.MTDVisit_MCP),
            //      }
            //    ).ToList();
            //}
            //else if (groupby == "Salesman")
            //{
            //    result =
            //    (
            //      from row in model.listItem
            //      group row by new { row.SalesmanID, row.SalesmanName } into g
            //      select new ReportSMVisitSummaryChartData()
            //      {
            //          Name = g.Key.SalesmanName,
            //          TotalAmount = g.Sum(x => x.MTDTotalAmount),
            //          OrderCount = g.Sum(x => x.MTDOrderCount),
            //          TotalSKU = g.Sum(x => x.MTDTotalSKU),
            //          TotalQuantity = g.Sum(x => x.MTDTotalQuantity),
            //          LPPC = g.Average(x => x.MTDLPPC),
            //          SOMCP = g.Average(x => x.MTDSO_MCP),
            //          VisitMCP = g.Average(x => x.MTDVisit_MCP),
            //      }
            //    ).ToList();
            //}
            #endregion

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
            chartData.chartName = Utility.Phrase("ChartMTD");
            chartData.YName = "";
            #endregion

            return Json(new
            {
                chartData
            }, JsonRequestBehavior.AllowGet);
            //return View();
        }

        [CompressFilter]
        public ActionResult ReportSalesAssessmentChartDaily(string groupby)
        {
            ReportSalesAssessmentVM model = SessionHelper.GetSession<ReportSalesAssessmentVM>("ReportSalesAssessment");

            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = model.listItem.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(OrderCount) as OrderCount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, AVERAGE(SO_MCP) as SOMCP, AVERAGE(Visit_MCP) as VisitMCP)");
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

        [CompressFilter]
        public ActionResult ReportSalesAssessmentChartPie(string groupby, string totalby)//
        {
            ReportSalesAssessmentVM model = SessionHelper.GetSession<ReportSalesAssessmentVM>("ReportSalesAssessment");

            #region Prepare Data For Chart
            //string totalby = "TotalAmount";
            //groupby = "RouteID";
            var result = new List<PieColumnData>();
            //decimal total = model.listItem.Sum(a=>a.TotalAmount);
            decimal total = (decimal)model.listItem.AsQueryable<pp_ReportSalesAssessmentResult>().Sum(totalby);
            //var queryTotal = model.listItem.AsQueryable().Sum("new (SUM(" + totalby + ") as y)");

            if (total > 0)
            {
                var dynamicQuery = model.listItem.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as name,SUM(" + totalby + ") as y)");
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
            //return View();
        }
        #endregion
        #endregion

        #region ReportOutletInvalidLocation
        #region ReportOutletInvalidLocation
        [Authorize]
        [ActionAuthorize("Tracking_ReportOutletInvalidLocation", true)]
        public ActionResult ReportOutletInvalidLocation(string strFromDate, string act, FormCollection formParam)//
        {
            ReportOutletInvalidLocationVM model = new ReportOutletInvalidLocationVM();
            //model.listItem = Global.Context.pp_GetOutletInvalidLocation().ToList();
            DataSet ds = new DataSet();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("pp_GetOutletInvalidLocation", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 300;
                    //// Add parameters
                    cmd.Parameters.AddWithValue("@UserName", username);
                    var adapt = new SqlDataAdapter(cmd);
                    adapt.Fill(ds);
                    if (ds != null && ds.Tables.Count >= 1)
                    {
                        model.listItem = ds.Tables[0];
                    }
                    else
                    {
                        model.listItem = new DataTable();
                    }
                }
            }
            SessionHelper.SetSession<ReportOutletInvalidLocationVM>("ReportOutletInvalidLocation", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportOutletInvalidLocationExport");
            }

            if (act == "ExportExcelRawData")
            {
                return RedirectToAction("ReportOutletInvalidLocationExportRAWData");
            }
            return View(model);
        }

        [Authorize]
        public PartialViewResult ReportOutletInvalidLocationPartial()
        {
            var model = SessionHelper.GetSession<ReportOutletInvalidLocationVM>("ReportOutletInvalidLocation").listItem;
            return PartialView("ReportOutletInvalidLocationPartial", model);
        }
        #endregion

        #region ReportOutletInvalidLocationExport

        public ActionResult ReportOutletInvalidLocationExport()
        {
            var model = SessionHelper.GetSession<ReportOutletInvalidLocationVM>("ReportOutletInvalidLocation").listItem;
            return PivotGridExtension.ExportToXlsx(ReportOutletInvalidLocationSettings(), model);
        }

        public static PivotGridSettings ReportOutletInvalidLocationSettings()
        {
            PivotGridSettings settings = new PivotGridSettings();

            settings.Name = "ReportOutletInvalidLocation";
            settings.CallbackRouteValues = new { Controller = "Tracking", Action = "ReportOutletInvalidLocationPartial" };
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsCustomization.AllowDrag = true;
            settings.OptionsCustomization.AllowDragInCustomizationForm = true;
            settings.OptionsView.ColumnTotalsLocation = PivotTotalsLocation.Far;

            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ShowColumnGrandTotals = false;
            settings.OptionsView.ShowColumnTotals = false;
            settings.OptionsView.ShowContextMenus = true;
            settings.OptionsView.ShowCustomTotalsForSingleValues = false;
            settings.OptionsView.ShowFilterHeaders = false;
            settings.OptionsView.ShowFilterSeparatorBar = false;
            settings.OptionsView.ShowGrandTotalsForSingleValues = false;
            settings.OptionsView.ShowRowGrandTotalHeader = false;
            settings.OptionsView.ShowRowGrandTotals = false;
            settings.OptionsView.ShowRowTotals = false;
            settings.OptionsView.ShowTotalsForSingleValues = false;
            settings.OptionsView.ShowDataHeaders = true;

            settings.OptionsPager.Position = PagerPosition.TopAndBottom;
            settings.OptionsPager.PagerAlign = DevExpress.Web.ASPxPivotGrid.PagerAlign.Right;
            settings.OptionsPager.ShowDisabledButtons = true;
            settings.OptionsPager.ShowNumericButtons = true;
            settings.OptionsPager.ShowSeparators = false;
            settings.OptionsPager.Summary.Visible = false;
            settings.OptionsPager.PageSizeItemSettings.Visible = true;
            settings.OptionsPager.PageSizeItemSettings.Position = PagerPageSizePosition.Left;


            settings.Groups.Add("RSMName - RegionName - ASMName - AreaName - ProvinceName - DistributorName - SaleSupID - RouteID - SalesmanID");

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 1;

                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("RegionName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 2;

            //    field.FieldName = "RSMID";
            //    field.Caption = Utility.Phrase("RSMID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 3;

                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 4;

            //    field.FieldName = "AreaID";
            //    field.Caption = Utility.Phrase("AreaID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 5;

                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("AreaName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 6;

            //    field.FieldName = "ASMID";
            //    field.Caption = Utility.Phrase("ASMID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 7;

                field.FieldName = "ASMName";
                field.Caption = Utility.Phrase("ASMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 8;

            //    field.FieldName = "ProvinceID";
            //    field.Caption = Utility.Phrase("ProvinceID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 9;

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    field.AreaIndex = 10;

            //    field.FieldName = "DistributorCode";
            //    field.Caption = Utility.Phrase("DistributorCode");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 11;

                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("DistributorName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 12;

                field.FieldName = "SaleSupID";
                field.Caption = Utility.Phrase("SaleSupID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 13;

                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("SaleSupName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 14;

                field.FieldName = "RouteID";
                field.Caption = Utility.Phrase("RouteID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 15;

                field.FieldName = "RouteName";
                field.Caption = Utility.Phrase("RouteName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 16;

                field.FieldName = "SalesmanID";
                field.Caption = Utility.Phrase("SalesmanID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 17;

                field.FieldName = "SalesmanName";
                field.Caption = Utility.Phrase("SalesmanName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 18;

                field.FieldName = "OutletID";
                field.Caption = Utility.Phrase("OutletID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 19;

                field.FieldName = "OutletName";
                field.Caption = Utility.Phrase("OutletName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 20;

                field.FieldName = "Address";
                field.Caption = Utility.Phrase("Address");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 21;

                field.FieldName = "Longtitude";
                field.Caption = Utility.Phrase("Longtitude");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 22;

                field.FieldName = "Latitude";
                field.Caption = Utility.Phrase("Latitude");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 23;

                field.FieldName = "Phone";
                field.Caption = Utility.Phrase("Phone");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 24;

                field.FieldName = "Mobile";
                field.Caption = Utility.Phrase("Mobile");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 25;

                field.FieldName = "Status";
                field.Caption = Utility.Phrase("Status");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.PreRender = (sender, e) =>
            {
                var pivotGrid = (MVCxPivotGrid)sender;
                pivotGrid.Styles.HeaderStyle.Font.Bold = true;
                pivotGrid.Styles.HeaderStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.CellStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.FieldValueStyle.Font.Size = new FontUnit(12, UnitType.Pixel);

                pivotGrid.Styles.TotalCellStyle.Font.Bold = true;
                pivotGrid.Styles.FilterAreaStyle.Font.Bold = true;
                pivotGrid.Styles.ColumnAreaStyle.Font.Bold = true;
            };
            settings.EndRefresh = (sender, e) =>
            {
                var pivotGrid = (MVCxPivotGrid)sender;
                pivotGrid.Styles.HeaderStyle.Font.Bold = true;
                pivotGrid.Styles.HeaderStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.CellStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.FieldValueStyle.Font.Size = new FontUnit(12, UnitType.Pixel);

                pivotGrid.Styles.TotalCellStyle.Font.Bold = true;
                pivotGrid.Styles.FilterAreaStyle.Font.Bold = true;
                pivotGrid.Styles.ColumnAreaStyle.Font.Bold = true;
            };

            //settings.OptionsPager.RowsPerPage = 100;

            return settings;
        }
        #endregion

        #region ReportOutletInvalidLocationExportRAWData
        public ActionResult ReportOutletInvalidLocationExportRAWData()
        {
            var model = SessionHelper.GetSession<ReportOutletInvalidLocationVM>("ReportOutletInvalidLocation").listItem;
            return GridViewExtension.ExportToXlsx(ReportOutletInvalidLocationSettingsRAWData(), model);
        }

        private static GridViewSettings ReportOutletInvalidLocationSettingsRAWData()
        {
            var settings = new GridViewSettings
            {
                Name = "ReportOutletInvalidLocation",
                KeyFieldName = "OutletID",
                CallbackRouteValues =
                    new { Controller = "Tracking", Action = "ReportOutletInvalidLocationPartial" },
                Width = Unit.Percentage(100)
            };
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;

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
            settings.Columns.Add(field =>
            {

                field.FieldName = "RSMID";
                field.Caption = Utility.Phrase("RSMID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
            });
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
            settings.Columns.Add(field =>
            {

                field.FieldName = "ASMID";
                field.Caption = Utility.Phrase("ASMID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ASMName";
                field.Caption = Utility.Phrase("ASMName");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ProvinceID";
                field.Caption = Utility.Phrase("ProvinceID");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
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

                field.FieldName = "Longtitude";
                field.Caption = Utility.Phrase("Longtitude");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "Latitude";
                field.Caption = Utility.Phrase("Latitude");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "Phone";
                field.Caption = Utility.Phrase("Phone");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "Mobile";
                field.Caption = Utility.Phrase("Mobile");
            });
            settings.Columns.Add(field =>
            {

                field.FieldName = "Status";
                field.Caption = Utility.Phrase("Status");
            });

            return settings;
        }
        #endregion
        #endregion

        #region ReportPC
        #region ReportPC
        [Authorize]
        [ActionAuthorize("Tracking_ReportPC", true)]
        public ActionResult ReportPC(string strFromDate, string strToDate, string act, string IsGreater, string strPercent, string showALL, FormCollection formParam)//
        {
            ReportPCVM model = new ReportPCVM();
            #region Validate Data
            try
            {
                //Begin get
                //Set default value
                if (string.IsNullOrEmpty(strFromDate))
                {
                    model.areaID = string.Empty;
                    model.FromDate = DateTime.Today;
                    model.distributorID = 0;
                    model.provinceID = string.Empty;
                    model.regionID = string.Empty;
                    model.routeID = string.Empty;
                    model.saleSupID = string.Empty;
                    model.ToDate = DateTime.Today;
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.strToDate = model.ToDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                    model.report = "ReportPC_SM";//string.Empty;
                    model.IsGreater = 1;
                    model.Percent = 0;
                    model.showAll = 2;
                }
                else
                {
                    model.areaID = Utility.StringParse(EditorExtension.GetValue<string>("AreaID"));// Utility.StringParse(areaID);
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.distributorID = Utility.IntParse(EditorExtension.GetValue<int>("DistributorID"));// Utility.IntParse(distributorID);
                    model.provinceID = string.Empty; //Utility.StringParse(EditorExtension.GetValue<string>("ProvinceID"));// Utility.StringParse(provinceID);
                    model.regionID = Utility.StringParse(EditorExtension.GetValue<string>("RegionID"));// Utility.StringParse(regionID);
                    model.routeID = Utility.StringParse(EditorExtension.GetValue<string>("RouteID"));// Utility.StringParse(routeID);
                    model.saleSupID = Utility.StringParse(EditorExtension.GetValue<string>("SalesSupID"));// Utility.StringParse(salesSupID);
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.ToDate = Utility.DateTimeParse(strToDate);
                    model.strToDate = model.ToDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                    model.report = Utility.StringParse(EditorExtension.GetValue<string>("report"));
                    model.IsGreater = Utility.IntParse(IsGreater);
                    model.Percent = Utility.IntParse(strPercent);
                    model.showAll = Utility.IntParse(showALL);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            #region LoadComboboxListItem
            model.ListRegion = ControllerHelper.GetListRegion(model.regionID); //model.regionID);
            model.ListArea = ControllerHelper.GetListArea(model.regionID);
            model.ListProvince = new List<DMSProvince>();// ControllerHelper.GetListProvince(model.regionID, model.areaID);
            model.ListSForce = ControllerHelper.GetListSaleSup(model.regionID, model.areaID, model.provinceID, model.distributorID);
            model.ListDistributor = ControllerHelper.GetListDistributor(model.regionID, model.areaID, model.provinceID, model.saleSupID);
            model.ListRoute = ControllerHelper.GetListRoute(model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID);
            #endregion
            if (
                !string.IsNullOrEmpty(strFromDate)
                )
            {
                if (model.report == "ReportPC_SM")
                {
                    model.listItemSM = Global.Context.pp_ReportPC_SM(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, 2, 0, 1, username).ToList();
                }
                else if (model.report == "ReportPC_SM_Daily")
                {
                    model.listItemSM_Daily = Global.Context.pp_ReportPC_SM_Daily(model.FromDate, model.ToDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, 2, 0, 1, username).ToList();
                }
                else if (model.report == "ReportPC_MTD")
                {
                    model.listItemMTD = Global.Context.pp_ReportPC_MTD(model.FromDate, model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID, model.routeID, 2, username).ToList();
                }
            }
            else
            {
                model.listItemSM = new List<pp_ReportPC_SMResult>();
                model.listItemSM_Daily = new List<pp_ReportPC_SM_DailyResult>();
                model.listItemMTD = new List<pp_ReportPC_MTDResult>();
            }
            SessionHelper.SetSession<ReportPCVM>("ReportPC", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportPCExport");
            }

            if (act == "ExportExcelRawData")
            {
                return RedirectToAction("ReportPCExportRAWData");
            }
            return View(model);
        }

        [Authorize]
        public PartialViewResult ReportPCPartial()
        {
            var model = SessionHelper.GetSession<ReportPCVM>("ReportPC");
            if (model.report == "ReportPC_SM")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemSM;
                return PartialView("ReportPCPartial", listItem);
            }
            else if (model.report == "ReportPC_SM_Daily")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemSM_Daily;
                return PartialView("ReportPCPartial", listItem);
            }
            else if (model.report == "ReportPC_MTD")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemMTD;
                return PartialView("ReportPCPartial", listItem);
            }
            return PartialView();
        }
        #endregion

        #region ReportPCExport
        public ActionResult ReportPCExport()
        {
            var model = SessionHelper.GetSession<ReportPCVM>("ReportPC");
            if (model.report == "ReportPC_SM")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemSM;
                return PivotGridExtension.ExportToXlsx(ReportPCSettings(), listItem);
            }
            else if (model.report == "ReportPC_SM_Daily")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemSM_Daily;
                return PivotGridExtension.ExportToXlsx(ReportPCSettings(), listItem);
            }
            else if (model.report == "ReportPC_MTD")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemMTD;
                return PivotGridExtension.ExportToXlsx(ReportPCSettings(), listItem);
            }
            return View();
        }

        public static PivotGridSettings ReportPCSettings()
        {
            var model = SessionHelper.GetSession<ReportPCVM>("ReportPC");

            #region Construct
            PivotGridSettings settings = new PivotGridSettings();
            settings.Name = model.report;
            settings.CallbackRouteValues = new { Controller = "Tracking", Action = "ReportPCPartial" };
            //settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowExpand = true;
            //settings.OptionsView.ColumnTotalsLocation = PivotTotalsLocation.Far;

            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ShowColumnGrandTotals = false;
            settings.OptionsView.ShowColumnTotals = false;
            settings.OptionsView.ShowContextMenus = true;
            settings.OptionsView.ShowCustomTotalsForSingleValues = false;
            settings.OptionsView.ShowFilterHeaders = false;
            settings.OptionsView.ShowFilterSeparatorBar = false;
            settings.OptionsView.ShowGrandTotalsForSingleValues = false;
            settings.OptionsView.ShowRowGrandTotalHeader = false;
            settings.OptionsView.ShowRowGrandTotals = false;
            settings.OptionsView.ShowRowTotals = false;
            settings.OptionsView.ShowTotalsForSingleValues = false;
            settings.OptionsView.ShowDataHeaders = false;

            settings.OptionsPager.Position = PagerPosition.TopAndBottom;
            settings.OptionsPager.PagerAlign = DevExpress.Web.ASPxPivotGrid.PagerAlign.Right;
            settings.OptionsPager.ShowDisabledButtons = true;
            settings.OptionsPager.ShowNumericButtons = true;
            settings.OptionsPager.ShowSeparators = false;
            settings.OptionsPager.Summary.Visible = false;
            settings.OptionsPager.PageSizeItemSettings.Visible = true;
            settings.OptionsPager.PageSizeItemSettings.Position = PagerPageSizePosition.Left;

            settings.OptionsView.ShowHorizontalScrollBar = true;

            #endregion

            #region ReportPC_SM
            if (model.report == "ReportPC_SM")
            {
                settings.Groups.Add("RSMName - RegionName - ASMName - AreaName - ProvinceName - DistributorName - SaleSupID - RouteID - SalesmanID");

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 0;

                //    field.FieldName = "RegionID";
                //    field.Caption = Utility.Phrase("RegionID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 1;
                    field.FieldName = "RegionName";
                    field.Caption = Utility.Phrase("RegionName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 2;

                //    field.FieldName = "RSMID";
                //    field.Caption = Utility.Phrase("RSMID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 3;
                    field.FieldName = "RSMName";
                    field.Caption = Utility.Phrase("RSMName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 4;

                //    field.FieldName = "AreaID";
                //    field.Caption = Utility.Phrase("AreaID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    // field.AreaIndex = 5;
                    field.FieldName = "AreaName";
                    field.Caption = Utility.Phrase("AreaName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });
                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 6;

                //    field.FieldName = "ASMID";
                //    field.Caption = Utility.Phrase("ASMID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 7;

                //    field.FieldName = "ASMName";
                //    field.Caption = Utility.Phrase("ASMName");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //    field.HeaderStyle.Wrap = DefaultBoolean.True;
                //});

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 8;

                //    field.FieldName = "ProvinceID";
                //    field.Caption = Utility.Phrase("ProvinceID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 9;

                    field.FieldName = "ProvinceName";
                    field.Caption = Utility.Phrase("ProvinceName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 10;

                //    field.FieldName = "DistributorCode";
                //    field.Caption = Utility.Phrase("DistributorCode");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 11;

                    field.FieldName = "DistributorName";
                    field.Caption = Utility.Phrase("DistributorName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 12;

                //    field.FieldName = "SaleSupID";
                //    field.Caption = Utility.Phrase("SaleSupID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 13;

                //    field.FieldName = "SaleSupName";
                //    field.Caption = Utility.Phrase("SaleSupName");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //    field.HeaderStyle.Wrap = DefaultBoolean.True;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 14;

                    field.FieldName = "RouteID";
                    field.Caption = Utility.Phrase("RouteID");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                    field.Visible = false;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 15;

                //    field.FieldName = "RouteName";
                //    field.Caption = Utility.Phrase("RouteName");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //    field.HeaderStyle.Wrap = DefaultBoolean.True;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 16;

                    field.FieldName = "SalesmanID";
                    field.Caption = Utility.Phrase("SalesmanID");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                    field.Visible = false;
                });

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 17;

                    field.FieldName = "SalesmanName";
                    field.Caption = Utility.Phrase("SalesmanName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    field.AreaIndex = 18;
                    field.ValueFormat.Format = Utility.info.DateTimeFormat;
                    field.ValueFormat.FormatType = FormatType.DateTime;
                    field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortDatePattern;

                    field.FieldName = "VisitDate";
                    field.Caption = Utility.Phrase("VisitDate");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                });


                //DATA AREA
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 0;
                    field.FieldName = "PCPass";
                    field.Caption = Utility.Phrase("PCPass");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.DataArea;
                //    field.AreaIndex = 1;
                //    field.FieldName = "PCNotPass";
                //    field.Caption = Utility.Phrase("PCNotPass");

                //    field.CellFormat.FormatType = FormatType.Custom;
                //    field.CellFormat.FormatString = "###,##0.##";
                //    field.SummaryType = PivotSummaryType.Sum;
                //});
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 2;
                    field.FieldName = "MCP";
                    field.Caption = Utility.Phrase("MCP");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 3;
                    field.FieldName = "PC";
                    field.Caption = Utility.Phrase("PC");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 4;
                    field.FieldName = "PC_MCP";
                    field.Caption = Utility.Phrase("PC_MCP");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.00";
                    field.SummaryType = PivotSummaryType.Average;
                });

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 5;
                    field.FieldName = "MTDPCPass";
                    field.Caption = Utility.Phrase("MTDPCPass");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.DataArea;
                //    field.AreaIndex = 6;
                //    field.FieldName = "MTDPCNotPass";
                //    field.Caption = Utility.Phrase("MTDPCNotPass");

                //    field.CellFormat.FormatType = FormatType.Custom;
                //    field.CellFormat.FormatString = "###,##0.##";
                //    field.SummaryType = PivotSummaryType.Sum;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 7;
                    field.FieldName = "ActualManday";
                    field.Caption = Utility.Phrase("ActualManday");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 8;
                    field.FieldName = "MTDMCP";
                    field.Caption = Utility.Phrase("MTDMCP");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 9;
                    field.FieldName = "MTDPC";
                    field.Caption = Utility.Phrase("MTDPC");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 10;
                    field.FieldName = "MTD_AVG_PC";
                    field.Caption = Utility.Phrase("MTD_AVG_PC");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.00";
                    field.SummaryType = PivotSummaryType.Average;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 11;
                    field.FieldName = "MTDPC_MCP";
                    field.Caption = Utility.Phrase("MTDPC_MCP");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.00";
                    field.SummaryType = PivotSummaryType.Average;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 12;
                    field.FieldName = "MTDLPPC";
                    field.Caption = Utility.Phrase("MTDLPPC");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.00";
                    field.SummaryType = PivotSummaryType.Average;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 13;
                    field.FieldName = "MTDTotalSKU";
                    field.Caption = Utility.Phrase("MTDTotalSKU");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
            }
            #endregion

            #region ReportPC_SM_Daily
            if (model.report == "ReportPC_SM_Daily")
            {
                settings.Groups.Add("RSMName - RegionName - ASMName - AreaName - ProvinceName - DistributorName - SaleSupID - RouteID - SalesmanID");

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 0;

                //    field.FieldName = "RegionID";
                //    field.Caption = Utility.Phrase("RegionID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 1;
                    field.FieldName = "RegionName";
                    field.Caption = Utility.Phrase("RegionName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 2;

                //    field.FieldName = "RSMID";
                //    field.Caption = Utility.Phrase("RSMID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 3;
                    field.FieldName = "RSMName";
                    field.Caption = Utility.Phrase("RSMName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 4;

                //    field.FieldName = "AreaID";
                //    field.Caption = Utility.Phrase("AreaID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    // field.AreaIndex = 5;
                    field.FieldName = "AreaName";
                    field.Caption = Utility.Phrase("AreaName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });
                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 6;

                //    field.FieldName = "ASMID";
                //    field.Caption = Utility.Phrase("ASMID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 7;

                //    field.FieldName = "ASMName";
                //    field.Caption = Utility.Phrase("ASMName");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //    field.HeaderStyle.Wrap = DefaultBoolean.True;
                //});

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 8;

                //    field.FieldName = "ProvinceID";
                //    field.Caption = Utility.Phrase("ProvinceID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 9;

                    field.FieldName = "ProvinceName";
                    field.Caption = Utility.Phrase("ProvinceName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 10;

                //    field.FieldName = "DistributorCode";
                //    field.Caption = Utility.Phrase("DistributorCode");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 11;

                    field.FieldName = "DistributorName";
                    field.Caption = Utility.Phrase("DistributorName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 12;

                //    field.FieldName = "SaleSupID";
                //    field.Caption = Utility.Phrase("SaleSupID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 13;

                //    field.FieldName = "SaleSupName";
                //    field.Caption = Utility.Phrase("SaleSupName");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //    field.HeaderStyle.Wrap = DefaultBoolean.True;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 14;

                    field.FieldName = "RouteID";
                    field.Caption = Utility.Phrase("RouteID");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                    field.Visible = false;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 15;

                //    field.FieldName = "RouteName";
                //    field.Caption = Utility.Phrase("RouteName");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //    field.HeaderStyle.Wrap = DefaultBoolean.True;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 16;

                    field.FieldName = "SalesmanID";
                    field.Caption = Utility.Phrase("SalesmanID");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                    field.Visible = false;
                });

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 17;

                    field.FieldName = "SalesmanName";
                    field.Caption = Utility.Phrase("SalesmanName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    field.AreaIndex = 18;
                    field.ValueFormat.Format = Utility.info.DateTimeFormat;
                    field.ValueFormat.FormatType = FormatType.DateTime;
                    field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortDatePattern;

                    field.FieldName = "VisitDate";
                    field.Caption = Utility.Phrase("VisitDate");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                });


                //DATA AREA
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 0;
                    field.FieldName = "PCPass";
                    field.Caption = Utility.Phrase("PCPass");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 1;
                    field.FieldName = "PCNotPass";
                    field.Caption = Utility.Phrase("PCNotPass");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 2;
                    field.FieldName = "MCP";
                    field.Caption = Utility.Phrase("MCP");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 3;
                    field.FieldName = "PC";
                    field.Caption = Utility.Phrase("PC");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 4;
                    field.FieldName = "PC_MCP";
                    field.Caption = Utility.Phrase("PC_MCP");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.00";
                    field.SummaryType = PivotSummaryType.Average;
                });
            }
            #endregion

            #region ReportPC_MTD
            if (model.report == "ReportPC_MTD")
            {
                settings.Groups.Add("RSMName - RegionName - ASMName - AreaName - ProvinceName - DistributorName - SaleSupID");

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 0;

                //    field.FieldName = "RegionID";
                //    field.Caption = Utility.Phrase("RegionID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 1;
                    field.FieldName = "RegionName";
                    field.Caption = Utility.Phrase("RegionName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 2;

                //    field.FieldName = "RSMID";
                //    field.Caption = Utility.Phrase("RSMID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 3;
                    field.FieldName = "RSMName";
                    field.Caption = Utility.Phrase("RSMName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 4;

                //    field.FieldName = "AreaID";
                //    field.Caption = Utility.Phrase("AreaID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    // field.AreaIndex = 5;
                    field.FieldName = "AreaName";
                    field.Caption = Utility.Phrase("AreaName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });
                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 6;

                //    field.FieldName = "ASMID";
                //    field.Caption = Utility.Phrase("ASMID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 7;

                //    field.FieldName = "ASMName";
                //    field.Caption = Utility.Phrase("ASMName");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //    field.HeaderStyle.Wrap = DefaultBoolean.True;
                //});

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    field.AreaIndex = 8;

                //    field.FieldName = "ProvinceID";
                //    field.Caption = Utility.Phrase("ProvinceID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 9;

                    field.FieldName = "ProvinceName";
                    field.Caption = Utility.Phrase("ProvinceName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.Visible = false;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 10;

                //    field.FieldName = "DistributorCode";
                //    field.Caption = Utility.Phrase("DistributorCode");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 11;

                    field.FieldName = "DistributorName";
                    field.Caption = Utility.Phrase("DistributorName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                //settings.Fields.Add(field =>
                //{
                //    field.Area = PivotArea.RowArea;
                //    //field.AreaIndex = 12;

                //    field.FieldName = "SaleSupID";
                //    field.Caption = Utility.Phrase("SaleSupID");
                //    field.RunningTotal = false;
                //    field.TotalsVisibility = PivotTotalsVisibility.None;
                //});

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    //field.AreaIndex = 13;

                    field.FieldName = "SaleSupName";
                    field.Caption = Utility.Phrase("SaleSupName");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                    field.HeaderStyle.Wrap = DefaultBoolean.True;
                });

                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.RowArea;
                    field.AreaIndex = 18;
                    field.ValueFormat.Format = Utility.info.DateTimeFormat;
                    field.ValueFormat.FormatType = FormatType.DateTime;
                    field.ValueFormat.FormatString = Utility.info.DateTimeFormat.ShortDatePattern;

                    field.FieldName = "VisitDate";
                    field.Caption = Utility.Phrase("VisitDate");
                    field.RunningTotal = false;
                    field.TotalsVisibility = PivotTotalsVisibility.None;
                });


                //DATA AREA
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 0;
                    field.FieldName = "RouteCount";
                    field.Caption = Utility.Phrase("RouteCount");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 1;
                    field.FieldName = "RoutePass";
                    field.Caption = Utility.Phrase("RoutePass");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 2;
                    field.FieldName = "RouteNotPass";
                    field.Caption = Utility.Phrase("RouteNotPass");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 3;
                    field.FieldName = "MTDRoutePass";
                    field.Caption = Utility.Phrase("MTDRoutePass");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
                settings.Fields.Add(field =>
                {
                    field.Area = PivotArea.DataArea;
                    field.AreaIndex = 4;
                    field.FieldName = "MTDRouteNotPass";
                    field.Caption = Utility.Phrase("MTDRouteNotPass");

                    field.CellFormat.FormatType = FormatType.Custom;
                    field.CellFormat.FormatString = "###,##0.##";
                    field.SummaryType = PivotSummaryType.Sum;
                });
            }
            #endregion

            #region Render
            settings.PreRender = (sender, e) =>
            {
                var pivotGrid = (MVCxPivotGrid)sender;
                pivotGrid.Styles.HeaderStyle.Font.Bold = true;
                pivotGrid.Styles.HeaderStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.CellStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.FieldValueStyle.Font.Size = new FontUnit(12, UnitType.Pixel);

                pivotGrid.Styles.TotalCellStyle.Font.Bold = true;
                pivotGrid.Styles.FilterAreaStyle.Font.Bold = true;
                pivotGrid.Styles.ColumnAreaStyle.Font.Bold = true;
            };
            settings.EndRefresh = (sender, e) =>
            {
                var pivotGrid = (MVCxPivotGrid)sender;
                pivotGrid.Styles.HeaderStyle.Font.Bold = true;
                pivotGrid.Styles.HeaderStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.CellStyle.Font.Size = new FontUnit(12, UnitType.Pixel);
                pivotGrid.Styles.FieldValueStyle.Font.Size = new FontUnit(12, UnitType.Pixel);

                pivotGrid.Styles.TotalCellStyle.Font.Bold = true;
                pivotGrid.Styles.FilterAreaStyle.Font.Bold = true;
                pivotGrid.Styles.ColumnAreaStyle.Font.Bold = true;
            };

            //settings.OptionsPager.RowsPerPage = 0;
            #endregion

            return settings;
        }
        #endregion

        #region ReportPCExportRAWData
        public ActionResult ReportPCExportRAWData()
        {
            var model = SessionHelper.GetSession<ReportPCVM>("ReportPC");
            if (model.report == "ReportPC_SM")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemSM;
                return GridViewExtension.ExportToXlsx(ReportPCSettingsRAWData(), listItem);
            }
            else if (model.report == "ReportPC_SM_Daily")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemSM_Daily;
                return GridViewExtension.ExportToXlsx(ReportPCSettingsRAWData(), listItem);
            }
            else if (model.report == "ReportPC_MTD")
            {
                var listItem = SessionHelper.GetSession<ReportPCVM>("ReportPC").listItemMTD;
                return GridViewExtension.ExportToXlsx(ReportPCSettingsRAWData(), listItem);
            }
            return View();
        }

        private static GridViewSettings ReportPCSettingsRAWData()
        {
            var model = SessionHelper.GetSession<ReportPCVM>("ReportPC");
            var settings = new GridViewSettings
            {
                Name = model.report,
                KeyFieldName = "SaleSupID",
                CallbackRouteValues = new { Controller = "Tracking", Action = "ReportPCPartial" },
                Width = Unit.Percentage(100)
            };
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;
            settings.Styles.Footer.ForeColor = System.Drawing.Color.Red;
            settings.Styles.Footer.Font.Size = 11;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;

            #region ReportPC_SM
            if (model.report == "ReportPC_SM")
            {
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
                settings.Columns.Add(field =>
                {

                    field.FieldName = "RSMID";
                    field.Caption = Utility.Phrase("RSMID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "RSMName";
                    field.Caption = Utility.Phrase("RSMName");
                });
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
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ASMID";
                    field.Caption = Utility.Phrase("ASMID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ASMName";
                    field.Caption = Utility.Phrase("ASMName");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ProvinceID";
                    field.Caption = Utility.Phrase("ProvinceID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ProvinceName";
                    field.Caption = Utility.Phrase("ProvinceName");
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
                //DATA AREA
                settings.Columns.Add(field =>
                {
                    field.FieldName = "PCPass";
                    field.Caption = Utility.Phrase("PCPass");

                });
                //settings.Columns.Add(field =>
                //{
                //    field.FieldName = "PCNotPass";
                //    field.Caption = Utility.Phrase("PCNotPass");

                //});
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MCP";
                    field.Caption = Utility.Phrase("MCP");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "PC";
                    field.Caption = Utility.Phrase("PC");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "PC_MCP";
                    field.Caption = Utility.Phrase("PC_MCP");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTDPCPass";
                    field.Caption = Utility.Phrase("MTDPCPass");

                });
                //settings.Columns.Add(field =>
                //{
                //    field.FieldName = "MTDPCNotPass";
                //    field.Caption = Utility.Phrase("MTDPCNotPass");

                //});
                settings.Columns.Add(field =>
                {
                    field.FieldName = "ActualManday";
                    field.Caption = Utility.Phrase("ActualManday");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTDMCP";
                    field.Caption = Utility.Phrase("MTDMCP");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTDPC";
                    field.Caption = Utility.Phrase("MTDPC");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTD_AVG_PC";
                    field.Caption = Utility.Phrase("MTD_AVG_PC");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTDPC_MCP";
                    field.Caption = Utility.Phrase("MTDPC_MCP");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTDLPPC";
                    field.Caption = Utility.Phrase("MTDLPPC");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTDTotalSKU";
                    field.Caption = Utility.Phrase("MTDTotalSKU");

                });
            }
            #endregion
            #region ReportPC_SM_Daily
            if (model.report == "ReportPC_SM_Daily")
            {
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
                settings.Columns.Add(field =>
                {

                    field.FieldName = "RSMID";
                    field.Caption = Utility.Phrase("RSMID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "RSMName";
                    field.Caption = Utility.Phrase("RSMName");
                });
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
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ASMID";
                    field.Caption = Utility.Phrase("ASMID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ASMName";
                    field.Caption = Utility.Phrase("ASMName");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ProvinceID";
                    field.Caption = Utility.Phrase("ProvinceID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ProvinceName";
                    field.Caption = Utility.Phrase("ProvinceName");
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
                //DATA AREA
                settings.Columns.Add(field =>
                {
                    field.FieldName = "PCPass";
                    field.Caption = Utility.Phrase("PCPass");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "PCNotPass";
                    field.Caption = Utility.Phrase("PCNotPass");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MCP";
                    field.Caption = Utility.Phrase("MCP");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "PC";
                    field.Caption = Utility.Phrase("PC");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "PC_MCP";
                    field.Caption = Utility.Phrase("PC_MCP");

                });
            }
            #endregion
            #region ReportPC_MTD
            if (model.report == "ReportPC_MTD")
            {
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
                settings.Columns.Add(field =>
                {

                    field.FieldName = "RSMID";
                    field.Caption = Utility.Phrase("RSMID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "RSMName";
                    field.Caption = Utility.Phrase("RSMName");
                });
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
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ASMID";
                    field.Caption = Utility.Phrase("ASMID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ASMName";
                    field.Caption = Utility.Phrase("ASMName");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ProvinceID";
                    field.Caption = Utility.Phrase("ProvinceID");
                });
                settings.Columns.Add(field =>
                {

                    field.FieldName = "ProvinceName";
                    field.Caption = Utility.Phrase("ProvinceName");
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

                    field.FieldName = "VisitDate";
                    field.Caption = Utility.Phrase("VisitDate");
                });
                //DATA AREA
                settings.Columns.Add(field =>
                {
                    field.FieldName = "RouteCount";
                    field.Caption = Utility.Phrase("RouteCount");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "RoutePass";
                    field.Caption = Utility.Phrase("RoutePass");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "RouteNotPass";
                    field.Caption = Utility.Phrase("RouteNotPass");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTDRoutePass";
                    field.Caption = Utility.Phrase("MTDRoutePass");

                });
                settings.Columns.Add(field =>
                {
                    field.FieldName = "MTDRouteNotPass";
                    field.Caption = Utility.Phrase("MTDRouteNotPass");

                });
            }
            #endregion

            return settings;
        }
        #endregion

        #region ReportPCChart
        public ActionResult ReportPCChart(string groupby)
        {
            return View();
        }

        public ActionResult ReportPCChartDaily(string groupby)
        {
            return View();
        }
        #endregion
        #endregion

        #region ReportOrderIndex
        #region ReportOrderIndex
        public ActionResult ReportOrderIndex(string strFromDate, string strToDate, string act, FormCollection formParam)
        {
            PermissionHelper.CheckPermissionByFeature("Tracking_ReportOrderIndex", this);
            ReportOrderIndexVM model = new ReportOrderIndexVM();
            #region Validate Data
            try
            {
                //Begin get
                //Set default value
                if (string.IsNullOrEmpty(strFromDate))
                {
                    model.areaID = string.Empty;
                    model.FromDate = DateTime.Today;
                    model.distributorID = 0;
                    model.provinceID = string.Empty;
                    model.regionID = string.Empty;
                    model.routeID = string.Empty;
                    model.saleSupID = string.Empty;
                    model.ToDate = DateTime.Today;
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.strToDate = model.ToDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                }
                else
                {
                    model.areaID = Utility.StringParse(EditorExtension.GetValue<string>("AreaID"));// Utility.StringParse(areaID);
                    model.FromDate = Utility.DateTimeParse(strFromDate);
                    model.distributorID = Utility.IntParse(EditorExtension.GetValue<int>("DistributorID"));// Utility.IntParse(distributorID);
                    model.provinceID = string.Empty; //Utility.StringParse(EditorExtension.GetValue<string>("ProvinceID"));// Utility.StringParse(provinceID);
                    model.regionID = Utility.StringParse(EditorExtension.GetValue<string>("RegionID"));// Utility.StringParse(regionID);
                    model.routeID = Utility.StringParse(EditorExtension.GetValue<string>("RouteID"));// Utility.StringParse(routeID);
                    model.saleSupID = Utility.StringParse(EditorExtension.GetValue<string>("SalesSupID"));// Utility.StringParse(salesSupID);
                    model.ToDate = Utility.DateTimeParse(strToDate);
                    model.strFromDate = model.FromDate.ToShortPattern();
                    model.strToDate = model.ToDate.ToShortPattern();
                    model.salesmanID = string.Empty;
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            #endregion

            #region LoadComboboxListItem
            model.ListRegion = ControllerHelper.GetListRegion(model.regionID); //model.regionID);
            if (string.IsNullOrEmpty(model.regionID) && model.ListRegion.Count == 1)
            {
                model.regionID = model.ListRegion.First().RegionID;
            }

            model.ListArea = ControllerHelper.GetListArea(model.regionID);
            if (string.IsNullOrEmpty(model.areaID) && model.ListArea.Count == 1)
            {
                model.areaID = model.ListArea.First().AreaID;
            }

            model.ListProvince = new List<DMSProvince>();// ControllerHelper.GetListProvince(model.regionID, model.areaID);
            if (string.IsNullOrEmpty(model.provinceID) && model.ListProvince.Count == 1)
            {
                model.provinceID = model.ListProvince.First().ProvinceID;
            }

            model.ListSForce = ControllerHelper.GetListSaleSup(model.regionID, model.areaID, model.provinceID, model.distributorID);
            if (string.IsNullOrEmpty(model.saleSupID) && model.ListSForce.Count == 1)
            {
                model.saleSupID = model.ListSForce.First().EmployeeID;
            }

            model.ListDistributor = ControllerHelper.GetListDistributor(model.regionID, model.areaID, model.provinceID, model.saleSupID);
            if (model.distributorID == 0 && model.ListDistributor.Count == 1)
            {
                model.distributorID = model.ListDistributor.First().DistributorID;
            }

            model.ListRoute = ControllerHelper.GetListRoute(model.regionID, model.areaID, model.provinceID, model.distributorID, model.saleSupID);
            if (string.IsNullOrEmpty(model.routeID) && model.ListRoute.Count == 1)
            {
                model.routeID = model.ListRoute.First().RouteID;
            }
            #endregion

            if (model.ToDate < model.FromDate)
            {
                model.listItem = new List<pp_ReportOrderIndexLevelResult>();
                ViewBag.StatusMessage = "Ngày không hợp lệ. Vui lòng chọn ngày hợp lệ.";
                return View(model);
            }

            #region Set default select if drop have only one item
            #endregion

            if (
                !string.IsNullOrEmpty(strFromDate)
                )
            {
                string username = SessionHelper.GetSession<string>("UserName");
                model.listItem = Global.Context.pp_ReportOrderIndexLevel(model.FromDate, model.ToDate, username, 1).ToList();
            }
            else
            {
                model.listItem = new List<pp_ReportOrderIndexLevelResult>();
            }
            SessionHelper.SetSession<ReportOrderIndexVM>("ReportOrderIndex", model);

            if (act == "ExportExcel")
            {
                return RedirectToAction("ReportOrderIndexExport");
            }

            if (act == "ExportExcelRawData")
            {
                return RedirectToAction("ReportOrderIndexExportRAWData");
            }
            return View(model);
        }

        [Authorize]
        public PartialViewResult ReportOrderIndexPartial()
        {
            List<pp_ReportOrderIndexLevelResult> model = SessionHelper.GetSession<ReportOrderIndexVM>("ReportOrderIndex").listItem;
            return PartialView("ReportOrderIndexPartial", model);
        }
        #endregion

        #region ReportSalemanKPIExport
        public ActionResult ReportOrderIndexExport()
        {
            var model = SessionHelper.GetSession<ReportOrderIndexVM>("ReportOrderIndex").listItem;
            return PivotGridExtension.ExportToXlsx(ReportOrderIndexSettings(), model);
        }

        public static PivotGridSettings ReportOrderIndexSettings()
        {
            PivotGridSettings settings = new PivotGridSettings();
            settings.Name = "ReportOrderIndex";
            settings.CallbackRouteValues = new { Controller = "Tracking", Action = "ReportOrderIndexPartial" };
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsCustomization.AllowDrag = false;
            settings.OptionsCustomization.AllowDragInCustomizationForm = false;
            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ColumnTotalsLocation = PivotTotalsLocation.Far;
            //settings.OptionsView.ShowGrandTotalsForSingleValues = true;
            settings.OptionsView.ShowRowTotals = true;
            settings.OptionsView.ShowColumnTotals = false;
            settings.OptionsView.ShowTotalsForSingleValues = true;
            settings.OptionsView.ShowColumnGrandTotalHeader = false;
            settings.OptionsView.ShowColumnGrandTotals = false;

            settings.Groups.Add("RSMID - RegionID - ASMID - AreaID - ProvinceID - DistributorCode - SaleSupID");


            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    //field.AreaIndex = 0;

            //    field.FieldName = "RegionID";
            //    field.Caption = Utility.Phrase("RegionID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 1;

                field.FieldName = "RegionName";
                field.Caption = Utility.Phrase("RegionName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.AutomaticTotals;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 2;

                field.FieldName = "RSMID";
                field.Caption = Utility.Phrase("RSMID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 3;

                field.FieldName = "RSMName";
                field.Caption = Utility.Phrase("RSMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.AutomaticTotals;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    //field.AreaIndex = 4;

            //    field.FieldName = "AreaID";
            //    field.Caption = Utility.Phrase("AreaID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 5;

                field.FieldName = "AreaName";
                field.Caption = Utility.Phrase("AreaName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.AutomaticTotals;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 6;

                field.FieldName = "ASMID";
                field.Caption = Utility.Phrase("ASMID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 7;

                field.FieldName = "ASMName";
                field.Caption = Utility.Phrase("ASMName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.AutomaticTotals;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.RowArea;
            //    //field.AreaIndex = 8;

            //    field.FieldName = "ProvinceID";
            //    field.Caption = Utility.Phrase("ProvinceID");
            //    field.RunningTotal = false;
            //    field.TotalsVisibility = PivotTotalsVisibility.None;
            //});

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 9;

                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.AutomaticTotals;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 10;

                field.FieldName = "DistributorCode";
                field.Caption = Utility.Phrase("DistributorCode");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 11;

                field.FieldName = "DistributorName";
                field.Caption = Utility.Phrase("DistributorName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 12;

                field.FieldName = "SaleSupID";
                field.Caption = Utility.Phrase("SaleSupID");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                //field.AreaIndex = 13;

                field.FieldName = "SaleSupName";
                field.Caption = Utility.Phrase("SaleSupName");
                field.RunningTotal = false;
                field.TotalsVisibility = PivotTotalsVisibility.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.ColumnArea;
                field.FieldName = "TitleCall";
                field.Caption = Utility.Phrase("TitleCall");
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.ColumnArea;
                field.FieldName = "SettingName";
                field.Caption = Utility.Phrase("SettingName");
            });


            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                //field.AreaIndex = 8;
                field.FieldName = "CallInLevel";
                field.Caption = Utility.Phrase("CallInLevel");

                field.CellFormat.FormatType = FormatType.Custom;
                field.CellFormat.FormatString = "###,#0";
                field.EmptyCellText = "0";
                field.EmptyValueText = "0";
                field.SummaryType = PivotSummaryType.Sum;
            });



            settings.OptionsPager.RowsPerPage = 0;
            return settings;
        }
        #endregion

        #region EditOrderIndexPartial
        public ActionResult EditOrderIndexPartial()
        {
            return PartialView("EditOrderIndexPartial", Global.Context.pp_GetOrderIndexLevel().ToList());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateOrderIndexPartial(pp_GetOrderIndexLevelResult product)
        {
            PermissionHelper.CheckPermissionByFeature("Tracking_UpdateOrderIndexPartial", this);
            if (ModelState.IsValid)
            {
                try
                {
                    var minItem = Global.Context.CustomSettings.Where(a => a.SettingCode == "ReportOrderIndex" + product.SettingName + "Min").FirstOrDefault();
                    if (minItem != null)
                    {
                        minItem.SettingValue = Utility.StringParseWithDecimalDegitEN(product.MinValue.Value);
                        Global.Context.SubmitChanges();
                    }

                    var maxItem = Global.Context.CustomSettings.Where(a => a.SettingCode == "ReportOrderIndex" + product.SettingName + "Max").FirstOrDefault();
                    if (maxItem != null)
                    {
                        maxItem.SettingValue = Utility.StringParseWithDecimalDegitEN(product.MaxValue.Value);
                        Global.Context.SubmitChanges();
                    }
                    ViewData["EditError"] = Utility.Phrase("MessageUpdateOK");
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = "Please, correct all errors.";

            return PartialView("EditOrderIndexPartial", Global.Context.pp_GetOrderIndexLevel().ToList());
        }
        #endregion

        #endregion

        #region TerritoryPerformanceHeat
        [ActionAuthorize("Tracking_TerritoryPerformanceHeat", true)]
        [CompressFilter]
        public ActionResult TerritoryPerformanceHeat(FormCollection formParam, string strVisitDate, string strSearch)
        {
            var model = new HomeVM();

            if (string.IsNullOrEmpty(strVisitDate))
            {
                if (Session["DateSelected"] == null)
                {
                    model.VisitDate = DateTime.Now;
                }
                else
                {
                    strVisitDate = Session["DateSelected"].ToString();
                    model.VisitDate = Utility.DateTimeParse(strVisitDate);
                }
            }
            else
            {
                model.VisitDate = Utility.DateTimeParse(strVisitDate);
                Session["DateSelected"] = strVisitDate;
            }


            model.listSaleman = new List<Map_Salesman>();

            model.strDate = DateTime.Now.ToString(Constant.ShortDatePattern);

            model.ListRegion = ControllerHelper.GetListRegion();
            model.ListArea = ControllerHelper.GetListArea(string.Empty);
            model.listSS = ControllerHelper.GetListSaleSup(string.Empty);
            model.listDis = ControllerHelper.GetListDistributor(string.Empty, string.Empty);
            var lstSalesManViolate = Global.Context.pp_GetSalesManViolate(Utility.DateTimeParse(strVisitDate), username).ToList();
            SessionHelper.SetSession("SalesManViolate", Global.Context.pp_GetSalesManViolate(model.VisitDate, username).ToList());
            SessionHelper.SetSession("TreeTerritory", Global.Context.pp_GetTreeTerritory(model.VisitDate, username).ToList());

            // model.listCategory = ControllerHelper.ListCategory("Coverage", strVisitDate);
            if (!string.IsNullOrEmpty(strSearch))
                ViewBag.Search = strSearch;
            else
                ViewBag.Search = "";
            if (PermissionHelper.CheckPermissionByFeature("NSM") || username.ToLower() == "admin" || PermissionHelper.CheckPermissionByFeature("NSD"))
            {
                ViewBag.RoleUser = "NSM";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                ViewBag.RoleUser = "RSM";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                ViewBag.RoleUser = "ASM";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                ViewBag.RoleUser = "SalesSup";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                ViewBag.RoleUser = "SalesSup";
            }
            return View(model);
        }
        #endregion

        #region TerritoryPerformance
        [ActionAuthorize("Tracking_TerritoryPerformance", true)]
        [CompressFilter]
        public ActionResult TerritoryPerformance(FormCollection formParam, string strVisitDate, string strSearch)
        {
            //string ipClient = Request.UserHostAddress;
            //string IP = Request.UserHostName;
            //string compName = Utility.DetermineCompName(IP);


            var model = new HomeVM();

            if (string.IsNullOrEmpty(strVisitDate))
            {
                if (Session["DateSelected"] == null)
                {
                    model.VisitDate = DateTime.Now;
                }
                else
                {
                    strVisitDate = Session["DateSelected"].ToString();
                    model.VisitDate = Utility.DateTimeParse(strVisitDate);
                }
            }
            else
            {
                model.VisitDate = Utility.DateTimeParse(strVisitDate);
                Session["DateSelected"] = strVisitDate;
            }


            model.listSaleman = new List<Map_Salesman>();

            model.strDate = DateTime.Now.ToString(Constant.ShortDatePattern);

            model.ListRegion = ControllerHelper.GetListRegion();
            model.ListArea = ControllerHelper.GetListArea(string.Empty);
            model.listSS = ControllerHelper.GetListSaleSup(string.Empty);
            model.listDis = ControllerHelper.GetListDistributor(string.Empty, string.Empty);
            var lstSalesManViolate = Global.Context.pp_GetSalesManViolate(Utility.DateTimeParse(strVisitDate), username).ToList();
            SessionHelper.SetSession("SalesManViolate", Global.Context.pp_GetSalesManViolate(model.VisitDate, username).ToList());
            SessionHelper.SetSession("TreeTerritory", Global.Context.pp_GetTreeTerritory(model.VisitDate, username).ToList());

            // model.listCategory = ControllerHelper.ListCategory("Coverage", strVisitDate);
            if (!string.IsNullOrEmpty(strSearch))
                ViewBag.Search = strSearch;
            else
                ViewBag.Search = "";
            if (PermissionHelper.CheckPermissionByFeature("NSM") || username.ToLower() == "admin" || PermissionHelper.CheckPermissionByFeature("NSD"))
            {
                ViewBag.RoleUser = "NSM";
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                ViewBag.RoleUser = "RSM";
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                ViewBag.RoleUser = "ASM";
            }
            else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
            {
                ViewBag.RoleUser = "SalesSup";
            }
            else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
            {
                ViewBag.RoleUser = "SalesSup";
            }
            return View(model);
        }

        #region Routing for RawData
        public class ListInformatinonOutLet : List<InformationOutLet>
        {
        }
        public class InformationOutLet
        {
            public double? Latitude { get; set; }
            public double? Longtitude { get; set; }
            public string DistributorName { get; set; }
            public string OutletName { get; set; }
            public string Address { get; set; }
        }
        public ActionResult ShowOutLets(string latitude, string longtitude)
        {
            var dt = Global.Context.pp_GetAllOutLetFollowLocation(Utility.StringParse(latitude), Utility.StringParse(longtitude)).ToList();
            ListInformatinonOutLet lstInfo = new ListInformatinonOutLet();
            foreach (var item in dt)
            {
                InformationOutLet info = new InformationOutLet();
                info.Latitude = Convert.ToDouble(item.Latitude);
                info.Longtitude = Convert.ToDouble(item.Longtitude);
                info.OutletName = item.OutletName;
                info.Address = item.Address;
                lstInfo.Add(info);
            }
            //SimulateRoute(lstInfo);
            return Json(lstInfo, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowDistributors(string latitude, string longtitude)
        {
            var dt = Global.Context.pp_GetAllDistributorFollowLocation(Utility.StringParse(latitude), Utility.StringParse(longtitude)).ToList();
            ListInformatinonOutLet lstInfo = new ListInformatinonOutLet();
            foreach (var item in dt)
            {
                InformationOutLet info = new InformationOutLet();
                info.Latitude = Convert.ToDouble(item.Latitude);
                info.Longtitude = Convert.ToDouble(item.Longtitude);
                info.DistributorName = item.DistributorName;
                info.Address = item.Address;
                lstInfo.Add(info);
            }
            //SimulateRoute(lstInfo);
            return Json(lstInfo, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region HeatMap
        //[ActionAuthorize("DashBoard_TerritoryPartial")]
        //[CompressFilter]
        public ActionResult GetDataPolygon(string type, string dateSelected, string groupby, string TerritoryID, string categoryID, string itemID)
        {
            var j = Global.Context.TerritoryPolygons.Where(a => a.Territory == TerritoryID).OrderBy(a => a.RenderOrder).ToList();
            if (j.Count > 0)
            {
                if (type == "CoverageOutlet" || type == "CoverageCategory")
                {
                    var info = ControllerHelper.GetQuantityCoverageTerritory(type, dateSelected, groupby, categoryID, itemID).Where(a => a.Code == TerritoryID).FirstOrDefault();
                    return Json(new
                    {
                        html = j,
                        info = info
                    }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    var info = ControllerHelper.GetRevenueTerritory(dateSelected, groupby, TerritoryID).Where(a => a.Code == TerritoryID).FirstOrDefault();
                    return Json(new
                    {
                        html = j,
                        info = info
                    }, JsonRequestBehavior.DenyGet);
                }
            }
            else
            {
                return Json(new
                {
                    html = j
                }, JsonRequestBehavior.DenyGet);
            }
        }
        //2016-05-24: Code get Provice tinh thanh
        private List<string> GennerateProvice(string type, string groupby, List<pp_ReportSalesAssessmentResult> model)
        {
            groupby = "Province";
            List<string> lst = new List<string>();            
            try
            {
                var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                foreach (dynamic item in dynamicQuery)
                {
                    lst.Add(item.ID.ToString());
                }
               
            }
            catch (Exception e)
            {
                string a = e.Message;
            }

            return lst;
        }
        private List<string> GennerateRevenue(string type, string groupby, string TerritoryID, string itemClick, List<pp_ReportSalesAssessmentResult> model)
        {
            List<string> lst = new List<string>();
            if ((!string.IsNullOrEmpty(itemClick) || !string.IsNullOrEmpty(TerritoryID)))
            {
                if (groupby == "Region")
                {
                    if (string.IsNullOrEmpty(itemClick))
                    {
                        var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID);
                        }
                    }
                    else
                    {
                        var dynamicQuery = model.AsQueryable().Where(x => x.RegionID == itemClick).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID);
                        }
                    }
                }
                else if (groupby == "Area")
                {
                    if (!string.IsNullOrEmpty(TerritoryID))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.RegionID == TerritoryID).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID);
                        }
                    }
                    else if (!string.IsNullOrEmpty(itemClick))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.AreaID == itemClick).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID);
                        }
                    }
                }
                else if (groupby == "Distributor")
                {
                    if (!string.IsNullOrEmpty(TerritoryID))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.AreaID == TerritoryID).GroupBy("new(" + groupby + "Code, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "Code as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID.ToString());
                        }
                    }
                    else if (!string.IsNullOrEmpty(itemClick))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.DistributorID == Utility.IntParse(itemClick)).GroupBy("new(" + groupby + "Code, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "Code as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID.ToString());
                        }
                    }
                }
                else if (groupby == "Route")
                {
                    if (!string.IsNullOrEmpty(TerritoryID))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.DistributorCode == TerritoryID).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID.ToString());
                        }
                    }
                    else if (!string.IsNullOrEmpty(itemClick))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.RouteID == itemClick).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID.ToString());
                        }
                    }
                }
            }
            else
            {
                if (groupby != "Distributor")
                {
                    var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                    foreach (dynamic item in dynamicQuery)
                    {
                        lst.Add(item.ID.ToString());
                    }
                }
                else
                {
                    var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "Code, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "Code as ID)");
                    foreach (dynamic item in dynamicQuery)
                    {
                        lst.Add(item.ID.ToString());
                    }
                }
            }
            return lst;
        }
        private List<string> GennerateQuantityCoverage(string type, string groupby, string TerritoryID, string itemClick, List<pp_GetDataRenderHeatMapResult> model)
        {
            List<string> lst = new List<string>();
            if ((!string.IsNullOrEmpty(itemClick) || !string.IsNullOrEmpty(TerritoryID)))
            {
                if (groupby == "Region")
                {
                    if (string.IsNullOrEmpty(itemClick))
                    {
                        var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID);
                        }
                    }
                    else
                    {
                        var dynamicQuery = model.AsQueryable().Where(x => x.RegionID == itemClick).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID);
                        }
                    }
                }
                else if (groupby == "Area")
                {
                    if (!string.IsNullOrEmpty(TerritoryID))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.RegionID == TerritoryID).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID);
                        }
                    }
                    else if (!string.IsNullOrEmpty(itemClick))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.AreaID == itemClick).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID);
                        }
                    }
                }
                else if (groupby == "Distributor")
                {
                    if (!string.IsNullOrEmpty(TerritoryID))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.AreaID == TerritoryID).GroupBy("new(" + groupby + "Code, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "Code as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID.ToString());
                        }
                    }
                    else if (!string.IsNullOrEmpty(itemClick))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.DistributorID == Utility.IntParse(itemClick)).GroupBy("new(" + groupby + "Code, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "Code as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID.ToString());
                        }
                    }
                }
                else if (groupby == "Route")
                {
                    if (!string.IsNullOrEmpty(TerritoryID))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.DistributorCode == TerritoryID).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID.ToString());
                        }
                    }
                    else if (!string.IsNullOrEmpty(itemClick))
                    {
                        var dynamicQuery = model.AsQueryable().Where(c => c.RouteID == itemClick).GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                        foreach (dynamic item in dynamicQuery)
                        {
                            lst.Add(item.ID.ToString());
                        }
                    }
                }
            }
            else
            {
                if (groupby != "Distributor")
                {
                    var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
                    foreach (dynamic item in dynamicQuery)
                    {
                        lst.Add(item.ID);
                    }
                }
                else
                {
                    var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "Code, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "Code as ID)");
                    foreach (dynamic item in dynamicQuery)
                    {
                        lst.Add(item.ID.ToString());
                    }
                }
            }
            return lst;
        }
        //2016-05-25: Get du lieu cho tinh thanh\
        private List<string> GennerateProviceQuantityCoverage(string type, string groupby, string TerritoryID, string itemClick, List<pp_GetDataRenderHeatMapResult> model)
        {
            groupby = "Province";
            List<string> lst = new List<string>();
            try
            {
            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "Name as Name, Key." + groupby + "ID as ID)");
            foreach (dynamic item in dynamicQuery)
            {
                lst.Add(item.ID);
            }
            }
            catch (Exception e)
            {
                string a = e.Message;
            }
            return lst;
        }
        public ActionResult GetDataProvinceTerritory(string type, string dateSelected, string groupby, string TerritoryID, string itemClick, string categoryID, string itemID)
        {
            List<string> lst = new List<string>();
            Session["typeHM"] = type;
            string role = string.Empty;
            if (string.IsNullOrEmpty(TerritoryID))
            {
                if (PermissionHelper.CheckPermissionByFeature("NSM"))
                {
                    role = "NSM";
                }
            }
            if ( type == "CoverageCategory")
            {
                List<pp_GetDataRenderHeatMapResult> model = (from sm in ControllerHelper.GetDataRenderHeatMapResult(type, dateSelected, categoryID, itemID)
                                                             join ut in ControllerHelper.ListRoute
                                                             on sm.RouteID equals ut.RouteID
                                                             where sm.DistributorID == ut.DistributorID
                                                                 && sm.RouteID == ut.RouteID
                                                                 && sm.RegionID != null
                                                                 && sm.AreaID != null 
                                                             select sm).Distinct().ToList();
                Session["modelQC"] = model;             
                lst = GennerateQuantityCoverage(type, groupby, TerritoryID, itemClick, model);    
                //lst = GennerateProvice(type, groupby, model);
            }else if (type == "CoverageOutlet")
            {
                List<pp_GetDataRenderHeatMapResult> model = (from sm in ControllerHelper.GetDataRenderHeatMapResult(type, dateSelected, categoryID, itemID)
                                                             join ut in ControllerHelper.ListRoute
                                                             on sm.RouteID equals ut.RouteID
                                                             where sm.DistributorID == ut.DistributorID
                                                                 && sm.RouteID == ut.RouteID
                                                                 && sm.RegionID != null
                                                                 && sm.AreaID != null
                                                             select sm).Distinct().ToList();
                Session["modelQC"] = model;
                lst = GennerateProviceQuantityCoverage(type, groupby, TerritoryID, itemClick, model);        
            }
            else
            {
                List<pp_ReportSalesAssessmentResult> model = (from sm in ControllerHelper.GetRevenueResult(dateSelected)
                                                              join ut in ControllerHelper.ListRoute
                                                              on sm.RouteID equals ut.RouteID
                                                              where   
                                                                  sm.DistributorID == ut.DistributorID
                                                                  && sm.RouteID.Equals(ut.RouteID, StringComparison.InvariantCultureIgnoreCase)
                                                              select sm).Distinct().ToList();
                Session["modelRev"] = model;
                lst = GennerateProvice(type, groupby, model);             
            }
            if ( type == "CoverageCategory")
            {
                var info = ControllerHelper.GetQuantityCoverageTerritory(type, dateSelected, groupby, categoryID, itemID).ToList();
                return Json(new
                {
                    html = 1,
                    info = info
                }, JsonRequestBehavior.DenyGet);
            } else if (type == "CoverageOutlet" )
            {
                var info = ControllerHelper.GetQuantityCoverageProvince(type, dateSelected, groupby, categoryID, itemID).ToList();    
                //2016-05-26: HieuNguyen: bo sung nhung region con thieu du lieu tren bang do.
                List<DMSProvince> listprovice = Global.Context.DMSProvinces.ToList();
                List<RenderDataHeatMap> infotem = new List<RenderDataHeatMap>();
                foreach (DMSProvince item in listprovice)
                {
                    var find = info.Where(x => x.Code == item.ProvinceID);
                    if (find.Count() <= 0)
                    {
                        RenderDataHeatMap ins = new RenderDataHeatMap();
                        ins.Level1 = item.Province;
                        ins.Code = item.ProvinceID;
                        ins.Name = item.Province;
                        ins.Category = "";
                        ins.InventoryItem = "";
                        ins.RatioQuantity = 0;
                        ins.TotalCoverage = 0;
                        ins.TotalOutlet = 0;
                        ins.TotalQuantity = 0;                      
                        infotem.Add(ins);
                    }
                }

                foreach (RenderDataHeatMap item in infotem)
                {
                    info.Add(item);
                }
                //end

                return Json(new
                {
                    html = 1,
                    info = info
                }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                var info = ControllerHelper.GetRevenueProvince(dateSelected, groupby, TerritoryID).ToList();
                //2016-05-26: HieuNguyen: bo sung nhung region con thieu du lieu tren bang do.
               
                List<DMSProvince> listprovice = Global.Context.DMSProvinces.ToList();
                List<ReportSMVisitSummaryChartData> infotem = new List<ReportSMVisitSummaryChartData>();
                foreach (DMSProvince item in listprovice)
                {
                    var find = info.Where(x => x.Code == item.ProvinceID);
                    if (find.Count() <= 0)
                    {
                        ReportSMVisitSummaryChartData ins = new ReportSMVisitSummaryChartData();
                        ins.Level1 = item.Province;
                        ins.Code = item.ProvinceID;
                        ins.Name = item.Province;
                        ins.TargetRevenue = 0;
                        ins.TotalAmount = 0;
                        ins.RatioRevenue = 0;
                        ins.OrderCount = 0;
                        ins.TotalSKU = 0;
                        ins.TotalQuantity = 0;
                        ins.LPPC = 0;
                        ins.OutletMustVisit = 0;
                        ins.OutletVisited = 0;
                        ins.SOMCP = 0;
                        ins.VisitMCP = 0;
                        infotem.Add(ins);
                    }
                }

                foreach (ReportSMVisitSummaryChartData item in infotem)
                {
                    info.Add(item);
                }
                //end
                return Json(new
                {
                    html = 1,
                    info = info
                }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult GetTerritory(string type, string dateSelected, string groupby, string TerritoryID, string itemClick, string categoryID, string itemID)
        {
            List<string> lst = new List<string>();
            Session["typeHM"] = type;
            string role = string.Empty;
            if (string.IsNullOrEmpty(TerritoryID))
            {
                if (PermissionHelper.CheckPermissionByFeature("NSM"))
                {
                    role = "NSM";
                }
                //else if (PermissionHelper.CheckPermissionByFeature("RSM"))
                //{
                //    groupby = "Region";
                //}
                //else if (PermissionHelper.CheckPermissionByFeature("ASM"))
                //{
                //    groupby = "Area";
                //}
                //else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
                //{
                //    groupby = "Route";
                //}
                //else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
                //{
                //    groupby = "Route";
                //}
            }
            if (type == "CoverageOutlet" || type == "CoverageCategory")
            {
                List<pp_GetDataRenderHeatMapResult> model = (from sm in ControllerHelper.GetDataRenderHeatMapResult(type, dateSelected, categoryID, itemID)
                                                             join ut in ControllerHelper.ListRoute
                                                             on sm.RouteID equals ut.RouteID
                                                             where sm.DistributorID == ut.DistributorID
                                                                 && sm.RouteID == ut.RouteID
                                                                 && sm.RegionID != null
                                                                 && sm.AreaID != null
                                                             select sm).Distinct().ToList();
                Session["modelQC"] = model;
                lst = GennerateQuantityCoverage(type, groupby, TerritoryID, itemClick, model);
            }
            else
            {
                List<pp_ReportSalesAssessmentResult> model = (from sm in ControllerHelper.GetRevenueResult(dateSelected)
                                                              join ut in ControllerHelper.ListRoute
                                                              on sm.RouteID equals ut.RouteID
                                                              where
                                                                  sm.DistributorID == ut.DistributorID
                                                                  && sm.RouteID.Equals(ut.RouteID, StringComparison.InvariantCultureIgnoreCase)
                                                              select sm).Distinct().ToList();
                Session["modelRev"] = model;
                lst = GennerateRevenue(type, groupby, TerritoryID, itemClick, model);
            }
            int incre = 0;
            foreach (var item in lst)
            {
                var j = Global.Context.TerritoryPolygons.Where(a => a.Territory == item).OrderBy(a => a.RenderOrder).ToList();
                if (j.Count > 0)
                {
                    incre++;
                    break;
                }
            }
            return Json(new
            {
                IsHasData = incre,
                ID = lst,
                Group = groupby,
                Role = role
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ComboBoxPartialCategory()
        {
            string dateSelected = Utility.StringParse(Request.Params["dateSL"]);
            string typeHeatMap = Utility.StringParse(Request.Params["typeHM"]);
            List<pp_GetCategoryItem_HeatMapResult> listCategory = ControllerHelper.ListCategory(typeHeatMap, dateSelected);
            return PartialView(listCategory);
        }
        public ActionResult ComboBoxPartialItemCategory()
        {
            string categoryID = Utility.StringParse(Request.Params["CategoryID"]);
            string strFromDate = Utility.StringParse(Request.Params["strFromDate"]);
            string typeHeatMap = Utility.StringParse(Request.Params["typeHM"]);
            List<pp_GetCategoryItem_HeatMapResult> listItem = ControllerHelper.GetListItem(strFromDate, typeHeatMap, categoryID);
            return PartialView(listItem);
        }
        public ActionResult GetListItemCategory(string typeHeatMap, string strFromDate)
        {
            List<pp_GetCategoryItem_HeatMapResult> listCategory = ControllerHelper.ListCategory(typeHeatMap, strFromDate);
            return Json(listCategory, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetListItem(string typeHeatMap, string strFromDate, string categoryID)
        {
            List<pp_GetCategoryItem_HeatMapResult> listItem = ControllerHelper.GetListItem(strFromDate, typeHeatMap, categoryID);
            return Json(listItem, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ContinueSeparateOutlet(string increment)
        {
            var lst = SessionHelper.GetSession<List<pp_GetCoverageOutlet_HeatMapResult>>("BLCoverage").ToList();
            List<pp_GetCoverageOutlet_HeatMapResult> Items = new List<pp_GetCoverageOutlet_HeatMapResult>();
            //lst.RemoveRange((1000 * Utility.IntParse(increment))+1, 1000);
            for (int i = 0; i < lst.Count(); i++)
            {
                if (i >= (1000 * Utility.IntParse(increment)) + 1 && i <= (1000 * Utility.IntParse(increment)) + 1000)
                {
                    Items.Add(lst[i]);
                }
            }
            return Json(new
            {
                info = Items
            });
        }
        public ActionResult SeparateOutlet(string date, string type, string inventoryID, string categoryID)
        {
            DateTime dt = Utility.DateTimeParse(date);
            List<pp_GetCoverageOutlet_HeatMapResult> j = Global.Context.pp_GetCoverageOutlet_HeatMap(dt, type, categoryID, inventoryID).ToList();
            SessionHelper.SetSession("BLCoverage", j);
            int index = j.Count > 1000 ? (j.Count / 1000) : 0;

            return Json(new
            {
                info = j.Count > 1000 ? j.Take(1000) : j,
                index = index
            });
            //var NbHasCoverage = j.Where(x => x.Visibility == 1).Count();
            //var NbNoCoverage = j.Where(x => x.Visibility == 0).Count();
            //var jsonResult = Json(j.Take(1000), JsonRequestBehavior.AllowGet);
            //jsonResult.MaxJsonLength = int.MaxValue;
            //return jsonResult;
        }
        public ActionResult GetCustomSettingCountry()
        {
            //var lstSetting = Global.Context.CustomColorSettings().ToList();
            CustomSetting setting = new CustomSetting();
            setting = Global.Context.CustomSettings.Where(x => x.SettingCode == "GeojsonRegion").FirstOrDefault();
            if (setting != null)
            {
                return Json(setting.SettingValue);
                //return Json(new
                //       {
                //           info = setting.SettingValue;
                //       });
            }else
            {
                return Json("");
            }
        }
        public ActionResult GetCustomSetting(string type)
        {
            //var lstSetting = Global.Context.CustomColorSettings().ToList();
            var lstSetting = Global.Context.CustomColorSettings.Where(x => x.Type == type).OrderBy(x => x.NumberIndex).ToList();
            return Json(new
            {
                info = lstSetting
            });
        }
        #endregion
        #region WorkWith
        // [ActionAuthorize ("RenderListInfoWorkWith")]
        public ActionResult RenderInfoWorkWith(string regionID, string areaID, int? distributorID, string salesSupID, string routeID, string strVisitDate)
        {
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            distributorID = Utility.IntParse(distributorID);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            salesSupID = Utility.StringParse(salesSupID);
            routeID = Utility.StringParse(routeID);

            var lstInfoWW = Global.Context.pp_GetWorkWithMap(visitDate, regionID, areaID, distributorID, salesSupID, routeID, username, 3).ToList();
            //var listStatusWW = (from a in lstInfoWW
            //                   //where a.SUPTimeStart != null
            //                   //     && a.SUPLatitudeStart != 0
            //                   //     && a.SUPLongtitudeStart != 0
            //                   select new 
            //                   {
            //                       ASMID = a.ASMID,
            //                       ASMName = a.ASMName,
            //                       DistributorCode = a.DistributorCode,
            //                       DistributorName = a.DistributorName,
            //                       RouteID = a.RouteID,
            //                       SaleSupID = a.SaleSupID,
            //                       SaleSupName = a.SaleSupName,
            //                       SalesmanID = a.SalePersonCD,
            //                       SalesmanName = a.SalesmanName,
            //                       Latitude = Utility.DecimalParseWithObject(a.Latitude),
            //                       Longtitude = Utility.DecimalParseWithObject(a.Longtitude),
            //                       SUPLatitudeStart = Utility.DecimalParseWithObject(a.SUPLatitudeStart),
            //                       SUPLongtitudeStart = Utility.DecimalParseWithObject(a.SUPLongtitudeStart),
            //                       ASMLatitudeStart = Utility.DecimalParseWithObject(a.ASMLatitudeStart),
            //                       ASMLongtitudeStart = Utility.DecimalParseWithObject(a.ASMLongtitudeStart),
            //                       SupWW = a.SupWW,
            //                       AsmWW = a.AsmWW
            //                   }).Distinct().ToList();

            return Json(new
            {
                html = lstInfoWW
            });
        }
        #endregion
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
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            DateTime fisrtDateLastMonth = visitDate.AddMonths(-1);
            fisrtDateLastMonth = new DateTime(fisrtDateLastMonth.Year, fisrtDateLastMonth.Month, 1);

            if (string.IsNullOrEmpty(routeCD) && !string.IsNullOrEmpty(salesmanID))
            {
                var mcp = Global.Context.VisitPlanHistories.Where(a => a.VisitDate == visitDate && a.DistributorID == distributorID && a.SalesmanID == salesmanID).FirstOrDefault();
                if (mcp != null)
                {
                    routeCD = mcp.RouteID;
                }
            }
            #endregion

            #region GET DATA

            var j = Global.Context.pp_GetVisitInfo(routeCD, distributorID, string.Empty, string.Empty, salesmanID, visitDate, username).ToList();
            #region listOutletInRoute
            var listOutletInRoute = (from a in j
                                     select new OutletInRoute()
                                     {
                                         ASMID = a.ASMID,
                                         ASMName = a.ASMName,
                                         SaleSupID = a.SaleSupID,
                                         SaleSupName = a.SaleSupName,
                                         DistributorID = a.DistributorID,
                                         DistributorCode = a.DistributorCode,
                                         DistributorName = a.DistributorName,
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
                                         ISMCP = a.ISMCP,
                                         T2 = a.T2,
                                         T3 = a.T3,
                                         T4 = a.T4,
                                         T5 = a.T5,
                                         T6 = a.T6,
                                         T7 = a.T7,
                                         CN = a.CN,
                                         strVisitInWeek = a.strVisitInWeek
                                     }).Distinct().ToList();
            #endregion
            #region listSMVisit
            var listSMVisit = (from a in j
                               where a.SMTimeStart != null
                                    && a.SMLatitude != 0
                                    && a.SMLongitude != 0
                               select new OutletSMVisit()
                               {
                                   DistributorID = a.DistributorID,
                                   OutletID = a.OutletID,
                                   DropSize = Utility.StringParseWithDecimalDegit(a.DropSize),
                                   TotalAmt = Utility.StringParseWithDecimalDegit(a.TotalAmt),
                                   TotalSKU = Utility.StringParseWithDecimalDegit(a.TotalSKU),
                                   Reason = a.Reason,
                                   SMTimeStart = a.SMTimeStart,
                                   SMTimeEnd = a.SMTimeEnd,
                                   SMLatitude = a.SMLatitude,
                                   SMLongitude = a.SMLongitude,
                                   SMDistance = Utility.StringParseWithDecimalDegit(a.SMDistance),
                                   ISMCP = a.ISMCP,
                                   HasOrder = a.HasOrder,
                                   HasVisit = a.HasVisit,
                                   RN = Utility.DecimalParse(a.RenderOrder),
                                   MarkerColor = a.MarkerColor,
                                   Code = a.Code,
                                   //ImageFile = a.VisitDate.Value.ToString(Constant.imageDateFormat) + "/" + a.VisitImage,
                                   ImageFile = a.VisitImage,
                               }).Distinct().ToList();

            var listSMVisitTracking = Global.Context.pp_GetVisitSMTracking(routeCD, distributorID, visitDate, username).ToList();
            #endregion
            #region listSSVisit
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
                                   SUPLatitudeStart = Utility.DecimalParseWithObject(a.SUPLatitudeStart),
                                   SUPLongtitudeStart = Utility.DecimalParseWithObject(a.SUPLongtitudeStart),
                                   SUPDistance = Utility.StringParseWithObject(a.SUPDistance)
                               }).Distinct().ToList();
            #endregion
            #region listASMVisit
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
                                    ASMLatitudeStart = Utility.DecimalParseWithObject(a.ASMLatitudeStart),
                                    ASMLongtitudeStart = Utility.DecimalParseWithObject(a.ASMLongtitudeStart),
                                    ASMDistance = Utility.StringParseWithObject(a.ASMDistance)
                                }).Distinct().ToList();
            #endregion

            var listImageDB = Global.Context.pp_GetVisitImageOneMonth(routeCD, distributorID, string.Empty, string.Empty, salesmanID, fisrtDateLastMonth, visitDate, SessionHelper.GetSession<string>("UserName")).ToList();
            var listImage = (from a in listImageDB
                             select new OutletVisitImage()
                             {
                                 DistributorID = a.DistributorID,
                                 ImageFile = a.ImageFile,
                                 OutletID = a.OutletID,
                                 VisitDate = a.VisitDate,
                                 strVisitDate = a.VisitDate.ToShortPattern()
                             }).Distinct().ToList();

            #region MERGE DATA
            foreach (OutletInRoute oir in listOutletInRoute)
            {
                oir.ListSMVisit = new List<OutletSMVisit>();
                oir.ListSSVisit = new List<OutletSSVisit>();
                oir.ListASMVisit = new List<OutletASMVisit>();
                oir.ListVisitImage = new List<OutletVisitImage>();

                oir.ListSMVisit.AddRange(listSMVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.SMTimeStart).Distinct().ToList());
                oir.ListSSVisit.AddRange(listSSVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.SUPTimeStart).Distinct().ToList());
                oir.ListASMVisit.AddRange(listASMVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.ASMTimeStart).Distinct().ToList());
                oir.ListVisitImage.AddRange(listImage.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderByDescending(a => a.VisitDate).Distinct().ToList());

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
            listSMVisit = listSMVisit.OrderBy(a => a.SMTimeStart).Distinct().ToList();
            listSSVisit = listSSVisit.OrderBy(a => a.SUPTimeStart).Distinct().ToList();
            listASMVisit = listASMVisit.OrderBy(a => a.ASMTimeStart).Distinct().ToList();
            listSMVisitTracking = listSMVisitTracking.OrderBy(a => a.StartTime).Distinct().ToList();

            var jsonResult = Json(new
            {
                html = listOutletInRoute,
                route = routeInfo,
                listSMVisit,
                listSSVisit,
                listASMVisit,
                listSMVisitTracking
            });
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        //DaiLV - 2016/08/23 - GetInfoOutlet
        [ActionAuthorize("GetOutletInRouteHeatMap")]
        [CompressFilter]
        public ActionResult GetOutletInRouteHeatMap(string regionID, string areaID, int? distributorID, string outletID, string routeID, string strVisitDate)
        {
            #region PARAM
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            distributorID = Utility.IntParse(distributorID);
            routeID = Utility.StringParse(routeID);
            
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            DateTime fisrtDateLastMonth = visitDate.AddMonths(-1);
            
            fisrtDateLastMonth = new DateTime(fisrtDateLastMonth.Year, fisrtDateLastMonth.Month, 1);
            string salesmanID = "";
            string salesmanName = "";
            if (routeID != null && distributorID != 0 && visitDate != null)
            {
                var SalemanInfoName = Global.Context.pp_GetSalemanID(visitDate, distributorID, routeID, username).FirstOrDefault();
                if (SalemanInfoName != null)
                {
                    salesmanID = SalemanInfoName.SalesmanID;
                    salesmanName = SalemanInfoName.SalesmanName;
                }
            }


            if (string.IsNullOrEmpty(routeID) && !string.IsNullOrEmpty(salesmanID))
            {
                var mcp = Global.Context.VisitPlanHistories.Where(a => a.VisitDate == visitDate && a.DistributorID == distributorID && a.SalesmanID == salesmanID).FirstOrDefault();
                if (mcp != null)
                {
                    routeID = mcp.RouteID;
                }
            }
            #endregion

            #region GET DATA

            var j = Global.Context.pp_GetVisitInfoOutlet(routeID, distributorID, outletID, salesmanID, visitDate, username).ToList();
            #region listOutletInRoute
            var listOutletInRoute = (from a in j
                                     select new OutletInRoute()
                                     {
                                         ASMID = a.ASMID,
                                         ASMName = a.ASMName,
                                         SaleSupID = a.SaleSupID,
                                         SaleSupName = a.SaleSupName,
                                         DistributorID = a.DistributorID,
                                         DistributorCode = a.DistributorCode,
                                         DistributorName = a.DistributorName,
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
                                         ISMCP = a.ISMCP,
                                         T2 = a.T2,
                                         T3 = a.T3,
                                         T4 = a.T4,
                                         T5 = a.T5,
                                         T6 = a.T6,
                                         T7 = a.T7,
                                         CN = a.CN,
                                         strVisitInWeek = a.strVisitInWeek
                                     }).Distinct().ToList();
            #endregion
            #region listSMVisit
            var listSMVisit = (from a in j
                               where a.SMTimeStart != null
                                    && a.SMLatitude != 0
                                    && a.SMLongitude != 0
                               select new OutletSMVisit()
                               {
                                   DistributorID = a.DistributorID,
                                   OutletID = a.OutletID,
                                   DropSize = Utility.StringParseWithDecimalDegit(a.DropSize),
                                   TotalAmt = Utility.StringParseWithDecimalDegit(a.TotalAmt),
                                   TotalSKU = Utility.StringParseWithDecimalDegit(a.TotalSKU),
                                   Reason = a.Reason,
                                   SMTimeStart = a.SMTimeStart,
                                   SMTimeEnd = a.SMTimeEnd,
                                   SMLatitude = a.SMLatitude,
                                   SMLongitude = a.SMLongitude,
                                   SMDistance = Utility.StringParseWithDecimalDegit(a.SMDistance),
                                   ISMCP = a.ISMCP,
                                   HasOrder = a.HasOrder,
                                   HasVisit = a.HasVisit,
                                   RN = Utility.DecimalParse(a.RenderOrder),
                                   MarkerColor = a.MarkerColor,
                                   Code = a.Code,
                                   //ImageFile = a.VisitDate.Value.ToString(Constant.imageDateFormat) + "/" + a.VisitImage,
                                   ImageFile = a.VisitImage,
                               }).Distinct().ToList();

            //var listSMVisitTracking = Global.Context.pp_GetVisitSMTracking(routeID, distributorID, visitDate, username).ToList();
            #endregion
            #region listSSVisit
            //var listSSVisit = (from a in j
            //                   where a.SUPTimeStart != null
            //                        && a.SUPLatitudeStart != 0
            //                        && a.SUPLongtitudeStart != 0
            //                   select new OutletSSVisit()
            //                   {
            //                       DistributorID = a.DistributorID,
            //                       OutletID = a.OutletID,
            //                       SUPTimeStart = a.SUPTimeStart,
            //                       SUPTimeEnd = a.SUPTimeEnd,
            //                       SUPLatitudeStart = Utility.DecimalParseWithObject(a.SUPLatitudeStart),
            //                       SUPLongtitudeStart = Utility.DecimalParseWithObject(a.SUPLongtitudeStart),
            //                       SUPDistance = Utility.StringParseWithObject(a.SUPDistance)
            //                   }).Distinct().ToList();
            #endregion
            #region listASMVisit
            //var listASMVisit = (from a in j
            //                    where a.ASMTimeStart != null
            //                        && a.ASMLatitudeStart != 0
            //                        && a.ASMLongtitudeStart != 0
            //                    select new OutletASMVisit()
            //                    {
            //                        DistributorID = a.DistributorID,
            //                        OutletID = a.OutletID,
            //                        ASMTimeStart = a.ASMTimeStart,
            //                        ASMTimeEnd = a.ASMTimeEnd,
            //                        ASMLatitudeStart = Utility.DecimalParseWithObject(a.ASMLatitudeStart),
            //                        ASMLongtitudeStart = Utility.DecimalParseWithObject(a.ASMLongtitudeStart),
            //                        ASMDistance = Utility.StringParseWithObject(a.ASMDistance)
            //                    }).Distinct().ToList();
            #endregion

            var listImageDB = Global.Context.pp_GetVisitImageOneMonthOutlet(routeID, distributorID, outletID, salesmanID, fisrtDateLastMonth, visitDate, SessionHelper.GetSession<string>("UserName")).ToList();
            var listImage = (from a in listImageDB
                             select new OutletVisitImage()
                             {
                                 DistributorID = a.DistributorID,
                                 ImageFile = a.ImageFile,
                                 OutletID = a.OutletID,
                                 VisitDate = a.VisitDate,
                                 strVisitDate = a.VisitDate.ToShortPattern()
                             }).Distinct().ToList();

            #region MERGE DATA
            foreach (OutletInRoute oir in listOutletInRoute)
            {
                oir.ListSMVisit = new List<OutletSMVisit>();
                oir.ListSSVisit = new List<OutletSSVisit>();
                oir.ListASMVisit = new List<OutletASMVisit>();
                oir.ListVisitImage = new List<OutletVisitImage>();

                oir.ListSMVisit.AddRange(listSMVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.SMTimeStart).Distinct().ToList());
                //oir.ListSSVisit.AddRange(listSSVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.SUPTimeStart).Distinct().ToList());
                //oir.ListASMVisit.AddRange(listASMVisit.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderBy(a => a.ASMTimeStart).Distinct().ToList());
                oir.ListVisitImage.AddRange(listImage.Where(a => a.OutletID == oir.OutletID && a.DistributorID == oir.DistributorID).Distinct().OrderByDescending(a => a.VisitDate).Distinct().ToList());

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
            //var routeInfo = Global.Context.pp_GetRouteInfoByUser(routeID, distributorID, string.Empty, string.Empty, string.Empty, visitDate, SessionHelper.GetSession<string>("UserName")).FirstOrDefault();
            listSMVisit = listSMVisit.OrderBy(a => a.SMTimeStart).Distinct().ToList();
            //listSSVisit = listSSVisit.OrderBy(a => a.SUPTimeStart).Distinct().ToList();
            //listASMVisit = listASMVisit.OrderBy(a => a.ASMTimeStart).Distinct().ToList();
            //listSMVisitTracking = listSMVisitTracking.OrderBy(a => a.StartTime).Distinct().ToList();

            var jsonResult = Json(new
            {
                salesmanID = salesmanID,
                salesmanName = salesmanName,
                html = listOutletInRoute,
                //route = routeInfo,
                listSMVisit,
                //listSSVisit,
                //listASMVisit,
                //listSMVisitTracking
            });
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [ActionAuthorize("RenderListSMLastLocation")]
        [CompressFilter]
        public ActionResult RenderListSMLastLocation(string strSMSelected, string regionID, string areaID, int? distributorID, string salesSupID, string strVisitDate)
        {
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            distributorID = Utility.IntParse(distributorID);
            salesSupID = Utility.StringParse(salesSupID);
            strSMSelected = Utility.StringParse(strSMSelected);
            string strSMViolate = string.Empty;
            List<string> listSM = new List<string>();
            if (!string.IsNullOrEmpty(strSMSelected))
            {
                listSM = strSMSelected.Split(',').Where(a => a != string.Empty).Distinct().ToList();
            }

            //var listSMLastLocation = ControllerHelper.GetSalemanLastLocationResult(distributorID.Value, salesSupID, string.Empty, visitDate)
            //    .Where(x => (x.RegionID == regionID || regionID == "") && (x.AreaID == areaID || areaID == "")).ToList();
            var listSMLastLocation = ControllerHelper.GetSalemanLastLocationResult(distributorID.Value, salesSupID, strSMSelected, visitDate)
                  .Where(x => (x.RegionID == regionID || regionID == "") && (x.AreaID == areaID || areaID == "")).ToList();
            var lstSalesManViolate = SessionHelper.GetSession<List<pp_GetSalesManViolateResult>>("SalesManViolate").ToList();
            var lstViolate = (from sm in listSMLastLocation
                              join ut in lstSalesManViolate
                                  on new { DistributorID = sm.DistributorID, SalesmanID = sm.SalesmanID } equals new { DistributorID = ut.DistributorID, SalesmanID = ut.SalesmanID }
                              orderby sm.SalesmanID
                              select sm.SalesmanID).Distinct().ToList();

            return Json(new
            {
                html = listSMLastLocation,
                infoViolate = lstViolate
            });
        }

        [ActionAuthorize("RenderListDistributorLocation")]//RenderListDistributorLocation
        [CompressFilter]
        public ActionResult RenderListDistributorLocation(string regionID, string areaID, string salesSupID, int? distributorID, string strVisitDate)
        {
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            distributorID = Utility.IntParse(distributorID);
            salesSupID = Utility.StringParse(salesSupID);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            //var model = Global.Context.pp_GetDistributorInfo(regionID, areaID, string.Empty, salesSupID, distributorID, string.Empty, string.Empty, visitDate, username).ToList();
            var modelD = Global.Context.pp_GetDistributorInfo(regionID, areaID, string.Empty, salesSupID, distributorID, string.Empty, string.Empty, visitDate, username).ToList();
            var model = (from c in modelD
                         join d in ControllerHelper.ListDistributor
                                   on new { c.DistributorID, c.RegionID } equals new { d.DistributorID, d.RegionID }
                         orderby c.DistributorID
                         select c).Distinct().ToList();
            var lstSalesManViolate = SessionHelper.GetSession<List<pp_GetSalesManViolateResult>>("SalesManViolate").ToList();
            var lstViolate = (from sm in model
                              join ut in lstSalesManViolate
                                  on new { DistributorID = sm.DistributorID } equals new { DistributorID = ut.DistributorID }
                              orderby sm.DistributorID
                              select sm.DistributorID).Distinct().ToList();
            return Json(new
            {
                html = model,
                infoViolate = lstViolate,
                lstSalesManViolate
            });
        }

        [ActionAuthorize("RenderListASMLastLocation")]
        [CompressFilter]
        public ActionResult RenderListASMLastLocation(string regionID, string areaID, string strVisitDate)
        {
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            int roleID = SessionHelper.GetSession<int>("RoleUser");
            Role role = Global.Context.Roles.FirstOrDefault(a => a.ID == roleID);
            var lstSalesManViolate = SessionHelper.GetSession<List<pp_GetSalesManViolateResult>>("SalesManViolate").ToList();
            // var listItem = ControllerHelper.GetASMVisitInfoResult(regionID, areaID, string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, visitDate);

            var listItem = Global.Context.pp_GetASMVisitInfo(regionID, areaID, string.Empty, visitDate, username).ToList();
            var lstViolate = (from sm in listItem.AsEnumerable()
                              join ut in lstSalesManViolate
                                  on new { RG = sm.RegionID, AI = sm.AreaID } equals new { RG = ut.RegionID, AI = ut.AreaID }
                              orderby sm.AreaID
                              select sm.AreaID).Distinct().ToList();
            return Json(new
            {
                html = listItem,
                infoViolate = lstViolate
            });
        }
        [ActionAuthorize("RenderListASMLastLocation")]
        [CompressFilter]
        public ActionResult GetSalesTeamLocation(string regionID, string areaID, string asmID, string strVisitDate, string type)
        {
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);

            var lstSalesManViolate = SessionHelper.GetSession<List<pp_GetSalesManViolateResult>>("SalesManViolate").ToList();
            var listASMSS = new List<pp_GetLastLocationASMResult>();
            if (PermissionHelper.CheckPermissionByFeature("NSM") || PermissionHelper.CheckPermissionByFeature("NSD"))
            {
                listASMSS = Global.Context.pp_GetLastLocationASM(regionID, areaID, "", "", visitDate, username).ToList();
            }
            else if (PermissionHelper.CheckPermissionByFeature("RSM"))
            {
                listASMSS = Global.Context.pp_GetLastLocationASM(regionID, areaID, username, "", visitDate, username).ToList();
            }
            else if (PermissionHelper.CheckPermissionByFeature("ASM"))
            {
                listASMSS = Global.Context.pp_GetLastLocationASM(regionID, areaID, "", username, visitDate, username).ToList();
            }
            var lstViolate = (from sm in listASMSS
                              join ut in lstSalesManViolate
                                  on new { RG = sm.RegionID, AI = sm.AreaID } equals new { RG = ut.RegionID, AI = ut.AreaID }
                              orderby sm.AreaID
                              select sm.AreaID).Distinct().ToList();
            if (!String.IsNullOrEmpty(asmID) || type == "SS")
            {
                lstViolate = (from sm in listASMSS
                              join ut in lstSalesManViolate
                                  on new { RG = sm.RegionID, AI = sm.AreaID } equals new { RG = ut.RegionID, AI = ut.AreaID }
                              orderby sm.SaleSupID
                              select ut.SaleSupID).Distinct().ToList();
            }
            return Json(new
            {
                html = listASMSS,
                infoViolate = lstViolate,
                lstSalesManViolate = lstSalesManViolate
            });
        }
        [ActionAuthorize("RenderListSSLastLocation")]
        [CompressFilter]
        public ActionResult RenderListSSLastLocation(string regionID, string areaID, string salesupID, int? distributorID, string strVisitDate)
        {
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            salesupID = Utility.StringParse(salesupID);
            distributorID = Utility.IntParse(distributorID);

            //var listItem = ControllerHelper.GetSUPVisitInfoResult(regionID, areaID, string.Empty, string.Empty, distributorID.Value, salesupID, string.Empty, string.Empty, visitDate);
            var listItem = Global.Context.pp_GetSUPVisitInfo(regionID, areaID, string.Empty, salesupID, distributorID.Value, visitDate, username).ToList();
            //var lstSalesManViolate = SessionHelper.GetSession<List<pp_GetSalesManViolateResult>>("SalesManViolate").ToList();
            //var lstViolate = (from sm in listItem
            //                  join ut in lstSalesManViolate
            //                      on new { DistributorID = sm.DistributorID } equals new { DistributorID = ut.DistributorID ?? 0 }
            //                  orderby sm.DistributorID
            //                  select sm.DistributorID).Distinct().ToList();
            return Json(new
            {
                html = listItem
            });
        }
        public ActionResult TerritoryTreeView(string strSearch)
        {
            var model = SessionHelper.GetSession<List<pp_GetTreeTerritoryResult>>("TreeTerritory");
            if (!string.IsNullOrEmpty(strSearch))
            {

                //var newModel = model.Where(a => (a.Level == 4 &&
                //                                (a.ID.ToUpper().Contains(strSearch.Trim().ToUpper()) || a.Name.ToUpper().Contains(strSearch.Trim().ToUpper())
                //                                || a.Name.ToUpper().Split('(')[1].Split('-')[1].Substring(1, a.Name.ToUpper().Split('(')[1].Split('-')[1].Length - 2) == strSearch.ToUpper()
                //                                ))
                //                                ||
                //                                (a.Level == 3 && a.Name.ToUpper().Contains(strSearch.Trim().ToUpper()))
                //                                ).ToList();

                var tempModel = model.Where(a => a.Name != null && a.ID != null).ToList();
                var newModel = tempModel.Where(a => (a.Level == 4 && a.Name.ToUpper().Contains(strSearch.Trim().ToUpper()))
                                              ).ToList();
                var mergeModel = newModel;
                foreach (var level4 in newModel)
                {
                    var parentLevel4 = from p in tempModel where p.ParentID == level4.ID select p;
                    mergeModel = mergeModel.Union(parentLevel4).ToList();
                }
                foreach (var item3 in newModel)
                {
                    var parentLevel3 = from p in tempModel where p.ID == item3.ParentID select p;
                    mergeModel = mergeModel.Union(parentLevel3).ToList();
                    foreach (var item2 in parentLevel3)
                    {
                        var parentLevel2 = from p in tempModel where p.ID == item2.ParentID select p;
                        mergeModel = mergeModel.Union(parentLevel2).ToList();
                        foreach (var item1 in parentLevel2)
                        {
                            var parentLevel1 = from p in tempModel where p.ID == item1.ParentID select p;
                            mergeModel = mergeModel.Union(parentLevel1).ToList();
                            foreach (var item in parentLevel1)
                            {
                                var parentLevel0 = from p in tempModel where p.ID == item.ParentID select p;
                                mergeModel = mergeModel.Union(parentLevel0).ToList();
                            }
                        }
                    }
                }
                return PartialView(mergeModel);
            }
            else
                return PartialView(model);
        }
        public static void CreateTreeViewLeftPanel(List<pp_GetTreeTerritoryResult> listNote, MVCxTreeViewNodeCollection nodesCollection, string parentID)
        {
            var lstTemp = listNote.Where(x => x.ParentID == parentID).OrderBy(x => x.Name);
            var lstSalesManViolate = SessionHelper.GetSession<List<pp_GetSalesManViolateResult>>("SalesManViolate").ToList();
            foreach (var row in lstTemp)
            {
                //string strTreeName = "<span onlick=\"alert('" + row.ID.ToString() + "');\" >"+ row.Name +"</span>";
                string name = row.Name;
                string id = row.TreeID.ToString();
                if (id != null && name != null)
                {
                    MVCxTreeViewNode node = nodesCollection.Add(name, id);
                    foreach (var item in lstSalesManViolate)
                    {
                        if (item.BranchID == id || item.RegionID == id || item.AreaID == id ||
                            Utility.StringParse(item.DistributorID) == id
                            || item.RouteID == id || item.SalesmanID == id)
                        {
                            node.NodeStyle.BackColor = Color.Yellow;
                            break;
                        }
                    }
                    CreateTreeViewLeftPanel(listNote, node.Nodes, id);
                }
            }
        }
        public ActionResult RenderSalesTeamOnMap()
        {
            string listSMSelected = string.Empty;
            string distributorIDSelected = string.Empty;
            string salesupIDSelected = string.Empty;
            string areaIDSelected = string.Empty;
            string regionIDSelected = string.Empty;
            string routeIDSelected = string.Empty;


            var model = SessionHelper.GetSession<List<pp_GetTreeTerritoryResult>>("TreeTerritory");
            if (model != null)
            {
                if (model.Count > 0)
                {
                    var item = model.Where(a => a.Level == 4).FirstOrDefault();
                    if (PermissionHelper.CheckPermissionByFeature("Admin"))
                    {
                        item = model.Where(a => a.Level == 1).FirstOrDefault();
                    }
                    else if (PermissionHelper.CheckPermissionByFeature("NSM") || PermissionHelper.CheckPermissionByFeature("NSD"))
                    {
                        item = model.Where(a => a.Level == 1).FirstOrDefault();
                    }
                    else if (PermissionHelper.CheckPermissionByFeature("ASM"))
                    {
                        item = model.Where(a => a.Level == 2).FirstOrDefault();
                    }
                    else if (PermissionHelper.CheckPermissionByFeature("SalesSup"))
                    {
                        item = model.Where(a => a.Level == 4).FirstOrDefault(a => a.ID == username);
                    }
                    else if (PermissionHelper.CheckPermissionByFeature("Distributor"))
                    {
                        var dis = (from x in Global.Context.Distributors where x.LoginID == username select x).FirstOrDefault();
                        if (dis != null)
                        {
                            item = model.Where(a => a.Level == 3).FirstOrDefault(a => a.ID == dis.DistributorID.ToString());
                            item = model.Where(a => a.Level == 4).FirstOrDefault(a => a.ParentID == item.ID);
                        }

                    }
                    if (item != null)
                    {
                        if (item.Level == 5)
                        {
                            routeIDSelected = Utility.StringParse(item.Name.Split('(')[1].Split('-')[1].Split(')')[0]);
                            listSMSelected = item.ID;
                            salesupIDSelected = item.ParentID;
                            distributorIDSelected = model.FirstOrDefault(a => a.ID == salesupIDSelected).ParentID;
                            areaIDSelected = model.FirstOrDefault(a => a.ID == distributorIDSelected).ParentID;
                            regionIDSelected = model.FirstOrDefault(a => a.ID == areaIDSelected).ParentID;
                        }
                        else if (item.Level == 4)
                        {
                            salesupIDSelected = item.ID;
                            distributorIDSelected = model.FirstOrDefault(a => a.ID == salesupIDSelected).ParentID;
                            areaIDSelected = model.FirstOrDefault(a => a.ID == distributorIDSelected).ParentID;
                            regionIDSelected = model.FirstOrDefault(a => a.ID == areaIDSelected).ParentID;
                        }
                        else if (item.Level == 3)
                        {
                            distributorIDSelected = item.ID;
                            areaIDSelected = model.FirstOrDefault(a => a.ID == distributorIDSelected).ParentID;
                            regionIDSelected = model.FirstOrDefault(a => a.ID == areaIDSelected).ParentID;
                        }
                        else if (item.Level == 2)
                        {
                            areaIDSelected = item.ID;
                            regionIDSelected = model.FirstOrDefault(a => a.ID == areaIDSelected).ParentID;
                        }
                        else if (item.Level == 1)
                        {
                            regionIDSelected = item.ID;
                        }
                        else if (item.Level == 0)
                        {
                        }
                    }
                }
            }
            return Json(new
            {
                listSMSelected,
                distributorIDSelected,
                salesupIDSelected,
                areaIDSelected,
                regionIDSelected,
                routeIDSelected
            });
        }
        public ActionResult TerritoryTreeViewSelected(string itemId)
        {
            string listSMSelected = string.Empty;
            string distributorIDSelected = string.Empty;
            string salesupIDSelected = string.Empty;
            string areaIDSelected = string.Empty;
            string regionIDSelected = string.Empty;
            string routeIDSelected = string.Empty;
            if (!string.IsNullOrEmpty(itemId))
            {
                var model = SessionHelper.GetSession<List<pp_GetTreeTerritoryResult>>("TreeTerritory");
                if (model != null)
                {
                    if (model.Count > 0)
                    {
                        var item = model.FirstOrDefault(a => a.TreeID == itemId);
                        if (item != null)
                        {
                            if (item.Level == 5)
                            {
                                routeIDSelected = Utility.StringParse(item.Name.Split('(')[1].Split('-')[1].Split(')')[0]);
                                listSMSelected = item.ID;
                                var treeSS = model.FirstOrDefault(a => a.TreeID == item.ParentID);
                                if (treeSS != null)
                                {
                                    salesupIDSelected = treeSS.ID;

                                    var treeD = model.FirstOrDefault(a => a.TreeID == treeSS.ParentID);
                                    if (treeD != null)
                                    {
                                        distributorIDSelected = treeD.ID;

                                        var treeA = model.FirstOrDefault(a => a.TreeID == treeD.ParentID);

                                        if (treeA != null)
                                        {
                                            areaIDSelected = treeA.ID;

                                            var treeS = model.FirstOrDefault(a => a.TreeID == treeA.ParentID);

                                            if (treeS != null)
                                            {
                                                regionIDSelected = treeS.ID;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (item.Level == 4)
                            {
                                salesupIDSelected = item.ID;
                                var treeD = model.FirstOrDefault(a => a.TreeID == item.ParentID);
                                if (treeD != null)
                                {
                                    distributorIDSelected = treeD.ID;

                                    var treeA = model.FirstOrDefault(a => a.TreeID == treeD.ParentID);

                                    if (treeA != null)
                                    {
                                        areaIDSelected = treeA.ID;

                                        var treeS = model.FirstOrDefault(a => a.TreeID == treeA.ParentID);

                                        if (treeS != null)
                                        {
                                            regionIDSelected = treeS.ID;
                                        }
                                    }
                                }
                            }
                            else if (item.Level == 3)
                            {
                                distributorIDSelected = item.ID;
                                var treeA = model.FirstOrDefault(a => a.TreeID == item.ParentID);

                                if (treeA != null)
                                {
                                    areaIDSelected = treeA.ID;

                                    var treeS = model.FirstOrDefault(a => a.TreeID == treeA.ParentID);

                                    if (treeS != null)
                                    {
                                        regionIDSelected = treeS.ID;
                                    }
                                }
                            }
                            else if (item.Level == 2)
                            {
                                areaIDSelected = item.ID;
                                var treeS = model.FirstOrDefault(a => a.TreeID == item.ParentID);

                                if (treeS != null)
                                {
                                    regionIDSelected = treeS.ID;
                                }
                            }
                            else if (item.Level == 1)
                            {
                                regionIDSelected = item.ID;
                            }
                            else if (item.Level == 0)
                            {
                            }
                        }
                    }
                }
            }

            return Json(new
            {
                listSMSelected,
                distributorIDSelected,
                salesupIDSelected,
                areaIDSelected,
                regionIDSelected,
                routeIDSelected
            });
        }
        #region Show outlet
        public ActionResult TerritoryTreeViewSelectedOutlet(string itemId)
        {
            string distributorIDSelected = string.Empty;
            string areaIDSelected = string.Empty;
            string regionIDSelected = string.Empty;
            if (!string.IsNullOrEmpty(itemId))
            {
                var model = SessionHelper.GetSession<List<pp_GetTreeTerritoryResult>>("TreeTerritory");
                if (model != null)
                {
                    if (model.Count > 0)
                    {
                        var item = model.FirstOrDefault(a => a.TreeID == itemId);
                        if (item != null)
                        {

                            if (item.Level == 3)
                            {
                                distributorIDSelected = item.ID;
                                var treeA = model.FirstOrDefault(a => a.TreeID == item.ParentID);

                                if (treeA != null)
                                {
                                    areaIDSelected = treeA.ID;

                                    var treeS = model.FirstOrDefault(a => a.TreeID == treeA.ParentID);

                                    if (treeS != null)
                                    {
                                        regionIDSelected = treeS.ID;
                                    }
                                }
                            }
                            else if (item.Level == 2)
                            {
                                areaIDSelected = item.ID;
                                var treeS = model.FirstOrDefault(a => a.TreeID == item.ParentID);

                                if (treeS != null)
                                {
                                    regionIDSelected = treeS.ID;
                                }
                            }
                            else if (item.Level == 1)
                            {
                                regionIDSelected = item.ID;
                            }
                            else if (item.Level == 0)
                            {
                            }
                        }
                    }
                }
            }
            return Json(new
            {
                distributorIDSelected,
                areaIDSelected,
                regionIDSelected,
            });
        }
        //DAILV - 23-08-2016
        [ActionAuthorize("RenderListAllOutletLocation")]//RenderListDistributorLocation
        [CompressFilter]
        public ActionResult RenderListAllOutletLocation(string regionID, string areaID, int? distributorID, string strVisitDate, string role)
        {
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            distributorID = Utility.IntParse(distributorID);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            var model = Global.Context.pp_GetOutletLocation(visitDate, regionID, areaID, distributorID, username).ToList();
            int ConfigZoom = (ControllerHelper.valueCustomSetting("ConfigZoomMap")) != null ? Utility.IntParse(ControllerHelper.valueCustomSetting("ConfigZoomMap")) : 0;
            var jsonResult = Json(new { model = model, ConfigZoom = ConfigZoom });
            jsonResult.MaxJsonLength = Int32.MaxValue;
            return jsonResult;
        }
        #endregion
        #region ReportSalesAssessmentChart
        [CompressFilter]
        public ActionResult AJAXReportSalesAssessment(string groupby, string strVisitDate, string branchID, string regionID, string areaID, string salesupID, int? distributorID, string routeCD, string salesmanID)
        {
            if (string.IsNullOrEmpty(strVisitDate))
            {
                strVisitDate = DateTime.Now.ToShortDateString();
            }
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            //groupby = Utility.StringParse(groupby);
            branchID = Utility.StringParse(branchID);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            salesupID = Utility.StringParse(salesupID);
            distributorID = Utility.IntParse(distributorID);
            routeCD = Utility.StringParse(routeCD);
            salesmanID = Utility.StringParse(salesmanID);
            groupby = Utility.StringParse(groupby);

            //Tab Revenue
            var model = Global.Context.pp_ReportSalesAssessmentDaily(visitDate, regionID, areaID, string.Empty, distributorID, salesupID, routeCD, salesmanID, username).ToList();
            var result = new List<ReportSMVisitSummaryChartData>();
            var dynamicQuery = model.AsQueryable().GroupBy("new(" + groupby + "ID, " + groupby + "Name)", "it").Select("new(Key." + groupby + "ID as ID, Key." + groupby + "Name as Name, SUM(TotalAmount) as TotalAmount, SUM(OrderCount) as OrderCount, SUM(TotalSKU) as TotalSKU, SUM(TotalQuantity) as TotalQuantity, AVERAGE(LPPC) as LPPC, AVERAGE(SO_MCP) as SOMCP, AVERAGE(Visit_MCP) as VisitMCP, SUM(OutletMustVisit) as OutletMustVisit, SUM(OutletVisited) as OutletVisited)");
            foreach (dynamic item in dynamicQuery)
            {
                result.Add(new ReportSMVisitSummaryChartData()
                {
                    Code = Utility.StringParse(item.ID),
                    Name = item.Name,
                    strTotalAmount = Utility.StringParseWithRoundingDecimalDegit(item.TotalAmount),
                    strOrderCount = Utility.StringParseWithRoundingDecimalDegit(item.OrderCount),
                    strTotalSKU = Utility.StringParseWithDecimalDegit(item.TotalSKU),
                    strTotalQuantity = Utility.StringParseWithDecimalDegit(item.TotalQuantity),
                    strLPPC = Utility.StringParseWithDecimalDegit(item.LPPC),
                    strSOMCP = Utility.StringParseWithDecimalDegit(item.SOMCP) + " %",
                    strVisitMCP = Utility.StringParseWithDecimalDegit(item.VisitMCP) + " %",
                    strOutletMustVisit = Utility.StringParseWithRoundingDecimalDegit(item.OutletMustVisit),
                    strOutletVisited = Utility.StringParseWithRoundingDecimalDegit(item.OutletVisited)
                });
            }
            var resultInfoOutlet = new List<ReportInfoOutLetVisit>();
            var resultInfoVisit = new List<ReportInfoOutLetVisit>();
            var resultSummary = new List<ReportInfoOutLetVisit>();
            //var modelInfo = new List<pp_ReportVisitResult>();

            if (!string.IsNullOrEmpty(salesmanID))
            {
                //var modelInfo = ControllerHelper.GetReportVisitResult(regionID, areaID, Convert.ToInt32(distributorID), salesupID,salesmanID,routeCD,visitDate).ToList();
                var modelInfo = Global.Context.pp_ReportVisit(visitDate, regionID, areaID, string.Empty, distributorID, salesupID, routeCD, salesmanID, string.Empty, string.Empty, username).ToList();
                string src = Url.Content(Constant.OutletImageFolder);
                string a = "<a href=\"" + src + "{0}\"><img src=\"" + src + "{1}\" style=\"max-width: 50px; padding-right: 5px; padding-top: 5px;\" alt=\"\"  rel=\"zoom\" class=\"smoothZoom\" /></a>";

                //Tab Info of Outlet
                var dynamicQueryInfo = modelInfo.AsQueryable().Select("new(" + groupby + "ID as ID, " + groupby + "Name as Name, OutletID, OutletName" +
                   ",Mobile, Address, ImgAvata)");
                foreach (dynamic item in dynamicQueryInfo)
                {
                    resultInfoOutlet.Add(new ReportInfoOutLetVisit()
                    {
                        SalesmanID = Utility.StringParse(item.ID),
                        SalesmanName = item.Name,
                        OutLetID = Utility.StringParse(item.OutletID),
                        OutLetName = item.OutletName,
                        Mobile = item.Mobile,
                        Address = item.Address,
                        ImageFile = string.Format(a, item.ImgAvata, item.ImgAvata)
                        //ImageFile = Constant.OutletImageFolder + item.ImgAvata
                    });
                }
                //Tab Info Visit
                var dynamicQueryVisit = modelInfo.AsQueryable().Select("new(" + groupby + "ID as ID, " + groupby + "Name as Name, OutletID, OutletName" +
                   ",strIsMCP, Distance, IsEnableAirPlaneMode, IsEnableGPSMode, IsEnableNetworkMode , StartTime, EndTime, strTimeSpanVisit, strTimeSpanMove, HasOrder, TotalAmount)");
                foreach (dynamic item in dynamicQueryVisit)
                {
                    resultInfoVisit.Add(new ReportInfoOutLetVisit()
                    {
                        SalesmanID = Utility.StringParse(item.ID),
                        SalesmanName = item.Name,
                        OutLetID = Utility.StringParse(item.OutletID),
                        OutLetName = item.OutletName,
                        StartTime = item.StartTime.ToShortTimeString(),
                        EndTime = item.EndTime.ToShortTimeString(),
                        Duration = item.strTimeSpanVisit,
                        TimeSpanMove = item.strTimeSpanMove,
                        strIsMCP = item.strIsMCP,
                        Distance = Utility.StringParse(item.Distance),
                        IsEnableAirPlaneMode = item.IsEnableAirPlaneMode == "1" ? "ON" : "OFF",
                        IsEnableGPSMode = item.IsEnableGPSMode == "1" ? "ON" : "OFF",
                        IsEnableNetworkMode = item.IsEnableNetworkMode == "1" ? "ON" : "OFF",
                        HasOrder = item.HasOrder,
                        strTotalAmout = Utility.StringParseWithRoundingDecimalDegit(item.TotalAmount)
                    });
                }
                //Tab Summary
                if (groupby == "Salesman" || groupby == "SalesSup")
                {
                    var modelInfoSummary = Global.Context.sp_InfoVisit(visitDate, distributorID, salesmanID, username);
                    var lstSalesManViolate = SessionHelper.GetSession<List<pp_GetSalesManViolateResult>>("SalesManViolate").ToList();
                    var dynamicQuerySummary = modelInfoSummary.AsQueryable().Select(" new(" + groupby + "ID as ID, " + groupby + "Name as Name, SaleSupName" +
                       ",NumberOutletVisited, NumberOutletRemain, NumberOrder, DistributorID)").Take(1);
                    foreach (dynamic item in dynamicQuerySummary)
                    {
                        resultSummary.Add(new ReportInfoOutLetVisit()
                        {
                            SalesmanID = Utility.StringParse(item.ID),
                            SalesmanName = item.Name,
                            SaleSupName = item.SaleSupName,
                            DistributorID = Utility.StringParse(item.DistributorID),
                            OutletVisited = Utility.StringParseWithRoundingDecimalDegit(item.NumberOutletVisited),
                            OutletRemain = Utility.StringParseWithRoundingDecimalDegit(item.NumberOutletRemain),
                            OrderCount = Utility.StringParseWithRoundingDecimalDegit(item.NumberOrder),
                            Compliance = (from sm in lstSalesManViolate
                                          where sm.DistributorID == item.DistributorID && sm.SalesmanID == Utility.StringParse(item.ID)
                                          select sm.SalesmanID).Distinct().FirstOrDefault() != null ? Utility.Phrase("Value_Yes") : Utility.Phrase("NonCompliance")
                        });
                    }
                }
            }
            return Json(new
            {
                list = result,
                lstInfoVisit = resultInfoVisit,
                lstInfoOutLet = resultInfoOutlet,
                lstSummary = resultSummary
            });
        }

        public ActionResult ReportSalesAssessmentDaily(string groupby)
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
            if (PermissionHelper.CheckPermissionByFeature("Admin"))
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
        #endregion
        #endregion
        #endregion

        #region TEST
        [CompressFilter]
        public ActionResult SimulatedAnnealing()
        {
            return View();
        }



        public JavaScriptResult abc()
        {
            return JavaScript("");
        }
        #endregion
        #region TEST INTERFACE
        [ActionAuthorize("Tracking_TerritoryPerformance")]
        [CompressFilter]
        public ActionResult TerritoryPerformance_Beta(FormCollection formParam, string strVisitDate, string strSearch)
        {
            var model = new HomeVM();
            if (string.IsNullOrEmpty(strVisitDate))
            {
                model.VisitDate = DateTime.Now;
            }
            else
            {
                model.VisitDate = Utility.DateTimeParse(strVisitDate);
            }

            model.listSaleman = new List<Map_Salesman>();


            model.strDate = DateTime.Now.ToString(Constant.ShortDatePattern);

            model.ListRegion = ControllerHelper.GetListRegion(string.Empty);
            model.ListArea = ControllerHelper.GetListArea(string.Empty);
            model.listSS = ControllerHelper.GetListSaleSup(string.Empty);
            model.listDis = ControllerHelper.GetListDistributor(string.Empty, string.Empty);
            var lstSalesManViolate = Global.Context.pp_GetSalesManViolate(Utility.DateTimeParse(strVisitDate), username).GroupBy(x => x.DistributorID).ToList();
            SessionHelper.SetSession("SalesManViolate", Global.Context.pp_GetSalesManViolate(model.VisitDate, username).ToList());
            SessionHelper.SetSession("TreeTerritory", Global.Context.pp_GetTreeTerritory(model.VisitDate, username).ToList());

            model.listCategory = ControllerHelper.ListCategory("Coverage", strVisitDate);

            if (!string.IsNullOrEmpty(strSearch))
                ViewBag.Search = strSearch;
            else
                ViewBag.Search = "";

            return View(model);
        }
        #endregion
    }
    #endregion
}