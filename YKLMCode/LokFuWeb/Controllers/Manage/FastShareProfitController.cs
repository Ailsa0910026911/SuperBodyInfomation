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
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// 直通车分润管理
    /// </summary>
    public class FastShareProfitController : BaseController
    {
        public ActionResult Index(FastOrder FastOrder, EFPagingInfo<Orders> p)
        {
            p.PageSize = 20;
            string STimeStr = string.Empty;
            string ETimeStr = string.Empty;
            string TNumStr = string.Empty;
            if (FastOrder.STime == null || FastOrder.STime == DateTime.MinValue)
            {
                FastOrder.STime = DateTime.Now.AddDays(-1);
            }
            if (FastOrder.ETime == null || FastOrder.ETime == DateTime.MinValue)
            {
                FastOrder.ETime = DateTime.Now;
            }

            TimeSpan TS = FastOrder.ETime.Subtract(FastOrder.STime);
            int Days = TS.Days;
            if (Days > 31)
            {
                ViewBag.ErrorMsg = "统计时间间隔不能超过31天！";
                return View("Error");
            }
            STimeStr = " AND L.AddTime >= '" + FastOrder.STime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            ETimeStr = " AND L.AddTime <= '" + FastOrder.ETime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            if(!FastOrder.TNum.IsNullOrEmpty())
            {
                TNumStr = " AND L.TNum = '" + FastOrder.TNum.ToString() + "'";
            }
            int s = ((p.PageIndex == 0 ? 1 : p.PageIndex) - 1) * p.PageSize;
            int e = (p.PageIndex + 1) * p.PageSize;
            var DataList = new List<FastShareProfitModel>();
            StringBuilder sql = new StringBuilder();
            sql.Append(" select * From ( ");
            sql.Append(" select O.Id,row_number() OVER (ORDER BY O.Id DESC) AS RowNumber,O.TNum,O.OType,O.UserState,O.Amoney,ISNULL(L.UId,0) UId,ISNULL(L.LogType,0) LogType,ISNULL(Sum(L.Profit), 0) Profit,L.AddTime  ");
            sql.Append(" From FastOrder(nolock) O Left Join OrderProfitLog(nolock) L ON O.TNum=L.TNum AND L.IsDel=0 ");
            sql.Append(" Where O.IsDel=0 " + STimeStr + ETimeStr + TNumStr);
            sql.Append(" Group By O.Id,O.TNum,L.UId,O.Amoney,O.OType,O.UserState,L.LogType,L.AddTime ");
            sql.Append(" ) AS D Where RowNumber BETWEEN " + s.ToString() + " AND " + e.ToString() + " Order By Id desc");
            DataList = Entity.ExecuteStoreQuery<FastShareProfitModel>(sql.ToString(), null).ToList();

            StringBuilder sql2 = new StringBuilder();
            sql2.Append(" select Count(1) c From ( ");
            sql2.Append(" select O.Id ");
            sql2.Append(" From FastOrder(nolock) O Left Join OrderProfitLog(nolock) L ON O.TNum=L.TNum AND L.IsDel=0 ");
            sql2.Append(" Where O.IsDel=0 " + STimeStr + ETimeStr + TNumStr);
            sql2.Append(" Group By O.Id,O.TNum,L.UId,O.Amoney,O.OType,O.UserState,L.LogType,L.AddTime ");
            sql2.Append(" ) AS D ");
            int c = Entity.ExecuteStoreQuery<int>(sql2.ToString(), null).FirstOrDefault();

            DataList = new PageOfItems<FastShareProfitModel>(DataList, p.PageIndex, p.PageSize, c, null);

            var UIds = DataList.Select(o => o.UId).ToList();
            var UsersList = this.Entity.Users.Where(o => UIds.Contains(o.Id)).ToList();

            ViewBag.UsersList = UsersList;
            ViewBag.DataList = DataList;
            ViewBag.FastOrder = FastOrder;
            ViewBag.Xls = this.checkPower("ExcelExport");
            return View();
        }

        public FileResult ExcelExport(FastOrder FastOrder, EFPagingInfo<FastOrder> p)
        {
            p.PageSize = 9999999;
            #region 查询条件
            string STimeStr = string.Empty;
            string ETimeStr = string.Empty;
            string TNumStr = string.Empty;
            if (FastOrder.STime == null || FastOrder.STime == DateTime.MinValue)
            {
                FastOrder.STime = DateTime.Now.AddDays(-1);
            }
            if (FastOrder.ETime == null || FastOrder.ETime == DateTime.MinValue)
            {
                FastOrder.ETime = DateTime.Now;
            }

            TimeSpan TS = FastOrder.ETime.Subtract(FastOrder.STime);
            int Days = TS.Days;
            if (Days > 31)
            {
                return null;
            }
            STimeStr = " AND L.AddTime >= '" + FastOrder.STime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            ETimeStr = " AND L.AddTime <= '" + FastOrder.ETime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            if (!FastOrder.TNum.IsNullOrEmpty())
            {
                TNumStr = " AND L.TNum = " + FastOrder.TNum.ToString();
            }
            int s = ((p.PageIndex == 0 ? 1 : p.PageIndex) - 1) * p.PageSize;
            int e = (p.PageIndex + 1) * p.PageSize;
            var DataList = new List<FastShareProfitModel>();
            StringBuilder sql = new StringBuilder();
            sql.Append(" select * From ( ");
            sql.Append(" select O.Id,row_number() OVER (ORDER BY O.Id DESC) AS RowNumber,O.TNum,O.OType,O.UserState,O.Amoney,ISNULL(L.UId,0) UId,ISNULL(L.LogType,0) LogType,ISNULL(Sum(L.Profit), 0) Profit,L.AddTime  ");
            sql.Append(" From FastOrder(nolock) O Left Join OrderProfitLog(nolock) L ON O.TNum=L.TNum AND L.IsDel=0 ");
            sql.Append(" Where O.IsDel=0 " + STimeStr + ETimeStr + TNumStr);
            sql.Append(" Group By O.Id,O.TNum,L.UId,O.Amoney,O.OType,O.UserState,L.LogType,L.AddTime ");
            sql.Append(" ) AS D Where RowNumber BETWEEN " + s.ToString() + " AND " + e.ToString() + " Order By Id desc");
            DataList = Entity.ExecuteStoreQuery<FastShareProfitModel>(sql.ToString(), null).ToList();
            var UIds = DataList.Select(o => o.UId).ToList();
            var UsersList = this.Entity.Users.Where(o => UIds.Contains(o.Id)).ToList();
            #endregion

            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("交易号", typeof(string)));
            table.Columns.Add(new DataColumn("交易类型", typeof(string)));
            table.Columns.Add(new DataColumn("交易金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("结算状态", typeof(string)));
            table.Columns.Add(new DataColumn("分润商户", typeof(string)));
            table.Columns.Add(new DataColumn("分润类型", typeof(string)));
            table.Columns.Add(new DataColumn("分润金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("分润时间", typeof(string)));

            // 填充数据
            foreach (var item in DataList)
            {

                var u = UsersList.FirstOrNew(o => o.Id == item.UId);
                row = table.NewRow();
                row[0] = item.TNum;
                row[1] = Utils.GetFastOrderModel().FirstOrNew(n => n.Id == item.OType).Name;
                row[2] = item.Amoney.ToString("F2");
                row[3] = UserStateName(item.UserState);
                row[4] = u.UserName;
                row[5] = LogTypeName(item.LogType);
                row[6] = item.Profit.ToString("F2");
                row[7] = item.AddTime.ToString("yyyy-MM-dd HH:mm:ss");
                table.Rows.Add(row);
            }

            return this.ExportExcelBase(table, "直通车分润");
        }


        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(FastOrder FastOrder, EFPagingInfo<FastOrder> p)
        {
            p.PageSize = 9999999;
            string STimeStr = string.Empty;
            string ETimeStr = string.Empty;
            string TNumStr = string.Empty;
            if (FastOrder.STime == null || FastOrder.STime == DateTime.MinValue)
            {
                FastOrder.STime = DateTime.Now.AddDays(-1);
            }
            if (FastOrder.ETime == null || FastOrder.ETime == DateTime.MinValue)
            {
                FastOrder.ETime = DateTime.Now;
            }

            TimeSpan TS = FastOrder.ETime.Subtract(FastOrder.STime);
            int Days = TS.Days;
            if (Days > 31)
            {
                return null;
            }
            STimeStr = " AND L.AddTime >= '" + FastOrder.STime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            ETimeStr = " AND L.AddTime <= '" + FastOrder.ETime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            if (!FastOrder.TNum.IsNullOrEmpty())
            {
                TNumStr = " AND L.TNum = " + FastOrder.TNum.ToString();
            }
            int s = ((p.PageIndex == 0 ? 1 : p.PageIndex) - 1) * p.PageSize;
            int e = (p.PageIndex + 1) * p.PageSize;
            var DataList = new List<FastShareProfitModel>();
            StringBuilder sql = new StringBuilder();
            sql.Append(" select * From ( ");
            sql.Append(" select O.Id,row_number() OVER (ORDER BY O.Id DESC) AS RowNumber,O.TNum,O.OType,O.UserState,O.Amoney,ISNULL(L.UId,0) UId,ISNULL(L.LogType,0) LogType,ISNULL(Sum(L.Profit), 0) Profit,L.AddTime  ");
            sql.Append(" From FastOrder(nolock) O Left Join OrderProfitLog(nolock) L ON O.TNum=L.TNum AND L.IsDel=0 ");
            sql.Append(" Where O.IsDel=0 " + STimeStr + ETimeStr + TNumStr);
            sql.Append(" Group By O.Id,O.TNum,L.UId,O.Amoney,O.OType,O.UserState,L.LogType,L.AddTime ");
            sql.Append(" ) AS D Where RowNumber BETWEEN " + s.ToString() + " AND " + e.ToString() + " Order By Id desc");
            DataList = Entity.ExecuteStoreQuery<FastShareProfitModel>(sql.ToString(), null).ToList();
            ViewBag.SumAmoney = DataList.Sum(o => o.Amoney);
            ViewBag.SumProfit = DataList.Sum(o => o.Profit);
            ViewBag.Count = DataList.Count;
            return this.View();
        }
        private string UserStateName(int UserState)
        { 
            string reslut = string.Empty;
            switch (UserState)
            {
                case 1:
                    reslut = "已结算";
                    break;
                case 2:
                    reslut = "结算失败";
                    break;
                case 3:
                    reslut = "处理中";
                    break;
                default:
                    reslut = "未结算";
                    break;
            }
            return reslut;
        }

        private string LogTypeName(int LogType)
        { 
            string reslut = string.Empty;
            switch (LogType)
            {
                case 1:
                    reslut = "商户分润";
                    break;
                case 2:
                    reslut = "代理分润";
                    break;
            }
            return reslut;
        }
        
    }

    public class FastShareProfitModel
    {
        public int UId { get; set; }

        public byte LogType { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string TNum { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        public decimal Amoney { get; set; }

        /// <summary>
        /// 交易类型
        /// </summary>
        public byte OType { get; set; }

        /// <summary>
        /// 结算状态
        /// </summary>
        public byte UserState { get; set; }

        /// <summary>
        /// 分润金额
        /// </summary>
        public decimal Profit { get;set; }

        /// <summary>
        /// 分润时间
        /// </summary>
        public DateTime AddTime { get; set; }
    }
}
