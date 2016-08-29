using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DMSERoute.Helpers;
using eRoute.Models;
using eRoute.Models.ViewModel;
using OfficeOpenXml;
using DevExpress.Web.Internal;
using System.IO;

namespace eRoute.Controllers
{
    public class PhraseController : Controller
    {
        #region Phrase
        [ActionAuthorize("LoadPhrase")]
        public ActionResult LoadPhrase()
        {
            ControllerHelper.LoadPhrase("");
            MvcApplication.ListLanguage = new List<Language>();
            MvcApplication.ListLanguage.AddRange(ControllerHelper.LoadLanguage());
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SetLanguage(string lang)
        {
            HttpCookie ckieLanguage = new HttpCookie(Utils.CurrentLanguageCookieKey);
            HttpContext.Response.Cookies.Remove(Utils.CurrentLanguageCookieKey);
            HttpRuntime.Cache.Remove("CacheListMenu");
            ckieLanguage.Value = lang;
            HttpContext.Response.SetCookie(ckieLanguage);
            return RedirectToAction("Home", "DashBoard");
        }

        [ActionAuthorize("Phrase_Dictionary", true)]
        [CompressFilter]
        public ActionResult Dictionary()
        {
            ViewData["Dictionaries"] = Utility.Dictionaries;
            ViewData["LanguageList"] = MvcApplication.ListLanguage;

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        [CompressFilter]
        [ActionAuthorize("Phrase_Dictionary")]
        public ActionResult Dictionary(string txtSearch, string act)
        {
            var dic = new Dictionary<string, Dictionary<string, string>>();
            List<Language> listLanguage = new List<Language>();
            listLanguage.AddRange(Global.Context.Languages.ToList());

            var language = Global.Context.Languages.Where(m => m.Code == Utility.SessionLanguage).FirstOrDefault();
            if (language != null)
            {
                List<Phrase> listCurrentPhrase = Global.Context.Phrases.Where(a => (a.LanguageID == language.LangID && a.PhraseText.Contains(txtSearch.Trim()))).ToList();
                Dictionary<string, string> listPhrase = new Dictionary<string, string>();
                foreach (Phrase item in listCurrentPhrase)
                {
                    listPhrase.Add(item.PhraseCode, item.PhraseText);
                }
                dic.Add(language.Code, listPhrase);


                foreach (Language item in listLanguage)
                {
                    if (item.Code != Utility.SessionLanguage)
                    {
                        List<Phrase> listNextPhrase = Global.Context.Phrases.Where(x => listCurrentPhrase.Select(s => s.PhraseCode).Contains(x.PhraseCode) && item.LangID == x.LanguageID).ToList();
                        listPhrase = new Dictionary<string, string>();
                        foreach (Phrase item1 in listNextPhrase)
                        {
                            listPhrase.Add(item1.PhraseCode, item1.PhraseText);
                        }
                        dic.Add(item.Code, listPhrase);
                    }
                }
            }
            ViewData["Dictionaries"] = dic;
            ViewData["LanguageList"] = MvcApplication.ListLanguage;

            ControllerHelper.LoadPhrase("");
            return View();
        }

        #region Import From Excel
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult FileUpload(string act)
        {
            PhraseVM model = new PhraseVM();
            #region Request.Files
            string message = "";
            if (Request.Files["uploadFile"] != null && Request.Files["uploadFile"].ContentLength > 0)
            {
                try
                {
                    using (var datafile = new ExcelPackage(Request.Files["uploadFile"].InputStream))
                    {
                        ExcelWorkbook wkb = datafile.Workbook;
                        ExcelWorksheet currentworksheet = wkb.Worksheets[1];
                        List<Phrase> listIP = new List<Phrase>();

                        #region IMPORT TO LIST
                        for (int row = 2; row <= currentworksheet.Dimension.End.Row; row++)
                        {
                            string importMessage = string.Empty;
                            Phrase n = new Phrase()
                            {
                                LanguageID = Convert.ToInt32(Utility.StringParse(currentworksheet.Cells["A" + row.ToString()].Value)),
                                PhraseCode = Utility.StringParse(currentworksheet.Cells["B" + row.ToString()].Value),
                                PhraseText = Utility.StringParse(currentworksheet.Cells["C" + row.ToString()].Value),
                            };
                            listIP.Add(n);
                        }
                        var lstExist = (from ip in listIP
                                        join p in Global.Context.Phrases
                                          on new { ip.PhraseCode, ip.LanguageID } equals new { p.PhraseCode, p.LanguageID }
                                        select ip
                                   ).Distinct().ToList();
                        if (lstExist.Count > 0)
                        {
                            foreach (Phrase de in lstExist)
                            {
                                var update = from e in Global.Context.Phrases where e.LanguageID == de.LanguageID && e.PhraseCode == de.PhraseCode select e;
                                if (update != null)
                                {
                                    (from e in Global.Context.Phrases where e.LanguageID == de.LanguageID && e.PhraseCode == de.PhraseCode select e).ToList().ForEach(s => s.PhraseText = de.PhraseText);
                                }
                                listIP.Remove(de);
                                //Global.Context.Phrases.DeleteOnSubmit(item);
                            }
                        }
                        foreach (Phrase ins in listIP)
                        {
                            Global.Context.Phrases.InsertOnSubmit(ins);
                        }
                        Global.Context.SubmitChanges();
                        message = "File uploaded successfully!";
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    CustomLog.LogError(ex);
                    message = string.Format("File upload failed: {0}", ex.Message);
                    return Json(message, "text/html");
                }
            }
            #endregion
            ControllerHelper.LoadPhrase("");
            return RedirectToAction("Dictionary");
        }
        #endregion

        #region Download Template
        [ActionAuthorize("Phrase_DownTemplate")]
        public ActionResult DownTemplate()
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/PhraseTemplate.xlsx");
            var fileName = "PhraseTemplate.xlsx";
            var mimeType = "application/vnd.ms-excel";
            return File(new FileStream(path, FileMode.Open), mimeType, fileName);
        }
        #endregion

        #region Ajax
         [HttpPost]
        //[ValidateInput(false)]
        //[ActionAuthorize("Dictionary")]
        //[ActionAuthorize("Phrase_UpdatePhrase")]
        public ActionResult UpdatePhrase(string languageID, string code, string text)
        {
            int langID = Convert.ToInt32(languageID);//Utility.IntParse(Utility.DecimalParse(languageID));
            Phrase item = Global.Context.Phrases.FirstOrDefault(a => a.LanguageID == langID && a.PhraseCode == code);
            item.PhraseText = text;
            Global.Context.SubmitChanges();

            return Json(true, JsonRequestBehavior.DenyGet);
        }
        #endregion // End Ajax
        #endregion
    }
}
