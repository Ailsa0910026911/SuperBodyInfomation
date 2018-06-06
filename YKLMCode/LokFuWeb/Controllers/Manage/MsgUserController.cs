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
    public class MsgUserController : BaseController
    {
        public ActionResult Index(MsgUser MsgUser, EFPagingInfo<MsgUser> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<MsgUser> MsgUserList1 = new PageOfItems<MsgUser>(new List<MsgUser>(), 0, 10, 0, new Hashtable());
                ViewBag.MsgUserList = MsgUserList1;
                ViewBag.MsgUser = MsgUser;
                ViewBag.UsersList = new List<Users>();
                ViewBag.IsMyDelete = checkPower("Delete");
                ViewBag.IsMyEdit = checkPower("Edit");
                ViewBag.IsMySave = checkPower("Save");
                ViewBag.IsMyAdd = checkPower("Add");
                return View();
            }
            if (!MsgUser.Info.IsNullOrEmpty())
            {
                if (MsgUser.Info == "全体商户")
                {
                    p.SqlWhere.Add(f => f.UId == 0);
                }
                else
                {
                    IList<Users> UList = Entity.Users.Where(n => n.TrueName.Contains(MsgUser.Info) || n.NeekName.Contains(MsgUser.Info) || n.UserName == MsgUser.Info).ToList();
                    List<int> UIds = new List<int>();
                    foreach (var pp in UList)
                    {
                        UIds.Add(pp.Id);
                    }
                    p.SqlWhere.Add(f => UIds.Contains(f.UId));
                }
            }
            if (!MsgUser.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(MsgUser.Name)); }
            if (!MsgUser.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (MsgUser.State == 99 ? 0 : MsgUser.State)); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgUser> MsgUserList = Entity.Selects<MsgUser>(p);
            ViewBag.MsgUserList = MsgUserList;
            ViewBag.MsgUser = MsgUser;
            IList<MsgUser> List = MsgUserList.GroupBy(n => n.UId).Select(n => n.First()).ToList();
            List<int> UId = new List<int>();
            foreach (var pp in List)
            {
                UId.Add(pp.UId);
            }
            ViewBag.UsersList = Entity.Users.Where(n => n.State == 1 && UId.Contains(n.Id)).ToList();

            ViewBag.IsMyDelete = checkPower("Delete");
            ViewBag.IsMyEdit = checkPower("Edit");
            ViewBag.IsMySave = checkPower("Save");
            ViewBag.IsMyAdd = checkPower("Add");
            return View();
        }
        public ActionResult Edit(MsgUser MsgUser, int UserId = 0)
        {
            Dictionary<int, string> userName = new Dictionary<int, string>();
            if (MsgUser.Id != 0)
            {
                MsgUser = Entity.MsgUser.FirstOrDefault(n => n.Id == MsgUser.Id);
                if (MsgUser == null)
                {
                    ViewBag.ErrorMsg = "数据不存在";
                    return View("Error");
                }
                //有多个用户的情况下
                if (MsgUser.UId == 0 && !string.IsNullOrEmpty(MsgUser.SendUsers))
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
                else if (MsgUser.UId == 0 && string.IsNullOrWhiteSpace(MsgUser.SendUsers))
                {
                    userName.Add(0, "全体商户");
                }
                //单个用户的情况下
                else if (MsgUser.UId > 0 && string.IsNullOrWhiteSpace(MsgUser.SendUsers))
                {
                    var userModel = Entity.Users.FirstOrDefault(x => x.Id == MsgUser.UId);
                    if (userModel != null)
                    {
                        userName.Add(userModel.Id, string.IsNullOrWhiteSpace(userModel.TrueName) ? userModel.UserName : userModel.TrueName);
                    }
                }
            }
            ViewBag.UserName = userName;
            ViewBag.SysAgentList = Entity.SysAgent.Where(x => x.Tier == 1 && x.State == 1).ToList();
            ViewBag.MsgUser = MsgUser;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(MsgUser MsgUser,  string AgentId, bool? IsShowSupAgent)
        {
            if (MsgUser.UId == 0)
            {
                Response.Redirect("/Manage/home/error.html?msg=参数错误~");
                return;
            }
            string sendUsers = "";
            //根据代理商发送
            //if (IsState == "0")
            //{
            //    int AId = Convert.ToInt32(AgentId);

            //    IList<Users> listUser = new List<Users>();
            //    //获取代理商
            //    SysAgent LowerLevelAgent = Entity.SysAgent.Where(s => s.Id == AId).FirstOrNew();
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
            //    if (listUser.Count > 1)
            //    {
            //        MsgUser.UId = 0;
            //        foreach (var item in listUser)
            //        {
            //            sendUsers += item.Id + ",";
            //        }
            //    }
            //    else
            //    {
            //        MsgUser.UId = listUser[0].Id;
            //    }
            //}
            MsgUser.PId = AdminUser.Id;
            MsgUser.SendUsers = !string.IsNullOrWhiteSpace(sendUsers) ? "," + sendUsers : sendUsers;
            MsgUser.AddTime = DateTime.Now;
            MsgUser.ReadUsers = string.Empty;
            MsgUser.DeleteUsers = string.Empty;
            Entity.MsgUser.AddObject(MsgUser);
            Entity.SaveChanges();
            if (string.IsNullOrWhiteSpace(MsgUser.SendUsers))
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
                Response.Redirect("/Manage/home/error.html?msg=参数错误~");
                return;
            }
            MsgUser baseMsgUser = Entity.MsgUser.FirstOrDefault(n => n.Id == MsgUser.Id);
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
            int Ret = Entity.MoveToDeleteEntity<MsgUser>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
