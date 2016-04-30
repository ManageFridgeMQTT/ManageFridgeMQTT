using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using WebManageFridgeMQTT.Models;

namespace WebManageFridgeMQTT.Utility
{
    public class Gateway
    {
        public MqttClient client;

        public void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                string clientID = (string)Utility.Helper.GetPropertyValue(sender, "ClientId");
                bool isConnected = (bool)Utility.Helper.GetPropertyValue(sender, "IsConnected");

                List<string> topicList = e.Topic.Split(new char[] { '/' }).ToList();
                if (topicList.Count > 0)
                {
                    if (topicList[0].ToString() == "info")
                    {
                        string strThietBiID = topicList[1].ToString();

                        ModelMess modelMess = Helper.ParseMessToModel(e.Message.ToList());
                        ThietBiStatusMess TbMess = Helper.ParseMessToValue(modelMess, strThietBiID);
                        if(string.IsNullOrEmpty(strThietBiID))
                        {
                            Global.Context.UpdateThieBiSatusMess(strThietBiID, TbMess.CommandType, TbMess.CommandId, TbMess.CommandAction, TbMess.Loai, TbMess.StatusMay, TbMess.Time, TbMess.TrangThai, TbMess.Latitude, TbMess.Longitude);
                        }

                        //if (Array.Equals(modelMess.CommandAction, ConstParam.BaoDuong) || Array.Equals(modelMess.CommandAction, ConstParam.DiChuyen) || Array.Equals(modelMess.CommandAction, ConstParam.Khoan) || Array.Equals(modelMess.CommandAction, ConstParam.Cau))
                        //{ 
                        //}
                        
                        /// chỉnh sửa các trường hợp ở đây

                    }
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            
        }

        public void client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            string clientID = (string)Utility.Helper.GetPropertyValue(sender, "ClientId");
            string result = "MqttMsgUnsubscribed------ ClientID: " + clientID + " --- MessageId: " + e.MessageId;
            CustomLog.LogError(result);
        }

        public void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            string clientID = (string)Utility.Helper.GetPropertyValue(sender, "ClientId");
            string result = "MqttMsgSubscribed----- ClientID: " + clientID + " --- MessageId: " + e.MessageId + " --- GrantedQoSLevels: " + Encoding.UTF8.GetString(e.GrantedQoSLevels);
            CustomLog.LogError(result);
        }
    }

}