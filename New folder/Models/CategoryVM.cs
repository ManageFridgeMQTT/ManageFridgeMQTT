using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace eRoute.Models.ViewModel
{
    public class CategoryVM
    {
        public Category ProjectNewModel { get; set; }
        public List<sp_Get_Cate_For_Lang_AllResult> ProjectListModel { get; set; }
        public List<sp_Get_Cate_For_Lang_AllResult> Get_Cate_For_Lang { get; set; }
    }
}