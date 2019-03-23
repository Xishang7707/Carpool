using Newtonsoft.Json.Linq;

namespace API.DAL
{
    public class NoticeDAL
    {
        /// <summary>
        /// 获取通知信息
        /// </summary>
        /// <param name="id">通知id</param>
        /// <returns></returns>
        public static JObject GetInfo(string id)
        {
            string cmd = @"select * from [Notice] where id=@id";
            JArray nt_jarr = DBHelper.GetData(cmd, "@id", id);
            return JObject.FromObject(nt_jarr[0]);
        }

        /// <summary>
        /// 获取用户所有通知
        /// </summary>
        /// <param name="recv_id">接收者id</param>
        /// <returns></returns>
        public static JArray GetAll(string recv_id)
        {
            string cmd = @"select * from [Notice] where recv_id=@recv_id order by time desc";
            JArray nt_jarr = DBHelper.GetData(cmd, "@recv_id", recv_id);
            return nt_jarr;
        }
        /// <summary>
        /// 获取用户未处理的通知
        /// </summary>
        /// <param name="recv_id">接收者id</param>
        /// <returns></returns>
        public static JArray GetUnprocessed(string recv_id)
        {
            string cmd = @"select * from [Notice] where recv_id=@recv_id and nt_state=0 order by time desc";
            JArray nt_jarr = DBHelper.GetData(cmd, "@recv_id", recv_id);
            return nt_jarr;
        }
    }
}