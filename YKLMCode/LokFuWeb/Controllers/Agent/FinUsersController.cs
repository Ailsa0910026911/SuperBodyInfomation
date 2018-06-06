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
    public class FinUsersController : BaseController
    {

        public ActionResult Index(Orders Orders, bool? IsShowSupAgent, bool? IsCloseNextAgent, int? LowerLevel, int IsFirst = 0)
        {
            if (Orders.STime.IsNullOrEmpty())
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.ETime.IsNullOrEmpty())
            {
                Orders.ETime = DateTime.Now;
            }

            if (IsShowSupAgent == null) IsShowSupAgent = true;
            if (IsCloseNextAgent == null) IsCloseNextAgent = false;
            LowerLevel = LowerLevel == null ? 0 : LowerLevel;
            if (checkPower("ALL"))
            {
                ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.AgentId == BasicAgent.Id).ToList();
            }
            else
            {
                ViewBag.SysAdminList = new List<SysAdmin>();
            }
            IList<FinUsersMode> FinUsersModeList;
            if (IsFirst == 0)
            {
                FinUsersModeList = new List<FinUsersMode>();
            }
            else
            {
                Dictionary<string, string> dicChar = new Dictionary<string, string>();
                dicChar.Add("STIME", Orders.STime.ToString("yyyy-MM-dd HH:mm:ss"));
                dicChar.Add("ETIME", Orders.ETime.ToString("yyyy-MM-dd HH:mm:ss"));
                dicChar.Add("Agent", this.BasicAgent.Id.ToString());
                dicChar.Add("Agent_ALL", IsShowSupAgent == false ? "0" : "1");
                dicChar.Add("IFOFF", IsCloseNextAgent == false ? "0" : "1");
                dicChar.Add("AId", Orders.AId == 0 ? "" : Orders.AId.ToString());
                FinUsersModeList = Entity.GetSPExtensions<FinUsersMode>("SP_Statistics_Code", dicChar);
            }
            this.ViewBag.FinUsersModeList = FinUsersModeList;
            ViewBag.Orders = Orders;
            //if (IsFirst == 0)
            //{
            //    if (IsShowSupAgent == null)
            //    {
            //        IsShowSupAgent = false;
            //    }
            //    LowerLevel = LowerLevel == null ? 0 : LowerLevel;
            //    if (checkPower("ALL"))
            //    {
            //        ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.AgentId == BasicAgent.Id).ToList();
            //    }
            //    else
            //    {
            //        ViewBag.SysAdminList = new List<SysAdmin>();
            //    }
            //    PageOfItems<Orders> OrdersList1 = new PageOfItems<Orders>(new List<Orders>(), 0, 10, 0, new Hashtable());
            //    ViewBag.OrdersList = OrdersList1;
            //    ViewBag.Orders = Orders;
            //    ViewBag.UsersList = new List<Users>();
            //    ViewBag.IsShowSupAgent = IsShowSupAgent;
            //    ViewBag.BasicAgent = BasicAgent;
            //    ViewBag.LowerLevel = LowerLevel;
            //    return View();
            //}
            
            //LowerLevel = LowerLevel == null ? 0 : LowerLevel;
           
            //if (!Orders.STime.IsNullOrEmpty() && !Orders.ETime.IsNullOrEmpty())
            //{
            //    DateTime ETime = Orders.ETime;
            //    p.SqlWhere.Add(f => f.PayTime > Orders.STime && f.PayTime < ETime);
            //}
            //IList<SysAgent> SysAgentList = null;
            //if (checkPower("ALL"))
            //{
            //    // p.SqlWhere.Add(f => f.Agent == BasicAgent.Id);//读取全部分支机构
            //    if ((bool)IsShowSupAgent)
            //    {
            //        IList<int> UID = new List<int>();
            //        if (LowerLevel != 0)
            //        {
            //            SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == LowerLevel).FirstOrNew();
            //            SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
            //        }
            //        else
            //        {
            //            SysAgentList = BasicAgent.GetSupAgent(Entity);//获取所有下级代理商信息
            //        }
            //        foreach (var s in SysAgentList)
            //        {
            //            UID.Add(s.Id);
            //        }
            //        p.SqlWhere.Add(f => UID.Contains(f.Agent));
            //    }
            //    else
            //    {
            //        p.SqlWhere.Add(f => f.Agent == BasicAgent.Id);//指定的代理
            //    }
            //    ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.AgentId == BasicAgent.Id).ToList();
            //    if (!Orders.AId.IsNullOrEmpty())
            //    {
            //        p.SqlWhere.Add(f => f.AId == Orders.AId);
            //    }
            //}
            //else
            //{
            //    p.SqlWhere.Add(f => f.AId == AdminUser.Id);//读取用户
            //    ViewBag.SysAdminList = new List<SysAdmin>();
            //}
            //p.SqlWhere.Add(f => f.TState == 2);
            //p.PageSize = 99999999;
            //p.OrderByList.Add("Id", "DESC");
            //IPageOfItems<Orders> OrdersList = Entity.Selects<Orders>(p);
            //ViewBag.OrdersList = OrdersList;
            //ViewBag.Orders = Orders;
            //IList<Orders> List = OrdersList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            //List<int> UId = new List<int>();
            //foreach (var pp in List)
            //{
            //    UId.Add(pp.UId);
            //}
            //ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.LowerLevel = LowerLevel;
            return View();
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
