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
    public class MsgUserController : BaseController
    {

        public ActionResult Index(MsgUser MsgUser, EFPagingInfo<MsgUser> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<MsgUser> MsgUserList1 = new PageOfItems<MsgUser>(new List<MsgUser>(), 0, 10, 0, new Hashtable());
                ViewBag.MsgUserList = MsgUserList1;
                ViewBag.MsgUser = MsgUser;
                return View();

            }
            if (!MsgUser.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(MsgUser.Name)); }
            if (!MsgUser.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (MsgUser.State == 99 ? 0 : MsgUser.State)); }
            p.SqlWhere.Add(f => f.PId == AdminUser.Id);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgUser> MsgUserList = Entity.Selects<MsgUser>(p);
            ViewBag.MsgUserList = MsgUserList;
            ViewBag.MsgUser = MsgUser;
            return View();
        }
        public ActionResult Edit(MsgUser MsgUser)
        {
            Dictionary<int, string> userName = new Dictionary<int, string>();
            if (MsgUser.Id != 0)
            {
                MsgUser = Entity.MsgUser.FirstOrDefault(x => x.Id == MsgUser.Id&&x.PId==AdminUser.Id);
                if (MsgUser == null)
                {
                    ViewBag.ErrorMsg = AgentLanguage.Empty;
                    return View("Error");
                }
                //有多个用户的情况下
                if (MsgUser.UId == 0 && MsgUser.SendUsers != ",0,")
                {
                    string[] uid = MsgUser.SendUsers.Substring(1, MsgUser.SendUsers.Length - 1).Split(',');
                    foreach (var item in uid)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            int Uid = Convert.ToInt32(item);
                            var userModel = Entity.Users.FirstOrDefault(x => x.Id == Uid);
                            if (userModel != null)
                            {
                                userName.Add(userModel.Id, string.IsNullOrWhiteSpace(userModel.TrueName) ? userModel.UserName : userModel.TrueName);
                            }
                        }
                    }
                }
                //全体商户的情况下
                else if (MsgUser.UId == 0 && MsgUser.SendUsers == ",0,")
                {
                    userName.Add(0, "全体商户");
                }
                //单个用户的情况下
                else if (MsgUser.UId > 0 && MsgUser.SendUsers != ",0,")
                {
                    var userModel = Entity.Users.FirstOrDefault(x => x.Id == MsgUser.UId);
                    if (userModel != null)
                    {
                        userName.Add(userModel.Id, string.IsNullOrWhiteSpace(userModel.TrueName) ? userModel.UserName : userModel.TrueName);
                    }
                }
            }
            ViewBag.UserName = userName;
            ViewBag.MsgUser = MsgUser;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(MsgUser MsgUser, bool? IsShowSupAgent)
        {
            if (MsgUser.UId == 0)
            {
                Response.Redirect("/Agent/home/error.html?msg=参数错误~");
                return;
            }
            IList<Users> listUser = new List<Users>();
            string sendUsers = "";
            //获取代理商
            SysAgent LowerLevelAgent = null;
            #region 单个用户或者全体商户 处理
            //if (MsgUser.UId == 0)
            //{
            //    LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == BasicAgent.Id).FirstOrNew();
            //    //是否要显示下级
            //    if ((bool)IsShowSupAgent)
            //    {
            //        IList<SysAgent> SysAgentList = LowerLevelAgent.GetSupAgent(Entity);
            //        listUser = SysAgentList.GetAgentUsers(Entity);
            //    }
            //    else
            //    {
            //        listUser = LowerLevelAgent.GetAgentUsers(Entity);
            //    }
            //    if (listUser.Count > 0)
            //    {
            //        MsgUser.UId = 0;
            //        foreach (var item in listUser)
            //        {
            //            sendUsers += item.Id + ",";
            //        }
            //    }
            //}
            #endregion

            MsgUser.PId = AdminUser.Id;
            //MsgUser.SendUsers = !string.IsNullOrWhiteSpace(sendUsers) ? "," + sendUsers : ",0,";
            MsgUser.SendUsers = "";
            MsgUser.AddTime = DateTime.Now;
            MsgUser.ReadUsers = string.Empty;
            MsgUser.DeleteUsers = string.Empty;
            Entity.MsgUser.AddObject(MsgUser);
            Entity.SaveChanges();
            if (MsgUser.UId > 0)
            {
                MsgUser.PushMsg(Entity);
            }
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(MsgUser MsgUser)
        {
            if (MsgUser.UId == 0)
            {
                Response.Redirect("/Agent/home/error.html?msg=参数错误~");
                return;
            }
            MsgUser baseMsgUser = Entity.MsgUser.FirstOrDefault(n => n.Id == MsgUser.Id && n.PId == AdminUser.Id);
            if (baseMsgUser==null)
            {
                Response.Redirect("/Agent/home/error.html?msg=无权限修改~");
                return;
            }
            MsgUser.SendUsers = baseMsgUser.SendUsers;
            MsgUser.UId = baseMsgUser.UId;
           // MsgUser.SendUsers = baseMsgUser.SendUsers;
            baseMsgUser = Request.ConvertRequestToModel<MsgUser>(baseMsgUser, MsgUser);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(MsgUser MsgUser, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgUser.Id.ToString(); }
            int Ret = Entity.ChangeEntity<MsgUser>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(MsgUser MsgUser, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgUser.Id.ToString(); }
            MsgUser baseMsgUser = Entity.MsgUser.FirstOrDefault(n => n.Id == MsgUser.Id && n.PId == AdminUser.Id);
            if (baseMsgUser == null)
            {
                Response.Redirect("/Agent/home/error.html?msg=无权限删除~");
                return;
            }
            int Ret = Entity.MoveToDeleteEntity<MsgUser>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
