using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using LokFu.Extensions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace LokFu.Controllers
{
    public class UsersLoginController : InitController
    {
        public UsersLoginController()
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
                Log.Write("[UsersLogin]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.UserName.IsNullOrEmpty() || Users.PassWord.IsNullOrEmpty()) { 
                //
                DataObj.OutError("1000");
                return;
            }else{
                Users.PassWord=Users.PassWord.GetMD5();
            }
            byte LoginType = Users.LoginType;
            if (LoginType.IsNullOrEmpty()) {
                LoginType = 0;
            }

            Users baseUsers = Entity.Users.Where(n => n.UserName == Users.UserName).FirstOrDefault();
            if (baseUsers == null)//用户不存在
            {
                DataObj.OutError("2001");
                return;
            }
            if (baseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.LoginLock == 1)//临时锁定
            {
                DataObj.OutError("2103");
                return;
            }
            if (baseUsers.PassWord != Users.PassWord)
            {
                SysSet SysSet = Entity.SysSet.FirstOrNew();
                //系统统一修改标识SAME001
                baseUsers.LoginErr++;
                if (baseUsers.LoginErr >= SysSet.LoginLock)
                {
                    baseUsers.LoginLock = 1;
                }
                Entity.SaveChanges();

                Users Out = new Users();
                Out.LoginErr = SysSet.LoginLock - baseUsers.LoginErr;
                Out.Cols = "LoginErr";
                DataObj.Data = Out.OutJson();

                DataObj.Code = "2002";
                if (Out.LoginErr == 0)
                {
                    DataObj.Msg = "帐号或密码不正确，请明日再试或取回登录密码";
                }
                else
                {
                    DataObj.Msg = "帐号或密码不正确，您还可以尝试" + Out.LoginErr + "次";
                }
                DataObj.OutString();

                return;
            }
            if (LoginType == 0)
            {
                //清除登录设置与用户关系
                Entity.ExecuteStoreCommand("Update Users Set Eno='' Where Eno='" + DataObj.ENo + "'");
            }
            DateTime now = DateTime.Now;
            Guid Gid=Guid.NewGuid();
            string mdstr = baseUsers.Id + "|" + baseUsers.UserName + "|" + Gid.ToString() + "|" + now.ToString();
            string taken = mdstr.GetMD5();
            if (LoginType == 0)
            {
                baseUsers.Token = taken;
            }
            else {
                baseUsers.PrintToken = string.Format("Print|{0}", taken);
            }
            baseUsers.LoginErr = 0;

            baseUsers.ENo = DataObj.ENo;//设置标识

            Entity.SaveChanges();

            int BankNum = Entity.UserCard.Count(n => n.UId == baseUsers.Id && n.State == 1);
            baseUsers.BankNum = BankNum;

            if (baseUsers.Pic.IsNullOrEmpty())
            {
                baseUsers.Pic = "none.png";
            }
            baseUsers.Pic = Utils.ImageUrl("UsersPic", baseUsers.Pic, AppImgPath);
            baseUsers.CardPic = Utils.ImageUrl("Users", baseUsers.CardPic, AppImgPath);
            baseUsers.CardFace = Utils.ImageUrl("Users", baseUsers.CardFace, AppImgPath);
            baseUsers.CardBack = Utils.ImageUrl("Users", baseUsers.CardBack, AppImgPath);

            string CashName = "UsersAgentTel" + baseUsers.Agent.ToString();
            if (HasCache)
            {
                baseUsers.AgentTel = CacheBuilder.EntityCache.Get(CashName, null) as string;
            }
            if (baseUsers.AgentTel.IsNullOrEmpty())
            {
                if (!baseUsers.Agent.IsNullOrEmpty())
                {
                    SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
                    baseUsers.AgentTel = SysAgent.Tel;
                }
                if (baseUsers.AgentTel.IsNullOrEmpty())
                {
                    SysSet SysSet = Entity.SysSet.FirstOrNew();
                    baseUsers.AgentTel = SysSet.Tel;
                }
                if (HasCache && !baseUsers.AgentTel.IsNullOrEmpty())
                {
                    CacheBuilder.EntityCache.Remove(CashName, null);
                    CacheBuilder.EntityCache.Add(CashName, baseUsers.AgentTel, DateTime.Now.AddMinutes(60), null);
                }
            }

            CashName = "UsersMsgCount" + baseUsers.Id.ToString();
            string count = string.Empty;
            if (HasCache)
            {
                count = CacheBuilder.EntityCache.Get(CashName, null) as string;
            }
            if (count == string.Empty || count == null)
            {
                string uid = string.Format(",{0},", baseUsers.Id);
                int Count = Entity.MsgUser.Count(n => (n.UId == baseUsers.Id && n.State == 1) || (n.UId == 0 && !n.ReadUsers.Contains(uid) && !n.DeleteUsers.Contains(uid) && n.AddTime > baseUsers.AddTime && (n.SendUsers.Contains(uid) || n.SendUsers == null || n.SendUsers == "")));
                baseUsers.MsgCount = Count;
                count = Count.ToString();
                if (HasCache)
                {
                    CacheBuilder.EntityCache.Remove(CashName, null);
                    CacheBuilder.EntityCache.Add(CashName, count, DateTime.Now.AddMinutes(6), null);
                }
            }
            else
            {
                baseUsers.MsgCount = Int32.Parse(count);
            }
            //AutoCashBank,AutoBank
            CashName = "UsersAutoBank" + baseUsers.Id.ToString();
            string AutoBank = string.Empty;
            if (!Users.AutoCashBank.IsNullOrEmpty() && Users.AutoCash == 1)
            {
                if (HasCache)
                {
                    AutoBank = CacheBuilder.EntityCache.Get(CashName, null) as string;
                }
                if (AutoBank.IsNullOrEmpty())
                {
                    UserCard UserCard = Entity.UserCard.FirstOrDefault(n => n.Id == Users.AutoCashBank && n.UId == Users.Id && n.State == 1);
                    if (UserCard != null)
                    {
                        AutoBank = UserCard.Card;
                        if (HasCache)
                        {
                            CacheBuilder.EntityCache.Remove(CashName, null);
                            CacheBuilder.EntityCache.Add(CashName, AutoBank, DateTime.Now.AddMinutes(1), null);
                        }
                    }
                }
            }
            baseUsers.AutoBank = AutoBank;

            if (baseUsers.Amount < 0)
            {
                baseUsers.Amount = 0;
            }

            var BaoConfig = Entity.BaoConfig.FirstOrNew();
            baseUsers.GetCost = BaoConfig.GetCost;
            baseUsers.YearPer = BaoConfig.YearPer;

            if (baseUsers.CardTrueName.IsNullOrEmpty() && baseUsers.CardType == 1)
            {
                baseUsers.IsAnewUpImg = 1;
            }
            SysMoneySet moneyset = Entity.SysMoneySet.FirstOrNew();
            baseUsers.VipPrice = moneyset.VipPrice;
            baseUsers.Cols = "Id,UserName,NeekName,TrueName,Mobile,QQ,Email,Address,Amount,Frozen,CardId,Pic,CardStae,CardFace,CardBack,CardPic,MiBao,Token,CardNum,CardRemark,BankNum,MsgCount,AgentTel,YYOpenState,InTypeMobile,InTypePC,T0Times,T1Times,AutoBao,AutoCash,AutoCashMoney,AutoCashBank,AutoBank,GetCost,YearPer,YAmount,AllRec,LastRec,IsAnewUpImg,IsVip,VipPrice";

            DataObj.Data = baseUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
