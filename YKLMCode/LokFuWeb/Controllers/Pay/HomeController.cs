using LokFu.Infrastructure;
using LokFu.Repositories;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace LokFu.Areas.Pay.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Error(string msg)
        {
            ViewBag.ErrorMsg = msg;
            return View();
        }
        /// <summary>
        /// 一户一码模式绑卡类交易入口
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="sign"></param>
        public void Pay(int Id, string sign)
        {
            if (Id.IsNullOrEmpty() || sign.IsNullOrEmpty())
            {
                Response.Write("Some Error[00]");
                return;
            }
            if (((Id * 100 + 99) + "Pay").GetMD5().Substring(8, 8) != sign)
            {
                Response.Write("Some Error[01]");
                return;
            }
            FastOrder FastOrder = Entity.FastOrder.FirstOrDefault(n => n.Id == Id);
            if (FastOrder == null)
            {
                Response.Write("Some Error[02]");
                return;
            }
            FastPayWay FastPayWay = Entity.FastPayWay.FirstOrDefault(n => n.Id == FastOrder.PayWay);
            if (FastPayWay == null)
            {
                Response.Write("Some Error[03]");
                return;
            }
            if (FastPayWay.DllName == "HFPay" || FastPayWay.DllName == "HFJSPay")
            {
                //不需要绑卡，去支付
                Response.Redirect("/paycenter/" + FastPayWay.DllName.ToLower() + "/index.html?etnum=" + HttpUtility.UrlEncode(LokFuEncode.LokFuAPIEncode(FastOrder.TNum, FastPayWay.DllName)));
            }
            else
            {
                //跳出绑卡
                Response.Redirect("/paycenter/cardpay/index.html?etnum=" + HttpUtility.UrlEncode(LokFuEncode.LokFuAPIEncode(FastOrder.TNum, "CardPay")));
            }
        }
    }
}
 