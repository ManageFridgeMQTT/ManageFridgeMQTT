using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ClientMQTT
{
    public class Gateway
    {
        public MqttClient client;

        public void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                CustomLog.LogArrayByte(e.Message);
                //string clientID = (string)ClientMQTT.Helper.GetPropertyValue(sender, "ClientId");
                //bool isConnected = (bool)ClientMQTT.Helper.GetPropertyValue(sender, "IsConnected");

                //List<string> topicList = e.Topic.Split(new char[] { '/' }).ToList();
                //if (topicList.Count > 0)
                //{
                //    if (topicList[0].ToString() == "info")
                //    {
                //        string strThietBiID = topicList[2].ToString();

                //        ModelMess modelMess = ClientMQTT.Helper.ParseMessToModel(e.Message.ToList());
                //        ThietBiStatusMess TbMess = ClientMQTT.Helper.ParseMessToValue(modelMess, strThietBiID);
                //        CustomLog.LogError(TbMess.WriteLog());
                //        if(string.IsNullOrEmpty(strThietBiID))
                //        {
                           
                //        }

                //    }
                //}
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            
        }

        public void client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            string clientID = (string)ClientMQTT.Helper.GetPropertyValue(sender, "ClientId");
            string result = "MqttMsgUnsubscribed------ ClientID: " + clientID + " --- MessageId: " + e.MessageId;
            CustomLog.LogError(result);
        }

        public void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            string clientID = (string)ClientMQTT.Helper.GetPropertyValue(sender, "ClientId");
            string result = "MqttMsgSubscribed----- ClientID: " + clientID + " --- MessageId: " + e.MessageId + " --- GrantedQoSLevels: " + Encoding.UTF8.GetString(e.GrantedQoSLevels);
            CustomLog.LogError(result);
        }
    }

}