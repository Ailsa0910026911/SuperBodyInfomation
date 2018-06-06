using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using System;
using System.Text.RegularExpressions;
namespace LokFu.Areas.Mobile.Controllers
{
    public class JoinController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            return View();
        }
        public void Add(ApplyJoin ApplyJoin, string code)
        {
            if (code.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("验证码错误");
                return;
            }
            Session.ClearCheckCode();
            ApplyJoin.State = 1;
            ApplyJoin.AddTime = DateTime.Now;
            ApplyJoin.AgentId = BasicUsers.Agent;
            ApplyJoin.AgentAId = BasicUsers.AId;
            SysAgent SysAgent = Entity.SysAgent.FirstOrNew(f => f.Id == BasicUsers.Agent);
            ApplyJoin.AgentName = SysAgent.Name;
            SysAgent TopSysAgent = SysAgent.GetTopAgent(Entity);
            if (TopSysAgent.IsTeiPai == 1)
            {
                ApplyJoin.TiePaiAgentId = TopSysAgent.Id;
                ApplyJoin.TiePaiAgentName = TopSysAgent.Name;
            }
            Entity.ApplyJoin.AddObject(ApplyJoin);
            Entity.SaveChanges();
            Response.Redirect("Success.html");
        }
        public ActionResult Success()
        {
            return View();
        }
    }
}
