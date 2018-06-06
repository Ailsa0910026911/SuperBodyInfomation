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
    public class JobOrdersPayController : BaseController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="JobOrders"></param>
        /// <param name="p"></param>
        /// <param name="IsShowSupAgent">是否显示下级</param>
        /// <param name="LowerLevel">下级代理商</param>
        /// <returns></returns>
        public ActionResult Index(JobOrders JobOrders, EFPagingInfo<JobOrders> p, DateTime? STime, DateTime? ETime, bool? IsShowSupAgent, int? LowerLevel, int IsFirst = 0)
        {

            #region 条件校验
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            LowerLevel = LowerLevel == null ? 0 : LowerLevel;
            p.SqlWhere.Add(f => f.PayState != 0);
            p.SqlWhere.Add(f => f.AgentState != 0);
            if (!JobOrders.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == JobOrders.TNum); }
            if (!JobOrders.Code.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName == JobOrders.Code || n.NeekName == JobOrders.Code || n.UserName == JobOrders.Code).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!JobOrders.AgentId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentId == JobOrders.AgentId); }
            if (!JobOrders.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == JobOrders.AgentState); }
            if (!STime.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.PayTime > STime);
            }
            else
            {
                STime = DateTime.Now.AddMonths(-1);
            }
            if (!ETime.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.PayTime < ETime);
            }
            else
            {
                ETime = DateTime.Now;
            }
            TimeSpan TS = ETime.Value.Subtract(STime.Value);
            int Days = TS.Days;
            if (Days > 31)
            {
                ViewBag.ErrorMsg = "统计时间间隔不能超过31天！";
                return View("Error");
            }
            IList<SysAgent> AgentsList = null;
            if ((bool)IsShowSupAgent)
            {
                IList<int> UID = new List<int>();
                if (LowerLevel != 0)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == LowerLevel).FirstOrNew();
                    AgentsList = LowerLevelAgent.GetSupAgent(Entity, true);
                }
                else
                {
                    AgentsList = BasicAgent.GetSupAgent(Entity, true);//获取所有下级代理商信息
                }
                UID = AgentsList.Select(o => o.Id).ToList();
                p.SqlWhere.Add(f => UID.Contains(f.AgentId));
            }
            else
            {
                p.SqlWhere.Add(f => f.AgentId == BasicAgent.Id);//读取全部分支机构
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<JobOrders> JobOrdersList = null;
            if (IsFirst == 0)
            {
                JobOrdersList = new PageOfItems<JobOrders>(new List<JobOrders>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                JobOrdersList = Entity.Selects<JobOrders>(p);
            }

            ViewBag.JobOrdersList = JobOrdersList;
            ViewBag.JobOrders = JobOrders;
            //统计查询
            if (JobOrdersList.TotalCount > 0)
            {
                var iquery = Entity.JobOrders.AsQueryable();
                foreach (var item in p.SqlWhere)
                {
                    iquery = iquery.Where(item);
                }
                //总金额
                decimal SumAmoney = iquery.Sum(o => o.TotalMoney);
                ViewBag.SumAmoney = SumAmoney;
                //我的总佣金
                decimal? SumMyProfit = Entity.OrderProfitLog.Where(o => o.Agent == BasicAgent.Id&&o.OrderType==31).Join(iquery, op => op.TNum, o => o.TNum, (op, o) => op.Profit).Sum(o => (decimal?)o);
                ViewBag.SumMyProfit = SumMyProfit;
            }

            //商户查询
            IList<JobOrders> List = JobOrdersList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            Dictionary<string, decimal> DicOrderProfitLog = new Dictionary<string, decimal>();
            IList<Users> UsersList = Entity.Users.Where(n => UId.Contains(n.Id)).ToList();
            foreach (var item in JobOrdersList)
            {
                DicOrderProfitLog.Add(item.TNum, Entity.OrderProfitLog.Where(o => o.TNum == item.TNum && o.Agent == BasicAgent.Id).Select(o => o.Profit).FirstOrDefault());
            }
            ViewBag.DicOrderProfitLog = DicOrderProfitLog;
            ViewBag.UsersList = UsersList;
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.AgentId == AdminUser.AgentId).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.BasicAgent = BasicAgent;
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            ViewBag.SysSet = SysSet;
            ViewBag.LowerLevel = LowerLevel;
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.STime = STime;
            ViewBag.ETime = ETime;
            return View();
        }
    }
}
