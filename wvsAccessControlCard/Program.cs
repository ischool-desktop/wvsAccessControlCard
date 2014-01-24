using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wvsAccessControlCard
{
    public class Program
    {
        /// <summary>
        /// 程式進入
        /// </summary>
        public static void Main()
        {
            // 讀取設定檔
            Global._Config = new ConfigManager();

            // 產生 GUID
            Guid g = Guid.NewGuid();
            string guid = g.ToString();

            Global._dtUploadData.Columns.Add("card_no");
            Global._dtUploadData.Columns.Add("oclock_name");
            Global._dtUploadData.Columns.Add("use_time");
            Global._dtUploadData.Columns.Add("use_type");
            Global._dtUploadData.Columns.Add("guid");
            Global._dtUploadData.Columns.Add("step2_time");
            Global._dtUploadData.Columns.Add("step2_session_guid");
            Global._dtUploadData.Columns.Add("upload_campus_time");
            Global._dtUploadData.Columns.Add("upload_success");
            Global._dtUploadData.Columns.Add("step3_session_guid");

            
            // 執行讀卡鐘            
            ACCHistoryCollector _ACCHistoryCollector = new ACCHistoryCollector();
            _ACCHistoryCollector.Start(guid);
            
            // 執行讀取資料、比對、解析            
            ACCHistoryParser _ACCHistoryParser = new ACCHistoryParser();
            _ACCHistoryParser.Start(guid);

            // 傳送資並紀錄          
            ACCHistoryUploader _ACCHistoryUploader = new ACCHistoryUploader();
            _ACCHistoryUploader.Start(guid);

            // 傳送簡訊資料
            SMSUploader _SMSUploader = new SMSUploader();
            _SMSUploader.Start(guid);

        }

    }
}
