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
    public class JobJCashRun : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "JCashRun";
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
                        DateTime Now = DateTime.Now;
                        DateTime Today = DateTime.Parse(Now.ToString("yyyy-MM-dd"));
                        IList<JobItem> JobItemList = Entity.JobItem.Where(n => n.State == 1 && n.RunTime > Today && n.RunTime <= Now && n.RunType == 2).ToList();//获取已经过期的VIP用户
                        foreach (var p in JobItemList)
                        {
                            p.State = 2;
                            p.RunedTime = DateTime.Now;
                        }
                        Entity.SaveChanges();
                        foreach (var p in JobItemList)
                        {
                            p.Cash(Entity);
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
