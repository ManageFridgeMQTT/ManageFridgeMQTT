using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrawlData
{
    public static class CustomLog
    {
        public static void LogError(Exception ex)
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
                .AppendLine();

            string filePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            filePath += "/Logs/Log.txt";

            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.Write(builder.ToString());
                writer.Flush();
            }
        }


        public static void LogError(string filename, Exception ex)
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
                .AppendLine();

            string filePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            filePath += "/Logs/" + filename + ".txt";

            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.Write(builder.ToString());
                writer.Flush();
            }
        }

        public static void LogError(string filename, string ex)
        {
            // You could use any logging approach here

            StringBuilder builder = new StringBuilder();
            builder
                //.AppendLine("----------")
                //.AppendLine(DateTime.Now.ToString())
                //.AppendFormat("Source:\t{0}", ex)
                .AppendLine(DateTime.Now.ToString() + ":\t" + ex);
            //.AppendLine();

            string filePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            filePath += "/Logs/" + filename + ".txt";

            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.Write(builder.ToString());
                writer.Flush();
            }
        }

        public static void LogError(string ex)
        {
            // You could use any logging approach here

            StringBuilder builder = new StringBuilder();
            builder
                //.AppendLine("----------")
                //.AppendLine(DateTime.Now.ToString())
                //.AppendFormat("Source:\t{0}", ex)
                .AppendLine(ex);
            //.AppendLine();

            string filePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            filePath += "/Logs/Log.txt";

            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.Write(builder.ToString());
                writer.Flush();
            }
        }
    }
}
