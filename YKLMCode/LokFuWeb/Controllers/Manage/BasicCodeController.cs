using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class BasicCodeController : BaseController
    {
       public ActionResult Index(BasicCode BasicCode, EFPagingInfo<BasicCode> p)
       {
            if(!BasicCode.CharCode.IsNullOrEmpty()){p.SqlWhere.Add(f => f.CharCode.Contains(BasicCode.CharCode));}
            if(!BasicCode.CharText.IsNullOrEmpty()){p.SqlWhere.Add(f => f.CharText.Contains(BasicCode.CharText));}
            if(!BasicCode.Sort.IsNullOrEmpty()){p.SqlWhere.Add(f => f.Sort==BasicCode.Sort);}
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BasicCode> BasicCodeList = Entity.Selects<BasicCode>(p);
            ViewBag.BasicCodeList = BasicCodeList;
            ViewBag.BasicCode = BasicCode;

            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
       }
       public ActionResult Edit(BasicCode BasicCode)
       {
            if(BasicCode.Id != 0) BasicCode = Entity.BasicCode.FirstOrDefault(n => n.Id == BasicCode.Id);
            if (BasicCode == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicCode = BasicCode;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
       }
        [ValidateInput(false)]
        public void Add(BasicCode BasicCode)
        {
            Entity.BasicCode.AddObject(BasicCode);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicCode BasicCode)
        {
            BasicCode baseBasicCode = Entity.BasicCode.FirstOrDefault(n => n.Id == BasicCode.Id);
            baseBasicCode = Request.ConvertRequestToModel<BasicCode>(baseBasicCode, BasicCode);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicCode BasicCode, string InfoList,string Clomn,string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicCode.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicCode>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicCode BasicCode, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)){ InfoList = BasicCode.Id.ToString();}
            int Ret = Entity.MoveToDeleteEntity<BasicCode>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
