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
    public class UsersReplaceMobileCodeController : InitController
    {
        public UsersReplaceMobileCodeController()
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
                Log.Write("[UsersReplaceMobileCode]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }

            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.UserName.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            //手机号码黑名单验证
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.UserName && UBL.State == 1) != null)
            {
                //提示暂不支持您手机号入网
                DataObj.OutError("2026");
                return;
            }
            Users BaseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (BaseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (BaseUsers.SAId != 0) {
                DataObj.OutError("2054");
                return;
            }
            if (BaseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (BaseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }

            Users oldUsers = Entity.Users.FirstOrDefault(n => n.UserName == Users.UserName);
            if (oldUsers != null)//用户已存在
            {
                DataObj.OutError("2005");
                return;
            }

            DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            //统计今天已经发送验证码次数
            int Times = Entity.SMSCode.Count(n => n.UId == BaseUsers.Id && n.CType == 4 && n.AddTime >= Today);
            if (Times >= SysSet.SMSTimes)
            {
                DataObj.OutError("2031");
                return;
            }
            if (Times > 0)
            { //第一次发送不获取，以节少系统资源
                SMSCode SMSCode = Entity.SMSCode.Where(n => n.UId == BaseUsers.Id && n.CType == 4 && n.AddTime >= Today).OrderByDescending(n => n.Id).FirstOrDefault();
                if (SMSCode.AddTime.AddMinutes(1) >= DateTime.Now)
                { //最后一次发送到现在不足1分钟
                    DataObj.OutError("2032");
                    return;
                }
            }
            //失效之前获取验证码
            IList<SMSCode> List = Entity.SMSCode.Where(n => n.UId == BaseUsers.Id && n.CType == 4 && n.State == 1).ToList();
            foreach (var p in List)
            {
                p.State = 0;
            }
            Entity.SaveChanges();

            //生成验证码
            string Code = Utils.RandomSMSCode(6);

            SMSCode SSC = new SMSCode();
            SSC.CType = 4;
            SSC.UId = BaseUsers.Id;
            SSC.Mobile = Users.UserName;
            SSC.Code = Code;
            SSC.AddTime = DateTime.Now;
            SSC.State = 1;
            Entity.SMSCode.AddObject(SSC);
            Entity.SaveChanges();

            SysAgent SA = Entity.SysAgent.FirstOrNew(n => n.Id == BaseUsers.Agent);
            SA = SA.GetTopAgent(Entity);
            //发送验证码
            SSC.SendSMS(SysSet, SA, Entity);

            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
