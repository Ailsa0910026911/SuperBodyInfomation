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
using System.Data.Objects;
namespace LokFu.Areas.Mobile.Controllers
{
    public class ShopController : BaseController
    {
        public bool IsWeiXinBrowser = false;
        public bool IsAlipayBrowser = false;
        //public string Shop_ENO = "00000000-0000-0000-0000-00000001";
        //public string Shop_Keys = "120df9d3b7cf977e9b5185d6a123f3fe120df9d3b7cf977e9b5185d6a123f3fe";
        public bool IsIPhone = false;
        public bool IsGoogle = false;
        public ShopController()
        {
            IsWeiXinBrowser = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("micromessenger");
            IsAlipayBrowser = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("alipayclient");
            IsIPhone = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("iphone") || System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("ipad");
            IsGoogle = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("android");
            ViewBag.IsWeiXinBrowser = IsWeiXinBrowser;
            ViewBag.IsAlipayBrowser = IsAlipayBrowser;
            ViewBag.IsIPhone = IsIPhone;
            ViewBag.IsGoogle = IsGoogle;
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
                SysControlList = Entity.SysControl.Where(n => n.State == 1 && n.Tag == "WeiXin" && n.LagEntryNum == 0).OrderBy(n => n.Sort).ToList();
            }
            else if (IsAlipayBrowser)
            {
                SysControlList = Entity.SysControl.Where(n => n.State == 1 && n.Tag == "Alipay" && n.LagEntryNum == 0).OrderBy(n => n.Sort).ToList();
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
            }
            ViewBag.SysControlList = List;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shopid"></param>
        /// <param name="Amount"></param>
        /// <param name="payway">这里payway是SysConfig的ID</param>
        /// <returns></returns>
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
            Users baseUsers = Entity.Users.FirstOrNew(n => n.Id == shopid && n.State == 1 && n.CardStae == 2);
            if (baseUsers.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "商户信息有误，请核实！";
                return View("Error");
            }
            if (Amount <= 0)
            {
                ViewBag.ErrorMsg = "付款金额有误！[00]";
                return View("Error");
            }
            Amount = Amount.FormatMoney();

            #region 处理选择通道
            string Tag = string.Empty;
            byte PayType = 0;//订单类型
            if (IsWeiXinBrowser)
            {
                Tag = "WeiXin";
            }
            else if (IsAlipayBrowser)
            {
                Tag = "Alipay";
            }
            #region 获取系统支付配置
            SysControl SysControl = Entity.SysControl.FirstOrDefault(n => n.Id == payway);
            if (SysControl == null)
            {
                ViewBag.ErrorMsg = "接口维护中";
                return View("Error");
            }
            SysControl syscontrol = SysControl.ChkState();
            if (syscontrol.State != 1)
            {
                ViewBag.ErrorMsg = "接口维护中";
                return View("Error");
            }

            if (Amount < SysControl.SNum || Amount > SysControl.ENum)
            {
                ViewBag.ErrorMsg = "本付款方式限定金额为" + SysControl.SNum.ToString("f2") + "-" + SysControl.ENum.ToString("f2");
                return View("Error");
            }

            if (SysControl.Tag != Tag)//接口对应的"操作应用程序"
            {
                string tagname = "";
                if (SysControl.Tag == "WeiXin")
                {
                    tagname = "微信";
                }
                else if (SysControl.Tag == "Alipay")
                {
                    tagname = "支付宝";
                }
                else
                {
                    tagname = "银联";
                }
                ViewBag.ErrorMsg = "请使用" + tagname + "操作";
                return View("Error");
            }
            PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(o => o.Id == SysControl.PayWay && o.State == 1);
            if (PayConfig == null)
            {
                ViewBag.ErrorMsg = "支付接口维护中，请使用其它支付通道！";
                return View("Error");
            }
            switch (PayConfig.GroupType)//从这里获取订单类型
            {
                case "AliPay":
                    PayType = 7;
                    break;
                case "WeiXin":
                    PayType = 8;
                    break;
            }
            if (PayType == 0)
            {
                ViewBag.ErrorMsg = "没有对应的支付配置";
                return View("Error");
            }
            #endregion
            #endregion

