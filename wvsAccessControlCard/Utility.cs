using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace wvsAccessControlCard
{
    public class Utility
    {
        /// <summary>
        /// 取得 XML 內值
        /// </summary>
        /// <param name="elm"></param>
        /// <param name="elmName"></param>
        /// <returns></returns>
        public static string GetElemetValue(XElement elm,string elmName)
        {
            string retVal = "";
            if (elm.Element(elmName) != null)
                retVal = elm.Element(elmName).Value;
            return retVal;
        }
    }
}
