using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using LokFu.Extensions;

namespace LokFu.Controllers
{
    public class PayConfigChangeController : InitController
    {
        public PayConfigChangeController()
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
                Log.Write("[PayConfigChange]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.Token.IsNullOrEmpty())
            {
                //
                DataObj.OutError("1000");
                return;
            }

            Users = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
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

            string CashName = "PayConfigChange_" + Users.Agent + "_" + Users.Id;
            if (HasCache)
            {
                string StringJson = CacheBuilder.EntityCache.Get(CashName, null) as string;
                if (!StringJson.IsNullOrEmpty())
                {
                    DataObj.Data = StringJson;
                    DataObj.Code = "0000";
                    DataObj.OutString();
                    return;
                }
            }

            //获取用户所属代理商
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(x => x.Id == Users.Agent);
            int ShowType = 0;
            int AgentId = 0;
            if (SysAgent != null)
            {
                SysAgent TopAgent = SysAgentExtensions.GetTopAgent(SysAgent, Entity);
                ShowType = TopAgent.AgentState.HasValue ? TopAgent.AgentState.Value : 0;
                AgentId = TopAgent.Id;
            }
            //获取顶级代理商
            IList<PayConfigChange> List = null;
            switch (ShowType)
            {
                case 0: //显示好付
                    List = Entity.PayConfigChange.Where(n => n.AgentId == 0 && n.State == 1).ToList();
                    break;
                case 1: //不显示
                    List = new List<PayConfigChange>();
                    break;
                case 2: //显示代理商
                    List = Entity.PayConfigChange.Where(n => n.AgentId == AgentId && n.State == 1).ToList();
                    break;
            }
            StringBuilder sb = new StringBuilder("");
            sb.Append("[");
            int i = 0;
            foreach (var p in List)
            {
                //处理数据
                IList<PayConfigTemp> list = Entity.PayConfigTemp.Where(n => n.PCCId == p.Id && n.State == 1).ToList();
                bool isAct = false;//是否可升级，默认不能升级
                foreach (var pp in list)
                {
                    pp.Cols = "PId,Name,Cost";
                    PayConfig PC = Entity.PayConfig.FirstOrNew(n => n.Id == pp.PId && n.State == 1);
                    pp.Name = PC.Name;
                    UserPay UP = Entity.UserPay.FirstOrNew(n => n.UId == Users.Id && n.PId == pp.PId);
                    if (pp.Cost < UP.Cost)
                    {//只要有一个比原配置低就可升级
                        isAct = true;
                    }
                }
                if (p.CState == 1)
                {//本套餐可升级提现费率
                    if (p.Cash0 < Users.Cash0)//用户可升级提现费率
                    {
                        isAct = true;
                    }
                    if (p.ECash0 < Users.ECash0)//用户可升级提现服务费
                    {
                        isAct = true;
                    }

                }
                else
                {
                    p.Cash0 = -1;
                    p.ECash0 = -1;
                }
                if (p.EState == 1)
                {
                    //本套餐可升级提现服务费
                    if (p.Cash1 < Users.Cash1)//用户可升级提现费率
                    {
                        isAct = true;
                    }
                    if (p.ECash1 < Users.ECash1)//用户可升级提现服务费
                    {
                        isAct = true;
                    }
                }
                else
                {
                    p.Cash1 = -1;
                    p.ECash1 = -1;
                }
                if (!isAct)
                {
                    p.State = 0;
                }
                //处理数据End
                if (p.State == 1)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append("{");
                    sb.Append(p.ToStr());
                    sb.Append(",");
                    sb.Append(list.EntityToString());
                    sb.Append("}");
                    i++;
                }
            }
            sb.Append("]");

            string data = sb.ToString();
            if (HasCache)
            {
                CacheBuilder.EntityCache.Remove(CashName, null);
                CacheBuilder.EntityCache.Add(CashName, data, DateTime.Now.AddMinutes(6), null);
            }
            DataObj.Data = data;

            DataObj.Code = "0000";
            DataObj.OutString();
            //Tools.OutString(ErrInfo.Return("0000"));
        }
    }
}
