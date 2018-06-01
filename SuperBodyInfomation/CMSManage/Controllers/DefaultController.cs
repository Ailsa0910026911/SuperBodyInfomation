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

        public ActionResult Test()
        {
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

        #region 收款订单分润处理（Index）
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
        #region 登录验证
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
                        return RedirectToAction("Index", "Default");
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
        #endregion
        #region 退出登录
        public ActionResult SignOut()
        {
            if (Session["name"] != null)
            {
                Session.Remove("name");
            }
            return RedirectToAction("Login", "Default");
        }
        #endregion
        #region 分销中心设置
        //分销配置
        [LoginCheckFilterAttribute(IsCheck = true)]
        public ActionResult Configuration(BusinessInfo bi)
        {
            ViewBag.Configuration = "添加";
            var id = Request["Id"];
            if (id != "" && id != null)
            {

                int num = int.Parse(id);
                var model = ct.BusinessInfo.Where(o => o.Id == num).FirstOrDefault();
                if (bi.Edit == "" || bi.Edit == null)
                {
                    model.Edit = "0";
                    ViewBag.BusinessInfo = model;
                    ViewBag.Configuration = "修改";
                    return View();
                }
                else
                {
                    model.Name = bi.Name;
                    model.Url = bi.Url;
                    model.Type = bi.Type;
                    model.Img = bi.Img;
                    model.OneDescribe = bi.OneDescribe;
                    model.Describe = bi.Describe;
                    model.Limit = bi.Limit;
                    model.SuccessRate = bi.SuccessRate;
                    model.CompanyName = bi.CompanyName;
                    model.InterestRate = bi.InterestRate;
                    model.ServiceCharge = bi.ServiceCharge;
                    model.Age = bi.Age;
                    model.ApplicationMaterial = bi.ApplicationMaterial;
                    model.AuthorizatonReason = bi.AuthorizatonReason;
                    model.HandlingTime = bi.HandlingTime;
                    model.LengthOfMaturity = bi.LengthOfMaturity;
                    model.ModeOfRepayment = bi.ModeOfRepayment;
                    model.Prepayment = bi.Prepayment;
                    model.ReviewTheWay = bi.ReviewTheWay;
                    model.CallBack = bi.CallBack;
                    model.MoneyWay = bi.MoneyWay;
                    model.MoneyDelete = bi.MoneyDelete;
                    model.SelectCredit = bi.SelectCredit;
                    model.DownloadAPP = bi.DownloadAPP;
                    model.ApplicationProcedure = bi.ApplicationProcedure;
                    model.Adress = bi.Adress;
                    model.Phone = bi.Phone;
                    ct.SaveChanges();
                    return RedirectToAction("ConfigurationList", "Default");
                }
            }
            else
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
        }
        public ActionResult ConfigurationList(int id = 0)
        {
            var model = ct.BusinessInfo.OrderByDescending(o => o.Id).ToPagedList(id, 10);
            if (Request.IsAjaxRequest())
                return PartialView("_ConfigurationListTable", model);
            return View(model);
        }

        #endregion
        #region 直通车支付配置（ThroughTrain）
        //直通车支付配置
        [LoginCheckFilterAttribute(IsCheck = true)]
        public ActionResult ThroughTrain(int id = 1)
        {
            var model = ct.FastPayWay.OrderByDescending(o => o.AddTime).ToPagedList(id, 10);
            if (Request.IsAjaxRequest())
                return PartialView("_ThroughTrainTable", model);
            return View(model);
        }
        //获取需要修改的FastPayWay信息
        public string GetString()
        {
            var Id = Request["id"];
            int id = int.Parse(Id);
            var model = ct.FastPayWay.Where(o => o.Id == id);
            var obj = JsonConvert.SerializeObject(model);
            return obj;
        }
        //添加和修改操作
        public ActionResult SaveFastPayWay(FastPayWay FastPayWay)
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
                return Content("<script>alert('费率设置有误！');window.history.go(-1);</script>");
            }
            FastPayWay baseFastPayWay = ct.FastPayWay.FirstOrDefault(n => n.Id == FastPayWay.Id);
            if (baseFastPayWay != null)//修改直通车通道
            {
                baseFastPayWay.Title = FastPayWay.Title;
                baseFastPayWay.ShowName = FastPayWay.ShowName;
                baseFastPayWay.NeekReg = FastPayWay.NeekReg;
                baseFastPayWay.NeekCard = FastPayWay.NeekCard;
                baseFastPayWay.TimeType = FastPayWay.TimeType;
                baseFastPayWay.STime = FastPayWay.STime;
                baseFastPayWay.ETime = FastPayWay.ETime;
                baseFastPayWay.InCost3 = FastPayWay.InCost3;
                baseFastPayWay.BankCost3 = FastPayWay.BankCost3;
                baseFastPayWay.MinCost3 = FastPayWay.MinCost3;
                baseFastPayWay.MaxCost3 = FastPayWay.MaxCost3;
                baseFastPayWay.BankSNum = FastPayWay.BankSNum;
                baseFastPayWay.BankENum = FastPayWay.BankENum;
                baseFastPayWay.Cash = FastPayWay.Cash;
                baseFastPayWay.Sort = FastPayWay.Sort;
                baseFastPayWay.State = FastPayWay.State;
                baseFastPayWay.QueryArray = FastPayWay.QueryArray;
                baseFastPayWay.Type = FastPayWay.Type;
            }
            else//添加直通车通道
            {
                FastPayWay.DllName = "HFJSPay";
                FastPayWay.AddTime = DateTime.Now;
                FastPayWay.GroupType = "D0";
                FastPayWay.Version = "V1.0.0";
                FastPayWay.CanOpenBank = 1;
                ct.FastPayWay.Add(FastPayWay);
            }
            ct.SaveChanges();

            return Content("<script>alert('操作成功！');history.go(-1);</script>");
        }
        //删除操作
        public string DeleteFastPayWay()
        {
            var id = Request["Id"];
            int Id = int.Parse(id);
            var model = ct.FastPayWay.Where(o => o.Id == Id).FirstOrDefault();
            if (model != null)
            {
                ct.FastPayWay.Remove(model);
                ct.SaveChanges();
                return "1";
            }
            else
            {
                return "0";
            }

        }
        #endregion
        //分销分润百分比配置
        public ActionResult DistributionFenRunList(int id = 1)
        {
            var model = ct.BusinessShareProfit.OrderByDescending(o => o.Id).ToPagedList(id, 10);
            if (Request.IsAjaxRequest())
                return PartialView("_DistributionFenRunListTable", model);
            return View(model);
        }

        //添加/修改 BusinessShareProfit
        public ActionResult DistributionFenRun(BusinessShareProfit bs)
        {
            ViewBag.BSTitle = "添加";
            var model = ct.BusinessShareProfit.Where(o => o.Id == bs.Id).FirstOrDefault();
            if (model != null)
            {
                if (bs.Extention == "" || bs.Extention == null)
                {
                    ViewBag.BSTitle = "修改";
                    model.Extention = "0";
                    ViewBag.BusinessShareProfit = model;
                    return View();
                }
                else
                {
                    model.S0_0 = bs.S0_0;
                    model.S1_1 = bs.S1_1;
                    model.S1_4 = bs.S1_4;
                    model.S1_5 = bs.S1_5;
                    model.S1_6 = bs.S1_6;
                    model.S2_5_4 = bs.S2_5_4;
                    model.S2_6_4 = bs.S2_6_4;
                    model.S2_6_5 = bs.S2_6_5;
                    model.S3_6_5_4 = bs.S3_6_5_4;
                    try
                    {
                        ct.SaveChanges();
                        return Content("<script>alert('修改成功！');</script>");
                    }
                    catch
                    {
                        return Content("<script>alert('修改失败！');</script>");
                    }
                }
            }
            else
            {
                if (bs.S0_0 != null)
                {
                    try
                    {
                        ct.BusinessShareProfit.Add(bs);
                        ct.SaveChanges();
                        return Content("<script>alert('添加成功！');</script>");
                    }
                    catch
                    {
                        return Content("<script>alert('添加失败！');</script>");
                    }
                }
                else
                {
                    return View();
                }
            }
        }
        public ActionResult FenRunByUsers(string Name = "", string Phone = "", int id = 1)
        {
            var model = ct.Users.OrderByDescending(o => o.AddTime).ToList();
            if (!string.IsNullOrWhiteSpace(Phone))
            {
                model = model.Where(o => o.UserName == Phone).ToList();
            }
            if (!string.IsNullOrWhiteSpace(Name))
            {
                model = model.Where(o => o.TrueName == Name).ToList();
            }
            model = model.ToPagedList(id, 10);
            if (Request.IsAjaxRequest())
                return PartialView("_FenRunByUsersTable", model);
            return View(model);
        }
    }
}