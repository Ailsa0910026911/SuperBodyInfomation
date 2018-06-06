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
    public class APPModule2Controller : BaseController
    {
        public ActionResult Index(SysAgent SysAgent, EFPagingInfo<APPModule> p, int SourceAgentId = 0)
        {
            var APPModuleList = Entity.APPModule.Where(o => o.AgentId == SysAgent.Id && o.Version == 1 && o.State==1).OrderBy(o => o.Sort).ToList();
            ViewBag.APPModuleList = APPModuleList;
            var APPBlockList = Entity.APPBlock.Where(o => o.AgentId == SysAgent.Id && o.State == 1).OrderBy(o => o.Sort).ToList();
            ViewBag.APPBlockList = APPBlockList;
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
            AdTag bannerTag = Entity.AdTag.FirstOrDefault(o => o.Tag == "newBanner" && o.State == 1);
            ViewBag.BannerTag = bannerTag;
            if ( bannerTag == null)
            {
                ViewBag.ErrorMsg = "无法找到广告标识";
                return View("Error");
            }
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
            var homeDir = new System.IO.DirectoryInfo(APPModelPath + "home2");
            var bottomDefDir = new System.IO.DirectoryInfo(APPModelPath + "bottom2\\default");
            var bottomActDir = new System.IO.DirectoryInfo(APPModelPath + "bottom2\\activate");

            var homeFiles = homeDir.GetFiles();
            var bottomDefFiles = bottomDefDir.GetFiles();
            var bottomActFiles = bottomActDir.GetFiles();

            ViewBag.homeFiles = homeFiles;
            ViewBag.bottomDefaultFiles = bottomDefFiles;
            ViewBag.bottomActFiles = bottomActFiles;

            //公告
            var Notice = this.Entity.MsgNotice.Where(o => (o.AgentId == 0 || o.AgentId == SysAgent.Id) && (o.NType == 0 || o.NType == 3) && o.State == 1).OrderByDescending(o => o.Id)
                    .Select(o => new { info = o.Info }).FirstOrDefault();
            var ninfo = string.Empty;

            if (Notice != null)
            {
                ninfo += Notice.info.IsNullOrEmpty() ? Notice.info : Utils.RemoveHtml(Notice.info) + "          ";
            }
            ViewBag.ninfo = ninfo;

            this.ViewBag.IsSave = this.checkSignPower("Save");
            this.ViewBag.IsDelete = this.checkSignPower("Delete");
            this.ViewBag.IsImportData = this.checkPower("ImportData");
            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult SaveAPPModule(APPModule APPModule)
        {
            APPModule baseAPPModule = null;
            JsonData JsonData = new JsonData();
            if (APPModule.Id.IsNullOrEmpty())
            {
                baseAPPModule  = APPModule;
                APPModule.AddTime = DateTime.Now;
                APPModule.State = 1;
                APPModule.Version = 1;
                Entity.APPModule.AddObject(APPModule);
            }
            else
            {
                baseAPPModule = Entity.APPModule.FirstOrDefault(n => n.Id == APPModule.Id);
                if (baseAPPModule == null)
                {
                    JsonData.IsSucceed = false;
                    JsonData.Message = "数据不存在";
                }
                else
                {
                    baseAPPModule = Request.ConvertRequestToModel<APPModule>(baseAPPModule, APPModule);
                }  
            }
            if (JsonData.IsSucceed == true)
            {
                //this.UpdateVersionAll(baseAPPModule.AgentId);
                Entity.SaveChanges();
                var Data = this.Entity.APPModule.Where(o => o.AgentId == baseAPPModule.AgentId && o.DisplaySite == baseAPPModule.DisplaySite && o.State == 1 && o.Version == 1)
                    .OrderBy(o=>o.Sort).ToList();
                JsonData.Result = Data;
            }
            return new JsonResult() { Data = JsonData };
        }

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult SaveAdinfo(AdInfo AdInfo)
        {
            var result = new JsonData();
            AdInfo baseAdInfo = null;
            if (AdInfo.Id.IsNullOrEmpty())
            {
                baseAdInfo = AdInfo;
                baseAdInfo.StartTime = DateTime.Now;
                baseAdInfo.EndTime = new DateTime(5000, 1, 1);
                baseAdInfo.State = 1;
                this.Entity.AdInfo.AddObject(AdInfo);
            }
            else
            {
                baseAdInfo = this.Entity.AdInfo.FirstOrDefault(o => o.Id == AdInfo.Id);
                if (baseAdInfo == null)
                {
                    result.IsSucceed = false;
                    result.Message = "数据不存在";
                }
                baseAdInfo = Request.ConvertRequestToModel<AdInfo>(baseAdInfo, AdInfo);
            }
            if(result.IsSucceed)
            {
                this.Entity.SaveChanges();
                var list = this.Entity.AdInfo.Where(o=>o.AgentId == baseAdInfo.AgentId && o.TId == baseAdInfo.TId && o.State==1)
                    .OrderBy(o=>o.Sort).ToList();
                result.Result = list;
            }

            return new JsonResult() { Data = result };
        }

        [ValidateInput(false)]
        [HttpPost]
        public JsonResult SaveAPPBlock(APPBlock APPBlock)
        {
            var result = new JsonData();
            APPBlock baseAPPBlock = null;
            if (APPBlock.Id.IsNullOrEmpty())
            {
                baseAPPBlock = APPBlock;
                baseAPPBlock.AddTime = DateTime.Now;
                baseAPPBlock.State = 1;
                this.Entity.APPBlock.AddObject(baseAPPBlock);
            }
            else
            {
                baseAPPBlock = this.Entity.APPBlock.FirstOrDefault(o => o.Id == APPBlock.Id);
                if (baseAPPBlock == null)
                {
                    result.IsSucceed = false;
                    result.Message = "数据不存在";
                }
                baseAPPBlock = Request.ConvertRequestToModel<APPBlock>(baseAPPBlock, APPBlock);
            }
            if (result.IsSucceed)
            {
                var MapPath = this.Server.MapPath("/UpLoadFiles/APPBlock/" + baseAPPBlock.PicUrl);
                var image = System.Drawing.Image.FromFile(MapPath);
                baseAPPBlock.Height = image.Height;
                baseAPPBlock.Width = image.Width;
                this.Entity.SaveChanges();
                var list = this.Entity.APPBlock.Where(o => o.AgentId == baseAPPBlock.AgentId && o.State == 1)
                    .OrderBy(o => o.Sort).ToList();
                result.Result = list;
            }

            return new JsonResult() { Data = result };
        }

        [HttpPost]
        public JsonResult DeleteAdinfo(AdInfo AdInfo)
        {
            var result = new JsonData();
            if (AdInfo.Id.IsNullOrEmpty())
            {
                result.IsSucceed = false;
                result.Message = "参数错误";
            }
            var baseAdInfo = this.Entity.AdInfo.FirstOrDefault(o => o.Id == AdInfo.Id);
            if (baseAdInfo == null)
            {
                result.IsSucceed = false;
                result.Message = "数据不存在";
            }
            this.Entity.AdInfo.DeleteObject(baseAdInfo);
            this.Entity.SaveChanges();
            return new JsonResult() { Data = result };
        }

        [HttpPost]
        public JsonResult DeleteAPPModule(APPModule APPModule)
        {
            var result = new JsonData();
            if (APPModule.Id.IsNullOrEmpty())
            {
                result.IsSucceed = false;
                result.Message = "参数错误";
            }
            if (APPModule.Id==17)
            {
                result.IsSucceed = false;
                result.Message = "该模块不能删除";
            }
            var baseAPPModule = this.Entity.APPModule.FirstOrDefault(o => o.Id == APPModule.Id);
            if (baseAPPModule == null)
            {
                result.IsSucceed = false;
                result.Message = "数据不存在";
            }
            this.Entity.APPModule.DeleteObject(baseAPPModule);
            this.Entity.SaveChanges();
            return new JsonResult() { Data = result };

        }

        public JsonResult DeleteAPPBlock(APPBlock APPBlock)
        { 
            var result = new JsonData();
            if (APPBlock.Id.IsNullOrEmpty())
            {
                result.IsSucceed = false;
                result.Message = "参数错误";
            }
            var baseAPPBlock = this.Entity.APPBlock.FirstOrDefault(o => o.Id == APPBlock.Id);
            if (baseAPPBlock == null)
            {
                result.IsSucceed = false;
                result.Message = "数据不存在";
            }
            this.Entity.APPBlock.DeleteObject(baseAPPBlock);
            this.Entity.SaveChanges();
            return new JsonResult() { Data = result }; 
        }

        /// <summary>
        /// 保存排序及显示数量
        /// </summary>
        /// <param name="homeids">中间</param>
        /// <param name="bottomids">底部</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveSort(string homeids, string bottomids, int AgentId)
        {
            var result = new JsonData();
           
            int[] homeIds = { 0 }; int[] bottomIds = { 0 }; int[] ids = { 0 };
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

            //保存排序
            var entitys = Entity.APPModule.Where(o => o.AgentId == AgentId).ToList();
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
                Entity.SaveChanges();
            }
            var home = this.Entity.APPModule.Where(o => o.DisplaySite == 1 && o.AgentId == AgentId && o.State == 1 && o.Version == 1).OrderBy(o => o.Sort).ToList();
            var bottom = this.Entity.APPModule.Where(o => o.DisplaySite == 2 && o.AgentId == AgentId && o.State == 1 && o.Version == 1).OrderBy(o => o.Sort).ToList();
            result.Result = new { home = home, bottom = bottom };
            
            return new JsonResult() { Data = result };
        }

        /// <summary>
        /// 数据导入
        /// </summary>
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult ImportData(string guanggao, string home, string bottom, string block, int TargetAgentId, int SourceAgentId)
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
                this.ReplaceAdInfo(SourceAgentId, TargetAgentId, "newBanner");
            }

            if (!home.IsNullOrEmpty())
            {
                this.ReplaceAPPModule(SourceAgentId, TargetAgentId, 1);
            }

            if (!bottom.IsNullOrEmpty())
            {
                this.ReplaceAPPModule(SourceAgentId, TargetAgentId, 2);
            }

            if (!block.IsNullOrEmpty())
            {
                this.ReplaceAPPBlock(SourceAgentId, TargetAgentId);
            }
            #endregion

            this.Entity.SaveChanges();
            return this.RedirectToRoute("ManageAction", new { action = "Index", Controller = "APPModule2", Id = TargetAgentId, SourceAgentId = SourceAgentId });
        }

        /// <summary>
        /// 替换APPModule
        /// </summary>
        private void ReplaceAPPModule(int SourceAgentId, int TargetAgentId, int DisplaySite)
        {
            var TargetList = this.Entity.APPModule.Where(o => o.DisplaySite == DisplaySite && o.AgentId == TargetAgentId && o.Version == 1).ToList();
            foreach (var item in TargetList)
            {
                this.Entity.APPModule.DeleteObject(item);
            }

            var SourceList = this.Entity.APPModule.Where(o => o.DisplaySite == DisplaySite && o.AgentId == SourceAgentId && o.Version==1).ToList();
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

        private void ReplaceAPPBlock(int SourceAgentId, int TargetAgentId)
        {
            var TargetList = this.Entity.APPBlock.Where(o => o.State == 1 && o.AgentId == TargetAgentId).ToList();
            foreach (var item in TargetList)
            {
                this.Entity.APPBlock.DeleteObject(item);
            }

            var SourceList = this.Entity.APPBlock.Where(o => o.State == 1 && o.AgentId == SourceAgentId).ToList();
            if (SourceList.Count > 0)
            {
                SourceList.ForEach(o =>
                {
                    //跟踪管理器中修改成添加,实体的id会重新分配，不必手动修改。
                    this.Entity.ObjectStateManager.ChangeObjectState(o, System.Data.EntityState.Added);
                    o.AgentId = TargetAgentId;
                    this.Entity.APPBlock.AddObject(o);
                });
            }
        }

    }

    public class JsonData
    {
        public JsonData()
        {
            this.IsSucceed = true;
            this.Message = "";
        }
        public bool IsSucceed { get; set; }
        public string Message { get; set; }
        public dynamic Result { get; set; }
    }
}
