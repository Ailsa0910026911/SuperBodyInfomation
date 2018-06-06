using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using LokFu.Extensions;
using System.Collections;
using System.Collections.Generic;
using LokFu.Infrastructure;
using System;
using LokFu.Models;
using LokFu.FastPay;
using System.Data.Objects;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;

namespace LokFu.Areas.Mobile.Controllers
{
    public class FastController : BaseController
    {
        public bool IsWeiXinBrowser = false;
        public bool IsAlipayBrowser = false;
        public bool IsGoogle = false;
        public bool IsIPhone = false;
        public FastController()
        {
            IsWeiXinBrowser = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("micromessenger");
            IsAlipayBrowser = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("alipayclient");
            IsGoogle = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("android");
            IsIPhone = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("iphone") || System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("ipad");
            ViewBag.IsWeiXinBrowser = IsWeiXinBrowser;
            ViewBag.IsAlipayBrowser = IsAlipayBrowser;
            ViewBag.IsGoogle = IsGoogle;
            ViewBag.IsIPhone = IsIPhone;
        }
        public ActionResult Index(Users Users, string SetCashType = "D0")
        {
            if (!Users.Id.IsNullOrEmpty())
            {
                Users = Entity.Users.FirstOrNew(n => n.Id == Users.Id);
            }
            //if (!IsLokFu && !IsAlipayBrowser && !IsWeiXinBrowser)
            //{
            //    ViewBag.ErrorMsg = "请使用微信或支付宝扫码！";
            //    return View("Error");
            //}
            if (Users == null)
            {
                ViewBag.ErrorMsg = "商户信息异常！";
                return View("Error");
            }
            if (Users.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "商户信息异常！";
                return View("Error");
            }
            if (Users.State != 1)//用户被锁定
            {
                ViewBag.ErrorMsg = "商户状态异常！";
                return View("Error");
            }
            if (Users.CardStae != 2)//未实名认证
            {
                ViewBag.ErrorMsg = "商户未认证，不能收款！";
                return View("Error");
            }
            ViewBag.Users = Users;
            FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == Users.Id);
            if (FastUser == null)
            {
                ViewBag.ErrorMsg = "商户未开通！";
                return View("Error");
            }
            ViewBag.FastUser = FastUser;
            FastConfig FastConfig = Entity.FastConfig.FirstOrNew();

