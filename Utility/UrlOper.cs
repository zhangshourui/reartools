using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Collections.Specialized;

namespace Maticsoft.Common
{
    /// <summary>
    /// URL的操作类
    /// </summary>
    public class UrlOper
    {
        /// <summary>
        /// 添加URL参数
        /// </summary>
        public static string SetUrlParam(string url, string paramName, string value)
        {
            Uri uri = new Uri(url);
            if (string.IsNullOrEmpty(uri.Query))
            {
                string eval = HttpUtility.UrlEncode(value);
                return String.Concat(url, "?" + paramName + "=" + eval);
            }
            else
            {
                if (!UpdateUrlQueryValue(ref url, paramName, value))
                {
                    string eval = HttpUtility.UrlEncode(value);
                    return string.Concat(url, "&" + paramName + "=" + HttpUtility.UrlEncode(eval));
                }
                {
                    return url;
                }
            }
        }
        /// <summary>
        /// 更新URL参数
        /// </summary>
        private static bool UpdateUrlQueryValue(ref string url, string paramName, string value)
        {
            url = HttpUtility.UrlEncode(value);
            string keyWord = paramName + "=";
            int index = url.IndexOf(keyWord) + keyWord.Length;
            if (index >= 0)
            {
                int index1 = url.IndexOf("&", index);
                if (index1 == -1)
                {
                    url = url.Remove(index, url.Length - index);
                    url = string.Concat(url, value);
                }
                else
                {
                    url = url.Remove(index, index1 - index);
                    url = url.Insert(index, value);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #region 分析URL所属的域
        public static void GetDomain(string fromUrl, out string domain, out string subDomain)
        {
            domain = "";
            subDomain = "";
            try
            {
                if (fromUrl.IndexOf("的名片") > -1)
                {
                    subDomain = fromUrl;
                    domain = "名片";
                    return;
                }

                UriBuilder builder = new UriBuilder(fromUrl);
                fromUrl = builder.ToString();

                Uri u = new Uri(fromUrl);

                if (u.IsWellFormedOriginalString())
                {
                    if (u.IsFile)
                    {
                        subDomain = domain = "客户端本地文件路径";

                    }
                    else
                    {
                        string Authority = u.Authority;
                        string[] ss = u.Authority.Split('.');
                        if (ss.Length == 2)
                        {
                            Authority = "www." + Authority;
                        }
                        int index = Authority.IndexOf('.', 0);
                        domain = Authority.Substring(index + 1, Authority.Length - index - 1).Replace("comhttp", "com");
                        subDomain = Authority.Replace("comhttp", "com");
                        if (ss.Length < 2)
                        {
                            domain = "不明路径";
                            subDomain = "不明路径";
                        }
                    }
                }
                else
                {
                    if (u.IsFile)
                    {
                        subDomain = domain = "客户端本地文件路径";
                    }
                    else
                    {
                        subDomain = domain = "不明路径";
                    }
                }
            }
            catch
            {
                subDomain = domain = "不明路径";
            }
        }

        /// <summary>
        /// 分析 url 字符串中的参数信息
        /// </summary>
        /// <param name="url">输入的 URL</param>
        /// <param name="baseUrl">输出 URL 的基础部分</param>
        /// <param name="nvc">输出分析后得到的 (参数名,参数值) 的集合</param>
        public static void ParseUrl(string url, out string baseUrl, out NameValueCollection nvc)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            nvc = new NameValueCollection();
            baseUrl = "";

            if (url == "")
                return;

            int questionMarkIndex = url.IndexOf('?');

            if (questionMarkIndex == -1)
            {
                baseUrl = url;
                return;
            }
            baseUrl = url.Substring(0, questionMarkIndex);
            if (questionMarkIndex == url.Length - 1)
                return;
            string ps = url.Substring(questionMarkIndex + 1);

            // 开始分析参数对    
            Regex re = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
            MatchCollection mc = re.Matches(ps);

            foreach (Match m in mc)
            {
                nvc.Add(m.Result("$2").ToLower(), m.Result("$3"));
            }
        }

        /// <summary>
        /// 获取URL参数列表
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static SortedDictionary<string, string> GetUrlParams(string url)
        {
            int startIndex = url.IndexOf("?");
            var paras = new SortedDictionary<string, string>();

            if (startIndex <= 0)
                return paras;

            string[] nameValues = url.Substring(startIndex + 1).Split('&');

            foreach (string s in nameValues)
            {
                string[] pair = s.Split('=');

                string name = pair[0];
                string value = string.Empty;

                if (pair.Length > 1)
                    value = HttpUtility.UrlDecode(pair[1]);

                paras.Add(name, value);
            }

            return paras;
        }
        #endregion
    }
}
