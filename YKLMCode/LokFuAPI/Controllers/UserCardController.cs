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
    public class UserCardController : InitController
    {
        public UserCardController()
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
                Log.Write("[UserCard]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserCard UserCard = new UserCard();
            UserCard = JsonToObject.ConvertJsonToModel(UserCard, json);

            if (UserCard.Type.IsNullOrEmpty())
            {
                UserCard.Type = 1;
            }

            //string CashName = "UserCard_" + UserCard.Type + "_" + UserCard.Token;
            //if (HasCache)
            //{
            //    string StringJson = CacheBuilder.EntityCache.Get(CashName, null) as string;
            //    if (!StringJson.IsNullOrEmpty())
            //    {
            //        DataObj.Data = StringJson;
            //        DataObj.Code = "0000";
            //        DataObj.OutString();
            //        return;
            //    }
            //}
            //获取用户信息
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
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }
            IList<UserCard> UserCardList = Entity.UserCard.Where(n => n.UId == baseUsers.Id && n.Type == UserCard.Type && n.State == 1).OrderBy(o => o.Id).ToList();
            var RunCardList = Entity.JobOrders.Where(o => (o.State == 2 || o.State == 3) && o.UId == baseUsers.Id).Select(o => new { o.UserCardId, o.TNum }).ToList();
            foreach (var item in UserCardList)
            {
                item.Cols += ",IsRun,TNum";
                var run = RunCardList.Where(o => o.UserCardId == item.Id).FirstOrDefault();
                if (run != null)
                {
                    item.IsRun = true;
                    item.TNum = run.TNum;
                }
                else
                {
                    item.IsRun = false;
                    item.TNum = "";
                }

            }

            string data = UserCardList.EntityToJson();
            //if (HasCache)
            //{
            //    CacheBuilder.EntityCache.Remove(CashName, null);
            //    CacheBuilder.EntityCache.Add(CashName, data, DateTime.Now.AddMinutes(15), null);
            //}
            DataObj.Data = data;
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
