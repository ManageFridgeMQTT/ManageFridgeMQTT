using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Web.UI;

namespace eRoute
{

    public class ThemeGroupModel : ThemeModelBase {
        List<ThemeModel> _themes = new List<ThemeModel>();

        [XmlElement(ElementName = "Theme")]
        public List<ThemeModel> Themes {
            get { return _themes; }
        }
    }

}
