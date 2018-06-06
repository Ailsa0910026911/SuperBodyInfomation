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
    public class SysConfig_4_0Controller : InitController
    {
        public SysConfig_4_0Controller()
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
        public class ConnectLog
        {
            public string No { get; set; }
            public DateTime UpdateTime { get; set; }
        }
        public void Post()
        {
            SysSet SysSet = new SysSet();
            string Data = DataObj.GetData();
            if (!Data.IsNullOrEmpty())
            {
                JObject json = new JObject();
                try
                {
                    json = (JObject)JsonConvert.DeserializeObject(Data);
                }
                catch (Exception Ex)
                {
                    Log.Write("[SysConfig_4_0]:", "【Data】" + Data, Ex);
                }
                if (json == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                //处理贴牌相关
                SysAgent SysAgent = new SysAgent();
                SysAgent = JsonToObject.ConvertJsonToModel(SysAgent, json);
                if (!SysAgent.Id.IsNullOrEmpty()){
                    SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == SysAgent.Id && n.State == 1 && n.IsTeiPai == 1);
                    if (SysAgent == null)
                    {
                        DataObj.OutError("1000");
                        return;
                    }
                }
                SysSet = Entity.SysSet.FirstOrDefault();

                //处理返回支付通道配置
                IList<SysControl> SysControlList = Entity.SysControl.OrderBy(n => n.Sort).ToList();//SysControl
                foreach (var p in SysControlList)
                {
                    p.Cols = "Tag,CName,State,SNum,ENum,PayWay";
                    p.ChkState();
                }
                SysSet.CtrlSet = SysControlList.EntityToJson();

                if (!SysAgent.Id.IsNullOrEmpty())
                {
                    if (!SysAgent.IsTeiPai.IsNullOrEmpty())
                    {
                        SysSet.Name = SysAgent.APPName.IsNullOrEmpty() ? SysAgent.Name : SysAgent.APPName;

                        SysSet.IosVer = SysAgent.IosVer;
                        SysSet.IosInt = SysAgent.IosInt;
                        SysSet.IosUrl = SysAgent.IosUrl;
                        SysSet.IosInfo = SysAgent.IosInfo;
                        SysSet.IosColor = SysAgent.IosColor.IsNullOrEmpty() ? SysSet.IosColor : SysAgent.IosColor;

                        SysSet.ApkVer = SysAgent.ApkVer;
                        SysSet.ApkInt = SysAgent.ApkInt;
                        SysSet.ApkUrl = SysAgent.ApkUrl;
                        SysSet.ApkInfo = SysAgent.ApkInfo;
                        SysSet.ApkColor = SysAgent.ApkColor.IsNullOrEmpty() ? SysSet.ApkColor : SysAgent.ApkColor;

                        SysSet.Tel = SysAgent.Tel;

                        SysSet.AppMenuBottom = SysAgent.AppMenuBottom;

                        SysSet.ApkSet3 = SysAgent.Set3;
                        SysSet.IosSet3 = SysAgent.Set3;

                        SysSet.ApkSet4 = SysAgent.Set4;
                        SysSet.IosSet4 = SysAgent.Set4;

                        SysSet.GetOrderWay = 1;

                        if (SysAgent.IsShowWord.IsNullOrEmpty())
                        {
                            SysSet.AppWord = "";
                        }
                    }
                    else
                    {
                        DataObj.OutError("3005");
                        return;
                    }
                }
                //处理快速到帐相关
                if (SysSet.SW1Key.IsNullOrEmpty())
                {
                    SysSet.SW1Key = "2小时到帐";
                }
                if (SysSet.SW2Key.IsNullOrEmpty())
                {
                    SysSet.SW2Key = "次日到帐";
                }
                DateTime Now = DateTime.Now;
                if (!SysSet.SW1sTime.IsNullOrEmpty() && !SysSet.SW1eTime.IsNullOrEmpty())
                {
                    if (SysSet.SW1sTime <= Now && SysSet.SW1eTime > Now)
                    {
                        if (!SysSet.SW1KeyT.IsNullOrEmpty())
                        {
                            SysSet.SW1Key = SysSet.SW1KeyT;
                        }
                    }
                }
                if (!SysSet.SW2sTime.IsNullOrEmpty() && !SysSet.SW2eTime.IsNullOrEmpty())
                {
                    if (SysSet.SW2sTime <= Now && SysSet.SW2eTime > Now)
                    {
                        if (!SysSet.SW2KeyT.IsNullOrEmpty())
                        {
                            SysSet.SW2Key = SysSet.SW2KeyT;
                        }
                    }
                }
                //定义返回数据
                if (Equipment.RqType == "Apple")
                {
                    SysSet.Cols = "AuthTimes,OpenAutoAuthIOS,AutoAuthAppKeyIOS,AutoAuthAppSecretIOS,AutoAuthAppCodeIOS,Name,IosVer,IosInt,IosUrl,IosInfo,IOSState,IosSet1,IosSet2,IosSet3,IosSet4,IosSet5,IosSet6,IosSet7,IosSet8,IosSet9,IosSet10,IosSet11,IosAutoUpdate,Tel,AppWord,SW1Key,SW2Key,IosColor,LagEntry,LagEntryNum,LagEntryDay,AutoCashMoney,AuthType,AuthPrice,Button,AppMenuBottom,GetOrderWay,CtrlSet";
                }
                if (Equipment.RqType == "Android")
                {
                    SysSet.Cols = "AuthTimes,OpenAutoAuthAndroid,AutoAuthAppKeyAndroid,AutoAuthAppSecretAndroid,AutoAuthAppCodeAndroid,Name,ApkVer,ApkInt,ApkUrl,ApkInfo,APKState,ApkSet1,ApkSet2,ApkSet3,ApkSet4,ApkSet5,ApkSet6,ApkSet7,ApkSet8,ApkSet9,ApkSet10,ApkSet11,ApkAutoUpdate,Tel,AppWord,SW1Key,SW2Key,ApkColor,LagEntry,LagEntryNum,LagEntryDay,AutoCashMoney,AuthType,AuthPrice,Button,AppMenuBottom,GetOrderWay,CtrlSet";
                }
                //T+n相关
                if (SysSet.LagEntry == 0)
                {
                    SysSet.LagEntryDay = 0;
                    SysSet.LagEntryNum = 0;
                }
                var Button = Entity.APPModule.Where(n => n.State == 1 && n.DisplaySite == 2 && n.Version == 1 && n.AgentId == SysAgent.Id)
                    .Select(o => new ButtonModel
                    {
                        picurl = o.PicUrl ?? string.Empty,
                        pictureurl = o.PictureUrl ?? string.Empty,
                        value = o.Value ?? string.Empty,
                        moduletype = o.ModuleType,
                        sort = o.Sort,
                        name = o.Name ?? string.Empty,
                        height = o.Height,
                        width = o.Width,
                    })
                    .OrderBy(o => o.sort).ToList();

                if (Button == null || Button.Count == 0)//贴牌没有配置功能按钮，将默认使用好付
                {
                    Button = Entity.APPModule.Where(n => n.State == 1 && n.DisplaySite == 2 && n.Version == 1 && n.AgentId == 0)
                    .Select(o => new ButtonModel
                    {
                        picurl = o.PicUrl ?? string.Empty,
                        pictureurl = o.PictureUrl ?? string.Empty,
                        value = o.Value ?? string.Empty,
                        moduletype = o.ModuleType,
                        sort = o.Sort,
                        name = o.Name ?? string.Empty,
                        height = o.Height,
                        width = o.Width,
                    })
                    .OrderBy(o => o.sort).ToList();
                }
                //添加Url域名
                Button.ForEach(o =>
                {
                    o.pictureurl = Utils.ImageUrl("APPModule", o.pictureurl, SysImgPath);
                    if (!o.picurl.IsNullOrEmpty())
                    {
                        o.picurl = Utils.ImageUrl("APPModule", o.picurl, SysImgPath);
                    }
                });

                var JSS = new System.Web.Script.Serialization.JavaScriptSerializer();
                string bjson = JSS.Serialize(Button.ToArray());
                JArray JO = (JArray)JsonConvert.DeserializeObject(bjson);
                SysSet.Button = JO;

                //贴牌特别处理
                if (SysAgent.IsTeiPai == 1)
                {
                    SysSet.LagEntry = 0;
                    SysSet.LagEntryDay = 0;
                    SysSet.LagEntryNum = 0;
                }

                string data = SysSet.OutJson();
                data = data.Replace("\"[{", "[{").Replace("}]\"", "}]");
                DataObj.Data = data;
                DataObj.Code = "0000";
                DataObj.OutString();
            }
        }
    }

    public class ButtonModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string picurl { get; set; }
        public string pictureurl { get; set; }

        public string value { get; set; }

        public byte moduletype { get; set; }

        public int sort { get; set; }

        public int height { get; set; }

        public int width { get; set; }
    }
}
