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
    public class APPBlockController : BaseController
    {
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="APPBlock"></param>
        /// <param name="p"></param>
        /// <param name="IsFirst"></param>
        /// <returns></returns>
        public ActionResult Index(APPBlock APPBlock, EFPagingInfo<APPBlock> p, int IsFirst = 0)
        {
            //条件
            p.SqlWhere.Add(f => f.AgentId == APPBlock.AgentId);
            if (!APPBlock.Name.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.Name == APPBlock.Name); }
            //排序
            p.OrderByList.Add("Sort", "ASC");
            IPageOfItems<APPBlock> APPBlockList = null;
            if (IsFirst == 0)
            {
                APPBlockList = new PageOfItems<APPBlock>(new List<APPBlock>(), 0, 10, 0, new Hashtable());
            }
            else
            {
                APPBlockList = Entity.Selects<APPBlock>(p);
            }

            ViewBag.APPBlockList = APPBlockList;
            ViewBag.APPBlock = APPBlock;
            //贴牌代理
            var SysAgentList = Entity.SysAgent.Where(o => o.IsTeiPai == 1 && o.State == 1 && o.Tier == 1).ToList();
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
            ViewBag.Add = this.checkPower("Add");
            ViewBag.Edit = this.checkPower("Edit");
            ViewBag.Save = this.checkPower("Save");
            ViewBag.Delete = this.checkPower("Delete");
            return View();
        }
        public ActionResult Edit(APPBlock APPBlock)
        {
            if (APPBlock.Id != 0) APPBlock = Entity.APPBlock.FirstOrDefault(n => n.Id == APPBlock.Id);
            if (APPBlock == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }
            ViewBag.APPBlock = APPBlock;

            //贴牌代理
            var SysAgentList = Entity.SysAgent.Where(o => o.IsTeiPai == 1 && o.State == 1 && o.Tier == 1).ToList();
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
            //加载类型选项
            string filename = HttpContext.Server.MapPath("/ModuleTypeSelectList.json");
            string jsonstr = System.IO.File.ReadAllText(filename);
            var ModuleTypeSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(jsonstr);
            ViewBag.ModuleTypeSelectList = ModuleTypeSelectList;
            string Bottomfilename = HttpContext.Server.MapPath("/ModuleTypeBottomSelectList.json");
            string Bottomjsonstr = System.IO.File.ReadAllText(Bottomfilename);
            var ModuleTypeBottomSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(Bottomjsonstr);
            ViewBag.ModuleTypeBottomSelectList = ModuleTypeBottomSelectList;

            if (Request.UrlReferrer != null)
            {
                Session["Url"] = Request.UrlReferrer.ToString();
            }
            return View();
        }
        public ActionResult Info(int id)
        {
            var baseAPPBlock = Entity.APPBlock.FirstOrDefault(n => n.Id == id);
            if (baseAPPBlock == null)
            {
                ViewBag.ErrorMsg = "数据不存在";
                return View("Error");
            }

            ViewBag.APPBlock = baseAPPBlock;
            if (baseAPPBlock.AgentId == 0)
            {
                var haofusysagent = new SysAgent()
                {
                    Id = 0,
                    Name = "好付",
                    AppBtnNumber = BasicSet.AppBtnNumber,
                    APPHasMore = BasicSet.APPHasMore,
                    APPName = BasicSet.Name,
                };
                ViewBag.SysAgent = haofusysagent;
            }
            else
            {
                ViewBag.SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == baseAPPBlock.AgentId);
            }
            //加载类型选项
            string filename = HttpContext.Server.MapPath("/ModuleTypeSelectList.json");
            string jsonstr = System.IO.File.ReadAllText(filename);
            var ModuleTypeSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(jsonstr);
            ViewBag.ModuleTypeSelectList = ModuleTypeSelectList;
            string Bottomfilename = HttpContext.Server.MapPath("/ModuleTypeBottomSelectList.json");
            string Bottomjsonstr = System.IO.File.ReadAllText(Bottomfilename);
            var ModuleTypeBottomSelectList = JsonConvert.DeserializeObject<SortedList<string, string>>(Bottomjsonstr);
            ViewBag.ModuleTypeBottomSelectList = ModuleTypeBottomSelectList;

            return View();
        }

        [ValidateInput(false)]
        public ActionResult Add(APPBlock APPBlock)
        {
            APPBlock = Request.ConvertRequestToModel<APPBlock>(APPBlock, APPBlock);
            APPBlock.AddTime = DateTime.Now;
            var file = HttpContext.Request.Files.Get("PicUrl");
            if (file != null && file.FileName != string.Empty)
            {
                var image = System.Drawing.Image.FromStream(file.InputStream);
                APPBlock.Height = image.Height;
                APPBlock.Width = image.Width;
            }
            Entity.APPBlock.AddObject(APPBlock);
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功";
            return View("Succeed");
        }

        [ValidateInput(false)]
        public ActionResult Save(APPBlock APPBlock)
        {
            APPBlock baseAPPBlock = Entity.APPBlock.FirstOrDefault(n => n.Id == APPBlock.Id);
            baseAPPBlock = Request.ConvertRequestToModel<APPBlock>(baseAPPBlock, APPBlock);
            var file = HttpContext.Request.Files.Get("PicUrl");
            if (file != null && file.FileName != string.Empty)
            {
                var image = System.Drawing.Image.FromStream(file.InputStream);
                baseAPPBlock.Height = image.Height;
                baseAPPBlock.Width = image.Width;
            }
            Entity.SaveChanges();
            ViewBag.Msg = "操作成功";
            return View("Succeed");
        }

        public void ChangeStatus(APPBlock APPBlock, string InfoList, string Clomn, string Value)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = APPBlock.Id.ToString(); }
            int Ret = Entity.ChangeEntity<APPBlock>(InfoList, Clomn, Value);
            Entity.SaveChanges();
            Response.Write(Ret);
        }
        public void Delete(APPBlock APPBlock, string InfoList, int? IsDel)
        {
            if (string.IsNullOrEmpty(InfoList)) { InfoList = APPBlock.Id.ToString(); }
            int Ret = Entity.MoveToDeleteEntity<APPBlock>(InfoList, IsDel, AdminUser.UserName);
            Entity.SaveChanges();
            Response.Write(Ret);
        }

    }
}
