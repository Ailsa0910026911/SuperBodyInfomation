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
    public class SysLogController : BaseController
    {

        public ActionResult Index(SysLog SysLog, EFPagingInfo<SysLog> p, int? AgentSysAdminId, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<SysLog> SysLogList1 = new PageOfItems<SysLog>(new List<SysLog>(), 0, 10, 0, new Hashtable());
                ViewBag.SysLogList = SysLogList1;
                ViewBag.SysLog = SysLog;
                ViewBag.AgentSysAdminId = AgentSysAdminId;
                //后台操作员
                ViewBag.SysAdminList = Entity.SysAdmin.Where(o => o.AgentId == 0).ToList();
                //代理操作员
                ViewBag.AgentSysAdminList = Entity.SysAdmin.Where(o => o.AgentId != 0).ToList();
                return View();
            }
            if (SysLog.PType == 1 || SysLog.PType.IsNullOrEmpty())
            {
                SysLog.PType = 1;
                if (!SysLog.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == SysLog.AId); }
            }
            else
            {
                if (!AgentSysAdminId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == AgentSysAdminId); }
            }
            if (!SysLog.ControllerName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ControllerName == SysLog.ControllerName); }
            if (!SysLog.ActionName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ActionName == SysLog.ActionName); }
            if (!SysLog.Title.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Title.Contains(SysLog.Title)); }
            p.SqlWhere.Add(f => f.PType == SysLog.PType);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<SysLog> SysLogList = Entity.Selects<SysLog>(p);
            ViewBag.SysLogList = SysLogList;
            ViewBag.SysLog = SysLog;
            ViewBag.AgentSysAdminId = AgentSysAdminId;
            //后台操作员
            ViewBag.SysAdminList = Entity.SysAdmin.Where(o => o.AgentId == 0).ToList();
            //代理操作员
            ViewBag.AgentSysAdminList = Entity.SysAdmin.Where(o => o.AgentId != 0).ToList();
            return View();
        }
    }
}
