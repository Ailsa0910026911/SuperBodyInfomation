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
    public class OrderPayConfigController : InitController
    {
        public OrderPayConfigController()
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
            PayConfigOrder PayConfigOrder = new PayConfigOrder();
            PayConfigOrder = JsonToObject.ConvertJsonToModel(PayConfigOrder, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            if (PayConfigOrder.PCCId.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (PayConfigOrder.X.IsNullOrEmpty() || PayConfigOrder.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == PayConfigOrder.Token);
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
            SysAgent SysAgent = new SysAgent();
            if (!baseUsers.Agent.IsNullOrEmpty()) {
                SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
            }
            //获取系统配置
            //SysSet SysSet = Entity.SysSet.FirstOrDefault();

            PayConfigChange PCC=Entity.PayConfigChange.FirstOrDefault(n=>n.Id==PayConfigOrder.PCCId);
            if (PCC == null) {
                DataObj.OutError("1001");
                return;
            }

            PayConfigOrder.UId = baseUsers.Id;
            PayConfigOrder.Agent = baseUsers.Agent;
            PayConfigOrder.AId = baseUsers.AId;
            PayConfigOrder.Amoney = PCC.BPrice.GetValueOrDefault();

            PayConfigOrder.OrderState = 1;
            PayConfigOrder.PayState = 0;
            PayConfigOrder.AddTime = DateTime.Now;
            //这里是利润计算==========
            //分支机构提成=产品价格-代理商价格
            decimal GetAll = PCC.BPrice.GetValueOrDefault() - PCC.CPrice.GetValueOrDefault();
            //利润舍位
            GetAll = GetAll.Floor();
            //总利润
            PayConfigOrder.AIdGet = (double)GetAll;
            //分支机构佣金设置为0，待分润计算后再写入
            PayConfigOrder.AgentGet = 0;

            PayConfigOrder.AgentState = 0;

            //这里暂时写0，等后面支付再重新计算
            PayConfigOrder.SysRate = 0;
            PayConfigOrder.Poundage = PayConfigOrder.Amoney * (decimal)PayConfigOrder.SysRate;

            //写入订单总表
            Orders Orders = new Orders();
            Orders.UId = PayConfigOrder.UId;
            Orders.TName = PCC.Title;

            Orders.PayType = 0;
            Orders.PayName = "升级费率";

            Orders.RUId = 0;
            Orders.RName = string.Empty;
            Orders.TType = 6;
            Orders.TState = 1;
            Orders.Amoney = PayConfigOrder.Amoney;
            Orders.Poundage = PayConfigOrder.Poundage;
            Orders.AddTime = DateTime.Now;
            Orders.PayState = 0;
            Orders.PayWay = 0;

            Orders.Agent = PayConfigOrder.Agent;
            Orders.AgentState = 0;
            Orders.AId = PayConfigOrder.AId;
            Orders.FId = 0;

            string OrderAddress = PayConfigOrder.OrderAddress;
            if (OrderAddress.IsNullOrEmpty())
            {
                OrderAddress = Utils.GetAddressByGPS(PayConfigOrder.X, PayConfigOrder.Y);
            }
            Orders.OrderAddress = OrderAddress;
            Orders.X = PayConfigOrder.X;
            Orders.Y = PayConfigOrder.Y;

            Entity.Orders.AddObject(Orders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, Orders);

            PayConfigOrder.OId = Orders.TNum;
            Entity.PayConfigOrder.AddObject(PayConfigOrder);
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
            Orders.Cols = "TNum,PayId,Amount,PayState";
            string TNum = Orders.TNum;
            string Sign = (TNum + "NewPay").GetMD5().Substring(8, 8);
            Orders.PayId = PayPath + "/pay/" + TNum + ".html?sign=" + Sign;
            //=======================================

            //获取最佳支付通道
            IList<PayConfig> PayConfigList = Entity.PayConfig.Where(n => n.State == 1).OrderBy(n => n.Cost).ToList();
            IList<SysControl> SysControlList = Entity.SysControl.OrderBy(n => n.Sort).ToList();//SysControl
            IList<SysControl> SCList = new List<SysControl>();
            IList<PayConfig> PCList = new List<PayConfig>();
            foreach (var p in SysControlList)
            {
                SysControl T = p.ChkState();
                if (T.State == 1)
                {
                    SCList.Add(p);
                }
            }
            foreach (var p in PayConfigList)
            {
                SysControl T = SCList.FirstOrDefault(n => n.PayWay == p.Id);
                if (T != null)
                {
                    if (PayConfigOrder.Amoney >= (decimal)T.SNum && PayConfigOrder.Amoney <= (decimal)T.ENum)
                    {
                        PCList.Add(p);
                    }
                }
            }
            IList<PayConfig> CashList = PCList.Where(n => n.GroupType == "Cash").OrderBy(n => n.Cost).ToList();
            IList<PayConfig> BankList = PCList.Where(n => n.GroupType == "Bank").OrderBy(n => n.Cost).ToList();
            IList<PayConfig> WxList = PCList.Where(n => n.GroupType == "WeiXin").OrderBy(n => n.Cost).ToList();
            IList<PayConfig> AliList = PCList.Where(n => n.GroupType == "AliPay").OrderBy(n => n.Cost).ToList();

            IList<PayConfig> List = new List<PayConfig>();
            if (CashList.Count > 0)
            {
                PayConfig Cash = new PayConfig();
                Cash.GroupType = "Cash";
                Cash.Name = "余额支付";
                Cash.State = 1;
                Cash.Cols = "Name,GroupType,State";
                List.Add(Cash);
            }
            if (BankList.Count > 0)
            {
                PayConfig Bank = new PayConfig();
                Bank.GroupType = "Bank";
                Bank.Name = "银联支付";
                Bank.State = 1;
                Bank.Cols = "Name,GroupType,State";
                List.Add(Bank);
            }
            //if (WxList.Count > 0)
            //{
            //    PayConfig Wx = new PayConfig();
            //    Wx.GroupType = "WeiXin";
            //    Wx.Name = "微信支付";
            //    Wx.State = 0;
            //    Wx.Cols = "Name,GroupType,State";
            //    List.Add(Wx);
            //}
            if (AliList.Count > 0)
            {
                PayConfig Ali = new PayConfig();
                Ali.GroupType = "AliPay";
                Ali.Name = "支付宝支付";
                Ali.State = 1;
                Ali.Cols = "Name,GroupType,State";
                List.Add(Ali);
            }
            string PCString = List.EntityToJson();
            JArray PCJson = (JArray)JsonConvert.DeserializeObject(PCString);
            Orders.PayConfig = PCJson;

            Orders.Cols += ",PayConfig";

            //Orders.paycon
            DataObj.Data = Orders.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
