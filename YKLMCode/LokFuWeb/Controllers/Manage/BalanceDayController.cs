using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.Extensions;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace LokFu.Areas.Manage.Controllers
{
    public class BalanceDayController : BaseController
    {
        public ActionResult Index(DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (STime == null || ETime == null)
            {
                ViewBag.ErrorMsg = "查询时间有误！";
                return View("Error");
            }

            ViewBag.Xls = this.checkPower("Xls");
            this.TempData["BaoStoryList"] = null;
            IList<BaoStory> BaoStoryList = null;
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            if (!STime.IsNullOrEmpty())
            {
                dicChar.Add("S_Time", STime.Value.ToString("yyyy-MM-dd"));
            }
            if (!ETime.IsNullOrEmpty())
            {
                ETime = ETime.Value.AddDays(1);
                dicChar.Add("E_Time", ETime.Value.ToString("yyyy-MM-dd"));
            }
            if (IsFirst > 0)
            {
                BaoStoryList = Entity.GetSPExtensions<BaoStory>("SP_StAtistics_Interest", dicChar);
            }
            ViewBag.S_Time = STime;
            ViewBag.E_Time = ETime.Value.AddDays(-1);
            ViewBag.BaoStoryList = BaoStoryList != null ? BaoStoryList.OrderByDescending(x => x.SDate).ToList() : null;
            this.TempData["BaoStoryList"] = BaoStoryList;
            this.TempData.Peek("BaoStoryList");
            return View();
        }

        /// <summary>
        /// 统计
        /// </summary>
        /// <returns></returns>
        public ActionResult IndexStats(DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (STime == null || ETime == null)
            {
                ViewBag.ErrorMsg = "查询时间有误！";
                return View("Error");
            }

            IList<SP_Statistics_InterestSUM> StatsList = null;
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            if (!STime.IsNullOrEmpty())
            {
                dicChar.Add("S_Time", STime.Value.ToString("yyyy-MM-dd"));
            }
            if (!ETime.IsNullOrEmpty())
            {
                ETime = ETime.Value.AddDays(1);
                dicChar.Add("E_Time", ETime.Value.ToString("yyyy-MM-dd"));
            }
            if (IsFirst > 0)
            {
                StatsList = Entity.GetSPExtensions<SP_Statistics_InterestSUM>("SP_Statistics_InterestSUM", dicChar);
            }

            ViewBag.StatsList = StatsList;
            return this.View();
        }
        public FileResult XLSDo(DateTime? STime, DateTime? ETime)
        {
            var BaoStoryList = this.TempData["BaoStoryList"] as IList<BaoStory>;
            if (BaoStoryList == null)
            {
                Response.Write("导出数据为空！");
                return null;
            }
            if (STime == null || ETime == null)
            {
                Response.Write("导出条件有误！");
                return null;
            }
            BaoStoryList = BaoStoryList.OrderByDescending(x => x.SDate).ToList();
            DataTable table = new DataTable();
            string fileName = "好付钱包余额理财汇总报表" + STime.Value.ToString("yyyy-MM-dd") + "至" + ETime.Value.ToString("yyyy-MM-dd");
            table.Columns.Add(new DataColumn("日期", typeof(string)));
            table.Columns.Add(new DataColumn("类型", typeof(string)));
            table.Columns.Add(new DataColumn("存入", typeof(string)));
            table.Columns.Add(new DataColumn("取出", typeof(string)));
            table.Columns.Add(new DataColumn("余额", typeof(string)));
            table.Columns.Add(new DataColumn("利息", typeof(string)));
            table.Columns.Add(new DataColumn("计息余额", typeof(string)));
            table.Columns.Add(new DataColumn("未计息余额", typeof(string)));
            table.Columns.Add(new DataColumn("万分收益", typeof(string)));
            table.Columns.Add(new DataColumn("年化收益", typeof(string)));

            DataRow row = null;
            foreach (var model in BaoStoryList)
            {
                row = table.NewRow();
                row[0] = model.SDate.ToString("yyyy-MM-dd");
                row[1] = model.LType == 1 ? " 理财利息" : "余额奖励金";
                row[2] = model.InMoney.ToString("F2");
                row[3] = model.OutMoney.ToString("F2");
                row[4] = model.BfAllMoney.ToString("F2");
                row[5] = model.Interest.ToString("F2");
                row[6] = model.BfActMoney.ToString("F2");
                row[7] = model.BfInMoney.ToString("F2");
                row[8] = model.GetCost.ToString("F2");
                row[9] = model.YearPer.ToString("N4") + "%";
                table.Rows.Add(row);
            }
            row = table.NewRow();
            row[0] = "总计：";
            row[1] = "";
            row[2] = BaoStoryList.Sum(x => x.InMoney).ToString("F2");
            row[3] = BaoStoryList.Sum(x => x.OutMoney).ToString("F2");
            row[4] = "";
            row[5] = BaoStoryList.Sum(x => x.Interest).ToString("F2");
            row[6] = "";
            row[7] = "";
            row[8] = "";
            row[9] = "";
            table.Rows.Add(row);
            return ExportExcelBase(table, fileName);
        }
    }

    public class SP_Statistics_InterestSUM
    {
        public Byte LType { get; set; }
        public decimal Interest {get;set;}

    }
}
