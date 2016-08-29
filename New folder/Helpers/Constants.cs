using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using DMSERoute.Helpers;

namespace Hammer.Helpers
{
    public class Constants
    {
        #region Config send mail
        public static string Host = ConfigurationManager.AppSettings["Host"];
        public static int Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
        public static string SubjectCreate = ConfigurationManager.AppSettings["SubjectCreate"];
        public static string SubjectReset = ConfigurationManager.AppSettings["SubjectReset"];
        public static string SubjectSchedule = ConfigurationManager.AppSettings["SubjectSchedule"];

        public static string FromEmail = ConfigurationManager.AppSettings["FromEmail"];
        public static string DisplayName = ConfigurationManager.AppSettings["DisplayName"];
        public static string Password = ConfigurationManager.AppSettings["Password"];
        #endregion

        #region Template body of email
        public static string EmailCreateUserHTML = ConfigurationManager.AppSettings["EmailCreateUserHTML"];
        public static string EmailResetPassHTML = ConfigurationManager.AppSettings["EmailResetPassHTML"];

        public static string EmailApproveScheduleHTML = ConfigurationManager.AppSettings["EmailApproveScheduleHTML"];
        public static string EmailDeleteScheduleHTML = ConfigurationManager.AppSettings["EmailDeleteScheduleHTML"];
        public static string EmailRejectScheduleHTML = ConfigurationManager.AppSettings["EmailRejectScheduleHTML"];
        public static string EmailUploadScheduleHTML = ConfigurationManager.AppSettings["EmailUploadScheduleHTML"];
        public static string EmailUploadScheduleManageHTML = ConfigurationManager.AppSettings["EmailUploadScheduleManageHTML"];
        public static string EmailChangeScheduleManageHTML = ConfigurationManager.AppSettings["EmailChangeScheduleManageHTML"];
        public static string EmailOpenScheduleHTML = ConfigurationManager.AppSettings["EmailOpenScheduleHTML"];
        #endregion

        #region Account

        public const int PASSWORD_LENGTH = 8;
        public const int NUMBER_OF_SPECIAL_CHARACTER = 4;

        #endregion

        public static int StoreTimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["StoreTimeOut"]);

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

        #region Prepare schedule

        public const string MONTH_FILENAME = "LichLamViec_{0}_{1}_Thang.xlsx";
        public const string DETAIL_FILENAME = "LichLamViec_{0}_{1}_ChiTiet.xlsx";

        #endregion

        #region Assessment

        public const string SM_TRAINING_ASSESSMENT_FILENAME = "Assessment_SS_SM_{0}_{1}_{2}.xlsx";
        public const string SS_TRAINING_ASSESSMENT_FILENAME = "Assessment_ASM_SS_{0}_{1}_{2}.xlsx";
        
        #endregion

    }
}