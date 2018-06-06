using System;
using System.Collections.Generic;
using System.Linq;
using LokFu.Repositories;
using LokFu.Extensions;
namespace LokFu.Areas.Base.Controllers
{
    public class BaseController : InitController
    {
        public SysAdmin AdminUser;
        public SysSet BasicSet;
        public BaseController() {
            BasicSet = Entity.SysSet.FirstOrNew();
            ViewBag.BasicSet = BasicSet;
        }
    }
}