using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class UsersMoveLogController : BaseController
    {

        public ActionResult Index(UsersMoveLog UsersMoveLog, EFPagingInfo<UsersMoveLog> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<UsersMoveLog> UsersMoveLogList1 = new PageOfItems<UsersMoveLog>(new List<UsersMoveLog>(), 0, 10, 0, new Hashtable());
                ViewBag.UsersMoveLogList = UsersMoveLogList1;
                ViewBag.UsersMoveLog = UsersMoveLog;
                return View();
            }
            if (!UsersMoveLog.UTrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UTrueName.Contains(UsersMoveLog.UTrueName)); }
            p.OrderByList.Add("AddTime", "DESC");
            IPageOfItems<UsersMoveLog> UsersMoveLogList = Entity.Selects<UsersMoveLog>(p);
            ViewBag.UsersMoveLogList = UsersMoveLogList;
            ViewBag.UsersMoveLog = UsersMoveLog;
            return View();
        }

    }
}
