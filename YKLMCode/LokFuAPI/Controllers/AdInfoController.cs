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
    public class AdInfoController : InitController
    {
        public AdInfoController()
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
            string Data = DataObj.GetData();
            if (Data.IsNullOrEmpty())
            {
                DataObj.OutError("1000");
                return;
            }
            JObject json = new JObject();
            try
            {
                json = (JObject)JsonConvert.DeserializeObject(Data);
            }
            catch (Exception Ex)
            {
                Log.Write("[AdInfo]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            AdInfo AdInfo = new AdInfo();
            AdInfo = JsonToObject.ConvertJsonToModel(AdInfo, json);
            if (AdInfo.Tag.IsNullOrEmpty()){
                DataObj.OutError("1000");
                return;
            }

            Users Users = null;
            int AgentId = AdInfo.AgentId;

            //if (AgentId.IsNullOrEmpty()) {

            //}
            if (!AdInfo.Token.IsNullOrEmpty())
            {
                Users = Entity.Users.FirstOrDefault(n => n.Token == AdInfo.Token);
                //获取信息
                SysAgent topSysAgent = null;
                if (Users != null)
                {
                    var temp = Entity.SysAgent.FirstOrDefault(o => o.Id == Users.Agent);
                    if (temp != null)
                    {
                        topSysAgent = temp.GetTopAgent(this.Entity);
                        if (topSysAgent.IsTeiPai == 1)
                        {
                            AgentId = topSysAgent.Id;
                        }
                    }
                }
            }
            

            if (Users != null && AdInfo.Tag == "newbanner")
            {
                if (Users.UserName == "13456789456" || Users.UserName == "13612345678")
                {
                    var preinstall = new List<AdInfo>();
                    var temp = new AdInfo()
                    {
                        Id = 1,
                        Name = "安全保障",
                        Pic = Utils.ImageUrl("AdInfo", "preinstall.png", SysImgPath),
                        ModuleType = 2,
                        Url = "",
                    };
                    preinstall.Add(temp);
                    DataObj.Data = preinstall.EntityToJson();
                    DataObj.Code = "0000";
                    DataObj.OutString();
                    return;
                }
            }

            #region 处理金牌标识(功能已取消，代码暂留)
            if (AdInfo.Tag == "jinpai")
            {
                if (!AdInfo.Token.IsNullOrEmpty())
                {
                    if (Users == null)
                    {
                        //如果上面过程没有读取用户，则在此重新读取用户
                        Users = Entity.Users.FirstOrDefault(n => n.Token == AdInfo.Token);
                    }
                    //获取用户信息
                    if (Users != null)
                    {
                        UsersFace UsersFace = Entity.UsersFace.FirstOrDefault(n => n.UId == Users.Id && n.CType == 1);
                        if (UsersFace == null)
                        {
                            UsersFace = new UsersFace();

                            UsersFace.UId = Users.Id;
                            UsersFace.CType = 1;
                            UsersFace.TrueName = Users.TrueName;
                            UsersFace.Mobile = Users.Mobile;
                            UsersFace.CardStae = Users.CardStae;
                            UsersFace.RegAddress = Users.RegAddress;
                            UsersFace.Agent = Users.Agent;
                            UsersFace.AId = Users.AId;
                            if (!Users.SAId.IsNullOrEmpty())
                            {
                                UsersFace.IsDaiLi = 1;
                            }
                            else
                            {
                                UsersFace.IsDaiLi = 0;
                            }
                            UsersFace.MobileType = 0;
                            if (Equipment.RqType == "Android")
                            {
                                UsersFace.MobileType = 1;
                            }
                            if (Equipment.RqType == "Apple")
                            {
                                UsersFace.MobileType = 2;
                            }
                            UsersFace.Times = 1;
                            UsersFace.When = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            UsersFace.State = 1;
                            UsersFace.UpdateTime = DateTime.Now;
                            UsersFace.IsNew = 1;
                            UsersFace.AddTime = DateTime.Now;
                            Entity.UsersFace.AddObject(UsersFace);
                        }
                        else
                        {
                            UsersFace.Times++;
                            UsersFace.UpdateTime = DateTime.Now;
                            UsersFace.IsNew = 1;
                            UsersFace.When += "|" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        Entity.SaveChanges();
                    }
                }
            }
            #endregion

            if (AgentId.IsNullOrEmpty()) {
                AgentId = 0;
            }
            string CashName = "AdInfo" + AdInfo.Tag + AgentId;
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

            IList<AdInfo> List = new List<AdInfo>();
            if (!AgentId.IsNullOrEmpty())
            {
                List = Entity.AdInfo.Where(n => n.Tag == AdInfo.Tag && n.State == 1 && n.StartTime < DateTime.Now && n.EndTime > DateTime.Now && n.AgentId == AgentId).OrderBy(n => n.Sort).ToList();
            }
            //如果没有配置则取默认(好付)的配置
            if (List.Count == 0)
            {
                List = Entity.AdInfo.Where(n => n.Tag == AdInfo.Tag && n.State == 1 && n.StartTime < DateTime.Now && n.EndTime > DateTime.Now && n.AgentId == 0).OrderBy(n => n.Sort).ToList();
            }

            foreach (var p in List) {
                p.Pic = Utils.ImageUrl("AdInfo", p.Pic, SysImgPath);
            }

            string data = List.EntityToJson();
            if (HasCache)
            {
                CacheBuilder.EntityCache.Remove(CashName, null);
                CacheBuilder.EntityCache.Add(CashName, data, DateTime.Now.AddMinutes(30), null);
            }
            DataObj.Data = data;
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
