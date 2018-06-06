using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class UserLogController : BaseController
    {
        public ActionResult Index(UserLog UserLog, EFPagingInfo<UserLog> p)
        {
            Users Users = Entity.Users.FirstOrNew(n => n.Id == UserLog.UId && n.Agent == BasicAgent.Id);
            if (Users != null)
            {
                p.SqlWhere.Add(f => f.UId == UserLog.UId);
                if (!UserLog.OId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OId == UserLog.OId); }
                if (!UserLog.OType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.OType == UserLog.OType); }
                if (!UserLog.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == UserLog.State); }
                p.OrderByList.Add("Id", "DESC");
                IPageOfItems<UserLog> UserLogList = Entity.Selects<UserLog>(p);
                ViewBag.UserLogList = UserLogList;
            }
            else
            {
                ViewBag.UserLogList = new List<UserLog>();
            }
            ViewBag.Users = Users;
            ViewBag.UserLog = UserLog;
            return View();
        }
    }
}
