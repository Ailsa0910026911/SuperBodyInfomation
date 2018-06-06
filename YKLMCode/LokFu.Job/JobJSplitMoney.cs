using LokFu;
using LokFu.Extensions;
using LokFu.Repositories;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace GoodPayJobs
{
    /// <summary>
    /// 执行任务/每天8点到20点每1分钟执行一次
    /// </summary>
    public class JobJSplitMoney : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "JSplitMoney";
            string CanRun = ConfigurationManager.AppSettings["Run" + JobName].ToString();
            if (CanRun == "true")
            {
                if (!IsRun)
                {
                    //0取消 1待付款 2待执行 3执行中 4执行完成 5执行失败 6暂停（预留）
                    LokFuEntity Entity = new LokFuEntity();
                    IsRun = true;
                    try
                    {
                        Utils.WriteLog("执行分润任务开始执行！", JobName);
                        DateTime Now = DateTime.Now.AddMinutes(-10);
                        DateTime Today = DateTime.Parse(Now.ToString("yyyy-MM-dd"));
                        IList<JobOrders> JobOrdersList = Entity.JobOrders.Where(n => n.PayedState == 1 && n.PayedTime > Today && n.PayedTime <= Now && n.AgentState == 0).ToList();//获取已经过期的VIP用户
                        foreach (var p in JobOrdersList)
                        {
                            p.PayAgent(Entity);
                            Utils.WriteLog("处理分润[" + p.TNum + "]！", JobName);
                        }
                        Utils.WriteLog("执行分润任务执行结束！[共计" + JobOrdersList.Count + "条]", JobName);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("执行分润任务执行过程出错！", Ex);
                    }
                    IsRun = false;
                }
            }
        }
    }
}
