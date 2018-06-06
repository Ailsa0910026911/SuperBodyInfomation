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
    public class BaoUsersController : BaoController
    {
        public BaoUsersController()
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
            DataObj.OutError("1020");
            return;
            /*
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
                Log.Write("[BaoUsers]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            BaoUsers BaoUsers = new BaoUsers();
            BaoUsers = JsonToObject.ConvertJsonToModel(BaoUsers, json);
            if (BaoUsers.Token.IsNullOrEmpty())
            { 
                DataObj.OutError("1000");
                return;
            }

            var sysSet = Entity.SysSet.FirstOrDefault();

            Users Users = Entity.Users.FirstOrDefault(n => n.Token == BaoUsers.Token);
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
            if (Users.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (Users.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }
            BaoUsers = Entity.BaoUsers.FirstOrDefault(n => n.UId == Users.Id);
            if (BaoUsers == null) {
                BaoUsers = new BaoUsers();
                BaoUsers.UId = Users.Id;
                BaoUsers.AllMoney = 0;
                BaoUsers.ActMoney = 0;
                BaoUsers.InMoney = 0;
                BaoUsers.AllRec = 0;
                BaoUsers.LastRec = 0;
                BaoUsers.AddTime = DateTime.Now;
                Entity.BaoUsers.AddObject(BaoUsers);
                Entity.SaveChanges();
            }
            BaoConfig BaoConfig = Entity.BaoConfig.FirstOrNew();

            BaoUsers.GetCost = BaoConfig.GetCost;
            BaoUsers.YearPer = BaoConfig.YearPer;
            BaoUsers.Alert = sysSet.BaoUserAlert;

            DataObj.Data = BaoUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
            */
        }
    }
}
