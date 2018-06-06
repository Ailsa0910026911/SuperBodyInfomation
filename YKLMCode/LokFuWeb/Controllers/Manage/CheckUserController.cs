using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Data;
namespace LokFu.Areas.Manage.Controllers
{
    public class CheckUserController : BaseController
    {
        public ActionResult Index(string UserName,DateTime? Date, EFPagingInfo<CheckUser> p, int show = 1)
        {
            p.PageSize = 50;
            DateTime TrueDate;
            if (Date.HasValue)
            {
                TrueDate = new DateTime(Date.Value.Year, Date.Value.Month, Date.Value.Day);
            }
            else
            {
                TrueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }
            IPageOfItems<CheckUser> CheckUserList = new PageOfItems<CheckUser>(new List<CheckUser>(), 0, 10, 0, new Hashtable()); ;
            var CheckUserMoneyList = new List<CheckUserMoney>();
            var UsersList = new List<CheckUserModel>();
            var CheckResultList = new List<CheckResult>();
            int uid = 0;
            int Count = 0;

            var CheckTaskList = this.Entity.CheckTask.Where(o => o.IsComplete == true).Select(o => o.TaskDate).ToList();
            var CheckTask = this.Entity.CheckTask.FirstOrDefault(o => o.TaskDate == TrueDate);

            if (!UserName.IsNullOrEmpty())
            {
                uid = this.Entity.Users.Where(o => o.UserName == UserName).Select(o => o.Id).FirstOrDefault();
                p.SqlWhere.Add(o => o.UId == uid);
            }
            if(CheckTask != null)
            {


                p.SqlWhere.Add(o => o.TaskDate == CheckTask.TaskDate);
                if (show == 1)
                {
                    p.SqlWhere.Add(o => o.NowMoney != o.CheckMoney || o.NowFrozen != o.CheckFrozen);
                }
                p.OrderByList.Add("Id", "DESC");
                CheckUserList = Entity.Selects<CheckUser>(p);
                Count = CheckUserList.TotalCount;

                var uids = CheckUserList.Select(o => o.UId).ToList();
                UsersList = this.Entity.Users.Where(o => uids.Contains(o.Id))
                    .Select(o => new CheckUserModel(){ Id = o.Id, TrueName = o.TrueName, UserName= o.UserName }).ToList();
                CheckUserMoneyList = this.Entity.CheckUserMoney.Where(o => uids.Contains(o.UId) && o.TaskDate == CheckTask.TaskDate).ToList();
            }
            ViewBag.CheckUserList = CheckUserList;
            ViewBag.CheckUserMoneyList = CheckUserMoneyList;
            ViewBag.UsersList = UsersList;
            ViewBag.CheckTask = CheckTask;
            ViewBag.CheckTaskList = CheckTaskList;
            ViewBag.TrueDate = TrueDate;
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            ViewBag.show = show;
            ViewBag.Count = Count;
            ViewBag.UserName = UserName;
            ViewBag.SaveRemark = this.checkPower("Remark");
            return View();
        }

