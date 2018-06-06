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
    public class BaoListController : BaoController
    {
        public BaoListController()
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
                Log.Write("[BaoList]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            BaoLog BaoLog = new BaoLog();
            BaoLog = JsonToObject.ConvertJsonToModel(BaoLog, json);
            if (BaoLog.Token.IsNullOrEmpty())
            { 
                DataObj.OutError("1000");
                return;
            }

            Users Users = Entity.Users.FirstOrDefault(n => n.Token == BaoLog.Token);
            if (Users == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (Users.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (Users.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (Users.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }

            EFPagingInfo<BaoLog> p = new EFPagingInfo<BaoLog>();

            if (!BaoLog.Pg.IsNullOrEmpty()) { p.PageIndex = BaoLog.Pg; }
            if (!BaoLog.Pgs.IsNullOrEmpty()) { p.PageSize = BaoLog.Pgs; }
            if (!BaoLog.LType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.LType == BaoLog.LType); }

            p.SqlWhere.Add(f => f.UId == Users.Id);
            p.SqlWhere.Add(f => f.LType == 1 || f.LType == 2 || f.LType == 3);
            p.OrderByList.Add("Id", "DESC");

            IPageOfItems<BaoLog> List = Entity.Selects<BaoLog>(p);

            foreach (var P in List) {
                if (P.LType == 1) {
                    P.LTypeName = "转入";
                }
                if (P.LType == 2)
                {
                    P.LTypeName = "转出";
                }
                if (P.LType == 3)
                {
                    P.LTypeName = "收益";
                }
            }

            IList<BaoLog> iList = List.ToList();

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
