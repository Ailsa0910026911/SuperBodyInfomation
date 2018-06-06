using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Collections;
namespace LokFu.Areas.Manage.Controllers
{
    /// <summary>
    /// 跑批异常订单
    /// </summary>
    public class CheckResultController : BaseController
    {
        /// <summary>
        /// 异常订单列表
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(CheckResult CheckResult, EFPagingInfo<CheckResult> p, DateTime? StartDT, DateTime? EndDT, int IsFirst = 0)
        {
            if (!CheckResult.CheckType.IsNullOrEmpty())
            {
                p.SqlWhere.Add(o => o.CheckType == CheckResult.CheckType);
            }
            if (!CheckResult.CheckMsg.IsNullOrEmpty())
            {
                var id = Entity.Users.Where(o => o.UserName == CheckResult.CheckMsg).Select(o=>o.Id).FirstOrDefault();
                p.SqlWhere.Add(o => o.UId == id);
            }
            if (!CheckResult.TNum.IsNullOrEmpty())
            {
                p.SqlWhere.Add(o => o.TNum == CheckResult.TNum);
            }
            if (StartDT.HasValue)
            {
                p.SqlWhere.Add(o => o.TaskDate >= StartDT.Value);
            }
            if (EndDT.HasValue)
            {
                p.SqlWhere.Add(o => o.TaskDate <= EndDT);
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<CheckResult> CheckResultList = null;
            if (IsFirst == 0)
            {
                CheckResultList = new PageOfItems<CheckResult>(new List<CheckResult>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                CheckResultList = Entity.Selects<CheckResult>(p);
            }
            var uids = CheckResultList.Select(o => o.UId).ToList();
            var UsersList = Entity.Users.Where(o => uids.Contains(o.Id)).Select(o => new CheckUserModel { Id = o.Id, TrueName = o.TrueName, UserName = o.UserName }).ToList();
            ViewBag.CheckResultList = CheckResultList;
            ViewBag.CheckResult = CheckResult;
            ViewBag.UsersList = UsersList;
            ViewBag.StartDT = StartDT;
            ViewBag.EndDT = EndDT;
            return View();
        }

    }
}
