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
namespace LokFu.Areas.Manage.Controllers
{
    public class APPModuleController : BaseController
    {
        public ActionResult Index(SysAgent SysAgent, EFPagingInfo<APPModule> p, int SourceAgentId = 0)
        {
            var APPModuleList = Entity.APPModule.Where(o => o.AgentId == SysAgent.Id && o.Version == 0).OrderBy(o => o.Id).ToList();
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
            var scanCodeInfo = Entity.AdInfo.FirstOrNew(o => o.TId == scanCodeTag.Id && o.State == 1 && o.AgentId == SysAgent.Id);
            ViewBag.scanCodeInfo = scanCodeInfo;
            var bannerInfos = Entity.AdInfo.OrderBy(o => o.Sort).Where(o => o.TId == bannerTag.Id && o.State == 1 && o.AgentId == SysAgent.Id).ToList();
            ViewBag.bannerInfos = bannerInfos;
            //贴牌代理
            var SysAgentList = Entity.SysAgent.Where(o => o.IsTeiPai == 1 && o.State == 1).ToList();
            var haofusysagent = new SysAgent()
            {
                Id = 0,
                Name = "好付",
                AppBtnNumber = BasicSet.AppBtnNumber,
                APPHasMore = BasicSet.APPHasMore,
                APPName = BasicSet.Name,
            };
            SysAgentList.Add(haofusysagent);
            SysAgentList = SysAgentList.OrderBy(o => o.Id).ToList();
            ViewBag.SysAgentList = SysAgentList;
            ViewBag.SysAgent = SysAgentList.FirstOrNew(o => o.Id == SysAgent.Id);
            ViewBag.SourceAgentId = SourceAgentId;
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

            this.ViewBag.IsAdd = this.checkPower("Add");
            this.ViewBag.IsSave = this.checkPower("Save");
            this.ViewBag.IsDelete = this.checkPower("Delete");
            this.ViewBag.IsEdit = this.checkPower("Edit");
            return View();
        }

        [ValidateInput(false)]
        public void Add(APPModule APPModule)
        {
            int sort = this.Entity.APPModule.Where(o => o.AgentId == APPModule.AgentId && o.DisplaySite == APPModule.DisplaySite).Max(o => (int?)o.Sort) ?? 0;
            APPModule.AddTime = DateTime.Now;
            APPModule.State = 1;
            APPModule.Sort = sort + 1;
            Entity.APPModule.AddObject(APPModule);
            if (!this.UpdateVersionAll(APPModule.AgentId))
            {
                return;
            }
            Entity.SaveChanges();

            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
            {
                ClearCacheSysConfig();
            });
            BaseRedirect();
        }

