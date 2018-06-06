using LokFu.Base;
using LokFu.Extensions;
using LokFu.FastPay;
using LokFu.Repositories;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace LokFu.Areas.Base.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }
        public void Test()
        {
            //FastUser FastUser = Entity.FastUser.FirstOrDefault(n => n.UId == 25);
            //FastUserPay FastUserPay = Entity.FastUserPay.FirstOrDefault(n => n.Id == 291);
            //FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == 15);
            //FastConfig FastConfig = Entity.FastConfig.FirstOrDefault();
            //BusFastPay BusFastPay = new BusFastPay();
            ////BusFastPay.AddMer(FastUser, FastPayWay, FastConfig, Entity);
            //BusFastPay.AddCard(FastUser, FastUserPay, FastPayWay, Entity);
        }
        public void ImgCode()
        {
            HttpContext.CreateCheckCodeImage();
        }
        public void CheckCode(string code)
        {
            if (code.ToUpper() != Session.GetCheckCode())
            {
                Response.Write("0");
            }
            else {
                Response.Write("1");
            }
        }
        public void UpFile()
        {
            string str = Upload.ProcessRequest();
            Response.Write(str);
            return;
        }
        public void SelectBankJson(string type, int? pid, int? bid, int? cid)
        {
            if (type == "ALLBank")
            {
                IList<BasicBank> BasicBankList = Entity.BasicBank.ToList();
                foreach (var p in BasicBankList)
                {
                    p.Cols = "Id,Name";
                }
                Response.Write(BasicBankList.EntityToJson());
            }
            if (type == "BasicBank")
            {
                IList<BasicBank> BasicBankList = Entity.BasicBank.Where(n => n.IsPayCard == 1).ToList();
                foreach (var p in BasicBankList)
                {
                    p.Cols = "Id,Name";
                }
                Response.Write("var BasicBank=" + BasicBankList.EntityToJson() + ";");
            }
            if (type == "BasicProvince")
            {
                IList<BasicProvince> BasicProvinceList = Entity.BasicProvince.Where(n => n.State == 1).ToList();
                Response.Write("var BasicProvince=" + BasicProvinceList.EntityToJson() + ";");
            }
            if (type == "BasicCity")
            {
                if (pid.IsNullOrEmpty()) {
                    pid = 0;
                }
                IList<BasicCity> BasicCityList = Entity.BasicCity.Where(n => n.State == 1 && n.PId == pid).ToList();
                Response.Write("var BasicCity=" + BasicCityList.EntityToJson() + ";");
            }
            if (type == "BasicBankInfo")
            {
                if (bid.IsNullOrEmpty())
                {
                    bid = 0;
                }
                if (cid.IsNullOrEmpty())
                {
                    cid = 0;
                }
                IList<BasicBankInfo> BasicBankInfoList = Entity.BasicBankInfo.Where(n => n.State == 1 && n.BId == bid && n.CId == cid).ToList();
                Response.Write("var BasicBankInfo=" + BasicBankInfoList.EntityToJson() + ";");
            }
        }
    }
    //===========================================
}
 