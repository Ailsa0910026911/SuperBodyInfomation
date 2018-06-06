using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace LokFu.Controllers
{
    public class UsersCheckController : InitController
    {
        public UsersCheckController()
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
                Log.Write("[UsersCheck]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.UserName.IsNullOrEmpty())
            { 
                DataObj.OutError("1000");
                return;
            }
            Users = Entity.Users.FirstOrDefault(n => n.UserName == Users.UserName);
            DataObj.Data = "";
            if (Users == null)//用户不存在
            {
                DataObj.Code = "0000";
            }
            else {
                DataObj.Code = "2005";
            }
            DataObj.OutString();
        }
    }
}
