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
    public class UsersPayPwdController : InitController
    {
        public UsersPayPwdController()
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
                Log.Write("[UsersPayPwd]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            if (Users.PassWord.IsNullOrEmpty() || Users.Token.IsNullOrEmpty())//使用PassWord字段
            {
                DataObj.OutError("1000");
                return;
            }
            if (Users.X.IsNullOrEmpty() || Users.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
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
            if (baseUsers.CardStae != 2)
            { //未实名认证
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao == 1)
            { //已设置支付密码
                DataObj.OutError("2009");
                return;
            }
            baseUsers.MiBao = 1;
            baseUsers.PayPwd = Users.PassWord.GetPayMD5();
            Entity.SaveChanges();

            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "设置支付密码";
            UserTrack.GPSAddress = Users.RegAddress;
            UserTrack.GPSX = Users.X;
            UserTrack.GPSY = Users.Y;
            baseUsers.SeavGPSLog(UserTrack, Entity);
            //=======================================

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
