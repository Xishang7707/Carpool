using Newtonsoft.Json.Linq;
using System;

namespace API.DAL
{
    public class OrderDAL
    {
        /// <summary>
        /// 发布用户拼车
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <param name="startplace">起点</param>
        /// <param name="endplace">终点</param>
        /// <param name="starttime">开始时间</param>
        /// <param name="paytype">支付方式</param>
        /// <param name="price">价格</param>
        /// <param name="way">途径</param>
        /// <param name="or_type">订单类型</param>
        /// <param name="identity">发布者身份</param>
        /// <returns></returns>
        public static bool Publish(string us_id, string startplace, string endplace, string starttime, string paytype, string price, string way, string or_type, string identity, string remarks)
        {
            string cmd = @"insert [Order](us_id, startplace, endplace, way, starttime, or_type, [identity], remarks, or_state)
                            values(@us_id, @startplace, @endplace, @way, @starttime, @or_type, @identity, @remarks, @or_state)";
            int result = DBHelper.Exec(cmd,
                "@us_id", us_id,
                "@startplace", startplace,
                "@endplace", endplace,
                "@way", way == null ? DBNull.Value.ToString() : way,
                "@starttime", starttime,
                "@or_type", or_type,
                "@identity", identity,//身份
                "@remarks", remarks,
                "@or_state", 0
                );
            if (result <= 0)
                return false;
            string cmd2 = @"select TOP(1) * from [Order] where us_id=@us_id and startplace=@startplace and endplace=@endplace order by time desc";
            JArray or_json = DBHelper.GetData(cmd2, "@us_id", us_id, "@startplace", startplace, "@endplace", endplace);
            string or_id = or_json[0]["or_id"].ToString();

            string cmd3 = @"insert [OrderDetails](or_id, us_id, paytype, price, or_state, [identity])
                            values(@or_id, @us_id, @paytype, @price, @or_state, @identity)";
            result = DBHelper.Exec(cmd3,
                "@or_id", or_id,
                "@us_id", us_id,
                "@paytype", paytype,
                "@price", (price == null || price == "") ? "0" : price,
                "@or_state", 0,
                "@identity", identity  //身份
                );
            return result > 0;
        }
        /// <summary>
        /// 搜索列表
        /// </summary>
        /// <param name="startplace">起点</param>
        /// <param name="endplace">终点</param>
        /// <param name="identity">
        /// 0.乘客
        /// 1.司机
        /// </param>
        /// <param name="curpage">页码</param>
        /// <param name="pagecount">数据量</param>
        /// <returns></returns>
        public static JObject SearchList(string startplace, string endplace, string identity, int curpage, int pagecount)
        {
            string or_cmd = @"select * from [Order] od left join [OrderDetails] ods on od.or_id=ods.or_id and od.us_id=ods.us_id where od.[identity]=@identity and od.or_state=0 ";
            string cnt_cmd = @"select COUNT(*) as cnt from [Order] od left join [OrderDetails] ods on od.or_id=ods.or_id and od.us_id=ods.us_id where od.[identity]=@identity and od.or_state=0 ";
            string order = @" order by od.or_state, od.time desc ";
            string offset = @" offset @passcount ROWS fetch next @pagecount ROWS only";
            int passcount = (curpage - 1) * pagecount;
            if (startplace != null)
            {
                or_cmd += @" and startplace like @startplace ";
                cnt_cmd += @" and startplace like @startplace ";
            }
            if (endplace != null)
            {
                or_cmd += @" and endplace like @endplace ";
                cnt_cmd += @" and endplace like @endplace ";
            }
            or_cmd += order;
            or_cmd += offset;
            JArray search_jarr = DBHelper.GetData(or_cmd,
                "@identity", identity,
                "@startplace", "%" + startplace + "%",
                "@endplace", "%" + endplace + "%",
                "@passcount", passcount,
                "@pagecount", pagecount);

            JArray search_cnt = DBHelper.GetData(cnt_cmd,
                "@identity", identity,
                "@startplace", "%" + startplace + "%",
                "@endplace", "%" + endplace + "%");

            int count = int.Parse(search_cnt[0]["cnt"].ToString());

            int pages = count / pagecount + (count % pagecount > 0 ? 1 : 0);

            JObject ret_json = new JObject();
            ret_json.Add("data", search_jarr);
            ret_json.Add("pages", pages);

            return ret_json;
        }
        /// <summary>
        /// 获取进行中的订单
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static JArray GetProcessing(string us_id)
        {
            string cmd = @"select * from [Order] od, [OrderDetails] ods where od.or_id=ods.or_id and (od.us_id=@us_id or ods.us_id=@us_id) and ods.or_state=0 order by ods.time desc";
            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id);
            return or_jarr;
        }
        ///0 进行中
        ///1 完成
        ///2 被关闭
        ///3 关闭
        /// <summary>
        /// 获取所有订单
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static JArray GetAll(string us_id)
        {
            string cmd = @"select * from [Order] od, [OrderDetails] ods where od.or_id=ods.or_id and (od.us_id=@us_id or ods.us_id=@us_id) order by ods.or_state, ods.time desc";
            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id);
            return or_jarr;
        }
        /// <summary>
        /// 获取已完成订单
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static JArray GetCompleted(string us_id)
        {
            string cmd = @"select * from [Order] od, [OrderDetails] ods where od.or_id=ods.or_id and (od.us_id=@us_id or ods.us_id=@us_id) and ods.or_state=1 order by ods.time desc";
            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id);
            return or_jarr;
        }
    }
}