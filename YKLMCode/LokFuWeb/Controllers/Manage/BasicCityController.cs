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
    public class BasicCityController : BaseController
    {
        public ActionResult Index(BasicCity BasicCity, EFPagingInfo<BasicCity> p, int IsFirst = 0)
        {
            if (IsFirst==0)
            {
                PageOfItems<BasicCity> BasicCityList1 = new PageOfItems<BasicCity>(new List<BasicCity>(), 0, 10, 0, new Hashtable());
                  ViewBag.BasicCityList = BasicCityList1;
                  ViewBag.BasicCity = BasicCity;
                  ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();

                  ViewBag.Add = this.checkPower("Add");
                  ViewBag.Edit = this.checkPower("Edit");
                  ViewBag.Delete = this.checkPower("Delete");
                  ViewBag.Save = this.checkPower("Save");
                  return View();
            }
            if (!BasicCity.PId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.PId == BasicCity.PId); }
            if (!BasicCity.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(BasicCity.Name)); }
            if (!BasicCity.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (BasicCity.State==99?0:BasicCity.State)); }
            p.OrderByList.Add("PId", "DESC");
            p.OrderByList.Add("Id", "ASC");
            IPageOfItems<BasicCity> BasicCityList = Entity.Selects<BasicCity>(p);
            ViewBag.BasicCityList = BasicCityList;
            ViewBag.BasicCity = BasicCity;
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BasicCity BasicCity)
        {
            if (BasicCity.Id != 0) BasicCity = Entity.BasicCity.FirstOrDefault(n => n.Id == BasicCity.Id);
            if (BasicCity == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicCity = BasicCity;
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BasicCity BasicCity)
        {
            Entity.BasicCity.AddObject(BasicCity);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicCity BasicCity)
        {
            BasicCity baseBasicCity = Entity.BasicCity.FirstOrDefault(n => n.Id == BasicCity.Id);
            baseBasicCity = Request.ConvertRequestToModel<BasicCity>(baseBasicCity, BasicCity);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicCity BasicCity, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicCity.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicCity>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicCity BasicCity, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicCity.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BasicCity>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
