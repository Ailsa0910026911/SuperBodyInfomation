using System.Collections.Generic;
using System.Linq;
using System;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.Extensions;
using System.Configuration;
using LokFu.WeiXin.Repositories;
using LokFu.WeiXin;
using System.Threading;
using System.Web;

namespace LokFu.Controllers
{
    public class ZhouBianDailyController : InitController
    {
        public static string AccToken = "";
        public ZhouBianDailyController()
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
                Log.Write("[ZhouBianDaily]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
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
            YYDevice YYDevice = Entity.YYDevice.FirstOrDefault(n => n.UId == baseUsers.Id);
            if (YYDevice == null) { 
                DataObj.OutError("1000");
                return;
            }
            DateTime Last = DateTime.Now.AddDays(-31);
            IList<YYDaily> YYDailyList = Entity.YYDaily.Where(n => n.DevId == YYDevice.DevId && n.OutDate > Last).OrderByDescending(n => n.OutDate).ToList();
            DataObj.Data = YYDailyList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }

    }
}
