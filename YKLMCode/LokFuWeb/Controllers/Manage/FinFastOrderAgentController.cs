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
namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// 直通车代理汇总
    /// </summary>
    public class FinFastOrderAgentController : BaseController
    {

        [HttpGet]
        public ActionResult Index(DateTime? SDate, DateTime? EDate)
        {
            if (!SDate.HasValue)
            {
                SDate = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,0,0,0);
            }
            if (!EDate.HasValue)
            {
                EDate = DateTime.Now;
            }
            
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("SDATE", SDate.Value.ToString("yyyy-MM-dd"));
            dicChar.Add("EDATE", EDate.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            IList<FastOrderAgentModel> FastOrderAgentModelList = Entity.GetSPExtensions<FastOrderAgentModel>("SP_Statistics_AgentPath", dicChar);
            this.ViewBag.SDate = SDate.Value;
            this.ViewBag.EDate = EDate.Value;
            this.ViewBag.FastOrderAgentModelList = FastOrderAgentModelList;
            var ids = FastOrderAgentModelList.Select(o => int.Parse(o.F_AgentPath)).ToList();
            var SysAgentList = Entity.SysAgent.Where(o => ids.Contains(o.Id)).ToList();
            this.ViewBag.SysAgentList = SysAgentList;
            ViewBag.ExcelExport = this.checkPower("ExcelExport");
            return View();
        }

        public FileResult ExcelExport(DateTime SDate, DateTime EDate)
        {
            DateTime EndDate = new DateTime(EDate.Year, EDate.Month, EDate.Day, EDate.Hour, EDate.Minute, EDate.Millisecond, 999);
            string fileName = string.Empty;
            DataTable table = new DataTable();

            fileName = "交易汇总" + SDate.ToString("yyyyMMdd") + "-" + EndDate.ToString("yyyyMMdd");
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("SDATE", SDate.ToString("yyyy-MM-dd"));
            dicChar.Add("EDATE", EndDate.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            IList<FastOrderAgentModel> DataList = Entity.GetSPExtensions<FastOrderAgentModel>("SP_Statistics_AgentPath", dicChar);
            var ids = DataList.Select(o => int.Parse(o.F_AgentPath)).ToList();
            var SysAgentList = Entity.SysAgent.Where(o => ids.Contains( o.Id)).ToList();

            // 创建 datatable
            table.Columns.Add(new DataColumn("代理商", typeof(string)));
            table.Columns.Add(new DataColumn("联系人", typeof(string)));
            table.Columns.Add(new DataColumn("联系电话", typeof(string)));
            table.Columns.Add(new DataColumn("总金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("总结算金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("总手续费", typeof(decimal)));
            table.Columns.Add(new DataColumn("总分润", typeof(decimal)));
            table.Columns.Add(new DataColumn("总利润", typeof(decimal)));

            // 填充数据
            DataRow row = null;
            foreach (var item in DataList)
            {
                var SysAgent = SysAgentList.FirstOrNew(o => o.Id == int.Parse(item.F_AgentPath));
                row = table.NewRow();
                row[0] = SysAgent.Name;
                row[1] = SysAgent.Linker;
                row[2] = SysAgent.LinkMobile;
                row[3] = item.Amoney.ToString("f2");
                row[4] = item.PayMoney.ToString("f2");
                row[5] = item.Poundage.ToString("f2");
                row[6] = item.AgentPayGet.ToString("f2");
                row[7] = item.HFGet.ToString("f2");
                table.Rows.Add(row);
            }

            return ExportExcelBase(table, fileName);
        }
    }

    public class FastOrderAgentModel
    {
        public string F_AgentPath { get; set; }
        public decimal Amoney { get; set; }
        public decimal Poundage { get; set; }
        public decimal HFGet { get; set; }
        public decimal AgentPayGet { get; set; }
        public decimal PayMoney { get; set; }
    }
}