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
    /// <summary>
    /// 收付直通车用户
    /// </summary>
    public class FastUserController : BaseController
    {
        public ActionResult Index(FastUser FastUser, EFPagingInfo<FastUser> p, DateTime? STime, DateTime? ETime, int IsFirst = 0)
        {
            #region 筛选条件
            if (!FastUser.TrueName.IsNullOrEmpty())
            { 
                if (!FastUser.UId.IsNullOrEmpty())
                {
                    switch (FastUser.UId)
                    {
                        case 1:
                            p.SqlWhere.Add(f => f.TrueName == FastUser.TrueName);
                            break;
                        case 2:
                            p.SqlWhere.Add(f => f.CardId == FastUser.CardId);
                            break;
                        case 3:
                            p.SqlWhere.Add(f => f.Card == FastUser.Card);
                            break;
                        case 4:
                            p.SqlWhere.Add(f => f.Bin == FastUser.Bin);
                            break;
                    }
                }
            }
            if (!FastUser.Bank.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.Bank == FastUser.Bank);
            }
            if (STime.HasValue)
            {
                p.SqlWhere.Add(f => f.AddTime >= STime);
            }
            if (ETime.HasValue)
            {
                p.SqlWhere.Add(f => f.AddTime <= ETime);
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<FastUser> FastUserList = null;
            if (IsFirst == 0)
            {
                FastUserList = new PageOfItems<FastUser>(new List<FastUser>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                FastUserList = Entity.Selects<FastUser>(p);
            }
            ViewBag.FastUserList = FastUserList;
            ViewBag.FastUser = FastUser;
            ViewBag.STime = STime;
            ViewBag.ETime = ETime;

            return View();
        }
    }
}
