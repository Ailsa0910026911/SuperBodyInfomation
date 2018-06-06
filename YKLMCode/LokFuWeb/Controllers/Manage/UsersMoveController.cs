using LokFu.Base;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace LokFu.Areas.Manage.Controllers
{
    public class UsersMoveController : BaseController
    {
        public ActionResult Index(Users Users, EFPagingInfo<Users> p, int IsFirst = 0)
        {
            ViewBag.AgentList = Entity.SysAgent.ToList();
            if (IsFirst == 0)
            {
                PageOfItems<Users> UsersList1 = new PageOfItems<Users>(new List<Users>(), 0, 10, 0, new Hashtable());
                ViewBag.UsersList = UsersList1;
                ViewBag.Users = Users;
                ViewBag.Batch = this.checkPower("Batch");
                ViewBag.AllUsers = this.checkPower("AllUsers");
                return View();
            }
            if (Users.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "请选择代理商";
                return View("Error");
            }
            if (!Users.Id.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Agent == Users.Id); }
            SysAgent SysAgent = Entity.SysAgent.FirstOrNew(o=>o.Id==Users.Id);
            p.SqlWhere.Add(f => f.UserName != SysAgent.LinkMobile);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Users> UsersList = Entity.Selects<Users>(p);
            ViewBag.UsersList = UsersList;
            ViewBag.Users = Users;
            ViewBag.Batch = this.checkPower("Batch");
            ViewBag.AllUsers = this.checkPower("AllUsers");
            return View();
        }

        public void Batch(int agengtid, string InfoList, int Value)
        {
            SysAgent tempAgent = Entity.SysAgent.FirstOrNew(o => o.Id == Value);//调入商户
            SysAgent Agengt = Entity.SysAgent.FirstOrNew(o => o.Id == agengtid);//调出商户
            if (tempAgent == null || Agengt == null)
            {
                Response.Write(0);
            }
            int Ret = 0;
            //string SQL = "update SysAgent set agentid='" + Value + "' where id in("+InfoList+")";
            //Ret = Entity.ExecuteStoreCommand(SQL);
            string[] users = InfoList.Split(',');

            //调入记录
            foreach (var info in users)
            {
                int temp = int.Parse(info);
                Users Users = Entity.Users.FirstOrDefault(o => o.Id == temp);
                if (Users != null)
                {
                    UsersMoveLog UsersMoveLog = new UsersMoveLog()
                    {
                        AddTime = DateTime.Now,
                        ToSAId = tempAgent.Id,
                        ToName = tempAgent.Name,
                        FromName = Agengt.Name,
                        FromSAId = Agengt.Id,
                        UId = Users.Id,
                        UTrueName = Users.TrueName.IsNullOrEmpty()?"":Users.TrueName,
                        OpName = AdminUser.TrueName,
                        Type = 1,
                        Tel=Users.UserName,
                    };
                    if (!Users.MyPId.IsNullOrEmpty())
                    {
                        ShareTotal ShareTotal = Entity.ShareTotal.FirstOrDefault(o => o.UId == Users.MyPId && o.Tier == 1);
                        if (ShareTotal != null)
                        {
                            ShareTotal.ShareNum = ShareTotal.ShareNum - 1;
                        }
                    }
                    Users.MyPId = 0;
                    Users.Agent = Value;
                    this.Entity.UsersMoveLog.AddObject(UsersMoveLog);
                }

            }

            Entity.SaveChanges();
            Response.Write(Ret);
        }


        public void AllUsers(int agengtid, int Value)
        {
            SysAgent tempAgent = Entity.SysAgent.FirstOrNew(o => o.Id == Value);//调入商户
            SysAgent Agengt = Entity.SysAgent.FirstOrNew(o => o.Id == agengtid);//调出商户
            if (tempAgent == null || Agengt == null)
            {
                Response.Write(0);
            }
            int Ret = 0;
            IList<Users> UsersList = Entity.Users.Where(o => o.Agent == agengtid && o.UserName != Agengt.LinkMobile).ToList();
            //调入记录
            foreach (var info in UsersList)
            {
                UsersMoveLog UsersMoveLog = new UsersMoveLog()
                {
                    AddTime = DateTime.Now,
                    ToSAId = tempAgent.Id,
                    ToName = tempAgent.Name,
                    FromName = Agengt.Name,
                    FromSAId = Agengt.Id,
                    UId = info.Id,
                    UTrueName = info.TrueName.IsNullOrEmpty() ? "" : info.TrueName,
                    OpName = AdminUser.TrueName,
                    Type = 1,
                    Tel = info.UserName,
                };
                if (!info.MyPId.IsNullOrEmpty())
                {
                    ShareTotal ShareTotal = Entity.ShareTotal.FirstOrDefault(o => o.UId == info.MyPId && o.Tier == 1);
                    if (ShareTotal != null)
                    {
                        ShareTotal.ShareNum = ShareTotal.ShareNum - 1;
                    }
                }
                info.MyPId = 0;
                info.Agent = Value;
                this.Entity.UsersMoveLog.AddObject(UsersMoveLog);
            }
            Entity.SaveChanges();
            Response.Write(Ret);
        }

    }
}
