using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections;
namespace LokFu.Areas.Agent.Controllers
{
    public class StopPayAuditController : BaseController
    {
        /// <summary>
        /// 解冻管理
        /// </summary>
        /// <param name="Users"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public ActionResult Index(StopPayAudit StopPayAudit, EFPagingInfo<StopPayAudit> p, bool IsShowSupAgent = false, int LowerLevel = 0, int UserSelect = 1, int IsFirst = 0)
        {
            if (!StopPayAudit.TState.IsNullOrEmpty()) { p.SqlWhere.Add(o => o.TState == StopPayAudit.TState); }
            //一级只有的功能
            if (BasicAgent.Tier == 1)
            {
                IList<SysAgent> SysAgentList = null;
                if (IsShowSupAgent)
                {
                    IList<int> UID = new List<int>();
                    if (LowerLevel != 0)
                    {
                        SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == LowerLevel).FirstOrNew();
                        SysAgentList = LowerLevelAgent.GetSupAgent(Entity, true);
                    }
                    else
                    {
                        SysAgentList = BasicAgent.GetSupAgent(Entity, true);//获取所有下级代理商信息
                    }
                    UID = SysAgentList.Select(o => o.Id).ToList();
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == BasicAgent.Id);//读取全部分支机构
                }
            }
            else
            {
                p.SqlWhere.Add(f => f.Agent == BasicAgent.Id);
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
            //用户表
            var SPAUsersList = StopPayAuditList.Select(o=>o.UId).ToList();
            ViewBag.UserSelect = Entity.Users.Where(o=> SPAUsersList.Contains(o.Id)).ToList();

            ViewBag.StopPayAuditList = StopPayAuditList;
            ViewBag.StopPayAudit = StopPayAudit;
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.LowerLevel = LowerLevel;
            return View();
        }

        public ActionResult Info(int id)
        {
            var StopPayAudit = this.Entity.StopPayAudit.FirstOrDefault(o => o.Id == id);
            if (StopPayAudit == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(StopPayAudit.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            var Users = this.Entity.Users.FirstOrNew(o => o.Id == StopPayAudit.UId);
            this.ViewBag.StopPayAudit = StopPayAudit;
            this.ViewBag.Users = Users;
            return View();
        }

        public ActionResult IndexRepeatSubmit(StopPayAudit StopPayAudit)
        {
            StopPayAudit = Request.ConvertRequestToModel<StopPayAudit>(StopPayAudit, StopPayAudit);
            if (StopPayAudit.Pic == "System.Web.HttpPostedFileWrapper" || StopPayAudit.Pic == null)
            {
                ViewBag.ErrorMsg = "文件类型错误";
                return View("Error");
            }
            if (StopPayAudit.Remark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写解除止付原因";
                return View("Error");
            }

            var baseStopPayAudit = this.Entity.StopPayAudit.FirstOrDefault(o => o.Id == StopPayAudit.Id);
            if (baseStopPayAudit == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(baseStopPayAudit.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }

            var BaseUsers = this.Entity.Users.Where(o => o.Id == baseStopPayAudit.UId).FirstOrDefault();
            if (BaseUsers == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!BaseUsers.IfUnFrozenRepeat())
            {
                ViewBag.ErrorMsg = "不符合重新提交止付规则，无法重新提交";
                return View("Error");
            }

            baseStopPayAudit.TState = 1;
            baseStopPayAudit.Remark = StopPayAudit.Remark;
            baseStopPayAudit.Pic = StopPayAudit.Pic;
            BaseUsers.StopPayAuditState = 1;
            Entity.SaveChanges();

            ViewBag.Title = "操作成功";
            return View("Succeed");
        }

    }
}
