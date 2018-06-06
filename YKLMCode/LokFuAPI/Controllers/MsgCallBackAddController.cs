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
    public class MsgCallBackAddController : InitController
    {
        public MsgCallBackAddController()
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
                Log.Write("[MsgCallBackAdd]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            MsgCallBack MsgCallBack = new MsgCallBack();
            MsgCallBack = JsonToObject.ConvertJsonToModel(MsgCallBack, json);

            if (MsgCallBack.Token.IsNullOrEmpty()||MsgCallBack.Name.IsNullOrEmpty()||MsgCallBack.Info.IsNullOrEmpty()){
                DataObj.OutError("1000");
                return;
            }

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == MsgCallBack.Token);
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
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }

            MsgCallBack.UId = baseUsers.Id;
            MsgCallBack.NeekName = baseUsers.TrueName;
            MsgCallBack.Linker = baseUsers.Mobile;
            MsgCallBack.AddTime = DateTime.Now;
            MsgCallBack.State = 1;

            Entity.MsgCallBack.AddObject(MsgCallBack);
            Entity.SaveChanges();

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
