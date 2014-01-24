using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace wvsAccessControlCard
{
    /// <summary>
    /// 解析讀卡資料
    /// </summary>
    public class ACCHistoryParser
    {
        int _FileRecordCount = 0;
        private string _GUID;
        string sourceFileName = @Global._Config.CAP_File;
        string descFileName = "";
        /// <summary>
        /// 取得檔案內紀錄
        /// </summary>
        public List<CardData> getFileRecord()
        {
            List<CardData> retVal = new List<CardData> ();         
            
            try
            {
                if (File.Exists(sourceFileName))
                {
                    // 讀取讀卡資料檔
                    StreamReader sr = new StreamReader(sourceFileName);

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        CardData cd = new CardData(line);
                        retVal.Add(cd);
                    }

                    //// debug 用
                    //StreamWriter sw = new StreamWriter(Application.StartupPath + "\\debug_card.txt", false);
                    //foreach (CardData cd in retVal)
                    //{
                    //    sw.WriteLine(cd.CardNo + "_" + cd.UseType + "_" + cd.Date);
                    //}
                    //sw.Close();

                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                LogAgent.logDB(2, "結束", false, DateTime.Now, "讀取刷卡資料檔失敗："+ex.Message, _GUID);
                Console.WriteLine("getFileRecord():" + ex.Message);
            }

            return retVal;
        }

        /// <summary>
        /// 取得LocalDB紀錄
        /// </summary>
        public Dictionary<string,DataRow> getDBRecords()
        {
            Dictionary<string, DataRow> retVal = new Dictionary<string, DataRow>();
                        
            try
            {
                NpgsqlConnection cn = Global.GetDBConnection();
                if (cn.State == ConnectionState.Open)
                {
                    string query = "select card_no, oclock_name, use_time, use_type, guid, step2_time, step2_session_guid, upload_campus_time, upload_success,step3_session_guid from access_control_card_history;";
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, cn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow dr in dt.Rows)
                    {
                        // card_no+use_type+use_time
                        string key = dr["card_no"].ToString() + "_" + dr["use_type"].ToString() + "_" + DateTime.Parse(dr["use_time"].ToString()).ToString("yyyyMMdd");
                        if (!retVal.ContainsKey(key))
                            retVal.Add(key, dr);
                    }
                    cn.Close();
                }
                
            }
            catch (Exception ex)
            {
                LogAgent.logDB(2, "結束", false, DateTime.Now, "讀取LocalDB失敗："+ex.Message, _GUID);
                Console.WriteLine(ex.Message);
            }
            
            return retVal;
        }

        /// <summary>
        /// 比對資料
        /// </summary>
        public void matchRecords()
        {
            // 取得讀卡資料
            List<CardData> CardDataList = getFileRecord();
            _FileRecordCount = CardDataList.Count;


            // 取得資料庫內資料
            Dictionary<string, DataRow> dbRowDict = getDBRecords();

            // 比對讀卡資料是否在資料庫內
            foreach (CardData cd in CardDataList)
            {
                string key = cd.CardNo + "_" + cd.UseType + "_" + cd.Date;
                if (!dbRowDict.ContainsKey(key))
                    Global._InsertCardDataList.Add(cd);
            }
            bool isOk = false;
            // 將資料寫入LocalDB                        
            List<string> inQList = new List<string>();
            NpgsqlConnection cn = Global.GetDBConnection();
            if (cn.State == ConnectionState.Open)
            {
                NpgsqlTransaction tran = cn.BeginTransaction();
                try
                {
                    NpgsqlCommand cmd = new NpgsqlCommand();
                    cmd.Connection = cn;
                    StringBuilder sbcmd = new StringBuilder();
                    
                    Global._dtUploadData.Clear();

                    foreach (CardData cd in Global._InsertCardDataList)
                    {
                        inQList.Clear();

                        // 給讀卡資料guid
                        string studCardGid = Guid.NewGuid().ToString();

                        inQList.Add(cd.CardNo);
                        inQList.Add(cd.ClockNo);
                        string use_time = cd.Date.Substring(0, 4) + "/" + cd.Date.Substring(4, 2) + "/" + cd.Date.Substring(6, 2) + " " + cd.Time.Substring(0, 2) + ":" + cd.Time.Substring(2, 2) + ":00";
                        inQList.Add(use_time);
                        inQList.Add(cd.UseType);
                        inQList.Add(studCardGid);
                        string strD2 = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        inQList.Add(strD2);
                        inQList.Add(_GUID);
                        string strD3 = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        inQList.Add(strD3);
                        inQList.Add("t");
                        inQList.Add(_GUID);

                        DataRow dr = Global._dtUploadData.NewRow();
                        dr["card_no"] = cd.CardNo;
                        dr["oclock_name"] = cd.ClockNo;
                        dr["use_time"] = use_time;
                        dr["use_type"] = cd.UseType;
                        dr["guid"] = studCardGid;
                        dr["step2_time"] = strD2;
                        dr["step2_session_guid"] = _GUID;
                        Global._dtUploadData.Rows.Add(dr);

                        string strSQL = "insert into access_control_card_history(card_no, oclock_name, use_time, use_type, guid, step2_time, step2_session_guid,upload_campus_time, upload_success, step3_session_guid) values ('" + string.Join("','", inQList.ToArray()) + "');";
                        sbcmd.AppendLine(strSQL);

                    }

                    if (Global._InsertCardDataList.Count > 0)
                    {
                        cmd.CommandText = sbcmd.ToString();
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                    isOk = true;
                }
                catch (Exception ex)
                {
                    LogAgent.logDB(2, "結束", false, DateTime.Now, "比對刷卡資料並寫入LocalDB失敗："+ex.Message, _GUID);
                    tran.Rollback();
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    cn.Close();
                }
            }
            if (isOk)
            {
                try
                {
                    if (File.Exists(sourceFileName))
                    {
                        // 讀完後備份到另一個地方
                        descFileName = @Global._Config.BackupPath + "\\card_no_history" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        File.Move(sourceFileName, descFileName);
                    }
                }
                catch (Exception ex)
                {
                    LogAgent.logDB(2, "結束", false, DateTime.Now, "將刷卡資料檔案搬移失敗："+ex.Message, _GUID);
                    Console.WriteLine("刷卡資料檔搬移失敗:" + ex.Message);
                }

            }
        }

        /// <summary>
        /// 啟動
        /// </summary>
        public void Start(string guid)
        {
            _GUID = guid;
            try
            {
                LogAgent.logDB(2, "開始", true, DateTime.Now, "讀取刷卡資料、解析、並寫入LocalDB。", guid);
                Global._InsertCardDataList.Clear();
                matchRecords();
                LogAgent.logDB(2, "結束", true, DateTime.Now, "讀取刷卡資料檔案內共" + _FileRecordCount + "筆，轉換寫入LocalDB共"+Global._InsertCardDataList.Count+"筆。", guid);
            }
            catch (Exception ex)
            {
                LogAgent.logDB(2, "結束", false, DateTime.Now, "讀取刷卡資料、解析、並寫入LocalDB失敗：" + ex.Message, guid);
                Console.WriteLine("ACCHistoryParser:" + ex.Message);
            }
        }
    }
}
