using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace eRoute.Models
{
    public class Constant
    {
        public static string FullDateTimePattern = "dd-MM-yyyy HH:mm:ss";
        public static string ShortTimePattern = "HH:mm";
        public static string TimePattern = "HH:mm:ss";
        public static string TimeSpanPattern = "h:m";
        public static string ShortDatePattern = "dd-MM-yyyy";
        public static string DatePattern = "dd-MM";
        public static string DateSQLPattern = "yyyy-MM-dd";

        public static string ROLE_CDD = "CDD";
        public static string ROLE_NSM = "NSM";
        public static string ROLE_ADMIN = "ADMIN";
        public static string ROLE_DB = "DISTRIBUTOR";
        public static string ROLE_SUP = "SUP";
        public static string ROLE_ASM = "ASM";
        public static string ROLE_RSM = "RSM";

        public static string FeatureLeader = "Leader";
        public static string FeatureMarker = "Marker";

        public static string SalesmanAvatarFolder = ConfigurationSettings.AppSettings.Get("SalesmanAvatarFolder");
        public static string OutletImageFolder = ConfigurationSettings.AppSettings.Get("OutletImageFolder");
        public static string SalesOrdersImageFolder = ConfigurationSettings.AppSettings.Get("SalesOrdersImageFolder");
        public static string ImagePlanogramFolder = ConfigurationSettings.AppSettings.Get("ImagePlanogramFolder");
        public static string imageDateFormat = ConfigurationSettings.AppSettings["ImageDateFormat"];

        public static string IssuesImageFolder = ConfigurationSettings.AppSettings["IssuesImageFolder"];

        //Auto generate password
        public static int LengthPassword = 20;
        public static int NumberOfSpecialCharacters = 5;

        //Config send mail
        public static string Host = ConfigurationSettings.AppSettings["Host"];
        public static int Port = Convert.ToInt32(ConfigurationSettings.AppSettings["Port"]);
        public static string SubjectCreate = ConfigurationSettings.AppSettings["SubjectCreate"];
        public static string SubjectReset = ConfigurationSettings.AppSettings["SubjectReset"];

        public static string FromEmail = ConfigurationSettings.AppSettings["FromEmail"];
        public static string DisplayName = ConfigurationSettings.AppSettings["DisplayName"];
        public static string Password = ConfigurationSettings.AppSettings["Password"];
        
        //Template body of email
        public static string EmailCreateUserHTML = ConfigurationSettings.AppSettings["EmailCreateUserHTML"];
        public static string EmailResetPassHTML = ConfigurationSettings.AppSettings["EmailResetPassHTML"];
        public static string EmailDeliveryFormHTML = ConfigurationSettings.AppSettings["EmailDeliveryFormHTML"];
        public static string EmailDistributorHTML = ConfigurationSettings.AppSettings["EmailDistributorHTML"];

        // Template PDF
        public static string PDFSourceFile = ConfigurationSettings.AppSettings["PDFSourceFile"];
        
        public static int StoreTimeOut = Convert.ToInt32(ConfigurationSettings.AppSettings["StoreTimeOut"]);

        public static string appFolder = ConfigurationSettings.AppSettings["AppFolder"];

        // Cham diem
     
        public static string ViewButton = "disabled";
        public static string Eval_Rev = "Rev";
        public static string Eval_Auditor = "3";
        public static string Eval_Type_Auto = "A";
        public static string Eval_Type_Manual = "M";
        public static string Eval_VSC009 = "VSC009";
        public static string Eval_VSC008 = "VSC008";
        public static string Eval_VSC007 = "VSC007";
        public static string Eval_VSC006 = "VSC006";
        public static string Eval_VSC005 = "VSC005";
        public static string Eval_VSC004 = "VSC004";
        // Trang thai cua ky danh gia
                     // 1. Chưa phân bố 
                     //2. Đã phân bổ
                     //3. Đang đánh giá 
                     //4. Đã đánh giá
                     //5. Đang xét duyệt
                     //6. Đã hoàn tất.
                     //7.Đã hủy

        public static int Eval_Stauts_1 = 1;
        public static int Eval_Stauts_2 = 2;
        public static int Eval_Stauts_3 = 3;
        public static int Eval_Stauts_4 = 4;
        public static int Eval_Stauts_5 = 5;
        public static int Eval_Stauts_6 = 6;
        public static int Eval_Stauts_7 = 7;

        //ECalendar
        #region Assessment
        #region SMAssessment
        public const char DailyWorks = 'D';
        public const char Tools = 'T';
        public const char Steps = 'S';
        #endregion

        #region Non-SMAssessment
        public const char TrainingProcess = 'P';
        public const char TrainingQuality = 'Q';
        public const char UpdateArchive = 'U';
        public const char UsingTool = 'T';
        public const char ToolsSM = 'O';
        #endregion

        #endregion
        public static string SubjectSchedule = ConfigurationManager.AppSettings["SubjectSchedule"];
        #region Template body of email        

        public static string EmailApproveScheduleHTML = ConfigurationManager.AppSettings["EmailApproveScheduleHTML"];
        public static string EmailDeleteScheduleHTML = ConfigurationManager.AppSettings["EmailDeleteScheduleHTML"];
        public static string EmailRejectScheduleHTML = ConfigurationManager.AppSettings["EmailRejectScheduleHTML"];
        public static string EmailUploadScheduleHTML = ConfigurationManager.AppSettings["EmailUploadScheduleHTML"];
        public static string EmailUploadScheduleManageHTML = ConfigurationManager.AppSettings["EmailUploadScheduleManageHTML"];
        public static string EmailChangeScheduleManageHTML = ConfigurationManager.AppSettings["EmailChangeScheduleManageHTML"];
        public static string EmailOpenScheduleHTML = ConfigurationManager.AppSettings["EmailOpenScheduleHTML"];
        #endregion
    }
}