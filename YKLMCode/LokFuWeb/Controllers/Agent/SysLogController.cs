using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
namespace LokFu.Areas.Agent.Controllers
{
    public class SysLogController : BaseController
    {
        
        public ActionResult Index(SysLog SysLog, EFPagingInfo<SysLog> p,int IsFirst=0)
        {
            var SysAdminList = this.Entity.SysAdmin.Where(o => o.AgentId == this.BasicAgent.Id).ToList();
            ViewBag.SysAdminList = SysAdminList;
            if (IsFirst==0)
            {
                PageOfItems<SysLog> SysLogList1 = new PageOfItems<SysLog>(new List<SysLog>(), 0, 10, 0, new Hashtable());
                ViewBag.SysLogList = SysLogList1;
                ViewBag.SysLog = SysLog;
                return View();
                
            }
           
            if (!SysLog.AId.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.AId == SysLog.AId);
            }
            else
            {
                var ids = SysAdminList.Select(o => o.Id).ToList();
                p.SqlWhere.Add(f => ids.Contains(f.AId));
            }
            if (!SysLog.Title.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Title.Contains(SysLog.Title)); }
            if (!SysLog.ControllerName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ControllerName == SysLog.ControllerName); }
            if (!SysLog.ActionName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ActionName == SysLog.ActionName); }
            p.SqlWhere.Add(f => f.PType == 2);
            p.OrderByList.Add("AddTime", "DESC");
            IPageOfItems<SysLog> SysLogList = Entity.Selects<SysLog>(p);
            ViewBag.SysLogList = SysLogList;
            ViewBag.SysLog = SysLog;
            return View();
        }

    }
}
