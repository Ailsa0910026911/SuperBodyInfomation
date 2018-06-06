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

namespace LokFu.Controllers
{
    public class UsersEditController : InitController
    {
        public UsersEditController()
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
                DataObj.OutError("1000.");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[UsersEdit]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
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
            if (baseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.MobileState == 1)//已验证不能改
            {
                Users.Mobile = baseUsers.Mobile;
            }
            if (baseUsers.EmailState == 1)//已验证不能改
            {
                Users.Email = baseUsers.Email;
            }
            if (baseUsers.CardStae == 1)//已验证不能改
            {
                Users.TrueName = baseUsers.TrueName;
            }
            Users.Cols = "NeekName,TrueName,Mobile,QQ,Email,Address,InTypePC,InTypeMobile";
            bool NoAllow = false;
            foreach (KeyValuePair<string, JToken> p in json)
            {
                if (p.Key == "intypemobile" || p.Key == "intypepc")
                {
                    NoAllow = true;
                }
            }
            if (NoAllow) {
                DataObj.OutError("1009");
                return;
            }
            foreach (KeyValuePair<string, JToken> p in json)
            {
                string pname = p.Key;
                if (Users.Cols.ToLower().IndexOf(pname) != -1) {
                    switch (pname) {
                        case "neekname":
                            baseUsers.NeekName = Users.NeekName;
                            break;
                        case "truename":
                            baseUsers.TrueName = Users.TrueName;
                            break;
                        case "mobile":
                            baseUsers.Mobile = Users.Mobile;
                            break;
                        case "qq":
                            baseUsers.QQ = Users.QQ;
                            break;
                        case "email":
                            baseUsers.Email = Users.Email;
                            break;
                        case "intypepc":
                            baseUsers.InTypePC = Users.InTypePC;
                            break;
                        case "intypemobile":
                            baseUsers.InTypeMobile = Users.InTypeMobile;
                            break;
                    }
                }
            }
            Entity.SaveChanges();
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
            //Tools.OutString(ErrInfo.Return("0000"));
        }
    }
}
