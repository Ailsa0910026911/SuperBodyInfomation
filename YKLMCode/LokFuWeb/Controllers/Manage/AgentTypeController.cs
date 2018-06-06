using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class AgentTypeController : BaseController
    {
       public ActionResult Index(AgentType AgentType, EFPagingInfo<AgentType> p)
       {
            if(!AgentType.Name.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Name.Contains(AgentType.Name));}
            //if(!AgentType.AgentID.IsNullOrEmpty()){p.SqlWhere.Add(f => f.AgentID==AgentType.AgentID);}
            if(!AgentType.State.IsNullOrEmpty()){p.SqlWhere.Add(f => f.State==AgentType.State);}
            p.OrderByList.Add("Id", "DESC");
            p.SqlWhere.Add(f => f.AgentID == 0);
            IPageOfItems<AgentType> AgentTypeList = Entity.Selects<AgentType>(p);
            ViewBag.AgentTypeList = AgentTypeList;
            ViewBag.AgentType = AgentType;
            return View();
       }
       public ActionResult Edit(AgentType AgentType)
       {
            if(AgentType.Id != 0) AgentType = Entity.AgentType.FirstOrDefault(n => n.Id == AgentType.Id);
            if (AgentType == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.AgentType = AgentType;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
       }
        [ValidateInput(false)]
        public void Add(AgentType AgentType)
        {
            AgentType.AddTime = DateTime.Now;
            AgentType.AgentID = 0;
            AgentType.RegisterPayGet = AgentType.RegisterPayGet / 100;
            Entity.AgentType.AddObject(AgentType);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(AgentType AgentType)
        {
            AgentType baseAgentType = Entity.AgentType.FirstOrDefault(n => n.Id == AgentType.Id);
            baseAgentType = Request.ConvertRequestToModel<AgentType>(baseAgentType, AgentType);
            baseAgentType.RegisterPayGet = baseAgentType.RegisterPayGet / 100;
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(AgentType AgentType, string InfoList,string Clomn,string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = AgentType.Id.ToString(); }
            int Ret = Entity.ChangeEntity<AgentType>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(AgentType AgentType, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)){ InfoList = AgentType.Id.ToString();}
            int Ret = Entity.MoveToDeleteEntity<AgentType>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
