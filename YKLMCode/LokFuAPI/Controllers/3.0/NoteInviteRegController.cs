using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.Repositories;
using LokFu.Extensions;
using System.Text;
using System.Security.Cryptography;
using System.Data.Objects.SqlClient;

namespace LokFu.Controllers
{
    /// <summary>
    /// 短信邀请(短信邀请自动注册)
    /// </summary>
    public class NoteInviteRegController : InitController
    {
        public NoteInviteRegController()
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
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            if (Users.Mobile.IsNullOrEmpty())
            {
                //
                DataObj.OutError("1000");
                return;
            }

            //验证是否重复
            Users Old = Entity.Users.FirstOrDefault(n => n.Mobile == Users.Mobile);
            if (Old != null)
            {
                DataObj.OutError("2005");
                return;
            }
            //手机号码黑名单验证
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.Mobile && UBL.State == 1) != null)
            {
                //提示暂不支持该手机号入网
                DataObj.OutError("2035");
                return;
            }
            Users baseUsers = null;
            if (Users.Token.IndexOf("Print|") == -1)
            {
                baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            }
            else {
                baseUsers = Entity.Users.FirstOrDefault(n => n.PrintToken == Users.Token);
            }
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
                //DataObj.OutError("2008");
                //return;
            }

            int IntervalCountAll = Entity.Users.Count(o => o.MyPId == baseUsers.Id && o.ShareType == 4);//统计所有邀请人数
            if (IntervalCountAll > 20)
            {
                //20人以上，验证其认证通过率
                int IntervalCountRZ = Entity.Users.Count(o => o.MyPId == baseUsers.Id && o.ShareType == 4 && o.CardStae == 2);//统计所有邀请并认证人数
                if (IntervalCountRZ * 10 < IntervalCountAll)
                {
                    DataObj.OutError("2102");
                    return;
                }
            }

            SysSet SysSet = this.Entity.SysSet.FirstOrNew();
            DateTime IntervalTime = DateTime.Now.AddSeconds(0 - SysSet.YaoQingIntervalTime);
            int IntervalCount = this.Entity.Users.Where(o => o.MyPId == baseUsers.Id && o.ShareType == 4 && o.AddTime > IntervalTime).Count();
            if (IntervalCount >= SysSet.YaoQingIntervalNumber)
            {
                DataObj.OutError("2101");
                return;
            }

            //邀请注册
            Users inviteUsers = new Users();
            inviteUsers.UserName = Users.Mobile;
            inviteUsers.Mobile = Users.Mobile;
            inviteUsers.Agent = baseUsers.Agent;
            inviteUsers.AId = baseUsers.AId;
            inviteUsers.MyPId = baseUsers.Id;

            inviteUsers.ShareType = 4;
            inviteUsers.RegAddress = "短信邀请";
            inviteUsers.X = "0";
            inviteUsers.Y = "0";

            string PassWord = Utils.GetCode();
            inviteUsers.PassWord = PassWord.GetMD5();

            inviteUsers.MobileState = 2;
            inviteUsers.EmailState = 0;
            inviteUsers.CardStae = 0;
            inviteUsers.State = 0;
            inviteUsers.Amount = 0;
            inviteUsers.Frozen = 0;
            inviteUsers.AddTime = DateTime.Now;
            inviteUsers.PayPwd = "";
            
            Entity.Users.AddObject(inviteUsers);

            //=======================================
            UserTrack UserTrack = new UserTrack();
            UserTrack.ENo = string.Empty;
            UserTrack.OPType = "邀请注册";
            UserTrack.IfYY = string.Empty;
            UserTrack.EqMobile = string.Empty;
            UserTrack.SysVer = string.Empty;
            UserTrack.SoftVer = string.Empty;
            UserTrack.SignalType = string.Empty;
            UserTrack.GPSAddress = string.Empty;
            UserTrack.GPSX = "0";
            UserTrack.GPSY = "0";
            Users.SeavGPSLog(UserTrack, Entity);
            //=======================================

            //自动开通
            //IList<PayConfig> PCList = Entity.PayConfig.Where(n => n.State == 1).ToList();
            //foreach (var PC in PCList)
            //{
            //    UserPay UserPay = new UserPay();
            //    UserPay.UId = inviteUsers.Id;
            //    UserPay.PId = PC.Id;
            //    UserPay.Cost = (double)PC.CostUser;
            //    Entity.UserPay.AddObject(UserPay);
            //}

            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == inviteUsers.Agent && n.State == 1);
            if (SysAgent == null) {
                SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == 1 && n.State == 1);
                inviteUsers.Agent = SysAgent.Id;
                inviteUsers.AId = SysAgent.AdminId.GetValueOrDefault();
            }
            //使用代理配置
            IList<UserPayAgent> UPAList = Entity.UserPayAgent.Where(n => n.AId == SysAgent.Id).OrderBy(n => n.PId).ToList();
            foreach (var p in UPAList)
            {
                UserPay UserPay = new UserPay();
                UserPay.UId = inviteUsers.Id;
                UserPay.PId = p.PId;
                UserPay.Cost = p.Cost;
                Entity.UserPay.AddObject(UserPay);
            }

            SysSet Sys = Entity.SysSet.FirstOrDefault();
            //inviteUsers.Cash0 = Sys.Cash0;
            //inviteUsers.ECash0 = Sys.ECash0;
            //inviteUsers.Cash1 = Sys.Cash1;
            //inviteUsers.ECash1 = Sys.ECash1;
            //使用代理配置
            inviteUsers.Cash0 = SysAgent.Cash0;
            inviteUsers.Cash1 = SysAgent.Cash1;
            inviteUsers.ECash0 = SysAgent.ECash0;
            inviteUsers.ECash1 = SysAgent.ECash1;

            inviteUsers.State = 1;
            Entity.SaveChanges();
            //自动开通End

            //开通后发送短信
            string NoteDownload = "https://fir.im/he3q";
            string CompanyName = Sys.Name;
            //查询根代理
            //var CurrentAgent = this.Entity.SysAgent.FirstOrNew(o => o.Id == baseUsers.Agent);//当前代理
            //if (CurrentAgent.Tier == 1)
            //{
            //    if (CurrentAgent.IsTeiPai == 1) {
            //        CompanyName = CurrentAgent.APPName;
            //        NoteDownload = CurrentAgent.NoteDownload.IsNullOrEmpty() ? NoteDownload : CurrentAgent.NoteDownload;
            //    }
            //}
            //else
            //{
            //    var TreeAgent = CurrentAgent.GetAgentsById(Entity);
            //    SysAgent RootAgent = TreeAgent.FirstOrDefault(o => o.Tier == 1);
            //    if (RootAgent.IsTeiPai == 1)
            //    {
            //        CompanyName = RootAgent.APPName;
            //        NoteDownload = RootAgent.NoteDownload.IsNullOrEmpty() ? NoteDownload : RootAgent.NoteDownload;
            //    }
            //}

            //根代理
            //string SendText = "{2}邀请您使用{3}，您的账号{0}已开通，登陆密码{1}，登陆后请修改登陆密码和尽快实名认证！APP下载地址{4}。";
            //SendText = string.Format(SendText, inviteUsers.UserName, PassWord, baseUsers.Mobile, CompanyName, NoteDownload);
            string SendText = "您的账号{0}已开通，登陆密码{1}，登录后请修改登录密码和尽快实名认证！APP下载地址{2}。";
            SendText = string.Format(SendText, inviteUsers.UserName, PassWord, NoteDownload);
            SMSLog SMSLog = new SMSLog();
            SMSLog.SendText = SendText;
            SMSLog.Mobile = inviteUsers.UserName;
            SMSLog.UId = baseUsers.Id;
            
            SysSet ss = new SysSet();
            ss.SMSEnd = Sys.SMSEnd;
            SysAgent SA = Entity.SysAgent.FirstOrNew(n => n.Id == baseUsers.Agent);
            SA = SA.GetTopAgent(Entity);
            SMSLog.SendSMS(ss, SA, Entity);

            //邀请注册end
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
