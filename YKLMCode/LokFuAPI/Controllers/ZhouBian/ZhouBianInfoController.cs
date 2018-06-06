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
    public class ZhouBianInfoController : InitController
    {
        public static string AccToken = "";
        public ZhouBianInfoController()
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
                Log.Write("[ZhouBianInfo]:", "【Data】" + Data, Ex);
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
            if (baseUsers.NeekName.IsNullOrEmpty()) {
                baseUsers.NeekName = baseUsers.TrueName;
                Entity.SaveChanges();
            }

            string wxtoken = GetToken();

            YYDevice YYDevice = Entity.YYDevice.FirstOrDefault(n => n.UId == baseUsers.Id && n.State == 2);//已绑定页面
            if (YYDevice == null) {
                //获取设备
                YYDevice = Entity.YYDevice.FirstOrDefault(n => n.UId == 0 && n.State == 1);//未绑定
                if (YYDevice == null) { 
                    //没有可用的设备
                    YYDevice = GetYYDevice(wxtoken);
                }
                if (YYDevice != null) {
                    YYDevice.UId = baseUsers.Id;
                    YYDevice.State = 2;
                    Entity.SaveChanges();
                }
            }
            if (YYDevice.PageId.IsNullOrEmpty()) { 
                //未绑定页面
                YYPage YYPage = Entity.YYPage.FirstOrDefault(n => n.UId == baseUsers.Id && n.State == 1);
                if (YYPage == null) {
                    //生成页面
                    string title = GetNeekName(baseUsers.NeekName);//从商户名称中获取6位
                    string description = "点击去付款";
                    string URL = string.Format(SysPath + "/mobile/shop/index-{0}.html", baseUsers.Id, baseUsers.UserName);
                    //string Icon = Utils.ImageUrl("UsersPic", baseUsers.Pic);
                    string Icon = Utils.ImageUrl("UsersPic", baseUsers.Pic, 120, 120, "Cut");
                    string IconPath = HttpContext.Current.Server.MapPath(Icon);
                    UploadImageResultJson Img = WXAPI.UploadImage(wxtoken, IconPath);
                    string wxpicurl = Img.data.pic_url;
                    AddPageResultJson Page = WXAPI.AddPage(wxtoken, title, description, URL, wxpicurl);
                    if (Page.errcode == ReturnCode.请求成功)
                    {
                        YYPage = new YYPage();
                        YYPage.Title = title;
                        YYPage.UId = baseUsers.Id;
                        YYPage.SubTitle = description;
                        YYPage.Url = URL;
                        YYPage.Icon = Icon;
                        YYPage.WXIcon = wxpicurl;
                        YYPage.AddTime = DateTime.Now;
                        YYPage.State = 1;
                        YYPage.PageId = Page.data.page_id.ToString();
                        Entity.YYPage.AddObject(YYPage);
                        Entity.SaveChanges();
                    }
                }
                //绑定页面
                long DevId = long.Parse(YYDevice.DevId);
                long PageId = long.Parse(YYPage.PageId);
                WxJsonResult WJR = WXAPI.BindPage(wxtoken, DevId, PageId);
                if (WJR.errcode == ReturnCode.请求成功) {
                    YYDevice.PageId = YYPage.PageId;
                    baseUsers.YYOpenState = 1;//标识用户已经开通
                    Entity.SaveChanges();
                }
            }

            YYDaily YYDaily = Entity.YYDaily.Where(n => n.UId == baseUsers.Id && n.DevId == YYDevice.DevId && n.OutDate < DateTime.Now).OrderByDescending(n => n.OutDate).FirstOrDefault();
            if (YYDaily == null)
            {
                YYDevice.ClickPV = 0;
                YYDevice.ClickUV = 0;
                YYDevice.ShakePV = 0;
                YYDevice.ShakeUV = 0;
            }else{
                YYDevice.ClickPV = YYDaily.ClickPV;
                YYDevice.ClickUV = YYDaily.ClickUV;
                YYDevice.ShakePV = YYDaily.ShakePV;
                YYDevice.ShakeUV = YYDaily.ShakeUV;
            }
            DataObj.Data = YYDevice.OutJson();
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
        private YYDevice GetYYDevice(string token, int Times = 0)
        {
            //查询是否有申请未返回状态
            YYApply YYApply = Entity.YYApply.FirstOrDefault(n => n.State == 1);
            if (YYApply == null) {
                string Reason = "新加商店设备";
                DeviceApplyResultJson Device = WXAPI.DeviceApply(token, Reason);
                if (Device.errcode == ReturnCode.请求成功) {
                    YYApply=new YYApply();
                    YYApply.AddTime = DateTime.Now;
                    YYApply.Reason = Reason;
                    YYApply.ApplyId = Device.data.apply_id.ToString();
                    YYApply.Num = 50;
                    YYApply.State = 1;
                    if (Device.data.audit_status == 0) {
                        YYApply.State = 0;
                    }
                    Entity.YYApply.AddObject(YYApply);
                    Entity.SaveChanges();
                    if (Device.data.audit_status == 2)
                    {
                        YYApply.State = 2;
                    }
                }
            }
            if (YYApply.State == 1)
            {
                YYApply = GetYYApply(token, YYApply);
            }
            if (YYApply.State == 0) {
                if (Times < 5)
                {
                    Thread.Sleep(5000);
                    return GetYYDevice(token, Times++);
                }
            }
            if (YYApply.State == 2)
            {
                int ApplyId = Int32.Parse(YYApply.ApplyId);          
                DeviceSearchResultJson Devices = WXAPI.SearchDeviceByApplyId(token, ApplyId);
                if (Devices.errcode == ReturnCode.请求成功) {
                    List<DeviceSearch_Data_Devices> DeviceList = Devices.data.devices;
                    foreach (var p in DeviceList) {
                        var minor = p.minor.ToString();
                        var savedata = this.Entity.YYDevice.FirstOrDefault(o => o.Minor == minor);
                        if (savedata == null)               
                        {
                            //设备入库
                            YYDevice YY = new YYDevice();
                            YY.DevId = p.device_id.ToString();
                            YY.UUID = p.uuid;
                            YY.Major = p.major.ToString();
                            YY.Minor = p.minor.ToString();
                            YY.UId = 0;
                            YY.PageId = string.Empty;
                            YY.AddTime = DateTime.Now;
                            YY.ActState = 0;
                            YY.State = 1;
                            Entity.YYDevice.AddObject(YY);
                        }
                    }
                    Entity.SaveChanges();
                }
            }
            YYDevice YYDevice = Entity.YYDevice.FirstOrDefault(n => n.UId == 0 && n.State == 1);
            return YYDevice;
        }
        private YYApply GetYYApply(string token, YYApply YYApply, int Times = 0)
        {
            if (YYApply.State == 1) {
                int ApplyId = Int32.Parse(YYApply.ApplyId);
                GetDeviceStatusResultJson DeviceStatus = WXAPI.DeviceApplyStatus(token, ApplyId);
                if (DeviceStatus.errcode == ReturnCode.请求成功) {
                    if (DeviceStatus.data.audit_status == 2) {
                        YYApply.State = 2;
                    }
                    if (DeviceStatus.data.audit_status == 0)
                    {
                        YYApply.State = 0;
                    }
                }
            }
            if (YYApply.State == 1) {
                if (Times < 10)
                {
                    Thread.Sleep(5000);
                    return GetYYApply(token, YYApply, Times++);
                }
            }
            return YYApply;
        }
    }
}
