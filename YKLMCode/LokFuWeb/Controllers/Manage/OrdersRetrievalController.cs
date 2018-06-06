using LokFu.Base;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// 交易检索
    /// </summary>
    public class OrdersRetrievalController : BaseController
    {

        public ActionResult Index(OrdersRetrievalInModel OrdersRetrievalInModel, EFPagingInfo<OrdersRetrievalInModel> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<OrdersRetrievalViewModel> OrdersRetrievalViewModelList1 = new PageOfItems<OrdersRetrievalViewModel>(new List<OrdersRetrievalViewModel>(), 0, 10, 0, new Hashtable());
                ViewBag.OrdersRetrievalViewModelList = OrdersRetrievalViewModelList1;
                ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
                ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
                ViewBag.OrdersRetrievalInModel = OrdersRetrievalInModel;
                return View();
            }
            var IQuery = this.Entity.Orders.Join(this.Entity.Users, o => o.UId, u => u.Id, (Orders, Users) => new OrdersRetrievalViewModel() { Orders = Orders, Users = Users });
            #region 条件
            TimeSpan TS = OrdersRetrievalInModel.ETime.Subtract(OrdersRetrievalInModel.STime);
            int Days = TS.Days;
            if (Days > 31)
            {
                ViewBag.ErrorMsg = "统计时间间隔不能超过31天！";
                return View("Error");
            }
            if (!OrdersRetrievalInModel.OrderAddress.IsNullOrEmpty())
            {
                var OrderAddress = OrdersRetrievalInModel.OrderAddress.Split(',');
                var predicate = PredicateBuilder.False<OrdersRetrievalViewModel>();
                foreach (var item in OrderAddress)
                {
                    predicate = predicate.Or(o => o.Orders.OrderAddress.Contains(item));
                }
                IQuery = IQuery.Where(predicate);
            }
            if (!OrdersRetrievalInModel.UsersState.IsNullOrEmpty())
            {
                int UsersState = OrdersRetrievalInModel.UsersState == 99 ? 0 : OrdersRetrievalInModel.UsersState;
                IQuery = IQuery.Where(o => o.Users.State == UsersState);
            }
            if (!OrdersRetrievalInModel.STime.IsNullOrEmpty())
            {
                IQuery = IQuery.Where(o => o.Orders.AddTime >= OrdersRetrievalInModel.STime);
            }
            if (!OrdersRetrievalInModel.ETime.IsNullOrEmpty())
            {
                var etime = OrdersRetrievalInModel.ETime.AddSeconds(-1);
                IQuery = IQuery.Where(o => o.Orders.AddTime <= etime);
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            var pages = IQuery.OrderByDescending(o => o.Orders.Id).Skip(p.PageIndex < 1 ? 0 : ((p.PageIndex - 1) * p.PageSize)).Take(p.PageSize);
            var OrdersRetrievalViewModelList = new PageOfItems<OrdersRetrievalViewModel>(pages, p.PageIndex, p.PageSize, IQuery.Count(), p.OrderByList);
            this.ViewBag.OrdersRetrievalViewModelList = OrdersRetrievalViewModelList;
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.OrdersRetrievalInModel = OrdersRetrievalInModel;
            return View();
        }
    }
    public class OrdersRetrievalInModel
    {
        public string OrderAddress { get; set; }
        public int UsersState { get; set; }
        public DateTime STime { get; set; }
        public DateTime ETime { get; set; }
    }
    public class OrdersRetrievalViewModel
    {
        public Orders Orders { get; set; }
        public Users Users { get; set; }
    }
}
