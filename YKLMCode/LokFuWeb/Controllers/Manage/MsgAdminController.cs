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
    public class MsgAdminController : BaseController
    {
        public ActionResult Index(MsgAdmin MsgAdmin, EFPagingInfo<MsgAdmin> p)
        {
            if (!MsgAdmin.AId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.AId == MsgAdmin.AId); }
            if (!MsgAdmin.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(MsgAdmin.Name)); }
            if (!MsgAdmin.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (MsgAdmin.State == 99 ? 0 : MsgAdmin.State)); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgAdmin> MsgAdminList = Entity.Selects<MsgAdmin>(p);
            ViewBag.MsgAdminList = MsgAdminList;
            ViewBag.MsgAdmin = MsgAdmin;
            ViewBag.SysAdminList = Entity.SysAdmin.Where(n => n.State == 1).ToList();
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.State == 1).ToList();
            ViewBag.IsMyDelete = checkPower("Delete");
            ViewBag.IsMyEdit = checkPower("Edit");
            ViewBag.IsMySave = checkPower("Save");
            ViewBag.IsMyAdd = checkPower("Add");
            return View();
        }
        public ActionResult Edit(MsgAdmin MsgAdmin)
        {
            Dictionary<int, string> userName = new Dictionary<int, string>();
            if (MsgAdmin.Id != 0)
            {
                MsgAdmin = Entity.MsgAdmin.FirstOrDefault(n => n.Id == MsgAdmin.Id);
                if (MsgAdmin == null)
                {
                    ViewBag.ErrorMsg = "数据不存在";
                    return View("Error");
                }
                switch (MsgAdmin.IsAdmin)
                {
                    //管理员
                    case 0:
                        //管理员多人
                        if (MsgAdmin.AId == 0 && !string.IsNullOrWhiteSpace(MsgAdmin.SendUsers))
                        {
                            string[] Aid = MsgAdmin.SendUsers.Substring(1, MsgAdmin.SendUsers.Length - 2).Split(',');
                            foreach (var item in Aid)
                            {
                                int aid = Convert.ToInt32(item);
                                var adminModel = Entity.SysAdmin.FirstOrDefault(x => x.Id == aid);
                                if (adminModel != null)
                                {
                                    userName.Add(adminModel.Id, string.IsNullOrWhiteSpace(adminModel.TrueName) ? adminModel.UserName : adminModel.TrueName);
                                }
                            }
                        }
                        //管理员单人
                        else if (MsgAdmin.AId > 0 && string.IsNullOrWhiteSpace(MsgAdmin.SendUsers))
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
                        if (MsgAdmin.AId == 0 && !string.IsNullOrWhiteSpace(MsgAdmin.SendUsers))
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
                        else if (MsgAdmin.AId > 0 && string.IsNullOrWhiteSpace(MsgAdmin.SendUsers))
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
            IList<SysAdmin> SysAdminList = Entity.SysAdmin.Where(n => n.AgentId == 0 && n.State == 1).ToList();
            ViewBag.SysAdminList = SysAdminList.Where(x => x.Id != AdminUser.Id).ToList();
            //所有的一级代理商
            ViewBag.SysAgentList = Entity.SysAgent.Where(n => n.Tier == 1 && n.State == 1).ToList();

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
                    case 0:  //管理员
                        MsgAdmin.IsAdmin = 0;
                        #region  给管理员发送
                        if (IsAdmin.HasValue)
                        {
                            //选择了所有的管理员
                            if (IsAdmin.Value == 0)
                            {
                                //获取所有的管理员
                                IList<SysAdmin> ListAdmin = Entity.SysAdmin.Where(n => n.AgentId == 0 && n.State == 1).ToList();
                                if (ListAdmin.Count > 1)
                                {
                                    MsgAdmin.AId = 0;
                                    foreach (var item in ListAdmin)
                                    {
                                        if (item.Id != AdminUser.Id)
                                        {
                                            sendUsers += item.Id + ",";
                                        }
                                    }
                                }
                                else
                                {
                                    MsgAdmin.AId = ListAdmin[0].Id;
                                }
                            }
                            else  //如果选择了单个管理员
                            {
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
                            if (IsPId.Value == 0)  //如果选择的所有代理商
                            {
                                IList<SysAgent> ListAgent = Entity.SysAgent.Where(n => n.Tier == 1 && n.State == 1).ToList();
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
                                    modelAdmin = Entity.SysAdmin.Where(x => x.Id == agentModel.AdminId).ToList(); //agentModel.GetAgentAdmins(Entity);
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
            MsgAdmin.SendUsers = !string.IsNullOrWhiteSpace(sendUsers) ? "," + sendUsers : sendUsers;
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
            MsgAdmin baseMsgAdmin = Entity.MsgAdmin.FirstOrDefault(n => n.Id == MsgAdmin.Id);
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
            int Ret = Entity.MoveToDeleteEntity<MsgAdmin>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
