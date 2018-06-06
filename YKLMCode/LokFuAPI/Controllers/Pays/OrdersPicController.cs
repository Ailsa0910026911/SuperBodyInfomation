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
using System.Web.Script.Serialization;

namespace LokFu.Controllers
{
    public class OrdersPicController : InitController
    {
        public OrdersPicController()
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
                Log.Write("[OrdersPicController]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }

            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, json);

            if (Orders.BankCardId.IsNullOrEmpty() || Orders.UserCardId.IsNullOrEmpty() || Orders.UserCardPic.IsNullOrEmpty()) {
                DataObj.OutError("1000");
                return;
            }

            Users baseUsers = Entity.Users.FirstOrDefault(n => n.Token == Orders.Token);
            if (baseUsers == null)//用户令牌不存在
            {
                DataObj.OutError("2004");
                return;
            }
            if (baseUsers.State != 1)//用户被锁定
            {
                DataObj.OutError("2003");
                return;
            }
            if (baseUsers.CardStae != 2)//未实名认证
            {
                DataObj.OutError("2006");
                return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                DataObj.OutError("2008");
                return;
            }

            Orders baseOrders = Entity.Orders.FirstOrDefault(n => n.TNum == Orders.TNum && n.UId == baseUsers.Id);
            if (baseOrders == null)//不存在
            {
                DataObj.OutError("1001");
                return;
            }
            if (baseOrders.IdCardState == 0)
            {
                DataObj.OutError("6041");
                return;
            }
            if (baseOrders.IdCardState == 2)
            {
                DataObj.OutError("6042");
                return;
            }
            if (baseOrders.IdCardState == 3)
            {
                DataObj.OutError("6043");
                return;
            }

            bool IsOld = true;
            #region 版本比较
            //处理贴牌相关
            var SysAgent = this.Entity.SysAgent.FirstOrDefault(o => o.Id == baseUsers.Agent);
            if (SysAgent == null)
            {
                DataObj.OutError("1000");
                return;
            }
            var topSysAgent = SysAgent.GetTopAgent(this.Entity);

            if (!Equipment.SoftVer.IsNullOrEmpty())
            {
                Version v1 = new Version(Equipment.SoftVer);//当前版本
                Version v2 = new Version("1.0");

                if (Equipment.RqType.ToLower() == "apple")
                {
                    //苹果
                    if (topSysAgent.IsTeiPai == 0)//好付
                    {
                        v2 = new Version("8.0");
                    }
                    else//贴牌
                    {
                        v2 = new Version("6.0");
                    }

                }
                else if (Equipment.RqType.ToLower() == "android")
                {
                    //安卓
                    if (topSysAgent.IsTeiPai == 0)//好付
                    {
                        v2 = new Version("8.0.0");
                    }
                    else //贴牌
                    {
                        v2 = new Version("6.0");
                    }
                }
                if (v1 >= v2)
                {
                    IsOld = false;
                }
            }
            #endregion

            if (!IsOld)//新版
            {
                baseOrders.UserCardPic = Orders.UserCardPic;
                baseOrders.UserCardName = Orders.UserCardName;
            }
            else
            {
                baseOrders.UserCardPic = Utils.Base64StringToImage(Orders.UserCardPic, "Orders");
            }
            
            baseOrders.BankCardId = Orders.BankCardId;
            baseOrders.UserCardId = Orders.UserCardId;
            baseOrders.IdCardState = 2;
            baseOrders.CardUpType = 0;
            baseOrders.CardUpdateTime = DateTime.Now;
            if (baseOrders.TState == 3)
            {
                baseOrders.TState = 1;
            }

            //调单记录日志
            OrdersDDLog OrdersDDLog = new OrdersDDLog()
            {
                AddTime = DateTime.Now,
                LastTime = null,
                LogType = 2,
                OpName = baseUsers.TrueName,
                Remark = string.Empty,
                TNum = baseOrders.TNum,
                InteriorRemark = string.Empty,
                Img = baseOrders.UserCardPic ?? string.Empty,
            };
            Entity.OrdersDDLog.AddObject(OrdersDDLog);

            Entity.SaveChanges();

            baseOrders.SendMsg(Entity);

            baseOrders.Cols += ",Json,PicList,UserCardName,CardUpdateTime";
            if (!baseOrders.UserCardPic.IsNullOrEmpty())
            {
                var UserCardPicList = baseOrders.UserCardPic.Split(',').ToList();
                for (int i = 0; i < UserCardPicList.Count; i++)
                {
                    var a = Utils.ImageUrl("Orders", UserCardPicList[i], AppImgPath);
                    UserCardPicList[i] = a;
                }
                JavaScriptSerializer JSS = new JavaScriptSerializer();
                string data = JSS.Serialize(UserCardPicList);
                JArray JO = (JArray)JsonConvert.DeserializeObject(data);
                baseOrders.PicList = JO;
            }

            DataObj.Data = baseOrders.OutJson();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
