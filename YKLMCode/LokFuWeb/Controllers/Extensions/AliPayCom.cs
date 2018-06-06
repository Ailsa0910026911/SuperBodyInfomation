using LokFu.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace LokFu.PayMent.ALIPAY
{
    public class AliPayCom
    {
        public string app_id = "";
        public string method = "";
        public string notify_url = "";

        private string biz_content = "";//这里由方法里面赋值
        private string timestamp = "";
        private const string charset = "gb2312";
        private const string sign_type = "RSA2";
        private const string SignType = "SHA256"; //对应NET签名方法
        private const string version = "1.0";
        private const string PostUrl = "https://openapi.alipay.com/gateway.do";
        public AliPayCom() {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public string Send(Dictionary<string, string> Biz)
        {
            JavaScriptSerializer Jss = new JavaScriptSerializer();
            biz_content = Jss.Serialize(Biz);
            SortedDictionary<string, string> Sort = new SortedDictionary<string, string>();
            Sort.Add("app_id", app_id);
            Sort.Add("method", method);
            Sort.Add("charset", charset);
            Sort.Add("timestamp", timestamp);
            Sort.Add("version", version);
            Sort.Add("notify_url", notify_url);
            Sort.Add("biz_content", biz_content);
            Sort.Add("sign_type", sign_type);
            Dictionary<string, string> Dic = Utils.FilterPara(Sort);
            string PostString = Utils.CreateLinkString(Dic);
            string Sign = this.SignTrue(PostString);
            PostString += "&sign=" + HttpUtility.UrlEncode(Sign);
            string RetString = Utils.PostRequests(PostUrl, PostString, "utf-8", null, charset);
            return RetString;
        }
        public string SignTrue(string source)
        {
            //==========================
            //读取私钥
            string FilePath = AppDomain.CurrentDomain.BaseDirectory;
            string file = FilePath + "certs\\";
            string PrivateKey = File.ReadAllText(file + "Alipay_" + app_id + "_private.cer");
            //==========================
            return RSAUtils.RSASign(source, PrivateKey, SignType, charset);
        }
        public bool CheckSign(string source, string signtrue)
        {
            //==========================
            //读取公钥
            string FilePath = AppDomain.CurrentDomain.BaseDirectory;
            string file = FilePath + "certs\\";
            string PublicKey = File.ReadAllText(file + "Alipay_" + app_id + ".cer");
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PublicKey);
            byte[] datasc = Encoding.GetEncoding(charset).GetBytes(source);
            byte[] datasn = Convert.FromBase64String(signtrue);
            bool Ret = rsa.VerifyData(datasc, SignType, datasn);
            return Ret;
        }
        //sign


    }
}
