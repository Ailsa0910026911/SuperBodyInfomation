using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace LokFu.FastPay
{
    public class WLBTools
    {

        /// <summary>
        /// 生成随机行业
        /// </summary>
        /// <param name="lens">随机字符长度</param>
        /// <returns>随机行业</returns>
        public static string RandomWxCode()
        {
            string[] Array = "292,153,209,210,116,129,293,294,295,296,297,298,305,319,323,123,299,306,320,321,143,157,300,148,149,301,307,308,302,303,304,147,230,322,324,155,309,242,158".Split(',');
            int len = Array.Length;
            Random random = new Random();
            int i = random.Next(len);
            string Code = Array[i];
            return Code;
        }
        /// <summary>
        /// 生成随机行业
        /// </summary>
        /// <param name="lens">随机字符长度</param>
        /// <returns>随机行业</returns>
        public static string RandomAliCode()
        {
            string[] Array = "2015050700000041,2015061690000029,2016062900190066,2015050700000042,2015050700000043,2015050700000045,2015101000064159".Split(',');
            int len = Array.Length;
            Random random = new Random();
            int i = random.Next(len);
            string Code = Array[i];
            return Code;
        }

        /// <summary>
        /// 微联宝字符串拼接签名
        /// </summary>
        /// <param name="dicArray">源文件</param>
        /// <param name="isSign">是否是签名</param>
        /// <returns></returns>
        public static string WLB_LinkString(Dictionary<string, string> dicArray, bool isSign = false)
        {
            StringBuilder prestr = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in dicArray)
            {
                if (isSign)
                {
                    prestr.Append(temp.Key + "=" + HttpUtility.UrlEncode(temp.Value) + "&");
                }
                else
                {
                    prestr.Append("#" + temp.Value);
                }
            }
            //去掉最後一個&字符
            if (isSign)
            {
                int nLen = prestr.Length;
                prestr.Remove(nLen - 1, 1);
            }
            return prestr.ToString();
        
        }

        /// <summary>
        /// 验签
        /// </summary>
        /// <param name="sort">源数据</param>
        /// <param name="key">加密串</param>
        /// <param name="type">微信&支付宝</param>
        /// <param name="sign">签名结果</param>
        /// <returns>返回结果</returns>
        public static bool Verify(SortedDictionary<string, string> obj, string key, string trxType, string sign)
        {

            bool ret = false;
            string str = "#";

            StringBuilder builder = new StringBuilder();

            if (trxType == "WX_SCANCODE")
            {
                builder.Append(str + obj["trxType"].ToString());
                builder.Append(str + obj["retCode"].ToString());
                builder.Append(str + obj["r1_merchantNo"].ToString());
                builder.Append(str + obj["r2_orderNumber"].ToString());
                builder.Append(str + obj["r3_amount"].ToString());
                builder.Append(str + obj["r4_bankId"].ToString());
                builder.Append(str + obj["r5_business"].ToString());
                builder.Append(str + obj["r6_timestamp"].ToString());
                builder.Append(str + obj["r7_completeDate"].ToString());
                builder.Append(str + obj["r8_orderStatus"].ToString());
                builder.Append(str + obj["r9_serialNumber"].ToString());
                builder.Append(str + obj["r10_t0PayResult"].ToString());
            }
            if (trxType == "OnlineQuery")
            {
                builder.Append(str + obj["trxType"].ToString());
                builder.Append(str + obj["retCode"].ToString());
                builder.Append(str + obj["r1_merchantNo"].ToString());
                builder.Append(str + obj["r2_orderNumber"].ToString());
                builder.Append(str + obj["r3_amount"].ToString());
                builder.Append(str + obj["r4_bankId"].ToString());
                builder.Append(str + obj["r5_business"].ToString());
                builder.Append(str + obj["r6_createDate"].ToString());
                builder.Append(str + obj["r7_completeDate"].ToString());
                builder.Append(str + obj["r8_orderStatus"].ToString());
                builder.Append(str + obj["r9_withdrawStatus"].ToString());
            }

            string signStr = (builder.Append(str).ToString() + key).GetMD5();
            if (signStr.ToLower() == sign.ToLower())
            {
                ret = true;
            }
            return ret;
        }

    }
}