            #region 订单生成
            //获取用户支付配置
            UserPay UserPay = Entity.UserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PId == SysControl.PayWay);
            if (UserPay == null)
            {
                ViewBag.ErrorMsg = "用户支付配置错误";
                return View("Error");
            }
            //获取分支机构信息
            SysAgent SysAgent = new SysAgent();
            if (!baseUsers.Agent.IsNullOrEmpty())
            {
                SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
            }
            //获取系统配置
            SysSet SysSet = Entity.SysSet.FirstOrDefault();

            //TN判断
            int InType = SysControl.LagEntryNum > 0 ? 1 : 0;
            if (InType == 1)
            {
                //客户端传来T+N但是系统没开启时无效
                if (SysSet.LagEntry == 0)
                {
                    InType = 0;
                }
            }
            var OrderF2F = new OrderF2F()
            {
                PayWay = SysControl.PayWay,
                AddTime = DateTime.Now,
                OType = PayType,
                Amoney = Amount,
            };
            if (InType == 1)
            {
                //手续费
                OrderF2F.Poundage = 0;
                //商户费率
                OrderF2F.UserRate = 0;
                //T+n时，代理佣金为 交易金额*费率
                decimal AgentPayGet = OrderF2F.Amoney * SysSet.AgentGet;
                OrderF2F.AgentPayGet = (Double)AgentPayGet;
                //佣金舍位
                OrderF2F.AgentPayGet = OrderF2F.AgentPayGet.Floor();
            }
            else
            {
                //手续费
                OrderF2F.Poundage = OrderF2F.Amoney * (decimal)UserPay.Cost;
                //手续费取进
                OrderF2F.Poundage = OrderF2F.Poundage.Ceiling();
                //商户费率
                OrderF2F.UserRate = UserPay.Cost;
                //分支机构佣金设置为0，待分润计算后再写入
                OrderF2F.AgentPayGet = 0;
            }
            OrderF2F.UId = baseUsers.Id;
            //到帐金额=支付金额-手续费
            OrderF2F.PayMoney = OrderF2F.Amoney - OrderF2F.Poundage;
            //第三方支付通道率
            OrderF2F.SysRate = (double)PayConfig.Cost;
            //这里是利润计算==========
            //利润=总金额-到帐-支付手续费
            decimal GetAll = OrderF2F.Amoney - OrderF2F.PayMoney - OrderF2F.Amoney * (decimal)OrderF2F.SysRate;
            //利润舍位
            GetAll = GetAll.Floor();
            //总利润
            OrderF2F.AIdPayGet = (double)GetAll;

            OrderF2F.Agent = SysAgent.Id;//分支机构Id
            OrderF2F.AId = baseUsers.AId;
            OrderF2F.FId = 0;
            OrderF2F.OrderState = 1;
            OrderF2F.PayState = 0;
            OrderF2F.AgentState = 0;
            OrderF2F.AddTime = DateTime.Now;
            OrderF2F.PayId = string.Empty;
            OrderF2F.PayType = PayType;

            //写入订单总表
            Orders Orders = new Orders();
            Orders.UId = OrderF2F.UId;
            Orders.TName = baseUsers.NeekName;

            Orders.PayType = PayType;
            Orders.PayName = "收银台-" + PayConfig.Name;;

            Orders.RUId = 0;
            Orders.RName = string.Empty;
            Orders.TType = OrderF2F.OType;
            Orders.TState = 1;
            Orders.Amoney = OrderF2F.Amoney;
            Orders.Poundage = OrderF2F.Poundage;
            Orders.AddTime = DateTime.Now;
            Orders.PayState = 0;
            Orders.PayWay = SysControl.PayWay;

            Orders.Agent = OrderF2F.Agent;
            Orders.AgentState = 0;
            Orders.AId = OrderF2F.AId;
            Orders.FId = 0;
            Orders.ComeWay = 2;//收银台订单的标识 1:APP订单 2:收银台订单

