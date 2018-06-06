using LokFu;
using LokFu.Repositories;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace GoodPayJobs
{
    /// <summary>
    /// 批量出款推送
    /// </summary>
    public class JobOutMoneyPush : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "OutMoneyPush";
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
                        IList<TaskCashInfo> List = Entity.TaskCashInfo.Where(n => n.NState == 1).OrderBy(n => n.Id).ToList();
                        foreach (var p in List)
                        {
                            Orders O = Entity.Orders.FirstOrDefault(n => n.TNum == p.OId);
                            O.SendMsg(Entity);
                            p.NState = 2;
                            Log.WriteLog("Notice执行完毕:" + p.OId, JobName);
                            Thread.Sleep(500);
                        }
                        Entity.SaveChanges();
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
