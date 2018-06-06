using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories.SqlServer;
namespace LokFu.Controllers
{
    public class UsersListController : InitController
    {
        public UsersListController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[Users]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            //获取用户信息
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }
            if (Users.UserType != 1 && Users.UserType != 2)
            {
                DataObj.OutError("9002");
                return;
            }

            //判断是否为代理商
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(o => o.LinkMobile == baseUsers.UserName && o.State == 1);
            IList<SysAgent> SysAgentList = null;
            IList<Users> UsersList = null;
            SysSet baseSysSet = Entity.SysSet.FirstOrDefault();
            //1用户2代理商
            if (SysAgent == null && Users.UserType == 2) 
            {
                DataObj.OutError("9003");
                return;
            }
            //if (Users.UserType == 1 && SysAgent!=null)
            //{
            //    UsersList = baseUsers.GetSupUsers(Entity, baseSysSet.GlobaPromoteMaxLevel);
            //    SysAgentList = SysAgent.GetSupAgent(Entity);
            //    IList<int> agents = SysAgentList.Select(o => o.Id).ToList();
            //    IList<Users> AgentUsersList = Entity.Users.Where(o => agents.Contains(o.Agent)).ToList();
            //    UsersList = UsersList.Concat(AgentUsersList).ToList();
            //}
            if (Users.UserType == 1)
            {
                UsersList = baseUsers.GetSupUsers(Entity, baseSysSet.GlobaPromoteMaxLevel);
            }
            if (Users.UserType == 2)
            {
                SysAgentList = SysAgent.GetSupAgent(Entity);
                //IList<int> agents = SysAgentList.Select(o => o.Id).ToList();
                //UsersList = Entity.Users.Where(o => agents.Contains(o.Agent)).ToList();
                IList<string> agents = SysAgentList.Select(o => o.LinkMobile).ToList();
                UsersList = Entity.Users.Where(o => agents.Contains(o.UserName)).ToList();
            }
           
            EFPagingInfo<Users> p = new EFPagingInfo<Users>();
            if (!Users.Pg.IsNullOrEmpty()) { p.PageIndex = Users.Pg; }
            if (!Users.Pgs.IsNullOrEmpty()) { p.PageSize = Users.Pgs; }
            if (UsersList == null)
            {
                p.SqlWhere.Add(o => o.Id == 0);
                p.OrderByList.Add("Id", "DESC");
                IPageOfItems<Users> List1 = Entity.Selects<Users>(p);
                IList<Users> iList1 = List1.ToList();
                StringBuilder sb1 = new StringBuilder("");
                sb1.Append("{");
                sb1.Append(List1.PageToString());
                sb1.Append(",");
                sb1.Append(iList1.EntityToString());
                sb1.Append("}");
                DataObj.Data = sb1.ToString();
                DataObj.Code = "0000";
                DataObj.OutString();
                return;
            }
            IList<int> usersid = UsersList.Where(o => o.Id != baseUsers.Id).Select(o => o.Id).ToList();
            p.SqlWhere.Add(f => usersid.Contains(f.Id));
            p.SqlWhere.Add(f => f.Id != baseUsers.Id);
            p.OrderByList.Add("Agent", "ASC");

            IPageOfItems<Users> List = Entity.Selects<Users>(p);

            IList<Users> iList = List.ToList();
            foreach (var info in iList)
            {
               
                if (Users.UserType == 1)
                {
                    if (info.MyPId != baseUsers.Id)
                    {
                        info.TrueName = info.TrueName.IsNullOrEmpty() ? "未认证" : info.TrueName.Substring(0, 1) + "**";
                        info.UserName = info.UserName.Substring(0, 3) + "****" + info.UserName.Substring(7, 4);
                    }
                    else
                    {
                        info.TrueName = info.TrueName.IsNullOrEmpty() ? "未认证" : info.TrueName;
                    }
                }
                if (Users.UserType == 2)
                {
                    SysAgent agentTemp = Entity.SysAgent.FirstOrDefault(o => o.Id == info.Agent);
                    if (agentTemp == null || agentTemp.AgentID != SysAgent.Id)
                    {
                        info.TrueName = info.TrueName.IsNullOrEmpty() ? "未认证" : info.TrueName.Substring(0, 1) + "**";
                        info.UserName = info.UserName.Substring(0, 3) + "****" + info.UserName.Substring(7, 4);
                    }
                    else
                    {
                        info.TrueName = info.TrueName.IsNullOrEmpty() ? "未认证" : info.TrueName;
                    }
                }
                
            }
            StringBuilder sb = new StringBuilder("");
            sb.Append("{");
            sb.Append(List.PageToString());
            sb.Append(",");
            sb.Append(iList.EntityToString());
            sb.Append("}");
            DataObj.Data = sb.ToString();
            DataObj.Code = "0000";
            DataObj.OutString();
        }

    }
}
