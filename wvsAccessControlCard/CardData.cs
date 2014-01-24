using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wvsAccessControlCard
{
    public class CardData
    {
        string _Source = "";
        string _ErrorString = "";
        bool _ParsePass = false;

        public CardData(string Source)
        {
            _Source = Source;
            _ParsePass = Parse();
        }

        /// <summary>
        /// 取得傳入原始資料
        /// </summary>
        /// <returns></returns>
        public string GetSourceData()
        {
            return _Source;
        }

        /// <summary>
        /// 取得是否解析成功
        /// </summary>
        /// <returns></returns>
        public bool GetPass()
        {
            return _ParsePass;
        }

        /// <summary>
        /// 取得錯誤訊息
        /// </summary>
        /// <returns></returns>
        public string GetErrorString()
        {
            return _ErrorString;
        }

        /// <summary>
        /// 解析刷卡資料
        /// </summary>
        /// <returns></returns>
        private bool Parse()
        {
            bool pass = false;

            try
            {
                int len=_Source.Length;
                if ( len> 20)
                {
                    try
                    {
                        ClockNo = _Source.Substring(0, 2);
                        Date = _Source.Substring(2, 8);
                        Time = _Source.Substring(10, 4);
                        UseType = _Source.Substring(14, 2);
                        InputType = _Source.Substring(16, 1);
                        InputNo = _Source.Substring(17, 1);
                        CardNo = _Source.Substring(18, (len-18));
                    }
                    catch (ArgumentOutOfRangeException ee)
                    { 
                        pass=false;
                        _ErrorString=ee.Message;
                    }
                               pass = true;
                }
                else
                {
                    pass = false;
                    _ErrorString = "資料格式長度有問題無法解析";
                }
            }
            catch (Exception ex)
            {
                pass = false;
                _ErrorString = ex.Message;
            }
            return pass;        
        }


        /// <summary>
        /// 卡鐘台號 01
        /// </summary>
        public string ClockNo { get; set; }

        /// <summary>
        /// 年月日(20130101)
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// 時分(1500)
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// 班別鍵 01 上學 02 放學
        /// </summary>
        public string UseType { get; set; }

        /// <summary>
        /// 輸入方式：0,用刷卡輸入，K,用鍵盤輸入。
        /// </summary>
        public string InputType { get; set; }
        
        /// <summary>
        /// 刷卡槽代碼：A,主刷卡槽，B,外接刷卡槽。
        /// </summary>
        public string InputNo { get; set; }

        /// <summary>
        /// 卡號
        /// </summary>
        public string CardNo { get; set; }
    }
}
