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
    public class ApplyLoanAddController : InitController
    {
        public ApplyLoanAddController()
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
                Log.Write("[ApplyLoanAdd]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            ApplyLoan ApplyLoan = new ApplyLoan();
            ApplyLoan = JsonToObject.ConvertJsonToModel(ApplyLoan, json);

            if (ApplyLoan.Token.IsNullOrEmpty()||ApplyLoan.Amount.IsNullOrEmpty()||ApplyLoan.TrueName.IsNullOrEmpty()){
                DataObj.OutError("1000");
                return;
            }

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

            ApplyLoan.UId = baseUsers.Id;
            ApplyLoan.AId = 0;
            ApplyLoan.State = 1;
            ApplyLoan.AddTime = DateTime.Now;

            ApplyLoan.AgentId = baseUsers.Agent;
            ApplyLoan.AgentAId = baseUsers.AId;
            ApplyLoan.PayState = 0;
            ApplyLoan.AgentPay = 0;
            //这里是利润计算==========
            ApplyLoan.Amoney = ApplyLoan.GetPrice(Entity);//获取价格
            ApplyLoan.AIdMoney = ApplyLoan.Amoney;//总利润
            ApplyLoan.AgentMoney = 0;//分支机构佣金设置为0，待分润计算后再写入

            Entity.ApplyLoan.AddObject(ApplyLoan);
            Entity.SaveChanges();

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
