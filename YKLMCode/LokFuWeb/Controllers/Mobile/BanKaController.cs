using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Models;
using LokFu.Infrastructure;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
using System.Collections;
namespace LokFu.Areas.Mobile.Controllers
{
    public class BanKaController : BaseController
    {
        public object Info(BanKaList BanKaList)
        {
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == BanKaList.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                ViewBag.ErrorMsg = "用户登录信息有误";
                return View("Error");
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                ViewBag.ErrorMsg = "您的帐户被限制登录";
                return View("Error");
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                ViewBag.ErrorMsg = "您未实名认证，无法访问";
                return View("Error");
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                ViewBag.ErrorMsg = "您未设置支付密码，无法访问";
                return View("Error");
            }
            BanKaList = Entity.BanKaList.FirstOrNew(n => n.Id == BanKaList.Id);
            BanKaOrder BanKaOrder = Entity.BanKaOrder.FirstOrDefault(n => n.OrderState == 2 && n.PayState == 1 && n.UId == baseUsers.Id && n.BKTId == BanKaList.BKTId);
            if (BanKaOrder == null)
            {
                ViewBag.ErrorMsg = "您未购买本服务，请购买后再使用！";
                return View("Error");
            }
            BanKaList.Click++;
            Entity.SaveChanges();
            string url = BanKaList.Url;
            if (url.IsNullOrEmpty()) {
                ViewBag.ErrorMsg = "链接有误，请稍后访问！";
                return View("Error");
            }
            Response.Redirect(url);
            return false;
        }
    }
}
 