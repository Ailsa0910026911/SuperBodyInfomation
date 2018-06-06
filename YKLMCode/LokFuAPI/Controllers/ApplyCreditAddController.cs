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
    public class ApplyCreditAddController : InitController
    {
        public ApplyCreditAddController()
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
                Log.Write("[ApplyCreditAdd]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            ApplyCredit ApplyCredit = new ApplyCredit();
            ApplyCredit = JsonToObject.ConvertJsonToModel(ApplyCredit, json);

            if (ApplyCredit.Token.IsNullOrEmpty()||ApplyCredit.BankId.IsNullOrEmpty()||ApplyCredit.TrueName.IsNullOrEmpty()){
                DataObj.OutError("1000");
                return;
            }

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

            ApplyCredit.UId = baseUsers.Id;
            ApplyCredit.AId = 0;
            ApplyCredit.State = 1;
            ApplyCredit.AddTime = DateTime.Now;
            ApplyCredit.AgentId = baseUsers.Agent;
            ApplyCredit.AgentAId = baseUsers.AId;
            ApplyCredit.PayState = 0;
            ApplyCredit.AgentPay = 0;
            //这里是利润计算==========
            ApplyCredit.Amoney = ApplyCredit.GetPrice(Entity);//获取价格
            ApplyCredit.AIdMoney = ApplyCredit.Amoney;//总利润
            ApplyCredit.AgentMoney = 0;//分支机构佣金设置为0，待分润计算后再写入

            Entity.ApplyCredit.AddObject(ApplyCredit);
            Entity.SaveChanges();

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
