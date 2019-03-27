using API.DAL;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;

namespace API.API
{
    public class UsersController : APIController
    {
        [HttpPost]
        public JObject Exist(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data?.ToString());
                string tel = data["tel"]?.ToString();
                if (tel == null || tel.Length != 11)
                    return SendData(400, "请求错误");

                if (UserDAL.Exist(data["tel"]?.ToString()))
                {
                    return SendData(10001, "手机号码已被注册");
                }
                return SendData(0, "手机号码可用");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject Register(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());

                if (data["tel"] == null || data["telcode"] == null || data["password"] == null)
                {
                    return SendData(400, "请求错误");
                }

                if (UserDAL.Exist(data["tel"]?.ToString()))
                {
                    return SendData(10001, "手机号码已被注册");
                }

                JObject telcode_json = (JObject)Session["telcode"];

                if (telcode_json["tel"].ToString() != data["tel"].ToString() ||
                    telcode_json["telcode"].ToString() != data["telcode"].ToString())
                    return SendData(12002, "短信验证码错误");

                bool result = UserDAL.Register(data["tel"]?.ToString(), data["password"]?.ToString());
                if (!result)
                    return SendData(-1, "注册失败");
                Session.Remove("telcode");
                return SendData(200, "注册成功");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误", e.ToString());
            }
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject Login(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());

                string tel = data["tel"]?.ToString();
                string password = data["password"]?.ToString();

                if (tel == "" || password == "")
                    return SendData(400, "请求错误");

                JObject log_data = UserDAL.Login(tel, password);
                if (log_data == null)
                {
                    return SendData(15002, "手机号码或者密码错误");
                }
                JObject ret_data = new JObject();

                ret_data.Add("CarpoolSSID", log_data["id"]);

                Session.Add("CarpoolSSID", log_data["id"]);

                return SendData(msg: "登录成功", data: ret_data);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetInfo(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data?.ToString());

                string us_id = data["CarpoolSSID"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"].ToString() != us_id)
                {
                    return SendData(15001, "未授权访问");
                }

                JObject us_json = UserDAL.GetInfo(us_id);
                if (us_json == null)
                    return SendData(400, "请求错误");

                JObject ret_json = new JObject();

                ret_json.Add("id", us_json["id"]?.ToString());
                ret_json.Add("tel", us_json["tel"]?.ToString());
                ret_json.Add("name", us_json["name"]?.ToString());
                ret_json.Add("attention", us_json["attention"]?.ToString());
                ret_json.Add("headportrait", us_json["headportrait"]?.ToString());
                ret_json.Add("type", us_json["type"]?.ToString());
                ret_json.Add("time", us_json["time"]?.ToString());

                return SendData(200, data: us_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 用户中心-基本信息
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject Menu_user_info(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data?.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                {
                    return SendData(15001, "未授权访问");
                }
                JObject us_json = UserDAL.GetInfo(us_id);
                JObject car_json = CarDAL.GetInfo(us_id);
                int or_processing_cnt = OrderDAL.GetProcessingCount(us_id);
                int or_all_cnt = OrderDAL.GetAllCount(us_id);
                int or_completed_cnt = OrderDAL.GetCompletedCount(us_id);
                JArray nt_Unprocessed = NoticeDAL.GetUnprocessed(us_id);

                JObject d_us = new JObject();
                d_us.Add("us_id", us_id);
                d_us.Add("tel", us_json["tel"]);
                d_us.Add("name", us_json["name"]);
                d_us.Add("headportrait", us_json["headportrait"]);

                JObject d_car = new JObject();
                d_car.Add("car_name", car_json == null ? null : car_json["car_name"]);
                d_car.Add("car_idcard", car_json == null ? null : car_json["car_idcard"]);

                JObject d_or = new JObject();
                d_or.Add("processing_count", or_processing_cnt);
                d_or.Add("all_count", or_all_cnt);
                d_or.Add("completed_count", or_completed_cnt);

                JObject d_nt = new JObject();
                d_nt.Add("unprocessed_count", nt_Unprocessed.Count);

                JObject ret_json = new JObject();
                ret_json.Add("user", d_us);
                ret_json.Add("car", d_car);
                ret_json.Add("order", d_or);
                ret_json.Add("notice", d_nt);

                return SendData(200, data: ret_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject UpdatePassword(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                string tel = data["tel"]?.ToString();
                string telcode = data["telcode"]?.ToString();
                string password = data["password"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                {
                    return SendData(15001, "未授权访问");
                }
                JObject telcode_json = (JObject)Session["telcode"];
                if (telcode_json["tel"].ToString() != tel ||
                    telcode_json["telcode"].ToString() != telcode)
                    return SendData(12002, "短信验证码错误");
                Session.Remove("telcode");
                bool result = UserDAL.UpdatePassword(us_id, password);
                if (!result)
                    return SendData(-1, "修改失败");
                return SendData(200, "密码修改成功,请使用新密码登录");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 实名认证
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject Verified(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"].ToString();
                string name = data["name"].ToString();
                string idcard = data["idcard"].ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                {
                    return SendData(15001, "未授权访问");
                }
                if (us_id == "" || idcard.Length != 18)
                    return SendData(400, "请求错误");

                if (UserDAL.HasVerified(idcard))
                    return SendData(15010, "身份证号已经被登记");

                bool result = UserDAL.Verified(us_id, name, idcard);
                if (!result)
                    return SendData(-1, "修改失败");
                return SendData(200, "密码修改成功,请使用新密码登录");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
    }
}
