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
using System.Configuration;

namespace LokFu.Controllers
{
    public class PayConfigNFCController : InitController
    {
        public PayConfigNFCController()
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
            string Tag = "NFC";//HFNFC
            if (ConfigurationManager.AppSettings["NFCPayWay"] != null)
            {
                Tag = ConfigurationManager.AppSettings["NFCPayWay"].ToString();
            }
            PayConfig PayConfig = Entity.PayConfig.FirstOrNew(n => n.DllName == Tag && n.State == 1);
            DataObj.Data = PayConfig.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
