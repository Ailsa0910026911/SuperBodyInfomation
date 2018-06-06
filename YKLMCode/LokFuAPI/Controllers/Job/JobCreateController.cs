using LokFu.Extensions;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
namespace LokFu.Controllers
{
    public class JobCreateController : InitController
    {
        public JobCreateController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[JobCreate]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);
            if (UserTrack.X.IsNullOrEmpty() || UserTrack.Y.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JobSet JobSet = Entity.JobSet.FirstOrNew();//获取配置
            decimal Floated = JobSet.Floated;//浮动因子
            #region 处理验证参数
            if (json["token"] == null || json["money"] == null || json["card"] == null)
            {
                DataObj.OutError("1004");
                return;
            }
            string Token = json["token"].ToString();
            string money = json["money"].ToString();
            string card = json["card"].ToString();
            if (Token.IsNullOrEmpty() || money.IsNullOrEmpty() || card.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            string mymoney = "";
            string number = "";
            JArray dates = null;
            if (json["mymoney"] != null) {
                mymoney = json["mymoney"].ToString();
                if (mymoney.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }
            else if (json["number"] != null && json["dates"] != null)
            {
                number = json["number"].ToString();
                dates = (JArray)json["dates"];
                if (number.IsNullOrEmpty())
                {
                    DataObj.OutError("1000");
                    return;
                }
            }
            else {
                DataObj.OutError("1004");
                return;
            }
            int Number = 0, Money = 0, MyMoney = 0, Card = 0;
            try {
                Money = Int32.Parse(money);
                Card = Int32.Parse(card);
                if (!number.IsNullOrEmpty()) {
                    Number = Int32.Parse(number);
                }
                if (!mymoney.IsNullOrEmpty())
                {
                    MyMoney = Int32.Parse(mymoney);
                }
            }catch(Exception){
                DataObj.OutError("1000");
                return;
            }
            if (Money <= 0)
            {
                DataObj.Msg = "帐单金额需大于零";
                DataObj.OutError("1000");
                return;
            }
            if (MyMoney > Money) {
                DataObj.Msg = "还款金额不能大于帐单金额";
                DataObj.OutError("1000");
                return;
            }
            int Days = 0;
            DateTime[] Dates = null;
            if (dates != null) {
                Days = dates.Count;
                Dates = new DateTime[Days];
                int Count = 0;
                bool NoFaile = true;
                foreach (var p in dates)
                {
                    string date = p["date"].ToString();
                    try
                    {
                        DateTime Date = DateTime.Parse(date);
                        Dates[Count] = Date;
                    }
                    catch (Exception)
                    {
                        NoFaile = false;
                    }
                    Count++;
                }
                if (!NoFaile)
                {
                    DataObj.OutError("1000");
                    return;
                }
            }

            DateTime Now = DateTime.Now;
            DateTime Today = DateTime.Parse(Now.ToString("yyyy-MM-dd"));
            //这里启用智能计算
            if (MyMoney > 0) {
                //计算最少几天还款
                //如果按MyMoney付的话，控制不超的话，应该去掉手续费
                decimal maxMoney = (decimal)MyMoney * (1 - JobSet.Cost) - JobSet.Cash;
                Days = (int)Math.Ceiling((decimal)Money * (1 + Floated) / maxMoney);//计算还款需要几天
                if (Days > JobSet.MaxDay) {
                    Days = JobSet.MaxDay;//限制最大天数
                }
                //这里增加修正，如果笔数过多，则修正笔数
                while (Money < JobSet.MinMoney * Days && maxMoney * (Days - 1) > Money)
                {
                    Days = Days - 1;//减少一天还款
                }
                if (Days < 2) {
                    Days = 2;
                }
                Dates = new DateTime[Days];
                //这里不能直接用传进来的计划金额去计算，应该总金额-传进来金额再平均
                int everyDayMoney = (Money - MyMoney) / (Days - 1);
                int bei = (int)Math.Ceiling(everyDayMoney / JobSet.DayMoney);//超过一定金额拆分笔数
                if (bei > JobSet.MaxPay) {
                    bei = JobSet.MaxPay;//每天刷笔数过大取最大
                }
                if (bei == 0) {
                    bei = 1;
                }
                Number = (Days - 1) * bei + 1;//第一天只能刷一笔
                Dates[0] = Today;
                for (var i = 1; i < Days; i++) {
                    if (Now > Today.AddHours(23))//超过23点了
                    {
                        Dates[i] = Today.AddDays(i + 1);
                    }
                    else {
                        Dates[i] = Today.AddDays(i);
                    }
                }
            }
            if (Number <= 0)
            {
                DataObj.OutError("1000");
                return;
            }
            if (Card <= 0)
            {
                DataObj.OutError("1000");
                return;
            }
            
            if (Days <= 0) {
                DataObj.OutError("1000");
                return;
            }
            if (Days > JobSet.MaxDay)
            {
                DataObj.Msg = "最大还款天数为" + JobSet.MaxDay + "天";
                DataObj.OutError("1000");
                return;
            }
            if (Days > Number)
            {
                DataObj.Msg = "刷卡笔数必需大于等于还款天数";
                DataObj.OutError("1000");
                return;
            }
            int MaxPayNum = (Days - 1) * JobSet.MaxPay + 1;
            if (Number > MaxPayNum)
            {
                DataObj.Msg = Days + "天最多刷卡笔数为" + MaxPayNum + "笔";
                DataObj.OutError("1000");
                return;
            }
            decimal EP = (decimal)Money / (decimal)Number;
            if (EP < JobSet.MinMoney) {
                if (MyMoney > 0)
                {
                    DataObj.Msg = "经测算每天还款金额不足" + JobSet.MinMoney.ToMoney() + "元。";
                }
                else {
                    DataObj.Msg = "账单金额不能低于" + (Number * JobSet.MinMoney).ToMoney() + "元";
                }
                DataObj.OutError("1000");
                return;
            }
            if (EP > JobSet.MaxMoney)
            {
                DataObj.Msg = "账单金额不能大于" + (Number * JobSet.MaxMoney).ToMoney() + "元";
                DataObj.OutError("1000");
                return;
            }
            
            decimal EM = Money / Days;
            if (EM < JobSet.MinMoney)
            {
                DataObj.Msg = "每天最小还款金额为" + (Days * JobSet.MinMoney).ToString("f2") + "元";
                DataObj.OutError("1000");
                return;
            }
            if (EM > JobSet.MaxMoney)
            {
                DataObj.Msg = "每天最大还款金额为" + JobSet.MaxMoney.ToMoney() + "元";
                DataObj.OutError("1000");
                return;
            }
            //数据排序，避免前端大小乱排上送数据造成错误
            Array.Sort(Dates);
            if (!Dates.GroupBy(o => o).All(o => o.Count() <= 1))
            {
                DataObj.Msg = "日期不能重复";
                DataObj.OutError("1000");
                return;
            }
            if (!Dates.All(o => o >= Today))
            {
                DataObj.Msg = "还款日期已过期";
                DataObj.OutError("1000");
                return;
            }

            DateTime FirstDate = Dates.First();
            DateTime LastDate = Dates.Last();
            //if (FirstDate < Today) {
            //    DataObj.Msg = "还款日期已过期";
            //    DataObj.OutError("1000");
            //    return;
            //}
            TimeSpan TS = FirstDate.Subtract(LastDate).Duration();
            TimeSpan TSs = new TimeSpan(JobSet.EqDays, 0, 0, 0);
            if (TS > TSs)
            {
                DataObj.Msg = "还款日期跨度过大";
                DataObj.OutError("1000");
                return;
            }
            #endregion
            
            byte AdvCost = JobSet.AdvCost;//预收刷卡手续费
            byte AdvCash = JobSet.AdvCash;//预收代付手续费
            int MaxRand = JobSet.MaxRand;//最大失败次数
            //Money Number DateTime[] Dates

            #region 获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }
            if (baseUsers.StopPayState == 2)//禁止支付
            {
                DataObj.OutError("6060");
                return;
            }
            #endregion

            UserCard UserCard = Entity.UserCard.FirstOrDefault(n => n.Id == Card && n.UId == baseUsers.Id && n.Type == 2 && n.State == 1);
            if (UserCard == null)
            {
                DataObj.OutError("1000");
                return;
            }
            UserCardOpen UserCardOpen = Entity.UserCardOpen.FirstOrDefault(n => n.State == 1 && n.CardNum == UserCard.Card && n.UId == baseUsers.Id);
            if (UserCardOpen == null)
            {
                DataObj.OutError("6032");
                return;
            }
            //if (UserCardOpen.PayWay == 1) {
            //    DataObj.Msg = "您的银行卡授权已过期，请删除后重新绑卡授权。";
            //    DataObj.OutError("6032");
            //    return;
            //}
            JobPayWay JobPayWay = Entity.JobPayWay.FirstOrDefault(n => n.State == 1 && n.GroupType == "Pay" && n.Id == UserCardOpen.PayWay);//从开卡记录中取通道
            if (JobPayWay == null)
            {
                DataObj.OutError("6031");
                return;
            }
            JobPayWay JobCashWay = Entity.JobPayWay.FirstOrDefault(n => n.State == 1 && n.GroupType == "Cash" && n.DllName == JobPayWay.DllName);//匹配出一条还款通道
            if (JobCashWay == null)
            {
                DataObj.OutError("6031");
                return;
            }
            bool IsRun = Entity.JobOrders.Where(o => (o.State == 2 || o.State == 3) && o.UId == baseUsers.Id && o.UserCardId == UserCard.Id).Select(o => o.UserCardId).FirstOrDefault() != 0 ? true : false;
            if (IsRun)
            {
                DataObj.OutError("7006");
                return;
            }
            decimal Cost = JobSet.Cost;//刷卡手续费
            decimal Cash = JobSet.Cash;//还款手续费
            //if (baseUsers.IsVIP == 1) {
            //    Cost = JobSet.VIPCost;
            //    Cash = JobSet.VIPCash;
            //}
            //开始任务
            RandItems RandItems = Rand.Create(Dates, Money, Number, Floated);
            List<jobitem> JobList = RandItems.JobList;
            //这里要加上验证
            //============================================================
            
            if (AdvCost == 0 || AdvCash == 0) {
                bool CheckOk = false;
                int CTimes = 0;
                while (!CheckOk && CTimes < MaxRand)
                {
                    CheckOk = true;
                    List<jobitem> JobListCash = JobList.Where(n => n.JobType == 2).ToList();
                    jobitem first = JobListCash.FirstOrDefault();
                    List<jobitem> JobListCashT = JobListCash.Skip(1).ToList();
                    foreach (var p in JobListCashT)
                    { 
                        //只需验证还款金额
                        decimal AddMoney = 0;
                        if (AdvCost == 0) {
                            AddMoney += p.Money * Cost;
                            AddMoney = AddMoney.Ceiling();
                        }
                        if (AdvCash == 0)
                        {
                            AddMoney += Cash;
                        }
                        if (first.Money < p.Money + AddMoney) {
                            CheckOk = false;
                            break;
                        }
                        first = p;
                    }
                    //============================================================
                    if (MyMoney > 0)
                    {
                        jobitem firstpay = JobList.Where(n => n.JobType == 1).OrderByDescending(n => n.Money).First();
                        //这里检测第一笔加上手续费后大于输入金额则重新生成
                        decimal Poundage1, Poundage2;
                        if (AdvCost == 1)//预收
                        {
                            Poundage1 = Money * Cost;//预收总金额*手续费
                        }
                        else
                        {
                            Poundage1 = firstpay.Money * Cost;//任务金额*手续费
                        }
                        Poundage1 = Poundage1.Ceiling();
                        if (AdvCash == 1)//预收
                        {
                            int AllPayNum = JobList.Count(n => n.JobType == 1);
                            Poundage2 = AllPayNum * Cash;//预收总笔数*手续费
                        }
                        else
                        {
                            Poundage2 = Cash;//第一天只有一笔交易和代付
                        }
                        decimal Poundage = Poundage1 + Poundage2;
                        if (firstpay.Money + Poundage > MyMoney)
                        {
                            CheckOk = false;
                        }
                    }
                    //============================================================
                    if (!CheckOk) { 
                        //失败了重新生成
                        RandItems = Rand.Create(Dates, Money, Number, Floated);
                        JobList = RandItems.JobList;
                    }
                    CTimes++;
                };
                if (!CheckOk) {
                    DataObj.OutError("6033");
                    return;
                }
            }
            //============================================================
            //生成订单
            JobOrders JobOrders = new JobOrders();
            JobOrders.UId = baseUsers.Id;
            JobOrders.Amount=0;
            JobOrders.State = 0;//0取消 1待付款 2待执行 3执行中 4执行完成 5执行失败 6暂停（预留）
            JobOrders.AddTime = DateTime.Now;
            JobOrders.PayState = 0;
            JobOrders.PayedState = 0;
            JobOrders.PayWay = JobPayWay.Id;
            JobOrders.CashWay = JobCashWay.Id;

            JobOrders.AgentId = baseUsers.Agent;
            JobOrders.AgentState = 0;
            JobOrders.AgentGet = 0;//代理分润，等分润执行时写回

            JobOrders.PayRate = JobPayWay.Cost;
            JobOrders.PayMin = JobPayWay.MinCost;
            JobOrders.PayMax = JobPayWay.MaxCost;
            JobOrders.CashRate = JobCashWay.Cost;
            JobOrders.CashMin = JobCashWay.MinCost;
            JobOrders.CashMax = JobCashWay.MaxCost;

            JobOrders.UPayRate = Cost;
            JobOrders.UPayMin = 0;
            JobOrders.UPayMax = 999999999;
            JobOrders.UCashRate = 0;
            JobOrders.UCashMin = Cash;
            JobOrders.UCashMax = 999999999;

            JobOrders.TotalMoney = 0;//总订单金额
            JobOrders.Poundage = 0;//总手续费
            JobOrders.HFGet = 0;//总成本
            JobOrders.PayPoundage = 0;//交易手续费
            JobOrders.CashPoundage = 0;//代付手续费
            JobOrders.PayGet = 0;//交易成本
            JobOrders.CashGet = 0;//代付成本
            JobOrders.UserCardId = UserCard.Id;//计划对应的信用卡
            JobOrders.AdvCost = JobSet.AdvCost;
            JobOrders.AdvCash = JobSet.AdvCash;

            Entity.JobOrders.AddObject(JobOrders);
            Entity.SaveChanges();
            Entity.Refresh(RefreshMode.StoreWins, JobOrders);
            IList<JobItem> JobItemList = new List<JobItem>();
            //============================================================
            DateTime FirstRun = Today;
            //获取代理商分润
            //SysAgent SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
            //IList<SysAgent> SysAgentList = SysAgent.GetAgentsById(Entity);
            //SysAgent TopAgent = SysAgentList.FirstOrNew();
            //decimal PaySplit = 0;//代理商费率
            //PaySplit = SysAgent.GetSplit(TopAgent.Tier, Entity);
            foreach (var p in JobList) {
                JobItem JobItem = new JobItem();
                JobItem.TNum = JobOrders.TNum;
                JobItem.UId = JobOrders.UId;
                JobItem.RunTime = p.RunTime;
                JobItem.RunType = p.JobType;
                JobItem.State = 0;//状态：0取消 1待执行 2执行中 3执行完成 4执行失败
                JobItem.RunState = 0;
                JobItem.RunSort = 0;
                JobItem.AddTime = DateTime.Now;
                JobItem.UserCardId = UserCard.Id;
                
                DateTime NowDate = DateTime.Parse(JobItem.RunTime.ToString("yyyy-MM-dd"));
                decimal Poundage = 0, Poundage1 = 0, Poundage2 = 0;
                if (NowDate == Today)
                {
                    //第一笔
                    if (p.JobType == 1)
                    {
                        //消费
                        if (AdvCost == 1)//预收
                        {
                            Poundage1 = Money * Cost;//预收总金额*手续费
                        }
                        else
                        {
                            Poundage1 = p.Money * Cost;//任务金额*手续费
                        }
                        Poundage1 = Poundage1.Ceiling();
                        //2017-11-06收到新需求，每笔刷卡加金额，代付不收费用
                        if (AdvCash == 1)//预收
                        {
                            int AllPayNum = JobList.Count(n => n.JobType == 1);
                            Poundage2 = AllPayNum * Cash;//预收总笔数*手续费
                        }
                        else
                        {
                            Poundage2 = Cash;//第一天只有一笔交易和代付
                        }
                        Poundage = Poundage1 + Poundage2;
                        JobItem.RunMoney = p.Money + Poundage;
                        JobItem.Poundage = Poundage;
                        JobItem.PayWay = JobPayWay.Id;
                        JobItem.UserCardId = 0;
                    }
                    if (p.JobType == 2)
                    {
                        //还款
                        JobItem.RunMoney = p.Money;
                        JobItem.Poundage = 0;
                        JobItem.PayWay = JobCashWay.Id;
                    }
                }
                else {
                    if (p.JobType == 1)
                    {
                        //消费
                        if (AdvCost == 0)//实收
                        {
                            Poundage1 = p.Money * Cost;//预收总金额
                            Poundage1 = Poundage1.Ceiling();
                        }
                        if (AdvCash == 0)//实收
                        {
                            //2017-11-06收到新需求，每笔刷卡加金额，代付不收费用
                            Poundage2 = Cash;
                            //每天只有一笔代付
                            //==================当天第一笔要加，其它不需要加
                            //if (FirstRun != NowDate) {
                                //Poundage2 = Cash;
                                //FirstRun = NowDate;
                            //}
                        }
                        Poundage = Poundage1 + Poundage2;
                        JobItem.RunMoney = p.Money + Poundage;
                        JobItem.Poundage = Poundage;
                        JobItem.PayWay = JobPayWay.Id;
                    }
                    if (p.JobType == 2)
                    {
                        //还款
                        JobItem.RunMoney = p.Money;
                        JobItem.Poundage = 0;
                        JobItem.PayWay = JobCashWay.Id;
                    }
                }
                decimal AgentPoundage = JobItem.Poundage;
                if (JobItem.RunType == 1) {
                    JobItem.RunGet = p.Money * JobPayWay.Cost;
                    if (JobItem.RunGet < JobPayWay.MinCost)
                    {
                        JobItem.RunGet = JobPayWay.MinCost;
                    }
                    if (JobItem.RunGet > JobPayWay.MaxCost)
                    {
                        JobItem.RunGet = JobPayWay.MaxCost;
                    }
                    AgentPoundage = p.Money * JobPayWay.CostAgent;
                    if (AgentPoundage < JobPayWay.MinAgentCost)
                    {
                        AgentPoundage = JobPayWay.MinAgentCost;
                    }
                    if (AgentPoundage > JobPayWay.MaxAgentCost)
                    {
                        AgentPoundage = JobPayWay.MaxAgentCost;
                    }
                }
                if (JobItem.RunType == 2)
                {
                    JobItem.RunGet = p.Money * JobCashWay.Cost;
                    if (JobItem.RunGet < JobCashWay.MinCost)
                    {
                        JobItem.RunGet = JobCashWay.MinCost;
                    }
                    if (JobItem.RunGet > JobCashWay.MaxCost)
                    {
                        JobItem.RunGet = JobCashWay.MaxCost;
                    }
                    AgentPoundage = p.Money * JobCashWay.CostAgent;
                    if (AgentPoundage < JobCashWay.MinAgentCost)
                    {
                        AgentPoundage = JobCashWay.MinAgentCost;
                    }
                    if (AgentPoundage > JobCashWay.MaxAgentCost)
                    {
                        AgentPoundage = JobCashWay.MaxAgentCost;
                    }
                }
                JobItem.RunGet = JobItem.RunGet.Ceiling();//通道成本
                //AgentPoundage = AgentPoundage.Ceiling();//代理手续费

                //JobItem.AgentGet = Poundage1 - AgentPoundage;//代理分润
                JobItem.AgentGet = 0;
                JobItem.HFGet = 0;
                //利润=用户手续费-代理分润-通道成本
                //JobItem.HFGet = JobItem.Poundage - JobItem.AgentGet - JobItem.RunGet;
                Entity.JobItem.AddObject(JobItem);
                JobItemList.Add(JobItem);
            }
            decimal TotalPoundage = JobItemList.Sum(n => n.Poundage);
            decimal TotalHFGet = JobItemList.Sum(n => n.HFGet);
            decimal TotalAgentGet = JobItemList.Sum(n => n.AgentGet);
            decimal TotalPoundage1 = JobItemList.Where(n => n.RunType == 1).Sum(n => n.Poundage);
            decimal TotalPoundage2 = JobItemList.Where(n => n.RunType == 2).Sum(n => n.Poundage);
            decimal TotalRunGet1 = JobItemList.Where(n => n.RunType == 1).Sum(n => n.RunGet);
            decimal TotalRunGet2 = JobItemList.Where(n => n.RunType == 2).Sum(n => n.RunGet);

            JobOrders.TotalMoney = Money + TotalPoundage;//总订单金额
            JobOrders.Poundage = TotalPoundage;//总手续费
            JobOrders.AgentGet = TotalAgentGet;//代理分润
            JobOrders.HFGet = TotalHFGet;//利润
            JobOrders.PayPoundage = TotalPoundage1;//交易手续费
            JobOrders.CashPoundage = TotalPoundage2;//代付手续费
            JobOrders.PayGet = TotalRunGet1;//交易成本
            JobOrders.CashGet = TotalRunGet2;//代付成本
            JobOrders.State = 1;//0取消 1待付款 2待执行 3执行中 4执行完成 5执行失败 6暂停（预留）
            JobOrders.SameGet = 0;
            Entity.SaveChanges();
            JobOrders.Cols = JobOrders.Cols + ",Items,UsersCard,SetCost,SetCash,XFCount,AdvCost,AdvCash";
            JobItemList = Entity.JobItem.Where(n => n.TNum == JobOrders.TNum).OrderBy(n => n.RunTime).ToList();
            //============================================================
            int RunSort = 1;
            foreach (var p in JobItemList) {
                if (RunSort == JobList.Count)
                {
                    //最后一笔特别标识
                    p.RunSort = 999;
                }
                else
                {
                    p.RunSort = RunSort;
                }
                RunSort++;
            }
            Entity.SaveChanges();
            //============================================================
            //这里加上修正有分问题
            IList<JobItem> JobItemPay = JobItemList.Where(n => n.RunType == 1).ToList();
            JobItem FirstItem = JobItemPay.First();
            JobItem LastItem = JobItemPay.Last();
            if (JobItemPay.Count > 1)
            {
                decimal RunAu = 0;//用于存放差额
                decimal Adv = 0;//用于存放每笔调整金额
                bool CanSvae = true;//用于判断是否能修正
                foreach (var p in JobItemPay)
                {
                    if (p.Id == FirstItem.Id) { 
                        //第一笔
                        if (MyMoney > p.RunMoney) {
                            RunAu = MyMoney - p.RunMoney;
                            p.RunMoney = MyMoney;
                            Adv = (RunAu / (JobItemPay.Count - 1)).Ceiling();//后台平均一笔要增减金额
                        }
                    }else if (p.Id == LastItem.Id){
                        //最后一笔
                        p.RunMoney = p.RunMoney - RunAu;//修正差额
                        if (p.RunMoney < 0)
                        {
                            CanSvae = false;
                        }
                    }
                    else {
                        decimal RunMoney = p.RunMoney;//任务金额
                        decimal Run10 = (RunMoney - Adv) * 10;//放大十倍
                        decimal Run10U = Math.Ceiling(Run10);//向上取整个
                        decimal RunMoneyU = Run10U / 10;//缩小十倍
                        p.RunMoney = RunMoneyU;//重新赋值
                        RunAu += (RunMoneyU - RunMoney);//记录差额
                    }
                }
                if (CanSvae)
                {
                    Entity.SaveChanges();
                }
            }
            //============================================================
            UserTrack.UId = baseUsers.Id;
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "创建还款订单";
            UserTrack.GPSAddress = UserTrack.GPSAddress;
            UserTrack.GPSX = UserTrack.X;
            UserTrack.GPSY = UserTrack.Y;
            UserTrack.UserName = JobOrders.TNum;
            UserTrack.SeavGPSLog(Entity);
            //=======================================
            //最后检测整个任务是否有单笔小于1元的
            if (JobItemList.Count(n => n.RunMoney < 1) > 0) {
                DataObj.OutError("6033");
                return;
            }
            //=======================================
            //这里之后不能执行SaveChanges，否则很严重
            foreach (var p in JobItemList) {
                p.State = 1;
            }
            var JobItemModelList = JobItemList.GroupBy(o => o.RunTime.Date, (x, o) => new JobItemModel
            {
                date = x,
                item = o.OrderBy(k => k.RunType).ThenBy(n => n.RunTime).ToList().EntityToJson(),
            }).OrderBy(o => o.date).ToList();
            JobOrders.Items = JobItemModelList.EntityToJson();
            JobOrders.XFCount = JobItemList.Where(o => o.RunType == 1).Count();;
            JobOrders.SetCost = JobOrders.UPayRate;
            JobOrders.SetCash = JobOrders.UCashMin;

            string UserCardStr = UserCard.OutJson();
            JObject JS = new JObject();
            try
            {
                JS = (JObject)JsonConvert.DeserializeObject(UserCardStr);
            }
            catch (Exception Ex)
            {
                Log.Write("[JobCreate]:", "【JsStr】" + UserCardStr, Ex);
            }
            JobOrders.UsersCard = JS;

            string data = JobOrders.OutJson();
            data = data.Replace("\"[{", "[{").Replace("}]\"", "}]").Replace("\\", "").Replace("]\"}", "]}");
            DataObj.Data = data;
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
