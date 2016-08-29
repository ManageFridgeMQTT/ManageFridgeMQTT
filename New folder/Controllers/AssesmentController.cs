using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using eRoute.Models;
using eRoute.Models.ViewModel;
//using DevExpress.Web.ASPxGridView;
using System.Data;
using DevExpress.Web.Data;
using System.Collections;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
//using DevExpress.Web.ASPxEditors;
using eRoute.Localization;

namespace eRoute.Controllers
{
    public class AssesmentController : Controller
    {
        AssesmentVM assesment = new AssesmentVM();
        //Auditor
        public ActionResult AuditorList()
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                assesment.sp_Assesment_Auditor_View = db.sp_Assesment_Auditor_View(DateTime.Now, User.Identity.Name, Utils.CurrentLanguage).ToList();
                var query = db.sp_Get_Status_For_Lang("AD", Utils.CurrentLanguage).ToList();
                ViewData["AD"] = query;
            }
            return View(assesment);
        }
        public PartialViewResult GridViewPartial(string dis)
        {
            ERouteDataContext db = new ERouteDataContext();
            List<sp_Get_Status_For_LangResult> listLang = db.sp_Get_Status_For_Lang("AD", Utils.CurrentLanguage).ToList();
            List<sp_Assesment_Auditor_ViewResult> query = new List<sp_Assesment_Auditor_ViewResult>();
            if (Request.HttpMethod == "POST")
            {
                int cbbDistributor = EditorExtension.GetValue<int>("cbbDistributor");
                string cbbStatus = EditorExtension.GetValue<string>("cbbStatus");
                if (cbbDistributor > 0)
                {
                    query = db.sp_Assesment_Auditor_View(EditorExtension.GetValue<DateTime>("deDate"), User.Identity.Name, Utils.CurrentLanguage).Where(x => x.DistributorID == cbbDistributor).ToList();
                }
                else
                {
                    query = db.sp_Assesment_Auditor_View(EditorExtension.GetValue<DateTime>("deDate"), User.Identity.Name, Utils.CurrentLanguage).ToList();
                }
                assesment.sp_Assesment_Auditor_View_Search = query.Where(x => x.Status == cbbStatus).ToList();
                int countItem = query.Where(x => x.RouteID != null).Count();
                if (countItem > 0)
                {
                    assesment.ItemCount = listLang.Where(z => z.StatusID == 2).FirstOrDefault().Name + " " + query.Where(x => x.Status == listLang.Where(z => z.StatusID == 2).FirstOrDefault().Name).Count() + "/" + query.Count();
                }
                Session["deDate"] = EditorExtension.GetValue<DateTime>("deDate");
            }
            else
            {
                if (Session["deDate"] != null)
                {
                    query = db.sp_Assesment_Auditor_View(DateTime.Parse(Session["deDate"].ToString()), User.Identity.Name, Utils.CurrentLanguage).ToList();
                }
                else
                {
                    assesment.sp_Assesment_Auditor_View_Search = new List<sp_Assesment_Auditor_ViewResult>();
                }
                string strNotYet = listLang.Where(z => z.StatusID == 1).FirstOrDefault().Name;
                assesment.sp_Assesment_Auditor_View_Search = query.Where(x => x.Status.Trim() == strNotYet.Trim()).ToList();
                int countItem = query.Where(x => x.RouteID != null).Count();
                if (countItem > 0)
                {
                    assesment.ItemCount = listLang.Where(z => z.StatusID == 2).FirstOrDefault().Name + " " + query.Where(x => x.Status == listLang.Where(z => z.StatusID == 2).FirstOrDefault().Name).Count() + "/" + query.Count();
                }
            }
            return PartialView(assesment);
        }
        public ActionResult AuditorAssesment(string id)
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                DateTime dt = DateTime.Now;
                int dis = 0;
                if (Session["deDate"] != null)
                {
                    ArrayList ary = new ArrayList();
                    dt = DateTime.Parse(Session["deDate"].ToString());
                    dis = int.Parse(id.Split('-')[1]);
                    ary.Add(dis);
                    ary.Add(dt);
                    ary.Add(id.Split('-')[0]);
                    Session["AddAssesment"] = ary;
                }
                assesment.sp_Assesment_Auditor = db.sp_Assesment_Auditor(dt, dis, id.Split('-')[0]).ToList();
                assesment.list_category = db.sp_Get_Cate_For_Lang_One(Utils.CurrentLanguage, "LD").ToList();
            }
            return View(assesment);
        }
        public ActionResult CreateAuditorAssesment(FormCollection collection)
        {
            ArrayList aryPara = Session["AddAssesment"] as ArrayList;
            if (aryPara != null)
            {
                List<AssesmentTemp> list = new List<AssesmentTemp>();
                ERouteDataContext db = new ERouteDataContext();
                string listCate = string.Empty;
                var query = from x in ((System.Collections.Specialized.NameValueCollection)(collection)).AllKeys where x.ToString() != "DXMVCEditorsValues" select x;
                foreach (var item in query)
                {
                    string[] val = item.Split('|');
                    if (val.Length == 3)
                    {
                        if (EditorExtension.GetValue<string>(item.ToString()) != "True" && val[2].ToString() != "txtNode")
                        {
                            list.Add(new AssesmentTemp { Img = int.Parse(val[0].ToString()), Dis = int.Parse(aryPara[0].ToString()), Date = aryPara[1].ToString(), Route = aryPara[2].ToString() });
                        }
                        else
                        {
                            string[] cut = item.Split('|');
                            string str = cut[0] + "|" + cut[1] + "" + "|txtNode";
                            list.Add(new AssesmentTemp { Img = int.Parse(val[0].ToString()), Dis = int.Parse(aryPara[0].ToString()), Date = aryPara[1].ToString(), Cate = val[2].ToString(), Route = aryPara[2].ToString(), Node = EditorExtension.GetValue<string>(str) });
                        }
                    }
                }
                foreach (var item in list.Select(x => x.Img).Distinct().ToList())
                {
                    Assessment am = new Assessment();
                    string str = string.Empty;
                    var fillSub = list.Where(x => x.Img == int.Parse(item.ToString())).ToList();
                    foreach (var sub in fillSub.Where(x => x.Cate != "txtNode" && x.Cate != null))
                    {
                        str += sub.Cate + ",";
                    }
                    bool checkType = true;
                    am = (from x in db.Assessments where x.DisplayOutletImageID == fillSub.FirstOrDefault().Img && x.DistributorID == fillSub.FirstOrDefault().Dis && x.RouteID == fillSub.FirstOrDefault().Route && x.ImageDate == DateTime.Parse(fillSub.FirstOrDefault().Date) select x).FirstOrDefault();
                    if (am == null)
                    {
                        checkType = false;
                        am = new Assessment();
                    }
                    am.DisplayOutletImageID = fillSub.FirstOrDefault().Img;
                    am.DistributorID = fillSub.FirstOrDefault().Dis;
                    am.CategoryID = str.TrimEnd(',');
                    am.RouteID = fillSub.FirstOrDefault().Route;
                    if (fillSub.Where(x => x.Node != null).Count() > 0)
                    {
                        am.OtherReasons = fillSub.Where(x => x.Node != null).FirstOrDefault().Node;
                    }
                    am.DateAssessment = DateTime.Now;
                    am.StatusAssessment = 3; // Chờ duyệt
                    am.StatusOpenColse = 8;// Mở
                    am.ImageDate = DateTime.Parse(fillSub.FirstOrDefault().Date);
                    if (checkType == false)
                    {
                        am.CreateBy = User.Identity.Name;
                        am.CreateDate = DateTime.Now;
                        db.Assessments.InsertOnSubmit(am);
                    }
                    db.SubmitChanges();
                }
                if (list.Count() <= 0)
                {
                    Assessment am = new Assessment();
                    am.DistributorID = int.Parse(aryPara[0].ToString());
                    am.RouteID = aryPara[2].ToString();
                    am.ImageDate = DateTime.Parse(aryPara[1].ToString());
                    am.DateAssessment = DateTime.Now;
                    am.StatusAssessment = 3; // Chờ duyệt
                    am.StatusOpenColse = 8;// Mở
                    am.CreateBy = User.Identity.Name;
                    am.CreateDate = DateTime.Now;
                    db.Assessments.InsertOnSubmit(am);
                    db.SubmitChanges();
                }
                Session["AddAssesment"] = null;
                return JavaScript("OnSuccess();");
            }
            return JavaScript("OnFailure();");
        }
        //Leader
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LeaderList(string id)
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                ArrayList ar = Session["deDateLeader"] as ArrayList;
                var query = db.sp_Get_Status_For_Lang("LD", Utils.CurrentLanguage).ToList();
                ViewData["LD"] = query;
                string UserName = string.Empty;
                if (id != null)
                {
                    UserName = id;
                }
                else
                {
                    UserName = User.Identity.Name;
                }
                assesment.sp_Assesment_Leader_View = db.sp_Assesment_Leader_View(UserName).ToList();
                string Auditor = string.Empty;
                int status = 3;
                if (ar != null)
                {
                    Auditor = ar[1].ToString();
                    status = int.Parse(ar[3].ToString());
                }
                else
                {
                    Auditor = assesment.sp_Assesment_Leader_View.Select(z => z.LoginIDAuditor).Distinct().FirstOrDefault();
                }
                assesment.sp_Assesment_Leader_View_By_Auditor = assesment.sp_Assesment_Leader_View.Where(x => x.LoginIDAuditor == Auditor).ToList();
                if (id != null)
                {
                    ar = new ArrayList();
                    ar.Add(Session["dt"].ToString());
                    ar.Add(assesment.sp_Assesment_Leader_View_By_Auditor.FirstOrDefault().LoginIDAuditor);
                    ar.Add(-1);
                    ar.Add(status);
                    ar.Add(id);
                    Session["IdLeader"] = id;
                    Session["deDateLeader"] = ar;
                }
            }
            return View(assesment);
        }
        public ActionResult GridViewPartialLeader(FormCollection col)
        {
            ERouteDataContext db = new ERouteDataContext();
            ArrayList ary = Session["deDateLeader"] as ArrayList;
            List<sp_Get_Status_For_LangResult> listLang = db.sp_Get_Status_For_Lang("LD", Utils.CurrentLanguage).ToList();
            string userName = string.Empty;
            if (ary != null)
            {
                if (ary.Count == 5)
                {
                    if (ary[4] != null)
                    {
                        userName = ary[4].ToString();
                    }
                }
                else
                {
                    userName = User.Identity.Name;
                }
            }
            else
            {
                userName = User.Identity.Name;
            }
            if (Request.HttpMethod == "POST")
            {
                int cbbDistributor = EditorExtension.GetValue<int>("cbbDistributor");
                string cbbAuditor = EditorExtension.GetValue<string>("cbbAuditor");
                int cbbStatus = EditorExtension.GetValue<int>("cbbStatus");
                DateTime dt = EditorExtension.GetValue<DateTime>("deDate");
                ary = new ArrayList();
                ary.Add(dt);
                ary.Add(cbbAuditor);
                ary.Add(cbbDistributor);
                ary.Add(cbbStatus);
                ary.Add(userName);
                Session["deDateLeader"] = ary;
                var query = (from x in db.sp_Assesment_Leader_Fillter(dt.Date, userName, cbbDistributor, cbbAuditor, Utils.CurrentLanguage) select x).ToList();
                assesment.sp_Assesment_Leader_Fillter = query.Where(x => x.StatusAssessment == cbbStatus).ToList();
                List<sp_Count_Route_By_LeaderResult> itemCount = db.sp_Count_Route_By_Leader(dt, cbbAuditor).ToList();
                string strCount = string.Empty;
                if (itemCount.Count() > 0)
                {
                    strCount = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + Localization.Localization.SumRoute + ":" + itemCount[0].ItemCount.Value + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + Localization.Localization.ImagesRoute + ":" + itemCount[1].ItemCount.Value + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + Localization.Localization.AssesmentRoute + ":" + itemCount[2].ItemCount.Value;
                }
                assesment.ItemCount = listLang.Where(x => x.StatusID == 5).FirstOrDefault().Name + " " + query.Where(x => x.StatusAssessment == 5).Count() + "/" + query.Where(x => x.StatusAssessment != 6).Count() + strCount;
                if (query.Where(x => x.StatusAssessment == 5).Count() == query.Where(x => x.StatusAssessment != 6).Count())
                {
                    if (query.Where(x => x.StatusAssessment != 6).Count() > 0 && query.Where(x => x.StatusAssessment == cbbStatus).Count() > 0)
                    {
                        if (assesment.sp_Assesment_Leader_Fillter.Count() == int.Parse(itemCount[0].ItemCount.Value.ToString()))
                        {
                            assesment.CheckItem = true;
                        }
                        else
                        {
                            assesment.CheckItem = false;
                        }
                    }
                    else
                    {
                        assesment.CheckItem = false;
                    }
                }
                else
                {
                    assesment.CheckItem = false;
                }
            }
            else
            {
                if (Session["deDateLeader"] != null)
                {
                    ArrayList aryList = Session["deDateLeader"] as ArrayList;
                    string cbbAuditor = aryList[1].ToString();
                    int status = int.Parse(aryList[3].ToString());
                    DateTime dt = DateTime.Parse(aryList[0].ToString());
                    int dis = int.Parse(aryList[2].ToString());
                    var query = (from x in db.sp_Assesment_Leader_Fillter(dt.Date, userName, dis, cbbAuditor, Utils.CurrentLanguage) select x).ToList();
                    assesment.sp_Assesment_Leader_Fillter = query.Where(x => x.StatusAssessment == status).ToList();
                    List<sp_Count_Route_By_LeaderResult> itemCount = db.sp_Count_Route_By_Leader(dt, cbbAuditor).ToList();
                    string strCount = string.Empty;
                    if (itemCount.Count() > 0)
                    {
                        strCount = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + Localization.Localization.SumRoute + ":" + itemCount[0].ItemCount.Value + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + Localization.Localization.ImagesRoute + ":" + itemCount[1].ItemCount.Value + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + Localization.Localization.AssesmentRoute + ":" + itemCount[2].ItemCount.Value;
                    }
                    assesment.ItemCount = listLang.Where(x => x.StatusID == 5).FirstOrDefault().Name + " " + query.Where(x => x.StatusAssessment == 5).Count() + "/" + query.Where(x => x.StatusAssessment != 6).Count() + strCount;
                    if (query.Where(x => x.StatusAssessment == 5).Count() == query.Where(x => x.StatusAssessment != 6).Count())
                    {
                        if (query.Where(x => x.StatusAssessment != 6).Count() > 0 && query.Where(x => x.StatusAssessment == status).Count() > 0)
                        {
                            if (assesment.sp_Assesment_Leader_Fillter.Count() == int.Parse(itemCount[0].ItemCount.Value.ToString()))
                            {
                                assesment.CheckItem = true;
                            }
                            else
                            {
                                assesment.CheckItem = false;
                            }
                        }
                        else
                        {
                            assesment.CheckItem = false;
                        }
                    }
                    else
                    {
                        assesment.CheckItem = false;
                    }
                }
                else
                {
                    assesment.sp_Assesment_Leader_Fillter = new List<sp_Assesment_Leader_FillterResult>();
                }
            }
            return PartialView(assesment);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ComboBoxPartialDistributor(string id)
        {
            string cbbAuditor = Request.Params["cbbAuditor"].ToString();
            ERouteDataContext db = new ERouteDataContext();
            assesment.sp_Assesment_Leader_View = db.sp_Assesment_Leader_View(User.Identity.Name).ToList();
            assesment.sp_Assesment_Leader_View_By_Auditor = assesment.sp_Assesment_Leader_View.Where(x => x.LoginIDAuditor == cbbAuditor).ToList();
            return PartialView(assesment);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LeaderAssesment(string id)
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                DateTime dt = DateTime.Now;
                int dis = 0;
                if (Session["deDateLeader"] != null)
                {
                    ArrayList ary = Session["deDateLeader"] as ArrayList;
                    dt = DateTime.Parse(ary[0].ToString());
                    dis = int.Parse(id.Split('-')[1]);
                    ary = new ArrayList();
                    ary.Add(dis);
                    ary.Add(dt);
                    ary.Add(id.Split('-')[0]);
                    Session["AddLeaderAssesment"] = ary;
                }
                assesment.sp_Assesment_Auditor = db.sp_Assesment_Auditor(dt, dis, id.Split('-')[0]).ToList();
                if (assesment.sp_Assesment_Auditor.Count() > 0)
                {
                    ViewBag.Name = db.sp_Get_Status_For_Lang("LD", Utils.CurrentLanguage).Where(x => x.StatusID == assesment.sp_Assesment_Auditor.FirstOrDefault().StatusAssessment).FirstOrDefault().Name;
                }
                assesment.list_category = db.sp_Get_Cate_For_Lang_One(Utils.CurrentLanguage, "LD").ToList();
            }
            return View(assesment);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LeaderAssesment(FormCollection col)
        {
            ArrayList aryPara = Session["AddLeaderAssesment"] as ArrayList;
            if (aryPara != null)
            {
                string Node = EditorExtension.GetValue<string>("txtMemo");
                string btnRefusal = EditorExtension.GetValue<string>("btnRefusal");
                string btnAcc = EditorExtension.GetValue<string>("btnAcc");
                string btnAss = EditorExtension.GetValue<string>("btnAss");
                List<AssesmentTemp> list = new List<AssesmentTemp>();
                ERouteDataContext db = new ERouteDataContext();
                string listCate = string.Empty;
                var query = from x in ((System.Collections.Specialized.NameValueCollection)(col)).AllKeys where x.ToString() != "DXMVCEditorsValues" select x;
                foreach (var item in query)
                {
                    string[] val = item.Split('|');
                    if (val.Length == 3)
                    {
                        if (EditorExtension.GetValue<string>(item.ToString()) != "True" && val[2].ToString() != "txtNode")
                        {
                            list.Add(new AssesmentTemp { Img = int.Parse(val[0].ToString()), Dis = int.Parse(aryPara[0].ToString()), Date = aryPara[1].ToString(), Route = aryPara[2].ToString() });
                        }
                        else
                        {
                            string[] cut = item.Split('|');
                            string str = cut[0] + "|" + cut[1] + "" + "|txtNode";
                            list.Add(new AssesmentTemp { Img = int.Parse(val[0].ToString()), Dis = int.Parse(aryPara[0].ToString()), Date = aryPara[1].ToString(), Cate = val[2].ToString(), Route = aryPara[2].ToString(), Node = EditorExtension.GetValue<string>(str) });
                        }
                    }
                }
                var dsitinct = list.Select(x => new { x.Date, x.Route }).Distinct().ToList();
                if (btnRefusal != null)
                {
                    if (Node != null)
                    {
                        DateTime dt = DateTime.Parse(dsitinct.FirstOrDefault().Date);
                        db.sp_Update_StatusAssessment(4, dsitinct.FirstOrDefault().Route, dt.Date, null, null, Node);
                        return JavaScript("OnRefusal();");
                    }
                    else
                    {
                        return JavaScript("OnRefusalNotEmpty();");
                    }
                }
                if (btnAcc != null)
                {
                    DateTime dt = DateTime.Parse(aryPara[1].ToString());
                    db.sp_Update_StatusAssessment(5, aryPara[2].ToString(), dt.Date, DateTime.Now.Date, User.Identity.Name, string.Empty);
                    return JavaScript("OnSuccessAcc();");
                }
                if (btnAss != null)
                {
                    var dsitinctAss = list.Select(x => new { x.Img, x.Date, x.Route, x.Dis }).Distinct().ToList();
                    foreach (var item in list.Select(x => x.Img).Distinct().ToList())
                    {
                        Assessment am = new Assessment();
                        string str = string.Empty;
                        var fillSub = list.Where(x => x.Img == int.Parse(item.ToString())).ToList();
                        foreach (var sub in fillSub.Where(x => x.Cate != "txtNode" && x.Cate != null))
                        {
                            str += sub.Cate + ",";
                        }
                        am = (from x in db.Assessments where x.DisplayOutletImageID == fillSub.FirstOrDefault().Img && x.DistributorID == fillSub.FirstOrDefault().Dis && x.RouteID == fillSub.FirstOrDefault().Route && x.ImageDate == DateTime.Parse(fillSub.FirstOrDefault().Date) select x).FirstOrDefault();
                        if (am != null)
                        {
                            am.DisplayOutletImageID = fillSub.FirstOrDefault().Img;
                            am.DistributorID = fillSub.FirstOrDefault().Dis;
                            am.CategoryID = str.TrimEnd(',');
                            am.RouteID = fillSub.FirstOrDefault().Route;
                            if (fillSub.Where(x => x.Node != null).Count() > 0)
                            {
                                am.OtherReasons = fillSub.Where(x => x.Node != null).FirstOrDefault().Node;
                            }
                            else
                            {
                                am.OtherReasons = null;
                            }
                            am.DateAssessment = DateTime.Now.Date;
                            am.ImageDate = DateTime.Parse(fillSub.FirstOrDefault().Date);
                            db.SubmitChanges();
                        }
                    }
                    DateTime dt = DateTime.Parse(dsitinct.FirstOrDefault().Date);
                    db.sp_Update_StatusAssessment(5, dsitinct.FirstOrDefault().Route, dt.Date, DateTime.Now.Date, User.Identity.Name, string.Empty);
                    return JavaScript("OnSuccess();");
                }
            }
            return JavaScript("OnFailure();");
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LeaderFis(FormCollection col, string id)
        {
            if (Session["deDateLeader"] != null)
            {
                ArrayList aryPara = Session["deDateLeader"] as ArrayList;
                using (ERouteDataContext db = new ERouteDataContext())
                {
                    int cbbDistributor = int.Parse(aryPara[2].ToString());
                    DateTime dt = DateTime.Parse(aryPara[0].ToString()).Date;
                    string auditor = aryPara[1].ToString();
                    if (cbbDistributor != -1)
                    {
                        db.sp_Update_StatusAssessment_Finish(6, cbbDistributor, dt, DateTime.Now.Date, User.Identity.Name);
                    }
                    else
                    {
                        db.sp_Update_StatusAssessment_Finish_All_Dis(6, auditor, dt, DateTime.Now.Date, User.Identity.Name);
                    }
                    db.SubmitChanges();
                }
                aryPara.RemoveAt(3);
                aryPara.Add(4);
                Session["deDateLeader"] = aryPara;
                return JavaScript("OnSuccess();");
            }
            return JavaScript("OnFailure();");
        }
        //Manage
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ManageList()
        {
            return View(assesment);
        }
        public ActionResult GridViewPartialManage(FormCollection clo)
        {
            DateTime dt = new DateTime();
            if (EditorExtension.GetValue<string>("deDate") == null)
            {
                if (Session["dt"] != null)
                {
                    dt = DateTime.Parse(Session["dt"].ToString());
                }
                else
                {
                    assesment.sp_List_Manage_View = new List<sp_List_Manage_ViewResult>();
                    return PartialView(assesment);
                }
            }
            else
            {
                dt = EditorExtension.GetValue<DateTime>("deDate");
                Session["dt"] = dt.Date;
            }
            using (ERouteDataContext db = new ERouteDataContext())
            {
                assesment.sp_List_Manage_View = db.sp_List_Manage_View(dt.Date).ToList();
                var query = assesment.sp_List_Manage_View.Select(x => x.LoginID).Distinct().ToList();
                List<sp_List_Manage_ViewResult> list = new List<sp_List_Manage_ViewResult>();
                if (query.Count() != assesment.sp_List_Manage_View.Count())
                {
                    foreach (var item in query)
                    {
                        var fillter = assesment.sp_List_Manage_View.Where(x => x.LoginID == item.ToString());
                        if (fillter.Count() > 1)
                        {
                            var fillterAgaint = fillter.Where(x => x.Status.Contains("Chưa hoàn thành")).FirstOrDefault();
                            if (fillterAgaint == null)
                            {
                                fillterAgaint = fillter.Where(x => x.StatusOpenColse.Contains("Mở")).FirstOrDefault();
                            }
                            list.Add(new sp_List_Manage_ViewResult { LoginID = fillterAgaint.LoginID, FullName = fillterAgaint.FullName, Status = fillterAgaint.Status, StatusOpenColse = fillterAgaint.StatusOpenColse });
                        }
                        else
                        {
                            list.Add(new sp_List_Manage_ViewResult { LoginID = fillter.FirstOrDefault().LoginID, FullName = fillter.FirstOrDefault().FullName, Status = fillter.FirstOrDefault().Status, StatusOpenColse = fillter.FirstOrDefault().StatusOpenColse });
                        }
                    }
                    assesment.sp_List_Manage_View = list;
                }
                if (assesment.sp_List_Manage_View.Where(x => x.Status.ToString() != "Hoàn thành").Count() > 0)
                {
                    assesment.CheckItem = false;
                }
                else
                {
                    assesment.CheckItem = true;
                }
            }
            return PartialView(assesment);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ManageFis(FormCollection col)
        {
            if (Session["dt"] != null)
            {
                using (ERouteDataContext db = new ERouteDataContext())
                {
                    db.sp_Update_StatusOpenColse(7, DateTime.Parse(Session["dt"].ToString()).Date, DateTime.Now, User.Identity.Name);
                    db.SubmitChanges();
                    return JavaScript("OnSuccess();");
                }
            }
            return JavaScript("OnFailure();");
        }
        //ReAssesment
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AssesmentList()
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                var listLeader = db.sp_Get_All_Info_Leader().ToList();
                if (listLeader.Count() > 0)
                {
                    ViewData["cbbLeader"] = listLeader;
                    int cbbAuditor = listLeader.FirstOrDefault().UserId;
                    ArrayList ary = (ArrayList)Session["DateUpdateStatus"];
                    if (ary != null)
                    {
                        cbbAuditor = int.Parse(ary[2].ToString());
                    }
                    var listAuditor = db.sp_Get_All_Info_Auditor(cbbAuditor).ToList();
                    ViewData["cbbAuditor"] = listAuditor;
                    int UserId = 0;
                    if (ary != null)
                    {
                        UserId = int.Parse(ary[3].ToString());
                    }
                    else
                    {
                        UserId = listAuditor.FirstOrDefault().UserId;
                    }
                    var listDistributor = db.sp_Get_All_Info_Distributor(UserId).ToList();
                    ViewData["cbbDistributor"] = listDistributor;
                }
                else
                {
                    ViewData["cbbDistributor"] = new List<sp_Get_All_Info_DistributorResult>();
                }

            }
            return View();
        }
        public static int UserID = int.MinValue;
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ComboBoxPartialAuditor()
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                int cbbLeader = int.Parse(Request.Params["cbbLeader"].ToString());
                var listAuditor = db.sp_Get_All_Info_Auditor(cbbLeader).ToList();
                ViewData["cbbAuditor"] = listAuditor;
                UserID = listAuditor.FirstOrDefault().UserId;
                return PartialView(ViewData["cbbAuditor"]);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ComboBoxPartialDis()
        {
            using (ERouteDataContext db = new ERouteDataContext())
            {
                int cbbAuditor = int.Parse(Request.Params["cbbAuditor"].ToString());
                List<sp_Get_All_Info_DistributorResult> listDistributor = db.sp_Get_All_Info_Distributor(cbbAuditor).ToList();
                ViewData["cbbDistributor"] = listDistributor;
                return PartialView(ViewData["cbbDistributor"]);
            }
        }
        public ActionResult GridViewPartialAssesment(FormCollection clo)
        {
            ERouteDataContext db = new ERouteDataContext();
            if (Request.HttpMethod == "POST")
            {
                DateTime deDate = EditorExtension.GetValue<DateTime>("deDate");
                int cbbLeader = EditorExtension.GetValue<int>("cbbLeader");
                int cbbAuditor = EditorExtension.GetValue<int>("cbbAuditor");
                int cbbDistributor = EditorExtension.GetValue<int>("cbbDistributor");
                ArrayList ary = new ArrayList();
                ary.Add(deDate);
                ary.Add(cbbDistributor);
                ary.Add(cbbLeader);
                ary.Add(cbbAuditor);
                Session["DateUpdateStatus"] = ary;
                assesment.sp_Manage_View_Status_Open_Day = db.sp_Manage_View_Status_Open_Day(deDate, cbbLeader, cbbAuditor, cbbDistributor, Utils.CurrentLanguage).ToList();
            }
            else
            {
                if (Session["DateUpdateStatus"] != null)
                {
                    ArrayList ary = (ArrayList)Session["DateUpdateStatus"];
                    DateTime deDate = DateTime.Parse(ary[0].ToString());
                    int cbbLeader = int.Parse(ary[2].ToString());
                    int cbbAuditor = int.Parse(ary[3].ToString());
                    int cbbDistributor = int.Parse(ary[1].ToString());
                    assesment.sp_Manage_View_Status_Open_Day = db.sp_Manage_View_Status_Open_Day(deDate, cbbLeader, cbbAuditor, cbbDistributor, Utils.CurrentLanguage).ToList();
                }
            }
            return PartialView(assesment);
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UpdateStatusOpenOrCloseDay(FormCollection col)
        {
            if (Session["DateUpdateStatus"] != null)
            {
                ArrayList ary = (ArrayList)Session["DateUpdateStatus"];
                int dis = int.Parse(ary[1].ToString());
                DateTime dt = DateTime.Parse(ary[0].ToString());
                bool checkVali = true;
                string btnOpen = EditorExtension.GetValue<string>("btnOpen");
                string btnClose = EditorExtension.GetValue<string>("btnClose");
                string txtNode = EditorExtension.GetValue<string>("txtNode");
                ERouteDataContext db = new ERouteDataContext();
                var query = from x in ((System.Collections.Specialized.NameValueCollection)(col)).AllKeys where x.Split('|').Length == 2 select x;
                if (txtNode == string.Empty || txtNode == null)
                {
                    checkVali = false;
                }
                if (checkVali)
                {
                    if (btnOpen != null)
                    {
                        checkVali = false;
                        foreach (var item in query)
                        {
                            string value = EditorExtension.GetValue<string>(item);
                            if (value == "C" || value == "True")
                            {
                                db.sp_Update_Assessment_OpenDay(dt, item.TrimEnd('|'), 8, txtNode, DateTime.Now.Date);
                                db.SubmitChanges();
                                checkVali = true;
                            }
                        }
                        if (checkVali)
                        {
                            return JavaScript("OnOpen();");
                        }
                        else
                        {
                            return JavaScript("OnSelectRoute();");
                        }
                    }
                }
                else
                {
                    return JavaScript("OnReOpen();");
                }
                if (btnClose != null)
                {
                    checkVali = false;
                    foreach (var item in query)
                    {
                        string value = EditorExtension.GetValue<string>(item);
                        if (value == "C" || value == "True")
                        {
                            db.sp_Update_Assessment_OpenDay(dt, item.TrimEnd('|'), 7, txtNode, DateTime.Now.Date);
                            db.SubmitChanges();
                            checkVali = true;
                        }
                    }
                    if (checkVali)
                    {
                        return JavaScript("OnClose();");
                    }
                    else
                    {
                        return JavaScript("OnSelectRoute();");
                    }
                }
            }
            return JavaScript("OnFailure();");
        }
    }
}
