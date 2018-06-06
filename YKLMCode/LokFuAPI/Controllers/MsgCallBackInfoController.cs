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
    public class MsgCallBackInfoController : InitController
    {
        public MsgCallBackInfoController()
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
                Log.Write("[MsgCallBackInfo]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            MsgCallBack MsgCallBack = new MsgCallBack();
            MsgCallBack = JsonToObject.ConvertJsonToModel(MsgCallBack, json);

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

            //获取信息
            MsgCallBack = Entity.MsgCallBack.FirstOrDefault(n => n.Id == MsgCallBack.Id && (n.UId == baseUsers.Id || n.UId == 0));
            if (MsgCallBack == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }

            DataObj.Data = MsgCallBack.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
