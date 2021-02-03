using System;
using Microsoft.AspNetCore.Http;

namespace Utility
{
    public static class Cookies
    {
        private static string _key = "*POU(^!@#akd5457865165";

        /// <summary>
        /// 写入cookie，默认时效为7天
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="page"></param>
        public static void SetCookie(this HttpContext context, string name, string value)
        {
            SetCookie(context, name, value, 7 * 24 * 60);
        }

        /// <summary>
        /// 写入cookie
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="min">过期时间，单位分钟。如果为0，则为会话cookie</param>
        /// <param name="page"></param>
        public static void SetCookie(this HttpContext context, string name, string value, int min)
        {
            HttpResponse Response = context.Response;
            {
                Response.Cookies.Append(name + "-encry", CommonTools.MD5(value + _key), new CookieOptions()
                {
                    Expires = min == 0 ? DateTime.MinValue : DateTime.Now.AddMinutes(min)
                });
                Response.Cookies.Append(name, value, new CookieOptions()
                {
                    Expires = min == 0 ? DateTime.MinValue : DateTime.Now.AddMinutes(min)
                });
            }
        }

        /// <summary>
        /// 获取cookie
        /// </summary>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetCookie(this HttpContext context, string name)
        {
            var request = context.Request;
            request.Cookies.TryGetValue(name + "-encry", out string value_md5);
            if (string.IsNullOrEmpty(value_md5))
                return null;
            request.Cookies.TryGetValue(name, out string value);
            if (string.IsNullOrEmpty(value))
                return null;

            if (CommonTools.MD5(value + _key) != value_md5)
            {
                throw new Exception("Cookie validation failure!");
            }
            else
                return value;
        }

        /// <summary>
        /// 删除cookie
        /// </summary>
        /// <param name="name"></param>
        /// <param name="page"></param>
        public static void Delete(this HttpContext context,string name)
        {

            HttpResponse response = context.Response;
            response.Cookies.Delete(name);
            response.Cookies.Delete(name + "-encry");
           
        }
    }
}