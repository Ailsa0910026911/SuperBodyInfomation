using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
namespace LokFu.Areas.Mobile.Controllers
{
    public class RegController : BaseController
    {
        public ActionResult Index(Users Users)
        {
            //string ShareDomain = ConfigurationManager.AppSettings["ShareDomain"].ToString();
            //string Url = System.Web.HttpContext.Current.Request.Url.ToString();
            //if (Url.IndexOf(ShareDomain) == -1) {
            //    string PathAndQuery = System.Web.HttpContext.Current.Request.Url.PathAndQuery;
            //    string NewUrl = ShareDomain + PathAndQuery;
            //    Response.Redirect(NewUrl);
            //    return View("NULL");
            //}
            ViewBag.Users = Users;
            return View();
        }
        public ActionResult Money(Users Users)
        {
            //string ShareDomain = ConfigurationManager.AppSettings["ShareDomain"].ToString();
            //string Url = System.Web.HttpContext.Current.Request.Url.ToString();
            //if (Url.IndexOf(ShareDomain) == -1)
            //{
            //    string PathAndQuery = System.Web.HttpContext.Current.Request.Url.PathAndQuery;
            //    string NewUrl = ShareDomain + PathAndQuery;
            //    Response.Redirect(NewUrl);
            //    return View("NULL");
            //}
            ViewBag.Users = Users;
            return View();
        }
        public ActionResult Reg(Users Users)
        {
            ViewBag.Users = Users;
            return View();
        }
        public void Save(Users Users)
        {
            if (Users.UserName.IsNullOrEmpty() || Users.PassWord.IsNullOrEmpty() || Users.Code.IsNullOrEmpty())
            {
                Response.Write("0");
                return;
            }
            //验证是否重复
            Users Old = Entity.Users.FirstOrDefault(n => n.UserName == Users.UserName);
            if (Old != null)
            {
                Response.Write("1");
                return;
            }
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Users.UserName && UBL.State == 1) != null)
            {
                Response.Write("6");
                return;
            }
            //手机验证码
            //失效之前获取验证码
            SMSCode SMSCode = Entity.SMSCode.OrderByDescending(n => n.Id).FirstOrDefault(n => n.UId == 0 && n.Mobile == Users.UserName && n.CType == 1 && n.Code == Users.Code);
            if (SMSCode == null)
            {
                Response.Write("2");
                return;
            }
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            if (SMSCode.State != 1)
            {
                Response.Write("3");
                return;
            }
            if (SMSCode.AddTime.AddMinutes(SysSet.SMSActives) < DateTime.Now)
            {
                Response.Write("4");
                return;
            }
            Users.Agent = 1;//默认指定
            Users.AId = 2;//默认指定
            if (!Users.MyPId.IsNullOrEmpty())
            {
                Users U = Entity.Users.FirstOrDefault(n => n.Id == Users.MyPId && n.State == 1);
                if (U == null)
                {
                    Users.MyPId = 0;
                    Utils.WriteLog("Users.MyPId=" + Users.MyPId + "Is Null!", "UsersRegAgent" + DateTime.Now.ToString("yyyy-MM-dd"));
                }
                else {
                    Users.Agent = U.Agent;//默认指定
                    Users.AId = U.AId;//默认指定
                }
            }
            else {
                Users.MyPId = 0;
            }
            if (!Users.PayConfigId.IsNullOrEmpty())
            {
                PayConfigChange PCC = Entity.PayConfigChange.FirstOrDefault(n => n.Id == Users.PayConfigId && n.State == 1 && n.ShareNumber > 0);
                if (PCC == null)
                {
                    Users.PayConfigId = 0;
                }
                Users.ShareType = 2;
            }
            else {
                Users.PayConfigId = 0;
            }
            if (Users.ShareType == 2)
            {
                if (Users.PayConfigId.IsNullOrEmpty() || Users.MyPId.IsNullOrEmpty())
                {
                    //Users.MyPId = 0;
                    Users.PayConfigId = 0;
                }
            }else{
                if (Users.MyPId.IsNullOrEmpty())
                {
                    Users.MyPId = 0;
                }
                Users.PayConfigId = 0;
            }
            if (Users.MyPId == 0)
            {
                Response.Write("5");
                return;
            }

