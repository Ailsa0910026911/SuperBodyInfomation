using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class FinFlowController : BaseController
    {
        public ActionResult Index(DateTime? STime,DateTime? ETime)
        {
            if (!STime.HasValue)
            {
                STime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            if (!ETime.HasValue)
            {
                ETime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, 999);
            }
            else
            {
                ETime.Value.AddMilliseconds(999);
            }
            
            var FinFlowModeList = FlowQuery(STime.Value, ETime.Value);
            this.ViewBag.FinFlowModeList = FinFlowModeList;
            this.ViewBag.STime = STime;
            this.ViewBag.ETime = ETime;
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            return View();
        }

        public FileResult ExcelExport(DateTime? STime, DateTime? ETime)
        {
            if (!STime.HasValue)
            {
                STime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            if (!ETime.HasValue)
            {
                ETime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, 999);
            }
            else
            {
                ETime.Value.AddMilliseconds(999);
            }

            var DataList = FlowQuery(STime.Value, ETime.Value);

            DataTable table = new DataTable();
            DataRow row = null;

            // 创建 datatable
            table.Columns.Add(new DataColumn("资金类型", typeof(string)));
            table.Columns.Add(new DataColumn("订单类型", typeof(string)));
            table.Columns.Add(new DataColumn("接口名称", typeof(string)));
            table.Columns.Add(new DataColumn("已付笔数", typeof(int)));
            table.Columns.Add(new DataColumn("未付笔数", typeof(int)));
            table.Columns.Add(new DataColumn("交易金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("商户到账金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("商户手续费", typeof(decimal)));
            table.Columns.Add(new DataColumn("支出手续费(交易)", typeof(decimal)));
            table.Columns.Add(new DataColumn("支出手续费(代付)", typeof(decimal)));
            table.Columns.Add(new DataColumn("成本", typeof(decimal)));
            table.Columns.Add(new DataColumn("分润", typeof(decimal)));
            table.Columns.Add(new DataColumn("利润", typeof(decimal)));
           
            // 填充数据
            foreach (var item in DataList)
            {
                switch (item.OrderType)
                {
                    case "FastOrder":
                        item.OrderType = "交易订单";
                        break;
                }
                row = table.NewRow();
                row[0] = item.FlowType == 1 ? "资金流入" : "资金流出";
                row[1] = item.OrderType;
                row[2] = item.PayWayName;
                row[3] = item.PayCounts;
                row[4] = item.NoPayCounts;
                row[5] = item.Amoney.ToString("F2");
                row[6] = item.PayMoney.ToString("F2");
                row[7] = item.Poundage.ToString("F2");
                row[8] = item.Cost.ToString("F2");
                row[9] = item.SysCash.ToString("F2");
                row[10] = (item.Cost + item.SysCash).ToString("F2");
                row[11] = item.AgentPayGet.ToString("F2");
                row[12] = item.Profit.ToString("F2");
               
                table.Rows.Add(row);
            }
            row = table.NewRow();
            row[0] = "";
            row[1] = "";
            row[2] = "合计";
            row[3] = DataList.Sum(o => o.PayCounts);
            row[4] = DataList.Sum(o => o.NoPayCounts);
            row[5] = DataList.Sum(o=>o.Amoney).ToString("F2");
            row[6] = DataList.Sum(o=>o.PayMoney).ToString("F2");
            row[7] = DataList.Sum(o=>o.Poundage).ToString("F2");
            row[8] = DataList.Sum(o=>o.Cost).ToString("F2");
            row[9] = DataList.Sum(o => o.SysCash).ToString("F2");
            row[10] = (DataList.Sum(o => o.Cost) + DataList.Sum(o => o.SysCash)).ToString("F2");
            row[11] = DataList.Sum(o => o.AgentPayGet).ToString("F2");
            row[12] = DataList.Sum(o => o.Profit).ToString("F2");
            
            table.Rows.Add(row);

            return this.ExportExcelBase(table, "盈亏报表");
        }


        private List<FinFlowMode> FlowQuery(DateTime STime, DateTime ETime)
        {
            var FastPayWayList = this.Entity.FastPayWay.ToList();
            var PayConfigList = this.Entity.PayConfig.ToList();
            var FinFlowModeList = new List<FinFlowMode>();
            var args_FO = new DbParameter[] 
            {
                new SqlParameter {ParameterName = "STime", Value = STime.ToString("yyyy-MM-dd HH:mm:ss")},
                new SqlParameter {ParameterName = "ETime", Value = ETime.ToString("yyyy-MM-dd HH:mm:ss.fff")}
            };
            var args_VO = new DbParameter[] 
            {
                new SqlParameter {ParameterName = "STime", Value = STime.ToString("yyyy-MM-dd HH:mm:ss")},
                new SqlParameter {ParameterName = "ETime", Value = ETime.ToString("yyyy-MM-dd HH:mm:ss.fff")}
            };
            var args_UC = new DbParameter[] 
            {
                new SqlParameter {ParameterName = "STime", Value = STime.ToString("yyyy-MM-dd HH:mm:ss")},
                new SqlParameter {ParameterName = "ETime", Value = ETime.ToString("yyyy-MM-dd HH:mm:ss.fff")}
            };
            string FastOrderSQL =
                        @";with a as (Select 
                        PayWay,COUNT(1) as PayCounts,
                        Sum(Amoney) AS Amoney ,
                        Sum(PayMoney) AS PayMoney,
                        Sum(Poundage) AS Poundage,
                        Sum(HFGet) AS Profit,
                        SUM(ROUND(sysrate*amoney,2,1)) AS Cost,
                        Sum(SysCash) as SysCash,
                        Sum(AgentPayGet) AS AgentPayGet 
                        From FastOrder (nolock)
                        Where PayTime > @STime AND PayTime <= @ETime
                        GROUP BY PayWay ) ,
                        c as (select PayWay,ISNULL(COUNT(PayState),0) NoPayCounts from FastOrder  where AddTime  > @STime AND AddTime <= @ETime and PayState  = 0 GROUP BY PayWay )                           
                        select  b.id as payway,isnull(a.PayCounts,0) as PayCounts,isnull(c.NoPayCounts,0) as NoPayCounts,
                        isnull(a.amoney,0) amoney,
                        isnull(a.paymoney,0) paymoney,
                        isnull(a.Poundage,0) Poundage,
                        isnull(a.Profit,0) Profit,
                        isnull(a.Cost,0) Cost,
                        isnull(a.SysCash,0) SysCash,
                        isnull(a.AgentPayGet,0) AgentPayGet,
                        1 AS FlowType,'FastOrder' AS OrderType 
                        from FastPayWay b 
                        left join a on a.PayWay  = b.Id 
                        left join c on c.PayWay  = b.Id 
                        where  b.State = 1 or PayCounts is not null order by b.State desc,PayCounts desc";

            var FinFlowMode_FO = this.Entity.ExecuteStoreQuery<FinFlowMode>(FastOrderSQL, args_FO).ToList();
            FinFlowMode_FO.ForEach(o => {
                FastPayWay FastPayWay = FastPayWayList.FirstOrNew(x => x.Id == o.PayWay);
                o.PayWayName = FastPayWay.ShowName + "（" + FastPayWay.Title + "）"; 
            });
            FinFlowModeList.AddRange(FinFlowMode_FO);

            return FinFlowModeList;
        }
    }

    public class FinFlowMode
    {
        /// <summary>
        /// 资金类型
        /// </summary>
        public int FlowType { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public string OrderType { get; set; }

        /// <summary>
        /// 接口ID
        /// </summary>
        public int PayWay { get; set; }

        /// <summary>
        /// 接口名称
        /// </summary>
        public string PayWayName { get; set; }

        /// <summary>
        /// 交易金额
        /// </summary>
        public decimal Amoney { get; set; }

        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal PayMoney { get; set; }

        /// <summary>
        /// 手续费
        /// </summary>
        public decimal Poundage { get; set; }

        /// <summary>
        /// 支出手续费(交易)
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 支出手续费(代付)
        /// </summary>
        public decimal SysCash { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public decimal Profit { get; set; }
        /// <summary>
        /// 分润
        /// </summary>
        public decimal AgentPayGet { get; set; }

        public int PayCounts { get; set; }

        public int NoPayCounts { get; set; }
    }
}
