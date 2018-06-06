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
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    /// <summary>
    /// 直通车代理汇总
    /// </summary>
    public class FinFastOrderAgentController : BaseController
    {
        [HttpGet]
        public ActionResult Index(DateTime? SDate, DateTime? EDate)
        {
            if (this.BasicAgent.AgentLevelMax == 0 || this.BasicAgent.AgentLevelMax == this.BasicAgent.Tier)
            {
                return null;
            }
            if (!SDate.HasValue)
            {
                SDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }
            if (!EDate.HasValue)
            {
                EDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999);
            }
            else
            {
                EDate = new DateTime(EDate.Value.Year, EDate.Value.Month, EDate.Value.Day, EDate.Value.Hour, EDate.Value.Minute, EDate.Value.Second, 999);
            }

            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("STIME", SDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            dicChar.Add("ETIME", EDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            dicChar.Add("AGENTID", this.BasicAgent.Id.ToString());
            var DataList = Entity.GetSPExtensions<FastOrderAgentModel>("SP_Statistics_Fastorder", dicChar);
            this.ViewBag.DataList = DataList;
            this.ViewBag.SDate = SDate.Value;
            this.ViewBag.EDate = EDate.Value;
            return View();
        }

        [HttpGet]
        public FileResult ExcelExport(DateTime? SDate, DateTime? EDate)
        {
            if (this.BasicAgent.AgentLevelMax == 0 || this.BasicAgent.AgentLevelMax == this.BasicAgent.Tier)
            {
                return null;
            }
            if (!SDate.HasValue)
            {
                SDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }
            if (!EDate.HasValue)
            {
                EDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59, 999);
            }
            else
            {
                EDate = new DateTime(EDate.Value.Year, EDate.Value.Month, EDate.Value.Day, EDate.Value.Hour, EDate.Value.Minute, EDate.Value.Second, 999);
            }

            DateTime StartDate = SDate.Value;
            DateTime EndDate = EDate.Value;
            string fileName = string.Empty;
            DataTable table = new DataTable();

            fileName = "交易汇总" + StartDate.ToString("yyyyMMdd") + "-" + EndDate.ToString("yyyyMMdd");

            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("STIME", SDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            dicChar.Add("ETIME", EDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            dicChar.Add("AGENTID", this.BasicAgent.Id.ToString());
            var DataList = Entity.GetSPExtensions<FastOrderAgentModel>("SP_Statistics_Fastorder", dicChar);

            // 创建 datatable
            table.Columns.Add(new DataColumn("代理商", typeof(string)));
            table.Columns.Add(new DataColumn("联系人", typeof(string)));
            table.Columns.Add(new DataColumn("联系电话", typeof(string)));
            table.Columns.Add(new DataColumn("总金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("总分润", typeof(decimal)));

            // 填充数据
            DataRow row = null;
            foreach (var item in DataList)
            {
                row = table.NewRow();
                row[0] = item.Name;
                row[1] = item.Linker;
                row[2] = item.LinkMobile;
                row[3] = item.Amoney.ToString("f2");
                row[4] = item.Profit.ToString("f2");
                table.Rows.Add(row);
            }

            return ExportExcelBase(table, fileName);
        }
    }

    public class FastOrderAgentModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LinkMobile { get; set; }

        public string Linker { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Amoney { get; set; }
        /// <summary>
        /// 总分润
        /// </summary>
        public decimal Profit { get; set; }
    }
}