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
        public static bool Apply(string us_id, string carname, string caridcard, string capacity, string cartype)
        {
            string cmd = @"insert Car(us_id, car_name, car_idcard, car_type, capacity) 
                            values(@us_id, @car_name, @car_idcard, @car_type, @capacity)";
            bool result = DBHelper.Exec(cmd, "@us_id", us_id, "@car_name", carname, "@car_idcard", caridcard, "@car_type", cartype, "@capacity", capacity) > 0;
            if (!result)
                return false;
            string cmd2 = @"update [User] set type=1 where id=@us_id";
            result = DBHelper.Exec(cmd2, "us_id", us_id) > 0;
            return result;
        }
        public static bool Exist(string caridcard)
        {
            string cmd = @"select * from [Car] where car_idcard=@caridcard";
            JArray car_jarr = DBHelper.GetData(cmd, "@caridcard", caridcard);
            if (car_jarr.Count <= 0)
                return false;
            return true;
        }
    }
}