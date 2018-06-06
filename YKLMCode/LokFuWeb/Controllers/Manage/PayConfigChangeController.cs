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
    public class PayConfigChangeController : BaseController
    {

        public ActionResult Index(PayConfigChange PayConfigChange, EFPagingInfo<PayConfigChange> p, int AgentId = 0, int State = 99, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<PayConfigChange> PayConfigChangeList1 = new PageOfItems<PayConfigChange>(new List<PayConfigChange>(), 0, 10, 0, new Hashtable());
                IList<SysAgent> SysAgentList1 = Entity.SysAgent.Where(x => x.Tier == 1).ToList();
                ViewBag.AgentId = AgentId;
                ViewBag.AgentState = State;
                ViewBag.SysAgentList = SysAgentList1;
                ViewBag.PayConfigChangeList = PayConfigChangeList1;
                ViewBag.PayConfigChange = PayConfigChange;
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (AgentId < 100001)
            {
                p.SqlWhere.Add(x => x.AgentId == AgentId);
            }
            if (State != 99)
            {
                p.SqlWhere.Add(x => x.State == State);
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<PayConfigChange> PayConfigChangeList = Entity.Selects<PayConfigChange>(p);

            IList<SysAgent> SysAgentList = Entity.SysAgent.Where(x => x.Tier == 1).ToList();

            ViewBag.AgentId = AgentId;
            ViewBag.AgentState = State;
            ViewBag.SysAgentList = SysAgentList;
            ViewBag.PayConfigChangeList = PayConfigChangeList;
            ViewBag.PayConfigChange = PayConfigChange;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(PayConfigChange PayConfigChange)
        {
            if (PayConfigChange.Id != 0) PayConfigChange = Entity.PayConfigChange.FirstOrDefault(n => n.Id == PayConfigChange.Id);
            if (PayConfigChange == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.PayConfigChange = PayConfigChange;
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(PayConfigChange PayConfigChange, int[] PId, double[] Cost, int[] PState)
        {
            PayConfigChange.Cash0 = PayConfigChange.Cash0 / 1000;
            PayConfigChange.Cash1 = PayConfigChange.Cash1 / 1000;
            PayConfigChange.AgentId = 0;
            PayConfigChange.State = 1;
            Entity.PayConfigChange.AddObject(PayConfigChange);
            Entity.SaveChanges();
            for (int i = 0; i < PId.Length; i++)
            {
                PayConfigTemp PCT = new PayConfigTemp();
                PCT.PCCId = PayConfigChange.Id;
                PCT.PId = PId[i];
                PCT.Cost = Cost[i] / 1000;
                PCT.State = (byte)PState[i];
                Entity.PayConfigTemp.AddObject(PCT);
            }
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(PayConfigChange PayConfigChange, int[] PId, double[] Cost, int[] PState)
        {
            PayConfigChange.Cash0 = PayConfigChange.Cash0 / 1000;
            PayConfigChange.Cash1 = PayConfigChange.Cash1 / 1000;
            PayConfigChange basePayConfigChange = Entity.PayConfigChange.FirstOrDefault(n => n.Id == PayConfigChange.Id);
            basePayConfigChange = Request.ConvertRequestToModel<PayConfigChange>(basePayConfigChange, PayConfigChange);
            for (int i = 0; i < PId.Length; i++)
            {
                int Pid = PId[i];
                PayConfigTemp PCT = Entity.PayConfigTemp.FirstOrNew(n => n.PCCId == basePayConfigChange.Id && n.PId == Pid);
                PCT.Cost = Cost[i] / 1000;
                PCT.State = (byte)PState[i];
                if (PCT.Id.IsNullOrEmpty())
                {
                    PCT.PId = PId[i];
                    PCT.PCCId = PayConfigChange.Id;
                    Entity.PayConfigTemp.AddObject(PCT);
                }
            }
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(PayConfigChange PayConfigChange, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = PayConfigChange.Id.ToString(); }
            int Ret = Entity.ChangeEntity<PayConfigChange>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        public void Delete(PayConfigChange PayConfigChange, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = PayConfigChange.Id.ToString(); }
            Hashtable DeleteHash = new Hashtable();
            DeleteHash.Add("PayConfigTemp", "PCCId");
            int Ret = Entity.MoveToDeleteEntity<PayConfigChange>(InfoList, IsDel, AdminUser.UserName, DeleteHash);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
