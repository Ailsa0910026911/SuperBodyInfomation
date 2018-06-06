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
    public class UsersReg_2_0Controller : InitController
    {
        public UsersReg_2_0Controller()
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
                Log.Write("[UsersReg_2_0]:", "【Data】" + Data, Ex);
            }
            if (json == null) {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);

            UserTrack UserTrack = new UserTrack();
            UserTrack = JsonToObject.ConvertJsonToModel(UserTrack, json);

            if (Users.UserName.IsNullOrEmpty() || Users.PassWord.IsNullOrEmpty() || Users.Mobile.IsNullOrEmpty() || Users.X.IsNullOrEmpty() || Users.Y.IsNullOrEmpty()) { 
                //
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
            //验证是否重复
            Users Old = Entity.Users.FirstOrDefault(n => n.UserName == Users.UserName);
            if (Old != null)
            {
                DataObj.OutError("2005");
                return;
            }

            //Card Card = Entity.Card.FirstOrDefault(n => n.Code == Users.CardNum && n.PasWd == Users.CardPWD);
            //if (Card == null) {
            //    DataObj.OutError("5001");
            //    return;
            //}
            //if (Card.State != 1) {
            //    DataObj.OutError("5002");
            //    return;
            //}
            //if (Card.AId.IsNullOrEmpty())
            //{
            //    DataObj.OutError("5002");
            //    return;
            //}
            //if (Card.AdminId.IsNullOrEmpty())
            //{
            //    DataObj.OutError("5002");
            //    return;
            //}

            //手机验证码
            //失效之前获取验证码
            SMSCode SMSCode = Entity.SMSCode.OrderByDescending(n=>n.Id).FirstOrDefault(n => n.UId == 0 && n.Mobile == Users.UserName && n.CType == 1 && n.Code == Users.Code);
            if (SMSCode == null) {
                DataObj.OutError("2033");
                return;
            }
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            if (SMSCode.State != 1) {
                DataObj.OutError("2034");
                return;
            }
            if (SMSCode.AddTime.AddMinutes(SysSet.SMSActives) < DateTime.Now) {
                DataObj.OutError("2034");
                return;
            }

            SysAgent SysAgent = null;
            //处理代理商
            if (!Users.Agent.IsNullOrEmpty())
            {
                SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == Users.Agent);
            }
            if (SysAgent == null)
            {
                SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == 1);
            }
            if (SysAgent == null)
            {
                SysAgent = Entity.SysAgent.Where(n => n.State == 1 && n.Tier == 1).OrderBy(n => n.Id).FirstOrDefault();
            }
            if (SysAgent != null)
            {
                Users.Agent = SysAgent.Id;
                Users.AId = SysAgent.AdminId.GetValueOrDefault();
            }

            Users.PassWord = Users.PassWord.GetMD5();
            Users.MobileState = 0;
            Users.EmailState = 0;
            Users.CardStae = 0;
            Users.State = 0;
            Users.Amount = 0;
            Users.Frozen = 0;
            Users.AddTime = DateTime.Now;

            Users.PayPwd = "";

            Users.MobileState = 2;

            Entity.Users.AddObject(Users);
            Entity.SaveChanges();

            //=======================================
            UserTrack.ENo = DataObj.ENo;
            UserTrack.OPType = "应用注册";
            UserTrack.GPSAddress = Users.RegAddress;
            UserTrack.GPSX = Users.X;
            UserTrack.GPSY = Users.Y;
            Users.SeavGPSLog(UserTrack, Entity);
            //=======================================

            SMSCode.State = 2;
            SMSCode.UId = Users.Id;
            Entity.SaveChanges();
            //if (!Users.Id.IsNullOrEmpty()) {
            //    Card.State = 2;
            //    Entity.SaveChanges();
            //}

            //自动开通
            //IList<PayConfig> PCList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            //foreach (var PC in PCList)
            //{
            //    UserPay UserPay = new UserPay();
            //    UserPay.UId = Users.Id;
            //    UserPay.PId = PC.Id;
            //    UserPay.Cost = (double)PC.CostUser;
            //    Entity.UserPay.AddObject(UserPay);
            //}
            //使用代理配置

            IList<UserPayAgent> UPAList = Entity.UserPayAgent.Where(n => n.AId == SysAgent.Id).OrderBy(n => n.PId).ToList();
            foreach (var p in UPAList)
            {
                UserPay UserPay = new UserPay();
                UserPay.UId = Users.Id;
                UserPay.PId = p.PId;
                UserPay.Cost = p.Cost;
                Entity.UserPay.AddObject(UserPay);
            }

            //SysSet Sys = Entity.SysSet.FirstOrDefault();
            //Users.Cash0 = Sys.Cash0;
            //Users.Cash1 = Sys.Cash1;
            //Users.ECash0 = Sys.ECash0;
            //Users.ECash1 = Sys.ECash1;
            //使用代理配置
            Users.Cash0 = SysAgent.Cash0;
            Users.Cash1 = SysAgent.Cash1;
            Users.ECash0 = SysAgent.ECash0;
            Users.ECash1 = SysAgent.ECash1;

            Users.State = 1;
            Entity.SaveChanges();
            //自动开通End
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
            //Tools.OutString(ErrInfo.Return("0000"));
        }
    }
}
