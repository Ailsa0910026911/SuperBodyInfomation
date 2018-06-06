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
    public class VIPOrderController : InitController
    {
        public VIPOrderController()
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
                Log.Write("[OrderPayConfig]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            VIPOrder VIPOrder = new VIPOrder();
            VIPOrder = JsonToObject.ConvertJsonToModel(VIPOrder, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);
            if (VIPOrder.X.IsNullOrEmpty() || VIPOrder.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == VIPOrder.Token);
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
            if (baseUsers.IsVip == 1)//是VIP不能再生成
            {
                DataObj.OutError("9001");
                return;
            }

            //获取分支机构信息
            SysAgent SysAgent = new SysAgent();
            if (!baseUsers.Agent.IsNullOrEmpty())
            {
                SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
            }
            VIPOrder.UId = baseUsers.Id;
            //VIPOrder.Agent = baseUsers.Agent;
            //PayConfigOrder.AId = baseUsers.AId;
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            VIPOrder.Amoney = SysMoneySet.VipPrice;
            VIPOrder.VName = "升级VIP";
            VIPOrder.State = 1;
            VIPOrder.PayState = 0;
            VIPOrder.AddTime = DateTime.Now;
            //这里是利润计算==========
            //利润舍位
            VIPOrder.SplitMoney = 0;
            VIPOrder.SplitState = 0;
            VIPOrder.Poundage = 0;
            VIPOrder.HFGet = 0;
            VIPOrder.PayWay = 15;
            VIPOrder.Days = 99999999;
            VIPOrder.UserState = 0;
            VIPOrder.Agent = baseUsers.Agent;
            VIPOrder.SameGet = 0;
            //写入订单总表
            Orders Orders = new Orders();
            Orders.UId = VIPOrder.UId;
            Orders.TName = "VIP升级";
            Orders.PayType = 0;
            Orders.PayName = "升级VIP";

            Orders.RUId = 0;
            Orders.RName = string.Empty;
            Orders.TType = 6;
            Orders.TState = 1;
            Orders.Amoney = VIPOrder.Amoney;
            Orders.Poundage = VIPOrder.Poundage;
            Orders.AddTime = DateTime.Now;
            Orders.PayState = 0;
            Orders.PayWay = 15;

            Orders.Agent = baseUsers.Agent;
            Orders.AgentState = 0;
            Orders.AId = baseUsers.AId;
            Orders.FId = 0;

            string OrderAddress = VIPOrder.OrderAddress;
            if (OrderAddress.IsNullOrEmpty())
            {
                OrderAddress = Utils.GetAddressByGPS(VIPOrder.X, VIPOrder.Y);
            }
            Orders.OrderAddress = OrderAddress;
            Orders.X = VIPOrder.X;
            Orders.Y = VIPOrder.Y;

            Entity.Orders.AddObject(Orders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, Orders);

            VIPOrder.TNum = Orders.TNum;
            Entity.VIPOrder.AddObject(VIPOrder);
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
            //Orders.PayId = PayPath + "/pay/" + TNum + ".html?sign=" + Sign;
            Orders.PayId = PayPath + "/mobile/orders/GoPay.html?sign=" + Sign+"&tnum="+TNum;
            //=======================================

            #region 走结算中心
            //获取最佳支付通道
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
            //        if (VIPOrder.Amoney >= (decimal)T.SNum && VIPOrder.Amoney <= (decimal)T.ENum)
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
