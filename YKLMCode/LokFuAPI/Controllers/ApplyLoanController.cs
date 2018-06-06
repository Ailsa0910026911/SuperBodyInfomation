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
    public class ApplyLoanController : InitController
    {
        public ApplyLoanController()
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
                Log.Write("[ApplyLoan]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            ApplyLoan ApplyLoan = new ApplyLoan();
            ApplyLoan = JsonToObject.ConvertJsonToModel(ApplyLoan, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == ApplyLoan.Token);
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

            EFPagingInfo<ApplyLoan> p = new EFPagingInfo<ApplyLoan>();
            if (!ApplyLoan.Pg.IsNullOrEmpty()) { p.PageIndex = ApplyLoan.Pg; }
            if (!ApplyLoan.Pgs.IsNullOrEmpty()) { p.PageSize = ApplyLoan.Pgs; }

            p.SqlWhere.Add(f => f.UId == baseUsers.Id);
            p.SqlWhere.Add(f => f.State > 0);

            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyLoan> List = Entity.Selects<ApplyLoan>(p);


            IList<ApplyLoan> iList = List.ToList();
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
