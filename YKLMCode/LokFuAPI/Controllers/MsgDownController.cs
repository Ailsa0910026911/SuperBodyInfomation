using LokFu.Extensions;
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
using System.Web.Script.Serialization;


namespace LokFu.Controllers
{
    public class MsgDownController : InitController
    {
        public MsgDownController()
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
                Log.Write("[MsgDown]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            MsgHelp MsgHelp = new MsgHelp();
            MsgHelp = JsonToObject.ConvertJsonToModel(MsgHelp, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == MsgHelp.Token);
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
            if (baseUsers.CardStae != 2)//未实名认证
            {
                //DataObj.OutError("2006");
                //return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                //DataObj.OutError("2008");
                //return;
            }
            MsgHelp MsgHelp_Temp = Entity.MsgHelp.FirstOrDefault(o => o.Id == MsgHelp.Id);
            if (MsgHelp_Temp == null)
            {
                DataObj.OutError("7070");
                return;
            }
            if (MsgHelp_Temp.State != 1)
            {
                DataObj.OutError("7070");
                return;
            }
            MsgHelp_Temp.Click = MsgHelp_Temp.Click + 1;
            Entity.SaveChanges();
            DataObj.Data = MsgHelp_Temp.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
