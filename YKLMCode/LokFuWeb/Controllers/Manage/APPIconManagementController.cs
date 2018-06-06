using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace LokFu.Areas.Manage.Controllers
{
    public class APPIconManagementController : BaseController
    {
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="APPModule"></param>
        /// <param name="p"></param>
        /// <param name="IsFirst"></param>
        /// <returns></returns>
        public ActionResult Index(APPModule APPModule, EFPagingInfo<APPModule> p, int IsFirst = 0)
        {
            //条件
            p.SqlWhere.Add(f => f.Version == 1);

            p.SqlWhere.Add(f => f.AgentId == APPModule.AgentId);
            if (!APPModule.DisplaySite.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.DisplaySite == APPModule.DisplaySite); }
            //排序
            p.OrderByList.Add("Sort", "ASC");
            IPageOfItems<APPModule> APPModuleList = null;
            if (IsFirst == 0)
            {
                APPModuleList = new PageOfItems<APPModule>(new List<APPModule>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                APPModuleList = Entity.Selects<APPModule>(p);
            }

            ViewBag.APPModuleList = APPModuleList;
            ViewBag.APPModule = APPModule;
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
            //加载类型选项
            string filename = HttpContext.Server.MapPath("/ModuleTypeSelectList.json");
            string jsonstr = System.IO.File.ReadAllText(filename);
            var ModuleTypeSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(jsonstr);
            ViewBag.ModuleTypeSelectList = ModuleTypeSelectList;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Delete = this.checkPower("Delete");
            return View();
        }
        public ActionResult Edit(APPModule APPModule)
        {
            if (APPModule.Id != 0) APPModule = Entity.APPModule.FirstOrDefault(n => n.Id == APPModule.Id);
            if (APPModule == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
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

            //加载类型选项
            string filename = HttpContext.Server.MapPath("/ModuleTypeSelectList.json");
            string jsonstr = System.IO.File.ReadAllText(filename);
            var ModuleTypeSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(jsonstr);
            ViewBag.ModuleTypeSelectList = ModuleTypeSelectList;
            string Bottomfilename = HttpContext.Server.MapPath("/ModuleTypeBottomSelectList.json");
            string Bottomjsonstr = System.IO.File.ReadAllText(Bottomfilename);
            var ModuleTypeBottomSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(Bottomjsonstr);
            ViewBag.ModuleTypeBottomSelectList = ModuleTypeBottomSelectList;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            this.ViewBag.APPModule = APPModule;
            return View();
        }
        public ActionResult Info(int id)
        {
            var baseAPPModule = Entity.APPModule.FirstOrDefault(n => n.Id == id);
            if (baseAPPModule == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }

            ViewBag.APPModule = baseAPPModule;
            if (baseAPPModule.AgentId == 0)
            {
                var haofusysagent = new SysAgent()
                {
                    Id = 0,
                    Name = "好付",
                    AppBtnNumber = BasicSet.AppBtnNumber,
                    APPHasMore = BasicSet.APPHasMore,
                    APPName = BasicSet.Name,
                };
                ViewBag.SysAgent = haofusysagent;
            }
            else
            {
                ViewBag.SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == baseAPPModule.AgentId);
            }
            //加载类型选项
            string filename = HttpContext.Server.MapPath("/ModuleTypeSelectList.json");
            string jsonstr = System.IO.File.ReadAllText(filename);
            var ModuleTypeSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(jsonstr);
            ViewBag.ModuleTypeSelectList = ModuleTypeSelectList;
            string Bottomfilename = HttpContext.Server.MapPath("/ModuleTypeBottomSelectList.json");
            string Bottomjsonstr = System.IO.File.ReadAllText(Bottomfilename);
            var ModuleTypeBottomSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(Bottomjsonstr);
            ViewBag.ModuleTypeBottomSelectList = ModuleTypeBottomSelectList;

            return View();
        }
        [ValidateInput(false)]
        public ActionResult Add(APPModule APPModule)
        {
            APPModule = Request.ConvertRequestToModel<APPModule>(APPModule, APPModule);
            APPModule.AddTime = DateTime.Now;
            APPModule.Version = 1;
            Entity.APPModule.AddObject(APPModule);
            this.UpdateVersionAll(APPModule.AgentId);
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功";
            return View("Succeed");
        }

        [ValidateInput(false)]
        public ActionResult Save(APPModule APPModule)
        {
            APPModule baseAPPModule = Entity.APPModule.FirstOrDefault(n => n.Id == APPModule.Id);
            baseAPPModule = Request.ConvertRequestToModel<APPModule>(baseAPPModule, APPModule);
            this.UpdateVersionAll(baseAPPModule.AgentId);
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功";
            return View("Succeed");
        }
        private bool UpdateVersionAll(int AgentId)
        {
            if (AgentId == 0)
            {
                //更新版本
                this.BasicSet.AppMenuHome++;
                this.BasicSet.AppMenuBottom++;
                this.BasicSet.AppMenuMore++;
                return true;
            }
            else
            {
                SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(o => o.Id == AgentId);
                if (SysAgent != null)
                {
                    //更新版本
                    SysAgent.AppMenuHome++;
                    SysAgent.AppMenuBottom++;
                    SysAgent.AppMenuMore++;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void ChangeStatus(APPModule APPModule, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = APPModule.Id.ToString(); }
            int Ret = Entity.ChangeEntity<APPModule>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(APPModule APPModule, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = APPModule.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<APPModule>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
