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
using LokFu.PayMent.ALF2FPAY;
using LokFu.Extensions;
using LokFu.PayMent.ALIPAY;
namespace LokFu.Areas.Pay.Controllers
{
    public class AliPayController : BaseController
    {
        //接止返回数据
        public void Notice()
        {
            //商户订单号
            string out_trade_no = Request.Form["out_trade_no"];
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == out_trade_no);
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
            string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 商户号,密钥,支付宝号
            if (PayConfigArr.Length != 3)
            {
                Response.Write("E1");
                return;
            }
            string AlipayVer = PayConfig.Version;

            string PId = PayConfigArr[0];
            string Key = PayConfigArr[1];
            Notify Notify = null;
            Notify = new Notify();
            Notify.pid = PId;
            Notify.key = Key;
            SortedDictionary<string, string> sPara = new SortedDictionary<string,string>();
            sPara = Notify.GetRequestPost();
            string AllString = Request.Form.ToString();
            AllString = System.Web.HttpUtility.UrlDecode(AllString);
            if (AlipayVer == "V2.0")
            {
                ALF2FPAY ALF2FPAY = new ALF2FPAY();
                Notify.sign_type = ALF2FPAY.sign_type;
                Notify.alipay_public_key = ALF2FPAY.alipay_public_key;
            }
            if (sPara.Count > 0)//判断是否有带返回参数
            {
                //支付宝交易号
                string trade_no = Request.Form["trade_no"];
                //交易状态
                string trade_status = Request.Form["trade_status"];
                //交易金额
                string total_fee = Request.Form["total_fee"];
                if (total_fee.IsNullOrEmpty())
                {
                    total_fee = Request.Form["total_amount"];
                }
                if (total_fee.IsNullOrEmpty())
                {
                    total_fee = "0";
                }
                decimal Amoney = decimal.Parse(total_fee);
                //================================================
                //记录通知信息
                PayLog PayLog = new PayLog();
                PayLog.PId = PayConfig.Id;
                PayLog.OId = out_trade_no;
                PayLog.TId = trade_no;
                PayLog.Amount = Amoney;
                PayLog.Way = "POST";
                PayLog.AddTime = DateTime.Now;
                PayLog.Data = AllString;
                PayLog.State = 1;
                Entity.PayLog.AddObject(PayLog);
                Entity.SaveChanges();
                //================================================
                string notify_id = sPara["notify_id"];
                string sign = sPara["sign"];
                bool verifyResult = Notify.Verify(sPara, notify_id, sign);
                if (verifyResult)//验证成功
                {
                    if (trade_status == "TRADE_FINISHED")
                    {
                        //退款及关闭处理
                        //支付完成超退款时间
                    }
                    else if (trade_status == "TRADE_SUCCESS")
                    {
                        //付款完成后
                        if (Orders.Amoney > Amoney)
                        {
                            Response.Write("E5");
                            return;
                        }
                        Orders = Orders.PaySuccess(Entity);
                    }
                    else
                    {
                    }
                    Response.Write("success");  //请不要修改或删除
                }
                else//验证失败
                {
                    Response.Write("fail");
                }
            }
            else
            {
                Response.Write("无通知参数");
            }
        }

        //支付宝直接支付
        public void AlipayNotice()
        {
            SortedDictionary<string, string> sPara = new SortedDictionary<string, string>();
           
            NameValueCollection coll = Request.Form;
            String[] requestItem = coll.AllKeys;
            int i = 0;
            for (i = 0; i < requestItem.Length; i++)
            {
                sPara.Add(requestItem[i], Request.Form[requestItem[i]]);
            }
            if (sPara.Count > 0)
            {
                string out_trade_no = Request.Form["out_trade_no"];//商户订单号
                string trade_no = Request.Form["trade_no"];//支付宝交易号
                Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == out_trade_no);
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
                string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 商户号,密钥,支付宝号
                if (PayConfigArr.Length != 3)
                {
                    Response.Write("E1");
                    return;
                }
                if (Orders.TType != 6 && Orders.TType != 10) 
                {
                    Response.Write("E1");
                    return;
                }
                if (Orders.PayState == 1)//已支付
                {
                    Response.Write("0000");
                    return;
                }
                //================================================
                //记录通知信息
                string AllString = Request.Form.ToString();
                AllString = System.Web.HttpUtility.UrlDecode(AllString);
                PayLog PayLog = new PayLog();
                PayLog.PId = PayConfig.Id;
                PayLog.OId = out_trade_no;
                PayLog.TId = trade_no;
                PayLog.Amount =Orders.Amoney;
                PayLog.Way = "POST";
                PayLog.AddTime = DateTime.Now;
                PayLog.Data = AllString;
                PayLog.State = 1;
                Entity.PayLog.AddObject(PayLog);

                Entity.SaveChanges();
                //================================================
                Dictionary<string, string> Dic = Utils.FilterPara(sPara, "sign|sign_type");
                string SignString = Utils.CreateLinkString(Dic);
                string sign = sPara["sign"];
                string sign_type = sPara["sign_type"];
                AliPayCom Com = new AliPayCom();
                string app_id = PayConfigArr[2];
                Com.app_id = app_id;
                //==========================
                //读取公钥
                string FilePath = AppDomain.CurrentDomain.BaseDirectory;
                string file = FilePath + "certs\\";
                //==========================
                //对异步通知进行验签
                if (Com.CheckSign(SignString, sign))
                {
                    string trade_status = sPara["trade_status"];
                    if (trade_status == "TRADE_FINISHED")
                    {
                        //退款及关闭处理
                        //支付完成超退款时间
                    }
                    else if (trade_status == "TRADE_SUCCESS")
                    {
                        //付款完成后
                        Orders = Orders.PaySuccess(Entity);
                    }
                    else
                    {

                    }
                    Response.Write("success");//请不要修改或删除
                }
                else
                {
                    Response.Write("E99");
                }
            }
            else
            {
                Response.Write("E0");
                return;
            }
        }
       
    }
}
 