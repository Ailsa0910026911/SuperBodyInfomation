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
    public class JobSetController : InitController
    {
        public JobSetController()
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
        }
        public void Post()
        {

            JobSet JobSet = Entity.JobSet.FirstOrNew();
            DataObj.Data = JobSet.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
