using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using DF.DataAccess;
using DF.DBMapping.Models;
using WebManageFridgeMQTT.Models;
using WebManageFridgeMQTT.Utility;

namespace WebManageFridgeMQTT.Controllers
{
    public class MonitoringController : Controller
    {
        DateTime DeviceDefaultFromDate = DateTime.Today.AddMonths(-1);
        DateTime DeviceDefaultToDate = DateTime.Today;

        #region Index
        public ActionResult Index()
        {
            DeviceInfoMV model = new DeviceInfoMV();
            try
            {
                model.TreeDevice = Global.Context.GetTreeThietBi("").ToList();
                model.ListDeviceInfo = Global.Context.Sp_GetInfoDevice("").ToList();
                model.ListCongTrinh = Global.Context.GetInfoCongTrinh("").ToList();
                model.ListDeviceInfoAll = Global.Context.Sp_GetInfoDeviceByIdAll().ToList();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            SessionHelper.SetSession<List<GetTreeThietBiResult>>("ListTreeDevice", model.TreeDevice);
            return View(model);
        } 
        #endregion

        #region ThietBi
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
                var ChildModel = model.Where(x => x.Cap == 1 && x.Name.Contains(strSearch.Trim())).ToList();
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

        public ActionResult PrepareInfoDeviceById(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                Session["ListDataModify"] = Global.Context.GetInfoDeviceModify(id, DeviceDefaultFromDate, DeviceDefaultToDate).ToList();
                Session["ListData"] = Global.Context.GetInfoDeviceActivity(id, DeviceDefaultFromDate, DeviceDefaultToDate).Skip(0).Take(10).ToList();
                Session["ListDataMove"] = Global.Context.GetInfoDeviceMove(id, DeviceDefaultFromDate, DeviceDefaultToDate).ToList();
                Session["ListDataReport"] = Global.Context.GetInfoDeviceReport(id, DeviceDefaultFromDate, DeviceDefaultToDate).ToList();
            }
            return Json(null);
        }

        public ActionResult Device()
        {
            List<Sp_GetInfoDeviceResult> data = Global.Context.Sp_GetInfoDevice("").ToList();
            return View(data);
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
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
            }

            //(List<GetInfoDeviceModifyResult>) Session["ListDataModify"];//
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

                //(List<GetInfoDeviceActivityResult>) Session["ListData"]; //
                model.ListData =  Global.Context.GetInfoDeviceActivity(thietBiID, FromDate, ToDate).Skip(0).Take(10).ToList();
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
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
            }

