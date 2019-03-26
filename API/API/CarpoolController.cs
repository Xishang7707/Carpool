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
        /// <summary>
        /// 获取用户所有订单信息
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetUserOrderAll(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                int curpage = int.Parse(data["curpage"].ToString());
                int pagecount = int.Parse(data["pagecount"].ToString());
                JObject od_json = OrderDAL.GetAll(us_id, curpage, pagecount);

                return SendData(200, data: od_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 获取进行中的订单
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetUserOrderProcessing(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                int curpage = int.Parse(data["curpage"].ToString());
                int pagecount = int.Parse(data["pagecount"].ToString());
                JObject od_json = OrderDAL.GetProcessing(us_id, curpage, pagecount);

                return SendData(200, data: od_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 获取已完成的订单
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetUserOrderCompleted(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                int curpage = int.Parse(data["curpage"].ToString());
                int pagecount = int.Parse(data["pagecount"].ToString());
                JObject od_json = OrderDAL.GetCompleted(us_id, curpage, pagecount);

                return SendData(200, data: od_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 订单操作
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject OrderOperation(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                string option = data["option"]?.ToString();
                string or_id = data["or_id"]?.ToString();
                if (option == null || or_id == null)
                    return SendData(400, "请求错误");

                JObject ods_json = null;
                bool result = false;
                switch (option)
                {
                    case "0"://完成订单
                        ods_json = OrderDAL.GetOrderDetailsInfo(or_id, us_id);
                        if (ods_json == null || ods_json["identity"].ToString() != "1")
                            return SendData(400, "请求错误");

                        result = OrderDAL.OrderComplete(or_id, us_id);
                        if (!result)
                            return SendData(20000, "修改失败");
                        else return SendData(200, "成功");
                    case "1"://关闭订单
                             //ods_json = OrderDAL.GetOrderDetailsInfo(or_id, us_id);
                             //if (ods_json == null || ods_json["identity"].ToString() != "1")
                             //    return SendData(400, "请求错误");

                        //result = OrderDAL.OrderComplete(or_id, us_id);
                        //if (!result)
                        //    return SendData(20000, "修改失败");
                        //else return SendData(200, "成功");
                        break;
                    case "2"://同意申请
                        {
                            string ods_id = data["ods_id"]?.ToString();
                            result = OrderDAL.Agree(or_id, ods_id);
                            if (result)
                                return SendData(200);
                            else return SendData(400, "请求错误");
                        }
                        break;
                    case "3"://拒绝申请
                        {
                            string ods_id = data["ods_id"]?.ToString();
                            result = OrderDAL.DisAgree(or_id, ods_id);
                            if (result)
                                return SendData(200);
                            else return SendData(400, "请求错误");
                        }
                        break;
                    default:
                        return SendData(400, "请求错误");
                }
                return SendData(400, "请求错误");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 获取申请的人数
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetApplyCount(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string or_id = data["or_id"]?.ToString();
                if (or_id == null || or_id == "")
                    return SendData(400, "请求错误");
                int applyingcount = OrderDAL.GetApplyingCount(or_id);
                int applyedcount = OrderDAL.GetApplyedCount(or_id);
                JObject ret_json = new JObject();
                ret_json.Add("applyingcount", applyingcount);
                ret_json.Add("applyedcount", applyedcount);

                return SendData(200, data: ret_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetInfo(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string or_id = data["or_id"]?.ToString();
                if (or_id == null || or_id == "")
                    return SendData(400, "请求错误");
                JObject ret_data = OrderDAL.GetInfo(or_id);
                return SendData(200, data: ret_data);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetDetailsInfo(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string or_id = data["or_id"]?.ToString();
                if (or_id == null || or_id == "")
                    return SendData(400, "请求错误");
                JObject od_json = OrderDAL.GetInfo(or_id);
                JObject ret_data = OrderDAL.GetOrderDetailsInfo(or_id, od_json["us_id"].ToString());
                return SendData(200, data: ret_data);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 获取详情页数据
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetOrderDetails(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string or_id = data["or_id"]?.ToString();
                if (or_id == null || or_id == "")
                    return SendData(400, "请求错误");
                JObject od_json = OrderDAL.GetInfo(or_id);
                JObject ods_json = OrderDAL.GetOrderDetailsInfo(or_id, od_json["us_id"].ToString());
                int applyedcount = OrderDAL.GetApplyedCount(or_id);
                int applyingcount = OrderDAL.GetApplyingCount(or_id);
                JObject apply_json = new JObject();
                apply_json.Add("applyingcount", applyingcount);
                apply_json.Add("applyedcount", applyedcount);
                JObject car_json = CarDAL.GetInfo(od_json["us_id"].ToString());
                JObject us_json = UserDAL.GetInfo(od_json["us_id"].ToString());

                JObject rt_us_json = new JObject();
                rt_us_json.Add("tel", us_json["tel"]);
                rt_us_json.Add("name", us_json["name"]);

                JObject ret_data = new JObject();
                ret_data.Add("order", od_json);
                ret_data.Add("orderdetails", ods_json);
                ret_data.Add("applycount", apply_json);
                ret_data.Add("car", car_json);
                ret_data.Add("userinfo", rt_us_json);


                return SendData(200, data: ret_data);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 申请加入
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject ApplyOrder(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string or_id = data["or_id"]?.ToString();
                string us_id = data["CarpoolSSID"]?.ToString();

                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                if (or_id == null || or_id == "")
                    return SendData(400, "请求错误");

                JObject or_json = OrderDAL.GetInfo(or_id);

                if (or_json["us_id"].ToString() == us_id)
                    return SendData(17005, "不能加入到自己的订单中");

                DateTime dt_starttime = DateTime.Parse(or_json["starttime"].ToString());
                if (dt_starttime.CompareTo(DateTime.Now) < 0)
                    return SendData(17001, "拼车已经开始，无法加入");

                if (or_json["or_state"].ToString() != "0")
                    return SendData(17002, "此拼车无效");

                if (OrderDAL.IsApply(or_id, us_id))
                    return SendData(17003, "已经申请/加入");

                bool result = OrderDAL.ApplyOrder(or_id, us_id);

                if (!result)
                    return SendData(20000, "服务错误");

                return SendData(200);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 邀请加入
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject InvateOrder(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string or_id = data["or_id"]?.ToString();
                string us_id = data["CarpoolSSID"]?.ToString();

                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                if (or_id == null || or_id == "")
                    return SendData(400, "请求错误");

                JObject or_json = OrderDAL.GetInfo(or_id);

                if (!UserDAL.IsDriver(us_id))
                    return SendData(15003, "未成为司机");

                if (or_json["us_id"].ToString() == us_id)
                    return SendData(17005, "不能加入到自己的订单中");

                DateTime dt_starttime = DateTime.Parse(or_json["starttime"].ToString());
                if (dt_starttime.CompareTo(DateTime.Now) < 0)
                    return SendData(17001, "开始时间已过，无法加入");

                if (or_json["or_state"].ToString() != "0")
                    return SendData(17002, "此拼车无效");

                if (OrderDAL.IsInvate(or_id, us_id))
                    return SendData(17004, "已经申请/加入");

                bool result = OrderDAL.ApplyOrder(or_id, us_id);

                if (!result)
                    return SendData(20000, "服务错误");

                return SendData(200);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        /// <summary>
        /// 是否已申请/加入
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject IsApplyOrInvate(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"].ToString();
                string or_id = data["or_id"].ToString();
                string option = data["option"].ToString();

                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");
                bool result = false;
                switch (option)
                {
                    case "0"://申请
                        result = OrderDAL.IsApply(or_id, us_id);
                        break;
                    case "1"://邀请
                        result = OrderDAL.IsInvate(or_id, us_id);
                        break;
                    default:
                        return SendData(400, "请求错误");
                }
                return SendData(200, data: result ? 1 : 0);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
    }
}
