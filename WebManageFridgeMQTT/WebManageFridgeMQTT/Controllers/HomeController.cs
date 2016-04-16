using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebManageFridgeMQTT.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            DataTable model = new DataTable();
            try
            {
                DataTable result = new DataTable();
                result = Utility.Helper.QueryStoredProcedure("GetAllClient");
                model = result;
            }
            catch (Exception)
            {
                
                throw;
            }
            return View(model);
        }

    }
}
