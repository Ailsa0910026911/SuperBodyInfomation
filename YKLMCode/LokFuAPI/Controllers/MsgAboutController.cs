using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections;
using LokFu;
using LokFu.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using LokFu.Extensions;

namespace LokFu.Controllers
{
    public class MsgAboutController : InitController
    {
        public MsgAboutController()
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
            IList<MsgAbout> MsgAboutList = Entity.MsgAbout.Where(n => n.State == 1).ToList();
            DataObj.Data = MsgAboutList.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
