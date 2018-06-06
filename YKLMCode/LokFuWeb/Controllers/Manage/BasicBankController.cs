using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class BasicBankController : BaseController
    {

        public ActionResult Index(BasicBank BasicBank, EFPagingInfo<BasicBank> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<BasicBank> BasicBankList1 = new PageOfItems<BasicBank>(new List<BasicBank>(), 0, 10, 0, new Hashtable());
                ViewBag.BasicBankList = BasicBankList1;
                ViewBag.BasicBank = BasicBank;
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!BasicBank.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(BasicBank.Name)); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BasicBank> BasicBankList = Entity.Selects<BasicBank>(p);
            ViewBag.BasicBankList = BasicBankList;
            ViewBag.BasicBank = BasicBank;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BasicBank BasicBank)
        {
            if (BasicBank.Id != 0) BasicBank = Entity.BasicBank.FirstOrDefault(n => n.Id == BasicBank.Id);
            if (BasicBank == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicBank = BasicBank;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BasicBank BasicBank)
        {
            BasicBank = Request.ConvertRequestToModel<BasicBank>(BasicBank, BasicBank);
            Entity.BasicBank.AddObject(BasicBank);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicBank BasicBank)
        {
            BasicBank baseBasicBank = Entity.BasicBank.FirstOrDefault(n => n.Id == BasicBank.Id);
            baseBasicBank = Request.ConvertRequestToModel<BasicBank>(baseBasicBank, BasicBank);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicBank BasicBank, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicBank.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicBank>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicBank BasicBank, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicBank.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BasicBank>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
