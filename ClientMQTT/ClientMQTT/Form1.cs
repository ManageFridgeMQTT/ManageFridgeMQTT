using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Web.Script.Serialization;

namespace ClientMQTT
{
    public partial class Form1 : Form
    {
        MqttClient client;
        Gateway gateway;
        string dcuSerial;
        string DPM;
        public Form1()
        {
            InitializeComponent();
            #region Config
            gateway = new Gateway();
            gateway.client = new MqttClient(IPAddress.Parse("45.117.80.39"));
            dcuSerial = "1111AAAA";
            gateway.client.Connect(dcuSerial);
            CustomLog.LogError("connect thanh cong");
            string[] topic = { "#", "test/#" };

            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
            gateway.client.Subscribe(topic, qosLevels);

            gateway.client.MqttMsgPublishReceived += gateway.client_MqttMsgPublishReceived;
            gateway.client.MqttMsgSubscribed += gateway.client_MqttMsgSubscribed;
            gateway.client.MqttMsgUnsubscribed += gateway.client_MqttMsgUnsubscribed;
            #endregion


            //client = new MqttClient(IPAddress.Parse("127.0.0.1"));
            //dcuSerial = "222AAA";
            //DPM = "DPM";
            //client.Connect(dcuSerial);
            //textBox1.Text = "connect thanh cong";
            //CustomLog.LogError("onnect thanh cong");
            //string[] topic = { "#", "req/#" };

            //byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
            //client.Subscribe(topic, qosLevels);

            //client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            //client.MqttMsgSubscribed += client_MqttMsgSubscribed;
            //client.MqttMsgUnsubscribed += client_MqttMsgUnsubscribed;
            
        }
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string result = "Nhan client_2: " + System.Text.Encoding.UTF8.GetString(e.Message);
            CustomLog.LogError(result);




            //byte[] abc = new byte[] { 0x01, 0x09, 0x31, 0x30, 0x2e, 0x38, 0x31, 0x37, 0x37, 0x33, 0x31 };
            //byte[] read = new byte[] { 0x01, 0x01, 0x01, 0x02, 0x04, 0x00, 0x00, 0x00, 0x01, 0x03, 0x01, 0x01, 0x24, 0x01, 0x03, 0x01, 0x01, 0x01 };
            //byte[] guilen = new byte[] { 0x02, 0x04, 0x00, 0x00, 0x00, 0x01, 0x05, 0x01, 0x01, 0x03, 0x04, 0x00, 0x00, 0x00, 0x01, 0x1F, 0x21, 0x18, 0x31, 0x30, 0x34, 0x39, 0x2E, 0x30, 0x36, 0x35, 0x31, 0x2C, 0x4E, 0x2C, 0x31, 0x30, 0x36, 0x33, 0x37, 0x2E, 0x32, 0x33, 0x30, 0x38, 0x2C, 0x45 };

            //string sdssdsdsdsds = Encoding.UTF8.GetString(read);
            //string sdsds = Encoding.UTF8.GetString(abc);
            //string sds = Encoding.UTF8.GetString(guilen);
        }

        void client_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        {
            string result = "client_MqttMsgUnsubscribed_ 2: " + e.MessageId.ToString();
            CustomLog.LogError(result);
        }

        void client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            string result = "client_MqttMsgSubscribed_ 2: " + e.MessageId.ToString();
            CustomLog.LogError(result);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string topic = "info/" + dcuSerial + "/" + DPM;
            MessageConfig result = new MessageConfig();
            result.CommandType = "2";
            result.CommandId = "1";
            result.CommandAction = "Get";
            result.Data = new Data()
            {
                Temperature = 35,
                Longitude = 52858588,
                Latitude = 58582525
            };
            result.Status = 1;

            var javaScriptSerializer = new JavaScriptSerializer();
            string message = javaScriptSerializer.Serialize(result);
            CustomLog.LogError(message);
            client.Publish(topic, Encoding.UTF8.GetBytes(message));
            textBox1.Text = message;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.Publish("test/" + dcuSerial + "/ds/sds", Encoding.UTF8.GetBytes("client2gui test/ds/sds Hello !!"));
            textBox1.Text = "client2gui test/ds/sds Hello !!";
        }

        private void abc()
        {

        }

    }
}
