using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using CrawlData;
using System.Windows.Forms;
using System.Threading;
using System.Net;

namespace WindowsService
{
    public partial class Service1 : ServiceBase
    {
        public static System.Timers.Timer timer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ThreadStart();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
        }

        public void ThreadStart()
        {
            try
            {
                if (timer == null || timer.Enabled == false)
                {
                    CustomLog.LogError("EmailNotify", "OnStart");
                    timer = new System.Timers.Timer(Convert.ToInt32(ConfigurationSettings.AppSettings["TimerSecond"]));//300 giây
                    //timer = new System.Timers.Timer(timerTickMiliSecond);
                    timer.Elapsed += new ElapsedEventHandler(aTimerSFA_Elapsed);
                    timer.Enabled = true;
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
        }

        protected override void OnStop()
        {
            try
            {
                CustomLog.LogError("EmailNotify", "OnStop");
                timer.Dispose();
                timer.Close();
                //ThreadStart();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
        }

        [STAThread]
        public void aTimerSFA_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                ThreadStart threadDelegate = new ThreadStart(myThread);
                Thread thread = new Thread(threadDelegate);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                //let the form start with some sleep time or whatever you want

                //ActionsToExecuteInWebBrowser();
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
        }


        void myThread()
        {
            try
            {
                //CustomLog.LogError("EmailNotify", "aTimerSFA_Elapsed");
                string url = ConfigurationSettings.AppSettings["Website"];

                #region Call Web site
                ProcedureLoadPage(url.ToString());
                #endregion
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
        }

        void ActionsToExecuteInWebBrowser()
        {
            //Whatever you want to do in the WebBrowser here
        }

        static void ProcedureLoadPage(string urlToLoad)//, HtmlDocument htmlDoc
        {
            try
            {
                HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                HttpWebRequest request = HttpWebRequest.Create(urlToLoad) as HttpWebRequest;
                request.Method = "GET";

                #region Timeout
                request.Timeout = Timeout.Infinite;
                request.KeepAlive = true;
                #endregion

                /* Sart browser signature */
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.97 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,vi;q=0.6");
                /* Sart browser signature */

                //Console.WriteLine(request.RequestUri.AbsoluteUri);
                WebResponse response = request.GetResponse();
                htmlDoc.Load(response.GetResponseStream(), true);

                CustomLog.LogError("EmailNotify", "Done_" + urlToLoad.ToString());
                //return htmlDoc;
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
            }
            //return null;
        }
    }
}
