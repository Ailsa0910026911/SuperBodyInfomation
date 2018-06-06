using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class UserPayCreditController : BaseController
    {
        public ActionResult Index(UserPayCredit UserPayCredit, EFPagingInfo<UserPayCredit> p)
        {
            if (!UserPayCredit.TrueName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TrueName.Contains(UserPayCredit.TrueName)); }
            if (!UserPayCredit.Mobile.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Mobile.Contains(UserPayCredit.Mobile)); }
            if (!UserPayCredit.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == UserPayCredit.State); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<UserPayCredit> UserPayCreditList = Entity.Selects<UserPayCredit>(p);
            ViewBag.UserPayCreditList = UserPayCreditList;
            ViewBag.UserPayCredit = UserPayCredit;
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(UserPayCredit UserPayCredit)
        {
            if (UserPayCredit.Id != 0) UserPayCredit = Entity.UserPayCredit.FirstOrDefault(n => n.Id == UserPayCredit.Id);
            if (UserPayCredit == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.UserPayCredit = UserPayCredit;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Save(UserPayCredit UserPayCredit)
        {
            UserPayCredit baseUserPayCredit = Entity.UserPayCredit.FirstOrDefault(n => n.Id == UserPayCredit.Id);
            baseUserPayCredit.State = UserPayCredit.State;
            if (UserPayCredit.Remark.IsNullOrEmpty()) {
                UserPayCredit.Remark = string.Empty;
            }
            string[] arrA = "已取消,待处理,已联系,已完成".Split(',');
            if (UserPayCredit.State > 3) {
                UserPayCredit.State = 0;
            }
            UserPayCredit.Remark = UserPayCredit.Remark + "【" + arrA[UserPayCredit.State] + "】";
            if (baseUserPayCredit.Remark.IsNullOrEmpty())
            {
                baseUserPayCredit.Remark = AdminUser.TrueName + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "," + UserPayCredit.Remark.Replace("|", "").Replace(",", "");
            }
            else {
                baseUserPayCredit.Remark = baseUserPayCredit.Remark + "|" + AdminUser.TrueName + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "," + UserPayCredit.Remark.Replace("|", "").Replace(",", "");
            }
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(UserPayCredit UserPayCredit, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = UserPayCredit.Id.ToString(); }
            int Ret = Entity.ChangeEntity<UserPayCredit>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(UserPayCredit UserPayCredit, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = UserPayCredit.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<UserPayCredit>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
