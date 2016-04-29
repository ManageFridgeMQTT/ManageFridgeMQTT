using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace WebManageFridgeMQTT.Utility
{
    public class Gateway
    {
        public MqttClient client;

        public void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            List<string> topicList = e.Topic.Split(new char[] { '/' }).ToList();
            if (topicList.Count > 0)
            {
                if (topicList[0].ToString() == "info")
                {
                    var deserializer = new JavaScriptSerializer();
                    MessageConfig results = deserializer.Deserialize<MessageConfig>(Encoding.UTF8.GetString(e.Message));
                    try
                    {
                        List<ObjParamSP> listParam = new List<ObjParamSP>();
                        listParam.Add(new ObjParamSP() { Key = "CommandType", Value = results.CommandType });
                        listParam.Add(new ObjParamSP() { Key = "CommandId", Value = results.CommandId });
                        listParam.Add(new ObjParamSP() { Key = "CommandAction", Value = results.CommandAction });
                        listParam.Add(new ObjParamSP() { Key = "ResultData", Value = deserializer.Serialize(results.Data) });
                        listParam.Add(new ObjParamSP() { Key = "MessStatus", Value = results.Status });
                        listParam.Add(new ObjParamSP() { Key = "Topic", Value = e.Topic });
                        listParam.Add(new ObjParamSP() { Key = "CreateTime", Value = DateTime.Now });
                        var sdsdsd = Utility.Helper.QueryStoredProcedure("LogMessages", listParam);
                    }
                    catch (Exception ex)
                    {
                        CustomLog.LogError(ex);
                        throw;
                    }



                }
            }

            string clientID = (string)Utility.Helper.GetPropertyValue(sender, "ClientId");
            bool isConnected = (bool)Utility.Helper.GetPropertyValue(sender, "IsConnected");
            string result = "MqttMsgPublishReceived----  ClientID: " + clientID + " --- Topic: " + e.Topic + " --- QosLevel: " + e.QosLevel + " --- Message: " + Encoding.UTF8.GetString(e.Message);
            CustomLog.LogError(result);
            //try
            //{
            //    List<ObjParamSP> listParam = new List<ObjParamSP>();
            //    listParam.Add(new ObjParamSP() { Key = "ClientID", Value = clientID });
            //    listParam.Add(new ObjParamSP() { Key = "ClientName", Value = e.Topic });
            //    listParam.Add(new ObjParamSP() { Key = "Description", Value = "sdsdssd" });
            //    listParam.Add(new ObjParamSP() { Key = "CategoryID", Value = 1 });
            //    listParam.Add(new ObjParamSP() { Key = "ClientStatus", Value = 1 });
            //    listParam.Add(new ObjParamSP() { Key = "UpdateTime", Value = DateTime.Now });
            //    var ab = Utility.Helper.QueryStoredProcedure("SetClient", listParam);
            //}
            //catch (Exception ex)
            //{
            //    CustomLog.LogError(ex);
            //    throw;
            //}
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
     
    public class MessageFrames
    {
        public byte[] CommandType { get; set;}
        public byte[] CommandId { get; set; }
        public byte[] CommandAction { get; set; }
        public byte[] Status { get; set; }
        public byte[] Data { get; set; }

        public MessageFrames()
        {
            this.CommandType = new byte[1];
            this.CommandId = new byte[4];
            this.CommandAction = new byte[1];
            this.Status = new byte[1];
        }

        public byte[] GatewayReadTemp(byte[] CommandId)
        {
            List<byte> result = new List<byte>();
            result.Add(0x01);
            result.Add(0x01);
            result.Add(0x02);
            result.AddRange(CommandId);
            result.Add(0x03);
            result.Add(0x01);   // đọc nhiệt độ
            result.Add(0x24);
            result.Add(0x01);
            result.Add(0x03);
            result.Add(0x01);
            result.Add(0x01);
            result.Add(0x01);  //tức thời
            return result.ToArray();
        }
    }



}