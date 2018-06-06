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
    /// 自动T1提现
    /// </summary>
    public class JobAutoCash : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "AutoCash";
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
                        IList<Users> List = Entity.Users.Where(n => n.State == 1 && n.AutoCash == 1 && n.Amount >= n.AutoCashMoney).ToList();
                        SysSet SysSet = Entity.SysSet.FirstOrDefault();
                        SysControl SysControl = Entity.SysControl.FirstOrDefault(n => n.Tag == "Cash");
                        if (SysControl != null)
                        {
                            foreach (var p in List)
                            {
                                p.AutoCash(Entity, SysSet, SysControl);
                                Log.WriteLog("提现完成:" + p.UserName, JobName);
                            }
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
