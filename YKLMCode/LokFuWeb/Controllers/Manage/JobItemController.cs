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
    public class JobItemController : BaseController
    {
        public ActionResult Index(JobItem JobItem, EFPagingInfo<JobItem> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            IPageOfItems<JobItem> JobItemList = null;
            if (STime.IsNullOrEmpty())
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ETime.IsNullOrEmpty())
            {
                ETime = DateTime.Now;
            }
            if (IsFirst == 0)
            {
                JobItem.State = 99;
                JobItemList = new PageOfItems<JobItem>(new List<JobItem>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                p = this.Condition(JobItem, p, STime, ETime);
                JobItemList = Entity.Selects<JobItem>(p);
            }
            List<int> UId = JobItemList.Select(o => o.UId).Distinct().ToList();
            List<int> CardId = JobItemList.Select(o => o.UserCardId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.UsersCardList = Entity.UserCard.Where(n => CardId.Contains(n.Id)).ToList();
            ViewBag.JobItemList = JobItemList;
            ViewBag.JobItem = JobItem;
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.ETime = ETime;
            ViewBag.STime = STime;
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            ViewBag.JobPayWayList = Entity.JobPayWay.ToList();
            return View();
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(JobItem JobItem, EFPagingInfo<JobItem> p, DateTime? STime, DateTime? ETime)
        {

            p = this.Condition(JobItem, p, STime, ETime);
            var Iquery = this.Entity.JobItem.AsQueryable();
            foreach (var item in p.SqlWhere)
            {
                Iquery = Iquery.Where(item);
            }
            ViewBag.SumRunMoney = Iquery.Sum(o => (decimal?)o.RunMoney) ?? 0m;
            ViewBag.SumPoundage = Iquery.Sum(o => (decimal?)o.Poundage) ?? 0m;
            ViewBag.SumHFGet = Iquery.Sum(o => (decimal?)o.HFGet) ?? 0m;
            ViewBag.SumAgentGet = Iquery.Sum(o => (decimal?)o.AgentGet) ?? 0m;
            ViewBag.Count = Iquery.Count();
            return this.View();
        }

        private EFPagingInfo<JobItem> Condition(JobItem JobItem, EFPagingInfo<JobItem> p, DateTime? STime, DateTime? ETime)
        {
            #region 筛选条件
            if (!JobItem.RunType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(o => o.RunType == JobItem.RunType);
            }

            if (!JobItem.TNum.IsNullOrEmpty())
            {
                if (JobItem.UId == 1)
                {
                    p.SqlWhere.Add(f => f.TNum == JobItem.TNum);
                }
                else if (JobItem.UId == 2)
                {
                    p.SqlWhere.Add(f => f.RunNum == JobItem.TNum);
                }
            }
            if (!STime.IsNullOrEmpty() && !ETime.IsNullOrEmpty())
            {
                if (JobItem.RunSort == 1)
                { p.SqlWhere.Add(f => f.AddTime >= STime && f.AddTime <= ETime); }
                else if (JobItem.RunSort == 2)
                {
                    p.SqlWhere.Add(f => f.RunTime >= STime && f.RunTime <= ETime);
                }
            }
            if (JobItem.State != 99)
            {
                p.SqlWhere.Add(f => f.State == JobItem.State);
            }
            #endregion
            //if (JobItem.State != 3 && JobItem.State != 4)
            //{
            //    p.OrderByList.Add("RunTime", "ASC");
            //}
            //else
            //{
            //    p.OrderByList.Add("RunEdTime", "ASC");
            //}
            p.OrderByList.Add("RunTime", "DESC");
            return p;
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExcelExport(JobItem JobItem, EFPagingInfo<JobItem> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            IPageOfItems<JobItem> JobItemList = null;
            if (IsFirst == 0)
            {
                JobItemList = new PageOfItems<JobItem>(new List<JobItem>(), 0, 10, 0, new Hashtable());
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
                p = this.Condition(JobItem, p, STime, ETime);
                p.PageSize = 9999999;
                JobItemList = Entity.Selects<JobItem>(p);
            }
            DataTable table = new DataTable();
            DataRow row = null;
            // 创建 datatable
            table.Columns.Add(new DataColumn("订单号", typeof(string)));
            table.Columns.Add(new DataColumn("交易号", typeof(string)));
            table.Columns.Add(new DataColumn("执行金额", typeof(string)));
            table.Columns.Add(new DataColumn("执行时间", typeof(string)));
            table.Columns.Add(new DataColumn("手续费", typeof(string)));
            table.Columns.Add(new DataColumn("利润", typeof(string)));
            table.Columns.Add(new DataColumn("成本", typeof(string)));
            table.Columns.Add(new DataColumn("分润", typeof(string)));
            table.Columns.Add(new DataColumn("状态", typeof(string)));
            table.Columns.Add(new DataColumn("类型", typeof(string)));
            table.Columns.Add(new DataColumn("备注", typeof(string)));
            table.Columns.Add(new DataColumn("订单状态备注", typeof(string)));
            string state = "";
                // 填充数据
                #region 明细
                foreach (var item in JobItemList)
                {
                    row = table.NewRow();
                    row[0] = item.TNum;
                    row[1] = item.RunNum;
                    row[2] = item.RunMoney.ToString("F2");
                    row[3] = item.RunTime.ToString("yyyy-MM-dd HH:mm");
                    row[4] = item.Poundage.ToString("F2");
                    row[5] = item.HFGet.ToString("F2");
                    row[6] = item.RunGet.ToString("F2");
                    row[7] = item.AgentGet.ToString("F2");
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
                    row[8] = state;
                    row[9] = item.RunType == 1 ? "消费" : "还款";
                    row[10] = item.Remark;
                    string stateremark = "";
                    if (item.State == 4)
                    {
                        JobItem JobItemTemp = Entity.JobItem.Where(o=>o.TNum==item.TNum).OrderBy(o=>o.RunTime).FirstOrNew();
                        if (JobItemTemp.State == 4)
                        {
                            stateremark = "首笔支付失败";
                        }
                    }
                    row[11] = stateremark;
                    table.Rows.Add(row);
                }
                #endregion
            string Time = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99);
            return this.ExportExcelBase(table, "任务订单明细-" + Time);
        }
    }
}
