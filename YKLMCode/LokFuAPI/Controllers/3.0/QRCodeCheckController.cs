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
using System.Data.Objects;

namespace LokFu.Controllers
{
    public class QRCodeCheckController : InitController
    {
        public QRCodeCheckController()
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
                Log.Write("[QRCodeCheck]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserLoginSceneid UserLoginSceneid = new UserLoginSceneid();
            UserLoginSceneid = JsonToObject.ConvertJsonToModel(UserLoginSceneid, json);
            if (UserLoginSceneid.Sceneid.IsNullOrEmpty() || UserLoginSceneid.Token.IsNullOrEmpty())
            { 
                DataObj.OutError("1000");
                return;
            }
            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == UserLoginSceneid.Token);
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

            UserLoginSceneid SLS = Entity.UserLoginSceneid.FirstOrDefault(n => n.Sceneid == UserLoginSceneid.Sceneid);
            if (SLS == null) {//不存在
                DataObj.OutError("2040");
                return;
            }
            if (!SLS.UId.IsNullOrEmpty()) {//已使用
                DataObj.OutError("2040");
                return;
            }
            if (SLS.AddTime.AddMinutes(5) < DateTime.Now) {//超时
                DataObj.OutError("2040");
                return;
            }
            if (!SLS.UId.IsNullOrEmpty())
            {//已使用
                DataObj.OutError("2040");
                return;
            }
            SLS.Times++;//扫次数+1;
            if (!SLS.Token.IsNullOrEmpty())
            {
                if (SLS.Token != UserLoginSceneid.Token) {//被别人扫过
                    DataObj.OutError("2040");
                    return;
                }
            }
            else {
                SLS.Token = UserLoginSceneid.Token;
            }
            Entity.SaveChanges();
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
