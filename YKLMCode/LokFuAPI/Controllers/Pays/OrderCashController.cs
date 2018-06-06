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
using LokFu.Repositories.SqlServer;
using System.Data.Objects.SqlClient;
namespace LokFu.Controllers
{
    public class OrderCashController : InitController
    {
        public OrderCashController()
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
                Log.Write("[OrderCash]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //System.Threading.Thread.Sleep(60000 * 60 * 24);
            OrderCash OrderCash = new OrderCash();
            OrderCash = JsonToObject.ConvertJsonToModel(OrderCash, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            if (OrderCash.TrunType.IsNullOrEmpty())
            {
                OrderCash.TrunType = 0;
            }
            if (OrderCash.TrunType != 0)
            {
                OrderCash.TrunType = 1;
            }

            string Tag = "Cash";
            if (OrderCash.TrunType == 0)
            {
                Tag = "CashT0";
            }
            if (OrderCash.TrunType == 1)
            {
                Tag = "CashT1";
            }
            SysControl SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == Tag);
            SysControl syscontrol = SysControl.ChkState();
            if (syscontrol.State != 1)
            {
                DataObj.OutError("1005");
                return;
            }

            if (OrderCash.PayPwd.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (OrderCash.PayPwd.Length < 6)//6位及以上
            {
                DataObj.OutError("1000");
                return;
            }
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == OrderCash.Token);
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
            if (baseUsers.PayLock == 1)//密码错误太多次锁定
            {
                DataObj.OutError("2050");
                return;
            }
            if (baseUsers.StopPayState == 2)//禁止支付
            {
                DataObj.OutError("6060");
                return;
            }

            //获取系统配置
            SysSet SysSet = Entity.SysSet.FirstOrDefault();

            //这里是执行指纹解锁
            bool IfCheckOk = true;
            if (OrderCash.PayPwd.Substring(0, 3) == "HF_")
            {
                string PassWord = OrderCash.PayPwd;
                PassWord = PassWord.Replace("HF_", "");
                string Token = baseUsers.Token;
                Token = Token + "GoodPay";
                string Md5Token = Token.GetMD5().ToUpper();
                string Pass = Md5Token.Substring(0, 4) + Md5Token.Substring(Md5Token.Length - 4, 4);
                if (Pass != PassWord)
                {
                    IfCheckOk = false;
                }
            }
            else if (baseUsers.PayPwd != OrderCash.PayPwd.GetPayMD5())
            {
                //原支付密码错误
                IfCheckOk = false;
            }
            if (!IfCheckOk)
            {
                //付密码错误
                baseUsers.PayErr++;
                if (baseUsers.PayErr >= SysSet.PayLock)
                {
                    baseUsers.PayLock = 1;
                }
                Entity.SaveChanges();
                Users Out = new Users();
                Out.PayErr = SysSet.PayLock - baseUsers.PayErr;
                Out.Cols = "PayErr";
                DataObj.Data = Out.OutJson();
                //DataObj.Code = "2010";
                DataObj.Code = "2002";
                if (Out.PayErr == 0)
                {
                    DataObj.Msg = "用户支付密码不正确，请明日再试或取回支付密码";
                }
                else
                {
                    DataObj.Msg = "用户支付密码不正确，您还可以尝试" + Out.PayErr + "次";
                }
                DataObj.OutString();
                return;
            }
            
            baseUsers.PayErr = 0;
            Entity.SaveChanges();

            //开始处理参数
            if (OrderCash.Amoney.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            OrderCash.Amoney = OrderCash.Amoney.FormatMoney();
            //提现金额大于余额
            if (OrderCash.Amoney > baseUsers.Amount)
            {
                DataObj.OutError("6001");
                return;
            }
            //冻结金额小于0不能提现
            if (baseUsers.Frozen < 0)
            {
                DataObj.OutError("6001");
                return;
            }
            if (OrderCash.X.IsNullOrEmpty() || OrderCash.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (OrderCash.Owner.IsNullOrEmpty() || OrderCash.Bank.IsNullOrEmpty() || OrderCash.CardNum.IsNullOrEmpty() || OrderCash.Deposit.IsNullOrEmpty() || OrderCash.Mobile.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            UserBlackList UserBlackList = Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == OrderCash.CardNum && UBL.State == 3);
            if (UserBlackList != null)
            {
                //提示暂不支持该卡提现
                DataObj.OutError("2017");
                return;
            }
            //获取分支机构信息
            SysAgent SysAgent = new SysAgent();
            if (!baseUsers.Agent.IsNullOrEmpty()) {
                SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
            }
            if (SysAgent.Id.IsNullOrEmpty()) {
                SysAgent.Cash0Times = 0;
                SysAgent.Cash1Times = 0;
            }

            decimal Peier = 0;
            if (OrderCash.TrunType == 0) {

                if (baseUsers.HasT0 != 1)
                {
                    //T0黑名单
                    DataObj.OutError("6020");
                    return;
                }

                //验证是否满足T0
                int Day = (DateTime.Now - baseUsers.AddTime).Days;
                int OrderCount = Entity.Orders.Count(n => (n.TType == 2 || n.TType == 5) && n.PayState == 2 && n.UId == baseUsers.Id);
                decimal? OrderMoney = Entity.Orders.Where(n => (n.TType == 2 || n.TType == 5) && n.PayState == 2 && n.UId == baseUsers.Id).Sum(n =>(decimal?) n.Amoney);
                if (OrderMoney.IsNullOrEmpty())
                {
                    OrderMoney = 0;
                }
                if (Day >= SysSet.CashDay && OrderCount >= SysSet.CashNum && OrderMoney >= SysSet.CashMoney)
                {

                }
                else {
                    DataObj.OutError("6020");
                    return;
                }
                //验证是否在快速提现时段内
                TaskTimeSet TaskTimeSet = Entity.TaskTimeSet.FirstOrDefault(n => n.STime <= DateTime.Now && n.ETime >= DateTime.Now && n.TId == 1);
                if (TaskTimeSet == null)
                {
                    DataObj.OutError("6018");
                    return;
                }
                Peier = TaskTimeSet.AllMoney - TaskTimeSet.UsedMoney;
            }

            OrderCash.UId = baseUsers.Id;
            if (OrderCash.TrunType == 0)//加急提现
            {
                if (baseUsers.T0Times < SysAgent.Cash0Times)
                {
                    //用户提现服务费
                    OrderCash.UserRate = 0;
                    //加急费用
                    OrderCash.Cash = 0;
                    //提现服务费
                    OrderCash.ECash = 0;
                }
                else {
                    //用户提现服务费=提现金额*加急费率+提现服务费
                    OrderCash.UserRate = (double)(OrderCash.Amoney * baseUsers.Cash0 + baseUsers.ECash0);
                    //提现服务费进位
                    OrderCash.UserRate = OrderCash.UserRate.Ceiling();
                    //加急费用
                    OrderCash.Cash = baseUsers.Cash0;
                    //提现服务费
                    OrderCash.ECash = baseUsers.ECash0;
                }
                //系统提现费成本
                OrderCash.CashRate = OrderCash.Amoney * SysSet.SysCash0 + SysSet.SysECash0;
                //系统手续费进位
                OrderCash.CashRate = OrderCash.CashRate.Ceiling();
                
                //这里是利润计算==========
                //代理商分润
                double GetAgent = OrderCash.UserRate - (double)(OrderCash.Amoney * SysSet.AgentCash0 + SysSet.AgentECash0);
                if (GetAgent < 0) {
                    GetAgent = 0;
                }
                //利润=收取用户手续费-系统支出手续费
                double GetAll = OrderCash.UserRate - (double)OrderCash.CashRate - GetAgent;
                //利润舍位
                GetAll = GetAll.Floor();
                //总利润
                OrderCash.AIdCashGet = (double)GetAll;
                //分支机构佣金设置为0，待分润计算后再写入
                OrderCash.AgentCashGet = GetAgent;
            }
            else {
                if (baseUsers.T1Times < SysAgent.Cash1Times)
                {
                    //用户提现服务费
                    OrderCash.UserRate = 0;
                    //加急费用
                    OrderCash.Cash = 0;
                    //提现服务费
                    OrderCash.ECash = 0;
                }
                else {
                    //用户提现服务费=提现金额*加急费率+提现服务费
                    OrderCash.UserRate = (double)(OrderCash.Amoney * baseUsers.Cash1 + baseUsers.ECash1);
                    //提现服务费进位
                    OrderCash.UserRate = OrderCash.UserRate.Ceiling();
                    //加急费用
                    OrderCash.Cash = baseUsers.Cash1;
                    //提现服务费
                    OrderCash.ECash = baseUsers.ECash1;
                }
                
                //系统提现费成本
                OrderCash.CashRate = OrderCash.Amoney * SysSet.SysCash1 + SysSet.SysECash1;
                //系统手续费进位
                OrderCash.CashRate = OrderCash.CashRate.Ceiling();
                
                //这里是利润计算==========
                //代理商分润
                double GetAgent = OrderCash.UserRate - (double)(OrderCash.Amoney * SysSet.AgentCash1 + SysSet.AgentECash1);
                if (GetAgent < 0)
                {
                    GetAgent = 0;
                }
                //利润=收取用户手续费-系统支出手续费
                double GetAll = OrderCash.UserRate - (double)OrderCash.CashRate - GetAgent;
                //利润舍位
                GetAll = GetAll.Floor();
                //总利润
                OrderCash.AIdCashGet = (double)GetAll;
                //分支机构佣金
                OrderCash.AgentCashGet = GetAgent;
            }
            decimal Money = OrderCash.Amoney - (decimal)OrderCash.UserRate;
            if (Money < 0)
            {
                DataObj.OutError("1006");
                return;
            }
            if (OrderCash.TrunType == 0)//T0时才验证
            {
                //付款金额
                
                //验证是否配额充足
                if (Peier < Money)
                {
                    DataObj.OutError("6019");
                    return;
                }
            }

            OrderCash.Agent = SysAgent.Id;//分支机构Id
            OrderCash.AId = baseUsers.AId;
            OrderCash.FId = 0;
            OrderCash.OrderState = 1;
            OrderCash.PayState = 1;
            OrderCash.AgentState = 0;
            OrderCash.FState = 0;
            OrderCash.AddTime = DateTime.Now;

            //写入前，判断交易金额限制
            if (OrderCash.Amoney < syscontrol.SNum || OrderCash.Amoney > syscontrol.ENum)
            {
                DataObj.OutError("1006");
                return;
            }
            //写入订单总表
            Orders Orders = new Orders();
            Orders.UId = OrderCash.UId;
            Orders.TName = baseUsers.TrueName;

            Orders.PayType = 0;
            Orders.PayName = "提现";

            Orders.RUId = 0;
            Orders.RName = string.Empty;
            Orders.TType = 2;
            Orders.TState = 1;
            Orders.Amoney = OrderCash.Amoney;
            Orders.Poundage = (decimal)OrderCash.UserRate;
            Orders.AddTime = DateTime.Now;
            Orders.PayState = 1;//提现为余额支付的一种特殊形式。
            Orders.PayWay = 4;
            Orders.PayTime = DateTime.Now;

            Orders.Agent = OrderCash.Agent;
            Orders.AgentState = 0;
            Orders.AId = OrderCash.AId;
            Orders.FId = 0;

            Orders.UserCardId = baseUsers.CardId;

            string OrderAddress = OrderCash.OrderAddress;
            if (OrderAddress.IsNullOrEmpty()) {
                OrderAddress = Utils.GetAddressByGPS(OrderCash.X, OrderCash.Y);
            }
            Orders.OrderAddress = OrderAddress;
            Orders.X = OrderCash.X;
            Orders.Y = OrderCash.Y;

            Orders.TrunType = OrderCash.TrunType;

            Entity.Orders.AddObject(Orders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, Orders);

            OrderCash.OId = Orders.TNum;

            Entity.OrderCash.AddObject(OrderCash);
            Entity.SaveChanges();

            //帐户变动记录
            int USERSID = baseUsers.Id;
            string TNUM = OrderCash.OId;
            decimal PAYMONEY = Orders.Amoney;
            string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 3, "");
            if (SP_Ret != "3")
            {
                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 3, PAYMONEY, SP_Ret), "SP_UsersMoney");
                Orders.TState = 0;
                Orders.PayState = 0;
                OrderCash.OrderState = 0;
                OrderCash.PayState = 0;
                Entity.SaveChanges();
                DataObj.OutError("8888");
                return;
            }

