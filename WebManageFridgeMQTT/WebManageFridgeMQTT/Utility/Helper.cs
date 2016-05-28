using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using uPLibrary.Networking.M2Mqtt;
using WebManageFridgeMQTT.Models;

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
                        if (value.Count >= 6)
                        {
                            if (value[3] == 0x24)
                            {
                                modelMess.Length = value.Skip(3).Take(3).ToArray();
                            }
                            
                        }
                        if (value.Count >= 9 )
                        {
                            if (value[6] == 0x06)
                            {
                                modelMess.States = value.Skip(6).Take(3).ToArray();
                            }
                        }
                        if (value.Count >= 15)
                        {
                            if (value[9] == 0x07)
                            {
                                modelMess.Time = value.Skip(11).Take(4).ToArray();
                            }
                        }
                        if (value.Count >= 40)
                        {
                            if (value[15] == 0x08)
                            {
                                modelMess.GPSByte = value.Skip(17).Take(26).ToArray();
                                modelMess.LatitudeByte = modelMess.GPSByte.Skip(0).Take(9).ToArray();
                                modelMess.LongitudeByte = modelMess.GPSByte.Skip(12).Take(10).ToArray();
                            }
                        }
                        if (value.Count >= 30)
                        {
                            if (value[3] == 0x1F)
                            {
                                if (Helper.ByteArrayCompare(value.Skip(3).Take(3).ToArray(), ConstParam.Value))
                                {
                                    modelMess.GPSByte = value.Skip(6).Take(24).ToArray();
                                    modelMess.LatitudeByte = modelMess.GPSByte.Skip(0).Take(9).ToArray();
                                    modelMess.LongitudeByte = modelMess.GPSByte.Skip(12).Take(10).ToArray();
                                }
                            }
                        }
                    }
                    else if (value[0] == 0x24)
                    {
                        modelMess.Sequence = value.Skip(0).Take(3).ToArray();
                    }
                }
            }

            return modelMess;
        }

        public static ThietBiStatusMess ParseMessToValue(ModelMess data, string thietBiId)
        {
            ThietBiStatusMess model = new ThietBiStatusMess();
            try
            {
                model.ThoiGian = DateTime.Now;
                model.ThietBiID = thietBiId;
                if (data.CommandType != null)
                {
                    string hex = BitConverter.ToString(data.CommandType);
                    model.CommandType = hex.Replace("-", " ");
                }
                if (data.CommandId != null)
                {
                    string hex = BitConverter.ToString(data.CommandId);
                    model.CommandId = hex.Replace("-", " ");
                }
                if (data.CommandAction != null)
                {
                    string hex = BitConverter.ToString(data.CommandAction);
                    model.CommandAction = hex.Replace("-", " ");
                }
                if (data.Time != null)
                {
                    model.Time = BitConverter.ToInt32(data.Time, 0);
                }
                if (data.LatitudeByte != null)
                {
                    model.Latitude = Helper.ConvertLatitude(data.LatitudeByte);
                }
                if (data.LongitudeByte != null)
                {
                    model.Longitude = Helper.ConvertLongitude(data.LongitudeByte);
                }


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
                 0 - Nổ máy
                 1 - Tắt máy
                */
                if (data.Status.Count() > 0)
                {
                    if (Helper.ByteArrayCompare(data.Status, ConstParam.NoMay))
                    {
                        model.StatusMay = 0;
                    }
                    else if (Helper.ByteArrayCompare(data.Status, ConstParam.TatMay))
                    {
                        model.StatusMay = 1;
                    }
                }


                if (Helper.ByteArrayCompare(data.CommandAction, ConstParam.DinhKyGPS))
                {
                    model.Loai = 1;
                }
                else if (Helper.ByteArrayCompare(data.CommandAction, ConstParam.BaoDuong))
                {
                    model.Loai = 2;
                    if (Helper.ByteArrayCompare(data.States, ConstParam.Dang))
                    {
                        model.TrangThai = 3;  //đang bảo dưỡng
                    }
                    else if (Helper.ByteArrayCompare(data.States, ConstParam.Khong))
                    {
                        model.TrangThai = 4;
                    }
                }
                else if (Helper.ByteArrayCompare(data.CommandAction, ConstParam.DiChuyen))
                {
                    model.Loai = 3;
                    if (Helper.ByteArrayCompare(data.States, ConstParam.Dang))
                    {
                        model.TrangThai = 5;  //đang Di chuyển
                    }
                    else if (Helper.ByteArrayCompare(data.States, ConstParam.Khong))
                    {
                        model.TrangThai = 6;
                    }
                }
                else if (Helper.ByteArrayCompare(data.CommandAction, ConstParam.Khoan))
                {
                    model.Loai = 4;
                    if (Helper.ByteArrayCompare(data.States, ConstParam.Dang))
                    {
                        model.TrangThai = 7;  //đang bảo dưỡng
                    }
                    else if (Helper.ByteArrayCompare(data.States, ConstParam.Khong))
                    {
                        model.TrangThai = 8;
                    }
                }
                else if (Helper.ByteArrayCompare(data.CommandAction, ConstParam.Cau))
                {
                    model.Loai = 5;
                    if (Helper.ByteArrayCompare(data.States, ConstParam.Dang))
                    {
                        model.TrangThai = 9;  //đang bảo dưỡng
                    }
                    else if (Helper.ByteArrayCompare(data.States, ConstParam.Khong))
                    {
                        model.TrangThai = 10;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                throw;
            }
            return model;
        }
        public static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            try
            {
                if (a1.Length == a2.Length)
                {
                    for (int i = 0; i < a1.Length; i++)
                        if (a1[i] != a2[i])
                            return false;
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                return false;
                throw;
            }
        }

        public static decimal ConvertLatitude(byte[] arrayData)
        {
            decimal result = 0;
            try
            {
                if(arrayData[3] != 0x00)
                {
                    string strData = System.Text.Encoding.UTF8.GetString(arrayData);
                    string strToaDo = strData.Substring(0, 2);
                    string strPhut = strData.Substring(2, 7);
                    strPhut = strPhut.Replace('.', ',');
                    decimal toado = Decimal.Parse(strToaDo);
                    decimal phut = Decimal.Parse(strPhut);
                    result = toado + (phut / 60);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                return 0;
                throw;
            }
            return result;
        }
        public static decimal ConvertLongitude(byte[] arrayData)
        {
            decimal result = 0;
            try
            {
                if (arrayData[4] != 0x00)
                {
                    string strData = System.Text.Encoding.UTF8.GetString(arrayData);
                    string strToaDo = strData.Substring(0, 3);
                    string strPhut = strData.Substring(3, 7);
                    strPhut = strPhut.Replace('.', ',');
                    decimal toado = Decimal.Parse(strToaDo);
                    decimal phut = Decimal.Parse(strPhut);
                    result = toado + (phut / 60);
                }
            }
            catch (Exception ex)
            {
                CustomLog.LogError(ex);
                return 0;
                throw;
            }
            return result;
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
        public byte[] LatitudeByte { get; set; }
        public byte[] LongitudeByte { get; set; }
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
        public string WriteByteLog()
        {
            string result = "";
            if (this.CommandType != null)
            {
                string hex = BitConverter.ToString(this.CommandType);
                string text = hex.Replace("-", " ");
                result += "---  CommandType:" + text;
            }
            if (this.CommandId != null)
            {
                string hex = BitConverter.ToString(this.CommandId);
                string text = hex.Replace("-", " ");
                result += "---  CommandId:" + text;
            }
            if (this.CommandAction != null)
            {
                string hex = BitConverter.ToString(this.CommandAction);
                string text = hex.Replace("-", " ");
                result += "---  CommandAction:" + text;
            }
            if (this.Status != null)
            {
                string hex = BitConverter.ToString(this.Status);
                string text = hex.Replace("-", " ");
                result += "---  Status:" + text;
            }
            if (this.States != null)
            {
                string hex = BitConverter.ToString(this.States);
                string text = hex.Replace("-", " ");
                result += "---  States:" + text;
            }
            if (this.Time != null)
            {
                string hex = BitConverter.ToString(this.Time);
                string text = hex.Replace("-", " ");
                result += "---  Time:" + text;
            }
            if (this.Sequence != null)
            {
                string hex = BitConverter.ToString(this.Sequence);
                string text = hex.Replace("-", " ");
                result += "---  Sequence:" + text;
            };
            if (this.GPSByte != null)
            {
                string hex = BitConverter.ToString(this.GPSByte);
                string text = hex.Replace("-", " ");
                result += "---  GPSByte:" + text;
            }
            if (this.Length != null)
            {
                string hex = BitConverter.ToString(this.Length);
                string text = hex.Replace("-", " ");
                result += "---  Length:" + text;
            }
            return result;
        }
    }

    public class ThietBiStatusMess
    {
        public DateTime ThoiGian { get; set; }

        public string CommandType { get; set; }
        public string CommandId { get; set; }
        public string CommandAction { get; set; }
        public string ThietBiID { get; set; }
        public int? Loai { get; set; }
        public int? StatusMay { get; set; }
        public int? TrangThai { get; set; }
        public int? Time { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }


        public string WriteLog()
        {
            string result = "";
            if (this.CommandType != null)
            {
                result += "CommandType:" + string.Join(" ", this.CommandType);
            }
            if (this.CommandId != null)
            {
                result += "CommandId:" + string.Join(" ", this.CommandId);
            }
            if (this.CommandAction != null)
            {
                result += "CommandAction:" + string.Join(" ", this.CommandAction);
            }
            if (this.ThietBiID != null)
            {
                result += "ThietBiID:" + this.ThietBiID;
            }
            if (this.Loai != null)
            {
                result += "Loai:" + this.Loai.Value.ToString();
            }
            if (this.StatusMay != null)
            {
                result += "StatusMay:" + this.StatusMay.Value.ToString();
            }
            if (this.TrangThai != null)
            {
                result += "TrangThai:" + this.TrangThai.Value.ToString();
            }
            if (this.Time != null)
            {
                result += "Time:" + this.Time.Value.ToString();
            }
            if (this.Latitude != null)
            {
                result += "Latitude:" + this.Latitude.Value.ToString();
            }
            if (this.Longitude != null)
            {
                result += "Longitude:" + this.Longitude.Value.ToString();
            }
            return result;
        }
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

        public static byte[] Dang = new byte[] { 0x06, 0x01, 0x00 };
        public static byte[] Khong = new byte[] { 0x06, 0x01, 0x01 };

        public static byte[] DangBaoDuong = new byte[] { 0x06, 0x01, 0x00 };
        public static byte[] KoBaoDuong = new byte[] { 0x06, 0x01, 0x01 };
        public static byte[] DangDiChuyen = new byte[] { 0x07, 0x01, 0x00 };
        public static byte[] KoDiChuyen = new byte[] { 0x07, 0x01, 0x01 };
        public static byte[] DangKhoan = new byte[] { 0x08, 0x01, 0x00 };
        public static byte[] KoKhoan = new byte[] { 0x08, 0x01, 0x01 };
        public static byte[] DangCau = new byte[] { 0x09, 0x01, 0x00 };
        public static byte[] KoCau = new byte[] { 0x09, 0x01, 0x01 };

        public static byte[] Value = new byte[] { 0x1F, 0x21, 0x18 };

    }

    public class DeviceActivity 
    {
        public string ThietBiID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<GetInfoDeviceActivityResult> ListData { get; set; }

        public List<GetInfoDeviceMoveResult> ListDataMove { get; set; }

        public List<GetInfoDeviceModifyResult> ListDataModify { get; set; }

        public List<GetInfoDeviceReportResult> ListDataReport { get; set; }
        

        public DeviceActivity()
        {
            this.ThietBiID = null;
            this.FromDate = DateTime.Now.AddMonths(-1);
            this.ToDate = DateTime.Now;
            this.ListData = new List<GetInfoDeviceActivityResult>();
            this.ListDataMove = new List<GetInfoDeviceMoveResult>();
            this.ListDataModify = new List<GetInfoDeviceModifyResult>();
            this.ListDataReport = new List<GetInfoDeviceReportResult>();
        }
    }
}