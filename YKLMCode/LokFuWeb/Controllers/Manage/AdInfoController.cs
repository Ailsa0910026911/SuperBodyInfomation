using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class AdInfoController : BaseController
    {
       
        public ActionResult Index(AdInfo AdInfo, EFPagingInfo<AdInfo> p, int IsFirst = 0)
        {
            if (IsFirst==0)
            {
                PageOfItems<AdInfo> AdInfoList1 = new PageOfItems<AdInfo>(new List<AdInfo>(), 0, 10, 0, new Hashtable());
                ViewBag.AdInfoList = AdInfoList1;
                ViewBag.AdInfo = AdInfo;
                ViewBag.AdTagList = Entity.AdTag.Where(n => n.State == 1).ToList();
                //贴牌代理
                var SysAgentList1 = Entity.SysAgent.Where(o => o.IsTeiPai == 1 && o.State == 1).ToList();
                var haofusysagent1 = new SysAgent()
                {
                    Id = 0,
                    Name = "好付",
                    AppBtnNumber = BasicSet.AppBtnNumber,
                    APPHasMore = BasicSet.APPHasMore,
                    APPName = BasicSet.Name,
                };
                SysAgentList1.Add(haofusysagent1);
                SysAgentList1 = SysAgentList1.OrderBy(o => o.Id).ToList();
                ViewBag.SysAgentList = SysAgentList1;
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!AdInfo.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(AdInfo.Name)); }
            if (!AdInfo.TId.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.TId == AdInfo.TId);
                p.OrderByList.Add("Sort", "DESC");
            }
            else
            {
                p.OrderByList.Add("Id", "DESC");
            }
            if (!AdInfo.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (AdInfo.State == 99 ? 0 : AdInfo.State)); }
            p.SqlWhere.Add(f => f.AgentId == AdInfo.AgentId);
            IPageOfItems<AdInfo> AdInfoList = Entity.Selects<AdInfo>(p);
            ViewBag.AdInfoList = AdInfoList;
            ViewBag.AdInfo = AdInfo;
            ViewBag.AdTagList = Entity.AdTag.Where(n => n.State == 1).ToList();
            //贴牌代理
            var SysAgentList = Entity.SysAgent.Where(o => o.IsTeiPai == 1 && o.State == 1).ToList();
            var haofusysagent = new SysAgent()
            {
                Id = 0,
                Name = "好付",
                AppBtnNumber = BasicSet.AppBtnNumber,
                APPHasMore = BasicSet.APPHasMore,
                APPName = BasicSet.Name,
            };
            SysAgentList.Add(haofusysagent);
            SysAgentList = SysAgentList.OrderBy(o => o.Id).ToList();
            ViewBag.SysAgentList = SysAgentList;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(AdInfo AdInfo)
        {
            if (AdInfo.Id != 0) { AdInfo = Entity.AdInfo.FirstOrDefault(n => n.Id == AdInfo.Id); }
            if (AdInfo == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.AdInfo = AdInfo;
            ViewBag.AdTagList = Entity.AdTag.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            string filename = HttpContext.Server.MapPath("/ModuleTypeSelectList.json");
            string jsonstr = System.IO.File.ReadAllText(filename);
            var ModuleTypeSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(jsonstr);
            ViewBag.ModuleTypeSelectList = ModuleTypeSelectList;
            return View();
        }
        [ValidateInput(false)]
        public void Add(AdInfo AdInfo)
        {
            AdInfo = Request.ConvertRequestToModel<AdInfo>(AdInfo, AdInfo);
            Entity.AdInfo.AddObject(AdInfo);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(AdInfo AdInfo)
        {
            AdInfo baseAdInfo = Entity.AdInfo.FirstOrDefault(n => n.Id == AdInfo.Id);
            baseAdInfo = Request.ConvertRequestToModel<AdInfo>(baseAdInfo, AdInfo);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(AdInfo AdInfo, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = AdInfo.Id.ToString(); }
            int Ret = Entity.ChangeEntity<AdInfo>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(AdInfo AdInfo, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = AdInfo.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<AdInfo>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
