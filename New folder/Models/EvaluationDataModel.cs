using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eRoute.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    using System.Globalization;
    using System.Web.Mvc;
using System.Configuration;
using DMSERoute.Helpers;

    public class DetailOutletImageClass
    {
        public string ScreenID {get; set;}
        public string UserID {get; set;}
        public int OutletIndex { get; set; }
        public string isMatchedWithBefore {get; set;}
        public string isCaptured {get; set;}
        public string isAccepted {get; set;}
        public string isPassed {get; set;}
        public string isFinished { get; set; }
        public string isReviewed { get; set; }
        public string Reason1 { get; set; }
        public string Reason2 { get; set; }
        public string Reason3 { get; set; }
        public string Reason4 { get; set; }
        public string DSKQChamDiem { get; set; }
        public string DSLyDoChamDiem { get; set; }
        public string DSNumericChamDiem { get; set; }
        public string currentOutletID { get; set; }
        public string currentOutletName { get; set; }
        public string nextOutletID { get; set; }
        public string nextOutletName { get; set; }
        public int selectedImageIdx { get; set; }
        public string ComparedImageIDName { get; set; }
        public string ImageIDName { get; set; }
        public string ImageIDDate { get; set; }
        public List<usp_GetComparedImageDataByOutletIDResult> ListLatestComparedImages;
        public string AvatarImageIDName { get; set; }
        public string SalemanName { get; set; }
        public string CaptureDistance { get; set; }
        public string ImageIDEvalDate { get; set; }
        public string ImageIDEvalUserName { get; set; }
        public List<ImageDataModel> EvalImagesList;

        public DetailOutletImageClass()
        {
            ScreenID = ""; UserID = "";
            OutletIndex = 0; 
            isMatchedWithBefore = "0"; isCaptured = "0"; isAccepted = "0"; isPassed = "0"; isFinished = "0"; isReviewed = "0";
            Reason1 = "0"; Reason2 = "0"; Reason3 = "0"; Reason4 = "0";
            DSKQChamDiem = "1;1;1;1;"; DSLyDoChamDiem = "0;0;0;0;";
            DSNumericChamDiem = ""; currentOutletID = ""; currentOutletName = "";  selectedImageIdx = 0; nextOutletID = ""; nextOutletName = "";
            ComparedImageIDName = ""; ImageIDName = ""; ImageIDDate = "";
            ListLatestComparedImages = new List<usp_GetComparedImageDataByOutletIDResult>();
            AvatarImageIDName = ""; SalemanName = ""; CaptureDistance = "0 m"; ImageIDEvalDate = ""; ImageIDEvalUserName = "";
            EvalImagesList = new List<ImageDataModel>();
        }
    }

    public class MatlabRunningClass
    {
        private bool result;
        private double rate1;
        private double rate2;

        public MatlabRunningClass()
        {
            result = false;
            rate1 = 0d;
            rate2 = 0d;
        }

        public MatlabRunningClass(bool bResult, double dRate1, double dRate2)
        {
            result = bResult;
            rate1 = dRate1;
            rate2 = dRate2;
        }

        public void SetValues(bool bResult, double dRate1, double dRate2)
        {
            result = bResult;
            rate1 = dRate1;
            rate2 = dRate2;
        }

        public void SetValues(bool bResult, double dRate1)
        {
            result = bResult;
            rate1 = dRate1;
            rate2 = -1;
        }

        public bool GetResult() { return result; }

        public double GetRate1() { return rate1; }

        public double GetRate2() { return rate2; }

        public bool isCaptured(MLApp.MLApp matlabIns, string workingPath, string fileName)
        {
            /*******************************************************/
            /* Recaptured image detection
            /*******************************************************/
            RunMATLAB(matlabIns, workingPath, fileName);

            //string str = "";
            //str += "\n" + "RECAPTURING DETECTION\n > Real Image: " + GetResult();
            //str += " > Rate: " + GetRate1() + " vs. " + GetRate2();

            return GetResult();
        }

        public bool isAccepted(MLApp.MLApp matlabIns, string workingPath, string fileName1, string fileName2, int initVLFeat)
        {
            /*******************************************************/
            /* Recaptured image detection
            /*******************************************************/
            RunMATLAB(matlabIns, workingPath, fileName1, fileName2, initVLFeat);

            //string str = "";
            //str = "IMAGE CORRELATION\n > Matched: " + GetResult();
            //str += "  > Rate: " + GetRate1();

            return GetResult();
        }


        public bool isPassed(MLApp.MLApp matlabIns, string workingPath, string fileName1)
        {
            /*******************************************************/
            /* Lid Haar cascade detector
            /*******************************************************/
            //string str = "";
            Boolean exi = RunMATLAB(matlabIns, workingPath, fileName1, 0);
            //str += "\n" + "LID HAAR CASCADE DETECTOR\n > 0do existence: " + exi;

            return exi;
        }

        /* Calculate 2 images' correlation */
        /*      - Input:    MATLAB instance, MATLAB working path, the 1st image filename, the 2nd image filename, initiate VLFeat library
         *      - Output:   [matched, rate]
         */
        public void RunMATLAB(MLApp.MLApp matlabIns, string matlabFuncPath, string fileName1, string fileName2, int initVLFeat)
        {
            // Change to the directory where the function is located 
            matlabIns.Execute("cd " + matlabFuncPath);

            // Define the output
            object result = null;

            // Call the MATLAB function myfunc
            matlabIns.Feval("CheckingMatchingImg", 2, out result, fileName1, fileName2, initVLFeat);

            // Display result
            object[] res = result as object[];

            SetValues(Convert.ToBoolean(res[0]), Convert.ToDouble(res[1]));
        }

        /* Recaptured image detection */
        /*      - Input:    MATLAB instance, MATLAB working path, image filename
         *      - Output:   [result, rate]
         */
        public void RunMATLAB(MLApp.MLApp matlabIns, string matlabFuncPath, string fileName)
        {
            // Change to the directory where the function is located 
            matlabIns.Execute("cd " + matlabFuncPath);

            // Define the output
            object result = null;

            // Call the MATLAB function myfunc
            matlabIns.Feval("DetectingImage", 3, out result, fileName);

            // Display result
            object[] res = result as object[];

            SetValues(Convert.ToBoolean(res[0]), Convert.ToDouble(res[1]), Convert.ToDouble(res[2]));

        }

        /* Lid Haar cascade detector */
        /*      - Input:    MATLAB instance, MATLAB working path, image filename, write output image
         *      - Output:   co tra 0do hay khong
         */
        public Boolean RunMATLAB(MLApp.MLApp matlabIns, string matlabFuncPath, string fileName, int writeImage)
        {
            // Change to the directory where the function is located 
            matlabIns.Execute("cd " + matlabFuncPath);

            // Define the output
            object result = null;

            // Call the MATLAB function myfunc
            matlabIns.Feval("LidHaarCascadeDetector", 1, out result, fileName, writeImage);

            // Display result
            object[] res = result as object[];

            return Convert.ToBoolean(res[0]);
        }

    }

    public class InputDataModel
    {
        public InputDataModel()
        {
            this.TyLeReview = 0;
        }
        public string MaDinhNghia { get; set; }
        public string MaThamChieu { get; set; }
        public string MaCTTB { get; set; }
        public string NoiDungCTTB { get; set; }
        public string ThoiGianCTTBTu { get; set; }
        public string ThoiGianCTTBDen { get; set; }
        public string KieuTGDanhGia { get; set; }
        public string ThoiGianDanhGiaTu { get; set; }
        public string ThoiGianDanhGiaDen { get; set; }
        public int TyLeReview { get; set; }
        public string LoaiHinhDanhGia { get; set; }
        public string DanhGiaNumeric { get; set; }
        public string DanhSachItemID { get; set; }
        public string DanhSachChonItem { get; set; }
        public string DanhSachUserID { get; set; }
        public string DanhSachUserRole { get; set; }
        public string LoaiKhuVuc { get; set; }
        public string DanhSachKhuVuc { get; set; }
        public string sNewEvaluationID { get; set; }
        public string DanhSachUserLeader { get; set; }
    }

    public class FilterDataModel
    {
        public FilterDataModel()
        { 
            inputEvaluationID = "";
            inputReferenceCD = "";
            inputDisplay = "";
            inputDescription = "";
            inputContent = "";
            inputDisplayDateFrom = "";
            inputDisplayDateTo = "";
            inputEvalDateFrom = "";
            inputEvalDateTo = "";
            inputStatus = 0;
            inputNumPage = 0;
            inputNumPaging = 0;
        }

        public string inputEvaluationID { get; set; }
        public string inputReferenceCD { get; set; }
        public string inputDisplay { get; set; }
        public string inputDescription { get; set; }
        public string inputContent { get; set; }
        public string inputDisplayDateFrom { get; set; }
        public string inputDisplayDateTo { get; set; }
        public string inputEvalDateFrom { get; set; }
        public string inputEvalDateTo { get; set; }
        public string inputScreenID { get; set; }
        public string inputUserID { get; set; }
        public int inputStatus { get; set; }
        public int inputNumPage { get; set; }
        public int inputNumPaging { get; set; }

        public string CombineFilterDateToString()
        {
            string str = "";

            str = str + inputEvaluationID + ";" + inputReferenceCD + ";" + inputDisplay + ";";
            str = str + inputDescription + ";" + inputContent + ";";
            str = str + inputDisplayDateFrom + ";" + inputDisplayDateTo + ";";
            str = str + inputEvalDateFrom + ";" + inputEvalDateTo + ";";
            str = str + inputScreenID + ";" + inputUserID + ";";
            str = str + inputStatus.ToString() +";" + inputNumPage.ToString() + ";" + inputNumPaging.ToString();

            return str;
        }

        public void UpdateToAppropriateData()
        {
            inputEvaluationID = (string.IsNullOrEmpty(inputEvaluationID)) ? "" : inputEvaluationID;
            inputReferenceCD = (string.IsNullOrEmpty(inputReferenceCD)) ? "" : inputReferenceCD;
            inputDisplay = (string.IsNullOrEmpty(inputDisplay)) ? "" : inputDisplay;
            inputDescription = (string.IsNullOrEmpty(inputDescription)) ? "" : inputDescription;
            inputContent = (string.IsNullOrEmpty(inputContent)) ? "" : inputContent;
            inputDisplayDateFrom = (string.IsNullOrEmpty(inputDisplayDateFrom)) ? "" : inputDisplayDateFrom;
            inputDisplayDateTo = (string.IsNullOrEmpty(inputDisplayDateTo)) ? "" : inputDisplayDateTo;
            inputEvalDateFrom = (string.IsNullOrEmpty(inputEvalDateFrom)) ? "" : inputEvalDateFrom;
            inputEvalDateTo = (string.IsNullOrEmpty(inputEvalDateTo)) ? "" : inputEvalDateTo;
            inputStatus = (inputStatus < 0) ? 0 : inputStatus;
            inputNumPage = (inputNumPage < 0) ? 0 : inputNumPage;
            inputNumPaging = (inputNumPaging < 0) ? 0 : inputNumPaging;
        }

    }

    // Thuan Tran add

    public class FilterModel
    {
        public FilterModel()
        {
            this.isAuto = false;
            this.HasRate = false;
            this.isNumeric = false;
            this.isGeography = false;
            this.Display = "";
            this.EvaluationID = "";
            this.DisplayID = "";
            this.ReferenceCD = "";
            this.Description = "";
            this.Content = "";
            this.DisplayFromDate = null;
            this.DisplayToDate = null;
            this.EvalFromDate = null;
            this.EvalToDate = null;
            this.ScreenID = "";
            this.UserID = "";
            this.Status = 0;
            this.Type = "0";
            this.listStatus = new List<SelectListItem>() { new SelectListItem() { Text = string.Empty, Value = string.Empty } };
            this.listType = new List<SelectListItem>() { new SelectListItem() { Text = string.Empty, Value = string.Empty } };
            this.listStatusAs = new List<SelectListItem>() { new SelectListItem() { Text = string.Empty, Value = string.Empty } };
        }
        public string DisplayID { get; set; }
        public string Display { get; set; }
        public string EvaluationID { get; set; }
        public string ReferenceCD { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime? DisplayFromDate { get; set; }
        public DateTime? DisplayToDate { get; set; }
        public DateTime? EvalFromDate { get; set; }
        public DateTime? EvalToDate { get; set; }
        public string ScreenID { get; set; }
        public string UserID { get; set; }
        public int Status { get; set; }
        public string Type { get; set; }
        public bool HasRate { get; set; }
        public int Rate { get; set; }
        public bool isAuto { get; set; }
        public bool isNumeric { get; set; }
        public bool isGeography { get; set; }
        public List<SelectListItem> listStatus { get; set; }
        public List<SelectListItem> listStatusAs { get; set; }
        public List<SelectListItem> listType { get; set; }
        public List<uvw_GetDisplayInformation> DisplayData { get; set; }
        public List<DMSAcuDisplayEval> ListAcuEval { get; set; }
        public List<usp_GetEvaluationInfoByTypeResult> EvaluationData { get; set; }
        public List<usp_GetEvaluationInfoByType_AssessmentResult> EvaluationDataAssessment { get; set; }
    }

    public class EvalAutoMarkMV
    {
        public string NameSceen;
        public Utility.StatusAutoMark status { get; set; }
        public DMSEvaluation Evaluation { get; set; }
        public bool CheckConnectMatLab { get; set; }

        public int TotalImages { get; set; }
        public int ImagesProgress { get; set; }
        public int ImageMarking { get; set; }
        public int ImagePesen { get; set; }
        public int TotalOutlet { get; set; }
        public int OutletMarking { get; set; }
        public int OutletRemain { get; set; }
        public int TimePlanMarking { get; set; }
        public double TimeMarking { get; set; }
        public int ImgNotExist { get; set; }
        public int ImgErrorMarking { get; set; }
        public int ImgThat { get; set; }
        public int ImgChuan { get; set; }
        public int ImgPass { get; set; }
        public int ImgNumberic { get; set; }
        public int ImgFakes { get; set; }
        public int ImgNotStandard { get; set; }
        public int ImgNotPass { get; set; }
        public int ImgNotPassNumberic { get; set; }
        public int ImgRejectMark { get; set; }
        public double TimeAverage { get; set; }
        public DateTime DateAutoMarking { get; set; }
        public string strDateFinish { get; set; }
        public string strTimeMarking { get; set; }
        public List<EvaluationImageClass> ImagesList { get; set; }
        public List<usp_GetListImageByResult> ListImageBy { get; set; }
        public EvalAutoMarkMV()
        {
            this.CheckConnectMatLab = false;
            this.status = Utility.StatusAutoMark.New;
            this.TotalImages = 0;
            this.ImagesProgress = 0;
            this.ImageMarking = 0;
            this.ImagePesen = 0;
            this.TotalOutlet = 0;
            this.OutletMarking = 0;
            this.OutletRemain = 0;
            this.TimePlanMarking = 0;
            this.TimeMarking = 0;
            this.ImgNotExist = 0;
            this.ImgErrorMarking = 0;
            this.ImgThat = 0;
            this.ImgChuan = 0;
            this.ImgPass = 0;
            this.ImgNumberic = 0;
            this.ImgFakes = 0;
            this.ImgNotStandard = 0;
            this.ImgNotPass = 0;
            this.ImgNotPassNumberic = 0;
            this.ImgRejectMark = 0;
            this.TimeAverage = 0;
            this.DateAutoMarking = DateTime.Now;
            Evaluation = new DMSEvaluation();
            ImagesList = new List<EvaluationImageClass>();
            ListImageBy = new List<usp_GetListImageByResult>();
        }
    }
    //end

    public class ImageDataModel
    {
        //public string EvalImageLocation = "http://eroute.thmilk.vn/SFA/SalesOrder/images/";
        public string EvalImageLocation = ConfigurationSettings.AppSettings["SalesOrdersImageFolder"];
        // Thong Tin So Sanh
        public int ImageID {get; set;}
        public string ImageIDName { get; set; }
        public string ImageDesc { get; set; }
        public string ImageIDDate {get; set;}
        public int ComparedImageID {get; set;}  //
        public string ComparedImageIDName { get; set; }
        public string ComparedImageDesc { get; set; }
        public string ComparedImageIDDate {get; set;}
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string AvatarImageFile { get; set; }

        public List<usp_GetComparedImageDataByOutletIDResult> ListLatestComparedImages;

        // Thong Tin Cham Diem
        public int ImageStatus { get; set; }
        public string ImageColorStatus { get; set; }
        public string ImageIDEvalStatus {get; set;}
        public string ImageIDRevStatus { get; set; }
        public string ImageIDEvaluated { get; set; }
        public string ImageIDReviewed { get; set; }
        public bool isEdit { get; set; }

        public List<string> ImageIDEvalResult;  // Index 0: Khop voi 4 anh ; 1: That/Gia ; 2: Chuan/KhongChuan ; 3: Dat/Khong Dat ; 4: Approve/Reject
        public List<string> ImageIDEvalReason; 
        public string ListNumericItemExists; //true : Ton Tai;  false : Khong Ton Tai


        public bool? isMatchedWithBefore { get; set; }
        public bool? isCaptured { get; set; }
        public bool? isAccepted { get; set; }
        public bool? isPassed { get; set; }
        public bool? isFinished { get; set; }
        public string isReviewed { get; set; }
        public bool? isApproved { get; set; }
        public string Reason1 { get; set; }
        public string Reason2 { get; set; }
        public string Reason3 { get; set; }
        public string Reason4 { get; set; }

        //Thong Tin Khac
        public string SalemanName {get; set;}
        public string CaptureDistance {get; set;}
        public DateTime? ImgDate { get; set; }
        public DateTime? ImgDateMark { get; set; }
        public DateTime? SyncDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public DateTime? ReFinishDate { get; set; }

        public string ImageIDEvalDate {get; set;}
        public string ImageIDEvalUserName {get; set;}

        public ImageDataModel()
        {
            ListLatestComparedImages = new List<usp_GetComparedImageDataByOutletIDResult>();
            ImageIDEvalResult = new List<string>();
            ImageIDEvalReason = new List<string>();

            ImageID = 0;
            ImageIDName = "";
            ImageIDDate = "";
            ComparedImageID = 0;
            ComparedImageIDName = "";
            ComparedImageIDDate = "";
            this.isEdit = true;

            ImageIDEvalStatus = "";
            ImageIDRevStatus = "";
            ImageIDEvaluated = "";
            ImageIDReviewed = "";
            
            ListNumericItemExists = "";

            SalemanName = "";
            CaptureDistance = "";
            ImageIDEvalDate = "";
            ImageIDEvalUserName = "";
        }

        public void UpdateImageFileName()
        {
            for (int i = 0; i < ListLatestComparedImages.Count; i++)
                ListLatestComparedImages[i].AnhSoSanh = EvalImageLocation + ListLatestComparedImages[i].AnhSoSanh;
        }
    }

    public class OutletDataModel
    {
        public string AvatarImageLocation = ConfigurationSettings.AppSettings["OutletImageFolder"]; //"http://eroute.nhatnhat.com/AvartaImage/";
        public string OutletID {get; set;}
        public string OutletName {get; set;}
        public bool hasData {get; set;}
        public int AvatarImageID {get; set;}
        public string AvatarImageIDName { get; set; }
        public string AvatarImageIDDate { get; set; }

        public List<ImageDataModel> EvalImagesList;
        public int selectedImageIdx { get; set; }

        public OutletDataModel()
        {
            OutletID = "";
            OutletName = "";
            hasData = false;
            AvatarImageID = 0;
            selectedImageIdx = 0;
            EvalImagesList = new List<ImageDataModel>();
        }
    }

    [Serializable]
    public class EvaluationDataModel
    {
        //Dùng cho các màn hình Định nghĩa kỳ đánh giá
        public FilterModel FilterModel { get; set; }
        public List<usp_GetEvaluationDetailInfoByFilter_ParamResult> EvalDefinitionResult { get; set; }
        public List<uvw_GetDisplayInformation> DisplayList;
        public List<usp_GetEvaluationInfoByTypeResult> EvaluationList;
        public List<usp_GetGeoologyInforTree_NewResult> GeoologyList;
        public List<usp_GetGeoologyInforTree_NewResult> GeoologyNewList;
        public List<uvw_GetUserInformation> AllUserList;
        public List<uvw_GetRolesInformation> RolesList;


        //Dùng cho man hinh load danh sách đánh giá.
        //public List<usp_GetOutletsImageEvaluationByFilter_ParamResult> OutletEvalDetailList { get; set; }

        public List<usp_GetOutletsOfEvalByResult> OutletEvalDetailList { get; set; }

        //Dùng cho man hinh load danh sách review
        public List<usp_GetEvaluationUserByResult> ReviewList;

        //dùng cho màn hình theo dõi tự động
        public List<usp_GetInfoMarkingByIDResult> AutoEvalList;

        //dùng cho màn hình chi tiết Eval
        public List<uvw_GetUserInformation> SelectedUserList;


        public List<usp_GetItemInforByIDResult> ListItem;
        public List<usp_GetUserInformationByIDResult> UserList;
        public List<usp_GetAddUserInformationByIDResult> AddUserList;
        
        
       
        //public List<ufn_GetOutletsImageReviewByIDResult> ReviewList;
        //public List<usp_GetOutletsImageReviewByFilterResult> ReviewList;
        
        public List<usp_ReportGeneralEvaluationResult> ReportGeneralEvaluationList;
        public List<usp_ReportDetailEvaluationResult> ReportDetailEvaluationList;
        public List<usp_ReportNumericEvaluationResult> ReportNumericEvaluationList;
        public List<ufn_GetOutletInforByIDResult> OutletList;
        public List<usp_GetDisplayReasonResult> ReasonList;
        public List<ufn_GetBaselineEvaluationByIDResult> BaselineList;
        public List<usp_GetOutletImageDataByIDResult> OutletImageData;
        public List<usp_GetOutletImageStatusByIDResult> OutletImageStatus;
        public List<usp_GetAutoEvalOutletImageByIDResult> AutoEvalOutletImageList;
        
        //for Man Hinh DetailOutletImageEvaluation

        public string selectedEvaluationID = ""; //EvaluationID Detail
        public string selectedOutletID = "";
        public string selectedOutletName = "";
        public string selectedAuditorID = "";

        // Dung cho viec danh gia chi tiet va xet duyet chi tiet
        public int idxOutlet = 0;
        public string currentOutletID = "";
        public string currentOutletName = "";
        public string nextOutletID = "";
        public string nextOutletName = "";
        public List<OutletDataModel> EvalOutletsList;

        //public List
        public EvaluationDataModel()
        {
            this.FilterModel = new Models.FilterModel();
            this.ReviewList = new List<usp_GetEvaluationUserByResult>();
            this.EvalOutletsList = new List<OutletDataModel>();
            this.ListItem = new List<usp_GetItemInforByIDResult>();
            this.UserList = new List<usp_GetUserInformationByIDResult>();
            this.SelectedUserList = new List<uvw_GetUserInformation>();
            this.AllUserList = new List<uvw_GetUserInformation>();
            this.DisplayList = new List<uvw_GetDisplayInformation>();
            this.OutletEvalDetailList = new List<usp_GetOutletsOfEvalByResult>();
            this.OutletList = new List<ufn_GetOutletInforByIDResult>();
        }

        public string GetValueAt(string ds, int Index)
        {
            string[] str = ds.Split(';');

            if (Index < str.Count())
                return str[Index];
            else
                return "0";
        }

        //for store usp_GetNumFromEvaluationID
        public string sSoOutlet = "0";
        public string sSoAuditor = "0";
        public string sSoPhanBo = "0";

        public void Init_GetNumFromEvaluationID()
        {

        }

        public void Update_GetNumFromEvaluationID()
        {

        }

        // for store usp_GetEvaluationDetailByID
        public string EvaluationID = ""; //Evaluation Header
        public string sMaDanhGia = "";
        public string sMaThamChieu = "";
        public string sChuongTrinhTrungBay = "";
        public string sThoiGianTrungBayFrom = "";
        public string sThoiGianTrungBayTo = "";
        public string sThoiGianDanhGiaFrom = "";
        public string sThoiGianDanhGiaTo = "";
        public string sMoTaCTTB = "";
        public string sNoiDungCTTB = "";
        public string sLoaiHinhDanhGia = "";
        public string sKieuTGDanhGia = "";
        public string sTyLeReview = "";
        public string sTrangThaiDanhGia = "0";
        public string sisNumeric = "0";
        public string sLoaiKhuVuc = "1";
        public string sDanhSachKhuVucID = "";

        public void ResetHeaderInformation()
        {
            EvaluationID = ""; //Evaluation Header
            sMaDanhGia = "";
            sMaThamChieu = "";
            sChuongTrinhTrungBay = "";
            sThoiGianTrungBayFrom = "";
            sThoiGianTrungBayTo = "";
            sThoiGianDanhGiaFrom = "";
            sThoiGianDanhGiaTo = "";
            sMoTaCTTB = "";
            sNoiDungCTTB = "";
            sLoaiHinhDanhGia = "";
            sKieuTGDanhGia = "";
            sTyLeReview = "";
            sTrangThaiDanhGia = "0";
            sisNumeric = "0";
            sLoaiKhuVuc = "1";
            sDanhSachKhuVucID = "";
            sSoAuditor = "";
            sSoCuaHangChuaDanhGia ="";
            sSoCuaHangChuaDanhGia = "";
        }

        public void UpdateTerritoryChecked()
        {
            for (int i = 0; i < GeoologyList.Count(); i++)
            {
                if (sDanhSachKhuVucID.Contains(GeoologyList[i].KhuVucDiaLyID))
                    GeoologyList[i].HanhDong = "checked";
            }
        }

        // Thong tin khac
        //public int ImageIndex = 0;
        //public List<string> DSOutletID;
        //public List<OutletImageDataModel> ListOutLetImages;

        public void Init_GetOutletImageDataByID()
        {
            //DSOutletID = new List<string>();
            //ListOutLetImages = new List<OutletImageDataModel>();
        }

        public void Update_GetOutletImageDataByID()
        {
            //for (int i = 0; i < ListOutLetImages.Count; i++)
            //    ListOutLetImages[i].UpdateDateFormat();
        }

        public string sNgayDanhGiaAuto = "";
        public string sThoiGianDanhGiaAuto = "0";
        public string sSoCuaHangDaDanhGia = "0";
        public string sSoCuaHangChuaDanhGia = "0";
        public string sNgayKetThucDanhGia = "";
        public string sTongSoAnhDanhGia = "0";
        public string sSoAnhGia = "0";
        public string sSoAnhChupKhongChuan = "0";
        public string sSoAnhChupKhongDat = "0";
        public string sSoAnhChamNumeric = "0";

        public void Init_GetAutoProccessingData()
        {

        }

        public void Update_GetAutoProccessingData()
        {
        }
    }

    public class ViewGeoology {
        public string MaKhuVuc { get; set; }
        public string id { get; set; }
        public string parentId { get; set; }
        public string text { get; set; }
        public Dictionary<string, bool> state { get; set; }
    }

    [Serializable]
    public class SessionDataModel
    {
        //Thông tin filter
        public FilterDataModel filterData;

        // Thông tin login
        public string LoginID { get; set; }
        public string PrevScreenID { get; set; }
        public string ScreenID { get; set; }
        public string ViewName { get; set; }

        // Thông tin khác
        public string CheckedData { get; set; }
        public string DSEvaluationID { get; set; }
        public string ViewType { get; set; }

        //Thông tin thành phần reload lại
        public bool isLoadHeaderInfo { get; set; }
        public bool isLoadDetailInfo { get; set; }
        public bool isLoadMasterInfo { get; set; }
        public string StatusFilter { get; set; }

        public SessionDataModel()
        {
            filterData = new FilterDataModel();
            isLoadHeaderInfo = true;
            isLoadDetailInfo = true;
            isLoadMasterInfo = true;

            //viewedEvaluationID = "";
            //ImageIndex = 0;
            StatusFilter = "1;1;1;1;";
            LoginID = "";
            ScreenID = "";
            PrevScreenID = "";
            CheckedData = "";
            ViewType = "All";
        }
        
    }

    public class ReviewModel
    {
        public DMSEvaluation Evaluation { get; set; }
        public int TotalImg { get; set; }
        public int ImgReviewed { get; set; }
        public string pathImageCompare { get; set; }
        public string sEvalID { get; set; }
        public string Salesman { get; set; }
        public string UserID { get; set; }
        public string Auditor { get; set; }
        public string StatusImage { get; set; }
        public bool Approved { get; set; }
        public DateTime? ImageDate { get; set; }
        public string NameSceen { get; set; }
        public int curent { get; set; }
        public int totalPage { get; set; }
        public int limit { get; set; }
        public bool IsCompleted { get; set; }
        public List<SelectListItem> ListAuditor { get; set; }
        public List<SelectListItem> ListSalesman { get; set; }
        public List<SelectListItem> ListStatus { get; set; }
        public List<usp_GetListImageByResult> ListImageBy { get; set; }
        public List<SelectListItem> ListReason { get; set; }
        public ReviewModel()
        {
            this.curent = 1;
            this.totalPage = 0;
            this.limit = 20;
            this.Approved = false;
            this.ImageDate = null;
            this.pathImageCompare = string.Empty;
            this.IsCompleted = false;
            this.ListReason = new List<SelectListItem>();
            this.ListAuditor = new List<SelectListItem>();
            this.ListSalesman = new List<SelectListItem>();
            this.ListStatus = new List<SelectListItem>();
            this.ListImageBy = new List<usp_GetListImageByResult>();
        }
    }

    public class MarkModel
    {
        public DMSEvaluation Evaluation { get; set; }
        public string PathImageCompare { get; set; }
        public string TypeMark { get; set; }
        public string sEvalID { get; set; }
        public string Auditor { get; set; }
        public string CurentOutletID { get; set; }
        public int ImgFirst { get; set; }
        public int ImgLast { get; set; }
        public int ImgNext { get; set; }
        public int ImgPrev { get; set; }
        public int ImgMarkNext { get; set; }
        public int TotalImg { get; set; }
        public int ImgMarked { get; set; }
        public int TotalOutletImg { get; set; }
        public int ImgOutletMarked { get; set; }
        public ImageDataModel CurentImage { get; set; }
        public List<usp_GetComparedImageDataByOutletIDResult> ListLatestComparedImages;
        public List<ImageDataModel> ListImgCurentOutlet;
        public List<ImageDataModel> ListResult;
        public List<usp_GetDisplayReasonResult> ReasonFake;
        public List<usp_GetDisplayReasonResult> ReasonAccepted;
        public List<usp_GetDisplayReasonResult> ReasonDisplay;
        public List<usp_GetItemInforByIDResult> ListItem;
        public List<SelectListItem> ListOultet;

        public MarkModel()
        {
            this.sEvalID = string.Empty;
            this.PathImageCompare = Constant.SalesOrdersImageFolder;
            this.CurentOutletID = string.Empty;
            this.Auditor = SessionHelper.GetSession<string>("UserName");
            this.ListLatestComparedImages = new List<usp_GetComparedImageDataByOutletIDResult>();
            this.ListImgCurentOutlet = new List<ImageDataModel>();
            this.ListOultet = new List<SelectListItem>();
            this.ReasonFake = new List<usp_GetDisplayReasonResult>();
            this.ReasonAccepted = new List<usp_GetDisplayReasonResult>();
            this.ReasonDisplay = new List<usp_GetDisplayReasonResult>();
        }
        
    }

    #region Display Reason management
    public class DisplayReasonVM
    {
        public int DisplayID { get; set; }
        public string TypeReason { get; set; }
        public DisplayReason Reason { get; set; }

        public ViewControlCombobox listComboboxProgram { get; set; }
        public List<SelectListItem> ListTypeReason { get; set; }

        public List<DisplayReason> Result { get; set; }

        public DisplayReasonVM()
        {
            this.DisplayID = 0;
            this.TypeReason = string.Empty;
            this.Reason = new DisplayReason();
        }
        public void SetDataFilter(string ProgramID = "", string TypeReason = "")
        {
            if (string.IsNullOrEmpty(ProgramID))
            {
                this.DisplayID = 0;
            }
            else
            {
                this.DisplayID = int.Parse(ProgramID);
            }
            this.TypeReason = TypeReason;
            ViewControlCombobox ctrCombobox = new ViewControlCombobox();
            ctrCombobox.TitleKey = Utility.Phrase("ProgramName");
            ctrCombobox.TitleName = Utility.Phrase("TimeProgram");
            ctrCombobox.listOption = Global.VisibilityContext.uvw_GetDisplayInformations.ToList().Select(s => new OptionCombobox { ID = s.MaCTTB.ToString(), Key = s.ChuongTrinhTrungBay.ToString(), Value = s.ThoiGianBatDau + " - " + s.ThoiGianKetThuc }).ToList();
            ctrCombobox.listOption.Insert(0, new OptionCombobox() { ID = string.Empty, Key = string.Empty, Value = string.Empty });
            ctrCombobox.SeleteID = this.DisplayID.ToString();
            this.listComboboxProgram = ctrCombobox;

            List<SelectListItem> listTypeReason = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("SelectAll"), Value = "", Selected = (this.TypeReason == "") ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Fake"), Value = "T", Selected = (this.TypeReason == "T") ? true : false },
                new SelectListItem() { Text = Utility.Phrase("NoStandard"), Value = "C", Selected = (this.TypeReason == "C") ? true : false },
                new SelectListItem() { Text = Utility.Phrase("NoPass"), Value = "D", Selected = (this.TypeReason == "D") ? true : false },
            };
            this.ListTypeReason = listTypeReason;
        }
    }
    #endregion

    public class InventoryVM
    {
        public int CategoryID { get; set; }
        public int Type { get; set; }
        public string InventoryName { get; set; }
        public DMSEvalInventoryItem Inventory { get; set; }

        public List<SelectListItem> ListCategory { get; set; }
        public List<SelectListItem> ListType { get; set; }
        public List<InventoryPath> ListInventoryPath { get; set; }

        public List<DMSEvalInventoryItem> Result { get; set; }

        public string StorageRoot { get; set; }

        public InventoryVM()
        {
            this.CategoryID = 0;
            this.Type = -1;
            this.InventoryName = string.Empty;
            this.Inventory = new DMSEvalInventoryItem();
            this.Result = new List<DMSEvalInventoryItem>();
        }
        public void SetDataFilter(string Category = "", string Type = "", string InventoryName = "")
        {
            if (string.IsNullOrEmpty(Category))
            {
                this.CategoryID = 0;
            }
            else
            {
                this.CategoryID = int.Parse(Category);
            }
            if (string.IsNullOrEmpty(Type))
            {
                this.Type = -1;
            }
            else
            {
                this.Type = int.Parse(Type);
            }
            this.InventoryName = InventoryName;
            List<SelectListItem> listType = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("SelectAll"), Value = "-1", Selected = (this.CategoryID == -1) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Company"), Value = "1", Selected = (this.CategoryID == 1) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("NotCompany"), Value = "0", Selected = (this.CategoryID == 0) ? true : false },
            };
            this.ListType = listType;
        }
    }
}