using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebManageFridgeMQTT.Models;
using WebManageFridgeMQTT.Utility;

namespace WebManageFridgeMQTT.Controllers
{
    public class MonitoringController : Controller
    {
        //
        // GET: /Monitoring/

        public ActionResult Index()
        {
            DeviceInfoMV model = new DeviceInfoMV();
            try
            {
                model.TreeDevice = Global.Context.GetTreeThietBi("").ToList();
                model.ListDeviceInfo = Global.Context.Sp_GetInfoDevice("").ToList();
                model.ListCongTrinh = Global.Context.GetInfoCongTrinh("").ToList();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            SessionHelper.SetSession<List<GetTreeThietBiResult>>("ListTreeDevice", model.TreeDevice);
            return View(model);
        }

        public ActionResult PopupReport()
        {
            DeviceActivity model = new DeviceActivity();
            return PartialView("PopupReport");
        }
        public ActionResult GetThietBi(string equipmentId, bool isParent)
        {
            var model = Global.Context.GetTreeThiet_ById(equipmentId, isParent).ToList();
            return Json(new { info = model }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TreeViewSelected(string strSearch)
        {
            var model = Global.Context.GetTreeThietBi("").ToList();
            var temp = new List<GetTreeThietBiResult>();
            if (string.IsNullOrWhiteSpace(strSearch))
            {
                return PartialView(model);
            }
            else
            {
             var  ChildModel = model.Where(x=> x.Cap == 1 && x.Name.Contains(strSearch.Trim())).ToList();
               var mergeModel = ChildModel;
               foreach (var level1 in ChildModel)
               {
                   var ParentModel = from p in model where p.Id == level1.Father select p;
                   mergeModel = mergeModel.Union(ParentModel).ToList();
               }
               return PartialView(mergeModel);
            }
        }
        public ActionResult GetInfoDeviceById(string id)
        {
            var model = Global.Context.Sp_GetInfoDeviceById(id).FirstOrDefault();
            return Json(model);
        }
        public ActionResult Device(FormCollection formParam,  string strSearch)
        {
            
            DeviceInfoMV model = new DeviceInfoMV();
            try
            {
                model.ListDeviceInfo = Global.Context.Sp_GetInfoDevice("").ToList();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            
            return View(model.ListDeviceInfo);
        }

        public ActionResult DeviceModify(string thietBiID, string strFromDate, string strToDate)
        {
            DeviceActivity model = new DeviceActivity();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.ThietBiID = thietBiID;
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model.ListDataModify = Global.Context.GetInfoDeviceModify(model.ThietBiID, model.FromDate, model.ToDate).ToList();
            return PartialView("DeviceModify", model);
        }
        public ActionResult DeviceActivity(string thietBiID)
        {
            ActivityChip model = new ActivityChip();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.infoDevice = Global.Context.Sp_GetInfoDeviceById(thietBiID).FirstOrDefault();
                DateTime FromDate = DateTime.Now.AddMonths(-1);
                DateTime ToDate = DateTime.Now;
                model.ListData = Global.Context.GetInfoDeviceActivity(thietBiID, FromDate, ToDate).OrderByDescending(x => x.ThoiGian).Take(5).ToList();
            }
            return PartialView("DeviceActivity", model);
        }
        public ActionResult PopupActivity(string thietBiID, string strFromDate, string strToDate)
        {
            DeviceActivity model = new DeviceActivity();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.ThietBiID = thietBiID;
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }

            //TEST
            model.FromDate = DateTime.Now.AddMonths(-1);
            model.ToDate = DateTime.Now;
            model.ListData = Global.Context.GetInfoDeviceActivity(model.ThietBiID, model.FromDate, model.ToDate).OrderByDescending(x => x.ThoiGian).ToList();
            return PartialView("PopupActivity", model);
        }

        public ActionResult DeviceMove(string thietBiID, string strFromDate, string strToDate)
        {
            DeviceActivity model = new DeviceActivity();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.ThietBiID = thietBiID;
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }

            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model.ListDataMove = Global.Context.GetInfoDeviceMove(thietBiID, model.FromDate, model.ToDate).ToList();
            return PartialView("DeviceMove", model);
        }
        public ActionResult DeviceReport(string thietBiID, string strFromDate, string strToDate)
        {
            DeviceActivity model = new DeviceActivity();
            if (!string.IsNullOrEmpty(thietBiID))
            {
                model.ThietBiID = thietBiID;
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }

            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }
            DateTime FromDate = DateTime.Now.AddMonths(-1);
            DateTime ToDate = DateTime.Now;
            model.ListDataReport = Global.Context.GetInfoDeviceReport(thietBiID, model.FromDate, model.ToDate).ToList();
            return PartialView("DeviceReport", model);
        }

