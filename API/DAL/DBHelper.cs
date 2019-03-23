using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;

namespace API.DAL
{
    class DBHelper
    {
        static string conStr = "server=.;database=Carpool;uid=sa;pwd=123456";
        static SqlConnection conn = new SqlConnection(conStr);


        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        private static void InitConnection()
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }
        /// <summary>
        /// 执行数据库语句
        /// </summary>
        /// <param name="cmd">数据库语句</param>
        /// <param name="values">参数化值</param>
        /// <returns></returns>
        public static int Exec(string cmd, params dynamic[] values)
        {
            InitConnection();
            SqlCommand sqlCmd = new SqlCommand(cmd, conn);
            for (int i = 0; i < values.Length; i += 2)
            {
                sqlCmd.Parameters.AddWithValue(values[i].ToString(), values[i + 1]);
            }

            int result = sqlCmd.ExecuteNonQuery();
            conn.Close();
            return result;
        }
        /// <summary>
        /// 获取表数据
        /// </summary>
        /// <param name="cmd">数据库语句</param>
        /// <returns>表数据</returns>
        public static JArray GetData(string cmd, params dynamic[] values)
        {
            InitConnection();
            SqlCommand sqlCmd = new SqlCommand(cmd, conn);
            for (int i = 0; i < values.Length; i += 2)
            {
                sqlCmd.Parameters.AddWithValue(values[i].ToString(), values[i + 1]);
            }

            SqlDataAdapter sda = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            conn.Close();

            return JArray.FromObject(dt);
        }
    }
}
