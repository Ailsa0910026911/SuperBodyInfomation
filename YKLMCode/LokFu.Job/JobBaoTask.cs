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
    /// 余额计息服务
    /// </summary>
    public class JobBaoTask : IJob
    {
        public static bool IsRun = false;
        public void Execute(IJobExecutionContext context)
        {
            string JobName = "BaoTask";
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
                        BaoConfig BaoConfig = Entity.BaoConfig.FirstOrDefault();
                        //余额理财昨天收益归0
                        //=============================================================================================
                        Entity.ExecuteStoreCommand("Update BaoUsers Set LastRec=0 Where LastRec>0");
                        //余额理财计息程序
                        //=============================================================================================
                        Log.WriteLog("余额理财任务开始执行！", JobName);
                        DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));//今天0点
                        DateTime EDate = Today.AddDays(1);//今天24点

                        int BaoTime = Int32.Parse(ConfigurationManager.AppSettings["BaoTime"].ToString());

                        DateTime sTime = Today.AddHours(BaoTime - 24);//计息前节点
                        DateTime eTime = Today.AddHours(BaoTime);//计息后节点

                        BaoStory BaoStory = Entity.BaoStory.FirstOrDefault(n => n.SDate == Today && n.LType == 1);
                        if (BaoStory == null)
                        {
                            //添加总日志，后期可形成曲线图
                            BaoStory = new BaoStory();
                            BaoStory.SDate = Today;
                            BaoStory.GetCost = BaoConfig.GetCost;
                            BaoStory.YearPer = BaoConfig.YearPer;

                            BaoStory.InMoney = 0;
                            BaoStory.OutMoney = 0;
                            BaoStory.BfAllMoney = 0;
                            BaoStory.BfActMoney = 0;
                            BaoStory.BfInMoney = 0;
                            BaoStory.LType = 1;
                            try
                            {
                                BaoStory.InMoney = Entity.BaoLog.Where(n => n.AddTime > sTime && n.AddTime <= eTime && n.LType == 1).Sum(n => n.Amount);
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额理财统计转入出错！", JobName);
                            }
                            try
                            {
                                BaoStory.OutMoney = Entity.BaoLog.Where(n => n.AddTime > sTime && n.AddTime <= eTime && n.LType == 2).Sum(n => n.Amount);
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额理财统计转出金额出错！", JobName);
                            }
                            try
                            {
                                BaoStory.BfAllMoney = Entity.BaoUsers.Sum(n => n.AllMoney);
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额理财统计总金额出错！", JobName);
                            }
                            try
                            {
                                BaoStory.BfActMoney = Entity.BaoUsers.Sum(n => n.ActMoney);
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额理财统计计息金额出错！", JobName);
                            }
                            try
                            {
                                BaoStory.BfInMoney = Entity.BaoUsers.Sum(n => n.InMoney);
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额理财统计未计息金额出错！", JobName);
                            }
                            Entity.BaoStory.AddObject(BaoStory);
                            Entity.SaveChanges();
                            IList<BaoUsers> BaoUsersList = Entity.BaoUsers.Where(n => n.InMoney > 0 || n.ActMoney > 0).ToList();
                            foreach (var p in BaoUsersList)
                            {
                                p.PayLIXI(BaoConfig, Entity);
                                Log.WriteLog("计算利息:" + p.UId, JobName);
                            }
                            try
                            {
                                BaoStory.Interest = Entity.BaoLog.Where(n => n.AddTime >= Today && n.AddTime < EDate && n.LType == 3).Sum(n => n.Amount);//利息
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额理财统计利息出错！", JobName);
                            }
                            BaoStory.AfAllMoney = BaoStory.BfAllMoney + BaoStory.Interest;
                            BaoStory.AfActMoney = BaoStory.BfActMoney + BaoStory.AfInMoney + BaoStory.Interest;
                            BaoStory.AfInMoney = 0;
                            Entity.SaveChanges();
                            Log.WriteLog("余额理财计息完成[" + BaoUsersList.Count + "]！", JobName);
                        }
                        Log.WriteLog("余额理财任务执行结束！", JobName);
                        //=============================================================================================
                        //自动转入余额理财
                        Log.WriteLog("自动转入余额理财任务开始执行！", JobName);
                        IList<Users> UsersList = Entity.Users.Where(n => n.State == 1 && n.AutoBao == 1 && n.Amount > 0 && (n.StopPayState == 0 || n.StopPayState == 1)).ToList();
                        foreach (var p in UsersList)
                        {
                            p.AutoBao(Entity);
                            Log.WriteLog("自动转入:" + p.Id, JobName);
                        }
                        Log.WriteLog("自动转入余额理财任务执行结束[" + UsersList.Count + "]！", JobName);
                        //=============================================================================================
                        Log.WriteLog("余额生息任务开始执行！", JobName);
                        BaoStory = Entity.BaoStory.FirstOrDefault(n => n.SDate == Today && n.LType == 2);
                        if (BaoStory == null)
                        {
                            //添加总日志，后期可形成曲线图
                            BaoStory = new BaoStory();
                            BaoStory.SDate = Today;
                            BaoStory.GetCost = BaoConfig.GetCost;
                            BaoStory.YearPer = BaoConfig.YearPer;

                            BaoStory.InMoney = 0;
                            BaoStory.OutMoney = 0;
                            BaoStory.BfAllMoney = 0;
                            BaoStory.BfActMoney = 0;
                            BaoStory.BfInMoney = 0;
                            BaoStory.LType = 2;
                            try
                            {
                                BaoStory.BfAllMoney = Entity.Users.Sum(n => n.Amount);
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额生息统计总金额出错！", JobName);
                            }
                            try
                            {
                                BaoStory.BfActMoney = Entity.Users.Sum(n => n.YAmount);
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额生息统计计息金额出错！", JobName);
                            }
                            Entity.BaoStory.AddObject(BaoStory);
                            Entity.SaveChanges();

                            decimal GetCost = BaoConfig.GetCost / 10000;
                            Entity.ExecuteStoreCommand("Exec SP_UsersProfit " + GetCost);

                            try
                            {
                                BaoStory.Interest = Entity.UserLog.Where(n => n.AddTime >= Today && n.AddTime < EDate && n.OType == 15).Sum(n => n.Amount);//利息
                            }
                            catch (Exception)
                            {
                                Log.WriteLog("余额生息统计利息出错！", JobName);
                            }
                            BaoStory.AfAllMoney = BaoStory.BfAllMoney + BaoStory.Interest;
                            BaoStory.AfActMoney = BaoStory.BfActMoney + BaoStory.AfInMoney + BaoStory.Interest;
                            BaoStory.AfInMoney = 0;
                            Entity.SaveChanges();
                        }
                        Log.WriteLog("余额生息任务执行结束！", JobName);
                        //=============================================================================================
                        #endregion
                        //-------------------------------------------------------
                        Log.Write(JobName + "任务执行结束！");
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
