using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using LokFu.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LokFu.Controllers
{
    public class ApplyCreditController : InitController
    {
        public ApplyCreditController()
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
                Log.Write("[ApplyCredit]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            ApplyCredit ApplyCredit = new ApplyCredit();
            ApplyCredit = JsonToObject.ConvertJsonToModel(ApplyCredit, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == ApplyCredit.Token);
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

            EFPagingInfo<ApplyCredit> p = new EFPagingInfo<ApplyCredit>();
            if (!ApplyCredit.Pg.IsNullOrEmpty()) { p.PageIndex = ApplyCredit.Pg; }
            if (!ApplyCredit.Pgs.IsNullOrEmpty()) { p.PageSize = ApplyCredit.Pgs; }

            p.SqlWhere.Add(f => f.UId == baseUsers.Id);
            p.SqlWhere.Add(f => f.State > 0);

            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCredit> List = Entity.Selects<ApplyCredit>(p);
            IList<BasicBank> BBList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            foreach (var pp in List) {
                if (!pp.BankId.IsNullOrEmpty()){
                    pp.BankName = BBList.FirstOrNew(n => n.Id == pp.BankId).Name;
                }
            }
            IList<ApplyCredit> iList = List.ToList();
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
