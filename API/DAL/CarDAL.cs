using Newtonsoft.Json.Linq;

namespace API.DAL
{
    public class CarDAL
    {
        /// <summary>
        /// 获取车辆信息
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static JObject GetInfo(string us_id)
        {
            string cmd = @"select * from [Car] where us_id=@us_id";
            JArray car_jarr = DBHelper.GetData(cmd, "@us_id", us_id);

            if (car_jarr.Count <= 0)
                return null;
            return JObject.FromObject(car_jarr[0]);
        }
    }
}