            //===========================================================
            //处理有哪些通道
            IList<FastPayWay> FastPayWayList = Entity.FastPayWay.Where(n => n.State == 1 && n.ManE == 0).OrderBy(n => n.Sort).ToList();
            if (!SetCashType.IsNullOrEmpty())
            {
                FastPayWayList = FastPayWayList.Where(n => n.GroupType == SetCashType).ToList();
            }
            if (FastPayWayList == null)
            {
                ViewBag.ErrorMsg = "暂无可用通道！";
                return View("Error");
            }
            IList<FastPayWay> PayWayList = new List<FastPayWay>();
            foreach (var p in FastPayWayList)
            {
                if (p.TimeType == 1)//限制时间，模式1
                {
                    DateTime STime = p.STime;
                    DateTime ETime = p.ETime;
                    DateTime NowSTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + STime.ToString("HH:mm:ss"));
                    DateTime NowETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + ETime.ToString("HH:mm:ss"));
                    if (NowSTime <= DateTime.Now && DateTime.Now <= NowETime)
                    {
                        //当前时间允许交易
                        PayWayList.Add(p);
                    }
                }
                else
                {
                    PayWayList.Add(p);
                }
            }
            if (PayWayList.Count < 1)
            {
                ViewBag.ErrorMsg = "暂无可用通道！";
                return View("Error");
            }
            IList<FastPayWay> WxList = PayWayList.Where(n => n.HasWeiXin == 1).ToList();
            IList<FastPayWay> AliList = PayWayList.Where(n => n.HasAliPay == 1).ToList();
            IList<FastPayWay> BankList = PayWayList.Where(n => n.HasBank == 1).ToList();
            IList<FastPayWay> List = new List<FastPayWay>();
            if (WxList.Count > 0)
            {
                FastPayWay Wx = new FastPayWay();
                Wx.Id = 2;
                Wx.GroupType = "weixin";
                Wx.DllName = "微信支付";
                Wx.SNum = WxList.OrderBy(n => n.SNum).FirstOrDefault().SNum;
                Wx.ENum = WxList.OrderByDescending(n => n.ENum).FirstOrDefault().ENum;
                List.Add(Wx);
            }
            if (AliList.Count > 0)
            {
                FastPayWay Ali = new FastPayWay();
                Ali.Id = 1;
                Ali.GroupType = "alipay";
                Ali.DllName = "支付宝支付";
                Ali.SNum = AliList.OrderBy(n => n.SNum2).FirstOrDefault().SNum2;
                Ali.ENum = AliList.OrderByDescending(n => n.ENum2).FirstOrDefault().ENum2;
                List.Add(Ali);
            }
            if (BankList.Count > 0)
            {
                //收银台暂不开放银联
                //FastPayWay Bank = new FastPayWay();
                //Bank.Id = 3;
                //Bank.GroupType = "bank";
                //Bank.DllName = "银行卡支付";
                //Bank.SNum = BankList.OrderBy(n => n.BankSNum).FirstOrDefault().BankSNum;
                //Bank.ENum = BankList.OrderByDescending(n => n.BankENum).FirstOrDefault().BankENum;
                //List.Add(Bank);
            }
            ViewBag.FastPayWayList = List;
            //===========================================================
            //这里要处理商户入驻
            BusFastPay.AddMer(FastUser, FastConfig, this.Entity);
            //===========================================================
            //处理自选商户 
            //Users.SetPayName = Users.NeekName;
            //Users.CanOpenMer = 0;
            return View();
        }
        public ActionResult GoPay(int shopid = 0, decimal Amount = 0, byte payway = 0)
        {
            if (shopid.IsNullOrEmpty()) {
                ViewBag.ErrorMsg = "商户信息有误，请核实！";
                return View("Error");
            }
            if (Amount.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "付款金额有误，请核实！";
                return View("Error");
            }
            if (payway.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请选择支付方式！";
                return View("Error");
            }
            if (payway == 3 && shopid != 26)
            {
                ViewBag.ErrorMsg = "暂不开放！";
                return View("Error");
            }
            Users Users = Entity.Users.FirstOrNew(n => n.Id == shopid && n.State == 1 && n.CardStae == 2);
            if (Users.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "商户信息有误，请核实！";
                return View("Error");
            }
            ViewBag.Users = Users;
            if (Amount <= 0)
            {
                ViewBag.ErrorMsg = "付款金额有误！[00]";
                return View("Error");
            }
            IList<FastPayWay> FastPayWayList = null;
            if (payway == 1)
            {
                FastPayWayList = Entity.FastPayWay.Where(n => n.State == 1 && n.SNum2 < Amount && n.ENum2 >= Amount && n.HasAliPay == 1).OrderBy(n => n.Sort).ToList();
            }
            else if (payway == 2)
            {
                FastPayWayList = Entity.FastPayWay.Where(n => n.State == 1 && n.SNum < Amount && n.ENum >= Amount && n.HasWeiXin == 1).OrderBy(n => n.Sort).ToList();
            }
            else
            {
                ViewBag.ErrorMsg = "支付方式有误！";
                return View("Error");
            }
            if (FastPayWayList.Count < 1) {
                ViewBag.ErrorMsg = "当前没有可用的渠道！";
                return View("Error");
            }
            FastPayWay FastPayWay = null;
            FastUserPay FastUserPay = null;
            IList<FastPayWay> PayWayList = new List<FastPayWay>();
            foreach (var p in FastPayWayList)
            {
                if (p.TimeType == 1)//限制时间，模式1
                {
                    DateTime STime = p.STime;
                    DateTime ETime = p.ETime;
                    DateTime NowSTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + STime.ToString("HH:mm:ss"));
                    DateTime NowETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " " + ETime.ToString("HH:mm:ss"));
                    if (NowSTime <= DateTime.Now && DateTime.Now <= NowETime)
                    {
                        //当前时间允许交易
                        PayWayList.Add(p);
                    }
                }
                else {
                    PayWayList.Add(p);
                }
            }
            if (PayWayList.Count < 1)
            {
                ViewBag.ErrorMsg = "请在可交易时间内进行交易！";
                return View("Error");
            }
            foreach (var p in PayWayList)
            {
                FastUserPay temp = Entity.FastUserPay.FirstOrDefault(n => n.UId == Users.Id && n.PayWay == p.Id && n.MerState == 1 && n.CardState == 1);
                if (temp != null)
                {
                    FastPayWay = p;
                    FastUserPay = temp;
                    break;
                }
            }
            if (FastUserPay == null)
            {
                ViewBag.ErrorMsg = "渠道未开通！";
                return View("Error");
            }
            if (FastPayWay == null)
            {
                ViewBag.ErrorMsg = "通道未开通！";
                return View("Error");
            }
            decimal UserCost = 0;//用户
            decimal BankCost = 0;
            decimal BankMin = 0;
            decimal BankMax = 0;
            decimal AgentCost = 0;//代理
            if (payway == 1) {//支付宝
                UserCost = FastUserPay.UserCost2;
                BankCost = FastPayWay.BankCost2;
                BankMin = FastPayWay.MinCost2;
                BankMax = FastPayWay.MaxCost2;
                AgentCost = FastPayWay.Cost2;
            }
            if (payway == 2)//微信
            {
                UserCost = FastUserPay.UserCost;
                BankCost = FastPayWay.BankCost;
                BankMin = FastPayWay.MinCost;
                BankMax = FastPayWay.MaxCost;
                AgentCost = FastPayWay.Cost;
            }
            if (payway == 3)//银联
            {
                UserCost = FastUserPay.UserCost3;
                BankCost = FastPayWay.BankCost3;
                BankMin = FastPayWay.MinCost3;
                BankMax = FastPayWay.MaxCost3;
                AgentCost = FastPayWay.Cost3;
            }
            //=======================生成订单===========================
            SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent);
            IList<SysAgent> SysAgentList = SysAgent.GetAgentsById(Entity);
            SysAgent TopAgent = SysAgentList.FirstOrNew(n => n.Tier == 1);

            FastOrder FastOrder = new FastOrder();
            FastOrder.ComeWay = 2;
            FastOrder.UId = Users.Id;
            FastOrder.Agent = SysAgent.Id;

            FastOrder.PayId = string.Empty;

            FastOrder.OType = payway;
            FastOrder.PayWay = FastPayWay.Id;
            FastOrder.CashType = FastPayWay.GroupType;
            FastOrder.Amoney = Amount;

            //用户手续费
            decimal Poundage = Amount * UserCost + FastUserPay.UserCash;
            Poundage = Poundage.Ceiling();
            FastOrder.Poundage = Poundage;
            //用户最终金额
            FastOrder.PayMoney = FastOrder.Amoney - FastOrder.Poundage;

            if (FastOrder.PayMoney < 0) {
                ViewBag.ErrorMsg = "结算金额出现负数！";
                return View("Error");
            }

            FastOrder.UserRate = UserCost;

            FastOrder.AgentRate = AgentCost;
            FastOrder.SysRate = BankCost;

            FastOrder.UserCash = FastUserPay.UserCash;
            FastOrder.SysCash = FastPayWay.Cash;

            //计算手续费差
            decimal PayGet = Amount * (UserCost - AgentCost);
            PayGet = PayGet.Floor();
            //一级代理利润
            decimal AgentPayGet = PayGet * (decimal)TopAgent.PayGet;
            AgentPayGet = AgentPayGet.Floor();

            FastOrder.AgentPayGet = AgentPayGet;

            string AgentPath = "|";
            string Split = "|";
            decimal MyGet = PayGet;
            foreach (var p in SysAgentList) {
                AgentPath += p.Id + "|";
                MyGet = MyGet * (decimal)p.PayGet;//各级代理分润
                MyGet = MyGet.Floor();
                Split += MyGet.ToString("F2") + "|";
            }
            FastOrder.AgentPath = AgentPath;
            FastOrder.Split = Split;

            decimal BankMoney = Amount * BankCost;
            if (BankMoney < BankMin)
            {
                BankMoney = BankMin;
            }
            if (BankMoney > BankMax)
            {
                BankMoney = BankMax;
            }
            //用户手续费(含代付手续费)-代付分润-银行手续费-银行代付成本
            decimal HFGet = Poundage - AgentPayGet - BankMoney - FastPayWay.Cash;
            FastOrder.HFGet = HFGet;

            FastOrder.State = 1;
            FastOrder.AddTime = DateTime.Now;

            FastOrder.PayState = 0;
            FastOrder.AgentState = 0;
            FastOrder.UserState = 0;

            FastOrder.CardName = FastUserPay.CardName;
            FastOrder.Bank = FastUserPay.Bank;
            FastOrder.Card = FastUserPay.Card;
            FastOrder.Bin = FastUserPay.Bin;

            Entity.FastOrder.AddObject(FastOrder);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, FastOrder);
            //=======================分润记录===========================
            MyGet = PayGet;
            foreach (var p in SysAgentList)
            {
                MyGet = MyGet * (decimal)p.PayGet;//各级代理分润
                MyGet = MyGet.Floor();
                FastSplit FastSplit = new FastSplit();
                FastSplit.Tnum = FastOrder.TNum;
                FastSplit.Profit = MyGet;
                FastSplit.AgentId = p.Id;
                FastSplit.Tier = p.Tier;
                FastSplit.AddTime = DateTime.Now;
                Entity.FastSplit.AddObject(FastSplit);
            }
            Entity.SaveChanges();
            //RespObj.queryId = Orders.TNum;
            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            //=======================请求接口开始===========================
            if (FastPayWay.DllName == "HFPay")
            {
                string NoticeUrl = NoticePath + "/PayCenter/HFPay/FastNotice.html";//后台通过地址
                #region 微信&支付宝
                string Action = "";
                if (FastOrder.OType == 1)
                {
                    Action = "AliSao";
                }
                else if (FastOrder.OType == 2)
                {
                    Action = "WxSao";
                }
                //提交结算中心
                string merId = PayConfigArr[0];//商户号
                string merKey = PayConfigArr[1];//商户密钥
                string PayWay = PayConfigArr[2];//绑定通道

                decimal money = FastOrder.Amoney * 100;
                string OrderMoney = money.ToString("F0");//金额，以分为单

                string PostJson = "{\"action\":\"" + Action + "\",\"txnamt\":\"" + OrderMoney + "\",\"merid\":\"" + merId + "\",\"payway\":\"" + PayWay + "\",\"orderid\":\"" + FastOrder.TNum + "\",\"backurl\":\"" + NoticeUrl + "\"}";

                string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                string Sign = (DataBase64 + merKey).GetMD5();

                DataBase64 = HttpUtility.UrlEncode(DataBase64);
                string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);

                string HF_Url = "https://api.zhifujiekou.com/api/mpgateway";

                string Ret = Utils.PostRequest(HF_Url, PostData, "utf-8");

                JObject JS = new JObject();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(Ret);
                }
                catch (Exception)
                {
                    FastOrder.State = 0;
                    FastOrder.Remark = "数据请求出错";
                    Entity.SaveChanges();
                    JS = null;
                }
                if (JS != null)
                {
                    string resp = JS["resp"].ToString();
                    Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception Ex)
                    {
                        FastOrder.State = 0;
                        FastOrder.Remark = "JSON加载出错";
                        Entity.SaveChanges();
                        JS = null;
                    }
                    if (JS != null)
                    {
                        string respcode = JS["respcode"].ToString();
                        if (respcode != "00")
                        {
                            string respmsg = JS["respmsg"].ToString();
                            FastOrder.State = 0;
                            FastOrder.Remark = respmsg;
                            Entity.SaveChanges();
                        }
                        else
                        {
                            if (JS["formaction"] == null)
                            {
                                FastOrder.State = 0;
                                FastOrder.Remark = "接口没有返回二维码";
                                Entity.SaveChanges();
                            }
                            else
                            {
                                string BankNum = JS["queryid"].ToString();
                                string qr_code = JS["formaction"].ToString();
                                FastOrder.PayId = qr_code;
                                FastOrder.Trade = BankNum;
                                Entity.SaveChanges();
                            }
                        }
                    }
                }
                #endregion
            }
            ViewBag.FastOrder = FastOrder;
            if (IsAlipayBrowser) {
                if (!FastOrder.PayId.IsNullOrEmpty())
                {
                    Response.Redirect(FastOrder.PayId);
                    return View("Null");
                }
            }
            return View();
        }
        public ActionResult Success(string code)
        {
            string TNum = LokFuEncode.Base64Decode(code);
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == TNum);
            if (FastOrder == null)
            {
                ViewBag.ErrorMsg = "订单不存在！";
                return View("Error");
            }
            if (FastOrder.PayState != 1)
            {
                ViewBag.ErrorMsg = "订单未支付！";
                return View("Error");
            }
            ViewBag.FastOrder = FastOrder;
            return View();
        }
        public string PayState(string code) {
            string TNum = LokFuEncode.Base64Decode(code);
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.TNum == TNum);
            if (FastOrder == null)
            {
                return "E1";
            }
            if (FastOrder.PayState == 1) {
                return "S";
            }
            if (FastOrder.State != 1)
            {
                return "E0";
            }
            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrNew(n => n.Id == FastOrder.PayWay);
            if (FastPayWay == null)
            {
                return "E2";
            }
            string[] PayConfigArr = FastPayWay.QueryArray.Split(new char[] { ',' });//接口信息
            if (FastPayWay.DllName == "HFPay")
            {
                #region 结算中心
                if (PayConfigArr.Length == 3)
                {
                    string HF_Url = "https://api.zhifujiekou.com/api/query";
                    string MerId = PayConfigArr[0];
                    string MerKey = PayConfigArr[1];
                    string orderId = FastOrder.TNum;//商户流水号
                    string PostJson = "{\"merid\":\"" + MerId + "\",\"orderid\":\"" + orderId + "\"}";
                    string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                    string Sign = (DataBase64 + MerKey).GetMD5();
                    DataBase64 = HttpUtility.UrlEncode(DataBase64);
                    string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                    string Ret = Utils.PostRequest(HF_Url, PostData, "utf-8");
                    JObject JS = new JObject();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception)
                    {
                        JS = null;
                    }
                    if (JS != null)
                    {
                        if (JS["resp"] != null)
                        {
                            string resp = JS["resp"].ToString();
                            Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                            try
                            {
                                JS = (JObject)JsonConvert.DeserializeObject(Ret);
                            }
                            catch (Exception)
                            {
                                JS = null;
                            }
                            if (JS != null)
                            {
                                string respcode = JS["respcode"].ToString();
                                if (respcode == "00")
                                {
                                    string resultcode = JS["resultcode"].ToString();
                                    if (resultcode == "0000" || resultcode == "1002" || resultcode == "1004")
                                    {
                                        string queryid = JS["queryid"].ToString();
                                        FastOrder.Trade = queryid;
                                        Entity.SaveChanges();
                                        string txnamt = JS["txnamt"].ToString();
                                        int factmoney = int.Parse(txnamt);
                                        if (((int)(FastOrder.Amoney * 100)) == factmoney)
                                        {
                                            FastOrder = FastOrder.PaySuccess(Entity);
                                        }
                                    }
                                }
                                //================================================
                                //这里记录日志
                                PayLog PayLog = new PayLog();
                                PayLog.PId = (int)FastOrder.PayWay;
                                PayLog.OId = FastOrder.TNum;
                                PayLog.TId = FastOrder.Trade;
                                PayLog.Amount = FastOrder.Amoney;
                                PayLog.Way = "Query";
                                PayLog.AddTime = DateTime.Now;
                                PayLog.Data = Ret;
                                PayLog.State = 1;
                                Entity.PayLog.AddObject(PayLog);
                                Entity.SaveChanges();
                                //================================================
                            }
                        }
                    }
                }
                #endregion
            }
            if (FastOrder.PayState == 1) {
                return "S";
            }
            if (FastOrder.State == 0)
            {
                return "E100";
            }
            return "G";
        }

        #region 钱包中打开才有
        public ActionResult MyQrCode(string act = "")
        {
            if (!IsLokFu) {
                ViewBag.ErrorMsg = "请在钱包中打开链接！";
                return View("Error");
            }
            if (BasicUsers == null)
            {
                ViewBag.ErrorMsg = "用户未登录或登录异常！";
                return View("Error");
            }
            if (BasicUsers.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "用户未登录或登录异常！";
                return View("Error");
            }
            if (BasicUsers.State != 1)//用户被锁定
            {
                ViewBag.ErrorMsg = "您的帐户被锁定！";
                return View("Error");
            }
            if (BasicUsers.CardStae != 2)//未实名认证
            {
                ViewBag.ErrorMsg = "您未实名认证！";
                return View("Error");
            }
            //if (baseUsers.MiBao != 1)//未设置支付密码
            //{}
            //==========================================================================================
            //获取直通车配置及用户直通车配置
            FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == BasicUsers.Id);
            if (FastUser == null) {
                //开通帐户
                FastUser = new FastUser();
                FastUser.UId = BasicUsers.Id;
                FastUser.TrueName = BasicUsers.TrueName;
                FastUser.CardId = BasicUsers.CardId;
                FastUser.AddTime = DateTime.Now;
                Entity.FastUser.AddObject(FastUser);
                Entity.SaveChanges();
            }
            ViewBag.FastUser = FastUser;
            if (FastUser.Card.IsNullOrEmpty() || act == "setbank")
            { 
                //未绑卡
                IList<UserCard> UserCardList = Entity.UserCard.Where(n => n.UId == BasicUsers.Id && n.Type == 1).ToList();
                if (UserCardList.Count < 1) {
                    return View("MyBankNull");
                }
                ViewBag.UserCardList = UserCardList;
                ViewBag.Act = act;
                return View("MyBank");
            }
            ViewBag.FastConfig = Entity.FastConfig.FirstOrNew();

            ViewBag.Code = BasicUsers.FastCode();

            FastConfig FastConfig = Entity.FastConfig.FirstOrNew();
            //===========================================================
            //这里要处理商户入驻
            BusFastPay.AddMer(FastUser, FastConfig, this.Entity);
            //===========================================================
            return View();
        }
        public ActionResult MyOrder(FastOrder FastOrder)
        {
            if (!IsLokFu)
            {
                ViewBag.ErrorMsg = "请在钱包中打开链接！";
                return View("Error");
            }
            if (BasicUsers == null)//用户令牌不存在
            {
                ViewBag.ErrorMsg = "您未登录或已在其它手机登录！";
                return View("Error");
            }
            if (BasicUsers.State != 1)//用户被锁定
            {
                ViewBag.ErrorMsg = "您的帐户被锁定！";
                return View("Error");
            }
            if (BasicUsers.CardStae != 2)//未实名认证
            {
                ViewBag.ErrorMsg = "您未实名认证！";
                return View("Error");
            }
            FastOrder FO = Entity.FastOrder.FirstOrDefault(n => n.UId == BasicUsers.Id && n.TNum == FastOrder.TNum);
            if (FO == null) {
                ViewBag.ErrorMsg = "订单不存在！";
                return View("Error");
            }
            ViewBag.FastOrder = FO;
            return View();
        }
        public ActionResult MyOrders(EFPagingInfo<FastOrder> p, int IsAjax = 0, int State = 0)
        {
            if (!IsLokFu)
            {
                ViewBag.ErrorMsg = "请在钱包中打开链接！";
                return View("Error");
            }
            if (BasicUsers == null)//用户令牌不存在
            {
                ViewBag.ErrorMsg = "您未登录或已在其它手机登录！";
                return View("Error");
            }
            if (BasicUsers.State != 1)//用户被锁定
            {
                ViewBag.ErrorMsg = "您的帐户被锁定！";
                return View("Error");
            }
            if (BasicUsers.CardStae != 2)//未实名认证
            {
                ViewBag.ErrorMsg = "您未实名认证！";
                return View("Error");
            }
            if (State == 1) {
                p.SqlWhere.Add(f => f.PayState == 1);
            }
            else if (State == 2) {
                p.SqlWhere.Add(f => f.PayState == 0);
            }
            else if (State == 3)
            {
                p.SqlWhere.Add(f => f.UserState == 1);
            }
            else if (State == 4)
            {
                p.SqlWhere.Add(f => f.UserState != 1 && f.PayState == 1);
            }
            p.SqlWhere.Add(f => f.UId == BasicUsers.Id);
            p.OrderByList.Add("Id", "DESC");
            p.PageSize = 10;//暂不分页，200笔
            IPageOfItems<FastOrder> FastOrderList = Entity.Selects<FastOrder>(p);
            ViewBag.FastOrderList = FastOrderList;
            if (IsAjax == 1)
            {
                return View("Orders");
            }
            return View();
        }
        public string SetBank(int bankid)
        {
            if (!IsLokFu)
            {
                return "请在钱包中打开链接！";
            }
            if (BasicUsers == null)//用户令牌不存在
            {
                return "您未登录或已在其它手机登录！";
            }
            if (BasicUsers.Id.IsNullOrEmpty())//用户令牌不存在
            {
                return "您未登录或已在其它手机登录！";
            }
            if (BasicUsers.State != 1)//用户被锁定
            {
                return "您的帐户被锁定！";
            }
            if (BasicUsers.CardStae != 2)//未实名认证
            {
                return "您未实名认证！";
            }
            FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == BasicUsers.Id);
            if (FastUser == null)
            {
                return "用户信息有误！";
            }
            UserCard UserCard = Entity.UserCard.FirstOrDefault(n => n.UId == BasicUsers.Id && n.Type == 1 && n.Id == bankid && n.State == 1);
            if (UserCard == null)
            {
                return "银行卡信息有误！";
            }
            if (FastUser.Card == UserCard.Card) {
                return "没有变更！";
            }
            FastUser.Card = UserCard.Card;
            FastUser.Bank = UserCard.Bank;
            FastUser.Bin = UserCard.Bin;
            Entity.SaveChanges();
            //这里要增加所有通道的商户注册及绑卡操作
            //=============================================================
            IList<FastPayWay> FastPayWayList = Entity.FastPayWay.OrderBy(n => n.Sort).ToList();
            IList<FastUserPay> FastUserPayList = Entity.FastUserPay.Where(n => n.UId == FastUser.UId).OrderBy(n => n.PayWay).ToList();
            foreach (var p in FastUserPayList)
            {
                FastPayWay FastPayWay = FastPayWayList.FirstOrDefault(n => n.Id == p.PayWay);
                if (FastPayWay != null)
                {
                    if (FastPayWay.DllName == "HFPay")
                    {
                        p.CardState = 1;//不需要验卡
                    }
                    else
                    {
                        p.CardState = 2;//重新标识状态为待提交
                    }
                    p.Bank = FastUser.Bank;
                    p.Card = FastUser.Card;
                    p.Bin = FastUser.Bin;
                    BusFastPay.AddCard(FastUser, p, FastPayWay, Entity);
                }
            }
            Entity.SaveChanges();
            //=============================================================
            return "ok";
        }
        #endregion
    }
}
