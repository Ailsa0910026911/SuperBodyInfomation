using LokFu.Base;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace LokFu.Areas.Manage.Controllers
{
    public class AgentMoveController : BaseController
    {
        public ActionResult Index(SysAgent SysAgent, EFPagingInfo<SysAgent> p, int IsFirst = 0)
        {
            ViewBag.AgentList = Entity.SysAgent.ToList();
            if (IsFirst == 0)
            {
                PageOfItems<SysAgent> SysAgentList1 = new PageOfItems<SysAgent>(new List<SysAgent>(), 0, 10, 0, new Hashtable());
                ViewBag.SysAgentList = SysAgentList1;
                ViewBag.SysAgent = SysAgent;
                ViewBag.Batch = this.checkPower("Batch");
                ViewBag.AllAgent = this.checkPower("AllAgent");
                return View();
            }
            if (SysAgent.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请选择代理商";
                return View("Error");
            }
            if (!SysAgent.Id.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentID == SysAgent.Id); }
            p.SqlWhere.Add(f => f.Id != SysAgent.Id);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<SysAgent> SysAgentList = Entity.Selects<SysAgent>(p);
            ViewBag.SysAgentList = SysAgentList;
            ViewBag.SysAgent = SysAgent;
            ViewBag.Batch = this.checkPower("Batch");
            ViewBag.AllAgent = this.checkPower("AllAgent");
            return View();
        }

        public void Batch(int agengtid,string InfoList, int Value)
        {
            SysAgent tempAgent = Entity.SysAgent.FirstOrNew(o => o.Id == Value);//调入商户
            SysAgent Agengt = Entity.SysAgent.FirstOrNew(o => o.Id == agengtid);//调出商户
            if (tempAgent == null || Agengt == null)
            {
                Response.Write(0);
            }
            int Ret = 0;
            //string SQL = "update SysAgent set agentid='" + Value + "' where id in("+InfoList+")";
            //Ret = Entity.ExecuteStoreCommand(SQL);
            string[] agents = InfoList.Split(',');
           
            //调入记录
            foreach (var info in agents)
            {
                int temp = int.Parse(info);
                SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(o => o.Id == temp);
                if (SysAgent != null)
                {
                    UsersMoveLog UsersMoveLog = new UsersMoveLog()
                    {
                        AddTime = DateTime.Now,
                        ToSAId = tempAgent.Id,
                        ToName = tempAgent.Name,
                        FromName = Agengt.Name,
                        FromSAId = Agengt.Id,
                        UId = SysAgent.Id,
                        UTrueName = SysAgent.Name,
                        OpName=AdminUser.TrueName,
                        Type=2,
                        Tel=SysAgent.LinkMobile,
                    };
                    SysAgent.AgentID = Value;
                    this.Entity.UsersMoveLog.AddObject(UsersMoveLog);
                }
                
            }
           
            Entity.SaveChanges();
            Response.Write(Ret);
        }


        public void AllAgent(int agengtid, int Value)
        {
            SysAgent tempAgent = Entity.SysAgent.FirstOrNew(o => o.Id == Value);//调入商户
            SysAgent Agengt = Entity.SysAgent.FirstOrNew(o => o.Id == agengtid);//调出商户
            if (tempAgent == null || Agengt == null)
            {
                Response.Write(0);
            }
            int Ret = 0;
            IList<SysAgent> SysAgentList = Entity.SysAgent.Where(o => o.AgentID == agengtid && o.Id != agengtid).ToList();
            //调入记录
            foreach (var info in SysAgentList)
            {
                    UsersMoveLog UsersMoveLog = new UsersMoveLog()
                    {
                        AddTime = DateTime.Now,
                        ToSAId = tempAgent.Id,
                        ToName = tempAgent.Name,
                        FromName = Agengt.Name,
                        FromSAId = Agengt.Id,
                        UId = info.Id,
                        UTrueName = info.Name,
                        OpName = AdminUser.TrueName,
                        Type = 2,
                        Tel=info.LinkMobile,
                    };
                    info.AgentID = Value;
                    this.Entity.UsersMoveLog.AddObject(UsersMoveLog);
            }
            Entity.SaveChanges();
            Response.Write(Ret);
        }

    }
}
