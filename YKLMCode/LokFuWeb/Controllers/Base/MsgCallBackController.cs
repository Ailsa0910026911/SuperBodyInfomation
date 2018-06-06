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
    public class MsgCallBackController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index(string comeurl)
        {
            //ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            //ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            this.Session["comeurl"] = comeurl;
            return View();
        }
        public void Add(MsgCallBack MsgCallBack, string code)
        {
            if (code.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("<script>alert('验证码错误');history.go(-1);</script>");
                return;
            }
            Session.ClearCheckCode();
            MsgCallBack.State = 1;
            MsgCallBack.AddTime = DateTime.Now;
            MsgCallBack.NeekName = "匿名";
            Entity.MsgCallBack.AddObject(MsgCallBack);
            Entity.SaveChanges();
            string comeurl = this.Session["comeurl"].ToString();
            Response.Write("<script>alert(\"提交成功~！\");location.href=\"/MsgCallBack/?comeurl=" + comeurl + "\";</script>");
        }
        public ActionResult Success(string comeurl)
        {
            ViewBag.ComeUrl = comeurl;
            return View();
        }
    }
}
 