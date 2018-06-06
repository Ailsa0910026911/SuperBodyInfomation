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
namespace LokFu.Areas.Agent.Controllers
{
    public class AgentAdminController : BaseController
    {

       
        public ActionResult Index(SysAdmin SysAdmin, EFPagingInfo<SysAdmin> p, int IsFirst = 0)
        {
            IPageOfItems<SysAdmin> SysAdminList;
            if (IsFirst==0)
            {
               SysAdminList = new PageOfItems<SysAdmin>(new List<SysAdmin>(), 0, 10, 0, new Hashtable());
                ViewBag.SysAdminList = SysAdminList;
                ViewBag.SysAdmin = SysAdmin;
                ViewBag.SysAgentList = Entity.SysAgent.ToList();
                return View();
            }
            p.SqlWhere.Add(f => f.AgentId > 0);
            if (!SysAdmin.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == SysAdmin.State); }
            if (!SysAdmin.AgentId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AgentId == SysAdmin.AgentId); }
            if (!SysAdmin.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName == SysAdmin.UserName); }
            if (!SysAdmin.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName == SysAdmin.TrueName); }
            if (!SysAdmin.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile == SysAdmin.Mobile); }
            p.OrderByList.Add("Id", "DESC");
            SysAdminList = Entity.Selects<SysAdmin>(p);
            ViewBag.SysAdminList = SysAdminList;
            ViewBag.SysAdmin = SysAdmin;
            ViewBag.SysAgentList = Entity.SysAgent.ToList();
            return View();
        }
        public ActionResult Edit(SysAdmin SysAdmin)
        {
            if (SysAdmin.Id != 0) SysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == SysAdmin.Id);
            ViewBag.SysAdmin = SysAdmin;
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.AgentID == BasicAgent.Id).ToList();
            //ViewBag.SysPowerList = Entity.SysPower.Where(n => n.PType == 2 && n.State == 1).OrderBy(n => n.Sort).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public object Add(SysAdmin SysAdmin, List<string> PId)
        {
            //验证是否重复
            SysAdmin Old = Entity.SysAdmin.FirstOrDefault(n => n.UserName == SysAdmin.UserName);
            if (Old != null)
            {
                ViewBag.ErrorMsg = "“登录帐户”已存在，请重新输入！";
                return View("Error");
            }
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == SysAdmin.AgentId && n.State == 1);
            if (SysAgent == null) {
                ViewBag.ErrorMsg = "所属于机构不存在或异常！";
                return View("Error");
            }
            string Str = string.Empty;
            if (PId != null)
            {
                foreach (var p in PId)
                {
                    if (p != string.Empty)
                    {
                        Str += "," + p;
                    }
                }
                Str += ",";
            }
            SysAdmin.PowerID = Str;
            SysAdmin.LoginTimes = 0;
            SysAdmin.AddTime = DateTime.Now;
            SysAdmin.PassWord = SysAdmin.PassWord.GetAdminMD5();
            Entity.SysAdmin.AddObject(SysAdmin);
            Entity.SaveChanges();
            if (SysAgent.AdminId.IsNullOrEmpty()) { //绑定第一个管理员
                SysAgent.AdminId = SysAdmin.Id;
            }
            Entity.SaveChanges();
            BaseRedirect();
            return true;
        }
        [ValidateInput(false)]
        public void Save(SysAdmin SysAdmin, List<string> PId)
        {
            string Str = string.Empty;
            if (PId != null)
            {
                foreach (var p in PId)
                {
                    if (p != string.Empty)
                    {
                        Str += "," + p;
                    }
                }
                Str += ",";
            }
            SysAdmin baseSysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == SysAdmin.Id);
            if (SysAdmin.PassWord.IsNullOrEmpty())
            {
                SysAdmin.PassWord = baseSysAdmin.PassWord;
            }
            else
            {
                SysAdmin.PassWord = SysAdmin.PassWord.GetAdminMD5();
            }
            baseSysAdmin = Request.ConvertRequestToModel<SysAdmin>(baseSysAdmin, SysAdmin);
            baseSysAdmin.PowerID = Str;
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(SysAdmin SysAdmin, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = SysAdmin.Id.ToString(); }
            int Ret = Entity.ChangeEntity<SysAdmin>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(SysAdmin SysAdmin, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = SysAdmin.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<SysAdmin>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
