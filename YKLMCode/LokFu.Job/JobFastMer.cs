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
    /// 收付直通车——商户查询
    /// </summary>
    public class JobFastMer : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "FastMer";
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
                        DateTime STime = DateTime.Now.AddDays(-2);
                        DateTime ETime = DateTime.Now.AddSeconds(-30);
                        IList<FastUserPay> List = Entity.FastUserPay.Where(n => n.MerState == 3 && n.CardState == 3 && n.BusiState == 3 && n.AddTime > STime && n.AddTime < ETime).ToList();
                        foreach (var p in List)
                        {
                            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == p.PayWay);
                            string[] PayConfigArr = FastPayWay.QueryArray.Split(',');
                            if (FastPayWay.DllName == "HFJSPay")
                            {
                                #region 结算系统
                                //不需要

                                #endregion
                            }
                            Log.WriteLog("查询商户[" + p.MerId + "]！", JobName);
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
