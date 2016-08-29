using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Data.Objects;
using System.IO;
using System.Threading;
using eRoute.Models;
using DMSERoute.Helpers;
using System.Net;
using System.Configuration;
using DevExpress.Web.Mvc;
using System.Web.UI.WebControls;
using System.Drawing.Printing;
using DevExpress.Web;


namespace eRoute.Controllers
{
    [LogAndRedirectOnError]
    public class EvaluationController : Controller
    {
        private static EvalAutoMarkMV EvalAutoMarkMV;

        private string StorageRoot
        {
            get {
                var settingName = Global.Context.CustomSettings.Where(t => t.SettingCode == "InventoryItemPath").Select(t => t.SettingName).FirstOrDefault();
                var settingValue = Global.Context.CustomSettings.Where(t => t.SettingCode == "InventoryItemPath").Select(t => t.SettingValue).FirstOrDefault();
                var fullPath = "~" + settingValue + settingName;
                return Path.Combine(Server.MapPath(fullPath));
            }
        }

        #region Các Function củ cần phải 
        //FROM JAVASCRIPT
        public void SearchDetailByInput(FilterDataModel FilterData)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            FilterData.UpdateToAppropriateData();
            profileData.filterData = FilterData;
            Session["UserProfile"] = profileData;

            string str = FilterData.CombineFilterDateToString();

            switch (FilterData.inputScreenID)
            {
                case "VSC001": //"EvaluationDefinition":
                    profileData.isLoadMasterInfo = false;
                    profileData.isLoadHeaderInfo = false;
                    profileData.isLoadDetailInfo = true;
                    Session["UserProfile"] = profileData;
                    break;
                case "VSC002": //"DetailEvaluationDefinitionView":
                    break;
                case "VSC003": //"DetailEvaluationDefinition":
                    break;
                case "VSC005": //"OutletImageEvaluation":
                    profileData.isLoadMasterInfo = false;
                    profileData.isLoadHeaderInfo = true;
                    profileData.isLoadDetailInfo = true;
                    Session["UserProfile"] = profileData;
                    break;
                case "VSC006": //"DetailOutletImageEvaluation":
                    break;
                case "VSC007": //"OutletImageReview":
                    profileData.isLoadMasterInfo = false;
                    profileData.isLoadHeaderInfo = true;
                    profileData.isLoadDetailInfo = true;
                    Session["UserProfile"] = profileData;
                    break;
                case "VSC008": //"DetailOutletImageReview":
                    break;
                case "VSC009": //"EvaluationBaseline":
                    break;
                case "VSC009R": //"EvaluationBaselineView":
                    break;
                case "VSC010": //"AutoEvaluation":
                    break;
                default:
                    break;
            }
        }

