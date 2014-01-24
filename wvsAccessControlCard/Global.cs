using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using FISCA.DSAClient;
using System.Data;
using System.Xml.Linq;

namespace wvsAccessControlCard
{
    public class Global
    {  
        /// <summary>
        /// 設定檔
        /// </summary>
        public static ConfigManager _Config;

        /// <summary>
        /// 新增LocalDB資料
        /// </summary>
        public static List<CardData> _InsertCardDataList = new List<CardData>();

        /// <summary>
        /// 取得Local資料庫連線
        /// </summary>
        /// <returns></returns>
        public static NpgsqlConnection GetDBConnection()
        {
            string cnString = "Server=" + Global._Config.DB_ServerName + ";Port=" + Global._Config.DB_Port + ";User Id=" + Global._Config.DB_UserName + ";Password=" + Global._Config.DB_Password + ";Database=" + Global._Config.DB_Name + ";";
            NpgsqlConnection cn = new NpgsqlConnection(cnString);
            cn.Open();
            return cn;
        }

        /// <summary>
        /// 取得 DSA 連線
        /// </summary>
        /// <returns></returns>
        public static Connection GetDSAConnection()
        {
            Connection cn = new Connection();
            cn.Connect(Global._Config.DSA_AccessPoint, Global._Config.DSA_ContractName, Global._Config.DSA_UserName, Global._Config.DSA_Password);
            return cn;
        }

        public static DataTable _dtUploadData = new DataTable();

       /// <summary>
        /// 傳送資料至 DSA
       /// </summary>
       /// <param name="step"></param>
       /// <param name="cn"></param>
       /// <param name="ServiceName"></param>
       /// <param name="ReqElement"></param>
       /// <param name="guid"></param>
        public static XElement SendRquest(int step,Connection cn, string ServiceName, XElement ReqElement,string guid)
        {
            XElement elm = null;
            Envelope rsp = null;
            try
            {
                XmlHelper req = new XmlHelper(ReqElement.ToString());

                rsp = cn.SendRequest(ServiceName, new Envelope(req));

                if (rsp.Body.XmlString != "")
                {
                    XmlHelper xmlrsp = new XmlHelper(rsp.Body);
                    elm = XElement.Parse(xmlrsp.XmlString);
                }             
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Request error:" + ex.Message);
                LogAgent.logDB(step, "結束", false, DateTime.Now, ex.Message, guid);
            }
            return elm;
        }
        
    }
}
