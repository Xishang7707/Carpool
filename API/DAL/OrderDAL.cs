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
            CheckAvailability();
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
            CheckAvailability();
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
        public static JObject GetProcessing(string us_id, int curpage, int pagecount)
        {
            CheckAvailability();
            string cmd = @"select od.* from [Order] od, [OrderDetails] ods where od.or_id=ods.or_id and (ods.us_id=@us_id) and (ods.or_state=0 or ods.or_state=1) order by ods.or_state, ods.time desc";
            string offset = @" offset @passcount ROWS fetch next @pagecount ROWS only";
            int passcount = (curpage - 1) * pagecount;

            cmd += offset;

            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id, "@passcount", passcount, "@pagecount", pagecount);

            string cmd_ods = @"select * from [OrderDetails] ods where or_id=@or_id and (or_state=0 or or_state=1 or or_state=15) order by [identity] desc";
            JArray search_jarr = new JArray();

            foreach (JObject item in or_jarr)
            {
                JObject json = new JObject();
                json.Add("order", item);
                //json.Add("user", UserDAL.GetInfo(item["us_id"].ToString()));
                JObject ods_json = OrderDAL.GetOrderDetailsInfo(item["or_id"].ToString(), item["us_id"].ToString());
                json.Add("orderdetail", ods_json);
                JObject owner = OrderDAL.GetOrderDetailsInfo(item["or_id"].ToString(), us_id);
                json.Add("owner", owner);
                JObject carus_tmp = OrderDAL.GetOrderCar(item["or_id"].ToString());
                JObject carus = new JObject();
                if (carus_tmp != null)
                {
                    carus.Add("id", carus_tmp["id"]);
                    carus.Add("tel", carus_tmp["tel"]);
                    carus.Add("name", carus_tmp["name"]);
                    //JObject carowner_json = 
                }
                json.Add("carus", carus_tmp == null ? null : carus);
                JArray ord_jarr = DBHelper.GetData(cmd_ods, "@or_id", item["or_id"].ToString());
                JArray info_jarr = new JArray();
                //if (ods_json["identity"].ToString() == "1" && ods_json["us_id"].ToString() == us_id)
                {
                    foreach (JObject tmp in ord_jarr)
                    {
                        if (tmp["us_id"].ToString() == us_id)
                            continue;
                        if (owner["identity"].ToString() == "0" && tmp["identity"].ToString() == "0")
                            continue;

                        JObject us_json = UserDAL.GetInfo(tmp["us_id"].ToString());
                        us_json.Remove("password");
                        us_json.Remove("idcard");
                        us_json.Remove("type");
                        us_json.Remove("time");

                        JObject info_json = new JObject();
                        info_json.Add("user", us_json);
                        info_json.Add("orderdetail", tmp);

                        info_jarr.Add(info_json);
                    }
                }
                json.Add("info", info_jarr);


                search_jarr.Add(json);
            }
            int count = GetProcessingCount(us_id);
            int pages = count / pagecount + (count % pagecount > 0 ? 1 : 0);
            JObject ret_json = new JObject();
            ret_json.Add("data", search_jarr);
            ret_json.Add("pages", pages);

            return ret_json;
        }
        /// <summary>
        /// 获取进行中的订单数量
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static int GetProcessingCount(string us_id)
        {
            CheckAvailability();
            string cmd = @"select COUNT(*) as cnt from [OrderDetails] ods where ods.us_id=@us_id and (ods.or_state=0 or ods.or_state=1)";
            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id);

            return int.Parse(or_jarr[0]["cnt"].ToString());
        }
        ///0 进行中
        ///10 申请中
        ///11 被拒绝
        ///15 邀请中
        ///20 完成
        ///30 被关闭
        ///31 关闭
        ///32 拒绝申请
        ///40 过期
        /// <summary>
        /// 获取所有订单
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static JObject GetAll(string us_id, int curpage, int pagecount)
        {
            CheckAvailability();
            string cmd = @"select od.* from [Order] od, [OrderDetails] ods where od.or_id=ods.or_id and (ods.us_id=@us_id) order by ods.or_state, ods.time desc";
            string offset = @" offset @passcount ROWS fetch next @pagecount ROWS only";
            int passcount = (curpage - 1) * pagecount;

            cmd += offset;

            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id, "@passcount", passcount, "@pagecount", pagecount);

            string cmd_ods = @"select * from [OrderDetails] ods where or_id=@or_id and (or_state=0 or or_state=1 or or_state=10 or or_state=15) order by [identity] desc";
            JArray search_jarr = new JArray();

            foreach (JObject item in or_jarr)
            {
                JObject json = new JObject();
                json.Add("order", item);
                //json.Add("user", UserDAL.GetInfo(item["us_id"].ToString()));
                JObject ods_json = OrderDAL.GetOrderDetailsInfo(item["or_id"].ToString(), item["us_id"].ToString());
                json.Add("orderdetail", ods_json);
                JObject owner = OrderDAL.GetOrderDetailsInfo(item["or_id"].ToString(), us_id);
                json.Add("owner", owner);
                JObject carus_tmp = OrderDAL.GetOrderCar(item["or_id"].ToString());
                JObject carus = new JObject();
                if (carus_tmp != null)
                {
                    carus.Add("id", carus_tmp["id"]);
                    carus.Add("tel", carus_tmp["tel"]);
                    carus.Add("name", carus_tmp["name"]);
                    //JObject carowner_json = 
                }
                json.Add("carus", carus_tmp == null ? null : carus);
                JArray ord_jarr = DBHelper.GetData(cmd_ods, "@or_id", item["or_id"].ToString());
                JArray info_jarr = new JArray();
                //乘客列表
                //ods_json["identity"].ToString()
                //if (ods_json["us_id"].ToString() == us_id)
                {

                    foreach (JObject tmp in ord_jarr)
                    {
                        if (tmp["us_id"].ToString() == us_id)
                            continue;
                        if (owner["identity"].ToString() == "0" && tmp["identity"].ToString() == "0")
                            continue;

                        JObject us_json = UserDAL.GetInfo(tmp["us_id"].ToString());
                        us_json.Remove("password");
                        us_json.Remove("idcard");
                        us_json.Remove("type");
                        us_json.Remove("time");

                        JObject info_json = new JObject();
                        info_json.Add("user", us_json);
                        info_json.Add("orderdetail", tmp);

                        info_jarr.Add(info_json);
                    }
                }
                json.Add("info", info_jarr);


                search_jarr.Add(json);
            }
            int count = GetAllCount(us_id);
            int pages = count / pagecount + (count % pagecount > 0 ? 1 : 0);
            JObject ret_json = new JObject();
            ret_json.Add("data", search_jarr);
            ret_json.Add("pages", pages);

            return ret_json;
        }
        /// <summary>
        /// 获取所有订单的数量
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static int GetAllCount(string us_id)
        {
            CheckAvailability();
            string cmd = @"select COUNT(*) as cnt from [Order] od, [OrderDetails] ods where od.or_id=ods.or_id and (ods.us_id=@us_id)";
            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id);

            return int.Parse(or_jarr[0]["cnt"].ToString());
        }
        /// <summary>
        /// 获取已完成订单
        /// </summary>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static JObject GetCompleted(string us_id, int curpage, int pagecount)
        {
            CheckAvailability();
            string cmd = @"select od.* from [Order] od, [OrderDetails] ods where od.or_id=ods.or_id and (ods.us_id=@us_id) and (ods.or_state=20) order by ods.or_state, ods.time desc";
            string offset = @" offset @passcount ROWS fetch next @pagecount ROWS only";
            int passcount = (curpage - 1) * pagecount;

            cmd += offset;

            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id, "@passcount", passcount, "@pagecount", pagecount);

            string cmd_ods = @"select * from [OrderDetails] ods where or_id=@or_id and (or_state=0 or or_state=1 or or_state=20) order by [identity] desc";
            JArray search_jarr = new JArray();

            foreach (JObject item in or_jarr)
            {
                JObject json = new JObject();
                json.Add("order", item);
                //json.Add("user", UserDAL.GetInfo(item["us_id"].ToString()));
                JObject ods_json = OrderDAL.GetOrderDetailsInfo(item["or_id"].ToString(), item["us_id"].ToString());
                json.Add("orderdetail", ods_json);
                JObject owner = OrderDAL.GetOrderDetailsInfo(item["or_id"].ToString(), us_id);
                json.Add("owner", owner);
                JObject carus_tmp = OrderDAL.GetOrderCar(item["or_id"].ToString());
                JObject carus = new JObject();
                if (carus_tmp != null)
                {
                    carus.Add("id", carus_tmp["id"]);
                    carus.Add("tel", carus_tmp["tel"]);
                    carus.Add("name", carus_tmp["name"]);
                    //JObject carowner_json = 
                }
                json.Add("carus", carus_tmp == null ? null : carus);
                JArray ord_jarr = DBHelper.GetData(cmd_ods, "@or_id", item["or_id"].ToString());
                JArray info_jarr = new JArray();
                //if (ods_json["identity"].ToString() == "1" && ods_json["us_id"].ToString() == us_id)
                {
                    foreach (JObject tmp in ord_jarr)
                    {
                        if (tmp["us_id"].ToString() == us_id)
                            continue;
                        if (owner["identity"].ToString() == "0" && tmp["identity"].ToString() == "0")
                            continue;

                        JObject us_json = UserDAL.GetInfo(tmp["us_id"].ToString());
                        us_json.Remove("password");
                        us_json.Remove("idcard");
                        us_json.Remove("type");
                        us_json.Remove("time");

                        JObject info_json = new JObject();
                        info_json.Add("user", us_json);
                        info_json.Add("orderdetail", tmp);

                        info_jarr.Add(info_json);
                    }
                }
                json.Add("info", info_jarr);


                search_jarr.Add(json);
            }
            int count = GetCompletedCount(us_id);
            int pages = count / pagecount + (count % pagecount > 0 ? 1 : 0);
            JObject ret_json = new JObject();
            ret_json.Add("data", search_jarr);
            ret_json.Add("pages", pages);

            return ret_json;
        }
        /// <summary>
        /// 获取已完成订单的数量
        /// </summary>
        /// <param name="us_id"></param>
        /// <returns></returns>
        public static int GetCompletedCount(string us_id)
        {
            CheckAvailability();
            string cmd = @"select COUNT(*) as cnt from [Order] od, [OrderDetails] ods where od.or_id=ods.or_id and (ods.us_id=@us_id) and ods.or_state=20";
            JArray or_jarr = DBHelper.GetData(cmd, "@us_id", us_id);
            return int.Parse(or_jarr[0]["cnt"].ToString());
        }
        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="or_id">订单id</param>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static JObject GetOrderDetailsInfo(string or_id, string us_id)
        {
            CheckAvailability();
            string cmd = @"select * from [OrderDetails] where or_id=@or_id and us_id=@us_id";
            JArray ods_jarr = DBHelper.GetData(cmd, "@or_id", or_id, "@us_id", us_id);
            if (ods_jarr.Count == 0)
                return null;
            return JObject.FromObject(ods_jarr[0]);
        }

        public static JObject GetOrderDetailsInfo(string ods_id)
        {
            CheckAvailability();
            string cmd = @"select * from [OrderDetails] where ods_id=@ods_id";
            JArray ods_jarr = DBHelper.GetData(cmd, "@ods_id", ods_id);
            if (ods_jarr.Count == 0)
                return null;
            return JObject.FromObject(ods_jarr[0]);
        }
        /// <summary>
        /// 完成订单
        /// </summary>
        /// <param name="or_id">订单id</param>
        /// <returns></returns>
        public static bool OrderComplete(string or_id, string us_id)
        {
            CheckAvailability();
            string cmd = @"update [Order] set or_state=20 where or_state=0 and or_id=@or_id";
            bool result = DBHelper.Exec(cmd, "@or_id", or_id) > 0;
            if (!result)
                return false;
            string cmd2 = @"update [OrderDetails] set or_state=20 where or_state=0 and or_id=@or_id";
            DBHelper.Exec(cmd2, "@or_id", or_id);
            string cmd3 = @"update [OrderDetails] set or_state=40 where or_state=10 and or_id=@or_id";
            DBHelper.Exec(cmd3, "@or_id", or_id);
            return true;
        }
        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="or_id">订单id</param>
        /// <param name="us_id">用户id</param>
        /// <returns></returns>
        public static bool OrderClose(string or_id, string us_id)
        {
            JObject or_json = OrderDAL.GetInfo(or_id);
            string cmd;
            int flag = -1;
            if (or_json["us_id"].ToString() == us_id)
            {
                cmd = @"update [OrderDetails] set or_state=30 where or_id=@or_id";
                flag = 0;
            }
            else
            {
                cmd = @"update [OrderDetails] set or_state=30 where or_id=@or_id and us_id=@us_id";
                flag = 1;
            }
            bool result = DBHelper.Exec(cmd, "@or_id", or_id) > 0;
            if (flag == 0)
            {
                string cmd2 = @"update [Order] set or_state=30 where or_id=@or_id and us_id=@us_id";
                return DBHelper.Exec(cmd2, "@or_id", or_id, "@us_id", us_id) > 0;
            }
            return false;
        }
        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="or_id"></param>
        /// <returns></returns>
        public static JObject GetInfo(string or_id)
        {
            CheckAvailability();
            string cmd = @"select * from [Order] where or_id=@or_id";
            JArray or_jarr = DBHelper.GetData(cmd, "@or_id", or_id);
            if (or_jarr.Count <= 0)
                return null;
            return JObject.FromObject(or_jarr[0]);
        }
        /// <summary>
        /// 获取申请中的人数
        /// </summary>
        /// <param name="or_id"></param>
        /// <returns></returns>
        public static int GetApplyingCount(string or_id)
        {
            CheckAvailability();
            string cmd = @"select COUNT(*) as cnt from [OrderDetails] where or_id=@or_id and or_state=10";
            JArray or_jarr = DBHelper.GetData(cmd, "@or_id", or_id);
            return int.Parse(or_jarr[0]["cnt"].ToString());
        }
        /// <summary>
        /// 获取已加入的人数
        /// </summary>
        /// <param name="or_id"></param>
        /// <returns></returns>
        public static int GetApplyedCount(string or_id)
        {
            CheckAvailability();
            string cmd = @"select COUNT(*) as cnt from [OrderDetails] where or_id=@or_id and or_state=0";
            JArray or_jarr = DBHelper.GetData(cmd, "@or_id", or_id);
            return int.Parse(or_jarr[0]["cnt"].ToString()) - 1;
        }
        /// <summary>
        /// 作为乘客加入车主的拼车
        /// </summary>
        /// <param name="or_id">司机订单id</param>
        /// <param name="us_id">乘客用户id</param>
        /// <returns></returns>
        public static bool ApplyOrder(string or_id, string us_id)
        {
            CheckAvailability();
            JObject or_json = OrderDAL.GetInfo(or_id);
            JObject ods_json = OrderDAL.GetOrderDetailsInfo(or_id, or_json["us_id"].ToString());
            if (or_json == null)
                return false;

            string cmd = @"insert [OrderDetails](or_id, us_id, price, or_state, [identity]) 
                            values(@or_id, @us_id, @price, @or_state, @identity)";
            bool result = DBHelper.Exec(cmd, "@or_id", or_id, "@us_id", us_id, "@or_state", 10, "@price", ods_json["price"].ToString(), "@identity", "0") > 0;
            return result;
        }
        /// <summary>
        /// 作为司机邀请乘客
        /// </summary>
        /// <param name="or_id">乘客订单id</param>
        /// <param name="us_id">司机用户id</param>
        /// <returns></returns>
        public static bool InviteOrder(string or_id, string us_id)
        {
            CheckAvailability();
            JObject or_json = OrderDAL.GetInfo(or_id);
            JObject ods_json = OrderDAL.GetOrderDetailsInfo(or_id, or_json["us_id"].ToString());
            if (or_json == null)
                return false;

            string cmd = @"insert [OrderDetails](or_id, us_id, price, or_state, [identity]) 
                            values(@or_id, @us_id, @price, @or_state, @identity)";
            bool result = DBHelper.Exec(cmd, "@or_id", or_id, "@us_id", us_id, "@or_state", 15, "@price", ods_json["price"].ToString(), "@identity", "1") > 0;
            return result;
        }
        /// <summary>
        /// 是否已经邀请/或已经加入
        /// </summary>
        /// <param name="or_id"></param>
        /// <param name="us_id"></param>
        /// <returns></returns>
        public static bool IsInvate(string or_id, string us_id)
        {
            CheckAvailability();
            string cmd = @"select * from [OrderDetails] where [identity]=1 and (or_state=15 or or_state=0) and us_id=@us_id and or_id=@or_id";
            JArray ods_jarr = DBHelper.GetData(cmd, "@us_id", us_id, "@or_id", or_id);
            if (ods_jarr.Count > 0)
                return true;
            return false;
        }
        /// <summary>
        /// 是否已经申请/或已经加入
        /// </summary>
        /// <param name="or_id"></param>
        /// <param name="us_id"></param>
        /// <returns></returns>
        public static bool IsApply(string or_id, string us_id)
        {
            CheckAvailability();
            string cmd = @"select * from [OrderDetails] where or_id=@or_id and (or_state=10 or or_state=0) and us_id=@us_id";
            JArray ods_jarr = DBHelper.GetData(cmd, "@us_id", us_id, "@or_id", or_id);
            if (ods_jarr.Count > 0)
                return true;
            return false;
        }
        ///0 进行中
        ///1 已经开始
        ///10 申请中
        ///11 被拒绝
        ///15 邀请中
        ///20 完成
        ///30 被关闭
        ///31 关闭
        ///32 拒绝申请
        ///33 拒绝邀请
        ///40 过期
        ///
        /// <summary>
        /// 更新状态
        /// </summary>
        public static void CheckAvailability()
        {
            try
            {
                //正在进行的
                string od_cmd_2 = @"update [OrderDetails] set or_state=1 where (or_state=0) and or_id in 
                                    (select or_id from [Order] where (or_state=0) and DATEDIFF(MINUTE, getdate(), starttime)<0)";
                DBHelper.Exec(od_cmd_2);
                string od_cmd_2_1 = @"update [Order] set or_state=1 where (or_state=0) and DATEDIFF(MINUTE, getdate(), starttime)<0";
                DBHelper.Exec(od_cmd_2_1);

                //已经开始后,正在申请/邀请的
                string od_cmd_1 = @"update [OrderDetails] set or_state=40 where (or_state=10 or or_state=15) and or_id in 
                                    (select or_id from [Order] where or_state=1)";
                DBHelper.Exec(od_cmd_1);
            }
            catch (Exception e)
            {

            }
        }
        /// <summary>
        /// 获取订单司机信息
        /// </summary>
        /// <param name="or_id"></param>
        /// <returns></returns>
        public static JObject GetOrderCar(string or_id)
        {
            CheckAvailability();
            string cmd = @"select * from [OrderDetails] where or_id=@or_id and [identity]=1";
            JArray car_jarr = DBHelper.GetData(cmd, "@or_id", or_id);
            if (car_jarr.Count <= 0)
                return null;
            JObject car_json = JObject.FromObject(car_jarr[0]);
            JObject carus = UserDAL.GetInfo(car_json["us_id"].ToString());
            return carus;
        }
        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="or_id"></param>
        /// <returns></returns>
        public static bool CloseOrder(string or_id)
        {
            return false;
        }

        /// <summary>
        /// 同意申请
        /// </summary>
        /// <param name="or_id">订单id</param>
        /// <param name="us_id">同意的用户id</param>
        /// <returns></returns>
        public static bool Agree(string or_id, string ods_id)
        {
            CheckAvailability();
            string cmd = @"update [OrderDetails] set or_state=0 where or_id=@or_id and ods_id=@ods_id and or_state=10";
            bool result = DBHelper.Exec(cmd, "@ods_id", ods_id, "@or_id", or_id) > 0;
            if (result)
                return true;
            return false;
        }
        /// <summary>
        /// 不同意申请
        /// </summary>
        /// <param name="or_id"></param>
        /// <param name="us_id"></param>
        /// <returns></returns>
        public static bool DisAgree(string or_id, string ods_id)
        {
            CheckAvailability();
            string cmd = @"update [OrderDetails] set or_state=32 where or_id=@or_id and ods_id=@ods_id and or_state=10";
            bool result = DBHelper.Exec(cmd, "@ods_id", ods_id, "@or_id", or_id) > 0;
            if (result)
                return true;
            return false;
        }
        /// <summary>
        /// 同意邀请
        /// </summary>
        /// <param name="ods_id">订单id</param>
        /// <returns></returns>
        public static bool AcceptApply(string ods_id)
        {
            CheckAvailability();
            string cmd = @"update [OrderDetails] set or_state=0 where ods_id=@ods_id and or_state=15";
            bool result = DBHelper.Exec(cmd, "@ods_id", ods_id) > 0;
            return result;
        }
        /// <summary>
        /// 拒绝邀请
        /// </summary>
        /// <param name="ods_id">订单id</param>
        /// <returns></returns>
        public static bool DisAcceptApply(string ods_id)
        {
            CheckAvailability();
            string cmd = @"update [OrderDetails] set or_state=33 where ods_id=@ods_id and or_state=15";
            bool result = DBHelper.Exec(cmd, "@ods_id", ods_id) > 0;
            return result;
        }
        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="ods_id"></param>
        /// <returns></returns>
        public static bool Payfor(string ods_id)
        {
            CheckAvailability();
            string cmd = @"update [OrderDetails] set ispay=1 where ods_id=@ods_id";
            bool result = DBHelper.Exec(cmd, "@ods_id", ods_id) > 0;
            return result;
        }
    }
}