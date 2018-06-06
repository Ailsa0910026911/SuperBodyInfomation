using LokFu;
using LokFu.Repositories;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace GoodPayJobs
{
    /// <summary>
    /// 交易分润
    /// </summary>
    public class JobOrderProfit : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "OrderProfit";
            string CanRun = ConfigurationManager.AppSettings["Run" + JobName].ToString();
            if (CanRun == "true")
            {
                if (!IsRun)
                {
                    LokFuEntity Entity = new LokFuEntity();
                    IsRun = true;
                    try
                    {
                        Log.Write(JobName + "任务开始执行！");
                        //-------------------------------------------------------
                        #region 任务主体
                        IList<Orders> List = Entity.Orders.Where(n => n.RunSplit == 1).Take(50).ToList();
                        foreach (var p in List) {
                            p.RunSplit = 2;
                        }
                        Entity.SaveChanges();
                        foreach (var O in List)
                        {
                            if (O.TType == 1)//银联卡支付
                            {
                                OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrDefault(n => n.OId == O.TNum);
                                if (OrderRecharge != null)
                                {
                                    OrderRecharge = OrderRecharge.PayAgent(Entity, 1, O.FrozenState);
                                    O.AgentPayGet = (decimal)OrderRecharge.AgentPayGet;
                                    O.AgentState = 1;
                                }
                            }
                            if (O.TType == 2)//提现
                            {
                                OrderCash OrderCash = Entity.OrderCash.FirstOrDefault(n => n.OId == O.TNum);
                                if (OrderCash != null)
                                {
                                    OrderCash = OrderCash.PayAgent(Entity, 1);
                                    O.AgentPayGet = (decimal)OrderCash.AgentCashGet;
                                }
                            }
                            if (O.TType == 3)//转帐
                            {
                                OrderTransfer OrderTransfer = Entity.OrderTransfer.FirstOrDefault(n => n.OId == O.TNum);
                                if (OrderTransfer != null)
                                {
                                    OrderTransfer = OrderTransfer.PayAgent(Entity, 1);
                                    O.AgentPayGet = (decimal)OrderTransfer.AgentPayGet;
                                }
                            }
                            if (O.TType == 5)//房租
                            {
                                OrderHouse OrderHouse = Entity.OrderHouse.FirstOrDefault(n => n.OId == O.TNum);
                                if (OrderHouse != null)
                                {
                                    OrderHouse = OrderHouse.PayAgent(Entity, 1);
                                    O.AgentPayGet = (decimal)OrderHouse.AgentPayGet;
                                   
                                }
                            }
                            if (O.TType == 6)//升级
                            {
                                VIPOrder VIPOrder = Entity.VIPOrder.FirstOrDefault(n => n.TNum == O.TNum);
                                if (VIPOrder != null)
                                {
                                    VIPOrder = VIPOrder.PayAgent(Entity, 1);
                                    O.AgentPayGet = (decimal)VIPOrder.SplitMoney;
                                    O.AgentState = 1;
                                }
                            }
                            if (O.TType == 7 || O.TType == 8 || O.TType == 9)//扫码 NFC
                            {
                                OrderF2F OrderF2F = Entity.OrderF2F.FirstOrDefault(n => n.OId == O.TNum);
                                if (OrderF2F != null)
                                {
                                    OrderF2F = OrderF2F.PayAgent(Entity, 1, O.FrozenState);
                                    O.AgentPayGet = (decimal)OrderF2F.AgentPayGet;
                                }
                            }
                            if (O.TType == 10)//代理自助开通
                            {
                                DaiLiOrder DaiLiOrder = Entity.DaiLiOrder.FirstOrDefault(n => n.OId == O.TNum);
                                if (DaiLiOrder != null)
                                {
                                    DaiLiOrder = DaiLiOrder.PayAgent(Entity, 1);
                                    O.AgentPayGet = (decimal)DaiLiOrder.AgentGet;
                                    O.AgentState = 1;
                                }
                            }
                           
                            O.RunSplit = 3;
                            Entity.SaveChanges();
                        }
                        #endregion
                        //-------------------------------------------------------
                        Log.Write(JobName + "任务执行结束！[共计" + List.Count + "条]");
                    }
                    catch (Exception Ex)
                    {
                        Log.Write(JobName + "任务执行过程出错！", Ex);
                    }
                    IsRun = false;
                }
                else {
                    Log.Write(JobName + "任务还在执行中！");
                }
            }
        }
    }
}
