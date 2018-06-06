using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class UserPromoteGetController : BaseController
    {
        public ActionResult Index(UserPromoteGet UserPromoteGet, EFPagingInfo<UserPromoteGet> p)
        {
            int GlobaPromoteMaxLevel = BasicSet.GlobaPromoteMaxLevel;
            var UserPromoteGetList = Entity.UserPromoteGet.Where(o => o.AgentID == this.BasicAgent.Id).ToList();
            if (UserPromoteGetList.Count > GlobaPromoteMaxLevel)
            {
                ViewBag.ErrorMsg = "参数错误,请联系客服.";
                return View("Error");
            }
            if (UserPromoteGetList.Count == 0)
            {
                for (int i = 1; i <= GlobaPromoteMaxLevel; i++)
                {
                    UserPromoteGetList.Add(new UserPromoteGet() { PromoteLevel = (byte)i, State = 1 });
                }
            }
            //int max = GlobaPromoteMaxLevel - UserPromoteGetList.Count;
            //for (int i = 1; i <= max; i++)
            //{
            //    UserPromoteGetList.Add(new UserPromoteGet() { PromoteLevel = (byte)i, State = 1 });
            //}
            ViewBag.UserPromoteGetList = UserPromoteGetList;
            return View();
        }
        [ValidateInput(false)]
        public ActionResult Save(List<UserPromoteGet> model, double promoteGet, byte isPromote, byte Set4 = 0)
        {
            if (model.Sum(o => o.PromoteGet) != 100)
            {
                ViewBag.ErrorMsg = "分润百分比超过100%,请重新填写";
                return View("Error");
            }
            if (model.Count > BasicSet.GlobaPromoteMaxLevel)
            {
                ViewBag.ErrorMsg = "上传参数错误,请联系客服.";
                return View("Error");
            }
            var UserPromoteGetList = Entity.UserPromoteGet.Where(o => o.AgentID == this.BasicAgent.Id).ToList();
            if (UserPromoteGetList.Count > BasicSet.GlobaPromoteMaxLevel)
            {
                ViewBag.ErrorMsg = "后台参数错误,请联系客服.";
                return View("Error");
            }

            var submitCount = model.Where(o => o.Id == 0).Count();
            if (UserPromoteGetList.Count + submitCount > BasicSet.GlobaPromoteMaxLevel)
            {
                ViewBag.ErrorMsg = "上传参数过多,请联系客服.";
                return View("Error");
            }

            foreach(var item in model)
            {
                UserPromoteGet baseUserPromoteGet;
                if (item.Id.IsNullOrEmpty())
                {
                    item.AddTime = DateTime.Now;
                    item.AgentID = this.BasicAgent.Id;
                    item.PromoteGet = item.PromoteGet / 100;
                    Entity.UserPromoteGet.AddObject(item);
                }
                else
                {
                    baseUserPromoteGet = UserPromoteGetList.FirstOrDefault(n => n.Id == item.Id);
                    baseUserPromoteGet.PromoteGet = item.PromoteGet / 100;
                    baseUserPromoteGet.PromoteLevel = item.PromoteLevel;
                    baseUserPromoteGet.State = item.State;
                }
            }
            BasicAgent.PromoteGet = promoteGet / 100;
            BasicAgent.IsPromote = isPromote;
            if (BasicAgent.IsTeiPai == 1 && BasicAgent.Tier == 1)
            {
                BasicAgent.Set4 = Set4;
            }
            ViewBag.IsColse = true;
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功";
            return View("Succeed");
            //return this.RedirectToAction("index");
        }
    }
}
