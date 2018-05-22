﻿using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;

namespace Com.Alipay
{
    /// <summary>
    /// 类名：Config
    /// 功能：基础配置类
    /// 详细：设置帐户有关信息及返回路径
    /// 版本：3.4
    /// 修改日期：2016-03-08
    /// 说明：
    /// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
    /// 该代码仅供学习和研究支付宝接口使用，只是提供一个参考。
    /// </summary>
    public class Config
    {

        //↓↓↓↓↓↓↓↓↓↓请在这里配置您的基本信息↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

        // 合作身份者ID，签约账号，以2088开头由16位纯数字组成的字符串，查看地址：https://b.alipay.com/order/pidAndKey.htm
        public static string partner = "2088031481141740";

        // 收款支付宝账号，以2088开头由16位纯数字组成的字符串，一般情况下收款账号就是签约账号
        public static string seller_id = partner;

        //商户的私钥,原始格式，RSA公私钥生成：https://doc.open.alipay.com/doc2/detail.htm?spm=a219a.7629140.0.0.nBDxfy&treeId=58&articleId=103242&docType=1
        public static string private_key = "MIICWwIBAAKBgQC7cATF6Tz18If/z7Ln28mH/qO8px3v3X9twRufmxnBgvKBR0UAG4FHKuBYR697y6Cm7evZmFPUwuhP6qrEtYQxepBHORnI6X3tVTUL4qJki2rtc0igOG7zt5IvOno8zRdr48itghVS6Va/FPD7MWX3K1HCKWd7v10UF4lD7OIW7wIDAQABAoGATSKKNFAJsERuE04MF/KQ+xBFnatAXKfQewBPN92COeqDlcQYlzS6fOkeLfZC"
+ "NzXTRR8AQim3xgT3yXuPDyiL34RbAjiFIkc0x0GC7VzwW0vx+4D8dPbIslBAEH+/qiDa6MMr7AN7APcF3yg2dhhlC4Ki34Grt03YvN4Ks9h9INECQQDj9j20CRvrKC688NKhQ7ipgSMUo7Dhs+ya6+8R65ROEBW1EJRVDzNXx8VmhhQu/hQC+XwR+UND2nfr"
+ "oqirmPhFAkEA0n3Na7VkkOt3pgwUWgOG13qWTmTwrMUAwyLRH1kzQ8UCjpUGBtF+ytgtFt8Z0lPNmECPtN9pzqQXVShBIFunowJAFdc2zDP/N8WOzM1p8MdzPtI/kS+pt8YsSh+GtTnT6LLD0ag3/fpJ2gQq1orMDkCLnElY6MPv/OVGjg30iRRL3QJAOgPA"
+ "bXJWYHb1yEdGCLU2IgfAi6TYjNXTavUarEg6j/Apz0CVo6V0C9ZfPxGInWbJoLASq6h0Eic54xewM4nvfwJAYO0TLVW2hMxymWGDQ5bqioFb7wyEl56SLQt7szK4RPpfUjG8nfM6iumPyKbOmjUpRosA+5YOboI6wum60TeNAA==";

        //支付宝的公钥，查看地址：https://b.alipay.com/order/pidAndKey.htm 
        public static string alipay_public_key = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCnxj/9qwVfgoUh/y2W89L6BkRAFljhNhgPdyPuBV64bfQNN1PjbCzkIM6qRdKBoLPXmKKMiFYnkd6rAoprih3/PrQEB/VsW8OoM8fxn67UDYuyBTqA23MML9q1+ilIZwBC2AQ2UBVOrFXfFl75p6/B5KsiNG9zpgmLCUYuLkxpLQIDAQAB";

        // 服务器异步通知页面路径，需http://格式的完整路径，不能加?id=123这类自定义参数,必须外网可以正常访问
        public static string notify_url = "http://cts.slyklm.com/Alipay/notify_url.aspx";

        // 页面跳转同步通知页面路径，需http://格式的完整路径，不能加?id=123这类自定义参数，必须外网可以正常访问
        public static string return_url = "http://cts.slyklm.com/Alipay/return_url.aspx";

        // 签名方式
        public static string sign_type = "RSA";

        // 调试用，创建TXT日志文件夹路径，见AlipayCore.cs类中的LogResult(string sWord)打印方法。
        public static string log_path = HttpRuntime.AppDomainAppPath.ToString() + "log/";

        // 字符编码格式 目前支持utf-8
        public static string input_charset = "utf-8";

        // 支付类型 ，无需修改
        public static string payment_type = "1";

        // 调用的接口名，无需修改
        public static string service = "alipay.wap.create.direct.pay.by.user";

        //↑↑↑↑↑↑↑↑↑↑请在这里配置您的基本信息↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

    }
}