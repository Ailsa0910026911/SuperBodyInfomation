using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections;

namespace LokFu.Areas.Manage.Controllers
{
    public class UserAuthController : BaseController
    {
        //
        // GET: /UserAuth/

        public ActionResult Index(UserAuth UserAuth, EFPagingInfo<UserAuth> p, int IsFirst = 0)
        {
            ViewBag.UserAuth = UserAuth;
            if (IsFirst==0)
            {
                UserAuth.AuthType = 99;
                PageOfItems<UserAuth> UserAuthList1 = new PageOfItems<UserAuth>(new List<UserAuth>(), 0, 10, 0, new Hashtable());
                ViewBag.UserAuthList = UserAuthList1;
                return View(); 
            }
            if (UserAuth.AuthType!=99) p.SqlWhere.Add(u => u.AuthType == UserAuth.AuthType);
            if (!UserAuth.OId.IsNullOrEmpty()) p.SqlWhere.Add(u => u.OId == UserAuth.OId);
            if (!UserAuth.AccountName.IsNullOrEmpty()) p.SqlWhere.Add(u => u.AccountName == UserAuth.AccountName);
            if (!UserAuth.BankAccount.IsNullOrEmpty()) p.SqlWhere.Add(u => u.BankAccount == UserAuth.BankAccount);
            if (!UserAuth.IdentityCode.IsNullOrEmpty()) p.SqlWhere.Add(u => u.IdentityCode == UserAuth.IdentityCode);
            if (!UserAuth.Mobile.IsNullOrEmpty())
            {
                Users Users = Entity.Users.FirstOrNew(u=>u.Mobile==UserAuth.Mobile);
                p.SqlWhere.Add(u=>u.UId==Users.Id);
            }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<UserAuth> UserAuthList = Entity.Selects<UserAuth>(p);
            ViewBag.UserAuthList = UserAuthList;
            return View();
        }

    }
}
