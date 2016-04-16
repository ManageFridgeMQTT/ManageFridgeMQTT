using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace WebManageFridgeMQTT.Models
{
    public sealed class Global
    {
        public static MySqlConnection Context
        {
            get
            {
                string conn = ConfigurationManager.AppSettings["MySQLConnection"];
                MySqlConnection sql_conn = new MySqlConnection(conn);
                return sql_conn;
            }
        }
    }
}