        public FileResult ExcelExport(string UserName, DateTime Date, int show)
        {
            if (Date == null)
            {
                return null;
            }

            var fileName = "跑批" + Date.ToString("yyyyMMdd");
            string file = Server.MapPath("/template") + "\\CheckUser.xls";
            var export = new ExcelExport();
            var workbook = export.GetWorkbook(file);
            //var CellStyle = export.CellStyle(workbook);
           // var UserList = this.Entity.Users.ToList();
            List<CheckUserModel> UsersList = new List<CheckUserModel>();
            int uid = 0;
            #region 商户汇总
            if (true)
            {
                DataTable table = new DataTable();
                DataRow row = null;
                var query = this.Entity.CheckUser.AsQueryable();
                if (!UserName.IsNullOrEmpty())
                {
                    uid = this.Entity.Users.Where(o => o.UserName == UserName).Select(o => o.Id).FirstOrDefault();
                    query = query.Where(o => o.UId == uid);
                }
                if (show == 1)
                {
                    query = query.Where(o => o.NowMoney != o.CheckMoney || o.NowFrozen != o.CheckFrozen);
                }
                var CheckUserList = query.Where(o => o.TaskDate == Date).ToList();
                var uids = CheckUserList.Select(o => o.UId).ToList();
                UsersList = this.Entity.Users.Where(o => uids.Contains(o.Id))
                  .Select(o => new CheckUserModel() { Id = o.Id, TrueName = o.TrueName, UserName = o.UserName }).ToList();

                // 创建 datatable
                table.Columns.Add(new DataColumn("商户", typeof(string)));
                table.Columns.Add(new DataColumn("手机号", typeof(string)));
                table.Columns.Add(new DataColumn("原始余额", typeof(decimal)));
                table.Columns.Add(new DataColumn("原始冻结金额", typeof(decimal)));
                table.Columns.Add(new DataColumn("模拟余额", typeof(decimal)));
                table.Columns.Add(new DataColumn("模拟冻结金额", typeof(decimal)));
                table.Columns.Add(new DataColumn("记录余额", typeof(decimal)));
                table.Columns.Add(new DataColumn("记录冻结金额", typeof(decimal)));
                table.Columns.Add(new DataColumn("差额余额", typeof(decimal)));
                table.Columns.Add(new DataColumn("差额冻结金额", typeof(decimal)));

                // 填充数据
                foreach (var item in CheckUserList)
                {
                    var u = UsersList.FirstOrDefault(o => o.Id == item.UId);
                    row = table.NewRow();
                    row[0] = u.TrueName;
                    row[1] = u.UserName;
                    row[2] = item.PastMoney.ToString("n2");
                    row[3] = item.PastFrozen.ToString("n2");
                    row[4] = item.CheckMoney.ToString("n2");
                    row[5] = item.CheckFrozen.ToString("n2");
                    row[6] = item.NowMoney.ToString("n2");
                    row[7] = item.NowFrozen.ToString("n2");
                    row[8] = (item.CheckMoney - item.NowMoney).ToString("n2");
                    row[9] = (item.CheckFrozen - item.NowFrozen).ToString("n2");
                    table.Rows.Add(row);
                }

                var Sheet = workbook.GetSheetAt(0);
                Sheet.GetRow(0).GetCell(0).SetCellValue(Date.ToString("yyyy-MM-dd"));
                int Befor = 3;
                foreach (DataRow item in table.Rows)
                {
                    var dataRow = Sheet.CreateRow(Befor);
                    foreach (DataColumn column in table.Columns)
                    {
                        var newCell = dataRow.CreateCell(column.Ordinal);
                        var drValue = item[column].ToString();
                        newCell = export.Assign(newCell, column.DataType.ToString(), drValue);
                    }
                    Befor++;
                }
            }
            #endregion

            #region 跑批过程
            if (true)
            {
                DataTable table = new DataTable();
                DataRow row = null;
                var CheckUserMoneyList = this.Entity.CheckUserMoney.Where(o => o.TaskDate == Date).OrderBy(o=>o.Progress).ToList();

                // 创建 datatable
                table.Columns.Add(new DataColumn("商户", typeof(string)));
                table.Columns.Add(new DataColumn("手机号", typeof(string)));
                table.Columns.Add(new DataColumn("操作", typeof(string)));
                table.Columns.Add(new DataColumn("变动余额", typeof(decimal)));
                table.Columns.Add(new DataColumn("变动冻结金额", typeof(decimal)));
                table.Columns.Add(new DataColumn("运算操作金额", typeof(decimal)));
                table.Columns.Add(new DataColumn("记录操作金额", typeof(decimal)));
                table.Columns.Add(new DataColumn("差额", typeof(decimal)));

                // 填充数据
                foreach (var item in CheckUserMoneyList)
                {
                    var u = UsersList.FirstOrNew(o => o.Id == item.UId);
                    row = table.NewRow();
                    row[0] = u.TrueName;
                    row[1] = u.UserName;
                    row[2] = CheckExtensions.GetProgressName(item.Progress);
                    row[3] = item.ChangeMoney.ToString("n2");
                    row[4] = item.ChangeFrozen.ToString("n2");
                    row[5] = item.ProgressMoney.ToString("n2");
                    row[6] = item.RecordMoney.ToString("n2");
                    row[7] = (item.ProgressMoney - item.RecordMoney).ToString("n2");
                    table.Rows.Add(row);
                }
                var Sheet = workbook.GetSheetAt(1);
                Sheet.GetRow(0).GetCell(0).SetCellValue(Date.ToString("yyyy-MM-dd"));
                int Befor = 2;
                foreach (DataRow item in table.Rows)
                {
                    var dataRow = Sheet.CreateRow(Befor);
                    foreach (DataColumn column in table.Columns)
                    {
                        var newCell = dataRow.CreateCell(column.Ordinal);
                        var drValue = item[column].ToString();
                        newCell = export.Assign(newCell, column.DataType.ToString(), drValue);
                    }
                    Befor++;
                }
            }
            #endregion

            #region 异常订单
            if (true)
            {
                DataTable table = new DataTable();
                DataRow row = null;
                var CheckResultList = this.Entity.CheckResult.Where(o => o.TaskDate == Date).OrderByDescending(o => o.Id).ToList();

                // 创建 datatable
                table.Columns.Add(new DataColumn("商户", typeof(string)));
                table.Columns.Add(new DataColumn("手机号", typeof(string)));
                table.Columns.Add(new DataColumn("订单号", typeof(string)));
                table.Columns.Add(new DataColumn("金额", typeof(decimal)));
                table.Columns.Add(new DataColumn("操作", typeof(string)));
                table.Columns.Add(new DataColumn("异常内容", typeof(string)));

                // 填充数据
                foreach (var item in CheckResultList)
                {
                    var u = UsersList.FirstOrNew(o => o.Id == item.UId);
                    row = table.NewRow();
                    row[0] = u.TrueName;
                    row[1] = u.UserName;
                    row[2] = item.TNum;
                    row[3] = item.Amount.ToString("n2");
                    row[4] = CheckExtensions.GetProgressName(item.CheckType);
                    row[5] = item.CheckMsg;
                    table.Rows.Add(row);
                }
                var Sheet = workbook.GetSheetAt(2);
                Sheet.GetRow(0).GetCell(0).SetCellValue(Date.ToString("yyyy-MM-dd"));
                int Befor = 2;
                foreach (DataRow item in table.Rows)
                {
                    var dataRow = Sheet.CreateRow(Befor);
                    foreach (DataColumn column in table.Columns)
                    {
                        var newCell = dataRow.CreateCell(column.Ordinal);
                        var drValue = item[column].ToString();
                        newCell = export.Assign(newCell, column.DataType.ToString(), drValue);
                    }
                    Befor++;
                }
            }
            #endregion

            return ExportExcelBase(workbook, fileName);
        }

