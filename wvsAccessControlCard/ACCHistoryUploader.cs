using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Npgsql;
using FISCA.DSAClient;
using System.Xml.Linq;

namespace wvsAccessControlCard
{
    public class ACCHistoryUploader
    {
        private string _GUID = "";
        string UploadCardNoHistoryServiceName = "service.UploadCardNoHistory";

        /// <summary>
        /// 傳送資料
        /// </summary>
        public void uploadRecord()
        {
            #region 將讀卡資料上傳至DSA
            try
            {
                Connection cn = Global.GetDSAConnection();

                List<List<DataRow>> rowList = new List<List<DataRow>> ();
                
                int sp = 100, cot =0,itm=0,idx=0;
                itm = Global._dtUploadData.Rows.Count / sp + 1;
                for (int i = 1; i <= itm; i++)
                    rowList.Add(new List<DataRow>());

                foreach (DataRow dr in Global._dtUploadData.Rows)
                {
                    if(cot ==sp)
                    {
                        cot=0;
                        idx++;
                    }                    
                    rowList[idx].Add(dr);
                    cot++;
                }              

                foreach (List<DataRow> rows in rowList)
                {
                    XElement ReqElement = ConverRowACCardHistoryl(rows);
                    Global.SendRquest(3,cn, UploadCardNoHistoryServiceName, ReqElement,_GUID);                  
                }
            }
            catch (Exception ex)
            {
                LogAgent.logDB(3, "結束", false, DateTime.Now, "傳送到DSA失敗："+ex.Message, _GUID);
                Console.WriteLine(ex.Message);
            }
            
            #endregion

            #region 處理local資料庫內未上傳資料上傳DSA
            DataTable dt = getNotUploadRecords();
            NpgsqlConnection cnP = new NpgsqlConnection();
            try
            {
                cnP = Global.GetDBConnection();

            }
            catch (Exception ex)
            {
                LogAgent.logDB(3, "結束", false, DateTime.Now, "建立LocalDB資料庫連線失敗：" + ex.Message, _GUID);
                Console.WriteLine(ex.Message);
            }
            Connection cnN = Global.GetDSAConnection();

            if (cnN.IsConnected)
            {
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = cnP;

            foreach (DataRow dr in dt.Rows)
            {
                // 1.send request
                bool isSendOk = false;
                try
                {
                    List<DataRow> iRow = new List<DataRow>();
                    iRow.Add(dr);
                    XElement reqElm = ConverRowACCardHistoryl(iRow);
                    Global.SendRquest(3,cnN, UploadCardNoHistoryServiceName, reqElm,_GUID);
                    isSendOk = true;
                }
                catch (Exception ex)
                {
                    LogAgent.logDB(3, "結束", false, DateTime.Now, "傳送到DSA資料庫失敗：" + ex.Message, _GUID);
                    Console.WriteLine(ex.Message);
                }
                   
                // 2.update status
                if (isSendOk)
                {
                    string uid = dr["uid"].ToString();
                    string strSQL = "update access_control_card_history set upload_success='t',upload_campus_time=now(),step3_session_guid='" + _GUID + "' where uid=" + uid;
                    cmd.CommandText = strSQL;
                    cmd.ExecuteNonQuery();
                }
            }
            if (cnP.State == ConnectionState.Open)
                cnP.Close();
            }
            else
            {
                LogAgent.logDB(3, "結束", false, DateTime.Now, "連線失敗", _GUID);
            }            
            #endregion
        }

    

        /// <summary>
        /// 取得未上傳成功的紀錄
        /// </summary>
        public DataTable getNotUploadRecords()
        {
            DataTable dt = new DataTable();
                        
            try
            {
                NpgsqlConnection cn = Global.GetDBConnection();
                if (cn.State == ConnectionState.Open)
                {
                    string query = "select uid,card_no, oclock_name, use_time, use_type, guid, step2_time, step2_session_guid, upload_campus_time, upload_success,step3_session_guid from access_control_card_history  where upload_success<>'t';";
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, cn);
                    da.Fill(dt);
                    
                    cn.Close();
                }
            }
            catch (Exception ex)
            {
                LogAgent.logDB(3, "結束", false, DateTime.Now, "取得未上傳成功的紀錄失敗：" + ex.Message, _GUID);
                Console.WriteLine(ex.Message);
            }
   
            return dt;
        }

        /// <summary>
        /// 將 DataRow轉成XML
        /// </summary>
        /// <param name="RowList"></param>
        /// <returns></returns>
        public XElement ConverRowACCardHistoryl(List<DataRow> RowList)
        {
            XElement ReqElm = new XElement("Request");

            foreach (DataRow dr in RowList)
            {
                XElement ReqElm2 = new XElement("AccessControlCard.history");
                XElement ReqElmData = new XElement("Field");

                ReqElmData.SetElementValue("CardNo", dr["card_no"].ToString());
                ReqElmData.SetElementValue("Guid", dr["guid"].ToString());
                ReqElmData.SetElementValue("OclockName", dr["oclock_name"].ToString());
                ReqElmData.SetElementValue("Step2SessionGuid", dr["step2_session_guid"].ToString());
                ReqElmData.SetElementValue("Step2Time", DateTime.Parse(dr["step2_time"].ToString()).ToString("yyyy/MM/dd HH:mm:ss"));
                ReqElmData.SetElementValue("UseTime", DateTime.Parse(dr["use_time"].ToString()).ToString("yyyy/MM/dd HH:mm:ss"));
                ReqElmData.SetElementValue("UseType", dr["use_type"].ToString());
                ReqElm2.Add(ReqElmData);
                ReqElm.Add(ReqElm2);
            }
            return ReqElm;
        }


        public void Start(string guid)        
        {
            try
            {
                LogAgent.logDB(3, "開始", true, DateTime.Now, "讀取LocalDB傳送到DSA。", guid);
                _GUID = guid;
                uploadRecord();
                LogAgent.logDB(3, "結束", true, DateTime.Now, "從LocalDB傳送到DSA共"+Global._dtUploadData.Rows.Count+"筆。", guid);
            }
            catch (Exception ex)
            {
                LogAgent.logDB(3, "結束", false, DateTime.Now, "讀取LocalDB傳送到DSA失敗：" + ex.Message, guid);
                Console.WriteLine("ACCHistoryUploader:" + ex.Message);
            }
        }
    }
}