            //(List<GetInfoDeviceActivityResult>)Session["ListData"];  //
            model.ListData = Global.Context.GetInfoDeviceActivity(model.ThietBiID, model.FromDate, model.ToDate).ToList();
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
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }

            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
            }

            // (List<GetInfoDeviceMoveResult>)Session["ListDataMove"];//
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
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }

            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
            }

            //(List<GetInfoDeviceReportResult>) Session["ListDataReport"];//
            model.ListDataReport = Global.Context.GetInfoDeviceReport(thietBiID, model.FromDate, model.ToDate).ToList();
            return PartialView("DeviceReport", model);
        } 
        #endregion

        #region Menu Left
        public ActionResult GetDeviceByCongTrinh(string congTrinhId)
        {
            List<GetTreeThietBiResult> model = new List<GetTreeThietBiResult>();
            model = Global.Context.GetTreeThietBi(congTrinhId).ToList();
            ViewData["ParentID"] = 1;
            ViewData["DisplayHeader"] = true;
            return PartialView("TreeViewDevice", model);
        }
        [HttpPost]
        public JsonResult SearchDeviceBy(string inputDevice)
        {
            List<GetTreeThietBiResult> data = SessionHelper.GetSession<List<GetTreeThietBiResult>>("ListTreeDevice");
            var result = data.Where(x => x.Cap == 1).Select(s => new { id = s.Id, label = s.Name, value = s.Name }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        } 
        #endregion

        #region CongTrinh
        protected IUnitOfWork unitofwork;

        #region TaiChinhCT
        public ActionResult TaiChinhCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            string totalMoney = "0";

            #region DefaultDate
            DateTime date = DateTime.Now;
            DateTime datett = new DateTime(date.Year, date.Month, 1);
            DateTime date5nam = date.AddMonths(-60);
            DateTime tungay = new DateTime(date5nam.Year, date5nam.Month, 1);
            DateTime denngay = datett.AddMonths(1).AddTicks(-1);
            #endregion

            model.FromDate = tungay;// DateTime.Now.AddYears(-5);
            model.ToDate = denngay;//DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;

                BaoCaoThuChiModel bc = QuanLyThuChi(model.CongTrinhId, model.FromDate, model.ToDate);
                model.ListTaiChinh = ViewQuanLyThuChi(bc);

                //model.ListTaiChinh = Global.Context.CongTrinhGetInfoQuanLyThuChi(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
                //if (model.ListTaiChinh != null)
                //{
                //    double totalThu = model.ListTaiChinh.Where(x => x.ThuChiType.HasValue && x.ThuChiType.Value).Sum(s => s.Tien);
                //    double totalChi = model.ListTaiChinh.Where(x => x.ThuChiType.HasValue && !x.ThuChiType.Value).Sum(s => s.Tien);
                //    double totalLoiNhuan = (totalThu > totalChi) ? (totalThu - totalChi) : 0;
                //    totalMoney = String.Format("{0:n0}", totalLoiNhuan);
                //}

                //ViewData["TotalMoney"] = totalMoney;
            }

            return PartialView("TaiChinhCT", model);
        }

        private BaoCaoThuChiModel QuanLyThuChi(string congTrinhId, DateTime _tungay, DateTime _denngay)
        {
            unitofwork = new UnitOfWork();
            BaoCaoThuChiModel model = new BaoCaoThuChiModel();
            List<SelectListItem> lst = new List<SelectListItem>();
            Guid dfctid = Guid.Parse(congTrinhId);//Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
            var lstCongTrinh = unitofwork.CongTrinhs.GetAll().Where(c => c.Dept != 1 || c.CongTrinhId == dfctid).ToList();
            Guid defaultCTId = new Guid();
            int tmp = 1;
            foreach (var ct in lstCongTrinh)
            {
                bool selected = false;
                if (tmp == 1)
                {
                    selected = true;
                    defaultCTId = ct.CongTrinhId;
                }
                tmp += 1;
                lst.Add(new SelectListItem()
                {
                    Text = ct.TenCongTrinh,
                    Value = ct.CongTrinhId.ToString(),
                    Selected = selected
                });
            }
            DateTime date = DateTime.Now;
            //DateTime datett = new DateTime(date.Year, date.Month, 1);
            //DateTime date5nam = date.AddMonths(-60);
            DateTime tungay = _tungay;//new DateTime(date5nam.Year, date5nam.Month, 1);
            DateTime denngay = _denngay;// datett.AddMonths(1).AddTicks(-1);
            DataTable kq = unitofwork.BaoCaoChiPhis.getBaoCaoDoanhThu(tungay, denngay);

            #region MAPCODE where theo cong trinh
            List<DataRow> rowsToDelete = new List<DataRow>();
            foreach (DataRow row in kq.Rows)
            {
                if (row["CongTrinhId"].ToString().ToUpper() != congTrinhId.ToUpper())
                {
                    rowsToDelete.Add(row);
                }
            }
            foreach (DataRow row in rowsToDelete)
            {
                kq.Rows.Remove(row);
            }
            kq.AcceptChanges();
            #endregion

            kq.Columns.Add("CPTB", typeof(Double));
            foreach (DataColumn col in kq.Columns)
                col.ReadOnly = false;
            DateTime bgindate = Convert.ToDateTime("Jan 01, 1900");

            //thiet bi
            double tngay = (tungay - bgindate).TotalDays;
            DateTime denngaytemp = denngay;
            if (denngay > date)
            {
                //string ddd = date.Month + " " + date.Day + ", " + date.Year;
                denngay = Convert.ToDateTime(date.ToShortDateString());//.AddDays(1);
            }
            double dngay = (denngay - bgindate).TotalDays;
            foreach (DataRow r in kq.Rows)
            {
                if (r["CongTrinhId"].ToString().ToUpper() != "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                {
                    double chiphitb = 0;
                    Guid ctid = Guid.Parse(r[0].ToString());
                    DataTable tbden = unitofwork.TrangThaiThietBis.getTBChuyenDen(ctid);
                    DataTable tbdi = unitofwork.TrangThaiThietBis.getTBChuyenDi(ctid);

                    // DataTable temp = tbden.Clone();
                    foreach (DataRow r1 in tbden.Rows)
                    {
                        string thietbidenid = r1[0].ToString();
                        double tgdi = 0;
                        double giamuathue = 0;
                        double tgden = Convert.ToDouble(r1[4].ToString());
                        if (tgden <= dngay)
                        {
                            foreach (DataRow r2 in tbdi.Rows)
                            {
                                if (r2[0].ToString() == thietbidenid)
                                {
                                    tgdi = Convert.ToDouble(r2[4].ToString());
                                    giamuathue = Convert.ToDouble(r2[2].ToString());
                                    tbdi.Rows.Remove(r2);
                                    break;
                                }
                            }
                            if (tgdi == 0 && r1[7].ToString() == "1" && r1[6].ToString() != null && r1[6].ToString() != "")
                            {
                                tgdi = Convert.ToDouble(r1[6].ToString());
                                giamuathue = Convert.ToDouble(r1[2].ToString());
                            }
                            if (tgdi != 0) // nếu đã đi
                            {
                                if (tgdi < tngay) // nếu đã đi trước thời gian tìm kiếm => không tính
                                {

                                }
                                else
                                {
                                    if (tgden < tngay) // nếu đến trước ngày bắt đầu tìm kiêm
                                    {
                                        tgden = tngay;
                                    }
                                    if (tgdi > dngay) // nếu đi sau ngay kết thúc tìm kiếm
                                    {
                                        tgdi = dngay;
                                    }
                                    chiphitb = chiphitb + (tgdi - tgden) * (giamuathue / 30);
                                }
                            }
                            else
                            { //nếu chưa đi
                                if (tgden < tngay) // nếu đến trước ngày bắt đầu tìm kiêm
                                {
                                    tgden = tngay;
                                }
                                if (r1[6].ToString() != "" && r1[6].ToString() != null)
                                {
                                    double trangay = Convert.ToDouble(r1[6].ToString());
                                    if (trangay < dngay)
                                    {
                                        if (trangay < tngay)
                                        {
                                            dngay = tngay;
                                        }
                                        else
                                        {
                                            dngay = trangay;
                                        }
                                    }
                                }
                                chiphitb = chiphitb + ((dngay - tgden) * (Convert.ToDouble(r1[2].ToString()) / 30));
                            }
                        }

                    }
                    double tongchiphitbtb = chiphitb;
                    DataTable tongcpsuachua = unitofwork.BCThietBis.getSuaChuaBCThietBi(ctid, tungay, denngay);
                    if (tongcpsuachua.Rows[0][2].ToString() != "" && tongcpsuachua.Rows[0][2].ToString() != null)
                    {
                        tongchiphitbtb += Convert.ToDouble(tongcpsuachua.Rows[0][2].ToString());
                    }
                    r["CPTB"] = tongchiphitbtb;
                }
                else
                {
                    r["thuvt"] = "0";
                }
            }

            //Đoạn này MAP ko xài
            //DataTable tongvc = unitofwork.BCVanChuyens.getTongVanChuyen(tungay, denngay);
            //model.ThoiGian = (tungay.ToString("dd/MM/yyyy") + " - " + denngaytemp.ToString("dd/MM/yyyy"));
            model.TKDoanhThu = kq;
            //model.listCongTrinh = lst;
            //if (tongvc.Rows[0][0].ToString() != "")
            //{
            //    model.TongVanChuyen = Convert.ToDouble(tongvc.Rows[0][0].ToString());
            //}

            return model;
        }

        private List<CongTrinhGetInfoQuanLyThuChiResult> ViewQuanLyThuChi(BaoCaoThuChiModel model)
        {
            List<CongTrinhGetInfoQuanLyThuChiResult> listItem = new List<CongTrinhGetInfoQuanLyThuChiResult>();

            double tong = 0;
            List<string> labelchart = new List<string>();
            List<double> datachart = new List<double>();
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            int countCols = model.TKDoanhThu.Columns.Count;
            double tttchi = 0;
            double tttthu = 0;
            double thuhd = 0;
            double tongthuhd = 0;
            double thuhd2 = 0;
            double tongthuhd2 = 0;
            double tongthukhac = 0;
            double thukhac = 0;
            double tttdoanhthu = 0;
            double tongdonhthuchia11 = 0;
            double tongchiVattu = 0;
            double tongchithietbi = 0;
            double tongchiccdc = 0;
            double tongchinhancong = 0;
            double tongchiluongvp = 0;
            double tongchivanchuyen = 0;

            for (int r = 0; r < model.TKDoanhThu.Rows.Count; r++)
            {
                string tencongtrinh = model.TKDoanhThu.Rows[r][1].ToString().ToUpper();
                tencongtrinh = tencongtrinh.Contains("CÔNG TRÌNH ") ? tencongtrinh.Replace("CÔNG TRÌNH ", "") : tencongtrinh;
                labelchart.Add(tencongtrinh);
                double dtchart = 0;
                string chiphipop = "";
                double tongchi = 0;
                if (model.TKDoanhThu.Rows[r][2].ToString() != "")
                {
                    tongchi = double.Parse(model.TKDoanhThu.Rows[r][2].ToString());
                    tttchi += tongchi;
                    chiphipop = tongchi.ToString("#,###", cul.NumberFormat).Replace(".", ",");

                }
                double chiVattu = 0;
                string chivattustring = "";
                if (model.TKDoanhThu.Rows[r][5].ToString() != "")
                {
                    chiVattu = double.Parse(model.TKDoanhThu.Rows[r][5].ToString());
                    tongchiVattu += chiVattu;
                    chivattustring = chiVattu.ToString("#,###", cul.NumberFormat).Replace(".", ",");

                }

                double chiccdc = 0;
                string chiccdctring = "";
                if (model.TKDoanhThu.Rows[r][6].ToString() != "")
                {
                    chiccdc = double.Parse(model.TKDoanhThu.Rows[r][6].ToString());
                    tongchiccdc += chiccdc;
                    chiccdctring = chiccdc.ToString("#,###", cul.NumberFormat).Replace(".", ",");

                }

                double chinhancong = 0;
                string chinhancongstring = "";
                if (model.TKDoanhThu.Rows[r][7].ToString() != "")
                {
                    chinhancong = double.Parse(model.TKDoanhThu.Rows[r][7].ToString());
                    tongchinhancong += chinhancong;
                    chinhancongstring = chinhancong.ToString("#,###", cul.NumberFormat).Replace(".", ",");

                }

                double chivanchuyen = 0;
                string chivanchuyenstring = "";
                if (model.TKDoanhThu.Rows[r][8].ToString() != "")
                {
                    chivanchuyen = double.Parse(model.TKDoanhThu.Rows[r][8].ToString());
                    tongchivanchuyen += chivanchuyen;
                    chivanchuyenstring = chivanchuyen.ToString("#,###", cul.NumberFormat).Replace(".", ",");
                }

                double chithietbi = 0;
                string chithietbistring = "";
                if (model.TKDoanhThu.Rows[r][10].ToString() != "")
                {
                    chithietbi = double.Parse(model.TKDoanhThu.Rows[r][10].ToString());
                    tongchithietbi += chithietbi;
                    chithietbistring = chithietbi.ToString("#,###", cul.NumberFormat).Replace(".", ",");

                }

                string tongthu = "";
                thuhd = 0;
                thukhac = 0;
                if (model.TKDoanhThu.Rows[r][3].ToString() != "")
                {
                    thuhd = double.Parse(model.TKDoanhThu.Rows[r][3].ToString());
                }
                thuhd2 = 0;
                if (model.TKDoanhThu.Rows[r][9].ToString() != "")
                {
                    thuhd2 = double.Parse(model.TKDoanhThu.Rows[r][9].ToString());
                }

                if (model.TKDoanhThu.Rows[r][0].ToString() == "af3cc397-da63-4154-8b70-8277b3fdc345")
                {
                    thukhac = model.TongVanChuyen;
                }
                else if (model.TKDoanhThu.Rows[r][4].ToString() != "")
                {
                    thukhac = double.Parse(model.TKDoanhThu.Rows[r][4].ToString());
                }
                tongthu = (thuhd + thukhac).ToString("#,###", cul.NumberFormat).Replace(".", ",");
                tttthu += (thuhd + thukhac);
                tongthuhd += thuhd;
                tongthuhd2 += thuhd2;
                tongthukhac += thukhac;
                string luongvp = (thuhd * 0.07).ToString("#,###", cul.NumberFormat).Replace(".", ",");
                tongchiluongvp += (thuhd * 0.07);
                string tttthuhd = thuhd.ToString("#,###", cul.NumberFormat).Replace(".", ",");
                string tttdoanhthuchia11 = (thuhd / 1.1).ToString("#,###", cul.NumberFormat).Replace(".", ",");
                string tttthuhd2 = thuhd2.ToString("#,###", cul.NumberFormat).Replace(".", ",");
                string tttthukhac = thukhac.ToString("#,###", cul.NumberFormat).Replace(".", ",");
                double tongdoanhthu = thukhac + (thuhd / 1.1) - tongchi - chiVattu - chiccdc - chinhancong - chivanchuyen - chithietbi - (thuhd * 0.07);
                tttdoanhthu += tongdoanhthu;
                tongdonhthuchia11 += (thuhd / 1.1);
                string tdaonhthu = "0";
                if (tongdoanhthu > 0)
                {
                    tdaonhthu = tongdoanhthu.ToString("#,###", cul.NumberFormat).Replace(".", ",");
                }
                dtchart = tongdoanhthu;
                datachart.Add(dtchart);
                int stt = r + 1;

                #region MAPCODE BANANA
                #region THU
                CongTrinhGetInfoQuanLyThuChiResult thusanluong = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Sản lượng",
                    ThuChiType = true,
                    ThuChiValue = tttthuhd2
                };
                CongTrinhGetInfoQuanLyThuChiResult thudoanhthu = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Doanh thu",
                    ThuChiType = true,
                    ThuChiValue = tttdoanhthuchia11
                };
                CongTrinhGetInfoQuanLyThuChiResult thuKhac = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Khác",
                    ThuChiType = true,
                    ThuChiValue = tttthukhac
                };
                #endregion

                #region CHI
                CongTrinhGetInfoQuanLyThuChiResult chiThietBi = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Thiết bị",
                    ThuChiType = false,
                    ThuChiValue = chithietbistring
                };
                CongTrinhGetInfoQuanLyThuChiResult chiCCDC = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "CCDC",
                    ThuChiType = false,
                    ThuChiValue = chiccdctring
                };
                CongTrinhGetInfoQuanLyThuChiResult chiVatTu = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Vật tư",
                    ThuChiType = false,
                    ThuChiValue = chivattustring
                };
                CongTrinhGetInfoQuanLyThuChiResult chiNhanCong = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Nhân công",
                    ThuChiType = false,
                    ThuChiValue = chinhancongstring
                };
                CongTrinhGetInfoQuanLyThuChiResult chiLuongVP = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Lương VP",
                    ThuChiType = false,
                    ThuChiValue = luongvp
                };
                CongTrinhGetInfoQuanLyThuChiResult chiVanChuyen = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Vận chuyển",
                    ThuChiType = false,
                    ThuChiValue = chivanchuyenstring
                };
                CongTrinhGetInfoQuanLyThuChiResult chiKhac = new CongTrinhGetInfoQuanLyThuChiResult()
                {
                    CongTrinhId = model.TKDoanhThu.Rows[r][0].ToString().ToUpper(),
                    ThuChiName = "Khác",
                    ThuChiType = false,
                    ThuChiValue = chiphipop
                };

                #endregion

                ViewData["TotalMoney"] = tdaonhthu;

                listItem.Add(thusanluong);
                listItem.Add(thudoanhthu);
                listItem.Add(thuKhac);

                listItem.Add(chiThietBi);
                listItem.Add(chiCCDC);
                listItem.Add(chiVatTu);
                listItem.Add(chiNhanCong);
                listItem.Add(chiLuongVP);
                listItem.Add(chiVanChuyen);
                listItem.Add(chiKhac);
                #endregion
            }
            return listItem;
        }

        public class BaoCaoThuChiModel
        {
            public List<SelectListItem> listCongTrinh { get; set; }
            public DataTable TKDoanhThu { get; set; }
            public string ThoiGian { get; set; }
            public double TongThu { get; set; }
            public double ThuHopDong { get; set; }
            public double ThuKhac { get; set; }
            public double TongChi { get; set; }
            public double DoanhThu { get; set; }
            public double TongVanChuyen { get; set; }

        }
        #endregion

        public ActionResult SanLuongCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            string totalMoney = "0";
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;

                unitofwork = new UnitOfWork();
                Guid ctid = Guid.Parse(congTrinhId);
                model.ListSanLuong = unitofwork.KhoiLuongThiCongs.GetAll().Where(cp => cp.IsActive == true && cp.CongTrinhID == ctid).ToList();
                //model.ListSanLuong = Global.Context.CongTrinhGetInfoBCSanLuong(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
                //if (model.ListSanLuong != null)
                //{
                //    double total = model.ListSanLuong.Where(x => x.Tien.HasValue).Sum(s => s.Tien.Value);
                //    totalMoney = String.Format("{0:n0}", total);
                //    ViewData["TotalMoney"] = totalMoney;
                //}
            }

            return PartialView("SanLuongCT", model);
        }
        #region BCThietbi
        public ActionResult CPDeviceCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            string totalMoney = "0";
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;

                BaoCaoThietBiModel bc = BaoCaoThietBi(model.CongTrinhId, model.FromDate, model.ToDate);
                model.ListThietBi = ViewBaoCaoThietBi(bc);
                //model.ListThietBi = Global.Context.CongTrinhGetInfoBCThietbi(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
                if (model.ListThietBi != null)
                {
                    decimal total = model.ListThietBi.Sum(s => s.Tien.Value);
                    totalMoney = String.Format("{0:n0}", total);
                    ViewData["TotalMoney"] = totalMoney;
                }
            }

            return PartialView("CPDeviceCT", model);
        }

        private BaoCaoThietBiModel BaoCaoThietBi(string CongTrinhID, DateTime _tungay, DateTime _denngay)
        {
            unitofwork = new UnitOfWork();
            BaoCaoThietBiModel kq = new BaoCaoThietBiModel();
            Guid ctid = Guid.Parse(CongTrinhID);
            DateTime tungay = _tungay;// Helper.DateTimeParseExact(TGArray[0].Trim().ToString(), "dd/MM/yyyy", null);
            DateTime denngay = _denngay;//Helper.DateTimeParseExact(TGArray[1].Trim().ToString(), "dd/MM/yyyy", null).AddDays(1).AddTicks(-1);
            DateTime date = DateTime.Now;
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("ThietBiId");
            dt.Columns.Add("TenThietBi");
            dt.Columns.Add("DonViThue");
            dt.Columns.Add("GiaBan");
            dt.Columns.Add("ThoiGian");
            dt.Columns.Add("ThoiGianDi");
            dt.Columns.Add("NgayOCT");
            dt.Columns.Add("TNgayOCT");
            dt.Columns.Add("ThanhTien");
            dt.Columns.Add("TongThanhTien");
            DateTime bgindate = Convert.ToDateTime("Jan 01, 1900");

            //thiet bi
            double tngay = (tungay - bgindate).TotalDays;
            DateTime denngaytemp = denngay;
            if (denngay > date)
            {
                string ddd = date.Month + " " + date.Day + ", " + date.Year;
                denngay = Convert.ToDateTime(ddd);//.AddDays(1);
            }
            double dngay = (denngay - bgindate).TotalDays;

            //Thời gian 5 năm
            DateTime datenow5nam = DateTime.Now;
            DateTime datett5nam = new DateTime(datenow5nam.Year, datenow5nam.Month, 1);
            DateTime date5nam = datenow5nam.AddMonths(-120);
            DateTime tungay5nam = new DateTime(date5nam.Year, date5nam.Month, 1);
            DateTime denngay5nam = datett5nam.AddMonths(1).AddTicks(-1);

            double tngay555 = (tungay5nam - bgindate).TotalDays;
            DateTime denngay5namtemp = denngay5nam;
            if (denngay5nam > date)
            {
                string ddd = date.Month + " " + date.Day + ", " + date.Year;
                denngay5nam = Convert.ToDateTime(ddd);//.AddDays(1);
            }
            double denngay555 = (denngay5nam - bgindate).TotalDays;

            DataTable tbden = unitofwork.TrangThaiThietBis.getTBChuyenDen(ctid);
            DataTable tbdi = unitofwork.TrangThaiThietBis.getTBChuyenDi(ctid);

            // DataTable temp = tbden.Clone();
            foreach (DataRow r1 in tbden.Rows)
            {
                double chiphitb = 0;
                double tongsongayoct = 0;
                double tongchiphi = 0;
                string thietbidenid = r1[0].ToString();
                double tgdi = 0;
                double giamuathue = 0;
                double tgden = Convert.ToDouble(r1[4].ToString());
                DataRow _ravi = dt.NewRow();
                _ravi["ThietBiId"] = r1[0].ToString();
                _ravi["TenThietBi"] = r1[1].ToString();
                _ravi["DonViThue"] = r1[9].ToString();
                _ravi["GiaBan"] = r1[2].ToString();
                _ravi["ThoiGian"] = r1[3].ToString();
                double snoct = 0;
                //if (tgden <= dngay)
                //{
                foreach (DataRow r2 in tbdi.Rows)
                {
                    if (r2[0].ToString() == thietbidenid)
                    {
                        tgdi = Convert.ToDouble(r2[4].ToString());
                        giamuathue = Convert.ToDouble(r2[2].ToString());
                        _ravi["ThoiGianDi"] = r2[3].ToString();
                        tbdi.Rows.Remove(r2);
                        break;
                    }
                }
                if (tgdi == 0 && r1[7].ToString() == "1" && r1[6].ToString() != null && r1[6].ToString() != "") //thiết bị thuê và đã trả
                {
                    tgdi = Convert.ToDouble(r1[6].ToString());
                    giamuathue = Convert.ToDouble(r1[2].ToString());
                    _ravi["ThoiGianDi"] = r1[8].ToString();
                }
                //else //tìm ngày điều chuyển sang ctrinh khác
                //{
                //    foreach (DataRow r2 in tbdi.Rows)
                //    {
                //        if (r2[0].ToString() == thietbidenid)
                //        {
                //            tgdi = Convert.ToDouble(r2[4].ToString());
                //            giamuathue = Convert.ToDouble(r2[2].ToString());
                //            _ravi["ThoiGianDi"] = r2[3].ToString();
                //            tbdi.Rows.Remove(r2);
                //            break;
                //        }
                //    }
                //}
                if (tgdi != 0) // nếu đã đi
                {
                    tongsongayoct = tgdi - tgden;
                    tongchiphi = tongsongayoct * (giamuathue / 30);
                    if (tgdi < tngay) // nếu đã đi trước thời gian tìm kiếm => không tính
                    {

                    }
                    else
                    {
                        if (tgden < tngay) // nếu đến trước ngày bắt đầu tìm kiêm
                        {
                            tgden = tngay;
                        }
                        if (tgdi > dngay) // nếu đi sau ngay kết thúc tìm kiếm
                        {
                            tgdi = dngay;
                        }
                        chiphitb = chiphitb + (tgdi - tgden) * (giamuathue / 30);
                        snoct = tgdi - tgden;
                    }
                }
                else
                { //nếu chưa đi
                    tongsongayoct = denngay555 - tgden;
                    tongchiphi = tongsongayoct * (Convert.ToDouble(r1[2].ToString()) / 30);
                    if (tgden < tngay) // nếu đến trước ngày bắt đầu tìm kiêm
                    {
                        tgden = tngay;
                    }
                    if (r1[6].ToString() != "" && r1[6].ToString() != null)
                    {
                        double trangay = Convert.ToDouble(r1[6].ToString());
                        if (trangay < dngay)
                        {
                            if (trangay < tngay)
                            {
                                dngay = tngay;
                            }
                            else
                            {
                                dngay = trangay;
                            }
                        }
                    }
                    chiphitb = chiphitb + ((dngay - tgden) * (Convert.ToDouble(r1[2].ToString()) / 30));
                    snoct = dngay - tgden;
                }
                _ravi["NgayOCT"] = snoct;
                _ravi["TNgayOCT"] = tongsongayoct;
                _ravi["ThanhTien"] = chiphitb;
                _ravi["TongThanhTien"] = tongchiphi;
                dt.Rows.Add(_ravi);
            }
            DataTable lstsuachua = unitofwork.BCThietBis.getListSuaChuaBCThietBi(ctid, tungay, denngay);
            kq.TKSuaChuaThietBi = lstsuachua;
            kq.TKBCBaoCaoThietBi = dt;
            return kq;
        }

        private List<CongTrinhGetInfoBCThietbiResult> ViewBaoCaoThietBi(BaoCaoThietBiModel model)
        {
            System.Globalization.CultureInfo cul = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            List<CongTrinhGetInfoBCThietbiResult> listItem = new List<CongTrinhGetInfoBCThietbiResult>();

            for (int r = 0; r < model.TKBCBaoCaoThietBi.Rows.Count; r++)
            {
                int stt = r + 1;
                string dongia = "";
                if (model.TKBCBaoCaoThietBi.Rows[r][3].ToString() != "")
                {
                    double temp1 = double.Parse(model.TKBCBaoCaoThietBi.Rows[r][3].ToString());
                    dongia = temp1.ToString("#,###", cul.NumberFormat).Replace(".", ",");
                }
                string thanhtien = "";
                if (model.TKBCBaoCaoThietBi.Rows[r][8].ToString() != "")
                {
                    double temp1 = double.Parse(model.TKBCBaoCaoThietBi.Rows[r][8].ToString());
                    thanhtien = temp1.ToString("#,###", cul.NumberFormat).Replace(".", ",");
                }
                string ttthanhtien = "";
                if (model.TKBCBaoCaoThietBi.Rows[r][9].ToString() != "")
                {
                    double temp1 = double.Parse(model.TKBCBaoCaoThietBi.Rows[r][9].ToString());
                    ttthanhtien = temp1.ToString("#,###", cul.NumberFormat).Replace(".", ",");
                }

                CongTrinhGetInfoBCThietbiResult item = new CongTrinhGetInfoBCThietbiResult() {
                    TenThietBi = model.TKBCBaoCaoThietBi.Rows[r][1].ToString(),
                    DonViThue = model.TKBCBaoCaoThietBi.Rows[r][2].ToString(),
                    NgayDen = model.TKBCBaoCaoThietBi.Rows[r][4].ToString(),
                    NgayDi = model.TKBCBaoCaoThietBi.Rows[r][5].ToString(),
                    DonGia = dongia,
                    SoNgay = model.TKBCBaoCaoThietBi.Rows[r][6].ToString(),
                    SoTien = thanhtien,
                    TongNgayOCongTrinh = model.TKBCBaoCaoThietBi.Rows[r][7].ToString(),
                    TongSoTien = ttthanhtien,
                    Tien = decimal.Parse(model.TKBCBaoCaoThietBi.Rows[r][9].ToString())
            };
                listItem.Add(item);
            }

            return listItem;
        }

        public class BaoCaoThietBiModel
        {
            public List<SelectListItem> listCongTrinh { get; set; }
            public DataTable TKBCBaoCaoThietBi { get; set; }
            public DataTable TKSuaChuaThietBi { get; set; }
            public string ThoiGian { get; set; }
            public string TenCongTrinh { get; set; }
        }
        #endregion
        public ActionResult CPVatTuCT(string congTrinhId, string strFromDate, string strToDate)
        {
            CongTringPopupMV model = new CongTringPopupMV();
            string totalMoney = "0";
            model.FromDate = DateTime.Now.AddYears(-5);
            model.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
            }

            if (!string.IsNullOrEmpty(congTrinhId))
            {
                model.CongTrinhId = congTrinhId;
                model.ListVatTu = Global.Context.CongTrinhGetInfoBCVatTu(model.CongTrinhId, model.FromDate, model.ToDate).ToList();
                if (model.ListVatTu != null)
                {
                    double total = model.ListVatTu.Where(x => x.Tien.HasValue).Sum(s => s.Tien.Value);
                    totalMoney = String.Format("{0:n0}", total);
                    ViewData["TotalMoney"] = totalMoney;
                }
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
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
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
                model.ThiCongCoc = Global.Context.CongTrinhGetInfoBCQuyTrinhThiCongCoc(model.CongTrinhId, cocId, null, null).FirstOrDefault();
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
                model.FromDate = Helper.DateTimeParse(strFromDate);
            }
            if (!string.IsNullOrEmpty(strFromDate))
            {
                model.ToDate = Helper.DateTimeParse(strToDate);
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
