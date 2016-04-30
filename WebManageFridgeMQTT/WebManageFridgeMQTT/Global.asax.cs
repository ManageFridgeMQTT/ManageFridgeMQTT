using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using uPLibrary.Networking.M2Mqtt;
using WebManageFridgeMQTT.Utility;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System.Web.Script.Serialization;

namespace WebManageFridgeMQTT
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        //public MqttClient client;
        string clientID;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            CustomLog.LogPath = HttpContext.Current.Server.MapPath("~/Logs/");


            //#region Config
            //Gateway gateway = new Gateway();
            //gateway.client = new MqttClient(IPAddress.Parse("127.0.0.1"));
            //clientID = "1111AAAA";
            //gateway.client.Connect(clientID);
            //CustomLog.LogError("connect thanh cong");
            //string[] topic = { "#", "Test/#" };

            //byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
            //gateway.client.Subscribe(topic, qosLevels);

            //gateway.client.MqttMsgPublishReceived += gateway.client_MqttMsgPublishReceived;
            //gateway.client.MqttMsgSubscribed += gateway.client_MqttMsgSubscribed;
            //gateway.client.MqttMsgUnsubscribed += gateway.client_MqttMsgUnsubscribed;
            //#endregion

            //#region Config
            //CustomLog.LogPath = HttpContext.Current.Server.MapPath("~/Logs/");
            //client = new MqttClient(IPAddress.Parse("127.0.0.1"));
            //clientID = "1111AAAA";
            //client.Connect(clientID);
            //CustomLog.LogError("connect thanh cong");
            //string[] topic = { "#", "Test/#" };

            //byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
            //client.Subscribe(topic, qosLevels);

            //client.MqttMsgPublishReceived += gateway.client_MqttMsgPublishReceived;

            //client.MqttMsgSubscribed += gateway.client_MqttMsgSubscribed;
            //client.MqttMsgUnsubscribed += gateway.client_MqttMsgUnsubscribed;
            //#endregion
        }
        //protected void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        //{
        //    List<string> topicList = e.Topic.Split(new char[] { '/' }).ToList();
        //    if (topicList.Count > 0)
        //    {
        //        if (topicList[0].ToString() == "info")
        //        {
        //            var deserializer = new JavaScriptSerializer();
        //            MessageConfig results = deserializer.Deserialize<MessageConfig>(Encoding.UTF8.GetString(e.Message));
        //            try
        //            {
        //                List<ObjParamSP> listParam = new List<ObjParamSP>();
        //                listParam.Add(new ObjParamSP() { Key = "CommandType", Value = results.CommandType });
        //                listParam.Add(new ObjParamSP() { Key = "CommandId", Value = results.CommandId });
        //                listParam.Add(new ObjParamSP() { Key = "CommandAction", Value = results.CommandAction });
        //                listParam.Add(new ObjParamSP() { Key = "ResultData", Value = deserializer.Serialize(results.Data) });
        //                listParam.Add(new ObjParamSP() { Key = "MessStatus", Value = results.Status });
        //                listParam.Add(new ObjParamSP() { Key = "Topic", Value = e.Topic });
        //                listParam.Add(new ObjParamSP() { Key = "CreateTime", Value = DateTime.Now });
        //                var sdsdsd = Utility.Helper.QueryStoredProcedure("LogMessages", listParam);
        //            }
        //            catch (Exception ex)
        //            {
        //                CustomLog.LogError(ex);
        //                throw;
        //            }



        //        }
        //    }

        //    string clientID = (string)Utility.Helper.GetPropertyValue(sender, "ClientId");
        //    bool isConnected = (bool)Utility.Helper.GetPropertyValue(sender, "IsConnected");
        //    string result = "MqttMsgPublishReceived----  ClientID: " + clientID + " --- Topic: " + e.Topic + " --- QosLevel: " + e.QosLevel + " --- Message: " + Encoding.UTF8.GetString(e.Message);
        //    CustomLog.LogError(result);
        //    //try
        //    //{
        //    //    List<ObjParamSP> listParam = new List<ObjParamSP>();
        //    //    listParam.Add(new ObjParamSP() { Key = "ClientID", Value = clientID });
        //    //    listParam.Add(new ObjParamSP() { Key = "ClientName", Value = e.Topic });
        //    //    listParam.Add(new ObjParamSP() { Key = "Description", Value = "sdsdssd" });
        //    //    listParam.Add(new ObjParamSP() { Key = "CategoryID", Value = 1 });
        //    //    listParam.Add(new ObjParamSP() { Key = "ClientStatus", Value = 1 });
        //    //    listParam.Add(new ObjParamSP() { Key = "UpdateTime", Value = DateTime.Now });
        //    //    var ab = Utility.Helper.QueryStoredProcedure("SetClient", listParam);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    CustomLog.LogError(ex);
        //    //    throw;
        //    //}
        //}

        //protected void client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        //{
        //    string clientID = (string)Utility.Helper.GetPropertyValue(sender, "ClientId");
        //    string result = "MqttMsgUnsubscribed------ ClientID: " + clientID + " --- MessageId: " + e.MessageId;
        //    CustomLog.LogError(result);
        //}

        //protected void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        //{
        //    string clientID = (string)Utility.Helper.GetPropertyValue(sender, "ClientId");
        //    string result = "MqttMsgSubscribed----- ClientID: " + clientID + " --- MessageId: " + e.MessageId + " --- GrantedQoSLevels: " + Encoding.UTF8.GetString(e.GrantedQoSLevels);
        //    CustomLog.LogError(result);
        //}
    }
}