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

namespace LokFu.Areas.Agent.Controllers
{
    public class JobOrdersController : BaseController
    {
        public bool IsAll = false;
        public ActionResult Index(JobOrders JobOrders, EFPagingInfo<JobOrders> p, bool? IsShowSupAgent, int? LowerLevel, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            IPageOfItems<JobOrders> JobOrdersList = null;
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            ViewBag.BasicAgent = BasicAgent;
            ViewBag.IsAll = IsAll;
            LowerLevel = LowerLevel == null ? 0 : LowerLevel;
            if (IsFirst == 0)
            {
                JobOrders.State = 99;
                JobOrdersList = new PageOfItems<JobOrders>(new List<JobOrders>(), 0, 10, 0, new Hashtable());
            }
            else
            {

                p = this.Condition(JobOrders, p, STime, ETime);

                IList<SysAgent> SysAgentList = null;
                if ((bool)IsShowSupAgent)
                {
                    IList<int> UID = new List<int>();
                    if (LowerLevel != 0)
                    {
                        SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == LowerLevel).FirstOrNew();
                        SysAgentList = LowerLevelAgent.GetSupAgent(Entity, true);
                    }
                    else
                    {
                        SysAgentList = BasicAgent.GetSupAgent(Entity, true);//获取所有下级代理商信息
                    }
                    UID = SysAgentList.Select(o => o.Id).ToList();
                    p.SqlWhere.Add(f => UID.Contains(f.AgentId));
                }
                else
                {
                    p.SqlWhere.Add(f => f.AgentId == BasicAgent.Id);//读取全部分支机构
                }

                JobOrdersList = Entity.Selects<JobOrders>(p);
            }
            ViewBag.JobOrdersList = JobOrdersList;
            ViewBag.JobOrders = JobOrders;
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.ETime = ETime;
            ViewBag.STime = STime;
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.LowerLevel = LowerLevel;
            List<int> UId = JobOrdersList.Select(o => o.UId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            return View();
        }
        public ActionResult Edit(JobOrders JobOrders)
        {
            if (JobOrders.Id != 0) JobOrders = Entity.JobOrders.FirstOrDefault(n => n.Id == JobOrders.Id);
            if (JobOrders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (!IsBelongToAgent(JobOrders.AgentId))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            ViewBag.OrderProfitLog = Entity.OrderProfitLog.FirstOrNew(o => o.LogType == 2 && o.Agent == this.AdminUser.AgentId && o.TNum == JobOrders.TNum);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == JobOrders.AgentId);
            ViewBag.Users = Entity.Users.FirstOrDefault(o => o.Id == JobOrders.UId);
            //前端没调用调出来做什么？By Lin 2017-12-19
            //ViewBag.JobPayWay = Entity.JobPayWay.FirstOrDefault(o => o.Id == JobOrders.PayWay);
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
            if (!IsBelongToAgent(JobOrders.AgentId))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            var JobItemList = this.Entity.JobItem.Where(o => o.TNum == TNum).OrderBy(o => o.RunTime).ToList();
            ViewBag.JobItemList = JobItemList;
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
                else
                {
                    IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(JobOrders.TNum) || n.NeekName.Contains(JobOrders.TNum) || n.UserName == JobOrders.TNum).ToList();
                    List<int> UIds = new List<int>();
                    foreach (var pp in UList)
                    {
                        UIds.Add(pp.Id);
                    }
                    p.SqlWhere.Add(f => UIds.Contains(f.UId));
                }
            }
            if (!STime.IsNullOrEmpty() && !ETime.IsNullOrEmpty())
            {
                DateTime ETime_temp = new DateTime(ETime.Value.Year, ETime.Value.Month, ETime.Value.Day, 23, 59, 59, 999);
                if (JobOrders.PayState == 1)
                { p.SqlWhere.Add(f => f.AddTime >= STime && f.AddTime <= ETime_temp); }
                else
                {
                    p.SqlWhere.Add(f => f.PayTime >= STime && f.PayTime <= ETime_temp);
                }

            }
            if (JobOrders.State != 99)
            {
                p.SqlWhere.Add(f => f.State == JobOrders.State);
            }
            #endregion
            //p.SqlWhere.Add(o => o.AgentId == BasicAgent.Id);
            p.OrderByList.Add("Id", "DESC");

            return p;
        }

        public ActionResult IndexOrderProfitLog(string tnum)
        {
            JobOrders Orders = Entity.JobOrders.FirstOrDefault(n => n.TNum == tnum);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "查询的交易不存在";
                return View("Error");
            }
            var query = this.Entity.OrderProfitLog.Where(o => o.TNum == tnum);
            if (Orders.AgentId == this.BasicAgent.Id)
            {
                query = query.Where(o => (o.Agent == this.BasicAgent.Id || o.LogType == 1));
            }
            else
            {
                query = query.Where(o => o.Agent == this.BasicAgent.Id);
            }
            var OrderProfitLog = query.OrderByDescending(o => o.Id).ToList();
            ViewBag.OrderProfitLog = OrderProfitLog;

            var UIds = OrderProfitLog.Select(o => o.UId).ToList();
            var UserNameList = this.Entity.Users.Where(o => UIds.Contains(o.Id)).ToDictionary(o => o.Id, o => o.TrueName);
            ViewBag.UserNameList = UserNameList;

            var SysAgent = OrderProfitLog.Select(o => o.Agent).ToList();
            var SysAgentList = this.Entity.SysAgent.Where(o => SysAgent.Contains(o.Id)).ToDictionary(o => o.Id, o => o.Name);
            ViewBag.SysAgentList = SysAgentList;

            return View();
        }

    }
}
