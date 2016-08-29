using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using System.Data;
using System.Configuration;
using System.Web.Configuration;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Net.Mail;
using eRoute.Models;
using DevExpress.Web;
using System.Linq.Expressions;
using System.Net;

namespace DMSERoute.Helpers
{
    public class Utility
    {
        public const string GroupMenu = "Menu";
        public const string IconClassDefault = "icon-etool-dashboard";
        public static string pathMatlab = string.Empty;
        public static string LogPath = String.Empty;
        public static byte[] bytes = Encoding.ASCII.GetBytes("DMSpro00");

        

        #region Enum
        public enum TypeData
        {
            Acu,
            ETool
        }

        public enum RoleName
        {
            TDV = 4,
            SS = 6,
            ASM = 1,
            RSM = 3,
            Auditor = 10,
            Leader = 9,
            NSD = 2,
            Admin = 5,
            SuperAdmin = 7,
            TradeMarketing = 100
        }

        public enum Target
        {
            SM,
            SS,

            Revenue,
            Visit,

        }

        public enum ChartName
        {
            RevenueDayOfSS,
            RevenueDayOfASM_RSM,
            RevenueDayPerOfSS,
            RevenueDayPerOfASM_RSM,
            VisitDayOfSS,
            VisitDayOfASM_RSM,
            VisitDayPerOfSS,
            VisitDayPerOfASM_RSM,
            VisitMonthOfASM_RSM,
            VisitMonthLine,
            SyncDaySS,
            SyncDayASM_RSM,
            RevenueInMonth,
            RevenueInMonthOfBOD,
            OtherInDay,
            QuantityInMonth,
            OtherInMonth,
            PieVisitInDay,
            PieVisitInMonth,
            PieOrderInDay,
            PieOrderInMonth
        }
        public enum TypeID
        {
            Evaluation,
            Outlet,
            Employer,
            ImgMarking,
            ImgFakes,
            ImgNotPass,
            ImgNotStandards,
            ImgNotNumberic,
            ImgNotYesMarking
        }

        public enum StatusAutoMark
        {
            New,
            InProcessing,
            Done
        }
        #endregion

