using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class BanKaListController : BaseController
    {
      
        public ActionResult Index(BanKaList BanKaList, EFPagingInfo<BanKaList> p, int IsFirst = 0)
        {
            if (IsFirst==0)
            {
                PageOfItems<BanKaList> BanKaListList1 = new PageOfItems<BanKaList>(new List<BanKaList>(), 0, 10, 0, new Hashtable());
                ViewBag.BanKaListList = BanKaListList1;
                ViewBag.BanKaList = BanKaList;
                ViewBag.BanKaTypeList = Entity.BanKaType.Where(n => n.State == 1).OrderBy(n => n.Sort).ToList();
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!BanKaList.BKTId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BKTId == BanKaList.BKTId); }
            if (!BanKaList.Title.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Title.Contains(BanKaList.Title)); }
            if (!BanKaList.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (BanKaList.State == 99 ? 0 : BanKaList.State)); }
            p.OrderByList.Add("Sort", "ASC");
            IPageOfItems<BanKaList> BanKaListList = Entity.Selects<BanKaList>(p);
            ViewBag.BanKaListList = BanKaListList;
            ViewBag.BanKaList = BanKaList;
            ViewBag.BanKaTypeList = Entity.BanKaType.Where(n => n.State == 1).OrderBy(n => n.Sort).ToList();
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BanKaList BanKaList)
        {
            ViewBag.BanKaTypeList = Entity.BanKaType.Where(n => n.State == 1).OrderBy(n => n.Sort).ToList();
            if (BanKaList.Id != 0) BanKaList = Entity.BanKaList.FirstOrDefault(n => n.Id == BanKaList.Id);
            if (BanKaList == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BanKaList = BanKaList;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BanKaList BanKaList)
        {
            BanKaList = Request.ConvertRequestToModel<BanKaList>(BanKaList, BanKaList);
            BanKaList.AddTime = DateTime.Now;
            BanKaList.Click = 0;
            Entity.BanKaList.AddObject(BanKaList);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BanKaList BanKaList)
        {
            BanKaList baseBanKaList = Entity.BanKaList.FirstOrDefault(n => n.Id == BanKaList.Id);
            baseBanKaList = Request.ConvertRequestToModel<BanKaList>(baseBanKaList, BanKaList);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BanKaList BanKaList, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BanKaList.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BanKaList>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BanKaList BanKaList, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BanKaList.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BanKaList>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
