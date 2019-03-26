using API.DAL;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;

namespace API.API
{
    public class CarsController : APIController
    {
        /// <summary>
        /// 获取车辆信息
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetInfo(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string id = data["id"].ToString();
                string option = data["option"].ToString();
                if (id == "" || option == "")
                    return SendData(400, "请求错误");

                JObject car_json = null;
                switch (option)
                {
                    case "0"://用户id
                        car_json = CarDAL.GetInfo(id);
                        break;
                    case "1"://订单id
                        JObject or_json = OrderDAL.GetInfo(id);
                        car_json = CarDAL.GetInfo(or_json["us_id"].ToString());
                        break;
                    default:
                        return SendData(400, "请求错误");
                }
                return SendData(200, data: car_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
        [HttpPost]
        public JObject Apply(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string carname = data["carname"].ToString();
                string caridcard = data["caridcard"].ToString();
                string capacity = data["capacity"].ToString();
                string cartype = data["cartype"].ToString();

                string us_id = data["CarpoolSSID"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"].ToString() != us_id)
                {
                    return SendData(15001, "未授权访问");
                }

                if (CarDAL.Exist(caridcard))
                    return SendData(17001, "车牌号码已被注册");

                if (!CarDAL.Apply(us_id, carname, caridcard, capacity, cartype))
                    return SendData(400, "请求错误");

                return SendData(200);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
    }
}
