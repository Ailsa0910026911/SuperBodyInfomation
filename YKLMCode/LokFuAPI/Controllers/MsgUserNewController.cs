using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LokFu.Controllers
{
    public class MsgUserNewController : InitController
    {
        public MsgUserNewController()
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
                Log.Write("[MsgUser]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            MsgUser MsgUser = new MsgUser();
            MsgUser = JsonToObject.ConvertJsonToModel(MsgUser, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == MsgUser.Token);
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
            
            string uid = string.Format(",{0},", baseUsers.Id);

            int Count = Entity.MsgUser.Count(n => (n.UId == baseUsers.Id && n.State == 1) || (n.UId == 0 && !n.ReadUsers.Contains(uid) && !n.DeleteUsers.Contains(uid) && n.AddTime > baseUsers.AddTime && (n.SendUsers.Contains(uid) || n.SendUsers == null || n.SendUsers == "")));

            baseUsers.Cols = "MsgCount";
            baseUsers.MsgCount = Count;

            DataObj.Data = baseUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
