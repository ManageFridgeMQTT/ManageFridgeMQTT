using System;
using System.IO;
using System.Reflection;
using sis.lib.ini;

namespace Settings
{
    //[IniAttribute(Section = "Settings")]
    public sealed class Ini : IniHelper
    {
        #region Instance
        public Ini() { }
        private static Ini _instance = null;
        public static Ini Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Ini();
                return _instance;
            }
        }
        #endregion Instance

        public static string fileName;
        static Ini()
        {
            Module module = Assembly.GetExecutingAssembly().GetModules()[0];
            fileName = string.Format(@"{0}\{1}.ini", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + Path.GetFileNameWithoutExtension(module.FullyQualifiedName).Replace(@"Service", @"Server"),
                Path.GetFileNameWithoutExtension(module.FullyQualifiedName).Replace(@"Service", @"Server"));
        }

        [IniAttribute(Section = "COMMON")]
        public Common _common = new Common();
        public Common common { get { return _common; } /*set { _common = value; }*/ }
        public class Common
        {
            [IniAttribute(Default = true)]
            public static bool WriteLog { get; set; }

            [IniAttribute(Default = false)]
            public static bool WriteXML { get; set; }
        }
        
        [IniAttribute(Section = "SQL")]
        public SQL _sql = new SQL();
        public SQL sql { get { return _sql; } /*set { _sql = value; }*/ }

        public class SQL
        {
            [IniAttribute(Default = "")]
            public static string Server { get; set; }

            [IniAttribute(Default = "")]
            public static string Database { get; set; }

            [IniAttribute(Default = "")]
            public static string UserName { get; set; }

            [IniAttribute(Default = "")]
            public static string UserPass { get; set; }

            [IniAttribute(Default = true)]
            public static bool WinAuth { get; set; } 
        }
    }
}