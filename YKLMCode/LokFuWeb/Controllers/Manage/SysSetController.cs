using LokFu.Extensions;
using LokFu.Repositories;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class SysSetController : BaseController
    {
        public ActionResult Edit()
        {
            ViewBag.SysSet = BasicSet;
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        [ValidateInput(false)]
        public object Save(SysSet SysSet)
        {
            SysSet.AutoAuthAppCodeIOS = SysSet.AutoAuthAppCodeIOS ?? string.Empty;
            SysSet.AutoAuthAppKeyIOS = SysSet.AutoAuthAppKeyIOS ?? string.Empty;
            SysSet.AutoAuthAppSecretIOS = SysSet.AutoAuthAppSecretIOS ?? string.Empty;
            SysSet.AutoAuthAppCodeAndroid = SysSet.AutoAuthAppCodeAndroid ?? string.Empty;
            SysSet.AutoAuthAppKeyAndroid = SysSet.AutoAuthAppKeyAndroid ?? string.Empty;
            SysSet.AutoAuthAppSecretAndroid = SysSet.AutoAuthAppSecretAndroid ?? string.Empty;
            SysSet baseSysSet = BasicSet;
            baseSysSet = Request.ConvertRequestToModel<SysSet>(baseSysSet, SysSet);
            if (!baseSysSet.NoWord.IsNullOrEmpty())
            {
                baseSysSet.NoWord = baseSysSet.NoWord.Replace("｜", "|").Replace(" ", "").Replace("　", "");
            }
            Entity.SaveChanges();
            //BaseRedirect();
            Response.Redirect("/Manage/SysSet/Edit.html");
            return null;
        }
        public ActionResult Money()
        {
            ViewBag.SysSet = BasicSet;
            ViewBag.MoneySave = this.checkPower("MoneySave");
            return View();
        }
        [ValidateInput(false)]
        public void MoneySave(SysSet SysSet, int? AnsyCash0, int? AnsyCash1, int? AnsyAgent0, int? AnsyAgent1)
        {
            SysSet.House = SysSet.House / 1000;

            SysSet.Cash0 = SysSet.Cash0 / 1000;
            SysSet.Cash1 = SysSet.Cash1 / 1000;

            SysSet.SysCash0 = SysSet.SysCash0 / 1000;
            SysSet.SysCash1 = SysSet.SysCash1 / 1000;

            SysSet.AgentCash0 = SysSet.AgentCash0 / 1000;
            SysSet.AgentCash1 = SysSet.AgentCash1 / 1000;

            SysSet.PayConfigAgent = SysSet.PayConfigAgent / 100;
            SysSet baseSysSet = BasicSet;
            SysSet.BaoUserAlert = SysSet.BaoUserAlert ?? string.Empty;
            SysSet.CashAlertMsgT0 = SysSet.CashAlertMsgT0 ?? string.Empty;
            SysSet.CashAlertMsgT1 = SysSet.CashAlertMsgT1 ?? string.Empty;
            baseSysSet = Request.ConvertRequestToModel<SysSet>(baseSysSet, SysSet);
            Entity.SaveChanges();

            if (AnsyAgent0 == 1)
            {
                string SQL = "Update SysAgent Set ECash0=" + baseSysSet.ECash0 + ",Cash0=" + baseSysSet.Cash0 + ",Cash0Times=" + baseSysSet.Cash0Times;
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyAgent1 == 1)
            {
                string SQL = "Update SysAgent Set ECash1=" + baseSysSet.ECash1 + ",Cash1=" + baseSysSet.Cash1 + ",Cash1Times=" + baseSysSet.Cash1Times;
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyCash0 == 1)
            {
                string SQL = "Update Users Set ECash0=" + baseSysSet.ECash0 + ",Cash0=" + baseSysSet.Cash0;
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyCash1 == 1)
            {
                string SQL = "Update Users Set ECash1=" + baseSysSet.ECash1 + ",Cash1=" + baseSysSet.Cash1;
                Entity.ExecuteStoreCommand(SQL);
            }

            if (AnsyAgent0 == 1 || AnsyAgent1 == 1 || AnsyCash0 == 1 || AnsyCash1 == 1)
            {
                APIExtensions.ClearCacheAll();
            }
            Response.Redirect("/Manage/SysSet/Money.html");
        }
        public ActionResult Agreement(int id =0)
        {
            int Id = BasicSet.Id;
            string Agreement = BasicSet.Agreement;
            if(id != 0)
            {
                var SysAgent = this.Entity.SysAgent.Where(o => o.Id == id).FirstOrDefault();
                if (SysAgent != null)
                {
                    Id = SysAgent.Id;
                    Agreement = SysAgent.Agreement;
                }
            }
            var SysAgentList = this.Entity.SysAgent.Where(o => o.IsTeiPai == 1).ToDictionary(o => o.Id, o => o.APPName);
            ViewBag.Id = Id;
            ViewBag.Agreement = Agreement;
            ViewBag.SysAgentList = SysAgentList;
            //ViewBag.SysSet = BasicSet;
            ViewBag.AgreementSave = this.checkPower("AgreementSave");
            return View();
        }
        [ValidateInput(false)]
        public void AgreementSave(SysSet SysSet)
        {
            if (SysSet.Id == 0)
            {
                SysSet baseSysSet = BasicSet;
                baseSysSet = Request.ConvertRequestToModel<SysSet>(baseSysSet, SysSet);
            }
            else
            {
                var SysAgent = this.Entity.SysAgent.Where(o => o.Id == SysSet.Id).FirstOrDefault();
                if (SysAgent != null)
                {
                    SysAgent.Agreement = SysSet.Agreement;
                }
            }
            
            Entity.SaveChanges();
            Response.Redirect("/Manage/SysSet/Agreement.html?Id=" + SysSet.Id);
        }

        public ActionResult EditCache()
        {
            return View();
        }
        public ActionResult FastSet()
        {
            FastConfig FastConfig = Entity.FastConfig.FirstOrDefault();
            ViewBag.FastConfig = FastConfig;
            ViewBag.FastSetSave = this.checkPower("FastSetSave");
            return View();
        }
        public void FastSetSave(FastConfig FastConfig)
        {
            FastConfig.UserCost = FastConfig.UserCost / 1000;
            FastConfig baseFastConfig = Entity.FastConfig.FirstOrDefault();
            baseFastConfig = Request.ConvertRequestToModel<FastConfig>(baseFastConfig, FastConfig);
            Entity.SaveChanges();
            Response.Redirect("/Manage/SysSet/FastSet.html");
        }
    }
}
