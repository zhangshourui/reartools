using System;
using Newtonsoft.Json;


namespace Utility
{
    /// <summary>
    /// Summary description for JsonHelper
    /// </summary>
    public static class JsonHelper
    {

        /// <summary>
        /// 生成Json格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetJson(object obj)
        {
            var jSetting = new JsonSerializerSettings
            {
                //NullValueHandling = NullValueHandling.Ignore, 
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
                Formatting = Formatting.None,
            };

            return JsonConvert.SerializeObject(obj, jSetting);


        }

        /// <summary>
        /// 获取Json的Model
        /// </summary>
        /// <param name="T"></param>
        /// <param name="szJson"></param>
        /// <returns></returns>
        public static object ParseFromJson(Type tp, string szJson)
        {
            if (string.IsNullOrEmpty(szJson))
            {
                return null;
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject(szJson, tp);

            /*
			using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(tp);
				return serializer.ReadObject(ms);
			}
			 */
            /*
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			object obj = serializer.Deserialize(szJson, tp);
			return obj;
			 */

        }
        public static T ParseFromJson<T>(string szJson)
        {
            if (string.IsNullOrEmpty(szJson))
            {
                return default(T);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(szJson);

            /*
			using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(tp);
				return serializer.ReadObject(ms);
			}
			 */
            /*
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			object obj = serializer.Deserialize(szJson, tp);
			return obj;
			 */

        }

        public static object ParseFromJsonArray(Type tp, string szJson)
        {
            if (string.IsNullOrEmpty(szJson))
            {
                return null;
            }

            Newtonsoft.Json.Linq.JArray jarray = Newtonsoft.Json.Linq.JArray.Parse(szJson);
            if (jarray.Count > 0)
            {
                Newtonsoft.Json.Linq.JToken jt = jarray[0];
                return jt.ToObject(tp);
            }
            else
                return null;
        }

    }

}
