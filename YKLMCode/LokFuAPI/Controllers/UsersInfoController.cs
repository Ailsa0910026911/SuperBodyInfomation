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

namespace LokFu.Controllers
{
    public class UsersInfoController : InitController
    {
        public UsersInfoController()
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
                Log.Write("[UsersInfo]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.Token.IsNullOrEmpty())
            {
                //
                DataObj.OutError("1000");
                return;
            }

            Users = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
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
            //===========================================================
            if (!Equipment.SoftVer.IsNullOrEmpty())
            {
                Version v1 = new Version(Equipment.SoftVer);//当前版本
                Version v2 = new Version("1.0");
                SysAgent SysAgent = new SysAgent();
                bool IsTeiPai = false;
                //--------------------------------------------------------
                //判断是否是贴牌
                if (!Users.Id.IsNullOrEmpty())
                {
                    SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == Users.Agent && n.State == 1);
                    if (SysAgent != null)
                    {
                        SysAgent = SysAgent.GetTopAgent(Entity);
                        if (SysAgent != null)
                        {
                            if (SysAgent.IsTeiPai == 1)
                            {
                                IsTeiPai = true;
                            }
                        }
                    }
                }
                //--------------------------------------------------------
                if (Equipment.RqType.ToLower() == "apple")
                {
                    //苹果
                    if (!IsTeiPai)//好付
                    {
                        v2 = new Version("8.0.1");
                    }
                    else//贴牌
                    {
                        v2 = new Version("6.0.0");
                    }

                }
                else if (Equipment.RqType.ToLower() == "android")
                {
                    //安卓
                    if (!IsTeiPai)//好付
                    {
                        v2 = new Version("8.0.2");
                    }
                    else //贴牌
                    {
                        v2 = new Version("6.0.0");
                    }
                }
                if (v1 >= v2)
                {
                    if (Users.IfCanIn == 0)
                    {
                        //已经是最新版
                        Users.IfCanIn = 1;
                        Users.AutoBao = 0;
                        Entity.SaveChanges();
                    }
                }
            }
            //===========================================================
            Users.AgentTel = "";

            string CashName = "UsersAgentTel" + Users.Agent.ToString();
            if (HasCache)
            {
                Users.AgentTel = CacheBuilder.EntityCache.Get(CashName, null) as string;
            }
            if(Users.AgentTel.IsNullOrEmpty())
            {
                if (!Users.Agent.IsNullOrEmpty())
                {
                    SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent);
                    Users.AgentTel = SysAgent.Tel;
                }
                if (Users.AgentTel.IsNullOrEmpty())
                {
                    SysSet SysSet = Entity.SysSet.FirstOrNew();
                    Users.AgentTel = SysSet.Tel;
                }
                if (HasCache && !Users.AgentTel.IsNullOrEmpty())
                {
                    CacheBuilder.EntityCache.Remove(CashName, null);
                    CacheBuilder.EntityCache.Add(CashName, Users.AgentTel, DateTime.Now.AddMinutes(60), null);
                }
            }

            if (Users.Pic.IsNullOrEmpty())
            {
                Users.Pic = "none.png";
            }
            Users.Pic = Utils.ImageUrl("UsersPic", Users.Pic, AppImgPath);
            Users.CardPic = Utils.ImageUrl("Users", Users.CardPic, AppImgPath);
            Users.CardFace = Utils.ImageUrl("Users", Users.CardFace, AppImgPath);
            Users.CardBack = Utils.ImageUrl("Users", Users.CardBack, AppImgPath);

            int BankNum = Entity.UserCard.Count(n => n.UId == Users.Id && n.State == 1 && n.Type==1);
            Users.BankNum = BankNum;


            CashName = "UsersMsgCount" + Users.Id.ToString();
            string count = string.Empty;
            if (HasCache)
            {
                count = CacheBuilder.EntityCache.Get(CashName, null) as string;
            }
            if (count == string.Empty || count == null)
            {
                string uid = string.Format(",{0},", Users.Id);
                int Count = Entity.MsgUser.Count(n => (n.UId == Users.Id && n.State == 1) || (n.UId == 0 && !n.ReadUsers.Contains(uid) && !n.DeleteUsers.Contains(uid) && n.AddTime > Users.AddTime && (n.SendUsers.Contains(uid) || n.SendUsers == null || n.SendUsers == "") && n.State > 0));
                Users.MsgCount = Count;
                count = Count.ToString();
                if (HasCache)
                {
                    CacheBuilder.EntityCache.Remove(CashName, null);
                    CacheBuilder.EntityCache.Add(CashName, count, DateTime.Now.AddMinutes(6), null);
                }
            }
            else {
                Users.MsgCount = Int32.Parse(count);
            }

            //AutoCashBank,AutoBank
            CashName = "UsersAutoBank" + Users.Id.ToString();
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
            Users.AutoBank = AutoBank;

            if (Users.Amount < 0)
            {
                Users.Amount = 0;
            }
            //输出格式化钱
            Users.Amount = Users.Amount.Floor();
            Users.Frozen = Users.Frozen.Floor();

            var BaoConfig = Entity.BaoConfig.FirstOrNew();
            Users.GetCost = BaoConfig.GetCost;
            Users.YearPer = BaoConfig.YearPer;

            if (Users.CardPic.IsNullOrEmpty() && Users.CardType == 1)
            {  
                Users.IsAnewUpImg = 1;
            }
            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            Users.VipPrice = SysMoneySet.VipPrice;
            //Users.Cols = "Id,UserName,NeekName,TrueName,Mobile,QQ,Email,Address,Amount,Frozen,CardId,Pic,CardStae,CardFace,CardBack,CardPic,MiBao,CardNum";
            Users.Cols = "Id,UserName,NeekName,TrueName,Mobile,QQ,Email,Address,Amount,Frozen,CardId,Pic,CardStae,CardFace,CardBack,CardPic,MiBao,Token,CardNum,CardRemark,BankNum,MsgCount,AgentTel,YYOpenState,InTypeMobile,InTypePC,T0Times,T1Times,AutoBao,AutoCash,AutoCashMoney,AutoCashBank,AutoBank,GetCost,YearPer,YAmount,AllRec,LastRec,IsAnewUpImg,IsVip,VipPrice";
            DataObj.Data = Users.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
