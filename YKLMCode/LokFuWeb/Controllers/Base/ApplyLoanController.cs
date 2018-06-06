﻿using System.Linq;
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
    public class ApplyLoanController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index(string comeurl)
        {
            ViewBag.BasicCarBrandList = Entity.BasicCarBrand.OrderBy(n => n.Letter).ThenBy(n => n.Name).ToList();
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            ViewBag.ComeUrl = comeurl;
            return View();
        }
        public void Add(ApplyLoan ApplyLoan,string code,string comeurl)
        {
            if (code.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("<script>alert('验证码错误');history.go(-1);</script>");
                return;
            }
            Session.ClearCheckCode();
            ApplyLoan.UId = 0;
            ApplyLoan.AId = 0;
            ApplyLoan.State = 1;
            ApplyLoan.AddTime = DateTime.Now;
            ApplyLoan.AgentId = 0;
            ApplyLoan.AgentAId = 0;
            ApplyLoan.PayState = 0;
            ApplyLoan.AgentPay = 0;
            //这里是利润计算==========
            ApplyLoan.Amoney = ApplyLoan.GetPrice(Entity);//获取价格
            ApplyLoan.AIdMoney = ApplyLoan.Amoney;//总利润
            ApplyLoan.AgentMoney = 0;//分支机构佣金设置为0，待分润计算后再写入
            Entity.ApplyLoan.AddObject(ApplyLoan);
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
 