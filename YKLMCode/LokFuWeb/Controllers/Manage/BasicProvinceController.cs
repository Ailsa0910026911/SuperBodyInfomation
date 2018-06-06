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
    public class BasicProvinceController : BaseController
    {
        public ActionResult Index(BasicProvince BasicProvince, EFPagingInfo<BasicProvince> p, int IsFirst = 0)
        {
            IPageOfItems<BasicProvince> BasicProvinceList = null;
            if (!BasicProvince.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(BasicProvince.Name)); }
            if (!BasicProvince.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (BasicProvince.State == 99 ? 0 : BasicProvince.State)); }
            p.OrderByList.Add("Id", "ASC");
            if (IsFirst == 0)
            {
                BasicProvinceList = new PageOfItems<BasicProvince>(new List<BasicProvince>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                BasicProvinceList = Entity.Selects<BasicProvince>(p);
            }

            ViewBag.BasicProvinceList = BasicProvinceList;
            ViewBag.BasicProvince = BasicProvince;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BasicProvince BasicProvince)
        {
            if (BasicProvince.Id != 0) BasicProvince = Entity.BasicProvince.FirstOrDefault(n => n.Id == BasicProvince.Id);
            if (BasicProvince == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicProvince = BasicProvince;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BasicProvince BasicProvince)
        {
            Entity.BasicProvince.AddObject(BasicProvince);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicProvince BasicProvince)
        {
            BasicProvince baseBasicProvince = Entity.BasicProvince.FirstOrDefault(n => n.Id == BasicProvince.Id);
            baseBasicProvince = Request.ConvertRequestToModel<BasicProvince>(baseBasicProvince, BasicProvince);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicProvince BasicProvince, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicProvince.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicProvince>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicProvince BasicProvince, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicProvince.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BasicProvince>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
