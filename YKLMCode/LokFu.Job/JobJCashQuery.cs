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
    /// 执行任务/每10分钟执行一次
    /// </summary>
    public class JobJCashQuery : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "JCashQuery";
            string CanRun = ConfigurationManager.AppSettings["Run" + JobName].ToString();
            if (CanRun == "true")
            {
                if (!IsRun)
                {
                    //0取消 1待付款 2待执行 3执行中 4执行完成 5执行失败 6暂停（预留）
                    //状态：0取消 1待执行 2执行中 3执行完成 4执行失败
                    LokFuEntity Entity = new LokFuEntity();
                    IsRun = true;
                    try
                    {
                        Utils.WriteLog("执行付款任务开始执行！", JobName);
                        DateTime ETime = DateTime.Now.AddMinutes(-1);
                        DateTime STime = DateTime.Now.AddDays(-1);
                        IList<JobItem> JobItemList = Entity.JobItem.Where(n => n.State == 2 && n.RunedTime > STime && n.RunedTime <= ETime && n.RunType == 2 && n.RunState == 2).ToList();//获取10分钟前的未明状态订单
                        foreach (var p in JobItemList)
                        {
                            p.CashQuery(Entity);
                            Utils.WriteLog("处理任务[" + p.RunNum + "]！", JobName);
                        }
                        Utils.WriteLog("执行付款任务执行结束！[共计" + JobItemList.Count + "条]", JobName);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("执行付款任务执行过程出错！", Ex);
                    }
                    IsRun = false;
                }
            }
        }
    }
}
