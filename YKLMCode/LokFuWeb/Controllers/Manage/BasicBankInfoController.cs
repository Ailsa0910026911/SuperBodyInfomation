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
    public class BasicBankInfoController : BaseController
    {

        public ActionResult Index(BasicBankInfo BasicBankInfo, EFPagingInfo<BasicBankInfo> p, int IsFirst = 0)
        {
            if (IsFirst == 0)
            {
                PageOfItems<BasicBankInfo> BasicBankInfoList1 = new PageOfItems<BasicBankInfo>(new List<BasicBankInfo>(), 0, 10, 0, new Hashtable());
                ViewBag.BasicBankInfoList = BasicBankInfoList1;
                ViewBag.BasicBankInfo = BasicBankInfo;
                ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
                ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
                ViewBag.BasicDistrictList = Entity.BasicDistrict.Where(n => n.State == 1).ToList();
                ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
                ViewBag.Add = this.checkPower("Add");
                ViewBag.Edit = this.checkPower("Edit");
                ViewBag.Delete = this.checkPower("Delete");
                ViewBag.Save = this.checkPower("Save");
                return View();
            }
            if (!BasicBankInfo.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name.Contains(BasicBankInfo.Name)); }
            if (!BasicBankInfo.BId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.BId == BasicBankInfo.BId); }
            if (!BasicBankInfo.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == (BasicBankInfo.State==99?0:BasicBankInfo.State)); }
            if (!BasicBankInfo.SId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.SId == BasicBankInfo.SId); }
            if (!BasicBankInfo.CId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.CId == BasicBankInfo.CId); }
            if (!BasicBankInfo.DId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.DId == BasicBankInfo.DId); }
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<BasicBankInfo> BasicBankInfoList = Entity.Selects<BasicBankInfo>(p);
            ViewBag.BasicBankInfoList = BasicBankInfoList;
            ViewBag.BasicBankInfo = BasicBankInfo;
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            ViewBag.BasicDistrictList = Entity.BasicDistrict.Where(n => n.State == 1).ToList();
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Delete = this.checkPower("Delete");
            ViewBag.Save = this.checkPower("Save");
            return View();
        }
        public ActionResult Edit(BasicBankInfo BasicBankInfo)
        {
            if (BasicBankInfo.Id != 0) BasicBankInfo = Entity.BasicBankInfo.FirstOrDefault(n => n.Id == BasicBankInfo.Id);
            if (BasicBankInfo == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.BasicBankInfo = BasicBankInfo;
            ViewBag.BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
            ViewBag.BasicCityList = Entity.BasicCity.Where(n => n.State == 1).ToList();
            ViewBag.BasicDistrictList = Entity.BasicDistrict.Where(n => n.State == 1).ToList();
            ViewBag.BasicBankList = Entity.BasicBank.Where(n => n.State == 1).ToList();
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Add(BasicBankInfo BasicBankInfo)
        {
            Entity.BasicBankInfo.AddObject(BasicBankInfo);
            Entity.SaveChanges();
            BaseRedirect();
        }
        [ValidateInput(false)]
        public void Save(BasicBankInfo BasicBankInfo)
        {
            BasicBankInfo baseBasicBankInfo = Entity.BasicBankInfo.FirstOrDefault(n => n.Id == BasicBankInfo.Id);
            baseBasicBankInfo = Request.ConvertRequestToModel<BasicBankInfo>(baseBasicBankInfo, BasicBankInfo);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public void ChangeStatus(BasicBankInfo BasicBankInfo, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicBankInfo.Id.ToString(); }
            int Ret = Entity.ChangeEntity<BasicBankInfo>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(BasicBankInfo BasicBankInfo, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = BasicBankInfo.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<BasicBankInfo>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
    }
}
