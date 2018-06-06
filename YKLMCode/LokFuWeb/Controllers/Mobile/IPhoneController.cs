using System.Linq;
using System.Web.Mvc;
using LokFu.Repositories;
using LokFu.Extensions;
using System;
namespace LokFu.Areas.Mobile.Controllers
{
    public class IPhoneController : WeiXinBaseController
    {
        public bool IsOutTime = false;
        public CutAct CutAct = new CutAct();
        public CutUsers CutUsers = new CutUsers();//当前页面用户
        public CutUsers CutPageUsers = new CutUsers();//当前页面用户
        public bool isMyPage = false;//是否有管理权限
        public IPhoneController()
        {
            if (!IsWeiXinBrowser)
            {
                return;
            }
            if (WeiXinUsers.Id.IsNullOrEmpty())
            {
                return;
            }
            CutAct = Entity.CutAct.FirstOrNew(n => n.Id == 2);
            int cid = 0;
            if (System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["cid"] != null)
            {
                try
                {
                    cid = Int32.Parse(System.Web.HttpContext.Current.Request.RequestContext.RouteData.Values["cid"].ToString());
                    CutPageUsers = Entity.CutUsers.FirstOrNew(n => n.Id == cid);
                }
                catch (Exception) { }
            }
            CutUsers = Entity.CutUsers.FirstOrNew(n => n.WXId == WeiXinUsers.Id && n.CAId == CutAct.Id);
            if (CutUsers.Id.IsNullOrEmpty()) {
                CutUsers = new CutUsers();
                CutUsers.WXId = WeiXinUsers.Id;
                CutUsers.CAId = CutAct.Id;
                CutUsers.NickName = WeiXinUsers.NickName;
                CutUsers.Times = 0;
                CutUsers.AllPrice = CutAct.Amount;
                CutUsers.MyPrice = CutAct.Amount;
                CutUsers.CutPrice = 0;
                CutUsers.AddTime = DateTime.Now;
                CutUsers.State = 1;
                Entity.CutUsers.AddObject(CutUsers);
                Entity.SaveChanges();
            }
            if (CutPageUsers.Id.IsNullOrEmpty()) {
                CutPageUsers = CutUsers;
            }
            if (CutUsers.Id == CutPageUsers.Id)
            {
                isMyPage = true;
            }
            if (CutPageUsers.AddTime.AddDays(CutAct.OutTime) < DateTime.Now)
            {
                IsOutTime = true;
            }
            ViewBag.CutAct = CutAct;
            ViewBag.IsOutTime = IsOutTime;
            ViewBag.CutPageUsers = CutPageUsers;
            ViewBag.CutUsers = CutUsers;
            ViewBag.isMyPage = isMyPage;
        }
        public ActionResult Index()
        {
            if (!IsWeiXinBrowser)
            {
                ViewBag.ErrorMsg = "请在微信客户端打开链接";
                return View("WeiXinErr");
            }
            if (CutUsers.Id.IsNullOrEmpty())
            {
                ViewBag.ErrorMsg = "您未授权，暂时不能参与活动[02]";
                return View("WeiXinErr");
            }
            if (CutPageUsers.CAId != CutAct.Id)
            {
                ViewBag.ErrorMsg = "活动信息有误";
                return View("WeiXinErr");
            }
            return View();
        }
        public void Self()
        {
            if (!IsWeiXinBrowser)
            {
                Response.Redirect("/Mobile/WeiXinErr.html?msg=请在微信客户端打开链接");
                return;
            }
            if (CutUsers.Id.IsNullOrEmpty())
            {
                Response.Redirect("/Mobile/WeiXinErr.html?msg=您未授权，暂时不能参与活动[02]");
                return;
            }
            Response.Redirect("/Mobile/IPhone/Index-" + CutUsers.Id + ".html");
        }
        public void ReSet()
        {
            if (!IsWeiXinBrowser)
            {
                Response.Redirect("/Mobile/WeiXinErr.html?msg=请在微信客户端打开链接");
                return;
            }
            if (CutUsers.Id.IsNullOrEmpty())
            {
                Response.Redirect("/Mobile/WeiXinErr.html?msg=您未授权，暂时不能参与活动[02]");
                return;
            }
            Entity.ExecuteStoreCommand("Update CutUsers Set IsDel=1 Where Id=" + CutUsers.Id);
            Response.Redirect("Self.html");
        }
        public void GetMoney() {
            if (!IsWeiXinBrowser)
            {
                Response.Write("e0");
                return;
            }
            if (CutUsers.Id.IsNullOrEmpty())
            {
                Response.Write("e0");
                return;
            }
            if (CutPageUsers.CAId != CutAct.Id)
            {
                Response.Write("e0");
                return;
            }
            if (IsOutTime)
            {
                Response.Write("e9");
                return;
            }
            CutLog CutLog = Entity.CutLog.FirstOrDefault(n => n.CUId == CutUsers.Id && n.CUIded == CutPageUsers.Id && n.CAId == CutAct.Id);
            if (CutLog != null)
            {
                Response.Write("e1");
                return;
            }
            decimal getMoney = Utils.CutMoney(CutPageUsers.MyPrice);
            if (getMoney == 0)
            {
                Response.Write("e0");
            }
            else {
                CutPageUsers.MyPrice -= getMoney;
                CutPageUsers.CutPrice += getMoney;
                CutPageUsers.Times++;
                CutLog Log = new CutLog();
                Log.CAId = CutAct.Id;
                Log.CUId = CutUsers.Id;
                Log.NickName = CutUsers.NickName;
                Log.CUIded = CutPageUsers.Id;
                Log.NickNamed = CutPageUsers.NickName;
                Log.Amoney = getMoney;
                Log.AddTime = DateTime.Now;
                Entity.CutLog.AddObject(Log);
                Entity.SaveChanges();
                Response.Write(((double)getMoney).ToString());
            }
        }
        public void CheckMy() {
            if (!IsWeiXinBrowser)
            {
                Response.Write("e0");
                return;
            }
            if (CutUsers.Id.IsNullOrEmpty())
            {
                Response.Write("e0");
                return;
            }
            if (CutPageUsers.CAId != CutAct.Id)
            {
                Response.Write("e0");
                return;
            }
            if (CutUsers.Id != CutPageUsers.Id) {//当前与登录不同
                Response.Write("e0");
                return;
            }
            if (IsOutTime)
            {
                Response.Write("e9");
                return;
            }
            if (CutUsers.MyPrice > 0)
            {
                Response.Write("e1");
                return;
            }
            string caid = string.Format("[{0}]", CutAct.Id);
            MsgCallBack Msg = Entity.MsgCallBack.FirstOrDefault(n => n.AId == CutUsers.Id && n.Info.Contains(caid));
            if (Msg != null)
            {
                Response.Write("e5");
                return;
            }
            Response.Write("ok");
        }
        public void TakeMy(string uname, string tel, string address)
        {
            if (uname.IsNullOrEmpty() || tel.IsNullOrEmpty() || address.IsNullOrEmpty())
            {
                Response.Write("e0");
                return;
            }
            if (!IsWeiXinBrowser)
            {
                Response.Write("e0");
                return;
            }
            if (CutUsers.Id.IsNullOrEmpty())
            {
                Response.Write("e0");
                return;
            }
            if (CutPageUsers.CAId != CutAct.Id)
            {
                Response.Write("e0");
                return;
            }
            if (CutUsers.Id != CutPageUsers.Id) {//当前与登录不同
                Response.Write("e0");
                return;
            }
            if (CutUsers.MyPrice > 0)
            {
                Response.Write("e1");
                return;
            }
            if (IsOutTime)
            {
                Response.Write("e9");
                return;
            }
            string caid = CutAct.Id.ToString();
            MsgCallBack Msg = Entity.MsgCallBack.FirstOrDefault(n => n.AId == CutUsers.Id && n.Info == caid);
            if (Msg != null) {
                Response.Write("e5");
                return;
            }
            MsgCallBack MsgCallBack = new MsgCallBack();
            MsgCallBack.UId = 0;
            MsgCallBack.State = 1;
            MsgCallBack.AId = CutUsers.Id;
            MsgCallBack.AddTime = DateTime.Now;
            MsgCallBack.NeekName = uname;
            MsgCallBack.Linker = tel;
            MsgCallBack.Info = "[" + CutAct.Id.ToString() + "]" + address;
            MsgCallBack.Name = "领奖：[" + CutAct.Name + "]";
            Entity.MsgCallBack.AddObject(MsgCallBack);
            Entity.SaveChanges();
            //短信通知异常
            SMSLog SMSLog = new SMSLog();
            SMSLog.UId = 0;
            SMSLog.Mobile = "18948755511";
            SMSLog.SendText = "领奖通知：[" + CutAct.Name + "]" + uname + "【" + BasicSet.SMSEnd + "】"; ;
            SMSLog.State = 1;
            SMSLog.AddTime = DateTime.Now;
            Entity.SMSLog.AddObject(SMSLog);
            Entity.SaveChanges();
            Response.Write("ok");
        }
        public void JuBao(string jubao) {
            MsgCallBack MsgCallBack = new MsgCallBack();
            MsgCallBack.UId = 0;
            MsgCallBack.State = 1;
            MsgCallBack.AId = 0;
            MsgCallBack.AddTime = DateTime.Now;
            MsgCallBack.NeekName = CutUsers.NickName;
            MsgCallBack.Linker = CutPageUsers.NickName;
            MsgCallBack.Info = jubao + "[|]" + CutUsers.Id + "[|]" + CutPageUsers.Id;
            MsgCallBack.Name = "举报：[" + CutAct.Name + "]";
            Entity.MsgCallBack.AddObject(MsgCallBack);
            Entity.SaveChanges();
            Response.Write("ok");
        }
    }
}
 