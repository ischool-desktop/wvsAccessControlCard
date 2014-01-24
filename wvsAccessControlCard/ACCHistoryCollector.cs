using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace wvsAccessControlCard
{
    /// <summary>
    /// 讀取讀卡機資料
    /// </summary>
    public class ACCHistoryCollector
    {
        public bool checkClockInUse()
        {
            return false;
        }

        /// <summary>
        /// 呼叫讀卡機程式
        /// </summary>
        public void invoke()
        {
            // 執行卡鐘程式
            System.Diagnostics.Process.Start(@Global._Config.CAP_RunApp);
            
            // 等20秒
            delayTime(Global._Config.CAP_WaitTime);

        }

        /// <summary>
        /// 等待時間(秒)
        /// </summary>
        /// <param name="sec"></param>
        public void delayTime(int sec)
        {
            Thread.Sleep(sec*1000);
        }

        /// <summary>
        /// 啟動
        /// </summary>
        public void Start(string guid)
        {
            try
            {
                LogAgent.logDB(1, "開始", true, DateTime.Now, "開始執行讀卡機程式。", guid);
                checkClockInUse();
                invoke();
                LogAgent.logDB(1, "結束", true, DateTime.Now, "結束執行讀卡機程式", guid);
            }
            catch (Exception ex)
            {
                LogAgent.logDB(1, "結束", false, DateTime.Now, "執行讀卡機程式錯誤：" + ex.Message, guid);
                Console.WriteLine("ACCHistoryCollector:" + ex.Message);
            }            
        }
    }
}
