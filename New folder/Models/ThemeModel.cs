using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Web.UI;

namespace eRoute
{

    public class ThemeModel : ThemeModelBase {
        string _spriteCssClass;

        [XmlAttribute]
        public string SpriteCssClass {
            get {
                if(_spriteCssClass == null)
                    return "";
                return _spriteCssClass;
            }
            set { _spriteCssClass = value; }
        }
    }

}
