using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using WebManageFridgeMQTT.Models;
using WebManageFridgeMQTT.Utility;

namespace WebManageFridgeMQTT.Controllers
{
    public class GeoFridgeController : Controller
    {
        //
        // GET: /GeoFridge/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Index(string strDate)
        {
            //List<Shop> model = new List<Shop>();
            //DateTime Today = DateTime.Now;
            //if (!String.IsNullOrEmpty(strDate))
            //{
            //    Today = DateTime.Parse(strDate);
            //}
            //List<ObjParamSP> listParam = new List<ObjParamSP>();
            //listParam.Add(new ObjParamSP() { Key = "Today", Value = Today });
            //listParam.Add(new ObjParamSP() { Key = "UserID", Value = "" });
            //DataTable data = Utility.Helper.QueryStoredProcedure("GetInfoShopBy", listParam);
            //model = Helper.DataTableToList<Shop>(data);
            return View();
        }

        public ActionResult GetInfoShopBy(string strDate)
        {
            DateTime Today = DateTime.Now;
            if (!String.IsNullOrEmpty(strDate))
            {
                Today = DateTime.Parse(strDate);
            }
            List<ObjParamSP> listParam = new List<ObjParamSP>();
            listParam.Add(new ObjParamSP() { Key = "Today", Value = Today });
            listParam.Add(new ObjParamSP() { Key = "UserID", Value = "" });
            DataTable data = Utility.Helper.QueryStoredProcedure("GetInfoShopBy", listParam);
            return Json(data);
        }
    }
}