            if (InType == 1)
            {
                Orders.LagEntryDay = SysControl.LagEntryDay;
                Orders.LagEntryNum = SysControl.LagEntryNum;
            }
            else
            {
                Orders.LagEntryDay = 0;
                Orders.LagEntryNum = 0;
            }

            Entity.Orders.AddObject(Orders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, Orders);

            OrderF2F.OId = Orders.TNum;
            Entity.OrderF2F.AddObject(OrderF2F);
            Entity.SaveChanges();

            //=======================================
            UserTrack UserTrack = new UserTrack();
            UserTrack.ENo = baseUsers.ENo;
            UserTrack.OPType = Orders.PayName;
            UserTrack.UserName = Orders.TNum;
            UserTrack.GPSAddress = "";
            UserTrack.GPSX = "";
            UserTrack.GPSY = "";
            UserTrack.IP = Utils.GetIP();
            Orders.SeavGPSLog(UserTrack, Entity);
            //=======================================

            #endregion

            #region 提交结算中心
            if (PayConfig.DllName == "HFAliPay" || PayConfig.DllName == "HFWeiXin")
            {
                string NoticeUrl = "";
                string Action = "";
                if (PayConfig.DllName == "HFAliPay")
                {
                    NoticeUrl = NoticePath + "/PayCenter/HFAliPay/Notice.html";
                    Action = "AliSao";
                }
                if (PayConfig.DllName == "HFWeiXin")
                {
                    NoticeUrl = NoticePath + "/PayCenter/HFWeiXin/Notice.html";
                    Action = "WxJsApi";
                }
                string[] PayConfigArr = PayConfig.QueryArray.Split(',');
                if (PayConfigArr.Length != 3)
                {
                    ViewBag.ErrorMsg = "参数错误!";
                    return View("Error");
                }
                //提交结算中心
                string merId = PayConfigArr[0];//商户号
                string merKey = PayConfigArr[1];//商户密钥
                string JsPayWay = PayConfigArr[2];//绑定通道

                string orderId = Orders.TNum;//商户流水号
                decimal money = Orders.Amoney * 100;
                long intmoney = Int64.Parse(money.ToString("F0"));
                string OrderMoney = intmoney.ToString();//金额，以分为单

                string PostJson = "{\"action\":\"" + Action + "\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"payway\":\"" + JsPayWay + "\",\"orderid\":\"" + orderId + "\",\"backurl\":\"" + NoticeUrl + "\"}";

                string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                string Sign = (DataBase64 + merKey).GetMD5();

                DataBase64 = HttpUtility.UrlEncode(DataBase64);
                string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);

                //本通道支持直联公众号
                if (IsWeiXinBrowser && Action == "WxJsApi")
                {
                    Orders.PayId = "公众号支付";
                    Entity.SaveChanges();
                    string gateway_url = "https://api.zhifujiekou.com/apis/wxjspaygateway";
                    string GoUrl = gateway_url + "?" + PostData;
                    Response.Redirect(GoUrl);
                    return View("Null");
                }

