using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using DMSERoute.Helpers;
using DMSERoute.Helpers.Html;
using eRoute.Filters;
using eRoute.Models;
using eRoute.Models.ViewModel;

namespace eRoute.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class MapTestController : Controller
    {
        //
        // GET: /MapTest/

        public ActionResult Index()
        {
            return View();
        }

        #region TerritoryPerformance
        [ActionAuthorize("TerritoryPerformance")]
        [CompressFilter]
        public ActionResult TerritoryPerformance(FormCollection formParam)
        {
            var model = new HomeVM();

            model.listSaleman = new List<Map_Salesman>();

            model.VisitDate = DateTime.Now;
            model.strDate = DateTime.Now.ToString(Constant.ShortDatePattern);

            model.ListRegion = ControllerHelper.GetListRegion(string.Empty);
            model.ListArea = ControllerHelper.GetListArea(string.Empty);
            model.listSS = ControllerHelper.GetListSaleSup(string.Empty);
            model.listDis = ControllerHelper.GetListDistributor(string.Empty, string.Empty);

            return View(model);
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
            var j = Global.Context.pp_GetRouteByUser(routeCD, distributorID, regionID, areaID, SessionHelper.GetSession<string>("UserName")).ToList();
            return Json(new
            {
                html = j
            });
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
                                         Latitude = a.Latitude.Value,
                                         Longtitude = a.Longtitude.Value,
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
                                         SUPLatitudeStart = a.SUPLatitudeStart,
                                         SUPLongtitudeStart = a.SUPLongtitudeStart,
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
                                        ASMLatitudeStart = a.ASMLatitudeStart,
                                        ASMLongtitudeStart = a.ASMLongtitudeStart,
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
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            distributorID = Utility.IntParse(distributorID);
            salesSupID = Utility.StringParse(salesSupID);
            strSMSelected = Utility.StringParse(strSMSelected);

            List<string> listSM = new List<string>();
            if (!string.IsNullOrEmpty(strSMSelected))
            {
                listSM = strSMSelected.Split(',').Where(a => a != string.Empty).Distinct().ToList();
            }

            var listSMLastLocation = Global.Context.pp_GetSalemanLastLocation(SessionHelper.GetSession<string>("UserName"), salesSupID, distributorID, string.Empty, visitDate).ToList();

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

        [ActionAuthorize("RenderListASMLastLocation")]
        [CompressFilter]
        public ActionResult RenderListASMLastLocation(string regionID, string areaID, string strVisitDate)
        {
            DateTime visitDate = Utility.DateTimeParse(strVisitDate);
            regionID = Utility.StringParse(regionID);
            areaID = Utility.StringParse(areaID);
            var listItem = Global.Context.pp_GetASMVisitInfo(regionID, areaID, string.Empty, visitDate, SessionHelper.GetSession<string>("UserName")).ToList();

            return Json(new
            {
                html = listItem
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
            var listItem = Global.Context.pp_GetSUPVisitInfo(regionID, areaID, string.Empty, salesupID, distributorID, visitDate, SessionHelper.GetSession<string>("UserName")).ToList();

            return Json(new
            {
                html = listItem
            });
        }
        #endregion 
        #endregion
    }
}