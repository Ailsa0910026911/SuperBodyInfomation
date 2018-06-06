using LokFu.Extensions;
using LokFu.Repositories;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class SysMoneySetController : BaseController
    {
        public ActionResult PaySplitSet()
        {
            ViewBag.SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            ViewBag.PaySplitSetSave = this.checkPower("PaySplitSetSave");
            ViewBag.PaySplitSetEdit = this.checkPower("PaySplitSetEdit");
            return View();
        }
        [ValidateInput(false)]
        public object PaySplitSetSave(SysMoneySet SysMoneySet)
        {
            SysMoneySet.PaySplitA1 = SysMoneySet.PaySplitA1 / 10000;
            SysMoneySet.PaySplitA2 = SysMoneySet.PaySplitA2 / 10000;
            SysMoneySet.PaySplitA3 = SysMoneySet.PaySplitA3 / 10000;
            SysMoneySet.PaySplitA4 = SysMoneySet.PaySplitA4 / 10000;
            SysMoneySet.PaySplitA5 = SysMoneySet.PaySplitA5 / 10000;
            SysMoneySet.PaySplitA6 = SysMoneySet.PaySplitA6 / 10000;
            SysMoneySet.PaySplitU0 = SysMoneySet.PaySplitU0 / 10000;
            SysMoneySet.PaySplitU1 = SysMoneySet.PaySplitU1 / 10000;
            SysMoneySet.PaySplitU2 = SysMoneySet.PaySplitU2 / 10000;
            SysMoneySet baseSysMoneySet = Entity.SysMoneySet.FirstOrNew();
            baseSysMoneySet = Request.ConvertRequestToModel<SysMoneySet>(baseSysMoneySet, SysMoneySet);
            Entity.SaveChanges();
            Response.Redirect("/Manage/SysMoneySet/PaySplitSet.html");
            return null;
        }
        public ActionResult JobSplitSet()
        {
            ViewBag.SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            ViewBag.JobSplitSetSave = this.checkPower("JobSplitSetSave");
            ViewBag.JobSplitSetEdit = this.checkPower("JobSplitSetEdit");
            return View();
        }
        [ValidateInput(false)]
        public object JobSplitSetSave(SysMoneySet SysMoneySet)
        {
            SysMoneySet.JobSplitA1 = SysMoneySet.JobSplitA1 / 10000;
            SysMoneySet.JobSplitA2 = SysMoneySet.JobSplitA2 / 10000;
            SysMoneySet.JobSplitA3 = SysMoneySet.JobSplitA3 / 10000;
            SysMoneySet.JobSplitA4 = SysMoneySet.JobSplitA4 / 10000;
            SysMoneySet.JobSplitA5 = SysMoneySet.JobSplitA5 / 10000;
            SysMoneySet.JobSplitA6 = SysMoneySet.JobSplitA6 / 10000;
            SysMoneySet.JobSplitU0 = SysMoneySet.JobSplitU0 / 10000;
            SysMoneySet.JobSplitU1 = SysMoneySet.JobSplitU1 / 10000;
            SysMoneySet.JobSplitU2 = SysMoneySet.JobSplitU2 / 10000;
            SysMoneySet baseSysMoneySet = Entity.SysMoneySet.FirstOrNew();
            baseSysMoneySet = Request.ConvertRequestToModel<SysMoneySet>(baseSysMoneySet, SysMoneySet);
            Entity.SaveChanges();
            Response.Redirect("/Manage/SysMoneySet/JobSplitSet.html");
            return null;
        }
        public ActionResult VipSplitSet()
        {
            ViewBag.SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            ViewBag.VipSplitSetSave = this.checkPower("VipSplitSetSave");
            ViewBag.VipSplitSetEdit = this.checkPower("VipSplitSetEdit");
            return View();
        }
        [ValidateInput(false)]
        public object VipSplitSetSave(SysMoneySet SysMoneySet)
        {
            SysMoneySet.VipSplitA1 = SysMoneySet.VipSplitA1 / 100;
            SysMoneySet.VipSplitA2 = SysMoneySet.VipSplitA2 / 100;
            SysMoneySet.VipSplitA3 = SysMoneySet.VipSplitA3 / 100;
            SysMoneySet.VipSplitA4 = SysMoneySet.VipSplitA4 / 100;
            SysMoneySet.VipSplitA5 = SysMoneySet.VipSplitA5 / 100;
            SysMoneySet.VipSplitA6 = SysMoneySet.VipSplitA6 / 100;

            SysMoneySet.VipSplitU0 = SysMoneySet.VipSplitU0 / 100;
            SysMoneySet.VipSplitU1 = SysMoneySet.VipSplitU1 / 100;
            SysMoneySet.VipSplitU2 = SysMoneySet.VipSplitU2 / 100;
            SysMoneySet baseSysMoneySet = Entity.SysMoneySet.FirstOrNew();
            baseSysMoneySet = Request.ConvertRequestToModel<SysMoneySet>(baseSysMoneySet, SysMoneySet);
            Entity.SaveChanges();
            Response.Redirect("/Manage/SysMoneySet/VipSplitSet.html");
            return null;
        }


        public ActionResult AgentSplitSet()
        {
            ViewBag.SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            ViewBag.AgentSplitSetSave = this.checkPower("AgentSplitSetSave");
            ViewBag.AgentSplitSetEdit = this.checkPower("AgentSplitSetEdit");
            return View();
        }
        [ValidateInput(false)]
        public object AgentSplitSetSave(SysMoneySet SysMoneySet)
        {
            SysMoneySet.AgentSplit0 = SysMoneySet.AgentSplit0 / 100;
            SysMoneySet.AgentSplit1 = SysMoneySet.AgentSplit1 / 100;
            SysMoneySet.AgentSplit2 = SysMoneySet.AgentSplit2 / 100;
            SysMoneySet.AgentSplit3 = SysMoneySet.AgentSplit3 / 100;
            SysMoneySet.AgentSplit4 = SysMoneySet.AgentSplit4 / 100;
            SysMoneySet.AgentSplit5 = SysMoneySet.AgentSplit5 / 100;

            SysMoneySet.SameAgent = SysMoneySet.SameAgent / 100;
            SysMoneySet baseSysMoneySet = Entity.SysMoneySet.FirstOrNew();
            baseSysMoneySet = Request.ConvertRequestToModel<SysMoneySet>(baseSysMoneySet, SysMoneySet);
            Entity.SaveChanges();
            Response.Redirect("/Manage/SysMoneySet/AgentSplitSet.html");
            return null;
        }
    }
}
