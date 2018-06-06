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
    public class UsersController : BaseController
    {
        public bool IsAll = false;

        public ActionResult Index(Users Users, EFPagingInfo<Users> p, bool? IsShowSupAgent, int? LowerLevel, int IsFirst = 0)
        {
            #region 条件
            if (checkPower("ALL"))
            {
                IsAll = true;
            }
            if (IsShowSupAgent == null)
            {
                IsShowSupAgent = false;
            }
            LowerLevel = LowerLevel == null ? 0 : LowerLevel;
            ViewBag.IsAll = IsAll;
            /*有没有指定交易所属代理
             * 有:校验是否从属关系
             * 无：指定当前代理
             */
            if (!Users.Agent.IsNullOrEmpty())
            {
                if (!IsBelongToAgent(Users.Agent))
                {
                    ViewBag.ErrorMsg = "只能查询当前用户下属代理的交易";
                    return View("Error");
                }
            }
            else
            {
                Users.Agent = this.BasicAgent.Id;
            }
            IList<SysAgent> SysAgentList = null;
            //没有"管理所有"权限的只能看到操作员自己的数据
            if (!IsAll)
            {
                p.SqlWhere.Add(f => f.AId == AdminUser.Id);//指定的操作员
            }
            else
            {
                if ((bool)IsShowSupAgent)
                {
                    IList<int> UID = new List<int>();
                    if (LowerLevel != 0)
                    {
                        SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == LowerLevel).FirstOrNew();
                        SysAgentList = LowerLevelAgent.GetSupAgent(Entity, true);
                    }
                    else
                    {
                        SysAgentList = BasicAgent.GetSupAgent(Entity);//获取所有下级代理商信息
                    }
                    foreach (var s in SysAgentList)
                    {
                        UID.Add(s.Id);
                    }
                    p.SqlWhere.Add(f => UID.Contains(f.Agent));
                }
                else
                {
                    p.SqlWhere.Add(f => f.Agent == Users.Agent);//指定的代理
                }
            }
            if (Users.CardStae.IsNullOrEmpty())
            {
                Users.CardStae = 2;
            }
            if (!Users.RegAddress.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.RegAddress.Contains(Users.RegAddress)); }
            if (!Users.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName == Users.UserName); }
            if (!Users.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName == Users.TrueName || f.NeekName == Users.TrueName); }
            if (!Users.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile == Users.Mobile); }
            if (!Users.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (Users.State == 99 ? 0 : Users.State)); }
            if (!Users.CardNum.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CardNum == Users.CardNum); }
            if (!Users.CardStae.IsNullOrEmpty())
            {
                int CardStae = Users.CardStae;
                if (CardStae == 99)
                {
                    CardStae = 0;
                }
                if (CardStae != 88)
                {
                    p.SqlWhere.Add(f => f.CardStae == CardStae);
                }
            }
            #endregion
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Users> UsersList = null;
            if (IsFirst == 0)
            {
                UsersList = new PageOfItems<Users>(new List<Users>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                UsersList = Entity.Selects<Users>(p);
            }

            foreach (var item in UsersList)
            {
                item.Address = item.QRCode();
            }
            ViewBag.UsersList = UsersList;
            ViewBag.Users = Users;
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            ViewBag.SysSet = SysSet;
            ViewBag.IsShowSupAgent = IsShowSupAgent;
            ViewBag.SysAgentList = SysAgentList;
            ViewBag.LowerLevel = LowerLevel;
            ViewBag.Entity = Entity;
            ViewBag.IsAssureImg = checkPower("SaveAssureImg");
            ViewBag.IsFrozen = this.checkPower("Frozen");
            ViewBag.IsUnFrozen = this.checkPower("UnFrozen");
            ViewBag.MyUsers = this.checkPower("MyUsers");

            return View();
        }

        public ActionResult Info(Users Users)
        {
            if (Users.Id != 0) Users = Entity.Users.FirstOrDefault(n => n.Id == Users.Id);
            if (Users == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(Users.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            ViewBag.Users = Users;
            ViewBag.UserPayList = Entity.UserPay.Where(n => n.UId == Users.Id).ToList();
            ViewBag.PayConfigList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            ViewBag.SysAgent = Entity.SysAgent.FirstOrNew(n => n.Id == Users.Agent);
            ViewBag.IsUserPayChangeEdit = this.checkPower("UserPayChange", "Edit");
            return View();
        }

        public ActionResult IndexUserFrozenLog(int id)
        {
            if (id == 0)
            {
                ViewBag.ErrorMsg = "参数错误";
                return View("Error");
            }
            List<UserFrozenLog> UserFrozenLog = this.Entity.UserFrozenLog.OrderByDescending(o => o.AddTime).Where(o => o.UId == id).ToList();
            this.ViewBag.UserFrozenLog = UserFrozenLog;
            return View();
        }
        [ValidateInput(false)]
        public ActionResult SaveAssureImg(Users Users, int? Id)
        {
            Users = Request.ConvertRequestToModel<Users>(Users, Users);
            if (Users.AssureImgName == "System.Web.HttpPostedFileWrapper" || Users.AssureImgName == null)
            {
                ViewBag.ErrorMsg = "文件类型错误";
                return View("Error");
            }
            Users BasicUsers = Entity.Users.FirstOrDefault(U => U.Id == Id);
            BasicUsers.AssureImgName = Users.AssureImgName;
            Entity.SaveChanges();
            return View("ReloadFrame");
        }

        public ActionResult IndexMyUsers(Users Users, EFPagingInfo<Users> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<Users> UsersList1 = new PageOfItems<Users>(new List<Users>(), 0, 10, 0, new Hashtable());
                ViewBag.UsersList = UsersList1;
                ViewBag.Users = Users;
                return View();
            }
            Users baseUsers = null;
            if (!Users.MyPId.IsNullOrEmpty()) { baseUsers = this.Entity.Users.FirstOrNew(o => o.Id == Users.MyPId); }
            if (baseUsers == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(baseUsers.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            if (!Users.UserName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.UserName.Contains(Users.UserName)); }
            if (!Users.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (Users.State == 99 ? 0 : Users.State)); }
            //if (!Users.MyPId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.MyPId == Users.MyPId); }
            if (!Users.ShareType.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.ShareType == Users.ShareType); }
            if (!Users.CardStae.IsNullOrEmpty())
            {
                int CardStae = Users.CardStae;
                if (CardStae == 99)
                {
                    CardStae = 0;
                }
                if (CardStae != 88)
                {
                    p.SqlWhere.Add(f => f.CardStae == CardStae);
                }
            }
            p.SqlWhere.Add(f => f.MyPId == Users.MyPId);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Users> UsersList = Entity.Selects<Users>(p);
            ViewBag.UsersList = UsersList;
            ViewBag.Users = Users;
            //显示上级名字
            return View();
        }

        /// <summary>
        /// 止付
        /// </summary>
        /// <param name="Users"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Frozen(int Id)
        {
            #region 条件
            if (Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请选择目标商户";
                return View("Error");
            }
            var BaseUsers = this.Entity.Users.Where(o => o.Id == Id).FirstOrDefault();
            if (BaseUsers == null)
            {
                ViewBag.ErrorMsg = "无法找到商户";
                return View("Error");
            }
            if (!BaseUsers.IfFrozen())
            {
                ViewBag.ErrorMsg = "不符合止付规则，无法止付";
                return View("Error");
            }
            var ForSysAgent = BaseUsers.ForSysAgent(this.Entity);
            if (ForSysAgent != null)
            {
                ViewBag.ErrorMsg = "代理商钱包账号无法止付";
                return View("Error");
            }
            if (this.BasicAgent.Tier != 1)
            {
                if (BaseUsers.Agent != this.BasicAgent.Id)
                {
                    ViewBag.ErrorMsg = "不能止付下级的商户";
                    return View("Error");
                }
            }
            else if (!IsBelongToAgent(BaseUsers.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            #endregion

            ViewBag.Users = BaseUsers;
            return View();
        }

        /// <summary>
        /// 止付
        /// </summary>
        /// <param name="Users"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Frozen(StopPayAudit StopPayAudit)
        {
            #region 条件
            if (StopPayAudit.UId.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请选择目标商户";
                return View("Error");
            }
            StopPayAudit = Request.ConvertRequestToModel<StopPayAudit>(StopPayAudit, StopPayAudit);
            if (StopPayAudit.Pic == "System.Web.HttpPostedFileWrapper" || StopPayAudit.Pic == null)
            {
                ViewBag.ErrorMsg = "文件类型错误";
                return View("Error");
            }
            if (StopPayAudit.Remark.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请填写止付原因";
                return View("Error");
            }
            var BaseUsers = this.Entity.Users.Where(o => o.Id == StopPayAudit.UId).FirstOrDefault();
            if (BaseUsers == null)
            {
                ViewBag.ErrorMsg = "无法找到商户";
                return View("Error");
            }
            if (!BaseUsers.IfFrozen())
            {
                ViewBag.ErrorMsg = "不符合止付规则，无法止付";
                return View("Error");
            }
            var ForSysAgent = BaseUsers.ForSysAgent(this.Entity);
            if (ForSysAgent != null)
            {
                ViewBag.ErrorMsg = "代理商钱包账号无法止付";
                return View("Error");
            }
            if (this.BasicAgent.Tier != 1)
            {
                if (BaseUsers.Agent != this.BasicAgent.Id)
                {
                    ViewBag.ErrorMsg = "不能止付下级的商户";
                    return View("Error");
                }
            }
            else if (!IsBelongToAgent(BaseUsers.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            #endregion

            BaseUsers.StopPayState = 2;
            BaseUsers.AutoBao = 0;
            BaseUsers.AutoCash = 0;

            var UserFrozenLog = new UserFrozenLog()
            {
                AddTime = DateTime.Now,
                OpName = AdminUser.TrueName,
                AId = AdminUser.Id,
                OpType = 1,
                LogType = 2,
                UId = StopPayAudit.UId,
                StopPayMoney = 0,
                Remark = StopPayAudit.Remark,
                Platform = 1,
                InteriorRemark = string.Empty,
                Img = StopPayAudit.Pic,
            };
            this.Entity.UserFrozenLog.AddObject(UserFrozenLog);
            this.Entity.SaveChanges();
            ViewBag.Title = "操作成功";
            ViewBag.Msg = BaseUsers.UserName + "止付成功！";
            return View("Succeed");
        }

        /// <summary>
        /// 解除止付
        /// </summary>
        /// <param name="Users"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UnFrozen(int Id)
        {
            #region 条件
            if (Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请选择目标商户";
                return View("Error");
            }
            var BaseUsers = this.Entity.Users.Where(o => o.Id == Id).FirstOrDefault();
            if (BaseUsers == null)
            {
                ViewBag.ErrorMsg = "无法找到商户";
                return View("Error");
            }
            if (!BaseUsers.IfUnFrozen())
            {
                ViewBag.ErrorMsg = "不符合解除止付规则，无法解除";
                return View("Error");
            }
            var ForSysAgent = BaseUsers.ForSysAgent(this.Entity);
            if (ForSysAgent != null)
            {
                ViewBag.ErrorMsg = "代理商钱包账号无法解除";
                return View("Error");
            }
            if (this.BasicAgent.Tier != 1)
            {
                if (BaseUsers.Agent != this.BasicAgent.Id)
                {
                    ViewBag.ErrorMsg = "不能解除下级的商户";
                    return View("Error");
                }
            }
            else if (!IsBelongToAgent(BaseUsers.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            #endregion
            ViewBag.Users = BaseUsers;
            return View("Frozen");
        }

        /// <summary>
        /// 解除止付
        /// </summary>
        /// <param name="Users"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UnFrozen(StopPayAudit StopPayAudit)
        {
            #region 条件
            if (StopPayAudit.UId.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请选择目标商户";
                return View("Error");
            }
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
            var BaseUsers = this.Entity.Users.Where(o => o.Id == StopPayAudit.UId).FirstOrDefault();
            if (BaseUsers == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!BaseUsers.IfUnFrozen())
            {
                ViewBag.ErrorMsg = "不符合解除止付规则，无法解除";
                return View("Error");
            }
            var ForSysAgent = BaseUsers.ForSysAgent(this.Entity);
            if (ForSysAgent != null)
            {
                ViewBag.ErrorMsg = "代理商钱包账号无法解除止付";
                return View("Error");
            }
            if (this.BasicAgent.Tier != 1)
            {
                if (BaseUsers.Agent != this.BasicAgent.Id)
                {
                    ViewBag.ErrorMsg = "不能解除下级的商户";
                    return View("Error");
                }
            }
            else if (!IsBelongToAgent(BaseUsers.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            #endregion

            BaseUsers.StopPayAuditState = 1;
            StopPayAudit.AddTime = DateTime.Now;
            StopPayAudit.CreateAdminId = this.AdminUser.Id;
            StopPayAudit.CreateAdminName = this.AdminUser.TrueName;
            StopPayAudit.StopPayMoney = 0;
            StopPayAudit.StopPayType = 2;
            StopPayAudit.TState = 1;
            StopPayAudit.Agent = BaseUsers.Agent;
            this.Entity.StopPayAudit.AddObject(StopPayAudit); 
            var UserFrozenLog = new UserFrozenLog()
            {
                AddTime = DateTime.Now,
                OpName = AdminUser.TrueName,
                AId = AdminUser.Id,
                OpType = 2,
                LogType = 1,
                UId = StopPayAudit.UId,
                StopPayMoney = 0,
                Remark = StopPayAudit.Remark,
                Platform = 1,
                InteriorRemark = string.Empty,
                Img = StopPayAudit.Pic,
            };
            this.Entity.UserFrozenLog.AddObject(UserFrozenLog);
            this.Entity.SaveChanges();
            ViewBag.Title = "操作成功";
            ViewBag.Msg = BaseUsers.UserName + "申请解除止付成功，请等待审核";
            return View("Succeed");
        }

        public ActionResult IndexUsersMoveLog(int id)
        {
            var BaseUsers = this.Entity.Users.FirstOrDefault(o => o.Id == id);
            if (BaseUsers == null)
            {
                ViewBag.ErrorMsg = AgentLanguage.Empty;
                return View("Error");
            }
            if (!IsBelongToAgent(BaseUsers.Agent))
            {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            var UsersMoveLogList = this.Entity.UsersMoveLog.Where(o => o.UId == id).OrderByDescending(o => o.Id).ToList();
            this.ViewBag.UsersMoveLogList = UsersMoveLogList;

            return View();
        }

    }
}
