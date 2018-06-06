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
    public class UsersPayInfoController : InitController
    {
        public UsersPayInfoController()
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
                Log.Write("[UsersPay]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserPay UserPay = new UserPay();
            UserPay = JsonToObject.ConvertJsonToModel(UserPay, json);
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == UserPay.Token);
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
            UserPay = Entity.UserPay.FirstOrDefault(n => n.UId == baseUsers.Id && n.PId == UserPay.PId);
            if (UserPay == null) {
                DataObj.OutError("1001");
                return;
            }
            DataObj.Data = UserPay.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
