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
    public class SysConfigController : InitController
    {
        public SysConfigController()
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
                    Log.Write("[SysConfig]:", "【Data】" + Data, Ex);
                }
                if (json == null)
                {
                    DataObj.OutError("1000");
                    return;
                }

                //处理贴牌相关
                SysAgent SysAgent = new SysAgent();
                SysAgent = JsonToObject.ConvertJsonToModel(SysAgent, json);
                if (SysAgent.Id.IsNullOrEmpty()){
                    SysAgent.Id=0;
                }

                bool IsOld = true;
                if (!Equipment.SoftVer.IsNullOrEmpty())
                {
                    Version v1 = new Version(Equipment.SoftVer);//当前版本
                    Version v2 = new Version("1.0");

                    if (!SysAgent.Id.IsNullOrEmpty())
                    {
                        SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == SysAgent.Id && n.State == 1 && n.IsTeiPai == 1);
                    }
                    if (SysAgent == null)
                    {
                        DataObj.OutError("1000");
                        return;
                    }
                    if (Equipment.RqType.ToLower() == "apple")
                    {
                        //苹果
                        if (SysAgent.IsTeiPai.IsNullOrEmpty())//好付
                        {
                            v2 = new Version("7.5");
                        }
                        else//贴牌
                        {
                            v2 = new Version("5.0");
                        }

                    }
                    else if (Equipment.RqType.ToLower() == "android")
                    {
                        //安卓
                        if (SysAgent.IsTeiPai.IsNullOrEmpty())//好付
                        {
                            v2 = new Version("6.6.6");
                        }
                        else //贴牌
                        {
                            v2 = new Version("4.0");
                        }
                    }
                    if (v1 >= v2)
                    {
                        IsOld = false;
                    }
                }

                string CashName = "SysConfig_" + (IsOld ? "O" : "N") + "_" + Equipment.RqType + SysAgent.Id;

                if (HasCache)
                {
                    string StringJson = CacheBuilder.EntityCache.Get(CashName, null) as string;
                    if (!StringJson.IsNullOrEmpty())
                    {
                        DataObj.Data = StringJson;
                        DataObj.Code = "0000";
                        DataObj.OutString();
                        return;
                    }
                }

                SysSet = Entity.SysSet.FirstOrDefault();
                if (!SysAgent.Id.IsNullOrEmpty())
                {
                    if (!SysAgent.IsTeiPai.IsNullOrEmpty())
                    {
                        //SysSet.Name = SysAgent.Name;

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

                        SysSet.AppBtnNumber = SysAgent.AppBtnNumber.IsNullOrEmpty() ? SysSet.AppBtnNumber : SysAgent.AppBtnNumber;
                        SysSet.APPHasMore = SysAgent.APPHasMore.IsNullOrEmpty() ? SysSet.APPHasMore : SysAgent.APPHasMore;
                        SysSet.AppMenuBottom = SysAgent.AppMenuBottom;
                        SysSet.AppMenuHome = SysAgent.AppMenuHome;
                        SysSet.AppMenuMore = SysAgent.AppMenuMore;
                        SysSet.Name = SysAgent.APPName.IsNullOrEmpty() ? SysAgent.Name : SysAgent.APPName;

                        SysSet.ApkSet3 = SysAgent.Set3;
                        SysSet.IosSet3 = SysAgent.Set3;

                        SysSet.ApkSet4 = SysAgent.Set4;
                        SysSet.IosSet4 = SysAgent.Set4;

                        #region 废弃
                        //特别处理，等开放自由设置后，需要去掉
                        //惠付钱包615 不要分享挣钱
                        //商银汇富796 激活码必填、不要分享挣钱
                        //铭越钱包1348不要降费率
                        //0 关闭 1仅开挣钱 2仅开降费率 3都打开
                        //if (SysAgent.Id == 615)
                        //{
                        //    if (SysSet.ApkSet4 == 3 || SysSet.ApkSet4 == 2)
                        //    {
                        //        SysSet.ApkSet4 = 2;
                        //    }
                        //    else
                        //    {
                        //        SysSet.ApkSet4 = 0;
                        //    }
                        //    if (SysSet.IosSet4 == 3 || SysSet.IosSet4 == 2)
                        //    {
                        //        SysSet.IosSet4 = 2;
                        //    }
                        //    else
                        //    {
                        //        SysSet.IosSet4 = 0;
                        //    }
                        //    //激活码必填
                        //    SysSet.ApkSet3 = 1;
                        //    SysSet.IosSet3 = 1;
                        //}
                        //if (SysAgent.Id == 796)
                        //{
                        //    if (SysSet.ApkSet4 == 3 || SysSet.ApkSet4 == 2)
                        //    {
                        //        SysSet.ApkSet4 = 2;
                        //    }
                        //    else
                        //    {
                        //        SysSet.ApkSet4 = 0;
                        //    }
                        //    if (SysSet.IosSet4 == 3 || SysSet.IosSet4 == 2)
                        //    {
                        //        SysSet.IosSet4 = 2;
                        //    }
                        //    else
                        //    {
                        //        SysSet.IosSet4 = 0;
                        //    }
                        //    //激活码必填
                        //    SysSet.ApkSet3 = 1;
                        //    SysSet.IosSet3 = 1;
                        //}
                        //if (SysAgent.Id == 1348)
                        //{
                        //    if (SysSet.ApkSet4 == 3 || SysSet.ApkSet4 == 1)
                        //    {
                        //        SysSet.ApkSet4 = 1;
                        //    }
                        //    else {
                        //        SysSet.ApkSet4 = 0;
                        //    }
                        //    if (SysSet.IosSet4 == 3 || SysSet.IosSet4 == 1)
                        //    {
                        //        SysSet.IosSet4 = 1;
                        //    }
                        //    else
                        //    {
                        //        SysSet.IosSet4 = 0;
                        //    }
                        //}
                        #endregion

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

                //处理返回支付通道配置
                IList<SysControl> SysControlList = Entity.SysControl.OrderBy(n => n.Sort).ToList();//SysControl
                foreach (var p in SysControlList)
                {
                    p.Cols = "Tag,CName,State,SNum,ENum,PayWay";
                    p.ChkState();
                }
                //处理关闭的不显示
                //IList<SysControl> SCList = new List<SysControl>();
                //foreach (var p in SysControlList) {
                //    if (p.State == 1) {
                //        SCList.Add(p);
                //    }
                //}
                if (IsOld) { 
                    //老板本，去除新加的标识
                    SysControlList = SysControlList.Where(n => n.Id < 16).ToList();
                }
                SysSet.CtrlSet = SysControlList.EntityToJson();

                //处理图标菜单
                Dictionary<int, List<APPModule>> APPModuleDictionary = new Dictionary<int, List<APPModule>>();
                int[] DisplaySite = new int[] { 1, 2, 3 };//1:home 2:底部 3:更多
                foreach (var index in DisplaySite)
                {
                    List<APPModule> List = new List<APPModule>();
                    //SysSet.APPHasMore 0:未设置 1：有 2:无
                    if (!(SysSet.APPHasMore == 2 && index == 3))
                    {
                        //查询区域的图标
                        IQueryable<APPModule> query = Entity.APPModule.Where(n => n.State == 1 && n.DisplaySite == index && n.Version==0).OrderBy(o => o.Sort);
                        int count = 0;
                        if (!SysAgent.Id.IsNullOrEmpty())
                        {
                            IQueryable<APPModule> countQuery = query.Where(n => n.AgentId == SysAgent.Id);
                            count = countQuery.Count();
                            if (count > 0)
                            {
                                query = countQuery;
                            }
                        }

                        //如果未配置使用默认(好付)的配置
                        if (count == 0)
                        {
                            query = query.Where(n => n.AgentId == 0);
                        }

                        //宫格模式
                        if (index == 1)
                        {
                            int take = SysSet.AppBtnNumber * 3;
                            if (SysSet.APPHasMore == 1)
                            {
                                take = take - 1;
                            }
                            query = query.Take(take);
                        }

                        List = query.ToList();
                        //添加Url域名
                        List.ForEach(o =>
                        {
                            o.PictureUrl = Utils.ImageUrl("APPModule", o.PictureUrl, SysImgPath);
                            if (!o.PicUrl.IsNullOrEmpty())
                            {
                                o.PicUrl = Utils.ImageUrl("APPModule", o.PicUrl, SysImgPath);
                            }
                        });
                    }

                    APPModuleDictionary.Add(index, List);
                }

                //添加更多按钮
                APPModule APP = new APPModule();
                APP.Id = 0;
                APP.Name = "更多";
                APP.PictureUrl = Utils.ImageUrl("APPModule", "gd.png", SysImgPath);
                APP.Value = "GD_home";
                APP.ModuleType = 1;
                APP.AddTime = DateTime.Now;
                APP.State = 1;
                APP.DisplaySite = 1;
                APP.Sort = 999;
                APPModuleDictionary[1].Add(APP);

                SysSet.Home = APPModuleDictionary[1].OrderBy(o=>o.Sort).ToList().EntityToJson();
                SysSet.Bottom = APPModuleDictionary[2].OrderBy(o => o.Sort).ToList().EntityToJson();
                SysSet.More = APPModuleDictionary[3].OrderBy(o => o.Sort).ToList().EntityToJson();

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
                    SysSet.Cols = "Name,IosVer,IosInt,IosUrl,IosInfo,IOSState,IosSet1,IosSet2,IosSet3,IosSet4,IosSet5,IosSet6,IosSet7,IosSet8,IosSet9,IosSet10,IosSet11,CtrlSet,Tel,CtrlMoney1,Home,More,Bottom,AppMenuHome,AppMenuMore,AppMenuBottom,AppWord,SW1Key,SW2Key,AppBtnNumber,IosColor,LagEntry,LagEntryNum,LagEntryDay,Cash0Times,Cash1Times,AutoCashMoney,AuthType,AuthPrice";
                }
                if (Equipment.RqType == "Android")
                {
                    SysSet.Cols = "Name,ApkVer,ApkInt,ApkUrl,ApkInfo,APKState,ApkSet1,ApkSet2,ApkSet3,ApkSet4,ApkSet5,ApkSet6,ApkSet7,ApkSet8,ApkSet9,ApkSet10,ApkSet11,CtrlSet,Tel,CtrlMoney1,Home,More,Bottom,AppMenuHome,AppMenuMore,AppMenuBottom,AppWord,SW1Key,SW2Key,AppBtnNumber,ApkColor,LagEntry,LagEntryNum,LagEntryDay,Cash0Times,Cash1Times,AutoCashMoney,AuthType,AuthPrice";
                }
                
                //T+n相关
                if (SysSet.LagEntry == 0)
                {
                    SysSet.LagEntryDay = 0;
                    SysSet.LagEntryNum = 0;
                }

                //贴牌特别处理
                if (SysAgent.IsTeiPai == 1) {
                    SysSet.LagEntry = 0;
                    SysSet.LagEntryDay = 0;
                    SysSet.LagEntryNum = 0;
                }

                SysSet.Cash0Times = 0;
                SysSet.Cash1Times = 0;

                string data = SysSet.OutJson();
                data = data.Replace("\"[{", "[{").Replace("}]\"", "}]");
                if (HasCache)
                {
                    CacheBuilder.EntityCache.Remove(CashName, null);
                    CacheBuilder.EntityCache.Add(CashName, data, DateTime.Now.AddMinutes(15), null);
                }
                DataObj.Data = data;
                DataObj.Code = "0000";
                DataObj.OutString();
                //Tools.OutString(ErrInfo.Return("0000"));
            }
        }
    }
}