            if (OrderCash.TrunType == 0)//T0时减少配额
            {
                DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                TaskTimeSet TaskTimeSet = Entity.TaskTimeSet.FirstOrDefault(n => n.ODate == Today);
                if (TaskTimeSet != null)
                {
                    TaskTimeSet.UsedMoney += Money;
                    Entity.SaveChanges();
                }
                baseUsers.T0Times = baseUsers.T0Times + 1;
            }
            else {
                baseUsers.T1Times = baseUsers.T1Times + 1;
            }
            if (SysSet.CashChecked == 1)
            {
                bool IsAuto = true;
                if (SysSet.EveryDayMaxCash > 0)//超出规定额度，变成人工审核。规定是0刚不执行
                {
                    DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                    var todaymoney = this.Entity.OrderCash.Where(o => (o.OrderState == 2 || o.OrderState == 1) && o.AddTime > Today && (o.PayState == 1 || o.PayState == 2) && o.UId == baseUsers.Id).Sum(o => (decimal?)o.Amoney) ?? 0m;
                    if (todaymoney > SysSet.EveryDayMaxCash)//当天累加提现 > 规定额度   
                    {
                        IsAuto = false;
                    }
                }
                if (IsAuto)
                {
                    //帐户变动记录
                    string SP_Ret2 = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 4, "");
                    if (SP_Ret2 != "3")
                    {
                        Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 4, PAYMONEY, SP_Ret2), "SP_UsersMoney");
                        DataObj.OutError("8888");
                        return;
                    }
                    else
                    {
                        //自动审核
                        Orders.TState = 2;
                        OrderCash.OrderState = 2;
                        OrderCash.AuditTime = DateTime.Now;
                        Entity.SaveChanges();
                    }
                    if (SysSet.CashPayWay == 1)//开启自动结算时执行
                    {
                        if (OrderCash.TrunType == 0)//T0时自动出款
                        {
                            if (OrderCash.Amoney <= SysSet.QCash0)
                            {
                                Utils.WriteLog("[S]" + Orders.TNum, "CashAuto");
                                OrderCash.PayCash(Orders, Entity);//去付款
                                Utils.WriteLog("[E]" + Orders.TNum, "CashAuto");
                            }
                            else
                            {
                                Utils.WriteLog("结算金额超出自动出款金额" + OrderCash.OId + ":提现金额[" + OrderCash.Amoney + "]自动出款金额[" + SysSet.QCash0 + "]", "OrderCash");
                            }
                        }
                        //else
                        //{
                        //    Utils.WriteLog("T0时自动出款没开启" + OrderCash.OId, "OrderCash");
                        //}
                    }
                    else
                    {
                        Utils.WriteLog("自动结算没开启" + OrderCash.OId, "OrderCash");
                    }
                }
                else {
                    Utils.WriteLog("单卡超过限定金额" + OrderCash.OId, "OrderCash");
                }
            }
            else {
                Utils.WriteLog("自动审核没开启" + OrderCash.OId, "OrderCash");
            }
            Entity.Refresh(RefreshMode.StoreWins, Orders);
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
            
            if (Orders.PayState == 3 && Orders.Remark == "本银行卡今日提交超过限额！") {
                DataObj.OutError("1016");
                return;
            }
            //超过单日单卡（10W）到账限额
            DataObj.Data = Orders.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
