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
    public class UserPayChangeController : BaseController
    {
       
        public ActionResult Index(UserPayChange UserPayChange, EFPagingInfo<UserPayChange> p,int IsFirst=0)
        {
            if (IsFirst==0)
            {
                PageOfItems<UserPayChange> UserPayChangeList1 = new PageOfItems<UserPayChange>(new List<UserPayChange>(), 0, 10, 0, new Hashtable());
                ViewBag.UserPayChangeList = UserPayChangeList1;
                ViewBag.UserPayChange = UserPayChange;
                ViewBag.UsersList = new List<Users>();
                ViewBag.Info = this.checkPower("Info");
                return View();
            }
            //用户名搜索
            if (!UserPayChange.Remark.IsNullOrEmpty())
            {
                IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(UserPayChange.Remark) || n.NeekName.Contains(UserPayChange.Remark) || n.UserName == UserPayChange.Remark).ToList();
                List<int> UIds = new List<int>();
                foreach (var pp in UList)
                {
                    UIds.Add(pp.Id);
                }
                p.SqlWhere.Add(f => UIds.Contains(f.UId));
            }

            if (!UserPayChange.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == UserPayChange.State); }
            p.SqlWhere.Add(f => f.SId == BasicAgent.Id);
            p.SqlWhere.Add(f => f.SAId == AdminUser.Id);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<UserPayChange> UserPayChangeList = Entity.Selects<UserPayChange>(p);
            ViewBag.UserPayChangeList = UserPayChangeList;
            ViewBag.UserPayChange = UserPayChange;
            //ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && n.Agent == AdminUser.AgentId).ToList();
            IList<UserPayChange> List = UserPayChangeList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.Info = this.checkPower("Info");
            return View();
        }
        public ActionResult Info(UserPayChange UserPayChange)//查看申请及结果
        {
            UserPayChange = Entity.UserPayChange.FirstOrDefault(n => n.Id == UserPayChange.Id);
            if (UserPayChange == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(UserPayChange.SId))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            ViewBag.UserPayChange = UserPayChange;
            Users Users = Entity.Users.FirstOrNew(n => n.Id == UserPayChange.UId);
            ViewBag.Users = Users;
            ViewBag.AgentAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == UserPayChange.SAId);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == UserPayChange.AId);
            ViewBag.UserPayTempList = Entity.UserPayTemp.Where(n => n.UPCId == UserPayChange.Id).ToList();
            ViewBag.UserPayList = Entity.UserPay.Where(n => n.UId == UserPayChange.UId).ToList();
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent);
            return View();
        }
        [ValidateInput(false)]
        public void Add(Users Users, int[] PId, double[] Cost)
        {
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Id == Users.Id && n.Agent == BasicAgent.Id);
            bool Check = true;
            if (baseUsers != null)
            {
                Users.Cash0 = Users.Cash0 / 1000;
                if (Users.Cash0 >= BasicSet.AgentCash0 && Users.ECash0 >= BasicSet.AgentECash0 && Users.Cash1 >= BasicSet.AgentCash1 && Users.ECash1 >= BasicSet.AgentECash1)
                {
                    baseUsers.Cash0 = Users.Cash0;
                    baseUsers.ECash0 = Users.ECash0;
                    baseUsers.Cash1 = Users.Cash1;
                    baseUsers.ECash1 = Users.ECash1;
                    baseUsers.State = 1;
                }
                else {
                    Check = false;
                }
                for (int i = 0; i < PId.Length; i++)
                {
                    int pid = PId[i];
                    double cost = Cost[i] / 1000;
                    PayConfig PC = Entity.PayConfig.FirstOrDefault(n => n.Id == pid);
                    if (PC == null) {
                        Check = false;
                    }
                    if (cost >= PC.CostAgent)
                    {
                        UserPay UserPay = new UserPay();
                        UserPay.UId = baseUsers.Id;
                        UserPay.PId = pid;
                        UserPay.Cost = cost;
                        Entity.UserPay.AddObject(UserPay);
                    }
                    else {
                        Check = false;
                    }
                }
            }
            //Entity.UserPayChange.AddObject(UserPayChange);
            if (Check)
            {
                Entity.SaveChanges();
                BaseRedirect();
            }
            else {
                Response.Redirect("/Agent/home/error.html?IsAjax=" + Request["IsAjax"] + "&msg=费率设置有误~");
            }
        }
        public ActionResult Edit(UserPayChange UserPayChange)//变更申请
        {
            Users Users = null;
            if (BasicAgent.Tier == 1)
            {
                Users = Entity.Users.FirstOrDefault(n => n.Id == UserPayChange.UId);
            }
            else
            {
                Users = Entity.Users.FirstOrDefault(n => n.Id == UserPayChange.UId && n.Agent == BasicAgent.Id);
            }
            
            if (Users == null)
            {
                ViewBag.ErrorMsg = "获取用户信息错误！";
                return View("Error");
            }
            if (!IsBelongToAgent(Users.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            if (Users.State != 1)
            {
                ViewBag.ErrorMsg = "用户未审核！";
                return View("Error");
            }
            int UPC = Entity.UserPayChange.Count(n => n.UId == Users.Id && n.State == 1);
            if (UPC > 0) {
                ViewBag.ErrorMsg = "当前用户有未处理完成申请，请不要重复申请！";
                return View("Error");
            }
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.UserPayList = Entity.UserPay.Where(n => n.UId == Users.Id).ToList();
            ViewBag.Users = Users;
            return View();
        }
        [ValidateInput(false)]
        public ActionResult Save(UserPayChange UserPayChange, int[] PId, double[] Cost)
        {
            Users baseUsers = null;
            if (BasicAgent.Tier == 1)
            {
                baseUsers = Entity.Users.FirstOrDefault(n => n.Id == UserPayChange.UId);
            }
            else
            {
                baseUsers = Entity.Users.FirstOrDefault(n => n.Id == UserPayChange.UId && n.Agent == BasicAgent.Id);
            }
            if (!IsBelongToAgent(baseUsers.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            int UPC = Entity.UserPayChange.Count(n => n.UId == baseUsers.Id && n.State == 1);
            if (UPC > 0)
            {
                //Response.Redirect("/Agent/home/error.html?IsAjax=" + Request["IsAjax"] + "&msg=当前用户有未处理完成申请，请不要重复申请！");
                ViewBag.ErrorMsg = "当前用户有未处理完成申请，请不要重复申请！";
                return View("Error");
            }
            else
            {
                bool Check = true;
                if (baseUsers != null)
                {
                    UserPayChange.CashNew0 = UserPayChange.CashNew0 / 1000;
                    UserPayChange.CashNew1 = UserPayChange.CashNew1 / 1000;
                    if (UserPayChange.CashNew0 < BasicSet.AgentCash0 || UserPayChange.ECashNew0 < BasicSet.AgentECash0 || UserPayChange.CashNew1 < BasicSet.AgentCash1 || UserPayChange.ECashNew1 < BasicSet.AgentECash1)
                    {
                        Check = false;
                    }
                    UserPayChange = Request.ConvertRequestToModel<UserPayChange>(UserPayChange, UserPayChange);
                    UserPayChange.AddTime = DateTime.Now;
                    UserPayChange.SId = BasicAgent.Id;
                    UserPayChange.SAId = AdminUser.Id;
                    UserPayChange.State = 1;
                    UserPayChange.Cash0 = baseUsers.Cash0;
                    UserPayChange.ECash0 = baseUsers.ECash0;
                    UserPayChange.Cash1 = baseUsers.Cash1;
                    UserPayChange.ECash1 = baseUsers.ECash1;
                    Entity.UserPayChange.AddObject(UserPayChange);
                    Entity.SaveChanges();
                    int upcId = UserPayChange.Id;
                    for (int i = 0; i < PId.Length; i++)
                    {
                        int pid = PId[i];
                        double cost = Cost[i] / 1000;
                        PayConfig PC = Entity.PayConfig.FirstOrDefault(n => n.Id == pid);
                        if (PC == null)
                        {
                            Check = false;
                        }
                        if (cost >= PC.CostAgent)
                        {
                            UserPayTemp UserPay = new UserPayTemp();
                            UserPay UP = Entity.UserPay.FirstOrNew(n => n.PId == PC.Id);
                            UserPay.UPCId = upcId;
                            UserPay.UId = baseUsers.Id;
                            UserPay.PId = pid;
                            UserPay.Cost = cost;
                            UserPay.ACost = UP.Cost;
                            Entity.UserPayTemp.AddObject(UserPay);
                        }
                        else
                        {
                            Check = false;
                        }
                    }
                }
                //Entity.UserPayChange.AddObject(UserPayChange);
                if (Check)
                {
                    Entity.SaveChanges();
                    return this.View("ReloadFrame");
                    //BaseRedirect();
                }
                else
                {
                    Entity.UserPayChange.DeleteObject(UserPayChange);
                    Entity.SaveChanges();
                    //Response.Redirect("/Agent/home/error.html?IsAjax=" + Request["IsAjax"] + "&msg=费率设置有误~");
                    ViewBag.ErrorMsg = "费率设置有误！";
                    return View("Error");
                }
            }
        }
    }
}
