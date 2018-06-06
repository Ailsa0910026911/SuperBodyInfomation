using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using LokFu.Extensions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace LokFu.Controllers
{
    public class UsersPassWordChkController : InitController
    {
        public UsersPassWordChkController()
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
                Log.Write("[UsersPassWrodChk]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.LoginLock == 1)//临时锁定
            {
                DataObj.OutError("2103");
                return;
            }

            if (baseUsers.PassWord != Users.PassWord.GetMD5())
            {
                SysSet SysSet = Entity.SysSet.FirstOrNew();
                //系统统一修改标识SAME001
                baseUsers.LoginErr++;
                if (baseUsers.LoginErr >= SysSet.LoginLock)
                {
                    baseUsers.Token = "Lock";//锁定退出
                    baseUsers.LoginLock = 1;
                }
                Entity.SaveChanges();

                Users Out = new Users();
                Out.LoginErr = SysSet.LoginLock - baseUsers.LoginErr;
                Out.Cols = "LoginErr";
                DataObj.Data = Out.OutJson();

                DataObj.Code = "2002";
                if (Out.LoginErr == 0)
                {
                    DataObj.Msg = "帐号或密码不正确，请明日再试或取回登录密码";
                }
                else
                {
                    DataObj.Msg = "帐号或密码不正确，您还可以尝试" + Out.LoginErr + "次";
                }
                DataObj.OutString();

                return;
            }
            baseUsers.LoginErr = 0;
            Entity.SaveChanges();
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
            //Tools.OutString(ErrInfo.Return("0000"));
        }
    }
}
