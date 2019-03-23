using API.DAL;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;

namespace API.API
{
    public class CarpoolController : APIController
    {
        /// <summary>
        /// 用户订单发布
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject PublishCustomer(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                string startplace = data["startplace"]?.ToString();
                string endplace = data["endplace"]?.ToString();
                string way = data["way"]?.ToString();
                string starttime = data["starttime"]?.ToString();
                string remarks = data["remarks"]?.ToString();
                string or_type = data["or_type"]?.ToString();
                string paytype = data["paytype"]?.ToString();
                string price = data["price"]?.ToString();


                if (startplace == null ||
                    endplace == null ||
                    starttime == null ||
                    or_type == null
                    )
                    return SendData(400, "请求错误");

                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                bool result = OrderDAL.Publish(
                    us_id,
                    startplace,
                    endplace,
                    starttime,
                    paytype,
                    price,
                    way,
                    or_type,
                    "0",
                    remarks);

                if (!result)
                    return SendData(13001, "订单发布失败");
                return SendData(200, "订单发布成功");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误", e.ToString());
            }
        }
        /// <summary>
        /// 车主发布
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject PublishDriver(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                string startplace = data["startplace"]?.ToString();
                string endplace = data["endplace"]?.ToString();
                string way = data["way"]?.ToString();
                string starttime = data["starttime"]?.ToString();
                string remarks = data["remarks"]?.ToString();
                string or_type = data["or_type"]?.ToString();
                string paytype = data["paytype"]?.ToString();
                string price = data["price"]?.ToString();

                if (startplace == null ||
                    endplace == null ||
                    starttime == null ||
                    or_type == null
                    )
                    return SendData(400, "请求错误");

                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                if (!UserDAL.IsDriver(us_id))
                    return SendData(15003, "未成为司机");

                bool result = OrderDAL.Publish(
                    us_id,
                    startplace,
                    endplace,
                    starttime,
                    paytype,
                    price,
                    way,
                    or_type,
                    "1",
                    remarks);

                if (!result)
                    return SendData(13001, "订单发布失败");
                return SendData(200, "订单发布成功");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误", e.ToString());
            }
        }
        /// <summary>
        /// 搜索列表
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject SearchList(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data?.ToString());
                string startplace = data["startplace"].ToString();
                string endplace = data["endplace"].ToString();
                string identity = data["identity"].ToString();
                string curpage = data["curpage"].ToString();
                string pagecount = data["pagecount"].ToString();
                JObject ret_json = OrderDAL.SearchList(startplace, endplace, identity, int.Parse(curpage), int.Parse(pagecount));

                return SendData(200, data: ret_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
    }
}
