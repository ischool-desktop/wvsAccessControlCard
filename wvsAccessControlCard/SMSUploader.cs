using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FISCA.DSAClient;

namespace wvsAccessControlCard
{
    /// <summary>
    /// 上傳簡訊功能
    /// </summary>
    public class SMSUploader
    {
        private string _GUID="";
        XElement SMSSettingsElm = null;
        XElement UnSendDataElm = null;
        XElement SendDataElm = null;

        List<StudSendData> _StudSendData = new List<StudSendData>();

        /// <summary>
        /// 取得要傳道SMS 資料
        /// </summary>
        public void getRecordFromUDS()
        {
            // 取得要傳送到SMS 資料
            try
            {
                Connection cn = Global.GetDSAConnection();
                if (cn.IsConnected)
                {
                    XElement reqElm = new XElement("Request");
                    UnSendDataElm = Global.SendRquest(4,cn,"service.GetUnsentRecords",reqElm,_GUID);
                }
            }
            catch (Exception ex)
            { 
            
            }
        
        }

        public void makeSMSContent()
        {
            /*
           <Response>
               <AccessControlCard.setting>
                   <Uid>790970</Uid>
                   <LastUpdate>2013-06-25 11:16:31.477736</LastUpdate>
                   <ArriveSchoolSms/>
                   <EnableArriveSchoolSms>f</EnableArriveSchoolSms>
                   <EnableAutoSms>f</EnableAutoSms>
                   <EnableErrorSms>t</EnableErrorSms>
                   <EnableLeaveSchoolSms>f</EnableLeaveSchoolSms>
                   <ErrorPhone/>
                   <LeaveSchoolSms/>
               </AccessControlCard.setting>
           </Response>  * */

            string ArriveSchoolSms = "";
            string LeaveSchoolSms = "";
            // 1. 取得設定值
            if(SMSSettingsElm !=null)
            {
                if (SMSSettingsElm.Element("AccessControlCard.setting") != null)
                {
                    if (SMSSettingsElm.Element("AccessControlCard.setting").Element("ArriveSchoolSms") != null)
                    {
                        ArriveSchoolSms = SMSSettingsElm.Element("AccessControlCard.setting").Element("ArriveSchoolSms").Value;
                    }

                    if (SMSSettingsElm.Element("AccessControlCard.setting").Element("LeaveSchoolSms") != null)
                    {
                        LeaveSchoolSms = SMSSettingsElm.Element("AccessControlCard.setting").Element("LeaveSchoolSms").Value;
                    }
                }
            }
/*
 <Response>
	<AccessControlCard.history>
		<Uid>790974</Uid>
		<LastUpdate>2013-06-25 11:16:56.578018</LastUpdate>
		<CardNo>12345678</CardNo>
		<Guid>pxhensizneydshrng</Guid>
		<OclockName>01</OclockName>
		<Step2SessionGuid>kjmnhytgbvfrecsd</Step2SessionGuid>
		<Step2Time>2013-06-25 10:10:10</Step2Time>
		<UseTime>2013-06-25 08:10:20</UseTime>
		<UseType>01</UseType>
		<StudentName>58437</StudentName>
		<StudentID>林小旻</StudentID>
	</AccessControlCard.history>
</Response>
 */
            _StudSendData.Clear();
            // 2. 取得未傳送資料
            if (UnSendDataElm != null)
            {
                foreach (XElement elmData in UnSendDataElm.Elements("AccessControlCard.history"))
                {
                    StudSendData ssd = new StudSendData();

                    ssd.CardNo = Utility.GetElemetValue(elmData, "CardNo");
                    ssd.isOk = false;
                    ssd.Message = "";
                    ssd.StudentID = Utility.GetElemetValue(elmData, "StudentID");
                    ssd.StudentName = Utility.GetElemetValue(elmData, "StudentName");
                    _StudSendData.Add(ssd);
                }
            }
        }

        /// <summary>
        /// 將簡訊傳送到SMS
        /// </summary>
        public void sendToSNSService()
        {
            if (_StudSendData.Count > 0)
            {
                
                foreach (StudSendData ssd in _StudSendData)
                {
                    // 1. 讀取資料

                    // 2. 傳送資料
                }                
            }
        }


        public void LogSuccessRecords()
        { 
        
        }

        /// <summary>
        /// 
        /// </summary>
        public void updateRecordAsBeingSent()
        { 
        
        }

        public void Start(string guid)        
        {
            _GUID = guid;
            try
            {
                LogAgent.logDB(4, "開始", true, DateTime.Now, "將DSA資料傳送到SMS。", guid);
                
                //// 取得 ischool 設定訊息資料  XML  file
                //Connection cn = Global.GetDSAConnection();
                //if(cn.IsConnected)
                //{
                //    XElement reqElm= new XElement ("Request");
                //    SMSSettingsElm = Global.SendRquest(4, cn, "service.GetSettings", reqElm, _GUID);
                //}
                //getRecordFromUDS();
                //makeSMSContent();
                //sendToSNSService();
                //LogSuccessRecords();
                Connection cn = Global.GetDSAConnection();
                if (cn.IsConnected)
                {
                    XElement ReqElement = new XElement("Request");
                    // 呼叫 Server 傳送資料
                    Global.SendRquest(3, cn, "service.UploadToSMSService", ReqElement, _GUID);
                }

                LogAgent.logDB(4, "結束", true, DateTime.Now, "將DSA資料傳送到SMS完成。", guid);
            }
            catch (Exception ex)
            {
                LogAgent.logDB(4, "結束", false, DateTime.Now, "將DSA資料傳送到SMS失敗："+ex.Message, guid);
            }
        }
    }
}
