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
    public class ApplyCreditController : BaseController
    {
        public ActionResult Index(ApplyCredit ApplyCredit, EFPagingInfo<ApplyCredit> p, int IsFirst = 0)
        {
            if (ApplyCredit.STime.IsNullOrEmpty())
            {
                ApplyCredit.STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ApplyCredit.ETime.IsNullOrEmpty())
            {
               // ApplyCredit.ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                 ApplyCredit.ETime = DateTime.Now;
            }
            if (IsFirst==0)
            {
                PageOfItems<ApplyCredit> ApplyCreditList1 = new PageOfItems<ApplyCredit>(new List<ApplyCredit>(), 0, 10, 0, new Hashtable());
                ViewBag.ApplyCreditList = ApplyCreditList1;
                ViewBag.ApplyCredit = ApplyCredit;
                ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
                return View();
            }
           
            if (!ApplyCredit.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName == ApplyCredit.TrueName); }
            if (!ApplyCredit.BankId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BankId == ApplyCredit.BankId); }
            if (!ApplyCredit.Education.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Education.Contains(ApplyCredit.Education)); }
            if (!ApplyCredit.SheBao.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.SheBao == ApplyCredit.SheBao); }
            if (!ApplyCredit.Marry.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Marry == ApplyCredit.Marry); }
            if (!ApplyCredit.HasCar.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCar == (ApplyCredit.HasCar == 99 ? 0 : ApplyCredit.HasCar)); }
            if (!ApplyCredit.House.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.House == ApplyCredit.House); }
            if (!ApplyCredit.HasCredit.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCredit == ApplyCredit.HasCredit); }
            if (!ApplyCredit.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyCredit.State); }
            if (!ApplyCredit.AgentPay.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentPay == (ApplyCredit.AgentPay == 99 ? 0 : ApplyCredit.AgentPay)); }
            if (!ApplyCredit.AgentAId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentAId == ApplyCredit.AgentAId); }
            if (checkPower("ALL"))
            {
                p.SqlWhere.Add(f => f.AgentId == BasicAgent.Id);//读取全部分支机构
            }
            else
            {
                p.SqlWhere.Add(f => f.AgentAId == AdminUser.Id);//读取用户
            }
            if (!ApplyCredit.STime.IsNullOrEmpty() && !ApplyCredit.ETime.IsNullOrEmpty())
            {
                DateTime ETime = ApplyCredit.ETime;
                p.SqlWhere.Add(f => f.AddTime > ApplyCredit.STime && f.AddTime < ETime);
            }
            p.PageSize = 99999999;
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCredit> ApplyCreditList = Entity.Selects<ApplyCredit>(p);
            ViewBag.ApplyCreditList = ApplyCreditList;
            ViewBag.ApplyCredit = ApplyCredit;
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            return View();
        }
    }
}
