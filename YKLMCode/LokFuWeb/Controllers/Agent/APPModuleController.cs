using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class APPModuleController : BaseController
    {
        public ActionResult Index(SysAgent SysAgent)
        {
            if (this.BasicAgent.Tier != 1)
            {
                ViewBag.ErrorMsg = "抱歉！该功能只有一级代理可用.";
                return View("Error");
            }
            if (this.BasicAgent.IsTeiPai == 0)
            {
                ViewBag.ErrorMsg = "非贴牌用户无法使用该功能";
                return View("Error");
            }

            var APPModuleList = Entity.APPModule.Where(o => o.AgentId == this.BasicAgent.Id && o.Version == 0).OrderBy(o => o.Id).ToList();
            ViewBag.APPModuleList = APPModuleList;
            //加载类型选项
            string filename = HttpContext.Server.MapPath("/ModuleTypeSelectList.json");
            string jsonstr = System.IO.File.ReadAllText(filename);
            var ModuleTypeSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(jsonstr);
            ViewBag.ModuleTypeSelectList = ModuleTypeSelectList;
            string Bottomfilename = HttpContext.Server.MapPath("/ModuleTypeBottomSelectList.json");
            string Bottomjsonstr = System.IO.File.ReadAllText(Bottomfilename);
            var ModuleTypeBottomSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(Bottomjsonstr);
            ViewBag.ModuleTypeBottomSelectList = ModuleTypeBottomSelectList;
            //加载广告及标识
            AdTag scanCodeTag = Entity.AdTag.FirstOrDefault(o => o.Tag == "SaoBg" && o.State == 1);
            ViewBag.ScanCodeTag = scanCodeTag;
            AdTag bannerTag = Entity.AdTag.FirstOrDefault(o => o.Tag == "banner" && o.State == 1);
            ViewBag.BannerTag = bannerTag;
            if (scanCodeTag == null || bannerTag == null)
            {
                ViewBag.ErrorMsg = "无法找到广告标识";
                return View("Error");
            }
            var scanCodeInfo = Entity.AdInfo.FirstOrNew(o => o.TId == scanCodeTag.Id && o.State == 1 && o.AgentId == this.BasicAgent.Id);
            ViewBag.scanCodeInfo = scanCodeInfo;
            var bannerInfos = Entity.AdInfo.OrderBy(o => o.Sort).Where(o => o.TId == bannerTag.Id && o.State == 1 && o.AgentId == this.BasicAgent.Id).ToList();
            ViewBag.bannerInfos = bannerInfos;

            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }

            //取得系统图标
            string PhysicalApplicationPath = this.HttpContext.Request.PhysicalApplicationPath;
            string APPModelPath = PhysicalApplicationPath + "UpLoadFiles\\APPModule\\";
            var homeDir = new System.IO.DirectoryInfo(APPModelPath + "home");
            var bottomDefDir = new System.IO.DirectoryInfo(APPModelPath + "bottom\\default");
            var bottomActDir = new System.IO.DirectoryInfo(APPModelPath + "bottom\\activate");

            var homeFiles = homeDir.GetFiles();
            var bottomDefFiles = bottomDefDir.GetFiles();
            var bottomActFiles = bottomActDir.GetFiles();

            ViewBag.homeFiles = homeFiles;
            ViewBag.bottomDefaultFiles = bottomDefFiles;
            ViewBag.bottomActFiles = bottomActFiles;

            ViewBag.SysAgent = this.BasicAgent;
            this.ViewBag.IsAdd = this.checkPower("Add");
            this.ViewBag.IsSave = this.checkPower("Save");
            this.ViewBag.IsDelete = this.checkPower("Delete");
            this.ViewBag.IsEdit = this.checkPower("Edit");
            return View();
        }

        [ValidateInput(false)]
        public ActionResult Add(APPModule APPModule)
        {
            if (this.BasicAgent.Tier != 1)
            {
                ViewBag.ErrorMsg = "抱歉！该功能只有一级代理可用.";
                return View("Error");
            }
            if (this.BasicAgent.IsTeiPai == 0)
            {
                ViewBag.ErrorMsg = "非贴牌用户无法使用该功能";
                return View("Error");
            }
            int sort = this.Entity.APPModule.Where(o => o.AgentId == APPModule.AgentId && o.DisplaySite == APPModule.DisplaySite).Max(o => (int?)o.Sort) ?? 0;
            APPModule.AddTime = DateTime.Now;
            APPModule.State = 1;
            APPModule.Sort = sort + 1;
            Entity.APPModule.AddObject(APPModule);
            if (!this.UpdateVersionAll(APPModule.AgentId))
            {
                return null;
            }
            Entity.SaveChanges();
            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
            {
                ClearCacheSysConfig();
            });
            BaseRedirect();
            return null;
        }

        [ValidateInput(false)]
        public ActionResult Save(APPModule APPModule)
        {
            if (this.BasicAgent.Tier != 1)
            {
                ViewBag.ErrorMsg = "抱歉！该功能只有一级代理可用.";
                return View("Error");
            }
            if (this.BasicAgent.IsTeiPai == 0)
            {
                ViewBag.ErrorMsg = "非贴牌用户无法使用该功能";
                return View("Error");
            }
            APPModule baseAPPModule = Entity.APPModule.FirstOrDefault(n => n.Id == APPModule.Id);
            if (baseAPPModule.IsLock == true)
            {
                ViewBag.ErrorMsg = "功能已锁定";
                return View("Error");
            }
            baseAPPModule = Request.ConvertRequestToModel<APPModule>(baseAPPModule, APPModule);
            if (!this.UpdateVersionAll(baseAPPModule.AgentId))
            {
                ViewBag.ErrorMsg = "版本更新失败";
                return View("Error");
            }
            Entity.SaveChanges();
            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
            {
                ClearCacheSysConfig();
            });
            BaseRedirect();
            return null;
        }

        /// <summary>
        /// 保存广告
        /// </summary>
        /// <param name="AdInfo"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SaveAdinfo(AdInfo AdInfo)
        {
            if (this.BasicAgent.Tier != 1)
            {
                ViewBag.ErrorMsg = "抱歉！该功能只有一级代理可用.";
                return View("Error");
            }
            if (this.BasicAgent.IsTeiPai == 0)
            {
                ViewBag.ErrorMsg = "非贴牌用户无法使用该功能";
                return View("Error");
            }
            if (AdInfo.Id.IsNullOrEmpty())
            {
                AdInfo.StartTime = DateTime.Now;
                AdInfo.EndTime = new DateTime(5000, 1, 1);
                AdInfo.State = 1;
                this.Entity.AdInfo.AddObject(AdInfo);
            }
            else
            {
                var baseAdInfo = this.Entity.AdInfo.FirstOrDefault(o => o.Id == AdInfo.Id);
                if (baseAdInfo == null)
                {
                    ViewBag.ErrorMsg = "数据不存在";
                    return View("Error");
                }
                baseAdInfo = Request.ConvertRequestToModel<AdInfo>(baseAdInfo, AdInfo);
            }
            this.Entity.SaveChanges();
            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
            {
                ClearCacheAdInfo();
            });
            //BaseRedirect();
            //return null;
            return this.RedirectToRoute("AgentAction", new { action = "Index", Controller = "APPModule" });
        }

        /// <summary>
        /// 删除广告
        /// </summary>
        /// <param name="AdInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public string DeleteAdinfo(AdInfo AdInfo)
        {
            if (this.BasicAgent.Tier != 1)
            {
                return "0";
            }
            if (this.BasicAgent.IsTeiPai == 0)
            {
                return "0";
            }
            if (AdInfo.Id.IsNullOrEmpty())
            {
                return "0";
            }
            var baseAdInfo = this.Entity.AdInfo.FirstOrDefault(o => o.Id == AdInfo.Id&&o.AgentId==BasicAgent.Id);
            if (baseAdInfo == null)
            {
                return "0";
            }
            this.Entity.AdInfo.DeleteObject(baseAdInfo);
            this.Entity.SaveChanges();
            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
            {
                ClearCacheAdInfo();
            });
            return "1";
        }

        /// <summary>
        /// 升级版本号
        /// </summary>
        /// <param name="AgentId">代理Id</param>
        /// <param name="DisplaySite">区域 1:home 2:底部 3:更多</param>
        /// <returns></returns>
        private bool UpdateVersion(int AgentId, int DisplaySite)
        {
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(o => o.Id == AgentId);
            if (SysAgent != null)
            {
                switch (DisplaySite)
                {
                    case 1:
                        SysAgent.AppMenuHome++;
                        break;
                    case 2:
                        SysAgent.AppMenuBottom++;
                        break;
                    case 3:
                        SysAgent.AppMenuMore++;
                        break;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 删除图标
        /// </summary>
        /// <param name="APPModule"></param>
        [HttpPost]
        public void DeleteAPPModule(APPModule APPModule)
        {
            if (APPModule.Id.IsNullOrEmpty()) { return; }
            APPModule baseAPPModule = Entity.APPModule.FirstOrDefault(n => n.Id == APPModule.Id&&n.AgentId==BasicAgent.Id);

            if (baseAPPModule != null)
            {
                if (baseAPPModule.IsLock == true)
                {
                    Response.Write("2");
                    return;
                }
                if (!this.UpdateVersionAll(baseAPPModule.AgentId))
                {
                    Response.Write(0);
                    return;
                }
                Entity.APPModule.DeleteObject(baseAPPModule);
                Entity.SaveChanges();
                System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
                {
                    ClearCacheSysConfig();
                });
                Response.Write("1");
            }
            else
            {
                Response.Write(0);
                return;
            }

        }

        /// <summary>
        /// 保存排序及显示数量
        /// </summary>
        /// <param name="homeids">中间</param>
        /// <param name="bottomids">底部</param>
        /// <param name="movelistids">更多</param>
        /// <param name="AppBtnNumber">显数量X3</param>
        /// <param name="APPHasMore">显示更多按钮</param>
        /// <returns></returns>
        [HttpPost]
        public string EditSort(string homeids, string bottomids, string movelistids, int AgentId, int AppBtnNumber, byte APPHasMore)
        {
            int[] homeIds = { 0 }; int[] bottomIds = { 0 }; int[] movelistIds = { 0 }; int[] ids = { 0 };
            SysAgent SysAgent = null;
            if (!homeids.IsNullOrEmpty())
            {
                homeIds = Array.ConvertAll<string, int>(homeids.Split(','), delegate(string s) { return int.Parse(s); });
                ids = ids.Union(homeIds).ToArray();
            }
            if (!bottomids.IsNullOrEmpty())
            {
                bottomIds = Array.ConvertAll<string, int>(bottomids.Split(','), delegate(string s) { return int.Parse(s); });
                ids = ids.Union(bottomIds).ToArray();
            }
            if (!movelistids.IsNullOrEmpty())
            {
                movelistIds = Array.ConvertAll<string, int>(movelistids.Split(','), delegate(string s) { return int.Parse(s); });
                ids = ids.Union(movelistIds).ToArray();
            }
            //更新配置
            SysAgent = Entity.SysAgent.FirstOrDefault(o => o.Id == AgentId);
            if (SysAgent != null)
            {
                SysAgent.APPHasMore = APPHasMore;
                SysAgent.AppBtnNumber = AppBtnNumber;
            }
            else
            {
                return "";//没有找到对应的代理商
            }

            this.UpdateVersionAll(AgentId);

            //保存排序
            var entitys = Entity.APPModule.Where(o => ids.Contains(o.Id) && o.AgentId == AgentId).ToList();
            if (entitys != null && entitys.Count > 0)
            {
                int homeSort = 1;
                foreach (var item in homeIds)
                {
                    var temp = entitys.FirstOrDefault(o => o.Id == item);
                    if (temp != null)
                    {
                        temp.Sort = homeSort;
                        homeSort++;
                    }
                }
                int bottomSort = 1;
                foreach (var item in bottomIds)
                {
                    var temp = entitys.FirstOrDefault(o => o.Id == item);
                    if (temp != null)
                    {
                        temp.Sort = bottomSort;
                        bottomSort++;
                    }
                }
                int moveSort = 1;
                foreach (var item in movelistIds)
                {
                    var temp = entitys.FirstOrDefault(o => o.Id == item);
                    if (temp != null)
                    {
                        temp.Sort = moveSort;
                        moveSort++;
                    }
                }
            }
            Entity.SaveChanges();
            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
            {
                ClearCacheSysConfig();
                ClearCacheAdInfo();
            });
            return "1";
        }

        /// <summary>
        /// 更新所有版本号
        /// </summary>
        /// <param name="AgentId"></param>
        /// <returns></returns>
        private bool UpdateVersionAll(int AgentId)
        {
            SysAgent SysAgent = Entity.SysAgent.FirstOrDefault(o => o.Id == AgentId);
            if (SysAgent != null)
            {
                //更新版本
                SysAgent.AppMenuHome++;
                SysAgent.AppMenuBottom++;
                SysAgent.AppMenuMore++;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ClearCache(string name, string host)
        {
            //string apk = "http://api.12fen.com.cn";
            string LastTime = this.AdminUser.LastTime.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss");
            string Token = LastTime.GetMD5();
            Token = string.Format("{0}|{1}", this.AdminUser.Id, Token);
            string url = host + "/home/clearcache?name={0}&key={1}";
            string getURL = string.Format(url, name, Token);
            Utils.HttpsGet(getURL);
        }

        private void ClearCacheSysConfig()
        {
            string apk = string.Empty;
            string api = string.Empty;
            if (this.BasicAgent.IsTeiPai == 1)
            {
                apk = "http://apk.kkapay.com";
                api = "http://api.kkapay.com";
            }
            else
            {
                apk = "http://apk.goodpay.net.cn";
                api = "http://api.goodpay.net.cn";
            }
            this.ClearCache("SysConfig", apk);
            this.ClearCache("SysConfig", api);
        }

        private void ClearCacheAdInfo()
        {
            string apk = string.Empty;
            string api = string.Empty;
            if (this.BasicAgent.IsTeiPai == 1)
            {
                apk = "http://apk.kkapay.com";
                api = "http://api.kkapay.com";
            }
            else
            {
                apk = "http://apk.goodpay.net.cn";
                api = "http://api.goodpay.net.cn";
            }
            this.ClearCache("AdInfo", apk);
            this.ClearCache("AdInfo", api);
        }
    }

}
