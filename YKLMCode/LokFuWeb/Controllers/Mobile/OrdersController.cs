using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LokFu.PayMent.ALIPAY;
using System.Collections.Generic;
namespace LokFu.Areas.Mobile.Controllers
{
    public class OrdersController : BaseController
    {
        public ActionResult Show(string data, string eno)
        {
            if (data.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "data参数错误！";
                return View("Error");
            }
            if (eno.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "eno参数错误！";
                return View("Error");
            }
            Equipment Equipment = Entity.Equipment.FirstOrDefault(n => n.No == eno);
            if (Equipment == null)
            {
                ViewBag.ErrorMsg = "设备故障！";
                return View("Error");
            }
            string Key = Equipment.Keys;
            //string Data = HttpUtility.UrlDecode(data, Encoding.UTF8);
            string Json = LokFuEncode.LokFuAPIDecode(data, Key);
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Json);
            }
            catch (Exception Ex)
            {
            }
            if (json == null)
            {
                ViewBag.ErrorMsg = "json参数错误！";
                return View("Error");
            }
            Orders order = new Orders();
            order = JsonToObject.ConvertJsonToModel(order, json);
            if (order.TNum.IsNullOrEmpty() || order.Token.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "json参数错误[02]！";
                return View("Error");
            }
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == order.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                ViewBag.ErrorMsg = "用户不存在或登录信息有误！";
                return View("Error");
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                ViewBag.ErrorMsg = "用户被锁定！";
                return View("Error");
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                ViewBag.ErrorMsg = "用户未实名认证！";
                return View("Error");
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                ViewBag.ErrorMsg = "用户未设置支付密码！";
                return View("Error");
            }
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == order.TNum);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "交易不存在！";
                return View("Error");
            }
            if (baseUsers.Id != Orders.UId && baseUsers.Id != Orders.RUId)//禁止代付
            {
                ViewBag.ErrorMsg = "不能查看他人订单！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.Users = baseUsers;
            ViewBag.PayUrl = PayPath + "/PayCenter/Pay/index.html?data=" + HttpUtility.UrlEncode(data) + "&eno=" + eno;
            return View();
        }

        #region 支付
        public void GoPay(string tnum,string sign)
        {
            if (tnum.IsNullOrEmpty() || sign.IsNullOrEmpty())
            {
                Response.Redirect("/mobile/error.html?msg=参数错误[01]");
                return;
            }
            if ((tnum + "NewPay").GetMD5().Substring(8, 8) != sign)
            {
                Response.Redirect("/mobile/error.html?msg=参数错误[00]");
                return;
            }
            Orders Orders = Entity.Orders.FirstOrDefault(o => o.TNum == tnum);
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Orders.UId);
            if (baseUsers == null)//用户不存在
            {
                Response.Redirect("/mobile/error.html?msg=用户不存在或信息有误");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                Response.Redirect("/mobile/error.html?msg=用户被锁定");
                return;
            }
            if (Orders== null)
            {
                Response.Redirect("/mobile/error.html?msg=订单不存在");
                return;
            }
            //if (Orders.InState != 1)
            //{
            //    Response.Redirect("/mobile/error.html?msg=订单状态异常");
            //    return;
            //}
            if (Orders.TType != 6 && Orders.TType != 10) 
            {
                Response.Redirect("/mobile/error.html?msg=订单类型错误");
                return;
            }
            if (Orders.PayState == 1)
            {
                Response.Redirect("/mobile/error.html?msg=订单已支付");
                return;
            }
            PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.DllName == "AliPay");
            if (PayConfig == null)
            {
                Response.Redirect("/mobile/error.html?msg=无支付通道");
                return;
            }
            
            //获取支付方式并提交
            string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 商户号,密钥，支付宝号
            if (PayConfigArr.Length != 3)
            {
                Response.Redirect("/mobile/error.html?msg=配置错误");
                return;
            }
            AliPayCom Com = new AliPayCom();
            string app_id = PayConfigArr[2];
            Com.app_id = app_id;
            Com.method = "alipay.trade.precreate";
            Com.notify_url = PayPath + "/paycenter/alipay/alipaynotice.html";
            Dictionary<string, string> Dic = new Dictionary<string, string>();
            Dic.Add("out_trade_no", Orders.TNum);
            Dic.Add("total_amount", Orders.Amoney.ToMoney());
            Dic.Add("timeout_express ", "30m");
            string subject = "用户升级VIP";
            if (Orders.TType == 10)
            {
                subject = "用户升级代理商";
            }
            Dic.Add("subject", subject);
            string response = Com.Send(Dic);
            JObject JObj = (JObject)JsonConvert.DeserializeObject(response);
            JObject Obj = (JObject)JObj["alipay_trade_precreate_response"];
            string code = Obj["code"].ToString();
            if (code == "10000")
            {
                string qr_code = Obj["qr_code"].ToString();
                Response.Redirect(qr_code);
            }
            else
            {
                string msg = Obj["msg"].ToString();
                Response.Redirect("/mobile/error.html?msg=" + msg);
            }
        }
        #endregion
    }
}