        public void LoadParticipantListForEvaluation(string DSNhanVienDuocChon)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            List<uvw_GetUserInformation> PrevSelectedUserList = new List<uvw_GetUserInformation>(profileProgram.SelectedUserList);
            //string[] ParticipantList = DSNhanVienDuocChon.Split(';');
            profileProgram.SelectedUserList.Clear();
            List<string> ParticipantList = DSNhanVienDuocChon.Split(';').ToList();
            foreach (string item in ParticipantList)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string[] value = item.Split('-');
                    uvw_GetUserInformation ins = new uvw_GetUserInformation();
                    ins = profileProgram.AllUserList.Where(user => user.MaNhanVien == value[1].ToString()).FirstOrDefault();
                    //ins.VaiTro = Convert.ToInt32(value[0].ToString());                   
                    profileProgram.SelectedUserList.Add(ins);
                }
            }

            // profileProgram.SelectedUserList = new List<uvw_GetUserInformation>();
            //profileProgram.SelectedUserList = profileProgram.AllUserList.Where(user => Array.IndexOf(ParticipantList, user.MaNhanVien) >= 0).ToList();

            //Voi nhung nhan vien da chon vai tro thi giu lai thong tin vai tro
            if (PrevSelectedUserList.Count > 0)
            {
                for (int i = 0; i < profileProgram.SelectedUserList.Count; i++)
                {
                    int index = PrevSelectedUserList.IndexOf(PrevSelectedUserList.Where(p => p.MaNhanVien == profileProgram.SelectedUserList[i].MaNhanVien).FirstOrDefault());
                    if (index >= 0)
                    {
                        profileProgram.SelectedUserList[i].VaiTro = PrevSelectedUserList[index].VaiTro;
                    }
                }
            }
            Session["ProgramProfile"] = profileProgram;
        }

        public string CancelEvaluationAction(string sEvalID)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;

            profileData.ScreenID = "VSC002";
            profileData.filterData.inputEvaluationID = sEvalID.Trim();
            Session["UserProfile"] = profileData;

            Global.VisibilityContext.usp_CancelEvaluationByID(profileData.filterData.inputEvaluationID);

            string str = "Đã hủy kỳ đánh giá " + profileData.filterData.inputEvaluationID + " thành công";

            return str;
        }

        public ActionResult ItemListByReferenceNbr()
        {
            var ProgramProfile = this.Session["ProgramProfile"] as EvaluationDataModel;
            string Eval = ProgramProfile.EvaluationID;
            List<usp_GetListInventoryItemsResult> ListOfItemByRefNum = Global.VisibilityContext.usp_GetListInventoryItems(Eval).ToList();
            ViewData["ItemInformation"] = ListOfItemByRefNum;

            return PartialView("ItemListByReferenceNbr");
        }

        public void LoadItemListByRefNumber(string sinputRef)
        {
            // Called from SCREEN VSC003: "Chi Tiết Kỳ Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC003";
            profileData.filterData.inputReferenceCD = sinputRef.Trim();
            Session["UserProfile"] = profileData;
        }

        public ActionResult UserListBySelection()
        {
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            ViewData["SelectedUserInformation"] = profileProgram.SelectedUserList;
            ViewData["RolesInformation"] = profileProgram.RolesList;
            return PartialView("UserListBySelection");
        }

        public string ModifyParticipants(string DanhSachUserID, string DanhSachUserRole)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;

            Global.VisibilityContext.usp_ModifiyParticipants(profileData.filterData.inputEvaluationID, DanhSachUserID, DanhSachUserRole, profileData.LoginID, profileData.ScreenID);

            string str = "Thay đổi danh sách nhân viên thành công";

            return str;
        }


        //public ActionResult LoadDataToDistributeScreen(string sEvalID)
        //{
        //    // Called from SCREEN VSC004: "Phân Bổ Cửa Hàng cho Nhân Viên"
        //    var profileData = this.Session["UserProfile"] as SessionDataModel;
        //    profileData.ScreenID = "VSC004";
        //    profileData.filterData.inputEvaluationID = sEvalID;

        //    Session["UserProfile"] = profileData;

        //    return RedirectToAction("DistributeOutletToAuditor");

        //}


        public void FilterOutletImageEvalByStatus(string AllStatus)
        {
            // Called from SCREEN VSC005: "Đánh Giá Hình Ảnh"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC005";
            profileData.StatusFilter = AllStatus.Trim();

            Session["UserProfile"] = profileData;

        }
        public ActionResult LoadDataToEvaluationScreen(string sEvalID)
        {
            // Called from SCREEN VSC005: "Đánh Giá Hình Ảnh"
            var profileData = this.Session["UserProfile"] as SessionDataModel;

            profileData.ScreenID = "VSC005";
            profileData.filterData.inputEvaluationID = sEvalID;

            Session["UserProfile"] = profileData;

            return RedirectToAction("OutletImageEvaluation");

        }

        public ActionResult ChangeViewTypeInDetailOutletImageEvaluationOrReview(string sViewType)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;

            profileData.ViewType = sViewType;
            Session["UserProfile"] = profileData;

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            profileProgram.selectedEvaluationID = "";
            Session["ProgramProfile"] = profileProgram;

            if (profileData.ScreenID == "VSC006")
                return RedirectToAction("DetailOutletImageEvaluation");
            if (profileData.ScreenID == "VSC008")
                return RedirectToAction("DetailOutletImageReview");

            return View();
        }
        public void GetAllDataForEvaluationAndReview(string ProcessType)
        {
            // Called from SCREEN VSC005: "Đánh Giá Hình Ảnh" and VSC007: "Xét Duyệt Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            string FilterParm = profileData.filterData.CombineFilterDateToString();

            if (profileProgram.EvalOutletsList.Count <= 0)
            {
                if (ProcessType == "VSC007") // Man hình review
                {
                    var listOutletRevew = Global.VisibilityContext.ufn_GetOutletInforByScreenID(profileProgram.EvaluationID, profileProgram.selectedAuditorID, ProcessType).ToList();
                    foreach (var item in listOutletRevew)
                    {
                        OutletDataModel OneOutlet = new OutletDataModel();
                        OneOutlet.OutletID = item.MaCuaHang;
                        OneOutlet.OutletName = item.TenCuaHang;

                        profileProgram.EvalOutletsList.Add(OneOutlet);
                    }
                }
                else
                {
                    profileProgram.OutletList = Global.VisibilityContext.ufn_GetOutletInforByID(profileProgram.EvaluationID, profileProgram.selectedAuditorID).ToList();
                }


            }

            int index = profileProgram.EvalOutletsList.IndexOf(profileProgram.EvalOutletsList.Where(p => p.OutletID == profileProgram.currentOutletID).FirstOrDefault());
            profileProgram.idxOutlet = index;
            profileProgram.nextOutletID = profileProgram.EvalOutletsList[(profileProgram.idxOutlet + 1) % profileProgram.EvalOutletsList.Count].OutletID;
            profileProgram.nextOutletName = profileProgram.EvalOutletsList[(profileProgram.idxOutlet + 1) % profileProgram.EvalOutletsList.Count].OutletName;

            if (profileProgram.idxOutlet < 0) profileProgram.idxOutlet = 0;

            OutletDataModel CurrentOutlet = profileProgram.EvalOutletsList[profileProgram.idxOutlet];
            CurrentOutlet.AvatarImageIDName = CurrentOutlet.AvatarImageLocation + Global.VisibilityContext.ufn_GetOutletAvatarImageByCustomerID(CurrentOutlet.OutletID);
            CurrentOutlet.AvatarImageIDDate = "";
            if (!CurrentOutlet.hasData)
            {
                List<usp_GetOutletImageDataByIDResult> query = Global.VisibilityContext.usp_GetOutletImageDataByID(profileProgram.selectedEvaluationID, profileData.LoginID, CurrentOutlet.OutletID, profileData.ViewType).ToList();
                for (int i = 0; i < query.Count; i++)
                {
                    ImageDataModel OneImage = new ImageDataModel();
                    OneImage.ImageID = query[i].OutletImageID;
                    OneImage.ImageIDName = OneImage.EvalImageLocation + query[i].FolderLocation + query[i].ImageFileName;
                    OneImage.ImageIDDate = query[i].CapturedDate;
                    OneImage.ImageIDEvaluated = "0";
                    OneImage.ImageIDReviewed = "0";
                    OneImage.SalemanName = query[i].SalesmanName;
                    OneImage.ImageIDEvalUserName = query[i].FullName;
                    OneImage.ImageIDEvalDate = query[i].NgayChamDiem;
                    OneImage.CaptureDistance = query[i].CapturedDistance;
                    OneImage.ComparedImageID = 0;
                    OneImage.ComparedImageIDName = OneImage.EvalImageLocation + query[i].AnhGocChup;
                    OneImage.ComparedImageIDDate = query[i].NgayAnhGocChup;

                    OneImage.ListLatestComparedImages = Global.VisibilityContext.usp_GetComparedImageDataByOutletID(profileProgram.selectedEvaluationID, CurrentOutlet.OutletID).ToList();
                    OneImage.UpdateImageFileName();

                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isMatchedWithBefore).ToString());
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isCaptured).ToString());
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isAccepted).ToString());
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isPassed).ToString());
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isFinished).ToString());

                    OneImage.ImageIDEvalReason.Add(query[i].Reason1.Trim() != "" ? query[i].Reason1.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason2.Trim() != "" ? query[i].Reason2.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason3.Trim() != "" ? query[i].Reason3.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add("0");

                    OneImage.ListNumericItemExists = query[i].DSNumericChamDiem;

                    OneImage.ImageIDEvalStatus = query[i].EvalStatus;
                    OneImage.ImageIDRevStatus = query[i].RevStatus;

                    CurrentOutlet.EvalImagesList.Add(OneImage);
                    profileProgram.EvalOutletsList[profileProgram.idxOutlet] = CurrentOutlet;

                }

                CurrentOutlet.hasData = true;
            } // cap lai session
            else
            {
                List<usp_GetOutletImageDataByIDResult> query = Global.VisibilityContext.usp_GetOutletImageDataByID(profileProgram.selectedEvaluationID, profileData.LoginID, CurrentOutlet.OutletID, profileData.ViewType).ToList();
                for (int i = 0; i < query.Count; i++)
                {
                    ImageDataModel OneImage = new ImageDataModel();
                    OneImage.ImageID = query[i].OutletImageID;
                    OneImage.ImageIDName = OneImage.EvalImageLocation + query[i].FolderLocation + query[i].ImageFileName;
                    OneImage.ImageIDDate = query[i].CapturedDate;
                    OneImage.ImageIDEvaluated = "0";
                    OneImage.ImageIDReviewed = "0";
                    OneImage.SalemanName = query[i].SalesmanName;
                    OneImage.ImageIDEvalUserName = query[i].FullName;
                    OneImage.ImageIDEvalDate = query[i].NgayChamDiem;
                    OneImage.CaptureDistance = query[i].CapturedDistance;
                    OneImage.ComparedImageID = 0;
                    OneImage.ComparedImageIDName = OneImage.EvalImageLocation + query[i].AnhGocChup;
                    OneImage.ComparedImageIDDate = query[i].NgayAnhGocChup;

                    OneImage.ListLatestComparedImages = Global.VisibilityContext.usp_GetComparedImageDataByOutletID(profileProgram.selectedEvaluationID, CurrentOutlet.OutletID).ToList();
                    OneImage.UpdateImageFileName();

                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isMatchedWithBefore).ToString());
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isCaptured).ToString());
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isAccepted).ToString());
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isPassed).ToString());
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isFinished).ToString());

                    OneImage.ImageIDEvalReason.Add(query[i].Reason1.Trim() != "" ? query[i].Reason1.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason2.Trim() != "" ? query[i].Reason2.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason3.Trim() != "" ? query[i].Reason3.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add("0");

                    OneImage.ListNumericItemExists = query[i].DSNumericChamDiem;

                    OneImage.ImageIDEvalStatus = query[i].EvalStatus;
                    OneImage.ImageIDRevStatus = query[i].RevStatus;

                    CurrentOutlet.EvalImagesList.RemoveAll(rs => rs.ImageID == OneImage.ImageID);
                    CurrentOutlet.EvalImagesList.Add(OneImage);
                    profileProgram.EvalOutletsList[profileProgram.idxOutlet] = CurrentOutlet;

                }
            }

            Session["UserProfile"] = profileData;
            Session["ProgramProfile"] = profileProgram;
        }
        public void SaveEvaluationResultIntoCurrentImageID(string ListNumericChamDiem,
                                                            string Reason1, string Reason2, string Reason3, string ReviewReason,
                                                            string isMatchedWithBefore, string isCaptured, string isAccepted, string isPassed)
        {
            // Called from SCREEN VSC006: "Chi Tiết Đánh Giá Hình Ảnh"
            if (Session["UserProfile"] == null || Session["ProgramProfile"] == null) return;

            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            int idxOutlet = profileProgram.idxOutlet;
            int selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;

            if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList.Count <= 0) return;

            profileData.ScreenID = "VSC006";
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[0] = isMatchedWithBefore;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[1] = isCaptured;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[2] = isAccepted;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[3] = isPassed;

            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[0] = Reason1;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[1] = Reason2;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[2] = Reason3;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[3] = ReviewReason;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvaluated = "1";

            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ListNumericItemExists = ListNumericChamDiem;

            Session["UserProfile"] = profileData;
            Session["ProgramProfile"] = profileProgram;
        }

        public ActionResult LoadNextDataToDetailOutletImageEvaluationScreen(string nextOutletID, string nextOutletName, int nextImageIndex, string funtion)
        {
            // Called from SCREEN VSC005: "Đánh Giá Hình Ảnh"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC005";
            //profileData.ImageIndex = nextImageIndex;
            Session["UserProfile"] = profileData;

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            profileProgram.selectedOutletName = nextOutletName;
            profileProgram.selectedOutletID = nextOutletID;
            profileProgram.OutletList = profileProgram.OutletList.OrderBy(x => x.TenCuaHang).ToList();
            profileProgram.EvalOutletsList = profileProgram.EvalOutletsList.OrderBy(x => x.OutletName).ToList();
            if (funtion == "next")
            {
                int idxOutlet = profileProgram.OutletList.FindIndex(x => x.MaCuaHang == nextOutletID);
                profileProgram.idxOutlet = idxOutlet;
                profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx = nextImageIndex;
                profileProgram.currentOutletID = profileProgram.OutletList[profileProgram.idxOutlet % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.currentOutletName = profileProgram.OutletList[profileProgram.idxOutlet % profileProgram.OutletList.Count].TenCuaHang;
                profileProgram.nextOutletID = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.nextOutletName = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].TenCuaHang;
            }
            else if (funtion == "back") // neu back
            {
                int idxOutlet = profileProgram.OutletList.FindIndex(x => x.MaCuaHang == profileProgram.currentOutletID);
                if (idxOutlet != 0) // 
                {
                    profileProgram.idxOutlet = idxOutlet;
                    profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx = nextImageIndex;
                    profileProgram.currentOutletID = profileProgram.OutletList[(profileProgram.idxOutlet - 1) % profileProgram.OutletList.Count].MaCuaHang;
                    profileProgram.currentOutletName = profileProgram.OutletList[(profileProgram.idxOutlet - 1) % profileProgram.OutletList.Count].TenCuaHang;
                    profileProgram.nextOutletID = profileProgram.OutletList[(profileProgram.idxOutlet) % profileProgram.OutletList.Count].MaCuaHang;
                    profileProgram.nextOutletName = profileProgram.OutletList[(profileProgram.idxOutlet) % profileProgram.OutletList.Count].TenCuaHang;
                }
                else // back ve cua hang dau tien. cho no ve cua hang cuoi cung.
                {
                    int count = profileProgram.OutletList.Count() - 1;
                    profileProgram.idxOutlet = count;
                    profileProgram.nextOutletID = profileProgram.currentOutletID;
                    profileProgram.nextOutletName = profileProgram.currentOutletName;
                    profileProgram.currentOutletID = profileProgram.OutletList[(profileProgram.idxOutlet) % profileProgram.OutletList.Count].MaCuaHang;
                    profileProgram.currentOutletName = profileProgram.OutletList[(profileProgram.idxOutlet) % profileProgram.OutletList.Count].TenCuaHang;

                }
            }
            else if (funtion == "first") // quay ve dong dau tien
            {
                profileProgram.idxOutlet = 0;
                profileProgram.currentOutletID = profileProgram.OutletList[(profileProgram.idxOutlet) % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.currentOutletName = profileProgram.OutletList[(profileProgram.idxOutlet) % profileProgram.OutletList.Count].TenCuaHang;
                profileProgram.nextOutletID = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.nextOutletName = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].TenCuaHang;
            }
            else if (funtion == "last") // quay ve dong dau tien
            {
                int count = profileProgram.OutletList.Count() - 1;
                profileProgram.idxOutlet = count;

                profileProgram.currentOutletID = profileProgram.OutletList[(profileProgram.idxOutlet) % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.currentOutletName = profileProgram.OutletList[(profileProgram.idxOutlet) % profileProgram.OutletList.Count].TenCuaHang;

                profileProgram.nextOutletID = profileProgram.OutletList[(0) % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.nextOutletName = profileProgram.OutletList[(0) % profileProgram.OutletList.Count].TenCuaHang;

            }
            else
            {
                int idxOutlet = profileProgram.OutletList.FindIndex(x => x.MaCuaHang == nextOutletID);
                profileProgram.idxOutlet = idxOutlet;
                profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx = nextImageIndex;
                profileProgram.currentOutletID = profileProgram.OutletList[profileProgram.idxOutlet % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.currentOutletName = profileProgram.OutletList[profileProgram.idxOutlet % profileProgram.OutletList.Count].TenCuaHang;
                profileProgram.nextOutletID = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.nextOutletName = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].TenCuaHang;
            }
            Session["ProgramProfile"] = profileProgram;
            return Json(new { selectedEvaluationID = profileProgram.EvaluationID, selectedAuditorID = profileProgram.selectedAuditorID, currentOutletID = profileProgram.currentOutletID, currentOutletName = profileProgram.currentOutletName }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult SaveEvaluationImageDataToDatabase()
        {
            // Called from SCREEN VSC006: "Chi Tiết Đánh Giá Hình Ảnh"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            profileData.ScreenID = "VSC006";
            var list = profileProgram.EvalOutletsList.Where(x => x.OutletID == profileProgram.currentOutletID).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                string ListOfImageID = "";
                string ListOfKQChamDiem = "";
                string ListOfLyDoChamDiem = "";
                string ListOfNumericChamDiem = "";
                string ListOfDaChamDiem = "";
                string EvalID = "";
                string AuditorID = "";
                string OutletID = "";
                // for (int j = 0; j < list[i].EvalImagesList.Count; j++)
                //{
                //    if (list[i].hasData)
                //    {
                //        EvalID = profileProgram.selectedEvaluationID;
                //        AuditorID = profileProgram.selectedAuditorID;
                //        OutletID = list[i].OutletID;

                //        ListOfImageID = ListOfImageID + list[i].EvalImagesList[j].ImageID + "&";
                //        ListOfKQChamDiem = ListOfKQChamDiem + string.Join(";", list[i].EvalImagesList[j].ImageIDEvalResult.ToArray()) + ";&";
                //        ListOfLyDoChamDiem = ListOfLyDoChamDiem + string.Join(";", list[i].EvalImagesList[j].ImageIDEvalReason.ToArray()) + ";&";
                //        ListOfNumericChamDiem = ListOfNumericChamDiem + list[i].EvalImagesList[j].ListNumericItemExists + "&";
                //        ListOfDaChamDiem = ListOfDaChamDiem + list[i].EvalImagesList[j].ImageIDEvaluated + "&";
                //    }
                //}

                int ideximage = list[i].selectedImageIdx;
                if (list[i].hasData)
                {
                    EvalID = profileProgram.selectedEvaluationID;
                    AuditorID = profileProgram.selectedAuditorID;
                    OutletID = list[i].OutletID;

                    ListOfImageID = ListOfImageID + list[i].EvalImagesList[ideximage].ImageID + "&";
                    ListOfKQChamDiem = ListOfKQChamDiem + string.Join(";", list[i].EvalImagesList[ideximage].ImageIDEvalResult.ToArray()) + ";&";
                    ListOfLyDoChamDiem = ListOfLyDoChamDiem + string.Join(";", list[i].EvalImagesList[ideximage].ImageIDEvalReason.ToArray()) + ";&";
                    ListOfNumericChamDiem = ListOfNumericChamDiem + list[i].EvalImagesList[ideximage].ListNumericItemExists + "&";
                    ListOfDaChamDiem = ListOfDaChamDiem + list[i].EvalImagesList[ideximage].ImageIDEvaluated + "&";
                }

                if (ListOfImageID != "")
                    Global.VisibilityContext.usp_SaveEvaluationImageResult(EvalID, AuditorID, OutletID, ListOfImageID, ListOfKQChamDiem, ListOfLyDoChamDiem, ListOfNumericChamDiem, ListOfDaChamDiem);
                // n gay mai cap nhap vo .
            }

            Session["UserProfile"] = profileData;
            Session["ProgramProfile"] = profileProgram;

            return RedirectToAction("DetailOutletImageEvaluation");
        }

        public void GoToDetailOutletImageReview(string selectedEvaluationID, string selectedAuditorID)
        {
            // Called from SCREEN VSC007: "Xét Duyệt Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC007";
            Session["UserProfile"] = profileData;

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            profileProgram.selectedEvaluationID = selectedEvaluationID.Trim();
            profileProgram.selectedAuditorID = selectedAuditorID.Trim();
            profileProgram.selectedOutletName = "";
            profileProgram.selectedOutletID = "";

            profileProgram.OutletList = Global.VisibilityContext.ufn_GetOutletInforByID(profileProgram.EvaluationID, profileProgram.selectedAuditorID).ToList();

            if (profileProgram.OutletList.Count > 0)
            {
                profileProgram.idxOutlet = 0;

                profileProgram.currentOutletID = profileProgram.OutletList[profileProgram.idxOutlet % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.currentOutletName = profileProgram.OutletList[profileProgram.idxOutlet % profileProgram.OutletList.Count].TenCuaHang;
                profileProgram.nextOutletID = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].MaCuaHang;
                profileProgram.nextOutletName = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].TenCuaHang;
            }

            Session["ProgramProfile"] = profileProgram;

            string result = "Finish Process";

        }
        public void LoadNextDataToDetailOutletImageReviewScreen(string nextOutletID, string nextOutletName, int nextImageIndex)
        {
            // Called from SCREEN VSC007: "Xét Duyệt Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC007";
            //profileData.ImageIndex = nextImageIndex;
            Session["UserProfile"] = profileData;

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            profileProgram.selectedOutletName = nextOutletName;
            profileProgram.selectedOutletID = nextOutletID;
            int idxOutlet = profileProgram.OutletList.FindIndex(x => x.MaCuaHang == nextOutletID); ;
            profileProgram.idxOutlet = idxOutlet;
            profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx = nextImageIndex;

            profileProgram.currentOutletID = profileProgram.OutletList[profileProgram.idxOutlet % profileProgram.OutletList.Count].MaCuaHang;
            profileProgram.currentOutletName = profileProgram.OutletList[profileProgram.idxOutlet % profileProgram.OutletList.Count].TenCuaHang;
            profileProgram.nextOutletID = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].MaCuaHang;
            profileProgram.nextOutletName = profileProgram.OutletList[(profileProgram.idxOutlet + 1) % profileProgram.OutletList.Count].TenCuaHang;

            Session["ProgramProfile"] = profileProgram;

        }

        public ActionResult SaveReviewImageDataToDatabase()
        {

            // Called from SCREEN VSC008: "Chi Tiết Xét Duyệt Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            profileData.ScreenID = "VSC008";

            for (int i = 0; i < profileProgram.EvalOutletsList.Count; i++)
            {
                string ListOfImageID = "";
                string ListOfReviewResult = "";
                string ListOfReviewReason = "";
                string ListOfDaChamDiem = "";
                string EvalID = "";
                string AuditorID = "";
                string OutletID = "";

                //for (int j = 0; j < profileProgram.EvalOutletsList[i].EvalImagesList.Count; j++)
                //{
                //    if (profileProgram.EvalOutletsList[i].hasData)
                //    {
                //        EvalID = profileProgram.selectedEvaluationID;
                //        AuditorID = profileProgram.selectedAuditorID;
                //        OutletID = profileProgram.EvalOutletsList[i].OutletID;

                //        ListOfImageID = ListOfImageID + profileProgram.EvalOutletsList[i].EvalImagesList[j].ImageID + "&";
                //        ListOfReviewResult = ListOfReviewResult + profileProgram.EvalOutletsList[i].EvalImagesList[j].ImageIDEvalResult[4] + "&";
                //        ListOfReviewReason = ListOfReviewReason + profileProgram.EvalOutletsList[i].EvalImagesList[j].ImageIDEvalReason[3] + "&";
                //        ListOfDaChamDiem = ListOfDaChamDiem + profileProgram.EvalOutletsList[i].EvalImagesList[j].ImageIDReviewed + "&";
                //    }
                //}
                int ideximage = profileProgram.EvalOutletsList[i].selectedImageIdx;
                if (profileProgram.EvalOutletsList[i].hasData)
                {
                    EvalID = profileProgram.selectedEvaluationID;
                    AuditorID = profileProgram.selectedAuditorID;
                    OutletID = profileProgram.EvalOutletsList[i].OutletID;

                    ListOfImageID = ListOfImageID + profileProgram.EvalOutletsList[i].EvalImagesList[ideximage].ImageID + "&";
                    ListOfReviewResult = ListOfReviewResult + profileProgram.EvalOutletsList[i].EvalImagesList[ideximage].ImageIDEvalResult[4] + "&";
                    ListOfReviewReason = ListOfReviewReason + profileProgram.EvalOutletsList[i].EvalImagesList[ideximage].ImageIDEvalReason[3] + "&";
                    ListOfDaChamDiem = ListOfDaChamDiem + profileProgram.EvalOutletsList[i].EvalImagesList[ideximage].ImageIDReviewed + "&";
                }

                if (ListOfImageID != "")
                    Global.VisibilityContext.usp_SaveReviewImageResult(EvalID, AuditorID, OutletID, ListOfImageID, ListOfReviewResult, ListOfReviewReason, ListOfDaChamDiem, profileData.LoginID);
            }


            Session["UserProfile"] = profileData;
            Session["ProgramProfile"] = profileProgram;
            //return View();
            //DetailOutletImageReview
            return RedirectToAction("DetailOtletImageReview", "Evaluation");
        }
        public void SaveReviewResultIntoCurrentImageID(string ImageReviewResult, string ImageReviewReason, string sOutletID, string sImageIndex)
        {
            // Called from SCREEN VSC008: "Chi Tiết Xét Duyệt Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            profileData.ScreenID = "VSC008";
            int idxOutlet = profileProgram.idxOutlet;
            int selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;

            if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList.Count <= 0) return;

            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[4] = ImageReviewResult;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[3] = ImageReviewReason;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDReviewed = "1";

            //int iOutletIndex = profileProgram.DSOutletID.FindIndex(x => x == sOutletID);
            //int iImageIndex = int.Parse(sImageIndex);
            //profileProgram.ListOutLetImages[iOutletIndex].DSReviewResult[iImageIndex] = ImageReviewResult.Trim();
            //profileProgram.ListOutLetImages[iOutletIndex].DSReviewReason[iImageIndex] = ImageReviewReason.Trim();

            Session["UserProfile"] = profileData;
        }

        public string BaseLineProcess(string ListEvaluationForBaseline, string BaselineByDate, string BaselineByWeek,
                                        string BaselineByMonth, string BaselineByDateFrom, string BaselineByDateTo,
                                        string BaselineByWeekNo, string BaselineByWeek_MonthYear, string BaselineByMonthNo)
        {
            if (ListEvaluationForBaseline == "")
                return Utility.Phrase("You must choose at least one Evaluation");

            // Called from SCREEN VSC009: "Baseline Kỳ Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;

            profileData.ScreenID = "VSC009";
            string str = String.Empty;

            string BaselineInformation = String.Empty, isBaselined = String.Empty;

            Global.VisibilityContext.usp_GetBaselineInfoFromEvaluationID(ListEvaluationForBaseline, BaselineByDate, BaselineByWeek, BaselineByMonth, BaselineByDateFrom, BaselineByDateTo, BaselineByWeekNo, BaselineByWeek_MonthYear, BaselineByMonthNo, ref BaselineInformation, ref isBaselined);

            if (isBaselined == "1")
            {
                Global.VisibilityContext.usp_BaselineProcess(ListEvaluationForBaseline, BaselineByDate, BaselineByWeek, BaselineByMonth, BaselineByDateFrom, BaselineByDateTo, BaselineByWeekNo, BaselineByWeek_MonthYear, BaselineByMonthNo, profileData.LoginID, profileData.ScreenID);
                str = Utility.Phrase("Successfully Evaluation Baseline: \n") + ListEvaluationForBaseline; //"Đã Baseline Thành Công";
            }
            else
                str = Utility.Phrase("Can not baseline because evaluations still have some unreviewed images in the baseline period : \n") + BaselineInformation;

            Session["UserProfile"] = profileData;
            return str;
        }

        public void BaseLinedView(string ListEvaluationForBaseline)
        {
            // Called from SCREEN VSC009R: "Báo Cáo Baseline Kỳ Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;

            profileData.CheckedData = ListEvaluationForBaseline.Trim();
            profileData.DSEvaluationID = ListEvaluationForBaseline.Trim();

            Session["UserProfile"] = profileData;
        }
        public void BackToTheMainPage(string EvalID)
        {
            // Called from SCREEN VSC009R: "Báo Cáo Baseline Kỳ Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;

            profileData.filterData.inputEvaluationID = EvalID;

            Session["UserProfile"] = profileData;
        }

        public bool URLExists(string url)
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

        //FROM CONTROLLER

        public ActionResult GetEvaluationInfo(string DisplayID)
        {
            List<usp_GetEvaluationInfoByTypeResult> model = new List<usp_GetEvaluationInfoByTypeResult>();
            if (SessionHelper.GetSession<FilterModel>("FilterModel") != null)
            {
                FilterModel filterModel = SessionHelper.GetSession<FilterModel>("FilterModel");
                if (filterModel.EvaluationData != null)
                {
                    model = filterModel.EvaluationData.Where(x => string.IsNullOrEmpty(DisplayID) || x.ProgramID == DisplayID).ToList();
                }
            }
            return PartialView("EvaluationOptionPartial", model);
        }
        public FilterModel SetDataModelFilter(string DisplayID = null, string DisplayCode = null, string EvaluationCode = null, string EvalState = null, string EvalType = null, string Content = null, string EvalDesc = null, string DispFromDate = null, string DispToDate = null, string EvalFromDate = null, string EvalToDate = null, string RefNumber = null)
        {
            FilterModel data = new FilterModel();
            data.DisplayID = String.IsNullOrEmpty(DisplayID) ? "" : DisplayID;
            data.Display = String.IsNullOrEmpty(DisplayCode) ? "" : DisplayCode;
            data.EvaluationID = String.IsNullOrEmpty(EvaluationCode) ? "" : EvaluationCode.Trim();
            data.Status = String.IsNullOrEmpty(EvalState) ? 0 : Int32.Parse(EvalState);
            data.Type = EvalType;
            data.Content = String.IsNullOrEmpty(Content) ? "" : Content;
            data.ReferenceCD = String.IsNullOrEmpty(RefNumber) ? "" : RefNumber;
            data.Description = String.IsNullOrEmpty(EvalDesc) ? "" : DisplayCode;
            DMSEvaluation evalobj = new DMSEvaluation();
            if (!String.IsNullOrEmpty(EvaluationCode))
            {
                evalobj = Global.VisibilityContext.DMSEvaluations.Where(x => x.EvaluationID == EvaluationCode.Trim()).FirstOrDefault();
                data.Status = evalobj.EvalState;
                data.Type = evalobj.EvalType;
            }

            if (!String.IsNullOrEmpty(DispFromDate))
            {
                data.DisplayFromDate = Utility.DateTimeParse(DispFromDate);
            }
            else
            {
                if (evalobj.EvaluationID != null)
                {
                    data.DisplayFromDate = evalobj.ProgramDateFrom;
                }
                else
                {
                    data.DisplayFromDate = null;
                }
            }
            if (!String.IsNullOrEmpty(DispToDate))
            {
                data.DisplayToDate = Utility.DateTimeParse(DispToDate);
            }
            else
            {
                if (evalobj.EvaluationID != null)
                {
                    data.DisplayToDate = evalobj.ProgramDateTo;
                }
                else
                {
                    data.DisplayToDate = null;
                }
            }

            if (!String.IsNullOrEmpty(EvalFromDate))
            {
                data.EvalFromDate = Utility.DateTimeParse(EvalFromDate);
            }
            else
            {
                if (evalobj.EvaluationID != null)
                {
                    data.EvalFromDate = evalobj.EvalDateFrom;
                }
                else
                {
                    data.EvalFromDate = null;
                }
            }

            if (!String.IsNullOrEmpty(EvalToDate))
            {
                data.EvalToDate = Utility.DateTimeParse(EvalToDate);
            }
            else
            {
                if (evalobj.EvaluationID != null)
                {
                    data.EvalToDate = evalobj.EvalDateTo;
                }
                else
                {
                    data.EvalToDate = null;
                }
            }
            data.DisplayData = Global.VisibilityContext.uvw_GetDisplayInformations.ToList();
            data.DisplayData.Insert(0, new uvw_GetDisplayInformation { STT = "000", MaCTTB = "", ChuongTrinhTrungBay = "", MoTa = "" });
            int RoleID = SessionHelper.GetSession<int>("RoleUser");
            if (RoleID == (int)Utility.RoleName.Leader || RoleID == (int)Utility.RoleName.Auditor)
            {
                data.UserID = SessionHelper.GetSession<string>("UserName");
            }
            data.EvaluationData = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "", "", data.UserID).ToList();

            List<SelectListItem> listStatus = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("SelectAll"), Value = "0", Selected = (data.Status == 0) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Evaluating"), Value = "1", Selected = (data.Status == 1) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Completed"), Value = "2", Selected = (data.Status == 2) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Cancelled"), Value = "3", Selected = (data.Status == 3) ? true : false }
            };
            data.listStatus = listStatus;
            //Man hinh danh sach ki danh gia
            List<SelectListItem> listStatusAs = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("SelectAll"), Value = "0", Selected = (data.Status == 0) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_1"), Value = "1", Selected = (data.Status == 1) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_2"), Value = "2", Selected = (data.Status == 2) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_3"), Value = "3", Selected = (data.Status == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_4"), Value = "4", Selected = (data.Status == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_5"), Value = "5", Selected = (data.Status == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_6"), Value = "6", Selected = (data.Status == 3) ? true : false },
                new SelectListItem() { Text = Utility.Phrase("EvalState_7"), Value = "7", Selected = (data.Status == 3) ? true : false },
			};
            data.listStatusAs = listStatusAs;
            List<SelectListItem> listType = new List<SelectListItem>() {
                new SelectListItem() { Text = Utility.Phrase("SelectAll"), Value = "0", Selected = (data.Type == "0") ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Automatic"), Value = "A", Selected = (data.Type == "A") ? true : false },
                new SelectListItem() { Text = Utility.Phrase("Manual"), Value = "M", Selected = (data.Type == "M") ? true : false }
            };
            data.listType = listType;

            return data;
        }


        public void InitSessionData()
        {
            string LoginID = SessionHelper.GetSession<string>("UserName");

            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            if (profileData == null)
            {
                profileData = new SessionDataModel();
                profileData.LoginID = LoginID;
                Session["UserProfile"] = profileData;
            }

            if (profileProgram == null)
            {
                profileProgram = new EvaluationDataModel();
                profileProgram.selectedEvaluationID = "";
                profileProgram.DisplayList = Global.VisibilityContext.uvw_GetDisplayInformations.ToList();
                Session["ProgramProfile"] = profileProgram;
            }

            //Global.VisibilityContext.usp_CheckAndUpdateEvalMasterTables();
        }

        public bool CheckDataInput(string ViewName)
        {
            bool success = true;
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            if (profileData == null) { success = false; return success; };
            if (profileProgram == null) { success = false; return success; };

            switch (ViewName)
            {
                case "EvaluationDefinition":
                    break;
                case "DetailEvaluationDefinitionView":
                    break;
                case "DetailEvaluationDefinition":
                    break;
                case "DistributeOutletToAuditor":
                    break;
                case "OutletImageEvaluation":
                    break;
                case "DetailOutletImageEvaluation":
                    if (profileProgram.selectedOutletID == "") success = false;
                    break;
                case "OutletImageReview":
                    break;
                case "DetailOutletImageReview":
                    if (profileProgram.selectedAuditorID == "") success = false;
                    break;
                case "EvaluationBaseline":
                    break;
                case "EvaluationBaselineView":
                    break;
                case "AutoEvaluation":
                    break;
                default:
                    break;
            }

            return success;
        }

        public void AssignSessionData(string ViewName)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            profileData.PrevScreenID = profileData.ScreenID;
            switch (ViewName)
            {
                case "EvaluationDefinition":
                    profileData.ScreenID = "VSC001";
                    profileData.ViewName = "EvaluationDefinition";

                    if (profileData.PrevScreenID != profileData.ScreenID)
                    {
                        profileData.filterData.inputEvaluationID = ""; //Reset Filter Information when go to another screen
                        profileProgram.ResetHeaderInformation();
                    }

                    break;
                case "DetailEvaluationDefinitionView":
                    profileData.ScreenID = "VSC002";
                    profileData.ViewName = "DetailEvaluationDefinitionView";
                    break;
                case "DetailEvaluationDefinition":
                    profileData.ScreenID = "VSC003";
                    profileData.ViewName = "DetailEvaluationDefinition";
                    break;
                case "OutletImageEvaluation":
                    profileData.ScreenID = "VSC005";
                    profileData.ViewName = "OutletImageEvaluation";

                    if (profileData.PrevScreenID != profileData.ScreenID)
                    {
                        profileData.filterData.inputEvaluationID = ""; //Reset Filter Information when go to another screen
                        profileProgram.ResetHeaderInformation();
                    }

                    break;
                case "DetailOutletImageEvaluation":
                    profileData.ScreenID = "VSC006";
                    profileData.ViewName = "DetailOutletImageEvaluation";
                    break;
                case "OutletImageReview":
                    profileData.ScreenID = "VSC007";
                    profileData.ViewName = "OutletImageReview";

                    if (profileData.PrevScreenID != profileData.ScreenID)
                    {
                        profileData.filterData.inputEvaluationID = ""; //Reset Filter Information when go to another screen
                        profileProgram.ResetHeaderInformation();
                    }

                    break;
                case "DetailOutletImageReview":
                    profileData.ScreenID = "VSC008";
                    profileData.ViewName = "DetailOutletImageReview";
                    break;
                case "EvaluationBaseline":
                    profileData.ScreenID = "VSC009";
                    profileData.ViewName = "EvaluationBaseline";
                    break;
                case "EvaluationBaselineView":
                    profileData.ScreenID = "VSC009R";
                    profileData.ViewName = "EvaluationBaselineView";

                    if (profileData.PrevScreenID != profileData.ScreenID)
                    {
                        profileData.filterData.inputEvaluationID = ""; //Reset Filter Information when go to another screen
                        profileProgram.ResetHeaderInformation();
                    }

                    break;
                case "AutoEvaluation":
                    profileData.ScreenID = "VSC010";
                    profileData.ViewName = "AutoEvaluation";

                    if (profileData.PrevScreenID != profileData.ScreenID)
                    {
                        profileData.filterData.inputEvaluationID = ""; //Reset Filter Information when go to another screen
                        profileProgram.ResetHeaderInformation();
                    }

                    break;
                default:
                    break;
            }

            Session["UserProfile"] = profileData;
        }

        public void LoadHeaderInformation(string ViewName)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            switch (ViewName)
            {
                case "DetailEvaluationDefinitionView":
                    if (profileData.isLoadMasterInfo)
                        profileProgram.EvaluationList = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "", "", "").ToList();

                    if (profileData.isLoadHeaderInfo)
                    {
                        profileProgram.EvaluationID = profileData.filterData.inputEvaluationID;
                        Global.VisibilityContext.usp_GetEvaluationInfoByID(profileProgram.EvaluationID, ref profileProgram.sMaDanhGia, ref profileProgram.sMaThamChieu, ref profileProgram.sChuongTrinhTrungBay, ref profileProgram.sThoiGianTrungBayFrom, ref profileProgram.sThoiGianTrungBayTo, ref profileProgram.sThoiGianDanhGiaFrom, ref profileProgram.sThoiGianDanhGiaTo, ref profileProgram.sMoTaCTTB, ref profileProgram.sNoiDungCTTB, ref profileProgram.sLoaiHinhDanhGia, ref profileProgram.sKieuTGDanhGia, ref profileProgram.sTyLeReview, ref profileProgram.sTrangThaiDanhGia);
                        Global.VisibilityContext.usp_GetEvaluationMoreInfoByID(profileProgram.EvaluationID, ref profileProgram.sisNumeric, ref profileProgram.sLoaiKhuVuc, ref profileProgram.sDanhSachKhuVucID);
                    }
                    profileProgram.EvaluationList = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "", "", "").ToList();
                    break;

                case "OutletImageReview":
                    profileProgram.EvaluationID = profileData.filterData.inputEvaluationID;

                    if (profileData.isLoadHeaderInfo)
                    {
                        profileProgram.EvaluationList = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "", "", "").ToList();
                        if (!string.IsNullOrEmpty(profileProgram.EvaluationID))
                            Global.VisibilityContext.usp_GetEvaluationInfoByID(profileProgram.EvaluationID, ref profileProgram.sMaDanhGia, ref profileProgram.sMaThamChieu, ref profileProgram.sChuongTrinhTrungBay, ref profileProgram.sThoiGianTrungBayFrom, ref profileProgram.sThoiGianTrungBayTo, ref profileProgram.sThoiGianDanhGiaFrom, ref profileProgram.sThoiGianDanhGiaTo, ref profileProgram.sMoTaCTTB, ref profileProgram.sNoiDungCTTB, ref profileProgram.sLoaiHinhDanhGia, ref profileProgram.sKieuTGDanhGia, ref profileProgram.sTyLeReview, ref profileProgram.sTrangThaiDanhGia);

                        profileProgram.EvalOutletsList = new List<OutletDataModel>();
                    }

                    break;
                case "DetailOutletImageReview":
                    profileProgram.ReasonList = new List<usp_GetDisplayReasonResult>();
                    if (profileProgram.OutletList.Count <= 0)
                        profileProgram.OutletList = Global.VisibilityContext.ufn_GetOutletInforByID(profileProgram.EvaluationID, profileProgram.selectedAuditorID).ToList();
                    break;
                case "EvaluationBaseline":
                    profileProgram.EvaluationList = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "", "", "").ToList();

                    if (profileProgram.EvaluationID != null && profileProgram.EvaluationID != "")
                    {
                        Global.VisibilityContext.usp_GetEvaluationInfoByID(profileProgram.EvaluationID, ref profileProgram.sMaDanhGia, ref profileProgram.sMaThamChieu, ref profileProgram.sChuongTrinhTrungBay, ref profileProgram.sThoiGianTrungBayFrom, ref profileProgram.sThoiGianTrungBayTo, ref profileProgram.sThoiGianDanhGiaFrom, ref profileProgram.sThoiGianDanhGiaTo, ref profileProgram.sMoTaCTTB, ref profileProgram.sNoiDungCTTB, ref profileProgram.sLoaiHinhDanhGia, ref profileProgram.sKieuTGDanhGia, ref profileProgram.sTyLeReview, ref profileProgram.sTrangThaiDanhGia);
                    }
                    break;
                case "EvaluationBaselineView":
                    profileProgram.EvaluationList = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "", "", "").ToList();

                    profileProgram.EvaluationID = profileData.filterData.inputEvaluationID;
                    if (profileProgram.EvaluationID != null && profileProgram.EvaluationID != "")
                    {
                        Global.VisibilityContext.usp_GetEvaluationInfoByID(profileProgram.EvaluationID, ref profileProgram.sMaDanhGia, ref profileProgram.sMaThamChieu, ref profileProgram.sChuongTrinhTrungBay, ref profileProgram.sThoiGianTrungBayFrom, ref profileProgram.sThoiGianTrungBayTo, ref profileProgram.sThoiGianDanhGiaFrom, ref profileProgram.sThoiGianDanhGiaTo, ref profileProgram.sMoTaCTTB, ref profileProgram.sNoiDungCTTB, ref profileProgram.sLoaiHinhDanhGia, ref profileProgram.sKieuTGDanhGia, ref profileProgram.sTyLeReview, ref profileProgram.sTrangThaiDanhGia);
                    }
                    break;
                case "AutoEvaluation":
                    profileProgram.EvaluationList = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "A", "", "").ToList();

                    if (profileData.filterData.inputEvaluationID == null)
                        profileData.filterData.inputEvaluationID = "";
                    profileProgram.EvaluationID = profileData.filterData.inputEvaluationID;

                    if (profileProgram.EvaluationID != null && profileProgram.EvaluationID != "")
                    {
                        Global.VisibilityContext.usp_GetEvaluationInfoByID(profileProgram.EvaluationID, ref profileProgram.sMaDanhGia, ref profileProgram.sMaThamChieu, ref profileProgram.sChuongTrinhTrungBay, ref profileProgram.sThoiGianTrungBayFrom, ref profileProgram.sThoiGianTrungBayTo, ref profileProgram.sThoiGianDanhGiaFrom, ref profileProgram.sThoiGianDanhGiaTo, ref profileProgram.sMoTaCTTB, ref profileProgram.sNoiDungCTTB, ref profileProgram.sLoaiHinhDanhGia, ref profileProgram.sKieuTGDanhGia, ref profileProgram.sTyLeReview, ref profileProgram.sTrangThaiDanhGia);
                    }
                    break;

                default:
                    break;
            }

            Session["ProgramProfile"] = profileProgram;
            Session["UserProfile"] = profileData;
        }

        public void LoadDetailInformation(string ViewName)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            string FilterParm = profileData.filterData.CombineFilterDateToString();

            switch (ViewName)
            {
                case "DetailEvaluationDefinitionView":
                    if (profileData.isLoadDetailInfo)
                    {
                        //profileProgram.GeoologyList = Global.VisibilityContext.ufn_GetGeoologyInforTree().ToList().OrderByDescending(o => o.Cap).ToList();
                        profileProgram.GeoologyList = Global.VisibilityContext.usp_GetGeoologyInforTree_New("", null, null).ToList().OrderByDescending(o => o.Cap).ToList();
                        //profileProgram.SaleOrgList = Global.VisibilityContext.ufn_GetSaleOrgInforTree("8").ToList().OrderByDescending(o => o.Cap).ToList();
                        //profileProgram.ListItem = Global.VisibilityContext.usp_GetItemInforByID(profileProgram.EvaluationID).ToList();

                        if (profileProgram.UserList.Count <= 0)
                            profileProgram.UserList = Global.VisibilityContext.usp_GetUserInformationByID(profileProgram.EvaluationID, "0").ToList();

                        if (profileProgram.AllUserList.Count <= 0)
                        {
                            profileProgram.AllUserList = Global.VisibilityContext.uvw_GetUserInformations.ToList();

                        }
                        else
                        {
                            //Hieu cap nhat du lieu duoi luoi lên chọn nhân viên.
                            foreach (var item in profileProgram.AllUserList)
                            {
                                //uvw_GetUserInformation ins = new uvw_GetUserInformation();
                                //ins = item;
                                usp_GetUserInformationByIDResult profile = profileProgram.UserList.Find(f => f.EvalUserID == item.MaNhanVien);
                                if (profile != null)
                                    item.VaiTro = profile.VaiTro;
                                //profileProgram.AllUserList.Add(ins);
                            }
                        }

                        if (profileProgram.RolesList != null)
                        {
                            if (profileProgram.RolesList.Count <= 0)
                                profileProgram.RolesList = Global.VisibilityContext.uvw_GetRolesInformations.ToList();
                        }
                        else
                        {
                            profileProgram.RolesList = Global.VisibilityContext.uvw_GetRolesInformations.ToList();
                        }
                        profileProgram.UpdateTerritoryChecked();
                    }
                    break;
                case "DetailEvaluationDefinition":
                    //profileProgram.GeoologyList = Global.VisibilityContext.ufn_GetGeoologyInforTree().ToList().OrderByDescending(o => o.Cap).ToList();
                    profileProgram.GeoologyList = Global.VisibilityContext.usp_GetGeoologyInforTree_New("", null, null).ToList().OrderByDescending(o => o.Cap).ToList();
                    //profileProgram.SaleOrgList = Global.VisibilityContext.ufn_GetSaleOrgInforTree("8").ToList().OrderByDescending(o => o.Cap).ToList();
                    profileProgram.AllUserList = Global.VisibilityContext.uvw_GetUserInformations.ToList();
                    // Hieu Add 2016-02-07 add list role 
                    profileProgram.RolesList = Global.VisibilityContext.uvw_GetRolesInformations.ToList();

                    break;
                case "DistributeOutletToAuditor":
                    break;
                case "OutletImageEvaluation":
                    if (profileData.isLoadDetailInfo)
                        //profileProgram.OutletEvalDetailList = Global.VisibilityContext.usp_GetOutletsImageEvaluationByFilter(FilterParm, profileData.StatusFilter).ToList();

                        if (profileProgram.OutletEvalDetailList.Count > 0)
                        {
                            profileProgram.idxOutlet = 0;
                            profileProgram.currentOutletID = profileProgram.OutletEvalDetailList[profileProgram.idxOutlet % profileProgram.OutletEvalDetailList.Count].MaCuaHang;
                            profileProgram.currentOutletName = profileProgram.OutletEvalDetailList[profileProgram.idxOutlet % profileProgram.OutletEvalDetailList.Count].TenCuaHang;
                            profileProgram.nextOutletID = profileProgram.OutletEvalDetailList[(profileProgram.idxOutlet + 1) % profileProgram.OutletEvalDetailList.Count].MaCuaHang;
                            profileProgram.nextOutletName = profileProgram.OutletEvalDetailList[(profileProgram.idxOutlet + 1) % profileProgram.OutletEvalDetailList.Count].TenCuaHang;
                        }

                    break;
                case "DetailOutletImageEvaluation":
                    GetAllDataForEvaluationAndReview("Evaluation");
                    profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel; //Goi lai vi da duoc cap nhat trong ham tren
                    break;
                case "OutletImageReview":
                    //if (profileData.isLoadDetailInfo)
                    //    //profileProgram.ReviewList = Global.VisibilityContext.ufn_GetOutletsImageReviewByID(profileProgram.EvaluationID).ToList();
                    //    profileProgram.ReviewList = Global.VisibilityContext.usp_GetOutletsImageReviewByFilter_Param(FilterParm).ToList();
                    break;
                case "DetailOutletImageReview":
                    GetAllDataForEvaluationAndReview("VSC007");
                    profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel; //Goi lai vi da duoc cap nhat trong ham tren
                    break;
                case "EvaluationBaseline":
                    if (profileData.filterData.inputEvaluationID == null)
                        profileData.filterData.inputEvaluationID = "";
                    profileProgram.EvaluationID = profileData.filterData.inputEvaluationID;
                    profileProgram.BaselineList = Global.VisibilityContext.ufn_GetBaselineEvaluationByID(profileData.filterData.inputEvaluationID).ToList();
                    break;
                case "EvaluationBaselineView":
                    profileProgram.ReportGeneralEvaluationList = Global.VisibilityContext.usp_ReportGeneralEvaluation(profileData.DSEvaluationID).ToList();
                    profileProgram.ReportDetailEvaluationList = Global.VisibilityContext.usp_ReportDetailEvaluation(profileData.DSEvaluationID).ToList();
                    profileProgram.ReportNumericEvaluationList = Global.VisibilityContext.usp_ReportNumericEvaluation(profileData.DSEvaluationID).ToList();
                    break;
                case "AutoEvaluation":
                    profileProgram.Init_GetAutoProccessingData();
                    profileProgram.Update_GetAutoProccessingData();

                    break;
                default:
                    break;
            }

            Session["ProgramProfile"] = profileProgram;
            Session["UserProfile"] = profileData;
        }

        public void UpdateSessionData(string ViewName)
        {
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;

            switch (ViewName)
            {
                case "EvaluationDefinition":
                    break;
                case "DetailEvaluationDefinitionView":
                    break;
                case "DetailEvaluationDefinition":
                    break;
                case "DistributeOutletToAuditor":
                    //Used For To Check Already Distribution or Not
                    profileData.CheckedData = "0";
                    ViewBag.SumOutlet = 0;
                    if (profileProgram.UserList.Count > 0)
                    {
                        profileData.CheckedData = (profileProgram.UserList[0].SoOutletChamDiem > 0) ? "1" : "0";
                    }
                    break;
                case "OutletImageEvaluation":
                    //profileProgram.OutletImageData = null; // De chay lai ham GetAllDataForMainProcess
                    break;
                case "DetailOutletImageEvaluation":
                    //profileProgram.ImageIndex = profileData.ImageIndex;
                    break;
                case "OutletImageReview":
                    //profileProgram.OutletImageData = null; // De chay lai ham GetAllDataForMainProcess
                    break;
                case "DetailOutletImageReview":
                    break;
                case "EvaluationBaseline":
                    break;
                case "EvaluationBaselineView":
                    break;
                case "AutoEvaluation":
                    break;
                default:
                    break;
            }

            Session["UserProfile"] = profileData;
            Session["ProgramProfile"] = profileProgram;
        }

        public void UpdateStatusOfImage(string ViewName)
        {
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            int idxOutlet = profileProgram.idxOutlet;
            int selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;

            if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList.Count <= 0) return;

            switch (ViewName)
            {
                case "DetailOutletImageEvaluation":
                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus == "1")
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvaluated = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus = Utility.Phrase("Eval_JustEvaluated");//"Vừa Chấm";
                    }
                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus == "2")
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvaluated = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus = Utility.Phrase("Eval_Evaluated");//"Vừa Chấm";
                    }
                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus == "3")
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvaluated = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus = Utility.Phrase("Eval_Approved");
                    }
                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus == "4"//"Đã Review-Reject" 
                                                                                                                           //||
                                                                                                                           //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus == "Chưa Chấm Lại")
                        )
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvaluated = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus = Utility.Phrase("Eval_NotReEvaluate");
                    }

                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus == "5"
                        )
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvaluated = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus = Utility.Phrase("Eval_Rejected");
                    }
                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus == "6"
                        )
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvaluated = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalStatus = Utility.Phrase("Eval_Approved");
                    }
                    break;
                case "DetailOutletImageReview":
                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDRevStatus == "1" //"Chưa Review"
                )
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDReviewed = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDRevStatus = Utility.Phrase("Eval_JustReviewed"); //"Vừa Review";
                    }
                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDRevStatus == "2" //""
               )
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDReviewed = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDRevStatus = Utility.Phrase("Eval_Approved");
                    }
                    if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDRevStatus == "3")
                    //"Đã Chấm Lại" ||
                    // profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDRevStatus == "Đã Review Lại-Reject")
                    {
                        profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDReviewed = "1";
                        //profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDRevStatus =
                        //    Utility.Phrase("Eval_JustReReviewed");//"Vừa Review Lại";
                    }

                    break;
                default:
                    break;
            }

            Session["ProgramProfile"] = profileProgram;
        }

        public void UpdateListOfItem(string ViewName)
        {
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            CustomLog.LogError("---id:" + profileProgram.idxOutlet);
            int idxOutlet = profileProgram.idxOutlet;
            string customerImageID = string.Empty;
            int selectedImageIdx = 0;
            if (profileProgram.EvalOutletsList != null && profileProgram.EvalOutletsList[idxOutlet] != null)
            {
                selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;
            }

            if (profileProgram.EvalOutletsList != null
                && profileProgram.EvalOutletsList[idxOutlet] != null
                && profileProgram.EvalOutletsList[idxOutlet].EvalImagesList != null)
            {
                CustomLog.LogError("-- list:" + profileProgram.EvalOutletsList.Count);
                CustomLog.LogError("-- listsdsdsdsdsd:" + profileProgram.EvalOutletsList[idxOutlet].EvalImagesList.Count);
                customerImageID = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageID.ToString();
            }
            CustomLog.LogError("Eval :" + profileProgram.selectedEvaluationID);
            CustomLog.LogError("custom :" + customerImageID);
            switch (ViewName)
            {
                case "DetailOutletImageEvaluation":
                    profileProgram.ListItem = Global.VisibilityContext.usp_GetItemInforByID(profileProgram.selectedEvaluationID, customerImageID).ToList();
                    break;
                case "DetailOutletImageReview":
                    profileProgram.ListItem = Global.VisibilityContext.usp_GetItemInforByID(profileProgram.selectedEvaluationID, customerImageID).ToList();
                    break;
                default:
                    break;
            }
            CustomLog.LogError("ListItem :" + profileProgram.ListItem.Count);

            Session["ProgramProfile"] = profileProgram;
        }
        // FROM VIEW 
        //VSC001: "Định Nghĩa Kỳ Đánh Giá"


        //SCREEN VSC002: "Xem Chi Tiết Kỳ Đánh Giá" <-- Button Chi Tiet trên View "Định Nghĩa Kỳ Đánh Giá"

        //SCREEN VSC003: "Chi Tiết Kỳ Đánh Giá" <-- Button Them Moi trên View "Định Nghĩa Kỳ Đánh Giá"
        [Authorize]
        [ActionAuthorize("Evaluation_DetailEvaluationDefinition")]
        public ActionResult DetailEvaluationDefinition()
        {
            InitSessionData();
            if (!CheckDataInput("DetailEvaluationDefinition")) return View();
            AssignSessionData("DetailEvaluationDefinition");

            LoadHeaderInformation("DetailEvaluationDefinition");
            LoadDetailInformation("DetailEvaluationDefinition");
            UpdateSessionData("DetailEvaluationDefinition");

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            //View Bag
            ViewBag.PageInformation = Utility.Phrase("Detail Evaluation Definition"); //"Chi Tiết Kỳ Đánh Giá";
            ViewBag.SetMenuImageEvaluationActive = "active";
            ViewBag.DataModel = profileProgram;

            var profileData = this.Session["UserProfile"] as SessionDataModel;
            ViewBag.ScreenID = profileData.ScreenID;
            ViewBag.UserID = profileData.LoginID;

            //View Data
            ViewData["EvaluationInformation"] = null;
            ViewData["DisplayInformation"] = profileProgram.DisplayList;
            ViewData["GeoInformation"] = profileProgram.GeoologyList;
            ViewData["UserInformation"] = profileProgram.AllUserList;
            ViewData["RolesInformation"] = profileProgram.RolesList;

            return View();
        }



        // SCREEN VSC005: "Đánh Giá Hình Ảnh"
        #region  OutletImageEvaluation

        [HttpGet]
        public JsonResult GetOutlet()
        {
            string LoginID = SessionHelper.GetSession<string>("UserName");
            string EvaluationID = SessionHelper.GetSession<string>("EvaluationID");
            //ufn_GetOutletInforByIDResult

            //  Global.VisibilityContext.ufn_GetOutletInforByID(profileProgram.EvaluationID, profileProgram.selectedAuditorID).ToList();
            var obj = (from doi in Global.VisibilityContext.ufn_GetOutletInforByID(EvaluationID, LoginID)

                       select new { doi.MaCuaHang, doi.TenCuaHang }).Distinct().OrderBy(x => x.TenCuaHang);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        #endregion


        // SCREEN VSC006: "Chi Tiết Đánh Giá Hình Ảnh" <-- Button Chi Tiet tren View: "Đánh Giá Hình Ảnh"
        [Authorize]
        [ActionAuthorize("Evaluation_DetailOutletImageEvaluation")]
        public ActionResult DetailOutletImageEvaluation(string selectedEvaluationID, string selectedAuditorID, string selectedOutletID, string selectedOutletName)
        {
            //
            // Called from SCREEN VSC005: "Đánh Giá Hình Ảnh"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC006";
            Session["UserProfile"] = profileData;

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            if (!string.IsNullOrEmpty(selectedEvaluationID))
            {
                profileProgram.selectedEvaluationID = selectedEvaluationID.Trim();
                profileProgram.EvaluationID = selectedEvaluationID.Trim();
                profileProgram.selectedOutletName = selectedOutletName.Trim();
                profileProgram.selectedOutletID = selectedOutletID.Trim();
                profileProgram.currentOutletID = selectedOutletID.Trim();
                profileProgram.currentOutletName = selectedOutletName.Trim();
                profileProgram.selectedAuditorID = selectedAuditorID.Trim();
            }
            // Lay danh sach outlet  
            if (profileProgram.EvalOutletsList.Count <= 0)
            {

                profileProgram.OutletList = Global.VisibilityContext.ufn_GetOutletInforByID(profileProgram.EvaluationID, profileProgram.selectedAuditorID).ToList();
                foreach (var item in profileProgram.OutletList)
                {
                    OutletDataModel OneOutlet = new OutletDataModel();
                    OneOutlet.OutletID = item.MaCuaHang;
                    OneOutlet.OutletName = item.TenCuaHang;
                    profileProgram.EvalOutletsList.Add(OneOutlet);
                }
            }
            int index = profileProgram.EvalOutletsList.IndexOf(profileProgram.EvalOutletsList.Where(p => p.OutletID == profileProgram.currentOutletID).FirstOrDefault());
            profileProgram.idxOutlet = index;
            profileProgram.nextOutletID = profileProgram.EvalOutletsList[(profileProgram.idxOutlet + 1) % profileProgram.EvalOutletsList.Count].OutletID;
            profileProgram.nextOutletName = profileProgram.EvalOutletsList[(profileProgram.idxOutlet + 1) % profileProgram.EvalOutletsList.Count].OutletName;

            if (profileProgram.idxOutlet < 0) profileProgram.idxOutlet = 0;

            OutletDataModel CurrentOutlet = profileProgram.EvalOutletsList[profileProgram.idxOutlet];
            CurrentOutlet.AvatarImageIDName = CurrentOutlet.AvatarImageLocation + Global.VisibilityContext.ufn_GetOutletAvatarImageByCustomerID(CurrentOutlet.OutletID);
            CurrentOutlet.AvatarImageIDDate = "";
            #region GetDataImageList
            if (!CurrentOutlet.hasData)
            {
                List<usp_GetOutletImageDataByIDResult> query = Global.VisibilityContext.usp_GetOutletImageDataByID(profileProgram.selectedEvaluationID, profileData.LoginID, CurrentOutlet.OutletID, profileData.ViewType).ToList();
                for (int i = 0; i < query.Count; i++)
                {
                    ImageDataModel OneImage = new ImageDataModel();
                    OneImage.ImageID = query[i].OutletImageID;
                    OneImage.ImageIDName = OneImage.EvalImageLocation + query[i].FolderLocation + query[i].ImageFileName;
                    OneImage.ImageIDDate = query[i].CapturedDate;
                    OneImage.ImageIDEvaluated = "0";
                    OneImage.ImageIDReviewed = "0";
                    OneImage.SalemanName = query[i].SalesmanName;
                    OneImage.ImageIDEvalUserName = query[i].FullName;
                    OneImage.ImageIDEvalDate = query[i].NgayChamDiem;
                    OneImage.CaptureDistance = query[i].CapturedDistance;
                    OneImage.ComparedImageID = 0;
                    OneImage.ComparedImageIDName = OneImage.EvalImageLocation + query[i].AnhGocChup;
                    OneImage.ComparedImageIDDate = query[i].NgayAnhGocChup;

                    OneImage.ListLatestComparedImages = Global.VisibilityContext.usp_GetComparedImageDataByOutletID(profileProgram.selectedEvaluationID, CurrentOutlet.OutletID).ToList();
                    OneImage.UpdateImageFileName();
                    if (query[i].isMatchedWithBefore == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isMatchedWithBefore).ToString());
                    }
                    if (query[i].isCaptured == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isCaptured).ToString());
                    }
                    if (query[i].isAccepted == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isAccepted).ToString());
                    }
                    if (query[i].isPassed == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isPassed).ToString());
                    }

                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isFinished).ToString());
                    OneImage.ImageIDEvalReason.Add(query[i].Reason1.Trim() != "" ? query[i].Reason1.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason2.Trim() != "" ? query[i].Reason2.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason3.Trim() != "" ? query[i].Reason3.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add("0");

                    OneImage.ListNumericItemExists = query[i].DSNumericChamDiem;

                    OneImage.ImageIDEvalStatus = query[i].EvalStatus;
                    OneImage.ImageIDRevStatus = query[i].RevStatus;

                    CurrentOutlet.EvalImagesList.Add(OneImage);
                    profileProgram.EvalOutletsList[profileProgram.idxOutlet] = CurrentOutlet;

                }

                CurrentOutlet.hasData = true;
            } // cap lai session
            else
            {
                List<usp_GetOutletImageDataByIDResult> query = Global.VisibilityContext.usp_GetOutletImageDataByID(profileProgram.selectedEvaluationID, profileData.LoginID, CurrentOutlet.OutletID, profileData.ViewType).ToList();
                for (int i = 0; i < query.Count; i++)
                {
                    ImageDataModel OneImage = new ImageDataModel();
                    OneImage.ImageID = query[i].OutletImageID;
                    OneImage.ImageIDName = OneImage.EvalImageLocation + query[i].FolderLocation + query[i].ImageFileName;
                    OneImage.ImageIDDate = query[i].CapturedDate;
                    OneImage.ImageIDEvaluated = "0";
                    OneImage.ImageIDReviewed = "0";
                    OneImage.SalemanName = query[i].SalesmanName;
                    OneImage.ImageIDEvalUserName = query[i].FullName;
                    OneImage.ImageIDEvalDate = query[i].NgayChamDiem;
                    OneImage.CaptureDistance = query[i].CapturedDistance;
                    OneImage.ComparedImageID = 0;
                    OneImage.ComparedImageIDName = OneImage.EvalImageLocation + query[i].AnhGocChup;
                    OneImage.ComparedImageIDDate = query[i].NgayAnhGocChup;

                    OneImage.ListLatestComparedImages = Global.VisibilityContext.usp_GetComparedImageDataByOutletID(profileProgram.selectedEvaluationID, CurrentOutlet.OutletID).ToList();
                    OneImage.UpdateImageFileName();
                    if (query[i].isMatchedWithBefore == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isMatchedWithBefore).ToString());
                    }
                    if (query[i].isCaptured == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isCaptured).ToString());
                    }
                    if (query[i].isAccepted == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isAccepted).ToString());
                    }
                    if (query[i].isPassed == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isPassed).ToString());
                    }
                    OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isFinished).ToString());

                    OneImage.ImageIDEvalReason.Add(query[i].Reason1.Trim() != "" ? query[i].Reason1.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason2.Trim() != "" ? query[i].Reason2.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason3.Trim() != "" ? query[i].Reason3.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add("0");

                    OneImage.ListNumericItemExists = query[i].DSNumericChamDiem;

                    OneImage.ImageIDEvalStatus = query[i].EvalStatus;
                    OneImage.ImageIDRevStatus = query[i].RevStatus;

                    CurrentOutlet.EvalImagesList.RemoveAll(rs => rs.ImageID == OneImage.ImageID);
                    CurrentOutlet.EvalImagesList.Add(OneImage);
                    profileProgram.EvalOutletsList[profileProgram.idxOutlet] = CurrentOutlet;

                }
            }
            #endregion

            Session["ProgramProfile"] = profileProgram;
            InitSessionData();
            if (!CheckDataInput("DetailOutletImageEvaluation")) return RedirectToAction("OutletImageEvaluation");
            AssignSessionData("DetailOutletImageEvaluation");
            LoadHeaderInformation("DetailOutletImageEvaluation");
            //UpdateSessionData("DetailOutletImageEvaluation");
            UpdateListOfItem("DetailOutletImageEvaluation");
            //View Bag
            ViewBag.PageInformation = Utility.Phrase("DetailOutletImageEvaluation"); //"Chi Tiết Đánh Giá Hình Ảnh";
            ViewBag.SetMenuImageEvaluationActive = "active";
            ViewBag.ViewType = profileData.ViewType;
            ViewBag.DataModel = profileProgram;
            //View Data
            int idxOutlet = profileProgram.idxOutlet;
            int selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;

            //if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList.Count <= 0) return RedirectToAction("OutletImageEvaluation");
            #region AssdataView
            DetailOutletImageClass data = new DetailOutletImageClass();
            data.ScreenID = profileData.ScreenID;
            data.UserID = profileData.LoginID;
            data.OutletIndex = idxOutlet;
            data.isMatchedWithBefore = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[0];
            data.isCaptured = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[1];
            data.isAccepted = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[2];
            data.isPassed = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[3];
            data.isFinished = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[4];

            data.Reason1 = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[0];
            data.Reason2 = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[1];
            data.Reason3 = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[2];
            data.Reason4 = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[3];

            data.DSKQChamDiem = "1;1;1;1;";
            data.DSLyDoChamDiem = "0;0;0;0;";
            data.DSNumericChamDiem = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ListNumericItemExists;

            data.currentOutletID = profileProgram.currentOutletID;
            data.currentOutletName = profileProgram.currentOutletName;
            data.selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;
            data.nextOutletID = profileProgram.nextOutletID;
            data.nextOutletName = profileProgram.nextOutletName;
            data.ComparedImageIDName = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ComparedImageIDName;
            data.ImageIDName = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDName;
            data.ImageIDDate = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDDate;
            data.ListLatestComparedImages = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ListLatestComparedImages;
            data.AvatarImageIDName = profileProgram.EvalOutletsList[idxOutlet].AvatarImageIDName;
            data.SalemanName = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].SalemanName;
            data.CaptureDistance = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].CaptureDistance;
            if (!string.IsNullOrEmpty(profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalDate))
            {
                data.ImageIDEvalDate = Convert.ToDateTime(profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalDate).ToShortDateString();
            }
            else
            {
                data.ImageIDEvalDate = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalDate;
            }
            data.ImageIDEvalUserName = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalUserName;
            data.EvalImagesList = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList;

            ViewData["Data"] = data;
            #endregion


            // View Data
            ViewData["OutletInformation"] = profileProgram.OutletList;
            ViewData["ReasonInformation"] = profileProgram.ReasonList;
            ViewData["ItemInformationByID"] = profileProgram.ListItem;
            ViewBag.ScreenID = profileData.ScreenID;
            DMSEvaluation eval = Global.VisibilityContext.DMSEvaluations.Where(x => x.EvaluationID == profileProgram.selectedEvaluationID).FirstOrDefault();
            if (eval != null)
            {
                profileProgram.sisNumeric = eval.isNumeric;
                if (profileProgram.sisNumeric == "0")
                {
                    ViewBag.btndisabled = Constant.ViewButton;
                }
                else
                {
                    ViewBag.btndisabled = string.Empty;
                }
            }
            else
            {
                ViewBag.btndisabled = Constant.ViewButton;
            }

            SessionHelper.SetSession<string>("EvaluationID", profileProgram.EvaluationID);
            return View();
        }

        // SCREEN VSC008: "Chi Tiết Xét Duyệt Đánh Giá" <-- Button Chi Tiet tren View: "Xét Duyệt Đánh Giá"

        // SCREEN VSC009: "Baseline Kỳ Đánh Giá"
        [Authorize]
        [ActionAuthorize("Evaluation_EvaluationBaseline", true)]
        public ActionResult EvaluationBaseline(string Reset)
        {
            InitSessionData();

            if (!CheckDataInput("EvaluationBaseline")) return View();
            AssignSessionData("EvaluationBaseline");

            LoadHeaderInformation("EvaluationBaseline");
            LoadDetailInformation("EvaluationBaseline");
            UpdateSessionData("EvaluationBaseline");

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            //View Bag
            ViewBag.PageInformation = Utility.Phrase("Evaluation Baseline"); //"Baseline Kỳ Đánh Giá";
            ViewBag.SetMenuImageEvaluationActive = "active";
            ViewBag.DataModel = profileProgram;

            var profileData = this.Session["UserProfile"] as SessionDataModel;
            ViewBag.ScreenID = profileData.ScreenID;
            ViewBag.UserID = profileData.LoginID;

            //View Data
            ViewData["EvaluationInformation"] = profileProgram.EvaluationList;
            ViewData["DisplayInformation"] = profileProgram.DisplayList;
            ViewData["BaselineInformation"] = profileProgram.BaselineList;

            return View();
        }

        // SCREEN VSC009R: "Báo Cáo Baseline Kỳ Đánh Giá" <-- Button Báo Cáo tren View: "Baseline Kỳ Đánh Giá"
        [Authorize]
        [ActionAuthorize("Evaluation_EvaluationBaselineView")]
        public ActionResult EvaluationBaselineView()
        {
            InitSessionData();

            if (!CheckDataInput("EvaluationBaselineView")) return View();
            AssignSessionData("EvaluationBaselineView");

            LoadHeaderInformation("EvaluationBaselineView");
            LoadDetailInformation("EvaluationBaselineView");
            UpdateSessionData("EvaluationBaselineView");

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            //View Bag
            ViewBag.PageInformation = Utility.Phrase("Evaluation Baseline View"); //"Báo Cáo Baseline Kỳ Đánh Giá";
            ViewBag.SetMenuImageEvaluationActive = "active";
            ViewBag.DataModel = profileProgram;

            var profileData = this.Session["UserProfile"] as SessionDataModel;
            ViewBag.ScreenID = profileData.ScreenID;
            ViewBag.UserID = profileData.LoginID;

            //View Data
            ViewData["EvaluationInformation"] = profileProgram.EvaluationList;
            ViewData["DisplayInformation"] = profileProgram.DisplayList;
            ViewData["ReportGeneralEvaluationList"] = profileProgram.ReportGeneralEvaluationList;
            ViewData["ReportDetailEvaluationList"] = profileProgram.ReportDetailEvaluationList;
            ViewData["ReportNumericEvaluationList"] = profileProgram.ReportNumericEvaluationList;


            if (profileData.CheckedData == "")
                ViewData["BaselinedInfo"] = Global.VisibilityContext.ufn_GetBaselinedInfoByID(profileData.filterData.inputEvaluationID).ToList();
            else
                ViewData["BaselinedInfo"] = Global.VisibilityContext.ufn_GetBaselinedInfoByID(profileData.CheckedData).ToList();

            return View();
        }
        #endregion

        //Quản lý định nghĩa đánh giá hình ảnh
        #region Evaluation Definition
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Evaluation_EvaluationDefinition", true)]
        public ActionResult EvaluationDefinition()
        {
            EvaluationDataModel model = new EvaluationDataModel();
            if (SessionHelper.GetSession<FilterModel>("FilterModel") != null)
            {
                model.FilterModel = SessionHelper.GetSession<FilterModel>("FilterModel");
            }
            else
            {
                model.FilterModel = SetDataModelFilter();
            }
            model.EvalDefinitionResult = Global.VisibilityContext.usp_GetEvaluationDetailInfoByFilter_Param(model.FilterModel.EvaluationID.Trim(), model.FilterModel.ReferenceCD, model.FilterModel.DisplayID, model.FilterModel.Description, model.FilterModel.Content, model.FilterModel.DisplayFromDate, model.FilterModel.DisplayToDate, model.FilterModel.EvalFromDate, model.FilterModel.EvalToDate, model.FilterModel.ScreenID, model.FilterModel.UserID, model.FilterModel.Status, model.FilterModel.Type).ToList();
            Session["FilterModel"] = model.FilterModel;
            InitSessionData();
            return View(model);
        }
        [HttpPost]
        [Authorize]
        [ActionAuthorize("Evaluation_EvaluationDefinition", true)]
        public ActionResult EvaluationDefinition(string DisplayID, string DisplayCode, string EvaluationCode, string EvalState, string EvalType, string EvalContent, string EvalDesc, string DispFromDate, string DispToDate, string EvalFromDate, string EvalToDate, string RefNumber)
        {
            EvaluationDataModel model = new EvaluationDataModel();
            model.FilterModel = SetDataModelFilter(DisplayID, DisplayCode, EvaluationCode, EvalState, EvalType, EvalContent, EvalDesc, DispFromDate, DispToDate, EvalFromDate, EvalToDate, RefNumber);
            model.EvalDefinitionResult = Global.VisibilityContext.usp_GetEvaluationDetailInfoByFilter_Param(model.FilterModel.EvaluationID, model.FilterModel.ReferenceCD, model.FilterModel.DisplayID, model.FilterModel.Description, model.FilterModel.Content, model.FilterModel.DisplayFromDate, model.FilterModel.DisplayToDate, model.FilterModel.EvalFromDate, model.FilterModel.EvalToDate, model.FilterModel.ScreenID, model.FilterModel.UserID, model.FilterModel.Status, model.FilterModel.Type).ToList();
            //model.EvalDefinitionResult = Global.VisibilityContext.usp_GetEvaluationDetailInfoByFilter_Param(model.FilterModel.EvaluationID.Trim(), model.FilterModel.ReferenceCD, model.FilterModel.DisplayID, model.FilterModel.Description, model.FilterModel.Content, model.FilterModel.DisplayFromDate, model.FilterModel.DisplayToDate, model.FilterModel.EvalFromDate, model.FilterModel.EvalToDate, model.FilterModel.ScreenID, model.FilterModel.UserID, model.FilterModel.Status, model.FilterModel.Type).ToList();
            InitSessionData();
            Session["FilterModel"] = model.FilterModel;
            return View(model);
        }

        [Authorize]
        [ActionAuthorize("Evaluation_EvaluationDefinition", true)]
        public ActionResult CreateEvalDefinition()
        {
            EvaluationDataModel model = new EvaluationDataModel();
            InitSessionData();
            model.FilterModel = SetDataModelFilter();
            model.FilterModel.ListAcuEval = Global.VisibilityContext.DMSAcuDisplayEvals.ToList();
            model.EvalDefinitionResult = Global.VisibilityContext.usp_GetEvaluationDetailInfoByFilter_Param(model.FilterModel.EvaluationID, model.FilterModel.ReferenceCD, model.FilterModel.DisplayID, model.FilterModel.Description, model.FilterModel.Content, model.FilterModel.DisplayFromDate, model.FilterModel.DisplayToDate, model.FilterModel.EvalFromDate, model.FilterModel.EvalToDate, model.FilterModel.ScreenID, model.FilterModel.UserID, model.FilterModel.Status, model.FilterModel.Type).ToList();
            model.GeoologyList = Global.VisibilityContext.usp_GetGeoologyInforTree_New(model.FilterModel.DisplayID, model.FilterModel.DisplayFromDate, model.FilterModel.DisplayToDate).ToList().OrderByDescending(o => o.Cap).ToList();
            //model.SaleOrgList = Global.VisibilityContext.ufn_GetSaleOrgInforTree("8").ToList().OrderByDescending(o => o.Cap).ToList();
            model.AllUserList = Global.VisibilityContext.uvw_GetUserInformations.ToList();
            model.RolesList = Global.VisibilityContext.uvw_GetRolesInformations.ToList();
            model.SelectedUserList.Clear();

            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            profileProgram.GeoologyList = model.GeoologyList;
            profileProgram.AllUserList = model.AllUserList;
            profileProgram.RolesList = model.RolesList;
            profileProgram.SelectedUserList = model.SelectedUserList;
            Session["ProgramProfile"] = profileProgram;

            return View(model);
        }

        public ActionResult CreateEvaluationIDFromInput(InputDataModel myParam)
        {
            try
            {
                // Called from SCREEN VSC003: "Chi Tiết Kỳ Đánh Giá"
                string resultParameter = "";
                if (Session["UserProfile"] == null) return Json(resultParameter, JsonRequestBehavior.AllowGet);
                var profileData = this.Session["UserProfile"] as SessionDataModel;
                profileData.ScreenID = "VSC003";

                Global.VisibilityContext.usp_GenerateNewEvaluationID(myParam.MaThamChieu, myParam.MaCTTB,
                                                            myParam.NoiDungCTTB, myParam.ThoiGianCTTBTu,
                                                            myParam.ThoiGianCTTBDen, myParam.KieuTGDanhGia,
                                                            myParam.ThoiGianDanhGiaTu, myParam.ThoiGianDanhGiaDen,
                                                            myParam.TyLeReview, myParam.LoaiHinhDanhGia,
                                                            myParam.DanhGiaNumeric, myParam.DanhSachItemID,
                                                            myParam.DanhSachChonItem, myParam.DanhSachUserID,
                                                            myParam.DanhSachUserRole, myParam.DanhSachUserLeader,
                                                            myParam.LoaiKhuVuc,
                                                            myParam.DanhSachKhuVuc, profileData.LoginID,
                                                            profileData.ScreenID, ref resultParameter);

                string sNewEvaluationID = resultParameter;

                Session["UserProfile"] = profileData;
                return Json(sNewEvaluationID, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }

        }
        public ActionResult GetAcuEvaluationInfo(string DisplayID)
        {
            List<DMSAcuDisplayEval> model = new List<DMSAcuDisplayEval>();
            model = Global.VisibilityContext.DMSAcuDisplayEvals.Where(x => x.DisplayID.HasValue && x.DisplayID.Value == Int32.Parse(DisplayID)).ToList();
            return PartialView("AcuEvalOptionPartial", model);
        }


        [Authorize]
        [ActionAuthorize("Evaluation_DetailEvaluationDefinitionView")]
        public ActionResult DetailEvaluationDefinitionView(string sEvalID)
        {
            EvaluationDataModel model = new EvaluationDataModel();
            model.FilterModel = SetDataModelFilter();
            DMSEvaluation eval = Global.VisibilityContext.DMSEvaluations.SingleOrDefault(x => x.EvaluationID == sEvalID.Trim());
            if (eval != null)
            {
                model.FilterModel.EvaluationID = eval.EvaluationID;
                model.FilterModel.Display = eval.ProgramName;
                model.FilterModel.DisplayID = eval.ProgramID;
                model.FilterModel.Description = eval.ProgramDescription;
                model.FilterModel.Content = eval.ProgramContent;
                model.FilterModel.DisplayFromDate = eval.ProgramDateFrom;
                model.FilterModel.DisplayToDate = eval.ProgramDateTo;
                model.FilterModel.EvalFromDate = eval.EvalDateFrom;
                model.FilterModel.EvalToDate = eval.EvalDateTo;
                model.FilterModel.Status = eval.EvalState;
                model.FilterModel.Type = eval.EvalType;
                if (eval.EvalType == "A")
                {
                    model.FilterModel.isAuto = true;
                    List<SelectListItem> listType = new List<SelectListItem>() {
                        new SelectListItem() { Text = Utility.Phrase("Automatic"), Value = "A", Selected = true },
                    };
                    model.FilterModel.listType = listType;
                    model.SelectedUserList.Clear();
                }
                else
                {
                    List<SelectListItem> listType = new List<SelectListItem>() {
                        new SelectListItem() { Text = Utility.Phrase("Manual"), Value = "M", Selected = true }
                    };
                    model.FilterModel.listType = listType;
                    model.SelectedUserList = Global.VisibilityContext.usp_GetUserInformationByID(model.FilterModel.EvaluationID, "0").Select(x =>
                        new uvw_GetUserInformation()
                        {
                            STT = x.STT,
                            MaNhanVien = x.EvalUserID,
                            TenNhanVien = x.TenNhanVien,
                            SoDienThoai = x.SoDienThoai,
                            VaiTro = x.VaiTro,
                            LeaderAuditor = x.EvalLeaderID
                        }).ToList();
                }
                if (eval.isNumeric == "1")
                {
                    model.FilterModel.isNumeric = true;
                }
                if (eval.ReviewRate != 0)
                {
                    model.FilterModel.HasRate = true;
                }
                model.FilterModel.Rate = eval.ReviewRate;
                List<SelectListItem> listStatus = new List<SelectListItem>() {
                    new SelectListItem() { Text = Utility.Phrase("Evaluating"), Value = "1", Selected = (eval.EvalState == 1) ? true : false },
                    new SelectListItem() { Text = Utility.Phrase("Completed"), Value = "2", Selected = (eval.EvalState == 2) ? true : false },
                    new SelectListItem() { Text = Utility.Phrase("Cancelled"), Value = "3", Selected = (eval.EvalState == 3) ? true : false }
                };
                model.FilterModel.listStatus = listStatus;
            }

            model.GeoologyList = Global.VisibilityContext.usp_GetGeoologyInforTree_New(model.FilterModel.DisplayID, model.FilterModel.DisplayFromDate, model.FilterModel.DisplayToDate).ToList().OrderByDescending(o => o.Cap).ToList();
            model.AllUserList = Global.VisibilityContext.uvw_GetUserInformations.ToList();
            model.RolesList = Global.VisibilityContext.uvw_GetRolesInformations.ToList();
            List<string> selectedGeoology = Global.VisibilityContext.DMSEvalWithTerritories.Where(x => x.EvaluationID == model.FilterModel.EvaluationID).Select(s => s.TerritoryID).ToList();
            List<ViewGeoology> data = new List<ViewGeoology>();
            foreach (var elm in model.GeoologyList)
            {
                Dictionary<string, bool> state = new Dictionary<string, bool>();
                if (selectedGeoology.Exists(x => x == elm.MaKhuVuc))
                {
                    state.Add("checked", true);
                    state.Add("selected", true);
                }
                else
                {
                    state.Add("checked", false);
                    state.Add("selected", false);
                }
                data.Add(new ViewGeoology()
                {
                    text = elm.KhuVucDiaLy + "  (" + elm.SoLuongCuaHang + " Cửa hàng )",
                    id = elm.KhuVucDiaLyID,
                    parentId = elm.KhuVucDiaLyChaID,
                    MaKhuVuc = elm.MaKhuVuc,
                    state = state
                });
            }
            ViewData["GeoologyData"] = data;
            ViewData["selectedGeoology"] = selectedGeoology;
            if (selectedGeoology.Count == data.Count)
            {
                model.FilterModel.isGeography = true;
            }
            InitSessionData();
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            profileProgram.EvaluationID = sEvalID;
            profileProgram.FilterModel = model.FilterModel;
            profileProgram.GeoologyList = model.GeoologyList;
            profileProgram.AllUserList = model.AllUserList;
            profileProgram.RolesList = model.RolesList;
            profileProgram.SelectedUserList = model.SelectedUserList;
            Session["ProgramProfile"] = profileProgram;

            return View(model);
        }

        public ActionResult ReloadGeoologyData(string DisplayID, string DispFromDate, string DispToDate)
        {
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            DateTime fromdate = new DateTime();
            DateTime todate = new DateTime();
            if (!String.IsNullOrEmpty(DispFromDate))
            {
                fromdate = DateTime.Parse(DispFromDate);
            }
            if (!String.IsNullOrEmpty(DispToDate))
            {
                todate = DateTime.Parse(DispToDate);
            }
            profileProgram.GeoologyNewList = Global.VisibilityContext.usp_GetGeoologyInforTree_New(DisplayID, fromdate, todate).ToList().OrderByDescending(o => o.Cap).ToList();
            List<ViewGeoology> data = new List<ViewGeoology>();
            foreach (var elm in profileProgram.GeoologyNewList)
            {
                Dictionary<string, bool> state = new Dictionary<string, bool>();
                state.Add("checked", false);
                state.Add("selected", false);
                data.Add(new ViewGeoology()
                {
                    text = elm.KhuVucDiaLy + "  (" + elm.SoLuongCuaHang + " Cửa hàng )",
                    id = elm.KhuVucDiaLyID,
                    parentId = elm.KhuVucDiaLyChaID,
                    MaKhuVuc = elm.MaKhuVuc,
                    state = state
                });
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateEvaluationID(InputDataModel myParam)
        {
            // Called from SCREEN VSC003: "Chi Tiết Kỳ Đánh Giá"
            int? resultParameter = null;
            if (Session["UserProfile"] == null) return Json(1);
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC003";
            Global.VisibilityContext.usp_UpdateEvaluationID(myParam.MaDinhNghia,
                                                        myParam.NoiDungCTTB, myParam.ThoiGianCTTBTu,
                                                        myParam.ThoiGianCTTBDen, myParam.KieuTGDanhGia,
                                                        myParam.ThoiGianDanhGiaTu, myParam.ThoiGianDanhGiaDen,
                                                        myParam.TyLeReview, myParam.LoaiHinhDanhGia,
                                                        myParam.DanhGiaNumeric, myParam.DanhSachItemID,
                                                        myParam.DanhSachUserID,
                                                        myParam.DanhSachUserRole, myParam.DanhSachUserLeader,
                                                        myParam.LoaiKhuVuc,
                                                        myParam.DanhSachKhuVuc, profileData.LoginID,
                                                        profileData.ScreenID, ref resultParameter);
            string sNewEvaluationID = "";
            if (resultParameter.Value == 1)
            {
                sNewEvaluationID = Utility.Phrase("SaveSuccessfully");
            }
            else
            {
                sNewEvaluationID = Utility.Phrase("SaveError");
            }

            Session["UserProfile"] = profileData;
            return Json(sNewEvaluationID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckImageEvaluation(string programID, string programFromDate, string programToDate, string tyleGeo, string listGeo)
        {
            string error = string.Empty;
            DateTime fromDate = new DateTime();
            DateTime toDate = new DateTime();
            if (!string.IsNullOrEmpty(programFromDate) && !string.IsNullOrEmpty(programToDate))
            {
                fromDate = DateTime.Parse(programFromDate);
                toDate = DateTime.Parse(programToDate);
            }
            int? result = 0;
            Global.VisibilityContext.usp_CheckImageEvaluation(programID, fromDate, toDate, tyleGeo, listGeo, ref result);
            if (result == null || result.Value == 0)
            {
                error = Utility.Phrase("EvalNotImage");
            }
            return Json(error, JsonRequestBehavior.AllowGet);
        }
        #endregion
        // SCREEN VSC004: "Phân Bổ Cửa Hàng cho Nhân Viên"
        #region  DistributeOutletToAuditor
        [Authorize]
        [ActionAuthorize("Evaluation_DistributeOutletToAuditor", true)]
        public ActionResult DistributeOutletToAuditor(string DisplayID = "", string DisplayCode = "", string EvaluationCode = "", string EvalState = "", string EvalContent = "", string EvalDesc = "", string DispFromDate = "", string DispToDate = "", string EvalFromDate = "", string EvalToDate = "", string RefNumber = "")
        {
            EvaluationDataModel model = new EvaluationDataModel();
            ViewBag.ScreenID = "VSC004";
            EvaluationCode = Utility.StringParse(EvaluationCode);
            if (string.IsNullOrEmpty(EvaluationCode))
            {
                if (SessionHelper.GetSession<FilterModel>("FilterModelDistribute") != null)
                {
                    model.FilterModel = SessionHelper.GetSession<FilterModel>("FilterModelDistribute");
                }
                else
                {
                    model.FilterModel = SetDataModelFilter();
                }
            }
            else
            {
                model.FilterModel = SetDataModelFilter(DisplayID, DisplayCode, EvaluationCode, EvalState, string.Empty, EvalContent, EvalDesc, DispFromDate, DispToDate, EvalFromDate, EvalToDate, RefNumber);

                DMSEvaluation eval = new DMSEvaluation();
                eval = Global.VisibilityContext.DMSEvaluations.FirstOrDefault(a => a.EvaluationID == EvaluationCode);
                if (eval != null)
                {
                    model.FilterModel.DisplayID = eval.ProgramID;
                    model.FilterModel.Display = eval.ProgramName;
                    model.FilterModel.Content = eval.ProgramContent;
                    model.FilterModel.EvalFromDate = eval.EvalDateFrom;
                    model.FilterModel.EvalToDate = eval.EvalDateTo;
                    model.FilterModel.UserID = SessionHelper.GetSession<string>("UserName");
                }
                Session["FilterModelDistribute"] = model.FilterModel;

            }
            model.UserList = Global.VisibilityContext.usp_GetUserInformationByID(EvaluationCode, Constant.Eval_Auditor).ToList();
            Global.VisibilityContext.usp_GetNumFromEvaluationID(EvaluationCode, ref model.sSoOutlet, ref model.sSoAuditor, ref model.sSoPhanBo);
            ViewBag.SumOutlet = 0;
            if (model.UserList.Count > 0) ViewBag.SumOutlet = model.UserList.Sum(item => item.SoOutletChamDiem);

            return View(model);
        }
        public string ProcessDistributionOutletToAuditor(string sEvalID, string DistributionDataFrom, string DistributionDataTo)
        {
            // Called from SCREEN VSC004: "Phân Bổ Cửa Hàng cho Nhân Viên"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC004";

            string inform = "";
            var checkEval = Global.VisibilityContext.DMSEvaluations.Where(x => x.EvaluationID == sEvalID).FirstOrDefault();
            if (checkEval != null)
            {
                if (checkEval.EvalState == 1 || checkEval.EvalState == 2)
                {
                    if (profileData.CheckedData == "1")
                    {
                        Global.VisibilityContext.usp_DistributeOutletToAuditor(sEvalID, DistributionDataFrom, DistributionDataTo, profileData.LoginID, profileData.ScreenID);
                        inform = "1";
                    }
                    else
                    {
                        Global.VisibilityContext.usp_DistributeOutletToAuditor(sEvalID, DistributionDataFrom, DistributionDataTo, profileData.LoginID, profileData.ScreenID);
                        inform = "1";
                    }
                }
                else
                {
                    inform = "0";
                }
            }
            else
            {
                inform = "0";
            }
            Session["UserProfile"] = profileData;

            return inform;
        }
        #endregion
        //Quản lý chấm điểm bằng tay
        #region Marking Evaluation
        [Authorize]
        [ActionAuthorize("Evaluation_OutletImageEvaluation", true)]
        public ActionResult OutletImageEvaluation(string sEvalID = "", string type = "")
        {
            EvaluationDataModel model = new EvaluationDataModel();
            InitSessionData();
            model.FilterModel.UserID = SessionHelper.GetSession<string>("UserName");
            if (SessionHelper.GetSession<FilterModel>("FilterModel") != null)
            {
                model.FilterModel = SessionHelper.GetSession<FilterModel>("FilterModel");
            }
            else
            {
                model.FilterModel = SetDataModelFilter();
            }
            if (!string.IsNullOrEmpty(sEvalID))
            {
                model.FilterModel.EvaluationID = sEvalID;
                Session["FilterModel"] = model.FilterModel;
            }
            model.FilterModel.EvaluationData = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "M", Utility.RoleName.Auditor.ToString(), model.FilterModel.UserID).Where(x => x.YourStatus >= 2 && x.YourStatus < 6).ToList();
            model.OutletEvalDetailList = Global.VisibilityContext.usp_GetOutletsOfEvalBy(model.FilterModel.EvaluationID, model.FilterModel.UserID, 0).ToList();
            string userID = string.Empty;
            if (PermissionHelper.CheckPermissionByFeature(Utility.RoleName.Auditor.ToString()))
            {
                userID = model.FilterModel.UserID;
            }
            if (!string.IsNullOrEmpty(model.FilterModel.EvaluationID) && type == "Refresh")
            {
                Global.VisibilityContext.usp_UpdateStatusImageSync(model.FilterModel.EvaluationID, userID, 3);  //3 là auditor
            }
            var listInfoEval = Global.VisibilityContext.usp_GetInfoMarkingByEvalID(model.FilterModel.EvaluationID, userID).ToList();
            usp_GetInfoMarkingByEvalIDResult infoMarking = listInfoEval.SingleOrDefault(x => x.EvaluationID == model.FilterModel.EvaluationID);
            EvalAutoMarkMV EvalMarkInfo = new EvalAutoMarkMV();
            EvalMarkInfo.Evaluation = Global.VisibilityContext.DMSEvaluations.SingleOrDefault(x => x.EvaluationID == model.FilterModel.EvaluationID);
            if (infoMarking != null)
            {
                EvalMarkInfo.TotalImages = infoMarking.TotalImg.Value;
                EvalMarkInfo.DateAutoMarking = infoMarking.EvalDateFrom.Value;
                EvalMarkInfo.ImageMarking = infoMarking.ImgMarking.Value;
                EvalMarkInfo.ImgFakes = infoMarking.ImgFakes.Value;
                EvalMarkInfo.ImgPass = infoMarking.ImgPass.Value;
                EvalMarkInfo.ImgThat = infoMarking.ImgReal.Value;
                EvalMarkInfo.ImgNumberic = infoMarking.ImgHasNumberic.Value;
                EvalMarkInfo.ImgNotPass = infoMarking.ImgNotPass.Value;
                EvalMarkInfo.ImgNotPassNumberic = infoMarking.ImgNotNumberic.Value;
                EvalMarkInfo.ImgErrorMarking = infoMarking.ImgNotYetMarking.Value;
                EvalMarkInfo.ImgNotStandard = infoMarking.ImgNotStandard.Value;
                EvalMarkInfo.OutletMarking = model.OutletEvalDetailList.Where(x => x.stringTrangThai == "Eval_FinishAssessment" || x.stringTrangThai == "Eval_NotEverReview" || x.stringTrangThai == "Eval_Reviewed").Count();
            }
            var elmEvalReject = Global.VisibilityContext.DMSEvalWithUserRoles.Where(x => x.EvaluationID == model.FilterModel.EvaluationID && x.EvalUserID == model.FilterModel.UserID && x.TotalImageRejected.HasValue).FirstOrDefault();
            if (elmEvalReject != null)
            {
                EvalMarkInfo.ImgRejectMark = elmEvalReject.TotalImageRejected.Value;
            }
            ViewData["EvalAutoMarkMV"] = EvalMarkInfo;
            Session["EvalAutoMarkMV"] = EvalMarkInfo;
            ViewBag.ScreenID = "VSC005";
            return View(model);
        }

        [Authorize]
        [ActionAuthorize("Evaluation_DetailOutletImageEvaluation")]
        public ActionResult MarkingOutletImageEval(string sEvalID, string sOutletID, string typeMarking = "")
        {
            MarkModel model = new MarkModel();
            if (SessionHelper.GetSession<MarkModel>("MarkModel") != null)
            {
                model = SessionHelper.GetSession<MarkModel>("MarkModel");
            }

            model.sEvalID = Utility.StringParse(sEvalID);
            model.Evaluation = Global.VisibilityContext.DMSEvaluations.SingleOrDefault(x => x.EvaluationID == sEvalID);
            List<usp_GetListImageByResult> data = Global.VisibilityContext.usp_GetListImageBy(sEvalID, "", model.Auditor, -1, -1, -1, 0).ToList();
            if (typeMarking == "RejectMark")
            {
                model.TypeMark = "RejectMark";
                data = data.Where(x => x.ReviewDate.HasValue && x.FinishDate.HasValue && !x.isApproved.Value).ToList();
            }
            else if (typeMarking == "ReMark")
            {
                model.TypeMark = "ReMark";
                data = data.Where(x => !x.ReviewDate.HasValue && x.FinishDate.HasValue && x.FinishDate != null).ToList();
            }
            else if (typeMarking == "Mark")
            {
                model.TypeMark = "Mark";
                data = data.Where(x => !x.ReviewDate.HasValue && !x.FinishDate.HasValue).ToList();
            }
            else
            {
                model.TypeMark = string.Empty;
            }

            model.ListResult = data.Select(s =>
                new ImageDataModel()
                {
                    ImageID = s.CustomerImageID,
                    ImageIDName = s.ImageFileName,
                    ImageDesc = s.ImageFileNameDesc,
                    CustomerID = s.CustomerID,
                    CustomerName = s.CustomerName,
                    ComparedImageID = s.ImageCompareID != null ? Int32.Parse(s.ImageCompareID) : 0,
                    ComparedImageIDName = s.ImageFileCompare != null ? s.ImageFileCompare : "",
                    ComparedImageDesc = s.ImageFileCompareDesc,
                    AvatarImageFile = s.ImageFileAvata != null ? s.ImageFileAvata : "",
                    SalemanName = s.SalesmanName,
                    CaptureDistance = s.Distance,
                    SyncDate = s.SyncDate,
                    ImgDate = s.CapturedDate,
                    ImgDateMark = s.FinishDate,
                    ImageIDEvalUserName = s.MarkingAssign,
                    ImageStatus = s.ImgStatus,
                    ImageColorStatus = s.ImgColorStatus,
                    isMatchedWithBefore = s.isMatchedWithBefore,
                    isAccepted = s.isAccepted,
                    isCaptured = s.isCaptured,
                    isPassed = s.isPassed,
                    Reason1 = s.Reason1,
                    Reason2 = s.Reason2,
                    Reason3 = s.Reason3,
                    isFinished = s.isFinished,
                    ReviewDate = s.ReviewDate,
                    ReFinishDate = s.ReFinishDate,
                    isApproved = s.isApproved,
                    isEdit = ((s.isApproved.HasValue && s.isApproved.Value) || s.ReReviewDate.HasValue) ? false : true
                }).ToList();
            if(model.ListResult != null)
            {
                model.ImgFirst = model.ListResult.First().ImageID;
                model.ImgLast = model.ListResult.Last().ImageID;
            }
            model.ReasonFake = Global.VisibilityContext.usp_GetDisplayReason(Int32.Parse(model.Evaluation.ProgramID), 1).Where(x => x.DisplayID != 0).ToList();
            model.ReasonAccepted = Global.VisibilityContext.usp_GetDisplayReason(Int32.Parse(model.Evaluation.ProgramID), 2).Where(x => x.DisplayID != 0).ToList();
            model.ReasonDisplay = Global.VisibilityContext.usp_GetDisplayReason(Int32.Parse(model.Evaluation.ProgramID), 3).Where(x => x.DisplayID != 0).ToList();

            if (string.IsNullOrEmpty(sOutletID) && model.ListResult != null)
            {
                model.CurentOutletID = model.ListResult.First().CustomerID;
            }
            else
            {
                model.CurentOutletID = Utility.StringParse(sOutletID);
            }

            Session["MarkModel"] = model;
            return View(model);
        }

        [Authorize]
        [ActionAuthorize("Evaluation_DetailOutletImageEvaluation")]
        public ActionResult MarkingImageEvalPartial(string sEvalID, string sOutletID, int ImageID = 0)
        {
            MarkModel model = new MarkModel();
            try
            {
                if (SessionHelper.GetSession<MarkModel>("MarkModel") != null)
                {
                    model = SessionHelper.GetSession<MarkModel>("MarkModel");
                }
                model.sEvalID = Utility.StringParse(sEvalID);
                if (ImageID == 0)
                {
                    if (string.IsNullOrEmpty(model.TypeMark))
                    {
                        model.ListResult = Global.VisibilityContext.usp_GetListImageBy(sEvalID, "", model.Auditor, -1, -1, -1, 0).Select(s =>
                        new ImageDataModel()
                        {
                            ImageID = s.CustomerImageID,
                            ImageIDName = s.ImageFileName,
                            ImageDesc = s.ImageFileNameDesc,
                            CustomerID = s.CustomerID,
                            CustomerName = s.CustomerName,
                            ComparedImageID = s.ImageCompareID != null ? Int32.Parse(s.ImageCompareID) : 0,
                            ComparedImageIDName = s.ImageFileCompare != null ? s.ImageFileCompare : "",
                            ComparedImageDesc = s.ImageFileCompareDesc,
                            AvatarImageFile = s.ImageFileAvata != null ? s.ImageFileAvata : "",
                            SalemanName = s.SalesmanName,
                            CaptureDistance = s.Distance,
                            SyncDate = s.SyncDate,
                            ImgDate = s.CapturedDate,
                            ImgDateMark = s.FinishDate,
                            ImageIDEvalUserName = s.MarkingAssign,
                            ImageStatus = s.ImgStatus,
                            ImageColorStatus = s.ImgColorStatus,
                            isMatchedWithBefore = s.isMatchedWithBefore,
                            isAccepted = s.isAccepted,
                            isCaptured = s.isCaptured,
                            isPassed = s.isPassed,
                            Reason1 = s.Reason1,
                            Reason2 = s.Reason2,
                            Reason3 = s.Reason3,
                            isFinished = s.isFinished,
                            ReviewDate = s.ReviewDate,
                            ReFinishDate = s.ReFinishDate,
                            isApproved = s.isApproved,
                            isEdit = ((s.isApproved.HasValue && s.isApproved.Value) || s.ReReviewDate.HasValue) ? false : true
                        }).ToList();
                    }
                    model.CurentOutletID = Utility.StringParse(sOutletID);
                    model.ListImgCurentOutlet = model.ListResult.Where(x => x.CustomerID == model.CurentOutletID).ToList();
                    model.CurentImage = model.ListImgCurentOutlet.Where(x => x.CustomerID == model.CurentOutletID).FirstOrDefault();
                }
                else
                {
                    model.CurentImage = model.ListResult.FirstOrDefault(x => x.ImageID == ImageID);
                    model.CurentOutletID = model.CurentImage.CustomerID;
                    model.ListImgCurentOutlet = model.ListResult.Where(x => x.CustomerID == model.CurentImage.CustomerID).ToList();
                }
                
                if (model.ListResult.Count == 1)
                {
                    model.ImgPrev = model.ImgFirst;
                    model.ImgNext = model.ImgLast;
                    model.ImgMarkNext = 0;
                }
                else
                {
                    int curentIndex = model.ListResult.FindIndex(x => x.ImageID == model.CurentImage.ImageID);
                    if (curentIndex < 0)
                    {
                        TempData["Mess"] = Utility.Phrase("Eval_Mess_Warning"); 
                        RedirectToAction("Evaluation", "OutletImageEvaluation", new { @sEvalID = sEvalID });
                    }
                    else if (curentIndex == 0)
                    {
                        model.ImgPrev = model.ImgFirst;
                        model.ImgNext = model.ListResult[curentIndex + 1].ImageID;
                        model.ImgMarkNext = model.ListResult[curentIndex + 1].ImageID;
                    }
                    else if (curentIndex == (model.ListResult.Count - 1))
                    {
                        model.ImgNext = model.ImgLast;
                        model.ImgPrev = model.ListResult[curentIndex - 1].ImageID;
                        model.ImgMarkNext = 0;

                        int temp = model.ListResult.FindIndex(x => 
                                        (x.ImageID != model.CurentImage.ImageID && model.TypeMark == "Mark" && !x.ReviewDate.HasValue && !x.isFinished.Value)
                                        || (x.ImageID != model.CurentImage.ImageID && model.TypeMark == "ReMark" && !x.ReviewDate.HasValue && x.ReFinishDate.HasValue)
                                        || (x.ImageID != model.CurentImage.ImageID && model.TypeMark == "RejectMark" && x.ReviewDate.HasValue && x.isFinished.Value && !x.isApproved.Value));
                        if (temp >= 0)
                        {
                            model.ImgMarkNext = model.ListResult[temp].ImageID;
                        }
                    }
                    else
                    {
                        model.ImgNext = model.ListResult[curentIndex + 1].ImageID;
                        model.ImgPrev = model.ListResult[curentIndex - 1].ImageID;
                        model.ImgMarkNext = model.ListResult[curentIndex + 1].ImageID;
                    }

                    if(model.ImgMarkNext != 0)
                    {
                        var ImageNext = model.ListResult.FirstOrDefault(x => x.ImageID == model.ImgMarkNext);
                        if (ImageNext != null && model.TypeMark == "Mark")
                        {
                            if (ImageNext.isFinished.HasValue && ImageNext.isFinished.Value)
                            {
                                int temp = model.ListResult.FindIndex(x => x.ImageID != model.CurentImage.ImageID && !x.ReviewDate.HasValue && !x.isFinished.Value);
                                if (temp >= 0)
                                {
                                    model.ImgMarkNext = model.ListResult[temp].ImageID;
                                }
                                else
                                {
                                    model.ImgMarkNext = 0;
                                }
                            }
                        }
                        else if (ImageNext != null && model.TypeMark == "ReMark")
                        {
                            if (ImageNext.ReFinishDate.HasValue)
                            {
                                int temp = model.ListResult.FindIndex(x => x.ImageID != model.CurentImage.ImageID && !x.ReviewDate.HasValue && x.isFinished.HasValue && x.isFinished.Value);
                                if (temp >= 0)
                                {
                                    model.ImgMarkNext = model.ListResult[temp].ImageID;
                                }
                                else
                                {
                                    model.ImgMarkNext = 0;
                                }
                            }
                        }
                        else if (ImageNext != null && model.TypeMark == "RejectMark")
                        {
                            if (ImageNext.isApproved.HasValue && ImageNext.isApproved.Value)
                            {
                                int temp = model.ListResult.FindIndex(x => x.ImageID != model.CurentImage.ImageID && x.ReviewDate.HasValue && x.isFinished.Value && !x.isApproved.Value);
                                if (temp >= 0)
                                {
                                    model.ImgMarkNext = model.ListResult[temp].ImageID;
                                }
                                else
                                {
                                    model.ImgMarkNext = 0;
                                }
                            }
                        }
                    }
                }
                    

                DMSEvalProgram program = Global.VisibilityContext.DMSEvalPrograms.FirstOrDefault(x => x.ProgramID == Int32.Parse(model.Evaluation.ProgramID));
                if (program != null)
                {
                    if (program.Type == 1)
                    {
                        model.PathImageCompare = Constant.ImagePlanogramFolder;
                    }
                }
                model.TotalImg = model.ListResult.Count();
                model.ImgMarked = model.ListResult.Where(x => x.isFinished.HasValue && x.isFinished.Value == true).Count();
                model.TotalOutletImg = model.ListImgCurentOutlet.Count;
                model.ImgOutletMarked = model.ListImgCurentOutlet.Where(x => x.isFinished.HasValue && x.isFinished.Value == true).Count();
                model.ListLatestComparedImages = Global.VisibilityContext.usp_GetComparedImageDataByOutletID(model.sEvalID, model.CurentImage.CustomerID).ToList();
                model.ListItem = Global.VisibilityContext.usp_GetItemInforByID(model.sEvalID, model.CurentImage.ImageID.ToString()).ToList();
                model.ListOultet = (from m in model.ListResult
                                    //where (model.TypeMark == "Mark" && !m.ReviewDate.HasValue && !m.isFinished.HasValue)
                                    //    || (model.TypeMark == "ReMark" && !m.ReviewDate.HasValue && m.isFinished.HasValue && m.isFinished != null)
                                    //    || (model.TypeMark == "RejectMark" && m.ReviewDate.HasValue && m.isFinished.HasValue && !m.isApproved.Value)
                                    //    || (string.IsNullOrEmpty(model.TypeMark))
                                    group m by new
                                    {
                                        m.CustomerID,
                                        m.CustomerName
                                    } into gm
                                    select new SelectListItem()
                                    {
                                        Text = gm.Key.CustomerName,
                                        Value = gm.Key.CustomerID,
                                        Selected = (gm.Key.CustomerID == model.CurentOutletID ? true : false)
                                    }).ToList();
            }
            catch (Exception ex)
            {
                RedirectToAction("Evaluation", "OutletImageEvaluation", new { @sEvalID = sEvalID });
                CustomLog.LogError(ex);
                throw;
            }
            return PartialView("MarkingImageEvalPartial", model);
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_DetailOutletImageEvaluation")]
        public ActionResult SaveImageMarkingData(string sEvalID, string sOutletID, string listImage, string listResultImg, string listReasonImg, string listInventoryImg)
        {
            int result = -1;
            try
            {
                string ListOfDaChamDiem = "";
                if (SessionHelper.GetSession<MarkModel>("MarkModel") != null)
                {
                    Global.VisibilityContext.usp_SaveEvaluationImageResult(sEvalID, SessionHelper.GetSession<string>("UserName"), sOutletID, listImage, listResultImg, listReasonImg, listInventoryImg, ListOfDaChamDiem);
                    MarkModel model = new MarkModel();
                    model = SessionHelper.GetSession<MarkModel>("MarkModel");
                    List<ImageDataModel> listImgOfOutlet = Global.VisibilityContext.usp_GetListImageBy(sEvalID, sOutletID, model.Auditor, -1, -1, -1, 0).Select(s =>
                        new ImageDataModel()
                        {
                            ImageID = s.CustomerImageID,
                            ImageIDName = s.ImageFileName,
                            ImageDesc = s.ImageFileNameDesc,
                            CustomerID = s.CustomerID,
                            CustomerName = s.CustomerName,
                            ComparedImageID = s.ImageCompareID != null ? Int32.Parse(s.ImageCompareID) : 0,
                            ComparedImageIDName = s.ImageFileCompare != null ? s.ImageFileCompare : "",
                            ComparedImageDesc = s.ImageFileCompareDesc,
                            AvatarImageFile = s.ImageFileAvata != null ? s.ImageFileAvata : "",
                            SalemanName = s.SalesmanName,
                            CaptureDistance = s.Distance,
                            SyncDate = s.SyncDate,
                            ImgDate = s.CapturedDate,
                            ImgDateMark = s.FinishDate,
                            ImageIDEvalUserName = s.MarkingAssign,
                            ImageStatus = s.ImgStatus,
                            ImageColorStatus = s.ImgColorStatus,
                            isMatchedWithBefore = s.isMatchedWithBefore,
                            isAccepted = s.isAccepted,
                            isCaptured = s.isCaptured,
                            isPassed = s.isPassed,
                            Reason1 = s.Reason1,
                            Reason2 = s.Reason2,
                            Reason3 = s.Reason3,
                            isFinished = s.isFinished,
                            ReviewDate = s.ReviewDate,
                            ReFinishDate = s.ReFinishDate,
                            isApproved = s.isApproved,
                            isEdit = ((s.isApproved.HasValue && s.isApproved.Value) || s.ReReviewDate.HasValue) ? false : true
                        }).ToList();
                    foreach (ImageDataModel elm in listImgOfOutlet)
                    {
                        int index = model.ListResult.FindIndex(x => x.ImageID == elm.ImageID);
                        if(index >= 0)
                        {
                            model.ListResult[index] = elm;
                        }
                    }
                    Session["MarkModel"] = model;
                    result = 0;
                    
                    if(model.ImgMarkNext == 0)
                    {
                        result = 1;
                        TempData["Mess"] = Utility.Phrase("Eval_Mess_MarkingAllSuccessfully");
                    }
                }
                else
                {
                    result = -1;
                }
            }
            catch (Exception ex)
            {
                result = -1;
                CustomLog.LogError(ex);
                RedirectToAction("Evaluation", "MarkingImageEvalPartial", new { @sEvalID = sEvalID, @sOutletID = sOutletID });
                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
        //Quản lý xét duyệt hình ảnh
        #region Review Evaluation
        /// <summary>
        /// Màn hình filter review kỳ đánh giá, kết quả hiển thị theo nhân viên
        /// </summary>
        [Authorize]
        [ActionAuthorize("Evaluation_OutletImageReview", true)]
        public ActionResult OutletImageReview(string sEvalID = "")
        {
            EvaluationDataModel model = new EvaluationDataModel();
            InitSessionData();
            model.FilterModel.UserID = SessionHelper.GetSession<string>("UserName");
            if (SessionHelper.GetSession<FilterModel>("FilterModelReview") != null)
            {
                model.FilterModel = SessionHelper.GetSession<FilterModel>("FilterModelReview");
            }
            else
            {
                model.FilterModel = SetDataModelFilter();
            }
            if (!string.IsNullOrEmpty(sEvalID))
            {
                model.FilterModel.EvaluationID = sEvalID;
                Session["FilterModelReview"] = model.FilterModel;
            }
            model.FilterModel.EvaluationData = Global.VisibilityContext.usp_GetEvaluationInfoByType("", "M", Utility.RoleName.Leader.ToString(), model.FilterModel.UserID).Where(x => x.YourStatus >= 2 && x.YourStatus < 6).ToList();
            if (!string.IsNullOrEmpty(model.FilterModel.EvaluationID))
            {
                model.ReviewList = Global.VisibilityContext.usp_GetEvaluationUserBy(model.FilterModel.EvaluationID, model.FilterModel.UserID).ToList();
            }
            
            return View(model);
        }
        [HttpGet]
        [Authorize]
        [ActionAuthorize("Evaluation_ReviewEvalByEmployer")]
        public ActionResult ReviewImageEmployer(string sEvalID, string auditor, int page = 1)
        {
            ReviewModel model = new ReviewModel();
            model.NameSceen = Utility.Phrase("ListImgEvaluation");
            if (SessionHelper.GetSession<ReviewModel>("ReviewModel") != null)
            {
                model = SessionHelper.GetSession<ReviewModel>("ReviewModel");
            }
            model.curent = page;
            
           
                model.Evaluation = Global.VisibilityContext.DMSEvaluations.FirstOrDefault(x => x.EvaluationID == sEvalID);
                #region Getdata
                var program = Global.VisibilityContext.DMSEvalPrograms.Where(x => x.ProgramID == Int32.Parse(model.Evaluation.ProgramID)).FirstOrDefault();
                if (program != null)
                {
                    if (program.Type == 1)
                    {
                        model.pathImageCompare = program.PathImage;
                    }
                }

                model.UserID = SessionHelper.GetSession<string>("UserName");
                if (model.Evaluation.ReviewRate > 0)
                {
                    var dataImage = (from ur in Global.VisibilityContext.DMSEvalWithUserRoles
                                     where ur.EvaluationID == model.Evaluation.EvaluationID && ur.EvalLeaderID == model.UserID
                                     group ur by ur.EvaluationID into g
                                     select new
                                     {
                                         Name = g.First().EvaluationID,
                                         TotalImg = g.Sum(s => s.TotalImage),
                                         ImgMark = g.Sum(s => s.TotalImageMarked),
                                         ImgReviewed = g.Sum(s => s.TotalImageReviewed),
                                     }).FirstOrDefault();
                    if (dataImage != null)
                    {
                        if (dataImage.ImgReviewed.HasValue && dataImage.TotalImg.HasValue)
                        {
                            if (((dataImage.ImgReviewed.Value * 100) / dataImage.TotalImg.Value) >= model.Evaluation.ReviewRate)
                            {
                                model.IsCompleted = true;
                            }
                        }
                    }
                }
                else
                {
                    model.IsCompleted = true;
                }

                model.sEvalID = sEvalID;
                model.Auditor = auditor;
                List<usp_GetListImageByResult> imagelist = new List<usp_GetListImageByResult>();
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID, auditor).OrderBy(o=>o.CapturedDate.Date).ToList();
                if (imagelist.Count % model.limit == 0)
                {
                    model.totalPage = imagelist.Count / model.limit;
                }
                else
                {
                    model.totalPage = (imagelist.Count / model.limit) + 1;
                }
                model.TotalImg = imagelist.Count;
                model.ImgReviewed = imagelist.Where(x => x.ReviewDate.HasValue).ToList().Count;

                var ReasonFake = Global.VisibilityContext.usp_GetDisplayReason(Int32.Parse(model.Evaluation.ProgramID), 1).ToList();
                var ReasonAccepted = Global.VisibilityContext.usp_GetDisplayReason(Int32.Parse(model.Evaluation.ProgramID), 2).ToList();
                var ReasonDisplay = Global.VisibilityContext.usp_GetDisplayReason(Int32.Parse(model.Evaluation.ProgramID), 3).ToList();
                foreach (usp_GetListImageByResult elm in imagelist)
                {
                    var reason1 = ReasonFake.Where(x => x.ReasonCode == elm.Reason1).FirstOrDefault();
                    if (reason1 != null)
                    {
                        elm.Reason1 = reason1.ShortDescription;
                    }
                    var reason2 = ReasonAccepted.Where(x => x.ReasonCode == elm.Reason2).FirstOrDefault();
                    if (reason2 != null)
                    {
                        elm.Reason2 = reason2.ShortDescription;
                    }
                    var reason3 = ReasonDisplay.Where(x => x.ReasonCode == elm.Reason3).FirstOrDefault();
                    if (reason3 != null)
                    {
                        elm.Reason3 = reason3.ShortDescription;
                    }
                }
                model.ListImageBy = imagelist;

                #endregion
                #region Filter
                List<SelectListItem> listSalesman = new List<SelectListItem>();
                listSalesman.Add(new SelectListItem()
                {
                    Text = "All",
                    Value = "",
                    Selected = true
                });
                foreach (var item in imagelist.GroupBy(x => new { x.SalesmanID, x.SalesmanName }).Select(s => new { s.Key.SalesmanID, s.Key.SalesmanName }).ToList())
                {
                    listSalesman.Add(new SelectListItem()
                    {
                        Text = item.SalesmanName,
                        Value = item.SalesmanID,
                        Selected = (item.SalesmanID == model.Salesman) ? true : false
                    });
                }
                model.ListSalesman = listSalesman;
                List<SelectListItem> listStaust = new List<SelectListItem>();
                listStaust.Add(new SelectListItem()
                {
                    Text = "All",
                    Value = "",
                    Selected = true
                });
                listStaust.Add(new SelectListItem() { Text = Utility.Phrase("NotYetReview"), Value = "0", Selected = (model.StatusImage == "0") ? true : false });
                listStaust.Add(new SelectListItem() { Text = Utility.Phrase("HasReview"), Value = "1", Selected = (model.StatusImage == "1") ? true : false });
                model.ListStatus = listStaust;
                #endregion

                var listReson = Global.VisibilityContext.usp_GetDisplayReason(Int32.Parse(model.Evaluation.ProgramID), 0).ToList();
                model.ListReason.Add(new SelectListItem()
                {
                    Text = Utility.Phrase("ChooseReason"),
                    Value = ""
                });

                foreach (var item in listReson)
                {
                    model.ListReason.Add(new SelectListItem()
                    {
                        Text = item.ShortDescription,
                        Value = item.ReasonCode
                    });
                }
                Session["ReviewModel"] = model;
            
            model.ListImageBy = model.ListImageBy.Skip((model.curent - 1) * model.limit).Take(model.limit).ToList();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ActionAuthorize("Evaluation_ReviewEvalByEmployer")]
        public ActionResult ReviewImageEmployer(string sEvalID, string auditor, string Salesman = "", string StatusImage = "", string strImageDate = "")
        {
            ReviewModel model = new ReviewModel();
            if (SessionHelper.GetSession<ReviewModel>("ReviewModel") != null)
            {
                model = SessionHelper.GetSession<ReviewModel>("ReviewModel");
                model.sEvalID = sEvalID;
                model.Auditor = auditor;
                model.Salesman = Salesman;
                model.StatusImage = StatusImage;
                List<usp_GetListImageByResult> imagelist = new List<usp_GetListImageByResult>();
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID, auditor).ToList();
                //if (!string.IsNullOrEmpty(model.Salesman) || !string.IsNullOrEmpty(model.StatusImage) || !string.IsNullOrEmpty(strImageDate))
                {
                    if (model.StatusImage == "1")
                    {
                        model.Approved = true;
                    }
                    else
                    {
                        model.Approved = false;
                    }
                    if (!string.IsNullOrEmpty(strImageDate))
                    {
                        model.ImageDate = DateTime.Parse(strImageDate);
                    }
                    imagelist = imagelist.Where(x => (model.Salesman == "" || x.SalesmanID == model.Salesman) && ((model.StatusImage == "1" && x.isApproved.HasValue && x.isApproved.Value) || (model.StatusImage == "0" && !x.isApproved.HasValue) || model.StatusImage == "") && (strImageDate == "" || x.CapturedDate.Date == model.ImageDate.Value.Date)).ToList();
                    model.curent = 1;
                    if (imagelist.Count % model.limit == 0)
                    {
                        model.totalPage = imagelist.Count / model.limit;
                    }
                    else
                    {
                        model.totalPage = (imagelist.Count / model.limit) + 1;
                    }
                    model.ListImageBy = imagelist;
                }

                model.UserID = SessionHelper.GetSession<string>("UserName");
                if (model.Evaluation.ReviewRate > 0)
                {
                    var dataImage = (from ur in Global.VisibilityContext.DMSEvalWithUserRoles
                                     where ur.EvaluationID == model.Evaluation.EvaluationID && ur.EvalLeaderID == model.UserID
                                     group ur by ur.EvaluationID into g
                                     select new
                                     {
                                         Name = g.First().EvaluationID,
                                         TotalImg = g.Sum(s => s.TotalImage),
                                         ImgMark = g.Sum(s => s.TotalImageMarked),
                                         ImgReviewed = g.Sum(s => s.TotalImageReviewed),
                                     }).FirstOrDefault();
                    if (dataImage != null)
                    {
                        if (dataImage.ImgReviewed.HasValue && dataImage.TotalImg.HasValue)
                        {
                            if (((dataImage.ImgReviewed.Value * 100) / dataImage.TotalImg.Value) >= model.Evaluation.ReviewRate)
                            {
                                model.IsCompleted = true;
                            }
                        }
                    }
                }
                else
                {
                    model.IsCompleted = true;
                }



                Session["ReviewModel"] = model;
            }
            else
            {
                return RedirectToAction("ReviewImageEmployer", "Evaluation", new { sEvalID = sEvalID, auditor = auditor });
            }
            model.ListImageBy = model.ListImageBy.Skip((model.curent - 1) * model.limit).Take(model.limit).ToList();
            return View(model);
        }

        public ActionResult UpdateDataReviewMutilImage(string sEvalID, string auditorID, string listImageSelect, string resultReview, string reviewReason)
        {
            int? result = 1;
            if (!string.IsNullOrEmpty(listImageSelect) && !string.IsNullOrEmpty(resultReview) && !string.IsNullOrEmpty(auditorID))
            {
                List<string> listresultReview = new List<string>();
                List<string> listreviewReason = new List<string>();
                List<string> list = listImageSelect.Split(';').ToList<string>();
                foreach (var item in list)
                {
                    listresultReview.Add(resultReview);
                    if (!string.IsNullOrEmpty(reviewReason))
                    {
                        listreviewReason.Add(reviewReason);
                    }
                    else
                    {
                        listreviewReason.Add(" ");
                    }

                }
                Global.VisibilityContext.usp_SaveListReviewImageResult(sEvalID, auditorID, listImageSelect, string.Join(";", listresultReview.ToArray()), string.Join(";", listreviewReason.ToArray()), SessionHelper.GetSession<string>("UserName"), ref result);

            }
            if (result == 0)
            {
                Session["ReviewModel"] = null;
                TempData["Messages"] = Utility.Phrase("SaveReviewImageSuccessfully");
            }
            else
            {
                TempData["Messages"] = Utility.Phrase("SaveReviewImageError");
            }
            return Json(TempData["Messages"], JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult DetailOutletImageReview(string selectedEvaluationID, string selectedAuditorID,
            string selectedOutletID, string selectedOutletName)
        {

            // Called from SCREEN VSC008: "Xét Duyệt Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            profileData.ScreenID = "VSC008";
            Session["UserProfile"] = profileData;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            if (!string.IsNullOrEmpty(selectedEvaluationID))
            {
                if (selectedEvaluationID != "undefined")
                {
                    profileProgram.selectedEvaluationID = selectedEvaluationID.Trim();
                    profileProgram.selectedAuditorID = selectedAuditorID.Trim();
                    profileProgram.selectedOutletName = selectedOutletName.Trim();
                    profileProgram.selectedOutletID = selectedOutletID.Trim();
                    profileProgram.currentOutletID = selectedOutletID.Trim();
                    profileProgram.currentOutletName = selectedOutletName.Trim();
                    profileProgram.EvaluationID = selectedEvaluationID.Trim();
                }
            }

            //profileProgram.OutletList = new List<ufn_GetOutletInforByIDResult>();
            //Lay danh sach outlet
            if (profileProgram.OutletList.Count() <= 0)
            {
                var listOutletRevew = Global.VisibilityContext.ufn_GetOutletInforByScreenID(profileProgram.EvaluationID, profileProgram.selectedAuditorID, profileData.ScreenID).ToList();
                foreach (var item in listOutletRevew)
                {
                    ufn_GetOutletInforByIDResult OneOutlet = new ufn_GetOutletInforByIDResult();
                    OneOutlet.MaCuaHang = item.MaCuaHang;
                    OneOutlet.TenCuaHang = item.TenCuaHang;
                    OneOutlet.MaNhaPhanPhoi = item.MaNhaPhanPhoi;
                    OneOutlet.TenNhaPhanPhoi = item.TenNhaPhanPhoi;
                    profileProgram.OutletList.Add(OneOutlet);

                    OutletDataModel OneOutletReview = new OutletDataModel();
                    OneOutletReview.OutletID = item.MaCuaHang;
                    OneOutletReview.OutletName = item.TenCuaHang;
                    profileProgram.EvalOutletsList.Add(OneOutletReview);
                }
                ////Lay danh sach outlet co ảnh de cham.
                //foreach (var item in listOutletRevew)
                //{
                //    OutletDataModel OneOutlet = new OutletDataModel();
                //    OneOutlet.OutletID = item.MaCuaHang;
                //    OneOutlet.OutletName = item.TenCuaHang;
                //    profileProgram.EvalOutletsList.Add(OneOutlet);
                //}
            }
            // Lay outlet tiep theo.      

            int index = profileProgram.EvalOutletsList.IndexOf(profileProgram.EvalOutletsList.Where(p => p.OutletID == profileProgram.currentOutletID).FirstOrDefault());
            profileProgram.idxOutlet = index;
            profileProgram.nextOutletID = profileProgram.EvalOutletsList[(profileProgram.idxOutlet + 1) % profileProgram.EvalOutletsList.Count].OutletID;
            profileProgram.nextOutletName = profileProgram.EvalOutletsList[(profileProgram.idxOutlet + 1) % profileProgram.EvalOutletsList.Count].OutletName;
            if (profileProgram.idxOutlet < 0) profileProgram.idxOutlet = 0;
            OutletDataModel CurrentOutlet = profileProgram.EvalOutletsList[profileProgram.idxOutlet];
            CurrentOutlet.AvatarImageIDName = CurrentOutlet.AvatarImageLocation + Global.VisibilityContext.ufn_GetOutletAvatarImageByCustomerID(CurrentOutlet.OutletID);
            CurrentOutlet.AvatarImageIDDate = "";
            #region GetdataImageList
            if (!CurrentOutlet.hasData)
            {
                profileData.ViewType = Constant.Eval_Rev;
                List<usp_GetOutletImageDataByIDResult> query = Global.VisibilityContext.usp_GetOutletImageDataByID(profileProgram.selectedEvaluationID, profileData.LoginID, CurrentOutlet.OutletID, profileData.ViewType).ToList();
                for (int i = 0; i < query.Count; i++)
                {
                    ImageDataModel OneImage = new ImageDataModel();
                    OneImage.ImageID = query[i].OutletImageID;
                    OneImage.ImageIDName = OneImage.EvalImageLocation + query[i].FolderLocation + query[i].ImageFileName;
                    OneImage.ImageIDDate = query[i].CapturedDate;
                    OneImage.ImageIDEvaluated = "0";
                    OneImage.ImageIDReviewed = "0";
                    OneImage.SalemanName = query[i].SalesmanName;
                    OneImage.ImageIDEvalUserName = query[i].FullName;
                    OneImage.ImageIDEvalDate = query[i].NgayChamDiem;
                    OneImage.CaptureDistance = query[i].CapturedDistance;
                    OneImage.ComparedImageID = 0;
                    OneImage.ComparedImageIDName = Constant.SalesOrdersImageFolder + query[i].AnhGocChup;
                    OneImage.ComparedImageIDDate = query[i].NgayAnhGocChup;
                    OneImage.Reason1 = query[i].Reason1;
                    OneImage.Reason2 = query[i].Reason2;
                    OneImage.Reason3 = query[i].Reason3;

                    OneImage.ListLatestComparedImages = Global.VisibilityContext.usp_GetComparedImageDataByOutletID(profileProgram.selectedEvaluationID, CurrentOutlet.OutletID).ToList();
                    OneImage.UpdateImageFileName();
                    if (query[i].isMatchedWithBefore == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isMatchedWithBefore).ToString());
                    }
                    if (query[i].isCaptured == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isCaptured).ToString());
                    }
                    if (query[i].isAccepted == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isAccepted).ToString());
                    }
                    if (query[i].isPassed == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isPassed).ToString());
                    }

                    if (query[i].isApproved == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isApproved).ToString());
                    }
                    //OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isMatchedWithBefore).ToString());
                    //OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isCaptured).ToString());
                    //OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isAccepted).ToString());
                    //OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isPassed).ToString());


                    OneImage.ImageIDEvalReason.Add(query[i].Reason1.Trim() != "" ? query[i].Reason1.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason2.Trim() != "" ? query[i].Reason2.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason3.Trim() != "" ? query[i].Reason3.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason.Trim() != "" ? query[i].Reason.Trim() : "0");

                    OneImage.ListNumericItemExists = query[i].DSNumericChamDiem;

                    OneImage.ImageIDEvalStatus = query[i].EvalStatus;
                    OneImage.ImageIDRevStatus = query[i].RevStatus;

                    CurrentOutlet.EvalImagesList.Add(OneImage);
                    profileProgram.EvalOutletsList[profileProgram.idxOutlet] = CurrentOutlet;

                }
                CurrentOutlet.hasData = true;
            }
            else
            {
                profileData.ViewType = Constant.Eval_Rev;
                List<usp_GetOutletImageDataByIDResult> query = Global.VisibilityContext.usp_GetOutletImageDataByID(profileProgram.selectedEvaluationID, profileData.LoginID, CurrentOutlet.OutletID, profileData.ViewType).ToList();
                for (int i = 0; i < query.Count; i++)
                {
                    ImageDataModel OneImage = new ImageDataModel();
                    OneImage.ImageID = query[i].OutletImageID;
                    OneImage.ImageIDName = OneImage.EvalImageLocation + query[i].FolderLocation + query[i].ImageFileName;
                    OneImage.ImageIDDate = query[i].CapturedDate;
                    OneImage.ImageIDEvaluated = "0";
                    OneImage.ImageIDReviewed = "0";
                    OneImage.SalemanName = query[i].SalesmanName;
                    OneImage.ImageIDEvalUserName = query[i].FullName;
                    OneImage.ImageIDEvalDate = query[i].NgayChamDiem;
                    OneImage.CaptureDistance = query[i].CapturedDistance;
                    OneImage.ComparedImageID = 0;
                    OneImage.ComparedImageIDName = OneImage.EvalImageLocation + query[i].AnhGocChup;
                    OneImage.ComparedImageIDDate = query[i].NgayAnhGocChup;

                    OneImage.ListLatestComparedImages = Global.VisibilityContext.usp_GetComparedImageDataByOutletID(profileProgram.selectedEvaluationID, CurrentOutlet.OutletID).ToList();
                    OneImage.UpdateImageFileName();
                    if (query[i].isMatchedWithBefore == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isMatchedWithBefore).ToString());
                    }
                    if (query[i].isCaptured == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isCaptured).ToString());
                    }
                    if (query[i].isAccepted == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isAccepted).ToString());
                    }
                    if (query[i].isPassed == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isPassed).ToString());
                    }
                    if (query[i].isApproved == null)
                    {
                        OneImage.ImageIDEvalResult.Add(string.Empty);
                    }
                    else
                    {
                        OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isApproved).ToString());
                    }
                    //OneImage.ImageIDEvalResult.Add(Convert.ToInt32(query[i].isFinished).ToString());

                    OneImage.ImageIDEvalReason.Add(query[i].Reason1.Trim() != "" ? query[i].Reason1.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason2.Trim() != "" ? query[i].Reason2.Trim() : "0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason3.Trim() != "" ? query[i].Reason3.Trim() : "0");
                    //OneImage.ImageIDEvalReason.Add("0");
                    OneImage.ImageIDEvalReason.Add(query[i].Reason.Trim() != "" ? query[i].Reason.Trim() : "0");

                    OneImage.ListNumericItemExists = query[i].DSNumericChamDiem;

                    OneImage.ImageIDEvalStatus = query[i].EvalStatus;
                    OneImage.ImageIDRevStatus = query[i].RevStatus;

                    CurrentOutlet.EvalImagesList.RemoveAll(rs => rs.ImageID == OneImage.ImageID);
                    CurrentOutlet.EvalImagesList.Add(OneImage);
                    profileProgram.EvalOutletsList[profileProgram.idxOutlet] = CurrentOutlet;

                }
            }
            #endregion

            //profileProgram.            

            Session["ProgramProfile"] = profileProgram;
            LoadHeaderInformation("DetailOutletImageReview");
            // Update Status Of Image Review
            UpdateStatusOfImage("DetailOutletImageReview");
            // Update Item List 
            UpdateListOfItem("DetailOutletImageReview");
            //View Data
            int idxOutlet = profileProgram.idxOutlet;
            int selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;

            //if (profileProgram.EvalOutletsList[idxOutlet].EvalImagesList.Count <= 0)
            //    return RedirectToAction("OutletImageReview");
            #region AssdataView
            DetailOutletImageClass data = new DetailOutletImageClass();
            data.ScreenID = profileData.ScreenID;
            data.UserID = profileData.LoginID;
            data.OutletIndex = idxOutlet;
            data.isMatchedWithBefore = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[0];
            data.isCaptured = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[1];
            data.isAccepted = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[2];
            data.isPassed = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[3];
            data.isReviewed = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[4];

            data.Reason1 = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[0];
            data.Reason2 = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[1];
            data.Reason3 = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[2];
            data.Reason4 = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[3];

            data.DSKQChamDiem = "1;1;1;1;";
            data.DSLyDoChamDiem = "0;0;0;0;";
            data.DSNumericChamDiem = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ListNumericItemExists;

            data.currentOutletID = profileProgram.currentOutletID;
            data.currentOutletName = profileProgram.currentOutletName;
            data.selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;
            data.nextOutletID = profileProgram.nextOutletID;
            data.nextOutletName = profileProgram.nextOutletName;
            data.ComparedImageIDName = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ComparedImageIDName;
            data.ImageIDName = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDName;
            data.ImageIDDate = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDDate;
            data.ListLatestComparedImages = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ListLatestComparedImages;
            data.AvatarImageIDName = profileProgram.EvalOutletsList[idxOutlet].AvatarImageIDName;
            data.SalemanName = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].SalemanName;
            data.CaptureDistance = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].CaptureDistance;

            data.ImageIDEvalUserName = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalUserName;
            data.EvalImagesList = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList;
            if (!string.IsNullOrEmpty(profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalDate))
            {
                data.ImageIDEvalDate = Convert.ToDateTime(profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalDate).ToShortDateString();
            }
            else
            {
                data.ImageIDEvalDate = profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalDate;
            }
            ViewData["Data"] = data;
            #endregion
            //View Bag
            ViewBag.PageInformation = Utility.Phrase("DetailOutletImageReview"); //"Chi Tiết Xét Duyệt Đánh Giá";
            ViewBag.SetMenuImageEvaluationActive = "active";
            ViewBag.DataModel = profileProgram;
            ViewBag.ViewType = profileData.ViewType;
            ViewBag.Auditor = profileProgram.selectedAuditorID;
            ViewBag.EvaluationID = profileProgram.selectedEvaluationID;
            //View Data
            ViewData["OutletInformation"] = profileProgram.OutletList;
            ViewData["ReasonInformation"] = profileProgram.ReasonList;
            ViewData["ItemInformationByID"] = profileProgram.ListItem;
            ViewBag.ScreenID = profileData.ScreenID;
            DMSEvalWithUserRole FirtRateReview = Global.VisibilityContext.DMSEvalWithUserRoles.Where(find => find.EvaluationID == profileProgram.selectedEvaluationID && find.EvalUserID == profileProgram.selectedAuditorID).FirstOrDefault();
            DMSEvaluation KPIReview = Global.VisibilityContext.DMSEvaluations.Where(x => x.EvaluationID == profileProgram.selectedEvaluationID).FirstOrDefault();
            if (FirtRateReview != null && KPIReview != null)
            {
                if (FirtRateReview.TotalImageReviewed == null)
                    FirtRateReview.TotalImageReviewed = 0;

                if (((FirtRateReview.TotalImageReviewed * 100) / FirtRateReview.TotalImage) >= KPIReview.ReviewRate && FirtRateReview.TotalImage == FirtRateReview.TotalImageMarked)
                {
                    ViewBag.SetVisBtnUpdateReviewStatus = "1";
                }
                else
                {
                    ViewBag.SetVisBtnUpdateReviewStatus = "0";
                }
                profileProgram.sisNumeric = KPIReview.isNumeric;
            }
            if (profileProgram.sisNumeric == "0")
            {
                ViewBag.btndisabled = Constant.ViewButton;
            }
            else
            {
                ViewBag.btndisabled = string.Empty;
            }
            return View();
        }

        [HttpPost]
        public ActionResult CompletedReview(string sEvalID)
        {
            string result = Utility.Phrase("NotCompletedReview");
            try
            {
                string UserID = SessionHelper.GetSession<string>("UserName");
                var listUserRoleEval = Global.VisibilityContext.DMSEvalWithUserRoles.Where(x => x.EvaluationID == sEvalID && x.EvalRoleID == 2).ToList();
                int count = 0;
                foreach (var elm in listUserRoleEval)
                {
                    if (elm.Status >= 6)
                    {
                        count++;
                    }
                    else
                    {
                        if (elm.EvalUserID.ToUpper() == UserID)
                        {
                            count++;
                            DMSEvalWithUserRole userRole = listUserRoleEval.Where(x => x.EvalUserID.ToUpper() == UserID).FirstOrDefault();
                            if (userRole != null)
                            {
                                userRole.Status = 6;
                                Global.VisibilityContext.SubmitChanges();
                            }
                        }
                    }
                }
                if (count == listUserRoleEval.Count)
                {
                    DMSEvaluation eval = Global.VisibilityContext.DMSEvaluations.FirstOrDefault(x => x.EvaluationID == sEvalID);
                    eval.EvalState = 6;
                    Global.VisibilityContext.SubmitChanges();
                }
                result = Utility.Phrase("CompletedReviewSuccessfully");
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        //Chi tiết xét duyệt hình ảnh
        #region  DetailOutletImageReview

        [HttpPost]
        public JsonResult GetOutletWithAuditor(string EvaluationID, string AuditorID)
        {
            //var test = Global.VisibilityContext.ufn_GetOutletReviewInforByID(EvaluationID, AuditorID).ToList();
            var obj = (from doi in Global.VisibilityContext.ufn_GetOutletReviewInforByID(EvaluationID.Trim(), AuditorID.Trim())
                       select new { doi.MaCuaHang, doi.TenCuaHang }).Distinct().OrderBy(x => x.TenCuaHang);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveReviewImageData(string ImageReviewResult, string ImageReviewReason, string sOutletID, string sImageIndex)
        {
            // Called from SCREEN VSC008: "Chi Tiết Xét Duyệt Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            profileData.ScreenID = "VSC008";
            int idxOutlet = profileProgram.idxOutlet;
            int selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[4] = ImageReviewResult;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[3] = ImageReviewReason;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDReviewed = "1";
            //Session["UserProfile"] = profileData;         
            for (int i = 0; i < profileProgram.EvalOutletsList.Count; i++)
            {
                string ListOfImageID = "";
                string ListOfReviewResult = "";
                string ListOfReviewReason = "";
                string ListOfDaChamDiem = "";
                string EvalID = "";
                string AuditorID = "";
                string OutletID = "";

                for (int j = 0; j < profileProgram.EvalOutletsList[i].EvalImagesList.Count; j++)
                {
                    if (profileProgram.EvalOutletsList[i].hasData)
                    {
                        EvalID = profileProgram.selectedEvaluationID;
                        AuditorID = profileProgram.selectedAuditorID;
                        OutletID = profileProgram.EvalOutletsList[i].OutletID;

                        ListOfImageID = ListOfImageID + profileProgram.EvalOutletsList[i].EvalImagesList[j].ImageID + "&";
                        ListOfReviewResult = ListOfReviewResult + profileProgram.EvalOutletsList[i].EvalImagesList[j].ImageIDEvalResult[4] + "&";
                        ListOfReviewReason = ListOfReviewReason + profileProgram.EvalOutletsList[i].EvalImagesList[j].ImageIDEvalReason[3] + "&";
                        ListOfDaChamDiem = ListOfDaChamDiem + profileProgram.EvalOutletsList[i].EvalImagesList[j].ImageIDReviewed + "&";
                    }
                }
                if (ListOfImageID != "")
                    Global.VisibilityContext.usp_SaveReviewImageResult(EvalID, AuditorID, OutletID, ListOfImageID, ListOfReviewResult, ListOfReviewReason, ListOfDaChamDiem, profileData.LoginID);
            }
            Session["UserProfile"] = profileData;
            Session["ProgramProfile"] = profileProgram;
            return Json("");
        }
        public JsonResult SaveStatusReviewRateData(string ImageReviewResult, string ImageReviewReason, string sOutletID, string sImageIndex)
        {
            // Called from SCREEN VSC008: "Chi Tiết Xét Duyệt Đánh Giá"
            var profileData = this.Session["UserProfile"] as SessionDataModel;
            var profileProgram = this.Session["ProgramProfile"] as EvaluationDataModel;
            profileData.ScreenID = "VSC008";
            int idxOutlet = profileProgram.idxOutlet;
            int selectedImageIdx = profileProgram.EvalOutletsList[idxOutlet].selectedImageIdx;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalResult[4] = ImageReviewResult;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDEvalReason[3] = ImageReviewReason;
            profileProgram.EvalOutletsList[idxOutlet].EvalImagesList[selectedImageIdx].ImageIDReviewed = "1";
            //Session["UserProfile"] = profileData;         
            for (int i = 0; i < profileProgram.EvalOutletsList.Count; i++)
            {
                Global.VisibilityContext.usp_UpdateReviewImageResult(profileProgram.selectedEvaluationID, profileProgram.selectedAuditorID, profileProgram.EvalOutletsList[i].OutletID, profileData.LoginID);
            }
            Session["UserProfile"] = profileData;
            Session["ProgramProfile"] = profileProgram;
            return Json("");
        }
        #endregion
        //Quản lý chấm điểm tự động
        #region Marking Auto
        //"Đánh Giá Hình Ảnh Tự Động"
        [Authorize]
        [ActionAuthorize("Evaluation_AutoEvaluation", true)]
        public ActionResult AutoEvaluation(string sEvalID)
        {
            EvaluationDataModel model = new EvaluationDataModel();
            if (String.IsNullOrEmpty(sEvalID))
            {
                sEvalID = Global.VisibilityContext.DMSEvaluations.Where(x => x.EvalType == "A").OrderByDescending(o => o.CreatedDateTime).First().EvaluationID;
            }
            model.FilterModel.EvaluationID = sEvalID;
            model.FilterModel.EvaluationData = CacheDataHelper.CacheGetAllEvaluation().Where(x => x.EvalType == "A").ToList();
            model.AutoEvalList = Global.VisibilityContext.usp_GetInfoMarkingByID(sEvalID).ToList();
            var listInfoEval = Global.VisibilityContext.usp_GetInfoMarkingByEvalID(sEvalID, string.Empty).ToList();
            usp_GetInfoMarkingByEvalIDResult infoMarking = listInfoEval.FirstOrDefault(x => x.EvaluationID.Trim() == sEvalID.Trim());
            EvalAutoMarkMV EvalMarkInfo = new EvalAutoMarkMV();
            EvalMarkInfo.Evaluation = Global.VisibilityContext.DMSEvaluations.FirstOrDefault(x => x.EvaluationID.Trim() == sEvalID.Trim());
            if (infoMarking != null)
            {
                EvalMarkInfo.TotalImages = infoMarking.TotalImg.Value;
                EvalMarkInfo.DateAutoMarking = infoMarking.EvalDateFrom.Value;
                EvalMarkInfo.ImageMarking = infoMarking.ImgMarking.Value;
                EvalMarkInfo.ImgFakes = infoMarking.ImgFakes.Value;
                EvalMarkInfo.ImgPass = infoMarking.ImgPass.Value;
                EvalMarkInfo.ImgThat = infoMarking.ImgReal.Value;
                EvalMarkInfo.ImgNumberic = infoMarking.ImgHasNumberic.Value;
                EvalMarkInfo.ImgNotPass = infoMarking.ImgNotPass.Value;
                EvalMarkInfo.ImgNotPassNumberic = infoMarking.ImgNotNumberic.Value;
                EvalMarkInfo.ImgErrorMarking = infoMarking.ImgNotYetMarking.Value;
                EvalMarkInfo.ImgNotStandard = infoMarking.ImgNotStandard.Value;
                EvalMarkInfo.OutletMarking = model.AutoEvalList.Where(x => x.Status == 1).Count();
            }
            ViewData["EvalAutoMarkMV"] = EvalMarkInfo;
            Session["EvalAutoMarkMV"] = EvalMarkInfo;
            Session["EvalAutoMarkMV"] = EvalMarkInfo;
            return View(model);
        }

        [Authorize]
        [ActionAuthorize("Evaluation_AutoEvaluation", true)]
        public ActionResult EvalAutoMark(string sEvalID)
        {
            EvalAutoMarkMV model = new EvalAutoMarkMV();
            if (EvalAutoMarkMV == null)
            {
                EvalAutoMarkMV = new EvalAutoMarkMV();
            }
            else
            {
                if (EvalAutoMarkMV.status == Utility.StatusAutoMark.Done && EvalAutoMarkMV.Evaluation.EvaluationID != sEvalID)
                {
                    EvalAutoMarkMV.status = Utility.StatusAutoMark.New;
                }
                else if (EvalAutoMarkMV.status == Utility.StatusAutoMark.New && EvalAutoMarkMV.Evaluation.EvaluationID != sEvalID)
                {
                    EvalAutoMarkMV.status = Utility.StatusAutoMark.New;
                }
                else
                {
                    model = EvalAutoMarkMV;
                    string error = Utility.Phrase("Er_NotyetFinish");
                    ViewData["Error"] = error;
                }
            }

            if (EvalAutoMarkMV.status == Utility.StatusAutoMark.New)
            {
                DMSEvaluation evaluation = Global.VisibilityContext.DMSEvaluations.FirstOrDefault(x => x.EvaluationID == sEvalID);
                if (evaluation != null)
                {
                    model.Evaluation = evaluation;
                    EvalAutoMarkMV.Evaluation = evaluation;
                    List<EvaluationImageClass> imagelist = new List<EvaluationImageClass>();
                    List<usp_GetAutoEvalOutletImageByIDResult> listImages = Global.VisibilityContext.usp_GetAutoEvalOutletImageByID(sEvalID).ToList();
                    foreach (var elm in listImages)
                    {
                        imagelist.Add(new EvaluationImageClass()
                        {
                            EvaluationID = elm.EvaluationID,
                            CustomerID = elm.MaCuaHang,
                            CustomerImageID = elm.MaHinhAnh,
                            ImageName = elm.HinhAnh,
                            InputImagePath = Constant.SalesOrdersImageFolder + elm.HinhAnh,
                            ComparedImagePath = Constant.SalesOrdersImageFolder + elm.AnhGocChup //"THSHOPTMKTE021020B05.jpg" // chỉnh sửa lại
                        });
                    }
                    EvalAutoMarkMV.TotalImages = imagelist.Count();
                    int timeImg = Int32.Parse(ConfigurationSettings.AppSettings["TimePlanMarkingImg"]);
                    EvalAutoMarkMV.TimePlanMarking = (EvalAutoMarkMV.TotalImages != 0) ? ((timeImg * EvalAutoMarkMV.TotalImages) / 60) : 0;
                    model.TotalImages = imagelist.Count();
                    model.TimePlanMarking = EvalAutoMarkMV.TimePlanMarking;
                    model.ImagesList = imagelist;
                    Session["imagelistAutoMarking"] = imagelist;
                }

            }
            else
            {
                List<EvaluationImageClass> list = SessionHelper.GetSession<List<EvaluationImageClass>>("imagelistAutoMarking");
                if (list == null)
                {
                    list = new List<EvaluationImageClass>();
                    List<usp_GetAutoEvalOutletImageByIDResult> listImages = Global.VisibilityContext.usp_GetAutoEvalOutletImageByID(EvalAutoMarkMV.Evaluation.EvaluationID).ToList();
                    foreach (var elm in listImages)
                    {
                        list.Add(new EvaluationImageClass()
                        {
                            EvaluationID = elm.EvaluationID,
                            CustomerID = elm.MaCuaHang,
                            CustomerImageID = elm.MaHinhAnh,
                            ImageName = elm.HinhAnh,
                            InputImagePath = Constant.SalesOrdersImageFolder + elm.HinhAnh,
                            ComparedImagePath = Constant.SalesOrdersImageFolder + elm.AnhGocChup
                            //ComparedImagePath = Constant.OutletImageFolder + elm.AnhGocChup //"THSHOPTMKTE021020B05.jpg" // chỉnh sửa lại
                        });
                    }
                    model.TotalImages = list.Count();
                    model.TimePlanMarking = EvalAutoMarkMV.TotalImages;
                    model.ImagesList = list;
                    Session["imagelistAutoMarking"] = list;
                }
                else
                {
                    model.TotalImages = list.Count();
                    model.TimePlanMarking = EvalAutoMarkMV.TotalImages;
                    model.ImagesList = list;
                }
            }
            Session["EvalAutoMarkMV"] = EvalAutoMarkMV;
            return View(model);
        }
        public ActionResult DetailListImage(string sEvalID, string strType, string customerID)
        {
            EvalAutoMarkMV model = new Models.EvalAutoMarkMV();
            if (SessionHelper.GetSession<EvalAutoMarkMV>("EvalAutoMarkMV") != null)
            {
                model = SessionHelper.GetSession<EvalAutoMarkMV>("EvalAutoMarkMV");
            }
            if (!string.IsNullOrEmpty(sEvalID))
            {
                model.Evaluation = Global.VisibilityContext.DMSEvaluations.SingleOrDefault(x => x.EvaluationID == sEvalID);
            }
            List<usp_GetListImageByResult> imagelist = new List<usp_GetListImageByResult>();
            Utility.TypeID type = (Utility.TypeID)Enum.Parse(typeof(Utility.TypeID), strType);
            if (type == Utility.TypeID.ImgFakes)
            {
                model.NameSceen = Utility.Phrase("ListImgFakes");
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID).Where(x => x.isFinished.Value == true && x.isCaptured.Value == false).ToList();
            }
            else if (type == Utility.TypeID.ImgNotPass)
            {
                model.NameSceen = Utility.Phrase("ListImgNotPass");
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID).Where(x => x.isFinished.Value == true && x.isPassed.Value == false).ToList();
            }
            else if (type == Utility.TypeID.ImgNotStandards)
            {
                model.NameSceen = Utility.Phrase("ListImgNotStandards");
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID).Where(x => x.isFinished.Value == true && x.isAccepted.Value == false).ToList();
            }
            else if (type == Utility.TypeID.ImgMarking)
            {
                model.NameSceen = Utility.Phrase("ListImgMarking");
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID).Where(x => x.isFinished.Value == true).ToList();
            }
            else if (type == Utility.TypeID.Evaluation)
            {
                model.NameSceen = Utility.Phrase("ListImgEvaluation");
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID).ToList();
            }
            else if (type == Utility.TypeID.Outlet)
            {
                model.NameSceen = Utility.Phrase("ListImgOulet");
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID).Where(x => x.isFinished.Value == true && x.CustomerID == customerID).ToList();
            }
            else if (type == Utility.TypeID.ImgNotYesMarking)
            {
                model.NameSceen = Utility.Phrase("ListImgNotYesMarking");
                imagelist = CacheDataHelper.CacheGetListImageByEval(sEvalID).Where(x => x.isFinished.Value == false).ToList();
            }

            model.ListImageBy = imagelist;
            Session["ListImageByCriteria"] = model;

            return View(model);
        }
        public ActionResult RunAutoMark()
        {
            string returnResult = Utility.Phrase("ConnectMatlabError");
            List<EvaluationImageClass> imagelist = SessionHelper.GetSession<List<EvaluationImageClass>>("imagelistAutoMarking");
            if (SessionHelper.GetSession<List<EvaluationImageClass>>("imagelistAutoMarking") != null)
            {
                EvalAutoMarkMV.status = Utility.StatusAutoMark.InProcessing;
                DMSEvaluation evaluation = Global.VisibilityContext.DMSEvaluations.SingleOrDefault(x => x.EvaluationID == EvalAutoMarkMV.Evaluation.EvaluationID);
                if (evaluation != null)
                {
                    evaluation.EvalState = 3;
                    Global.VisibilityContext.SubmitChanges();
                }
                AutoMarkProcessing(EvalAutoMarkMV.Evaluation, imagelist);

                if (EvalAutoMarkMV.CheckConnectMatLab)
                {
                    returnResult = Utility.Phrase("ConnectMatlabSuccessfully");
                }

            }
            return Json(returnResult, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        public void AutoMarkProcessing(DMSEvaluation evaluation, List<EvaluationImageClass> imagelist)
        {
            try
            {
                EvalAutoMarkMV.Evaluation = evaluation;
                EvaluationMatlabClass matlab = new EvaluationMatlabClass();
                string connectMatlab = matlab.MatlabObj.Execute("clc; clear");
                if (string.IsNullOrEmpty(connectMatlab))
                {
                    CustomLog.LogError("StartMatLab run Eval " + evaluation.EvaluationID);
                    EvalAutoMarkMV.CheckConnectMatLab = true;
                }

                matlab.MatlabObj.Execute("cd " + matlab.matlabFuncPath);
                CustomLog.LogError("cd " + matlab.matlabFuncPath);
                if (evaluation.isNumeric == "1")
                {
                    matlab.isNumberic = true;
                }
                if (matlab.isNumberic)
                {
                    List<DMSEvalWithInventoryItem> listItem = Global.VisibilityContext.DMSEvalWithInventoryItems.Where(x => x.EvaluationID == evaluation.EvaluationID).ToList();
                    matlab.numNumericItem = listItem.Count();
                    matlab.arrItemImagePath = listItem.Select(s => matlab.numbericDataPath + "\\" + s.ItemID + ".jpg").ToList();
                    matlab.arrItemNumbericID = listItem.Select(s => s.ItemID.ToString()).ToList();
                    matlab.arrNumFeature = new double[matlab.numNumericItem];
                    matlab.arrPassOrNotTime = new double[matlab.numNumericItem];
                    matlab.arrPassOrNotResult = new int[matlab.numNumericItem];
                }

                List<EvaluationImageClass> imagelist1 = new List<EvaluationImageClass>();
                List<EvaluationImageClass> imagelist2 = new List<EvaluationImageClass>();
                List<EvaluationImageClass> imagelist3 = new List<EvaluationImageClass>();
                List<EvaluationImageClass> imagelist4 = new List<EvaluationImageClass>();
                int number = 0;
                int numberLate = 0;
                if (imagelist.Count > 4)
                {
                    if (imagelist.Count < 200)
                    {
                        number = imagelist.Count / 2;
                        numberLate = imagelist.Count - number;

                        imagelist1 = imagelist.GetRange(0, number);
                        imagelist2 = imagelist.GetRange(number, numberLate);
                    }
                    else if (imagelist.Count < 300)
                    {
                        number = imagelist.Count / 3;
                        numberLate = imagelist.Count - (number * 2);

                        imagelist1 = imagelist.GetRange(0, number);
                        imagelist2 = imagelist.GetRange(number, number);
                        imagelist3 = imagelist.GetRange((number * 2), numberLate);
                    }
                    else
                    {
                        number = imagelist.Count / 4;
                        numberLate = imagelist.Count - (number * 3);

                        imagelist1 = imagelist.GetRange(0, number);
                        imagelist2 = imagelist.GetRange(number, number);
                        imagelist3 = imagelist.GetRange((number * 2), number);
                        imagelist4 = imagelist.GetRange((number * 3), numberLate);
                    }

                }
                else
                {
                    imagelist1 = imagelist;
                }

                //ThreadProcessing(matlab, imagelist, ref EvalAutoMarkMV);
                //CustomLog.LogError("BackgroundWorker_0");
                //BackgroundWorker worker = new BackgroundWorker();
                //worker.DoWork += (sender, e) =>
                //{
                //    CustomLog.LogError("BackgroundWorker_1");
                //    ThreadProcessing(matlab, imagelist, ref EvalAutoMarkMV);
                //};
                //worker.RunWorkerAsync();


                if (imagelist1.Count > 0)
                {
                    Thread thread1 = new Thread(delegate ()
                    {
                        AutoMarking.ThreadProcessing(matlab, imagelist1, ref EvalAutoMarkMV);
                    });
                    thread1.Start();
                }

                if (imagelist2.Count > 0)
                {
                    Thread thread2 = new Thread(delegate ()
                    {
                        AutoMarking.ThreadProcessing(matlab, imagelist2, ref EvalAutoMarkMV);
                    });
                    thread2.Start();
                }

                if (imagelist3.Count > 0)
                {
                    Thread thread3 = new Thread(delegate ()
                    {
                        AutoMarking.ThreadProcessing(matlab, imagelist3, ref EvalAutoMarkMV);
                    });
                    thread3.Start();
                }

                if (imagelist4.Count > 0)
                {
                    Thread thread4 = new Thread(delegate ()
                    {
                        AutoMarking.ThreadProcessing(matlab, imagelist4, ref EvalAutoMarkMV);
                    });
                    thread4.Start();
                }

                // waiting all thread 
                //thread1.Join();
                //thread2.Join();
                //thread3.Join();
                //EvalAutoMarkMV.status = "Done";
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }


        }

        public ActionResult GetDataAutoInProcessing()
        {
            if (EvalAutoMarkMV != null)
            {
                if (EvalAutoMarkMV.status == Utility.StatusAutoMark.InProcessing)
                {
                    if (EvalAutoMarkMV.TotalImages == EvalAutoMarkMV.ImagesProgress)
                    {
                        //try
                        {
                            EvalAutoMarkMV.status = Utility.StatusAutoMark.Done;
                            DMSEvaluation evaluation = Global.VisibilityContext.DMSEvaluations.SingleOrDefault(x => x.EvaluationID == EvalAutoMarkMV.Evaluation.EvaluationID);
                            if (evaluation != null)
                            {
                                evaluation.EvalState = 6;
                                Global.VisibilityContext.SubmitChanges();
                            }
                        }
                        //catch (Exception ex)
                        //{
                        //    throw;
                        //}

                    }
                    EvalAutoMarkMV.TimeAverage = EvalAutoMarkMV.TimeMarking / EvalAutoMarkMV.ImagesProgress;

                }

            }

            EvalAutoMarkMV data = new EvalAutoMarkMV();
            data = EvalAutoMarkMV;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetailImages(string sEvalID, string customerID, int imageID)
        {
            EvalAutoMarkMV model = new EvalAutoMarkMV();
            if (imageID == null || imageID == 0)
            {
                model.ListImageBy = CacheDataHelper.CacheGetListImageByEval(sEvalID).Where(x => x.CustomerID == customerID).ToList();
                if (model.ListImageBy.Count > 0)
                {
                    imageID = model.ListImageBy[0].CustomerImageID;
                }
            }
            else
            {
                if (SessionHelper.GetSession<EvalAutoMarkMV>("ListImageByCriteria") != null)
                {
                    model = SessionHelper.GetSession<EvalAutoMarkMV>("ListImageByCriteria");

                }
            }
            ViewData["customerID"] = customerID;
            ViewData["imageID"] = imageID;
            return View(model);
        }

        public ActionResult MarkingAutoOneImage(string sEvalID, string customerID, int imageID)
        {
            try
            {
                EvaluationImageClass elm = new EvaluationImageClass();
                string errorMarking = "";
                if (SessionHelper.GetSession<EvalAutoMarkMV>("ListImageByCriteria") != null)
                {
                    EvaluationMatlabClass matlab = new EvaluationMatlabClass();

                    usp_GetListImageByResult infoImage = new usp_GetListImageByResult();
                    var list = SessionHelper.GetSession<EvalAutoMarkMV>("ListImageByCriteria").ListImageBy;
                    infoImage = list.SingleOrDefault(x => x.EvaluationID.Trim() == sEvalID.Trim() && x.CustomerID.Trim() == customerID.Trim() && x.CustomerImageID == imageID);
                    elm.EvaluationID = infoImage.EvaluationID;
                    elm.CustomerID = infoImage.CustomerID;
                    elm.CustomerImageID = infoImage.CustomerImageID;
                    elm.ImageName = infoImage.ImageFileName;
                    elm.InputImagePath = Constant.SalesOrdersImageFolder + infoImage.ImageFileName;
                    elm.ComparedImagePath = Constant.SalesOrdersImageFolder + infoImage.ImageFileCompare;
                    DMSEvaluation evaluation = Global.VisibilityContext.DMSEvaluations.SingleOrDefault(x => x.EvaluationID == sEvalID.Trim());

                    string connectMatlab = matlab.MatlabObj.Execute("clc; clear");
                    matlab.MatlabObj.Execute("cd " + matlab.matlabFuncPath);
                    if (evaluation.isNumeric == "1")
                    {
                        matlab.isNumberic = true;
                    }
                    if (matlab.isNumberic)
                    {
                        List<DMSEvalWithInventoryItem> listItem = Global.VisibilityContext.DMSEvalWithInventoryItems.Where(x => x.EvaluationID == evaluation.EvaluationID).ToList();
                        matlab.numNumericItem = listItem.Count();
                        matlab.arrItemImagePath = listItem.Select(s => matlab.numbericDataPath + "\\" + s.ItemID + ".jpg").ToList();
                        matlab.arrItemNumbericID = listItem.Select(s => s.ItemID.ToString()).ToList();
                        matlab.arrNumFeature = new double[matlab.numNumericItem];
                        matlab.arrPassOrNotTime = new double[matlab.numNumericItem];
                        matlab.arrPassOrNotResult = new int[matlab.numNumericItem];
                    }


                    int ProcessType = 0;
                    object output;
                    object[] result;

                    elm.CheckImageValidOrNot();
                    if (elm.isExistImg)
                    {
                        try
                        {
                            ProcessType = 1;
                            output = null;
                            matlab.MatlabObj.Feval(matlab.FunctionName, 10, out output, elm.InputImagePath, elm.ComparedImagePath, "None", matlab.RealFakeThreshold, matlab.StandardOrNotThreshold, matlab.PassOrNotThreshold, ProcessType, matlab.BagPath, matlab.MeanMhistRGBPath, matlab.VLFeat_LibPath);
                            result = output as object[];

                            elm.RealOrFakeResult = Convert.ToInt16(result[0]);
                            elm.PercentFake = Convert.ToDouble(result[3]);
                            elm.RealOrFakeTime = Convert.ToDouble(result[6]);
                            elm.RealOrFakeResult = (elm.PercentFake >= matlab.RealFakeThreshold) ? 0 : 1;
                            elm.ErrorStatus = elm.ErrorStatus + Convert.ToString(result[9]);

                            ProcessType = 2;
                            output = null;
                            matlab.MatlabObj.Feval(matlab.FunctionName, 10, out output, elm.InputImagePath, elm.ComparedImagePath, "None", matlab.RealFakeThreshold, matlab.StandardOrNotThreshold, matlab.PassOrNotThreshold, ProcessType, matlab.BagPath, matlab.MeanMhistRGBPath, matlab.VLFeat_LibPath);
                            result = output as object[];
                            elm.StandardOrNotResult = Convert.ToInt16(result[1]);
                            elm.NumCorrelation = Convert.ToDouble(result[4]);
                            elm.StandardOrNotTime = Convert.ToDouble(result[7]);
                            elm.StandardOrNotResult = (elm.NumCorrelation >= matlab.StandardOrNotThreshold) ? 1 : 0;
                            elm.ErrorStatus = elm.ErrorStatus + Convert.ToString(result[9]);

                            ProcessType = 3; //Run check Item exists or not
                            List<string> numberExist = new List<string>();
                            for (int k = 0; k < matlab.numNumericItem; k++)
                            {
                                output = null;
                                matlab.MatlabObj.Feval(matlab.FunctionName, 10, out output, elm.InputImagePath, elm.ComparedImagePath, matlab.arrItemImagePath[k], matlab.RealFakeThreshold, matlab.StandardOrNotThreshold, matlab.PassOrNotThreshold, ProcessType, matlab.BagPath, matlab.MeanMhistRGBPath, matlab.VLFeat_LibPath);
                                result = output as object[];
                                matlab.arrNumFeature[k] = Convert.ToDouble(result[5]);
                                matlab.arrPassOrNotTime[k] = Convert.ToDouble(result[8]);
                                matlab.arrPassOrNotResult[k] = (matlab.arrNumFeature[k] >= matlab.PassOrNotThreshold) ? 1 : 0;
                                elm.ErrorStatus = elm.ErrorStatus + Convert.ToString(result[9]);
                                if (matlab.arrPassOrNotResult[k] == 1)
                                {
                                    numberExist.Add(matlab.arrItemNumbericID[k]);
                                }
                            }

                            elm.PassOrNotResult = matlab.arrPassOrNotResult.Aggregate(1, (a, b) => a * b);
                            elm.PassOrNotNumberic = matlab.arrPassOrNotResult.Aggregate(1, (a, b) => a * b);
                            elm.PassOrNotTime = matlab.arrPassOrNotTime.Aggregate(0.0, (a, b) => a + b) / matlab.numNumericItem;
                            elm.NumFeature = matlab.arrNumFeature.Aggregate(0.0, (a, b) => a + b);

                            elm.StandardOrNotResult = elm.StandardOrNotResult * elm.RealOrFakeResult;
                            elm.PassOrNotResult = elm.PassOrNotResult * elm.RealOrFakeResult * elm.StandardOrNotResult;
                            elm.ErrorStatus = (elm.ErrorStatus == "") ? "No Error" : elm.ErrorStatus;

                            using (NGVisibilityDataContext context = new NGVisibilityDataContext())
                            {
                                context.usp_UpdateAutoEvalOutletImageByID(
                                    elm.EvaluationID,
                                    elm.CustomerID,
                                    elm.CustomerImageID,
                                    elm.RealOrFakeResult,
                                    elm.StandardOrNotResult,
                                    elm.PassOrNotResult,
                                    string.Join(";", numberExist.ToArray())
                                    );
                            }
                            elm.Marking = true;
                        }
                        catch (Exception ex)
                        {
                            errorMarking = Utility.Phrase("ErrorMarking");
                        }
                    }
                    else
                    {
                        errorMarking = Utility.Phrase("NotExistImage");
                    }

                }
                var returnResult = new { data = elm, errorMarking = errorMarking };
                return Json(returnResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
        }

        public ActionResult CancellDataAuto(string sEvalID)
        {
            var mess = "";
            if (EvalAutoMarkMV.Evaluation.EvaluationID == sEvalID)
            {
                // chưa xử lý được

            }
            else
            {
                // cần stored xử lý
            }
            return Json(mess, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AutoMarkingExportExecl(string sEvalID)
        {
            List<usp_GetInfoMarkingByIDResult> result = Global.VisibilityContext.usp_GetInfoMarkingByID(sEvalID).ToList();
            return GridViewExtension.ExportToXlsx(AutoMarkingSettingExport(), result);
        }
        public ActionResult AutoMarkingExportPDF(string sEvalID)
        {
            List<usp_GetInfoMarkingByIDResult> result = Global.VisibilityContext.usp_GetInfoMarkingByID(sEvalID).ToList();
            return GridViewExtension.ExportToPdf(AutoMarkingSettingExport(), result);
        }
        private static GridViewSettings AutoMarkingSettingExport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "AutoMarkingExport";
            settings.KeyFieldName = "CustomerID";
            settings.Width = Unit.Percentage(100);
            settings.Styles.Header.Font.Bold = true;
            settings.Styles.Header.HorizontalAlign = HorizontalAlign.Center;

            settings.SettingsExport.Landscape = true;
            settings.SettingsExport.TopMargin = 0;
            settings.SettingsExport.LeftMargin = 0;
            settings.SettingsExport.RightMargin = 0;
            settings.SettingsExport.BottomMargin = 0;
            settings.SettingsExport.PaperKind = PaperKind.A4;
            settings.Settings.ShowPreview = true;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.RowType == GridViewRowType.Data && e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            settings.Columns.Add(field =>
            {
                field.FieldName = "EvaluationID";
                field.Caption = Utility.Phrase("EvaluationID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "CustomerID";
                field.Caption = Utility.Phrase("CustomerID");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "CustomerName";
                field.Caption = Utility.Phrase("OutletName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Address";
                field.Caption = Utility.Phrase("OutletAdress");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ProvinceName";
                field.Caption = Utility.Phrase("ProvinceName");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "TotalImg";
                field.Caption = Utility.Phrase("TotalImages");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgFakes";
                field.Caption = Utility.Phrase("ImgFakes");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgNotStandard";
                field.Caption = Utility.Phrase("ImgNotStandard");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgNotPass";
                field.Caption = Utility.Phrase("ImgNotPass");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "Phone";
                field.Caption = Utility.Phrase("Phone");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "ImgHasNumberic";
                field.Caption = Utility.Phrase("ImgHasNumberic");
            });
            settings.Columns.Add(field =>
            {
                field.FieldName = "StrStatus";
                field.Caption = Utility.Phrase("Status");
                field.UnboundType = DevExpress.Data.UnboundColumnType.String;
            });
            settings.CustomUnboundColumnData = (s, e) =>
            {
                if (e.Column.FieldName == "StrStatus")
                {
                    string Status = (e.GetListSourceFieldValue("Status")).ToString();
                    e.Value = Utility.Phrase(Status + "_OutletMark");
                };
            };
            settings.Columns.Add(field =>
            {
                field.FieldName = "UserID";
                field.Caption = Utility.Phrase("Marker");
            });

            return settings;
        }
        #endregion
        //Quản lý lý do
        #region Reason management 
        [Authorize]
        [ActionAuthorize("Evaluation_DisplayReasonManagement", true)]
        public ActionResult DisplayReasonManagement(string Program, string TypeReasonFilter)
        {

            DisplayReasonVM model = new DisplayReasonVM();
            try
            {
                model.SetDataFilter(Program, TypeReasonFilter);
                model.Result = Global.VisibilityContext.DisplayReasons.Where(x => x.DisplayID != 0 && x.DisplayID == model.DisplayID && (string.IsNullOrEmpty(model.TypeReason) || x.TypeOfReason == model.TypeReason)).ToList();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_ReasonManagement")]
        public ActionResult AddReason(string Program, string ReasonCode, string ReasonName, string ReasonDescription, string ReasonType, string ReasonActive)
        {
            DisplayReasonVM data = new DisplayReasonVM();
            DisplayReason model = new DisplayReason();
            if (!string.IsNullOrEmpty(Program) && !string.IsNullOrEmpty(ReasonCode) && !string.IsNullOrEmpty(ReasonType))
            {
                model.DisplayID = int.Parse(Program);
                model.ReasonCode = ReasonCode;
                model.ShortDescription = ReasonName;
                model.LongDescription = ReasonDescription;
                model.TypeOfReason = ReasonType;
                if (string.IsNullOrEmpty(ReasonActive))
                {
                    model.Active = false;
                }
                else
                {
                    model.Active = true;
                }
                model.CreatedByID = SessionHelper.GetSession<string>("UserName");
                model.CreatedDateTime = DateTime.Now;
                model.LastModifiedByID = SessionHelper.GetSession<string>("UserName");
                model.LastModifiedDateTime = DateTime.Now;
                Global.VisibilityContext.DisplayReasons.InsertOnSubmit(model);
                Global.VisibilityContext.SubmitChanges();
                TempData["Messages"] = Utility.Phrase("SaveAddReasonSuccessfully");
            }
            data.SetDataFilter(Program);
            data.Result = Global.VisibilityContext.DisplayReasons.Where(x => x.DisplayID == model.DisplayID).ToList();
            return View("DisplayReasonManagement", data);
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_ReasonManagement")]
        public ActionResult EditReason(string Program, string ReasonCode, string ReasonName, string ReasonDescription, string ReasonType, string ReasonActive)
        {
            DisplayReasonVM data = new DisplayReasonVM();
            DisplayReason model = new DisplayReason();
            if (!string.IsNullOrEmpty(Program) && !string.IsNullOrEmpty(ReasonCode) && !string.IsNullOrEmpty(ReasonType))
            {
                model = Global.VisibilityContext.DisplayReasons.Where(x => x.DisplayID == int.Parse(Program) && x.ReasonCode == ReasonCode).FirstOrDefault();
                model.ShortDescription = ReasonName;
                model.LongDescription = ReasonDescription;
                model.TypeOfReason = ReasonType;
                if (string.IsNullOrEmpty(ReasonActive))
                {
                    model.Active = false;
                }
                else
                {
                    model.Active = true;
                }
                model.LastModifiedByID = SessionHelper.GetSession<string>("UserName");
                model.LastModifiedDateTime = DateTime.Now;
                Global.VisibilityContext.SubmitChanges();
                TempData["Messages"] = Utility.Phrase("SaveuUpdateReasonSuccessfully");
            }
            data.SetDataFilter(Program);
            data.Result = Global.VisibilityContext.DisplayReasons.Where(x => x.DisplayID == model.DisplayID).ToList();
            return View("DisplayReasonManagement", data);
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_ReasonManagement")]
        public ActionResult CheckReasonCode(string Program, string ReasonCode)
        {
            string result = "";
            if (!string.IsNullOrEmpty(Program) && !string.IsNullOrEmpty(ReasonCode))
            {
                var data = Global.VisibilityContext.DisplayReasons.Where(x => x.DisplayID == int.Parse(Program) && x.ReasonCode == ReasonCode).ToList();
                if (data.Count() >= 1)
                {
                    result = Utility.Phrase("ReasonCodeIsExist");
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        #endregion
        // Quản lý sản phẩm
        #region Inventory management 
        [Authorize]
        [ActionAuthorize("Evaluation_InventoryManagement", true)]
        public ActionResult InventoryManagement(string Category, string Type, string FilterInventoryName)
        {
            InventoryVM model = new InventoryVM();
            try
            {
                model.SetDataFilter(Category, Type, FilterInventoryName);
                var data = Global.VisibilityContext.DMSEvalInventoryItems.ToList();
                var image = Global.VisibilityContext.InventoryPaths.ToList();
                List<SelectListItem> listCategory = new List<SelectListItem>();
                listCategory.Add(new SelectListItem() { Text = Utility.Phrase("SelectAll"), Value = "0", Selected = (model.CategoryID == 0) ? true : false });
                foreach (var elm in data.GroupBy(x => new { x.CategoryID, x.CategoryName }).Select(x => new { x.Key.CategoryID, x.Key.CategoryName }).ToList())
                {
                    listCategory.Add(
                        new SelectListItem()
                        {
                            Text = elm.CategoryName,
                            Value = elm.CategoryID.ToString(),
                            Selected = (model.CategoryID == elm.CategoryID) ? true : false
                        }
                    );
                }
                var settingName = Global.Context.CustomSettings.Where(t => t.SettingCode == "InventoryItemPath").Select(t => t.SettingName).FirstOrDefault();
                var settingValue = Global.Context.CustomSettings.Where(t => t.SettingCode == "InventoryItemPath").Select(t => t.SettingValue).FirstOrDefault();
                model.ListCategory = listCategory;
                model.Result = data.Where(x => (model.CategoryID == 0 || x.CategoryID == model.CategoryID) && (model.Type == -1 || x.Type == model.Type) && (string.IsNullOrEmpty(FilterInventoryName) || x.Descr.Contains(model.InventoryName))).ToList();
                model.ListInventoryPath = image;
                model.StorageRoot = ("~" + settingValue + settingName);
                Session["InventoryVM"] = model;
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            return View(model);
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_InventoryManagement")]
        public ActionResult AddInventory(DMSEvalInventoryItem data)
        {
            InventoryVM model = new InventoryVM();
            if (!string.IsNullOrEmpty(data.InventoryCD))
            {
                data.Type = 0;
                data.CreatedByID = new Guid();
                data.CreatedDateTime = DateTime.Now;
                data.LastModifiedByID = new Guid();
                data.LastModifiedDateTime = DateTime.Now;
                Global.VisibilityContext.DMSEvalInventoryItems.InsertOnSubmit(data);
                Global.VisibilityContext.SubmitChanges();
                TempData["Messages"] = Utility.Phrase("SaveAddInventorySuccessfully");
            }
            if (SessionHelper.GetSession<InventoryVM>("InventoryVM") != null)
            {
                model = SessionHelper.GetSession<InventoryVM>("InventoryVM");
                model.Result = Global.VisibilityContext.DMSEvalInventoryItems.Where(x => (model.CategoryID == 0 || x.CategoryID == model.CategoryID) && (model.Type == -1 || x.Type == model.Type)).ToList();
            }
            else
            {
                return RedirectToAction("InventoryManagement");
            }
            return RedirectToAction("InventoryManagement");
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_InventoryManagement")]
        public ActionResult EditInventory(DMSEvalInventoryItem data)
        {
            DMSEvalInventoryItem inventory = new DMSEvalInventoryItem();
            if (!string.IsNullOrEmpty(data.InventoryCD))
            {
                inventory = Global.VisibilityContext.DMSEvalInventoryItems.Where(x => x.InventoryID == data.InventoryID).FirstOrDefault();
                inventory.Descr = data.Descr;
                inventory.VendorID = data.VendorID;
                inventory.VendorName = data.VendorName;
                inventory.LastModifiedDateTime = DateTime.Now;
                inventory.CategoryID = data.CategoryID;
                inventory.CategoryName = data.CategoryName;
                inventory.Active = data.Active;
                Global.VisibilityContext.SubmitChanges();
                TempData["Messages"] = Utility.Phrase("SaveUpdateInventorySuccessfully");
            }
            return RedirectToAction("InventoryManagement");
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_InventoryManagement")]
        public ActionResult CheckInventoryCode(string inventoryCD)
        {
            string result = "";
            if (!string.IsNullOrEmpty(inventoryCD))
            {
                var data = Global.VisibilityContext.DMSEvalInventoryItems.Where(x => x.InventoryCD == inventoryCD.Trim()).ToList();
                if (data.Count() >= 1)
                {
                    result = Utility.Phrase("InventoryCDIsExist");
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_InventoryManagement")]
        public ActionResult GetVendorName(int vendorID)
        {
            string result = "";
            if (vendorID > 0)
            {
                result = Global.VisibilityContext.DMSEvalInventoryItems.Where(x => x.VendorID == vendorID.ToString() && x.Type == 0).Select(t => t.VendorName).FirstOrDefault();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_InventoryManagement")]
        public ActionResult GetCategoryName(int categoryID)
        {
            string result = "";
            if (categoryID > 0)
            {
                result = Global.VisibilityContext.DMSEvalInventoryItems.Where(x => x.CategoryID == categoryID && x.Type == 0).Select(t => t.CategoryName).FirstOrDefault();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [ActionAuthorize("Evaluation_InventoryManagement")]
        public void Delete(string id)
        {
            var filename = id;
            var filePath = Path.Combine(StorageRoot, filename);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [ActionAuthorize("Evaluation_InventoryManagement")]
        public void Download(string id)
        {
            var filename = id;
            var filePath = Path.Combine(StorageRoot, filename);

            var context = HttpContext;

            if (System.IO.File.Exists(filePath))
            {
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + "\"");
                context.Response.ContentType = "application/octet-stream";
                context.Response.ClearContent();
                context.Response.WriteFile(filePath);
            }
            else
                context.Response.StatusCode = 404;
        }

        [HttpPost]
        [ActionAuthorize("Evaluation_InventoryManagement")]
        public ActionResult UploadFiles(IEnumerable<HttpPostedFileBase> files, string InventoryID, string removeImg)
        {
            removeImg = removeImg.Trim(',');
            var statuses = new List<ViewDataUploadFilesResult>();
            if (string.IsNullOrEmpty(InventoryID) || InventoryID == "0")
                return Json(statuses, JsonRequestBehavior.AllowGet);

            // insert image
            var headers = Request.Headers;
            if (files != null && files.Count() > 0)
            {
                UploadWholeFile(files, statuses);
            }

            foreach (var item in statuses)
            {
                var data = new InventoryPath();
                data.InventoryID = int.Parse(InventoryID);
                data.InventoryPathName = item.name;
                data.CreatedBy = new Guid();
                data.CreatedDateTime = DateTime.Now;
                Global.VisibilityContext.InventoryPaths.InsertOnSubmit(data);
                Global.VisibilityContext.SubmitChanges();
            }

            // check and delete image
            List<string> listDeleteImage = (removeImg == "") ? null : removeImg.Split(',').ToList();
            if(listDeleteImage != null && listDeleteImage.Count > 0)
            {
                List<InventoryPath> listImage = (from t in Global.VisibilityContext.InventoryPaths.ToList()
                                                 join l in listDeleteImage.ToList()
                                                 on t.InventoryPathName equals l.ToString()
                                                 select t).ToList();
                foreach (var item in listImage)
                {
                    Delete(item.InventoryPathName);
                    Global.VisibilityContext.InventoryPaths.DeleteOnSubmit(item);
                }
                Global.VisibilityContext.SubmitChanges();
            }
            

            JsonResult result = Json(statuses);
            result.ContentType = "text/plain";

            return result;
        }

        private string EncodeFile(string fileName)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        private void UploadWholeFile(IEnumerable<HttpPostedFileBase> files, List<ViewDataUploadFilesResult> statuses)
        {
            foreach (var item in files)
            {
                if (item != null && item.ContentLength > 0)
                {
                    var newFileName = DateTime.Now.ToString("ddMMyyyyhhmmssfff") + Path.GetExtension(item.FileName);

                    var fullPath = Path.Combine(StorageRoot, newFileName);

                    item.SaveAs(fullPath);

                    statuses.Add(new ViewDataUploadFilesResult()
                    {
                        name = newFileName,
                        size = item.ContentLength,
                        type = item.ContentType,
                        //url = "/Home/Download/" + file.FileName,
                        delete_url = "/Evaluation/Delete?id=" + newFileName,
                        //thumbnail_url = @"data:image/png;base64," + EncodeFile(fullPath),
                        delete_type = "GET",
                    });
                }
            }
        }

        #endregion
    }
}
