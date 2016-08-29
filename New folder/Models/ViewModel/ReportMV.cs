using DMSERoute.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eRoute.Models.ViewModel
{
    public class ReportMV
    {
    }
    public class ReportBasicMV
    {
        public string ReportID { get; set; }
        public TypeDate TypeDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Week { get; set; }
        public int Month { get; set; }
        public int Quarter { get; set; }
        public int Year { get; set; }
        public int Distributor { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string RegionSale { get; set; }
        public string HierarchyLevel { get; set; }
        public string HierarchyValue { get; set; }
        public string InventoryItem { get; set; }
        public string Template { get; set; }
        public string Promotion { get; set; }
        public string Program { get; set; }
        public string Transaction { get; set; }
        public string KPIPeriod { get; set; }
        public string Channel { get; set; }
        public string Warehouse { get; set; }
        public string KPIPeriodNbr { get; set; }
        public string Period { get; set; }
        public string RefNbr { get; set; }

        public List<SelectListItem> ListTypeDate { get; set; }
        public List<SelectListItem> ListMonth { get; set; }
        public List<SelectListItem> ListQuarter { get; set; }
        public List<SelectListItem> ListYear { get; set; }

        public List<DMSAttributeConfig> ListHierarchyLevel { get; set; }
        public List<ViewControlTreeView> TreeView { get; set; }
        public List<OptionKPIPeriod> ListKPIPeriod { get; set; }
        public List<OptionKPIPeriodConfig> ListKPIPeriodConfig { get; set; }


        public ViewControlCombobox CbxDistributor { get; set; }
        public ViewControlCombobox CbxWeek { get; set; }
        public ViewControlCombobox CbxHierarchyLevel { get; set; }
        public ViewControlCombobox CbxHierarchyValue { get; set; }
        public ViewControlCombobox CbxInventoryItem { get; set; }
        public ViewControlCombobox CbxTemplate { get; set; }
        public ViewControlCombobox CbxChannel { get; set; }
        public ViewControlCombobox CbxWarehouse { get; set; }
        public ViewControlCombobox CbxCountry { get; set; }
        public ViewControlCombobox CbxProvince { get; set; }
        public ViewControlCombobox CbxPromotion { get; set; }

        public ReportBasicMV()
        {
            this.FromDate = DateTime.Now.FirstDayOfMonth();
            this.ToDate = DateTime.Now;
            this.Distributor = 0;
            this.Year = DateTime.Now.Year;
            this.Country = "VN";
            
            List<SelectListItem> list = new List<SelectListItem>(); 
            foreach (TypeDate elm in Enum.GetValues(typeof(TypeDate)))
            {
                list.Add(new SelectListItem { Text = Utility.Phrase("TypeDate_" + elm.ToString()), Value = elm.ToString() });
            }
            this.ListTypeDate = list;

            //for Month
            list = new List<SelectListItem>();
            list = Global.Context.DMSMonths.Where(x => x.Year == DateTime.Now.Year.ToString()).Select(s =>
                          new SelectListItem { Text = s.Month, Value = s.Month }
                        ).ToList();
            this.ListMonth = list;
            //for Quarters
            list = new List<SelectListItem>();
            list = Global.Context.DMSQuarters.Where(x => x.Year == DateTime.Now.Year.ToString()).Select(s =>
                          new SelectListItem { Text = s.Quarter, Value = s.Quarter }
                        ).ToList();
            this.ListQuarter = list;
            //for Year
            list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = DateTime.Now.AddYears(-1).Year.ToString(), Value = DateTime.Now.AddYears(-1).Year.ToString() });
            list.Add(new SelectListItem { Text = DateTime.Now.Year.ToString(), Value = DateTime.Now.Year.ToString() });
            list.Add(new SelectListItem { Text = DateTime.Now.AddYears(1).Year.ToString(), Value = DateTime.Now.AddYears(1).Year.ToString() });
            this.ListYear = list;


            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = this.Distributor.ToString();
            ctrCombobox.TitleKey = Utility.Phrase("DistributorID");
            ctrCombobox.TitleName = Utility.Phrase("DistributorName");
            ctrCombobox.listOption = ControllerHelper.GetListDistributorWithRegionArea(string.Empty, string.Empty).Select(s => new OptionCombobox { ID = s.Distributor_OldID.ToString(), Key = s.DistributorCode.ToString().Trim(), Value = s.DistributorName }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            this.CbxDistributor = ctrCombobox;

            //for Week
            ctrCombobox = new ViewControlCombobox();
            ctrCombobox.SeleteID = this.Week.ToString();
            ctrCombobox.TitleKey = Utility.Phrase("WeekID");
            ctrCombobox.TitleName = Utility.Phrase("WeekName");
            ctrCombobox.listOption = Global.Context.DMSWeeks.Where(x => x.StartDate.HasValue && x.EndDate.HasValue && x.Year == this.Year.ToString()).Select(
                                            s => new OptionCombobox { ID = s.Week, Key = s.Week, Value = s.StartDate.Value.Date.ToString() + " - " + s.EndDate.Value.Date.ToString() }).ToList();
            if (ctrCombobox.listOption.Count == 1)
            {
                ctrCombobox.SeleteID = ctrCombobox.listOption[0].ID;
            }
            this.CbxWeek = ctrCombobox;


        }

        public void SetInitialData(ReportType reportType)
        {
            
        }

    }

    public class OptionKPIPeriod
    {
        public string KPIPeriodNbr { get; set; }
        public string Descr { get; set; }
        public string Period { get; set; }
        public string PeriodFull { get; set; }
        public int SalesOrgID { get; set; }
        public int SalesAreaID { get; set; }
        public string SalesAreaName { get; set; }
        public string CategoryCD { get; set; }
        public string CategoryName { get; set; }
        public string Channel { get; set; }
    }

    public class OptionKPIPeriodConfig
    {
        public string RefNbr { get; set; }
        public string ShortDescription { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string CodeListSalesID { get; set; }
        public string Description { get; set; }
        public string CodeKPI { get; set; }
        public string NameKPI { get; set; }
    }
}