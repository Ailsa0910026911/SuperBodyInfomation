using CMSManage.Extended;
using CTCommon;
using CTModel;
using LokFu;
using LokFu.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace CMSManage.Controllers
{
    public class DefaultController : Controller
    {
        private CTContext ct = new CTContext();
        public string GetString()
        {
            var Id = Request["id"];
            int id = int.Parse(Id);
            var model = ct.FastPayWay.Where(o => o.Id == id);
            var obj = JsonConvert.SerializeObject(model);
            return obj;
        }
        public ActionResult Test(){
            var num = "17638836608";
            //电信手机号码正则        
            string dianxin = @"^1[3578][01379]\d{8}$";
            Regex dReg = new Regex(dianxin);
            //联通手机号正则        
            string liantong = @"^1[34578][01256]\d{8}$";
            Regex tReg = new Regex(liantong);
            //移动手机号正则        
            string yidong = @"^(134[012345678]\d{7}|1[34578][012356789]\d{8})$";
            Regex yReg = new Regex(yidong);

            if (!dReg.IsMatch(num) && !tReg.IsMatch(num) && !yReg.IsMatch(num))
            {
                ViewBag.ErrorMsg = "请正确填写联系手机号格式";
                return View("Error");
            }
            return View();
        }
        // GET: Default

        #region 收款订单分润处理
        [LoginCheckFilterAttribute(IsCheck = true)]
        public ActionResult Index(string otitle = "", string TNum = "", string startTime = "", string endTime = "", int id = 1)
        {
            var title = ct.FastPayWay.ToList();
            //绑定交易通道下拉列表
            var selectItemList = new List<SelectListItem>() { new SelectListItem() { Value = "0", Text = "全部", Selected = true } };
            var selectList = new SelectList(title, "Title", "Title");
            selectItemList.AddRange(selectList);
            ViewBag.database = selectItemList;
            //end
            //var startTime = DateTime.Now.AddDays(-30);
            //var model = ct.FastOrder.Where(o=>o.AddTime>startTime).ToList();
            var model = ct.FastOrder.Where(o => o.PayState == 0).ToList();
            if (startTime != "")
            {
                DateTime startDate = DateTime.Parse(startTime);
                model = model.Where(o => o.AddTime > startDate).ToList();
            }
            if (endTime != "")
            {
                DateTime endDate = DateTime.Parse(endTime);
                model = model.Where(o => o.AddTime < endDate).ToList();
            }
            if (TNum != "")
            {
                model = model.Where(O => O.TNum == TNum).ToList();
            }
            if (otitle != "0" & otitle != "")
            {
                model = model.Where(o => o.OrderType == otitle).ToList();
            }
            ViewBag.OrderNum = model.Count();
            model = model.OrderByDescending(o => o.AddTime).ToPagedList(id, 10);
            if (Request.IsAjaxRequest())
                return PartialView("_IndexTable", model);
            return View(model);
        }
        #endregion
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string UserName = "", string Password = "")
        {
            if (UserName != "" && Password != "")
            {
                var pd = Password.GetAdminMD5();
                var model = ct.SysAdmin.Where(o => o.UserName == UserName).ToList();
                if (model.Count > 0)
                {
                    model = model.Where(o => o.PassWord == pd).ToList();
                    if (model.Count() > 0)
                    {
                        Session["name"] = model[0].UserName;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Response.Write("<script>alert('密码填写错误！');</script>");
                        return View();
                    }
                }
                else
                {
                    Response.Write("<script>alert('用户名填写错误！');</script>");
                    return View();
                }
            }
            else
            {
                Response.Write("<script>alert('用户名和密码不能为空！');</script>");
                return View();
            }

        }
        public ActionResult SignOut()
        {
            if (Session["name"] != null)
            {
                Session.Remove("name");
            }
            return RedirectToAction("Login");
        }
        //分销配置
        [LoginCheckFilterAttribute(IsCheck = true)]
        public ActionResult Configuration(BusinessInfo bi)
        {
            if (bi.Name != "" && bi.Name != null)
            {
                try
                {
                    ct.BusinessInfo.Add(bi);
                    ct.SaveChanges();
                    return Content("<script>alert('添加成功！');history.go(-1);</script>");
                }
                catch (Exception e)
                {
                    return Content("<script>alert('添加失败！原因：'" + e.Message + ");history.go(-1);</script>");
                }
            }
            else
            {
                return View();
            }
        }
        //直通车支付配置
        public ActionResult ThroughTrain(int id = 1)
        {
            var model = ct.FastPayWay.OrderByDescending(o => o.AddTime).ToPagedList(id, 10);
            if (Request.IsAjaxRequest())
                return PartialView("_ThroughTrainTable", model);
            return View(model);
        }
        public ActionResult Edit(FastPayWay FastPayWay)
        {
            if (FastPayWay.Id != 0) FastPayWay = ct.FastPayWay.FirstOrDefault(n => n.Id == FastPayWay.Id);
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
        public ActionResult Save(FastPayWay FastPayWay, string[] queryArray, int STimeHH, int STimemm, int ETimeHH, int ETimemm)
        {
            FastPayWay.Cost = FastPayWay.Cost / 100;
            FastPayWay.Cost2 = FastPayWay.Cost2 / 100;
            FastPayWay.Cost3 = FastPayWay.Cost3 / 100;

            FastPayWay.BankCost = FastPayWay.BankCost / 100;
            FastPayWay.BankCost2 = FastPayWay.BankCost2 / 100;
            FastPayWay.BankCost3 = FastPayWay.BankCost3 / 100;

            FastPayWay.InCost = FastPayWay.InCost / 100;
            FastPayWay.InCost2 = FastPayWay.InCost2 / 100;
            FastPayWay.InCost3 = FastPayWay.InCost3 / 100;
            if (FastPayWay.Cost < 0 || FastPayWay.BankCost < 0 || FastPayWay.Cost >= 1)
            {
                ViewBag.ErrorMsg = "费率设置有误";
                return View("Error");
            }
            FastPayWay baseFastPayWay = ct.FastPayWay.FirstOrDefault(n => n.Id == FastPayWay.Id);
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
                baseFastPayWay = Request.ConvertRequestToModel(baseFastPayWay, FastPayWay);
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

                ct.FastPayWay.Add(FastPayWay);
            }
            ct.SaveChanges();

            APIExtensions.ClearCacheAll();
            ViewBag.Msg = "操作成功";
            return View("Succeed");
        }
    }
}