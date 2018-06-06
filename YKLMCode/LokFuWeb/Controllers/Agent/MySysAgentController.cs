using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class MySysAgentController : BaseController
    {
        public ActionResult Edit(SysAgent SysAgent)
        {
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        [ValidateInput(false)]
        public void Save(SysAgent SysAgent)
        {
            SysAgent baseSysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == BasicAgent.Id);
            SysAgent.Name = baseSysAgent.Name;
            SysAgent.State = baseSysAgent.State;

            SysAgent.Cash0 = baseSysAgent.Cash0;
            SysAgent.Cash1 = baseSysAgent.Cash1;
            SysAgent.ECash0 = baseSysAgent.ECash0;
            SysAgent.ECash1 = baseSysAgent.ECash1;
            SysAgent.Cash0Times = baseSysAgent.Cash0Times;
            SysAgent.Cash1Times = baseSysAgent.Cash1Times;

            baseSysAgent = Request.ConvertRequestToModel<SysAgent>(baseSysAgent, SysAgent);
            Entity.SaveChanges();
            BaseRedirect();
        }

        public ActionResult Money()
        {
            ViewBag.MoneySave = this.checkPower("MoneySave");
            return View();
        }
        [ValidateInput(false)]
        public object MoneySave(SysAgent SysAgent, int? AnsyCash0, int? AnsyCash1, int? AnsyNext0, int? AnsyNext1)
        {

            SysAgent.Cash0 = SysAgent.Cash0 / 1000;
            SysAgent.Cash1 = SysAgent.Cash1 / 1000;

            if (SysAgent.Cash0 < BasicSet.AgentCash0 || SysAgent.ECash0 < BasicSet.AgentECash0 || SysAgent.Cash1 < BasicSet.AgentCash1 || SysAgent.ECash1 < BasicSet.AgentECash1)
            {
                ViewBag.ErrorMsg = "费率设置有误！";
                return View("Error");
            }
            if (SysAgent.Cash0Times > BasicSet.AgentCash0Times || SysAgent.Cash1Times > BasicSet.AgentCash1Times)
            {
                ViewBag.ErrorMsg = "费率设置有误！";
                return View("Error");
            }


            SysAgent.Name = BasicAgent.Name;
            SysAgent.State = BasicAgent.State;
            BasicAgent = Request.ConvertRequestToModel<SysAgent>(BasicAgent, SysAgent);

            Entity.SaveChanges();

            string AgentIds = BasicAgent.Id.ToString();//用户下方批量更新下级代理
            IList<SysAgent> AgentList = null;
            if (!AnsyCash0.IsNullOrEmpty() || !AnsyCash1.IsNullOrEmpty() || !AnsyNext0.IsNullOrEmpty() || !AnsyNext1.IsNullOrEmpty())
            {
                if (BasicAgent.Tier < BasicAgent.AgentLevelMax)// 当前级数小于最大级数，说明还有
                {
                    //取得代理商所有的下级
                    AgentList = BasicAgent.GetSupAgent(Entity, true);
                    foreach (var p in AgentList)
                    {
                        AgentIds = AgentIds + "," + p.Id;
                    }
                }
            }

            if (AnsyCash0 == 1)//同步到用户
            {
                string SQL = "Update Users Set ECash0=" + BasicAgent.ECash0 + ",Cash0=" + BasicAgent.Cash0 + " where Agent in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyCash1 == 1)//同步到用户
            {
                string SQL = "Update Users Set ECash1=" + BasicAgent.ECash1 + ",Cash1=" + BasicAgent.Cash1 + " where Agent in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyNext0 == 1)//同步到代理
            {
                string SQL = "Update SysAgent Set ECash0=" + BasicAgent.ECash0 + ",Cash0=" + BasicAgent.Cash0 + ",Cash0Times=" + BasicAgent.Cash0Times + " where Id in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            if (AnsyNext1 == 1)//同步到代理
            {
                string SQL = "Update SysAgent Set ECash1=" + BasicAgent.ECash1 + ",Cash1=" + BasicAgent.Cash1 + ",Cash1Times=" + BasicAgent.Cash1Times + " where Id in(" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
            }
            BaseRedirect();
            return null;
        }
        [ValidateInput(false)]
        public JsonResult EditChecktelephone(string fieldId, string fieldValue)
        {
            if (fieldValue.Contains("400-608-6765") || fieldValue.Contains("23769678") || fieldValue.Contains("22220076") || fieldValue.Contains("4006086765"))
            {
                return Json(new object[] { "Tel", false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new object[] { "Tel", true }, JsonRequestBehavior.AllowGet);
        }
    }
}
