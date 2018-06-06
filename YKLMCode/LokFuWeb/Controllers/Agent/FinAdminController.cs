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
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class FinAdminController : BaseController
    {

        public ActionResult Index(Orders Orders, int IsFirst = 0)
        {
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                Orders.ETime = DateTime.Now;
            }

            IList<FinAdminMode> FinAdminModeList = null;
            if (IsFirst == 0)
            {
                FinAdminModeList = new List<FinAdminMode>();
            }
            else
            {
                Dictionary<string, string> dicChar = new Dictionary<string, string>();
                dicChar.Add("STIME", Orders.STime.ToString("yyyy-MM-dd HH:mm:ss"));
                dicChar.Add("ETIME", Orders.ETime.ToString("yyyy-MM-dd HH:mm:ss"));
                dicChar.Add("AGENTID", this.BasicAgent.Id.ToString());
                FinAdminModeList = Entity.GetSPExtensions<FinAdminMode>("SP_Statistics_Salesman", dicChar);
            }
            this.ViewBag.FinAdminModeList = FinAdminModeList;
            this.ViewBag.Orders = Orders;
            return View();
        }
    }

    public class FinAdminMode
    {
        public string Truename { get; set; }
        /// <summary>
        /// 银联卡支付.数量
        /// </summary>
        public int C_Recharge { get; set; }
        /// <summary>
        /// 银联卡支付.金额
        /// </summary>
        public decimal A_Recharge { get; set; }
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
    }
}
