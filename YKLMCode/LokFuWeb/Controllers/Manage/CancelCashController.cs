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
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class CancelCashController : BaseController
    {

        public ActionResult Index(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent, int IsFirst = 0, int TimeType=1)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(OrderCash, p, IsShowSupAgent, IsFirst, TimeType);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrderCash> OrderCashList = null;
            if (IsFirst == 0)
            {
                OrderCashList = new PageOfItems<OrderCash>(new List<OrderCash>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                OrderCashList = Entity.Selects<OrderCash>(p);
            }
            ViewBag.OrderCashList = OrderCashList;
            ViewBag.OrderCash = OrderCash;
            IList<OrderCash> List = OrderCashList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Save = this.checkPower("Save");
            ViewBag.TimeType = TimeType;
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            return View();
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent, int IsFirst = 0, int TimeType = 1)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(OrderCash, p, IsShowSupAgent, IsFirst, TimeType);
            var Iquery = this.Entity.OrderCash.AsQueryable();
            foreach (var item in p.SqlWhere)
            {
                Iquery = Iquery.Where(item);
            }
            var Count = Iquery.Count();
            var SumAmoney = Iquery.Sum(o => (decimal?)o.Amoney);
            var SumPaymoney = Iquery.Sum(o => (double?)o.UserRate);
            this.ViewBag.SumAmoney = SumAmoney;
            this.ViewBag.SumPaymoney = SumPaymoney;
            this.ViewBag.Count = Count;
            return this.View();
        }

        [NonAction]
        private EFPagingInfo<OrderCash> Condition(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent, int IsFirst = 0, int TimeType = 1)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p.SqlWhere.Add(f => f.OrderState == 2);
            p.SqlWhere.Add(f => f.PayState == 3 || f.PayState == 4);
            //if (!OrderCash.UId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UId == OrderCash.UId); }
            if (!OrderCash.Owner.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(OrderCash.Owner) || n.NeekName.Contains(OrderCash.Owner) || n.UserName == OrderCash.Owner).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }
            if (!OrderCash.OId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OId == OrderCash.OId); }
            if (!OrderCash.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == OrderCash.Agent).FirstOrNew();
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
                    p.SqlWhere.Add(f => f.Agent == OrderCash.Agent);
                }

            }
            if (!OrderCash.PayState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.PayState == OrderCash.PayState); }
            if (!OrderCash.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == OrderCash.AId); }
            if (!OrderCash.FId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FId == OrderCash.FId); }
            if (TimeType == 1)
            {
                if (!OrderCash.AddTime.IsNullOrEmpty() && !OrderCash.FTime.IsNullOrEmpty())
                {
                    // DateTime FTime = ((DateTime)OrderCash.FTime).AddDays(1);
                    DateTime FTime = OrderCash.FTime.Value.AddMilliseconds(999);
                    p.SqlWhere.Add(f => f.AddTime > OrderCash.AddTime && f.AddTime < FTime);
                }
            }
            else
            {
                if (!OrderCash.AddTime.IsNullOrEmpty() && !OrderCash.FTime.IsNullOrEmpty())
                {
                    DateTime FTime = OrderCash.FTime.Value.AddMilliseconds(999);
                    p.SqlWhere.Add(f => f.RefundTime > OrderCash.AddTime && f.RefundTime < FTime);
                }
            }
            return p;
        }

        public ActionResult Edit(OrderCash OrderCash)
        {
            if (OrderCash.Id != 0) OrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            if (OrderCash == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.OrderCash = OrderCash;
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderCash.OId);
            if (Orders.PayState != 3 && Orders.PayState != 4)
            {
                ViewBag.ErrorMsg = "当前状态不能退款！";
                return View("Error");
            }
            if (Orders.TState != 2)
            {
                ViewBag.ErrorMsg = "交易不成功，不能退款！";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == Orders.UId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Orders.Agent);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.AId);
            ViewBag.FinAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.FId);
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }

        [ValidateInput(false)]
        public void SaveEdit(OrderCash OrderCash)
        {
            OrderCash baseOrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == baseOrderCash.OId);
            if (baseOrderCash.OrderState == 2 && baseOrderCash.PayState == 3)
            {
                baseOrderCash.PayState = 2;
                baseOrderCash.FState = 1;
                baseOrderCash.FTime = DateTime.Now;
                Orders.PayState = 2;
                Entity.SaveChanges();
                //======分润======
                baseOrderCash = baseOrderCash.PayAgent(Entity, 1);
                Orders.AgentPayGet = (decimal)baseOrderCash.AgentCashGet;

                var OrderCashLog = new OrderCashLog()
                {
                    AddTime = DateTime.Now,
                    AdminId = this.AdminUser.Id,
                    AdminName = this.AdminUser.TrueName,
                    TNum = baseOrderCash.OId,
                    Remark = string.Empty,
                    LogType = 4,
                };
                this.Entity.OrderCashLog.AddObject(OrderCashLog);

                Entity.SaveChanges();
                Orders.SendMsg(Entity);//发送消息类
            }
            BaseRedirect();
        }
        [ValidateInput(false)]
        public ActionResult Save(OrderCash OrderCash)
        {
            OrderCash baseOrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            string tnum = "TK" + baseOrderCash.OId;
            if (!this.GetRuningState(tnum))
            {
                this.SetRuningState(tnum);
                try
                {
                    OrdersPayOnly OPO = new OrdersPayOnly();
                    OPO.TNum = tnum;
                    OPO.AddTime = DateTime.Now;
                    Entity.OrdersPayOnly.AddObject(OPO);
                    Entity.SaveChanges();
                }
                catch (Exception)
                {
                    ViewBag.ErrorMsg = "重复入帐!";
                    return View("Error");
                }
                //return null;
                if (baseOrderCash.OrderState == 2 && baseOrderCash.PayState == 3)
                {
                    var OrderCashLog = new OrderCashLog()
                    {
                        AddTime = DateTime.Now,
                        AdminId = this.AdminUser.Id,
                        AdminName = this.AdminUser.TrueName,
                        TNum = baseOrderCash.OId,
                        Remark = string.Empty,
                        LogType = 3,
                    };
                    this.Entity.OrderCashLog.AddObject(OrderCashLog);
                    baseOrderCash.RefundTime = DateTime.Now;
                    baseOrderCash.PayState = 4;
                    Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == baseOrderCash.OId);
                    Orders.PayState = 4;
                    //退款到余额
                    Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == baseOrderCash.UId);
                    //帐户变动记录
                    int USERSID = baseUsers.Id;
                    string TNUM = Orders.TNum;
                    decimal PAYMONEY = baseOrderCash.Amoney;
                    string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PAYMONEY, 6, "");
                    if (SP_Ret != "3")
                    {
                        Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 6, PAYMONEY, SP_Ret), "SP_UsersMoney");
                    }
                    //======分润======
                    baseOrderCash = baseOrderCash.PayAgent(Entity, 2);
                    Orders.AgentPayGet = (decimal)baseOrderCash.AgentCashGet;
                    Entity.SaveChanges();
                    Orders.SendMsg(Entity);//发送消息类
                    //T0时增加配额
                    if (baseOrderCash.TrunType == 0)
                    {
                        decimal Money = baseOrderCash.Amoney - (decimal)baseOrderCash.UserRate;
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
                BaseRedirect();
            }
            return null;
        }

        public ActionResult IndexOrderCashLog(string TNum)
        {
            var OrderCashLogList = this.Entity.OrderCashLog.Where(o => o.TNum == TNum).ToList();
            this.ViewBag.OrderCashLogList = OrderCashLogList;
            return View();
        }

        public ActionResult Info(OrderCash OrderCash)
        {
            if (OrderCash.Id != 0) OrderCash = Entity.OrderCash.FirstOrDefault(n => n.Id == OrderCash.Id);
            ViewBag.OrderCash = OrderCash;
            Orders Orders = Entity.Orders.FirstOrDefault(n => n.TNum == OrderCash.OId);
            if (Orders == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Orders = Orders;
            Users Users = Entity.Users.FirstOrNew(n => n.Id == Orders.UId);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.Users = Users;
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Orders.Agent);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.AId);
            ViewBag.FinAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == Orders.FId);
            return View("Edit");
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="ApplyJoin"></param>
        /// <param name="p"></param>
        /// <param name="STime"></param>
        /// <param name="ETime"></param>
        /// <param name="IsShowSupAgent"></param>
        /// <returns></returns>
        public FileResult ExcelExport(OrderCash OrderCash, EFPagingInfo<OrderCash> p, bool? IsShowSupAgent, int IsFirst = 0, int TimeType = 1)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            p = this.Condition(OrderCash, p, IsShowSupAgent, IsFirst, TimeType);

            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrderCash> OrderCashList = Entity.Selects<OrderCash>(p);

            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("订单号", typeof(string)));
            table.Columns.Add(new DataColumn("添加时间", typeof(string)));
            table.Columns.Add(new DataColumn("实名认证", typeof(string)));
            table.Columns.Add(new DataColumn("开户名", typeof(string)));
            table.Columns.Add(new DataColumn("手机号码", typeof(string)));
            table.Columns.Add(new DataColumn("银行", typeof(string)));
            table.Columns.Add(new DataColumn("银行账号", typeof(string)));
            table.Columns.Add(new DataColumn("提现方式", typeof(string)));
            table.Columns.Add(new DataColumn("提现金额", typeof(string)));
            table.Columns.Add(new DataColumn("手续费率", typeof(string)));
            table.Columns.Add(new DataColumn("退款状态", typeof(string)));
            table.Columns.Add(new DataColumn("退款时间", typeof(string)));
            table.Columns.Add(new DataColumn("交易备注", typeof(string)));
            List<int> UId = new List<int>();
            foreach (var pp in OrderCashList)
            {
                UId.Add(pp.UId);
            }
            IList<Users> UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            // 填充数据
            foreach (var item in OrderCashList)
            {
                Users Users = UsersList.FirstOrNew(n => n.Id == item.UId);
                row = table.NewRow();
                row[0] = item.OId;
                row[1] = item.AddTime.ToString();
                row[2] = Users.TrueName;
                row[3] = item.Owner;
                row[4] = item.Mobile;
                row[5] = item.Deposit ;
                row[6] = item.CardNum;
                row[7] = "T+"+item.TrunType;
                row[8] = item.Amoney.ToString("F2");
                row[9] = item.UserRate.ToString("F2");
                row[10] = (item.PayState==3?"申请中":"已退款");
                row[11] = (item.RefundTime != null ? Convert.ToDateTime(item.RefundTime).ToString("yyyy-MM-dd HH:mm") : "");
                row[12] = item.Remark;
                table.Rows.Add(row);
            }
            string Time = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99);
            return this.ExportExcelBase(table, "提现退款审核-" + Time);
        }
    }
}
