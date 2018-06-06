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
    public class SysAdminController : BaseController
    {
     
        public ActionResult Index(SysAdmin SysAdmin, EFPagingInfo<SysAdmin> p, int IsFirst=0)
        {
            if (IsFirst==0)
            {
                PageOfItems<SysAdmin> SysAdminList1 = new PageOfItems<SysAdmin>(new List<SysAdmin>(), 0, 10, 0, new Hashtable());
                ViewBag.SysAdminList = SysAdminList1;
                ViewBag.SysAdmin = SysAdmin;
                ViewBag.Save = this.checkPower("Save");
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Info = this.checkPower("Info");
                return View();
            }
            p.SqlWhere.Add(f => f.AgentId == BasicAgent.Id);//只能管理本机构下管理员
            if (!SysAdmin.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == SysAdmin.State); }
            if (!SysAdmin.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName == SysAdmin.UserName); }
            if (!SysAdmin.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName == SysAdmin.TrueName); }
            if (!SysAdmin.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile == SysAdmin.Mobile); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<SysAdmin> SysAdminList = Entity.Selects<SysAdmin>(p);
            ViewBag.SysAdminList = SysAdminList;
            ViewBag.SysAdmin = SysAdmin;
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Info = this.checkPower("Info");
            return View();
        }
        public ActionResult Info(SysAdmin SysAdmin)
        {
            //不能修改自己和代理操作员
            if (SysAdmin.Id == BasicAgent.AdminId || SysAdmin.Id == AdminUser.Id)
            {
                ViewBag.ErrorMsg = "代理商和当前用户不能编辑";
                return View("Error");
            }
            if (SysAdmin.Id != 0) SysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == SysAdmin.Id && n.AgentId == BasicAgent.Id);
            if (SysAdmin == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            ViewBag.SysAdmin = SysAdmin;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        public ActionResult Edit(SysAdmin SysAdmin)
        {
            //不能修改自己和代理操作员
            if (SysAdmin.Id == BasicAgent.AdminId || SysAdmin.Id == AdminUser.Id)
            {
                ViewBag.ErrorMsg = "代理商和当前用户不能编辑";
                return View("Error");
            }
            if (SysAdmin.Id != 0) SysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == SysAdmin.Id && n.AgentId == BasicAgent.Id);
            if (SysAdmin == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            ViewBag.SysAdmin = SysAdmin;
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
            SysAdmin.AgentId = BasicAgent.Id;
            SysAdmin.LoginTimes = 0;
            SysAdmin.AddTime = DateTime.Now;
            SysAdmin.PassWord = SysAdmin.PassWord.GetAdminMD5();
            //Lin 20160318 升级成多级代理商模式，去掉
            SysAdmin.PayGet = 0;//SysAdmin.PayGet / 100;
            Entity.SysAdmin.AddObject(SysAdmin);
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
            SysAdmin baseSysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == SysAdmin.Id && n.AgentId == BasicAgent.Id);
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
            //if (!SysAdmin.PayGet.IsNullOrEmpty())
            //{
            //    baseSysAdmin.PayGet = SysAdmin.PayGet / 100;
            //}
            //Lin 20160318 升级成多级代理商模式，去掉
            SysAdmin.PayGet = 0;
            Entity.SaveChanges();
            BaseRedirect();
        }
    }
}
