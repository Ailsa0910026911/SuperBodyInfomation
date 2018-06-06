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
    public class ApplyCreditController : BaseController
    {
      
        public ActionResult Index(ApplyCredit ApplyCredit, EFPagingInfo<ApplyCredit> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
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
            if (!ApplyCredit.AgentAId.IsNullOrEmpty()) {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == ApplyCredit.AgentAId).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = SysAgentList.Select(o=>o.Id).ToList();
                    p.SqlWhere.Add(f => UID.Contains(f.AgentAId));
                }
                else
                {
                    p.SqlWhere.Add(f => f.AgentAId == ApplyCredit.AgentAId);
                }
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<ApplyCredit> ApplyCreditList = null;
            if( IsFirst==0 )
            {
               ApplyCreditList = new PageOfItems<ApplyCredit>(new List<ApplyCredit>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                ApplyCreditList = Entity.Selects<ApplyCredit>(p);
            }
            ViewBag.ApplyCreditList = ApplyCreditList;
            ViewBag.ApplyCredit = ApplyCredit;
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier==1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(ApplyCredit ApplyCredit)
        {
            if (ApplyCredit.Id != 0) ApplyCredit = Entity.ApplyCredit.FirstOrDefault(n => n.Id == ApplyCredit.Id);
            if (ApplyCredit == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.ApplyCredit = ApplyCredit;
            ViewBag.BasicBank = Entity.BasicBank.FirstOrNew(n => n.Id == ApplyCredit.BankId);
            ViewBag.BasicProvince = Entity.BasicProvince.FirstOrNew(n => n.Id == ApplyCredit.ComProvince);
            ViewBag.BasicCity = Entity.BasicCity.FirstOrNew(n => n.Id == ApplyCredit.ComCity);
            ViewBag.BasicDistrict = Entity.BasicDistrict.FirstOrNew(n => n.Id == ApplyCredit.ComDistrict);
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(ApplyCredit ApplyCredit)
        {
            ApplyCredit baseApplyCredit = Entity.ApplyCredit.FirstOrDefault(n => n.Id == ApplyCredit.Id);
            baseApplyCredit = Request.ConvertRequestToModel<ApplyCredit>(baseApplyCredit, ApplyCredit);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(ApplyCredit ApplyCredit, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = ApplyCredit.Id.ToString(); }
            int Ret = Entity.ChangeEntity<ApplyCredit>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
