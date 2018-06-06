using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class AdTagController : BaseController
    {
        public ActionResult Index(AdTag AdTag, EFPagingInfo<AdTag> p)
        {
            p.OrderByList.Add("State", "DESC");
            p.OrderByList.Add("Sort", "ASC");
            IPageOfItems<AdTag> AdTagList = Entity.Selects<AdTag>(p);
            ViewBag.AdTagList = AdTagList;
            ViewBag.AdTag = AdTag;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");

            return View();
        }
        public ActionResult Edit(AdTag AdTag)
        {
            if (AdTag.Id != 0)
            {
                AdTag = Entity.AdTag.FirstOrDefault(n => n.Id == AdTag.Id);
            }
            if (AdTag == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.AdTag = AdTag;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(AdTag AdTag)
        {
            Entity.AdTag.AddObject(AdTag);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(AdTag AdTag)
        {
            AdTag baseAdTag = Entity.AdTag.FirstOrDefault(n => n.Id == AdTag.Id);
            baseAdTag = Request.ConvertRequestToModel<AdTag>(baseAdTag, AdTag);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(AdTag AdTag, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = AdTag.Id.ToString(); }
            int Ret = Entity.ChangeEntity<AdTag>(InfoList, Clomn, Value);
            Response.Write(Ret);
        }
        public void Delete(AdTag AdTag, string InfoList,int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = AdTag.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<AdTag>(InfoList, IsDel, AdminUser.UserName);
            Response.Write(Ret);
        }
    }
}
