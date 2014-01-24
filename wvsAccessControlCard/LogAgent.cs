using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;

namespace wvsAccessControlCard
{
    public class LogAgent
    {
        /// <summary>
        /// 寫入 Log
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stage"></param>
        /// <param name="is_successful"></param>
        /// <param name="log_time"></param>
        /// <param name="description"></param>
        /// <param name="session_guid"></param>
        public static void logDB(int step, string stage, bool is_successful, DateTime log_time, string description, string session_guid)
        {   
            // INSERT INTO transaction_log(step, stage, is_successful, log_time, description, session_guid) VALUES (?, ?, ?, ?, ?, ?);
            string cnString = "Server=" + Global._Config.DB_ServerName + ";Port=" + Global._Config.DB_Port + ";User Id=" + Global._Config.DB_UserName + ";Password=" + Global._Config.DB_Password + ";Database=" + Global._Config.DB_Name + ";";
            NpgsqlConnection cn = new NpgsqlConnection(cnString);
            cn.Open();
            NpgsqlTransaction tran = cn.BeginTransaction();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = cn;
                string query = "insert into transaction_log(step, stage, is_successful, log_time, description, session_guid) values (" + step + ",'" + stage + "', '" + is_successful + "', '" + log_time.ToString("yyyy/MM/dd HH:mm:ss") + "','"+description+"', '"+session_guid+"');";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                Console.WriteLine("log Error:" + ex.Message);
            }
            finally
            {
                cn.Close();
            }
        }
    }
}
