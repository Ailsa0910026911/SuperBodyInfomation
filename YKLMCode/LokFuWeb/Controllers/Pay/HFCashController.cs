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
using System.Configuration;
using System.Threading;
namespace LokFu.Areas.Pay.Controllers
{
    public class HFCashController : BaseController
    {
        public void Notice()
        {
            string merId = ConfigurationManager.AppSettings["Cash_merId"].ToString();//商户号
            string merKey = ConfigurationManager.AppSettings["Cash_merKey"].ToString();//商户密钥
            string Resp = Request.Form["resp"];
            string Sign = Request.Form["sign"];
            string MD5Str = Resp + merKey;
            string sign = MD5Str.GetMD5();
            if (Sign != sign)
            {
                Response.Write("E2");
                return;
            }
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
            //================================================
            PayLog PayLog = new PayLog();
            PayLog.PId = 0;
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
            if (merId != merid)
            {
                Response.Write("E1");
                return;
            }
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == orderid);
            if (Orders == null)
            {
                Response.Write("E4");
                return;
            }
            if (resultcode == "0000")
            {
                if (Orders.TType == 2)
                {
                    OrderCash OrderCash = Entity.OrderCash.FirstOrDefault(n => n.OId == orderid);
                    if (OrderCash.FState == 0)
                    {
                        OrderCash.PayState = 2;
                        OrderCash.FState = 1;
                        OrderCash.FTime = DateTime.Now;
                        Orders.PayState = 2;
                        if (OrderCash.AgentCashGet > 0)
                        {
                            Orders.RunSplit = 1;
                        }
                        Entity.SaveChanges();
                        Orders.SendMsg(Entity);//发送消息类
                    }
                }
                if (Orders.TType == 5)
                {
                    OrderHouse OrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.OId == orderid);
                    if (OrderHouse.FState == 0)
                    {
                        OrderHouse.PayState = 2;
                        OrderHouse.FState = 1;
                        OrderHouse.FTime = DateTime.Now;
                        Orders.PayState = 2;
                        if (OrderHouse.AIdPayGet > 0)
                        {
                            Orders.RunSplit = 1;
                        }
                        Entity.SaveChanges();
                        //======分润======
                        Orders.SendMsg(Entity);//发送消息类
                    }
                }
            }else if (resultcode == "2001"){

            }else{
                if (Orders.TType == 2)
                {
                    if (Orders.PayState != 4 && Orders.PayState !=3)
                    {
                        OrderCash OrderCash = Entity.OrderCash.FirstOrDefault(n => n.OId == orderid);

                        OrderCash.PayState = 3;
                        OrderCash.Remark = resultmsg;
                        Orders.PayState = 3;
                        Orders.Remark = resultmsg;
                        Entity.SaveChanges();
                        Orders.SendMsg(Entity);//发送消息类
                    }
                }
                if (Orders.TType == 5)
                {

                    if (Orders.PayState != 4 && Orders.PayState != 3)
                    {
                        OrderHouse OrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.OId == orderid);

                        OrderHouse.PayState = 3;
                        OrderHouse.Remark = resultmsg;
                        Orders.PayState = 3;
                        Orders.Remark = resultmsg;
                        Entity.SaveChanges();
                        Orders.SendMsg(Entity);//发送消息类
                    }
                }
            }
            Response.Write("0000");
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

            var FastOrderChange = Entity.FastOrderChange.FirstOrDefault(o => o.STNum == orderid);
            if (FastOrderChange != null) {
                orderid = FastOrderChange.TNum;
            }
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == orderid);
            if (FastOrder == null)
            {
                Response.Write("E1");
                return;
            }
            if (FastOrder.State != 1)
            {
                Response.Write("E2");
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
                Response.Write("E4");
                return;
            }
            if (MerId != merid)
            {
                Response.Write("E5");
                return;
            }
            int factmoney = int.Parse(txnamt);
            if (((int)(FastOrder.PayMoney * 100)) != factmoney)
            {
                Response.Write("E7");
                return;
            }
            if (resultcode == "0000")
            {
                FastOrder.UserState = 1;
                if(FastOrderChange != null)
                {
                    FastOrderChange.State = 1;
                }
            }
            else if (resultcode == "2002" || resultcode == "2003")
            {
                FastOrder.UserState = 2;
            }
            else
            {

            }
            Entity.SaveChanges();
            Response.Write("0000");
        }
    }
}
 