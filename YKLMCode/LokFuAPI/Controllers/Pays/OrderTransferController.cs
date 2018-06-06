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
    public class OrderTransferController : InitController
    {
        public OrderTransferController()
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
                Log.Write("[OrderTransfer]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            OrderTransfer OrderTransfer = new OrderTransfer();
            OrderTransfer = JsonToObject.ConvertJsonToModel(OrderTransfer, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            string Tag = "Transfer";
            SysControl SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == Tag);
            SysControl syscontrol = SysControl.ChkState();
            if (syscontrol.State != 1)
            {
                DataObj.OutError("1005");
                return;
            }

            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == OrderTransfer.Token);
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

            if (baseUsers.UserName == OrderTransfer.ToUserName)//不能给自己转帐
            {
                DataObj.OutError("6031");
                return;
            }
            if (baseUsers.Amount < OrderTransfer.PayMoney)//余额不足
            {
                DataObj.OutError("6001");
                return;
            }

            //获取收款用户信息
            Users ToUsers = Entity.Users.FirstOrDefault(n => n.UserName == OrderTransfer.ToUserName);
            if (ToUsers == null)//用户不存在
            {
                DataObj.OutError("6004");
                return;
            }
            if (ToUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("6003");
                return;
            }
            //if (ToUsers.CardStae != 2)//未实名认证
            //{
            //    DataObj.OutError("6006");
            //    return;
            //}

            //开始处理参数
            if (OrderTransfer.PayMoney.IsNullOrEmpty())//转帐金额
            {
                DataObj.OutError("1000");
                return;
            }
            OrderTransfer.PayMoney = OrderTransfer.PayMoney.FormatMoney();
            if (OrderTransfer.PayType.IsNullOrEmpty())
            {
                OrderTransfer.PayType = 0;
            }
            if (OrderTransfer.X.IsNullOrEmpty() || OrderTransfer.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (OrderTransfer.AId.IsNullOrEmpty())
            {
                //支付方式，用于取出支付费率信息，计算各接口金额
                DataObj.OutError("1000");
                return;
            }

            //获取系统支付配置
            PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == OrderTransfer.AId && n.State == 1);
            if (PayConfig == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //转帐目前只支持余额
            if (PayConfig.DllName != "Cash")
            {
                DataObj.OutError("1000");
                return;
            }
            //获取分支机构信息
            SysAgent SysAgent = new SysAgent();
            
            //获取系统配置
            //SysSet SysSet = Entity.SysSet.FirstOrDefault();

            OrderTransfer.UId = baseUsers.Id;
            OrderTransfer.RUId = ToUsers.Id;

            if (OrderTransfer.IsMe == 1)//付方出手续费
            {
                //获取付方支付配置
                UserPay UserPay = Entity.UserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PId == OrderTransfer.AId);
                if (UserPay == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                //获取付方机构信息
                if (!baseUsers.Agent.IsNullOrEmpty())
                {
                    SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
                }

                //手续费
                OrderTransfer.Poundage = OrderTransfer.PayMoney * (decimal)UserPay.Cost;
                //手续费进位
                OrderTransfer.Poundage = OrderTransfer.Poundage.Ceiling();
                //总金额=转帐金额+手续费
                OrderTransfer.Amoney = OrderTransfer.PayMoney + OrderTransfer.Poundage;
                //第三方支付通道率
                OrderTransfer.SysRate = (double)PayConfig.Cost;
                //商户费率
                OrderTransfer.UserRate = UserPay.Cost;
                //这里是利润计算==========
                //利润=总金额-转到帐-支付手续费
                decimal GetAll = OrderTransfer.Amoney - OrderTransfer.PayMoney - OrderTransfer.Amoney * (decimal)OrderTransfer.SysRate;
                //利润舍位
                GetAll = GetAll.Floor();
                //总利润
                OrderTransfer.AIdPayGet = (double)GetAll;
                //分支机构佣金设置为0，待分润计算后再写入
                OrderTransfer.AgentPayGet = 0;
            }
            else {//收方出手续费
                //获取收方支付配置
                UserPay UserPay = Entity.UserPay.FirstOrDefault(n => n.UId == ToUsers.Id && n.PId == OrderTransfer.AId);
                if (UserPay == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                //获取收方机构信息
                if (!ToUsers.Agent.IsNullOrEmpty())
                {
                    SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == ToUsers.Agent);
                }
                //总金额=转帐金额+手续费
                OrderTransfer.Amoney = OrderTransfer.PayMoney;
                //手续费
                OrderTransfer.Poundage = OrderTransfer.Amoney * (decimal)UserPay.Cost;
                //手续费进位
                OrderTransfer.Poundage = OrderTransfer.Poundage.Ceiling();
                //到帐金额
                OrderTransfer.PayMoney = OrderTransfer.Amoney - OrderTransfer.Poundage;
                //第三方支付通道率
                OrderTransfer.SysRate = (double)PayConfig.Cost;
                //商户费率
                OrderTransfer.UserRate = UserPay.Cost;
                //这里是利润计算==========
                //利润=总金额-转到帐-支付手续费
                decimal GetAll = OrderTransfer.Amoney - OrderTransfer.PayMoney - OrderTransfer.Amoney * (decimal)OrderTransfer.SysRate;
                //利润舍位
                GetAll = GetAll.Floor();
                //总利润
                OrderTransfer.AIdPayGet = (double)GetAll;
                //分支机构佣金设置为0，待分润计算后再写入
                OrderTransfer.AgentPayGet =  0;
            }

            OrderTransfer.Agent = SysAgent.Id;//分支机构Id
            OrderTransfer.AId = baseUsers.AId;
            OrderTransfer.FId = 0;
            OrderTransfer.OrderState = 1;
            OrderTransfer.PayState = 0;
            OrderTransfer.AgentState = 0;
            OrderTransfer.AddTime = DateTime.Now;

            //写入前，判断交易金额限制
            if (OrderTransfer.Amoney < syscontrol.SNum || OrderTransfer.Amoney > syscontrol.ENum)
            {
                DataObj.OutError("1006");
                return;
            }
            //写入订单总表
            Orders Orders = new Orders();
            Orders.UId = OrderTransfer.UId;
            Orders.TName = "自 " + baseUsers.TrueName + " To " + ToUsers.TrueName;

            Orders.PayType = OrderTransfer.PayType;
            switch (Orders.PayType)
            {
                case 0:
                    Orders.PayName = "转帐";
                    break;
                case 1:
                    Orders.PayName = "当面付";
                    break;
                case 2:
                    Orders.PayName = "收银台-钱包";
                    break;
            }

            Orders.RUId = ToUsers.Id;
            Orders.RName = ToUsers.TrueName;
            Orders.TType = 3;
            Orders.TState = 1;
            Orders.Amoney = OrderTransfer.Amoney;
            Orders.Poundage = OrderTransfer.Poundage;
            Orders.AddTime = DateTime.Now;
            Orders.PayState = 0;
            Orders.PayWay = PayConfig.Id;

            Orders.Agent = OrderTransfer.Agent;
            Orders.AgentState = 0;
            Orders.AId = OrderTransfer.AId;
            Orders.FId = 0;

            string OrderAddress = OrderTransfer.OrderAddress;
            if (OrderAddress.IsNullOrEmpty())
            {
                OrderAddress = Utils.GetAddressByGPS(OrderTransfer.X, OrderTransfer.Y);
            }
            Orders.OrderAddress = OrderAddress;
            Orders.X = OrderTransfer.X;
            Orders.Y = OrderTransfer.Y;

            Orders.Remark = OrderTransfer.Remark;

            Entity.Orders.AddObject(Orders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, Orders);

            OrderTransfer.OId = Orders.TNum;
            Entity.OrderTransfer.AddObject(OrderTransfer);
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

            DataObj.Data = Orders.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
