using LokFu.Extensions;
using LokFu.Infrastructure;
using LokFu.Models;
using LokFu.Repositories;
using LokFu.Repositories.SqlServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;


namespace LokFu.Controllers
{
    public class OrdersController : InitController
    {
        public OrdersController()
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
                Log.Write("[Orders]:", "【Data】" + Data, Ex);
            }
            if (json == null)
            {
                DataObj.OutError("1000");
                return;
            }
            Orders Orders = new Orders();
            Orders = JsonToObject.ConvertJsonToModel(Orders, json);

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
                //DataObj.OutError("2006");
                //return;
            }
            if (baseUsers.MiBao != 1)//未设置支付密码
            {
                //DataObj.OutError("2008");
                //return;
            }
            
            EFPagingInfo<Orders> p = new EFPagingInfo<Orders>();
            if (!Orders.Pg.IsNullOrEmpty()) { p.PageIndex = Orders.Pg; }
            if (!Orders.Pgs.IsNullOrEmpty()) { p.PageSize = Orders.Pgs; }
            if (!Orders.STime.IsNullOrEmpty())
            {
                p.SqlWhere.Add(f => f.AddTime >= Orders.STime);
            }
            if (!Orders.ETime.IsNullOrEmpty())
            {
                Orders.ETime = Orders.ETime.AddHours(24);
                p.SqlWhere.Add(f => f.AddTime <= Orders.ETime);
            }
            if (!Orders.IdCardState.IsNullOrEmpty())
            {//凭证状态
                if (Orders.IdCardState == 99)
                {
                    p.SqlWhere.Add(f => f.IdCardState == 0);
                }
                else if (Orders.IdCardState == 98)
                {
                    p.SqlWhere.Add(f => f.IdCardState == 1 || f.IdCardState == 2 || f.IdCardState == 4);
                }
                else
                {
                    p.SqlWhere.Add(f => f.IdCardState == Orders.IdCardState);
                }
            }

            bool IsOld = true;
            #region 版本比较 升级之后比较长时间后可以考滤删除版本判断代码
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
                #region 4.0条件判断
                if (!(Orders.TType == 3 && Orders.TState == 3))
                {
                    p.SqlWhere.Add(o => o.UId == baseUsers.Id);
                }
                if (!Orders.TType.IsNullOrEmpty())
                {
                    p.SqlWhere.Add(f => f.TType == Orders.TType);//读取对应的类型
                    if (Orders.TType == 1)
                    {
                        if (!Orders.TState.IsNullOrEmpty())
                        {
                            switch (Orders.TState)
                            {
                                case 1://未付
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                    break;
                                case 2://已付
                                    p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                                    break;
                                case 3://待传证照
                                    p.SqlWhere.Add(f => f.PayState == 1 && f.IdCardState == 1);
                                    break;
                                case 4://待审核/已传证照
                                    p.SqlWhere.Add(f => f.PayState == 1 && f.IdCardState == 2);
                                    break;
                                case 5://审核失败
                                    p.SqlWhere.Add(f => f.PayState == 1 && f.IdCardState == 4);
                                    break;
                                case 6://退单
                                    p.SqlWhere.Add(f => f.TState == 4);
                                    break;
                                case 7://待入帐
                                    p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 0);
                                    break;
                            }
                        }
                    }
                    if (Orders.TType == 2)
                    {
                        switch (Orders.TState)
                        {
                            case 1://处理中
                                p.SqlWhere.Add(f => f.TState == 1);
                                break;
                            case 2://已汇出
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 2);
                                break;
                            case 3://提现失败
                                p.SqlWhere.Add(f => f.TState == 3);
                                break;
                            case 4://出款中
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                                break;
                            case 5://退款
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 4);
                                break;
                            case 6://退款中
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 3);
                                break;
                        }
                    }
                    if (Orders.TType == 3)
                    {
                        switch (Orders.TState)
                        {
                            case 1://未付
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                                break;
                            case 2://已付
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                                break;
                            case 3://已收
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1 && f.RUId == baseUsers.Id);
                                break;
                        }
                    }
                    if (Orders.TType == 6)
                    {
                        if (!Orders.TState.IsNullOrEmpty()) { p.SqlWhere.Add(f => f.TState == Orders.TState); }
                    }
                    if (Orders.TType == 7 || Orders.TType == 8)
                    {
                        if (!Orders.TState.IsNullOrEmpty())
                        {
                            if (Orders.TState == 99)
                            {
                                p.SqlWhere.Add(f => f.TState == 0);
                            }
                            if (Orders.TState == 1)
                            {
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 0);
                            }
                            if (Orders.TState == 2)
                            {
                                p.SqlWhere.Add(f => f.TState == 2 && f.PayState == 1);
                            }
                            if (Orders.TState == 3)//退单
                            {
                                p.SqlWhere.Add(f => f.TState == 4);
                            }
                            if (Orders.TState == 4)//待入帐
                            {       
                                p.SqlWhere.Add(f => f.TState == 1 && f.PayState == 1 && f.InState == 0);
                            }
                            if (Orders.TState == 5)//待审核/已传证照
                            {
                                p.SqlWhere.Add(f => f.PayState == 1 && f.InState == 1 && f.IdCardState == 2);
                            }
                            if (Orders.TState == 6)//待传证照
                            {
                                p.SqlWhere.Add(f => f.PayState == 1 && f.InState == 1 && f.IdCardState == 1);
                            }
                            if (Orders.TState == 7)//审核失败
                            {
                                p.SqlWhere.Add(f => f.PayState == 1 && f.InState == 1 && f.IdCardState == 4);
                            }
                        }
                    }
                }
                else
                {
                    if (!Orders.TState.IsNullOrEmpty()) {
                        if (Orders.TState == 99)
                        {
                            p.SqlWhere.Add(f => f.TState == 0);
                        }
                        if (Orders.TState == 1)
                        {
                            p.SqlWhere.Add(f => f.PayState == 1); 
                        }
                        if (Orders.TState == 2)
                        {
                            p.SqlWhere.Add(f => f.PayState == 0 && f.TState != 0);
                        }
                    }
                }
                #endregion
            }
            else
            { 
                #region 4.0之前条件判断
                //搜索
                if (Orders.TType.IsNullOrEmpty())
                {//不限类型，读取收支交易，收款订单只显示已支付订单
                    p.SqlWhere.Add(f => f.UId == baseUsers.Id || (f.RUId == baseUsers.Id && f.PayState == 1) && f.TType != 2);
                }
                else
                {
                    if (Orders.TType == 4)
                    {
                        //已支付收款订单
                        p.SqlWhere.Add(f => f.TType == 3);
                        p.SqlWhere.Add(f => f.RUId == baseUsers.Id && f.PayState == 1);
                    }
                    else if (Orders.TType == 95)
                    { //转帐单
                        p.SqlWhere.Add(f => f.TType == 3);
                        p.SqlWhere.Add(f => (f.RUId == baseUsers.Id && f.PayState == 1) || f.UId == baseUsers.Id);
                    }
                    else if (Orders.TType == 96) { //收款订单
                        p.SqlWhere.Add(f => (f.TType == 3 && f.RUId == baseUsers.Id && f.PayState == 1) || (f.TType == 1 && f.UId == baseUsers.Id));
                    }
                    else
                    {
                        p.SqlWhere.Add(f => f.TType == Orders.TType);
                        p.SqlWhere.Add(f => f.UId == baseUsers.Id);
                    }
                
                }
                if (!Orders.TState.IsNullOrEmpty()) {//交易状态
                    p.SqlWhere.Add(f => f.TState == Orders.TState);
                }
                if (!Orders.PayState.IsNullOrEmpty()){//支付状态
                    if (Orders.PayState == 99) {
                        Orders.PayState = 0;
                    }
                    p.SqlWhere.Add(f => f.PayState == Orders.PayState);
                }
                #endregion
            }

            p.OrderByList.Add("Id", "DESC");
            IPageOfItems<Orders> List = Entity.Selects<Orders>(p);
            //处理转帐订单
            foreach (var pp in List) {
                pp.Cols = pp.Cols + ",Json,PicList,UserCardName,CardUpdateTime";
                if (baseUsers.Id == pp.RUId)
                {
                    pp.TType = 4;
                }
                JObject JS = new JObject();
                if (pp.TType == 1)
                { //银联卡支付
                    OrderRecharge OrderRecharge = Entity.OrderRecharge.FirstOrNew(n => n.OId == pp.TNum);
                    OrderRecharge.Cols = "Amoney,PayMoney,Poundage,PayType";
                    string JsStr = OrderRecharge.OutJson();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                    }
                    pp.Json = JS;
                }
                if (pp.TType == 2)//提现不能取消
                {
                    OrderCash OrderCash = Entity.OrderCash.FirstOrNew(n => n.OId == pp.TNum);
                    OrderCash.Cols = "Owner,Bank,CardNum,Deposit,Mobile,Province,City,District,Amoney,UserRate,TrunType,PayMoney";
                    OrderCash.PayMoney = OrderCash.Amoney - (decimal)OrderCash.UserRate;
                    string JsStr = OrderCash.OutJson();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                    }
                    pp.Json = JS;
                }
                if (pp.TType == 3 || pp.TType == 4)//付款
                {
                    OrderTransfer OrderTransfer = Entity.OrderTransfer.FirstOrNew(n => n.OId == pp.TNum);
                    Users Users = new Users();
                    if (pp.TType == 4) {
                        Users = Entity.Users.FirstOrNew(n => n.Id == OrderTransfer.UId);
                    }
                    if (pp.TType == 3)
                    {
                        Users = Entity.Users.FirstOrNew(n => n.Id == OrderTransfer.RUId);
                    }
                    OrderTransfer.Mobile = Users.UserName;
                    OrderTransfer.ToUserName = Users.TrueName;
                    OrderTransfer.Cols = "UId,RUId,PayMoney,Poundage,Amoney,Mobile,ToUserName";
                    string JsStr = OrderTransfer.OutJson();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                    }
                    pp.Json = JS;
                }
                if (pp.TType == 5)//防租
                {
                    OrderHouse OrderHouse = Entity.OrderHouse.FirstOrNew(n => n.OId == pp.TNum);
                    OrderHouse.Cols = "HouseOwner,Bank,CardNum,Deposit,Mobile,MonthRent,SecurityMoney,PayMonth,Poundage,Amoney";
                    string JsStr = OrderHouse.OutJson();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                    }
                    pp.Json = JS;
                }
                if (pp.TType == 6)
                { //升级
                    //PayConfigOrder PayConfigOrder = Entity.PayConfigOrder.FirstOrNew(n => n.OId == pp.TNum);
                    //Out = Out + "," + PayConfigOrder.ToStr();
                    pp.Poundage = 0;
                }
                if (pp.TType == 7 || pp.TType == 8 || pp.TType == 9)
                { //扫码
                    OrderF2F OrderF2F = Entity.OrderF2F.FirstOrNew(n => n.OId == pp.TNum);
                    OrderF2F.Cols = "Amoney,PayMoney,Poundage";
                    string JsStr = OrderF2F.OutJson();
                    try
                    {
                        JS = (JObject)JsonConvert.DeserializeObject(JsStr);
                    }
                    catch (Exception Ex)
                    {
                        Log.Write("[Order]:", "【JsStr】" + JsStr, Ex);
                    }
                    pp.Json = JS;
                }
                if (pp.TType == 10)
                {//代理

                }
                pp.DoRemark(Entity);
                pp.StateTxt = pp.GetState();
                if (pp.RUId == baseUsers.Id && pp.PayState == 1)
                {
                    pp.StateTxt = "已收";
                }
                if (pp.IdCardState > 1)
                {
                    pp.Remark = pp.DDAuditRemark;
                }
                if(!pp.Remark.IsNullOrEmpty())
                {
                    pp.Remark = pp.Remark.Replace("\t", "");
                }
                if (!pp.UserCardPic.IsNullOrEmpty())
                {
                    var UserCardPicList = pp.UserCardPic.Split(',').ToList();
                    for (int i = 0; i < UserCardPicList.Count; i++)
                    {
                        var a = Utils.ImageUrl("Orders", UserCardPicList[i], AppImgPath);
                        UserCardPicList[i] = a;
                    }
                    JavaScriptSerializer JSS = new JavaScriptSerializer();
                    string data = JSS.Serialize(UserCardPicList);
                    JArray JO = (JArray)JsonConvert.DeserializeObject(data);
                    pp.PicList = JO;
                }
            }

            IList<OrdersModel> OML = Utils.GetOrdersModel();
            OML = OML.Where(n => n.Id != 10).ToList();
            foreach (var P in OML) {
                if (P.Id == 3) {
                    P.Id = 95;
                }
            }

            IList<Orders> iList = List.ToList();
            StringBuilder sb = new StringBuilder("");
            sb.Append("{");
            sb.Append(List.PageToString());
            sb.Append(",");
            sb.Append(iList.EntityToString());
            sb.Append(",");
            sb.Append(OML.EntityToString());
            
            sb.Append(",\"state\":{");
            sb.Append("\"0\":[{\"key\":\"交易关闭\",\"value\":\"99\"},{\"key\":\"已付\",\"value\":\"1\"},{\"key\":\"未付\",\"value\":\"2\"}],");
            sb.Append("\"1\":[{\"key\":\"未付\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"2\"},{\"key\":\"待传证照\",\"value\":\"3\"},{\"key\":\"已传证照\",\"value\":\"4\"},{\"key\":\"审核失败\",\"value\":\"5\"},{\"key\":\"退单\",\"value\":\"6\"},{\"key\":\"待入账\",\"value\":\"7\"}],");
            sb.Append("\"2\":[{\"key\":\"处理中\",\"value\":\"1\"},{\"key\":\"已汇出\",\"value\":\"2\"},{\"key\":\"提现失败\",\"value\":\"3\"},{\"key\":\"出款中\",\"value\":\"4\"},{\"key\":\"已退款\",\"value\":\"5\"},{\"key\":\"退款中\",\"value\":\"6\"}],");
            sb.Append("\"3\":[{\"key\":\"未付\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"2\"},{\"key\":\"已收\",\"value\":\"3\"}],");
            sb.Append("\"6\":[{\"key\":\"交易关闭\",\"value\":\"99\"},{\"key\":\"未付\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"2\"}],");
            sb.Append("\"7\":[{\"key\":\"交易关闭\",\"value\":\"99\"},{\"key\":\"进行中\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"2\"},{\"key\":\"退单\",\"value\":\"3\"},{\"key\":\"待入账\",\"value\":\"4\"},{\"key\":\"已传证照\",\"value\":\"5\"},{\"key\":\"待传证照\",\"value\":\"6\"},{\"key\":\"审核失败\",\"value\":\"7\"}],");
            sb.Append("\"8\":[{\"key\":\"交易关闭\",\"value\":\"99\"},{\"key\":\"进行中\",\"value\":\"1\"},{\"key\":\"已付\",\"value\":\"2\"},{\"key\":\"退单\",\"value\":\"3\"},{\"key\":\"待入账\",\"value\":\"4\"},{\"key\":\"已传证照\",\"value\":\"5\"},{\"key\":\"待传证照\",\"value\":\"6\"},{\"key\":\"审核失败\",\"value\":\"7\"}]");
            sb.Append("},");

            sb.Append("\"type\":[{\"key\":\"0\",\"value\":\"全部\"},{\"key\":\"1\",\"value\":\"银联卡支付\"},{\"key\":\"2\",\"value\":\"提现\"},{\"key\":\"3\",\"value\":\"转账\"},{\"key\":\"6\",\"value\":\"升级\"},{\"key\":\"7\",\"value\":\"支付宝\"},{\"key\":\"8\",\"value\":\"微信\"}]");

            sb.Append("}");
            DataObj.Data = sb.ToString();
            DataObj.Code = "0000";
            DataObj.OutString();
        }
    }
}
