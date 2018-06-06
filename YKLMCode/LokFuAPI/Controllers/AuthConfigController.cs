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
    public class AuthConfigController : InitController
    {
        public AuthConfigController()
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
            SysSet SysSet = Entity.SysSet.FirstOrNew();
            string[] BankArray = new string[] { "工商银行", "建设银行", "农业银行", "中国银行", "邮政储蓄银行", "中信银行", "光大银行", "华夏银行", "民生银行", "上海银行", "北京银行", "东亚银行", "兴业银行", "宁波银行", "浦东发展银行", "广发银行", "平安银行", "长沙银行", "成都农村商业银行", "重庆农村商业银行", "重庆银行", "大连银行", "东营市商业银行", "福建农村信用社", "贵阳银行", "广州银行", "广州农村商业银行", "哈尔滨银行", "湖南省农村信用社", "徽商银行", "河北银行", "杭州银行", "常熟农商银行", "江苏银行", "江阴农商银行", "九江银行", "兰州银行", "龙江银行", "南昌银行", "南京银行", "青海银行", "上海农商银行", "上饶银行", "顺德农商银行", "台州银行", "温州银行", "乌鲁木齐商业银行", "无锡农村商业银行", "吴江农村商业银行", "浙江稠州商业银行", "浙江泰隆商业银行", "浙江民泰商业银行", "锦州银行" };
            int i = 1;
            IList<BasicBank> List = new List<BasicBank>();
            bool Show = false;
            if (Equipment.RqType == "Apple")
            {
                if (SysSet.IosSet10 == 6) {
                    Show = true;
                }
            }
            if (Equipment.RqType == "Android")
            {
                if (SysSet.ApkSet10 == 6)
                {
                    Show = true;
                }
            }
            if (Show)
            {
                foreach (var p in BankArray)
                {
                    BasicBank BB = new BasicBank();
                    BB.Id = i;
                    BB.Name = p;
                    BB.Cols = "Id,Name";
                    List.Add(BB);
                    i++;
                }
            }
            DataObj.Data = List.EntityToJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
