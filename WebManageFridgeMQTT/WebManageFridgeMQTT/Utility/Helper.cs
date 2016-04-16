using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using uPLibrary.Networking.M2Mqtt;

namespace WebManageFridgeMQTT.Utility
{
    public static class Helper
    {
        public static object GetPropertyValue(object ob, string propertyName)
        {
            return ob.GetType().GetProperties().Single(pi => pi.Name == propertyName).GetValue(ob, null);
        }
        public static DataTable QueryStoredProcedure(string name, List<ObjParamSP> listParam = null)
        {
            DataTable resulft = new DataTable();
            try
            {
                using (var conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["MySQLConnection"].ConnectionString))
                {
                    conn.Open();
                    DataSet ds = new DataSet();
                    using (var cmd = new MySqlCommand(name, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;
                        if (listParam != null)
                        {
                            foreach (ObjParamSP elm in listParam)
                            {
                                cmd.Parameters.AddWithValue(elm.Key, elm.Value);
                            }
                        }
                        var adapt = new MySqlDataAdapter(cmd);
                        adapt.Fill(ds);
                        if (ds != null && ds.Tables.Count >= 1)
                        {
                            resulft = ds.Tables[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
                throw;
            }
            return resulft;
        }
    }
    public class ObjParamSP
    {
        private string key;
        public object Value { get; set; }
        public string Key
        {
            get { return "@" + key; }
            set { key = value; }
        }
    }
}