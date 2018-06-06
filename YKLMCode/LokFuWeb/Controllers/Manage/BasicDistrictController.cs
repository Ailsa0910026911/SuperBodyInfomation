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
    public class BasicDistrictController : BaseController
    {
        public ActionResult Index(BasicDistrict BasicDistrict, EFPagingInfo<BasicDistrict> p, int IsFirst = 0)
        {
            IPageOfItems<BasicDistrict> BasicDistrictList = null;
            if (!BasicDistrict.CId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CId == BasicDistrict.CId); }
            if (!BasicDistrict.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(BasicDistrict.Name)); }
            if (!BasicDistrict.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (BasicDistrict.State == 99 ? 0 : BasicDistrict.State)); }
            p.OrderByList.Add("Id", "DESC");
            if (IsFirst == 0)
            {
                BasicDistrictList = new PageOfItems<BasicDistrict>(new List<BasicDistrict>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                BasicDistrictList = Entity.Selects<BasicDistrict>(p);
            }

            ViewBag.BasicDistrictList = BasicDistrictList;
            ViewBag.BasicDistrict = BasicDistrict;
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BasicDistrict BasicDistrict)
        {
            if (BasicDistrict.Id != 0) BasicDistrict = Entity.BasicDistrict.FirstOrDefault(n => n.Id == BasicDistrict.Id);
            if (BasicDistrict == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicDistrict = BasicDistrict;
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BasicDistrict BasicDistrict)
        {
            Entity.BasicDistrict.AddObject(BasicDistrict);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicDistrict BasicDistrict)
        {
            BasicDistrict baseBasicDistrict = Entity.BasicDistrict.FirstOrDefault(n => n.Id == BasicDistrict.Id);
            baseBasicDistrict = Request.ConvertRequestToModel<BasicDistrict>(baseBasicDistrict, BasicDistrict);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicDistrict BasicDistrict, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicDistrict.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicDistrict>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicDistrict BasicDistrict, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicDistrict.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BasicDistrict>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