        [ValidateInput(false)]
        public void Save(APPModule APPModule)
        {
            APPModule baseAPPModule = Entity.APPModule.FirstOrDefault(n => n.Id == APPModule.Id);
            baseAPPModule = Request.ConvertRequestToModel<APPModule>(baseAPPModule, APPModule);
            if (!this.UpdateVersionAll(baseAPPModule.AgentId))
            {
                return;
            }
            Entity.SaveChanges();
            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
            {
                ClearCacheSysConfig();
            });
            BaseRedirect();
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
            return this.RedirectToRoute("ManageAction", new { action = "Index", Controller = "APPModule", Id = AdInfo.AgentId });
            //BaseRedirect();
        }

        /// <summary>
        /// 删除广告
        /// </summary>
        /// <param name="AdInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public string DeleteAdinfo(AdInfo AdInfo)
        {
            if (AdInfo.Id.IsNullOrEmpty())
            {
                return "0";
            }
            var baseAdInfo = this.Entity.AdInfo.FirstOrDefault(o => o.Id == AdInfo.Id);
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
        /// <param name="AgentId">代理Id,0为系统默认</param>
        /// <param name="DisplaySite">区域 1:home 2:底部 3:更多</param>
        /// <returns></returns>
        private bool UpdateVersion(int AgentId, int DisplaySite)
        {
            if (AgentId == 0)
            {
                switch (DisplaySite)
                {
                    case 1:
                        BasicSet.AppMenuHome++;
                        break;
                    case 2:
                        BasicSet.AppMenuBottom++;
                        break;
                    case 3:
                        BasicSet.AppMenuMore++;
                        break;
                }
                return true;
            }
            else
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
        }

        /// <summary>
        /// 删除图标
        /// </summary>
        /// <param name="APPModule"></param>
        [HttpPost]
        public void DeleteAPPModule(APPModule APPModule)
        {
            if (APPModule.Id.IsNullOrEmpty()) { return; }
            APPModule baseAPPModule = Entity.APPModule.FirstOrDefault(n => n.Id == APPModule.Id);
            if (baseAPPModule != null)
            {
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
            if (AgentId == 0)
            {
                this.BasicSet.APPHasMore = APPHasMore;
                this.BasicSet.AppBtnNumber = AppBtnNumber;
            }
            else
            {
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
            });
            return "1";
        }

        /// <summary>
        /// 数据导入
        /// </summary>
        /// <param name="guanggao"></param>
        /// <param name="saoma"></param>
        /// <param name="home"></param>
        /// <param name="bottom"></param>
        /// <param name="more"></param>
        /// <param name="TargetAgentId"></param>
        /// <param name="SourceAgentId"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditImportData(string guanggao, string saoma, string home, string bottom, string more, int TargetAgentId, int SourceAgentId)
        {
            #region 校验
            if (SourceAgentId == TargetAgentId)
            {
                ViewBag.ErrorMsg = "不能导入自己的数据,请重新选择";
                return View("Error");
            }
            if (TargetAgentId == 0)
            {
                ViewBag.ErrorMsg = "不能导入到默认(好付)的数据,请重新选择";
                return View("Error");
            }
            if (SourceAgentId != 0)
            {
                SysAgent SysAgent = this.Entity.SysAgent.FirstOrDefault(o => o.Id == SourceAgentId);
                if (SysAgent == null)
                {
                    ViewBag.ErrorMsg = "源代理商不存在";
                    return View("Error");
                }
            }
            if (TargetAgentId != 0)
            {
                SysAgent SysAgent = this.Entity.SysAgent.FirstOrDefault(o => o.Id == TargetAgentId);
                if (SysAgent == null)
                {
                    ViewBag.ErrorMsg = "目标代理商不存在";
                    return View("Error");
                }
            }
            #endregion

            #region 替换
            if (!guanggao.IsNullOrEmpty())
            {
                this.ReplaceAdInfo(SourceAgentId, TargetAgentId, "banner");
            }

            if (!saoma.IsNullOrEmpty())
            {
                this.ReplaceAdInfo(SourceAgentId, TargetAgentId, "SaoBg");
            }

            if (!home.IsNullOrEmpty())
            {
                this.ReplaceAPPModule(SourceAgentId, TargetAgentId, 1);
            }

            if (!bottom.IsNullOrEmpty())
            {
                this.ReplaceAPPModule(SourceAgentId, TargetAgentId, 2);
            }

            if (!more.IsNullOrEmpty())
            {
                this.ReplaceAPPModule(SourceAgentId, TargetAgentId, 3);
            }
            #endregion

            this.UpdateVersionAll(TargetAgentId);

            this.Entity.SaveChanges();
            System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
            {
                ClearCacheSysConfig();
                ClearCacheAdInfo();
            });
            return this.RedirectToRoute("ManageAction", new { action = "Index", Controller = "APPModule", Id = TargetAgentId, SourceAgentId = SourceAgentId });
        }

        /// <summary>
        /// 更新所有版本号
        /// </summary>
        /// <param name="AgentId"></param>
        /// <returns></returns>
        private bool UpdateVersionAll(int AgentId)
        {
            if (AgentId == 0)
            {
                //更新版本
                this.BasicSet.AppMenuHome++;
                this.BasicSet.AppMenuBottom++;
                this.BasicSet.AppMenuMore++;
                return true;
            }
            else
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
        }

        /// <summary>
        /// 替换APPModule
        /// </summary>
        /// <param name="SourceAgentId"></param>
        /// <param name="TargetAgentId"></param>
        /// <param name="DisplaySite"></param>
        /// <returns></returns>
        private void ReplaceAPPModule(int SourceAgentId, int TargetAgentId, int DisplaySite)
        {
            var TargetList = this.Entity.APPModule.Where(o => o.DisplaySite == DisplaySite && o.AgentId == TargetAgentId).ToList();
            foreach (var item in TargetList)
            {
                this.Entity.APPModule.DeleteObject(item);
            }

            var SourceList = this.Entity.APPModule.Where(o => o.DisplaySite == DisplaySite && o.AgentId == SourceAgentId).ToList();
            if (SourceList.Count > 0)
            {
                SourceList.ForEach(o =>
                {
                    //跟踪管理器中修改成添加,实体的id会重新分配，不必手动修改。
                    this.Entity.ObjectStateManager.ChangeObjectState(o, System.Data.EntityState.Added);
                    o.AgentId = TargetAgentId;
                    this.Entity.APPModule.AddObject(o);
                });
            }
        }

        /// <summary>
        /// 替换AdInfo
        /// </summary>
        /// <param name="SourceAgentId"></param>
        /// <param name="TargetAgentId"></param>
        /// <param name="Tag"></param>
        private void ReplaceAdInfo(int SourceAgentId, int TargetAgentId, string Tag)
        {
            var TargetList = this.Entity.AdInfo.Where(o => o.Tag == Tag && o.State == 1 && o.AgentId == TargetAgentId).ToList();
            foreach (var item in TargetList)
            {
                this.Entity.AdInfo.DeleteObject(item);
            }

            var SourceList = this.Entity.AdInfo.Where(o => o.Tag == Tag && o.State == 1 && o.AgentId == SourceAgentId).ToList();
            if (SourceList.Count > 0)
            {
                SourceList.ForEach(o =>
                {
                    //跟踪管理器中修改成添加,实体的id会重新分配，不必手动修改。
                    this.Entity.ObjectStateManager.ChangeObjectState(o, System.Data.EntityState.Added);
                    o.AgentId = TargetAgentId;
                    this.Entity.AdInfo.AddObject(o);
                });
            }
        }

        private void ClearCache(string name)
        {
            string apk = "http://apk.goodpay.net.cn/";
            //string apk = "http://api.12fen.com.cn";
            string LastTime = this.AdminUser.LastTime.GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss");
            string Token = LastTime.GetMD5();
            Token = string.Format("{0}|{1}", this.AdminUser.Id, Token);
            string url = apk + "/home/clearcache?name={0}&key={1}";
            string getURL = string.Format(url, name, Token);
            Utils.HttpsGet(getURL);
        }

        private void ClearCacheSysConfig()
        {
            this.ClearCache("SysConfig");
        }

        private void ClearCacheAdInfo()
        {
            this.ClearCache("AdInfo");
        }
    }

}
