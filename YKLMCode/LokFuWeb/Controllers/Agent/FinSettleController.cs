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
    public class FinSettleController : BaseController
    {
        public bool IsAll = false;
       
        public ActionResult Index(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int? LowerLevel, int IsFirst = 0)
        {
            if (IsFirst==0)
            {
                if (IsShowSupAgent == null) IsShowSupAgent = false;
                if (checkPower("ALL"))
                {
                    IsAll = true;
                }
                LowerLevel = LowerLevel == null ? 0 : LowerLevel;
                ViewBag.IsAll = IsAll;
                PageOfItems<Orders> OrdersList1 = new PageOfItems<Orders>(new List<Orders>(), 0, 10, 0, new Hashtable());
                ViewBag.OrdersList = OrdersList1;
                ViewBag.Orders = Orders;
                //查询对应的商户
                List<int> UId1 = OrdersList1.Select(o => o.UId).Distinct().ToList();
                ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId1.Contains(n.Id)).ToList();
                ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
                ViewBag.IsShowSupAgent = IsShowSupAgent;
                ViewBag.BasicAgent = BasicAgent;
                SysSet SysSet1 = Entity.SysSet.FirstOrNew();
                ViewBag.SysSet = SysSet1;
                ViewBag.Entity = Entity;
                ViewBag.LowerLevel = LowerLevel;
                return View();
            }
            //if (Orders.STime.IsNullOrEmpty())
            //{
            //    Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //}
            //if (Orders.ETime.IsNullOrEmpty())
            //{
            //    Orders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //}
            //if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
            //{
            //    DateTime ETime = Orders.ETime;
            //    p.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
            //}

            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            if (checkPower("ALL"))
            {
                IsAll = true;
            }
            ViewBag.IsAll = IsAll;
            /*有没有指定交易所属代理
             * 有:校验是否从属关系
             * 无：指定当前代理
             */
            if (!Orders.Agent.IsNullOrEmpty())
            {
                if (!IsBelongToAgent(Orders.Agent))
                {
                    ViewBag.ErrorMsg = "只能查询当前用户下属代理的交易";
                    return View("Error");
                }
            }
            else
            {
                Orders.Agent = this.BasicAgent.Id;
            }
            LowerLevel = LowerLevel == null ? 0 : LowerLevel;
            //没有"管理所有"权限的只能看到操作员自己的数据
            //if (!IsAll)
            //{
            //    p.SqlWhere.Add(f => f.AId == AdminUser.Id);//交易所属用户
            //}
            //else
            {
                IList<SysAgent> SysAgentList = null;
                if ((bool)IsShowSupAgent)
                {
                    IList<int> UID = new List<int>();
                    if (LowerLevel != 0)
                    {
                        SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == LowerLevel).FirstOrNew();
                        SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    }
                    else
                    {
                        SysAgentList = BasicAgent.GetSupAgent(Entity);//获取所有下级代理商信息
                    }
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == Orders.Agent);//交易所属代理
                }
            }
            #region 筛选条件
            if (!Orders.OrderAddress.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OrderAddress.Contains(Orders.OrderAddress)); }
            if (!Orders.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum==Orders.TNum); }
            if (!Orders.LagEntryDay.IsNullOrEmpty())
            {
                if (Orders.LagEntryDay == 99)
                {
                    p.SqlWhere.Add(f => f.LagEntryDay == 0);
                }
                else
                {
                    p.SqlWhere.Add(f => f.LagEntryDay > 0);
                }
            }
            if (!Orders.TrunType.IsNullOrEmpty())
            {
                if (Orders.TrunType == 1)
                {
                    p.SqlWhere.Add(f => f.TrunType == 1);
                }
                else
                {
                    p.SqlWhere.Add(f => f.TrunType == 0);
                }
            }

            if (!Orders.TName.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(Orders.TName) || n.NeekName.Contains(Orders.TName) || n.UserName == Orders.TName).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!Orders.PayWay.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.PayWay == Orders.PayWay); }
            if (!Orders.STime.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.AddTime >= Orders.STime);
            }
            if (!Orders.ETime.IsNullOrEmpty())
            {
                DateTime ETime = Orders.ETime.AddHours(23).AddMinutes(59).AddSeconds(59);
                p.SqlWhere.Add(f => f.AddTime <= ETime);
            }
            #region 交易类型条件判断
            p.SqlWhere.Add(f => f.TType == 2);//读取对应的类型
            if (Orders.TType == 2)
            {
                if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
            }
            #endregion
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Orders> OrdersList = Entity.Selects<Orders>(p);
            ViewBag.OrdersList = OrdersList;
            ViewBag.Orders = Orders;
            //查询对应的商户
            List<int> UId = OrdersList.Select(o => o.UId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            //因为上面判段过了，这里其实是不用的
            //if (checkPower("ALL"))
            //{
            //    ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && n.Agent == AdminUser.AgentId && UId.Contains(n.Id)).ToList();
            //}
            //else
            //{
            //    ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && n.AId == AdminUser.Id && UId.Contains(n.Id)).ToList();
            //}
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.BasicAgent = BasicAgent;
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            ViewBag.SysSet = SysSet;
            ViewBag.Entity = Entity;
            ViewBag.LowerLevel = LowerLevel;
            return View();
        }
    }
}
