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
using LokFu.Extensions;
using System.IO;
using LokFu.PayMent.WxPayAPI;
using System.Configuration;
namespace LokFu.Areas.Pay.Controllers
{
    public class WeiXinPayController : BaseController
    {
        //接止返回数据
        public void Notice()
        {
            Stream InputStream = Request.InputStream;
            Notify Notify=new Notify();
            WxPayData notifyData = Notify.GetNotifyData(InputStream);
            notifyData.SaveLog(Entity);
            WxPayData res = new WxPayData();
            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                Response.Write(res.ToXml());
                return;
            }
            string transaction_id = notifyData.GetValue("transaction_id").ToString();
            string outtradeno = notifyData.GetValue("out_trade_no").ToString();

            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == outtradeno);
            if (Orders == null)
            {
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单不存在");
                Response.Write(res.ToXml());
                return;
            }
            PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == Orders.PayWay && n.State == 1);
            if (PayConfig == null)
            {
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付通道关闭或配置不正确");
                Response.Write(res.ToXml());
                return;
            }
            WxPayConfig WxPayConfig = new WxPayConfig();
            string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 appid,mchid,key,appsecret
            if (PayConfigArr.Length != 4 && PayConfigArr.Length != 5)
            {
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付通道配置不正确");
                Response.Write(res.ToXml());
                return;
            }
            if (PayConfigArr.Length == 5)
            {
                WxPayConfig.SubMCHID = PayConfigArr[4];
            }
            string ServerIp = ConfigurationManager.AppSettings["ServerIp"].ToString();
            WxPayConfig.IP = ServerIp;
            WxPayConfig.APPID = PayConfigArr[0];
            WxPayConfig.MCHID = PayConfigArr[1];
            WxPayConfig.KEY = PayConfigArr[2];
            WxPayConfig.APPSECRET = PayConfigArr[3];
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData resp = WxPayApi.OrderQuery(req, WxPayConfig);
            resp.SaveLog(Entity);
            if (resp.GetValue("return_code").ToString() == "SUCCESS" && resp.GetValue("result_code").ToString() == "SUCCESS")
            {
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                Response.Write(res.ToXml());
                //支付成功
                if (resp.GetValue("trade_state").ToString() == "SUCCESS")
                {
                    string out_trade_no = resp.GetValue("out_trade_no").ToString();
                    if (out_trade_no != outtradeno) {
                        res.SetValue("return_code", "FAIL");
                        res.SetValue("return_msg", "订单信息有误");
                        Response.Write(res.ToXml());
                        return;
                    }
                    OrderF2F Orderf2f = Entity.OrderF2F.FirstOrDefault(n => n.OId == out_trade_no);
                    if (Orderf2f == null)
                    {
                        res.SetValue("return_code", "FAIL");
                        res.SetValue("return_msg", "订单不存在");
                        Response.Write(res.ToXml());
                        return;
                    }
                    Orders = Orders.PaySuccess(Entity);
                }
            }
            else
            {
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Response.Write(res.ToXml());
            }
        }
    }
}
 