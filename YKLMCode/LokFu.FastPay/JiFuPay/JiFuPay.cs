using LokFu.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
namespace LokFu.FastPay.JiFuPay
{
    public class Head
    {
        public string version;
        public string charset;
        public string partnerNo;
        public string txnCode;
        public string orderId;
        public string reqDate;
        public string reqTime;
    }
    public class JFTools
    {
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Encrypt(string toEncrypt, string key, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.BlockSize = 128;
            rDel.KeySize = 256;
            rDel.FeedbackSize = 128;
            rDel.Padding = PaddingMode.PKCS7;
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }


        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="toDecrypt"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string Decrypt(string toDecrypt, string key, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.BlockSize = 128;
            rDel.KeySize = 256;
            rDel.FeedbackSize = 128;
            rDel.Padding = PaddingMode.PKCS7;
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }


        /// <summary>  
        /// SHA1 加密，返回大写字符串  
        /// </summary>  
        /// <param name="content">需要加密字符串</param>  
        /// <param name="encode">指定加密编码</param>  
        /// <returns>返回40位大写字符串</returns>  
        public static string SHA1(string content, Encoding encode)
        {
            try
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                byte[] bytes_in = encode.GetBytes(content);
                byte[] bytes_out = sha1.ComputeHash(bytes_in);
                sha1.Dispose();
                string result = BitConverter.ToString(bytes_out);
                result = result.Replace("-", "");
                return result.ToLower();
            }
            catch (Exception ex)
            {
                throw new Exception("SHA1加密出错：" + ex.Message);
            }
        }

        public static string GetBankLatter(string bankAbbr)
        {
            string bankCode = "AAA";
            switch (bankAbbr)
            {
                case "102":
                    bankCode = "ICBC";
                    break;
                case "103":
                    bankCode = "ABC";
                    break;
                case "104":
                    bankCode = "BOC";
                    break;
                case "105":
                    bankCode = "CCB";
                    break;
                case "201":
                    bankCode = "CDB";
                    break;
                case "202":
                    bankCode = "EXIMBANK";
                    break;
                case "203":
                    bankCode = "ADBC";
                    break;
                case "301":
                    bankCode = "BCOM";
                    break;
                case "302":
                    bankCode = "CITIC";
                    break;
                case "303":
                    bankCode = "CEB";
                    break;
                case "304":
                    bankCode = "HXB";
                    break;
                case "305":
                    bankCode = "CMBC";
                    break;
                case "306":
                    bankCode = "CGB";
                    break;
                case "307":
                    bankCode = "PAB";
                    break;
                case "308":
                    bankCode = "CMB";
                    break;
                case "309":
                    bankCode = "CIB";
                    break;
                case "310":
                    bankCode = "SPDB";
                    break;
                case "313":
                    bankCode = "BIN";
                    break;
                case "315":
                    bankCode = "EGBANK";
                    break;
                case "316":
                    bankCode = "CZB";
                    break;
                case "403":
                    bankCode = "PSBC";
                    break;
                case "501":
                    bankCode = "HSBC";
                    break;
                case "502":
                    bankCode = "HKBEA";
                    break;
                case "503":
                    bankCode = "NCBCHINA";
                    break;
                case "504":
                    bankCode = "HANGSENG";
                    break;
                case "783":
                    bankCode = "SDB";
                    break;
                case "905":
                    bankCode = "UNIONPAY";
                    break;
                case "317":
                    bankCode = "BOB";
                    break;
                case "318":
                    bankCode = "HCCB";
                    break;
                case "319":
                    bankCode = "NJCB";
                    break;
                case "314":
                    bankCode = "BRCB";
                    break;
            }
            return bankCode;
        }
    }
    public class JiFuFdPay {
        public static string FengdingUrl = "http://fast.jfpays.com:19087/rest/api/";
        /// <summary>
        /// Token类型
        /// </summary>
        /*
             * 15、商户注册
             * 16、商户侧开通短信
             * 17、商户侧绑卡开通(无通知)
             * 18、银联侧绑卡开通(前后台通知)
             * 19、卡开通状态查询
             * 20、支付短信
             * 21、消费支付(后台通知)
             * 22、支付状态查询
             * 23、商户费率、结算银行卡信息变更
             * 25、无卡支付提现
       */
        public enum TokenType : int { MerReg = 15, MerSms = 16, MerCard = 17, PayCard = 18, PayCardQuery = 19, PaySms = 20, PayDo = 21, PayQuery = 22, MerFree = 23, PayCash = 25 };
        public static string GetToKen(TokenType TokenType, string partnerNo, string EncryptKey, string SignKey)
        {
            string txnCode = "700001";
            DateTime Now = DateTime.Now;
            string ReqNum = Now.ToString("yyyyMMddHHmmssfff");
            Random Random = new Random(Utils.GetRandomSeed());
            int Num = Random.Next(0, 1000);
            ReqNum = ReqNum + Num.ToString();
            Dictionary<string, object> map = new Dictionary<string, object>();
            map.Add("head", new Head { version = "1.0.0", charset = "UTF-8", partnerNo = partnerNo, txnCode = txnCode, orderId = ReqNum, reqDate = Now.ToString("yyyyMMdd"), reqTime = Now.ToString("yyyyMMddHHmmss") });
            map.Add("tokenType", TokenType);
            JavaScriptSerializer Jss = new JavaScriptSerializer();
            string PostString = Jss.Serialize(map);
            //AES加密
            string encryptData = JFTools.Encrypt(PostString, EncryptKey, EncryptKey);
            //签名
            string signData = JFTools.SHA1(PostString + SignKey, Encoding.UTF8);
            map.Add("sign", signData);
            string ext = "rmark";
            string paramStr = string.Format("encryptData={0}&partnerNo={1}&signData={2}&orderId={3}&ext={4}", HttpUtility.UrlEncode(encryptData), partnerNo, HttpUtility.UrlEncode(signData), ReqNum, ext);
            string RetString = Utils.PostRequest(FengdingUrl + txnCode, paramStr);
            JObject JObj = new JObject();
            try
            {
                JObj = (JObject)JsonConvert.DeserializeObject(RetString);
            }
            catch (Exception)
            {
                Utils.WriteLog("【" + PostString + "】" + RetString, "JFPay");
                JObj = null;
            }
            if (JObj != null)
            {
                string data = JObj["encryptData"].ToString();
                string decryptData = JFTools.Decrypt(data, EncryptKey, EncryptKey);
                JObj = (JObject)JsonConvert.DeserializeObject(decryptData);
                JObject Head = JObj;
                if (JObj["head"] != null)
                {
                    Head = (JObject)JObj["head"];
                }
                string respCode = "000000";
                if (Head["respCode"] != null)
                {
                    respCode = Head["respCode"].ToString();
                }
                if (respCode == "000000")
                {
                    string token = JObj["token"].ToString();
                    return token;
                }
                else
                {
                    string respMsg = Head["respMsg"].ToString();
                    Utils.WriteLog("token：[" + respCode + "]" + respMsg + "||" + decryptData + "【" + PostString + "】", "JFPay");
                }
            }
            return "Error";
        }
    }
}
