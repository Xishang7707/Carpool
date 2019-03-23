using Newtonsoft.Json.Linq;
using System;
using System.Web;
using System.Web.Http;
using System.Web.SessionState;

namespace API.API
{
    public class APIController : ApiController
    {
        public HttpRequest Request { get; set; }
        public HttpResponse Response { get; set; }
        public HttpSessionState Session { get; set; }

        public APIController()
        {
            Request = HttpContext.Current.Request;
            Response = HttpContext.Current.Response;
            Session = HttpContext.Current.Session;
        }

        /// <summary>
        /// 向客户端发送数据
        /// </summary>
        /// <param name="code">状态码</param>
        /// <param name="msg">信息</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        protected JObject SendData(int code = 200, string msg = "成功", JToken data = null)
        {
            JObject ret_json = new JObject();
            ret_json.Add("code", code);
            ret_json.Add("msg", msg);
            ret_json.Add("data", data);

            return ret_json;
        }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetNowTime()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds; // 相差毫秒数
            return timeStamp;
        }
    }
}
