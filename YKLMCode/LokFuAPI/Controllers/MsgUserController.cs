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
    public class MsgUserController : InitController
    {
        public MsgUserController()
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

            string uid = string.Format(",{0},", baseUsers.Id);

            EFPagingInfo<MsgUser> p = new EFPagingInfo<MsgUser>();
            if (!MsgUser.Pg.IsNullOrEmpty()) { p.PageIndex = MsgUser.Pg; }
            if (!MsgUser.Pgs.IsNullOrEmpty()) { p.PageSize = MsgUser.Pgs; }
            //p.SqlWhere.Add(f => f.UId == baseUsers.Id || ( !f.DeleteUsers.Contains(uid) && f.SendUsers.Contains(uid)) );群发功能检索
            p.SqlWhere.Add(f => f.UId == baseUsers.Id );
            p.SqlWhere.Add(f => f.State > 0 && f.AddTime > baseUsers.AddTime);

            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgUser> List = Entity.Selects<MsgUser>(p);

            foreach (var P in List.Where(n => n.UId == 0))
            { //处理已读
                if (P.ReadUsers.IsNullOrEmpty()) {
                    P.ReadUsers = string.Empty;
                }
                if (P.ReadUsers.IndexOf(uid) != -1) {
                    P.State = 2;
                }
                P.Info = Utils.RemoveHtml(P.Info);
                P.Info = P.Info.Replace("	", "");
            }

            IList<MsgUser> iList = List.ToList();
            StringBuilder sb = new StringBuilder("");
            sb.Append("{");
            sb.Append(List.PageToString());
            sb.Append(",");
            sb.Append(iList.EntityToString());
            sb.Append("}");
            DataObj.Data = sb.ToString();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
