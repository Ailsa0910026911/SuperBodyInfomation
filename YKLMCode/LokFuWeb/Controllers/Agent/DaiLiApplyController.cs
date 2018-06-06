using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    /// <summary>
    /// 金牌代理(业务已停用)
    /// </summary>
    public class DaiLiApplyController : BaseController
    {
        public ActionResult Index(EFPagingInfo<DaiLiApply> p)
        {
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<DaiLiApply> DaiLiApplyList = Entity.Selects<DaiLiApply>(p);
            ViewBag.DaiLiApplyList = DaiLiApplyList;
            return View();
        }
        public ActionResult Edit()
        {
            return View();
        }
        [ValidateInput(false)]
        public void Add(DaiLiApply DaiLiApply)
        {
            //TODO:因为代理级别业务修改，这里这要改变
            //if (BasicAgent.Levels == 3) {
            //    DaiLiApply Temp = Entity.DaiLiApply.FirstOrDefault(n => n.Agent == BasicAgent.Id && (n.OrderState == 1 || n.OrderState == 2));
            //    if (Temp != null) {
            //        Response.Write("有未处理完成订单在处理中~");
            //        return;
            //    }
            //}
            DaiLiApply = Request.ConvertRequestToModel<DaiLiApply>(DaiLiApply, DaiLiApply);
            DaiLiApply.AddTime = DateTime.Now;
            DaiLiApply.Agent = BasicAgent.Id;
            DaiLiApply.AId = AdminUser.Id;
            DaiLiApply.Amoney = 0;
            DaiLiApply.OrderState = 1;//1待处理 2已开通 3取消
            if (DaiLiApply.Area.IsNullOrEmpty()) {
                int Area = 0;
                try
                {
                    Area = Int32.Parse(DaiLiApply.Area);
                }
                catch (Exception) { }
                if (!Area.IsNullOrEmpty()) {
                    BasicCity BasicCity = Entity.BasicCity.FirstOrNew(n => n.Id == Area);
                    BasicProvince BasicProvince = Entity.BasicProvince.FirstOrNew(n => n.Id == BasicCity.PId);
                    DaiLiApply.Area = BasicProvince.Name + "|" + BasicCity.Name;
                }
            }
            //if (BasicAgent.Levels == 3) {
            //    DaiLiApply.Remark = "金牌代理申请升级钻石代理";
            //}
            //Entity.DaiLiApply.AddObject(DaiLiApply);
            //Entity.SaveChanges();
            //if (BasicAgent.Levels == 3) {
            //    Response.Redirect("/Agent/DaiLiApply/Edit.html");
            //}
            //if (BasicAgent.Levels == 4)
            //{
            //    Response.Redirect("/Agent/DaiLiApply/Index.html");
            //}
        }
        public void ChangeStatus(DaiLiApply DaiLiApply)
        {
            DaiLiApply = Entity.DaiLiApply.FirstOrNew(n => n.Id == DaiLiApply.Id);
            if (DaiLiApply.OrderState == 1)
            {
                DaiLiApply.OrderState = 0;
                Entity.SaveChanges();
            }
            Response.Redirect("/Agent/DaiLiApply/Index.html");
        }
    }
}
