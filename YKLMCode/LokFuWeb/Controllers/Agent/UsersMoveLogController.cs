using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class UsersMoveLogController : BaseController
    {
        /// <summary>
        /// 商户调入
        /// </summary>
        /// <returns></returns>
        [AdminFilter(false)]//不记录操作日志
        public ActionResult UsersMove()
        {
            return View();
        }
        /// <summary>
        /// 商户调入
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UsersMove(Users users)
        {
            #region 校验
            Users Users = this.Entity.Users.Where(o => o.UserName == users.UserName.Trim() && o.TrueName == users.TrueName.Trim() && o.CardId == users.CardId.Trim()).FirstOrDefault();
            if (Users == null)
            {
                ViewBag.ErrorMsg = "用户不存在";
                return View("Error");
            }
            if (Users.CardStae != 2)
            {
                ViewBag.ErrorMsg = "用户未实名认证";
                return View("Error");
            }
            if (Users.SAId != 0)
            {
                ViewBag.ErrorMsg = "此商户不可调入";
                return View("Error");
            }
            if (Users.Agent == this.BasicAgent.Id)
            {
                ViewBag.ErrorMsg = "用户：" + Users.UserName + " ,已经是你的商户，无需调入";
                return View("Error");
            }
            UserCard UserCard = this.Entity.UserCard.FirstOrDefault(o => o.UId == Users.Id && o.Card == users.CardNum.Trim() && o.Type == 1 && o.State == 1);
            if (UserCard == null)
            {
                ViewBag.ErrorMsg = "借记卡卡号不正确";
                return View("Error");
            }
            
            var FSysAgent = this.Entity.SysAgent.FirstOrDefault(o => o.Id == Users.Agent);
            if (FSysAgent == null)
            {
                ViewBag.ErrorMsg = "所属代理不存在";
                return View("Error");
            }
            #endregion
            //处理下级用户
            var sonUsers = this.Entity.Users.Where(o => o.MyPId == Users.Id).ToList();
            foreach (var item in sonUsers)
            {
                //新增分享记录减少
                if (!item.MyPId.IsNullOrEmpty())
                {
                    ShareTotal ShareTotal = Entity.ShareTotal.FirstOrDefault(o => o.UId == item.MyPId && o.Tier == 1);
                    if (ShareTotal != null)
                    {
                        ShareTotal.ShareNum = ShareTotal.ShareNum - 1;
                    }
                }
                item.MyPId = 0;
            }

            int Agent = 0;

            //处理调入用户
            Agent = Users.Agent;
            if (!Users.MyPId.IsNullOrEmpty())
            {
                ShareTotal ShareTotal = Entity.ShareTotal.FirstOrDefault(o => o.UId == Users.MyPId && o.Tier == 1);
                if (ShareTotal != null)
                {
                    ShareTotal.ShareNum = ShareTotal.ShareNum - 1;
                }
            }
            Users.MyPId = 0;
            Users.Agent = this.BasicAgent.Id;
            Users.AId = this.AdminUser.Id;

            //调入记录
            UsersMoveLog UsersMoveLog = new UsersMoveLog()
            {
                AddTime = DateTime.Now,
                ToSAId = this.BasicAgent.Id,
                ToName = this.BasicAgent.Name,
                FromName = FSysAgent.Name,
                FromSAId = Agent,
                UId = Users.Id,
                UTrueName = users.TrueName ?? users.UserName,
                OpName=this.BasicAgent.Name,
                Type=1,
                Tel=users.UserName,
            };
            this.Entity.UsersMoveLog.AddObject(UsersMoveLog);

            this.Entity.SaveChanges();
            ViewBag.Title = "操作成功";
            ViewBag.Msg = string.Format("商户：{0}，已经成功调入！！", Users.UserName);
            return View("Succeed");
        }

        public ActionResult Index(UsersMoveLog UsersMoveLog, EFPagingInfo<UsersMoveLog> p)
        {
            p.SqlWhere.Add(o => o.FromSAId == this.BasicAgent.Id || o.ToSAId == this.BasicAgent.Id);
            p.OrderByList.Add("AddTime", "DESC");
            IPageOfItems<UsersMoveLog> UsersMoveLogList = Entity.Selects<UsersMoveLog>(p);
            this.ViewBag.UsersMoveLogList = UsersMoveLogList;
            this.ViewBag.UsersMoveLog = UsersMoveLog;
            return View();
        }

    }
}
