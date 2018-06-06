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
    public class SpreadUsersController : InitController
    {
        public SpreadUsersController()
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
            //判断是否为代理商
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(o => o.LinkMobile == baseUsers.UserName && o.State == 1);
            IList<SysAgent> SysAgentList = null;
            IList<Users> UsersList = null;
            SysSet baseSysSet=Entity.SysSet.FirstOrDefault();
            //1用户2代理商
            if (SysAgent != null)
            {
                baseUsers.UserType = 2;
            }
            else
            {
                baseUsers.UserType = 1;
            }
            //if (baseUsers.UserType == 1)
            //{
                UsersList = baseUsers.GetSupUsers(Entity, baseSysSet.GlobaPromoteMaxLevel);
                UsersList = UsersList.Where(o => o.Id != baseUsers.Id).ToList();
                baseUsers.UserTotal = UsersList.Count();
           // }
            if (baseUsers.UserType == 2)
            {
                SysAgentList = SysAgent.GetSupAgent(Entity);
                IList<int> agents = SysAgentList.Where(o => o.Id != SysAgent.Id).Select(o => o.Id).ToList();
               // UsersList = Entity.Users.Where(o => agents.Contains(o.Agent) && o.Id != baseUsers.Id).ToList();
                //baseUsers.UserTotal = UsersList.Count();
                //SysAgentList = SysAgentList.Where(o => o.Id != SysAgent.Id).ToList();
                baseUsers.AgentTotal = agents.Count();
            }
            baseUsers.Cols = "Id,UserName,UserType,UserTotal,AgentTotal";
            DataObj.Data = baseUsers.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }

    }
}
