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
    public class DailyComparedController : BaseController
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
            ViewBag.DB_Account_DailyComparedList = Entity.DB_Account_DailyCompared.Where(o => o.DATED >= STime && o.DATED <= ETime).OrderBy(o=>o.DATED).ToList();
            ViewBag.Xls = this.checkPower("Xls");
            ViewBag.S_Time = STime;
            ViewBag.E_Time = ETime;
            return View();
        }
        //导出报表
        public FileResult XLSDo(DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            if (!STime.IsNullOrEmpty() && !ETime.IsNullOrEmpty())
            {
                IList<DB_Account_DailyCompared> SystemBalanceList = null;
                
                if (IsFirst > 0)
                {
                    SystemBalanceList = Entity.DB_Account_DailyCompared.Where(o => o.DATED >= STime && o.DATED <= ETime).OrderBy(o => o.DATED).ToList();
                }
                else
                {
                    Response.Write("请先查询数据！"); return null;
                }
                if (SystemBalanceList != null && SystemBalanceList.Count() > 0)
                {
                    
                    DataTable table = new DataTable();
                    DataRow row = null;

                    //创建表
                    table.Columns.Add(new DataColumn("日期", typeof(string)));
                    table.Columns.Add(new DataColumn("差额", typeof(string)));
                    table.Columns.Add(new DataColumn("充值交易金额", typeof(string)));
                    table.Columns.Add(new DataColumn("充值交易手续费", typeof(string)));
                    table.Columns.Add(new DataColumn("支付宝金额", typeof(string)));
                    table.Columns.Add(new DataColumn("支付宝手续费", typeof(string)));
                    table.Columns.Add(new DataColumn("微信金额", typeof(string)));
                    table.Columns.Add(new DataColumn("微信手续费", typeof(string)));
                    table.Columns.Add(new DataColumn("NFC金额", typeof(string)));
                    table.Columns.Add(new DataColumn("NFC手续费", typeof(string)));
                    table.Columns.Add(new DataColumn("前期转帐非余额支付金额", typeof(string)));
                    table.Columns.Add(new DataColumn("前期转帐非余额支付手续费", typeof(string)));
                    table.Columns.Add(new DataColumn("用户资金可用余额", typeof(string)));
                    table.Columns.Add(new DataColumn("用户资金冻结金额", typeof(string)));
                    table.Columns.Add(new DataColumn("利息收入", typeof(string)));
                    table.Columns.Add(new DataColumn("红包中奖", typeof(string)));
                    table.Columns.Add(new DataColumn("分润", typeof(string)));
                    table.Columns.Add(new DataColumn("余额奖励金", typeof(string)));
                    table.Columns.Add(new DataColumn("提现交易", typeof(string)));
                    table.Columns.Add(new DataColumn("房租订单", typeof(string)));

                    table.Columns.Add(new DataColumn("升级订单", typeof(string)));
                    table.Columns.Add(new DataColumn("人工扣款", typeof(string)));
                    table.Columns.Add(new DataColumn("鉴权", typeof(string)));
                    table.Columns.Add(new DataColumn("理财帐户总额", typeof(string)));
                    
                    foreach (var item in SystemBalanceList)
                    {
                        row = table.NewRow();
                        row[0] = item.DATED.Value.ToString("yyyy-MM-dd");
                        row[1] = item.DiffResult.ToMoney();
                        row[2] = item.ORDERS_1.ToMoney();
                        row[3] = item.ORDERS_P1.ToMoney();
                        row[4] = item.ORDERS_7.ToMoney();
                        row[5] = item.ORDERS_P7.ToMoney();
                        row[6] = item.ORDERS_8.ToMoney();
                        row[7] = item.ORDERS_P8.ToMoney();
                        row[8] = item.ORDERS_9.ToMoney();
                        row[9] = item.ORDERS_P9.ToMoney();
                        row[10] = item.ORDERS_3.ToMoney();
                        row[11] = item.ORDERS_P3.ToMoney();
                        row[12] = item.U_Amony.ToMoney();
                        row[13] = item.U_Frozen.ToMoney();
                        row[14] = item.Baglog.ToMoney();
                        row[15] = item.TurnLog.ToMoney();
                        row[16] = item.OrderProfitLog.ToMoney();
                        row[17] = item.Userlog15.ToMoney();
                        row[18] = item.ORDERS_2.ToMoney();
                        row[19] = item.ORDERS_5.ToMoney();


                        row[20] = item.ORDERS_6.ToMoney();
                        row[21] = item.ORDERS_12.ToMoney();
                        row[22] = item.UserAuth.ToMoney();
                        row[23] = item.B_Amony.ToMoney();
                       
                        table.Rows.Add(row);
                    }
                    string fileName = "每日系统对账";
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
}
