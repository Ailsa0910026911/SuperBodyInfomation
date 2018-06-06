using LokFu.Extensions;
using LokFu.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace LokFu.Controllers
{
    public class DaiLiInfoController : InitController
    {
        public DaiLiInfoController()
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

            SysMoneySet SysMoneySet = Entity.SysMoneySet.FirstOrNew();
            string Path = HttpContext.Current.Server.MapPath("/UpLoadFiles/AgentPrice/Agent.png");
            DaiLi DaiLi=new DaiLi();
            DaiLi.imageurl = SysImgPath + "/UpLoadFiles/AgentPrice/Agent.png";
            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(Path);
            DaiLi.height = originalImage.Height;
            DaiLi.width = originalImage.Width;
            List<AgentsInfo> list = new List<AgentsInfo>();
            AgentsInfo info = new AgentsInfo();
            info.tier = 5;
            info.name = "渠道商";
            info.agentprice = SysMoneySet.AgentPrice5;
            list.Add(info);
            info = new AgentsInfo();
            info.tier = 6;
            info.name = "分销商";
            info.agentprice = SysMoneySet.AgentPrice6;
            list.Add(info);
            DaiLi.agentspricelist = list;
            //SysMoneySet.AgentPricesList = "[{\"tier\":\"5\",\"agentprice\":\"" + SysMoneySet.AgentPrice5 + "\"},{\"tier\":\"6\",\"agentprice\":\"" + SysMoneySet.AgentPrice6 + "\"}]";
            //SysMoneySet.AgentPricesList = JsonConvert.SerializeObject(list);
            //SysMoneySet.Cols = "ImageUrl,AgentPricesList,Width,Height";
            DataObj.Data = JsonConvert.SerializeObject(DaiLi);
            DataObj.Code = "0000";
            DataObj.OutString();
        }
        public class DaiLi
        { 
         public string imageurl { get; set; }
         public List<AgentsInfo> agentspricelist;
         public int width { get; set; }
         public int height { get; set; }
        }
        public class AgentsInfo
        {
            public int tier { get; set; }
            public string name { get; set; }
            public decimal agentprice { get; set; }
        }
    }
   
}

