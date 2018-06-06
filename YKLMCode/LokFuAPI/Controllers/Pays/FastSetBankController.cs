using LokFu.FastPay;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LokFu.Controllers
{
    public class FastSetBankController : InitController
    {
        public FastSetBankController()
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
                Log.Write("[FastSetBankController]:", "【Data】" + Data, Ex);
                json = null;
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserCard UserCard = new UserCard();
            UserCard = JsonToObject.ConvertJsonToModel(UserCard, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == UserCard.Token);
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

            FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == baseUsers.Id);
            if (FastUser == null)
            {
                DataObj.OutError("2035");
                return;
            }

            UserCard UC = Entity.UserCard.FirstOrDefault(n => n.UId == baseUsers.Id && n.Type == 1 && n.Id == UserCard.Id && n.State == 1);
            if (UC == null)
            {
                DataObj.OutError("1001");
                return;
            }

            if (FastUser.Card == UC.Card)
            {
                DataObj.OutError("2080");
                return;
            }
            string GPSRemark = "旧默认结算卡:" + FastUser.Card + " 新默认结算卡:" + UC.Card;

            FastUser.Card = UC.Card;
            FastUser.Bank = UC.Bank;
            FastUser.Bin = UC.Bin;
            Entity.SaveChanges();

            //这里要增加所有通道的商户注册及绑卡操作
            //=============================================================
            IList<FastPayWay> FastPayWayList = Entity.FastPayWay.OrderBy(n => n.Sort).ToList();
            //购买商户与默认商户都需要改卡
            IList<FastUserPay> FastUserPayList = Entity.FastUserPay.Where(n => n.UId == baseUsers.Id).OrderBy(n => n.PayWay).ToList();
            foreach (var p in FastUserPayList)
            {
                FastPayWay FastPayWay = FastPayWayList.FirstOrDefault(n => n.Id == p.PayWay);
                if (FastPayWay != null)
                {
                    if (FastPayWay.DllName == "MiBank" || FastPayWay.DllName == "HFPay" || FastPayWay.DllName == "ZBLHPay" || FastPayWay.DllName == "JiFuJFPay")
                    {
                        p.CardState = 1;//不需要验卡
                    }
                    else
                    {
                        p.CardState = 2;//重新标识状态为待提交
                    }
                    p.Bank = FastUser.Bank;
                    p.Card = FastUser.Card;
                    p.Bin = FastUser.Bin;
                    BusFastPay.AddCard(FastUser, p, FastPayWay, Entity);
                }
            }
            Entity.SaveChanges(); 

            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "更换默认结算卡";
            UserTrack.UserName = "";
            UserTrack.Remark = GPSRemark;
            UserTrack.UId = FastUser.UId;
            UserTrack.SeavGPSLog(Entity);
            //=======================================

            FastUser.Cols = "Card,Bank,Bin";
            DataObj.Data = FastUser.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();

        }
    }
}