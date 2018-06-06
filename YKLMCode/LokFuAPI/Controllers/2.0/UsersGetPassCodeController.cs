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


namespace LokFu.Controllers
{
    public class UsersGetPassCodeController : InitController
    {
        public UsersGetPassCodeController()
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
                Log.Write("[UsersGetPass]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;

            //身份证号，手机号（登录帐户），邮箱
            //CardId,UserName,Email

            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.UserName.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            ////手机号码黑名单验证
            //if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.UserName && UBL.State == 1) != null)
            //{
            //    //提示暂不支持您手机号入网
            //    DataObj.OutError("2026");
            //    return;
            //}
            Users BaseUsers = Entity.Users.Where(n => n.UserName == Users.UserName).FirstOrDefault();
            if (BaseUsers == null)//用户不存在
            {
                DataObj.OutError("2001");
                return;
            }
            if (BaseUsers.State != 1)
            {
                DataObj.OutError("2003");
                return;
            }
            //if (BaseUsers.CardStae != 2)//未实名认证
            //{
            //    DataObj.OutError("2006");
            //    return;
            //}

            DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            //统计今天已经发送注册验证码次数
            int Times = Entity.SMSCode.Count(n => n.UId == BaseUsers.Id && n.Mobile == BaseUsers.UserName && n.CType == 2 && n.AddTime >= Today);
            if (Times >= SysSet.SMSTimes)
            {
                DataObj.OutError("2031");
                return;
            }
            if (Times > 0)
            { //第一次发送不获取，以节少系统资源
                SMSCode SMSCode = Entity.SMSCode.Where(n => n.UId == BaseUsers.Id && n.Mobile == BaseUsers.UserName && n.CType == 2 && n.AddTime >= Today).OrderByDescending(n => n.Id).FirstOrDefault();
                if (SMSCode.AddTime.AddMinutes(1) >= DateTime.Now)
                { //最后一次发送到现在不足1分钟
                    DataObj.OutError("2032");
                    return;
                }
            }
            //失效之前获取验证码
            IList<SMSCode> List = Entity.SMSCode.Where(n => n.UId == BaseUsers.Id && n.Mobile == BaseUsers.UserName && n.CType == 2 && n.State == 1).ToList();
            foreach (var p in List)
            {
                p.State = 0;
            }
            Entity.SaveChanges();

            //生成验证码
            string Code = Utils.RandomSMSCode(6);

            SMSCode SSC = new SMSCode();
            SSC.CType = 2;
            SSC.UId = BaseUsers.Id;
            SSC.Mobile = BaseUsers.UserName;
            SSC.Code = Code;
            SSC.AddTime = DateTime.Now;
            SSC.State = 1;
            Entity.SMSCode.AddObject(SSC);
            Entity.SaveChanges();

            SysAgent SA = Entity.SysAgent.FirstOrNew(n => n.Id == BaseUsers.Agent);
            SA = SA.GetTopAgent(Entity);
            //发送验证码
            SSC.SendSMS(SysSet, SA, Entity);

            BaseUsers.Cols = "CardStae";
            DataObj.Data = BaseUsers.OutJson();

            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
