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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data;
namespace LokFu.Areas.Manage.Controllers
{
    public class SameGetController : BaseController
    {

        public ActionResult Index(OrderProfitLog OrderProfitLog, EFPagingInfo<OrderProfitLog> p, int IsFirst = 0)
        {
            this.TempData.Remove("SysAgentList");
            #region 校验与初始化
            //if (OrderProfitLog.STime.IsNullOrEmpty())
            //{
            //    OrderProfitLog.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //}
            //if (OrderProfitLog.ETime.IsNullOrEmpty())
            //{
            //    //OrderProfitLog.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            //    OrderProfitLog.ETime = DateTime.Now;
            //}
            p.SqlWhere.Add(f => f.LogType ==3);
            if (!OrderProfitLog.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == OrderProfitLog.TNum); }
            if (!OrderProfitLog.OrderType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OrderType == OrderProfitLog.OrderType); }
            if (!OrderProfitLog.Agent.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Agent == OrderProfitLog.Agent);
            }
            if (!OrderProfitLog.STime.IsNullOrEmpty() && !OrderProfitLog.ETime.IsNullOrEmpty())
            {
                DateTime ETime = OrderProfitLog.ETime;
                p.SqlWhere.Add(f => f.AddTime > OrderProfitLog.STime && f.AddTime < ETime);
            }
            #endregion
            p.PageSize = 20;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrderProfitLog> OrderProfitLogList = null;
            if (IsFirst == 0)
            {
                OrderProfitLogList = new PageOfItems<OrderProfitLog>(new List<OrderProfitLog>(), 0, 20, 0, new Hashtable());
            }
            else
            {
                OrderProfitLogList = Entity.Selects<OrderProfitLog>(p);
            }
            ViewBag.OrderProfitLogList = OrderProfitLogList;
            ViewBag.OrderProfitLog = OrderProfitLog;
            IList<OrderProfitLog> List = OrderProfitLogList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = OrderProfitLogList.Select(o => o.UId).Distinct().ToList();
            IList<Users> UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();

            List<int> AgentId = OrderProfitLogList.Select(o => o.Agent).Distinct().ToList();
            IList<SysAgent> AgentList = Entity.SysAgent.Where(n => n.State == 1 && AgentId.Contains(n.Id)).ToList();
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => AgentId.Contains(n.Id)).ToList();
            this.TempData["SysAgentList"] = ViewBag.SysAgentList;
            ViewBag.SelectSysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.Xls = this.checkPower("Xls");
            return View();
        }

        public ActionResult XLSDo(OrderProfitLog OrderProfitLog, EFPagingInfo<OrderProfitLog> p)
        {
            p.SqlWhere.Add(f => f.LogType ==3);
            if (!OrderProfitLog.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == OrderProfitLog.TNum); }
            if (!OrderProfitLog.LogType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.LogType == OrderProfitLog.LogType); }
            if (!OrderProfitLog.Agent.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Agent == OrderProfitLog.Agent);
            }
            if (!OrderProfitLog.STime.IsNullOrEmpty() && !OrderProfitLog.ETime.IsNullOrEmpty())
            {
                DateTime ETime = OrderProfitLog.ETime;
                p.SqlWhere.Add(f => f.AddTime > OrderProfitLog.STime && f.AddTime < ETime);
            }
            p.PageSize = 999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<OrderProfitLog> OrderProfitLogList = null;
            OrderProfitLogList = Entity.Selects<OrderProfitLog>(p);
            IList<OrderProfitLog> List = OrderProfitLogList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = OrderProfitLogList.Select(o => o.UId).Distinct().ToList();
            IList<Users>  UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();

            List<int> AgentId = OrderProfitLogList.Select(o => o.Agent).Distinct().ToList();
            IList<SysAgent> AgentList=Entity.SysAgent.Where(n=>n.State==1&&AgentId.Contains(n.Id)).ToList();
            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("交易号", typeof(string)));
            table.Columns.Add(new DataColumn("交易商户", typeof(string)));
            table.Columns.Add(new DataColumn("交易类型", typeof(string)));
            table.Columns.Add(new DataColumn("交易金额", typeof(string)));
            table.Columns.Add(new DataColumn("分润金额", typeof(string)));
            table.Columns.Add(new DataColumn("分润时间", typeof(string)));
            table.Columns.Add(new DataColumn("代理商名称", typeof(string)));
            table.Columns.Add(new DataColumn("代理商层级", typeof(string)));
            string state = "";
            //订单按照支付时间排序
            foreach (var item in OrderProfitLogList)
            {
                 Users Users = UsersList.FirstOrNew(n => n.Id == item.UId);
                 SysAgent SysAgent = AgentList.FirstOrNew(o => o.Id == item.Agent);
                if (item.OrderType == 21)
                {
                    state="直通车交易";
                }
                else if (item.OrderType == 10)
                {
                    state="自助开通代理";
                }
                else if (item.OrderType == 31)
                {
                    state="刷还交易";
                }
                row = table.NewRow();
                row[0] = item.TNum;
                row[1] = Users.TrueName;
                row[2] = state;
                row[3] = item.Amoney;
                row[4] = item.Profit;
                row[5] = item.AddTime.ToString("yyyy-MM-dd HH:mm");
                row[6] = SysAgent.Name;
                row[7] = item.Tier+"级";
                table.Rows.Add(row);
             }
                  string Time = DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(10, 99);
                  return this.ExportExcelBase(table, "同级分润明细-" + Time);
        }
    }
}
