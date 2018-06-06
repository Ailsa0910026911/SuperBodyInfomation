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
    public class OrdersPayController : BaseController
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Orders"></param>
        /// <param name="p"></param>
        /// <param name="IsShowSupAgent">是否显示下级</param>
        /// <param name="LowerLevel">下级代理商</param>
        /// <returns></returns>
        public ActionResult Index(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int? LowerLevel, int IsFirst = 0)
        {
            bool IsAll = false;
            if (checkPower("ALL"))
            {
                IsAll = true;
            }
            ViewBag.IsAll = IsAll;
            #region 条件校验
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            LowerLevel = LowerLevel == null ? 0 : LowerLevel;
            p.SqlWhere.Add(f => f.PayState != 0);
            p.SqlWhere.Add(f => f.TState == 2);
            if (!Orders.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == Orders.TNum); }
            if (!Orders.TName.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName == Orders.TName || n.NeekName == Orders.TName || n.UserName == Orders.TName).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!Orders.TType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TType == Orders.TType); }
            if (!Orders.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == Orders.AId); }
            if (!Orders.AgentState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentState == Orders.AgentState); }
            if (!Orders.STime.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.PayTime > Orders.STime);
            }
            else
            {
                Orders.STime = DateTime.Now.AddMonths(-1);
            }
            if (!Orders.ETime.IsNullOrEmpty())
            {
                DateTime ETime = Orders.ETime;
                p.SqlWhere.Add(f => f.PayTime < ETime);
            }
            else
            {
                Orders.ETime = DateTime.Now;
            }
            TimeSpan TS = Orders.ETime.Subtract(Orders.STime);
            int Days = TS.Days;
            if (Days > 31)
            {
                ViewBag.ErrorMsg = "统计时间间隔不能超过31天！";
                return View("Error");
            }
            if (IsAll)
            {
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
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == BasicAgent.Id);//读取全部分支机构
                }
            }
            else
            {
                p.SqlWhere.Add(f => f.AId == AdminUser.Id);//读取用户
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Orders> OrdersList = null;
            if (IsFirst == 0)
            {
                OrdersList = new PageOfItems<Orders>(new List<Orders>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                OrdersList = Entity.Selects<Orders>(p);
            }
            
            ViewBag.OrdersList = OrdersList;
            ViewBag.Orders = Orders;
            //统计查询
            if (OrdersList.TotalCount > 0)
            {
                var iquery = Entity.Orders.AsQueryable();
                foreach (var item in p.SqlWhere)
                {
                    iquery = iquery.Where(item);
                }
                //总金额
                decimal SumAmoney = iquery.Sum(o => o.Amoney);
                ViewBag.SumAmoney = SumAmoney;
                //我的总佣金
                decimal? SumMyProfit = Entity.OrderProfitLog.Where(o => o.Agent == BasicAgent.Id).Join(iquery, op => op.TNum, o => o.TNum, (op, o) => op.Profit).Sum(o => (decimal?)o);
                ViewBag.SumMyProfit = SumMyProfit;
            }

            //商户查询
            IList<Orders> List = OrdersList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            Dictionary<string, decimal> DicOrderProfitLog = new Dictionary<string, decimal>();
            IList<Users> UsersList = Entity.Users.Where(n => UId.Contains(n.Id)).ToList();
            foreach (var item in OrdersList)
            {
                DicOrderProfitLog.Add(item.TNum, Entity.OrderProfitLog.Where(o => o.TNum == item.TNum && o.Agent == BasicAgent.Id).Select(o => o.Profit).FirstOrDefault());
            }
            ViewBag.DicOrderProfitLog = DicOrderProfitLog;
            ViewBag.UsersList = UsersList;
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.AgentId == AdminUser.AgentId).ToList();
            //ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.BasicAgent = BasicAgent;
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            ViewBag.SysSet = SysSet;
            ViewBag.LowerLevel = LowerLevel;
            ViewBag.Edit = this.checkPower("Edit");
            return View();
        }
    }
}
