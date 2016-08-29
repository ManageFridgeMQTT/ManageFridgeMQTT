// Developer Express Code Central Example:
// How to change a theme on-the-fly
// 
// This sample illustrates the possibility to change themes on-the-fly. The current
// theme is based on the Cookies value that is set in the client-side
// RadioButtonList.SelectedIndexChanged event handler. See also:
// ASP:
// http://www.devexpress.com/scid=E1342
// MVC: Applying Themes
// (ms-help://DevExpress.NETv11.2/DevExpress.AspNet/CustomDocument9143.htm)
// http://www.devexpress.com/scid=E2871
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E3825

using System;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace eRoute
{
    [XmlRoot("Themes")]
    public class ThemesModel
    {
        static ThemesModel _current;
        static readonly object _currentLock = new object();

        public static ThemesModel Current
        {
            get
            {
                lock (_currentLock)
                {
                    if (_current == null)
                    {
                        using (Stream stream = File.OpenRead(HttpContext.Current.Server.MapPath("~/App_Data/Themes.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(ThemesModel));
                            _current = (ThemesModel)serializer.Deserialize(stream);
                        }
                    }
                    return _current;
                }
            }
        }

        List<ThemeGroupModel> _groups = new List<ThemeGroupModel>();

        [XmlElement("ThemeGroup")]
        public List<ThemeGroupModel> Groups
        {
            get { return _groups; }
        }
    }


    public enum CommonThemes
    {
        iOS,
        Aqua,
        DevEx,
        BlackGlass,
        Glass,
        Office2003Blue,
        Office2003Olive,
        Office2003Silver,
        Office2010Black,
        Office2010Blue,
        Office2010Silver,
        PlasticBlue,
        RedWine,
        SoftOrange,
        Youthful
    }

    public class ThemeModelBase
    {
        string _name;
        string _title;

        [XmlAttribute]
        public string Name
        {
            get
            {
                if (_name == null)
                    return "";
                return _name;
            }
            set { _name = value; }
        }
        [XmlAttribute]
        public string Title
        {
            get
            {
                if (_title == null)
                    return "";
                return _title;
            }
            set { _title = value; }
        }
    }
}