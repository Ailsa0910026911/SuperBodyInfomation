using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using LokFu.Extensions;
using System.Data.Objects;

namespace LokFu.Controllers
{
    public class OrderDaiLi_3_0Controller : InitController
    {
        public OrderDaiLi_3_0Controller()
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
                Log.Write("[OrderDaiLi_3_0]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            DaiLiOrder DaiLiOrder = new DaiLiOrder();
            DaiLiOrder = JsonToObject.ConvertJsonToModel(DaiLiOrder, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            //设置开通等级
            if (DaiLiOrder.Tier.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (DaiLiOrder.X.IsNullOrEmpty() || DaiLiOrder.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }

            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == DaiLiOrder.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }

            //获取分支机构信息
            //AgentType AgentType = Entity.AgentType.FirstOrNew(n => n.Id == DaiLiOrder.Levels);
            //if (AgentType.Id.IsNullOrEmpty()) {
            //    DataObj.OutError("1000");
            //    return;
            //}
            //if (AgentType.AgentID != baseUsers.Agent) { 
            //    //用户代理与开通等级代理不同
            //    DataObj.OutError("1000");
            //    return;
            //}
            SysAgent SysAgent = Entity.SysAgent.FirstOrNew(o => o.LinkMobile == baseUsers.UserName);
            if (!SysAgent.LinkMobile.IsNullOrEmpty())
            {
                if (SysAgent.Tier < 5 || DaiLiOrder.Tier >= SysAgent.Tier) 
                {
                    DataObj.OutError("9000");
                    return;
                }
            }

            DaiLiOrder.UId = baseUsers.Id;
            DaiLiOrder.UserName = baseUsers.UserName;
            DaiLiOrder.TureName = baseUsers.TrueName;

            DaiLiOrder.Agent = baseUsers.Agent;
            DaiLiOrder.AId = baseUsers.AId;
           // DaiLiOrder.Amoney = AgentType.RegisterFee;

            DaiLiOrder.OrderState = 1;
            DaiLiOrder.PayState = 0;
            DaiLiOrder.AddTime = DateTime.Now;
            DaiLiOrder.SameGet = 0;
            //这里是利润计算==========
            DaiLiOrder.AIdGet =Convert.ToDouble(DaiLiOrder.Amoney);//总利润
            DaiLiOrder.AgentGet = 0;//分支机构佣金设置为0，待分润计算后再写入
            DaiLiOrder.AgentState = 0;

            //写入订单总表
            Orders Orders = new Orders();
            Orders.UId = DaiLiOrder.UId;
            Orders.TName = "自助开通代理";
            Orders.RUId = 0;
            Orders.RName = string.Empty;
            Orders.TType = 10;
            Orders.TState = 1;
            Orders.Amoney = DaiLiOrder.Amoney;
            Orders.Poundage = 0;
            Orders.AddTime = DateTime.Now;
            Orders.PayState = 0;
            Orders.PayWay = 15;

            Orders.Agent = DaiLiOrder.Agent;
            Orders.AgentState = 0;
            Orders.AId = DaiLiOrder.AId;
            Orders.FId = 0;

            string OrderAddress = DaiLiOrder.OrderAddress;
            if (OrderAddress.IsNullOrEmpty())
            {
                OrderAddress = Utils.GetAddressByGPS(DaiLiOrder.X, DaiLiOrder.Y);
            }
            Orders.OrderAddress = OrderAddress;
            Orders.X = DaiLiOrder.X;
            Orders.Y = DaiLiOrder.Y;

            Entity.Orders.AddObject(Orders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, Orders);

            DaiLiOrder.OId = Orders.TNum;
            Entity.DaiLiOrder.AddObject(DaiLiOrder);
            Entity.SaveChanges();

            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = Orders.PayName;
            UserTrack.UserName = Orders.TNum;
            UserTrack.GPSAddress = Orders.OrderAddress;
            UserTrack.GPSX = Orders.X;
            UserTrack.GPSY = Orders.Y;
            Orders.SeavGPSLog(UserTrack, Entity);
            //=======================================
            Orders.SendMsg(Entity);//发送消息类
            //=======================================
            Orders.Cols = "TNum,PayId,Amoney,PayState";
            string TNum = Orders.TNum;
            string Sign = (TNum + "NewPay").GetMD5().Substring(8, 8);
           // Orders.PayId = PayPath + "/pay/" + TNum + ".html?sign=" + Sign;
            Orders.PayId = PayPath + "/mobile/orders/GoPay.html?sign=" + Sign + "&tnum=" + TNum;
            //=======================================

            //获取最佳支付通道
            #region 结算中心
            //IList<PayConfig> PayConfigList = Entity.PayConfig.Where(n => n.State == 1).OrderBy(n => n.Cost).ToList();
            //IList<SysControl> SysControlList = Entity.SysControl.OrderBy(n => n.Sort).ToList();//SysControl
            //IList<SysControl> SCList = new List<SysControl>();
            //IList<PayConfig> PCList = new List<PayConfig>();
            //foreach (var p in SysControlList)
            //{
            //    SysControl T = p.ChkState();
            //    if (T.State == 1)
            //    {
            //        SCList.Add(p);
            //    }
            //}
            //foreach (var p in PayConfigList)
            //{
            //    SysControl T = SCList.FirstOrDefault(n => n.PayWay == p.Id);
            //    if (T != null)
            //    {
            //        if (Orders.Amoney >= (decimal)T.SNum && Orders.Amoney <= (decimal)T.ENum)
            //        {
            //            PCList.Add(p);
            //        }
            //    }
            //}
            //IList<PayConfig> CashList = PCList.Where(n => n.GroupType == "Cash").OrderBy(n => n.Cost).ToList();
            //IList<PayConfig> BankList = PCList.Where(n => n.GroupType == "Bank").OrderBy(n => n.Cost).ToList();
            //IList<PayConfig> WxList = PCList.Where(n => n.GroupType == "WeiXin").OrderBy(n => n.Cost).ToList();
            //IList<PayConfig> AliList = PCList.Where(n => n.GroupType == "AliPay").OrderBy(n => n.Cost).ToList();

            //IList<PayConfig> List = new List<PayConfig>();
            //if (CashList.Count > 0)
            //{
            //    PayConfig Cash = new PayConfig();
            //    Cash.GroupType = "Cash";
            //    Cash.Name = "余额支付";
            //    Cash.State = 1;
            //    Cash.Cols = "Name,GroupType,State";
            //    List.Add(Cash);
            //}
            //if (BankList.Count > 0)
            //{
            //    PayConfig Bank = new PayConfig();
            //    Bank.GroupType = "Bank";
            //    Bank.Name = "银联支付";
            //    Bank.State = 1;
            //    Bank.Cols = "Name,GroupType,State";
            //    List.Add(Bank);
            //}
            //if (WxList.Count > 0)
            //{
            //    PayConfig Wx = new PayConfig();
            //    Wx.GroupType = "WeiXin";
            //    Wx.Name = "微信支付";
            //    Wx.State = 0;
            //    Wx.Cols = "Name,GroupType,State";
            //    List.Add(Wx);
            //}
            //if (AliList.Count > 0)
            //{
            //    PayConfig Ali = new PayConfig();
            //    Ali.GroupType = "AliPay";
            //    Ali.Name = "支付宝支付";
            //    Ali.State = 1;
            //    Ali.Cols = "Name,GroupType,State";
            //    List.Add(Ali);
            //}
            //string PCString = List.EntityToJson();
            //JArray PCJson = (JArray)JsonConvert.DeserializeObject(PCString);
            //Orders.PayConfig = PCJson;

            //Orders.Cols += ",PayConfig";
            #endregion
            //Orders.paycon
            DataObj.Data = Orders.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
