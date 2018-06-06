using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.Extensions;
using LokFu.Repositories.SqlServer;
namespace LokFu.Controllers
{
    public class BaoUsersOutController : BaoController
    {
        public BaoUsersOutController()
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
                Log.Write("[BaoUsersOut]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            BaoUsers BaoUsers = new BaoUsers();
            BaoUsers = JsonToObject.ConvertJsonToModel(BaoUsers, json);
            if (BaoUsers.Token.IsNullOrEmpty() || BaoUsers.ActMoney.IsNullOrEmpty() || BaoUsers.PayPwd.IsNullOrEmpty())
            { 
                DataObj.OutError("1000");
                return;
            }
            if (BaoUsers.PayPwd.Length < 6)//6位及以上
            {
                DataObj.OutError("1000");
                return;
            }
            Users Users = Entity.Users.FirstOrDefault(n => n.Token == BaoUsers.Token);
            if (Users == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (Users.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (Users.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (Users.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }
            decimal ActMoney = BaoUsers.ActMoney;
            string PayPwd = BaoUsers.PayPwd;

            BaoUsers = Entity.BaoUsers.FirstOrDefault(n => n.UId == Users.Id);
            if (BaoUsers == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //提现金额大于余额
            if (ActMoney > BaoUsers.AllMoney)
            {
                DataObj.OutError("6001");
                return;
            }
            if (Users.PayLock == 1)//密码错误太多次锁定
            {
                DataObj.OutError("2050");
                return;
            }

            //这里是执行指纹解锁
            bool IfCheckOk = true;
            if (PayPwd.Substring(0, 3) == "HF_")
            {
                string PassWord = PayPwd;
                PassWord = PassWord.Replace("HF_", "");
                string Token = Users.Token;
                Token = Token + "GoodPay";
                string Md5Token = Token.GetMD5().ToUpper();
                string Pass = Md5Token.Substring(0, 4) + Md5Token.Substring(Md5Token.Length - 4, 4);
                if (Pass != PassWord)
                {
                    IfCheckOk = false;
                }
            }
            else if (Users.PayPwd != PayPwd.GetPayMD5())
            {
                //原支付密码错误
                IfCheckOk = false;
            }
            if (!IfCheckOk)
            {
                //付密码错误
                SysSet SysSet = Entity.SysSet.FirstOrNew();
                //系统统一修改标识SAME002
                Users.PayErr++;
                if (Users.PayErr >= SysSet.PayLock)
                {
                    Users.PayLock = 1;
                }
                Entity.SaveChanges();
                Users Out = new Users();
                Out.PayErr = SysSet.PayLock - Users.PayErr;
                Out.Cols = "PayErr";
                DataObj.Data = Out.OutJson();
                DataObj.Code = "2010";
                DataObj.OutString();
                return;
            }

            Users.PayErr = 0;
            Entity.SaveChanges();

            //业务开始
            //===========================================================================
            //余额宝变动记录
            BaoLog BaoLog = new BaoLog();
            BaoLog.UId = Users.Id;
            BaoLog.LType = 2;//1转入 2转出 3收益
            BaoLog.BeforAmount = BaoUsers.ActMoney;
            BaoLog.BeforFrozen = BaoUsers.InMoney;
            BaoLog.Amount = ActMoney;

            if (BaoUsers.InMoney == 0)
            {
                //未计息帐户没钱，全部扣除计息帐户
                BaoUsers.ActMoney = BaoUsers.ActMoney - ActMoney;
                BaoUsers.AllMoney = BaoUsers.AllMoney - ActMoney;
            }
            else
            {
                //未计息帐户有钱
                if (ActMoney < BaoUsers.InMoney)
                {
                    //转出小于未计息帐户金额，直接扣除未计息帐户中金额
                    BaoUsers.InMoney = BaoUsers.InMoney - ActMoney;
                    BaoUsers.AllMoney = BaoUsers.AllMoney - ActMoney;
                }else{
                    //转出大于未计息帐户金额，直接扣除未计息帐户中全部金额，不足从计息帐户中扣除
                    BaoUsers.ActMoney = BaoUsers.ActMoney + BaoUsers.InMoney - ActMoney;
                    BaoUsers.InMoney = 0;
                    BaoUsers.AllMoney = BaoUsers.AllMoney - ActMoney;
                }
            }

            BaoLog.AfterAmount = BaoUsers.ActMoney;
            BaoLog.AfterFrozen = BaoUsers.InMoney;
            BaoLog.State = 1;
            BaoLog.AddTime = DateTime.Now;
            Entity.BaoLog.AddObject(BaoLog);

            //帐户变动记录
            int USERSID = Users.Id;
            string SP_Ret = Entity.SP_UsersMoney(USERSID, "理财转出", ActMoney, 1, "");
            if (SP_Ret != "3")
            {
                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, "理财转出", 1, ActMoney, SP_Ret), "SP_UsersMoney");
                DataObj.OutError("8888");
                return;
            }
            Entity.SaveChanges();

            BaoConfig BaoConfig = Entity.BaoConfig.FirstOrNew();
            BaoUsers.GetCost = BaoConfig.GetCost;
            DataObj.Data = BaoUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
