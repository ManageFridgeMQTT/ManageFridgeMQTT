using DMSERoute.Helpers;
using eRoute.Models;
using eRoute.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eRoute.Controllers
{
    [LogAndRedirectOnError]
    public class ReportController : Controller
    {
        public string UserLogin { get {
                string username = SessionHelper.GetSession<string>("UserName");
                if (!string.IsNullOrEmpty(username) && username.ToUpper() == "ADMIN")
                {
                    return String.IsNullOrEmpty(ControllerHelper.valueCustomSetting("ReportMapUserAcu")) ? "" : ControllerHelper.valueCustomSetting("ReportMapUserAcu");
                }
                else
                {
                    return username;
                }
            } }

        #region Funtion Summary
        public ActionResult ReloadOptionHierarchyValue(string strAttribute)
        {
            //CbxHierarchyValue
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = null;
            ctrCombobox.TitleKey = Utility.Phrase("HierarchyValueID");
            ctrCombobox.TitleName = Utility.Phrase("HierarchyValueName");
            int attribute = 0;
            if (!string.IsNullOrEmpty(strAttribute))
            {
                attribute = int.Parse(strAttribute);
            }
            ctrCombobox.listOption = Global.Context.DMSAttributes.Where(x => x.Type == "IN" && x.AttributeNbr == attribute).Select(
                                            s => new OptionCombobox { ID = s.AttributeID.ToString(), Key = s.AttributeCD.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            ViewData["NameID"] = "HierarchyValue";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrCombobox);
        }
        public ActionResult ReloadOptionInventoryItem(string strAttribute, string HierarchyValueID)
        {
            int HierarchyValue = 0;
            if (!string.IsNullOrEmpty(HierarchyValueID))
            {
                HierarchyValue = int.Parse(HierarchyValueID);
            }
            //CbxHierarchyValue
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = null;
            ctrCombobox.TitleKey = Utility.Phrase("InventoryItemID");
            ctrCombobox.TitleName = Utility.Phrase("InventoryItemName");
            ctrCombobox.listOption = Global.Context.pp_GetInventoryByHierarchy(HierarchyValue, strAttribute).Select(
                                            s => new OptionCombobox { ID = s.InventoryID.ToString(), Key = s.InventoryCD, Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }

            ViewData["NameID"] = "InventoryItem";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrCombobox);
        }
        public JsonResult ReloadTreeView(int channelID = 0)
        {
            List<ViewControlTreeView> result = GetTreeViewList(channelID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReloadOptionKPIPeriodConfig(string RefNbr = "")
        {
            List<OptionKPIPeriodConfig> kpiPeriodConf = Global.Context.pp_GetKPIPeriodConfig(RefNbr).Select(t => new OptionKPIPeriodConfig
            {
                CodeKPI = t.CodeKPI,
                CodeListSalesID = t.CodeListSalesID,
                Description = t.Description,
                FromDate = (t.FromDate.HasValue ? t.FromDate.Value : DateTime.Today),
                NameKPI = t.NameKPI,
                RefNbr = t.RefNbr,
                ShortDescription = t.ShortDescription,
                ToDate = (t.ToDate.HasValue ? t.ToDate.Value : DateTime.Today)
            }).ToList();
            return PartialView("KPIPeriodConfigPartial", kpiPeriodConf);
        }
        public ActionResult ReloadOptionWeek(int year = 0)
        {
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = null;
            ctrCombobox.TitleKey = Utility.Phrase("WeekID");
            ctrCombobox.TitleName = Utility.Phrase("WeekName");
            ctrCombobox.listOption = Global.Context.DMSWeeks.Where(x => x.StartDate.HasValue && x.EndDate.HasValue && x.Year == year.ToString()).Select(
                                            s => new OptionCombobox { ID = s.Week, Key = s.Week, Value = s.StartDate.Value.Date.ToString() + " - " + s.EndDate.Value.Date.ToString() }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            ViewData["NameID"] = "Week";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrCombobox);
        }

        public ActionResult ReloadOptionProvince(string countryID)
        {
            //CbxHierarchyValue
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = null;
            ctrCombobox.TitleKey = Utility.Phrase("ProvinceID");
            ctrCombobox.TitleName = Utility.Phrase("ProvinceName");
            ctrCombobox.listOption = Global.Context.DMSProvinceAlls.Where(x=>x.CountryID == countryID).Select(
                                            s => new OptionCombobox { ID = s.ProvinceID.ToString(), Key = s.ProvinceID.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            ViewData["NameID"] = "Province";
            return PartialView("~/Views/Shared/Control/ComboboxPartial.cshtml", ctrCombobox);
        }

        private List<ViewControlTreeView> GetTreeViewList(int channelID = 0)
        {
            var rootNode = Global.Context.pp_GetTreeviewRegion(channelID, this.UserLogin).Where(t => t.Level == 0).Select(t => new ViewControlTreeView()
            {
                id = t.ValueID.ToString(),
                ValueCD = t.ValueCD,
                text = t.Descr
            }).ToList();
            foreach (var item in rootNode)
            {
                BuildChildNode(item);
            }

            return rootNode;
        }

        private void BuildChildNode(ViewControlTreeView rootNode)
        {
            if (rootNode != null)
            {
                List<ViewControlTreeView> chidNode = Global.Context.pp_GetTreeviewRegion(0, this.UserLogin).Where(t => t.ParentID == int.Parse(rootNode.id)).Select(t => new ViewControlTreeView()
                {
                    id = t.ValueID.ToString(),
                    ValueCD = t.ValueCD,
                    text = t.Descr
                }).ToList();
                if (chidNode.Count > 0)
                {
                    foreach (var childRootNode in chidNode)
                    {
                        BuildChildNode(childRootNode);
                        rootNode.nodes.Add(childRootNode);
                    }
                }
            }
        }
        #endregion

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_Purchase", true)]
        public ActionResult Purchase()
        {
            ReportBasicMV model = new ReportBasicMV();
            model.ReportID = ReportType.ReportBLSalesIn.ToString();
            //CbxHierarchyLevel
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.HierarchyLevel;
            ctrCombobox.TitleKey = Utility.Phrase("HierarchyLevelID");
            ctrCombobox.TitleName = Utility.Phrase("HierarchyLevelName");
            ctrCombobox.listOption = Global.Context.DMSAttributeConfigs.Where(x=>
                                            x.Type == "IN" && x.HierachyLevel.HasValue).Select(
                                            s => new OptionCombobox { ID = s.AttributeNbr.ToString(), Key = s.HierachyLevel.ToString(), Value = s.Attribute }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxHierarchyLevel = ctrCombobox;

            //CbxHierarchyValue
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.HierarchyValue;
            ctrCombobox.TitleKey = Utility.Phrase("HierarchyValueID");
            ctrCombobox.TitleName = Utility.Phrase("HierarchyValueName");
            ctrCombobox.listOption = Global.Context.DMSAttributes.Where(x => x.Type == "IN").Select(
                                            s => new OptionCombobox { ID = s.AttributeID.ToString(), Key = s.AttributeCD.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxHierarchyValue = ctrCombobox;

            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.InventoryItem;
            ctrCombobox.TitleKey = Utility.Phrase("InventoryItemID");
            ctrCombobox.TitleName = Utility.Phrase("InventoryItemName");
            ctrCombobox.listOption = Global.Context.pp_GetInventoryByHierarchy(0, string.Empty).Select(
                                            s => new OptionCombobox { ID = s.InventoryID.ToString(), Key = s.InventoryCD, Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxInventoryItem = ctrCombobox;

            //CbxInventoryItem
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Template;
            ctrCombobox.TitleKey = Utility.Phrase("TemplateID");
            ctrCombobox.TitleName = Utility.Phrase("TemplateName");
            ctrCombobox.listOption = Global.Context.pp_GetReportTemplateBy(model.ReportID).Select(
                                            s => new OptionCombobox { ID = s.TemplateID.ToString(), Key = s.TemplateID.ToString(), Value = s.TemplateName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxTemplate = ctrCombobox;
            model.TreeView = GetTreeViewList();

            return View(model);
        }

        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_Sales", true)]
        public ActionResult Sales()
        {
            ReportBasicMV model = new ReportBasicMV();
            model.ReportID = ReportType.ReportBLSales.ToString();

            //CbxHierarchyLevel
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.HierarchyLevel;
            ctrCombobox.TitleKey = Utility.Phrase("HierarchyLevelID");
            ctrCombobox.TitleName = Utility.Phrase("HierarchyLevelName");
            ctrCombobox.listOption = Global.Context.DMSAttributeConfigs.Where(x =>
                                            x.Type == "IN" && x.HierachyLevel.HasValue).Select(
                                            s => new OptionCombobox { ID = s.AttributeNbr.ToString(), Key = s.HierachyLevel.ToString(), Value = s.Attribute }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxHierarchyLevel = ctrCombobox;

            //CbxHierarchyValue
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.HierarchyValue;
            ctrCombobox.TitleKey = Utility.Phrase("HierarchyValueID");
            ctrCombobox.TitleName = Utility.Phrase("HierarchyValueName");
            ctrCombobox.listOption = Global.Context.DMSAttributes.Where(x => x.Type == "IN").Select(
                                            s => new OptionCombobox { ID = s.AttributeID.ToString(), Key = s.AttributeCD.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxHierarchyValue = ctrCombobox;

            //CbxInventoryItem
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.InventoryItem;
            ctrCombobox.TitleKey = Utility.Phrase("InventoryItemID");
            ctrCombobox.TitleName = Utility.Phrase("InventoryItemName");
            ctrCombobox.listOption = Global.Context.pp_GetInventoryByHierarchy(0, string.Empty).Select(
                                            s => new OptionCombobox { ID = s.InventoryID.ToString(), Key = s.InventoryCD, Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxInventoryItem = ctrCombobox;

            //template
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Template;
            ctrCombobox.TitleKey = Utility.Phrase("TemplateID");
            ctrCombobox.TitleName = Utility.Phrase("TemplateName");
            ctrCombobox.listOption = Global.Context.pp_GetReportTemplateBy(model.ReportID).Select(
                                            s => new OptionCombobox { ID = s.TemplateID.ToString(), Key = s.TemplateID.ToString(), Value = s.TemplateName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxTemplate = ctrCombobox;

            // Province
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Province;
            ctrCombobox.TitleKey = Utility.Phrase("ProvinceID");
            ctrCombobox.TitleName = Utility.Phrase("ProvinceName");
            ctrCombobox.listOption = Global.Context.DMSProvinceAlls.Where(x => x.CountryID == model.Country).Select(
                                            s => new OptionCombobox { ID = s.ProvinceID.ToString(), Key = s.ProvinceID.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxProvince = ctrCombobox;
            model.TreeView = GetTreeViewList();
            return View(model);
        }

        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_Promotion", true)]
        public ActionResult Promotion()
        {
            ReportBasicMV model = new ReportBasicMV();
            model.ReportID = ReportType.ReportBLPromotion.ToString();
            
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            //template
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Template;
            ctrCombobox.TitleKey = Utility.Phrase("TemplateID");
            ctrCombobox.TitleName = Utility.Phrase("TemplateName");
            ctrCombobox.listOption = Global.Context.pp_GetReportTemplateBy(model.ReportID).Select(
                                            s => new OptionCombobox { ID = s.TemplateID.ToString(), Key = s.TemplateID.ToString(), Value = s.TemplateName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxTemplate = ctrCombobox;

            // Country
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Country;
            ctrCombobox.TitleKey = Utility.Phrase("CountryID");
            ctrCombobox.TitleName = Utility.Phrase("CountryName");
            ctrCombobox.listOption = Global.Context.Countries.Select(
                                            s => new OptionCombobox { ID = s.CountryID, Key = s.CountryID, Value = s.Description }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxCountry = ctrCombobox;

            // Province
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Province;
            ctrCombobox.TitleKey = Utility.Phrase("ProvinceID");
            ctrCombobox.TitleName = Utility.Phrase("ProvinceName");
            ctrCombobox.listOption = Global.Context.DMSProvinceAlls.Where(x=>x.CountryID == model.Country).Select(
                                            s => new OptionCombobox { ID = s.ProvinceID.ToString(), Key = s.ProvinceID.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxProvince = ctrCombobox;

            // Promotion
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Promotion;
            ctrCombobox.TitleKey = Utility.Phrase("PromotionID");
            ctrCombobox.TitleName = Utility.Phrase("PromotionName");
            ctrCombobox.listOption = Global.Context.DMSPromotions.Select(
                                            s => new OptionCombobox { ID = s.PromotionID.ToString(), Key = s.PromotionCD, Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxPromotion = ctrCombobox;
            model.TreeView = GetTreeViewList();
            return View(model);
        }

        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_KPISales", true)]
        public ActionResult KPISales()
        {
            ReportBasicMV model = new ReportBasicMV();
            model.ReportID = ReportType.ReportBLKPI.ToString();

            // Template
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Template;
            ctrCombobox.TitleKey = Utility.Phrase("TemplateID");
            ctrCombobox.TitleName = Utility.Phrase("TemplateName");
            ctrCombobox.listOption = Global.Context.pp_GetReportTemplateBy(model.ReportID).Select(
                                            s => new OptionCombobox { ID = s.TemplateID.ToString(), Key = s.TemplateID.ToString(), Value = s.TemplateName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxTemplate = ctrCombobox;

            //Period
            List<OptionKPIPeriod> kpiPeriod = Global.Context.pp_GetKPIPeriod().Select(t => new OptionKPIPeriod
            {
                CategoryCD = t.CategoryCD,
                CategoryName = t.CategoryName,
                Descr = t.Descr,
                KPIPeriodNbr = t.KPIPeriodNbr,
                Period = t.Period.ToString(),
                PeriodFull = t.PeriodFull,
                SalesAreaID = (t.SalesAreaID.HasValue ? t.SalesAreaID.Value : 0),
                SalesAreaName = t.SalesAreaName,
                SalesOrgID = (t.SalesOrgID.HasValue ? t.SalesOrgID.Value : 0),
                Channel = t.Channel
            }).ToList();
            model.ListKPIPeriod = kpiPeriod;

            //Period config
            List<OptionKPIPeriodConfig> kpiPeriodConf = Global.Context.pp_GetKPIPeriodConfig("").Select(t => new OptionKPIPeriodConfig
            {
                CodeKPI = t.CodeKPI,
                CodeListSalesID = t.CodeListSalesID,
                Description = t.Description,
                FromDate = (t.FromDate.HasValue ? t.FromDate.Value : DateTime.Today),
                NameKPI = t.NameKPI,
                RefNbr = t.RefNbr,
                ShortDescription = t.ShortDescription,
                ToDate = (t.ToDate.HasValue ? t.ToDate.Value : DateTime.Today)
            }).ToList();
            model.ListKPIPeriodConfig = kpiPeriodConf;

            model.TreeView = GetTreeViewList();

            return View(model);
        }

        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_Inventory", true)]
        public ActionResult Inventory()
        {
            ReportBasicMV model = new ReportBasicMV();
            model.ReportID = ReportType.ReportInventoryRealtime.ToString();

            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Template;
            ctrCombobox.TitleKey = Utility.Phrase("TemplateID");
            ctrCombobox.TitleName = Utility.Phrase("TemplateName");
            ctrCombobox.listOption = Global.Context.pp_GetReportTemplateBy(model.ReportID).Select(
                                            s => new OptionCombobox { ID = s.TemplateID.ToString(), Key = s.TemplateID.ToString(), Value = s.TemplateName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxTemplate = ctrCombobox;

            return View(model);
        }

        [HttpGet]
        [Authorize]
        [ActionAuthorize("Report_InventoryResult", true)]
        public ActionResult InventoryResult()
        {
            ReportBasicMV model = new ReportBasicMV();
            model.ReportID = ReportType.ReportBLINInventoryStock.ToString();
            
            //CbxHierarchyLevel
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.HierarchyLevel;
            ctrCombobox.TitleKey = Utility.Phrase("HierarchyLevelID");
            ctrCombobox.TitleName = Utility.Phrase("HierarchyLevelName");
            ctrCombobox.listOption = Global.Context.DMSAttributeConfigs.Where(x =>
                                            x.Type == "IN" && x.HierachyLevel.HasValue).Select(
                                            s => new OptionCombobox { ID = s.AttributeNbr.ToString(), Key = s.HierachyLevel.ToString(), Value = s.Attribute }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxHierarchyLevel = ctrCombobox;

            //CbxHierarchyValue
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.HierarchyValue;
            ctrCombobox.TitleKey = Utility.Phrase("HierarchyValueID");
            ctrCombobox.TitleName = Utility.Phrase("HierarchyValueName");
            ctrCombobox.listOption = Global.Context.DMSAttributes.Where(x => x.Type == "IN").Select(
                                            s => new OptionCombobox { ID = s.AttributeID.ToString(), Key = s.AttributeCD.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxHierarchyValue = ctrCombobox;

            //CbxInventoryItem
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.InventoryItem;
            ctrCombobox.TitleKey = Utility.Phrase("InventoryItemID");
            ctrCombobox.TitleName = Utility.Phrase("InventoryItemName");
            ctrCombobox.listOption = Global.Context.pp_GetInventoryByHierarchy(0, string.Empty).Select(
                                            s => new OptionCombobox { ID = s.InventoryID.ToString(), Key = s.InventoryCD, Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxInventoryItem = ctrCombobox;

            //template
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Template;
            ctrCombobox.TitleKey = Utility.Phrase("TemplateID");
            ctrCombobox.TitleName = Utility.Phrase("TemplateName");
            ctrCombobox.listOption = Global.Context.pp_GetReportTemplateBy(model.ReportID).Select(
                                            s => new OptionCombobox { ID = s.TemplateID.ToString(), Key = s.TemplateID.ToString(), Value = s.TemplateName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxTemplate = ctrCombobox;

            //Channel
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Channel;
            ctrCombobox.TitleKey = Utility.Phrase("ChannelID");
            ctrCombobox.TitleName = Utility.Phrase("ChannelName");
            ctrCombobox.listOption = Global.Context.DMSAttributes.Where(x => x.Type == "DC" && x.AttributeNbr == 0).Select(
                                            s => new OptionCombobox { ID = s.AttributeID.ToString(), Key = s.AttributeCD.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxChannel = ctrCombobox;

            //Warehouse
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = model.Warehouse;
            ctrCombobox.TitleKey = Utility.Phrase("WarehouseID");
            ctrCombobox.TitleName = Utility.Phrase("WarehouseName");
            ctrCombobox.listOption = Global.Context.pp_GetWarehouse().Select(
                                            s => new OptionCombobox { ID = s.SiteID.ToString(), Key = s.SiteCD.ToString(), Value = s.Descr }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            model.CbxWarehouse = ctrCombobox;
            model.TreeView = GetTreeViewList();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult RunReport(ReportBasicMV model)
        {
            string reportURL = ControllerHelper.valueCustomSetting("ReportURL");
            string url = string.Empty;
            string fromDate = "";
            string toDate = "";

            #region SetFromToDate
            if (model.Year == 0)
            {
                model.Year = DateTime.Now.Year;
            }

            if (model.TypeDate == TypeDate.D)
            {
                fromDate = model.FromDate.ToReportPattern();
                toDate = model.ToDate.ToReportPattern();
            }
            else if (model.TypeDate == TypeDate.W)
            {
                var week = Global.Context.DMSWeeks.Where(x => x.Year == model.Year.ToString() && x.Week == model.Week.ToString("00") && x.StartDate.HasValue && x.EndDate.HasValue).FirstOrDefault();
                if (week != null)
                {
                    fromDate = week.StartDate.Value.ToReportPattern();
                    toDate = week.EndDate.Value.ToReportPattern();
                }
            }
            else if (model.TypeDate == TypeDate.M)
            {
                var month = Global.Context.DMSMonths.Where(x => x.Year == model.Year.ToString() && x.Month == model.Month.ToString("00")).FirstOrDefault();
                if (month != null)
                {
                    fromDate = month.StartDate.ToReportPattern();
                    toDate = month.EndDate.ToReportPattern();
                }
            }
            else if (model.TypeDate == TypeDate.Q)
            {
                var quarter = Global.Context.DMSQuarters.Where(x => x.Year == model.Year.ToString() && x.Quarter == model.Quarter.ToString("00")).FirstOrDefault();
                if(quarter != null)
                {
                    fromDate = quarter.StartDate.ToReportPattern();
                    toDate = quarter.EndDate.ToReportPattern();
                }
            }
            else
            {
                DateTime firstDay = new DateTime(model.Year, 1, 1);
                DateTime lastDay = new DateTime(model.Year, 12, 31);
                fromDate = firstDay.ToReportPattern();
                toDate = lastDay.ToReportPattern();
            }
            #endregion

            #region Classify Template Report
            if(model.Distributor != 0)
            {

                if (model.ReportID == ReportType.ReportBLSalesIn.ToString())
                {
                    string param = reportURL + "ReportID={0}&CompanyID={1}&DistributorID={2}&LoginID={3}&FromDate={4:yyyy-MM-dd}&ToDate={5:yyyy-MM-dd}" +
                           "&SalesAreaID={6}&HierarchyID={7}&InventoryID={8}&TemplateID={9}";
                    url = string.Format(param,
                                    model.ReportID,
                                    Utility.Encrypt(CacheDataHelper.CommanyID.ToString()),
                                    Utility.Encrypt(model.Distributor.ToString()),
                                    this.UserLogin,
                                    fromDate,
                                    toDate,
                                    model.RegionSale,
                                    model.HierarchyLevel,
                                    model.InventoryItem,
                                    model.Template
                                    );
                }
                else if (model.ReportID == ReportType.ReportBLSales.ToString())
                {
                    string param = reportURL + "ReportID={0}&CompanyID={1}&DistributorID={2}&LoginID={3}&FromDate={4:yyyy-MM-dd}&ToDate={5:yyyy-MM-dd}" +
                               "&CountryID={6}&ProvinceID={7}&SalesAreaID={8}&HierarchyID={9}&InventoryID={10}&TemplateID={11}";
                    url = string.Format(param,
                                    model.ReportID,
                                    Utility.Encrypt(CacheDataHelper.CommanyID.ToString()),
                                    Utility.Encrypt(model.Distributor.ToString()),
                                    this.UserLogin,
                                    fromDate,
                                    toDate,
                                    model.Country,
                                    model.Province,
                                    model.RegionSale,
                                    model.HierarchyLevel,
                                    model.InventoryItem,
                                    model.Template
                                    );
                }
                else if (model.ReportID == ReportType.ReportBLKPI.ToString())
                {
                    string param = reportURL + "ReportID={0}&CompanyID={1}&DistributorID={2}&LoginID={3}&FromDate={4:yyyy-MM-dd}&ToDate={5:yyyy-MM-dd}" +
                        "&KPIPeriodNbr={6}&Period={7}&RefNbr={8}&TemplateID={9}";
                    url = string.Format(param,
                                    model.ReportID,
                                    Utility.Encrypt(CacheDataHelper.CommanyID.ToString()),
                                    Utility.Encrypt(model.Distributor.ToString()),
                                    this.UserLogin,
                                    fromDate,
                                    toDate,
                                    model.KPIPeriodNbr,
                                    model.Period,
                                    model.RefNbr,
                                    model.Template
                                    );
                }
                else if (model.ReportID == ReportType.ReportBLPromotion.ToString())
                {
                    string param = reportURL + "ReportID={0}&CompanyID={1}&DistributorID={2}&LoginID={3}&FromDate={4:yyyy-MM-dd}&ToDate={5:yyyy-MM-dd}" +
                               "&CountryID={6}&ProvinceID={7}&SalesAreaID={8}&PromotionID={9}&SchemeID={10}&DealID={11}&TemplateID={12}";
                    url = string.Format(param,
                                    model.ReportID,
                                    Utility.Encrypt(CacheDataHelper.CommanyID.ToString()),
                                    Utility.Encrypt(model.Distributor.ToString()),
                                    this.UserLogin,
                                    fromDate,
                                    toDate,
                                    model.Country,
                                    model.Province,
                                    model.RegionSale,
                                    model.Promotion,
                                    model.Program,
                                    model.Transaction,
                                    model.Template
                                    );
                }
                else if (model.ReportID == ReportType.ReportInventoryRealtime.ToString())
                {
                    string param = reportURL + "ReportID={0}&CompanyID={1}&DistributorID={2}&LoginID={3}&TemplateID={4}";
                    url = string.Format(param,
                                    model.ReportID,
                                    Utility.Encrypt(CacheDataHelper.CommanyID.ToString()),
                                    Utility.Encrypt(model.Distributor.ToString()),
                                    this.UserLogin,
                                    model.Template
                                    );
                }
                else if (model.ReportID == ReportType.ReportBLINInventoryStock.ToString())
                {
                    string param = reportURL + "ReportID={0}&CompanyID={1}&DistributorID={2}&LoginID={3}&FromDate={4:yyyy-MM-dd}&ToDate={5:yyyy-MM-dd}" +
                               "&ChannelID={6}&SalesAreaID={7}&HierarchyID={8}&InventoryID={9}&SiteID={10}&TemplateID={11}";
                    url = string.Format(param,
                                    model.ReportID,
                                    Utility.Encrypt(CacheDataHelper.CommanyID.ToString()),
                                    Utility.Encrypt(model.Distributor.ToString()),
                                    this.UserLogin,
                                    fromDate,
                                    toDate,
                                    model.Channel,
                                    model.RegionSale,
                                    model.HierarchyLevel,
                                    model.InventoryItem,
                                    model.Warehouse,
                                    model.Template
                                    );
                }
            }
            #endregion
            CustomLog.LogError(url);
            return Json(url, JsonRequestBehavior.AllowGet);
        }


    }
}
