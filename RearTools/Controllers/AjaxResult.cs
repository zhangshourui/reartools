using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace RearTools.Controllers
{
    public class AjaxResult : ActionResult
    {
        public ResultCode Code { get; set; }
        /// <summary>
        /// 结果说明
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 操作的附加数据
        /// </summary>
        public object Data { get; set; }
        public string ToJson()
        {
            return Utility.JsonHelper.GetJson(this);
        }
        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.ContentType = "application/json";
            var json = Utility.JsonHelper.GetJson(this);

            context.HttpContext.Response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(System.Text.Encoding.UTF8.GetBytes(json)));

        }

        /// <summary>
        /// 返回浏览器错误消息，可以同时返回携带的数据，默认为null。
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static AjaxResult Error(string msg, object data = null)
        {
            return new AjaxResult() { Data = data, Message = msg, Success = false, Code = ResultCode.BizErr };
        }
        public static AjaxResult Error(string msg, ResultCode code)
        {
            return new AjaxResult() { Data = null, Message = msg, Success = false, Code = code };
        }
        /// <summary>
        ///  返回浏览器成功的消息，并携带数据（Data字段）。同时制定消息内容
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static AjaxResult OK(string msg, object data = null)
        {
            return new AjaxResult() { Data = data, Message = msg, Success = true };
        }

        /// <summary>
        ///  返回浏览器成功的消息，并携带数据（Data字段）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static AjaxResult OK(object data)
        {
            return new AjaxResult() { Data = data, Message = null, Success = true };
        }
        /// <summary>
        /// 返回浏览器成功的消息，不携带数据（Data字段为null）
        /// </summary>
        /// <returns></returns>
        public static AjaxResult OK()
        {
            return new AjaxResult() { Data = null, Message = null, Success = true };
        }
        public enum ResultCode
        {
            [Description("操作成功")]
            OK = 0,

            [Description("系统异常")]
            ErrorException = 101,

            [Description("消息格式不正确")]
            ErrorFmt = 102,

            [Description("空数据")]
            ErrorEmpty = 104,

            [Description("不允许操作")]
            ErrorNotAllowed = 105,

            [Description("请求方式错误")]
            ErrorHttpMethod = 106,

            [Description("需要登录")]
            NotLogin = 107,

            /*开始业务逻辑方面从错误*/
            __bll_start = 255,
            [Description("一般性业务错误")]
            BizErr = 256,
        }

    }

}
