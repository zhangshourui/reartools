using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class IPHelper
    {

        //public static bool IsIPLimit(out string errorMsg)
        //{
        //    bool result = false;
        //    errorMsg = string.Empty;
        //    string ipconfig = ConfigurationManager.AppSettings["AuthIPList"].ToString().Trim();
        //    string userIP = Utility.CommonTools.GetClientIP();
        //    IPAddress clientIp = System.Net.IPAddress.Parse(userIP);
        //    IPAddress masterIp = System.Net.IPAddress.Parse(ipconfig.Split('/')[0]);
        //    IPAddress ipcode = System.Net.IPAddress.Parse(ipconfig.Split('/')[1]);
        //    //判断是不是回环地址
        //    long start = IPHelper.IP2Long("127.0.0.1");
        //    long end = IPHelper.IP2Long("127.0.0.255");
        //    string clientIpStr = string.Empty;
        //    long ipAddress = 0;
        //    if (clientIp.ToString().Equals("::1"))
        //    {
        //        clientIpStr = "127.0.0.1";
        //        ipAddress = IPHelper.IP2Long(clientIpStr);
        //    }
        //    if (ipAddress >= start && ipAddress <= end) //是回环地址
        //    {
        //        //IP Address fits within range!
        //    }
        //    else
        //    {
        //        if (clientIp.AddressFamily != AddressFamily.InterNetwork)
        //        {
        //            errorMsg = "不是IPV4地址,没有权限访问";
        //            return result;
        //        }

        //        //clientIp.Address&0x7f000000 == 0x7f000000;
        //        byte[] bytesMaster = masterIp.GetAddressBytes().Zip(ipcode.GetAddressBytes(), (i, m) => (byte)(i & m)).ToArray();
        //        string masterResult = new IPAddress(bytesMaster).ToString().Trim();
        //        byte[] bytesClient = clientIp.GetAddressBytes().Zip(ipcode.GetAddressBytes(), (i, m) => (byte)(i & m)).ToArray();
        //        string clientResult = new IPAddress(bytesClient).ToString().Trim();
        //        if (!masterResult.Equals(clientResult))
        //        {
        //            errorMsg = "您没有权限访问，请联系管理员";
        //            return result;
        //        }
        //    }

        //    result = true;

        //    return result;
        //}
        public static long IP2Long(string ip)
        {
            //code from www.sharejs.com
            string[] ipBytes;
            double num = 0;
            if (!string.IsNullOrEmpty(ip))
            {
                ipBytes = ip.Split('.');
                for (int i = ipBytes.Length - 1; i >= 0; i--)
                {
                    num += ((int.Parse(ipBytes[i]) % 256) * Math.Pow(256, (3 - i)));
                }
            }
            return (long)num;
        }
    }
}
