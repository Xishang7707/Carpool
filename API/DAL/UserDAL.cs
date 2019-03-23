using Newtonsoft.Json.Linq;

namespace API.DAL
{
    public class UserDAL
    {
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="tel">手机号码</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        public static bool Register(string tel, string pwd)
        {
            string cmd = @"insert [User](tel, password) values(@tel, @password)";
            int result = DBHelper.Exec(cmd, "@tel", tel, "@password", pwd);
            if (result <= 0)
                return false;
            return true;
        }
        /// <summary>
        /// 用户是否存在
        /// </summary>
        /// <param name="tel">手机号码</param>
        /// <returns></returns>
        public static bool Exist(string tel)
        {
            string cmd = @"select COUNT(*) as cnt from [User] where tel=@tel";
            JArray data = DBHelper.GetData(cmd, "@tel", tel);
            if (data[0]["cnt"].ToString() != "0")
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="tel">手机号码</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static JObject Login(string tel, string password)
        {
            string cmd = @"select * from [User] where tel=@tel and password=@password";
            JArray log_data = DBHelper.GetData(cmd, "@tel", tel, "@password", password);
            if (log_data.Count == 0)
                return null;
            return JObject.FromObject(log_data[0]);
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="id">用户id</param>
        /// <returns></returns>
        public static JObject GetInfo(string id)
        {
            string cmd = @"select * from [User] where id=@id";
            JArray us_jarr = DBHelper.GetData(cmd, "@id", id);
            if (us_jarr.Count == 0)
                return null;

            return JObject.FromObject(us_jarr[0]);
        }
        /// <summary>
        /// 验证是否是司机
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static bool IsDriver(string us_id)
        {
            string cmd = @"select COUNT(*) as cnt from [Car] where us_id=@us_id";
            JArray car_jarr = DBHelper.GetData(cmd, "@us_id", us_id);

            if (int.Parse(car_jarr[0]["cnt"].ToString()) <= 0)
                return false;
            return true;
        }
    }
}