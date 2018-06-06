using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data;
namespace LokFu.Areas.Manage.Controllers
{
    public class JobOrdersPayController : BaseController
    {

        public ActionResult Index(OrderProfitLog OrderProfitLog, EFPagingInfo<OrderProfitLog> p, string TName, DateTime? STime, DateTime? ETime, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }

            if (STime.IsNullOrEmpty())
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ETime.IsNullOrEmpty())
            {
                //JobOrders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                ETime = DateTime.Now;
            }
            IPageOfItems<OrderProfitLog> OrderProfitLogList = null;
            if (IsFirst == 0)
            {
                OrderProfitLogList = new PageOfItems<OrderProfitLog>(new List<OrderProfitLog>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                p = this.Condition(OrderProfitLog, p, TName, STime, ETime, IsShowSupAgent);
                OrderProfitLogList = Entity.Selects<OrderProfitLog>(p);
            }
            ViewBag.TName = TName;
            ViewBag.OrderProfitLogList = OrderProfitLogList;
            ViewBag.OrderProfitLog = OrderProfitLog;
            IList<OrderProfitLog> List = OrderProfitLogList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.SelectAgentsList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.STime = STime;
            ViewBag.ETime = ETime;
            ViewBag.ExcelExport = this.checkPower("Xls");
            return View();
        }
        public ActionResult Edit(JobOrders JobOrders)
        {
            JobOrders = Entity.JobOrders.FirstOrDefault(n => n.TNum == JobOrders.TNum);
            if (JobOrders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.JobOrders = JobOrders;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == JobOrders.UId);
            //ViewBag.Agents = Entity.Agents.FirstOrNew(n => n.Id == JobOrders.AgentId);
            //ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == JobOrders.AgentId);
            ViewBag.JobPayWay = Entity.JobPayWay.FirstOrDefault(o => o.Id == JobOrders.PayWay);
            ViewBag.UserTrailIndex = this.checkPower("UserTrail", "Index");
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View("../JobOrders/Edit");
        }

        public FileResult XLSDo(OrderProfitLog OrderProfitLog, EFPagingInfo<OrderProfitLog> p, string TName, DateTime? STime, DateTime? ETime, bool? IsShowSupAgent)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }

            if (STime.IsNullOrEmpty())
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ETime.IsNullOrEmpty())
            {
                //JobOrders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                ETime = DateTime.Now;
            }
            p.PageSize = 9999999;
            p = this.Condition(OrderProfitLog, p, TName, STime, ETime, IsShowSupAgent);
            IPageOfItems<OrderProfitLog> OrderProfitLogList = Entity.Selects<OrderProfitLog>(p);
            IList<OrderProfitLog> List = OrderProfitLogList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            var UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("交易号", typeof(string)));
            table.Columns.Add(new DataColumn("交易金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("分润商户", typeof(string)));
            table.Columns.Add(new DataColumn("分润类型", typeof(string)));
            table.Columns.Add(new DataColumn("分润金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("分润时间", typeof(string)));

            // 填充数据
            foreach (var item in OrderProfitLogList)
            {

                var u = UsersList.FirstOrNew(o => o.Id == item.UId);
                row = table.NewRow();
                row[0] = item.TNum;
                row[1] = item.Amoney.ToString("F2");
                row[2] = u.UserName;
                row[3] = LogTypeName(item.LogType);
                row[4] = item.Profit.ToString("F2");
                row[5] = item.AddTime.ToString("yyyy-MM-dd HH:mm:ss");
                table.Rows.Add(row);
            }

            return this.ExportExcelBase(table, "自动刷还分润");
        }

        private EFPagingInfo<OrderProfitLog> Condition(OrderProfitLog OrderProfitLog, EFPagingInfo<OrderProfitLog> p, string TName, DateTime? STime, DateTime? ETime, bool? IsShowSupAgent)
        {
            #region 校验与初始化

            // p.SqlWhere.Add(f => f.PayState == 1);
            if (!OrderProfitLog.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == OrderProfitLog.TNum); }
            if (!TName.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName == TName || n.NeekName == TName || n.UserName == TName).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }

            if (!OrderProfitLog.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == OrderProfitLog.Agent).FirstOrNew();
                    IList<SysAgent> AgentsList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = new List<int>();
                    foreach (var s in AgentsList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == OrderProfitLog.Agent);
                }
            }
            //if (!JobOrders.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == (JobOrders.AgentState == 99 ? 0 : JobOrders.AgentState)); }
            if (!STime.IsNullOrEmpty() && !ETime.IsNullOrEmpty())
            {
                DateTime ETimes = ETime.Value;
                p.SqlWhere.Add(f => f.AddTime > STime && f.AddTime < ETimes);
            }
            #endregion
            p.SqlWhere.Add(o => o.OrderType == 31);
            p.OrderByList.Add("Id", "DESC");

            return p;
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(OrderProfitLog OrderProfitLog, EFPagingInfo<OrderProfitLog> p, string TName, DateTime? STime, DateTime? ETime, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }

            if (STime.IsNullOrEmpty())
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ETime.IsNullOrEmpty())
            {
                //JobOrders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                ETime = DateTime.Now;
            }
            p.PageSize = 9999999;
            p = this.Condition(OrderProfitLog, p, TName, STime, ETime, IsShowSupAgent);
            IPageOfItems<OrderProfitLog> OrderProfitLogList = Entity.Selects<OrderProfitLog>(p);
            ViewBag.SumAmoney = OrderProfitLogList.Sum(o => o.Amoney);
            ViewBag.SumProfit = OrderProfitLogList.Sum(o => o.Profit);
            ViewBag.Count = OrderProfitLogList.ToList().Count;
            return this.View();
        }
        private string LogTypeName(int LogType)
        {
            string reslut = string.Empty;
            switch (LogType)
            {
                case 1:
                    reslut = "商户分润";
                    break;
                case 2:
                    reslut = "代理分润";
                    break;
            }
            return reslut;
        }
    }
}
