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
namespace LokFu.Areas.Pay.Controllers
{
    public class HFNFCController : BaseController
    {
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
    }
}
 