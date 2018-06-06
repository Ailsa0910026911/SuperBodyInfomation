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
    public class UnionpayController : BaseController
    {
        public ActionResult Result()
        {
            // 使用Dictionary保存参数
            Dictionary<string, string> resData = new Dictionary<string, string>();
            NameValueCollection coll = Request.Form;
            string[] requestItem = coll.AllKeys;
            for (int i = 0; i < requestItem.Length; i++)
            {
                resData.Add(requestItem[i], Request.Form[requestItem[i]]);
            }
            string merId = resData["merId"];//商户号
            string orderId = resData["orderId"];//商户订单号
            string queryId = resData["queryId"];//交易查询流水号
            string txnAmt = resData["txnAmt"];//交易金额
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == orderId);
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
            //================================================
            PayLog PayLog = new PayLog();
            PayLog.PId = PayConfig.Id;
            PayLog.OId = orderId;
            PayLog.TId = queryId;
            PayLog.Amount = decimal.Parse(txnAmt) / 100;
            PayLog.Way = "GET";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Request.Form.ToString();
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            // 返回报文中不包含UPOG,表示Server端正确接收交易请求,则需要验证Server端返回报文的签名
            if (!SDKUtil.Validate(resData, Encoding.UTF8))
            {
                ViewBag.ErrorMsg = "签名错误！";
                return View("Error");
            }
            //================================================
            string[] strArray = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 商户号,终端号,密钥
            string MemberID = strArray[0];//商户号
            if (merId != MemberID)
            {
                ViewBag.ErrorMsg = "支付信息有误！";
                return View("Error");
            }
            string respCode = resData["respCode"];//响应码
            if (respCode != "00")
            {
                ViewBag.ErrorMsg = "支付失败！[" + respCode + "]";
                return View("Error");
            }
            //string respMsg = resData["respMsg"];//应答信息
            //if (respMsg != "success")
            //{
            //    ViewBag.ErrorMsg = "支付失败！[" + respMsg + "]";
            //    return View("Error");
            //}
            
            int factmoney = int.Parse(txnAmt);
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
            // 使用Dictionary保存参数
            Dictionary<string, string> resData = new Dictionary<string, string>();
            NameValueCollection coll = Request.Form;
            string[] requestItem = coll.AllKeys;
            for (int i = 0; i < requestItem.Length; i++)
            {
                string formvalue = Request.Form[requestItem[i]];
                if (formvalue.IndexOf("%") != -1)
                {
                    formvalue = HttpUtility.UrlDecode(formvalue, Encoding.UTF8);
                }
                resData.Add(requestItem[i], formvalue);
            }
            string merId = resData["merId"];//商户号
            string orderId = resData["orderId"];//商户订单号
            string queryId = resData["queryId"];//交易查询流水号
            string txnAmt = resData["txnAmt"];//交易金额
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == orderId);
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
            //================================================
            PayLog PayLog = new PayLog();
            PayLog.PId = PayConfig.Id;
            PayLog.OId = orderId;
            PayLog.TId = queryId;
            PayLog.Amount = decimal.Parse(txnAmt) / 100;
            PayLog.Way = "POST";
            PayLog.AddTime = DateTime.Now;
            PayLog.Data = Request.Form.ToString();
            PayLog.State = 1;
            Entity.PayLog.AddObject(PayLog);
            Entity.SaveChanges();
            //================================================
            // 返回报文中不包含UPOG,表示Server端正确接收交易请求,则需要验证Server端返回报文的签名
            if (!SDKUtil.Validate(resData, Encoding.UTF8))
            {
                Response.Write("E2");
                return;
            }
            //================================================
            string[] strArray = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 商户号,终端号,密钥
            string MemberID = strArray[0];//商户号
            if (merId != MemberID)
            {
                Response.Write("E1");
                return;
            }
            string respCode = resData["respCode"];//响应码
            if (respCode != "00")
            {
                Response.Write("E3");
                return;
            }
            //string respMsg = resData["respMsg"];//应答信息
            //if (respMsg != "success")
            //{
            //    Response.Write("E4");
            //    return;
            //}
            int factmoney = int.Parse(txnAmt);
            if (((int)(Orders.Amoney * 100)) > factmoney)
            {
                Response.Write("E5");
                return;
            }
            Orders = Orders.PaySuccess(Entity);
            Response.Write("OK");
        }
    }
}
 