using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Web.Mvc;
namespace LokFu.Areas.Shop.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult MyCode()
        {
            return View();
        }
        public ActionResult MyInfo()
        {
            return View();
        }
        public void SetType(byte intype)
        {
            BasicUsers.InTypePC = intype;
            Entity.SaveChanges();
        }

        public ActionResult MyOrders(Orders Orders, EFPagingInfo<Orders> p)
        {
            p.SqlWhere.Add(f => f.UId == BasicUsers.Id || (f.RUId == BasicUsers.Id && f.PayState == 1));//交易所属用户
            if (!Orders.TNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TNum == Orders.TNum); }
            if (!Orders.STime.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.AddTime >= Orders.STime);
            }
            if (!Orders.ETime.IsNullOrEmpty())
            {
                DateTime ETime = Orders.ETime.AddDays(1);
                p.SqlWhere.Add(f => f.AddTime < ETime);
            }
            #region 交易类型条件判断
            if (!Orders.TType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.TType == Orders.TType);//读取对应的类型
                if (Orders.TType == 1)
                {
                    if (!Orders.TState.IsNullOrEmpty())
                    {
                        switch (Orders.TState)
                        {
                            case 1://未付
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                break;
                            case 2://已付
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                                break;
                            case 3://待传证照
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                                break;
                            case 4://待审核
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                                break;
                            case 5://审核失败
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 4);
                                break;
                        }
                    }
                }
                if (Orders.TType == 2)
                {
                    if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                }
                if (Orders.TType == 3)
                {
                    switch (Orders.TState)
                    {
                        case 1://未付
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                            break;
                        case 2://已付
                            p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                            break;
                        case 3://待传证照
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                            break;
                        case 4://待审核
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                            break;
                        case 5://审核失败
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 4);
                            break;
                    }
                }
                if (Orders.TType == 5)
                {
                    if (!Orders.TState.IsNullOrEmpty())
                    {
                        switch (Orders.TState)
                        {
                            case 99://未付
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                break;
                            case 1://处理中
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 0);
                                break;
                            case 2://已汇出
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 2);
                                break;
                            case 4://出款中
                                p.SqlWhere.Add(f => (f.TState == 2 && f.PayState == 2 && f.IdCardState == 0) || (f.TState == 2 && f.PayState == 1 && f.IdCardState == 3));
                                break;
                            case 3://审核失败
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 4);
                                break;
                            case 5://退款中
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 3);
                                break;
                            case 6://已退款
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 4);
                                break;
                            case 7://待传身份证
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 1);
                                break;
                            case 8://已传身份证
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.IdCardState == 2);
                                break;
                        }
                    }
                }
                if (Orders.TType == 6)
                {
                    if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                }
                if (Orders.TType == 7 || Orders.TType == 8 || Orders.TType == 9)
                {
                    if (!Orders.TState.IsNullOrEmpty())
                    {
                        if (Orders.TState == 99)
                        {
                            p.SqlWhere.Add(f => f.TState == 0);
                        }
                        if (Orders.TState == 1)
                        {
                            p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                        }
                        if (Orders.TState == 2)
                        {
                            p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                        }
                    }
                }
                if (Orders.TType == 10)
                {
                    if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                }
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Orders> OrdersList = Entity.Selects<Orders>(p);
            ViewBag.OrdersList = OrdersList;
            ViewBag.Orders = Orders;
            return View();
        }
        public ActionResult Error(string msg)
        {
            ViewBag.ErrorMsg = msg;
            return View();
        }
    }
}
 