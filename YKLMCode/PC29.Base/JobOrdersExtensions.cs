using LokFu;
using LokFu.Extensions;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PC29.Base
{
    public static partial class JobOrdersExtensions
    {
        /// <summary>
        /// 分润
        /// </summary>
        /// <param name="O"></param>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public static JobOrders PayAgent(this JobOrders JobOrders, LokFuEntity Entity)
        {
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            if (JobOrders.AgentId.IsNullOrEmpty())
            {//代理商没有情况下
                return JobOrders;
            }
            //if (JobOrders.AgentGet.IsNullOrEmpty())
            //{//没有佣金
            //    return JobOrders;
            //}
            Users OrderUser = Entity.Users.FirstOrDefault(n => n.Id == JobOrders.UId);//读取订单用户
            #region 结算
            if (JobOrders.AgentState != 0)
            {//已结算，不能重复结算
                return JobOrders;
            }
            JobOrders.AgentState = 1;
            Entity.SaveChanges();
            //获取各级代理商
            SysAgent SysAgent = new SysAgent();
            SysAgent.Id = JobOrders.AgentId;
            IList<SysAgent> SysAgentList = SysAgent.GetAgentsById(Entity);
            decimal AIdPayGet = 0; //总佣金
            decimal AgengtGet = 0;//分润
            decimal Amount = JobOrders.TotalMoney - JobOrders.Poundage;
            int tier = 1;
            foreach (var p in SysAgentList)
            {
                if (p.State == 1)
                {
                    decimal PayGet = SysAgent.GetJobSplit(p.Tier, Entity);
                    AIdPayGet = Amount * PayGet;//当前级总佣金，需获取是否有下级拆分他的佣金
                    AIdPayGet = AIdPayGet.Floor();

                    decimal AIdPayGetNext = 0;//定义下一级佣金
                    if (tier < SysAgentList.Count)
                    {
                        //不是最后一级，需计算下级拆分金额
                        SysAgent SysAgentNext = SysAgentList.Skip(tier).Take(1).FirstOrDefault();
                        if (SysAgentNext != null)
                        {
                            if (SysAgentNext.State == 1)
                            {//下级状态有效时才计算，如关闭了则不正计算下级
                                decimal PayGetNext = SysAgent.GetJobSplit(SysAgentNext.Tier, Entity);
                                AIdPayGetNext = Amount * PayGetNext;
                                AIdPayGetNext = AIdPayGetNext.Floor();
                            }
                        }
                    }
                    decimal AIdPayGetMy = AIdPayGet - AIdPayGetNext;//当前级所能得到真实佣金

                    decimal UsersGetAll = 0;//定义所有用户佣金
                    //最后一级代理商 处理用户分润
                    #region 最后一级代理商 处理用户分润
                    if (tier == SysAgentList.Count)
                    {
                        
                        int MaxLevel = SysSet.GlobaPromoteMaxLevel;
                        //有用户分润，开始执行用户分润
                        Users Users = new Users();
                        Users.Id = JobOrders.UId;
                        //获取用户各级关系，最大级不超过用户配置级数。返回数据包含当前用户，当前用户级数标识Tier为0
                        IList<Users> UsersList = Users.GetUsersById(Entity, MaxLevel);
                        int UsersTier = 1;
                        foreach (var U in UsersList.Where(n => n.Tier > 0 && n.State == 1))
                        {
                            //UsersGetAll
                            //UserPromoteGet UserPromoteGet = UserPromoteGetList.FirstOrDefault(n => n.PromoteLevel == U.Tier);
                            //if (UserPromoteGet != null)
                            //{
                            decimal PromoteGet = Users.GetUsersJobSplit(Entity, U.Tier);
                            decimal UsersGet = Amount * PromoteGet;
                            UsersGet = UsersGet.Floor();
                            if (UsersTier == 1)
                            {
                                UsersGetAll = UsersGet;
                            }
                            //UsersGetSum += UsersGet;

                            if (UsersTier < UsersList.Count)
                            {
                                //不是最后一级，需计算下级拆分金额
                                byte nexttier = (byte)(U.Tier + 1);
                                Users UsersNext = UsersList.FirstOrDefault(o => o.Tier == nexttier);
                                if (UsersNext != null)
                                {
                                    if (UsersNext.State == 1)
                                    {//下级状态有效时才计算，如关闭了则不正计算下级
                                        decimal PayGetNext = Users.GetUsersJobSplit(Entity, UsersNext.Tier);
                                        decimal UserPayGetNext = Amount * PayGetNext;
                                        UserPayGetNext = UserPayGetNext.Floor();
                                        //减掉下级用户的金额
                                        UsersGet = UsersGet - UserPayGetNext;
                                    }
                                }
                            }
                            if (UsersGet > 0)
                            {
                                //帐户变动记录
                                string Remark = string.Format("{0}[{1}]", "卡管家分润", JobOrders.TNum);
                                string SP_Ret = Entity.SP_UsersMoney(U.Id, JobOrders.TNum, UsersGet, 8, Remark, 0);
                                if (SP_Ret != "3")
                                {
                                    Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", U.Id, JobOrders.TNum, 8, UsersGet, SP_Ret), "SP_UsersMoney");
                                }
                                //记录清分记录
                                OrderProfitLog OPL = new OrderProfitLog();
                                OPL.AddTime = DateTime.Now;
                                OPL.UId = U.Id;
                                OPL.Agent = 0;
                                OPL.TNum = JobOrders.TNum;
                                OPL.LogType = 1;
                                OPL.Tier = U.Tier;
                                OPL.Profit = UsersGet;
                                OPL.Amoney = Amount;
                                OPL.OrderType = 31;
                                OPL.UserName = OrderUser.UserName;
                                Entity.OrderProfitLog.AddObject(OPL);
                                //=====增加统计记录=====
                                //ShareTotal ShareTotal = Entity.ShareTotal.FirstOrDefault(n => n.UId == U.Id && n.Tier == U.Tier);
                                //if (ShareTotal == null)
                                //{
                                //    ShareTotal = new ShareTotal();
                                //    ShareTotal.UId = U.Id;
                                //    ShareTotal.AddTime = DateTime.Now;
                                //    ShareTotal.ShareNum = 0;
                                //    ShareTotal.Amount = JobOrders.Amount;
                                //    ShareTotal.Profit = UsersGet;
                                //    ShareTotal.Tier = U.Tier;
                                //    Entity.ShareTotal.AddObject(ShareTotal);
                                //}
                                //else
                                //{
                                //    ShareTotal.Amount += JobOrders.Amount;
                                //    ShareTotal.Profit += UsersGet;
                                //}
                                AgengtGet = AgengtGet + UsersGet;
                            }
                            UsersTier++;
                            // }
                        }
                    }
                    #endregion
                    //20160704 Lin 增加统计用户拆分多少，最后一级代理金额减掉这部分金额即可。
                    //解决用户分剩下钱规系统逻辑问题
                    AIdPayGetMy = AIdPayGetMy - UsersGetAll;

                    #region 最后一级代理商 处理同级分润
                    if (tier == SysAgentList.Count)
                    {
                        if (!p.SameAgent.IsNullOrEmpty() && AIdPayGet > 0)
                        {
                            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
                            decimal SameMoney = AIdPayGet * SysMoneySet.SameAgent;
                            SameMoney = SameMoney.Floor();
                            SysAgent SameSysAgent = Entity.SysAgent.FirstOrNew(o => o.Id == p.SameAgent);
                            if (SameSysAgent.State == 1 && !SameSysAgent.MyUId.IsNullOrEmpty()) 
                            {
                                //帐户变动记录
                                string Remark = string.Format("{0}[{1}]", "卡管家同级分润", JobOrders.TNum);
                                string SP_Ret = Entity.SP_UsersMoney(SameSysAgent.MyUId, JobOrders.TNum, SameMoney, 8, Remark, 0);
                                if (SP_Ret != "3")
                                {
                                    Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", SameSysAgent.MyUId, JobOrders.TNum, 8, SameMoney, SP_Ret), "SP_UsersMoney");
                                }
                                //记录清分记录
                                OrderProfitLog OPL = new OrderProfitLog();
                                OPL.AddTime = DateTime.Now;
                                OPL.UId = SameSysAgent.MyUId;
                                OPL.Agent = SameSysAgent.Id;
                                OPL.TNum = JobOrders.TNum;
                                OPL.LogType = 3;
                                OPL.Tier = p.Tier;
                                OPL.Profit = SameMoney;
                                OPL.Amoney = Amount;
                                OPL.OrderType = 31;
                                OPL.UserName = OrderUser.UserName;
                                Entity.OrderProfitLog.AddObject(OPL);
                                JobOrders.SameGet = SameMoney;
                            }
                        }
                    }
                    #endregion

                    if (!p.MyUId.IsNullOrEmpty() && AIdPayGetMy > 0)//某一级未绑定钱包，钱留给系统^-^
                    {
                        //获取钱包信息
                        //为了减少系统开销，这里不读取用户信息——By Lin
                        //Users Users = Entity.Users.FirstOrDefault(n => n.Id == p.MyUId);
                        if (p.MyUId > 0)
                        {
                            //帐户变动记录
                            string Remark = string.Format("{0}[{1}]", "卡管家分润", JobOrders.TNum);
                            string SP_Ret = Entity.SP_UsersMoney(p.MyUId, JobOrders.TNum, AIdPayGetMy, 8, Remark, 0);
                            if (SP_Ret != "3")
                            {
                                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", p.MyUId, JobOrders.TNum, 8, AIdPayGetMy, SP_Ret), "SP_UsersMoney");
                            }
                            //记录清分记录
                            OrderProfitLog OPL = new OrderProfitLog();
                            OPL.AddTime = DateTime.Now;
                            OPL.UId = p.MyUId;
                            OPL.Agent = p.Id;
                            OPL.TNum = JobOrders.TNum;
                            OPL.LogType = 2;
                            OPL.Tier = p.Tier;
                            OPL.Profit = AIdPayGetMy;
                            OPL.Amoney = Amount;
                            OPL.OrderType = 31;
                            OPL.UserName = OrderUser.UserName;
                            Entity.OrderProfitLog.AddObject(OPL);
                            AgengtGet = AgengtGet + AIdPayGetMy;
                        }
                    }
                }
                tier++;
            }

            Entity.SaveChanges();
            //统计所有分润
            //decimal Profit = 0;
            //if (Entity.OrderProfitLog.Count(n => n.TNum == JobOrders.TNum) > 0)
            //{
            //    Profit = Entity.OrderProfitLog.Where(n => n.TNum == JobOrders.TNum).Sum(n => n.Profit);//是否会为空的情况
            //}
            JobOrders.AgentGet = AgengtGet;//记录总佣金支出，以便总系统计算利润
            decimal RunGet = Entity.JobItem.Where(o => o.TNum == JobOrders.TNum && o.RunType == 1).Sum(o => o.RunGet);
            JobOrders.HFGet = JobOrders.Poundage - AgengtGet - RunGet - JobOrders.CashGet;
            JobOrders.AgentTime = DateTime.Now;
            Entity.SaveChanges();
            #endregion
            return JobOrders;
        }

    }
}
