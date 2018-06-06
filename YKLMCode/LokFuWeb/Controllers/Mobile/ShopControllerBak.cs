using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using LokFu.Infrastructure;
using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.WeiXin;
using LokFu.PayMent.WxPayAPI;
using LokFu.WeiXin.Repositories;
namespace LokFu.Areas.Mobile.Controllers
{
    public class ShopController : BaseController
    {
        public bool IsWeiXinBrowser = false;
        public bool IsAlipayBrowser = false;
        public string Shop_ENO = "00000000-0000-0000-0000-00000001";
        public string Shop_Keys = "120df9d3b7cf977e9b5185d6a123f3fe120df9d3b7cf977e9b5185d6a123f3fe";
        public bool IsIPhone = false;
        public ShopController()
        {
            IsWeiXinBrowser = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("micromessenger");
            IsAlipayBrowser = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("alipayclient");
            IsIPhone = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("iphone") || System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("ipad");
            ViewBag.IsWeiXinBrowser = IsWeiXinBrowser;
            ViewBag.IsAlipayBrowser = IsAlipayBrowser;
            ViewBag.IsIPhone = IsIPhone;
        }
        public ActionResult Index(Users Users)
        {
            if (!Users.Id.IsNullOrEmpty())
            {
                Users = Entity.Users.FirstOrNew(n => n.Id == Users.Id);
            }
            ViewBag.Users = Users;
            IList<SysControl> SysControlList = new List<SysControl>();
            if (IsWeiXinBrowser)
            {
                SysControlList = Entity.SysControl.Where(n => n.State == 1 && n.Tag == "WeiXin").OrderBy(n => n.Sort).ToList();
            }
            else if (IsAlipayBrowser)
            {
                SysControlList = Entity.SysControl.Where(n => n.State == 1 && n.Tag == "Alipay").OrderBy(n => n.Sort).ToList();
            }
            else
            {
                SysControlList = Entity.SysControl.Where(n => n.State == 1 && n.Tag == "RecMoneyMulti").OrderBy(n => n.Sort).ToList();
            }
            DateTime Now = DateTime.Now;
            DateTime ToDay = DateTime.Parse(Now.ToString("yyyy-MM-dd"));
            IList<SysControl> List = new List<SysControl>();
            foreach (var p in SysControlList)
            {
                SysControl SC = p.GetState();
                if (SC.State == 1)
                {
                    SC.CName = p.CName;
                    SC.PayWay = p.PayWay;
                    List.Add(SC);
                }
                //if (p.TimeType == 0) {
                //    List.Add(p);
                //}
                //else if (p.TimeType == 1) {
                //    DateTime sTime = p.STime;
                //    DateTime eTime = p.ETime;
                //    DateTime STime = ToDay.AddHours(sTime.Hour).AddMinutes(sTime.Minute).AddSeconds(sTime.Second);
                //    DateTime ETime = ToDay.AddHours(eTime.Hour).AddMinutes(eTime.Minute).AddSeconds(eTime.Second);
                //    if (STime < Now && ETime > Now)
                //    {
                //        List.Add(p);
                //    }
                //}
            }
            ViewBag.SysControlList = List;
            return View();
        }
        public ActionResult Info(Users Users)
        {
            if (!Users.Id.IsNullOrEmpty())
            {
                Users = Entity.Users.FirstOrNew(n => n.Id == Users.Id);
            }
            ViewBag.Users = Users;
            IList<UserPic> UserPicList = Entity.UserPic.Where(n => n.UId == Users.Id).OrderBy(n => n.Sort).ToList();
            ViewBag.UserPicList = UserPicList;
            return View();
        }
        public ActionResult GoPay(int shopid = 0, decimal Amount = 0, byte payway = 0)
        {
            if (shopid.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "商户信息有误，请核实！";
                return View("Error");
            }
            if (Amount.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "付款金额有误，请核实！";
                return View("Error");
            }
            Users Users = Entity.Users.FirstOrNew(n => n.Id == shopid && n.State == 1 && n.CardStae == 2);
            if (Users.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "商户信息有误，请核实！";
                return View("Error");
            }
            if (Amount <= 0)
            {
                ViewBag.ErrorMsg = "付款金额有误！[00]";
                return View("Error");
            }
            string Tag = string.Empty;
            if (IsWeiXinBrowser)
            {
                Tag = "WeiXin";
            }
            else if (IsAlipayBrowser)
            {
                Tag = "Alipay";
            }
            else
            {
                Tag = "RecMoneyMulti";
            }
            SysControl SysControl = Entity.SysControl.FirstOrNew(n => n.Tag == Tag && n.Id == payway);
            SysControl syscontrol = SysControl.ChkState();
            if (syscontrol.State != 1)
            {
                ViewBag.ErrorMsg = "支付接口维护中，请使用其它支付通道！";
                return View("Error");
            }
            int SNum = syscontrol.SNum;
            int ENum = syscontrol.ENum;
            if (SysControl.Tag == "RecMoneyMulti")
            {
                if (ENum > BasicSet.CtrlMoney1)
                {
                    ENum = (int)BasicSet.CtrlMoney1;
                }
            }
            if (Amount < SNum || Amount > ENum)
            {
                ViewBag.ErrorMsg = "收款金额范围：" + SNum + "~" + ENum + "元！";
                return View("Error");
            }
            var PayConfig = Entity.PayConfig.FirstOrDefault(o=>o.Id == SysControl.PayWay);
            if(PayConfig == null)
            {
                ViewBag.ErrorMsg = "支付配置不存在";
                return View("Error");
            }
            //业务开始
            switch (PayConfig.DllName)
            {
                case "Transfer"://钱包接口
                    OrderTransfer OrderTransfer = new OrderTransfer();
                    OrderTransfer.Amoney = Amount;
                    OrderTransfer.Remark = Users.UserName;
                    ViewBag.OrderTransfer = OrderTransfer;
                    SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent && n.State == 1 && n.IsTeiPai == 1);
                    ViewBag.SysAgent = SysAgent;
                    return View("Transfer");
                case "HFAliPay"://不管直联还是结算中心，由API处理
                case "Alipay":
                    return AliPay(Users, Amount, PayConfig, SysControl);
                case "HFWeiXin"://不管直联还是结算中心，由API处理
                case "WeiXin":
                    return WeiXinPay(Users, Amount, PayConfig, SysControl);
                case "Unionpay"://走银联接口
                case "HFPay"://走银联接口
                    return BankPay(Users, Amount, PayConfig, SysControl);
                default:
                    return View();
            }
        }
        private ActionResult AliPay(Users Users, decimal Amount, PayConfig PayConfig, SysControl SysControl)
        {
            if (Users.Token.IsNullOrEmpty())
            {
                Users.Token = DateTime.Now.ToString().GetMD5();
                Entity.SaveChanges();
            }
            int InType = 0;
            if (SysControl.LagEntryNum > 0)
            {
                InType = 1;
            }

            string PostJson = "{amoney:" + Amount.ToString("F2") + ",token:\"" + Users.Token + "\",payid:\"shop\",otype:7,action:\"Create\",x:\"0\",y:\"0\",intype:\"" + InType + "\",payway:\"" + PayConfig.Id + "\",orderaddress:\"网店收银台:" + Utils.GetAddressAndIp() + "\",ip:\"" + Utils.GetIP() + "\"}";
            //提交数据
            string PostData = LokFuEncode.LokFuAPIEncode(PostJson, Shop_Keys);
            PostData = HttpUtility.UrlEncode(PostData);
            //Post参数
            string PostString = "eno=" + Shop_ENO + "&data=" + PostData + "&code=0000";
            //AppPath = "http://localhost:2610/";
            string url = AppPath + "/API/OrderQC/";
            string RetString = Utils.PostRequest(url, PostString, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(RetString);
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "接口数据有误！[01]";
                return View("Error");
            }
            if (json == null)
            {
                ViewBag.ErrorMsg = "接口数据有误！[02]";
                return View("Error");
            }
            string code = "";
            string data = "";
            try
            {
                code = json["code"].ToString();
                data = json["data"].ToString();
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "接口数据有误！[03]";
                return View("Error");
            }
            if (code != "0000")
            {
                ViewBag.ErrorMsg = "交易有误！[" + code + "]";
                return View("Error");
            }
            //解密
            string RetData = LokFuEncode.LokFuAPIDecode(data, Shop_Keys);
            JObject Json = new JObject();
            try
            {
                Json = (JObject)JsonConvert.DeserializeObject(RetData);
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "数据解析有误！[01]";
                return View("Error");
            }
            if (Json == null)
            {
                ViewBag.ErrorMsg = "数据解析有误！[02]";
                return View("Error");
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, Json);
            if (Orders.PayId.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "订单信息有误";
                return View("Error");
            }
            string Url = Orders.PayId;
            //Response.Redirect(Url);
            return this.Redirect(Url);
            //return View("GoPay");
        }
        private ActionResult WeiXinPay(Users Users, decimal Amount, PayConfig PayConfig, SysControl SysControl)
        {
            if (Users.Token.IsNullOrEmpty())
            {
                Users.Token = DateTime.Now.ToString().GetMD5();
                Entity.SaveChanges();
            }

            int InType = 0;
            if (SysControl.LagEntryNum > 0)
            {
                InType = 1;
            }
            
            #region 微信支付
            string PostJson = "{amoney:" + Amount.ToString("F2") + ",token:\"" + Users.Token + "\",payid:\"shop\",otype:8,action:\"Create\",x:\"0\",y:\"0\",intype:\"" + InType + "\",payway:\"" + PayConfig.Id + "\",orderaddress:\"网店收银台，IP:" + Utils.GetAddressAndIp() + "\",ip:\"" + Utils.GetIP() + "\"}";
            //提交数据
            string PostData = LokFuEncode.LokFuAPIEncode(PostJson, Shop_Keys);
            PostData = HttpUtility.UrlEncode(PostData);
            //Post参数
            string PostString = "eno=" + Shop_ENO + "&data=" + PostData + "&code=0000";
            //AppPath = "http://localhost:2610";
            string url = AppPath + "/API/OrderQC/";
            string RetString = Utils.PostRequest(url, PostString, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(RetString);
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "接口数据有误！[01]";
                return View("Error");
            }
            if (json == null)
            {
                ViewBag.ErrorMsg = "接口数据有误！[02]";
                return View("Error");
            }
            string code = "";
            string data = "";
            try
            {
                code = json["code"].ToString();
                data = json["data"].ToString();
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "接口数据有误！[03]";
                return View("Error");
            }
            if (code != "0000")
            {
                ViewBag.ErrorMsg = "交易有误！[" + code + "]";
                return View("Error");
            }
            //解密
            string RetData = LokFuEncode.LokFuAPIDecode(data, Shop_Keys);
            JObject Json = new JObject();
            try
            {
                Json = (JObject)JsonConvert.DeserializeObject(RetData);
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "数据解析有误！[01]";
                return View("Error");
            }
            if (Json == null)
            {
                ViewBag.ErrorMsg = "数据解析有误！[02]";
                return View("Error");
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, Json);
            if (Orders.PayId.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "订单信息有误";
                return View("Error");
            }
            string PayId = Orders.PayId;
            string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });
            if (PayConfig.DllName == "WeiXin")
            {
                string AppId = PayConfigArr[0];
                string AppKey = PayConfigArr[2];

                WxPayData jsApiParam = new WxPayData();
                jsApiParam.SetValue("appId", AppId);
                jsApiParam.SetValue("timeStamp", WxPayApi.GenerateTimeStamp());
                jsApiParam.SetValue("nonceStr", WxPayApi.GenerateNonceStr());
                jsApiParam.SetValue("package", "prepay_id=" + PayId);
                jsApiParam.SetValue("signType", "MD5");
                jsApiParam.SetValue("paySign", jsApiParam.MakeSign(AppKey));
                string Parameters = jsApiParam.ToJson();
                ViewBag.Parameters = Parameters;
                Orders = Entity.Orders.FirstOrNew(n => n.TNum == Orders.TNum);
                ViewBag.Orders = Orders;
                return View("WeiXinPay");
            }
            else if (PayConfig.DllName == "HFWeiXin")
            {
                //提交结算中心
                string merId = PayConfigArr[0];//商户号
                string merKey = PayConfigArr[1];//商户密钥
                string orderId = Orders.TNum;//商户流水号
                string myData = "{\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\",\"code\":\"" + PayId + "\"}";
                string DataBase64 = LokFuEncode.Base64Encode(myData, "utf-8");
                string Sign = (DataBase64 + merKey).GetMD5();
                DataBase64 = HttpUtility.UrlEncode(DataBase64);
                string myUrl = string.Format("req={0}&sign={1}", DataBase64, Sign);
                string Url = "https://api.zhifujiekou.com/wxjsapi/gopay.html?" + myUrl;
                //Response.Redirect(Url);
                return this.Redirect(Url);
            }
            else
            {
                return View("Null");
            }
            //return View("Null");
            #endregion
        }
        private ActionResult BankPay(Users Users, decimal Amount, PayConfig PayConfig, SysControl SysControl)
        {
            if (Users.Token.IsNullOrEmpty())
            {
                Users.Token = DateTime.Now.ToString().GetMD5();
                Entity.SaveChanges();
            }

            int InType = 0;
            if (SysControl.LagEntryNum > 0)
            {
                InType = 1;
            }

            string PostJson = "{amoney:" + Amount.ToString("F2") + ",token:\"" + Users.Token + "\",aid:2,paytype:4,x:\"0\",y:\"0\",intype:\"" + InType + "\",payway:\"" + PayConfig.Id + "\",orderaddress:\"网店收银台，IP:" + Utils.GetAddressAndIp() + "\"}";
            //提交数据
            string PostData = LokFuEncode.LokFuAPIEncode(PostJson, Shop_Keys);
            PostData = HttpUtility.UrlEncode(PostData);
            //Post参数
            string PostString = "eno=" + Shop_ENO + "&data=" + PostData + "&code=0000";
            string url = AppPath + "/API/OrderRecharge/";
            string RetString = Utils.PostRequest(url, PostString, "utf-8");
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(RetString);
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "接口数据有误！[01]";
                return View("Error");
            }
            if (json == null)
            {
                ViewBag.ErrorMsg = "接口数据有误！[02]";
                return View("Error");
            }
            string code = "";
            string data = "";
            try
            {
                code = json["code"].ToString();
                data = json["data"].ToString();
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "接口数据有误！[03]";
                return View("Error");
            }
            if (code != "0000")
            {
                ViewBag.ErrorMsg = "交易有误！[" + code + "]";
                return View("Error");
            }
            //解密
            string RetData = LokFuEncode.LokFuAPIDecode(data, Shop_Keys);
            JObject Json = new JObject();
            try
            {
                Json = (JObject)JsonConvert.DeserializeObject(RetData);
            }
            catch (Exception)
            {
                ViewBag.ErrorMsg = "数据解析有误！[01]";
                return View("Error");
            }
            if (Json == null)
            {
                ViewBag.ErrorMsg = "数据解析有误！[02]";
                return View("Error");
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, Json);
            if (Orders.TNum.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "订单信息有误";
                return View("Error");
            }
            string payJson = "{tnum:\"" + Orders.TNum + "\",token:\"" + Users.Token + "\"}";
            string JumpData = LokFuEncode.LokFuAPIEncode(payJson, Shop_Keys);
            JumpData = HttpUtility.UrlEncode(JumpData);
            string JumpString = "eno=" + Shop_ENO + "&data=" + JumpData + "&code=0000";
            string Url = "/PayCenter/Pay/index.html?" + JumpString;
            return this.Redirect(Url);
            //Response.Redirect(Url);
            //return View("GoPay");
        }


    }
}
