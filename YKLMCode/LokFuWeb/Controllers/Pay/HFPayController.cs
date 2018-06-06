using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;
using System.Web;
using LokFu.Infrastructure;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using LokFu.Payment.Unionpay;
using LokFu.Extensions;
namespace LokFu.Areas.Pay.Controllers
{
    public class HFPayController : BaseController
    {
        private string DllName = "HFPay";

        public ActionResult Index(string etnum)
        {
            if (etnum.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "Some Error[00]";
                return View("Error");
            }
            string tnum = LokFuEncode.LokFuAPIDecode(etnum, DllName);
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == tnum);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "Some Error[02]";
                return View("Error");
            }
            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastOrder.PayWay);
            if (FastPayWay == null)
            {
                ViewBag.ErrorMsg = "Some Error[03]";
                return View("Error");
            }
            if (FastPayWay.DllName != DllName)
            {
                ViewBag.ErrorMsg = "Some Error[04]";
                return View("Error");
            }
            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            if (PayConfigArr.Length != 3)
            {
                ViewBag.ErrorMsg = "Some Error[05]";
                return View("Error");
            }
            FastUser FastUser = Entity.FastUser.FirstOrNew(n => n.UId == FastOrder.UId);

            string merId = PayConfigArr[0];//商户号
            string merKey = PayConfigArr[1];//商户密钥
            string PayWay = PayConfigArr[2];//绑定通道

            string orderId = FastOrder.TNum;//商户流水号
            string OrderMoney = (FastOrder.Amoney * 100).ToString("F0");//金额，以分为单

            string TrueName = FastUser.TrueName;
            string CardId = FastUser.CardId;

            //填写参数
            string ReturnUrl = PayPath + "/PayCenter/HFPay/FastResult.html";//支付成功跳转页
            string BackUrl = NoticePath + "/PayCenter/HFPay/FastNotice.html";//后台通过地址


            if (TrueName.IsNullOrEmpty() || CardId.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "TrueName IsNull";
                return View("Error");
            }
            if (FastPayWay.Id == 8)
            {
                PayWay = "3855";
            }
            //if (FastPayWay.Id == 13)
            //{
            //    PayWay = "3852";
            //}

            string Data = "{\"action\":\"goAndPay\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"payway\":\"" + PayWay + "\",\"orderid\":\"" + orderId + "\",\"backurl\":\"" + BackUrl + "\",\"fronturl\":\"" + ReturnUrl + "\",\"accname\":\"" + TrueName + "\",\"accno\":\"" + CardId + "\"}";

            string DataBase64 = LokFuEncode.Base64Encode(Data, "utf-8");
            string Sign = (DataBase64 + merKey).GetMD5();

            string GoUrl = "https://api.zhifujiekou.com/apis/gateway?";
            GoUrl += "req=" + HttpUtility.UrlEncode(DataBase64);
            GoUrl += "&sign=" + Sign;

            Response.Redirect(GoUrl);

            return View("Null");
        }

        public ActionResult Result()
        {
            string Resp = Request.QueryString["resp"];
            string Sign = Request.QueryString["sign"];
            string SignStr = Resp;
            Resp = LokFuEncode.Base64Decode(Resp, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Resp);
            }
            catch (Exception Ex)
            {
                ViewBag.ErrorMsg = Ex.ToString();
                return View("Error");
            }
            if (json == null)
            {
                ViewBag.ErrorMsg = "数据处理出错";
                return View("Error");
            }
            string resultcode = json["resultcode"].ToString();//交易结果码
            string resultmsg = json["resultmsg"].ToString();//交易结果信息
            string queryid = json["queryid"].ToString();//交易流水号
            string txnamt = json["txnamt"].ToString();//交易金额\
            string merid = json["merid"].ToString();//交易金额
            string orderid = json["orderid"].ToString();//交易金额

            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == orderid);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "订单信息有误！";
                return View("Error");
            }
            PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == Orders.PayWay && n.State == 1);
            if (PayConfig == null)
            {
                ViewBag.ErrorMsg = "支付通道已关闭！";
                return View("Error");
            }
            string ConfigStr = PayConfig.QueryArray;
            string[] ConfigArr = ConfigStr.Split(',');
            string merId = ConfigArr[0];
            string merKey = ConfigArr[1];
            string MD5Str = SignStr + merKey;
            string sign = MD5Str.GetMD5();
            //================================================
            PayLog PayLog = new PayLog();
            PayLog.PId = PayConfig.Id;
            PayLog.OId = orderid;
            PayLog.TId = queryid;
            PayLog.Amount = decimal.Parse(txnamt) / 100;
            PayLog.Way = "GET";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Request.QueryString.ToString();
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            if (Sign != sign)
            {
                ViewBag.ErrorMsg = "签名错误！";
                return View("Error");
            }
            if (merId != merid)
            {
                ViewBag.ErrorMsg = "商户号不一置！";
                return View("Error");
            }
            if (resultcode != "0000" && resultcode != "1002")
            {
                ViewBag.ErrorMsg = "支付失败！[" + resultcode + "]" + resultmsg;
                return View("Error");
            }
            //string respMsg = resData["respMsg"];//应答信息
            //if (respMsg != "success")
            //{
            //    ViewBag.ErrorMsg = "支付失败！[" + respMsg + "]";
            //    return View("Error");
            //}
            int factmoney = int.Parse(txnamt);
            if (((int)(Orders.Amoney * 100)) > factmoney)
            {
                ViewBag.ErrorMsg = "支付金额与交易金额不符！";
                return View("Error");
            }
            Orders = Orders.PaySuccess(Entity);
            ViewBag.Orders = Orders;
            return View("Success");
        }
        public void Notice()
        {
            
            string Resp = Request.Form["resp"];
            string Sign = Request.Form["sign"];
            string SignStr = Resp;
            Resp = LokFuEncode.Base64Decode(Resp, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Resp);
            }
            catch (Exception Ex)
            {
                Response.Write(Ex.ToString());
                return;
            }
            if (json == null)
            {
                Response.Write("Json Null");
                return;
            }
            string resultcode = json["resultcode"].ToString();//交易结果码
            string resultmsg = json["resultmsg"].ToString();//交易结果信息
            string queryid = json["queryid"].ToString();//交易流水号
            string txnamt = json["txnamt"].ToString();//交易金额\
            string merid = json["merid"].ToString();//交易金额
            string orderid = json["orderid"].ToString();//交易金额

            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == orderid);
            if (Orders == null)
            {
                Response.Write("E4");
                return;
            }
            PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == Orders.PayWay && n.State == 1);
            if (PayConfig == null)
            {
                Response.Write("E0");
                return;
            }
            string ConfigStr = PayConfig.QueryArray;
            string[] ConfigArr = ConfigStr.Split(',');
            string merId = ConfigArr[0];
            string merKey = ConfigArr[1];
            string MD5Str = SignStr + merKey;
            string sign = MD5Str.GetMD5();
            //================================================
            PayLog PayLog = new PayLog();
            PayLog.PId = PayConfig.Id;
            PayLog.OId = orderid;
            PayLog.TId = queryid;
            PayLog.Amount = decimal.Parse(txnamt) / 100;
            PayLog.Way = "POST";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Request.Form.ToString();
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            if (Sign != sign)
            {
                Response.Write("E2");
                return;
            }
            if (merId != merid)
            {
                Response.Write("E1");
                return;
            }
            if (resultcode != "0000" && resultcode != "1002")
            {
                Response.Write("E3");
                return;
            }
            int factmoney = int.Parse(txnamt);
            if (((int)(Orders.Amoney * 100)) > factmoney)
            {
                Response.Write("E5");
                return;
            }
            Orders = Orders.PaySuccess(Entity);
            Response.Write("0000");
        }


        public ActionResult FastResult()
        {
            string Resp = Request.QueryString["resp"];
            string Sign = Request.QueryString["sign"];
            string SignStr = Resp;
            Resp = LokFuEncode.Base64Decode(Resp, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Resp);
            }
            catch (Exception Ex)
            {
                ViewBag.ErrorMsg = Ex.ToString();
                return View("Error");
            }
            if (json == null)
            {
                ViewBag.ErrorMsg = "数据处理出错";
                return View("Error");
            }

            string resultcode = json["resultcode"].ToString();//交易结果码
            string resultmsg = json["resultmsg"].ToString();//交易结果信息
            string queryid = json["queryid"].ToString();//交易流水号
            string txnamt = json["txnamt"].ToString();//交易金额
            string merid = json["merid"].ToString();//商户号
            string orderid = json["orderid"].ToString();//交易单号

            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == orderid);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "交易不存在！";
                return View("Error");
            }
            if (FastOrder.State != 1)
            {
                ViewBag.ErrorMsg = "交易异常！";
                return View("Error");
            }
            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastOrder.PayWay);
            if (FastPayWay == null)
            {
                ViewBag.ErrorMsg = "支付通道不存在！";
                return View("Error");
            }
            if (FastPayWay.State != 1)
            {
                ViewBag.ErrorMsg = "支付通道已关闭！";
                return View("Error");
            }
            if (FastPayWay.DllName != "HFPay")
            {
                ViewBag.ErrorMsg = "支付通道异常！";
                return View("Error");
            }
            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            if (PayConfigArr.Length != 3)
            {
                ViewBag.ErrorMsg = "支付通道配置异常！";
                return View("Error");
            }
            string MerId = PayConfigArr[0];
            string MerKey = PayConfigArr[1];

            string MD5Str = SignStr + MerKey;
            string sign = MD5Str.GetMD5();
            //================================================
            //这里记录日志
            PayLog PayLog = new PayLog();
            PayLog.PId = (int)FastOrder.PayWay;
            PayLog.OId = FastOrder.TNum;
            PayLog.TId = FastOrder.Trade;
            PayLog.Amount = decimal.Parse(txnamt) / 100;
            PayLog.Way = "POST";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Request.Form.ToString();
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            if (Sign != sign)
            {
                ViewBag.ErrorMsg = "签名错误！";
                return View("Error");
            }
            if (MerId != merid)
            {
                ViewBag.ErrorMsg = "商户号不一置！";
                return View("Error");
            }
            if (resultcode != "0000" && resultcode != "1002")
            {
                ViewBag.ErrorMsg = "支付失败！[" + resultcode + "]" + resultmsg;
                return View("Error");
            }
            int factmoney = int.Parse(txnamt);
            if (((int)(FastOrder.Amoney * 100)) != factmoney)
            {
                ViewBag.ErrorMsg = "支付金额与交易金额不符！";
                return View("Error");
            }
            FastOrder.Trade = queryid;
            Entity.SaveChanges();

            FastOrder = FastOrder.PaySuccess(Entity);
            ViewBag.FastOrder = FastOrder;
            return View("FastSuccess");
        }

        public void FastNotice()
        {
            string Resp = Request.Form["resp"];
            string Sign = Request.Form["sign"];
            string SignStr = Resp;
            Resp = LokFuEncode.Base64Decode(Resp, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Resp);
            }
            catch (Exception Ex)
            {
                Response.Write(Ex.ToString());
                return;
            }
            if (json == null)
            {
                Response.Write("Json Null");
                return;
            }
            string resultcode = json["resultcode"].ToString();//交易结果码
            string resultmsg = json["resultmsg"].ToString();//交易结果信息
            string queryid = json["queryid"].ToString();//交易流水号
            string txnamt = json["txnamt"].ToString();//交易金额\
            string merid = json["merid"].ToString();//交易金额
            string orderid = json["orderid"].ToString();//交易金额

            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == orderid);
            if (FastOrder == null)
            {
                Response.Write("E1");
                return;
            }
            if (FastOrder.PayState == 1)
            {
                Response.Write("P1");
                return;
            }
            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastOrder.PayWay);
            if (FastPayWay == null)
            {
                Response.Write("E3");
                return;
            }
            if (FastPayWay.State != 1)
            {
                Response.Write("E4");
                return;
            }
            if (FastPayWay.DllName != "HFPay")
            {
                Response.Write("E5");
                return;
            }
            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            if (PayConfigArr.Length != 3)
            {
                Response.Write("E53");
                return;
            }
            string MerId = PayConfigArr[0];
            string MerKey = PayConfigArr[1];

            string MD5Str = SignStr + MerKey;
            string sign = MD5Str.GetMD5();
            //================================================
            //这里记录日志
            PayLog PayLog = new PayLog();
            PayLog.PId = (int)FastOrder.PayWay;
            PayLog.OId = FastOrder.TNum;
            PayLog.TId = FastOrder.Trade;
            PayLog.Amount = decimal.Parse(txnamt) / 100;
            PayLog.Way = "POST";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Request.Form.ToString();
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            if (Sign != sign)
            {
                Response.Write("E4");
                return;
            }
            if (MerId != merid)
            {
                Response.Write("E5");
                return;
            }
            if (resultcode != "0000" && resultcode != "1002")
            {
                Response.Write("E6");
                return;
            }
            int factmoney = int.Parse(txnamt);
            if (((int)(FastOrder.Amoney * 100)) != factmoney)
            {
                Response.Write("E7");
                return;
            }
            FastOrder.Trade = queryid;
            Entity.SaveChanges();

            FastOrder = FastOrder.PaySuccess(Entity);
            ViewBag.FastOrder = FastOrder;

            Response.Write("0000");
        }
    }
}
 