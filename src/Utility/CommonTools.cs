using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace Utility
{
    public static class CommonTools
    {
        static private Random rnd = new Random();

#pragma warning disable IDE0018 // 内联变量声明

        /// <summary>
        /// 将一个字符串转换为数字形式，转换失败返回默认值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static int GetNumber<T>(T str, int defaultValue)
        {
            if (str == null)
                return defaultValue;
            int i = 0;
            if (!int.TryParse(str.ToString(), out i))
                i = defaultValue;

            return i;
        }

        public static uint GetNumber<T>(T str, uint defaultValue)
        {
            if (str == null)
                return defaultValue;
            uint i = 0;
            if (!uint.TryParse(str.ToString(), out i))
                i = defaultValue;

            return i;
        }

        /// <summary>
        /// 将一个字符串转换为数字形式，转换失败返回默认值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static double GetNumber<T>(T str, double defaultValue)
        {
            if (str == null)
                return defaultValue;
            double d = 0;

            if (!double.TryParse(str.ToString(), out d))
                d = defaultValue;
            return d;
        }

        /// <summary>
        /// 将一个字符串转换为数字形式，转换失败返回默认值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static float GetNumber<T>(T str, float defaultValue)
        {
            if (str == null)
                return defaultValue;
            float d = 0;
            if (!float.TryParse(str.ToString(), out d))
                d = defaultValue;
            return d;
        }/// 将一个字符串转换为数字形式，转换失败返回默认值

        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static decimal GetNumber<T>(T str, decimal defaultValue)
        {
            if (str == null)
                return defaultValue;
            decimal d = 0;
            if (!decimal.TryParse(str.ToString(), out d))
                d = defaultValue;
            return d;
        }

        /// 将一个字符串转换为长整形式，转换失败返回默认值
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static long GetNumber<T>(T str, long defaultValue)
        {
            if (str == null)
                return defaultValue;
            long d = 0;
            if (!long.TryParse(str.ToString(), out d))
                d = defaultValue;
            return d;
        }

        /// <summary>
        /// 将字符串转换为时间，如果转换失败，返回默认值
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static DateTime GetDate(string s, DateTime defaultValue)
        {
            DateTime dt;
            if (s == null)
                return defaultValue;

            if (!DateTime.TryParse(s, out dt))
            {
                return defaultValue;
            }
            else
            {
                return dt;
            }
        }

#pragma warning restore IDE0018 // 内联变量声明

        /// <summary>
        /// 判断字符串是否是数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric<T>(T str)
        {
            if (str == null)
                return false;
            if (IsBlank(str.ToString(), true))
                return false;

            if (Regex.IsMatch(str.ToString(), @"^-?\d+$"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 判断字符串是否为空，
        /// </summary>
        /// <param name="s">要验证的字符串</param>
        /// <param name="bTrim">是否截取两端空格</param>
        /// <returns></returns>
        public static bool IsBlank(string s, bool bTrim)
        {
            if (s == null)
                return true;
            if (s.Trim().Length == 0)
                return true;
            return false;
        }

        /// <summary>
        /// 判断字符串是否为空，
        /// </summary>
        /// <param name="s">要验证的字符串</param>
        /// <returns></returns>
        public static bool IsBlank(string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /*
		public static bool IsValidEmail(String strValid)
		{
			//Regex rCode = new Regex(@"^\w+((-\w+)|(\.\w+))*\@\w+((\.|-)\w+)*\.\w+$ ");
			Regex rCode = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)* ");
			return rCode.IsMatch(strValid);
		}
		 */

        /// <summary>
        /// 判断DataTable是否有记录
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        [Obsolete]
        public static bool HasRecord(DataTable dt)
        {
            if (dt == null)
                return false;
            if (dt.Rows.Count == 0)
                return false;

            return true;
        }


        /// <summary>
        /// 判断ds是否有数据行
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        [Obsolete]
        public static bool HasRecord(DataSet ds)
        {
            if (ds == null)
                return false;
            if (ds.Tables.Count == 0)
                return false;
            return HasRecord(ds.Tables[0]);
        }

        public static bool HasData(DataTable dt)
        {
            return dt != null && dt.Rows.Count > 0;
        }
        public static bool HasData(DataSet ds)
        {
            if (ds == null)
                return false;
            if (ds.Tables.Count == 0)
                return false;
            return HasData(ds.Tables[0]);
        }

        /// <summary>
        /// 是否是合法的日期
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static bool IsValidDate(int year, int month, int day)
        {
            if (month <= 0 || month > 12)
                return false;

            if (day > 31 || day <= 0)
                return false;

            if (month == 2)
            {
                //判断2月有多少天(28or29)
                int nDaysOfFebr = 28;
                if (year % 4 == 0 && year % 100 != 0)
                {
                    nDaysOfFebr = 29;
                }
                if (year % 100 == 0 && year % 400 == 0)
                {
                    nDaysOfFebr = 29;
                }
                if (day > nDaysOfFebr)
                    return false;
            }
            //	4 6 9 11月有30天
            if (month == 4 || month == 6 || month == 9 || month == 11)
            {
                if (month == 31)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// html转文本
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ConvertHtml2Text(string source)
        {
            if (source == null)
                return null;

            string result = source.Replace("\r", " ");
            result = source.Replace("\r\n", "");
            //remove line breaks,tabs
            result = result.Replace("\n", " ");
            result = result.Replace("\t", " ");

            //remove the header
            result = Regex.Replace(result, "(<head>).*(</head>)", string.Empty, RegexOptions.IgnoreCase);

            result = Regex.Replace(result, @"<(\s*)script(\s*)([^>]*)>", "<script>", RegexOptions.IgnoreCase);
            //	result = Regex.Replace(result, @"(<script>).*(</script>)", string.Empty, RegexOptions.IgnoreCase);

            //remove all styles
            result = Regex.Replace(result, @"<( )*style([^>])*>", "<style>", RegexOptions.IgnoreCase); //clearing attributes
            result = Regex.Replace(result, "(<style>).*(</style>)", string.Empty, RegexOptions.IgnoreCase);

            //insert tabs in spaces of <td> tags
            result = Regex.Replace(result, @"<( )*td([^>])*>", " ", RegexOptions.IgnoreCase);

            //insert line breaks in places of <br> and <li> tags
            result = Regex.Replace(result, @"<( )*br( )*>", " ", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"<( )*li( )*>", " ", RegexOptions.IgnoreCase);

            //insert line paragraphs in places of <tr> and <p> tags
            result = Regex.Replace(result, @"<( )*tr([^>])*>", " ", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"<( )*p([^>])*>", " ", RegexOptions.IgnoreCase);

            //remove anything thats enclosed inside < >
            result = Regex.Replace(result, @"<[^>]*>", "\r", RegexOptions.IgnoreCase);

            //replace special characters:
            result = Regex.Replace(result, @"&amp;", "&", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"&nbsp;", " ", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"&lt;", "<", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"&gt;", ">", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"&(.{2,6});", string.Empty, RegexOptions.IgnoreCase);

            //remove extra line breaks and tabs
            result = Regex.Replace(result, @" ( )+", " ");
            //result = Regex.Replace(result, "(\r)( )+(\r)", "\r\r");
            //result = Regex.Replace(result, @"(\r\r)+", "\r\n");
            result = Regex.Replace(result, "(\r)( )+(\r)", "\r");
            result = Regex.Replace(result, @"(\r)+", "\r\n");

            return result;
        }

        public static string GetRandomName(string prefix, string append)
        {
            int i = rnd.Next(1000, 9999);

            return string.Format("{0}{1:X}{2}{3}", prefix, (ulong)DateTime.Now.ToBinary(), i, append);
        }

        public static string MD5(string str)
        {
            using (MD5 m = new MD5CryptoServiceProvider())
            {
                byte[] s = m.ComputeHash(System.Text.Encoding.Default.GetBytes(str));
                string retstr = BitConverter.ToString(s);
                retstr = retstr.Replace("-", "");
                return retstr;
            }
        }

        public static string MD5(byte[] buffer)
        {
            using (MD5 m = new MD5CryptoServiceProvider())
            {
                byte[] s = m.ComputeHash(buffer);
                string retstr = BitConverter.ToString(s);
                retstr = retstr.Replace("-", "");
                return retstr;
            }
        }

        public static string MD5(string str, System.Text.Encoding coding)
        {
            using (MD5 m = new MD5CryptoServiceProvider())
            {
                byte[] s = m.ComputeHash(coding.GetBytes(str));
                string retstr = BitConverter.ToString(s);
                retstr = retstr.Replace("-", "");
                return retstr;
            }
        }

        public static string MD5(string str, string salt)
        {
            using (MD5 m = new MD5CryptoServiceProvider())
            {
                byte[] s = m.ComputeHash(System.Text.Encoding.Default.GetBytes(str + salt));
                string retstr = BitConverter.ToString(s);
                retstr = retstr.Replace("-", "");
                return retstr;
            }
        }

        public static string MD5(string str, string salt, System.Text.Encoding coding)
        {
            using (MD5 m = new MD5CryptoServiceProvider())
            {
                byte[] s = m.ComputeHash(coding.GetBytes(str + salt));
                string retstr = BitConverter.ToString(s);
                retstr = retstr.Replace("-", "");
                return retstr;
            }
        }
        public static string SHA256(string content, string salt)
        {
            var coding = System.Text.Encoding.UTF8;
            using (var m = new SHA256CryptoServiceProvider())
            {
                byte[] s = m.ComputeHash(coding.GetBytes(content + salt));
                string retstr = BitConverter.ToString(s);
                retstr = retstr.Replace("-", "").ToLower();
                return retstr;
            }
        }
        public static string SHA1(string content, string salt)
        {
            var coding = System.Text.Encoding.UTF8;
            using (var m = new SHA1CryptoServiceProvider())
            {
                byte[] s = m.ComputeHash(coding.GetBytes(content + salt));
                string retstr = BitConverter.ToString(s);
                retstr = retstr.Replace("-", "").ToLower();
                return retstr;
            }
        }

        /// <summary>
        /// 计算文件的MD5校验
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                string retstr = BitConverter.ToString(retVal);
                retstr = retstr.Replace("-", "").ToLower();
                return retstr;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool ExistsASCIIChars(string s)
        {
            int slen = s.Length;
            for (int i = 0; i < slen; i++)
            {
                int char32 = char.ConvertToUtf32(s, i);
                if (char32 >= 0 && char32 < 128)
                    return true;
            }
            return false;
        }

        public static bool ExistsCNChars(string s)
        {
            /* cn chars range*/
            int chfrom = Convert.ToInt32("4e00", 16);    //范围（0x4e00～0x9fff）转换成int（chfrom～chend）
            int chend = Convert.ToInt32("9fff", 16);

            int slen = s.Length;
            for (int i = 0; i < slen; i++)
            {
                int char32 = char.ConvertToUtf32(s, i);
                if (char32 >= chfrom || char32 < chend)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 将以秒为单位的时长转换为HH:mm:ss格式表示方法
        /// </summary>
        /// <param name="timelong"></param>
        /// <returns></returns>
        public static string FormatTimeLong(double timelong)
        {
            int hours = (int)timelong / 3600;
            int leftsec = (int)timelong % 3600;
            int minutes = (int)leftsec / 60;
            leftsec = leftsec % 60;
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, leftsec);
        }

        /// <summary>
        /// timestamp毫秒
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime FormatDate(double timestamp)
        {
            DateTime dt = DateTime.Parse("1970-1-1 0:00:00");
            dt = dt.AddMilliseconds(timestamp);
            dt = DateTime.FromFileTime(dt.ToFileTimeUtc());
            return dt;
        }

        /// <summary>
        /// timestamp秒
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime FormatDate(int timestamp)
        {
            DateTime dt = DateTime.Parse("1970-1-1 0:00:00");
            dt = dt.AddSeconds(timestamp);
            dt = DateTime.FromFileTime(dt.ToFileTimeUtc());
            return dt;
        }

        public static double FormatDate(DateTime date)
        {
            DateTime dt = DateTime.Parse("1970-1-1 0:00:00");
            return (date.ToUniversalTime() - dt).TotalSeconds;
        }
        /// <summary>
        /// 生成时间戳 
        /// </summary>
        /// <returns>当前时间减去 1970-01-01 00.00.00 得到的毫秒数</returns>
        public static string GetTimeStamp(DateTime nowTime)
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long unixTime = (long)Math.Round((nowTime.ToUniversalTime() - startTime).TotalMilliseconds);
            return unixTime.ToString();
        }

        /*
		public static bool ExportDataSet2EXCEL(DataSet ds, string file)
		{
			Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
			excel.Visible = false;
			Microsoft.Office.Interop.Excel.Workbook wb = excel.Workbooks.Add(Missing.Value);

			Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;

			System.Data.DataTable dt = ds.Tables[0];

			// 写表头
			foreach (DataColumn dc in dt.Columns)
			{
				worksheet.Cells[1, dc.Ordinal + 1] = dc.ColumnName;
				Microsoft.Office.Interop.Excel.Range range = (Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, dc.Ordinal + 1];
				//Microsoft.Office.Interop.Excel.Range range = (Microsoft.Office.Interop.Excel.Range)worksheet.Rows[1, dc.Ordinal + 1];
				range.Font.Bold = true;
				range.Font.Size = 12;
			}

			int rowIndex = 2; //从第二行开始，第一行为表头
			foreach (DataRow dr in dt.Rows)
			{
				foreach (DataColumn dc in dt.Columns)
				{
					worksheet.Cells[rowIndex, dc.Ordinal + 1] = dr[dc.Ordinal].ToString();
				}
				rowIndex++;
			}

			wb.Saved = true;
			wb.SaveCopyAs(file);
			//worksheet.SaveAs(file, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
			excel.Quit();
			System.GC.Collect();
			return true;
		}
		*/

        public static bool ExportDataSet2CSV(DataSet ds, string file)
        {
            if (!CommonTools.HasData(ds))
            {
                return false;
            }
            char delimiter = '\t';
            System.Data.DataTable dt = ds.Tables[0];
            StringBuilder excel = new StringBuilder();
            // 写表头
            foreach (DataColumn dc in dt.Columns)
            {
                if (excel.Length == 0)
                    excel.Append(dc.ColumnName);
                else
                {
                    excel.Append(delimiter);
                    excel.Append(dc.ColumnName);
                }
            }
            excel.Append("\r\n");

            foreach (DataRow dr in dt.Rows)
            {
                bool bfirstCol = true;
                foreach (DataColumn dc in dt.Columns)
                {
                    Type coltype = dc.DataType;
                    string quote = "";
                    if (coltype == typeof(string) || coltype == typeof(DateTime))
                    {
                        quote = "\"";
                    }
                    if (!bfirstCol)
                    {
                        excel.Append(delimiter);
                    }
                    excel.Append(quote);
                    excel.Append(dr[dc.Ordinal].ToString());
                    excel.Append(quote);

                    bfirstCol = false;
                }
                excel.Append("\r\n");
            }

            using (StreamWriter sw = new StreamWriter(file, false, System.Text.Encoding.GetEncoding("GBK")))
            {
                sw.Write(excel.ToString());
            }

            return true;
        }

        public static bool ExportDataSet2CSV(string text, string file)
        {
            using (StreamWriter sw = new StreamWriter(file, false, System.Text.Encoding.GetEncoding("GBK")))
            {
                sw.Write(text);
            }
            return true;
        }


        private static string GetClientIP(this HttpContext context)
        {
            if (context == null)
                return null;
            string strIPAddr = "";
            string xfr = context.Request.Headers["X-Forwarded-For"];
            if (string.IsNullOrEmpty(xfr) || xfr.IndexOf("unknown") >= 0)
            {
                strIPAddr = context.Connection.RemoteIpAddress.ToString();
            }
            else
            {
                string[] arr = xfr.Split(new char[] { ',', ';' });
                if (arr.Length >= 1)
                {
                    strIPAddr = arr[0].Trim();
                }
            }
            return strIPAddr;

        }

        /// <summary>
        /// get app path or site root. end with "\"
        /// </summary>
        /// <returns></returns>
        public static string GetAppPath()
        {
            string appbase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            if (appbase.EndsWith("\\"))
                return appbase;
            else
                return appbase += "\\";
        }

        public static bool PrepareLogDir(string dir)
        {
            if (System.IO.Directory.Exists(dir) == false)
            {
                System.IO.Directory.CreateDirectory(dir);
            }
            return System.IO.Directory.Exists(dir);
        }

        /// <summary>
        /// 32位字节序调整
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static UInt32 Swap32(long v)
        {
            return (UInt32)(((v & 0x000000ff) << 24) | ((v & 0x0000ff00) << 8) | ((v & 0x00ff0000) >> 8) | ((v & 0xff000000) >> 24));
        }

        public static object ConvertObject(object obj, Type type)
        {
            if (type == null) return obj;
            if (obj == null) return type.IsValueType ? Activator.CreateInstance(type) : null;

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (type.IsAssignableFrom(obj.GetType())) // 如果待转换对象的类型与目标类型兼容，则无需转换
            {
                return obj;
            }
            else if ((underlyingType ?? type).IsEnum) // 如果待转换的对象的基类型为枚举
            {
                if (underlyingType != null && string.IsNullOrEmpty(obj.ToString())) // 如果目标类型为可空枚举，并且待转换对象为null 则直接返回null值
                {
                    return null;
                }
                else
                {
                    return Enum.Parse(underlyingType ?? type, obj.ToString());
                }
            }
            else if (typeof(IConvertible).IsAssignableFrom(underlyingType ?? type)) // 如果目标类型的基类型实现了IConvertible，则直接转换
            {
                try
                {
                    return Convert.ChangeType(obj, underlyingType ?? type, null);
                }
                catch
                {
                    return underlyingType == null ? Activator.CreateInstance(type) : null;
                }
            }
            else
            {
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertFrom(obj.GetType()))
                {
                    return converter.ConvertFrom(obj);
                }
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    object o = constructor.Invoke(null);
                    PropertyInfo[] propertys = type.GetProperties();
                    Type oldType = obj.GetType();
                    foreach (PropertyInfo property in propertys)
                    {
                        PropertyInfo p = oldType.GetProperty(property.Name);
                        if (property.CanWrite && p != null && p.CanRead)
                        {
                            property.SetValue(o, ConvertObject(p.GetValue(obj, null), property.PropertyType), null);
                        }
                    }
                    return o;
                }
            }
            return obj;
        }

#pragma warning disable IDE0019 // 使用模式匹配

        public static string GetDescription(Enum obj)
        {
            string objName = obj.ToString();
            Type t = obj.GetType();
            FieldInfo fi = t.GetField(objName);
            if (fi == null)
                return objName;

            var displayNameAttr = fi.GetCustomAttribute<DisplayNameAttribute>(false);
            if (displayNameAttr != null)
                return displayNameAttr.DisplayName;

            var displayAttr = fi.GetCustomAttribute<DisplayAttribute>(false);
            if (displayAttr != null)
                return displayAttr.Name;

            var arrDesc = fi.GetCustomAttribute<DescriptionAttribute>(false);
            if (arrDesc != null)
                return arrDesc.Description;


            return objName;
        }

        public static T GetEnumAttribute<T>(Enum obj, bool inherit = false) where T : class
        {
            string objName = obj.ToString();
            Type t = obj.GetType();
            FieldInfo fi = t.GetField(objName);
            if (fi == null)
                return null;
            T[] arrDesc = fi.GetCustomAttributes(typeof(T), inherit) as T[];

            if (arrDesc != null && arrDesc.Length > 0)
                return arrDesc[0];
            else
                return null;
        }
        /// <summary>
        /// 获取type类型中，名称为fieldName的字段的TAttrib类型属性
        /// </summary>
        /// <typeparam name="TAttrib"></typeparam>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static TAttrib GetFieldAttribute<TAttrib>(Type type, string fieldName, bool inherit = false) where TAttrib : class
        {
            FieldInfo fi = type.GetField(fieldName);
            if (fi == null)
                return null;
            TAttrib[] arrDesc = fi.GetCustomAttributes(typeof(TAttrib), inherit) as TAttrib[];

            if (arrDesc != null && arrDesc.Length > 0)
                return arrDesc[0];
            else
                return null;
        }

        /// <summary>
        /// 获取指定了TAttrib的枚举值列表
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <typeparam name="TAttrib">要检索的Attribute类型</typeparam>
        /// <returns>指定了TAttrib的枚举值列表</returns>
        public static List<TEnum> GetEnumValuesWithAttribute<TEnum, TAttrib>()
            where TAttrib : class
        {
            return GetEnumValuesWithAttribute<TEnum, TAttrib>(null);
        }


        /// <summary>
        /// 获取枚举值包含 TAttrib 且满足特定条件的枚举值列表
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <typeparam name="TAttrib">要检索的Attribute类型</typeparam>
        /// <param name="match">枚举值包含 TAttrib 且必须满足特定条件，match返回true则表示符合条件。</param>
        /// <returns>符合条件的枚举值列表</returns>
        public static List<TEnum> GetEnumValuesWithAttribute<TEnum, TAttrib>(Func<TAttrib, bool> match)
            where TAttrib : class
        {
            var valueList = System.Enum.GetValues(typeof(TEnum));
            var resultList = new List<TEnum>();
            foreach (TEnum enumItem in valueList)
            {
                var attr = GetFieldAttribute<TAttrib>(typeof(TEnum), enumItem.ToString(), false);
                if (attr != null)
                {
                    if (match != null)
                    {
                        if (match(attr))
                        {
                            resultList.Add(enumItem);
                        }
                    }
                    else
                    {
                        resultList.Add(enumItem);

                    }
                }
            }
            return resultList;
        }

        public static string GetDescription(PropertyInfo prop)
        {
            DescriptionAttribute descrAttr = prop.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute;
            if (descrAttr != null)
                return descrAttr.Description;
            else
                return null;
        }

        public static string GetDescription(Type type)
        {
            DescriptionAttribute descrAttr = type.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute;
            if (descrAttr != null)
                return descrAttr.Description;
            else
                return null;
        }

        public static string GetDescription(Type enumType, string enumValue)
        {
            string objName = enumValue.ToString();
            FieldInfo fi = enumType.GetField(objName);
            if (fi == null)
                return objName;

            var displayNameAttr = fi.GetCustomAttribute<DisplayNameAttribute>(false);
            if (displayNameAttr != null)
                return displayNameAttr.DisplayName;

            var displayAttr = fi.GetCustomAttribute<DisplayAttribute>(false);
            if (displayAttr != null)
                return displayAttr.Name;

            var arrDesc = fi.GetCustomAttribute<DescriptionAttribute>(false);
            if (arrDesc != null)
                return arrDesc.Description;


            return objName;
        }
#pragma warning restore IDE0019 // 使用模式匹配

        public static string GenerateRandom(int length)
        {
            string RandomCode;
            if (length > 16)
            {
                return null;
            }

            if (length > 8)
            {
                RandomCode = rnd.Next((int)Math.Pow(10, 7), (int)Math.Pow(10, 8) - 1).ToString();
                RandomCode += rnd.Next((int)Math.Pow(10, length - 9), (int)Math.Pow(10, length - 8) - 1).ToString();
            }
            else
            {
                RandomCode = rnd.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length) - 1).ToString();
            }
            return RandomCode.Length == length ? RandomCode : null;
        }

        /// <summary>
        /// 是否是合法的手机号
        /// </summary>
        /// <param name="phoneNumber">手机号</param>
        /// <returns></returns>
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^1[^12]\d{9}$");
        }

        public static object CloneObject(object o)
        {
            Type t = o.GetType();
            PropertyInfo[] properties = t.GetProperties();
            Object p = t.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, o, null);
            foreach (PropertyInfo pi in properties)
            {
                if (pi.CanWrite)
                {
                    object value = pi.GetValue(o, null);
                    pi.SetValue(p, value, null);
                }
            }
            return p;
        }

        private const double EARTH_RADIUS = 6378137.00;//地球半径

        private static double Rad(double d)
        {
            return d * Math.PI / 180.0;
        }
        /// <summary>
        /// 获取两个经纬度之间的距离，单位米
        /// </summary>
        /// <param name="lng1"></param>
        /// <param name="lat1"></param>
        /// <param name="lng2"></param>
        /// <param name="lat2"></param>
        /// <returns></returns>
        public static double GetDistance(double lng1, double lat1, double lng2, double lat2)
        {
            double radLat1 = Rad(lat1);
            double radLat2 = Rad(lat2);
            double a = radLat1 - radLat2;
            double b = Rad(lng1) - Rad(lng2);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
             Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            s = Math.Round(s * 10000) / 10000;
            return Math.Abs(s);
        }

        public static string NumberToChinese(int number)
        {
            string res = string.Empty;
            string str = number.ToString();
            string schar = str.Substring(0, 1);
            switch (schar)
            {
                case "1":
                    res = "一";
                    break;
                case "2":
                    res = "二";
                    break;
                case "3":
                    res = "三";
                    break;
                case "4":
                    res = "四";
                    break;
                case "5":
                    res = "五";
                    break;
                case "6":
                    res = "六";
                    break;
                case "7":
                    res = "七";
                    break;
                case "8":
                    res = "八";
                    break;
                case "9":
                    res = "九";
                    break;
                default:
                    res = "零";
                    break;
            }
            if (str.Length > 1)
            {
                switch (str.Length)
                {
                    case 2:
                    case 6:
                        res += "十";
                        break;
                    case 3:
                    case 7:
                        res += "百";
                        break;
                    case 4:
                        res += "千";
                        break;
                    case 5:
                        res += "万";
                        break;
                    default:
                        res += "";
                        break;
                }
                res += NumberToChinese(int.Parse(str.Substring(1, str.Length - 1)));
            }
            return res;
        }

        /// <summary>
        /// 去掉集合相邻重复项
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IList<int> RemoveDuplicateList(IList<int> list)
        {
            if (list != null && list.Count > 0)
            {
                for (int i = list.Count - 1; i > 0; i--)
                {
                    if (list[i] == list[i - 1])
                    {
                        list.RemoveAt(i);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取CRC验证
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isReverse"></param>
        /// <returns></returns>
        public static long ToModbusCRC16(string s, bool isReverse)
        {

            return CRC16(StringToHexByte(s, true));
        }

        private static long CRC16(byte[] data)
        {
            int len = data.Length;
            if (len > 0)
            {
                ushort crc = 0xFFFF;

                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }

                return (crc & 0x00FF);
            }
            return 0;
        }

        /// <summary>
        /// 字符串转字节
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isFilterChinese">是否过滤中文字符</param>
        /// <returns></returns>
        public static byte[] StringToHexByte(string str, bool isFilterChinese)
        {
            string hex = isFilterChinese ? FilterChinese(str) : ConvertChinese(str);

            //清除所有空格
            hex = hex.Replace(" ", "");
            //若字符个数为奇数，补一个0
            hex += hex.Length % 2 != 0 ? "0" : "";

            byte[] result = new byte[hex.Length / 2];
            for (int i = 0, c = result.Length; i < c; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return result;
        }

        /// <summary>
        /// Html转文字
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static string ConvertHtmlToText(string strHtml)
        {
            strHtml = strHtml.Replace("\r\n", "");
            strHtml = strHtml.Replace("</p>", "\r\n").Replace("<br/>", "\r\n");
            string[] aryReg ={
          @"<script[^>]*?>.*?</script>",
          @"<(.[^>]*)>",
          //@"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
          //@"([\r\n])[\s]+",
          @"&(quot|#34);",
          @"&(amp|#38);",
          @"&(lt|#60);",
          @"&(gt|#62);",
          @"&(nbsp|#160);",
          @"&(iexcl|#161);",
          @"&(cent|#162);",
          @"&(pound|#163);",
          @"&(copy|#169);",
          @"&#(\d+);",
          @"-->",
          @"<!--.*\n"

         };

            string[] aryRep = {
           "",
           "",
           //"",
           //"",
           "\"",
           "&",
           "<",
           ">",
           " ",
           "\xa1",//chr(161),
           "\xa2",//chr(162),
           "\xa3",//chr(163),
           "\xa9",//chr(169),
           "",
           "\r\n",
           ""
          };

            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }

            strOutput = strOutput.Replace("<", "");
            strOutput = strOutput.Replace(">", "");
            //strOutput.Replace("\r\n", "");

            return strOutput;
        }

        /// <summary>
        /// 过滤中文字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string FilterChinese(string str)
        {
            StringBuilder s = new StringBuilder();
            foreach (short c in str.ToCharArray())
            {
                if (c > 0 && c < 127)
                {
                    s.Append((char)c);
                }
            }
            return s.ToString();
        }

        private static string ConvertChinese(string str)
        {
            StringBuilder s = new StringBuilder();
            foreach (short c in str.ToCharArray())
            {
                if (c <= 0 || c >= 127)
                {
                    s.Append(c.ToString("X4"));
                }
                else
                {
                    s.Append((char)c);
                }
            }
            return s.ToString();
        }

        public static string MysqlParamFilter(this string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
                return paramValue;
            else
                return paramValue.Replace("'", "''").Replace("\\", "\\\\").Replace("%", "\\%");
        }

        /// <summary>
        /// 深度复制对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="RealObject"></param>
        /// <returns></returns>
        public static T Clone<T>(T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                //利用 System.Runtime.Serialization序列化与反序列化完成引用对象的复制  
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, RealObject);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(objectStream);
            }
        }

        /// <summary>
        /// 深度复制集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="List"></param>
        /// <returns></returns>
        public static List<T> Clone<T>(object List)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, List);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as List<T>;
            }
        }
        public static bool EmptyArray<T>(List<T> arr)
        {
            return arr == null || arr.Count == 0;
        }
        public static bool EmptyArray<T>(T[] arr)
        {
            return arr == null || arr.Length == 0;
        }
    }
}