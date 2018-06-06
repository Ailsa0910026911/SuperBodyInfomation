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
namespace LokFu.Areas.Agent.Controllers
{
    public class ApplyLoanController : BaseController
    {
        public ActionResult Index(ApplyLoan ApplyLoan, EFPagingInfo<ApplyLoan> p, int IsFirst = 0)
        {
            if (ApplyLoan.STime.IsNullOrEmpty())
            {
                ApplyLoan.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ApplyLoan.ETime.IsNullOrEmpty())
            {
               // ApplyLoan.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                 ApplyLoan.ETime =DateTime.Now;
            }
            if (IsFirst == 0)
            {
                PageOfItems<ApplyLoan> ApplyLoanList1 = new PageOfItems<ApplyLoan>(new List<ApplyLoan>(), 0, 10, 0, new Hashtable());
                ViewBag.ApplyLoanList = ApplyLoanList1;
                ViewBag.ApplyLoan = ApplyLoan;
                return View();
            }
           
            if (!ApplyLoan.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName == ApplyLoan.TrueName); }
            if (!ApplyLoan.Education.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Education.Contains(ApplyLoan.Education)); }
            if (!ApplyLoan.SheBao.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.SheBao == ApplyLoan.SheBao); }
            if (!ApplyLoan.Marry.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Marry == ApplyLoan.Marry); }
            if (!ApplyLoan.HasCar.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCar == (ApplyLoan.HasCar == 99 ? 0 : ApplyLoan.HasCar)); }
            if (!ApplyLoan.House.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.House == ApplyLoan.House); }
            if (!ApplyLoan.HasCredit.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCredit == ApplyLoan.HasCredit); }
            if (!ApplyLoan.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyLoan.State); }
            if (!ApplyLoan.AgentPay.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentPay == (ApplyLoan.AgentPay == 99 ? 0 : ApplyLoan.AgentPay)); }
            if (!ApplyLoan.AgentAId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentAId == ApplyLoan.AgentAId); }
            if (checkPower("ALL"))
            {
                p.SqlWhere.Add(f => f.AgentId == BasicAgent.Id);//读取全部分支机构
            }
            else
            {
                p.SqlWhere.Add(f => f.AgentAId == AdminUser.Id);//读取用户
            }
            if (!ApplyLoan.STime.IsNullOrEmpty() && !ApplyLoan.ETime.IsNullOrEmpty())
            {
                DateTime ETime = ApplyLoan.ETime;
                p.SqlWhere.Add(f => f.PayTime > ApplyLoan.STime && f.PayTime < ETime);
            }
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyLoan> ApplyLoanList = Entity.Selects<ApplyLoan>(p);
            ViewBag.ApplyLoanList = ApplyLoanList;
            ViewBag.ApplyLoan = ApplyLoan;
            return View();
        }
    }
}
