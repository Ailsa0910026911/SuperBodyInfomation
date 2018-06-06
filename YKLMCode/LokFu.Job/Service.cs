using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace GoodPayJobs
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("好付钱包任务调度服务启动2");//在系统事件查看器里的应用程序事件里来源的描述  
            Log.Write("好付钱包任务调度服务启动2");
            try
            {
                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();//内存调度
                IScheduler IScheduler = schedulerFactory.GetScheduler();

                string TimeOutMoney = ConfigurationManager.AppSettings["TimeOutMoney"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job1 = JobBuilder.Create(typeof(JobOutMoney)).WithIdentity("JobOutMoney").Build();
                //创建并定义触发器的规则
                ITrigger trigger1 = TriggerBuilder.Create().WithCronSchedule(TimeOutMoney).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job1, trigger1);

                string TimeOutMoneyPush = ConfigurationManager.AppSettings["TimeOutMoneyPush"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job2 = JobBuilder.Create(typeof(JobOutMoneyPush)).WithIdentity("JobOutMoneyPush").Build();
                //创建并定义触发器的规则
                ITrigger trigger2 = TriggerBuilder.Create().WithCronSchedule(TimeOutMoneyPush).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job2, trigger2);

                string TimeBaoTask = ConfigurationManager.AppSettings["TimeBaoTask"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job3 = JobBuilder.Create(typeof(JobBaoTask)).WithIdentity("JobBaoTask").Build();
                //创建并定义触发器的规则
                ITrigger trigger3 = TriggerBuilder.Create().WithCronSchedule(TimeBaoTask).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job3, trigger3);

                string TimeAutoCash = ConfigurationManager.AppSettings["TimeAutoCash"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job4 = JobBuilder.Create(typeof(JobAutoCash)).WithIdentity("JobAutoCash").Build();
                //创建并定义触发器的规则
                ITrigger trigger4 = TriggerBuilder.Create().WithCronSchedule(TimeAutoCash).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job4, trigger4);

                string TimeTnInMoney = ConfigurationManager.AppSettings["TimeTnInMoney"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job5 = JobBuilder.Create(typeof(JobTnInMoney)).WithIdentity("JobTnInMoney").Build();
                //创建并定义触发器的规则
                ITrigger trigger5 = TriggerBuilder.Create().WithCronSchedule(TimeTnInMoney).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job5, trigger5);

                string TimeOrderProfit = ConfigurationManager.AppSettings["TimeOrderProfit"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job6 = JobBuilder.Create(typeof(JobOrderProfit)).WithIdentity("JobOrderProfit").Build();
                //创建并定义触发器的规则
                ITrigger trigger6 = TriggerBuilder.Create().WithCronSchedule(TimeOrderProfit).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job6, trigger6);

                string TimeFastCash = ConfigurationManager.AppSettings["TimeFastCash"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job7 = JobBuilder.Create(typeof(JobFastCash)).WithIdentity("JobFastCash").Build();
                //创建并定义触发器的规则
                ITrigger trigger7 = TriggerBuilder.Create().WithCronSchedule(TimeFastCash).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job7, trigger7);

                string TimeFastQuery = ConfigurationManager.AppSettings["TimeFastQuery"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job8 = JobBuilder.Create(typeof(JobFastQuery)).WithIdentity("JobFastQuery").Build();
                //创建并定义触发器的规则
                ITrigger trigger8 = TriggerBuilder.Create().WithCronSchedule(TimeFastQuery).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job8, trigger8);

                string TimeFastMer = ConfigurationManager.AppSettings["TimeFastMer"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job9 = JobBuilder.Create(typeof(JobFastMer)).WithIdentity("JobFastMer").Build();
                //创建并定义触发器的规则
                ITrigger trigger9 = TriggerBuilder.Create().WithCronSchedule(TimeFastMer).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job9, trigger9);

                string TimeFastProfit = ConfigurationManager.AppSettings["TimeFastProfit"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job10 = JobBuilder.Create(typeof(JobFastProfit)).WithIdentity("JobFastProfit").Build();
                //创建并定义触发器的规则
                ITrigger trigger10 = TriggerBuilder.Create().WithCronSchedule(TimeFastProfit).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job10, trigger10);


                string TimeJCashQuery = ConfigurationManager.AppSettings["TimeJCashQuery"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job11 = JobBuilder.Create(typeof(JobJCashQuery)).WithIdentity("JobJCashQuery").Build();
                //创建并定义触发器的规则
                ITrigger trigger11 = TriggerBuilder.Create().WithCronSchedule(TimeJCashQuery).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job11, trigger11);

                string TimeJCashRun = ConfigurationManager.AppSettings["TimeJCashRun"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job12 = JobBuilder.Create(typeof(JobJCashRun)).WithIdentity("JobJCashRun").Build();
                //创建并定义触发器的规则
                ITrigger trigger12 = TriggerBuilder.Create().WithCronSchedule(TimeJCashRun).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job12, trigger12);

                string TimeJPayQuery = ConfigurationManager.AppSettings["TimeJPayQuery"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job13 = JobBuilder.Create(typeof(JobJPayQuery)).WithIdentity("JobJPayQuery").Build();
                //创建并定义触发器的规则
                ITrigger trigger13 = TriggerBuilder.Create().WithCronSchedule(TimeJPayQuery).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job13, trigger13);

                string TimeJPayRun = ConfigurationManager.AppSettings["TimeJPayRun"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job14 = JobBuilder.Create(typeof(JobJPayRun)).WithIdentity("JobJPayRun").Build();
                //创建并定义触发器的规则
                ITrigger trigger14 = TriggerBuilder.Create().WithCronSchedule(TimeJPayRun).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job14, trigger14);

                string TimeJSplitMoney = ConfigurationManager.AppSettings["TimeJSplitMoney"].ToString();
                //创建一个Job来执行特定的任务
                IJobDetail job15 = JobBuilder.Create(typeof(JobJSplitMoney)).WithIdentity("JobJSplitMoney").Build();
                //创建并定义触发器的规则
                ITrigger trigger15 = TriggerBuilder.Create().WithCronSchedule(TimeJSplitMoney).Build();
                //将创建好的任务和触发规则加入到Quartz中
                IScheduler.ScheduleJob(job15, trigger15);


                //开始
                IScheduler.Start();

                Log.Write("任务初始化完成！");
            }
            catch (Exception ex)
            {
                Log.Write("任务初始化出错：", ex);
            }
        }

        protected override void OnStop()
        {
            Log.Write("好付钱包任务调度服务停止2");
            EventLog.WriteEntry("好付钱包任务调度服务停止2");
        }
    }
}
