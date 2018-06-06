using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Extensions;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Manage.Controllers
{
    [AdminFilter(false)]
    public class MyController : BaseController
    {
        public MyController()
        {
            ViewBag.Authorization = true;//允许权限
        }
        public ActionResult Notice(EFPagingInfo<MsgNotice> p)
        {
            p.SqlWhere.Add(f => f.NType == 0 || f.NType == 1);
            p.SqlWhere.Add(f => f.State == 1 && f.AddTime > AdminUser.AddTime);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgNotice> MsgNoticeList = Entity.Selects<MsgNotice>(p);
            ViewBag.MsgNoticeList = MsgNoticeList;
            return View();
        }
        public ActionResult NoticeInfo(int Id)
        {
            MsgNotice MsgNotice = Entity.MsgNotice.FirstOrNew(n => n.Id == Id && n.NType <= 1 && n.State == 1);
            if (MsgNotice.Id > 0)
            {
                string ReadAdmin = MsgNotice.ReadAdmin;
                string AdminId = string.Format("|{0}|", AdminUser.Id);
                if (ReadAdmin.IndexOf(AdminId) == -1)
                {
                    if (ReadAdmin.IsNullOrEmpty())
                    {
                        ReadAdmin = AdminId;
                    }
                    else
                    {
                        ReadAdmin = string.Format("{0}{1}|", ReadAdmin, AdminUser.Id);
                    }
                    MsgNotice.ReadAdmin = ReadAdmin;
                    Entity.SaveChanges();
                }
            }
            ViewBag.MsgNotice = MsgNotice;
            return View();
        }
        public ActionResult Msg(EFPagingInfo<MsgAdmin> p)
        {
            string SysAdmin = string.Format(",{0},", AdminUser.Id);
            p.SqlWhere.Add(f => f.State > 0 && (f.AId == AdminUser.Id || f.SendUsers.Contains(SysAdmin)) && f.AddTime > AdminUser.AddTime);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgAdmin> MsgAdminList = Entity.Selects<MsgAdmin>(p);
            foreach (var item in MsgAdminList)
            {
                if (!item.ReadUsers.IsNullOrEmpty())
                {
                    if (item.ReadUsers.Contains(SysAdmin))
                    {
                        item.State = 2;
                    }
                    else
                    {
                        item.State = 1;
                    }
                }
            }
            ViewBag.MsgAdminList = MsgAdminList;
            return View();
        }
        public ActionResult MsgInfo(int Id)
        {
            string SysAdmin = string.Format(",{0},", AdminUser.Id);
            //MsgInfo
            MsgAdmin MsgAdmin = Entity.MsgAdmin.FirstOrNew(n => n.Id == Id & n.State != 0);
            if (MsgAdmin.AId > 0 && MsgAdmin.AId == AdminUser.Id)
            {
                if (MsgAdmin.State == 1)
                {
                    MsgAdmin.State = 2;
                }
            }
            else if (MsgAdmin.AId == 0 && MsgAdmin.SendUsers.Contains(SysAdmin))
            {
                if (MsgAdmin.ReadUsers != null)
                {
                    SysAdmin = SysAdmin.Substring(1, SysAdmin.Length - 1);
                }
                MsgAdmin.ReadUsers += SysAdmin;
            }
            //if (MsgAdmin.State == 1 && MsgAdmin.ReadUsers == null || !MsgAdmin.ReadUsers.Contains(SysAdmin))
            //{
            //    //如果用户ID大于0并且为当前用户的话  直接修改状态
            //    if (MsgAdmin.AId > 0 && MsgAdmin.AId == AdminUser.Id)
            //    {
            //        if (MsgAdmin.State == 1)
            //        {
            //            MsgAdmin.State = 2;
            //        }
            //    }
            //    //多个用户的情况下 修改ReadUsers字段增加已读状态
            //    else if (MsgAdmin.AId == 0 && MsgAdmin.SendUsers != null || MsgAdmin.SendUsers.Contains(SysAdmin))
            //    {
            //        if (MsgAdmin.ReadUsers != null) { SysAdmin = SysAdmin.Substring(1, SysAdmin.Length - 1); }
            //        MsgAdmin.ReadUsers += SysAdmin;
            //    }
            //}
            Entity.SaveChanges();
            ViewBag.MsgAdmin = MsgAdmin;
            return View();
        }
        public ActionResult Info()
        {
            return View();
        }
        [AdminFilter(true, "修改个人信息")]
        public void InfoSave(SysAdmin SysAdmin, string PassWord1)
        {
            SysAdmin baseSysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == AdminUser.Id);
            SysAdmin.PassWord = baseSysAdmin.PassWord;//保证密码不被修改
            SysAdmin.PowerID = baseSysAdmin.PowerID;//保证权限不被自己修改
            baseSysAdmin = Request.ConvertRequestToModel<SysAdmin>(baseSysAdmin, SysAdmin);
            Entity.SaveChanges();
            BaseRedirect();
        }
        public ActionResult Pass()
        {
            return View();
        }

        [AdminFilter(true, "修改登录密码")]
        public object PassSave(SysAdmin SysAdmin, string PassWord1)
        {
            SysAdmin baseSysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == AdminUser.Id);
            if (!PassWord1.IsNullOrEmpty() && !SysAdmin.PassWord.IsNullOrEmpty())//修改密码
            {
                if (baseSysAdmin.PassWord == SysAdmin.PassWord.GetAdminMD5())//验证原始密码 
                {
                    baseSysAdmin.PassWord = PassWord1.GetAdminMD5();
                }
                else
                {//原密码错误
                    ViewBag.ErrorMsg = "原密码错误，无法修改！";
                    return View("Error");
                }
                Entity.SaveChanges();
            }
            BaseRedirect();
            return true;
        }
        
        [AdminFilter(true, "上线下线QQ")]
        public void QQSave()
        {
            SysAdmin baseSysAdmin = Entity.SysAdmin.FirstOrDefault(n => n.Id == AdminUser.Id);
            if (baseSysAdmin.QQState == 1)
            {

                baseSysAdmin.QQState = 0;
            }
            else {
                baseSysAdmin.QQState = 1;
            }
            Entity.SaveChanges();
            Response.Write("ok");
        }
        public JsonResult EditCheckNumber(int state, string value)
        {
            value = value.Replace(" ", "");
            if (Entity.UserBlackList.FirstOrDefault(u => u.CardNumber == value && u.State == state) != null)
            {
                return Json(new { code = 2 });
            }
            return Json(new { code = 1 });
        }
    }
}