            Users.PassWord = Users.PassWord.GetMD5();
            Users.Mobile = Users.UserName;
            Users.EmailState = 0;
            Users.CardStae = 0;
            Users.State = 0;
            Users.Amount = 0;
            Users.Frozen = 0;
            Users.AddTime = DateTime.Now;
            Users.PayPwd = "";
            //Users.Agent = 1;//默认指定
            //Users.AId = 2;//默认指定
            Users.MobileState = 2;
            Users.RegAddress = "网页注册，" + Utils.GetAddressAndIp();
            Users.X = "0";
            Users.Y = "0";
            Entity.Users.AddObject(Users);
            Entity.SaveChanges();
            //=======================================
            UserTrack UserTrack = new UserTrack();
            UserTrack.ENo = string.Empty;
            UserTrack.OPType = "网页注册";
            UserTrack.IfYY = string.Empty;
            UserTrack.EqMobile = string.Empty;
            UserTrack.SysVer = Utils.GetCustomOS();//操作系统版本
            UserTrack.SoftVer = Utils.GetCustomBrowser();//软件版本
            UserTrack.SignalType = string.Empty;
            UserTrack.GPSAddress = string.Empty;
            UserTrack.GPSX = "0";
            UserTrack.GPSY = "0";
            Users.SeavGPSLog(UserTrack, Entity);
            //=======================================
            SMSCode.State = 2;
            SMSCode.UId = Users.Id;
            Entity.SaveChanges();
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

            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == Users.Agent && n.State == 1);
            if (SysAgent == null)
            {
                Utils.WriteLog("Users.Id=" + Users.Id + "Read Agent Null Users.Agent=" + Users.Agent + "!", "UsersRegAgent" + DateTime.Now.ToString("yyyy-MM-dd"));
                SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == 1 && n.State == 1);
                Users.Agent = SysAgent.Id;
                Users.AId = SysAgent.AdminId.GetValueOrDefault();
            }
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
            //Users.ECash0 = Sys.ECash0;
            //Users.Cash1 = Sys.Cash1;
            //Users.ECash1 = Sys.ECash1;

            //使用代理配置
            Users.Cash0 = SysAgent.Cash0;
            Users.Cash1 = SysAgent.Cash1;
            Users.ECash0 = SysAgent.ECash0;
            Users.ECash1 = SysAgent.ECash1;

            Users.State = 1;
            Entity.SaveChanges();
            //自动开通End
            Response.Write("OK");
        }
        public void GetCode(string UserName, string tuCode, int Agent = 0)
        {
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == UserName && UBL.State == 1) != null)
            {
                Response.Write("6");
                return;
            }
            if (tuCode.IsNullOrEmpty())
            {
                Response.Write("4");
                return;
            }
            if (tuCode.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("4");
                return;
            }
            Session.ClearCheckCode();
            if (UserName.IsNullOrEmpty())
            {
                return;
            }
            //验证是否重复
            Users Old = Entity.Users.FirstOrDefault(n => n.UserName == UserName);
            if (Old != null)
            {
                Response.Write("1");
                return;
            }
            DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            //统计今天已经发送注册验证码次数
            int Times = Entity.SMSCode.Count(n => n.UId == 0 && n.Mobile == UserName && n.CType == 1 && n.AddTime >= Today);
            if (Times >= SysSet.SMSTimes)
            {
                Response.Write("2");
                return;
            }
            if (Times > 0)
            { //第一次发送不获取，以节少系统资源
                SMSCode SMSCode = Entity.SMSCode.Where(n => n.UId == 0 && n.Mobile == UserName && n.CType == 1 && n.AddTime >= Today).OrderByDescending(n => n.Id).FirstOrDefault();
                if (SMSCode.AddTime.AddMinutes(1) >= DateTime.Now)
                { //最后一次发送到现在不足1分钟
                    Response.Write("3");
                    return;
                }
            }
            //失效之前获取验证码
            IList<SMSCode> List = Entity.SMSCode.Where(n => n.UId == 0 && n.Mobile == UserName && n.CType == 1 && n.State == 1).ToList();
            foreach (var p in List)
            {
                p.State = 0;
            }
            Entity.SaveChanges();
            //生成验证码
            string Code = Utils.RandomSMSCode(4);
            SMSCode SSC = new SMSCode();
            SSC.CType = 1;
            SSC.UId = 0;
            SSC.Mobile = UserName;
            SSC.Code = Code;
            SSC.AddTime = DateTime.Now;
            SSC.State = 1;
            Entity.SMSCode.AddObject(SSC);
            Entity.SaveChanges();

            SysAgent SA = Entity.SysAgent.FirstOrNew(n => n.Id == Agent);
            SA = SA.GetTopAgent(Entity);
            //发送验证码
            SSC.SendSMS(SysSet, SA, Entity);

            Response.Write("OK");
        }
    }
}
 