using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eRoute.Models.ViewModel
{
    public class PhraseVM
    {
        public string LanguageID { get; set; }
        public string PhraseCode { get; set; }
        public string PhraseText { get; set; }
        public List<PhraseVM> lstPhraseVM { get; set; }
    }
}