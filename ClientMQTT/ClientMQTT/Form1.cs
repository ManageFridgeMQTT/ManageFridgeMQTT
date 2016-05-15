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
        string dcuSerial;
        string DPM;
        public Form1()
        {
            InitializeComponent();
            client = new MqttClient(IPAddress.Parse("45.117.80.39"));
            //client = new MqttClient("http://45.117.80.39:1883");
            dcuSerial = "2s22AAA";
            DPM = "DPM";
            client.Connect(dcuSerial);
            textBox1.Text = "connect thanh cong";
            CustomLog.LogError("onnect thanh cong");
            string[] topic = { "#", "test/#", "test" };

            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
            client.Subscribe(topic, qosLevels);

            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            client.MqttMsgSubscribed += client_MqttMsgSubscribed;
            client.MqttMsgUnsubscribed += client_MqttMsgUnsubscribed;
            
        }
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string result = "Nhan client_2: " + System.Text.Encoding.UTF8.GetString(e.Message);
            CustomLog.LogError(result);
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
            client.Publish("toado", Encoding.UTF8.GetBytes(textBox3.Text));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.Publish("nhietdo", Encoding.UTF8.GetBytes(textBox2.Text));
        }

        private void abc()
        {

        }

    }
}
