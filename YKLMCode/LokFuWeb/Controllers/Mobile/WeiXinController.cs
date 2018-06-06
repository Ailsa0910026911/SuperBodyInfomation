using System;
using System.Collections.Generic;
using System.Linq;
using LokFu.Repositories;
using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.WeiXin;
using System.Configuration;
using LokFu.WeiXin.Repositories;
namespace LokFu.Areas.Mobile.Controllers
{
    public class WeiXinController : InitController
    {
        public void GetOpenId(string state, string code)
        {
            if (code == null) {
                Response.Write("Null");
                return;
            }
            string AppId = ConfigurationManager.AppSettings["wxAppId"].ToString();
            string AppSecret = ConfigurationManager.AppSettings["wxAppSecret"].ToString();
            string reg = string.Empty;
            wxLogin wxLogin = new wxLogin();
            ACCESST ACCESST = wxLogin.GetWeiXinAccess(code, AppId, AppSecret, out reg);
            if (ACCESST.openid.IsNullOrEmpty())
            {
                Response.Redirect("/Mobile/WeiXinErr.html?msg=您未授权，暂时不能参与活动");
                return;
            }
            Response.Cookies.SetWXOpenId(ACCESST.openid);
            WeiXinUsers WeiXinUsers = Entity.WeiXinUsers.FirstOrNew(n => n.OpenId == ACCESST.openid);
            string BackUrl = "";
            if (Request.QueryString["BackUrl"] != null)
            {
                if (!Request.QueryString["BackUrl"].ToString().IsNullOrEmpty())
                {
                    BackUrl = Request.QueryString["BackUrl"].ToString();
                }
            }
            if (WeiXinUsers.Id == 0)
            {
                if (state == "Base") {
                    string burl = "http://" + Utils.GetHostName() + "/Mobile/Weixin/GetOpenId.html?BackUrl=" + BackUrl + "";
                    burl = System.Web.HttpUtility.UrlEncode(burl);
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + AppId + "&redirect_uri=" + burl + "&response_type=code&scope=snsapi_userinfo&state=UserInfo#wechat_redirect";
                    Response.Redirect(url);
                }
                if (state == "UserInfo")
                {
                    WeiXinUser WeiXinUser = wxLogin.GetWeiXinUser(ACCESST.access_token, ACCESST.openid, out reg);
                    WeiXinUsers = new WeiXinUsers();
                    WeiXinUsers.UId = 0;
                    WeiXinUsers.OpenId = WeiXinUser.openid;
                    WeiXinUsers.ComeId = 0;
                    WeiXinUsers.AddTime = DateTime.Now;
                    WeiXinUsers.State = 1;
                    WeiXinUsers.NickName = WeiXinUser.nickname;
                    WeiXinUsers.Sex = WeiXinUser.sex;
                    WeiXinUsers.Province = WeiXinUser.province;
                    WeiXinUsers.City = WeiXinUser.city;
                    WeiXinUsers.Country = WeiXinUser.country;
                    WeiXinUsers.HeadImgUrl = WeiXinUser.headimgurl;
                    WeiXinUsers.Privilege = WeiXinUser.privilege;
                    Entity.WeiXinUsers.AddObject(WeiXinUsers);
                    Entity.SaveChanges();
                }
            }
            Response.Redirect(BackUrl);
        }
    }
    public class WeiXinBaseController : InitController
    {
        public bool IsWeiXinBrowser = false;
        public string AppId = "";
        public string AppSecret = "";
        public WeiXinUsers WeiXinUsers = new WeiXinUsers();//当前微信用户
        public SysSet BasicSet;
        public WeiXinBaseController()
        {
            AppId = ConfigurationManager.AppSettings["wxAppId"].ToString();
            AppSecret = ConfigurationManager.AppSettings["wxAppSecret"].ToString();
            IsWeiXinBrowser = System.Web.HttpContext.Current.Request.UserAgent.ToLower().Contains("micromessenger");
            if (IsWeiXinBrowser) {
                //System.Web.HttpContext.Current.Response.Cookies.SetWXOpenId("orL8iwlAz9nORcOb4Gq0PNeCedqY");
                string openid = System.Web.HttpContext.Current.Request.Cookies.GetWXOpenId();
                if (!openid.IsNullOrEmpty())
                {
                    WeiXinUsers = Entity.WeiXinUsers.FirstOrNew(n => n.OpenId == openid);
                }
                else {
                    WeiXinUsers = new WeiXinUsers();
                }
                if (WeiXinUsers.Id.IsNullOrEmpty())
                {
                    string str = "";
                    if (System.Web.HttpContext.Current.Request.Url != null)
                    {
                        str = System.Web.HttpContext.Current.Request.Url.ToString();
                    }
                    string burl = "http://" + Utils.GetHostName() + "/Mobile/Weixin/GetOpenId.html?BackUrl=" + str + "";
                    burl = System.Web.HttpUtility.UrlEncode(burl);
                    string url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=" + AppId + "&redirect_uri=" + burl + "&response_type=code&scope=snsapi_base&state=Base#wechat_redirect";
                    System.Web.HttpContext.Current.Response.Redirect(url);
                    return;
                }
            }
            BasicSet = Entity.SysSet.FirstOrNew();
            ViewBag.BasicSet = BasicSet;
            ViewBag.WeiXinUsers = WeiXinUsers;
            ViewBag.AppId = AppId;
            ViewBag.AppSecret = AppSecret;
        }
    }
}