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


namespace LokFu.Controllers
{
    public class MsgUserInfoController : InitController
    {
        public MsgUserInfoController()
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
                Log.Write("[MsgUserInfo]:", "【Data】" + Data, Ex);
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
            //if (baseUsers.CardStae != 2)//未实名认证
            //{
            //    DataObj.OutError("2006");
            //    return;
            //}
            //if (baseUsers.MiBao != 1)//未设置支付密码
            //{
            //    DataObj.OutError("2008");
            //    return;
            //}

            //获取信息
            MsgUser = Entity.MsgUser.FirstOrDefault(n => n.Id == MsgUser.Id && (n.UId == baseUsers.Id || n.UId == 0));
            if (MsgUser == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }
            else {//标识为已读
                if (MsgUser.State == 1 && MsgUser.UId > 0)
                {
                    MsgUser.State = 2;
                    Entity.SaveChanges();
                }
                else if (MsgUser.UId == 0)
                {
                    string uid=string.Format(",{0},",baseUsers.Id);
                    if (MsgUser.ReadUsers.IsNullOrEmpty()) {
                        MsgUser.ReadUsers = uid;
                    }else if (MsgUser.ReadUsers.IndexOf(uid) == -1) {
                        MsgUser.ReadUsers += baseUsers.Id.ToString() + ",";
                    }
                    Entity.SaveChanges();
                }
            }

            MsgUser.Info = Utils.RemoveHtml(MsgUser.Info);
            MsgUser.Info = MsgUser.Info.Replace("	", "");

            MsgUser.Cols = "Id,Name,Info,UId,AddTime";
            DataObj.Data = MsgUser.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
