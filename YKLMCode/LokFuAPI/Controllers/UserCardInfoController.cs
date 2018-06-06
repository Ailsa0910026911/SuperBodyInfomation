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
    public class UserCardInfoController : InitController
    {
        public UserCardInfoController()
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
                Log.Write("[UserCardInfo]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserCard UserCard = new UserCard();
            UserCard = JsonToObject.ConvertJsonToModel(UserCard, json);
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
            UserCard = Entity.UserCard.FirstOrDefault(n => n.Id == UserCard.Id && n.UId == baseUsers.Id && n.State == 1);
            if (UserCard == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }

            if (!UserCard.Province.IsNullOrEmpty())
            {
                BasicProvince BasicProvince = Entity.BasicProvince.FirstOrDefault(n => n.Id == UserCard.Province);
                if (BasicProvince != null)
                {
                    UserCard.ProvinceName = BasicProvince.Name;
                }
            }
            if (!UserCard.City.IsNullOrEmpty())
            {
                BasicCity BasicCity = Entity.BasicCity.FirstOrDefault(n => n.Id == UserCard.City);
                if (BasicCity != null)
                {
                    UserCard.CityName = BasicCity.Name;
                }
            }

            UserCard.Cols = "Id,Bank,Card,Name,Type,Deposit,Province,City,District,Bin,BId,ProvinceName,CityName,Mobile";
            DataObj.Data = UserCard.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
