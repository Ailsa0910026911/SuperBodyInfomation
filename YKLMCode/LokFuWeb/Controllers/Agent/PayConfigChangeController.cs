using System.Linq;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Web.Mvc;
using System.Collections;
namespace LokFu.Areas.Agent.Controllers
{
    public class PayConfigChangeController : BaseController
    {
        //
        // GET: /PayConfigChange/

        public ActionResult Index(PayConfigChange PayConfigChange, EFPagingInfo<PayConfigChange> p)
        {
            p.SqlWhere.Add(x => x.AgentId == BasicAgent.Id);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<PayConfigChange> PayConfigChangeList = Entity.Selects<PayConfigChange>(p);

            ViewBag.AgentState = BasicAgent.AgentState.HasValue ? BasicAgent.AgentState.Value.ToString() : "-1";
            ViewBag.PayConfigChangeList = PayConfigChangeList;
            ViewBag.PayConfigChange = PayConfigChange;
            return View();
        }
        public ActionResult Edit(int Id = 0)
        {
            PayConfigChange PayConfigChange = null;
            if (!Id.IsNullOrEmpty())
            {
                PayConfigChange = Entity.PayConfigChange.FirstOrDefault(n => n.Id == Id && n.AgentId == this.BasicAgent.Id);
                if (PayConfigChange == null)
                {
                    ViewBag.ErrorMsg = "数据不存在";
                    return View("Error");
                }
                if (!IsBelongToAgent(PayConfigChange.AgentId.GetValueOrDefault()))
                {
                    ViewBag.ErrorMsg = AgentLanguage.Surmount;
                    return View("Error");
                }
            }
            else
            {
                PayConfigChange = new PayConfigChange();
            }
            //if (PayConfigChange.Id != 0) PayConfigChange = Entity.PayConfigChange.FirstOrDefault(n => n.Id == PayConfigChange.Id);
            //if (!PayConfigChange.Id.IsNullOrEmpty())
            //{
            //    if (!IsBelongToAgent(PayConfigChange.AgentId.GetValueOrDefault()))
            //    {
            //        ViewBag.ErrorMsg = AgentLanguage.Surmount;
            //        return View("Error");
            //    }
            //}
            
            ViewBag.PayConfigChange = PayConfigChange;
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        /// <summary>
        /// 修改代理商表费率套餐设置
        /// </summary>
        /// <param name="AgentState"></param>
        /// <returns></returns>
        [HttpPost]
        public string EditUpdateAgentId(string AgentState)
        {
            string retChar = "";
            int agentState = 0;
            if (string.IsNullOrWhiteSpace(AgentState) || !int.TryParse(AgentState, out agentState))
            {
                retChar = "10005";
            }
            else
            {
                SysAgent agentModels = Entity.SysAgent.FirstOrDefault(x => x.Id == BasicAgent.Id);
                if (agentModels == null)
                {
                    retChar = "10004";
                }
                else
                {
                    agentModels.AgentState = agentState;
                    Entity.SaveChanges();
                }
            }
            return retChar;
        }
        [ValidateInput(false)]
        public void Add(PayConfigChange PayConfigChange, int[] PId, double[] Cost, int[] PState)
        {
            bool Check = true;

            PayConfigChange.Cash0 = PayConfigChange.Cash0 / 1000;
            PayConfigChange.Cash1 = PayConfigChange.Cash1 / 1000;

            if (PayConfigChange.Cash0 < BasicSet.AgentCash0 || PayConfigChange.ECash0 < BasicSet.AgentECash0 || PayConfigChange.Cash1 < BasicSet.AgentCash1 || PayConfigChange.ECash1 < BasicSet.AgentECash1)
            {
                Check = false;
            }

            PayConfigChange.AgentId = BasicAgent.Id;
            PayConfigChange.State = 1;

            if (!PayConfigChange.BPrice.IsNullOrEmpty())
            {
                PayConfigChange.CPrice = (PayConfigChange.BPrice.Value * BasicSet.PayConfigAgent).Ceiling();//代理商价格
                PayConfigChange.BPrice = (PayConfigChange.BPrice.Value + PayConfigChange.CPrice.Value).Ceiling();
            }else{
                PayConfigChange.APrice = 0;
                PayConfigChange.BPrice = 0;
                PayConfigChange.CPrice = 0;
            }
            if (PayConfigChange.BPrice < 10) {
                Utils.WriteLog(PayConfigChange.AgentId + "|" + PayConfigChange.BPrice, "PayConfigChangeUserError");
                Response.Redirect("/Agent/home/error.html?msg=升级套餐需发布10元以上价格，否则无法完成支付~");
                return;
            }
            Entity.PayConfigChange.AddObject(PayConfigChange);
            if (Check)
            {
                Entity.SaveChanges();
            }
            else
            {
                Response.Redirect("/Agent/home/error.html?msg=费率设置有误~");
                return;
            }

            for (int i = 0; i < PId.Length; i++)
            {
                int Pid = PId[i];
                double cost = Cost[i] / 1000;
                PayConfig PC = Entity.PayConfig.FirstOrDefault(n => n.Id == Pid);
                if (PC == null)
                {
                    Check = false;
                }
                if (cost >= PC.CostAgent)
                {
                    PayConfigTemp PCT = new PayConfigTemp();
                    PCT.PCCId = PayConfigChange.Id;
                    PCT.PId = Pid;
                    PCT.Cost = cost;
                    PCT.State = (byte)PState[i];
                    Entity.PayConfigTemp.AddObject(PCT);
                }
                else
                {
                    Check = false;
                }
                
            }
            if (Check)
            {
                Entity.SaveChanges();
                BaseRedirect();
            }
            else
            {
                //Entity.PayConfigChange.DeleteObject(PayConfigChange);
                Entity.DeleteObject(PayConfigChange);
                Entity.SaveChanges();
                Response.Redirect("/Agent/home/error.html?msg=费率设置有误~");
            }
        }
        [ValidateInput(false)]
        public ActionResult Save(PayConfigChange PayConfigChange, int[] PId, double[] Cost, int[] PState)
        {
            bool Check = true;
            PayConfigChange.Cash0 = PayConfigChange.Cash0 / 1000;
            PayConfigChange.Cash1 = PayConfigChange.Cash1 / 1000;

            if (PayConfigChange.Cash0 < BasicSet.AgentCash0 || PayConfigChange.ECash0 < BasicSet.AgentECash0 || PayConfigChange.Cash1 < BasicSet.AgentCash1 || PayConfigChange.ECash1 < BasicSet.AgentECash1)
            {
                Check = false;
            }
            PayConfigChange BasePayConfigChange = this.Entity.PayConfigChange.FirstOrDefault(o => o.Id == PayConfigChange.Id);
            if (BasePayConfigChange == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(BasePayConfigChange.AgentId.GetValueOrDefault()))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            BasePayConfigChange = Request.ConvertRequestToModel<PayConfigChange>(BasePayConfigChange, PayConfigChange);

            if (!BasePayConfigChange.BPrice.IsNullOrEmpty())
            {
                BasePayConfigChange.CPrice = BasePayConfigChange.BPrice * BasicSet.PayConfigAgent;//代理商价格
                BasePayConfigChange.BPrice = BasePayConfigChange.BPrice + BasePayConfigChange.CPrice;
            }
            else
            {
                BasePayConfigChange.APrice = 0;
                BasePayConfigChange.BPrice = 0;
                BasePayConfigChange.CPrice = 0;
            }
            if (PayConfigChange.BPrice < 10)
            {
                Utils.WriteLog(PayConfigChange.AgentId + "|" + PayConfigChange.BPrice, "PayConfigChangeUserError");
                Response.Redirect("/Agent/home/error.html?msg=升级套餐需发布10元以上价格，否则无法完成支付~");
                return null;
            }
            for (int i = 0; i < PId.Length; i++)
            {
                int Pid = PId[i];
                double cost = Cost[i] / 1000;
                PayConfig PC = Entity.PayConfig.FirstOrDefault(n => n.Id == Pid);
                if (PC == null)
                {
                    Check = false;
                }
                if (cost >= PC.CostAgent)
                {
                    PayConfigTemp PCT = Entity.PayConfigTemp.FirstOrNew(n => n.PCCId == BasePayConfigChange.Id && n.PId == Pid);
                    PCT.Cost = Cost[i] / 1000;
                    PCT.State = (byte)PState[i];
                    if (PCT.Id.IsNullOrEmpty())
                    {
                        PCT.PId = PId[i];
                        PCT.PCCId = PayConfigChange.Id;
                        Entity.PayConfigTemp.AddObject(PCT);
                    }
                }
                else
                {
                    Check = false;
                }
            }
            if (Check)
            {
                Entity.SaveChanges();
                BaseRedirect();
            }
            else
            {
                Response.Redirect("/Agent/home/error.html?msg=费率设置有误~");
            }
            return null;
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
        public ActionResult Delete(PayConfigChange PayConfigChange, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = PayConfigChange.Id.ToString(); }
            PayConfigChange = this.Entity.PayConfigChange.FirstOrNew(o => o.Id == PayConfigChange.Id);
            if (PayConfigChange == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(PayConfigChange.AgentId.GetValueOrDefault()))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            Hashtable DeleteHash = new Hashtable();
            DeleteHash.Add("PayConfigTemp", "PCCId");
            int Ret = Entity.MoveToDeleteEntity<PayConfigChange>(InfoList, IsDel, BasicAgent.Name, DeleteHash);
            Entity.SaveChanges();
            Response.Write(Ret);
            return null;
        }
    }
}
