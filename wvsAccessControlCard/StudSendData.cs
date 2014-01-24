using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wvsAccessControlCard
{
    /// <summary>
    /// 學生傳送資料
    /// </summary>
    public class StudSendData
    {
        /// <summary>
        /// 學生ID
        /// </summary>
        public string StudentID { get; set; }

        /// <summary>
        /// 學生姓名
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// 卡號
        /// </summary>
        public string CardNo { get; set; }

        /// <summary>
        /// 傳送訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool isOk { get; set; }

    }
}
