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
    public class BasicCardBinController : BaseController
    {

        public ActionResult Index(BasicCardBin BasicCardBin, EFPagingInfo<BasicCardBin> p, int IsFirst = 0)
        {
            if (!BasicCardBin.BankCode.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BankCode == BasicCardBin.BankCode); }
            if (!BasicCardBin.BIN.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BIN == BasicCardBin.BIN); }
            if (!BasicCardBin.BankName.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BankName == BasicCardBin.BankName); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BasicCardBin> BasicCardBinList;
            if (IsFirst == 0)
            {
                BasicCardBinList = new PageOfItems<BasicCardBin>(new List<BasicCardBin>(), 0, 10, 0, new Hashtable());
                
            }
            else
            {
                BasicCardBinList = Entity.Selects<BasicCardBin>(p);
            }
            ViewBag.BasicCardBinList = BasicCardBinList;
            ViewBag.BasicCardBin = BasicCardBin;
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BasicCardBin BasicCardBin)
        {
            if (BasicCardBin.Id != 0) BasicCardBin = Entity.BasicCardBin.FirstOrDefault(n => n.Id == BasicCardBin.Id);
            if (BasicCardBin == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicCardBin = BasicCardBin;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BasicCardBin BasicCardBin)
        {
            BasicCardBin = Request.ConvertRequestToModel<BasicCardBin>(BasicCardBin, BasicCardBin);
            Entity.BasicCardBin.AddObject(BasicCardBin);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicCardBin BasicCardBin)
        {
            BasicCardBin baseBasicCardBin = Entity.BasicCardBin.FirstOrDefault(n => n.Id == BasicCardBin.Id);
            if (baseBasicCardBin == null)
            {
                Response.Write("数据不存在");
                return;
            }
            baseBasicCardBin = Request.ConvertRequestToModel<BasicCardBin>(baseBasicCardBin, BasicCardBin);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicCardBin BasicCardBin, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicCardBin.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicCardBin>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicCardBin BasicCardBin, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicCardBin.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BasicCardBin>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
