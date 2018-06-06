using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;
namespace LokFu.Areas.Manage.Controllers
{
    public class JobOrdersController : BaseController
    {
        public ActionResult Index(JobOrders JobOrders, EFPagingInfo<JobOrders> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            IPageOfItems<JobOrders> JobOrdersList = null;
            if (IsFirst == 0)
            {
                JobOrders.State = 99;
                JobOrders.AgentState = 99;
                JobOrdersList = new PageOfItems<JobOrders>(new List<JobOrders>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                p = this.Condition(JobOrders, p, STime, ETime);
                JobOrdersList = Entity.Selects<JobOrders>(p);
            }
            List<int> UId = JobOrdersList.Select(o => o.UId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.JobOrdersList = JobOrdersList;
            ViewBag.JobOrders = JobOrders;
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.ETime = ETime;
            ViewBag.STime = STime;
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            ViewBag.SplitMoney = this.checkPower("SplitMoney");
            ViewBag.JobPayWayList = Entity.JobPayWay.ToList();
            return View();
        }
        public ActionResult Edit(JobOrders JobOrders)
        {
            if (JobOrders.Id != 0) { JobOrders = Entity.JobOrders.FirstOrDefault(n => n.Id == JobOrders.Id); }
            else if (!JobOrders.TNum.IsNullOrEmpty()) { JobOrders = Entity.JobOrders.FirstOrDefault(n => n.TNum == JobOrders.TNum); }
            if (JobOrders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = Entity.Users.FirstOrDefault(o => o.Id == JobOrders.UId);
            ViewBag.JobPayWay = Entity.JobPayWay.FirstOrDefault(o => o.Id == JobOrders.PayWay);
            ViewBag.JobOrders = JobOrders;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }

        public ActionResult IndexJobOrders(string TNum)
        {
            JobOrders JobOrders = Entity.JobOrders.FirstOrDefault(n => n.TNum == TNum);
            if (JobOrders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.JobOrders = JobOrders;

            var JobItemList = this.Entity.JobItem.Where(o => o.TNum == TNum).OrderBy(o => o.RunTime).ToList();
            ViewBag.PaywayList = Entity.JobPayWay.ToList();
            ViewBag.UsersCardList = Entity.UserCard.Where(o => o.UId == JobOrders.UId).ToList();
            ViewBag.JobItemList = JobItemList;
            ViewBag.CloseJob = this.checkPower("CloseJob");
            ViewBag.RetSetItem = this.checkPower("RetSetItem");
            ViewBag.RepairCloseJob = this.checkPower("RepairCloseJob");
            ViewBag.RepairRetSetItem = this.checkPower("RepairRetSetItem");

            ViewBag.CancelJob = this.checkPower("CancelJob");
            ViewBag.SetTimeSave = this.checkPower("SetTimeSave");

            SP_JobReSet SP_JobReSet = new SP_JobReSet();
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("TNum", TNum);
            dicChar.Add("Type", "3");
            IList<SP_JobReSet> SP_JobReSetList = Entity.GetSPExtensions<SP_JobReSet>("SP_JobReSet", dicChar);
            if (SP_JobReSetList.Count > 0)
            {
                SP_JobReSet = SP_JobReSetList.FirstOrNew();
            }
            ViewBag.SP_JobReSet = SP_JobReSet;
            return View();
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(JobOrders JobOrders, EFPagingInfo<JobOrders> p, DateTime? STime, DateTime? ETime)
        {

            p = this.Condition(JobOrders, p, STime, ETime);
            var Iquery = this.Entity.JobOrders.AsQueryable();
            foreach (var item in p.SqlWhere)
            {
                Iquery = Iquery.Where(item);
            }
            ViewBag.SumAmoney = Iquery.Sum(o => (decimal?)o.TotalMoney) ?? 0m;
            ViewBag.SumPoundage = Iquery.Sum(o => (decimal?)o.Poundage) ?? 0m;
            ViewBag.Count = Iquery.Count();
            return this.View();
        }

        private EFPagingInfo<JobOrders> Condition(JobOrders JobOrders, EFPagingInfo<JobOrders> p, DateTime? STime, DateTime? ETime)
        {
            #region 筛选条件
            if (!JobOrders.PayWay.IsNullOrEmpty())
            {
                p.SqlWhere.Add(o => o.PayWay == JobOrders.PayWay);
            }

            if (!JobOrders.TNum.IsNullOrEmpty())
            {
                if (JobOrders.UId == 1)
                {
                    p.SqlWhere.Add(f => f.TNum == JobOrders.TNum);
                }
                else if (JobOrders.UId == 2)
                {
                    IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(JobOrders.TNum) || n.NeekName.Contains(JobOrders.TNum) || n.UserName == JobOrders.TNum).ToList();
                    List<int> UIds = new List<int>();
                    foreach (var pp in UList)
                    {
                        UIds.Add(pp.Id);
                    }
                    p.SqlWhere.Add(f => UIds.Contains(f.UId));
                }
                else if (JobOrders.UId == 3)
                {
                    JobItem JobItem = Entity.JobItem.FirstOrNew(o => o.RunNum == JobOrders.TNum);
                    p.SqlWhere.Add(f => f.TNum == JobItem.TNum);
                }
            }
            if (!STime.IsNullOrEmpty() && !ETime.IsNullOrEmpty())
            {
                DateTime ETime_temp = new DateTime(ETime.Value.Year, ETime.Value.Month, ETime.Value.Day, 23, 59, 59, 999);
                if (JobOrders.PayState == 1)
                { p.SqlWhere.Add(f => f.AddTime >= STime && f.AddTime <= ETime_temp); }
                else if (JobOrders.PayState == 2)
                {
                    p.SqlWhere.Add(f => f.PayTime >= STime && f.PayTime <= ETime_temp);
                }
                else if (JobOrders.PayState == 3)
                {
                    IList<JobItem> joblist = Entity.JobItem.Where(o => o.RunTime >= STime && o.RunTime <= ETime_temp).ToList();
                    List<string> tnums = joblist.Select(o => o.TNum).Distinct().ToList();
                    p.SqlWhere.Add(f => tnums.Contains(f.TNum));
                }

            }
            if (JobOrders.State != 99)
            {
                p.SqlWhere.Add(f => f.State == JobOrders.State);
            }
            if (JobOrders.AgentState != 99)
            {
                p.SqlWhere.Add(f => f.AgentState == JobOrders.AgentState);
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");

            return p;
        }

        ///// <summary>
        ///// 分润明细
        ///// </summary>
        ///// <param name="orderNum"></param>
        ///// <returns></returns>
        //public ActionResult IndexOrderProfitLog(string TNum)
        //{
        //    if (!TNum.IsNullOrEmpty())
        //    {
        //        List<OrderProfitLog> OrderProfitLogs = Entity.OrderProfitLog.Where(o => o.TNum == TNum).OrderBy(o => o.AddTime).ToList();
        //        List<int> UIds = new List<int>();
        //        foreach (var p in OrderProfitLogs.Where(n => n.LogType == 1))
        //        {
        //            UIds.Add(p.UId);
        //        }
        //        List<Users> users = Entity.Users.Where(o => UIds.Contains(o.Id)).ToList();
        //        List<int> Agents = new List<int>();
        //        foreach (var p in OrderProfitLogs.Where(n => n.LogType == 2))
        //        {
        //            Agents.Add(p.Agent);
        //        }
        //        List<Agents> sysAgents = Entity.Agents.Where(o => Agents.Contains(o.Id)).ToList(); ;
        //        foreach (var item in OrderProfitLogs)
        //        {
        //            switch (item.LogType)
        //            {
        //                case 1:
        //                    item.users = users.FirstOrNew(o => o.Id == item.UId);
        //                    break;
        //                case 2:
        //                    item.sysAgent = sysAgents.FirstOrNew(o => o.Id == item.Agent);
        //                    break;
        //            }
        //        }
        //        this.ViewBag.OrderProfitLogs = OrderProfitLogs;
        //    }
        //    return this.View();
        //}

        public ActionResult IndexOrderProfitLog(string tnum)
        {
            var OrderProfitLog = this.Entity.OrderProfitLog.Where(o => o.TNum == tnum).OrderByDescending(o => o.Id).ToList();
            ViewBag.OrderProfitLog = OrderProfitLog;

            var UIds = OrderProfitLog.Select(o => o.UId).ToList();
            var UserNameList = this.Entity.Users.Where(o => UIds.Contains(o.Id)).ToDictionary(o => o.Id, o => o.TrueName);
            ViewBag.UserNameList = UserNameList;

            var Agents = OrderProfitLog.Select(o => o.Agent).ToList();
            var AgentsList = this.Entity.SysAgent.Where(o => Agents.Contains(o.Id)).ToDictionary(o => o.Id, o => o.Name);
            ViewBag.AgentsList = AgentsList;
            return View();
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExcelExport(JobOrders JobOrders, EFPagingInfo<JobOrders> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            IPageOfItems<JobOrders> JobOrdersList = null;
            if (IsFirst == 0)
            {
                JobOrdersList = new PageOfItems<JobOrders>(new List<JobOrders>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                if (STime.IsNullOrEmpty())
                {
                    STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                }
                if (ETime.IsNullOrEmpty())
                {
                    ETime = DateTime.Now;
                }
                TimeSpan TS = Convert.ToDateTime(ETime) - Convert.ToDateTime(STime);
                int Days = TS.Days;
                if (Days > 10)
                {
                    ViewBag.ErrorMsg = "导出时间间隔不能超过10天！";
                    return View("Error");
                }
                p = this.Condition(JobOrders, p, STime, ETime);
                p.PageSize = 9999999;
                p.SqlWhere.Add(f => f.PayState == 1);
                p.OrderByList.Add("PayState", "ASC");
                JobOrdersList = Entity.Selects<JobOrders>(p);
            }

            List<string> TNumList = JobOrdersList.Select(o => o.TNum).Distinct().ToList();
            //IList<JobItem> JobItemList = Entity.JobItem.Where(n => TNumList.Contains(n.TNum)).OrderBy(n=>n.RunTime).ToList();

            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("订单号", typeof(string)));
            table.Columns.Add(new DataColumn("订单创建时间", typeof(string)));
            table.Columns.Add(new DataColumn("订单支付时间", typeof(string)));
            table.Columns.Add(new DataColumn("订单状态", typeof(string)));
            table.Columns.Add(new DataColumn("交易号", typeof(string)));
            table.Columns.Add(new DataColumn("金额", typeof(string)));
            table.Columns.Add(new DataColumn("执行时间", typeof(string)));
            table.Columns.Add(new DataColumn("手续费", typeof(string)));
            table.Columns.Add(new DataColumn("利润", typeof(string)));
            table.Columns.Add(new DataColumn("成本", typeof(string)));
            table.Columns.Add(new DataColumn("分润", typeof(string)));
            table.Columns.Add(new DataColumn("状态", typeof(string)));
            table.Columns.Add(new DataColumn("类型", typeof(string)));
            string state = "";
            //订单按照支付时间排序
            foreach (var temp in JobOrdersList)
            {
                IList<JobItem> JobItemList = Entity.JobItem.Where(n => n.TNum == temp.TNum).OrderBy(n => n.RunTime).ToList();
                if (JobOrders.PayState == 3)
                { 
                    DateTime ETime_temp = new DateTime(ETime.Value.Year, ETime.Value.Month, ETime.Value.Day, 23, 59, 59, 999);
                    JobItemList = JobItemList.Where(o => o.RunTime >= STime && o.RunTime <= ETime_temp).ToList();
                }
                // 填充数据
                #region 明细
                foreach (var item in JobItemList)
                {
                    row = table.NewRow();
                    row[0] = item.TNum;
                    row[1] = temp.AddTime.ToString("yyyy-MM-dd HH:mm");
                    row[2] = temp.PayTime.Value.ToString("yyyy-MM-dd HH:mm");
                    row[3] = temp.GetState();
                    row[4] = item.RunNum;
                    row[5] = item.RunMoney.ToString("F2");
                    row[6] = item.RunTime.ToString("yyyy-MM-dd HH:mm");
                    row[7] = item.Poundage.ToString("F2");
                    row[8] = item.HFGet.ToString("F2");
                    row[9] = item.RunGet.ToString("F2");
                    row[10] = item.AgentGet.ToString("F2");
                    switch (item.State)
                    {
                        case 0:
                            state = "取消";
                            break;
                        case 1:
                            state = "待执行";
                            break;
                        case 2:
                            state = "执行中";
                            break;
                        case 3:
                            state = "执行完成";
                            break;
                        case 4:
                            state = "执行失败";
                            break;
                    }
                    row[11] = state;
                    row[12] = item.RunType == 1 ? "消费" : "还款";
                    table.Rows.Add(row);
                }
                #endregion
            }
            string Time = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99);
            return this.ExportExcelBase(table, "任务订单明细-" + Time);
        }

        /// <summary>
        /// 关闭任务
        /// </summary>
        /// <param name="tnum"></param>
        /// <returns></returns>
        public JsonResult CloseJob(string tnum)
        {
            JsonResult JR = new JsonResult() { ContentType = "text/html" };
            if (tnum.IsNullOrEmpty())
            {
                JR.Data = new { error = 1, info = "参数有误" };
                return JR;
            }
            //重新统计成功与失败后金额
            Entity.ExecuteStoreCommand("Exec SP_JobReSet '" + tnum + "',1");

            JobOrders JobOrders = Entity.JobOrders.FirstOrDefault(n => n.TNum == tnum);
            if (JobOrders == null) {
                JR.Data = new { error = 1, info = "订单不存在" };
                return JR;
            }
            if (JobOrders.State != 5)
            {
                JR.Data = new { error = 1, info = "当前订单状态不能结清操作" };
                return JR;
            }
            if (JobOrders.Amount < 1) {
                JR.Data = new { error = 1, info = "本订单余额小于0无需再次清算" };
                return JR;
            }
            int count = Entity.JobItem.Count(n => n.TNum == tnum && n.State != 3 && n.State != 4 && n.State != 0);
            if (count > 0)
            {
                JR.Data = new { error = 1, info = "本订单有" + count + "笔交易未达到最终状态，暂不可操作。" };
                return JR;
            }
            DateTime Now = DateTime.Now;
            DateTime RunTime = Now;
            if (RunTime.Hour < 12) {
                RunTime.AddHours(12 - RunTime.Hour);
            }
            //生成多一单还款计划就OK了
            JobItem JobItem = new JobItem();
            JobItem.UId = JobOrders.UId;
            JobItem.TNum = JobOrders.TNum;
            JobItem.RunMoney = JobOrders.Amount;


            JobItem.RunTime = RunTime;
            JobItem.Poundage = 0;
            JobItem.RunGet = JobItem.RunMoney * JobOrders.CashRate;
            if (JobItem.RunGet < JobOrders.CashMin)
            {
                JobItem.RunGet = JobOrders.CashMin;
            }
            if (JobItem.RunGet > JobOrders.CashMax)
            {
                JobItem.RunGet = JobOrders.CashMax;
            }
            JobItem.RunGet = JobItem.RunGet.Ceiling();//通道成本
            JobItem.AgentGet = 0;
            //利润=用户手续费-代理分润-通道成本
            JobItem.HFGet = JobItem.Poundage - JobItem.AgentGet - JobItem.RunGet;
            JobItem.State = 1;
            JobItem.AddTime = Now;
            JobItem.RunType = 2;
            JobItem.RunState = 0;
            JobItem.PayWay = JobOrders.CashWay;
            JobItem.UserCardId = JobOrders.UserCardId;
            JobItem.Remark = "任务失败退回剩余金额";
            JobItem.RunSort = 9999;
            Entity.JobItem.AddObject(JobItem);
            Entity.SaveChanges();
            JR.Data = new { error = 0, info = "处理成功" };
            return JR;
        }
        /// <summary>
        /// 重启任务
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="RunTime"></param>
        /// <returns></returns>
        public JsonResult RetSetItem(int Id, DateTime RunTime)
        {
            JsonResult JR = new JsonResult() { ContentType = "text/html" };
            if (Id.IsNullOrEmpty())
            {
                JR.Data = new { error = 1, info = "参数有误" };
                return JR;
            }
            JobItem OldItem = Entity.JobItem.FirstOrDefault(n => n.Id == Id);
            if (OldItem == null)
            {
                JR.Data = new { error = 1, info = "交易不存在" };
                return JR;
            }
            if (OldItem.State != 4)
            {
                JR.Data = new { error = 1, info = "当前交易状态不能重启操作" };
                return JR;
            }
            JobOrders JobOrders = Entity.JobOrders.FirstOrDefault(n => n.TNum == OldItem.TNum);
            if (JobOrders == null)
            {
                JR.Data = new { error = 1, info = "订单不存在" };
                return JR;
            }
            if (JobOrders.State != 5)
            {
                JR.Data = new { error = 1, info = "当前订单状态不能重启操作" };
                return JR;
            }
            int count = Entity.JobItem.Count(n => n.TNum == OldItem.TNum && n.State != 3 && n.State != 4 && n.State != 0);
            if (count > 0)
            {
                JR.Data = new { error = 1, info = "本订单有" + count + "笔交易未达到最终状态，暂不可操作。" };
                return JR;
            }
            string Remark = "重启自" + OldItem.RunNum;
            //复制多一单还款计划
            JobItem JobItem = Entity.JobItem.FirstOrDefault(n => n.TNum == OldItem.TNum && n.Remark == Remark && n.State == 0 && n.RunState == 0);
            if (JobItem == null)
            {
                JobItem = new JobItem();

                JobItem.UId = OldItem.UId;
                JobItem.TNum = OldItem.TNum;
                JobItem.RunNum = OldItem.RunNum;
                JobItem.RunMoney = OldItem.RunMoney;
                JobItem.Poundage = OldItem.Poundage;
                JobItem.AgentGet = OldItem.AgentGet;
                JobItem.HFGet = OldItem.HFGet;
                JobItem.RunGet = OldItem.RunGet;
                JobItem.RunType = OldItem.RunType;
                JobItem.PayWay = OldItem.PayWay;
                JobItem.UserCardId = OldItem.UserCardId;
                JobItem.RunSort = OldItem.RunSort;

                JobItem.RunTime = RunTime;
                JobItem.RunState = 0;
                JobItem.State = 0;
                JobItem.AddTime = DateTime.Now;
                JobItem.RunedTime = null;
                JobItem.Remark = Remark;
                Entity.JobItem.AddObject(JobItem);
                Entity.SaveChanges();
            }
            else {
                JobItem.RunTime = RunTime;
                Entity.SaveChanges();
            }
            //这里要验证进出帐是否是平的
            decimal D1 = Entity.JobItem.Where(n => n.TNum == OldItem.TNum && n.RunType == 1 && (n.State == 0 || n.State == 3)).Sum(n => n.RunMoney);
            decimal D2 = Entity.JobItem.Where(n => n.TNum == OldItem.TNum && n.RunType == 1 && (n.State == 0 || n.State == 3)).Sum(n => n.Poundage);
            decimal E1 = Entity.JobItem.Where(n => n.TNum == OldItem.TNum && n.RunType == 2 && (n.State == 0 || n.State == 3)).Sum(n => n.RunMoney);
            decimal A0 = D1 - D2 - E1;
            if (A0 < 0)
            {
                JR.Data = new { error = 1, info = "检查到任务不平帐"};
                return JR;
            }
            if (A0 > 0.5M)
            {
                JR.Data = new { error = 1, info = "检查到任务不平帐" };
                return JR;
            }
            //验证是否有比它还晚的交易
            count = Entity.JobItem.Count(n => n.TNum == OldItem.TNum && n.State == 0 && n.RunTime < RunTime && n.Id != JobItem.Id);
            if (count > 0) {
                JR.Data = new { error = 99, info = "当前需要调整后续费任务时间", item = JobItem.Id };
                return JR;
            }
            count = Entity.JobItem.Count(n => n.TNum == OldItem.TNum && n.State == 0 && n.RunTime < DateTime.Now && n.Id != JobItem.Id);
            if (count > 0)
            {
                JR.Data = new { error = 99, info = "当前需要调整后续费任务时间", item = JobItem.Id };
                return JR;
            }
            //增加检测排序执行时间
            IList<JobItem> JobItemList = Entity.JobItem.Where(n => n.TNum == OldItem.TNum && n.State == 0).OrderBy(n => n.RunSort).ThenBy(n => n.Id).ToList();
            if (JobItemList.Count == 0) {
                JR.Data = new { error = 1, info = "没有需要重启的交易" };
                return JR;
            }
            DateTime RT = JobItemList.First().RunTime;
            bool CanRun = true;
            foreach (var p in JobItemList.Skip(1)) {
                if (RT > p.RunTime) {
                    JR.Data = new { error = 99, info = p.RunNum + "执行时间比前一笔交易慢" };
                    CanRun = false;
                    break;
                }
                RT = p.RunTime;
            }
            if (!CanRun)
            {
                return JR;
            }
            Entity.ExecuteStoreCommand("Exec SP_JobReSet '" + OldItem.TNum + "',2");
            JR.Data = new { error = 0, info = "处理成功" };
            return JR;
        }
        /// <summary>
        /// 展示待调整执行时间任务列表
        /// </summary>
        /// <param name="TNum"></param>
        /// <returns></returns>
        public ActionResult SetTime(string TNum) {
            IList<JobItem> JobItemList = Entity.JobItem.Where(n => n.TNum == TNum && (n.State == 0 || n.State == 1)).OrderBy(n => n.RunSort).ThenBy(n => n.Id).ToList();
            ViewBag.JobItemList = JobItemList;
            return View();
        }
        /// <summary>
        /// 保存任务执行时间
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="RunTime"></param>
        public void SetTimeSave(int Id, DateTime RunTime)
        {
            JobItem JobItem = Entity.JobItem.FirstOrDefault(n => n.Id == Id);
            JobItem.RunTime = RunTime;
            Entity.SaveChanges();
            Response.Write("OK");
        }

        /// <summary>
        /// 补单并关闭任务
        /// </summary>
        /// <param name="tnum"></param>
        /// <returns></returns>
        public JsonResult RepairCloseJob(int Id)
        {
            JsonResult JR = new JsonResult() { ContentType = "text/html" };
            if (Id.IsNullOrEmpty())
            {
                JR.Data = new { error = 1, info = "参数有误" };
                return JR;
            }
            JobItem JobItem = Entity.JobItem.FirstOrNew(o => o.Id == Id);
            JobItem.State = 2;
            JobItem.RunState = 2;
            Entity.SaveChanges();
            //查询订单状态
            JobItem = JobItem.PayQuery(Entity);
            if (JobItem.State != 3) {
                JR.Data = new { error = 1, info = "当前订单补单失败!" };
                return JR;
            }
            JR = CloseJob(JobItem.TNum);
            return JR;
        }
        /// <summary>
        ///  补单并重启任务
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult RepairRetSetItem(int Id)
        {
            JsonResult JR = new JsonResult() { ContentType = "text/html" };
            if (Id.IsNullOrEmpty())
            {
                JR.Data = new { error = 1, info = "参数有误" };
                return JR;
            }
            JobItem JobItem = Entity.JobItem.FirstOrDefault(n => n.Id == Id);
            if (JobItem == null)
            {
                JR.Data = new { error = 1, info = "交易不存在" };
                return JR;
            }
            if (JobItem.State != 4)
            {
                JR.Data = new { error = 1, info = "当前交易状态不能重启操作" };
                return JR;
            }
            JobOrders JobOrders = Entity.JobOrders.FirstOrDefault(n => n.TNum == JobItem.TNum);
            if (JobOrders == null)
            {
                JR.Data = new { error = 1, info = "订单不存在" };
                return JR;
            }
            if (JobOrders.State != 5 && JobOrders.State != 1) 
            {
                JR.Data = new { error = 1, info = "当前订单状态不能重启操作" };
                return JR;
            }
            int count = Entity.JobItem.Count(n => n.TNum == JobItem.TNum && n.State != 3 && n.State != 4 && n.State != 0);
            if (count > 0)
            {
                JR.Data = new { error = 1, info = "本订单有" + count + "笔交易未达到最终状态，暂不可操作。" };
                return JR;
            }

            //这里要验证进出帐是否是平的
            decimal D1 = Entity.JobItem.Where(n => n.TNum == JobItem.TNum && n.RunType == 1 && (n.State == 0 || n.State == 3 || (n.State == 4 && n.Id == Id))).Sum(n => n.RunMoney);
            decimal D2 = Entity.JobItem.Where(n => n.TNum == JobItem.TNum && n.RunType == 1 && (n.State == 0 || n.State == 3 || (n.State == 4 && n.Id == Id))).Sum(n => n.Poundage);
            decimal E1 = Entity.JobItem.Where(n => n.TNum == JobItem.TNum && n.RunType == 2 && (n.State == 0 || n.State == 3)).Sum(n => n.RunMoney);
            decimal A0 = D1 - D2 - E1;
            if (A0 < 0)
            {
                JR.Data = new { error = 1, info = "检查到任务不平帐" };
                return JR;
            }
            if (A0 > 0.5M)
            {
                JR.Data = new { error = 1, info = "检查到任务不平帐" };
                return JR;
            }
            count = Entity.JobItem.Count(n => n.TNum == JobItem.TNum && n.State == 0 && n.RunTime < DateTime.Now);
            if (count > 0)
            {
                JR.Data = new { error = 99, info = "当前需要调整后续费任务时间", item = JobItem.Id };
                return JR;
            }
            //增加检测排序执行时间
            IList<JobItem> JobItemList = Entity.JobItem.Where(n => n.TNum == JobItem.TNum && n.State == 0).OrderBy(n => n.RunSort).ThenBy(n => n.Id).ToList();
            if (JobItemList.Count == 0)
            {
                JR.Data = new { error = 1, info = "没有需要重启的交易" };
                return JR;
            }
            DateTime RT = JobItemList.First().RunTime;
            bool CanRun = true;
            foreach (var p in JobItemList.Skip(1))
            {
                if (RT > p.RunTime)
                {
                    JR.Data = new { error = 99, info = p.RunNum + "执行时间比前一笔交易慢" };
                    CanRun = false;
                    break;
                }
                RT = p.RunTime;
            }
            if (!CanRun)
            {
                return JR;
            }
            //查询订单状态
            JobItem.State = 2;
            JobItem.RunState = 2;
            Entity.SaveChanges();
            JobItem = JobItem.PayQuery(Entity);
            if (JobItem.State != 3)
            {
                JR.Data = new { error = 1, info = "当前订单补单失败!" };
                return JR;
            }
            Entity.SaveChanges();
            Entity.ExecuteStoreCommand("Exec SP_JobReSet '" + JobItem.TNum + "',2");
            JR.Data = new { error = 0, info = "处理成功" };
            return JR;
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="tnum"></param>
        /// <returns></returns>
        public JsonResult CancelJob(string tnum, string Rs)
        {
            JsonResult JR = new JsonResult() { ContentType = "text/html" };
            if (tnum.IsNullOrEmpty() || Rs.IsNullOrEmpty())
            {
                JR.Data = new { error = 1, info = "参数有误" };
                return JR;
            }
            Entity.ExecuteStoreCommand("Exec SP_JobReSet '" + tnum + "',1");
            JobOrders baseJobOrders = Entity.JobOrders.FirstOrNew(o => o.TNum == tnum);
            if (baseJobOrders == null)
            {
                JR.Data = new { error = 1, info = "订单不存在" };
                return JR;
            }
            if (baseJobOrders.State != 3)
            {
                JR.Data = new { error = 1, info = "当前订单状态不能取消操作" };
                return JR;
            }
            bool IsItemRun = Entity.JobItem.Any(o => o.TNum == baseJobOrders.TNum && o.State == 2);
            if (IsItemRun)
            {
                JR.Data = new { error = 1, info = "子订单有正在执行中的状态,不能执行该操作" };
                return JR;
            }
            
            baseJobOrders.Remark = Rs + ",时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            baseJobOrders.State = 5;
            Entity.SaveChanges();
            //取消所有待执行订单
            Entity.ExecuteStoreCommand("Update JobItem Set State=0 Where TNum='" + baseJobOrders.TNum + "' and State=1");

            //重新统计成功与失败后金额
            if (baseJobOrders.Amount >= 1) {
                JR = CloseJob(baseJobOrders.TNum);
                return JR;
            }
            JR.Data = new { error = 0, info = "取消订单成功!" };
            return JR;
        }
        public JsonResult SplitMoney(string tnum) {
            JsonResult JR = new JsonResult() { ContentType = "text/html" };
            if (tnum.IsNullOrEmpty())
            {
                JR.Data = new { error = 1, info = "参数有误" };
                return JR;
            }
            JobOrders JobOrders = Entity.JobOrders.FirstOrNew(o => o.TNum == tnum);
            if (JobOrders == null)
            {
                JR.Data = new { error = 1, info = "订单不存在" };
                return JR;
            }
            if (JobOrders.State != 5)
            {
                JR.Data = new { error = 1, info = "当前订单状态不能分润" };
                return JR;
            }
            if (JobOrders.AgentState != 0)
            {
                JR.Data = new { error = 1, info = "当前订单已分润" };
                return JR;
            }
            IList<JobItem> JobItemList = Entity.JobItem.Where(n => n.State == 3 && n.TNum == JobOrders.TNum).ToList();
            if (JobItemList.Count == 0)
            {
                JR.Data = new { error = 1, info = "当前任务没有执行成功的交易" };
                return JR;
            }
            decimal TotalPoundage = JobItemList.Sum(n => n.Poundage);
            decimal TotalHFGet = JobItemList.Sum(n => n.HFGet);
            decimal TotalAgentGet = JobItemList.Sum(n => n.AgentGet);
            decimal TotalPoundage1 = JobItemList.Where(n => n.RunType == 1).Sum(n => n.Poundage);
            decimal TotalPoundage2 = JobItemList.Where(n => n.RunType == 2).Sum(n => n.Poundage);
            decimal TotalRunGet1 = JobItemList.Where(n => n.RunType == 1).Sum(n => n.RunGet);
            decimal TotalRunGet2 = JobItemList.Where(n => n.RunType == 2).Sum(n => n.RunGet);
            JobOrders.Cols = "Id,Poundage,AgentGet,HFGet,PayPoundage,CashPoundage,PayGet,CashGet";
            string OldData = JobOrders.ToJson();
            JobOrders.RemarkSplit = "操作人:" + AdminUser.TrueName + ",操作时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "【" + OldData + "】";
            JobOrders.Poundage = TotalPoundage;//总手续费
            JobOrders.AgentGet = TotalAgentGet;//代理分润
            JobOrders.HFGet = TotalHFGet;//利润
            JobOrders.PayPoundage = TotalPoundage1;//交易手续费
            JobOrders.CashPoundage = TotalPoundage2;//代付手续费
            JobOrders.PayGet = TotalRunGet1;//交易成本
            JobOrders.CashGet = TotalRunGet2;//代付成本
            if (JobOrders.AgentGet == 0)
            {
                JobOrders.AgentState = 1;
                JobOrders.AgentTime = DateTime.Now;
            }
            Entity.SaveChanges();
            
            if (JobOrders.AgentGet > 0)
            {
                JobOrders = JobOrders.PayAgent(Entity);
            }
            JR.Data = new { error = 0, info = "取消订单成功!" };
            return JR;
        }
    }
    public class SP_JobReSet
    {
        public decimal Amoney { get; set; }
        public decimal XPayMoney { get; set; }
        public decimal XCashPoundage { get; set; }
        public decimal HPayMoney { get; set; }
        public decimal HCashPoundage { get; set; }
    }
}
