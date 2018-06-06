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
    public class StopPayAuditController : BaseController
    {
        public ActionResult Index(StopPayAudit StopPayAudit, EFPagingInfo<StopPayAudit> p, bool IsShowSupAgent = false, int LowerLevel = 0, int IsFirst = 0)
        {
            #region 条件

            if (!StopPayAudit.Agent.IsNullOrEmpty())
            {
                //是否要显示下级
                if ((bool)IsShowSupAgent)
                {
                    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == StopPayAudit.Agent).FirstOrNew();
                    IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity, true);
                    IList<int> UID = new List<int>();
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == StopPayAudit.Agent);
                }
            }
            if (!StopPayAudit.TState.IsNullOrEmpty())
            {
                p.SqlWhere.Add(o => o.TState == StopPayAudit.TState);
            }
            if (!StopPayAudit.CreateAdminName.IsNullOrEmpty())
            {
                List<int> uids = new List<int>();
                if (StopPayAudit.UId == 1)
                {
                    uids = Entity.Users.Where(o => o.UserName == StopPayAudit.CreateAdminName).Select(o => o.Id).ToList();
                }
                else if (StopPayAudit.UId == 2)
                {
                    uids = Entity.Users.Where(o => o.Mobile == StopPayAudit.CreateAdminName).Select(o => o.Id).ToList();
                }
                else if (StopPayAudit.UId == 3)
                {
                    uids = Entity.Users.Where(o => o.TrueName == StopPayAudit.CreateAdminName).Select(o => o.Id).ToList();
                }
                p.SqlWhere.Add(f => uids.Contains(f.UId));
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<StopPayAudit> StopPayAuditList = null;
            if (IsFirst == 0)
            {
                StopPayAuditList = new PageOfItems<StopPayAudit>(new List<StopPayAudit>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                StopPayAuditList = Entity.Selects<StopPayAudit>(p);
            }
            ViewBag.StopPayAuditList = StopPayAuditList;
            ViewBag.StopPayAudit = StopPayAudit;
            //用户表
            var SPAUsersList = StopPayAuditList.Select(o => o.UId).ToList();
            ViewBag.UserSelect = Entity.Users.Where(o => SPAUsersList.Contains(o.Id)).ToList();

            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1).ToList();
            ViewBag.IsShowSupAgent = IsShowSupAgent;//是否显示下级
            ViewBag.IsAudit = this.checkPower("Audit");
            return View();
        }

        [HttpGet]
        public ActionResult Audit(int Id)
        {
            if (Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写ID";
                return View("Error");
            }

            var StopPayAudit = Entity.StopPayAudit.FirstOrDefault(o => o.Id == Id);
            if (StopPayAudit == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }

            var BaseUsers = this.Entity.Users.FirstOrDefault(o => o.Id == StopPayAudit.UId);
            if (BaseUsers == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (BaseUsers.StopPayAuditState == 1)
            {
                if (!BaseUsers.IfUnFrozenAudit())
                {
                    ViewBag.ErrorMsg = "不符合解冻审核规则";
                    return View("Error");
                }
            }
            ViewBag.Users = BaseUsers;
            ViewBag.StopPayAudit = StopPayAudit;
            ViewBag.BasicDescList1 = GetBasicDescList(BasicCodeEnum.Djsh);
            ViewBag.BasicDescList2 = GetBasicDescList(BasicCodeEnum.Djnbsh);
            return View();
        }

        /// <summary>
        /// 解除止付审核
        /// </summary>
        [HttpPost]
        public ActionResult Audit(StopPayAudit StopPayAudit)
        {
            if (StopPayAudit.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写ID";
                return View("Error");
            }

            var BaseStopPayAudit = Entity.StopPayAudit.FirstOrDefault(o => o.Id == StopPayAudit.Id);
            if (BaseStopPayAudit == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }

            var BaseUsers = this.Entity.Users.FirstOrDefault(o => o.Id == BaseStopPayAudit.UId);
            if (BaseUsers == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            if (!BaseUsers.IfUnFrozenAudit())
            {
                ViewBag.ErrorMsg = "不符合解冻审核规则";
                return View("Error");
            }
            //商户数据修改
            BaseUsers.StopPayAuditState = StopPayAudit.TState;
            if (BaseUsers.StopPayAuditState == 2)
            {
                BaseUsers.StopPayState = 0;
            }

            BaseStopPayAudit.TState = StopPayAudit.TState;
            BaseStopPayAudit.AuditRemark = StopPayAudit.AuditRemark;
            BaseStopPayAudit.AuditInteriorRemark = StopPayAudit.AuditInteriorRemark;
            BaseStopPayAudit.AuditTime = DateTime.Now;
            BaseStopPayAudit.AuditAdminId = this.AdminUser.Id;
            BaseStopPayAudit.AuditAdminName = this.AdminUser.TrueName;

            //记录日志
            var UserFrozenLog = new UserFrozenLog()
            {
                Img = string.Empty,
                Remark = StopPayAudit.AuditRemark,
                AddTime = DateTime.Now,
                OpName = this.AdminUser.TrueName,
                UId = BaseUsers.Id,
                LogType = StopPayAudit.TState,
                OpType = 2,
                AId = this.AdminUser.Id,
                InteriorRemark = StopPayAudit.AuditInteriorRemark,
                Platform = 2,
            };
            this.Entity.UserFrozenLog.AddObject(UserFrozenLog);
            this.Entity.SaveChanges();
            ViewBag.Title = "操作成功";
            ViewBag.Msg = "操作结果：审核" + (StopPayAudit.TState == 2 ? "通过" : "不通过");
            return View("Succeed");
        }

    }
}