        [HttpGet]
        public ActionResult Remark(int Id)
        {
            var CheckUser = this.Entity.CheckUser.Where(o => o.Id == Id).FirstOrDefault();
            if (CheckUser == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.CheckUser = CheckUser;
            return View();
        }

        [HttpPost]
        public ActionResult Remark(CheckUser CheckUser)
        {
            var baseCheckUser = this.Entity.CheckUser.Where(o => o.Id == CheckUser.Id).FirstOrDefault();
            if (baseCheckUser == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            baseCheckUser.Remark = CheckUser.Remark;
            this.Entity.SaveChanges();
            BaseRedirect();
            return null;
        }
    }

    public class CheckUserModel
    {
        public int Id { get; set; }
        public string TrueName { get; set; }
        public string UserName { get; set; }
    }

    public static class CheckExtensions
    {
        public static Dictionary<int, string> TaskProgress = new Dictionary<int, string>()
        {
            {10,"T0-交易订单-支付"},
            {11,"T0-交易订单-分润"},
            {20,"TN-交易订单-支付冻结"},
            {21,"TN-交易订单-分润冻结"},
            {30,"TN-交易订单-支付解冻"},
            {31,"TN-交易订单-分润解冻"},
            {40,"提现-申请冻结"},
            {41,"提现-审核成功"},
            {42,"提现-审核失败"},
            {43,"提现-分润"},
            {44,"提现-审核退款"},
            {45,"提现-分润退款"},
            {49,"升级-支付"},
            {50,"升级-分润"},
            {60,"交易调单-自动-支付冻结"},
            {61,"交易调单-自动-分润冻结"},
            {62,"交易调单-手动-支付冻结"},
            {63,"交易调单-手动-分润冻结"},
            {65,"交易调单-审核-支付退款"},
            {66,"交易调单-审核-分润退款"},
            {68,"交易调单-审核-支付解冻"},
            {69,"交易调单-审核-分润解冻"},
            {70,"交易退款-支付冻结"},
            {71,"交易退款-分润冻结"},
            {72,"交易退款-审核-支付解冻"},
            {73,"交易退款-审核-分润解冻"},
            {74,"交易退款-审核-支付退款"},
            {75,"交易退款-审核-分润退款"},
            {80,"理财转入"},
            {81,"理财转出"},
            {90,"转账转入"},
            {91,"转账转出"},
            {100,"扣款"},
            {105,"部分止付"},
            {106,"解除部分止付"},
            {110,"鉴权"},
            {120,"理财收益"},
            {121,"余额奖励金"},
            {131,"交易订单-分润退款"},
            {132,"直通车-分润"},
            {135,"自动还-分润"},
        };

        public static string GetProgressName(int Progress)
        {
            return TaskProgress.Where(o => o.Key == Progress).Select(o => o.Value).FirstOrDefault() ?? string.Empty;
        }
    }
}
