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
namespace LokFu.Areas.Manage.Controllers
{
    public class UserPayChangeController : BaseController
    {

        public ActionResult Index(UserPayChange UserPayChange, EFPagingInfo<UserPayChange> p, bool? IsShowSupAgent, int IsFirst = 0)
        {
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            if (IsFirst == 0)
            {
                PageOfItems<UserPayChange> UserPayChangeList1 = new PageOfItems<UserPayChange>(new List<UserPayChange>(), 0, 10, 0, new Hashtable());
                ViewBag.UserPayChangeList = UserPayChangeList1;
                ViewBag.UserPayChange = UserPayChange;
                ViewBag.UsersList = null;
                ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1).ToList();
                ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
                ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级
               
                return View();
            }
            //if (UserPayChange.State.IsNullOrEmpty())
            //{
            //    UserPayChange.State = 1;
            //}
            //if (UserPayChange.State == 99)
            //{
            //    UserPayChange.State = 0;
            //}
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
            if (!UserPayChange.SId.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == UserPayChange.SId).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
                    IList<int> UID = new List<int>();
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.SId));
                }
                else
                {
                    p.SqlWhere.Add(f => f.SId == UserPayChange.SId);
                }
            }
            if (!UserPayChange.SAId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.SAId == UserPayChange.SAId); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<UserPayChange> UserPayChangeList = null;
            if (IsFirst == 0)
            {
                UserPayChangeList = new PageOfItems<UserPayChange>(new List<UserPayChange>(), 0, 10, 0, new Hashtable());
            }
            else
            { 
                UserPayChangeList = Entity.Selects<UserPayChange>(p);
            }
            ViewBag.UserPayChangeList = UserPayChangeList;
            ViewBag.UserPayChange = UserPayChange;
            IList<UserPayChange> List = UserPayChangeList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(UserPayChange UserPayChange)
        {
            UserPayChange = Entity.UserPayChange.FirstOrDefault(n => n.Id == UserPayChange.Id);
            if (UserPayChange == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.UserPayChange = UserPayChange;
            ViewBag.Users = Entity.Users.FirstOrNew(n => n.Id == UserPayChange.UId);
            ViewBag.AgentAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == UserPayChange.SAId);
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == UserPayChange.SId);
            ViewBag.SysAdmin = Entity.SysAdmin.FirstOrNew(n => n.Id == UserPayChange.AId);
            ViewBag.UserPayTempList = Entity.UserPayTemp.Where(n => n.UPCId == UserPayChange.Id).ToList();
            ViewBag.UserPayList = Entity.UserPay.Where(n => n.UId == UserPayChange.UId).ToList();
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(UserPayChange UserPayChange)
        {
            UserPayChange baseUserPayChange = Entity.UserPayChange.FirstOrDefault(n => n.Id == UserPayChange.Id);
            if (UserPayChange.State == 2)
            { //审核
                Users Users = Entity.Users.FirstOrDefault(n => n.Id == baseUserPayChange.UId);
                Users.Cash0 = baseUserPayChange.CashNew0;
                Users.ECash0 = baseUserPayChange.ECashNew0;
                Users.Cash1 = baseUserPayChange.CashNew1;
                Users.ECash1 = baseUserPayChange.ECashNew1;
                IList<UserPayTemp> TempList = Entity.UserPayTemp.Where(n => n.UPCId == baseUserPayChange.Id).ToList();
                IList<UserPay> PayList = Entity.UserPay.Where(n => n.UId == baseUserPayChange.UId).ToList();
                foreach (var p in PayList)
                {
                    UserPayTemp Temp = TempList.FirstOrDefault(n => n.PId == p.PId);
                    if (Temp != null)
                    {
                        p.Cost = Temp.Cost;
                    }
                }
            }
            baseUserPayChange.State = UserPayChange.State;
            baseUserPayChange.EditRemark = UserPayChange.EditRemark;
            baseUserPayChange.AId = AdminUser.Id;
            baseUserPayChange.EditTime = DateTime.Now;
            Entity.SaveChanges();
            BaseRedirect();
        }
    }
}
