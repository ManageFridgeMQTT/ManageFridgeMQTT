using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
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
        public Timer TimerTick;

        public void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                string log = Helper.ByteToString(e.Message);
                string clientID = (string)Utility.Helper.GetPropertyValue(sender, "ClientId");
                bool isConnected = (bool)Utility.Helper.GetPropertyValue(sender, "IsConnected");

                List<string> topicList = e.Topic.Split(new char[] { '/' }).ToList();
                if (topicList.Count >= 3)
                {
                    if (topicList[0].ToString() == "info")
                    {
                        string strThietBiID = topicList[2].ToString();
                        ModelMess modelMess = Helper.ParseMessToModel(e.Message.ToList());
                        log += Environment.NewLine + modelMess.WriteByteLog();
                        ThietBiStatusMess TbMess = Helper.ParseMessToValue(modelMess, strThietBiID);
                        if (!string.IsNullOrEmpty(strThietBiID))
                        {
                            using (DeviceTrackingDataContext Context = new DeviceTrackingDataContext())
                            {
                                Context.UpdateThieBiSatusMess(strThietBiID, TbMess.CommandType, TbMess.CommandId, TbMess.CommandAction, TbMess.Loai, TbMess.StatusMay, TbMess.Time, TbMess.TrangThai, TbMess.Latitude, TbMess.Longitude);
                            }
                            log += Environment.NewLine + TbMess.WriteLog();
                        }
                    }
                }
                CustomLog.LogDevice(log);
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

        public Gateway()
        {
            int interval = 600000;
            if(!string.IsNullOrEmpty(ConfigurationManager.AppSettings["TimePingInterval"].ToString())){
                Int32.Parse(ConfigurationManager.AppSettings["TimePingInterval"].ToString());
            }
            this.TimerTick = new Timer();
            this.TimerTick.Interval = interval;
            this.TimerTick.Elapsed += new ElapsedEventHandler(Time_Elapsed);
            this.TimerTick.AutoReset = true;
        }
        public void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (this.client.IsConnected)
                {
                    byte[] ping = new byte[] { 0x03, 0x01, 0x01 };
                    this.client.Publish("ping", Encoding.UTF8.GetBytes("ping"));
                }
                else
                {
                    #region Config
                    this.client = new MqttClient(IPAddress.Parse("45.117.80.39"));
                    string clientID = "1111AAAAzzz";
                    this.client.Connect(clientID);
                    CustomLog.LogError("reconnect thanh cong");
                    string[] topic = { "#", "Test/#" };

                    byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
                    this.client.Subscribe(topic, qosLevels);

                    this.client.MqttMsgPublishReceived += this.client_MqttMsgPublishReceived;
                    this.client.MqttMsgSubscribed += this.client_MqttMsgSubscribed;
                    this.client.MqttMsgUnsubscribed += this.client_MqttMsgUnsubscribed;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
        }
    }
}