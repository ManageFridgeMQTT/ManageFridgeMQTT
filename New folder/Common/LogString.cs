using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Log
{
    class LogString
    {
        private const int LOG_MAX_FILE_SIZE = 2097152;
        private const int LOG_MAX_FILE_COUNT = 256;
        public const int LOG_MAX_LIST_ROWS = 256;

        private string  m_strPath       =   string.Empty;
        private string  m_strName       =   string.Empty;
        private string  m_strLogPath    =   string.Empty;
        private string  m_strLog        =   string.Empty;

        public delegate void LogUpdateDelegate( string message );
        public event LogUpdateDelegate OnLogUpdate;

        private static Hashtable m_LogsTable = new Hashtable();
        //
        // Generate the log file name
        //
        public string FileName { get { return String.Format("{0}{1}.log", m_strLogPath, m_strName); } }

        // Constructor
        public LogString( string strFileName )
        {   try
            {
                Module mod = Assembly.GetExecutingAssembly().GetModules()[0];
                
                if (strFileName == "")
                    m_strName = Path.GetFileNameWithoutExtension(mod.FullyQualifiedName);
                else
                    m_strName = strFileName;

                m_strPath = Utils.UserDataPath.Get(m_strName);

                m_strLogPath = String.Format("{0}\\{1}_Logs\\", m_strPath, m_strName);
                if (!Directory.Exists(m_strLogPath))
                    Directory.CreateDirectory( m_strLogPath );
            }
            catch ( Exception e )
            {
                m_strLogPath = string.Empty;
                Debug.WriteLine("The Log Directory Creation failed: {0}", e.ToString());
            }
        }

        // Check for log lenght and rename if needed
        //
        private void CheckFileList()
        {
            DateTime dt = DateTime.Now;
            String FileNameNew = String.Format("{0}{1}{2:_yyyyMMddHHmmss}.log", m_strLogPath, m_strName, dt);
            DirectoryInfo dir = new DirectoryInfo( m_strLogPath );
            FileInfo[] fi = dir.GetFiles( m_strName + "*.log", SearchOption.TopDirectoryOnly );

            try 
            {
                // Ensure that the target does not exist.
                if( File.Exists( FileNameNew ) )    
                    File.Delete( FileNameNew );

                // Move the file.
                File.Move( FileName, FileNameNew );
            } 
            catch( Exception e ) 
            {
                Debug.WriteLine("The process failed: {0}", e.ToString());
            }

            //deletes the redundant file
            if( fi.Length >= LOG_MAX_FILE_COUNT )
            {
                do
                {
                    try
                    {
                        // Ensure that the target does not exist.
                        if( File.Exists( m_strLogPath + fi[0].Name ) )
                        {
                            if( !fi[0].Name.Equals( FileName ) )
                                File.Delete( m_strLogPath + fi[0].Name );
                        }
                    }
                    catch ( Exception e )
                    {
                        Debug.WriteLine( "The process failed: {0}", e.ToString() );
                    }

                    fi = dir.GetFiles( m_strName + "*.log", SearchOption.TopDirectoryOnly );
                }
                while( fi.Length >= LOG_MAX_FILE_COUNT );
            }
        }

        // Write the log file. Delete existing first
        private void WriteLog()
        {
            FileInfo fi = new FileInfo( FileName );
            lock ( m_strLog )        // lock resource
            {
                if ( File.Exists( FileName ) )
                    if ( fi.Length > LOG_MAX_FILE_SIZE )
                        CheckFileList();

                WriteToLog();
            }
         }

        private void WriteToLog()
        {
            int _count = 5;// wait 500 msec...otherwise log is lost
            while(_count > 0)
            {
                try
                {
                    using( StreamWriter strWriter = new StreamWriter( FileName, true ) )
                    {
                        strWriter.Write( m_strLog + "\r\n" );
                        strWriter.AutoFlush = true;
                        strWriter.Flush();
                        strWriter.Close();
                        break;
                    }
                }
                catch( Exception )
                {
                    Thread.Sleep(100);
                    _count = _count-1;
                    Debug.WriteLine( String.Format("File {0} locked - can't write info.", FileName ) );
                }
            }
            if( _count <= 0 )
                Debug.WriteLine(String.Format("File {0} locked - info lost.", FileName));
        }

        // Access to named LogString instances.      
        public static LogString GetLogString( string name, string file_name )
        {
            // If it exists, return the existing log.
            if ( m_LogsTable.ContainsKey( name ) ) return ( LogString )m_LogsTable[name];

            // Create and return a new log.
            LogString rv = new LogString( file_name );
            m_LogsTable.Add( name, rv ); // add to table
            return rv;
        }

        // Get the logging string. This returns the current log string.      
        public string Log
        {
            get
            {
                lock ( m_strLog )  // lock the resource
                {
                    // lock the resource
                    return m_strLog;
                }
            }
        }

        public void LogCreate()
        {
            try
            {
                // Determine whether the directory exists.
                if ( Directory.Exists( m_strLogPath ) )
                {
                    Debug.WriteLine( m_strLogPath + " path exists already." );
                    return;
                }

                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory( m_strLogPath );
                Debug.WriteLine( string.Format("The directory {0} was created successfully at {1}.", m_strLogPath, Directory.GetCreationTime(m_strLogPath)));
            }
            catch( Exception e )
            {
                Debug.WriteLine( m_strLogPath + " - The process failed: {0}", e.ToString() );
            }
            finally { }
        }

        // Clients can get update callbacks. The LogUpdateDelegate will be 
        // called whenever a new entry is added to a log.       
        public void Add(string str, bool enable)
        {
            if ( !enable ) return;                
            Add(str);
        }

        public void Add(string str)
        {   
            string toadd = "";

            DateTime dt = DateTime.Now;
            toadd = string.Format("{0:d2}.{1:d2}.{2:d4}-{3:d2}:{4:d2}:{5:d2}.{6:d3}\t",
                 dt.Day, dt.Month, dt.Year, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
            // Add the string
            toadd += str;

            lock (m_strLog) // lock resource 
            {
                m_strLog = toadd;
                WriteLog();
            }
            if (OnLogUpdate != null) OnLogUpdate(toadd);
        }
    }
}
