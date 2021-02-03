using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Utility
{
    public static class HttpUtil
    {
        private static Log5 log = new Log5();

        public static string GetHttpGetString(string url, ContentType contentType = ContentType.text_plain)
        {
            try
            {
                System.Net.HttpWebRequest myRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                myRequest.Method = "GET";

                myRequest.ContentType = contentType.ToContentTypeStr();
                myRequest.Proxy = null;

                // Get response
                System.Net.HttpWebResponse myResponse = (System.Net.HttpWebResponse)myRequest.GetResponse();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8")))
                {
                    string content = reader.ReadToEnd();
                    return content;
                }
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }

        public static object GetHttpGetResult(string url, Type tp)
        {
            string content = GetHttpGetString(url);
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    return Utility.JsonHelper.ParseFromJson(tp, content);
                }
                catch (Exception ex)
                {
                    log.Error(content);
                    log.Error(ex.ToString());

                    return null;
                }
            }
            else
                return null;
        }
        public static T GetHttpGetResult<T>(string url) where T : class, new()
        {
            string content = GetHttpGetString(url);
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    return Utility.JsonHelper.ParseFromJson<T>(content);
                }
                catch (Exception ex)
                {
                    log.Error(content);
                    log.Error(ex.ToString());

                    return null;
                }
            }
            else
                return null;
        }

        public static string GetPostString(string url, string data, ContentType contentType = ContentType.application_json)
        {
            try
            {
                byte[] postBytes = System.Text.Encoding.GetEncoding("utf-8").GetBytes(data);

                System.Net.HttpWebRequest myRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                myRequest.Method = "POST";

                myRequest.ContentType = contentType.ToContentTypeStr();
                myRequest.ContentLength = postBytes.Length;
                myRequest.Proxy = null;

                System.IO.Stream newStream = myRequest.GetRequestStream();
                newStream.Write(postBytes, 0, postBytes.Length);
                newStream.Close();

                // Get response
                System.Net.HttpWebResponse myResponse = (System.Net.HttpWebResponse)myRequest.GetResponse();
                using (System.IO.StreamReader reader = new System.IO.StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8")))
                {
                    string content = reader.ReadToEnd();
                    return content;
                }
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// post data to server, including url verification and data encryption
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T GetPostResult<T>(string url, string data, ContentType contentType = ContentType.application_json) where T : class
        {
            if (data == null)
                return null;
            string poststr = data;
            var respText = "";
            try
            {
                respText = GetPostString(url, poststr, contentType);
                if (!string.IsNullOrEmpty(respText))
                {
                    return JsonHelper.ParseFromJson<T>(respText);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                log.Error("post exception: msg={0}, url={1}, resp_content={2}", ex.Message, url, respText);
                throw;
            }

        }

        /// <summary>
        /// 枚举转string
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static string ToContentTypeStr(this ContentType ct)
        {
            var s = ct.ToString();
            return s.Replace("__", "-").Replace("_", "/");
        }

        private static Encoding GetRequestEncoding(HttpRequest request)
        {
            //var requestContentType = request.ContentType;
            //var requestMediaType = requestContentType == null ? default(MediaType) : new MediaType(requestContentType);
            //var requestEncoding = requestMediaType.Encoding;
            //if (requestEncoding == null)
            //{
            //    requestEncoding = Encoding.UTF8;
            //}
            //return requestEncoding;

            return Encoding.UTF8;
        }

        public static string DumpHttpRequest(HttpRequest request)
        {
            try
            {
                string postStr = null;

                if (request.Method == "POST")
                {
                    Stream stm = request.Body;
                    stm.Seek(0, SeekOrigin.Begin);
                    byte[] postBuffer = null;
                    postBuffer = new byte[stm.Length];
                    stm.Read(postBuffer, 0, (int)stm.Length);
                    if (stm.CanSeek)
                    {
                        stm.Seek(0, SeekOrigin.Begin);
                    }

                    postStr = (GetRequestEncoding(request) ?? System.Text.Encoding.UTF8).GetString(postBuffer);
                }


                string urlParams = request.Path + request.QueryString;
                /*
                 * http headers
                 */
                string reqHeaderparms = string.Empty;
                foreach (var hEntry in request.Headers)
                {
                    var val = hEntry.Value;
                    reqHeaderparms += string.Format("{0}: {1}\r\n", hEntry.Key, val);
                }



                //log记录
                var logStr = $@"
    {request.Method} { urlParams}
    {reqHeaderparms}

    { postStr}";


                return logStr;

            }
            catch (Exception ex)
            {
                log.Error("全局Application_Error出现异常:\r\n" + ex.ToString() + "\r\n");
                return null;
            }
        }
    }

    public enum ContentType
    {
        /// <summary>
        /// text/html
        /// </summary>
        text_html,

        /// <summary>
        /// text/plain
        /// </summary>
        text_plain,

        /// <summary>
        /// application/json
        /// </summary>
        application_json,

        /// <summary>
        /// application/x-www-form-urlencoded
        /// </summary>
        application_x__www__form__urlencoded,
    }
}