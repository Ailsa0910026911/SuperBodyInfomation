using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class FinAgentController : BaseController
    {
     
        public ActionResult Index(Orders Orders, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                Orders.ETime = DateTime.Now;
            }
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            TimeSpan TS = Orders.ETime.Subtract(Orders.STime);
            int Days = TS.Days;
            if (Days > 31)
            {
                ViewBag.ErrorMsg = "统计时间间隔不能超过31天！";
                return View("Error");
            }

            IList<FinAgentMode> FinAgentModeList = null;
            if (IsFirst == 0)
            {
                FinAgentModeList = new List<FinAgentMode>();
            }
            else
            {
                Dictionary<string, string> dicChar = new Dictionary<string, string>();
                dicChar.Add("STIME", Orders.STime.ToString("yyyy-MM-dd HH:mm:ss"));
                dicChar.Add("ETIME", Orders.ETime.ToString("yyyy-MM-dd HH:mm:ss"));
                dicChar.Add("IFALL", IsShowSupAgent == false ? "0" : "1");
                dicChar.Add("IFOFF", "0");
                dicChar.Add("TTYPE", "2");
                dicChar.Add("AGENTID", this.BasicAgent.Id.ToString());
                FinAgentModeList = Entity.GetSPExtensions<FinAgentMode>("SP_Statistics_Agent", dicChar);
            }
            ViewBag.FinAgentModeList = FinAgentModeList;
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.BasicAgent = BasicAgent;
            ViewBag.Orders = Orders;
            return View();
        }
    }

    public class FinAgentMode
    {
        public string NAME { get; set; }
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
        /// <summary>
        /// 佣金汇总
        /// </summary>
        public decimal AgentPayGet { get; set; }
    }
}
