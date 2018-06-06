using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Models;
using LokFu.Infrastructure;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
namespace LokFu.Areas.Base.Controllers
{
    public class JoinController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index(string comeurl)
        {
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            ViewBag.ComeUrl = comeurl;
            return View();
        }
        public void Add(ApplyJoin ApplyJoin, string code, string comeurl)
        {
            if (code.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("<script>alert('验证码错误');history.go(-1);</script>");
                return;
            }
            Session.ClearCheckCode();
            ApplyJoin.State = 1;
            ApplyJoin.AddTime = DateTime.Now;
            ApplyJoin.AgentId = 0;
            ApplyJoin.AgentAId = 0;
            Entity.ApplyJoin.AddObject(ApplyJoin);
            Entity.SaveChanges();
            Response.Write("<script>alert(\"提交成功~！\");location.href=\"/join/?comeurl=" + comeurl + "\";</script>");
        }
        public ActionResult Success(string comeurl)
        {
            ViewBag.ComeUrl = comeurl;
            return View();
        }
    }
}
 