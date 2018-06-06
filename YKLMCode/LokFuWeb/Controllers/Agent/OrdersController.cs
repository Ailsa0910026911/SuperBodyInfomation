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
using Newtonsoft.Json;
namespace LokFu.Areas.Agent.Controllers
{
    public class OrdersController : BaseController
    {
        public bool IsAll = false;
        public ActionResult Index(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int? LowerLevel, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            if (IsFirst == 0)
            {
                if (checkPower("ALL"))
                {
                    IsAll = true;
                }
                ViewBag.IsAll = IsAll;
                PageOfItems<Orders> OrdersList1 = new PageOfItems<Orders>(new List<Orders>(), 0, 10, 0, new Hashtable());
                ViewBag.OrdersList = OrdersList1;
                ViewBag.Orders = Orders;
                //查询对应的商户
                List<int> UId1 = OrdersList1.Select(o => o.UId).Distinct().ToList();
                ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId1.Contains(n.Id)).ToList();
                ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
                ViewBag.IsShowSupAgent = false;
                ViewBag.BasicAgent = BasicAgent;
                SysSet SysSet1 = Entity.SysSet.FirstOrNew();
                ViewBag.SysSet = SysSet1;
                ViewBag.Entity = Entity;
                ViewBag.LowerLevel = 0;
                ViewBag.Edit = this.checkPower("Edit");
                return View();
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
            if (!IsAll)
            {
                p.SqlWhere.Add(f => f.AId == AdminUser.Id);//交易所属用户
            }
            else
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
            #region 筛选条件
            if (!Orders.OrderAddress.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OrderAddress.Contains(Orders.OrderAddress)); }
            if (!Orders.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == Orders.TNum); }
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
            if (!Orders.TType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.TType == Orders.TType);//读取对应的类型
                if (Orders.TType == 1)
                {
                    if (!Orders.TState.IsNullOrEmpty())
                    {
                        switch (Orders.TState)
                        {
                            case 1://未付
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                break;
                            case 2://已付
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                                break;
                            case 3://待传证照
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                                break;
                            case 4://待审核
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                                break;
                            case 5://审核失败
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 4);
                                break;
                            case 6://退单
                                p.SqlWhere.Add(f => f.TState == 4);
                                break;
                            case 7://待入帐
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 0);
                                break;
                        }
                    }
                }
                if (Orders.TType == 2)
                {
                    if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                }
                if (Orders.TType == 3)
                {
                    switch (Orders.TState)
                    {
                        case 1://未付
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                            break;
                        case 2://已付
                            p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                            break;
                        case 3://待传证照
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                            break;
                        case 4://待审核
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                            break;
                        case 5://审核失败
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 4);
                            break;
                    }
                }
                if (Orders.TType == 5)
                {
                    if (!Orders.TState.IsNullOrEmpty())
                    {
                        switch (Orders.TState)
                        {
                            case 99://未付
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                break;
                            case 1://处理中
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 0);
                                break;
                            case 2://已汇出
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 2);
                                break;
                            case 4://出款中
                                p.SqlWhere.Add(f => (f.TState == 2 && f.PayState == 2 && f.IdCardState == 0) || (f.TState == 2 && f.PayState == 1 && f.IdCardState == 3));
                                break;
                            case 3://审核失败
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 4);
                                break;
                            case 5://退款中
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 3);
                                break;
                            case 6://已退款
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 4);
                                break;
                            case 7://待传身份证
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                                break;
                            case 8://已传身份证
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                                break;
                        }
                    }
                }
                if (Orders.TType == 6)
                {
                    if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                }
                if (Orders.TType == 7 || Orders.TType == 8 || Orders.TType == 9)
                {
                    if (!Orders.TState.IsNullOrEmpty())
                    {
                        if (Orders.TState == 99)
                        {
                            p.SqlWhere.Add(f => f.TState == 0);
                        }
                        if (Orders.TState == 1)
                        {
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                        }
                        if (Orders.TState == 2)
                        {
                            p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                        }
                        if (Orders.TState == 3)//退单
                        {
                            p.SqlWhere.Add(f => f.TState == 4);
                        }
                        if (Orders.TState == 4)//待入帐
                        {
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 0);
                        }
                        if (Orders.TState == 5)//待审核
                        {
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 1 && f.IdCardState == 2);
                        }
                        if (Orders.TState == 6)//待传证照
                        {
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 1 && f.IdCardState == 1);
                        }
                        if (Orders.TState == 7)//审核失败
                        {
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 1 && f.IdCardState == 4);
                        }
                    }
                }
                if (Orders.TType == 10)
                {
                    if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                }
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
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.BasicAgent = BasicAgent;
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            ViewBag.SysSet = SysSet;
            ViewBag.Entity = Entity;
            ViewBag.LowerLevel = LowerLevel;
            ViewBag.Edit = this.checkPower("Edit");
            return View();
        }

        public ActionResult Edit(Orders Orders)
        {
            if (Orders.Id != 0) Orders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "查询的交易不存在";
                return View("Error");
            }
            if (checkPower("ALL"))
            {
                IsAll = true;
            }
            ViewBag.IsAll = IsAll;
            if (!IsBelongToAgent(Orders.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == Orders.UId);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.AId);
            ViewBag.OrderProfitLog = Entity.OrderProfitLog.FirstOrNew(o => o.LogType == 2 && o.Agent == this.AdminUser.AgentId && o.TNum == Orders.TNum);
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            ViewBag.OrderProfitLogList = this.checkPower("Orders", "OrderProfitLogList");
            return View();
        }

        public ActionResult EditDanBao(Orders Orders)
        {
            Orders baseOrders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "查询的交易不存在";
                return View("Error");
            }
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == baseOrders.Agent);
            if (SysAgent == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(SysAgent.Id))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }

            ViewBag.baseOrders = baseOrders;
            ViewBag.Orders = Orders;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View("DanBao");
        }

        [ValidateInput(false)]
        public ActionResult Add(Orders Orders, List<string> Contract)
        {

            if (Contract==null || Orders.DDUserRm.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写相关数据";
                return View("Error");
            }
            if (Orders.CardUpType == 2)
            {
                if (this.BasicAgent.Tier != 1)
                {
                    ViewBag.ErrorMsg = "只有一级代理商才能操作";
                    return View("Error");
                }
            }
            string pic = "pic000";
            foreach (var info in Contract)
            {
                pic = pic + "," + info;
            }
            pic = pic.Replace("pic000,", "");
            Orders.UserCardPic = pic;
            Orders baseOrders = null;
            if (this.BasicAgent.Tier == 1)
            {
                baseOrders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            }
            else
            {
                baseOrders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id && n.Agent == this.BasicAgent.Id);
            }

            if (baseOrders.IdCardState == 1 || baseOrders.IdCardState == 4)
            {
                Orders = Request.ConvertRequestToModel<Orders>(Orders, Orders);
                if (Orders.UserCardPic == "System.Web.HttpPostedFileWrapper" || Orders.UserCardPic == null)
                {
                    ViewBag.ErrorMsg = "文件类型错误";
                    return View("Error");
                }
                baseOrders.UserCardPic = Orders.UserCardPic;
                baseOrders.BankCardId = Orders.BankCardId;
                baseOrders.UserCardId = Orders.UserCardId;
                baseOrders.IdCardState = 2;
                baseOrders.CardUpType = Orders.CardUpType;
                baseOrders.DDUserRm = Orders.DDUserRm;
                baseOrders.CardUpdateTime = DateTime.Now;
                if (baseOrders.TState == 3)
                {
                    baseOrders.TState = 1;
                }
                //调单日志
                var OrdersDDLog = new OrdersDDLog()
                {
                    AddTime = DateTime.Now,
                    LastTime = baseOrders.DDLastTime,
                    LogType = 2,
                    OpName = this.AdminUser.TrueName,
                    Remark = baseOrders.DDUserRm,
                    TNum = baseOrders.TNum,
                    Img = baseOrders.UserCardPic,
                };
                this.Entity.OrdersDDLog.AddObject(OrdersDDLog);
                Entity.SaveChanges();
                baseOrders.SendMsg(Entity);
            }
            return this.View("ReloadFrame");
            //BaseRedirect();
        }

        //public ActionResult OrderProfitLogList(string orderNum)
        //{
        //    if (!orderNum.IsNullOrEmpty())
        //    {
        //        Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == orderNum );
        //        if (Orders == null)
        //        {
        //            ViewBag.ErrorMsg = "查询的交易不存在";
        //            return View("Error");
        //        }
        //        /*
        //         * 订单号=订单号 并且 所属id==我的代理id 或者 记录类型==用户
        //         * 结果：代理只能看到自己的利润和用户的利润
        //         */
        //        List<OrderProfitLog> OrderProfitLogs = new List<OrderProfitLog>();
        //        if (Orders.Agent == this.BasicAgent.Id)
        //        {
        //            OrderProfitLogs = Entity.OrderProfitLog.Where(o => o.TNum == orderNum && (o.Agent == this.BasicAgent.Id || o.LogType == 1)).OrderBy(o => o.AddTime).ToList();
        //        }
        //        else
        //        {
        //            OrderProfitLogs = Entity.OrderProfitLog.Where(o => o.TNum == orderNum && o.Agent == this.BasicAgent.Id).OrderBy(o => o.AddTime).ToList();
        //        }
        //        var GroupByIds = OrderProfitLogs.GroupBy(o => o.LogType, o => o.UId).ToArray();
        //        List<Users> users = null;
        //        foreach (var item in GroupByIds)
        //        {
        //            switch (item.Key)
        //            {
        //                case 1:
        //                    users = Entity.Users.Where(o => item.Contains(o.Id)).ToList();
        //                    break;
        //            }
        //        }
        //        foreach (var item in OrderProfitLogs)
        //        {
        //            switch (item.LogType)
        //            {
        //                case 1:
        //                    item.users = users.FirstOrDefault(o => o.Id == item.UId);
        //                    break;
        //            }
        //        }
        //        this.ViewBag.OrderProfitLogs = OrderProfitLogs;
        //    }
        //    return this.View();
        //}

        public ActionResult IndexOrdersDDLog(OrdersDDLog OrdersDDLog)
        {
            List<OrdersDDLog> OrdersDDLogList = null;
            if (!OrdersDDLog.TNum.IsNullOrEmpty())
            {
                OrdersDDLogList = this.Entity.OrdersDDLog.Where(o => o.TNum == OrdersDDLog.TNum).OrderByDescending(o => o.AddTime).ToList();
            }
            ViewBag.Orders = Entity.Orders.FirstOrDefault(o => o.TNum == OrdersDDLog.TNum);
            this.ViewBag.OrdersDDLogList = OrdersDDLogList;
            return View();
        }

        public ActionResult OrdersRiskIndex(Orders Orders, EFPagingInfo<Orders> p, int IsFirst = 0)
        {
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            if (IsFirst == 0)
            {
                var AgentList1 = this.BasicAgent.GetSupAgent(this.Entity);
                PageOfItems<Orders> OrdersList1 = new PageOfItems<Orders>(new List<Orders>(), 0, 10, 0, new Hashtable());
                ViewBag.Orders = Orders;
                ViewBag.OrdersList = OrdersList1;
                ViewBag.UsersList = new List<Users>();
                ViewBag.AgentList = AgentList1;
                return View();
            }
            if (!Orders.DDAuto.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.DDAuto == Orders.DDAuto); }
            if (!Orders.IdCardState.IsNullOrEmpty())
            {
                if (Orders.IdCardState == 99)
                {
                    p.SqlWhere.Add(f => f.IdCardState > 0);
                }
                else
                {
                    p.SqlWhere.Add(f => f.IdCardState == Orders.IdCardState);
                }
            }
            else
            {
                Orders.IdCardState = 1;
                p.SqlWhere.Add(f => f.IdCardState == Orders.IdCardState);
            }
            if (!Orders.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == Orders.TNum); }
            if (!Orders.STime.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CardAddTime >= Orders.STime); }
            if (!Orders.ETime.IsNullOrEmpty())
            {
                var ETime = Orders.ETime.AddSeconds(-1);
                p.SqlWhere.Add(f => f.CardAddTime <= ETime);
            }
            //下级代理
            var AgentList = this.BasicAgent.GetSupAgent(this.Entity);
            if (this.BasicAgent.Tier == 1)
            {
                var aids = AgentList.Select(o => o.Id).ToList();
                p.SqlWhere.Add(o => aids.Contains(o.Agent));
            }
            else
            {
                p.SqlWhere.Add(o => o.Agent == this.BasicAgent.Id);
            }
            p.OrderByList.Add("CardAddTime", "DESC");
            IPageOfItems<Orders> OrdersList = Entity.Selects<Orders>(p);
            ViewBag.Orders = Orders;
            ViewBag.OrdersList = OrdersList;
            //查询对应的商户
            List<int> UId = OrdersList.Select(o => o.UId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.AgentList = AgentList;
            ViewBag.SaveAssureImg = this.checkPower("SaveAssureImg");
            return View("OrdersRiskIndex");
        }

        public ActionResult IndexOrderProfitLog(string tnum)
        {
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == tnum );
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "查询的交易不存在";
                return View("Error");
            }
            var query = this.Entity.OrderProfitLog.Where(o => o.TNum == tnum);
            if (Orders.Agent == this.BasicAgent.Id)
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

            var Agents = OrderProfitLog.Select(o => o.Agent).ToList();
            var AgentsList = this.Entity.SysAgent.Where(o => Agents.Contains(o.Id)).ToDictionary(o => o.Id, o => o.Name);
            ViewBag.AgentsList = AgentsList;

            return View();
        }

    }
    public class Attachs
    {
        public string Id { get; set; }
        public string SaveFileName { get; set; }
    }
}
