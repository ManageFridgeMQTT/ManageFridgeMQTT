using System;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;

// 19.01.2010 P.Peev
namespace Log
{
    class LogXML
    {
        private const int LOG_MAX_FOLDER_COUNT = 32;

        public  string  m_strClassName  =   string.Empty;        
        private string  m_strPath       =   string.Empty;
        private string  m_strName       =   string.Empty;
        private string  m_strLogPath    =   string.Empty;
        private string  m_strXML        =   string.Empty;        

        private static Hashtable m_LogsTable = new Hashtable();

        // Constructor
        private LogXML( string address, string strFileName )
        {   
            try
            {
                Module mod = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0];

                if( strFileName == "" )
                    m_strName = Path.GetFileNameWithoutExtension( mod.FullyQualifiedName );
                else
                    m_strName = strFileName;

                m_strPath = Utils.UserDataPath.Get(m_strName);

                m_strLogPath = String.Format( "{0}\\{1}_Logs\\{2}\\", m_strPath, m_strName, address );

                if( !Directory.Exists( m_strLogPath ) )
                    Directory.CreateDirectory( m_strLogPath );
            }
            catch( Exception e )
            {
                m_strLogPath = string.Empty;
                Debug.WriteLine("The Log Directory Creation failed: {0}", e.ToString());
            }
        }

        // Access to named LogXML instances.      
        public static LogXML GetLogXML( string name, string file_name, string address )
        {
            string key = name+"\\"+address;
            // If it exists, return the existing log.
            if (m_LogsTable.ContainsKey(key)) 
                return (LogXML)m_LogsTable[key];

            // Create and return a new log.
            LogXML rv = new LogXML( address, file_name );
            m_LogsTable.Add( key, rv ); // add to table
            return rv;
        }

        // Clients can get update callbacks. The LogUpdateDelegate will be 
        // called whenever a new entry is added to a log.       
        public void Add( string xml, string enable )
        {
            if (enable != "True")
                return;

            lock (m_strXML) // lock resource 
            {
                m_strXML = xml;
                WriteLog();
            }
        }

        public void Add(string xml, bool enable)
        {
            Add(xml, enable.ToString());
        }

        // Write the log file. Delete existing first
        private void WriteLog()
        {
            m_strClassName = GetFileNameFromXML();
            DateTime dt = DateTime.Now;
            string strDateString = 
                string.Format("{0:d4}{1:d2}{2:d2}", dt.Year, dt.Month, dt.Day );
            string strTimeString = 
                string.Format("{0:d2}{1:d2}{2:d2}{3:d3}_", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
            string strSubFolderPath = m_strLogPath + strDateString + "\\";
            string FileName = strSubFolderPath + strTimeString + m_strClassName + ".XML";

            if (!Directory.Exists(strSubFolderPath))// creates subfolder if it is nesesery
            {
                CheckDirectoryCount();
                Directory.CreateDirectory(strSubFolderPath);
                Debug.WriteLine(string.Format("Directory created: {0}", strSubFolderPath ));
            }

            FileInfo fi = new FileInfo( FileName );
            lock( m_strXML )        // lock resource
            {
                using( StreamWriter sw = File.CreateText(FileName) )
                {
                    sw.Write(m_strXML);         // write entire contents
                    sw.Close();
                }
            }
        }

        private string GetFileNameFromXML()
        {
            string strFileName = string.Empty;
            XmlReader reader = null;
            try
            {
                using (reader = XmlReader.Create(new StringReader(m_strXML)))
                {
                    while (reader.Read() && strFileName == string.Empty)
                    {
                        switch (reader.NodeType)// get first note to determine serialized class within
                        {
                            case XmlNodeType.Element:
                                strFileName = reader.Name;
                                break;
                        }
                    }
                }
            }
            catch
            {
                strFileName = "Unknown";
                Debug.WriteLine( string.Format("Unknown class name found!") );
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return strFileName;
        }

        private void CheckDirectoryCount()
        {
            string strSearchString = "????????";
            DirectoryInfo dir = new DirectoryInfo( m_strLogPath );
            DirectoryInfo[] di = dir.GetDirectories( strSearchString );

            //deletes the redundant file
            if (di.Length >= LOG_MAX_FOLDER_COUNT)
            {
                do
                {
                    try
                    {
                        // Ensure that the target does not exist.
                        if( Directory.Exists( m_strLogPath+di[0].Name ) )
                        {
                            DirectoryInfo dr = new DirectoryInfo(m_strLogPath+di[0].Name);
                            dr.Delete(true);
                            Debug.WriteLine(string.Format("Directory deleted: {0}", m_strLogPath+di[0].Name));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("The process failed: {0}", e.ToString());
                    }

                    di = dir.GetDirectories(strSearchString);
                }
                while (di.Length >= LOG_MAX_FOLDER_COUNT) ;
            }
        }
    }
}
