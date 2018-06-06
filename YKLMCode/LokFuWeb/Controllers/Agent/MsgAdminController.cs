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
    public class MsgAdminController : BaseController
    {
        public ActionResult Index(MsgAdmin MsgAdmin, EFPagingInfo<MsgAdmin> p, int IsFirst = 0)
        {
            if (IsFirst==0)
            {
                PageOfItems<MsgAdmin> MsgAdminList1 = new PageOfItems<MsgAdmin>(new List<MsgAdmin>(), 0, 10, 0, new Hashtable());
                ViewBag.MsgAdminList = MsgAdminList1;
                ViewBag.MsgAdmin = MsgAdmin;
                ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1).ToList();
                return View();
            }
            if (!MsgAdmin.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == MsgAdmin.AId); }
            if (!MsgAdmin.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(MsgAdmin.Name)); }
            if (!MsgAdmin.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (MsgAdmin.State == 99 ? 0 : MsgAdmin.State)); }
            p.SqlWhere.Add(f => f.PId == AdminUser.Id);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgAdmin> MsgAdminList = Entity.Selects<MsgAdmin>(p);
            ViewBag.MsgAdminList = MsgAdminList;
            ViewBag.MsgAdmin = MsgAdmin;
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1).ToList();
            //ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1 && n.AgentID == n.Id).ToList();
            return View();
        }
        public ActionResult Edit(MsgAdmin MsgAdmin)
        {
            Dictionary<int, string> userName = new Dictionary<int, string>();
            if (MsgAdmin.Id != 0)
            {
                MsgAdmin = Entity.MsgAdmin.FirstOrDefault(n => n.Id == MsgAdmin.Id&&n.PId==AdminUser.Id);
                if (MsgAdmin == null)
                {
                    ViewBag.ErrorMsg = AgentLanguage.Empty;
                    return View("Error");
                }
                switch (MsgAdmin.IsAdmin)
                {
                    //管理员
                    case 0:
                        //管理员多人
                        if (MsgAdmin.AId == 0 && MsgAdmin.SendUsers != ",0,")
                        {
                            string[] Aid = MsgAdmin.SendUsers.Substring(1, MsgAdmin.SendUsers.Length - 2).Split(',');
                            foreach (var item in Aid)
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                {
                                    int aid = Convert.ToInt32(item);
                                    var adminModel = Entity.SysAdmin.FirstOrDefault(x => x.Id == aid);
                                    if (adminModel != null)
                                    {
                                        userName.Add(adminModel.Id, string.IsNullOrWhiteSpace(adminModel.TrueName) ? adminModel.UserName : adminModel.TrueName);
                                    }
                                }
                            }
                        }
                        //管理员单人
                        else if (MsgAdmin.AId > 0 && MsgAdmin.SendUsers == ",0,")
                        {
                            var adminModel = Entity.SysAdmin.FirstOrDefault(x => x.Id == MsgAdmin.AId);
                            if (adminModel != null)
                            {
                                userName.Add(adminModel.Id, string.IsNullOrWhiteSpace(adminModel.TrueName) ? adminModel.UserName : adminModel.TrueName);
                            }
                        }
                        break;
                    //代理商
                    case 1:
                        //代理商多人
                        if (MsgAdmin.AId == 0 && MsgAdmin.SendUsers != ",0,")
                        {
                            string[] Aid = MsgAdmin.SendUsers.Substring(1, MsgAdmin.SendUsers.Length - 2).Split(',');
                            foreach (var item in Aid)
                            {
                                int aid = Convert.ToInt32(item);
                                var adminModel = Entity.SysAdmin.FirstOrDefault(x => x.Id == aid);
                                if (adminModel != null)
                                {
                                    userName.Add(aid, string.IsNullOrWhiteSpace(adminModel.TrueName) ? adminModel.UserName : adminModel.TrueName);
                                }
                            }
                        }
                        else if (MsgAdmin.AId > 0 && MsgAdmin.SendUsers == ",0,")
                        {
                            var adminModel = Entity.SysAdmin.FirstOrDefault(x => x.Id == MsgAdmin.AId);
                            if (adminModel != null)
                            {
                                userName.Add(MsgAdmin.AId, string.IsNullOrWhiteSpace(adminModel.TrueName) ? adminModel.UserName : adminModel.TrueName);
                            }
                        }
                        break;
                }
            }
            ViewBag.MsgAdmin = MsgAdmin;
            //所有管理员
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.AgentId == BasicAgent.Id && n.State == 1).ToList();
            //所有的代理商
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.AgentID == BasicAgent.Id).ToList();
            ViewBag.UserName = userName;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        /// <summary>
        /// 添加操作
        /// </summary>
        /// <param name="MsgAdmin"></param>
        /// <param name="IsObj">发送对象</param>
        /// <param name="IsAdmin">选择发送的管理员</param>
        /// <param name="IsPId">选择发送的代理商</param>
        /// <param name="IsLevel">是否包含下级</param>
        [ValidateInput(false)]
        public void Add(MsgAdmin MsgAdmin, int? IsObj, int? IsAdmin, int? IsPId, int? IsLevel)
        {
            string sendUsers = "";
            //获取发送对象
            if (IsObj.HasValue)
            {
                switch (IsObj)
                {
                    case 0://管理员
                        MsgAdmin.IsAdmin = 0;
                        #region  给管理员发送
                        if (IsAdmin.HasValue)
                        {
                            if (IsAdmin.Value == 0)
                            {
                                //选择了所有的管理员
                                IList<SysAdmin> ListAdmin = BasicAgent.GetAgentAdmins(Entity);//获取所有的管理员
                                if (ListAdmin.Count > 1)
                                {
                                    MsgAdmin.AId = 0;
                                    foreach (var item in ListAdmin)
                                    {
                                        sendUsers += item.Id + ",";
                                    }
                                }
                                else
                                {
                                    MsgAdmin.AId = ListAdmin[0].Id;
                                }
                            }
                            else
                            {
                                //如果选择了单个管理员
                                MsgAdmin.AId = IsAdmin.Value;
                            }
                        }
                        #endregion
                        break;
                    case 1:  //代理商
                        MsgAdmin.IsAdmin = 1;
                        #region 给代理商发送
                        int isLevel = IsLevel.HasValue ? IsLevel.Value : 0;

                        if (IsPId.HasValue)
                        {
                            if (IsPId.Value == 0)//如果选择的所有代理商
                            {
                                IList<SysAgent> ListAgent = BasicAgent.GetSupAgent(Entity);
                                if (isLevel == 1)
                                {
                                    //包含下级
                                    ListAgent = ListAgent.GetSupAgent(Entity);
                                }
                                IList<SysAdmin> modelAdmin = ListAgent.GetAgentAdmins(Entity);
                                if (modelAdmin.Count > 1)
                                {
                                    MsgAdmin.AId = 0;
                                    foreach (var adminItem in modelAdmin)
                                    {
                                        sendUsers += adminItem.Id + ",";
                                    }
                                }
                                else
                                {
                                    MsgAdmin.AId = modelAdmin[0].Id;
                                }
                            }
                            else  //如果选择的单个代理商
                            {
                                SysAgent agentModel = Entity.SysAgent.FirstOrDefault(x => x.Id == IsPId);
                                IList<SysAdmin> modelAdmin = null;
                                if (isLevel == 0)  //不包含下级
                                {
                                    modelAdmin = agentModel.GetAgentAdmins(Entity);
                                }
                                else  //包含下级
                                {
                                    //获取下级代理商
                                    IList<SysAgent> ListAgent = agentModel.GetSupAgent(Entity);
                                    modelAdmin = ListAgent.GetAgentAdmins(Entity);
                                }
                                if (modelAdmin.Count > 1)
                                {
                                    MsgAdmin.AId = 0;
                                    foreach (var item in modelAdmin)
                                    {
                                        sendUsers += item.Id + ",";
                                    }
                                }
                                else
                                {
                                    MsgAdmin.AId = modelAdmin[0].Id;
                                }
                            }
                        }
                        #endregion
                        break;
                }
            }
            MsgAdmin.PId = AdminUser.Id;
            MsgAdmin.SendUsers = !string.IsNullOrWhiteSpace(sendUsers) ? "," + sendUsers : ",0,";
            MsgAdmin.Name = MsgAdmin.Name;
            MsgAdmin.Info = MsgAdmin.Info;
            MsgAdmin.State = MsgAdmin.State;
            MsgAdmin.AddTime = DateTime.Now;
            Entity.MsgAdmin.AddObject(MsgAdmin);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(MsgAdmin MsgAdmin)
        {
            MsgAdmin baseMsgAdmin = Entity.MsgAdmin.FirstOrDefault(n => n.Id == MsgAdmin.Id && n.PId == AdminUser.Id);
            if (baseMsgAdmin == null)
            {
                Response.Redirect("/Agent/home/error.html?msg=无权限修改~");
                return;
            }
            MsgAdmin.SendUsers = baseMsgAdmin.SendUsers;
            MsgAdmin.AId = baseMsgAdmin.AId;
            baseMsgAdmin = Request.ConvertRequestToModel<MsgAdmin>(baseMsgAdmin, MsgAdmin);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(MsgAdmin MsgAdmin, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgAdmin.Id.ToString(); }
            int Ret = Entity.ChangeEntity<MsgAdmin>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(MsgAdmin MsgAdmin, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = MsgAdmin.Id.ToString(); }
            MsgAdmin baseMsgAdmin = Entity.MsgAdmin.FirstOrDefault(n => n.Id == MsgAdmin.Id && n.PId == AdminUser.Id);
            if (baseMsgAdmin == null)
            {
                Response.Redirect("/Agent/home/error.html?msg=无权限删除~");
                return;
            }
            int Ret = Entity.MoveToDeleteEntity<MsgAdmin>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
