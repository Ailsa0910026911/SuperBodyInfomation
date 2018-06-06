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

namespace LokFu.Controllers
{
    public class UsersTrueName_2_0Controller : InitController
    {
        public UsersTrueName_2_0Controller()
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
                Log.Write("[UsersTrueName_2_0]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            SysSet SysSet = Entity.SysSet.FirstOrNew();
            if (!Users.CardId.IsNullOrEmpty())
            {
                var y = Users.CardId.Substring(6, 4);
                var m = Users.CardId.Substring(10, 2);
                var d = Users.CardId.Substring(12, 2);
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

            //验证身份证是否被限制
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.CardId && UBL.State == 2) != null)
            {
                //提示暂不支持您入网
                DataObj.OutError("2027");
                return;
            }
            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);
            //Users.CardPic = System.Web.HttpContext.Current.Request.Form["cardpic"];

            if (Users.CardPic.IsNullOrEmpty() || Users.CardPic == "Err")
            {
                DataObj.OutError("4001");
                return;
            }
            if (Users.TrueName.IsNullOrEmpty() || Users.CardId.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
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

            if (baseUsers.CardStae == 2)
            { //已实名认证
                DataObj.OutError("2007");
                return;
            }
            int CardIdCount = Entity.Users.Count(n => n.CardId == Users.CardId && n.CardStae == 2);
            if (CardIdCount > 0) //身份证已用过
            {
                DataObj.OutError("2020");
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
                foreach (var p in NoWord) {
                    if (NeekName.Contains(p)) {
                        hasNoWord = true;
                        noword = p;
                        break;
                    }
                }
                if (hasNoWord) {
                    DataObj.Msg = "禁止使用关键词“" + noword + "”";
                    DataObj.OutError("2025");
                    return;
                }
            }

            //2016-09-30 限制重名提交
            Users tempUsers = Entity.Users.FirstOrDefault(n => n.NeekName == Users.NeekName && n.State == 1 && (n.CardStae == 2 || n.CardStae == 1));
            if (tempUsers != null) {
                DataObj.OutError("2024");
                return;
            }

            #region 贴牌配置
            int ApkSet3 = SysSet.ApkSet3;
            int IosSet3 = SysSet.IosSet3;
            var vSysAgent = Entity.SysAgent.FirstOrDefault(o => o.Id == baseUsers.Agent);
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
            bool IsCheckCard = false;
            //必填
            if ((ApkSet3 == 1 && this.Equipment.RqType == "Android") || (IosSet3 == 1 && this.Equipment.RqType == "Apple"))
            {
                IsCheckCard = true;
                if (Users.CardNum.IsNullOrEmpty() || Users.CardPWD.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }

            //选填
            if ((ApkSet3 == 3 && this.Equipment.RqType == "Android") || (IosSet3 == 3 && this.Equipment.RqType == "Apple"))
            {
                if (!Users.CardNum.IsNullOrEmpty() && !Users.CardPWD.IsNullOrEmpty())
                {
                    IsCheckCard = true;
                }
            }

            if (IsCheckCard)
            {
                Card Card = Entity.Card.FirstOrDefault(n => n.Code == Users.CardNum && n.PasWd == Users.CardPWD && n.Auto == 1);
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

            Users.CardPic = Utils.Base64StringToImage(Users.CardPic, "Users");
            Users.CardFace = Utils.Base64StringToImage(Users.CardFace, "Users");
            Users.CardBack = Utils.Base64StringToImage(Users.CardBack, "Users");
            Users.CarLicensePic = Users.CarLicensePic.IsNullOrEmpty() ? string.Empty : Utils.Base64StringToImage(Users.CarLicensePic, "Users");
            Users.CarLocationPic = Users.CarLocationPic.IsNullOrEmpty() ? string.Empty : Utils.Base64StringToImage(Users.CarLocationPic, "Users");
            Users.CarOther = Users.CarOther.IsNullOrEmpty() ? string.Empty : Utils.Base64StringToImage(Users.CarOther, "Users");

            baseUsers.CardStae = 1;
            baseUsers.TrueName = Users.TrueName;
            baseUsers.NeekName = Users.NeekName;
            baseUsers.CardId = Users.CardId;
            baseUsers.CardPic = Users.CardPic;
            baseUsers.CardFace = Users.CardFace;
            baseUsers.CardBack = Users.CardBack;
            baseUsers.CarLicensePic = Users.CarLicensePic;
            baseUsers.CarLocationPic = Users.CarLocationPic;
            baseUsers.CarOther = Users.CarOther;
            baseUsers.AddAuthTime = DateTime.Now;

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

            Entity.SaveChanges();
            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "人工认证";
            UserTrack.GPSAddress = GPSAddress;
            UserTrack.GPSX = Users.X;
            UserTrack.GPSY = Users.Y;
            baseUsers.SeavGPSLog(UserTrack, Entity);
            //=======================================


            baseUsers.CardPic = Utils.ImageUrl("Users", baseUsers.CardPic, AppImgPath);
            baseUsers.CardFace = Utils.ImageUrl("Users", baseUsers.CardFace, AppImgPath);
            baseUsers.CardBack = Utils.ImageUrl("Users", baseUsers.CardBack, AppImgPath);
            baseUsers.CarLicensePic = Utils.ImageUrl("Users", baseUsers.CarLicensePic, AppImgPath);
            baseUsers.CarLocationPic = Utils.ImageUrl("Users", baseUsers.CarLocationPic, AppImgPath);
            baseUsers.CarOther = Utils.ImageUrl("Users", baseUsers.CarOther, AppImgPath);

            baseUsers.Cols = "TrueName,CardStae,CardPic,CardFace,CardBack,CardId,CarLicensePic,CarLocationPic,CarOther";

            DataObj.Data = baseUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
