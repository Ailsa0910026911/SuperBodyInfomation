using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class BasicCarBrandController : BaseController
    {
       public ActionResult Index(BasicCarBrand BasicCarBrand, EFPagingInfo<BasicCarBrand> p)
       {
            if(!BasicCarBrand.Name.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Name.Contains(BasicCarBrand.Name));}
            if(!BasicCarBrand.Score.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Score==BasicCarBrand.Score);}
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BasicCarBrand> BasicCarBrandList = Entity.Selects<BasicCarBrand>(p);
            ViewBag.BasicCarBrandList = BasicCarBrandList;
            ViewBag.BasicCarBrand = BasicCarBrand;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
       }
       public ActionResult Edit(BasicCarBrand BasicCarBrand)
       {
            if(BasicCarBrand.Id != 0) BasicCarBrand = Entity.BasicCarBrand.FirstOrDefault(n => n.Id == BasicCarBrand.Id);
            if (BasicCarBrand == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicCarBrand = BasicCarBrand;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
       }
        [ValidateInput(false)]
        public void Add(BasicCarBrand BasicCarBrand)
        {
            BasicCarBrand = Request.ConvertRequestToModel<BasicCarBrand>(BasicCarBrand, BasicCarBrand);
            Entity.BasicCarBrand.AddObject(BasicCarBrand);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicCarBrand BasicCarBrand)
        {
            BasicCarBrand baseBasicCarBrand = Entity.BasicCarBrand.FirstOrDefault(n => n.Id == BasicCarBrand.Id);
            baseBasicCarBrand = Request.ConvertRequestToModel<BasicCarBrand>(baseBasicCarBrand, BasicCarBrand);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicCarBrand BasicCarBrand, string InfoList,string Clomn,string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicCarBrand.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicCarBrand>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicCarBrand BasicCarBrand, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)){ InfoList = BasicCarBrand.Id.ToString();}
            int Ret = Entity.MoveToDeleteEntity<BasicCarBrand>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
