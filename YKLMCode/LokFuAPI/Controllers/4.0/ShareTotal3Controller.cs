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
using System.Data.Objects;

namespace LokFu.Controllers
{
    public class ShareTotal3Controller : InitController
    {
        public ShareTotal3Controller()
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
                Log.Write("[ShareTotal3]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.Token.IsNullOrEmpty())
            {
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
            //if (Users.MiBao != 1)//未设置支付密码
            //{
            //    DataObj.OutError("2008");
            //    return;
            //}

            StringBuilder allsb = new StringBuilder("");
            allsb.Append("{ ");
            #region 分润统计
            DateTime tdate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            DateTime ldate = tdate.AddDays(-1);
            decimal Today = Entity.OrderProfitLog.Where(n => n.UId == Users.Id && n.LogType == 1 && n.AddTime > tdate).Sum(n => (decimal?)n.Profit) ?? 0m;
            decimal Yesterday = Entity.OrderProfitLog.Where(n => n.UId == Users.Id && n.LogType == 1 && n.AddTime > ldate && n.AddTime < tdate).Sum(n => (decimal?)n.Profit) ?? 0m;
            decimal Amount = Entity.ShareTotal.Where(n => n.UId == Users.Id).Sum(o => (decimal?)o.Profit) ?? 0m;
            allsb.Append("\"today\":\"" + Today.ToString("f2") + "\",\"yesterday\":\"" + Yesterday.ToString("f2") + "\",\"total\":\"" + Amount.ToString("f2") + "\",\"list\":");

            #endregion

            string CashName = "PayConfigChange_" + Users.Agent + "_" + Users.Id;
            if (HasCache)
            {
                string StringJson = CacheBuilder.EntityCache.Get(CashName, null) as string;
                if (!StringJson.IsNullOrEmpty())
                {
                    allsb.Append(StringJson + "}");
                    DataObj.Data = allsb.ToString();
                    DataObj.Code = "0000";
                    DataObj.OutString();
                    return;
                }
            }

            #region 降费套餐
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
                List<PayConfigTemp> list = Entity.PayConfigTemp.Where(n => n.PCCId == p.Id && n.State == 1).ToList();
                bool isAct = false;//是否可升级，默认不能升级
                var PCTDelIds = new List<int>();
                foreach (var pp in list)
                {
                    pp.Cols = "PId,Name,Cost,GroupType,Poundage";
                    PayConfig PC = Entity.PayConfig.FirstOrDefault(n => n.Id == pp.PId && n.State == 1);
                    if (PC != null)
                    {
                        pp.Name = PC.Name;
                        pp.GroupType = PC.GroupType;
                        pp.Poundage = 0;
                        UserPay UP = Entity.UserPay.FirstOrNew(n => n.UId == Users.Id && n.PId == pp.PId);
                        if (pp.Cost < UP.Cost)
                        {//只要有一个比原配置低就可升级
                            isAct = true;
                        }
                        else
                        {
                            PCTDelIds.Add(pp.Id);
                        }
                    }
                    else
                    {
                        PCTDelIds.Add(pp.Id);
                    }
                }
                if (PCTDelIds.Count>0)
                {
                    list.RemoveAll(o => PCTDelIds.Contains(o.Id));
                }
                

                if (p.CState == 1)
                {//本套餐可升级提现费率
                    //用户可升级提现费率或提现服务费
                    if (p.Cash0 < Users.Cash0 || p.ECash0 < Users.ECash0)
                    {
                        var T0Cash = new PayConfigTemp()
                        {
                            Poundage = p.ECash0,
                            Cost = (double)p.Cash0,
                            GroupType = "Cash",
                            Name = "T0提现",
                            PId = 0,
                        };
                        T0Cash.Cols = "PId,Name,Cost,GroupType,Poundage";
                        list.Add(T0Cash);
                        isAct = true;
                    }
                    //var T0Cash = new PayConfigTemp()
                    //{
                    //    Poundage = p.ECash0,
                    //    Cost = (double)p.Cash0,
                    //    GroupType = "Cash",
                    //    Name = "T0提现",
                    //    PId = 0,
                    //};
                    //T0Cash.Cols = "PId,Name,Cost,GroupType,Poundage";
                    //list.Add(T0Cash);
                    //isAct = true;
                }

                if (p.EState == 1)
                {//本套餐可升级提现费率
                    //用户可升级提现费率或提现服务费
                    if (p.Cash1 < Users.Cash1 || p.ECash1 < Users.ECash1)//用户可升级提现费率
                    {
                        var T1Cash = new PayConfigTemp()
                        {
                            Poundage = p.ECash1,
                            Cost = (double)p.Cash1,
                            GroupType = "Cash",
                            Name = "T1提现",
                            PId = 0,
                        };
                        T1Cash.Cols = "PId,Name,Cost,GroupType,Poundage";
                        list.Add(T1Cash);
                        isAct = true;
                    }

                    //var T1Cash = new PayConfigTemp()
                    //{
                    //    Poundage = p.ECash1,
                    //    Cost = (double)p.Cash1,
                    //    GroupType = "Cash",
                    //    Name = "T1提现",
                    //    PId = 0,
                    //};
                    //T1Cash.Cols = "PId,Name,Cost,GroupType,Poundage";
                    //list.Add(T1Cash);
                    //isAct = true;
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

            allsb.Append(sb.ToString() + "}");
            #endregion                                                     
             
            DataObj.Data = allsb.ToString();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
