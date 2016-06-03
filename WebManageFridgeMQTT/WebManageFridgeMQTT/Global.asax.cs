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
            //gateway.client = new MqttClient(IPAddress.Parse("45.117.80.39"));
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
        }
    }
}