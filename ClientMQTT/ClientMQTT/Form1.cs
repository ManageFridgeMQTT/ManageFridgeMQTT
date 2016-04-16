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

namespace ClientMQTT
{
    public partial class Form1 : Form
    {
        MqttClient client;
        string clientID;
        public Form1()
        {
            InitializeComponent();
            client = new MqttClient(IPAddress.Parse("127.0.0.1"));
            clientID = "44444444";
            client.Connect(clientID);
            textBox1.Text = "client_2 connect thanh cong";
            CustomLog.LogError("client_2 connect thanh cong");
            string[] topic = { "SSSS/sdsdsdsd", "SSSS/abc/tran" };

            byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE };
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
            client.Publish("test/" + clientID + "/abc", Encoding.UTF8.GetBytes("client2gui  test/abc Hello !! !!"));
            textBox1.Text = "client2gui test/abc Hello !!";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.Publish("test/" + clientID + "/ds/sds", Encoding.UTF8.GetBytes("client2gui test/ds/sds Hello !!"));
            textBox1.Text = "client2gui test/ds/sds Hello !!";
        }

    }
}
