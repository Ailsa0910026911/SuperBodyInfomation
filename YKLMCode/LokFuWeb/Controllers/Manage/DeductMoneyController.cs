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
    /// 商户扣款
    /// </summary>
    public class DeductMoneyController : BaseController
    {

        public ActionResult Index(DeductMoney DeductMoney, EFPagingInfo<DeductMoney> p, int IsFirst = 0, string Mobile = "")
        {
            if (!DeductMoney.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == DeductMoney.TState); }
            if (!DeductMoney.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName.Contains(DeductMoney.UserName)); }
            if (!Mobile.IsNullOrEmpty())
            {
                Users Users = Entity.Users.FirstOrDefault(n => n.Mobile == Mobile);
                if (Users != null) { p.SqlWhere.Add(f => f.UId == Users.Id); }
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<DeductMoney> DeductMoneyList = null;
            if (IsFirst == 0)
            {
                DeductMoneyList = new PageOfItems<DeductMoney>(new List<DeductMoney>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                DeductMoneyList = Entity.Selects<DeductMoney>(p);
            }
            List<int> UId = DeductMoneyList.Select(o => o.UId).Distinct().ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();

            ViewBag.DeductMoneyList = DeductMoneyList;
            ViewBag.DeductMoney = DeductMoney;
            ViewBag.Info = this.checkPower("Info");
            ViewBag.IsInfo = this.checkPower("Info");
            ViewBag.IsAudit = this.checkPower("Audit");
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            ViewBag.MobileSelect = Mobile;
            return View();
        }

        /// <summary>
        /// 查看
        /// </summary>
        /// <param name="DeductMoney"></param>
        /// <returns></returns>
        public ActionResult Info(int id)
        {
            var baseDeductMoney = Entity.DeductMoney.FirstOrDefault(n => n.Id == id);
            if (baseDeductMoney == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }

            ViewBag.DeductMoney = baseDeductMoney;
            ViewBag.Users = Entity.Users.FirstOrDefault(n => n.Id == baseDeductMoney.UId);
            return View();
        }

        [HttpGet]
        public ActionResult Audit(int id)
        {
            var baseDeductMoney = Entity.DeductMoney.FirstOrDefault(n => n.Id == id);
            if (baseDeductMoney == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }

            ViewBag.DeductMoney = baseDeductMoney;
            ViewBag.Users = Entity.Users.FirstOrDefault(n => n.Id == baseDeductMoney.UId);
            return View("Info");
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="DeductMoney"></param>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Audit(DeductMoney DeductMoney)
        {
            #region 校验与初始化
            if (!(DeductMoney.TState == 2 || DeductMoney.TState == 3))
            {
                ViewBag.ErrorMsg = "请选择审核结果";
                return View("Error");
            }
            if (DeductMoney.TState == 3 && DeductMoney.AuditRemark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写审核说明";
                return View("Error");
            }
            DeductMoney baseDeductMoney = Entity.DeductMoney.FirstOrDefault(n => n.Id == DeductMoney.Id);
            if (baseDeductMoney == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (baseDeductMoney.TState != 1)
            {
                ViewBag.ErrorMsg = "数据已经审核过了";
                return View("Error");
            }
            //用户扣款
            Users baseUsers = Entity.Users.FirstOrDefault(o => o.Id == baseDeductMoney.UId);
            if (baseUsers == null)
            {
                ViewBag.ErrorMsg = "商户不存在";
                return View("Error");
            }
            #endregion
            DeductMoney.AuditRemark = DeductMoney.AuditRemark ?? string.Empty;
            #region 扣款
            //审核通过
            if (DeductMoney.TState == 2)
            {
                //商户扣款
                int USERSID = baseUsers.Id;
                decimal PayMoney = baseDeductMoney.Amoney;
                string SP_Ret = Entity.SP_UsersMoney(USERSID, "人工扣款", PayMoney, 12, DeductMoney.AuditRemark);
                if (SP_Ret != "3")
                {
                    Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", USERSID, "人工扣款", 12, PayMoney, SP_Ret), "SP_UsersMoney");
                    ViewBag.ErrorMsg = "人工扣款失败";
                    return View("Error");
                }
            }
            #endregion
            //*避免被savechanges下面这些一定要放在后面
            baseDeductMoney.AuditRemark = DeductMoney.AuditRemark;
            baseDeductMoney.AuditAdminName = AdminUser.TrueName;
            baseDeductMoney.AuditAdminId = AdminUser.Id;
            baseDeductMoney.AuditTime = DateTime.Now;
            baseDeductMoney.TState = DeductMoney.TState;
            Entity.SaveChanges();
            ViewBag.Title = "操作成功";
            ViewBag.Msg = "操作结果：审核" + (baseDeductMoney.TState == 2 ? "通过" : "失败");
            return View("Succeed");
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public FileResult ExcelExport(DeductMoney DeductMoney, EFPagingInfo<DeductMoney> p, int IsFirst = 0, string Mobile = "")
        {
            if (!DeductMoney.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == DeductMoney.TState); }
            if (!DeductMoney.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName.Contains(DeductMoney.UserName)); }
            if (!Mobile.IsNullOrEmpty())
            {
                Users Users = Entity.Users.FirstOrDefault(n => n.Mobile == Mobile);
                if (Users != null) { p.SqlWhere.Add(f => f.UId == Users.Id); }
            }
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<DeductMoney> DeductMoneyList = null;
            if (IsFirst == 0)
            {
                DeductMoneyList = new PageOfItems<DeductMoney>(new List<DeductMoney>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                DeductMoneyList = Entity.Selects<DeductMoney>(p);
            }

            List<int> UId = DeductMoneyList.Select(o => o.UId).Distinct().ToList();
            IList<Users> UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();


            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("真实姓名", typeof(string)));
            table.Columns.Add(new DataColumn("金额", typeof(string)));
            table.Columns.Add(new DataColumn("状态", typeof(string)));
            table.Columns.Add(new DataColumn("手机号", typeof(string)));
            table.Columns.Add(new DataColumn("发起人", typeof(string)));
            table.Columns.Add(new DataColumn("发起时间", typeof(string)));
            table.Columns.Add(new DataColumn("审核人", typeof(string)));
            table.Columns.Add(new DataColumn("审核时间", typeof(string)));

            // 填充数据
            foreach (var item in DeductMoneyList)
            {
                Users Users = UsersList.FirstOrNew(n => n.Id == item.UId);
                row = table.NewRow();
                row[0] = item.UserName;
                row[1] = item.Amoney.ToString("f2");

                string TState = "待审核";
                switch (item.TState)
                {
                    case 1:
                        TState = "待审核";
                        break;
                    case 2:
                        TState = "审核通过";
                        break;
                    case 3:
                        TState = "审核不通过";
                        break;
                }

                row[2] = TState;
                row[3] = Users.Mobile;
                row[4] = item.CreateAdminName;
                row[5] = item.AddTime;
                row[6] = item.AuditAdminName;
                row[7] = (item.AuditTime.HasValue ? item.AuditTime.Value.ToString() : "");
                table.Rows.Add(row);
            }
            string Time = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99);
            return this.ExportExcelBase(table, "扣款管理-" + Time);
        }

    }
}
