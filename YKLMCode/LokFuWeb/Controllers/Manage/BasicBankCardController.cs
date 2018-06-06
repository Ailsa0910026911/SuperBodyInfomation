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
    public class BasicBankCardController : BaseController
    {

        public ActionResult Index(BasicBankCard BasicBankCard, EFPagingInfo<BasicBankCard> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<BasicBankCard> BasicBankCardList1 = new PageOfItems<BasicBankCard>(new List<BasicBankCard>(), 0, 10, 0, new Hashtable());
                ViewBag.BasicBankCardList = BasicBankCardList1;
                ViewBag.BasicBankCard = BasicBankCard;
                ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!BasicBankCard.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(BasicBankCard.Name)); }
            if (!BasicBankCard.BId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BId == BasicBankCard.BId); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BasicBankCard> BasicBankCardList = Entity.Selects<BasicBankCard>(p);
            ViewBag.BasicBankCardList = BasicBankCardList;
            ViewBag.BasicBankCard = BasicBankCard;
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BasicBankCard BasicBankCard)
        {
            if (BasicBankCard.Id != 0) BasicBankCard = Entity.BasicBankCard.FirstOrDefault(n => n.Id == BasicBankCard.Id);
            if (BasicBankCard == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicBankCard = BasicBankCard;
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BasicBankCard BasicBankCard)
        {
            Entity.BasicBankCard.AddObject(BasicBankCard);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicBankCard BasicBankCard)
        {
            BasicBankCard baseBasicBankCard = Entity.BasicBankCard.FirstOrDefault(n => n.Id == BasicBankCard.Id);
            baseBasicBankCard = Request.ConvertRequestToModel<BasicBankCard>(baseBasicBankCard, BasicBankCard);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicBankCard BasicBankCard, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicBankCard.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicBankCard>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicBankCard BasicBankCard, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicBankCard.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BasicBankCard>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
