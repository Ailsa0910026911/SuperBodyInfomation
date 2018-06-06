using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class FinTotalController : BaseController
    {
        public FinTotalController()
        {
            //ViewBag.Authorization = true;//允许权限
        }

        public ActionResult Index(FinTotal FinTotal, EFPagingInfo<FinTotal> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<FinTotal> FinTotalList1 = new PageOfItems<FinTotal>(new List<FinTotal>(), 0, 10, 0, new Hashtable());
                ViewBag.FinTotalList = FinTotalList1;
                ViewBag.FinTotal = FinTotal;
                ViewBag.IsCountByYear = false;
                return View();
            }
            p.OrderByList.Add("AddTime", "DESC");
            bool IsCountByYear = false;
            if (!FinTotal.Id.IsNullOrEmpty())
            {
                p.SqlWhere.Add(n => n.AddTime.Year == FinTotal.Id);
                IsCountByYear = true;
            }
            p.PageSize = 12;//一年为一页
            IPageOfItems<FinTotal> FinTotalList = Entity.Selects<FinTotal>(p);
            if (p.PageIndex < 2)
            {//第一页处理
                FinTotal FT = FinTotalList.FirstOrDefault();
                if (FT != null)
                {//有数据才处理
                    DateTime DT = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
                    if (FT.AddTime < DT)
                    {//本月未在数据库中 
                        FinTotal ft = new FinTotal();
                        ft.AddTime = DT;
                        Entity.FinTotal.AddObject(ft);
                        Entity.SaveChanges();
                        FT = Update(FT);
                        //重新获取数据
                        FinTotalList = Entity.Selects<FinTotal>(p);
                    }
                }
            }
            ViewBag.FinTotalList = FinTotalList;
            ViewBag.FinTotal = FinTotal;
            ViewBag.IsCountByYear = IsCountByYear;
            return View();
        }
        public void Info(FinTotal FinTotal)
        {
            if (FinTotal.Id != 0)
            {
                FinTotal = Entity.FinTotal.FirstOrDefault(n => n.Id == FinTotal.Id);
                FinTotal = Update(FinTotal);
                Entity.SaveChanges();
            }
            string StrJson = FinTotal.OutJson();
            Response.Write(StrJson);
        }
        private FinTotal Update(FinTotal FinTotal)
        {
            if (FinTotal.Id.IsNullOrEmpty())
            {
                return new FinTotal();
            }
            FinTotal = Entity.FinTotal.FirstOrDefault(n => n.Id == FinTotal.Id);
            if (FinTotal == null)
            {
                return new FinTotal();
            }
            DateTime ST = FinTotal.AddTime;
            DateTime ET = ST.AddMonths(1);
            //银联总交易额
            IQueryable<Orders> a = Entity.Orders.Where(n => n.PayWay == 2 && n.PayState == 1 && n.PayTime >= ST && n.PayTime < ET);
            decimal A = a.Count() > 0 ? a.Sum(n => n.Amoney) : 0;
            //总手续费
            decimal B = 0;
            IQueryable<OrderRecharge> c = Entity.OrderRecharge.Where(n => n.PayState == 1 && n.PayTime >= ST && n.PayTime < ET);
            //银联卡支付总额
            decimal C = c.Count() > 0 ? c.Sum(n => n.Amoney) : 0;
            //银联卡支付总手续费B
            decimal D = c.Count() > 0 ? c.Sum(n => n.Poundage) : 0;
            IQueryable<OrderCash> e = Entity.OrderCash.Where(n => n.PayState == 2 && n.AddTime >= ST && n.AddTime < ET);
            //提现总额
            decimal E = e.Count() > 0 ? e.Sum(n => n.Amoney) : 0;
            IQueryable<OrderCash> f = Entity.OrderCash.Where(n => n.PayState == 2 && n.AddTime >= ST && n.AddTime < ET && n.TrunType == 0);
            //T0提现总额
            decimal F = f.Count() > 0 ? f.Sum(n => n.Amoney) : 0;
            //T0提现手续费
            decimal G = (decimal)(f.Count() > 0 ? f.Sum(n => (double)n.UserRate) : 0);
            //提现笔数
            int H = e.Count();
            //提现服务费
            double I = e.Count() > 0 ? e.Sum(n => n.UserRate) : 0;
            IQueryable<OrderTransfer> j = Entity.OrderTransfer.Where(n => n.PayState == 1 && n.PayTime >= ST && n.PayTime < ET);
            //转帐总额
            decimal J = j.Count() > 0 ? j.Sum(n => n.Amoney) : 0;
            //转帐手续费B
            decimal K = j.Count() > 0 ? j.Sum(n => n.Poundage) : 0;
            IQueryable<OrderHouse> l = Entity.OrderHouse.Where(n => n.PayState == 2 && n.PayTime >= ST && n.PayTime < ET);
            //房租总额
            decimal L = l.Count() > 0 ? l.Sum(n => n.PayMoney) : 0;
            //房租手续费
            decimal M = l.Count() > 0 ? l.Sum(n => n.Poundage) : 0;
            IQueryable<OrderHouse> m = Entity.OrderHouse.Where(n => n.PayState == 2 && n.PayTime >= ST && n.PayTime < ET);
            //房租T0总额
            decimal L0 = m.Count() > 0 ? m.Sum(n => n.PayMoney) : 0;
            //房租T0手续费
            decimal M0 = m.Count() > 0 ? m.Sum(n => n.CashRate * n.PayMoney) : 0;
            //房租手续费银联
            double M1 = l.Count() > 0 ? l.Sum(n => n.UserRate * (double)n.PayMoney) : 0;
            IQueryable<PayConfigOrder> z = Entity.PayConfigOrder.Where(n => n.PayState == 1 && n.PayTime >= ST && n.PayTime < ET);
            //升级总额
            decimal N = z.Count() > 0 ? z.Sum(n => n.Amoney) : 0;
            IQueryable<OrderF2F> o = Entity.OrderF2F.Where(n => n.PayState == 1 && n.PayTime >= ST && n.PayTime < ET && n.PayWay == 5);
            //支付宝总额
            decimal O = o.Count() > 0 ? o.Sum(n => n.Amoney) : 0;
            //支付宝手续费
            decimal P = o.Count() > 0 ? o.Sum(n => n.Poundage) : 0;
            IQueryable<OrderF2F> q = Entity.OrderF2F.Where(n => n.PayState == 1 && n.PayTime >= ST && n.PayTime < ET && n.PayWay == 6);
            //微信总额
            decimal Q = q.Count() > 0 ? q.Sum(n => n.Amoney) : 0;
            //微信手续费
            decimal R = q.Count() > 0 ? q.Sum(n => n.Poundage) : 0;
            IQueryable<OrderF2F> p = Entity.OrderF2F.Where(n => n.PayState == 1 && n.PayTime >= ST && n.PayTime < ET && n.PayWay == 7);
            //NFC总额
            decimal S = p.Count() > 0 ? p.Sum(n => n.Amoney) : 0;
            //NFC手续费
            decimal T = p.Count() > 0 ? p.Sum(n => n.Poundage) : 0;
            B = D + K + (decimal)M1;
            FinTotal.Update = DateTime.Now;
            FinTotal.TotalAmoney = A;
            FinTotal.TotlaPoundage = B;
            FinTotal.Amoney1 = C;
            FinTotal.Poundage1 = D;
            FinTotal.Amoney2 = E;
            FinTotal.Poundage2 = 0;
            FinTotal.Amoney2_0 = F;
            FinTotal.Poundage2_0 = G;
            FinTotal.Number2 = H;
            FinTotal.Poundage2_1 = (decimal)I;
            FinTotal.Amoney3 = J;
            FinTotal.Poundage3 = K;
            FinTotal.Amoney4 = 0;
            FinTotal.Poundage4 = 0;
            FinTotal.Amoney5 = L;
            FinTotal.Poundage5 = M;
            FinTotal.Amoney5_0 = L0;
            FinTotal.Poundage5_0 = M0;
            FinTotal.Amoney6 = N;
            FinTotal.Poundage6 = 0;
            FinTotal.Amoney7 = O;
            FinTotal.Poundage7 = P;
            FinTotal.Amoney8 = Q;
            FinTotal.Poundage8 = R;
            FinTotal.Amoney9 = S;
            FinTotal.Poundage9 = T;
            Entity.SaveChanges();
            return FinTotal;
        }
    }
}
