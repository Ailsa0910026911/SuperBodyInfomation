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
    public class BaoUsersController : BaseController
    {
        public ActionResult Index(BaoUsers BaoUsers, EFPagingInfo<BaoUsersVM> p, string UserName, int? AgentID, bool? HasMoney, int IsFirst = 0, bool IsShowSupAgent = false)
        {
            int id = 0;
            var iquery = Entity.BaoUsers.Join(this.Entity.Users, b => b.UId, u => u.Id, (b, u) => new BaoUsersVM() { Users = u, BaoUsers = b });
            if (!UserName.IsNullOrEmpty())
            {
                iquery = iquery.Where(o => o.Users.UserName == UserName);
            }
            if (!BaoUsers.PayPwd.IsNullOrEmpty())
            {
                id = this.Entity.Users.FirstOrNew(o => o.CardId == BaoUsers.PayPwd).Id;
                if (id != 0)
                {
                    iquery = iquery.Where(o => o.BaoUsers.UId == id);
                }
            }
            if(HasMoney.HasValue)
            {
                if (HasMoney.Value)
                {
                    iquery = iquery.Where(o => o.BaoUsers.AllMoney > 0);
                }
                else
                {
                    iquery = iquery.Where(o => o.BaoUsers.AllMoney == 0);
                }
            }
            
            if (AgentID.HasValue)
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == AgentID.Value).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity, true);
                    IList<int> UID = SysAgentList.Select(o=>o.Id).ToList();
                    iquery = iquery.Where(f => UID.Contains(f.Users.Agent));
                }
                else
                {
                    iquery = iquery.Where(f => f.Users.Agent == AgentID.Value);
                }
            }
            PageOfItems<BaoUsersVM> BaoUsersList = null;
            decimal sumAllMoney = 0m;
            decimal sumInMoney = 0m;
            if (IsFirst == 0)
            {
                BaoUsersList = new PageOfItems<BaoUsersVM>(new List<BaoUsersVM>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                var count = iquery.Count();
                sumAllMoney = iquery.Sum(o => (decimal?)o.BaoUsers.AllMoney) ?? 0;
                sumInMoney = iquery.Sum(o => (decimal?)o.BaoUsers.InMoney) ?? 0;
                List<BaoUsersVM> a = iquery.OrderByDescending(o => o.BaoUsers.AllMoney).Skip(p.PageIndex < 1 ? 0 : ((p.PageIndex - 1) * p.PageSize)).Take(p.PageSize).ToList();
                BaoUsersList = new PageOfItems<BaoUsersVM>(a, p.PageIndex, p.PageSize, count, p.OrderByList);
            }

            ViewBag.BaoUsersList = BaoUsersList;
            ViewBag.BaoUsers = BaoUsers;

            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级
            ViewBag.AgentID = AgentID;
            ViewBag.UserName = UserName;

            ViewBag.sumAllMoney = sumAllMoney;
            ViewBag.sumInMoney = sumInMoney;
            ViewBag.HasMoney = HasMoney;
            ViewBag.Log = this.checkPower("Log");
            ViewBag.TransferUser = this.checkPower("TransferUser");
            return View();
        }
        public ActionResult Log(BaoLog BaoLog, EFPagingInfo<BaoLog> p)
        {
            if (!BaoLog.UId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UId == BaoLog.UId); }
            if (!BaoLog.LType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.LType == BaoLog.LType); }
            if (!BaoLog.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == BaoLog.State); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BaoLog> BaoLogList = Entity.Selects<BaoLog>(p);
            ViewBag.BaoLogList = BaoLogList;
            ViewBag.BaoLog = BaoLog;
            return View();
        }

        public ActionResult TransferUser(int id)
        {
            var BaoUsers = this.Entity.BaoUsers.FirstOrDefault(o=>o.Id == id);
            if (BaoUsers == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (BaoUsers.AllMoney == 0)
            {
                ViewBag.ErrorMsg = "总金额为0不能转出到余额";
                return View("Error");
            }
            var Users = this.Entity.Users.FirstOrDefault(o => o.Id == BaoUsers.UId);
            if (Users == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            var AllMoney = BaoUsers.AllMoney;
            var BaoLog = new BaoLog()
            {
                UId = Users.Id,
                AddTime = DateTime.Now,
                LType = 2,
                Amount = AllMoney,
                AfterAmount = 0,
                AfterFrozen = 0,
                BeforAmount = BaoUsers.AllMoney,
                BeforFrozen = 0,
                State = 1,
            };
            this.Entity.BaoLog.AddObject(BaoLog);

            string SP_Ret = Entity.SP_UsersMoney(Users.Id, "理财转出", AllMoney, 1, "转出到余额");
            if (SP_Ret != "3")
            {
                Utils.WriteLog(string.Format("U{0},O{1},T{2}:{3}【{4}】", Users.Id, "理财转出", 1, AllMoney, SP_Ret), "SP_UsersMoney");
            }

            BaoUsers.AllMoney = 0;
            BaoUsers.ActMoney = 0;
            this.Entity.SaveChanges();

            Utils.WriteLog("用户ID:" + Users.Id.ToString() + "金额:" + AllMoney.ToString() + "操作员:" + this.AdminUser.TrueName + "操作时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), "TransferUser");
            ViewBag.Msg = "操作成功";
            return View("Succeed");
        }
    }

    public class BaoUsersVM
    {
        public BaoUsers BaoUsers { get; set; }
        public Users Users { get; set; }
    }
}
