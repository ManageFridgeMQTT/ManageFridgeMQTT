using System;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace eRoute
{
    [XmlRoot("Languages")]
    public class LanguagesModel
    {
        static LanguagesModel _current;
        static readonly object _currentLock = new object();

        List<LanguageModel> _languages = new List<LanguageModel>();

        [XmlElement(ElementName = "Language")]
        public List<LanguageModel> Languages
        {
            get { return _languages; }
        }

        public static LanguagesModel Current
        {
            get
            {
                lock (_currentLock)
                {
                    if (_current == null)
                    {
                        using (Stream stream = File.OpenRead(HttpContext.Current.Server.MapPath("~/App_Data/Languages.xml")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(LanguagesModel));
                            _current = (LanguagesModel)serializer.Deserialize(stream);
                        }
                    }
                    return _current;
                }
            }
        }
    }

    public class LanguageModel : LanguageModelBase
    {
        string _spriteCssClass;

        [XmlAttribute]
        public string SpriteCssClass
        {
            get
            {
                if (_spriteCssClass == null)
                    return "";
                return _spriteCssClass;
            }
            set { _spriteCssClass = value; }
        }
    }

    public class LanguageModelBase
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

    public enum CommonLanguages
    {
        EN,
        VI
    }
}