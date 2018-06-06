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
    public class UsersPayController : InitController
    {
        public UsersPayController()
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

            IList<PayConfig> PayConfigList = Entity.PayConfig.Where(n => n.State == 1).OrderBy(n => n.Sort).ToList();
            IList<UserPay> UserPayList = Entity.UserPay.Where(n => n.UId == baseUsers.Id).ToList();
            IList<UserPay> List = new List<UserPay>();
            foreach (var p in PayConfigList) {
                UserPay = UserPayList.FirstOrDefault(n => n.PId == p.Id);
                if (UserPay != null) {
                    UserPay.Name = p.Name;
                    List.Add(UserPay);
                }
            }

            DataObj.Data = List.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
