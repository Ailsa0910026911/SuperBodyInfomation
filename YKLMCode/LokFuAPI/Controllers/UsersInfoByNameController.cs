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
    public class UsersInfoByNameController : InitController
    {
        public UsersInfoByNameController()
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
                Log.Write("[UsersInfoByName]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.UserName.IsNullOrEmpty() && Users.Id.IsNullOrEmpty())
            { 
                DataObj.OutError("1000");
                return;
            }
            if (!Users.UserName.IsNullOrEmpty())
            {
                Users = Entity.Users.FirstOrDefault(n => n.UserName == Users.UserName);
            }else if (!Users.Id.IsNullOrEmpty())
            {
                Users = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            }
            if (Users == null) {
                DataObj.OutError("2001");
                return;
            }
            if (Users.Id.IsNullOrEmpty())//用户不存在
            {
                DataObj.OutError("2001");
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
            if (Users.Pic.IsNullOrEmpty())
            {
                Users.Pic = "none.png";
            }
            Users.Pic = Utils.ImageUrl("UsersPic", Users.Pic, AppImgPath);

            Users.Cols = "UserName,NeekName,TrueName,Pic";
            DataObj.Data = Users.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
