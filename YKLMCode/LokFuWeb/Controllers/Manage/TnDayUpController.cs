using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.Extensions;
using LokFu.Repositories.SqlServer;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// T0统计报表
    /// </summary>
    public class TnDayUpController : BaseController
    {
        public ActionResult Index(Orders Orders, EFPagingInfo<Orders> p, int IsFirst = 0)
        {
            ViewBag.Xls = this.checkPower("Xls");
            if (IsFirst == 0)
            {
                Orders.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                Orders.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (Orders.STime == null || Orders.STime == DateTime.MinValue || Orders.ETime == null || Orders.ETime == DateTime.MinValue)
            {
                ViewBag.ErrorMsg = "查询时间有误！";
                return View("Error");
            }
            IList<TnOrders> OrdersList = null;
            Dictionary<string, string> dicChar = new Dictionary<string, string>();
            if (!Orders.STime.IsNullOrEmpty())
            {
                dicChar.Add("S_Time", Orders.STime.ToString("yyyy-MM-dd"));
            }
            if (!Orders.ETime.IsNullOrEmpty())
            {
                Orders.ETime = Orders.ETime.AddDays(1);
                dicChar.Add("E_Time", Orders.ETime.ToString("yyyy-MM-dd"));
            }
            if (IsFirst > 0)
            {
                OrdersList = Entity.GetSPExtensions<TnOrders>("SP_Statistics_Orders", dicChar);
            }
            Orders.ETime = Orders.ETime.AddDays(-1);
            ViewBag.Orders = Orders;
            ViewBag.OrdersList = OrdersList != null ? OrdersList.OrderByDescending(x => x.PayTime).ToList() : null;
            return View();
        }
        public void XLSDo(Orders Orders, IList<TnOrders> ordersList)
        {

        }
    }
}
