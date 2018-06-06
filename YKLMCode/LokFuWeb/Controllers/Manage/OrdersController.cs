using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.PayMent.ALF2FPAY;
using LokFu.PayMent.WxPayAPI;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class OrdersController : BaseController
    {
        public ActionResult Index(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int IsFirst = 0, int TimeType = 1)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }

            if (TimeType == 1)
            {
                if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
                {
                    DateTime ETime = Orders.ETime.AddMilliseconds(999);
                    p.SqlWhere.Add(f => f.AddTime > Orders.STime && f.AddTime < ETime);
                }
            }
            else
            {
                if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
                {
                    DateTime ETime = Orders.ETime.AddMilliseconds(999);
                    p.SqlWhere.Add(f => f.InTimed > Orders.STime && f.InTimed < ETime);
                }
            }
            TimeSpan TS = Orders.ETime.Subtract(Orders.STime);
            int Days = TS.Days;
            if (Days > 31)
            {
                ViewBag.ErrorMsg = "统计时间间隔不能超过31天！";
                return View("Error");
            }

            p = Condition(Orders, p, IsShowSupAgent, TimeType);
            
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
            //副表信息
            List<int> UId = OrdersList.Select(o => o.UId).Distinct().ToList();
            List<int> AgentIds = OrdersList.Select(o => o.Agent).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.ResideSysAgentList = Entity.SysAgent.Where(n => AgentIds.Contains(n.Id)).ToList();

            //其他信息
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级

            //权限相关
            ViewBag.OrdersRepair = this.checkPower("OrdersRepair");
            ViewBag.Chargeback = this.checkPower("Chargeback");
            ViewBag.DiaoDanSave = this.checkPower("DiaoDanSave");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.TimeType = TimeType;

            return View();
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int TimeType = 1)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }

            if (TimeType == 1)
            {
                if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
                {
                    DateTime ETime = Orders.ETime.AddMilliseconds(999);
                    p.SqlWhere.Add(f => f.AddTime > Orders.STime && f.AddTime < ETime);
                }
            }
            else
            {
                if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
                {
                    DateTime ETime = Orders.ETime.AddMilliseconds(999);
                    p.SqlWhere.Add(f => f.InTimed > Orders.STime && f.InTimed < ETime);
                }
            }

            //TimeSpan TS = Orders.ETime.Subtract(Orders.STime);
            //int Days = TS.Days;
            //if (Days > 31)
            //{
            //    return null;
            //}

            p = Condition(Orders, p, IsShowSupAgent, TimeType);
            var Iquery = this.Entity.Orders.AsQueryable();
            foreach (var item in p.SqlWhere)
            {
                Iquery = Iquery.Where(item);
            }
            ViewBag.SumAmoney = Iquery.Sum(o => (decimal?)o.Amoney) ?? 0m;
            ViewBag.SumPoundage = Iquery.Sum(o => (decimal?)o.Poundage) ?? 0m;
            ViewBag.Count = Iquery.Count();

            return this.View();
        }

        public ActionResult Info(Orders Orders)
        {
            if (!Orders.Id.IsNullOrEmpty())
            {
                Orders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            }
            else if (!Orders.TNum.IsNullOrEmpty())
            {
                Orders = Entity.Orders.FirstOrDefault(n => n.TNum == Orders.TNum);
            }
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            //Orders.TState = 99;
            ViewBag.Orders = Orders;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == Orders.UId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Orders.Agent);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.AId);
            ViewBag.FinAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.FId);
            ViewBag.Save = this.checkPower("Save");
            ViewBag.OrderProfitLogList = this.checkPower("OrderProfitLogList");
            ViewBag.UserTrailIndex = this.checkPower("UserTrail", "Index");
            return View("Edit");
        }

        public ActionResult Edit(Orders Orders)
        {
            ViewBag.Save = this.checkPower("Save");
            ViewBag.OrderProfitLogList = this.checkPower("OrderProfitLogList");
            ViewBag.UserTrailIndex = this.checkPower("UserTrail", "Index");
            ViewBag.BasicDescList1 = GetBasicDescList(BasicCodeEnum.Ddsh);
            ViewBag.BasicDescList2 = GetBasicDescList(BasicCodeEnum.Dhnbsh);
            if (Orders.Id != 0) Orders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (Orders.PayState != 1)
            {
                ViewBag.ErrorMsg = "交易未支付，无需要审核！";
                return View("Error");
            }
            if (Orders.DDAuto == 1 && Orders.TState == 2)
            {
                ViewBag.ErrorMsg = "自动调单：交易已成功，无需要审核！";
                return View("Error");
            }
            if (Orders.DDAuto == 1 && Orders.TState == 3)
            {
                ViewBag.ErrorMsg = "自动调单：交易失败，无需要审核！";
                return View("Error");
            }
            if (Orders.DDAuto == 2 && Orders.TState != 2 && Orders.IdCardState != 2)
            {
                ViewBag.ErrorMsg = "手动调单：交易未成功，无需要审核！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == Orders.UId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Orders.Agent);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.AId);
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View("../Orders/Edit");
        }

        [ValidateInput(false)]
        public ActionResult Save(Orders Orders)
        {
            if (!(Orders.IdCardState == 3 || Orders.IdCardState == 4 || Orders.IdCardState == 0))
            {
                ViewBag.ErrorMsg = "请选择审核结果";
                return View("Error");
            }
            Orders baseOrders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            #region 审核
            if (baseOrders.TState == 1 && (baseOrders.IdCardState == 2 || baseOrders.IdCardState == 0))
            {
                baseOrders.TState = Orders.TState;
                baseOrders.IdCardState = Orders.IdCardState;
                baseOrders.InternalRm = Orders.InternalRm;
                
                #region 提现审核
                if (baseOrders.TType == 2)
                { //提现订单
                    OrderCash OrderCash = Entity.OrderCash.FirstOrNew(n => n.OId == baseOrders.TNum);
                    OrderCash.OrderState = baseOrders.TState;
                    OrderCash.AuditTime = DateTime.Now;
                    Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == baseOrders.UId);
                    if (baseOrders.TState == 2)
                    {//交易成功
                        //扣除冻结金额
                        if (baseUsers.Frozen >= baseOrders.Amoney)
                        {
                            //帐户变动记录
                            int USERSID = baseUsers.Id;
                            string TNUM = baseOrders.TNum;
                            decimal PAYMONEY = baseOrders.Amoney;
                            short OTYPE = 4;
                            string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, OTYPE, "");
                            if (SP_Ret != "3")
                            {
                                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, OTYPE, PAYMONEY, SP_Ret), "SP_UsersMoney");
                                ViewBag.ErrorMsg = "扣款失败";
                                return View("Error");
                            }
                            if (BasicSet.CashPayWay == 1)
                            {
                                if (BasicSet.CashPayWay == 1 && OrderCash.TrunType == 0 && OrderCash.Amoney <= BasicSet.QCash0)
                                {
                                    //自动出款
                                    OrderCash.PayCash(baseOrders, Entity);//去付款
                                }
                            }
                        }
                        else
                        {
                            Response.Redirect("/Manage/home/error.html?IsAjax=" + Request["IsAjax"] + "&msg=冻结金额少于提现金额~");
                            return null;
                        }
                    }
                    if (baseOrders.TState == 3)//交易失败
                    {
                        baseOrders.Remark = Orders.Remark;
                        OrderCash.Remark = Orders.Remark;
                        //解冻结金额
                        if (baseUsers.Frozen >= baseOrders.Amoney)
                        {
                            //帐户变动记录
                            int USERSID = baseUsers.Id;
                            string TNUM = baseOrders.TNum;
                            decimal PAYMONEY = baseOrders.Amoney;
                            string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 5, "");
                            if (SP_Ret != "3")
                            {
                                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 5, PAYMONEY, SP_Ret), "SP_UsersMoney");
                                ViewBag.ErrorMsg = "退款失败";
                                return View("Error");
                            }
                            //T0时增加配额
                            if (OrderCash.TrunType == 0)
                            {
                                decimal Money = OrderCash.Amoney - (decimal)OrderCash.UserRate;
                                DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                                TaskTimeSet TaskTimeSet = Entity.TaskTimeSet.FirstOrDefault(n => n.ODate == Today);
                                if (TaskTimeSet != null)
                                {
                                    if (TaskTimeSet.UsedMoney >= Money)
                                    {
                                        TaskTimeSet.UsedMoney -= Money;
                                    }
                                    else
                                    {
                                        TaskTimeSet.UsedMoney = 0;
                                    }
                                    Entity.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            Response.Redirect("/Manage/home/error.html?IsAjax=" + Request["IsAjax"] + "&msg=冻结金额少于提现金额~");
                            return null;
                        }
                    }
                }
                #endregion
                #region 转帐审核
                if (baseOrders.TType == 3)
                {
                    //转帐订单
                    OrderTransfer OrderTransfer = Entity.OrderTransfer.FirstOrNew(n => n.OId == baseOrders.TNum);
                    OrderTransfer.OrderState = baseOrders.TState;
                    baseOrders.Remark = Orders.Remark;
                    OrderTransfer.Remark = Orders.Remark;
                    if (baseOrders.IdCardState == 3 || baseOrders.IdCardState == 0)//审核通过才入帐
                    {
                        //获取收款用户信息
                        Users Users = Entity.Users.FirstOrDefault(n => n.Id == OrderTransfer.RUId);
                        if (Users != null)
                        {
                            //帐户变动记录
                            int USERSID = Users.Id;
                            string TNUM = baseOrders.TNum;
                            decimal PAYMONEY = OrderTransfer.PayMoney;
                            string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 1, "");
                            if (SP_Ret != "3")
                            {
                                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 1, PAYMONEY, SP_Ret), "SP_UsersMoney");
                                ViewBag.ErrorMsg = "入款失败";
                                return View("Error");
                            }
                        }
                        //======分润======
                        OrderTransfer = OrderTransfer.PayAgent(Entity, 1);
                        Orders.AgentPayGet = (decimal)OrderTransfer.AgentPayGet;
                        Entity.SaveChanges();
                    }
                }
                #endregion
                #region 付房租审核
                if (baseOrders.TType == 5)
                { //付房租订单
                    OrderHouse OrderHouse = Entity.OrderHouse.FirstOrNew(n => n.OId == baseOrders.TNum);
                    OrderHouse.OrderState = baseOrders.TState;
                    baseOrders.Remark = Orders.Remark;
                    OrderHouse.Remark = Orders.Remark;
                    if (baseOrders.TState == 2)
                    {
                        if (BasicSet.CashPayWay == 1 && OrderHouse.TrunType == 0 && OrderHouse.Amoney <= BasicSet.QCash0)
                        {
                            //自动出款
                            OrderHouse.PayCash(baseOrders, Entity);//去付款
                        }
                    }
                    if (baseOrders.TState == 3 && Orders.PayState == 3)//付房租审核失败，走退款流程，20150715新增加
                    {
                        OrderHouse.OrderState = 2;
                        baseOrders.TState = 2;
                        OrderHouse.PayState = 3;
                        baseOrders.PayState = 3;
                        baseOrders.IdCardState = 0;
                        Entity.SaveChanges();
                        //T0时增加配额
                        if (OrderHouse.TrunType == 0)
                        {
                            decimal Money = OrderHouse.PayMoney;
                            DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                            TaskTimeSet TaskTimeSet = Entity.TaskTimeSet.FirstOrDefault(n => n.ODate == Today);
                            if (TaskTimeSet != null)
                            {
                                if (TaskTimeSet.UsedMoney >= Money)
                                {
                                    TaskTimeSet.UsedMoney -= Money;
                                }
                                else
                                {
                                    TaskTimeSet.UsedMoney = 0;
                                }
                                Entity.SaveChanges();
                            }
                        }
                    }
                }
                #endregion

                //if (baseOrders.TType == 6)
                //{ //升级订单
                //    PayConfigOrder PayConfigOrder = Entity.PayConfigOrder.FirstOrNew(n => n.OId == baseOrders.TNum);
                //    PayConfigOrder.OrderState = baseOrders.TState;
                //    baseOrders.Remark = Orders.Remark;
                //    PayConfigOrder.Remark = Orders.Remark;
                //}
                //if (baseOrders.TType == 10)
                //{ //代理订单
                //    DaiLiOrder DaiLiOrder = Entity.DaiLiOrder.FirstOrNew(n => n.OId == baseOrders.TNum);
                //    DaiLiOrder.OrderState = baseOrders.TState;
                //    baseOrders.Remark = Orders.Remark;
                //    DaiLiOrder.Remark = Orders.Remark;
                //}
            }
            #endregion
            Entity.SaveChanges();
            baseOrders.SendMsg(Entity);//发送消息类
            BaseRedirect();
            return null;
        }

        /// <summary>
        /// 分润明细
        /// </summary>
        /// <param name="orderNum"></param>
        /// <returns></returns>
        public ActionResult OrderProfitLogList(string orderNum)
        {
            if (!orderNum.IsNullOrEmpty())
            {
                List<OrderProfitLog> OrderProfitLogs = Entity.OrderProfitLog.Where(o => o.TNum == orderNum).OrderBy(o => o.AddTime).ToList();
                List<int> UIds = new List<int>();
                foreach (var p in OrderProfitLogs.Where(n => n.LogType == 1)) {
                    UIds.Add(p.UId);
                }
                List<Users> users = Entity.Users.Where(o => UIds.Contains(o.Id)).ToList();
                List<int> Agents = new List<int>();
                foreach (var p in OrderProfitLogs.Where(n => n.LogType == 2))
                {
                    Agents.Add(p.Agent);
                }
                List<SysAgent> sysAgents = Entity.SysAgent.Where(o => Agents.Contains(o.Id)).ToList(); ;
                foreach (var item in OrderProfitLogs)
                {
                    switch (item.LogType)
                    {
                        case 1:
                            item.users = users.FirstOrNew(o => o.Id == item.UId);
                            break;
                        case 2:
                            item.sysAgent = sysAgents.FirstOrNew(o => o.Id == item.Agent);
                            break;
                    }
                }
                this.ViewBag.OrderProfitLogs = OrderProfitLogs;
            }
            return this.View();
        }

        #region 发起调单

        /// <summary>
        /// 发起调单
        /// </summary>
        /// <param name="Orders"></param>
        /// <returns></returns>
        public ActionResult EditDiaoDan(int id)
        {
            var Orders = Entity.Orders.FirstOrDefault(n => n.Id == id);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            ViewBag.BasicDescList = GetBasicDescList(BasicCodeEnum.Dddd);
            ViewBag.Orders = Orders;
            return View();
        }

        /// <summary>
        /// 发起调单保存
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public object DiaoDanSave(Orders Orders)
        {
            #region 校验及初始化
            if (Orders.Remark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写调单说明";
                return View("Error");
            }
            if (Orders.DDLastTime.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写过期时间";
                return View("Error");
            }
            Orders baseOrders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            if (!baseOrders.IfCanDianDan())
            {
                ViewBag.ErrorMsg = "不满足调单条件";
                return View("Error");
            }
            baseOrders.TState = 1;
            #region 银联调单
            if (baseOrders.TType == 1)
            { //银联卡支付订单
                OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrNew(n => n.OId == baseOrders.TNum);
                OrderRecharge.OrderState = baseOrders.TState;
                baseOrders.Remark = Orders.Remark;
                OrderRecharge.Remark = Orders.Remark;
                if (baseOrders.FrozenState == 0)
                {
                    //冻结操作(包含了佣金冻结)
                    OrderRecharge.SetFrozen(Entity);
                    baseOrders.FrozenState = 1;
                }
            }
            #endregion
            #region 微信 支付宝 NFC
            if (baseOrders.TType == 7 || baseOrders.TType == 8 || baseOrders.TType == 9)
            { //银联卡支付订单
                OrderF2F OrderF2F = Entity.OrderF2F.FirstOrNew(n => n.OId == baseOrders.TNum);
                OrderF2F.OrderState = baseOrders.TState;
                baseOrders.Remark = Orders.Remark;
                OrderF2F.Remark = Orders.Remark;
                if (baseOrders.FrozenState == 0)
                {
                    //冻结操作(包含了佣金冻结)
                    OrderF2F.SetFrozen(Entity);
                    baseOrders.FrozenState = 1;
                }
            }
            #endregion
            #endregion
            //订单信息修改
            baseOrders.DDAuto = 2;
            baseOrders.CardAddTime = DateTime.Now;//调单时间
            baseOrders.IdCardState = 1;
            baseOrders.DDLastTime = Orders.DDLastTime.GetValueOrDefault();
            baseOrders.InternalRm = Orders.InternalRm;
            //调单记录
            var OrdersDDLog = new OrdersDDLog()
            {
                TNum = baseOrders.TNum,
                LogType = 1,
                Remark = baseOrders.Remark,
                AddTime = DateTime.Now,
                LastTime = baseOrders.DDLastTime,
                OpName = AdminUser.TrueName,
                InteriorRemark = baseOrders.InternalRm,
            };
            Entity.OrdersDDLog.AddObject(OrdersDDLog);
            Entity.SaveChanges();
            BaseRedirect();
            return null;
        }

        /// <summary>
        /// 调单记录
        /// </summary>
        /// <param name="OrdersDDLog"></param>
        /// <returns></returns>
        public ActionResult IndexDDLog(string TNum)
        {
            var Orders = this.Entity.Orders.FirstOrDefault(o=>o.TNum == TNum);
            this.ViewBag.Orders = Orders;
            var OrdersDDLogList = this.Entity.OrdersDDLog.Where(o => o.TNum == TNum).OrderByDescending(o => o.AddTime).ToList();
            this.ViewBag.OrdersDDLogList = OrdersDDLogList;
            return View();
        }

        #endregion

        #region 调单

        /// <summary>
        /// 风险管理
        /// </summary>
        /// <param name="Orders"></param>
        /// <param name="p"></param>
        /// <param name="IsShowSupAgent"></param>
        /// <param name="IsFirst"></param>
        /// <returns></returns>
        public ActionResult OrdersRiskIndex(Orders Orders, EFPagingInfo<Orders> p, bool IsShowSupAgent = false, int IsFirst = 0)
        {
            ViewBag.Add = this.checkPower("Add");
            #region 条件
            if (!Orders.DDAuto.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.DDAuto == Orders.DDAuto); }
            if (!Orders.IdCardState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.IdCardState == Orders.IdCardState); }
            if (!Orders.STime.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CardAddTime >= Orders.STime); }
            if (!Orders.ETime.IsNullOrEmpty())
            {
                var ETime = Orders.ETime.AddSeconds(-1);
                p.SqlWhere.Add(f => f.CardAddTime <= ETime);
            }
            if (!Orders.TNum.IsNullOrEmpty())
            {
                int uid = 0;
                switch (Orders.TrunType)//TrunType在这里表示筛选项目
                {
                    case 1://单号
                        p.SqlWhere.Add(f => f.TNum == Orders.TNum);
                        break;
                    case 2://账户
                        var temp1 = Entity.Users.Where(o => o.UserName == Orders.TNum).Select(o => new { o.Id, o.UserName }).FirstOrDefault();
                        uid = temp1 != null ? temp1.Id : 0;
                        break;
                    case 3://真实姓名
                        var temp2 = Entity.Users.Where(o => o.TrueName == Orders.TNum).Select(o => new { o.Id, o.TrueName }).FirstOrDefault();
                        uid = temp2 != null ? temp2.Id : 0;
                        break;
                    case 4://商户名称
                        var temp3 = Entity.Users.Where(o => o.NeekName == Orders.TNum).Select(o => new { o.Id, o.NeekName }).FirstOrDefault();
                        uid = temp3 != null ? temp3.Id : 0;
                        break;
                }
                if (uid != 0)
                {
                    p.SqlWhere.Add(o => o.UId == uid);
                }
            }

            //是否选择了分支机构
            if (!Orders.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == Orders.Agent).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = new List<int>();
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == Orders.Agent);
                }
            }
            #endregion
            p.SqlWhere.Add(o => o.DDAuto != 0);
            p.OrderByList.Add("CardAddTime", "DESC");
            IPageOfItems<Orders> OrdersList = null;
            if (IsFirst == 0)
            {
                OrdersList = new PageOfItems<Orders>(new List<Orders>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                OrdersList = Entity.Selects<Orders>(p);
            }
            ViewBag.Orders = Orders;
            ViewBag.OrdersList = OrdersList;
            //查询对应的商户
            List<int> UId = OrdersList.Select(o => o.UId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();

            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级

            ViewBag.IsInfo = this.checkPower("DiaoDanInfo");
            ViewBag.IsAudit = this.checkPower("DiaoDanAudit");
            ViewBag.IsForceRefund = this.checkPower("ForceRefund");
            return View();
        }

        /// <summary>
        /// 查看调单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DiaoDanInfo(int id)
        {
            var Orders = Entity.Orders.FirstOrDefault(n => n.Id == id);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            return View("DiaoDanInfo");
        }

        /// <summary>
        /// 调单审核
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DiaoDanAudit(int id)
        {
            var Orders = Entity.Orders.FirstOrDefault(n => n.Id == id);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            return View("DiaoDanInfo");
        }

        /// <summary>
        /// 调单审核
        /// </summary>
        /// <param name="Orders"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DiaoDanAudit(Orders Orders)
        {
            if (!(Orders.IdCardState == 3 || Orders.IdCardState == 4))
            {
                ViewBag.ErrorMsg = "请选择审核结果";
                return View("Error");
            }
            if (Orders.IdCardState == 4 && Orders.DDAuditRemark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写审核说明";
                return View("Error");
            }
            Orders baseOrders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            if (baseOrders.IdCardState != 2)
            {
                ViewBag.ErrorMsg = "订单已审核通过";
                return View("Error");
            }
            baseOrders.IdCardState = Orders.IdCardState;
            baseOrders.DDAuditRemark = Orders.DDAuditRemark ?? string.Empty;
            baseOrders.InternalRm = Orders.InternalRm;
            baseOrders.CardEditTime = DateTime.Now;
            
            byte LogType = 0;
            #region 银联调单审核
            if (baseOrders.TType == 1)
            {
                //银联卡支付订单
                OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrNew(n => n.OId == baseOrders.TNum);
                OrderRecharge.Remark = baseOrders.DDAuditRemark;
                OrderRecharge.OrderState = baseOrders.TState;
                if (baseOrders.IdCardState == 3 || baseOrders.IdCardState == 0)
                {
                    baseOrders.TState = 2;
                    OrderRecharge.OrderState = 2;
                    //是否有冻结
                    if (baseOrders.FrozenState == 1)
                    {
                        //解冻操作(包含了佣金解决)
                        OrderRecharge.SetUnFrozen(Entity);
                        baseOrders.FrozenState = 0;
                    }
                    Entity.SaveChanges();
                    LogType = 4;
                }
                else
                {
                    LogType = 5;
                }
            }
            #endregion
            #region 微信 支付宝 NFC 审核
            if (baseOrders.TType == 7 || baseOrders.TType == 8 || baseOrders.TType == 9)
            {
                //银联卡支付订单
                OrderF2F OrderF2F = Entity.OrderF2F.FirstOrNew(n => n.OId == baseOrders.TNum);
                OrderF2F.OrderState = baseOrders.TState;
                OrderF2F.Remark = baseOrders.DDAuditRemark;
                if (baseOrders.IdCardState == 3 || baseOrders.IdCardState == 0)
                {
                    baseOrders.TState = 2;
                    OrderF2F.OrderState = 2;
                    //是否有冻结
                    if (baseOrders.FrozenState == 1)
                    {
                        //解冻操作(包含了佣金解决)
                        OrderF2F.SetUnFrozen(Entity);
                        baseOrders.FrozenState = 0;
                    }
                    Entity.SaveChanges();
                    LogType = 4;
                }
                else
                {
                    LogType = 5;
                }
            }
            #endregion
            if (LogType > 0)
            {
                //只有调单记录日志
                OrdersDDLog OrdersDDLog = new OrdersDDLog()
                {
                    AddTime = DateTime.Now,
                    LastTime = baseOrders.DDLastTime,
                    LogType = LogType,
                    OpName = AdminUser.TrueName,
                    Remark = baseOrders.DDAuditRemark,
                    TNum = baseOrders.TNum,
                    InteriorRemark = baseOrders.InternalRm,
                };
                Entity.OrdersDDLog.AddObject(OrdersDDLog);
            }
            this.Entity.SaveChanges();
            ViewBag.Msg = Orders.TNum + " 审核结果：" + (Orders.IdCardState == 3 ? "成功" : "失败");
            return View("Succeed");
        }

        [HttpGet]
        public ActionResult ForceRefund(int id)
        {
            Orders baseOrders = Entity.Orders.FirstOrDefault(n => n.Id == id);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            ViewBag.Orders = baseOrders;
            return View("DiaoDanInfo");
        }

        /// <summary>
        /// 强制退款
        /// </summary>
        /// <param name="Orders"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForceRefund(Orders Orders)
        {
            #region 校验及初始化
            Orders baseOrders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (!(baseOrders.IdCardState == 1 || baseOrders.IdCardState == 4))
            {
                ViewBag.ErrorMsg = "状态异常";
                return View("Error");
            }
            #endregion
            
            //实际到账金额
            decimal PayMoney = 0;
            #region 查询PayMoney
            //银联卡支付订单
            OrderRecharge OrderRecharge = null;
            OrderF2F OrderF2F = null;
            if (baseOrders.TType == 1)
            {
                OrderRecharge = Entity.OrderRecharge.FirstOrNew(n => n.OId == baseOrders.TNum);
                PayMoney = OrderRecharge.PayMoney;
            }
            //微信 支付宝 NFC
            if (baseOrders.TType == 7 || baseOrders.TType == 8 || baseOrders.TType == 9)
            {
                OrderF2F = Entity.OrderF2F.FirstOrNew(n => n.OId == baseOrders.TNum);
                PayMoney = OrderF2F.PayMoney;
            }
            #endregion

            //订单强制退款
            int USERSID = baseOrders.UId;
            string TNUM = baseOrders.TNum;
            string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PayMoney, 11, "强制退款");
            if (SP_Ret != "3")
            {
                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 11, PayMoney, SP_Ret), "SP_UsersMoney");
                ViewBag.ErrorMsg = "退款失败";
                return View("Error");
            }

            //退款信息修改
            baseOrders.TState = 4;
            baseOrders.CardEditTime = DateTime.Now;//审核时间
            baseOrders.IdCardState = 6;
            baseOrders.InternalRm = Orders.InternalRm;
            baseOrders.DDAuditRemark = Orders.DDAuditRemark;
            
            //调单记录
            var OrdersDDLog = new OrdersDDLog()
            {
                TNum = baseOrders.TNum,
                LogType = 6,
                Remark = baseOrders.DDAuditRemark,
                AddTime = DateTime.Now,
                LastTime = baseOrders.DDLastTime,
                OpName = AdminUser.TrueName,
                InteriorRemark = baseOrders.InternalRm,
            };
            Entity.OrdersDDLog.AddObject(OrdersDDLog);

            #region 下级佣金退款
            //银联卡支付订单
            if (baseOrders.TType == 1)
            {
                OrderRecharge.OrderState = baseOrders.TState;
                //下级佣金退款
                OrderRecharge.PayAgent(Entity, 2, 1);
            }
            //微信 支付宝 NFC
            if (baseOrders.TType == 7 || baseOrders.TType == 8 || baseOrders.TType == 9)
            {
                OrderF2F.OrderState = baseOrders.TState;
                //下级佣金退款
                OrderF2F.PayAgent(Entity, 2, 1);
            }
            #endregion

            Entity.SaveChanges();
            ViewBag.Msg = "订单:" + baseOrders.TNum + " ,强制退款成功";
            return View("Succeed");
        }

        #endregion

        #region 发起补单

        /// <summary>
        /// 自动补单
        /// </summary>
        /// <param name="Orders"></param>
        /// <returns></returns>
        public ActionResult OrdersRepair(Orders Orders)
        {
            #region 校验
            Orders = Entity.Orders.FirstOrDefault(n => n.Id == Orders.Id);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            if (!Orders.IfOrdersRepair())
            {
                ViewBag.ErrorMsg = "不符合补单规则";
                return View("Error");
            }
            if (this.Request.QueryString["Confirm"] == "true")
            {
                Orders.ClearRuningState();
                var baseOrdersPayOnly = Entity.OrdersPayOnly.Where(o => o.TNum == Orders.TNum).FirstOrDefault();
                if (baseOrdersPayOnly != null)
                {
                    Entity.OrdersPayOnly.DeleteObject(baseOrdersPayOnly);
                }
                Entity.SaveChanges();
            }
            //var OrdersPayOnly = Entity.OrdersPayOnly.Where(o => o.TNum == Orders.TNum).FirstOrDefault();
            //if (Orders.GetRuningState() || OrdersPayOnly != null)
            //{
            //    ViewBag.Msg = "入账流程未完成，是否先结束流程？";
            //    return View("Confirm");
            //}
            if (this.Request.QueryString["Confirm"] == "true")
            {
                var ConfirmOrdersPayOnly = Entity.OrdersPayOnly.Where(o => o.TNum == Orders.TNum).FirstOrDefault();
                Entity.OrdersPayOnly.DeleteObject(ConfirmOrdersPayOnly);
                Entity.SaveChanges();
            }
            var OrdersPayOnly = Entity.OrdersPayOnly.Where(o => o.TNum == Orders.TNum).FirstOrDefault();
            if (OrdersPayOnly != null)
            {
                ViewBag.Msg = "入账流程未完成，是否先结束流程？";
                return View("Confirm");
            }
            #endregion

            #region 有查询接口流程
            if (Orders.TType == 7 || Orders.TType == 8 || Orders.TType == 9 || Orders.TType == 1)
            {
                bool IsSucceed = false;
                PayConfig PayConfig = Entity.PayConfig.FirstOrNew(n => n.Id == Orders.PayWay);
                string DllName = PayConfig.DllName;

                #region 支付宝
                if (Orders.TType == 7 && PayConfig.DllName == "AliPay")
                {
                    #region 支付宝处理
                    string AlipayVer = "2.0";
                    string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 商户号,密钥,APPId
                    if (PayConfigArr.Length != 3)
                    {
                        ViewBag.ErrorMsg = "配置信息不正确";
                        return View("Error");
                    }
                    if (AlipayVer == "2.0")
                    {
                        #region 2.0新接口
                        ALF2FPAY ALF2FPAY = new ALF2FPAY();
                        ALF2FPAY.pid = PayConfigArr[0];
                        ALF2FPAY.appId = PayConfigArr[2];

                        IAopClient client = new DefaultAopClient(ALF2FPAY.serverUrl, ALF2FPAY.appId, ALF2FPAY.merchant_private_key, "json", ALF2FPAY.version, ALF2FPAY.sign_type, ALF2FPAY.alipay_public_key, ALF2FPAY.charset);

                        string QueryStr = "{\"out_trade_no\":\"" + Orders.TNum + "\"}";
                        ALF2FPAYObj ObjQuery = new ALF2FPAYObj();
                        ObjQuery.BizCode = QueryStr;
                        ObjQuery.Client = client;

                        AlipayTradeQueryRequest payRequst = new AlipayTradeQueryRequest();
                        payRequst.BizContent = ObjQuery.BizCode;
                        Dictionary<string, string> paramsDict = (Dictionary<string, string>)payRequst.GetParameters();
                        AlipayTradeQueryResponse payResponse = null;

                        payResponse = ObjQuery.Client.Execute(payRequst);
                        if (payResponse != null)
                        {
                            payResponse.SaveLog(Entity);//保存记录
                            if (string.Compare(payResponse.Code, "10000", false) == 0)
                            {
                                if (payResponse.TradeStatus == "TRADE_FINISHED" || payResponse.TradeStatus == "TRADE_SUCCESS")
                                {
                                    Orders.TState = 1;
                                    Orders = Orders.PaySuccess(this.Entity);
                                    if (Orders.PayState == 1)
                                    {
                                        IsSucceed = true;
                                    }
                                }
                                else
                                {
                                    ViewBag.ErrorMsg = payResponse.SubMsg;
                                    return View("Error");
                                }
                            }
                            else
                            {
                                ViewBag.ErrorMsg = payResponse.SubMsg;
                                return View("Error");
                            }
                        }
                        #endregion
                    }

                    #endregion
                }
                #endregion

                #region 微信
                if (Orders.TType == 8 && PayConfig.DllName == "WeiXin")
                {
                    #region 微信处理
                    //初始化支付配置
                    WxPayConfig WxPayConfig = new WxPayConfig();
                    string[] PayConfigArr = PayConfig.QueryArray.Split(new char[] { ',' });//接口信息 appid,mchid,key,appsecret
                    if (PayConfigArr.Length != 4 && PayConfigArr.Length != 5)
                    {
                        ViewBag.ErrorMsg = "配置信息不正确";
                        return View("Error");
                    }
                    string ServerIp = ConfigurationManager.AppSettings["ServerIp"].ToString();
                    string Wx_Cert_Path = ConfigurationManager.AppSettings["Wx_Cert_Path"].ToString();
                    string Wx_Cert_PWD = ConfigurationManager.AppSettings["Wx_Cert_PWD"].ToString();
                    WxPayConfig.IP = ServerIp;
                    WxPayConfig.APPID = PayConfigArr[0];
                    WxPayConfig.MCHID = PayConfigArr[1];
                    WxPayConfig.KEY = PayConfigArr[2];
                    WxPayConfig.APPSECRET = PayConfigArr[3];
                    if (PayConfigArr.Length == 5)
                    {
                        WxPayConfig.SubMCHID = PayConfigArr[4];
                    }
                    WxPayConfig.SSLCERT_PATH = Wx_Cert_Path;
                    WxPayConfig.SSLCERT_PASSWORD = Wx_Cert_PWD;

                    int succResult = 0;//查询结果
                    MicroPay MicroPay = new MicroPay();
                    WxPayData queryResult = MicroPay.WXQuery(Orders.TNum, WxPayConfig, Entity, out succResult);
                    if (succResult == 1)
                    {
                        Orders.TState = 1;
                        Orders = Orders.PaySuccess(this.Entity);
                        if (Orders.PayState == 1)
                        {
                            IsSucceed = true;
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = "接口未成功处理订单";
                        return View("Error");
                    }

                    #endregion
                }
                #endregion

                #region NFC
                if (Orders.TType == 9)
                {
                    if (PayConfig.DllName == "NFC")
                    {
                        #region NFC处理
                        //1000000951,f38e989d-900f-4768-8a01-f6667a21f7d3
                        string[] QueryArr = PayConfig.QueryArray.Split(',');
                        if (QueryArr.Length != 2)
                        {
                            ViewBag.ErrorMsg = "配置信息不正确";
                            return View("Error");
                        }
                        string merchantCode = QueryArr[0];
                        string Key = QueryArr[1];
                        string outOrderId = Orders.TNum;
                        string dataStr = "merchantCode=" + merchantCode + "&outOrderId=" + outOrderId;
                        string signStr = dataStr + "&KEY=" + Key;
                        string sign = signStr.GetMD5().ToUpper();
                        string dataJson = "{\"merchantCode\":\"" + merchantCode + "\",\"outOrderId\":\"" + outOrderId + "\",\"sign\":\"" + sign + "\"}";
                        string postData = "{\"param\":" + dataJson + ",\"project_id\":\"WEPAYPLUGIN_PAY\"}";
                        string postUrl = "https://payment.kklpay.com/query/queryOrder.do";
                        string Ret = Utils.GetPostJson(postUrl, postData);
                        //"{"code":"00","data":{"amount":1,"instructCode":"11001998044","merchantCode":"1000000951","outOrderId":"201511170900077","replyCode":"00","sign":"EA778C87B5ACDCBC7735BB78C15CAC72","transTime":"20151117174726","transType":"00200"},"msg":"成功"}"
                        JObject JS = new JObject();
                        try
                        {
                            JS = (JObject)JsonConvert.DeserializeObject(Ret);
                        }
                        catch (Exception Ex)
                        {
                            Utils.WriteLog("[OrderNFC]:JSON[" + Ret + "]" + Ex.ToString(), "orderface");
                        }
                        if (JS == null)
                        {
                            ViewBag.ErrorMsg = "返回信息异常";
                            return View("Error");
                        }

                        string code = JS["code"].ToString();//返回状态--
                        if (code == "00")
                        {
                            JObject JSD = (JObject)JS["data"];
                            if (JSD != null)
                            {
                                string amount = JSD["amount"].ToString();//交易金额 单位分
                                string instructCode = JSD["instructCode"].ToString();//交易单号
                                string merchantCodeR = JSD["merchantCode"].ToString();//商户号
                                string outOrderIdR = JSD["outOrderId"].ToString();//订单号
                                string replyCode = JSD["replyCode"].ToString();//交易状态
                                string transTime = JSD["transTime"].ToString();//交易时间
                                string transType = JSD["transType"].ToString();//交易类型
                                string signR = JSD["sign"].ToString();
                                //================================================
                                PayLog PayLog = new PayLog();
                                PayLog.PId = PayConfig.Id;
                                PayLog.OId = outOrderId;
                                PayLog.TId = instructCode;
                                PayLog.Amount = decimal.Parse(amount) / 100;
                                PayLog.Way = "GET";
                                PayLog.AddTime = DateTime.Now;
                                PayLog.Data = Ret;
                                PayLog.State = 1;
                                Entity.PayLog.AddObject(PayLog);
                                Entity.SaveChanges();
                                //================================================
                                if (replyCode == "00")
                                {
                                    if (merchantCodeR == merchantCode)
                                    {
                                        int factmoney = int.Parse(amount);
                                        if (((int)(Orders.Amoney * 100)) == factmoney)
                                        {
                                            Orders.TState = 1;
                                            Orders = Orders.PaySuccess(this.Entity);
                                            if (Orders.PayState == 1)
                                            {
                                                IsSucceed = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else if (PayConfig.DllName == "HFNFC")
                    {
                        #region 好付NFC处理
                        string[] QueryArr = PayConfig.QueryArray.Split(',');
                        if (QueryArr.Length != 3)
                        {
                            ViewBag.ErrorMsg = "配置信息不正确";
                            return View("Error");
                        }
                        //提交结算中心
                        string merId = QueryArr[0];//商户号
                        string merKey = QueryArr[1];//商户密钥
                        string orderId = Orders.TNum;//商户流水号
                        string PostJson = "{\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\"}";
                        string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                        string Sign = (DataBase64 + merKey).GetMD5();
                        DataBase64 = HttpUtility.UrlEncode(DataBase64);
                        string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                        string HFNFC_Url = "https://api.zhifujiekou.com/api/query";
                        string Ret = Utils.PostRequest(HFNFC_Url, PostData, "utf-8");

                        JObject JS = new JObject();
                        try
                        {
                            JS = (JObject)JsonConvert.DeserializeObject(Ret);
                        }
                        catch (Exception Ex)
                        {
                            Utils.WriteLog("[OrderNFC_HFQuery]:JSON[" + Ret + "]" + Ex.ToString(), "orderface");
                        }
                        if (JS == null)
                        {
                            ViewBag.ErrorMsg = "返回信息异常";
                            return View("Error");
                        }
                        string resp = JS["resp"].ToString();
                        Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                        try
                        {
                            JS = (JObject)JsonConvert.DeserializeObject(Ret);
                        }
                        catch (Exception Ex)
                        {
                            Utils.WriteLog("[OrderNFC_HFQuery]:JSONRet2[" + Ret + "]" + Ex.ToString(), "orderface");
                        }
                        if (JS == null)
                        {
                            ViewBag.ErrorMsg = "返回信息异常";
                            return View("Error");
                        }
                        string respcode = JS["respcode"].ToString();
                        if (respcode != "00")
                        {
                            string respmsg = JS["respmsg"].ToString();
                            Utils.WriteLog("[OrderNFC_HFQuery_Err]:【" + respcode + "】" + respmsg, "orderface");
                            ViewBag.ErrorMsg = respmsg;
                            return View("Error");
                        }
                        string resultcode = JS["resultcode"].ToString();
                        if (resultcode == "0000" || resultcode == "1002")
                        {
                            string txnamt = JS["txnamt"].ToString();
                            int factmoney = int.Parse(txnamt);
                            if (((int)(Orders.Amoney * 100)) == factmoney)
                            {
                                Orders.TState = 1;
                                Orders = Orders.PaySuccess(this.Entity);
                                if (Orders.PayState == 1)
                                {
                                    IsSucceed = true;
                                }
                            }
                        }
                        else
                        {
                            Utils.WriteLog("[Order_HFQuery_Err]:【" + respcode + "】", "orderface");
                            ViewBag.ErrorMsg = "订单：" + Orders.TNum + " 补单失败,原因：" + JS["resultmsg"];
                            return View("Error");
                        }
                        #endregion
                    }
                }
                #endregion

                #region 好付
                if ((DllName == "HFAliPay" && Orders.TType == 7) || (DllName == "HFWeiXin" && Orders.TType == 8) || (DllName == "HFPay" && Orders.TType == 1))
                {
                    #region 好付处理
                    string[] QueryArr = PayConfig.QueryArray.Split(',');
                    if (QueryArr.Length != 3)
                    {
                        ViewBag.ErrorMsg = "配置信息不正确";
                        return View("Error");
                    }
                    //提交结算中心
                    string merId = QueryArr[0];//商户号
                    string merKey = QueryArr[1];//商户密钥
                    string orderId = Orders.TNum;//商户流水号
                    string PostJson = "{\"merid\":\"" + merId + "\",\"orderid\":\"" + orderId + "\"}";
                    string DataBase64 = LokFuEncode.Base64Encode(PostJson, "utf-8");
                    string Sign = (DataBase64 + merKey).GetMD5();
                    DataBase64 = HttpUtility.UrlEncode(DataBase64);
                    string PostData = string.Format("req={0}&sign={1}", DataBase64, Sign);
                    string HF_Url = "https://api.zhifujiekou.com/api/query";
                    string Ret = Utils.PostRequest(HF_Url, PostData, "utf-8");

                    JObject JS = new JObject();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception Ex)
                    {
                        Utils.WriteLog("[Order_HFQuery]:JSON[" + Ret + "]" + Ex.ToString(), "orderface");
                    }
                    if (JS == null || JS["resp"] == null)
                    {
                        ViewBag.ErrorMsg = "返回信息异常";
                        return View("Error");
                    }
                    string resp = JS["resp"].ToString();
                    Ret = LokFuEncode.Base64Decode(resp, "utf-8");
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(Ret);
                    }
                    catch (Exception Ex)
                    {
                        Utils.WriteLog("[Order_HFQuery]:JSONRet2[" + Ret + "]" + Ex.ToString(), "orderface");
                    }
                    if (JS == null)
                    {
                        ViewBag.ErrorMsg = "返回信息异常";
                        return View("Error");
                    }
                    string respcode = JS["respcode"].ToString();
                    if (respcode != "00")
                    {
                        string respmsg = JS["respmsg"].ToString();
                        Utils.WriteLog("[Order_HFQuery_Err]:【" + respcode + "】" + respmsg, "orderface");
                        ViewBag.ErrorMsg = respmsg;
                        return View("Error");
                    }
                    string resultcode = JS["resultcode"].ToString();
                    if (resultcode == "0000" || resultcode == "1002")
                    {
                        string txnamt = JS["txnamt"].ToString();
                        int factmoney = int.Parse(txnamt);
                        if (((int)(Orders.Amoney * 100)) == factmoney)
                        {
                            Orders.TState = 1;
                            Orders = Orders.PaySuccess(this.Entity);
                            if (Orders.PayState == 1)
                            {
                                IsSucceed = true;
                            }
                        }
                    }
                    else
                    {
                        Utils.WriteLog("[Order_HFQuery_Err]:【" + respcode + "】", "orderface");
                        ViewBag.ErrorMsg = " 补单失败,原因：" + JS["resultmsg"];
                        return View("Error");
                    }
                    #endregion
                }
                #endregion

                if (IsSucceed)
                {
                    var BaseOrdersRepair = Entity.OrdersRepair.FirstOrDefault(o => o.TNum == Orders.TNum);
                    if (BaseOrdersRepair == null)
                    {
                        BaseOrdersRepair = new OrdersRepair()
                        {
                            CreateAdminName = AdminUser.TrueName,
                            CreateAdminId = AdminUser.Id,
                            TNum = Orders.TNum,
                            AddTime = DateTime.Now,
                            Amoney = Orders.Amoney,
                            RepairType = 1,
                            TState = 2,
                            Remark = string.Empty,
                            Pic = string.Empty,
                        };
                        this.Entity.OrdersRepair.AddObject(BaseOrdersRepair);
                    }
                    else
                    {
                        BaseOrdersRepair.CreateAdminName = AdminUser.TrueName;
                        BaseOrdersRepair.CreateAdminId = AdminUser.Id;
                        BaseOrdersRepair.RepairType = 1;
                        BaseOrdersRepair.TState = 2;
                        BaseOrdersRepair.Remark = string.Empty;
                        BaseOrdersRepair.Pic = string.Empty;
                    }
                    Orders.RepairState = 2;
                    this.Entity.SaveChanges();
                    ViewBag.Msg = Orders.TNum + " 补单成功!";
                    return View("Succeed");
                }
                else
                {
                    ViewBag.ErrorMsg = Orders.TNum + " 补单失败!";
                    return View("Error");
                }
            }
            #endregion

            #region 无查询接口流程
            else if (Orders.TType == 1)
            {
                return this.RedirectToAction("RepairSave", new { TNum = Orders.TNum, IsAjax = 1 });
            }
            #endregion
            else
            {
                ViewBag.ErrorMsg = "订单类型不符";
                return View();
            }
        }

        [HttpGet]
        public ActionResult RepairSave(Orders Orders)
        {
            Orders = Entity.Orders.FirstOrDefault(n => n.TNum == Orders.TNum);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            if (!Orders.IfOrdersRepair())
            {
                ViewBag.ErrorMsg = "不符合补单规则";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.BasicDescList = GetBasicDescList(BasicCodeEnum.Ddbd);
            return View();
        }

        /// <summary>
        /// 人工补单
        /// </summary>
        /// <param name="DeductMoney"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult RepairSave(OrdersRepair OrdersRepair)
        {
            #region 校验及初始化
            if (OrdersRepair.Remark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写备注";
                return View("Error");
            }
            OrdersRepair = Request.ConvertRequestToModel<OrdersRepair>(OrdersRepair, OrdersRepair);
            if (OrdersRepair.Pic == "System.Web.HttpPostedFileWrapper" || OrdersRepair.Pic == null)
            {
                ViewBag.ErrorMsg = "文件类型错误";
                return View("Error");
            }
            Orders baseOrders = Entity.Orders.FirstOrDefault(n => n.TNum == OrdersRepair.TNum);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            if (!baseOrders.IfOrdersRepair())
            {
                ViewBag.ErrorMsg = "不符合补单规则";
                return View("Error");
            }

            #endregion
            //订单信息修改
            var BaseOrdersRepair = Entity.OrdersRepair.FirstOrDefault(o => o.TNum == baseOrders.TNum);
            baseOrders.RepairState = 1;
            if (BaseOrdersRepair == null)
            {
                #region 新补单
                BaseOrdersRepair = new OrdersRepair()
                {
                    CreateAdminName = AdminUser.TrueName,
                    CreateAdminId = AdminUser.Id,
                    TNum = baseOrders.TNum,
                    AddTime = DateTime.Now,
                    Amoney = baseOrders.Amoney,
                    RepairType = 2,
                    TState = 1,
                    Pic = OrdersRepair.Pic,
                    Remark = OrdersRepair.Remark,
                };
                this.Entity.OrdersRepair.AddObject(BaseOrdersRepair);
                //记录
                var OrdersRepairLog = new OrdersRepairLog()
                {
                    AddTime = DateTime.Now,
                    TNum = baseOrders.TNum,
                    LogType = 1,
                    AdminId = AdminUser.Id,
                    AdminName = AdminUser.TrueName,
                    Img = OrdersRepair.Pic,
                    Remark = OrdersRepair.Remark,
                };
                this.Entity.OrdersRepairLog.AddObject(OrdersRepairLog);
                #endregion
            }
            else
            {
                #region 重新提交
                BaseOrdersRepair.CreateAdminName = AdminUser.TrueName;
                BaseOrdersRepair.CreateAdminId = AdminUser.Id;
                BaseOrdersRepair.RepairType = 2;
                BaseOrdersRepair.TState = 1;
                BaseOrdersRepair.Pic = OrdersRepair.Pic;
                BaseOrdersRepair.Remark = OrdersRepair.Remark;

                var OrdersRepairLog = new OrdersRepairLog()
                {
                    AddTime = DateTime.Now,
                    TNum = baseOrders.TNum,
                    LogType = 4,
                    AdminId = AdminUser.Id,
                    AdminName = AdminUser.TrueName,
                    Img = OrdersRepair.Pic,
                    Remark = OrdersRepair.Remark,
                };
                this.Entity.OrdersRepairLog.AddObject(OrdersRepairLog);
                #endregion
            }
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功，请等待审核!";
            return View("Succeed");
        }
        #endregion

        #region 发起退款
        /// <summary>
        /// 退单/退款
        /// </summary>
        /// <param name="Orders"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Chargeback(Orders Orders)
        {
            Orders = Entity.Orders.FirstOrDefault(n => n.TNum == Orders.TNum);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            if (!Orders.IfCanTuiDan())
            {
                ViewBag.ErrorMsg = "不符合退款规则";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.BasicDescList = GetBasicDescList(BasicCodeEnum.Ddtk);
            return View();
        }

        /// <summary>
        /// 退单/退款
        /// </summary>
        /// <param name="DeductMoney"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Chargeback(OrdersRefund OrdersRefund)
        {
            #region 校验及初始化
            if (OrdersRefund.Remark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写备注";
                return View("Error");
            }
            OrdersRefund = Request.ConvertRequestToModel<OrdersRefund>(OrdersRefund, OrdersRefund);
            if (OrdersRefund.Pic == "System.Web.HttpPostedFileWrapper" || OrdersRefund.Pic == null)
            {
                ViewBag.ErrorMsg = "文件类型错误";
                return View("Error");
            }
            Orders baseOrders = Entity.Orders.FirstOrDefault(o => o.TNum == OrdersRefund.TNum);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            if (!baseOrders.IfCanTuiDan())
            {
                ViewBag.ErrorMsg = "不符合退款规则";
                return View("Error");
            }

            #endregion

            #region 退款前先冻结
            decimal PayMoney = 0;
            // 银联卡支付订单
            if (baseOrders.TType == 1)
            {
                OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrNew(n => n.OId == baseOrders.TNum);
                OrderRecharge.OrderState = baseOrders.TState;
                baseOrders.Remark = OrdersRefund.Remark;
                OrderRecharge.Remark = OrdersRefund.Remark;
                if (baseOrders.FrozenState == 0)
                {
                    //冻结操作(包含了佣金冻结)
                    OrderRecharge.SetFrozen(Entity);
                    baseOrders.FrozenState = 1;
                }
                PayMoney = OrderRecharge.PayMoney;
            }
            // 微信 支付宝 NFC
            if (baseOrders.TType == 7 || baseOrders.TType == 8 || baseOrders.TType == 9)
            {
                OrderF2F OrderF2F = Entity.OrderF2F.FirstOrNew(n => n.OId == baseOrders.TNum);
                OrderF2F.OrderState = baseOrders.TState;
                baseOrders.Remark = OrdersRefund.Remark;
                OrderF2F.Remark = OrdersRefund.Remark;
                if (baseOrders.FrozenState == 0)
                {
                    //冻结操作(包含了佣金冻结)
                    OrderF2F.SetFrozen(Entity);
                    baseOrders.FrozenState = 1;
                }
                PayMoney = OrderF2F.PayMoney;
            }
            #endregion

            //修改订单信息
            baseOrders.TDState = 1;//审核中
            var baseOrdersRefund = this.Entity.OrdersRefund.FirstOrDefault(o => o.TNum == baseOrders.TNum);
            if (baseOrdersRefund == null)
            {
                #region 新添加
                baseOrdersRefund = new OrdersRefund()
                {
                    TNum = baseOrders.TNum,
                    AddTime = DateTime.Now,
                    Amoney = PayMoney,
                    CreateAdminId = this.AdminUser.Id,
                    CreateAdminName = this.AdminUser.TrueName,
                    Pic = OrdersRefund.Pic,
                    Remark = OrdersRefund.Remark,
                    TDLastTime = OrdersRefund.TDLastTime,
                    TState = 1,
                    UId = baseOrders.UId,
                };
                this.Entity.OrdersRefund.AddObject(baseOrdersRefund);
                var OrdersRefundLog = new OrdersRefundLog()
                {
                    AddTime = DateTime.Now,
                    AdminId = this.AdminUser.Id,
                    AdminName = this.AdminUser.TrueName,
                    Img = OrdersRefund.Pic,
                    Remark = OrdersRefund.Remark,
                    LogType = 1,
                    TNum = baseOrders.TNum,
                };
                this.Entity.OrdersRefundLog.AddObject(OrdersRefundLog);
                #endregion
            }
            else
            {
                #region 重新提交
                baseOrdersRefund.TState = 1;
                baseOrdersRefund.Remark = OrdersRefund.Remark;
                baseOrdersRefund.Pic = OrdersRefund.Pic;
                baseOrdersRefund.CreateAdminId = this.AdminUser.Id;
                baseOrdersRefund.CreateAdminName = this.AdminUser.TrueName;
                var OrdersRefundLog = new OrdersRefundLog()
                {
                    AddTime = DateTime.Now,
                    AdminId = this.AdminUser.Id,
                    AdminName = this.AdminUser.TrueName,
                    Img = OrdersRefund.Pic,
                    Remark = OrdersRefund.Remark,
                    LogType = 4,
                    TNum = baseOrders.TNum,
                };
                this.Entity.OrdersRefundLog.AddObject(OrdersRefundLog);
                #endregion
            }
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功，请等待审核!";
            return View("Succeed");
        }
        #endregion

        public ActionResult IndexUserLog(string tnum)
        {
            var UserLogList = this.Entity.UserLog.Where(o => o.OId == tnum).OrderByDescending(o=>o.Id).ToList();
            var UIds = UserLogList.Select(o => o.UId).ToList();
            var UserNameList = this.Entity.Users.Where(o => UIds.Contains(o.Id)).ToDictionary(o=>o.Id,o=>o.TrueName);
            ViewBag.UserLogList = UserLogList;
            ViewBag.UserNameList = UserNameList;
            return View();
        }

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

        private EFPagingInfo<Orders> Condition(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, int TimeType = 1)
        {
            
            if (!Orders.TNum.IsNullOrEmpty())
            {
                if (Orders.UId == 1)
                {
                    p.SqlWhere.Add(f => f.TNum == Orders.TNum);
                }
                else
                {
                    IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(Orders.TNum) || n.NeekName.Contains(Orders.TNum) || n.UserName == Orders.TNum).ToList();
                    List<int> UIds = new List<int>();
                    foreach (var pp in UList)
                    {
                        UIds.Add(pp.Id);
                    }
                    p.SqlWhere.Add(f => UIds.Contains(f.UId));
                }
            }

            if (!Orders.TrunType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.TrunType == (Orders.TrunType == 99 ? 0 : Orders.TrunType));
            }
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
            if (!Orders.PayWay.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.PayWay == Orders.PayWay); }
            //是否选择了分支机构
            if (!Orders.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == Orders.Agent).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = SysAgentList.Select(o => o.Id).ToList();
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == Orders.Agent);
                }
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
                    //if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                    switch (Orders.TState)
                    {
                        case 1://处理中
                            p.SqlWhere.Add(f => f.TState == 1);
                            break;
                        case 2://已汇出
                            p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 2);
                            break;
                        case 3://提现失败
                            p.SqlWhere.Add(f => f.TState == 3);
                            break;
                        case 4://出款中
                            p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                            break;
                    }
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
                            p.SqlWhere.Add(f => f.TState == 3 && f.PayState == 1 && f.IdCardState == 4);
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
                                p.SqlWhere.Add(f => (f.TState == 1 || f.TState == 3) && f.PayState == 1 && f.IdCardState == 4);
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
                            p.SqlWhere.Add(f => f.PayState == 1 && f.InState == 1 && f.IdCardState == 1);
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

            return p;
        }
    }
}
