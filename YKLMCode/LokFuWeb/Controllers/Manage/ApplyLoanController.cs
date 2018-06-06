using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class ApplyLoanController : BaseController
    {
  
        public ActionResult Index(ApplyLoan ApplyLoan, EFPagingInfo<ApplyLoan> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null) IsShowSupAgent = false;
            if (!ApplyLoan.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName == ApplyLoan.TrueName); }
            if (!ApplyLoan.Education.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Education.Contains(ApplyLoan.Education)); }
            if (!ApplyLoan.SheBao.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.SheBao == ApplyLoan.SheBao); }
            if (!ApplyLoan.Marry.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Marry == ApplyLoan.Marry); }
            if (!ApplyLoan.HasCar.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCar == (ApplyLoan.HasCar == 99 ? 0 : ApplyLoan.HasCar)); }
            if (!ApplyLoan.House.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.House == ApplyLoan.House); }
            if (!ApplyLoan.HasCredit.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.HasCredit == ApplyLoan.HasCredit); }
            if (!ApplyLoan.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == ApplyLoan.State); }
            if (!ApplyLoan.AgentPay.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentPay == (ApplyLoan.AgentPay == 99 ? 0 : ApplyLoan.AgentPay)); }
            if (!ApplyLoan.AgentAId.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == ApplyLoan.AgentAId).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = new List<int>();
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.AgentAId));
                }
                else
                {
                    p.SqlWhere.Add(f => f.AgentAId == ApplyLoan.AgentAId);
                }
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyLoan> ApplyLoanList = null;
            if (IsFirst == 0)
            {
                ApplyLoanList = new PageOfItems<ApplyLoan>(new List<ApplyLoan>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                ApplyLoanList = Entity.Selects<ApplyLoan>(p);
            }
            ViewBag.ApplyLoanList = ApplyLoanList;
            ViewBag.ApplyLoan = ApplyLoan;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(ApplyLoan ApplyLoan)
        {
            if (ApplyLoan.Id != 0) ApplyLoan = Entity.ApplyLoan.FirstOrDefault(n => n.Id == ApplyLoan.Id);
            if (ApplyLoan == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.ApplyLoan = ApplyLoan;
            ViewBag.BasicProvince = Entity.BasicProvince.FirstOrNew(n => n.Id == ApplyLoan.ComProvince);
            ViewBag.BasicCity = Entity.BasicCity.FirstOrNew(n => n.Id == ApplyLoan.ComCity);
            ViewBag.BasicDistrict = Entity.BasicDistrict.FirstOrNew(n => n.Id == ApplyLoan.ComDistrict);
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(ApplyLoan ApplyLoan)
        {
            ApplyLoan baseApplyLoan = Entity.ApplyLoan.FirstOrDefault(n => n.Id == ApplyLoan.Id);
            baseApplyLoan = Request.ConvertRequestToModel<ApplyLoan>(baseApplyLoan, ApplyLoan);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(ApplyLoan ApplyLoan, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = ApplyLoan.Id.ToString(); }
            int Ret = Entity.ChangeEntity<ApplyLoan>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
