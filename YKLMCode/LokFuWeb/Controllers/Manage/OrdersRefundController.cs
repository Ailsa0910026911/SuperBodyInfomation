using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// 退单
    /// </summary>
    public class OrdersRefundController : BaseController
    {
        public ActionResult Index(OrdersRefund OrdersRefund, EFPagingInfo<OrdersRefund> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            p = this.Condition(OrdersRefund, p, STime, ETime, IsFirst);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrdersRefund> OrdersRefundList = null;
            if (IsFirst == 0)
            {
                OrdersRefundList = new PageOfItems<OrdersRefund>(new List<OrdersRefund>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                OrdersRefundList = Entity.Selects<OrdersRefund>(p);
            }
            ViewBag.OrdersRefundList = OrdersRefundList;
            ViewBag.OrdersRefund = OrdersRefund;
            ViewBag.STime = STime;
            ViewBag.ETime = ETime;
            ViewBag.IsInfo = this.checkPower("Info");
            ViewBag.IsAudit = this.checkPower("Audit");
            ViewBag.IsAnewSubmit = this.checkPower("Orders", "Chargeback");
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            return View();
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
        public FileResult ExcelExport(OrdersRefund OrdersRefund, EFPagingInfo<OrdersRefund> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {

            p = this.Condition(OrdersRefund, p, STime, ETime, IsFirst);

            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrdersRefund> OrdersRefundList = null;
            if (IsFirst == 0)
            {
                OrdersRefundList = new PageOfItems<OrdersRefund>(new List<OrdersRefund>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                OrdersRefundList = Entity.Selects<OrdersRefund>(p);
            }
            DataTable table = new DataTable();
            DataRow row = null;
            // 创建 datatable
            table.Columns.Add(new DataColumn("订单号", typeof(string)));
            table.Columns.Add(new DataColumn("金额", typeof(string)));
            table.Columns.Add(new DataColumn("状态", typeof(string)));
            table.Columns.Add(new DataColumn("发起人", typeof(string)));
            table.Columns.Add(new DataColumn("发起时间", typeof(string)));
            table.Columns.Add(new DataColumn("过期时间", typeof(string)));
            table.Columns.Add(new DataColumn("审核人", typeof(string)));
            table.Columns.Add(new DataColumn("审核时间", typeof(string)));
            List<int> UId = new List<int>();
            foreach (var pp in OrdersRefundList)
            {
                UId.Add(pp.UId);
            }
            IList<Users> UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            // 填充数据
            foreach (var item in OrdersRefundList)
            {
                Users Users = UsersList.FirstOrNew(n => n.Id == item.UId);
                row = table.NewRow();
                row[0] = item.TNum;
                row[1] = item.Amoney.ToString("f2");

                string TState = "";
                switch (item.TState)
                {
                    case 1: 
                        TState = "待审核";
                        break;
                    case 2: 
                        TState = "审核通过";
                        break;
                    case 3:
                        TState = "审核失败";
                        break;
                }

                row[2] = TState;
                row[3] = item.CreateAdminName;
                row[4] = item.AddTime;
                row[5] = item.TDLastTime.HasValue ? item.TDLastTime.Value.ToString("yyyy-MM-dd") : "";
                row[6] = item.AuditAdminName;
                row[7] = (item.AuditTime.HasValue ? item.AuditTime.Value.ToString() : "");
                table.Rows.Add(row);
            }
            string Time = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99);
            return this.ExportExcelBase(table, "退款管理-" + Time);
        }
        [NonAction]
        private EFPagingInfo<OrdersRefund> Condition(OrdersRefund OrdersRefund, EFPagingInfo<OrdersRefund> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            if (!OrdersRefund.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == OrdersRefund.TNum); }
            if (!OrdersRefund.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == OrdersRefund.TState); }
            if (STime.HasValue)
            {
                if (OrdersRefund.CreateAdminId == 1)
                {
                    p.SqlWhere.Add(f => f.AddTime >= STime);
                }
                else if (OrdersRefund.CreateAdminId == 2)
                {
                    p.SqlWhere.Add(f => f.TDLastTime >= STime);
                }
                else
                {
                    p.SqlWhere.Add(f => f.AuditTime >= STime);
                }
            }
            if (ETime.HasValue)
            {
                if (OrdersRefund.CreateAdminId == 1)
                {
                    // var NETime = ETime.Value.AddDays(1);
                    var NETime = ETime.Value;
                    p.SqlWhere.Add(f => f.AddTime <= NETime);
                }
                else if (OrdersRefund.CreateAdminId == 2)
                {
                    p.SqlWhere.Add(f => f.TDLastTime <= ETime);
                }
                else
                {
                    p.SqlWhere.Add(f => f.AuditTime <= ETime);
                }
            }
            return p;
        }
        public ActionResult Info(OrdersRefund OrdersRefund)
        {
            OrdersRefund = Entity.OrdersRefund.FirstOrDefault(n => n.Id == OrdersRefund.Id);
            if (OrdersRefund == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }

            ViewBag.OrdersRefund = OrdersRefund;
            return View("Edit");
        }

        public ActionResult IndexOrdersRefund(string TNum)
        {
            var OrdersRefund = this.Entity.OrdersRefund.FirstOrDefault(o => o.TNum == TNum);
            if (OrdersRefund == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            var OrdersRefundLogList = this.Entity.OrdersRefundLog.Where(o => o.TNum == TNum).OrderByDescending(o => o.AddTime).ToList();
            this.ViewBag.OrdersRefundLogList = OrdersRefundLogList;
            this.ViewBag.OrdersRefund = OrdersRefund;
            return View();
        }

        [HttpGet]
        public ActionResult Audit(int Id)
        {
            var OrdersRefund = Entity.OrdersRefund.FirstOrDefault(n => n.Id == Id);
            if (OrdersRefund == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicDescList1 = GetBasicDescList(BasicCodeEnum.Ddtksh);
            ViewBag.OrdersRefund = OrdersRefund;
            return View("Edit");
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="DeductMoney"></param>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Audit(OrdersRefund OrdersRefund)
        {
            #region 校验及初始化
            if (!(OrdersRefund.TState == 2 || OrdersRefund.TState == 3))
            {
                ViewBag.ErrorMsg = "请审核";
                return View("Error");
            }
            if (OrdersRefund.TState == 3 && OrdersRefund.AuditRemark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写审核备注";
                return View("Error");
            }
            if (OrdersRefund.AuditRemark.IsNullOrEmpty())
            {
                OrdersRefund.AuditRemark = string.Empty;
            }
            var baseOrdersRefund = Entity.OrdersRefund.FirstOrDefault(n => n.Id == OrdersRefund.Id);
            if (baseOrdersRefund == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (baseOrdersRefund.TState != 1)
            {
                ViewBag.ErrorMsg = "已审核";
                return View("Error");
            }
            Orders baseOrders = Entity.Orders.FirstOrDefault(o => o.TNum == baseOrdersRefund.TNum);
            if (baseOrders == null)
            {
                ViewBag.ErrorMsg = "订单不存在";
                return View("Error");
            }
            Users baseUsers = Entity.Users.FirstOrDefault(o => o.Id == baseOrders.UId);
            if (baseUsers == null)
            {
                ViewBag.ErrorMsg = "订单商户不存在";
                return View("Error");
            }
            #endregion

            //实际到账金额
            decimal PayMoney = 0;
            #region 获取实际到账金额
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

            #region 审核通过
            if (OrdersRefund.TState == 2)
            {
                //订单扣款
                int USERSID = baseUsers.Id;
                string TNUM = baseOrders.TNum;

                string SP_Ret = Entity.SP_UsersMoney(USERSID, TNUM, PayMoney, 11, OrdersRefund.AuditRemark);
                if (SP_Ret != "3")
                {
                    Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, TNUM, 11, PayMoney, SP_Ret), "SP_UsersMoney");
                    ViewBag.ErrorMsg = "扣款失败";
                    return View("Error");
                }

                //订单信息修改
                baseOrders.TState = 4;
                baseOrders.TDState = OrdersRefund.TState;

                #region 退还佣金
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
            }
            #endregion

            #region 审核失败
            if (OrdersRefund.TState == 3)
            {
                //资金解冻
                #region 银联卡支付订单
                if (baseOrders.TType == 1)
                {
                    //包含下级分润
                    OrderRecharge.SetUnFrozen(Entity);
                }
                #endregion
                #region 微信 支付宝 NFC
                if (baseOrders.TType == 7 || baseOrders.TType == 8 || baseOrders.TType == 9)
                {
                    //包含下级分润
                    OrderF2F.SetUnFrozen(Entity);
                }
                #endregion
                baseOrders.FrozenState = 0;
                //修改冗余属性
                baseOrders.TDState = OrdersRefund.TState;
            }
            #endregion

            //保存退款信息,避免被savechanges
            baseOrdersRefund.AuditAdminId = this.AdminUser.Id;
            baseOrdersRefund.AuditAdminName = this.AdminUser.TrueName;
            baseOrdersRefund.AuditRemark = OrdersRefund.AuditRemark;
            baseOrdersRefund.TState = OrdersRefund.TState;
            baseOrdersRefund.AuditTime = DateTime.Now;
            //记录
            var OrdersRefundLog = new OrdersRefundLog()
            {
                AddTime = DateTime.Now,
                AdminId = this.AdminUser.Id,
                AdminName = this.AdminUser.TrueName,
                Img = string.Empty,
                LogType = OrdersRefund.TState,
                Remark = OrdersRefund.AuditRemark,
                TNum = OrdersRefund.TNum,
            };
            this.Entity.OrdersRefundLog.AddObject(OrdersRefundLog);
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功，审核结果：" + (OrdersRefund.TState == 2 ? "通过" : "失败");
            return View("Succeed");
        }

    }
}
