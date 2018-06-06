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
using System.Configuration;
using System.Text.RegularExpressions;
using LokFu.Infrastructure;
using System.Web;

namespace LokFu.Controllers
{
    public class UsersTrueNameByAPIController : InitController
    {
        public UsersTrueNameByAPIController()
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
                Log.Write("[UsersTrueNameByAPI]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserAuth UserAuth = new UserAuth();
            UserAuth = JsonToObject.ConvertJsonToModel(UserAuth, json);

            SysSet SysSet = this.Entity.SysSet.FirstOrNew();

            if (!UserAuth.IdentityCode.IsNullOrEmpty())
            {
                var y = UserAuth.IdentityCode.Substring(6, 4);
                var m = UserAuth.IdentityCode.Substring(10, 2);
                var d = UserAuth.IdentityCode.Substring(12, 2);
                var birthday = new DateTime(int.Parse(y), int.Parse(m), int.Parse(d));
                var now = DateTime.Now;
                int age = now.Year - birthday.Year;
                if (now.Month < birthday.Month || (now.Month == birthday.Month && now.Day < birthday.Day)) { age--; }
                if (age > SysSet.AuthMaxAge || age < SysSet.AuthMinAge)
                {
                    DataObj.OutError("1104");
                    return;
                }
            }

            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            if (UserAuth.Token.IsNullOrEmpty()) {
                DataObj.OutError("1000");
                return;
            }
            if (UserAuth.NeekName.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (Users.X.IsNullOrEmpty() || Users.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }

            string HaoFu_Auth_Open = ConfigurationManager.AppSettings["HaoFu_Auth_Open"].ToString();
            //SysSet SysSet = Entity.SysSet.FirstOrNew();

            byte CardItemNum = 0;
            byte CardType = 0;
            byte UsedCardType = 0;
            if (Equipment.RqType == "Apple")
            {
                CardItemNum = SysSet.IosSet10;
                if (SysSet.IosSet11 == 0)
                {
                    CardType = 0;
                }
                if (SysSet.IosSet11 == 1)
                {
                    CardType = 2;
                }
                if (SysSet.IosSet11 == 2)
                {
                    CardType = 1;
                }
            }
            else {
                CardItemNum = SysSet.ApkSet10;
                if (SysSet.ApkSet11 == 0)
                {
                    CardType = 0;
                }
                if (SysSet.ApkSet11 == 1)
                {
                    CardType = 2;
                }
                if (SysSet.ApkSet11 == 2)
                {
                    CardType = 1;
                }
            }
            if (HaoFu_Auth_Open != "true") {
                CardItemNum = 6;//兼容直连六要素接口
            }
            if (CardItemNum == 6)
            {
                if (UserAuth.BankAccount.IsNullOrEmpty() || UserAuth.AccountName.IsNullOrEmpty() || UserAuth.IdentityCode.IsNullOrEmpty() || UserAuth.Mobile.IsNullOrEmpty() || UserAuth.CVV.IsNullOrEmpty() || UserAuth.EndDate.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
                CardType = 2;//六要素一定是信用卡
            }
            else if (CardItemNum == 4)
            {
                if (UserAuth.BankAccount.IsNullOrEmpty() || UserAuth.AccountName.IsNullOrEmpty() || UserAuth.IdentityCode.IsNullOrEmpty() || UserAuth.Mobile.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }
            else if (CardItemNum == 3)
            {
                if (UserAuth.BankAccount.IsNullOrEmpty() || UserAuth.AccountName.IsNullOrEmpty() || UserAuth.IdentityCode.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }
            else
            {
                DataObj.OutError("1000");
                return;
            }
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == UserAuth.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.CardStae == 2) { //已实名认证
                DataObj.OutError("2007");
                return;
            }
            int CardIdCount = Entity.Users.Count(n => n.CardId == UserAuth.IdentityCode && n.CardStae == 2);
            if (CardIdCount > 0) //身份证已用过
            {
                DataObj.OutError("2020");
                return;
            }
            //验证账号是否被限制
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == baseUsers.UserName && UBL.State == 1) != null)
            {
                //提示暂不支持您入网
                DataObj.OutError("2027");
                return;
            }
            //验证身份证是否被限制
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == UserAuth.IdentityCode && UBL.State == 2) != null)
            {
                //提示暂不支持您入网
                DataObj.OutError("2027");
                return;
            }
            //验证银行卡是否被限制
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == UserAuth.BankAccount && UBL.State == 3) != null)
            {
                //提示暂不支持您入网
                DataObj.OutError("2027");
                return;
            }
            //2016-10-07 非法关键词限制
            if (!SysSet.NoWord.IsNullOrEmpty())
            {
                string NeekName = Users.NeekName;
                NeekName = NeekName.Replace(" ", "").Replace("　", "");
                string[] NoWord = SysSet.NoWord.Split('|');
                bool hasNoWord = false;
                string noword = "";
                foreach (var p in NoWord)
                {
                    if (NeekName.Contains(p))
                    {
                        hasNoWord = true;
                        noword = p;
                        break;
                    }
                }
                if (hasNoWord)
                {
                    DataObj.Msg = "禁止使用关键词“" + noword + "”";
                    DataObj.OutError("2025");
                    return;
                }
            }

            //2016-09-30 限制重名提交
            Users tempUsers = Entity.Users.FirstOrDefault(n => n.NeekName == Users.NeekName && n.State == 1 && (n.CardStae == 2 || n.CardStae == 1));
            if (tempUsers != null)
            {
                DataObj.OutError("2024");
                return;
            }

            int Count = Entity.UserAuth.Count(n => n.UId == baseUsers.Id && n.IsCharge == 1);
            if (Count >= SysSet.AuthTimes)
            {
                DataObj.OutError("2022");
                return;
            }

            if (!UserAuth.EndDate.IsNullOrEmpty())
            {
                //处理年月问题传上来是MMYY
                //20151112调整成YYMM
                if (UserAuth.EndDate.Length == 4)
                {
                    string MM = UserAuth.EndDate.Substring(0, 2);
                    string YY = UserAuth.EndDate.Substring(2, 2);
                    UserAuth.EndDate = YY + MM;
                }
            }

            UserAuth.AddTime = DateTime.Now;
            UserAuth.UId = baseUsers.Id;
            UserAuth.IsCharge = 0;
            Entity.UserAuth.AddObject(UserAuth);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, UserAuth);

            #region 贴牌配置
            int ApkSet3 = SysSet.ApkSet3;
            int IosSet3 = SysSet.IosSet3;
            var vSysAgent = Entity.SysAgent.FirstOrDefault(o=>o.Id == baseUsers.Agent);
            if (vSysAgent == null)
            {
                DataObj.OutError("1000");
                return;
            }
            var topSysAgent = vSysAgent.GetTopAgent(Entity);
            if (topSysAgent != null && topSysAgent.IsTeiPai == 1)
            {
                ApkSet3 = topSysAgent.Set3;
                IosSet3 = topSysAgent.Set3;
            }
            #endregion

            #region 激活码逻辑
            Card Card = null;
            bool IsCheckCard = false;
            //必填
            if ((ApkSet3 == 1 && this.Equipment.RqType == "Android") || (IosSet3 == 1 && this.Equipment.RqType == "Apple"))
            {
                IsCheckCard = true;
                if (UserAuth.CardNum.IsNullOrEmpty() || UserAuth.CardPWD.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }

            //选填
            if ((ApkSet3 == 3 && this.Equipment.RqType == "Android") || (IosSet3 == 3 && this.Equipment.RqType == "Apple"))
            {
                if (!UserAuth.CardNum.IsNullOrEmpty() && !UserAuth.CardPWD.IsNullOrEmpty())
                {
                    IsCheckCard = true;
                }
            }

            if (IsCheckCard)
            {
                Card = Entity.Card.FirstOrDefault(n => n.Code == UserAuth.CardNum && n.PasWd == UserAuth.CardPWD && n.Auto == 1);
                if (Card == null)
                {
                    DataObj.OutError("5001");
                    return;
                }
                if (Card.State != 1)
                {
                    string StateStr = string.Empty;
                    switch (Card.State)
                    {
                        case 2:
                            StateStr = "已授权";
                            break;
                        case 3:
                            StateStr = "已使用";
                            break;
                        case 0:
                            StateStr = "已失效";
                            break;
                    }

                    DataObj.Msg = "激活码" + StateStr;
                    DataObj.OutError("5001");
                    return;
                }
                if (Card.AId.IsNullOrEmpty())
                {
                    DataObj.OutError("5002");
                    return;
                }
                if (Card.AdminId.IsNullOrEmpty())
                {
                    DataObj.OutError("5002");
                    return;
                }
                Card.State = 3;//使用中
                baseUsers.CardNum = Card.Code;
            }
            #endregion

            string ret_code = "";
            string ret_msg = "";
            string isCharge = "";

            string CONTENT = "";

            #region 第三方接口查询
            if (HaoFu_Auth_Open =="true")
            { 
                if (CardItemNum != 6)
                {
                    UsedCardType = GetCardType(UserAuth.BankAccount);
                    if (CardType == 1)//要求借记卡
                    {
                        if (UsedCardType != 1)
                        {
                            DataObj.OutError("2023");
                            Utils.WriteLog(CardType + "===" + UsedCardType + "无法查询的卡：" + UserAuth.BankAccount, "bankcard");
                            return;
                        }
                    }
                    if (CardType == 2)//要求信用卡
                    {
                        if (UsedCardType != 2)
                        {
                            DataObj.OutError("2023");
                            Utils.WriteLog(CardType + "===" + UsedCardType + "无法查询的卡：" + UserAuth.BankAccount, "bankcard");
                            return;
                        }
                    }
                }
                else {
                    UsedCardType = 2;
                }

                string HaoFu_Auth_MerId = ConfigurationManager.AppSettings["HaoFu_Auth_MerId"].ToString();
                string HaoFu_Auth_MerKey = ConfigurationManager.AppSettings["HaoFu_Auth_MerKey"].ToString();
                string HaoFu_Auth_Url = ConfigurationManager.AppSettings["HaoFu_Auth_Url"].ToString();

                string data = "{\"action\":\"authuser\",\"merid\":\"" + HaoFu_Auth_MerId + "\",\"orderid\":\"" + UserAuth.OId + "\",\"bankaccount\":\"" + UserAuth.BankAccount + "\",\"accountname\":\"" + UserAuth.AccountName + "\",\"identitycode\":\"" + UserAuth.IdentityCode + "\",\"mobile\":\"" + UserAuth.Mobile + "\",\"cvv\":\"" + UserAuth.CVV + "\",\"enddate\":\"" + UserAuth.EndDate + "\"}";
                string DataBase64 = LokFuEncode.Base64Encode(data, "utf-8");
                string Sign = (DataBase64 + HaoFu_Auth_MerKey).GetMD5();

                DataBase64 = HttpUtility.UrlEncode(DataBase64, Encoding.UTF8);
                string postdata = "req=" + DataBase64 + "&sign=" + Sign;

                CONTENT = Utils.PostRequest(HaoFu_Auth_Url, postdata, "utf-8");

                JObject JS = new JObject();
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
                }
                catch (Exception Ex)
                {
                    Log.Write("[UsersTrueNameByAPI]:", "【CONTENT】" + CONTENT, Ex);
                }
                if (JS == null)
                {
                    DataObj.OutError("2021");
                    return;
                }
                string resp = JS["resp"].ToString();
                CONTENT = LokFuEncode.Base64Decode(resp, "utf-8");
                try
                {
                    JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
                }
                catch (Exception Ex)
                {
                    Log.Write("[UsersTrueNameByAPI]:", "【CONTENT2】" + CONTENT, Ex);
                }
                if (JS == null)
                {
                    DataObj.OutError("2021");
                    return;
                }
                ret_code = JS["respcode"].ToString();
                ret_msg = JS["respmsg"].ToString();
                if (JS["ischarge"] != null)
                {
                    isCharge = JS["ischarge"].ToString();
                }
                if (isCharge == "1")
                {
                    UserAuth.IsCharge = 1;
                }
                else
                {
                    UserAuth.IsCharge = 0;
                }
            }
            #endregion

            #region 审核处理
            UserAuth.RetCode = ret_code;
            UserAuth.RetMsg = ret_msg;
            UserAuth.RetLog = CONTENT;

            baseUsers.TrueName = UserAuth.AccountName;
            baseUsers.NeekName = UserAuth.NeekName;
            baseUsers.CardId = UserAuth.IdentityCode;

            baseUsers.CardFace = "";
            
            baseUsers.CardType = 1;
            if (UsedCardType == 2)
            {
                baseUsers.HasT0 = 1;
            }
            else {
                baseUsers.HasT0 = 0;
            }
            
            string Code = "0000";
            if (ret_code == "0000")
            {
                baseUsers.CardStae = 2;//直接审核通过
                //=======================================
                string GPSAddress = Users.RegAddress;
                if (GPSAddress.IsNullOrEmpty())
                {
                    GPSAddress = Utils.GetAddressByGPS(Users.X, Users.Y);
                }
                if (!GPSAddress.IsNullOrEmpty())
                {
                    baseUsers.RegAddress = GPSAddress;
                    baseUsers.X = Users.X;
                    baseUsers.Y = Users.Y;
                }
                //=======================================
                UserTrack.ENo = DataObj.ENo;
                UserTrack.OPType = "自动认证";
                UserTrack.GPSAddress = GPSAddress;
                UserTrack.GPSX = Users.X;
                UserTrack.GPSY = Users.Y;
                baseUsers.SeavGPSLog(UserTrack, Entity);
                //=======================================

                //20151125需要对有绑卡的用户验证是否是来源与推广，增加抽奖次数
                #region 有人推广
                if (!baseUsers.MyPId.IsNullOrEmpty())
                {
                    if (baseUsers.ShareType == 2)
                    {
                        //支付通道升级推广
                        PayConfigChange PCC = Entity.PayConfigChange.FirstOrDefault(n => n.Id == baseUsers.PayConfigId && n.State == 1 && n.ShareNumber > 0);
                        if (PCC != null) {
                            int count = Entity.Users.Count(n => n.CardStae == 2 && n.State == 1 && n.MyPId == baseUsers.MyPId && n.PayConfigId == baseUsers.PayConfigId && n.Id != baseUsers.Id);

                            count++;//这里统计会漏掉当前用户的，需要增加1个。

                            if (count == PCC.ShareNumber) {//相等的那一次调整，避免超出了还一直调整
                                Users UP = Entity.Users.FirstOrDefault(n => n.Id == baseUsers.MyPId);
                                if (PCC.CState == 1)
                                {
                                    if (UP.Cash0 > PCC.Cash0)
                                    {
                                        UP.Cash0 = PCC.Cash0;
                                    }
                                    if (UP.ECash0 > PCC.ECash0)
                                    {
                                        UP.ECash0 = PCC.ECash0;
                                    }
                                }
                                if (PCC.EState == 1)
                                {
                                    if (UP.Cash1 > PCC.Cash1)
                                    {
                                        UP.Cash1 = PCC.Cash1;
                                    }
                                    if (UP.ECash1 > PCC.ECash1)
                                    {
                                        UP.ECash1 = PCC.ECash1;
                                    }
                                }
                                IList<UserPay> List = Entity.UserPay.Where(n => n.UId == baseUsers.MyPId).ToList();//获取用户配置
                                foreach (var p in List)
                                {
                                    PayConfigTemp PCT = Entity.PayConfigTemp.FirstOrDefault(n => n.PId == p.PId && n.PCCId == PCC.Id);
                                    //获取套餐配置
                                    if (PCT != null)
                                    {
                                        if (PCT.State == 1)
                                        {
                                            if (p.Cost > PCT.Cost)
                                            {
                                                p.Cost = PCT.Cost;
                                            }
                                        }
                                    }
                                }
                                string UserPayCashName = "UserPay_" + UP.Id.ToString() + "_" + Equipment.RqType;
                                CacheBuilder.EntityCache.Remove(UserPayCashName, null);
                                string PayConfigChangeCashName = "PayConfigChange_" + UP.Agent + "_" + UP.Id;
                                CacheBuilder.EntityCache.Remove(PayConfigChangeCashName, null);
                                //Utils.WriteLog("UserPayCashName:" + UserPayCashName + " PayConfigChangeCashName:" + PayConfigChangeCashName, "ClearCache");
                            }
                        }
                        
                    }
                    if (baseUsers.ShareType == 1){
                        //增加推广抽奖次数
                        TurnUsers TurnUsers = Entity.TurnUsers.FirstOrNew(n => n.UId == baseUsers.MyPId);
                        if (!TurnUsers.Id.IsNullOrEmpty())
                        {
                            TurnUsers.Times++;
                        }
                    }
                    #region 分享统计数
                    //===================================增加分享统计数===================================
                    //获取用户所属各级分润配置
                   // IList<UserPromoteGet> UserPromoteGetList = Entity.UserPromoteGet.Where(n => n.AgentID == baseUsers.Agent).OrderBy(n => n.PromoteLevel).ToList();
                    int MaxLevel = SysSet.GlobaPromoteMaxLevel;
                    //获取用户各级关系，最大级不超过用户配置级数。返回数据包含当前用户，当前用户级数标识Tier为0
                    IList<Users> UsersList = baseUsers.GetUsersById(Entity, MaxLevel);
                    foreach (var U in UsersList.Where(n => n.Tier > 0)){
                        ShareTotal ShareTotal = Entity.ShareTotal.FirstOrDefault(n => n.UId == U.Id && n.Tier == U.Tier);
                        if (ShareTotal == null)
                        {
                            ShareTotal = new ShareTotal();
                            ShareTotal.UId = U.Id;
                            ShareTotal.AddTime = DateTime.Now;
                            ShareTotal.ShareNum = 1;
                            ShareTotal.Amount = 0;
                            ShareTotal.Profit = 0;
                            ShareTotal.Tier = U.Tier;
                            Entity.ShareTotal.AddObject(ShareTotal);
                        }
                        else {
                            ShareTotal.ShareNum += 1;
                        }
                        Entity.SaveChanges();
                    }
                    //===================================增加分享统计数 End===================================
                    #endregion
                }
                #endregion

                if (Card != null)
                {
                    //代理商信息
                    baseUsers.Agent = Card.AId;
                    baseUsers.AId = Card.AdminId;
                    //推广信息
                    if (!Card.PUId.IsNullOrEmpty())
                    {
                        if (baseUsers.MyPId.IsNullOrEmpty())
                        {
                            //不是费率升级推广才有效
                            baseUsers.MyPId = Card.PUId;
                            baseUsers.PayConfigId = 0;
                            baseUsers.ShareType = 1;
                        }
                    }
                    Card.State = 2;
                }
            }
            else {
                Code = "2021";
                baseUsers.CardStae = 3;//审核失败
                baseUsers.CardNum = "";
                if (Card != null)
                {
                    Card.State = 1;
                }
            }
            Entity.SaveChanges();
            #endregion
            DataObj.Data = UserAuth.OutJson();
            DataObj.Code = Code;
            DataObj.OutString();
        }
        private byte GetCardType(string BankNum)
        {
            string HaoFu_Auth_MerId = ConfigurationManager.AppSettings["HaoFu_Auth_MerId"].ToString();
            string HaoFu_Auth_MerKey = ConfigurationManager.AppSettings["HaoFu_Auth_MerKey"].ToString();

            string data = "{\"action\":\"bankcard\",\"merid\":\"" + HaoFu_Auth_MerId + "\",\"bankaccount\":\"" + BankNum + "\"}";
            string DataBase64 = LokFuEncode.Base64Encode(data, "utf-8");
            string Sign = (DataBase64 + HaoFu_Auth_MerKey).GetMD5();

            DataBase64 = HttpUtility.UrlEncode(DataBase64, Encoding.UTF8);
            string postdata = "req=" + DataBase64 + "&sign=" + Sign;

            string CONTENT = Utils.PostRequest("https://api.zhifujiekou.com/api/bankcardtype", postdata, "utf-8");

            JObject JS = new JObject();
            try
            {
                JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
            }
            catch (Exception Ex)
            {
                Log.Write("[GetCardType]:", "【CONTENT】" + CONTENT, Ex);
            }
            if (JS == null)
            {
                return 0;
            }
            string resp = JS["resp"].ToString();
            CONTENT = LokFuEncode.Base64Decode(resp, "utf-8");
            try
            {
                JS = (JObject)JsonConvert.DeserializeObject(CONTENT);
            }
            catch (Exception Ex)
            {
                Log.Write("[GetCardType]:", "【CONTENT2】" + CONTENT, Ex);
            }
            if (JS == null)
            {
                return 0;
            }
            string ret_code = JS["respcode"].ToString();
            if (ret_code == "0000")
            {
                string CardType = JS["cardtype"].ToString();
                if (CardType == "1") {
                    return 1;
                }
                if (CardType == "2")
                {
                    return 2;
                }
                return 0;
            }
            else {
                return 0;
            }
        }
    }
}
