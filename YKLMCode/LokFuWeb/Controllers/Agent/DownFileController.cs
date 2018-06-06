using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web.Mvc;
namespace LokFu.Areas.Agent.Controllers
{
    public class DownFileController : BaseController
    {
        public ActionResult Index()
        {
            var DownFileTagList = Entity.DownFileTag.Where(o => o.State == 1).OrderBy(o=>o.Sort).ToList();
            ViewBag.DownFileTagList = DownFileTagList;
            return View();
        }
        public ActionResult Info(DownFile DownFile,EFPagingInfo<DownFile> p)
        { 
            if (!DownFile.TId.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TId == DownFile.TId); }
            p.OrderByList.Add("Sort", "ESC");
            IPageOfItems<DownFile> DownFileList = Entity.Selects<DownFile>(p);
            ViewBag.DownFileList = DownFileList;
            return View();
        }
    }
}
