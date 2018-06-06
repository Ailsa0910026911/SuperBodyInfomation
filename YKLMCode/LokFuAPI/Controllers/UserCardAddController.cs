using System.Linq;
using System;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.Extensions;

namespace LokFu.Controllers
{
    public class UserCardAddController : InitController
    {
        public UserCardAddController()
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
                Log.Write("[UserCardAdd]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserCard UserCard = new UserCard(); 
            UserCard = JsonToObject.ConvertJsonToModel(UserCard, json);

            #region 校验
            if (UserCard.PayPwd.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (UserCard.PayPwd.Length < 6)//6位及以上
            {
                DataObj.OutError("1000");
                return;
            }
            var Bank = this.Entity.BasicBank.Where(o=>o.Name == UserCard.Bank).FirstOrDefault();
            if (Bank == null)
            {
                DataObj.OutError("1103");
                return;
            }

            var BasicBankInfo = this.Entity.BasicBankInfo.Where(o => o.BId == Bank.Id && o.BIN == UserCard.Bin).FirstOrDefault();
            if (BasicBankInfo == null)
            {
                DataObj.OutError("1102");
                return;
            }

            //获取用户信息
            string Token = UserCard.Token;
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Token);
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
            UserBlackList UserBlackList = Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == UserCard.Card && UBL.State == 3);
            if (UserBlackList != null)
            {
                //提示暂不支持该卡绑定
                DataObj.OutError("2016");
                return;
            }
            UserCard UserCard_ = Entity.UserCard.FirstOrDefault(n => n.UId == baseUsers.Id && n.Card == UserCard.Card && n.State == 1);//已绑定本张银行卡
            if (UserCard_ != null)
            {
                DataObj.OutError("2015");
                return;
            }
            if (baseUsers.PayLock == 1)//密码错误太多次锁定
            {
                DataObj.OutError("2050");
                return;
            }
            #region 验证是否是借记卡 by anjing 2018-01-03
            if (GetCardType(UserCard.Card)!=1)
            {
                DataObj.OutError("2016");
                return;
            }
            #endregion

            #region 密码验证
            //这里是执行指纹解锁
            bool IfCheckOk = true;
            if (UserCard.PayPwd.Substring(0, 3) == "HF_")
            {
                string PassWord = UserCard.PayPwd;
                PassWord = PassWord.Replace("HF_", "");
                string token = baseUsers.Token;
                token = token + "GoodPay";
                string Md5Token = token.GetMD5().ToUpper();
                string Pass = Md5Token.Substring(0, 4) + Md5Token.Substring(Md5Token.Length - 4, 4);
                if (Pass != PassWord)
                {
                    IfCheckOk = false;
                }
            }
            else if (baseUsers.PayPwd != UserCard.PayPwd.GetPayMD5())
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
                return;
            }
            baseUsers.PayErr = 0;
            #endregion

            BasicBank BasicBank = Entity.BasicBank.FirstOrDefault(n => n.Name == UserCard.Bank);
            if (BasicBank == null) {
                DataObj.OutError("1000");
                return;
            }
            #endregion

            Users cacheUsers = CacheBuilder.EntityCache.Get(baseUsers.Id.ToString() + "CardPicTemp") as Users;
            if (cacheUsers != null)
            {
                UserCard.Pic = cacheUsers.CardPic;
                UserCard.ScanNo = cacheUsers.CardNum ?? string.Empty;
            }
            else
            {
                UserCard.Pic = string.Empty;
                UserCard.ScanNo = string.Empty;
            }

            UserCard.UId = baseUsers.Id;
            UserCard.BId = BasicBank.Id;
            UserCard.Mobile = UserCard.Mobile.IsNullOrEmpty() ? string.Empty : UserCard.Mobile;
            UserCard.State = 1;
            UserCard.AddTime = DateTime.Now;
            Entity.UserCard.AddObject(UserCard);
            Entity.SaveChanges();

            //if (HasCache)
            //{
            //    string CashName = "UserCard_" + UserCard.Type + "_" + Token;
            //    CacheBuilder.EntityCache.Remove(CashName, null);
            //}
            DataObj.Data = UserCard.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
