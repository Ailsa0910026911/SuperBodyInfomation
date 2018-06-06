using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections;

namespace LokFu.Areas.Agent.Controllers
{
    public class ApplyCreditCardController : BaseController
    {
        //
        // GET: /ApplyCreditCard/

        public ActionResult Index(ApplyCreditCard ApplyCreditCard, EFPagingInfo<ApplyCreditCard> p, DateTime? STime, DateTime? ETime, int IsFirst = 0, int IsShowSupAgent = -1, string BankName = "")
        {
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Entity = Entity;
            if (STime.IsNullOrEmpty())
            {
                STime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            }
            if (ETime.IsNullOrEmpty())
            {
               // ETime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                 ETime = DateTime.Now;
            }
            ViewBag.ETime = ETime;
            ViewBag.STime = STime;
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.CreditCardUrl != null).ToList();
            ViewBag.ApplyCreditCard = ApplyCreditCard;
            ViewBag.BankName = BankName;
            ViewBag.BasicAgent = BasicAgent;
            if (IsFirst == 0)
            {
                PageOfItems<ApplyCreditCard> ApplyCreditCardList1 = new PageOfItems<ApplyCreditCard>(new List<ApplyCreditCard>(), 0, 10, 0, new Hashtable());
                ViewBag.ApplyCreditCardList = ApplyCreditCardList1;
                return View();
            }
           // ETime = ETime.Value.AddDays(1);
            p.SqlWhere.Add(f=>f.AddTime>=STime&&f.AddTime<=ETime);
            //if (BasicAgent.Tier == 1)
            //{
            //    p.SqlWhere.Add(f => f.FirstAgentId == BasicAgent.Id);
            //}
            //else
            //{
            //    p.SqlWhere.Add(f => f.AgentId == BasicAgent.Id);
            //}
            if (IsShowSupAgent == 1)
            {
                List<Int32> AgentId = new List<Int32>();
                IList<SysAgent> SysAgentList = BasicAgent.GetSupAgent(Entity);
                foreach (var pp in SysAgentList)
                {
                    AgentId.Add(pp.Id);
                }
                p.SqlWhere.Add(f => AgentId.Contains(f.AgentId));
            }
            else
            {
                p.SqlWhere.Add(f => f.AgentId == BasicAgent.Id);
            }
            if (!BankName.IsNullOrEmpty())
            {
               IList< BasicBank> BasicBankList=Entity.BasicBank.Where(b => b.Name.Contains( BankName)).ToList();
               IList<int> BasicBankId = new List<int>();
               foreach (var item in BasicBankList)
               {
                   BasicBankId.Add(item.Id);
               }
                p.SqlWhere.Add(f =>BasicBankId.Contains( f.BankId));
            }
            if (!ApplyCreditCard.OrderNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OrderNum == ApplyCreditCard.OrderNum); }
            if (!ApplyCreditCard.UserMobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserMobile == ApplyCreditCard.UserMobile); }
            if (!ApplyCreditCard.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName == ApplyCreditCard.UserName); }
            if (!ApplyCreditCard.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyCreditCard.State); }
            if (!ApplyCreditCard.FirstAgentAmount.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.FirstAgentAmount == ApplyCreditCard.FirstAgentAmount); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCreditCard> ApplyCreditCardList = Entity.Selects<ApplyCreditCard>(p);
            ViewBag.ApplyCreditCardList = ApplyCreditCardList;

            return View();
        }

    }
}
