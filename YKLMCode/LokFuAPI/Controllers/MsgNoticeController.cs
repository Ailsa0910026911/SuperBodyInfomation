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
    public class MsgNoticeController : InitController
    {
        public MsgNoticeController()
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
                Log.Write("[MsgNotice]:", "【Data】" + Data, Ex);
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

            //下次改版让手机端传AgentId，这让有点伤性能，然后删除这条备注
            SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
            SysAgent = SysAgent.GetTopAgent(Entity);

            EFPagingInfo<MsgNotice> p = new EFPagingInfo<MsgNotice>();
            if (!MsgNotice.Pg.IsNullOrEmpty()) { p.PageIndex = MsgNotice.Pg; }
            if (!MsgNotice.Pgs.IsNullOrEmpty()) { p.PageSize = MsgNotice.Pgs; }

            p.SqlWhere.Add(f => f.NType == 0 || f.NType == 3);
            p.SqlWhere.Add(f => f.State == 1 && f.AddTime > baseUsers.AddTime);
            int AgentId = 999999999;
            if (SysAgent.IsTeiPai == 1) {
                AgentId = SysAgent.Id;
            }
            p.SqlWhere.Add(f => f.AgentId == 0 || f.AgentId == AgentId);

            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgNotice> List = Entity.Selects<MsgNotice>(p);

            //处理以读未读
            string UserId = string.Format("|{0}|", baseUsers.Id);
            foreach (var pp in List) {
                pp.State = (byte)(pp.ReadUsers != null && pp.ReadUsers.IndexOf(UserId) == -1 ? 1 : 2);
                if(pp.Info!=null)
                {
                    pp.Info = Utils.RemoveHtml(pp.Info);
                    pp.Info = pp.Info.Replace("	", "");
                }
            }
            IList<MsgNotice> iList = List.ToList();
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
