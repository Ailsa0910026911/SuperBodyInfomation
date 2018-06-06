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
    public class MsgNoticeInfoController : InitController
    {
        public MsgNoticeInfoController()
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
                Log.Write("[MsgNoticeInfo]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            MsgNotice MsgNotice = new MsgNotice();
            MsgNotice = JsonToObject.ConvertJsonToModel(MsgNotice, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == MsgNotice.Token);
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
            MsgNotice = Entity.MsgNotice.FirstOrDefault(n => n.Id == MsgNotice.Id && (n.NType == 0 || n.NType == 3));
            if (MsgNotice == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }
            else {//标识为已读
                string ReadUsers = MsgNotice.ReadUsers;
                string UserId = string.Format("|{0}|", baseUsers.Id);
                if (ReadUsers.IndexOf(UserId) == -1)
                {
                    if (ReadUsers.IsNullOrEmpty())
                    {
                        ReadUsers = UserId;
                    }
                    else
                    {
                        ReadUsers = string.Format("{0}{1}|", ReadUsers, baseUsers.Id);
                    }
                    MsgNotice.ReadUsers = ReadUsers;
                    Entity.SaveChanges();
                }
            }

            MsgNotice.Info = Utils.RemoveHtml(MsgNotice.Info);
            MsgNotice.Info = MsgNotice.Info.Replace("	", "");

            MsgNotice.Cols = "Id,Name,Info";
            DataObj.Data = MsgNotice.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
