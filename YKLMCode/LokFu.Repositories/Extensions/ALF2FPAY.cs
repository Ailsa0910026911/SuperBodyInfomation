using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LokFu.Repositories
{
    public partial class ALF2FPAY
    {
        public static string alipay_public_key = System.AppDomain.CurrentDomain.BaseDirectory + "certs\\alipay_public.pem";
        public static string merchant_private_key = System.AppDomain.CurrentDomain.BaseDirectory + "certs\\alipay_private_key.pem";
        public static string merchant_public_key = System.AppDomain.CurrentDomain.BaseDirectory + "certs\\alipay_public_key.pem";
        public static string serverUrl = "https://openapi.alipay.com/gateway.do";
        public static string mapiUrl = "https://mapi.alipay.com/gateway.do";

        public static string charset = "utf-8";
        public static string sign_type = "RSA";
        public static string version = "1.0";

        private string APPID = "";
        private string PID = "";

        public string appId
        {
            get { return APPID; }
            set { APPID = value; }
        }
        public string pid
        {
            get { return PID; }
            set { PID = value; }
        }

        public ALF2FPAY()
        {

        }

        public static string getMerchantPublicKeyStr()
        {
            StreamReader sr = new StreamReader(merchant_public_key);
            string pubkey = sr.ReadToEnd();
            sr.Close();
            if (pubkey != null)
            {
              pubkey=  pubkey.Replace("-----BEGIN PUBLIC KEY-----", "");
              pubkey = pubkey.Replace("-----END PUBLIC KEY-----", "");
              pubkey = pubkey.Replace("\r", "");
              pubkey = pubkey.Replace("\n", "");
            }
            return pubkey;
        }

        public static string getMerchantPriveteKeyStr()
        {
            StreamReader sr = new StreamReader(merchant_private_key);
            string pubkey = sr.ReadToEnd();
            sr.Close();
            if (pubkey != null)
            {
                pubkey = pubkey.Replace("-----BEGIN PUBLIC KEY-----", "");
                pubkey = pubkey.Replace("-----END PUBLIC KEY-----", "");
                pubkey = pubkey.Replace("\r", "");
                pubkey = pubkey.Replace("\n", "");
            }
            return pubkey;
        }
    }
}