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
    public class OrderHouseController : InitController
    {
        public OrderHouseController()
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
                Log.Write("[OrderHouse]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            OrderHouse OrderHouse = new OrderHouse();
            OrderHouse = JsonToObject.ConvertJsonToModel(OrderHouse, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            string Tag = "House";
            SysControl SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == Tag);
            SysControl syscontrol = SysControl.ChkState();
            if (syscontrol.State != 1)
            {
                DataObj.OutError("1005");
                return;
            }

            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == OrderHouse.Token);
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
            //开始处理参数
            if (OrderHouse.HouseOwner.IsNullOrEmpty() || OrderHouse.Bank.IsNullOrEmpty() || OrderHouse.Deposit.IsNullOrEmpty() || OrderHouse.CardNum.IsNullOrEmpty() || OrderHouse.Mobile.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (OrderHouse.MonthRent.IsNullOrEmpty() || OrderHouse.PayMonth.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (OrderHouse.X.IsNullOrEmpty() || OrderHouse.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (OrderHouse.AId.IsNullOrEmpty())
            {//支付方式，用于取出支付费率信息，计算各接口金额
                DataObj.OutError("1000");
                return;
            }
            if (OrderHouse.AId == 4)
            {//支付方式，用于取出支付费率信息，计算各接口金额
                DataObj.OutError("6022");
                return;
            }
            if (OrderHouse.SecurityMoney.IsNullOrEmpty()) {
                OrderHouse.SecurityMoney = 0;
            }
            //获取用户支付配置
            //UserPay UserPay = Entity.UserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PId == OrderHouse.AId);
            //if (UserPay == null) {
            //    DataObj.OutError("1000");
            //    return;
            //}

            OrderHouse.SecurityMoney = OrderHouse.SecurityMoney.FormatMoney();
            OrderHouse.MonthRent = OrderHouse.MonthRent.FormatMoney();

            //获取系统支付配置
            PayConfig PayConfig = Entity.PayConfig.FirstOrDefault(n => n.Id == OrderHouse.AId && n.State == 1);
            if (PayConfig == null)
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

            if (OrderHouse.TrunType.IsNullOrEmpty())
            {
                OrderHouse.TrunType = 0;
            }
            if (OrderHouse.TrunType != 0)
            {
                OrderHouse.TrunType = 1;
            }
            decimal Peier = 0;
            if (OrderHouse.TrunType == 0)
            {
                if (baseUsers.HasT0 != 1)
                {
                    //T0黑名单
                    DataObj.OutError("6020");
                    return;
                }
                //验证是否满足T0
                int Day = (DateTime.Now - baseUsers.AddTime).Days;
                int OrderCount = Entity.Orders.Count(n => (n.TType == 2 || n.TType == 5) && n.PayState == 2 && n.UId == baseUsers.Id);
                decimal? OrderMoney = Entity.Orders.Where(n => (n.TType == 2 || n.TType == 5) && n.PayState == 2 && n.UId == baseUsers.Id).Sum(n => (decimal?)n.Amoney);
                if (OrderMoney.IsNullOrEmpty()) {
                    OrderMoney = 0;
                }
                if (Day >= SysSet.CashDay && OrderCount >= SysSet.CashNum && OrderMoney >= SysSet.CashMoney)
                {

                }
                else
                {
                    DataObj.OutError("6020");
                    return;
                }
                //验证是否在快速提现时段内
                TaskTimeSet TaskTimeSet = Entity.TaskTimeSet.FirstOrDefault(n => n.STime <= DateTime.Now && n.ETime >= DateTime.Now && n.TId == 1);
                if (TaskTimeSet == null) {
                    DataObj.OutError("6018");
                    return;
                }
                Peier = TaskTimeSet.AllMoney - TaskTimeSet.UsedMoney;
            }

            OrderHouse.UId = baseUsers.Id;
            double House = SysSet.House;
            double Cost = (double)PayConfig.Cost;
            //余额支付不收手续费
            if (PayConfig.DllName == "Cash") {
                House = 0;
                Cost = 0;
            }
            if (OrderHouse.TrunType == 0)
            {
                //总房租=月租金*月数+压金
                OrderHouse.PayMoney = OrderHouse.MonthRent * OrderHouse.PayMonth + OrderHouse.SecurityMoney;
                //手续费=总房租*(付房租系统费率+加急费率)+提现服务费
                OrderHouse.Poundage = OrderHouse.PayMoney * ((decimal)House + baseUsers.Cash0) + baseUsers.ECash0;
                //手续费进位
                OrderHouse.Poundage = OrderHouse.Poundage.Ceiling();
                //总付款金额=房租+手续费
                OrderHouse.Amoney = OrderHouse.PayMoney + OrderHouse.Poundage;
                //第三方支付通道率
                OrderHouse.SysRate = Cost;
                //商户费率
                OrderHouse.UserRate = House;
                //提现费率
                OrderHouse.CashRate = OrderHouse.PayMoney * SysSet.Cash0 + SysSet.ECash0;
                //这里是利润计算==========
                //利润=总支付金额-房租-支付手续费-提现服务费
                decimal GetAll = OrderHouse.Poundage - OrderHouse.PayMoney * ((decimal)Cost + SysSet.Cash0) - baseUsers.ECash0;
                //利润舍位
                GetAll = GetAll.Floor();
                //总利润
                OrderHouse.AIdPayGet = (double)GetAll;
                //分支机构佣金设置为0，待分润计算后再写入
                OrderHouse.AgentPayGet = 0;
            }
            else {
                //总房租=月租金*月数+压金
                OrderHouse.PayMoney = OrderHouse.MonthRent * OrderHouse.PayMonth + OrderHouse.SecurityMoney;
                //手续费=总房租*付房租系统费率
                OrderHouse.Poundage = OrderHouse.PayMoney * ((decimal)House + baseUsers.Cash1) + baseUsers.ECash1;
                //总付款金额=房租+手续费
                OrderHouse.Amoney = OrderHouse.PayMoney + OrderHouse.Poundage;
                //第三方支付通道率
                OrderHouse.SysRate = Cost;
                //商户费率
                OrderHouse.UserRate = House;
                //提现服务费--T+1无手续费
                OrderHouse.CashRate = OrderHouse.PayMoney * SysSet.Cash1 + SysSet.ECash1;
                //这里是利润计算==========
                //利润=总支付金额-房租-支付手续费
                decimal GetAll = OrderHouse.Poundage - OrderHouse.Amoney * ((decimal)Cost + SysSet.Cash1) - baseUsers.ECash1;
                //总利润
                OrderHouse.AIdPayGet = (double)GetAll;
                //分支机构佣金设置为0，待分润计算后再写入
                OrderHouse.AgentPayGet = 0;
            }
            if (OrderHouse.TrunType == 0)//T0时才验证
            {
                //需要支付房东金额
                decimal TotalPrice = OrderHouse.PayMoney;
                //验证是否配额充足
                if (Peier < TotalPrice)
                {
                    DataObj.OutError("6019");
                    return;
                }
            }

            OrderHouse.Agent = SysAgent.Id;//分支机构Id
            OrderHouse.AId = baseUsers.AId;
            OrderHouse.FId = 0;
            OrderHouse.OrderState = 1;
            OrderHouse.PayState = 0;
            OrderHouse.AgentState = 0;
            OrderHouse.FState = 0;
            OrderHouse.AddTime = DateTime.Now;

            //写入前，判断交易金额限制
            if (OrderHouse.Amoney < syscontrol.SNum || OrderHouse.Amoney > syscontrol.ENum)
            {
                DataObj.OutError("1006");
                return;
            }
            //写入订单总表
            Orders Orders = new Orders();
            Orders.Remark = OrderHouse.Remark;
            Orders.UId = OrderHouse.UId;
            Orders.TName = OrderHouse.HouseOwner + "的房租";

            Orders.PayType = 0;
            Orders.PayName = "房租";

            Orders.RUId = 0;
            Orders.RName = string.Empty;
            Orders.TType = 5;
            Orders.TState = 1;
            Orders.Amoney = OrderHouse.Amoney;
            Orders.Poundage = OrderHouse.Poundage;
            Orders.AddTime = DateTime.Now;
            Orders.PayState = 0;
            Orders.PayWay = PayConfig.Id;

            Orders.Agent = OrderHouse.Agent;
            Orders.AgentState = 0;
            Orders.AId = OrderHouse.AId;
            Orders.FId = 0;

            string OrderAddress = OrderHouse.OrderAddress;
            if (OrderAddress.IsNullOrEmpty())
            {
                OrderAddress = Utils.GetAddressByGPS(OrderHouse.X, OrderHouse.Y);
            }
            Orders.OrderAddress = OrderAddress;
            Orders.X = OrderHouse.X;
            Orders.Y = OrderHouse.Y;

            Orders.TrunType = OrderHouse.TrunType;

            Entity.Orders.AddObject(Orders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, Orders);

            OrderHouse.OId = Orders.TNum;
            Entity.OrderHouse.AddObject(OrderHouse);
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
