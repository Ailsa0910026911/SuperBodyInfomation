using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Models;
using LokFu.Infrastructure;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
namespace LokFu.Areas.Base.Controllers
{
    public class ApplyCreditController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index(string comeurl)
        {
            ViewBag.BasicCarBrandList = Entity.BasicCarBrand.OrderBy(n => n.Letter).ThenBy(n => n.Name).ToList();
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1 && n.IsApply==1).ToList();
            ViewBag.BasicBankALLList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            ViewBag.ComeUrl = comeurl;
            return View();
        }
        public void Add(ApplyCredit ApplyCredit,string code,string comeurl,List<int> Bank)
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
                AC.UId = 0;
                AC.AId = 0;
                AC.State = 1;
                AC.AddTime = DateTime.Now;
                AC.AgentId = 0;
                AC.AgentAId = 0;
                AC.PayState = 0;
                AC.AgentPay = 0;
                AC.CompanyNature = string.Empty;
                //这里是利润计算==========
                AC.Amoney = AC.GetPrice(Entity);//获取价格
                AC.AIdMoney = AC.Amoney;//总利润
                AC.AgentMoney = 0;//分支机构佣金设置为0，待分润计算后再写入
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
    }
}
 