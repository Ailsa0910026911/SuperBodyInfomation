using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Models;
using LokFu.Infrastructure;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
using LokFu.Base;
namespace LokFu.Areas.Mobile.Controllers
{
    public class ApplyCreditController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index(string comeurl)
        {
            ViewBag.BasicCarBrandList = Entity.BasicCarBrand.OrderBy(n => n.Letter).ThenBy(n => n.Name).ToList();
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1 && n.IsApply == 1).ToList();
            ViewBag.BasicBankALLList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            ViewBag.ComeUrl = comeurl;
            return View();
        }
        public void Add(ApplyCredit ApplyCredit, string code, string comeurl, List<int> Bank)
        {
            if (code.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("<script>alert('验证码错误');history.go(-1);</script>");
                return;
            }
            Session.ClearCheckCode();
            foreach (int p in Bank)
            {
                ApplyCredit AC = new ApplyCredit();
                AC = Request.ConvertRequestToModel<ApplyCredit>(AC, ApplyCredit);
                AC.BankId = p;
                AC.UId = BasicUsers.Id;
                AC.AId = 0;
                AC.State = 1;
                AC.AddTime = DateTime.Now;
                AC.AgentId = BasicAgent.Id;
                AC.AgentAId = AdminUser.Id;
                AC.PayState = 0;
                AC.AgentPay = 0;
                //这里是利润计算==========
                AC.Amoney = AC.GetPrice(Entity);//获取价格
                AC.AIdMoney = AC.Amoney;//总利润
                AC.AgentMoney = 0;//分支机构佣金设置为0，待分润计算后再写入
                AC.SheBao = ApplyCredit.SheBao.IsNullOrEmpty() ? string.Empty : ApplyCredit.SheBao;
                Entity.ApplyCredit.AddObject(AC);
            }
            Entity.SaveChanges();
            Response.Write("<script>location.href=\"success.html?comeurl=" + comeurl + "\";</script>");
        }
        public ActionResult Success(string comeurl)
        {
            ViewBag.ComeUrl = comeurl;
            return View();
        }
        public bool CodeIsTrue(string code)
        {
            if (code.ToUpper() != Session.GetCheckCode())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public FileContentResult SecurityCode()
        {
            var imageHelp = new ImageHelp();
            var code = imageHelp.CreateRandomCode(4).ToUpper();
            Session["LokFuCode"] = code;
            var image = imageHelp.CreateImage(code);
            var buffer = image.GetBuffer();
            image.Close();
            return File(buffer, "image/gif");
        }
        public ActionResult ApplyCreditCard(ApplyCredit ApplyCredit)
        {
            ViewBag.BasicUsers = BasicUsers;
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.CreditCardUrl != null).ToList();
            return View();
        }
        public void GetCode(string Mobile, int Agent = 0)
        {
            //if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Mobile && UBL.State == 1) != null)
            //{
            //    Response.Write("6");
            //    return;
            //}
            if (Mobile.IsNullOrEmpty())
            {
                return;
            }
            DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            //统计今天已经发送注册验证码次数
            int Times = Entity.SMSCode.Count(n => n.UId == BasicUsers.Id && n.Mobile == Mobile && n.CType == 1 && n.AddTime >= Today);
            if (Times >= SysSet.SMSTimes)
            {
                Response.Write("2");
                return;
            }
            if (Times > 0)
            { //第一次发送不获取，以节少系统资源
                SMSCode SMSCode = Entity.SMSCode.Where(n => n.UId == BasicUsers.Id && n.Mobile == Mobile && n.CType == 1 && n.AddTime >= Today).OrderByDescending(n => n.Id).FirstOrDefault();
                if (SMSCode.AddTime.AddMinutes(1) >= DateTime.Now)
                { //最后一次发送到现在不足1分钟
                    Response.Write("3");
                    return;
                }
            }
            //失效之前获取验证码
            IList<SMSCode> List = Entity.SMSCode.Where(n => n.UId == BasicUsers.Id && n.Mobile == Mobile && n.CType == 1 && n.State == 1).ToList();
            foreach (var p in List)
            {
                p.State = 0;
            }
            Entity.SaveChanges();
            //生成验证码
            string Code = Utils.RandomSMSCode(4);
            SMSCode SSC = new SMSCode();
            SSC.CType = 1;
            SSC.UId = BasicUsers.Id;
            SSC.Mobile = Mobile;
            SSC.Code = Code;
            SSC.AddTime = DateTime.Now;
            SSC.State = 1;
            Entity.SMSCode.AddObject(SSC);
            Entity.SaveChanges();
            //发送验证码
            SSC.SendSMS(SysSet, Entity);

            Response.Write("OK");
        }
        public void CheckSMSCode(string Code, string Mobile)
        {
            //if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Mobile && UBL.State == 1) != null)
            //{
            //    Response.Write("6");
            //    return;
            //}
            SMSCode SMSCode = Entity.SMSCode.OrderByDescending(n => n.Id).FirstOrDefault(n => n.UId == BasicUsers.Id && n.Mobile == Mobile && n.CType == 1 && n.Code == Code && n.State == 1);
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
            Response.Write("OK");
        }
        public void CheckUnique()
        {
            DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
            ApplyCredit ApplyCredit = Entity.ApplyCredit.FirstOrDefault(A => A.UId == BasicUsers.Id && A.AddTime > Today);
            if (ApplyCredit != null)
            {
                Response.Write("1");
            }
            else
            {
                Response.Write("OK");
            }
        }
        public void AddNew(ApplyCredit ApplyCredit, int BankId, string IDcard)
        {
            IList<SMSCode> List = Entity.SMSCode.Where(n => n.UId == BasicUsers.Id && n.Mobile == ApplyCredit.Mobile && n.CType == 1 && n.State == 1).ToList();
            foreach (var p in List)
            {
                p.State = 0;
            }
            Entity.SaveChanges();
            IList<SysAgent> parentAgents = BasicAgent.GetAgentsById(Entity);
            string Agents = "|";
            foreach (var item in parentAgents)
            {
                Agents += item.Id + "|";
            }
            ApplyCreditCard AC = new ApplyCreditCard()
            {
                AgentId = BasicAgent.Id,
                BankId = BankId,
                Uid = BasicUsers.Id,
                FirstAgentAmount = 0,
                FirstAgentAmountState = 0,
                UserName = ApplyCredit.TrueName,
                UserMobile = ApplyCredit.Mobile,
                UserIdCard = IDcard,
                FirstAgentId = BasicAgent.GetTopAgent(Entity).Id,
                Relation = Agents,
                OrderNum = DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                SettlementAmount = 0,
                SettlementState = 0,
                AddTime = DateTime.Now,
                State = 1
            };
            //已审核之前的状态不添加第二条数据 包括已审核的状态
            if (Entity.ApplyCreditCard.FirstOrDefault(ACC => ACC.BankId == BankId && ACC.UserIdCard == IDcard && ACC.State <3) == null)
            {
                Entity.ApplyCreditCard.AddObject(AC);
                Entity.SaveChanges();
            }
         
            //ApplyCredit AC = new ApplyCredit();
            //AC = Request.ConvertRequestToModel<ApplyCredit>(AC, ApplyCredit);
            //AC.BankId = BankId;
            //AC.UId = BasicUsers.Id;
            //AC.AId = 0;
            //AC.State = 1;
            //AC.AddTime = DateTime.Now;
            //AC.AgentId = BasicAgent.Id;
            //AC.AgentAId = AdminUser.Id;
            //AC.PayState = 0;
            //AC.AgentPay = 0;
            ////这里是利润计算==========
            //AC.Amoney = 0;//获取价格
            //AC.AIdMoney = AC.Amoney;//总利润
            //AC.AgentMoney = 0;//分支机构佣金设置为0，待分润计算后再写入
            //AC.Company = "";
            //AC.CompanyNature = "";
            //AC.SheBao = "";
            //AC.HasSheBao = 0;
            //AC.Marry = 0;
            //AC.HasCar = 0;
            //AC.HasCredit = 0;
            //Entity.ApplyCredit.AddObject(AC);
            //Entity.SaveChanges();
            Response.Write("<script>location.href=\"" + Entity.BasicBank.FirstOrNew(b => b.Id == BankId).CreditCardUrl + "\";</script>");
        }
    }
}
