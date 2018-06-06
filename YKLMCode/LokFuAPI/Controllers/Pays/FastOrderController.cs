using LokFu.Extensions;
using LokFu.FastPay;
using LokFu.Infrastructure;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;

namespace LokFu.Controllers
{
    public class FastOrderController : InitController
    {
        public FastOrderController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[FastOrderController]:", "【Data】" + Data, Ex);
                json = null;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            FastOrder InFastOrder = new FastOrder(); 
            InFastOrder = JsonToObject.ConvertJsonToModel(InFastOrder, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            if (InFastOrder.Id == 99999)
            {
                DataObj.Msg = "当前通道维护中,建议您开通收款到银行卡功能,享受更多收款通道";
                DataObj.OutError("1000");
                return;
            }

            if (InFastOrder.CashType.IsNullOrEmpty())
            {
                InFastOrder.CashType = "D0";
                //DataObj.OutError("1000");
                //return;
            }
            decimal Amount = InFastOrder.Amoney;
            byte payway = InFastOrder.OType;
            if (Amount <= 0)
            {
                DataObj.OutError("1000");
                return;
            }
            if (payway != 1 && payway != 2 && payway != 3)
            {
                DataObj.Msg = "你当前版本不支持该交易，请等待新版本发布及升级！";
                DataObj.OutError("1000");
                return;
            }

            Users Users = Entity.Users.FirstOrDefault(n => n.Token == InFastOrder.Token);
            #region 用户
            if (Users == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (Users.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (Users.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (Amount.IsNullOrEmpty() || payway.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            #endregion

            FastUser FastUser = Entity.FastUser.FirstOrDefault(o => o.UId == Users.Id);
            if (FastUser == null)
            {
                DataObj.OutError("2070");
                return;
            }
            #region 取通道
            IList<FastPayWay> FastPayWayList = null;
            if (InFastOrder.Id.IsNullOrEmpty())
            {
                //这是旧逻辑,有一些旧版还在用
                if (payway == 1)
                {
                    FastPayWayList = Entity.FastPayWay.Where(n => n.State == 1 && n.SNum2 < Amount && n.ENum2 >= Amount && n.HasAliPay == 1).OrderBy(n => n.Sort).ToList();
                }
                else if (payway == 2)
                {
                    FastPayWayList = Entity.FastPayWay.Where(n => n.State == 1 && n.SNum < Amount && n.ENum >= Amount && n.HasWeiXin == 1).OrderBy(n => n.Sort).ToList();
                }
                else if (payway == 3)
                {
                    FastPayWayList = Entity.FastPayWay.Where(n => n.State == 1 && n.BankSNum < Amount && n.BankENum >= Amount && n.HasBank == 1).OrderBy(n => n.Sort).ToList();
                }
                else
                {
                    DataObj.OutError("1000");
                    return;
                }
            }
            else
            {
                var query = Entity.FastPayWay.Where(n => n.Id == InFastOrder.Id && n.State == 1);
                if (payway == 1)
                {
                    query = query.Where(o => o.HasAliPay == 1 && o.SNum2 <= Amount && o.ENum2 >= Amount);
                }
                else if (payway == 2)
                {
                    query = query.Where(o => o.HasWeiXin == 1 && o.SNum <= Amount && o.ENum >= Amount);
                }
                else if (payway == 3)
                {
                    query = query.Where(o => o.HasBank == 1 && o.BankSNum <= Amount && o.BankENum >= Amount);
                }
                FastPayWayList = query.ToList();
            }
            #endregion
            if (FastPayWayList.Count < 1)
            {
                DataObj.OutError("2079");
                return;
            }
            #region 通道验证及商户进件信息验证
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
                else
                {
                    PayWayList.Add(p);
                }
            }
            if (PayWayList.Count < 1)
            {
                DataObj.OutError("2071");
                return;
            }
            foreach (var p in PayWayList)
            {
                FastUserPay temp = Entity.FastUserPay.FirstOrDefault(n => n.UId == Users.Id && n.PayWay == p.Id && n.MerState == 1 && n.CardState == 1 && n.BusiState == 1);
                if (temp != null)
                {
                    FastPayWay = p;
                    FastUserPay = temp;
                    break;
                }
            }
            if (FastUserPay == null)
            {
                DataObj.OutError("2072");
                return;
            }
            if (FastPayWay == null)
            {
                DataObj.OutError("2073");
                return;
            }
            #endregion
            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
            #region 验证配置
            if (FastPayWay.DllName == "HFPay")
            {
                if (PayConfigArr.Length != 3)
                {
                    DataObj.OutError("2074");
                    return;
                }
            }
            if (FastPayWay.DllName == "HFJSPay")
            {
                if (PayConfigArr.Length != 3)
                {
                    DataObj.OutError("2074");
                    return;
                }
            }
            #endregion

            decimal UserCost = 0;//用户
            decimal BankCost = 0;
            decimal BankMin = 0;
            decimal BankMax = 0;
            decimal AgentCost = 0;//代理
            if (payway == 1)
            {//支付宝
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
            #region 创建交易
            //=======================生成订单===========================
            SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent);
            IList<SysAgent> SysAgentList = SysAgent.GetAgentsById(Entity);
            SysAgent TopAgent = SysAgentList.FirstOrNew();

            FastOrder FastOrder = new FastOrder();
            FastOrder.ComeWay = 1;
            FastOrder.UId = Users.Id;
            FastOrder.Agent = SysAgent.Id;

            FastOrder.PayId = string.Empty;

            FastOrder.OType = payway;
            FastOrder.PayWay = FastPayWay.Id;
            FastOrder.CashType = InFastOrder.CashType;
            FastOrder.Amoney = Amount;
           
            //用户手续费
            decimal Poundage = Amount * UserCost + FastUserPay.UserCash;
            Poundage = Poundage.Ceiling();
            FastOrder.Poundage = Poundage;
            //用户最终金额
            FastOrder.PayMoney = FastOrder.Amoney - FastOrder.Poundage;

            if (FastOrder.PayMoney < 0)
            {
                DataObj.OutError("2076");
                return;
            }
            FastOrder.UserRate = UserCost;
            FastOrder.AgentRate = AgentCost;
            FastOrder.SysRate = BankCost;
            FastOrder.UserCash = FastUserPay.UserCash;
            FastOrder.SysCash = FastPayWay.Cash;
            FastOrder.SameGet = 0;
            //计算手续费差
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            decimal PaySplit = 0;//代理商费率
            PaySplit = SysAgent.GetSplit(TopAgent.Tier,Entity);
            decimal PayGet = Amount * PaySplit;
            PayGet = PayGet.Floor();
            //一级代理利润
            decimal AgentPayGet = PayGet;
            AgentPayGet = AgentPayGet.Floor();
            FastOrder.AgentPayGet = 0;
            string AgentPath = "|";
            string Split = "|";
            decimal MyGet = PayGet;
            foreach (var p in SysAgentList)
            {
                PaySplit = SysAgent.GetSplit(p.Tier, Entity);
                AgentPath += p.Id + "|";
                MyGet = Amount * PaySplit;//各级代理分润
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
            BankMoney = BankMoney.Floor();
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
            #endregion

            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "到银行卡交易";
            UserTrack.UserName = FastOrder.TNum;
            UserTrack.UId = FastOrder.UId;
            UserTrack.SeavGPSLog(Entity);
            //=======================================

            //=======================分润记录===========================
            MyGet = PayGet;
            int tier = 1;
            foreach (var p in SysAgentList)
            {
                PaySplit = SysAgent.GetSplit(p.Tier, Entity);
                MyGet = Amount * PaySplit;//各级代理分润
                MyGet = MyGet.Floor();
                FastSplit FastSplit = new FastSplit();
                FastSplit.Tnum = FastOrder.TNum;
                FastSplit.Profit = MyGet;
                FastSplit.AgentId = p.Id;
                FastSplit.Tier = p.Tier;
                FastSplit.AddTime = DateTime.Now;
                Entity.FastSplit.AddObject(FastSplit);
                tier++;
            }
            Entity.SaveChanges();
            //=======================请求接口开始===========================
            if (FastOrder.OType == 1 || FastOrder.OType == 2)
            {
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
            }
            else if (FastOrder.OType == 3)
            {
                if (FastPayWay.DllName == "HFPay" || FastPayWay.DllName == "HFJSPay")
                {
                    #region
                    //银联不用请求第三方，直接生成链接
                    FastOrder.PayId = PayPath + "/paycenter/pay-" + FastOrder.Id + ".html?sign=" + ((FastOrder.Id * 100 + 99) + "Pay").GetMD5().Substring(8, 8);
                    Entity.SaveChanges();
                    #endregion
                }
            }
            if (FastOrder.State == 1)
            {
                if (FastOrder.PayState == 1)
                {
                    if (FastOrder.UserState == 1)
                    {
                        FastOrder.State = 3;
                    }
                    else
                    {
                        FastOrder.State = 2;
                    }
                }
                else
                {
                    FastOrder.State = 1;
                }
            }
            else
            {
                FastOrder.State = 0;
                DataObj.OutError("1005");
                return;
            }
            FastOrder.Cols = "TNum,PayId,Amoney,Poundage,State";

            DataObj.Data = FastOrder.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}