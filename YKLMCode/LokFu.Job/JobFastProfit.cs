using LokFu;
using LokFu.Extensions;
using LokFu.Repositories;
using Quartz;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace GoodPayJobs
{
    /// <summary>
    /// 收付直通车——分润
    /// </summary>
    public class JobFastProfit : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "FastProfit";
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
                        FastConfig FastConfig = Entity.FastConfig.FirstOrNew();
                        DateTime eDate = DateTime.Now.AddSeconds(-10);
                        //只读取用户结算10s后的数据
                        //用户已结算成功，且结算时间小于10秒前，分润方式为自动分润，代理商未分润
                        IList<FastOrder> List = Entity.FastOrder.Where(n => n.AgentWay == 1 && n.AgentState == 0 && n.UserState == 1 && n.UserTime < eDate).ToList();
                        if (FastConfig.AgentWay == 1)
                        {
                            foreach (var p in List) {
                                p.PayAgent(Entity, 1);
                            }
                        }
                        else {
                            Log.WriteLog("当前为人工结算！", JobName);
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