        public ActionResult GetDeviceByCongTrinh(string congTrinhId)
        {
            List<GetTreeThietBiResult> model = new List<GetTreeThietBiResult>();
            model = Global.Context.GetTreeThietBi(congTrinhId).ToList();
            ViewData["ParentID"] = 1;
            ViewData["DisplayHeader"] = true;
            return PartialView("TreeViewDevice", model);
        }
        public ActionResult SearchDeviceBy(string inputDevice)
        {
            List<GetTreeThietBiResult> model = new List<GetTreeThietBiResult>();
            if (SessionHelper.GetSession<List<GetTreeThietBiResult>>("ListTreeDevice") != null)
            {
                var data = SessionHelper.GetSession<List<GetTreeThietBiResult>>("ListTreeDevice");
                if (inputDevice == "")
                {
                    model = data.ToList();
                    ViewData["ParentID"] = 0;
                }
                else
                {
                    model = data.Where(x => x.Cap == 1 && x.Name.Contains(inputDevice)).ToList();
                    ViewData["ParentID"] = 1;
                }
                
            }
            ViewData["inputDevice"] = inputDevice;
            return PartialView("TreeViewDevice", model);
        }

        #region CongTrinh
        public ActionResult TaiChinhCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            string totalMoney = "0";
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;
                model.ListTaiChinh = Global.Context.CongTrinhGetInfoQuanLyThuChi(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
                if(model.ListTaiChinh != null)
                {
                    
                }

                ViewData["TotalMoney"] = totalMoney;
            }

            return PartialView("TaiChinhCT", model);
        }
        public ActionResult SanLuongCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;
                model.ListSanLuong = Global.Context.CongTrinhGetInfoBCSanLuong(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
            }

            return PartialView("SanLuongCT", model);
        }
        public ActionResult CPDeviceCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;
                model.ListThietBi = Global.Context.CongTrinhGetInfoBCThietbi(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
            }

            return PartialView("CPDeviceCT", model);
        }
        public ActionResult CPVatTuCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;
                model.ListVatTu = Global.Context.CongTrinhGetInfoBCVatTu(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
            }

            return PartialView("CPVatTuCT", model);
        }
        public ActionResult ThiCongCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;
                model.ListThiCong = Global.Context.CongTrinhGetInfoBCQuyTrinhThiCong(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
            }
            return PartialView("ThiCongCT", model);
        }
        public ActionResult CocDetailCT(string congTrinhId, string cocId)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            if (!string.IsNullOrEmpty(congTrinhId) && !string.IsNullOrEmpty(cocId))
            {
                model.CongTrinhId = congTrinhId;
                model.ListThiCongCoc = Global.Context.CongTrinhGetInfoBCQuyTrinhThiCongCoc(model.CongTrinhId, cocId, null, null).ToList();
                model.ListThiCongChiTiet = Global.Context.CongTrinhGetInfoBCQuyTrinhThiCongChiTiet(model.CongTrinhId, cocId, null, null).ToList();
            }
            return PartialView("CocDetailCT", model);
        }
        public ActionResult NhanVienCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = DateTime.Parse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = DateTime.Parse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;
                model.ListNhanVien = Global.Context.CongTrinhGetInfoDSNhanVien(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
            }

            return PartialView("NhanVienCT", model);
        }


        #endregion

    }
}
