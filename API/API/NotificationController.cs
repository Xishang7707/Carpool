using API.DAL;
using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;

namespace API.API
{
    public class NotificationController : APIController
    {
        /// <summary>
        /// 检查通知信息
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject CheckNotify(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());
                string us_id = data["CarpoolSSID"]?.ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");
                JArray nt_jarr = NoticeDAL.GetNewNotify(us_id);

                return SendData(200, data: nt_jarr);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误", e.ToString());
            }
        }
        /// <summary>
        /// 获取所有通知
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject GetNoticeAll(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());

                string us_id = data["CarpoolSSID"].ToString();
                string pagecount = data["pagecount"].ToString();
                string curpage = data["curpage"].ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                JObject nt_json = NoticeDAL.GetAll(us_id, int.Parse(pagecount), int.Parse(curpage));

                return SendData(200, data: nt_json);
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误", e.ToString());
            }
        }
        /// <summary>
        /// 标记阅读通知
        /// </summary>
        /// <param name="in_data"></param>
        /// <returns></returns>
        [HttpPost]
        public JObject NoticeDeal(dynamic in_data)
        {
            try
            {
                JObject data = JObject.Parse(in_data.ToString());

                string us_id = data["CarpoolSSID"].ToString();
                string nt_id = data["nt_id"].ToString();
                if (us_id == null || Session["CarpoolSSID"]?.ToString() != us_id)
                    return SendData(15001, "未授权访问");

                bool result = NoticeDAL.Deal(nt_id);
                if (result)
                    return SendData(200);
                return SendData(20000, "服务错误");
            }
            catch (Exception e)
            {
                return SendData(400, "请求错误");
            }
        }
    }
}
