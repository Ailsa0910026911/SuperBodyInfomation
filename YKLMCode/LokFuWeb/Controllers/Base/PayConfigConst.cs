using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
namespace LokFuWeb.Controllers.Base
{
    /// <summary>
    /// 支付基础配置的名称
    /// </summary>
    public static class PayConfigConst
    {
        public static readonly StringDictionary dictionary = new StringDictionary()
        {
            {"Baofoo","商户号,终端号,密钥"},
            {"Unionpay","商户号"},
            {"Cash",""},
            {"AliPay","商户号,密钥,支付宝号"},
            {"WeiXin","公众号appid,商户号,商户密钥,公众号appsecret,子商户号"},
            {"NFC","商户号,密钥"},
            {"CMBC","商户号,密钥"},
            {"HFPay","商户号,密钥,通道"},
            {"HFAliPay","商户号,密钥,通道"},
            {"HFWeiXin","商户号,密钥,通道"},
            {"HFNFC","商户号,密钥,通道"}
        };
    }
    /// <summary>
    /// 支付基础配置的名称
    /// </summary>
    public static class FastPayWayConst
    {
        public static readonly StringDictionary dictionary = new StringDictionary()
        {
            {"GHTPay","商户号,业务码,密钥"},{"GhtMPay","商户号,业务码,密钥"},
            {"MiPay","商户号,加密Key,加密iv,签名Key"},{"MiShua","商户号,加密Key,加密iv,签名Key"},{"MiBank","商户号,加密Key,加密iv,签名Key"},
            {"QiaPay","商户号,密钥"},
            {"WFTWA","商户号,签名密钥,加密密钥"},
            {"HFPay","商户号,密钥,通道"},
            {"BJNSHT1","商户号,密钥"},{"BJNSHD0","商户号,密钥"},
            {"WLBPay","商户号,密钥,Des"},
            {"SFPay","商户号"},
            {"ZBLHPay","商户号,密钥,加密号,通道"},
            {"HLBYYH5","商户号,密钥,加密号"},
            {"HLBYYJF","商户号,密钥,加密号"},
            {"XJPay","商户号,密钥"},
            {"JiFuPay","商户号,加密密钥,签名密钥"},
            {"JiFuJFPay","商户号,加密密钥,签名密钥"},
            {"JiFuFdPay","商户号,加密密钥,签名密钥,费率编码"},
            {"BCPay","商户号,签名密钥,加密密钥"},
            {"ZxPay","商户号,签名密钥,加密密钥"},
            {"HFJSPay","商户号,密钥,通道"}
        };
    }
}