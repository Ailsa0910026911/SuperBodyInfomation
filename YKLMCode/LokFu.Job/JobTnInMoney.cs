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
    /// Tn自动解冻
    /// </summary>
    public class JobTnInMoney : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "TnInMoney";
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
                        IList<TaskOrders> List = Entity.TaskOrders.Where(n => n.State == 1 && n.ODate <= DateTime.Now).ToList();
                        foreach (var p in List)
                        {
                            p.State = 2;
                            Entity.SaveChanges();
                            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == p.OId);
                            if (Orders != null)
                            {
                                if (Orders.FrozenState == 1)
                                {
                                    Orders.FrozenState = 0;
                                    Entity.SaveChanges();
                                    if (Orders.TType == 1)
                                    {
                                        OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrDefault(n => n.OId == Orders.TNum);
                                        if (OrderRecharge != null)
                                        {
                                            OrderRecharge.SetUnFrozen(Entity);
                                        }
                                    }
                                    if (Orders.TType == 7 || Orders.TType == 8 || Orders.TType == 9)
                                    {
                                        OrderF2F OrderF2F = Entity.OrderF2F.FirstOrDefault(n => n.OId == Orders.TNum);
                                        if (OrderF2F != null)
                                        {
                                            OrderF2F.SetUnFrozen(Entity);
                                        }
                                    }
                                }
                                Orders.InState = 1;
                                Orders.TState = 2;
                                Orders.InTimed = DateTime.Now;
                                Entity.SaveChanges();
                            }
                            Log.WriteLog("执行入帐:" + p.OId, JobName);
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
