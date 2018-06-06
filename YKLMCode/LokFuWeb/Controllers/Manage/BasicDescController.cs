using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class BasicDescController : BaseController
    {
        public ActionResult Index(BasicDesc BasicDesc, EFPagingInfo<BasicDesc> p)
        {
            if (!BasicDesc.CharCode.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CharCode.Contains(BasicDesc.CharCode)); }
            if (!BasicDesc.TitleCode.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TitleCode.Contains(BasicDesc.TitleCode)); }
            if (!BasicDesc.DescText.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.DescText.Contains(BasicDesc.DescText)); }
            if (!BasicDesc.Sort.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Sort == BasicDesc.Sort); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BasicDesc> BasicDescList = Entity.Selects<BasicDesc>(p);
            ViewBag.BasicDescList = BasicDescList;
            ViewBag.BasicDesc = BasicDesc;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BasicDesc BasicDesc)
        {
            if (BasicDesc.Id != 0) BasicDesc = Entity.BasicDesc.FirstOrDefault(n => n.Id == BasicDesc.Id);
            if (BasicDesc == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            EFPagingInfo<BasicCode> p_BasicCode = new EFPagingInfo<BasicCode>();
            p_BasicCode.OrderByList.Add("Sort", "desc");
            ViewBag.BasicCodeList = Entity.Selects<BasicCode>(p_BasicCode).ToList();
            ViewBag.BasicDesc = BasicDesc;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BasicDesc BasicDesc)
        {
            Entity.BasicDesc.AddObject(BasicDesc);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicDesc BasicDesc)
        {
            BasicDesc baseBasicDesc = Entity.BasicDesc.FirstOrDefault(n => n.Id == BasicDesc.Id);
            baseBasicDesc = Request.ConvertRequestToModel<BasicDesc>(baseBasicDesc, BasicDesc);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicDesc BasicDesc, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicDesc.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicDesc>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicDesc BasicDesc, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicDesc.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BasicDesc>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
