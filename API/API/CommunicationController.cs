using API.DAL;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Http;

namespace API.API
{
    public class CommunicationController : APIController
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        /// option
        ///     0.注册
        [HttpPost]
        public JObject SendTelCode(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string tel = data["tel"]?.ToString();
                string option = data["option"]?.ToString();
                if (tel == null || tel.Length != 11 || option == "")
                    return SendData(12001, "短信发送失败");
                Random rand = new Random((int)GetNowTime());
                string telcode = rand.Next(100000, 999999) + "";
                bool result = SendTelCode(tel, telcode);

                switch (option)
                {
                    case "0":
                        if (UserDAL.Exist(tel))
                            return SendData(10001, "手机号码已被注册");
                        break;
                    default:
                        return SendData(400, "请求错误");
                }

                if (!result)
                    return SendData(12001, "短信发送失败");
                JObject telcode_json = new JObject();

                telcode_json.Add("telcode", telcode);
                telcode_json.Add("tel", tel);

                Session["telcode"] = telcode_json;
                return SendData(200, "发送成功");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="tel">手机号码</param>
        /// <param name="tel_code">验证码</param>
        /// <returns></returns>
        [NonAction]
        public static bool SendTelCode(string tel, string tel_code)
        {
            try
            {
                string app_code = "eab9ae3d33ef8003";
                string tel_req_url = "http://api.jisuapi.com/sms/send";
                string querys = "mobile=" + tel + "&content=验证码：" + tel_code + "，如非本人操作，请忽略本短信【拼车网】&appkey=" + app_code;

                HttpWebRequest httpRequest = null;
                HttpWebResponse httpResponse = null;

                tel_req_url += "?" + querys;

                httpRequest = (HttpWebRequest)WebRequest.Create(tel_req_url);

                httpRequest.Method = "GET";
                //httpRequest.Headers.Add("Authorization", "APPCODE " + app_code);

                try
                {
                    httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                }
                catch (WebException ex)
                {
                    httpResponse = (HttpWebResponse)ex.Response;
                }

                Stream st = httpResponse.GetResponseStream();
                StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));

                string ret_str = reader.ReadToEnd();
                JObject ret_json = JObject.Parse(ret_str);

                string code = ret_json["status"]?.ToString();

                if (code == null || code != "0")
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