        #region Encrypt/Decrypt CompanyID 
        public static string Encrypt(string originalString)
        {
            if (String.IsNullOrWhiteSpace(originalString))
                return String.Empty;

            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
            var writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();

            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        public static string Decrypt(string cryptedString)
        {
            if (String.IsNullOrWhiteSpace(cryptedString))
                return String.Empty;

            var cryptoProvider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream
                (Convert.FromBase64String(cryptedString));
            var cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            var reader = new StreamReader(cryptoStream);

            return reader.ReadToEnd();
        }
        #endregion

        public static string DetermineCompName(string IP)
        {
            IPAddress myIP = IPAddress.Parse(IP);
            IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
            List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
            return compName.First();
        }

        #region PARAM Language
        public static Dictionary<string, Dictionary<string, string>> Dictionaries;
        public static string DefaultLanguage = String.Empty;
        public static string SessionLanguage = String.Empty;
        public static List<int> ListLanguageID;
        public static int DefaultLanguageID = 0;
        #endregion

        #region PARAM CultureInfo
        public static CultureInfo info;
        public static CultureInfo infoEN;
        public static DateTime DateDefaut = new DateTime(1900, 1, 1);
        public static string DateSQLPattern = String.Empty;
        public static string DateReportPattern = String.Empty;
        private static char DOT = '.'; // Dot Seperator
        public static string ShortDateFormat;
        public static string LongDateFormat;
        #endregion


        #region PARAM WEB
        public static string Connect = String.Empty;
        public static string logPath;
        public static int pageSizeDefault = 10;
        #endregion

        #region SqlInjection
        //Source: http://forums.asp.net/t/1254125.aspx
        //Defines the set of characters that will be checked.
        //You can add to this list, or remove items from this list, as appropriate for your site
        public static string[] blackList = {"--",";--",";","/*","*/","@@","@",
                                         "char","nchar","varchar","nvarchar",
                                         "alter","begin","cast","create","cursor","declare","delete","drop","end","exec","execute",
                                         "fetch","insert","kill","open",
                                         "select", "sys","sysobjects","syscolumns",
                                         "table","update"};

        //The utility method that performs the blacklist comparisons
        //You can change the error handling, and error redirect location to whatever makes sense for your site.
        public static bool SafeWithSqlInjection(string parameter)
        {
            CompareInfo comparer = CultureInfo.InvariantCulture.CompareInfo;

            for (int i = 0; i < blackList.Length; i++)
            {
                if (comparer.IndexOf(parameter, blackList[i], CompareOptions.IgnoreCase) >= 0)
                {
                    //
                    //Handle the discovery of suspicious Sql characters here
                    //
                    //Response.Redirect("~/Error.aspx");  //generic error page on your site
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Phrase
        public static void InsertPhrase(string phraseCode)
        {
            try
            {
                phraseCode = phraseCode ?? string.Empty;
                phraseCode = phraseCode.Trim();
                if (phraseCode == String.Empty || phraseCode.Contains(" "))
                {
                    return;
                }

                ERouteDataContext db = new ERouteDataContext(Utility.Connect);
                List<Phrase> phraseList = db.Phrases.Where(p => p.PhraseCode == phraseCode).ToList();

                if (ListLanguageID != null && phraseList.Count != ListLanguageID.Count)
                {
                    int langCount = ListLanguageID.Count;
                    if (phraseCode.Contains("SystemConfig"))
                    {
                        Phrase p = new Phrase();
                        p.PhraseCode = phraseCode;
                        p.PhraseText = ConvertCodeToText(phraseCode);
                        p.LanguageID = DefaultLanguageID;

                        if (db.Phrases.Where(a => a.PhraseCode == phraseCode && a.LanguageID == DefaultLanguageID).FirstOrDefault() == null)
                        {
                            db.Phrases.InsertOnSubmit(p);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < langCount; i++)
                        {
                            Phrase p = new Phrase();
                            p.PhraseCode = phraseCode;
                            p.PhraseText = ConvertCodeToText(phraseCode);
                            p.LanguageID = ListLanguageID[i];
                            if (db.Phrases.Where(a => a.PhraseCode == phraseCode && a.LanguageID == ListLanguageID[i]).FirstOrDefault() == null)
                            {
                                db.Phrases.InsertOnSubmit(p);
                            }
                        }
                    }
                    db.SubmitChanges();
                }
            }
            catch
            {

            }
        }

        public static string Phrase(string phraseCode)
        {
            try
            {
                if (phraseCode == null)
                {
                    return String.Empty;
                }
                if (Dictionaries == null)
                {
                    return phraseCode;
                }

                SessionLanguage = StringParse(Utils.CurrentLanguage);
                return Dictionaries[SessionLanguage][phraseCode];
            }
            catch
            {
                InsertPhrase(phraseCode.ToString());
                return phraseCode;
            }
        }

        public static string Phrase(string phraseCode, string languageCode)
        {
            try
            {
                if (phraseCode == null)
                {
                    return String.Empty;
                }
                if (Dictionaries == null)
                {
                    return phraseCode;
                }
                SessionLanguage = StringParse(Utils.CurrentLanguage);
                return Dictionaries[SessionLanguage][phraseCode];
            }
            catch
            {
                InsertPhrase(phraseCode.ToString());
                return phraseCode;
            }
        }

        public static string Phrase(object objPhraseCode)
        {
            try
            {
                if (objPhraseCode != null)
                {
                    string phraseCode = objPhraseCode.ToString();
                    if (phraseCode == null)
                    {
                        return String.Empty;
                    }
                    if (Dictionaries == null)
                    {
                        return phraseCode;
                    }

                    SessionLanguage = StringParse(Utils.CurrentLanguage);

                    if (string.IsNullOrEmpty(SessionLanguage))
                        SessionLanguage = DefaultLanguage;
                    return Dictionaries[SessionLanguage][phraseCode];
                }
                return string.Empty;
            }
            catch
            {
                InsertPhrase(objPhraseCode.ToString());
                return string.Empty;
            }
        }

        public static string PhraseRef(string refGroup, string phraseCode)
        {
            try
            {
                if (phraseCode == null)
                {
                    return String.Empty;
                }
                if (Dictionaries == null)
                {
                    return phraseCode;
                }

                phraseCode = refGroup + "_" + phraseCode;
                SessionLanguage = StringParse(Utils.CurrentLanguage);

                if (phraseCode.Contains("SystemConfig"))
                {
                    return Dictionaries[DefaultLanguage][phraseCode];
                }
                else
                {
                    return Dictionaries[SessionLanguage][phraseCode];
                }
            }
            catch
            {
                InsertPhrase(phraseCode.ToString());
                return phraseCode;
            }
        }

        // Convert: "Success_CreateOrganization" -> "Success Create Organization"
        public static string ConvertCodeToText(string code)
        {
            string temp = String.Empty;
            if (!string.IsNullOrEmpty(code))
            {
                code = code.Replace("LBL_", "").Replace("BLM_SUC_", "Success").Replace("BLM_ERR_", "Error").Replace("GUI_VAL_", "").Replace("GUI_CFM_", "");

                // Replace UpperCase "A" to " A"
                temp = Regex.Replace(code, @"([A-Z])", " $1");
                //Replace "_" to " "
                //Replace "  " to " "
                //Then TRIM()
                temp = temp.Replace("_", " ").Replace("  ", " ").Trim();
                return temp;
            }
            return String.Empty;
        }

        #endregion

        #region ReadNumberToVietnamese
        private static readonly string[] VietnameseSigns = new string[] { "aAeEoOuUiIdDyY",
        "áàạảãâấầậẩẫăắằặẳẵ",
        "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
        "éèẹẻẽêếềệểễ",
        "ÉÈẸẺẼÊẾỀỆỂỄ",
        "óòọỏõôốồộổỗơớờợởỡ",
        "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
        "úùụủũưứừựửữ",
        "ÚÙỤỦŨƯỨỪỰỬỮ",
        "íìịỉĩ",
        "ÍÌỊỈĨ",
        "đ",
        "Đ",
        "ýỳỵỷỹ",
        "ÝỲỴỶỸ" };


        public static string RemoveSign4VietnameseString(string str)
        {
            try
            {
                //Tiến hành thay thế , lọc bỏ dấu cho chuỗi
                for (int i = 1; i < VietnameseSigns.Length; i++)
                {
                    for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    {
                        str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
                    }
                }
                return str;
            }
            catch
            {
                return str;
            }
        }

        public static string ReadDecimalToVietnamese(decimal? de)
        {
            if (!de.HasValue)
            {
                return String.Empty;
            }
            decimal value = Round(de.Value, 0);
            if (value > 0)
                return (ConvertNumberToVietnamese(value.ToString().Replace(".", "")).Replace("  ", " ").Replace(",", "") + " ").Trim().ToUpper();
            else
                return string.Empty;
        }

        public static string UppercaseFirst(string s)
        {
            s = s.Substring(0, 1).ToUpper() + s.Substring(1, s.Length - 1);
            return s;
        }

        public static bool money = true;// True: không trăm lẻ ba, FALSE: ba

        public static string ConvertNumberToVietnamese(string Number)
        {
            if (Number.Trim().Length > 0 && Number.Trim().Length <= 12)
            {
                return ConvertTwelveNumberToVietnamese(Number, 0);
            }
            else
            {
                //Step by step cut 9 digit of the last and convert them
                string total = "";
                string FollowNumber = Number;
                int i = 0;
                string temp;
                while (FollowNumber.Length > 0)
                {
                    if (FollowNumber.Length >= 9)
                    {
                        //Cut 9 digit of the last and convert them here
                        temp = FollowNumber.Substring(FollowNumber.Length - 9, 9);
                        total = total.Insert(0, ConvertTwelveNumberToVietnamese(temp, i) + ", ");
                        //Remove 9 digit of the last from Number
                        FollowNumber = FollowNumber.Remove(FollowNumber.Length - 9);
                    }
                    else
                    {
                        temp = FollowNumber;
                        total = total.Insert(0, ConvertTwelveNumberToVietnamese(temp, i) + ", ");
                        FollowNumber = "";
                    }
                    i++;
                }
                // Makeup total in special case
                total = total.Replace(", ,", ",");
                if (total.IndexOf(",", total.Length - 2) > 0)
                    total = total.Remove(total.Length - 2);
                return total;
            }
        }

        public static string ConvertTwelveNumberToVietnamese(string Number, Int32 intbillions)
        {
            if (Number.Trim().Length > 0 && Convert.ToInt64(Number) > 0)
            {
                string strbillions = "";
                for (Int32 i = 0; i < intbillions; i++)
                {
                    strbillions += " tỉ";
                }

                //representing hundreds
                if (Convert.ToInt64(Number).ToString().Length <= 3)
                {
                    return ConvertThreeNumberToVietnamese(Number) + strbillions;
                }
                //representing thousands
                else
                    if (Convert.ToInt64(Number).ToString().Length <= 6 && Convert.ToInt64(Number).ToString().Length > 3)
                    {
                        //pattern goes here
                        string subResult1;
                        string subResult2;
                        string Number1 = Number.Substring(0, Number.Length - 3);//thousands
                        string Number2 = Number.Substring(Number1.Length, 3);//tens
                        subResult1 = ConvertThreeNumberToVietnamese(Number1);
                        subResult2 = ConvertThreeNumberToVietnamese(Number2);
                        //if the this is xxx,000
                        if (Convert.ToInt32(Number2) == 0)
                        {
                            return subResult1 + " " + "nghìn" + strbillions;
                        }
                        //if the this is xxx,xxx
                        else
                        {
                            return subResult1 + " " + "nghìn" + strbillions + ", " + subResult2 + strbillions;
                        }
                    }
                    //representing millions
                    else
                        if (Convert.ToInt64(Number).ToString().Length <= 9 && Convert.ToInt64(Number).ToString().Length > 6)
                        {
                            string subResult1;
                            string subResult2;
                            string subResult3;
                            string Number1 = Number.Substring(0, Number.Length - 6);//millions
                            string Number2 = Number.Substring(Number1.Length, 3);//thousands
                            string Number3 = Number.Substring(Number1.Length + 3, 3);//tens
                            subResult1 = ConvertThreeNumberToVietnamese(Number1);
                            subResult2 = ConvertThreeNumberToVietnamese(Number2);
                            subResult3 = ConvertThreeNumberToVietnamese(Number3);
                            //if the this is xxx,000,000
                            if (Convert.ToInt32(Number3) == 0 && Convert.ToInt32(Number2) == 0)
                            {
                                return subResult1 + " " + "triệu" + strbillions;
                            }
                            //if the this is xxx,xxx,000
                            else
                                if (Convert.ToInt32(Number3) == 0)
                                {
                                    return subResult1 + " " + "triệu" + strbillions + ", " + subResult2 + " " + "nghìn" + strbillions;
                                }
                                //if the this is xxx,000,xxx
                                else
                                    if (Convert.ToInt32(Number2) == 0)
                                    {
                                        return subResult1 + " " + "triệu" + strbillions + ", " + subResult3 + strbillions;
                                    }
                                    else
                                    {
                                        return subResult1 + " " + "triệu" + strbillions + ", " + subResult2 + " " + "nghìn" + strbillions + ", " + subResult3 + strbillions;
                                    }
                        }
                        //representing billions
                        else
                            if (Convert.ToInt64(Number).ToString().Length <= 12 && Convert.ToInt64(Number).ToString().Length > 9)
                            {
                                string subResult1;
                                string subResult2;
                                string subResult3;
                                string subResult4;
                                string Number1 = Number.Substring(0, Number.Length - 9);//billions
                                string Number2 = Number.Substring(Number1.Length, 3);//millions
                                string Number3 = Number.Substring(Number1.Length + 3, 3);//thousands
                                string Number4 = Number.Substring(Number1.Length + 6, 3);//tens
                                subResult1 = ConvertThreeNumberToVietnamese(Number1);
                                subResult2 = ConvertThreeNumberToVietnamese(Number2);
                                subResult3 = ConvertThreeNumberToVietnamese(Number3);
                                subResult4 = ConvertThreeNumberToVietnamese(Number4);
                                //if the this is xxx,000,000,000
                                if (Convert.ToInt32(Number2) == 0 && Convert.ToInt32(Number3) == 0 && Convert.ToInt32(Number4) == 0)
                                {
                                    return subResult1 + " " + "tỉ";
                                }
                                //if the this is xxx,xxx,000,000
                                else
                                    if (Convert.ToInt32(Number3) == 0 && Convert.ToInt32(Number4) == 0)
                                    {
                                        return subResult1 + " " + "tỉ" + ", " + subResult2 + " " + "triệu";
                                    }
                                    //if the this is xxx,000,000,xxx
                                    else
                                        if (Convert.ToInt32(Number2) == 0 && Convert.ToInt32(Number3) == 0)
                                        {
                                            return subResult1 + " " + "tỉ" + ", " + subResult4;
                                        }
                                        //if the this is xxx,xxx,xxx,000
                                        else
                                            if (Convert.ToInt32(Number4) == 0)
                                            {
                                                return subResult1 + " " + "tỉ" + ", " + subResult2 + " " + "triệu" + ", " + subResult3 + " " + "nghìn";
                                            }
                                            //if the this is xxx,xxx,000,xxx
                                            else
                                                if (Convert.ToInt32(Number3) == 0)
                                                {
                                                    return subResult1 + " " + "tỉ" + ", " + subResult2 + " " + "triệu" + ", " + subResult4;
                                                }
                                                //if the this is xxx,000,xxx,xxx
                                                else
                                                    if (Convert.ToInt32(Number2) == 0)
                                                    {
                                                        return subResult1 + " " + "tỉ" + ", " + subResult3 + " " + "nghìn" + ", " + subResult4;
                                                    }
                                                    else
                                                    {
                                                        return subResult1 + " " + "tỉ" + ", " + subResult2 + " " + "triệu" + ", " + subResult3 + " " + "nghìn" + ", " + subResult4;
                                                    }
                            }
                            else
                                return "";
            }
            else
                return "";
        }

        public static string ConvertThreeNumberToVietnamese(string strNumber)
        {
            if (strNumber.ToString().Length <= 2 || Convert.ToInt32(strNumber) == 0)
                money = false;
            else
                money = true;

            uint Number = Convert.ToUInt32(strNumber);
            string strhh, strh1, strh2, strh3;
            string str = "";
            uint h1 = Number / 100; //trăm
            uint h2 = Number % 100;
            uint h3 = h2 / 10; //mười
            uint h4 = h2 % 10; //unit

            switch (h1)
            {
                case 0:
                    if (money == true)
                        strh3 = "không trăm";
                    else
                        strh3 = "";
                    break;
                case 1:
                    strh3 = "một trăm";
                    break;
                case 2:
                    strh3 = "hai trăm";
                    break;
                case 3:
                    strh3 = "ba trăm";
                    break;
                case 4:
                    strh3 = "bốn trăm";
                    break;
                case 5:
                    strh3 = "năm trăm";
                    break;
                case 6:
                    strh3 = "sáu trăm";
                    break;
                case 7:
                    strh3 = "bảy trăm";
                    break;
                case 8:
                    strh3 = "tám trăm";
                    break;
                case 9:
                    strh3 = "chín trăm";
                    break;
                default:
                    strh3 = "";
                    break;
            }
            switch (h3)
            {
                case 0:
                    if (h1 != 0 && h4 != 0 || (money == true && h4 != 0)) //nếu có hàng trăm
                        strh2 = "lẻ";
                    else
                        strh2 = "";
                    break;
                case 1:
                    strh2 = "mười";
                    break;
                case 2:
                    strh2 = "hai mươi";
                    break;
                case 3:
                    strh2 = "ba mươi";
                    break;
                case 4:
                    strh2 = "bốn mươi";
                    break;
                case 5:
                    strh2 = "năm mươi";
                    break;
                case 6:
                    strh2 = "sáu mươi";
                    break;
                case 7:
                    strh2 = "bảy mươi";
                    break;
                case 8:
                    strh2 = "tám mươi";
                    break;
                case 9:
                    strh2 = "chín mươi";
                    break;
                default:
                    strh2 = "";
                    break;
            }

            switch (h4)
            {
                case 1:
                    if (h3 != 0)
                        strh1 = "mốt";
                    else
                        strh1 = "một";
                    break;
                case 2:
                    strh1 = "hai";
                    break;
                case 3:
                    strh1 = "ba";
                    break;
                case 4:
                    if (h3 != 0)
                        strh1 = "bốn";
                    else
                        strh1 = "bốn";
                    break;
                case 5:
                    if (h3 != 0)
                        strh1 = "lăm";
                    else
                        strh1 = "năm";
                    break;
                case 6:
                    strh1 = "sáu";
                    break;
                case 7:
                    strh1 = "bảy";
                    break;
                case 8:
                    strh1 = "tám";
                    break;
                case 9:
                    strh1 = "chín";
                    break;
                default:
                    if (h3 == 0 && h1 == 0)
                        strh1 = "không";
                    else
                        strh1 = "";
                    break;
            }

            //Eleven - Twelve - ... - Nineteen
            if (strh2 == "mười" && strh1 == "mốt")
            {
                strh1 = "";
                strh2 = "mười một";
            }
            else
                if (strh2 == "mười" && strh1 == "hai")
                {
                    strh1 = "";
                    strh2 = "mười hai";
                }
                else
                    if (strh2 == "mười" && strh1 == "ba")
                    {
                        strh1 = "";
                        strh2 = "mười ba";
                    }
                    else
                        if (strh2 == "mười" && strh1 == "bốn")
                        {
                            strh1 = "";
                            strh2 = "mười bốn";
                        }
                        else
                            if (strh2 == "mười" && strh1 == "năm")
                            {
                                strh1 = "";
                                strh2 = "mười lăm";
                            }
                            else
                                if (strh2 == "mười" && strh1 == "sáu")
                                {
                                    strh1 = "";
                                    strh2 = "mười sáu";
                                }
                                else
                                    if (strh2 == "mười" && strh1 == "bảy")
                                    {
                                        strh1 = "";
                                        strh2 = "mười bảy";
                                    }
                                    else
                                        if (strh2 == "mười" && strh1 == "tám")
                                        {
                                            strh1 = "";
                                            strh2 = "mười tám";
                                        }
                                        else
                                            if (strh2 == "mười" && strh1 == "chín")
                                            {
                                                strh1 = "";
                                                strh2 = "mười chín";
                                            }

            //special cases
            if (str.Length == 1)
            {
                strhh = strh1;
            }
            else
                if (strh1 == "không" && str.Length > 1)
                {
                    strhh = strh3 + " " + strh2;
                }
                else
                    if (strh2 == "")
                    {
                        strhh = strh3 + " " + strh1;
                    }
                    else
                        if (str.Length == 2)
                        {
                            strhh = strh3 + " " + strh2 + " " + strh1;
                        }
                        else
                        {
                            strhh = strh3 + " " + strh2 + " " + strh1;
                        }
            return strhh;
        }
        #endregion

        #region Special Char

        public static string FormatSpecialChar(string Str)
        {
            Str = Str.Replace("\\", "\\\\");
            Str = Str.Replace("'", "\\'");
            Str = Str.Replace(@"""", "&quot;");
            return Str;
        }

        public static string FormatSQLSpecial(string Str)
        {
            if (!string.IsNullOrEmpty(Str))
            {
                Str = Str.Trim();
                Str = Str.Replace("'", "''");
                Str = Str.Replace("--", "");
            }
            return Str;
        }


        public static string DecodeVietnamese(string text)
        {
            string[] pattern = new string[7];
            pattern[0] = "a|(á|ả|à|ạ|ã|ă|ắ|ẳ|ằ|ặ|ẵ|â|ấ|ẩ|ầ|ậ|ẫ)";
            pattern[1] = "o|(ó|ỏ|ò|ọ|õ|ô|ố|ổ|ồ|ộ|ỗ|ơ|ớ|ở|ờ|ợ|ỡ)";
            pattern[2] = "e|(é|è|ẻ|ẹ|ẽ|ê|ế|ề|ể|ệ|ễ)";
            pattern[3] = "u|(ú|ù|ủ|ụ|ũ|ư|ứ|ừ|ử|ự|ữ)";
            pattern[4] = "i|(í|ì|ỉ|ị|ĩ)";
            pattern[5] = "y|(ý|ỳ|ỷ|ỵ|ỹ)";
            pattern[6] = "d|đ";
            for (int i = 0; i < pattern.Length; i++)
            {
                char replaceChar = pattern[i][0];
                System.Text.RegularExpressions.MatchCollection matchs = System.Text.RegularExpressions.Regex.Matches(text, pattern[i]);
                foreach (System.Text.RegularExpressions.Match m in matchs)
                {
                    text = text.Replace(m.Value[0], replaceChar);
                }
            }
            text = text.Replace(" ", "_");
            return text;
        }

        public static string UrlToString(string text)
        {
            text = text.Trim();
            text = text.Replace(":", "");
            text = text.Replace("/", "-");
            text = text.Replace("\\", "-");
            text = text.Replace("?", "");
            text = text.Replace(" ", "-");
            return text;
        }
        #endregion

        #region Convert

        #region Bool & BoolN Parse

        public static bool BoolParse(int i)
        {
            return (i == 1);
        }

        public static bool BoolParse(int? i)
        {
            return (i.HasValue && i == 1);
        }

        public static bool BoolParse(object o)
        {
            if (o == null)
                return false;
            bool temp = false;
            if (bool.TryParse(o.ToString(), out temp))
                return temp;

            return false;
        }


        #endregion

        #region Int & IntN Parse

        public static int IntParse(bool b)
        {
            return b ? 1 : 0;
        }

        public static int IntParse(bool? b)
        {
            return (b.HasValue && b.Value) ? 1 : 0;
        }

        public static int IntParse(string str)
        {
            int i = 0;
            if (int.TryParse(str, out i))
            {
            }
            return i;
        }

        public static int IntParse(object o)
        {
            if (o == null)
            {
                return 0;
            }
            int i = 0;
            if (int.TryParse(o.ToString(), out i))
            {
            }
            return i;
        }

        public static int? IntNParse(string str)
        {
            int i = 0;
            if (int.TryParse(str, out i))
            {
                return i;
            }
            return null;
        }

        public static int? IntNParse(object o)
        {
            if (o == null)
            {
                return null;
            }
            int i = 0;
            if (int.TryParse(o.ToString(), out i))
            {
                return i;
            }
            return null;
        }

        #endregion

        #region Decimal & DecimalN Parse
        public static decimal DecimalParseFromHour(decimal hour, decimal minute)
        {
            decimal de = 0M;
            de = hour + minute / 60;
            return de;
        }

        public static decimal DecimalParse(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Trim();
            }

            decimal de = 0M;
            if (decimal.TryParse(str, NumberStyles.Any, info, out de))
            {
            }
            return de;
        }

        public static decimal DecimalParseVN(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Trim();
                str = str.Replace(",", "");
            }

            decimal de = 0M;
            if (decimal.TryParse(str, NumberStyles.Any, info, out de))
            {
            }
            return de;
        }

        public static decimal DecimalParse(object o)
        {
            if (o == null)
            {
                return 0;
            }

            decimal de = 0M;
            if (decimal.TryParse(o.ToString(), NumberStyles.Any, info, out de))
            {
            }
            return de;
        }

        public static decimal? DecimalNParse(string str)
        {
            decimal de = 0M;
            if (decimal.TryParse(str, NumberStyles.Any, info, out de))
            {
                return de;
            }
            return null;
        }

        public static decimal? DecimalNParse(object o)
        {
            if (o == null)
            {
                return null;
            }
            decimal de = 0;
            if (decimal.TryParse(o.ToString(), NumberStyles.Number, info, out de))
            {
                return de;
            }
            return null;
        }
        public static decimal DecimalDParse(object o)
        {
            if (o == null)
            {
                return 0;
            }
            decimal de = 0;
            if (decimal.TryParse(o.ToString(), NumberStyles.Number, info, out de))
            {
            }
            return de;
        }
        #endregion

        #region String & StringN Parse

        public static string StringParse(string input)
        {
            input = input ?? string.Empty;
            input = input == "null" ? string.Empty : input;
            input = input.Trim();
            string pattern = "\\s+";
            string replacement = " ";
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(input, replacement);
            return input;
        }

        public static string StringParse(decimal de)
        {
            return String.Format(info, "{0:#}", de);
        }

        public static string StringParse(decimal? de)
        {
            if (!de.HasValue)
            {
                return "0";
            }
            return StringParse(de.Value);
        }

        public static string StringParse(int[] ID)
        {
            if (ID == null)
            {
                return "0";
            }
            else
            {
                string result = String.Empty;
                for (int i = 0; i < ID.Length; i++)
                {
                    result = result + ID[i] + ",";
                }

                if (result.Length > 0)
                    return result.Remove(result.Length - 1);
                return "0";
            }
        }

        public static string StringParse(List<int> ID)
        {
            if (ID == null)
            {
                return "0";
            }
            else
            {
                string result = String.Empty;
                for (int i = 0; i < ID.Count; i++)
                {
                    result = result + ID[i] + ",";
                }

                if (result.Length > 0)
                    return result.Remove(result.Length - 1);
                return "0";
            }
        }

        public static string StringParseToSQLQuery(List<string> ID)
        {
            if (ID == null)
            {
                return "''";
            }
            else
            {
                string result = "'";
                for (int i = 0; i < ID.Count; i++)
                {
                    result = result + ID[i] + "','";
                }
                if (result.Length > 0)
                    return result.Remove(result.Length - 2);
                return "";
            }
        }

        public static string StringParse(List<string> ID)
        {
            if (ID == null)
            {
                return "";
            }
            else
            {
                string result = String.Empty;
                for (int i = 0; i < ID.Count; i++)
                {
                    result = result + ID[i] + ", ";
                }
                if (result.Length > 0)
                    return result.Remove(result.Length - 1);
                return "";
            }
        }

        public static string StringParse(byte[] b)
        {
            return BitConverter.ToString(b);
        }

        public static string StringParse(object o)
        {
            return o == null ? String.Empty : o.ToString();
        }

        public static string StringNParse(object o)
        {
            return o == null ? String.Empty : o.ToString();
        }

        public static string StringParse(bool? bo)
        {
            if (bo.HasValue)
            {
                if (bo.Value)
                    return "1";
            }

            return "0";
        }

        public static string StringNParse(bool? bo)
        {
            if (bo.HasValue)
            {
                if (bo.Value)
                    return "1";
            }

            return string.Empty;
        }

        public static string StringNParse(decimal? de)
        {
            if (!de.HasValue || de == 0M)
            {
                return String.Empty;
            }
            return StringParse(de.Value);
        }

        public static string StringParseWithDecimalDegit(decimal de)
        {
            info.NumberFormat.NumberDecimalDigits = 2;
            info.NumberFormat.CurrencyDecimalDigits = 2;
            string result = String.Format(info, "{0:C}", de);
            //info.NumberFormat.NumberDecimalDigits = 0;
            //info.NumberFormat.CurrencyDecimalDigits = 0;
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Trim();
            }
            return result;
        }

        public static string StringParseWithDecimalDegit(double? de)
        {
            info.NumberFormat.NumberDecimalDigits = 2;
            info.NumberFormat.CurrencyDecimalDigits = 2;
            string result = String.Format(info, "{0:C}", de);
            //info.NumberFormat.NumberDecimalDigits = 0;
            //info.NumberFormat.CurrencyDecimalDigits = 0;
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Trim();
            }
            return result;
        }

        public static string StringParseWithObject(object de)
        {
            string result = "0";
            try
            {
                result = de.ToString();
            }
            catch
            {
                result = "0";
            }
            return result;
        }
        public static decimal DecimalParseWithObject(object de)
        {
            decimal result = 0;
            try
            {
                result = Convert.ToDecimal(de.ToString());
            }
            catch
            {
                result = 0;
            }
            return result;
        }
        public static string StringParseWithDecimalDegitEN(decimal de)
        {
            //info.NumberFormat.NumberDecimalDigits = 2;
            //info.NumberFormat.CurrencyDecimalDigits = 2;
            string result = String.Format(infoEN, "{0:C}", de);
            //info.NumberFormat.NumberDecimalDigits = 0;
            //info.NumberFormat.CurrencyDecimalDigits = 0;
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Trim();
            }
            if (result == "0")
            {
                result = string.Empty;
            }

            return result;
        }

        public static string StringParseWithDecimalDegit(decimal? de)
        {
            if (!de.HasValue)
            {
                return "0.00";
            }
            return StringParseWithDecimalDegit(de.Value);
        }

        public static string StringNParseWithDecimalDegit(decimal? de)
        {
            if (!de.HasValue || de == 0M)
            {
                return String.Empty;
            }
            return StringParseWithDecimalDegit(de.Value);
        }

        public static string StringParseWithAutoDecimalDegit(decimal? de)
        {
            char groupSer = char.Parse(Utility.info.NumberFormat.NumberGroupSeparator);
            if (!de.HasValue || de == 0M)
            {
                return "0";
            }
            string[] str = de.Value.ToString().Split(groupSer);
            str[1] = str[1].Replace("00", String.Empty);
            if (str[1].EndsWith("0"))
            {
                str[1] = str[1].Remove(str[1].Length - 1);
            }
            info.NumberFormat.NumberDecimalDigits = 2;// str[1].Length;
            info.NumberFormat.CurrencyDecimalDigits = 2;// str[1].Length;

            string result = String.Format(info, "{0:C}", de);
            info.NumberFormat.NumberDecimalDigits = 0;
            info.NumberFormat.CurrencyDecimalDigits = 0;
            return result;
        }

        public static string StringNParseWithAutoDecimalDegit(decimal? de)
        {
            char groupSer = char.Parse(Utility.info.NumberFormat.NumberGroupSeparator);
            if (!de.HasValue || de == 0M)
            {
                return String.Empty;
            }
            string[] str = de.Value.ToString().Split(groupSer);
            str[1] = str[1].Replace("00", String.Empty);
            if (str[1].EndsWith("0"))
            {
                str[1] = str[1].Remove(str[1].Length - 1);
            }
            info.NumberFormat.NumberDecimalDigits = 2;// str[1].Length;
            info.NumberFormat.CurrencyDecimalDigits = 2;// str[1].Length;

            string result = String.Format(info, "{0:C}", de);
            info.NumberFormat.NumberDecimalDigits = 0;
            info.NumberFormat.CurrencyDecimalDigits = 0;
            return result;
        }

        public static string StringParseWithRoundingDecimalDegit(decimal de)
        {
            //return String.Format(info, "{0:#}", de);
            info.NumberFormat.NumberDecimalDigits = 0;
            info.NumberFormat.CurrencyDecimalDigits = 0;
            string result = String.Format(info, "{0:C}", de);
            return result;
        }

        public static string StringParseWithRoundingDecimalDegit(decimal? de)
        {
            if (!de.HasValue)
            {
                return "0";
            }
            return StringParseWithRoundingDecimalDegit(de.Value);
        }

        public static string StringNParseWithRoundingDecimalDegit(decimal? de)
        {
            if (!de.HasValue || de == 0M)
            {
                return String.Empty;
            }
            return StringParseWithRoundingDecimalDegit(de.Value);
        }
        #endregion

        #region Byte Parse

        public static byte[] ByteParse(string hexStr)
        {
            int numberChars = hexStr.Length;
            byte[] b = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                b[i / 2] = Convert.ToByte(hexStr.Substring(i, 2), 16);
            return b;
        }

        #endregion

        #region DateTime Parse

        public static DateTime DateTimeParse(object o)
        {
            if (o == null)
            {
                return DateTime.MinValue;
            }
            DateTime dt = new DateTime();
            if (DateTime.TryParse(o.ToString(), info, DateTimeStyles.None, out dt))
            {
                return dt;
            }
            return DateTime.MinValue;
        }

        public static DateTime? DateTimeNParse(object o)
        {
            if (o == null)
            {
                return null;
            }
            DateTime dt = new DateTime();
            if (DateTime.TryParse(o.ToString(), info, DateTimeStyles.None, out dt))
            {
                return dt;
            }
            return null;
        }

        public static void DateTimeSecondToHourAndMinute(decimal second, out decimal hour, out decimal minute)
        {
            try
            {
                TimeSpan t = TimeSpan.FromSeconds((double)second);
                hour = t.Hours;
                minute = t.Minutes;
            }
            catch
            {
                hour = 0;
                minute = 0;
            }
        }

        public static void DateTimeMinuteToHourAndMinute(decimal numberOfMinute, out decimal hour, out decimal minute)
        {
            try
            {
                TimeSpan t = TimeSpan.FromMinutes((double)numberOfMinute);
                hour = t.Hours;
                minute = t.Minutes;
            }
            catch
            {
                hour = 0;
                minute = 0;
            }
        }

        public static int DateTimeMinuteToHour(decimal minute)
        {
            try
            {
                TimeSpan t = TimeSpan.FromMinutes((double)minute);
                return t.Hours;
            }
            catch
            {
                return 0;
            }
        }

        public static decimal DateTimeMinuteToMinute(decimal minute)
        {
            try
            {
                TimeSpan t = TimeSpan.FromMinutes((double)minute);
                return t.Minutes;
            }
            catch
            {
                return 0;
            }
        }

        public static int DateTimeSecondToHour(decimal second)
        {
            try
            {
                TimeSpan t = TimeSpan.FromSeconds((double)second);
                return t.Hours;
            }
            catch
            {
                return 0;
            }
        }

        public static decimal DateTimeSecondToMinute(decimal second)
        {
            try
            {
                TimeSpan t = TimeSpan.FromSeconds((double)second);
                return t.Minutes;
            }
            catch
            {
                return 0;
            }
        }

        public static decimal DateTimeTimeToSecond(decimal hour, decimal minute)
        {
            decimal de = 0M;
            de = (hour * 3600) + (minute * 60);
            return de;
        }

        public static decimal DateTimeTimeToMinute(decimal hour, decimal minute)
        {
            decimal de = 0M;
            de = (hour * 60) + minute;
            return de;
        }

        #endregion

        #region Image
        //public static byte[] ImageToByte(Image image)
        //{
        //    if (image == null)
        //        return null;
        //    MemoryStream ms = new MemoryStream();
        //    image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
        //    return ms.ToArray();
        //}

        //public static Image ByteToImage(byte[] b)
        //{
        //    if (b == null)
        //        return null;

        //    MemoryStream ms = new MemoryStream(b);
        //    Image returnImage = Image.FromStream(ms);
        //    return returnImage;
        //}
        #endregion

        public static string StringParseFromDateSQL(object o)
        {
            string input = StringParse(o);

            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            else
            {
                DateTime dt = DateTime.Parse(input);
                return dt.ToShortPattern();
            }
        }

        public static decimal Round(decimal de, int degit)
        {
            if (degit >= 0)
            {
                return Math.Round(de, degit);
            }
            else
            {
                de = Math.Round(de, 0);
                decimal tmp = de % (decimal)Math.Pow(10, Math.Abs(degit));
                de -= tmp;
                if (tmp >= 5 * (decimal)Math.Pow(10, Math.Abs(degit + 1)))
                {
                    de += (decimal)Math.Pow(10, Math.Abs(degit));
                }

                return de;
            }
        }

        public static string LimitString(string str, int limit)
        {
            if (str.Length > limit)
                return str.Substring(0, limit) + "...";
            else
                return str;
        }

        public static string ConvertListIDToString(int[] ID)
        {
            try
            {
                if (ID == null)
                    return "0";
                else
                {
                    string result = "";
                    for (int i = 0; i < ID.Length; i++)
                    {
                        if (i < ID.Length - 1)
                        {
                            result = result + ID[i].ToString() + ",";
                        }
                        else
                        {
                            result = result + ID[i].ToString();
                        }
                    }
                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        public static string GetNextCode(string preFix, string lastCode, string subFix)
        {
            string nextcode = string.Empty;

            if (!string.IsNullOrEmpty(preFix))
            {
                lastCode = lastCode.Replace(preFix, "");
            }
            if (!string.IsNullOrEmpty(subFix))
            {
                lastCode = lastCode.Replace(subFix, "");
            }

            int numberLength = lastCode.Length;
            string format = string.Empty;
            for (int i = 1; i <= numberLength; i++)
            {
                format += "0";
            }

            int nextNumber = IntParse(lastCode) + 1;
            nextcode = nextNumber.ToString(format);
            nextcode = preFix + nextcode + subFix;

            return nextcode;
        }
        #endregion

        #region LogEx
        public static void LogEx(string ExCode, Exception ex)
        {
            string Str = "----------------header------------------------\r\n";
            Str = Str + "Timestamp: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            Str = Str + "\r\nExceptionCode: " + ExCode;
            Str = Str + "\r\nMessage: " + ex.Message;
            Str = Str + "\r\nStackTrace:\r\n" + ex.StackTrace;
            Str = Str + "\r\nSource: " + ex.Source;
            Str = Str + "\r\n\r\n";
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(Str);
            string filePath = Utility.LogPath + "Log.txt";
            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.Write(builder.ToString());
                writer.Flush();
            }

        }

        public static void LogEx(string ExCode, string ex)
        {
            string logFile = System.Configuration.ConfigurationSettings.AppSettings["LogFile"];
            logFile = string.IsNullOrEmpty(logFile) ? "Log.txt" : logFile;

            string FileName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Logs\" + logFile;
            StreamWriter f = File.AppendText(FileName);
            string Str = "----------------header------------------------\r\n";
            Str = Str + "Timestamp: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            Str = Str + "\r\n" + ExCode + ": " + ex;
            Str = Str + "\r\n\r\n";

            f.Write(Str);
            f.Close();
        }

        public static void LogEx(string filename, string ExCode, string ex)
        {
            string logFile = string.IsNullOrEmpty(filename) ? "Log.txt" : filename + ".txt";


            string FileName = System.AppDomain.CurrentDomain.BaseDirectory + @"\Logs\" + logFile;
            StreamWriter f = File.AppendText(FileName);
            string Str = "----------------header------------------------\r\n";
            Str = Str + "Timestamp: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            Str = Str + "\r\n" + ExCode + ": " + ex;
            Str = Str + "\r\n\r\n";

            f.Write(Str);
            f.Close();
        }
        #endregion

        #region SendMail
        public static bool SendMail(string subject, string fromEmail, string fromHoten, string noidung, string toEmail)
        {
            //System.Web.Mail.SmtpMail.SmtpServer is obsolete in 2.0
            //System.Net.Mail.SmtpClient is the alternate class for this in 2.0

            System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient();
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();

            try
            {
                System.Net.Mail.MailAddress fromAddress = new System.Net.Mail.MailAddress(fromEmail, fromHoten);

                // SMTP info
                smtpClient.Host = "localhost"; // Default in IIS will be localhost & Default port will be 25
                smtpClient.Port = 25;

                //From address will be given as a MailAddress Object
                message.From = fromAddress;

                // To address collection of MailAddress
                message.To.Add(toEmail);

                // Message subject
                message.Subject = subject;
                message.SubjectEncoding = System.Text.Encoding.UTF8;

                // Message body content
                message.IsBodyHtml = true;
                message.Body = noidung;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Priority = System.Net.Mail.MailPriority.Normal;

                // Send SMTP mail
                Utility.LogEx("smtpClient.Send", fromEmail);
                Utility.LogEx("smtpClient.Send", toEmail);
                Utility.LogEx("smtpClient.Send", message.Subject);
                Utility.LogEx("smtpClient.Send", message.Body);

                smtpClient.Send(message);

                return true;
            }
            catch (Exception ex)
            {
                Utility.LogEx("Exception", ex);
                Utility.LogEx("Exception", ex.InnerException);
                return false;
            }
        }

        public static void SendMail(string from, List<string> to, List<string> cc, string subject, string body)
        {
            MailMessage msgMail = new MailMessage();
            msgMail.From = new MailAddress(from);
            msgMail.Subject = subject;
            msgMail.Body = body;

            if (to.Count != 0)
            {
                foreach (string obj in to)
                {
                    msgMail.To.Add(obj);
                }
            }

            if (cc.Count != 0)
            {
                foreach (string obj in cc)
                {
                    msgMail.CC.Add(obj);
                }
            }
            SmtpClient Smtp = new SmtpClient();
            //Utility.LogEx("LBL_EmailError", new Exception("Begin Send Mail"));
            Smtp.Send(msgMail);
            //Utility.LogEx("LBL_EmailError", new Exception("End Send Mail"));

        }
        #endregion

        //#region Cookie

        #region Get Set Cookie Default
        public static void SetCookie(System.Web.Mvc.Controller controller, string cookieName, string value)
        {
            //for store the values in the cookies
            controller.Response.Cookies[cookieName].Value = value;
            //how much time cookies save on the user hard drive
            //tempController.Response.Cookies["MyName"].Expires = DateTime.Now.AddDays(10);
            // now this cookies save 10 days on the user hard drive.after 10 days this cookies expires.
        }

        public static string GetCookie(System.Web.Mvc.Controller controller, string cookieName)
        {
            string value = String.Empty;
            if (controller.Request.Cookies[cookieName] != null)
            {
                value = controller.Request.Cookies[cookieName].Value;
                return value;
            }
            else
                return "";
        }
        #endregion

        //public static void SetPageSizeCookie(System.Web.Mvc.Controller controller, string cookieName, string value)
        //{
        //    controller.Response.Cookies[cookieName].Value = value;
        //    controller.Response.Cookies[cookieName].Expires = DateTime.Now.AddDays(1);
        //}

        //public static int GetPageSizeCookie(System.Web.Mvc.Controller controller, string cookieName)
        //{
        //    string value = String.Empty;
        //    // Read Cookie
        //    if (controller.Request.Cookies[cookieName] != null)
        //    {
        //        value = controller.Request.Cookies[cookieName].Value;
        //        int temp = 0;
        //        if (int.TryParse(value, out temp))
        //            return temp;
        //        else
        //        {
        //            // If Cookie not a page Size, set cookie value to default value, return default value
        //            SetCookie(controller, cookieName, pageSizeDefault.ToString());
        //            return pageSizeDefault;
        //        }
        //    }
        //    // If Cookie is null, set cookie value to Default value
        //    else
        //    {
        //        SetCookie(controller, cookieName, pageSizeDefault.ToString());
        //        return pageSizeDefault;
        //    }
        //}

        //#endregion        

        #region ReflectorUtil
        public static T CreateInstance<T>()
        {
            T obj = Activator.CreateInstance<T>();
            return obj;
        }

        public static Object GetPropertyValue(Object obj, String propertyName)
        {
            if (obj == null)
                return null;
            Type type = obj.GetType();
            if (propertyName.IndexOf(DOT) < 0)
            {
                PropertyInfo pi = type.GetProperty(propertyName);
                return pi.GetValue(obj, null);
            }
            else
            {
                String[] strs = propertyName.Split(DOT);
                int len = strs.Length;
                Object obj2 = null;
                for (int i = 0; i < len; i++)
                {
                    obj2 = GetPropertyValue(obj, strs[i]);
                    obj = obj2;
                }
                return obj2;
            }
        }

        public static List<T> CopyList<T>(IEnumerable list1)
        {
            if (list1 == null)
                return new List<T>();

            List<T> list2 = new List<T>();
            Type preType = null;
            Type curType = null;
            PropertyInfo[] preProps = null;
            PropertyInfo[] curProps = null;
            PropertyInfo prop2 = null;
            Type type2 = typeof(T);
            T obj2;
            foreach (Object obj1 in list1)
            {
                curType = obj1.GetType();
                if (curType != preType)
                {
                    curProps = curType.GetProperties();
                }
                else
                {
                    curProps = preProps;
                }
                preType = curType;
                preProps = curProps;

                String propName = null; ;

                obj2 = CreateInstance<T>();
                foreach (PropertyInfo prop1 in preProps)
                {
                    propName = prop1.Name;
                    prop2 = type2.GetProperty(propName);
                    if (prop2 != null && prop2.CanWrite)
                    {
                        prop2.SetValue(obj2, GetPropertyValue(obj1, propName), null);
                    }
                }
                list2.Add(obj2);
            }
            return list2;
        }

        /// <summary>
        /// Dùng để Copy dối tượng lúc Cần Insert dữ liệu từ ViewModel to table in database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Copy<T>(Object source)
        {
            if (source == null)
                return default(T);

            T dest = CreateInstance<T>();
            PropertyInfo prop2;
            Type type1 = source.GetType();
            Type type2 = dest.GetType();
            if (type1 != type2)
            {
                PropertyInfo[] props1 = type1.GetProperties();
                String propName;
                foreach (PropertyInfo prop1 in props1)
                {
                    propName = prop1.Name;
                    prop2 = type2.GetProperty(propName);
                    if (prop2 != null && prop2.CanWrite)
                    {
                        prop2.SetValue(dest, GetPropertyValue(source, propName), null);
                    }
                }
            }
            else
            {
                PropertyInfo[] props1 = type1.GetProperties();
                foreach (PropertyInfo prop1 in props1)
                {
                    if (prop1.CanWrite && prop1.CanRead)
                    {
                        prop1.SetValue(dest, prop1.GetValue(source, null), null);
                    }
                }
            }
            return dest;
        }

        //public static string GetObjectName(Object input)
        //{
        //    Type t = input.GetType();
        //    PropertyInfo p = t.GetProperties();
        //    String propName = p.Name;
        //}

        #endregion //ReflectorUtil

        #region ValidateData
        public static string validateErrMes = String.Empty;

        #region Security
        public static string EncodeMd5(string str)
        {
            byte[] pass = Encoding.UTF8.GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            pass = md5.ComputeHash(pass);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in pass)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        public static string Encrypt(string toEncrypt, string key, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string toDecrypt, string key, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
        #endregion

        public static bool ValidatePhoneFax(char keyChar)
        {
            char[] acceptInput = { '+', '-', '(', ')', (char)8, ' ' };
            if ((Char.IsNumber(keyChar) || acceptInput.Contains(keyChar)))
                return false;
            return true;
        }

        public static bool IsFileUpload(string filename)
        {
            string str = System.IO.Path.GetExtension(filename).ToLower();
            string[] extension = new string[] { ".jpg", ".jpeg", ".bmp", ".gif", ".swf", ".flv", ".doc", ".xls", ".pdf", ".rar", ".zip", ".chm" };
            for (int i = 0; i < extension.Length; i++)
            {
                if (str == extension[i])
                    return true;
            }
            return false;
        }

        #region ValidateString
        public static bool ValidateStringWhiteSpace(string input, string fieldName)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                validateErrMes = "LBL_" + fieldName + "_WhiteSpaceError";
                result = false;
            }
            return result;
        }

        public static bool ValidateStringEmpty(string input, string fieldName)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (string.IsNullOrEmpty(input))
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                result = false;
            }
            return result;
        }

        public static bool ValidateStringLengthMin(string input, string fieldName, int minimumLength)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (minimumLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_MinError";
                    result = false;
                }

                input = input.Trim();
                if (input.Length < minimumLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MinError";
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringLengthMax(string input, string fieldName, int maxLength)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (maxLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_MaxError";
                    result = false;
                }

                input = input.Trim();
                if (input.Length > maxLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MaxError";
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringLengthRange(string input, string fieldName, int minimumLength, int maxLength)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (minimumLength > 0)
            {
                if (string.IsNullOrEmpty(input))
                {
                    validateErrMes = "LBL_" + fieldName + "_EmptyError";
                    result = false;
                }

                input = input.Trim();
                if (input.Length < minimumLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MinError";
                    result = false;
                }

                if (input.Length > maxLength)
                {
                    validateErrMes = "LBL_" + fieldName + "_MaxError";
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringContain(string input, string fieldName, string contain)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(contain))
            {
                if (!input.Contains(contain))
                {
                    validateErrMes = "LBL_" + fieldName + "_ContainError";
                    result = false;
                }
            }
            return result;
        }

        public static bool ValidateStringRegex(string input, string fieldName, string regex, RegexOptions option)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(regex))
            {
                // Here we call Regex.Match.
                Match match = Regex.Match(input, regex, option);

                // Here we check the Match instance.
                if (!match.Success)
                {
                    validateErrMes = "LBL_" + fieldName + "_RegexError";
                    result = false;
                }
            }
            return result;
        }
        #endregion

        #region ValidateDecimal
        public static bool ValidateDecimalGreatZero(decimal input, string fieldName)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input <= 0)
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalMin(decimal input, string fieldName, decimal minimum)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalMax(decimal input, string fieldName, decimal max)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                result = false;
            }
            return result;
        }

        public static bool ValidateDecimalRange(decimal input, string fieldName, decimal minimum, decimal max)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                result = false;
            }

            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                result = false;
            }
            return result;
        }
        #endregion

        #region ValidateInt
        public static bool ValidateIntGreatZero(int input, string fieldName)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input <= 0)
            {
                validateErrMes = "LBL_" + fieldName + "_EmptyError";
                result = false;
            }
            return result;
        }

        public static bool ValidateIntMin(int input, string fieldName, int minimum)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                result = false;
            }
            return result;
        }

        public static bool ValidateIntMax(int input, string fieldName, int max)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                result = false;
            }
            return result;
        }

        public static bool ValidateIntRange(int input, string fieldName, int minimum, int max)
        {
            bool result = true;
            validateErrMes = string.Empty;
            if (input < minimum)
            {
                validateErrMes = "LBL_" + fieldName + "_MinError";
                result = false;
            }

            if (input > max)
            {
                validateErrMes = "LBL_" + fieldName + "_MaxError";
                result = false;
            }
            return result;
        }
        #endregion


        #endregion

        public static string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public static string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;
            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }

        public static string FooMethod(string path)
        {
            var mappedPath = Constant.appFolder + path;
            return mappedPath;
        }
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwyxzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        static public bool URLExists(string url)
        {
            bool result = true;
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Timeout = 1200; // miliseconds
            webRequest.Method = "HEAD";
            try
            {
                webRequest.GetResponse();
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
    public class ObjParamSP
    {
        private string key;
        public object Value { get; set; }
        public string Key
        {
            get { return "@" + key; }
            set { key = value; }
        }
    }
    public class JsonMenu
    {
        public int id { get; set; }
        public List<JsonMenu> children { get; set; }
    }
    public class TabControlFeaturesDemoOptions
    {
        public TabControlFeaturesDemoOptions()
        {
            ActivateTabPageAction = ActivateTabPageAction.Click;
            EnableHotTrack = true;
            SaveStateToCookies = true;
            TabAlign = TabAlign.Left;
            TabPosition = TabPosition.Top;
        }

        public ActivateTabPageAction ActivateTabPageAction { get; set; }
        public bool EnableHotTrack { get; set; }
        public bool SaveStateToCookies { get; set; }
        public TabAlign TabAlign { get; set; }
        public TabPosition TabPosition { get; set; }
    }

    public class ViewControlCombobox
    {
        public string SeleteID { get; set; }
        public string TitleKey { get; set; }
        public string TitleName { get; set; }
        public List<OptionCombobox> listOption { get; set; }
        public ViewControlCombobox() {
            this.listOption = new List<OptionCombobox>() { new OptionCombobox(){ Key = string.Empty, Value = string.Empty} };
        }
    }
    public class OptionCombobox
    {
        public string ID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ViewControlTreeView
    {
        public ViewControlTreeView()
        {
            nodes = new List<ViewControlTreeView>();
        }
        public string id { get; set; }
        public string text { get; set; }
        public string ValueCD { get; set; }
        public IList<ViewControlTreeView> nodes { get; set; }
    }

    public class EvaluationMatlabClass
    {
        public MLApp.MLApp MatlabObj;
        public string FunctionName = "AutoRunEvaluation";

        public int RealFakeThreshold = 100;
        public int StandardOrNotThreshold = 100;
        public int PassOrNotThreshold = 80;

        public bool isNumberic = false;
        public int numNumericItem;
        public List<string> arrItemNumbericID;
        public List<string> arrItemImagePath;
        public double[] arrNumFeature;
        public double[] arrPassOrNotTime;
        public int[] arrPassOrNotResult;


        public string ProjectPath;
        public string matlabFuncPath;
        public string numbericDataPath;

        public string BagPath;
        public string MeanMhistRGBPath;
        public string VLFeat_LibPath;

        public EvaluationMatlabClass()
        {
            MatlabObj = new MLApp.MLApp();

            ProjectPath = Utility.pathMatlab;
            matlabFuncPath = ProjectPath + ConfigurationSettings.AppSettings["MatlabFuncPath"];
            numbericDataPath = ProjectPath + ConfigurationSettings.AppSettings["NumbericDataPath"];

            numNumericItem = 0;
            arrItemNumbericID = new List<string>();
            arrItemImagePath = new List<string>();
            //arrNumFeature = new double[numNumericItem];
            //arrPassOrNotTime = new double[numNumericItem];
            //arrPassOrNotResult = new int[numNumericItem];

            //Date for Auto Evaluation
            BagPath = ProjectPath + "AutoEvaluationData\\Bag.mat";
            MeanMhistRGBPath = ProjectPath + "AutoEvaluationData\\MeanMhistRGB.mat";
            VLFeat_LibPath = ProjectPath + "AutoEvaluationData\\VLFeat_Lib\\toolbox\\vl_setup.m";
        }
    }
    public class EvaluationImageClass
    {
        public string EvaluationID;
        public string CustomerID;
        public int CustomerImageID;
        public string ImageName;

        public string InputImagePath;
        public bool isExistImg;
        public string ErrorStatus;

        public string ComparedImagePath;
        public int RealOrFakeResult = 0;
        public double PercentFake = 0;
        public double RealOrFakeTime = 0;

        public int StandardOrNotResult = 0;
        public double NumCorrelation = 0;
        public double StandardOrNotTime = 0;

        public int PassOrNotResult = 0;
        public double PassOrNotTime = 0;
        public double NumFeature = 0;
        public string ImageStatus;

        public bool Marking;
        public int PassOrNotNumberic = 0;

        public EvaluationImageClass()
        {
            InputImagePath = "";
            ComparedImagePath = "";
            ErrorStatus = "";
            isExistImg = false;
            this.Marking = false;
        }
        public void CheckImageValidOrNot()
        {
            this.isExistImg = Utility.URLExists(InputImagePath);
            if (this.isExistImg)
            {
                this.isExistImg = Utility.URLExists(ComparedImagePath);
            }
            
        }
    }

    public class AutoMarkingResult
    {
        public int RealOrFakeResult;
 
    }
 

}

