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
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class FinUsersController : BaseController
    {

        public ActionResult Index(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, bool? IsCloseNextAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null) IsShowSupAgent = true;
            if (IsCloseNextAgent == null) IsCloseNextAgent = false;
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                  Orders.ETime = DateTime.Now;
            }

            IList<FinUsersMode> FinUsersModeList = null;
            if (IsFirst == 0)
            {
                FinUsersModeList = new List<FinUsersMode>();
            }
            else
            {
                Dictionary<string, string> dicChar = new Dictionary<string, string>();
                dicChar.Add("STIME", Orders.STime.ToString("yyyy-MM-dd HH:mm:ss"));
                dicChar.Add("ETIME", Orders.ETime.ToString("yyyy-MM-dd HH:mm:ss"));
                dicChar.Add("Agent", Orders.Agent.ToString());
                dicChar.Add("Agent_ALL", IsShowSupAgent == false ? "0" : "1");
                dicChar.Add("IFOFF", IsCloseNextAgent == false ? "0" : "1");
                FinUsersModeList = Entity.GetSPExtensions<FinUsersMode>("SP_Statistics_Code", dicChar);
            }
            ViewBag.FinUsersModeList = FinUsersModeList;
            ViewBag.Orders = Orders;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.IsCloseNextAgent = IsCloseNextAgent;
            ViewBag.Xls = this.checkPower("Xls");
            return View();
        }
        public FileResult XLSDo(Orders Orders, EFPagingInfo<Orders> p, bool? IsShowSupAgent, bool? IsCloseNextAgent)
        {
            if (IsShowSupAgent == null) IsShowSupAgent = true;
            if (IsCloseNextAgent == null) IsCloseNextAgent = false;
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                Orders.ETime = DateTime.Now;
            }
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            dicChar.Add("STIME", Orders.STime.ToString("yyyy-MM-dd HH:mm:ss"));
            dicChar.Add("ETIME", Orders.ETime.ToString("yyyy-MM-dd HH:mm:ss"));
            dicChar.Add("Agent", Orders.Agent.ToString());
            dicChar.Add("Agent_ALL", IsShowSupAgent == false ? "0" : "1");
            dicChar.Add("IFOFF", IsCloseNextAgent == false ? "0" : "1");
            var FinUsersModeList = Entity.GetSPExtensions<FinUsersMode>("SP_Statistics_Code", dicChar);

            string fileName = string.Empty;
            DataTable table = new DataTable();
            fileName = "按商户汇总";

            // 创建 datatable
            table.Columns.Add(new DataColumn("商户名称", typeof(string)));
            table.Columns.Add(new DataColumn("商户姓名", typeof(string)));
            table.Columns.Add(new DataColumn("银联卡支付.数量", typeof(int)));
            table.Columns.Add(new DataColumn("银联卡支付.金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("提现.数量", typeof(int)));
            table.Columns.Add(new DataColumn("提现.金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("转帐.数量", typeof(int)));
            table.Columns.Add(new DataColumn("转帐.金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("房租.数量", typeof(int)));
            table.Columns.Add(new DataColumn("房租.金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("升级.数量", typeof(int)));
            table.Columns.Add(new DataColumn("升级.金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("支付宝.数量", typeof(int)));
            table.Columns.Add(new DataColumn("支付宝.金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("微信.数量", typeof(int)));
            table.Columns.Add(new DataColumn("微信.金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("NFC.数量", typeof(int)));
            table.Columns.Add(new DataColumn("NFC.金额", typeof(decimal)));
            table.Columns.Add(new DataColumn("汇总.数量", typeof(int)));
            table.Columns.Add(new DataColumn("汇总.金额", typeof(decimal)));
            // 填充数据
            DataRow row = null;
            foreach (var item in FinUsersModeList)
            {
                var O = item;
                row = table.NewRow();
                row[0] = O.NEEKNAME;
                row[1] = O.TrueName;
                row[2] = O.C_Recharge;
                row[3] = O.A_Recharge.ToMoney();
                row[4] = O.C_OrderCash;
                row[5] = O.A_OrderCash.ToMoney();
                row[6] = O.C_OrderTransfer;
                row[7] = O.A_OrderTransfer.ToMoney();
                row[8] = O.C_OrderHouse;
                row[9] = O.A_OrderHouse.ToMoney();
                row[10] = O.C_PayConfigOrder;
                row[11] = O.A_PayConfigOrder.ToMoney();
                row[12] = O.C_Alipay;
                row[13] = O.A_Alipay.ToMoney();
                row[14] = O.C_Weixin;
                row[15] = O.A_Weixin.ToMoney();
                row[16] = O.C_NFC;
                row[17] = O.A_NFC.ToMoney();
                row[18] = O.C_Total;
                row[19] = O.A_Total.ToMoney();
                table.Rows.Add(row);
            }
            return this.ExportExcelBase(table, fileName);
        }
    }

    public class FinUsersMode
    {
        public string NEEKNAME { get; set; }
        public string TrueName { get; set; }
        /// <summary>
        /// 银联卡支付.数量
        /// </summary>
        public int C_Recharge { get; set; }
        /// <summary>
        /// 银联卡支付.金额
        /// </summary>
        public decimal A_Recharge { get; set; }
        /// <summary>
        /// 提现.数量
        /// </summary>
        public int C_OrderCash { get; set; }
        /// <summary>
        /// 提现.金额
        /// </summary>
        public decimal A_OrderCash { get; set; }
        /// <summary>
        /// 转帐.数量
        /// </summary>
        public int C_OrderTransfer { get; set; }
        /// <summary>
        /// 转帐.金额
        /// </summary>
        public decimal A_OrderTransfer { get; set; }
        /// <summary>
        /// 房租.数量
        /// </summary>
        public int C_OrderHouse { get; set; }
        /// <summary>
        /// 房租.金额
        /// </summary>
        public decimal A_OrderHouse { get; set; }
        /// <summary>
        /// 升级.数量
        /// </summary>
        public int C_PayConfigOrder { get; set; }
        /// <summary>
        /// 升级.金额
        /// </summary>
        public decimal A_PayConfigOrder { get; set; }
        /// <summary>
        /// 支付宝.数量
        /// </summary>
        public int C_Alipay { get; set; }
        /// <summary>
        /// 支付宝.金额
        /// </summary>
        public decimal A_Alipay { get; set; }
        /// <summary>
        /// 微信.数量
        /// </summary>
        public int C_Weixin { get; set; }
        /// <summary>
        /// 微信.金额
        /// </summary>
        public decimal A_Weixin { get; set; }
        /// <summary>
        /// NFC.数量
        /// </summary>
        public int C_NFC { get; set; }
        /// <summary>
        /// NFC.金额
        /// </summary>
        public decimal A_NFC { get; set; }
        /// <summary>
        /// 汇总.数量
        /// </summary>
        public int C_Total { get; set; }
        /// <summary>
        /// 汇总.金额
        /// </summary>
        public decimal A_Total { get; set; }
        /// <summary>
        /// 佣金汇总
        /// </summary>
        public decimal AgentPayGet { get; set; }
    }
}
