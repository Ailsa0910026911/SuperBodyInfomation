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
    public class MsgUserDeleteController : InitController
    {
        public MsgUserDeleteController()
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

            string Delete = MsgUser.Delete;//要删除的ID列
            string[] Arr = Delete.Split(',');
            List<int> List = new List<int>();
            try
            {
                foreach (var p in Arr)
                {
                    string ppp = p.Trim();
                    int pp = Int32.Parse(ppp);
                    List.Add(pp);
                }
            }
            catch { 
                
            }

            IList<MsgUser> MsgUserList = Entity.MsgUser.Where(n => List.Contains(n.Id)).ToList();
            //删除自己的
            string DeleteIds = "0";
            foreach (var p in MsgUserList.Where(n => n.UId == baseUsers.Id))
            {
                DeleteIds += "," + p.Id;
            }
            string SQL = "Update MsgUser Set IsDel=1 Where Id in (" + DeleteIds + ")";
            Entity.ExecuteStoreCommand(SQL);

            //全员标识已删除
            string uid = string.Format(",{0},", baseUsers.Id);
            foreach (var p in MsgUserList.Where(n => n.UId == 0))
            {
                if (p.DeleteUsers.IsNullOrEmpty())
                {
                    p.DeleteUsers = uid;
                }
                else if (p.DeleteUsers.IndexOf(uid) == -1)
                {
                    p.DeleteUsers += baseUsers.Id.ToString() + ",";
                }
            }
            Entity.SaveChanges();
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
