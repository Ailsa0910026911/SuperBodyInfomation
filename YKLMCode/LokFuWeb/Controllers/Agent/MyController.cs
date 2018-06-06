using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Extensions;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Linq;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
namespace LokFu.Areas.Agent.Controllers
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
            p.SqlWhere.Add(f => (f.NType == 0 || f.NType == 2) && f.State == 1);
            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<MsgNotice> MsgNoticeList = Entity.Selects<MsgNotice>(p);
            ViewBag.MsgNoticeList = MsgNoticeList;
            return View();
        }
        public ActionResult NoticeInfo(int Id)
        {
            MsgNotice MsgNotice = Entity.MsgNotice.FirstOrNew(n => n.Id == Id && (n.NType == 0 || n.NType == 2) && n.State == 1);
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
            else {
                ViewBag.ErrorMsg = AgentLanguage.Surmount;
                return View("Error");
            }
            Entity.SaveChanges();
            ViewBag.MsgAdmin = MsgAdmin;
            return View();
        }
        public ActionResult Info(string type)
        {
            if (!type.IsNullOrEmpty())
            {
                ViewBag.ViewType = type;
            }
            return View();
        }

        [AdminFilter(true,"修改个人信息")]
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
        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCode(string Tel, byte CType)
        {
          
            if (Tel.IsNullOrEmpty())
            {
                return Json(new { code = 1, msg = "手机号不能为空" });
            }
            if (CType != 5)
            {
                return Json(new { code = 1, msg = " 非法操作" });
            }
            //验证是否重复
            SysAdmin Old = Entity.SysAdmin.FirstOrDefault(n => n.UserName == Tel);
            if (Old != null)
            {
                return Json(new { code = 1, msg = "“联系手机号”已在系统中存在，无法开通管理员！" });
            }
            if (Entity.UserBlackList.FirstOrDefault(UBL => UBL.CardNumber == Tel && UBL.State == 1) != null)
            {
                return Json(new { code = 1, msg = "暂不支持您的手机号入网！" });
            }
            SysSet ss = new SysSet();
            ss.SMSEnd = BasicSet.SMSEnd;
            ss.SMSActives = BasicSet.SMSActives;
            DateTime Today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
           
            //失效之前获取验证码
            IList<SMSCode> List = Entity.SMSCode.Where(n => n.UId == BasicAgent.Id && n.Mobile == Tel && n.CType == CType && n.State == 1).ToList();
            foreach (var p in List)
            {
                p.State = 0;
            }
            Entity.SaveChanges();
            //生成验证码
            string Code = Utils.RandomSMSCode(6);
             
            SMSCode SSC = new SMSCode();
            SSC.CType = CType;
            SSC.UId = BasicAgent.Id;
            SSC.Mobile = Tel;
            SSC.Code = Code;
            SSC.AddTime = DateTime.Now;
            SSC.State = 1;
            Entity.SMSCode.AddObject(SSC);
            Entity.SaveChanges();
            var topAgent = BasicAgent.GetTopAgent(this.Entity);
            //发送验证码
            SSC.SendSMS(ss, topAgent, Entity);

            return Json(new { code = 2, msg = "ok" });
        }
        /// <summary>
        /// 检验验证码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckCode(string Tel, byte CType,string code)
        {

            if (Tel.IsNullOrEmpty())
            {
                return Json(new { code = 1, msg = "手机号不能为空" });
            }
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            //失效之前获取验证码
            SMSCode SMSCode = Entity.SMSCode.OrderByDescending(n => n.Id).FirstOrDefault(n => n.UId == BasicAgent.Id && n.Mobile == Tel && n.CType == CType && n.Code == code);
            if (SMSCode == null)
            {
                return Json(new { code = 1, msg = "验证码错误" });
            }
            if (SMSCode.State != 1)
            {
                return Json(new { code = 1, msg = "验证码已被使用过" });
            }
            if (SMSCode.AddTime.AddMinutes(SysSet.SMSActives) < DateTime.Now)
            {
                return Json(new { code = 1, msg = "验证码已失效" });
            }

            return Json(new { code = 2, msg = "ok" });
        }
        public ActionResult QRCode()
        {
            return View();
        }
    }
}
