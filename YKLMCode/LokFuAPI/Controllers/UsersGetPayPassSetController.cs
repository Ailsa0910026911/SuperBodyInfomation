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
    public class UsersGetPayPassSetController : InitController
    {
        public UsersGetPayPassSetController()
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
                Log.Write("[UsersGetPayPassSet]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            string trueToken = string.Empty;

            if(!Users.PToken.IsNullOrEmpty())
            {
                trueToken = Users.PToken;
            }
            else
            {
                if (!Users.Token.IsNullOrEmpty())
                {
                    trueToken = Users.Token;
                }
                else
                {
                    DataObj.OutError("1000");
                    return;
                }
            }

            if (Users.UserName.IsNullOrEmpty() || Users.PassWord.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            if (Users.X.IsNullOrEmpty() || Users.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            ////手机号码黑名单验证
            //if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.UserName && UBL.State == 1) != null)
            //{
            //    //提示暂不支持您手机号入网
            //    DataObj.OutError("2026");
            //    return;
            //}
            if (trueToken.Substring(0, 4) != "pppp")
            {
                DataObj.OutError("9999");
                return;
            }
            Users BaseUsers = Entity.Users.Where(n => n.Token == trueToken && n.UserName == Users.UserName).FirstOrDefault();
            if (BaseUsers == null)//令牌错误
            {
                DataObj.OutError("2014");
                return;
            }
            if (BaseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (BaseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (BaseUsers.MiBao != 1)
            {
                DataObj.OutError("2008");
                return;
            }
            BaseUsers.PayLock = 0;//解除锁定
            BaseUsers.PayErr = 0;//解除锁定
            BaseUsers.PayPwd = Users.PassWord.GetPayMD5();
            Entity.SaveChanges();

            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "重设支付密码";
            UserTrack.GPSAddress = Users.RegAddress;
            UserTrack.GPSX = Users.X;
            UserTrack.GPSY = Users.Y;
            BaseUsers.SeavGPSLog(UserTrack, Entity);
            //=======================================

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
