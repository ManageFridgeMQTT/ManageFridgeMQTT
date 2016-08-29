using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eRoute.Models;
//using DevExpress.Web.ASPxEditors;
using DevExpress.Web.Mvc;
using eRoute.Models.ViewModel;
using DMSERoute.Helpers;
using DevExpress.Web;

namespace eRoute.Controllers
{
    public class HomeController : Controller
    {
        string username = SessionHelper.GetSession<string>("UserName");

        public ActionResult Index()
        {
            return RedirectToAction("Home", "DashBoard");
            //return RedirectToAction("TerritoryPerformance", "Tracking");
            //return RedirectToAction("Heatmap", "DashBoard");
            //return View();
        }

        public ActionResult GeoServer()
        {
            return View();
        }

        public ActionResult HtmlEditorPartialView()
        {
            return PartialView("HtmlEditorPartialView");
        }
        #region Category
        int id = int.MinValue;
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionAuthorize("Home_Category", true)]
        public ActionResult Category()
        {
            CategoryVM wrapperModel = new CategoryVM();
            ERouteDataContext db = new ERouteDataContext();
            {
                wrapperModel.ProjectListModel = db.sp_Get_Cate_For_Lang_All(Utils.CurrentLanguage).ToList();
                if (id != int.MinValue)
                {
                    wrapperModel.Get_Cate_For_Lang = wrapperModel.ProjectListModel.Where(x => x.CategoryID == id).ToList();
                    wrapperModel.ProjectNewModel = (from x in db.Categories where x.CategoryID == id orderby x.Order select x).FirstOrDefault();
                    wrapperModel.ProjectNewModel.Node = wrapperModel.ProjectNewModel.ParentID + "|" + (wrapperModel.ProjectNewModel.Level.Value - 1);
                    wrapperModel.Get_Cate_For_Lang.FirstOrDefault().Node = wrapperModel.ProjectNewModel.ParentID + "|" + (wrapperModel.ProjectNewModel.Level.Value - 1);
                }
                else
                {
                    wrapperModel.Get_Cate_For_Lang = new List<sp_Get_Cate_For_Lang_AllResult>();
                }
                List<Language> lag = db.Languages.Where(x => x.Active == true).ToList();
                List<sp_Get_Cate_For_Lang_By_CateIDResult> CateId = db.sp_Get_Cate_For_Lang_By_CateID(id).ToList();
                ViewData["Lang"] = lag;
                ViewData["CateID"] = CateId;
            }
            return View(wrapperModel);
        }
        public ActionResult EditCategory(CategoryVM cate, int Id)
        {
            id = Id;
            bool checkValue = true;
            if (Request.HttpMethod == "POST")
            {
                using (ERouteDataContext db = new ERouteDataContext())
                {
                    List<Language> lag = db.Languages.Where(x => x.Active == true).ToList();
                    string strName = string.Empty;
                    foreach (var item in lag)
                    {
                        strName = EditorExtension.GetValue<string>("txt" + item.LangName);
                        if (strName != null)
                        {
                            checkValue = true;
                        }
                        else
                        {
                            checkValue = false;
                        }
                    }
                    if (checkValue == false)
                    {
                        checkValue = false;
                        return JavaScript("OnName();");
                    }
                    string strOrder = EditorExtension.GetValue<string>("txtOrder");
                    if (strOrder == null)
                    {
                        checkValue = false;
                        return JavaScript("OnOrder();");
                    }
                    string[] CategoryID = EditorExtension.GetValue<string>("CategoryID").Split('|');
                    string status = EditorExtension.GetValue<string>("Status");
                    if (status == null)
                    {
                        status = "False";
                    }
                    string strCode = EditorExtension.GetValue<string>("txtCode");
                    string strDes = EditorExtension.GetValue<string>("txtDes");
                    Category ct = (from x in db.Categories where x.CategoryID == Id select x).FirstOrDefault();
                    if (strCode != ct.Code)
                    {
                        var checkdistinct = (from x in db.Categories where x.Code == strCode select x).ToList();
                        if (checkdistinct.Count() > 0)
                        {
                            checkValue = false;
                            return JavaScript("OnDistinct();");
                        }
                    }
                    if (checkValue && ModelState.IsValid)
                    {
                        if (CategoryID.Length <= 1)
                        {
                            ct.Level = 1;
                            ct.Status = bool.Parse(status);
                            ct.ParentID = 0;
                        }
                        else
                        {
                            ct.Level = int.Parse(CategoryID[1]) + 1;
                            ct.Status = bool.Parse(status);
                            ct.ParentID = int.Parse(CategoryID[0]);
                        }
                        ct.Code = strCode;
                        ct.Order = int.Parse(strOrder);
                        ct.Description = strDes;
                        db.SubmitChanges();
                        cate.ProjectNewModel = null;
                        foreach (var item in lag)
                        {
                            CategoryForLang catefor = db.CategoryForLangs.Where(x => x.CategoryID == id && x.LangID == item.LangID).FirstOrDefault();
                            catefor.Name = EditorExtension.GetValue<string>("txt" + item.LangName);
                            db.SubmitChanges();
                        }
                        return RedirectToAction("Category");
                    }
                }
            }
            Category();
            return View("Category");
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Category(CategoryVM cate, FormCollection col)
        {
            bool checkValue = true;
            ERouteDataContext db = new ERouteDataContext();
            List<Language> lag = db.Languages.Where(x => x.Active == true).ToList();
            string strName = string.Empty;
            foreach (var item in lag)
            {
                strName = EditorExtension.GetValue<string>("txt" + item.LangName);
                if (strName != null)
                {
                    checkValue = true;
                }
                else
                {
                    checkValue = false;
                }
            }
            if (checkValue == false)
            {
                checkValue = false;
                return JavaScript("OnName();");
            }
            string strOrder = EditorExtension.GetValue<string>("txtOrder");
            if (strOrder == null)
            {
                checkValue = false;
                return JavaScript("OnOrder();");
            }
            string[] CategoryID = EditorExtension.GetValue<string>("CategoryID").Split('|');
            string status = EditorExtension.GetValue<string>("Status");
            if (status == null)
            {
                status = "False";
            }
            string strCode = EditorExtension.GetValue<string>("txtCode");
            string strDes = EditorExtension.GetValue<string>("txtDes");
            var checkdistinct = (from x in db.Categories where x.Code == strCode select x).ToList();
            if (checkdistinct.Count() > 0)
            {
                checkValue = false;
                return JavaScript("OnDistinct();");
            }
            Category category = new Category();
            if (checkValue && ModelState.IsValid)
            {
                if (CategoryID.Length <= 1)
                {
                    category.Level = 1;
                    category.Status = bool.Parse(status);
                    category.ParentID = 0;
                }
                else
                {
                    category.Level = int.Parse(CategoryID[1]) + 1;
                    category.Status = bool.Parse(status);
                    category.ParentID = int.Parse(CategoryID[0]);
                }
                category.Code = strCode;
                category.Order = int.Parse(strOrder);
                category.Description = strDes;
                db.Categories.InsertOnSubmit(category);
                db.SubmitChanges();
                foreach (var item in lag)
                {
                    CategoryForLang cateforlang = new CategoryForLang();
                    cateforlang.CategoryID = category.CategoryID;
                    strName = EditorExtension.GetValue<string>("txt" + item.LangName);
                    cateforlang.LangID = item.LangID;
                    cateforlang.Name = strName;
                    db.CategoryForLangs.InsertOnSubmit(cateforlang);
                    db.SubmitChanges();
                }
                return JavaScript("OnSuccessCreate();");
            }
            cate.ProjectNewModel.Node = null;
            cate.ProjectNewModel = null;
            return View(cate);
        }
        public ActionResult CategoryTreeView()
        {
            ERouteDataContext db = new ERouteDataContext();
            CategoryVM wrapperModel = new CategoryVM();
            wrapperModel.ProjectListModel = db.sp_Get_Cate_For_Lang_All(Utils.CurrentLanguage).ToList();
            return PartialView(wrapperModel);
        }
        public ActionResult DeleteCategory(int Id)
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                Category ct = (from x in db.Categories where x.CategoryID == Id select x).FirstOrDefault();
                var query = db.CategoryForLangs.Where(x => x.CategoryID == Id).ToList();
                db.CategoryForLangs.DeleteAllOnSubmit(query);
                db.Categories.DeleteOnSubmit(ct);
                db.SubmitChanges();
            }
            return RedirectToAction("Category");
        }
        public static void CreateTreeViewNodesRecursive(List<sp_Get_Cate_For_Lang_AllResult> listCategory, MVCxTreeViewNodeCollection nodesCollection, int parentID)
        {
            foreach (var row in listCategory.Where(x => x.ParentID == parentID).OrderBy(x => x.Order))
            {
                MVCxTreeViewNode node = nodesCollection.Add(row.Name, row.CategoryID.ToString());
                node.Checked = row.Status.Value;
                node.NavigateUrl = "~/Home/EditCategory/" + row.CategoryID.ToString();
                //node.
                CreateTreeViewNodesRecursive(listCategory, node.Nodes, row.CategoryID);
            }
        }
        private static string Writeline(int number)
        {
            string str = string.Empty;
            for (int i = 0; i < number; i++)
            {
                str += " - ";
            }
            return str;
        }
        public static void CreateComboBoxRecursive(List<sp_Get_Cate_For_Lang_AllResult> listCategory, ListEditItemCollection cbb, int parentID)
        {
            foreach (var row in listCategory.Where(x => x.ParentID == parentID))
            {
                cbb.Add(Writeline(row.Level.Value) + row.Name + Writeline(row.Level.Value), row.CategoryID + "|" + row.Level);
                CreateComboBoxRecursive(listCategory, cbb, row.CategoryID);
            }
        }
        #endregion

        #region
        [Authorize]
        [ActionAuthorize("CustomSetting", true)]
        public ActionResult CustomSetting()
        {
            PermissionHelper.CheckPermissionByFeature("CustomSetting", this);
            var model = Global.Context.CustomColorSettings.ToList();
            // var model = Utility.CopyList<CustomColorSetting>(listSetting);
            SessionHelper.SetSession<List<CustomColorSetting>>("CustomSetting", model);
            return View(model);
        }
        public ActionResult CustomSettingPartialView()
        {
            var model = SessionHelper.GetSession<List<CustomColorSetting>>("CustomSetting");
            return PartialView("CustomSettingPartialView", model);
        }
        public ActionResult ActionUserTable(string username, string action)
        {
            return PartialView();
        }
        [HttpPost]
        [ActionAuthorize("CustomSetting_AddNewSetting")]
        public ActionResult AddNewSetting(CustomColorSetting setting)
        {               
            var model = SessionHelper.GetSession<List<CustomColorSetting>>("CustomSetting");
            try
            {
                if (!string.IsNullOrEmpty(setting.SettingName) && !string.IsNullOrEmpty(setting.Type) && !string.IsNullOrEmpty(setting.Color)
                    && setting.ValueFrom != null && setting.ValueTo != null)
                {
                    CustomColorSetting vCustomSetting = new CustomColorSetting()
                    {
                        SettingName = setting.SettingName,
                        ValueFrom = setting.ValueFrom,
                        ValueTo = setting.ValueTo,
                        Type = setting.Type,
                        Color = setting.Color
                    };
                    Global.Context.CustomColorSettings.InsertOnSubmit(vCustomSetting);
                    Global.Context.SubmitChanges();
                    model.Add(vCustomSetting);
                    ViewData["StatusMessage"] = Utility.Phrase("CreateSuccessfull");
                }
                else
                {
                    ViewData["Error"] = Utility.Phrase("MissInformation");
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
            }
            return PartialView("CustomSettingPartialView",model);
        }
        [HttpPost]
        [ActionAuthorize("CustomSetting_UpdateSetting")]
        public ActionResult UpdateSetting(CustomColorSetting setting)
        {
            var model = SessionHelper.GetSession<List<CustomColorSetting>>("CustomSetting");
            try
            {
                if (!string.IsNullOrEmpty(setting.SettingName) && !string.IsNullOrEmpty(setting.Type) && !string.IsNullOrEmpty(setting.Color)
                   && setting.ValueFrom != null && setting.ValueTo != null)
                {
                    CustomColorSetting vCustomSetting = Global.Context.CustomColorSettings.FirstOrDefault(a => a.ID == setting.ID);
                    if (vCustomSetting != null)
                    {
                        vCustomSetting.SettingName = setting.SettingName;
                        vCustomSetting.ValueFrom = setting.ValueFrom;
                        vCustomSetting.ValueTo = setting.ValueTo;
                        vCustomSetting.Type = setting.Type;
                        vCustomSetting.Color = setting.Color;
                        Global.Context.SubmitChanges();
                        ViewData["StatusMessage"] = Utility.Phrase("UpdateSuccessfull");

                        model = UpdateUserForDataSession(model, vCustomSetting);
                        SessionHelper.SetSession<List<CustomColorSetting>>("CustomSetting", model);
                    }
                }
                else
                {
                    ViewData["Error"] = Utility.Phrase("MissInformation");
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
            }
            return PartialView("CustomSettingPartialView",model);
        }
        [NonAction]
        public static List<CustomColorSetting> UpdateUserForDataSession(List<CustomColorSetting> list, CustomColorSetting userReplace)
        {
            int index = list.FindIndex(x => x.ID.Equals(userReplace.ID));
            if (index > -1)
            {
                list[index] = userReplace;
            }
            return list;
        }
        public ActionResult DeleteSetting(string pID)
        {
            var model = SessionHelper.GetSession<List<CustomColorSetting>>("CustomSetting");
            try
            {
                if (pID != null)
                {
                    CustomColorSetting vCustomSetting = Global.Context.CustomColorSettings.FirstOrDefault(a => a.ID == Utility.IntParse(pID));
                    if (vCustomSetting != null)
                    {
                        model.Remove(vCustomSetting);
                        Global.Context.CustomColorSettings.DeleteOnSubmit(vCustomSetting);
                        Global.Context.SubmitChanges();
                        SessionHelper.SetSession<List<CustomColorSetting>>("CustomSetting", model);
                        ViewData["StatusMessage"] = Utility.Phrase("DeleteSuccessfull");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message;
            }
            return PartialView("CustomSettingPartialView", model);
        }
        #endregion
    }
}