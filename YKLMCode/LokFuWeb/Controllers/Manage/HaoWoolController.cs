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
    public class HaoWoolController : BaseController
    {
       public ActionResult Index(HaoWool HaoWool, EFPagingInfo<HaoWool> p)
       {
            if(!HaoWool.Title.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Title.Contains(HaoWool.Title));}
            if(!HaoWool.SmallTitle.IsNullOrEmpty()){p.SqlWhere.Add(f => f.SmallTitle.Contains(HaoWool.SmallTitle));}
            if(!HaoWool.SmallPic.IsNullOrEmpty()){p.SqlWhere.Add(f => f.SmallPic.Contains(HaoWool.SmallPic));}
            if(!HaoWool.Pic.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Pic.Contains(HaoWool.Pic));}
            if(!HaoWool.Click.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Click==HaoWool.Click);}
            if(!HaoWool.IsTop.IsNullOrEmpty()){p.SqlWhere.Add(f => f.IsTop==HaoWool.IsTop);}
            if(!HaoWool.State.IsNullOrEmpty()){p.SqlWhere.Add(f => f.State==HaoWool.State);}
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<HaoWool> HaoWoolList = Entity.Selects<HaoWool>(p);
            ViewBag.HaoWoolList = HaoWoolList;
            ViewBag.HaoWool = HaoWool;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
       }
       public ActionResult Edit(HaoWool HaoWool)
       {
            if(HaoWool.Id != 0) HaoWool = Entity.HaoWool.FirstOrDefault(n => n.Id == HaoWool.Id);
            if (HaoWool == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.HaoWool = HaoWool;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
       }
        [ValidateInput(false)]
        public void Add(HaoWool HaoWool)
        {
            HaoWool = Request.ConvertRequestToModel<HaoWool>(HaoWool, HaoWool);
            HaoWool.AddTime = DateTime.Now;
            HaoWool.Click = 0;
            Entity.HaoWool.AddObject(HaoWool);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(HaoWool HaoWool)
        {
            HaoWool baseHaoWool = Entity.HaoWool.FirstOrDefault(n => n.Id == HaoWool.Id);
            baseHaoWool = Request.ConvertRequestToModel<HaoWool>(baseHaoWool, HaoWool);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(HaoWool HaoWool, string InfoList,string Clomn,string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = HaoWool.Id.ToString(); }
            int Ret = Entity.ChangeEntity<HaoWool>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(HaoWool HaoWool, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)){ InfoList = HaoWool.Id.ToString();}
            int Ret = Entity.MoveToDeleteEntity<HaoWool>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
