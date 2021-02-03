using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Utility
{
    public class PubConstant
    {
        /// <summary>
		/// 获取连接字符串
		/// </summary>
//		public static string ConnectionString
//        {
//            get
//            {
//#if DEBUG
//                return ConfigurationManager.AppSettings["ConnectionStringSQLServer-local"];

//#else
//				return ConfigurationManager.AppSettings["ConnectionStringSQLServer"];
//#endif
//            }
//        }
        public static int CommandTimeout
        {
            get
            {
#if DEBUG
                return Utility.CommonTools.GetNumber(ConfigurationManager.AppSettings["SQLServerCommandTimeout"], 60);
#else
				return Utility.CommonTools.GetNumber(ConfigurationManager.AppSettings["SQLServerCommandTimeout"], 60);
#endif
            }
        }
        public static int CommandTimeoutMysql
        {
            get
            {
#if DEBUG
                return Utility.CommonTools.GetNumber(ConfigurationManager.AppSettings["SQLServerCommandTimeout"], 60);
#else
                return Utility.CommonTools.GetNumber(ConfigurationManager.AppSettings["MysqlCommandTimeout"], 60);
#endif
            }
        }

        public static string ConnectionStringSqlit
        {
            get
            {
                /*	string dbFile = "";
					if (System.IO.File.Exists(DBFileToday))
						dbFile = DBFileToday;
					else if (System.IO.File.Exists(DBFileYesterday))
						dbFile = DBFileYesterday;
					else
					{
						//throw new Exception("sqlite db does not exist.");
					}
				*/
                string connstr = string.Format("Data Source={0}AmountMaildb;Pooling=true;FailIfMissing=false",
                                  CommonTools.GetAppPath());
                return connstr;
            }
        }

        /// <summary>
        /// 得到web.config里配置项的数据库连接字符串。
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static string GetConnectionString(string configName)
        {
            string connectionString;
            string ConStringEncrypt = ConfigurationManager.AppSettings["ConStringEncrypt"];

            connectionString = ConfigurationManager.AppSettings[configName];
            return connectionString;
        }
    }
}
