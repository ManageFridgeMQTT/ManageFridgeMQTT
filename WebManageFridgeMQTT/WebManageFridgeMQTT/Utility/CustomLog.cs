using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace WebManageFridgeMQTT.Utility
{
    public static class CustomLog
    {
        public static string LogPath = String.Empty;

        private static object locker = new object();
        public static void LogArrayByte(byte[] mess)
        {
            lock (locker)
            {
                try
                {
                    string hex = BitConverter.ToString(mess);
                    string text = hex.Replace("-", " ");

                    StringBuilder builder = new StringBuilder();
                    builder
                        .AppendLine("----------")
                        .AppendLine(DateTime.Now.ToString())
                        .AppendFormat("Message:\t{0}", text)
                        .AppendLine();

                    string filePath = CustomLog.LogPath + "Log.txt";
                    using (StreamWriter writer = File.AppendText(filePath))
                    {
                        writer.Write(builder.ToString());
                        writer.Flush();
                    }
                }
                catch (Exception ex)
                {
                    CustomLog.LogError(ex);
                    throw;
                }
            }
        }
        public static void LogError(Exception ex)
        {
            lock (locker)
            {
                StringBuilder builder = new StringBuilder();
                builder
                    .AppendLine("----------")
                    .AppendLine(DateTime.Now.ToString())
                    .AppendFormat("Source:\t{0}", ex.Source)
                    .AppendLine()
                    .AppendFormat("Target:\t{0}", ex.TargetSite)
                    .AppendLine()
                    .AppendFormat("Type:\t{0}", ex.GetType().Name)
                    .AppendLine()
                    .AppendFormat("Message:\t{0}", ex.Message)
                    .AppendLine()
                    .AppendFormat("Stack:\t{0}", ex.StackTrace)
                    .AppendLine();

                string filePath = CustomLog.LogPath + "Log.txt";
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }

        }

        public static void LogError(string ex)
        {
            lock (locker)
            {
                StringBuilder builder = new StringBuilder();
                builder
                    .AppendLine("----------")
                    .AppendLine(DateTime.Now.ToString())
                    .AppendFormat("Message:\t{0}", ex)
                    .AppendLine();

                string filePath = CustomLog.LogPath + "Log.txt";
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }
        }

        public static void LogDevice(string ex)
        {
            lock (locker)
            {
                StringBuilder builder = new StringBuilder();
                builder
                    .AppendLine("----------")
                    .AppendLine(DateTime.Now.ToString())
                    .AppendFormat("Message:\t{0}", ex)
                    .AppendLine();

                string filePath = CustomLog.LogPath + "LogDevice.txt";
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }
        }

        public static void LogError(string filename, dynamic obj)
        {
            lock (locker)
            {
                StringBuilder builder = new StringBuilder();
                builder
                    .AppendLine("----------")
                    .AppendLine(DateTime.Now.ToString())
                    .AppendFormat("To email:\t{0}", obj.Email)
                    .AppendLine()
                    .AppendFormat("Username:\t{0}", obj.UserName)
                    .AppendLine()
                    .AppendFormat("Password:\t{0}", obj.Password)
                    .AppendLine()
                    .AppendFormat("ConfirmationToken:\t{0}", obj.ConfirmationToken)
                    .AppendLine();

                string filePath = CustomLog.LogPath + "Log.txt";
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }
        }

        public static void LogError(string filename, Exception ex, dynamic obj)
        {
            lock (locker)
            {
                // You could use any logging approach here

                StringBuilder builder = new StringBuilder();
                builder
                    .AppendLine("----------")
                    .AppendLine(DateTime.Now.ToString())
                    .AppendFormat("Source:\t{0}", ex.Source)
                    .AppendLine()
                    .AppendFormat("Target:\t{0}", ex.TargetSite)
                    .AppendLine()
                    .AppendFormat("Type:\t{0}", ex.GetType().Name)
                    .AppendLine()
                    .AppendFormat("Message:\t{0}", ex.Message)
                    .AppendLine()
                    .AppendFormat("Stack:\t{0}", ex.StackTrace)
                    .AppendLine()
                    .AppendFormat("To email:\t{0}", obj.Email)
                    .AppendLine()
                    .AppendFormat("UserName:\t{0}", obj.UserName)
                    .AppendLine()
                    .AppendFormat("Password:\t{0}", obj.Password)
                    .AppendLine()
                    .AppendFormat("ConfirmationToken:\t{0}", obj.ConfirmationToken)
                    .AppendLine();

                string filePath = CustomLog.LogPath + "Log.txt";
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }
        }

        public static void LogError(string filename, Exception ex)
        {
            // You could use any logging approach here
            lock (locker)
            {
                StringBuilder builder = new StringBuilder();
                builder
                    .AppendLine("----------")
                    .AppendLine(DateTime.Now.ToString())
                    .AppendFormat("Source:\t{0}", ex.Source)
                    .AppendLine()
                    .AppendFormat("Target:\t{0}", ex.TargetSite)
                    .AppendLine()
                    .AppendFormat("Type:\t{0}", ex.GetType().Name)
                    .AppendLine()
                    .AppendFormat("Message:\t{0}", ex.Message)
                    .AppendLine()
                    .AppendFormat("Stack:\t{0}", ex.StackTrace)
                    .AppendLine();

                string filePath = CustomLog.LogPath + "Log.txt";
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }
        }

        public static void LogError(string filename, string ex)
        {
            // You could use any logging approach here
            lock (locker)
            {
                StringBuilder builder = new StringBuilder();
                builder
                    .AppendLine("----------")
                    .AppendLine(DateTime.Now.ToString())
                    .AppendFormat("Message:\t{0}", ex)
                    .AppendLine();

                string filePath = CustomLog.LogPath + "Log.txt";
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.Write(builder.ToString());
                    writer.Flush();
                }
            }
        }
    }
}