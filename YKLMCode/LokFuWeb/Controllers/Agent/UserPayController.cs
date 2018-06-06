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
    public class UserPayController : BaseController
    {
        public ActionResult Index(UserPay UserPay)
        {
            Users Users = Entity.Users.FirstOrNew(n => n.Id == UserPay.UId );
            if (Users == null)
            {
                ViewBag.ErrorMsg = "查询的商户不存在";
                return View("Error");
            }
            if (!IsBelongToAgent(Users.Agent))
            {
                ViewBag.ErrorMsg = "只能查询当前用户下属代理的商户";
                return View("Error");
            }
            ViewBag.UserPayList = Entity.UserPay.Where(n => n.UId == UserPay.UId).ToList();
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.Users = Users;
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent);
            return View();
        }
    }
}
