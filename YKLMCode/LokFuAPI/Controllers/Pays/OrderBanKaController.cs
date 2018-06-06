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
namespace LokFu.Controllers
{
    public class OrderBanKaController : InitController
    {
        public OrderBanKaController()
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
                Log.Write("[OrderBanKa]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            BanKaOrder BanKaOrder = new BanKaOrder();
            BanKaOrder = JsonToObject.ConvertJsonToModel(BanKaOrder, json);
            if (BanKaOrder.BKTId.IsNullOrEmpty() || BanKaOrder.PayPWD.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (BanKaOrder.PayPWD.Length < 6)//6位及以上
            {
                DataObj.OutError("1000");
                return;
            }
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == BanKaOrder.Token);
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

            //这里是执行指纹解锁
            bool IfCheckOk = true;
            if (BanKaOrder.PayPWD.Substring(0, 3) == "HF_")
            {
                string PassWord = BanKaOrder.PayPWD;
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
            else if (baseUsers.PayPwd != BanKaOrder.PayPWD.GetPayMD5())
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
                DataObj.Code = "2010";
                DataObj.OutString();
                return;
            }
            
            baseUsers.PayErr = 0;


            //检测是否已经购买过
            BanKaOrder BKO = Entity.BanKaOrder.FirstOrDefault(n => n.UId == baseUsers.Id && n.BKTId == BanKaOrder.BKTId && n.OrderState == 2 && n.PayState == 1);
            if (BKO != null) {
                DataObj.OutError("6051");
                return;
            }

            //获取产品
            BanKaType BanKaType = Entity.BanKaType.FirstOrDefault(n => n.Id == BanKaOrder.BKTId && n.State == 1);
            if (BanKaType == null) {
                DataObj.OutError("1000");
                return;
            }
            if (BanKaType.Amoney > baseUsers.Amount)
            {//余额不足
                DataObj.OutError("6026");
                return;
            }

            BanKaOrder.UId = baseUsers.Id;
            BanKaOrder.Amoney = BanKaType.Amoney;

            BanKaOrder.OrderState = 1;
            BanKaOrder.PayState = 0;
            BanKaOrder.AddTime = DateTime.Now;

            BanKaOrder.Agent = baseUsers.Agent;
            BanKaOrder.AId = baseUsers.AId;
            BanKaOrder.AgentState = 0;
            //这里是利润计算==========
            BanKaOrder.AgentGet = 0;
            BanKaOrder.AIdGet = 0;

            Entity.BanKaOrder.AddObject(BanKaOrder);

            Entity.SaveChanges();

            Entity.Refresh(RefreshMode.StoreWins, BanKaOrder);

            //开始支付流程
            //==================================================================
            //帐户变动记录
            int USERSID = baseUsers.Id;
            string TNUM = BanKaOrder.OId;
            decimal PAYMONEY = BanKaOrder.Amoney;
            string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 2, BanKaType.Title);
            if (SP_Ret != "3")
            {
                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 2, PAYMONEY, SP_Ret), "SP_UsersMoney");
                BanKaOrder.OrderState = 0;
                Entity.SaveChanges();
                DataObj.OutError("8888");
                return;
            }
            //==================================================================
            //交易标识
            BanKaOrder.OrderState = 2;
            BanKaOrder.PayState = 1;
            Entity.SaveChanges();

            DataObj.Data = BanKaOrder.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
