using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Models;
using LokFu.Infrastructure;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
namespace LokFu.Areas.Mobile.Controllers
{
    public class HuanKuanController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index(string comeurl)
        {
            if (BasicUsers.Id.IsNullOrEmpty()) {
                ViewBag.ErrorMsg = "登录信息有误";
                return View("Error");
            }
            return View();
        }
        public void Add(decimal Amount, string code)
        {
            if (BasicUsers.Id.IsNullOrEmpty())
            {
                Response.Write("e1");
                return;
            }
            if (code.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("e2");
                return;
            }
            Session.ClearCheckCode();
            UserPayCredit UPC = Entity.UserPayCredit.FirstOrDefault(n => n.UId == BasicUsers.Id && n.State == 1);
            if (UPC != null) {
                Response.Write("e3");
                return;
            }
            UserPayCredit UserPayCredit = new UserPayCredit();
            UserPayCredit.UId = BasicUsers.Id;
            UserPayCredit.TrueName = BasicUsers.TrueName;
            UserPayCredit.Mobile = BasicUsers.UserName;
            UserPayCredit.AgentId = BasicUsers.Agent;
            UserPayCredit.AId = BasicUsers.AId;
            UserPayCredit.Amount = Amount;
            UserPayCredit.State = 1;
            UserPayCredit.AddTime = DateTime.Now;
            Entity.UserPayCredit.AddObject(UserPayCredit);
            Entity.SaveChanges();
            Response.Write("ok");
        }
    }
}
 