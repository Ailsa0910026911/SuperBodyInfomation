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
using LokFu.Repositories.SqlServer;
namespace LokFu.Controllers
{
    public class PayController : InitController
    {
        public PayController()
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
                Log.Write("[Pay]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Orders order = new Orders();
            order = JsonToObject.ConvertJsonToModel(order, json);
            if (order.TNum.IsNullOrEmpty() || order.Token.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (order.PayPWD.IsNullOrEmpty())//使用PassWord字段
            {
                DataObj.OutError("1000");
                return;
            }
            if (order.PayPWD.Length < 6)//6位及以上
            {
                DataObj.OutError("1000");
                return;
            }
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == order.Token);
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

            //这里是执行指纹解锁
            bool IfCheckOk = true;
            if (order.PayPWD.Substring(0, 3) == "HF_")
            {
                string PassWord = order.PayPWD;
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
            else if (baseUsers.PayPwd != order.PayPWD.GetPayMD5())
            {
                //原支付密码错误
                IfCheckOk = false;
            }
            if (!IfCheckOk)
            {
                //付密码错误
                SysSet SysSet = Entity.SysSet.FirstOrNew();
                //系统统一修改标识SAME002
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

                //DataObj.Code = "2010";
                DataObj.OutString();
                return;
            }

            baseUsers.PayErr = 0;
            Entity.SaveChanges();

            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == order.TNum);
            if (Orders == null)
            {
                DataObj.OutError("1001");
                return;
            }
            if (baseUsers.Id != Orders.UId)//禁止代付
            {
                DataObj.OutError("6021");
                return;
            }
            if (Orders.TType != 3 && Orders.TType != 5 && Orders.TType != 6 && Orders.TType != 10)
            {
                DataObj.OutError("6022");
                return;
            }
            if (Orders.PayWay != 4 && Orders.PayWay != 0)
            {
                DataObj.OutError("1008");
                return;
            }
            if (Orders.TState != 1)
            {
                DataObj.OutError("6024");
                return;
            }
            if (Orders.PayState != 0)
            {//已支付
                DataObj.OutError("6025");
                return;
            }
            if (Orders.Amoney > baseUsers.Amount) {
                DataObj.OutError("6026");
                return;
            }
            //开始支付流程
            //==================================================================
            //帐户变动记录
            int USERSID = baseUsers.Id;
            string TNUM = Orders.TNum;
            decimal PAYMONEY = Orders.Amoney;
            string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 2, "");
            if (SP_Ret != "3")
            {
                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 2, PAYMONEY, SP_Ret), "SP_UsersMoney");
                Orders.TState = 0;
                Orders.PayState = 0;
                Entity.SaveChanges();
                DataObj.OutError("8888");
                return;
            }
            //==================================================================
            //处理支付订单
            Orders.PayWay = 4;
            Entity.SaveChanges();
            Orders = Orders.PaySuccess(Entity);
            baseUsers.Cols = "Id,UserName,TrueName,Amount,Frozen";
            DataObj.Data = baseUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
