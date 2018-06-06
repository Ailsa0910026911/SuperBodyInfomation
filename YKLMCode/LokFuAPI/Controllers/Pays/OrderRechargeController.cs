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
    public class OrderRechargeController : InitController
    {
        public OrderRechargeController()
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
                Log.Write("[OrderRecharge]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            OrderRecharge OrderRecharge = new OrderRecharge();
            OrderRecharge = JsonToObject.ConvertJsonToModel(OrderRecharge, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);
            int PayWay = OrderRecharge.PayWay;
            if (PayWay.IsNullOrEmpty()) {
                PayWay = OrderRecharge.AId;
            }
            if (PayWay.IsNullOrEmpty())
            {
                //支付方式，用于取出支付费率信息，计算各接口金额
                DataObj.OutError("1000");
                return;
            }
            if (OrderRecharge.PayType.IsNullOrEmpty())
            {
                OrderRecharge.PayType = 0;
            }
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == OrderRecharge.Token);
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

            int InType = 0;
            if (!OrderRecharge.InType.IsNullOrEmpty())
            {
                InType = 1;
            }
            SysControl SysControl = new SysControl();
            bool IsNew = true;
            if (OrderRecharge.Id.IsNullOrEmpty())
            {
                //这是旧逻辑,有一些旧版还在用
                #region 版本比较 升级之后比较长时间后可以考滤删除版本判断代码
                SysAgent vSysAgent = Entity.SysAgent.FirstOrDefault(o => o.Id == baseUsers.Agent);
                if (vSysAgent == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                IsNew = BizExt.NewOrOldVersion(vSysAgent, Equipment, this.Entity);
                #endregion
                if (IsNew)//新版
                {
                    if (InType == 1)
                    {
                        SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == "RecMoneyLocal" && n.PayWay == PayWay && n.LagEntryDay > 0);
                    }
                    else
                    {
                        SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == "RecMoneyLocal" && n.PayWay == PayWay && n.LagEntryDay == 0);
                    }
                }
                else//旧版
                {
                    SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == "RecMoneyLocal" && n.PayWay == PayWay);
                }
            }
            else
            {
                SysControl = Entity.SysControl.FirstOrDefault(n => n.Id == OrderRecharge.Id);
                PayWay = SysControl.PayWay;
                if (SysControl.LagEntryDay > 0)
                {
                    InType = 1;
                }
                else
                {
                    InType = 0;
                }
            }
            //获取系统支付配置
            if (SysControl == null)
            {
                DataObj.OutError("1005");
                return;
            }
            if (InType == 1 && (SysControl.LagEntryDay.IsNullOrEmpty() || SysControl.LagEntryNum.IsNullOrEmpty()))
            {
                DataObj.Msg = "请升级到最新版再发起Tn到帐交易！";
                DataObj.OutError("1005");
                return;
            }
            SysControl syscontrol = SysControl.ChkState();
            if (syscontrol.State != 1)
            {
                DataObj.OutError("1005");
                return;
            }

            //开始处理参数
            if (OrderRecharge.Amoney.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            OrderRecharge.Amoney = OrderRecharge.Amoney.FormatMoney();

            if (OrderRecharge.X.IsNullOrEmpty() || OrderRecharge.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }

            //获取用户支付配置
            UserPay UserPay = Entity.UserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PId == PayWay);
            if (UserPay == null)
            {
                DataObj.Msg = "你当前版本不支持该交易，请等待新版本发布及升级！";
                DataObj.OutError("1000");
                return;
            }
            //获取系统支付配置
            PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == PayWay && n.State == 1);
            if (PayConfig == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (PayConfig.GroupType != "Bank")
            {
                DataObj.OutError("1000");
                return;
            }
            //获取分支机构信息
            SysAgent SysAgent = new SysAgent();
            if (!baseUsers.Agent.IsNullOrEmpty()) {
                SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
            }
            //获取系统配置
            SysSet SysSet = Entity.SysSet.FirstOrDefault();
            if (InType == 1)
            {//客户端传来T+N但是系统没开启时无效
                if (SysSet.LagEntry == 0)
                {
                    InType = 0;
                }
            }
            //本算法在当系统费用为0时有问题，系统费用为0的T5会导致代理分不到钱。
            if (InType == 1)
            {
                //T+N免手续费
                OrderRecharge.Poundage = 0;
                //商户费率
                OrderRecharge.UserRate = 0;
                //T+n时，代理佣金为 交易金额*费率

                decimal AgentPayGet = OrderRecharge.Amoney * SysSet.AgentGet;
                OrderRecharge.AgentPayGet = (Double)AgentPayGet;
                //佣金舍位
                OrderRecharge.AgentPayGet = OrderRecharge.AgentPayGet.Floor();
            }
            else {
                //手续费
                OrderRecharge.Poundage = OrderRecharge.Amoney * (decimal)UserPay.Cost;
                //手续费取进
                OrderRecharge.Poundage = OrderRecharge.Poundage.Ceiling();
                //商户费率
                OrderRecharge.UserRate = UserPay.Cost;
                //分支机构佣金设置为0，待分润计算后再写入
                OrderRecharge.AgentPayGet = 0;
            }

            OrderRecharge.UId = baseUsers.Id;

            //到帐金额=支付金额-手续费
            OrderRecharge.PayMoney = OrderRecharge.Amoney - OrderRecharge.Poundage;
            //第三方支付通道率
            OrderRecharge.SysRate = (double)PayConfig.Cost;

            //这里是利润计算==========
            //利润=总金额-到帐-支付手续费
            decimal GetAll = OrderRecharge.Amoney - OrderRecharge.PayMoney - OrderRecharge.Amoney * (decimal)OrderRecharge.SysRate;
            //利润舍位
            GetAll = GetAll.Floor();
            //总利润
            OrderRecharge.AIdPayGet = (double)GetAll;
            OrderRecharge.PayWay = PayWay;
            OrderRecharge.Agent = SysAgent.Id;//分支机构Id
            OrderRecharge.AId = baseUsers.AId;
            OrderRecharge.FId = 0;
            OrderRecharge.OrderState = 1;
            OrderRecharge.PayState = 0;
            OrderRecharge.AgentState = 0;
            OrderRecharge.AddTime = DateTime.Now;

            //写入前，判断交易金额限制
            if (OrderRecharge.Amoney < syscontrol.SNum || OrderRecharge.Amoney > syscontrol.ENum) {
                DataObj.OutError("1006");
                return;
            }
    
            //写入订单总表
            Orders Orders = new Orders();
            Orders.UId = OrderRecharge.UId;
            Orders.TName = baseUsers.TrueName;

            Orders.PayType = OrderRecharge.PayType;
            switch (Orders.PayType) {
                case 0:
                    Orders.ComeWay = 1;
                    Orders.PayName = "充值-" + PayConfig.Name;
                    break;
                case 1:
                    Orders.ComeWay = 1;
                    Orders.PayName = "短信收款-" + PayConfig.Name;
                    break;
                case 2:
                    Orders.ComeWay = 1;
                    Orders.PayName = "本地收款-" + PayConfig.Name;
                    break;
                case 3:
                    Orders.ComeWay = 1;
                    Orders.PayName = "当面付-" + PayConfig.Name;
                    break;
                case 4:
                    Orders.ComeWay = 2;
                    Orders.PayName = "收银台-" + PayConfig.Name;
                    break;
            }

            Orders.RUId = 0;
            Orders.RName = string.Empty;
            Orders.TType = 1;
            Orders.TState = 1;
            Orders.Amoney = OrderRecharge.Amoney;
            Orders.Poundage = OrderRecharge.Poundage;
            Orders.AddTime = DateTime.Now;
            Orders.PayState = 0;
            Orders.PayWay = PayConfig.Id;

            Orders.Agent = OrderRecharge.Agent;
            Orders.AgentState = 0;
            Orders.AId = OrderRecharge.AId;
            Orders.FId = 0;
            //Orders.ComeWay = 1;

            string OrderAddress = OrderRecharge.OrderAddress;
            if (OrderAddress.IsNullOrEmpty())
            {
                OrderAddress = Utils.GetAddressByGPS(OrderRecharge.X, OrderRecharge.Y);
            }
            Orders.OrderAddress = OrderAddress;
            Orders.X = OrderRecharge.X;
            Orders.Y = OrderRecharge.Y;

            if (InType == 1)
            {
                if (IsNew)//新版
                {
                    Orders.LagEntryDay = SysControl.LagEntryDay;
                    Orders.LagEntryNum = SysControl.LagEntryNum;
                }
                else
                {
                    Orders.LagEntryDay = SysSet.LagEntryDay;
                    Orders.LagEntryNum = SysSet.LagEntryNum;
                }
            }
            else
            {
                Orders.LagEntryDay = 0;
                Orders.LagEntryNum = 0;
            }

            Entity.Orders.AddObject(Orders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, Orders);

            OrderRecharge.OId = Orders.TNum;
            Entity.OrderRecharge.AddObject(OrderRecharge);
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
            Orders.Cols = "Id,TNum,PayId";
            string TNum = Orders.TNum;
            string Sign=(TNum + "NewPay").GetMD5().Substring(8, 8);
            Orders.PayId = PayPath + "/pay/" + TNum + ".html?sign=" + Sign;
            //=======================================
            DataObj.Data = Orders.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
