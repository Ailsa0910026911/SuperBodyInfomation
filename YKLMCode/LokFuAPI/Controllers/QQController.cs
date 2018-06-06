using LokFu.Extensions;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LokFu.Controllers
{
    public class QQController : InitController
    {
        public QQController()
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
            IList<SysAdmin> SysAdminList = Entity.SysAdmin.Where(n => n.State == 1 && n.QQNum.Length > 4).OrderByDescending(n => n.QQState).ThenBy(n => n.QQName).ToList();
            DataObj.Data = SysAdminList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();

        }
    }
}
