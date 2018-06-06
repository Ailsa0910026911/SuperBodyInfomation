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
    public class UsersGetPassController : InitController
    {
        public UsersGetPassController()
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
            //1.0接口不开放，关闭处理2016-06-27
            DataObj.OutError("9999");
            return;
            //string Data = DataObj.GetData();
            //if (Data.IsNullOrEmpty())
            //{
            //    DataObj.OutError("1000");
            //    return;
            //}
            //JObject json = new JObject();
            //try
            //{
            //    json = (JObject)JsonConvert.DeserializeObject(Data);
            //}
            //catch (Exception Ex)
            //{
            //    Log.Write("[UsersGetPass]:", "【Data】" + Data, Ex);
            //}
            //if (json == null) {
            //    DataObj.OutError("1000");
            //    return;
            //}
            ////JObject pp = (JObject)p;

            ////身份证号，手机号（登录帐户），邮箱
            ////CardId,UserName,Email

            //Users Users = new Users();
            //Users = JsonToObject.ConvertJsonToModel(Users, json);
            //if (Users.UserName.IsNullOrEmpty() || Users.CardId.IsNullOrEmpty() || Users.TrueName.IsNullOrEmpty())
            //{
            //    DataObj.OutError("1000");
            //    return;
            //}
            //Users BaseUsers = Entity.Users.Where(n => n.UserName == Users.UserName).FirstOrDefault();
            //if (BaseUsers == null)//用户不存在
            //{
            //    DataObj.OutError("2001");
            //    return;
            //}
            //if (BaseUsers.State != 1)
            //{
            //    DataObj.OutError("2003");
            //    return;
            //}
            //if (BaseUsers.CardStae != 2)//未实名认证
            //{
            //    DataObj.OutError("2006");
            //    return;
            //}
            //if (BaseUsers.TrueName != Users.TrueName)
            //{
            //    DataObj.OutError("2011");
            //    return;
            //}
            //if (BaseUsers.CardId != Users.CardId)
            //{
            //    DataObj.OutError("2012");
            //    return;
            //}
            //DateTime now = DateTime.Now;
            //Guid Gid=Guid.NewGuid();
            //string mdstr = Users.Id + "|" + Users.UserName + "|" + Gid.ToString() +"|" + now.ToString();
            //string taken = mdstr.GetMD5();
            //BaseUsers.Token = "gggg" + taken;
            //Entity.SaveChanges();
            //BaseUsers.Cols = "Token";
            //DataObj.Data = BaseUsers.OutJson();
            //DataObj.Code = "0000";
            //DataObj.OutString();
        }
    }
}
