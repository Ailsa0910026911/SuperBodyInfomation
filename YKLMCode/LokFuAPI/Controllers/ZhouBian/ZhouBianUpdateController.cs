using System.Collections.Generic;
using System.Linq;
using System;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LokFu.Extensions;
using System.Configuration;
using LokFu.WeiXin.Repositories;
using LokFu.WeiXin;
using System.Threading;
using System.Web;

namespace LokFu.Controllers
{
    public class ZhouBianUpdateController : InitController
    {
        public static string AccToken = "";
        public ZhouBianUpdateController()
        {
            if (!InitState)
            {
                DataObj.OutError("8080");
                return;
            }
            if (DataObj == null)
            {
                DataObj.OutError("1000");
                return;
            }
            if (!DataObj.IsReg)
            {
                DataObj.OutError("3002");
                return;
            }
        }
        public void Post()
        {
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[ZhouBianUpdate]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            //JObject pp = (JObject)p;
            Users Users = new Users();
            Users = JsonToObject.ConvertJsonToModel(Users, json);
            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Users.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.Pic.IsNullOrEmpty())//未上传头象
            {
                DataObj.OutError("2041");
                return;
            }

            string wxtoken = GetToken();

            YYDevice YYDevice = Entity.YYDevice.FirstOrDefault(n => n.UId == baseUsers.Id && n.State == 2);//已绑定页面
            YYPage YYPage = Entity.YYPage.FirstOrDefault(n => n.UId == baseUsers.Id && n.State == 1);

            string title = GetNeekName(baseUsers.NeekName);//从商户名称中获取6位
            string description = "点击去付款";
            string URL = string.Format(SysPath + "/mobile/shop/index-{0}.html", baseUsers.Id, baseUsers.UserName);
            string Icon = Utils.ImageUrl("UsersPic", baseUsers.Pic, 120, 120, "Cut");

            if (Icon != YYPage.Icon || title != YYPage.Title) {
                string wxpicurl = YYPage.WXIcon;
                if (Icon != YYPage.Icon)//修改了头象则需要重新上传头象
                {
                    string IconPath = HttpContext.Current.Server.MapPath(Icon);
                    UploadImageResultJson Img = WXAPI.UploadImage(wxtoken, IconPath);
                    wxpicurl = Img.data.pic_url;
                    long page_id = Int64.Parse(YYPage.PageId);
                    UpdatePageResultJson Page = WXAPI.EditPage(wxtoken, page_id, title, description, URL, wxpicurl);
                    if (Page.errcode == ReturnCode.请求成功)
                    {
                        YYPage.Title = title;
                        YYPage.Icon = Icon;
                        YYPage.WXIcon = wxpicurl;
                        Entity.SaveChanges();
                    }
                }
            }
            DataObj.Data = "";
            DataObj.Code = "0000";
            DataObj.OutString();
        }
        private string GetToken() {
            string AppId = ConfigurationManager.AppSettings["wxAppId"].ToString();
            string AppSecret = ConfigurationManager.AppSettings["wxAppSecret"].ToString();
            AccToken = MemoryCacheBuilder.EntityCache.Get("access_token", null) as string;
            if (AccToken.IsNullOrEmpty())
            {
                AccToken = WXAPI.GetToken(AppId, AppSecret, "client_credential").access_token;
                MemoryCacheBuilder.EntityCache.Remove("access_token", null);
                MemoryCacheBuilder.EntityCache.Add("access_token", AccToken, DateTime.Now.AddMinutes(60.0), null);
            }
            return AccToken;
        }
        private string GetNeekName(string name) {
            if (name.Length > 6)
            {
                if (name.IndexOf("市") != -1)
                {
                    name = name.Replace(name.Split('市')[0] + "市", "");
                }
                if (name.Length > 6) {
                    name = name.Substring(0, 5) + "…";
                }
            }
            return name;
        }

    }
}
