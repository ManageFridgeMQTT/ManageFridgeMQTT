using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
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
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLConnection"].ConnectionString))
                {
                    conn.Open();
                    DataSet ds = new DataSet();
                    using (var cmd = new SqlCommand(name, conn))
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
                        var adapt = new SqlDataAdapter(cmd);
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

        /// <summary>
        /// Converts a DataTable to a list with generic objects
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="table">DataTable</param>
        /// <returns>List with generic objects</returns>
        public static List<T> DataTableToList<T>(DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        public static ModelMess ParseMessToModel(List<byte> data)
        {
            ModelMess modelMess = new ModelMess();
            if (data != null)
            {
                List<byte> value = new List<byte>();
                if (data[0] == 0x01)
                {
                    modelMess.CommandType = data.Skip(0).Take(3).ToArray();
                    modelMess.CommandId = data.Skip(3).Take(6).ToArray();
                    modelMess.CommandAction = data.Skip(9).Take(3).ToArray();
                    value = data.Skip(12).ToList();
                }
                else if (data[0] == 0x02)
                {
                    modelMess.CommandId = data.Skip(0).Take(6).ToArray();
                    modelMess.CommandAction = data.Skip(6).Take(3).ToArray();
                    value = data.Skip(9).ToList();
                }
                else if (data[0] == 0x03)
                {
                    modelMess.CommandAction = data.Skip(0).Take(3).ToArray();
                    value = data.Skip(3).ToList();
                }

                if (value != null)
                {
                    if (value[0] == 0x05)
                    {
                        modelMess.Status = value.Skip(0).Take(3).ToArray();
                        if (value[3] == 0x24)
                        {
                            modelMess.Length = value.Skip(3).Take(3).ToArray();
                        }
                        if (value[6] == 0x06)
                        {
                            modelMess.States = value.Skip(6).Take(3).ToArray();
                        }

                    }
                    else if (value[0] == 0x24)
                    {
                        modelMess.Sequence = value.Skip(0).Take(3).ToArray();
                    }
                    if (value.Count > 9)
                    {
                        if (value[9] == 0x07)
                        {
                            modelMess.Time = value.Skip(9).Take(6).ToArray();
                        }
                    }
                    if (value.Count > 15)
                    {
                        if (value[15] == 0x08)
                        {
                            modelMess.GPSByte = value.Skip(15).Take(26).ToArray();
                        }
                    }
                }
            }

            return modelMess;
        }

        public static ThietBiStatusMess ParseMessToValue(ModelMess data)
        {
            ThietBiStatusMess model = new ThietBiStatusMess();
            /*
             Loại:
             1 - GPS định kỳ
             2 - Bảo Dưỡng
             3 - Di chuyển
             4 - Khoan
             5 - Cẩu
            */
            /*
             Trang Thai:
             1 - Nổ máy
             2 - Tắt máy
            */

            if (Array.Equals(data.CommandAction, ConstParam.DinhKyGPS))
            {

            }
            else if (Array.Equals(data.CommandAction, ConstParam.BaoDuong))
            {
                model.Loai = 2;
            }
            else if (Array.Equals(data.CommandAction, ConstParam.DiChuyen))
            {

            }
            else if (Array.Equals(data.CommandAction, ConstParam.Khoan))
            {

            }
            else if (Array.Equals(data.CommandAction, ConstParam.Cau))
            {

            }
            return model;
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

    class MessageConfig
    {
        public string CommandType { get; set; }
        public string CommandId { get; set; }
        public string CommandAction { get; set; }
        public Data Data { get; set; }
        public int Status { get; set; }
    }

    class Data
    {
        public int Temperature { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }

    public class ModelMess
    {
        public byte[] CommandType { get; set; }
        public byte[] CommandId { get; set; }
        public byte[] CommandAction { get; set; }
        public byte[] Status { get; set; }
        public byte[] States { get; set; }
        public byte[] Time { get; set; }
        public byte[] Sequence { get; set; }
        public byte[] GPSByte { get; set; }
        public byte[] Length { get; set; }

        public ModelMess()
        {
            this.CommandType = null;
            this.CommandId = null;
            this.CommandAction = null;
            this.Status = null;
            this.States = null;
            this.Time = null;
            this.Sequence = null;
            this.GPSByte = null;
            this.Length = null;
        }
    }

    public class ThietBiStatusMess
    {
        public DateTime ThoiGian { get; set; }
        public string ThietBiID { get; set; }
        public int Loai { get; set; }
        public int TrangThai { get; set; }
        public int Time { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
    public static class ConstParam
    {
        public static byte[] DinhKyGPS = new byte[] { 0x03, 0x01, 0x02 };
        public static byte[] BaoDuong = new byte[] { 0x03, 0x01, 0x0A };
        public static byte[] DiChuyen = new byte[] { 0x03, 0x01, 0x0B };
        public static byte[] Khoan = new byte[] { 0x03, 0x01, 0x0C };
        public static byte[] Cau = new byte[] { 0x03, 0x01, 0x0D };
        public static byte[] NoMay = new byte[] { 0x05, 0x01, 0x00 };
        public static byte[] TatMay = new byte[] { 0x05, 0x01, 0x01 };
    }
}