                string HF_Url = "https://api.zhifujiekou.com/api/mpgateway";
                string Ret = Utils.PostRequest(HF_Url, PostData, "utf-8");
                JObject JS = new JObject();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(Ret);
                }
                catch (Exception)
                {
                    Utils.WriteLog("[Order_HF_Err]:【PostData】" + PostData + "\n【Ret】" + Ret, "ShopErr");
                    ViewBag.ErrorMsg = "返回数据异常!";
                    return View("Error");
                }
                string resp = JS["resp"].ToString();
                Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(Ret);
                }
                catch (Exception)
                {
                    Utils.WriteLog("[Order_HF_Err]:【Ret】" + Ret, "ShopErr");
                    ViewBag.ErrorMsg = "返回结果异常!";
                    return View("Error");
                }
                string respcode = JS["respcode"].ToString();
                if (respcode != "00")
                {
                    string respmsg = JS["respmsg"].ToString();
                    Utils.WriteLog("[Order_HF_Err]:【" + respcode + "】" + respmsg, "ShopErr");
                    ViewBag.ErrorMsg = respmsg;
                    return View("Error");
                }
                if (JS["formaction"] == null)
                {
                    Utils.WriteLog("[Order_HF_Err]:【formaction NULL】" + Ret, "ShopErr");
                    ViewBag.ErrorMsg = "未能生成二维码";
                    return View("Error");
                }
                Orders.PayId = JS["formaction"].ToString();
            }
            #endregion

            this.ViewBag.Orders = Orders;
            return View();
        }

        public ActionResult Success(string code)
        {
            string TNum = LokFuEncode.Base64Decode(code);
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == TNum);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "订单不存在！";
                return View("Error");
            }
            if (Orders.PayState != 1)
            {
                ViewBag.ErrorMsg = "订单未支付！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            return View();
        }

        /// <summary>
        /// 查询订单状态
        /// </summary>
        /// <param name="code">订单号</param>
        /// <returns></returns>
        public string PayState(string code)
        {
            string TNum = LokFuEncode.Base64Decode(code);
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == TNum);
            if (Orders == null)
            {
                return "E1";
            }
            if (Orders.PayState == 1)
            {
                return "S";
            }
            if (Orders.TState != 1)
            {
                return "E0";
            }
            PayConfig PayConfig = Entity.PayConfig.FirstOrNew(n => n.Id == Orders.PayWay);
            if (PayConfig == null)
            {
                return "E2";
            }
            if (Orders.TState == 1 && Orders.PayState == 0)
            {
                if (PayConfig.DllName == "HFAliPay" || PayConfig.DllName == "HFWeiXin")
                {
                    #region 查询结算中心
                    string[] QueryArr = PayConfig.QueryArray.Split(',');
                    if (QueryArr.Length == 3)
                    {
                        //提交结算中心
                        string merId = QueryArr[0];//商户号
                        string merKey = QueryArr[1];//商户密钥
                        string orderId = Orders.TNum;//商户流水号
                        string PostJson = "{\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\"}";
                        string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                        string Sign = (DataBase64 + merKey).GetMD5();
                        DataBase64 = HttpUtility.UrlEncode(DataBase64);
                        string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                        string HF_Url = "https://api.zhifujiekou.com/api/query";
                        string Ret = Utils.PostRequest(HF_Url, PostData, "utf-8");
                        JObject JS = new JObject();
                        try
                        {
                            JS = (JObject)JsonConvert.DeserializeObject(Ret);
                        }
                        catch (Exception)
                        {
                            Utils.WriteLog("[Query_HF_Err]:【PostData】" + PostData + "\n【Ret】" + Ret, "ShopErr");
                            return "ER";
                        }
                        string resp = JS["resp"].ToString();
                        Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                        try
                        {
                            JS = (JObject)JsonConvert.DeserializeObject(Ret);
                        }
                        catch (Exception)
                        {
                            Utils.WriteLog("[Query_HF_Err]:", "【Ret】" + Ret, "ShopErr");
                            return "ER";
                        }

                        string respcode = JS["respcode"].ToString();
                        if (respcode != "00")
                        {
                            string respmsg = JS["respmsg"].ToString();
                            Utils.WriteLog("[Query_HF_Err]:【" + respcode + "】" + respmsg, "PayState");
                            return "ER";
                        }
                        string resultcode = JS["resultcode"].ToString();
                        if (resultcode == "0000" || resultcode == "1002" || resultcode == "1004")
                        {
                            string txnamt = JS["txnamt"].ToString();
                            int factmoney = int.Parse(txnamt);
                            if (((int)(Orders.Amoney * 100)) == factmoney)
                            {
                                Orders = Orders.PaySuccess(Entity);
                            }
                        }
                    }
                    #endregion
                }
            }
            if (Orders.PayState == 1)
            {
                return "S";
            }
            if (Orders.TState == 0)
            {
                return "E100";
            }
            return "G";
        }

    }
}
