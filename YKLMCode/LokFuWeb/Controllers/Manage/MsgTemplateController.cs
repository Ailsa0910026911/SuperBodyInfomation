using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    public class MsgTemplateController : BaseController
    {
        public ActionResult MsgText()
        {
            IList<MsgTemplate> MsgTemplateList = Entity.MsgTemplate.Where(n => n.SendWay == 1).ToList();
            ViewBag.MsgTemplateList = MsgTemplateList;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        public ActionResult MsgEmail()
        {
            IList<MsgTemplate> MsgTemplateList = Entity.MsgTemplate.Where(n => n.SendWay == 2).ToList();
            ViewBag.MsgTemplateList = MsgTemplateList;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        public ActionResult MsgDuanXin()
        {
            IList<MsgTemplate> MsgTemplateList = Entity.MsgTemplate.Where(n => n.SendWay == 3).ToList();
            ViewBag.MsgTemplateList = MsgTemplateList;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        public ActionResult MsgWeiXin()
        {
            IList<MsgTemplate> MsgTemplateList = Entity.MsgTemplate.Where(n => n.SendWay == 4).ToList();
            ViewBag.MsgTemplateList = MsgTemplateList;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        public ActionResult MsgPush()
        {
            IList<MsgTemplate> MsgTemplateList = Entity.MsgTemplate.Where(n => n.SendWay == 5).ToList();
            ViewBag.MsgTemplateList = MsgTemplateList;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        [ValidateInput(false)]
        public void Index(MsgTemplate MsgTemplate)
        {
            MsgTemplate baseMsgTemplate = Entity.MsgTemplate.FirstOrDefault(n => n.Id == MsgTemplate.Id);
            baseMsgTemplate = Request.ConvertRequestToModel<MsgTemplate>(baseMsgTemplate, MsgTemplate);
            Entity.SaveChanges();
            Response.Write("OK");
        }

        public ActionResult MsgPushInner()
        {
            IList<MsgTemplate> MsgTemplateList = Entity.MsgTemplate.Where(n => n.SendWay == 6).ToList();
            ViewBag.MsgTemplateList = MsgTemplateList;
            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
    }
}
