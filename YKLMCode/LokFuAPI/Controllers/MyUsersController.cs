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
    public class MyUsersController : InitController
    {
        public MyUsersController()
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
                Log.Write("[MyUsers]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.Token.IsNullOrEmpty() || Users.ShareType.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            PayConfigChange PayConfigChange = new PayConfigChange();

            PayConfigChange.Title = "分润";
            PayConfigChange.ShowTip = "";

            if (Users.ShareType == 2)
            {
                if (!Users.PayConfigId.IsNullOrEmpty())
                {
                    PayConfigChange = Entity.PayConfigChange.FirstOrDefault(n => n.Id == Users.PayConfigId);
                    if (PayConfigChange == null)
                    {
                        DataObj.OutError("1000");
                        return;
                    }
                }
            }
            Users baseUsers = null;
            if (Users.Token.IndexOf("Print|") == -1)
            {
                baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            }
            else
            {
                baseUsers = Entity.Users.FirstOrDefault(n => n.PrintToken == Users.Token);
            }
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
            IList<Users> UsersList = new List<Users>();
            if (Users.ShareType == 2)
            {
                if (Users.PayConfigId.IsNullOrEmpty())
                {
                    UsersList = Entity.Users.Where(n => n.MyPId == baseUsers.Id && n.State == 1 && n.ShareType == Users.ShareType).OrderByDescending(o => o.Id).ToList();
                }
                else
                {
                    UsersList = Entity.Users.Where(n => n.MyPId == baseUsers.Id && n.State == 1 && n.ShareType == Users.ShareType && n.PayConfigId == Users.PayConfigId).OrderByDescending(o => o.Id).ToList();
                }
            }
            else {
                UsersList = Entity.Users.Where(n => n.MyPId == baseUsers.Id && n.State == 1 && n.ShareType == Users.ShareType).OrderByDescending(o=>o.Id).ToList();
            }
            
            foreach (var p in UsersList) {
                p.Cols = "UserName,AddTime,State,CardRemark,Code,ShareType";
                p.CardRemark = PayConfigChange.Title;
                p.Code = PayConfigChange.ShowTip;
                if (p.CardStae == 2)
                {
                    p.State = 1;
                }else {
                    p.State = 0;
                }
            }
            DataObj.Data = UsersList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
