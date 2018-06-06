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
    public class BanKaTypeController : BaseController
    {
       public ActionResult Index(BanKaType BanKaType, EFPagingInfo<BanKaType> p)
       {
            if(!BanKaType.Title.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Title.Contains(BanKaType.Title));}
            if(!BanKaType.Pic.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Pic.Contains(BanKaType.Pic));}
            if(!BanKaType.State.IsNullOrEmpty()){p.SqlWhere.Add(f => f.State==BanKaType.State);}
            if(!BanKaType.Sort.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Sort==BanKaType.Sort);}
            p.OrderByList.Add("Sort", "ASC");
            IPageOfItems<BanKaType> BanKaTypeList = Entity.Selects<BanKaType>(p);
            ViewBag.BanKaTypeList = BanKaTypeList;
            ViewBag.BanKaType = BanKaType;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
       }
       public ActionResult Edit(BanKaType BanKaType)
       {
            if(BanKaType.Id != 0) BanKaType = Entity.BanKaType.FirstOrDefault(n => n.Id == BanKaType.Id);
            if (BanKaType == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BanKaType = BanKaType;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
       }
        [ValidateInput(false)]
        public void Add(BanKaType BanKaType)
        {
            BanKaType = Request.ConvertRequestToModel<BanKaType>(BanKaType, BanKaType);
            BanKaType.AddTime = DateTime.Now;
            Entity.BanKaType.AddObject(BanKaType);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BanKaType BanKaType)
        {
            BanKaType baseBanKaType = Entity.BanKaType.FirstOrDefault(n => n.Id == BanKaType.Id);
            baseBanKaType = Request.ConvertRequestToModel<BanKaType>(baseBanKaType, BanKaType);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BanKaType BanKaType, string InfoList,string Clomn,string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BanKaType.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BanKaType>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BanKaType BanKaType, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)){ InfoList = BanKaType.Id.ToString();}
            int Ret = Entity.MoveToDeleteEntity<BanKaType>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
