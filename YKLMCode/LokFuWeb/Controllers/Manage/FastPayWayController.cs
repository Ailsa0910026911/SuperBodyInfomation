using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class FastPayWayController : BaseController
    {
        public ActionResult Index(FastPayWay FastPayWay, EFPagingInfo<FastPayWay> p)
        {
            if (!FastPayWay.Title.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Title.Contains(FastPayWay.Title)); }
            if (!FastPayWay.State.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.State == FastPayWay.State); }
            p.PageSize = 9999;
            p.OrderByList.Add("State", "DESC");
            IPageOfItems<FastPayWay> FastPayWayList = Entity.Selects<FastPayWay>(p);
            ViewBag.FastPayWayList = FastPayWayList.OrderByDescending(o => o.State).ThenBy(o => o.Sort).ToList();
            ViewBag.FastPayWay = FastPayWay;
            ViewBag.Save = this.checkPower("Save");
            ViewBag.SetManE = this.checkPower("SetManE");
            ViewBag.Remove = this.checkPower("Remove");
            return View();
        }
        public ActionResult Edit(FastPayWay FastPayWay)
        {
            if (FastPayWay.Id != 0) FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastPayWay.Id);
            //var a = "yklm123456";
            //var b = a.GetMD5();
            if (FastPayWay == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.FastPayWay = FastPayWay;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        public ActionResult Add()
        {
            return View();
        }
        public ActionResult Delete(FastPayWay FastPayWay)
        {
            if (FastPayWay.Id != 0) FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastPayWay.Id);
            if (FastPayWay == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            try
            {
                Entity.FastPayWay.DeleteObject(FastPayWay);
                Entity.SaveChanges();
                APIExtensions.ClearCacheAll();
                ViewBag.Msg = "删除成功";
                return View("Succeed");
            }
            catch
            {
                ViewBag.Msg = "删除失败";
                return View("Error");
            }

        }
        [ValidateInput(false)]
        public ActionResult Save(FastPayWay FastPayWay, string[] queryArray, int STimeHH, int STimemm, int ETimeHH, int ETimemm)
        {
            FastPayWay.Cost = FastPayWay.Cost / 1000;
            FastPayWay.Cost2 = FastPayWay.Cost2 / 1000;
            FastPayWay.Cost3 = FastPayWay.Cost3 / 1000;

            FastPayWay.BankCost = FastPayWay.BankCost / 1000;
            FastPayWay.BankCost2 = FastPayWay.BankCost2 / 1000;
            FastPayWay.BankCost3 = FastPayWay.BankCost3 / 1000;

            FastPayWay.InCost = FastPayWay.InCost / 1000;
            FastPayWay.InCost2 = FastPayWay.InCost2 / 1000;
            FastPayWay.InCost3 = FastPayWay.InCost3 / 1000;
            if (FastPayWay.Cost < 0 || FastPayWay.BankCost < 0 || FastPayWay.Cost >= 1)
            {
                ViewBag.ErrorMsg = "费率设置有误";
                return View("Error");
            }
            FastPayWay baseFastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastPayWay.Id);
            if (baseFastPayWay != null)//修改直通车通道
            {
                //如果是微信支付配置的子商户号没有填写的话，去掉这个元素
                if (baseFastPayWay.DllName == "WeiXin")
                {
                    if (queryArray[4].IsNullOrEmpty())
                    {
                        var temp = new ArrayList(queryArray);
                        temp.RemoveAt(4);
                        queryArray = (string[])temp.ToArray(typeof(string));
                    }
                }
                if (queryArray != null)
                {
                    baseFastPayWay.QueryArray = string.Join(",", queryArray);
                }
                baseFastPayWay = Request.ConvertRequestToModel<FastPayWay>(baseFastPayWay, FastPayWay);
                baseFastPayWay.HasAliPay = FastPayWay.HasAliPay;
                baseFastPayWay.HasBank = FastPayWay.HasBank;
                baseFastPayWay.HasWeiXin = FastPayWay.HasWeiXin;
                DateTime STime = DateTime.Parse("1990-01-01 " + STimeHH + ":" + STimemm + ":00");
                DateTime ETime = DateTime.Parse("1990-01-01 " + ETimeHH + ":" + ETimemm + ":" + (ETimeHH == 23 && ETimemm == 59 ? "59" : "00"));
                baseFastPayWay.STime = STime;
                baseFastPayWay.ETime = ETime;
            }
            else//添加直通车通道
            {
                FastPayWay.Title = "测试名称";
                FastPayWay.DllName = "HFJSPay";
                FastPayWay.AddTime = DateTime.Now;
                FastPayWay.GroupType = "D0";
                FastPayWay.Version = "V1.0.0";
                FastPayWay.CanOpenBank = 1;
                DateTime STime = DateTime.Parse("1990-01-01 " + STimeHH + ":" + STimemm + ":00");
                DateTime ETime = DateTime.Parse("1990-01-01 " + ETimeHH + ":" + ETimemm + ":" + (ETimeHH == 23 && ETimemm == 59 ? "59" : "00"));
                FastPayWay.STime = STime;
                FastPayWay.ETime = ETime;

                Entity.FastPayWay.AddObject(FastPayWay);
            }
            Entity.SaveChanges();

            APIExtensions.ClearCacheAll();
            ViewBag.Msg = "操作成功";
            return View("Succeed");
        }
        public void ChangeStatus(FastPayWay FastPayWay, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = FastPayWay.Id.ToString(); }
            int Ret = Entity.ChangeEntity<FastPayWay>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public string SetManE(int Id)
        {
            FastPayWay baseFastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == Id);
            if (baseFastPayWay == null)
            {
                return "2";
            }
            baseFastPayWay.ManE = (byte)(1 - baseFastPayWay.ManE);
            Entity.SaveChanges();
            return baseFastPayWay.ManE.ToString();
        }

        //测试页面
        public ActionResult IsRemove(FastPayWay FastPayWay)
        {
            if (FastPayWay.Id != 0) FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastPayWay.Id);
            if (FastPayWay == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.FastPayWay = FastPayWay;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
    }
}
