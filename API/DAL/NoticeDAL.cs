using Newtonsoft.Json.Linq;

namespace API.DAL
{
    /// <summary>
    /// 0 申请
    /// 1 同意申请
    /// 2 拒绝申请
    /// 3 邀请
    /// </summary>
    public class NoticeDAL
    {
        /// <summary>
        /// 获取用户未处理的通知
        /// </summary>
        /// <param name="recv_id">接收者id</param>
        /// <returns></returns>
        public static JArray GetUnprocessed(string recv_id)
        {
            string cmd = @"select * from [Notice] where recv_id=@recv_id and is_deal=0 order by time desc";
            JArray nt_jarr = DBHelper.GetData(cmd, "@recv_id", recv_id);

            return nt_jarr;
        }
        /// <summary>
        /// 获取用户最新的通知
        /// </summary>
        /// <param name="recv_id">接收者id</param>
        /// <returns></returns>
        public static JArray GetNewNotify(string recv_id)
        {
            string cmd = @"select * from [Notice] where recv_id=@recv_id and is_notifyed=0 order by time desc";
            JArray nt_jarr = DBHelper.GetData(cmd, "@recv_id", recv_id);

            string cmd2 = @"update [Notice] set is_notifyed=1 where id in (";

            foreach (JObject item in nt_jarr)
            {
                cmd2 += item["id"].ToString();
                if (nt_jarr.IndexOf(item) + 1 != nt_jarr.Count)
                    cmd2 += ", ";
                else cmd2 += ")";

            }
            if (nt_jarr.Count > 0)
                DBHelper.Exec(cmd2);

            return nt_jarr;
        }

        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="info">
        /// 内容
        /// title:标题
        /// content:内容
        /// </param>
        /// <param name="r_id">接收者</param>
        /// <param name="s_id">发送者</param>
        /// <returns></returns>
        public static bool Notification(string title, string content, string tigger, string r_id, string s_id = "5")
        {
            string cmd = @"insert Notice(send_id, tigger, recv_id, title, data) values(@s_id, @tigger, @r_id, @title, @data)";
            return DBHelper.Exec(cmd, "@s_id", s_id, "@tigger", tigger, "@r_id", r_id, "@title", title, "@data", content) > 0;
        }
        /// <summary>
        /// 已查看消息
        /// </summary>
        /// <param name="nt_id"></param>
        /// <returns></returns>
        public static bool Deal(string nt_id)
        {
            string cmd = @"update [Notice] set is_deal=1 where id=@nt_id";
            return DBHelper.Exec(cmd, "@nt_id", nt_id) > 0;
        }
        /// <summary>
        /// 获取接收者所有消息
        /// </summary>
        /// <param name="r_id">接收者id</param>
        /// <param name="pagecount">一页数量</param>
        /// <param name="curpage">获取的页码</param>
        /// <returns></returns>
        public static JObject GetAll(string r_id, int pagecount, int curpage)
        {
            string cmd = @"select * from [Notice] where recv_id=@r_id order by time desc ";
            string offset = @" offset @passcount ROWS fetch next @pagecount ROWS only";
            int passcount = (curpage - 1) * pagecount;
            cmd += offset;
            JArray nt_jarr = DBHelper.GetData(cmd, "@r_id", r_id, "@passcount", passcount, "@pagecount", pagecount);
            
            int count = GetAllCount(r_id);
            int pages = count / pagecount + (count % pagecount > 0 ? 1 : 0);
            JObject ret_json = new JObject();
            ret_json.Add("data", nt_jarr);
            ret_json.Add("pages", pages);

            return ret_json;
        }
        /// <summary>
        /// 获取所有消息的数量
        /// </summary>
        /// <param name="r_id">接收者</param>
        /// <returns></returns>
        public static int GetAllCount(string r_id)
        {
            string cmd = @"select COUNT(*) as cnt from [Notice] where recv_id=@r_id";
            JArray nt_jarr = DBHelper.GetData(cmd, "@r_id", r_id);
            return int.Parse(nt_jarr[0]["cnt"].ToString());
        }
    }
}