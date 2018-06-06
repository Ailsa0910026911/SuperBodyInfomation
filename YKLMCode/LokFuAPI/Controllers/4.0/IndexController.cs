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
    /// <summary>
    /// 首页接口
    /// </summary>
    public class IndexController : InitController
    {
        public IndexController()
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
            if (!Data.IsNullOrEmpty())
            {
                JObject json = new JObject();
                try
                {
                    json = (JObject)JsonConvert.DeserializeObject(Data);
                }
                catch (Exception Ex)
                {
                    Log.Write("[Index]:", "【Data】" + Data, Ex);
                }
                if (json == null)
                {
                    DataObj.OutError("1000");
                    return;
                }
                
                var Users = new Users();
                Users = JsonToObject.ConvertJsonToModel(Users, json);
                Users BaseUsers = Entity.Users.FirstOrDefault(o => o.Token == Users.Token);
                if (BaseUsers == null)//用户令牌不存在
                {
                    DataObj.OutError("2004");
                    return;
                }

                SysAgent SysAgent = new SysAgent();
                if (!Users.Id.IsNullOrEmpty())
                {
                    SysAgent = Entity.SysAgent.FirstOrDefault(n => n.Id == Users.Id && n.State == 1 && n.IsTeiPai == 1);
                    if (SysAgent == null)
                    {
                        DataObj.OutError("1000");
                        return;
                    }
                }      

                var SysSet = Entity.SysSet.FirstOrDefault();
                var IndexModel = new IndexModel();

                if (SysAgent.Id == 0)
                {
                    SysAgent.Id = 999999999;
                }

                var Notice = this.Entity.MsgNotice.Where(o => (o.AgentId == 0 || o.AgentId == SysAgent.Id) && (o.NType == 0 || o.NType == 3) && o.State == 1).OrderByDescending(o => o.Id)
                    .FirstOrDefault();
                IndexModel.nid = 0;
                IndexModel.ninfo = string.Empty;
                
                if (Notice != null)
                {
                    if (!Notice.Info.IsNullOrEmpty())
                    {
                        Notice.Info = Notice.Info.Replace("\t", "").Replace("\n", "").Replace("\r", "");
                    }
                    IndexModel.ninfo += Notice.Info.IsNullOrEmpty() ? Notice.Info : Utils.RemoveHtml(Notice.Info) + "          ";
                }

                #region 功能按钮@内容块
                var Button = Entity.APPModule.Where(n => n.State == 1 && n.DisplaySite == 1 && n.Version == 1 && n.AgentId == SysAgent.Id).OrderBy(o => o.Sort)
                   .Select(o => new ButtonModel
                   {
                       id = o.Id,
                       picurl = o.PicUrl ?? string.Empty,
                       pictureurl = o.PictureUrl ?? string.Empty,
                       value = o.Value ?? string.Empty,
                       moduletype = o.ModuleType,
                       name = o.Name ?? string.Empty,
                       height = o.Height,
                       width = o.Width,
                       sort = 0,
                   }).ToList();
               
                if (Button == null || Button.Count == 0)//贴牌没有配置功能按钮，将默认使用好付
                {
                    Button = Entity.APPModule.Where(n => n.State == 1 && n.DisplaySite == 1 && n.Version == 1 && n.AgentId == 0).OrderBy(o => o.Sort)
                   .Select(o => new ButtonModel
                   {
                       id = o.Id,
                       picurl = o.PicUrl ?? string.Empty,
                       pictureurl = o.PictureUrl ?? string.Empty,
                       value = o.Value ?? string.Empty,
                       moduletype = o.ModuleType,
                       name = o.Name ?? string.Empty,
                       height = o.Height,
                       width = o.Width,
                       sort = 0,
                   }).ToList();
                }
                foreach (var info in Button)
                {
                    if (info.id == 17)
                    {
                        if (BaseUsers.IsVip == 1)
                        {
                            info.moduletype = 2;
                        }
                        else
                        {
                            info.moduletype = 1;
                            info.value = "VIPZQ_home";
                        }
                    }
                }

                int seed = 1;
                ButtonModel temp = null;
                Button.ForEach(o =>
                {
                    if (seed == 12 && Button.Count > 12)
                    {
                        temp = new ButtonModel()
                        {
                            sort = seed,
                            moduletype = 1,
                            value = "GD_home",
                            name = "更多",
                            pictureurl = Utils.ImageUrl("APPModule", "home2/functionicon09_4.png", SysImgPath),
                            height = 100,
                            width = 100,
                            picurl = string.Empty,
                        };
                        seed++;
                    }
                    o.pictureurl = Utils.ImageUrl("APPModule", o.pictureurl, SysImgPath);
                    if (!o.picurl.IsNullOrEmpty())
                    {
                        o.picurl = Utils.ImageUrl("APPModule", o.picurl, SysImgPath);
                    }
                    o.sort = seed;
                    seed++;
                });
                if (temp!=null)
                {
                    Button.Add(temp);
                }
                Button = Button.OrderBy(o => o.sort).ToList();

                var Block = Entity.APPBlock.Where(o => o.AgentId == SysAgent.Id && o.State == 1).OrderBy(o => o.Sort).ToList();
                if (Block == null || Block.Count == 0)//贴牌没有配置功能按钮，将默认使用好付
                {
                    Block = Entity.APPBlock.Where(o => o.AgentId == 0 && o.State == 1).OrderBy(o => o.Sort).ToList();
                }
                var BlockModel = Block.Select(o => new BlockModel
                    {
                        Name = o.Name,
                        IconUrl = o.IconUrl ?? string.Empty,
                        //LinkName1 = o.LinkName1 ?? string.Empty,
                        //LinkName2 = o.LinkName2 ?? string.Empty,
                        LinkType = o.LinkType ?? 0,
                        //LinkType1 = o.LinkType1 ?? 0,
                        //LinkType2 = o.LinkType2 ?? 0,
                        LinkUrl = o.LinkUrl ?? string.Empty,
                        //LinkUrl1 = o.LinkUrl1 ?? string.Empty,
                        //LinkUrl2 = o.LinkUrl2 ?? string.Empty,
                        PicUrl = o.PicUrl ?? string.Empty,
                        Sort = o.Sort,
                        SubName = o.SubName ?? string.Empty,
                        width = o.Width,
                        Height = o.Height,
                        Links = new List<BlockLink>() { 
                            new BlockLink() { moduletype = o.LinkType1 ?? 0, name = o.LinkName1 ?? string.Empty, value = o.LinkUrl1 ?? string.Empty, sort = 1},
                            new BlockLink() { moduletype = o.LinkType2 ?? 0, name = o.LinkName2 ?? string.Empty, value = o.LinkUrl2 ?? string.Empty, sort = 2},
                        },
                    }).ToList();
                BlockModel.ForEach(o =>
                {
                    o.IconUrl = Utils.ImageUrl("APPBlock", o.IconUrl, SysImgPath);
                    o.PicUrl = Utils.ImageUrl("APPBlock", o.PicUrl, SysImgPath);
                    o.Links.RemoveAll(x => x.moduletype.IsNullOrEmpty() || x.name.IsNullOrEmpty() || x.value.IsNullOrEmpty());
                    o.Links = o.Links.OrderBy(x => x.sort).ToList();
                });

                IndexModel.block = BlockModel;
                IndexModel.button = Button;
                IndexModel.appmenuhome = SysSet.AppMenuHome;
                #endregion

                #region 为应对苹果审核特殊处理
                if (BaseUsers != null)
                {
                    if (BaseUsers.UserName == "13456789456" || BaseUsers.UserName == "13612345678")
                    {
                        Button = new List<ButtonModel>();
                        var temp1 = new ButtonModel()
                        {
                            name = "钱包",
                            pictureurl = Utils.ImageUrl("APPModule", "201761218549218.png", SysImgPath),
                            moduletype = 1,
                            sort = 1,
                            value = "QB_home",
                            picurl = string.Empty,
                        };
                        Button.Add(temp1);
                        var temp2 = new ButtonModel()
                        {
                            name = "查快递",
                            pictureurl = Utils.ImageUrl("APPModule", "20176710135353.png", SysImgPath),
                            moduletype = 2,
                            sort = 2,
                            value = "http://m.kuaidi100.com/uc/index.html#input",
                            picurl = string.Empty,
                        };
                        Button.Add(temp2);
                        var temp3 = new ButtonModel()
                        {
                            name = "专属客服",
                            pictureurl = Utils.ImageUrl("APPModule", "201767101122288.png", SysImgPath),
                            moduletype = 1,
                            sort = 3,
                            value = "ZSKF_home",
                            picurl = string.Empty,
                        };
                        Button.Add(temp3);
                        IndexModel.button = Button;

                        var BlockList = new List<BlockModel>();
                        var Block1 = new BlockModel()
                        {
                            Name = "我要收款",
                            SubName = "支持微信/支付宝/银行卡",
                            IconUrl = Utils.ImageUrl("APPBlock", "20176711449864.png", SysImgPath),
                            PicUrl = Utils.ImageUrl("APPBlock", "20176711486389.png", SysImgPath),
                            Sort = 1,
                            LinkType = 1,
                            LinkUrl = "SK_home",
                            Height = 400,
                            width = 1020,
                            Links = new List<BlockLink>() { 
                            new BlockLink() { moduletype = 1, name = string.Empty, value = "SK_home", sort = 1},
                            new BlockLink() { moduletype = 1, name = string.Empty, value = "SK_home", sort = 2},
                            },
                        };
                        BlockList.Add(Block1);
                        IndexModel.block = BlockList;
                    }
                }
                #endregion

                var TopButton = new List<ButtonModel>();
                ButtonModel Toptemp = null;
                Toptemp = new ButtonModel()
                {
                    sort = seed,
                    moduletype = 1,
                    value = "SK_home",
                    name = "信用卡收款",
                    pictureurl = Utils.ImageUrl("APPModule", "/UpLoadFiles/APPModule/home2/topshoukuan.png", SysImgPath),
                    height = 100,
                    width = 100,
                    picurl = string.Empty,
                };
                TopButton.Add(Toptemp);
                Toptemp = new ButtonModel()
                {
                    sort = seed,
                    moduletype = 1,
                    value = "XYKGJ_home",
                    name = "信用卡代还",
                    pictureurl = Utils.ImageUrl("APPModule", "/UpLoadFiles/APPModule/home2/topdaihuan.png", SysImgPath),
                    height = 100,
                    width = 100,
                    picurl = string.Empty,
                };
                TopButton.Add(Toptemp);
                IndexModel.topbutton = TopButton;
                DataObj.Data = JsonConvert.SerializeObject(IndexModel);
                DataObj.Code = "0000";
                DataObj.OutString();
                //Tools.OutString(ErrInfo.Return("0000"));
            }
        }

        public class IndexModel
        {
            
            public int appmenuhome { get; set; }

            public int nid { get; set; }

            public string ninfo { get; set; }

            public List<ButtonModel> button { get; set; }

            public List<ButtonModel> topbutton { get; set; }

            public List<BlockModel> block { get; set; }
        }

        public class BlockModel
        { 
            public string Name {get;set;}
            public string IconUrl {get;set;}
            //public string LinkName1 {get;set;}
            //public string LinkName2 {get;set;}
            public byte? LinkType {get;set;}
            //public byte? LinkType1 {get;set;}
            //public byte? LinkType2 {get;set;}
            public string LinkUrl {get;set;}
            //public string LinkUrl1 {get;set;}
            //public string LinkUrl2 {get;set;}
            public string PicUrl {get;set;}
            public int Sort {get;set;}
            public string SubName {get;set;}
            public int width {get;set;}
            public int Height {get;set;}

            public List<BlockLink> Links { get; set; }
        }

        public class BlockLink
        {
            public string name { get; set; }

            public string value { get; set; }

            public byte moduletype { get; set; }

            public int sort { get; set; }
        }

    }

   
}
