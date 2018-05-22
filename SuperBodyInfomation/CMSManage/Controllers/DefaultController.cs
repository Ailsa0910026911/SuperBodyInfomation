using CMSManage.Extended;
using CTCommon;
using CTModel;
using LokFu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;

namespace CMSManage.Controllers
{
    public class DefaultController : Controller
    {
        private CTContext ct = new CTContext();
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
    }
}