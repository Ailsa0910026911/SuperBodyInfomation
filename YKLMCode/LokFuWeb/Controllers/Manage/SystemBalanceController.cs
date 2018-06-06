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
    public class SystemBalanceController : BaseController
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
            IList<SystemBalance> SystemBalanceList = null;
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            if (!STime.IsNullOrEmpty())
            {
                dicChar.Add("STIME", STime.Value.ToString("yyyy-MM-dd"));
            }
            if (!ETime.IsNullOrEmpty())
            {
               // ETime = ETime.Value.AddDays(1);
                dicChar.Add("ETIME", ETime.Value.ToString("yyyy-MM-dd"));
            }
            if (IsFirst > 0)
            {
                SystemBalanceList = Entity.GetSPExtensions<SystemBalance>("SP_Statistics_Account", dicChar);
            }
            ViewBag.S_Time = STime;
            ViewBag.E_Time = ETime;
            ViewBag.SystemBalanceList = SystemBalanceList != null ? SystemBalanceList.OrderByDescending(x => x.dated).ToList() : null;
            ViewBag.Xls = this.checkPower("Xls");
            return View();
        }
        //导出报表
        public FileResult XLSDo(DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            if (!STime.IsNullOrEmpty() && !ETime.IsNullOrEmpty())
            {
                IList<SystemBalance> SystemBalanceList = null;
                Dictionary<string, string> dicChar = new Dictionary<string, string>();
                dicChar.Add("STIME", STime.Value.ToString("yyyy-MM-dd"));               
                dicChar.Add("ETIME", ETime.Value.ToString("yyyy-MM-dd"));

                if (IsFirst > 0)
                {
                    SystemBalanceList = Entity.GetSPExtensions<SystemBalance>("SP_Statistics_Account", dicChar);
                }
                else
                {
                    Response.Write("请先查询数据！"); return null;
                }
                if (SystemBalanceList != null && SystemBalanceList.Count()>0)
                {
                    SystemBalanceList = SystemBalanceList != null ? SystemBalanceList.OrderByDescending(x => x.dated).ToList() : null;

                    DataTable table = new DataTable();
                    DataRow row = null;
                    
                    //创建表
                    table.Columns.Add(new DataColumn("日期", typeof(string)));
                    table.Columns.Add(new DataColumn("余额", typeof(string)));
                    table.Columns.Add(new DataColumn("冻结金额", typeof(string)));
                    string Nbsp = "                       ";
                    foreach (var item in SystemBalanceList)
                    {
                        row = table.NewRow();
                        row[0] = item.dated.ToString("yyyy-MM-dd") + Nbsp;
                        row[1] = item.Amount.ToString("F2") + Nbsp;
                        row[2] = item.Frozen.ToString("F2") + Nbsp;
                        table.Rows.Add(row);
                    }
                    string fileName = "系统结余";
                    string Time = STime.Value.ToString("yyyy-MM-dd") + "至" + ETime.Value.ToString("yyyy-MM-dd");
                    return this.ExportExcelBase(table, fileName + Time);
                }
                else
                {
                    Response.Write("暂无符合条件数据！"); return null;
                }
            }
            else
            {
                Response.Write("查询时间有误！"); return null;
            }
        }        
    }
    public class SystemBalance
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime dated { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 冻结
        /// </summary>
        public decimal Frozen { get; set; }

    }
}
