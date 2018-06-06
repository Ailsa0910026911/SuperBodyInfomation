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
namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// 补单
    /// </summary>
    public class OrdersRepairController : BaseController
    {
        public ActionResult Index(OrdersRepair OrdersRepair, EFPagingInfo<OrdersRepair> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            #region 筛选条件
            if (!OrdersRepair.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == OrdersRepair.TNum); }
            if (!OrdersRepair.RepairType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.RepairType == OrdersRepair.RepairType);
            }
            if (!OrdersRepair.TState.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.TState == OrdersRepair.TState);
            }
            if (STime.HasValue)
            {
                p.SqlWhere.Add(f => f.AddTime >= STime);
            }
            if (ETime.HasValue)
            {
                //var NETime = ETime.Value.AddDays(1);
                var NETime = ETime.Value;
                p.SqlWhere.Add(f => f.AddTime <= NETime);
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrdersRepair> OrdersRepairList = null;
            if (IsFirst == 0)
            {
                OrdersRepairList = new PageOfItems<OrdersRepair>(new List<OrdersRepair>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                OrdersRepairList = Entity.Selects<OrdersRepair>(p);
            }
            ViewBag.OrdersRepairList = OrdersRepairList;
            ViewBag.OrdersRepair = OrdersRepair;
            ViewBag.STime = STime;
            ViewBag.ETime = ETime;

            //权限相关
            ViewBag.IsInfo = this.checkPower("Info");
            ViewBag.IsAudit = this.checkPower("Audit");
            ViewBag.IsRepairSave = this.checkPower("Orders", "RepairSave");

            return View();
        }

        public ActionResult Info(OrdersRepair OrdersRepair)
        {
            if (!OrdersRepair.Id.IsNullOrEmpty())
            {
                OrdersRepair = Entity.OrdersRepair.FirstOrDefault(n => n.Id == OrdersRepair.Id);
            }
            if (OrdersRepair == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.OrdersRepair = OrdersRepair;
            return View("Edit");
        }

        public ActionResult IndexOrdersRepair(string TNum)
        {
            var OrdersRepair = this.Entity.OrdersRepair.FirstOrDefault(o => o.TNum == TNum);
            if (OrdersRepair == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            var OrdersRepairLogList = this.Entity.OrdersRepairLog.Where(o => o.TNum == TNum).OrderByDescending(o=>o.AddTime).ToList();
            this.ViewBag.OrdersRepairLogList = OrdersRepairLogList;
            this.ViewBag.OrdersRepair = OrdersRepair;
            return View();
        }

        [HttpGet]
        public ActionResult Audit(int Id)
        {
            var OrdersRepair = Entity.OrdersRepair.FirstOrDefault(n => n.Id == Id);
            if (OrdersRepair == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicDescList1 = GetBasicDescList(BasicCodeEnum.Ddbdsh);
            ViewBag.OrdersRepair = OrdersRepair;
            return View("Edit");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Audit(OrdersRepair OrdersRepair)
        {
            if (!(OrdersRepair.TState == 2 || OrdersRepair.TState == 3))
            {
                ViewBag.ErrorMsg = "请审核";
                return View("Error");
            }
            if (OrdersRepair.AuditRemark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写审核备注";
                return View("Error");
            }
            var baseOrdersRepair = Entity.OrdersRepair.FirstOrDefault(n => n.Id == OrdersRepair.Id);
            if (baseOrdersRepair == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (baseOrdersRepair.TState != 1)
            {
                ViewBag.ErrorMsg = "已审核";
                return View("Error");
            }
            Orders baseOrders = Entity.Orders.FirstOrDefault(o => o.TNum == baseOrdersRepair.TNum);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }

            //审核通过
            if (OrdersRepair.TState == 2)
            {
                baseOrders.TState = 1;
                baseOrders.PaySuccess(this.Entity);

                if (baseOrders.PayState != 1)
                {
                    ViewBag.ErrorMsg = baseOrders.TNum + " 补单失败!";
                    return View("Error");
                }
            }
            //补单数据
            baseOrdersRepair.TState = OrdersRepair.TState;
            baseOrdersRepair.AuditAdminId = AdminUser.Id;
            baseOrdersRepair.AuditAdminName = AdminUser.TrueName;
            baseOrdersRepair.AuditRemark = OrdersRepair.AuditRemark;
            baseOrdersRepair.AuditTime = DateTime.Now;
            baseOrders.RepairState = OrdersRepair.TState;

            //操作记录
            var OrdersRepairLog = new OrdersRepairLog()
            {
                AddTime = DateTime.Now,
                TNum = OrdersRepair.TNum,
                LogType = OrdersRepair.TState,
                AdminId = AdminUser.Id,
                AdminName = AdminUser.TrueName,
                Remark = OrdersRepair.AuditRemark,
            };
            this.Entity.OrdersRepairLog.AddObject(OrdersRepairLog);

            this.Entity.SaveChanges();

            ViewBag.Msg = "操作成功，审核结果：" + (OrdersRepair.TState == 2 ? "通过" : "失败");
            return View("Succeed");
        }

    }
}
