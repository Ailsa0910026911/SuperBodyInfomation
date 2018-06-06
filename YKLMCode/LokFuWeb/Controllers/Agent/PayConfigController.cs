using System.Linq;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Web.Mvc;
using System.Collections;
using System.Collections.Generic;
namespace LokFu.Areas.Agent.Controllers
{
    public class PayConfigController : BaseController
    {
        //
        // GET: /PayConfigChange/
        public ActionResult Index()
        {
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.UserPayAgentList = Entity.UserPayAgent.Where(n => n.AId == BasicAgent.Id).ToList();
            
            Session["Url"] = "/Agent/PayConfig/Index.html?success=true";
            ViewBag.Save = this.checkPower("Save");
            return View();
        }

        [ValidateInput(false)]
        public void Save(int[] PId, double[] Cost, int[] PState, int? AnsyCash, int? AnsyNext)
        {
            bool Check = true;

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
                    UserPayAgent PCT = Entity.UserPayAgent.FirstOrNew(n => n.AId == BasicAgent.Id && n.PId == Pid);
                    PCT.Cost = Cost[i] / 1000;
                    if (PCT.Id.IsNullOrEmpty())
                    {
                        PCT.PId = PId[i];
                        PCT.AId = BasicAgent.Id;
                        Entity.UserPayAgent.AddObject(PCT);
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
            }
            else
            {
                Response.Redirect("/Agent/home/error.html?msg=费率设置有误~");
                return;
            }

            IList<PayConfig> PayConfigList = new List<PayConfig>();
            IList<UserPayAgent> UserPayAgentList = new List<UserPayAgent>();
            IList<SysAgent> AgentList = new List<SysAgent>();
            string AgentIds = "0";
            if (AnsyCash == 1 || AnsyNext == 1)
            {
                PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
                UserPayAgentList = Entity.UserPayAgent.Where(n => n.AId == BasicAgent.Id).ToList();
                //取得代理商所有的下级
                AgentList = BasicAgent.GetSupAgent(Entity, true);
                foreach (var p in AgentList)
                {
                    AgentIds = AgentIds + "," + p.Id;
                }
            }
            if (AnsyCash == 1)
            {
                //使用删除全部后根据用户表生成，有效解决了因接口关闭或新增加接口，老用户没有配置问题
                string SQL = "Delete UserPay Where UId in(Select Id From Users Where Agent in(" + AgentIds + "))";
                Entity.ExecuteStoreCommand(SQL);
                foreach (var p in PayConfigList)
                {
                    double? cost = p.CostUser;
                    UserPayAgent PCT = UserPayAgentList.FirstOrNew(n => n.AId == BasicAgent.Id && n.PId == p.Id);
                    if (!PCT.Id.IsNullOrEmpty())
                    {
                        cost = PCT.Cost;
                    }
                    SQL = "INSERT INTO UserPay(UId,PId,Cost,IsDel) Select ID," + p.Id + " As PId," + cost + " As Cost, 0 As IsDel From Users where Id in(Select Id From Users Where Agent in(" + AgentIds + "))";
                    Entity.ExecuteStoreCommand(SQL);
                }
            }
            if (AnsyNext == 1)
            {
                //使用删除全部后根据用户表生成，有效解决了因接口关闭或新增加接口，老用户没有配置问题
                string SQL = "Delete UserPayAgent Where AId in (" + AgentIds + ")";
                Entity.ExecuteStoreCommand(SQL);
                foreach (var p in PayConfigList)
                {
                    double? cost = p.CostUser;
                    UserPayAgent PCT = UserPayAgentList.FirstOrNew(n => n.AId == BasicAgent.Id && n.PId == p.Id);
                    if (!PCT.Id.IsNullOrEmpty())
                    {
                        cost = PCT.Cost;
                    }
                    SQL = "INSERT INTO UserPayAgent(AId,PId,Cost,IsDel) Select ID," + p.Id + " As PId," + cost + " As Cost, 0 As IsDel From SysAgent where Id in(" + AgentIds + ")";
                    Entity.ExecuteStoreCommand(SQL);
                }
            }
            if (AnsyCash == 1 || AnsyNext == 1)
            {
                APIExtensions.ClearCacheAll();
            }
            BaseRedirect();
        }
    }
}
